// Data/DbInitializer.cs
using WebApi.Models;

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