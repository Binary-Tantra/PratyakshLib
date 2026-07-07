using Raylib_cs;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class VariablePanel : UILayoutBase
{
    private Selectable? currentSelected = null;
    private Button addButton;
    private Button removeButton;

    private Action<int?> onSelectVariable;
    private Action<int, string> onRenameVariable;

    private bool sendSelectNull = false;

    private int scrollId;

    public VariablePanel(int posX, int posY, Action<int?> onSelectVariable, Action onAddNewVariable, Action<int> onRemoveVariable, Action<int, string> onRenameVariable, Drawable? parent = null) : base(posX, posY, 220, 300, parent)
    {
        this.onSelectVariable = onSelectVariable;
        this.onRenameVariable = onRenameVariable;

        scrollId = IdGen.GetNewID();

        addButton = new Button(50, 20, "Add", (addBtn) =>
        {
            onAddNewVariable?.Invoke();
            Deselect();
        }, 0, parent: this);

        removeButton = new Button(50, 20, "Remove", (remBtn) =>
        {
            if (currentSelected != null)
            {
                Selectable selectionCache = currentSelected;
                Deselect();

                if (selectionCache.Payload != null)
                {
                    int id = (int)selectionCache.Payload;
                    onRemoveVariable?.Invoke(id);
                    layout.RemoveLayoutSelectable(id);
                }
            }
        }, 0, parent: this);

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

            layout.BeginScrollView(scrollId, layoutWidth - 10, layoutHeight - 50 - 40, 40, 5);
            {
                for (int i = 0; i < varIds.Count; i++)
                {
                    layout.BeginHorizontal(0);
                    {
                        Variable v = variables[varIds[i]];
                        string selectableText = v.VarName;

                        layout.Selectable(v.Id, selectableText, layoutWidth - 30, 24, OnVarUISelected, v.Id).OnTextEdited += (sel) =>
                        {
                            if (!Engine.Graph.RenameVariable((int)sel.Payload, sel.SelectableText))
                                sel.SetText(v.VarName);
                        };
                    }
                    layout.EndHorizontal(20);
                }
            }
            layout.EndScrollView();
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
