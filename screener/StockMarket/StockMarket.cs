using System;
using System.Linq;

using StockMarket;
using StockDataParser;

namespace screener
{
    public class StockMarket
    {
        StockDB dB;
        public StockMarket() {
            dB = new StockDB();
        }
        public void updateCompaniesList()
        {
            NseStockData nse = new NseStockData();
            var list = nse.getListOfCompanies();
            if(list == null) {
                Console.WriteLine("Could not get list of companies from NSE");
                Environment.Exit(1);
            }
            dB.AddCompaniesToList(list);
        }

        public void updateBhavData(DateTime date)
        {
            NseStockData nse = new NseStockData();
            var stockData = nse.updateBhavData(date);
            if(stockData != null)
                dB.AddDailyStockData(stockData, date);
        }

        public DateTime getLastDate()
        {
            return dB.GetLastUpdatedDate();
        }

        public void getLTP()
        {
            var list = dB.GetLTP().ToList();
            Console.WriteLine("Count : {0}", list.Count());
        }
    }
}