using UnityEngine;

static public class GameObjectExt
{
    static public T EnsureComponent<T> (this GameObject go) where T : Component
    {
        var getComp = go.GetComponent<T>();
        if (getComp != null) return getComp;
        return go.AddComponent<T>();
    }
}
