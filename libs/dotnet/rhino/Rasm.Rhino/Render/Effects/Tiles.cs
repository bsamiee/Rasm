using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.Render.Presets;
using Rasm.Rhino.Render.Sessions;
using Rasm.Rhino.Render.UI;
using Rasm.Rhino.UI;
using SkiaSharp;
using UnitsNet;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
internal sealed partial class TileRank {
    public static readonly TileRank Current = new(viewed: true);
    public static readonly TileRank Visible = new(viewed: true);
    public static readonly TileRank Rest = new(viewed: false);

    public bool Viewed { get; }
}

public sealed record TileLimits(int Batch, Duration Budget, int Capacity) {
    public static TileLimits Default { get; } = new(2, Duration.FromMilliseconds(250), 256);
}

public sealed record TileLimitRows(PlugInSetting<int, int> Batch, PlugInSetting<int, int> Budget, PlugInSetting<int, int> Capacity) {
    public IO<TileLimits> Limits(ISettingReader reader) =>
        IO.lift(() => (reader.Current(Batch), reader.Current(Budget), reader.Current(Capacity))
            .Apply(static (batch, budget, capacity) => new TileLimits(batch, Duration.FromMilliseconds(budget), capacity))
            .As());
}

public sealed record TileRequest(Rendering Rendering, ValueSet Set, Seq<EffectKind> Chain) {
    public static TileRequest Recorded(Rendering rendering, Seq<EffectKind> kinds) =>
        new(rendering, rendering.Grading, rendering.Running(kinds));

    public static Fin<TileRequest> Applied(Rendering rendering, Preset preset, ValueSet current, LanguageExt.HashSet<EntryKey> locks, Seq<EffectKind> chain) =>
        preset.Values.Map(values => new TileRequest(rendering, values.Over(current, locks), chain));

