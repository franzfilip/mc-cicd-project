using ProductManagement.Models;

namespace ProductManagement.GraphQL.ObjectTypes
{
    public class ProductOType: ObjectType<Product>
    {
        protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
        {
            descriptor.Field(x => x.StoredProducts)
                .Type<ListType<NonNullType<StoredProductOType>>>();
            base.Configure(descriptor);
        }
    }
}
