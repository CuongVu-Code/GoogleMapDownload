using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleMapPlugin.Models
{
    public class Vn2000Coordinate
    {
        public double X { get; set; }

        public double Y { get; set; }

        public Vn2000Coordinate(
            double x,
            double y)
        {
            X = x;
            Y = y;
        }
    }
}