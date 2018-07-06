using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KorokRaceStartBlastBehaviour : MonoBehaviour
{
    public Vector3 destination;
    
    public float speed;

    public GameObject raceGoalPrefab;

    private bool moving;

    public ParticleSystem particles;

    private float timerValue;
    private int tileX;
    private int tileY;

    public AudioSource audioSource;
    public AudioClip ballSpawnSound;
    public AudioClip ballDisappearSound;

    public KorokStumpBehaviour originStump;

    public void SetInfo(int x, int y, float timer, Vector3 dest, KorokStumpBehaviour stump)
    {
        originStump = stump;
        tileX = x;
        tileY = y;
        timerValue = timer;
        destination = dest;
    }

    // Use this for initialization
    void Start ()
    {
        moving = true;

        audioSource.Stop();
        audioSource.clip = ballSpawnSound;
        audioSource.Play();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (moving)
        {
            Vector3 direction = (destination - this.transform.position).normalized;
            float distance = Vector3.Distance(destination, this.transform.position);

            if (distance < (speed * Time.deltaTime))
            {
                this.transform.position = destination;
                moving = false;
                particles.Stop();
                GameObject goalGo = Instantiate(raceGoalPrefab, this.transform.position + 2*Vector3.up, Quaternion.identity);
                goalGo.GetComponent<KorokRaceTargetBehaviour>().tileX = tileX;
                goalGo.GetComponent<KorokRaceTargetBehaviour>().tileY = tileY;
                goalGo.GetComponent<KorokRaceTargetBehaviour>().totalTime = timerValue;
                goalGo.GetComponent<KorokRaceTargetBehaviour>().remainingTime = timerValue;
                goalGo.GetComponent<KorokRaceTargetBehaviour>().originStump = originStump;

                audioSource.Stop();
                audioSource.clip = ballDisappearSound;
                audioSource.Play();

                StartCoroutine(WaitAndDestroyItself(2.0f));
            }
            else
            {
                this.transform.position += direction * speed * Time.deltaTime;
            }
        }

    }

    IEnumerator WaitAndDestroyItself(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);
    }
}
