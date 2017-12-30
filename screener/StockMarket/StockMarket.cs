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

        public CachedResult()
        {
            ltpData = new List<DailyStockData>();
            stats = new List<StockStats>();
            sectorChange = new List<SectorChange>();
        }

        public void Clear() {
            ltpData.Clear();
            stats.Clear();
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
            dB.AddCompaniesToList(list);
        }

        public List<CompanyInfo> getCompanyList()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var result = dB.getCompanyList();
            sp.Stop();
            Console.WriteLine("getCompanyList() took {0} seconds", sp.Elapsed);
            return result;
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
                StockMarket.cache.ltpData.Clear();
            }
            return;
        }

        public void updateBhavDataToLatest()
        {
            var date = getLastDate();
            //var count = DateTime.Now.Date.CompareTo(date);
            TimeSpan count = DateTime.Now - date;
            for(int i = 1; i <= count.Days; i++)
            {
                Console.WriteLine("Updating data for {0}", date.AddDays(i).Date);
                updateBhavData(date.AddDays(i));
            }

            return;
        }

        public DateTime getLastDate()
        {
            return dB.GetLastTradeDate();
        }

        public List<DailyStockData> getLTP(int day = 0)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            if(StockMarket.cache.ltpData.Count == 0) {
                var date = dB.GetLastTradeDate(day);
                var result = dB.GetLTP(date);
                StockMarket.cache.ltpData = result;
            }
            sp.Stop();
            Console.WriteLine("getLTP() took {0} seconds", sp.Elapsed);
            return StockMarket.cache.ltpData;
        }

        public List<SectorChange> getSectorChange()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            if(StockMarket.cache.sectorChange.Count() == 0)
            {
                var result = dB.GetSectorChange(0).OrderByDescending(x => x.change).ToList();
                StockMarket.cache.sectorChange = result;
            }
            sp.Stop();
            Console.WriteLine("getSectorChange() took {0} seconds", sp.Elapsed);
            return StockMarket.cache.sectorChange;
        }

        public void getSectorMonthlyStats()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            dB.GetSectorMonthlyStats();
            sp.Stop();
            Console.WriteLine("getSectorMonthlyStats() took {0} seconds", sp.Elapsed);
        }

        public List<StockStats> GetStockStats()
        {
            Stopwatch sp = new Stopwatch();

            sp.Start();
            if(StockMarket.cache.stats.Count == 0)
            {
                var list = dB.GetStockStats();
                StockMarket.cache.stats = list;
            }
            sp.Stop();
            Console.WriteLine("GetStockStats() took {0} seconds", sp.Elapsed);
            return StockMarket.cache.stats;
        }
        public List<StockHistory> GetStockHistory(string symbol, int days = 100)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var list = dB.GetStockHistory(symbol, days);
            sp.Stop();
            Console.WriteLine("GetStockHistory() took {0} seconds", sp.Elapsed);
            return list;
        }
    }
}