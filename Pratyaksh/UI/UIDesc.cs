using Pratyaksh.Core.DataBinding;
using Pratyaksh.UI.UIElements;
using System.Numerics;

namespace Pratyaksh.UI;

public enum UIElementType
{
    Text, Label, Button, InputField, Selectable, Toggle, Dropdown, CycleSelector, LinkButton, StatusBadge, AlertBanner, Slider, Group,
    BindableLabel, BindableToggle, BindableInputField_String, BindableInputField_Int, BindableInputField_Float, BindableSelectable, BindableDropdown, BindableSlider
}

public abstract class UIElementDescription
{
    public string text;

    protected UIElementDescription(string text)
    {
        this.text = text;
    }
}

public class TextDesc : UIElementDescription
{
    public int fontSize;
    public Raylib_cs.Color color;

    public TextDesc(string text, int fontSize = 15, Raylib_cs.Color? color = null) : base(text)
    {
        this.fontSize = fontSize;
        this.color = color ?? Raylib_cs.Color.DarkGray;
    }
}

public class RectUIEDescription : UIElementDescription
{
    public int? width;
    public int? height;

    public RectUIEDescription(string text, int? width, int? height) : base(text)
    {
        this.width = width;
        this.height = height;
    }
}

public class LabelDesc : RectUIEDescription
{
    public int fontSize = 15;
    public Raylib_cs.Color? textColor;

    public LabelDesc(string text, int fontSize = 15, Raylib_cs.Color? textColor = null) : base(text, 0, 0)
    {
        Vector2 tSize = LayoutEngine.MeasureText(text, fontSize);
        width = (int)tSize.X;
        height = (int)tSize.Y;

        this.fontSize = fontSize;
        this.textColor = textColor;
    }
}

public class ButtonDesc : RectUIEDescription
{
    public Action<Button> onClick;

    public int fontSize;
    public bool hasBorder;

    public Raylib_cs.Color? fillColor;
    public Raylib_cs.Color? borderColor;
    public Raylib_cs.Color? textColor;

    public ButtonDesc(string text, int? width, int? height, Action<Button> onClick, int fontSize = 15, bool hasBorder = true, Raylib_cs.Color? fillColor = null, Raylib_cs.Color? borderColor = null, Raylib_cs.Color? textColor = null) : base(text, width, height)
    {
        this.onClick = onClick;

        this.fontSize = fontSize;
        this.hasBorder = hasBorder;
        
        this.fillColor = fillColor;
        this.borderColor = borderColor;
        this.textColor = textColor;
    }
}

public class InputFieldDesc : RectUIEDescription
{
    public string placeholderText;
    public bool isMasked;
    internal Action<InputField>? onTextChanged;
    internal Action<InputField>? onFocusEnd;

    public InputFieldDesc(string placeholderText, string inputFieldText, int? width, int? height, bool isMasked = false) : base(inputFieldText, width, height)
    {
        this.placeholderText = placeholderText;
        this.isMasked = isMasked;
    }
}

public class SelectableDesc : RectUIEDescription
{
    public bool startingSelected;
    public Action<Selectable> onSelect;

    public SelectableDesc(string text, bool startingSelected, int? width, int? height, Action<Selectable> onSelect) : base(text, width, height)
    {
        this.startingSelected = startingSelected;
        this.onSelect = onSelect;
    }
}

public class ToggleDesc : RectUIEDescription
{
    public bool startingState;
    public Action<Toggle> onToggle;

    public ToggleDesc(string text, bool startingState, int? width, int? height, Action<Toggle> onToggle) : base(text, width, height)
    {
        this.startingState = startingState;
        this.onToggle = onToggle;
    }
}

public class DropdownDesc : RectUIEDescription
{
    public string[] options;
    public int selectedIndex;
    public Action<Dropdown> onSelectionChanged;

    public DropdownDesc(string[] options, int selectedIndex, int? width, int? height, Action<Dropdown> onSelectionChanged) : base("", width, height)
    {
        this.options = options;
        this.selectedIndex = selectedIndex;
        this.onSelectionChanged = onSelectionChanged;
    }
}

public class CycleSelectorDesc : RectUIEDescription
{
    public string[] options;
    public int selectedIndex;
    public Action<CycleSelector> onSelectionChanged;

    public CycleSelectorDesc(string[] options, int selectedIndex, int? width, int? height, Action<CycleSelector> onSelectionChanged) : base("", width, height)
    {
        this.options = options;
        this.selectedIndex = selectedIndex;
        this.onSelectionChanged = onSelectionChanged;
    }
}

public class LinkButtonDesc : RectUIEDescription
{
    public string url;
    public Action<LinkButton>? onClick;

    public LinkButtonDesc(string text, string url, Action<LinkButton>? onClick = null, int? width = null, int? height = null) : base(text, width, height)
    {
        this.url = url;
        this.onClick = onClick;
    }
}

