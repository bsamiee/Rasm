# [UI_CACHE]

Cache keys durable browser-resident bands by `Digest.Key<"content">`, verifies every leaf, and commits through one enumerable ledger.

## [01]-[INDEX]

- [02]-[BAND_LEDGER]: Cache holds the band roster, leaf address, Schema-coded ledger, and store Tag composition obligation; `Cache`.
- [03]-[INTEGRITY_GATE]: per-leaf digest verification, the keyed-band identity law, the typed refusal family; `CacheFault`.
- [04]-[RESIDENCY_ENTRY]: `Cache.resident` — the one get-or-mint entry and its two-phase commit; `Cache`.
- [05]-[QUOTA_SWEEP]: Cache sweeps under the pressure table, victim order, and the measured reclaim receipt; `Cache`.

## [02]-[BAND_LEDGER]

[BAND_LEDGER]:
- Owner: `Cache` stores octet leaves and one `HashMap<Digest.Key<"content">, Cache.Entry>` ledger through `KeyValueStore`.
- Packages: `@effect/platform`, `effect`, and `@rasm/ts/core` (`Digest`, `Fault.Class`).
- Law: the band roster is closed and each row carries its whole policy — `keyed` states whether the addressing key must equal the content's own digest, `remintable` states whether a cold miss can reproduce the bytes, and `rank` orders eviction; a band is one row and a per-band code path is the named defect.
- Law: `draft` is never swept because it is never remintable — an upload spill is the only copy of bytes the user cannot reproduce, so pressure reclaims every other band to the floor before it touches one, and a band added without answering `remintable` truthfully turns the cache into a data-loss surface.
- Law: a bundle is named leaves, never a blob — a snapshot whose shape is several buffers (an acceleration tree's roots beside its index) stores one leaf per buffer under one key, exactly as a multi-file served asset rides one digest directory, so reassembly reads the leaf roster the entry carries and this module stays ignorant of what any band's bytes mean.
- Law: leaf order publishes ONCE, on the value carrying the bytes — `Cache.Leaves` is an ordered non-empty sequence of `{ name, octets }` and never a keyed container, so the roster a caller hands at mint IS the roster the entry records and every reader walks that sequence by POSITION with the name as its evidence. Position-addressed parts therefore survive the flatten whole: the `bvh` band files one snapshot per geometry in the residency walk's traversal order (the one stable per-geometry address a content-addressed parse fixes, and the order `viewer/scene`'s `GlbViewport.Snapshots` array contract reads back), and a read-back whose slot answers a different name refuses as `leaf-torn` instead of resolving by lookup. Deriving that order from a container's enumeration is the fork this law exists to foreclose: an OPFS read-back and a re-keyed construction enumerate differently, and the traversal silently disagrees with the digest it was built under.
- Law: the root is scoped at composition — `KeyValueStore.prefix` narrows the store to this app's own segment before the Tag reaches this module, so two apps sharing an origin share no keyspace and neither can evict the other's residency; a root spelled inside this module is the shared-cache defect.
- Law: the OPFS binding is a DIRECT Tag satisfaction and the obligation is the root's — the browser platform package ships `layerLocalStorage` and `layerSessionStorage` alone, and `KeyValueStore.layerFileSystem` demands a `FileSystem`/`Path` pair no browser implementation answers, so that route buys a twenty-member file-system port to spend six; `KeyValueStore.make` takes exactly the six OPFS serves and DERIVES `has`, `isEmpty`, `modify`, `modifyUint8Array`, and `forSchema`, so the whole binding is one directory handle acquired once under `Layer.effect`. `makeStringOnly` is the near-miss that base64s every payload — the wrong currency for a store whose leaves are octets — and `layerStorage` is shaped for a synchronous `Storage`, which OPFS is not.
- Boundary: fetching, haul scheduling, and byte budgets are `runtime/browser/fetch`'s and reach this plane as already-verified octets; the persistence GRANT and the `navigator.storage` estimate are that same plane's owned native calls, arriving here as the `Cache.Budget` value `[05]` reads; the Layer satisfying the Tag is the app root's own composition, and the decoded value a band re-warms into lives in an atom, never here.
- Growth: a new band is one `_BANDS` row; a new leaf shape is data the entry already carries; a new store backend is a Layer at the composition root.

