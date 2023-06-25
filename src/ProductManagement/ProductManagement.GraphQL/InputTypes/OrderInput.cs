using ProductManagement.Models;

namespace ProductManagement.GraphQL.InputTypes
{
    public class OrderInput : InputObjectType<ReceivedOrder>
    {
        protected override void Configure(IInputObjectTypeDescriptor<ReceivedOrder> descriptor)
        {
            descriptor.Field(o => o.ReceivedOrderItems)
                .Name("orderItems")
                .Type<NonNullType<ListType<NonNullType<OrderItemInput>>>>();
        }
    }
}
