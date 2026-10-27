using System.Globalization;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class Visibility {
    public static readonly Visibility Hidden = new(drawn: false, hasKeyFocus: false);
    public static readonly Visibility Shown = new(drawn: true, hasKeyFocus: false);
    public static readonly Visibility Active = new(drawn: true, hasKeyFocus: true);

    public bool Drawn { get; }
    public bool HasKeyFocus { get; }

    public static Visibility Of(bool shown, bool key) => !shown ? Hidden : key ? Active : Shown;
}

public sealed record HostPlacement(Role Root) {
    public static HostPlacement Window { get; } = new(Role.Window) { Cycles = true };
    public static HostPlacement Panel { get; } = new(Role.Panel);

    public bool Cycles { get; init; }
}

public sealed record Host(SKSize Size, ColorScheme Theme, Visibility Visibility, HostPlacement Placement, CultureInfo Culture);

public sealed record SceneTarget<TPixels>(SKSurface Surface, SKImageInfo Info, Option<TPixels> Pixels) : IDisposable where TPixels : IDisposable {
    public void Dispose() => Surface.Dispose();
}

public sealed record Allocation<TPixels>(TPixels Pixels, SKPixmap Pixmap) where TPixels : IDisposable;

public sealed record SlotFrame(SKRectI Clip, SKRectI Frame);

public sealed record PlacedControl<TControl>(Hosted.Slot Slot, TControl Control) where TControl : notnull;

public sealed record SceneBackend<TPixels>(
    Func<SKSizeI, Fin<Allocation<TPixels>>> Allocated,
    Func<Seq<SKRect>, IO<Unit>> Invalidated,
    Func<Func<Duration, IO<Unit>>, IO<Func<bool, IO<Unit>>>> DisplayLinked) where TPixels : IDisposable {
    public SKSurfaceProperties Properties { get; init; } = Capture.Aligned;
    public Func<Mount, Output, IO<Unit>> Applied { get; init; } = static (_, _) => IO.pure(unit);
}

