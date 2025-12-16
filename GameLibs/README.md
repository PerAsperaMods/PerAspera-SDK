# 🎮 GameLibs - Per Aspera Reference Assemblies

## 🎯 Purpose

This folder contains **reference assemblies** for Per Aspera game compilation. These are NOT the original game DLL files, but **stripped assemblies** containing only metadata (types, methods, properties) needed for compilation.

## ⚙️ Auto-Detection

The SDK automatically detects game assemblies in this order:

1. **Custom Path**: `-p:PerAsperaGameLibs=C:\Path\To\GameLibs`
2. **BepInEx Interop**: `%USERPROFILE%\.steam\steam\steamapps\common\Per Aspera\BepInEx\interop\`
3. **Local GameLibs**: This folder (included in SDK)

## 🔧 Setup Options

### Option 1: Use Included GameLibs (Recommended)
```bash
git clone https://github.com/PerAsperaMods/PerAspera-SDK
cd PerAspera-SDK
dotnet build  # ✅ Works out of the box
```

### Option 2: BepInEx Auto-Detection
```bash
# 1. Install Per Aspera + BepInEx 6.x
# 2. Launch game once (generates interop assemblies)
# 3. Build SDK
dotnet build -p:PerAsperaGameLibs=""  # Forces BepInEx detection
```

### Option 3: Custom GameLibs Path
```bash
# Point to your custom reference assemblies
dotnet build -p:PerAsperaGameLibs="D:\MyGameLibs"
```

### Option 4: Generate Your Own
```bash
# Use GameLibsMaker to create reference assemblies
.\Tools\GameLibsMaker.exe -input "C:\SteamLibrary\...\Per Aspera" -output ".\MyGameLibs"
dotnet build -p:PerAsperaGameLibs=".\MyGameLibs"
```

## 📋 Requirements

Required assemblies for compilation:
- `Assembly-CSharp.dll` (main game code)
- `UnityEngine.*.dll` (Unity runtime)
- `Cinemachine.dll`, `TextMeshPro.dll`, etc.

## 🚫 Legal Notes

- ✅ **Reference assemblies** (metadata only) - Legal to distribute
- ❌ **Original game DLL** - Copyrighted, not included
- ✅ **BepInEx interop** - Generated assemblies, legal
- ✅ **GameLibsMaker output** - Stripped assemblies, legal

## 🎯 For Mod Developers

```xml
<!-- In your mod project -->
<PropertyGroup>
  <!-- Use SDK with auto-detected assemblies -->
  <PerAsperaGameLibs>auto</PerAsperaGameLibs>
</PropertyGroup>

<ItemGroup>
  <PackageReference Include="PerAspera.ModSDK" Version="1.1.0" />
</ItemGroup>
```

## 🔍 Troubleshooting

### Build Error: "Assembly-CSharp.dll not found"
```bash
# Check BepInEx installation
ls "%USERPROFILE%\.steam\steam\steamapps\common\Per Aspera\BepInEx\interop"

# Or use custom path
dotnet build -p:PerAsperaGameLibs="path\to\your\gamelibs"
```

### Build Error: "Type 'Planet' not found"
- Ensure Per Aspera game is installed
- Run the game with BepInEx once to generate interop assemblies
- Check that Assembly-CSharp.dll contains game types

---

**🚀 Ready to mod? The SDK handles the complexity for you!**