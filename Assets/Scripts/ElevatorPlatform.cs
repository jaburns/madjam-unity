using UnityEngine;

public class ElevatorPlatform : MonoBehaviour
{
    public float MaxY;

    Rigidbody2D _rb;
    int _on;
    Vector2 _origin;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _origin = _rb.position;
    }

    void FixedUpdate()
    {
        if ((_on > 0) == GravitySetting.Reverse) {
            seekMin();
        } else {
            seekMax();
        }

        if (_on > 0) _on--;
    }

    void seekMax()
    {
        if (_rb.position.y < MaxY) {
            _rb.MovePosition(_rb.position.WithY(_rb.position.y + 0.1f));
        } else {
            _rb.MovePosition(_origin.WithY(MaxY));
        }
    }

    void seekMin()
    {
        if (_rb.position.y > _origin.y) {
            _rb.MovePosition(_rb.position.WithY(_rb.position.y - 0.1f));
        } else {
            _rb.MovePosition(_origin);
        }
    }

    void StayOn()
    {
        _on = 2;
    }
}

