package models

import uuid "github.com/satori/go.uuid"

type Order struct {
	CustomerID uuid.UUID   `json:"customer_id"`
	OrderItems []OrderItem `json:"order_items"`
}
