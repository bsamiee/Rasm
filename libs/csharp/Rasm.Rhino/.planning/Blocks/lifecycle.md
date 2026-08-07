# [RASM_RHINO_BLOCK_LIFECYCLE]

Preview lifecycle (`Rasm.Rhino.Blocks`) owns bitmap custody, versioned grants, document-scoped invalidation, linked-source refresh, and deterministic disposal. Host acquisition and disposal stay outside `Atom.Swap`; `Change` captures both swaps and every transition on one `Fin<VaultOutcome>` rail.

## [01]-[INDEX]

- [02]-[REFRESH_POLICY]: `RefreshPolicy`, `UpdateStyle`, `LinkSubject`, and `LinkWatchPolicy` partitioning matching versions by grant state across the lazy, eager, and drop rows, with `RefreshRefusal` and `RefreshDegrade` carrying every degrade as typed evidence.
- [03]-[PREVIEW_CUSTODY]: `DocSeal` and `PreviewKey` structural request identity and `PreviewGrant` the only bitmap window — a claim-gated borrow, never a handed-out handle — superseded versions moving atomically to retirement and `VaultOutcome` carrying leased transition products without captured mutation.
- [04]-[LIFECYCLE]: `BlockLifecycle` observing definition-table, worksession, and document-close facts through one atom transition owning enrolment, invalidation, and eviction, and `Lease` enrolling then reserving or rendering outside the atom before committing with its first grant.
- [05]-[SURFACE_LEDGER]: owner-to-ingress-to-state-to-egress roster across `BlockLifecycle`, `PreviewGrant`, and the policy owners.

## [02]-[REFRESH_POLICY]

`RefreshPolicy` partitions matching versions by grant state. Every row removes and closes zero-grant versions; `Lazy` keeps granted versions stale, `Eager` keeps them stale and regenerates them, and `Drop` moves them to retirement. A caller's row is a REQUEST: `Invalidate` resolves it per definition through `RefreshPolicy.Of` before its pure transition, so the sweep answers over live host state rather than the caller's wish.

- Law: grant state alone does not decide a refresh — `RefreshPolicy.Of` resolves over the PRODUCT of grant state and the two independent host discriminants a linked definition carries: the `SourceMode` row behind `InstanceDefinition.UpdateType` (`Static` embeds and never re-reads, `LinkedAndEmbedded` re-reads yet survives a missing source, `Linked` requires it) and the `UpdateStyle` row behind the document-scoped `RhinoDoc.LinkedInstanceDefinitionUpdate` policy. Regenerating a `Never` document's LINKED definition from a changed external file is exactly the case the host discriminant exists to foreclose, and a policy keyed on grant state alone cannot express it.
- Law: the three source rows answer independently and the vocabulary owns the answer — `SourceMode.Regenerates(styled, readable)` is the behavior column, so `Static` admits eager regeneration unconditionally (it re-renders embedded geometry and consults no external file), `LinkedAndEmbedded` admits it whenever the document style permits regardless of source readability (its embedded copy survives a missing source), and `Linked` requires both. An arm ladder over the product re-derives what the row already states and inverts the common case, which is the deleted form.
- Law: no raw host discriminant reaches a `LinkSubject` column — `UpdateStyle` re-closes the document update policy with its `Updates` column and `SourceHealth` (model.md) is the branch's one archive-status vocabulary with its `Stale`/`Broken` columns, so `Styled` and `Readable` read row behavior rather than re-spelling an `is-not-one-row` comparison or an or-chain a new host ordinal silently joins.
- Law: source AVAILABILITY is a third axis, not a failure — an eager row over a broken source degrades to the stale-keep arm with typed evidence instead of regenerating from nothing, and `SourceHealth` names no row for `NotALinkedInstanceDefinition`, so an unlinked definition carries `Option<SourceHealth>.None` — absence, not a row meaning "not applicable". `LinkSubject` closes the (source-mode × update-style × source-health) product as ROW DATA and `RefreshPolicy.Of` is a lookup over it, never a branch ladder at a call site.
- Law: a degrade is EVIDENCE, never a silent substitution — `RefreshPolicy.Of` answers the effective row beside an `Option<RefreshRefusal>` naming which axis refused, `Invalidate` folds every refusal into `RefreshDegrade` rows on its `RefreshReceipt`, and a caller reads exactly which definitions kept a stale preview and why.
- Law: `SkipNestedLinkedDefinitions` is settable on the live definition, so a nested-load posture is a subject column the refresh writes once at admission, never re-derived per version sweep.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------------
using System.Threading;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class RefreshPolicy {
    public static readonly RefreshPolicy Lazy = new(key: 0, rerenderGranted: false, retireGranted: false);
    public static readonly RefreshPolicy Eager = new(key: 1, rerenderGranted: true, retireGranted: false);
    public static readonly RefreshPolicy Drop = new(key: 2, rerenderGranted: false, retireGranted: true);

    public bool RerenderGranted { get; }
    public bool RetireGranted { get; }

    public static (RefreshPolicy Effective, Option<RefreshRefusal> Cause) Of(LinkSubject subject, RefreshPolicy requested) =>
        subject.Refuses(requested: requested).Case switch {
            RefreshRefusal cause => (Lazy, Some(cause)),
            _ => (requested, Option<RefreshRefusal>.None),
        };
}

