using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinwheel : MonoBehaviour
{
    [SerializeField] ParticleSystem tempParticles;
    [SerializeField] float spinTime = 5f;
    public bool isSpinning;

    public IEnumerator SpinPinwheel()
    {
        if (!isSpinning)
        {
            isSpinning = true;
            tempParticles.Play(true);
            yield return new WaitForSeconds(spinTime);
            tempParticles.Stop(true);
            isSpinning = false;
        }
        else yield return null;
    }
}
