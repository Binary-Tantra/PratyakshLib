using System.Numerics;
using Pratyaksh.Core;

namespace Pratyaksh.UI.UIElements;

public class InputField : UIBase, IPointerInteractable, IKeyInteractable
{
    private string placeholderText;
    private string inputFieldText;

    private int fontSize;

    private bool isShowingPlaceholder = false;
    private bool isFocused = false;

    public Action<InputField>? OnTextChanged;
    public Action<InputField>? OnFocusEnd;

    private bool notifyTextChanged;

    public string InputFieldText
    {
        get => inputFieldText;
        set
        {
            if (inputFieldText == value)
                return;

            inputFieldText = value;
            isShowingPlaceholder = string.IsNullOrEmpty(inputFieldText);

            if (notifyTextChanged)
                OnTextChanged?.Invoke(this);
        }
    }

    private bool isMasked = false;
    private char maskChar = '*';

    public bool IsMasked { get => isMasked; set => isMasked = value; }
    public char MaskChar { get => maskChar; set => maskChar = value; }

    public bool IsFocused => isFocused;

    public InputField(string placeholderText, string inputFieldText, int posX, int posY, int width, int height, Action<InputField>? onTextEdited, Action<InputField>? onFocusEnd, int fontSize = 15, bool isMasked = false, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, width, height, parent, parentBasis)
    {
        selfInteractable = true;

        OnTextChanged = onTextEdited;
        OnFocusEnd = onFocusEnd;

        if (string.IsNullOrEmpty(placeholderText))
            placeholderText = "Enter text";

        notifyTextChanged = false;
        {
            this.placeholderText = placeholderText;
            this.isMasked = isMasked;

            InputFieldText = inputFieldText;
        }
        notifyTextChanged = true;

        this.fontSize = fontSize;
    }

    public void SetTextWithoutNotify(string text)
    {
        inputFieldText = text ?? string.Empty;
        isShowingPlaceholder = string.IsNullOrEmpty(inputFieldText);
    }

    protected override void OnDraw()
    {
        // Palette
        Raylib_cs.Color fillColor = new((byte)38, (byte)38, (byte)38, (byte)255);
        Raylib_cs.Color borderNormal = new((byte)85, (byte)85, (byte)85, (byte)255);
        Raylib_cs.Color borderFocused = new((byte)65, (byte)120, (byte)200, (byte)255);
        Raylib_cs.Color inputTextCol = new((byte)215, (byte)215, (byte)215, (byte)255);
        Raylib_cs.Color placeholderCol = new((byte)105, (byte)105, (byte)105, (byte)255);
        Raylib_cs.Color cursorCol = new((byte)190, (byte)190, (byte)190, (byte)255);

        const int textPadX = 6;
        int textY = (int)(Position.Y + (Height - fontSize) / 2f);   // vertically center the text

        // BG
        Raylib_cs.Raylib.DrawRectangle((int)Position.X, (int)Position.Y, Width, Height, fillColor);

        // Border
        Raylib_cs.Rectangle borderRect = new(InteractionRect.X, InteractionRect.Y, InteractionRect.Width, InteractionRect.Height);
        
        if (isFocused)
            Raylib_cs.Raylib.DrawRectangleLinesEx(borderRect, 2f, borderFocused);
        else
            Raylib_cs.Raylib.DrawRectangleLinesEx(borderRect, 1f, borderNormal);

        // Text display string (masked or raw)
        string displayText = isShowingPlaceholder 
            ? placeholderText 
            : (isMasked ? new string(maskChar, InputFieldText.Length) : InputFieldText);

        // Placeholder Text or Normal Text
        if (isShowingPlaceholder)
            LayoutEngine.DrawTextAbsolute(placeholderText, (int)Position.X + textPadX, textY, placeholderCol, fontSize, Vector2.Zero);
        else
            LayoutEngine.DrawTextAbsolute(displayText, (int)Position.X + textPadX, textY, inputTextCol, fontSize, Vector2.Zero);

        // Blinking Cursor
        if (isFocused)
        {
            bool showCursorTime = (Engine.Instance.GetTime() % 1.0 < 0.5);

            if (showCursorTime)
            {
                int textW = isShowingPlaceholder ? 0 : LayoutEngine.MeasureTextW(displayText, fontSize);
                int cursorX = (int)Position.X + textPadX + textW + 1;

                Raylib_cs.Raylib.DrawLine(cursorX, (int)Position.Y + 5, cursorX, (int)Position.Y + Height - 7, cursorCol);
            }
        }
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left)
            return false;

        SetFocus();
        return true;
    }

    public bool SetFocus()
    {
        Engine.Instance.InteractionManager.CaptureFocus(this);
        isFocused = true;
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left)
            return false;

        return true;
    }

    public bool OnKeyDown(KeyInteractEventData kvt)
    {
        if (!isFocused)
            return false;

        // Check for Ctrl + V (Paste)
        if (kvt.Key == KeyboardKey.V && kvt.IsCtrlDown)
        {
            string clipboardText = Clipboard.GetClipboardText();

            if (!string.IsNullOrEmpty(clipboardText))
            {
                if (isShowingPlaceholder)
                {
                    InputFieldText = clipboardText;
                    isShowingPlaceholder = false;
                }
                else
                {
                    InputFieldText += clipboardText;
                }
            }

            return true;
        }

        if ((kvt.Key >= KeyboardKey.A && kvt.Key <= KeyboardKey.Z) ||
            (kvt.Key >= KeyboardKey.Zero && kvt.Key <= KeyboardKey.Nine) ||
            (kvt.Key >= KeyboardKey.KPZero && kvt.Key <= KeyboardKey.KPNine) ||
             kvt.Key == KeyboardKey.Space ||
             kvt.Key == KeyboardKey.Period || kvt.Key == KeyboardKey.KPDecimal ||
             kvt.Key == KeyboardKey.Minus || kvt.Key == KeyboardKey.KPMinus ||
             kvt.Key == KeyboardKey.Comma)
        {
            string newK;

            if (kvt.Key == KeyboardKey.Space)
                newK = " ";
            else if (kvt.Key >= KeyboardKey.Zero && kvt.Key <= KeyboardKey.Nine)
                newK = ((int)kvt.Key - (int)KeyboardKey.Zero).ToString();
            else if (kvt.Key >= KeyboardKey.KPZero && kvt.Key <= KeyboardKey.KPNine)
                newK = ((int)kvt.Key - (int)KeyboardKey.KPZero).ToString();
            else if (kvt.Key == KeyboardKey.Period || kvt.Key == KeyboardKey.KPDecimal)
                newK = ".";
            else if (kvt.Key == KeyboardKey.Minus || kvt.Key == KeyboardKey.KPMinus)
                newK = "-";
            else if (kvt.Key == KeyboardKey.Comma)
                newK = ",";
            else newK = kvt.IsShiftDown ?
                 kvt.Key.ToString().ToUpper() :
                 kvt.Key.ToString().ToLower();

            if (isShowingPlaceholder)
            {
                InputFieldText = newK;
                isShowingPlaceholder = false;
            }
            else InputFieldText += newK;
        }
        else if (kvt.Key == KeyboardKey.Backspace)
        {
            if (InputFieldText.Length > 0)
                InputFieldText = InputFieldText[..^1];
            else
                InputFieldText = string.Empty;
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
        if (!isFocused)
            return;

        isFocused = false;

        if (Engine.Instance.InteractionManager.CurrentlyFocused == this)
            Engine.Instance.InteractionManager.ReleaseFocus();

        OnFocusEnd?.Invoke(this);
    }

    public void OnFocusLost()
    {
        EndFocus();
    }
}