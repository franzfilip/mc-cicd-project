using ProductManagement.BusinessLogic;
using ProductManagement.Models;

namespace ProductManagement.GraphQL.Resolvers
{
    public class ProductResolver
    {
        public IQueryable<Product> GetProducts([Service] IService<Product> productService)
        {
            return productService.Get();
        }
    }
}