```typescript signature
import { KeyValueStore } from "@effect/platform"
import { Digest } from "@rasm/ts/core"
import { Array, DateTime, Effect, Layer, Schema } from "effect"

const _bands = ["glb", "bvh", "frame", "draft", "media"] as const

// one row per band: identity posture, reproducibility, and eviction order — every band decision reads off this table;
// media is speculative prefetch (view/media#SOURCE_PLANE consumes it), so it evicts ahead of every rendered product
const _BANDS = {
  glb: { keyed: true, remintable: true, rank: 2 },
  bvh: { keyed: false, remintable: true, rank: 1 },
  frame: { keyed: true, remintable: true, rank: 3 },
  draft: { keyed: false, remintable: false, rank: 0 },
  media: { keyed: true, remintable: true, rank: 4 },
} as const

const _LEDGER = "ledger"

class Leaf extends Schema.Class<Leaf>("Leaf")({
  name: Schema.NonEmptyString,
  extent: Schema.Int.pipe(Schema.nonNegative()),
  digest: Digest.Key.content,
}) {}

class Entry extends Schema.Class<Entry>("Entry")({
  key: Digest.Key.content,
  band: Schema.Literal(..._bands),
  state: Schema.Literal("pending", "resident"), // two-phase commit: a torn write leaves a reapable pending row
  leaves: Schema.NonEmptyArray(Leaf),
  at: Schema.DateTimeUtc,
}) {
  get extent(): number {
    return Array.reduce(this.leaves, 0, (total, leaf) => total + leaf.extent)
  }
  get address(): (name: string) => string {
    return (name) => `${this.band}/${this.key}/${name}`
  }
}

class Ledger extends Schema.Class<Ledger>("Ledger")({
  entries: Schema.HashMap({ key: Digest.Key.content, value: Entry }),
}) {}

// _Index holds the enumerable roster as a value, because the store contract answers size and never a key listing
const _Index = KeyValueStore.layerSchema(Ledger, "ui/CacheLedger")

// _opfs names the six members OPFS answers, and the whole of what `make` demands: it synthesizes has/isEmpty/modify/
// modifyUint8Array/forSchema from them, so the binding is a projection — never a FileSystem port built to reach a Layer
declare const _opfs: (
  root: FileSystemDirectoryHandle,
) => Pick<KeyValueStore.KeyValueStore, "get" | "getUint8Array" | "set" | "remove" | "clear" | "size">

// HOW the store is built is this module's, WHICH segment it occupies is the root's — the app's own prefix is the one
// parameter, so the keyspace law above holds without a root literal landing here; `_Index` layers over what this yields
const _store = (segment: string): Layer.Layer<KeyValueStore.KeyValueStore> =>
  Layer.effect(
    KeyValueStore.KeyValueStore,
    Effect.map(
      // BOUNDARY ADAPTER: the origin-private directory is a promise-shaped native handle acquired ONCE per composition
      Effect.promise(() => globalThis.navigator.storage.getDirectory()),
      (root) => KeyValueStore.prefix(KeyValueStore.make(_opfs(root)), segment),
    ),
  )
```

## [03]-[INTEGRITY_GATE]

