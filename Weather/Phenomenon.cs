using System;
using System.Xml;

namespace Weather
{
    public class Phenomenon
    {
        public uint Id { get; set; }
        public required string Name { get; set; }


        public static readonly string[] names = new string[]
        {
            "Clear",
            "Few clouds",
            "Variable clouds",
            "Cloudy with clear spells",
            "Overcast",
            "Light snow shower",
            "Moderate snow shower",
            "Heavy snow shower",
            "Light shower",
            "Moderate shower",
            "Heavy shower",
            "Light rain",
            "Moderate rain",
            "Heavy rain",
            "Glaze",
            "Light sleet",
            "Moderate sleet",
            "Light snowfall",
            "Moderate snowfall",
            "Heavy snowfall",
            "Hail",
            "Mist",
            "Fog",
        };


        public static Phenomenon? FromXml(XmlNode stationNode)
        {
            string? phenomenonName = stationNode["phenomenon"]?.InnerText;
            if (phenomenonName is null)
            {
                return null;
            } else
            {
                Phenomenon phenomenon = new Phenomenon { Name = phenomenonName };
                return phenomenon;
            }
        }
    }

}