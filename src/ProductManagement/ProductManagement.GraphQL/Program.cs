using Microsoft.EntityFrameworkCore;
using ProductManagement.BusinessLogic;
using ProductManagement.DataAccess;
using ProductManagement.EF;
using ProductManagement.GraphQL.Mutations;
using ProductManagement.GraphQL.Queries;
using ProductManagement.GraphQL.Subscriptions;

namespace ProductManagement.GraphQL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load the appsettings configuration based on the current environment
            var configuration = new ConfigurationBuilder()
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddTransient(typeof(IService<>), typeof(Service<>));
            builder.Services.AddTransient<OrderService, OrderService>();

            var connectionString = configuration["ConnectionString"];
            Console.WriteLine("ConnectionString: " + connectionString);
            builder.Services.AddDbContext<ProductManagementDbContext>(options =>
                options.UseNpgsql(connectionString));

            builder.Services.AddControllers();

            builder.Services
            .AddGraphQLServer()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .AddQueryType<Query>()
            .AddTypeExtension<ProductQuery>()
            .AddMutationType<Mutation>()
            .AddTypeExtension<OrderMutation>()
            .AddSubscriptionType<Subscription>()
            .AddTypeExtension<OrderSubscription>()
            .AddInMemorySubscriptions();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseRouting();
            
            app.UseWebSockets();
            //app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapGraphQL();
                endpoints.MapGet("/healthz", async context => {
                    using (var scope = app.Services.CreateScope()) {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ProductManagementDbContext>();
                        var result = await dbContext.Database.CanConnectAsync();
                        Console.WriteLine("Can connect to db: " + result);
                        context.Response.StatusCode = result ? 200 : 500;
                    }
                });
            });
            app.MapControllers();

            app.Run();
        }
    }
}