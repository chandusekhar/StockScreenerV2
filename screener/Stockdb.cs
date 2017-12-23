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
        public SectorChange(string industry = "", decimal change = 0)
        {
            this.industry = industry;
            this.change = decimal.Round(change, 2);
        }
    }
    public class MonthlyStats
    {
        public int month { get; set; }
        public int year { get; set; }
        public decimal change { get; set; }

        public MonthlyStats(int month = 0, int year = 0, decimal change = 0)
        {
            this.month = month;
            this.year =  year;
            this.change = decimal.Round(change, 2);
        }
    }
    public class SectorStats
    {
        public string industry { get; set; }
        public List<MonthlyStats> stats;
    }
    public class StockDB
    {
        public StockDB()
        {

        }

        private Dictionary<string, string> getSymbolToIndustryMapping()
        {
            using (var db = new StockDataContext())
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                db.companyInformation.Select(x => new { x.symbol, x.industry })
                                     .ToList()
                                     .ForEach(x => dict.TryAdd(x.symbol, x.industry));
                return dict;
            }
        }

        public int AddCompaniesToList(List<CompanyInformation> list)
        {
            using (var db = new StockDataContext())
            {
                db.companyInformation.AddRange(list);
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

                // Set the industry for each company
                ltp.ForEach(x => x.industry = symIndMapping.TryGetValue(x.symbol, out string industry) ? industry : ConstValues.defaultIndustry);
                var sectorChange = ltp.GroupBy(x => x.industry)
                                      .Select(x => new SectorInformation(date, x.Key, x.Average(y => y.change)))
                                      .OrderBy(x => x.industry);
                db.sectorInformation.AddRange(sectorChange);
                Console.WriteLine("Saved : {0}", db.SaveChanges());
            }
        }

        public int AddDailyStockData(List<DailyStockData> stockData, DateTime date)
        {
            var symIndMapping = getSymbolToIndustryMapping();
            using (var db = new StockDataContext())
            {
                db.stockData.AddRange(stockData);
                stockData.ForEach(x => x.industry = symIndMapping.TryGetValue(x.symbol, out string industry) ? industry : ConstValues.defaultIndustry);
                var sectorChange = stockData.GroupBy(x => x.industry)
                                            .Select(x => new SectorInformation(date, x.Key, x.Average(y => y.change)))
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
            if (day < 0) throw new Exception(@"Day ${day} should be greater or equal to 0");
            using (var db = new StockDataContext())
            {
                var trading_days = db.stockData.Select(x => x.date)
                                               .Distinct().OrderByDescending(x => x)
                                               .ToList();
                return (trading_days.Count() > day) ? trading_days[day] : trading_days.Last();
            }
        }

        public List<DailyStockData> GetLTP(int day = 0)
        {
            var lastTradedDate = GetLastTradeDate(day);
            var symIndMapping = getSymbolToIndustryMapping();
            using (var db = new StockDataContext())
            {
                var ltp = db.stockData.Where(x => x.date.CompareTo(lastTradedDate) == 0).ToList();

                ltp.ForEach(x => x.industry = symIndMapping.TryGetValue(x.symbol, out string industry) ? industry : ConstValues.defaultIndustry);
                return ltp;
            }
        }

        public List<DailyStockData> GetLTP(DateTime date)
        {
            var symIndMapping = getSymbolToIndustryMapping();
            using (var db = new StockDataContext())
            {
                var ltp = db.stockData.Where(x => x.date.CompareTo(date.Date) == 0).ToList();

                ltp.ForEach(x => x.industry = symIndMapping.TryGetValue(x.symbol, out string industry) ? industry : ConstValues.defaultIndustry);
                return ltp;
            }
        }

        public List<SectorChange> GetSectorChange(int day = 0)
        {
            var lastTradedDate = GetLastTradeDate(day);
            using (var db = new StockDataContext())
            {
                return db.sectorInformation.Where(x => x.date.CompareTo(lastTradedDate.Date) == 0)
                                           .Select(x => new SectorChange(x.industry, x.change))
                                           .ToList();
            }
        }

        public List<SectorStats> GetSectorMonthlyStats()
        {
            using (var db = new StockDataContext())
            {
                var result = db.sectorInformation
                               .GroupBy(x => new { x.industry })
                               .OrderBy(x => x.Key.industry)
                               .Select(x => new SectorStats()
                                    {
                                        industry = x.Key.industry,
                                        stats = x.GroupBy(y => new { y.date.Year, y.date.Month })
                                                 .Select(y => new MonthlyStats(y.Key.Month, y.Key.Year, y.Average(a => a.change)))
                                                 .OrderByDescending(y => y.year)
                                                 .ThenByDescending(y => y.month)
                                                 .ToList()
                                    }
                               )
                               .ToList();
                foreach (var item in result)
                {
                    //Console.WriteLine("{0}, {1}, {2}", item.industry, item.stats[0].month, item.stats[0].change);
                    Console.Write("{0} ", item.industry);
                    foreach(var i in item.stats)
                    {
                        Console.Write(" {0} ", i.change);
                    }
                    Console.WriteLine("");
                }
                return result;
            }
        }

        public class StockStats
        {
            public string symbol { get; set; }
            public string series { get; set; }
            public decimal[] avgPriceChange { get; set; }
            public decimal[] avgVolumeChage { get; set; }

            public StockStats(string symbol, string series, List<DailyStockData> list)
            {
                // Interval Range. All number are days
                var int_list = new List<int>{5, 10, 20, 30, 60, 120, 240};

                // Allocate array and initialize to 0
                avgPriceChange = Enumerable.Repeat<decimal>(0, int_list.Count()+1).ToArray();
                avgVolumeChage = Enumerable.Repeat<decimal>(0, int_list.Count()+1).ToArray();

                // Set the symbol and series for this stock
                (this.symbol, this.series) = (symbol, series);

                // Set today's change and deliverable volume
                (avgPriceChange[0], avgVolumeChage[0]) = (list[0].change, list[0].deliverableQty);

                // Compute the avg price and deliverable volume
                for(int i = 0; i < int_list.Count; i++)
                {
                    // Check if the data exists for this interval
                    if(list.Count() > int_list[i])
                    {
                        avgPriceChange[i+1] = decimal.Round(100 * (list[0].lastPrice - list[int_list[i]].open) / list[int_list[i]].open, 2);
                        avgVolumeChage[i+1] = decimal.Round((decimal)list.Take(int_list[i]).Average(x => x.deliverableQty), 2);
                    }
                }
            }
        }

        public List<StockStats> GetStockStats()
        {
            using(var db = new StockDataContext())
            {
                return db.stockData.Where(x => (x.series == "BE" || x.series == "EQ"))
                                   .GroupBy(x => new {x.symbol, x.series})
                                   .OrderBy(x => x.Key.symbol)
                                   .ThenBy(x => x.Key.series)
                                   .Select(x => new StockStats(x.Key.symbol,
                                                               x.Key.series,
                                                               x.OrderByDescending(y => y.date).ToList()))
                                   .ToList();
            }
        }
    }
}