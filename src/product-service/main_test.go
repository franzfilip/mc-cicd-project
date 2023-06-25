// main_test.go

package main

import (
	"bytes"
	"database/sql"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"net/http/httptest"
	"os"
	"strconv"
	"testing"

	"github.com/blazing-panda/mom-mom-schoerghuber/auth-service/auth"
	"github.com/blazing-panda/mom-mom-schoerghuber/product-service/stores"
	"github.com/gorilla/mux"
	"github.com/stretchr/testify/assert"
)

var a App
var db *sql.DB

var adminToken = "your_admin_jwt_token"
var moderatorToken = "your_moderator_jwt_token"

func TestMain(m *testing.M) {
	user := os.Getenv("APP_DB_USERNAME")
	password := os.Getenv("APP_DB_PASSWORD")
	dbname := os.Getenv("APP_DB_NAME")
	connectionString :=
		fmt.Sprintf("user=%s password=%s dbname=%s sslmode=disable", user, password, dbname)
	var err error
	db, err = sql.Open("postgres", connectionString)
	if err != nil {
		log.Fatal(err)
		return
	}
	router := mux.NewRouter()

	a = App{}
	a.Initialize(
		stores.NewSQLProductStore(db),
		os.Getenv("APP_JWT_SECRET"),
		router)
	b := auth.App{}
	b.Initialize(os.Getenv("APP_JWT_SECRET"), 300, router)

	ensureTableExists()

	code := m.Run()
	clearTable()
	os.Exit(code)
}

// these need to be executed before all other tests
func TestGenerateAdminTokenSuccess(t *testing.T) {
	payload := []byte(`{"username":"admin","password":"admin"}`)

	req, _ := http.NewRequest("POST", "/token", bytes.NewBuffer(payload))
	req.Header.Set("Content-Type", "application/json")
	response := executeRequest(req)

	assert.Equal(t, http.StatusOK, response.Code)

	var responseBody map[string]string
	err := json.Unmarshal(response.Body.Bytes(), &responseBody)
	assert.Nil(t, err)
	assert.NotEmpty(t, responseBody["token"])

	adminToken = responseBody["token"] // Update the global variable with the retrieved token
}

func TestGenerateModeratorTokenSuccess(t *testing.T) {
	payload := []byte(`{"username":"moderator","password":"moderator"}`)

	req, _ := http.NewRequest("POST", "/token", bytes.NewBuffer(payload))
	req.Header.Set("Content-Type", "application/json")
	response := executeRequest(req)

	assert.Equal(t, http.StatusOK, response.Code)

	var responseBody map[string]string
	err := json.Unmarshal(response.Body.Bytes(), &responseBody)
	assert.Nil(t, err)
	assert.NotEmpty(t, responseBody["token"])

	moderatorToken = responseBody["token"] // Update the global variable with the retrieved token
}

func TestGenerateTokenFail(t *testing.T) {
	payload := []byte(`{"username":"admin","password":"wrong_password"}`)

	req, _ := http.NewRequest("POST", "/token", bytes.NewBuffer(payload))
	req.Header.Set("Content-Type", "application/json")
	response := executeRequest(req)

	assert.Equal(t, http.StatusUnauthorized, response.Code)
}

func ensureTableExists() {
	if _, err := db.Exec(tableCreationQuery); err != nil {
		log.Fatal(err)
	}
}

func clearTable() {
	db.Exec("DELETE FROM products")
	db.Exec("ALTER SEQUENCE products_id_seq RESTART WITH 1")
}

const tableCreationQuery = `CREATE TABLE IF NOT EXISTS products
(
    id SERIAL,
    name TEXT NOT NULL,
    price NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    CONSTRAINT products_pkey PRIMARY KEY (id)
)`

func TestEmptyTable(t *testing.T) {
	clearTable()

	req, _ := http.NewRequest("GET", "/products", nil)
	response := executeRequest(req)

	checkResponseCode(t, http.StatusOK, response.Code)

	if body := response.Body.String(); body != "[]" {
		t.Errorf("Expected an empty array. Got %s", body)
	}
}

func executeRequest(req *http.Request) *httptest.ResponseRecorder {
	rr := httptest.NewRecorder()
	a.Router.ServeHTTP(rr, req)

	return rr
}

func checkResponseCode(t *testing.T, expected, actual int) {
	if expected != actual {
		t.Errorf("Expected response code %d. Got %d\n", expected, actual)
	}
}

func TestGetNonExistentProduct(t *testing.T) {
	clearTable()

	req, _ := http.NewRequest("GET", "/product/11", nil)
	response := executeRequest(req)

	checkResponseCode(t, http.StatusNotFound, response.Code)

	var m map[string]string
	json.Unmarshal(response.Body.Bytes(), &m)
	if m["error"] != "Product not found" {
		t.Errorf("Expected the 'error' key of the response to be set to 'Product not found'. Got '%s'", m["error"])
	}
}

