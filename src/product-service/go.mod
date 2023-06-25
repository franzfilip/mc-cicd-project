module github.com/blazing-panda/mom-mom-schoerghuber/product-service

go 1.19

require (
	github.com/gorilla/mux v1.8.0
	github.com/lib/pq v1.10.8
	github.com/stretchr/testify v1.8.2
)

require (
	github.com/jackc/pgpassfile v1.0.0 // indirect
	github.com/jackc/pgservicefile v0.0.0-20221227161230-091c0ba34f0a // indirect
	github.com/jackc/pgx/v5 v5.3.1 // indirect
	github.com/jinzhu/inflection v1.0.0 // indirect
	github.com/jinzhu/now v1.1.5 // indirect
	github.com/pkg/errors v0.9.1 // indirect
	github.com/satori/go.uuid v1.2.0 // indirect
	golang.org/x/crypto v0.9.0 // indirect
	golang.org/x/text v0.9.0 // indirect
	gorm.io/driver/postgres v1.5.0 // indirect
	gorm.io/gorm v1.25.1 // indirect
)

require (
	github.com/davecgh/go-spew v1.1.1 // indirect
	github.com/golang-jwt/jwt/v5 v5.0.0
	github.com/jackc/pgx v3.6.2+incompatible
	github.com/pmezard/go-difflib v1.0.0 // indirect
	github.com/rabbitmq/amqp091-go v1.8.1
	github.com/rs/cors v1.9.0
	gopkg.in/yaml.v3 v3.0.1 // indirect
)

replace github.com/blazing-panda/mom-mom-schoerghuber/auth-service => ../auth-service
