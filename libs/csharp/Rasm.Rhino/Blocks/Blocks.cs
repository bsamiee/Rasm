using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
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

    public Fin<MutationReceipt> Run(BlockMutation op, Op? key = null) =>
        Optional(op).ToFin(Fail: key.OrDefault().InvalidInput())
            .Bind(valid => Operations.RunMutation(op: valid, owner: this));

    public Fin<BlockResult> Run(BlockQuery op, Op? key = null) =>
        Optional(op).ToFin(Fail: key.OrDefault().InvalidInput())
            .Bind(valid => Operations.RunQuery(op: valid, owner: this));

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
[StructLayout(LayoutKind.Auto)]
public readonly record struct CacheKey(uint Serial, Guid DefId);

[StructLayout(LayoutKind.Auto)]
internal readonly record struct PreviewKey(uint Serial, Guid DefId, PreviewFingerprint Spec, uint Version);

internal sealed class VersionedStore<TKey, TValue>(Func<TKey, Guid> versionKey)
    where TKey : notnull
    where TValue : notnull {

    private sealed record State(
        HashMap<TKey, Entry> Entries,
        HashMap<Guid, uint> Versions);

    private sealed record Entry(TValue Value, uint VersionAtInsert, int RefCount);

    private readonly Func<TKey, Guid> versionKey = versionKey ?? throw new ArgumentNullException(paramName: nameof(versionKey));
    private readonly Atom<State> store = Atom(value: new State(Entries: HashMap<TKey, Entry>(), Versions: HashMap<Guid, uint>()));

    public Option<TValue> Find(TKey key) {
        State s = store.Value;
        return s.Entries.Find(key: key) switch {
            { IsSome: true, Case: Entry e } when e.VersionAtInsert == s.Versions.Find(key: versionKey(arg: key)).IfNone(noneValue: 0u) =>
                Some(value: e.Value),
            _ => Option<TValue>.None,
        };
    }

    public TValue Insert(TKey key, TValue value, int refCount = 1) {
        _ = store.Swap(f: s => {
            uint v = s.Versions.Find(key: versionKey(arg: key)).IfNone(noneValue: 0u);
            return s with { Entries = s.Entries.AddOrUpdate(key: key, value: new Entry(value, v, refCount)) };
        });
        return value;
    }

    /// Some(value) only when entry exists AND version matches; on hit RefCount++.
    public Option<TValue> Borrow(TKey key) {
        State result = store.Swap(f: s => s.Entries.Find(key: key) switch {
            { IsSome: true, Case: Entry e } when e.VersionAtInsert == s.Versions.Find(key: versionKey(arg: key)).IfNone(noneValue: 0u) =>
                s with { Entries = s.Entries.AddOrUpdate(key: key, value: e with { RefCount = e.RefCount + 1 }) },
            _ => s,
        });
        return result.Entries.Find(key: key)
            .Bind(e => e.VersionAtInsert == result.Versions.Find(key: versionKey(arg: key)).IfNone(noneValue: 0u)
                ? Some(value: e.Value) : Option<TValue>.None);
    }

    /// Some(value) only when refcount transitions to 0 — caller disposes outside Swap.
    public Option<TValue> Release(TKey key) {
        TValue? captured = default;
        _ = store.Swap(f: s => s.Entries.Find(key: key) switch {
            { IsSome: true, Case: Entry e } when e.RefCount > 1 =>
                s with { Entries = s.Entries.AddOrUpdate(key: key, value: e with { RefCount = e.RefCount - 1 }) },
            { IsSome: true, Case: Entry e } => CaptureAndRemove(s: s, key: key, value: e.Value, into: ref captured),
            _ => s,
        });
        return captured is null ? Option<TValue>.None : Some(value: captured);
    }

    private static State CaptureAndRemove(State s, TKey key, TValue value, ref TValue? into) {
        into = value;
        return s with { Entries = s.Entries.Remove(key: key) };
    }

    /// Atomic version-bump + filter; VersionAtInsert tags guard stale inserts.
    public Unit Invalidate(Guid versionId) {
        Func<TKey, Guid> vk = versionKey;
        _ = store.Swap(f: s => s with {
            Versions = s.Versions.AddOrUpdate(key: versionId, value: s.Versions.Find(key: versionId).IfNone(noneValue: 0u) + 1u),
            Entries = s.Entries.Filter((k, _) => vk(arg: k) != versionId),
        });
        return unit;
    }

    public Unit EvictWhere(Func<TKey, bool> predicate) {
        _ = store.Swap(f: s => s with { Entries = s.Entries.Filter((k, _) => !predicate(arg: k)) });
        return unit;
    }
}

internal static class SnapshotVault {
    private static readonly VersionedStore<CacheKey, Definition> store = new(versionKey: static k => k.DefId);

    internal static Option<Definition> Find(uint docSerial, DefinitionId id) =>
        store.Find(key: new CacheKey(Serial: docSerial, DefId: id.Value));

