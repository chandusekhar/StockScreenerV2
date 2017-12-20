using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

/*
    Help: https://docs.microsoft.com/en-us/ef/core/get-started/netcore/new-db-sqlite
    dotnet ef migrations add InitialCreate &&  dotnet ef database update
 */

namespace StockMarket
{
    public class ConstValues
    {
        public const string defaultUser = "user";
        public const string defaultIndustry = "noIndustry";
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

    public class StockDataContext : DbContext
    {
        public DbSet<CompanyInformation> companyInformation { get; set; }
        public DbSet<WatchList> watchList { get; set; }
        public DbSet<PortfolioInformation> portfolioInformation { get; set; }
        public DbSet<DailyStockData> stockData { get; set; }
        public DbSet<CircuitBreaker> circuitBreaker { get; set; }
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

            // Set the index for faster query operations
            modelBuilder.Entity<WatchList>().HasIndex(x => new { x.isinNumber, x.series, x.symbol });
            modelBuilder.Entity<PortfolioInformation>().HasIndex(x => new { x.isinNumber, x.series, x.symbol });
            modelBuilder.Entity<CompanyInformation>().HasIndex(x => new { x.symbol, x.isinNumber, x.series });
            modelBuilder.Entity<DailyStockData>().HasIndex(x => new { x.date, x.symbol, x.isinNumber, x.series });
            modelBuilder.Entity<DailyStockData>().HasIndex(x => new { x.symbol, x.series });
            modelBuilder.Entity<DailyStockData>().HasIndex(x => x.date);
            modelBuilder.Entity<CircuitBreaker>().HasIndex(x => x.date);
        }
    }
}