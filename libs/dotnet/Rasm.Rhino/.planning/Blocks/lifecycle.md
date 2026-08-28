# [RASM_RHINO_BLOCK_LIFECYCLE]

Preview lifecycle (`Rasm.Rhino.Blocks`) owns bitmap custody, versioned grants, document-scoped invalidation, linked-source refresh, and deterministic disposal. Host acquisition and disposal stay outside every atom transition; each transition answers a `Transition<VaultState>` verdict whose committed state carries this caller's own product, so no ticket registry, no outcome map, and no cleanup swap exist beside the cell. The vault itself is an instance the shell capsule seats once at plug-in load — `ShellMount.Vault` mints `BlockVault.Of()` and claims the one mount seat — so the process-wide cell has one declared writer and this page owns the algebra alone.

## [01]-[INDEX]

- [02]-[REFRESH_POLICY]: `RefreshAction`, `RefreshPolicy`, `UpdateStyle`, `LinkSubject`, and `LinkWatchPolicy` partitioning matching versions by grant state.
- [03]-[PREVIEW_CUSTODY]: `DocSeal` and `PreviewKey` structural request identity and `PreviewGrant` the only bitmap window — a claim-gated borrow, never a handed-out handle — superseded versions moving atomically to retirement and `VaultOutcome` carrying leased transition products without captured mutation.
- [04]-[LIFECYCLE]: `BlockVault` — the seated instance owning enrolment, invalidation, and eviction through kernel `Cell` transitions — and `BlockLifecycle`, the mounted facade whose entries the folder composes; `Lease` enrols then reserves or renders outside the atom before committing with its first grant.
- [05]-[SURFACE_LEDGER]: owner-to-ingress-to-state-to-egress roster across `BlockLifecycle`, `BlockVault`, `PreviewGrant`, and the policy owners.

## [02]-[REFRESH_POLICY]

`RefreshPolicy` partitions matching versions by grant state. Every row removes and closes zero-grant versions; `Lazy` keeps granted versions stale, `Eager` keeps them stale and regenerates them, and `Drop` moves them to retirement. A caller's row is a REQUEST: `Invalidate` resolves it per definition through `RefreshPolicy.Of` before its pure transition, so the sweep answers over live host state rather than the caller's wish.

