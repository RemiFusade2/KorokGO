using System;
using System.Net.Sockets;
using System.IO;
using UnityEngine;

namespace Assets.scripts.DataModel
{
    public delegate void SendRequest();
    public delegate bool ComputeNetworkData(NetworkData data);
    public delegate void CloseConnection();

    public class NetworkConnection
    {
        public int maxNumberOfConnections;
        public string serverName;
        public int hostPort;

        // Delegates
        public SendRequest methodToSendRequest;
        public ComputeNetworkData methodToComputeData;
        public CloseConnection methodToAbandonConnection;

        #region Network Transport Code (just in case we switch one day)

        /*
        #region Network Transport Code

        // Network transport info
        public bool useNetworkTransport;

        public string serverName;
        public int hostPort;
        private int hostId;

        public int maxNumberOfConnections;

        private int myCurrentConnectionID;

        private int myReliableChannelId;
        private int myUnreliableChannelId;

        public void StartConnectionUsingNetworkTransport()
        {
            // An example of initializing the Transport Layer with custom settings

            //GlobalConfig gConfig = new GlobalConfig();
            //gConfig.MaxPacketSize = 500; // bytes
            //NetworkTransport.Init(gConfig);

            NetworkTransport.Init();

            // define several communication channels
            ConnectionConfig config = new ConnectionConfig();
            myReliableChannelId = config.AddChannel(QosType.Reliable);
            myUnreliableChannelId = config.AddChannel(QosType.Unreliable);

            // how many configurations allowed on host
            HostTopology topology = new HostTopology(config, maxNumberOfConnections);

            // create host using config
            hostId = NetworkTransport.AddHost(topology, hostPort);


            // trying to connect to host
            byte error;
            Debug.Log("call to getHostEntry");
            System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(serverName);
            string ipAdress = hostEntry.AddressList[0].ToString();
            Debug.Log("NetworkTransport.Connect(" + hostId + ", " + ipAdress + ", " + hostPort + ", 0, out error)");
            myCurrentConnectionID = NetworkTransport.Connect(hostId, ipAdress, hostPort, 0, out error);
            Debug.Log("Connect error = " + error.ToString());

            // trying to disconnect from host
            //NetworkTransport.Disconnect(hostId, myCurrentConnectionID, out error);
        }

        // Exclusive for NetworkTransport class
        private void CheckNetworkReceiveSignals()
        {
            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            byte error;

            NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);
            switch (recData)
            {
                case NetworkEventType.Nothing:
                    {
                        // no event received from network
                        break;
                    }
                case NetworkEventType.ConnectEvent:
                    {
                        // Connection event come in. It can be new connection, or it can be response on previous connect command
                        if (myCurrentConnectionID == connectionId)
                        {
                            // my active connect request approved
                            Debug.Log("NETWORK : my active connect request approved (connectionId = " + connectionId + ")");

                            // trying to send message to host
                            StartSendingData();
                        }
                        else
                        {
                            // somebody else connect to me
                            Debug.Log("NETWORK : somebody else connect to me (connectionId = " + connectionId + ")");
                        }
                        break;
                    }
                case NetworkEventType.DataEvent:
                    {
                        // Data received. 
                        // In this case recHostId will define host,
                        // connectionId will define connection, 
                        // channelId will define channel;
                        // dataSize will define size of the received data. 
                        // If recBuffer is big enough to contain data, data will be copied in the buffer. 
                        // If not, error will contain MessageToLong error and you will need reallocate buffer and call this function again.
                        Debug.Log("NETWORK : data receive from host (ID:" + recHostId + ") connection (ID:" + connectionId + ") channel (ID:" + channelId + ") data size = " + dataSize);
                        Debug.Log("NETWORK : error = " + error.ToString());
                        Debug.Log("NETWORK : data = " + Tools.GetStringFromByteArray(recBuffer));

                        // TODO : do something with those data depending on their type
                        //object data = NetworkData.CreateFromJSON(Tools.GetStringFromByteArray(recBuffer)).GetData();
                    }
                    break;
                case NetworkEventType.DisconnectEvent:
                    {
                        if (myCurrentConnectionID == connectionId)
                        {
                            //cannot connect by some reason see error
                            Debug.Log("NETWORK : can't connect. Error = " + error.ToString());
                        }
                        else
                        {
                            //one of the established connection has been disconnected
                            Debug.Log("NETWORK : one of the established connection has been disconnected");

                            StopSendingData();
                        }
                        break;
                    }
            }
        }

        private void SendMessageUsingNetworkTransport(string message)
        {
            byte[] buffer = Tools.GetByteArrayFromString(message);
            int bufferLength = buffer.Length;
            byte error;
            NetworkTransport.Send(hostId, myCurrentConnectionID, myUnreliableChannelId, buffer, bufferLength, out error);
            Debug.Log("Send message error = " + error.ToString());
        }

        #endregion


        #region TCP Client Code

        private bool tcpConnectionReady;
        private TcpClient tcpClient;
        private NetworkStream tcpNetworkStream;
        private StreamReader tcpStreamReader;
        private StreamWriter tcpStreamWriter;

        public void StartConnectionUsingTCPClient()
        {
            //if already connected , ignore this function
            if (tcpConnectionReady)
            {
                return;
            }

            // Get server IP Adress
            System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(serverName);
            string ipAdress = hostEntry.AddressList[0].ToString();

            // Create the socket
            try
            {
                Debug.Log(" new TcpClient(" + ipAdress + ", " + hostPort + ");");
                tcpClient = new TcpClient(ipAdress, hostPort);
                tcpConnectionReady = true;

                tcpNetworkStream = tcpClient.GetStream();
                tcpStreamReader = new StreamReader(tcpNetworkStream);
                tcpStreamWriter = new StreamWriter(tcpNetworkStream);

                StartSendingData();
            }
            catch (System.Exception e)
            {
                tcpConnectionReady = false;
                Debug.Log("socket error: " + e.Message);
            }
        }

        public void DropConnectionUsingTCPClient()
        {
            tcpClient.Close();
            tcpConnectionReady = false;
        }

        public string ReadTCPClientStream()
        {
            string data = null;
            if (tcpConnectionReady)
            {
                data = tcpStreamReader.ReadLine();
                Debug.Log("READ : " + data);
            }
            else
            {
                Debug.Log("Can't read stream because connection failed");
            }
            return data;
        }

        public void WriteTCPClientStream(string data)
        {
            tcpStreamWriter.Write(data);
            tcpStreamWriter.Flush();
            Debug.Log("WRITE : " + data);
        }

        private void CheckTCPClientReceiveSignals()
        {
            if (tcpConnectionReady)
            {
                string data = ReadTCPClientStream();
                try
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        NetworkData networkData = NetworkData.CreateFromJSON(data);
                        UpdateFromNetworkData(networkData);
                    }
                    else
                    {
                        Debug.LogWarning("CheckTCPClientReceiveSignals() : received empty data");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Exception occured in CheckTCPClientReceiveSignals() : Exception is : " + e.Message);
                }
            }
        }

        #endregion
    */

