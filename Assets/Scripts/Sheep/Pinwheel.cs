using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinwheel : PuzzleKey
{
    [SerializeField] ParticleSystem tempParticles;
    [SerializeField] float spinTime = 5f;
    [SerializeField] string idleAnim;
    [SerializeField] string spinAnim;

    public IEnumerator SpinPinwheel()
    {
        if (!isOpened)
        {
			isOpened = true;
            tempParticles.Play(true);
            GetComponent<Animator>().Play(spinAnim);
            yield return new WaitForSeconds(spinTime);
            tempParticles.Stop(true);
			isOpened = false;
            GetComponent<Animator>().Play(idleAnim);
        }
        else yield return null;
    }

    public bool GetOpened()
    {
        return isOpened;
    }
}
