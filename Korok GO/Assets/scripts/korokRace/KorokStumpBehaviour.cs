using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KorokStumpBehaviour : MonoBehaviour
{
    public int tileX;
    public int tileY;

    public bool solved;
    public bool raceIsRunning;

    public GameObject korokRaceStartBlastPrefab;

	// Use this for initialization
	void Start ()
    {
        solved = false;
    }
	/*
    void OnTriggerEnter(Collider other)
    {
        if (!solved && !raceIsRunning && other.tag.Equals("Player"))
        {
            // play sound : TODO
            raceIsRunning = true;
            SendBlast();
        }
    }*/

    void OnMouseDown()
    {
        float distanceWithPlayer = Vector3.Distance(this.transform.position, MapPlayerBehaviour.instance.transform.position);
        bool isInReach = distanceWithPlayer <= MapPlayerBehaviour.instance.playerReach;
        if (!solved && !raceIsRunning && isInReach)
        {
            raceIsRunning = true;
            SendBlast();
        }
    }

    private void SendBlast()
    {
        float distanceInRealWorld = 50 + 10 * (((tileX % 7) + tileY) % 6);
        Vector2 latlonOfTile = MapGeneratorOSM.instance.TilePosToWorldPos(tileX, tileY);
        int nextTileX = tileX + 1;
        nextTileX = (nextTileX > Mathf.Pow(2, MapGeneratorOSM.instance.zoomLevel)) ? 0 : nextTileX;
        Vector2 latlonOfNextTile = MapGeneratorOSM.instance.TilePosToWorldPos(nextTileX, tileY);
        double realDistanceBetweenTiles = Tools.getDistanceFromLatLonInM(latlonOfTile.x, latlonOfTile.y, latlonOfNextTile.x, latlonOfNextTile.y);
        double distanceInScene = (distanceInRealWorld / realDistanceBetweenTiles) * 5;

        float angle = Mathf.Deg2Rad * (Vector3.SignedAngle(Vector3.forward, this.transform.forward, Vector3.up));
        Vector3 direction = Mathf.Cos(angle) * Vector3.forward + Mathf.Sin(angle) * Vector3.right;
        GameObject blastGo = Instantiate(korokRaceStartBlastPrefab, this.transform.position, Quaternion.identity);
        float targetSpeedInKmH = 2 + (tileX % 4);
        float targetSpeedInMS = (targetSpeedInKmH * 1000) / 3600;
        float timerValueInSeconds = distanceInRealWorld / targetSpeedInMS;
        blastGo.GetComponent<KorokRaceStartBlastBehaviour>().SetInfo(tileX, tileY, timerValueInSeconds, this.transform.position + direction * (float)distanceInScene, this);        
    }

    public void Solved()
    {
        raceIsRunning = false;
        solved = true;
    }

    public void Failed()
    {
        raceIsRunning = false;
        solved = false;
    }
}
