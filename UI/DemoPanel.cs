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

    private int passwordFieldId;
    private string passwordText;

    private (int id, bool isSelected, Color color)[] selectableIds;
    private int scrollView2Id;

    private int dropdownId;
    private int dropdownSelectedIdx;

    private int cycleSelectorId;
    private int cycleSelectedIdx = 0;

    private int customBtnId1;
    private int customBtnId2;
    private int linkBtnId;
    private int statusBadgeId1;
    private int statusBadgeId2;
    private int alertBannerId;

    private Selectable? previousSelected = null;

    public DemoPanel(int posX, int posY, Drawable? parent = null) : base(posX, posY, 410, 480, parent)
    {
        scrollViewId = IdGen.GetNewID();
        buttonIds = [IdGen.GetNewID(), IdGen.GetNewID(), IdGen.GetNewID()];
        
        inputFieldId = IdGen.GetNewID();
        fieldText = "";

        passwordFieldId = IdGen.GetNewID();
        passwordText = "SecretPass123";
        
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

        cycleSelectorId = IdGen.GetNewID();
        customBtnId1 = IdGen.GetNewID();
        customBtnId2 = IdGen.GetNewID();
        linkBtnId = IdGen.GetNewID();
        statusBadgeId1 = IdGen.GetNewID();
        statusBadgeId2 = IdGen.GetNewID();
        alertBannerId = IdGen.GetNewID();
    }

    // Callbacks for our interactive elements
    private void OnDemoButtonPressed(Button btn)
    {
        selectedSpace = (int)btn.Payload;
        Console.WriteLine($"[DemoPanel] Button Pressed: {btn.ButtonText} | Payload: {btn.Payload}");
    }

    private void OnDemoSelectablePressed(Selectable sel)
    {
        if (previousSelected != sel && previousSelected != null)
        {
            int prevIdx = (int)previousSelected.Payload;
            
            previousSelected?.Deselect();
            selectableIds[prevIdx].isSelected = false;
        }

        (int id, _, Color color) = selectableIds[(int)sel.Payload];

        selectableIds[(int)sel.Payload] = (id, sel.IsSelected, color);
        currentSelectedColor = color;
        
        Console.WriteLine($"[DemoPanel] Selectable Chosen: {sel.SelectableText} | Payload: {sel.Payload}");

        previousSelected = sel;
    }

    public override void OnDrawLayout()
    {
        // 1. Draw the main Section background and header
        layout.SectionEx("Layout Engine Showcase", layoutWidth, layoutHeight,
            Raylib.Fade(Color.DarkBlue, 0.7f),
            Raylib.Fade(Color.Gray, 0.65f),
            Color.White, 0.08f, false);

        layout.BeginScrollView(scrollViewId, layoutWidth, 440, 40, 10);
        {
            layout.AddSpace(5);
            
            // --- Alert Banner Demonstration ---
            layout.AlertBanner(alertBannerId, "System Status: All services operational", AlertType.Success, layoutWidth - 30, 28);
            
            layout.AddSpace(10);

            // --- Status Badges ---
            layout.BeginHorizontalEx(10, (int)Position.X + 10);
            {
                layout.StatusBadge(statusBadgeId1, "Active", StatusType.Active);
                layout.AddSpace(15);
                layout.StatusBadge(statusBadgeId2, "Processing", StatusType.Processing);
                layout.AddSpace(15);
                layout.LinkButton(linkBtnId, "Open GitHub Repo", "https://github.com");
            }
            layout.EndHorizontal(24);

            layout.AddSpace(10);
            
            // --- A simple text element ---
            layout.Text("Welcome to the Extended UI Demo Panel!", Color.Gold);
            
            layout.AddSpace(5);

            // Colored Custom Buttons Demonstration
            layout.BeginHorizontalEx(10, (int)Position.X + 10);
            {
                layout.Button(customBtnId1, "Primary", 85, 25, OnDemoButtonPressed, 15, new Color((byte)28, (byte)100, (byte)200, (byte)255), Color.SkyBlue, Color.White);
                layout.Button(customBtnId2, "Danger", 85, 25, OnDemoButtonPressed, 30, new Color((byte)180, (byte)40, (byte)40, (byte)255), Color.Red, Color.White);
                layout.Button(buttonIds[2], "Default", 85, 25, OnDemoButtonPressed, 45);
            }
            layout.EndHorizontal(25);
            
            layout.AddSpace(10);

            // --- Cycle Selector ---
            layout.BeginHorizontalEx(50, (int)Position.X + 10);
            {
                layout.Text("Theme Mode: ", Color.White);
                layout.AddSpace(15);
                layout.CycleSelector(cycleSelectorId, ["Dark Mode", "Light Mode", "High Contrast", "Cyberpunk"], cycleSelectedIdx, 160, 24, (cs) => {
                    cycleSelectedIdx = cs.SelectedIndex;
                });
            }
            layout.EndHorizontal(25);

            layout.AddSpace(10);

            // --- Form Inputs (Standard & Masked Password) ---
            layout.BeginHorizontalEx(50, (int)Position.X + 10);
            {
                layout.Text("User Name: ", Color.White);
                layout.AddSpace(10);
                layout.InputField(inputFieldId, "Type name...", fieldText, 140, 24, (inpf) => fieldText = inpf.InputFieldText);
            }
            layout.EndHorizontal(25);

            layout.AddSpace(5);

            layout.BeginHorizontalEx(50, (int)Position.X + 10);
            {
                layout.Text("Password:  ", Color.White);
                layout.AddSpace(10);
                layout.InputField(passwordFieldId, "Password...", passwordText, 140, 24, (inpf) => passwordText = inpf.InputFieldText, isMasked: true);
            }
            layout.EndHorizontal(25);

            layout.AddSpace(10);

            // --- Text Truncation Demonstration ---
            layout.BeginHorizontalEx(50, (int)Position.X + 10);
            {
                layout.Text("Long Title: ", Color.Gray);
                layout.AddSpace(5);
                layout.TextTruncated("This is a very long text string that will truncate nicely when exceeding width limit", 240, Color.LightGray);
            }
            layout.EndHorizontal(20);

            layout.AddSpace(10);

            // --- Nested Selectables & Dropdown ---
            layout.TextPanelPro("Dropdown & Custom Selectables", layoutWidth - 30, 25, Color.DarkGreen, Color.White);

            layout.AddSpace(10);

            int height = 0;
            layout.BeginHorizontalEx(10, (int)Position.X + 10);
            {
                layout.Text("Resolution: ", Color.White);
                layout.AddSpace(50);
                height = layout.Dropdown(dropdownId, ["1920x1080", "2560x1440", "3840x2160"], dropdownSelectedIdx, 150, 24, (dd) =>
                {
                    dropdownSelectedIdx = dd.SelectedIndex;
                }, dropdownId).Height;
            }
            layout.EndHorizontal(height);

            layout.AddSpace(15);

            layout.BeginHorizontalEx(15, (int)Position.X + 10);
            {
                layout.BeginScrollView(scrollView2Id, 135, 110, 0, 5);
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

                // Right Column: Panel displaying selected color
                layout.Panel(175, 60, currentSelectedColor);
            }
            layout.EndHorizontal(110);
        }
        layout.EndScrollView();
    }
}