package models

import uuid "github.com/satori/go.uuid"

type StoredProduct struct {
	WarehouseID int ` gorm:"primaryKey"`
	Warehouse   Warehouse
	ProductUUID uuid.UUID `gorm:"primaryKey"`
	Product     Product
	ProductID   int ` gorm:"primaryKey"`
}
