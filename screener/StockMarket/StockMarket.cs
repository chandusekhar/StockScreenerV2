using System;
using System.Linq;

using StockMarket;
using StockDataParser;
using System.Diagnostics;

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

        public void updateBhavData(DateTime date)
        {
            NseStockData nse = new NseStockData();
            var stockData = nse.updateBhavData(date);
            if (stockData != null)
                dB.AddDailyStockData(stockData, date);
        }

        public DateTime getLastDate()
        {
            return dB.GetLastTradeDate();
        }

        public void getLTP()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var list = dB.GetLTP();
            sw.Stop();

            foreach (var item in list)
            {
                Console.WriteLine("{0} -> {1} -> {2}", item.symbol, item.industry, item.change);
            }
            Console.WriteLine("Count : {0}, Elapsed : {1}", list.Count(), sw.Elapsed);
        }

        public void getIndustryChange()
        {
            dB.GetIndustyChange();
        }
    }
}