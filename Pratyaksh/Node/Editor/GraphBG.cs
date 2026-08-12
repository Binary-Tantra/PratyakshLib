using Pratyaksh.Core;
using Raylib_cs;

namespace Pratyaksh.Node.Editor;

public class GraphBG : Actor
{
    private int graphWidth;
    private int graphHeight;

    private int graphLinesPer100Px;

    private int hMainLinesCount;
    private int vMainLinesCount;

    private (int X, int Y) graphOffset;

    public GraphBG(int graphWidth, int graphHeight, int graphLinesPer100Px = 1) : base(null)
    {
        treeInteractable = false;

        this.graphWidth = graphWidth;
        this.graphHeight = graphHeight;

        this.graphLinesPer100Px = graphLinesPer100Px;

        hMainLinesCount = (int)(graphHeight / (float)(100 * graphLinesPer100Px)); // Height used for horizontal lines...not width.
        vMainLinesCount = (int)(graphWidth / (float)(100 * graphLinesPer100Px));

        graphOffset.X = -((hMainLinesCount * 100) / 2);
        graphOffset.Y = -((vMainLinesCount * 100) / 2);
    }

    public void DrawGrid(int startX, int startY, int endX, int endY, int vLines, int hLines, Color gridColor)
    {
        int vSpace = (int)((endY - startY) / (float)hLines);
        int vLineSize = endY - startY;
        for (int i = 0; i < vLines; i++)
            Raylib.DrawLine(graphOffset.X + startX + i * vSpace, graphOffset.Y + startY, graphOffset.X + startX + i * vSpace, graphOffset.Y + endY, gridColor);

        int hSpace = (int)((endX - startX) / (float)vLines);
        int hLineSize = endX - startX;
        for (int i = 0; i < hLines; i++)
            Raylib.DrawLine(graphOffset.X + startX, graphOffset.Y + startY + i * hSpace, graphOffset.X + endX, graphOffset.Y + startY + i * hSpace, gridColor);
    }

    public void DrawGraph()
    {
        int segments = vMainLinesCount * hMainLinesCount;
        int segmentXSize = 100;// graphWidth / vMainLinesCount;
        int segmentYSize = 100;// graphHeight / hMainLinesCount;

        for (int i = 0; i < segments; i++)
        {
            int startX = (i % vMainLinesCount) * segmentXSize;
            int startY = (i / vMainLinesCount) * segmentYSize;
            DrawGrid(startX, startY, startX + segmentXSize, startY + segmentYSize, 10, 10, new Color((byte)100, (byte)100, (byte)100, (byte)40));
        }

        DrawGrid(0, 0, vMainLinesCount * 100, hMainLinesCount * 100, vMainLinesCount, hMainLinesCount, new Color((byte)0, (byte)0, (byte)0, (byte)150));
    }

    protected override void OnDraw()
    {
        DrawGraph();
    }
}
