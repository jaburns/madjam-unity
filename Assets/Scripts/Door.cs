using UnityEngine;

public class Door : MonoBehaviour
{
    void OnSwitch()
    {
        transform.position = 10000 * Vector3.up;
    }
}
