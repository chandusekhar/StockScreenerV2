using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace StockMarket
{
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
        [DefaultValue("NoIndustry")]
        public string industry { get; set; }
    }

    public class WatchList
    {
        [Required]
        public string symbol { get; set; }
        [Required]
        public string isinNumber { get; set; }
        [Required]
        public string series { get; set; }
        [DefaultValue("")]
        public string notes { get; set; }
        [DefaultValue(null)]
        public string userName { get; set; }
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
        [DefaultValue("")]
        public string notes { get; set; }
        [DefaultValue(null)]
        public string userName { get; set; }
    }

    public class DailyStockData
    {
        [Required]
        public string symbol { get; set; }
        [Required]
        public string series { get; set; }
        [Required]
        public float open { get; set; }
        [Required]
        public float high { get; set; }
        [Required]
        public float low { get; set; }
        [Required]
        public float close { get; set; }
        [Required]
        public float lastPrice { get; set; }
        [Required]
        public float prevClose { get; set; }
        [Required]
        public long totalTradedQty { get; set; }
        [Required]
        public float totalTradedValue { get; set; }
        [Required]
        public DateTime date { get; set; }
        [Required]
        public long totalTrades { get; set; }
        [Required]
        public string isinNumber { get; set; }
        [Required]
        public long deliverableQty { get; set; }
        [Required]
        public float deliveryPercentage { get; set; }
    }

    public class CircuitBreaker
    {
        [Required]
        public string nseSymbol { get; set; }
        [Required]
        public string series { get; set; }
        [Required]
        public char high_low { get; set; }
        [Key]
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

            // Set the index for faster query operations
            modelBuilder.Entity<WatchList>().HasIndex(x => new { x.isinNumber, x.series, x.symbol });
            modelBuilder.Entity<PortfolioInformation>().HasIndex(x => new { x.isinNumber, x.series, x.symbol });
            modelBuilder.Entity<CompanyInformation>().HasIndex(x => new { x.symbol, x.isinNumber, x.series });
            modelBuilder.Entity<DailyStockData>().HasIndex(x => new { x.date, x.symbol, x.isinNumber, x.series });
            modelBuilder.Entity<CircuitBreaker>().HasIndex(x => x.date);
        }
    }
}