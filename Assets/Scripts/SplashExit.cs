using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashExit : MonoBehaviour
{
	[SerializeField] float timer = 3;
    // Start is called before the first frame update
    void Start()
    {
		Invoke("Next", timer);
    }

    // Update is called once per frame
    void Next()
    {
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