[INTEGRITY_GATE]:
- Law: every read-back is verified before the bytes leave — each leaf re-mints through `Digest.mint("content", octets)` and compares against the digest the entry recorded, so a truncated write, a partial quota eviction, or a corrupted page reads as a typed refusal rather than as content; trusting a stored byte because the key was found is exactly the failure a content-addressed store exists to make impossible.
- Law: `keyed` bands prove identity, not just integrity — where the row says the addressing key IS the content's digest, the single leaf must re-mint to that key, so a `glb` band cannot serve one asset's bytes under another's address; an unkeyed band (an acceleration snapshot filed under the geometry's key) proves each leaf against its own recorded digest alone, because its bytes are derived from the keyed content rather than being it.
- Law: verification failure is self-healing, never fatal — the gate retires the entry and the entry point falls through to the mint, so a corrupted band costs one re-mint and the caller never sees the refusal; the fault reaches a caller only where no mint can reproduce the bytes.
- Law: two legs partition the refusal — `integrity` names damage inside the stored entry, so the gate retires it and re-mints, and `store` names the host refusing an entry that is sound, where retiring would destroy live bytes; `evict` therefore DERIVES off the leg rather than riding a stored column beside it.
- Law: each reason declares the subject it renders — a torn leaf names the slot and the leaf it promised, a key mismatch names both the address and the digest the bytes minted to, and a host refusal names the scope and the cause it carried; one shared free-string detail re-opens the axis `reason` already closed.

```typescript signature
import { Fault } from "@rasm/ts/core"
import { Array, Effect, Schema } from "effect"

// a store refusal reading or amending the index belongs to no band, so the ledger stands as its own scope beside the
// band roster — naming a band there blames an asset that had nothing to do with the failure, and the two integrity
// reasons cannot spell it at all: damage inside an entry always names the entry's own band
const _Band = Schema.Literal(..._bands)
const _Scope = Schema.Literal(..._bands, "ledger")

// one row per reason: the core class the projection reads, the leg that partitions this page's census, and the
// subject that reason alone renders from
const _family = Fault.Class.family(
  ["key-mismatch", "leaf-torn", "quota-refused", "store-refused"] as const,
  {
    "key-mismatch": Fault.Class.row({
      class: "malformed",
      leg: "integrity",
      detail: Schema.Struct({ band: _Band, key: Digest.Key.content, minted: Digest.Key.content }),
      render: ({ band, key, minted }) => `${band} band holds ${minted} under address ${key}`,
    }),
    "leaf-torn": Fault.Class.row({
      class: "conflicted",
      leg: "integrity",
      detail: Schema.Struct({
        band: _Band,
        key: Digest.Key.content,
        slot: Schema.Int.pipe(Schema.nonNegative()),
        leaf: Schema.NonEmptyString,
      }),
      render: ({ band, key, slot, leaf }) => `${band}/${key} slot ${slot} answers no intact ${leaf}`,
    }),
    "quota-refused": Fault.Class.row({
      class: "unavailable",
      leg: "store",
      detail: Schema.Struct({ band: _Scope, cause: Schema.String }),
      render: ({ band, cause }) => `origin quota refused ${band}: ${cause}`,
    }),
    "store-refused": Fault.Class.row({
      class: "unavailable",
      leg: "store",
      detail: Schema.Struct({ band: _Scope, cause: Schema.String }),
      render: ({ band, cause }) => `store refused ${band}: ${cause}`,
    }),
  },
)

declare namespace CacheFault {
  type Reason = (typeof _family.kinds)[number]
  type Issue = typeof _family.payload.Type
}

class CacheFault extends Schema.TaggedError<CacheFault>()("CacheFault", {
  case: _family.payload,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  // DERIVED off the leg that already partitions the census: the integrity leg indicts the stored entry, the store leg
  // indicts the host, and a stored per-row bit beside them would be a second answer to a question the leg settles
  get evict(): boolean {
    return _family.legOf(this.case.reason) === "integrity"
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

const _verified = (entry: Entry, leaves: Cache.Leaves): Effect.Effect<Cache.Leaves, CacheFault> =>
  Effect.gen(function* () {
    // POSITION joins the roster to the bytes and the NAME is that position's evidence: the entry published this order
    // at mint, so a slot answering a different name is a re-keyed construction rather than a lookup to fall back on,
    // and the refusal carries that slot instead of the entry key alone
    const torn = (slot: number, leaf: Leaf) =>
      new CacheFault({ case: { reason: "leaf-torn", band: entry.band, key: entry.key, slot, leaf: leaf.name } })
    yield* Effect.forEach(
      entry.leaves,
      (leaf, at) =>
        Effect.mapError(Array.get(leaves, at), () => torn(at, leaf)).pipe(
          Effect.filterOrFail((held) => held.name === leaf.name, () => torn(at, leaf)),
          Effect.flatMap((held) =>
            Effect.filterOrFail(Digest.mint("content", held.octets), (digest) => digest === leaf.digest, () => torn(at, leaf))),
        ),
      { discard: true },
    )
    // integrity holds for every leaf; a keyed band additionally proves the ADDRESS is the content's own digest, and
    // the refusal names BOTH addresses because a mismatch is only readable as the pair
    const head = Array.headNonEmpty(entry.leaves)
    return yield* _BANDS[entry.band].keyed && head.digest !== entry.key
      ? Effect.fail(new CacheFault({ case: { reason: "key-mismatch", band: entry.band, key: entry.key, minted: head.digest } }))
      : Effect.succeed(leaves)
  })
```

