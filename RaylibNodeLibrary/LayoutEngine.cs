using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

using RaylibNodeLibrary.DataBinding;
using System.Runtime.CompilerServices;

namespace RaylibNodeLibrary.UI;

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

public struct ElementInfo<UIType> where UIType : UIBase
{
    public UIType UIElement { get; set; }
    public bool IsActiveThisFrame { get; set; }
    public BinderBase? DataBinder { get; set; }

    public ElementInfo(UIType uiElement, BinderBase? dataBinder)
    {
        UIElement = uiElement;
        DataBinder = dataBinder;
    }

    public ElementInfo<UIType> Activate()
    {
        IsActiveThisFrame = true;
        return this;
    }

    public ElementInfo<UIType> Deactivate()
    {
        IsActiveThisFrame = false;
        return this;
    }
}

public class LayoutEngine
{
    private LayoutOperation[] layoutOps = new LayoutOperation[20];

    private int layoutOpsIdx = -1;
    private int lastHorizontalIdx = -1;
    private int lastVerticalIdx = -1;

    private static Font textFont;

    private Dictionary<int, ElementInfo<Button>> layoutButtons = [];
    private Dictionary<int, ElementInfo<Selectable>> layoutSelectables = [];
    private Dictionary<int, ElementInfo<InputField>> layoutInputFields = [];
    private Dictionary<int, ElementInfo<Toggle>> layoutToggles = [];
    private Dictionary<int, ElementInfo<ScrollView>> layoutScrollViews = [];
    private Dictionary<int, ElementInfo<Dropdown>> layoutDropdowns = [];
    private Dictionary<int, ElementInfo<CycleSelector>> layoutCycleSelectors = [];
    private Dictionary<int, ElementInfo<LinkButton>> layoutLinkButtons = [];
    private Dictionary<int, ElementInfo<StatusBadge>> layoutStatusBadges = [];
    private Dictionary<int, ElementInfo<AlertBanner>> layoutAlertBanners = [];

    private Stack<EditorObject?> parentStack = new();
    private Stack<int> activeScrollViews = new();

    EditorObject? defaultParent = null;

    private static void ActivateElements<T>(Dictionary<int, ElementInfo<T>> targetDict) where T : UIBase
    {
        foreach (var kvp in targetDict)
            targetDict[kvp.Key] = targetDict[kvp.Key].Activate();
    }

    private static void DeactivateElements<T>(Dictionary<int, ElementInfo<T>> targetDict) where T : UIBase
    {
        foreach (var kvp in targetDict)
            targetDict[kvp.Key] = targetDict[kvp.Key].Deactivate();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DeleteElement<T>(int id, Dictionary<int, ElementInfo<T>> targetDict) where T : UIBase
    {
        if (targetDict.TryGetValue(id, out var info))
        {
            info.DataBinder?.Unbind();
            info.UIElement.Delete();
            targetDict.Remove(id);
        }
    }

    private static void DeleteElements<T>(Dictionary<int, ElementInfo<T>> targetDict) where T : UIBase
    {
        foreach (var kvp in targetDict)
        {
            targetDict[kvp.Key].DataBinder?.Unbind();
            targetDict[kvp.Key].UIElement.Delete();
        }

        targetDict.Clear();
    }

    private static void UpdateActiveElements<T>(Dictionary<int, ElementInfo<T>> targetDict) where T : UIBase
    {
        foreach (var kvp in targetDict)
        {
            if (targetDict[kvp.Key].IsActiveThisFrame)
                targetDict[kvp.Key].UIElement.Update();
        }
    }

    private static Drawable? HitTestActiveElements<T>(Dictionary<int, ElementInfo<T>> targetDict, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition) where T : UIBase
    {
        var keys = targetDict.Keys.ToArray();
        for (int i = keys.Length - 1; i >= 0; i--)
        {
            int key = keys[i];
            if (targetDict[key].IsActiveThisFrame)
            {
                var hit = targetDict[key].UIElement.HitTest(mouseScreenPosition, mouseWorldPosition);
                if (hit != null) return hit;
            }
        }
        return null;
    }

    public void BeginFrame()
    {
        DeactivateElements(layoutButtons);
        DeactivateElements(layoutSelectables);
        DeactivateElements(layoutInputFields);
        DeactivateElements(layoutToggles);
        DeactivateElements(layoutScrollViews);
        DeactivateElements(layoutDropdowns);
        DeactivateElements(layoutCycleSelectors);
        DeactivateElements(layoutLinkButtons);
        DeactivateElements(layoutStatusBadges);
        DeactivateElements(layoutAlertBanners);
    }

    public void EndFrame()
    {
        foreach (var kvp in layoutInputFields)
        {
            if (!layoutInputFields[kvp.Key].IsActiveThisFrame && layoutInputFields[kvp.Key].UIElement.IsFocused)
            {
                InteractionManager.ReleaseFocus();
            }
        }
    }

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

        DeleteElements(layoutButtons);
        DeleteElements(layoutSelectables);
        DeleteElements(layoutInputFields);
        DeleteElements(layoutToggles);
        DeleteElements(layoutScrollViews);
        DeleteElements(layoutDropdowns);
        DeleteElements(layoutCycleSelectors);
        DeleteElements(layoutLinkButtons);
        DeleteElements(layoutStatusBadges);
        DeleteElements(layoutAlertBanners);
    }

