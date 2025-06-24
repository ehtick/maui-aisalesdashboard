﻿namespace AISalesDashboard
{
    public class SalesDataService
    {
        private List<SalesData>? _cachedSalesData;
        private List<Product>? _products;
        private List<Region>? _regions;

        public SalesDataService()
        {
            LoadSampleData();
        }

        #region Methods

        private void LoadSampleData()
        {
            // In a real app, this would come from a database or API
            _products =
            [
                new Product { Id = "P001", Name = "iPhone 5S", Category = "Electronics", BasePrice = 1200m, IsActive = true, Icon="iphone_4k.png" },
                new Product { Id = "P002", Name = "Smart TV", Category = "Electronics", BasePrice = 1299.99m, IsActive = true, Icon="smart_tv.png" },
                new Product { Id = "P002", Name = "Laptop", Category = "Electronics", BasePrice = 1599.99m, IsActive = true, Icon="laptop.png" },
                new Product { Id = "P003", Name = "Wireless Headphones", Category = "Audio", BasePrice = 199.99m, IsActive = true, Icon="wireless_earbuds.png" },
                new Product { Id = "P004", Name = "iPhone 4", Category = "Wearables", BasePrice = 249.99m, IsActive = true, Icon="iphone_4k.png" },
                new Product { Id = "P005", Name = "Tablet Ultra", Category = "Electronics", BasePrice = 599.99m, IsActive = true, Icon="tablet.png" },
                new Product { Id = "P005", Name = "Gaming Mouse", Category = "Electronics", BasePrice = 399.99m, IsActive = true, Icon="gaming_mouse.png" },
            ];

            _regions =
            [
                new Region { Id = "R001", Name = "North America", Country = "USA", Latitude = 38.7946, Longitude = -106.5348 },
                new Region { Id = "R002", Name = "Europe", Country = "Germany", Latitude = 51.1657, Longitude = 10.4515 },
                new Region { Id = "R003", Name = "Asia Pacific", Country = "China", Latitude = 35.8617, Longitude = 104.1954 },
                new Region { Id = "R004", Name = "Latin America", Country = "Brazil", Latitude = -14.2350, Longitude = -51.9253 }
            ];

            // Generate 2 years of sample data
            _cachedSalesData = GenerateSampleData();
        }

        private List<SalesData> GenerateSampleData()
        {
            var random = new Random(123); // Fixed seed for reproducibility
            var startDate = DateTime.Now.AddYears(-2);
            var endDate = DateTime.Now;
            var salesData = new List<SalesData>();

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                foreach (var product in _products!)
                {
                    foreach (var region in _regions!)
                    {
                        var baseQuantity = CalculateBaseQuantity(product.Id);
                        var seasonalFactor = CalculateSeasonalFactor(product.Category, date.Month);
                        var regionalFactor = CalculateRegionalFactor(region.Id);
                        var dayOfWeekFactor = CalculateDayOfWeekFactor(date.DayOfWeek);
                        var trendFactor = CalculateTrendFactor(product.Id, (date - startDate).Days);
                        var randomFactor = 0.8 + random.NextDouble() * 0.4;

                        var quantity = (int)(baseQuantity * seasonalFactor * regionalFactor * dayOfWeekFactor * trendFactor * randomFactor);
                        var unitPrice = product.BasePrice * (0.95m + (decimal)random.NextDouble() * 0.1m);
                        var revenue = quantity * unitPrice;
                        var cost = revenue * 0.6m;

                        salesData.Add(new SalesData
                        {
                            Date = date,
                            ProductId = product.Id,
                            ProductName = product.Name,
                            RegionId = region.Id,
                            RegionName = region.Name,
                            Revenue = revenue,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            Cost = cost,
                            Image = product.Icon,
                        });

                        if (random.NextDouble() > 0.8)
                            break;
                    }
                }
            }

            return salesData;
        }

        private int CalculateBaseQuantity(string productId) => productId switch
        {
            "P001" => 100,
            "P002" => 50,
            "P003" => 200,
            "P004" => 75,
            "P005" => 60,
            _ => 50
        };

        private double CalculateSeasonalFactor(string category, int month) => category switch
        {
            "Electronics" => month is 11 or 12 ? 1.5 : 1.0,
            "Wearables" => month is 1 or 6 ? 1.3 : 1.0,
            _ => 1.0
        };

        private double CalculateRegionalFactor(string regionId) => regionId switch
        {
            "R001" => 1.2,
            "R002" => 1.1,
            "R003" => 1.3,
            "R004" => 0.9,
            _ => 1.0
        };

        private double CalculateDayOfWeekFactor(DayOfWeek dayOfWeek) => dayOfWeek switch
        {
            DayOfWeek.Saturday => 1.2,
            DayOfWeek.Sunday => 0.8,
            _ => 1.0
        };

        private double CalculateTrendFactor(string productId, int daysSinceStart) => productId switch
        {
            "P001" => Math.Min(1.5, 1.0 + daysSinceStart * 0.0005),
            "P004" => Math.Min(1.8, 1.0 + daysSinceStart * 0.0008),
            "P002" => daysSinceStart > 365 ? 0.7 : 1.0,
            _ => 1.0
        };

        public async Task<List<SalesData>> GetSalesDataAsync(DateRange dateRange)
        {
            await Task.Delay(10);
            return _cachedSalesData!
                .Where(x => x.Date >= dateRange.StartDate && x.Date <= dateRange.EndDate)
                .ToList();
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            await Task.Delay(10);
            return _products!.ToList();
        }

        public async Task<List<Region>> GetRegionsAsync()
        {
            await Task.Delay(10);
            return _regions!.ToList();
        }

        public async Task<Dictionary<string, decimal>> GetSalesSummaryAsync(DateRange dateRange)
        {
            var salesData = await GetSalesDataAsync(dateRange);

            var totalRevenue = salesData.Sum(x => x.Revenue);
            var totalCost = salesData.Sum(x => x.Cost);
            var totalProfit = totalRevenue - totalCost;
            var avgProfitMargin = totalRevenue > 0 ? totalProfit / totalRevenue * 100 : 0;
            var totalQuantity = salesData.Sum(x => x.Quantity);

            var previousPeriodLength = (dateRange.EndDate - dateRange.StartDate).TotalDays;
            var previousPeriodStart = dateRange.StartDate.AddDays(-previousPeriodLength);
            var previousPeriodEnd = dateRange.StartDate.AddDays(-1);

            var previousPeriodData = await GetSalesDataAsync(new DateRange(previousPeriodStart, previousPeriodEnd));
            var previousRevenue = previousPeriodData.Sum(x => x.Revenue);
            var revenueGrowth = previousRevenue > 0 ? (totalRevenue - previousRevenue) / previousRevenue * 100 : 0;

            return new Dictionary<string, decimal>
            {
                { "TotalRevenue", totalRevenue },
                { "TotalCost", totalCost },
                { "TotalProfit", totalProfit },
                { "AverageProfitMargin", avgProfitMargin },
                { "TotalQuantity", totalQuantity },
                { "RevenueGrowth", revenueGrowth }
            };
        }

        #endregion
    }
}