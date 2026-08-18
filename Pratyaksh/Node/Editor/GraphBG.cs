using System.Numerics;
using Pratyaksh.Core;
using Raylib_cs;

namespace Pratyaksh.Node.Editor;

public struct GridDesc(float gridSpacing, float thickness, Color gridColor)
{
    public float gridSpacing = gridSpacing;
    public float thickness = thickness;
    public Color gridColor = gridColor;
}

public class GraphBG : Actor
{
    private int graphWidth;
    private int graphHeight;
    private Vector2 graphOffset;

    private readonly GridDesc[] grids;

    public GraphBG(int graphWidth, int graphHeight, Vector2? graphOffset = null) : base(null)
    {
        treeInteractable = false;

        this.graphWidth = graphWidth;
        this.graphHeight = graphHeight;

        grids = new GridDesc[2];

        grids[0] = new GridDesc(10f, 1.5f, new(100, 100, 100, 40));
        grids[1] = new GridDesc(100f, 1.5f, new(0, 0, 0, 150));

        this.graphOffset = graphOffset ?? Vector2.Zero;
    }

    public GraphBG(int graphWidth, int graphHeight, GridDesc[] grids, Vector2? graphOffset = null) : base(null)
    {
        treeInteractable = false;

        this.graphWidth = graphWidth;
        this.graphHeight = graphHeight;

        this.grids = grids;

        this.graphOffset = graphOffset ?? Vector2.Zero;
    }

    private static void DrawGrid(float startX, float startY, float endX, float endY, float spacing, float thickness, Color gridColor)
    {
        float firstX = MathF.Floor(startX / spacing) * spacing;
        float firstY = MathF.Floor(startY / spacing) * spacing;

        for (float x = firstX; x <= endX; x += spacing)
        {
            if (x >= startX)
                Raylib.DrawLineEx(new Vector2(x, startY), new Vector2(x, endY), thickness, gridColor);
        }

        for (float y = firstY; y <= endY; y += spacing)
        {
            if (y >= startY)
                Raylib.DrawLineEx(new Vector2(startX, y), new Vector2(endX, y), thickness, gridColor);
        }
    }

    public void DrawGraph()
    {
        IWorldToScreenTransformer transformer = Engine.Instance.InteractionManager.WorldToScreenTransformer;

        if (transformer == null) return;

        // 1. Get visible bounds in world coordinates
        Vector2 screenTopLeft = Vector2.Zero;
        Vector2 screenBottomRight = new(transformer.GetWidth(), transformer.GetHeight());
        Vector2 worldTopLeft = transformer.ScreenToWorld(screenTopLeft);
        Vector2 worldBottomRight = transformer.ScreenToWorld(screenBottomRight);

        float viewMinX = MathF.Min(worldTopLeft.X, worldBottomRight.X);
        float viewMaxX = MathF.Max(worldTopLeft.X, worldBottomRight.X);
        float viewMinY = MathF.Min(worldTopLeft.Y, worldBottomRight.Y);
        float viewMaxY = MathF.Max(worldTopLeft.Y, worldBottomRight.Y);

        // 2. Clamp visible area to graph bounds
        float gridMinX = graphOffset.X;
        float gridMaxX = graphOffset.X + graphWidth;
        float gridMinY = graphOffset.Y;
        float gridMaxY = graphOffset.Y + graphHeight;

        float startX = MathF.Max(viewMinX, gridMinX);
        float endX = MathF.Min(viewMaxX, gridMaxX);
        float startY = MathF.Max(viewMinY, gridMinY);
        float endY = MathF.Min(viewMaxY, gridMaxY);

        if (startX >= endX || startY >= endY)
            return;

        for (int i = 0; i < grids.Length; i++)
            DrawGrid(startX, startY, endX, endY, grids[i].gridSpacing, grids[i].thickness, grids[i].gridColor);
    }

    protected override void OnDraw()
    {
        DrawGraph();
    }
}
