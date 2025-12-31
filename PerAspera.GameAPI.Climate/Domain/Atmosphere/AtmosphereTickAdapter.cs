using Il2CppInterop.Runtime;
using PerAspera.GameAPI.Climate;
using System;

public class AtmosphereTickAdapter : ITick
{
    // TODO: Implement AtmosphereGrid
    // private readonly AtmosphereGrid _grid;

    public AtmosphereTickAdapter(IntPtr ptr) : base(ptr) { }

    // TODO: Implement AtmosphereGrid
    // public AtmosphereTickAdapter(AtmosphereGrid grid)
    //     : base(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ITick>.NativeClassPtr))
    // {
    //     _grid = grid;
    // }

    public AtmosphereTickAdapter(object grid)
        : base(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<ITick>.NativeClassPtr))
    {
        // TODO: Store grid reference
    }

    public override void OnTick(float deltaDays)
    {
        // TODO: Call grid.Tick(deltaDays)
        // _grid.Tick(deltaDays);
    }
}
