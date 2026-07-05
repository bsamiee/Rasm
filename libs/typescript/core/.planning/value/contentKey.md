# [CORE_CONTENTKEY]

The one content identity of the branch and the digest engine beneath it: `ContentKey` is the `XxHash128` seed-zero digest branded at the canonical `:x32` spelling — 32 lowercase hex characters in the big-endian layout the C# `System.IO.Hashing.XxHash128` seed-0 mint persists — and `Digest` is the ONE hasher surface, a width-parameterized row table whose `content` row is that mint, whose sibling rows carry the short trace address and the wire checksum, and whose session and keyed modalities ride the same compiled state machines. Exactly three sites delegate to the content mint — `interchange/frame` reassembly, the runtime browser fetch worker, and the data object store — and a second mint, a second content-address notion, or a non-zero seed is the named cross-language drift defect. The hasher's hex is already the canonical spelling — `hash-wasm` and the C# mint render the digest in the same big-endian order, proven by the frozen corpus vector both mints hash to — so no byte-order step exists anywhere on the key path, and bit-parity is asserted against the frozen `CANONICAL_BYTE_IDENTITY` and `MATERIAL_LAYER_GOLDEN` corpora by the `tests/contracts` drivers. The module is `core/src/value/contentKey.ts` — the only cataloguing and import site of `hash-wasm` in the branch.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]           | [OWNS]                                                             | [PUBLIC]     |
| :-----: | :------------------ | :------------------------------------------------------------------ | :----------- |
|  [01]   | `KEY_BRAND`         | the `:x32` branded schema and its sixteen-byte binary twin          | `ContentKey` |
|  [02]   | `DIGEST_TABLE`      | the width-row vocabulary, memoized compiles, the polymorphic mint   | `Digest`     |
|  [03]   | `RESUMABLE_SESSION` | the save/load checkpoint algebra over one shared compiled hasher    | `Digest`     |
|  [04]   | `SEALED_DIGEST`     | the keyed authentication mint and its `Seal` brand                  | `Digest`     |

## [2]-[KEY_BRAND]

[KEY_BRAND]:
- Owner: `ContentKey`, a same-name schema-plus-type pair — the const is the branded schema every field record composes (`Schema.Struct({ key: ContentKey })`), the type is the branded string every signature speaks, and one import serves both planes.
- Law: the `:x32` spelling is the whole refinement — exactly 32 lowercase hex characters, the big-endian hex of the 16-byte digest — so an admitted key is wire-canonical by construction and equality is bare `===` on branded strings with zero comparator ceremony.
- Law: an inbound key on a C# wire shape decodes through this schema at the wire's field record; a key minted locally arrives through `Digest.mint("content", ...)` — both paths land the identical brand, so verification at a delegate is mint-then-compare with no normalize step.
- Law: `Digest.FromBytes` is the binary twin — 16 raw bytes carried on a frame decode through `Encoding.encodeHex` into the branded hex and encode back through `Encoding.decodeHex` — one declaration owns the byte-carried key, and a delegate-side hex conversion is unspellable; `digest("binary")` bytes and the frame bytes share this display order, the reverse of the C# little-endian destination-buffer dump, so raw-buffer parity reverses one side and the hex path never does.
- Boundary: the data object store aliases its object key as a type alias over this brand, never a re-brand; a second brand over the same digest forks container identity.
- Packages: `effect` (`Schema`, `Encoding`, `ParseResult`).

## [3]-[DIGEST_TABLE]

