using UnityEngine;

public class BlobController : MonoBehaviour
{
    static public Vector3? s_checkedPoint;

    public float BindRange;

    float _t;
    bool _globbing;

    BlobBinder _binder;
    BlobBinder _oldBinder;

    void Awake()
    {
        GravitySetting.Reset();
    }

    void FixedUpdate()
    {
        if (_binder == null) {
            if (s_checkedPoint.HasValue) {
                transform.position = s_checkedPoint.Value;
            }
            bindTo(getClosestBinder());
            var cam = FindObjectOfType<CameraController>();
            cam.transform.position = transform.position.WithZ(cam.transform.position.z);
        }
        if (Controls.Instance.Swap == Controls.ControlState.Press) {
            bindTo(getClosestBinder());
        }

        if (Controls.Instance.Trick == Controls.ControlState.Press) {
            GravitySetting.SwitchGravity();
        }
    }

    void LateUpdate()
    {
        if (!_globbing) {
            if (_binder) transform.position = _binder.Target.transform.position;
            return;
        }
        _t += Time.deltaTime * 2;

        if (_oldBinder == null || _t >= 1) {
            _binder.HasBlob = true;
            transform.position = _binder.Target.transform.position;
            _globbing = false;
            _oldBinder = null;
        } else {
            var t = damp(_t);
            transform.position = _oldBinder.Target.transform.position + (_binder.Target.transform.position - _oldBinder.Target.transform.position) * t;
        }
    }

    static float damp(float t)
    {
        if (t < 0) t = 0;
        else if (t > 1) t = 1;
        return 0.5f - 0.5f*Mathf.Cos(Mathf.PI*t);
    }

    BlobBinder getClosestBinder()
    {
        float closestDist = float.MaxValue;
        BlobBinder closestBinder = null;

        foreach (var binder in FindObjectsOfType<BlobBinder>()) {
            if (binder.HasBlob) continue;
            var d2 = (binder.Target.transform.position.WithZ(0) - transform.position.WithZ(0)).sqrMagnitude;
            if (d2 < closestDist) {
                closestDist = d2;
                closestBinder = binder;
            }
        }

        return closestDist < BindRange*BindRange ? closestBinder : null;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, BindRange);
    }

    void bindTo(BlobBinder binder)
    {
        if (binder == null) return;
        if (_binder) {
            _binder.HasBlob = false;
            _oldBinder = _binder;
        }

        _binder = binder;
        binder.BlobRef = this;
        _globbing = true;
        _t = 0;

        if (MusicController.Instance) {
            MusicController.Instance.SetMusic(_binder.MusicIndex);
        }
    }
}
