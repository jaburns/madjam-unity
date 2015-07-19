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

    Vector2 _lastWorldPos;
    Vector2 _curWorldPos;
    Vector2 _velocityEstimate;

    public Vector2 VelocityEstimate { get { return _velocityEstimate; } }
    public Vector2 Normal { get { return Vector2.right.Rotate(_curPos.normalDegrees); } }

    void Awake()
    {
        enabled = false;
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
    }

    public void SnapTo(Rigidbody2D target, Vector2 pos, Vector2 normal)
    {
        if (_target != null) Unsnap();

        enabled = true;
        _target = target;

        _curPos = worldCoordsToSurfaceCoords(pos, normal);
        _lastPos = _curPos;
        _updateFlag = true;
    }

    public void Unsnap()
    {
        if (_target == null) return;

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
        var hits = Physics2D.LinecastAll(p0, p1).Where(h => h.rigidbody == _target).ToArray();
        if (!hits.Any()) return false;
        var hit = hits.FirstOrDefault();

        if (hit.point.VeryNear(p0)) return false;

        if (hit.collider.gameObject.tag == "Slippy") {
            Unsnap();
            return true;
        }

        moveToSurfaceCoord(worldCoordsToSurfaceCoords(hit.point, hit.normal));
        return true;
    }

    void moveToSurfaceCoord(SurfaceCoords pos)
    {
        _lastPos = _curPos;
        _curPos = pos;
        _updateFlag = true;
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
