using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public InputField NickNameInput;
    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;

    public int death = 0;
    public int kill = 0;
    public int skin = 0;
    public Text deathCountText; //데스
    public Text killCountText; //킬
    public int map = 0;
    public Animator character;
    public int Dragon = 0;

    void Awake()
    {
        Lobby lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        NickNameInput.text = lobby.Name;
        Screen.SetResolution(960, 540, false); //가로세로
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }
    
    public void Connect() => PhotonNetwork.ConnectUsingSettings();

    public void Character0()
    {
        skin = 0;
        Lobby lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        lobby.skin = 0;
    }
    public void Character1()
    {
        skin = 1;
        Lobby lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        lobby.skin = 1;
    }
    public void Character2()
    {
        skin = 2;
        Lobby lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        lobby.skin = 2;
    }
    public void Character3()
    {
        skin = 3;
        Lobby lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        lobby.skin = 3;
    }
    public void Character4()
    {
        skin = 4;
        Lobby lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        lobby.skin = 4;
    }

    public void map0()
    {
        Lobby lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        lobby.map = 0;
        lobby.Name = NickNameInput.text;
        SceneManager.LoadScene("Map0");
    }
    public void map1()
    {
        Lobby lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        lobby.map = 1;
        lobby.Name = NickNameInput.text;
        SceneManager.LoadScene("Map1");
    }
    public void map2()
    {
        Lobby lobby = GameObject.Find("Lobby").GetComponent<Lobby>();
        lobby.map = 2;
        lobby.Name = NickNameInput.text;
        SceneManager.LoadScene("Map2");
    }



    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 20 }, null);
    }

    public override void OnJoinedRoom()
    {
        DisconnectPanel.SetActive(false);
        StartCoroutine("DestroyBullet");
        Spawn();
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag("Bullet")) GO.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
    }

    public void Spawn()
    {
        if(map == 0)
        {
            PhotonNetwork.Instantiate("Player", new Vector2(Random.Range(-6f, 40f), 1), Quaternion.identity);
        }
        if(map == 1)
        {
            PhotonNetwork.Instantiate("Player", new Vector2(Random.Range(6f, 30f), 36), Quaternion.identity);
        }
        if (map == 2)
        {
            PhotonNetwork.Instantiate("Player", new Vector2(Random.Range(14f, 23f), 75), Quaternion.identity);
            Invoke("Dragon3", 3f);

        }
        RespawnPanel.SetActive(false);
    }

    void Update()
    {
        character.SetInteger("skin", skin);
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        deathCountText.text = "데스 : " + death;
        killCountText.text = "킬 : " + kill;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        RespawnPanel.SetActive(false);
    }

    public void Dragon3()
    {
        PhotonNetwork.Instantiate("Dragon", new Vector3(17.6f, 72.22f, 91.89999f), Quaternion.Euler(0, -180.0f, 0));
    }

}
