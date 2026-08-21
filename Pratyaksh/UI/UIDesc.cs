using Pratyaksh.Core;
using Pratyaksh.Core.DataBinding;
using Pratyaksh.UI.UIElements;

namespace Pratyaksh.UI;

public enum UIElementType
{
    Text, Button, InputField, Selectable, Toggle, Dropdown, CycleSelector, LinkButton, StatusBadge, AlertBanner, Slider, Group,
    BindableToggle, BindableInputField_String, BindableInputField_Int, BindableInputField_Float, BindableSelectable, BindableDropdown, BindableSlider
}

public abstract class UIElementDescription
{
    private string _text;
    public virtual string Text { get => _text; set => _text = value; }

    protected UIElementDescription(string text)
    {
        _text = text;
    }

    public abstract UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft);
}

public class TextDesc : UIElementDescription
{
    private Raylib_cs.Color _color;
    public Raylib_cs.Color Color { get => _color; set => _color = value; }

    public TextDesc(string text, Raylib_cs.Color color) : base(text)
    {
        _color = color;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        throw new NotSupportedException("TextDesc does not construct a UIBase widget directly.");
    }
}

public class RectUIEDescription : UIElementDescription
{
    private int? _width;
    private int? _height;

    public int? Width { get => _width; set => _width = value; }
    public int? Height { get => _height; set => _height = value; }

    public RectUIEDescription(string text, int? width, int? height) : base(text)
    {
        _width = width;
        _height = height;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        throw new NotSupportedException("RectUIEDescription is a base class and cannot construct a UIBase directly.");
    }
}

public class ButtonDesc : RectUIEDescription
{
    private Action<Button>? _onClick;
    private int _fontSize;
    private bool _hasBorder;
    private Raylib_cs.Color? _fillColor;
    private Raylib_cs.Color? _borderColor;
    private Raylib_cs.Color? _textColor;

    public Action<Button>? OnClick { get => _onClick; set => _onClick = value; }
    public int FontSize { get => _fontSize; set => _fontSize = value; }
    public bool HasBorder { get => _hasBorder; set => _hasBorder = value; }
    public Raylib_cs.Color? FillColor { get => _fillColor; set => _fillColor = value; }
    public Raylib_cs.Color? BorderColor { get => _borderColor; set => _borderColor = value; }
    public Raylib_cs.Color? TextColor { get => _textColor; set => _textColor = value; }

    public ButtonDesc(string text, int? width, int? height, Action<Button>? onClick, int fontSize = 15, bool hasBorder = true, Raylib_cs.Color? fillColor = null, Raylib_cs.Color? borderColor = null, Raylib_cs.Color? textColor = null) : base(text, width, height)
    {
        _onClick = onClick;
        _fontSize = fontSize;
        _hasBorder = hasBorder;
        _fillColor = fillColor;
        _borderColor = borderColor;
        _textColor = textColor;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        return new Button(0, 0, Width ?? 150, Height ?? 25, Text, _onClick, null, _fontSize, _hasBorder, _fillColor, _borderColor, _textColor, parent, parentBasis);
    }
}

public class InputFieldDesc : RectUIEDescription
{
    private string _placeholderText;
    private bool _isMasked;
    private Action<InputField>? _onTextChanged;
    private Action<InputField>? _onFocusEnd;
    private BindableValueBase<string>? _stringDataModel;
    private BindableValueBase<int>? _intDataModel;
    private BindableValueBase<float>? _floatDataModel;

    public string PlaceholderText { get => _placeholderText; set => _placeholderText = value; }
    public bool IsMasked { get => _isMasked; set => _isMasked = value; }
    public Action<InputField>? OnTextChanged { get => _onTextChanged; set => _onTextChanged = value; }
    public Action<InputField>? OnFocusEnd { get => _onFocusEnd; set => _onFocusEnd = value; }
    public BindableValueBase<string>? StringDataModel => _stringDataModel;
    public BindableValueBase<int>? IntDataModel => _intDataModel;
    public BindableValueBase<float>? FloatDataModel => _floatDataModel;
    public bool IsBindable => _stringDataModel != null || _intDataModel != null || _floatDataModel != null;

