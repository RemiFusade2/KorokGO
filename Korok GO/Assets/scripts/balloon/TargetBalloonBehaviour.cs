using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WayPattern
{
    STATIONARY,
    HORIZONTAL_CIRCLE,
    VERTICAL_CIRCLE
}

public class TargetBalloonBehaviour : MonoBehaviour
{
    public Animator animator;

    public GameObject objectToHideWhenPoped;

    public bool poped;

    public WayPattern pattern;
    public bool patternReverse;
    public float patternSpeed;
    public float patternRadius;

    private float patternCurrentAngle;
    private Vector3 patternCenter;

    public AudioSource audioSource;
    public AudioClip spawnSFX;
    public AudioClip flyingSFX;
    public AudioClip popSFX;

    public void SetPattern(WayPattern p, bool rev, float speed, float radius)
    {
        pattern = p;
        patternReverse = rev;
        patternSpeed = speed;
        patternRadius = radius;
        patternCurrentAngle = 0;

        switch (pattern)
        {
            case WayPattern.STATIONARY:
                break;
            case WayPattern.HORIZONTAL_CIRCLE:
                //patternCurrentAngle = Random.Range(-Mathf.PI, Mathf.PI);
                patternCurrentAngle = -Mathf.PI;
                patternCenter = this.transform.position - radius * ( Mathf.Cos(patternCurrentAngle) * Vector3.right + Mathf.Sin(patternCurrentAngle) * Vector3.forward);
                break;
            case WayPattern.VERTICAL_CIRCLE:
                patternCurrentAngle = -Mathf.PI;
                patternCenter = this.transform.position + Vector3.up * radius;
                break;
        }
    }

    void OnMouseDown()
    {
        if (!poped)
        {
            PopTarget();
        }
    }

    private void PopTarget()
    {
        poped = true;
        animator.SetTrigger("Pop");
        
        audioSource.Stop();
        audioSource.clip = popSFX;
        audioSource.loop = false;
        audioSource.Play();
    }

	// Use this for initialization
	void Start ()
    {
        audioSource.Stop();
        audioSource.clip = spawnSFX;
        audioSource.Play();
        StartCoroutine(WaitAndPlayFlyingLoopSFX(1.0f));
    }

    IEnumerator WaitAndPlayFlyingLoopSFX(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Stop();
        audioSource.clip = flyingSFX;
        audioSource.loop = true;
        audioSource.Play();
    }
	
	// Update is called once per frame
	void Update ()
    {
        try
        {
            this.transform.LookAt(Camera.main.transform, Vector3.up);
        }
        catch (System.Exception )
        {

        }
        patternCurrentAngle += Time.deltaTime * patternSpeed / Mathf.PI;

        switch (pattern)
        {
            case WayPattern.STATIONARY:
                break;
            case WayPattern.HORIZONTAL_CIRCLE:
                this.transform.position = patternCenter + patternRadius * ((patternReverse ? -1 : 1) * Mathf.Cos(patternCurrentAngle) * Vector3.right + Mathf.Sin(patternCurrentAngle) * Vector3.forward);
                break;
            case WayPattern.VERTICAL_CIRCLE:
                this.transform.position = patternCenter + patternRadius * ((patternReverse ? -1 : 1) * Mathf.Cos(patternCurrentAngle) * Vector3.right + Mathf.Sin(patternCurrentAngle) * Vector3.up);
                break;
        }
	}

    public void UnSpawn()
    {
        animator.SetTrigger("Pop");
        StartCoroutine(WaitAndDestroyItself(1.5f));
    }

    IEnumerator WaitAndDestroyItself(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);
    }
}
