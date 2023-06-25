using ProductManagement.GraphQL.ObjectTypes;
using ProductManagement.GraphQL.Resolvers;

namespace ProductManagement.GraphQL.Queries
{
    public class ProductQuery: ObjectTypeExtension<Query>
    {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
        {
            descriptor.Field("products")
                .ResolveWith<ProductResolver>(x => x.GetProducts(default))
                .UseProjection()
                .UseFiltering()
                .UseSorting()
                .Type<ListType<NonNullType<ProductOType>>>();
        }
    }
}
