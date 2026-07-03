# [KERNEL_CONTENTKEY]

The one content identity of the branch: `ContentKey` is the `XxHash128` seed-zero digest branded at the canonical `:x32` spelling — 32 lowercase hex characters in the big-endian layout the C# `System.IO.Hashing.XxHash128` seed-0 mint persists — and `contentKey` is the ONE mint (invariant 2, `[R2]`, seams `K:47`/`CO:110`). Exactly three sites delegate to it — `wire/frame` reassembly, the `browser/transport` decode worker, and `store/object` `ObjectKey` — and a second mint, a second content-address notion, or a non-zero seed is the named cross-language drift defect. The hasher's hex is already that canonical spelling — `hash-wasm` and the C# mint render the digest in the same big-endian order, proven by the frozen corpus vector both mints hash to — so no byte-order step exists anywhere on the key path, and bit-parity is asserted against the frozen `CANONICAL_BYTE_IDENTITY` and `MATERIAL_LAYER_GOLDEN` corpora by the `tests/contracts` drivers. The module is `kernel/src/identity/contentkey.ts` — the only cataloguing and import site of `hash-wasm` in the branch.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                       | [PUBLIC]     |
| :-----: | :----------- | :------------------------------------------------------------ | :----------- |
|  [01]   | `KEY_BRAND`  | the `:x32` branded schema and its one-name type               | `ContentKey` |
|  [02]   | `SEED_ZERO_MINT` | the memoized WASM hasher and the modality-polymorphic mint | `contentKey` |

## [2]-[KEY_BRAND]

[KEY_BRAND]:
- Owner: `ContentKey`, a same-name schema-plus-type pair — the const is the branded schema every field record composes (`Schema.Struct({ key: ContentKey })`), the type is the branded string every signature speaks, and one import serves both planes.
- Law: the `:x32` spelling is the whole refinement — exactly 32 lowercase hex characters, the big-endian hex of the 16-byte digest — so an admitted key is wire-canonical by construction and equality is bare `===` on branded strings with zero comparator ceremony.
- Law: an inbound key on a C# wire shape decodes through this schema at the wire's field record; a key minted locally arrives through `contentKey` — both paths land the identical brand, so verification at a delegate is mint-then-compare with no normalize step.
- Growth: a binary-carried key (16 raw big-endian bytes on a frame) lands as one `Schema.transformOrFail` row at this owner when a wire shape demands it — `Encoding.encodeHex` on decode, `Encoding.decodeHex` on encode — never a delegate-side hex conversion.
- Boundary: `store/object` aliases `ObjectKey = ContentKey` as a type alias over this brand, never a re-brand; a second brand over the same digest would fork container identity.
- Packages: `effect` (`Schema`).

## [3]-[SEED_ZERO_MINT]

[SEED_ZERO_MINT]:
- Owner: `contentKey`, the modality-polymorphic mint — one annotated arrow whose input discriminates on the value shape: a whole `Uint8Array` payload or an `Iterable<Uint8Array>` chunk sequence, both landing on one digest path; a `mode` flag, a `mintMany` twin, or a string input (encoding ambiguity) is the rejected surface, and text hashes only after the caller's own explicit encode to bytes.
- Law: the seed is zero on both halves — `createXXHash128(0, 0)` — and the factory promise is memoized through `GlobalValue.globalValue` under the module's scoped key, so the WASM compile happens once per runtime across bundler-duplicated module instances and `init()` resets state between mints without recompiling.
- Law: one digest path serves both modalities — `init` → `update` per chunk → `digest()` — and the returned hex is already the canonical big-endian `:x32` the C# mint persists; a byte-order shuffle anywhere on this path mints the memory-dump spelling and is the named endianness defect, and `digest("binary")` bytes are display-ordered — the reverse of the C# little-endian destination-buffer dump — so raw-buffer parity reverses one side and the hex path never does.
- Law: the hash section is synchronous and JS-thread-atomic — every await sits before `init`, so concurrent mints on the shared hasher cannot interleave and the shared state machine needs no lock; a future streaming modality (chunks arriving over time) must construct a private hasher per mint, never span an await across the shared one.
- Law: the mint cannot fail — `Effect.promise` carries the compile (rejection is a defect), and the brand decode is proven by construction (the hasher's hex is always `:x32`), so the surface is `Effect<ContentKey>` and `Effect.orDie` states that a decode fault here is a defect, never a channel member.
- Exemption: `_digested` is a marked `// BOUNDARY ADAPTER` kernel — the `IHasher` state machine forces statements across the chunk walk, and only the immutable hex string leaves; the implementer carries the mark on its first line.
- Growth: a keyed digest, a checksum family, or a second algorithm is a new row on the `hash-wasm` pattern catalogued at `kernel/.api/hash-wasm.md` — one new interior factory and one new owner, never a seed knob on this mint.
- Boundary: delegates import `contentKey` and compare; they never import `hash-wasm`, never re-hash for parity claims (byte-level corpus equality only), and their mismatch faults are their own folder rails.
- Packages: `hash-wasm` (`createXXHash128`, `IHasher`); `effect` (`Effect`, `GlobalValue`, `Predicate`, `Schema`).

```typescript
import { Effect, GlobalValue, Predicate, Schema } from "effect"
import { createXXHash128, type IHasher } from "hash-wasm"

const _SHAPE = /^[0-9a-f]{32}$/

const _hex = Schema.String.pipe(Schema.pattern(_SHAPE))
const ContentKey: Schema.brand<typeof _hex, "ContentKey"> = _hex.pipe(Schema.brand("ContentKey"))
type ContentKey = typeof ContentKey.Type

const _minted = Schema.decode(ContentKey)
const _compiled = GlobalValue.globalValue("@rasm/ts/kernel/ContentKey", () => createXXHash128(0, 0))

const _digested = (hasher: IHasher, chunks: Iterable<Uint8Array>): string => {
  const armed = hasher.init()
  for (const chunk of chunks) armed.update(chunk)
  return armed.digest()
}

const contentKey = (payload: Uint8Array | Iterable<Uint8Array>): Effect.Effect<ContentKey> =>
  Effect.orDie(
    Effect.flatMap(Effect.promise(() => _compiled), (hasher) =>
      _minted(_digested(hasher, Predicate.isUint8Array(payload) ? [payload] : payload)),
    ),
  )

// --- [EXPORTS] --------------------------------------------------------------------------

export { ContentKey, contentKey }
```
