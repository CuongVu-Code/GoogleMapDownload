using GoogleMapPlugin.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace GoogleMapPlugin.Services
{
    public class GoogleTileFileService
    {
        /// <summary>
        /// Lưu toàn bộ Tile đã tải xuống vào thư mục.
        /// Cấu trúc:
        /// RootFolder
        ///   └── Zoom
        ///       └── X
        ///           └── Y.jpg
        /// </summary>
        public int SaveTiles(List<GoogleTileDownloadResult> results,int zoom,string rootFolder)
        {
            if (results == null || results.Count == 0)
            {
                return 0;
            }
            if (string.IsNullOrEmpty(rootFolder))
            {
                throw new ArgumentException("Thư mục lưu Tile không hợp lệ.");
            }
            int success = 0;
            int i;
            for (i = 0; i < results.Count; i++)
            {
                GoogleTileDownloadResult result = results[i];
                if (result == null)
                {
                    continue;
                }
                if (!result.Success)
                {
                    continue;
                }
                if (result.Data == null || result.Data.Length == 0)
                {
                    continue;
                }
                string filePath = GetTileFilePath(rootFolder,zoom,result.X,result.Y);

                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.WriteAllBytes(filePath, result.Data);
                success++;
            }
            return success;
        }
        /// <summary>
        /// Tạo đường dẫn file của một Tile.
        /// </summary>
        public string GetTileFilePath(string rootFolder,int zoom,int tileX,int tileY)
        {
            string zoomFolder = Path.Combine(rootFolder,zoom.ToString());
            string xFolder = Path.Combine(zoomFolder, tileX.ToString());
            string fileName = tileY.ToString() + ".jpg";
            return Path.Combine(xFolder, fileName);
        }
        /// <summary>
        /// Lưu một Tile.
        /// </summary>
        public bool SaveTile(GoogleTileDownloadResult result, int zoom, string rootFolder)
        {
            if (result == null)
            {
                return false;
            }
            if (!result.Success)
            {
                return false;
            }
            if (result.Data == null || result.Data.Length == 0)
            {
                return false;
            }
            string filePath = GetTileFilePath(rootFolder,zoom,result.X,result.Y);
            string directory =  Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllBytes(filePath,result.Data);
            return true;
        }
    }
}