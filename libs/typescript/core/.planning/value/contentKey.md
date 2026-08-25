# [CORE_CONTENTKEY]

`Digest` is the sole semantic-digest owner and the branch's only `hash-wasm` import site. `ArtifactId` owns the protocol-fixed SHA-256 identity of raw artifact octets without admitting SHA-256 to the semantic algorithm vocabulary. `CanonicalWriter` is the branch's one framed semantic-field stream; algorithm rows derive branded keys and both wire codecs, while one polymorphic mint and one sealed incremental-session algebra consume isolated machines. Module: `core/src/value/contentKey.ts`.

## [01]-[DIGEST_OWNER]

- The ordered algorithm vocabulary carries factory, width, brand, and wire case; `Digest.Key<K>` and `Digest.codecs[K]` derive from those rows.
- `content` is seed-zero XXH128, `trace` seed-zero XXH64, `check` CRC32, and `proof` BLAKE3-256; text crosses only after explicit encoding.
- `ArtifactId` is a separate, closed SHA-256 owner: its preimage is the ordered raw artifact octets, its protobuf form is exactly 32 bytes, and its landed key is branded lower-hex. It never enters `Digest.Kind` or its row table.
- `Digest.Session` seals every detached checkpoint behind `Redacted`; each operation loads one snapshot atomically into a fresh machine.
- Keyed authentication and KDFs belong to the security owner; no keyed key material or unbranded raw digest leaves this module.
- `CanonicalWriter` emits fixed-width integers little-endian, UTF-8 strings and arbitrary octets behind an int32 byte-length frame, and repeated semantic rows behind an int32 count; callers state field order and never frame.
- `raw` is reserved for fixed-width or already-delimited bytes. A variable-width generated `bytes` field uses `bytes`, and a collection uses `rows`, so neither can shift an adjacent field boundary.

```typescript
import { Effect, Either, Encoding, ParseResult, Predicate, Record, Redacted, Schema } from "effect"
import { createBLAKE3, createCRC32, createSHA256, createXXHash64, createXXHash128, type IHasher } from "hash-wasm"
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

const _artifactKey = _key("ArtifactSha256", 64)
const _artifactCodec = _codec(_artifactKey, 32, "lower")

const _text = new TextEncoder()

class CanonicalWriter {
  readonly #chunks: Array<Uint8Array> = []

  private static i32(value: number): number {
    if (!Number.isSafeInteger(value) || value < -0x8000_0000 || value > 0x7fff_ffff) {
      throw new RangeError(`canonical int32 out of range: ${value}`)
    }
    return value
  }

  private static signed64(value: bigint): bigint {
    if (value < -(1n << 63n) || value >= 1n << 63n) throw new RangeError(`canonical int64 out of range: ${value}`)
    return value
  }

  private emit(value: Uint8Array): this {
    this.#chunks.push(value.slice())
    return this
  }

  bool(value: boolean): this {
    return this.emit(Uint8Array.of(value ? 1 : 0))
  }

  ordinal(value: number): this {
    const word = new Uint8Array(4)
    new DataView(word.buffer).setInt32(0, CanonicalWriter.i32(value), true)
    return this.emit(word)
  }

  i64(value: bigint): this {
    const word = new Uint8Array(8)
    new DataView(word.buffer).setBigInt64(0, CanonicalWriter.signed64(value), true)
    return this.emit(word)
  }

  u128(value: bigint): this {
    if (value < 0n || value >= 1n << 128n) throw new RangeError(`canonical uint128 out of range: ${value}`)
    return this.i64(BigInt.asIntN(64, value)).i64(BigInt.asIntN(64, value >> 64n))
  }

  string(value: string): this {
    return this.bytes(_text.encode(value))
  }

  bytes(value: Uint8Array): this {
    return this.ordinal(value.byteLength).raw(value)
  }

  raw(value: Uint8Array): this {
    return this.emit(value)
  }

  rows<A>(values: ReadonlyArray<A>, field: (value: A, writer: CanonicalWriter) => void): this {
    this.ordinal(values.length)
    for (const value of values) field(value, this)
    return this
  }

  close(): Iterable<Uint8Array> {
    return this.#chunks.map((chunk) => chunk.slice())
  }
}

const _minted = <Kind extends Digest.Kind>(kind: Kind, hex: string): Effect.Effect<Digest.Key<Kind>> =>
  Effect.orDie(Schema.decode(_algorithms.at(kind).key)(hex))

const _hasher = <Kind extends Digest.Kind>(kind: Kind): Effect.Effect<IHasher> =>
  Effect.promise(() => _rows[kind].make())

const _walk = (hasher: IHasher, payload: Digest.Payload): string => {
  const armed = hasher.init()
  if (Predicate.isUint8Array(payload)) armed.update(payload)
  else for (const chunk of payload) armed.update(chunk)
  return armed.digest()
}

const ArtifactId = {
  Key: _artifactKey,
  codec: _artifactCodec,
  mint: (payload: Digest.Payload): Effect.Effect<ArtifactId.Identity> =>
    Effect.flatMap(
      Effect.promise(() => createSHA256()),
      (hasher) => Effect.orDie(Schema.decode(_artifactKey)(_walk(hasher, payload))),
    ),
} as const

declare namespace ArtifactId {
  type Identity = Schema.Schema.Type<typeof _artifactKey>
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { ArtifactId, CanonicalWriter, Digest }
```

## [02]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
