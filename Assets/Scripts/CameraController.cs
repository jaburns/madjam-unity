using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;

    Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (Target == null) {
            Target = GameObject.Find("Blob").transform;
        }
    }

    void FixedUpdate()
    {
        // TODO look up SmoothDamp for cam movements
        //
        var newPosition = Target.transform.position.WithZ(transform.position.z);
        var targPos = _rb.position + (newPosition.AsVector2() - _rb.position) / 10;
        _rb.MovePosition(targPos);
    }
}
