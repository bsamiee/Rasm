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

    public static Visibility Of(bool shown, bool hasKeyFocus) => !shown ? Hidden : hasKeyFocus ? Active : Shown;
}

public sealed record HostPlacement(Role Root) {
    public static HostPlacement Window { get; } = new(Role.Window) { Cycles = true };
    public static HostPlacement Panel { get; } = new(Role.Panel);

    public bool Cycles { get; init; }
}

[SmartEnum]
public sealed partial class Blending {
    public static readonly Blending WithinWindow = new(static _ => true);
    public static readonly Blending BehindWindow = new(static overSlot => !overSlot);
    public static readonly Blending Raster = new(static _ => false);

    [UseDelegateFromConstructor]
    internal partial bool Native(bool overSlot);
}

public sealed record Host(SKSize Size, ColorScheme Theme, Visibility Visibility, HostPlacement Placement, CultureInfo Culture) {
    public Blending Blending { get; init; } = Blending.Raster;
    public Factor Headroom { get; init; } = Factor.Unit;
}

public sealed record SceneTarget(SKSurface Surface, SKImageInfo Info) : IDisposable {
    public void Dispose() => Surface.Dispose();
}

public sealed record SlotFrame(SKRectI Clip, SKRectI Frame);

public sealed record SceneBackend(
    Func<SKImageInfo, Fin<SceneTarget>> Allocated,
    Func<Seq<SKRectI>, IO<Unit>> Invalidated,
    Func<Func<Duration, IO<Unit>>, IO<Func<bool, IO<Unit>>>> DisplayLinked) {
    public Func<SceneTarget, SKImageInfo, Option<SKSurface>> Offscreen { get; init; } = static (target, info) =>
        Optional(target.Surface.Context).Match(
            Some: context => Optional(SKSurface.Create(context, budgeted: true, info, target.Surface.SurfaceProperties)),
            None: () => Optional(SKSurface.Create(info, target.Surface.SurfaceProperties)));
    public Func<Mount, Output, IO<Unit>> Applied { get; init; } = static (_, _) => IO.pure(unit);
    public Option<Func<Transfer, SKPoint, IO<Unit>>> Lifted { get; init; }
}

