using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleMapPlugin.Models
{
    public class Wgs84Coordinate
    {
        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public Wgs84Coordinate()
        {
        }

        public Wgs84Coordinate(
            double longitude,
            double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public override string ToString()
        {
            return
                "Longitude = " +
                Longitude.ToString("0.0000000000") +
                ", Latitude = " +
                Latitude.ToString("0.0000000000");
        }
    }
}