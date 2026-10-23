using Rasm.UI.Access;
using Rasm.UI.Inputs;
using Rasm.UI.Motion;
using Rasm.UI.Paint;
using Rasm.UI.Scenes;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Target {
    public sealed record Raster(nint Pixels, int Stride, SKImageInfo Info) : Target;
    public sealed record Gpu(GRRecordingContext Context, GRBackendTexture Texture, GRSurfaceOrigin Origin, SKColorType ColorType) : Target;
}

public sealed record Display(float Scale, SKColorSpace ColorSpace);

public sealed record Surface(
    Func<Func<Target, IO<Unit>>, IO<Unit>> Frame,
    IO<Display> Display,
    Func<Seq<SKRect>, IO<Unit>> Invalidate,
    Func<Seq<Reserved>, IO<Unit>> Reserve,
    Func<bool, IO<Unit>> Capture,
    IO<Unit> Focus,
    Func<CursorKind, IO<Unit>> Cursor,
    IO<Theme> Theme,
    Func<Func<Theme, IO<Unit>>, Action<IDisposable>, IO<Unit>> Themed,
    IO<TimeSpan> Now,
    Func<Func<TimeSpan, IO<Unit>>, IO<IDisposable>> Ticks,
    Func<Seq<Accessible>, IO<Unit>> Publish);

internal sealed record Live(Scene Scene, Routing Routing, Seq<SKRect> Damage, Seq<Accessible> Tree) {
    public CursorKind Cursor => (Routing.Captured | Routing.Hover).Bind(Scene.Layout.Path).Bind(static path => path.Head).Map(static laid => laid.Node.Cursor).IfNone(CursorKind.Default);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class Mounted : IDisposable {
    private readonly Surface surface;
    private readonly PaintCache cache = new();
    private Live live;
    private Option<Func<TimeSpan, (Node View, Settling Settling)>> playing;
    private Option<IDisposable> ticking;

    private Mounted(Surface surface, Scene scene) => (this.surface, live) = (surface, new Live(scene, Routing.Idle, Seq(SKRect.Create(scene.Size)), []));

    public static IO<Mounted> Mount(Surface surface, Node view, SKSize size, Action<IDisposable> retain) =>
        from theme in surface.Theme
        let mounted = new Mounted(surface, Scene.Build(theme, view, size))
        from owned in IO.lift(() => retain(mounted))
        from themed in surface.Themed(mounted.Restyle, retain)
        from shown in mounted.Step(static scene => (scene, Seq(SKRect.Create(scene.Size))))
        select mounted;

    // --- [TRANSITIONS]
    public IO<Unit> Show(Node view) => Halted().Bind(_ => Step(scene => scene.Reconcile(view)));

    public IO<Unit> Animate(Func<TimeSpan, (Node View, Settling Settling)> view) =>
        from played in IO.lift(() => playing = Some(view))
        from now in surface.Now
        from shown in Tick(now)
        select unit;

    public IO<Unit> Resize(SKSize size) => Step(scene => scene.Resize(size));

    public IO<Unit> Restyle(Theme theme) => Step(scene => scene.Restyle(theme));

    private IO<Unit> Step(Func<Scene, (Scene Scene, Seq<SKRect> Damage)> transition) =>
        from before in IO.lift(() => live)
        from next in IO.lift(() => Advance(held => transition(held.Scene) switch {
            var (scene, damage) => new Live(scene, held.Routing.Pruned(scene.Layout), damage, Accessibility.Tree(scene.Layout)),
        }))
        from invalidated in surface.Invalidate(next.Damage)
        from reserved in surface.Reserve(next.Scene.Reservations)
        from published in when(next.Tree != before.Tree, surface.Publish(next.Tree))
        select unit;

    // --- [MOTION]
    private IO<Unit> Tick(TimeSpan now) =>
        playing.Match(
            Some: view => view(now) switch {
                var (node, settling) =>
                    from shown in Step(scene => scene.Reconcile(node))
                    from paced in settling is Settling.Busy ? Ticking() : Stopped()
                    select unit,
            },
            None: static () => IO.pure(unit));

    private IO<Unit> Ticking() =>
        unless(ticking.IsSome, surface.Ticks(Tick).Map(lease => ticking = Some(lease)).Map(static _ => unit)).As();

    private IO<Unit> Stopped() => IO.lift(() => {
        _ = ticking.Iter(static lease => lease.Dispose());
        ticking = None;
    });

    private IO<Unit> Halted() => Stopped().Map(_ => playing = None).Map(static _ => unit);

    // --- [INPUT]
    public IO<Response> Receive(Input input) =>
        from before in IO.lift(() => live)
        from routed in Routes.Dispatch(IO.lift(() => live.Scene), before.Routing, input)
        from after in IO.lift(() => Advance(held => held with { Routing = routed.Routing.Pruned(held.Scene.Layout) }))
        from captured in when(after.Routing.Captured.IsSome != before.Routing.Captured.IsSome, surface.Capture(after.Routing.Captured.IsSome))
        from focused in when(after.Routing.Focus.Head.IsSome && after.Routing.Focus.Head != before.Routing.Focus.Head, surface.Focus)
        from cursor in when(after.Cursor != before.Cursor, surface.Cursor(after.Cursor))
        select routed.Response;

    public IO<Response> Perform(Accessible element, AccessAction action) =>
        Receive(new Input.Act(action, new SKPoint(element.Bounds.MidX, element.Bounds.MidY)));

    // --- [RENDER]
    public IO<Unit> Render(Seq<SKRect> damage) =>
        damage.Head.Map(first => damage.Tail.Fold(first, SKRect.Union)).Match(
            Some: clip => from display in surface.Display
                          from painted in surface.Frame(target => Painted(target, display, live.Scene, clip))
                          select unit,
            None: static () => IO.pure(unit));

    private IO<Unit> Painted(Target target, Display display, Scene scene, SKRect clip) =>
        (from drawn in use(() => target.Switch(
                display,
                raster: static (_, raster) => SKSurface.Create(raster.Info, raster.Pixels, raster.Stride),
                gpu: static (shown, gpu) => SKSurface.Create(gpu.Context, gpu.Texture, gpu.Origin, 0, gpu.ColorType, shown.ColorSpace)))
         from opened in IO.lift(() => {
             _ = drawn.Canvas.Save();
             drawn.Canvas.Scale(display.Scale);
             drawn.Canvas.ClipRect(clip);
             drawn.Canvas.Clear(SKColors.Transparent);
         })
         from painted in Painter.Paint(drawn.Canvas, scene, cache, display.Scale)
         from closed in IO.lift(() => {
             drawn.Canvas.Restore();
             drawn.Flush();
         })
         select unit).Bracket();

    private Live Advance(Func<Live, Live> transition) {
        Live next = transition(live);
        live = next;
        return next;
    }

    public void Dispose() {
        _ = ticking.Iter(static lease => lease.Dispose());
        ticking = None;
        cache.Dispose();
    }
}
