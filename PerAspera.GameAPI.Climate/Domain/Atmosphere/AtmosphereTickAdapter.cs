using Il2CppInterop.Runtime;
using PerAspera.GameAPI.Climate;
using System;

public class AtmosphereTickAdapter : ITick
{
    private readonly AtmosphereGrid _grid;

    public AtmosphereTickAdapter(IntPtr ptr) : base(ptr) { }

    public AtmosphereTickAdapter(AtmosphereGrid grid)
        : base(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ITick>.NativeClassPtr))
    {
        _grid = grid;
    }

    public override void OnTick(float deltaDays)
    {
        _grid.Tick(deltaDays);
    }
}
