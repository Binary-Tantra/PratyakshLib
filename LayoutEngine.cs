using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

using RaylibNodeLibrary;
using RaylibNodeLibrary.UI;

namespace LibLayoutEngine;

public enum LayoutOpType
{
    Horizontal, Vertical
}

public struct LayoutOperation
{
    public int CurrentPos { get; set; }
    public int PosModifier { get; set; }
    public LayoutOpType OpType { get; set; }
    public int PosModifiedCount { get; set; }
}

public class LayoutEngine
{
    private LayoutOperation[] layoutOps = new LayoutOperation[20];

    private int layoutOpsIdx = -1;
    private int lastHorizontalIdx = -1;
    private int lastVerticalIdx = -1;

    private static Font textFont;

    private Dictionary<int, Button> layoutButtons = [];
    private Dictionary<int, Selectable> layoutSelectables = [];
    private Dictionary<int, InputField> layoutInputFields = [];
    private Dictionary<int, Toggle> layoutToggles = [];
    private Dictionary<int, ScrollView> layoutScrollViews = [];
    private Dictionary<int, Dropdown> layoutDropdowns = [];
    private Dictionary<int, CycleSelector> layoutCycleSelectors = [];
    private Dictionary<int, LinkButton> layoutLinkButtons = [];
    private Dictionary<int, StatusBadge> layoutStatusBadges = [];
    private Dictionary<int, AlertBanner> layoutAlertBanners = [];

    private Dictionary<int, RaylibNodeLibrary.DataBinding.BoolToggleBinder> layoutToggleBinders = [];
    private Dictionary<int, RaylibNodeLibrary.DataBinding.StringInputBinder> layoutInputFieldStringBinders = [];
    private Dictionary<int, RaylibNodeLibrary.DataBinding.IntInputBinder> layoutInputFieldIntBinders = [];
    private Dictionary<int, RaylibNodeLibrary.DataBinding.FloatInputBinder> layoutInputFieldFloatBinders = [];
    private Dictionary<int, RaylibNodeLibrary.DataBinding.BoolSelectableBinder> layoutSelectableBinders = [];
    private Dictionary<int, RaylibNodeLibrary.DataBinding.IntDropdownBinder> layoutDropdownBinders = [];

    private HashSet<int> activeInputFields = [];
    private HashSet<int> activeButtons = [];
    private HashSet<int> activeSelectables = [];
    private HashSet<int> activeToggles = [];
    private HashSet<int> activeScrollViewSet = [];
    private HashSet<int> activeDropdowns = [];
    private HashSet<int> activeCycleSelectors = [];
    private HashSet<int> activeLinkButtons = [];
    private HashSet<int> activeStatusBadges = [];
    private HashSet<int> activeAlertBanners = [];

    private HashSet<int> buildingActiveInputFields = [];
    private HashSet<int> buildingActiveButtons = [];
    private HashSet<int> buildingActiveSelectables = [];
    private HashSet<int> buildingActiveToggles = [];
    private HashSet<int> buildingActiveScrollViewSet = [];
    private HashSet<int> buildingActiveDropdowns = [];
    private HashSet<int> buildingActiveCycleSelectors = [];
    private HashSet<int> buildingActiveLinkButtons = [];
    private HashSet<int> buildingActiveStatusBadges = [];
    private HashSet<int> buildingActiveAlertBanners = [];

    public void BeginFrame()
    {
        buildingActiveInputFields.Clear();
        buildingActiveButtons.Clear();
        buildingActiveSelectables.Clear();
        buildingActiveToggles.Clear();
        buildingActiveScrollViewSet.Clear();
        buildingActiveDropdowns.Clear();
        buildingActiveCycleSelectors.Clear();
        buildingActiveLinkButtons.Clear();
        buildingActiveStatusBadges.Clear();
        buildingActiveAlertBanners.Clear();
    }

    public void EndFrame()
    {
        foreach (var kvp in layoutInputFields)
        {
            if (!buildingActiveInputFields.Contains(kvp.Key) && kvp.Value.IsFocused)
            {
                InteractionManager.ReleaseFocus();
            }
        }

        activeInputFields = [.. buildingActiveInputFields];
        activeButtons = [.. buildingActiveButtons];
        activeSelectables = [.. buildingActiveSelectables];
        activeToggles = [.. buildingActiveToggles];
        activeScrollViewSet = [.. buildingActiveScrollViewSet];
        activeDropdowns = [.. buildingActiveDropdowns];
        activeCycleSelectors = [.. buildingActiveCycleSelectors];
        activeLinkButtons = [.. buildingActiveLinkButtons];
        activeStatusBadges = [.. buildingActiveStatusBadges];
        activeAlertBanners = [.. buildingActiveAlertBanners];
    }

    private Stack<EditorObject?> parentStack = new();
    private Stack<int> activeScrollViews = new();

    EditorObject? defaultParent = null;

    public static void InitSLEDefaultFont(Font font)
    {
        textFont = font;
    }

    public LayoutEngine(EditorObject? defaultParent)
    {
        this.defaultParent = defaultParent;
    }