    public void UpdateLayoutElements()
    {
        UpdateActiveElements(layoutButtons);
        UpdateActiveElements(layoutSelectables);
        UpdateActiveElements(layoutInputFields);
        UpdateActiveElements(layoutToggles);
        UpdateActiveElements(layoutScrollViews);
        UpdateActiveElements(layoutDropdowns);
        UpdateActiveElements(layoutCycleSelectors);
        UpdateActiveElements(layoutLinkButtons);
        UpdateActiveElements(layoutStatusBadges);
        UpdateActiveElements(layoutAlertBanners);
    }

    public void RemoveLayoutButton(int id) => DeleteElement(id, layoutButtons);
    public void RemoveLayoutSelectable(int id) => DeleteElement(id, layoutSelectables);
    public void RemoveLayoutInputField(int id) => DeleteElement(id, layoutInputFields);
    public void RemoveLayoutToggle(int id) => DeleteElement(id, layoutToggles);
    public void RemoveLayoutScrollView(int id) => DeleteElement(id, layoutScrollViews);
    public void RemoveLayoutDropdown(int id) => DeleteElement(id, layoutDropdowns);
    public void RemoveLayoutCycleSelector(int id) => DeleteElement(id, layoutCycleSelectors);
    public void RemoveLayoutLinkButton(int id) => DeleteElement(id, layoutLinkButtons);
    public void RemoveLayoutStatusBadge(int id) => DeleteElement(id, layoutStatusBadges);
    public void RemoveLayoutAlertBanner(int id) => DeleteElement(id, layoutAlertBanners);

