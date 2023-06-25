using ConsumerSimulator.GraphQLClient;
using ConsumerSimulator.GraphQLClient.State;
using DataModel;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ConsumerSimulator {
    public class Simulator {
        private readonly IConsumerSimulator _client;

        public Simulator(IConsumerSimulator client) {
            _client = client;
        }

        public async Task StartSimulationAsync() {
            var products = await GetProducts();
            PrintProducts(products);

            _client.OrderPlaced.Watch().Subscribe(result =>
            {
                PrintOrderPlaced(result.Data);
            });

            //wait until inited
            await Task.Delay(2000);

            while (true) {
                var result = await PlaceOrder(products.Select(x => x.Id).ToArray());
                PrintMutationResult(result);

                //Rerun every 5 seconds
                await Task.Delay(5000);
            }
        }

        private void PrintMutationResult(List<Tuple<Guid, decimal>> results) {
            Console.WriteLine("-----------------------Mutation Order Placed------------------------------------------");
            Console.WriteLine($"{results.Count} customers have been placed orders in worth of: {results.Select(x => x.Item2).Sum()} EUR");
            Console.WriteLine("-----------------------Mutation Order Placed------------------------------------------");
        }

        private async Task<List<Product>> GetProducts() {
            ProductFilterInput filterInput = new ProductFilterInput { Price = new DecimalOperationFilterInput { Gt = 20m } };
            ProductSortInput productSortInput = new ProductSortInput { Price = SortEnumType.Desc };
            var result = await _client.GetProducts.ExecuteAsync(filterInput, new List<ProductSortInput> { productSortInput });
            List<Product> products = new List<Product>();
            if (result.Errors?.Any() ?? false) {
                Console.WriteLine("Errors:");
                foreach (var error in result.Errors) {
                    Console.WriteLine(error.Message);
                }
            }
            else {
                foreach (var product in result.Data.Products) {
                    products.Add(new Product {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price
                    });
                }
            }

            return products;
        }

        private void PrintProducts(List<Product> products) {
            int idWidth = 5;
            int nameWidth = 80;
            int priceWidth = 10;
            string separator = new string('-', idWidth + nameWidth + priceWidth + 7); // 7 is for spaces and "|"

            string headerFormat = "{0,-" + idWidth + "} | {1,-" + nameWidth + "} | {2," + priceWidth + "}";
            string rowFormat = "{0,-" + idWidth + "} | {1,-" + nameWidth + "} | {2," + priceWidth + "}";

            Console.WriteLine(separator);
            Console.WriteLine(String.Format(headerFormat, "Id", "Name", "Price"));
            Console.WriteLine(separator);
            foreach (var product in products) {
                Console.WriteLine(String.Format(rowFormat, product.Id, product.Name, product.Price));
            }
            Console.WriteLine(separator);
        }

        private async Task<List<Tuple<Guid, decimal>>> PlaceOrder(long[] productIds) {
            Random rand = new Random();
            int customers = rand.Next(1, 1);
            List<Tuple<Guid, decimal>> results = new();

            for(int i = 0; i < customers; i++) {
                int productsToOrder = rand.Next(1, 5);
                Guid customerId = Guid.NewGuid();

                ReceivedOrderInput orderInput = new ReceivedOrderInput { CustomerId = customerId.ToString() };
                List<ReceivedOrderItemsInput> itemsInput = new List<ReceivedOrderItemsInput>();
                for(int j = 0; j < productsToOrder; j++) {
                    itemsInput.Add(new ReceivedOrderItemsInput { Amount = rand.Next(1, 5), ProductId = productIds[rand.Next(productIds.Count())] });
                }
                orderInput.OrderItems = itemsInput;

                var result = await _client.PlaceOrder.ExecuteAsync(orderInput);

                if (result.Errors?.Any() ?? false) {
                    Console.WriteLine("Errors:");
                    foreach (var error in result.Errors) {
                        Console.WriteLine(error.Message);
                    }
                }
                else {
                    results.Add(new Tuple<Guid, decimal>(customerId, result.Data.PlaceOrder.TotalAmount));
                }
            }

            return results;
        }

        private void PrintOrderPlaced(IOrderPlacedResult orderResult) {
            if (orderResult.OrderPlaced == null) {
                Console.WriteLine("No order placed.");
                return;
            }

            var order = orderResult.OrderPlaced;

            int idWidth = 15;
            int customerWidth = 45;
            int totalWidth = 20;
            int productWidth = 15;
            int productUUIDWidth = 45;
            int warehouseWidth = 20;
            int productPriceWidth = 15;

            string separator = new string('#', idWidth + customerWidth + totalWidth + productWidth + productUUIDWidth + warehouseWidth + productPriceWidth + 28); // 28 is for spaces and "|"

            string headerFormat = "{0,-" + idWidth + "} | {1,-" + customerWidth + "} | {2," + totalWidth + "} | {3,-" + productWidth + "} | {4,-" + productUUIDWidth + "} | {5," + warehouseWidth + "} | {6," + productPriceWidth + "}";
            string rowFormat = "{0,-" + idWidth + "} | {1,-" + customerWidth + "} | {2," + totalWidth + "} | {3,-" + productWidth + "} | {4,-" + productUUIDWidth + "} | {5," + warehouseWidth + "} | {6," + productPriceWidth + "}";

            Console.WriteLine(separator);
            Console.WriteLine(String.Format(headerFormat, "Order ID", "Customer ID", "Total Amount", "Product ID", "Product UUID", "Warehouse ID", "Product Price"));
            Console.WriteLine(separator);

            foreach (var item in order.OrderResultItems) {
                Console.WriteLine(String.Format(rowFormat,
                    order.Id,
                    order.CustomerId,
                    order.TotalAmount,
                    item.Product.Id,
                    item.ProductUUID,
                    item.WarehouseId,
                    item.Product.Price));
            }

            Console.WriteLine(separator);
        }


    }
}
