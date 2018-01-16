using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using StockDataParser;
using StockDatabase;

namespace screener
{

    class CachedResult
    {
        public List<DailyStockData> ltpData;
        public List<StockStats> stats;
        public List<SectorChange> sectorChange;
        public List<CompanyInfo> companyInfo;
        public List<SectorStats> sectorStats;
        public List<StockMonthlyStats> monthlyStats;

        public CachedResult()
        {
            ltpData = new List<DailyStockData>();
            stats = new List<StockStats>();
            sectorChange = new List<SectorChange>();
            companyInfo = new List<CompanyInfo>();
            sectorStats = new List<SectorStats>();
            monthlyStats = new List<StockMonthlyStats>();
        }

        public void Clear() {
            ltpData.Clear();
            stats.Clear();
            sectorChange.Clear();
            companyInfo.Clear();
            sectorStats.Clear();
            monthlyStats.Clear();
        }
    }


    public class Logger
    {
        public static void WriteLine(string str)
        {
            Console.WriteLine("{0}: {1}", DateTime.Now, str);
        }
    }

    public class StockMarket
    {
        static CachedResult cache = new CachedResult();

        StockDB dB;
        public StockMarket()
        {
            dB = new StockDB();
        }
        public void updateCompaniesList()
        {
            NseStockData nse = new NseStockData();
            var list = nse.getListOfCompanies();
            if (list == null)
            {
                Console.WriteLine("Could not get list of companies from NSE");
                return;
            }
            if(0 != dB.AddCompaniesToList(list))
            {
                StockMarket.cache.Clear();
            }
        }

        public int updateBhavData(DateTime date)
        {
            int  count = 0;
            NseStockData nse = new NseStockData();
            var result = nse.updateBhavData(date);
            if (result.stockData != null)
            {
                count += dB.AddDailyStockData(result.stockData, result.circuitBreakerData, date);
            }
            if(count != 0)
            {
                StockMarket.cache.Clear();
            }
            return count;
        }

        public void updateBhavData(string bhavFile, string mtoFile)
        {
            int count = 0;
            var date = DateTime.Parse("01/01/2018");
            NseStockData nse = new NseStockData();
            var stockData = nse.updateBhavData(bhavFile, mtoFile);
            if (stockData != null)
            {
                count += dB.AddDailyStockData(stockData, null, date);
            }
            if(count != 0)
            {
                StockMarket.cache.Clear();
            }
        }

        public int updateBhavDataToLatest()
        {
            int count = 0;
            var date = getLastDate();
            //var count = DateTime.Now.Date.CompareTo(date);
            TimeSpan date_diff = DateTime.Now - date;
            for(int i = 1; i <= date_diff.Days; i++)
            {
                Logger.WriteLine($"UpdateBhavDataToLatest() updating date for {date.AddDays(i).Date}");
                count += updateBhavData(date.AddDays(i));
            }

            return count;
        }

        public DateTime getLastDate()
        {
            return dB.GetLastTradeDate();
        }

        public List<CompanyInfo> getCompanyList()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            if(StockMarket.cache.companyInfo.Count() == 0)
            {
                var result = dB.getCompanyList();
                StockMarket.cache.companyInfo = result;
            }
            sp.Stop();
            Logger.WriteLine($"getCompanyList() took {sp.Elapsed} seconds");
            return StockMarket.cache.companyInfo;
        }

        public List<DailyStockData> getLTP(int day = 0)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            if(StockMarket.cache.ltpData.Count() == 0) {
                var date = dB.GetLastTradeDate(day);
                var result = dB.GetLTP(date);
                StockMarket.cache.ltpData = result;
            }
            sp.Stop();
            Logger.WriteLine($"getLTP({day}) took {sp.Elapsed} seconds");
            return StockMarket.cache.ltpData;
        }

        public List<SectorChange> getSectorChange()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            if(StockMarket.cache.sectorChange.Count() == 0)
            {
                var result = dB.GetSectorChange(0);
                StockMarket.cache.sectorChange = result;
            }
            sp.Stop();
            Logger.WriteLine($"getSectorChange() took {sp.Elapsed} seconds");
            return StockMarket.cache.sectorChange;
        }


        public List<CircuitBreakerInfo> TodayCircuitBreaker()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var result = dB.GetCircuitBreaker(0);
            sp.Stop();
            Logger.WriteLine($"TodayCircuitBreaker() took {sp.Elapsed} seconds");
            return result;
        }

        public List<SectorStats> getSectorMonthlyStats()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            if(StockMarket.cache.sectorStats.Count() == 0)
            {
                var db = dB.GetSectorMonthlyStats();
                StockMarket.cache.sectorStats = db;
            }
            sp.Stop();
            Logger.WriteLine($"getSectorMonthlyStats() took {sp.Elapsed} seconds");
            return StockMarket.cache.sectorStats;
        }

        public void updateMonthlyStocks(int year)
        {
            var result = dB.GetStockMonthlyStats(year);
        }

        public List<StockMonthlyStats> getStockMonthlyStats(int year = 2017)
        {
            List<StockMonthlyStats> result = new List<StockMonthlyStats>();
            Stopwatch sp = new Stopwatch();

            sp.Start();
            if(DateTime.Now.Year == year)
            {
                // Fetch the data for the current year
                if(StockMarket.cache.monthlyStats.Count() == 0)
                {
                    result = dB.GetStockMonthlyStats(year);
                    StockMarket.cache.monthlyStats = result;
                }
                else {
                    result = StockMarket.cache.monthlyStats;
                }
            }
            else
            {
                result = dB.GetStockMonthlyStatsFromTable(year);
            }
            sp.Stop();
            Logger.WriteLine($"getStockMonthlyStats({year}) took {sp.Elapsed} seconds");
            return result;
        }

        public List<StockStats> GetStockStats()
        {
            Stopwatch sp = new Stopwatch();

            sp.Start();
            if(StockMarket.cache.stats.Count() == 0)
            {
                var list = dB.GetStockStats();
                StockMarket.cache.stats = list;
            }
            sp.Stop();
            Logger.WriteLine($"GetStockStats() took {sp.Elapsed} seconds");
            return StockMarket.cache.stats;
        }
        public List<StockHistory> GetStockHistory(string symbol, int days = 200)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var list = dB.GetStockHistory(symbol, days);
            sp.Stop();
            Logger.WriteLine($"GetStockStats({symbol}, {days}) took {sp.Elapsed} seconds");
            return list;
        }

        public List<StockHistory> GetStockHistory(string symbol, int year, int month)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var list = dB.GetStockHistory(symbol, year, month);
            sp.Stop();
            Logger.WriteLine($"GetStockStats({symbol}, {year}, {month}) took {sp.Elapsed} seconds");
            return list;
        }
    }
}