using CoreAnimation;
using CoreFoundation;
using Metal;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using SkiaSharp;

namespace Rasm.Rhino.UI.Mac;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class MetalContext {
    // --- [STATE]
    private readonly Atom<LanguageExt.HashSet<CAMetalLayer>> shown = Atom(LanguageExt.HashSet<CAMetalLayer>.Empty);

    private MetalContext(IMTLDevice device, IMTLCommandQueue queue, GRContext context) => (Device, Queue, Context) = (device, queue, context);

    public IMTLDevice Device { get; }

    public IMTLCommandQueue Queue { get; }

    public GRContext Context { get; }

    // --- [ACQUIRE]
    internal static IO<MetalContext> Acquired(IPlugInSink owner, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from device in DisposalOps.Acquire(IO.lift(static () => Refused.UnlessFound(MTLDevice.SystemDefault, nameof(MTLDevice.SystemDefault))), owned)
            from queue in DisposalOps.Acquire(IO.lift(() => Refused.UnlessFound(device.CreateCommandQueue(), nameof(IMTLDevice.CreateCommandQueue))), owned)
            from context in DisposalOps.Acquire(DisposalOps.Within(
                () => new GRMtlBackendContext { DeviceHandle = device.Handle, QueueHandle = queue.Handle },
                static backend => IO.lift(() => Refused.UnlessFound(GRContext.CreateMetal(backend), nameof(GRContext.CreateMetal)))), owned)
            let site = new CallbackSite(owner, typeof(DispatchSource.MemoryPressure), nameof(DispatchSource.SetEventHandler))
            from pressure in DisposalOps.Acquire(
                IO.lift(static () => new DispatchSource.MemoryPressure(MemoryPressureFlags.Warn | MemoryPressureFlags.Critical, DispatchQueue.MainQueue)),
                static source => Callbacks.Each(Seq(IO.lift(source.Cancel), IO.lift(source.Dispose))).Map(unit), owned)
            from watched in IO.lift(() => {
                pressure.SetEventHandler(() => Callbacks.Run(IO.lift(() => {
                    if (pressure.PressureFlags.HasFlag(MemoryPressureFlags.Critical)) context.PurgeResources();
                    else context.PurgeUnlockedResources(scratchResourcesOnly: false);
                }), site));
                pressure.Resume();
            })
            select new MetalContext(device, queue, context),
            retain);

    // --- [SHOWN]
    internal IO<Unit> Shown(CAMetalLayer layer, Visibility visibility) =>
        IO.lift(() => (Before: shown.Value, After: shown.Swap(held => visibility != Visibility.Hidden ? held.AddOrUpdate(layer) : held.Remove(layer))))
            .Bind(moved => when(moved.After.IsEmpty && !moved.Before.IsEmpty, IO.lift(() => Context.PurgeUnlockedResources(scratchResourcesOnly: false))).As());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Metal {
    // --- [TARGETS]
    internal static IO<OpaqueTarget> Targets(IPlugInSink owner, Action<IDisposable> retain) =>
        MetalContext.Acquired(owner, retain).Map(static context => (OpaqueTarget)((view, attached) => Attach(context, view, attached)));

    // --- [ATTACH]
    internal static IO<Unit> Attach(MetalContext context, SceneView view, Action<IDisposable> retain) =>
        from layer in DisposalOps.Acquire(IO.lift(() => new CAMetalLayer { Device = context.Device, FramebufferOnly = false }), retain)
        from shown in DisposalOps.Acquire(context.Shown(layer, Visibility.Shown), _ => context.Shown(layer, Visibility.Hidden), retain)
        from layered in DrawnViews.Layers(view, layer, view.Sink, retain)
        select unit;
}
