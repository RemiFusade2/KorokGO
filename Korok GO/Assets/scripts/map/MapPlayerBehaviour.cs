using Assets.scripts.Location;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionMeasure
{
    public float lattitude;
    public float longitude;
    public long timestamp;
}

public class MapPlayerBehaviour : MonoBehaviour
{
    public static MapPlayerBehaviour instance;

    [Header("References in scene")]
    public MapGeneratorOSM mapGenerator;
    public Transform playerTransform;
    public Rigidbody playerRigidbody;
    public Animator playerAnimator;
    public Transform playerMouvTarget;
    public Transform playerReachAreaTransform;

    [Header("Player info on Map")]
    public bool playerPositionIsValid;
    public float playerLat;
    public float playerLon;
    public float playerReach;

    [Header("Debug")]
    public bool editorMode;
    public float editorModePlayerSpeed;

    public void SetPlayerReach(float newReach)
    {
        playerReach = newReach;
        float epsilon = 0.01f;
        playerReachAreaTransform.localScale = new Vector3(playerReach * 2, playerReach * 2, playerReach * 2);
        playerReachAreaTransform.localPosition = Vector3.up * (epsilon * playerReach - 2.5f);
    }

    private List<PlayerPositionMeasure> mouvementMeasures;

    private PlayerPositionMeasure GetLastMeasure()
    {
        if (mouvementMeasures.Count <= 0)
            return null;
        return mouvementMeasures[mouvementMeasures.Count - 1];
    }

    public void AddPlayerPositionMeasure(float lat, float lon)
    {
        PlayerPositionMeasure measure = new PlayerPositionMeasure() { lattitude = lat, longitude = lon, timestamp = DateTime.Now.Ticks };
        mouvementMeasures.Add(measure);
        RemoveUnusedPlayerPositionMeasures();
    }

    private void RemoveUnusedPlayerPositionMeasures()
    {
        while (mouvementMeasures.Count > 10)
        {
            mouvementMeasures.RemoveAt(0);
        }
    }

    /// <summary>
    /// Return player speed in m/s (in real world)
    /// </summary>
    /// <returns></returns>
    private float PlayerCurrentSpeedFromMeasures()
    {
        float speed = 0;
        if (mouvementMeasures.Count > 1)
        {
            PlayerPositionMeasure oldestMeasure = mouvementMeasures[0];
            PlayerPositionMeasure currentMeasure = mouvementMeasures[mouvementMeasures.Count - 1];
            double distanceInMeters = Tools.getDistanceFromLatLonInM(currentMeasure.lattitude, currentMeasure.longitude, oldestMeasure.lattitude, oldestMeasure.longitude);
            float timeInSeconds = Mathf.Abs((currentMeasure.timestamp - oldestMeasure.timestamp) * 1.0f / TimeSpan.TicksPerSecond);
            speed = (float)( distanceInMeters / timeInSeconds );
        }
        return speed;
    }

    private PlayerPositionMeasure NextPositionEstimated()
    {
        PlayerPositionMeasure estimatedNextMeasure = null;
        if (mouvementMeasures.Count > 1)
        {
            // at least 2 measures available
            PlayerPositionMeasure lastMeasure = mouvementMeasures[mouvementMeasures.Count - 1];
            PlayerPositionMeasure oldMeasure = mouvementMeasures[mouvementMeasures.Count - 2];
            estimatedNextMeasure = new PlayerPositionMeasure()
            {
                longitude = lastMeasure.longitude * 2 - oldMeasure.longitude,
                lattitude = lastMeasure.lattitude * 2 - oldMeasure.lattitude,
                timestamp = lastMeasure.timestamp * 2 - oldMeasure.timestamp
            };
        }
        else if (mouvementMeasures.Count == 1)
        {
            // only one measure available
            estimatedNextMeasure = mouvementMeasures[0];
        }
        return estimatedNextMeasure;
    }

    void Awake()
    {
        MapPlayerBehaviour.instance = this;

        mouvementMeasures = new List<PlayerPositionMeasure>();
    }

