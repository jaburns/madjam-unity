using System;

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
    }
}
