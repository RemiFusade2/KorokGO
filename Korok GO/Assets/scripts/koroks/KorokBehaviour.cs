using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum KorokType
{
    KOROK_MOVING,
    KOROK_UNDERROCK,
    KOROK_CIRCLEROCKS,
    KOROK_FLOWERRACE,
    KOROK_TARGETS,
    KOROK_TIMERACE
}

public class KorokBehaviour : MonoBehaviour
{
    public int xID;
    public int yID;

    public Animator animator;
    public GameObject korokGo;

    public bool isFound;

    public KorokType type;


    public static string GetHashCodeForIDs(int xID, int yID)
    {
        return "KorokGO_" + xID.ToString() + "_" + yID.ToString();
    }

    private void TapOnKorok()
    {
        float distance = Vector3.Distance(this.transform.position, Controller.instance.player.position);
        if (distance < 6 && !isFound)
        {
            switch (type)
            {
                case KorokType.KOROK_MOVING:
                    FindKorok();
                    break;
                case KorokType.KOROK_UNDERROCK:
                    FindKorok();
                    break;
                case KorokType.KOROK_CIRCLEROCKS:
                    bool rockInPossession = false;
                    if (rockInPossession)
                    {
                        FindKorok();
                    }
                    break;
                case KorokType.KOROK_FLOWERRACE:
                case KorokType.KOROK_TARGETS:
                    StartTargetsChallenge();
                    break;
                case KorokType.KOROK_TIMERACE:
                    StartTimeRaceChallenge();
                    break;
            }
        }
    }

    private void StartTimeRaceChallenge()
    {
        // not implemented
    }

    private void StartTargetsChallenge()
    {
        // not implemented
    }

    private void FindKorok()
    {
        Controller.instance.FindKorok(this.transform.position, xID, yID);
        Destroy(korokGo);
    }

	// Use this for initialization
	void Start ()
    {
        //isFound = false;
    }

    void Update()
    {
        if (isFound)
        {
            try
            {
                this.transform.LookAt(Camera.main.transform, Vector3.up);
            }
            catch (System.Exception)
            {

            }
        }
        else
        {
            RaycastHit hit = new RaycastHit();
            for (int i = 0; i < Input.touchCount; ++i)
            {
                if (Input.GetTouch(i).phase.Equals(TouchPhase.Began))
                {
                    // Construct a ray from the current touch coordinates
                    Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
                    if (Physics.Raycast(ray, out hit))
                    {
                        TapOnKorok();
                    }
                }
            }
        }
    }

    void OnMouseDown()
    {
        float distanceWithPlayer = Vector3.Distance(this.transform.position, MapPlayerBehaviour.instance.transform.position);
        bool isInReach = distanceWithPlayer <= MapPlayerBehaviour.instance.playerReach;
        if (!isFound && isInReach)
        {
            TapOnKorok();
        }
    }

    public void SetAnimation()
    {
        animator.SetInteger("animation", Random.Range(0, 3));
    }
}
