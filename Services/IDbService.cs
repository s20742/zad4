using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transactions.Models;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IDbService
    {
        Task<Order> GetOrderAsync(int idProduct, int amount);
        Task<Product> GetProductAsync(int id);
        Task<Warehouse> GetWarehouseAsync(int id);
        Task<ProductWarehouse> GetProductWarehouseAsync(int id);
        Task UpdateOrderAsync(int id);

        Task AddProductWarehouseAsync(ProductWarehouse productWarehouse);
        Task<MethodResult> AddNewProductWarehouse(ProductWarehouse productWarehouse);
    }
}