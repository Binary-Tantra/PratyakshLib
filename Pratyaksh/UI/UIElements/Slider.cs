using System.Numerics;
using Raylib_cs;
using Pratyaksh.Core;
using MouseButton = Pratyaksh.Core.MouseButton;

namespace Pratyaksh.UI.UIElements;

public class Slider : UIBase, IPointerInteractable, IDragable
{
    private float minValue;
    private float maxValue;
    private float value;
    private float? step;

    private bool showValue;
    private string? format;
    private int fontSize;
    private object? payload;

    private bool isDragging;

    private Action<Slider>? onValueChanged;

    private Color? customTrackColor;
    private Color? customFillColor;
    private Color? customThumbColor;
    private Color? customThumbBorderColor;
    private Color? customTextColor;

    public float MinValue
    {
        get => minValue;
        set
        {
            minValue = value;
            if (this.value < minValue) this.value = minValue;
        }
    }

    public float MaxValue
    {
        get => maxValue;
        set
        {
            maxValue = value;
            if (this.value > maxValue) this.value = maxValue;
        }
    }

    public float Value
    {
        get => value;
        set => SetValue(value);
    }

    public float? Step
    {
        get => step;
        set => step = value;
    }

    public bool ShowValue
    {
        get => showValue;
        set => showValue = value;
    }

    public string? Format
    {
        get => format;
        set => format = value;
    }

    public int FontSize
    {
        get => fontSize;
        set => fontSize = value;
    }

    public object? Payload => payload;
    public bool IsDragging => isDragging;

    public float NormalizedValue => (maxValue > minValue) ? Math.Clamp((value - minValue) / (maxValue - minValue), 0f, 1f) : 0f;

    public Color? TrackColor { get => customTrackColor; set => customTrackColor = value; }
    public Color? FillColor { get => customFillColor; set => customFillColor = value; }
    public Color? ThumbColor { get => customThumbColor; set => customThumbColor = value; }
    public Color? ThumbBorderColor { get => customThumbBorderColor; set => customThumbBorderColor = value; }
    public Color? TextColor { get => customTextColor; set => customTextColor = value; }

    public Slider(int posX, int posY, float value, float minValue, float maxValue, int width, int height, Action<Slider>? onValueChanged, object? payload = null, bool showValue = true, string? format = null, int fontSize = 13, float? step = null, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, width, height, parent, parentBasis)
    {
        selfInteractable = true;

        this.minValue = minValue;
        this.maxValue = maxValue >= minValue ? maxValue : minValue;
        this.step = step;
        this.showValue = showValue;
        this.format = format;
        this.fontSize = fontSize;
        this.payload = payload;
        this.onValueChanged = onValueChanged;
        this.isDragging = false;

        this.value = Math.Clamp(value, this.minValue, this.maxValue);
        if (this.step.HasValue && this.step.Value > 0f)
        {
            this.value = MathF.Round((this.value - this.minValue) / this.step.Value) * this.step.Value + this.minValue;
            this.value = Math.Clamp(this.value, this.minValue, this.maxValue);
        }
    }

    public void SetValue(float val)
    {
        float clamped = Math.Clamp(val, minValue, maxValue);
        if (step.HasValue && step.Value > 0f)
        {
            clamped = MathF.Round((clamped - minValue) / step.Value) * step.Value + minValue;
            clamped = Math.Clamp(clamped, minValue, maxValue);
        }

        if (Math.Abs(value - clamped) > 0.00001f)
        {
            value = clamped;
            onValueChanged?.Invoke(this);
        }
    }

    public void SetValueWithoutNotify(float val)
    {
        float clamped = Math.Clamp(val, minValue, maxValue);
        if (step.HasValue && step.Value > 0f)
        {
            clamped = MathF.Round((clamped - minValue) / step.Value) * step.Value + minValue;
            clamped = Math.Clamp(clamped, minValue, maxValue);
        }
        value = clamped;
    }

    public void SetRange(float min, float max)
    {
        minValue = min;
        maxValue = max >= min ? max : min;
        SetValue(value);
    }

    public void SetOnValueChanged(Action<Slider>? callback)
    {
        onValueChanged = callback;
    }

    private void UpdateValueFromPosition(Vector2 screenPos)
    {
        int thumbRadius = Math.Max(4, Height / 2 - 2);
        float trackLeft = Position.X + thumbRadius;
        float trackRight = Position.X + Width - thumbRadius;
        float trackWidth = trackRight - trackLeft;

        float t;
        if (trackWidth > 0f)
            t = Math.Clamp((screenPos.X - trackLeft) / trackWidth, 0f, 1f);
        else
            t = 0f;

        float calculated = minValue + t * (maxValue - minValue);
        SetValue(calculated);
    }

