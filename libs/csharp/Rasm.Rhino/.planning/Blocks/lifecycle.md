# [RASM_RHINO_BLOCK_LIFECYCLE]

The block lifecycle rail (`Rasm.Rhino.Blocks`). One owner carries host event ingress, versioned preview leasing, deferred refresh, document eviction, and native disposable release: definition-table and document-close facts arrive through the document observation stream under deferred delivery — never a second idle queue or a private event bridge — and one vault atom holds every rendered preview as a versioned entry whose bitmap disposes exactly when its last grant returns and its version is superseded. Refresh behavior is a policy row with its behavior columns, not a cache-mode enum deciding unrelated semantics, and linked-file change composes the document file observation with its debounced clock-stamped gate driving one `Refresh` commit on the operation rail. The census-era `RefCache`/`PreviewVault`/`SnapshotVault`/`BlockVaults`/`EventBridge`/`IdlePump` split collapses into this one spine.

## [01]-[INDEX]

- [02]-[REFRESH_POLICY]: `RefreshPolicy` — the invalidation behavior rows.
- [03]-[PREVIEW_VAULT]: `PreviewKey`, the versioned vault atom, and the `PreviewGrant` custody window.
- [04]-[EVENT_INGRESS]: observation attachment, document eviction, and the linked-file watch.
- [05]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[REFRESH_POLICY]

- Owner: `RefreshPolicy` keyless `[SmartEnum]` — the invalidation decision as rows with two behavior columns: `Rerender` re-renders a stale granted entry on the idle edge, `Evict` drops the entry outright; `Lazy` marks stale and re-renders on the next lease, `Eager` re-renders granted entries immediately after invalidation, `Drop` evicts on every invalidation.
- Law: the policy decides only staleness handling — grant custody, version arithmetic, and disposal are vault law identical under every row, so a row swap never changes who owns a bitmap.
- Growth: a new invalidation behavior is one row with its two columns; the invalidation fold reads it with zero new surface.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum]
public sealed partial class RefreshPolicy {
    public static readonly RefreshPolicy Lazy = new(rerender: false, evict: false);
    public static readonly RefreshPolicy Eager = new(rerender: true, evict: false);
    public static readonly RefreshPolicy Drop = new(rerender: false, evict: true);

    public bool Rerender { get; }
    public bool Evict { get; }
}
```

## [03]-[PREVIEW_VAULT]

- Owner: `PreviewKey` — the vault identity: document key, definition guid, and the structural `PreviewSpec`, so two specs never alias one image; `PreviewEntry` — the internal versioned holding: owned bitmap lease, version, grant count, staleness; `VaultState` — the ONE CAS domain: the live map plus the retired ledger of superseded versions still under grant; `PreviewGrant` — the consumer custody window: the bitmap is readable for exactly the grant's lifetime, and disposal returns the reference.
- Entry: `BlockLifecycle.Lease(DocumentSession, BlockRef, PreviewSpec) : Fin<PreviewGrant>` — a fresh vault hit grants the held version; a miss or a stale entry renders through the operation rail's `Preview` ask outside the swap, commits the new version under one swap, and grants the new image.
- Law: disposal is version-and-grant arithmetic across both maps — a superseded or evicted version with live grants parks in the retired ledger and disposes when its LAST grant returns; a zero-grant version disposes at the supersede or evict transition; a live grant therefore never dereferences a disposed native, and an unreferenced image never lingers.
- Law: live and retired transition as one value — `VaultState` is one atom, so a supersede that must both replace the live entry and park the prior version commits in one CAS; two separate cells expose a half-moved state to a concurrent release, and the single atom makes that state unobservable.
- Law: rendering runs outside the swap and the grant reserves at the commit — the host `CreatePreviewBitmap` call is acquisition, the swap commits the new version with its first grant already counted, and a racing supersede therefore parks the prior granted version instead of disposing under a live window; nothing renders or disposes inside a CAS body.
- Law: the grant is the one public bitmap window — no result shape carries a bare `System.Drawing.Bitmap`, and a consumer keeping an image past its grant copies it inside the window.

```csharp
// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PreviewKey(DocKey Document, Guid Definition, PreviewSpec Spec) : IDetachedDocumentResult;

