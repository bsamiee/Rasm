using CoreAnimation;
using CoreFoundation;
using Metal;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Surfaces;
using SkiaSharp;

namespace Rasm.RhinoCommon.UI.Mac;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class MetalContext {
    // --- [STATE]
    private readonly Atom<HashSet<CAMetalLayer>> shown = Atom(HashSet<CAMetalLayer>());

    private MetalContext(IMTLDevice device, IMTLCommandQueue queue, GRContext context) => (Device, Queue, Context) = (device, queue, context);

    public IMTLDevice Device { get; }

    public IMTLCommandQueue Queue { get; }

    public GRContext Context { get; }

    // --- [ACQUIRE]
    internal static IO<MetalContext> Acquired(IPlugInLifetime owner) =>
        from device in use(IO.lift(static () => Conversions.Found(MTLDevice.SystemDefault)))
        from queue in use(IO.lift(() => Conversions.Found(device.CreateCommandQueue())))
        from context in use((from backend in use(() => new GRMtlBackendContext { DeviceHandle = device.Handle, QueueHandle = queue.Handle })
                             from made in IO.lift(() => Conversions.Found(GRContext.CreateMetal(backend)))
                             select made).Bracket())
        from pressure in use(
            static () => new DispatchSource.MemoryPressure(MemoryPressureFlags.Warn | MemoryPressureFlags.Critical, DispatchQueue.MainQueue),
            static source => Callbacks.Each(Seq(IO.lift(source.Cancel), IO.lift(source.Dispose))).Map(unit))
        from watched in IO.lift(() => {
            pressure.SetEventHandler(() => owner.Run(IO.lift(() => {
                if (pressure.PressureFlags.HasFlag(MemoryPressureFlags.Critical)) context.PurgeResources();
                else context.PurgeUnlockedResources(scratchResourcesOnly: false);
            })));
            pressure.Resume();
        })
        select new MetalContext(device, queue, context);

    // --- [SHOWN]
    internal IO<Unit> Showing(CAMetalLayer layer) =>
        IO.lift(() => Moved(layer, visible: true)) >> Products.Released(() => Moved(layer, visible: false));

    private Unit Moved(CAMetalLayer layer, bool visible) {
        HashSet<CAMetalLayer> before = shown.Value;
        HashSet<CAMetalLayer> after = shown.Swap(held => visible ? held.AddOrUpdate(layer) : held.Remove(layer));
        if (after.IsEmpty && !before.IsEmpty) Context.PurgeUnlockedResources(scratchResourcesOnly: false);
        return unit;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Metal {
    // --- [TARGETS]
    internal static IO<OpaqueTarget> Targets(IPlugInLifetime owner) =>
        MetalContext.Acquired(owner).Map(static context => (OpaqueTarget)(view => Attach(context, view)));

    // --- [ATTACH]
    internal static IO<Unit> Attach(MetalContext context, SceneView view) =>
        from layer in use(() => new CAMetalLayer { Device = context.Device, FramebufferOnly = false })
        from shown in context.Showing(layer)
        from layered in DrawnViews.Layers(view, layer, view.Owner)
        select unit;
}
