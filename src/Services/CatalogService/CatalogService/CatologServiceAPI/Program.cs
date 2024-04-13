using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CatologServiceAPI.Infrastructure.Context; // Veritaban� ba�lam�n�z i�in
using Microsoft.EntityFrameworkCore; // EF Core i�in
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Bu k�s�mda ASP.NET Core MVC Controller servislerini ekliyoruz.
builder.Services.AddControllers();

// Swagger/OpenAPI deste�i ekleniyor, API dok�mantasyonu i�in kullan�l�r.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Entity Framework Core SQL Server ba�lant�s� ekleniyor.
// Connection string, appsettings.json dosyas�ndan al�nabilir.
builder.Services.AddDbContext<CatalogContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
            sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
        }));

// HTTP istek i�leyicisini yap�land�r�rken HTTPS y�nlendirmesi ve g�venlik duvar� ayarlar� yap�l�r.
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Yetkilendirme middleware'� tan�mlan�r.
app.UseAuthorization();

// Controller endpoint'leri haritalan�r.
app.MapControllers();

// Uygulama �al��t�r�l�r.
app.Run();
