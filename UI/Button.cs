using Raylib_cs;
using System.Numerics;

namespace RaylibNodeLibrary.UI;

public class Button : UIBase, IPointerInteractable
{
    private string buttonText;
    
    private int fontSize;
    private bool hasBorder;

    private bool pressed;

    private object payload;

    private Action<Button> onButtonPressed;

    public string ButtonText 
    {
        get => buttonText; 
        set => buttonText = value; 
    }

    public object Payload { get => payload; }

    private Color? customFillColor;
    private Color? customBorderColor;
    private Color? customTextColor;

    public Color? FillColor { get => customFillColor; set => customFillColor = value; }
    public Color? BorderColor { get => customBorderColor; set => customBorderColor = value; }
    public Color? TextColor { get => customTextColor; set => customTextColor = value; }

    public Button(int posX, int posY, int width, int height, string buttonText, Action<Button> onButtonPressed, object payload, int fontSize = 15, bool hasBorder = true, Color? fillColor = null, Color? borderColor = null, Color? textColor = null, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, width, height, parent, parentBasis)
    {
        selfInteractable = true;

        float sizeX = Raylib.MeasureText(buttonText, fontSize);
        if (width > sizeX) sizeX = width;

        float sizeY = fontSize + 5;
        if (height > sizeY) sizeY = height;

        Size = new Vector2(sizeX, sizeY);

        this.onButtonPressed = onButtonPressed;
        this.payload = payload;

        this.buttonText = buttonText;
        this.fontSize = fontSize;
        this.hasBorder = hasBorder;
        this.customFillColor = fillColor;
        this.customBorderColor = borderColor;
        this.customTextColor = textColor;
    }

    protected override void OnDraw()
    {
        // Default Base Colors
        Color baseFill = customFillColor ?? new Color((byte)48, (byte)48, (byte)48, (byte)255);
        
        Color fillNormal = baseFill;
        Color fillHover = customFillColor.HasValue
            ? new Color((byte)Math.Min(255, baseFill.R + 30), (byte)Math.Min(255, baseFill.G + 30), (byte)Math.Min(255, baseFill.B + 30), baseFill.A)
            : new Color((byte)64, (byte)64, (byte)64, (byte)255);
        Color fillPressed = customFillColor.HasValue
            ? new Color((byte)Math.Max(0, baseFill.R - 20), (byte)Math.Max(0, baseFill.G - 20), (byte)Math.Max(0, baseFill.B - 20), baseFill.A)
            : new Color((byte)30, (byte)30, (byte)30, (byte)255);

        Color borderNorm = customBorderColor ?? new Color((byte)75, (byte)75, (byte)75, (byte)255);
        Color borderHover = customBorderColor.HasValue
            ? new Color((byte)Math.Min(255, borderNorm.R + 30), (byte)Math.Min(255, borderNorm.G + 30), (byte)Math.Min(255, borderNorm.B + 30), borderNorm.A)
            : new Color((byte)108, (byte)108, (byte)108, (byte)255);

        Color labelColor = customTextColor ?? new Color((byte)200, (byte)200, (byte)200, (byte)255);

        // Fill
        Color currentFill = pressed ? fillPressed : (hovered ? fillHover : fillNormal);
        Color currentBorder = hovered ? borderHover : borderNorm;

        // BG
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y, currentFill);

        // Border
        if (hasBorder)
            Raylib.DrawRectangleLinesEx(new Rectangle(Position.X, Position.Y, Size.X, Size.Y), 1f, currentBorder);

        // Text (centered)
        int textW = Raylib.MeasureText(buttonText, fontSize);
        int textX = (int)(Position.X + (Size.X - textW) / 2f);
        int textY = (int)(Position.Y + (Size.Y - fontSize) / 2f);

        LayoutEngine.DrawTextAbsolute(buttonText, textX, textY, labelColor, fontSize, Vector2.Zero);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        pressed = true;
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        onButtonPressed?.Invoke(this);
        pressed = false;
        return true;
    }
}
