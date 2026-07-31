# [RASM_RHINO_BLOCK_LIFECYCLE]

Preview lifecycle (`Rasm.Rhino.Blocks`) owns bitmap custody, versioned grants, document-scoped invalidation, linked-source refresh, and deterministic disposal. Host acquisition and disposal stay outside `Atom.Swap`; `Change` captures both swaps and every transition on one `Fin<VaultOutcome>` rail.

## [01]-[INDEX]

- [02]-[REFRESH_POLICY]: `RefreshPolicy`, `LinkSubject`, and `LinkWatchPolicy` partitioning matching versions by grant state across the lazy, eager, and drop rows, with `RefreshRefusal` and `RefreshDegrade` carrying every degrade as typed evidence.
- [03]-[PREVIEW_CUSTODY]: `PreviewKey` structural request identity and `PreviewGrant` the only bitmap window — a release-gated borrow, never a handed-out handle — superseded versions moving atomically to retirement and `VaultOutcome` carrying leased transition products without captured mutation.
- [04]-[LIFECYCLE]: `BlockLifecycle` observing definition-table, worksession, and document-close facts through one atom transition owning invalidation and eviction, and `Lease` reserving or rendering outside the atom before committing with its first grant.
- [05]-[SURFACE_LEDGER]: owner-to-ingress-to-state-to-egress roster across `BlockLifecycle`, `PreviewGrant`, and the policy owners.

## [02]-[REFRESH_POLICY]

`RefreshPolicy` partitions matching versions by grant state. Every row removes and closes zero-grant versions; `Lazy` keeps granted versions stale, `Eager` keeps them stale and regenerates them, and `Drop` moves them to retirement. A caller's row is a REQUEST: `Invalidate` resolves it per definition through `RefreshPolicy.Of` before its pure transition, so the sweep answers over live host state rather than the caller's wish.

- Law: grant state alone does not decide a refresh — `RefreshPolicy.Of` resolves over the PRODUCT of grant state and the two independent host discriminants a linked definition carries: the `SourceMode` row behind `InstanceDefinition.UpdateType` (`Static` embeds and never re-reads, `LinkedAndEmbedded` re-reads yet survives a missing source, `Linked` requires it) and the document-scoped `RhinoDoc.LinkedInstanceDefinitionUpdate` policy (`Prompt`, `AlwaysUpdate`, `NeverUpdate`). Regenerating a `NeverUpdate` document's LINKED definition from a changed external file is exactly the case the host discriminant exists to foreclose, and a policy keyed on grant state alone cannot express it.
- Law: the three source rows answer independently and the vocabulary owns the answer — `SourceMode.Regenerates(styled, readable)` is the behavior column, so `Static` admits eager regeneration unconditionally (it re-renders embedded geometry and consults no external file), `LinkedAndEmbedded` admits it whenever the document style permits regardless of source readability (its embedded copy survives a missing source), and `Linked` requires both. An arm ladder over the product re-derives what the row already states and inverts the common case, which is the deleted form.
- Law: source AVAILABILITY is a third axis, not a failure — `InstanceDefinition.ArchiveFileStatus` distinguishes `NotALinkedInstanceDefinition`, `LinkedFileNotReadable`, `LinkedFileNotFound`, `LinkedFileIsUpToDate`, `LinkedFileIsNewer`, `LinkedFileIsOlder`, and `LinkedFileIsDifferent`, so an eager row over an unreadable source degrades to the stale-keep arm with typed evidence instead of regenerating from nothing; `LinkSubject` closes the (source-mode × update-style × archive-status) product as ROW DATA and `RefreshPolicy.Of` is a lookup over it, never a branch ladder at a call site.
- Law: a degrade is EVIDENCE, never a silent substitution — `RefreshPolicy.Of` answers the effective row beside an `Option<RefreshRefusal>` naming which axis refused, `Invalidate` folds every refusal into `RefreshDegrade` rows on its `RefreshReceipt`, and a caller reads exactly which definitions kept a stale preview and why.
- Law: `SkipNestedLinkedDefinitions` is settable on the live definition, so a nested-load posture is a subject column the refresh writes once at admission, never re-derived per version sweep.

