using System;
using System.Linq;

using StockMarket;
using StockDataParser;
using System.Collections.Generic;

namespace screener
{
    public class SectorChange
    {
        public string industry { get; set; }
        public decimal change { get; set; }
        public int numOfCompanies {get; set; }
    }
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

        private void UpdateSectorChangeInformation(DateTime date)
        {
            var symIndMapping = getSymbolToIndustryMapping();
            using (var db = new StockDataContext())
            {
                var ltp = db.stockData.Where(x => x.date.CompareTo(date.Date) == 0).ToList();
                ltp.ForEach(x => x.industry = symIndMapping.TryGetValue(x.symbol, out string industry) ? industry : ConstValues.defaultIndustry);
                var sectorChange = ltp.GroupBy(x => x.industry)
                                      .Select(x => new SectorInformation()
                                      {
                                          date = date,
                                          industry = x.Key,
                                          change = decimal.Round(x.Average(y => y.change), 2)
                                      })
                                      .OrderBy(x => x.industry);
                db.sectorInformation.AddRange(sectorChange);
                Console.WriteLine("Saved : {0}", db.SaveChanges());
            }
        }

        public int AddDailyStockData(List<DailyStockData> stockData, DateTime date)
        {
            var symIndMapping = getSymbolToIndustryMapping();
            using(var db = new StockDataContext())
            {
                db.stockData.AddRange(stockData);
                stockData.ForEach(x => x.industry = symIndMapping.TryGetValue(x.symbol, out string industry) ? industry : ConstValues.defaultIndustry);
                var sectorChange = stockData.GroupBy(x => x.industry)
                                       .Select(x => new SectorInformation() {
                                                date = date,
                                                industry = x.Key,
                                                change = decimal.Round(x.Average(y => y.change), 2)
                                        })
                                        .OrderBy(x => x.industry);
                db.sectorInformation.AddRange(sectorChange);
                var count = db.SaveChanges();
                Console.WriteLine("Added {0} rows while updating companies Stock Data for {1} ", count, date.ToString());
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
            var symIndMapping = getSymbolToIndustryMapping();
            using(var db = new StockDataContext())
            {
                var ltp = db.stockData.Where(x => x.date.CompareTo(lastTradedDate) == 0).ToList();

                ltp.ForEach(x => x.industry = symIndMapping.TryGetValue(x.symbol, out string industry) ? industry : ConstValues.defaultIndustry);
                return ltp;
            }
        }

        public List<DailyStockData> GetLTP(DateTime date)
        {
            var symIndMapping = getSymbolToIndustryMapping();
            using(var db = new StockDataContext())
            {
                var ltp = db.stockData.Where(x => x.date.CompareTo(date.Date) == 0).ToList();

                ltp.ForEach(x => x.industry = symIndMapping.TryGetValue(x.symbol, out string industry) ? industry : ConstValues.defaultIndustry);
                return ltp;
            }
        }

        public List<SectorChange> GetIndustyChange(int day = 0)
        {
            var lastTradedDate = GetLastTradeDate(day);
            using(var db = new StockDataContext())
            {
                return db.sectorInformation.Where(x => x.date.CompareTo(lastTradedDate.Date) == 0)
                                           .Select(x => new SectorChange() { industry = x.industry, change = x.change})
                                           .ToList();
            }
        }
    }
}