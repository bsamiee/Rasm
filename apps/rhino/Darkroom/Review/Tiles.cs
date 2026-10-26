using System.Runtime.CompilerServices;
using Darkroom.Chain;
using Rasm.Imaging.Pixels;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI;
using SkiaSharp;
using UnitsNet;

namespace Darkroom.Review;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class TileRank {
    public static readonly TileRank Current = new(shown: true);
    public static readonly TileRank Visible = new(shown: true);
    public static readonly TileRank Rest = new(shown: false);

    public bool Shown { get; }
}

public sealed record TileLimits(int Batch, TimeSpan Budget, int Capacity) {
    public static TileLimits Default { get; } = new(2, TimeSpan.FromMilliseconds(250), 256);
}

public sealed record TileRequest(Rendering Rendering, ChainState State) {
    public static IO<TileRequest> Recorded(Rendering rendering) => rendering.Chain.Map(state => new TileRequest(rendering, state));

    public Fin<TileRequest> Looked(Guid kind, string look) => State.Looked(kind, look).Map(state => this with { State = state });

    public IO<PixelFrame> Formed(Derivations derivations) => Rendering.Frame.Bind(input => Rendering.Displayed(State, input, derivations));

    internal (TileKey Key, PixelExtent Tile) Keyed(PixelExtent asked) =>
        Rendering.Extent.Fitted(asked) switch {
            var tile => (new TileKey(Rendering.Image.FileName, Rendering.Saved, State, tile), tile),
        };
}

internal sealed record TileKey(string FileName, Instant Saved, ChainState State, PixelExtent Tile);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PreviewTiles : IDisposable {
    private static readonly Duration Tick = Duration.FromMilliseconds(150);

    private readonly Atom<Ledger> held = Atom(Ledger.Empty);
    private readonly Atom<TileLimits> limits = Atom(TileLimits.Default);
    private readonly Derivations derivations = new();
    private readonly IPlugInViews views;

    private PreviewTiles(IPlugInViews views) =>
        (this.views, Landed) = (views, Subscriptions.Changed(held).DistinctUntilChanged(static ledger => (unit, ledger.Landings)).Choose(static _ => Some(unit)));

    private sealed record Pending(TileKey Key, TileRequest Request, PixelExtent Extent, HashMap<TileRank, long> Asks) {
        public bool Holds(TileRank rank, Func<long, bool> recent) => Asks.Find(rank).Exists(at => !rank.Shown || recent(at));

        public bool Ranked(Func<long, bool> recent) => Asks.Exists((rank, at) => !rank.Shown || recent(at));
    }

    private sealed record Kept(TileRequest Request, PixelExtent Extent, Fin<PixelFrame> Result, long Drawn);

    private sealed record Ledger(HashMap<string, PixelFrame> Proxies, Seq<Pending> Queue, Option<Pending> Running, HashMap<TileKey, Kept> Tiles, long Landings) {
        public static Ledger Empty { get; } = new(HashMap<string, PixelFrame>(), Seq<Pending>(), None, HashMap<TileKey, Kept>(), 0L);

        public long Bytes =>
            (toSeq(Tiles.Values).Bind(static kept => kept.Result.ToSeq()) + toSeq(Proxies.Values))
                .Fold(0L, static (sum, frame) => sum + (sizeof(float) * (long)frame.Block.Length));

        public Ledger Asked(TileKey key, TileRequest request, PixelExtent tile, TileRank rank, long now) =>
            this with {
                Tiles = rank.Shown ? Tiles.TrySetItem(key, kept => kept with { Drawn = now }) : Tiles,
                Queue = Queue.Exists(pending => pending.Key == key)
                    ? Queue.Map(pending => pending.Key == key ? pending with { Asks = pending.Asks.AddOrUpdate(rank, now) } : pending)
                    : Running.Exists(running => running.Key == key) || (rank.Shown && Tiles.ContainsKey(key))
                        ? Queue
                        : Queue.Add(new Pending(key, request, tile, HashMap((rank, now)))),
            };

        public Ledger Popped(Func<long, bool> recent) =>
            toSeq(TileRank.Items).Choose(rank => Queue.Find(pending => pending.Holds(rank, recent))).Head.Match(
                Some: next => this with { Queue = Queue.Filter(pending => pending.Key != next.Key && pending.Ranked(recent)), Running = Some(next) },
                None: () => this with { Queue = Seq<Pending>(), Running = None });

        public Ledger Landed(Pending running, Fin<PixelFrame> result, long now) =>
            this with { Running = None, Tiles = Tiles.AddOrUpdate(running.Key, new Kept(running.Request, running.Extent, result, now)), Landings = Landings + 1 };

        public Ledger Evicted(int capacity, Func<long, bool> recent) =>
            this with {
                Tiles = Tiles.Except(Tiles.Filter(kept => !recent(kept.Drawn)).AsIterable()
                    .OrderBy(static entry => entry.Value.Drawn)
                    .Take(Tiles.Count - capacity)
                    .Select(static entry => entry.Key)),
            } switch {
                var trimmed => trimmed with {
                    Proxies = trimmed.Proxies.Intersect((trimmed.Queue + trimmed.Running.ToSeq()).Map(static pending => pending.Request.Rendering.Image.FileName)
                        + toSeq(trimmed.Tiles.Values).Map(static kept => kept.Request.Rendering.Image.FileName)),
                },
            };