[SmartEnum<int>]
public sealed partial class RefreshRefusal {
    public static readonly RefreshRefusal DocumentPolicy = new(key: 0);
    public static readonly RefreshRefusal SourceUnreadable = new(key: 1);
}

public sealed record RefreshDegrade(
    Guid Definition,
    RefreshPolicy Requested,
    RefreshPolicy Effective,
    RefreshRefusal Cause) : IDetachedDocumentResult;

public sealed record RefreshReceipt(
    Seq<RefreshDegrade> Degraded,
    int Freed,
    int Retired,
    int Rerendered) : IDetachedDocumentResult {
    internal static readonly RefreshReceipt Empty = new(
        Degraded: Seq<RefreshDegrade>(),
        Freed: 0,
        Retired: 0,
        Rerendered: 0);
}

// The document-scoped update style is a host enum with three named rows; it re-closes here as the keyed owner every
// arm reads through its `Updates` column, so an unlisted host ordinal refuses at admission rather than reading as
// "not NeverUpdate" — the exact silent widening a bare `is not` comparison against one row admits.
[SmartEnum<LinkedInstanceDefinitionUpdateStyle>]
public sealed partial class UpdateStyle {
    public static readonly UpdateStyle Prompt = new(key: LinkedInstanceDefinitionUpdateStyle.Prompt, updates: true);
    public static readonly UpdateStyle Always = new(key: LinkedInstanceDefinitionUpdateStyle.AlwaysUpdate, updates: true);
    public static readonly UpdateStyle Never = new(key: LinkedInstanceDefinitionUpdateStyle.NeverUpdate, updates: false);

    public bool Updates { get; }
}

[ComplexValueObject]
public sealed partial class LinkSubject {
    public SourceMode Mode { get; }
    public UpdateStyle Style { get; }
    public Option<SourceHealth> Health { get; }
    public bool SkipNested { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SourceMode mode,
        ref UpdateStyle style,
        ref Option<SourceHealth> health,
        ref bool skipNested) =>
        validationError = mode is not null && style is not null
            ? validationError
            : new ValidationError("link subject requires an admitted source mode and update style");

    // `SourceHealth` (model.md) is the branch's ONE archive-status vocabulary and it names no row for
    // `NotALinkedInstanceDefinition` — an unlinked definition has no source health, which is absence, so the
    // column is optional and a `Static` subject carries `None` instead of a row meaning "not applicable".
    public static Fin<LinkSubject> Of(InstanceDefinition definition, RhinoDoc document, Op key) =>
        from mode in SourceMode.Of(update: definition.UpdateType, key: key)
        from style in key.Row<LinkedInstanceDefinitionUpdateStyle, UpdateStyle>(document.LinkedInstanceDefinitionUpdate)
        let health = SourceHealth.Of(status: definition.ArchiveFileStatus)
        from admitted in Admission.Admitted(
            fault: Validate(mode, style, health, definition.SkipNestedLinkedDefinitions, out LinkSubject? subject),
            value: subject,
            refusal: key.InvalidResult())
        select admitted;

    internal bool Styled => Style.Updates;

    internal bool Readable => Health.Exists(static row => !row.Broken);

    internal Option<RefreshRefusal> Refuses(RefreshPolicy requested) =>
        !requested.RerenderGranted || Mode.Regenerates(styled: Styled, readable: Readable)
            ? Option<RefreshRefusal>.None
            : Some(Styled ? RefreshRefusal.SourceUnreadable : RefreshRefusal.DocumentPolicy);
}

