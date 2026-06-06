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

    public string SelectableText { get => selectableText; }
    public int Width { get => width; }
    public int Height { get => height; }
    public object Payload { get => payload; }

    private bool isBeingEdited = false;
    private InputField editIF;

    public Action<Selectable> OnTextEdited;

    public Selectable(string selectableText, int posX, int posY, int width, int height, Action<Selectable> onSelectableSelect, object payload, int fontSize = 15, Color? bgColor = null, Color? bgSelectionColor = null, Color? textColor = null, Drawable? parent = null) : base(parent)
    {
        selfInteractable = true;

        this.selectableText = selectableText;

        RelativePosition = new Vector2(posX, posY);

        this.width = width;
        this.height = height;

        this.onSelectableSelect = onSelectableSelect;
        this.payload = payload;

        this.fontSize = fontSize;

        this.bgColor = bgColor ?? new Color((byte)175, (byte)175, (byte)175, (byte)255);
        this.bgSelectionColor = bgSelectionColor ?? new Color((byte)175, (byte)175, (byte)255, (byte)255);
        this.textColor = textColor ?? Color.Black;

        Engine.OnAnyPointerDown += OnClickOff;

        editIF = new InputField("", selectableText, 0, 0, width, height, parent: this);

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
        if (isBeingEdited) editIF.Render();
        else
        {
            Raylib.DrawRectangle((int)Position.X, (int)Position.Y, width, height, isSelected ? bgSelectionColor : bgColor);
            Raylib.DrawText(selectableText, (int)Position.X, (int)Position.Y, fontSize, textColor);
        }
    }

    protected override void OnUpdate()
    {
        if (isBeingEdited)
            editIF.Update();
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
        isSelected = true;
        onSelectableSelect?.Invoke(this);

        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        return false;
    }

    public bool OnDoubleClick(PointerInteractEventData eventData)
    {
        isBeingEdited = true;
        editIF.SetFocus();

        return true;
    }

    public void SetText(string newText)
    {
        selectableText = newText;
        editIF.SetText(newText);
    }
}
