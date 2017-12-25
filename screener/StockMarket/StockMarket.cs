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
            return dB.getCompanyList();
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
            var count = DateTime.Now.Date.CompareTo(date);
            for(int i = 1; i <= count; i++)
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
            return dB.GetLTP(day);
        }

        public List<SectorChange> getSectorChange()
        {
            var result = dB.GetSectorChange(1).OrderBy(x => x.change).ToList();
            foreach(var item in result)
            {
                Console.WriteLine("{0}, {1}", item.sector, item.change);
            }
            return result;
        }

        public void getSectorMonthlyStats()
        {
            dB.GetSectorMonthlyStats();
        }

        public List<StockStats> GetStockStats()
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var list = dB.GetStockStats();
            sp.Stop();
            Console.WriteLine("Time taken: {0}", sp.ElapsedMilliseconds);
            return list;
        }
    }
}