using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;

using TinyCsvParser;
using TinyCsvParser.Mapping;

using StockMarket;

namespace StockDataParser
{
    class CsvCompanyInformationMapping : CsvMapping<CompanyInformation>
    {
        public CsvCompanyInformationMapping() : base()
        {
            MapProperty(0, x => x.symbol);
            MapProperty(1, x => x.companyName);
            MapProperty(2, x => x.series);
            MapProperty(3, x => x.dateOfListing);
            MapProperty(4, x => x.paidUpvalue);
            MapProperty(5, x => x.marketLot);
            MapProperty(6, x => x.isinNumber);
            MapProperty(7, x => x.faceValue);
        }
    }

    public class NseStockData
    {
        public NseStockData()
        {

        }

        public IEnumerable<CompanyInformation> getListOfCompanies()
        {
            try
            {
                const string listOfCompanies_link = @"https://www.nseindia.com/content/equities/EQUITY_L.csv";
                string filename = Path.GetTempFileName();

                Console.WriteLine("Filename : {0}", filename);

                // Download the file
                WebClient client = new WebClient();
                client.Headers["User-Agent"] = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US; rv:1.9.2.15) Gecko/20110303 Firefox/3.6.15";
                client.DownloadFile(listOfCompanies_link, filename);

                // Set CSV file parsing options
                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
                CsvCompanyInformationMapping csvMapper = new CsvCompanyInformationMapping();
                CsvParser<CompanyInformation> csvParser = new CsvParser<CompanyInformation>(csvParserOptions, csvMapper);

                // Parse the CSV file
                var result = csvParser
                    .ReadFromFile(filename, Encoding.ASCII)
                    .Select(x => x.Result);

                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
                Environment.Exit(1);
            }
            return null;
        }
    }
}