using UnityEngine;

public class MooseController : MonoBehaviour
{
    // Ratio of Flash game coordinate space to Unity units.
    public const float MOVEMENT_SCALE = 0.62f / 17.6f;
    //3.5227e-2f

    // Left/right movement
    const float MAX_RUN   = 4 * MOVEMENT_SCALE;
    const float RUN_DECEL = 2 * MOVEMENT_SCALE;
    const float RUN_ACCEL = 0.5f * MOVEMENT_SCALE;
    const float AIR_DECEL = 0.5f * MOVEMENT_SCALE;

    // Jumping
    float GRAVITY        =  2 * MOVEMENT_SCALE;
    const float MAX_FALL = 16 * MOVEMENT_SCALE;

    const int   STAMPEDE_LENGTH = 30;
    const float STAMPEDE_SPEED = 0.5f;
    const int   WAIT_TO_GRAZE_AFTER_MOVING = 30;

    static public int CollisionLayerMask { get {
        return SpiderController.CollisionLayerMask;
    } }

    MooseDimensions _heroDim;
    MooseSnap _snap;
    Animator _anim;

    Vector2 _newPosition;
    Vector2 _oldPosition;
    Vector2 _vel;

    int _cantSnapCounter;
    bool _faceRight = true;

    float _idleFenceLeft;
    float _idleFenceRight;
    int _idleState;

    int _afterControlsCount;
    int _stampedeCount;

    BlobBinder _blobBinder;

    public Vector2 Position { get { return _snap.enabled ? _snap.Position : _newPosition; } }
    public Vector2 Velocity { get { return _snap.enabled ? _snap.VelocityEstimate : _vel; } }
    public bool FaceRight { get { return _faceRight; } }
    public bool Charging { get { return _stampedeCount > 0; } }

    void Awake()
    {
        _newPosition = transform.position.AsVector2();
        _oldPosition = _newPosition;

        _snap = GetComponent<MooseSnap>();
        _heroDim = GetComponent<MooseDimensions>();
        _blobBinder = GetComponentInChildren<BlobBinder>();
        _anim = GetComponentInChildren<Animator>();

        endSnap(false);
    }

    void Start()
    {
        GravitySetting.OnGravitySwitch += GravitySwitch;
    }
    void OnDestroy()
    {
        GravitySetting.OnGravitySwitch -= GravitySwitch;
    }

    void Update()
    {
        var p0 = _oldPosition.AsVector3(transform.position.z);
        var p1 = _newPosition.AsVector3(transform.position.z);
        transform.position = Vector3.Lerp(p0, p1, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);
    }

    void GravitySwitch()
    {
        GRAVITY *= -1;
        endSnap(true);
    }

    void FixedUpdate()
    {
        if (_cantSnapCounter > 0) _cantSnapCounter--;
        _oldPosition = _newPosition;

        bool pressingLeft = false;
        bool pressingRight = false;

        if (_blobBinder.HasBlob) {
            pressingLeft = Controls.IsDown(Controls.Instance.Left);
            pressingRight = Controls.IsDown(Controls.Instance.Right);

            if (pressingLeft || pressingRight) {
                _afterControlsCount = WAIT_TO_GRAZE_AFTER_MOVING;
            }
        }

        if (!pressingLeft && !pressingRight || !_blobBinder.HasBlob) {
            if (_afterControlsCount > 0) {
                _afterControlsCount--;
            } else {
                if (Random.value > 0.9f) {
                    _idleState = Mathf.FloorToInt(Random.value * 3) - 1;
                }

                if (_idleState < 0 && transform.position.x < _idleFenceLeft) {
                    _idleState = 1;
                } else if (_idleState > 0 && transform.position.x > _idleFenceRight) {
                    _idleState = -1;
                }
// Disable erratic grazing
//                pressingLeft = _idleState < 0;
//                pressingRight = _idleState > 0;

                Debug.DrawLine(new Vector3(_idleFenceLeft, 1000, 0), new Vector3(_idleFenceLeft, -1000, 0));
                Debug.DrawLine(new Vector3(_idleFenceRight, 1000, 0), new Vector3(_idleFenceRight, -1000, 0));
            }
        } else {
            _idleFenceLeft = transform.position.x - 2.0f;
            _idleFenceRight = transform.position.x + 2.0f;
        }

        var t = _anim.gameObject.transform;
        t.localScale = t.localScale.WithZ(Mathf.Abs(t.localScale.z) * (_faceRight ? 1 : -1));

        if (_stampedeCount == 0 && Controls.Instance.Act == Controls.ControlState.Press && _blobBinder.HasBlob) {
            _stampedeCount = STAMPEDE_LENGTH;
        }

        if (_stampedeCount > 0) {
            _stampedeCount--;
            handleStampede();
        } else {
            handleRun(pressingLeft, pressingRight);
        }

        if (!_snap.enabled) {
            handleVertMovement();
        }

        if (_snap.enabled) {
            var updateResult = _snap.UpdatePosition(_vel.x, normalIsGroundGravDependent);

            if (updateResult.solidWallCollision) {
                _vel.x = 0;
            }
            if (!updateResult.stillStanding) {
                endSnap(false);
                freeMovement();
            }
        } else {
            freeMovement();
        }
    }

    static bool normalIsGround(Vector2 n) { return n.y >=  Mathf.Cos(Mathf.PI / 3); }
    static bool normalIsRoof(Vector2 n)   { return n.y <= -Mathf.Cos(Mathf.PI / 3); }
    static bool normalIsWall(Vector2 n)   { return !normalIsGround(n) && !normalIsRoof(n); }