        #endregion

        private bool _tcpConnectionReady;

        private TcpClient _tcpClient;
        private NetworkStream _tcpNetworkStream;
        private StreamReader _tcpStreamReader;
        private StreamWriter _tcpStreamWriter;

        public NetworkConnection(string server, int port)
        {
            _tcpConnectionReady = false;

            serverName = server;
            hostPort = port;
            maxNumberOfConnections = 1;
            methodToSendRequest = delegate ()
            {
                Debug.LogError("SendRequest() : delegate not implemented");
            };
            methodToComputeData = delegate (NetworkData input)
            {
                Debug.LogError("ComputeNetworkData() : delegate not implemented");
                return false;
            };
            methodToAbandonConnection = delegate ()
            {
                Debug.LogError("CloseConnection() : delegate not implemented");
            };
        }

        private void OpenStreamsAndSendRequest(IAsyncResult result)
        {
            if (_tcpClient.Connected) //Connected to host, do something
            {
                try
                {
                    Debug.Log("Client is Connected");
                    _tcpConnectionReady = true;

                    _tcpNetworkStream = _tcpClient.GetStream();
                    _tcpStreamReader = new StreamReader(_tcpNetworkStream);
                    _tcpStreamWriter = new StreamWriter(_tcpNetworkStream);

                    methodToSendRequest();
                }
                catch (System.Exception e)
                {
                    _tcpConnectionReady = false;
                    Debug.LogError("OpenStreamsAndSendRequest() error: " + e.Message);
                    methodToAbandonConnection();
                }
            }
            else
            {
                // Not connected, do something
                Debug.LogWarning("Client is Not Connected");
                methodToAbandonConnection();
            }
        }

