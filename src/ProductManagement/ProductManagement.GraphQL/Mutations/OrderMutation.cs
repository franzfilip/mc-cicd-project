using HotChocolate.Types;
using ProductManagement.GraphQL.InputTypes;
using ProductManagement.GraphQL.ObjectTypes;
using ProductManagement.Models;

namespace ProductManagement.GraphQL.Mutations
{
    public class OrderMutation: ObjectTypeExtension<Mutation>
    {
        protected override void Configure(IObjectTypeDescriptor<Mutation> descriptor)
        {
              descriptor.Field("placeOrder")
                .Argument("order", x => x.Type<NonNullType<OrderInput>>())
                .Type<NonNullType<OrderResultOType>>()
                .ResolveWith<Resolvers.OrderResolver>(x => x.PlaceOrder(default, default, default));
        }
    }
}
