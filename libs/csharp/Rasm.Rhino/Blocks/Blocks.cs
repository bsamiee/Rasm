using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI;
using Rhino.DocObjects.Tables;
namespace Rasm.Rhino.Blocks;

// --- [TYPES] ------------------------------------------------------------------------------
internal enum RefCacheMode { Snapshot, Preview }

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class RhinoBlocks {
    private RhinoBlocks(RhinoDoc document, RunMode mode) {
        Document = document ?? throw new ArgumentNullException(paramName: nameof(document));
        Mode = mode;
        EventBridge.EnsureHook();
    }

    public RhinoDoc Document { get; }
    public RunMode Mode { get; }
    internal bool RunScriptEcho => Mode == RunMode.Interactive;

    public static RhinoBlocks Live(RhinoDoc document, RunMode mode = RunMode.Interactive) =>
        new(document: document, mode: mode);

    public Fin<BlockOutcome> Run(BlockOp op, Op? key = null) {
        Op runKey = key.OrDefault();
        return Optional(op).ToFin(Fail: runKey.InvalidInput())
            .Bind(valid => {
                Fin<BlockOutcome> Work() => runKey.Catch(() => Operations.Run(op: valid, owner: this));
                return RhinoUi.DispatchThread(uiBound: valid.RequiresUiThread(), mode: Mode, run: Work, name: nameof(Run));
            });
    }

    public Subscription Subscribe(Action<BlockTableEvent> handler, BlockSubscriptionPolicy? policy = null) {
        ArgumentNullException.ThrowIfNull(argument: handler);
        return EventBridge.Attach(doc: Document, handler: handler, policy: policy ?? BlockSubscriptionPolicy.Default);
    }

    public Fin<Unit> ScheduleIdle(Func<RhinoDoc, Fin<DocumentReceipt>> work, Op? key = null) =>
        Optional(work).ToFin(Fail: key.OrDefault().InvalidInput())
            .Map(valid => { _ = IdlePump.Enqueue(document: Document, work: valid); return unit; });
    public Fin<T> Use<T>(DefinitionRef refer, Func<InstanceDefinition, Fin<T>> project, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(project).ToFin(Fail: op.InvalidInput())
            .Bind(valid => Operations.Resolve(table: Document.InstanceDefinitions, refer: refer, key: op)
                .Bind(pair => op.Catch(() => valid(arg: pair.Live))));
    }

    public Fin<Subscription> Watch(ArchivePath path, WatchPolicy? policy = null) =>
        (policy ?? WatchPolicy.Default)
            .Admit(key: Op.Of(name: nameof(Watch)))
            .Bind(valid => WatchBus.SubscribeFile(
                path: path.Value,
                debounce: valid.Debounce,
                clock: valid.EffectiveClock,
                sink: () => Document switch {
                    { IsAvailable: true, IsClosing: false } doc =>
                        Fin.Succ(value: IdlePump.Enqueue(document: doc, work: live => Operations.RefreshLinkedDocument(
                            doc: live,
                            filter: BlockFilter.ArchivesOnly(Seq(path.Value)),
                            policy: LinkRefreshPolicy.Changed,
                            batch: BatchPolicy.Default,
                            key: Op.Of(name: nameof(BlockOp.RefreshLinks))))),
                    _ => Fin.Succ(value: unit),
                }));

    internal Fin<PreviewHandle> AcquirePreview(InstanceDefinition definition, PreviewSpec spec, Op key) =>
        PreviewVault.Acquire(doc: Document, def: definition, spec: spec, key: key);
}

