# Wrapper Enhancement Guide — Auto-Add Properties

## Problem
After generating a wrapper shell, manually adding properties is tedious.

## Solution
**Smart analyzer** that scans decompiled source and auto-generates property getters.

---

## Quick Start

### Step 1: Generate Wrapper
```powershell
cd F:\ModPeraspera\SDK
./Generate-Wrappers.ps1
```

Generates: `MyClassWrapper.cs` (empty shell)

### Step 2: Enhance with Properties
```powershell
./Enhance-Wrapper.ps1 -NativeClassName "MyClass" -WrapperName "MyClassWrapper"
```

Output:
```
Analyzing native class: MyClass
Found: F:\ModPeraspera_Raw_Extrac\PerAsperaData\ScriptsAssembly\MyClass.cs
Found 12 public properties
SUCCESS: Added 12 properties to MyClassWrapper
```

### Step 3: Review & Build
```powershell
# Check generated properties
cat F:\ModPeraspera\SDK\PerAspera.GameAPI.Wrappers\MyClassWrapper.cs

# Build
dotnet build F:\ModPeraspera\SDK\SDK.sln
```

---

## How It Works

### 1. Analysis Phase
Scans decompiled source for public properties:
```csharp
public string Name { get; }
public int Count { get; }
public List<Item> Items { get; }
```

### 2. Generation Phase
Creates type-safe getters:
```csharp
public string? Name => GetProperty<string>("name");
public int? Count => GetProperty<int>("count");
public IList<object>? Items => GetProperty<IList<object>>("items");
```

### 3. Insertion Phase
Adds to wrapper before closing brace:
```csharp
public class MyClassWrapper : WrapperBase
{
    // ... existing code ...

    // Properties from MyClass
    public string? Name => GetProperty<string>("name");
    public int? Count => GetProperty<int>("count");
    // ... etc
}
```

---

## Full Workflow Example

### Create 3 wrappers with properties in 5 minutes:

```powershell
# 1. Generate all shells
./Generate-Wrappers.ps1

# 2. Enhance each with properties
./Enhance-Wrapper.ps1 -NativeClassName "Building" -WrapperName "BuildingWrapper"
./Enhance-Wrapper.ps1 -NativeClassName "Faction" -WrapperName "FactionWrapper"
./Enhance-Wrapper.ps1 -NativeClassName "ResourceType" -WrapperName "ResourceTypeWrapper"

# 3. Build once
dotnet build F:\ModPeraspera\SDK\SDK.sln

# 3 wrappers done!
```

---

## Important Notes

### Property Name Mapping
The script **lowercases** property names:
- `MyProperty` → `GetProperty("myProperty")`
- `Count` → `GetProperty("count")`

If the game uses different casing, manually adjust in the wrapper.

### Type Conversion
IL2CPP types are auto-converted:
- `List<T>` → `IList<object>`
- `String` → `string?`
- `Int32` → `int?`

### Custom Properties
After enhancement, you can add custom methods:
```csharp
public class MyClassWrapper : WrapperBase
{
    // Auto-generated properties
    public string? Name => GetProperty<string>("name");
    
    // Custom method (add manually)
    public void DoSomething()
    {
        InvokeMethod("CustomMethod");
    }
}
```

---

## Troubleshooting

### "Could not find decompiled source for X"
→ The class name must match the filename in `F:\ModPeraspera_Raw_Extrac\PerAsperaData\ScriptsAssembly\`

### "No properties found to add"
→ The class might have only private properties or be a struct

### Properties look wrong
→ Check the decompiled source file for the actual property names and adjust manually

---

## Integration with CI/CD

Could add to GitHub Actions:
```yaml
- name: Generate Wrappers
  run: pwsh ./SDK/Generate-Wrappers.ps1

- name: Enhance Wrappers
  run: |
    pwsh ./SDK/Enhance-Wrapper.ps1 -NativeClassName "Building" -WrapperName "BuildingWrapper"
    pwsh ./SDK/Enhance-Wrapper.ps1 -NativeClassName "Faction" -WrapperName "FactionWrapper"
    # ... more wrappers

- name: Build SDK
  run: dotnet build ./SDK/SDK.sln
```

---

## Next Steps

1. Run: `./Generate-Wrappers.ps1`
2. For each wrapper: `./Enhance-Wrapper.ps1 -NativeClassName "X" -WrapperName "XWrapper"`
3. Build and test

**Target: 30 wrappers with full properties in 1 hour!**
