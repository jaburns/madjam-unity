using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
    public Vector2[] Nodes;
    public float Speed;

    Rigidbody2D _rb;
    int _curSeekNode;
    Vector2 _start;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _start = _rb.position;
        _curSeekNode = 0;
    }

    void FixedUpdate()
    {
        var node = getNode(_curSeekNode);
        var delta = (node - _rb.position);

        if (delta.sqrMagnitude < Speed*Speed) {
            _rb.MovePosition(node);
            _curSeekNode++;
            if (_curSeekNode >= Nodes.Length) {
                _curSeekNode = -1;
            }
        }

        _rb.MovePosition(_rb.position + delta.normalized * Speed);
    }

    Vector2 getNode(int i)
    {
        if (i < 0) return _start;
        return Nodes[i];
    }
}
