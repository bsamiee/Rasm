using System.Diagnostics.CodeAnalysis;
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
        EventBridge.EnsureHook();
    }

    public RhinoDoc Document { get; }
    public RunMode Mode { get; }

    public static RhinoBlocks Live(RhinoDoc document, RunMode mode = RunMode.Interactive) =>
        new(document: document, mode: mode);

    public Fin<BlockOutcome> Run(BlockOp op, Op? key = null) {
        Op runKey = key.OrDefault();
        return Optional(op).ToFin(Fail: runKey.InvalidInput())
            .Bind(valid => ExecuteRun(op: valid, runKey: runKey));
    }

    private Fin<BlockOutcome> ExecuteRun(BlockOp op, Op runKey) {
        Fin<BlockOutcome> Work() => runKey.Catch(() => Operations.Run(op: op, owner: this));
        return RhinoUi.WhenUiBound(uiBound: op.RequiresUiThread(), run: Work, name: nameof(Run));
    }

    public Subscription Subscribe(Action<BlockTableEvent> handler, BlockSubscriptionPolicy? policy = null) {
        ArgumentNullException.ThrowIfNull(argument: handler);
        return EventBridge.Attach(doc: Document, handler: handler, policy: policy ?? BlockSubscriptionPolicy.Default);
    }

    public Fin<Unit> ScheduleIdle(Func<RhinoDoc, Fin<DocumentReceipt>> work, Op? key = null) =>
        Optional(work).ToFin(Fail: key.OrDefault().InvalidInput())
            .Map(valid => { _ = IdlePump.Enqueue(document: Document, work: valid); return unit; });

    /// Bounded native capsule: resolves the definition then projects under Op.Catch.
    public Fin<T> Use<T>(DefinitionRef refer, Func<InstanceDefinition, Fin<T>> project, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(project).ToFin(Fail: op.InvalidInput())
            .Bind(valid => Operations.Resolve(table: Document.InstanceDefinitions, refer: refer, key: op)
                .Bind(pair => op.Catch(() => valid(arg: pair.Live))));
    }

    public Fin<Subscription> Watch(ArchivePath path, WatchPolicy? policy = null) =>
        (policy ?? WatchPolicy.Default)
            .Admit(key: Op.Of(name: nameof(Watch)))
            .Bind(valid => Operations.AttachWatcher(owner: this, path: path, policy: valid));

    public Fin<Unit> SetLinkedPolicy(LinkedPolicy policy, Op? key = null) =>
        Op.Of(name: nameof(SetLinkedPolicy)).Catch(() => {
            Document.LinkedInstanceDefinitionUpdate = policy.Native;
            return Fin.Succ(value: unit);
        });

    /// Per-definition opt-out paired with SetLinkedPolicy.
    public Fin<Unit> SetSkipNestedLinked(DefinitionRef refer, bool value, Op? key = null) =>
        Use(refer: refer, project: def => Op.Of(name: nameof(SetSkipNestedLinked)).Catch(() => {
            def.SkipNestedLinkedDefinitions = value;
            return Fin.Succ(value: unit);
        }), key: key);

    internal Fin<PreviewHandle> AcquirePreview(InstanceDefinition definition, PreviewSpec spec, Op key) =>
        PreviewVault.Acquire(doc: Document, def: definition, spec: spec, key: key);
}

// --- [TYPES] [SUBSCRIPTION] ---------------------------------------------------------------
[Union]
[SuppressMessage(category: "Design", checkId: "CA1063", Justification = "[Union]-generated case records are sealed; Dispose dispatches polymorphically over the closed sum.")]
[SuppressMessage(category: "Usage", checkId: "CA1816", Justification = "No finalizer on Union cases; SuppressFinalize unnecessary.")]
public abstract partial record Subscription : IDisposable {
    private Subscription() { }
    public sealed record Empty() : Subscription;
    public sealed record Atomic(Action Detach) : Subscription;
    public sealed record Composite(Seq<Subscription> Members) : Subscription;