- Law: a row's behaviour is ONE `CapabilitySet<RefreshAction>` column — `Rerender` and `Retire` are the two actions a sweep can take on a GRANTED version, the two bool columns that spelled them delete, and the one illegal corner (a sweep both regenerating and retiring the same granted version) is a `CapabilityLaw` refusal at the roster rather than a combination two independent bools silently admitted.
- Law: grant state alone does not decide a refresh — `RefreshPolicy.Of` resolves over the PRODUCT of grant state and the link conditions a definition carries: the `SourceMode` row behind `InstanceDefinition.UpdateType` declares WHICH conditions it requires (`Requires : CapabilitySet<LinkCondition>`, model.md), and the definition's live `LinkSubject` declares which it HOLDS. Regenerating a `Never` document's LINKED definition from a changed external file is exactly the case the condition algebra forecloses — `Regenerates` is one `AdmitsAll` read, and the refusal DERIVES from the missing condition rather than a ternary re-deriving it from two bools.
- Law: no raw host discriminant reaches a `LinkSubject` column — `UpdateStyle` re-closes the document update policy with its `Updates` column and `SourceHealth` (model.md) is the branch's one archive-status vocabulary, so `Conditions` reads row behaviour rather than re-spelling an `is-not-one-row` comparison a new host ordinal silently joins.
- Law: source AVAILABILITY is a condition, not a failure — an eager row over a broken source resolves to the stale-keep arm before the sweep, and `SourceHealth` names no row for `NotALinkedInstanceDefinition`, so an unlinked definition carries `Option<SourceHealth>.None` — absence, not a row meaning "not applicable".
- Law: `SkipNestedLinkedDefinitions` is settable on the live definition, so a nested-load posture is a subject column the refresh writes once at admission, never re-derived per version sweep.
- Packages: Thinktecture.Runtime.Extensions for the rosters and generated admission; NodaTime for the debounce carrier (`libs/dotnet/.api/api-nodatime.md`); LanguageExt.Core for the carriers; `Domain/validation` for `ICapability`/`CapabilitySet`/`CapabilityLaw` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md`, `api-languageext.md`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using NodaTime;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RefreshAction : ICapability<RefreshAction> {
    public static readonly RefreshAction Rerender = new(key: "rerender");
    public static readonly RefreshAction Retire = new(key: "retire");

    public static CapabilityLaw<RefreshAction> Law => Corner.Value;
    private static readonly Lazy<CapabilityLaw<RefreshAction>> Corner = new(static () =>
        CapabilityLaw<RefreshAction>.Forbidden(Seq(CapabilitySet<RefreshAction>.Of(Rerender, Retire))));
}

[SmartEnum<int>]
public sealed partial class RefreshPolicy {
    public static readonly RefreshPolicy Lazy = new(key: 0, actions: CapabilitySet<RefreshAction>.Of());
    public static readonly RefreshPolicy Eager = new(key: 1, actions: CapabilitySet<RefreshAction>.Of(RefreshAction.Rerender));
    public static readonly RefreshPolicy Drop = new(key: 2, actions: CapabilitySet<RefreshAction>.Of(RefreshAction.Retire));

    public CapabilitySet<RefreshAction> Actions { get; }

    public static RefreshPolicy Of(LinkSubject subject, RefreshPolicy requested) =>
        !requested.Actions.Admits(capability: RefreshAction.Rerender) || subject.Mode.Regenerates(held: subject.Conditions)
            ? requested
            : Lazy;
}

[SmartEnum<LinkedInstanceDefinitionUpdateStyle>]
public sealed partial class UpdateStyle {
    public static readonly UpdateStyle Prompt = new(key: LinkedInstanceDefinitionUpdateStyle.Prompt, updates: true);
    public static readonly UpdateStyle Always = new(key: LinkedInstanceDefinitionUpdateStyle.AlwaysUpdate, updates: true);
    public static readonly UpdateStyle Never = new(key: LinkedInstanceDefinitionUpdateStyle.NeverUpdate, updates: false);

    public bool Updates { get; }
}

[ComplexValueObject]
[ValidationError]
public sealed partial class LinkSubject {
    public SourceMode Mode { get; }
    public UpdateStyle Style { get; }
    public Option<SourceHealth> Health { get; }
    public bool SkipNested { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref SourceMode mode,
        ref UpdateStyle style,
        ref Option<SourceHealth> health,
        ref bool skipNested) =>
        validationError = mode is not null && style is not null
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { nameof(LinkSubject), "an admitted source mode and update style", Option<>.None }));

    public static Fin<LinkSubject> Of(InstanceDefinition definition, RhinoDoc document) =>
        from mode in SourceMode.Of(update: definition.UpdateType)
        from style in FactoryBridge.Row<LinkedInstanceDefinitionUpdateStyle, UpdateStyle>(document.LinkedInstanceDefinitionUpdate)
        let health = SourceHealth.Of(status: definition.ArchiveFileStatus)
        from admitted in FactoryBridge.Accept<LinkSubject>(
            fault: Validate(mode, style, health, definition.SkipNestedLinkedDefinitions, out LinkSubject? subject),
            admitted: subject)
        select admitted;

    internal CapabilitySet<LinkCondition> Conditions {
        get {
            CapabilitySet<LinkCondition> held = CapabilitySet<LinkCondition>.Of();
            held = Style.Updates ? held.With(capability: LinkCondition.Styled) : held;
            return Health.Exists(static row => !row.Condition.Admits(capability: ArchiveCondition.Broken))
                ? held.With(capability: LinkCondition.Readable)
                : held;
        }
    }

}

[ComplexValueObject]
[ValidationError]
public sealed partial class LinkWatchPolicy {
    public Duration Debounce { get; }
    public StreamPolicy Policy { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Duration debounce,
        ref StreamPolicy stream) =>
        validationError = debounce >= Duration.Zero && stream is not null
            ? validationError
            : new ValidationError(string.Join(" | ", new object?[] { nameof(LinkWatchPolicy), "a non-negative debounce and a facts bound", Option<>.None }));

    public static Fin<LinkWatchPolicy> Of(Duration debounce, StreamPolicy stream) =>
        FactoryBridge.Accept<LinkWatchPolicy>(
            fault: Validate(debounce, stream, out LinkWatchPolicy? policy),
            admitted: policy);
}
```

## [03]-[PREVIEW_CUSTODY]

`PreviewKey` combines document enrolment, definition, and structural request identity. `PreviewGrant` is the only public bitmap window; `Commit` mints each version above every live and retired version the key already holds, so a re-created key after eviction never aliases a retired grant. Superseded versions with grants move atomically to `Retired`; zero-grant versions release after publication and surface cleanup faults on the reachable grant, while a failed transition releases the uncommitted image.

