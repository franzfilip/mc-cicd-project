// app.go

package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"strconv"
	"strings"

	"github.com/blazing-panda/mom-mom-schoerghuber/product-service/errors"
	"github.com/blazing-panda/mom-mom-schoerghuber/product-service/interfaces"
	"github.com/blazing-panda/mom-mom-schoerghuber/product-service/models"
	"github.com/golang-jwt/jwt/v5"
	"github.com/rs/cors"

	_ "github.com/golang-jwt/jwt/v5"
	"github.com/gorilla/mux"
	_ "github.com/lib/pq"
)

type App struct {
	Router    *mux.Router
	JwtSecret string
	Store     interfaces.ProductStore
}

func (a *App) Initialize(store interfaces.ProductStore, jwtSecret string, router *mux.Router) {
	a.Router = router
	a.JwtSecret = jwtSecret
	a.Store = store
	a.initializeRoutes()
}

func (a *App) Run(port int) {
	c := cors.New(cors.Options{
		AllowedOrigins: []string{"*"},
		AllowedMethods: []string{"POST", "PUT", "GET", "DELETE"},
		AllowedHeaders: []string{"Content-Type", "Access-Control-Allow-Origin", "Access-Control-Allow-Headers", "Authorization"},
		Debug:          true,
	})

	log.Printf("Server listening on :%d\n", port)
	log.Fatal(http.ListenAndServe(fmt.Sprintf(":%d", port), c.Handler(a.Router)))
}

func (a *App) getProduct(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	id, err := strconv.Atoi(vars["id"])
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid product ID")
		return
	}

	p, err := a.Store.GetProduct(id)
	if err != nil {
		if err == errors.ErrProductNotFound {
			respondWithError(w, http.StatusNotFound, "Product not found")
		} else {
			respondWithError(w, http.StatusInternalServerError, err.Error())
		}
		return
	}

	respondWithJSON(w, http.StatusOK, p)
}

func respondWithError(w http.ResponseWriter, code int, message string) {
	respondWithJSON(w, code, map[string]string{"error": message})
}

func respondWithJSON(w http.ResponseWriter, code int, payload interface{}) {
	response, _ := json.Marshal(payload)

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(code)
	w.Write(response)
}

func (a *App) getProducts(w http.ResponseWriter, r *http.Request) {
	count, _ := strconv.Atoi(r.FormValue("count"))
	start, _ := strconv.Atoi(r.FormValue("start"))

	if count > 10 || count < 1 {
		count = 10
	}
	if start < 0 {
		start = 0
	}

	products, err := a.Store.ListProducts(start, count)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}

	respondWithJSON(w, http.StatusOK, products)
}

func (a *App) createProduct(w http.ResponseWriter, r *http.Request) {
	var p models.Product
	decoder := json.NewDecoder(r.Body)
	if err := decoder.Decode(&p); err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid request payload")
		return
	}
	defer r.Body.Close()

	id, err := a.Store.CreateProduct(p)
	if err != nil {
		respondWithError(w, http.StatusInternalServerError, err.Error())
		return
	}
	p.ID = id

	respondWithJSON(w, http.StatusCreated, p)
}

func (a *App) updateProduct(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	id, err := strconv.Atoi(vars["id"])
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid product ID")
		return
	}

	var p models.Product
	decoder := json.NewDecoder(r.Body)
	if err := decoder.Decode(&p); err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid resquest payload")
		return
	}
	defer r.Body.Close()
	p.ID = id

	if err := a.Store.UpdateProduct(p); err != nil {
		if err == errors.ErrProductNotFound {
			respondWithError(w, http.StatusNotFound, "Product not found")
		} else {
			respondWithError(w, http.StatusInternalServerError, err.Error())
		}
		return
	}

	respondWithJSON(w, http.StatusOK, p)
}

func (a *App) deleteProduct(w http.ResponseWriter, r *http.Request) {
	vars := mux.Vars(r)
	id, err := strconv.Atoi(vars["id"])
	if err != nil {
		respondWithError(w, http.StatusBadRequest, "Invalid Product ID")
		return
	}

	if err := a.Store.DeleteProduct(id); err != nil {
		if err == errors.ErrProductNotFound {
			respondWithError(w, http.StatusNotFound, "Product not found")
		} else {
			respondWithError(w, http.StatusInternalServerError, err.Error())
		}
		return
	}

	respondWithJSON(w, http.StatusOK, map[string]string{"result": "success"})
}

// Healthcheck endpoint that complies with Java Microprofile Health specification
func (a *App) healthCheck(w http.ResponseWriter, r *http.Request) {
	dbErr := a.Store.CheckHealth()
	if dbErr != nil {
		health := map[string]interface{}{
			"status": "DOWN",
			"checks": []map[string]interface{}{
				{
					"name":   "database",
					"status": "DOWN",
				},
			},
		}
		respondWithJSON(w, http.StatusServiceUnavailable, health)
		return
	}

	health := map[string]interface{}{
		"status": "UP",
		"checks": []map[string]interface{}{
			{
				"name":   "database",
				"status": "UP",
			},
		},
	}

	respondWithJSON(w, http.StatusOK, health)
}

// Middleware to check if the request has a valid JWT token
func (a *App) jwtAuthentication(requiredRoles []string, next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		tokenString := r.Header.Get("Authorization")
		if tokenString == "" {
			respondWithError(w, http.StatusUnauthorized, "No token provided, expected JWT Bearer token")
			return
		}
		// remove Bearer prefix
		tokenString = strings.Replace(tokenString, "Bearer ", "", 1)

		claims := jwt.MapClaims{}
		// this also automatically checks if the token is expired
		token, err := jwt.ParseWithClaims(tokenString, claims, func(token *jwt.Token) (interface{}, error) {
			if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
				return nil, fmt.Errorf("unexpected signing method: %v", token.Header["alg"])
			}
			return []byte(a.JwtSecret), nil
		})

		if err != nil {
			respondWithError(w, http.StatusUnauthorized, "Authentication error: "+err.Error())
			return
		}

		if !token.Valid {
			respondWithError(w, http.StatusUnauthorized, "Authentication error: Token is not valid")
			return
		}

		if !contains(requiredRoles, claims["sub"].(string)) {
			respondWithError(w, http.StatusUnauthorized, "Authentication error: Insufficient permissions")
			return
		}

		next(w, r)
	}
}

// helper function to check if a string is in a slice
func contains(arr []string, str string) bool {
	for _, item := range arr {
		if item == str {
			return true
		}
	}
	return false
}

func (a *App) initializeRoutes() {
	a.Router.HandleFunc("/products", a.getProducts).Methods("GET")
	a.Router.HandleFunc("/product/{id:[0-9]+}", a.getProduct).Methods("GET")
	// use jwtAuthentication middleware to protect the following endpoints
	a.Router.HandleFunc("/product", a.jwtAuthentication([]string{"admin", "moderator"}, a.createProduct)).Methods("POST")
	a.Router.HandleFunc("/product/{id:[0-9]+}", a.jwtAuthentication([]string{"admin", "moderator"}, a.updateProduct)).Methods("PUT")
	a.Router.HandleFunc("/product/{id:[0-9]+}", a.jwtAuthentication([]string{"admin"}, a.deleteProduct)).Methods("DELETE")
	// additional endpoints
	a.Router.HandleFunc("/health", a.healthCheck).Methods("GET")
}
