using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinwheel : MonoBehaviour
{
    [SerializeField] float spinTime = 5f;
    public bool isSpinning;

    public IEnumerator SpinPinwheel()
    {
        isSpinning = true;
        yield return new WaitForSeconds(spinTime);
        isSpinning = false;
    }
}
