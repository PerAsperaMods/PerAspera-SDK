# PerAspera.GameAPI.UI — IMGUI SDK Components

Standardized IMGUI helpers for Per Aspera mod development. Create panels, colors, and resources displays matching the native game UI style.

## Overview

The UI SDK provides reusable IMGUI components that match Per Aspera's native visual style:
- **UIColors** — Standardized color palette (cyan for values, orange for warnings, etc.)
- **UIStyles** — Pre-configured GUI styles (Header, Label, Value, Warning, Error, Button)
- **UIPanel** — Draggable window panels with headers and close buttons
- **UIResourceRow** — Standardized rows for displaying resource data

## Quick Start

```csharp
using PerAspera.ModSDK;  // Auto-imports PerAspera.GameAPI.UI

// In your mod's OnGUI() method:
private UIPanel _resourcePanel = new UIPanel("Resources", new Rect(100, 100, 300, 400));

void OnGUI()
{
    _resourcePanel.OnGUI(() => {
        UIResourceRow.DrawRow("Oxygen", 150.5f, "F1");
        UIResourceRow.DrawRow("Water", 75.2f, "F1");
    });
}
```

## Components

### UIColors

Static class with predefined colors:

```csharp
var cyan = UIColors.Primary;       // Cyan for values
var white = UIColors.Secondary;    // White for labels
var orange = UIColors.Warning;     // Orange for alerts
var red = UIColors.Error;          // Red for critical alerts
var green = UIColors.Success;      // Green for completed states
```

Helper methods:

```csharp
// Get color by status
var color = UIColors.GetStatusColor("OK");      // → Green
var color = UIColors.GetStatusColor("WARNING"); // → Orange
var color = UIColors.GetStatusColor("ERROR");   // → Red

// Get color by temperature (for climate data)
var color = UIColors.GetTemperatureColor(-50f);  // → Blue (cold)
var color = UIColors.GetTemperatureColor(150f);  // → Red (hot)

// Set alpha
var transparent = UIColors.WithAlpha(UIColors.Primary, 0.5f);
```

### UIStyles

Static factory for GUI styles:

```csharp
// Pre-initialized styles
var headerStyle = UIStyles.Header;   // Bold white, 14pt
var labelStyle  = UIStyles.Label;    // White, 11pt
var valueStyle  = UIStyles.Value;    // Cyan bold, 11pt
var warningStyle = UIStyles.Warning; // Orange, 10pt
var errorStyle  = UIStyles.Error;    // Red bold, 10pt

// Usage
GUI.Label(rect, "O2:", UIStyles.Label);
GUI.Label(rect, "150.5", UIStyles.Value);
```

Force reinitialize styles:
```csharp
UIStyles.Reset();  // Useful for theme changes
```

### UIPanel

Draggable window with header and close button:

```csharp
// Create panel
var panel = new UIPanel("My Panel", new Rect(100, 100, 300, 400));

// Render in OnGUI
panel.OnGUI(() => {
    GUILayout.Label("Your content here");
    if (GUILayout.Button("Action")) {
        // Handle click
    }
});

// Control visibility
panel.IsVisible = false;
panel.Toggle();
panel.CenterOnScreen(300, 400);

// Access/modify size
var rect = panel.WindowRect;
```

### UIResourceRow

Helper methods to draw standardized resource rows:

```csharp
// Simple row: label + value
UIResourceRow.DrawRow("Oxygen", 150.5f);

// Row with custom format
UIResourceRow.DrawRow("Water", 75.23456f, "F2");  // "75.23"

// Row with progress bar
UIResourceRow.DrawRowWithBar("Stored Resources", 150, 200);

// Row with status indicator (colored square)
UIResourceRow.DrawRowWithStatus("Power", 85.5f, "OK");
UIResourceRow.DrawRowWithStatus("Health", 20f, "ERROR");
```

## Integration with Mods

### MasterGUI Example

```csharp
using PerAspera.ModSDK;

class MasterGUIHandler
{
    private UIPanel _mainPanel = new UIPanel("Master Control", new Rect(50, 50, 400, 600));
    private UIPanel _statsPanel = new UIPanel("Statistics", new Rect(500, 50, 300, 200));

    void OnGUI()
    {
        _mainPanel.OnGUI(() => {
            GUILayout.Label("Overview", UIStyles.Header);
            UIResourceRow.DrawRow("Drones Active", 42f);
            UIResourceRow.DrawRowWithBar("Battery", 750, 1000);
        });

        _statsPanel.OnGUI(() => {
            GUILayout.Label("Per Minute", UIStyles.Label);
            UIResourceRow.DrawRow("Production", 5.2f, "F2");
            UIResourceRow.DrawRow("Consumption", 3.1f, "F2");
        });
    }
}
```

### POI Validator Example

```csharp
using PerAspera.ModSDK;

class POIBrowserPanel
{
    private UIPanel _poiBrowser = new UIPanel("POI Browser", new Rect(100, 100, 350, 500));
    private List<POIData> _pois = new();

    void OnGUI()
    {
        _poiBrowser.OnGUI(() => {
            foreach (var poi in _pois)
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(poi.Name, UIStyles.Label, GUILayout.Width(150));
                    GUILayout.Label(poi.Type, UIStyles.Value, GUILayout.Width(100));
                    if (GUILayout.Button("Inspect")) {
                        SelectPOI(poi);
                    }
                }
                GUILayout.EndHorizontal();
            }
        });
    }
}
```

## Styling Philosophy

The UI SDK follows Per Aspera's native design:

- **Color Scheme** : Dark blue-grey panels with cyan accents for values
- **Hierarchy** : Bold headers, white labels, cyan values, colored warnings
- **Density** : Compact rows with clear spacing
- **Consistency** : All mods using the SDK look native

## Performance Notes

- **UIStyles** are lazy-initialized and cached — no allocations after first access
- **UIPanel** uses `GUILayout.BeginArea` for efficiency
- All color methods are allocation-free
- Use `OnGUI()` pattern for responsiveness

## Tier 2 & 3 Components (Coming Soon)

- `UIStatusLabel` — Colored status badges
- `UICollapsible` — Expandable/collapsible categories
- `UIGraph` — Wrapper for game's Graph + data binding
- `UILayoutHelpers` — Automatic spacing and alignment

---

**Version** : 1.0.0  
**Namespace** : `PerAspera.GameAPI.UI` (auto-imported via ModSDK)  
**Agent Expert** : `@per-aspera-sdk-ui`
