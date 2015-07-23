using System;
using UnityEngine;

static public class GravitySetting
{
    static bool s_reverseGravity;

    static public event Action OnGravitySwitch;
    static public bool Reverse { get { return s_reverseGravity; } }

    static public void Reset()
    {
        s_reverseGravity = false;
        OnGravitySwitch = null;
    }

    static public void SwitchGravity()
    {
        s_reverseGravity = !s_reverseGravity;
        if (OnGravitySwitch != null) OnGravitySwitch();

        Physics2D.gravity = s_reverseGravity
            ? new Vector2(0,  Mathf.Abs(Physics2D.gravity.y))
            : new Vector2(0, -Mathf.Abs(Physics2D.gravity.y));
    }
}
