using Raylib_cs;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class DemoPanel : UILayoutBase
{
    private int selectedSpace = 15;
    private Color currentSelectedColor = Color.Black;

    private int scrollViewId;
    private int[] buttonIds;
    
    private int inputFieldId;
    private string fieldText;

    private (int id, bool isSelected, Color color)[] selectableIds;
    private int scrollView2Id;

    private int dropdownId;
    private int dropdownSelectedIdx;

    public DemoPanel(int posX, int posY, Drawable? parent = null) : base(posX, posY, 400, 410, parent)
    {
        scrollViewId = IdGen.GetNewID();
        buttonIds = [IdGen.GetNewID(), IdGen.GetNewID(), IdGen.GetNewID()];
        
        inputFieldId = IdGen.GetNewID();
        fieldText = "";
        
        selectableIds = [
                         (IdGen.GetNewID(), false, Color.Red),
                         (IdGen.GetNewID(), false, Color.SkyBlue),
                         (IdGen.GetNewID(), false, Color.Orange),
                         (IdGen.GetNewID(), false, Color.Green),
                         (IdGen.GetNewID(), false, Color.Magenta),
                         (IdGen.GetNewID(), false, Color.Yellow)
                        ];
        
        scrollView2Id = IdGen.GetNewID();
        
        dropdownId = IdGen.GetNewID();
        dropdownSelectedIdx = 0;
    }

    // Callbacks for our interactive elements
    private void OnDemoButtonPressed(Button btn)
    {
        selectedSpace = (int)btn.Payload;
        Console.WriteLine($"[DemoPanel] Button Pressed: {btn.ButtonText} | Payload: {btn.Payload}");
    }

    private void OnDemoSelectablePressed(Selectable sel)
    {
        (int id, _, Color color) = selectableIds[(int)sel.Payload];
        
        selectableIds[(int)sel.Payload] = (id, sel.IsSelected, color);
        currentSelectedColor = color;
        
        Console.WriteLine($"[DemoPanel] Selectable Chosen: {sel.SelectableText} | Payload: {sel.Payload}");
    }

    public override void OnDrawLayout()
    {
        // 1. Draw the main Section background and header (using 10% of height for the header)
        layout.SectionEx("Layout Engine Showcase", layoutWidth, layoutHeight,
            Raylib.Fade(Color.DarkBlue, 0.7f),
            Raylib.Fade(Color.Gray, 0.65f),
            Color.White, 0.1f, false);
        
        // Calculate the starting Y position just below the header (header is 10% of 350 = 35px)
        int startY = (int)Position.Y + 40;

        layout.BeginScrollView(scrollViewId, layoutWidth, 370, 40, 10);
        {
            layout.AddSpace(5);
            
            // --- A simple text element ---
            layout.Text("Welcome to the Demo Panel!", Color.Gold);
            
            layout.AddSpace(5);

            // We add a little margin on the left
            layout.BeginHorizontalEx(10, (int)Position.X + 10);
            {
                layout.Button(buttonIds[0], "Space = 15", 90, 25, OnDemoButtonPressed, 15);
                layout.Button(buttonIds[1], "Space = 30", 90, 25, OnDemoButtonPressed, 30);
                layout.Button(buttonIds[2], "Space = 45", 90, 25, OnDemoButtonPressed, 45);
            }
            layout.EndHorizontal(25);
            
            layout.AddSpace(10);

            // --- Horizontal Row 2: Form Input ---
            layout.BeginHorizontalEx(10, (int)Position.X + 10);
            {
                layout.Text("Enter Name: ", Color.White);
                layout.AddSpace(100);
                layout.InputField(inputFieldId, "Type here...", fieldText, 200, 25, (inpf) =>
                {
                    fieldText = inpf.InputFieldText;
                });
            }
            layout.EndHorizontal(25);

            layout.AddSpace(selectedSpace);

            // --- Horizontal Row 3: Nested Layouts & Panels ---
            layout.TextPanelPro("Nested Selectables & Colored Panel", layoutWidth - 20, 25, Color.DarkGreen, Color.White);

            layout.AddSpace(15);

            int height = 0;
            layout.BeginHorizontalEx(10, (int)Position.X + 10);
            {
                layout.Text("Resolution: ", Color.White);
                layout.AddSpace(50);
                height = layout.Dropdown(dropdownId, ["1920x1080", "2560x1440", "3840x2160"], dropdownSelectedIdx, 150, 25, (dd) =>
                {
                    Console.WriteLine($"Dropdown updated -> Index: {dd.SelectedIndex} | Value: {dd.SelectedOption}");
                    dropdownSelectedIdx = dd.SelectedIndex;
                }, dropdownId).Height;
            }
            layout.EndHorizontal(height);

            layout.AddSpace(15);

            layout.BeginHorizontalEx(15, (int)Position.X + 10);
            {
                layout.BeginScrollView(scrollView2Id, 135, 120, 0, 5);
                {
                    layout.Selectable(selectableIds[0].id, selectableIds[0].isSelected, "Red", 120, 20, OnDemoSelectablePressed, 0);
                    layout.Selectable(selectableIds[1].id, selectableIds[1].isSelected, "Sky Blue", 120, 20, OnDemoSelectablePressed, 1);
                    layout.Selectable(selectableIds[2].id, selectableIds[2].isSelected, "Orange", 120, 20, OnDemoSelectablePressed, 2);
                    layout.Selectable(selectableIds[3].id, selectableIds[3].isSelected, "Green", 120, 20, OnDemoSelectablePressed, 3);
                    layout.Selectable(selectableIds[4].id, selectableIds[4].isSelected, "Magenta", 120, 20, OnDemoSelectablePressed, 4);
                    layout.Selectable(selectableIds[5].id, selectableIds[5].isSelected, "Yellow", 120, 20, OnDemoSelectablePressed, 5);
                }
                layout.EndScrollView();

                layout.AddSpace(20);

                // Right Column: A simple panel displaying the selected color.
                layout.Panel(175, 70, currentSelectedColor);
            }
            layout.EndHorizontal(120);
        }
        layout.EndScrollView();
    }
}