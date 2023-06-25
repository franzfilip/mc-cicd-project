package models

import uuid "github.com/satori/go.uuid"

type OrderResultItem struct {
	ID            int         ` json:"order_result_item_id" gorm:"primaryKey"`
	ProductID     int         ` json:"product_id"`
	Product       Product     `json:"product"`
	ProductUUID   uuid.UUID   `json:"product_uuid"`
	WarehouseID   int         ` json:"warehouse_id"`
	Warehouse     Warehouse   `json:"warehouse"`
	OrderResultID int         ` json:"order_result_id"`
	OrderResult   OrderResult `json:"order_result"`
}
