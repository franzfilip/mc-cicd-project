package models

type Warehouse struct {
	ID             int ` gorm:"primaryKey"`
	Zip            int ``
	StoredProducts []StoredProduct
}
