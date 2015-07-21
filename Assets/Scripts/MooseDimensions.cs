using UnityEngine;

public class MooseDimensions : MonoBehaviour
{
    public Color GizmoColor = Color.red;

    public float HalfWidth;
    public float HalfHeight;
    public float InsetX;
    public float InsetY;

    public float Width  { get { return 2 * HalfWidth;  } }
    public float Height { get { return 2 * HalfHeight; } }

    void OnDrawGizmos ()
    {
        Gizmos.color = GizmoColor;

        var o = transform.position + HalfHeight * Vector3.up;

        Gizmos.DrawLine(o + HalfWidth*Vector3.right, o - HalfWidth*Vector3.right);
        Gizmos.DrawLine(
            o + HalfWidth*Vector3.right + (HalfHeight - InsetY)*Vector3.up,
            o - HalfWidth*Vector3.right + (HalfHeight - InsetY)*Vector3.up
        );
        Gizmos.DrawLine(
            o + HalfWidth*Vector3.right - (HalfHeight - InsetY)*Vector3.up,
            o - HalfWidth*Vector3.right - (HalfHeight - InsetY)*Vector3.up
        );

        Gizmos.DrawLine(o + HalfHeight*Vector3.up, o - HalfHeight*Vector3.up);
        Gizmos.DrawLine(
            o + HalfHeight*Vector3.up + (HalfWidth - InsetX)*Vector3.right,
            o - HalfHeight*Vector3.up + (HalfWidth - InsetX)*Vector3.right
        );
        Gizmos.DrawLine(
            o + HalfHeight*Vector3.up - (HalfWidth - InsetX)*Vector3.right,
            o - HalfHeight*Vector3.up - (HalfWidth - InsetX)*Vector3.right
        );

        Gizmos.DrawLine(
            o + new Vector3(-HalfWidth,  HalfHeight),
            o + new Vector3( HalfWidth,  HalfHeight)
        );
        Gizmos.DrawLine(
            o + new Vector3( HalfWidth,  HalfHeight),
            o + new Vector3( HalfWidth, -HalfHeight)
        );
        Gizmos.DrawLine(
            o + new Vector3( HalfWidth, -HalfHeight),
            o + new Vector3(-HalfWidth, -HalfHeight)
        );
        Gizmos.DrawLine(
            o + new Vector3(-HalfWidth, -HalfHeight),
            o + new Vector3(-HalfWidth,  HalfHeight)
        );
    }
}
