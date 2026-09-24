using System.Threading.Channels;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<int>]
[ValidationError<ValidationFailure>]
public readonly partial struct Capacity {
    public static Fin<Capacity> From(int value) =>
        Validate(value, provider: null, out Capacity item) is { } error ? error : item;

    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref int value) {
        if (value < 1)
            validationError = new Invalid(nameof(Capacity));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ChannelMode {
    public sealed record Latest() : ChannelMode;

    public sealed record DropOldest(Capacity Capacity) : ChannelMode;

    public sealed record DropNewest(Capacity Capacity) : ChannelMode;

    public sealed record DropWrite(Capacity Capacity) : ChannelMode;

    public sealed record Unbounded() : ChannelMode;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Delivery<T> {
    public abstract Func<T, IO<Unit>> Sink { get; init; }

    public sealed record Inline(Func<T, IO<Unit>> Sink) : Delivery<T>;

    public sealed record Idle(Capacity Capacity, Func<T, IO<Unit>> Sink) : Delivery<T>;

    public sealed record Channel(ChannelMode Mode, Func<T, IO<Unit>> Sink) : Delivery<T>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeliverySource {
    public sealed record Event(EventKind Kind) : DeliverySource;

    public sealed record File(string WatchedPath) : DeliverySource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DeliveryLoss {
    public sealed record ChannelOverflow(DeliverySource Source) : DeliveryLoss;

    public sealed record IdleOverflow(DeliverySource Source) : DeliveryLoss;

    public sealed record Reentrant(DeliverySource Source) : DeliveryLoss;

    public sealed record CallbackFault(EventKind Kind, Error Cause) : DeliveryLoss;

    public sealed record SinkFault(DeliverySource Source, Error Cause) : DeliveryLoss;

    public sealed record Canceled(int ItemCount) : DeliveryLoss;

    public sealed record FileOverflow(string WatchedPath) : DeliveryLoss;

    public sealed record FileFault(string WatchedPath, Error Cause) : DeliveryLoss;

    public sealed record DetachFault(Error Cause) : DeliveryLoss;
}

public readonly record struct FileChange(WatcherChangeTypes Kind, string Path, Option<string> PreviousPath);

public sealed record FileBatch(Seq<FileChange> Changes, long Overflow) {
    public static FileBatch Empty { get; } = new(Seq<FileChange>(), Overflow: 0L);
}

public sealed record WatchedFile {
    private WatchedFile(string path, string directory) {
        Path = path;
        Directory = directory;
    }

    public string Path { get; }

    public string Directory { get; }

    public string Name => System.IO.Path.GetFileName(Path);

    public static IO<WatchedFile> Create(string path) =>
        from existing in Answers.ExistingPath(path)
        from directory in IO.lift(() => Optional(System.IO.Path.GetDirectoryName(existing)).ToFin(new Invalid(nameof(System.IO.Path.GetDirectoryName))))
        select new WatchedFile(existing, directory);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Deliveries {
    // --- [CHANNELS]
    public static Channel<T> CreateChannel<T>(ChannelMode mode, Action<T> dropped) =>
        mode.Switch(
            dropped,
            latest: static (drop, _) => Channel.CreateBounded(Bounded(1, BoundedChannelFullMode.DropOldest), drop),
            dropOldest: static (drop, bounded) => Channel.CreateBounded(Bounded(bounded.Capacity, BoundedChannelFullMode.DropOldest), drop),
            dropNewest: static (drop, bounded) => Channel.CreateBounded(Bounded(bounded.Capacity, BoundedChannelFullMode.DropNewest), drop),
            dropWrite: static (drop, bounded) => Channel.CreateBounded(Bounded(bounded.Capacity, BoundedChannelFullMode.DropWrite), drop),
            unbounded: static (_, _) => Channel.CreateUnbounded<T>());

    private static BoundedChannelOptions Bounded(int capacity, BoundedChannelFullMode mode) =>
        new(capacity) { FullMode = mode, SingleReader = true, AllowSynchronousContinuations = false };

    // --- [ROUTES]
    private static IO<Route<T>> Open<T>(Delivery<T> delivery, Origin<T> origin) =>
        delivery.Switch(
            origin,
            inline: static (source, inline) => IO.lift(static () => Atom(0))
                .Map(depth => new Route<T>(item => Direct(depth, inline.Sink, item, source), IO.pure(unit))),
            idle: static (source, idle) =>
                from queue in IO.lift(() => CreateChannel<T>(new ChannelMode.DropNewest(idle.Capacity), item => source.Lost(new DeliveryLoss.IdleOverflow(source.Source(item)))))
                from handler in Events.OnIdle((_, _) => _ = Drained(queue.Reader, idle.Sink, source).RunSafe())
                select new Route<T>(
                    item => IO.lift(() => { _ = queue.Writer.TryWrite(item); }),
                    IO.lift(() => { _ = queue.Writer.TryComplete(); }).Bind(_ => IO.lift(handler.Dispose))),
            channel: static (source, channel) =>
                from queue in IO.lift(() => CreateChannel<T>(channel.Mode, item => source.Lost(new DeliveryLoss.ChannelOverflow(source.Source(item)))))
                from reader in Pump(queue.Reader, channel.Sink, source).Fork()
                select new Route<T>(
                    item => IO.lift(() => { _ = queue.Writer.TryWrite(item); }),
                    from canceled in reader.Cancel
                    from completed in IO.lift(() => { _ = queue.Writer.TryComplete(); })
                    from remaining in IO.lift(() => queue.Reader.Count)
                    from noted in when(remaining > 0, IO.lift(() => source.Lost(new DeliveryLoss.Canceled(remaining)))).As()
                    select noted));

    private static IO<Unit> Direct<T>(Atom<int> depth, Func<T, IO<Unit>> sink, T item, Origin<T> origin) =>
        Disposal.Bracketed(
            depth.SwapIO(static level => level + 1),
            _ => depth.SwapIO(static level => level - 1).Map(static _ => unit),
            level => level > 1 ? IO.lift(() => origin.Lost(new DeliveryLoss.Reentrant(origin.Source(item)))) : Guarded(sink, item, origin));

    private static IO<Unit> Drained<T>(ChannelReader<T> reader, Func<T, IO<Unit>> sink, Origin<T> origin) =>
        IO.lift(() => Buffered(reader, reader.Count))
            .Bind(items => items.TraverseM(item => Guarded(sink, item, origin)).As())
            .Map(static _ => unit);

    private static Seq<T> Buffered<T>(ChannelReader<T> reader, int count) =>
        (count > 0) && reader.TryRead(out T? item) ? item.Cons(Buffered(reader, count - 1)) : Seq<T>();

    private static IO<Unit> Pump<T>(ChannelReader<T> reader, Func<T, IO<Unit>> sink, Origin<T> origin) =>
        IO.liftVAsync(env => reader.ReadAsync(env.Token))
            .Bind(item => Guarded(sink, item, origin))
            .Repeat();

    private static IO<Unit> Guarded<T>(Func<T, IO<Unit>> sink, T item, Origin<T> origin) =>
        sink(item).IfFail(error => IO.lift(() => origin.Lost(new DeliveryLoss.SinkFault(origin.Source(item), error))));

    private static IO<Unit> Closing(Seq<IO<Unit>> steps, Action<DeliveryLoss> lost) =>
        steps.TraverseM(step => step.IfFail(error => IO.lift(() => lost(new DeliveryLoss.DetachFault(error)))))
            .As()
            .Map(static _ => unit);

    private static Disposal Closer(Seq<IO<Unit>> steps, Action<DeliveryLoss> lost) =>
        new(() => _ = Closing(steps, lost).RunSafe());

    private sealed record Route<T>(Func<T, IO<Unit>> Deliver, IO<Unit> Close);

    private sealed record Origin<T>(Func<T, DeliverySource> Source, Action<DeliveryLoss> Lost);

    // --- [OBSERVATION]
    public static IO<IDisposable> Observe(EventScope scope, Seq<EventKind> kinds, Delivery<DocEvent> delivery, Action<DeliveryLoss> lost) =>
        from supported in IO.lift(() => Supported(kinds, delivery))
        from route in Open(delivery, new Origin<DocEvent>(static item => new DeliverySource.Event(item.Kind), lost))
        from attached in GeometryOps.OnFailure(
            Events.AttachAll(kinds.Map(kind => Events.Attach(kind, scope, route.Deliver, error => lost(new DeliveryLoss.CallbackFault(kind, error))))),
            Closing(Seq(route.Close), lost))
        select (IDisposable)Closer(Seq(route.Close, IO.lift(attached.Dispose)), lost);

    public static IO<IDisposable> ObserveFile(WatchedFile file, TimeSpan debounce, TimeProvider clock, Capacity bound, Delivery<FileBatch> delivery, Action<DeliveryLoss> lost) =>
        from positive in IO.lift(() => Invalid.Unless(debounce > TimeSpan.Zero, nameof(debounce)))
        from route in Open(delivery, new Origin<FileBatch>(_ => new DeliverySource.File(file.Path), lost))
        from close in GeometryOps.OnFailure(Watched(file, debounce, clock, bound, route, lost), Closing(Seq(route.Close), lost))
        select (IDisposable)Closer(Seq(close, route.Close), lost);

    private static Fin<Unit> Supported(Seq<EventKind> kinds, Delivery<DocEvent> delivery) =>
        kinds.Filter(static kind => kind.Category == EventCategory.Draw)
            .TraverseM(kind => delivery.Switch<EventKind, Fin<Unit>>(
                kind,
                inline: static (drawn, _) => new UnboundedChannel(drawn),
                idle: static (drawn, _) => new UnboundedChannel(drawn),
                channel: static (drawn, channel) => channel.Mode is ChannelMode.Unbounded ? new UnboundedChannel(drawn) : unit))
            .As()
            .Map(static _ => unit);

    private static IO<IO<Unit>> Watched(WatchedFile file, TimeSpan debounce, TimeProvider clock, Capacity bound, Route<FileBatch> route, Action<DeliveryLoss> lost) =>
        from source in IO.lift(() => new FileSystemWatcher(file.Directory, file.Name) { NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size })
        from watch in GeometryOps.OnFailure(IO.lift(() => new Watch(source, file, debounce, clock, bound, route, lost)), IO.lift(source.Dispose))
        from attached in GeometryOps.OnFailure(
            Events.AttachAll(Seq(
                Events.Attach<FileSystemEventHandler>(h => source.Changed += h, h => source.Changed -= h, watch.Edited),
                Events.Attach<FileSystemEventHandler>(h => source.Created += h, h => source.Created -= h, watch.Edited),
                Events.Attach<FileSystemEventHandler>(h => source.Deleted += h, h => source.Deleted -= h, watch.Edited),
                Events.Attach<RenamedEventHandler>(h => source.Renamed += h, h => source.Renamed -= h, watch.Renamed),
                Events.Attach<ErrorEventHandler>(h => source.Error += h, h => source.Error -= h, watch.Errored))),
            watch.Close(Thinktecture.Empty.Disposable()))
        from enabled in GeometryOps.OnFailure(IO.lift(() => { source.EnableRaisingEvents = true; }), watch.Close(attached))
        select watch.Close(attached);

    private sealed class Watch {
        private readonly Lock gate = new();

        private readonly FileSystemWatcher source;

        private readonly WatchedFile file;

        private readonly TimeSpan debounce;

        private readonly Capacity bound;

        private readonly Route<FileBatch> route;

        private readonly Action<DeliveryLoss> lost;

        private readonly ITimer timer;

        private Option<FileBatch> batch = Some(FileBatch.Empty);

        public Watch(FileSystemWatcher source, WatchedFile file, TimeSpan debounce, TimeProvider clock, Capacity bound, Route<FileBatch> route, Action<DeliveryLoss> lost) {
            this.source = source;
            this.file = file;
            this.debounce = debounce;
            this.bound = bound;
            this.route = route;
            this.lost = lost;
            timer = clock.CreateTimer(_ => Flushed(), state: null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Edited(object? sender, FileSystemEventArgs args) =>
            Noted(new FileChange(args.ChangeType, args.FullPath, Option<string>.None));

        public void Renamed(object? sender, RenamedEventArgs args) =>
            Noted(new FileChange(args.ChangeType, args.FullPath, Some(args.OldFullPath)));

        public void Errored(object? sender, ErrorEventArgs args) =>
            lost(new DeliveryLoss.FileFault(file.Path, Error.New(args.GetException())));

        public IO<Unit> Close(IDisposable attached) =>
            from closed in IO.lift(() => {
                lock (gate) {
                    batch = Option<FileBatch>.None;
                    _ = timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                }
                source.EnableRaisingEvents = false;
                attached.Dispose();
                source.Dispose();
            })
            from stopped in IO.liftAsync(async _ => {
                await timer.DisposeAsync().ConfigureAwait(false);
                return unit;
            })
            select stopped;

        private void Noted(FileChange change) {
            lock (gate) {
                if (batch.Case is not FileBatch held)
                    return;
                bool overflowed = held.Changes.Count >= bound;
                batch = Some(overflowed ? held with { Overflow = held.Overflow + 1 } : held with { Changes = held.Changes.Add(change) });
                if (overflowed)
                    lost(new DeliveryLoss.FileOverflow(file.Path));
                _ = timer.Change(debounce, Timeout.InfiniteTimeSpan);
            }
        }

        private void Flushed() {
            Option<FileBatch> taken;
            lock (gate) {
                taken = batch;
                batch = batch.Map(static _ => FileBatch.Empty);
            }
            _ = taken.Iter(held => _ = route.Deliver(held).RunSafe());
        }
    }
}
