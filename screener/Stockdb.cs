using System;
using System.Linq;
using System.Collections.Generic;

using StockDataParser;


namespace StockDatabase
{

    public class CompanyInfo
    {
        public string symbol { get; set; }
        public string series { get; set; }
        public string industry { get; set; }
        public string companyName { get; set; }
        public decimal ltp { get; set; }
        public decimal change { get; set; }
    }

    public class StockHistory {
        public DateTime date {get; set;}
        public decimal ltp { get; set; }
        public decimal change {get; set; }
        public decimal totalTrades { get; set; }
        public long deliverableQty { get; set; }
        public decimal deliveryPercentage { get; set; }

    }

    public class StockStats
    {
        public string sector { get; set; }
        public string symbol { get; set; }
        public string series { get; set; }
        public decimal ltp { get; set; }
        public decimal[] avgPriceChange { get; set; }
        public decimal[] avgVolumeChage { get; set; }
    }

    public class SectorChange
    {
        public string sector { get; set; }
        public decimal change { get; set; }
        public SectorChange(string sector = "", decimal change = 0)
        {
            this.sector = sector;
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

        public List<CompanyInfo> getCompanyList()
        {
            using(var db = new StockDataContext())
            {
                var result = db.companyInformation.Select(x => new CompanyInfo() {
                                                    companyName = x.companyName,
                                                    symbol = x.symbol,
                                                    series = x.series,
                                                    industry = x.industry
                                                })
                                                .ToList();
                return result;

            }
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
                                               .Distinct()
                                               .OrderByDescending(x => x)
                                               .ToList();
                return (trading_days.Count() > day) ? trading_days[day] : trading_days.Last();
            }
        }

        public List<DailyStockData> GetLTP(int day = 0)
        {
            var lastTradedDate = GetLastTradeDate(day);
            return GetLTP(lastTradedDate);
        }

        public List<DailyStockData> GetLTP(DateTime date)
        {
            var symSectorMapping = getSymbolToIndustryMapping();
            using (var db = new StockDataContext())
            {
                var ltp = db.stockData.Where(x => (x.date.CompareTo(date.Date) == 0) && (x.series == "EQ" || x.series == "BE" || x.series == "SM")).ToList();
                ltp.ForEach(x => x.industry = symSectorMapping.TryGetValue(x.symbol, out string industry) ? industry : ConstValues.defaultIndustry);
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
                                           .OrderByDescending(x => x.change)
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
                return result;
            }
        }

        private StockStats getStockStats(string symbol, Dictionary<string, string> mapping, IEnumerable<DailyStockData> list)
        {
            StockStats stats = new StockStats();
            // Interval Range. All number are days
            var day_interval = new List<int>{5, 10, 20, 30, 60, 120, 240};

            // Allocate array and initialize to 0
            stats.avgPriceChange = Enumerable.Repeat<decimal>(0, day_interval.Count()+1).ToArray();
            stats.avgVolumeChage = Enumerable.Repeat<decimal>(0, day_interval.Count()+1).ToArray();

            // Set the symbol and series for this stock
            (stats.symbol, stats.series, stats.sector) =
                (symbol, "EQ", mapping.TryGetValue(symbol, out string sector) ? sector : ConstValues.defaultIndustry);

            // Set today's change and deliverable volume
            (stats.avgPriceChange[0], stats.avgVolumeChage[0], stats.ltp) = (list.ElementAt(0).change, list.ElementAt(0).deliverableQty, list.ElementAt(0).lastPrice);

            // Compute the avg price and deliverable volume
            for(int i = 0; i < day_interval.Count; i++)
            {
                // Check if the data exists for this interval
                if(list.Count() > day_interval[i])
                {
                    stats.avgPriceChange[i+1] = decimal.Round(100 * (list.ElementAt(1).lastPrice - list.ElementAt(day_interval[i]).open) / list.ElementAt(day_interval[i]).open, 2);
                    stats.avgVolumeChage[i+1] = decimal.Round((decimal)list.Skip(1).Take(day_interval[i]).Average(x => x.deliverableQty), 2);
                }
            }
            return stats;
        }

        public List<StockStats> GetStockStats()
        {
            var mapping = getSymbolToIndustryMapping();
            using(var db = new StockDataContext())
            {
                return db.stockData.Where(x => (x.series == "BE" || x.series == "EQ"))
                                   .GroupBy(x => new {x.symbol})
                                   .OrderBy(x => x.Key.symbol)
                                   .Select(x => getStockStats(x.Key.symbol,
                                                              mapping,
                                                              x.OrderByDescending(y => y.date)))
                                   .ToList();
            }
        }

        public List<StockHistory> GetStockHistory(string symbol, int numEntries)
        {
            using(var db = new StockDataContext())
            {
                var result = db.stockData.Where(x => (x.symbol == symbol) && (x.series == "EQ" || x.series == "BE"))
                                         .Select(x => new StockHistory {
                                                change = x.change,
                                                deliverableQty = x.deliverableQty,
                                                deliveryPercentage = x.deliveryPercentage,
                                                totalTrades = x.totalTrades,
                                                date = x.date.Date,
                                                ltp = x.close
                                            })
                                         .OrderByDescending(x => x.date)
                                         .Take(numEntries)
                                         .ToList();
                return result;
            }
        }
    }
}