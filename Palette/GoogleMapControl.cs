using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
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
        }
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
        private Label labelProgress;
        private Button btnDownload;
        private Button btnDelete;
        private Button btnCheck;
        private Button btnCheck1;
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
            lblPoint1.Text = "ĐIỂM 1";
            lblPoint1.Location = new Point(40, y);
            lblPoint1.AutoSize = true;
            lblPoint1.Font = new Font("Arial", 9, FontStyle.Bold);
            panel.Controls.Add(lblPoint1);
            // =================================
            // ĐIỂM 2
            // =================================
            lblPoint2 = new Label();
            lblPoint2.Text = "ĐIỂM 2";
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
            // BUTTON PICK 1
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
            lblMapType.Text = "Loại bản đồ:";
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
            //ADD LABLE PROGRESS
            labelProgress = new Label();
            labelProgress.Location = new Point(10, y);
            labelProgress.AutoSize = true;
            panel.Controls.Add(labelProgress);
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
            y += 50;
            // =================================
            // DELETE
            // =================================
            btnDelete = new Button();
            btnDelete.Text = "XÓA ẢNH";
            btnDelete.Location = new Point(10, y);
            btnDelete.Width = 280;
            btnDelete.Height = 30;
            panel.Controls.Add(btnDelete);
            // =================================
            // TEST
            // =================================
            btnCheck = new Button();
            btnCheck.Text = "GOOGLE CORDINATE";
            btnCheck.Location = new Point(10, y + 45);
            btnCheck.Width = 125;//170;
            btnCheck.Click += btnTestJgwAccuracy_Click;
            panel.Controls.Add(btnCheck);
            // =================================
            // TEST1
            // =================================
            btnCheck1 = new Button();
            btnCheck1.Text = "GOOGLE TILE";
            btnCheck1.Location = new Point(145, y + 45);
            btnCheck1.Width = 125;//170;
            btnCheck1.Click += BtnCheck1_Click;
            panel.Controls.Add(btnCheck1);
        }
        private void cmbKTT_DropDown(object sender, EventArgs e)
        {
            cmbKTT.Focus();
        }       
        private void BtnDownload_Click(object sender, EventArgs e) //Nút Download
        {
            //StartTileDownload();
            //TestRequestToTile();
            StartRealTileDownload();
        }
        // =====================================
        // CHỌN ĐIỂM 1
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
                
        //Nút test
        private void BtnCheck_Click(object sender, EventArgs e)
        {
            //TestCoordinate();
            //TestTileList();
            TestMergeTiles();
            TestCropTiles();
            //TestJGW();
            
        }
        private void BtnCheck1_Click(object sender, EventArgs e)
        {
            //TestGoogleTile();
            //TestDownloadTile();
            btnTestCoordinate();
        }
        private void LoadKinhTuyenTruc()
        {
            cmbKTT.DataSource = KinhTuyenTrucData.GetAll();
            cmbKTT.DisplayMember = "HienThi";
            cmbKTT.ValueMember = "KinhDo";
            cmbKTT.SelectedIndex = 0;
        }
        //HÀM KIỂM TRA TOẠ ĐỘ VN2000 sang WGS84
        private void TestCoordinate() //Ham test Toa do
        {
            CoordinateService service = new CoordinateService();
            Wgs84Coordinate result = service.ToWgs84(574108.652052, 2363981.765218, 105.0);
            MessageBox.Show("VN2000\n\n" + "X = 574108.652052\n" + "Y = 2363981.765218\n" + "KTT = 105.000000°\n\n" + "WGS84\n\n" + "Longitude = " + result.Longitude.ToString("0.000000000000") + "\nLatitude = " + result.Latitude.ToString("0.000000000000"), "TEST VN2000 → WGS84");
        }
        //HÀM KIỂM TRA GOOGLE TILE
        private void TestGoogleTile()
        {
            GoogleTileService service = new GoogleTileService();
            double longitude = 105.716500243568;
            double latitude = 21.369075499011;
            int zoom = 17;
            GoogleTileCoordinate result = service.LatLonToTile(latitude, longitude, zoom);
            MessageBox.Show(
                "WGS84\n\n" +
                "Longitude = " + longitude.ToString("0.000000000000") +
                "\nLatitude = " + latitude.ToString("0.000000000000") +
                "\n\nZoom = " + zoom +
                "\n\nGoogle Tile\n\n" +
                "Tile X = " + result.TileX +
                "\nTile Y = " + result.TileY +
                "\n\nGlobal Pixel\n\n" +
                "Pixel X = " + result.PixelX.ToString("0.000000") +
                "\nPixel Y = " + result.PixelY.ToString("0.000000"),
                "TEST WGS84 → GOOGLE TILE");
        }
        //Test download Tile
        private void TestDownloadTile()
        {
            GoogleMapService service = new GoogleMapService();
            int tileX = 104026;
            int tileY = 57568;
            int zoom = 17;
            string layer = service.GetLayer("satellite");
            byte[] data = service.DownloadTile(tileX, tileY, zoom, layer);
            if (data == null)
            {
                MessageBox.Show("Tải tile thất bại.", "TEST TILE");
                return;
            }
            string file = @"C:\GoogleMap\test_tile.jpg";
            File.WriteAllBytes(file, data);
            MessageBox.Show(
                "Tải tile thành công!\n\n" +
                "Tile X = " + tileX + "\n" +
                "Tile Y = " + tileY + "\n" +
                "Zoom = " + zoom + "\n" +
                "Layer = " + layer + "\n\n" +
                "File:\n" + file,
                "TEST DOWNLOAD TILE");
        }
        private void TestTileList()
        {
            GoogleTileService service = new GoogleTileService();
            // Ví dụ WGS84
            double latMin = 21.35;
            double latMax = 21.37;
            double lonMin = 105.70;
            double lonMax = 105.72;
            int zoom = 17;
            GoogleTileRange range = service.GetTileRange(latMin, lonMin, latMax, lonMax, zoom);
            List<GoogleTileItem> tiles = service.GetTileList(range);
            string message = "GOOGLE TILE RANGE\n\n" +
                "Min X = " + range.MinX +
                "\nMax X = " + range.MaxX +
                "\nMin Y = " + range.MinY +
                "\nMax Y = " + range.MaxY +
                "\n\nWidth = " + range.Width +
                "\nHeight = " + range.Height +
                "\nTotal Tiles = " + range.TotalTiles;
            MessageBox.Show(message, "TEST TILE LIST");
        }
        ///
        private void TestDownloadTiles()
        {
            GoogleMapService service = new GoogleMapService();
            // ---------------------------------------------------------
            // TEST TILE
            // ---------------------------------------------------------
            List<GoogleTileItem> tiles = new List<GoogleTileItem>();
            tiles.Add(new GoogleTileItem(104026, 57568));
            tiles.Add(new GoogleTileItem(104027, 57568));
            tiles.Add(new GoogleTileItem(104026, 57569));
            tiles.Add(new GoogleTileItem(104027, 57569));
            // ---------------------------------------------------------
            // DOWNLOAD
            // ---------------------------------------------------------
            string layer = service.GetLayer("satellite");
            List<GoogleTileDownloadResult> results = service.DownloadTilesParallel(tiles, 17, layer, 8, TileProgress);
        }
        private void TileProgress(int completed, int total)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new GoogleMapService.TileProgressHandler(TileProgress), new object[] { completed, total });
                return;
            }
            int percent = 0;
            if (total > 0)
            {
                percent = completed * 100 / total;
            }
            labelProgress.Text = "Tile: " + completed + " / " + total + " (" + percent + "%)";
        }
        private void StartTileDownload()
        {
            GoogleMapService service = new GoogleMapService();
            List<GoogleTileItem> tiles = new List<GoogleTileItem>();
            // ---------------------------------------------------------
            // TEST 4 TILE
            // ---------------------------------------------------------
            tiles.Add(new GoogleTileItem(104026, 57568));
            tiles.Add(new GoogleTileItem(104027, 57568));
            tiles.Add(new GoogleTileItem(104026, 57569));
            tiles.Add(new GoogleTileItem(104027, 57569));
            string layer = service.GetLayer("satellite");
            TileDownloadTestState state = new TileDownloadTestState();
            state.Service = service;
            state.Tiles = tiles;
            state.Zoom = 17;
            state.Layer = layer;
            ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadTileTestWorker), state);
        }
        private void DownloadTileTestWorker(object obj)
        {
            TileDownloadTestState state = (TileDownloadTestState)obj;
            try
            {
                List<GoogleTileDownloadResult> results =
                    state.Service.DownloadTilesParallel(
                        state.Tiles,
                        state.Zoom,
                        state.Layer,
                        8,
                        new GoogleMapService.TileProgressHandler(
                            TileProgress));
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
                            labelProgress.Text =
                                "Hoàn thành: " +
                                success + "/" +
                                results.Count +
                                " tile";
                            MessageBox.Show(
                                "Đã tải xong!\n\n" +
                                "Tổng tile: " +
                                results.Count +
                                "\n" +
                                "Thành công: " +
                                success,
                                "GOOGLE MAP");
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
        private void TestRequestToTile()
        {
            GoogleMapRequest request = GetRequestFromUI();
            if (request == null)
                return;
            MessageBox.Show("X1 = " + request.X1 + "\nY1 = " + request.Y1 + "\n\nX2 = " + request.X2 +
                "\nY2 = " + request.Y2 + "\n\nKTT = " + request.KinhTuyenTruc +
                "\nZoom = " + request.Zoom + "\nMapType = " + request.MapType,"TEST GOOGLE MAP REQUEST");
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
            labelProgress.Text = "Chuẩn bị tải " + tiles.Count + " tile...";
            // ==========================================
            // 7. TẠO STATE
            // ==========================================
            TileDownloadTestState state =  new TileDownloadTestState();
            state.Service = service;
            state.Tiles = tiles;
            state.Zoom = request.Zoom;
            state.Layer = layer;
            // ==========================================
            // 8. CHẠY BACKGROUND
            // ==========================================
            ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadTileTestWorker),state);
        }
        //Test merge tile
        private void TestMergeTiles()
        {
            try
            {
                // -----------------------------------------
                // 1. Lấy Request
                // -----------------------------------------
                GoogleMapRequest request = GetRequestFromUI();
                if (request == null)
                {
                    return;
                }
                // -----------------------------------------
                // 2. VN2000 → WGS84
                // -----------------------------------------
                CoordinateService coordinateService = new CoordinateService();
                Wgs84Coordinate wgs1 = coordinateService.ToWgs84(request.X1,request.Y1,request.KinhTuyenTruc);
                Wgs84Coordinate wgs2 = coordinateService.ToWgs84(request.X2,request.Y2,request.KinhTuyenTruc);
                double latMin = Math.Min(wgs1.Latitude,wgs2.Latitude);
                double latMax = Math.Max(wgs1.Latitude,wgs2.Latitude);
                double lonMin = Math.Min(wgs1.Longitude,wgs2.Longitude);
                double lonMax = Math.Max(wgs1.Longitude, wgs2.Longitude);
                // -----------------------------------------
                // 3. Tile Range
                // -----------------------------------------
                GoogleTileService tileService = new GoogleTileService();
                GoogleTileRange range = tileService.GetTileRange(latMin,lonMin,latMax,lonMax,request.Zoom);
                List<GoogleTileItem> tiles = tileService.GetTileList(range);
                if (tiles == null || tiles.Count == 0)
                {
                    MessageBox.Show("Không có Tile.", "Google Map");
                    return;
                }
                // -----------------------------------------
                // 4. Merge
                // -----------------------------------------
                GoogleTileMergeService mergeService = new GoogleTileMergeService();
                string tileFolder = @"C:\GoogleMap\Tiles";
                string outputFile = @"C:\GoogleMap\merged_test.jpg";
                string result = mergeService.MergeTiles(tiles, request.Zoom,tileFolder,outputFile);
                // -----------------------------------------
                // 5. Thông báo
                // -----------------------------------------
                MessageBox.Show("Merge thành công!\n\n" +
                    "Tile Range:\n" + "X: " +
                    range.MinX + " → " +
                    range.MaxX + "\n" + "Y: " +
                    range.MinY + " → " +
                    range.MaxY + "\n\n" + "Tổng Tile: " +
                    tiles.Count + "\n\n" + "File:\n" +
                    result,"GOOGLE MAP");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),"Lỗi Merge");
            }
        }
        private void TestCropTiles()
        {
            try
            {
                // -----------------------------------------
                // 1. Lấy thông tin từ giao diện
                // -----------------------------------------
                GoogleMapRequest request =  GetRequestFromUI();
                if (request == null)
                {
                    return;
                }
                // -----------------------------------------
                // 2. VN2000 → WGS84
                // -----------------------------------------
                CoordinateService coordinateService = new CoordinateService();
                Wgs84Coordinate wgs1 = coordinateService.ToWgs84(request.X1,request.Y1,request.KinhTuyenTruc);
                Wgs84Coordinate wgs2 = coordinateService.ToWgs84(request.X2,request.Y2, request.KinhTuyenTruc);
                // -----------------------------------------
                // 3. Xác định Lat/Lon Min-Max
                // -----------------------------------------
                double latMin = Math.Min(wgs1.Latitude,wgs2.Latitude);
                double latMax = Math.Max(wgs1.Latitude,wgs2.Latitude);
                double lonMin = Math.Min(wgs1.Longitude, wgs2.Longitude);
                double lonMax = Math.Max(wgs1.Longitude,wgs2.Longitude);
                // -----------------------------------------
                // 4. Xác định Tile Range
                // -----------------------------------------
                GoogleTileService tileService = new GoogleTileService();
                GoogleTileRange range = tileService.GetTileRange(latMin,lonMin,latMax,lonMax,request.Zoom);
                // -----------------------------------------
                // 5. Tạo danh sách Tile
                // -----------------------------------------
                List<GoogleTileItem> tiles = tileService.GetTileList(range);
                if (tiles == null || tiles.Count == 0)
                {
                    MessageBox.Show("Không có Tile.","Google Map");
                    return;
                }
                // -----------------------------------------
                // 6. Đường dẫn ảnh Merge
                // -----------------------------------------
                string mergedFile = @"C:\GoogleMap\merged_test.jpg";
                if (!File.Exists(mergedFile))
                {
                    MessageBox.Show("Không tìm thấy ảnh Merge:\n\n" + mergedFile +"\n\n" + "Hãy tải và Merge Tile trước.","Google Map");
                    return;
                }
                // -----------------------------------------
                // 7. Crop
                // -----------------------------------------
                GoogleTileCropService cropService = new GoogleTileCropService();
                string outputFile = @"C:\GoogleMap\google_map_test.jpg";
                string result = cropService.CropImage(mergedFile,outputFile,range,latMin,lonMin,latMax,lonMax,request.Zoom);
                // -----------------------------------------
                // 8. Thông báo
                // -----------------------------------------
                MessageBox.Show("Crop thành công!\n\n" +
                    "WGS84:\n" + "Lat Min = " +
                    latMin.ToString("0.0000000000") +
                    "\n" + "Lat Max = " +
                    latMax.ToString("0.0000000000") +
                    "\n" + "Lon Min = " +
                    lonMin.ToString("0.0000000000") +
                    "\n" + "Lon Max = " +
                    lonMax.ToString("0.0000000000") +
                    "\n\n" + "File:\n" + result,"GOOGLE MAP");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),"Lỗi Crop");
            }
        }
        private void btnTestCoordinate()
        {
            try
            {
                double x = 574108.652052;
                double y = 2363981.765218;
                double ktt = 105.0;
                CoordinateService service = new CoordinateService();
                // =============================================
                // VN2000 → WGS84
                // =============================================
                Wgs84Coordinate wgs = service.ToWgs84(x,y,ktt);
                // =============================================
                // WGS84 → VN2000
                // =============================================
                Vn2000Coordinate vn = service.ToVn2000(wgs.Latitude,wgs.Longitude,ktt);
                // =============================================
                // Sai số
                // =============================================
                double dx = vn.X - x;
                double dy = vn.Y - y;
                double error = Math.Sqrt(dx * dx + dy * dy);
                // =============================================
                // Hiển thị
                // =============================================
                MessageBox.Show("VN2000 ban đầu:\n\n" +
                    "X = " + x.ToString("0.000000") + "\n" +
                    "Y = " + y.ToString("0.000000") + "\n\n" +
                    "WGS84:\n\n" + "Longitude = " +
                    wgs.Longitude.ToString("0.0000000000") + "\n" +
                    "Latitude = " + wgs.Latitude.ToString("0.0000000000") + "\n\n" +
                    "VN2000 sau chuyển đổi:\n\n" +
                    "X = " + vn.X.ToString("0.000000") +"\n" +
                    "Y = " + vn.Y.ToString("0.000000") + "\n\n" +
                    "Sai số:\n\n" +
                    "dX = " + dx.ToString("0.000000") +" m\n" +
                    "dY = " + dy.ToString("0.000000") + " m\n" +
                    "Sai số tổng = " + error.ToString("0.000000") +
                    " m","TEST VN2000 ↔ WGS84");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),"Lỗi TEST");
            }
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
            // TEST JGW ACCURACY
            // =========================================================

