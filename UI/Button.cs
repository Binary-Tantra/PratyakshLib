using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public class Button : UIBase, IPointerInteractable
{
    private System.Numerics.Vector2 dimensions;
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

    public float Width { get => dimensions.X; }
    public float Height { get => dimensions.Y; }

    public Button(float width, float height, string buttonText, Action<Button> onButtonPressed, object payload, int fontSize = 15, bool hasBorder = true, Drawable? parent = null) : base(parent)
    {
        selfInteractable = true;

        dimensions.X = Raylib.MeasureText(buttonText, fontSize);
        if (width > dimensions.X) dimensions.X = width;

        dimensions.Y = fontSize + 5;
        if (height > dimensions.Y) dimensions.Y = height;

        this.onButtonPressed = onButtonPressed;
        this.payload = payload;

        this.buttonText = buttonText;
        this.fontSize = fontSize;
        this.hasBorder = hasBorder;
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, dimensions.X, dimensions.Y);
    }

    protected override void OnDraw()
    {
        // Palette
        Color fillNormal = new((byte)48, (byte)48, (byte)48, (byte)255);
        Color fillHover = new((byte)64, (byte)64, (byte)64, (byte)255);
        Color fillPressed = new((byte)30, (byte)30, (byte)30, (byte)255);
        Color borderNorm = new((byte)75, (byte)75, (byte)75, (byte)255);
        Color borderHover = new((byte)108, (byte)108, (byte)108, (byte)255);
        Color labelColor = new((byte)200, (byte)200, (byte)200, (byte)255);

        // Fill
        Color fillColor = pressed ? fillPressed : (hovered ? fillHover : fillNormal);
        //if (hovered) Console.WriteLine(fillColor);
        Color borderColor = hovered ? borderHover : borderNorm;

        // BG
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y, (int)dimensions.X, (int)dimensions.Y, fillColor);

        // Border
        if (hasBorder)
            Raylib.DrawRectangleLinesEx(new Rectangle(Position.X, Position.Y, dimensions.X, dimensions.Y), 1f, borderColor);

        // Text (centered)
        int textW = Raylib.MeasureText(buttonText, fontSize);
        int textX = (int)(Position.X + (dimensions.X - textW) / 2f);
        int textY = (int)(Position.Y + (dimensions.Y - fontSize) / 2f);

        Raylib.DrawText(buttonText, textX, textY, fontSize, labelColor);
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
