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

    /// Bounded native capsule: resolves a definition, projects through `project` under Op.Catch,
    /// guarantees mutation cannot escape the closure.
    public Fin<T> Use<T>(DefinitionRef refer, Func<InstanceDefinition, Fin<T>> project, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(project).ToFin(Fail: op.InvalidInput())
            .Bind(valid => Operations.Resolve(table: Document.InstanceDefinitions, refer: refer, key: op)
                .Bind(pair => op.Catch(() => valid(arg: pair.Live))));
    }

    /// Linked-archive watcher. Returns Subscription IDisposable, not MutationReceipt.
    public Fin<Subscription> Watch(ArchivePath path, WatchPolicy? policy = null) =>
        Operations.AttachWatcher(owner: this, path: path, policy: policy ?? WatchPolicy.Default);

    /// Document-level linked-update policy.
    public Fin<Unit> SetLinkedPolicy(LinkedPolicy policy, Op? key = null) =>
        Op.Of(name: nameof(SetLinkedPolicy)).Catch(() => {
            Document.LinkedInstanceDefinitionUpdate = policy.Native;
            return Fin.Succ(value: unit);
        });

    /// Per-definition opt-out for nested linked resolution (paired with SetLinkedPolicy).
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
        // [BOUNDARY ADAPTER — IDisposable contract; the single permitted switch statement here.]
        switch (this) {
            case Empty: break;
            case Atomic a: a.Detach(); break;
            case Composite c: _ = c.Members.Iter(static m => m.Dispose()); break;
        }
    }

    /// Monoid algebra — identity = Nothing; associative; flattens nested Composites left-biased.
    /// (Cannot formally implement Monoid&lt;Subscription&gt; trait: case-record `Empty` name collides
    /// with the trait's static `Empty` property — `|` + Nothing provide the same algebra.)
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
// [DEFERRED — plan §9] VersionedStore is the shared versioned-cache primitive. Promote to
// a Rasm.Rhino.Caching namespace when Layers/Materials/Hatches/Linetypes need the same shape;
// the type itself is namespace-agnostic — only its placement is Blocks-internal today.
[StructLayout(LayoutKind.Auto)]
public readonly record struct CacheKey(uint Serial, Guid DefId);

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

    /// Borrow: returns Some(value) only if entry exists AND version matches; on success RefCount++.
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

    /// Release: returns Some(value) when refcount transitions to 0 — caller disposes outside Swap.
    /// Capture/reclaim pattern: Swap retries on contention; the captured reference reflects only
    /// the winning attempt because entries are immutable records (each retry sees same Value field).
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

    /// Invalidate: atomic version-bump + filter; stale-insert protection guaranteed by the
    /// VersionAtInsert tag carried on each Entry.
    /// [DEFERRED — plan §9] Filter is O(N). Partition-by-versionId
    /// (HashMap&lt;Guid, HashMap&lt;TKey, Entry&gt;&gt;) makes invalidate O(1) lookup + O(1) replace.
    /// Defer until profile shows allocation pressure at >1K cached entries.
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
    private static readonly VersionedStore<CacheKey, Bitmap> store = new(versionKey: static k => k.DefId);

    [SuppressMessage(category: "Reliability", checkId: "CA2000", Justification = "PreviewHandle release path disposes the bitmap.")]
    internal static Fin<PreviewHandle> Acquire(RhinoDoc doc, InstanceDefinition def, PreviewSpec spec, Op key) {
        CacheKey k = new(Serial: doc.RuntimeSerialNumber, DefId: def.Id);
        return store.Borrow(key: k) switch {
            { IsSome: true, Case: Bitmap bmp } => Fin.Succ(value: Handle(key: k, bitmap: bmp)),
            _ => RhinoUi.OnUiThread(run: () => Render(def: def, spec: spec, key: k, op: key)),
        };
    }

    private static Fin<PreviewHandle> Render(InstanceDefinition def, PreviewSpec spec, CacheKey key, Op op) =>
        op.Catch(() => ResolveDisplayMode(refer: spec.ResolvedMode, op: op).Bind(displayId =>
            RenderNative(def: def, spec: spec, displayId: displayId) switch {
                Bitmap bmp => Fin.Succ(value: Handle(key: key, bitmap: store.Insert(key: key, value: bmp))),
                _ => Fin.Fail<PreviewHandle>(error: op.InvalidResult()),
            }));

    private static Fin<Guid> ResolveDisplayMode(DisplayModeRef refer, Op op) =>
        refer switch {
            DisplayModeRef.WireframeCase => Fin.Succ(value: Guid.Empty),
            DisplayModeRef.ById r => Fin.Succ(value: r.Id),
            DisplayModeRef.ByName r => Optional(DisplayModeDescription.FindByName(englishName: r.Name))
                .Map(static desc => desc.Id)
                .ToFin(Fail: op.InvalidInput()),
            _ => Fin.Fail<Guid>(error: op.InvalidInput()),
        };

    private static Bitmap? RenderNative(InstanceDefinition def, PreviewSpec spec, Guid displayId) {
        Size size = new(width: spec.Width, height: spec.Height);
        IsometricCamera camera = spec.Isometric ? IsometricCamera.Northeast : IsometricCamera.None;
        return def.CreatePreviewBitmap(
            displayModeId: displayId,
            viewportProjection: DefinedViewportProjection.Perspective,
            isometricCamera: camera,
            drawDecorations: spec.DrawDecorations,
            bitmapSize: size,
            applyDpiScaling: spec.ApplyDpiScaling);
    }

    private static PreviewHandle Handle(CacheKey key, Bitmap bitmap) =>
        new(bitmap: bitmap, release: _ => Reclaim(key: key));

    private static Unit Reclaim(CacheKey key) =>
        store.Release(key: key).Match(
            Some: static bmp => { bmp.Dispose(); return unit; },
            None: static () => unit);

    internal static Unit Invalidate(Guid defId) => store.Invalidate(versionId: defId);
    internal static Unit EvictDoc(uint serial) => store.EvictWhere(predicate: k => k.Serial == serial);
}

