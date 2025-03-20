using Microsoft.EntityFrameworkCore;
using System;


namespace Weather
{
    public class WeatherDbContext : DbContext
    {
        public DbSet<Station> Stations { get; set; }
        public DbSet<Phenomenon> Phenomena { get; set; }
        public DbSet<Record> Records { get; set; }

        public WeatherDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // optionsBuilder.UseInMemoryDatabase("weather");
            optionsBuilder.UseSqlite("Data Source=weather.sqlite");
        }


        public static void PopulatePhenomena()
        {
            Console.WriteLine("Populating the phenomena database");
            using WeatherDbContext db = new WeatherDbContext();
            HashSet<string> currentPhenomenonNames = (from phenomenon in db.Phenomena select phenomenon.Name).ToHashSet();

            List<Phenomenon> missingPhenomena = new List<Phenomenon>();
            foreach (string phenomenonName in Phenomenon.names)
            {
                if (!currentPhenomenonNames.Contains(phenomenonName))
                {
                    missingPhenomena.Add(new Phenomenon { Name = phenomenonName });
                }
            }

            db.AddRange(missingPhenomena);
            db.SaveChanges();
        }
    }

}
