using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using UnityEngine;

namespace Assets.scripts.Map.OpenStreetMap
{
    class TileLoader
    {
        public static TileLoader instance;
        private static int webRequestCount;
        private static int webRequestMaxCount = 1023;

        private Dictionary<string, string> allTileTexturesFilePathDico;
        private Dictionary<string, Texture2D> allTileTexturesDico;

        private List<string> osmServersUrl;

        public string tileTextureFilesDirectory;

        private object fileAccessLock;

        private MapGeneratorOSM mapGenerator;

        private static System.Random rand = new System.Random();


        private void InitializeOSMServers()
        {
            osmServersUrl = new List<string>();

            osmServersUrl.Add("http://osm-local.libratoi.fr/osm_tiles");
           
            //osmServersUrl.Add("http://osm.libratoi.fr/osm_tiles");
            /*
            osmServersUrl.Add("http://a.tile.openstreetmap.org");
            osmServersUrl.Add("http://b.tile.openstreetmap.org");
            osmServersUrl.Add("http://c.tile.openstreetmap.org");*/
            /*osmServersUrl.Add("http://a.tile.opencyclemap.org/cycle");
            osmServersUrl.Add("http://b.tile.opencyclemap.org/cycle");
            osmServersUrl.Add("http://c.tile.opencyclemap.org/cycle");*/
        }

        private string RandomServer()
        {
            int serverIndex = rand.Next(osmServersUrl.Count);
            string server = osmServersUrl[serverIndex];
            return server;
        }

        public TileLoader()
        {
            InitializeOSMServers();
            allTileTexturesFilePathDico = new Dictionary<string, string>();
            allTileTexturesDico = new Dictionary<string, Texture2D>();
            tileTextureFilesDirectory = Application.persistentDataPath + "/OSMTiles";
            fileAccessLock = new object();
            Debug.Log("Store tile textures in : " + tileTextureFilesDirectory);
            if (!File.Exists(tileTextureFilesDirectory))
            {
                Directory.CreateDirectory(tileTextureFilesDirectory);
            }
        }

        public static void Initialize(MapGeneratorOSM generator)
        {
            instance = new TileLoader();
            webRequestCount = 0;
            instance.mapGenerator = generator;
        }
        
        private Texture2D LoadPNG(string filePath)
        {
            Texture2D tex = null;
            lock(fileAccessLock)
            {
                try
                {
                    //Debug.Log("Read file : " + filePath);
                    if (File.Exists(filePath))
                    {
                        byte[] fileData = File.ReadAllBytes(filePath);
                        tex = new Texture2D(2, 2);
                        tex.LoadImage(fileData);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("LoadPNG got exception : " + ex.Message);
                }
            }
            return tex;
        }

        /// <summary>
        /// Load tile either from local cache or from web (in this case, send an asynchronous request that will be received later)
        /// Returns true if load happened (either from cache or from web), returns false if it didn't (because too many requests are pending)
        /// </summary>
        /// <param name="zoom"></param>
        /// <param name="coordX"></param>
        /// <param name="coordY"></param>
        /// <returns></returns>
        public bool LoadTileImageFromZoomAndCoords(int zoom, int coordX, int coordY)
        {
            string key = MapGeneratorOSM.GetKeyFromZoomAndCoords(zoom, coordX, coordY);
            string tilePath = GetTilePathFromZoomAndCoords(zoom, coordX, coordY);

            if (!allTileTexturesFilePathDico.ContainsKey(key))
            {
                // We don't know tile path, it means we never created the file nor made a request to download it

                bool fileExists = false;
                lock (fileAccessLock)
                {
                    fileExists = File.Exists(tilePath);
                    if (fileExists)
                    {
                        // check if file really contains something (never too careful)
                        FileInfo info = new FileInfo(tilePath);
                        if (info.Length <= 0)
                        {
                            // file exist but is empty
                            fileExists = false;
                        }
                    }
                }

                if (fileExists)
                {
                    // but it already exists in cache ! So we should just ask for rendering
                    mapGenerator.EnqueueTileToDisplay(key);
                }
                else
                {
                    // it doesn't exist yet, so we should create it and download its content
                    CreateTileDirectoriesIfNeeded(zoom, coordX);

                    // except if there are too many requests pending
                    if (webRequestCount >= webRequestMaxCount)
                    {
                        //Debug.LogWarning("Prevent download of tile : " + key + " because webRequestCount is = " + webRequestCount);
                        return false;
                    }

                    allTileTexturesFilePathDico.Add(key, tilePath);
                    // Call web request to download image
                    lock (fileAccessLock)
                    {
                        try
                        {
                            string url = RandomServer() + "/" + zoom + "/" + coordX + "/" + coordY + ".png";
                            //Debug.Log(url);
                            WebClient client = new WebClient();

                            // sync
                            //client.DownloadFile(new Uri(url), tilePath);

                            // async
                            client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadDataCallback);
                            client.DownloadFileAsync(new Uri(url), tilePath, key);

                            webRequestCount++;
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError("LoadTileImageFromZoomAndCoords got exception : " + ex.Message);
                        }
                    }
                }
            }
            return true;
        }

