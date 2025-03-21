
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Weather;

namespace PricingService
{
    public class Program
    {
        protected static readonly string[] selectedStations = new string[]
        {
            "Tallinn-Harku",
            "Tartu-Tõravere",
            "Pärnu"
        };

        public static void Main(string[] args)
        {
            DbContextOptionsBuilder<WeatherDbContext> optionsBuilder = new DbContextOptionsBuilder<WeatherDbContext>();
            optionsBuilder.UseSqlite("Data Source=weather.sqlite");
            DbContextOptions<WeatherDbContext> dbOptions = optionsBuilder.Options;

            using WeatherDbContext dbContext = new WeatherDbContext(dbOptions);
            dbContext.PopulatePhenomena();
            
            Importer weatherImporter = new Importer(dbContext, selectedStations);

            // Make sure records are imported before starting the web-service
            weatherImporter.ImportWeatherData().Wait();

            using CancellationTokenSource cancellationSource = new CancellationTokenSource();
            _ = weatherImporter.RunAsync(new TimeSpan(0, 15, 0), new TimeSpan(0, 15, 0), cancellationSource.Token);

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<WeatherDbContext>((serviceProvider, options) =>
            {
                options.UseSqlite("Data Source=weather.sqlite");
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Delivery pricing API", Version = "v1" });

                // Custom Enum mapping to display the name and code of each value.
                c.SchemaFilter<EnumSchemaFilter>();
            });

            WebApplication app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            
            app.Run();

            cancellationSource.Cancel();
        }
    }
}
