using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CatologServiceAPI.Infrastructure.Context
{
    public class CatalogContextDesignFactory : IDesignTimeDbContextFactory<CatalogContext>
    {
        public CatalogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>()
                            .UseSqlServer("Server=localhost,1433; Database=CatalogService; User Id=SA; Password=reallyStrongPwd123; Encrypt=True; TrustServerCertificate=True;");
            return new CatalogContext(optionsBuilder.Options);
        }
    }
}
