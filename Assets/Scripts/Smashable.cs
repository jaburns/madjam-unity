using UnityEngine;

public class Smashable : MonoBehaviour
{
    public void Smash()
    {
        transform.position = 1000 * Vector3.up;
    }
}
