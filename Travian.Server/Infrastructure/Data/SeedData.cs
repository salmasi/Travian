namespace YourProjectName.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(AppDbContext context)
        {
            if (!context.BuildingProductionRates.Any())
            {
                await context.BuildingProductionRates.AddRangeAsync(
                    new BuildingProductionRate { /* ... */ },
                    // ...
                );
                
                await context.SaveChangesAsync();
            }
        }
    }
}