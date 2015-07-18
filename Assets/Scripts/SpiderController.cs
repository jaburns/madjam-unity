using UnityEngine;

public class SpiderController : MonoBehaviour
{
    const float SPEED = 0.5f;

    public Rigidbody2D DefaultGround;

    SpiderSnap _snap;
    BlobBinder _blobBinder;

    void Awake()
    {
        _snap = GetComponent<SpiderSnap>();
        _snap.SnapTo(DefaultGround, transform.position, Vector3.up);
        _blobBinder = GetComponentInChildren<BlobBinder>();
    }

    void FixedUpdate()
    {
        if (_blobBinder.HasBlob) {
            if (Controls.IsDown(Controls.Instance.Right)) {
                _snap.MoveClockwise(SPEED);
            } else if (Controls.IsDown(Controls.Instance.Left)) {
                _snap.MoveClockwise(-SPEED);
            }
        }
    }
}
