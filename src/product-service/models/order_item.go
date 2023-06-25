package models

type OrderItem struct {
	ProductID int `json:"product_id"`
	Amount    int `json:"amount"`
}
