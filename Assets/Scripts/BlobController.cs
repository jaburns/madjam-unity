using UnityEngine;

public class BlobController : MonoBehaviour
{
    BlobBinder _binder;

    void FixedUpdate()
    {
        if (Controls.Instance.Swap == Controls.ControlState.Press) {
            bindTo(getClosestBinder());
        }
    }

    BlobBinder getClosestBinder()
    {
        foreach (var binder in FindObjectsOfType<BlobBinder>()) {
            if (!binder.HasBlob) return binder;
        }
        return null;
    }

    void bindTo(BlobBinder binder)
    {
        if (_binder) _binder.HasBlob = false;

        _binder = binder;
        _binder.HasBlob = true;
        transform.position = _binder.transform.position;
        transform.parent = _binder.transform;
    }
}
