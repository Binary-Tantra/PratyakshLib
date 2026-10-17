using System.Numerics;
using Pratyaksh.Core;
using Pratyaksh.UI;
using Raylib_cs;
using Transform = Pratyaksh.Core.Transform;
using MouseButton = Pratyaksh.Core.MouseButton;

namespace Pratyaksh.Node.Editor.UI;

public class MinimapPanel : UILayoutBase
{
    private readonly EditorCamera2D camera;

    private bool isRightDragging = false;
    private Vector2 dragWorldOffset = Vector2.Zero;
    private Pratyaksh.Core.Rectangle activeWorldBounds;
    private Pratyaksh.Core.Rectangle visualWorldBounds;
    private Pratyaksh.Core.Rectangle lerpStartBounds;
    private bool isLerping = false;
    private float lerpTimer = 0f;
    private const float LERP_DURATION = 0.5f;

    private const float WORLD_PADDING = 150f;
    private const float PANEL_ROUNDNESS = 0.08f;
    private const int PANEL_ROUND_SEGMENTS = 8;
    private const float OUTLINE_THICKNESS = 2.0f;

    private static readonly Color BgColor = new(22, 22, 22, 230);
    private static readonly Color BorderColor = new(110, 110, 110, 220);
    private static readonly Color CamBoxFillColor = Raylib.Fade(Color.White, 0.08f);
    private static readonly Color CamBoxOutlineColor = Raylib.Fade(Color.White, 0.4f);

    protected override string PanelName => "Minimap";

    public MinimapPanel(int posX, int posY, EditorCamera2D camera, Transform? parent, ParentBasis? parentBasis = ParentBasis.BottomRight)
        : base(posX, posY, 240, 160, parent, parentBasis)
    {
        this.camera = camera;
        selfInteractable = true;
        activeWorldBounds = CalculateWorldBounds();
        visualWorldBounds = activeWorldBounds;
        lerpStartBounds = activeWorldBounds;
    }

