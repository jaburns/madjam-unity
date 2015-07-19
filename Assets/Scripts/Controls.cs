using UnityEngine;

public class Controls : MonoBehaviour
{
    static Controls s_instance;

    static public Controls Instance {
        get {
            if (s_instance == null) {
                var go = new GameObject();
                s_instance = go.AddComponent<Controls>();
                go.name = "Controls";
            }
            return s_instance;
        }
    }

    public enum ControlState {
        Up, Press, Hold, Release, Trick
    }

    const KeyCode LEFT  = KeyCode.LeftArrow;
    const KeyCode RIGHT = KeyCode.RightArrow;
    const KeyCode ACT   = KeyCode.Space;
    const KeyCode SWAP  = KeyCode.LeftShift;
    const KeyCode TRICK = KeyCode.Q;

    ControlState _left;
    ControlState _right;
    ControlState _act;
    ControlState _swap;
    ControlState _trick;

    public ControlState Left  { get { return _left; } }
    public ControlState Right { get { return _right; } }
    public ControlState Act   { get { return _act; } }
    public ControlState Swap  { get { return _swap; } }
    public ControlState Trick { get { return _trick; } }

    static public bool IsDown(ControlState state)
    {
        return state == ControlState.Press || state == ControlState.Hold;
    }

    static public bool IsUp(ControlState state)
    {
        return state == ControlState.Release || state == ControlState.Up;
    }

    void FixedUpdate()
    {
        updateStateFromSignal(ref _left,  Input.GetKey(LEFT ));
        updateStateFromSignal(ref _right, Input.GetKey(RIGHT));
        updateStateFromSignal(ref _act,   Input.GetKey(ACT  ));
        updateStateFromSignal(ref _swap,  Input.GetKey(SWAP ));
        updateStateFromSignal(ref _trick, Input.GetKey(TRICK));
    }

    static void updateStateFromSignal(ref ControlState state, bool signal)
    {
        if (signal) {
            switch (state) {
                case ControlState.Up:      state = ControlState.Press; break;
                case ControlState.Press:   state = ControlState.Hold;  break;
                case ControlState.Release: state = ControlState.Press; break;
            }
        } else {
            switch (state) {
                case ControlState.Press:   state = ControlState.Release; break;
                case ControlState.Hold:    state = ControlState.Release; break;
                case ControlState.Release: state = ControlState.Up;      break;
            }
        }
    }
}
