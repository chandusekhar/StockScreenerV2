using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

using StockDataParser;
using StockDatabase;

namespace screener
{
    public class StockMarket
    {
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
            NseStockData nse = new NseStockData();
            var stockData = nse.updateBhavData(date);
            if (stockData != null)
                dB.AddDailyStockData(stockData, date);
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
            var date = dB.GetLastTradeDate(day);
            var result = dB.GetLTP(date);
            sp.Stop();
            Console.WriteLine("getLTP() took {0} seconds", sp.Elapsed);
            return result;
        }

        public List<SectorChange> getSectorChange()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var result = dB.GetSectorChange(0).OrderByDescending(x => x.change).ToList();
            sp.Stop();
            Console.WriteLine("getSectorChange() took {0} seconds", sp.Elapsed);
            return result;
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
            var list = dB.GetStockStats();
            sp.Stop();
            Console.WriteLine("GetStockStats() took {0} seconds", sp.Elapsed);
            return list;
        }
    }
}