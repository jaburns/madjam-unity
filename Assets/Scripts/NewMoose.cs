using UnityEngine;

public class NewMoose : MonoBehaviour
{
    public int StampedeLength = 30;
    public float StampedeSpeed = 10;
    public float NormalSpeed = 3;

    public bool Charging { get { return _stampedeCount > 0; } }

    BlobBinder _blobBinder;
    Rigidbody2D _rb;
    int _stampedeCount;
    bool _faceRight;
    float _vx;
    GameObject _floor;
    bool _newFloor;

    void Awake()
    {
        _blobBinder = GetComponentInChildren<BlobBinder>();
        _rb = GetComponent<Rigidbody2D>();
        _stampedeCount = 0;
        _vx = 0;
        _faceRight = false;
    }

    void FixedUpdate ()
    {
        if (_newFloor) {
            _floor.SendMessage("WalkOn", null, SendMessageOptions.DontRequireReceiver);
            _newFloor = false;
        }

        bool pressingLeft = false;
        bool pressingRight = false;

        if (_blobBinder.HasBlob) {
            pressingLeft = Controls.IsDown(Controls.Instance.Left);
            pressingRight = Controls.IsDown(Controls.Instance.Right);
            if (_stampedeCount == 0 && Controls.Instance.Act == Controls.ControlState.Press) {
                _stampedeCount = StampedeLength;
            }
        }

        if (_stampedeCount > 0) {
            _stampedeCount--;
            _vx = _faceRight ? StampedeSpeed : -StampedeSpeed;
        } else {
            if (pressingLeft) {
                _vx = -NormalSpeed;
                _faceRight = false;
            }
            else if (pressingRight) {
                _vx = NormalSpeed;
                _faceRight = true;
            }
            else {
                _vx = 0;
            }
        }

        _rb.velocity = new Vector2(_vx, _rb.velocity.y);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (_stampedeCount > 0) {
            foreach (var contact in col.contacts) {
                contact.collider.gameObject.SendMessage("Smash", null, SendMessageOptions.DontRequireReceiver);
            }
        }

        foreach (var contact in col.contacts) {
            if (BirdyController.normalIsFloor(contact.normal)) {
                _floor = contact.collider.gameObject;
                transform.parent = _floor.transform;
                _newFloor = true;
                break;
            }
        }
    }

    void OnCollisionExit2D(Collision2D col)
    {
        foreach (var contact in col.contacts) {
            if (contact.collider.gameObject == _floor) {
                leaveFloor();
                break;
            }
        }
    }

    void leaveFloor()
    {
        transform.parent = null;
        if (_floor != null) {
            _floor.SendMessage("WalkOff", null, SendMessageOptions.DontRequireReceiver);
            _floor = null;
        }
    }
}
