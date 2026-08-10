using Pratyaksh.Core;

namespace Pratyaksh.UI.UIElements;

public class Toggle : UIBase, IPointerInteractable
{
    private bool isOn;
    private float knobT; // interpolating knob position for sliding anim: 0 = off, 1 = on, 0.5 = middle, etc.
    private object payload;

    private Action<Toggle>? onToggleChanged;

    public bool IsOn => isOn;
    public object Payload => payload;

    public bool Value
    {
        get => isOn;
        set
        {
            isOn = value;
            knobT = value ? 1f : 0f;
        }
    }

    public Toggle(int posX, int posY, bool toggleValue, int trackWidth, int trackHeight, Action<Toggle>? onToggleChanged, object? payload, int fontSize = 15, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, trackWidth, trackHeight, parent, parentBasis)
    {
        selfInteractable = true;

        isOn = toggleValue;
        knobT = toggleValue ? 1f : 0f;

        this.payload = payload;
        this.onToggleChanged = onToggleChanged;
    }

    protected override void OnUpdate()
    {
        // Lerp knob toward the target side each frame (1 second to settle)
        float target = isOn ? 1f : 0f;
        float error = target - knobT;
        float absoluteError = Math.Abs(error);

        if (absoluteError > 0.01)
        {
            if (absoluteError < 10 * Engine.Instance.DeltaTime)
            {
                knobT = target;
            }
            else
            {
                float normal = error / Math.Abs(error);
                float delta = normal * 10 * Engine.Instance.DeltaTime;

                knobT += delta;
            }
        }
    }

    protected override void OnDraw()
    {
        // Palette
        Raylib_cs.Color trackOff = new((byte)52, (byte)52, (byte)52, (byte)255);
        Raylib_cs.Color trackOffHover = new((byte)68, (byte)68, (byte)68, (byte)255);
        Raylib_cs.Color trackOn = new((byte)28, (byte)88, (byte)175, (byte)255);
        Raylib_cs.Color trackOnHover = new((byte)40, (byte)105, (byte)195, (byte)255);
        Raylib_cs.Color borderOff = new((byte)80, (byte)80, (byte)80, (byte)255);
        Raylib_cs.Color borderOn = new((byte)50, (byte)115, (byte)205, (byte)255);
        Raylib_cs.Color knobColor = new((byte)220, (byte)220, (byte)220, (byte)255);
        Raylib_cs.Color labelColor = new((byte)185, (byte)185, (byte)185, (byte)255);

        int tx = (int)Position.X;
        int ty = (int)Position.Y;

        // Track
        Raylib_cs.Color trackFill = isOn ? (hovered ? trackOnHover : trackOn)
                                          : (hovered ? trackOffHover : trackOff);
        Raylib_cs.Color trackBorder = isOn ? borderOn : borderOff;

        var trackRect = new Raylib_cs.Rectangle(tx, ty, Width, Height);
        Raylib_cs.Raylib.DrawRectangleRounded(trackRect, 1.0f, 8, trackFill);
        Raylib_cs.Raylib.DrawRectangleRoundedLinesEx(trackRect, 1.0f, 8, 1f, trackBorder);

        // Knob
        int knobRadius = Height / 2 - 3;
        int knobMinCX = tx + 3 + knobRadius;            // fully-left centre
        int knobMaxCX = tx + Width - 3 - knobRadius;    // fully-right centre
        int knobCX = (int)(knobMinCX + (knobMaxCX - knobMinCX) * knobT);
        int knobCY = ty + Height / 2;

        Raylib_cs.Raylib.DrawCircle(knobCX, knobCY, knobRadius, knobColor);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left)
            return false;

        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left)
            return false;

        isOn = !isOn;
        onToggleChanged?.Invoke(this);

        return true;
    }

    public void SetValue(bool value)
    {
        isOn = value;
    }

    public void SetIsOnWithoutNotify(bool value)
    {
        isOn = value;
        knobT = value ? 1f : 0f;
    }

    public void SetOnToggleChanged(Action<Toggle>? onToggleChanged)
    {
        this.onToggleChanged = onToggleChanged;
    }
}