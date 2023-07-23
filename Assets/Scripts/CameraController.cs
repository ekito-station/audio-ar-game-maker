using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CameraController : MonoBehaviourPunCallbacks, IPunOwnershipCallbacks
{
    public ToolManager toolManager;
    public Camera arCamera;

    public AudioSource camAudioSource;
    public AudioClip eraseSound;

    public float interval = 0.1f;

    private Coroutine transformBall;
    private Coroutine transformLine;
    private Coroutine transformFloor;
    private Coroutine transformWall;
    
    public GameObject initialPosition;
    public GameObject currentPosition;
    private Coroutine volumeCoroutine;
    public float maxDistance = 0.5f;

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            // 指を離したらコルーチンを停止
            if (transformBall != null)
            {
                StopCoroutine(transformBall);
                transformBall = null;
            }
            if (transformLine != null)
            {
                StopCoroutine(transformLine);
                transformLine = null;
            }
            if (transformFloor != null)
            {
                StopCoroutine(transformFloor);
                transformFloor = null;
            }
            if (transformWall != null)
            {
                StopCoroutine(transformWall);
                transformWall = null;
            }
            if (volumeCoroutine != null)
            {
                StopCoroutine(volumeCoroutine);
                volumeCoroutine = null;
            }
            StopAllCoroutines();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject otherObj = other.gameObject;

        if (otherObj.tag == "Point" || otherObj.tag == "Base")
        {
            // 何もしない
        }
        else
        {
            ObjectController objCtrl = otherObj.GetComponent<ObjectController>();
            // 編集・削除
            if (toolManager.pageNum == 1 && toolManager.modeNum == 0)
            {
                Debug.Log("erase mode");
                camAudioSource.PlayOneShot(eraseSound);
                DestroyObj(otherObj);
                return;
            }

            // 当たった時に音が鳴る
            objCtrl.MakeSoundOnCollision();

            // 体験
            if (toolManager.pageNum == 2)
            {
                if (objCtrl.shouldDisappear)
                {
                    InstantiateSpeaker(otherObj);
                    DestroyObj(otherObj);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        GameObject otherObj = other.gameObject;
        ObjectController objCtrl = otherObj.GetComponent<ObjectController>();

        // // 編集・削除
        // if (toolManager.pageNum == 1 && toolManager.modeNum == 0)
        // {
        //     Debug.Log("erase mode");
        //     camAudioSource.PlayOneShot(eraseSound);
        //     DestroyObj(otherObj);
        //     return;
        // }

        // // （初めにオブジェクトに触れた時のみ）当たった時に音が鳴る
        // if (!objCtrl.isPlayed)
        // {
        //     objCtrl.MakeSoundOnCollision();
        //     objCtrl.isPlayed = true;
        // }

        // // 体験モードで設定によっては削除する
        // if (toolManager.pageNum == 2)
        // {
        //     if (objCtrl.shouldDisappear)
        //     {
        //         DestroyObj(otherObj);
        //         return;
        //     }
        // }

        // 編集・移動
        if (toolManager.pageNum == 1 && toolManager.modeNum == 1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 操作ログを保存
                toolManager.moveNum += 1;
                PlayerPrefs.SetInt("moveNum", toolManager.moveNum);
                toolManager.moveNumText.text = "moveNum " + toolManager.moveNum.ToString();
                PlayerPrefs.Save();
                // ネットワークオブジェクトの所有権を確認
                PhotonView photonViewComponent = otherObj.GetComponent<PhotonView>();
                if (!photonViewComponent.IsMine)
                {
                    // 自分が所有者でない場合移譲してもらう
                    photonViewComponent.TransferOwnership(PhotonNetwork.LocalPlayer);
                    Debug.Log("TransferOwnership（編集・移動）");
                }
            }
            if (Input.GetMouseButton(0))
            {
                if (otherObj.tag == "Wall" || otherObj.tag == "Floor")
                {
                    // オブジェクトの位置をプレイヤーの位置に
                    Vector3 pos0 = arCamera.transform.position;
                    otherObj.transform.position = new Vector3(pos0.x, 0.0f, pos0.z);
                    // オブジェクトの向きをプレイヤーに向かせる
                    otherObj.transform.eulerAngles = new Vector3(0.0f, arCamera.transform.eulerAngles.y, 0.0f);
                }
                else
                {
                    // オブジェクトの位置をプレイヤーの位置に
                    otherObj.transform.position = arCamera.transform.position;
                }
            }
        }

        // 編集・変形
        if (toolManager.pageNum == 1 && toolManager.modeNum == 2)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 操作ログを保存
                toolManager.transformNum += 1;
                PlayerPrefs.SetInt("transformNum", toolManager.transformNum);
                toolManager.transformNumText.text = "transformNum " + toolManager.transformNum.ToString();
                PlayerPrefs.Save();
                // ネットワークオブジェクトの所有権を確認
                PhotonView photonViewComponent = otherObj.GetComponent<PhotonView>();
                if (!photonViewComponent.IsMine)
                {
                    // 自分が所有者でない場合移譲してもらう
                    photonViewComponent.TransferOwnership(PhotonNetwork.LocalPlayer);
                    Debug.Log("TransferOwnership（編集・変形）");
                }
                // オブジェクトの変形
                if (otherObj.tag == "Ball")
                {
                    transformBall = StartCoroutine(TransformBall(otherObj));
                }
                else if (otherObj.tag == "Line")
                {
                    transformLine = StartCoroutine(TransformLine(otherObj));
                }
                else if (otherObj.tag == "Floor")
                {
                    transformFloor = StartCoroutine(TransformFloor(otherObj));
                }
                else if (otherObj.tag == "Wall")
                {
                    transformWall = StartCoroutine(TransformWall(otherObj));
                }
            }
        }

        // 編集・音量
        if (toolManager.pageNum == 1 && toolManager.modeNum == 3)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 操作ログを保存
                toolManager.volumeNum += 1;
                PlayerPrefs.SetInt("volumeNum", toolManager.volumeNum);
                toolManager.volumeNumText.text = "volumeNum " + toolManager.volumeNum.ToString();
                PlayerPrefs.Save();
                // InitialPositionの位置・向きをARCameraと合わせる
                initialPosition.transform.position = arCamera.transform.position;
                initialPosition.transform.eulerAngles = new Vector3(0.0f, arCamera.transform.eulerAngles.y, 0.0f);
                // 音量調整のコルーチンを開始
                volumeCoroutine = StartCoroutine(VolumeCoroutine(objCtrl));
            }
        }
    }
    
    // 体験時にオブジェクトが削除された場合、代わりに音を鳴らすSpeakerを生成する関数
    private void InstantiateSpeaker(GameObject obj)
    {
        AudioSource objAudioSource = obj.GetComponent<AudioSource>();
        float maxDist = objAudioSource.maxDistance;
        float minDist = objAudioSource.minDistance;
        float volume = objAudioSource.volume;
        ObjectController objCtrl = obj.GetComponent<ObjectController>();
        int soundOnCollision = objCtrl.soundOnCollision;
        
        GameObject speaker = PhotonNetwork.Instantiate("Speaker", obj.transform.position, Quaternion.identity);
        SpeakerController speakerCtrl = speaker.GetComponent<SpeakerController>();
        speakerCtrl.ConfigureSpeaker(maxDist, minDist, volume, soundOnCollision);
    }

    // オブジェクトを削除する関数
    private void DestroyObj(GameObject obj)
    {
        Debug.Log("DestroyObj");
        // ネットワークオブジェクトの所有権を確認
        PhotonView photonViewComponent = obj.GetComponent<PhotonView>();
        if (photonViewComponent.IsMine)
        {
            PhotonNetwork.Destroy(obj);
        }
        else
        {
            photonViewComponent.TransferOwnership(PhotonNetwork.LocalPlayer);
            Debug.Log("TransferOwnership");
        }
        // 操作ログを保存
        toolManager.eraseNum += 1;
        PlayerPrefs.SetInt("eraseNum", toolManager.eraseNum);
        toolManager.eraseNumText.text = "eraseNum " + toolManager.eraseNum.ToString();
        PlayerPrefs.Save();
    }

    void IPunOwnershipCallbacks.OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {

    }

    void IPunOwnershipCallbacks.OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        if (toolManager.pageNum == 1 && toolManager.modeNum == 0)
        {
            PhotonNetwork.Destroy(targetView.gameObject);
            Debug.Log("Destroyed the object. (編集・削除)");
        }
        else if (toolManager.pageNum == 2)
        {
            PhotonNetwork.Destroy(targetView.gameObject);
            Debug.Log("Destroyed the object. (体験)");
        }
    }

    void IPunOwnershipCallbacks.OnOwnershipTransferFailed(PhotonView targetView, Player previousOwner)
    {

    }

    // Ballを変形するコルーチン
    private IEnumerator TransformBall(GameObject ball)
    {
        Vector3 centerPos = ball.transform.position;
        BallController ballCtrl = ball.GetComponent<BallController>();

        while (true)
        {
            // 変形
            Vector3 endPos = toolManager.CalcPos();
            float radius = (endPos - centerPos).magnitude;
            float diameter = radius * 2;
            ball.transform.localScale = new Vector3(diameter, diameter, diameter);
            // Ball向け設定の変更
            ballCtrl.ConfigureBall(radius);

            yield return new WaitForSeconds(interval);
        }
    }

    // Lineを変形するコルーチン
    private IEnumerator TransformLine(GameObject line)
    {
        Vector3 userPos = toolManager.CalcPos();
        LineController lineCtrl = line.GetComponent<LineController>();

        // 端点との距離を比較し、動かす点を1つ選ぶ
        Vector3 startPos = lineCtrl.startPos;
        Vector3 endPos = lineCtrl.endPos;
        float distToStart = Vector3.SqrMagnitude(userPos - startPos);
        float distToEnd = Vector3.SqrMagnitude(userPos - endPos);

        bool isStartMoving;
        if (distToStart < distToEnd)
        {
            isStartMoving = true;
        }
        else
        {
            isStartMoving = false;
        }

        while (true)
        {
            // 現在位置を取得
            Vector3 curPos = toolManager.CalcPos();
            // Lineの中点を取得
            Vector3 lineVec;
            Vector3 centerCoord;
            if (isStartMoving)
            {
                lineVec = endPos - curPos;
                centerCoord = curPos + lineVec * 0.5f;
            }
            else
            {
                lineVec = curPos - startPos;
                centerCoord = startPos + lineVec * 0.5f;
            }
            float dist = lineVec.magnitude;
            Vector3 lineY = new Vector3(0.0f, dist, 0.0f);
            // 変形
            line.transform.position = centerCoord;
            line.transform.rotation = Quaternion.identity;
            line.transform.rotation = Quaternion.FromToRotation(lineY, lineVec);
            Vector3 lineScale = line.transform.localScale;
            line.transform.localScale = new Vector3(lineScale.x, dist / 2, lineScale.z);
            // Line向け設定の変更
            if (isStartMoving)
            {
                lineCtrl.ConfigureLine(dist / 2 + toolManager.front, curPos, endPos);
            }
            else
            {
                lineCtrl.ConfigureLine(dist / 2 + toolManager.front, startPos, curPos);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    // Floorを変形するコルーチン
    private IEnumerator TransformFloor(GameObject floor)
    {
        Vector3 centerPos = floor.transform.position;
        FloorController floorCtrl = floor.GetComponent<FloorController>();

        while (true)
        {
            // 変形
            Vector3 endPos = toolManager.CalcPos();
            Vector3 endPos1 = new Vector3(endPos.x, 0.0f, endPos.z);
            float radius = (endPos1 - centerPos).magnitude;
            float diameter = radius * 2;
            Vector3 floorScale = floor.transform.localScale;
            floor.transform.localScale = new Vector3(diameter, floorScale.y, diameter);
            // Floor向け設定の変更
            floorCtrl.ConfigureFloor(radius);

            yield return new WaitForSeconds(interval);
        }
    }

    // Wallを変形するコルーチン
    private IEnumerator TransformWall(GameObject wall)
    {
        Vector3 userPos = toolManager.CalcPos();
        WallController wallCtrl = wall.GetComponent<WallController>();

        // 端点との距離を比較し、動かす点を1つ選ぶ
        Vector3 startPos = wallCtrl.startPos;
        Vector3 endPos = wallCtrl.endPos;
        float distToStart = Vector3.SqrMagnitude(userPos - startPos);
        float distToEnd = Vector3.SqrMagnitude(userPos - endPos);

        bool isStartMoving;
        if (distToStart < distToEnd)
        {
            isStartMoving = true;
        }
        else
        {
            isStartMoving = false;
        }

        while (true)
        {
            // 現在位置を取得
            Vector3 curPos = toolManager.CalcPos();
            Vector3 curPos1 = new Vector3(curPos.x, 0.0f, curPos.z);
            // Wallの中点を取得
            Vector3 wallVec;
            Vector3 centerCoord;
            if (isStartMoving)
            {
                wallVec = endPos - curPos1;
                centerCoord = curPos1 + wallVec * 0.5f;
            }
            else
            {
                wallVec = curPos1 - startPos;
                centerCoord = startPos + wallVec * 0.5f;
            }
            float dist = wallVec.magnitude;
            Vector3 wallX = new Vector3(dist, 0.0f, 0.0f);
            // 変形
            wall.transform.position = centerCoord;
            wall.transform.rotation = Quaternion.identity;
            wall.transform.rotation = Quaternion.FromToRotation(wallX, wallVec);
            Vector3 wallScale = wall.transform.localScale;
            wall.transform.localScale = new Vector3(dist, wallScale.y, wallScale.z);
            // Wall向け設定の変更
            if (isStartMoving)
            {
                wallCtrl.ConfigureWall(dist / 2 + toolManager.front, curPos1, endPos);
            }
            else
            {
                wallCtrl.ConfigureWall(dist / 2 + toolManager.front, startPos, curPos1);
            }

            yield return new WaitForSeconds(interval);
        }
    }

    // 音量を変更するコルーチン
    private IEnumerator VolumeCoroutine(ObjectController objCtrl)
    {
        while (true)
        {
            currentPosition.transform.position = arCamera.transform.position;

            float distance = currentPosition.transform.localPosition.x;
            if (distance > 0.4f)
            {
                distance = 0.4f;
            }
            else if (distance < -0.4f)
            {
                distance = -0.4f;
            }

            objCtrl.ChangeVolume(distance);

            yield return new WaitForSeconds(interval);
        }
    }

    // private void OnTriggerExit(Collider other)
    // {
    //     GameObject otherObj = other.gameObject;
    //     ObjectController objCtrl = otherObj.GetComponent<ObjectController>();

    //     objCtrl.isPlayed = false;
    // }
}