- Law: `DocKey` alone cannot key the vault — it IS `RhinoDoc.RuntimeSerialNumber`, which the host RECYCLES across a close/open pair, so a document that closes holding retired grants aliases the next document handed the same serial and that document's first lease reads the dead document's bitmaps. `DocSeal` is the per-enrolment monotonic stamp that closes the alias: the key carries `(DocSeal, DocKey)`, close-eviction drops the seal with the entries, and a re-opened document mints a fresh seal that cannot address a parked row.
- Law: `VaultOutcome` carries transition products without captured mutation, and every product carries the vault's `Lease<GdiBitmap>` rather than the raw handle — a consumer reaches the image only through `PreviewGrant.Use`, a borrow the claim gate closes, so no caller can dispose an image the vault still owns. The sparse consumers read `SwitchPartially` over the family — a hand `is` probe beside the generated dispatch is the arm that silently stops matching when a case lands.
- Law: the grant composes the package's `LifecycleGate` (`Document/lifetime.md`) — `Use` takes a claim for the whole body and `Dispose` closes through the same gate, so a release ISSUED DURING a live borrow waits on that claim rather than freeing a bitmap the body still reads. A released-flag check before the body is a check, not a gate. A grant hand-rolling a `lock`/`Monitor` release machine beside the capsule is the collapsed form.
- Law: a grant's cleanup roster is a total APPEND cell — the S9 carve-out for an unconditional step — so the fault a `Dispose` cannot carry outward parks readable on `CleanupFaults` and no verdict exists to discard.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using GdiBitmap = System.Drawing.Bitmap;
using Rasm.Domain;
using Rasm.Rhino.Document;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DocSeal(long Value);

public sealed record PreviewKey : IDetachedDocumentResult {
    internal PreviewKey(DocSeal seal, DocKey document, Guid definition, BlockPreview spec) =>
        (Seal, Document, Definition, Spec) = (seal, document, definition, spec);

    public DocSeal Seal { get; }
    public DocKey Document { get; }
    public Guid Definition { get; }
    public BlockPreview Spec { get; }
}

internal sealed record PreviewEntry(int Version, Lease<GdiBitmap> Image, int Grants, bool Stale);

