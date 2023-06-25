using HotChocolate.Subscriptions;
using ProductManagement.BusinessLogic;
using ProductManagement.GraphQL.InputTypes;
using ProductManagement.Models;
using System.Net.Mail;

namespace ProductManagement.GraphQL.Resolvers
{
    public class OrderResolver
    {
        public async Task<OrderResult> PlaceOrder([Service]OrderService orderService, [Service]ITopicEventSender sender, ReceivedOrder order)
        {
            var result = await orderService.PlaceOrder(order);
            await sender.SendAsync("orderPlaced", result);
            return result;
        }
    }
}
