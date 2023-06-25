package stores

import (
	"context"
	"encoding/json"
	"log"
	"math/rand"
	"time"

	"github.com/blazing-panda/mom-mom-schoerghuber/product-service/models"
	amqp "github.com/rabbitmq/amqp091-go"
	uuid "github.com/satori/go.uuid"
	"gorm.io/gorm"
)

type OrderManagement struct {
	db      *gorm.DB
	channel *amqp.Channel
}

func NewOrdermanagementImpl(db *gorm.DB, conn *amqp.Channel) *OrderManagement {
	return &OrderManagement{db: db, channel: conn}
}

func (self *OrderManagement) PlaceOrder(order models.ReceivedOrder) models.OrderResult {
	self.createNewProductsIfNotEnough(order)
	orderResult := models.OrderResult{CustomerID: order.CustomerID, OrderResultItems: []models.OrderResultItem{}}
	var productsToDelete = []models.StoredProduct{}
	for _, item := range order.OrderItems {
		var storedProducts = self.getStoredProductsByProductID(item.ProductID)

		for count, sp := range storedProducts {
			if count > item.Amount {
				break
			}
			orderResult.OrderResultItems = append(orderResult.OrderResultItems, models.OrderResultItem{ProductUUID: sp.ProductUUID, WarehouseID: sp.WarehouseID, ProductID: sp.ProductID})
			productsToDelete = append(productsToDelete, sp)
		}

		//get price of current product
		var product = models.Product{ID: item.ProductID}
		if err := self.db.First(&product).Error; err != nil {
			log.Fatal("Failed to fetch product:", err)
		}

		orderResult.TotalAmount += product.Price * float32(item.Amount)

	}

	//Create OrderResult
	self.db.Create(&orderResult)

	//Delete the selected StoredProducts from the StoredProducts table
	for _, sp := range productsToDelete {
		self.db.Delete(&sp)
	}

	q, err := self.channel.QueueDeclare(
		order.CustomerID.String(), // name
		false,                     // durable
		false,                     // delete when unused
		false,                     // exclusive
		false,                     // no-wait
		nil,                       // arguments
	)
	failOnError(err, "Failed to declare a queue")

	ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancel()
	body, _ := json.Marshal(orderResult)
	log.Printf("Sending message to queue %s with content %s\n", q.Name, body)
	err = self.channel.PublishWithContext(ctx,
		"",     // exchange
		q.Name, // routing key
		false,  // mandatory
		false,  // immediate
		amqp.Publishing{
			ContentType: "text/json",
			Body:        []byte(body),
		})

	return orderResult
}

func (self *OrderManagement) createNewProductsIfNotEnough(order models.ReceivedOrder) {
	//Checks if enough products are available for each item
	for _, item := range order.OrderItems {
		var storedProducts = self.getStoredProductsByProductID(item.ProductID)

		if item.Amount > len(storedProducts) {
			log.Printf("Creating %d products for (%d)\n", item.Amount-len(storedProducts), item.ProductID)

			q, err := self.channel.QueueDeclare(
				order.CustomerID.String(), // name
				false,                     // durable
				false,                     // delete when unused
				false,                     // exclusive
				false,                     // no-wait
				nil,                       // arguments
			)
			failOnError(err, "Failed to declare a queue")

			ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
			defer cancel()
			// log.Printf("Sending message to queue %s", q.Name)
			body := "Product needs to be created [SIMULATING...]"
			log.Printf("Sending message to queue %s with content %s\n", q.Name, body)

			err = self.channel.PublishWithContext(ctx,
				"",     // exchange
				q.Name, // routing key
				false,  // mandatory
				false,  // immediate
				amqp.Publishing{
					ContentType: "text/plain",
					Body:        []byte(body),
				})

			// time.Sleep(1000)
			self.simulateCreationOfProducts(self.getMostFrequentWarehouseID(storedProducts), item.ProductID, item.Amount-len(storedProducts))
		}
	}
}

func failOnError(err error, msg string) {
	if err != nil {
		log.Panicf("%s: %s", msg, err)
	}
}

func (self *OrderManagement) getStoredProductsByProductID(productID int) []models.StoredProduct {
	var storedProducts []models.StoredProduct
	if err := self.db.Where("product_id = ?", productID).Find(&storedProducts).Error; err != nil {
		log.Fatal("Failed to fetch stored products:", err)
	}

	return storedProducts
}

func (self *OrderManagement) simulateCreationOfProducts(warehouseId int, productId int, amountToCreate int) {
	for i := 0; i < amountToCreate; i++ {
		self.db.Create(&models.StoredProduct{WarehouseID: warehouseId, ProductID: productId, ProductUUID: uuid.NewV1()})
	}
	log.Printf("Created %d products with id (%d) in warehouse %d\n", amountToCreate, productId, warehouseId)
}

func (self *OrderManagement) getMostFrequentWarehouseID(storedProducts []models.StoredProduct) int {
	freq := make(map[int]int)

	for _, sp := range storedProducts {
		freq[sp.WarehouseID]++
	}

	maxFreq := 0
	mostFreqID := 0

	for id, f := range freq {
		if f > maxFreq {
			maxFreq = f
			mostFreqID = id
		}
	}

	rand.Seed(time.Now().UnixNano())
	if mostFreqID == 0 && len(freq) > 0 {
		var value = rand.Intn(len(freq))
		return freq[value]
	} else if len(freq) == 0 {
		var warehouses []models.Warehouse
		if err := self.db.Find(&warehouses).Error; err != nil {
			log.Fatal("Failed to fetch warehouses:", err)
		}
		var value = rand.Intn(len(warehouses))
		return warehouses[value].ID
	}

	return mostFreqID
}