    internal static Unit Upsert(uint docSerial, Definition definition) {
        _ = store.Insert(key: new CacheKey(Serial: docSerial, DefId: definition.Id.Value), value: definition);
        return unit;
    }

    internal static Unit Invalidate(Guid defId) => store.Invalidate(versionId: defId);
    internal static Unit EvictDoc(uint serial) => store.EvictWhere(predicate: k => k.Serial == serial);
}

internal static class PreviewVault {
    private sealed record State(
        HashMap<PreviewKey, Entry> Entries,
        HashMap<Guid, uint> Versions);

    private sealed record Entry(Bitmap Bitmap, int RefCount, bool Stale);

    private static readonly Atom<State> store = Atom(value: new State(Entries: HashMap<PreviewKey, Entry>(), Versions: HashMap<Guid, uint>()));

    [SuppressMessage(category: "Reliability", checkId: "CA2000", Justification = "PreviewHandle release path disposes the bitmap.")]
    internal static Fin<PreviewHandle> Acquire(RhinoDoc doc, InstanceDefinition def, PreviewSpec spec, Op key) {
        PreviewKey k = Key(doc: doc, def: def, spec: spec);
        return Borrow(key: k) switch {
            { IsSome: true, Case: Bitmap bmp } => Fin.Succ(value: Handle(key: k, bitmap: bmp)),
            _ => RhinoUi.OnUiThread(run: () => Render(def: def, spec: spec, key: k, op: key)),
        };
    }

    private static PreviewKey Key(RhinoDoc doc, InstanceDefinition def, PreviewSpec spec) {
        State s = store.Value;
        uint version = s.Versions.Find(key: def.Id).IfNone(noneValue: 0u);
        return new PreviewKey(Serial: doc.RuntimeSerialNumber, DefId: def.Id, Spec: spec.Fingerprint, Version: version);
    }

    private static Fin<PreviewHandle> Render(InstanceDefinition def, PreviewSpec spec, PreviewKey key, Op op) =>
        op.Catch(() => spec.ResolvedMode.Resolve(op: op).Bind(displayId =>
            RenderNative(def: def, spec: spec, displayId: displayId) switch {
                Bitmap bmp => Fin.Succ(value: StoreRendered(key: key, rendered: bmp)),
                _ => Fin.Fail<PreviewHandle>(error: op.InvalidResult()),
            }));

    private static Bitmap? RenderNative(InstanceDefinition def, PreviewSpec spec, Guid displayId) {
        Size size = new(width: spec.Width, height: spec.Height);
        return def.CreatePreviewBitmap(
            displayModeId: displayId,
            viewportProjection: spec.Projection,
            isometricCamera: spec.Camera,
            drawDecorations: spec.DrawDecorations,
            bitmapSize: size,
            applyDpiScaling: spec.ApplyDpiScaling);
    }

    private static Option<Bitmap> Borrow(PreviewKey key) {
        State after = store.Swap(f: s => s.Entries.Find(key: key) switch {
            { IsSome: true, Case: Entry e } when !e.Stale && key.Version == s.Versions.Find(key: key.DefId).IfNone(noneValue: 0u) =>
                s with { Entries = s.Entries.AddOrUpdate(key: key, value: e with { RefCount = e.RefCount + 1 }) },
            _ => s,
        });
        return after.Entries.Find(key: key).Bind(e => !e.Stale && key.Version == after.Versions.Find(key: key.DefId).IfNone(noneValue: 0u)
            ? Some(value: e.Bitmap)
            : Option<Bitmap>.None);
    }

    private static PreviewHandle StoreRendered(PreviewKey key, Bitmap rendered) {
        Bitmap? dispose = null;
        Bitmap selected = rendered;
        bool cached = false;
        _ = store.Swap(f: s => key.Version == s.Versions.Find(key: key.DefId).IfNone(noneValue: 0u)
            ? s.Entries.Find(key: key) switch {
                { IsSome: true, Case: Entry e } when !e.Stale =>
                    CaptureRendered(state: s, key: key, existing: e, rendered: rendered, selected: ref selected, dispose: ref dispose, cached: ref cached),
                { IsSome: true, Case: Entry e } when e.Stale && e.RefCount > 0 =>
                    CaptureRejected(state: s, rendered: rendered, dispose: ref dispose),
                _ => CaptureInserted(state: s, key: key, rendered: rendered, cached: ref cached),
            }
            : CaptureRejected(state: s, rendered: rendered, dispose: ref dispose));
        dispose?.Dispose();
        return cached
            ? Handle(key: key, bitmap: selected)
            : new PreviewHandle(bitmap: selected, release: static handle => handle.Bitmap.Dispose());
    }

    private static State CaptureRendered(State state, PreviewKey key, Entry existing, Bitmap rendered, ref Bitmap selected, ref Bitmap? dispose, ref bool cached) {
        selected = existing.Bitmap;
        dispose = rendered;
        cached = true;
        return state with { Entries = state.Entries.AddOrUpdate(key: key, value: existing with { RefCount = existing.RefCount + 1 }) };
    }

