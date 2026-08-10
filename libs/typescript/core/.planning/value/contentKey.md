# [CORE_CONTENTKEY]

`Digest` is the sole content-digest owner and the branch's only `hash-wasm` import site. Algorithm rows derive branded keys and both wire codecs; one polymorphic mint and one sealed incremental-session algebra consume isolated machines. Module: `core/src/value/contentKey.ts`.

## [01]-[DIGEST_OWNER]

- The ordered algorithm vocabulary carries factory, width, brand, and wire case; `Digest.Key<K>` and `Digest.codecs[K]` derive from those rows.
- `content` is seed-zero XXH128, `trace` seed-zero XXH64, `check` CRC32, and `proof` BLAKE3-256; text crosses only after explicit encoding.
- `Digest.Session` seals every detached checkpoint behind `Redacted`; each operation loads one snapshot atomically into a fresh machine.
- Keyed authentication and KDFs belong to the security owner; no keyed key material or unbranded raw digest leaves this module.

```typescript signature
import { Effect, Either, Encoding, ParseResult, Predicate, Record, Redacted, Schema } from "effect"
import { createBLAKE3, createCRC32, createXXHash64, createXXHash128, type IHasher } from "hash-wasm"
import { Shape } from "./schema.ts"

const _hex = (width: number, alphabet: "lower" | "upper" = "lower"): RegExp =>
  new RegExp(`^[0-9${alphabet === "lower" ? "a-f" : "A-F"}]{${width}}$`)

const _key = <const Brand extends string>(brand: Brand, width: number) =>
  Schema.String.pipe(Schema.pattern(_hex(width)), Schema.brand(brand))

const _kinds = ["check", "content", "proof", "trace"] as const
const _rows = {
  check: { bytes: 4, key: _key("Checksum", 8), make: () => createCRC32(), wire: "lower" },
  content: { bytes: 16, key: _key("ContentKey", 32), make: () => createXXHash128(0, 0), wire: "upper" },
  proof: { bytes: 32, key: _key("ProofKey", 64), make: () => createBLAKE3(256), wire: "lower" },
  trace: { bytes: 8, key: _key("TraceKey", 16), make: () => createXXHash64(0, 0), wire: "lower" },
} as const
type _AlgorithmRows = {
  readonly [Kind in (typeof _kinds)[number]]: Omit<(typeof _rows)[Kind], "make">
}
const _algorithmRows = Record.map(_rows, ({ bytes, key, wire }) => ({ bytes, key, wire })) as unknown as _AlgorithmRows
const _algorithms = Shape.vocabulary(_kinds, _algorithmRows)

const _codec = <Key extends Schema.Schema.Any>(key: Key, bytes: number, wire: "lower" | "upper") => {
  const Bytes = Schema.Uint8ArrayFromSelf.pipe(Schema.filter((value) => value.length === bytes))
  const Wire = Schema.String.pipe(Schema.pattern(_hex(bytes * 2, wire)))
  return {
    bytes: Schema.transformOrFail(Bytes, key, {
      strict: true,
      decode: (value) => ParseResult.succeed(Encoding.encodeHex(value)),
      encode: (value, _options, ast) =>
        Either.match(Encoding.decodeHex(value), {
          onLeft: () => ParseResult.fail(new ParseResult.Type(ast, value, "<malformed-digest>")),
          onRight: ParseResult.succeed,
        }),
    }),
    wire: Schema.transform(Wire, key, {
      strict: true,
      decode: (value) => value.toLowerCase(),
      encode: (value) => wire === "upper" ? value.toUpperCase() : value,
    }),
  } as const
}

type _Keys = { readonly [Kind in (typeof _kinds)[number]]: _AlgorithmRows[Kind]["key"] }
type _Codecs = {
  readonly [Kind in (typeof _kinds)[number]]: ReturnType<typeof _codec<_AlgorithmRows[Kind]["key"]>>
}
const _keys = Record.map(_algorithmRows, (row) => row.key) as unknown as _Keys
const _codecs = Record.map(
  _algorithmRows,
  (row) => _codec(row.key, row.bytes, row.wire),
) as unknown as _Codecs

const _minted = <Kind extends Digest.Kind>(kind: Kind, hex: string): Effect.Effect<Digest.Key<Kind>> =>
  Effect.orDie(Schema.decode(_algorithms.at(kind).key)(hex))

const _hasher = <Kind extends Digest.Kind>(kind: Kind): Effect.Effect<IHasher> =>
  Effect.promise(() => _rows[kind].make())

const _walk = (hasher: IHasher, payload: Digest.Payload): string => {
  // BOUNDARY ADAPTER: IHasher is statement-shaped mutable state; only its detached digest leaves this atomic walk.
  const armed = hasher.init()
  if (Predicate.isUint8Array(payload)) armed.update(payload)
  else for (const chunk of payload) armed.update(chunk)
  return armed.digest()
}

class _Session<Kind extends Digest.Kind = Digest.Kind> {
  readonly kind: Kind
  readonly #state: Redacted.Redacted<Uint8Array>

  private constructor(kind: Kind, state: Uint8Array) {
    this.kind = kind
    this.#state = Redacted.make(state.slice())
    Object.freeze(this)
  }

  static make = <Kind extends Digest.Kind>(kind: Kind, state: Uint8Array): _Session<Kind> => new _Session(kind, state)
  static load = <Kind extends Digest.Kind>(session: _Session<Kind>, hasher: IHasher): IHasher =>
    hasher.load(Redacted.value(session.#state).slice())

  checkpoint(): Redacted.Redacted<Uint8Array> {
    return Redacted.make(Redacted.value(this.#state).slice())
  }
}

function _open<Kind extends Digest.Kind>(kind: Kind): Effect.Effect<_Session<Kind>>
function _open<Kind extends Digest.Kind>(kind: Kind, checkpoint: Redacted.Redacted<Uint8Array>): Effect.Effect<_Session<Kind>>
function _open<Kind extends Digest.Kind>(
  kind: Kind,
  checkpoint?: Redacted.Redacted<Uint8Array>,
): Effect.Effect<_Session<Kind>> {
  return checkpoint === undefined
    ? Effect.map(_hasher(kind), (hasher) => _Session.make(kind, hasher.init().save()))
    : Effect.map(_hasher(kind), (hasher) => _Session.make(kind, hasher.load(Redacted.value(checkpoint).slice()).save()))
}

const Digest = {
  ..._algorithms,
  Key: _keys,
  codecs: _codecs,
  mint: <Kind extends Digest.Kind>(kind: Kind, payload: Digest.Payload): Effect.Effect<Digest.Key<Kind>> =>
    Effect.flatMap(_hasher(kind), (hasher) => _minted(kind, _walk(hasher, payload))),
  Session: {
    open: _open,
    absorb: <Kind extends Digest.Kind>(session: Digest.Session<Kind>, chunk: Uint8Array): Effect.Effect<Digest.Session<Kind>> =>
      Effect.map(_hasher(session.kind), (hasher) =>
        _Session.make(session.kind, _Session.load(session, hasher).update(chunk).save())),
    checkpoint: <Kind extends Digest.Kind>(session: Digest.Session<Kind>): Redacted.Redacted<Uint8Array> => session.checkpoint(),
    finish: <Kind extends Digest.Kind>(session: Digest.Session<Kind>): Effect.Effect<Digest.Key<Kind>> =>
      Effect.flatMap(_hasher(session.kind), (hasher) => _minted(session.kind, _Session.load(session, hasher).digest())),
  },
} as const

declare namespace Digest {
  type Kind = (typeof _kinds)[number]
  type Key<Kind extends Digest.Kind = Digest.Kind> = Schema.Schema.Type<(typeof _rows)[Kind]["key"]>
  type Payload = Uint8Array | Iterable<Uint8Array>
  type Session<Kind extends Digest.Kind = Digest.Kind> = _Session<Kind>
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Digest }
```

## [02]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
