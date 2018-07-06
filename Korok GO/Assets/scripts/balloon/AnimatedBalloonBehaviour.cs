using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedBalloonBehaviour : MonoBehaviour
{

    public List<ParticleSystem> particles;

    public void SendParticles()
    {
        foreach (ParticleSystem p in particles)
        {
            p.Stop();
            p.Play();
        }
    }
}