    public void ResetLayout()
    {
        layoutOpsIdx = -1;
        lastHorizontalIdx = -1;
        lastVerticalIdx = -1;

        foreach (var b in layoutToggleBinders.Values) b.Unbind();
        layoutToggleBinders.Clear();

        foreach (var b in layoutInputFieldStringBinders.Values) b.Unbind();
        layoutInputFieldStringBinders.Clear();

        foreach (var b in layoutInputFieldIntBinders.Values) b.Unbind();
        layoutInputFieldIntBinders.Clear();

        foreach (var b in layoutInputFieldFloatBinders.Values) b.Unbind();
        layoutInputFieldFloatBinders.Clear();

        foreach (var b in layoutSelectableBinders.Values) b.Unbind();
        layoutSelectableBinders.Clear();

        foreach (var b in layoutDropdownBinders.Values) b.Unbind();
        layoutDropdownBinders.Clear();

        List<Button> lbs = [.. layoutButtons.Select((kvp) => kvp.Value)];
        
        for (int i = 0; i < lbs.Count; i++)
            lbs[i].Delete();

        layoutButtons.Clear();


        List<Selectable> lss = [.. layoutSelectables.Select((kvp) => kvp.Value)];

        for (int i = 0; i < lss.Count; i++)
            lss[i].Delete();

        layoutSelectables.Clear();


        List<InputField> lifs = [.. layoutInputFields.Select((kvp) => kvp.Value)];

        for (int i = 0; i < lifs.Count; i++)
            lifs[i].Delete();

        layoutInputFields.Clear();


        List<Toggle> lts = [.. layoutToggles.Select((kvp) => kvp.Value)];
        
        for (int i = 0; i < lts.Count; i++)
            lts[i].Delete();
        
        layoutToggles.Clear();


        List<ScrollView> lsvs = [.. layoutScrollViews.Select((kvp) => kvp.Value)];
        
        for (int i = 0; i < lsvs.Count; i++)
            lsvs[i].Delete();
        
        layoutScrollViews.Clear();


        List<Dropdown> ldd = [.. layoutDropdowns.Select((kvp) => kvp.Value)];
        
        for (int i = 0; i < ldd.Count; i++)
            ldd[i].Delete();
        
        layoutDropdowns.Clear();


        List<CycleSelector> lcs = [.. layoutCycleSelectors.Select((kvp) => kvp.Value)];
        for (int i = 0; i < lcs.Count; i++)
            lcs[i].Delete();
        layoutCycleSelectors.Clear();


        List<LinkButton> llb = [.. layoutLinkButtons.Select((kvp) => kvp.Value)];
        for (int i = 0; i < llb.Count; i++)
            llb[i].Delete();
        layoutLinkButtons.Clear();


        List<StatusBadge> lsb = [.. layoutStatusBadges.Select((kvp) => kvp.Value)];
        for (int i = 0; i < lsb.Count; i++)
            lsb[i].Delete();
        layoutStatusBadges.Clear();


        List<AlertBanner> lab = [.. layoutAlertBanners.Select((kvp) => kvp.Value)];
        for (int i = 0; i < lab.Count; i++)
            lab[i].Delete();
        layoutAlertBanners.Clear();
    }

