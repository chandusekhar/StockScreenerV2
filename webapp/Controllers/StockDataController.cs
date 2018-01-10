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
        static StockMarket stockMarket = new StockMarket();
        public StockDataController()
        {

        }

        [HttpGet("[action]")]
        public IActionResult CompanyList()
        {
            try
            {
                return Ok(stockMarket.getCompanyList());
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"WEBAPI CompanyList() failed with exception. Message {ex.Message}");
                return NotFound("Exception in CompanyList() function");
            }
        }

        [HttpGet("[action]")]
        public IActionResult TodayStockReport()
        {
            try
            {
                var result = stockMarket.getLTP().Select(x => new
                {
                    change = x.change,
                    symbol = x.symbol,
                    series = x.series,
                    lastPrice = x.lastPrice,
                    industry = x.industry,
                    qty = x.totalTradedQty
                })
                                                .ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"WEBAPI TodayStockReport() failed with exception. Message {ex.Message}");
                return NotFound("Exception in TodayStockReport() function");
            }
        }

        [HttpGet("[action]")]
        public IActionResult TodayVolumeReport()
        {
            try
            {
                var result = stockMarket.GetStockStats().ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"WEBAPI TodayVolumeReport() failed with exception. Message {ex.Message}");
                return NotFound("Exception in TodayVolumeReport() function");
            }
        }

        [HttpGet("[action]")]
        public IActionResult TodaySectorChange()
        {
            try
            {
                var result = stockMarket.getSectorChange();
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"WEBAPI TodaySectorChange() failed with exception. Message {ex.Message}");
                return NotFound("Exception in TodaySectorReport() function");
            }
        }

        [HttpGet("[action]")]
        public IActionResult GetHistory(string symbol)
        {
            try
            {
                var result = stockMarket.GetStockHistory(symbol);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"WEBAPI GetHistory() failed with exception. Message {ex.Message}");
                return NotFound("Exception in GetHistory() function");
            }
        }

        [HttpGet("[action]")]
        public IActionResult GetSectorStats()
        {
            try
            {
                var result = stockMarket.getSectorMonthlyStats();
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"WEBAPI GetSectorStats() failed with exception. Message {ex.Message}");
                return NotFound("Exception in GetSectorStats() function");
            }
        }

        [HttpGet("[action]")]
        public IActionResult UpdateBhav()
        {
            try
            {
                stockMarket.updateBhavDataToLatest();
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"WEBAPI UpdateBhav() failed with exception. Message {ex.Message}");
                return NotFound("Exception in UpdateBhav() function");
            }
        }

        [HttpGet("[action]")]
        public IActionResult GetStockMonthlyStats(int year)
        {
            try
            {
                var result = stockMarket.getStockMonthlyStats(year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.WriteLine($"WEBAPI GetStockMonthlyStats() failed with exception. Message {ex.Message}");
                return NotFound("Exception in GetStockMonthlyStats() function");
            }
        }
    }
}
