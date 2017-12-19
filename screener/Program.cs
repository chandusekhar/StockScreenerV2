using System;
using System.Linq;

using StockMarket;
using StockDataParser;

namespace screener
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            NseStockData nse = new NseStockData();
            var list = nse.getListOfCompanies();
            if(list == null) {
                Console.WriteLine("parseFileFailed\n");
                Environment.Exit(1);
            }
            using(var db = new StockDataContext())
            {
                foreach(var item in list)
                {
                    db.companyInformation.Add(item);
                }
                Console.WriteLine("Added {0} companies to db", db.SaveChanges());
            }
            return;
        }
    }
}
