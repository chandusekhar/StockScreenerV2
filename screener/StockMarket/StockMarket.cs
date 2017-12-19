using System;
using System.Linq;

using StockMarket;
using StockDataParser;

namespace screener
{
    public class StockMarket
    {
        public StockMarket() {

        }
        public void updateCompaniesList()
        {
            StockDB dB = new StockDB();
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
            StockDB dB = new StockDB();
            NseStockData nse = new NseStockData();
            var stockData = nse.updateBhavData(date);
            if(stockData != null)
                dB.AddDailyStockData(stockData, date);
        }
    }
}