public sealed record PlacedControls<TControl>(
    Func<Hosted.Slot, SlotFrame, Option<Element>, IO<TControl>> Added,
    Func<TControl, Hosted.Slot, SlotFrame, IO<TControl>> Moved,
    Func<TControl, IO<Unit>> Removed) where TControl : notnull {
    public HashMap<Seq<Key>, (Hosted.Slot Slot, TControl Control)> Controls { get; private init; }

    public IO<Unit> Released => Controls.Values.ToSeq().TraverseM(held => Removed(held.Control)).As().Map(static _ => unit);

    public IO<PlacedControls<TControl>> Reconciled(Mount mount) {
        HashMap<Seq<Key>, Hosted.Slot> wanted = toHashMap(mount.Hosted.Choose(static hosted => Optional(hosted as Hosted.Slot)).Map(static slot => (slot.Path, slot)));
        return toTrackingHashMap(Controls.Map(static held => held.Slot).AsIterable().Map(static entry => (entry.Key, entry.Value))).Snapshot()
            .RemoveRange(Controls.Keys.Filter(path => !wanted.ContainsKey(path)))
            .AddOrUpdateRange(wanted.AsIterable().Map(static entry => (entry.Key, entry.Value)))
            .Changes.AsIterable().ToSeq()
            .Fold(IO.pure(this), (sequence, change) => sequence.Bind(placed => placed.Changed(change.Key, change.Value, mount)));
    }

    private IO<PlacedControls<TControl>> Changed(Seq<Key> path, Change<Hosted.Slot> change, Mount mount) =>
        (change, Controls.Find(path).Map(static held => held.Control).Case) switch {
            (EntryAdded<Hosted.Slot> added, _) => Added(added.Value, Placed(added.Value), mount.Elements.Find(path))
                .Map(control => this with { Controls = Controls.AddOrUpdate(path, (added.Value, control)) }),
            (EntryMappedTo<Hosted.Slot> mapped, TControl control) => Moved(control, mapped.To, Placed(mapped.To)).Map(moved => this with { Controls = Controls.AddOrUpdate(path, (mapped.To, moved)) }),
            (EntryRemoved<Hosted.Slot>, TControl control) => Removed(control).Map(_ => this with { Controls = Controls.Remove(path) }),
            _ => IO.pure(this),
        };

    private static SlotFrame Placed(Hosted.Slot slot) =>
        new(SKRectI.Ceiling(slot.Clip, outwards: true), SKRectI.Round(SKRect.Create(slot.Frame.Location - slot.Clip.Location, slot.Frame.Size)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SceneHost : IDisposable {
    // --- [STATE]
    private sealed record Raster(SceneTarget Target, PaintCache Cache, float Scale, bool Extended);

    private sealed record Hosting(Host Host, DefaultStyles Defaults);

    private sealed record Session {
        internal static Session Empty { get; } = new();

        internal Option<Node> View { get; init; }
        internal Option<Hosting> Hosting { get; init; }
        internal Option<Mount> Current { get; init; }
        internal Option<Func<bool, IO<Unit>>> Running { get; init; }
        internal Option<SynchronizationContext> Context { get; init; }
        internal Option<Fin<Mounted>> Moved { get; init; }

        internal Session Stepped(Option<Fin<Mounted>> moved) => this with { Moved = moved, Current = moved.Bind(static held => held.ToOption()).Map(static held => held.Mount) | Current };
    }

    private readonly SceneBackend backend;
    private readonly TimeProvider clock;
    private readonly ITimer timer;
    private readonly SKRegion damage = new();
    private readonly Shaping shaping;
    private readonly Atom<Session> session = Atom(Session.Empty);
    private Option<Raster> raster;

    public SceneHost(SceneBackend backend, TimeProvider clock, Seq<SKTypeface> embedded = default) {
        (this.backend, this.clock) = (backend, clock);
        shaping = new Shaping(embedded);
        timer = clock.CreateTimer(static held => ((SceneHost)held!).TimerElapsed(), this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public Option<Mount> Current => session.Value.Current;

    // --- [DERIVED]
    public IO<Duration> Now => IO.lift(() => clock.GetElapsedTime(0L).ToDuration());

    public Option<SKSize> Preferred(SKSize offered) =>
        Current.Map(held => {
            SKSize preferred = held.Scene.Preferred(offered);
            return new SKSize(preferred.Width, float.Min(preferred.Height, offered.Height));
        });

    private Seq<Seq<Key>> Animated => raster.Map(static held => held.Cache.Animated).IfNone([]);

    // --- [SESSION]
    public IO<Unit> Show(Node next) =>
        from now in Now
        from swapped in session.SwapIO(held => held.View.Exists(prior => ReferenceEquals(prior, next))
            ? held with { Moved = None }
            : (held with { View = Some(next) }).Stepped(held.Current.Map(current => current.Show(next, now)) | held.Hosting.Map(placed => Mount.Create(placed.Host, placed.Defaults, next, [], shaping))))
        from applied in Applied(swapped)
        select unit;

    public IO<Unit> Rehost(Host host, DefaultStyles defaults) =>
        from now in Now
        from swapped in session.SwapIO(held => held.Hosting == Some(new Hosting(host, defaults))
            ? held with { Moved = None }
            : (held with { Hosting = Some(new Hosting(host, defaults)) }).Stepped(held.Current.Match(
                Some: current => held.Hosting.Exists(was => was.Defaults.Equals(defaults))
                    ? Some<Fin<Mounted>>(current.Rehost(host, now))
                    : held.View.Map(view => Mount.Create(host, defaults, view, current.Scene.Adjusted, shaping)),
                None: () => held.View.Map(view => Mount.Create(host, defaults, view, [], shaping)))))
        from applied in Applied(swapped)
        select unit;

    public IO<bool> Receive(Input input, Duration at) =>
        session.SwapIO(held => held.Stepped(held.Current.Map(current => (Fin<Mounted>)current.Receive(input, at)))).Bind(Applied);

    public IO<Unit> Released =>
        from held in session.ValueIO
        from cleared in IO.lift(Cleared)
        from stopped in held.Running.Traverse(static run => run(false)).As()
        select unit;

    private IO<Unit> Tick(Duration now) =>
        from animated in IO.lift(() => Animated)
        from swapped in session.SwapIO(held => held.Stepped(held.Current.Map(current => (Fin<Mounted>)current.Tick(now, animated))))
        from applied in Applied(swapped)
        select unit;

    // --- [OUTPUT]
    private IO<bool> Applied(Session swapped) => swapped.Moved.Match(Some: moved => IO.lift(moved).Bind(mounted => Emitted(swapped.Running, mounted)), None: static () => IO.pure(value: false));

    private IO<bool> Emitted(Option<Func<bool, IO<Unit>>> running, Mounted moved) =>
        from damaged in IO.lift(() => Damaged(moved.Output.Damage))
        from invalidated in unless(moved.Output.Damage.IsEmpty, backend.Invalidated(Regioned(moved.Output.Damage))).As()
        from linked in Linked(running, moved.Mount)
        from armed in Scheduled(moved.Mount.Animations.Due)
        from applied in backend.Applied(moved.Mount, moved.Output)
        from answered in moved.Output.Effects.IfNone(IO.pure(unit))
        from lifted in (from held in moved.Output.Lifted from export in backend.Lifted select held.Transfer.Bind(transfer => export(transfer, held.At))).IfNone(IO.pure(unit))
        select moved.Output.Effects.IsSome;

    private Unit Damaged(Seq<SKRect> rects) =>
        raster.Iter(held => rects.Iter(rect => damage.Op(SKRectI.Ceiling(SKMatrix.CreateScale(held.Scale, held.Scale).MapRect(rect), outwards: true), SKRegionOperation.Union)));

    private static Seq<SKRectI> Regioned(Seq<SKRect> rects) {
        using SKRegion region = new();
        _ = rects.Iter(rect => region.Op(SKRectI.Ceiling(rect, outwards: true), SKRegionOperation.Union));
        using SKRegion.RectIterator walk = region.CreateRectIterator();
        Seq<SKRectI> found = [];
        while (walk.Next(out SKRectI rect)) found = found.Add(rect);
        return found;
    }

    private IO<Unit> Linked(Option<Func<bool, IO<Unit>>> held, Mount mount) =>
        from animated in IO.lift(() => Animated)
        let animating = mount.Animating(animated)
        from running in held.Match(
            Some: static run => IO.pure(Some(run)),
            None: () => animating || mount.Animations.Due.IsSome
                ? backend.DisplayLinked(Tick).Bind(started => session.SwapIO(current => current with { Running = current.Running | Some(started) })).Map(static current => current.Running)
                : IO.pure(Option<Func<bool, IO<Unit>>>.None))
        from ran in running.Traverse(run => run(animating)).As()
        select unit;

    // --- [TIMER]
    private IO<Unit> Scheduled(Option<Duration> deadline) =>
        from context in IO.syncContext
        from now in Now
        from stored in session.SwapIO(held => held with { Context = context })
        from changed in IO.lift(() => timer.Change(deadline.Map(at => Duration.Max(at - now, Duration.Zero).ToTimeSpan()).IfNone(Timeout.InfiniteTimeSpan), period: Timeout.InfiniteTimeSpan))
        select unit;

    private void TimerElapsed() {
        Session held = session.Value;
        using EnvIO environment = EnvIO.New(token: CancellationToken.None, syncContext: held.Context.ValueUnsafe());
        _ = held.Running.Traverse(static run => run(true)).As().Post().Run(environment);
    }

    // --- [PAINT]
    public IO<Option<SceneTarget>> Paint(float scale) =>
        session.ValueIO.Bind(held => held.Current
            .Map(current => (Mount: current, Pixels: SKRectI.Ceiling(SKRect.Create(current.Host.Size.Width * scale, current.Host.Size.Height * scale)).Size))
            .Filter(static current => current.Pixels is { Width: > 0, Height: > 0 })
            .Match(
                Some: current =>
                    from target in IO.lift(() => Rastered(current.Pixels, scale, current.Mount.Scene.Extended || (float)current.Mount.Host.Headroom > 1f))
                    from now in Now
                    from drawn in IO.lift(() => Drawn(target, current.Mount, now))
                    select Some(target.Target),
                None: static () => IO.pure(Option<SceneTarget>.None)));

    private Fin<Raster> Rastered(SKSizeI pixels, float scale, bool extended) =>
        raster.Filter(held => held.Target.Info.Size == pixels && held.Scale.Equals(scale) && held.Extended == extended)
            .Match(Some: static held => held, None: () => Reallocated(pixels, scale, extended));

    private Fin<Raster> Reallocated(SKSizeI pixels, float scale, bool extended) {
        _ = Cleared(raster);
        Fin<Raster> made = backend.Allocated(Capture.Info(pixels, extended))
            .Map(target => new Raster(target, new PaintCache(target.Info, target.Surface.SurfaceProperties, shaping, info => backend.Offscreen(target, info)), scale, extended));
        raster = made.ToOption();
        _ = damage.SetRect(SKRectI.Create(pixels));
        return made;
    }

    private Fin<Unit> Drawn(Raster target, Mount current, Duration now) {
        if (damage.IsEmpty) return unit;
        SKCanvas canvas = target.Target.Surface.Canvas;
        canvas.ResetMatrix();
        canvas.Scale(target.Scale);
        return Painter.Paint(canvas, target.Cache, current, damage, now).Map(_ => {
            target.Target.Surface.Flush(submit: true);
            damage.SetEmpty();
            return unit;
        });
    }

    // --- [RELEASE]
    private Unit Cleared() {
        _ = Cleared(raster);
        raster = None;
        _ = session.Swap(static _ => Session.Empty);
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
