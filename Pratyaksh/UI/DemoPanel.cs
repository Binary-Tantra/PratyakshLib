using Pratyaksh.Core;
using Pratyaksh.UI.UIElements;

namespace Pratyaksh.UI;

public class DemoPanel : UILayoutBase
{
    private int selectedSpace = 15;
    private Raylib_cs.Color currentSelectedColor = Raylib_cs.Color.Black;

    private int scrollViewId;
    private int[] buttonIds;
    
    private int inputFieldId;
    private string fieldText;

    private int passwordFieldId;
    private string passwordText;

    private (int id, bool isSelected, Raylib_cs.Color color)[] selectableIds;
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
    private int sliderId;
    private float sliderValue = 65f;

    private Selectable? previousSelected = null;

    protected override string PanelName => "DemoPanel";

    public DemoPanel(int posX, int posY, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, 410, 480, parent, parentBasis)
    {
        scrollViewId = IdGen.GetNewID();
        buttonIds = [IdGen.GetNewID(), IdGen.GetNewID(), IdGen.GetNewID()];
        
        inputFieldId = IdGen.GetNewID();
        fieldText = "";

        passwordFieldId = IdGen.GetNewID();
        passwordText = "SecretPass123";
        
        selectableIds = [
                         (IdGen.GetNewID(), false, Raylib_cs.Color.Red),
                         (IdGen.GetNewID(), false, Raylib_cs.Color.SkyBlue),
                         (IdGen.GetNewID(), false, Raylib_cs.Color.Orange),
                         (IdGen.GetNewID(), false, Raylib_cs.Color.Green),
                         (IdGen.GetNewID(), false, Raylib_cs.Color.Magenta),
                         (IdGen.GetNewID(), false, Raylib_cs.Color.Yellow)
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
        sliderId = IdGen.GetNewID();
    }

    public override Dictionary<string, object?> GetSaveData()
    {
        return new Dictionary<string, object?>
        {
            ["fieldText"] = fieldText,
            ["passwordText"] = passwordText,
            ["cycleSelectedIdx"] = cycleSelectedIdx,
            ["dropdownSelectedIdx"] = dropdownSelectedIdx,
            ["selectedSpace"] = selectedSpace,
            ["sliderValue"] = sliderValue,
            ["selectableStates"] = selectableIds.Select(s => s.isSelected).ToList()
        };
    }

    public override void RestoreSaveData(System.Text.Json.JsonElement data)
    {
        if (data.ValueKind != System.Text.Json.JsonValueKind.Object) return;

        if (data.TryGetProperty("fieldText", out var ft) && ft.ValueKind == System.Text.Json.JsonValueKind.String)
            fieldText = ft.GetString() ?? "";

        if (data.TryGetProperty("passwordText", out var pt) && pt.ValueKind == System.Text.Json.JsonValueKind.String)
            passwordText = pt.GetString() ?? "";

        if (data.TryGetProperty("cycleSelectedIdx", out var cs) && cs.ValueKind == System.Text.Json.JsonValueKind.Number)
            cycleSelectedIdx = cs.GetInt32();

        if (data.TryGetProperty("dropdownSelectedIdx", out var dd) && dd.ValueKind == System.Text.Json.JsonValueKind.Number)
            dropdownSelectedIdx = dd.GetInt32();

        if (data.TryGetProperty("selectedSpace", out var ss) && ss.ValueKind == System.Text.Json.JsonValueKind.Number)
            selectedSpace = ss.GetInt32();

        if (data.TryGetProperty("sliderValue", out var sv) && sv.ValueKind == System.Text.Json.JsonValueKind.Number)
            sliderValue = sv.GetSingle();

        if (data.TryGetProperty("selectableStates", out var selArray) && selArray.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            int idx = 0;
            foreach (var item in selArray.EnumerateArray())
            {
                if (idx < selectableIds.Length && item.ValueKind is System.Text.Json.JsonValueKind.True or System.Text.Json.JsonValueKind.False)
                {
                    bool isSel = item.GetBoolean();
                    selectableIds[idx].isSelected = isSel;
                    if (isSel) currentSelectedColor = selectableIds[idx].color;
                }
                idx++;
            }
        }
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

        (int id, _, Raylib_cs.Color color) = selectableIds[(int)sel.Payload];

        selectableIds[(int)sel.Payload] = (id, sel.IsSelected, color);
        currentSelectedColor = color;
        
        Console.WriteLine($"[DemoPanel] Selectable Chosen: {sel.SelectableText} | Payload: {sel.Payload}");

        previousSelected = sel;
    }

    public override void OnDrawLayout()
    {
        // 1. Draw the main Section background and header
        layout.SectionEx("Layout Engine Showcase", Width, Height,
            Raylib_cs.Raylib.Fade(Raylib_cs.Color.DarkBlue, 0.7f),
            Raylib_cs.Raylib.Fade(Raylib_cs.Color.Gray, 0.65f),
            Raylib_cs.Color.White, 0.08f, false);

        layout.BeginScrollView(scrollViewId, Width, 440, 40, 10);
        {
            layout.AddSpace(5);
            
            // --- Alert Banner Demonstration ---
            layout.AlertBanner(alertBannerId, "System Status: All services operational", AlertType.Success, Width - 30, 28);
            
            layout.AddSpace(10);

            // --- Status Badges ---
            layout.BeginHorizontalEx(10, (int)Position.X + 10);
            {
                layout.StatusBadge(statusBadgeId1, "Active", StatusType.Active);
                layout.AddSpace(15);
                layout.StatusBadge(statusBadgeId2, "Processing", StatusType.Processing);
                layout.AddSpace(15);
                layout.LinkButton(linkBtnId, "Open GitHub Repo", "https://github.com/Binary-Tantra/PratyakshLib");
            }
            layout.EndHorizontal(24);

            layout.AddSpace(10);
            
            // --- A simple text element ---
            layout.Text("Welcome to the Extended UI Demo Panel!", Raylib_cs.Color.Gold);
            
            layout.AddSpace(5);

            // Colored Custom Buttons Demonstration
            layout.BeginHorizontalEx(10, (int)Position.X + 10);
            {
                layout.Button(customBtnId1, "Primary", 85, 25, OnDemoButtonPressed, 15, new Raylib_cs.Color((byte)28, (byte)100, (byte)200, (byte)255), Raylib_cs.Color.SkyBlue, Raylib_cs.Color.White);
                layout.Button(customBtnId2, "Danger", 85, 25, OnDemoButtonPressed, 30, new Raylib_cs.Color((byte)180, (byte)40, (byte)40, (byte)255), Raylib_cs.Color.Red, Raylib_cs.Color.White);
                layout.Button(buttonIds[2], "Default", 85, 25, OnDemoButtonPressed, 45);
            }
            layout.EndHorizontal(25);
            
            layout.AddSpace(10);

            // --- Cycle Selector ---
            layout.BeginHorizontalEx(50, (int)Position.X + 10);
            {
                layout.Text("Theme Mode: ", Raylib_cs.Color.White);
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
                layout.Text("User Name: ", Raylib_cs.Color.White);
                layout.AddSpace(10);
                layout.InputField(inputFieldId, "Type name...", fieldText, 140, 24, (inpf) => fieldText = inpf.InputFieldText);
            }
            layout.EndHorizontal(25);

            layout.AddSpace(5);

            layout.BeginHorizontalEx(50, (int)Position.X + 10);
            {
                layout.Text("Password:  ", Raylib_cs.Color.White);
                layout.AddSpace(10);
                layout.InputField(passwordFieldId, "Password...", passwordText, 140, 24, (inpf) => passwordText = inpf.InputFieldText, isMasked: true);
            }
            layout.EndHorizontal(25);

            layout.AddSpace(10);

            // --- Slider Demonstration ---
            layout.BeginHorizontalEx(50, (int)Position.X + 10);
            {
                layout.Text("Volume:    ", Raylib_cs.Color.White);
                layout.AddSpace(10);
                layout.Slider(sliderId, sliderValue, 0f, 100f, 140, 20, (sl) => sliderValue = sl.Value, format: "{0:0}%", step: 1f);
            }
            layout.EndHorizontal(22);

            layout.AddSpace(10);

            // --- Text Truncation Demonstration ---
            layout.BeginHorizontalEx(50, (int)Position.X + 10);
            {
                layout.Text("Long Title: ", Raylib_cs.Color.Gray);
                layout.AddSpace(5);
                layout.TextTruncated("This is a very long text string that will truncate nicely when exceeding width limit", 240, Raylib_cs.Color.LightGray);
            }
            layout.EndHorizontal(20);

            layout.AddSpace(10);

            // --- Nested Selectables & Dropdown ---
            layout.TextPanelPro("Dropdown & Custom Selectables", Width - 30, 25, Raylib_cs.Color.DarkGreen, Raylib_cs.Color.White);

            layout.AddSpace(10);

            int height = 0;
            layout.BeginHorizontalEx(10, (int)Position.X + 10);
            {
                layout.Text("Resolution: ", Raylib_cs.Color.White);
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