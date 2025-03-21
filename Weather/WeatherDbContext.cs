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


        public void ImportRecord(Record record)
        {
            record.Station = ImportStation(record.Station);
            if (record.Phenomenon is not null)
            {
                record.Phenomenon = ImportPhenomenon(record.Phenomenon);
            }

            Records.Add(record);
        }


        public Station ImportStation(Station station)
        {
            Station? dbStation = (from s in Stations where s.Name == station.Name select s).FirstOrDefault();
            if (dbStation is null)
            {
                Console.WriteLine($"Adding database entry for weather station {station.Name}");
                Stations.Add(station);

                return station;
            }
            else
            {
                if (dbStation.WmoCode != station.WmoCode)
                {
                    throw new Exception($"WMO code of station {station.Name} ({station.WmoCode}) does not match the known code {dbStation.WmoCode}");
                }

                return dbStation;
            }
        }


        public Phenomenon ImportPhenomenon(Phenomenon phenomenon)
        {
            Phenomenon? dbPhenomenon = (from p in Phenomena where p.Name == phenomenon.Name select p).FirstOrDefault();
            if (dbPhenomenon is null)
            {
                throw new Exception($"Unknown weather phenomenon '{phenomenon.Name}'");
            }

            return dbPhenomenon;
        }

    }

}