private void btnTestJgwAccuracy_Click(
    object sender,
    EventArgs e)
        {
            try
            {
                // =================================================
                // 1. Lấy Request từ giao diện
                // =================================================

                GoogleMapRequest request =
                    GetRequestFromUI();

                if (request == null)
                {
                    return;
                }


                // =================================================
                // 2. File ảnh Crop
                // =================================================

                string imageFile =
                    @"C:\GoogleMap\google_map_test.jpg";


                // =================================================
                // 3. File JGW
                // =================================================

                string jgwFile =
                    @"C:\GoogleMap\google_map_test.jgw";


                if (!File.Exists(imageFile))
                {
                    MessageBox.Show(
                        "Không tìm thấy ảnh Crop:\n\n" +
                        imageFile +
                        "\n\n" +
                        "Hãy chạy TEST CROP trước.",
                        "TEST JGW ACCURACY");

                    return;
                }


                if (!File.Exists(jgwFile))
                {
                    MessageBox.Show(
                        "Không tìm thấy file JGW:\n\n" +
                        jgwFile +
                        "\n\n" +
                        "Hãy chạy TEST JGW trước.",
                        "TEST JGW ACCURACY");

                    return;
                }


                // =================================================
                // 4. Đọc kích thước ảnh
                // =================================================

                Bitmap bitmap =
                    new Bitmap(imageFile);

                int width =
                    bitmap.Width;

                int height =
                    bitmap.Height;

                bitmap.Dispose();


                // =================================================
                // 5. Đọc 6 dòng JGW
                // =================================================

                string[] lines =
                    File.ReadAllLines(jgwFile);


                if (lines.Length != 6)
                {
                    MessageBox.Show(
                        "File JGW phải có đúng 6 dòng.",
                        "TEST JGW ACCURACY");

                    return;
                }


                double A =
                    ParseJgwValue(lines[0]);

                double D =
                    ParseJgwValue(lines[1]);

                double B =
                    ParseJgwValue(lines[2]);

                double E =
                    ParseJgwValue(lines[3]);

                double C =
                    ParseJgwValue(lines[4]);

                double F =
                    ParseJgwValue(lines[5]);


                // =================================================
                // 6. Chuyển 2 điểm VN2000 → WGS84
                // =================================================

                CoordinateService coordinateService =
                    new CoordinateService();


                Wgs84Coordinate wgs1 =
                    coordinateService.ToWgs84(
                        request.X1,
                        request.Y1,
                        request.KinhTuyenTruc);


                Wgs84Coordinate wgs2 =
                    coordinateService.ToWgs84(
                        request.X2,
                        request.Y2,
                        request.KinhTuyenTruc);


                double latMin =
                    Math.Min(
                        wgs1.Latitude,
                        wgs2.Latitude);


                double latMax =
                    Math.Max(
                        wgs1.Latitude,
                        wgs2.Latitude);


                double lonMin =
                    Math.Min(
                        wgs1.Longitude,
                        wgs2.Longitude);


                double lonMax =
                    Math.Max(
                        wgs1.Longitude,
                        wgs2.Longitude);


                // =================================================
                // 7. Tính Tile Range
                // =================================================

                GoogleTileService tileService =
                    new GoogleTileService();


                GoogleTileRange range =
                    tileService.GetTileRange(
                        latMin,
                        lonMin,
                        latMax,
                        lonMax,
                        request.Zoom);


                // =================================================
                // 8. Tính Global Pixel của vùng Crop
                // =================================================

                PointF globalMin =
                    LatLonToGlobalPixelForTest(
                        latMax,
                        lonMin,
                        request.Zoom);


                PointF globalMax =
                    LatLonToGlobalPixelForTest(
                        latMin,
                        lonMax,
                        request.Zoom);


                double originX =
                    range.MinX *
                    256.0;


                double originY =
                    range.MinY *
                    256.0;


                int cropLeft =
                    (int)Math.Floor(
                        globalMin.X -
                        originX);


                int cropTop =
                    (int)Math.Floor(
                        globalMin.Y -
                        originY);


                // =================================================
                // 9. 4 TÂM PIXEL CẦN KIỂM TRA
                //
                // P1 = trên trái
                // P2 = trên phải
                // P3 = dưới trái
                // P4 = dưới phải
                // =================================================

                double p1x =
                    0.5;

                double p1y =
                    0.5;


                double p2x =
                    width - 0.5;

                double p2y =
                    0.5;


                double p3x =
                    0.5;

                double p3y =
                    height - 0.5;


                double p4x =
                    width - 0.5;

                double p4y =
                    height - 0.5;


                // =================================================
                // 10. Local Pixel → Global Pixel
                // =================================================

                double gp1x =
                    cropLeft + p1x;

                double gp1y =
                    cropTop + p1y;


                double gp2x =
                    cropLeft + p2x;

                double gp2y =
                    cropTop + p2y;


                double gp3x =
                    cropLeft + p3x;

                double gp3y =
                    cropTop + p3y;


                double gp4x =
                    cropLeft + p4x;

                double gp4y =
                    cropTop + p4y;


                // =================================================
                // 11. Global Pixel → WGS84
                // =================================================

                Wgs84Coordinate testWgs1 =
                    GlobalPixelToWgs84ForTest(
                        gp1x,
                        gp1y,
                        request.Zoom);


                Wgs84Coordinate testWgs2 =
                    GlobalPixelToWgs84ForTest(
                        gp2x,
                        gp2y,
                        request.Zoom);


                Wgs84Coordinate testWgs3 =
                    GlobalPixelToWgs84ForTest(
                        gp3x,
                        gp3y,
                        request.Zoom);


                Wgs84Coordinate testWgs4 =
                    GlobalPixelToWgs84ForTest(
                        gp4x,
                        gp4y,
                        request.Zoom);


                // =================================================
                // 12. WGS84 → VN2000
                // =================================================

                Vn2000Coordinate real1 =
                    coordinateService.ToVn2000(
                        testWgs1.Latitude,
                        testWgs1.Longitude,
                        request.KinhTuyenTruc);


                Vn2000Coordinate real2 =
                    coordinateService.ToVn2000(
                        testWgs2.Latitude,
                        testWgs2.Longitude,
                        request.KinhTuyenTruc);


                Vn2000Coordinate real3 =
                    coordinateService.ToVn2000(
                        testWgs3.Latitude,
                        testWgs3.Longitude,
                        request.KinhTuyenTruc);


                Vn2000Coordinate real4 =
                    coordinateService.ToVn2000(
                        testWgs4.Latitude,
                        testWgs4.Longitude,
                        request.KinhTuyenTruc);


                // =================================================
                // 13. Dùng JGW tính lại VN2000
                // =================================================

                Vn2000Coordinate jgw1 =
                    ApplyJgw(
                        p1x,
                        p1y,
                        A,
                        B,
                        C,
                        D,
                        E,
                        F);


                Vn2000Coordinate jgw2 =
                    ApplyJgw(
                        p2x,
                        p2y,
                        A,
                        B,
                        C,
                        D,
                        E,
                        F);


                Vn2000Coordinate jgw3 =
                    ApplyJgw(
                        p3x,
                        p3y,
                        A,
                        B,
                        C,
                        D,
                        E,
                        F);


                Vn2000Coordinate jgw4 =
                    ApplyJgw(
                        p4x,
                        p4y,
                        A,
                        B,
                        C,
                        D,
                        E,
                        F);


                // =================================================
                // 14. Tính sai số
                // =================================================

                double error1 =
                    CalculateError(
                        real1,
                        jgw1);


                double error2 =
                    CalculateError(
                        real2,
                        jgw2);


                double error3 =
                    CalculateError(
                        real3,
                        jgw3);


                double error4 =
                    CalculateError(
                        real4,
                        jgw4);


                // =================================================
                // 15. Tạo nội dung báo cáo
                // =================================================

                string message =
                    "KIỂM TRA ĐỘ CHÍNH XÁC JGW\n" +
                    "============================\n\n" +

                    "Ảnh:\n" +
                    width +
                    " x " +
                    height +
                    " pixel\n\n" +

                    "JGW:\n" +
                    "A = " +
                    A.ToString("0.###############") +
                    "\n" +

                    "D = " +
                    D.ToString("0.###############") +
                    "\n" +

                    "B = " +
                    B.ToString("0.###############") +
                    "\n" +

                    "E = " +
                    E.ToString("0.###############") +
                    "\n" +

                    "C = " +
                    C.ToString("0.###############") +
                    "\n" +

                    "F = " +
                    F.ToString("0.###############") +

                    "\n\n" +

                    "P1 - GÓC TRÊN TRÁI\n" +"VN2000 thực:\n" +"X = " + real1.X.ToString("0.000000") +"\n" + "Y = " + real1.Y.ToString("0.000000") +"\n" + "VN2000 từ JGW:\n" + "X = " + jgw1.X.ToString("0.000000") + "\n" + "Y = " +jgw1.Y.ToString("0.000000") + "\n" + "Sai số = " +error1.ToString("0.000000") +" m\n\n" + "P2 - GÓC TRÊN PHẢI\n" +"VN2000 thực:\n" +"X = " +real2.X.ToString("0.000000") +"\n" +"Y = " +real2.Y.ToString("0.000000") +"\n" + "VN2000 từ JGW:\n" +"X = " +jgw2.X.ToString("0.000000") +"\n" +"Y = " +jgw2.Y.ToString("0.000000") +"\n" + "Sai số = " + error2.ToString("0.000000") +" m\n\n" +"P3 - GÓC DƯỚI TRÁI\n" +"VN2000 thực:\n" + "X = " + real3.X.ToString("0.000000") +"\n" + "Y = " + real3.Y.ToString("0.000000") + "\n" +"VN2000 từ JGW:\n" +"X = " +jgw3.X.ToString("0.000000") +"\n" + "Y = " + jgw3.Y.ToString("0.000000") +"\n" +"Sai số = " +error3.ToString("0.000000") +" m\n\n" +"P4 - GÓC DƯỚI PHẢI\n" +"VN2000 thực:\n" +"X = " +real4.X.ToString("0.000000") +"\n" +"Y = " + real4.Y.ToString("0.000000") +"\n" +"VN2000 từ JGW:\n" + "X = " + jgw4.X.ToString("0.000000") +"\n" + "Y = " + jgw4.Y.ToString("0.000000") +"\n" +"Sai số = " + error4.ToString("0.000000") +" m";
                MessageBox.Show(message,"TEST JGW ACCURACY");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(),"Lỗi TEST JGW ACCURACY");
            }
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
