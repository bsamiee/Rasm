using System.Drawing;
using Rasm.Rhino.Commands;
using Rasm.Rhino.UI;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Blocks;

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class RhinoBlocks {
    private RhinoBlocks(RhinoDoc document, RunMode mode) {
        Document = document ?? throw new ArgumentNullException(paramName: nameof(document));
        Mode = mode;
    }

    public RhinoDoc Document { get; }
    public RunMode Mode { get; }

    public static RhinoBlocks Live(RhinoDoc document, RunMode mode = RunMode.Interactive) =>
        new(document: document, mode: mode);

    public Fin<MutationReceipt> Run(BlockMutationOp op, Op? key = null) =>
        Optional(op)
            .ToFin(Fail: key.OrDefault().InvalidInput())
            .Bind(valid => BlockOperations.RunMutation(op: valid, owner: this));

    public Fin<BlockResult> Run(BlockQueryOp op, Op? key = null) =>
        Optional(op)
            .ToFin(Fail: key.OrDefault().InvalidInput())
            .Bind(valid => BlockOperations.RunQuery(op: valid, owner: this));

    public Fin<TProjected> Run<TProjected>(BlockMutationOp op, Func<MutationReceipt, Fin<TProjected>> project, Op? key = null) =>
        Optional(project).ToFin(Fail: key.OrDefault().InvalidInput()).Bind(map => Run(op: op, key: key).Bind(map));

    public Fin<TProjected> Run<TProjected>(BlockQueryOp op, Func<BlockResult, Fin<TProjected>> project, Op? key = null) =>
        Optional(project).ToFin(Fail: key.OrDefault().InvalidInput()).Bind(map => Run(op: op, key: key).Bind(map));

    public IDisposable Subscribe(Action<BlockTableEvent> handler, BlockSubscriptionPolicy? policy = null) {
        ArgumentNullException.ThrowIfNull(argument: handler);
        return BlockEventBridge.Attach(document: Document, handler: handler, policy: policy ?? BlockSubscriptionPolicy.Default);
    }

    public Fin<Unit> ScheduleIdle(Func<RhinoDoc, Fin<DocumentReceipt>> work, Op? key = null) =>
        Optional(work).ToFin(Fail: key.OrDefault().InvalidInput()).Map(valid => {
            _ = BlockIdlePump.Enqueue(document: Document, work: valid);
            return unit;
        });

    internal Fin<PreviewHandle> AcquirePreview(InstanceDefinition definition, PreviewSpec spec, Op key) =>
        BlockPreviewCache.Acquire(document: Document, definition: definition, spec: spec, key: key);
}

// --- [COMPOSITION] [DEFINITION CACHE] -----------------------------------------------------
internal static class BlockDefinitionCache {
    private sealed record CacheEntry(BlockSnapshot Snapshot, uint Counter);

    private static readonly Atom<HashMap<(uint DocSerial, Guid DefId), CacheEntry>> store =
        Atom(value: HashMap<(uint DocSerial, Guid DefId), CacheEntry>());

    private static readonly Atom<HashMap<Guid, uint>> counters =
        Atom(value: HashMap<Guid, uint>());

    internal static Option<BlockSnapshot> Find(uint docSerial, DefinitionId id) {
        uint expected = counters.Value.Find(key: id.Value).IfNone(noneValue: 0u);
        return store.Value.Find(key: (docSerial, id.Value)) switch {
            { IsSome: true, Case: CacheEntry entry } when entry.Counter == expected => Some(entry.Snapshot),
            _ => Option<BlockSnapshot>.None,
        };
    }

    internal static Unit Upsert(uint docSerial, BlockSnapshot snapshot) {
        uint current = counters.Value.Find(key: snapshot.Id.Value).IfNone(noneValue: 0u);
        _ = store.Swap(f: m => m.AddOrUpdate(key: (docSerial, snapshot.Id.Value), value: new CacheEntry(Snapshot: snapshot, Counter: current)));
        return unit;
    }

    internal static Unit Invalidate(Guid defId) {
        Guid target = defId;
        _ = counters.Swap(f: m => m.AddOrUpdate(key: target, value: m.Find(key: target).IfNone(noneValue: 0u) + 1u));
        return unit;
    }

