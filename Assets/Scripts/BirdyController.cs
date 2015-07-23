using UnityEngine;

public class BirdyController : MonoBehaviour
{
    BlobBinder _blobBinder;
    Rigidbody2D _rb;

	void Awake ()
	{
	    _blobBinder = GetComponentInChildren<BlobBinder>();
	    _rb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate()
    {
        if (_blobBinder.HasBlob) {
            if (Controls.Instance.Act == Controls.ControlState.Press) {
                _rb.isKinematic = false;
                _rb.AddForce(Vector2.up * 100);
            }
            if (!_rb.isKinematic) {
                if (Controls.IsDown(Controls.Instance.Right)) {
                    _rb.AddForce(Vector2.right * 10);
                } else if (Controls.IsDown(Controls.Instance.Left)) {
                    _rb.AddForce(-Vector2.right * 10);
                }
            }
        }
    }

    static bool normalIsFloor(Vector2 normal)
    {
        return normal.y > 0.5;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        foreach (var contact in col.contacts) {
            if (normalIsFloor(contact.normal)) {
                attachToFloor();
            }
        }
    }

    void attachToFloor()
    {
        _rb.isKinematic = true;
    }
}

