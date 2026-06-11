# Wrapper Generation Guide — Auto-Generate SDK Wrappers

## Problem
Creating wrappers manually for 100+ game classes is slow and tedious.

## Solution
**Automated wrapper generation** using PowerShell script.

---

## Quick Start

### 1️⃣ Register Native Class

Add to `NativeTypes.cs`:
```csharp
/// <summary>Thin holder for native MyGameClass instance.</summary>
public class MyGameClassNative
{
    public object NativeInstance { get; }
    public MyGameClassNative(object native) => NativeInstance = native;
}
```

### 2️⃣ Run Generator Script

```powershell
cd F:\ModPeraspera\SDK
./Generate-Wrappers.ps1
```

**Output:**
```
✅ Found 15 *Native classes
✅ CREATED: BuildingWrapper.cs
✅ CREATED: FactionWrapper.cs
✅ CREATED: MyGameClassWrapper.cs
...
🎉 Generated 3 wrapper(s)
```

### 3️⃣ Add Properties

Edit the generated wrapper:
```csharp
public class MyGameClassWrapper : WrapperBase
{
    public string? Name => GetProperty<string>("name");
    public int? Count => GetProperty<int>("itemCount");
    
    public void DoSomething()
    {
        InvokeMethod("MethodName");
    }
}
```

### 4️⃣ Build & Done!

```powershell
dotnet build F:\ModPeraspera\SDK\SDK.sln
```

---

## When Game Code Changes

If **Tlou Games adds new classes** to the game:

1. Add to `NativeTypes.cs`
2. Run script again with `-Force` to regenerate:
   ```powershell
   ./Generate-Wrappers.ps1 -Force
   ```
3. Update properties
4. Rebuild

**Total time: ~5 minutes** instead of 1-2 hours manual work!

---

## Workflow Examples

### Example 1: New Routing Class

Game update adds `PathFinder` class.

```powershell
# 1. Add to NativeTypes.cs
public class PathFinderNative { ... }

# 2. Generate
./Generate-Wrappers.ps1

# 3. Edit generated PathFinderWrapper.cs
# public List<Vector3>? GetPath() => GetProperty<List<Vector3>>("path");

# 4. Build
dotnet build
```

### Example 2: New Resource Type

Game update adds `ResourceCategory` class.

Same steps — generator handles it automatically!

---

## Script Features

| Flag | Purpose |
|------|---------|
| (none) | Generate new wrappers only (skip existing) |
| `-Force` | Overwrite existing wrappers |
| `-OutputDir` | Custom output directory (default: SDK/Wrappers) |

---

## Common Tasks

### List all Native types
```powershell
grep "public class.*Native" F:\ModPeraspera\SDK\PerAspera.GameAPI.Native\NativeTypes.cs
```

### Regenerate all wrappers
```powershell
./Generate-Wrappers.ps1 -Force
```

### Check what wrappers exist
```powershell
ls F:\ModPeraspera\SDK\PerAspera.GameAPI.Wrappers\*Wrapper.cs | Measure-Object
```

---

## Architecture

```
Game Update (Tlou Games)
    ↓
Add *Native class to NativeTypes.cs
    ↓
Run Generate-Wrappers.ps1
    ↓
Auto-generated *Wrapper.cs created
    ↓
Add properties to wrapper (5-10 min per wrapper)
    ↓
Build SDK
    ↓
Mods can use: var wrapper = new MyGameClassWrapper(nativeObj);
```

---

## Integration with CI/CD

Could add to GitHub Actions:
```yaml
- name: Generate Wrappers
  run: pwsh ./SDK/Generate-Wrappers.ps1
  
- name: Build SDK
  run: dotnet build ./SDK/SDK.sln
```

Every time NativeTypes.cs changes → Wrappers auto-regenerate!

---

## Questions?

- **Do I need to run this manually?** Yes, when game classes change.
- **Does it overwrite my properties?** No, only regenerates shell. Use `-Force` if you want to reset.
- **What if a Native class is deprecated?** Remove from NativeTypes.cs, wrapper becomes stale (can delete manually).
- **Can I customize generated code?** Yes! Edit properties after generation.

---

## Next Steps

1. ✅ Create NativeTypes.cs entries for key classes
2. ✅ Run generator
3. ✅ Add properties to each wrapper
4. ✅ Build & release SDK

**Target: 50 wrappers in 1 week** instead of 1 per day!
