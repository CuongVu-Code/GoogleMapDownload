using GoogleMapPlugin.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace GoogleMapPlugin.Services
{
    public class GoogleTileMergeService
    {
        private const int TILE_SIZE = 256;

        /// <summary>
        /// Ghép danh sách Google Tile thành một ảnh lớn.
        /// </summary>
        public string MergeTiles(List<GoogleTileItem> tiles,int zoom,string tileFolder,string outputFile)
        {
            if (tiles == null || tiles.Count == 0)
            {
                throw new ArgumentException("Danh sách Tile rỗng.");
            }
            if (string.IsNullOrEmpty(tileFolder))
            {
                throw new ArgumentException("Thư mục Tile không hợp lệ.");
            }
            if (string.IsNullOrEmpty(outputFile))
            {
                throw new ArgumentException("File ảnh đầu ra không hợp lệ.");
            }
            // Tìm phạm vi Tile
            int minX = tiles[0].X;
            int maxX = tiles[0].X;
            int minY = tiles[0].Y;
            int maxY = tiles[0].Y;
            int i;
            for (i = 1; i < tiles.Count; i++)
            {
                if (tiles[i].X < minX)
                    minX = tiles[i].X;
                if (tiles[i].X > maxX)
                    maxX = tiles[i].X;
                if (tiles[i].Y < minY)
                    minY = tiles[i].Y;
                if (tiles[i].Y > maxY)
                    maxY = tiles[i].Y;
            }
            int tileColumns = maxX - minX + 1;
            int tileRows = maxY - minY + 1;
            int imageWidth = tileColumns * TILE_SIZE;
            int imageHeight = tileRows * TILE_SIZE;
            Bitmap mergedBitmap = new Bitmap(imageWidth,imageHeight,PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(mergedBitmap))
            {
                graphics.Clear(Color.White);
                for (i = 0; i < tiles.Count; i++)
                {
                    GoogleTileItem tile = tiles[i];
                    string filePath = GetTileFilePath(tileFolder,zoom,tile.X,tile.Y);
                    if (!File.Exists(filePath))
                    {
                        continue;
                    }
                    int offsetX = (tile.X - minX) * TILE_SIZE;
                    int offsetY = (tile.Y - minY) * TILE_SIZE;
                    using (Bitmap tileBitmap = new Bitmap(filePath))
                    {
                        graphics.DrawImage(tileBitmap,offsetX,offsetY,TILE_SIZE,TILE_SIZE);
                    }
                }
            }
            string outputDirectory = Path.GetDirectoryName(outputFile);
            if (!string.IsNullOrEmpty(outputDirectory))
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
            }
            mergedBitmap.Save(outputFile,ImageFormat.Jpeg);
            mergedBitmap.Dispose();
            return outputFile;
        }
        /// <summary>
        /// Tạo đường dẫn của một Tile.
        /// </summary>
        private string GetTileFilePath(string tileFolder,int zoom,int tileX,int tileY)
        {
            //return Path.Combine(tileFolder,zoom.ToString(),tileX.ToString(),tileY.ToString() + ".jpg");
            string zoomFolder = Path.Combine(tileFolder,zoom.ToString());
            string xFolder =    Path.Combine(zoomFolder,tileX.ToString());
            return Path.Combine(xFolder,tileY.ToString() + ".jpg");
        }

        //Test Tile Merge

       
    }
}