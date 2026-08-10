using System.Numerics;
using Pratyaksh.Core;

namespace Pratyaksh.UI.UIElements;

public class Selectable : UIBase, IPointerInteractable, IDoubleClickable
{
    private string selectableText;

    private int fontSize;

    private Action<Selectable> onSelectableSelect;
    private Action<Selectable> onSelectableDeselect;

    private Raylib_cs.Color bgColor;
    private Raylib_cs.Color bgSelectionColor;
    private Raylib_cs.Color textColor;

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

    public object Payload { get => payload; }
    public bool IsSelected { get => isSelected; }

    private bool isBeingEdited = false;
    private InputField editIF;

    public Action<Selectable> OnTextEdited;

    public Selectable(string selectableText, bool isSelected, int posX, int posY, int width, int height, Action<Selectable> onSelectableSelect, object payload, int fontSize = 15, Raylib_cs.Color? bgColor = null, Raylib_cs.Color? bgSelectionColor = null, Raylib_cs.Color? textColor = null, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, width, height, parent, parentBasis)
    {
        selfInteractable = true;

        this.selectableText = selectableText;

        this.isSelected = isSelected;

        this.onSelectableSelect = onSelectableSelect;
        this.payload = payload;

        this.fontSize = fontSize;

        this.bgColor = bgColor ?? new Raylib_cs.Color((byte)38, (byte)38, (byte)38, (byte)255);
        this.bgSelectionColor = bgSelectionColor ?? new Raylib_cs.Color((byte)28, (byte)50, (byte)88, (byte)255);
        this.textColor = textColor ?? new Raylib_cs.Color((byte)200, (byte)200, (byte)200, (byte)255);

        Engine.Instance.InteractionManager.AnyPointerEvent += OnClickOff;

        editIF = new InputField("", selectableText, 0, 0, Width, Height, null, null, parent: this);

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
        if (evt.MouseButton == MouseButton.Left && target != this)
            Deselect();
    }

    protected override void OnDraw()
    {
        if (isBeingEdited)
        {
            editIF.Render();
            return;
        }

        // Palette - derived from user-provided or default colors
        Raylib_cs.Color fillNormal = bgColor;
        Raylib_cs.Color fillHover = new Raylib_cs.Color((byte)Math.Min(255, bgColor.R + 25), (byte)Math.Min(255, bgColor.G + 25), (byte)Math.Min(255, bgColor.B + 25), bgColor.A);
        Raylib_cs.Color fillSelected = bgSelectionColor;
        Raylib_cs.Color accentBar = new((byte)65, (byte)120, (byte)200, (byte)255);
        Raylib_cs.Color textNormal = textColor;
        Raylib_cs.Color textSelected = textColor;

        const int accentW = 3;
        const int textPadX = 9;

        // BG fill according to selection.
        Raylib_cs.Color fill = isSelected ? fillSelected : (hovered ? fillHover : fillNormal);
        Raylib_cs.Raylib.DrawRectangle((int)Position.X, (int)Position.Y, Width, Height, fill);

        // Left accent bar (when selected)
        if (isSelected)
            Raylib_cs.Raylib.DrawRectangle((int)Position.X, (int)Position.Y, accentW, Height, accentBar);

        // Text
        int textY = (int)(Position.Y + (Height - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute(selectableText, (int)Position.X + textPadX, textY, isSelected ? textSelected : textNormal, fontSize, Vector2.Zero);
    }

    protected override void OnUpdate()
    {
        if (isBeingEdited)
            editIF.Update();
    }

    public void Select(bool notify = true)
    {
        isSelected = true;
        if (notify) onSelectableSelect?.Invoke(this);
    }

    public void Deselect(bool notify = true)
    {
        isSelected = false;
        if (notify) onSelectableDeselect?.Invoke(this);
    }

    public void SetIsSelectedWithoutNotify(bool selected)
    {
        if (selected) Select(false);
        else Deselect(false);
    }

    protected override void OnDelete()
    {
        Deselect();
        editIF.Delete();

        Engine.Instance.InteractionManager.AnyPointerEvent -= OnClickOff;
    }

    public void SetOnSelect(Action<Selectable>? onSelectableSelect)
    {
        this.onSelectableSelect = onSelectableSelect;
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left)
            return false;

        Select();

        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left)
            return false;

        return true;
    }

    public bool OnDoubleClick(PointerInteractEventData eventData)
    {
        if (eventData.MouseButton != MouseButton.Left)
            return false;

        isBeingEdited = true;
        editIF.SetFocus();

        return true;
    }
}