## [04]-[RESIDENCY_ENTRY]

[RESIDENCY_ENTRY]:
- Owner: `Cache.resident(band, key, mint)` — the ONE entry every consumer reaches: it reads the ledger row, verifies every leaf on a hit, and on a miss or a failed verification runs the caller's `mint` and commits. Callers state WHAT the bytes are and never WHERE they live, so residency is a property of the call rather than a protocol every consumer re-implements; a bare read, a bare write, or a has-then-get pair beside this entry re-derives the modality it owns.
- Law: the commit is two-phase and the ledger leads — a `pending` row lands before the first leaf is written and flips to `resident` only after the last one, so a reload interrupted mid-write finds a row it can reap by address rather than leaves nothing enumerates; the reverse order charges quota for bytes no ledger names.
- Law: the mint runs at most once per miss and its bytes and their ORDER are the only things the caller supplies — leaf digests, extents, and the timestamp are minted here from those bytes, so a caller cannot record an identity it did not compute and the `ASSERTED_VALUE` shape has no path in; the sequence is non-empty by type, so an empty mint cannot be spelled rather than dying at a runtime arm.
- Law: the entry is band-polymorphic, never band-switched — every band walks the same read, verify, mint, commit path and the `_BANDS` row supplies the only variation; a `residentGlb`/`residentFrame` pair is the sibling family this entry deletes.
- Law: the caller's own fault channel rides through untouched — `mint` fails on its own rail and this entry adds only `CacheFault`, so a cold-start warm that cannot fetch reports the fetch's reason rather than a cache reason wearing it.
- Boundary: reassembly is the consumer's — `viewer/scene` deserializes a `bvh` bundle's leaves onto its geometry BY POSITION, matching the mesh order its own `GlbViewport.Snapshots` array contract already reads back, and `view/chart` reads a `frame` bundle's IPC leaf; this owner returns ordered named octets and interprets none of them.