    public void Dispose() {
        // BOUNDARY ADAPTER — IDisposable requires terminal imperative dispatch.
        switch (this) {
            case Empty: break;
            case Atomic a: a.Detach(); break;
            case Composite c: _ = c.Members.Iter(static m => m.Dispose()); break;
        }
    }

    /// Monoid algebra (identity = Nothing). Cannot implement Monoid&lt;Subscription&gt; — the
    /// case-record `Empty` name collides with the trait's `Empty` static.
    public static Subscription operator |(Subscription a, Subscription b) =>
        (a, b) switch {
            (Empty, _) => b,
            (_, Empty) => a,
            (Composite ca, Composite cb) => new Composite(Members: ca.Members + cb.Members),
            (Composite ca, _) => new Composite(Members: ca.Members.Add(value: b)),
            (_, Composite cb) => new Composite(Members: Seq(a) + cb.Members),
            _ => new Composite(Members: Seq(a, b)),
        };

    public static Subscription Of(Action detach) => new Atomic(Detach: detach);
    public static readonly Subscription Nothing = new Empty();
}

// --- [COMPOSITION] [CACHE] ----------------------------------------------------------------
internal enum RefCacheMode { Snapshot, Preview }

/// Unified ref-count cache: snapshot mode hard-evicts on invalidate; preview mode retains stale entries while borrowed.
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

    /// Caller hands off `rendered`; cache returns the live entry (hit → return existing, miss → insert rendered), plus the
    /// reject-dispose slot for the value the cache did NOT retain (so caller can dispose safely).
    internal (TValue Selected, bool Cached, TValue? RejectDispose) Store(TKey key, TValue rendered) {
        TValue selected = rendered;
        TValue? rejectDispose = default;
        bool cached = false;
        _ = atom.Swap(f: s => {
            uint current = s.Versions.Find(key: versionId(arg: key)).IfNone(noneValue: 0u);
            bool versionStale = versionTag is not null && versionTag(arg: key) != current;
            return versionStale
                ? (rejectDispose = rendered, s).Item2
                : s.Entries.Find(key: key) switch {
                    { IsSome: true, Case: Entry e } when Fresh(key: key, entry: e, state: s) =>
                        (selected = e.Value, rejectDispose = rendered, cached = true,
                         s with { Entries = s.Entries.AddOrUpdate(key: key, value: e with { RefCount = e.RefCount + 1 }) }).Item4,
                    { IsSome: true, Case: Entry e } when e.Stale && e.RefCount > 0 =>
                        (rejectDispose = rendered, s).Item2,
                    _ => (cached = true,
                          s with { Entries = s.Entries.AddOrUpdate(key: key, value: new Entry(Value: rendered, VersionAtInsert: current, RefCount: 1, Stale: false)) }).Item2,
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
            _ => RhinoUi.OnUiThread(run: () => Render(def: def, spec: spec, key: cacheKey, op: key)),
        };
    }

    private static (uint Serial, Guid DefId, ulong Spec, uint Version) Key(RhinoDoc doc, InstanceDefinition def, PreviewSpec spec) =>
        (Serial: doc.RuntimeSerialNumber, DefId: def.Id, Spec: spec.Fingerprint.Value, Version: store.VersionOf(id: def.Id));

    private static Fin<PreviewHandle> Render(InstanceDefinition def, PreviewSpec spec, (uint Serial, Guid DefId, ulong Spec, uint Version) key, Op op) =>
        op.Catch(() => spec.ResolvedMode.Resolve(op: op).Bind(displayId =>
            RenderNative(def: def, spec: spec, displayId: displayId) switch {
                Bitmap bmp => Fin.Succ(value: StoreRendered(key: key, rendered: bmp)),
                _ => Fin.Fail<PreviewHandle>(error: op.InvalidResult()),
            }));

    private static Bitmap? RenderNative(InstanceDefinition def, PreviewSpec spec, Guid displayId) {
        Size size = new(width: spec.Width, height: spec.Height);
        return spec.HighlightMemberId.Case switch {
            Guid memberId => def.CreatePreviewBitmap(
                definitionObjectId: memberId,
                viewportProjection: spec.Projection,
                displayMode: DisplayModeRef.NativeMode(displayId: displayId),
                bitmapSize: size,
                applyDpiScaling: spec.ApplyDpiScaling),
            _ => def.CreatePreviewBitmap(
                displayModeId: displayId,
                viewportProjection: spec.Projection,
                isometricCamera: spec.Camera,
                drawDecorations: spec.DrawDecorations,
                bitmapSize: size,
                applyDpiScaling: spec.ApplyDpiScaling),
        };
    }

    private static PreviewHandle StoreRendered((uint Serial, Guid DefId, ulong Spec, uint Version) key, Bitmap rendered) {
        (Bitmap selected, bool cached, Bitmap? rejectDispose) = store.Store(key: key, rendered: rendered);
        rejectDispose?.Dispose();
        return cached
            ? Handle(key: key, bitmap: selected)
            : new PreviewHandle(bitmap: selected, release: static handle => handle.Bitmap.Dispose());
    }

    private static PreviewHandle Handle((uint Serial, Guid DefId, ulong Spec, uint Version) key, Bitmap bitmap) =>
        new(bitmap: bitmap, release: _ => store.Release(key: key));

    internal static Unit Invalidate(Guid defId) => store.InvalidateDef(defId: defId);
    internal static Unit EvictDoc(uint serial) => store.EvictDoc(serial: serial);
}

// --- [COMPOSITION] [IDLE] -----------------------------------------------------------------
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
        // BOUNDARY ADAPTER — Interlocked owns one-time Rhino idle subscription.
        if (Interlocked.CompareExchange(location1: ref hooked, value: 1, comparand: 0) != 0) return;
        RhinoApp.Idle += static (_, _) => Drain();
    }

    private static void Drain() {
        int taken = DrainUntilDeadline(budget: budget);
        _ = queue.Swap(f: live => toSeq(live.Skip(count: taken)));
    }

    /// BOUNDARY ADAPTER — idle pumping is budgeted host integration; TakeWhile early-exits on deadline.
    private static int DrainUntilDeadline(TimeSpan budget) {
        long start = clock.GetTimestamp();
        Seq<Fin<DocumentReceipt>> drained = queue.Value
            .TakeWhile(_ => clock.GetElapsedTime(startingTimestamp: start) < budget)
            .Map(item => Op.Of(name: nameof(Drain)).Catch(() => item.Work(arg: item.Document)))
            .ToSeq();
        return drained.Count;
    }
}

