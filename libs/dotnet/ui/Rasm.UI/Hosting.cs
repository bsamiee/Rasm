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

[SmartEnum]
public sealed partial class Blending {
    public static readonly Blending WithinWindow = new(static _ => true);
    public static readonly Blending BehindWindow = new(static overSlot => !overSlot);
    public static readonly Blending Raster = new(static _ => false);

    [UseDelegateFromConstructor]
    internal partial bool Native(bool overSlot);
}

public sealed record Host(SKSize Size, Theme Theme, Visibility Visibility, Role Root, CultureInfo Culture) {
    public bool Cycles { get; init; } = Root == Role.Window;
    public Blending Blending { get; init; } = Blending.Raster;
    public Factor Headroom { get; init; } = Factor.Unit;
}

public sealed record SceneBackend(
    Func<SKImageInfo, Fin<SKSurface>> Allocated,
    Func<Seq<SKRectI>, IO<Unit>> Invalidated,
    Func<Func<Duration, IO<Unit>>, IO<Func<bool, IO<Unit>>>> DisplayLinked) {
    public Func<SKSurface, SKImageInfo, Option<SKSurface>> Offscreen { get; init; } = static (surface, info) =>
        Optional(surface.Context is { } context ? SKSurface.Create(context, budgeted: true, info, surface.SurfaceProperties) : SKSurface.Create(info, surface.SurfaceProperties));
    public Func<Mount, IO<Unit>> Applied { get; init; } = static _ => IO.pure(unit);
    public Option<Func<Transfer, SKPoint, IO<Unit>>> Lifted { get; init; }
}