```csharp signature
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
    int Retired,
    int Rerendered) : IDetachedDocumentResult {
    internal static readonly RefreshReceipt Empty = new(
        Degraded: Seq<RefreshDegrade>(),
        Retired: 0,
        Rerendered: 0);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct LinkSubject {
    public SourceMode Mode { get; }
    public LinkedInstanceDefinitionUpdateStyle Style { get; }
    public InstanceDefinitionArchiveFileStatus Archive { get; }
    public bool SkipNested { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SourceMode mode,
        ref LinkedInstanceDefinitionUpdateStyle style,
        ref InstanceDefinitionArchiveFileStatus archive,
        ref bool skipNested) =>
        validationError = mode is not null && Enum.IsDefined(style) && Enum.IsDefined(archive)
            ? validationError
            : new ValidationError("link subject requires an admitted source mode and defined host style and archive rows");

    public static Fin<LinkSubject> Of(InstanceDefinition definition, RhinoDoc document, Op key) =>
        SourceMode.Of(update: definition.UpdateType, key: key).Map(mode => Create(
            mode: mode,
            style: document.LinkedInstanceDefinitionUpdate,
            archive: definition.ArchiveFileStatus,
            skipNested: definition.SkipNestedLinkedDefinitions));

    internal bool Styled => Style is not LinkedInstanceDefinitionUpdateStyle.NeverUpdate;

    internal bool Readable => Archive is InstanceDefinitionArchiveFileStatus.LinkedFileIsUpToDate
        or InstanceDefinitionArchiveFileStatus.LinkedFileIsNewer
        or InstanceDefinitionArchiveFileStatus.LinkedFileIsOlder
        or InstanceDefinitionArchiveFileStatus.LinkedFileIsDifferent;

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

`PreviewKey` combines document, definition, and structural request identity. `PreviewGrant` is the only public bitmap window; `Commit` mints each version above every live and retired version the key already holds, so a re-created key after eviction never aliases a retired grant. Superseded versions with grants move atomically to `Retired`; zero-grant versions release after publication and surface cleanup faults on the reachable grant, while a failed transition releases the uncommitted image.

`VaultOutcome` carries transition products without captured mutation, and every product carries the vault's `Lease<Bitmap>` rather than the raw handle — a consumer reaches the image only through `PreviewGrant.Use`, a borrow the release gate closes, so no caller can dispose an image the vault still owns.

`Change` keys its result by TICKET inside the immutable state returned by `Swap`: each caller stamps its own key into a receipt map, reads its own key back, removes only that key, and captures both CAS operations. Concurrent transitions therefore never overwrite one another's product, and a key missing after its own swap is a typed fault, never a `Miss` a committing caller would misread as an absent entry.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
public sealed record PreviewKey : IDetachedDocumentResult {
    internal PreviewKey(DocKey document, Guid definition, BlockPreview spec) =>
        (Document, Definition, Spec) = (document, definition, spec);

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
    public sealed record Swept(Seq<Lease<System.Drawing.Bitmap>> Closing, Seq<PreviewKey> Rerender) : VaultOutcome;

    internal static readonly VaultOutcome Clean = new Swept(
        Closing: Seq<Lease<System.Drawing.Bitmap>>(),
        Rerender: Seq<PreviewKey>());
}

internal sealed record RefreshResolution(HashMap<Guid, RefreshPolicy> Rows, Seq<RefreshDegrade> Degraded) {
    internal static readonly RefreshResolution Empty = new(
        Rows: HashMap<Guid, RefreshPolicy>(),
        Degraded: Seq<RefreshDegrade>());
}

internal sealed record VaultState(
    HashMap<PreviewKey, PreviewEntry> Live,
    HashMap<(PreviewKey Key, int Version), PreviewEntry> Retired,
    HashMap<long, VaultOutcome> Receipts) {
    internal static readonly VaultState Empty = new(
        Live: HashMap<PreviewKey, PreviewEntry>(),
        Retired: HashMap<(PreviewKey, int), PreviewEntry>(),
        Receipts: HashMap<long, VaultOutcome>());
}

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class PreviewGrant : IDisposable {
    private readonly Lock gate = new();
    private readonly Lease<System.Drawing.Bitmap> image;
    private readonly Func<Seq<Error>> release;
    private Seq<Error> cleanupFaults;
    private int released;

    internal PreviewGrant(
        PreviewKey key,
        int version,
        Lease<System.Drawing.Bitmap> image,
        Seq<Error> cleanupFaults,
        Func<Seq<Error>> release) {
        Key = key;
        Version = version;
        this.image = image;
        this.cleanupFaults = cleanupFaults;
        this.release = release;
    }

    public PreviewKey Key { get; }
    public int Version { get; }
    public Seq<Error> CleanupFaults {
        get { lock (gate) { return cleanupFaults; } }
    }

    // The vault owns the image for the whole grant, so the window is a BORROW the release gate closes;
    // handing the bitmap out as a property lets a consumer dispose state the vault still indexes.
    public Fin<T> Use<T>(Func<System.Drawing.Bitmap, Fin<T>> body, Op? key = null) {
        Op op = key.OrDefault();
        return from run in op.Need(body)
               from _ in guard(Volatile.Read(location: ref released) == 0, op.InvalidContext()).ToFin()
               from result in op.Catch(() => run(image.Resource))
               select result;
    }

    public void Dispose() {
        if (Interlocked.Exchange(location1: ref released, value: 1) == 0) {
            Seq<Error> faults = release();
            lock (gate) {
                cleanupFaults = cleanupFaults.Concat(faults);
            }
        }
    }
}
```