internal sealed record PreviewEntry(int Version, Lease<System.Drawing.Bitmap> Image, int Grants, bool Stale);

internal sealed record VaultState(
    HashMap<PreviewKey, PreviewEntry> Live,
    HashMap<(PreviewKey Key, int Version), PreviewEntry> Retired) {
    internal static readonly VaultState Empty = new(
        Live: HashMap<PreviewKey, PreviewEntry>(),
        Retired: HashMap<(PreviewKey, int), PreviewEntry>());
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class PreviewGrant : IDisposable {
    private readonly Action release;
    private int released;

    internal PreviewGrant(PreviewKey key, int version, System.Drawing.Bitmap image, Action release) {
        Key = key;
        Version = version;
        Image = image;
        this.release = release;
    }

    public PreviewKey Key { get; }
    public int Version { get; }
    public System.Drawing.Bitmap Image { get; }

    public void Dispose() =>
        _ = Interlocked.Exchange(location1: ref released, value: 1) is 0 ? fun(release)() : unit;
}
```

## [04]-[EVENT_INGRESS]

- Owner: `BlockLifecycle` — the one spine: the vault atom, the session-scoped engagement, the invalidation fold, the lease entry, the eviction fold, and the linked-file watch.
- Entry: `Engage(DocumentSession, RefreshPolicy) : Fin<Watch>` binds ONE deferred observation over the definition-table and document-close families scoped to the session's document; `Lease(DocumentSession, BlockRef, PreviewSpec) : Fin<PreviewGrant>`; `Evict(DocKey) : Unit` releases every entry of a closed document; `WatchLinked(DocumentSession, BlockRef, string, TimeSpan, TimeProvider) : Fin<Watch>` drives a debounced `Refresh` commit per settled file change.
- Law: ingress is the document stream, never a private handler — the observation is `Observation.Host` under `EventScope.Document(session.Key)`, the keyed definition-table `Component` fact invalidates that document's entries, and the keyed `Closed` fact evicts them precisely; the sink rides `Delivery.Deferred`, so invalidation runs on the host idle edge, a definition-table storm inside one command coalesces, and the vault never re-renders mid-mutation.
- Law: the engagement owns the eager path with a bounded lifetime — `Eager` re-renders only stale entries still under grant through the engaging session, the watch disposes before the session it composes, and a disposed session fails a straggling re-render typed instead of touching a dead handle; an image nobody holds re-renders lazily on its next lease, so idle work is proportional to live consumers, never vault size.
- Law: the linked-file watch composes `Observation.File` — debounce window and clock ride the observation value, the fire gate is the stream's resettable one-shot timer, and the settled fire commits one `Refresh` through the operation rail so the reload carries undo, redraw, and receipt evidence; the worksession family remains the host-raised sibling for paths the host itself watches.
- Boundary: this page names no host event member — `DocumentStream`, `Watch`, `EventFamily`, and the operation rail are its whole reach.

```csharp
// --- [SERVICES] ---------------------------------------------------------------------------
public static class BlockLifecycle {
    private static readonly Atom<VaultState> Vault = Atom(value: VaultState.Empty);

    public static Fin<Watch> Engage(DocumentSession session, RefreshPolicy policy) {
        Op op = Op.Of();
        return from active in Optional(policy).ToFin(Fail: op.InvalidInput())
               from watch in DocumentStream.Observe(request: new Observation.Host(
                   Scope: new EventScope.Document(Key: session.Key),
                   Families: Seq(EventFamily.InstanceDefinitionTable, EventFamily.Closed),
                   Delivery: new Delivery.Deferred(Sink: fact =>
                       fact.Origin is EventOrigin.Host { Family: var family } && family == EventFamily.Closed
                           ? Fin.Succ(value: Evict(document: session.Key))
                           : Invalidate(document: session.Key, policy: active, session: session, op: op)),
                   Receipts: ReceiptPolicy.Operational))
               select watch;
    }

    public static Fin<PreviewGrant> Lease(DocumentSession session, BlockRef target, PreviewSpec spec) {
        Op op = Op.Of();
        return from key in session.Demand(
                   use: document => target.Resolve(document: document, key: op)
                       .Map(definition => new PreviewKey(Document: session.Key, Definition: definition.Id, Spec: spec)),
                   key: op,
                   needs: [SessionNeed.Read])
               from grant in TryGrant(key: key).Match(
                   Some: static held => Fin.Succ(value: held),
                   None: () => Render(session: session, target: target, key: key, op: op))
               select grant;
    }

    public static Unit Evict(DocKey document) {
        Seq<Lease<System.Drawing.Bitmap>> closing = Seq<Lease<System.Drawing.Bitmap>>();
        _ = Vault.Swap(f: state => {
            Seq<(PreviewKey Key, PreviewEntry Entry)> gone = state.Live.AsIterable()
                .Filter(pair => pair.Key.Document == document)
                .Map(static pair => (pair.Key, pair.Value))
                .ToSeq();
            closing = gone.Filter(static row => row.Entry.Grants == 0).Map(static row => row.Entry.Image);
            return new VaultState(
                Live: gone.Fold(state.Live, static (map, row) => map.Remove(key: row.Key)),
                Retired: gone.Filter(static row => row.Entry.Grants > 0)
                    .Fold(state.Retired, static (map, row) => map.AddOrUpdate(key: (row.Key, row.Entry.Version), value: row.Entry)));
        });
        return ignore(closing.Iter(static image => image.Dispose()));
    }

    public static Fin<Watch> WatchLinked(
        DocumentSession session, BlockRef target, string path, TimeSpan debounce, TimeProvider clock) =>
        DocumentStream.Observe(request: new Observation.File(
            Path: path,
            Debounce: debounce,
            Clock: clock,
            Delivery: new Delivery.Deferred(Sink: fact => Blocks.Commit(
                session: session,
                plan: BlockTransaction.Batch(name: nameof(WatchLinked), new BlockOp.Refresh(Target: target))).Map(static _ => unit)),
            Receipts: ReceiptPolicy.Operational));

    private static Fin<Unit> Invalidate(DocKey document, RefreshPolicy policy, DocumentSession session, Op op) {
        Seq<Lease<System.Drawing.Bitmap>> closing = Seq<Lease<System.Drawing.Bitmap>>();
        Seq<PreviewKey> granted = Seq<PreviewKey>();
        _ = Vault.Swap(f: state => {
            Seq<(PreviewKey Key, PreviewEntry Entry)> hit = state.Live.AsIterable()
                .Filter(pair => pair.Key.Document == document)
                .Map(static pair => (pair.Key, pair.Value))
                .ToSeq();
            granted = hit.Filter(static row => row.Entry.Grants > 0).Map(static row => row.Key);
            closing = policy.Evict ? hit.Filter(static row => row.Entry.Grants == 0).Map(static row => row.Entry.Image) : Seq<Lease<System.Drawing.Bitmap>>();
            return policy.Evict
                ? new VaultState(
                    Live: hit.Fold(state.Live, static (map, row) => map.Remove(key: row.Key)),
                    Retired: hit.Filter(static row => row.Entry.Grants > 0)
                        .Fold(state.Retired, static (map, row) => map.AddOrUpdate(key: (row.Key, row.Entry.Version), value: row.Entry)))
                : state with { Live = hit.Fold(state.Live, static (map, row) => map.AddOrUpdate(key: row.Key, value: row.Entry with { Stale = true })) };
        });
        _ = closing.Iter(static image => image.Dispose());
        return policy.Rerender && !policy.Evict
            ? granted.TraverseM(key => Rerendered(session: session, key: key, op: op)).As().Map(static _ => unit)
            : Fin.Succ(value: unit);
    }

    private static Fin<Unit> Rerendered(DocumentSession session, PreviewKey key, Op op) =>
        BlockRef.Of(id: key.Definition)
            .Bind(target => Render(session: session, target: target, key: key, op: op))
            .Map(static grant => fun(grant.Dispose)());

    private static Fin<PreviewGrant> Render(DocumentSession session, BlockRef target, PreviewKey key, Op op) =>
        Blocks.Ask(session: session, request: new BlockAsk.Preview(Target: target, Spec: key.Spec)).Bind(answer =>
            answer is BlockAnswer.Rendered rendered
                ? op.Catch(() => {
                    Option<Lease<System.Drawing.Bitmap>> zeroGrantPrior = Option<Lease<System.Drawing.Bitmap>>.None;
                    int committed = 0;
                    _ = Vault.Swap(f: state => {
                        Option<PreviewEntry> prior = state.Live.Find(key: key);
                        committed = prior.Map(static held => held.Version + 1).IfNone(noneValue: 1);
                        zeroGrantPrior = prior.Filter(static held => held.Grants == 0).Map(static held => held.Image);
                        PreviewEntry next = new(Version: committed, Image: rendered.Preview, Grants: 1, Stale: false);
                        return new VaultState(
                            Live: state.Live.AddOrUpdate(key: key, value: next),
                            Retired: prior.Filter(static held => held.Grants > 0)
                                .Map(held => state.Retired.AddOrUpdate(key: (key, held.Version), value: held))
                                .IfNone(state.Retired));
                    });
                    _ = zeroGrantPrior.Iter(static image => image.Dispose());
                    return Fin.Succ(value: new PreviewGrant(
                        key: key, version: committed, image: rendered.Preview.Resource,
                        release: () => Release(key: key, version: committed)));
                })
                : Fin.Fail<PreviewGrant>(error: op.InvalidResult()));

    private static Option<PreviewGrant> TryGrant(PreviewKey key) {
        Option<(int Version, System.Drawing.Bitmap Image)> taken = Option<(int, System.Drawing.Bitmap)>.None;
        _ = Vault.Swap(f: state => state.Live.Find(key: key).Case switch {
            PreviewEntry { Stale: false } current =>
                ((taken = Some((current.Version, current.Image.Resource))), state with {
                    Live = state.Live.AddOrUpdate(key: key, value: current with { Grants = current.Grants + 1 }),
                }).Item2,
            _ => ((taken = Option<(int, System.Drawing.Bitmap)>.None), state).Item2,
        });
        return taken.Map(held => new PreviewGrant(
            key: key, version: held.Version, image: held.Image,
            release: () => Release(key: key, version: held.Version)));
    }

    private static void Release(PreviewKey key, int version) {
        Option<Lease<System.Drawing.Bitmap>> closing = Option<Lease<System.Drawing.Bitmap>>.None;
        _ = Vault.Swap(f: state => state.Live.Find(key: key).Case switch {
            PreviewEntry current when current.Version == version =>
                state with { Live = state.Live.AddOrUpdate(key: key, value: current with { Grants = int.Max(current.Grants - 1, 0) }) },
            _ => state.Retired.Find(key: (key, version)).Case switch {
                PreviewEntry parked when parked.Grants <= 1 =>
                    ((closing = Some(parked.Image)), state with { Retired = state.Retired.Remove(key: (key, version)) }).Item2,
                PreviewEntry parked =>
                    state with { Retired = state.Retired.AddOrUpdate(key: (key, version), value: parked with { Grants = parked.Grants - 1 }) },
                _ => state,
            },
        });
        _ = closing.Iter(static image => image.Dispose());
    }
}
```

## [05]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]            | [OWNER]          | [FORM]                                             | [ENTRY]                                   |
| :-----: | :------------------- | :--------------- | :------------------------------------------------- | :---------------------------------------- |
|  [01]   | invalidation rows    | `RefreshPolicy`  | keyless rows, `Rerender`/`Evict` columns           | `Engage(session, policy)`                 |
|  [02]   | preview identity     | `PreviewKey`     | document + definition + structural spec            | vault keying                              |
|  [03]   | vault state          | `VaultState`     | one CAS domain: live map + retired grant ledger    | `BlockLifecycle` swaps                    |
|  [04]   | custody window       | `PreviewGrant`   | version-pinned bitmap access, idempotent return    | `Lease` / `Dispose`                       |
|  [05]   | event ingress        | `BlockLifecycle` | one deferred keyed observation over two families   | `Engage` / `Evict`                        |
|  [06]   | linked-file watching | `BlockLifecycle` | debounced `Observation.File` driving one `Refresh` | `WatchLinked(session, target, path, ...)` |
