# [RASM_RHINO_BLOCK_LIFECYCLE]

Preview lifecycle (`Rasm.Rhino.Blocks`) owns bitmap custody, versioned grants, document-scoped invalidation, linked-source refresh, and deterministic disposal. Host acquisition and disposal stay outside `Atom.Swap`; `Change` captures both swaps and every transition on one `Fin<VaultOutcome>` rail.

## [01]-[INDEX]

| [INDEX] | [OWNER]                             | [CONTRACT]                                       |
| :-----: | :---------------------------------- | :----------------------------------------------- |
|  [01]   | `RefreshPolicy` · `LinkWatchPolicy` | invalidation and observation policy              |
|  [02]   | `PreviewGrant`                      | versioned bitmap custody window                  |
|  [03]   | `BlockLifecycle`                    | observation, lease, eviction, and linked refresh |

## [02]-[REFRESH_POLICY]

`RefreshPolicy` partitions matching versions by grant state. Every row removes and closes zero-grant versions; `Lazy` keeps granted versions stale, `Eager` keeps them stale and regenerates them, and `Drop` moves them to retirement.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class RefreshPolicy {
    public static readonly RefreshPolicy Lazy = new(key: 0, rerenderGranted: false, retireGranted: false);
    public static readonly RefreshPolicy Eager = new(key: 1, rerenderGranted: true, retireGranted: false);
    public static readonly RefreshPolicy Drop = new(key: 2, rerenderGranted: false, retireGranted: true);

    public bool RerenderGranted { get; }
    public bool RetireGranted { get; }
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

`VaultOutcome` carries transition products without captured mutation. `Change` stamps one result inside the immutable state returned by `Swap`, removes only its own stamp, captures both CAS operations, and returns one fault-aware result to every caller.

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
    public sealed record Granted(int Version, System.Drawing.Bitmap Image) : VaultOutcome;
    public sealed record Committed(int Version, System.Drawing.Bitmap Image, Seq<Lease<System.Drawing.Bitmap>> Closing) : VaultOutcome;
    public sealed record Swept(Seq<Lease<System.Drawing.Bitmap>> Closing, Seq<PreviewKey> Rerender) : VaultOutcome;

    internal static readonly VaultOutcome Clean = new Swept(
        Closing: Seq<Lease<System.Drawing.Bitmap>>(),
        Rerender: Seq<PreviewKey>());
}

internal sealed record VaultReceipt(long Ticket, VaultOutcome Outcome);

internal sealed record VaultState(
    HashMap<PreviewKey, PreviewEntry> Live,
    HashMap<(PreviewKey Key, int Version), PreviewEntry> Retired,
    Option<VaultReceipt> Receipt) {
    internal static readonly VaultState Empty = new(
        Live: HashMap<PreviewKey, PreviewEntry>(),
        Retired: HashMap<(PreviewKey, int), PreviewEntry>(),
        Receipt: Option<VaultReceipt>.None);
}

// --- [SERVICES] ----------------------------------------------------------------------------
public sealed class PreviewGrant : IDisposable {
    private readonly Lock gate = new();
    private readonly Func<Seq<Error>> release;
    private Seq<Error> cleanupFaults;
    private int released;

    internal PreviewGrant(
        PreviewKey key,
        int version,
        System.Drawing.Bitmap image,
        Seq<Error> cleanupFaults,
        Func<Seq<Error>> release) {
        Key = key;
        Version = version;
        Image = image;
        this.cleanupFaults = cleanupFaults;
        this.release = release;
    }

    public PreviewKey Key { get; }
    public int Version { get; }
    public System.Drawing.Bitmap Image { get; }
    public Seq<Error> CleanupFaults {
        get { lock (gate) { return cleanupFaults; } }
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
                   Delivery: new Delivery.Deferred(Sink: fact => fact.Origin switch {
                       EventOrigin.Host { Family: var family } when family == EventFamily.Closed =>
                           Evict(document: owner.Key),
                       EventOrigin.Host { Family: var family }
                           when family == EventFamily.InstanceDefinitionTable || family == EventFamily.WorksessionFile =>
                           Invalidate(document: owner.Key, policy: active, session: Some(owner), op: op),
                       _ => Fin.Succ(value: unit),
                   }),
                   Receipts: ReceiptPolicy.Operational))
               select watch;
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

    public static Fin<Unit> Evict(DocKey document) =>
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

    private static Fin<Unit> Invalidate(
        DocKey document,
        RefreshPolicy policy,
        Option<DocumentSession> session,
        Op op) {
        return Change(transition: state => {
            Seq<(PreviewKey Key, PreviewEntry Entry)> hit = state.Live.AsIterable()
                .Filter(pair => pair.Key.Document == document)
                .Map(static pair => (pair.Key, pair.Value))
                .ToSeq();
            Seq<Lease<System.Drawing.Bitmap>> closing = hit
                .Filter(static row => row.Entry.Grants == 0)
                .Map(static row => row.Entry.Image);
            Seq<PreviewKey> rerender = policy.RerenderGranted
                ? hit.Filter(static row => row.Entry.Grants > 0).Map(static row => row.Key)
                : Seq<PreviewKey>();
            VaultState next = hit.Fold(
                (State: state, Policy: policy),
                static (fold, row) => (
                    State: row.Entry.Grants == 0 || fold.Policy.RetireGranted
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
                    Policy: fold.Policy)).State;
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
                : Fin.Fail<Unit>(error: op.InvalidResult()));
    }

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
                    Receipt: state.Receipt);
                return (changed, new VaultOutcome.Committed(
                    Version: version,
                    Image: image.Resource,
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
                new VaultOutcome.Granted(Version: current.Version, Image: current.Image.Resource)),
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
        System.Drawing.Bitmap image,
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

    private static Fin<VaultOutcome> Change(
        Func<VaultState, (VaultState State, VaultOutcome Outcome)> transition,
        Op op) => op.Catch(() => {
        long current = Interlocked.Increment(location: ref ticket);
        VaultState settled = Vault.Swap(f: state => {
            (VaultState next, VaultOutcome outcome) = transition(arg: state);
            return next with { Receipt = Some(new VaultReceipt(Ticket: current, Outcome: outcome)) };
        });
        VaultOutcome result = settled.Receipt
            .Filter(receipt => receipt.Ticket == current)
            .Map(static receipt => receipt.Outcome)
            .IfNone(new VaultOutcome.Miss());
        _ = Vault.Swap(f: state => state.Receipt.Exists(receipt => receipt.Ticket == current)
            ? state with { Receipt = Option<VaultReceipt>.None }
            : state);
        return Fin.Succ(value: result);
    });
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]          | [INGRESS]                                    | [STATE]                | [EGRESS]              |
| :-----: | :--------------- | :------------------------------------------- | :--------------------- | :-------------------- |
|  [01]   | `BlockLifecycle` | `Engage` · `Lease` · `WatchLinked` · `Evict` | `Atom<VaultState>`     | watch or grant        |
|  [02]   | `PreviewGrant`   | `Granted`                                    | release gate           | bounded bitmap access |
|  [03]   | policy owners    | generated admission                          | invalidation decisions | policy values         |

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
