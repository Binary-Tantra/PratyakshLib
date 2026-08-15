# Pratyaksh.Core

[![NuGet Version](https://img.shields.io/nuget/v/Pratyaksh.Core.svg)](https://www.nuget.org/packages/Pratyaksh.Core/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

**Pratyaksh.Core** is the foundational architecture layer for the Pratyaksh ecosystem. It provides low-level scene hierarchy management, spatial transformation abstractions, a robust input handling and gesture disambiguation engine, an MVVM reactive two-way data-binding framework, and extensible serialization contracts.

Designed to be lightweight and runtime-agnostic, `Pratyaksh.Core` has no dependencies on external graphics APIs or windowing frameworks.

---

## 📦 Key Features

- **Scene Graph & Hierarchy**: Object hierarchy (`Drawable`, `Actor`, `EditorObject`, `UIBase`) supporting nested relative/absolute positioning, anchor-based parent offsets (`ParentBasis`), bounding box math, and recursive hit-testing.
- **Input Pipeline & Gesture Disambiguation**: Advanced `InteractionManager` capable of:
  - Resolving ambiguous click vs. drag interactions using squared Euclidean distance thresholds.
  - Tracking double-clicks with configurable timing.
  - Validating scissor clipping regions (`IClippable`) so clipped child elements don't receive invalid pointer hits.
  - Bubbling and direct pointer/keyboard event dispatching.
  - Clean focus capture and loss-of-focus notifications.
- **MVVM Reactive Two-Way Data Binding**: Generic `BindableBase<T>`, `BindableValueBase<T>`, `BindableUIBase<T>`, and `Binder<TVal, TUI, TType>` architecture with loopback suppression.
- **Serialization Engine**: Clean serialization abstractions (`ISerializationEngine`, `BaseSerializer`) with built-in `JsonSerializationEngine` powered by `System.Text.Json`.

---

## 🚀 Installation

Install via the .NET CLI:

```bash
dotnet add package Pratyaksh.Core
```

Or via the NuGet Package Manager:

```powershell
Install-Package Pratyaksh.Core
```

---

## 🛠️ Usage Examples

### 1. Two-Way Reactive Data Binding

```csharp
using Pratyaksh.Core.DataBinding;

// 1. Define a reactive model value
var playerName = new BindableString("Hero");

// 2. Observe changes from the data model
playerName.onBoundUIChange += (newVal) =>
{
    Console.WriteLine($"Model value changed to: {newVal}");
};

// 3. Mutate the value (notifies all attached binders)
playerName.Set("Warrior", notifyBound: true);
```

### 2. Implementing a Custom Binder

```csharp
using Pratyaksh.Core.DataBinding;

// Custom UI adapter for a text component
public class CustomTextUI : BindableUIBase<string>
{
    private string displayText = string.Empty;

    public override string Get() => displayText;
    protected override string GetDefault() => string.Empty;

    protected override void OnSet(string newVal)
    {
        displayText = newVal;
        Console.WriteLine($"UI rendered updated text: {displayText}");
    }

    public void UserTypedText(string text)
    {
        displayText = text;
        NotifyBoundValOfChange(text); // Pushes UI edit back to Model
    }
}

// Binder binding model to UI
public class TextBinder : Binder<BindableValueBase<string>, CustomTextUI, string> { }

// Connect them
var nameModel = new BindableString("Initial Name");
var nameUI = new CustomTextUI();
var binder = new TextBinder();

binder.Bind(nameModel, nameUI); // Initial push from Model -> UI
nameUI.UserTypedText("Updated Name"); // UI change updates Model
```

### 3. Creating an Interactable Scene Object

```csharp
using System.Numerics;
using Pratyaksh.Core;

public class CustomBox : EditorObject, IPointerInteractable
{
    public Vector2 Size { get; set; } = new(100, 50);

    public override Rectangle InteractionRect => new(Position.X, Position.Y, Size.X, Size.Y);

    public CustomBox(Drawable? parent = null) : base(parent)
    {
        selfInteractable = true;
    }

    public override bool InteractionUseWorldPos() => false;

    public bool OnMouseDown(PointerInteractEventData evt)
    {
        Console.WriteLine($"Box clicked at: {evt.ScreenPosition}");
        return true; // Consume event
    }

    public bool OnMouseUp(PointerInteractEventData evt) => false;
}
```

---

## 📄 License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
