// main.go

package main

import (
	"database/sql"
	"encoding/json"
	"fmt"
	"log"
	"os"

	"github.com/blazing-panda/mom-mom-schoerghuber/product-service/models"
	"github.com/blazing-panda/mom-mom-schoerghuber/product-service/stores"
	"github.com/gorilla/mux"
	amqp "github.com/rabbitmq/amqp091-go"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
)

func main() {
	user := os.Getenv("APP_DB_USERNAME")
	password := os.Getenv("APP_DB_PASSWORD")
	dbname := os.Getenv("APP_DB_NAME")
	host := os.Getenv("APP_DB_HOST")
	connectionString :=
		fmt.Sprintf("host=%s user=%s password=%s dbname=%s sslmode=disable", host, user, password, dbname)
	db, err := sql.Open("postgres", connectionString)
	if err != nil {
		log.Fatal(err)
		return
	}
	a := App{}
	a.Initialize(
		stores.NewSQLProductStore(db),
		os.Getenv("APP_JWT_SECRET"),
		mux.NewRouter())

	// Start the HTTP server in a goroutine
	go func() {
		println("Starting HTTP server on port 8088")
		a.Run(8088)
	}()

	go func() {
		var dsn = ""
		if os.Getenv("AMQP_USER") == "" {
			dsn = "host=localhost user=postgres password=example dbname=prod port=5432"

		} else {
			dsn = connectionString
		}

		db, err := gorm.Open(postgres.Open(dsn), &gorm.Config{})
		if err != nil {
			panic("failed to connect database")
		}
		db.AutoMigrate(&models.Product{})
		db.AutoMigrate(&models.Warehouse{})
		db.AutoMigrate(&models.StoredProduct{})
		db.AutoMigrate(&models.OrderResult{})
		db.AutoMigrate(&models.OrderResultItem{})

		log.Println("Connecting to RabbitMQ...")
		var amqpuri = getRabbitMQConnectionString()
		conn, err := amqp.Dial(amqpuri)
		failOnError(err, "Failed to connect to RabbitMQ")
		log.Println("Connected to RabbitMQ...")
		defer conn.Close()

		ch, err := conn.Channel()
		failOnError(err, "Failed to open a channel")
		defer ch.Close()

		q, err := ch.QueueDeclare(
			"placeorder", // name
			false,        // durable
			false,        // delete when unused
			false,        // exclusive
			false,        // no-wait
			nil,          // arguments
		)
		failOnError(err, "Failed to declare a queue")

		msgs, err := ch.Consume(
			q.Name, // queue
			"",     // consumer
			true,   // auto-ack
			false,  // exclusive
			false,  // no-local
			false,  // no-wait
			nil,    // args
		)
		failOnError(err, "Failed to register a consumer")

		var orderManagement = stores.NewOrdermanagementImpl(db, ch)

		for msg := range msgs {
			log.Printf("Received a message: %s", msg.Body)
			var order models.ReceivedOrder
			err := json.Unmarshal([]byte(msg.Body), &order)
			if err != nil {
				fmt.Println("Error:", err)
				return
			}
			orderManagement.PlaceOrder(order)
		}
	}()

	// Wait forever
	select {}
}

func failOnError(err error, msg string) {
	if err != nil {
		log.Panicf("%s: %s", msg, err)
	}
}

func getRabbitMQConnectionString() string {
	var localhost = "amqp://guest:guest@localhost:5672/"
	var amqpUser = os.Getenv("AMQP_USER")
	var amqpHost = os.Getenv("AMQP_HOST")
	var ampqPassword = os.Getenv("AMQP_PASSWORD")

	if amqpUser == "" && ampqPassword == "" {
		return localhost
	}
	var connstring = "amqp://" + amqpUser + ":" + ampqPassword + "@" + amqpHost + ":5672/"
	log.Print("RabbitMQ connection string: " + connstring)
	return connstring
}
