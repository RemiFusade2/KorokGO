using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KorokManager : MonoBehaviour
{
    public static KorokManager instance;

    public Texture2D korokOnMapMask;

    public GameObject movingKorokPrefab;
    public GameObject korokUnderARockPrefab;
    public GameObject korokRockCirclePrefab;
    public GameObject korokFlowerTrackPrefab;
    public GameObject raceStumpKorokPrefab;
    public GameObject KorokWithTargetsPrefab;

    public GameObject appearingKorokPrefab;

    public bool resetData;

    void Awake()
    {
        KorokManager.instance = this;

        if (resetData)
        {
            PlayerPrefs.DeleteAll();
        }
    }
    

    public void AddKorokOnMap(Vector3 position, int xID, int yID)
    {
        bool korokAlreadyFound = PlayerPrefs.GetString(KorokBehaviour.GetHashCodeForIDs(xID, yID), "").Contains("Found");

        int x = xID % 256;
        int y = yID % 256;
        bool korokAtThisPlace = korokOnMapMask.GetPixel(x, y) == Color.black;
        
        if (korokAtThisPlace)
        {
            if (korokAlreadyFound)
            {
                // Found korok
                string korokPositionStr = PlayerPrefs.GetString(KorokBehaviour.GetHashCodeForIDs(xID, yID));
                string[]korokCoords = korokPositionStr.Split(',');
                if (korokCoords.Length >= 3)
                {
                    Vector3 korokPositionRelativeToTile = new Vector3(float.Parse(korokCoords[0]), float.Parse(korokCoords[1]), float.Parse(korokCoords[2]));
                    GameObject korokTile = MapGeneratorOSM.instance.GetTileFromCoords(xID, yID);
                    Vector3 korokPosition = korokTile.transform.position + korokPositionRelativeToTile;
                    AddFoundKorok(korokPosition);
                }
            }
            else
            {
                // Challenge
                int value = ((yID % 7) * 5 + xID) % 7;
                value = value % 5;
                if (value == 0)
                {
                    // moving invisible korok
                    GameObject korok = Instantiate(movingKorokPrefab, position, Quaternion.identity, this.transform);
                    korok.transform.GetChild(0).GetComponent<KorokBehaviour>().xID = xID;
                    korok.transform.GetChild(0).GetComponent<KorokBehaviour>().yID = yID;
                    korok.transform.GetChild(0).GetComponent<KorokBehaviour>().SetAnimation();
                }
                else if (value == 1)
                {
                    // pinwheel
                    Quaternion orientation = Quaternion.Euler(0, 90 * (xID % 5) + 18 * (yID % 11), 0);
                    GameObject wheel = Instantiate(KorokWithTargetsPrefab, position, orientation, this.transform);
                    wheel.GetComponent<WheelBehaviour>().SetPattern(xID, yID);
                }
                else if (value == 2)
                {
                    // stump
                    Quaternion orientation = Quaternion.Euler(0, 90 * (xID % 5) + 18 * (yID % 11), 0);
                    GameObject stump = Instantiate(raceStumpKorokPrefab, position, orientation, this.transform);
                    stump.GetComponent<KorokStumpBehaviour>().tileX = xID;
                    stump.GetComponent<KorokStumpBehaviour>().tileY = yID;
                }
                else
                {
                    // flower
                    Quaternion orientation = Quaternion.Euler(0, 90 * (yID % 5) + 18 * (xID % 11), 0);
                    GameObject flower = Instantiate(korokFlowerTrackPrefab, position, orientation, this.transform);
                    flower.GetComponent<FlowerBehaviour>().tileX = xID;
                    flower.GetComponent<FlowerBehaviour>().tileY = yID;
                    flower.GetComponent<FlowerBehaviour>().remainingFlowers = 3 + ((xID % 5) + yID) % 2;
                }

            }
        }
    }

    public void AddFoundKorok(Vector3 position)
    {
        Instantiate(appearingKorokPrefab, new Vector3(position.x, 3, position.z), Quaternion.identity, this.transform);
    }


}
