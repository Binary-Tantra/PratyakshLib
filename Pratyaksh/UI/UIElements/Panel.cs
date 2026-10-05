using System.Numerics;
using Pratyaksh.Core;

namespace Pratyaksh.UI.UIElements;

public class Panel : UIBase, IClippable
{
    public Panel(int width, int height, Transform? parent = null, ParentBasis? parentBasis = null) 
        : base(0, 0, width, height, parentBasis, parent)
    {
    }

    public Rectangle GetScissorRect(IWorldToScreenTransformer2D transformer)
    {
        Rectangle rect = new(UITransform.X, UITransform.Y, UITransform.Size.X, UITransform.Size.Y);
        bool worldSpace = InteractionUseWorldPos() || CheckAncestorsForInteractWorldPos();

        if (worldSpace)
            rect = transformer.WorldToScreen(rect);

        return rect;
    }

    protected override void OnDraw()
    {
    }
}
