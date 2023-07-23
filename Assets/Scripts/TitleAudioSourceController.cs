using UnityEngine;

public class TitleAudioSourceController : MonoBehaviour
{
    public static bool isLoad = false;

    private void Awake() {
        if (isLoad) {
            Destroy(this.gameObject);
            return;
        }
        isLoad = true;
        DontDestroyOnLoad(this.gameObject);
    }
}
