# Pratyaksh.UI

[![NuGet Version](https://img.shields.io/nuget/v/Pratyaksh.UI.svg)](https://www.nuget.org/packages/Pratyaksh.UI/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

**Pratyaksh.UI** is a standalone, immediate/retained hybrid UI component toolkit powered by [Raylib-cs](https://github.com/Subtixx/Raylib-cs) and `Pratyaksh.Core`. It combines the ergonomic simplicity of immediate-mode layout declarations (`BeginHorizontal`, `BeginScrollView`, `SectionEx`) with the performance and state retention of retained widget lifecycles.

---

## 📦 Key Features

- **Hybrid Immediate/Retained Layout Engine**:
  - Declarative nested layout API (`BeginHorizontal`, `EndHorizontal`, `BeginVertical`, `EndVertical`, `BeginScrollView`, `EndScrollView`, `AddSpace`, `SectionEx`).
  - Automatic retained widget caching across frames to avoid per-frame widget recreation.
- **Rich 11-Widget Component Suite**:
  - `Button` (with custom background, border, text colors, and payload handlers).
  - `InputField` (supports placeholder text, focus state, text changes, and masked password mode).
  - `Dropdown` (expandable overlay selection menu).
  - `CycleSelector` (compact horizontal multi-choice stepper).
  - `ScrollView` (smooth mouse-wheel scrolling, draggable scrollbar thumb, and scissor clipping).
  - `Selectable` (toggleable selection item with custom payload).
  - `Toggle` (animated switch toggle).
  - `Slider` (interactive track bar with continuous or stepped values, formatted overlay value text, and drag interaction).
  - `LinkButton` (clickable hyperlinked button).
  - `AlertBanner` (status notification banner with success/warning/error/info styling).
  - `StatusBadge` (compact pill badge with color indicators).
- **Two-Way Data-Bound UI Adapters**:
  - Out-of-the-box binders (`RLToggleUI`, `RLInputFieldUI_String`, `RLInputFieldUI_Int`, `RLInputFieldUI_Float`, `RLSelectableUI`, `RLDropdownUI`, `RLSliderUI`) for zero-boilerplate model-view synchronization.
- **Raylib Hosting & Multi-Panel Canvas**:
  - Built-in `Canvas` managing persistent panels and transient popups (with click-away auto-dismissal).
  - `BaseRaylibEngine` providing game-loop integration, resizing events, delta-time calculations, and 2D camera support.

---

## 🚀 Installation

Install via the .NET CLI:

```bash
dotnet add package Pratyaksh.UI
```

Or via the NuGet Package Manager:

```powershell
Install-Package Pratyaksh.UI
```

---

## 🛠️ Usage Examples

### 1. Creating a Custom UI Panel

Inherit from `UILayoutBase` to build structured panels with declarative layout routines:

```csharp
using Pratyaksh.Core;
using Pratyaksh.UI;
using Raylib_cs;

public class MyCustomPanel : UILayoutBase
{
    private int buttonId = IdGen.GetNewID();
    private int inputId = IdGen.GetNewID();
    private int toggleId = IdGen.GetNewID();

    private string username = string.Empty;
    private bool notificationsEnabled = true;

    protected override string PanelName => "SettingsPanel";

    public MyCustomPanel(int x, int y, Drawable? parent = null) : base(x, y, 300, 250, parent) { }

    public override void OnDrawLayout()
    {
        // 1. Draw panel background and title header
        layout.SectionEx("Settings", Width, Height,
            Raylib.Fade(Color.DarkBrown, 0.8f),
            Raylib.Fade(Color.Gray, 0.7f),
            Color.White, 0.1f, false);

        layout.BeginHorizontalEx(0, (int)Position.X);
        {
            layout.AddSpace(10);

            layout.BeginVerticalEx(0, (int)Position.Y);
            {
                layout.AddSpace(50);

                // 2. Horizontal row: Label + InputField
                layout.BeginHorizontal(30);
                {
                    layout.Text("User: ", 15, Color.White);

                    layout.InputField(inputId, "Enter username...", username, 180, 25, (field) =>
                    {
                        username = field.InputFieldText;
                    });
                }
                layout.EndHorizontal(25);

                layout.AddSpace(10);

                // 3. Horizontal row: Toggle
                layout.BeginHorizontal(35);
                {
                    layout.Text("Alerts: ", 15, Color.White);

                    layout.Toggle(toggleId, notificationsEnabled, 50, 20, (tog) =>
                    {
                        notificationsEnabled = tog.IsOn;
                    }, toggleId);
                }
                layout.EndHorizontal(20);

                layout.AddSpace(15);

                // 4. Action Button
                layout.BeginHorizontal(10);
                {
                    layout.Button(buttonId, "Save Changes", 120, 30, (btn) =>
                    {
                        Console.WriteLine($"Saved! Username: {username}, Alerts: {notificationsEnabled}");
                    }, buttonId);
                }
                layout.EndHorizontal(30);
            }
            layout.EndVertical(Width);
        }
        layout.EndHorizontal(Height);
    }
}
```

### 2. Hosting in Raylib Application Loop

```csharp
using Pratyaksh.UI;
using Raylib_cs;

public class MyGameEngine : BaseRaylibEngine
{
    private Canvas canvas = null!;

    public MyGameEngine() : base(1024, 576, "Pratyaksh.UI Demo", clearScreen: true, clearColor: Color.DarkGray) { }

    protected override void OnSetup()
    {
        canvas = new Canvas((int)camera.GetWidth(), (int)camera.GetHeight(), (evt, target) => false);

        // Add panel to canvas
        var panel = new MyCustomPanel(50, 50, canvas);
        canvas.AddPanel(panel, saveable: false, transient: false);

        uiElements.Add(canvas);
    }
}

// Entry point
public static class Program
{
    public static void Main()
    {
        var engine = new MyGameEngine();
        engine.Start();
    }
}
```

---

## 📄 License

This project is licensed under the MIT License. See [LICENSE](../../LICENSE) for details.
