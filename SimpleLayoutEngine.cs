using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

using RaylibNodeLibrary;
using RaylibNodeLibrary.UI;

namespace RlSimpleLayoutEngine;

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

public struct RaylibScrollData
{
    public Rectangle PanelRec { get; set; }
    public Rectangle PanelContentRec { get; set; }
    public Rectangle PanelView { get; set; }
    public Vector2 PanelScroll { get; set; }
}

public class RlSimpleLayout
{
    private LayoutOperation[] layoutOps = new LayoutOperation[10];

    private int layoutOpsIdx = -1;
    private int lastHorizontalIdx = -1;
    private int lastVerticalIdx = -1;

    private int scrollWidthCache = -1;
    private int scrollHeightCache = -1;

    private Font textFont;

    private Dictionary<int, Button> layoutButtons = [];
    private Dictionary<int, Selectable> layoutSelectables = [];
    private Dictionary<int, InputField> layoutInputFields = [];

    public void InitFont(Font font)
    {
        textFont = font;
    }

    public void ResetLayout()
    {
        layoutOpsIdx = -1;
        lastHorizontalIdx = -1;
        lastVerticalIdx = -1;

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
    }

    public void UpdateLayoutElements()
    {
        List<Button> lbs = [.. layoutButtons.Select((kvp) => kvp.Value)];
        for (int i = 0; i < lbs.Count; i++)
            lbs[i].Update();

        List<Selectable> lss = [.. layoutSelectables.Select((kvp) => kvp.Value)];
        for (int i = 0; i < lss.Count; i++)
            lss[i].Update();

        List<InputField> lifs = [.. layoutInputFields.Select((kvp) => kvp.Value)];
        for (int i = 0; i < lifs.Count; i++)
            lifs[i].Update();
    }

    public void RemoveLayoutButton(int id)
    {
        _ = layoutButtons.Remove(id);
    }

    public void RemoveLayoutSelectable(int id)
    {
        _ = layoutSelectables.Remove(id);
    }

    public void RemoveLayoutInputField(int id)
    {
        _ = layoutInputFields.Remove(id);
    }

