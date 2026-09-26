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
        // Tên RegApp dùng để đánh dấu (XData) các ảnh do chính plugin này
        // chèn vào bản vẽ -> giúp DetachAllGoogleMapImages() chỉ gỡ đúng
        // ảnh Google Map, không đụng tới ảnh raster khác người dùng tự chèn.
        private const string AppName = "GOOGLEMAP_PLUGIN";

        /// <summary>
        /// Chèn ảnh vào AutoCAD tại đúng vị trí/tỉ lệ/góc xoay theo file JGW đi kèm.
        /// </summary>
        /// <param name="imagePath">Đường dẫn ảnh (.jpg/.png...)</param>
        /// <param name="jgwPath">Đường dẫn file World File 6 dòng (A,D,B,E,C,F)</param>
        [CommandMethod("INSERTGEOIMAGE")]
        public static void InsertImageWithJgwCommand()
        {
            // Ví dụ gọi thủ công, chỉnh lại đường dẫn thực tế
            InsertImageWithJgw(@"C:\GoogleMap\google_map_test.jpg",
                                @"C:\GoogleMap\google_map_test.jgw");
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
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // -------------------------------------------------
                // 3b. Đảm bảo RegApp "GOOGLEMAP_PLUGIN" tồn tại, để có thể
                //     gắn XData đánh dấu ảnh (dùng cho DetachAllGoogleMapImages)
                // -------------------------------------------------
                RegAppTable regTable = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
                if (!regTable.Has(AppName))
                {
                    regTable.UpgradeOpen();
                    RegAppTableRecord regApp = new RegAppTableRecord();
                    regApp.Name = AppName;
                    regTable.Add(regApp);
                    tr.AddNewlyCreatedDBObject(regApp, true);
                }

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

                // Gắn XData đánh dấu: entity này do plugin Google Map chèn,
                // kèm đường dẫn ảnh gốc để tiện tra cứu/kiểm tra sau này.
                rasterImage.XData = new ResultBuffer(
                    new TypedValue((int)DxfCode.ExtendedDataRegAppName, AppName),
                    new TypedValue((int)DxfCode.ExtendedDataAsciiString, imagePath));

                BlockTable bt =
                    (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr =
                    (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                btr.AppendEntity(rasterImage);
                tr.AddNewlyCreatedDBObject(rasterImage, true);

                // -------------------------------------------------
                // 7. Liên kết reactor giữa entity và định nghĩa ảnh
                //    (bắt buộc để ảnh hiển thị và cập nhật đúng)
                // -------------------------------------------------
                RasterImage.EnableReactors(true);
                rasterImage.AssociateRasterDef(imgDef);

                tr.Commit();
            }

            ed.WriteMessage("\nĐã chèn ảnh georeferenced vào CAD thành công (" +
                             width + "x" + height + " px).");
        }

        /// <summary>
        /// Đếm thủ công số entity RasterImage còn tham chiếu tới 1
        /// RasterImageDef, quét toàn bộ Database (không chỉ Model Space).
        /// Không dùng RasterImageDef.NumberOfObjects vì thuộc tính này
        /// không tồn tại trong AcDbMgd.dll của AutoCAD 2007 -- cách đếm
        /// thủ công này hoạt động thống nhất trên mọi phiên bản.
        /// </summary>
        private static int CountRasterImageReferences(Transaction tr, Database db, ObjectId defId)
        {
            int count = 0;
            BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);

            foreach (ObjectId btrId in bt)
            {
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                foreach (ObjectId entId in btr)
                {
                    if (entId.IsErased)
                    {
                        continue;
                    }

                    RasterImage ri = tr.GetObject(entId, OpenMode.ForRead) as RasterImage;
                    if (ri != null && ri.ImageDefId == defId)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static double ParseJgwValue(string line)
        {
            return double.Parse(line.Trim(), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gỡ (detach) toàn bộ ảnh Google Map đã chèn vào bản vẽ đang mở
        /// từ trước tới nay: xoá entity RasterImage + RasterImageDef tương
        /// ứng khỏi bản vẽ. KHÔNG xoá file .jpg/.jgw trên đĩa.
        /// Chỉ gỡ đúng ảnh do chính plugin này chèn (nhận diện qua XData
        /// RegApp "GOOGLEMAP_PLUGIN"), không đụng tới ảnh raster khác.
        /// </summary>
        /// <returns>Số lượng ảnh đã gỡ.</returns>
        [CommandMethod("GGMAP_DETACHALL")]
        public static int DetachAllGoogleMapImages()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                throw new System.Exception("Không có bản vẽ CAD nào đang mở.");
            }

            Database db = doc.Database;
            Editor ed = doc.Editor;
            int removedCount = 0;

            using (DocumentLock docLock = doc.LockDocument())
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)tr.GetObject(
                    bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                // Gom trước danh sách entity cần xoá (không xoá ngay trong
                // lúc duyệt foreach để tránh ảnh hưởng tới việc lặp).
                System.Collections.Generic.List<ObjectId> entitiesToErase =
                    new System.Collections.Generic.List<ObjectId>();
                System.Collections.Generic.List<ObjectId> imageDefIds =
                    new System.Collections.Generic.List<ObjectId>();

                foreach (ObjectId entId in btr)
                {
                    RasterImage rasterImage = tr.GetObject(entId, OpenMode.ForRead) as RasterImage;
                    if (rasterImage == null)
                    {
                        continue;
                    }

                    ResultBuffer xdata = rasterImage.GetXDataForApplication(AppName);
                    if (xdata == null)
                    {
                        continue; // Không phải ảnh do plugin này chèn -> bỏ qua
                    }
                    xdata.Dispose();

                    entitiesToErase.Add(entId);
                    if (!imageDefIds.Contains(rasterImage.ImageDefId))
                    {
                        imageDefIds.Add(rasterImage.ImageDefId);
                    }
                }

                // Xoá entity RasterImage trước
                foreach (ObjectId entId in entitiesToErase)
                {
                    RasterImage rasterImage = (RasterImage)tr.GetObject(entId, OpenMode.ForWrite);
                    rasterImage.Erase(true);
                    removedCount++;
                }

                // Xoá RasterImageDef không còn entity nào tham chiếu nữa
                foreach (ObjectId defId in imageDefIds)
                {
                    if (defId.IsErased)
                    {
                        continue;
                    }

                    RasterImageDef def = tr.GetObject(defId, OpenMode.ForRead) as RasterImageDef;
                    if (def != null && CountRasterImageReferences(tr, db, defId) == 0)
                    {
                        def.UpgradeOpen();
                        def.Erase(true);
                    }
                }

                tr.Commit();
            }

            if (ed != null)
            {
                ed.WriteMessage("\nĐã gỡ " + removedCount + " ảnh Google Map khỏi bản vẽ.");
            }

            return removedCount;
        }
    }
}
