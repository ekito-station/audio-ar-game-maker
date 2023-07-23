using Photon.Pun;

public class FloorController : ObjectController
{
    public void ConfigureFloor(float radius)
    {
        photonView.RPC(nameof(ConfigureFloorRPC), RpcTarget.AllBuffered, radius);
    }

    [PunRPC]
    public void ConfigureFloorRPC(float radius)
    {
        audioSource.maxDistance = radius + 1.0f;
        audioSource.minDistance = radius;
    }

    // private void OnTriggerEnter(Collider other) 
    // {
    //     if (other.gameObject.tag == "MainCamera")
    //     {
    //         // audioSource.PlayOneShot(wallIndicator);
    //         audioSource.clip = floorSound;
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
