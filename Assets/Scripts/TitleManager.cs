using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviourPunCallbacks
{
    public static bool isMulti = false;
    public static AudioSource titleAudioSource;

    public AudioClip swipeSound;
    public AudioClip doubleTapSound;
    public AudioClip standbySound;

    public static AudioSource titleAudioSource1;

    public AudioClip titleVoice;
    public AudioClip soloPlayVoice;
    public AudioClip multiPlayVoice;

    private float fingerPosY0;
    private float fingerPosY1;
    public float posThY;

    private int tapCount;
    private bool isDoubleTap;
    public float doubleTapTh = 0.5f;

    public GameObject soloPlayOn;
    public GameObject soloPlayOff;
    public GameObject multiPlayOn;
    public GameObject multiPlayOff;

    public GameObject popupCanvas;

    private void Awake() {
        titleAudioSource = GameObject.FindWithTag("TitleAudioSource").GetComponent<AudioSource>();
        titleAudioSource1 = GameObject.FindWithTag("TitleAudioSource1").GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeMode();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            fingerPosY0 = Input.mousePosition.y;
        }
        if (Input.GetMouseButtonUp(0))
        {
            fingerPosY1 = Input.mousePosition.y;
            float diffY = fingerPosY1 - fingerPosY0;
            if (Mathf.Abs(diffY) > posThY)  // Swipe vertically
            {
                SwipeVertically();
                return;
            }
            else    // Tap or Double Tap
            {
                tapCount++;
                Invoke(nameof(Tap), doubleTapTh);
            }
        }
    }

    private void SwipeVertically()
    {
        titleAudioSource.PlayOneShot(swipeSound);
        isMulti = !isMulti;

        ChangeMode();
    }

    private void ChangeMode()
    {
        if (isMulti)
        {
            titleAudioSource1.PlayOneShot(multiPlayVoice);
            soloPlayOn.SetActive(false);
            soloPlayOff.SetActive(true);
            multiPlayOn.SetActive(true);
            multiPlayOff.SetActive(false);
        }
        else
        {
            titleAudioSource1.PlayOneShot(soloPlayVoice);
            soloPlayOn.SetActive(true);
            soloPlayOff.SetActive(false);
            multiPlayOn.SetActive(false);
            multiPlayOff.SetActive(true);
        }
    }

    private void Tap()
    {
        if (tapCount < 2)
        {
            if (!isDoubleTap)   // Single Tap
            {

            }
            isDoubleTap = false;
        }
        else    // Double Tap
        {
            isDoubleTap = true;

            titleAudioSource.PlayOneShot(doubleTapSound);

            // ページ遷移
            SceneTransition();
        }
        tapCount = 0;
    }

    private void SceneTransition()
    {
        if (isMulti)    // マルチプレイ
        {
            SceneManager.LoadScene("MultiPlayMenu");
        }
        else    //　ソロプレイ
        {
            popupCanvas.SetActive(true);
            titleAudioSource.clip = standbySound;
            titleAudioSource.Play();
            PhotonNetwork.OfflineMode = true;
        }
    }

    // ソロプレイ向け
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room0", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        popupCanvas.SetActive(false);
        TitleManager.titleAudioSource.Stop();
        TitleManager.titleAudioSource.PlayOneShot(doubleTapSound);
        SceneManager.LoadScene("Tool");
    }
}