internal sealed class RefCache<TKey, TValue>(
    RefCacheMode mode,
    Func<TKey, Guid> versionId,
    Func<TKey, uint>? versionTag,
    Func<TKey, uint>? docSerial,
    Action<TValue>? dispose)
    where TKey : notnull
    where TValue : class {

    private sealed record Entry(TValue Value, uint VersionAtInsert, int RefCount, bool Stale);

    private sealed record State(HashMap<TKey, Entry> Entries, HashMap<Guid, uint> Versions);

    private readonly Atom<State> atom = Atom(value: new State(Entries: HashMap<TKey, Entry>(), Versions: HashMap<Guid, uint>()));

    internal uint VersionOf(Guid id) => atom.Value.Versions.Find(key: id).IfNone(noneValue: 0u);

    private bool Fresh(TKey key, Entry entry, State state) =>
        !entry.Stale && CurrentVersion(key: key, state: state) == entry.VersionAtInsert;

    private uint CurrentVersion(TKey key, State state) =>
        state.Versions.Find(key: versionId(arg: key)).IfNone(noneValue: 0u);

    internal Option<TValue> Find(TKey key) {
        State s = atom.Value;
        return s.Entries.Find(key: key).Bind(e => Fresh(key: key, entry: e, state: s) ? Some(value: e.Value) : Option<TValue>.None);
    }

    internal TValue Insert(TKey key, TValue value, int refCount = 1) {
        _ = atom.Swap(f: s => {
            uint v = s.Versions.Find(key: versionId(arg: key)).IfNone(noneValue: 0u);
            return s with { Entries = s.Entries.AddOrUpdate(key: key, value: new Entry(Value: value, VersionAtInsert: v, RefCount: refCount, Stale: false)) };
        });
        return value;
    }

    internal Option<TValue> Borrow(TKey key) {
        State after = atom.Swap(f: s => s.Entries.Find(key: key) switch {
            { IsSome: true, Case: Entry e } when Fresh(key: key, entry: e, state: s) =>
                s with { Entries = s.Entries.AddOrUpdate(key: key, value: e with { RefCount = e.RefCount + 1 }) },
            _ => s,
        });
        return after.Entries.Find(key: key).Bind(e => Fresh(key: key, entry: e, state: after) ? Some(value: e.Value) : Option<TValue>.None);
    }
    internal (Option<TValue> Selected, bool Cached, TValue? RejectDispose) Store(TKey key, TValue rendered) {
        Option<TValue> selected = Option<TValue>.None;
        TValue? rejectDispose = default;
        bool cached = false;
        _ = atom.Swap(f: s => {
            uint current = s.Versions.Find(key: versionId(arg: key)).IfNone(noneValue: 0u);
            bool versionStale = versionTag is not null && versionTag(arg: key) != current;
            return versionStale
                ? (rejectDispose = rendered, s).s
                : s.Entries.Find(key: key) switch {
                    { IsSome: true, Case: Entry e } when Fresh(key: key, entry: e, state: s) =>
                        (selected = Some(e.Value), rejectDispose = rendered, cached = true,
                         s with { Entries = s.Entries.AddOrUpdate(key: key, value: e with { RefCount = e.RefCount + 1 }) }).Item4,
                    { IsSome: true, Case: Entry e } when e.Stale && e.RefCount > 0 =>
                        (rejectDispose = rendered, s).s,
                    _ => (selected = Some(rendered), cached = true,
                          s with { Entries = s.Entries.AddOrUpdate(key: key, value: new Entry(Value: rendered, VersionAtInsert: current, RefCount: 1, Stale: false)) }).Item3,
                };
        });
        return (selected, cached, rejectDispose);
    }

    internal Unit Release(TKey key) {
        TValue? captured = default;
        _ = atom.Swap(f: s => s.Entries.Find(key: key) switch {
            { IsSome: true, Case: Entry e } when e.RefCount > 1 =>
                s with { Entries = s.Entries.AddOrUpdate(key: key, value: e with { RefCount = e.RefCount - 1 }) },
            { IsSome: true, Case: Entry e } =>
                (captured = e.Value, s with { Entries = s.Entries.Remove(key: key) }).Item2,
            _ => s,
        });
        _ = Optional(captured).Iter(value => dispose?.Invoke(obj: value));
        return unit;
    }

    internal Unit InvalidateDef(Guid defId) =>
        mode == RefCacheMode.Preview
            ? ApplyMask(mask: k => versionId(arg: k) == defId, bump: Some(value: defId))
            : HardInvalidate(defId: defId);

    internal Unit EvictDoc(uint serial) =>
        docSerial is null
            ? unit
            : ApplyMask(mask: k => docSerial(arg: k) == serial, bump: Option<Guid>.None);

    internal Unit EvictWhere(Func<TKey, bool> predicate) {
        Seq<TValue> captured = Seq<TValue>();
        _ = atom.Swap(f: s => {
            captured = s.Entries.Filter((k, _) => predicate(arg: k)).Values.Map(static e => e.Value).ToSeq();
            return s with { Entries = s.Entries.Filter((k, _) => !predicate(arg: k)) };
        });
        _ = captured.Iter(value => dispose?.Invoke(obj: value));
        return unit;
    }

    private Unit HardInvalidate(Guid defId) {
        Func<TKey, Guid> vid = versionId;
        _ = atom.Swap(f: s => s with {
            Versions = s.Versions.AddOrUpdate(key: defId, value: s.Versions.Find(key: defId).IfNone(noneValue: 0u) + 1u),
            Entries = s.Entries.Filter((k, _) => vid(arg: k) != defId),
        });
        return unit;
    }

    private Unit ApplyMask(Func<TKey, bool> mask, Option<Guid> bump) {
        Seq<TValue> captured = Seq<TValue>();
        _ = atom.Swap(f: s => {
            HashMap<TKey, Entry> marked = s.Entries.Map((k, e) => mask(arg: k) ? e with { Stale = true } : e);
            HashMap<TKey, Entry> kept = marked.Filter((k, e) => !mask(arg: k) || e.RefCount > 0);
            captured = marked.Filter((k, e) => mask(arg: k) && e.RefCount <= 0).Values.Map(static e => e.Value).ToSeq();
            return s with {
                Versions = bump.Case switch {
                    Guid id => s.Versions.AddOrUpdate(key: id, value: s.Versions.Find(key: id).IfNone(noneValue: 0u) + 1u),
                    _ => s.Versions,
                },
                Entries = kept,
            };
        });
        _ = captured.Iter(value => dispose?.Invoke(obj: value));
        return unit;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class PreviewVault {
    private static readonly RefCache<(uint Serial, Guid DefId, ulong Spec, uint Version), Bitmap> store = new(
        mode: RefCacheMode.Preview,
        versionId: static k => k.DefId,
        versionTag: static k => k.Version,
        docSerial: static k => k.Serial,
        dispose: static bmp => bmp.Dispose());

    [SuppressMessage(category: "Reliability", checkId: "CA2000", Justification = "PreviewHandle release path disposes the bitmap.")]
    internal static Fin<PreviewHandle> Acquire(RhinoDoc doc, InstanceDefinition def, PreviewSpec spec, Op key) {
        (uint Serial, Guid DefId, ulong Spec, uint Version) cacheKey = Key(doc: doc, def: def, spec: spec);
        return store.Borrow(key: cacheKey) switch {
            { IsSome: true, Case: Bitmap bmp } => Fin.Succ(value: Handle(key: cacheKey, bitmap: bmp)),
            _ => Render(def: def, spec: spec, key: cacheKey, op: key),
        };
    }

    private static (uint Serial, Guid DefId, ulong Spec, uint Version) Key(RhinoDoc doc, InstanceDefinition def, PreviewSpec spec) =>
        (Serial: doc.RuntimeSerialNumber, DefId: def.Id, Spec: spec.Fingerprint.Value, Version: store.VersionOf(id: def.Id));

    private static Fin<PreviewHandle> Render(InstanceDefinition def, PreviewSpec spec, (uint Serial, Guid DefId, ulong Spec, uint Version) key, Op op) =>
        op.Catch(() =>
            from displayId in spec.ResolvedMode.Resolve(op: op)
            from rendered in RenderNative(def: def, spec: spec, displayId: displayId, op: op)
            from stored in StoreRendered(key: key, rendered: rendered)
            select stored);

    private static Fin<Bitmap> RenderNative(InstanceDefinition def, PreviewSpec spec, Guid displayId, Op op) {
        Size size = new(width: spec.Width, height: spec.Height);
        return spec.HighlightMemberId.Case switch {
            Guid memberId =>
                from displayMode in DisplayModeRef.NativeMode(displayId: displayId, op: op)
                from bitmap in Optional(def.CreatePreviewBitmap(
                        definitionObjectId: memberId,
                        viewportProjection: spec.Projection,
                        displayMode: displayMode,
                        bitmapSize: size,
                        applyDpiScaling: spec.ApplyDpiScaling))
                    .ToFin(Fail: op.InvalidResult())
                select bitmap,
            _ => Optional(def.CreatePreviewBitmap(
                    displayModeId: displayId,
                    viewportProjection: spec.Projection,
                    isometricCamera: spec.Camera,
                    drawDecorations: spec.DrawDecorations,
                    bitmapSize: size,
                    applyDpiScaling: spec.ApplyDpiScaling))
                .ToFin(Fail: op.InvalidResult()),
        };
    }

    private static Fin<PreviewHandle> StoreRendered((uint Serial, Guid DefId, ulong Spec, uint Version) key, Bitmap rendered) {
        (Option<Bitmap> selected, bool cached, Bitmap? rejectDispose) = store.Store(key: key, rendered: rendered);
        rejectDispose?.Dispose();
        return selected.ToFin(Fail: Op.Of(name: nameof(StoreRendered)).InvalidResult())
            .Map(bitmap => cached
                ? Handle(key: key, bitmap: bitmap)
                : new PreviewHandle(bitmap: bitmap, release: static handle => handle.Bitmap.Dispose()));
    }

    private static PreviewHandle Handle((uint Serial, Guid DefId, ulong Spec, uint Version) key, Bitmap bitmap) =>
        new(bitmap: bitmap, release: _ => store.Release(key: key));

    internal static Unit Invalidate(Guid defId) => store.InvalidateDef(defId: defId);
    internal static Unit EvictDoc(uint serial) => store.EvictDoc(serial: serial);
}

internal static class SnapshotVault {
    private static readonly RefCache<(uint Serial, Guid DefId), Definition> store = new(
        mode: RefCacheMode.Snapshot,
        versionId: static k => k.DefId,
        versionTag: null,
        docSerial: static k => k.Serial,
        dispose: null);

    internal static Option<Definition> Find(uint docSerial, DefinitionId id) =>
        store.Find(key: (Serial: docSerial, DefId: id.Value));

    internal static Unit Upsert(uint docSerial, Definition definition) {
        _ = store.Insert(key: (Serial: docSerial, DefId: definition.Id.Value), value: definition);
        return unit;
    }

    internal static Unit Invalidate(Guid defId) => store.InvalidateDef(defId: defId);
    internal static Unit EvictDoc(uint serial) => store.EvictDoc(serial: serial);
}

// --- [COMPOSITION] ------------------------------------------------------------------------
internal static class EventBridge {
    private static readonly EventDispatcher<InstanceDefinitionTableEventArgs, BlockTableEvent> Dispatcher = new(
        hook: static h => RhinoDoc.InstanceDefinitionTableEvent += h,
        project: static args => (Serial: args.Document?.RuntimeSerialNumber ?? 0u, Event: BlockTableEvent.From(args: args)),
        prologue: static (serial, snapshot) => Invalidate(serial: serial, snapshot: snapshot),
        defer: static (serial, work) => Optional(RhinoDoc.FromRuntimeSerialNumber(serialNumber: serial))
            .Iter(doc => IdlePump.Enqueue(document: doc, work: _ => { work(); return Fin.Succ(value: DocumentReceipt.Empty); })));
    private static int closeHooked;

    internal static Subscription Attach(RhinoDoc doc, Action<BlockTableEvent> handler, BlockSubscriptionPolicy policy) {
        EnsureHook();
        return Dispatcher.Register(
            serial: doc.RuntimeSerialNumber,
            filter: policy.Filter.Case switch { Func<BlockTableEvent, bool> pred => pred, _ => static _ => true },
            sink: handler,
            deferral: policy.DeferToIdle ? DeferralPolicy.Idle : DeferralPolicy.Immediate);
    }

    internal static void EnsureHook() {
        _ = Dispatcher;
        if (Interlocked.CompareExchange(location1: ref closeHooked, value: 1, comparand: 0) != 0) return;
        RhinoDoc.CloseDocument += static (_, args) => OnDocClose(serial: args.DocumentSerialNumber);
    }

    private static Unit OnDocClose(uint serial) {
        _ = SnapshotVault.EvictDoc(serial: serial);
        _ = PreviewVault.EvictDoc(serial: serial);
        _ = ContentIndex.EvictDoc(serial: serial);
        return Dispatcher.DropSerial(serial: serial);
    }

    private static Unit Invalidate(uint serial, BlockTableEvent snapshot) {
        if (snapshot.Kind == InstanceDefinitionTableEventType.Sorted) return unit;
        RhinoDoc? document = RhinoDoc.FromRuntimeSerialNumber(serialNumber: serial);
        _ = ContentIndex.Invalidate(serial: serial, doc: document, snapshot: snapshot);
        return Seq(snapshot.Old, snapshot.New)
            .Choose(static candidate => candidate)
            .Map(static d => d.Id.Value)
            .Distinct()
            .Iter(defId => Operations.InvalidateDefinition(defId: defId, doc: Optional(document)));
    }
}

internal static class IdlePump {
    private static readonly Atom<Seq<(RhinoDoc Document, Func<RhinoDoc, Fin<DocumentReceipt>> Work)>> queue =
        Atom(value: Seq<(RhinoDoc, Func<RhinoDoc, Fin<DocumentReceipt>>)>());
    private static int hooked;

    private static readonly TimeSpan DefaultBudget = TimeSpan.FromMilliseconds(value: 8);
    private static TimeProvider clock = TimeProvider.System;
    private static TimeSpan budget = DefaultBudget;

    internal static void ConfigureClock(TimeProvider provider) => clock = provider ?? TimeProvider.System;

    internal static void ConfigureBudget(TimeSpan value) => budget = value > TimeSpan.Zero ? value : DefaultBudget;

    internal static Unit Enqueue(RhinoDoc document, Func<RhinoDoc, Fin<DocumentReceipt>> work) {
        EnsureHook();
        _ = queue.Swap(f: current => current.Add(value: (document, work)));
        return unit;
    }

    internal static void EnsureHook() {
        if (Interlocked.CompareExchange(location1: ref hooked, value: 1, comparand: 0) != 0) return;
        RhinoApp.Idle += static (_, _) => Drain();
    }

    private static void Drain() {
        int taken = DrainUntilDeadline(budget: budget);
        _ = queue.Swap(f: live => toSeq(live.Skip(count: taken)));
    }
    private static int DrainUntilDeadline(TimeSpan budget) {
        long start = clock.GetTimestamp();
        Seq<Fin<DocumentReceipt>> drained = queue.Value
            .TakeWhile(_ => clock.GetElapsedTime(startingTimestamp: start) < budget)
            .Map(item => item.Document is { IsAvailable: true, IsClosing: false }
                ? Op.Of(name: nameof(Drain)).Catch(() => item.Work(arg: item.Document))
                : Fin.Succ(value: DocumentReceipt.Empty))
            .Map(result => result.MapFail(error => {
                RhinoApp.WriteLine($"Rasm block idle refresh failed: {error.Message}");
                return error;
            }))
            .ToSeq();
        return drained.Count;
    }
}
