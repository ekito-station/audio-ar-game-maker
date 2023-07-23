using UnityEngine;
using Photon.Pun;

public class SpeakerController : MonoBehaviourPunCallbacks
{
    public AudioSource audioSource;
    public float waitTime;
    
    public void ConfigureSpeaker(float maxDistance, float minDistance, float volume, int soundOnCollision)
    {
        photonView.RPC(nameof(ConfigureSpeakerRPC), RpcTarget.AllBuffered, maxDistance, minDistance, volume, soundOnCollision);
        // 一定期間後に削除する
        Invoke(nameof(DestroySpeaker), waitTime);
    }

    [PunRPC]
    public void ConfigureSpeakerRPC(float maxDistance, float minDistance, float volume, int soundOnCollision)
    {
        // AudioSourceの設定を行う
        audioSource.maxDistance = maxDistance;
        audioSource.minDistance = minDistance;
        audioSource.volume = volume;
        
        // 当たった時の音を鳴らす
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

    private void DestroySpeaker()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }
}
