using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KorokRaceTargetBehaviour : MonoBehaviour
{
    public List<Transform> circleTransforms;

    public float totalTime;
    public float remainingTime;

    public int tileX;
    public int tileY;

    public bool succeed;
    public bool moving;

    public AudioSource audioSource;
    public AudioClip startSound;
    public AudioClip failSound;
    public AudioClip loopSound;

    public List<ParticleSystem> particles;

    public KorokStumpBehaviour originStump;

	// Use this for initialization
	void Start ()
    {
        succeed = false;
        moving = true;

        audioSource.Stop();
        audioSource.clip = startSound;
        audioSource.Play();

        StartCoroutine(WaitAndPlayLoopSound(0.5f));
    }

    IEnumerator WaitAndPlayLoopSound(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Stop();
        audioSource.loop = true;
        audioSource.clip = loopSound;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update ()
    {
        if (moving && !succeed)
        {
            remainingTime -= Time.deltaTime;

            try
            {
                this.transform.LookAt(Camera.main.transform);
            }
            catch (System.Exception)
            {

            }

            float minScaleValue = 0.25f;
            float t = ((totalTime - remainingTime) / totalTime) * circleTransforms.Count;
            foreach (Transform circle in circleTransforms)
            {
                if (t >= 1)
                {
                    circle.localScale = Vector3.one * minScaleValue;
                    t -= 1;
                }
                else if (t >= 0)
                {
                    float v = minScaleValue + (1 - t) * (1 - minScaleValue);
                    circle.localScale = v * Vector3.one;
                    break;
                }
                else
                {
                    circle.localScale = Vector3.one;
                }
            }

            if (remainingTime < 0)
            {
                EndTimer();
            }
        }
    }

    private void EndTimer()
    {
        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = failSound;
        audioSource.Play();

        originStump.Failed();

        moving = false;
        succeed = false;
        StartCoroutine(WaitAndDestroyItself(1.0f));
    }

    void OnMouseDown()
    {
        float distanceWithPlayer = Vector3.Distance(this.transform.position, MapPlayerBehaviour.instance.transform.position);
        bool isInReach = distanceWithPlayer <= MapPlayerBehaviour.instance.playerReach;
        if (!succeed && isInReach)
        {
            audioSource.Stop();
            AudioManager.instance.PlayVictory();
            moving = false;
            succeed = true;
            originStump.Solved();
            foreach (ParticleSystem p in particles)
            {
                p.gameObject.SetActive(true);
            }
            StartCoroutine(WaitAndScaleDown());
            StartCoroutine(WaitAndFindKorok(2.0f));
        }
    }

    /*
    void OnTriggerEnter(Collider other)
    {
        if (!succeed && other.tag.Equals("Player"))
        {
            audioSource.Stop();
            AudioManager.instance.PlayVictory();
            moving = false;
            succeed = true;
            originStump.Solved();
            foreach (ParticleSystem p in particles)
            {
                p.gameObject.SetActive(true);
            }
            StartCoroutine(WaitAndScaleDown());
            StartCoroutine(WaitAndFindKorok(2.0f));
        }
    }*/

    IEnumerator WaitAndScaleDown()
    {
        yield return new WaitForEndOfFrame();
        this.transform.localScale = this.transform.localScale * 0.8f;
        if (this.transform.localScale.magnitude > 0)
        {
            StartCoroutine(WaitAndScaleDown());
        }
    }

    IEnumerator WaitAndFindKorok(float delay)
    {
        yield return new WaitForSeconds(delay);
        Controller.instance.FindKorok(this.transform.position, tileX, tileY);
        StartCoroutine(WaitAndDestroyItself(0.0f));
    }

    IEnumerator WaitAndDestroyItself(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);
    }
}
