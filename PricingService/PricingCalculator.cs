using Microsoft.EntityFrameworkCore;
using System.Linq;
using Weather;


namespace PricingService
{
    public static class PricingCalculator
    {
        static readonly Dictionary<string, string> weatherStations = new Dictionary<string, string>()
        {
            { "Tallinn", "Tallinn-Harku" },
            { "Tartu", "Tartu-Tõravere" },
            { "Parnu", "Pärnu" }
        };

        static readonly Dictionary<string, Dictionary<VehicleType, float>> regionalBaseFeeDb = new Dictionary<string, Dictionary<VehicleType, float>>()
        {
            {
                "Tallinn",
                new Dictionary<VehicleType, float>()
                {
                    { VehicleType.Car, 4.0f},
                    { VehicleType.Scooter, 3.5f },
                    { VehicleType.Bike, 3.0f }
                }
            },
            {
                "Tartu",
                new Dictionary<VehicleType, float>()
                {
                    { VehicleType.Car, 3.5f },
                    { VehicleType.Scooter, 3.0f },
                    { VehicleType.Bike, 2.5f }
                }
            },
            {
                "Parnu",
                new Dictionary<VehicleType, float>()
                {
                    { VehicleType.Car, 3.0f },
                    { VehicleType.Scooter, 2.5f },
                    { VehicleType.Bike, 2.0f }
                }
            }
        };
        
        public static float GetDeliveryPrice(string city, VehicleType vehicle)
        {
            if (!weatherStations.ContainsKey(city))
            {
                throw new InvalidDeliveryException($"Cannot deliver to the city of {city}");
            }

            string weatherStationName = weatherStations[city];

            using WeatherDbContext db = new WeatherDbContext();
            Record? weatherRecord = (from r in db.Records where r.Station.Name == weatherStationName orderby r.Timestamp descending select r).Include(r => r.Station).Include(r => r.Phenomenon).FirstOrDefault();
            if (weatherRecord is null)
            {
                throw new Exception($"No weather data available in {city}");
            }

            float price = 0.0f;

            price += GetRegionalBaseFee(city, vehicle);
            price += GetAirTemperatureExtraFee(vehicle, weatherRecord);
            price += GetWindSpeedExtraFee(vehicle, weatherRecord);
            price += GetWeatherPhenomenonExtraFee(vehicle, weatherRecord);

            return price;
        }

        public static float GetRegionalBaseFee(string city, VehicleType vehicle)
        {
            if (!regionalBaseFeeDb.TryGetValue(city, out Dictionary<VehicleType, float>? regionalFees))
            {
                throw new Exception($"Unsupported city '{city}'");
            }
            return regionalFees[vehicle];
        }

        public static float GetAirTemperatureExtraFee(VehicleType vehicle, Record record)
        {
            if (vehicle == VehicleType.Car)
            {
                return 0.0f;
            }

            float temperature = record.AirTemp;
            if (temperature < -10.0f)
            {
                return 1.0f;
            }
            else if (temperature < 0.0f)
            {
                return 0.5f;
            }
            else
            {
                return 0.0f;
            }
        }

        public static float GetWindSpeedExtraFee(VehicleType vehicle, Record record)
        {
            if (vehicle != VehicleType.Bike)
            {
                return 0.0f;
            }

            float windSpeed = record.WindSpeed;
            if (windSpeed < 10.0f)
            {
                return 0.0f;
            }
            else if (windSpeed < 20.0f)
            {
                return 0.5f;
            }
            else
            {
                throw new InvalidDeliveryException("Usage of selected vehicle type is forbidden");
            }
        }

        public static float GetWeatherPhenomenonExtraFee(VehicleType vehicle, Record record)
        {
            if (vehicle == VehicleType.Car)
            {
                return 0.0f;
            }

            Phenomenon? phenomenon = record.Phenomenon;

            if (phenomenon == null)
            {
                throw new NullReferenceException($"Phenomenon missing in {record.Station.Name}");
            }

            switch (phenomenon.Name)
            {
                case "Light snow shower":
                case "Moderate snow shower":
                case "Heavy snow shower":
                case "Light sleet":
                case "Moderate sleet":
                case "Light snowfall":
                case "Moderate snowfall":
                case "Heavy snowfall":
                    return 1.0f;

                case "Light shower":
                case "Moderate shower":
                case "Heavy shower":
                case "Light rain":
                case "Moderate rain":
                case "Heavy rain":
                    return 0.5f;

                case "Glaze":
                case "Hail":
                    throw new InvalidDeliveryException("Usage of selected vehicle type is forbidden");

                case "Clear":
                case "Few clouds":
                case "Variable clouds":
                case "Cloudy with clear spells":
                case "Overcast":
                case "Mist":
                case "Fog":
                    return 0.0f;

                default:
                    throw new Exception($"Unexpected weather phenomenon '{phenomenon}'");
            }
        }
    }


    public enum VehicleType
    {
        Car,
        Scooter,
        Bike
    }


    public class InvalidDeliveryException : Exception
    {
        public InvalidDeliveryException()
        {
        }

        public InvalidDeliveryException(string? message) : base(message)
        {
        }
    }

}
