using Microsoft.VisualBasic;
using Pratyaksh.Core;
using Pratyaksh.Core.DataBinding;
using Pratyaksh.UI.DataBinding;
using Pratyaksh.UI.UIElements;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Pratyaksh.UI;

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

public struct ElementInfo(UIBase uiElement, BinderBase? dataBinder)
{
    public UIBase UIElement { get; set; } = uiElement;
    public BinderBase? DataBinder { get; set; } = dataBinder;
    public bool IsActiveThisFrame { get; set; }

    public readonly bool Is<T>() where T : UIBase
    {
        return UIElement is T;
    }

    public readonly T Get<T>() where T : UIBase
    {
        return (T)UIElement;
    }

    public ElementInfo Activate()
    {
        IsActiveThisFrame = true;
        return this;
    }

    public ElementInfo Deactivate()
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

    private static Raylib_cs.Font textFont;

    private Dictionary<int, ElementInfo> layoutElements = []; //NOTE: Layering support is not there after refactor. Previously was broken (probably). This is intended. Will be re-added later.

    private Stack<EditorObject?> parentStack = new();
    private Stack<int> activeScrollViews = new();

    private EditorObject? defaultParent = null;

    private static void ActivateElements(Dictionary<int, ElementInfo> targetDict)
    {
        foreach (var kvp in targetDict)
            targetDict[kvp.Key] = targetDict[kvp.Key].Activate();
    }

    private static void DeactivateElements(Dictionary<int, ElementInfo> targetDict)
    {
        foreach (var kvp in targetDict)
            targetDict[kvp.Key] = targetDict[kvp.Key].Deactivate();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DeleteElement(int id, Dictionary<int, ElementInfo> targetDict)
    {
        if (targetDict.TryGetValue(id, out var info))
        {
            info.DataBinder?.Unbind();
            info.UIElement.Delete();
            targetDict.Remove(id);
        }
    }

    private static void DeleteElements(Dictionary<int, ElementInfo> targetDict)
    {
        foreach (var kvp in targetDict)
        {
            targetDict[kvp.Key].DataBinder?.Unbind();
            targetDict[kvp.Key].UIElement.Delete();
        }

        targetDict.Clear();
    }

    private static void UpdateActiveElements(Dictionary<int, ElementInfo> targetDict)
    {
        foreach (var kvp in targetDict)
        {
            if (targetDict[kvp.Key].IsActiveThisFrame)
                targetDict[kvp.Key].UIElement.Update();
        }
    }

    private static Drawable? HitTestActiveElements(Dictionary<int, ElementInfo> targetDict, IWorldToScreenTransformer transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        var keys = targetDict.Keys.ToArray();
        for (int i = keys.Length - 1; i >= 0; i--)
        {
            int key = keys[i];
            if (targetDict[key].IsActiveThisFrame)
            {
                var hit = targetDict[key].UIElement.HitTest(transformer, mouseScreenPosition, mouseWorldPosition);
                if (hit != null) return hit;
            }
        }
        return null;
    }

    public void BeginFrame()
    {
        DeactivateElements(layoutElements);
    }

    public void EndFrame()
    {
        foreach (KeyValuePair<int, ElementInfo> kvp in layoutElements)
        {
            if (!kvp.Value.IsActiveThisFrame && kvp.Value.Is<InputField>() && kvp.Value.Get<InputField>().IsFocused)
                Engine.Instance.InteractionManager.ReleaseFocus();
        }
    }

    public static void InitSLEDefaultFont(Raylib_cs.Font font)
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

        DeleteElements(layoutElements);
    }

    public void UpdateLayoutElements()
    {
        UpdateActiveElements(layoutElements);
    }

    public void RemoveLayoutElement(int id) => DeleteElement(id, layoutElements);

    public Drawable? HitTestElements(IWorldToScreenTransformer transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        Drawable? hit = HitTestActiveElements(layoutElements, transformer, mouseScreenPosition, mouseWorldPosition);
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
        return (int)Raylib_cs.Raylib.MeasureTextEx(textFont, text, fontSize, 0.5f).X;
    }

    public static int MeasureTextH(string text, float fontSize)
    {
        return (int)Raylib_cs.Raylib.MeasureTextEx(textFont, text, fontSize, 0.5f).Y;
    }

