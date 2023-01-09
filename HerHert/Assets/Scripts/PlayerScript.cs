using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Cinemachine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D RB;
    public Animator AN;
    public SpriteRenderer SR;
    public PhotonView PV;
    public Text NickNameText;
    public Image HealthImage;
    public Image BulletImage;
    public Image WaterLimit;
    public float Bullettime = 0.01f;
    public float Playerskin = 0;
    public int Up = 0;
    public float Bullet = 0.3f;

    bool isGround;  //점프상태
    bool isWater;
    bool isBug;
    bool isLava;
    Vector3 curPos; //캐릭터 동기화

    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;
    AudioSource audioSource;



    void Awake()
    {
        
        // 닉네임
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;
        audioSource = GetComponent<AudioSource>();

        if (PV.IsMine)
        {

            // 2D 카메라
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;

            NetworkManager manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            Playerskin = manager.skin;
            AN.SetInteger("skin", (int)Playerskin);
            manager.skin = (int)Playerskin;
        }
    }


    void Update()
    {

        if (PV.IsMine)
        {
            NetworkManager manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            Playerskin = manager.skin;
            AN.SetInteger("skin", (int)Playerskin);
            manager.skin = (int)Playerskin;



            BulletImage.fillAmount += 0.5f*Time.deltaTime;
            // ← → 이동
            float axis = Input.GetAxisRaw("Horizontal");
            RB.velocity = new Vector2(4 * axis, RB.velocity.y); 

            if (axis != 0)//보는방향
            {
                AN.SetBool("walk", true);
                PV.RPC("FlipXRPC", RpcTarget.AllBuffered, axis); // 재접속시 filpX를 동기화해주기 위해서 AllBuffered
            }
            else AN.SetBool("walk", false);


            // space 점프, 바닥체크
            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Ground"));//동그라미를 만들어서 바닥 체크,레이아웃 만들어야댐, 0.5밑에 0.07반지름 원만듦
            AN.SetBool("jump", !isGround);  //땅에있으면 false
            if (Input.GetKeyDown(KeyCode.Space) && isGround)
            {
                PV.RPC("JumpRPC", RpcTarget.All);//바닥에있고 점프키누르면 점프
                audioSource.clip = audio1;
                audioSource.Play();
            }

            isWater = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.25f), 0.07f, 1 << LayerMask.NameToLayer("Water"));//물 체크
            if (isWater)
            {
                RB.velocity = RB.velocity * 0.5f;
                transform.Translate(new Vector2(Input.GetAxisRaw("Horizontal")*Time.deltaTime * 5, Input.GetAxisRaw("Vertical") * Time.deltaTime * 5));
                if(manager.map == 1)
                {
                    WaterLimit.fillAmount += 0.1f * Time.deltaTime;
                    if (WaterLimit.fillAmount == 1f)
                    {
                        HealthImage.fillAmount -= 0.05f* Time.deltaTime;
                    }
                }
            }
            else
            {
                WaterLimit.fillAmount -= 1f * Time.deltaTime;
            }

            isBug = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Bug"));//버그 체크
            if (isBug)
            {
                PV.RPC("BugRPC", RpcTarget.All);
            }

            isLava = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.5f), 0.07f, 1 << LayerMask.NameToLayer("Lava"));//용암 체크
            if (isLava)
            {
                PV.RPC("LavaRPC", RpcTarget.All);
            }


            float UpDown = Input.GetAxisRaw("Vertical");    //총알발사
            if (Input.GetKeyDown(KeyCode.Z) && (BulletImage.fillAmount >= Bullet))
            {
                PV.RPC("BulletRPC", RpcTarget.All);
                PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(SR.flipX ? -0.5f : 0.5f, 0, 0), Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, SR.flipX ? -1 : 1, (int)Playerskin, Up = (int)UpDown*90);
                AN.SetTrigger("shot"); //총알 발사 
                audioSource.clip = audio2;
                audioSource.Play();
            }
        }
        // IsMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos; 
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }

    [PunRPC]
    void BulletRPC()
    {
        BulletImage.fillAmount -= Bullet;
    }

    [PunRPC]
    void FlipXRPC(float axis) => SR.flipX = axis == -1;//움직이는 방향

    [PunRPC]    //점프
    void JumpRPC()
    {
        RB.velocity = Vector2.zero;
        RB.AddForce(Vector2.up * 700);
    }

    [PunRPC]//물
    void WaterRPC()
    {
        float axis1 = Input.GetAxisRaw("Vertical");
        RB.velocity = new Vector2(4 * axis1, RB.velocity.y); 
    }

    [PunRPC]//버그방지
    void BugRPC()
    {
        RB.transform.Translate(new Vector2(0, 1));
    }

    [PunRPC]//용암방지
    void LavaRPC()
    {
        HealthImage.fillAmount -= 1f*Time.deltaTime;
        if (HealthImage.fillAmount <= 0 && PV.IsMine)
        {
            NetworkManager manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            if (manager.map == 2)
            {
                Dragon Dragon = GameObject.FindWithTag("Dragon").GetComponent<Dragon>();
                if (Dragon.text1 > 0)
                {
                    Dragon.text1++;
                }
            }
            manager.death += 1;
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);    //캔버스에서 리스폰패널을 활성화시킴
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered); // Destrot는 AllBuffered로 해야 제대로 사라져 복제버그가 안 생긴다, 죽으면 다 사라지게함
        }
    }

    public void Hit()   //총알에서 호출함
    {
        HealthImage.fillAmount -= 0.1f; //총알맞으면 피깎임
        AN.SetTrigger("x");
        audioSource.clip = audio3;
        audioSource.Play();
        if (HealthImage.fillAmount <= 0)
        {
            NetworkManager manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
            if (manager.map == 2)
            {
                DragonGo();
            }
            audioSource.Play();
            manager.death += 1;
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);    //캔버스에서 리스폰패널을 활성화시킴
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered); // Destrot는 AllBuffered로 해야 제대로 사라져 복제버그가 안 생긴다, 죽으면 다 사라지게함
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);   //파괴

    public void DragonGo()
    {
        Dragon Dragon1 = GameObject.FindWithTag("Dragon").GetComponent<Dragon>();
        Dragon1.text1 = 4;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)   //위치, 체력 변수 동기화
        {
            stream.SendNext(transform.position);
            stream.SendNext(HealthImage.fillAmount);
            stream.SendNext(BulletImage.fillAmount);
            stream.SendNext(Playerskin);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
            BulletImage.fillAmount = (float)stream.ReceiveNext();
            Playerskin = (float)stream.ReceiveNext();
        }
    }
}
