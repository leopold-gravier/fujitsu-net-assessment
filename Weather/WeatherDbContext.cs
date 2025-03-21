using Microsoft.EntityFrameworkCore;
using System;


namespace Weather
{
    public class WeatherDbContext : DbContext
    {
        public DbSet<Station> Stations { get; set; }
        public DbSet<Phenomenon> Phenomena { get; set; }
        public DbSet<Record> Records { get; set; }

        public WeatherDbContext(DbContextOptions options): base(options)
        {
            Database.EnsureCreated();
        }


        public void PopulatePhenomena()
        {
            Console.WriteLine("Populating the phenomena database");
            HashSet<string> currentPhenomenonNames = (from phenomenon in Phenomena select phenomenon.Name).ToHashSet();

            List<Phenomenon> missingPhenomena = new List<Phenomenon>();
            foreach (string phenomenonName in Phenomenon.names)
            {
                if (!currentPhenomenonNames.Contains(phenomenonName))
                {
                    missingPhenomena.Add(new Phenomenon { Name = phenomenonName });
                }
            }

            AddRange(missingPhenomena);
            SaveChanges();
        }
    }

}
