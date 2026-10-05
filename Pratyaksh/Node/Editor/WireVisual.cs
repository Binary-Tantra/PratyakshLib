using System.Numerics;
using Pratyaksh.Core;
using Raylib_cs;

namespace Pratyaksh.Node.Editor;

public class ObjectOrPosition(bool isPosition, Vector3 position, EditorObject? editorObject)
{
    private bool isPosition = isPosition;
    private Vector3 position = position;
    private EditorObject? editorObject = editorObject;

    public bool IsPosition { get => isPosition; }
    public EditorObject? EditorObject { get => editorObject; }

    public Vector3 GetPos()
    {
        if (isPosition)
            return position;
        else return editorObject == null ? Vector3.Zero : editorObject.Transform.Position;
    }

    public void SetPos(EditorObject editorObject)
    {
        this.editorObject = editorObject;
        isPosition = false;
    }

    public void SetPos(Vector3 position)
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

    public Vector3 WireStart { get => wireStart.GetPos(); }
    public Vector3 WireEnd { get => wireEnd.GetPos(); }

    public WireVisual(Pratyaksh.Core.Transform parent) : base(parent)
    {
        ResetWire();
    }

    public void ResetWire()
    {
        wireStart = new ObjectOrPosition(true, Vector3.Zero, null);
        wireEnd = new ObjectOrPosition(true, Vector3.Zero, null);
        renderingComponent.Hide();
    }

    public void SetStartPos(Vector3 newStartPos)
    {
        wireStart.SetPos(newStartPos);
    }

    public void SetEndPos(Vector3 newEndPos)
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
        Vector3 p0 = wireStart.GetPos();
        Vector3 p3 = wireEnd.GetPos();
        float dist = Vector3.Distance(p0, p3);
        float offset = Math.Max(dist * 0.5f, 35.0f);

        Vector3 t0;
        Vector3 t3;

        PortVisual? sp = wireStart.EditorObject as PortVisual;
        PortVisual? ep = wireEnd.EditorObject as PortVisual;

        if (sp != null && ep != null)
        {
            t0 = sp.GetBezierTangent(offset);
            t3 = ep.GetBezierTangent(offset);
        }
        else if (sp != null)
        {
            t0 = sp.GetBezierTangent(offset);
            t3 = -t0;
        }
        else if (ep != null)
        {
            t3 = ep.GetBezierTangent(offset);
            t0 = -t3;
        }
        else
        {
            t0 = new Vector3(offset, 0, 0);
            t3 = new Vector3(-offset, 0, 0);
        }

        Vector3 p1 = p0 + t0;
        Vector3 p2 = p3 + t3;

        Raylib.DrawSplineSegmentBezierCubic(p0.AsVector2(), p1.AsVector2(), p2.AsVector2(), p3.AsVector2(), wireThickness, wireColor);
    }

    public void NotifyDeleted(PortVisual portUI)
    {
        bool flag = false;

        if (!wireStart.IsPosition && (wireStart.EditorObject == portUI || wireEnd.EditorObject == portUI))
            flag = true;

        if (flag) ResetWire();
    }
}
