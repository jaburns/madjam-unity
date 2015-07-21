using UnityEngine;

public class BirdController : MonoBehaviour
{
    float GRAVITY = 2 * MooseController.MOVEMENT_SCALE;
    const float MAX_FALL = 16 * MooseController.MOVEMENT_SCALE;

    Vector2 _newPosition;
    Vector2 _oldPosition;
    Vector2 _vel;

    BlobBinder _blobBinder;
    MooseDimensions _heroDim;

    void Awake()
    {
        _newPosition = transform.position.AsVector2();
        _oldPosition = _newPosition;

        _heroDim = GetComponent<MooseDimensions>();
        _blobBinder = GetComponentInChildren<BlobBinder>();
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

    void FixedUpdate()
    {
        _oldPosition = _newPosition;

        bool pressingLeft = false;
        bool pressingRight = false;
        bool pressingAction = false;

        if (_blobBinder.HasBlob) {
            pressingLeft = Controls.IsDown(Controls.Instance.Left);
            pressingRight = Controls.IsDown(Controls.Instance.Right);
            pressingAction = Controls.IsDown(Controls.Instance.Act);
        }

        move(pressingLeft, pressingRight, pressingAction);

        freeMovement();
    }

    void GravitySwitch()
    {
        GRAVITY *= -1;
    }

    void move(bool left, bool right, bool fly)
    {
        _vel.x = left ? -0.1f : right ? 0.1f : 0;
        if (fly) {
            _vel.y = 0.1f;
        } else {
            _vel.y -= GRAVITY;
            if (Mathf.Abs(_vel.y) > MAX_FALL) {
                _vel.y = (GRAVITY < 0) ? MAX_FALL : -MAX_FALL;
            }
        }
    }

    static bool normalIsGround(Vector2 n) { return n.y >=  Mathf.Cos(Mathf.PI / 3); }
    static bool normalIsRoof(Vector2 n)   { return n.y <= -Mathf.Cos(Mathf.PI / 3); }
    static bool normalIsWall(Vector2 n)   { return !normalIsGround(n) && !normalIsRoof(n); }

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
        var maybeHit = DoubleLineCast.Cast(pt0, pt1, MooseController.CollisionLayerMask);
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
            //if (_cantSnapCounter == 0) {
            //    startSnap(hit.collider, hit.rigidbody, newPos - offset, hit.normal);
            //}
        } else if (normalIsRoof(hit.normal)) {
            if (_vel.y > 0) {
                _vel.y = 0;
            }
            newPos.y = (hit.point - Vector2.up * _heroDim.Height).y;

            var hitBody = hit.collider.GetComponent<Rigidbody2D>();
            if (hitBody && hitBody.velocity.y < 0) {
                _vel.y = hitBody.velocity.y * Time.fixedDeltaTime;
                newPos.y += _vel.y;
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

            var maybeHit = DoubleLineCast.Cast(pt0, pt1, MooseController.CollisionLayerMask);
            if (maybeHit.HasValue && normalIsWall(maybeHit.Value.normal)) {
                newPos.x = maybeHit.Value.point.x + Mathf.Sign(maybeHit.Value.normal.x) * _heroDim.HalfWidth;
                if (maybeHit.Value.normal.x * _vel.x < 0) {
                    _vel.x = 0;
                }
            }
        }

        return newPos;
    }
}
