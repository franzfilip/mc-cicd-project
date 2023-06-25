package stores

import (
	"database/sql"
	"fmt"

	"github.com/blazing-panda/mom-mom-schoerghuber/product-service/errors"
	"github.com/blazing-panda/mom-mom-schoerghuber/product-service/models"
)

type SQLProductStore struct {
	db *sql.DB
}

func NewSQLProductStore(db *sql.DB) *SQLProductStore {
	return &SQLProductStore{db: db}
}

func (store *SQLProductStore) GetProduct(id int) (models.Product, error) {
	var p models.Product
	err := store.db.QueryRow("SELECT id, name, price FROM products WHERE id=$1", id).Scan(&p.ID, &p.Name, &p.Price)
	if err != nil {
		if err == sql.ErrNoRows {
			return models.Product{}, errors.ErrProductNotFound
		}
		return models.Product{}, fmt.Errorf("get product: %w", err)
	}
	return p, nil
}

func (store *SQLProductStore) UpdateProduct(p models.Product) error {
	_, err := store.db.Exec("UPDATE products SET name=$1, price=$2 WHERE id=$3", p.Name, p.Price, p.ID)
	if err != nil {
		if err == sql.ErrNoRows {
			return errors.ErrProductNotFound
		}
		return fmt.Errorf("update product: %w", err)
	}
	return nil
}

func (store *SQLProductStore) DeleteProduct(id int) error {
	_, err := store.db.Exec("DELETE FROM products WHERE id=$1", id)
	if err != nil {
		if err == sql.ErrNoRows {
			return errors.ErrProductNotFound
		}
		return fmt.Errorf("delete product: %w", err)
	}
	return nil
}

func (store *SQLProductStore) CreateProduct(p models.Product) (int, error) {
	err := store.db.QueryRow("INSERT INTO products(name, price) VALUES($1, $2) RETURNING id", p.Name, p.Price).Scan(&p.ID)
	if err != nil {
		return 0, fmt.Errorf("create product: %w", err)
	}
	return p.ID, nil
}

func (store *SQLProductStore) ListProducts(start, count int) ([]models.Product, error) {
	rows, err := store.db.Query("SELECT id, name, price FROM products LIMIT $1 OFFSET $2", count, start)
	if err != nil {
		return nil, fmt.Errorf("list products: %w", err)
	}
	defer rows.Close()

	products := []models.Product{}
	for rows.Next() {
		var p models.Product
		if err := rows.Scan(&p.ID, &p.Name, &p.Price); err != nil {
			return nil, fmt.Errorf("list products: %w", err)
		}
		products = append(products, p)
	}

	return products, nil
}

func (store *SQLProductStore) CheckHealth() error {
	return store.db.Ping()
}
