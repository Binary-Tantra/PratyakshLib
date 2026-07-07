using Raylib_cs;

namespace RaylibNodeLibrary.UI;

public class InspectorPanel : UILayoutBase
{
    public InspectorPanel(int posX, int posY, Drawable? parent = null) : base(posX, posY, 200, 300, parent)
    {

    }

    public override void OnDrawLayout()
    {
        layout.SectionEx("Inspector", layoutWidth, layoutHeight - 50, Raylib.Fade(Color.DarkGray, 0.65f), Raylib.Fade(Color.Gray, 0.65f), Raylib.Fade(Color.White, 0.7f), 0.1f, false);
    }
}
