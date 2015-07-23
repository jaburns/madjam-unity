using UnityEngine;

public class BirdyController : MonoBehaviour
{
    const float MAX_X = 10;
    const float MAX_Y = 10;

    BlobBinder _blobBinder;
    Rigidbody2D _rb;
    GameObject _floor;

    void Awake ()
    {
        _blobBinder = GetComponentInChildren<BlobBinder>();
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
        if (_floor) {
            _floor.SendMessage("StayOn", null, SendMessageOptions.DontRequireReceiver);
        }

        if (_blobBinder.HasBlob) {
            if (Controls.Instance.Act == Controls.ControlState.Press) {
                _rb.velocity += Vector2.up * 5;
                leaveFloor();
            }
            if (!_rb.isKinematic) {
                if (Controls.IsDown(Controls.Instance.Right)) {
                    _rb.AddForce(Vector2.right * 10);
                } else if (Controls.IsDown(Controls.Instance.Left)) {
                    _rb.AddForce(-Vector2.right * 10);
                }
            }
        }
        if (_rb.velocity.x >  MAX_X) _rb.velocity = _rb.velocity.WithX( MAX_X);
        if (_rb.velocity.x < -MAX_X) _rb.velocity = _rb.velocity.WithX(-MAX_X);
        if (_rb.velocity.y >  MAX_Y) _rb.velocity = _rb.velocity.WithY( MAX_Y);
        if (_rb.velocity.y < -MAX_Y) _rb.velocity = _rb.velocity.WithY(-MAX_Y);
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

