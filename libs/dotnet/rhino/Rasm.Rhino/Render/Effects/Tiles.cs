using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Viewers;
using Rasm.Rhino.UI.Views;
using Rhino.PlugIns;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TileBatch : System.Numerics.IMinMaxValue<TileBatch> {
    public static TileBatch MinValue { get; } = new(1);
    public static TileBatch MaxValue { get; } = new(8);
    public static TileBatch Default { get; } = new(2);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value == int.Clamp(value, MinValue._value, MaxValue._value) ? null : new InvalidRhinoValue();
}

[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TileBudget : System.Numerics.IMinMaxValue<TileBudget> {
    public static TileBudget MinValue { get; } = new(20);
    public static TileBudget MaxValue { get; } = new(2000);
    public static TileBudget Default { get; } = new(250);

    public Duration Span => Duration.FromMilliseconds(_value);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value == int.Clamp(value, MinValue._value, MaxValue._value) ? null : new InvalidRhinoValue();
}

[ValueObject<int>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TileCapacity : System.Numerics.IMinMaxValue<TileCapacity> {
    public static TileCapacity MinValue { get; } = new(16);
    public static TileCapacity MaxValue { get; } = new(4096);
    public static TileCapacity Default { get; } = new(256);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value == int.Clamp(value, MinValue._value, MaxValue._value) ? null : new InvalidRhinoValue();
}

public sealed record TileLimits(TileBatch Batch, TileBudget Budget, TileCapacity Capacity) {
    public static TileLimits Default { get; } = new(TileBatch.Default, TileBudget.Default, TileCapacity.Default);
}

public sealed record TileLimitRows(
    PlugInSetting<TileBatch, int, InvalidRhinoValue> Batch,
    PlugInSetting<TileBudget, int, InvalidRhinoValue> Budget,
    PlugInSetting<TileCapacity, int, InvalidRhinoValue> Capacity) {
    public Seq<PlugInSetting> Rows => [Batch, Budget, Capacity];

    public IO<TileLimits> Limits(SettingsNode node) =>
        (PlugInSettings.Current(node, Batch), PlugInSettings.Current(node, Budget), PlugInSettings.Current(node, Capacity))
            .Apply(static (batch, budget, capacity) => new TileLimits(batch, budget, capacity))
            .As();
}

public sealed record TileRequest(RenderSlot Slot, ValueSet Set, Seq<EffectKind> Chain) {
    public static TileRequest Recorded(RenderSlot slot, Seq<EffectKind> kinds) =>
        slot.State.Match(
            Some: state => new TileRequest(slot, state.Grading, state.Running(kinds)),
            None: () => new TileRequest(slot, ValueSet.Empty, Seq<EffectKind>()));

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

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class TileRank {
    public static readonly TileRank Current = new(forced: false);
    public static readonly TileRank Visible = new(forced: false);
    public static readonly TileRank Rest = new(forced: true);

    public bool Forced { get; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PreviewTiles {
    // --- [ACQUISITION]
    private static readonly Duration Tick = Duration.FromMilliseconds(150);
    private static readonly Duration Window = Tick * 2;

    private readonly Atom<Ledger> held = Atom(Ledger.Empty);
    private readonly Atom<TileLimits> bounds = Atom(TileLimits.Default);
    private readonly Derivations derivations = new();
    private readonly IPlugInViews owner;

    private PreviewTiles(IPlugInViews owner) => this.owner = owner;

    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> Register(Option<TileLimitRows> limits, Func<PreviewTiles, Seq<Func<PlugIn, IPlugInSink, IO<IDisposable>>>> readers) =>
        (plugIn, sink) =>
            from tiles in IO.lift(() => new PreviewTiles((IPlugInViews)sink))
            let views = tiles.owner
            let read = limits.Match(
                Some: rows => rows.Limits(views.Settings).Bind(tiles.Limit),
                None: static () => IO.pure(unit))
            from initial in read
            let cell = ((IPlugInRendering)sink).Tiles
            let site = new CallbackSite(sink, typeof(PreviewTiles), nameof(Register))
            from acquired in DisposalOps.AcquireAll(
                limits.Map(_ => SettingRoots.Saved(views.Settings).Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ => read), sink)).ToSeq()
                    .Add(tiles.Attach(site))
                    .Concat(readers(tiles).Map(row => row(plugIn, sink)))
                    .Add(cell.SwapIO(_ => Some(tiles)).Map(_ => (IDisposable)new Disposal<Atom<Option<PreviewTiles>>>(cell, static filled => _ = filled.Swap(static _ => None)))),
                DisposalOps.Release)
            select DisposalOps.Composite(acquired, site);

    private IO<IDisposable> Attach(CallbackSite site) =>
        Ticked.Repeat(Schedule.spaced(Tick.ToTimeSpan())).Fork()
            .Map(worker => (IDisposable)new Disposal<ForkIO<Unit>>(worker, running => Callbacks.Succeeded(running.Cancel, site)));

    private IO<Unit> Limit(TileLimits current) => bounds.SwapIO(_ => current).Map(static _ => unit);

    // --- [ASKS]
    public TileSource Source(IO<Option<TileRequest>> request, IO<Option<string>> file) =>
        new(request.Map(static asked => asked.Map(static wanted => wanted.Slot.Extent)),
            extent => request.Bind(asked => asked.Match(
                Some: wanted => Asked(wanted, extent, TileRank.Current),
                None: static () => IO.pure(Option<PixelFrame>.None))),
            TransferCurve.Srgb,
            file);

    public IO<PixelFrame> Formed(TileRequest request, PixelExtent extent) =>
        Monad.recur(unit, _ => Asked(request, extent, TileRank.Visible).Bind(static found => found.Match(
            Some: static tile => IO.pure(Next.Done<Unit, PixelFrame>(tile)),
            None: static () => IO.yieldFor(Tick.ToTimeSpan()).Map(static _ => Next.Loop<Unit, PixelFrame>(unit))))).As();

    public IO<Disposal<Thumbnail>> Thumbnail(Control view, TileRequest request, Size size) =>
        from device in Themes.Device(size)
        from extent in IO.lift(() => PixelExtent.Validate(device.Width, device.Height, out PixelExtent asked) is { } error ? Fin.Fail<PixelExtent>(error) : Fin.Succ(asked))
        from tile in Formed(request, extent)
        from pixels in FrameView.Raster(owner, view, tile, TransferCurve.Srgb)
        select new Disposal<Thumbnail>(new Thumbnail.Raster(pixels), UI.Assets.Thumbnail.Release);

    public IO<Unit> Refresh =>
        from now in IO.lift(owner.Clock.GetTimestamp)
        from _ in held.SwapIO(state => state.Refreshed(now))
        select unit;

    public long Bytes => held.Value.Bytes;

    private IO<Option<PixelFrame>> Asked(TileRequest request, PixelExtent asked, TileRank rank) =>
        from now in IO.lift(owner.Clock.GetTimestamp)
        let keyed = request.Keyed(asked)
        from ledger in held.SwapIO(state => state.Asked(keyed.Key, request, keyed.Tile, rank, now))
        from tile in ledger.Tiles.Find(keyed.Key).Traverse(static kept => IO.lift(kept.Result)).As()
        select tile;

    // --- [FORMATION]
    private IO<Unit> Ticked =>
        (from limits in bounds.ValueIO
         from began in IO.lift(owner.Clock.GetTimestamp)
         from _ in Monad.recur(0, count => Step(limits, began, count)).As()
         from now in IO.lift(owner.Clock.GetTimestamp)
         from __ in held.SwapIO(state => state.Evicted(limits.Capacity, Recent(now)))
         select unit)
        .Catch(static error => !error.Is(Errors.Cancelled), Reported);

    private IO<Next<int, Unit>> Step(TileLimits limits, long began, int count) =>
        from now in IO.lift(owner.Clock.GetTimestamp)
        from next in count == limits.Batch || (count > 0 && owner.Clock.GetElapsedTime(began, now).ToDuration() >= limits.Budget.Span)
            ? IO.pure(Next.Done<int, Unit>(unit))
            : from ledger in held.SwapIO(state => state.Popped(Recent(now)))
              from landed in ledger.Running.Match(
                  Some: running => Land(running, ledger.Proxies).Map(_ => Next.Loop<int, Unit>(count + 1)),
                  None: static () => IO.pure(Next.Done<int, Unit>(unit)))
              select landed
        select next;

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
        from now in IO.lift(owner.Clock.GetTimestamp)
        from _ in held.SwapIO(state => state.Landed(running, result, now))
        select unit;

    private IO<Unit> Reported(Error error) => IO.lift(() => owner.Report(error, typeof(PreviewTiles), nameof(Attach)));

    private Func<long, bool> Recent(long now) => at => owner.Clock.GetElapsedTime(at, now).ToDuration() < Window;

    // --- [LEDGER]
    private sealed record Pending(ContentKey Key, TileRequest Request, PixelExtent Extent, HashMap<TileRank, long> Asks) {
        public bool Holds(TileRank rank, Func<long, bool> recent) => Asks.Find(rank).Exists(at => rank.Forced || recent(at));

        public bool Ranked(Func<long, bool> recent) => toSeq(TileRank.Items).Exists(rank => Holds(rank, recent));
    }

    private sealed record Kept(TileRequest Request, PixelExtent Extent, Fin<PixelFrame> Result, long Drawn);

    private sealed record Ledger(HashMap<ContentKey, PixelFrame> Proxies, Seq<Pending> Queue, Option<Pending> Running, HashMap<ContentKey, Kept> Tiles) {
        public static Ledger Empty { get; } = new(HashMap<ContentKey, PixelFrame>(), Seq<Pending>(), None, HashMap<ContentKey, Kept>());

        public long Bytes =>
            toSeq(Tiles.Values).Bind(static kept => kept.Result.ToSeq()).Concat(toSeq(Proxies.Values))
                .Fold(0L, static (sum, frame) => sum + (sizeof(float) * (long)frame.Block.Length));

        public Ledger Asked(ContentKey key, TileRequest request, PixelExtent tile, TileRank rank, long now) =>
            this with {
                Tiles = rank.Forced ? Tiles : Tiles.TrySetItem(key, kept => kept with { Drawn = now }),
                Queue = Queue.Exists(pending => pending.Key == key)
                    ? Queue.Map(pending => pending.Key == key ? pending with { Asks = pending.Asks.AddOrUpdate(rank, now) } : pending)
                    : Running.Exists(running => running.Key == key) || (!rank.Forced && Tiles.ContainsKey(key))
                        ? Queue
                        : Queue.Add(new Pending(key, request, tile, HashMap((rank, now)))),
            };

        public Ledger Popped(Func<long, bool> recent) =>
            toSeq(TileRank.Items).Choose(rank => Queue.Find(pending => pending.Holds(rank, recent))).Head.Match(
                Some: next => this with { Queue = Queue.Filter(pending => pending.Key != next.Key && pending.Ranked(recent)), Running = Some(next) },
                None: () => this with { Queue = Seq<Pending>(), Running = None });

        public Ledger Landed(Pending running, Fin<PixelFrame> result, long now) =>
            this with { Running = None, Tiles = Tiles.AddOrUpdate(running.Key, new Kept(running.Request, running.Extent, result, now)) };

        public Ledger Evicted(TileCapacity capacity, Func<long, bool> recent) =>
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
