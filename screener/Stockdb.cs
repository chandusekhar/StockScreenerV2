using System;
using System.Linq;

using StockMarket;
using StockDataParser;
using System.Collections.Generic;

namespace screener
{
    public class StockDB
    {
        public StockDB()
        {

        }

        public int AddCompaniesToList(IEnumerable<CompanyInformation> list)
        {
            using(var db = new StockDataContext())
            {
                foreach(var item in list)
                {
                    db.companyInformation.Add(item);
                }
                var count = db.SaveChanges();
                Console.WriteLine("Added {0} companies to db", count);
                return count;
            }
        }

        public int AddDailyStockData(IEnumerable<DailyStockData> list, DateTime date)
        {
            using(var db = new StockDataContext())
            {
                foreach(var item in list)
                {
                    db.stockData.Add(item);
                }
                var count = db.SaveChanges();
                Console.WriteLine("Added {0} companies Stock Data for {1} to db", count, date.ToString());
                return count;
            }
        }

        public DateTime GetLastUpdatedDate()
        {
            using(var db = new StockDataContext())
            {
                return db.stockData.Select(x => x.date).OrderByDescending(x => x).FirstOrDefault();
            }
        }

        public List<DailyStockData> GetLTP()
        {
            var date = GetLastUpdatedDate();
            using(var db = new StockDataContext())
            {
                return db.stockData.Where(x => x.date.CompareTo(date) == 0).Select(x => x).ToList();
            }
        }
    }
}