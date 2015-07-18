using UnityEngine;

static public class Vector2Ext
{
    static public Vector2 WithX(this Vector2 v, float x) { return new Vector2(x, v.y); }
    static public Vector2 WithY(this Vector2 v, float y) { return new Vector2(v.x, y); }

    static public Vector2 Rotate(this Vector2 v, float degrees)
    {
        return Quaternion.Euler(0, 0, degrees) * v;
    }

    static public Vector3 AsVector3(this Vector2 v, float z = 0.0f)
    {
        return new Vector3(v.x, v.y, z);
    }

    static public Vector2 FromPolar(float radius, float radians)
    {
        return new Vector2 {
            x = radius * Mathf.Cos(radians),
            y = radius * Mathf.Sin(radians)
        };
    }

    static public float Dot(this Vector2 v, Vector2 v1)
    {
        return v.x*v1.x + v.y*v1.y;
    }

    static public bool VeryNear(this Vector2 v, Vector2 v1)
    {
        return (v - v1).sqrMagnitude < 1e-9;
    }
}

static public class Vector3Ext
{
    static public Vector3 WithX (this Vector3 vec, float x) { return new Vector3 (x, vec.y, vec.z); }
    static public Vector3 WithY (this Vector3 vec, float y) { return new Vector3 (vec.x, y, vec.z); }
    static public Vector3 WithZ (this Vector3 vec, float z) { return new Vector3 (vec.x, vec.y, z); }

    static public Vector3 WithXY (this Vector3 vec, float x, float y) { return new Vector3 (x, y, vec.z); }
    static public Vector3 WithYZ (this Vector3 vec, float y, float z) { return new Vector3 (vec.x, y, z); }
    static public Vector3 WithXZ (this Vector3 vec, float x, float z) { return new Vector3 (x, vec.y, z); }

    static public Vector2 AsVector2(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }
}
