using System;
using System.Linq;
using UnityEngine;

public class SpiderSnap : MonoBehaviour
{
    struct SurfaceCoords
    {
        public float degrees;
        public float radius;
        public float normalDegrees;
    }

    Rigidbody2D _target;
    SurfaceCoords _curPos;
    SurfaceCoords _lastPos;
    bool _updateFlag;
    SpiderController _controller;

    Vector2 _lastWorldPos;
    Vector2 _curWorldPos;
    Vector2 _velocityEstimate;

    Vector2 _lastNormal;

    public Vector2 VelocityEstimate { get { return _velocityEstimate; } }
    public Vector2 Normal { get { return _lastNormal; } }

    void Awake()
    {
        enabled = false;
        _controller = GetComponent<SpiderController>();
    }

    void Update()
    {
        if (_target == null) return;

        var dt = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        var p0 = surfaceCoordsToWorldCoords(_lastPos);
        var p1 = surfaceCoordsToWorldCoords(_curPos);

        transform.position = p0 + (p1 - p0) * dt;
    }

    void FixedUpdate()
    {
        _curWorldPos = surfaceCoordsToWorldCoords(_curPos);
        _velocityEstimate = _curWorldPos - _lastWorldPos;
        _lastWorldPos = _curWorldPos;

        if (_updateFlag) _updateFlag = false;
        else _lastPos = _curPos;

        transform.localRotation = Quaternion.Euler(0, 0, _curPos.normalDegrees - 90);
    }

    public void SnapTo(Rigidbody2D target, Vector2 pos, Vector2 normal)
    {
        if (_target != null) Unsnap();

        enabled = true;
        _target = target;

        _target.gameObject.SendMessage("WalkOn", null, SendMessageOptions.DontRequireReceiver);

        _curPos = worldCoordsToSurfaceCoords(pos, normal);
        _lastNormal = Vector2.right.Rotate(_curPos.normalDegrees);
        _lastPos = _curPos;
        _updateFlag = true;
    }

    public void Unsnap()
    {
        if (_target == null) return;

        _target.gameObject.SendMessage("WalkOff", null, SendMessageOptions.DontRequireReceiver);

        enabled = false;
        _target = null;
    }

    public Vector2 CheckNormal()
    {
        var normalAngle = (_target.rotation + _curPos.normalDegrees) * Mathf.Deg2Rad;
        return Vector2Ext.FromPolar(1, normalAngle);
    }

    public void MoveClockwise(float units)
    {
        if (_target == null) return;

        var normalAngle = (_target.rotation + _curPos.normalDegrees) * Mathf.Deg2Rad;
        var normal = Vector2Ext.FromPolar(1, normalAngle);
        var moveVec = Vector2Ext.FromPolar(units, normalAngle - Mathf.PI / 2);
        var newPos = transform.position.AsVector2() + moveVec;

        if (linecastAndSnap(newPos + normal, newPos - normal)) return;

        var cornerTestRot = -45 * Mathf.Sign(units);
        var cornerNormal = normal.Rotate(cornerTestRot);
        var cornerMoveVec = moveVec.Rotate(cornerTestRot);
        var cornerNewPos = transform.position.AsVector2() + cornerMoveVec;

        if (linecastAndSnap(cornerNewPos + cornerNormal, cornerNewPos - cornerNormal)) return;

        cornerTestRot = 45 * Mathf.Sign(units);
        cornerNormal = normal.Rotate(cornerTestRot);
        cornerMoveVec = moveVec.Rotate(cornerTestRot);
        cornerNewPos = transform.position.AsVector2() + cornerMoveVec;

        if (linecastAndSnap(cornerNewPos + cornerNormal, cornerNewPos - cornerNormal)) return;
    }

    bool linecastAndSnap(Vector2 p0, Vector2 p1)
    {
        var hits = Physics2D.LinecastAll(p0, p1, SpiderController.CollisionLayerMask);
        if (hits.Length < 1) return false;
        var hit = hits[0];

        if (hit.point.VeryNear(p0)) return false;

        if (hit.collider.gameObject.tag == "Slippy" && _controller.isNormalLetGoable(hit.normal)) {
            Unsnap();
            return true;
        }

        if (hit.rigidbody != _target) {
            Unsnap();
            SnapTo(hit.rigidbody, hit.point, hit.normal);
        } else {
            moveToSurfaceCoord(worldCoordsToSurfaceCoords(hit.point, hit.normal));
        }

        return true;
    }

    void moveToSurfaceCoord(SurfaceCoords pos)
    {
        _lastPos = _curPos;
        _curPos = pos;
        _updateFlag = true;
        _lastNormal = Vector2.right.Rotate(_curPos.normalDegrees);
    }

    Vector2 surfaceCoordsToWorldCoords(SurfaceCoords s)
    {
        var theta = (_target.gameObject.transform.rotation.eulerAngles.z + s.degrees) * Mathf.Deg2Rad;
        return Vector2Ext.FromPolar(s.radius, theta) + _target.transform.position.AsVector2();
    }

    SurfaceCoords worldCoordsToSurfaceCoords(Vector2 pos, Vector2 normal)
    {
        var ds = pos - _target.position;
        var angleOffset = _target.rotation;

        return new SurfaceCoords {
            radius = ds.magnitude,
            degrees = Mathf.Atan2(ds.y, ds.x) * Mathf.Rad2Deg - angleOffset,
            normalDegrees = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - angleOffset
        };
    }
}
