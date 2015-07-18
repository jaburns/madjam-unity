using UnityEngine;

static public class CameraExt
{
    static public Vector2 OrthoSize(this Camera camera)
    {
        return (camera.ViewportToWorldPoint(new Vector3(1, 1, 0))
              - camera.ViewportToWorldPoint(Vector3.zero)).AsVector2();
    }
}