// --- [COMPOSITION] [EVENTS] ---------------------------------------------------------------
internal static class EventBridge {
    private static readonly Atom<HashMap<uint, Seq<Subscriber>>> bySerial =
        Atom(value: HashMap<uint, Seq<Subscriber>>());
    private static int hooked;

    /// Per-thread re-entry guard; concurrent dispatch from distinct threads proceeds in parallel.
    /// The check-then-Swap is race-safe: only the entering thread can insert its own tid.
    private static readonly Atom<LanguageExt.HashSet<int>> dispatching = Atom(value: LanguageExt.HashSet<int>.Empty);

    internal static Subscription Attach(RhinoDoc doc, Action<BlockTableEvent> handler, BlockSubscriptionPolicy policy) {
        EnsureHook();
        Subscriber sub = new(Id: Guid.NewGuid(), Handler: handler, Policy: policy);
        uint serial = doc.RuntimeSerialNumber;
        _ = bySerial.Swap(f: map => map.AddOrUpdate(key: serial, value: map.Find(key: serial) switch {
            { IsSome: true, Case: Seq<Subscriber> existing } => existing.Add(value: sub),
            _ => Seq(sub),
        }));
        return Subscription.Of(detach: () => Detach(serial: serial, id: sub.Id));
    }

    internal static void EnsureHook() {
        // BOUNDARY ADAPTER — Interlocked owns one-time Rhino event subscription.
        if (Interlocked.CompareExchange(location1: ref hooked, value: 1, comparand: 0) != 0) return;
        RhinoDoc.InstanceDefinitionTableEvent += static (_, args) => Dispatch(args: args);
        RhinoDoc.CloseDocument += static (_, args) => OnDocClose(serial: args.DocumentSerialNumber);
    }

