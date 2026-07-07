using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public class InputField : UIBase, IPointerInteractable, IKeyInteractable
{
    private string placeholderText;
    private string inputFieldText;

    private int width;
    private int height;

    private int fontSize;

    private bool isShowingPlaceholder = false;
    private bool isFocused = false;

    public Action<InputField> OnTextChanged;
    public Action<InputField> OnFocusEnd;

    public string InputFieldText { get => inputFieldText; }

    public InputField(string placeholderText, string startingText, int posX, int posY, int width, int height, int fontSize = 15, Drawable? parent = null) : base(parent)
    {
        selfInteractable = true;

        if (string.IsNullOrEmpty(placeholderText))
            placeholderText = "Enter text";

        this.placeholderText = placeholderText;

        if (string.IsNullOrEmpty(startingText))
        {
            inputFieldText = placeholderText;
            isShowingPlaceholder = true;
        }
        else
        {
            inputFieldText = startingText;
            isShowingPlaceholder = false;
        }

        RelativePosition = new System.Numerics.Vector2(posX, posY);

        this.width = width;
        this.height = height;

        this.fontSize = fontSize;
    }

    protected override void OnDraw()
    {
        // Palette
        Color fillColor = new((byte)38, (byte)38, (byte)38, (byte)255);
        Color borderNormal = new((byte)85, (byte)85, (byte)85, (byte)255);
        Color borderFocused = new((byte)65, (byte)120, (byte)200, (byte)255);
        Color inputTextCol = new((byte)215, (byte)215, (byte)215, (byte)255);
        Color placeholderCol = new((byte)105, (byte)105, (byte)105, (byte)255);
        Color cursorCol = new((byte)190, (byte)190, (byte)190, (byte)255);

        const int textPadX = 6;
        int textY = (int)(Position.Y + (height - fontSize) / 2f);   // vertically center the text

        // BG
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y, width, height, fillColor);

        // Border
        Rectangle borderRect = new(Position.X, Position.Y, width, height);
        
        if (isFocused)
            Raylib.DrawRectangleLinesEx(borderRect, 2f, borderFocused);
        else
            Raylib.DrawRectangleLinesEx(borderRect, 1f, borderNormal);

        // Placeholder Text or Normal Text
        if (isShowingPlaceholder)
            Raylib.DrawText(placeholderText, (int)Position.X + textPadX, textY, fontSize, placeholderCol);
        else
            Raylib.DrawText(inputFieldText, (int)Position.X + textPadX, textY, fontSize, inputTextCol);

        // Blinking Cursor
        if (isFocused)
        {
            bool showCursorTime = (Raylib.GetTime() % 1.0 < 0.5);

            if (showCursorTime)
            {
                int textW = isShowingPlaceholder ? 0 : Raylib.MeasureText(inputFieldText, fontSize);
                int cursorX = (int)Position.X + textPadX + textW + 1;

                Raylib.DrawLine(cursorX, (int)Position.Y + 3, cursorX, (int)Position.Y + height - 3, cursorCol);
            }
        }
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, width, height);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        SetFocus();
        return true;
    }

    public void SetFocus()
    {
        InteractionManager.CaptureFocus(this);
        isFocused = true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        return true;
    }

    private void SetIFText(string updated)
    {
        inputFieldText = updated;
        OnTextChanged?.Invoke(this);
    }

    public bool OnKeyDown(KeyInteractEventData kvt)
    {
        if (!isFocused)
            return false;

        if ((kvt.Key >= KeyboardKey.A && kvt.Key <= KeyboardKey.Z) ||
            (kvt.Key >= KeyboardKey.Zero && kvt.Key <= KeyboardKey.Nine) ||
             kvt.Key == KeyboardKey.Space)
        {
            string newK;

            if (kvt.Key == KeyboardKey.Space)
                newK = " ";
            
            else if (kvt.Key >= KeyboardKey.Zero &&
                kvt.Key <= KeyboardKey.Nine)
                newK = ((int)kvt.Key - 48).ToString();
            
            else newK = kvt.IsShiftDown ?
                 kvt.Key.ToString().ToUpper() :
                 kvt.Key.ToString().ToLower();

            if (isShowingPlaceholder)
            {
                SetIFText(newK);
                isShowingPlaceholder = false;
            }
            else SetIFText(inputFieldText + newK);
        }
        else if (kvt.Key == KeyboardKey.Backspace)
        {
            if (inputFieldText.Length > 0)
                inputFieldText = inputFieldText[..^1];
            else inputFieldText = string.Empty;
        }
        else if (kvt.Key == KeyboardKey.Enter) EndFocus();
        else return false;

        return true;
    }

    public bool OnKeyUp(KeyInteractEventData kvt)
    {
        return true;
    }

    private void EndFocus()
    {
        isFocused = false;
        OnFocusEnd?.Invoke(this);
    }

    public void OnFocusLost()
    {
        EndFocus();
    }

    public void SetText(string newText)
    {
        inputFieldText = newText;
        isShowingPlaceholder = false;
    }
}