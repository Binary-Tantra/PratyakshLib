using System.Numerics;
using Pratyaksh.Core;

namespace Pratyaksh.UI.UIElements;

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

    private Raylib_cs.Color? customFillColor;
    private Raylib_cs.Color? customBorderColor;
    private Raylib_cs.Color? customTextColor;

    public Raylib_cs.Color? FillColor { get => customFillColor; set => customFillColor = value; }
    public Raylib_cs.Color? BorderColor { get => customBorderColor; set => customBorderColor = value; }
    public Raylib_cs.Color? TextColor { get => customTextColor; set => customTextColor = value; }

    public Button(int posX, int posY, int width, int height, string buttonText, Action<Button> onButtonPressed, object payload, int fontSize = 15, bool hasBorder = true, Raylib_cs.Color? fillColor = null, Raylib_cs.Color? borderColor = null, Raylib_cs.Color? textColor = null, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, width, height, parent, parentBasis)
    {
        selfInteractable = true;

        /*float sizeX = Raylib_cs.Raylib.MeasureText(buttonText, fontSize);
        if (width > sizeX) sizeX = width;

        float sizeY = fontSize + 5;
        if (height > sizeY) sizeY = height;

        Size = new Vector2(sizeX, sizeY);*/

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
        Raylib_cs.Color baseFill = customFillColor ?? new Raylib_cs.Color((byte)48, (byte)48, (byte)48, (byte)255);
        
        Raylib_cs.Color fillNormal = baseFill;
        Raylib_cs.Color fillHover = customFillColor.HasValue
            ? new Raylib_cs.Color((byte)Math.Min(255, baseFill.R + 30), (byte)Math.Min(255, baseFill.G + 30), (byte)Math.Min(255, baseFill.B + 30), baseFill.A)
            : new Raylib_cs.Color((byte)64, (byte)64, (byte)64, (byte)255);
        Raylib_cs.Color fillPressed = customFillColor.HasValue
            ? new Raylib_cs.Color((byte)Math.Max(0, baseFill.R - 20), (byte)Math.Max(0, baseFill.G - 20), (byte)Math.Max(0, baseFill.B - 20), baseFill.A)
            : new Raylib_cs.Color((byte)30, (byte)30, (byte)30, (byte)255);

        Raylib_cs.Color borderNorm = customBorderColor ?? new Raylib_cs.Color((byte)75, (byte)75, (byte)75, (byte)255);
        Raylib_cs.Color borderHover = customBorderColor.HasValue
            ? new Raylib_cs.Color((byte)Math.Min(255, borderNorm.R + 30), (byte)Math.Min(255, borderNorm.G + 30), (byte)Math.Min(255, borderNorm.B + 30), borderNorm.A)
            : new Raylib_cs.Color((byte)108, (byte)108, (byte)108, (byte)255);

        Raylib_cs.Color labelColor = customTextColor ?? new Raylib_cs.Color((byte)200, (byte)200, (byte)200, (byte)255);

        // Fill
        Raylib_cs.Color currentFill = pressed ? fillPressed : (hovered ? fillHover : fillNormal);
        Raylib_cs.Color currentBorder = hovered ? borderHover : borderNorm;

        // BG
        Raylib_cs.Raylib.DrawRectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y, currentFill);

        // Border
        if (hasBorder)
            Raylib_cs.Raylib.DrawRectangleLinesEx(new Raylib_cs.Rectangle(Position.X, Position.Y, Size.X, Size.Y), 1f, currentBorder);

        // Text (centered)
        Vector2 textSize = LayoutEngine.MeasureText(buttonText, fontSize);
        int textX = (int)(Position.X + (Size.X - textSize.X) / 2);
        int textY = (int)(Position.Y + (Size.Y - textSize.Y) / 2);

        LayoutEngine.DrawTextAbsolute(buttonText, textX, textY, labelColor, fontSize, Vector2.Zero);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left)
            return false;

        pressed = true;
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left)
            return false;

        onButtonPressed?.Invoke(this);
        pressed = false;
        return true;
    }
}
