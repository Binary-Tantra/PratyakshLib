using System.Numerics;
using LibLayoutEngine;
using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public class Dropdown : UIBase, IPointerInteractable
{
    private string[] options;
    private int selectedIndex;
    private bool isOpen = false;

    private int width;
    private int itemHeight;
    private int fontSize;

    private Action<Dropdown> onSelectionChanged;
    private object payload;

    private List<Selectable> optionSelectables = new();

    public string[] Options => options;
    
    public int SelectedIndex
    {
        get => selectedIndex;
        set => selectedIndex = value;
    }
    
    public string SelectedOption => (options != null && options.Length > selectedIndex && selectedIndex >= 0) ? options[selectedIndex] : "";
    public object Payload => payload;
    public int Width => width;

    // Accordion logic: height expands when open so the LayoutEngine reflows elements below it.
    public int Height => isOpen ? itemHeight + (options.Length * itemHeight) : itemHeight;

    public Dropdown(string[] options, int selectedIndex, int posX, int posY, int width, int itemHeight, Action<Dropdown> onSelectionChanged, object payload, int fontSize = 15, Drawable? parent = null) : base(parent)
    {
        selfInteractable = true;

        this.options = options ?? [];
        this.selectedIndex = selectedIndex;
        
        RelativePosition = new Vector2(posX, posY);
        
        this.width = width;
        this.itemHeight = itemHeight;
        this.onSelectionChanged = onSelectionChanged;
        this.payload = payload;
        this.fontSize = fontSize;

        // Subscribe to global clicks to close the dropdown if the user clicks outside
        Engine.OnAnyPointerDown += OnClickOff;

        RebuildOptions();
    }

    private void RebuildOptions()
    {
        foreach (var opt in optionSelectables)
            opt.Delete();

        optionSelectables.Clear();

        for (int i = 0; i < options.Length; i++)
        {
            int index = i;
            Selectable sel = new(options[i], 0, itemHeight + (i * itemHeight), width, itemHeight, (s) => OnOptionSelected(index), null, fontSize, parent: this);
            optionSelectables.Add(sel);
        }
    }

    public void SetOptions(string[] newOptions, int newSelectedIndex)
    {
        options = newOptions ?? [];
        selectedIndex = newSelectedIndex;

        RebuildOptions();
    }

    private void OnOptionSelected(int index)
    {
        selectedIndex = index;
        isOpen = false;
        onSelectionChanged?.Invoke(this);
    }

    private void OnClickOff(PointerInteractEventData evt, EditorObject? target)
    {
        if (isOpen && evt.mouseButton == MouseButton.Left)
        {
            // Close if clicked outside the dropdown header and options
            bool clickedInside = target == this || optionSelectables.Contains(target as Selectable);
            if (!clickedInside)
            {
                isOpen = false;
            }
        }
    }

    protected override void OnDraw()
    {
        Color fillNormal = new((byte)38, (byte)38, (byte)38, (byte)255);
        Color fillHover = new((byte)55, (byte)55, (byte)55, (byte)255);
        Color fillOpen = new((byte)28, (byte)50, (byte)88, (byte)255);
        Color borderNorm = new((byte)85, (byte)85, (byte)85, (byte)255);
        Color textCol = new((byte)200, (byte)200, (byte)200, (byte)255);

        Color fill = isOpen ? fillOpen : (hovered ? fillHover : fillNormal);

        Raylib.DrawRectangle((int)Position.X, (int)Position.Y, width, itemHeight, fill);
        Raylib.DrawRectangleLinesEx(new Rectangle(Position.X, Position.Y, width, itemHeight), 1f, borderNorm);

        int textY = (int)(Position.Y + (itemHeight - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute(SelectedOption, (int)Position.X + 8, textY, textCol, fontSize, Vector2.Zero);

        // Draw chevron icon
        int iconX = (int)Position.X + width - 15;
        if (isOpen)
        {
            Raylib.DrawLine(iconX, textY + 8, iconX + 4, textY + 2, textCol);
            Raylib.DrawLine(iconX + 4, textY + 2, iconX + 8, textY + 8, textCol);
        }
        else
        {
            Raylib.DrawLine(iconX, textY + 2, iconX + 4, textY + 8, textCol);
            Raylib.DrawLine(iconX + 4, textY + 8, iconX + 8, textY + 2, textCol);
        }
    }

    protected override void OnUpdate()
    {
        if (isOpen)
        {
            for (int i = 0; i < optionSelectables.Count; i++)
                optionSelectables[i].Update();
        }
    }

    protected override Drawable? OnChildrenHitTest(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        if (isOpen)
        {
            for (int i = optionSelectables.Count - 1; i >= 0; i--)
            {
                var hit = optionSelectables[i].HitTest(mouseScreenPosition, mouseWorldPosition);
                if (hit != null) return hit;
            }
        }

        return null;
    }

    protected override Rectangle OnGetInteractionRect()
    {
        return new Rectangle(Position.X, Position.Y, width, itemHeight);
    }

    protected override void OnDelete()
    {
        Engine.OnAnyPointerDown -= OnClickOff;
        foreach (var opt in optionSelectables) opt.Delete();
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.mouseButton != MouseButton.Left) return false;
        isOpen = !isOpen;
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        return evt.mouseButton == MouseButton.Left;
    }

    public void SetOnSelectionChanged(Action<Dropdown> onSelectionChanged)
    {
        this.onSelectionChanged = onSelectionChanged;
    }

    public void DrawOverlay()
    {
        if (isOpen)
        {
            Color borderNorm = new((byte)85, (byte)85, (byte)85, (byte)255);

            for (int i = 0; i < optionSelectables.Count; i++)
            {
                optionSelectables[i].RelativePosition = new Vector2(0, itemHeight + (i * itemHeight));
                optionSelectables[i].Render();
            }
            Raylib.DrawRectangleLinesEx(new Rectangle(Position.X, Position.Y + itemHeight, width, optionSelectables.Count * itemHeight), 1f, borderNorm);
        }
    }
}