using UnityEngine;

static public class RectExt
{
    static public Vector2 PointOutsideDistance (this Rect rect, Vector2 pt)
    {
        float x, y;

             if (pt.x < rect.xMin) x = pt.x - rect.xMin;
        else if (pt.x > rect.xMax) x = pt.x - rect.xMax;
        else x = 0;

             if (pt.y < rect.yMin) y = pt.y - rect.yMin;
        else if (pt.y > rect.yMax) y = pt.y - rect.yMax;
        else y = 0;

        return new Vector2 (x, y);
    }
}
