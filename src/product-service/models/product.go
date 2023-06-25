package models

type Product struct {
	ID    int     `json:"id" json:"order_result_item_id" gorm:"primaryKey"`
	Name  string  `json:"name"`
	Price float32 `json:"price"`
}
