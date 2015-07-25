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
    const KeyCode RETRY = KeyCode.Backspace;
    const KeyCode TRICK = KeyCode.Q;

    ControlState _left;
    ControlState _right;
    ControlState _act;
    ControlState _swap;
    ControlState _trick;
    ControlState _retry;

    const ControlState EMPTY = ControlState.Up;

    public bool Enabled = true;

    public ControlState Left  { get { return Enabled ? _left  : EMPTY ; } }
    public ControlState Right { get { return Enabled ? _right : EMPTY ; } }
    public ControlState Act   { get { return Enabled ? _act   : EMPTY ; } }
    public ControlState Swap  { get { return Enabled ? _swap  : EMPTY ; } }
    public ControlState Trick { get { return Enabled ? _trick : EMPTY ; } }
    //public ControlState Trick { get { return EMPTY; }}
    public ControlState Retry { get { return _retry; } }

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
        updateStateFromSignal(ref _retry, Input.GetKey(RETRY));
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
