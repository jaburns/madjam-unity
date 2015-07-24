using UnityEngine;

public class BirdyController : MonoBehaviour
{
    const float MAX_X = 10;
    const float MAX_Y = 10;

    public Transform ModelHolder;

    BlobBinder _blobBinder;
    Animator _anim;
    Rigidbody2D _rb;
    GameObject _floor;
    float _roto;
    bool _faceRight;

    void Awake ()
    {
        _blobBinder = GetComponentInChildren<BlobBinder>();
        _anim = GetComponentInChildren<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        GravitySetting.OnGravitySwitch += leaveFloor;
    }
    void OnDestroy()
    {
        GravitySetting.OnGravitySwitch -= leaveFloor;
    }

    void leaveFloor()
    {
        transform.parent = null;
        _rb.isKinematic = false;
        _floor = null;
    }

    void FixedUpdate()
    {
        if (GravitySetting.Reverse) {
            if (_roto < 180) {
                _roto += 30;
            } else {
                _roto = 180;
            }
        } else {
            if (_roto > 0) {
                _roto -= 30;
            } else {
                _roto = 0;
            }
        }
        ModelHolder.localRotation = Quaternion.Euler(_roto, 0, 0);

        if (_floor) {
            _floor.SendMessage("StayOn", null, SendMessageOptions.DontRequireReceiver);
        }

        _anim.SetBool("flying", _floor != null);

        if (_blobBinder.HasBlob) {
            if (Controls.Instance.Act == Controls.ControlState.Press) {
                _rb.velocity += Vector2.up * 5 * (GravitySetting.Reverse ? -1 : 1);
                leaveFloor();
            }
            if (!_rb.isKinematic) {
                if (Controls.IsDown(Controls.Instance.Right)) {
                    _rb.AddForce(Vector2.right * 10);
                    _faceRight = true;
                } else if (Controls.IsDown(Controls.Instance.Left)) {
                    _rb.AddForce(-Vector2.right * 10);
                    _faceRight = false;
                }
            }
        }
        if (_rb.velocity.x >  MAX_X) _rb.velocity = _rb.velocity.WithX( MAX_X);
        if (_rb.velocity.x < -MAX_X) _rb.velocity = _rb.velocity.WithX(-MAX_X);
        if (_rb.velocity.y >  MAX_Y) _rb.velocity = _rb.velocity.WithY( MAX_Y);
        if (_rb.velocity.y < -MAX_Y) _rb.velocity = _rb.velocity.WithY(-MAX_Y);

        var t = _anim.gameObject.transform;
        t.localScale = t.localScale.WithZ(Mathf.Abs(t.localScale.z) * (_faceRight ? 1 : -1));
        _blobBinder.transform.localPosition = _blobBinder.transform.localPosition.WithX(Mathf.Abs(_blobBinder.transform.localPosition.x) * (_faceRight ? 1 : -1));
    }

    static public bool normalIsFloor(Vector2 normal)
    {
        return GravitySetting.Reverse ? normal.y < -0.5 : normal.y > 0.5;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        foreach (var contact in col.contacts) {
            if (BirdyController.normalIsFloor(contact.normal)) {
                _floor = contact.collider.gameObject;
                transform.parent = _floor.transform;
                _rb.isKinematic = true;
                break;
            }
        }
    }
}

