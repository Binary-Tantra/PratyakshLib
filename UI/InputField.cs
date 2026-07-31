using Raylib_cs;
using System.Numerics;

namespace RaylibNodeLibrary.UI;

public class InputField : UIBase, IPointerInteractable, IKeyInteractable
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool CloseClipboard();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr GetClipboardData(uint uFormat);

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

    private static string GetClipboardTextSafe()
    {
        const uint CF_UNICODETEXT = 13;
        if (!OpenClipboard(IntPtr.Zero)) return string.Empty;
        try
        {
            IntPtr handle = GetClipboardData(CF_UNICODETEXT);
            if (handle == IntPtr.Zero) return string.Empty;
            return System.Runtime.InteropServices.Marshal.PtrToStringUni(handle) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
        finally
        {
            CloseClipboard();
        }
    }

    public void PasteFromClipboard()
    {
        try
        {
            string clipboardText = GetClipboardTextSafe();
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
        }
        catch { /* Clipboard access failed */ }
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
        int textY = (int)(Position.Y + (Height - fontSize) / 2f);   // vertically center the text

        // BG
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y, Width, Height, fillColor);

        // Border
        Rectangle borderRect = new(Position.X, Position.Y, Width, Height);
        
        if (isFocused)
            Raylib.DrawRectangleLinesEx(borderRect, 2f, borderFocused);
        else
            Raylib.DrawRectangleLinesEx(borderRect, 1f, borderNormal);

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
            bool showCursorTime = (Raylib.GetTime() % 1.0 < 0.5);

            if (showCursorTime)
            {
                int textW = isShowingPlaceholder ? 0 : LayoutEngine.MeasureTextW(displayText, fontSize);
                int cursorX = (int)Position.X + textPadX + textW + 1;

                Raylib.DrawLine(cursorX, (int)Position.Y + 5, cursorX, (int)Position.Y + Height - 7, cursorCol);
            }
        }
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        SetFocus();
        return true;
    }

    public bool SetFocus()
    {
        InteractionManager.CaptureFocus(this);
        isFocused = true;
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
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
            PasteFromClipboard();
            return true;
        }

        if ((kvt.Key >= KeyboardKey.A && kvt.Key <= KeyboardKey.Z) ||
            (kvt.Key >= KeyboardKey.Zero && kvt.Key <= KeyboardKey.Nine) ||
            (kvt.Key >= KeyboardKey.Kp0 && kvt.Key <= KeyboardKey.Kp9) ||
             kvt.Key == KeyboardKey.Space ||
             kvt.Key == KeyboardKey.Period || kvt.Key == KeyboardKey.KpDecimal ||
             kvt.Key == KeyboardKey.Minus || kvt.Key == KeyboardKey.KpSubtract ||
             kvt.Key == KeyboardKey.Comma)
        {
            string newK;

            if (kvt.Key == KeyboardKey.Space)
                newK = " ";
            else if (kvt.Key >= KeyboardKey.Zero && kvt.Key <= KeyboardKey.Nine)
                newK = ((int)kvt.Key - (int)KeyboardKey.Zero).ToString();
            else if (kvt.Key >= KeyboardKey.Kp0 && kvt.Key <= KeyboardKey.Kp9)
                newK = ((int)kvt.Key - (int)KeyboardKey.Kp0).ToString();
            else if (kvt.Key == KeyboardKey.Period || kvt.Key == KeyboardKey.KpDecimal)
                newK = ".";
            else if (kvt.Key == KeyboardKey.Minus || kvt.Key == KeyboardKey.KpSubtract)
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

        if (InteractionManager.CurrentlyFocused == this)
            InteractionManager.ReleaseFocus();

        OnFocusEnd?.Invoke(this);
    }

    public void OnFocusLost()
    {
        EndFocus();
    }
}