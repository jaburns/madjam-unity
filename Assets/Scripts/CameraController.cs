using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform Target;

    Rigidbody2D _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    //void LateUpdate ()
    //{
    //    var newPosition = Target.transform.position.WithZ(transform.position.z);
    //    transform.position += (newPosition - transform.position) / 10;
    //}

    void FixedUpdate()
    {
        // SmoothDamp
        var newPosition = Target.transform.position.WithZ(transform.position.z);
        var targPos = _rb.position + (newPosition.AsVector2() - _rb.position) / 10;
        _rb.MovePosition(targPos);
    }
}
