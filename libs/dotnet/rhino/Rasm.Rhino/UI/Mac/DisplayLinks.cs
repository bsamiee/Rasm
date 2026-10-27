using System.Runtime.InteropServices;
using CoreAnimation;
using Eto.Forms;
using Eto.Mac.Forms;
using Foundation;
using ObjCRuntime;
using Rasm.RhinoCommon.PlugIns;

namespace Rasm.RhinoCommon.UI.Mac;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DisplayLinks {
    // --- [DELIVERY]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Frame(nint view, nint selector, nint link);

    private sealed record Link(CADisplayLink Native, Func<Duration, IO<Unit>> Tick, IPlugInLifetime Owner);

    private static readonly Selector Tick = new($"{typeof(DisplayLinks).Assembly.GetName().Name}.frame:");
    private static readonly AtomHashMap<nint, Fin<Unit>> Targets = AtomHashMap<nint, Fin<Unit>>();
    private static readonly AtomHashMap<nint, Link> Links = AtomHashMap<nint, Link>();

    private static Frame Deliver(IPlugInLifetime owner) =>
        (_, _, link) => owner.Run(IO.lift(() => Links.Find(link).Iter(static held => held.Owner.Run(held.Tick(Duration.FromSeconds(held.Native.TargetTimestamp))))));

    // --- [SOURCES]
    internal static IO<Func<bool, IO<Unit>>> Of(Control view, Func<Duration, IO<Unit>> tick, IPlugInLifetime owner) =>
        from host in IO.lift(() => Conversions.Found(view.GetContainerView()))
        from added in IO.lift(() => {
            nint target = Class.GetHandle(host.GetType());
            return Targets.FindOrAdd(target, () => RhinoFault.Unless(Eto.Mac.ObjCExtensions.AddMethod(target, Tick.Handle, Deliver(owner), "v@:@")));
        })
        from held in IO.lift(static () => Atom(Option<CADisplayLink>.None))
        from attached in NativeControls.Lifetime(view,
            from link in use(IO.lift(() => host.GetDisplayLink(host, Tick)), shown =>
                Callbacks.Each(Seq(IO.lift(shown.Invalidate), IO.lift(() => { _ = Links.Remove(shown.Handle); }), held.SwapIO(static _ => None).Map(unit), IO.lift(shown.Dispose))).Map(unit))
            from linked in IO.lift(() => {
                _ = Links.Add(link.Handle, new Link(link, tick, owner));
                _ = held.Swap(_ => Some(link));
                link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
            })
            select unit,
            owner)
        select fun((bool running) => IO.lift(() => held.Value.Iter(link => link.Paused = !running)));
}