    public override string Text
    {
        get
        {
            if (_stringDataModel != null) return _stringDataModel.Get();
            if (_intDataModel != null) return _intDataModel.Get().ToString();
            if (_floatDataModel != null) return _floatDataModel.Get().ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture);
            return base.Text;
        }
        set
        {
            if (_stringDataModel != null) _stringDataModel.Set(value, true);
            else if (_intDataModel != null)
            {
                if (int.TryParse(value, out int v)) _intDataModel.Set(v, true);
            }
            else if (_floatDataModel != null)
            {
                if (float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float v)) _floatDataModel.Set(v, true);
            }
            else
            {
                base.Text = value;
            }
        }
    }

    public InputFieldDesc(string placeholderText, string inputFieldText, int? width = null, int? height = null, bool isMasked = false, Action<InputField>? onTextChanged = null, Action<InputField>? onFocusEnd = null) : base(inputFieldText, width, height)
    {
        _placeholderText = placeholderText;
        _isMasked = isMasked;
        _onTextChanged = onTextChanged;
        _onFocusEnd = onFocusEnd;
    }

    public InputFieldDesc(string placeholderText, BindableValueBase<string> dataModel, int? width = null, int? height = null, bool isMasked = false) : base("", width, height)
    {
        _placeholderText = placeholderText;
        _stringDataModel = dataModel;
        _isMasked = isMasked;
    }

    public InputFieldDesc(string placeholderText, BindableValueBase<int> dataModel, int? width = null, int? height = null, bool isMasked = false) : base("", width, height)
    {
        _placeholderText = placeholderText;
        _intDataModel = dataModel;
        _isMasked = isMasked;
    }

    public InputFieldDesc(string placeholderText, BindableValueBase<float> dataModel, int? width = null, int? height = null, bool isMasked = false) : base("", width, height)
    {
        _placeholderText = placeholderText;
        _floatDataModel = dataModel;
        _isMasked = isMasked;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        return new InputField(_placeholderText, Text, 0, 0, Width ?? 150, Height ?? 25, _onTextChanged, _onFocusEnd, 15, _isMasked, parent, parentBasis);
    }
}

public class SelectableDesc : RectUIEDescription
{
    private bool _isSelected;
    private BindableValueBase<bool>? _dataModel;
    private Action<Selectable>? _onSelect;

    public bool IsSelected
    {
        get => _dataModel != null ? _dataModel.Get() : _isSelected;
        set
        {
            if (_dataModel != null) _dataModel.Set(value, true);
            else _isSelected = value;
        }
    }

    public bool StartingSelected { get => IsSelected; set => IsSelected = value; }
    public BindableValueBase<bool>? DataModel => _dataModel;
    public bool IsBindable => _dataModel != null;
    public Action<Selectable>? OnSelect { get => _onSelect; set => _onSelect = value; }

    public SelectableDesc(string text, bool startingSelected, int? width = null, int? height = null, Action<Selectable>? onSelect = null) : base(text, width, height)
    {
        _isSelected = startingSelected;
        _onSelect = onSelect;
    }

    public SelectableDesc(string text, BindableValueBase<bool> dataModel, int? width = null, int? height = null) : base(text, width, height)
    {
        _dataModel = dataModel;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        return new Selectable(Text, IsSelected, 0, 0, Width ?? 150, Height ?? 25, _onSelect, null, 15, null, null, null, parent, parentBasis);
    }
}

public class ToggleDesc : RectUIEDescription
{
    private bool _value;
    private BindableValueBase<bool>? _dataModel;
    private Action<Toggle>? _onToggle;

    public bool Value
    {
        get => _dataModel != null ? _dataModel.Get() : _value;
        set
        {
            if (_dataModel != null) _dataModel.Set(value, true);
            else _value = value;
        }
    }

