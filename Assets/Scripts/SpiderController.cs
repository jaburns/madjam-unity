using UnityEngine;

public class SpiderController : MonoBehaviour
{
    const float MOVEMENT_SCALE = 0.62f / 17.6f;

    float GRAVITY  =  2 * MOVEMENT_SCALE;
    const float MAX_FALL = 16 * MOVEMENT_SCALE;
    const float SPEED    = 0.3f;

    Vector2 _oldPosition;
    Vector2 _newPosition;
    float _vely;
    int _fallCheckCountdown;
    bool _faceRight;
    Animator _anim;

    SpiderSnap _snap;
    BlobBinder _blobBinder;

    static public int CollisionLayerMask { get {
        return ~(
            1 << LayerMask.NameToLayer("Triggers") |
            1 << LayerMask.NameToLayer("Animals") |
            1 << LayerMask.NameToLayer("Death")
        );
    } }

    void Awake()
    {
        _newPosition = transform.position.AsVector2();
        _oldPosition = _newPosition;
        _vely = 5*GRAVITY;

        _snap = GetComponent<SpiderSnap>();
        _blobBinder = GetComponentInChildren<BlobBinder>();
        _anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        GravitySetting.OnGravitySwitch += GravitySwitch;
    }
    void OnDestroy()
    {
        GravitySetting.OnGravitySwitch -= GravitySwitch;
    }

    void GravitySwitch()
    {
        GRAVITY *= -1;
    }

    void Update()
    {
        var p0 = _oldPosition.AsVector3(transform.position.z);
        var p1 = _newPosition.AsVector3(transform.position.z);
        transform.position = Vector3.Lerp(p0, p1, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);
    }

    public bool isNormalLetGoable(Vector2 normal)
    {
        return GRAVITY > 0 ? (normal.y < 0.5f) : (normal.y > -0.5f);
    }

    void endSnap()
    {
        _snap.Unsnap();
        _vely = 0;
        _fallCheckCountdown = 2;
        _oldPosition = transform.position.AsVector2();
        _newPosition = _oldPosition + _snap.Normal * 0.2f;
    }

    void FixedUpdate()
    {
        _oldPosition = _newPosition;

        var t = _anim.gameObject.transform;
        t.localScale = t.localScale.WithZ(Mathf.Abs(t.localScale.z) * (_faceRight ? 1 : -1));

        if (_snap.enabled) {
            if (_blobBinder.HasBlob) {
                if (Controls.IsDown(Controls.Instance.Right)) {
                    _faceRight = true;
                    _snap.MoveClockwise(SPEED);
                } else if (Controls.IsDown(Controls.Instance.Left)) {
                    _faceRight = false;
                    _snap.MoveClockwise(-SPEED);
                }
                if (isNormalLetGoable(_snap.Normal) && Controls.IsDown(Controls.Instance.Act)) {
                    _snap.Unsnap();
                }
            }
            if (!_snap.enabled) {
                _vely = 0;
                _fallCheckCountdown = 2;
                _oldPosition = transform.position.AsVector2();
                _newPosition = _oldPosition + _snap.Normal * 0.2f;
            }
        } else {
            _vely -= GRAVITY;
            if (Mathf.Abs(_vely) > MAX_FALL) {
                _vely = (GRAVITY < 0) ? MAX_FALL : -MAX_FALL;
            }
            _newPosition.y += _vely;

            if (_fallCheckCountdown > 0) {
                _fallCheckCountdown--;
            } else {
                var p0 = transform.position.AsVector2();
                var p1 = transform.position.AsVector2() + (Vector2.up*_vely*1.5f);
                var hit = Physics2D.Linecast(p0, p1, CollisionLayerMask);
                if (hit.rigidbody && !isNormalLetGoable(hit.normal) && !p0.VeryNear(hit.point)) {
                    _snap.SnapTo(hit.rigidbody, hit.point, hit.normal);
                }
            }
        }
    }
}