[DIGEST_TABLE]:
- Owner: `Digest`, the assembled hasher vocabulary — the interior row table carries one factory per width row, the interior brand anchors carry one key schema per row, and the exported owner assembles the binary twin, the mint, the session algebra, and the keyed mint under a stated annotation; the roster is seed data on one parameterized pattern, so a new digest width is a row plus its brand anchor, never a new surface.
- Law: three rows ride the table — `content` (`createXXHash128(0, 0)`, 32 hex, the cross-language ContentKey), `trace` (`createXXHash64(0, 0)`, 16 hex, the short correlation address log and sampling keys carry), `check` (`createCRC32()`, 8 hex, the wire checksum frame rails verify) — and the seed is zero on every seeded row; a non-zero seed on any content-address path is out of contract.
- Law: each row's factory promise is memoized through `GlobalValue.globalValue` under a row-keyed scope, so the WASM compile happens once per runtime per row across bundler-duplicated module instances, `init()` resets state between mints without recompiling, and an untouched row never compiles.
- Law: `mint` is modality-polymorphic — one annotated arrow whose payload discriminates on the value shape: a whole `Uint8Array` or an `Iterable<Uint8Array>` chunk sequence, both landing on one digest walk; a `mode` flag, a `mintMany` twin, or a string input (encoding ambiguity) is the rejected surface, and text hashes only after the caller's own explicit encode to bytes. The iterable modality IS the streaming verify — a multi-band reassembly proves its declared key over held bands with zero joined re-hash, and the interchange `Parity` combinator delegates exactly this walk, so no streaming-verify sibling exists anywhere.
- Law: the mint cannot fail — `Effect.promise` carries the compile (rejection is a defect), the returned hex is proven by construction against the row's brand, and the per-row decode record is the mapped handler contract that keeps the generic indexed dispatch cast-free; `Effect.orDie` states that a decode fault here is a defect, never a channel member.
- Law: the digest walk is synchronous and JS-thread-atomic — every await sits before `init`, so concurrent mints on a shared hasher cannot interleave and the shared state machine needs no lock.
- Exemption: `_walk` is a marked kernel — the `IHasher` state machine forces the statement loop across the chunk walk, only the immutable hex string leaves, and the implementer carries the `// BOUNDARY ADAPTER` mark on its first line.
- Growth: a wider or keyed-content row (`createBLAKE3` variable `bits`, `createXXHash3`) is one table row plus one brand anchor; a KDF surface (`argon2id`, `bcrypt`, `scrypt`) stays out of this floor — secret derivation is the security branch's concern.
- Boundary: delegates import `Digest` and compare; they never import `hash-wasm`, never re-hash for parity claims (byte-level corpus equality only), and their mismatch faults are their own folder rails.
- Packages: `hash-wasm` (`createXXHash128`, `createXXHash64`, `createCRC32`, `createBLAKE3`, `IHasher`); `effect` (`Effect`, `GlobalValue`, `Predicate`, `Redacted`, `Schema`).

## [4]-[RESUMABLE_SESSION]

[RESUMABLE_SESSION]:
- Owner: the session algebra on `Digest` — `session(kind, saved?)` opens a fresh checkpoint or resumes a saved one, `absorb(session, chunk)` advances it, `finish(session)` seals it into the row's branded key — and a session is an immutable value `{ kind, state }` whose `state` is the `IHasher.save()` snapshot, so chunks arriving over time, interleaved mints, and cross-await streaming all share the one compiled hasher instead of constructing a private machine per mint.
- Law: every step is a synchronous `load -> update -> save` atom over the shared hasher — the mutable state machine is entered and exited inside one expression, no hasher reference lives across an await, and two interleaved sessions cannot corrupt each other because each step rehydrates its own snapshot.
- Law: the saved state is as sensitive as the input — a session over secret bytes is held and transported under the caller's secrecy discipline, and a snapshot resumes only under the same `hash-wasm` build; a persisted snapshot crossing a build boundary is a defect the caller owns.
- Law: resume discriminates on arity — `session(kind)` compiles-and-initializes, `session(kind, saved)` wraps the snapshot with zero hasher work — and both land the same session shape on the same rail.
- Growth: a windowed rolling digest is one consumer fold over `absorb`/`finish`; no session variant lands here.
- Boundary: which stream a session folds over, its chunk sizing, and its backpressure are the consuming rail's geometry; this owner fixes the checkpoint algebra only.

## [5]-[SEALED_DIGEST]

[SEALED_DIGEST]:
- Owner: `Digest.mac`, the keyed authentication mint — `createBLAKE3(256, key)` over the sealed 32-byte key, walking the same payload modalities as `mint` and landing the 64-hex `Seal` brand — the wire-auth digest a frame or capability descriptor carries beside its content key.
- Law: the key arrives `Redacted<Uint8Array>` and unwraps exactly once inside the mint — the one consuming boundary — so a raw key never occupies a signature, a log, or a session snapshot; a malformed key length is a defect the compile rejection escalates, never a channel member.
- Law: `Seal` is its own brand — a keyed digest branded `ContentKey` forges content identity, so the two brands never unify and a MAC can never occupy an identity slot.
- Law: the keyed hasher is private per call — the key parameterizes the compile, so no keyed state machine is shared or memoized, and the unkeyed rows' global memo is never keyed material.
- Growth: an HMAC row over a digest factory (`createHMAC(createSHA256(), key)`) lands as one sibling arm when a peer contract demands an HMAC construction over a specific hash; the blake3 keyed mode stays the default seal.
- Boundary: which surfaces carry seals, key custody, and rotation are `security` concerns; this owner mints and brands only.

