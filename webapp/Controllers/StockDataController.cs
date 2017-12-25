using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using screener;
using StockDatabase;

namespace webapp.Controllers
{
    [Route("api/[controller]")]
    public class StockDataController : Controller
    {
        StockMarket stockMarket;
        public StockDataController()
        {
            stockMarket = new StockMarket();
        }

        [HttpGet("[action]")]
        public IActionResult  CompanyList()
        {
            try
            {
                return Ok(stockMarket.getCompanyList());
            }
            catch(Exception)
            {
               return NotFound("Exception in CompanyList() function");
            }
        }
    }
}
