using Assets.scripts.DataModel;
using Assets.scripts.Location;
using Assets.scripts.Map.OpenStreetMap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using UnityEngine;

public class MapGeneratorOSM : MonoBehaviour
{
    public static MapGeneratorOSM instance;

    [Header("References in scene")]
    public Transform mapParent;
    public MapPlayerBehaviour playerScript;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public Texture defaultTileTexture;

    [Header("Map settings")]
    public int zoomLevel;
    public int positionTileZoomLevel = 6;
    public int tilesCountRadius;

    [Header("Map info")]
    public bool mapIsReadyForGeneration;
    public bool mapIsReady;
    
    private Dictionary<string, TileOSMBehaviour> tilesDico;

    private GameObject positionTile;

    private Vector3 unset = Vector3.one * -1; // constant



    #region Tiles to download

    private Queue<string> tilesToDownload; // main thread enqueues, download thread dequeues

    private void EnqueueTileKeyToDownload(string tileKey)
    {
        //Debug.Log("EnqueueTileKeyToDownload : " + tileKey);
        lock (tilesToDownload)
        {
            tilesToDownload.Enqueue(tileKey);
        }
    }

    public string DequeueTileKeyToDownload()
    {
        string tileKey;
        lock (tilesToDownload)
        {
            try
            {
                tileKey = tilesToDownload.Dequeue();
                if (!string.IsNullOrEmpty(tileKey))
                {
                    //Debug.Log("DequeueTileKeyToDownload : " + tileKey);
                }
            }
            catch (System.Exception)
            {
                tileKey = null;
            }
        }
        return tileKey;
    }

    #endregion

    #region Tiles to display

    private Queue<string> tilesToDisplay; // download thread enqueues, main thread dequeues

    public void EnqueueTileToDisplay(string tileKey)
    {
        //Debug.Log("EnqueueTileToDisplay : " + tileKey);
        lock (tilesToDisplay)
        {
            tilesToDisplay.Enqueue(tileKey);
        }
    }

    private TileOSMBehaviour DequeueTileToDisplay()
    {
        string tileKey;
        TileOSMBehaviour tile = null;
        lock (tilesToDisplay)
        {
            try
            {
                tileKey = tilesToDisplay.Dequeue();
                //Debug.Log("DequeueTileToDisplay : " + tileKey);
                tile = tilesDico[tileKey];
            }
            catch (System.Exception)
            {
                tileKey = null;
            }
        }
        if (string.IsNullOrEmpty(tileKey))
        {
            return null;
        }
        return tile;
    }

    #endregion

    #region Tools