public sealed record PlacedControls<TControl>(
    Func<Hosted.Slot, SlotFrame, Option<Element>, IO<TControl>> Added,
    Func<TControl, Hosted.Slot, SlotFrame, IO<TControl>> Moved,
    Func<TControl, IO<Unit>> Removed) where TControl : notnull {
    public HashMap<Seq<Key>, PlacedControl<TControl>> Controls { get; private init; }

    public IO<Unit> Released => Controls.Values.ToSeq().TraverseM(held => Removed(held.Control)).As().Map(static _ => unit);

    public IO<PlacedControls<TControl>> Reconciled(Mount mount) {
        HashMap<Seq<Key>, Hosted.Slot> wanted = toHashMap(mount.Hosted.Choose(static hosted => hosted.Switch(slot: static slot => Some(slot), backdrop: static _ => Option<Hosted.Slot>.None)).Map(static slot => (slot.Path, slot)));
        return toTrackingHashMap(Controls.Map(static held => held.Slot).AsIterable().Map(static entry => (entry.Key, entry.Value))).Snapshot()
            .RemoveRange(Controls.Keys.Filter(path => !wanted.ContainsKey(path)))
            .AddOrUpdateRange(wanted.AsIterable().Map(static entry => (entry.Key, entry.Value)))
            .Changes.AsIterable().ToSeq()
            .Fold(IO.pure(this), (sequence, change) => sequence.Bind(placed => placed.Changed(change.Key, change.Value, mount)));
    }

    private IO<PlacedControls<TControl>> Changed(Seq<Key> path, Change<Hosted.Slot> change, Mount mount) =>
        (change, Controls.Find(path).Map(static held => held.Control).Case) switch {
            (EntryAdded<Hosted.Slot> added, _) => Added(added.Value, Placed(added.Value), Access.Flattened(mount.Tree).Find(element => element.Id == path))
                .Map(control => this with { Controls = Controls.AddOrUpdate(path, new PlacedControl<TControl>(added.Value, control)) }),
            (EntryMappedTo<Hosted.Slot> mapped, TControl control) => Moved(control, mapped.To, Placed(mapped.To)).Map(moved => this with { Controls = Controls.AddOrUpdate(path, new PlacedControl<TControl>(mapped.To, moved)) }),
            (EntryRemoved<Hosted.Slot>, TControl control) => Removed(control).Map(_ => this with { Controls = Controls.Remove(path) }),
            _ => IO.pure(this),
        };

    private static SlotFrame Placed(Hosted.Slot slot) =>
        new(SKRectI.Ceiling(slot.Clip, outwards: true), SKRectI.Round(SKRect.Create(slot.Frame.Location - slot.Clip.Location, slot.Frame.Size)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SceneHost<TPixels> : IDisposable where TPixels : IDisposable {
    // --- [STATE]
    private sealed record Raster(SceneTarget<TPixels> Target, PaintCache Cache, float Scale);

    private sealed record Hosting(Host Host, DefaultStyles Defaults);

    private readonly SceneBackend<TPixels> backend;
    private readonly TimeProvider clock;
    private readonly ITimer timer;
    private readonly SKRegion damage = new();
    private readonly Shaping shaping = new();
    private Option<Node> view;
    private Option<Hosting> hosting;
    private Option<Raster> raster;
    private Option<Func<bool, IO<Unit>>> running;
    private Option<SynchronizationContext> timerContext;

    public SceneHost(SceneBackend<TPixels> backend, TimeProvider clock) {
        (this.backend, this.clock) = (backend, clock);
        timer = clock.CreateTimer(static held => ((SceneHost<TPixels>)held!).TimerElapsed(), this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public Option<Mount> Current { get; private set; }

    public Atom<float> Peak { get; } = Atom(1f);

    // --- [DERIVED]
    public IO<Duration> Now => IO.lift(() => clock.GetElapsedTime(0L).ToDuration());

    public Option<SKSize> Preferred(SKSize offered) =>
        Current.Map(held => {
            SKSize preferred = held.Scene.Preferred(offered);
            return new SKSize(preferred.Width, float.Min(preferred.Height, offered.Height));
        });

    // --- [SESSION]
    public IO<Unit> Show(Node next) =>
        IO.lift(() => view).Bind(prior => unless(prior.Exists(held => ReferenceEquals(held, next)),
            from stored in IO.lift(() => view = Some(next))
            from shown in Current.Match(
                Some: held => from now in Now from moved in IO.lift(() => held.Show(next, now)) from applied in Applied(moved) select unit,
                None: () => hosting.Match(Some: placed => Created(placed.Host, placed.Defaults, []), None: static () => IO.pure(unit)))
            select unit).As());

    public IO<Unit> Rehost(Host host, DefaultStyles defaults) =>
        IO.lift(() => hosting).Bind(prior => unless(prior == Some(new Hosting(host, defaults)),
            from stored in IO.lift(() => hosting = Some(new Hosting(host, defaults)))
            from moved in Current.Match(
                Some: held => prior.Exists(was => was.Defaults.Equals(defaults))
                    ? from now in Now from applied in Applied(held.Rehost(host, now)) select unit
                    : Created(host, defaults, held.Scene.Adjusted),
                None: () => Created(host, defaults, []))
            select unit).As());

    public IO<bool> Receive(Input input, Duration at) =>
        IO.lift(() => Current).Bind(held => held.Match(Some: current => Applied(current.Receive(input, at)), None: static () => IO.pure(value: false)));

    public IO<Unit> Released =>
        IO.lift(() => {
            Option<Func<bool, IO<Unit>>> held = running;
            _ = Cleared();
            return held;
        }).Bind(static held => held.Traverse(static run => run(false)).As()).Map(static _ => unit);

    private IO<Unit> Created(Host host, DefaultStyles defaults, HashMap<Seq<Key>, float> adjusted) =>
        view.Match(
            Some: node => from made in IO.lift(() => Mount.Create(host, defaults, node, adjusted, shaping)) from applied in Applied(made) select unit,
            None: static () => IO.pure(unit));

    private IO<Unit> Tick(Duration now) =>
        IO.lift(() => Current).Bind(held => held.Traverse(current => Applied(current.Tick(now))).As()).Map(static _ => unit);

    // --- [OUTPUT]
    private IO<bool> Applied(Mounted moved) =>
        from stored in IO.lift(() => {
            _ = Damaged(moved.Output.Damage);
            Current = Some(moved.Mount);
            return unit;
        })
        from invalidated in unless(moved.Output.Damage.IsEmpty, backend.Invalidated(moved.Output.Damage)).As()
        from linked in Linked(moved.Mount)
        from armed in Scheduled(moved.Mount.Animations.Due)
        from applied in backend.Applied(moved.Mount, moved.Output)
        from answered in moved.Output.Effects.IfNone(IO.pure(unit))
        select moved.Output.Effects.IsSome;

    private Unit Damaged(Seq<SKRect> rects) =>
        raster.Iter(held => rects.Iter(rect => damage.Op(SKRectI.Ceiling(SKMatrix.CreateScale(held.Scale, held.Scale).MapRect(rect), outwards: true), SKRegionOperation.Union)));

    private IO<Unit> Linked(Mount mount) =>
        (running.IsNone && (mount.Animating || mount.Animations.Due.IsSome) ? backend.DisplayLinked(Tick).Map(started => running = Some(started)) : IO.pure(running))
        .Bind(held => held.Traverse(run => run(mount.Animating)).As()).Map(static _ => unit);

    // --- [TIMER]
    private IO<Unit> Scheduled(Option<Duration> deadline) =>
        from context in IO.syncContext
        from now in Now
        from changed in IO.lift(() => {
            timerContext = context;
            return timer.Change(deadline.Map(at => Duration.Max(at - now, Duration.Zero).ToTimeSpan()).IfNone(Timeout.InfiniteTimeSpan), Timeout.InfiniteTimeSpan);
        })
        select unit;

    private void TimerElapsed() {
        using EnvIO environment = EnvIO.New(token: CancellationToken.None, syncContext: timerContext.ValueUnsafe());
        _ = IO.lift(() => running).Bind(static held => held.Traverse(static run => run(true)).As()).Post().Run(environment);
    }

    // --- [PAINT]
    public IO<Option<SceneTarget<TPixels>>> Paint(float scale) =>
        IO.lift(() => Current).Bind(held => held
            .Map(current => (Mount: current, Pixels: new SKSizeI((int)MathF.Ceiling(current.Host.Size.Width * scale), (int)MathF.Ceiling(current.Host.Size.Height * scale))))
            .Filter(static current => current.Pixels is { Width: > 0, Height: > 0 })
            .Match(
                Some: current =>
                    from target in IO.lift(() => Rastered(current.Pixels, scale, current.Mount.Scene.Extended))
                    from now in Now
                    from drawn in IO.lift(() => Drawn(target, current.Mount, now))
                    select Some(target.Target),
                None: static () => IO.pure(Option<SceneTarget<TPixels>>.None)));

    private Fin<Raster> Rastered(SKSizeI pixels, float scale, bool extended) =>
        raster.Filter(held => held.Target.Info.Size == pixels && held.Scale.Equals(scale) && held.Target.Pixels.IsNone == extended)
            .Match(Some: static held => held, None: () => Reallocated(pixels, scale, extended));

    private Fin<Raster> Reallocated(SKSizeI pixels, float scale, bool extended) {
        _ = Cleared(raster);
        raster = None;
        return Allocated(pixels, extended).Map(target => {
            Raster made = new(target, new PaintCache(target.Info, backend.Properties), scale);
            raster = Some(made);
            _ = damage.SetRect(SKRectI.Create(pixels));
            return made;
        });
    }

    private Fin<SceneTarget<TPixels>> Allocated(SKSizeI pixels, bool extended) {
        if (!extended) return backend.Allocated(pixels).Bind(held => Wrapped(held.Pixels, held.Pixmap));
        SKImageInfo info = Capture.HalfFloat(pixels);
        return Capture.Surface(() => SKSurface.Create(info, backend.Properties)).Map(surface => new SceneTarget<TPixels>(surface, info, None));
    }

    private Fin<SceneTarget<TPixels>> Wrapped(TPixels pixels, SKPixmap pixmap) {
        using (pixmap) {
            return Capture.Surface(() => SKSurface.Create(pixmap.Info, pixmap.GetPixels(), pixmap.RowBytes, static (_, held) => ((IDisposable)held).Dispose(), pixels, backend.Properties))
                .Map(surface => new SceneTarget<TPixels>(surface, pixmap.Info, Some(pixels)))
                .MapFail(error => {
                    pixels.Dispose();
                    return error;
                });
        }
    }

    private Fin<Unit> Drawn(Raster target, Mount current, Duration now) {
        if (damage.IsEmpty) return unit;
        SKCanvas canvas = target.Target.Surface.Canvas;
        canvas.ResetMatrix();
        canvas.Scale(target.Scale);
        return Painter.Paint(canvas, target.Cache, current, damage, now).Map(_ => {
            damage.SetEmpty();
            return unit;
        });
    }

    // --- [RELEASE]
    private Unit Cleared() {
        _ = Cleared(raster);
        (raster, Current, hosting, running) = (None, None, None, None);
        damage.SetEmpty();
        _ = timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        return unit;
    }

    private static Unit Cleared(Option<Raster> held) =>
        held.Iter(static current => {
            current.Target.Dispose();
            current.Cache.Dispose();
        });

    public void Dispose() {
        _ = Cleared();
        timer.Dispose();
        damage.Dispose();
        shaping.Dispose();
    }
}
