using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleMapPlugin.Models
{
    public class GoogleMapRequest
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double KinhTuyenTruc { get; set; }
        public int Zoom { get; set; }
        public string MapType { get; set; }
    }
}