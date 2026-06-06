using Raylib_cs;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class VariablePanel : UILayout
{
    private Selectable? currentSelected = null;
    private Button addButton;
    private Button removeButton;

    private Action<int?> onSelectVariable;
    private Action<int, string> onRenameVariable;

    private bool sendSelectNull = false;

    public VariablePanel(int posX, int posY, Action<int?> onSelectVariable, Action onAddNewVariable, Action<int> onRemoveVariable, Action<int, string> onRenameVariable, Drawable? parent = null) : base(posX, posY, 200, 300, parent)
    {
        this.onSelectVariable = onSelectVariable;
        this.onRenameVariable = onRenameVariable;
        
        addButton = new Button(50, 20, "Add", (addBtn) =>
        {
            onAddNewVariable?.Invoke();
            Deselect();
        }, 0);

        removeButton = new Button(50, 20, "Remove", (remBtn) =>
        {
            if (currentSelected != null)
            {
                Deselect();

                int id = (int)currentSelected.Payload;
                onRemoveVariable?.Invoke(id);
                layout.RemoveLayoutSelectable(id);
            }
        }, 0);

        Engine.OnAnyPointerDown += OnAnyPointerInput;
        Engine.OnHandleInputComplete += OnHandleInputComplete;
    }

    private void Deselect()
    {
        currentSelected?.Deselect();
        currentSelected?.OnTextEdited -= OnRenameSelectable;
        currentSelected = null;
    }

    private void OnRenameSelectable(Selectable sel)
    {
        int varId = (int)sel.Payload;
        onRenameVariable?.Invoke(varId, sel.SelectableText);
    }

    protected override void OnDelete()
    {
        Engine.OnAnyPointerDown -= OnAnyPointerInput;
        Engine.OnHandleInputComplete -= OnHandleInputComplete;
    }

    private void OnAnyPointerInput(PointerInteractEventData evt, EditorObject? target)
    {
        if (evt.mouseButton == MouseButton.Left && target != addButton && target != removeButton && target is Button b && b.ButtonText == "")
        {
            Deselect();
            sendSelectNull = true;
        }
    }

    private void OnHandleInputComplete()
    {
        if (sendSelectNull)
        {
            onSelectVariable?.Invoke(null);
            sendSelectNull = false;
        }
    }

    private void OnVarUISelected(Selectable varSel)
    {
        if (currentSelected != varSel)
            Deselect();

        currentSelected = varSel;
        currentSelected.OnTextEdited += OnRenameSelectable;
        onSelectVariable?.Invoke((int)varSel.Payload);
    }

    public override void OnDrawLayout()
    {
        Dictionary<int, Variable> variables = Engine.Graph.Variables;
        List<int> varIds = [.. variables.Keys];

        layout.SectionEx("Variables", layoutWidth, layoutHeight - 50, Raylib.Fade(Color.DarkGray, 0.65f), Raylib.Fade(Color.Gray, 0.65f), Raylib.Fade(Color.White, 0.7f), 0.1f, false);

        layout.BeginHorizontal(0);
        {
            layout.AddSpace(10);

            layout.BeginVerticalEx(5, (int)Position.Y + 40);
            {
                for (int i = 0; i < varIds.Count; i++)
                {
                    layout.BeginHorizontal(0);
                    {
                        Variable v = variables[varIds[i]];
                        string selectableText = v.VarName;

                        layout.Selectable(v.Id, selectableText, 150, 20, OnVarUISelected, v.Id).OnTextEdited += (sel) =>
                        {
                            if (!Engine.Graph.RenameVariable((int)sel.Payload, sel.SelectableText))
                                sel.SetText(v.VarName);
                        };
                    }
                    layout.EndHorizontal(20);
                }
            }
            layout.EndVertical(layoutWidth);
        }
        layout.EndHorizontal(layoutHeight - 50);

        layout.AddSpace(10);

        layout.BeginHorizontal(10);
        {
            layout.Button(addButton);
            layout.Button(removeButton);
        }
        layout.EndHorizontal(50);
    }
}
