using System.Numerics;
using Pratyaksh.Core;

namespace Pratyaksh.UI.UIElements;

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

    public CycleSelector(string[] options, int selectedIndex, int posX, int posY, int width, int height, Action<CycleSelector>? onSelectionChanged, object? payload = null, int fontSize = 15, Transform? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, width, height, parentBasis, parent)
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
        int btnWidth = Math.Min(UITransform.Height, 28);
        if (UITransform.Width < btnWidth * 2 + 20) btnWidth = Math.Max(16, UITransform.Width / 4);

        Vector2 mousePos = Raylib_cs.Raylib.GetMousePosition();
        float relX = mousePos.X - UITransform.X;
        float relY = mousePos.Y - UITransform.Y;

        bool isMouseOver = hovered && (relY >= 0 && relY <= UITransform.Height);
        bool isLeftHovered = isMouseOver && (relX >= 0 && relX < btnWidth);
        bool isRightHovered = isMouseOver && (relX >= UITransform.Width - btnWidth && relX <= UITransform.Width);
        bool isCenterHovered = isMouseOver && (!isLeftHovered && !isRightHovered);

        // Palette
        Raylib_cs.Color btnNormal = new((byte)48, (byte)48, (byte)48, (byte)255);
        Raylib_cs.Color btnHover = new((byte)75, (byte)75, (byte)75, (byte)255);
        Raylib_cs.Color borderNorm = new((byte)75, (byte)75, (byte)75, (byte)255);
        Raylib_cs.Color borderHover = new((byte)120, (byte)120, (byte)120, (byte)255);
        
        Raylib_cs.Color labelBgNormal = new((byte)30, (byte)30, (byte)30, (byte)255);
        Raylib_cs.Color labelBgHover = new((byte)40, (byte)40, (byte)40, (byte)255);

        Raylib_cs.Color textCol = new((byte)220, (byte)220, (byte)220, (byte)255);

        // 1. Draw Left Button (<)
        Raylib_cs.Color leftFill = isLeftHovered ? btnHover : btnNormal;
        Raylib_cs.Color leftBorder = isLeftHovered ? borderHover : borderNorm;
        Raylib_cs.Rectangle leftBtnRect = new(UITransform.X, UITransform.Y, btnWidth, UITransform.Height);
        Raylib_cs.Raylib.DrawRectangleRec(leftBtnRect, leftFill);
        Raylib_cs.Raylib.DrawRectangleLinesEx(leftBtnRect, 1f, leftBorder);

        int arrowTextW1 = LayoutEngine.MeasureTextW("<", fontSize);
        int arrowX1 = (int)(UITransform.X + (btnWidth - arrowTextW1) / 2f);
        int arrowY1 = (int)(UITransform.Y + (UITransform.Height - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute("<", arrowX1, arrowY1, isLeftHovered ? Raylib_cs.Color.White : textCol, fontSize, Vector2.Zero);

        // 2. Draw Right Button (>)
        Raylib_cs.Color rightFill = isRightHovered ? btnHover : btnNormal;
        Raylib_cs.Color rightBorder = isRightHovered ? borderHover : borderNorm;
        Raylib_cs.Rectangle rightBtnRect = new(UITransform.X + UITransform.Width - btnWidth, UITransform.Y, btnWidth, UITransform.Height);
        Raylib_cs.Raylib.DrawRectangleRec(rightBtnRect, rightFill);
        Raylib_cs.Raylib.DrawRectangleLinesEx(rightBtnRect, 1f, rightBorder);

        int arrowTextW2 = LayoutEngine.MeasureTextW(">", fontSize);
        int arrowX2 = (int)(UITransform.X + UITransform.Width - btnWidth + (btnWidth - arrowTextW2) / 2f);
        int arrowY2 = (int)(UITransform.Y + (UITransform.Height - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute(">", arrowX2, arrowY2, isRightHovered ? Raylib_cs.Color.White : textCol, fontSize, Vector2.Zero);

        // 3. Draw Center Label Area
        int labelWidth = UITransform.Width - (btnWidth * 2);
        Raylib_cs.Rectangle labelRect = new(UITransform.X + btnWidth, UITransform.Y, labelWidth, UITransform.Height);
        Raylib_cs.Color labelBg = isCenterHovered ? labelBgHover : labelBgNormal;
        Raylib_cs.Raylib.DrawRectangleRec(labelRect, labelBg);
        Raylib_cs.Raylib.DrawRectangleLinesEx(labelRect, 1f, borderNorm);

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

        int labelX = (int)(UITransform.X + btnWidth + (labelWidth - optW) / 2f);
        int labelY = (int)(UITransform.Y + (UITransform.Height - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute(optStr, labelX, labelY, isCenterHovered ? Raylib_cs.Color.White : textCol, fontSize, Vector2.Zero);
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left) return false;
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left) return false;

        if (options.Length > 0)
        {
            int btnWidth = Math.Min(UITransform.Height, 28);
            if (UITransform.Width < btnWidth * 2 + 20) btnWidth = Math.Max(16, UITransform.Width / 4);

            float clickX = evt.ScreenPosition.X - UITransform.X;
            if (clickX >= 0 && clickX < btnWidth)
            {
                // Left Arrow Button -> Cycle Backward
                selectedIndex = (selectedIndex - 1 + options.Length) % options.Length;
                onSelectionChanged?.Invoke(this);
            }
            else if (clickX >= UITransform.Width - btnWidth)
            {
                // Right Arrow Button -> Cycle Forward
                selectedIndex = (selectedIndex + 1) % options.Length;
                onSelectionChanged?.Invoke(this);
            }
            else if (clickX >= btnWidth && clickX < UITransform.Width - btnWidth)
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