    public bool StartingState { get => Value; set => Value = value; }
    public BindableValueBase<bool>? DataModel => _dataModel;
    public bool IsBindable => _dataModel != null;
    public Action<Toggle>? OnToggle { get => _onToggle; set => _onToggle = value; }

    public ToggleDesc(string text, bool startingState, int? width = null, int? height = null, Action<Toggle>? onToggle = null) : base(text, width, height)
    {
        _value = startingState;
        _onToggle = onToggle;
    }

    public ToggleDesc(string text, BindableValueBase<bool> dataModel, int? width = null, int? height = null) : base(text, width, height)
    {
        _dataModel = dataModel;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        return new Toggle(0, 0, Value, Width ?? 38, Height ?? 20, _onToggle, null, 15, parent, parentBasis);
    }
}

public class DropdownDesc : RectUIEDescription
{
    private string[] _options;
    private int _selectedIndex;
    private BindableValueBase<int>? _dataModel;
    private Action<Dropdown>? _onSelectionChanged;

    public string[] Options { get => _options; set => _options = value; }

    public int SelectedIndex
    {
        get => _dataModel != null ? _dataModel.Get() : _selectedIndex;
        set
        {
            if (_dataModel != null) _dataModel.Set(value, true);
            else _selectedIndex = value;
        }
    }

    public BindableValueBase<int>? DataModel => _dataModel;
    public bool IsBindable => _dataModel != null;
    public Action<Dropdown>? OnSelectionChanged { get => _onSelectionChanged; set => _onSelectionChanged = value; }

    public DropdownDesc(string[] options, int selectedIndex, int? width = null, int? height = null, Action<Dropdown>? onSelectionChanged = null) : base("", width, height)
    {
        _options = options;
        _selectedIndex = selectedIndex;
        _onSelectionChanged = onSelectionChanged;
    }

    public DropdownDesc(string[] options, BindableValueBase<int> dataModel, int? width = null, int? height = null) : base("", width, height)
    {
        _options = options;
        _dataModel = dataModel;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        return new Dropdown(_options, SelectedIndex, 0, 0, Width ?? 150, Height ?? 25, _onSelectionChanged, null, 15, parent, parentBasis);
    }
}

public class CycleSelectorDesc : RectUIEDescription
{
    private string[] _options;
    private int _selectedIndex;
    private Action<CycleSelector>? _onSelectionChanged;

    public string[] Options { get => _options; set => _options = value; }
    public int SelectedIndex { get => _selectedIndex; set => _selectedIndex = value; }
    public Action<CycleSelector>? OnSelectionChanged { get => _onSelectionChanged; set => _onSelectionChanged = value; }

    public CycleSelectorDesc(string[] options, int selectedIndex, int? width = null, int? height = null, Action<CycleSelector>? onSelectionChanged = null) : base("", width, height)
    {
        _options = options;
        _selectedIndex = selectedIndex;
        _onSelectionChanged = onSelectionChanged;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        return new CycleSelector(_options, _selectedIndex, 0, 0, Width ?? 150, Height ?? 25, _onSelectionChanged, null, 15, parent, parentBasis);
    }
}

public class LinkButtonDesc : RectUIEDescription
{
    private string _url;
    private Action<LinkButton>? _onClick;

    public string Url { get => _url; set => _url = value; }
    public Action<LinkButton>? OnClick { get => _onClick; set => _onClick = value; }

    public LinkButtonDesc(string text, string url, Action<LinkButton>? onClick = null, int? width = null, int? height = null) : base(text, width, height)
    {
        _url = url;
        _onClick = onClick;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        return new LinkButton(0, 0, Text, _url, _onClick, 14, parent, parentBasis);
    }
}

public class StatusBadgeDesc : RectUIEDescription
{
    private StatusType _statusType;
    private Raylib_cs.Color? _customColor;

    public StatusType StatusType { get => _statusType; set => _statusType = value; }
    public Raylib_cs.Color? CustomColor { get => _customColor; set => _customColor = value; }

