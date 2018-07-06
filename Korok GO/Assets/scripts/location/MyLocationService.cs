using UnityEngine;
using System.Collections;

namespace Assets.scripts.Location
{
    class MyLocationService
    {
        public static MyLocationService instance;

        public GeoPoint playerLocation = new GeoPoint();

        public bool locationServiceIsRunning = false;
        public int maxWait = 30; // seconds
        private float locationUpdateInterval = 0.2f; // seconds

        //private double lastLocUpdate = 0.0; //seconds
        

        public IEnumerator StartLocationService()
        {
            Debug.Log("Player Loc started.");

            // First, check if user has location service enabled
            if (Application.platform.Equals(RuntimePlatform.WindowsPlayer) || 
                Application.platform.Equals(RuntimePlatform.LinuxPlayer) || 
                Application.platform.Equals(RuntimePlatform.WindowsEditor))
            {
                Debug.LogWarning("Locations is disabled on Windows Player, Linux Player, or in Editor mode (Windows)");

                //NOTE: If location is not enabled, we initialize the postion of the player to somewhere in Ingré, just for demonstration purposes
                playerLocation.setLatLon_deg(47.919281f, 1.798275f);

                // To get the game run on Editor without location services
                locationServiceIsRunning = true;
                yield break;
            }
            else
            {
                // Start service before querying location
                Input.location.Start();

                // Wait until service initializes
                while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
                {
                    yield return new WaitForSeconds(1);
                    maxWait--;
                }

                // Service didn't initialize in maxWait seconds
                if (maxWait < 1)
                {
                    Debug.LogError("Locations services timed out.");
                    yield break;
                }

                // Connection has failed
                if (Input.location.status == LocationServiceStatus.Failed)
                {
                    Debug.LogError("Location services failed.");
                    yield break;
                }
                else if (Input.location.status == LocationServiceStatus.Running)
                {
                    playerLocation.setLatLon_deg(Input.location.lastData.latitude, Input.location.lastData.longitude);
                    Debug.Log("Location: " + Input.location.lastData.latitude.ToString("R") + " " + Input.location.lastData.longitude.ToString("R") + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
                    locationServiceIsRunning = true;

                    //Input.compass.enabled = true;
                    //lastLocUpdate = Input.location.lastData.timestamp;
                }
                else
                {
                    Debug.LogError("Unknown Error. Status : " + Input.location.status.ToString());
                }
            }
        }

        public IEnumerator RunLocationService()
        {
            double lastLocUpdate = 0.0;
            while (locationServiceIsRunning)
            {
                if (lastLocUpdate != Input.location.lastData.timestamp)
                {
                    playerLocation.setLatLon_deg(Input.location.lastData.latitude, Input.location.lastData.longitude);
                    lastLocUpdate = Input.location.lastData.timestamp;
                }
                yield return new WaitForSeconds(locationUpdateInterval);
            }
        }
    }
}
