using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Lobby : MonoBehaviourPunCallbacks
{
    public static Lobby instance = null;
    public int map = -1;
    public int skin = 0;
    public string Name = "";

    void Awake()
    {
        var input = gameObject.GetComponent<InputField>();

        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (map >= 0)
        {
            NetworkManager manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            manager.map = map;
            manager.skin = skin;
        }

        var input = gameObject.GetComponent<InputField>();

    }
    public void map0()
    {
        map = 0;
        SceneManager.LoadScene("Map0");
    }
    public void map1()
    {
        Lobby lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        map = 1;
        SceneManager.LoadScene("Map1");
    }
    public void map2()
    {
         map = 2;
        SceneManager.LoadScene("Map2");
    }
}