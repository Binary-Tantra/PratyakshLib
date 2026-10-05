using System.Numerics;

namespace Pratyaksh.Core;

public class UITransform : Transform
{
    private Vector2 size;
    private ParentBasis parentBasis;
    private UIBase? uibParent;

    public event Action? OnResize;

    public Vector2 Size
    {
        get => size;
        set
        {
            if (size != value)
            {
                size = value;
                SetBasisInfo(parentBasis); // Refresh basis after chaning size.

                OnResize?.Invoke();
            }
        }
    }

    public int Width { get => (int)Size.X; }
    public int Height { get => (int)Size.Y; }

    internal UITransform(int relativePosX, int relativePosY, int width, int height, ParentBasis? parentBasis, PratyakshObject owner) : base(owner)
    {
        Size = new Vector2(width, height);

        this.parentBasis = parentBasis ?? ParentBasis.TopLeft;
        SetParent(parent, false);

        RelativePosition = new Vector3(relativePosX, relativePosY, 0);
    }

    public override Vector3 Position
    {
        get
        {
            if (parent == null)
                return RelativePosition;

            Vector3 parentBasisPos = CalcParentBasisPos(parentBasis, uibParent, parent);
            return parentBasisPos + RelativePosition;
        }
        set
        {
            Vector3 required = value;

            if (parent == null)
                RelativePosition = required;
            else
            {
                Vector3 diff = required - CalcParentBasisPos(parentBasis, uibParent, parent);
                RelativePosition = diff;
            }
        }
    }

    public override float X
    {
        get
        {
            if (parent == null)
                return RelativePosition.X;

            float parentBasisPos = CalcParentBasisPos(parentBasis, uibParent, parent).X;
            return parentBasisPos + RelativePosition.X;
        }
        set
        {
            float required = value;

            if (parent == null)
                RelativePosition = new Vector3(required, RelativePosition.Y, RelativePosition.Z);
            else
            {
                float diff = required - CalcParentBasisPos(parentBasis, uibParent, parent).X;
                RelativePosition = new Vector3(diff, RelativePosition.Y, RelativePosition.Z);
            }
        }
    }

    public override float Y
    {
        get
        {
            if (parent == null)
                return RelativePosition.Y;

            float parentBasisPos = CalcParentBasisPos(parentBasis, uibParent, parent).Y;
            return parentBasisPos + RelativePosition.Y;
        }
        set
        {
            float required = value;

            if (parent == null)
                RelativePosition = new Vector3(RelativePosition.X, required, RelativePosition.Z);
            else
            {
                float diff = required - CalcParentBasisPos(parentBasis, uibParent, parent).Y;
                RelativePosition = new Vector3(RelativePosition.X, diff, RelativePosition.Z); ;
            }
        }
    }

    public override float Z
    {
        get
        {
            if (parent == null)
                return RelativePosition.Z;

            float parentBasisPos = CalcParentBasisPos(parentBasis, uibParent, parent).Z;
            return parentBasisPos + RelativePosition.Z;
        }
        set
        {
            float required = value;

            if (parent == null)
                RelativePosition = new Vector3(RelativePosition.X, RelativePosition.Y, required);
            else
            {
                float diff = required - CalcParentBasisPos(parentBasis, uibParent, parent).Z;
                RelativePosition = new Vector3(RelativePosition.X, RelativePosition.Y, diff);
            }
        }
    }

    private Vector3 CalcParentBasisPos(ParentBasis parentBasis, UIBase? uibParent, Transform parent)
    {
        if (uibParent == null)
        {
            return parent.Position;
        }

        return parentBasis switch
        {
            ParentBasis.TopLeft => uibParent.Transform.Position,
            ParentBasis.TopCenter => uibParent.Transform.Position + new Vector3(uibParent.UITransform.Size.X / 2 - Size.X / 2, 0, 0),
            ParentBasis.TopRight => uibParent.Transform.Position + new Vector3(uibParent.UITransform.Size.X - Size.X, 0, 0),
            ParentBasis.Left => uibParent.Transform.Position + new Vector3(0, uibParent.UITransform.Size.Y / 2 - Size.Y / 2, 0),
            ParentBasis.Center => uibParent.Transform.Position + new Vector3(uibParent.UITransform.Size.X / 2 - Size.X / 2, uibParent.UITransform.Size.Y / 2 - Size.Y / 2, 0),
            ParentBasis.Right => uibParent.Transform.Position + new Vector3(uibParent.UITransform.Size.X - Size.X, uibParent.UITransform.Size.Y / 2 - Size.Y / 2, 0),
            ParentBasis.BottomLeft => uibParent.Transform.Position + new Vector3(0, uibParent.UITransform.Size.Y - Size.Y, 0),
            ParentBasis.BottomCenter => uibParent.Transform.Position + new Vector3(uibParent.UITransform.Size.X / 2 - Size.X / 2, uibParent.UITransform.Size.Y - Size.Y, 0),
            ParentBasis.BottomRight => uibParent.Transform.Position + new Vector3(uibParent.UITransform.Size.X - Size.X, uibParent.UITransform.Size.Y - Size.Y, 0),
            
            _ => throw new ArgumentOutOfRangeException(nameof(parentBasis), parentBasis, "Invalid parent basis in CalcParentBasisPos!"),
        };
    }

    public void SetBasisInfo(ParentBasis parentBasis)
    {
        this.parentBasis = parentBasis;
    }

    protected override void OnSetParent(Transform? newParent, Transform? oldParent, bool preservePosition)
    {
        Vector3 currAbsPos = Position;

        if (newParent?.Owner is UIBase uib) uibParent = uib;
        else uibParent = null;

        parent = newParent;

        if (preservePosition) Position = currAbsPos;    // Recalculate relative position based on new parent
        else RelativePosition = Vector3.Zero;           // Reset relative position if not preserving
    }
}