```typescript
import { Effect, Either, Encoding, GlobalValue, ParseResult, Predicate, Redacted, Schema } from "effect"
import { createBLAKE3, createCRC32, createXXHash64, createXXHash128, type IHasher } from "hash-wasm"

const _hex = (width: number): RegExp => new RegExp(`^[0-9a-f]{${width}}$`)

const _content = Schema.String.pipe(Schema.pattern(_hex(32)))
const ContentKey: Schema.brand<typeof _content, "ContentKey"> = _content.pipe(Schema.brand("ContentKey"))
type ContentKey = typeof ContentKey.Type

const _Trace = Schema.String.pipe(Schema.pattern(_hex(16)), Schema.brand("TraceKey"))
const _Check = Schema.String.pipe(Schema.pattern(_hex(8)), Schema.brand("Checksum"))
const _Seal = Schema.String.pipe(Schema.pattern(_hex(64)), Schema.brand("Seal"))
const _Bytes = Schema.Uint8ArrayFromSelf.pipe(Schema.filter((bytes) => bytes.length === 16))

const _rows = {
  check: { make: () => createCRC32() },
  content: { make: () => createXXHash128(0, 0) },
  trace: { make: () => createXXHash64(0, 0) },
} as const

declare namespace Digest {
  type Kind = keyof typeof _rows
  type Key<K extends Kind = Kind> = {
    readonly check: typeof _Check.Type
    readonly content: ContentKey
    readonly trace: typeof _Trace.Type
  }[K]
  type Payload = Uint8Array | Iterable<Uint8Array>
  type Seal = typeof _Seal.Type
  type Session<K extends Kind = Kind> = { readonly kind: K; readonly state: Uint8Array }
  type Shape = {
    readonly FromBytes: Schema.transformOrFail<typeof _Bytes, typeof ContentKey>
    readonly Seal: typeof _Seal
    readonly absorb: <K extends Kind>(session: Session<K>, chunk: Uint8Array) => Effect.Effect<Session<K>>
    readonly finish: <K extends Kind>(session: Session<K>) => Effect.Effect<Key<K>>
    readonly mac: (key: Redacted.Redacted<Uint8Array>, payload: Payload) => Effect.Effect<Seal>
    readonly mint: <K extends Kind>(kind: K, payload: Payload) => Effect.Effect<Key<K>>
    readonly session: <K extends Kind>(kind: K, saved?: Uint8Array) => Effect.Effect<Session<K>>
  }
  type _Rows<T extends Record<Kind, { readonly make: () => Promise<IHasher> }> = typeof _rows> = T
}

const _minted: { readonly [K in Digest.Kind]: (hex: string) => Effect.Effect<Digest.Key<K>> } = {
  check: (hex) => Effect.orDie(Schema.decode(_Check)(hex)),
  content: (hex) => Effect.orDie(Schema.decode(ContentKey)(hex)),
  trace: (hex) => Effect.orDie(Schema.decode(_Trace)(hex)),
}

const _compiled = <K extends Digest.Kind>(kind: K): Effect.Effect<IHasher> =>
  Effect.promise(() => GlobalValue.globalValue(`@rasm/ts/core/Digest/${kind}`, () => _rows[kind].make()))

const _walk = (hasher: IHasher, payload: Digest.Payload): string => {
  const armed = hasher.init()
  if (Predicate.isUint8Array(payload)) armed.update(payload)
  else for (const chunk of payload) armed.update(chunk)
  return armed.digest()
}

const _FromBytes: Schema.transformOrFail<typeof _Bytes, typeof ContentKey> = Schema.transformOrFail(_Bytes, ContentKey, {
  strict: true,
  decode: (bytes) => ParseResult.succeed(Encoding.encodeHex(bytes)),
  encode: (hex, _options, ast) =>
    Either.match(Encoding.decodeHex(hex), {
      onLeft: () => ParseResult.fail(new ParseResult.Type(ast, hex, "<malformed-hex>")),
      onRight: (bytes) => ParseResult.succeed(bytes),
    }),
})

const Digest: Digest.Shape = {
  FromBytes: _FromBytes,
  Seal: _Seal,
  absorb: (session, chunk) =>
    Effect.map(_compiled(session.kind), (hasher) => ({
      kind: session.kind,
      state: hasher.load(session.state).update(chunk).save(),
    })),
  finish: (session) =>
    Effect.flatMap(_compiled(session.kind), (hasher) => _minted[session.kind](hasher.load(session.state).digest())),
  mac: (key, payload) =>
    Effect.flatMap(Effect.promise(() => createBLAKE3(256, Redacted.value(key))), (hasher) =>
      Effect.orDie(Schema.decode(_Seal)(_walk(hasher, payload)))),
  mint: (kind, payload) => Effect.flatMap(_compiled(kind), (hasher) => _minted[kind](_walk(hasher, payload))),
  session: (kind, saved) =>
    saved === undefined
      ? Effect.map(_compiled(kind), (hasher) => ({ kind, state: hasher.init().save() }))
      : Effect.succeed({ kind, state: saved }),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ContentKey, Digest }
```