    public static Vector2 MeasureText(string text, float fontSize)
    {
        return Raylib_cs.Raylib.MeasureTextEx(textFont, text, fontSize, 0.5f);
    }

    public static void DrawTextAbsolute(string text, int posX, int posY, Raylib_cs.Color fontColor, float fontSize, Vector2 offset)
    {
        Raylib_cs.Raylib.DrawTextPro(textFont, text, new Vector2(posX + (int)offset.X, posY + (int)offset.Y), Vector2.Zero, 0, fontSize, 0.5f, fontColor);
    }

    public static void DrawTextAbsolute(string text, int posX, int posY, Raylib_cs.Color fontColor, Vector2 offset)
    {
        Raylib_cs.Raylib.DrawTextEx(textFont, text, new Vector2(posX + (int)offset.X, posY + (int)offset.Y), 15, 0.5f, fontColor);
    }

    public static void DrawPanelAbsolute(int posX, int posY, int width, int height, Raylib_cs.Color panelColor)
    {
        Raylib_cs.Raylib.DrawRectangle(posX, posY, width, height, panelColor);
    }

    public static void DrawTextPanelAbsolute(string text, int posX, int posY, int panelWidth, int panelHeight, int textSize, Raylib_cs.Color panelColor, Raylib_cs.Color textColor, Vector2 textOffset)
    {
        DrawPanelAbsolute(posX, posY, panelWidth, panelHeight, panelColor);
        DrawTextAbsolute(text, posX, posY, textColor, textSize, textOffset);
    }

    public static void DrawSectionAbsolute(string heading, int posX, int posY, int width, int heigth, float headingPercent, int headingSize, Raylib_cs.Color headingBgColor, Raylib_cs.Color bodyBgColor, Raylib_cs.Color fontColor)
    {
        int headingHeight = (int)(heigth * headingPercent);

        DrawPanelAbsolute(posX, posY + headingHeight, width, heigth - headingHeight, bodyBgColor);

        if (headingHeight != 0)
            DrawTextPanelAbsolute(heading, posX, posY, width, headingHeight, headingSize, headingBgColor, fontColor, new Vector2(5, 0));
        else
            Raylib_cs.Raylib.DrawTextEx(textFont, heading, new Vector2(posX + 5, posY), headingSize, 0.5f, fontColor);
    }

    public void Text(string text, Raylib_cs.Color fontColor, bool updateLayout = true)
    {
        DrawTextAbsolute(text, PosX_Dynamic(), PosY_Dynamic(), fontColor, new Vector2(0, 0));
        if (updateLayout) DrawAny(text.Length * 2, 20);
    }

    public void TextTruncated(string text, int maxWidth, Raylib_cs.Color fontColor, int fontSize = 15, bool updateLayout = true)
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

    public void Panel(int width, int height, Raylib_cs.Color panelColor, bool updateLayout = true)
    {
        DrawPanelAbsolute(PosX_Dynamic(), PosY_Dynamic(), width, height, panelColor);
        if (updateLayout) DrawAny(width, height);
    }

