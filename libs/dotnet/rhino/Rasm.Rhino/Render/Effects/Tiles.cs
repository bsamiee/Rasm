using Eto.Drawing;
using Eto.Forms;
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
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
internal sealed partial class TileRank {
    public static readonly TileRank Current = new(viewed: true);
    public static readonly TileRank Visible = new(viewed: true);
    public static readonly TileRank Rest = new(viewed: false);

    public bool Viewed { get; }
}

[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TileBatch : System.Numerics.IMinMaxValue<TileBatch> {
    public static TileBatch MinValue { get; } = new(1);
    public static TileBatch MaxValue { get; } = new(8);
    public static TileBatch Default { get; } = new(2);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TileBudget : System.Numerics.IMinMaxValue<TileBudget> {
    public static TileBudget MinValue { get; } = new(20);
    public static TileBudget MaxValue { get; } = new(2000);
    public static TileBudget Default { get; } = new(250);

    public Duration Span => Duration.FromMilliseconds(_value);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct TileCapacity : System.Numerics.IMinMaxValue<TileCapacity> {
    public static TileCapacity MinValue { get; } = new(16);
    public static TileCapacity MaxValue { get; } = new(4096);
    public static TileCapacity Default { get; } = new(256);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public sealed record TileLimits(TileBatch Batch, TileBudget Budget, TileCapacity Capacity) {
    public static TileLimits Default { get; } = new(TileBatch.Default, TileBudget.Default, TileCapacity.Default);
}

public sealed record TileLimitRows(
    PlugInSetting<TileBatch, int> Batch,
    PlugInSetting<TileBudget, int> Budget,
    PlugInSetting<TileCapacity, int> Capacity) {
    public static TileLimitRows Default { get; } = new(
        Batch: new(ValueKey.Numeric("PreviewBatch", "Tiles per tick", "Preview tiles formed in one scheduling tick", TileBatch.Default,
            NumericDomain.Bounded<TileBatch, int>(new() { Form = NumberForm.Field })), SettingType.Integer),
        Budget: new(ValueKey.Numeric("PreviewBudget", "Time budget", "Time limit for preview tiles per scheduling tick", TileBudget.Default,
            NumericDomain.Bounded<TileBudget, int>(new() { Form = NumberForm.Field, Unit = Quantity.GetUnitInfo(DurationUnit.Millisecond) })), SettingType.Integer),
        Capacity: new(ValueKey.Numeric("PreviewCapacity", "Cached tiles", "Preview tiles kept while offscreen", TileCapacity.Default,
            NumericDomain.Bounded<TileCapacity, int>(new() { Form = NumberForm.Field })), SettingType.Integer));

    public Seq<PlugInSetting> Rows => [Batch, Budget, Capacity];

    public Seq<Child> Children => [
        new ControlRow.Group("Preview tiles", RowRules.Always) { Wording = Wording.Library },
        .. SectionRows.Setting(Batch, RowRules.Always),
        .. SectionRows.Setting(Budget, RowRules.Always),
        .. SectionRows.Setting(Capacity, RowRules.Always),
    ];

    public IO<TileLimits> Limits(ISettingReader reader) =>
        IO.lift(() => (reader.Current(Batch), reader.Current(Budget), reader.Current(Capacity))
            .Apply(static (batch, budget, capacity) => new TileLimits(batch, budget, capacity))
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
    private readonly Ref<Seq<Raster>> rasters = Ref(Seq<Raster>());

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
            from images in DisposalOps.Acquire(IO.lift(() => new Disposal<IO<Unit>>(tiles.Emptied, static close => Callbacks.Settled(close, CancellationToken.None).ThrowIfFail())), owned)
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
            TransferCurve.Srgb,
            file);

    internal IO<(PixelFrame Frame, PixelFrame Proxy)> Formed(TileRequest request, PixelExtent extent, TileRank rank) =>
        Monad.recur(unit, _ => Asked(request, extent, rank).Bind(static found => found.Match(
            Some: static tile => IO.pure(Next.Done<Unit, (PixelFrame Frame, PixelFrame Proxy)>(tile)),
            None: static () => IO.yieldFor(Tick.ToTimeSpan()).Map(static _ => Next.Loop<Unit, (PixelFrame Frame, PixelFrame Proxy)>(unit))))).As();

    public IO<Disposal<Thumbnail>> Thumbnail(Control view, TileRequest request, Size size, Action<Disposal<Thumbnail>> retain) =>
        DisposalOps.Acquire(owned =>
            from device in HostTheme.Device(size)
            from extent in IO.lift(() => PixelExtent.Validated(device.Width, device.Height))
            from tile in Formed(request, extent, TileRank.Visible)
            from pixels in FrameView.Raster(view, tile.Frame, TransferCurve.Srgb, owned)
            select new Disposal<Thumbnail>(new Thumbnail.Raster(pixels), Rhino.UI.Drawing.Thumbnail.Release), (thumbnail, _) => retain(thumbnail));

    public IO<Unit> Refresh =>
        from now in IO.lift(clock.GetTimestamp)
        from _ in held.SwapIO(state => state.Refreshed(now))
        select unit;

    public long Bytes => held.Value.Bytes;

    public IO<Option<Unit>> Paint(object source, Option<Control> view, PixelFrame frame, PixelExtent extent, Imaging.Color.Transfer encoding, Action<Bitmap> draw) =>
        from now in IO.lift(clock.GetTimestamp)
        let key = (Source: source, View: view, Frame: frame, Extent: frame.Size.Fitted(extent), Encoding: encoding)
        from pixels in IO.lift(() => rasters.Value.Find(row => row.Key == key)).Bind(found => found.Match(
            Some: static row => IO.pure(Some(row.Pixels)),
            None: () => Requested(Rastered(key, now))))
        from painted in pixels.Traverse(bitmap => IO.lift(() => {
            _ = atomic(() => rasters.Swap(rows => rows.Map(row => row.Key == key ? row with { Drawn = now } : row)));
            draw(bitmap);
        })).As()
        from released in DisposalOps.Within(() => atomic(() => {
            Seq<Raster> current = rasters.Value;
            Seq<Bitmap> gone = toSeq(current.Filter(row => !Recent(now)(row.Drawn)).AsIterable()
                .OrderBy(static row => row.Drawn).Take(current.Count + held.Value.Tiles.Count - bounds.Value.Capacity)
                .Select(static row => row.Pixels));
            rasters.Value = current.Filter(row => !gone.Exists(removed => ReferenceEquals(row.Pixels, removed)));
            return gone;
        }), static _ => IO.pure(unit), DisposalOps.Release)
        select painted;

    private IO<Bitmap> Rastered((object Source, Option<Control> View, PixelFrame Frame, PixelExtent Extent, Imaging.Color.Transfer Encoding) key, long now) {
        Action<Bitmap> kept = bitmap => _ = atomic(() => rasters.Swap(rows => rows.Add(new Raster(key, bitmap, now))));
        return from reduced in IO.lift(() => key.Frame.Downscaled(key.Extent))
               from image in key.View.Match(
                   Some: control => FrameView.Raster(control, reduced, key.Encoding, kept),
                   None: () => DisposalOps.Acquire(IO.lift(() => ImagePlots.Pixels(reduced.Size, key.Encoding.Displayed(reduced))), kept))
               select image;
    }

    private IO<Unit> Emptied =>
        atomicIO(() => {
            Seq<Raster> images = rasters.Value;
            rasters.Value = [];
            return images.Map(static row => row.Pixels);
        }).Bind(DisposalOps.Release).OnUiThread().Uninterruptible();

    private IO<Option<(PixelFrame Frame, PixelFrame Proxy)>> Asked(TileRequest request, PixelExtent asked, TileRank rank) =>
        from now in IO.lift(clock.GetTimestamp)
        let keyed = request.Keyed(asked)
        from ledger in held.SwapIO(state => state.Asked(keyed.Key, request, keyed.Tile, rank, now))
        from tile in ledger.Tiles.Find(keyed.Key).Traverse(kept => IO.lift(kept.Result.Map(frame => (Frame: frame, Proxy: ledger.Proxies[request.Rendering.Key])))).As()
        select tile;

    // --- [FORMATION]
    private IO<Unit> Ticked =>
        (from reset in atomicIO(() => {
            if (usage.Value.Running is 0) usage.Value = (0, Duration.Zero, 0);
            return unit;
        })
         from _ in Monad.recur(unit, _ => Step).As()
         from limits in bounds.ValueIO
         from now in IO.lift(clock.GetTimestamp)
         from __ in held.SwapIO(state => state.Evicted(limits.Capacity - rasters.Value.Count, Recent(now)))
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
            () => atomic(() => {
                (int count, Duration spent, int running) = usage.Value;
                TileLimits policy = bounds.Value;
                bool accepted = count < policy.Batch && (count is 0 || spent < policy.Budget.Span);
                if (accepted) usage.Value = (count + 1, spent, running + 1);
                return accepted ? Some(clock.GetTimestamp()) : Option<long>.None;
            }),
            began => began.Match(Some: _ => request.Map(static value => Some(value)), None: static () => IO.pure(Option<T>.None)),
            began => began.Traverse(at => atomicIO(() => {
                (int count, Duration spent, int running) = usage.Value;
                usage.Value = (count, spent + clock.GetElapsedTime(at).ToDuration(), running - 1);
                return unit;
            })).As().Map(unit));

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
        select unit;

    private IO<Unit> Reported(Error error) => IO.lift(() => owner.Report(error, typeof(PreviewTiles), nameof(Register)));

    private Func<long, bool> Recent(long now) => at => clock.GetElapsedTime(at, now).ToDuration() < Tick * 2;

    // --- [LEDGER]
    private sealed record Raster((object Source, Option<Control> View, PixelFrame Frame, PixelExtent Extent, Imaging.Color.Transfer Encoding) Key, Bitmap Pixels, long Drawn);

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
