using LibLayoutEngine;
using Raylib_cs;
using System.Numerics;

namespace RaylibNodeLibrary.UI;

public class Selectable : UIBase, IPointerInteractable, IDoubleClickable
{
    private string selectableText;

    private int width;
    private int height;

    private int fontSize;

    private Action<Selectable> onSelectableSelect;

    private Color bgColor;
    private Color bgSelectionColor;
    private Color textColor;

    private object payload;

    private bool isSelected = false;

    public string SelectableText
    {
        get => selectableText;
        set
        {
            if (!isBeingEdited)
            {
                selectableText = value;
                editIF.InputFieldText = value;
            }
        }
    }

    public int Width { get => width; }
    public int Height { get => height; }
    public object Payload { get => payload; }
    public bool IsSelected { get => isSelected; }

    private bool isBeingEdited = false;
    private InputField editIF;

    public Action<Selectable> OnTextEdited;

    public Selectable(string selectableText, bool isSelected, int posX, int posY, int width, int height, Action<Selectable> onSelectableSelect, object payload, int fontSize = 15, Color? bgColor = null, Color? bgSelectionColor = null, Color? textColor = null, Drawable? parent = null) : base(parent)
    {
        selfInteractable = true;

        this.selectableText = selectableText;

        RelativePosition = new Vector2(posX, posY);

        this.isSelected = isSelected;

        this.width = width;
        this.height = height;

        this.onSelectableSelect = onSelectableSelect;
        this.payload = payload;

        this.fontSize = fontSize;

        this.bgColor = bgColor ?? new Color((byte)175, (byte)175, (byte)175, (byte)255);
        this.bgSelectionColor = bgSelectionColor ?? new Color((byte)175, (byte)175, (byte)255, (byte)255);
        this.textColor = textColor ?? Color.Black;

        Engine.OnAnyPointerDown += OnClickOff;

        editIF = new InputField("", selectableText, 0, 0, width, height, null, null, parent: this);

        editIF.OnTextChanged += (inpField) => this.selectableText = inpField.InputFieldText;
        editIF.OnFocusEnd += (inpField) =>
        {
            this.selectableText = inpField.InputFieldText;
            isBeingEdited = false;
            OnTextEdited?.Invoke(this);
        };
    }

    private void OnClickOff(PointerInteractEventData evt, EditorObject? target)
    {
        if (evt.mouseButton == MouseButton.Left && target != this)
            Deselect();
    }

    protected override void OnDraw()
    {
        if (isBeingEdited)
        {
            editIF.Render();
            return;
        }

        // Palette
        Color fillNormal = new((byte)38, (byte)38, (byte)38, (byte)255);
        Color fillHover = new((byte)55, (byte)55, (byte)55, (byte)255);
        Color fillSelected = new((byte)28, (byte)50, (byte)88, (byte)255);
        Color accentBar = new((byte)65, (byte)120, (byte)200, (byte)255);
        Color textNormal = new((byte)200, (byte)200, (byte)200, (byte)255);
        Color textSelected = new((byte)220, (byte)228, (byte)255, (byte)255);

        const int accentW = 3;
        const int textPadX = 9;

        // BG fill according to selection.
        Color fill = isSelected ? fillSelected : (hovered ? fillHover : fillNormal);
        Raylib.DrawRectangle((int)Position.X, (int)Position.Y, width, height, fill);

        // Left accent bar (when selected)
        if (isSelected)
            Raylib.DrawRectangle((int)Position.X, (int)Position.Y, accentW, height, accentBar);

        // Text
        int textY = (int)(Position.Y + (height - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute(selectableText, (int)Position.X + textPadX, textY, isSelected ? textSelected : textNormal, fontSize, Vector2.Zero);
    }

    protected override void OnUpdate()
    {
        if (isBeingEdited)
            editIF.Update();
    }

    public void Select()
    {
        isSelected = true;
        onSelectableSelect?.Invoke(this);
    }

    public void Deselect()
    {
        isSelected = false;
    }

    protected override void OnDelete()
    {
        Deselect();
        editIF.Delete();

        Engine.OnAnyPointerDown -= OnClickOff;
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, width, height);
    }

    public void SetDimensions(int width, int height)
    {
        this.width = width;
        this.height = height;
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        Select();

        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left)
            return false;

        return true;
    }

    public bool OnDoubleClick(PointerInteractEventData eventData)
    {
        if (eventData.mouseButton != MouseButton.Left)
            return false;

        isBeingEdited = true;
        editIF.SetFocus();

        return true;
    }
}
