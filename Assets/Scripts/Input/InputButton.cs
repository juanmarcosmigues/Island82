using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class InputButton
{
    public string name;
    public bool enabled;
    public System.Action onPressedDown;
    public System.Action<float> onHolding, onRelease;
    public EnabledCondition outsideEnabled;

    public delegate bool EnabledCondition();

    public float buttonTimePressed => pressed ? Time.time - timePressed : 0f;
    public int framesPressed => pressed ? Time.frameCount - framePressed : 0;
    public bool buttonIsPressed => pressed;
    public InputAction inputAction { get; protected set; }

    protected bool pressed;
    protected float timePressed;
    protected int framePressed;
    protected float timeReleased;
    protected int frameReleased;

    public InputButton (string name, InputAction inputAction)
    {
        this.name = name;
        this.inputAction = inputAction;
    }

    public virtual void Initialize ()
    {
        inputAction.started += c => ButtonPressed();
        inputAction.canceled += c => ButtonReleased();
        enabled = true;
    }

    public virtual void Update ()
    {
        if (!enabled || !outsideEnabled())
        {
            pressed = false;
            return;
        }

        if (pressed && framePressed < Time.frameCount)
        {
            onHolding?.Invoke(Time.time - timePressed);
        }
    }

    protected virtual void ButtonPressed ()
    {
        if (!enabled || !outsideEnabled()) return;

        pressed = true;
        timePressed = Time.time;
        framePressed = Time.frameCount;

        onPressedDown?.Invoke();
    }
    protected virtual void ButtonReleased ()
    {
        if (!enabled || !outsideEnabled()) return;

        pressed = false;
        timeReleased = Time.time;
        frameReleased = Time.frameCount;

        onRelease?.Invoke(Time.time - timePressed);    
    }
}

[System.Serializable]
public class InputAxisAsButton : InputButton
{
    public enum Direction
    {
        None = -1,
        East = 0, NorthEast = 1, North = 2, NorthWest = 3,
        West = 4, SouthWest = 5, South = 6, SouthEast = 7
    }

    public float threshold = 0.5f;          // magnitude required to "press"
    public float initialRepeatDelay = 0.5f;   // time held before auto-repeat starts
    public float repeatInterval = 0.4f;     // interval between auto-repeats

    public Vector2 axisValue { get; private set; }
    public Direction currentDirection { get; private set; } = Direction.None;

    private float nextRepeatTime;

    public InputAxisAsButton(string name, InputAction inputAction)
        : base(name, inputAction) { }

    public override void Initialize()
    {
        // Intentionally do NOT subscribe to started/canceled.
        // For a Vector2 action those fire on any value change away from / back to zero,
        // which doesn't map cleanly to "8-direction button" semantics.
        enabled = true;
    }

    public override void Update()
    {
        bool gateOpen = enabled && (outsideEnabled == null || outsideEnabled());
        if (!gateOpen)
        {
            if (pressed) DoRelease();
            return;
        }

        axisValue = inputAction.ReadValue<Vector2>();
        bool active = axisValue.magnitude >= threshold;
        Direction newDir = active ? GetOctant(axisValue) : Direction.None;

        if (newDir != currentDirection)
        {
            // Either we left the deadzone, entered it, or rotated into a new octant.
            if (pressed) DoRelease();
            if (newDir != Direction.None)
            {
                currentDirection = newDir;
                DoPress();
                nextRepeatTime = Time.time + initialRepeatDelay;
            }
        }
        else if (pressed)
        {
            // Same direction held — handle auto-repeat.
            if (Time.time >= nextRepeatTime)
            {
                onPressedDown?.Invoke();
                nextRepeatTime = Time.time + repeatInterval;
            }
            if (framePressed < Time.frameCount)
                onHolding?.Invoke(Time.time - timePressed);
        }
    }

    private void DoPress()
    {
        pressed = true;
        timePressed = Time.time;
        framePressed = Time.frameCount;
        onPressedDown?.Invoke();
    }

    private void DoRelease()
    {
        pressed = false;
        timeReleased = Time.time;
        frameReleased = Time.frameCount;
        onRelease?.Invoke(Time.time - timePressed);
        currentDirection = Direction.None;
    }

    private static Direction GetOctant(Vector2 axis)
    {
        float angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg;
        if (angle < 0f) angle += 360f;
        // Sectors of 45° centered on each cardinal/diagonal:
        // 0°=E, 45°=NE, 90°=N, 135°=NW, 180°=W, 225°=SW, 270°=S, 315°=SE
        int octant = Mathf.RoundToInt(angle / 45f) % 8;
        return (Direction)octant;
    }
}
