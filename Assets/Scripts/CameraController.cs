using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;

    // Update is called once per frame
    void LateUpdate ()
    {
        var newPosition = Target.transform.position.WithZ(transform.position.z);
        transform.position += (newPosition - transform.position) / 10;
    }
}
