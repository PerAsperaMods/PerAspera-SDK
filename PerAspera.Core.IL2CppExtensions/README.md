# PerAspera.Core.IL2CppExtensions

Extension methods and helpers for safe IL2CPP interop with Unity objects in Per Aspera modding.

## 📦 Purpose

Provides type-safe extension methods for working with IL2CPP objects through reflection, eliminating the need for unsafe casts and manual null checks.

## 🎯 Key Features

- **Safe Reflection**: `GetMemberValue<T>`, `SetMemberValue`, `InvokeMethod<T>`
- **Convenience Aliases**: `GetFieldValue`, `GetPropertyValue`, `SetFieldValue`, `SetPropertyValue`
- **IL2CPP Type Handling**: `GetIl2CppType` for proper type resolution
- **Automatic Type Conversion**: Safe conversion between IL2CPP and managed types
- **Null Safety**: All methods handle null gracefully with default returns

## 🔧 Usage

```csharp
using PerAspera.Core.IL2CPP;

// Get field value
float temperature = planetObj.GetFieldValue<float>("_temperature");

// Invoke method
int buildingCount = universeObj.InvokeMethod<int>("GetBuildingCount");

// Set property
buildingObj.SetPropertyValue("_activated", true);

// Get IL2CPP type
var type = nativeObj.GetIl2CppType();
```

## 📚 Extension Methods

### GetMemberValue<T>
Safely retrieves property or field value with type conversion.

### SetMemberValue
Sets property or field value on IL2CPP object.

### InvokeMethod<TResult>
Invokes method with parameters and type-safe return.

### GetFieldValue<T> / GetPropertyValue<T>
Convenience aliases for `GetMemberValue<T>`.

### SetFieldValue / SetPropertyValue
Convenience aliases for `SetMemberValue`.

### GetIl2CppType
Resolves the actual IL2CPP type of an object.

## 🏗️ Architecture Position

```
PerAspera.Core (Logging, Utilities)
    ↓
PerAspera.Core.IL2CppExtensions ← YOU ARE HERE
    ↓
PerAspera.GameAPI (Native Types, Events)
    ↓
PerAspera.GameAPI.Wrappers (Public API)
```

## 🎯 Version

Follows SDK versioning from `Version.props`.

## 📝 Notes

- All extension methods handle null inputs gracefully
- Failed conversions return `default(T)` and log warnings
- Method invocation supports parameter type inference
- Supports both properties and fields transparently