[Union(
    ConversionFromValue = ConversionOperatorsGeneration.None,
    MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal abstract partial record VaultOutcome {
    private VaultOutcome() { }
    public sealed record Miss : VaultOutcome;
    public sealed record Granted(int Version, Lease<GdiBitmap> Image) : VaultOutcome;
    public sealed record Committed(int Version, Lease<GdiBitmap> Image, Seq<Lease<GdiBitmap>> Closing) : VaultOutcome;
    public sealed record Swept(Seq<(PreviewKey Key, SweepAction Action, Option<Lease<GdiBitmap>> Closing)> Rows) : VaultOutcome;
    public sealed record Discharged(Option<Watch> Observation) : VaultOutcome;

    internal static readonly VaultOutcome Clean = new Swept(
        Rows: Seq<(PreviewKey, SweepAction, Option<Lease<GdiBitmap>>)>());
}

[SmartEnum<int>]
internal sealed partial class SweepAction {
    internal static readonly SweepAction Freed = new(key: 0);
    internal static readonly SweepAction Retired = new(key: 1);
    internal static readonly SweepAction Rerender = new(key: 2);
    internal static readonly SweepAction Unresolved = new(key: 3);
}

internal sealed record DocEnrolment(DocSeal Seal, Watch Observation);

internal sealed record VaultState(
    HashMap<PreviewKey, PreviewEntry> Live,
    HashMap<(PreviewKey Key, int Version), PreviewEntry> Retired,
    VaultOutcome LastOutcome) {
    internal static readonly VaultState Empty = new(
        Live: HashMap<PreviewKey, PreviewEntry>(),
        Retired: HashMap<(PreviewKey, int), PreviewEntry>(),
        LastOutcome: VaultOutcome.Clean);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PreviewGrant : IDisposable {
    private readonly LifecycleGate gate;
    private readonly Lease<GdiBitmap> image;
    private readonly Func<Seq<Error>> release;
    private readonly Atom<Seq<Error>> cleanupFaults;

    private PreviewGrant(
        PreviewKey key,
        int version,
        LifecycleGate gate,
        Lease<GdiBitmap> image,
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
        Lease<GdiBitmap> image,
        Seq<Error> cleanupFaults,
        Func<Seq<Error>> release) =>
        LifecycleGate.Of(settleWithin: BlockVault.GrantSettle.ToTimeSpan()).Map(gate => new PreviewGrant(version: version,
            gate: gate,
            image: image,
            cleanupFaults: cleanupFaults,
            release: release));

    public Fin<T> Use<T>(Func<GdiBitmap, Fin<T>> body) {
        return Admit.Need(body).Bind(run => gate.Within(
            body: () => run(arg: image.Resource),
            refused: () => Fin.Fail<T>(error: new KernelFault.InvalidContext())));
    }

    public void Dispose() {
        _ = gate.Close(
            stop: static () => Fin.Succ(value: unit),
            settle: () => BlockVault.Lowered(faults: release())).Match(
                Succ: static _ => unit,
                Fail: error => ignore(cleanupFaults.Swap(f: held => held.Add(value: error))));
    }
}
```

## [04]-[LIFECYCLE]

`BlockVault` is the algebra: enrolment, invalidation, eviction, grant, and release as kernel `Cell` transitions over instance state. `BlockLifecycle` is the mounted facade the folder composes — its one process-global is the MOUNT SEAT, a `Cell.Seat` cell whose sole writer is the shell capsule's `ShellMount.Vault` case at plug-in load, so a second claimant reads a typed refusal and an unmounted process refuses `MissingContext` at every entry instead of consulting a vault nobody seated (branch RULINGS `[02]`: composition roots stay at the shell; `Plugin/lifecycle.md`'s load root routes the capsule mount).

`Engage` observes definition-table, worksession-file, and document-close facts through deferred document delivery and answers the document's `DocSeal`. Definition and worksession facts invalidate the document; close facts evict it, discharging the enrolment with the entries. No mutation runs inside the host callback that raised the table event.

`Lease` enrols first, then reserves a fresh cached version or renders outside the atom and commits the owned lease with its first grant. Eager regeneration and closing-image cleanup settle as independent applicative attempts, and every failure remains typed before the fold returns.

- Law: enrolment is idempotent and every vault ingress walks it, so a document holding entries always holds the observation that evicts them — eviction is a consequence of leasing, never a second call. Enrolment is `Cell.Claim` over the keyed enrolment cell: first claim wins, and the LOSING claim's already-minted `Watch` closes on the `Ceded` arm — the mint ran once outside the CAS, so the surplus is this caller's to close and a dropped watch (a live host subscription no owner can reach) is unrepresentable.
- Law: every vault transition is `Cell.Commit` reading its product off `Transition.Current.LastOutcome` — the committed state IS this caller's, so concurrent transitions never overwrite one another's product and the former ticket registry, outcome map, and second cleanup swap delete whole. A refused or contended commit reads its case; the one total-append cell (the grant's cleanup roster) stays a plain swap under the S9 carve-out.
- Law: `WatchLinked` accepts one admitted observation policy, projects its NodaTime debounce to the host span at the ONE observation boundary, and commits one typed `Refresh` transaction per settled change; the clock resolves at the entry (`TimeProvider.System` absent a caller's), never off a value-object column.
- Law: `ReleaseAll` delegates to kernel `Custody.Release`; a page spelling its own release loop beside it is the deleted form, and `Lowered` aggregates an already-collected fault roster.
- Packages: `Document/lifetime.md` for `LifecycleGate`, `Watch`/`SubscriptionRelease`; kernel `Domain/results` for `Custody`; `Document/events.md` for `DocumentStream`/`Observation`/`EventFamily`/`Delivery`/`StreamPolicy`; `Domain/results` for `Cell`/`Transition`/`Lease` (`libs/dotnet/.api/api-languageext.md`); NodaTime for `Duration`.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using GdiBitmap = System.Drawing.Bitmap;
using NodaTime;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;

namespace Rasm.Rhino.Blocks;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class BlockVault {
    private readonly Atom<VaultState> vault = Atom(value: VaultState.Empty);
    private readonly Atom<HashMap<DocKey, DocEnrolment>> enrolled = Atom(HashMap<DocKey, DocEnrolment>());
    private readonly Atom<long> seals = Atom(0L);

    internal static readonly Duration GrantSettle = Duration.FromSeconds(5);

    public static BlockVault Of() => new();

    internal Fin<DocSeal> Enrol(DocumentSession owner, RefreshPolicy policy) =>
        from watch in DocumentStream.Observe(request: new Observation.Host(
            Scope: new EventScope.Document(Key: owner.Key),
            Families: Seq(
                EventFamily.InstanceDefinitionTable,
                EventFamily.WorksessionFile,
                EventFamily.Closed),
            Delivery: new Delivery.Deferred(Sink: fact => Delivered(fact: fact, owner: owner, policy: policy)),
            Policy: StreamPolicy.Operational))
        let seal = new DocSeal(Value: Cell.Commit(seals, static held => held + 1L).Current)
        from seated in Cell.Claim(enrolled, owner.Key, () => new DocEnrolment(Seal: seal, Observation: watch)).Switch(
            state: watch,
            committed: (held, row) => row.State.Find(owner.Key)
                .Map(static entry => entry.Seal)
                .ToFin(Fail: new KernelFault.InvalidResult()),
            ceded: (held, row) => row.State.Find(owner.Key)
                .Map(static entry => entry.Seal)
                .ToFin(Fail: new KernelFault.InvalidResult())
                .Bind(seated => Closed(watch: held).Map(_ => seated)),
            refused: static (held, row) => Fin.Fail<DocSeal>(error: row.Cause),
            contended: static (held, _) => Fin.Fail<DocSeal>(error: new KernelFault.InvalidResult()))
        select seated;

    internal Fin<DocSeal> Engage(DocumentSession session, RefreshPolicy policy) =>
        from owner in Admit.Need(session)
        from active in Admit.Need(policy)
        from seal in enrolled.Value.Find(owner.Key).Match(
            Some: static held => Fin.Succ(value: held.Seal),
            None: () => Enrol(owner: owner, policy: active))
        select seal;

    private static Fin<Unit> Closed(Watch watch) => Try.lift(() =>
        watch.Close() is SubscriptionRelease.Faulted faulted
            ? Lowered(faults: faulted.Errors)
            : Fin.Succ(value: unit)).Run().Bind(static inner => inner);

    private Fin<Unit> Delivered(DocEvent fact, DocumentSession owner, RefreshPolicy policy) {
        return fact.Origin switch {
            EventOrigin.Host { Family: var family } when family == EventFamily.Closed => Evict(document: owner.Key),
            EventOrigin.Host { Family: var family }
                when family == EventFamily.InstanceDefinitionTable || family == EventFamily.WorksessionFile =>
                Invalidate(document: owner.Key, policy: policy, session: Some(owner)),
            _ => Fin.Succ(value: unit),
        };
    }

    internal Fin<PreviewGrant> Lease(DocumentSession owner, ResourceRef address, BlockPreview request) =>
        from seal in Engage(session: owner, policy: RefreshPolicy.Lazy)
        from key in owner.Demand(
            use: document => Definitions.Resolve(target: address, document: document)
                .Map(definition => new PreviewKey(
                    seal: seal,
                    document: owner.Key,
                    definition: definition.Id,
                    spec: request)),
            needs: [SessionNeed.Read])
        from cached in TryGrant()
        from grant in cached.Match(
            Some: static held => Fin.Succ(value: held),
            None: () => Render(session: owner, target: address))
        select grant;

    internal Fin<Unit> Evict(DocKey document) =>
        from _ in Invalidate(
            document: document,
            policy: RefreshPolicy.Drop,
            session: Option<DocumentSession>.None)
        from discharged in Cell.Step(
                enrolled,
                held => held.Find(document).Map(_ => held.Remove(key: document)),
                new KernelFault.InvalidResult()).Switch(
            state: enrolled.Value.Find(document),
            committed: static (held, _) => Fin.Succ(value: held),
            ceded: static (held, _) => Fin.Fail<Option<DocEnrolment>>(error: new KernelFault.InvalidResult()),
            refused: static (_, _) => Fin.Succ(Option<DocEnrolment>.None),
            contended: static (held, _) => Fin.Fail<Option<DocEnrolment>>(error: new KernelFault.InvalidResult()))
        from closed in discharged
            .TraverseM(held => Closed(watch: held.Observation)).As()
            .Map(static _ => unit)
        select unit;

    internal Fin<Watch> WatchLinked(
        DocumentSession owner,
        ResourceRef address,
        string source,
        LinkWatchPolicy active,
        Option<TimeProvider> clock) =>
        DocumentStream.Observe(request: new Observation.File(
            Path: source,
            Debounce: active.Debounce.ToTimeSpan(),
            Clock: clock.IfNone(TimeProvider.System),
            Delivery: new Delivery.Deferred(Sink: _ =>
                from plan in BlockTransaction.Batch(
                    name: nameof(WatchLinked),
                    redraw: RedrawPolicy.Deferred,
                    operations: [new BlockOp.Refresh(Target: address)])
                from __ in Blocks.Commit(session: owner, transaction: plan)
                select unit),
            Policy: active.Policy));

    private Fin<Unit> Invalidate(
        DocKey document,
        RefreshPolicy policy,
        Option<DocumentSession> session) =>
        Resolved(document: document, requested: policy, session: session).Bind(resolved =>
        Commit(transition: state => {
            Seq<(PreviewKey Key, PreviewEntry Entry)> hit = state.Live.AsIterable()
                .Filter(pair => pair.Key.Document == document)
                .Map(static pair => (pair.Key, pair.Value))
                .ToSeq();
            Seq<(PreviewKey Key, SweepAction Action, Option<Lease<GdiBitmap>> Closing)> rows = hit.Map(row =>
                resolved.Find(row.Key.Definition).Match(
                    None: () => (row.Key, SweepAction.Unresolved, Option<Lease<GdiBitmap>>.None),
                    Some: effective => row.Entry.Grants == 0
                        ? (row.Key, SweepAction.Freed, Some(row.Entry.Image))
                        : effective.Actions.Admits(capability: RefreshAction.Retire)
                            ? (row.Key, SweepAction.Retired, Option<Lease<GdiBitmap>>.None)
                            : effective.Actions.Admits(capability: RefreshAction.Rerender)
                                ? (row.Key, SweepAction.Rerender, Option<Lease<GdiBitmap>>.None)
                                : (row.Key, SweepAction.Unresolved, Option<Lease<GdiBitmap>>.None)));
            VaultState next = rows.Fold(
                state,
                (fold, row) => row.Action == SweepAction.Freed
                    ? fold with { Live = fold.Live.Remove() }
                    : row.Action == SweepAction.Retired
                        ? fold.Live.Find(row.Key).Match(
                            Some: entry => fold with {
                                Live = fold.Live.Remove(),
                                Retired = fold.Retired.AddOrUpdate(key: (row.Key, entry.Version), value: entry),
                            },
                            None: () => fold)
                        : fold.Live.Find(row.Key).Match(
                            Some: entry => fold with {
                                Live = fold.Live.AddOrUpdate(value: entry with { Stale = true }),
                            },
                            None: () => fold));
            return (next, new VaultOutcome.Swept(Rows: rows));
        }).Bind(outcome => outcome.SwitchPartially(
            state: (Session: session, Cell: this),
            @default: static (held, _) => Fin.Fail<Unit>(error: new KernelFault.InvalidResult()),
            swept: (held, swept) => Attempted(() => Lowered(faults: ReleaseAll(
                        images: swept.Rows.Choose(static row => row.Closing))),
                    () => swept.Rows
                        .Filter(static row => row.Action == SweepAction.Rerender)
                        .Traverse(row => held.Session.ToFin(Fail: new KernelFault.MissingContext())
                            .Bind(active => Rerendered(session: active))
                            .ToValidation())
                        .As()
                        .ToFin()
                        .Map(static _ => unit))));

    private Fin<HashMap<Guid, RefreshPolicy>> Resolved(
        DocKey document,
        RefreshPolicy requested,
        Option<DocumentSession> session) =>
        session.Match(
            None: () => Fin.Succ(vault.Value.Live.AsIterable()
                .Filter(pair => pair.Key.Document == document)
                .Map(pair => (pair.Key.Definition, requested))
                .ToHashMap()),
            Some: active => active.Demand(
                use: held => vault.Value.Live.AsIterable()
                    .Filter(pair => pair.Key.Document == document)
                    .Map(static pair => pair.Key.Definition)
                    .Distinct()
                    .ToSeq()
                    .Traverse(definition => (
                        from target in ResourceRef.Of(id: definition)
                        from resolved in Definitions.Resolve(target: target, document: held)
                        from subject in LinkSubject.Of(definition: resolved, document: held)
                        let effective = RefreshPolicy.Of(subject: subject, requested: requested)
                        select (Definition: definition, Effective: effective)).ToValidation())
                    .As()
                    .ToFin()
                    .Map(static rows => rows.Map(static row => (row.Definition, row.Effective)).ToHashMap()),
                needs: [SessionNeed.Read]));

    private Fin<Unit> Rerendered(DocumentSession session, PreviewKey key) =>
        ResourceRef.Of(id: key.Definition)
            .Bind(target => Render(session: session, target: target))
            .Bind(grant => Try.lift(() => {
                grant.Dispose();
                return Lowered(faults: grant.CleanupFaults);
            }).Run().Bind(static inner => inner));

    private Fin<PreviewGrant> Render(DocumentSession session, ResourceRef target, PreviewKey key) =>
        Blocks.Ask(session: session, request: new BlockAsk.Preview(Target: target, Spec: key.Spec)).Bind(answer =>
            answer is BlockAnswer.Rendered rendered
                ? Committed(image: rendered.Preview)
                : Fin.Fail<PreviewGrant>(error: new KernelFault.InvalidResult()));

    private Fin<PreviewGrant> Committed(PreviewKey key, Lease<GdiBitmap> image) =>
        Commit(transition: state => {
                Option<PreviewEntry> prior = state.Live.Find(owner.Key);
                int liveVersion = prior.Map(static held => held.Version).IfNone(noneValue: 0);
                int retiredVersion = state.Retired.AsIterable()
                    .Filter(pair => pair.Key.Key == key)
                    .Fold(0, static (high, pair) => int.Max(high, pair.Key.Version));
                int version = int.Max(liveVersion, retiredVersion) + 1;
                Seq<Lease<GdiBitmap>> closing = prior
                    .Filter(static held => held.Grants == 0)
                    .Map(static held => Seq(held.Image))
                    .IfNone(Seq<Lease<GdiBitmap>>());
                PreviewEntry next = new(Version: version, Image: image, Grants: 1, Stale: false);
                VaultState changed = state with {
                    Live = state.Live.AddOrUpdate(value: next),
                    Retired = prior.Filter(static held => held.Grants > 0)
                        .Map(held => state.Retired.AddOrUpdate(key: (held.Version), value: held))
                        .IfNone(state.Retired),
                };
                return (changed, new VaultOutcome.Committed(Version: version, Image: image, Closing: closing));
            })
            .Bind(outcome => outcome.SwitchPartially(
                state: (Key: key, Cell: this),
                @default: static (held, _) => Fin.Fail<PreviewGrant>(error: new KernelFault.InvalidResult()),
                committed: (held, committed) => Granted(version: committed.Version,
                    image: committed.Image,
                    cleanupFaults: ReleaseAll(images: committed.Closing))))
            .Rollback(release: () => Lowered(faults: ReleaseAll(images: Seq(image))));

    private Fin<Option<PreviewGrant>> TryGrant(PreviewKey key) =>
        Commit(transition: state => state.Live.Find(row.Key).Case switch {
            PreviewEntry { Stale: false } current => (
                state with {
                    Live = state.Live.AddOrUpdate(value: current with { Grants = current.Grants + 1 }),
                },
                new VaultOutcome.Granted(Version: current.Version, Image: current.Image)),
            _ => (state, new VaultOutcome.Miss()),
        }).Bind(outcome => outcome.SwitchPartially(
            state: (Key: key, Cell: this),
            @default: static (_, _) => Fin.Succ(Option<PreviewGrant>.None),
            granted: (held, granted) => Granted(version: granted.Version,
                    image: granted.Image,
                    cleanupFaults: Seq<Error>())
                .Map(Some)));

    private Fin<PreviewGrant> Granted(
        PreviewKey key,
        int version,
        Lease<GdiBitmap> image,
        Seq<Error> cleanupFaults) =>
        PreviewGrant.Of(version: version,
            image: image,
            cleanupFaults: cleanupFaults,
            release: () => Release(version: version));

    private static Seq<Error> ReleaseAll(Seq<Lease<GdiBitmap>> images) =>
        Custody.Release(
                held: images,
                release: image => Try.lift(() => { image.Dispose(); return Fin.Succ(value: unit); }).Run().Bind(static inner => inner))
            .Match(Succ: static _ => Seq<Error>(), Fail: static error => Seq(error));

    internal static Fin<Unit> Lowered(Seq<Error> faults) =>
        faults.IsEmpty ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: Error.Many(faults));

    private static Fin<Unit> Attempted(params ReadOnlySpan<Func<Fin<Unit>>> attempts) =>
        Custody.Release(releases: toSeq(attempts.ToArray()));

    private Seq<Error> Release(PreviewKey key, int version) {
        return Commit(transition: state => state.Live.Find(row.Key).Case switch {
                PreviewEntry current when current.Version == version => (
                    state with {
                        Live = state.Live.AddOrUpdate(value: current with { Grants = int.Max(current.Grants - 1, 0) }),
                    },
                    VaultOutcome.Clean),
                _ => state.Retired.Find(key: (version)).Case switch {
                    PreviewEntry parked when parked.Grants <= 1 => (
                        state with { Retired = state.Retired.Remove(key: (version)) },
                        (VaultOutcome)new VaultOutcome.Swept(
                            Rows: Seq((SweepAction.Freed, Some(parked.Image))))),
                    PreviewEntry parked => (
                        state with {
                            Retired = state.Retired.AddOrUpdate(
                                key: (version),
                                value: parked with { Grants = parked.Grants - 1 }),
                        },
                        VaultOutcome.Clean),
                    _ => (state, VaultOutcome.Clean),
                },
            })
            .Match(
                Succ: outcome => outcome is VaultOutcome.Swept swept
                    ? ReleaseAll(images: swept.Rows.Choose(static row => row.Closing))
                    : Seq<Error>(),
                Fail: static error => Seq(error));
    }

    private Fin<VaultOutcome> Commit(
        Func<VaultState, (VaultState State, VaultOutcome Outcome)> transition) => Try.lift(() =>
        Cell.Commit(vault, state => {
            (VaultState next, VaultOutcome outcome) = transition(arg: state);
            return next with { LastOutcome = outcome };
        }).Switch(
            committed: static row => Fin.Succ(value: row.State.LastOutcome),
            ceded: static () => Fin.Fail<VaultOutcome>(error: new KernelFault.InvalidResult()),
            refused: static row => Fin.Fail<VaultOutcome>(error: row.Cause),
            contended: static () => Fin.Fail<VaultOutcome>(error: new KernelFault.InvalidResult()))).Run().Bind(static inner => inner);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class BlockLifecycle {
    private static readonly Atom<Option<BlockVault>> Seat = Atom(Option<BlockVault>.None);

    public static Fin<Unit> Mount(BlockVault vault) {
        return Cell.Seat(Seat, () => vault).Switch(
            committed: static _ => Fin.Succ(value: unit),
            ceded: static (held) => Fin.Fail<Unit>(error: new KernelFault.InvalidContext()),
            refused: static row => Fin.Fail<Unit>(error: row.Cause),
            contended: static (held) => Fin.Fail<Unit>(error: new KernelFault.InvalidResult()));
    }

    private static Fin<BlockVault> Mounted() => Seat.Value.ToFin(Fail: new KernelFault.MissingContext());

    public static Fin<DocSeal> Engage(DocumentSession session, RefreshPolicy policy) {
        return Mounted().Bind(vault => vault.Engage(session: session, policy: policy));
    }

    public static Fin<PreviewGrant> Lease(DocumentSession session, ResourceRef target, BlockPreview spec) {
        return from vault in Mounted()
               from owner in Admit.Need(session)
               from address in Admit.Need(target)
               from request in Admit.Need(spec)
               from grant in vault.Lease(owner: owner, address: address, request: request)
               select grant;
    }

    public static Fin<Unit> Evict(DocKey document) {
        return Mounted().Bind(vault => vault.Evict(document: document));
    }

    public static Fin<Watch> WatchLinked(
        DocumentSession session,
        ResourceRef target,
        string path,
        LinkWatchPolicy policy,
        Option<TimeProvider> clock = default) {
        return from vault in Mounted()
               from owner in Admit.Need(session)
               from address in Admit.Need(target)
               from active in Admit.Need(policy)
               from source in Acceptance.Text(value: path)
               from watch in vault.WatchLinked(
                   owner: owner, address: address, source: source, active: active, clock: clock)
               select watch;
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]          | [INGRESS]                              | [STATE]                        | [EGRESS]                      |
| :-----: | :--------------- | :------------------------------------- | :----------------------------- | :---------------------------- |
|  [01]   | `BlockLifecycle` | `Mount` · `Engage` · `Lease` · `Evict` | `Cell.Seat` mount cell         | seal · grant · `Unit`         |
|  [02]   | `BlockVault`     | capsule mint (`ShellMount.Vault`)      | `Atom<VaultState>` + enrolment | `Transition`-carried products |
|  [03]   | `PreviewGrant`   | `PreviewGrant.Of`                      | `LifecycleGate` claims         | claim-gated bitmap borrow     |
|  [04]   | policy owners    | generated admission                    | invalidation decisions         | policy values                 |
|  [05]   | `DocSeal`        | enrolment claim                        | recycled-serial guard          | `PreviewKey` identity half    |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
