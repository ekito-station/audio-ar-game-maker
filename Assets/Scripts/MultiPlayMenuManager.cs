using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MultiPlayMenuManager : MonoBehaviourPunCallbacks
{
    public AudioClip swipeSound;
    public AudioClip doubleTapSound;
    public AudioClip standbySound;

    public AudioClip room1Voice;
    public AudioClip room2Voice;
    public AudioClip room3Voice;

    private float fingerPosX0;
    private float fingerPosX1;
    public float posThX;
    private float fingerPosY0;
    private float fingerPosY1;
    public float posThY;

    private int tapCount;
    private bool isDoubleTap;
    public float doubleTapTh = 0.5f;

    private int roomNum = 1;

    public GameObject room1On;
    public GameObject room1Off;
    public GameObject room2On;
    public GameObject room2Off;
    public GameObject room3On;
    public GameObject room3Off;

    public GameObject popupCanvas;

    // Start is called before the first frame update
    void Start()
    {
        ChangeRoom();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            fingerPosY0 = Input.mousePosition.y;
            fingerPosX0 = Input.mousePosition.x;
        }
        if (Input.GetMouseButtonUp(0))
        {
            fingerPosY1 = Input.mousePosition.y;
            float diffY = fingerPosY1 - fingerPosY0;
            if (Mathf.Abs(diffY) > posThY)  // Swipe vertically
            {
                SwipeVertically(diffY);
                return;
            }
            fingerPosX1 = Input.mousePosition.x;
            float diffX = fingerPosX1 - fingerPosX0;
            if (Mathf.Abs(diffX) > posThX)  // Swipe horizontally
            {
                SwipeHorizontally(diffX);
                return;
            }
            else    // Tap or Double Tap
            {
                tapCount++;
                Invoke(nameof(Tap), doubleTapTh);
            }
        }
    }

    private void SwipeVertically(float diffY)
    {
        TitleManager.titleAudioSource.PlayOneShot(swipeSound);

        if (diffY > 0) roomNum--;
        else roomNum++;

        if (roomNum == 0) roomNum = 3;
        else if (roomNum == 4) roomNum = 1;

        ChangeRoom();
    }

    private void SwipeHorizontally(float diffX)
    {
        if (diffX > 0)  // Titleに戻る
        {
            TitleManager.titleAudioSource.PlayOneShot(swipeSound);
            SceneManager.LoadScene("Title");
        }
    }

    private void ChangeRoom()
    {
        switch (roomNum)
        {
            case 1:
                TitleManager.titleAudioSource1.PlayOneShot(room1Voice);
                room1On.SetActive(true);
                room1Off.SetActive(false);
                room2On.SetActive(false);
                room2Off.SetActive(true);
                room3On.SetActive(false);
                room3Off.SetActive(true);
                break;
            case 2:
                TitleManager.titleAudioSource1.PlayOneShot(room2Voice);
                room1On.SetActive(false);
                room1Off.SetActive(true);
                room2On.SetActive(true);
                room2Off.SetActive(false);
                room3On.SetActive(false);
                room3Off.SetActive(true);
                break;
            case 3:
                TitleManager.titleAudioSource1.PlayOneShot(room3Voice);
                room1On.SetActive(false);
                room1Off.SetActive(true);
                room2On.SetActive(false);
                room2Off.SetActive(true);
                room3On.SetActive(true);
                room3Off.SetActive(false);
                break;
            default:
                break;
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
            
            // ページ遷移
            EnterRoom();
        }
        tapCount = 0;
    }

    private void EnterRoom()
    {
        popupCanvas.SetActive(true);
        TitleManager.titleAudioSource.clip = standbySound;
        TitleManager.titleAudioSource.Play();
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        switch (roomNum)
        {
            case 1:
                PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions(), TypedLobby.Default);
                break;
            case 2:
                PhotonNetwork.JoinOrCreateRoom("Room2", new RoomOptions(), TypedLobby.Default);
                break;
            case 3:
                PhotonNetwork.JoinOrCreateRoom("Room3", new RoomOptions(), TypedLobby.Default);
                break;
            default:
                break;
        }
    }

    public override void OnJoinedRoom()
    {
        popupCanvas.SetActive(false);
        TitleManager.titleAudioSource.Stop();
        TitleManager.titleAudioSource.PlayOneShot(doubleTapSound);

        PhotonNetwork.IsMessageQueueRunning = false;
        SceneManager.LoadScene("Tool");
    }
}