    public Drawable? HitTestElements(Vector2 mouseScreenPosition, Vector2 mouseWorldPosition)
    {
        List<Button> lbs = [.. layoutButtons.Select((kvp) => kvp.Value)];
        for (int i = lbs.Count - 1; i >= 0; i--)
        {
            var hit = lbs[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        List<Selectable> lss = [.. layoutSelectables.Select((kvp) => kvp.Value)];
        for (int i = lss.Count - 1; i >= 0; i--)
        {
            var hit = lss[i].HitTest(mouseScreenPosition, mouseWorldPosition);
            if (hit != null) return hit;
        }

        List<InputField> lifs = [.. layoutInputFields.Select((kvp) => kvp.Value)];
        for (int i = lifs.Count - 1; i >= 0; i--)
        {
            var hit = lifs[i].HitTest(mouseScreenPosition, mouseWorldPosition);
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

    public int GetScrollH(RaylibScrollData scrollData)
    {
        return (int)scrollData.PanelContentRec.Width;
    }

    public int GetScrollV(RaylibScrollData scrollData)
    {
        return (int)scrollData.PanelContentRec.Height;
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

    private void DrawText_Internal(string text, int posX, int posY, Color fontColor, Vector2 offset)
    {
        DrawTextEx(textFont, text, new Vector2(posX + (int)offset.X, posY + (int)offset.Y), 15, 0.5f, fontColor);
    }

    private void DrawPanel_Internal(int posX, int posY, int width, int height, Color panelColor)
    {
        DrawRectangle(posX, posY, width, height, panelColor);
    }

    private void DrawTextPanel_Internal(string text, int posX, int posY, int panelWidth, int panelHeight, Color panelColor, Color textColor, Vector2 textOffset)
    {
        DrawPanel_Internal(posX, posY, panelWidth, panelHeight, panelColor);
        DrawText_Internal(text, posX, posY, textColor, textOffset);
    }

    private void DrawSection_Internal(string heading, int posX, int posY, int width, int heigth, float headingPercent, int fontSize, Color headingBgColor, Color bodyBgColor, Color fontColor)
    {
        int headingHeight = (int)(heigth * headingPercent);

        DrawPanel_Internal(posX, posY + headingHeight, width, heigth - headingHeight, bodyBgColor);

        if (headingHeight != 0)
            DrawTextPanel_Internal(heading, posX, posY, width, headingHeight, headingBgColor, fontColor, new Vector2(5, 0));
        else
            DrawTextEx(textFont, heading, new Vector2(posX + 5, posY), fontSize, 0.5f, fontColor);
    }

    private void DrawButton_Internal(Button button, int posX, int posY)
    {
        if (!layoutButtons.TryGetValue(button.Id, out Button? cachedButton))
        {
            cachedButton = button;
            layoutButtons.Add(button.Id, cachedButton);
        }

        cachedButton.RelativePosition = new Vector2(posX, posY);
        cachedButton.Render();
    }

    private void DrawButton_Internal(int id, string buttonText, int posX, int posY, int buttonWidth, int buttonHeight, Action<Button> onButtonPressed, object payload, int fontSize, bool hasBorder, Color buttonColor, Color textColor)
    {
        if (!layoutButtons.TryGetValue(id, out Button? cachedButton))
        {
            cachedButton = new Button(buttonWidth, buttonHeight, buttonText, onButtonPressed, payload, fontSize, hasBorder, buttonColor, textColor, null);
            layoutButtons.Add(id, cachedButton);
        }

        cachedButton.RelativePosition = new Vector2(posX, posY);
        cachedButton.Render();
    }

    private void DrawSelectable_Internal(Selectable selectable, int posX, int posY)
    {
        if (!layoutSelectables.TryGetValue(selectable.Id, out Selectable? cachedSelectable))
        {
            cachedSelectable = selectable;
            layoutSelectables.Add(selectable.Id, cachedSelectable);
        }

        cachedSelectable.RelativePosition = new Vector2(posX, posY);
        cachedSelectable.Render();
    }

    private Selectable DrawSelectable_Internal(int id, string selectableText, int posX, int posY, int selectableWidth, int selectableHeight, int fontSize, Action<Selectable> onSelectableSelect, object payload, Color bgColor, Color bgSelectionColor, Color textColor)
    {
        if (!layoutSelectables.TryGetValue(id, out Selectable? cachedSelectable))
        {
            cachedSelectable = new Selectable(selectableText, posX, posY, selectableWidth, selectableHeight, onSelectableSelect, payload, fontSize, bgColor, bgSelectionColor, textColor, null);
            layoutSelectables.Add(id, cachedSelectable);
        }

        cachedSelectable.RelativePosition = new Vector2(posX, posY);
        cachedSelectable.Render();

        return cachedSelectable;
    }

    private void DrawInputField_Internal(int id, string placeholderText, string startingText, int posX, int posY, int inputFieldWidth, int inputFieldHeight, int fontSize, Color bgColor, Color textColor)
    {
        if (!layoutInputFields.TryGetValue(id, out InputField? cachedInputField))
        {
            cachedInputField = new InputField(placeholderText, startingText, posX, posY, inputFieldWidth, inputFieldHeight, fontSize, bgColor, textColor);
            layoutInputFields.Add(id, cachedInputField);
        }

        cachedInputField.RelativePosition = new Vector2(posX, posY);
        cachedInputField.Render();
    }

    public void Text(string text, Color fontColor, bool updateLayout = true)
    {
        DrawText_Internal(text, PosX_Dynamic(), PosY_Dynamic(), fontColor, new Vector2(0, 0));
        if (updateLayout) DrawAny(text.Length * 2, 20);
    }

    public void Panel(int width, int height, Color panelColor, bool updateLayout = true)
    {
        DrawPanel_Internal(PosX_Dynamic(), PosY_Dynamic(), width, height, panelColor);
        if (updateLayout) DrawAny(width, height);
    }

    public void TextPanelFixed(string text, int x, int y, int panelWidth, int panelHeight, Color panelColor, Color textColor, bool updateLayout = true)
    {
        PosX_Dynamic();
        PosY_Dynamic();

        DrawTextPanel_Internal(text, x, y, panelWidth, panelHeight, panelColor, textColor, new Vector2(5, 0));
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void TextPanelPro(string text, int panelWidth, int panelHeight, Color panelColor, Color textColor, bool updateLayout = true)
    {
        DrawTextPanel_Internal(text, PosX_Dynamic(), PosY_Dynamic(), panelWidth, panelHeight, panelColor, textColor, new Vector2(5, 0));
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void TextPanelEx(string text, int panelWidth, int panelHeight, Vector2 panelOffset, bool updateLayout = true)
    {
        DrawTextPanel_Internal(text, PosX_Dynamic(), PosY_Dynamic(), panelWidth, panelHeight, Color.Gray, Color.LightGray, panelOffset);
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void TextPanel(string text, int panelWidth, int panelHeight, bool updateLayout = true)
    {
        DrawTextPanel_Internal(text, PosX_Dynamic(), PosY_Dynamic(), panelWidth, panelHeight, Color.LightGray, Color.DarkGray, new Vector2(5, 0));
        if (updateLayout) DrawAny(panelWidth, panelHeight);
    }

    public void SectionEx(string heading, int width, int height, Color headingBgColor, Color bodyBgColor, Color fontColor, float headerPerc, bool updateLayout = true)
    {
        DrawSection_Internal(heading, PosX_Dynamic(), PosY_Dynamic(), width, height, headerPerc, 20, headingBgColor, bodyBgColor, fontColor);
        if (updateLayout) DrawAny(width, height);
    }

    public void Section(string heading, int width, int heigth, float headerPerc, bool updateLayout = true)
    {
        DrawSection_Internal(heading, PosX_Dynamic(), PosY_Dynamic(), width, heigth, headerPerc, 20, Color.DarkGray, Color.Gray, Color.LightGray);
        if (updateLayout) DrawAny(width, heigth);
    }

    public void Button(Button button, bool updateLayout = true)
    {
        DrawButton_Internal(button, PosX_Dynamic(), PosY_Dynamic());
        if (updateLayout) DrawAny((int)button.Width, (int)button.Height);
    }

    public void Button(int id, string buttonText, int buttonWidth, int buttonHeight, Action<Button> onButtonPressed, object payload, bool updateLayout = true)
    {
        DrawButton_Internal(id, buttonText, PosX_Dynamic(), PosY_Dynamic(), buttonWidth, buttonHeight, onButtonPressed, payload, 15, true, Color.LightGray, Color.DarkGray);
        if (updateLayout) DrawAny(buttonWidth, buttonHeight);
    }

    public void Selectable(Selectable selectable, bool updateLayout = true)
    {
        DrawSelectable_Internal(selectable, PosX_Dynamic(), PosY_Dynamic());
        if (updateLayout) DrawAny(selectable.Width, selectable.Height);
    }

    public Selectable Selectable(int id, string selectableText, int selectableWidth, int selectableHeight, Action<Selectable> onSelectableSelect, object payload, bool updateLayout = true)
    {
        Selectable selectable = DrawSelectable_Internal(id, selectableText, PosX_Dynamic(), PosY_Dynamic(), selectableWidth, selectableHeight, 15, onSelectableSelect, payload, new Color((byte)175, (byte)175, (byte)175, (byte)255), new Color((byte)175, (byte)175, (byte)255, (byte)255), Color.Black);
        if (updateLayout) DrawAny(selectableWidth, selectableHeight);

        return selectable;
    }

    public void InputField(int id, string placeholderText, string startingText, int inputFieldWidth, int inputFieldHeight, bool updateLayout = true)
    {
        DrawInputField_Internal(id, placeholderText, startingText, PosX_Dynamic(), PosY_Dynamic(), inputFieldWidth, inputFieldHeight, 15, Color.LightGray, Color.Black);
        if (updateLayout) DrawAny(inputFieldWidth, inputFieldHeight);
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

    public Rectangle GetRectFromScrollView(RaylibScrollData scrollData)
    {
        return new Rectangle(scrollData.PanelRec.X + scrollData.PanelScroll.X, scrollData.PanelRec.Y + scrollData.PanelScroll.Y, scrollData.PanelContentRec.Width, scrollData.PanelContentRec.Height);
    }

    public void BeginScrollView(ref RaylibScrollData scrollData, int width, int height, Vector2 panelContentSize, out Vector2 panelScrollOut)
    {
        int xx = PosX();
        int yy = PosY();

        scrollData.PanelRec = new Rectangle(xx, yy, width, height);
        scrollData.PanelContentRec = new Rectangle(xx, yy, panelContentSize.X, panelContentSize.Y);

        //GuiScrollPanel(scrollData.PanelRec, null, scrollData.PanelContentRec, &scrollData.PanelScroll, &scrollData.PanelView);
        panelScrollOut = scrollData.PanelScroll;

        BeginScissorMode((int)scrollData.PanelView.X, (int)scrollData.PanelView.Y, (int)scrollData.PanelView.Width, (int)scrollData.PanelView.Height);

        Rectangle r = GetRectFromScrollView(scrollData);

        BeginHorizontalEx(0, (int)r.X);
        BeginVerticalEx(0, (int)r.Y);

        scrollWidthCache = (int)r.Width;
        scrollHeightCache = (int)r.Height;
    }

    public void EndScrollView(int finalSize)
    {
        EndVertical(scrollHeightCache);
        EndHorizontal(scrollWidthCache);

        scrollHeightCache = scrollWidthCache = -1;

        EndScissorMode();
        DrawAny(finalSize, finalSize);
    }

    public void AddSpace(int space)
    {
        DrawAny(space, space);
    }

    public void NotifyDraw(int width, int height)
    {
        DrawAny(width, height);
    }
}