using Raylib_cs;
using RaylibNodeLibrary.DataModel;

namespace RaylibNodeLibrary.UI;

public class SearchMenu : UILayoutBase
{
    private List<(string name, object payload)> items;
    private Action<object> onItemSelected;
    private string searchQuery = "";

    private int searchInputId;
    private int scrollId;

    private bool firstFrame = true;

    public SearchMenu(int posX, int posY, int width, int height, List<(string name, object payload)> items, Action<object> onItemSelected, Drawable? parent = null, ParentBasis? parentBasis = null) : base(posX, posY, width, height, parent, parentBasis)
    {
        this.items = items;
        this.onItemSelected = onItemSelected;

        searchInputId = IdGen.GetNewID();
        scrollId = IdGen.GetNewID();
    }

    public override void OnDrawLayout()
    {
        layout.SectionEx("", Width, Height, new Color(), new Color((byte)45, (byte)45, (byte)45, (byte)255), new Color(), 0.0f, false);

        // Search Input Field
        InputField inputField = layout.InputField(searchInputId, "Search...", searchQuery, Width, 30, (field) =>
        {
            searchQuery = field.InputFieldText;
        });

        if (firstFrame)
        {
            inputField.SetFocus();
            firstFrame = false;
        }

        layout.BeginHorizontal(0);
        {
            layout.AddSpace(10);
            layout.BeginVertical(5);
            {
                layout.BeginScrollView(scrollId, Width - 10, Height - 50, 10, 0);
                {
                    string lowerQuery = searchQuery.ToLower();

                    for (int i = 0; i < items.Count; i++)
                    {
                        var (name, payload) = items[i];
                        if (string.IsNullOrEmpty(lowerQuery) || name.Contains(lowerQuery, StringComparison.CurrentCultureIgnoreCase))
                        {
                            layout.BeginHorizontal(0);
                            {
                                layout.Selectable(scrollId + 1 + i, false, name, Width - 30, 24, (sel) =>
                                {
                                    onItemSelected?.Invoke(payload);
                                }, null);
                            }
                            layout.EndHorizontal(24);
                        }
                    }
                }
                layout.EndScrollView();
            }
            layout.EndVertical(Width - 20);
        }
        layout.EndHorizontal(Height);
    }
}