[ComplexValueObject]
public sealed partial class LinkWatchPolicy {
    public TimeSpan Debounce { get; }
    public TimeProvider Clock { get; }
    public ReceiptPolicy Receipts { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref TimeSpan debounce,
        ref TimeProvider clock,
        ref ReceiptPolicy receipts) =>
        validationError = debounce >= TimeSpan.Zero && clock is not null && receipts is not null
            ? validationError
            : new ValidationError(message: "linked observation policy is invalid");
}
```

## [03]-[PREVIEW_CUSTODY]

`PreviewKey` combines document enrolment, definition, and structural request identity. `PreviewGrant` is the only public bitmap window; `Commit` mints each version above every live and retired version the key already holds, so a re-created key after eviction never aliases a retired grant. Superseded versions with grants move atomically to `Retired`; zero-grant versions release after publication and surface cleanup faults on the reachable grant, while a failed transition releases the uncommitted image.

- Law: `DocKey` alone cannot key the vault — it IS `RhinoDoc.RuntimeSerialNumber`, which the host RECYCLES across a close/open pair, so a document that closes holding retired grants aliases the next document handed the same serial and that document's first lease reads the dead document's bitmaps. `DocSeal` is the per-enrolment monotonic stamp that closes the alias: the key carries `(DocSeal, DocKey)`, close-eviction drops the seal with the entries, and a re-opened document mints a fresh seal that cannot address a parked row.

`VaultOutcome` carries transition products without captured mutation, and every product carries the vault's `Lease<Bitmap>` rather than the raw handle — a consumer reaches the image only through `PreviewGrant.Use`, a borrow the claim gate closes, so no caller can dispose an image the vault still owns.

- Law: the grant composes the package's `LifecycleGate` (events.md) — `Use` takes a claim for the whole body and `Dispose` closes through the same gate, so a release ISSUED DURING a live borrow waits on that claim rather than freeing a bitmap the body still reads. A released-flag check before the body is a check, not a gate: it passes, the peer disposes, and the body reads a freed native handle. A grant hand-rolling a `lock`/`Monitor` release machine beside the capsule is the collapsed form.

`Change` keys its result by TICKET inside the immutable state returned by `Swap`: each caller stamps its own key into a receipt map, reads its own key back, removes only that key, and captures both CAS operations. Concurrent transitions therefore never overwrite one another's product, and a key missing after its own swap is a typed fault, never a `Miss` a committing caller would misread as an absent entry.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The enrolment stamp, not the host serial, is the durable half of a preview address: `DocKey` is recycled, this
// is not, so structural equality over the pair distinguishes two documents the host gave one serial.
public readonly record struct DocSeal(long Value);

public sealed record PreviewKey : IDetachedDocumentResult {
    internal PreviewKey(DocSeal seal, DocKey document, Guid definition, BlockPreview spec) =>
        (Seal, Document, Definition, Spec) = (seal, document, definition, spec);

    public DocSeal Seal { get; }
    public DocKey Document { get; }
    public Guid Definition { get; }
    public BlockPreview Spec { get; }
}

internal sealed record PreviewEntry(int Version, Lease<System.Drawing.Bitmap> Image, int Grants, bool Stale);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record VaultOutcome {
    private VaultOutcome() { }
    public sealed record Miss : VaultOutcome;
    public sealed record Granted(int Version, Lease<System.Drawing.Bitmap> Image) : VaultOutcome;
    public sealed record Committed(int Version, Lease<System.Drawing.Bitmap> Image, Seq<Lease<System.Drawing.Bitmap>> Closing) : VaultOutcome;
    public sealed record Swept(
        Seq<Lease<System.Drawing.Bitmap>> Closing,
        Seq<PreviewKey> Retired,
        Seq<PreviewKey> Rerender,
        Seq<PreviewKey> Unresolved) : VaultOutcome;
    // `Surplus` is the losing watch of an enrolment race: two callers mint, one CAS wins, the loser's observation
    // closes outside the transition. Dropping it silently leaks a live host subscription per race.
    public sealed record Enrolled(DocSeal Seal, Option<Watch> Surplus) : VaultOutcome;
    public sealed record Discharged(Option<Watch> Observation) : VaultOutcome;

    internal static readonly VaultOutcome Clean = new Swept(
        Closing: Seq<Lease<System.Drawing.Bitmap>>(),
        Retired: Seq<PreviewKey>(),
        Rerender: Seq<PreviewKey>(),
        Unresolved: Seq<PreviewKey>());
}

internal sealed record RefreshResolution(HashMap<Guid, RefreshPolicy> Rows, Seq<RefreshDegrade> Degraded) {
    internal static readonly RefreshResolution Empty = new(
        Rows: HashMap<Guid, RefreshPolicy>(),
        Degraded: Seq<RefreshDegrade>());
}

internal sealed record DocEnrolment(DocSeal Seal, Watch Observation);

internal sealed record VaultState(
    HashMap<PreviewKey, PreviewEntry> Live,
    HashMap<(PreviewKey Key, int Version), PreviewEntry> Retired,
    HashMap<DocKey, DocEnrolment> Enrolled,
    HashMap<long, VaultOutcome> Receipts) {
    internal static readonly VaultState Empty = new(
        Live: HashMap<PreviewKey, PreviewEntry>(),
        Retired: HashMap<(PreviewKey, int), PreviewEntry>(),
        Enrolled: HashMap<DocKey, DocEnrolment>(),
        Receipts: HashMap<long, VaultOutcome>());
}

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class PreviewGrant : IDisposable {
    private readonly LifecycleGate gate;
    private readonly Lease<System.Drawing.Bitmap> image;
    private readonly Func<Seq<Error>> release;
    private readonly Atom<Seq<Error>> cleanupFaults;

    private PreviewGrant(
        PreviewKey key,
        int version,
        LifecycleGate gate,
        Lease<System.Drawing.Bitmap> image,
        Seq<Error> cleanupFaults,
        Func<Seq<Error>> release) {
        Key = key;
        Version = version;
        this.gate = gate;
        this.image = image;
        this.cleanupFaults = Atom(value: cleanupFaults);
        this.release = release;
    }

    public PreviewKey Key { get; }
    public int Version { get; }
    public Seq<Error> CleanupFaults => cleanupFaults.Value;

    internal static Fin<PreviewGrant> Of(
        PreviewKey key,
        int version,
        Lease<System.Drawing.Bitmap> image,
        Seq<Error> cleanupFaults,
        Func<Seq<Error>> release,
        Op op) =>
        LifecycleGate.Of(settleWithin: BlockLifecycle.GrantSettle, key: op).Map(gate => new PreviewGrant(
            key: key,
            version: version,
            gate: gate,
            image: image,
            cleanupFaults: cleanupFaults,
            release: release));

    // The vault owns the image for the whole grant, so the window is a BORROW the CLAIM gate closes: a body in
    // flight holds a claim, so a concurrent `Dispose` waits on that claim instead of freeing a bitmap the body is
    // still reading. A flag-only release loses that race — it flips, the vault frees the image, and the running
    // body reads a disposed native handle. Handing the bitmap out as a property loses the window entirely.
    public Fin<T> Use<T>(Func<System.Drawing.Bitmap, Fin<T>> body, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(body).Bind(run => gate.Within(
            body: () => run(arg: image.Resource),
            refused: () => Fin.Fail<T>(error: op.InvalidContext()),
            key: op));
    }

    public void Dispose() {
        Op op = Op.Of(name: nameof(Dispose));
        _ = gate.Close(
            stop: static () => Fin.Succ(value: unit),
            settle: () => BlockLifecycle.Lowered(faults: release()),
            key: op).Match(
                Succ: static _ => unit,
                Fail: error => ignore(cleanupFaults.Swap(f: held => held.Add(value: error))));
    }
}
```