    public Drawable? HitTestElements(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        Drawable? hit = HitTestActiveElements(layoutAlertBanners, mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        hit = HitTestActiveElements(layoutDropdowns, mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        hit = HitTestActiveElements(layoutCycleSelectors, mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        hit = HitTestActiveElements(layoutLinkButtons, mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        hit = HitTestActiveElements(layoutButtons, mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        hit = HitTestActiveElements(layoutSelectables, mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        hit = HitTestActiveElements(layoutInputFields, mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        hit = HitTestActiveElements(layoutToggles, mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

        hit = HitTestActiveElements(layoutScrollViews, mouseScreenPosition, mouseWorldPosition);
        if (hit != null) return hit;

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

    public static int MeasureTextH(string text, float fontSize)
    {
        return (int)MeasureTextEx(textFont, text, fontSize, 0.5f).Y;
    }

    public static Vector2 MeasureText(string text, float fontSize)
    {
        return MeasureTextEx(textFont, text, fontSize, 0.5f);
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

    private Button DrawButtonAbsolute(Button button, int posX, int posY)
    {
        int id = button.Id;
        bool found = layoutButtons.ContainsKey(id);

        if (!found)
        {
            ElementInfo<Button> elem = new(button, null);
            layoutButtons.Add(id, elem);
        }

        layoutButtons[id] = layoutButtons[id].Activate();

        layoutButtons[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutButtons[id].UIElement.Render();

        return layoutButtons[id].UIElement;
    }

    private Button DrawButtonAbsolute(int id, string buttonText, int posX, int posY, int buttonWidth, int buttonHeight, Action<Button> onButtonPressed, object payload, int fontSize, bool hasBorder, Color? fillColor = null, Color? borderColor = null, Color? textColor = null)
    {
        bool found = layoutButtons.ContainsKey(id);

        if (!found)
        {
            Button b = new(posX, posY, buttonWidth, buttonHeight, buttonText, onButtonPressed, payload, fontSize, hasBorder, fillColor, borderColor, textColor, defaultParent);
            ElementInfo<Button>  elem = new(b, null);
            layoutButtons.Add(id, elem);
        }
        else
        {
            layoutButtons[id].UIElement.ButtonText = buttonText;
            if (fillColor.HasValue) layoutButtons[id].UIElement.FillColor = fillColor;
            if (borderColor.HasValue) layoutButtons[id].UIElement.BorderColor = borderColor;
            if (textColor.HasValue) layoutButtons[id].UIElement.TextColor = textColor;
        }

        layoutButtons[id] = layoutButtons[id].Activate();

        layoutButtons[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutButtons[id].UIElement.Render();
        return layoutButtons[id].UIElement;
    }

    private Selectable DrawSelectableAbsolute(Selectable selectable, int posX, int posY)
    {
        int id = selectable.Id;
        bool found = layoutSelectables.ContainsKey(id);

        if (!found)
        {
            ElementInfo<Selectable> elem = new(selectable, null);
            layoutSelectables.Add(id, elem);
        }

        layoutSelectables[id] = layoutSelectables[id].Activate();

        layoutSelectables[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutSelectables[id].UIElement.Render();
        return layoutSelectables[id].UIElement;
    }

    private Selectable DrawSelectableAbsolute(int id, bool isSelected, string selectableText, int posX, int posY, int selectableWidth, int selectableHeight, int fontSize, Action<Selectable> onSelectableSelect, object? payload, Color bgColor, Color bgSelectionColor, Color textColor)
    {
        bool found = layoutSelectables.ContainsKey(id);

        if (!found)
        {
            Selectable sel = new(selectableText, isSelected, posX, posY, selectableWidth, selectableHeight, onSelectableSelect, payload, fontSize, bgColor, bgSelectionColor, textColor, defaultParent);
            ElementInfo<Selectable> elem = new(sel, null);
            layoutSelectables.Add(id, elem);
        }
        else
        {
            if (isSelected != layoutSelectables[id].UIElement.IsSelected)
            {
                if (isSelected) layoutSelectables[id].UIElement.Select(false);
                else layoutSelectables[id].UIElement.Deselect(false);
            }

            layoutSelectables[id].UIElement.SelectableText = selectableText;
        }

        layoutSelectables[id] = layoutSelectables[id].Activate();

        layoutSelectables[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutSelectables[id].UIElement.Render();

        return layoutSelectables[id].UIElement;
    }

    private InputField DrawInputFieldAbsolute(int id, string placeholderText, string fieldText, int posX, int posY, int inputFieldWidth, int inputFieldHeight, Action<InputField>? onTextEdited, Action<InputField>? onFocusEnd, int fontSize, bool isMasked = false)
    {
        bool found = layoutInputFields.ContainsKey(id);

        if (!found)
        {
            InputField cachedInputField = new InputField(placeholderText, fieldText, posX, posY, inputFieldWidth, inputFieldHeight, onTextEdited, onFocusEnd, fontSize, isMasked, defaultParent);
            ElementInfo<InputField> elem = new(cachedInputField, null);
            layoutInputFields.Add(id, elem);
        }
        else
        {
            if (!layoutInputFields[id].UIElement.IsFocused)
            {
                layoutInputFields[id].UIElement.InputFieldText = fieldText;
            }

            layoutInputFields[id].UIElement.IsMasked = isMasked;
            layoutInputFields[id].UIElement.OnTextChanged = onTextEdited;
            layoutInputFields[id].UIElement.OnFocusEnd = onFocusEnd;
        }

        layoutInputFields[id] = layoutInputFields[id].Activate();

        layoutInputFields[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutInputFields[id].UIElement.Render();
        return layoutInputFields[id].UIElement;
    }

    private Toggle DrawToggleAbsolute(int id, bool toggleValue, int posX, int posY, int toggleWidth, int toggleHeight, Action<Toggle>? onToggleChanged, object? payload)
    {
        bool found = layoutToggles.ContainsKey(id);
        if (!found)
        {
            Toggle cachedToggle = new(posX, posY, toggleValue, toggleWidth, toggleHeight, onToggleChanged, payload, 15, defaultParent);
            ElementInfo<Toggle> elem = new(cachedToggle, null);
            layoutToggles.Add(id, elem);
        }
        else
        {
            layoutToggles[id].UIElement.Value = toggleValue;
            layoutToggles[id].UIElement.SetOnToggleChanged(onToggleChanged);
        }

        layoutToggles[id] = layoutToggles[id].Activate();

        layoutToggles[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutToggles[id].UIElement.Render();
        return layoutToggles[id].UIElement;
    }

    private Dropdown DrawDropdownAbsolute(int id, string[] options, int selectedIndex, int posX, int posY, int width, int itemHeight, Action<Dropdown>? onSelectionChanged, object? payload, int fontSize)
    {
        bool found = layoutDropdowns.ContainsKey(id);
        if (!found)
        {
            Dropdown cachedDropdown = new Dropdown(options, selectedIndex, posX, posY, width, itemHeight, onSelectionChanged, payload, fontSize, defaultParent);
            ElementInfo<Dropdown> elem = new(cachedDropdown, null);
            layoutDropdowns.Add(id, elem);
        }
        else
        {
            if (layoutDropdowns[id].UIElement.Options.Length != options.Length)
                layoutDropdowns[id].UIElement.SetOptions(options, selectedIndex);
            else
            {
                layoutDropdowns[id].UIElement.SelectedIndex = selectedIndex;
                layoutDropdowns[id].UIElement.SetOnSelectionChanged(onSelectionChanged);
            }
        }

        layoutDropdowns[id] = layoutDropdowns[id].Activate();

        layoutDropdowns[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutDropdowns[id].UIElement.Render();
        return layoutDropdowns[id].UIElement;
    }

    private CycleSelector DrawCycleSelectorAbsolute(int id, string[] options, int selectedIndex, int posX, int posY, int width, int height, Action<CycleSelector>? onSelectionChanged, object? payload, int fontSize)
    {
        bool found = layoutCycleSelectors.ContainsKey(id);
        if (!found)
        {
            CycleSelector cachedCycle = new CycleSelector(options, selectedIndex, posX, posY, width, height, onSelectionChanged, payload, fontSize, defaultParent);
            ElementInfo<CycleSelector> elem = new(cachedCycle, null);
            layoutCycleSelectors.Add(id, elem);
        }
        else
        {
            layoutCycleSelectors[id].UIElement.Options = options;
            layoutCycleSelectors[id].UIElement.SetOnSelectionChanged(onSelectionChanged);
        }

        layoutCycleSelectors[id] = layoutCycleSelectors[id].Activate();

        layoutCycleSelectors[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutCycleSelectors[id].UIElement.Render();
        return layoutCycleSelectors[id].UIElement;
    }

    private LinkButton DrawLinkButtonAbsolute(int id, string text, string url, int posX, int posY, Action<LinkButton>? onClick, int fontSize)
    {
        bool found = layoutLinkButtons.ContainsKey(id);
        if (!found)
        {
            LinkButton cachedLink = new(posX, posY, text, url, onClick, fontSize, defaultParent);
            ElementInfo<LinkButton> elem = new(cachedLink, null);
            layoutLinkButtons.Add(id, elem);
        }
        else
        {
            layoutLinkButtons[id].UIElement.Text = text;
            layoutLinkButtons[id].UIElement.Url = url;
        }

        layoutLinkButtons[id] = layoutLinkButtons[id].Activate();

        layoutLinkButtons[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutLinkButtons[id].UIElement.Render();
        return layoutLinkButtons[id].UIElement;
    }

    private StatusBadge DrawStatusBadgeAbsolute(int id, string text, StatusType statusType, Color? customColor, int posX, int posY, int fontSize)
    {
        bool found = layoutStatusBadges.ContainsKey(id);
        if (!found)
        {
            StatusBadge cachedBadge = new(posX, posY, text, statusType, customColor, fontSize, defaultParent);
            ElementInfo<StatusBadge> elem = new(cachedBadge, null);
            layoutStatusBadges.Add(id, elem);
        }
        else
        {
            layoutStatusBadges[id].UIElement.Text = text;
            layoutStatusBadges[id].UIElement.Type = statusType;
            if (customColor.HasValue) layoutStatusBadges[id].UIElement.CustomColor = customColor.Value;
        }

        layoutStatusBadges[id] = layoutStatusBadges[id].Activate();

        layoutStatusBadges[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutStatusBadges[id].UIElement.Render();
        return layoutStatusBadges[id].UIElement;
    }

    private AlertBanner DrawAlertBannerAbsolute(int id, string message, AlertType alertType, int posX, int posY, int width, int height, bool isDismissible, int fontSize)
    {
        bool found = layoutAlertBanners.ContainsKey(id);
        if (!found)
        {
            AlertBanner cachedBanner = new(posX, posY, message, alertType, width, height, isDismissible, fontSize, defaultParent);
            ElementInfo<AlertBanner> elem = new(cachedBanner, null);
            layoutAlertBanners.Add(id, elem);
        }
        else
        {
            layoutAlertBanners[id].UIElement.Message = message;
            layoutAlertBanners[id].UIElement.Type = alertType;
        }

        layoutAlertBanners[id] = layoutAlertBanners[id].Activate();

        layoutAlertBanners[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutAlertBanners[id].UIElement.Render();
        return layoutAlertBanners[id].UIElement;
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

    public Button Button(Button button, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        Button b = DrawButtonAbsolute(button, (int)pos.X, (int)pos.Y);
        if (updateLayout) DrawAny((int)button.Width, (int)button.Height);
        return b;
    }

    public Button Button(int id, string buttonText, int buttonWidth, int buttonHeight, Action<Button> onButtonPressed, object payload, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        Button b = DrawButtonAbsolute(id, buttonText, (int)pos.X, (int)pos.Y, buttonWidth, buttonHeight, onButtonPressed, payload, 15, true);
        if (updateLayout) DrawAny(buttonWidth, buttonHeight);
        return b;
    }

    public Button Button(int id, string buttonText, int buttonWidth, int buttonHeight, Action<Button> onButtonPressed, object payload, Color? fillColor, Color? borderColor = null, Color? textColor = null, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        Button b = DrawButtonAbsolute(id, buttonText, (int)pos.X, (int)pos.Y, buttonWidth, buttonHeight, onButtonPressed, payload, 15, true, fillColor, borderColor, textColor);
        if (updateLayout) DrawAny(buttonWidth, buttonHeight);
        return b;
    }

    public Selectable Selectable(Selectable selectable, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        Selectable sel = DrawSelectableAbsolute(selectable, (int)pos.X, (int)pos.Y);
        if (updateLayout) DrawAny(selectable.Width, selectable.Height);
        return sel;
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

    public InputField InputField(int id, string placeholderText, string fieldText, int inputFieldWidth, int inputFieldHeight, Action<InputField>? onTextEdited = null, Action<InputField>? onFocusEnd = null, bool isMasked = false, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        InputField field = DrawInputFieldAbsolute(id, placeholderText, fieldText, (int)pos.X, (int)pos.Y, inputFieldWidth, inputFieldHeight, onTextEdited, onFocusEnd, 15, isMasked);
        if (updateLayout) DrawAny(inputFieldWidth, inputFieldHeight);
        return field;
    }

    public Toggle Toggle(int id, bool toggleValue, int toggleWidth, int toggleHeight, Action<Toggle>? onToggleChanged, object? payload, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        Toggle toggle = DrawToggleAbsolute(id, toggleValue, (int)pos.X, (int)pos.Y, toggleWidth, toggleHeight, onToggleChanged, payload);
        
        if (updateLayout) DrawAny(toggleWidth, toggleHeight);
        return toggle;
    }

    public Dropdown Dropdown(int id, string[] options, int selectedIndex, int width, int itemHeight, Action<Dropdown>? onSelectionChanged, object? payload, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null)
            pos -= defaultParent.Position;

        DrawDropdownAbsolute(id, options, selectedIndex, (int)pos.X, (int)pos.Y, width, itemHeight, onSelectionChanged, payload, 15);

        Dropdown cached = layoutDropdowns[id].UIElement;

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
                truncatedStr = string.Concat(text.AsSpan(0, len), ellipsis);
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
    public ScrollView BeginScrollView(int id, int viewWidth, int viewHeight, int startYOffset = 0, int spacing = 0)
    {
        bool found = layoutScrollViews.ContainsKey(id);
        if (!found)
        {
            ScrollView newSvc = new ScrollView(viewWidth, viewHeight, defaultParent);
            ElementInfo<ScrollView> elem = new(newSvc, null);
            layoutScrollViews.Add(id, elem);
        }

        layoutScrollViews[id] = layoutScrollViews[id].Activate();
        ScrollView svc = layoutScrollViews[id].UIElement;

        svc.Size = new Vector2(viewWidth, viewHeight);
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
            Rectangle defaultParentIntrRect = defaultParent.GetInteractableRect();
            defaultParentEndX = defaultParent.Position.X + defaultParentIntrRect.Width;
            defaultParentEndY = defaultParent.Position.Y + defaultParentIntrRect.Height;
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
        return svc;
    }

    public void EndScrollView()
    {
        if (activeScrollViews.Count == 0) return;

        int svId = activeScrollViews.Pop();
        ScrollView svc = layoutScrollViews[svId].UIElement;

        int contentWidth = PosX() - ((int)svc.Position.X + (int)svc.ScrollOffset.X);
        int contentHeight = PosY() - ((int)svc.Position.Y + (int)svc.ScrollOffset.Y);

        svc.SetContentSize(new Vector2(Math.Max(svc.Size.X, contentWidth), Math.Max(svc.Size.Y, contentHeight)));

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
        DrawAny((int)svc.Size.X, (int)svc.Size.Y);
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
        foreach (var kvp in layoutDropdowns)
        {
            if (layoutDropdowns[kvp.Key].IsActiveThisFrame)
                layoutDropdowns[kvp.Key].UIElement.DrawOverlay();
        }
    }

    // ==================== BINDABLE DRAWING METHODS ====================

    public Toggle DrawBindableToggleAbsolute(int id, BindableValueBase<bool> dataModel, int posX, int posY, int toggleWidth, int toggleHeight)
    {
        bool found = layoutToggles.ContainsKey(id);
        if (!found)
        {
            Toggle cachedToggle = new(posX, posY, dataModel.Get(), toggleWidth, toggleHeight, null, id, 15, defaultParent);
            RLToggleUI uiWrapper = new(cachedToggle);
            BoolToggleBinder binder = new();
            binder.Bind(dataModel, uiWrapper);

            ElementInfo<Toggle> elem = new(cachedToggle, binder);
            layoutToggles.Add(id, elem);
        }
        else
        {
            if (layoutToggles[id].DataBinder is BoolToggleBinder binder)
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();
                    if (binder.GetBoundUIObject() is RLToggleUI uiTarget)
                        binder.Bind(dataModel, uiTarget);
                }
            }
        }

        layoutToggles[id] = layoutToggles[id].Activate();

        layoutToggles[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutToggles[id].UIElement.Render();
        return layoutToggles[id].UIElement;
    }

    public Toggle BindableToggle(int id, BindableValueBase<bool> dataModel, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null)
            pos -= defaultParent.Position;

        Toggle toggle = DrawBindableToggleAbsolute(id, dataModel, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
        return toggle;
    }

    public InputField DrawBindableInputFieldStringAbsolute(int id, string placeholderText, RaylibNodeLibrary.DataBinding.BindableValueBase<string> dataModel, int posX, int posY, int width, int height)
    {
        bool found = layoutInputFields.ContainsKey(id);
        if (!found)
        {
            InputField cachedField = new InputField(placeholderText, dataModel.Get(), posX, posY, width, height, null, null, 15, false, defaultParent);
            RLInputFieldUI_String uiWrapper = new RLInputFieldUI_String(cachedField);
            StringInputBinder binder = new StringInputBinder();
            binder.Bind(dataModel, uiWrapper);

            ElementInfo<InputField> elem = new ElementInfo<InputField>(cachedField, binder);
            layoutInputFields.Add(id, elem);
        }
        else
        {
            if (layoutInputFields[id].DataBinder is StringInputBinder binder)
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();
                    if (binder.GetBoundUIObject() is RLInputFieldUI_String uiTarget)
                        binder.Bind(dataModel, uiTarget);
                }
            }
        }

        layoutInputFields[id] = layoutInputFields[id].Activate();

        layoutInputFields[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutInputFields[id].UIElement.Render();
        return layoutInputFields[id].UIElement;
    }

    public InputField BindableInputFieldString(int id, string placeholderText, RaylibNodeLibrary.DataBinding.BindableValueBase<string> dataModel, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;
        InputField field = DrawBindableInputFieldStringAbsolute(id, placeholderText, dataModel, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
        return field;
    }

    public InputField DrawBindableInputFieldIntAbsolute(int id, string placeholderText, RaylibNodeLibrary.DataBinding.BindableValueBase<int> dataModel, int posX, int posY, int width, int height)
    {
        bool found = layoutInputFields.ContainsKey(id);
        if (!found)
        {
            InputField cachedField = new InputField(placeholderText, dataModel.Get().ToString(), posX, posY, width, height, null, null, 15, false, defaultParent);
            RLInputFieldUI_Int uiWrapper = new RLInputFieldUI_Int(cachedField);
            IntInputBinder binder = new IntInputBinder();
            binder.Bind(dataModel, uiWrapper);

            ElementInfo<InputField> elem = new ElementInfo<InputField>(cachedField, binder);
            layoutInputFields.Add(id, elem);
        }
        else
        {
            if (layoutInputFields[id].DataBinder is IntInputBinder binder)
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();
                    if (binder.GetBoundUIObject() is RLInputFieldUI_Int uiTarget)
                        binder.Bind(dataModel, uiTarget);
                }
            }
        }

        layoutInputFields[id] = layoutInputFields[id].Activate();

        layoutInputFields[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutInputFields[id].UIElement.Render();
        return layoutInputFields[id].UIElement;
    }

    public InputField BindableInputFieldInt(int id, string placeholderText, RaylibNodeLibrary.DataBinding.BindableValueBase<int> dataModel, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;
        InputField field = DrawBindableInputFieldIntAbsolute(id, placeholderText, dataModel, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
        return field;
    }

    public InputField DrawBindableInputFieldFloatAbsolute(int id, string placeholderText, RaylibNodeLibrary.DataBinding.BindableValueBase<float> dataModel, int posX, int posY, int width, int height)
    {
        bool found = layoutInputFields.ContainsKey(id);
        if (!found)
        {
            InputField cachedField = new InputField(placeholderText, dataModel.Get().ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture), posX, posY, width, height, null, null, 15, false, defaultParent);
            RLInputFieldUI_Float uiWrapper = new RLInputFieldUI_Float(cachedField);
            FloatInputBinder binder = new FloatInputBinder();
            binder.Bind(dataModel, uiWrapper);

            ElementInfo<InputField> elem = new ElementInfo<InputField>(cachedField, binder);
            layoutInputFields.Add(id, elem);
        }
        else
        {
            if (layoutInputFields[id].DataBinder is FloatInputBinder binder)
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();
                    if (binder.GetBoundUIObject() is RLInputFieldUI_Float uiTarget)
                        binder.Bind(dataModel, uiTarget);
                }
            }
        }

        layoutInputFields[id] = layoutInputFields[id].Activate();

        layoutInputFields[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutInputFields[id].UIElement.Render();
        return layoutInputFields[id].UIElement;
    }

    public InputField BindableInputFieldFloat(int id, string placeholderText, BindableValueBase<float> dataModel, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;
        InputField field = DrawBindableInputFieldFloatAbsolute(id, placeholderText, dataModel, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
        return field;
    }

    public Selectable DrawBindableSelectableAbsolute(int id, BindableValueBase<bool> valueTarget, string selectableText, int posX, int posY, int width, int height)
    {
        bool found = layoutSelectables.ContainsKey(id);

        if (!found)
        {
            Selectable sel = new(selectableText, valueTarget.Get(), posX, posY, width, height, (sel) => { }, id, 15, Color.Gray, Color.Blue, Color.White, defaultParent);

            RLSelectableUI uiTarget = new(sel);
            BoolSelectableBinder binder = new();
            binder.Bind(valueTarget, uiTarget);

            ElementInfo<Selectable> elem = new(sel, binder);

            layoutSelectables.Add(id, elem);
        }
        else
        {
            layoutSelectables[id].UIElement.SelectableText = selectableText;

            if (layoutSelectables[id].DataBinder is BoolSelectableBinder selBinder)
            {
                if (selBinder.GetBoundValObject() != valueTarget)
                {
                    selBinder.Unbind();

                    if (selBinder.GetBoundUIObject() is RLSelectableUI uiTarget)
                        selBinder.Bind(valueTarget, uiTarget);
                }
            }
        }

        layoutSelectables[id] = layoutSelectables[id].Activate();

        layoutSelectables[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutSelectables[id].UIElement.Render();
        return layoutSelectables[id].UIElement;
    }

    public Selectable BindableSelectable(int id, BindableValueBase<bool> dataModel, string selectableText, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        
        if (defaultParent != null)
            pos -= defaultParent.Position;

        Selectable sel = DrawBindableSelectableAbsolute(id, dataModel, selectableText, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
        return sel;
    }

    public Dropdown DrawBindableDropdownAbsolute(int id, string[] options, BindableValueBase<int> dataModel, int posX, int posY, int width, int height)
    {
        bool found = layoutDropdowns.ContainsKey(id);
        if (!found)
        {
            Dropdown cachedDropdown = new Dropdown(options, dataModel.Get(), posX, posY, width, height, null, id, 15, defaultParent);
            RLDropdownUI uiWrapper = new RLDropdownUI(cachedDropdown);
            IntDropdownBinder binder = new IntDropdownBinder();
            binder.Bind(dataModel, uiWrapper);

            ElementInfo<Dropdown> elem = new ElementInfo<Dropdown>(cachedDropdown, binder);
            layoutDropdowns.Add(id, elem);
        }
        else
        {
            if (layoutDropdowns[id].DataBinder is IntDropdownBinder binder)
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();
                    if (binder.GetBoundUIObject() is RLDropdownUI uiTarget)
                        binder.Bind(dataModel, uiTarget);
                }
            }
        }

        layoutDropdowns[id] = layoutDropdowns[id].Activate();

        layoutDropdowns[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutDropdowns[id].UIElement.Render();
        return layoutDropdowns[id].UIElement;
    }

    public Dropdown BindableDropdown(int id, string[] options, BindableValueBase<int> dataModel, int width, int height, bool updateLayout = true)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());
        if (defaultParent != null) pos -= defaultParent.Position;
        Dropdown dropdown = DrawBindableDropdownAbsolute(id, options, dataModel, (int)pos.X, (int)pos.Y, width, height);
        if (updateLayout) DrawAny(width, height);
        return dropdown;
    }
}