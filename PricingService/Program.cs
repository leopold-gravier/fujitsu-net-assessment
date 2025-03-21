
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
            Weather.WeatherDbContext.PopulatePhenomena();
            Weather.Importer weatherImporter = new Weather.Importer(selectedStations);

            // Make sure records are imported before starting the web-service
            weatherImporter.ImportWeatherData().Wait();

            using CancellationTokenSource cancellationSource = new CancellationTokenSource();
            _ = weatherImporter.RunAsync(new TimeSpan(0, 15, 0), new TimeSpan(0, 15, 0), cancellationSource.Token);

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<Weather.WeatherDbContext>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
