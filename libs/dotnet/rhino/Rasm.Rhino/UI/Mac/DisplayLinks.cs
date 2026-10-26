using System.Runtime.InteropServices;
using AppKit;
using CoreAnimation;
using Eto.Forms;
using Eto.Mac.Forms;
using Foundation;
using ObjCRuntime;

namespace Rasm.Rhino.UI.Mac;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class DisplayLinks {
    // --- [DELIVERY]
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Frame(nint view, nint selector, nint link);

    private static readonly Selector Tick = new($"{typeof(DisplayLinks).Assembly.GetName().Name}.frame:");
    private static readonly AtomHashMap<nint, Fin<Unit>> Targets = AtomHashMap<nint, Fin<Unit>>();
    private static readonly AtomHashMap<nint, (CADisplayLink Link, Func<Duration, IO<Unit>> Tick, CallbackSite Site)> Links =
        AtomHashMap<nint, (CADisplayLink Link, Func<Duration, IO<Unit>> Tick, CallbackSite Site)>();

    private static Frame Deliver(CallbackSite site) =>
        (_, _, link) => Callbacks.Run(IO.lift(() => Links.Find(link).Iter(static held => Callbacks.Run(held.Tick(Duration.FromSeconds(held.Link.TargetTimestamp)), held.Site))), site);

    // --- [SOURCES]
    internal static IO<Func<bool, IO<Unit>>> Of(Control view, Func<Duration, IO<Unit>> tick, IPlugInSink sink, Action<IDisposable> retain) =>
        from host in IO.lift(() => Refused.UnlessFound(view.GetContainerView(), nameof(MacControlExtensions.GetContainerView)))
        let site = new CallbackSite(sink, typeof(CADisplayLink), nameof(NSView.GetDisplayLink))
        from added in IO.lift(() => {
            nint target = Class.GetHandle(host.GetType());
            return Targets.FindOrAdd(target, () => Refused.Unless(Eto.Mac.ObjCExtensions.AddMethod(target, Tick.Handle, Deliver(site), "v@:@"), nameof(Eto.Mac.ObjCExtensions.AddMethod)));
        })
        from held in IO.lift(static () => Atom(Option<CADisplayLink>.None))
        from attached in NativeControls.Lifetime(view, owned =>
            from link in DisposalOps.Acquire(IO.lift(() => host.GetDisplayLink(host, Tick)), shown =>
                Callbacks.Each(Seq(IO.lift(shown.Invalidate), IO.lift(() => { _ = Links.Remove(shown.Handle); }), held.SwapIO(static _ => None).Map(unit), IO.lift(shown.Dispose))).Map(unit), owned)
            from linked in IO.lift(() => {
                _ = Links.Add(link.Handle, (link, tick, site));
                _ = held.Swap(_ => Some(link));
                link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
            })
            select unit,
            site, retain)
        select fun((bool running) => IO.lift(() => held.Value.Iter(link => link.Paused = !running)));
}
