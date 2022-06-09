using System;
using System.Net;
using System.Threading.Tasks;
using Npgsql;
using Transactions.Models;
using WebApplication1.Models;

namespace WebApplication1.Services
{
public class DbService : IDbService
    {
        private const string ConnString =
            "Host=localhost:49155;Username=postgres;Password=postgrespw;Database=zad4";

        public async Task<Order> GetOrderAsync(int idProduct, int amount)
        {
            string sql = ("SELECT * FROM \"Order\" WHERE IdProduct = @IdProduct AND Amount = @amount");
            await using NpgsqlConnection connection = new(ConnString);
            await using NpgsqlCommand comm = new(sql, connection);
            comm.Parameters.AddWithValue("IdProduct", idProduct);
            comm.Parameters.AddWithValue("amount", amount);
            await connection.OpenAsync();

            await using NpgsqlDataReader rdr = await comm.ExecuteReaderAsync();
            await rdr.ReadAsync();

            if (!rdr.HasRows)
            {
                return null;
            }

            DateTime? FulfilledAt = (rdr["FulfilledAt"] == DBNull.Value) ? null : (DateTime)rdr["FulfilledAt"];
            Order result = new Order()
            {
                IdOrder = int.Parse(rdr["IdOrder"].ToString()),
                IdProduct = int.Parse(rdr["IdProduct"].ToString()),
                Amount = int.Parse(rdr["Amount"].ToString()),
                CreatedAt = (DateTime)rdr["CreatedAt"],
                FulfilledAt = FulfilledAt,
            };
            
            await connection.CloseAsync();
            return result;
        }

        public async Task<Product> GetProductAsync(int id)
        {
            string sql = ("SELECT * FROM Product WHERE idProduct = @idProduct");
            await using NpgsqlConnection connection = new(ConnString);
            await using NpgsqlCommand comm = new(sql, connection);
            comm.Parameters.AddWithValue("idProduct", id);
            await connection.OpenAsync();

            await using NpgsqlDataReader rdr = await comm.ExecuteReaderAsync();
            await rdr.ReadAsync();

            if (!rdr.HasRows)
            {
                return null;
            }

            Product result = new Product()
            {
                IdProduct = int.Parse(rdr["IdProduct"].ToString()),
                Name = rdr["Name"].ToString(),
                Description = rdr["Description"].ToString(),
                Price = (decimal)rdr["Price"]
            };
            
            await connection.CloseAsync();
            return result;
        }

        public async Task<Warehouse> GetWarehouseAsync(int id)
        {
            string sql = ("SELECT * FROM Warehouse WHERE idWarehouse = @idWarehouse");
            await using NpgsqlConnection connection = new(ConnString);
            await using NpgsqlCommand comm = new(sql, connection);
            comm.Parameters.AddWithValue("idWarehouse", id);
            await connection.OpenAsync();

            await using NpgsqlDataReader rdr = await comm.ExecuteReaderAsync();
            await rdr.ReadAsync();

            if (!rdr.HasRows)
            {
                return null;
            }

            Warehouse result = new Warehouse()
            {
                IdWarehouse = int.Parse(rdr["IdWarehouse"].ToString()),
                Name = rdr["Name"].ToString(),
                Address = rdr["Address"].ToString(),
            };
            
            await connection.CloseAsync();
            return result;
        }

        public async Task<ProductWarehouse> GetProductWarehouseAsync(int id)
        {
            {
                string sql = ("SELECT * FROM Product_Warehouse WHERE IdOrder = @idOrder");
                await using NpgsqlConnection connection = new(ConnString);
                await using NpgsqlCommand comm = new(sql, connection);
                comm.Parameters.AddWithValue("idOrder", id);
                await connection.OpenAsync();

                await using NpgsqlDataReader rdr = await comm.ExecuteReaderAsync();
                await rdr.ReadAsync();

                if (!rdr.HasRows)
                {
                    return null;
                }

                ProductWarehouse result = new ProductWarehouse()
                {
                    IdProductWarehouse = int.Parse(rdr["IdProductWarehouse"].ToString()),
                    IdWarehouse = int.Parse(rdr["IdWarehouse"].ToString()),
                    IdOrder = int.Parse(rdr["IdOrder"].ToString()),
                    IdProduct = int.Parse(rdr["IdProduct"].ToString()),
                    Amount = int.Parse(rdr["Amount"].ToString()),
                    Price = Convert.ToDecimal(rdr["Price"].ToString()),
                    CreatedAt = (DateTime)rdr["CreatedAt"],
                };
            
                await connection.CloseAsync();
                return result;
            }
        }