    public StatusBadgeDesc(string text, StatusType statusType = StatusType.Idle, Raylib_cs.Color? customColor = null, int? width = null, int? height = null) : base(text, width, height)
    {
        _statusType = statusType;
        _customColor = customColor;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        return new StatusBadge(0, 0, Text, _statusType, _customColor, 13, parent, parentBasis);
    }
}

public class AlertBannerDesc : RectUIEDescription
{
    private AlertType _alertType;
    private bool _isDismissible;

    public AlertType AlertType { get => _alertType; set => _alertType = value; }
    public bool IsDismissible { get => _isDismissible; set => _isDismissible = value; }

    public AlertBannerDesc(string message, AlertType alertType = AlertType.Error, bool isDismissible = true, int? width = null, int? height = null) : base(message, width, height)
    {
        _alertType = alertType;
        _isDismissible = isDismissible;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        return new AlertBanner(0, 0, Text, _alertType, Width ?? 360, Height ?? 32, _isDismissible, 13, parent, parentBasis);
    }
}

public class SliderDesc : RectUIEDescription
{
    private float _value;
    private BindableValueBase<float>? _dataModel;
    private float _minValue;
    private float _maxValue;
    private float? _step;
    private bool _showValue;
    private string? _format;
    private Action<Slider>? _onValueChanged;

    public float Value
    {
        get => _dataModel != null ? _dataModel.Get() : _value;
        set
        {
            if (_dataModel != null) _dataModel.Set(value, true);
            else _value = value;
        }
    }

    public BindableValueBase<float>? DataModel => _dataModel;
    public bool IsBindable => _dataModel != null;
    public float MinValue { get => _minValue; set => _minValue = value; }
    public float MaxValue { get => _maxValue; set => _maxValue = value; }
    public float? Step { get => _step; set => _step = value; }
    public bool ShowValue { get => _showValue; set => _showValue = value; }
    public string? Format { get => _format; set => _format = value; }
    public Action<Slider>? OnValueChanged { get => _onValueChanged; set => _onValueChanged = value; }

    public SliderDesc(string label, float value, float minValue, float maxValue, int? width = null, int? height = null, Action<Slider>? onValueChanged = null, bool showValue = true, string? format = null, float? step = null) : base(label, width, height)
    {
        _value = value;
        _minValue = minValue;
        _maxValue = maxValue;
        _step = step;
        _showValue = showValue;
        _format = format;
        _onValueChanged = onValueChanged;
    }

    public SliderDesc(string label, BindableValueBase<float> dataModel, float minValue = 0f, float maxValue = 1f, int? width = null, int? height = null, bool showValue = true, string? format = null, float? step = null) : base(label, width, height)
    {
        _dataModel = dataModel;
        _minValue = minValue;
        _maxValue = maxValue;
        _step = step;
        _showValue = showValue;
        _format = format;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        return new Slider(0, 0, Value, _minValue, _maxValue, Width ?? 150, Height ?? 20, _onValueChanged, null, _showValue, _format, 13, _step, parent, parentBasis);
    }
}

public class HorizontalGroupDesc : RectUIEDescription
{
    private int _spacing;
    private List<(UIElementType elemType, UIElementDescription elemDesc)> _uiElements;

    public int Spacing { get => _spacing; set => _spacing = value; }
    public List<(UIElementType elemType, UIElementDescription elemDesc)> UIElements { get => _uiElements; set => _uiElements = value; }

    public HorizontalGroupDesc(string layoutName, int spacing, List<(UIElementType, UIElementDescription)> uiElements, int? width = null, int? height = null) : base(layoutName, width, height)
    {
        _spacing = spacing;
        _uiElements = uiElements;
    }

    public override UIBase Construct(EditorObject? parent = null, ParentBasis parentBasis = ParentBasis.TopLeft)
    {
        throw new NotSupportedException("HorizontalGroupDesc is a layout container and does not construct a single UIBase.");
    }
}