using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Slots;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Viewers;
using Rasm.Rhino.UI.Views;
using Rhino.PlugIns;

namespace Rasm.Rhino.Render.Effects;

// --- [TYPES] ---------------------------------------------------------------------------
internal enum TileRank {
    Current,
    Visible,
    Rest,
}

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TileBatch : System.Numerics.IMinMaxValue<TileBatch> {
    public static TileBatch MinValue { get; } = new(1);
    public static TileBatch MaxValue { get; } = new(8);
    public static TileBatch Default { get; } = new(2);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value >= MinValue._value && value <= MaxValue._value ? null : new InvalidRhinoValue();
}

[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TileBudget : System.Numerics.IMinMaxValue<TileBudget> {
    public static TileBudget MinValue { get; } = new(20);
    public static TileBudget MaxValue { get; } = new(2000);
    public static TileBudget Default { get; } = new(250);

    public Duration Span => Duration.FromMilliseconds(_value);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value >= MinValue._value && value <= MaxValue._value ? null : new InvalidRhinoValue();
}

[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TileCapacity : System.Numerics.IMinMaxValue<TileCapacity> {
    public static TileCapacity MinValue { get; } = new(16);
    public static TileCapacity MaxValue { get; } = new(4096);
    public static TileCapacity Default { get; } = new(256);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value >= MinValue._value && value <= MaxValue._value ? null : new InvalidRhinoValue();
}

public sealed record TileLimits(TileBatch Batch, TileBudget Budget, TileCapacity Capacity) {
    public static TileLimits Default { get; } = new(TileBatch.Default, TileBudget.Default, TileCapacity.Default);
}

public sealed record TileLimitRows(
    PlugInSetting<TileBatch, int> Batch,
    PlugInSetting<TileBudget, int> Budget,
    PlugInSetting<TileCapacity, int> Capacity) {
    public Seq<PlugInSetting> Rows => [Batch, Budget, Capacity];

    public Seq<Child> Children =>
        (SectionRows.Setting(Batch, RowRules.Always)
         + SectionRows.Setting(Budget, RowRules.Always)
         + SectionRows.Setting(Capacity, RowRules.Always))
            .Map(static row => (Child)row);

    public IO<TileLimits> Limits(ISettingReader reader) =>
        (reader.Current(Batch), reader.Current(Budget), reader.Current(Capacity))
            .Apply(static (batch, budget, capacity) => new TileLimits(batch, budget, capacity))
            .As();
}

public sealed record TileRequest(RenderSlot Slot, ValueSet Set, Seq<EffectKind> Chain) {
    public static TileRequest Recorded(RenderSlot slot, Seq<EffectKind> kinds) =>
        new(slot, slot.Grading, slot.Running(kinds));

    public static Fin<TileRequest> Applied(RenderSlot slot, Preset preset, ValueSet current, LanguageExt.HashSet<EntryKey> locks, Seq<EffectKind> chain) =>
        preset.Values.Map(values => new TileRequest(slot, values.Over(current, locks), chain));

    public ContentKey Rendering =>
        Slot.State.Match(
            Some: static state => state.Key,
            None: () => ContentKey.Of(KeyDomain.HistorySlot, stream => stream.Integer(Slot.Saved.ToUnixTimeMilliseconds()).Integer(Slot.Extent.Width).Integer(Slot.Extent.Height)));

    public (ContentKey Key, PixelExtent Tile) Keyed(PixelExtent asked) =>
        Slot.Extent.Fitted(asked) switch {
            var tile => (
                ContentKey.Of(KeyDomain.Tile, stream => stream
                    .Integer<UInt128>(Rendering)
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
    private static readonly Duration Window = Tick * 2;

    private readonly Atom<Ledger> held = Atom(Ledger.Empty);
    private readonly Atom<TileLimits> bounds = Atom(TileLimits.Default);
    private readonly Ref<(int Count, Duration Spent, int Running)> usage = Ref((0, Duration.Zero, 0));
    private readonly Derivations derivations;
    private readonly IPlugInSink owner;
    private readonly TimeProvider clock;
    private readonly IO<TileLimits> limits;
    private readonly Ref<Seq<Raster>> rasters = Ref(Seq<Raster>());

    private PreviewTiles(IPlugInSink owner, TimeProvider clock, IO<TileLimits> limits, Derivations derivations) =>
        (this.owner, this.clock, this.limits, this.derivations) = (owner, clock, limits, derivations);

    public static Func<PlugIn, IPlugInSink, Action<IDisposable>, IO<Unit>> Register(Option<TileLimitRows> limits, Func<PreviewTiles, Seq<Func<PlugIn, IPlugInSink, Action<IDisposable>, IO<Unit>>>> readers) =>
        (plugIn, sink, retain) => DisposalOps.Acquire(owned =>
            from views in IO.pure((IPlugInViews)sink)
            from tiles in Open(sink, views.Clock,
                limits.Match(Some: rows => rows.Limits(views.Settings), None: static () => IO.pure(TileLimits.Default)),
                limits.Map(_ => SettingRoots.Saved(views.Settings)), owned)
            let cell = ((IPlugInRendering)sink).Tiles
            from acquired in readers(tiles).TraverseM(row => row(plugIn, sink, owned)).As()
            from served in DisposalOps.Change(() => cell.Value, current => _ = cell.Swap(_ => current), Some(tiles), owned)
            select unit, (_, lease) => retain(lease));

    public static IO<PreviewTiles> Open(IPlugInSink owner, TimeProvider clock, IO<TileLimits> limits, Option<HostEvent<Unit>> changes, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from derivations in DisposalOps.Acquire(IO.lift(static () => new Derivations()), static held => IO.lift(held.Dispose), owned)
            from tiles in IO.lift(() => new PreviewTiles(owner, clock, limits, derivations))
            from environment in envIO
            from images in DisposalOps.Acquire(
                IO.lift(() => new Disposal<IO<Unit>>(tiles.Emptied, close => close.Run(environment))), static lease => IO.lift(lease.Dispose), owned)
            let read = limits.Bind(tiles.Limit)
            from initial in read
            from saved in changes.TraverseM(changed => changed.Choose(static _ => Some((unit, unit)))
                .Through(Subscriptions.Idle<Unit, Unit>(_ => read), owner, owned)).As()
            from worker in DisposalOps.Fork(tiles.Ticked.Repeat(Schedule.spaced(Tick.ToTimeSpan())), owned)
            select tiles, (_, lease) => retain(lease));

    private IO<Unit> Limit(TileLimits current) => bounds.SwapIO(_ => current).Map(static _ => unit);

    // --- [ASKS]
    public TileSource Source(IO<Option<TileRequest>> request, IO<Option<string>> file) =>
        new(request.Map(static asked => asked.Map(static wanted => wanted.Slot.Extent)),
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
            from extent in IO.lift(() => PixelExtent.Validate(device.Width, device.Height, out PixelExtent asked) is { } error ? Fin.Fail<PixelExtent>(error) : Fin.Succ(asked))
            from tile in Formed(request, extent, TileRank.Visible)
            from pixels in FrameView.Raster(owner, view, tile.Frame, TransferCurve.Srgb, owned)
            select new Disposal<Thumbnail>(new Thumbnail.Raster(pixels), UI.Assets.Thumbnail.Release), (thumbnail, _) => retain(thumbnail));

    public IO<Unit> Refresh =>
        from now in IO.lift(clock.GetTimestamp)
        from _ in held.SwapIO(state => state.Refreshed(now))
        select unit;

    public long Bytes => held.Value.Bytes;

    public IO<Option<Unit>> Paint(object source, Option<Control> view, PixelFrame frame, PixelExtent extent, Imaging.ColorManagement.Transfer encoding, Action<Bitmap> draw) =>
        from current in limits.Bind(Limit)
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

    private IO<Bitmap> Rastered((object Source, Option<Control> View, PixelFrame Frame, PixelExtent Extent, Imaging.ColorManagement.Transfer Encoding) key, long now) {
        Action<Bitmap> kept = bitmap => _ = atomic(() => rasters.Swap(rows => rows.Add(new Raster(key, bitmap, now))));
        return from reduced in IO.lift(() => key.Frame.Downscaled(key.Extent))
               from image in key.View.Match(
                   Some: control => FrameView.Raster(owner, control, reduced, key.Encoding, kept),
                   None: () => DisposalOps.Acquire(IO.lift(() => Plots.Pixels(reduced.Size, key.Encoding.Displayed(reduced))), static bitmap => IO.lift(bitmap.Dispose), kept))
               select image;
    }

    private IO<Unit> Emptied =>
        atomicIO(() => {
            Seq<Raster> images = rasters.Value;
            rasters.Value = [];
            return images.Map(static row => row.Pixels);
        }).Bind(DisposalOps.Release).Post().Uninterruptible();

    private IO<Option<(PixelFrame Frame, PixelFrame Proxy)>> Asked(TileRequest request, PixelExtent asked, TileRank rank) =>
        from now in IO.lift(clock.GetTimestamp)
        let keyed = request.Keyed(asked)
        from ledger in held.SwapIO(state => state.Asked(keyed.Key, request, keyed.Tile, rank, now))
        from tile in ledger.Tiles.Find(keyed.Key).Traverse(kept => IO.lift(kept.Result.Map(frame => (Frame: frame, Proxy: ledger.Proxies[request.Rendering])))).As()
        select tile;

    // --- [FORMATION]
    private IO<Unit> Ticked =>
        (from reset in atomicIO(() => {
            if (usage.Value.Running == 0) usage.Value = (0, Duration.Zero, 0);
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
                bool accepted = count < policy.Batch && (count == 0 || spent < policy.Budget.Span);
                if (accepted) usage.Value = (count + 1, spent, running + 1);
                return accepted ? Some(clock.GetTimestamp()) : Option<long>.None;
            }),
            began => began.Match(Some: _ => request.Map(static value => Some(value)), None: static () => IO.pure(Option<T>.None)),
            began => began.Traverse(at => atomicIO(() => {
                (int count, Duration spent, int running) = usage.Value;
                usage.Value = (count, spent + clock.GetElapsedTime(at, clock.GetTimestamp()).ToDuration(), running - 1);
                return unit;
            })).As().Map(static _ => unit));

    private IO<Unit> Land(Pending running, HashMap<ContentKey, PixelFrame> proxies) =>
        (from proxy in Proxied(running, proxies)
         from tile in EffectPipeline.Form(proxy.Downscaled(running.Extent), running.Request.Set, running.Request.Chain, derivations)
         from kept in Keep(running, tile)
         select kept)
        .Catch(static error => !error.Is(Errors.Cancelled), error => Keep(running, error).Bind(_ => Reported(error)));

    private IO<PixelFrame> Proxied(Pending running, HashMap<ContentKey, PixelFrame> proxies) =>
        running.Request.Rendering switch {
            var rendering => proxies.Find(rendering)
                .Filter(proxy => proxy.Size.Width >= running.Extent.Width && proxy.Size.Height >= running.Extent.Height)
                .Match(
                    Some: static proxy => IO.pure(proxy),
                    None: () =>
                        from frame in running.Request.Slot.Frame
                        let reduced = frame.Downscaled(running.Extent)
                        from _ in held.SwapIO(state => state with { Proxies = state.Proxies.AddOrUpdate(rendering, reduced) })
                        select reduced),
        };

    private IO<Unit> Keep(Pending running, Fin<PixelFrame> result) =>
        from now in IO.lift(clock.GetTimestamp)
        from _ in held.SwapIO(state => state.Landed(running, result, now))
        select unit;

    private IO<Unit> Reported(Error error) => IO.lift(() => owner.Report(error, typeof(PreviewTiles), nameof(Register)));

    private Func<long, bool> Recent(long now) => at => clock.GetElapsedTime(at, now).ToDuration() < Window;

    // --- [LEDGER]
    private sealed record Raster((object Source, Option<Control> View, PixelFrame Frame, PixelExtent Extent, Imaging.ColorManagement.Transfer Encoding) Key, Bitmap Pixels, long Drawn);

    private sealed record Pending(ContentKey Key, TileRequest Request, PixelExtent Extent, HashMap<TileRank, long> Asks) {
        public bool Holds(TileRank rank, Func<long, bool> recent) => Asks.Find(rank).Exists(at => rank == TileRank.Rest || recent(at));

        public bool Ranked(Func<long, bool> recent) => toSeq(Enum.GetValues<TileRank>()).Exists(rank => Holds(rank, recent));
    }

    private sealed record Kept(TileRequest Request, PixelExtent Extent, Fin<PixelFrame> Result, long Drawn);

    private sealed record Ledger(HashMap<ContentKey, PixelFrame> Proxies, Seq<Pending> Queue, Option<Pending> Running, HashMap<ContentKey, Kept> Tiles) {
        public static Ledger Empty { get; } = new(HashMap<ContentKey, PixelFrame>(), Seq<Pending>(), None, HashMap<ContentKey, Kept>());

        public long Bytes =>
            toSeq(Tiles.Values).Bind(static kept => kept.Result.ToSeq()).Concat(toSeq(Proxies.Values))
                .Fold(0L, static (sum, frame) => sum + (sizeof(float) * (long)frame.Block.Length));

        public Ledger Asked(ContentKey key, TileRequest request, PixelExtent tile, TileRank rank, long now) =>
            this with {
                Tiles = rank == TileRank.Rest ? Tiles : Tiles.TrySetItem(key, kept => kept with { Drawn = now }),
                Queue = Queue.Exists(pending => pending.Key == key)
                    ? Queue.Map(pending => pending.Key == key ? pending with { Asks = pending.Asks.AddOrUpdate(rank, now) } : pending)
                    : Running.Exists(running => running.Key == key) || (rank != TileRank.Rest && Tiles.ContainsKey(key))
                        ? Queue
                        : Queue.Add(new Pending(key, request, tile, HashMap((rank, now)))),
            };

        public Ledger Popped(Func<long, bool> recent) =>
            toSeq(Enum.GetValues<TileRank>()).Choose(rank => Queue.Find(pending => pending.Holds(rank, recent))).Head.Match(
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
                var trimmed => trimmed with { Proxies = trimmed.Proxies.Intersect(trimmed.Requests.Map(static request => request.Rendering)) },
            };

        public Ledger Refreshed(long now) =>
            toSeq(Tiles.AsIterable()).Fold(
                this with { Tiles = Tiles.Filter(static kept => kept.Result.IsSucc) },
                (ledger, entry) => ledger.Asked(entry.Key, entry.Value.Request, entry.Value.Extent, TileRank.Rest, now));

        private Seq<TileRequest> Requests =>
            Queue.Map(static pending => pending.Request)
                .Concat(Running.Map(static pending => pending.Request).ToSeq())
                .Concat(toSeq(Tiles.Values).Map(static kept => kept.Request));
    }
}
