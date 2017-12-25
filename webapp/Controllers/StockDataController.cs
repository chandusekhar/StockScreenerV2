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

        [HttpGet("[action]")]
        public IActionResult  TodayReport()
        {
            try
            {
                var result = stockMarket.getLTP().Select(x => new {
                    change = x.change,
                    symbol = x.symbol,
                    series = x.series,
                    lastPrice = x.lastPrice,
                    industry = x.industry
                }).ToList();
                return Ok(result);
            }
            catch(Exception)
            {
               return NotFound("Exception in TodayReport() function");
            }
        }
    }
}
