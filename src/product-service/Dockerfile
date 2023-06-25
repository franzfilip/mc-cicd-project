# product-service/Dockerfile
FROM golang:latest

WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY . .

RUN go build -o product-service .

EXPOSE 8010

CMD [ "./product-service" ]
