using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

/*
    Help: https://docs.microsoft.com/en-us/ef/core/get-started/netcore/new-db-sqlite
    dotnet ef migrations add InitialCreate &&  dotnet ef database update
 */

namespace StockDatabase
{
    public class ConstValues
    {
        public const string defaultUser = "user";
        public const string defaultIndustry = "noIndustry";
    }

    public class SectorInformation
    {
        [Required]
        public DateTime date { get; set; }
        [Required]
        public string industry { get; set; }
        [Required]
        public decimal change { get; set; }

        public SectorInformation(DateTime date, string industry, decimal change)
        {
            this.date = date;
            this.industry = industry;
            if(this.industry == "")
            {
                this.industry = ConstValues.defaultIndustry;
            }
            this.change = decimal.Round(change, 2);
        }

        public SectorInformation()
        {

        }
    }
    public class CompanyInformation
    {
        [Required]
        public string symbol { get; set; }
        [Required]
        public string series { get; set; }
        [Required]
        public string isinNumber { get; set; }
        [Required]
        public string companyName { get; set; }
        public DateTime dateOfListing { get; set; }
        public decimal paidUpvalue { get; set; }
        public int marketLot { get; set; }
        [Required]
        public decimal faceValue { get; set; }
        public string industry { get; set; }

        public CompanyInformation()
        {
            this.industry = ConstValues.defaultIndustry;
        }
    }

    public class WatchList
    {
        [Required]
        public string symbol { get; set; }
        [Required]
        public string isinNumber { get; set; }
        [Required]
        public string series { get; set; }
        public string notes { get; set; }
        [Required]
        public string userName { get; set; }

        public WatchList()
        {
            this.userName = ConstValues.defaultUser;
            this.notes = "";
        }
    }

    public class PortfolioInformation
    {
        [Required]
        public string symbol { get; set; }
        [Required]
        public string isinNumber { get; set; }
        [Required]
        public string series { get; set; }
        [Required]
        public DateTime buyDate { get; set; }
        [Required]
        public decimal buyPrice { get; set; }
        public string notes { get; set; }
        [Required]
        public string userName { get; set; }

        public PortfolioInformation()
        {
            this.userName = ConstValues.defaultUser;
            this.notes = "";
        }
    }

    public class DailyStockData
    {
        [NotMapped]
        public string industry { get; set; }

        [NotMapped]
        public decimal change
        {
            get
            {
                return Math.Round(100 * (lastPrice - prevClose) / prevClose, 2);
            }
        }
        [Required]
        public string symbol { get; set; }
        [Required]
        public string series { get; set; }
        [Required]
        public decimal open { get; set; }
        [Required]
        public decimal high { get; set; }
        [Required]
        public decimal low { get; set; }
        [Required]
        public decimal close { get; set; }
        [Required]
        public decimal lastPrice { get; set; }
        [Required]
        public decimal prevClose { get; set; }
        [Required]
        public long totalTradedQty { get; set; }
        [Required]
        public decimal totalTradedValue { get; set; }
        [Required]
        public DateTime date { get; set; }
        [Required]
        public long totalTrades { get; set; }
        [Required]
        public string isinNumber { get; set; }
        [Required]
        public long deliverableQty { get; set; }
        [Required]
        public decimal deliveryPercentage { get; set; }
    }

    public class CircuitBreaker
    {
        [Required]
        public string nseSymbol { get; set; }
        [Required]
        public string series { get; set; }
        [Required]
        public char high_low { get; set; }
        [Required]
        public DateTime date { get; set; }
    }

    public class StockMonthlyStats
    {
        [Required]
        public int year { get; set;}
        [Required]
        public string symbol { get; set; }
        [NotMapped]
        public decimal[] change
        {
            get
            {
                return Array.ConvertAll(InternalData.Split(';'), Decimal.Parse);
            }
            set
            {
                var _data = Array.ConvertAll<decimal, string>(value, Convert.ToString);
                InternalData = String.Join(";", _data);
            }
        }

        [Required]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string InternalData { get; set; }

        public StockMonthlyStats()
        {
            change = new decimal[12];
        }

        public StockMonthlyStats(string symbol, int year) {
            change = new decimal[12];
            this.year = year;
            this.symbol = symbol;
        }
    }

    public class StockDataContext : DbContext
    {
        public DbSet<CompanyInformation> companyInformation { get; set; }
        public DbSet<WatchList> watchList { get; set; }
        public DbSet<PortfolioInformation> portfolioInformation { get; set; }
        public DbSet<DailyStockData> stockData { get; set; }
        public DbSet<CircuitBreaker> circuitBreaker { get; set; }
        public DbSet<SectorInformation> sectorInformation { get; set;}
        public DbSet<StockMonthlyStats> monthlyStockStats {get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=data.db");
            optionsBuilder.EnableSensitiveDataLogging(true);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WatchList>().HasKey(x => new { x.isinNumber, x.series, x.symbol });
            modelBuilder.Entity<CompanyInformation>().HasKey(x => new { x.series, x.isinNumber, x.symbol });
            modelBuilder.Entity<PortfolioInformation>().HasKey(x => new { x.isinNumber, x.series, x.symbol });
            modelBuilder.Entity<DailyStockData>().HasKey(x => new { x.isinNumber, x.date, x.series });
            modelBuilder.Entity<CircuitBreaker>().HasKey(x => new { x.nseSymbol, x.date, x.series });
            modelBuilder.Entity<SectorInformation>().HasKey( x=> new {x.date, x.industry});
            modelBuilder.Entity<StockMonthlyStats>().HasKey(x => new {x.symbol, x.year});

            // Set the index for faster query operations
            modelBuilder.Entity<WatchList>().HasIndex(x => new { x.isinNumber, x.series, x.symbol });
            modelBuilder.Entity<PortfolioInformation>().HasIndex(x => new { x.isinNumber, x.series, x.symbol });
            modelBuilder.Entity<CompanyInformation>().HasIndex(x => new { x.symbol, x.isinNumber, x.series });
            modelBuilder.Entity<DailyStockData>().HasIndex(x => new { x.date, x.symbol, x.isinNumber, x.series });
            modelBuilder.Entity<DailyStockData>().HasIndex(x => new { x.symbol, x.series });
            modelBuilder.Entity<DailyStockData>().HasIndex(x => x.date);
            modelBuilder.Entity<SectorInformation>().HasIndex(x => x.date);
            modelBuilder.Entity<SectorInformation>().HasIndex(x => x.industry);
            modelBuilder.Entity<CircuitBreaker>().HasIndex(x => x.date);
            modelBuilder.Entity<StockMonthlyStats>().HasIndex(x => x.symbol);
            modelBuilder.Entity<StockMonthlyStats>().HasIndex(x => x.year);
        }
    }
}