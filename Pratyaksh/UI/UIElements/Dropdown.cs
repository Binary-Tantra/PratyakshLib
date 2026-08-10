using System.Numerics;
using Pratyaksh.Core;

namespace Pratyaksh.UI.UIElements;

public class Dropdown : UIBase, IPointerInteractable
{
    private string[] options;
    private int selectedIndex;

    private bool isOpen;

    private bool IsOpen
    {
        get => isOpen;
        set
        {
            if (isOpen != value)
            {
                isOpen = value;
                // height expands when open so the LayoutEngine moves elements below it.
                Size = new Vector2(Size.X, isOpen ? itemHeight + (options.Length * itemHeight) : itemHeight);
            }
        }
    }

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

    public Dropdown(string[] options, int selectedIndex, int posX, int posY, int width, int itemHeight, Action<Dropdown> onSelectionChanged, object payload, int fontSize = 15, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, width, itemHeight, parent, parentBasis)
    {
        selfInteractable = true;

        this.options = options ?? [];
        this.selectedIndex = selectedIndex;
        
        this.itemHeight = itemHeight;
        this.onSelectionChanged = onSelectionChanged;
        this.payload = payload;
        this.fontSize = fontSize;

        // Subscribe to global clicks to close the dropdown if the user clicks outside
        Engine.Instance.InteractionManager.AnyPointerEvent += OnClickOff;

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
            Selectable sel = new(options[i], i == selectedIndex, 0, itemHeight + (i * itemHeight), Width, itemHeight, (s) => OnOptionSelected(index), null, fontSize, parent: this);
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
        IsOpen = false;
        onSelectionChanged?.Invoke(this);
    }

    private void OnClickOff(PointerInteractEventData evt, EditorObject? target)
    {
        if (IsOpen && evt.MouseButton == MouseButton.Left)
        {
            // Close if clicked outside the dropdown header and options
            bool clickedInside = target == this || optionSelectables.Contains(target as Selectable);
            if (!clickedInside)
            {
                IsOpen = false;
            }
        }
    }

    protected override void OnDraw()
    {
        Raylib_cs.Color fillNormal = new((byte)38, (byte)38, (byte)38, (byte)255);
        Raylib_cs.Color fillHover = new((byte)55, (byte)55, (byte)55, (byte)255);
        Raylib_cs.Color fillOpen = new((byte)28, (byte)50, (byte)88, (byte)255);
        Raylib_cs.Color borderNorm = new((byte)85, (byte)85, (byte)85, (byte)255);
        Raylib_cs.Color textCol = new((byte)200, (byte)200, (byte)200, (byte)255);

        Raylib_cs.Color fill = IsOpen ? fillOpen : (hovered ? fillHover : fillNormal);

        Raylib_cs.Raylib.DrawRectangle((int)Position.X, (int)Position.Y, Width, itemHeight, fill);
        Raylib_cs.Raylib.DrawRectangleLinesEx(new Raylib_cs.Rectangle(Position.X, Position.Y, Width, itemHeight), 1f, borderNorm);

        int textY = (int)(Position.Y + (itemHeight - fontSize) / 2f);
        LayoutEngine.DrawTextAbsolute(SelectedOption, (int)Position.X + 8, textY, textCol, fontSize, Vector2.Zero);

        // Draw chevron icon
        int iconX = (int)Position.X + Width - 15;
        if (IsOpen)
        {
            Raylib_cs.Raylib.DrawLine(iconX, textY + 8, iconX + 4, textY + 2, textCol);
            Raylib_cs.Raylib.DrawLine(iconX + 4, textY + 2, iconX + 8, textY + 8, textCol);
        }
        else
        {
            Raylib_cs.Raylib.DrawLine(iconX, textY + 2, iconX + 4, textY + 8, textCol);
            Raylib_cs.Raylib.DrawLine(iconX + 4, textY + 8, iconX + 8, textY + 2, textCol);
        }
    }

    protected override void OnUpdate()
    {
        if (IsOpen)
        {
            for (int i = 0; i < optionSelectables.Count; i++)
                optionSelectables[i].Update();
        }
    }

    protected override Drawable? OnChildrenHitTest(IWorldToScreenTransformer transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        if (IsOpen)
        {
            for (int i = optionSelectables.Count - 1; i >= 0; i--)
            {
                var hit = optionSelectables[i].HitTest(transformer, mouseScreenPosition, mouseWorldPosition);
                if (hit != null) return hit;
            }
        }

        return null;
    }

    protected override void OnDelete()
    {
        Engine.Instance.InteractionManager.AnyPointerEvent -= OnClickOff;
        foreach (var opt in optionSelectables) opt.Delete();
    }

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.MouseButton != MouseButton.Left) return false;
        IsOpen = !IsOpen;
        return true;
    }

    public bool OnMouseUp(PointerInteractEventData evt)
    {
        return evt.MouseButton == MouseButton.Left;
    }

    public void SetOnSelectionChanged(Action<Dropdown> onSelectionChanged)
    {
        this.onSelectionChanged = onSelectionChanged;
    }

    public void SetSelectedIndexWithoutNotify(int index)
    {
        selectedIndex = index;
        for (int i = 0; i < optionSelectables.Count; i++)
        {
            optionSelectables[i].SetIsSelectedWithoutNotify(i == selectedIndex);
        }
    }

    public void DrawOverlay()
    {
        if (IsOpen)
        {
            Raylib_cs.Color borderNorm = new((byte)85, (byte)85, (byte)85, (byte)255);

            for (int i = 0; i < optionSelectables.Count; i++)
            {
                optionSelectables[i].RelativePosition = new Vector2(0, itemHeight + (i * itemHeight));
                optionSelectables[i].Render();
            }

            Raylib_cs.Raylib.DrawRectangleLinesEx(new Raylib_cs.Rectangle(Position.X, Position.Y + itemHeight, Width, optionSelectables.Count * itemHeight), 1f, borderNorm);
        }
    }
}