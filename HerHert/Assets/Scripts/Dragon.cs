using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Dragon : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView PV;
    public int Pattern = 0;
    public float timer;
    public float timer1;
    public int Idle = 1;
    public int Idle1 = 1;
    public int waitingTime;
    public Animator AN;
    public Image HealthImage;
    public Image HealthImage2;
    int Ball = 90;
    public int Lava1 = 0;
    public Text HP;
    public int text1 = 3;
    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;
    public AudioClip audio4;
    AudioSource audioSource;


    void Awake()
    {
        NetworkManager manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        if (manager.Dragon == 1)
        {
            Destroy(gameObject);
        }
        HealthImage = GameObject.Find("DragonHealth").GetComponent<Image>();
        HealthImage2 = GameObject.Find("DragonHealth2").GetComponent<Image>();
        HP = GameObject.Find("DragonHP").GetComponent<Text>();
        audioSource = GetComponent<AudioSource>();
        timer = 0.0f;
        timer1 = 0.0f;
        waitingTime = 5;
        HealthImage.fillAmount = 1f;
        HealthImage2.fillAmount = 1f;
        HP.text = text1.ToString();

        audioSource.clip = audio2;
        audioSource.Play();
    }

    void Update()
    {
        NetworkManager manager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
        manager.Dragon = 1;

        InvokeRepeating("randomRPC", 3f, 1f);

        HP.text = text1.ToString();
        if (text1 > 3)
        {
            text1 = 3;
            HealthImage.fillAmount = 1f;
            HealthImage2.fillAmount = 1f;
        }

        if (Idle>0 && Idle1 > 0) timer += Time.deltaTime;

        AN.SetInteger("DragonPattern", 0);

        if (timer > waitingTime)
        {
            if (Pattern == 1)
            {
                Attack1RPC();
                timer = 0;
                Idle = 0;
            }
            else if(Pattern == 2)
            {
                Attack2RPC();
                timer = 0;
                Idle = 0;
            }
            else
                timer = 0;
        }
        if(timer1 > 3)
        {
            CancelInvoke("Pattern1");
            CancelInvoke("Pattern2");
            Idle = 1;
        }
    }
    void randomRPC()
    {
        Pattern++;
        if (Pattern > 2)
        {
            Pattern = 0;
        }
    }

    void Attack1RPC()
    {
        AN.SetInteger("DragonPattern", 1);
        InvokeRepeating("Pattern1",1f,0.3f);
        timer1 = 0;
        audioSource.clip = audio2;
        audioSource.Play();
    }
    void Attack2RPC()
    {
        AN.SetInteger("DragonPattern", 2);
        InvokeRepeating("Pattern2", 1f, 0.3f);
        timer1 = 0;
        audioSource.clip = audio3;
        audioSource.Play();
    }
    void Attack3()
    {
        AN.SetInteger("DragonPattern", 3);
        timer1 = 0;
    }

    void Pattern1()
    {
        for (int i = 0; i < 10; i++)
        {
            Ball = Random.Range(180, 360);
            PhotonNetwork.Instantiate("Ball", new Vector3(17.7f, 81.8f, 0), Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, 2, 0, Ball);
        }
        timer1 += 1;
    }
    void Pattern2()
    {
        for (int i = 0; i < 10; i++)
        {
            float randomX = Random.Range(6f, 30f); //적이 나타날 X좌표를 랜덤으로 생성해 줍니다.
            Ball = Random.Range(260, 280);
            PhotonNetwork.Instantiate("Ball", new Vector3(randomX, 90, 0), Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, 1, 1, Ball);
        }
        timer1 += 0.5f;
    }

    void Pattern3()
    {
        audioSource.clip = audio1;
        audioSource.Play();
        for (int i = 0; i < 2; i++)
        {
            Ball = Random.Range(-90, 90);
            PhotonNetwork.Instantiate("Ball", new Vector3(6f, 80f, 0), Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, 1, 0, Ball);
            PhotonNetwork.Instantiate("Ball", new Vector3(29f, 80f, 0), Quaternion.identity).GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, 1, 0, Ball+180);
        }
        timer1 += 1;

        PV.RPC("UpRPC", RpcTarget.AllBuffered);

    }
    void Pattern3_1()
    {
        CancelInvoke("Pattern3");
        Lava1 = 0;
        Idle = 1;
        Idle1 = 1;
        timer = 0;
        AN.SetBool("Trig", false);
        AN.SetBool("Trig2", true);
        InvokeRepeating("Pattern3_2", 1f, 0.3f);
        Invoke("Pattern3_3", 13f);
        GameObject.Find("DragonHealth").GetComponent<Image>().color = new Color(255, 0, 0);
        GameObject.Find("DragonHealth2").GetComponent<Image>().color = new Color(255, 0, 0);
        if (text1 <= 0)
        {
            PV.RPC("DieRPC", RpcTarget.AllBuffered);
        }
    }
    void Pattern3_2()
    {
        PV.RPC("Down1RPC", RpcTarget.AllBuffered);
    }
    void Pattern3_3()
    {
        CancelInvoke("Pattern3_2");
        AN.SetBool("Trig2", false);
    }

    public void Hit()
    {
        if (Lava1 == 0)
        {
            HealthImage.fillAmount -= 0.02f;
            HealthImage2.fillAmount -= 0.02f;
        }
        if (HealthImage.fillAmount <= 0 && Idle ==1 && text1>=0)
        {
            HealthImage.fillAmount = 1f;
            HealthImage2.fillAmount = 1f;
            text1 -= 1;
            AN.SetBool("Trig", true);
            audioSource.clip = audio1;
            audioSource.Play();

            Lava1 = 1;
            HealthImage.color = new Color(0, 0, 255);
            HealthImage2.color = new Color(0, 0,255);
            InvokeRepeating("Pattern3", 1f, 0.3f);
            Invoke("Pattern3_1", 10f);
            
            timer = 0;
            Idle1 = 0;
        }
    }
    [PunRPC]
    void UpRPC()
    {
        GameObject Lava = GameObject.Find("Lava");
        if(Lava.transform.position.y<74)
        {
            Lava.transform.Translate(Vector3.up * 3f * Time.deltaTime);
        }
    }
    [PunRPC]
    void Down1RPC()
    {
        GameObject Lava = GameObject.Find("Lava");
        if (Lava.transform.position.y > 71.5)
        {
            Lava.transform.Translate(Vector3.up * -3f * Time.deltaTime);
        }
    }
    [PunRPC]
    void DieRPC()
    {
        AN.SetTrigger("die");
        timer = 0;
        Idle1 = 0;
        Idle = 0;
        this.GetComponent<CapsuleCollider2D>().enabled = false;
        HealthImage.fillAmount = 0f;
        HealthImage2.fillAmount = 0f;
        audioSource.clip = audio4;
        audioSource.Play();
    }
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)   //변수 동기화
        {
            stream.SendNext(HealthImage.fillAmount);
            stream.SendNext(HealthImage2.fillAmount);
            stream.SendNext(text1);
            stream.SendNext(timer);
            stream.SendNext(timer1);
            stream.SendNext(Idle);
            stream.SendNext(Idle1);
            stream.SendNext(Pattern);
        }
        else
        {
            HealthImage.fillAmount = (float)stream.ReceiveNext();
            HealthImage2.fillAmount = (float)stream.ReceiveNext();
            text1 = (int)stream.ReceiveNext();
            timer = (float)stream.ReceiveNext();
            timer1 = (float)stream.ReceiveNext();
            Idle = (int)stream.ReceiveNext();
            Idle1 = (int)stream.ReceiveNext();
            Pattern = (int)stream.ReceiveNext();
        }
    }
}
