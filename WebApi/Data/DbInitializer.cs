// Data/DbInitializer.cs
using Shared.Models; // Updated namespace for Workload model

namespace WebApi.Data;
public static class DbInitializer
{
    public static void Initialize(AppDbContext context)
    {
        context.Database.EnsureCreated();

        // Look for any workloads.
        if (context.Workloads.Any())
        {
            return;   // DB has been seeded
        }
    }
}