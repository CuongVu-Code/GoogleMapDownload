using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using GoogleMapDownload.Palette;
using GoogleMapPlugin.Cad;
using GoogleMapPlugin.Data;
using GoogleMapPlugin.Models;
using GoogleMapPlugin.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace GoogleMapPlugin
{
    public partial class GoogleMapControl : UserControl
    {
        private class TileDownloadTestState
        {
            public GoogleMapService Service;
            public List<GoogleTileItem> Tiles;
            public int Zoom;
            public string Layer;
            public GoogleMapRequest Request;
        }
        #region"Khởi tạo các control"
        //====Chọn 2 điểm để xác định vùng cần tải ====
        private Label lblDiem1;
        private Label lblDiem2;

        private TextBox txtDiem1_X;
        private TextBox txtDiem1_Y;
        private TextBox txtDiem2_X;
        private TextBox txtDiem2_Y;

        private Button btnChonVung;
        // ==============================
        // ĐIỂM 1
        // ==============================
        private Label lblPoint1;
        private Label lblX1;
        private Label lblY1;
        private TextBox txtX1;
        private TextBox txtY1;
        private Button btnPick1;
        // ==============================
        // ĐIỂM 2
        // ==============================
        private Label lblPoint2;
        private Label lblX2;
        private Label lblY2;
        private TextBox txtX2;
        private TextBox txtY2;
        private Button btnPick2;
        // ==============================
        // KTT
        // ==============================
        private Label lblKTT;
        private ComboBox cmbKTT;
        // ==============================
        // MAP TYPE
        // ==============================
        private Label lblMapType;
        private ComboBox cmbMapType;
        // ==============================
        // ZOOM
        // ==============================
        private Label lblZoom;
        private NumericUpDown numZoom;
        // ==============================
        // BUTTON
        // ==============================
        //private Label labelProgress;
        private Button btnDownload;
        private Button btnDelete;
        private Button btnCheck;
        private Button btnCheck1;

        private ProgressBarWithText progressBarTile;
        #endregion
        #region"Hàm khởi tạo các control"
        // ==============================
        // CONSTRUCTOR
        // ==============================
        public GoogleMapControl()
        {
            InitializeControl();
            cmbKTT.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbKTT.IntegralHeight = false;
            cmbKTT.DropDown += cmbKTT_DropDown;
            LoadKinhTuyenTruc();
        }
        #endregion
        #region"Khởi tạo giao diện, thêm các control vào Palette"
        // ==============================
        // KHỞI TẠO GIAO DIỆN
        // ==============================
        private void InitializeControl()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = SystemColors.Control;
            this.AutoScroll = true;
            // =================================
            // TITLE
            // =================================
            Label lblTitle = new Label();
            lblTitle.Text = "GOOGLE MAP DOWNLOAD" + "\nCopyright by Vũ Lê Cường";
            lblTitle.ForeColor = Color.BlueViolet;
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 40;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Font = new Font("Arial", 10, FontStyle.Bold);
            this.Controls.Add(lblTitle);
            // =================================
            // PANEL
            // =================================
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(10);
            this.Controls.Add(panel);
            int y = 50;            
            // =================================
            // ĐIỂM 1
            // =================================
            lblPoint1 = new Label();
            lblPoint1.Text = "Toạ độ điểm 1";
            lblPoint1.Location = new Point(40, y);
            lblPoint1.AutoSize = true;
            lblPoint1.Font = new Font("Arial", 9, FontStyle.Bold);
            panel.Controls.Add(lblPoint1);
            // =================================
            // ĐIỂM 2
            // =================================
            lblPoint2 = new Label();
            lblPoint2.Text = "Toạ độ điểm 2";
            lblPoint2.Location = new Point(185, y);
            lblPoint2.AutoSize = true;
            lblPoint2.Font = new Font("Arial", 9, FontStyle.Bold);
            panel.Controls.Add(lblPoint2);
            y += 25;
            // X1
            lblX1 = new Label();
            lblX1.Text = "X1:";
            lblX1.Location = new Point(10, y + 3);
            lblX1.AutoSize = true;
            panel.Controls.Add(lblX1);
            txtX1 = new TextBox();
            txtX1.Location = new Point(40, y+3);
            txtX1.Width = 105;// 260;// 170;
            panel.Controls.Add(txtX1);
            // X2
            lblX2 = new Label();
            lblX2.Text = "X2:";
            lblX2.Location = new Point(155, y + 3);
            lblX2.AutoSize = true;
            panel.Controls.Add(lblX2);
            txtX2 = new TextBox();
            txtX2.Location = new Point(185, y);
            txtX2.Width = 105;//260;// 170;
            panel.Controls.Add(txtX2);
            y += 25;
            // Y1
            lblY1 = new Label();
            lblY1.Text = "Y1:";
            lblY1.Location = new Point(10, y + 3);
            lblY1.AutoSize = true;
            panel.Controls.Add(lblY1);
            txtY1 = new TextBox();
            txtY1.Location = new Point(40, y+3);
            txtY1.Width = 105;// 260;// 170;
            panel.Controls.Add(txtY1);           
            // Y2
            lblY2 = new Label();
            lblY2.Text = "Y2:";
            lblY2.Location = new Point(155, y + 3);
            lblY2.AutoSize = true;
            panel.Controls.Add(lblY2);
            txtY2 = new TextBox();
            txtY2.Location = new Point(185, y);
            txtY2.Width = 105;// 260;// 170;
            panel.Controls.Add(txtY2);     
            y += 40;
            // BUTTON CHỌN VÙNG BẢN DỒ CẦN TẢI
            btnPick1 = new Button();
            btnPick1.Text = "Chọn vùng";
            btnPick1.Location = new Point(10, y);
            btnPick1.Width = 280;// 170;
            btnPick1.Height = 30;
            btnPick1.Click += BtnVungChon_Click;
            panel.Controls.Add(btnPick1);
            y += 30;            
            // =================================
            // KTT
            // =================================
            lblKTT = new Label();
            lblKTT.Text = "Kinh tuyến trục:";
            lblKTT.Location = new Point(10, y);
            lblKTT.AutoSize = true;
            panel.Controls.Add(lblKTT);
            y += 30;
            cmbKTT = new ComboBox();
            cmbKTT.Location = new Point(10, y);
            cmbKTT.Width = 280;
            cmbKTT.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbKTT.DropDown += cmbKTT_DropDown;
            panel.Controls.Add(cmbKTT);
            y += 30;        
        // =================================
        // MAP TYPE
        // =================================
            lblMapType = new Label();
            lblMapType.Text = "Kiểu bản đồ:";
            lblMapType.Location = new Point(10, y);
            lblMapType.AutoSize = true;
            panel.Controls.Add(lblMapType);
            y += 30;
            cmbMapType = new ComboBox();
            cmbMapType.Location = new Point(10, y);
            cmbMapType.Width = 280;
            cmbMapType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMapType.Items.Add("Satellite");
            cmbMapType.Items.Add("Road");
            cmbMapType.Items.Add("Hybrid");
            cmbMapType.Items.Add("Terrain");
            cmbMapType.SelectedIndex = 0;
            panel.Controls.Add(cmbMapType);
            y += 30;
            // =================================
            // ZOOM
            // =================================
            lblZoom = new Label();
            lblZoom.Text = "Google Zoom:";
            lblZoom.Location = new Point(10, y);
            lblZoom.AutoSize = true;
            panel.Controls.Add(lblZoom);
            //y += 30;
            numZoom = new NumericUpDown();
            numZoom.Location = new Point(140, y);
            numZoom.Width = 150;
            numZoom.Minimum = 1;
            numZoom.Maximum = 21;
            numZoom.Value = 17;
            panel.Controls.Add(numZoom);
            y += 30;            
            //Thêm ProgressBar
            progressBarTile = new ProgressBarWithText();
            progressBarTile.Location = new Point(10, y);
            progressBarTile.Width = 280;
            progressBarTile.Height = 25;
            progressBarTile.Minimum = 0;
            progressBarTile.Maximum = 100;
            progressBarTile.Value = 0;
            progressBarTile.Style = ProgressBarStyle.Continuous;
            panel.Controls.Add(progressBarTile);
            y += 30;
            // =================================
            // DOWNLOAD
            // =================================
            btnDownload = new Button();
            btnDownload.Text = "TẢI GOOGLE MAP";
            btnDownload.Location = new Point(10, y);
            btnDownload.Width = 280;
            btnDownload.Height = 30;
            btnDownload.Click += BtnDownload_Click;
            panel.Controls.Add(btnDownload);
            y += 30;
            // =================================
            // DELETE
            // =================================
            btnDelete = new Button();
            btnDelete.Text = "XÓA ẢNH";
            btnDelete.Location = new Point(10, y);
            btnDelete.Width = 280;
            btnDelete.Height = 30;
            panel.Controls.Add(btnDelete);
           
        }
        #endregion
        private void cmbKTT_DropDown(object sender, EventArgs e)
        {
            cmbKTT.Focus();
        }       
        private void BtnDownload_Click(object sender, EventArgs e) //Nút Download
        {            
            StartRealTileDownload();
        }
        // =====================================
        // CHỌN VÙNG BẢN ĐỒ CẦN TẢI
        // =====================================
        private void BtnVungChon_Click(object sender, EventArgs e)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            // Cho AutoCAD nhận focus
            GoogleMapPalette.SetKeepFocus(false);
            try
            {
                PromptPointOptions options1 = new PromptPointOptions("\nChọn điểm 1: ");
                PromptPointResult result1 = ed.GetPoint(options1);
                if (result1.Status != PromptStatus.OK)
                {
                    return;
                }
                txtX1.Text = result1.Value.X.ToString("0.000");
                txtY1.Text = result1.Value.Y.ToString("0.000");              
                double x1 = double.Parse(txtX1.Text);
                double y1 = double.Parse(txtY1.Text);
                Point3d p1 = new Point3d(x1, y1, 0.0);
                PromptPointResult result2 = ed.GetCorner("\nChọn góc đối diện: ", p1);
                if (result2.Status != PromptStatus.OK)
                {
                    return;
                }
                txtX2.Text = result2.Value.X.ToString("0.000");
                txtY2.Text = result2.Value.Y.ToString("0.000");
            }
            finally
            {
                // Trả lại focus cho Palette
                GoogleMapPalette.SetKeepFocus(true);
            }           
        }
            
        private void LoadKinhTuyenTruc()
        {
            cmbKTT.DataSource = KinhTuyenTrucData.GetAll();
            cmbKTT.DisplayMember = "HienThi";
            cmbKTT.ValueMember = "KinhDo";
            cmbKTT.SelectedIndex = 0;
        }        
        private void TileProgress(int completed, int total)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new GoogleMapService.TileProgressHandler(TileProgress), new object[] { completed, total });
                return;
            }
            progressBarTile.Maximum = total > 0 ? total : 100;
            progressBarTile.Value = Math.Min(Math.Max(completed, 0), progressBarTile.Maximum);
            progressBarTile.CustomText = $"Đang tải Tile: {completed} / {total} ({(total > 0 ? completed * 100 / total : 0)}%)";
            progressBarTile.Invalidate(); // ép vẽ lại
        }        
        private void DownloadTileTestWorker(object obj)
        {
            TileDownloadTestState state = (TileDownloadTestState)obj;
            try
            {
                List<GoogleTileDownloadResult> results = state.Service.DownloadTilesParallel(state.Tiles,state.Zoom,state.Layer,8,
                        new GoogleMapService.TileProgressHandler(TileProgress));
                // -----------------------------------------------------
                // LƯU TILE VÀO Ổ CỨNG
                // -----------------------------------------------------
                GoogleTileFileService fileService = new GoogleTileFileService();
                string tileFolder = @"C:\GoogleMap\Tiles";
                int success = fileService.SaveTiles(results,state.Zoom,tileFolder);
                // -----------------------------------------------------
                // TRỞ VỀ UI THREAD
                // -----------------------------------------------------
                this.BeginInvoke(
                    new MethodInvoker(
                        delegate
                        {
                            btnDownload.Enabled = true;
                            progressBarTile.CustomText =
                                "Hoàn thành tải: " +
                                success + "/" +
                                results.Count +
                                " tile. Đang ghép ảnh...";
                            progressBarTile.Invalidate();
                            try
                            {
                                // -----------------------------------------------------
                                // GHÉP TILE → ẢNH LỚN
                                // -----------------------------------------------------
                                string mergedFile = MergeTilesAuto(state.Request);
                                // -----------------------------------------------------
                                // CẮT ẢNH ĐÚNG VÙNG YÊU CẦU
                                // -----------------------------------------------------
                                progressBarTile.CustomText = "Đang cắt ảnh...";
                                progressBarTile.Invalidate();
                                double latMin, lonMin, latMax, lonMax;
                                GoogleTileRange range;
                                string croppedFile = CropTilesAuto(
                                    state.Request, mergedFile,
                                    out latMin, out lonMin, out latMax, out lonMax, out range);
                                // -----------------------------------------------------
                                // TẠO FILE JGW
                                // -----------------------------------------------------
                                progressBarTile.CustomText = "Đang tạo file JGW...";
                                progressBarTile.Invalidate();
                                string jgwFile = CreateJgwAuto(
                                    state.Request, croppedFile, range, latMin, lonMin, latMax, lonMax);
                                // -----------------------------------------------------
                                // CHÈN ẢNH VÀO BẢN VẼ CAD
                                // -----------------------------------------------------
                                progressBarTile.CustomText = "Đang chèn ảnh vào CAD...";
                                progressBarTile.Invalidate();
                                InsertGeoRasterImage.InsertImageWithJgw(croppedFile, jgwFile);
                                progressBarTile.CustomText = "Hoàn tất! Đã chèn ảnh vào CAD.";
                                progressBarTile.Invalidate();
                                MessageBox.Show("Đã tải, ghép, cắt ảnh và chèn vào bản vẽ CAD thành công!\n\n" +
                                    "Tổng tile: " + results.Count + "\n" +
                                    "Thành công: " + success,
                                    "GOOGLE MAP");
                            }
                            catch (Exception exProcess)
                            {
                                progressBarTile.CustomText = "Lỗi khi xử lý sau khi tải ảnh.";
                                progressBarTile.Invalidate();
                                MessageBox.Show(
                                    exProcess.ToString(),
                                    "Lỗi xử lý sau khi tải ảnh");
                            }
                        }));
            }
            catch (Exception ex)
            {
                this.BeginInvoke(new MethodInvoker(
                        delegate
                        {
                            btnDownload.Enabled = true;
                            MessageBox.Show(ex.Message,"Lỗi");
                        }));
            }
        }
        private GoogleMapRequest GetRequestFromUI()
        {
            double x1;
            double y1;
            double x2;
            double y2;
            if (!double.TryParse(txtX1.Text, out x1))
            {
                MessageBox.Show("Tọa độ X1 không hợp lệ.");
                return null;
            }
            if (!double.TryParse(txtY1.Text, out y1))
            {
                MessageBox.Show("Tọa độ Y1 không hợp lệ.");
                return null;
            }
            if (!double.TryParse(txtX2.Text, out x2))
            {
                MessageBox.Show("Tọa độ X2 không hợp lệ.");
                return null;
            }
            if (!double.TryParse(txtY2.Text, out y2))
            {
                MessageBox.Show("Tọa độ Y2 không hợp lệ.");
                return null;
            }
            GoogleMapRequest request = new GoogleMapRequest();
            request.X1 = x1;
            request.Y1 = y1;
            request.X2 = x2;
            request.Y2 = y2;
            request.KinhTuyenTruc = Convert.ToDouble(cmbKTT.SelectedValue);
            request.Zoom = Convert.ToInt32(numZoom.Value);
            request.MapType = cmbMapType.SelectedItem.ToString();
            return request;
        }        
        private void StartRealTileDownload()
        {
            // ==========================================
            // 1. LẤY REQUEST TỪ GIAO DIỆN
            // ==========================================
            GoogleMapRequest request = GetRequestFromUI();
            if (request == null)
                return;
            // ==========================================
            // 2. CHUYỂN VN2000 → WGS84
            // ==========================================
            CoordinateService coordinateService = new CoordinateService();
            Wgs84Coordinate wgs1 = coordinateService.ToWgs84(request.X1,request.Y1,request.KinhTuyenTruc);
            Wgs84Coordinate wgs2 = coordinateService.ToWgs84(request.X2,request.Y2,request.KinhTuyenTruc);
            // ==========================================
            // 3. XÁC ĐỊNH MIN / MAX
            // ==========================================
            double latMin = Math.Min(wgs1.Latitude, wgs2.Latitude);
            double latMax = Math.Max(wgs1.Latitude, wgs2.Latitude);
            double lonMin = Math.Min(wgs1.Longitude, wgs2.Longitude);
            double lonMax = Math.Max(wgs1.Longitude, wgs2.Longitude);
            // ==========================================
            // 4. LẤY DANH SÁCH GOOGLE TILE
            // ==========================================
            GoogleTileService tileService = new GoogleTileService();
            GoogleTileRange range = tileService.GetTileRange(latMin,lonMin,latMax,lonMax,request.Zoom);
            List<GoogleTileItem> tiles =  tileService.GetTileList(range);
            if (tiles == null || tiles.Count == 0)
            {
                MessageBox.Show("Không xác định được Google Tile.","Google Map",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }
            // ==========================================
            // 5. LẤY LAYER
            // ==========================================
            GoogleMapService service = new GoogleMapService();
            string layer =  service.GetLayer(request.MapType);
            // ==========================================
            // 6. KHÓA NÚT DOWNLOAD
            // ==========================================
            btnDownload.Enabled = false;
            progressBarTile.CustomText= "Chuẩn bị tải " + tiles.Count + " tile...";
            // ==========================================
            // 7. TẠO STATE
            // ==========================================
            TileDownloadTestState state =  new TileDownloadTestState();
            state.Service = service;
            state.Tiles = tiles;
            state.Zoom = request.Zoom;
            state.Layer = layer;
            state.Request = request;
            // ==========================================
            // 8. CHẠY BACKGROUND
            // ==========================================
            ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadTileTestWorker),state);
        }        
        private void TestJGW()
        {
            try
            {
                GoogleMapRequest request = GetRequestFromUI();
                if (request == null)
                {
                    return;
                }
                // =============================================
                // 1. Chuyển 2 điểm VN2000 → WGS84
                // =============================================
                CoordinateService coordinateService = new CoordinateService();
                Wgs84Coordinate wgs1 = coordinateService.ToWgs84(request.X1, request.Y1, request.KinhTuyenTruc);
                Wgs84Coordinate wgs2 = coordinateService.ToWgs84(request.X2, request.Y2, request.KinhTuyenTruc);
                double latMin = Math.Min(wgs1.Latitude, wgs2.Latitude);
                double latMax = Math.Max(wgs1.Latitude, wgs2.Latitude);
                double lonMin = Math.Min(wgs1.Longitude, wgs2.Longitude);
                double lonMax = Math.Max(wgs1.Longitude, wgs2.Longitude);
                // =============================================
                // 2. Xác định Tile Range
                // =============================================
                GoogleTileService tileService = new GoogleTileService();
                GoogleTileRange range = tileService.GetTileRange(latMin, lonMin, latMax, lonMax, request.Zoom);
                // =============================================
                // 3. File ảnh Crop
                // =============================================
                string imageFile = @"C:\GoogleMap\google_map_test.jpg";
                if (!File.Exists(imageFile))
                {
                    MessageBox.Show("Không tìm thấy ảnh:\n\n" + imageFile + "\n\n" + "Hãy chạy TEST CROP trước.",
                        "TEST JGW");
                    return;
                }
                // =============================================
                // 4. File JGW
                // =============================================
                string jgwFile = @"C:\GoogleMap\google_map_test.jgw";
                // =============================================
                // 5. Tạo JGW
                // =============================================
                GoogleWorldFileService worldFileService = new GoogleWorldFileService();
                string result = worldFileService.CreateJgw(imageFile, jgwFile, range, latMin, lonMin, latMax, lonMax, request.Zoom, request.KinhTuyenTruc);
                // =============================================
                // 6. Đọc lại file JGW
                // =============================================
                string[] lines = File.ReadAllLines(result);
                if (lines.Length != 6)
                {
                    MessageBox.Show("File JGW không có đúng 6 dòng.", "TEST JGW");
                    return;
                }
                string message = "Tạo JGW thành công!\n\n" +
                    "File:\n" + result + "\n\n" +
                    "6 dòng JGW:\n\n" +
                    "1: " + lines[0] + "\n" +
                    "2: " + lines[1] + "\n" +
                    "3: " + lines[2] + "\n" +
                    "4: " + lines[3] + "\n" +
                    "5: " + lines[4] + "\n" +
                    "6: " + lines[5];
                MessageBox.Show(message, "TEST JGW");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Lỗi TEST JGW");
            }
        }
        // =========================================================
        // GHÉP TILE → ẢNH LỚN (dùng cho luồng tự động sau khi tải)
        // =========================================================
        private string MergeTilesAuto(GoogleMapRequest request)
        {
            CoordinateService coordinateService = new CoordinateService();
            Wgs84Coordinate wgs1 = coordinateService.ToWgs84(request.X1, request.Y1, request.KinhTuyenTruc);
            Wgs84Coordinate wgs2 = coordinateService.ToWgs84(request.X2, request.Y2, request.KinhTuyenTruc);
            double latMin = Math.Min(wgs1.Latitude, wgs2.Latitude);
            double latMax = Math.Max(wgs1.Latitude, wgs2.Latitude);
            double lonMin = Math.Min(wgs1.Longitude, wgs2.Longitude);
            double lonMax = Math.Max(wgs1.Longitude, wgs2.Longitude);

            GoogleTileService tileService = new GoogleTileService();
            GoogleTileRange range = tileService.GetTileRange(latMin, lonMin, latMax, lonMax, request.Zoom);
            List<GoogleTileItem> tiles = tileService.GetTileList(range);
            if (tiles == null || tiles.Count == 0)
            {
                throw new Exception("Không xác định được Google Tile để ghép.");
            }
            GoogleTileMergeService mergeService = new GoogleTileMergeService();
            string tileFolder = @"C:\GoogleMap\Tiles";
            string outputFile = @"C:\GoogleMap\merged_test.jpg";
            mergeService.MergeTiles(tiles, request.Zoom, tileFolder, outputFile);
            return outputFile;
        }
        // =========================================================
        // CẮT ẢNH ĐÚNG VÙNG YÊU CẦU (dùng cho luồng tự động)
        // =========================================================
        private string CropTilesAuto(
            GoogleMapRequest request,
            string mergedFile,
            out double latMin,
            out double lonMin,
            out double latMax,
            out double lonMax,
            out GoogleTileRange range)
        {
            CoordinateService coordinateService = new CoordinateService();
            Wgs84Coordinate wgs1 = coordinateService.ToWgs84(request.X1, request.Y1, request.KinhTuyenTruc);
            Wgs84Coordinate wgs2 = coordinateService.ToWgs84(request.X2, request.Y2, request.KinhTuyenTruc);
            latMin = Math.Min(wgs1.Latitude, wgs2.Latitude);
            latMax = Math.Max(wgs1.Latitude, wgs2.Latitude);
            lonMin = Math.Min(wgs1.Longitude, wgs2.Longitude);
            lonMax = Math.Max(wgs1.Longitude, wgs2.Longitude);
            GoogleTileService tileService = new GoogleTileService();
            range = tileService.GetTileRange(latMin, lonMin, latMax, lonMax, request.Zoom);
            List<GoogleTileItem> tiles = tileService.GetTileList(range);
            if (tiles == null || tiles.Count == 0)
            {
                throw new Exception("Không có Tile để cắt.");
            }
            if (!File.Exists(mergedFile))
            {
                throw new Exception("Không tìm thấy ảnh Merge:\n" + mergedFile);
            }
            GoogleTileCropService cropService = new GoogleTileCropService();
            string outputFile = @"C:\GoogleMap\google_map_test.jpg";
            cropService.CropImage(mergedFile, outputFile, range, latMin, lonMin, latMax, lonMax, request.Zoom);
            return outputFile;
        }
        // =========================================================
        // TẠO FILE JGW (dùng cho luồng tự động)
        // =========================================================
        private string CreateJgwAuto(
            GoogleMapRequest request,
            string croppedFile,
            GoogleTileRange range,
            double latMin,
            double lonMin,
            double latMax,
            double lonMax)        {
            string jgwFile = @"C:\GoogleMap\google_map_test.jgw";
            GoogleWorldFileService worldFileService = new GoogleWorldFileService();
            string result = worldFileService.CreateJgw(
                croppedFile, jgwFile, range, latMin, lonMin, latMax, lonMax, request.Zoom, request.KinhTuyenTruc);
            string[] lines = File.ReadAllLines(result);
            if (lines.Length != 6)
            {
                throw new Exception("File JGW không có đúng 6 dòng.");
            }
            return result;
        }           
        // =========================================================
        // ĐỌC GIÁ TRỊ JGW
        // =========================================================
        private double ParseJgwValue(string text)
        {
            return Convert.ToDouble(text.Trim(),CultureInfo.InvariantCulture);
        }
        // =========================================================
        // ÁP DỤNG JGW
        //
        // X = A * pixelX + B * pixelY + C
        // Y = D * pixelX + E * pixelY + F
        // =========================================================
        private Vn2000Coordinate ApplyJgw(double pixelX,double pixelY,double A,double B,double C,double D,double E,double F)
        {
            double x = A * pixelX + B * pixelY + C;
            double y = D * pixelX + E * pixelY + F;
            return new Vn2000Coordinate(x,y);
        }
        // =========================================================
        // TÍNH SAI SỐ
        // =========================================================
        private double CalculateError(Vn2000Coordinate realPoint,Vn2000Coordinate jgwPoint)
        {
            double dx = jgwPoint.X - realPoint.X;
            double dy = jgwPoint.Y - realPoint.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        // =========================================================
        // WGS84 → GLOBAL PIXEL
        // Google Web Mercator
        // =========================================================
        private PointF LatLonToGlobalPixelForTest(double latitude,double longitude,int zoom)
        {
            const int TILE_SIZE_TEST = 256;
            double mapSize = TILE_SIZE_TEST * Math.Pow(2.0,zoom);
            if (latitude > 85.05112878)
            {
                latitude =  85.05112878;
            }
            if (latitude < -85.05112878)
            {
                latitude = -85.05112878;
            }
            double x =(longitude + 180.0)/ 360.0 * mapSize;
            double sinLatitude = Math.Sin(latitude * Math.PI /180.0);
            double y =(0.5- Math.Log((1.0 + sinLatitude)/(1.0 - sinLatitude))/(4.0 * Math.PI)) * mapSize;
            return new PointF((float)x, (float)y);
        }
        // =========================================================
        // GLOBAL PIXEL → WGS84
        // =========================================================
        private Wgs84Coordinate GlobalPixelToWgs84ForTest(double pixelX,double pixelY,int zoom)
        {
            const int TILE_SIZE_TEST = 256;
            double mapSize = TILE_SIZE_TEST * Math.Pow(2.0,zoom);
            double longitude = pixelX / mapSize * 360.0 - 180.0;
            double n = Math.PI- 2.0 * Math.PI * pixelY / mapSize;
            double latitude = 180.0 /Math.PI * Math.Atan(Math.Sinh(n));
            return new Wgs84Coordinate(longitude,latitude);
        }
               
    }

}