using Assets.scripts.DataModel;
using Assets.scripts.Location;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class DataModel : MonoBehaviour
{
    public static DataModel instance;

    [Header("Server info")]
    // Server Connection Info
    public string localServerName;
    public string remoteServerName;
    private string serverName;
    public int localHostPort;
    public int remoteHostPort;
    private int hostPort;

    public bool useRemoteServer;

    public NetworkConnection connection;

    private void SetServer(bool remote)
    {
        serverName = remote ? remoteServerName : localServerName;
        hostPort = remote ? remoteHostPort : localHostPort;
    }

    private void AbandonConnection()
    {
        Debug.Log("Abandon Connection");
        if (connection != null)
        {
            connection.DropConnectionUsingTCPClient();
            connection = null;
        }
    }

    private bool UpdateNetworkData(NetworkData data)
    {
        bool res = false;
        try
        {
            // data.GetData() is an object of the Type stored in "JSONType" field
            // according to the type of the object, you can compute whatever you want
            Debug.Log("UpdateNetworkData() : " + data.ToJSON());
            Debug.LogWarning("TO BE IMPLEMENTED");

            res = true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("UpdateNetworkData() got an exception : " + ex.Message);
        }
        return res;
    }

    private Thread readLinesThread;

    private void ReadLinesInfinite()
    {
        while (true)
        {
            Thread.Sleep(100);
            if (connection != null)
            {
                connection.CheckTCPClientReceiveSignals();
            }
        }
    }

    private NetworkConnection createNetworkConnectionWithDelegates(SendRequest methodToSendRequestOnceConnectionIsEstablished, ComputeNetworkData methodToComputeDataOnceTheyAreReceived)
    {
        NetworkConnection connection = new NetworkConnection(serverName, hostPort);
        connection.methodToSendRequest = methodToSendRequestOnceConnectionIsEstablished;
        connection.methodToComputeData = methodToComputeDataOnceTheyAreReceived;
        connection.methodToAbandonConnection = AbandonConnection;
        if (!readLinesThread.IsAlive)
        {
            readLinesThread = new Thread(() => ReadLinesInfinite());
            readLinesThread.Start();
            Debug.Log("Start new readLine thread");
        }
        return connection;
    }

    private void SendRequest(float lat, float lon)
    {

    }

    private void SendRequest()
    {
        if (MyLocationService.instance.locationServiceIsRunning)
        {
            Debug.Log("Location service is running : SendRequest");
            SendRequest(MyLocationService.instance.playerLocation.lat_d, MyLocationService.instance.playerLocation.lon_d);
        }
        else
        {
            Debug.LogError("Impossible to send Request : location service is not working");
        }
    }

    public void ConnectAndSendMapEncRequest()
    {
        connection = createNetworkConnectionWithDelegates(SendRequest, UpdateNetworkData);
        connection.StartConnectionUsingTCPClient();
    }

    [Header("Data Model")]
    public string currentPlayerIDConnection;
    
    #region Location

    public IEnumerator StartLocationService()
    {
        yield return StartCoroutine(MyLocationService.instance.StartLocationService());
        StartCoroutine(MyLocationService.instance.RunLocationService());
    }
    public IEnumerator InitializeLocationService()
    {
        MyLocationService.instance = new MyLocationService();
        yield return StartCoroutine(StartLocationService());
    }

    #endregion

    #region MonoBehaviour methods

    /// <summary>
    /// Awake will initialize the connection.  
    /// RunAsyncInit is just for show.  You can do the normal SQLiteInit to ensure that it is
    /// initialized during the Awake() phase and everything is ready during the Start() phase
    /// </summary>
    void Awake()
    {
        instance = this;
        /*
        firstConnectionSucceeded = false;
        firstConnectionErrorMessage = null;*/
        DontDestroyOnLoad(this.gameObject);
    }

    // Use this for initialization
    void Start()
    {
        connection = null;
        readLinesThread = new Thread(() => ReadLinesInfinite());
        readLinesThread.Start();
    }

    /// <summary>
    /// Called when DataModel is destroyed, aka: when application is closed.
    /// </summary>
    void OnDestroy()
    {
        if (connection != null)
        {
            connection.DropConnectionUsingTCPClient();
        }
        if (readLinesThread.IsAlive)
        {
            readLinesThread.Abort();
        }
    }

    #endregion
}