    internal static Unit Evict(uint docSerial, DefinitionId id) {
        _ = Invalidate(defId: id.Value);
        (uint DocSerial, Guid DefId) target = (docSerial, id.Value);
        _ = store.Swap(f: m => m.Remove(key: target));
        return unit;
    }

    internal static Unit EvictDocument(uint docSerial) {
        uint target = docSerial;
        _ = store.Swap(f: m => m.Filter((k, _) => k.DocSerial != target));
        return unit;
    }
}

// --- [COMPOSITION] [PREVIEW CACHE] --------------------------------------------------------
internal static class BlockPreviewCache {
    private sealed record CacheEntry(Bitmap Bitmap, int RefCount, uint Counter);

    private static readonly Atom<HashMap<(uint DocSerial, Guid DefId), CacheEntry>> store =
        Atom(value: HashMap<(uint DocSerial, Guid DefId), CacheEntry>());

    private static readonly Atom<HashMap<Guid, uint>> counters =
        Atom(value: HashMap<Guid, uint>());

    [System.Diagnostics.CodeAnalysis.SuppressMessage(category: "Reliability", checkId: "CA2000", Justification = "PreviewHandle ownership transfers to caller; cache owns bitmap.")]
    internal static Fin<PreviewHandle> Acquire(RhinoDoc document, InstanceDefinition definition, PreviewSpec spec, Op key) {
        uint counter = counters.Value.Find(key: definition.Id).IfNone(noneValue: 0u);
        (uint serial, Guid defId) = (document.RuntimeSerialNumber, definition.Id);
        return store.Value.Find(key: (serial, defId)) switch {
            { IsSome: true, Case: CacheEntry entry } when entry.Counter == counter =>
                Fin.Succ(value: Borrow(serial: serial, defId: defId, bitmap: entry.Bitmap)),
            _ => RhinoUi.OnUiThread(run: () => Render(definition: definition, spec: spec, serial: serial, counter: counter, key: key)),
        };
    }

    private static Fin<PreviewHandle> Render(InstanceDefinition definition, PreviewSpec spec, uint serial, uint counter, Op key) =>
        key.Catch(() => RenderBitmap(definition: definition, spec: spec) switch {
            Bitmap bmp => Fin.Succ(value: Insert(serial: serial, defId: definition.Id, bitmap: bmp, counter: counter)),
            _ => Fin.Fail<PreviewHandle>(error: key.InvalidResult()),
        });

    private static Bitmap? RenderBitmap(InstanceDefinition definition, PreviewSpec spec) {
        Size size = new(width: spec.Width, height: spec.Height);
        IsometricCamera camera = spec.Isometric ? IsometricCamera.Northeast : IsometricCamera.None;
        Guid displayMode = spec.DisplayModeId.IfNone(noneValue: Guid.Empty);
        return definition.CreatePreviewBitmap(
            displayModeId: displayMode,
            viewportProjection: DefinedViewportProjection.Perspective,
            isometricCamera: camera,
            drawDecorations: spec.DrawDecorations,
            bitmapSize: size,
            applyDpiScaling: false);
    }

    private static PreviewHandle Insert(uint serial, Guid defId, Bitmap bitmap, uint counter) {
        (uint DocSerial, Guid DefId) target = (serial, defId);
        _ = store.Swap(f: m => m.AddOrUpdate(key: target, value: new CacheEntry(Bitmap: bitmap, RefCount: 1, Counter: counter)));
        return new PreviewHandle(bitmap: bitmap, release: _ => Release(serial: serial, defId: defId));
    }

    private static PreviewHandle Borrow(uint serial, Guid defId, Bitmap bitmap) {
        (uint DocSerial, Guid DefId) target = (serial, defId);
        _ = store.Swap(f: m => m.UpdateIf(key: target, update: static entry => entry with { RefCount = entry.RefCount + 1 }));
        return new PreviewHandle(bitmap: bitmap, release: _ => Release(serial: serial, defId: defId));
    }

    private static Unit Release(uint serial, Guid defId) {
        (uint DocSerial, Guid DefId) target = (serial, defId);
        _ = store.Swap(f: m => m.Find(key: target) switch {
            { IsSome: true, Case: CacheEntry entry } when entry.RefCount > 1 =>
                m.AddOrUpdate(key: target, value: entry with { RefCount = entry.RefCount - 1 }),
            { IsSome: true, Case: CacheEntry entry } => DisposeAndRemove(map: m, key: target, entry: entry),
            _ => m,
        });
        return unit;
    }

