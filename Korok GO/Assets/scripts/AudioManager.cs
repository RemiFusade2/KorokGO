using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource source;

    public AudioClip KorokYahaha;
    public AudioClip KorokByeBye;
    public AudioClip SeedWonUI;
    public AudioClip PlicUI;
    public AudioClip Victory;

    public void PlayVictory()
    {
        source.Stop();
        source.loop = false;
        source.clip = Victory;
        source.Play();
    }

    public void PlayKorokYahaha()
    {
        source.Stop();
        source.clip = KorokYahaha;
        source.Play();
    }

    public void PlayKorokByeBye()
    {
        source.Stop();
        source.clip = KorokByeBye;
        source.Play();
    }

    public void PlaySeedWonUI()
    {
        source.Stop();
        source.clip = SeedWonUI;
        source.Play();
    }

    public void PlayPlicUI()
    {
        source.Stop();
        source.clip = PlicUI;
        source.Play();
    }

    void Awake()
    {
        AudioManager.instance = this;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