    public void UpdateLayoutElements()
    {
        List<Button> lbs = [.. layoutButtons.Where(kvp => activeButtons.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = 0; i < lbs.Count; i++)
            lbs[i].Update();

        List<Selectable> lss = [.. layoutSelectables.Where(kvp => activeSelectables.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = 0; i < lss.Count; i++)
            lss[i].Update();

        List<InputField> lifs = [.. layoutInputFields.Where(kvp => activeInputFields.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = 0; i < lifs.Count; i++)
            lifs[i].Update();

        List<Toggle> lts = [.. layoutToggles.Where(kvp => activeToggles.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = 0; i < lts.Count; i++)
            lts[i].Update();

        List<ScrollView> lsvs = [.. layoutScrollViews.Where(kvp => activeScrollViewSet.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = 0; i < lsvs.Count; i++)
            lsvs[i].Update();

        List<Dropdown> ldd = [.. layoutDropdowns.Where(kvp => activeDropdowns.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = 0; i < ldd.Count; i++)
            ldd[i].Update();

        List<CycleSelector> lcs = [.. layoutCycleSelectors.Where(kvp => activeCycleSelectors.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = 0; i < lcs.Count; i++)
            lcs[i].Update();

        List<LinkButton> llb = [.. layoutLinkButtons.Where(kvp => activeLinkButtons.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = 0; i < llb.Count; i++)
            llb[i].Update();

        List<StatusBadge> lsb = [.. layoutStatusBadges.Where(kvp => activeStatusBadges.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = 0; i < lsb.Count; i++)
            lsb[i].Update();

        List<AlertBanner> lab = [.. layoutAlertBanners.Where(kvp => activeAlertBanners.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = 0; i < lab.Count; i++)
            lab[i].Update();
    }

    public void RemoveLayoutButton(int id)
    {
        _ = layoutButtons.Remove(id);
    }

    public void RemoveLayoutSelectable(int id)
    {
        if (layoutSelectableBinders.TryGetValue(id, out var binder))
        {
            binder.Unbind();
            layoutSelectableBinders.Remove(id);
        }
        _ = layoutSelectables.Remove(id);
    }

    public void RemoveLayoutInputField(int id)
    {
        if (layoutInputFieldStringBinders.TryGetValue(id, out var sBinder))
        {
            sBinder.Unbind();
            layoutInputFieldStringBinders.Remove(id);
        }
        if (layoutInputFieldIntBinders.TryGetValue(id, out var iBinder))
        {
            iBinder.Unbind();
            layoutInputFieldIntBinders.Remove(id);
        }
        if (layoutInputFieldFloatBinders.TryGetValue(id, out var fBinder))
        {
            fBinder.Unbind();
            layoutInputFieldFloatBinders.Remove(id);
        }
        _ = layoutInputFields.Remove(id);
    }

    public InputField? GetInputField(int id)
    {
        layoutInputFields.TryGetValue(id, out InputField? inputField);
        return inputField;
    }

    public void RemoveLayoutToggle(int id)
    {
        if (layoutToggleBinders.TryGetValue(id, out var binder))
        {
            binder.Unbind();
            layoutToggleBinders.Remove(id);
        }
        _ = layoutToggles.Remove(id);
    }

    public void RemoveLayoutDropdown(int id)
    {
        if (layoutDropdownBinders.TryGetValue(id, out var binder))
        {
            binder.Unbind();
            layoutDropdownBinders.Remove(id);
        }
        _ = layoutDropdowns.Remove(id);
    }

    public void RemoveLayoutCycleSelector(int id)
    {
        _ = layoutCycleSelectors.Remove(id);
    }

    public void RemoveLayoutLinkButton(int id)
    {
        _ = layoutLinkButtons.Remove(id);
    }

    public void RemoveLayoutStatusBadge(int id)
    {
        _ = layoutStatusBadges.Remove(id);
    }

    public void RemoveLayoutAlertBanner(int id)
    {
        _ = layoutAlertBanners.Remove(id);
    }

    public Drawable? HitTestElements(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        List<AlertBanner> lab = [.. layoutAlertBanners.Where(kvp => activeAlertBanners.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = lab.Count - 1; i >= 0; i--)
        {
            var hit = lab[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        List<Dropdown> ldd = [.. layoutDropdowns.Where(kvp => activeDropdowns.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = ldd.Count - 1; i >= 0; i--)
        {
            var hit = ldd[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        List<CycleSelector> lcs = [.. layoutCycleSelectors.Where(kvp => activeCycleSelectors.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = lcs.Count - 1; i >= 0; i--)
        {
            var hit = lcs[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        List<LinkButton> llb = [.. layoutLinkButtons.Where(kvp => activeLinkButtons.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = llb.Count - 1; i >= 0; i--)
        {
            var hit = llb[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        List<Button> lbs = [.. layoutButtons.Where(kvp => activeButtons.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = lbs.Count - 1; i >= 0; i--)
        {
            var hit = lbs[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        List<Selectable> lss = [.. layoutSelectables.Where(kvp => activeSelectables.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = lss.Count - 1; i >= 0; i--)
        {
            var hit = lss[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        List<InputField> lifs = [.. layoutInputFields.Where(kvp => activeInputFields.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = lifs.Count - 1; i >= 0; i--)
        {
            var hit = lifs[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        List<Toggle> lts = [.. layoutToggles.Where(kvp => activeToggles.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = lts.Count - 1; i >= 0; i--)
        {
            var hit = lts[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        List<ScrollView> lsvs = [.. layoutScrollViews.Where(kvp => activeScrollViewSet.Contains(kvp.Key)).Select((kvp) => kvp.Value)];
        for (int i = lsvs.Count - 1; i >= 0; i--)
        {
            var hit = lsvs[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        return null;
    }

    public int LYOGetPos(int idx)
    {
        return layoutOps[idx].CurrentPos + layoutOps[idx].PosModifier * layoutOps[idx].PosModifiedCount;
    }

    public void LYOAddPos(int idx, int addAmt)
    {
        layoutOps[idx].CurrentPos += addAmt;
    }

    public int LYOGetPosAndUpdate(int idx)
    {
        return layoutOps[idx].CurrentPos + layoutOps[idx].PosModifier * layoutOps[idx].PosModifiedCount++;
    }

    public void LYOUpdate(int idx)
    {
        layoutOps[idx].PosModifiedCount++;
    }

    public int PosX()
    {
        if (lastHorizontalIdx == -1)
            return 0;
        else
            return LYOGetPos(lastHorizontalIdx);
    }

    public int PosY()
    {
        if (lastVerticalIdx == -1)
            return 0;
        else
            return LYOGetPos(lastVerticalIdx);
    }

    public int CurrentWidth()
    {
        int idx = -1;
        for (int i = 0; i < layoutOps.Length; i++)
        {
            if (layoutOps[i].OpType == LayoutOpType.Horizontal)
            {
                idx = i;
                break;
            }
        }

        if (idx == -1)
            return 0;

        return PosX() - LYOGetPos(idx);
    }

    public int PosX_Dynamic()
    {
        if (lastHorizontalIdx == -1)
            return 0;
        else
        {
            if (lastHorizontalIdx == layoutOpsIdx)
                return LYOGetPosAndUpdate(lastHorizontalIdx);
            else return LYOGetPos(lastHorizontalIdx);
        }
    }

    public int PosY_Dynamic()
    {
        if (lastVerticalIdx == -1)
            return 0;
        else
        {
            if (lastVerticalIdx == layoutOpsIdx)
                return LYOGetPosAndUpdate(lastVerticalIdx);
            else return LYOGetPos(lastVerticalIdx);
        }
    }

    private void AddNewLayoutOp(int pos, int dist, LayoutOpType type)
    {
        layoutOps[++layoutOpsIdx] = new LayoutOperation() { CurrentPos = pos, PosModifier = dist, OpType = type, PosModifiedCount = 0 };

        if (type == LayoutOpType.Horizontal)
            lastHorizontalIdx = layoutOpsIdx;
        else lastVerticalIdx = layoutOpsIdx;
    }

    private void RemoveLastLayoutOp()
    {
        if (layoutOpsIdx <= 0)
        {
            layoutOpsIdx = lastHorizontalIdx = lastVerticalIdx = -1;
        }
        else
        {
            if (lastHorizontalIdx == layoutOpsIdx)
            {
                lastHorizontalIdx = -1;

                for (int i = layoutOpsIdx - 1; i >= 0; i--)
                {
                    if (layoutOps[i].OpType == LayoutOpType.Horizontal)
                    {
                        lastHorizontalIdx = i;
                        break;
                    }
                }
            }
            else if (lastVerticalIdx == layoutOpsIdx)
            {
                lastVerticalIdx = -1;

                for (int i = layoutOpsIdx - 1; i >= 0; i--)
                {
                    if (layoutOps[i].OpType == LayoutOpType.Vertical)
                    {
                        lastVerticalIdx = i;
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Last horizontal and vertical indices were equal! This should never happen!");
                return;
            }

            layoutOpsIdx--;

            if (layoutOpsIdx != -1)
                layoutOps[layoutOpsIdx].PosModifiedCount++;
        }
    }

    public void DrawAny(int width, int height)
    {
        if (lastHorizontalIdx == lastVerticalIdx)
            return;
        else
        {
            if (lastHorizontalIdx < lastVerticalIdx)
                layoutOps[lastVerticalIdx].CurrentPos += height;
            else
                layoutOps[lastHorizontalIdx].CurrentPos += width;
        }
    }

    public static int MeasureTextW(string text, float fontSize)
    {
        return (int)MeasureTextEx(textFont, text, fontSize, 0.5f).X;
    }

    public static void DrawTextAbsolute(string text, int posX, int posY, Color fontColor, float fontSize, Vector2 offset)
    {
        DrawTextPro(textFont, text, new Vector2(posX + (int)offset.X, posY + (int)offset.Y), Vector2.Zero, 0, fontSize, 0.5f, fontColor);
    }

    public static void DrawTextAbsolute(string text, int posX, int posY, Color fontColor, Vector2 offset)
    {
        DrawTextEx(textFont, text, new Vector2(posX + (int)offset.X, posY + (int)offset.Y), 15, 0.5f, fontColor);
    }

    public static void DrawPanelAbsolute(int posX, int posY, int width, int height, Color panelColor)
    {
        DrawRectangle(posX, posY, width, height, panelColor);
    }

    public static void DrawTextPanelAbsolute(string text, int posX, int posY, int panelWidth, int panelHeight, int textSize, Color panelColor, Color textColor, Vector2 textOffset)
    {
        DrawPanelAbsolute(posX, posY, panelWidth, panelHeight, panelColor);
        DrawTextAbsolute(text, posX, posY, textColor, textSize, textOffset);
    }

    public static void DrawSectionAbsolute(string heading, int posX, int posY, int width, int heigth, float headingPercent, int headingSize, Color headingBgColor, Color bodyBgColor, Color fontColor)
    {
        int headingHeight = (int)(heigth * headingPercent);

        DrawPanelAbsolute(posX, posY + headingHeight, width, heigth - headingHeight, bodyBgColor);

        if (headingHeight != 0)
            DrawTextPanelAbsolute(heading, posX, posY, width, headingHeight, headingSize, headingBgColor, fontColor, new Vector2(5, 0));
        else
            DrawTextEx(textFont, heading, new Vector2(posX + 5, posY), headingSize, 0.5f, fontColor);
    }

    private void DrawButtonAbsolute(Button button, int posX, int posY)
    {
        buildingActiveButtons.Add(button.Id);
        if (!layoutButtons.TryGetValue(button.Id, out Button? cachedButton))
        {
            cachedButton = button;
            layoutButtons.Add(button.Id, cachedButton);
        }

        cachedButton.RelativePosition = new Vector2(posX, posY);
        cachedButton.Render();
    }

    private void DrawButtonAbsolute(int id, string buttonText, int posX, int posY, int buttonWidth, int buttonHeight, Action<Button> onButtonPressed, object payload, int fontSize, bool hasBorder, Color? fillColor = null, Color? borderColor = null, Color? textColor = null)
    {
        buildingActiveButtons.Add(id);
        if (!layoutButtons.TryGetValue(id, out Button? cachedButton))
        {
            cachedButton = new Button(buttonWidth, buttonHeight, buttonText, onButtonPressed, payload, fontSize, hasBorder, fillColor, borderColor, textColor, defaultParent);
            layoutButtons.Add(id, cachedButton);
        }
        else
        {
            cachedButton.ButtonText = buttonText;
            if (fillColor.HasValue) cachedButton.FillColor = fillColor;
            if (borderColor.HasValue) cachedButton.BorderColor = borderColor;
            if (textColor.HasValue) cachedButton.TextColor = textColor;
        }

        cachedButton.RelativePosition = new Vector2(posX, posY);
        cachedButton.Render();
    }

    private void DrawSelectableAbsolute(Selectable selectable, int posX, int posY)
    {
        buildingActiveSelectables.Add(selectable.Id);
        if (!layoutSelectables.TryGetValue(selectable.Id, out Selectable? cachedSelectable))
        {
            cachedSelectable = selectable;
            layoutSelectables.Add(selectable.Id, cachedSelectable);
        }

        cachedSelectable.RelativePosition = new Vector2(posX, posY);
        cachedSelectable.Render();
    }

    private Selectable DrawSelectableAbsolute(int id, bool isSelected, string selectableText, int posX, int posY, int selectableWidth, int selectableHeight, int fontSize, Action<Selectable> onSelectableSelect, object? payload, Color bgColor, Color bgSelectionColor, Color textColor)
    {
        buildingActiveSelectables.Add(id);
        if (!layoutSelectables.TryGetValue(id, out Selectable? cachedSelectable))
        {
            cachedSelectable = new Selectable(selectableText, isSelected, posX, posY, selectableWidth, selectableHeight, onSelectableSelect, payload, fontSize, bgColor, bgSelectionColor, textColor, defaultParent);
            layoutSelectables.Add(id, cachedSelectable);
        }
        else
        {
            if (isSelected != cachedSelectable.IsSelected)
            {
                if (isSelected) cachedSelectable.Select(false);
                else cachedSelectable.Deselect(false);
            }

            cachedSelectable.SelectableText = selectableText;
        }

        cachedSelectable.RelativePosition = new Vector2(posX, posY);
        cachedSelectable.Render();

        return cachedSelectable;
    }

    private void DrawInputFieldAbsolute(int id, string placeholderText, string fieldText, int posX, int posY, int inputFieldWidth, int inputFieldHeight, Action<InputField>? onTextEdited, Action<InputField>? onFocusEnd, int fontSize, bool isMasked = false)
    {
        buildingActiveInputFields.Add(id);
        if (!layoutInputFields.TryGetValue(id, out InputField? cachedInputField))
        {
            cachedInputField = new InputField(placeholderText, fieldText, posX, posY, inputFieldWidth, inputFieldHeight, onTextEdited, onFocusEnd, fontSize, isMasked, defaultParent);
            layoutInputFields.Add(id, cachedInputField);
        }
        else
        {
            if (!cachedInputField.IsFocused)
            {
                cachedInputField.InputFieldText = fieldText;
            }

            cachedInputField.IsMasked = isMasked;
            cachedInputField.OnTextChanged = onTextEdited;
            cachedInputField.OnFocusEnd = onFocusEnd;
        }

        cachedInputField.RelativePosition = new Vector2(posX, posY);
        cachedInputField.Render();
    }

    private void DrawToggleAbsolute(int id, bool toggleValue, string label, int posX, int posY, int toggleWidth, int toggleHeight, Action<Toggle>? onToggleChanged, object? payload)
    {
        buildingActiveToggles.Add(id);
        if (!layoutToggles.TryGetValue(id, out Toggle? cachedToggle))
        {
            cachedToggle = new Toggle(toggleValue, label, toggleWidth, toggleHeight, onToggleChanged, payload, 15, defaultParent);
            layoutToggles.Add(id, cachedToggle);
        }
        else
        {
            cachedToggle.Value = toggleValue;
            cachedToggle.Label = label;
            cachedToggle.SetOnToggleChanged(onToggleChanged);
        }

        cachedToggle.RelativePosition = new Vector2(posX, posY);
        cachedToggle.Render();
    }

    private void DrawDropdownAbsolute(int id, string[] options, int selectedIndex, int posX, int posY, int width, int itemHeight, Action<Dropdown>? onSelectionChanged, object? payload, int fontSize)
    {
        buildingActiveDropdowns.Add(id);
        if (!layoutDropdowns.TryGetValue(id, out Dropdown? cachedDropdown))
        {
            cachedDropdown = new Dropdown(options, selectedIndex, posX, posY, width, itemHeight, onSelectionChanged, payload, fontSize, defaultParent);
            layoutDropdowns.Add(id, cachedDropdown);
        }
        else
        {
            if (cachedDropdown.Options.Length != options.Length)
                cachedDropdown.SetOptions(options, selectedIndex);
            else
            {
                cachedDropdown.SelectedIndex = selectedIndex;
                cachedDropdown.SetOnSelectionChanged(onSelectionChanged);
            }
        }

        cachedDropdown.RelativePosition = new Vector2(posX, posY);
        cachedDropdown.Render();
    }

    private CycleSelector DrawCycleSelectorAbsolute(int id, string[] options, int selectedIndex, int posX, int posY, int width, int height, Action<CycleSelector>? onSelectionChanged, object? payload, int fontSize)
    {
        buildingActiveCycleSelectors.Add(id);
        if (!layoutCycleSelectors.TryGetValue(id, out CycleSelector? cachedCycle))
        {
            cachedCycle = new CycleSelector(options, selectedIndex, posX, posY, width, height, onSelectionChanged, payload, fontSize, defaultParent);
            layoutCycleSelectors.Add(id, cachedCycle);
        }
        else
        {
            cachedCycle.Options = options;
            cachedCycle.SetOnSelectionChanged(onSelectionChanged);
        }

        cachedCycle.RelativePosition = new Vector2(posX, posY);
        cachedCycle.Render();
        return cachedCycle;
    }

    private LinkButton DrawLinkButtonAbsolute(int id, string text, string url, int posX, int posY, Action<LinkButton>? onClick, int fontSize)
    {
        buildingActiveLinkButtons.Add(id);
        if (!layoutLinkButtons.TryGetValue(id, out LinkButton? cachedLink))
        {
            cachedLink = new LinkButton(text, url, onClick, fontSize, defaultParent);
            layoutLinkButtons.Add(id, cachedLink);
        }
        else
        {
            cachedLink.Text = text;
            cachedLink.Url = url;
        }

        cachedLink.RelativePosition = new Vector2(posX, posY);
        cachedLink.Render();
        return cachedLink;
    }

    private StatusBadge DrawStatusBadgeAbsolute(int id, string text, StatusType statusType, Color? customColor, int posX, int posY, int fontSize)
    {
        buildingActiveStatusBadges.Add(id);
        if (!layoutStatusBadges.TryGetValue(id, out StatusBadge? cachedBadge))
        {
            cachedBadge = new StatusBadge(text, statusType, customColor, fontSize, defaultParent);
            layoutStatusBadges.Add(id, cachedBadge);
        }
        else
        {
            cachedBadge.Text = text;
            cachedBadge.Type = statusType;
            if (customColor.HasValue) cachedBadge.CustomColor = customColor.Value;
        }

        cachedBadge.RelativePosition = new Vector2(posX, posY);
        cachedBadge.Render();
        return cachedBadge;
    }

    private AlertBanner DrawAlertBannerAbsolute(int id, string message, AlertType alertType, int posX, int posY, int width, int height, bool isDismissible, int fontSize)
    {
        buildingActiveAlertBanners.Add(id);
        if (!layoutAlertBanners.TryGetValue(id, out AlertBanner? cachedBanner))
        {
            cachedBanner = new AlertBanner(message, alertType, width, height, isDismissible, fontSize, defaultParent);
            layoutAlertBanners.Add(id, cachedBanner);
        }
        else
        {
            cachedBanner.Message = message;
            cachedBanner.Type = alertType;
        }

        cachedBanner.RelativePosition = new Vector2(posX, posY);
        cachedBanner.Render();
        return cachedBanner;
    }

    public void Text(string text, Color fontColor, bool updateLayout = true)
    {
        DrawTextAbsolute(text, PosX_Dynamic(), PosY_Dynamic(), fontColor, new Vector2(0, 0));
        if (updateLayout) DrawAny(text.Length * 2, 20);
    }

    public void Panel(int width, int height, Color panelColor, bool updateLayout = true)
    {
        DrawPanelAbsolute(PosX_Dynamic(), PosY_Dynamic(), width, height, panelColor);
        if (updateLayout) DrawAny(width, height);
    }

    public void TextPanelFixed(string text, int x, int y, int panelWidth, int panelHeight, Color panelColor, Color textColor, bool updateLayout = true)
    {
        PosX_Dynamic();
        PosY_Dynamic();

        DrawTextPanelAbsolute(text, x, y, panelWidth, panelHeight, 15, panelColor, textColor, new Vector2(5, 0));
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void TextPanelPro(string text, int panelWidth, int panelHeight, Color panelColor, Color textColor, bool updateLayout = true)
    {
        DrawTextPanelAbsolute(text, PosX_Dynamic(), PosY_Dynamic(), panelWidth, panelHeight, 15, panelColor, textColor, new Vector2(5, 0));
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void TextPanelEx(string text, int panelWidth, int panelHeight, Vector2 panelOffset, bool updateLayout = true)
    {
        DrawTextPanelAbsolute(text, PosX_Dynamic(), PosY_Dynamic(), panelWidth, panelHeight, 15, Color.Gray, Color.LightGray, panelOffset);
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void TextPanel(string text, int panelWidth, int panelHeight, bool updateLayout = true)
    {
        DrawTextPanelAbsolute(text, PosX_Dynamic(), PosY_Dynamic(), panelWidth, panelHeight, 15, Color.LightGray, Color.DarkGray, new Vector2(5, 0));
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void SectionEx(string heading, int width, int height, Color headingBgColor, Color bodyBgColor, Color fontColor, float headerPerc, bool updateLayout = true)
    {
        DrawSectionAbsolute(heading, PosX_Dynamic(), PosY_Dynamic(), width, height, headerPerc, 20, headingBgColor, bodyBgColor, fontColor);
        if (updateLayout) DrawAny(width, height);
    }

    public void Section(string heading, int width, int heigth, float headerPerc, bool updateLayout = true)
    {
        DrawSectionAbsolute(heading, PosX_Dynamic(), PosY_Dynamic(), width, heigth, headerPerc, 20, Color.DarkGray, Color.Gray, Color.LightGray);
        if (updateLayout) DrawAny(width, heigth);
    }

    public void Button(Button button, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        DrawButtonAbsolute(button, (int)pos.X, (int)pos.Y);
        if (updateLayout) DrawAny((int)button.Width, (int)button.Height);
    }

    public void Button(int id, string buttonText, int buttonWidth, int buttonHeight, Action<Button> onButtonPressed, object payload, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        DrawButtonAbsolute(id, buttonText, (int)pos.X, (int)pos.Y, buttonWidth, buttonHeight, onButtonPressed, payload, 15, true);
        if (updateLayout) DrawAny(buttonWidth, buttonHeight);
    }

    public void Button(int id, string buttonText, int buttonWidth, int buttonHeight, Action<Button> onButtonPressed, object payload, Color? fillColor, Color? borderColor = null, Color? textColor = null, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        DrawButtonAbsolute(id, buttonText, (int)pos.X, (int)pos.Y, buttonWidth, buttonHeight, onButtonPressed, payload, 15, true, fillColor, borderColor, textColor);
        if (updateLayout) DrawAny(buttonWidth, buttonHeight);
    }

    public void Selectable(Selectable selectable, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        DrawSelectableAbsolute(selectable, (int)pos.X, (int)pos.Y);
        if (updateLayout) DrawAny(selectable.Width, selectable.Height);
    }

    public Selectable Selectable(int id, bool isSelected, string selectableText, int selectableWidth, int selectableHeight, Action<Selectable> onSelectableSelect, object? payload, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        
        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;
        
        Selectable selectable = DrawSelectableAbsolute(id, isSelected, selectableText, (int)pos.X, (int)pos.Y, selectableWidth, selectableHeight, 15, onSelectableSelect, payload, new Color((byte)38, (byte)38, (byte)38, (byte)255), new Color((byte)28, (byte)50, (byte)88, (byte)255), new Color((byte)200, (byte)200, (byte)200, (byte)255));
        if (updateLayout) DrawAny(selectableWidth, selectableHeight);

        return selectable;
    }

    public void InputField(int id, string placeholderText, string fieldText, int inputFieldWidth, int inputFieldHeight, Action<InputField>? onTextEdited = null, Action<InputField>? onFocusEnd = null, bool isMasked = false, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        DrawInputFieldAbsolute(id, placeholderText, fieldText, (int)pos.X, (int)pos.Y, inputFieldWidth, inputFieldHeight, onTextEdited, onFocusEnd, 15, isMasked);
        if (updateLayout) DrawAny(inputFieldWidth, inputFieldHeight);
    }

    public void Toggle(int id, bool toggleValue, string label, int toggleWidth, int toggleHeight, Action<Toggle>? onToggleChanged, object? payload, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        DrawToggleAbsolute(id, toggleValue, label, (int)pos.X, (int)pos.Y, toggleWidth, toggleHeight, onToggleChanged, payload);
        if (updateLayout) DrawAny(toggleWidth, toggleHeight);
    }

    public Dropdown Dropdown(int id, string[] options, int selectedIndex, int width, int itemHeight, Action<Dropdown>? onSelectionChanged, object? payload, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null)
            pos -= defaultParent.Position;

        DrawDropdownAbsolute(id, options, selectedIndex, (int)pos.X, (int)pos.Y, width, itemHeight, onSelectionChanged, payload, 15);

        Dropdown cached = layoutDropdowns[id];

        // Core accordion logic: layout relies on the Dropdown reporting its expanded height.
        if (updateLayout) DrawAny(width, cached.Height);
        return cached;
    }

    public CycleSelector CycleSelector(int id, string[] options, int selectedIndex, int width, int height, Action<CycleSelector>? onSelectionChanged, object? payload = null, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;

        CycleSelector cached = DrawCycleSelectorAbsolute(id, options, selectedIndex, (int)pos.X, (int)pos.Y, width, height, onSelectionChanged, payload, 15);
        if (updateLayout) DrawAny(width, height);
        return cached;
    }

    public LinkButton LinkButton(int id, string text, string url, Action<LinkButton>? onClick = null, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;

        LinkButton cached = DrawLinkButtonAbsolute(id, text, url, (int)pos.X, (int)pos.Y, onClick, 14);
        int textW = MeasureTextW(text, 14);
        if (updateLayout) DrawAny(textW, 18);
        return cached;
    }

    public StatusBadge StatusBadge(int id, string text, StatusType statusType = StatusType.Idle, Color? customColor = null, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;

        StatusBadge cached = DrawStatusBadgeAbsolute(id, text, statusType, customColor, (int)pos.X, (int)pos.Y, 13);
        int textW = MeasureTextW(text, 13);
        int badgeW = textW + 24;
        int badgeH = 13 + 8;
        if (updateLayout) DrawAny(badgeW, badgeH);
        return cached;
    }

    public AlertBanner AlertBanner(int id, string message, AlertType alertType = AlertType.Error, int width = 360, int height = 32, bool isDismissible = true, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;

        AlertBanner cached = DrawAlertBannerAbsolute(id, message, alertType, (int)pos.X, (int)pos.Y, width, height, isDismissible, 13);
        if (updateLayout && !cached.IsDismissed) DrawAny(width, height);
        return cached;
    }

    public void TextTruncated(string text, int maxWidth, Color fontColor, int fontSize = 15, bool updateLayout = true)
    {
        string truncatedStr = text;
        int measuredW = MeasureTextW(text, fontSize);
        if (measuredW > maxWidth)
        {
            const string ellipsis = "...";
            int ellipsisW = MeasureTextW(ellipsis, fontSize);
            int availableW = maxWidth - ellipsisW;

            if (availableW > 0)
            {
                int len = text.Length;
                while (len > 0 && MeasureTextW(text.Substring(0, len), fontSize) > availableW)
                {
                    len--;
                }
                truncatedStr = text.Substring(0, len) + ellipsis;
            }
            else
            {
                truncatedStr = ellipsis;
            }
        }

        DrawTextAbsolute(truncatedStr, PosX_Dynamic(), PosY_Dynamic(), fontColor, fontSize, Vector2.Zero);
        if (updateLayout) DrawAny(Math.Min(measuredW, maxWidth), fontSize + 4);
    }

    public void BeginHorizontal(int spacingDist)
    {
        AddNewLayoutOp(PosX_Dynamic(), spacingDist, LayoutOpType.Horizontal);
    }

    public void BeginHorizontalEx(int spacingDist, int posXOverride)
    {
        AddNewLayoutOp(posXOverride, spacingDist, LayoutOpType.Horizontal);
    }

    public void EndHorizontal(int endHeight)
    {
        RemoveLastLayoutOp();
        DrawAny(0, endHeight);
    }

    public void BeginVertical(int spacingDist)
    {
        AddNewLayoutOp(PosY_Dynamic(), spacingDist, LayoutOpType.Vertical);
    }

    public void BeginVerticalEx(int spacingDist, int posYOverride)
    {
        AddNewLayoutOp(posYOverride, spacingDist, LayoutOpType.Vertical);
    }

    public void EndVertical(int endWidth)
    {
        RemoveLastLayoutOp();
        DrawAny(endWidth, 0);
    }

    // Add the spacing parameter to the method signature
    public void BeginScrollView(int id, int viewWidth, int viewHeight, int startYOffset = 0, int spacing = 0)
    {
        buildingActiveScrollViewSet.Add(id);
        if (!layoutScrollViews.TryGetValue(id, out ScrollView? svc))
        {
            svc = new ScrollView(viewWidth, viewHeight, defaultParent);
            layoutScrollViews.Add(id, svc);
        }

        svc.SetViewSize(new Vector2(viewWidth, viewHeight));
        svc.RelativePosition = new Vector2(PosX_Dynamic(), PosY_Dynamic() + startYOffset);

        if (defaultParent != null)
            svc.RelativePosition -= defaultParent.Position;

        Rectangle scissorRect = svc.GetScissorRect();

        float scissorEndX = scissorRect.X + scissorRect.Width;
        float scissorEndY = scissorRect.Y + scissorRect.Height;

        // Now we cut the scroll scissor according to current parent's scissor if current parent scissor is smaller than required scroll scissor XD
        float defaultParentEndX;
        float defaultParentEndY;

        if (defaultParent != null)
        {
            // TODO: Using interactable rect's width/height instead of visual's! For now it works.
            defaultParentEndX = defaultParent.Position.X + defaultParent.GetInteractableRect().Width;
            defaultParentEndY = defaultParent.Position.Y + defaultParent.GetInteractableRect().Height;
        }
        else defaultParentEndX = defaultParentEndY = float.PositiveInfinity;

        int scissorWidth;
        int scissorHeight;

        if (defaultParentEndX < scissorEndX)
            scissorWidth = (int)(scissorRect.Width - (scissorEndX - defaultParentEndX));
        else
            scissorWidth = (int)scissorRect.Width;

        if (defaultParentEndY < scissorEndY)
            scissorHeight = (int)(scissorRect.Height - (scissorEndY - defaultParentEndY));
        else
            scissorHeight = (int)scissorRect.Height;

        Raylib.BeginScissorMode((int)scissorRect.X, (int)scissorRect.Y, scissorWidth, scissorHeight);

        int startX = (int)svc.Position.X;
        int startY = (int)svc.Position.Y;

        // Pass the spacing to the internal vertical layout!
        BeginHorizontalEx(0, startX + (int)svc.ScrollOffset.X);
        BeginVerticalEx(spacing, startY + (int)svc.ScrollOffset.Y);

        parentStack.Push(defaultParent);
        defaultParent = svc;
        activeScrollViews.Push(id);
    }

    public void EndScrollView()
    {
        if (activeScrollViews.Count == 0) return;

        int svId = activeScrollViews.Pop();
        ScrollView svc = layoutScrollViews[svId];

        int contentWidth = PosX() - ((int)svc.Position.X + (int)svc.ScrollOffset.X);
        int contentHeight = PosY() - ((int)svc.Position.Y + (int)svc.ScrollOffset.Y);

        svc.SetContentSize(new Vector2(Math.Max(svc.ViewSize.X, contentWidth), Math.Max(svc.ViewSize.Y, contentHeight)));

        EndVertical(contentWidth);
        EndHorizontal(contentHeight);

        Raylib.EndScissorMode();

        defaultParent = parentStack.Pop();

        // Re-apply the parent's scissor rect to prevent leaking out of bounds
        if (defaultParent != null)
        {
            Rectangle pRect = defaultParent.GetInteractableRect();
            Raylib.BeginScissorMode((int)pRect.X, (int)pRect.Y, (int)pRect.Width, (int)pRect.Height);
        }

        svc.Render();
        DrawAny((int)svc.ViewSize.X, (int)svc.ViewSize.Y);
    }

    public void AddSpace(int space)
    {
        DrawAny(space, space);
    }

    public void NotifyDraw(int width, int height)
    {
        DrawAny(width, height);
    }

    public void DrawOverlays()
    {
        List<Dropdown> ldd = [.. layoutDropdowns.Values];
        for (int i = 0; i < ldd.Count; i++)
        {
            ldd[i].DrawOverlay();
        }
    }

    // ==================== BINDABLE DRAWING METHODS ====================

    public void DrawBindableToggleAbsolute(int id, RaylibNodeLibrary.DataBinding.BindableValueBase<bool> dataModel, string label, int posX, int posY, int toggleWidth, int toggleHeight)
    {
        buildingActiveToggles.Add(id);
        if (!layoutToggles.TryGetValue(id, out Toggle? cachedToggle))
        {
            cachedToggle = new Toggle(dataModel.Get(), label, toggleWidth, toggleHeight, null, id, 15, defaultParent);
            layoutToggles.Add(id, cachedToggle);

            var uiWrapper = new RaylibNodeLibrary.DataBinding.RLToggleUI(cachedToggle);
            var binder = new RaylibNodeLibrary.DataBinding.BoolToggleBinder();
            binder.Bind(dataModel, uiWrapper);
            layoutToggleBinders.Add(id, binder);
        }
        else
        {
            cachedToggle.Label = label;
            if (layoutToggleBinders.TryGetValue(id, out var binder))
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();
                    binder.Bind(dataModel, (RaylibNodeLibrary.DataBinding.RLToggleUI)binder.GetBoundUIObject()!);
                }
            }
        }

        cachedToggle.RelativePosition = new Vector2(posX, posY);
        cachedToggle.Render();
    }

    public void BindableToggle(int id, RaylibNodeLibrary.DataBinding.BindableValueBase<bool> dataModel, string label, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;
        DrawBindableToggleAbsolute(id, dataModel, label, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
    }

    public void DrawBindableInputFieldStringAbsolute(int id, string placeholderText, RaylibNodeLibrary.DataBinding.BindableValueBase<string> dataModel, int posX, int posY, int width, int height)
    {
        buildingActiveInputFields.Add(id);
        if (!layoutInputFields.TryGetValue(id, out InputField? cachedField))
        {
            cachedField = new InputField(placeholderText, dataModel.Get(), posX, posY, width, height, null, null, 15, false, defaultParent);
            layoutInputFields.Add(id, cachedField);

            var uiWrapper = new RaylibNodeLibrary.DataBinding.RLInputFieldUI_String(cachedField);
            var binder = new RaylibNodeLibrary.DataBinding.StringInputBinder();
            binder.Bind(dataModel, uiWrapper);
            layoutInputFieldStringBinders.Add(id, binder);
        }
        else
        {
            if (layoutInputFieldStringBinders.TryGetValue(id, out var binder))
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();
                    binder.Bind(dataModel, (RaylibNodeLibrary.DataBinding.RLInputFieldUI_String)binder.GetBoundUIObject()!);
                }
            }
        }

        cachedField.RelativePosition = new Vector2(posX, posY);
        cachedField.Render();
    }

    public void BindableInputFieldString(int id, string placeholderText, RaylibNodeLibrary.DataBinding.BindableValueBase<string> dataModel, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;
        DrawBindableInputFieldStringAbsolute(id, placeholderText, dataModel, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
    }

    public void DrawBindableInputFieldIntAbsolute(int id, string placeholderText, RaylibNodeLibrary.DataBinding.BindableValueBase<int> dataModel, int posX, int posY, int width, int height)
    {
        buildingActiveInputFields.Add(id);
        if (!layoutInputFields.TryGetValue(id, out InputField? cachedField))
        {
            cachedField = new InputField(placeholderText, dataModel.Get().ToString(), posX, posY, width, height, null, null, 15, false, defaultParent);
            layoutInputFields.Add(id, cachedField);

            var uiWrapper = new RaylibNodeLibrary.DataBinding.RLInputFieldUI_Int(cachedField);
            var binder = new RaylibNodeLibrary.DataBinding.IntInputBinder();
            binder.Bind(dataModel, uiWrapper);
            layoutInputFieldIntBinders.Add(id, binder);
        }
        else
        {
            if (layoutInputFieldIntBinders.TryGetValue(id, out var binder))
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();
                    binder.Bind(dataModel, (RaylibNodeLibrary.DataBinding.RLInputFieldUI_Int)binder.GetBoundUIObject()!);
                }
            }
        }

        cachedField.RelativePosition = new Vector2(posX, posY);
        cachedField.Render();
    }

    public void BindableInputFieldInt(int id, string placeholderText, RaylibNodeLibrary.DataBinding.BindableValueBase<int> dataModel, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;
        DrawBindableInputFieldIntAbsolute(id, placeholderText, dataModel, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
    }

    public void DrawBindableInputFieldFloatAbsolute(int id, string placeholderText, RaylibNodeLibrary.DataBinding.BindableValueBase<float> dataModel, int posX, int posY, int width, int height)
    {
        buildingActiveInputFields.Add(id);
        if (!layoutInputFields.TryGetValue(id, out InputField? cachedField))
        {
            cachedField = new InputField(placeholderText, dataModel.Get().ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture), posX, posY, width, height, null, null, 15, false, defaultParent);
            layoutInputFields.Add(id, cachedField);

            var uiWrapper = new RaylibNodeLibrary.DataBinding.RLInputFieldUI_Float(cachedField);
            var binder = new RaylibNodeLibrary.DataBinding.FloatInputBinder();
            binder.Bind(dataModel, uiWrapper);
            layoutInputFieldFloatBinders.Add(id, binder);
        }
        else
        {
            if (layoutInputFieldFloatBinders.TryGetValue(id, out var binder))
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();
                    binder.Bind(dataModel, (RaylibNodeLibrary.DataBinding.RLInputFieldUI_Float)binder.GetBoundUIObject()!);
                }
            }
        }

        cachedField.RelativePosition = new Vector2(posX, posY);
        cachedField.Render();
    }

    public void BindableInputFieldFloat(int id, string placeholderText, RaylibNodeLibrary.DataBinding.BindableValueBase<float> dataModel, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;
        DrawBindableInputFieldFloatAbsolute(id, placeholderText, dataModel, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
    }

    public void DrawBindableSelectableAbsolute(int id, RaylibNodeLibrary.DataBinding.BindableValueBase<bool> dataModel, string selectableText, int posX, int posY, int width, int height)
    {
        buildingActiveSelectables.Add(id);
        if (!layoutSelectables.TryGetValue(id, out Selectable? cachedSelectable))
        {
            cachedSelectable = new Selectable(selectableText, dataModel.Get(), posX, posY, width, height, null, id, 15, Color.Gray, Color.Blue, Color.White, defaultParent);
            layoutSelectables.Add(id, cachedSelectable);

            var uiWrapper = new RaylibNodeLibrary.DataBinding.RLSelectableUI(cachedSelectable);
            var binder = new RaylibNodeLibrary.DataBinding.BoolSelectableBinder();
            binder.Bind(dataModel, uiWrapper);
            layoutSelectableBinders.Add(id, binder);
        }
        else
        {
            cachedSelectable.SelectableText = selectableText;
            if (layoutSelectableBinders.TryGetValue(id, out var binder))
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();
                    binder.Bind(dataModel, (RaylibNodeLibrary.DataBinding.RLSelectableUI)binder.GetBoundUIObject()!);
                }
            }
        }

        cachedSelectable.RelativePosition = new Vector2(posX, posY);
        cachedSelectable.Render();
    }

    public void BindableSelectable(int id, RaylibNodeLibrary.DataBinding.BindableValueBase<bool> dataModel, string selectableText, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;
        DrawBindableSelectableAbsolute(id, dataModel, selectableText, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
    }

    public void DrawBindableDropdownAbsolute(int id, string[] options, RaylibNodeLibrary.DataBinding.BindableValueBase<int> dataModel, int posX, int posY, int width, int height)
    {
        buildingActiveDropdowns.Add(id);
        if (!layoutDropdowns.TryGetValue(id, out Dropdown? cachedDropdown))
        {
            cachedDropdown = new Dropdown(options, dataModel.Get(), posX, posY, width, height, null, id, 15, defaultParent);
            layoutDropdowns.Add(id, cachedDropdown);

            var uiWrapper = new RaylibNodeLibrary.DataBinding.RLDropdownUI(cachedDropdown);
            var binder = new RaylibNodeLibrary.DataBinding.IntDropdownBinder();
            binder.Bind(dataModel, uiWrapper);
            layoutDropdownBinders.Add(id, binder);
        }
        else
        {
            if (layoutDropdownBinders.TryGetValue(id, out var binder))
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();
                    binder.Bind(dataModel, (RaylibNodeLibrary.DataBinding.RLDropdownUI)binder.GetBoundUIObject()!);
                }
            }
        }

        cachedDropdown.RelativePosition = new Vector2(posX, posY);
        cachedDropdown.Render();
    }

    public void BindableDropdown(int id, string[] options, RaylibNodeLibrary.DataBinding.BindableValueBase<int> dataModel, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;
        DrawBindableDropdownAbsolute(id, options, dataModel, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
    }
}