    internal static Unit Invalidate(uint serial, Guid defId) {
        Guid idTarget = defId;
        _ = counters.Swap(f: m => m.AddOrUpdate(key: idTarget, value: m.Find(key: idTarget).IfNone(noneValue: 0u) + 1u));
        (uint DocSerial, Guid DefId) target = (serial, defId);
        _ = store.Swap(f: m => m.Find(key: target) switch {
            { IsSome: true, Case: CacheEntry entry } => DisposeAndRemove(map: m, key: target, entry: entry),
            _ => m,
        });
        return unit;
    }

    private static HashMap<(uint DocSerial, Guid DefId), CacheEntry> DisposeAndRemove(
        HashMap<(uint DocSerial, Guid DefId), CacheEntry> map,
        (uint DocSerial, Guid DefId) key,
        CacheEntry entry) {
        // [BOUNDARY ADAPTER — Bitmap.Dispose inside swap; eviction is idempotent.]
        entry.Bitmap.Dispose();
        return map.Remove(key: key);
    }
}

// --- [COMPOSITION] [IDLE PUMP] ------------------------------------------------------------
internal static class BlockIdlePump {
    private static readonly Atom<Seq<(RhinoDoc Document, Func<RhinoDoc, Fin<DocumentReceipt>> Work)>> queue =
        Atom(value: Seq<(RhinoDoc, Func<RhinoDoc, Fin<DocumentReceipt>>)>());
    private static readonly Lock hookLock = new();
    private static bool hooked;

    private static readonly TimeSpan DefaultBudget = TimeSpan.FromMilliseconds(value: 8);
    private static TimeProvider clock = TimeProvider.System;
    private static TimeSpan budget = DefaultBudget;

    internal static void ConfigureClock(TimeProvider provider) => clock = provider ?? TimeProvider.System;

    internal static void ConfigureBudget(TimeSpan value) => budget = value > TimeSpan.Zero ? value : DefaultBudget;

    internal static Unit Enqueue(RhinoDoc document, Func<RhinoDoc, Fin<DocumentReceipt>> work) {
        EnsureHook();
        (RhinoDoc, Func<RhinoDoc, Fin<DocumentReceipt>>) entry = (document, work);
        _ = queue.Swap(f: current => current.Add(value: entry));
        return unit;
    }

    private static void EnsureHook() {
        lock (hookLock) {
            // [BOUNDARY ADAPTER — single Idle subscription per process; set-once mutation.]
            if (hooked) return;
            RhinoApp.Idle += static (_, _) => Drain();
            hooked = true;
        }
    }

    private static void Drain() {
        int taken = DrainUntilDeadline(budget: budget);
        _ = queue.Swap(f: live => toSeq(live.Skip(count: taken)));
    }

    private static int DrainUntilDeadline(TimeSpan budget) {
        long start = clock.GetTimestamp();
        Seq<(RhinoDoc Document, Func<RhinoDoc, Fin<DocumentReceipt>> Work)> snapshot = queue.Value;
        int taken = 0;
        // [BOUNDARY ADAPTER — time-bounded queue drain; the only imperative loop in this file.]
        foreach ((RhinoDoc Document, Func<RhinoDoc, Fin<DocumentReceipt>> Work) in snapshot) {
            if (clock.GetElapsedTime(startingTimestamp: start) >= budget) break;
            _ = Op.Of(name: nameof(Drain)).Catch(() => Work(arg: Document));
            taken++;
        }
        return taken;
    }
}

// --- [COMPOSITION] [EVENT BRIDGE] ---------------------------------------------------------
internal static class BlockEventBridge {
    private static readonly Atom<HashMap<uint, Seq<Subscriber>>> bySerial =
        Atom(value: HashMap<uint, Seq<Subscriber>>());
    private static readonly Lock hookLock = new();
    private static bool hooked;

    internal static IDisposable Attach(RhinoDoc document, Action<BlockTableEvent> handler, BlockSubscriptionPolicy policy) {
        EnsureHook();
        Subscriber subscriber = new(Id: Guid.NewGuid(), Handler: handler, Policy: policy);
        uint serial = document.RuntimeSerialNumber;
        _ = bySerial.Swap(f: current => current.AddOrUpdate(key: serial, value: ExtendList(current: current, serial: serial, subscriber: subscriber)));
        return new Subscription(serial: serial, id: subscriber.Id);
    }

