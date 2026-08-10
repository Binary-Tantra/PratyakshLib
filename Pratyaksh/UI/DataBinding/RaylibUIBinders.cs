using Pratyaksh.Core.DataBinding;
using Pratyaksh.UI.UIElements;

namespace Pratyaksh.UI.DataBinding;

public class RLToggleUI : BindableUIBase<bool>
{
    private Toggle toggle;

    public RLToggleUI(Toggle toggle)
    {
        this.toggle = toggle;
    }

    private void RelayChange(Toggle tog)
    {
        NotifyBoundValOfChange(tog.IsOn);
    }

    public override bool Get() => toggle.IsOn;

    protected override void OnSet(bool newVal)
    {
        toggle.SetIsOnWithoutNotify(newVal);
    }

    protected override bool GetDefault() => false;

    public override void NotifyBind()
    {
        toggle.SetOnToggleChanged(RelayChange);
    }

    public override void NotifyUnbind()
    {
        toggle.SetOnToggleChanged(null);
    }
}

public class RLInputFieldUI_String : BindableUIBase<string>
{
    private InputField inputField;

    public RLInputFieldUI_String(InputField inputField)
    {
        this.inputField = inputField;
    }

    private void RelayChange(InputField field)
    {
        NotifyBoundValOfChange(field.InputFieldText);
    }

    public override string Get() => inputField.InputFieldText;

    protected override void OnSet(string newVal)
    {
        inputField.SetTextWithoutNotify(newVal ?? "");
    }

    protected override string GetDefault() => "";

    public override void NotifyBind()
    {
        inputField.OnTextChanged = RelayChange;
    }

    public override void NotifyUnbind()
    {
        inputField.OnTextChanged = null;
    }
}

public class RLInputFieldUI_Int : BindableUIBase<int>
{
    private InputField inputField;

    public RLInputFieldUI_Int(InputField inputField)
    {
        this.inputField = inputField;
    }

    private void RelayChange(InputField field)
    {
        if (int.TryParse(field.InputFieldText, out int result))
        {
            NotifyBoundValOfChange(result);
        }
    }

    public override int Get() => int.TryParse(inputField.InputFieldText, out int res) ? res : 0;

    protected override void OnSet(int newVal)
    {
        inputField.SetTextWithoutNotify(newVal.ToString());
    }

    protected override int GetDefault() => 0;

    public override void NotifyBind()
    {
        inputField.OnTextChanged = RelayChange;
    }

    public override void NotifyUnbind()
    {
        inputField.OnTextChanged = null;
    }
}

public class RLInputFieldUI_Float : BindableUIBase<float>
{
    private InputField inputField;

    public RLInputFieldUI_Float(InputField inputField)
    {
        this.inputField = inputField;
    }

    private void RelayChange(InputField field)
    {
        if (float.TryParse(field.InputFieldText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
        {
            NotifyBoundValOfChange(result);
        }
    }

    public override float Get() => float.TryParse(inputField.InputFieldText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float res) ? res : 0f;

    protected override void OnSet(float newVal)
    {
        inputField.SetTextWithoutNotify(newVal.ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture));
    }

    protected override float GetDefault() => 0f;

    public override void NotifyBind()
    {
        inputField.OnTextChanged = RelayChange;
    }

    public override void NotifyUnbind()
    {
        inputField.OnTextChanged = null;
    }
}

public class RLSelectableUI : BindableUIBase<bool>
{
    private Selectable selectable;

    public RLSelectableUI(Selectable selectable)
    {
        this.selectable = selectable;
    }

    private void RelayChange(Selectable sel)
    {
        NotifyBoundValOfChange(sel.IsSelected);
    }

    public override bool Get() => selectable.IsSelected;

    protected override void OnSet(bool newVal)
    {
        selectable.SetIsSelectedWithoutNotify(newVal);
    }

    protected override bool GetDefault() => false;

    public override void NotifyBind()
    {
        selectable.SetOnSelect(RelayChange);
    }

    public override void NotifyUnbind()
    {
        selectable.SetOnSelect(null);
    }
}

public class RLDropdownUI : BindableUIBase<int>
{
    private Dropdown dropdown;

    public RLDropdownUI(Dropdown dropdown)
    {
        this.dropdown = dropdown;
    }

    private void RelayChange(Dropdown dd)
    {
        NotifyBoundValOfChange(dd.SelectedIndex);
    }

    public override int Get() => dropdown.SelectedIndex;

    protected override void OnSet(int newVal)
    {
        dropdown.SetSelectedIndexWithoutNotify(newVal);
    }

    protected override int GetDefault() => 0;

    public override void NotifyBind()
    {
        dropdown.SetOnSelectionChanged(RelayChange);
    }

    public override void NotifyUnbind()
    {
        dropdown.SetOnSelectionChanged(null);
    }
}

// Typed Binders
public class BoolToggleBinder : Binder<BindableValueBase<bool>, RLToggleUI, bool> { }
public class StringInputBinder : Binder<BindableValueBase<string>, RLInputFieldUI_String, string> { }
public class IntInputBinder : Binder<BindableValueBase<int>, RLInputFieldUI_Int, int> { }
public class FloatInputBinder : Binder<BindableValueBase<float>, RLInputFieldUI_Float, float> { }
public class BoolSelectableBinder : Binder<BindableValueBase<bool>, RLSelectableUI, bool> { }
public class IntDropdownBinder : Binder<BindableValueBase<int>, RLDropdownUI, int> { }
