using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelBehaviour : MonoBehaviour
{ 
    public WayPattern patternOfBalloons;
    public float distanceOfBalloonsFromWheel;
    public int numberOfBalloons;
    public float linearSpeedOfBalloons;

    private List<TargetBalloonBehaviour> balloons;
    private bool challengeIsActive;
    private bool challengeIsSolved;

    public GameObject balloonPrefab;

    private int tileX;
    private int tileY;

    void Start()
    {
        challengeIsActive = false;
        challengeIsSolved = false;
        balloons = new List<TargetBalloonBehaviour>();
    }

    void Update()
    {
        if (challengeIsActive && !challengeIsSolved && CheckIfBalloonsArePoped())
        {
            challengeIsSolved = true;
            SpawnKorok();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!challengeIsSolved && other.tag.Equals("Player"))
        {
            SpawnBalloons();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!challengeIsSolved && other.tag.Equals("Player"))
        {
            UnSpawnBalloons();
        }
    }

    public void SetPattern(int x, int y)
    {
        tileX = x;
        tileY = y;
    }

    public void SpawnBalloons()
    {
        challengeIsActive = true;        
        long v = ((long)tileX * 20 + tileY) % 5;
        if (v == 0)
        {
            patternOfBalloons = WayPattern.STATIONARY;
            distanceOfBalloonsFromWheel = 5 + (tileX % 10);
            numberOfBalloons = 3 + (tileY % 3);
            float angle = 0;
            for (int i = 0; i < numberOfBalloons; i++)
            {
                angle = (i * 1.0f / numberOfBalloons) * Mathf.PI * 2;
                Vector3 balloonPosition = this.transform.position + distanceOfBalloonsFromWheel * (Mathf.Cos(angle) * Vector3.right + Mathf.Sin(angle) * Vector3.forward);
                GameObject balloon = Instantiate(balloonPrefab, balloonPosition, Quaternion.identity);
                balloon.GetComponent<TargetBalloonBehaviour>().SetPattern(patternOfBalloons, false, 0, 0);
                balloons.Add(balloon.GetComponent<TargetBalloonBehaviour>());
            }
        }
        else if (v == 1)
        {
            patternOfBalloons = WayPattern.STATIONARY;
            distanceOfBalloonsFromWheel = 10 + (tileX % 5) + Random.Range(-5, 15);
            numberOfBalloons = 3 + 2 * (tileY % 2);
            for (int i = 0; i < numberOfBalloons; i++)
            {
                float angle = Random.Range(-Mathf.PI, Mathf.PI);
                Vector3 balloonPosition = this.transform.position + distanceOfBalloonsFromWheel * (Mathf.Cos(angle) * Vector3.right + Mathf.Sin(angle) * Vector3.forward);
                GameObject balloon = Instantiate(balloonPrefab, balloonPosition, Quaternion.identity);
                balloon.GetComponent<TargetBalloonBehaviour>().SetPattern(patternOfBalloons, false, 0, 0);
                balloons.Add(balloon.GetComponent<TargetBalloonBehaviour>());
            }
        }
        else if (v == 2)
        {
            patternOfBalloons = WayPattern.HORIZONTAL_CIRCLE;
            distanceOfBalloonsFromWheel = 5 + (tileX % 10);
            numberOfBalloons = 3 + 2 * (tileY % 5);
            bool reverse = (tileX % 2) == 0;
            float radiusOfBalloonPath = 2 + (tileY % 3);
            for (int i = 0; i < numberOfBalloons; i++)
            {
                Vector3 balloonPosition = this.transform.position - i * 2 * Vector3.right;
                radiusOfBalloonPath = 2 * i + 1;
                reverse = !reverse;
                GameObject balloon = Instantiate(balloonPrefab, balloonPosition, Quaternion.identity);
                float radialSpeed = linearSpeedOfBalloons / (Mathf.PI * 2 * radiusOfBalloonPath);
                radialSpeed = 2;
                balloon.GetComponent<TargetBalloonBehaviour>().SetPattern(patternOfBalloons, reverse, radialSpeed, radiusOfBalloonPath);
                balloons.Add(balloon.GetComponent<TargetBalloonBehaviour>());
            }
        }
        else if (v == 3)
        {
            patternOfBalloons = WayPattern.HORIZONTAL_CIRCLE;
            distanceOfBalloonsFromWheel = 10 + (tileX % 5) + Random.Range(-5, 15);
            numberOfBalloons = 3 + 2 * (tileY % 2);
            bool reverse = Random.Range(0, 2) == 0;
            float radialSpeed = Random.Range(0.5f, 2);
            float radius = 3 + tileX % 6 + tileY % 7;
            for (int i = 0; i < numberOfBalloons; i++)
            {
                reverse = Random.Range(0, 2) == 0;
                float angle = Random.Range(-Mathf.PI, Mathf.PI);
                Vector3 balloonPosition = this.transform.position + distanceOfBalloonsFromWheel * (Mathf.Cos(angle) * Vector3.right + Mathf.Sin(angle) * Vector3.forward);
                GameObject balloon = Instantiate(balloonPrefab, balloonPosition, Quaternion.identity);
                balloon.GetComponent<TargetBalloonBehaviour>().SetPattern(patternOfBalloons, reverse, radialSpeed, radius);
                balloons.Add(balloon.GetComponent<TargetBalloonBehaviour>());
            }
        }
        else if (v == 4)
        {
            patternOfBalloons = WayPattern.VERTICAL_CIRCLE;
            distanceOfBalloonsFromWheel = 10 + (tileX % 5) + Random.Range(-5, 15);
            numberOfBalloons = 3 + 2 * (tileY % 2);
            bool reverse = Random.Range(0, 2) == 0;
            float radialSpeed = Random.Range(1, 2.5f);
            float radius = 3 + tileX % 2 + tileY % 3;
            for (int i = 0; i < numberOfBalloons; i++)
            {
                reverse = Random.Range(0, 2) == 0;
                float angle = Random.Range(-Mathf.PI, Mathf.PI);
                Vector3 balloonPosition = this.transform.position + distanceOfBalloonsFromWheel * (Mathf.Cos(angle) * Vector3.right + Mathf.Sin(angle) * Vector3.forward);
                GameObject balloon = Instantiate(balloonPrefab, balloonPosition, Quaternion.identity);
                balloon.GetComponent<TargetBalloonBehaviour>().SetPattern(patternOfBalloons, reverse, radialSpeed, radius);
                balloons.Add(balloon.GetComponent<TargetBalloonBehaviour>());
            }
        }
    }

    public void UnSpawnBalloons()
    {
        foreach(TargetBalloonBehaviour balloon in balloons)
        {
            balloon.UnSpawn();
        }
        challengeIsActive = false;
        balloons.Clear();
    }

    public bool CheckIfBalloonsArePoped()
    {
        bool res = true;
        foreach (TargetBalloonBehaviour balloon in balloons)
        {
            res &= balloon.poped;
        }
        return res;
    }

    public void SpawnKorok()
    {
        Controller.instance.FindKorok(this.transform.position, tileX, tileY);
    }
}
