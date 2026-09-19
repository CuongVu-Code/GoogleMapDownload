using System;
using GoogleMapPlugin.Models;
using System.Collections.Generic;

namespace GoogleMapPlugin.Services
{
    public class GoogleTileService
    {
        private const int TILE_SIZE = 256;
        //HÀM TÍNH SIN
        private double Asinh(double x)
        {
            return Math.Log(x + Math.Sqrt(x * x + 1.0));
        }
        // =====================================================
        // WGS84 → GOOGLE TILE
        // =====================================================
        public GoogleTileCoordinate LatLonToTile(double latitude,double longitude,int zoom)
        {
            // -------------------------------------------------
            // Kiểm tra Zoom
            // -------------------------------------------------
            if (zoom < 0)
            {
                throw new ArgumentException("Zoom phải >= 0.");
            }
            // -------------------------------------------------
            // Giới hạn Latitude
            // Google Web Mercator không nhận quá ±85.05112878°
            // -------------------------------------------------
            latitude = Math.Max(-85.05112878,Math.Min(85.05112878,latitude));
            // -------------------------------------------------
            // Số tile mỗi chiều
            // -------------------------------------------------
            double n = Math.Pow(2.0,zoom);
            // -------------------------------------------------
            // Latitude → Radian
            // -------------------------------------------------
            double latRad = latitude * Math.PI /180.0;
            // -------------------------------------------------
            // Pixel X
            // -------------------------------------------------
            double pixelX =(longitude + 180.0)/360.0 * n * TILE_SIZE;
            // -------------------------------------------------
            // Pixel Y
            // -------------------------------------------------
            double pixelY =(1.0- Asinh(Math.Tan(latRad))/Math.PI)/2.0*n*TILE_SIZE;
            // -------------------------------------------------
            // Tile X/Y
            // -------------------------------------------------
            int tileX = (int)Math.Floor(pixelX /TILE_SIZE);
            int tileY = (int)Math.Floor(pixelY /TILE_SIZE);
            return new GoogleTileCoordinate(tileX,tileY,pixelX,pixelY);
        }
        // =====================================================
        // WGS84 → TILE X/Y
        // Tương đương deg2num() của Python
        // =====================================================
        public int[] Deg2Num(double latitude,double longitude,int zoom)
        {
            GoogleTileCoordinate result = LatLonToTile(latitude,longitude,zoom);
            return new int[]
            {
                result.TileX,
                result.TileY
            };
        }
        // =====================================================
        // WGS84 → GLOBAL PIXEL
        // =====================================================
        public double[] LatLonToPixel(double latitude,double longitude,int zoom)
        {
            GoogleTileCoordinate result = LatLonToTile(latitude,longitude,zoom);
            return new double[]
            {
                result.PixelX,
                result.PixelY
            };
        }
        public GoogleTileRange GetTileRange(double latMin,double lonMin,double latMax,double lonMax,int zoom)
        {
            GoogleTileCoordinate topLeft = LatLonToTile(latMax,lonMin,zoom);
            GoogleTileCoordinate bottomRight = LatLonToTile(latMin,lonMax,zoom);
            GoogleTileRange range = new GoogleTileRange();
            range.MinX = Math.Min(topLeft.TileX,bottomRight.TileX);
            range.MaxX =Math.Max(topLeft.TileX,bottomRight.TileX);
            range.MinY = Math.Min(topLeft.TileY,bottomRight.TileY);
            range.MaxY = Math.Max(topLeft.TileY,bottomRight.TileY);
            return range;
        }
        public List<GoogleTileItem> GetTileList(GoogleTileRange range)
        {
            List<GoogleTileItem> tiles =  new List<GoogleTileItem>();

            for (int x = range.MinX;x <= range.MaxX;x++)
            {
                for (int y = range.MinY;y <= range.MaxY;y++)
                {
                    tiles.Add(new GoogleTileItem(x, y));
                }
            }
            return tiles;
        }
    }
}