        public void StartConnectionUsingTCPClient()
        {
            //if already connected , ignore this function
            if (_tcpConnectionReady)
            {
                return;
            }

            // Get server IP Adress
            System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(serverName);
            string ipAdress = hostEntry.AddressList[0].ToString();
            Debug.Log("IP of " + serverName + " is " + ipAdress);

            // Create the socket
            try
            {
                Debug.Log("New TcpClient(" + ipAdress + ", " + hostPort + ");");

                AsyncCallback callBack = new AsyncCallback(this.OpenStreamsAndSendRequest);
                _tcpClient = new TcpClient();
                _tcpClient.BeginConnect(ipAdress, hostPort, callBack, _tcpClient);
            }
            catch (System.Exception e)
            {
                _tcpConnectionReady = false;
                Debug.LogError("Socket error: " + e.Message);
                methodToAbandonConnection();
            }
        }

        public void DropConnectionUsingTCPClient()
        {
            if (_tcpConnectionReady)
            {
                _tcpClient.Close();
                _tcpConnectionReady = false;
            }
        }

        public string ReadTCPClientStream()
        {
            string data = null;
            if (_tcpConnectionReady)
            {
                data = _tcpStreamReader.ReadLine();
                if (!string.IsNullOrEmpty(data))
                {
                    Debug.Log("READ : " + data);
                }
                else
                {
                    Debug.Log("READ : Empty line");
                }
            }
            else
            {
                Debug.LogError("Can't read stream because connection failed");
                methodToAbandonConnection();
            }
            return data;
        }

        public void WriteTCPClientStream(string data)
        {
            if (_tcpConnectionReady)
            {
                _tcpStreamWriter.Write(data+"\n");
                _tcpStreamWriter.Flush();
                Debug.Log("WRITE : " + data);
            }
            else
            {
                Debug.LogError("Can't write stream because connection failed");
                methodToAbandonConnection();
            }
        }

        public void CheckTCPClientReceiveSignals()
        {
            if (_tcpConnectionReady)
            {
                string data = ReadTCPClientStream();
                try
                {
                    if (!string.IsNullOrEmpty(data))
                    {
                        NetworkData networkData = NetworkData.CreateFromJSON(data);
                        if (networkData != null && !string.IsNullOrEmpty(networkData.ToJSON()))
                        {
                            Debug.Log("DATA RECEIVED: " + data);
                            methodToComputeData(networkData);
                        }
                        else
                        {
                            Debug.LogWarning("DATA RECEIVED ARE CORRUPTED: " + data);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Exception occured in CheckTCPClientReceiveSignals() : Exception is : " + e.Message);
                }
            }

            // Detect if client disconnected
            try
            {
                if (_tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (_tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        // Client disconnected
                        Debug.Log("Client not connected");
                        methodToAbandonConnection();
                    }
                    else
                    {
                        // Client still connected
                        Debug.Log("Client still connected, waiting message");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception in read line thread: " + ex.Message);
            }
        }
    }
}
