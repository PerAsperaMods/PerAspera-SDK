using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;

// Test simple pour vérifier PerAspera.GameLibs
[BepInPlugin("TestPackage", "Test PerAspera GameLibs", "1.0.0")]
public class TestPlugin : BasePlugin
{
    public override void Load()
    {
        Log.LogInfo("✅ PerAspera.GameLibs package works!");
        
        // Test d'accès aux types Per Aspera
        Log.LogInfo($"✅ BaseGame type available: {typeof(BaseGame).FullName}");
        Log.LogInfo($"✅ Universe type available: {typeof(Universe).FullName}");
        Log.LogInfo($"✅ Planet type available: {typeof(Planet).FullName}");
        
        Log.LogInfo("🎯 Package test completed successfully!");
    }
}
