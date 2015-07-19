using UnityEngine;

public class SpiderController : MonoBehaviour
{
    const float GRAVITY  =  2 * MooseController.MOVEMENT_SCALE;
    const float MAX_FALL = 16 * MooseController.MOVEMENT_SCALE;
    const float SPEED    = 0.5f;

    Vector2 _oldPosition;
    Vector2 _newPosition;
    float _vely;
    int _fallCheckCountdown;

    SpiderSnap _snap;
    BlobBinder _blobBinder;

    void Awake()
    {
        _newPosition = transform.position.AsVector2();
        _oldPosition = _newPosition;

        _snap = GetComponent<SpiderSnap>();
        _blobBinder = GetComponentInChildren<BlobBinder>();
    }

    void Update()
    {
        var p0 = _oldPosition.AsVector3(transform.position.z);
        var p1 = _newPosition.AsVector3(transform.position.z);
        transform.position = Vector3.Lerp(p0, p1, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);
    }

    void FixedUpdate()
    {
        _oldPosition = _newPosition;

        if (_snap.enabled) {
            if (_blobBinder.HasBlob) {
                if (Controls.IsDown(Controls.Instance.Right)) {
                    _snap.MoveClockwise(SPEED);
                } else if (Controls.IsDown(Controls.Instance.Left)) {
                    _snap.MoveClockwise(-SPEED);
                }
            }
            if (!_snap.enabled) {
                _vely = 0;
                _fallCheckCountdown = 10;
                _oldPosition = _newPosition = transform.position.AsVector2();
            }
        } else {
            _vely -= GRAVITY;
            if (_vely < -MAX_FALL) {
                _vely = -MAX_FALL;
            }
            _newPosition = fall(_newPosition);

            if (_fallCheckCountdown > 0) {
                _fallCheckCountdown--;
            } else {
                var p = transform.position.AsVector2();
                var hit = Physics2D.Linecast(p + Vector2.up, p);
                if (hit.rigidbody) {
                    _snap.SnapTo(hit.rigidbody, hit.point, hit.normal);
                }
            }
        }
    }

    Vector2 fall(Vector2 pos)
    {
        return pos.WithY(pos.y + _vely);
    }
}
