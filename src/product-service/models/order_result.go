package models

import uuid "github.com/satori/go.uuid"

type OrderResult struct {
	ID               int               ` json:"order_id" gorm:"primaryKey"`
	CustomerID       uuid.UUID         `json:"customer_id"`
	TotalAmount      float32           `json:"total_amount"`
	OrderResultItems []OrderResultItem `json:"order_result_items"`
}
