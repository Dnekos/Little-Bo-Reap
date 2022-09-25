using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    Transform player;

    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }

    }
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void SetPlayer(Transform thePlayer)
    {
        player = thePlayer;
    }
    public Transform GetPlayer()
    {
        return player;
    }
}
