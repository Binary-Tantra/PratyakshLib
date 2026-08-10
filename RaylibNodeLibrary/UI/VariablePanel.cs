using Pratyaksh.Core;
using Pratyaksh.UI;
using Pratyaksh.UI.UIElements;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class VariablePanel : UILayoutBase
{
    private Selectable? currentSelected = null;
    private Button addButton;
    private Button removeButton;

    private Action<int?> onSelectVariable;
    private Action<int, string> onRenameVariable;
    private Action<int, DataType> onChangeVariableType;
    private Action<int, int, List<(string, object)>, Action<object>> requestSearchMenu;

    private bool sendSelectNull = false;

    private int scrollId;

    protected override string PanelName => "VariablePanel";

    public VariablePanel(int posX, int posY, Action<int?> onSelectVariable, Action onAddNewVariable, Action<int> onRemoveVariable, Action<int, string> onRenameVariable, Action<int, DataType> onChangeVariableType, Action<int, int, List<(string, object)>, Action<object>> requestSearchMenu, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, 220, 300, parent, parentBasis)
    {
        this.onSelectVariable = onSelectVariable;
        this.onRenameVariable = onRenameVariable;
        this.onChangeVariableType = onChangeVariableType;
        this.requestSearchMenu = requestSearchMenu;

        scrollId = IdGen.GetNewID();

        addButton = new Button(0, 0, 50, 20, "Add", (addBtn) =>
        {
            onAddNewVariable?.Invoke();
            Deselect();
        }, 0, parent: this);

        removeButton = new Button(0, 0, 50, 20, "Remove", (remBtn) =>
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

        Engine.Instance.InteractionManager.AnyPointerEvent += OnAnyPointerInput;
        GEngine.OnHandleInputComplete += OnHandleInputComplete;
    }

    private void Deselect()
    {
        currentSelected?.Deselect();
        currentSelected?.OnTextEdited = (sel) => { };
        
        currentSelected = null;
    }

    private void OnRenameSelectable(Selectable sel)
    {
        int varId = (int)sel.Payload;
        onRenameVariable?.Invoke(varId, sel.SelectableText);
    }

    protected override void OnDelete()
    {
        Engine.Instance.InteractionManager.AnyPointerEvent -= OnAnyPointerInput;
        GEngine.OnHandleInputComplete -= OnHandleInputComplete;
    }

    private void OnAnyPointerInput(PointerInteractEventData evt, EditorObject? target)
    {
        if (evt.MouseButton == MouseButton.Left && currentSelected != null)
        {
            if (target != null)
            {
                if (target == currentSelected || target.IsAncestor(currentSelected))
                    return;

                if (target == addButton || target == removeButton || target.IsAncestor(addButton) || target.IsAncestor(removeButton))
                    return;

                if (target.HasAncestorOfType<SearchMenu>() || target.HasAncestorOfType<InspectorPanel>())
                    return;

                if (currentSelected.Payload is int selectedVarId && target is Button btn && btn.Payload is int btnVarId && btnVarId == selectedVarId)
                    return;
            }

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
        currentSelected.OnTextEdited = OnRenameSelectable;
        sendSelectNull = false;
        onSelectVariable?.Invoke((int)varSel.Payload);
    }

    public override void OnDrawLayout()
    {
        Dictionary<int, Variable> variables = GEngine.Graph.Variables;
        List<int> varIds = [.. variables.Keys];

        layout.SectionEx("Variables", Width, Height - 50, Raylib_cs.Raylib.Fade(Raylib_cs.Color.DarkGray, 0.65f), Raylib_cs.Raylib.Fade(Raylib_cs.Color.Gray, 0.65f), Raylib_cs.Raylib.Fade(Raylib_cs.Color.White, 0.7f), 0.1f, false);

        layout.BeginHorizontal(0);
        {
            layout.AddSpace(10);

            layout.BeginScrollView(scrollId, Width - 10, Height - 50 - 40, 40, 5);
            {
                List<DataType> dataTypes = GEngine.Graph.Types.AllTypes.Where(t => t.Category == DataCategory.Data).ToList();
                string[] dataTypeNames = dataTypes.Select(t => t.Name).ToArray();

                for (int i = 0; i < varIds.Count; i++)
                {
                    layout.BeginHorizontal(0);
                    {
                        Variable v = variables[varIds[i]];
                        string selectableText = v.VarName;
                        bool isSelected = currentSelected != null && currentSelected.Payload is int selectedId && selectedId == v.Id;

                        layout.Selectable(v.Id, isSelected, selectableText, Width - 120, 24, OnVarUISelected, v.Id);

                        layout.Button(v.Id + 1000000, v.VarType.Name, 80, 24, (btn) =>
                        {
                            System.Numerics.Vector2 mp = Raylib_cs.Raylib.GetMousePosition();
                            List<(string name, object payload)> typeItems = dataTypes.Select(dt => (dt.Name, (object)dt)).ToList();

                            requestSearchMenu?.Invoke((int)mp.X, (int)mp.Y, typeItems, (payload) =>
                            {
                                onChangeVariableType?.Invoke(v.Id, (DataType)payload);
                            });

                        }, v.Id);
                    }
                    layout.EndHorizontal(24);
                }
            }
            layout.EndScrollView();
        }
        layout.EndHorizontal(Height - 50);

        layout.AddSpace(10);

        layout.BeginHorizontal(10);
        {
            layout.Button(addButton);
            layout.Button(removeButton);
        }
        layout.EndHorizontal(50);
    }
}
