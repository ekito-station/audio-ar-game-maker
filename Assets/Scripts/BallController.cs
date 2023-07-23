using Photon.Pun;

public class BallController : ObjectController
{
    public void ConfigureBall(float radius)
    {
        photonView.RPC(nameof(ConfigureBallRPC), RpcTarget.AllBuffered, radius);
    }

    [PunRPC]
    public void ConfigureBallRPC(float radius)
    {
        audioSource.maxDistance = radius + 1.5f;
        audioSource.minDistance = radius;
    }
}
