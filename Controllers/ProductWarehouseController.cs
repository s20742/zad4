using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Transactions.Models;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductWarehouseController : ControllerBase
    {
        private readonly IDbService _dbService;

        public ProductWarehouseController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpPost]
        public async Task<IActionResult> AddProductWarehouse(ProductWarehouse productWarehouse)
        {
            MethodResult result = await _dbService.AddNewProductWarehouse(productWarehouse);

            return StatusCode((int)result.StatusCode, result.Message);
        }
    }
}