    public (ContentKey Key, PixelExtent Tile) Keyed(PixelExtent asked) =>
        Rendering.Extent.Fitted(asked) switch {
            var tile => (
                ContentKey.Of(KeyDomain.Tile, stream => stream
                    .Integer<UInt128>(Rendering.Key)
                    .Rows(Chain, static (row, kind) => row.Id(kind.Id))
                    .Rows(toSeq(Set.Entries.AsIterable().OrderBy(static entry => entry.Key)), static (row, entry) =>
                        row.Text(entry.Key.Owner).Rows(entry.Key.Path, static (field, segment) => field.Text(segment)).Text(entry.Value))
                    .Integer(tile.Width)
                    .Integer(tile.Height)),
                tile),
        };
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PreviewTiles {
    // --- [ACQUISITION]
    private static readonly Duration Tick = Duration.FromMilliseconds(150);

    private readonly Atom<Ledger> held = Atom(Ledger.Empty);
    private readonly Atom<TileLimits> bounds = Atom(TileLimits.Default);
    private readonly Ref<(int Count, Duration Spent, int Running)> usage = Ref((0, Duration.Zero, 0));
    private readonly Derivations derivations;
    private readonly IPlugInSink owner;
    private readonly TimeProvider clock;
    private event EventHandler<EventArgs>? landed;

    private PreviewTiles(IPlugInSink owner, TimeProvider clock, Derivations derivations) =>
        (this.owner, this.clock, this.derivations) = (owner, clock, derivations);

    public static PlugInSlot<PreviewTiles> Current { get; } = new();

    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Register(Option<TileLimitRows> limits, Func<PreviewTiles, Seq<Func<IPlugInViews, Action<IDisposable>, IO<Unit>>>> readers) =>
        (sink, retain) => DisposalOps.Acquire(owned =>
            from tiles in Open(sink, sink.Clock, limits.Map(rows => (rows.Limits(sink.Settings), SettingRoots.Saved(sink.Settings))), owned)
            from acquired in readers(tiles).TraverseM(row => row(sink, owned)).As()
            from served in Current.Publish(sink, tiles)
            select unit, retain);

    public static IO<PreviewTiles> Open(IPlugInSink owner, TimeProvider clock, Option<(IO<TileLimits> Read, HostEvent<Unit> Changed)> limits, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from derivations in DisposalOps.Acquire(IO.lift(static () => new Derivations()), owned)
            from tiles in IO.lift(() => new PreviewTiles(owner, clock, derivations))
            from watched in limits.TraverseM(source =>
                from initial in source.Read.Bind(tiles.Limit)
                from saved in source.Changed.Choose(static _ => Some((unit, unit)))
                    .Through(Subscriptions.Idle<Unit, Unit>(_ => source.Read.Bind(tiles.Limit)), owner, owned)
                select unit).As()
            from worker in DisposalOps.Fork(tiles.Ticked.Repeat(Schedule.spaced(Tick.ToTimeSpan())), owned)
            select tiles, retain);

    private IO<Unit> Limit(TileLimits current) => writeIO(bounds, current).Map(unit);

    // --- [ASKS]
    public TileSource Source(IO<Option<TileRequest>> request, IO<Option<string>> file) =>
        new(request.Map(static asked => asked.Map(static wanted => wanted.Rendering.Extent)),
            extent => request.Bind(asked => asked.Match(
                Some: wanted => Asked(wanted, extent, TileRank.Current).Map(static found => found.Map(static pixels => pixels.Frame)),
                None: static () => IO.pure(Option<PixelFrame>.None))),
            EffectPipeline.Display,
            file,
            Landed);

    internal IO<(PixelFrame Frame, PixelFrame Proxy)> Formed(TileRequest request, PixelExtent extent, TileRank rank) =>
        Monad.recur(unit, _ => Asked(request, extent, rank).Bind(static found => found.Match(
            Some: static tile => IO.pure(Next.Done<Unit, (PixelFrame Frame, PixelFrame Proxy)>(tile)),
            None: static () => IO.yieldFor(Tick.ToTimeSpan()).Map(static _ => Next.Loop<Unit, (PixelFrame Frame, PixelFrame Proxy)>(unit))))).As();

    public IO<SKImage> Thumbnail(TileRequest request, PixelExtent extent, float headroom) =>
        from tile in Formed(request, extent, TileRank.Visible)
        from image in PreviewRows.Image(tile.Frame, EffectPipeline.Display.Encoding.Transfer, EffectPipeline.Display, headroom)
        select image;

    public IO<Unit> Refresh =>
        from now in IO.lift(clock.GetTimestamp)
        from _ in held.SwapIO(state => state.Refreshed(now))
        select unit;

    public long Bytes => held.Value.Bytes;

    public HostEvent<Unit> Landed =>
        Subscriptions.Host<EventArgs>(typeof(PreviewTiles), handler => landed += handler, handler => landed -= handler).Choose(static _ => Some(unit));

    public IO<Option<SKImage>> Image(PixelFrame frame, PixelExtent extent, Transfer encoding, float headroom) =>
        Requested(IO.lift(() => frame.Downscaled(frame.Size.Fitted(extent))).Bind(reduced => PreviewRows.Image(reduced, encoding, EffectPipeline.Display, headroom)));

    private IO<Option<(PixelFrame Frame, PixelFrame Proxy)>> Asked(TileRequest request, PixelExtent asked, TileRank rank) =>
        from now in IO.lift(clock.GetTimestamp)
        let keyed = request.Keyed(asked)
        from ledger in held.SwapIO(state => state.Asked(keyed.Key, request, keyed.Tile, rank, now))
        from tile in ledger.Tiles.Find(keyed.Key).Traverse(kept => IO.lift(kept.Result.Map(frame => (Frame: frame, Proxy: ledger.Proxies[request.Rendering.Key])))).As()
        select tile;

    // --- [FORMATION]
    private IO<Unit> Ticked =>
        (from reset in atomicIO(() => usage.Swap(static current => current.Running is 0 ? (0, Duration.Zero, 0) : current))
         from _ in Monad.recur(unit, _ => Step).As()
         from limits in bounds.ValueIO
         from now in IO.lift(clock.GetTimestamp)
         from __ in held.SwapIO(state => state.Evicted(limits.Capacity, Recent(now)))
         select unit)
        .Catch(static error => !error.Is(Errors.Cancelled), Reported);

    private IO<Next<Unit, Unit>> Step =>
        from now in IO.lift(clock.GetTimestamp)
        from ledger in held.SwapIO(state => state.Popped(Recent(now)))
        from next in ledger.Running.Match(
            Some: running => Requested(Land(running, ledger.Proxies)).Bind(result => result.Match(
                Some: static _ => IO.pure(Next.Loop<Unit, Unit>(unit)),
                None: () => held.SwapIO(state => state with { Queue = running.Cons(state.Queue), Running = None })
                    .Map(static _ => Next.Done<Unit, Unit>(unit)))),
            None: static () => IO.pure(Next.Done<Unit, Unit>(unit)))
        select next;

    private IO<Option<T>> Requested<T>(IO<T> request) =>
        DisposalOps.Within(
            () => {
                TileLimits policy = bounds.Value;
                return atomic(() => {
                    (int count, Duration spent, int running) = usage.Value;
                    bool accepted = count < policy.Batch && (count is 0 || spent < policy.Budget);
                    if (accepted) usage.Value = (count + 1, spent, running + 1);
                    return accepted;
                }) ? Some(clock.GetTimestamp()) : Option<long>.None;
            },
            began => began.Match(Some: _ => request.Map(static value => Some(value)), None: static () => IO.pure(Option<T>.None)),
            began => began.Traverse(at => IO.lift(() => clock.GetElapsedTime(at).ToDuration())
                .Bind(elapsed => atomicIO(() => usage.Swap(current => (current.Count, current.Spent + elapsed, current.Running - 1))))).As().Map(unit));

    private IO<Unit> Land(Pending running, HashMap<ContentKey, PixelFrame> proxies) =>
        (from proxy in Proxied(running, proxies)
         from tile in EffectPipeline.Form(proxy.Downscaled(running.Extent), running.Request.Set, running.Request.Chain, derivations)
         from kept in Keep(running, tile)
         select kept)
        .Catch(static error => !error.Is(Errors.Cancelled), error => Keep(running, error).Bind(_ => Reported(error)));

    private IO<PixelFrame> Proxied(Pending running, HashMap<ContentKey, PixelFrame> proxies) =>
        running.Request.Rendering.Key switch {
            var rendering => proxies.Find(rendering)
                .Filter(proxy => proxy.Size.Width >= running.Extent.Width && proxy.Size.Height >= running.Extent.Height)
                .Match(
                    Some: static proxy => IO.pure(proxy),
                    None: () =>
                        from frame in running.Request.Rendering.Frame
                        let reduced = frame.Downscaled(running.Extent)
                        from _ in held.SwapIO(state => state with { Proxies = state.Proxies.AddOrUpdate(rendering, reduced) })
                        select reduced),
        };

    private IO<Unit> Keep(Pending running, Fin<PixelFrame> result) =>
        from now in IO.lift(clock.GetTimestamp)
        from _ in held.SwapIO(state => state.Landed(running, result, now))
        from raised in IO.lift(() => landed?.Invoke(this, EventArgs.Empty))
        select unit;

    private IO<Unit> Reported(Error error) => IO.lift(() => owner.Report(error, typeof(PreviewTiles), nameof(Register)));

    private Func<long, bool> Recent(long now) => at => clock.GetElapsedTime(at, now).ToDuration() < Tick * 2;

    // --- [LEDGER]
    private sealed record Pending(ContentKey Key, TileRequest Request, PixelExtent Extent, HashMap<TileRank, long> Asks) {
        public bool Holds(TileRank rank, Func<long, bool> recent) => Asks.Find(rank).Exists(at => !rank.Viewed || recent(at));

        public bool Ranked(Func<long, bool> recent) => Asks.Exists((rank, at) => !rank.Viewed || recent(at));
    }

    private sealed record Kept(TileRequest Request, PixelExtent Extent, Fin<PixelFrame> Result, long Drawn);

    private sealed record Ledger(HashMap<ContentKey, PixelFrame> Proxies, Seq<Pending> Queue, Option<Pending> Running, HashMap<ContentKey, Kept> Tiles) {
        public static Ledger Empty { get; } = new(HashMap<ContentKey, PixelFrame>(), Seq<Pending>(), None, HashMap<ContentKey, Kept>());

        public long Bytes =>
            (toSeq(Tiles.Values).Bind(static kept => kept.Result.ToSeq()) + toSeq(Proxies.Values))
                .Fold(0L, static (sum, frame) => sum + (sizeof(float) * (long)frame.Block.Length));

        public Ledger Asked(ContentKey key, TileRequest request, PixelExtent tile, TileRank rank, long now) =>
            this with {
                Tiles = rank.Viewed ? Tiles.TrySetItem(key, kept => kept with { Drawn = now }) : Tiles,
                Queue = Queue.Exists(pending => pending.Key == key)
                    ? Queue.Map(pending => pending.Key == key ? pending with { Asks = pending.Asks.AddOrUpdate(rank, now) } : pending)
                    : Running.Exists(running => running.Key == key) || (rank.Viewed && Tiles.ContainsKey(key))
                        ? Queue
                        : Queue.Add(new Pending(key, request, tile, HashMap((rank, now)))),
            };

        public Ledger Popped(Func<long, bool> recent) =>
            toSeq(TileRank.Items).Choose(rank => Queue.Find(pending => pending.Holds(rank, recent))).Head.Match(
                Some: next => this with { Queue = Queue.Filter(pending => pending.Key != next.Key && pending.Ranked(recent)), Running = Some(next) },
                None: () => this with { Queue = Seq<Pending>(), Running = None });

        public Ledger Landed(Pending running, Fin<PixelFrame> result, long now) =>
            this with { Running = None, Tiles = Tiles.AddOrUpdate(running.Key, new Kept(running.Request, running.Extent, result, now)) };

        public Ledger Evicted(int capacity, Func<long, bool> recent) =>
            this with {
                Tiles = Tiles.Except(Tiles.Filter(kept => !recent(kept.Drawn)).AsIterable()
                    .OrderBy(static entry => entry.Value.Drawn)
                    .ThenBy(static entry => entry.Key)
                    .Take(Tiles.Count - capacity)
                    .Select(static entry => entry.Key)),
            } switch {
                var trimmed => trimmed with {
                    Proxies = trimmed.Proxies.Intersect((trimmed.Queue + trimmed.Running.ToSeq()).Map(static pending => pending.Request.Rendering.Key)
                        + toSeq(trimmed.Tiles.Values).Map(static kept => kept.Request.Rendering.Key)),
                },
            };

        public Ledger Refreshed(long now) =>
            toSeq(Tiles.AsIterable()).Fold(
                this with { Tiles = Tiles.Filter(static kept => kept.Result.IsSucc) },
                (ledger, entry) => ledger.Asked(entry.Key, entry.Value.Request, entry.Value.Extent, TileRank.Rest, now));
    }
}
