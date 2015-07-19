using UnityEngine;

public class BlobController : MonoBehaviour
{
    public float BindRange;

    BlobBinder _binder;

    void Awake()
    {
        GravitySetting.Reset();
    }

    void FixedUpdate()
    {
        if (_binder == null) {
            bindTo(getClosestBinder());
        }
        if (Controls.Instance.Swap == Controls.ControlState.Press) {
            bindTo(getClosestBinder());
            GravitySetting.SwitchGravity();
        }
    }

    BlobBinder getClosestBinder()
    {
        float closestDist = float.MaxValue;
        BlobBinder closestBinder = null;

        foreach (var binder in FindObjectsOfType<BlobBinder>()) {
            if (binder.HasBlob) continue;
            var d2 = (binder.transform.position.WithZ(0) - transform.position.WithZ(0)).sqrMagnitude;
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
        if (_binder) _binder.HasBlob = false;

        _binder = binder;
        _binder.HasBlob = true;
        transform.position = _binder.transform.position;
        transform.parent = _binder.transform;
    }
}
