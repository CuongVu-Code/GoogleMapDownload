using GoogleMapPlugin.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GoogleMapPlugin.Services
{
    public class GoogleTileCropService
    {
        private const int TILE_SIZE = 256;


        /// <summary>
        /// Cắt ảnh merged theo phạm vi WGS84.
        /// </summary>
        public string CropImage(
            string mergedFile,
            string outputFile,
            GoogleTileRange tileRange,
            double latMin,
            double lonMin,
            double latMax,
            double lonMax,
            int zoom)
        {
            if (!File.Exists(mergedFile))
            {
                throw new FileNotFoundException(
                    "Không tìm thấy ảnh Merge:\n" +
                    mergedFile);
            }

            if (tileRange == null)
            {
                throw new ArgumentNullException(
                    "tileRange");
            }

            // -----------------------------------------
            // 1. Chuyển WGS84 → Global Pixel
            // -----------------------------------------

            PointF pixelMin =
                LatLonToGlobalPixel(
                    latMax,
                    lonMin,
                    zoom);

            PointF pixelMax =
                LatLonToGlobalPixel(
                    latMin,
                    lonMax,
                    zoom);


            // -----------------------------------------
            // 2. Tọa độ Pixel trong ảnh Merge
            // -----------------------------------------

            double originX =
                tileRange.MinX * TILE_SIZE;

            double originY =
                tileRange.MinY * TILE_SIZE;

            double cropLeftDouble =
                pixelMin.X - originX;

            double cropTopDouble =
                pixelMin.Y - originY;

            double cropRightDouble =
                pixelMax.X - originX;

            double cropBottomDouble =
                pixelMax.Y - originY;


            // -----------------------------------------
            // 3. Làm tròn để bao phủ đủ vùng
            // -----------------------------------------

            int cropLeft =
                (int)Math.Floor(
                    cropLeftDouble);

            int cropTop =
                (int)Math.Floor(
                    cropTopDouble);

            int cropRight =
                (int)Math.Ceiling(
                    cropRightDouble);

            int cropBottom =
                (int)Math.Ceiling(
                    cropBottomDouble);


            int cropWidth =
                cropRight - cropLeft;

            int cropHeight =
                cropBottom - cropTop;


            if (cropWidth <= 0 ||
                cropHeight <= 0)
            {
                throw new Exception(
                    "Kích thước vùng Crop không hợp lệ.");
            }


            // -----------------------------------------
            // 4. Kiểm tra giới hạn ảnh Merge
            // -----------------------------------------

            int mergedWidth =
                tileRange.Width * TILE_SIZE;

            int mergedHeight =
                tileRange.Height * TILE_SIZE;


            if (cropLeft < 0 ||
                cropTop < 0 ||
                cropRight > mergedWidth ||
                cropBottom > mergedHeight)
            {
                throw new Exception(
                    "Vùng Crop nằm ngoài ảnh Merge.\n\n" +
                    "Crop:\n" +
                    cropLeft + ", " +
                    cropTop + " → " +
                    cropRight + ", " +
                    cropBottom +
                    "\n\n" +
                    "Ảnh Merge:\n" +
                    mergedWidth + " × " +
                    mergedHeight);
            }


            // -----------------------------------------
            // 5. Thực hiện Crop
            // -----------------------------------------

            Bitmap croppedBitmap =
                new Bitmap(
                    cropWidth,
                    cropHeight,
                    PixelFormat.Format24bppRgb);

            using (Bitmap mergedBitmap =
                new Bitmap(mergedFile))
            {
                using (Graphics graphics =
                    Graphics.FromImage(croppedBitmap))
                {
                    graphics.DrawImage(
                        mergedBitmap,
                        new Rectangle(
                            0,
                            0,
                            cropWidth,
                            cropHeight),
                        new Rectangle(
                            cropLeft,
                            cropTop,
                            cropWidth,
                            cropHeight),
                        GraphicsUnit.Pixel);
                }
            }


            // -----------------------------------------
            // 6. Tạo thư mục output
            // -----------------------------------------

            string directory =
                Path.GetDirectoryName(outputFile);

            if (!string.IsNullOrEmpty(directory))
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(
                        directory);
                }
            }


            // -----------------------------------------
            // 7. Lưu ảnh
            // -----------------------------------------

            croppedBitmap.Save(
                outputFile,
                ImageFormat.Jpeg);

            croppedBitmap.Dispose();


            return outputFile;
        }


        /// <summary>
        /// Chuyển Latitude/Longitude sang
        /// Global Pixel của Google Maps.
        /// </summary>
        private PointF LatLonToGlobalPixel(
            double latitude,
            double longitude,
            int zoom)
        {
            double mapSize =
                TILE_SIZE *
                Math.Pow(
                    2,
                    zoom);

            // Giới hạn Latitude
            if (latitude > 85.05112878)
            {
                latitude = 85.05112878;
            }

            if (latitude < -85.05112878)
            {
                latitude = -85.05112878;
            }


            // Pixel X
            double x =
                (longitude + 180.0)
                / 360.0
                * mapSize;


            // Pixel Y
            double sinLatitude =
                Math.Sin(
                    latitude *
                    Math.PI /
                    180.0);

            double y =
                (0.5 -
                Math.Log(
                    (1.0 + sinLatitude)
                    /
                    (1.0 - sinLatitude))
                /
                (4.0 * Math.PI))
                * mapSize;


            return new PointF(
                (float)x,
                (float)y);
        }
    }
}