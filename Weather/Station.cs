using System;
using System.Globalization;
using System.Xml;

namespace Weather
{
    public class Station
    {
        public uint Id { get; set; }
        public required string Name { get; set; }
        public uint WmoCode { get; set; }


        public static Station? FromXml(XmlNode stationNode)
        {
            string? stationName = stationNode["name"]?.InnerText;
            if (stationName is null)
            {
                // TODO: Log warnings when parsing fails
                return null;
            }
            if (!uint.TryParse(stationNode["wmocode"]?.InnerText, CultureInfo.InvariantCulture, out uint wmoCode))
            {
                return null;
            }

            Station station = new Station { Name = stationName, WmoCode = wmoCode };
            return station;
        }
    }

}