    public Vector2 TilePosToWorldPos(float tileX, float tileY)
    {
        double n = Math.Pow(2, zoomLevel);
        double lon_deg = tileX / n * 360.0 - 180.0;
        double lat_rad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * tileY / n)));
        double lat_deg = lat_rad * 180.0 / Math.PI;
        return new Vector2((float)lat_deg, (float)lon_deg);
    }

    private Vector2 WorldToTilePos(double lat, double lon, int zoom)
    {
        Vector2 p = new Vector2();
        p.x = (float)((lon + 180.0) / 360.0 * (1 << zoom));
        p.y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
            1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));
        return p;
    }

    private Vector2 WorldToTilePos(double lat, double lon)
    {
        return WorldToTilePos(lat, lon, zoomLevel);
    }

    private Vector2 TileToWorldPos(double tile_x, double tile_y)
    {
        Vector2 p = new Vector2();
        double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoomLevel));

        p.y = (float)((tile_x / Math.Pow(2.0, zoomLevel) * 360.0) - 180.0);
        p.x = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

        return p;
    }

    private Vector2 NeighbourTileCoordinates(int tileX, int tileY, int dx, int dy)
    {
        // Xs loop
        int neighbourTileX = tileX + dx;
        int maxTileX = (int)Math.Round(Math.Pow(2, zoomLevel)) - 1;
        if (neighbourTileX < 0)
        {
            neighbourTileX = neighbourTileX + (maxTileX + 1);
        }
        if (neighbourTileX > maxTileX)
        {
            neighbourTileX = neighbourTileX - (maxTileX + 1);
        }

        // Ys don't loop
        int neighbourTileY = tileY + dy;
        int maxTileY = (int)Math.Round(Math.Pow(2, zoomLevel)) - 1;
        if (neighbourTileY < 0 || neighbourTileY > maxTileY)
        {
            // out of bounds
            return Vector2.one * -1;
        }
        
        return new Vector2(neighbourTileX, neighbourTileY);
    }

    public static string GetKeyFromZoomAndCoords(int zoom, int coordX, int coordY)
    {
        return zoom + "." + coordX + "." + coordY;
    }

    public GameObject GetTileFromCoords(int coordX, int coordY)
    {
        string tileKey = GetKeyFromZoomAndCoords(zoomLevel, coordX, coordY);
        if (tilesDico.ContainsKey(tileKey))
        {
            return tilesDico[tileKey].gameObject;
        }
        else
        {
            return null;
        }
    }


    private void InitializePositionTileForPosition(float lat, float lon)
    {
        if (positionTile == null)
        {
            Vector2 tilePos = WorldToTilePos(lat, lon, positionTileZoomLevel);
            
            int tileX = Mathf.FloorToInt(tilePos.x);
            int tileY = Mathf.FloorToInt(tilePos.y);

            float positionOnTileX = tilePos.x - tileX;
            float positionOnTileY = tilePos.y - tileY;
            //Debug.Log("positionOnTileX = " + positionOnTileX);
            //Debug.Log("positionOnTileY = " + positionOnTileY);
            //positionOffsetInScene = new Vector2();
            Vector3 positionOfPositionTile = ( (0.5f - positionOnTileX) * Vector3.right + (0.5f - positionOnTileY) * -Vector3.forward) * 5 * Mathf.Pow(2, zoomLevel - positionTileZoomLevel);

            positionTile = Instantiate(tilePrefab, positionOfPositionTile, Quaternion.Euler(0, 180, 0));
            positionTile.transform.localScale *= Mathf.Pow(2, zoomLevel - positionTileZoomLevel);
            positionTile.name = "positiontile_" + tileX + "_" + tileY;
            positionTile.transform.SetParent(mapParent);
            positionTile.GetComponent<TileOSMBehaviour>().SetData(positionTileZoomLevel, tileX, tileY);
            //string tileKey = GetKeyFromZoomAndCoords(positionTileZoomLevel, tileX, tileY);
            //positionTile.GetComponent<TileOSMBehaviour>().LoadTexture();
            positionTile.GetComponent<Renderer>().enabled = false;
            //tilesDico.Add(tileKey, positionTile.GetComponent<TileOSMBehaviour>());
            //EnqueueTileKeyToDownload(tileKey);
        }
    }

    private Vector3 GetWorldPositionFromLatLonOnPositionTile(float lat, float lon)
    {
        Vector3 worldPosition = Vector3.zero;
        try
        {
            Vector2 tilePos = WorldToTilePos(lat, lon, positionTileZoomLevel);
            int tileX = Mathf.FloorToInt(tilePos.x);
            int tileY = Mathf.FloorToInt(tilePos.y);
            float positionOnTileX = (tilePos.x - tileX);
            float positionOnTileY = (tilePos.y - tileY);
            float tileSize = positionTile.transform.localScale.x * 10;
            worldPosition = positionTile.transform.position + (-(tileSize / 2) + positionOnTileX * tileSize) * Vector3.right + ((tileSize / 2) - positionOnTileY * tileSize) * Vector3.forward;
        }
        catch (Exception ex)
        {
            Debug.LogError("GetWorldPositionFromLatLonOnPositionTile("+lat+","+lon+") got exception: " + ex.Message);
        }
        return worldPosition;
    }

    public Vector3 GetWorldPositionFromLatLon(float lat, float lon)
    {
        Vector2 tilePos = WorldToTilePos(lat, lon);
        int tileX = Mathf.FloorToInt(tilePos.x);
        int tileY = Mathf.FloorToInt(tilePos.y);
        float positionOnTileX = (tilePos.x - tileX);
        float positionOnTileY = (tilePos.y - tileY);
        string tileKey = GetKeyFromZoomAndCoords(zoomLevel, tileX, tileY);
        Vector3 worldPosition = unset;
        if (tilesDico.ContainsKey(tileKey))
        {
            float tileSize = tilesDico[tileKey].transform.localScale.x * 10;
            worldPosition = tilesDico[tileKey].transform.position + (-(tileSize / 2) + positionOnTileX * tileSize) * Vector3.right + ((tileSize / 2) - positionOnTileY * tileSize) * Vector3.forward;
        }
        if (worldPosition.Equals(unset))
        {
            // no tile is available to give position
            // we can try on position tile
            worldPosition = GetWorldPositionFromLatLonOnPositionTile(lat, lon);
        }
        return worldPosition;
    }

    #endregion

    #region Add and position Tiles

    private void AddTilesAroundPosition(float centerLat, float centerLon, int radiusTiles)
    {
        Vector2 centerTileCoordinates = WorldToTilePos(centerLat, centerLon);
        int centerTileX = Mathf.FloorToInt(centerTileCoordinates.x);
        int centerTileY = Mathf.FloorToInt(centerTileCoordinates.y);

        string centerTileKey = GetKeyFromZoomAndCoords(zoomLevel, centerTileX, centerTileY);
        Vector3 centerTilePosition = Vector3.zero;
        if (tilesDico.ContainsKey(centerTileKey))
        {
            centerTilePosition = tilesDico[centerTileKey].transform.position;
        }
        else
        {
            centerTilePosition = GetWorldPositionFromLatLonOnPositionTile(centerLat, centerLon);
        }

        int count = radiusTiles;

        // spiral from 0,0
        int toX = (count + 1) * 2;
        int toY = (count + 1) * 2;
        int x = 0;
        int y = 0;
        int dx = 0;
        int dy = -1;
        for (int i = 0; i < (Math.Max(toX, toY) * Math.Max(toX, toY)); i++)
        {
            if (x > (-toX / 2) && x < (toX / 2) && y > (-toY / 2) && y < (toY / 2))
            {
                // these coordinates are OK, do stuff
                Vector2 neighbourTileCoordinates = NeighbourTileCoordinates(centerTileX, centerTileY, x, y);
                int neighbourTileX = Mathf.FloorToInt(neighbourTileCoordinates.x);
                int neighbourTileY = Mathf.FloorToInt(neighbourTileCoordinates.y);

                double distanceBetweenCenterAndCoordinates = Math.Sqrt(x * x + y * y);
                if (distanceBetweenCenterAndCoordinates < count)
                {
                    // distance is close enough, we should have a tile there
                    string tileKey = GetKeyFromZoomAndCoords(zoomLevel, neighbourTileX, neighbourTileY);
                    if (!tilesDico.ContainsKey(tileKey))
                    {
                        // this tile doesn't exist yet, we should add it
                        float sizeOfTile = 5;
                        Vector3 positionInWorld = centerTilePosition + x * sizeOfTile * Vector3.right + y * sizeOfTile * -Vector3.forward;
                        AddTile(neighbourTileX, neighbourTileY, positionInWorld);
                    }
                }
            }
            if (x == y || (x < 0 && x == -y) || (x > 0 && x == 1 - y))
            {
                int tmp = dx;
                dx = -dy;
                dy = tmp;
            }
            x = x + dx;
            y = y + dy;
        }
    }

    private void AddTile(int coordX, int coordY, Vector3 positionInWorld)
    {
        // The tile itself
        GameObject tile = Instantiate(tilePrefab, positionInWorld, Quaternion.Euler(0,180,0));
        tile.GetComponent<MeshRenderer>().material.mainTexture = defaultTileTexture;
        tile.name = "tile_" + coordX + "_" + coordY;
        tile.transform.SetParent(mapParent);
        tile.GetComponent<TileOSMBehaviour>().SetData(zoomLevel, coordX, coordY);
        string tileKey = GetKeyFromZoomAndCoords(zoomLevel, coordX, coordY);
        tilesDico.Add(tileKey, tile.GetComponent<TileOSMBehaviour>());
        EnqueueTileKeyToDownload(tileKey);

        KorokManager.instance.AddKorokOnMap(positionInWorld, coordX, coordY);
    }

    #endregion

    #region debug

    private void DebugPutCubeAtPosition(float lat, float lon)
    {
        Vector3 positionInScene = GetWorldPositionFromLatLon(lat, lon);
        if (!positionInScene.Equals(Vector3.one * -1))
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.GetComponent<Collider>().enabled = false;
            cube.transform.position = positionInScene + 0.1f * Vector3.up;
            cube.transform.localScale = Vector3.one * 0.2f;
        }
    }

    #endregion
    
    #region Thread that download textures

    private Thread tileDownloaderThread;

    private void DownloadTilesInfinite()
    {
        Debug.Log("DownloadTilesInfinite started");
        while (true)
        {
            Thread.Sleep(1);
            try
            {
                string tileKey = DequeueTileKeyToDownload();
                if (!string.IsNullOrEmpty(tileKey))
                {
                    TileOSMBehaviour tile = tilesDico[tileKey];
                    if (!tile.TextureDownloadHasBeenRequested())
                    {
                        bool loadTileDidHappen = tile.LoadTexture();
                        if (!loadTileDidHappen)
                        {
                            EnqueueTileKeyToDownload(tileKey);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("tileKey is not empty but download has already been requested : " + tileKey);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("tileDownloaderThread got an exception : " + ex.Message);
            }
        }
    }

    private void StartDownloadTilesThread()
    {
        tileDownloaderThread = new Thread(() => DownloadTilesInfinite());
        tileDownloaderThread.Name = "tileDownloaderThread";
        tileDownloaderThread.Start();
    }

    #endregion

    #region Coroutine to display downloaded textures
    
    private Coroutine checkForTexturesToDisplayCoroutine;

    private IEnumerator CheckForTexturesToDisplay()
    {
        yield return new WaitForSeconds(0);
        TileOSMBehaviour tile = DequeueTileToDisplay();
        if (tile != null)
        {
            tile.SetTexture();
        }
        checkForTexturesToDisplayCoroutine = StartCoroutine(CheckForTexturesToDisplay());
    }

    private void InitCoroutineToDisplayTiles()
    {
        checkForTexturesToDisplayCoroutine = StartCoroutine(CheckForTexturesToDisplay());
    }

    #endregion

    #region Update Tiles from player position

    private Vector2 lastTilePlayerStoodOn;

    private void UpdateTilesFromPlayerPosition(float lat, float lon)
    {
        Vector2 newTilePlayerStandsOn = WorldToTilePos(lat, lon);
        int newCoordX = Mathf.FloorToInt(newTilePlayerStandsOn.x);
        int newCoordY = Mathf.FloorToInt(newTilePlayerStandsOn.y);
        int lastCoordX = Mathf.FloorToInt(lastTilePlayerStoodOn.x);
        int lastCoordY = Mathf.FloorToInt(lastTilePlayerStoodOn.y);
        if (newCoordX != lastCoordX || newCoordY != lastCoordY)
        {
            AddTilesAroundPosition(lat, lon, tilesCountRadius);
            lastTilePlayerStoodOn = new Vector2(newCoordX, newCoordY);
        }
    }

    #endregion

    public void GenerateOSMMap()
    {
        mapIsReady = false;

        tilesDico = new Dictionary<string, TileOSMBehaviour>();
        tilesToDisplay = new Queue<string>();
        tilesToDownload = new Queue<string>();

        TileLoader.Initialize(this);

        //StartDownloadTilesThread();
        //InitCoroutineToDisplayTiles();

        mapIsReadyForGeneration = true;
    }

    #region MonoBehaviour methods

    void Awake()
    {
        MapGeneratorOSM.instance = this;
    }

    // Use this for initialization
    void Start ()
    {
    }

	
	// Update is called once per frame
	void Update ()
    {
        if (mapIsReadyForGeneration && playerScript.playerPositionIsValid)
        {
            InitializePositionTileForPosition(playerScript.playerLat, playerScript.playerLon);

            UpdateTilesFromPlayerPosition(playerScript.playerLat, playerScript.playerLon);
            mapIsReady = true;
        }
    }

    void OnDestroy()
    {
        /*
        if (tileDownloaderThread.IsAlive)
        {
            tileDownloaderThread.Abort();
        }*/
        if (checkForTexturesToDisplayCoroutine != null)
        {
            StopCoroutine(checkForTexturesToDisplayCoroutine);
            checkForTexturesToDisplayCoroutine = null;
        }
    }

    #endregion
}
