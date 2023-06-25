package interfaces

import "github.com/blazing-panda/mom-mom-schoerghuber/product-service/models"

type ProductStore interface {
	GetProduct(id int) (models.Product, error)
	UpdateProduct(p models.Product) error
	DeleteProduct(id int) error
	CreateProduct(p models.Product) (int, error)
	ListProducts(start, count int) ([]models.Product, error)
	CheckHealth() error
}