    public void TextPanelFixed(string text, int x, int y, int panelWidth, int panelHeight, Raylib_cs.Color panelColor, Raylib_cs.Color textColor, bool updateLayout = true)
    {
        PosX_Dynamic();
        PosY_Dynamic();

        DrawTextPanelAbsolute(text, x, y, panelWidth, panelHeight, 15, panelColor, textColor, new Vector2(5, 0));
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void TextPanelPro(string text, int panelWidth, int panelHeight, Raylib_cs.Color panelColor, Raylib_cs.Color textColor, bool updateLayout = true)
    {
        DrawTextPanelAbsolute(text, PosX_Dynamic(), PosY_Dynamic(), panelWidth, panelHeight, 15, panelColor, textColor, new Vector2(5, 0));
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void TextPanelEx(string text, int panelWidth, int panelHeight, Vector2 panelOffset, bool updateLayout = true)
    {
        DrawTextPanelAbsolute(text, PosX_Dynamic(), PosY_Dynamic(), panelWidth, panelHeight, 15, Raylib_cs.Color.Gray, Raylib_cs.Color.LightGray, panelOffset);
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void TextPanel(string text, int panelWidth, int panelHeight, bool updateLayout = true)
    {
        DrawTextPanelAbsolute(text, PosX_Dynamic(), PosY_Dynamic(), panelWidth, panelHeight, 15, Raylib_cs.Color.LightGray, Raylib_cs.Color.DarkGray, new Vector2(5, 0));
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void SectionEx(string heading, int width, int height, Raylib_cs.Color headingBgColor, Raylib_cs.Color bodyBgColor, Raylib_cs.Color fontColor, float headerPerc, bool updateLayout = true)
    {
        DrawSectionAbsolute(heading, PosX_Dynamic(), PosY_Dynamic(), width, height, headerPerc, 20, headingBgColor, bodyBgColor, fontColor);
        if (updateLayout) DrawAny(width, height);
    }

    public void Section(string heading, int width, int heigth, float headerPerc, bool updateLayout = true)
    {
        DrawSectionAbsolute(heading, PosX_Dynamic(), PosY_Dynamic(), width, heigth, headerPerc, 20, Raylib_cs.Color.DarkGray, Raylib_cs.Color.Gray, Raylib_cs.Color.LightGray);
        if (updateLayout) DrawAny(width, heigth);
    }

    public ElementInfo GetOrAdd(int id, UIBase element, BinderBase? binder)
    {
        bool found = layoutElements.ContainsKey(id);

        if (!found)
        {
            ElementInfo elem = new(element, binder);
            layoutElements.Add(id, elem);
        }

        return layoutElements[id];
    }

    public ElementInfo GetOrAdd(int id, UIElementDescription uiDesc, BinderBase? binder, EditorObject? defaultParent, ParentBasis parentBasis)
    {
        bool found = layoutElements.ContainsKey(id);

        if (!found)
        {
            UIBase element = uiDesc.Construct(defaultParent, parentBasis);
            ElementInfo elem = new(element, binder);
            layoutElements.Add(id, elem);
        }

        return layoutElements[id];
    }

    private void DrawElementAbsolute(ElementInfo elementInfo, bool updateLayout)
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        elementInfo = elementInfo.Activate();

        elementInfo.UIElement.RelativePosition = pos;
        elementInfo.UIElement.Render();
        
        if (updateLayout) DrawAny(elementInfo.UIElement.Width, elementInfo.UIElement.Height);
    }

    public ElementInfo ElementDirect(int id, UIBase element, BinderBase? binder, bool updateLayout)
    {
        ElementInfo targetEI = GetOrAdd(id, element, binder);
        DrawElementAbsolute(targetEI, updateLayout);

        return targetEI;
    }

    public ElementInfo ElementFromDesc(int id, UIElementDescription uiDesc, BinderBase? binder, bool updateLayout)
    {
        ElementInfo targetEI = GetOrAdd(id, uiDesc, binder, defaultParent, ParentBasis.TopLeft);
        DrawElementAbsolute(targetEI, updateLayout);

        return targetEI;
    }

    private ElementInfo HandleBinder<T, K>(int id, BindableValueBase<T> dataModel, ElementInfo targetEI, Func<K> factory) where K : BindableUIBase<T>
    {
        if (targetEI.DataBinder == null)
        {
            Binder<BindableValueBase<T>, K, T> binder = new();
            binder.Bind(dataModel, factory.Invoke());
            targetEI.DataBinder = binder;

            layoutElements[id] = targetEI;
        }

        if (targetEI.DataBinder is Binder<BindableValueBase<T>, K, T> potBinder)
        {
            if (potBinder.GetBoundValObject() != dataModel)
            {
                potBinder.Unbind();

                if (potBinder.GetBoundUIObject() is K uiTarget)
                    potBinder.Bind(dataModel, uiTarget);
            }
        }

        return targetEI;
    }

    public Button Button(Button button, bool updateLayout = true) => ElementDirect(button.Id, button, null, updateLayout).Get<Button>();

    public Button Button(int id, string buttonText, int buttonWidth, int buttonHeight, Action<Button>? onButtonPressed = null, object? payload = null, int fontSize = 15, bool hasBorder = true, Raylib_cs.Color? fillColor = null, Raylib_cs.Color? borderColor = null, Raylib_cs.Color? textColor = null, bool updateLayout = true)
    {
        ButtonDesc buttonDesc = new(buttonText, buttonWidth, buttonHeight, onButtonPressed, fontSize, hasBorder, fillColor, borderColor, textColor);
        return Button(id, buttonDesc, updateLayout);
    }

    public Button Button(int id, ButtonDesc buttonDesc, bool updateLayout = true)
    {
        Button stored = ElementFromDesc(id, buttonDesc, null, updateLayout).Get<Button>();
        stored.ButtonText = buttonDesc.Text;

        stored.FillColor = buttonDesc.FillColor;
        stored.BorderColor = buttonDesc.BorderColor;
        stored.TextColor = buttonDesc.TextColor;

        return stored;
    }

    public Selectable Selectable(Selectable selectable, bool updateLayout = true) => ElementDirect(selectable.Id, selectable, null, updateLayout).Get<Selectable>();

    public Selectable Selectable(int id, bool isSelected, string selectableText, int selectableWidth, int selectableHeight, Action<Selectable>? onSelectableSelect = null, object? payload = null, int fontSize = 15, Raylib_cs.Color? bgColor = null, Raylib_cs.Color? bgSelectionColor = null, Raylib_cs.Color? textColor = null, bool updateLayout = true)
    {
        bgColor ??= new Raylib_cs.Color((byte)38, (byte)38, (byte)38, (byte)255);
        bgSelectionColor ??= new Raylib_cs.Color((byte)28, (byte)50, (byte)88, (byte)255);
        textColor ??= new Raylib_cs.Color((byte)200, (byte)200, (byte)200, (byte)255);

        SelectableDesc selDesc = new(selectableText, isSelected, selectableWidth, selectableHeight, onSelectableSelect);
        return Selectable(id, selDesc, updateLayout);
    }

    public Selectable Selectable(int id, SelectableDesc selDesc, bool updateLayout = true)
    {
        ElementInfo targetEI = ElementFromDesc(id, selDesc, null, updateLayout);
        if (selDesc.IsBindable && selDesc.DataModel != null)
        {
            targetEI = HandleBinder(id, selDesc.DataModel, targetEI, () => new RLSelectableUI(targetEI.Get<Selectable>()));
        }

        Selectable stored = targetEI.Get<Selectable>();

        if (!selDesc.IsBindable)
        {
            if (selDesc.IsSelected != stored.IsSelected)
            {
                if (selDesc.IsSelected) stored.Select(false);
                else stored.Deselect(false);
            }
        }

        stored.SelectableText = selDesc.Text;

        return stored;
    }

    public Selectable BindableSelectable(int id, BindableValueBase<bool> dataModel, string selectableText, int width, int height, bool updateLayout = true)
    {
        SelectableDesc selDesc = new(selectableText, dataModel, width, height);
        return Selectable(id, selDesc, updateLayout);
    }

    public InputField InputField(InputField inputField, bool updateLayout = true) => ElementDirect(inputField.Id, inputField, null, updateLayout).Get<InputField>();

    public InputField InputField(int id, string placeholderText, string fieldText, int inputFieldWidth, int inputFieldHeight, Action<InputField>? onTextEdited = null, Action<InputField>? onFocusEnd = null, int fontSize = 15, bool isMasked = false, bool updateLayout = true)
    {
        InputFieldDesc desc = new(placeholderText, fieldText, inputFieldWidth, inputFieldHeight, isMasked, onTextEdited, onFocusEnd);
        return InputField(id, desc, updateLayout);
    }

    public InputField InputField(int id, InputFieldDesc inputFieldDesc, bool updateLayout = true)
    {
        ElementInfo targetEI = ElementFromDesc(id, inputFieldDesc, null, updateLayout);
        if (inputFieldDesc.StringDataModel != null)
        {
            targetEI = HandleBinder(id, inputFieldDesc.StringDataModel, targetEI, () => new RLInputFieldUI_String(targetEI.Get<InputField>()));
        }
        else if (inputFieldDesc.IntDataModel != null)
        {
            targetEI = HandleBinder(id, inputFieldDesc.IntDataModel, targetEI, () => new RLInputFieldUI_Int(targetEI.Get<InputField>()));
        }
        else if (inputFieldDesc.FloatDataModel != null)
        {
            targetEI = HandleBinder(id, inputFieldDesc.FloatDataModel, targetEI, () => new RLInputFieldUI_Float(targetEI.Get<InputField>()));
        }

        InputField stored = targetEI.Get<InputField>();
        if (!inputFieldDesc.IsBindable)
        {
            if (!stored.IsFocused)
            {
                stored.InputFieldText = inputFieldDesc.Text;
            }
        }

        stored.IsMasked = inputFieldDesc.IsMasked;
        stored.OnTextChanged = inputFieldDesc.OnTextChanged;
        stored.OnFocusEnd = inputFieldDesc.OnFocusEnd;

        return stored;
    }

    public InputField BindableInputFieldString(int id, string placeholderText, BindableValueBase<string> dataModel, int width, int height, bool updateLayout = true)
    {
        InputFieldDesc desc = new(placeholderText, dataModel, width, height);
        return InputField(id, desc, updateLayout);
    }

    public InputField BindableInputFieldInt(int id, string placeholderText, BindableValueBase<int> dataModel, int width, int height, bool updateLayout = true)
    {
        InputFieldDesc desc = new(placeholderText, dataModel, width, height);
        return InputField(id, desc, updateLayout);
    }

    public InputField BindableInputFieldFloat(int id, string placeholderText, BindableValueBase<float> dataModel, int width, int height, bool updateLayout = true)
    {
        InputFieldDesc desc = new(placeholderText, dataModel, width, height);
        return InputField(id, desc, updateLayout);
    }

    public Toggle Toggle(Toggle toggle, bool updateLayout = true) => ElementDirect(toggle.Id, toggle, null, updateLayout).Get<Toggle>();

    public Toggle Toggle(int id, bool toggleValue, int toggleWidth, int toggleHeight, Action<Toggle>? onToggleChanged = null, object? payload = null, bool updateLayout = true)
    {
        ToggleDesc desc = new("", toggleValue, toggleWidth, toggleHeight, onToggleChanged);
        return Toggle(id, desc, updateLayout);
    }

    public Toggle Toggle(int id, ToggleDesc toggleDesc, bool updateLayout = true)
    {
        ElementInfo targetEI = ElementFromDesc(id, toggleDesc, null, updateLayout);
        if (toggleDesc.IsBindable && toggleDesc.DataModel != null)
        {
            targetEI = HandleBinder(id, toggleDesc.DataModel, targetEI, () => new RLToggleUI(targetEI.Get<Toggle>()));
        }

        Toggle stored = targetEI.Get<Toggle>();
        if (!toggleDesc.IsBindable)
        {
            stored.Value = toggleDesc.Value;
        }
        stored.SetOnToggleChanged(toggleDesc.OnToggle);

        return stored;
    }

    public Toggle BindableToggle(int id, BindableValueBase<bool> dataModel, int width, int height, bool updateLayout = true)
    {
        ToggleDesc desc = new("", dataModel, width, height);
        return Toggle(id, desc, updateLayout);
    }

    public Dropdown Dropdown(Dropdown dropdown, bool updateLayout = true) => ElementDirect(dropdown.Id, dropdown, null, updateLayout).Get<Dropdown>();

    public Dropdown Dropdown(int id, string[] options, int selectedIndex, int width, int itemHeight, Action<Dropdown>? onSelectionChanged = null, object? payload = null, int fontSize = 15, bool updateLayout = true)
    {
        DropdownDesc desc = new(options, selectedIndex, width, itemHeight, onSelectionChanged);
        return Dropdown(id, desc, updateLayout);
    }

    public Dropdown Dropdown(int id, DropdownDesc dropdownDesc, bool updateLayout = true)
    {
        ElementInfo targetEI = ElementFromDesc(id, dropdownDesc, null, updateLayout);
        if (dropdownDesc.IsBindable && dropdownDesc.DataModel != null)
        {
            targetEI = HandleBinder(id, dropdownDesc.DataModel, targetEI, () => new RLDropdownUI(targetEI.Get<Dropdown>()));
        }

        Dropdown stored = targetEI.Get<Dropdown>();
        if (stored.Options.Length != dropdownDesc.Options.Length)
            stored.SetOptions(dropdownDesc.Options, dropdownDesc.SelectedIndex);
        else
        {
            if (!dropdownDesc.IsBindable)
            {
                stored.SelectedIndex = dropdownDesc.SelectedIndex;
            }
            stored.SetOnSelectionChanged(dropdownDesc.OnSelectionChanged);
        }

        return stored;
    }

    public Dropdown BindableDropdown(int id, string[] options, BindableValueBase<int> dataModel, int width, int height, bool updateLayout = true)
    {
        DropdownDesc desc = new(options, dataModel, width, height);
        return Dropdown(id, desc, updateLayout);
    }

    public CycleSelector CycleSelector(CycleSelector cycleSelector, bool updateLayout = true) => ElementDirect(cycleSelector.Id, cycleSelector, null, updateLayout).Get<CycleSelector>();

    public CycleSelector CycleSelector(int id, string[] options, int selectedIndex, int width, int height, Action<CycleSelector>? onSelectionChanged = null, object? payload = null, int fontSize = 15, bool updateLayout = true)
    {
        CycleSelectorDesc desc = new(options, selectedIndex, width, height, onSelectionChanged);
        return CycleSelector(id, desc, updateLayout);
    }

    public CycleSelector CycleSelector(int id, CycleSelectorDesc cycleSelectorDesc, bool updateLayout = true)
    {
        CycleSelector stored = ElementFromDesc(id, cycleSelectorDesc, null, updateLayout).Get<CycleSelector>();
        stored.SelectedIndex = cycleSelectorDesc.SelectedIndex;
        stored.Options = cycleSelectorDesc.Options;
        stored.SetOnSelectionChanged(cycleSelectorDesc.OnSelectionChanged);

        return stored;
    }

    public LinkButton LinkButton(LinkButton linkButton, bool updateLayout = true) => ElementDirect(linkButton.Id, linkButton, null, updateLayout).Get<LinkButton>();

    public LinkButton LinkButton(int id, string text, string url, Action<LinkButton>? onClick = null, int fontSize = 14, bool updateLayout = true)
    {
        LinkButtonDesc desc = new(text, url, onClick);
        return LinkButton(id, desc, updateLayout);
    }

    public LinkButton LinkButton(int id, LinkButtonDesc linkButtonDesc, bool updateLayout = true)
    {
        LinkButton stored = ElementFromDesc(id, linkButtonDesc, null, updateLayout).Get<LinkButton>();
        stored.Text = linkButtonDesc.Text;
        stored.Url = linkButtonDesc.Url;

        return stored;
    }

    public StatusBadge StatusBadge(StatusBadge statusBadge, bool updateLayout = true) => ElementDirect(statusBadge.Id, statusBadge, null, updateLayout).Get<StatusBadge>();

    public StatusBadge StatusBadge(int id, string text, StatusType statusType = StatusType.Idle, Raylib_cs.Color? customColor = null, int fontSize = 13, bool updateLayout = true)
    {
        StatusBadgeDesc desc = new(text, statusType, customColor);
        return StatusBadge(id, desc, updateLayout);
    }

    public StatusBadge StatusBadge(int id, StatusBadgeDesc statusBadgeDesc, bool updateLayout = true)
    {
        StatusBadge stored = ElementFromDesc(id, statusBadgeDesc, null, updateLayout).Get<StatusBadge>();
        stored.Text = statusBadgeDesc.Text;
        stored.Type = statusBadgeDesc.StatusType;

        if (statusBadgeDesc.CustomColor.HasValue) stored.CustomColor = statusBadgeDesc.CustomColor.Value;

        return stored;
    }

    public AlertBanner AlertBanner(AlertBanner alertBanner, bool updateLayout = true) => ElementDirect(alertBanner.Id, alertBanner, null, updateLayout).Get<AlertBanner>();

    public AlertBanner AlertBanner(int id, string message, AlertType alertType = AlertType.Error, int width = 360, int height = 32, int fontSize = 13, bool isDismissible = true, bool updateLayout = true)
    {
        AlertBannerDesc desc = new(message, alertType, isDismissible, width, height);
        return AlertBanner(id, desc, updateLayout);
    }

    public AlertBanner AlertBanner(int id, AlertBannerDesc alertBannerDesc, bool updateLayout = true)
    {
        AlertBanner stored = ElementFromDesc(id, alertBannerDesc, null, updateLayout).Get<AlertBanner>();
        stored.Message = alertBannerDesc.Text;
        stored.Type = alertBannerDesc.AlertType;

        return stored;
    }

    public Slider Slider(Slider slider, bool updateLayout = true) => ElementDirect(slider.Id, slider, null, updateLayout).Get<Slider>();

    public Slider Slider(int id, float value, float minValue, float maxValue, int width, int height, Action<Slider>? onValueChanged = null, object? payload = null, bool showValue = true, string? format = null, float? step = null, int fontSize = 13, bool updateLayout = true)
    {
        SliderDesc desc = new("", value, minValue, maxValue, width, height, onValueChanged, showValue, format, step);
        return Slider(id, desc, updateLayout);
    }

    public Slider Slider(int id, SliderDesc sliderDesc, bool updateLayout = true)
    {
        ElementInfo targetEI = ElementFromDesc(id, sliderDesc, null, updateLayout);
        if (sliderDesc.IsBindable && sliderDesc.DataModel != null)
        {
            targetEI = HandleBinder(id, sliderDesc.DataModel, targetEI, () => new RLSliderUI(targetEI.Get<Slider>()));
        }

        Slider stored = targetEI.Get<Slider>();
        stored.MinValue = sliderDesc.MinValue;
        stored.MaxValue = sliderDesc.MaxValue;
        stored.Step = sliderDesc.Step;
        stored.ShowValue = sliderDesc.ShowValue;
        stored.Format = sliderDesc.Format;

        if (!sliderDesc.IsBindable)
        {
            if (!stored.IsDragging) stored.SetValueWithoutNotify(sliderDesc.Value);
        }

        stored.SetOnValueChanged(sliderDesc.OnValueChanged);

        return stored;
    }

    public Slider BindableSlider(int id, BindableValueBase<float> dataModel, float minValue, float maxValue, int width, int height, bool showValue = true, string? format = null, float? step = null, int fontSize = 13, bool updateLayout = true)
    {
        SliderDesc desc = new("", dataModel, minValue, maxValue, width, height, showValue, format, step);
        return Slider(id, desc, updateLayout);
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
        bool found = layoutElements.ContainsKey(id);
        if (!found)
        {
            ScrollView newSvc = new(viewWidth, viewHeight, defaultParent);
            ElementInfo elem = new(newSvc, null);
            layoutElements.Add(id, elem);
        }

        layoutElements[id] = layoutElements[id].Activate();
        ScrollView svc = layoutElements[id].Get<ScrollView>();

        svc.Size = new Vector2(viewWidth, viewHeight);
        svc.RelativePosition = new Vector2(PosX_Dynamic(), PosY_Dynamic() + startYOffset);

        if (defaultParent != null)
            svc.RelativePosition -= defaultParent.Position;

        Rectangle scissorRect = svc.GetScissorRect(Engine.Instance.InteractionManager.WorldToScreenTransformer);

        float scissorEndX = scissorRect.X + scissorRect.Width;
        float scissorEndY = scissorRect.Y + scissorRect.Height;

        // Now we cut the scroll scissor according to current parent's scissor if current parent scissor is smaller than required scroll scissor XD
        float defaultParentEndX;
        float defaultParentEndY;

        if (defaultParent != null)
        {
            // TODO: Using interactable rect's width/height instead of visual's! For now it works.
            Rectangle defaultParentIntrRect = defaultParent.GetInteractableRect(Engine.Instance.InteractionManager.WorldToScreenTransformer);
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

        Raylib_cs.Raylib.BeginScissorMode((int)scissorRect.X, (int)scissorRect.Y, scissorWidth, scissorHeight);

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
        if (activeScrollViews.Count == 0)
            return;

        int svId = activeScrollViews.Pop();
        ScrollView svc = layoutElements[svId].Get<ScrollView>();

        int contentWidth = PosX() - ((int)svc.Position.X + (int)svc.ScrollOffset.X);
        int contentHeight = PosY() - ((int)svc.Position.Y + (int)svc.ScrollOffset.Y);

        svc.SetContentSize(new Vector2(Math.Max(svc.Size.X, contentWidth), Math.Max(svc.Size.Y, contentHeight)));

        EndVertical(contentWidth);
        EndHorizontal(contentHeight);

        Raylib_cs.Raylib.EndScissorMode();

        defaultParent = parentStack.Pop();

        // Re-apply the parent's scissor rect to prevent leaking out of bounds
        if (defaultParent != null)
        {
            Rectangle pRect = defaultParent.GetInteractableRect(Engine.Instance.InteractionManager.WorldToScreenTransformer);
            Raylib_cs.Raylib.BeginScissorMode((int)pRect.X, (int)pRect.Y, (int)pRect.Width, (int)pRect.Height);
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
        foreach (KeyValuePair<int, ElementInfo> kvp in layoutElements)
        {
            if (kvp.Value.UIElement is IOverlayable overlay && kvp.Value.IsActiveThisFrame)
                overlay.DrawOverlay();
        }
    }
}