    protected override void OnDraw()
    {
        int thumbRadius = Math.Max(4, Height / 2 - 2);
        int trackH = Math.Max(4, Height / 3);
        int trackY = (int)(Position.Y + (Height - trackH) / 2);

        // Palette
        Color trackBgNormal = customTrackColor ?? new Color((byte)42, (byte)42, (byte)42, (byte)255);
        Color trackBgHover = customTrackColor.HasValue
            ? new Color((byte)Math.Min(255, trackBgNormal.R + 20), (byte)Math.Min(255, trackBgNormal.G + 20), (byte)Math.Min(255, trackBgNormal.B + 20), trackBgNormal.A)
            : new Color((byte)55, (byte)55, (byte)55, (byte)255);

        Color trackBorder = new Color((byte)75, (byte)75, (byte)75, (byte)255);

        Color fillNormal = customFillColor ?? new Color((byte)28, (byte)88, (byte)175, (byte)255);
        Color fillHover = customFillColor.HasValue
            ? new Color((byte)Math.Min(255, fillNormal.R + 25), (byte)Math.Min(255, fillNormal.G + 25), (byte)Math.Min(255, fillNormal.B + 25), fillNormal.A)
            : new Color((byte)40, (byte)105, (byte)195, (byte)255);
        Color fillDrag = customFillColor.HasValue
            ? new Color((byte)Math.Min(255, fillNormal.R + 40), (byte)Math.Min(255, fillNormal.G + 40), (byte)Math.Min(255, fillNormal.B + 40), fillNormal.A)
            : new Color((byte)48, (byte)125, (byte)225, (byte)255);

        Color currentFill = isDragging ? fillDrag : (hovered ? fillHover : fillNormal);
        Color currentTrackBg = (hovered || isDragging) ? trackBgHover : trackBgNormal;

        // 1. Draw Track Background
        Raylib_cs.Rectangle trackRec = new(Position.X, trackY, Width, trackH);
        Raylib.DrawRectangleRounded(trackRec, 0.8f, 4, currentTrackBg);
        Raylib.DrawRectangleRoundedLinesEx(trackRec, 0.8f, 4, 1f, trackBorder);

        // 2. Draw Active/Filled Track
        float t = NormalizedValue;
        float trackLeft = Position.X + thumbRadius;
        float trackRight = Position.X + Width - thumbRadius;
        float thumbCX = trackLeft + t * (trackRight - trackLeft);

        float activeW = Math.Max(0, thumbCX - Position.X);
        if (activeW > 2)
        {
            Raylib_cs.Rectangle activeRec = new(Position.X, trackY, activeW, trackH);
            Raylib.DrawRectangleRounded(activeRec, 0.8f, 4, currentFill);
        }

        // 3. Draw Thumb / Knob
        Color thumbNormal = customThumbColor ?? new Color((byte)215, (byte)215, (byte)215, (byte)255);
        Color thumbHover = customThumbColor.HasValue
            ? new Color((byte)Math.Min(255, thumbNormal.R + 30), (byte)Math.Min(255, thumbNormal.G + 30), (byte)Math.Min(255, thumbNormal.B + 30), thumbNormal.A)
            : new Color((byte)245, (byte)245, (byte)245, (byte)255);
        Color thumbDrag = Color.White;

        Color currentThumb = isDragging ? thumbDrag : (hovered ? thumbHover : thumbNormal);

        Color thumbBorder = customThumbBorderColor ?? (isDragging ? new Color((byte)60, (byte)140, (byte)230, (byte)255) : (hovered ? new Color((byte)100, (byte)150, (byte)220, (byte)255) : new Color((byte)90, (byte)90, (byte)90, (byte)255)));

        int knobCY = (int)(Position.Y + Height / 2);
        Raylib.DrawCircle((int)thumbCX, knobCY, thumbRadius, currentThumb);
        Raylib.DrawCircleLines((int)thumbCX, knobCY, thumbRadius, thumbBorder);

        // 4. Draw Formatted Value Text if enabled
        if (showValue)
        {
            string valStr;
            if (string.IsNullOrEmpty(format))
            {
                valStr = value.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (format.Contains('{'))
            {
                valStr = string.Format(System.Globalization.CultureInfo.InvariantCulture, format, value);
            }
            else
            {
                valStr = value.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
            }

            int textW = LayoutEngine.MeasureTextW(valStr, fontSize);
            int textX = (int)(Position.X + (Width - textW) / 2f);
            int textY = (int)(Position.Y + (Height - fontSize) / 2f);

            Color textCol = customTextColor ?? new Color((byte)225, (byte)225, (byte)225, (byte)255);
            // Draw text with subtle shadow for crisp readability over any background
            LayoutEngine.DrawTextAbsolute(valStr, textX + 1, textY + 1, new Color((byte)10, (byte)10, (byte)10, (byte)180), fontSize, Vector2.Zero);
            LayoutEngine.DrawTextAbsolute(valStr, textX, textY, textCol, fontSize, Vector2.Zero);
        }
    }

    private Vector2 GetHitPosition(PointerInteractEventData evt)
    {
        bool worldSpace = InteractionUseWorldPos() || CheckAncestorsForInteractWorldPos();
        return worldSpace ? evt.WorldPosition : evt.ScreenPosition;
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left) return false;

        isDragging = true;
        Engine.Instance.InteractionManager.CapturePointer(this);
        UpdateValueFromPosition(GetHitPosition(evt));
        return true;
    }

    public bool OnDragStart(PointerInteractEventData evt)
    {
        return isDragging;
    }

    public void OnDrag(PointerInteractEventData evt)
    {
        if (isDragging)
        {
            UpdateValueFromPosition(GetHitPosition(evt));
        }
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (isDragging && evt.MouseButton == MouseButton.Left)
        {
            UpdateValueFromPosition(GetHitPosition(evt));
            isDragging = false;
            Engine.Instance.InteractionManager.ReleasePointer();
            return true;
        }

        return false;
    }
}
