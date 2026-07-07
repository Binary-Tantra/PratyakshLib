using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public class Toggle : UIBase, IPointerInteractable
{
    private bool isOn;
    private float knobT; // interpolating knob position for sliding anim: 0 = off, 1 = on, 0.5 = middle, etc.
    private int trackWidth;
    private int trackHeight;
    private string label;
    private int fontSize;
    private object payload;

    private Action<Toggle> onToggleChanged;

    public bool IsOn => isOn;
    public object Payload => payload;

    public Toggle(bool startingValue, string label, int trackWidth, int trackHeight, Action<Toggle> onToggleChanged, object payload, int fontSize = 15, Drawable? parent = null) : base(parent)
    {
        selfInteractable = true;

        isOn = startingValue;
        knobT = startingValue ? 1f : 0f;
        this.label = label;

        this.trackWidth = trackWidth;
        this.trackHeight = trackHeight;
        this.fontSize = fontSize;
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
            if (absoluteError < 10 * Engine.DeltaTime)
            {
                knobT = target;
            }
            else
            {
                float normal = error / Math.Abs(error);
                float delta = normal * 10 * Engine.DeltaTime;

                knobT += delta;
            }
        }
    }

    protected override void OnDraw()
    {
        // Palette
        Color trackOff = new((byte)52, (byte)52, (byte)52, (byte)255);
        Color trackOffHover = new((byte)68, (byte)68, (byte)68, (byte)255);
        Color trackOn = new((byte)28, (byte)88, (byte)175, (byte)255);
        Color trackOnHover = new((byte)40, (byte)105, (byte)195, (byte)255);
        Color borderOff = new((byte)80, (byte)80, (byte)80, (byte)255);
        Color borderOn = new((byte)50, (byte)115, (byte)205, (byte)255);
        Color knobColor = new((byte)220, (byte)220, (byte)220, (byte)255);
        Color labelColor = new((byte)185, (byte)185, (byte)185, (byte)255);

        int tx = (int)Position.X;
        int ty = (int)Position.Y;

        // Track
        Color trackFill = isOn ? (hovered ? trackOnHover : trackOn)
                                 : (hovered ? trackOffHover : trackOff);
        Color trackBorder = isOn ? borderOn : borderOff;

        var trackRect = new Rectangle(tx, ty, trackWidth, trackHeight);
        Raylib.DrawRectangleRounded(trackRect, 1.0f, 8, trackFill);
        Raylib.DrawRectangleRoundedLinesEx(trackRect, 1.0f, 8, 1f, trackBorder);

        // Knob
        int knobRadius = trackHeight / 2 - 3;
        int knobMinCX = tx + 3 + knobRadius;                    // fully-left centre
        int knobMaxCX = tx + trackWidth - 3 - knobRadius;       // fully-right centre
        int knobCX = (int)(knobMinCX + (knobMaxCX - knobMinCX) * knobT);
        int knobCY = ty + trackHeight / 2;

        Raylib.DrawCircle(knobCX, knobCY, knobRadius, knobColor);

        // Label
        if (!string.IsNullOrEmpty(label))
        {
            int textY = ty + (trackHeight - fontSize) / 2;
            Raylib.DrawText(label, tx + trackWidth + 8, textY, fontSize, labelColor);
        }
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, trackWidth, trackHeight);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        isOn = !isOn;
        onToggleChanged?.Invoke(this);

        return true;
    }

    public void SetValue(bool value)
    {
        isOn = value;
    }
}