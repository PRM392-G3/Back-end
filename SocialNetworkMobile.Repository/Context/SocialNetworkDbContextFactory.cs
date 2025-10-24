using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SocialNetworkMobile.Repository.Context
{
    public class SocialNetworkDbContextFactory : IDesignTimeDbContextFactory<SocialNetworkDbContext>
    {
        public SocialNetworkDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "", "SocialNetworkMobile", "appsettings.json"), optional: false)
                .Build();

            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Create DbContextOptionsBuilder
            var optionsBuilder = new DbContextOptionsBuilder<SocialNetworkDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new SocialNetworkDbContext(optionsBuilder.Options);
        }
    }
}