```typescript signature
import { HashMap, Option, Predicate } from "effect"

declare namespace Cache {
  type Band = (typeof _bands)[number]
  type Scope = typeof _Scope.Type // what a refusal blames: an asset band, or the index that belongs to none
  // one leaf's bytes beside the name they answer to, in the SEQUENCE POSITION that is its address
  type Octets = { readonly name: string; readonly octets: Uint8Array<ArrayBuffer> }
  // `Leaves` IS the ordered owning value: it publishes the roster once, beside the bytes it names, so every reader
  // walks it by position and no consumer re-derives order from a container's enumeration. Non-empty by type, because
  // a mint yielding no leaf is a caller defect no recovery arm could act on
  type Leaves = Array.NonEmptyReadonlyArray<Cache.Octets>
  type Bundle = { readonly key: Digest.Key<"content">; readonly band: Cache.Band; readonly leaves: Cache.Leaves }
  type Vault = KeyValueStore.KeyValueStore | KeyValueStore.SchemaStore<Ledger, never>
  type _Rows<T extends Record<Cache.Band, { readonly keyed: boolean; readonly remintable: boolean; readonly rank: number }> = typeof _BANDS> = T
}

// every platform refusal re-tags once at this seam, so no member below carries the store's own fault vocabulary; the
// two store reasons split HERE because only one of them a sweep can answer — the origin refusing on quota rejects
// with the platform's own `QuotaExceededError`, and the same write lands once pressure reclaims, while every other
// refusal names a store that answers nothing
const _quota = (cause: unknown): boolean =>
  Predicate.hasProperty(cause, "name") && cause.name === "QuotaExceededError"

const _refused = <A, R>(band: Cache.Scope, self: Effect.Effect<A, unknown, R>): Effect.Effect<A, CacheFault, R> =>
  Effect.mapError(self, (cause) =>
    new CacheFault({
      case: { reason: _quota(cause) ? "quota-refused" : "store-refused", band, cause: String(cause) },
    }))

const _ledger: Effect.Effect<Ledger, CacheFault, Cache.Vault> = Effect.flatMap(_Index.tag, (index) =>
  Effect.map(_refused("ledger", index.get(_LEDGER)), Option.getOrElse(() => new Ledger({ entries: HashMap.empty() }))))

// _amend seeds the row on the first commit and atomically read-modify-writes on every later one, because the store's
// modify is not an upsert; amending through a get-then-set pair would let two concurrent commits clobber each other's row
const _amend = (band: Cache.Band, step: (ledger: Ledger) => Ledger): Effect.Effect<void, CacheFault, Cache.Vault> =>
  Effect.flatMap(_Index.tag, (index) =>
    Effect.flatMap(_refused(band, index.modify(_LEDGER, step)), (amended) =>
      Option.match(amended, {
        onNone: () => _refused(band, index.set(_LEDGER, step(new Ledger({ entries: HashMap.empty() })))),
        onSome: () => Effect.void,
      })))

// BOUNDARY ADAPTER: the store allocates every buffer it answers, so its bytes are non-shared by construction; the
// port's non-shared generic is proven once here rather than re-guarded at every consumer handing .buffer to a decoder
const _octets = (band: Cache.Band, address: string): Effect.Effect<Option.Option<Uint8Array<ArrayBuffer>>, CacheFault, Cache.Vault> =>
  Effect.flatMap(KeyValueStore.KeyValueStore, (store) =>
    Effect.map(_refused(band, store.getUint8Array(address)), (held) =>
      Option.map(held, (octets) => new Uint8Array(octets.buffer, octets.byteOffset, octets.byteLength))))

const _committed = (entry: Entry, leaves: Cache.Leaves): Effect.Effect<void, CacheFault, Cache.Vault> =>
  Effect.gen(function* () {
    const store = yield* KeyValueStore.KeyValueStore
    // _committed lands the pending row FIRST: a write torn here leaves a row the sweep can reap by address, never orphan bytes
    yield* _amend(entry.band, (held) => new Ledger({ entries: HashMap.set(held.entries, entry.key, entry) }))
    // roster and bytes walk in ONE order — the entry's — so the address written and the leaf recorded are the same
    // slot, and no name lookup stands between the two to disagree
    yield* Effect.forEach(
      entry.leaves,
      (leaf, at) =>
        Effect.flatMap(
          Effect.mapError(
            Array.get(leaves, at),
            () => new CacheFault({ case: { reason: "leaf-torn", band: entry.band, key: entry.key, slot: at, leaf: leaf.name } }),
          ),
          (held) => _refused(entry.band, store.set(entry.address(leaf.name), held.octets)),
        ),
      { discard: true },
    )
    const sealed = new Entry({ key: entry.key, band: entry.band, state: "resident", leaves: entry.leaves, at: entry.at })
    yield* _amend(entry.band, (held) => new Ledger({ entries: HashMap.set(held.entries, entry.key, sealed) }))
  })

const _minted = <E, R>(
  band: Cache.Band,
  key: Digest.Key<"content">,
  mint: Effect.Effect<Cache.Leaves, E, R>,
): Effect.Effect<Cache.Bundle, CacheFault | E, Cache.Vault | R> =>
  Effect.gen(function* () {
    const leaves = yield* mint
    const at = yield* DateTime.now
    // identity is minted HERE from the caller's own bytes, IN the caller's own order: a caller cannot record an extent
    // or a digest it never computed, and the sequence it handed over IS the roster this entry publishes downstream
    const rows = yield* Effect.forEach(leaves, (held) =>
      Effect.map(Digest.mint("content", held.octets), (digest) =>
        new Leaf({ name: held.name, extent: held.octets.byteLength, digest })))
    yield* _committed(new Entry({ key, band, state: "pending", leaves: rows, at }), leaves)
    return { key, band, leaves }
  })

// _read walks the ENTRY's roster, so the sequence it yields carries the published order by construction and the
// verify below never reconstructs one; a leaf the ledger promises and the store cannot answer IS the torn state,
// re-spelled at the point of knowledge rather than carried on as an absence a later reader has to interpret
const _read = (entry: Entry): Effect.Effect<Cache.Leaves, CacheFault, Cache.Vault> =>
  Effect.flatMap(
    Effect.forEach(entry.leaves, (leaf, at) =>
      Effect.flatMap(
        _octets(entry.band, entry.address(leaf.name)),
        Option.match({
          onNone: () =>
            Effect.fail(new CacheFault({ case: { reason: "leaf-torn", band: entry.band, key: entry.key, slot: at, leaf: leaf.name } })),
          onSome: (octets): Effect.Effect<Cache.Octets> => Effect.succeed({ name: leaf.name, octets }),
        }),
      )),
    (leaves) => _verified(entry, leaves),
  )

const _resident = <E, R>(
  band: Cache.Band,
  key: Digest.Key<"content">,
  mint: Effect.Effect<Cache.Leaves, E, R>,
): Effect.Effect<Cache.Bundle, CacheFault | E, Cache.Vault | R> =>
  Effect.flatMap(_ledger, (held) =>
    Option.match(
      Option.filter(HashMap.get(held.entries, key), (entry) => entry.state === "resident" && entry.band === band),
      {
        onNone: () => _minted(band, key, mint),
        // a failed verification retires and re-mints: a corrupted band costs one mint and never reaches the caller
        onSome: (entry) =>
          Effect.catchIf(
            Effect.map(_read(entry), (leaves): Cache.Bundle => ({ key, band, leaves })),
            (fault) => fault.evict,
            () => Effect.andThen(_retire(key), _minted(band, key, mint)),
          ),
      },
    ))
```

