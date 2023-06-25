using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;

namespace ConsumerSimulator
{
    public class Simulator
    {
        private readonly int _customersToSimulate = 1;
        private readonly string _placeOrderQueue = "placeorder";
        public Simulator(int customersToSimulate)
        {
            _customersToSimulate = customersToSimulate;
        }

        public void StartSimulation()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _placeOrderQueue,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            SendOrders(channel);

        }

        private void SendOrders(IModel channel)
        {
            ConcurrentBag<OrderResult> results = new ConcurrentBag<OrderResult>();
            Parallel.For(0, _customersToSimulate, p =>
            {
                List<string> output = new List<string>();
                Random random = new Random();
                int ordersToPlace = random.Next(1, 3);

                var order = new Order { CustomerId = Guid.NewGuid(), OrderItems = GenerateRandomOrderItems() };

                for (int i = 0; i < ordersToPlace; i++)
                {
                    AddOrderToQueue(order, channel, output);
                    order.OrderItems[0].Amount = 2;
                }
                HandleResponse(order.CustomerId, ordersToPlace, channel, output).ForEach(x => results.Add(x));

                output.ForEach(s =>
                {
                    Console.WriteLine(s);
                });
            });

            Console.WriteLine("\n\n###########################################################################################");
            Console.WriteLine($"{results.GroupBy(x => x.CustomerId).Count()} Customers have placed {results.GroupBy(x => x.OrderId).Count()} Orders resulting in {results.Sum(x => x.TotalAmount)}");
        }

        private List<OrderItem> GenerateRandomOrderItems()
        {
            List<OrderItem> orderItems = new();
            Random random = new Random();
            int orderItemsToCreate = random.Next(1, 3);

            for (int i = 0; i < orderItemsToCreate; i++)
            {
                orderItems.Add(new OrderItem { Amount = random.Next(1, 10), ProductId = random.Next(1, 15) });
            }

            return orderItems;
        }

        private void AddOrderToQueue(Order order, IModel channel, List<string> output)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(order));

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: _placeOrderQueue,
                                 basicProperties: null,
                                 body: body);

            AddToOutput(output, $"Published Order on Queue {_placeOrderQueue}");
            AddToOutput(output, order.ToString());
        }

        private List<OrderResult> HandleResponse(Guid customerId, int requiredAnswers, IModel channel, List<string> output)
        {
            List<OrderResult> results = new List<OrderResult>();

            AddToOutput(output, $"==================================Listening on {customerId}==================================", false);
            channel.QueueDeclare(queue: customerId.ToString(),
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            while (results.Count < requiredAnswers)
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    try
                    {
                        var result = JsonConvert.DeserializeObject<OrderResult>(message);
                        results.Add(result);
                        AddToOutput(output, $"Received Order:\n{result}");
                    }
                    catch (Exception ex)
                    {
                        AddToOutput(output, $"{customerId} Order is Pending: {message}");
                    }
                };
                channel.BasicConsume(queue: customerId.ToString(),
                                     autoAck: true,
                                     consumer: consumer);
            }

            channel.QueueDelete(customerId.ToString(), false, false);
            AddToOutput(output, $"==================================Finished on {customerId}==================================", false);

            return results;
        }

        private void AddToOutput(List<string> output, string value, bool includeTab = true)
        {
            if (includeTab)
            {
                value = value.Replace("\r", "");
                if (value.Contains("\n"))
                {
                    foreach (var item in value.Split("\n"))
                    {
                        output.Add($"\t{item}");
                    }
                }
                else
                {
                    output.Add($"\t{value}");
                }
            }
            else
            {
                output.Add(value);
            }
        }
    }
}