func TestCreateProduct(t *testing.T) {

	clearTable()

	var jsonStr = []byte(`{"name":"test product", "price": 11.22}`)
	req, _ := http.NewRequest("POST", "/product", bytes.NewBuffer(jsonStr))
	req.Header.Set("Authorization", adminToken)
	req.Header.Set("Content-Type", "application/json")

	response := executeRequest(req)
	checkResponseCode(t, http.StatusCreated, response.Code)

	var m map[string]interface{}
	json.Unmarshal(response.Body.Bytes(), &m)

	if m["name"] != "test product" {
		t.Errorf("Expected product name to be 'test product'. Got '%v'", m["name"])
	}

	if m["price"] != 11.22 {
		t.Errorf("Expected product price to be '11.22'. Got '%v'", m["price"])
	}

	// the id is compared to 1.0 because JSON unmarshaling converts numbers to
	// floats, when the target is a map[string]interface{}
	if m["id"] != 1.0 {
		t.Errorf("Expected product ID to be '1'. Got '%v'", m["id"])
	}
}

func TestGetProduct(t *testing.T) {
	clearTable()
	addProducts(1)

	req, _ := http.NewRequest("GET", "/product/1", nil)
	response := executeRequest(req)

	checkResponseCode(t, http.StatusOK, response.Code)
}

// main_test.go

func addProducts(count int) {
	if count < 1 {
		count = 1
	}

	for i := 0; i < count; i++ {
		db.Exec("INSERT INTO products(name, price) VALUES($1, $2)", "Product "+strconv.Itoa(i), (i+1.0)*10)
	}
}

func TestUpdateProduct(t *testing.T) {

	clearTable()
	addProducts(1)

	req, _ := http.NewRequest("GET", "/product/1", nil)
	response := executeRequest(req)
	var originalProduct map[string]interface{}
	json.Unmarshal(response.Body.Bytes(), &originalProduct)

	var jsonStr = []byte(`{"name":"test product - updated name", "price": 11.22}`)
	req, _ = http.NewRequest("PUT", "/product/1", bytes.NewBuffer(jsonStr))
	req.Header.Set("Authorization", adminToken)
	req.Header.Set("Content-Type", "application/json")

	response = executeRequest(req)

	checkResponseCode(t, http.StatusOK, response.Code)

	var m map[string]interface{}
	json.Unmarshal(response.Body.Bytes(), &m)

	if m["id"] != originalProduct["id"] {
		t.Errorf("Expected the id to remain the same (%v). Got %v", originalProduct["id"], m["id"])
	}

	if m["name"] == originalProduct["name"] {
		t.Errorf("Expected the name to change from '%v' to '%v'. Got '%v'", originalProduct["name"], m["name"], m["name"])
	}

	if m["price"] == originalProduct["price"] {
		t.Errorf("Expected the price to change from '%v' to '%v'. Got '%v'", originalProduct["price"], m["price"], m["price"])
	}
}

func TestDeleteProductWithSufficientPermissions(t *testing.T) {
	clearTable()
	addProducts(1)

	req, _ := http.NewRequest("GET", "/product/1", nil)
	response := executeRequest(req)
	checkResponseCode(t, http.StatusOK, response.Code)

	req, _ = http.NewRequest("DELETE", "/product/1", nil)
	req.Header.Set("Authorization", adminToken)
	response = executeRequest(req)

	checkResponseCode(t, http.StatusOK, response.Code)

	req, _ = http.NewRequest("GET", "/product/1", nil)
	response = executeRequest(req)
	checkResponseCode(t, http.StatusNotFound, response.Code)
}

func TestDeleteProductWithoutToken(t *testing.T) {
	clearTable()
	addProducts(1)

	req, _ := http.NewRequest("GET", "/product/1", nil)
	response := executeRequest(req)
	checkResponseCode(t, http.StatusOK, response.Code)

	req, _ = http.NewRequest("DELETE", "/product/1", nil)
	response = executeRequest(req)

	checkResponseCode(t, http.StatusUnauthorized, response.Code)
}

func TestDeleteProductWithInsufficientPermissions(t *testing.T) {
	clearTable()
	addProducts(1)

	req, _ := http.NewRequest("GET", "/product/1", nil)
	response := executeRequest(req)
	checkResponseCode(t, http.StatusOK, response.Code)

	req, _ = http.NewRequest("DELETE", "/product/1", nil)
	req.Header.Set("Authorization", moderatorToken)
	response = executeRequest(req)

	checkResponseCode(t, http.StatusUnauthorized, response.Code)
}

func TestDeleteProductWithInvalidToken(t *testing.T) {
	clearTable()
	addProducts(1)

	req, _ := http.NewRequest("GET", "/product/1", nil)
	response := executeRequest(req)
	checkResponseCode(t, http.StatusOK, response.Code)

	req, _ = http.NewRequest("DELETE", "/product/1", nil)
	req.Header.Set("Authorization", "invalidToken")
	response = executeRequest(req)

	checkResponseCode(t, http.StatusUnauthorized, response.Code)
}