        public async Task UpdateOrderAsync(int id)
        {
            DateTime currentDate = DateTime.Now;
            string sql = ("UPDATE \"Order\" SET FulfilledAt = @currentDate WHERE idOrder = @idOrder");
            await using NpgsqlConnection connection = new(ConnString);
            await using NpgsqlCommand comm = new(sql, connection);
            comm.Parameters.AddWithValue("currentDate", currentDate);
            comm.Parameters.AddWithValue("idOrder", id);


            await connection.OpenAsync();
            await comm.ExecuteReaderAsync();
        }

        public async Task AddProductWarehouseAsync(ProductWarehouse productWarehouse)
        {
            string sql = "INSERT INTO product_warehouse (idwarehouse, idProduct, idOrder, amount, price, createdAt) VALUES (@idWarehouse, @idProduct, @idOrder, @amount, @price, @createdAt)";
            await using NpgsqlConnection connection = new(ConnString);
            await using NpgsqlCommand comm = new(sql, connection);
            comm.Parameters.AddWithValue("idWarehouse",  productWarehouse.IdWarehouse);
            comm.Parameters.AddWithValue("idProduct", productWarehouse.IdProduct);
            comm.Parameters.AddWithValue("idOrder", productWarehouse.IdOrder);
            comm.Parameters.AddWithValue("amount", productWarehouse.Amount);
            comm.Parameters.AddWithValue("price", productWarehouse.Price);
            comm.Parameters.AddWithValue("createdAt", productWarehouse.CreatedAt);

            await connection.OpenAsync();
            await using NpgsqlDataReader rdr = await comm.ExecuteReaderAsync();
        }

        public async Task<MethodResult> AddNewProductWarehouse(ProductWarehouse productWarehouse)
        {
            if (productWarehouse.Amount <= 0)
            {
                return new()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Amount should be greater than 0"
                };
            }

            Product product = await GetProductAsync(productWarehouse.IdProduct);
            if (product == null)
            {
                return new()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Not found Product with given IdProduct"
                };
            }

            Warehouse warehouse = await GetWarehouseAsync(productWarehouse.IdWarehouse);
            if (warehouse == null)
            {
                return new()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Not found Warehouse with given IdWarehouse"
                };
            }

            Order order = await GetOrderAsync(productWarehouse.IdProduct, productWarehouse.Amount);
            
            if (order == null)
            {
                return new()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = "Not found Order with given IdProduct and amount"
                };
            }

            if (order.FulfilledAt != null)
            {
                return new()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Order is already realised"
                };
            }

            if (order.CreatedAt >= productWarehouse.CreatedAt)
            {
                return new()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Created at in order should be lower than created at in productWarehouse"
                };
            }

            ProductWarehouse oldProductWarehouse = await GetProductWarehouseAsync(order.IdOrder);

            if (oldProductWarehouse != null)
            {
                return new()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Product Warehouse with provided data already exist"
                };
            }
            
            
            productWarehouse.CreatedAt = DateTime.Now;
            productWarehouse.Price = productWarehouse.Amount * product.Price;
            productWarehouse.IdOrder = order.IdOrder;
            await AddProductWarehouseAsync(productWarehouse);
            await UpdateOrderAsync(order.IdOrder);

            ProductWarehouse result = await GetProductWarehouseAsync(productWarehouse.IdOrder);
            
            return new()
            {
                StatusCode = HttpStatusCode.OK,
                Message = result.IdProductWarehouse.ToString()
            };
        }
    }
}