        private void DownloadDataCallback(object sender, AsyncCompletedEventArgs e)
        {
            //System.Threading.AutoResetEvent waiter = (System.Threading.AutoResetEvent)e.UserState;
            try
            {
                // If the request was not canceled and did not throw
                // an exception, display the resource.
                if (!e.Cancelled && e.Error == null)
                {
                    /*
                    byte[] data = (byte[])e.Result;
                    string textData = System.Text.Encoding.UTF8.GetString(data);

                    Console.WriteLine(textData);*/
                    
                    string tileKey = e.UserState as string;
                    mapGenerator.EnqueueTileToDisplay(tileKey);

                    //Debug.LogWarning("webRequestCount: " + webRequestCount);
                }
                else
                {
                    Debug.LogWarning("Cancelled : " + e.Cancelled);
                    Debug.LogWarning("Error : " + e.Error.Message);
                }
                webRequestCount--;
            }
            finally
            {
                // Let the main application thread resume.
                //waiter.Set();
            }
        }

        public Texture2D GetTextureFromZoomAndCoords(int zoom, int coordX, int coordY)
        {
            Texture2D texture = null;
            string key = MapGeneratorOSM.GetKeyFromZoomAndCoords(zoom, coordX, coordY);
            string tilePath = GetTilePathFromZoomAndCoords(zoom, coordX, coordY);

            bool fileExists = false;
            lock (fileAccessLock)
            {
                fileExists = File.Exists(tilePath);
            }
            
            if (!allTileTexturesFilePathDico.ContainsKey(key) && fileExists)
            {
                // texture filename is not known but file already exist in memory !
                // memorize filename
                allTileTexturesFilePathDico.Add(key, tilePath);
            }

            if (allTileTexturesDico.ContainsKey(key))
            {
                // texture already exists ! Just return it
                texture = allTileTexturesDico[key];
            }
            else if (allTileTexturesFilePathDico.ContainsKey(key) && fileExists)
            {
                // texture filename is already known and the corresponding file exists, create corresponding texture and save it
                texture = LoadPNG(allTileTexturesFilePathDico[key]);
                allTileTexturesDico.Add(key, texture);
            }
            else
            {
                //Debug.LogWarning("No texture nor filepath nor file exist !");
            }
            return texture;
        }

        private string GetTilePathFromZoomAndCoords(int zoom, int coordX, int coordY)
        {
            return tileTextureFilesDirectory + "/" + zoom + "/" + coordX + "/" + coordY + ".png";
        }

        private void CreateTileDirectoriesIfNeeded(int zoom, int coordX)
        {
            string tileZoomDirectory = tileTextureFilesDirectory + "/" + zoom;
            string tileCoordXDirectory = tileTextureFilesDirectory + "/" + zoom + "/" + coordX;
            lock(fileAccessLock)
            {
                if (!File.Exists(tileZoomDirectory))
                {
                    Directory.CreateDirectory(tileZoomDirectory);
                }
                if (!File.Exists(tileCoordXDirectory))
                {
                    Directory.CreateDirectory(tileCoordXDirectory);
                }
            }
        }
    }
}
