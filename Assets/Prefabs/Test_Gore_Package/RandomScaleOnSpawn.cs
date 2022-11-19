using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomScaleOnSpawn : MonoBehaviour
{
    [SerializeField] float minSize = 0.25f;
    [SerializeField] float maxSize = 1f;

    //on enable, randomize the size of this object
    private void OnEnable()
    {
        float rand = Random.Range(minSize, maxSize);
        float randRotation = Random.Range(0, 360);

        transform.localScale = new Vector3(rand, rand, rand);
        transform.rotation = Quaternion.Euler(0, randRotation, 0);
    }
}
