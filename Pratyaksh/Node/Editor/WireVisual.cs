using System.Numerics;
using Pratyaksh.Core;
using Raylib_cs;

namespace Pratyaksh.Node.Editor;

public class ObjectOrPosition(bool isPosition, Vector2 position, EditorObject? editorObject)
{
    private bool isPosition = isPosition;
    private Vector2 position = position;
    private EditorObject? editorObject = editorObject;

    public bool IsPosition { get => isPosition; }
    public EditorObject? EditorObject { get => editorObject; }

    public Vector2 GetPos()
    {
        if (isPosition)
            return position;
        else return editorObject == null ? Vector2.Zero : editorObject.Position;
    }

    public void SetPos(EditorObject editorObject)
    {
        this.editorObject = editorObject;
        isPosition = false;
    }

    public void SetPos(Vector2 position)
    {
        this.position = position;
        isPosition = true;
    }
}

public class WireVisual : Actor
{
    private ObjectOrPosition wireStart;
    private ObjectOrPosition wireEnd;

    private float wireThickness = 1.5f;

    private Color wireColor = Color.White;

    public void SetColor(Color color)
    {
        wireColor = color;
    }

    public void SetThickness(float thickness)
    {
        wireThickness = thickness;
    }

    public Vector2 WireStart { get => wireStart.GetPos(); }
    public Vector2 WireEnd { get => wireEnd.GetPos(); }

    public WireVisual(Drawable parent) : base(parent)
    {
        ResetWire();
    }

    public void ResetWire()
    {
        wireStart = new ObjectOrPosition(true, Vector2.Zero, null);
        wireEnd = new ObjectOrPosition(true, Vector2.Zero, null);
        Hide();
    }

    public void SetStartPos(Vector2 newStartPos)
    {
        wireStart.SetPos(newStartPos);
    }

    public void SetEndPos(Vector2 newEndPos)
    {
        wireEnd.SetPos(newEndPos);
    }

    public void SetStartPos(EditorObject newStartObj)
    {
        wireStart.SetPos(newStartObj);
    }

    public void SetEndPos(EditorObject newEndObj)
    {
        wireEnd.SetPos(newEndObj);
    }

    protected override void OnDraw()
    {
        Raylib.DrawLineBezier(wireStart.GetPos(), wireEnd.GetPos(), wireThickness, wireColor);
    }

    public void NotifyDeleted(PortVisual portUI)
    {
        bool flag = false;

        if (!wireStart.IsPosition && (wireStart.EditorObject == portUI || wireEnd.EditorObject == portUI))
            flag = true;

        if (flag) ResetWire();
    }
}
