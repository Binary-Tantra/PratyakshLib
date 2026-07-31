using System.Numerics;
using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public class CycleSelector : UIBase, IPointerInteractable
{
    private string[] options;
    private int selectedIndex;
    private int fontSize;
    private object payload;

    private Action<CycleSelector>? onSelectionChanged;

    public string[] Options
    {
        get => options;
        set => options = value ?? [];
    }

    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (options.Length > 0)
                selectedIndex = Math.Clamp(value, 0, options.Length - 1);
            else selectedIndex = 0;
        }
    }

    public string SelectedOption => (options.Length > 0 && selectedIndex < options.Length) ? options[selectedIndex] : string.Empty;
    public object Payload => payload;

    public CycleSelector(string[] options, int selectedIndex, int posX, int posY, int width, int height, Action<CycleSelector>? onSelectionChanged, object? payload = null, int fontSize = 15, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, width, height, parent, parentBasis)
    {
        selfInteractable = true;

        this.options = options ?? [];
        this.selectedIndex = (this.options.Length > 0) ? Math.Clamp(selectedIndex, 0, this.options.Length - 1) : 0;
        this.onSelectionChanged = onSelectionChanged;
        this.payload = payload ?? string.Empty;
        this.fontSize = fontSize;
    }

    protected override void OnDraw()
    {
        int btnWidth = Math.Min(Height, 28);
        if (Width < btnWidth * 2 + 20) btnWidth = Math.Max(16, Width / 4);

        Vector2 mousePos = Raylib.GetMousePosition();
        float relX = mousePos.X - Position.X;
        float relY = mousePos.Y - Position.Y;

        bool isMouseOver = hovered && (relY >= 0 && relY <= Height);
        bool isLeftHovered = isMouseOver && (relX >= 0 && relX < btnWidth);
        bool isRightHovered = isMouseOver && (relX >= Width - btnWidth && relX <= Width);
        bool isCenterHovered = isMouseOver && (!isLeftHovered && !isRightHovered);

        // Palette
        Color btnNormal = new((byte)48, (byte)48, (byte)48, (byte)255);
        Color btnHover = new((byte)75, (byte)75, (byte)75, (byte)255);
        Color borderNorm = new((byte)75, (byte)75, (byte)75, (byte)255);
        Color borderHover = new((byte)120, (byte)120, (byte)120, (byte)255);
        
        Color labelBgNormal = new((byte)30, (byte)30, (byte)30, (byte)255);
        Color labelBgHover = new((byte)40, (byte)40, (byte)40, (byte)255);

        Color textCol = new((byte)220, (byte)220, (byte)220, (byte)255);

        // 1. Draw Left Button (<)
        Color leftFill = isLeftHovered ? btnHover : btnNormal;
        Color leftBorder = isLeftHovered ? borderHover : borderNorm;
        Rectangle leftBtnRect = new(Position.X, Position.Y, btnWidth, Height);
        Raylib.DrawRectangleRec(leftBtnRect, leftFill);
        Raylib.DrawRectangleLinesEx(leftBtnRect, 1f, leftBorder);

        int arrowTextW1 = LayoutEngine.MeasureTextW("<", fontSize);
        int arrowX1 = (int)(Position.X + (btnWidth - arrowTextW1) / 2f);
        int arrowY1 = (int)(Position.Y + (Height - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute("<", arrowX1, arrowY1, isLeftHovered ? Color.White : textCol, fontSize, Vector2.Zero);

        // 2. Draw Right Button (>)
        Color rightFill = isRightHovered ? btnHover : btnNormal;
        Color rightBorder = isRightHovered ? borderHover : borderNorm;
        Rectangle rightBtnRect = new(Position.X + Width - btnWidth, Position.Y, btnWidth, Height);
        Raylib.DrawRectangleRec(rightBtnRect, rightFill);
        Raylib.DrawRectangleLinesEx(rightBtnRect, 1f, rightBorder);

        int arrowTextW2 = LayoutEngine.MeasureTextW(">", fontSize);
        int arrowX2 = (int)(Position.X + Width - btnWidth + (btnWidth - arrowTextW2) / 2f);
        int arrowY2 = (int)(Position.Y + (Height - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute(">", arrowX2, arrowY2, isRightHovered ? Color.White : textCol, fontSize, Vector2.Zero);

        // 3. Draw Center Label Area
        int labelWidth = Width - (btnWidth * 2);
        Rectangle labelRect = new(Position.X + btnWidth, Position.Y, labelWidth, Height);
        Color labelBg = isCenterHovered ? labelBgHover : labelBgNormal;
        Raylib.DrawRectangleRec(labelRect, labelBg);
        Raylib.DrawRectangleLinesEx(labelRect, 1f, borderNorm);

        // Format and display option string centered inside label area
        string optStr = SelectedOption;
        int optW = LayoutEngine.MeasureTextW(optStr, fontSize);
        
        // Truncate if option string is too wide for label area
        if (optW > labelWidth - 8 && labelWidth > 16)
        {
            const string ellipsis = "..";
            int ellW = LayoutEngine.MeasureTextW(ellipsis, fontSize);
            int availableW = labelWidth - 8 - ellW;
            int len = optStr.Length;
            while (len > 0 && LayoutEngine.MeasureTextW(optStr.Substring(0, len), fontSize) > availableW)
            {
                len--;
            }
            optStr = optStr.Substring(0, len) + ellipsis;
            optW = LayoutEngine.MeasureTextW(optStr, fontSize);
        }

        int labelX = (int)(Position.X + btnWidth + (labelWidth - optW) / 2f);
        int labelY = (int)(Position.Y + (Height - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute(optStr, labelX, labelY, isCenterHovered ? Color.White : textCol, fontSize, Vector2.Zero);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left) return false;
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left) return false;

        if (options.Length > 0)
        {
            int btnWidth = Math.Min(Height, 28);
            if (Width < btnWidth * 2 + 20) btnWidth = Math.Max(16, Width / 4);

            float clickX = evt.ScreenPosition.X - Position.X;
            if (clickX >= 0 && clickX < btnWidth)
            {
                // Left Arrow Button -> Cycle Backward
                selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                onSelectionChanged?.Invoke(this);
            }
            else if (clickX >= Width - btnWidth)
            {
                // Right Arrow Button -> Cycle Forward
                selectedIndex = (selectedIndex + 1) % options.Length;
                onSelectionChanged?.Invoke(this);
            }
            else if (clickX >= btnWidth && clickX < Width - btnWidth)
            {
                // Center Label -> Cycle Forward
                selectedIndex = (selectedIndex + 1) % options.Length;
                onSelectionChanged?.Invoke(this);
            }
        }

        return true;
    }

    public void SetOnSelectionChanged(Action<CycleSelector>? callback)
    {
        onSelectionChanged = callback;
    }
}
