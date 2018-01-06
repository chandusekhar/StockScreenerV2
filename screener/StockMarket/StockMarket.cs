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

        public CachedResult()
        {
            ltpData = new List<DailyStockData>();
            stats = new List<StockStats>();
            sectorChange = new List<SectorChange>();
            companyInfo = new List<CompanyInfo>();
        }

        public void Clear() {
            ltpData.Clear();
            stats.Clear();
            sectorChange.Clear();
            companyInfo.Clear();
        }
    }


    class Logger
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

        public void updateBhavData(DateTime date)
        {
            int  count = 0;
            NseStockData nse = new NseStockData();
            var stockData = nse.updateBhavData(date);
            if (stockData != null)
            {
                count += dB.AddDailyStockData(stockData, date);
            }
            if(count != 0)
            {
                StockMarket.cache.Clear();
            }
            return;
        }

        public void updateBhavData(string bhavFile, string mtoFile)
        {
            int count = 0;
            var date = DateTime.Parse("01/01/2018");
            NseStockData nse = new NseStockData();
            var stockData = nse.updateBhavData(bhavFile, mtoFile);
            if (stockData != null)
            {
                count += dB.AddDailyStockData(stockData, date);
            }
            if(count != 0)
            {
                StockMarket.cache.Clear();
            }
        }

        public void updateBhavDataToLatest()
        {
            var date = getLastDate();
            //var count = DateTime.Now.Date.CompareTo(date);
            TimeSpan count = DateTime.Now - date;
            for(int i = 1; i <= count.Days; i++)
            {
                Logger.WriteLine($"UpdateBhavDataToLatest() updating date for {date.AddDays(i).Date}");
                updateBhavData(date.AddDays(i));
            }

            return;
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
            Logger.WriteLine($"getLTP() took {sp.Elapsed} seconds");
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

        public void getSectorMonthlyStats()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var db = dB.GetSectorMonthlyStats();
            foreach(var item in db)
            {
                Console.WriteLine("Sector: {0}", item.industry);
                foreach(var i in item.stats)
                {
                    Console.Write(" {0}/M>{1} ", i.change, i.month);
                }
                Console.WriteLine("");
            }
            sp.Stop();
            Logger.WriteLine($"getSectorMonthlyStats() took {sp.Elapsed} seconds");
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
            Logger.WriteLine($"GetStockStats({symbol}) took {sp.Elapsed} seconds");
            return list;
        }
    }
}