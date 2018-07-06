using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerBehaviour : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip spawnClip;
    public AudioClip unspawnClip;

    public int tileX;
    public int tileY;

    public int remainingFlowers;

    public List<MeshRenderer> petals;
    public MeshRenderer pistil;
    public Material whitePetalMat;
    public Material whitePistilMat;
    public Animator animator;
    public ParticleSystem particles;

    private bool found;

    public string ValueForFlower()
    {
        string hash = "";
        hash = this.transform.position.x + "," + this.transform.position.y + "," + this.transform.position.y + ",Flower," + remainingFlowers;
        return hash;
    }

    // Use this for initialization
    void Start ()
    {
        found = false;
        animator.SetBool("Visible", true);

        if (remainingFlowers == 0)
        {
            pistil.material = whitePistilMat;
            foreach (MeshRenderer renderer in petals)
            {
                renderer.material = whitePetalMat;
            }
        }

        audioSource.Stop();
        audioSource.clip = spawnClip;
        audioSource.Play();
    }

    public void SetInfo(int x, int y, int remaining, bool firstSpawn)
    {
        tileX = x;
        tileY = y;
        remainingFlowers = remaining;
        if (!firstSpawn)
        {

        }
    }

    void OnMouseDown()
    {
        float distanceWithPlayer = Vector3.Distance(this.transform.position, MapPlayerBehaviour.instance.transform.position);
        bool isInReach = distanceWithPlayer <= MapPlayerBehaviour.instance.playerReach;
        if (!found && isInReach)
        {
            found = true;
            if (remainingFlowers == 0)
            {
                // Find Korok !
                AudioManager.instance.PlayVictory();
                StartCoroutine(WaitAndDisappear(0));
                StartCoroutine(WaitAndSpawnKorok(2));
            }
            else
            {
                // Spawn next flower
                StartCoroutine(WaitAndDisappear(0));
                StartCoroutine(WaitAndSpawnNextFlower(1));
            }
        }
    }

    /*
    void OnTriggerEnter(Collider other)
    {
        if (!found && other.tag.Equals("Player"))
        {
            found = true;
            if (remainingFlowers == 0)
            {
                // Find Korok !
                AudioManager.instance.PlayVictory();
                StartCoroutine(WaitAndDisappear(0));
                StartCoroutine(WaitAndSpawnKorok(2));
            }
            else
            {
                // Spawn next flower
                StartCoroutine(WaitAndDisappear(0));
                StartCoroutine(WaitAndSpawnNextFlower(1));
            }
        }
    }*/

    private IEnumerator WaitAndDisappear(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetBool("Visible", false);
        particles.Stop();
        particles.Play();
        audioSource.Stop();
        audioSource.clip = unspawnClip;
        audioSource.Play();
        StartCoroutine(WaitAndDestroyItself(2));
    }

    private IEnumerator WaitAndSpawnKorok(float delay)
    {
        yield return new WaitForSeconds(delay);
        Controller.instance.FindKorok(this.transform.position, tileX, tileY);
    }

    private IEnumerator WaitAndSpawnNextFlower(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        float distanceInRealWorld = 20 + 5 * (((tileY % 11) + tileX) % 5);
        Vector2 latlonOfTile = MapGeneratorOSM.instance.TilePosToWorldPos(tileX, tileY);
        int nextTileX = tileX + 1;
        nextTileX = (nextTileX > Mathf.Pow(2, MapGeneratorOSM.instance.zoomLevel)) ? 0 : nextTileX;
        Vector2 latlonOfNextTile = MapGeneratorOSM.instance.TilePosToWorldPos(nextTileX, tileY);
        double realDistanceBetweenTiles = Tools.getDistanceFromLatLonInM(latlonOfTile.x, latlonOfTile.y, latlonOfNextTile.x, latlonOfNextTile.y);
        double distanceInScene = (distanceInRealWorld / realDistanceBetweenTiles) * 5;

        float angle = Mathf.Deg2Rad * ((tileY % 11) * 82 + (tileX % 7) * 12 + Random.Range(-45, 45.0f) );
        Vector3 direction = Mathf.Cos(angle) * Vector3.forward + Mathf.Sin(angle) * Vector3.right;
        Vector3 position = this.transform.position + direction * (float)distanceInScene;
        GameObject nextFlower = Instantiate(this.gameObject, position, Quaternion.Euler(0, ((tileY % 5) * 211 + (tileX % 11) * 8), 0));
        nextFlower.GetComponent<FlowerBehaviour>().tileX = tileX;
        nextFlower.GetComponent<FlowerBehaviour>().tileY = tileY;
        nextFlower.GetComponent<FlowerBehaviour>().remainingFlowers = remainingFlowers - 1;
    }

    private IEnumerator WaitAndDestroyItself(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);
    }
}
