using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleMapPlugin.Models
{
    public class AffineTransform
    {
        // X = A * pixelX + B * pixelY + C
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }

        // Y = D * pixelX + E * pixelY + F
        public double D { get; set; }
        public double E { get; set; }
        public double F { get; set; }
    }
}
