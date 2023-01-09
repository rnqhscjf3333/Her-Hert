using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BulletScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public Animator AN;
    public float wldus = 5f;
    int dir;    //방향
    int fire = 0;
    public int killed = 0;
    public int bulletskin = 0;
    public int BulletUp = 0;




    void Start()
    {
        if (PV.IsMine)
        {
        }
        transform.localScale = new Vector3(dir, 1, 1);
        transform.eulerAngles = new Vector3(0, 0, BulletUp);
        Destroy(gameObject, 5f);  //3.5초 뒤에 파괴
    }

    void Update()
    {

        if (fire == 0)
        {
            transform.Translate(Vector3.right * 10 * Time.deltaTime * dir);//속도
        }
        if (PV.IsMine && killed == 1)
        {
            NetworkManager manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            manager.kill += 1;
            killed = 0;
        }
    }


    void OnTriggerEnter2D(Collider2D col) // col을 RPC의 매개변수로 넘겨줄 수 없다
    {

        if (col.tag == "Ground")
        {
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);//그라운드에 충돌하면 파괴
        }

        if (col.tag == "Dragon" && col.GetComponent<PhotonView>().IsMine)
        {
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);//그라운드에 충돌하면 파괴
            col.GetComponent<Dragon>().Hit(); //부딫힌사람은 Hit함수
        }


        if (!PV.IsMine && col.tag == "Player" && col.GetComponent<PhotonView>().IsMine) // 느린쪽에 맞춰서 Hit판정, 자기총알 아니고 플레이어에 맞고, 
        {
            if (col.GetComponent<PlayerScript>().HealthImage.fillAmount <= 0.1)
            {
                PV.RPC("killing", RpcTarget.AllBuffered);
            }
            col.GetComponent<PlayerScript>().Hit(); //부딫힌사람은 Hit함수
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);//그라운드에 충돌하면 파괴
        }

    }


    [PunRPC]
    void DirRPC(int dir, int Playerskin, int Up)
    {
        this.dir = dir; //총알 방향 플레이어에서 받음
        this.bulletskin = Playerskin;
        AN.SetInteger("skin", bulletskin);
        this.BulletUp = dir*Up;
    }


    [PunRPC]
    void DestroyRPC()
    {
        AN.SetTrigger("shoot");
        Destroy(gameObject, 0.5f);//파괴
        fire = 1;
    }
    [PunRPC]
    void killing()
    {
        killed = 1;
    }
}
