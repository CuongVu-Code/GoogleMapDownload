using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleMapPlugin.Models
{
    public class GoogleTileItem
    {
        public int X { get; set; }

        public int Y { get; set; }


        public GoogleTileItem(
            int x,
            int y)
        {
            X = x;
            Y = y;
        }
    }
}