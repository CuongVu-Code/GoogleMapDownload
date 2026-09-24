using System;
using System.Globalization;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

namespace GoogleMapPlugin.Cad
{
    public static class InsertGeoRasterImage
    {
        /// <summary>
        /// Chèn ảnh vào AutoCAD tại đúng vị trí/tỉ lệ/góc xoay theo file JGW đi kèm.
        /// </summary>
        /// <param name="imagePath">Đường dẫn ảnh (.jpg/.png...)</param>
        /// <param name="jgwPath">Đường dẫn file World File 6 dòng (A,D,B,E,C,F)</param>
        [CommandMethod("INSERTGEOIMAGE")]
        public static void InsertImageWithJgwCommand()
        {
            // Ví dụ gọi thủ công, chỉnh lại đường dẫn thực tế
            InsertImageWithJgw(@"C:\GoogleMap\google_map_test.jpg",@"C:\GoogleMap\google_map_test.jgw");
        }
        public static void InsertImageWithJgw(string imagePath, string jgwPath)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            if (!File.Exists(imagePath))
            {
                ed.WriteMessage("\nKhông tìm thấy ảnh: " + imagePath);
                return;
            }
            if (!File.Exists(jgwPath))
            {
                ed.WriteMessage("\nKhông tìm thấy file JGW: " + jgwPath);
                return;
            }
            // =================================================
            // 1. Đọc 6 tham số JGW (thứ tự chuẩn: A, D, B, E, C, F)
            // =================================================
            string[] lines = File.ReadAllLines(jgwPath);
            if (lines.Length != 6)
            {
                ed.WriteMessage("\nFile JGW phải có đúng 6 dòng.");
                return;
            }
            double A = ParseJgwValue(lines[0]);
            double D = ParseJgwValue(lines[1]);
            double B = ParseJgwValue(lines[2]);
            double E = ParseJgwValue(lines[3]);
            double C = ParseJgwValue(lines[4]);
            double F = ParseJgwValue(lines[5]);
            // =================================================
            // 2. Đọc kích thước ảnh (pixel)
            // =================================================
            int width, height;
            using (var bmp = new System.Drawing.Bitmap(imagePath))
            {
                width = bmp.Width;
                height = bmp.Height;
            }
            // =================================================
            // 3. Quy đổi JGW (tâm pixel) -> Origin/Xaxis/Yaxis (cạnh ảnh)
            // =================================================
            double originX = A * (-0.5) + B * (height - 0.5) + C;
            double originY = D * (-0.5) + E * (height - 0.5) + F;
            Vector3d xAxis = new Vector3d(A * width, D * width, 0);
            Vector3d yAxis = new Vector3d(-B * height, -E * height, 0);
            // Bắt buộc khoá document: hàm này được gọi từ sự kiện UI của
            // Palette (modeless), không chạy trong ngữ cảnh [CommandMethod]
            // nên AutoCAD không tự khoá document giúp mình.
            using (DocumentLock docLock = doc.LockDocument())
            using (Transaction tr = db.TransactionManager.StartTransaction())            {
                // -------------------------------------------------
                // 4. Lấy / tạo Image Dictionary
                // -------------------------------------------------
                ObjectId imageDictId = RasterImageDef.GetImageDictionary(db);
                if (imageDictId == ObjectId.Null)
                {
                    imageDictId = RasterImageDef.CreateImageDictionary(db);
                }
                DBDictionary imageDict =
                    (DBDictionary)tr.GetObject(imageDictId, OpenMode.ForWrite);
                // -------------------------------------------------
                // 5. Tạo RasterImageDef trỏ tới file ảnh
                // -------------------------------------------------
                RasterImageDef imgDef = new RasterImageDef();
                imgDef.SourceFileName = imagePath;
                imgDef.Load();
                string imgName = Path.GetFileNameWithoutExtension(imagePath);
                if (imageDict.Contains(imgName))
                {
                    imgName += "_" + Guid.NewGuid().ToString("N").Substring(0, 6);
                }
                ObjectId imgDefId = imageDict.SetAt(imgName, imgDef);
                tr.AddNewlyCreatedDBObject(imgDef, true);
                // -------------------------------------------------
                // 6. Tạo entity RasterImage với Orientation từ JGW
                // -------------------------------------------------
                RasterImage rasterImage = new RasterImage();
                rasterImage.ImageDefId = imgDefId;
                rasterImage.Orientation = new CoordinateSystem3d(
                    new Point3d(originX, originY, 0),
                    xAxis,
                    yAxis);
                rasterImage.DisplayOptions = ImageDisplayOptions.Show;
                BlockTable bt =
                    (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr =
                    (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                btr.AppendEntity(rasterImage);
                tr.AddNewlyCreatedDBObject(rasterImage, true);
                // Đưa ảnh xuống dưới cùng
                DrawOrderTable drawOrderTable =(DrawOrderTable)tr.GetObject(btr.DrawOrderTableId, OpenMode.ForWrite);
                ObjectIdCollection ids = new ObjectIdCollection();
                ids.Add(rasterImage.ObjectId);
                drawOrderTable.MoveToBottom(ids);
                // -------------------------------------------------
                // 7. Liên kết reactor giữa entity và định nghĩa ảnh
                //    (bắt buộc để ảnh hiển thị và cập nhật đúng)
                // -------------------------------------------------
                RasterImage.EnableReactors(true);
                rasterImage.AssociateRasterDef(imgDef);
                tr.Commit();
                ed.Regen();
            }
            ed.WriteMessage("\nĐã chèn ảnh georeferenced vào CAD thành công (" +
                             width + "x" + height + " px).");
        }
        private static double ParseJgwValue(string line)
        {
            return double.Parse(line.Trim(), CultureInfo.InvariantCulture);
        }
    }
}
