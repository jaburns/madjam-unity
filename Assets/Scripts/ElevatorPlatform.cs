using UnityEngine;
using System.Collections;

public class ElevatorPlatform : MonoBehaviour
{
    public float MaxY;

    Rigidbody2D _rb;
    bool _on;
    Vector2 _origin;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _origin = _rb.position;
    }

    void FixedUpdate()
    {
        if (_on) {
            if (_rb.position.y < MaxY) {
                _rb.MovePosition(_rb.position.WithY(_rb.position.y + 0.1f));
            } else {
                _rb.MovePosition(_origin.WithY(MaxY));
            }
        } else if (_rb.position.y > _origin.y) {
            _rb.MovePosition(_rb.position.WithY(_rb.position.y - 0.1f));
        } else {
            _rb.MovePosition(_origin);
        }
    }

    void WalkOn()
    {
        _on = true;
    }

    void WalkOff()
    {
        _on = false;
    }
}