## [05]-[QUOTA_SWEEP]

[QUOTA_SWEEP]:
- Owner: `Cache.sweep(budget)` — the reclaim fold: the pressure row for the budget's verdict names the fraction of measured usage to free, the victim order ranks every remintable entry, and the fold takes victims until the target is met, answering one `Cache.Reclaim` — the bytes asked for, the bytes its own fold measured, and the keys it retired. `Cache.retire(key)` is the same removal for one key — a revoked asset, an explicit purge — and both drop leaves before the ledger row so no row can outlive its bytes.
- Law: the budget is MEASURED and supplied, never probed here — `runtime/browser/persist` owns `navigator.storage` and its estimate, and its verdict vocabulary (`ample`, `tight`, `critical`, `opaque`) is spelled here field-for-field for the pressure key alone; a `navigator.storage` read in this module is the ungated-native-call defect, a local remap of those four words is the cross-plane fork, and `opaque` — the host withholding numbers — sweeps as if tight rather than as if empty, because an unmeasurable origin can evict the whole store out from under the ledger.
- Law: the victim order is band rank then age, and non-remintable bands are not candidates at all — a `draft` spill is filtered out before ordering rather than ranked last, so no pressure level can reach it; a rank comparison that reaches it runs under `critical`.
- Law: the sweep takes whole entries — a partially reclaimed entry leaves a ledger row promising leaves that no longer exist, which is exactly the `leaf-torn` state the gate exists to catch; the fold therefore overshoots its target rather than splitting a victim.
- Law: reclaim states its own arithmetic — `freed` is the victim fold's measured sum against the `target` the pressure row named, so a sweep that exhausted every remintable band and still fell short is distinguishable from one that met its target instead of reading as the same key list; the next budget the composing root supplies is still the measurement of the store, and a running total carried across sweeps is a tally no producer took.

