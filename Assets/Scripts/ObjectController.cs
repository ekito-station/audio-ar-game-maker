using System.Collections;
using UnityEngine;
using Photon.Pun;

public abstract class ObjectController : MonoBehaviourPunCallbacks
{
    // 設定パラメータ
    [System.NonSerialized] public int soundRinging = 1;
    [System.NonSerialized] public int soundOnCollision = 1;
    [System.NonSerialized] public int howToMove;
    [System.NonSerialized] public bool shouldDisappear;

    // 動き方
    [System.NonSerialized] public Vector3 dir;
    public float walkSpeed = 0.003f;
    public float moveTime = 2.0f;
    private Coroutine moveRandomly;

    public AudioSource audioSource;
    [System.NonSerialized] public bool isPlayed;

    // Start is called before the first frame update
    public void Start()
    {
        audioSource.Play();
    }

    // Update is called once per frame
    public void Update()
    {
        // 動かす
        if (howToMove != 0)
        {
            transform.Translate(dir.x, dir.y, dir.z, Space.World);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Wall")
        {
            // 壁で反射させる
            Vector3 normal = other.gameObject.transform.forward;
            dir = Vector3.Reflect(dir, normal);
        }
    }

    public void ConfigureObj(bool _shouldDisappear, int _soundRinging, int _soundOnCollision, int _howToMove, Vector3 _forward)
    {
        photonView.RPC(nameof(ConfigureObjRPC), RpcTarget.AllBuffered, _shouldDisappear, _soundRinging, _soundOnCollision);

        // 動き方
        Debug.Log("setting for moving");
        howToMove = _howToMove;
        Debug.Log("howToMove: " + howToMove);
        if (howToMove == 1) // 前に進める
        {
            // dir = new Vector3(0.0f, _forward.y, 0.0f);
            dir = _forward;
            dir *= walkSpeed;
        }
        else if (howToMove == 2)    // ランダムに進める
        {
            moveRandomly = StartCoroutine(MoveRandomly());
        }
    }

    [PunRPC]
    public void ConfigureObjRPC(bool _shouldDisappear, int _soundRinging, int _soundOnCollision)
    {
        // 当たった時に消えるかどうか
        shouldDisappear = _shouldDisappear;

        // 常に鳴る音
        ToolManager toolManager = GameObject.FindWithTag("ToolManager").GetComponent<ToolManager>();
        switch (_soundRinging)
        {
            case 1:
                audioSource.clip = toolManager.soundRinging1;
                break;
            case 2:
                audioSource.clip = toolManager.soundRinging2;
                break;
            case 3:
                audioSource.clip = toolManager.soundRinging3;
                break;
            case 4:
                audioSource.clip = toolManager.soundRinging4;
                break;
            case 5:
                audioSource.clip = toolManager.soundRinging5;
                break;
            default:
                break;
        }
        audioSource.loop = true;
        audioSource.Play();
        Debug.Log("Playing sound: " + _soundRinging);

        // 当たった時に鳴る音
        soundOnCollision = _soundOnCollision;
    }

    public void MakeSoundOnCollision()
    {
        photonView.RPC(nameof(MakeSoundOnCollisionRPC), RpcTarget.AllBuffered);
    }

    [PunRPC]
    public void MakeSoundOnCollisionRPC()
    {
        ToolManager toolManager = GameObject.FindWithTag("ToolManager").GetComponent<ToolManager>();

        switch (soundOnCollision)
        {
            case 1:
                audioSource.PlayOneShot(toolManager.soundOnCollision1);
                break;
            case 2:
                audioSource.PlayOneShot(toolManager.soundOnCollision2);
                break;
            case 3:
                audioSource.PlayOneShot(toolManager.soundOnCollision3);
                break;
            case 4:
                audioSource.PlayOneShot(toolManager.soundOnCollision4);
                break;
            case 5:
                audioSource.PlayOneShot(toolManager.soundOnCollision5);
                break;
            default:
                break;
        }
    }

    public void ChangeVolume(float distance)
    {
        photonView.RPC(nameof(ChangeVolumeRPC), RpcTarget.AllBuffered, distance);
    }

    [PunRPC]
    public void ChangeVolumeRPC(float distance)
    {
        audioSource.volume = 0.6f + distance;
    }

    private IEnumerator MoveRandomly()
    {
        while (true)
        {
            // 向きを決める
            int randNum = Random.Range(0, 8);
            switch (randNum)
            {
                case 0:
                    dir = new Vector3(1.0f, 0.0f, 0.0f);
                    break;
                case 1:
                    dir = new Vector3(0.7f, 0.0f, -0.7f);
                    break;
                case 2:
                    dir = new Vector3(0.0f, 0.0f, -1.0f);
                    break;
                case 3:
                    dir = new Vector3(-0.7f, 0.0f, -0.7f);
                    break;
                case 4:
                    dir = new Vector3(-1.0f, 0.0f, 0.0f);
                    break;
                case 5:
                    dir = new Vector3(-0.7f, 0.0f, 0.7f);
                    break;
                case 6:
                    dir = new Vector3(0.0f, 0.0f, 1.0f);
                    break;
                case 7:
                    dir = new Vector3(0.7f, 0.0f, 0.7f);
                    break;
                default:
                    break;
            }
            dir *= walkSpeed;

            yield return new WaitForSeconds(moveTime);
        }
    }
}
