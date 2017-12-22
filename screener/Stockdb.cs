using System;
using System.Linq;

using StockMarket;
using StockDataParser;
using System.Collections.Generic;

namespace screener
{
    public class StockDB
    {
        public StockDB()
        {

        }

        public int AddCompaniesToList(List<CompanyInformation> list)
        {
            using(var db = new StockDataContext())
            {
                list.ForEach(x => db.companyInformation.Add(x));

                // Return the number of companies added to list
                return db.SaveChanges();
            }
        }

        public int AddDailyStockData(List<DailyStockData> list, DateTime date)
        {
            using(var db = new StockDataContext())
            {
                foreach(var item in list)
                {
                    db.stockData.Add(item);
                }
                var count = db.SaveChanges();
                Console.WriteLine("Added {0} companies Stock Data for {1} to db", count, date.ToString());
                return count;
            }
        }

        public DateTime GetLastTradeDate()
        {
            using(var db = new StockDataContext())
            {
                return db.stockData.Select(x => x.date).OrderByDescending(x => x).FirstOrDefault();
            }
        }

        private Dictionary<string, string> getSymbolToIndustryMapping()
        {
            using(var db = new StockDataContext())
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                db.companyInformation.Select(x => new {x.symbol, x.industry}).ToList().ForEach(x => dict.TryAdd(x.symbol, x.industry));
                return dict;
            }
        }
        public List<DailyStockData> GetLTP()
        {
            var lastTradedDate = GetLastTradeDate();
            var symbolIndustryMapping = getSymbolToIndustryMapping();
            using(var db = new StockDataContext())
            {
                var ltp = db.stockData.Where(x => x.date.CompareTo(lastTradedDate) == 0).OrderBy(x => x.industry).ToList();
                foreach (var item in ltp)
                {
                    string industry;
                    item.industry = symbolIndustryMapping.TryGetValue(item.symbol, out industry) ? industry : ConstValues.defaultIndustry;
                }
                return ltp;
            }
        }

        public void GetIndustyChange()
        {
            var stockPrices = GetLTP();

            var result = from stock in stockPrices
                         group stock by stock.industry into segments
                         select new { Industry = segments.Key, change = Math.Round(segments.Average(x => x.change), 2)};

            result = result.OrderBy(x => x.change);
            foreach(var item in result)
            {
                Console.WriteLine("{0} {1}", item.Industry, item.change);
            }
        }
    }
}