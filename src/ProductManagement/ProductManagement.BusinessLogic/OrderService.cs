using Microsoft.EntityFrameworkCore;
using ProductManagement.DataAccess;
using ProductManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.BusinessLogic
{
    public class OrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderResult> PlaceOrder(ReceivedOrder order)
        {
            await _unitOfWork.BeginTransactionAsync();

            await CreateNewProductsIfNotEnough(order);
            var orderResult = new OrderResult
            {
                CustomerId = order.CustomerId,
                OrderResultItems = new List<OrderResultItem>()
            };

            var storedProductsToDelete = new List<StoredProduct>();

            foreach (var item in order.ReceivedOrderItems)
            {
                var storedProducts = await _unitOfWork.GetRepository<StoredProduct>().Get(sp => sp.ProductId == item.ProductId).ToListAsync();

                foreach(var sp in storedProducts)
                {
                    if(orderResult.OrderResultItems.Count() >= item.Amount)
                    {
                        break;
                    }
                    orderResult.OrderResultItems.Add(new OrderResultItem
                    {
                        ProductUUID = sp.ProductUUID,
                        WarehouseId = sp.WarehouseId,
                        ProductId = sp.ProductId
                    });
                    storedProductsToDelete.Add(sp);
                }

                var product = await _unitOfWork.GetRepository<Product>().Get(p => p.Id == item.ProductId).SingleOrDefaultAsync();

                if (product == null)
                {
                    throw new Exception($"Failed to fetch product with ID: {item.ProductId}");
                }

                orderResult.TotalAmount += product.Price * item.Amount;
            }

            await _unitOfWork.GetRepository<OrderResult>().AddAsync(orderResult);

            foreach (var sp in storedProductsToDelete)
            {
                await _unitOfWork.GetRepository<StoredProduct>().RemoveAsync(sp);
            }


            await _unitOfWork.CommitTransactionAsync();

            return orderResult;
        }

        private async Task CreateNewProductsIfNotEnough(ReceivedOrder order)
        {
            foreach (var item in order.ReceivedOrderItems)
            {
                var storedProducts = _unitOfWork.GetRepository<StoredProduct>().Get(sp => sp.ProductId == item.ProductId);

                if (item.Amount > storedProducts.Count())
                {
                    var amountToCreate = item.Amount - storedProducts.Count();
                    await SimulateCreationOfProducts(await GetMostFrequentWarehouseID(storedProducts), item.ProductId, amountToCreate);
                }
            }
        }

        private async Task SimulateCreationOfProducts(long warehouseId, long productId, int amountToCreate)
        {
            for (int i = 0; i < amountToCreate; i++)
            {
                await _unitOfWork.GetRepository<StoredProduct>().AddAsync(new StoredProduct
                {
                    WarehouseId = warehouseId,
                    ProductId = productId,
                    ProductUUID = Guid.NewGuid().ToString()
                });
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<long> GetMostFrequentWarehouseID(IQueryable<StoredProduct> storedProducts)
        {
            var mostFrequentWarehouseId = storedProducts
                .GroupBy(sp => sp.WarehouseId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            if (mostFrequentWarehouseId == 0)
            {
                var warehouses = await _unitOfWork.GetRepository<Warehouse>().Get().ToListAsync();
                var random = new Random();
                mostFrequentWarehouseId = warehouses.ElementAt(random.Next(warehouses.Count())).Id;
            }

            return mostFrequentWarehouseId;
        }
    }
}
