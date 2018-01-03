using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;

using TinyCsvParser;
using TinyCsvParser.Mapping;

using StockDatabase;


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

    class CsvDailyStockDataMapping : CsvMapping<DailyStockData>
    {
        public CsvDailyStockDataMapping() : base()
        {
            MapProperty(0, x => x.symbol);
            MapProperty(1, x => x.series);
            MapProperty(2, x => x.open);
            MapProperty(3, x => x.high);
            MapProperty(4, x => x.low);
            MapProperty(5, x => x.close);
            MapProperty(6, x => x.lastPrice);
            MapProperty(7, x => x.prevClose);
            MapProperty(8, x => x.totalTradedQty);
            MapProperty(9, x => x.totalTradedValue);
            MapProperty(10, x => x.date);
            MapProperty(11, x => x.totalTrades);
            MapProperty(12, x => x.isinNumber);
        }
    }

    class DailyStockDeliveryPosition
    {
        public int record { get; set; }
        public int serialNumber { get; set; }
        public string symbol { get; set; }
        public string series { get; set; }
        public long qtyTraded { get; set; }
        public long deliverableQty { get; set; }
        public decimal deliveryPercentage { get; set; }
    }

    class CsvDailyStockDeviveryPositionMapping : CsvMapping<DailyStockDeliveryPosition>
    {
        public CsvDailyStockDeviveryPositionMapping() : base()
        {
            MapProperty(0, x => x.record);
            MapProperty(1, x => x.serialNumber);
            MapProperty(2, x => x.symbol);
            MapProperty(3, x => x.series);
            MapProperty(4, x => x.qtyTraded);
            MapProperty(5, x => x.deliverableQty);
            MapProperty(6, x => x.deliveryPercentage);
        }
    }

    class IsinToIndustry
    {
        public string isinNumber { get; set; }
        public string industry { get; set; }
    }

    class CsvIsinToIndustry : CsvMapping<IsinToIndustry>
    {
        public CsvIsinToIndustry() : base()
        {
            MapProperty(6, x => x.isinNumber);
            MapProperty(7, x => x.industry);
        }
    }

    class FileDownloader
    {
        public FileDownloader()
        {

        }

        public static string downloadFile(string url)
        {
            try
            {
                // Create a tmp file
                string filename = Path.GetTempFileName();

                // Download the file
                WebClient client = new WebClient();
                client.Headers["User-Agent"] = "curl/7.56.1";
                client.Headers["Host"] = "www.nseindia.com";
                client.DownloadFile(url, filename);
                return filename;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}\nExceptionURl: {1}", ex.Message, url);
            }
            return null;
        }

    }

    public class NseStockData
    {
        const string listOfCompaniesInNSE = @"https://www.nseindia.com/content/equities/EQUITY_L.csv";
        public NseStockData()
        {

        }

        private (bool status, string bhavFile, string mtoFile) downloadBhavAndMTOFile(DateTime date)
        {
            String tmpFolder = Path.GetTempPath();
            // Build the bhav Url
            string bhavUrl = string.Format("https://www.nseindia.com/content/historical/EQUITIES/{0}/{1}/cm{2}bhav.csv.zip",
                                            date.ToString("yyyy"), date.ToString("MMM").ToUpper(), date.ToString("ddMMMyyyy").ToUpper());

            // build the deliverables URL
            string deliveryPositionsUrl = string.Format("https://www.nseindia.com/archives/equities/mto/MTO_{0}{1}{2}.DAT",
                                                         date.ToString("dd"), date.ToString("MM"), date.ToString("yyyy"));

            Console.WriteLine("Bhav URL : {0}\nDelivery URL: {1}", bhavUrl, deliveryPositionsUrl);

            // Download bhav and delivery file
            string bhavFile = FileDownloader.downloadFile(bhavUrl);
            string deliveryFile = FileDownloader.downloadFile(deliveryPositionsUrl);
            if (bhavFile == null || deliveryFile == null)
            {
                Console.WriteLine("Could not update daily stock price for date {0}", date.ToString("dd/MM/yyyy"));
                return (false, null, null);
            }

            //Check the size of bhav file are more than 1k to make sure we have
            //downloaded the correct file
            var bhav = new FileInfo(bhavFile).Length;
            var delivery = new FileInfo(deliveryFile).Length;
            if (bhav < 1000 || delivery < 1000)
            {
                Console.WriteLine("Could not update daily stock price for date {0}", date.ToString("dd/MM/yyyy"));
                return (false, null, null);
            }

            // unzip the bhav file
            //string bhavFileUnzipped = string.Format("{0}/cm{1}bhav.csv", tmpFolder, date.ToString("ddMMMyyyy").ToUpper());
            string bhavFileUnzipped = Path.Combine(tmpFolder, string.Format("cm{1}bhav.csv", date.ToString("ddMMMyyyy").ToUpper()));
            if (File.Exists(bhavFileUnzipped)) File.Delete(bhavFileUnzipped);
            ZipFile.ExtractToDirectory(bhavFile, tmpFolder);

            // Return the unzipped the bhav file
            return (true, bhavFileUnzipped, deliveryFile);
        }

        // Parse the MTO csv file
        private IEnumerable<DailyStockDeliveryPosition> parseMTOFile(string csvFile)
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvDailyStockDeviveryPositionMapping csvMapper = new CsvDailyStockDeviveryPositionMapping();
            CsvParser<DailyStockDeliveryPosition> csvParser = new CsvParser<DailyStockDeliveryPosition>(csvParserOptions, csvMapper);

            var result = csvParser.ReadFromFile(csvFile, Encoding.ASCII)
                                  .Skip(3)  // Skip the first four lines as they contain only header
                                  .Select(x => x.Result);

            return result;
        }

        //Parse the Bhav csv file
        private  IEnumerable<DailyStockData> parseBhavFile(string csvFile)
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvDailyStockDataMapping csvMapper = new CsvDailyStockDataMapping();
            CsvParser<DailyStockData> csvParser = new CsvParser<DailyStockData>(csvParserOptions, csvMapper);

            var result = csvParser.ReadFromFile(csvFile, Encoding.ASCII)
                                  .Select(x => x.Result);

            return result;
        }

        // Download and update the bhav data in DB for a given date
        public List<DailyStockData> updateBhavData(DateTime date)
        {
            var files = downloadBhavAndMTOFile(date);
            if (files.status == false)
                return null;
            var stockData = parseBhavFile(files.bhavFile).ToList();
            var deliveryData = parseMTOFile(files.mtoFile).ToList();

            Console.WriteLine("{0}, {1}", files.bhavFile, files.mtoFile);;
            foreach (var stock in stockData)
            {
                if (stock.series == "BE")
                {
                    // In BE series all the trades will be delivered
                    stock.deliverableQty = stock.totalTradedQty;
                    stock.deliveryPercentage = 100;
                }
                else
                {
                    // Fill the deliveryQty and % for every stock
                    var result = deliveryData.Where(x => x.series == stock.series && x.symbol == stock.symbol && x.qtyTraded == stock.totalTradedQty)
                                             .ToList();
                    if (result.Count() > 0)
                    {
                        var res = result.First();
                        stock.deliverableQty = res.deliverableQty;
                        stock.deliveryPercentage = res.deliveryPercentage;
                    }
                }
            }
            return stockData;
        }

        public List<DailyStockData> updateBhavData(string bhavFile, string mtoFile)
        {
            var stockData = parseBhavFile(bhavFile).ToList();
            var deliveryData = parseMTOFile(mtoFile).ToList();

            Console.WriteLine("{0}, {1}", bhavFile, mtoFile);;
            foreach (var stock in stockData)
            {
                if (stock.series == "BE")
                {
                    // In BE series all the trades will be delivered
                    stock.deliverableQty = stock.totalTradedQty;
                    stock.deliveryPercentage = 100;
                }
                else
                {
                    // Fill the deliveryQty and % for every stock
                    var result = deliveryData.Where(x => x.series == stock.series && x.symbol == stock.symbol && x.qtyTraded == stock.totalTradedQty)
                                             .ToList();
                    if (result.Count() > 0)
                    {
                        var res = result.First();
                        stock.deliverableQty = res.deliverableQty;
                        stock.deliveryPercentage = res.deliveryPercentage;
                    }
                }
            }
            return stockData;
        }

        Dictionary<string, string> getIndustryList()
        {
            CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
            CsvIsinToIndustry csvMapper = new CsvIsinToIndustry();
            CsvParser<IsinToIndustry> csvParser = new CsvParser<IsinToIndustry>(csvParserOptions, csvMapper);

            try
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                csvParser.ReadFromFile(@"./data/ListOfScrips.csv", Encoding.ASCII)
                         .Select(x => x.Result)
                         .ToList()
                         .ForEach(x => dict.TryAdd(x.isinNumber, x.industry));

                return dict;
            }
            catch(Exception ex)
            {
                Console.WriteLine("{0}", ex.Message);
            }
            return null;

        }

        public List<CompanyInformation> getListOfCompanies()
        {
            try
            {
                string filename = FileDownloader.downloadFile(listOfCompaniesInNSE);
                if (filename == null)
                {
                    Console.WriteLine("Unable to download file {0}", listOfCompaniesInNSE);
                    return null;
                }

                // Parse CSV file
                CsvParserOptions csvParserOptions = new CsvParserOptions(true, ',');
                CsvCompanyInformationMapping csvMapper = new CsvCompanyInformationMapping();
                CsvParser<CompanyInformation> csvParser = new CsvParser<CompanyInformation>(csvParserOptions, csvMapper);

                // Parse the CSV file
                var result = csvParser.ReadFromFile(filename, Encoding.ASCII)
                                      .Select(x => x.Result)
                                      .ToList();

                var mapping = getIndustryList();
                result.ForEach(x => x.industry =  mapping.TryGetValue(x.isinNumber, out string industry) ? industry : ConstValues.defaultIndustry);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
                Environment.Exit(1);
            }
            return null;
        }
    }
}