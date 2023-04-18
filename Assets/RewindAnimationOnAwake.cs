using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindAnimationOnAwake : MonoBehaviour
{
	Animation credits;
	
	private void OnEnable()
	{
		if (credits == null)
			credits = GetComponent<Animation>();
		credits.Rewind();
		credits.Play();
	}
}
