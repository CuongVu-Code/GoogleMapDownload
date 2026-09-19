using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleMapPlugin.Models
{
    public class GoogleTileRange
    {
        public int MinX { get; set; }

        public int MaxX { get; set; }

        public int MinY { get; set; }

        public int MaxY { get; set; }


        public int Width
        {
            get
            {
                return MaxX - MinX + 1;
            }
        }


        public int Height
        {
            get
            {
                return MaxY - MinY + 1;
            }
        }


        public int TotalTiles
        {
            get
            {
                return Width * Height;
            }
        }
    }
}
