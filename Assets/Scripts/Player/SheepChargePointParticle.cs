using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepChargePointParticle : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> particles;

    public void ChangeParticleColors(Color newColor)
    {
        for(int i = 0; i < particles.Count; i++)
        {
            var module = particles[i].main;
            module.startColor = newColor;
        }
    }
}