        public Ledger Refreshed(long now) =>
            toSeq(Tiles.AsIterable()).Fold(
                this with { Tiles = Tiles.Filter(static kept => kept.Result.IsSucc) },
                (ledger, entry) => ledger.Asked(entry.Key, entry.Value.Request, entry.Value.Extent, TileRank.Rest, now));
    }

    // --- [ACQUISITION]
    public static PlugInSlot<PreviewTiles> Slot { get; } = new();

    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> Register { get; } = static (views, retain) =>
        DisposalOps.Acquire(owned =>
            from tiles in DisposalOps.Acquire(IO.lift(() => new PreviewTiles(views)), owned)
            let store = PlugInSettings.Record(views.Settings, TileLimits.Default, Applied.Live)
            from initial in tiles.Limited(store)
            from watched in store.Changed.Traverse(changed => changed.Inline(_ => tiles.Limited(store), views, owned)).As()
            from worker in DisposalOps.Fork(tiles.Ticked.Repeat(Schedule.spaced(Tick.ToTimeSpan())), owned)
            from published in Slot.Publish(views, tiles)
            select unit, retain);

    public void Dispose() => derivations.Dispose();

    private IO<Unit> Limited(ValueStore<TileLimits> store) =>
        store.Read.Bind(read => limits.SwapIO(_ => read.IfNone(TileLimits.Default))).Map(static _ => unit);

    // --- [READS]
    public HostEvent<Unit> Landed { get; }

    public long Bytes => held.Value.Bytes;

    public IO<Option<Fin<PixelFrame>>> Asked(TileRequest request, PixelExtent extent, TileRank rank) =>
        from now in IO.lift(views.Clock.GetTimestamp)
        let keyed = request.Keyed(extent)
        from ledger in held.SwapIO(state => state.Asked(keyed.Key, request, keyed.Tile, rank, now))
        select ledger.Tiles.Find(keyed.Key).Map(static kept => kept.Result);

    public IO<PixelFrame> Formed(TileRequest request, PixelExtent extent, TileRank rank) =>
        Asked(request, extent, rank)
            .RepeatUntil(Schedule.spaced(Tick.ToTimeSpan()), static found => found.IsSome)
            .Bind(static found => IO.lift(found.ToFin(new Refused(nameof(Asked), Refused.Absent)).Bind(static result => result)));

    public IO<SKImage> Thumbnail(TileRequest request, PixelExtent extent, Luminance peak) =>
        from tile in Formed(request, extent, TileRank.Visible)
        from image in IO.lift(() => tile.Displayed(DisplayFormation.Display, peak))
        select image;

    // --- [WRITES]
    public IO<Unit> Refresh =>
        from now in IO.lift(views.Clock.GetTimestamp)
        from refreshed in held.SwapIO(state => state.Refreshed(now))
        select unit;

    // --- [FORMATION]
    private IO<Unit> Ticked =>
        from bounds in limits.ValueIO
        from started in IO.lift(views.Clock.GetTimestamp)
        from ran in Monad.recur<IO, int, Unit>(0, count =>
            from now in IO.lift(views.Clock.GetTimestamp)
            from next in count >= bounds.Batch || (count > 0 && views.Clock.GetElapsedTime(started, now) >= bounds.Budget)
                ? IO.pure(Next.Done<int, Unit>(unit))
                : (from ledger in held.SwapIO(state => state.Popped(Recent(now)))
                   from stepped in ledger.Running.Match(
                       Some: running => Land(running, ledger.Proxies).Map(_ => Next.Loop<int, Unit>(count + 1)),
                       None: static () => IO.pure(Next.Done<int, Unit>(unit)))
                   select stepped)
            select next).As()
        from ended in IO.lift(views.Clock.GetTimestamp)
        from evicted in held.SwapIO(state => state.Evicted(bounds.Capacity, Recent(ended)))
        select unit;

    private IO<Unit> Land(Pending running, HashMap<string, PixelFrame> proxies) =>
        (from proxy in Proxied(running, proxies)
         from tile in running.Request.Rendering.Displayed(running.Request.State, proxy.Downscaled(running.Extent), derivations)
         from kept in Keep(running, tile)
         select kept)
        .Catch(static error => !error.Is(Errors.Cancelled), error => Keep(running, error).Bind(_ => Reported(error)));

    private IO<PixelFrame> Proxied(Pending running, HashMap<string, PixelFrame> proxies) =>
        proxies.Find(running.Request.Rendering.Image.FileName)
            .Filter(proxy => proxy.Size.Width >= running.Extent.Width && proxy.Size.Height >= running.Extent.Height)
            .Match(
                Some: static proxy => IO.pure(proxy),
                None: () =>
                    from frame in running.Request.Rendering.Frame
                    let reduced = frame.Downscaled(running.Extent)
                    from stored in held.SwapIO(state => state with { Proxies = state.Proxies.AddOrUpdate(running.Request.Rendering.Image.FileName, reduced) })
                    select reduced);

    private IO<Unit> Keep(Pending running, Fin<PixelFrame> result) =>
        from now in IO.lift(views.Clock.GetTimestamp)
        from landed in held.SwapIO(state => state.Landed(running, result, now))
        select unit;

    private IO<Unit> Reported(Error error, [CallerMemberName] string member = "") => IO.lift(() => views.Report(error, typeof(PreviewTiles), member));

    private Func<long, bool> Recent(long now) => at => views.Clock.GetElapsedTime(at, now).ToDuration() < Tick * 2;
}
