using System;
using System.Collections.Generic;
using UnityEngine;

public class MooseSnap : MonoBehaviour
{
    public struct UpdateResult
    {
        public bool stillStanding;
        public int wallCollision;
        public bool solidWallCollision;
    }

    struct SurfaceCoords
    {
        public float degrees;
        public float radius;
        public float normalDegrees;
    }

    struct SnapResult
    {
        public bool success;
        public SurfaceCoords newPos;
        public Vector2 normal;
    }

    public float PushForce;
    public bool FixVX;

    Collider2D _target;
    Rigidbody2D _targetBody;

    SurfaceCoords _curPos;
    SurfaceCoords _lastPos;
    bool _updateFlag;
    bool _justSnapped;
    MooseDimensions _heroDim;
    MooseController _mooseController;

    Vector2 _lastWorldPos;
    Vector2 _curWorldPos;
    Vector2 _velocityEstimate;

    public Vector2 Position { get { return _curWorldPos; } }
    public Vector2 VelocityEstimate { get { return _velocityEstimate; } }

    void Awake()
    {
        enabled = false;
        _heroDim = GetComponent<MooseDimensions>();
        _mooseController = GetComponent<MooseController>();
    }

    void Update()
    {
        if (_target == null) return;
        if (_justSnapped) return;

        var dt = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        var p0 = surfaceCoordsToWorldCoords(_lastPos);
        var p1 = surfaceCoordsToWorldCoords(_curPos);

        transform.position = p0 + (p1 - p0) * dt;
        if (GravitySetting.Reverse) {
            transform.position -= Vector3.up * _heroDim.Height;
        }
    }