public class StatusBadgeDesc : RectUIEDescription
{
    public StatusType statusType;
    public Raylib_cs.Color? customColor;

    public StatusBadgeDesc(string text, StatusType statusType = StatusType.Idle, Raylib_cs.Color? customColor = null, int? width = null, int? height = null) : base(text, width, height)
    {
        this.statusType = statusType;
        this.customColor = customColor;
    }
}

public class AlertBannerDesc : RectUIEDescription
{
    public AlertType alertType;
    public bool isDismissible;

    public AlertBannerDesc(string message, AlertType alertType = AlertType.Error, bool isDismissible = true, int? width = null, int? height = null) : base(message, width, height)
    {
        this.alertType = alertType;
        this.isDismissible = isDismissible;
    }
}

public class SliderDesc : RectUIEDescription
{
    public float value;
    public float minValue;
    public float maxValue;
    public float? step;
    public bool showValue;
    public string? format;
    public Action<Slider>? onValueChanged;

    public SliderDesc(string label, float value, float minValue, float maxValue, int? width = null, int? height = null, Action<Slider>? onValueChanged = null, bool showValue = true, string? format = null, float? step = null) : base(label, width, height)
    {
        this.value = value;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.step = step;
        this.showValue = showValue;
        this.format = format;
        this.onValueChanged = onValueChanged;
    }
}

public class HorizontalGroupDesc : RectUIEDescription
{
    public int spacing;
    public List<(UIElementType elemType, UIElementDescription elemDesc)> uiElements;

    public HorizontalGroupDesc(string layoutName, int spacing, List<(UIElementType, UIElementDescription)> uiElements, int? width, int? height) : base(layoutName, width, height)
    {
        this.spacing = spacing;
        this.uiElements = uiElements;
    }
}

public class BindableLabelDesc : RectUIEDescription
{
    public BindableValueBase<string> label;
    public int fontSize = 15;
    public Raylib_cs.Color? textColor;

    public BindableLabelDesc(BindableValueBase<string> dataModel, int fontSize = 15, Raylib_cs.Color? textColor = null) : base(dataModel.Get(), 0, 0)
    {
        label = dataModel;

        Vector2 tSize = LayoutEngine.MeasureText(label.Get(), fontSize);
        width = (int)tSize.X;
        height = (int)tSize.Y;

        this.fontSize = fontSize;
        this.textColor = textColor;
    }
}

public class BindableToggleDesc : RectUIEDescription
{
    public BindableValueBase<bool> dataModel;

    public BindableToggleDesc(string label, BindableValueBase<bool> dataModel, int? width = null, int? height = null) : base(label, width, height)
    {
        this.dataModel = dataModel;
    }
}

public class BindableInputFieldStringDesc : RectUIEDescription
{
    public BindableValueBase<string> dataModel;
    public string placeholderText;

    public BindableInputFieldStringDesc(string placeholderText, BindableValueBase<string> dataModel, int? width = null, int? height = null) : base("", width, height)
    {
        this.placeholderText = placeholderText;
        this.dataModel = dataModel;
    }
}

public class BindableInputFieldIntDesc : RectUIEDescription
{
    public BindableValueBase<int> dataModel;
    public string placeholderText;

    public BindableInputFieldIntDesc(string placeholderText, BindableValueBase<int> dataModel, int? width = null, int? height = null) : base("", width, height)
    {
        this.placeholderText = placeholderText;
        this.dataModel = dataModel;
    }
}

public class BindableInputFieldFloatDesc : RectUIEDescription
{
    public BindableValueBase<float> dataModel;
    public string placeholderText;

    public BindableInputFieldFloatDesc(string placeholderText, BindableValueBase<float> dataModel, int? width = null, int? height = null) : base("", width, height)
    {
        this.placeholderText = placeholderText;
        this.dataModel = dataModel;
    }
}

public class BindableSelectableDesc : RectUIEDescription
{
    public BindableValueBase<bool> dataModel;

    public BindableSelectableDesc(string text, BindableValueBase<bool> dataModel, int? width = null, int? height = null) : base(text, width, height)
    {
        this.dataModel = dataModel;
    }
}

public class BindableDropdownDesc : RectUIEDescription
{
    public BindableValueBase<int> dataModel;
    public string[] options;

    public BindableDropdownDesc(string[] options, BindableValueBase<int> dataModel, int? width = null, int? height = null) : base("", width, height)
    {
        this.options = options;
        this.dataModel = dataModel;
    }
}

public class BindableSliderDesc : RectUIEDescription
{
    public BindableValueBase<float> dataModel;
    public float minValue;
    public float maxValue;
    public float? step;
    public bool showValue;
    public string? format;

    public BindableSliderDesc(string label, BindableValueBase<float> dataModel, float minValue = 0f, float maxValue = 1f, int? width = null, int? height = null, bool showValue = true, string? format = null, float? step = null) : base(label, width, height)
    {
        this.dataModel = dataModel;
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.step = step;
        this.showValue = showValue;
        this.format = format;
    }
}