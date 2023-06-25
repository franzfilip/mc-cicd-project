package main

import (
	"flag"

	"github.com/blazing-panda/mom-mom-schoerghuber/auth-service/auth"
	"github.com/gorilla/mux"
)

func main() {
	var port int
	flag.IntVar(&port, "port", 8080, "Port on which the server will listen")

	var argSecretKey string
	flag.StringVar(&argSecretKey, "secret", "secret", "Secret key for signing JWT tokens")

	var jwtExpirationTime int
	flag.IntVar(&jwtExpirationTime, "exp", 300, "Expiration time for JWT tokens in seconds")

	flag.Parse()

	a := auth.App{}
	a.Initialize(argSecretKey, jwtExpirationTime, mux.NewRouter())
	a.Run(port)
}
