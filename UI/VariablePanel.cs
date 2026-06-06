using Raylib_cs;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class VariablePanel : UILayout
{
    private Selectable? currentSelected = null;
    private Button addButton;
    private Button removeButton;

    public VariablePanel(int posX, int posY, Drawable? parent = null) : base(posX, posY, 200, 300, parent)
    {
        addButton = new Button(50, 20, "Add", (addBtn) =>
        {
            Engine.Graph.AddVariable("New Var");
            currentSelected = null;
        }, 0);

        removeButton = new Button(50, 20, "Remove", (remBtn) =>
        {
            if (currentSelected != null)
            {
                int id = (int)currentSelected.Payload;
                
                layout.RemoveLayoutSelectable(id);
                Engine.Graph.RemoveVariable(id);
            }
            currentSelected = null;
        }, 0);

        Engine.OnAnyPointerDown += DeselectCurrent;
    }

    private void DeselectCurrent(PointerInteractEventData evt, EditorObject? target)
    {
        if (target != addButton && target != removeButton)
            currentSelected = null;
    }

    private void OnVarUISelected(Selectable varSel)
    {
        if (currentSelected != varSel)
            currentSelected?.Deselect();

        currentSelected = varSel;

        /*Variable? targetVar = Engine.Graph.GetVariable((int)varSel.Payload);

        if (targetVar != null)
            Console.WriteLine("Var pressed: " + targetVar);*/
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
