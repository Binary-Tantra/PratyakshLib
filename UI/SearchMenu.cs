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

    public SearchMenu(int posX, int posY, int width, int height, List<(string name, object payload)> items, Action<object> onItemSelected, Drawable? parent = null) 
        : base(posX, posY, width, height, parent)
    {
        this.items = items;
        this.onItemSelected = onItemSelected;

        searchInputId = IdGen.GetNewID();
        scrollId = IdGen.GetNewID();
    }

    public override void OnDrawLayout()
    {
        layout.SectionEx("", layoutWidth, layoutHeight, new Color(), new Color((byte)45, (byte)45, (byte)45, (byte)255), new Color(), 0.0f, false);

        // Search Input Field
        layout.InputField(searchInputId, "Search...", searchQuery, layoutWidth, 30, (inputField) =>
        {
            searchQuery = inputField.InputFieldText;
        });

        if (firstFrame)
        {
            InputField? inputField = layout.GetInputField(searchInputId);
            if (inputField != null)
            {
                inputField.SetFocus();
                firstFrame = false;
            }
        }

        layout.BeginHorizontal(0);
        {
            layout.AddSpace(10);
            layout.BeginVertical(5);
            {
                layout.BeginScrollView(scrollId, layoutWidth - 10, layoutHeight - 50, 10, 0);
                {
                    string lowerQuery = searchQuery.ToLower();

                    for (int i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        if (string.IsNullOrEmpty(lowerQuery) || item.name.ToLower().Contains(lowerQuery))
                        {
                            layout.BeginHorizontal(0);
                            {
                                layout.Selectable(scrollId + 1 + i, item.name, layoutWidth - 30, 24, (sel) =>
                                {
                                    onItemSelected?.Invoke(item.payload);
                                }, null);
                            }
                            layout.EndHorizontal(24);
                        }
                    }
                }
                layout.EndScrollView();
            }
            layout.EndVertical(layoutWidth - 20);
        }
        layout.EndHorizontal(layoutHeight);
    }
}
