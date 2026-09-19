using System;
using System.Collections.Generic;
using System.Text;

namespace GoogleMapPlugin.Models
{
    public class GoogleTileCoordinate
    {
        public int TileX { get; set; }

        public int TileY { get; set; }

        public double PixelX { get; set; }

        public double PixelY { get; set; }


        public GoogleTileCoordinate()
        {
        }


        public GoogleTileCoordinate(
            int tileX,
            int tileY,
            double pixelX,
            double pixelY)
        {
            TileX = tileX;
            TileY = tileY;

            PixelX = pixelX;
            PixelY = pixelY;
        }
    }
}
