using System;
using System.Globalization;
using System.Xml;


namespace Weather
{
    public class Record
    {
        public uint Id { get; set; }
        public virtual required Station Station { get; set; }
        public float AirTemp { get; set; }
        public float WindSpeed { get; set; }
        public virtual Phenomenon? Phenomenon { get; set; }
        public long Timestamp { get; set; }


        public static Record? FromXml(XmlNode stationNode, long timestamp)
        {
            Station? station = Station.FromXml(stationNode);
            if (station is null)
            {
                return null;
            }

            Phenomenon? phenomenon = Phenomenon.FromXml(stationNode);

            if (!float.TryParse(stationNode["airtemperature"]?.InnerText, CultureInfo.InvariantCulture, out float airTemperature))
            {
                return null;
            }
            if (!float.TryParse(stationNode["windspeed"]?.InnerText, CultureInfo.InvariantCulture, out float windSpeed))
            {
                return null;
            }

            Record record = new Record { Station = station, Phenomenon = phenomenon, AirTemp = airTemperature, WindSpeed = windSpeed, Timestamp = timestamp };
            return record;
        }
    }

}