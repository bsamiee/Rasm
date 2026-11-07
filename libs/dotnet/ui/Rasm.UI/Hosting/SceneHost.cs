using Rasm.UI.Input;
using Rasm.UI.Rendering;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Hosting;

// --- [SERVICES] ------------------------------------------------------------------------

public sealed class SceneHost : IAsyncDisposable {

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

        internal Option<View> View { get; init; }
        internal Option<(Host Host, StyleSheet Sheet)> Hosting { get; init; }
        internal Option<Mount> Current { get; init; }
        internal Option<Func<bool, IO<Unit>>> Running { get; init; }
        internal Option<SynchronizationContext> Context { get; init; }

        internal (Session Next, Option<Fin<Mount>> Moved) Stepped(Option<Fin<Mount>> moved) => (this with { Current = moved.Bind(static held => held.ToOption()) || Current }, moved);
    }

    private readonly SceneBackend backend;
    private readonly TimeProvider clock;
    private readonly ITimer timer;
    private readonly Shaping shaping;
    private readonly Lock gate = new();
    private Session session = Session.Empty;
    private HashMap<Option<Seq<Key>>, Raster> rasters;

    public SceneHost(SceneBackend backend, TimeProvider clock, Seq<SKTypeface> embedded = default) {
        (this.backend, this.clock) = (backend, clock);
        shaping = new Shaping(embedded);
        timer = clock.CreateTimer(static held => ((SceneHost)held!).TimerElapsed(), this, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }

    public Option<Mount> Current => Volatile.Read(ref session).Current;

    // --- [DERIVED]

    public IO<Duration> Now => IO.lift(() => clock.GetElapsedTime(0L).ToDuration());

    public Option<SKSize> Preferred(SKSize offered) => Current.Map(held => held.Scene.Preferred(offered));

    private Seq<Seq<Key>> Animated => toSeq(rasters.Values).Bind(static held => held.Cache.Animated).Distinct();

    // --- [SESSIONS]

    public IO<Unit> Show(View next) =>
        Changed(held => held.View.Exists(prior => ReferenceEquals(prior, next)) ? None : Some(held with { View = Some(next) }), (current, now) => current.Show(next, now));

    public IO<Unit> Rehost(Host host, StyleSheet sheet) =>
        Changed(held => held.Hosting == Some((Host: host, Sheet: sheet)) ? None : Some(held with { Hosting = Some((Host: host, Sheet: sheet)) }), (current, now) => current.Rehost(host, sheet, now));

    public IO<bool> Receive(InputEvent input, Duration at) =>
        Applied(held => held.Stepped(held.Current.Map(current => (Fin<Mount>)current.Receive(input, at))));

    public IO<Unit> Released =>
        from held in IO.lift(Taken)
        from cleared in IO.lift(() => Cleared(held))
        from stopped in held.Running.Traverse(static run => run(false)).As()
        select unit;

    private IO<Unit> Changed(Func<Session, Option<Session>> changed, Func<Mount, Duration, Fin<Mount>> step) =>
        from now in Now
        from applied in Applied(held => changed(held).Match(
            Some: next => next.Stepped(next.Current.Map(current => step(current, now)) || (from placed in next.Hosting from view in next.View select Mount.Create(placed.Host, placed.Sheet, view, shaping))),
            None: () => (held, Option<Fin<Mount>>.None)))
        select unit;

    private IO<Unit> Tick(Duration now) =>
        from animated in IO.lift(() => Animated)
        from applied in Applied(held => held.Stepped(held.Current.Map(current => (Fin<Mount>)current.Tick(now, animated))))
        select unit;

    // --- [OUTPUTS]

    private IO<bool> Applied(Func<Session, (Session Next, Option<Fin<Mount>> Moved)> step) =>
        IO.lift(() => Stored(held => step(held) switch { var (next, moved) => (moved.Exists(static changed => changed.IsFail) ? held : next, (next.Running, Moved: moved)) }))
            .Bind(stepped => stepped.Moved.Match(Some: moved => IO.lift(moved).Bind(mount => Emitted(stepped.Running, mount)), None: static () => IO.pure(value: false)));

    private IO<bool> Emitted(Option<Func<bool, IO<Unit>>> running, Mount mount) =>
        from damaged in IO.lift(() => Damaged(mount))
        from invalidated in unless(mount.Output.Damage.IsEmpty, backend.Invalidated(mount.Output.Damage.Map(static rect => SKRectI.Ceiling(rect, outwards: true)).Distinct())).As()
        from linked in Linked(running, mount)
        from armed in Scheduled(mount.Timers.Due)
        from applied in backend.Applied(mount)
        from answered in mount.Output.Effects.IfNone([]).Iter(static effect => effect).As()
        from lifted in (from held in mount.Output.Lifted from export in backend.Lifted select held.Transfer.Bind(transfer => export(transfer, held.At))).IfNone(IO.pure(unit))
        select mount.Output.Effects.IsSome;

    private Unit Damaged(Mount mount) {
        _ = rasters.Filter((popup, _) => popup.Exists(path => !mount.Scene.Popups.Contains(path))).Iter(static (_, held) => held.Dispose());
        rasters = rasters.Filter((popup, _) => popup.ForAll(mount.Scene.Popups.Contains));
        return toSeq(rasters.Values).Iter(held => held.Damaged(mount.Output.Damage));
    }

    private IO<Unit> Linked(Option<Func<bool, IO<Unit>>> held, Mount mount) =>
        from animated in IO.lift(() => Animated)
        let animating = mount.Animating(animated)
        from running in held.Match(
            Some: static run => IO.pure(Some(run)),
            None: () => animating || mount.Timers.Due.IsSome
                ? backend.DisplayLinked(Tick).Map(started => Stored(current => (current with { Running = current.Running || Some(started) }) switch { var next => (next, next.Running) }))
                : IO.pure(Option<Func<bool, IO<Unit>>>.None))
        from ran in running.Traverse(run => run(animating)).As()
        select unit;

    // --- [TIMER]

    private IO<Unit> Scheduled(Option<Duration> deadline) =>
        from context in IO.syncContext
        from now in Now
        from stored in IO.lift(() => Stored(held => (held with { Context = context }, unit)))
        from changed in IO.lift(() => timer.Change(deadline.Map(at => Duration.Max(at - now, Duration.Zero).ToTimeSpan()).IfNone(Timeout.InfiniteTimeSpan), period: Timeout.InfiniteTimeSpan))
        select unit;

    private void TimerElapsed() {
        Session held = Volatile.Read(ref session);
        using EnvIO environment = EnvIO.New(token: CancellationToken.None, syncContext: held.Context.ValueUnsafe());
        _ = IO.lift(() => Volatile.Read(ref session).Running).Bind(static running => running.Traverse(static run => run(true)).As()).Post().Run(environment);
    }

    // --- [DRAWING]

    public IO<Option<SKSurface>> Paint(float scale, Option<Seq<Key>> popup = default) =>
        from held in IO.lift(() => Volatile.Read(ref session))
        from now in Now
        from drawn in IO.lift((from current in held.Current
                               from frame in popup.Match(
                                   Some: path => current.Hosted.Find(hosted => hosted is Hosted.Popup && hosted.Path == path).Map(static hosted => hosted.Frame),
                                   None: Some(SKRect.Create(current.Host.Size)))
                               let info = Capture.Info(frame.Size, scale, current.Scene.Extended)
                               where info.Width > 0 && info.Height > 0
                               select (Mount: current, Origin: frame.Location, Info: info))
            .Traverse(target => Rastered(popup, target.Info, scale, target.Origin).Bind(raster => raster.Drawn(target.Mount, popup, now))).As())
        from linked in held.Current.Traverse(current => Linked(held.Running, current)).As()
        select drawn;

    private Fin<Raster> Rastered(Option<Seq<Key>> popup, SKImageInfo info, float scale, SKPoint origin) {
        Option<Raster> prior = rasters.Find(popup);
        return prior.Filter(held => held.Fits(info, scale, origin)).Match(Some: static held => held, None: () => {
            _ = prior.Iter(static held => held.Dispose());
            Fin<Raster> made = backend.Allocated(info).Map(surface => new Raster(surface, info, new PaintCache(info, surface.SurfaceProperties, shaping, Optional(surface.Context)), scale, origin));
            rasters = made.Map(held => rasters.AddOrUpdate(popup, held)).IfFail(rasters.Remove(popup));
            return made;
        });
    }

    // --- [RELEASE]

    private T Stored<T>(Func<Session, (Session Next, T Result)> change) {
        lock (gate) {
            (Session next, T result) = change(session);
            Volatile.Write(ref session, next);
            return result;
        }
    }

    private Session Taken() => Stored(static held => (Session.Empty, held));

    private Unit Cleared(Session held) {
        _ = held.Current.Iter(mount => {
            _ = timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            mount.Scene.Outlines.Dispose();
        });
        _ = toSeq(rasters.Values).Iter(static raster => raster.Dispose());
        rasters = [];
        return unit;
    }

    public async ValueTask DisposeAsync() {
        using (shaping)
        await using (timer.ConfigureAwait(false))
            _ = await Released.RunAsync().ConfigureAwait(false);
    }
}
