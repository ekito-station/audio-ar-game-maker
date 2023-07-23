using UnityEngine;
using Photon.Pun;

public class WallController : ObjectController
{
    [System.NonSerialized] public Vector3 startPos;
    [System.NonSerialized] public Vector3 endPos;

    public void ConfigureWall(float halfDist, Vector3 startPosition, Vector3 endPosition)
    {
        photonView.RPC(nameof(ConfigureWallRPC), RpcTarget.AllBuffered, halfDist, startPosition, endPosition);
    }

    [PunRPC]
    public void ConfigureWallRPC(float halfDist, Vector3 startPosition, Vector3 endPosition)
    {
        audioSource.maxDistance = halfDist + 1.0f;
        audioSource.minDistance = halfDist;

        startPos = startPosition;
        endPos = endPosition;
    }

    // private void OnTriggerEnter(Collider other) 
    // {
    //     if (other.gameObject.tag == "MainCamera")
    //     {
    //         // audioSource.PlayOneShot(wallIndicator);
    //         audioSource.clip = wallIndicator;
    //         audioSource.Play();
    //     }
    // }

    // private void OnTriggerExit(Collider other) 
    // {
    //     if (other.gameObject.tag == "MainCamera")
    //     {
    //         audioSource.Stop();
    //     }
    // }
}