    static bool normalIsGroundGravDependent(Vector2 n) { return GravitySetting.Reverse ? n.y <= -Mathf.Cos(Mathf.PI / 3) : n.y >=  Mathf.Cos(Mathf.PI / 3); }

    void freeMovement()
    {
        _newPosition =
            vertCollision(
            wallCollision(
                _newPosition + _vel
            ));
    }

    Vector2 vertCollision(Vector2 newPos)
    {
        var test = vertTestAtOffset(newPos, 0);
        if (test.HasValue) return test.Value;
        test = vertTestAtOffset(newPos, 1);
        if (test.HasValue) return test.Value;
        test = vertTestAtOffset(newPos, -1);
        if (test.HasValue) return test.Value;
        return newPos;
    }

    Vector2? vertTestAtOffset(Vector2 newPos, float offsetScale)
    {
        var offVec = offsetScale * new Vector2(_heroDim.HalfWidth - _heroDim.InsetX, 0);
        var pt0 = newPos + Vector2.up * _heroDim.Height + offVec;
        var pt1 = newPos + offVec;
        var maybeHit = DoubleLineCast.Cast(pt0, pt1, CollisionLayerMask);
        Debug.DrawLine(pt0, pt1, Color.green);
        if (maybeHit.HasValue && !maybeHit.Value.embedded) {
            return reactToVertCollision(newPos, maybeHit.Value, Vector2.zero);
        }
        return null;
    }

    Vector2 reactToVertCollision(Vector2 newPos, DoubleLineCast.Result hit, Vector2 offset)
    {
        if (normalIsGround(hit.normal)) {
            _vel.y = 0;
            newPos.y = hit.point.y;
            if (!GravitySetting.Reverse) {
                if (_cantSnapCounter == 0) {
                    startSnap(hit.collider, hit.rigidbody, newPos - offset, hit.normal);
                }
            }
        } else if (normalIsRoof(hit.normal)) {
            if (_vel.y > 0) {
                _vel.y = 0;
            }
            newPos.y = (hit.point - Vector2.up * _heroDim.Height).y;
            if (GravitySetting.Reverse) {
                if (_cantSnapCounter == 0) {
                    newPos.y = hit.point.y;
                    startSnap(hit.collider, hit.rigidbody, newPos - offset, hit.normal);
                }
            }
        }

        return newPos;
    }

    Vector2 wallCollision(Vector2 newPos)
    {
        float firstY = Mathf.Max(
            _vel.y < 0 ? -_vel.y : 0,
            _heroDim.InsetY
        );
        float lastY = Mathf.Min(
            _vel.y > 0 ? _heroDim.Height - _vel.y : _heroDim.Height,
            _heroDim.Height - _heroDim.InsetY
        );
        float stepY = (lastY - firstY) / 2;

        for (float offy = firstY; offy < lastY+1e-9f; offy += stepY) {
            var offset = Vector2.up * offy;
            var pt0 = newPos + offset - _heroDim.HalfWidth * Vector2.right;
            var pt1 = newPos + offset + _heroDim.HalfWidth * Vector2.right;
            Debug.DrawLine(pt0, pt1, Color.green);

            // hmm
            var maybeHit = DoubleLineCast.Cast(pt0, pt1, CollisionLayerMask);
            if (maybeHit.HasValue && normalIsWall(maybeHit.Value.normal)) {
                newPos.x = maybeHit.Value.point.x + Mathf.Sign(maybeHit.Value.normal.x) * _heroDim.HalfWidth;
                if (maybeHit.Value.normal.x * _vel.x < 0) {
                    _vel.x = 0;
                }
            }
        }

        return newPos;
    }

    void startSnap(Collider2D coll, Rigidbody2D rb, Vector2 pt, Vector2 normal)
    {
        _idleFenceLeft = transform.position.x - 2.0f;
        _idleFenceRight = transform.position.x + 2.0f;
        _snap.SnapTo(coll, rb, pt, normal, _vel);
    }

    void endSnap(bool fromGrav)
    {
        _snap.Unsnap(fromGrav);
        _cantSnapCounter = 2;
        _vel.y = _snap.VelocityEstimate.y;
        _oldPosition = transform.position.AsVector2();
        _newPosition = _oldPosition;
        _snap.enabled = false;
    }

    void handleRun(bool left, bool right)
    {
        if (left) {
            _vel.x -= _vel.x > 0 ? RUN_DECEL : RUN_ACCEL;
            if (_vel.x < -MAX_RUN) _vel.x = -MAX_RUN;
            _faceRight = false;
        }
        else if (right) {
            _vel.x += _vel.x < 0 ? RUN_DECEL : RUN_ACCEL;
            if (_vel.x > MAX_RUN) _vel.x = MAX_RUN;
            _faceRight = true;
        } else {
            var decel = _snap.enabled ? RUN_DECEL : AIR_DECEL;
            if (_vel.x > decel) {
                _vel.x -= decel;
            } else if (_vel.x < -decel) {
                _vel.x += decel;
            } else {
                _vel.x = 0;
            }
        }
    }

    void handleStampede()
    {
        _vel.x = _faceRight ? STAMPEDE_SPEED : -STAMPEDE_SPEED;
    }

    void handleVertMovement()
    {
        _vel.y -= GRAVITY;
        if (Mathf.Abs(_vel.y) > MAX_FALL) {
            _vel.y = (GRAVITY < 0) ? MAX_FALL : -MAX_FALL;
        }
    }
}
