using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using ProductManagement.BusinessLogic;
using ProductManagement.GraphQL.ObjectTypes;
using ProductManagement.Models;

namespace ProductManagement.GraphQL.Subscriptions
{
    public class OrderSubscription: ObjectTypeExtension<Subscription>
    {
        protected override void Configure(IObjectTypeDescriptor<Subscription> descriptor)
        {
            descriptor
                .Field("orderPlaced")
                .Type<OrderResultOType>()
                .Resolve(context => context.GetEventMessage<OrderResult>())
                .Subscribe(async context =>
                {
                    var reiver = context.Service<ITopicEventReceiver>();

                    ISourceStream stream = await reiver.SubscribeAsync<OrderResult>("orderPlaced");

                    return stream;
                });
            base.Configure(descriptor);
        }
    }
}