    public override void OnDrawLayout()
    {
        // Headerless panel: all custom visual drawing is handled in OnDraw
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        if (isLerping)
        {
            lerpTimer += Raylib.GetFrameTime();
            float t = Math.Clamp(lerpTimer / LERP_DURATION, 0f, 1f);
            float smoothT = t * t * (3f - 2f * t);

            visualWorldBounds = new Pratyaksh.Core.Rectangle(
                Lerp(lerpStartBounds.X, activeWorldBounds.X, smoothT),
                Lerp(lerpStartBounds.Y, activeWorldBounds.Y, smoothT),
                Lerp(lerpStartBounds.Width, activeWorldBounds.Width, smoothT),
                Lerp(lerpStartBounds.Height, activeWorldBounds.Height, smoothT)
            );

            if (t >= 1f)
            {
                isLerping = false;
                visualWorldBounds = activeWorldBounds;
            }
        }
        else if (!isRightDragging)
        {
            activeWorldBounds = CalculateWorldBounds();
            visualWorldBounds = activeWorldBounds;
        }
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    protected override void OnDraw()
    {
        Raylib_cs.Rectangle panelRect = new(UITransform.X, UITransform.Y, UITransform.Width, UITransform.Height);

        // 1. Draw rounded background
        Raylib.DrawRectangleRounded(panelRect, PANEL_ROUNDNESS, PANEL_ROUND_SEGMENTS, BgColor);

        // 2. Use visual bounds for rendering
        Pratyaksh.Core.Rectangle bounds = visualWorldBounds;
        Pratyaksh.Core.Rectangle mapContentRect = new(UITransform.X + 3, UITransform.Y + 3, UITransform.Width - 6, UITransform.Height - 6);

        float scale = Math.Min(mapContentRect.Width / bounds.Width, mapContentRect.Height / bounds.Height);
        float offsetX = mapContentRect.X + (mapContentRect.Width - bounds.Width * scale) * 0.5f;
        float offsetY = mapContentRect.Y + (mapContentRect.Height - bounds.Height * scale) * 0.5f;

        // 3. Clipped content drawing
        Raylib.BeginScissorMode((int)panelRect.X, (int)panelRect.Y, (int)panelRect.Width, (int)panelRect.Height);
        {
            // Draw connections
            DrawConnections(bounds, scale, offsetX, offsetY);

            // Draw nodes
            DrawNodes(bounds, scale, offsetX, offsetY);

            // Draw camera viewport box
            DrawCameraBox(bounds, scale, offsetX, offsetY);
        }
        Raylib.EndScissorMode();

        // 4. Draw rounded outline on top
        Raylib.DrawRectangleRoundedLinesEx(panelRect, PANEL_ROUNDNESS, PANEL_ROUND_SEGMENTS, OUTLINE_THICKNESS, BorderColor);
    }

    private void DrawConnections(Pratyaksh.Core.Rectangle bounds, float scale, float offsetX, float offsetY)
    {
        if (NodeEditorEngine.Graph?.Connections == null)
            return;

        foreach (var conn in NodeEditorEngine.Graph.Connections.Values)
        {
            if (NodeEditorEngine.PortToPortUIDict.TryGetValue(conn.SourcePortId, out PortVisual? srcPort) &&
                NodeEditorEngine.PortToPortUIDict.TryGetValue(conn.TargetPortId, out PortVisual? tgtPort) &&
                srcPort != null && tgtPort != null)
            {
                Vector2 p0 = WorldToMinimap(new Vector2(srcPort.Transform.Position.X, srcPort.Transform.Position.Y), bounds, scale, offsetX, offsetY);
                Vector2 p3 = WorldToMinimap(new Vector2(tgtPort.Transform.Position.X, tgtPort.Transform.Position.Y), bounds, scale, offsetX, offsetY);

                float dist = Vector2.Distance(p0, p3);
                float offset = Math.Max(dist * 0.5f, 4f);

                Vector3 t0w = srcPort.GetBezierTangent(offset / Math.Max(scale, 0.0001f));
                Vector3 t3w = tgtPort.GetBezierTangent(offset / Math.Max(scale, 0.0001f));

                Vector2 t0 = new Vector2(t0w.X, t0w.Y) * scale;
                Vector2 t3 = new Vector2(t3w.X, t3w.Y) * scale;

                Vector2 p1 = p0 + t0;
                Vector2 p2 = p3 + t3;

                Raylib.DrawSplineSegmentBezierCubic(p0, p1, p2, p3, 1.2f, srcPort.PortColor);
            }
        }
    }

    private void DrawNodes(Pratyaksh.Core.Rectangle bounds, float scale, float offsetX, float offsetY)
    {
        if (NodeEditorEngine.NodeToNodeUIDict == null)
            return;

        foreach (var nodeUI in NodeEditorEngine.NodeToNodeUIDict.Values)
        {
            if (nodeUI == null) continue;

            Vector2 nodePos = WorldToMinimap(new Vector2(nodeUI.WorldRect.X, nodeUI.WorldRect.Y), bounds, scale, offsetX, offsetY);
            float nodeW = Math.Max(nodeUI.WorldRect.Width * scale, 4f);
            float nodeH = Math.Max(nodeUI.WorldRect.Height * scale, 3f);

            Raylib_cs.Rectangle nRect = new(nodePos.X, nodePos.Y, nodeW, nodeH);

            // Node body fill
            Raylib.DrawRectangleRec(nRect, nodeUI.BodyFillColor);

            // Node header fill
            if (nodeUI.ShowHeader)
            {
                float headH = Math.Max(nodeUI.HeaderWorldRect.Height * scale, 2f);
                Raylib_cs.Rectangle hRect = new(nodePos.X, nodePos.Y, nodeW, Math.Min(headH, nodeH));
                Raylib.DrawRectangleRec(hRect, nodeUI.HeaderFillColor);
            }

            // Node border
            Raylib.DrawRectangleLinesEx(nRect, 1.0f, nodeUI.BorderColor);
        }
    }

    private void DrawCameraBox(Pratyaksh.Core.Rectangle bounds, float scale, float offsetX, float offsetY)
    {
        GetCameraWorldExtents(out float camMinX, out float camMinY, out float camMaxX, out float camMaxY);

        Vector2 camTL = WorldToMinimap(new Vector2(camMinX, camMinY), bounds, scale, offsetX, offsetY);
        Vector2 camBR = WorldToMinimap(new Vector2(camMaxX, camMaxY), bounds, scale, offsetX, offsetY);

        Raylib_cs.Rectangle camRect = new(camTL.X, camTL.Y, camBR.X - camTL.X, camBR.Y - camTL.Y);

        Raylib.DrawRectangleRec(camRect, CamBoxFillColor);
        Raylib.DrawRectangleLinesEx(camRect, 1.5f, CamBoxOutlineColor);
    }

    private void GetCameraWorldExtents(out float minX, out float minY, out float maxX, out float maxY)
    {
        Vector2 screenDim = new(camera.GetWidth(), camera.GetHeight());
        Vector2 worldTL = camera.ScreenToWorld(Vector2.Zero);
        Vector2 worldBR = camera.ScreenToWorld(screenDim);

        minX = Math.Min(worldTL.X, worldBR.X);
        maxX = Math.Max(worldTL.X, worldBR.X);
        minY = Math.Min(worldTL.Y, worldBR.Y);
        maxY = Math.Max(worldTL.Y, worldBR.Y);
    }

    private Pratyaksh.Core.Rectangle CalculateWorldBounds()
    {
        GetCameraWorldExtents(out float minX, out float minY, out float maxX, out float maxY);

        if (NodeEditorEngine.NodeToNodeUIDict != null)
        {
            foreach (var nodeUI in NodeEditorEngine.NodeToNodeUIDict.Values)
            {
                if (nodeUI == null) continue;

                var wr = nodeUI.WorldRect;
                if (wr.X < minX) minX = wr.X;
                if (wr.Y < minY) minY = wr.Y;
                if (wr.X + wr.Width > maxX) maxX = wr.X + wr.Width;
                if (wr.Y + wr.Height > maxY) maxY = wr.Y + wr.Height;
            }
        }

        minX -= WORLD_PADDING;
        maxX += WORLD_PADDING;
        minY -= WORLD_PADDING;
        maxY += WORLD_PADDING;

        float width = Math.Max(maxX - minX, 100f);
        float height = Math.Max(maxY - minY, 100f);

        return new Pratyaksh.Core.Rectangle(minX, minY, width, height);
    }

    private static Vector2 WorldToMinimap(Vector2 worldPos, Pratyaksh.Core.Rectangle bounds, float scale, float offsetX, float offsetY)
    {
        return new Vector2(
            offsetX + (worldPos.X - bounds.X) * scale,
            offsetY + (worldPos.Y - bounds.Y) * scale
        );
    }

    private Vector2 MinimapToWorld(Vector2 screenPos)
    {
        Pratyaksh.Core.Rectangle bounds = activeWorldBounds;
        Pratyaksh.Core.Rectangle mapContentRect = new(UITransform.X + 3, UITransform.Y + 3, UITransform.Width - 6, UITransform.Height - 6);

        float scale = Math.Min(mapContentRect.Width / bounds.Width, mapContentRect.Height / bounds.Height);
        float offsetX = mapContentRect.X + (mapContentRect.Width - bounds.Width * scale) * 0.5f;
        float offsetY = mapContentRect.Y + (mapContentRect.Height - bounds.Height * scale) * 0.5f;

        float clampedScreenX = Math.Clamp(screenPos.X, mapContentRect.X, mapContentRect.X + mapContentRect.Width);
        float clampedScreenY = Math.Clamp(screenPos.Y, mapContentRect.Y, mapContentRect.Y + mapContentRect.Height);

        return new Vector2(
            bounds.X + (clampedScreenX - offsetX) / scale,
            bounds.Y + (clampedScreenY - offsetY) / scale
        );
    }

    private void PanCameraToMinimapPos(Vector2 screenPos)
    {
        Vector2 targetWorldPos = MinimapToWorld(screenPos);
        camera.CenterOnWorldPosition(targetWorldPos);
    }

    private void PanCameraWithOffset(Vector2 screenPos)
    {
        Vector2 mouseWorldPos = MinimapToWorld(screenPos);
        camera.CenterOnWorldPosition(mouseWorldPos + dragWorldOffset);
    }

    public override bool OnMouseDown(PointerInteractEventData evt)
    {
        if (evt.MouseButton == MouseButton.Right)
            return true;

        return base.OnMouseDown(evt);
    }

    public override bool OnDragStart(PointerInteractEventData evt)
    {
        if (evt.MouseButton == MouseButton.Left)
        {
            return base.OnDragStart(evt);
        }

        if (evt.MouseButton == MouseButton.Right)
        {
            isRightDragging = true;
            isLerping = false;
            activeWorldBounds = CalculateWorldBounds();
            visualWorldBounds = activeWorldBounds;
            Engine.Instance.InteractionManager.CapturePointer(this);

            GetCameraWorldExtents(out float camMinX, out float camMinY, out float camMaxX, out float camMaxY);
            Vector2 camWorldCenter = new((camMinX + camMaxX) * 0.5f, (camMinY + camMaxY) * 0.5f);
            Vector2 mouseWorldAtStart = MinimapToWorld(evt.ScreenPosition);
            dragWorldOffset = camWorldCenter - mouseWorldAtStart;

            PanCameraWithOffset(evt.ScreenPosition);
            return true;
        }

        return false;
    }

    public override void OnDrag(PointerInteractEventData evt)
    {
        if (isRightDragging)
        {
            PanCameraWithOffset(evt.ScreenPosition);
            return;
        }

        base.OnDrag(evt);
    }

    public override bool OnMouseUp(PointerInteractEventData evt)
    {
        if (evt.MouseButton == MouseButton.Left)
        {
            return base.OnMouseUp(evt);
        }

        if (evt.MouseButton == MouseButton.Right)
        {
            if (isRightDragging)
            {
                isRightDragging = false;
                dragWorldOffset = Vector2.Zero;
                Engine.Instance.InteractionManager.ReleasePointer();

                // Actual position snaps immediately
                activeWorldBounds = CalculateWorldBounds();

                // Visual position takes 0.5 seconds to lerp to it
                lerpStartBounds = visualWorldBounds;
                lerpTimer = 0f;
                isLerping = true;
            }
            else
            {
                // Single right-click inside the minimap jumps camera
                PanCameraToMinimapPos(evt.ScreenPosition);

                // Actual position snaps immediately
                activeWorldBounds = CalculateWorldBounds();

                // Visual position takes 0.5 seconds to lerp to it
                lerpStartBounds = visualWorldBounds;
                lerpTimer = 0f;
                isLerping = true;
            }

            return true; // Prevents Canvas context menu from opening
        }

        return false;
    }
}
