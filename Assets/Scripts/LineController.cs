using UnityEngine;
using Photon.Pun;

public class LineController : ObjectController
{
    [System.NonSerialized] public Vector3 startPos;
    [System.NonSerialized] public Vector3 endPos;

    public void ConfigureLine(float halfDist, Vector3 startPosition, Vector3 endPosition)
    {
        photonView.RPC(nameof(ConfigureLineRPC), RpcTarget.AllBuffered, halfDist, startPosition, endPosition);
    }

    [PunRPC]
    public void ConfigureLineRPC(float halfDist, Vector3 startPosition, Vector3 endPosition)
    {
        audioSource.maxDistance = halfDist + 1.0f;
        audioSource.minDistance = halfDist;

        startPos = startPosition;
        endPos = endPosition;
    }
}