    private static Unit OnDocClose(uint serial) {
        _ = SnapshotVault.EvictDoc(serial: serial);
        _ = PreviewVault.EvictDoc(serial: serial);
        _ = ContentIndex.EvictDoc(serial: serial);
        // When the last subscriber bucket drops, any tids in `dispatching` must be stale
        // leftovers from a bypassed `finally` — safe to flush; recycled tids can't re-block.
        if (bySerial.Swap(f: map => map.Remove(key: serial)).IsEmpty)
            _ = dispatching.Swap(f: static _ => []);
        return unit;
    }

    /// Re-entry guard: Immediate subscribers that mutate during handling fire Dispatch
    /// recursively; the inner mutation's own table event arrives after outer unwinds.
    private static void Dispatch(InstanceDefinitionTableEventArgs args) {
        int tid = System.Environment.CurrentManagedThreadId;
        // BOUNDARY ADAPTER — Rhino events can re-enter while subscribers mutate the table.
        if (dispatching.Value.Contains(key: tid)) return;
        _ = dispatching.Swap(f: set => set.Add(key: tid));
        try {
            uint serial = args.Document?.RuntimeSerialNumber ?? 0u;
            BlockTableEvent snapshot = BlockTableEvent.From(args: args);
            _ = Invalidate(document: args.Document, serial: serial, snapshot: snapshot);
            Seq<Subscriber> subs = bySerial.Value.Find(key: serial).IfNone(noneValue: Seq<Subscriber>());
            if (subs.IsEmpty) return;
            _ = subs
                .Filter(s => s.Policy.Filter switch {
                    { IsSome: true, Case: Func<BlockTableEvent, bool> pred } => pred(arg: snapshot),
                    _ => true,
                })
                .Iter(s => Deliver(sub: s, snapshot: snapshot, serial: serial));
        } finally { _ = dispatching.Swap(f: set => set.Remove(key: tid)); }
    }

    private static void Deliver(Subscriber sub, BlockTableEvent snapshot, uint serial) {
        void Run() => _ = Op.Of(name: nameof(Deliver)).Catch(() => {
            sub.Handler(obj: snapshot);
            return Fin.Succ(value: unit);
        });
        if (!sub.Policy.DeferToIdle) { Run(); return; }
        RhinoDoc? doc = RhinoDoc.FromRuntimeSerialNumber(serialNumber: serial);
        if (doc is null) return;
        _ = IdlePump.Enqueue(document: doc, work: _ => { Run(); return Fin.Succ(value: DocumentReceipt.Empty); });
    }

    private static Unit Detach(uint serial, Guid id) {
        _ = bySerial.Swap(f: map => map.Find(key: serial) switch {
            { IsSome: true, Case: Seq<Subscriber> existing } =>
                map.AddOrUpdate(key: serial, value: existing.Filter(s => s.Id != id)),
            _ => map,
        });
        return unit;
    }

    /// Sort-only events touch no definition state; mutation/content events flush ContentIndex and per-def caches.
    private static Unit Invalidate(RhinoDoc? document, uint serial, BlockTableEvent snapshot) {
        if (snapshot.Kind == InstanceDefinitionTableEventType.Sorted) return unit;
        _ = ContentIndex.Invalidate(serial: serial, doc: document, snapshot: snapshot);
        return Seq(snapshot.Old, snapshot.New)
            .Choose(static candidate => candidate)
            .Map(static d => d.Id.Value)
            .Distinct()
            .Iter(defId => Operations.InvalidateDefinition(
                defId: defId, doc: Optional(RhinoDoc.FromRuntimeSerialNumber(serialNumber: serial))));
    }

    private sealed record Subscriber(Guid Id, Action<BlockTableEvent> Handler, BlockSubscriptionPolicy Policy);
}
