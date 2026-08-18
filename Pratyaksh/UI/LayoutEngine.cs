using System.Numerics;
using System.Runtime.CompilerServices;
using Pratyaksh.Core;
using Pratyaksh.Core.DataBinding;
using Pratyaksh.UI.DataBinding;
using Pratyaksh.UI.UIElements;

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

    private static void ActivateElements<T>(Dictionary<int, ElementInfo> targetDict) where T : UIBase
    {
        foreach (var kvp in targetDict)
            targetDict[kvp.Key] = targetDict[kvp.Key].Activate();
    }

    private static void DeactivateElements<T>(Dictionary<int, ElementInfo> targetDict) where T : UIBase
    {
        foreach (var kvp in targetDict)
            targetDict[kvp.Key] = targetDict[kvp.Key].Deactivate();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DeleteElement<T>(int id, Dictionary<int, ElementInfo> targetDict) where T : UIBase
    {
        if (targetDict.TryGetValue(id, out var info))
        {
            info.DataBinder?.Unbind();
            info.UIElement.Delete();
            targetDict.Remove(id);
        }
    }

    private static void DeleteElements<T>(Dictionary<int, ElementInfo> targetDict) where T : UIBase
    {
        foreach (var kvp in targetDict)
        {
            targetDict[kvp.Key].DataBinder?.Unbind();
            targetDict[kvp.Key].UIElement.Delete();
        }

        targetDict.Clear();
    }

    private static void UpdateActiveElements<T>(Dictionary<int, ElementInfo> targetDict) where T : UIBase
    {
        foreach (var kvp in targetDict)
        {
            if (targetDict[kvp.Key].IsActiveThisFrame)
                targetDict[kvp.Key].UIElement.Update();
        }
    }

    private static Drawable? HitTestActiveElements<T>(Dictionary<int, ElementInfo> targetDict, IWorldToScreenTransformer transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition) where T : UIBase
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
        DeactivateElements<UIBase>(layoutElements);
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

        DeleteElements<UIBase>(layoutElements);
    }

    public void UpdateLayoutElements()
    {
        UpdateActiveElements<UIBase>(layoutElements);
    }

    public void RemoveLayoutElement(int id) => DeleteElement<UIBase>(id, layoutElements);

    public Drawable? HitTestElements(IWorldToScreenTransformer transformer, Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        Drawable? hit = HitTestActiveElements<UIBase>(layoutElements, transformer, mouseScreenPosition, mouseWorldPosition);
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

    private T DrawElementAbsolute<T>(int id, Func<(T, BinderBase?)> factory, Action<(T, BinderBase?)> storedReflect, int posX, int posY) where T : UIBase
    {
        bool found = layoutElements.ContainsKey(id);

        if (!found)
        {
            (T newElem, BinderBase? binder) = factory.Invoke();
            ElementInfo elem = new(newElem, binder);
            layoutElements.Add(id, elem);
        }
        else storedReflect.Invoke((layoutElements[id].Get<T>(), layoutElements[id].DataBinder));

        layoutElements[id] = layoutElements[id].Activate();

        layoutElements[id].UIElement.RelativePosition = new Vector2(posX, posY);
        layoutElements[id].UIElement.Render();

        return layoutElements[id].Get<T>();
    }

    private T DrawElementAbsolute<T>(int id, Func<T> factory, Action<T> storedReflect, int posX, int posY) where T : UIBase
    {
        return DrawElementAbsolute(id, () =>
        {
            T elem = factory.Invoke();
            BinderBase? binder = null;
            return (elem, binder);
        }, (expanded) =>
        {
            storedReflect.Invoke(expanded.Item1);
        },
        posX, posY);
    }

    private Button DrawButtonAbsolute(int id, string buttonText, int posX, int posY, int buttonWidth, int buttonHeight, Action<Button> onButtonPressed, object payload, int fontSize, bool hasBorder, Raylib_cs.Color? fillColor = null, Raylib_cs.Color? borderColor = null, Raylib_cs.Color? textColor = null)
    {
        return DrawElementAbsolute(id, () =>
        {
            return new Button(posX, posY, buttonWidth, buttonHeight, buttonText, onButtonPressed, payload, fontSize, hasBorder, fillColor, borderColor, textColor, defaultParent);
        }, (stored) =>
        {
            stored.ButtonText = buttonText;

            if (fillColor.HasValue) stored.FillColor = fillColor;
            if (borderColor.HasValue) stored.BorderColor = borderColor;
            if (textColor.HasValue) stored.TextColor = textColor;
        }, posX, posY);
    }

    private Selectable DrawSelectableAbsolute(int id, bool isSelected, string selectableText, int posX, int posY, int selectableWidth, int selectableHeight, int fontSize, Action<Selectable> onSelectableSelect, object? payload, Raylib_cs.Color bgColor, Raylib_cs.Color bgSelectionColor, Raylib_cs.Color textColor)
    {
        return DrawElementAbsolute(id, () =>
        {
            return new Selectable(selectableText, isSelected, posX, posY, selectableWidth, selectableHeight, onSelectableSelect, payload, fontSize, bgColor, bgSelectionColor, textColor, defaultParent);
        }, (stored) =>
        {
            if (isSelected != stored.IsSelected)
            {
                if (isSelected) stored.Select(false);
                else stored.Deselect(false);
            }

            stored.SelectableText = selectableText;
        }, posX, posY);
    }

    private InputField DrawInputFieldAbsolute(int id, string placeholderText, string fieldText, int posX, int posY, int inputFieldWidth, int inputFieldHeight, Action<InputField>? onTextEdited, Action<InputField>? onFocusEnd, int fontSize, bool isMasked = false)
    {
        return DrawElementAbsolute(id, () =>
        {
            return new InputField(placeholderText, fieldText, posX, posY, inputFieldWidth, inputFieldHeight, onTextEdited, onFocusEnd, fontSize, isMasked, defaultParent);
        }, (stored) =>
        {
            if (!stored.IsFocused)
            {
                stored.InputFieldText = fieldText;
            }

            stored.IsMasked = isMasked;
            stored.OnTextChanged = onTextEdited;
            stored.OnFocusEnd = onFocusEnd;
        }, posX, posY);
    }

    private Toggle DrawToggleAbsolute(int id, bool toggleValue, int posX, int posY, int toggleWidth, int toggleHeight, Action<Toggle>? onToggleChanged, object? payload)
    {
        return DrawElementAbsolute(id, () =>
        {
            return new Toggle(posX, posY, toggleValue, toggleWidth, toggleHeight, onToggleChanged, payload, 15, defaultParent);
        }, (stored) =>
        {
            stored.Value = toggleValue;
            stored.SetOnToggleChanged(onToggleChanged);
        }, posX, posY);
    }

    private Dropdown DrawDropdownAbsolute(int id, string[] options, int selectedIndex, int posX, int posY, int width, int itemHeight, Action<Dropdown>? onSelectionChanged, object? payload, int fontSize)
    {
        return DrawElementAbsolute(id, () =>
        {
            return new Dropdown(options, selectedIndex, posX, posY, width, itemHeight, onSelectionChanged, payload, fontSize, defaultParent);
        }, (stored) =>
        {
            if (stored.Options.Length != options.Length)
                stored.SetOptions(options, selectedIndex);
            else
            {
                stored.SelectedIndex = selectedIndex;
                stored.SetOnSelectionChanged(onSelectionChanged);
            }
        }, posX, posY);
    }

    private CycleSelector DrawCycleSelectorAbsolute(int id, string[] options, int selectedIndex, int posX, int posY, int width, int height, Action<CycleSelector>? onSelectionChanged, object? payload, int fontSize)
    {
        return DrawElementAbsolute(id, () =>
        {
            return new CycleSelector(options, selectedIndex, posX, posY, width, height, onSelectionChanged, payload, fontSize, defaultParent);
        }, (stored) =>
        {
            stored.SelectedIndex = selectedIndex;
            stored.Options = options;
            stored.SetOnSelectionChanged(onSelectionChanged);
        }, posX, posY);
    }

    private LinkButton DrawLinkButtonAbsolute(int id, string text, string url, int posX, int posY, Action<LinkButton>? onClick, int fontSize)
    {
        return DrawElementAbsolute(id, () =>
        {
            return new LinkButton(posX, posY, text, url, onClick, fontSize, defaultParent);
        }, (stored) =>
        {
            stored.Text = text;
            stored.Url = url;
        }, posX, posY);
    }

    private StatusBadge DrawStatusBadgeAbsolute(int id, string text, StatusType statusType, Raylib_cs.Color? customColor, int posX, int posY, int fontSize)
    {
        return DrawElementAbsolute(id, () =>
        {
            return new StatusBadge(posX, posY, text, statusType, customColor, fontSize, defaultParent);
        }, (stored) =>
        {
            stored.Text = text;
            stored.Type = statusType;

            if (customColor.HasValue) stored.CustomColor = customColor.Value;
        }, posX, posY);
    }

    private AlertBanner DrawAlertBannerAbsolute(int id, string message, AlertType alertType, int posX, int posY, int width, int height, bool isDismissible, int fontSize)
    {
        return DrawElementAbsolute(id, () =>
        {
            return new AlertBanner(posX, posY, message, alertType, width, height, isDismissible, fontSize, defaultParent);
        }, (stored) =>
        {
            stored.Message = message;
            stored.Type = alertType;
        }, posX, posY);
    }

    private Slider DrawSliderAbsolute(int id, float value, float minValue, float maxValue, int posX, int posY, int width, int height, Action<Slider>? onValueChanged, object? payload, bool showValue = true, string? format = null, int fontSize = 13, float? step = null)
    {
        return DrawElementAbsolute(id, () =>
        {
            return new Slider(posX, posY, value, minValue, maxValue, width, height, onValueChanged, payload, showValue, format, fontSize, step, defaultParent);
        }, (stored) =>
        {
            stored.MinValue = minValue;
            stored.MaxValue = maxValue;
            stored.Step = step;
            stored.ShowValue = showValue;
            stored.Format = format;
            stored.FontSize = fontSize;

            if (!stored.IsDragging) stored.SetValueWithoutNotify(value);

            stored.SetOnValueChanged(onValueChanged);
        }, posX, posY);
    }

    public void Text(string text, Raylib_cs.Color fontColor, bool updateLayout = true)
    {
        DrawTextAbsolute(text, PosX_Dynamic(), PosY_Dynamic(), fontColor, new Vector2(0, 0));
        if (updateLayout) DrawAny(text.Length * 2, 20);
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

    public T DrawElement<T>(T element, bool updateLayout = true) where T : UIBase
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        T drawnElem = DrawElementAbsolute(element.Id, () => element, (stored) => { }, (int)pos.X, (int)pos.Y);
        if (updateLayout) DrawAny((int)drawnElem.Width, (int)drawnElem.Height);
        return drawnElem;
    }

    public Button Button(Button button, bool updateLayout = true) => DrawElement(button, updateLayout);
    public Selectable Selectable(Selectable selectable, bool updateLayout = true) => DrawElement(selectable, updateLayout);
    public InputField InputField(InputField inputField, bool updateLayout = true) => DrawElement(inputField, updateLayout);
    public Toggle Toggle(Toggle toggle, bool updateLayout = true) => DrawElement(toggle, updateLayout);
    public Dropdown Dropdown(Dropdown dropdown, bool updateLayout = true) => DrawElement(dropdown, updateLayout);
    public CycleSelector CycleSelector(CycleSelector cycleSelector, bool updateLayout = true) => DrawElement(cycleSelector, updateLayout);
    public LinkButton LinkButton(LinkButton linkButton, bool updateLayout = true) => DrawElement(linkButton, updateLayout);
    public StatusBadge StatusBadge(StatusBadge statusBadge, bool updateLayout = true) => DrawElement(statusBadge, updateLayout);
    public AlertBanner AlertBanner(AlertBanner alertBanner, bool updateLayout = true) => DrawElement(alertBanner, updateLayout);
    public Slider Slider(Slider slider, bool updateLayout = true) => DrawElement(slider, updateLayout);

    public T DrawElement<T>(int id, Func<Vector2, T> drawAbsoluteCaller, bool updateLayout = true) where T : UIBase
    {
        Vector2 pos = new(PosX_Dynamic(), PosY_Dynamic());

        if (defaultParent != null) // then make relative.
            pos -= defaultParent.Position;

        T drawnElem = drawAbsoluteCaller.Invoke(pos);
        if (updateLayout) DrawAny(drawnElem.Width, drawnElem.Height);
        return drawnElem;
    }

    public Button Button(int id, string buttonText, int buttonWidth, int buttonHeight, Action<Button> onButtonPressed, object payload, Raylib_cs.Color? fillColor = null, Raylib_cs.Color? borderColor = null, Raylib_cs.Color? textColor = null, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawButtonAbsolute(id, buttonText, (int)pos.X, (int)pos.Y, buttonWidth, buttonHeight, onButtonPressed, payload, 15, true, fillColor, borderColor, textColor);
        }, updateLayout);
    }

    public Selectable Selectable(int id, bool isSelected, string selectableText, int selectableWidth, int selectableHeight, Action<Selectable> onSelectableSelect, object? payload, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawSelectableAbsolute(id, isSelected, selectableText, (int)pos.X, (int)pos.Y, selectableWidth, selectableHeight, 15, onSelectableSelect, payload, new Raylib_cs.Color((byte)38, (byte)38, (byte)38, (byte)255), new Raylib_cs.Color((byte)28, (byte)50, (byte)88, (byte)255), new Raylib_cs.Color((byte)200, (byte)200, (byte)200, (byte)255));
        }, updateLayout);
    }

    public InputField InputField(int id, string placeholderText, string fieldText, int inputFieldWidth, int inputFieldHeight, Action<InputField>? onTextEdited = null, Action<InputField>? onFocusEnd = null, bool isMasked = false, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawInputFieldAbsolute(id, placeholderText, fieldText, (int)pos.X, (int)pos.Y, inputFieldWidth, inputFieldHeight, onTextEdited, onFocusEnd, 15, isMasked);
        }, updateLayout);
    }

    public Toggle Toggle(int id, bool toggleValue, int toggleWidth, int toggleHeight, Action<Toggle>? onToggleChanged, object? payload, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawToggleAbsolute(id, toggleValue, (int)pos.X, (int)pos.Y, toggleWidth, toggleHeight, onToggleChanged, payload);
        }, updateLayout);
    }

    public Dropdown Dropdown(int id, string[] options, int selectedIndex, int width, int itemHeight, Action<Dropdown>? onSelectionChanged, object? payload, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawDropdownAbsolute(id, options, selectedIndex, (int)pos.X, (int)pos.Y, width, itemHeight, onSelectionChanged, payload, 15);
        }, updateLayout);
    }

    public CycleSelector CycleSelector(int id, string[] options, int selectedIndex, int width, int height, Action<CycleSelector>? onSelectionChanged, object? payload = null, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawCycleSelectorAbsolute(id, options, selectedIndex, (int)pos.X, (int)pos.Y, width, height, onSelectionChanged, payload, 15);
        }, updateLayout);
    }

    public LinkButton LinkButton(int id, string text, string url, Action<LinkButton>? onClick = null, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawLinkButtonAbsolute(id, text, url, (int)pos.X, (int)pos.Y, onClick, 14);
        }, updateLayout);
    }

    public StatusBadge StatusBadge(int id, string text, StatusType statusType = StatusType.Idle, Raylib_cs.Color? customColor = null, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawStatusBadgeAbsolute(id, text, statusType, customColor, (int)pos.X, (int)pos.Y, 13);
        }, updateLayout);
    }

    public AlertBanner AlertBanner(int id, string message, AlertType alertType = AlertType.Error, int width = 360, int height = 32, bool isDismissible = true, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawAlertBannerAbsolute(id, message, alertType, (int)pos.X, (int)pos.Y, width, height, isDismissible, 13);
        }, updateLayout);
    }

    public Slider Slider(int id, float value, float minValue, float maxValue, int width, int height, Action<Slider>? onValueChanged = null, object? payload = null, bool showValue = true, string? format = null, float? step = null, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawSliderAbsolute(id, value, minValue, maxValue, (int)pos.X, (int)pos.Y, width, height, onValueChanged, payload, showValue, format, 13, step);
        }, updateLayout);
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

    // ==================== BINDABLE DRAWING METHODS ====================

    public ElemType DrawBindableElementAbsolute<ElemType, ValType, RLUIType>(int id, BindableValueBase<ValType> dataModel, int posX, int posY, Func<(ElemType, RLUIType)> factory, Action<ElemType> storedReflect = null) where ElemType : UIBase where RLUIType : BindableUIBase<ValType>
    {
        return DrawElementAbsolute(id, () =>
        {
            (ElemType newElem, RLUIType uiBindable) = factory.Invoke();

            Binder<BindableValueBase<ValType>, RLUIType, ValType> binder = new();
            binder.Bind(dataModel, uiBindable);

            return (newElem, binder);
        }, ((ElemType stored, BinderBase? binder) expanded) =>
        {
            storedReflect?.Invoke(expanded.stored);

            if (expanded.binder is Binder<BindableValueBase<ValType>, RLUIType, ValType> binder)
            {
                if (binder.GetBoundValObject() != dataModel)
                {
                    binder.Unbind();

                    if (binder.GetBoundUIObject() is RLUIType uiTarget)
                        binder.Bind(dataModel, uiTarget);
                }
            }
        }, posX, posY);
    }

    public Toggle BindableToggle(int id, BindableValueBase<bool> dataModel, int width, int height, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawBindableElementAbsolute(id, dataModel, (int)pos.X, (int)pos.Y, () =>
            {
                Toggle newToggle = new((int)pos.X, (int)pos.Y, dataModel.Get(), width, height, null, id, 15, defaultParent);
                RLToggleUI newUIBindable = new(newToggle);
                return (newToggle, newUIBindable);
            });
        }, updateLayout);
    }

    public InputField BindableInputFieldString(int id, string placeholderText, BindableValueBase<string> dataModel, int width, int height, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawBindableElementAbsolute(id, dataModel, (int)pos.X, (int)pos.Y, () =>
            {
                InputField newInputField = new(placeholderText, dataModel.Get(), (int)pos.X, (int)pos.Y, width, height, null, null, 15, false, defaultParent);
                RLInputFieldUI_String newUIBindable = new(newInputField);
                return (newInputField, newUIBindable);
            });
        }, updateLayout);
    }

    public InputField BindableInputFieldInt(int id, string placeholderText, BindableValueBase<int> dataModel, int width, int height, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawBindableElementAbsolute(id, dataModel, (int)pos.X, (int)pos.Y, () =>
            {
                InputField newInputField = new(placeholderText, dataModel.Get().ToString(), (int)pos.X, (int)pos.Y, width, height, null, null, 15, false, defaultParent);
                RLInputFieldUI_Int newUIBindable = new(newInputField);
                return (newInputField, newUIBindable);
            });
        }, updateLayout);
    }

    public InputField BindableInputFieldFloat(int id, string placeholderText, BindableValueBase<float> dataModel, int width, int height, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawBindableElementAbsolute(id, dataModel, (int)pos.X, (int)pos.Y, () =>
            {
                InputField newInputField = new(placeholderText, dataModel.Get().ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture), (int)pos.X, (int)pos.Y, width, height, null, null, 15, false, defaultParent);
                RLInputFieldUI_Float newUIBindable = new(newInputField);
                return (newInputField, newUIBindable);
            });
        }, updateLayout);
    }

    public Selectable BindableSelectable(int id, BindableValueBase<bool> dataModel, string selectableText, int width, int height, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawBindableElementAbsolute(id, dataModel, (int)pos.X, (int)pos.Y, () =>
            {
                Selectable selectable = new(selectableText, dataModel.Get(), (int)pos.X, (int)pos.Y, width, height, (sel) => { }, id, 15, Raylib_cs.Color.Gray, Raylib_cs.Color.Blue, Raylib_cs.Color.White, defaultParent);
                RLSelectableUI newUIBindable = new(selectable);
                return (selectable, newUIBindable);
            });
        }, updateLayout);
    }

    public Dropdown BindableDropdown(int id, string[] options, BindableValueBase<int> dataModel, int width, int height, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawBindableElementAbsolute(id, dataModel, (int)pos.X, (int)pos.Y, () =>
            {
                Dropdown dropdown = new(options, dataModel.Get(), (int)pos.X, (int)pos.Y, width, height, null, id, 15, defaultParent);
                RLDropdownUI newUIBindable = new(dropdown);
                return (dropdown, newUIBindable);
            });
        }, updateLayout);
    }

    public Slider BindableSlider(int id, BindableValueBase<float> dataModel, float minValue, float maxValue, int width, int height, bool showValue = true, string? format = null, float? step = null, int fontSize = 13, bool updateLayout = true)
    {
        return DrawElement(id, (pos) =>
        {
            return DrawBindableElementAbsolute(id, dataModel, (int)pos.X, (int)pos.Y, () =>
            {
                Slider slider = new((int)pos.X, (int)pos.Y, dataModel.Get(), minValue, maxValue, width, height, null, id, showValue, format, fontSize, step, defaultParent);
                RLSliderUI newUIBindable = new(slider);
                return (slider, newUIBindable);
            }, (stored) =>
            {
                stored.MinValue     = minValue;
                stored.MaxValue     = maxValue;
                stored.Step         = step;
                stored.ShowValue    = showValue;
                stored.Format       = format;
                stored.FontSize     = fontSize;
            });
        }, updateLayout);
    }
}