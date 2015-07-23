using UnityEngine;

public class Door : MonoBehaviour
{
    void OnSwitch()
    {
        Destroy(gameObject);
    }
}