// --- [COMPOSITION] [IDLE] -----------------------------------------------------------------
internal static class IdlePump {
    private static readonly Atom<Seq<(RhinoDoc Document, Func<RhinoDoc, Fin<DocumentReceipt>> Work)>> queue =
        Atom(value: Seq<(RhinoDoc, Func<RhinoDoc, Fin<DocumentReceipt>>)>());
    private static int hooked;   // CAS gate: 0 = not hooked, 1 = hooked

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

    private static void EnsureHook() {
        // [BOUNDARY ADAPTER — one-shot CAS gate; canonical .NET primitive over Atom.Swap which
        //  returns the new value (not previous) and cannot distinguish first-caller from contenders.]
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
        // [BOUNDARY ADAPTER — time-bounded queue drain; the only imperative loop in this file.]
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
    private static int hooked;   // CAS gate (one-shot wire of native events)

    [ThreadStatic]
    private static bool dispatching;   // re-entrancy guard for synchronous (Immediate) subscribers

    internal static Subscription Attach(RhinoDoc doc, Action<BlockTableEvent> handler, BlockSubscriptionPolicy policy) {
        EnsureHook();
        Subscriber sub = new(Id: Guid.NewGuid(), Handler: handler, Policy: policy);
        uint serial = doc.RuntimeSerialNumber;
        // Inline upsert: single pattern-match over current bucket, no auxiliary helper.
        _ = bySerial.Swap(f: map => map.AddOrUpdate(key: serial, value: map.Find(key: serial) switch {
            { IsSome: true, Case: Seq<Subscriber> existing } => existing.Add(value: sub),
            _ => Seq(sub),
        }));
        return Subscription.Of(detach: () => Detach(serial: serial, id: sub.Id));
    }

    private static void EnsureHook() {
        // [BOUNDARY ADAPTER — once-only attach via CAS gate; wires both table events and doc lifecycle.]
        if (Interlocked.CompareExchange(location1: ref hooked, value: 1, comparand: 0) != 0) return;
        RhinoDoc.InstanceDefinitionTableEvent += static (_, args) => Dispatch(args: args);
        RhinoDoc.CloseDocument += static (_, args) => OnDocClose(serial: args.Document?.RuntimeSerialNumber ?? 0u);
    }

    private static Unit OnDocClose(uint serial) {
        _ = SnapshotVault.EvictDoc(serial: serial);
        _ = PreviewVault.EvictDoc(serial: serial);
        _ = Operations.InvalidateContentIndex(serial: serial);
        _ = bySerial.Swap(f: map => map.Remove(key: serial));
        return unit;
    }

    /// [BOUNDARY ADAPTER — InstanceDefinitionTableEvent re-entry: an Immediate subscriber that
    /// mutates the table during handling fires Dispatch recursively. The ThreadStatic gate
    /// drops recursive deliveries on the same thread; the originating mutation issues its own
    /// table event for the inner change, observed after the outer dispatch unwinds.]
    private static void Dispatch(InstanceDefinitionTableEventArgs args) {
        if (dispatching) return;
        dispatching = true;
        try {
            uint serial = args.Document?.RuntimeSerialNumber ?? 0u;
            BlockTableEvent snapshot = BlockTableEvent.From(args: args);
            _ = Invalidate(serial: serial, snapshot: snapshot);
            Seq<Subscriber> subs = bySerial.Value.Find(key: serial).IfNone(noneValue: Seq<Subscriber>());
            if (subs.IsEmpty) return;
            // Inline filter predicate (was Allow helper) — single call-site, no abstraction value.
            _ = subs
                .Filter(s => s.Policy.Filter switch {
                    { IsSome: true, Case: Func<BlockTableEvent, bool> pred } => pred(arg: snapshot),
                    _ => true,
                })
                .Iter(s => Deliver(sub: s, snapshot: snapshot, serial: serial));
        } finally { dispatching = false; }
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
            // Sorted: cache is keyed by Guid not Index; sort-only re-numbering preserves cache validity.
            InstanceDefinitionTableEventType.Sorted => unit,
            InstanceDefinitionTableEventType.Deleted or InstanceDefinitionTableEventType.Modified =>
                snapshot.Old.Case switch {
                    Definition old => InvalidateOne(serial: serial, defId: old.Id.Value),
                    _ => unit,
                },
            _ => unit,
        };

    private static Unit InvalidateOne(uint serial, Guid defId) {
        _ = PreviewVault.Invalidate(defId: defId);
        _ = SnapshotVault.Invalidate(defId: defId);
        _ = Operations.InvalidateContentIndex(serial: serial);
        return unit;
    }

    private sealed record Subscriber(Guid Id, Action<BlockTableEvent> Handler, BlockSubscriptionPolicy Policy);
}