## [04]-[LIFECYCLE]

`Engage` observes definition-table, worksession-file, and document-close facts through deferred document delivery. Definition and worksession facts invalidate the document; close facts evict it through the same sweep under `RefreshPolicy.Drop`, so one atom transition owns invalidation and eviction. No mutation runs inside the host callback that raised the table event.

`Lease` first reserves a fresh cached version or renders outside the atom and commits the owned lease with its first grant. Eager regeneration and closing-image cleanup settle as independent applicative attempts, and every failure remains typed before the fold returns to `Fin<Unit>`.

`WatchLinked` accepts one admitted observation policy, delegates its debounce, clock, and receipt bounds to `Observation.File`, and commits one typed `Refresh` transaction per settled change.

```csharp signature
// --- [SERVICES] ----------------------------------------------------------------------------
public static class BlockLifecycle {
    private static readonly Atom<VaultState> Vault = Atom(value: VaultState.Empty);
    private static long ticket;

    public static Fin<Watch> Engage(DocumentSession session, RefreshPolicy policy) {
        Op op = Op.Of();
        return from owner in Optional(session).ToFin(Fail: op.InvalidInput())
               from active in Optional(policy).ToFin(Fail: op.InvalidInput())
               from watch in DocumentStream.Observe(request: new Observation.Host(
                   Scope: new EventScope.Document(Key: owner.Key),
                   Families: Seq(
                       EventFamily.InstanceDefinitionTable,
                       EventFamily.WorksessionFile,
                       EventFamily.Closed),
                   Delivery: new Delivery.Deferred(Sink: fact => Delivered(fact: fact, owner: owner, policy: active)),
                   Receipts: ReceiptPolicy.Operational))
               select watch;
    }

    // The sink outlives `Engage`, so each delivery mints its OWN key: one captured key would stamp every later
    // invalidation with one stale provenance and no delivery could be attributed to the fact that raised it.
    private static Fin<Unit> Delivered(EventFact fact, DocumentSession owner, RefreshPolicy policy) {
        Op key = Op.Of(name: nameof(Engage));
        return (fact.Origin switch {
            EventOrigin.Host { Family: var family } when family == EventFamily.Closed =>
                Invalidate(document: owner.Key, policy: RefreshPolicy.Drop, session: Option<DocumentSession>.None, op: key),
            EventOrigin.Host { Family: var family }
                when family == EventFamily.InstanceDefinitionTable || family == EventFamily.WorksessionFile =>
                Invalidate(document: owner.Key, policy: policy, session: Some(owner), op: key),
            _ => Fin.Succ(value: RefreshReceipt.Empty),
        }).Map(static _ => unit);
    }

    public static Fin<PreviewGrant> Lease(DocumentSession session, ResourceRef target, BlockPreview spec) {
        Op op = Op.Of();
        return from owner in Optional(session).ToFin(Fail: op.InvalidInput())
               from address in Optional(target).ToFin(Fail: op.InvalidInput())
               from request in Optional(spec).ToFin(Fail: op.InvalidInput())
               from key in owner.Demand(
                   use: document => Definitions.Resolve(target: address, document: document, key: op)
                       .Map(definition => new PreviewKey(document: owner.Key, definition: definition.Id, spec: request)),
                   key: op,
                   needs: [SessionNeed.Read])
               from cached in TryGrant(key: key, op: op)
               from grant in cached.Match(
                   Some: static held => Fin.Succ(value: held),
                   None: () => Render(session: owner, target: address, key: key, op: op))
               select grant;
    }

    public static Fin<RefreshReceipt> Evict(DocKey document) =>
        Invalidate(document: document, policy: RefreshPolicy.Drop, session: Option<DocumentSession>.None, op: Op.Of());

    public static Fin<Watch> WatchLinked(
        DocumentSession session,
        ResourceRef target,
        string path,
        LinkWatchPolicy policy) {
        Op op = Op.Of();
        return from owner in Optional(session).ToFin(Fail: op.InvalidInput())
               from address in Optional(target).ToFin(Fail: op.InvalidInput())
               from active in Optional(policy).ToFin(Fail: op.InvalidInput())
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
            RefreshPolicy Row(PreviewKey key) => resolved.Rows.Find(key.Definition).IfNone(policy);
            Seq<Lease<System.Drawing.Bitmap>> closing = hit
                .Filter(static row => row.Entry.Grants == 0)
                .Map(static row => row.Entry.Image);
            Seq<PreviewKey> rerender = hit
                .Filter(row => row.Entry.Grants > 0 && Row(row.Key).RerenderGranted)
                .Map(static row => row.Key);
            VaultState next = hit.Fold(
                (State: state, Row: (Func<PreviewKey, RefreshPolicy>)Row),
                (fold, row) => (
                    State: row.Entry.Grants == 0 || fold.Row(row.Key).RetireGranted
                        ? fold.State with {
                            Live = fold.State.Live.Remove(key: row.Key),
                            Retired = row.Entry.Grants > 0
                                ? fold.State.Retired.AddOrUpdate(
                                    key: (row.Key, row.Entry.Version),
                                    value: row.Entry)
                                : fold.State.Retired,
                        }
                        : fold.State with {
                            Live = fold.State.Live.AddOrUpdate(
                                key: row.Key,
                                value: row.Entry with { Stale = true }),
                        },
                    Row: fold.Row)).State;
            return (next, new VaultOutcome.Swept(Closing: closing, Rerender: rerender));
        }, op: op).Bind(outcome => outcome is VaultOutcome.Swept swept
                ? Settle(
                    () => Settle(faults: ReleaseAll(images: swept.Closing, op: op)),
                    () => swept.Rerender
                        .Traverse(key => session.ToFin(Fail: op.MissingContext())
                            .Bind(active => Rerendered(session: active, key: key, op: op))
                            .ToValidation())
                        .As()
                        .ToFin()
                        .Map(static _ => unit))
                    .Map(_ => new RefreshReceipt(
                        Degraded: resolved.Degraded,
                        Retired: swept.Closing.Count,
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
                return Settle(faults: grant.CleanupFaults);
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
                ? Fin.Succ(value: Granted(
                    key: key,
                    version: committed.Version,
                    image: committed.Image,
                    cleanupFaults: ReleaseAll(images: committed.Closing, op: op)))
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
        }, op: op).Map(outcome => outcome is VaultOutcome.Granted granted
                ? Some(Granted(
                    key: key,
                    version: granted.Version,
                    image: granted.Image,
                    cleanupFaults: Seq<Error>()))
                : Option<PreviewGrant>.None);

    private static PreviewGrant Granted(
        PreviewKey key,
        int version,
        Lease<System.Drawing.Bitmap> image,
        Seq<Error> cleanupFaults) =>
        new(
            key: key,
            version: version,
            image: image,
            cleanupFaults: cleanupFaults,
            release: () => Release(key: key, version: version));

    private static Seq<Error> ReleaseAll(Seq<Lease<System.Drawing.Bitmap>> images, Op op) => images
        .Choose(image => op.Catch(() => { image.Dispose(); return Fin.Succ(value: unit); }).Match(
            Succ: static _ => Option<Error>.None,
            Fail: static error => Some(error)));

    private static Fin<Unit> Settle(Seq<Error> faults) =>
        faults.Head.Match(
            Some: first => Fin.Fail<Unit>(error: faults.Tail.Fold(first, static (error, fault) => error + fault)),
            None: static () => Fin.Succ(value: unit));

    private static Fin<Unit> Settle(params Func<Fin<Unit>>[] attempts) =>
        toSeq(attempts)
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
                            Rerender: Seq<PreviewKey>())),
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

| [INDEX] | [OWNER]          | [INGRESS]                                    | [STATE]                | [EGRESS]                         |
| :-----: | :--------------- | :------------------------------------------- | :--------------------- | :------------------------------- |
|  [01]   | `BlockLifecycle` | `Engage` · `Lease` · `WatchLinked` · `Evict` | `Atom<VaultState>`     | watch · grant · `RefreshReceipt` |
|  [02]   | `PreviewGrant`   | `Granted`                                    | release gate           | release-gated bitmap borrow      |
|  [03]   | policy owners    | generated admission                          | invalidation decisions | policy values                    |
|  [04]   | `RefreshDegrade` | `RefreshPolicy.Of` refusal                   | per-definition cause   | degrade evidence rows            |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
