using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinwheel : MonoBehaviour
{
    [SerializeField] ParticleSystem tempParticles;
    [SerializeField] float spinTime = 5f;
    [SerializeField] string idleAnim;
    [SerializeField] string spinAnim;
    public bool isSpinning;

    public IEnumerator SpinPinwheel()
    {
        if (!isSpinning)
        {
            isSpinning = true;
            tempParticles.Play(true);
            GetComponent<Animator>().Play(spinAnim);
            yield return new WaitForSeconds(spinTime);
            tempParticles.Stop(true);
            isSpinning = false;
            GetComponent<Animator>().Play(idleAnim);
        }
        else yield return null;
    }
}
