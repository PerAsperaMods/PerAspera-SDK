# IL2CPP Compatibility Guide

## AddComponent Method Limitation

### The Problem

In IL2CPP Unity builds, the non-generic `GameObject.AddComponent(Type)` method **does not exist**. This is a fundamental limitation of IL2CPP's AOT (Ahead-Of-Time) compilation.

The error you might see:
```
Method not found: 'UnityEngine.Component UnityEngine.GameObject.AddComponent(System.Type)'
```

This error occurs when:
1. Code tries to call `gameObject.AddComponent(typeof(SomeComponent))`
2. IL2CppInterop's internal method caching tries to find this method
3. Third-party libraries use reflection to call AddComponent with a Type parameter

### The Solution

**Always use the generic method:**
```csharp
// ❌ WRONG - This will fail in IL2CPP
var component = gameObject.AddComponent(typeof(MyComponent));

// ✅ CORRECT - Use the generic method
var component = gameObject.AddComponent<MyComponent>();
```

### Using IL2CppComponentHelper

This project provides `IL2CppComponentHelper` to safely add components with proper error handling and type registration:

```csharp
using PerAspera.SDK.TwitchIntegration;

// Add component to existing GameObject
var component = IL2CppComponentHelper.AddComponentSafe<MyComponent>(gameObject);

// Create new GameObject with component
var component = IL2CppComponentHelper.CreateGameObjectWithComponent<MyComponent>(
    "MyGameObject", 
    dontDestroyOnLoad: true
);

// Get existing or add new component
var component = IL2CppComponentHelper.GetOrAddComponent<MyComponent>(gameObject);
```

### Type Registration

When adding custom MonoBehaviour components in IL2CPP, you must register them first:

```csharp
using Il2CppInterop.Runtime.Injection;

// Register type before use
ClassInjector.RegisterTypeInIl2Cpp<MyCustomComponent>();

// Then you can add it
var component = gameObject.AddComponent<MyCustomComponent>();
```

The `IL2CppComponentHelper` handles this automatically.

### Common Issues

#### Issue: "The type initializer for 'MethodInfoStoreGeneric_AddComponent_Public_T_0`1' threw an exception"

This error occurs when IL2CppInterop's internal method caching system tries to cache both generic and non-generic versions of AddComponent, but fails because the non-generic version doesn't exist in IL2CPP.

**Cause**: Usually triggered by:
- Using libraries not designed for IL2CPP
- Reflection code trying to get the AddComponent method by Type
- IL2CppInterop version incompatibilities

**Solution**:
1. Ensure all code uses the generic `AddComponent<T>()` method
2. Use IL2CPP-compatible versions of third-party libraries
3. Avoid reflection-based component addition
4. Use the provided `IL2CppComponentHelper` utilities

#### Issue: Component added but not working

Make sure the type is properly registered with `ClassInjector.RegisterTypeInIl2Cpp<T>()` before creating instances.

### Best Practices

1. **Always use generics**: Never use `AddComponent(Type)` - it doesn't exist in IL2CPP
2. **Register types early**: Call `ClassInjector.RegisterTypeInIl2Cpp<T>()` during plugin initialization
3. **Handle exceptions**: Type registration might fail if already registered - catch and ignore these
4. **Use helpers**: The provided `IL2CppComponentHelper` handles common pitfalls
5. **Avoid reflection**: Don't use `typeof()` with AddComponent in IL2CPP environments

### Additional Resources

- [BepInEx IL2CPP Documentation](https://docs.bepinex.dev/master/articles/dev_guide/plugin_tutorial/4_IL2CPP.html)
- [Il2CppInterop Documentation](https://github.com/BepInEx/Il2CppInterop)
- [Unity IL2CPP Limitations](https://docs.unity3d.com/Manual/IL2CPP.html)

## For Library Authors

If you're creating a library to be used with IL2CPP:

1. **Never use non-generic AddComponent**: Always use `gameObject.AddComponent<T>()`
2. **Provide type registration helpers**: Make it easy for users to register types
3. **Document IL2CPP requirements**: Clearly state IL2CPP compatibility in your README
4. **Test with IL2CPP**: Don't assume Mono behaviors work in IL2CPP
5. **Avoid Type parameters**: Don't create APIs that take `Type` for component operations

### Example: IL2CPP-Safe API Design

```csharp
// ❌ BAD - Takes Type parameter
public Component AddComponentByType(GameObject go, Type componentType)
{
    return go.AddComponent(componentType); // Will fail in IL2CPP
}

// ✅ GOOD - Uses generics
public T AddComponentGeneric<T>(GameObject go) where T : Component
{
    return go.AddComponent<T>(); // Works in IL2CPP
}
```
