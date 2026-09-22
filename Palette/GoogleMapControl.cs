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
            //string st = r + "";
            lblPoint1.Text = "ĐIỂM 1";
            lblPoint1.Location = new Point(10, y);
            lblPoint1.AutoSize = true;
            lblPoint1.Font = new Font("Arial", 9, FontStyle.Bold);
            panel.Controls.Add(lblPoint1);
            y += 30;
            // X1
            lblX1 = new Label();
            lblX1.Text = "X:";
            lblX1.Location = new Point(10, y + 3);
            lblX1.AutoSize = true;
            panel.Controls.Add(lblX1);
            txtX1 = new TextBox();
            txtX1.Location = new Point(35, y);
            txtX1.Width = 235;// 260;// 170;
            panel.Controls.Add(txtX1);
            y += 30;
            // Y1
            lblY1 = new Label();
            lblY1.Text = "Y:";
            lblY1.Location = new Point(10, y + 3);
            lblY1.AutoSize = true;
            panel.Controls.Add(lblY1);
            txtY1 = new TextBox();
            txtY1.Location = new Point(35, y);
            txtY1.Width = 235;// 260;// 170;
            panel.Controls.Add(txtY1);
            y += 30;
            // BUTTON PICK 1
            btnPick1 = new Button();
            btnPick1.Text = "CHỌN ĐIỂM 1";
            btnPick1.Location = new Point(35, y);
            btnPick1.Width = 235;// 170;
            btnPick1.Click += BtnPick1_Click;
            panel.Controls.Add(btnPick1);
            y += 45;
            // =================================
            // ĐIỂM 2
            // =================================
            lblPoint2 = new Label();
            lblPoint2.Text = "ĐIỂM 2";
            lblPoint2.Location = new Point(10, y);
            lblPoint2.AutoSize = true;
            lblPoint2.Font = new Font("Arial", 9, FontStyle.Bold);
            panel.Controls.Add(lblPoint2);
            y += 30;
            // X2
            lblX2 = new Label();
            lblX2.Text = "X:";
            lblX2.Location = new Point(10, y + 3);
            lblX2.AutoSize = true;
            panel.Controls.Add(lblX2);
            txtX2 = new TextBox();
            txtX2.Location = new Point(35, y);
            txtX2.Width = 235;//260;// 170;
            panel.Controls.Add(txtX2);
            y += 30;
            // Y2
            lblY2 = new Label();
            lblY2.Text = "Y:";
            lblY2.Location = new Point(10, y + 3);
            lblY2.AutoSize = true;
            panel.Controls.Add(lblY2);
            txtY2 = new TextBox();
            txtY2.Location = new Point(35, y);
            txtY2.Width = 235;// 260;// 170;
            panel.Controls.Add(txtY2);
            y += 30;
            // BUTTON PICK 2
            btnPick2 = new Button();
            btnPick2.Text = "CHỌN ĐIỂM 2";
            btnPick2.Location = new Point(35, y);
            btnPick2.Width = 235;//170;
            btnPick2.Click += BtnPick2_Click;
            panel.Controls.Add(btnPick2);
            y += 50;
            // =================================
            // KTT
            // =================================
            lblKTT = new Label();
            lblKTT.Text = "Kinh tuyến trục:";
            lblKTT.Location = new Point(10, y);
            lblKTT.AutoSize = true;
            panel.Controls.Add(lblKTT);
            y += 25;
            cmbKTT = new ComboBox();
            cmbKTT.Location = new Point(10, y);
            cmbKTT.Width = 260;
            cmbKTT.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbKTT.DropDown += cmbKTT_DropDown;
            panel.Controls.Add(cmbKTT);
            y += 45;
        
        // =================================
        // MAP TYPE
        // =================================
        lblMapType = new Label();
            lblMapType.Text = "Loại bản đồ:";
            lblMapType.Location = new Point(10, y);
            lblMapType.AutoSize = true;
            panel.Controls.Add(lblMapType);
            y += 25;
            cmbMapType = new ComboBox();
            cmbMapType.Location = new Point(10, y);
            cmbMapType.Width = 260;
            cmbMapType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMapType.Items.Add("Satellite");
            cmbMapType.Items.Add("Road");
            cmbMapType.Items.Add("Hybrid");
            cmbMapType.Items.Add("Terrain");
            cmbMapType.SelectedIndex = 0;
            panel.Controls.Add(cmbMapType);
            y += 45;
            // =================================
            // ZOOM
            // =================================
            lblZoom = new Label();
            lblZoom.Text = "Google Zoom:";
            lblZoom.Location = new Point(10, y);
            lblZoom.AutoSize = true;
            panel.Controls.Add(lblZoom);
            y += 25;
            numZoom = new NumericUpDown();
            numZoom.Location = new Point(10, y);
            numZoom.Width = 100;
            numZoom.Minimum = 1;
            numZoom.Maximum = 21;
            numZoom.Value = 17;
            panel.Controls.Add(numZoom);
            //ADD LABLE PROGRESS
            labelProgress = new Label();
            labelProgress.Location = new Point(125, y);
            panel.Controls.Add(labelProgress);
            y += 50;
            // =================================
            // DOWNLOAD
            // =================================
            btnDownload = new Button();
            btnDownload.Text = "TẢI GOOGLE MAP";
            btnDownload.Location = new Point(10, y);
            btnDownload.Width = 260;
            btnDownload.Height = 35;
            btnDownload.Click += BtnDownload_Click;
            panel.Controls.Add(btnDownload);
            y += 45;
            // =================================
            // DELETE
            // =================================
            btnDelete = new Button();
            btnDelete.Text = "XÓA ẢNH";
            btnDelete.Location = new Point(10, y);
            btnDelete.Width = 260;
            btnDelete.Height = 30;
            panel.Controls.Add(btnDelete);
            // =================================
            // TEST
            // =================================
            btnCheck = new Button();
            btnCheck.Text = "GOOGLE CORDINATE";
            btnCheck.Location = new Point(10, y + 45);
            btnCheck.Width = 125;//170;
            btnCheck.Click += BtnCheck_Click;
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
        private void BtnPick1_Click(object sender, EventArgs e)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            Editor ed = doc.Editor;
            PromptPointOptions options = new PromptPointOptions("\nChọn điểm 1: ");
            PromptPointResult result = ed.GetPoint(options);
            if (result.Status == PromptStatus.OK)
            {
                Point3d point = result.Value;
                txtX1.Text = point.X.ToString("0.000000");
                txtY1.Text = point.Y.ToString("0.000000");
            }
        }
        // =====================================
        // CHỌN ĐIỂM 2
        // =====================================
        private void BtnPick2_Click(object sender, EventArgs e)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            Editor ed = doc.Editor;
            // =========================================
            // LẤY ĐIỂM 1 TỪ TEXTBOX
            // =========================================
            double x1;
            double y1;
            if (!double.TryParse(txtX1.Text, out x1) || !double.TryParse(txtY1.Text, out y1))
            {
                MessageBox.Show("Bạn hãy chọn điểm 1 trước.", "Google Map", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            Point3d p1 = new Point3d(x1, y1, 0.0);
            // =========================================
            // CHỌN GÓC ĐỐI DIỆN
            // =========================================
            PromptPointResult result = ed.GetCorner("\nChọn góc đối diện: ", p1);
            // =========================================
            // KIỂM TRA KẾT QUẢ
            // =========================================
            if (result.Status == PromptStatus.OK)
            {
                Point3d p2 = result.Value;
                txtX2.Text = p2.X.ToString("0.000000");
                txtY2.Text = p2.Y.ToString("0.000000");

            }
        }
        //Nút test
        private void BtnCheck_Click(object sender, EventArgs e)
        {
            //TestCoordinate();
            //TestTileList();
            TestMergeTiles();
        }
        private void BtnCheck1_Click(object sender, EventArgs e)
        {
            //TestGoogleTile();
            TestDownloadTile();
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
                this.BeginInvoke(
                    new MethodInvoker(
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
                GoogleTileService tileService =
                    new GoogleTileService();

                GoogleTileRange range =
                    new GoogleTileRange();

                range.MinX = 104026;
                range.MaxX = 104027;
                range.MinY = 57568;
                range.MaxY = 57569;

                List<GoogleTileItem> tiles =
                    tileService.GetTileList(range);

                GoogleTileMergeService mergeService =
                    new GoogleTileMergeService();

                string tileFolder =
                    @"C:\GoogleMap\Tiles";

                string outputFile =
                    @"C:\GoogleMap\merged_test.jpg";

                string result =
                    mergeService.MergeTiles(
                        tiles,
                        17,
                        tileFolder,
                        outputFile);

                MessageBox.Show(
                    "Ghép Tile thành công!\n\n" +
                    "File:\n" +
                    result,
                    "GOOGLE MAP");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Lỗi Merge Tile");
            }
        }
    }
}
