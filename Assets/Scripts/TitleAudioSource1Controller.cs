using UnityEngine;

public class TitleAudioSource1Controller : MonoBehaviour
{
    public static bool isLoad1 = false;

    private void Awake() {
        if (isLoad1) {
            Destroy(this.gameObject);
            return;
        }
        isLoad1 = true;
        DontDestroyOnLoad(this.gameObject);
    }
}
