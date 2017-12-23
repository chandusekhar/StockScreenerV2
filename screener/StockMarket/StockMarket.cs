using System;
using System.Linq;

using StockMarket;
using StockDataParser;
using System.Diagnostics;
using System.Collections.Generic;

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

        public void GetStockStats()
        {
            var list = dB.GetStockStats();
            foreach(var item in list)
            {
                Console.Write("{0} {1}", item.symbol, item.series);
                {
                    int count = 0;
                    foreach(var i in item.avgPriceChange)
                        Console.Write(" {0}/{1} ", i, item.avgVolumeChage[count++]);
                }
                Console.WriteLine("");
            }
        }
    }
}