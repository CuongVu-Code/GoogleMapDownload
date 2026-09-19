using GoogleMapPlugin.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;

namespace GoogleMapPlugin.Services
{
    
    public class GoogleMapService
    {
        public delegate void TileProgressHandler(int completed, int total);
        private const int TILE_SIZE = 256;

        private const string GOOGLE_URL =
            "https://mt1.google.com/vt/" +
            "?lyrs={0}&x={1}&y={2}&z={3}";
        public class DownloadSharedState
        {
            public List<GoogleTileDownloadResult> Results;
            public int Completed;
            public int Total;
            public object SyncRoot;
            public TileProgressHandler Progress;
            public DownloadSharedState()
            {
                Results = new List<GoogleTileDownloadResult>();
                Completed = 0;
                Total = 0;
                SyncRoot = new object();
                Progress = null;
            }
        }
        public class DownloadWorkerState
        {
            public GoogleMapService Service;
            public GoogleTileItem Tile;
            public int Index;
            public int Zoom;
            public string Layer;
            public Semaphore Semaphore;
            public DownloadSharedState Shared;
            public ManualResetEvent Event;
        }

        // =====================================================
        // TẢI 1 TILE GOOGLE
        // =====================================================

        public byte[] DownloadTile(
            int tileX,
            int tileY,
            int zoom,
            string layer)
        {
            string url = string.Format(GOOGLE_URL,layer,tileX,tileY,zoom);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent ="Mozilla/5.0";
            request.Timeout = 20000;
            try
            {
                using (HttpWebResponse response =(HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode !=HttpStatusCode.OK)
                    {
                        return null;
                    }
                    using (Stream stream =response.GetResponseStream())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            byte[] buffer = new byte[8192];
                            int bytesRead;
                            while ((bytesRead = stream.Read(buffer,0,buffer.Length)) > 0)
                            {
                                ms.Write(buffer,0,bytesRead);
                            }
                            return ms.ToArray();
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }
        public string GetLayer(string mapType)
        {
            switch (
                mapType.ToLower())
            {
                case "satellite":
                    return "s";

                case "road":
                    return "m";

                case "hybrid":
                    return "y";

                case "terrain":
                    return "p";

                default:
                    return "s";
            }
        }
        //Download song song
        public List<GoogleTileDownloadResult>DownloadTilesParallel(List<GoogleTileItem> tiles,int zoom,string layer,int maxWorkers, GoogleMapService.TileProgressHandler progress)
        {
            List<GoogleTileDownloadResult> empty =
                new List<GoogleTileDownloadResult>();
            if (tiles == null ||
                tiles.Count == 0)
            {
                return empty;
            }
            // ---------------------------------------------------------
            // SHARED STATE
            // ---------------------------------------------------------
            DownloadSharedState shared = new DownloadSharedState();
            shared.Total = tiles.Count;
            shared.Progress = progress;  
            // ---------------------------------------------------------
            // TẠO RESULT
            // ---------------------------------------------------------
            int i;
            for (i = 0; i < tiles.Count; i++)
            {
                shared.Results.Add(new GoogleTileDownloadResult(tiles[i].X,tiles[i].Y));
            }
            // ---------------------------------------------------------
            // SEMAPHORE
            // ---------------------------------------------------------
            Semaphore semaphore = new Semaphore(maxWorkers,maxWorkers);
            // ---------------------------------------------------------
            // EVENT
            // ---------------------------------------------------------
            ManualResetEvent[] events = new ManualResetEvent[tiles.Count];
            for (i = 0; i < tiles.Count; i++)
            {
                events[i] = new ManualResetEvent(false);
            }
            // ---------------------------------------------------------
            // QUEUE WORKER
            // ---------------------------------------------------------
            for (i = 0; i < tiles.Count; i++)
            {
                DownloadWorkerState state = new DownloadWorkerState();
                state.Service = this;
                state.Tile = tiles[i];
                state.Index = i;
                state.Zoom = zoom;
                state.Layer = layer;
                state.Semaphore =semaphore;
                state.Shared =shared;
                state.Event =events[i];
                ThreadPool.QueueUserWorkItem(new WaitCallback(DownloadWorker),state);
            }
            // ---------------------------------------------------------
            // CHỜ TẤT CẢ TILE
            // ---------------------------------------------------------
            for (i = 0; i < events.Length; i++)
            {
                events[i].WaitOne();
            }
            // ---------------------------------------------------------
            // GIẢI PHÓNG
            // ---------------------------------------------------------
            semaphore.Close();
            for (i = 0;
                 i < events.Length;
                 i++)
            {
                events[i].Close();
            }
            return shared.Results;
        }
        private void DownloadWorker(object obj)
        {
            DownloadWorkerState state = (DownloadWorkerState)obj;
            try
            {
                // Chờ một slot
                state.Semaphore.WaitOne();
                GoogleTileDownloadResult result = new GoogleTileDownloadResult(state.Tile.X,state.Tile.Y);
                // -----------------------------------------------------
                // TẢI TILE
                // -----------------------------------------------------
                byte[] data = DownloadTile(state.Tile.X,state.Tile.Y,state.Zoom,state.Layer);
                if (data != null)
                {
                    result.Data = data;
                    result.Success = true;
                }
                // -----------------------------------------------------
                // LƯU KẾT QUẢ
                // -----------------------------------------------------
                lock (state.Shared.SyncRoot)
                {
                    state.Shared.Results[state.Index] = result;
                    state.Shared.Completed++;
                    int completed =state.Shared.Completed;
                    if (state.Shared.Progress != null)
                    {
                        state.Shared.Progress(completed,state.Shared.Total);
                    }
                }
            }
            catch
            {
                // Không để một tile làm chết toàn bộ quá trình
            }
            finally
            {
                state.Semaphore.Release();
                state.Event.Set();
            }
        }
    }
}