public sealed record PlacedControls<TControl>(
    Func<Hosted.Slot, Option<Element>, IO<TControl>> Added,
    Func<TControl, Hosted.Slot, IO<TControl>> Moved,
    Func<TControl, IO<Unit>> Removed) where TControl : notnull {
    public HashMap<Seq<Key>, (Hosted.Slot Slot, TControl Control)> Controls { get; private init; }

    public IO<Unit> Released => Controls.Values.ToSeq().TraverseM(held => Removed(held.Control)).As().Map(static _ => unit);

    public IO<PlacedControls<TControl>> Reconciled(Mount mount) {
        HashMap<Seq<Key>, Hosted.Slot> wanted = toHashMap(mount.Hosted.Choose(static hosted => Optional(hosted as Hosted.Slot)).Map(static slot => (slot.Path, slot)));
        return toSeq(Controls.Keys).Concat(wanted.Keys).Distinct().Fold(IO.pure(this), (sequence, path) => sequence.Bind(placed => placed.Changed(path, wanted.Find(path), mount)));
    }

    private IO<PlacedControls<TControl>> Changed(Seq<Key> path, Option<Hosted.Slot> wanted, Mount mount) =>
        Controls.Find(path).Match(
            Some: held => wanted.Match(
                Some: slot => held.Slot == slot ? IO.pure(this) : Moved(held.Control, slot).Map(moved => this with { Controls = Controls.AddOrUpdate(path, (slot, moved)) }),
                None: () => Removed(held.Control).Map(_ => this with { Controls = Controls.Remove(path) })),
            None: () => wanted.Match(
                Some: slot => Added(slot, mount.Elements.Find(path)).Map(control => this with { Controls = Controls.AddOrUpdate(path, (slot, control)) }),
                None: () => IO.pure(this)));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SceneHost : IDisposable {
    // --- [STATE]
    private sealed class Raster(SKSurface surface, SKImageInfo info, PaintCache cache, float scale, SKPoint origin) : IDisposable {
        private readonly SKRegion damage = new(info.Rect);
        private readonly SKMatrix device = SKMatrix.CreateScaleTranslation(scale, scale, -origin.X * scale, -origin.Y * scale);

        internal PaintCache Cache => cache;

        internal bool Fits(SKImageInfo wanted, float at, SKPoint from) => info == wanted && scale.Equals(at) && origin == from;

        internal void Damaged(Seq<SKRect> rects) =>
            _ = rects.Iter(rect => damage.Op(SKRectI.Intersect(SKRectI.Ceiling(device.MapRect(rect), outwards: true), info.Rect), SKRegionOperation.Union));

        internal Fin<SKSurface> Drawn(Mount mount, Option<Seq<Key>> popup, Duration now) {
            if (damage.IsEmpty) return surface;
            surface.Canvas.ResetMatrix();
            surface.Canvas.Scale(scale);
            return Painter.Paint(surface.Canvas, cache, mount, damage, now, popup).Map(_ => {
                surface.Flush(submit: true);
                damage.SetEmpty();
                return surface;
            });
        }

        public void Dispose() {
            damage.Dispose();
            cache.Dispose();
            surface.Dispose();
        }
    }

    private sealed record Session {
        internal static Session Empty { get; } = new();

        internal Option<Node> View { get; init; }
        internal Option<(Host Host, StyleSheet Sheet)> Hosting { get; init; }
        internal Option<Mount> Current { get; init; }
        internal Option<Func<bool, IO<Unit>>> Running { get; init; }
        internal Option<SynchronizationContext> Context { get; init; }
        internal Option<Fin<Mount>> Moved { get; init; }

        internal Session Stepped(Option<Fin<Mount>> moved) => this with { Moved = moved, Current = moved.Bind(static held => held.ToOption()) | Current };
    }

    private readonly SceneBackend backend;
    private readonly TimeProvider clock;
    private readonly ITimer timer;
    private readonly Shaping shaping;
    private readonly Atom<Session> session = Atom(Session.Empty);
    private HashMap<Option<Seq<Key>>, Raster> rasters;

    public SceneHost(SceneBackend backend, TimeProvider clock, Seq<SKTypeface> embedded = default) {
        (this.backend, this.clock) = (backend, clock);
        shaping = new Shaping(embedded);
        timer = clock.CreateTimer(static held => ((SceneHost)held!).TimerElapsed(), this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public Option<Mount> Current => session.Value.Current;

    // --- [DERIVED]
    public IO<Duration> Now => IO.lift(() => clock.GetElapsedTime(0L).ToDuration());

    public Option<SKSize> Preferred(SKSize offered) =>
        Current.Map(held => held.Scene.Preferred(offered) switch {
            var preferred => preferred with { Height = float.Min(preferred.Height, offered.Height) },
        });

    private Seq<Seq<Key>> Animated => toSeq(rasters.Values).Bind(static held => held.Cache.Animated).Distinct();

    // --- [SESSION]
    public IO<Unit> Show(Node next) =>
        from now in Now
        from swapped in session.SwapIO(held => held.View.Exists(prior => ReferenceEquals(prior, next))
            ? held with { Moved = None }
            : (held with { View = Some(next) }).Stepped(held.Current.Map(current => current.Show(next, now)) | held.Hosting.Map(placed => Mount.Create(placed.Host, placed.Sheet, next, shaping))))
        from applied in Applied(swapped)
        select unit;

    public IO<Unit> Rehost(Host host, StyleSheet sheet) =>
        from now in Now
        from swapped in session.SwapIO(held => held.Hosting == Some((Host: host, Sheet: sheet))
            ? held with { Moved = None }
            : (held with { Hosting = Some((Host: host, Sheet: sheet)) }).Stepped(held.Current.Map(current => (Fin<Mount>)current.Rehost(host, sheet, now)) | held.View.Map(view => Mount.Create(host, sheet, view, shaping))))
        from applied in Applied(swapped)
        select unit;

    public IO<bool> Receive(Input input, Duration at) =>
        session.SwapIO(held => held.Stepped(held.Current.Map(current => (Fin<Mount>)current.Receive(input, at)))).Bind(Applied);

    public IO<Unit> Released =>
        from held in session.ValueIO
        from cleared in IO.lift(Cleared)
        from stopped in held.Running.Traverse(static run => run(false)).As()
        select unit;

    private IO<Unit> Tick(Duration now) =>
        from animated in IO.lift(() => Animated)
        from swapped in session.SwapIO(held => held.Stepped(held.Current.Map(current => (Fin<Mount>)current.Tick(now, animated))))
        from applied in Applied(swapped)
        select unit;

    // --- [OUTPUT]
    private IO<bool> Applied(Session swapped) => swapped.Moved.Match(Some: moved => IO.lift(moved).Bind(mount => Emitted(swapped.Running, mount)), None: static () => IO.pure(value: false));

    private IO<bool> Emitted(Option<Func<bool, IO<Unit>>> running, Mount mount) =>
        from damaged in IO.lift(() => Damaged(mount))
        from invalidated in unless(mount.Output.Damage.IsEmpty, backend.Invalidated(Regioned(mount.Output.Damage))).As()
        from linked in Linked(running, mount)
        from armed in Scheduled(mount.Animations.Due)
        from applied in backend.Applied(mount)
        from answered in mount.Output.Effects.IfNone(IO.pure(unit))
        from lifted in (from held in mount.Output.Lifted from export in backend.Lifted select held.Transfer.Bind(transfer => export(transfer, held.At))).IfNone(IO.pure(unit))
        select mount.Output.Effects.IsSome;

    private Unit Damaged(Mount mount) {
        _ = rasters.Filter((popup, _) => popup.Exists(path => !mount.Scene.Popups.Contains(path))).Iter(static (_, held) => held.Dispose());
        rasters = rasters.Filter((popup, _) => popup.ForAll(mount.Scene.Popups.Contains));
        return toSeq(rasters.Values).Iter(held => held.Damaged(mount.Output.Damage));
    }

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
    public IO<Option<SKSurface>> Paint(float scale) => Painted(None, scale);

    public IO<Option<SKSurface>> Paint(float scale, Seq<Key> popup) => Painted(Some(popup), scale);

    private IO<Option<SKSurface>> Painted(Option<Seq<Key>> popup, float scale) =>
        from held in session.ValueIO
        from now in Now
        from drawn in IO.lift(() => (from current in held.Current
                                     from frame in popup.Match(
                                         Some: path => current.Hosted.Find(hosted => hosted is Hosted.Popup && hosted.Path == path).Map(static hosted => hosted.Frame),
                                         None: () => Some(SKRect.Create(current.Host.Size)))
                                     let info = Capture.Info(frame.Size, scale, current.Scene.Extended)
                                     where info.Width > 0 && info.Height > 0
                                     select (Mount: current, Origin: frame.Location, Info: info))
            .Traverse(target => Rastered(popup, target.Info, scale, target.Origin).Bind(raster => raster.Drawn(target.Mount, popup, now))).As())
        select drawn;

    private Fin<Raster> Rastered(Option<Seq<Key>> popup, SKImageInfo info, float scale, SKPoint origin) =>
        rasters.Find(popup).Filter(held => held.Fits(info, scale, origin)).Match(Some: static held => held, None: () => {
            _ = rasters.Find(popup).Iter(static held => held.Dispose());
            Fin<Raster> made = backend.Allocated(info).Map(surface => new Raster(surface, info, new PaintCache(info, surface.SurfaceProperties, shaping, wanted => backend.Offscreen(surface, wanted)), scale, origin));
            rasters = made.Map(held => rasters.AddOrUpdate(popup, held)).IfFail(rasters.Remove(popup));
            return made;
        });

    // --- [RELEASE]
    private Unit Cleared() {
        _ = session.Value.Current.Iter(static held => held.Scene.Outlines.Dispose());
        _ = toSeq(rasters.Values).Iter(static held => held.Dispose());
        rasters = [];
        _ = session.Swap(static _ => Session.Empty);
        _ = timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        return unit;
    }

    public void Dispose() {
        _ = Cleared();
        timer.Dispose();
        shaping.Dispose();
    }
}