```typescript signature
import { Order } from "effect"

declare namespace Cache {
  type Verdict = keyof typeof _PRESSURE
  type Budget = { readonly usage: number; readonly quota: number; readonly headroom: number; readonly verdict: Cache.Verdict }
  // the sweep's own tally: what the pressure row asked for, what the victim fold actually took, and the keys it
  // retired — the shortfall is the subtraction a caller can now perform, never a claim this owner asserts
  type Reclaim = {
    readonly target: number
    readonly freed: number
    readonly retired: ReadonlyArray<Digest.Key<"content">>
  }
  type Shape = {
    readonly bands: typeof _bands
    readonly rows: typeof _BANDS
    readonly Ledger: typeof Ledger
    readonly Index: typeof _Index
    readonly store: typeof _store
    readonly resident: typeof _resident
    readonly retire: typeof _retire
    readonly sweep: typeof _sweep
  }
}

// _PRESSURE states the fraction of measured usage a sweep reclaims; an opaque origin sweeps as if tight because it may evict everything
const _PRESSURE = { ample: 0, tight: 0.25, critical: 0.6, opaque: 0.25 } as const

const _victim: Order.Order<Entry> = Order.combine(
  Order.mapInput(Order.reverse(Order.number), (entry: Entry) => _BANDS[entry.band].rank),
  Order.mapInput(DateTime.Order, (entry: Entry) => entry.at),
)

const _retire = (key: Digest.Key<"content">): Effect.Effect<void, CacheFault, Cache.Vault> =>
  Effect.gen(function* () {
    const store = yield* KeyValueStore.KeyValueStore
    const index = yield* _Index.tag
    const held = yield* _ledger
    yield* Option.match(HashMap.get(held.entries, key), {
      onNone: () => Effect.void,
      onSome: (entry) =>
        // leaves first: a row outliving its bytes is the torn state the gate exists to catch
        Effect.andThen(
          Effect.forEach(entry.leaves, (leaf) => _refused(entry.band, store.remove(entry.address(leaf.name))), { discard: true }),
          _refused(entry.band, index.set(_LEDGER, new Ledger({ entries: HashMap.remove(held.entries, key) }))),
        ),
    })
  })

const _target = (budget: Cache.Budget): number => budget.usage * _PRESSURE[budget.verdict]

const _sweep = (budget: Cache.Budget): Effect.Effect<Cache.Reclaim, CacheFault, Cache.Vault> =>
  Effect.flatMap(_ledger, (held) => {
    const target = _target(budget)
    // ONE fold both chooses and measures, so the receipt below carries the sum this walk already took rather than a
    // second walk over the victims it chose — a shortfall is the fold's own arithmetic, never a re-derived claim
    const taken = Array.reduce(
      // a non-remintable band is filtered OUT before ordering, so no pressure level can reach an upload spill
      Array.sort(Array.filter(HashMap.values(held.entries), (entry) => _BANDS[entry.band].remintable), _victim),
      { freed: 0, victims: Array.empty<Entry>() },
      // whole entries only: the fold overshoots its target rather than splitting one and stranding its ledger row
      (state, entry) =>
        state.freed >= target
          ? state
          : { freed: state.freed + entry.extent, victims: Array.append(state.victims, entry) },
    )
    return Effect.map(
      Effect.forEach(taken.victims, (entry) => Effect.as(_retire(entry.key), entry.key)),
      (retired): Cache.Reclaim => ({ target, freed: taken.freed, retired }),
    )
  })

const Cache: Cache.Shape = {
  bands: _bands,
  rows: _BANDS,
  Ledger,
  Index: _Index,
  store: _store,
  resident: _resident,
  retire: _retire,
  sweep: _sweep,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Cache, CacheFault }
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
