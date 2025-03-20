using System;
using System.Globalization;
using System.Xml;


namespace Weather
{
    public class Importer
    {
        protected string[] selectedStations;


        public Importer(string[] selectedStations)
        {
            this.selectedStations = selectedStations;
        }


        public async Task RunAsync(TimeSpan initialDelay, TimeSpan delay, CancellationToken cancellationToken)
        {
            await Task.Delay(initialDelay, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await ImportWeatherData();
                await Task.Delay(delay, cancellationToken);
            }
        }


        public async Task ImportWeatherData()
        {
            string weatherXml = await FetchWeatherData();
            List<Record> records = ParseWeatherData(weatherXml);
            RegisterRecords(records);
        }

        protected async Task<string> FetchWeatherData()
        {
            Console.WriteLine("Fetching weather data");
            const string url = "https://www.ilmateenistus.ee/ilma_andmed/xml/observations.php";

            using HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);

            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/xml"));
            string result = await client.GetStringAsync("");

            using HttpResponseMessage response = await client.GetAsync("");
            response.EnsureSuccessStatusCode();
            string xml = await response.Content.ReadAsStringAsync();

            return result;
        }


        protected List<Record> ParseWeatherData(string xml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            XmlNode? observation = document.DocumentElement;
            if (observation is null)
            {
                return new List<Record>();
            }
            if (observation.Name != "observations")
            {
                return new List<Record>();
            }

            string? timestampStr = observation.Attributes?["timestamp"]?.Value;
            if (!long.TryParse(timestampStr, CultureInfo.InvariantCulture, out long timestamp))
            {
                return new List<Record>();
            }

            XmlNodeList? stations = observation.SelectNodes("/observations/station");
            if (stations is null)
            {
                return new List<Record>();
            }

            List<Record> records = new List<Record>();
            foreach (XmlNode station in stations)
            {
                if (!selectedStations.Contains(station["name"]?.InnerText))
                {
                    continue;
                }

                Record? record = Record.FromXml(station, timestamp);
                if (record is not null)
                {
                    records.Add(record);
                }
            }

            return records;
        }


        protected void RegisterRecords(IEnumerable<Record> records)
        {
            Console.WriteLine($"Adding {records.Count()} weather records");

            using WeatherDbContext db = new WeatherDbContext();

            foreach (Record record in records)
            {
                Station? station = (from s in db.Stations where s.Name == record.Station.Name select s).FirstOrDefault();
                if (station is null)
                {
                    Console.WriteLine($"Adding database entry for weather station {record.Station.Name}");
                    db.Stations.Add(record.Station);
                }
                else
                {
                    if (record.Station.WmoCode != station.WmoCode)
                    {
                        throw new Exception($"WMO code of station {record.Station.Name} ({record.Station.WmoCode}) does not match the known code {station.WmoCode}");
                    }

                    record.Station = station;
                }

                if (record.Phenomenon is not null)
                {
                    Phenomenon? phenomenon = (from p in db.Phenomena where p.Name == record.Phenomenon.Name select p).FirstOrDefault();
                    if (phenomenon is null)
                    {
                        throw new Exception($"Unknown weather phenomenon '{record.Phenomenon.Name}'");
                    }
                    else
                    {
                        record.Phenomenon = phenomenon;
                    }
                }
            }

            db.Records.AddRange(records);

            db.SaveChanges();
        }

    }

}
