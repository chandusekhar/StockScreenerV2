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

        private Dictionary<string, string> getSymbolToIndustryMapping()
        {
            using(var db = new StockDataContext())
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                db.companyInformation.Select(x => new {x.symbol, x.industry}).ToList().ForEach(x => dict.TryAdd(x.symbol, x.industry));
                return dict;
            }
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

        // 0 refers to the last trading day and positive number (day) refers to
        // last trading day - day
        public DateTime GetLastTradeDate(int day = 0)
        {
            if(day < 0) throw new Exception(@"Day ${day} should be greater or equal to 0");
            using(var db = new StockDataContext())
            {
                var trading_days = db.stockData.Select(x => x.date).Distinct().OrderByDescending(x => x).ToList();
                return (trading_days.Count() > day) ? trading_days[day] : trading_days.Last();
            }
        }

        public List<DailyStockData> GetLTP(int day = 0)
        {
            var lastTradedDate = GetLastTradeDate(day);
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
            for(int i = 0; i < 10; i++)
            {
                var stockPrices = GetLTP(i);

                var result = (from stock in stockPrices
                            group stock by stock.industry into segments
                            select new {
                                Industry = segments.Key,
                                change = Math.Round(segments.Average(x => x.change), 2)
                            })
                            .OrderBy(x => x.change)
                            .ToList();

                foreach(var item in result)
                {
                    Console.WriteLine("{0} {1}", item.Industry, item.change);
                }
            }
        }
    }
}