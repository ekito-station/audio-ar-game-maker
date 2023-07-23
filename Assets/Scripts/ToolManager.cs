using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class ToolManager : MonoBehaviourPunCallbacks
{
    // 位置合わせ用
    [SerializeField] private ARSessionOrigin sessionOrigin;
    [SerializeField] private ARTrackedImageManager imageManager;
    private GameObject worldOrigin;    // ワールドの原点として振る舞うオブジェクト
    private Coroutine originCoroutine;
    // public GameObject checkTracking;
    public float trackInterval = 1.0f;
    private bool isTracked = false;

    public Camera arCamera;
    public float front;

    public TextMeshProUGUI pageText;
    public TextMeshProUGUI modeText;
    public TextMeshProUGUI optionText;

    public GameObject backCanvas;
    public GameObject popupCanvas;
    public GameObject adminCanvas;

    private float fingerPosX0;
    private float fingerPosX1;
    public float posThX;
    private float fingerPosY0;
    private float fingerPosY1;
    public float posThY;

    private int tapCount;
    private bool isDoubleTap;
    public float doubleTapTh = 0.5f;

    private bool drawingLine;
    private bool drawingWall;
    private bool drawingFloor;
    private Vector3 prePos;

    // public GameObject pointPrefab;
    public GameObject pointObj;

    public AudioSource camAudioSource;
    public AudioClip slideSound;
    public AudioClip pointSound;

    [System.NonSerialized] public int pageNum;    // 0: Place, 1: Edit, 2: Listen, 3: Setting
    [System.NonSerialized] public int modeNum;

    public float untilChangeMode = 1.0f;

    public AudioClip markerVoice;
    private bool isMarkerDetected = false;

    // 配置モードの音声
    public AudioClip placeVoice;
    public AudioClip pointVoice;
    public AudioClip lineVoice;
    public AudioClip floorVoice;
    public AudioClip wallVoice;

    // 編集モードの音声
    public AudioClip editVoice;
    public AudioClip moveVoice;
    public AudioClip transformVoice;
    public AudioClip volumeVoice;
    public AudioClip eraseVoice;

    // 聴取モードの音声
    public AudioClip listenVoice;

    // 設定モードの音声
    public AudioClip settingVoice;
    // 当たった時に消えるかどうか
    public AudioClip notDisappearVoice;
    public AudioClip disappearVoice;
    // 常に鳴っている音
    public AudioClip notSoundRingingVoice;
    public AudioClip soundRingingVoice;
    public AudioClip soundRingingVoice1;
    public AudioClip soundRingingVoice2;
    public AudioClip soundRingingVoice3;
    public AudioClip soundRingingVoice4;
    public AudioClip soundRingingVoice5;
    // 当たった時に聞こえる音
    public AudioClip notSoundOnCollisionVoice;
    public AudioClip soundOnCollisionVoice;
    public AudioClip soundOnCollisionVoice1;
    public AudioClip soundOnCollisionVoice2;
    public AudioClip soundOnCollisionVoice3;
    public AudioClip soundOnCollisionVoice4;
    public AudioClip soundOnCollisionVoice5;
    // 動き方
    public AudioClip notMoveVoice;
    public AudioClip moveForwardVoice;
    public AudioClip moveRandomlyVoice;

    // 設定用の変数
    // 当たった時に消えるかどうか
    [System.NonSerialized] public bool shouldDisappear;
    // 常に鳴っている音
    [System.NonSerialized] public int soundRinging = 1;
    public AudioClip soundRinging1;
    public AudioClip soundRinging2;
    public AudioClip soundRinging3;
    public AudioClip soundRinging4;
    public AudioClip soundRinging5;
    // 当たった時に聞こえる音
    [System.NonSerialized] public int soundOnCollision = 1;
    public AudioClip soundOnCollision1;
    public AudioClip soundOnCollision2;
    public AudioClip soundOnCollision3;
    public AudioClip soundOnCollision4;
    public AudioClip soundOnCollision5;
    // 動き方
    [System.NonSerialized] public int howToMove;    // 0: 動かない、1: 前に動く、2: ランダムに動く

    // 操作ログ用
    private bool isAdmin;

    [System.NonSerialized] public int ballNum;
    [System.NonSerialized] public int lineNum;
    [System.NonSerialized] public int floorNum;
    [System.NonSerialized] public int wallNum;

    [System.NonSerialized] public int moveNum;
    [System.NonSerialized] public int transformNum;
    [System.NonSerialized] public int volumeNum;
    [System.NonSerialized] public int eraseNum;

    [System.NonSerialized] public int disappearNum;
    [System.NonSerialized] public int soundRingingNum;
    [System.NonSerialized] public int soundOnCollisionNum;
    [System.NonSerialized] public int howToMoveNum;

    private bool isChangedDisappearNum;
    private bool isChangedSoundRingingNum;
    private bool isChangedSoundOnCollisionNum;
    private bool isChangedHowToMoveNum;

    public TextMeshProUGUI ballNumText;
    public TextMeshProUGUI lineNumText;
    public TextMeshProUGUI floorNumText;
    public TextMeshProUGUI wallNumText;

    public TextMeshProUGUI moveNumText;
    public TextMeshProUGUI transformNumText;
    public TextMeshProUGUI volumeNumText;
    public TextMeshProUGUI eraseNumText;

    public TextMeshProUGUI disappearNumText;
    public TextMeshProUGUI soundRingingNumText;
    public TextMeshProUGUI soundOnCollisionNumText;
    public TextMeshProUGUI howToMoveNumText;

    // 位置合わせ
    public override void OnEnable()
    {
        base.OnEnable();
        worldOrigin = new GameObject("Origin");
        Debug.Log("Created origin.");
        imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private IEnumerator OriginDecide(ARTrackedImage trackedImage, float trackInterval)
    {
        yield return null;
        Debug.Log("OriginDecide");
        Debug.Log("isTracked " + isTracked);

        if (!isTracked)
        {
            isTracked = true;
            Invoke(nameof(Retracking), trackInterval);

            var trackedImageTransform = trackedImage.transform;
            worldOrigin.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            // 原点をマーカーの位置に移す
            sessionOrigin.MakeContentAppearAt(worldOrigin.transform, trackedImageTransform.position, trackedImageTransform.localRotation);
            Debug.Log("Adjusted the origin.");
        }
        originCoroutine = null;
        // checkTracking.SetActive(false);
    }

    private void Retracking()
    {
        Debug.Log("Retracking");
        isTracked = false;
    }

    // // ワールド座標を任意の点から見たローカル座標に変換
    // public Vector3 WorldToOriginLocal(Vector3 world)    // worldはワールド座標
    // {
    //     return worldOrigin.transform.InverseTransformDirection(world);
    // }

    // TrackedImagesChanged時の処理
    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)  // eventArgsは検出イベントに関する引数
    {
        foreach (var trackedImage in eventArgs.added)
        {
            // checkTracking.SetActive(true);
            StartCoroutine(OriginDecide(trackedImage, 0));
            // デバイスを振動させる
            if (SystemInfo.supportsVibration)
            {
                Handheld.Vibrate();
            }
            OnMarkerDetected();
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            if (originCoroutine == null)
            {
                // checkTracking.SetActive(true);
                originCoroutine = StartCoroutine(OriginDecide(trackedImage, trackInterval));
                // デバイスを振動させる
                if (SystemInfo.supportsVibration)
                {
                    Handheld.Vibrate();
                }
            }
        }
    }

    private void Awake()
    {
        // 操作ログのロード
        LoadLog();
        isMarkerDetected = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true;

        // 画面初期化
        pageText.text = "";
        modeText.text = "";
        optionText.text = "";

        if (TitleManager.isMulti)
        {
# if UNITY_EDITOR
            OnMarkerDetected();
# else
            popupCanvas.SetActive(true);
            camAudioSource.PlayOneShot(markerVoice);
# endif
        }
        else
        {
            OnMarkerDetected();
        }
    }

    private void OnMarkerDetected()
    {
        isMarkerDetected = true;
        popupCanvas.SetActive(false);
        ChangePage();
    }

    private void LoadLog()
    {
        ballNum = PlayerPrefs.GetInt("ballNum", 0);
        lineNum = PlayerPrefs.GetInt("lineNum", 0);
        floorNum = PlayerPrefs.GetInt("floorNum", 0);
        wallNum = PlayerPrefs.GetInt("wallNum", 0);

        moveNum = PlayerPrefs.GetInt("moveNum", 0);
        transformNum = PlayerPrefs.GetInt("transformNum", 0);
        volumeNum = PlayerPrefs.GetInt("volumeNum", 0);
        eraseNum = PlayerPrefs.GetInt("eraseNum", 0);

        disappearNum = PlayerPrefs.GetInt("disappearNum", 0);
        soundRingingNum = PlayerPrefs.GetInt("soundRingingNum", 0);
        soundOnCollisionNum = PlayerPrefs.GetInt("soundOnCollisionNum", 0);
        howToMoveNum = PlayerPrefs.GetInt("howToMoveNum", 0);

        ballNumText.text = "ballNum " + ballNum.ToString();
        lineNumText.text = "lineNum " + lineNum.ToString();
        floorNumText.text = "floorNum " + floorNum.ToString();
        wallNumText.text = "wallNum " + wallNum.ToString();

        moveNumText.text = "moveNum " + moveNum.ToString();
        transformNumText.text = "transformNum " + transformNum.ToString();
        volumeNumText.text = "volumeNum " + volumeNum.ToString();
        eraseNumText.text = "eraseNum " + eraseNum.ToString();

        disappearNumText.text = "disappearNum " + disappearNum.ToString();
        soundRingingNumText.text = "soundRingingNum " + soundRingingNum.ToString();
        soundOnCollisionNumText.text = "soundOnCollisionNum " + soundOnCollisionNum.ToString();
        howToMoveNumText.text = "howToMoveNum " + howToMoveNum.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        // マーカーを読み込んでない場合は操作を無効にする
        if (!isMarkerDetected)
        {
            return;
        }

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
        camAudioSource.PlayOneShot(slideSound);

        // 管理者画面をオフに
        isAdmin = false;
        adminCanvas.SetActive(false);
        // 配置関連の変数を初期化
        drawingLine = false;
        drawingFloor = false;
        drawingWall = false;

        // 設定の操作ログを記録
        if (pageNum == 3)
        {
            if (isChangedDisappearNum)
            {
                disappearNum += 1;
                PlayerPrefs.SetInt("disappearNum", disappearNum);
                disappearNumText.text = "disappearNum " + disappearNum.ToString();
            }
            if (isChangedSoundRingingNum)
            {
                soundRingingNum += 1;
                PlayerPrefs.SetInt("soundRingingNum", soundRingingNum);
                soundRingingNumText.text = "soundRingingNum " + soundRingingNum.ToString();
            }
            if (isChangedSoundOnCollisionNum)
            {
                soundOnCollisionNum += 1;
                PlayerPrefs.SetInt("soundOnCollisionNum", soundOnCollisionNum);
                soundOnCollisionNumText.text = "soundOnCollisionNum " + soundOnCollisionNum.ToString();
            }
            if (isChangedHowToMoveNum)
            {
                howToMoveNum += 1;
                PlayerPrefs.SetInt("howToMoveNum", howToMoveNum);
                howToMoveNumText.text = "howToMoveNum " + howToMoveNum.ToString();
            }
            PlayerPrefs.Save();

            // 変数リセット
            isChangedDisappearNum = false;
            isChangedSoundRingingNum = false;
            isChangedSoundOnCollisionNum = false;
            isChangedHowToMoveNum = false;
        }

        // pageNumを変更
        if (diffY > 0) pageNum--;
        else pageNum++;

        if (pageNum == -1) pageNum = 3;
        else if (pageNum == 4) pageNum = 0;

        ChangePage();
    }

    private void SwipeHorizontally(float diffX)
    {
        // 配置関連の変数を初期化
        drawingLine = false;
        drawingFloor = false;
        drawingWall = false;

        if (pageNum != 2)   // 体験以外
        {
            camAudioSource.PlayOneShot(slideSound);

            if (diffX > 0) modeNum--;
            else modeNum++;

            switch (pageNum)
            {
                case 0: // 配置
                    if (modeNum == -1) modeNum = 3;
                    else if (modeNum == 4) modeNum = 0;
                    break;
                case 1: // 編集
                    if (modeNum == -1) modeNum = 3;
                    else if (modeNum == 4) modeNum = 0;
                    break;
                case 3: // 設定
                    if (modeNum == -1) modeNum = 3;
                    else if (modeNum == 0) camAudioSource.PlayOneShot(soundRingingVoice);
                    else if (modeNum == 1) camAudioSource.PlayOneShot(soundOnCollisionVoice);
                    else if (modeNum == 4) modeNum = 0;
                    break;
                default:
                    break;
            }

            ChangeMode();
        }
        else    // 体験の際に横スワイプ＋ダブルタップで管理画面表示
        {
            isAdmin = !isAdmin;
            adminCanvas.SetActive(false);
        }
    }

    private void ChangePage()
    {
        // モードのリセット
        modeNum = 0;
        modeText.text = "";
        CancelInvoke();

        switch (pageNum)
        {
            case 0: // 配置
                backCanvas.SetActive(false);
                pageText.text = "配置";
                camAudioSource.PlayOneShot(placeVoice);
                Invoke(nameof(ChangeMode), untilChangeMode);
                break;
            case 1: // 編集
                backCanvas.SetActive(false);
                pageText.text = "編集";
                camAudioSource.PlayOneShot(editVoice);
                Invoke(nameof(ChangeMode), untilChangeMode);
                break;
            case 2: // 体験
                backCanvas.SetActive(true);
                pageText.text = "体験";
                camAudioSource.PlayOneShot(listenVoice);
                modeText.text = "";
                break;
            case 3: // 設定
                backCanvas.SetActive(false);
                pageText.text = "設定";
                camAudioSource.PlayOneShot(settingVoice);
                Invoke(nameof(CallSoundRingingVoice), untilChangeMode);
                Invoke(nameof(ChangeMode), untilChangeMode);
                break;
            default:
                break;
        }
    }

    public void CallSoundRingingVoice()
    {
        camAudioSource.PlayOneShot(soundRingingVoice);
    }

    public void ChangeMode()
    {
        CancelInvoke(); // ChangeOptionの呼び出しを一旦キャンセル
        if (pageNum == 0)   // 配置
        {
            switch (modeNum)
            {
                case 0:
                    modeText.text = "点";
                    camAudioSource.PlayOneShot(pointVoice);
                    break;
                case 1:
                    modeText.text = "線分";
                    camAudioSource.PlayOneShot(lineVoice);
                    break;
                case 2:
                    modeText.text = "床";
                    camAudioSource.PlayOneShot(floorVoice);
                    break;
                case 3:
                    modeText.text = "壁";
                    camAudioSource.PlayOneShot(wallVoice);
                    break;
                default:
                    break;
            }
        }
        else if (pageNum == 1)  // 編集
        {
            switch (modeNum)
            {
                case 0:
                    modeText.text = "削除";
                    camAudioSource.PlayOneShot(eraseVoice);
                    break;
                case 1:
                    modeText.text = "移動";
                    camAudioSource.PlayOneShot(moveVoice);
                    break;
                case 2:
                    modeText.text = "変形";
                    camAudioSource.PlayOneShot(transformVoice);
                    break;
                case 3:
                    modeText.text = "音量";
                    camAudioSource.PlayOneShot(volumeVoice);
                    break;
                default:
                    break;
            }
        }
        else if (pageNum == 3)  // 設定
        {
            switch (modeNum)
            {
                case 0: // 聞こえる音
                    string firstText1 = "常に鳴る音";
                    switch (soundRinging)
                    {
                        case 0:
                            modeText.text = "常に鳴らない";
                            camAudioSource.PlayOneShot(notSoundRingingVoice);
                            break;
                        case 1:
                            modeText.text = firstText1 + "1";
                            // camAudioSource.PlayOneShot(soundRingingVoice1);
                            camAudioSource.PlayOneShot(soundRinging1);
                            break;
                        case 2:
                            modeText.text = firstText1 + "2";
                            // camAudioSource.PlayOneShot(soundRingingVoice2);
                            camAudioSource.PlayOneShot(soundRinging2);
                            break;
                        case 3:
                            modeText.text = firstText1 + "3";
                            // camAudioSource.PlayOneShot(soundRingingVoice3);
                            camAudioSource.PlayOneShot(soundRinging3);
                            break;
                        case 4:
                            modeText.text = firstText1 + "4";
                            // camAudioSource.PlayOneShot(soundRingingVoice4);
                            camAudioSource.PlayOneShot(soundRinging4);
                            break;
                        case 5:
                            modeText.text = firstText1 + "5";
                            // camAudioSource.PlayOneShot(soundRingingVoice5);
                            camAudioSource.PlayOneShot(soundRinging5);
                            break;
                        default:
                            break;
                    }
                    break;
                case 1: // 当たった時の音
                    string firstText2 = "当たった時に\n鳴る音";
                    switch (soundOnCollision)
                    {
                        case 0:
                            modeText.text = "当たった時に\n鳴らない";
                            camAudioSource.PlayOneShot(notSoundOnCollisionVoice);
                            break;
                        case 1:
                            modeText.text = firstText2 + "1";
                            // camAudioSource.PlayOneShot(soundOnCollisionVoice1);
                            camAudioSource.PlayOneShot(soundOnCollision1);
                            break;
                        case 2:
                            modeText.text = firstText2 + "2";
                            // camAudioSource.PlayOneShot(soundOnCollisionVoice2);
                            camAudioSource.PlayOneShot(soundOnCollision2);
                            break;
                        case 3:
                            modeText.text = firstText2 + "3";
                            // camAudioSource.PlayOneShot(soundOnCollisionVoice3);
                            camAudioSource.PlayOneShot(soundOnCollision3);
                            break;
                        case 4:
                            modeText.text = firstText2 + "4";
                            // camAudioSource.PlayOneShot(soundOnCollisionVoice4);
                            camAudioSource.PlayOneShot(soundOnCollision4);
                            break;
                        case 5:
                            modeText.text = firstText2 + "5";
                            // camAudioSource.PlayOneShot(soundOnCollisionVoice5);
                            camAudioSource.PlayOneShot(soundOnCollision5);
                            break;
                        default:
                            break;
                    }
                    break;
                case 2: // 動き方
                    switch (howToMove)
                    {
                        case 0:
                            modeText.text = "動かない";
                            camAudioSource.PlayOneShot(notMoveVoice);
                            break;
                        case 1:
                            modeText.text = "前に動く";
                            camAudioSource.PlayOneShot(moveForwardVoice);
                            break;
                        case 2:
                            modeText.text = "ランダムに動く";
                            camAudioSource.PlayOneShot(moveRandomlyVoice);
                            break;
                        default:
                            break;
                    }
                    break;
                case 3: // 当たった時に消えるかどうか
                    if (shouldDisappear)
                    {
                        modeText.text = "当たった時に\n消える";
                        camAudioSource.PlayOneShot(disappearVoice);
                    }
                    else
                    {
                        modeText.text = "当たった時に\n消えない";
                        camAudioSource.PlayOneShot(notDisappearVoice);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public Vector3 CalcPos()
    {
        Transform camTrans = arCamera.transform;
        Vector3 pos = camTrans.position + front * camTrans.forward;
        return pos;
    }

    private void Tap()
    {
        Vector3 curPos = CalcPos();
        if (tapCount < 2)
        {
            if (!isDoubleTap)   // Single Tap
            {
                if (pageNum == 0)   // 配置
                {
                    switch (modeNum)
                    {
                        case 0:
                            PlaceBall();
                            break;
                        case 1:
                            if (!drawingLine)
                            {
                                StartDrawingLine(curPos);
                            }
                            else
                            {
                                DrawLine(prePos, curPos);
                            }
                            break;
                        case 2:
                            DrawFloor(curPos);
                            break;
                        case 3:
                            if (!drawingWall)
                            {
                                StartDrawingWall(curPos);
                            }
                            else
                            {
                                DrawWall(prePos, curPos);
                            }
                            break;
                        default:
                            break;
                    }
                }
                else if (pageNum == 3)  // 設定
                {
                    switch (modeNum)
                    {
                        case 0: // 常に鳴る音
                            isChangedSoundRingingNum = true;
                            soundRinging += 1;
                            if (soundRinging > 5)
                            {
                                soundRinging = 0;
                            }
                            break;
                        case 1: // 当たった時に鳴る音
                            isChangedSoundOnCollisionNum = true;
                            soundOnCollision += 1;
                            if (soundOnCollision > 5)
                            {
                                soundOnCollision = 0;
                            }
                            break;
                        case 2: //　動き方
                            isChangedHowToMoveNum = true;
                            howToMove += 1;
                            if (howToMove > 2)
                            {
                                howToMove = 0;
                            }
                            break;
                        case 3: // 当たった時に消えるかどうか
                            isChangedDisappearNum = true;
                            shouldDisappear = !shouldDisappear;
                            break;
                        default:
                            break;
                    }
                    ChangeMode();
                }
            }
            isDoubleTap = false;
        }
        else    // Double Tap
        {
            isDoubleTap = true;
            if (pageNum == 0)   // 配置
            {
                switch (modeNum)
                {
                    case 1:
                        if (drawingLine)
                        {
                            DrawLine(prePos, curPos);
                            drawingLine = false;
                            // Pointを消す
                            pointObj.SetActive(false);
                        }
                        break;
                    case 3:
                        if (drawingWall)
                        {
                            DrawWall(prePos, curPos);
                            drawingWall = false;
                            // Pointを消す
                            pointObj.SetActive(false);
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (pageNum == 2)
            {
                if (isAdmin)    // 体験の際に横スワイプ＋ダブルタップで管理画面表示
                {
                    adminCanvas.SetActive(true);
                }
            }
        }
        tapCount = 0;
    }

    public void PlaceBall()
    {
        // 点を配置する
        Vector3 pos = CalcPos();
        GameObject ball = PhotonNetwork.Instantiate("Ball", pos, Quaternion.identity);
        //　設定を反映させる
        ConfigureObj(ball);
        // 操作ログを保存
        ballNum += 1;
        PlayerPrefs.SetInt("ballNum", ballNum);
        ballNumText.text = "ballNum " + ballNum.ToString();
        PlayerPrefs.Save();
    }

    public void StartDrawingLine(Vector3 _curPos)
    {
        // Pointを置く
        pointObj.SetActive(true);
        pointObj.transform.position = _curPos;
        camAudioSource.PlayOneShot(pointSound);
        // 始点を定める
        prePos = _curPos;
        drawingLine = true;
    }

    public void DrawLine(Vector3 _prePos, Vector3 _curPos)
    {
        // Pointを置く
        pointObj.transform.position = _curPos;
        // Lineの中点を取得
        Vector3 lineVec = _curPos - _prePos;    // Lineの方向を取得
        float dist = lineVec.magnitude;         // Lineの幅を取得
        Vector3 lineY = new Vector3(0.0f, dist, 0.0f);
        Vector3 halfLineVec = lineVec * 0.5f;
        Vector3 centerCoord = _prePos + halfLineVec;
        // Lineを引く
        GameObject line = PhotonNetwork.Instantiate("Line", centerCoord, Quaternion.identity);
        line.transform.rotation = Quaternion.FromToRotation(lineY, lineVec);
        Vector3 lineScale = line.transform.localScale;
        line.transform.localScale = new Vector3(lineScale.x, dist / 2, lineScale.z);
        // 設定を反映させる
        ConfigureObj(line);
        // Line向け設定を反映させる
        LineController lineCtrl = line.GetComponent<LineController>();
        lineCtrl.ConfigureLine(dist / 2 + front, _prePos, _curPos);
        // prePosを更新
        prePos = _curPos;
        // 操作ログを保存
        lineNum += 1;
        PlayerPrefs.SetInt("lineNum", lineNum);
        lineNumText.text = "lineNum " + lineNum.ToString();
        PlayerPrefs.Save();
    }

    public void DrawFloor(Vector3 _curPos)
    {
        if (!drawingFloor)
        {
            // Pointを置く
            Vector3 curPos1 = new Vector3(_curPos.x, 0.0f, _curPos.z);
            pointObj.SetActive(true);
            pointObj.transform.position = curPos1;
            camAudioSource.PlayOneShot(pointSound);
            // 中心を定める
            prePos = _curPos;
            drawingFloor = true;
        }
        else
        {
            // Pointを消す
            pointObj.SetActive(false);
            // 直径の長さを取得
            Vector3 prePos1 = new Vector3(prePos.x, 0.0f, prePos.z);
            Vector3 curPos1 = new Vector3(_curPos.x, 0.0f, _curPos.z);
            float dist = (curPos1 - prePos1).magnitude * 2 + front * 2;  // ユーザの場所の少し外側まで配置するために+front*2
            // 床を配置
            GameObject floor = PhotonNetwork.Instantiate("Floor", prePos1, Quaternion.identity);
            Vector3 floorScale = floor.transform.localScale;
            floor.transform.localScale = new Vector3(dist, floorScale.y, dist);
            // 設定を反映させる
            ConfigureObj(floor);
            // Floor向け設定を反映させる
            FloorController floorCtrl = floor.GetComponent<FloorController>();
            floorCtrl.ConfigureFloor(dist / 2);
            // drawingFloorを初期化
            drawingFloor = false;
            // 操作ログを保存
            floorNum += 1;
            PlayerPrefs.SetInt("floorNum", floorNum);
            floorNumText.text = "floorNum " + floorNum.ToString();
            PlayerPrefs.Save();
        }
    }

    public void StartDrawingWall(Vector3 _curPos)
    {
        // Pointを置く
        Vector3 curPos1 = new Vector3(_curPos.x, 0.0f, _curPos.z);
        pointObj.SetActive(true);
        pointObj.transform.position = curPos1;
        camAudioSource.PlayOneShot(pointSound);
        // 始点を定める
        prePos = _curPos;
        drawingWall = true;
    }

    public void DrawWall(Vector3 _prePos, Vector3 _curPos)
    {
        Vector3 prePos1 = new Vector3(_prePos.x, 0.0f, _prePos.z);
        Vector3 curPos1 = new Vector3(_curPos.x, 0.0f, _curPos.z);

        // Pointを置く
        pointObj.transform.position = curPos1;

        // 壁の中点を取得
        Vector3 wallVec = curPos1 - prePos1;    // 壁の方向を取得
        float dist = wallVec.magnitude;         // 壁の幅を取得
        Vector3 wallX = new Vector3(dist, 0.0f, 0.0f);
        Vector3 halfWallVec = wallVec * 0.5f;
        Vector3 centerCoord = prePos1 + halfWallVec;
        // 壁を描く
        GameObject wall = PhotonNetwork.Instantiate("Wall", centerCoord, Quaternion.identity);
        wall.transform.rotation = Quaternion.FromToRotation(wallX, wallVec);
        Vector3 wallScale = wall.transform.localScale;
        wall.transform.localScale = new Vector3(dist, wallScale.y, wallScale.z);
        // 設定を反映させる
        ConfigureObj(wall);
        // Wall向け設定を反映させる
        WallController wallCtrl = wall.GetComponent<WallController>();
        wallCtrl.ConfigureWall(dist / 2 + front, prePos1, curPos1);
        // prePosを更新
        prePos = _curPos;
        // 操作ログを保存
        wallNum += 1;
        PlayerPrefs.SetInt("wallNum", wallNum);
        wallNumText.text = "wallNum " + wallNum.ToString();
        PlayerPrefs.Save();
    }

    public void ConfigureObj(GameObject obj)
    {
        ObjectController objCtrl = obj.GetComponent<ObjectController>();
        objCtrl.ConfigureObj(shouldDisappear, soundRinging, soundOnCollision, howToMove, arCamera.transform.forward);
    }
}
