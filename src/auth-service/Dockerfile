# auth-service/Dockerfile
FROM golang:latest

WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY . .

RUN go build -o auth-service .

EXPOSE 8000

CMD ./auth-service -port 8000 -secret $APP_JWT_SECRET -exp $JWT_EXP_TIME