    private static State CaptureInserted(State state, PreviewKey key, Bitmap rendered, ref bool cached) {
        cached = true;
        return state with { Entries = state.Entries.AddOrUpdate(key: key, value: new Entry(Bitmap: rendered, RefCount: 1, Stale: false)) };
    }

    private static State CaptureRejected(State state, Bitmap rendered, ref Bitmap? dispose) {
        dispose = null;
        return state;
    }

    private static PreviewHandle Handle(PreviewKey key, Bitmap bitmap) =>
        new(bitmap: bitmap, release: _ => Reclaim(key: key));

    private static Unit Reclaim(PreviewKey key) {
        Bitmap? dispose = null;
        _ = store.Swap(f: s => s.Entries.Find(key: key) switch {
            { IsSome: true, Case: Entry e } when e.RefCount > 1 =>
                s with { Entries = s.Entries.AddOrUpdate(key: key, value: e with { RefCount = e.RefCount - 1 }) },
            { IsSome: true, Case: Entry e } =>
                CaptureDisposal(state: s, key: key, bitmap: e.Bitmap, dispose: ref dispose),
            _ => s,
        });
        dispose?.Dispose();
        return unit;
    }

    private static State CaptureDisposal(State state, PreviewKey key, Bitmap bitmap, ref Bitmap? dispose) {
        dispose = bitmap;
        return state with { Entries = state.Entries.Remove(key: key) };
    }

    internal static Unit Invalidate(Guid defId) {
        Seq<Bitmap> dispose = Seq<Bitmap>();
        _ = store.Swap(f: s => {
            HashMap<PreviewKey, Entry> entries = s.Entries.Map((k, e) => k.DefId == defId ? e with { Stale = true } : e);
            HashMap<PreviewKey, Entry> kept = entries.Filter((k, e) => k.DefId != defId || e.RefCount > 0);
            dispose = entries.Filter((k, e) => k.DefId == defId && e.RefCount <= 0).Values.Map(static e => e.Bitmap).ToSeq();
            return s with {
                Versions = s.Versions.AddOrUpdate(key: defId, value: s.Versions.Find(key: defId).IfNone(noneValue: 0u) + 1u),
                Entries = kept,
            };
        });
        _ = dispose.Iter(static bmp => bmp.Dispose());
        return unit;
    }

    internal static Unit EvictDoc(uint serial) {
        Seq<Bitmap> dispose = Seq<Bitmap>();
        _ = store.Swap(f: s => {
            HashMap<PreviewKey, Entry> entries = s.Entries.Map((k, e) => k.Serial == serial ? e with { Stale = true } : e);
            HashMap<PreviewKey, Entry> kept = entries.Filter((k, e) => k.Serial != serial || e.RefCount > 0);
            dispose = entries.Filter((k, e) => k.Serial == serial && e.RefCount <= 0).Values.Map(static e => e.Bitmap).ToSeq();
            return s with { Entries = kept };
        });
        _ = dispose.Iter(static bmp => bmp.Dispose());
        return unit;
    }
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

    private static int DrainUntilDeadline(TimeSpan budget) {
        long start = clock.GetTimestamp();
        Seq<(RhinoDoc Document, Func<RhinoDoc, Fin<DocumentReceipt>> Work)> snapshot = queue.Value;
        int taken = 0;
        // BOUNDARY ADAPTER — idle pumping is budgeted host integration.
        foreach ((RhinoDoc Document, Func<RhinoDoc, Fin<DocumentReceipt>> Work) in snapshot) {
            if (clock.GetElapsedTime(startingTimestamp: start) >= budget) break;
            _ = Op.Of(name: nameof(Drain)).Catch(() => Work(arg: Document));
            taken++;
        }
        return taken;
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
        _ = Operations.InvalidateContentIndex(serial: serial);
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
            _ = Invalidate(serial: serial, snapshot: snapshot);
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

    private static Unit Invalidate(uint serial, BlockTableEvent snapshot) =>
        snapshot.Kind switch {
            InstanceDefinitionTableEventType.Sorted => unit,
            _ => InvalidateDefinitions(serial: serial, snapshot: snapshot),
        };

    private static Unit InvalidateDefinitions(uint serial, BlockTableEvent snapshot) {
        _ = Operations.InvalidateContentIndex(serial: serial);
        return Seq(snapshot.Old, snapshot.New)
            .Choose(static candidate => candidate)
            .Map(static d => d.Id.Value)
            .Distinct()
            .Iter(defId => InvalidateOne(serial: serial, defId: defId));
    }

    private static Unit InvalidateOne(uint serial, Guid defId) {
        _ = PreviewVault.Invalidate(defId: defId);
        _ = SnapshotVault.Invalidate(defId: defId);
        return unit;
    }

    private sealed record Subscriber(Guid Id, Action<BlockTableEvent> Handler, BlockSubscriptionPolicy Policy);
}
