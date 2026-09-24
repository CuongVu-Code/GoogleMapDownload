using GoogleMapPlugin.Models;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace GoogleMapPlugin.Services
{
    public class GoogleWorldFileService
    {
        private const int TILE_SIZE = 256;
        // =====================================================
        // TẠO WORLD FILE JGW
        // =====================================================
        public string CreateJgw(
            string imageFile,
            string jgwFile,
            GoogleTileRange tileRange,
            double latMin,
            double lonMin,
            double latMax,
            double lonMax,
            int zoom,
            double ktt)
        {
            if (!File.Exists(imageFile))
            {
                throw new FileNotFoundException(
                    "Không tìm thấy file ảnh.",
                    imageFile);
            }

            if (tileRange == null)
            {
                throw new ArgumentNullException(
                    "tileRange");
            }
            // =================================================
            // 1. Đọc kích thước ảnh Crop
            // =================================================
            Bitmap bitmap = new Bitmap(imageFile);
            int width =  bitmap.Width;
            int height = bitmap.Height;
            bitmap.Dispose();
            if (width <= 0 || height <= 0)
            {
                throw new Exception(
                    "Kích thước ảnh không hợp lệ.");
            }
            // =================================================
            // 2. Tính Global Pixel của vùng Crop
            //
            // Phần này phải giống GoogleTileCropService
            // =================================================
            PointF pixelMin = LatLonToGlobalPixel(latMax,lonMin,zoom);
            PointF pixelMax = LatLonToGlobalPixel(latMin,lonMax,zoom);
            double originX =  tileRange.MinX * TILE_SIZE;
            double originY =  tileRange.MinY * TILE_SIZE;
            int cropLeft =    (int)Math.Floor(pixelMin.X - originX);
            int cropTop =     (int)Math.Floor(pixelMin.Y - originY);
            int cropRight =   (int)Math.Ceiling(pixelMax.X - originX);
            int cropBottom =  (int)Math.Ceiling(pixelMax.Y - originY);
            int cropWidth =   cropRight -  cropLeft;
            int cropHeight =  cropBottom - cropTop;
            // =================================================
            // 3. Kiểm tra kích thước
            // =================================================
            if (cropWidth <= 0 ||cropHeight <= 0)
            {
                throw new Exception("Kích thước Crop không hợp lệ.");
            }
            if (cropWidth != width || cropHeight != height)
            {
                throw new Exception("Kích thước ảnh Crop không khớp.\n\n" +
                    "Kích thước tính toán:\n" +
                    cropWidth +
                    " x " +
                    cropHeight +
                    "\n\n" +
                    "Kích thước ảnh thực tế:\n" +
                    width +
                    " x " +
                    height);
            }
            // =================================================
            // 4. GLOBAL PIXEL
            //
            // Dùng Global Pixel để tìm WGS84.
            //
            // Pixel đầu tiên:
            // tâm pixel = 0.5
            // =================================================
            double globalPixel1X = originX + cropLeft + 0.5;
            double globalPixel1Y = originY + cropTop + 0.5;
            double globalPixel2X = originX + cropLeft + width - 0.5;
            double globalPixel2Y = originY + cropTop + 0.5;
            double globalPixel3X = originX + cropLeft + 0.5;
            double globalPixel3Y = originY + cropTop + height - 0.5;
            // =================================================
            // 5. GLOBAL PIXEL → WGS84
            // =================================================
            Wgs84Coordinate wgs1 = GlobalPixelToWgs84(globalPixel1X, globalPixel1Y, zoom);
            Wgs84Coordinate wgs2 = GlobalPixelToWgs84(globalPixel2X, globalPixel2Y, zoom);
            Wgs84Coordinate wgs3 = GlobalPixelToWgs84(globalPixel3X, globalPixel3Y, zoom);
            // =================================================
            // 6. WGS84 → VN2000
            // =================================================
            CoordinateService coordinateService = new CoordinateService();
            Vn2000Coordinate vn1 = coordinateService.ToVn2000(wgs1.Latitude,wgs1.Longitude,ktt);
            Vn2000Coordinate vn2 = coordinateService.ToVn2000(wgs2.Latitude,wgs2.Longitude,ktt);
            Vn2000Coordinate vn3 = coordinateService.ToVn2000(wgs3.Latitude,wgs3.Longitude,ktt);
            // =================================================
            // 7. LOCAL PIXEL
            //
            // Đây là điểm quan trọng.
            //
            // JGW dùng tọa độ pixel của CHÍNH ẢNH CROP,
            // không dùng Global Pixel.
            //
            // P1 = tâm pixel trên trái
            // P2 = tâm pixel trên phải
            // P3 = tâm pixel dưới trái
            // =================================================
            double pixel1X =
                0.5;
            double pixel1Y =
                0.5;
            double pixel2X =
                width - 0.5;
            double pixel2Y =
                0.5;
            double pixel3X =
                0.5;
            double pixel3Y =
                height - 0.5;
            // =================================================
            // 8. Tính Affine Transform
            // =================================================
            AffineTransform affine =
                CalculateAffine(
                    pixel1X,
                    pixel1Y,
                    vn1,
                    pixel2X,
                    pixel2Y,
                    vn2,
                    pixel3X,
                    pixel3Y,
                    vn3);
            // =================================================
            // 9. Tạo thư mục
            // =================================================
            string directory =
                Path.GetDirectoryName(
                    jgwFile);
            if (!string.IsNullOrEmpty(directory))
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(
                        directory);
                }
            }
            // =================================================
            // 10. Ghi file JGW
            //
            // Thứ tự:
            //
            // A
            // D
            // B
            // E
            // C
            // F
            // =================================================
            using (StreamWriter writer =
                new StreamWriter(jgwFile))
            {
                writer.WriteLine(
                    affine.A.ToString(
                        "0.###############",
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    affine.D.ToString(
                        "0.###############",
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    affine.B.ToString(
                        "0.###############",
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    affine.E.ToString(
                        "0.###############",
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    affine.C.ToString(
                        "0.###############",
                        CultureInfo.InvariantCulture));
                writer.WriteLine(
                    affine.F.ToString(
                        "0.###############",
                        CultureInfo.InvariantCulture));
            }
            return jgwFile;
        }
        // =====================================================
        // TÍNH AFFINE TRANSFORM
        // =====================================================
        private AffineTransform CalculateAffine(
            double pixel1X,
            double pixel1Y,
            Vn2000Coordinate point1,
            double pixel2X,
            double pixel2Y,
            Vn2000Coordinate point2,
            double pixel3X,
            double pixel3Y,
            Vn2000Coordinate point3)
        {
            /*
             *
             * X = A * pixelX
             *   + B * pixelY
             *   + C
             *
             * Y = D * pixelX
             *   + E * pixelY
             *   + F
             *
             */
            double determinant =
                (pixel2X - pixel1X)
                *
                (pixel3Y - pixel1Y)
                -
                (pixel3X - pixel1X)
                *
                (pixel2Y - pixel1Y);
            if (Math.Abs(determinant) <
                1.0e-12)
            {
                throw new Exception(
                    "Không thể tính Affine Transform.");
            }
            // =================================================
            // X
            // =================================================
            double dx21 =
                point2.X -
                point1.X;
            double dx31 =
                point3.X -
                point1.X;
            double A =
                (
                    dx21 *
                    (pixel3Y - pixel1Y)
                    -
                    dx31 *
                    (pixel2Y - pixel1Y)
                )
                /
                determinant;
            double B =
                (
                    (pixel2X - pixel1X) *
                    dx31
                    -
                    (pixel3X - pixel1X) *
                    dx21
                )
                /
                determinant;
            double C =
                point1.X
                - A * pixel1X
                - B * pixel1Y;
            // =================================================
            // Y
            // =================================================
            double dy21 =
                point2.Y -
                point1.Y;
            double dy31 =
                point3.Y -
                point1.Y;
            double D =
                (
                    dy21 *
                    (pixel3Y - pixel1Y)
                    -
                    dy31 *
                    (pixel2Y - pixel1Y)
                )
                /
                determinant;
            double E =
                (
                    (pixel2X - pixel1X) *
                    dy31
                    -
                    (pixel3X - pixel1X) *
                    dy21
                )
                /
                determinant;
            double F =
                point1.Y
                - D * pixel1X
                - E * pixel1Y;
            AffineTransform result =
                new AffineTransform();
            result.A = A;
            result.B = B;
            result.C = C;
            result.D = D;
            result.E = E;
            result.F = F;
            return result;
        }
        // =====================================================
        // WGS84 → GLOBAL PIXEL
        //
        // Google Web Mercator
        // =====================================================
        private PointF LatLonToGlobalPixel(
            double latitude,
            double longitude,
            int zoom)
        {
            double mapSize =
                TILE_SIZE *
                Math.Pow(
                    2.0,
                    zoom);
            // -------------------------------------------------
            // Giới hạn Latitude của Web Mercator
            // -------------------------------------------------
            if (latitude >
                85.05112878)
            {
                latitude =
                    85.05112878;
            }
            if (latitude <
                -85.05112878)
            {
                latitude =
                    -85.05112878;
            }
            // -------------------------------------------------
            // X
            // -------------------------------------------------
            double x =
                (longitude + 180.0)
                / 360.0
                * mapSize;
            // -------------------------------------------------
            // Y
            // -------------------------------------------------
            double sinLatitude =
                Math.Sin(
                    latitude *
                    Math.PI /
                    180.0);
            double y =
                (
                    0.5
                    -
                    Math.Log(
                        (1.0 + sinLatitude)
                        /
                        (1.0 - sinLatitude)
                    )
                    /
                    (4.0 * Math.PI)
                )
                *
                mapSize;
            return new PointF(
                (float)x,
                (float)y);
        }
        // =====================================================
        // GLOBAL PIXEL → WGS84
        // =====================================================
        private Wgs84Coordinate GlobalPixelToWgs84(
            double pixelX,
            double pixelY,
            int zoom)
        {
            double mapSize =
                TILE_SIZE *
                Math.Pow(
                    2.0,
                    zoom);
            // -------------------------------------------------
            // X → Longitude
            // -------------------------------------------------
            double longitude =
                pixelX /
                mapSize *
                360.0
                - 180.0;
            // -------------------------------------------------
            // Y → Latitude
            // -------------------------------------------------
            double n =
                Math.PI
                -
                2.0 *
                Math.PI *
                pixelY /
                mapSize;
            double latitude =
                180.0 /
                Math.PI *
                Math.Atan(
                    Math.Sinh(n));
            return new Wgs84Coordinate(
                longitude,
                latitude);
        }
    }
}