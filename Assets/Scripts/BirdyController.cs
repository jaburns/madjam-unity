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
            _rb.AddForce(Vector2.right * 10);
        }
    }
}