## [04]-[LIFECYCLE]

`Engage` observes definition-table, worksession-file, and document-close facts through deferred document delivery and answers the document's `DocSeal`. Definition and worksession facts invalidate the document; close facts evict it, discharging the enrolment with the entries, so one atom transition owns invalidation, eviction, and enrolment custody. No mutation runs inside the host callback that raised the table event.

`Lease` enrols first, then reserves a fresh cached version or renders outside the atom and commits the owned lease with its first grant. Eager regeneration and closing-image cleanup settle as independent applicative attempts, and every failure remains typed before the fold returns to `Fin<Unit>`.

- Law: enrolment is idempotent and every vault ingress walks it, so a document holding entries always holds the observation that evicts them — eviction is a consequence of leasing, never a second call. A losing enrolment race closes its surplus `Watch` rather than dropping it, because a dropped watch is a live host subscription no owner can reach.

`WatchLinked` accepts one admitted observation policy, delegates its debounce, clock, and receipt bounds to `Observation.File`, and commits one typed `Refresh` transaction per settled change.

```csharp signature
// --- [SERVICES] ----------------------------------------------------------------------------
public static class BlockLifecycle {
    private static readonly Atom<VaultState> Vault = Atom(value: VaultState.Empty);

    // The one bound a grant's claim gate settles within: a borrow is a synchronous read of an in-memory bitmap, so
    // a claim outliving this window is a stuck body, not a slow one, and the close reports rather than hangs.
    internal static readonly TimeSpan GrantSettle = TimeSpan.FromSeconds(5);

    private static long ticket;

    // Enrolment is IDEMPOTENT and every ingress walks it: a document holding vault entries always holds the close
    // observation that evicts them, so eviction is a structural consequence of leasing rather than a second call a
    // caller can forget. A forgotten `Engage` used to leak every preview of every closed document for the process.
    public static Fin<DocSeal> Engage(DocumentSession session, RefreshPolicy policy) {
        Op op = Op.Of();
        return from owner in op.Need(session)
               from active in op.Need(policy)
               from seal in Vault.Value.Enrolled.Find(key: owner.Key).Case switch {
                   DocEnrolment held => Fin.Succ(value: held.Seal),
                   _ => Enrol(owner: owner, policy: active, op: op),
               }
               select seal;
    }

    private static Fin<DocSeal> Enrol(DocumentSession owner, RefreshPolicy policy, Op op) =>
        from watch in DocumentStream.Observe(request: new Observation.Host(
            Scope: new EventScope.Document(Key: owner.Key),
            Families: Seq(
                EventFamily.InstanceDefinitionTable,
                EventFamily.WorksessionFile,
                EventFamily.Closed),
            Delivery: new Delivery.Deferred(Sink: fact => Delivered(fact: fact, owner: owner, policy: policy)),
            Receipts: ReceiptPolicy.Operational))
        from outcome in Change(transition: state => state.Enrolled.Find(key: owner.Key).Case switch {
            DocEnrolment held => (state, (VaultOutcome)new VaultOutcome.Enrolled(Seal: held.Seal, Surplus: Some(watch))),
            _ => Seated(state: state, key: owner.Key, watch: watch),
        }, op: op)
        from seal in outcome is VaultOutcome.Enrolled enrolled
            ? enrolled.Surplus.Match(
                Some: loser => Closed(watch: loser, op: op).Map(_ => enrolled.Seal),
                None: () => Fin.Succ(value: enrolled.Seal))
            : Fin.Fail<DocSeal>(error: op.InvalidResult())
        select seal;

    private static (VaultState State, VaultOutcome Outcome) Seated(VaultState state, DocKey key, Watch watch) {
        DocSeal seal = new(Value: Interlocked.Increment(location: ref ticket));
        return (
            state with { Enrolled = state.Enrolled.AddOrUpdate(key: key, value: new DocEnrolment(Seal: seal, Observation: watch)) },
            new VaultOutcome.Enrolled(Seal: seal, Surplus: Option<Watch>.None));
    }

    private static Fin<Unit> Closed(Watch watch, Op op) => op.Catch(() =>
        watch.Close() is SubscriptionRelease.Faulted faulted
            ? Lowered(faults: faulted.Errors)
            : Fin.Succ(value: unit));

    // The sink outlives `Engage`, so each delivery mints its OWN key: one captured key would stamp every later
    // invalidation with one stale provenance and no delivery could be attributed to the fact that raised it.
    private static Fin<Unit> Delivered(DocEvent fact, DocumentSession owner, RefreshPolicy policy) {
        Op key = Op.Of(name: nameof(Engage));
        return (fact.Origin switch {
            EventOrigin.Host { Family: var family } when family == EventFamily.Closed => Evict(document: owner.Key),
            EventOrigin.Host { Family: var family }
                when family == EventFamily.InstanceDefinitionTable || family == EventFamily.WorksessionFile =>
                Invalidate(document: owner.Key, policy: policy, session: Some(owner), op: key),
            _ => Fin.Succ(value: RefreshReceipt.Empty),
        }).Map(static _ => unit);
    }

    public static Fin<PreviewGrant> Lease(DocumentSession session, ResourceRef target, BlockPreview spec) {
        Op op = Op.Of();
        return from owner in op.Need(session)
               from address in op.Need(target)
               from request in op.Need(spec)
               from seal in Engage(session: owner, policy: RefreshPolicy.Lazy)
               from key in owner.Demand(
                   use: document => Definitions.Resolve(target: address, document: document, key: op)
                       .Map(definition => new PreviewKey(
                           seal: seal,
                           document: owner.Key,
                           definition: definition.Id,
                           spec: request)),
                   key: op,
                   needs: [SessionNeed.Read])
               from cached in TryGrant(key: key, op: op)
               from grant in cached.Match(
                   Some: static held => Fin.Succ(value: held),
                   None: () => Render(session: owner, target: address, key: key, op: op))
               select grant;
    }

    // Eviction discharges the enrolment WITH the entries: the seal dies here, so a document handed the same
    // recycled host serial enrols fresh and its keys can never address a row this sweep parked or missed.
    public static Fin<RefreshReceipt> Evict(DocKey document) {
        Op op = Op.Of();
        return from receipt in Invalidate(
                   document: document,
                   policy: RefreshPolicy.Drop,
                   session: Option<DocumentSession>.None,
                   op: op)
               from outcome in Change(
                   transition: state => state.Enrolled.Find(key: document).Case switch {
                       DocEnrolment held => (
                           state with { Enrolled = state.Enrolled.Remove(key: document) },
                           (VaultOutcome)new VaultOutcome.Discharged(Observation: Some(held.Observation))),
                       _ => (state, new VaultOutcome.Discharged(Observation: Option<Watch>.None)),
                   },
                   op: op)
               from _ in outcome is VaultOutcome.Discharged discharged
                   ? discharged.Observation.Match(
                       Some: watch => Closed(watch: watch, op: op),
                       None: static () => Fin.Succ(value: unit))
                   : Fin.Fail<Unit>(error: op.InvalidResult())
               select receipt;
    }

    public static Fin<Watch> WatchLinked(
        DocumentSession session,
        ResourceRef target,
        string path,
        LinkWatchPolicy policy) {
        Op op = Op.Of();
        return from owner in op.Need(session)
               from address in op.Need(target)
               from active in op.Need(policy)
               from source in op.AcceptText(value: path)
               from watch in DocumentStream.Observe(request: new Observation.File(
                   Path: source,
                   Debounce: active.Debounce,
                   Clock: active.Clock,
                   Delivery: new Delivery.Deferred(Sink: _ =>
                       from plan in BlockTransaction.Batch(
                           name: nameof(WatchLinked),
                           redraw: RedrawPolicy.Deferred,
                           operations: [new BlockOp.Refresh(Target: address)])
                       from __ in Blocks.Commit(session: owner, transaction: plan)
                       select unit),
                   Receipts: active.Receipts))
               select watch;
    }

    // `RefreshPolicy.Of` is the PRODUCER the product law demands: the requested row is a REQUEST, and each definition's own
    // `LinkSubject` decides whether it is admitted or degrades to `Lazy`. The lookup is built from the host BEFORE the atom
    // transition, because the transition is pure — so an eviction with no session (the document is gone) keeps the requested row,
    // which is only ever `Drop`, and every live sweep answers per definition.
    private static Fin<RefreshReceipt> Invalidate(
        DocKey document,
        RefreshPolicy policy,
        Option<DocumentSession> session,
        Op op) {
        return Resolved(document: document, requested: policy, session: session, op: op).Bind(resolved =>
        Change(transition: state => {
            Seq<(PreviewKey Key, PreviewEntry Entry)> hit = state.Live.AsIterable()
                .Filter(pair => pair.Key.Document == document)
                .Map(static pair => (pair.Key, pair.Value))
                .ToSeq();
            // The resolution answers over the LIVE sweep, so a key it does not carry is an entry the sweep did not
            // see — a state fault, not an occasion to substitute the caller's requested row. `Drop` degrading a
            // definition the resolver refused would silently retire a preview the host policy protects.
            Seq<PreviewKey> unresolved = hit
                .Filter(row => resolved.Rows.Find(row.Key.Definition).IsNone)
                .Map(static row => row.Key);
            Seq<(PreviewKey Key, PreviewEntry Entry, RefreshPolicy Row)> rows = hit
                .Choose(row => resolved.Rows.Find(row.Key.Definition).Map(effective => (row.Key, row.Entry, Row: effective)));
            Seq<Lease<System.Drawing.Bitmap>> closing = rows
                .Filter(static row => row.Entry.Grants == 0)
                .Map(static row => row.Entry.Image);
            Seq<PreviewKey> retired = rows
                .Filter(static row => row.Entry.Grants > 0 && row.Row.RetireGranted)
                .Map(static row => row.Key);
            Seq<PreviewKey> rerender = rows
                .Filter(static row => row.Entry.Grants > 0 && row.Row.RerenderGranted)
                .Map(static row => row.Key);
            VaultState next = rows.Fold(
                state,
                static (fold, row) => row.Entry.Grants == 0 || row.Row.RetireGranted
                    ? fold with {
                        Live = fold.Live.Remove(key: row.Key),
                        Retired = row.Entry.Grants > 0
                            ? fold.Retired.AddOrUpdate(key: (row.Key, row.Entry.Version), value: row.Entry)
                            : fold.Retired,
                    }
                    // A granted version the row keeps is marked stale in place: the next `TryGrant` misses on
                    // `Stale`, renders a fresh version, and `Commit` supersedes this one — so the stale entry has
                    // a declared exit and never parks in `Live` unreachable by both grant and sweep.
                    : fold with {
                        Live = fold.Live.AddOrUpdate(key: row.Key, value: row.Entry with { Stale = true }),
                    });
            return (next, new VaultOutcome.Swept(
                Closing: closing,
                Retired: retired,
                Rerender: rerender,
                Unresolved: unresolved));
        }, op: op).Bind(outcome => outcome is VaultOutcome.Swept swept
                ? Attempted(
                    () => guard(swept.Unresolved.IsEmpty, op.InvalidResult()).ToFin(),
                    () => Lowered(faults: ReleaseAll(images: swept.Closing, op: op)),
                    () => swept.Rerender
                        .Traverse(key => session.ToFin(Fail: op.MissingContext())
                            .Bind(active => Rerendered(session: active, key: key, op: op))
                            .ToValidation())
                        .As()
                        .ToFin()
                        .Map(static _ => unit))
                    // `Retired` counts versions moved to retirement and `Freed` counts images released; the two
                    // are disjoint by construction — a zero-grant entry frees, a granted entry retires — so one
                    // column standing for both under either name reports a number no consumer can act on.
                    .Map(_ => new RefreshReceipt(
                        Degraded: resolved.Degraded,
                        Freed: swept.Closing.Count,
                        Retired: swept.Retired.Count,
                        Rerendered: swept.Rerender.Count))
                : Fin.Fail<RefreshReceipt>(error: op.InvalidResult())));
    }

    // One read demand answers the whole sweep: every live preview's definition resolves its `LinkSubject` once, so the
    // requested row is admitted or degraded per definition and a `NeverUpdate`, embedded, or unreadable-source definition
    // can never be regenerated from a changed external file — the exact case the host discriminants exist to foreclose.
    private static Fin<RefreshResolution> Resolved(
        DocKey document,
        RefreshPolicy requested,
        Option<DocumentSession> session,
        Op op) =>
        session.Match(
            None: () => Fin.Succ(RefreshResolution.Empty),
            Some: active => active.Demand(
                use: held => Vault.Value.Live.AsIterable()
                    .Filter(pair => pair.Key.Document == document)
                    .Map(static pair => pair.Key.Definition)
                    .Distinct()
                    .ToSeq()
                    .Traverse(definition => (
                        from target in ResourceRef.Of(id: definition)
                        from resolved in Definitions.Resolve(target: target, document: held, key: op)
                        from subject in LinkSubject.Of(definition: resolved, document: held, key: op)
                        let row = RefreshPolicy.Of(subject: subject, requested: requested)
                        select (
                            Definition: definition,
                            row.Effective,
                            Degrade: row.Cause.Map(cause => new RefreshDegrade(
                                Definition: definition,
                                Requested: requested,
                                Effective: row.Effective,
                                Cause: cause)))).ToValidation())
                    .As()
                    .ToFin()
                    .Map(static rows => new RefreshResolution(
                        Rows: rows.Map(static row => (row.Definition, row.Effective)).ToHashMap(),
                        Degraded: rows.Choose(static row => row.Degrade))),
                key: op,
                needs: [SessionNeed.Read]));

    private static Fin<Unit> Rerendered(DocumentSession session, PreviewKey key, Op op) =>
        ResourceRef.Of(id: key.Definition)
            .Bind(target => Render(session: session, target: target, key: key, op: op))
            .Bind(grant => op.Catch(() => {
                grant.Dispose();
                return Lowered(faults: grant.CleanupFaults);
            }));

    private static Fin<PreviewGrant> Render(DocumentSession session, ResourceRef target, PreviewKey key, Op op) =>
        Blocks.Ask(session: session, request: new BlockAsk.Preview(Target: target, Spec: key.Spec)).Bind(answer =>
            answer is BlockAnswer.Rendered rendered
                ? Commit(key: key, image: rendered.Preview, op: op)
                : Fin.Fail<PreviewGrant>(error: op.InvalidResult()));

    private static Fin<PreviewGrant> Commit(PreviewKey key, Lease<System.Drawing.Bitmap> image, Op op) {
        Fin<VaultOutcome> transitioned = Change(transition: state => {
                Option<PreviewEntry> prior = state.Live.Find(key: key);
                int liveVersion = prior.Map(static held => held.Version).IfNone(noneValue: 0);
                int retiredVersion = state.Retired.AsIterable()
                    .Filter(pair => pair.Key.Key == key)
                    .Fold(0, static (high, pair) => int.Max(high, pair.Key.Version));
                int version = int.Max(liveVersion, retiredVersion) + 1;
                Seq<Lease<System.Drawing.Bitmap>> closing = prior
                    .Filter(static held => held.Grants == 0)
                    .Map(static held => Seq(held.Image))
                    .IfNone(Seq<Lease<System.Drawing.Bitmap>>());
                PreviewEntry next = new(Version: version, Image: image, Grants: 1, Stale: false);
                VaultState changed = new(
                    Live: state.Live.AddOrUpdate(key: key, value: next),
                    Retired: prior.Filter(static held => held.Grants > 0)
                        .Map(held => state.Retired.AddOrUpdate(key: (key, held.Version), value: held))
                        .IfNone(state.Retired),
                    Receipts: state.Receipts);
                return (changed, new VaultOutcome.Committed(
                    Version: version,
                    Image: image,
                    Closing: closing));
            }, op: op);

        return transitioned.Bind(outcome => outcome is VaultOutcome.Committed committed
                ? Granted(
                    key: key,
                    version: committed.Version,
                    image: committed.Image,
                    cleanupFaults: ReleaseAll(images: committed.Closing, op: op),
                    op: op)
                : Fin.Fail<PreviewGrant>(error: op.InvalidResult()))
            .MapFail(primary => ReleaseAll(images: Seq(image), op: op)
                .Fold(primary, static (error, cleanup) => error + cleanup));
    }

    private static Fin<Option<PreviewGrant>> TryGrant(PreviewKey key, Op op) =>
        Change(transition: state => state.Live.Find(key: key).Case switch {
            PreviewEntry { Stale: false } current => (
                state with {
                    Live = state.Live.AddOrUpdate(
                        key: key,
                        value: current with { Grants = current.Grants + 1 }),
                },
                new VaultOutcome.Granted(Version: current.Version, Image: current.Image)),
            _ => (state, new VaultOutcome.Miss()),
        }, op: op).Bind(outcome => outcome is VaultOutcome.Granted granted
                ? Granted(
                        key: key,
                        version: granted.Version,
                        image: granted.Image,
                        cleanupFaults: Seq<Error>(),
                        op: op)
                    .Map(Some)
                : Fin.Succ(Option<PreviewGrant>.None));

    private static Fin<PreviewGrant> Granted(
        PreviewKey key,
        int version,
        Lease<System.Drawing.Bitmap> image,
        Seq<Error> cleanupFaults,
        Op op) =>
        PreviewGrant.Of(
            key: key,
            version: version,
            image: image,
            cleanupFaults: cleanupFaults,
            release: () => Release(key: key, version: version),
            op: op);

    private static Seq<Error> ReleaseAll(Seq<Lease<System.Drawing.Bitmap>> images, Op op) => images
        .Choose(image => op.Catch(() => { image.Dispose(); return Fin.Succ(value: unit); }).Match(
            Succ: static _ => Option<Error>.None,
            Fail: static error => Some(error)));

    // `Lowered` folds an already-collected fault roster onto the rail; `Attempted` RUNS independent attempts and
    // accumulates their faults. Two operations, two names — one name over both reads as an overload of the same
    // verb and a call site cannot tell whether it is reporting faults or producing them.
    internal static Fin<Unit> Lowered(Seq<Error> faults) =>
        faults.Head.Match(
            Some: first => Fin.Fail<Unit>(error: faults.Tail.Fold(first, static (error, fault) => error + fault)),
            None: static () => Fin.Succ(value: unit));

    private static Fin<Unit> Attempted(params ReadOnlySpan<Func<Fin<Unit>>> attempts) =>
        toSeq(attempts.ToArray())
            .Traverse(attempt => attempt().ToValidation())
            .As()
            .ToFin()
            .Map(static _ => unit);

    private static Seq<Error> Release(PreviewKey key, int version) {
        Op op = Op.Of(name: nameof(Release));
        return Change(transition: state => state.Live.Find(key: key).Case switch {
                PreviewEntry current when current.Version == version => (
                    state with {
                        Live = state.Live.AddOrUpdate(
                            key: key,
                            value: current with { Grants = int.Max(current.Grants - 1, 0) }),
                    },
                    VaultOutcome.Clean),
                _ => state.Retired.Find(key: (key, version)).Case switch {
                    PreviewEntry parked when parked.Grants <= 1 => (
                        state with { Retired = state.Retired.Remove(key: (key, version)) },
                        (VaultOutcome)new VaultOutcome.Swept(
                            Closing: Seq(parked.Image),
                            Retired: Seq<PreviewKey>(),
                            Rerender: Seq<PreviewKey>(),
                            Unresolved: Seq<PreviewKey>())),
                    PreviewEntry parked => (
                        state with {
                            Retired = state.Retired.AddOrUpdate(
                                key: (key, version),
                                value: parked with { Grants = parked.Grants - 1 }),
                        },
                        VaultOutcome.Clean),
                    _ => (state, VaultOutcome.Clean),
                },
            }, op: op)
            .Match(
                Succ: outcome => outcome is VaultOutcome.Swept swept
                    ? ReleaseAll(images: swept.Closing, op: op)
                    : Seq<Error>(),
                Fail: static error => Seq(error));
    }

    // Every caller keys its own product: the transition stamps `current`, reads `current` back, and clears
    // `current` alone, so a concurrent transition never overwrites a peer's outcome and no caller inherits one.
    private static Fin<VaultOutcome> Change(
        Func<VaultState, (VaultState State, VaultOutcome Outcome)> transition,
        Op op) => op.Catch(() => {
        long current = Interlocked.Increment(location: ref ticket);
        VaultState settled = Vault.Swap(f: state => {
            (VaultState next, VaultOutcome outcome) = transition(arg: state);
            return next with { Receipts = next.Receipts.AddOrUpdate(key: current, value: outcome) };
        });
        Fin<VaultOutcome> result = settled.Receipts.Find(key: current).ToFin(Fail: op.InvalidResult());
        _ = Vault.Swap(f: state => state with { Receipts = state.Receipts.Remove(key: current) });
        return result;
    });
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]          | [INGRESS]                                    | [STATE]                | [EGRESS]                        |
| :-----: | :--------------- | :------------------------------------------- | :--------------------- | :------------------------------ |
|  [01]   | `BlockLifecycle` | `Engage` · `Lease` · `WatchLinked` · `Evict` | `Atom<VaultState>`     | seal · grant · `RefreshReceipt` |
|  [02]   | `PreviewGrant`   | `PreviewGrant.Of`                            | `LifecycleGate` claims | claim-gated bitmap borrow       |
|  [03]   | policy owners    | generated admission                          | invalidation decisions | policy values                   |
|  [04]   | `RefreshDegrade` | `RefreshPolicy.Of` refusal                   | per-definition cause   | degrade evidence rows           |
|  [05]   | `DocSeal`        | enrolment                                    | recycled-serial guard  | `PreviewKey` identity half      |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