    private static Seq<Subscriber> ExtendList(HashMap<uint, Seq<Subscriber>> current, uint serial, Subscriber subscriber) =>
        current.Find(key: serial) switch {
            { IsSome: true, Case: Seq<Subscriber> existing } => existing.Add(value: subscriber),
            _ => Seq(subscriber),
        };

    private static void EnsureHook() {
        lock (hookLock) {
            if (hooked) return;
            RhinoDoc.InstanceDefinitionTableEvent += static (_, args) => Dispatch(args: args);
            hooked = true;
        }
    }

    private static void Dispatch(InstanceDefinitionTableEventArgs args) {
        uint serial = args.Document?.RuntimeSerialNumber ?? 0u;
        Seq<Subscriber> subscribers = bySerial.Value.Find(key: serial).IfNone(noneValue: Seq<Subscriber>());
        if (subscribers.IsEmpty) return;
        BlockTableEvent snapshot = BlockTableEvent.From(args: args);
        _ = Invalidate(serial: serial, snapshot: snapshot);
        _ = subscribers
            .Filter(s => AllowEvent(subscriber: s, snapshot: snapshot))
            .Iter(s => DispatchOne(subscriber: s, snapshot: snapshot, serial: serial));
    }

    private static bool AllowEvent(Subscriber subscriber, BlockTableEvent snapshot) =>
        subscriber.Policy.Filter switch {
            { IsSome: true, Case: Func<BlockTableEvent, bool> predicate } => predicate(arg: snapshot),
            _ => true,
        };

    private static void DispatchOne(Subscriber subscriber, BlockTableEvent snapshot, uint serial) {
        // [BOUNDARY ADAPTER — McNeel rule: defer mutation in event handlers to RhinoApp.Idle.]
        RhinoDoc? document = RhinoDoc.FromRuntimeSerialNumber(serialNumber: serial);
        if (document is null) return;
        _ = BlockIdlePump.Enqueue(document: document, work: _ => Invoke(handler: subscriber.Handler, snapshot: snapshot));
    }

    private static Fin<DocumentReceipt> Invoke(Action<BlockTableEvent> handler, BlockTableEvent snapshot) =>
        Op.Of(name: nameof(Invoke)).Catch(() => {
            handler(obj: snapshot);
            return Fin.Succ(value: DocumentReceipt.Empty);
        });

    private static Unit Invalidate(uint serial, BlockTableEvent snapshot) =>
        snapshot.Kind switch {
            InstanceDefinitionTableEventType.Deleted or InstanceDefinitionTableEventType.Modified =>
                snapshot.Old switch {
                    { IsSome: true, Case: BlockArchiveSnapshot old } => InvalidateCaches(serial: serial, defId: old.Id.Value),
                    _ => unit,
                },
            InstanceDefinitionTableEventType.Sorted => FullEvict(serial: serial),
            _ => unit,
        };

    private static Unit InvalidateCaches(uint serial, Guid defId) {
        _ = BlockPreviewCache.Invalidate(serial: serial, defId: defId);
        _ = DefinitionId.From(value: defId).ToOption() switch {
            { IsSome: true, Case: DefinitionId id } => BlockDefinitionCache.Evict(docSerial: serial, id: id),
            _ => unit,
        };
        return unit;
    }

    private static Unit FullEvict(uint serial) {
        _ = BlockDefinitionCache.EvictDocument(docSerial: serial);
        return unit;
    }

    private sealed record Subscriber(Guid Id, Action<BlockTableEvent> Handler, BlockSubscriptionPolicy Policy);

    private sealed class Subscription(uint serial, Guid id) : IDisposable {
        private bool disposed;
        public void Dispose() {
            // [BOUNDARY ADAPTER — IDisposable; idempotent removal from process-static map.]
            if (disposed) return;
            disposed = true;
            uint serialTarget = serial;
            Guid idTarget = id;
            _ = bySerial.Swap(f: current => current.Find(key: serialTarget) switch {
                { IsSome: true, Case: Seq<Subscriber> existing } =>
                    current.AddOrUpdate(key: serialTarget, value: existing.Filter(s => s.Id != idTarget)),
                _ => current,
            });
        }
    }
}
