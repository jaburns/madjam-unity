using UnityEngine;

static public class DoubleLineCast
{
    public struct Result
    {
        public bool embedded;
        public Vector2 point;
        public Vector2 normal;
        public float depth;
        public Collider2D collider;
        public Rigidbody2D rigidbody;

        public Result(bool embedded, Vector2 depthPoint, RaycastHit2D hit)
        {
            this.embedded = embedded;
            this.depth = (hit.point - depthPoint).magnitude;
            this.normal = hit.normal;
            this.point = hit.point;
            this.collider = hit.collider;
            this.rigidbody = hit.rigidbody;
        }
    }

    static public Result? Cast(Vector2 p0, Vector2 p1, int layerMask)
    {
        var hits = Physics2D.LinecastAll(p0, p1, layerMask);
        if (hits.Length < 1) return null;
        var hit = hits[0];

        // If the linecast is originating inside of a wall, then hit.point will be equal to p0.

        if (!p0.VeryNear(hit.point)) {
            return new Result(false, p1, hit);
        }

        // Redo the cast from the other direction if we had started from inside a wall last time.

        hit = Physics2D.LinecastAll(p1, p0, layerMask)[0];

        return new Result(p1.VeryNear(hit.point), p0, hit);
    }
}