    void FixedUpdate()
    {
        _justSnapped = false;

        _curWorldPos = surfaceCoordsToWorldCoords(_curPos);
        _velocityEstimate = _curWorldPos - _lastWorldPos;
        _lastWorldPos = _curWorldPos;

        if (_updateFlag) _updateFlag = false;
        else _lastPos = _curPos;

        if (_target != null) {
            _target.gameObject.SendMessage("StayOn", null, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void SnapTo(Collider2D target, Rigidbody2D rb, Vector2 pos, Vector2 normal, Vector2 initialVelEstimate)
    {
        if (_target != null) Unsnap(false);

        enabled = true;

        _target = target;
        _targetBody = rb;

        _velocityEstimate = initialVelEstimate;
        _curPos = worldCoordsToSurfaceCoords(pos, normal);
        _curWorldPos = pos;
        _lastWorldPos = _curWorldPos;
        _lastPos = _curPos;
        _updateFlag = true;
        _justSnapped = true;
    }

    public void Unsnap(bool fromGrav)
    {
        if (_target == null) return;

        transform.position = surfaceCoordsToWorldCoords(_curPos);
        if (fromGrav != GravitySetting.Reverse) {
            transform.position -= Vector3.up * _heroDim.Height;
        }

        enabled = false;
        _target = null;
        _targetBody = null;
    }

    public UpdateResult UpdatePosition(float vx, Func<Vector2, bool> normalCheck)
    {
        if (_target == null) return new UpdateResult {
            stillStanding = false,
            wallCollision = 0,
            solidWallCollision = false
        };

        if (GravitySetting.Reverse) vx *= -1;

        return doUpdatePosition(vx, normalCheck, 0, false, true);
    }

    UpdateResult doUpdatePosition(float vx, Func<Vector2, bool> normalCheck, int collisionValue, bool collisionSolid, bool checkWalls)
    {
        var normalAngle = ((_targetBody ? _targetBody.rotation : _target.transform.rotation.z) + _curPos.normalDegrees) * Mathf.Deg2Rad;

        if (!checkWalls && Mathf.Abs(vx) < 0.0001f) {
            return new UpdateResult {
                stillStanding = normalCheck(Vector2Ext.FromPolar(1, normalAngle)),
                wallCollision = collisionValue,
                solidWallCollision = collisionSolid
            };
        }

        if (FixVX) {
            vx /= Mathf.Cos(normalAngle - Mathf.PI / 2);
        }

        var pos = surfaceCoordsToWorldCoords(_curPos);

        var normal = Vector2Ext.FromPolar(1, normalAngle);
        var tangent = normal.Rotate(-90);
        var moveVec = vx * tangent;
        var newPos = pos + moveVec;

        if (checkWalls) {
            var newMiddle = newPos + (GravitySetting.Reverse ? -1 : 1) * Vector2.up * _heroDim.HalfHeight;
            var wallTestL = newMiddle - (tangent * _heroDim.HalfWidth) / tangent.x;
            var wallTestR = newMiddle + (tangent * _heroDim.HalfWidth) / tangent.x;

            Debug.DrawLine(wallTestL, wallTestR, Color.magenta);

            bool pushed;
            var penetration = wallTest(wallTestL, wallTestR, vx > 0 ? PushForce : -PushForce, out pushed, normalCheck);

            if (penetration.HasValue) {
                var depth = penetration.Value;
                if (GravitySetting.Reverse) depth *= -1;
                var colVal = 0;
                if (!pushed) colVal = depth > 0 ? 1 : -1;
                return doUpdatePosition(vx - depth, normalCheck, colVal, !pushed, false);
            }

            if (Mathf.Abs(vx) < 0.0001f) {
                return new UpdateResult {
                    stillStanding = normalCheck(Vector2Ext.FromPolar(1, normalAngle)),
                    wallCollision = collisionValue,
                    solidWallCollision = collisionSolid
                };
            }
        }

        // Check if the center ray of the player hits the target collider.

        if (maybeMoveToSnap(linecastAndSnap(newPos + normal, newPos - normal), normalCheck)) {
            return new UpdateResult {
                stillStanding = true,
                wallCollision = collisionValue,
                solidWallCollision = collisionSolid
            };
        }

        // Check if we're walking around a 90 degree or greater bend.

        var cornerTestRot = -45 * Mathf.Sign(vx);
        var cornerNormal = normal.Rotate(cornerTestRot);
        var cornerMoveVec = moveVec.Rotate(cornerTestRot);
        var cornerNewPos = pos + cornerMoveVec;

        if (maybeMoveToSnap(linecastAndSnap(cornerNewPos + cornerNormal, cornerNewPos - cornerNormal), normalCheck)) {
            return new UpdateResult {
                stillStanding = true,
                wallCollision = collisionValue,
                solidWallCollision = collisionSolid
            };
        }

        // Check if we're edging out where the center doesn't collider, but an edge does.
        var edgeVec = tangent * (_heroDim.HalfWidth - _heroDim.InsetX);
        if (maybeMoveToSnap(linecastAndSnap(newPos + edgeVec + normal, newPos + edgeVec - normal, -edgeVec), normalCheck)
        ||  maybeMoveToSnap(linecastAndSnap(newPos - edgeVec + normal, newPos - edgeVec - normal,  edgeVec), normalCheck)) {
            return new UpdateResult {
                stillStanding = true,
                wallCollision = collisionValue,
                solidWallCollision = collisionSolid
            };
        }

        return new UpdateResult {
            stillStanding = false,
            wallCollision = collisionValue,
            solidWallCollision = collisionSolid
        };
    }

    float? wallTest(Vector2 p0, Vector2 p1, float forceX, out bool pushed, Func<Vector2, bool> normalCheck)
    {
        pushed = false;

        var result = DoubleLineCast.Cast(p0, p1, MooseController.CollisionLayerMask);
        if (!result.HasValue) return null;
        if (normalCheck(result.Value.normal)) return null;

        if (_mooseController.Charging) {
            var smashable = result.Value.collider.GetComponent<Smashable>();
            if (smashable) {
                smashable.Smash();
                return null;
            }
        }

        var targy = result.Value.collider.GetComponent<Rigidbody2D>();
        if (targy != null && !targy.isKinematic && result.Value.normal.x * forceX < 0) {
            pushed = true;
            targy.AddForceAtPosition(20 * forceX * Vector2.right, result.Value.point);
        }

        return result.Value.depth * -Mathf.Sign(result.Value.normal.x);
    }

    SnapResult linecastAndSnap(Vector2 p0, Vector2 p1) { return linecastAndSnap(p0, p1, Vector2.zero); }
    SnapResult linecastAndSnap(Vector2 p0, Vector2 p1, Vector2 offset)
    {
        var hitsPrime = Physics2D.LinecastAll(p0, p1, MooseController.CollisionLayerMask);
        var hitsDos = new List<RaycastHit2D>();
        for (var i = 0; i < hitsPrime.Length; ++i) {
            if (_targetBody) {
                if (hitsPrime[i].rigidbody == _targetBody) {
                    hitsDos.Add(hitsPrime[i]);
                }
            } else {
                if (hitsPrime[i].collider == _target) {
                    hitsDos.Add(hitsPrime[i]);
                }
            }
        }
        if (hitsDos.Count == 0) return new SnapResult { success = false };
        var hit = hitsDos[0];

        return new SnapResult {
            success = true,
            newPos = worldCoordsToSurfaceCoords(hit.point + offset, hit.normal),
            normal = hit.normal
        };
    }

    bool maybeMoveToSnap(SnapResult snap, Func<Vector2, bool> normalCheck)
    {
        if (!snap.success || !normalCheck(snap.normal)) return false;
        moveToSurfaceCoord(snap.newPos);
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
        var targetObject = _targetBody ? _targetBody.gameObject : _target.gameObject;

        var theta = (targetObject.transform.rotation.eulerAngles.z + s.degrees) * Mathf.Deg2Rad;
        return Vector2Ext.FromPolar(s.radius, theta) + targetObject.transform.position.AsVector2();
    }

    SurfaceCoords worldCoordsToSurfaceCoords(Vector2 pos, Vector2 normal)
    {
        var ds = pos - (_targetBody ? _targetBody.position : _target.transform.position.AsVector2());
        var angleOffset = _targetBody ? _targetBody.rotation : _target.transform.rotation.z;

        return new SurfaceCoords {
            radius = ds.magnitude,
            degrees = Mathf.Atan2(ds.y, ds.x) * Mathf.Rad2Deg - angleOffset,
            normalDegrees = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - angleOffset
        };
    }
}
