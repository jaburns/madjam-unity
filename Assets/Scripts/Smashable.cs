using UnityEngine;

public class Smashable : MonoBehaviour
{
    public void Smash()
    {
        Debug.Log("Smash");
        transform.position = 1000 * Vector3.up;
    }
}
