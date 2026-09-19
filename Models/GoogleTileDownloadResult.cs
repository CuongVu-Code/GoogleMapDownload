using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleMapPlugin.Models
{
    public class GoogleTileDownloadResult
    {
        public int X { get; set; }
        public int Y { get; set; }
        public byte[] Data { get; set; }
        public bool Success { get; set; }
        public GoogleTileDownloadResult(int x,int y)
        {
            X = x;
            Y = y;
            Data = null;
            Success = false;
        }
    }
}