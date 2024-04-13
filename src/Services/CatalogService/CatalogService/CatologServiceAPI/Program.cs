using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CatologServiceAPI.Infrastructure.Context; // Veritabaný baðlamýnýz için
using Microsoft.EntityFrameworkCore; // EF Core için
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Bu kýsýmda ASP.NET Core MVC Controller servislerini ekliyoruz.
builder.Services.AddControllers();

// Swagger/OpenAPI desteði ekleniyor, API dokümantasyonu için kullanýlýr.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Entity Framework Core SQL Server baðlantýsý ekleniyor.
// Connection string, appsettings.json dosyasýndan alýnabilir.
builder.Services.AddDbContext<CatalogContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        }));

// HTTP istek iþleyicisini yapýlandýrýrken HTTPS yönlendirmesi ve güvenlik duvarý ayarlarý yapýlýr.
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Yetkilendirme middleware'ý tanýmlanýr.
app.UseAuthorization();

// Controller endpoint'leri haritalanýr.
app.MapControllers();

// Uygulama çalýþtýrýlýr.
app.Run();
