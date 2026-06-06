using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public class InputField : UIBase, IPointerInteractable, IKeyInteractable
{
    private string placeholderText;
    private string inputFieldText;

    private int width;
    private int height;

    private int fontSize;

    private Color bgColor;
    private Color textColor;

    private bool isShowingPlaceholder = false;
    private bool isFocused = false;

    public Action<InputField> OnTextChanged;
    public Action<InputField> OnFocusEnd;

    public string InputFieldText { get => inputFieldText; }

    public InputField(string placeholderText, string startingText, int posX, int posY, int width, int height, int fontSize = 15, Color? bgColor = null, Color? textColor = null, Drawable? parent = null) : base(parent)
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

        this.bgColor = bgColor ?? Color.LightGray;
        this.textColor = textColor ?? Color.Black;
    }

    protected override void OnDraw()
    {
        if (isFocused)
        {
            Raylib.DrawRectangle((int)Position.X, (int)Position.Y, width, height, new Color((byte)0, (byte)0, (byte)200, (byte)255));
            Raylib.DrawRectangleLines((int)Position.X, (int)Position.Y, width, height, new Color((byte)255, (byte)255, (byte)255, (byte)255));
        }
        else
        {
            Raylib.DrawRectangle((int)Position.X, (int)Position.Y, width, height, bgColor);
            Raylib.DrawRectangleLines((int)Position.X, (int)Position.Y, width, height, new Color((byte)25, (byte)25, (byte)25, (byte)255));
        }
        
        if (isFocused)
            Raylib.DrawText(inputFieldText, (int)Position.X + 5, (int)Position.Y + 5, fontSize, new Color((byte)200, (byte)200, (byte)200, (byte)255));
        else
            Raylib.DrawText(inputFieldText, (int)Position.X + 5, (int)Position.Y + 5, fontSize, textColor);
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, width, height);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
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
        return false;
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
        return false;
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