    // Use this for initialization
    void Start()
    {
        if (MyLocationService.instance == null || editorMode || Application.platform != RuntimePlatform.Android)
        {
            AddManualPlayerPositionMeasure(playerLat, playerLon);
        }
        StartCoroutine(WaitAndUpdatePlayerMovement());

        this.SetPlayerReach(this.playerReach);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (MyLocationService.instance == null || editorMode || Application.platform != RuntimePlatform.Android)
        {
            // editor mode : player location depends on inputs
            float latStep = 0.001f * editorModePlayerSpeed;
            float lonStep = 0.001f * editorModePlayerSpeed;

            float newPlayerLat = playerLat;
            float newPlayerLon = playerLon;

            bool spacePressed = Input.GetKeyDown(KeyCode.Space);
            bool leftArrowPressed = Input.GetKeyDown(KeyCode.LeftArrow);
            bool rightArrowPressed = Input.GetKeyDown(KeyCode.RightArrow);
            bool upArrowPressed = Input.GetKeyDown(KeyCode.UpArrow);
            bool downArrowPressed = Input.GetKeyDown(KeyCode.DownArrow);

            bool newInput = spacePressed || leftArrowPressed || rightArrowPressed || upArrowPressed || downArrowPressed;

            if (leftArrowPressed)
            {
                newPlayerLon -= lonStep;
            }
            if (rightArrowPressed)
            {
                newPlayerLon += lonStep;
            }
            newPlayerLon = (newPlayerLon < -180) ? (newPlayerLon + 360) : ((newPlayerLon > 180) ? (newPlayerLon - 360) : newPlayerLon);
            if (upArrowPressed)
            {
                newPlayerLat += latStep;
            }
            if (downArrowPressed)
            {
                newPlayerLat -= latStep;
            }
            newPlayerLat = (newPlayerLat < -90) ? -90 : ((newPlayerLat > 90) ? 90 : newPlayerLat);

            if (newInput)
            {
                AddManualPlayerPositionMeasure(newPlayerLat, newPlayerLon);
                playerLat = newPlayerLat;
                playerLon = newPlayerLon;
            }

            playerPositionIsValid = true;
        }
        else if (MyLocationService.instance.locationServiceIsRunning)
        {
            // release mode : player location depends on GPS
            GeoPoint playerLocation = MyLocationService.instance.playerLocation;
            playerLon = playerLocation.lon_d;
            playerLat = playerLocation.lat_d;
            AddManualPlayerPositionMeasure(playerLat, playerLon);

            playerPositionIsValid = true;
        }

        if (playerPositionIsValid && mapGenerator.mapIsReady)
        {
            PlayerPositionMeasure playerPosition = GetLastMeasure();
            if (playerPosition != null)
            {
                Vector3 playerPositionInScene = mapGenerator.GetWorldPositionFromLatLon(playerPosition.lattitude, playerPosition.longitude);
                playerMouvTarget.position = playerPositionInScene + 2.5f * Vector3.up;
            }
        }
    }

    private void AddManualPlayerPositionMeasure(float lat, float lon)
    {
        this.AddPlayerPositionMeasure(lat, lon);
    }

    private IEnumerator WaitAndUpdatePlayerMovement()
    {
        yield return new WaitForEndOfFrame();
        if (playerPositionIsValid && mapGenerator.mapIsReady)
        {
            UpdatePlayerMovement();
        }
        StartCoroutine(WaitAndUpdatePlayerMovement());
    }

    private void UpdatePlayerMovement()
    {
        Vector3 currentPlayerPosition = playerTransform.position;
        Vector3 destinationPlayerPosition = playerMouvTarget.position;
        Vector3 directionToDestination = (destinationPlayerPosition - currentPlayerPosition).normalized;

        float distanceInScene = Vector3.Distance(currentPlayerPosition, destinationPlayerPosition);
        float estimatedSpeedInWorld = this.PlayerCurrentSpeedFromMeasures();
        float speedComputedFromMeasures = estimatedSpeedInWorld * 0.02f;
        float speedComputedFromDistance = distanceInScene * 0.4f;
        float maxDistanceInScene = 5;
        float minDistanceInScene = 0.2f;
        float t = (distanceInScene - minDistanceInScene) / (maxDistanceInScene - minDistanceInScene);
        t = (t < 0) ? 0 : ((t > 1) ? 1 : t);

        float speed = speedComputedFromDistance * t + speedComputedFromMeasures * (1 - t);
        

        if (distanceInScene > 100)
        {
            // teleport
            playerRigidbody.MovePosition(playerMouvTarget.position);
        }
        else
        {
            // move
            if (distanceInScene > 0.1f)
            {
                speed = speed < 0.04f ? 0.04f : speed;
                playerRigidbody.velocity = directionToDestination * speed;

                // turn
                float signedAngleBetweenCurrentAndTargetOrientation = Vector3.SignedAngle(playerTransform.forward, directionToDestination, Vector3.up) * Mathf.Deg2Rad;
                float angularSpeedInRadians = 1.5f;
                if (Mathf.Abs(signedAngleBetweenCurrentAndTargetOrientation) > 0.01f)
                {
                    playerRigidbody.angularVelocity = new Vector3(0, angularSpeedInRadians * signedAngleBetweenCurrentAndTargetOrientation, 0);
                }
                else
                {
                    playerRigidbody.angularVelocity = Vector3.zero;
                }

                UpdateAnimatorStatus(speed, signedAngleBetweenCurrentAndTargetOrientation);
            }
            else
            {
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
                UpdateAnimatorStatus(0, 0);
            }
        }
    }

    private void UpdateAnimatorStatus(float speed, float directionAngle)
    {
        playerAnimator.SetFloat("Speed", (speed > 0 && speed < 0.1f) ? 0.1f : speed );
        playerAnimator.SetFloat("Direction", directionAngle);
    }
}
