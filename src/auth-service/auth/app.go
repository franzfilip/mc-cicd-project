package auth

import (
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"time"

	"github.com/golang-jwt/jwt/v5"
	"github.com/gorilla/mux"
	"github.com/rs/cors"
)

type Credentials struct {
	Username string `json:"username"`
	Password string `json:"password"`
}

type App struct {
	JwtSecret         string
	JwtExpirationTime int
	Router            *mux.Router
}

func (a *App) Initialize(jwtSecret string, jwtExpirationTime int, router *mux.Router) {
	a.Router = router
	a.JwtSecret = jwtSecret
	a.JwtExpirationTime = jwtExpirationTime
	a.Router.HandleFunc(
		"/token", a.tokenHandler).Methods("POST")
	a.Router.HandleFunc("/health", a.healthCheck).Methods("GET")
}

func (a *App) Run(port int) {
	c := cors.New(cors.Options{
		AllowedOrigins: []string{"*"},
		AllowedMethods: []string{"POST", "PUT", "GET", "DELETE"},
		AllowedHeaders: []string{"Content-Type", "Access-Control-Allow-Origin", "Access-Control-Allow-Headers"},
		Debug:          true,
	})

	log.Printf("Server listening on :%d\n", port)
	log.Fatal(http.ListenAndServe(fmt.Sprintf(":%d", port), c.Handler(a.Router)))
}

func (a *App) tokenHandler(w http.ResponseWriter, r *http.Request) {
	var credentials Credentials
	err := json.NewDecoder(r.Body).Decode(&credentials)
	if err != nil {
		log.Printf("Error decoding request body: %v", err)
		respondWithError(w, http.StatusBadRequest, "Invalid request payload. Expected format: { \"username\": \"string\", \"password\": \"string\"}\"")
		return
	}

	// Replace this with proper authentication logic
	// hardcoded users => could be replaced with users from a database
	if credentials.Username == "admin" && credentials.Password == "admin" {
		token, err := a.generateJWT("admin", "admin")
		if err != nil {
			respondWithError(w, http.StatusInternalServerError, err.Error())
			return
		}
		respondWithJSON(w, http.StatusOK, map[string]string{"token": token})
		return
	}

	if credentials.Username == "moderator" && credentials.Password == "moderator" {
		token, err := a.generateJWT("moderator", "moderator")
		if err != nil {
			respondWithError(w, http.StatusInternalServerError, err.Error())
			return
		}
		respondWithJSON(w, http.StatusOK, map[string]string{"token": token})
		log.Printf("Token issued for user: %s", credentials.Username)
		return
	}

	log.Printf("Invalid credentials: %v", credentials)
	respondWithError(w, http.StatusUnauthorized, "Invalid credentials")
}

// Generate a JWT token
func (a *App) generateJWT(username, role string) (string, error) {
	expirationTime := time.Now().Add(time.Duration(a.JwtExpirationTime) * time.Second)

	claims := jwt.MapClaims{
		"exp":      expirationTime.Unix(),
		"iss":      "auth-service",
		"sub":      role,
		"username": username,
	}

	token := jwt.NewWithClaims(jwt.SigningMethodHS256, claims)

	tokenString, err := token.SignedString([]byte(a.JwtSecret))
	if err != nil {
		log.Printf("Error signing token: %v", err)
		return "", err
	}

	return tokenString, nil
}

func respondWithError(w http.ResponseWriter, code int, message string) {
	respondWithJSON(w, code, map[string]string{"error": message})
}

// Healthcheck endpoint that complies with Java Microprofile Health specification
func (a *App) healthCheck(w http.ResponseWriter, r *http.Request) {

	health := map[string]interface{}{
		"status": "UP",
		"checks": []map[string]interface{}{
			{
			},
		},
	}

	respondWithJSON(w, http.StatusOK, health)
}

func respondWithJSON(w http.ResponseWriter, code int, payload interface{}) {
	response, _ := json.Marshal(payload)

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(code)
	_, err := w.Write(response)
	if err != nil {
		log.Printf("Error writing response: %v", err)
	}
}
