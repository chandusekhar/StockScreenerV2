using System;
using System.Linq;

using StockMarket;
using StockDataParser;

namespace screener
{
    class Program
    {
        void UpdateStockDataFor1Year()
        {
            StockMarket sm = new StockMarket();
            DateTime date = DateTime.ParseExact("01-01-2017", "dd-MM-yyyy", null);
            for(int i = 0; i < 365; i++)
            {
                sm.updateBhavData(date.AddDays(i));
            }
        }
        static void Main(string[] args)
        {
            StockMarket sm = new StockMarket();
            sm.getLTP();

            return;
        }
    }
}
