using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleMapPlugin.Models
{
    public class KinhTuyenTruc
    {
        public int STT { get; set; }

        public string TinhThanh { get; set; }

        public double KinhDo { get; set; }

        public string HienThi
        {
            get
            {
                return TinhThanh +
                       " - " +
                       KinhDo.ToString("0.00") +
                       "°";
            }
        }

        public override string ToString()
        {
            return HienThi;
        }
    }
}