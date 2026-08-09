# [CORE_FORMAT]

`Format` owns the branch's encoding engines. One defect-normalizing transform lifts every third-party decoder onto the typed `ParseError` rail, four arms — protobuf, CBOR, MessagePack, JSON — publish bounded complete-payload codecs, and the closed RFC 6902 algebra applies patches prototype-safely. Module `core/src/interchange/format.ts` admits an encoding as one arm row, a descriptor family as one `_suite` key, and a MessagePack extension as one type-byte registration.

`Format` composes the `value` floor's `Shape` and `Clock.Hlc` beside the generated `interchange_pb.ts` descriptors, and hands `interchange/codec` the arm rows that select a family's codec, name its contract-compatibility token, and render a quarantined frame. Every engine and ceiling configures once at module initialization, so an ingress arms its bounds before the first untrusted byte.

## [01]-[INDEX]

- [02]-[ENGINE_FOLD]: one defect-normalizing fold every arm's transform composes; interior.
- [03]-[PROTO_ENGINE]: semantic protobuf and the singular descriptor registry; `Format.proto`.
- [04]-[CBOR_ENGINE]: bounded complete-payload decode; `Format.cbor`.
- [05]-[MSGPACK_ENGINE]: bounded complete payloads and the `Clock.Hlc` extension; `Format.msgpack`.
- [06]-[JSONPATCH_ENGINE]: typed operations and prototype-safe rooted application; `Format.Patch`.
- [07]-[JSON_ENGINE]: bounded UTF-8 JSON and refused-octet rendering; `Format.json`.
- [08]-[ARM_ROWS]: fits, admit, compatibility, self-description, render, and degrade per arm; `Format.arms`.

## [02]-[ENGINE_FOLD]

- Owner: `_lifted` admits raw octets, normalizes engine defects, and bounds encoded output.
- Law: engines and registries configure once at module initialization.
- Law: a decode-only engine refuses encode with `ParseResult.Forbidden`.
- Packages: `effect` (`Schema`, `ParseResult`, `Either`).

```typescript signature
import { Array, Effect, Either, Match, Option, ParseResult, Schema, type Types } from "effect"

const _Octets = Schema.Uint8ArrayFromSelf.pipe(
  Schema.filter((octets) => octets.byteLength <= Shape.Ingress.floor.bytes, {
    message: () => "<payload-overrun>",
  }),
)

const _lifted = (
  decode: (octets: Uint8Array) => unknown,
  encode: (value: unknown) => Uint8Array | undefined,
): Schema.Schema<unknown, Uint8Array> =>
  Schema.transformOrFail(_Octets, Schema.Unknown, {
    strict: true,
    decode: (octets, _options, ast) =>
      Either.try({ try: () => decode(octets), catch: (defect) => new ParseResult.Type(ast, octets, String(defect)) }),
    encode: (value, _options, ast) =>
      Either.try({ try: () => encode(value), catch: (defect) => new ParseResult.Type(ast, value, String(defect)) }).pipe(
        Either.flatMap(Either.liftPredicate(
          (octets): octets is Uint8Array => octets !== undefined,
          () => new ParseResult.Forbidden(ast, value, "<decode-only>"),
        )),
      ),
  })
```

## [03]-[PROTO_ENGINE]

- Owner: `Format.proto` composes complete protobuf messages with owned schemas and exposes one descriptor registry.
- Law: unknown fields survive semantic decode and encode, while recursion and raw-byte admission remain bounded.
- Law: protobuf parity is semantic; only frozen map and unknown-field posture fixtures may claim exact bytes.
- Law: generated descriptors enter through `_suite`; no handwritten peer schema or second registry exists.
- Law: `_suite` keys transcribe declared message names verbatim; a family owning no descriptor source rides a `codec` arm.
- Packages: `@bufbuild/protobuf`; `effect`; `../value/schema.ts`; generated `./interchange_pb.ts`.

```typescript signature
import {
  createRegistry,
  type DescMessage,
  fromBinary,
  isMessage,
  type Message,
  type Registry,
  toBinary,
  toJsonString,
} from "@bufbuild/protobuf"
import { Shape } from "../value/schema.ts"
import * as pb from "./interchange_pb.ts"

const _READ = { readUnknownFields: true, recursionLimit: 24 } as const
const _WRITE = { writeUnknownFields: true } as const

const _Message: Schema.Schema<Message> = Schema.declare((input: unknown): input is Message => isMessage(input))

const _frame = (gen: DescMessage): Schema.Schema<Message, Uint8Array> =>
  _lifted(
    (octets) => fromBinary(gen, octets, _READ),
    (value) => (isMessage(value, gen) ? toBinary(gen, value, _WRITE) : undefined),
  ).pipe(Schema.compose(_Message, { strict: false }))

// Keys TRANSCRIBE descriptor names verbatim — a key is the message a `.proto` declares and its value the generated
// `<Name>Schema` protoc-gen-es derives from that same name — so the corpus mints every spelling and this registry
// re-spells none. Rows group by declaring source: element the graph envelopes and their node and edge payloads,
// channels the texture families, compute the suite service vocabulary, organization the containment envelope.
// Compute's messages carry NO `Wire` suffix while element's carry it on every message, because that suffix breaks a
// COLLISION rather than marking a projection — element seats a wire type beside a domain twin per message, where
// nothing co-resident with compute's collides. `AssetSetManifest` reads unsuffixed for that one reason.
//
// Families enter only where THIS branch decodes them: `rasm.scene.v1` declares a landed source whose consumer is the
// python energy owner, so a row for it mounts a reader against bytes no browser receives.
//
// Families owning no descriptor source stay absent BY LAW and ride their own `interchange/codec` arm: the AppHost
// runtime-evidence set ([02.21]) and the AppUi product-shell set ([02.22]) owe none under [02.9]; the appearance
// families mint their wire as the producer's MessagePack integer-keyed roster; `HlcStampWire` carries the [02.7]
// two-half cell whose frozen layout a descriptor's tag bytes would displace; a quantity crosses inside
// `MeasureValueWire`. Seam summary is no family at all — it rides `NodeWire` field 7 as the `rasm.element.v1`
// payload.
const _names = [
  "ElementGraphWire", "GraphDeltaWire", "NodeWire", "RelationshipWire",
  "TextureSetWire", "AssetSetManifest",
  "FaultDetail", "ArtifactFrame", "GeometryPayload",
  "OrganizationWire",
] as const

const _suite = {
  ElementGraphWire: pb.ElementGraphWireSchema,
  GraphDeltaWire: pb.GraphDeltaWireSchema,
  NodeWire: pb.NodeWireSchema,
  RelationshipWire: pb.RelationshipWireSchema,
  TextureSetWire: pb.TextureSetWireSchema,
  AssetSetManifest: pb.AssetSetManifestSchema,
  FaultDetail: pb.FaultDetailSchema,
  ArtifactFrame: pb.ArtifactFrameSchema,
  GeometryPayload: pb.GeometryPayloadSchema,
  OrganizationWire: pb.OrganizationWireSchema,
} as const

declare namespace Proto {
  type Name = keyof typeof _suite
  type Shape = Types.Simplify<{
    readonly names: typeof _names
    readonly suite: typeof _suite
    readonly registry: Registry
    readonly frame: typeof _frame
    readonly family: <A, I>(gen: DescMessage, owned: Schema.Schema<A, I>) => Schema.Schema<A, Uint8Array>
  }>
  type _Rows<T extends Record<(typeof _names)[number], DescMessage> = typeof _suite> = T
  type _Keys<K extends (typeof _names)[number] = Name> = K
}

const Proto: Proto.Shape = {
  names: _names,
  suite: _suite,
  registry: createRegistry(...Array.map(_names, (name) => _suite[name])),
  frame: _frame,
  family: (gen, owned) => _frame(gen).pipe(Schema.compose(owned, { strict: false })),
}
```

## [04]-[CBOR_ENGINE]

- Owner: `Format.cbor`, the bounded RFC 8949 decoder for one complete payload.
- Law: the arm is decode-only because cbor-x does not prove producer map ordering.
- Law: package size ceilings derive from `Shape.Ingress` and arm before decoder construction.
- Law: transport framing supplies one complete payload because raw chunk walks accept incomplete EOF tails.
- Packages: `cbor-x` (`Decoder`, `setSizeLimits` via the local augmentation); `effect` (`Schema`).

```typescript signature
import { Decoder, setSizeLimits } from "cbor-x"

declare module "cbor-x" {
  function setSizeLimits(limits: {
    readonly maxArraySize?: number
    readonly maxMapSize?: number
  }): void
}

const _POSTURE = { useRecords: false, mapsAsObjects: true, tagUint8Array: true } as const
const _CEILINGS = {
  maxArraySize: Shape.Ingress.floor.collection,
  maxMapSize: Shape.Ingress.floor.members,
} as const

setSizeLimits(_CEILINGS) // The once-at-init DoS gate arms before codec construction.
const _cborDecoder = new Decoder(_POSTURE)

// `schema` and `frame` are one construction read at two grains, exactly as the MessagePack arm reads them: a family
// composes its owned schema onto the frame and a held payload renders through the bare one. Leaving `schema` off
// this arm made its one family spell the composition inline, so the cbor binding read unlike every sibling row.
const _cborFrame: Schema.Schema<unknown, Uint8Array> =
  _lifted((octets) => _cborDecoder.decode(octets), () => undefined)

const Cbor: {
  readonly frame: Schema.Schema<unknown, Uint8Array>
  readonly schema: <A, I>(owned: Schema.Schema<A, I>) => Schema.Schema<A, Uint8Array>
} = {
  frame: _cborFrame,
  schema: (owned) => _cborFrame.pipe(Schema.compose(owned, { strict: false })),
}
```

## [05]-[MSGPACK_ENGINE]

- Owner: `Format.msgpack` owns bounded complete-payload decode, encode, and the `Clock.Hlc` extension.
- Law: extension decode delegates to `Clock.Hlc.FromBytes`, and i64 values remain `bigint`.
- Law: transport framing supplies complete payloads because package stream decoders accept incomplete EOF tails.
- Law: encoder key sorting stabilizes this arm's egress without claiming a cross-implementation canonical encoding.
- Law: `frame` decodes a payload with no owned schema, so a held frame renders where its family's schema already refused.
- Packages: `@msgpack/msgpack` (`Decoder`, `Encoder`, `ExtData`, `ExtensionCodec`); `effect` (`Schema`); `../value/clock.ts` (`Clock.Hlc`).

```typescript signature
import { Decoder as PackDecoder, Encoder as PackEncoder, ExtData, ExtensionCodec } from "@msgpack/msgpack"
import { Clock } from "../value/clock.ts"

const _EXT = { hlc: 8 } as const
const _PACK_CEILINGS = {
  maxStrLength: Shape.Ingress.floor.bytes,
  maxBinLength: Shape.Ingress.floor.bytes,
  maxArrayLength: Shape.Ingress.floor.collection,
  maxMapLength: Shape.Ingress.floor.members,
  maxExtLength: Shape.Ingress.floor.bytes,
} as const

declare namespace Pack {
  type Context = { readonly intern: (cell: Uint8Array) => Clock.Hlc }
  type Shape = {
    readonly frame: Schema.Schema<unknown, Uint8Array>
    readonly schema: <A, I>(owned: Schema.Schema<A, I>) => Schema.Schema<A, Uint8Array>
  }
}

const _context: Pack.Context = { intern: Schema.decodeSync(Clock.Hlc.FromBytes) }
const _cell = Schema.encodeSync(Clock.Hlc.FromBytes)

const _extensions: ExtensionCodec<Pack.Context> = new ExtensionCodec<Pack.Context>()
_extensions.register({
  type: _EXT.hlc,
  encode: (value) => (value instanceof Clock.Hlc ? _cell(value) : null),
  decode: (data, _type, context) => context.intern(data),
})

const _packOptions = { extensionCodec: _extensions, context: _context, useBigInt64: true, ..._PACK_CEILINGS } as const
const _packDecoder = new PackDecoder<Pack.Context>(_packOptions)
const _packEncoder = new PackEncoder<Pack.Context>({ ..._packOptions, sortKeys: true })

// One lifted pair serves every family: `schema` composes an owned schema onto it and the bare frame stays reachable,
// so a quarantined payload renders through the same configured decoder that refused it rather than through a second
// construction whose ceilings and extension registry could drift off this one.
const _packFrame: Schema.Schema<unknown, Uint8Array> =
  _lifted((octets) => _packDecoder.decode(octets), (value) => _packEncoder.encode(value))

const Pack: Pack.Shape = {
  frame: _packFrame,
  schema: (owned) => _packFrame.pipe(Schema.compose(owned, { strict: false })),
}
```

## [06]-[JSONPATCH_ENGINE]

- Owner: `Format.Patch` owns the closed RFC 6902 operation union and rooted immutable application.
- Law: paths reject prototype tokens, one structured clone isolates the input pair, and non-root ops delegate to `rfc6902`.
- Law: root removal returns `Option.none`; the EntityEdit members arm requires a present successor.

```typescript signature
import { applyPatch } from "rfc6902"
import { InvalidOperationError, MissingError, TestError } from "rfc6902/patch"
import { Pointer, unescapeToken } from "rfc6902/pointer"

const _prototypeTokens = new Set(["__proto__", "prototype", "constructor"])
const _safePointer = (path: string): boolean =>
  path === "" || path.slice(1).split("/").every((token) => !_prototypeTokens.has(unescapeToken(token)))

const _PatchPointer = Shape.Refined.JsonPointer.pipe(
  Schema.filter(_safePointer, { identifier: "PrototypeSafeJsonPointer" }),
)

const _PatchOperation = Schema.Union(
  Schema.Struct({ op: Schema.Literal("add"), path: _PatchPointer, value: Shape.Json }),
  Schema.Struct({ op: Schema.Literal("remove"), path: _PatchPointer }),
  Schema.Struct({ op: Schema.Literal("replace"), path: _PatchPointer, value: Shape.Json }),
  Schema.Struct({ op: Schema.Literal("move"), from: _PatchPointer, path: _PatchPointer }),
  Schema.Struct({ op: Schema.Literal("copy"), from: _PatchPointer, path: _PatchPointer }),
  Schema.Struct({ op: Schema.Literal("test"), path: _PatchPointer, value: Shape.Json }),
)

const _PatchDocument = Schema.Array(_PatchOperation).pipe(
  Schema.filter((patch) => patch.length <= Shape.Ingress.floor.collection || "<patch-operation-overrun>"),
)

type _PatchJson = Shape.Json
const _patchAlike = Schema.equivalence(Shape.Json)

declare namespace Patch {
  type Operation = Schema.Schema.Type<typeof _PatchOperation>
  type Document = ReadonlyArray<Operation>
  type Slot = InvalidOperationError | MissingError | TestError
  type Shape = {
    readonly Operation: typeof _PatchOperation
    readonly Document: typeof _PatchDocument
    readonly apply: (target: _PatchJson, patch: Document) => Effect.Effect<
      Option.Option<_PatchJson>, readonly [Slot, number]
    >
  }
}

const _missing = (path: string): Either.Either<never, Patch.Slot> => Either.left(new MissingError(path))
const _evaluated = (target: Shape.Json, path: string) => Either.try({
  try: () => Pointer.fromJSON(path).evaluate(target),
  catch: () => new MissingError(path),
})

const _located = (target: Shape.Json, path: string): Either.Either<Shape.Json, Patch.Slot> => {
  return Either.flatMap(_evaluated(target, path), (endpoint) =>
    endpoint.parent !== undefined && endpoint.parent !== null
      && Object.prototype.hasOwnProperty.call(endpoint.parent, endpoint.key)
      ? Either.right(endpoint.value as Shape.Json)
      : _missing(path))
}

const _insert = (target: Shape.Json, path: string, value: Shape.Json): Either.Either<Shape.Json, Patch.Slot> =>
  Either.flatMap(_evaluated(target, path), (endpoint) => {
    if (typeof endpoint.parent !== "object" || endpoint.parent === null) return _missing(path)
    if (Array.isArray(endpoint.parent)) {
      if (endpoint.key === "-") {
        endpoint.parent.push(value)
        return Either.right(target)
      }
      if (!/^(0|[1-9]\d*)$/.test(endpoint.key)) return _missing(path)
      const index = Number(endpoint.key)
      if (!Number.isSafeInteger(index) || index > endpoint.parent.length) return _missing(path)
      endpoint.parent.splice(index, 0, value)
      return Either.right(target)
    }
    Object.defineProperty(endpoint.parent, endpoint.key, {
      configurable: true,
      enumerable: true,
      value,
      writable: true,
    })
    return Either.right(target)
  })

const _present = (
  target: Option.Option<Shape.Json>,
  path: string,
  use: (document: Shape.Json) => Either.Either<Option.Option<Shape.Json>, Patch.Slot>,
): Either.Either<Option.Option<Shape.Json>, Patch.Slot> =>
  Option.match(target, { onNone: () => _missing(path), onSome: use })

const _delegated = (
  document: Shape.Json,
  operation: Patch.Operation,
): Either.Either<Option.Option<Shape.Json>, Patch.Slot> =>
  Either.flatMap(
    Either.try({
      try: () => applyPatch(document, [operation], { implicitArrayCreation: false })[0]!,
      catch: () => new InvalidOperationError(operation),
    }),
    (slot) => slot === null ? Either.right(Option.some(document)) : Either.left(slot),
  )

const _rooted = (
  target: Option.Option<Shape.Json>,
  operation: Patch.Operation,
): Either.Either<Option.Option<Shape.Json>, Patch.Slot> => {
  if (operation.path === "") {
    return Match.value(operation).pipe(
      Match.discriminatorsExhaustive("op")({
        add: ({ value }) => Either.right(Option.some(value)),
        remove: () => _present(target, "", () => Either.right(Option.none())),
        replace: ({ value }) => _present(target, "", () => Either.right(Option.some(value))),
        test: ({ value }) => _present(target, "", (document) =>
          _patchAlike(document, value)
            ? Either.right(target)
            : Either.left(new TestError(document, value))),
        copy: ({ from }) => _present(target, "", (document) =>
          from === "" ? Either.right(target) : Either.map(_located(document, from), Option.some)),
        move: ({ from }) => _present(target, "", (document) =>
          from === "" ? Either.right(target) : Either.map(_located(document, from), Option.some)),
      }),
    )
  }
  return _present(target, operation.path, (document) => Match.value(operation).pipe(
    Match.discriminatorsExhaustive("op")({
      add: (row) => _delegated(document, row),
      remove: (row) => _delegated(document, row),
      replace: (row) => _delegated(document, row),
      test: (row) => _delegated(document, row),
      copy: (row) => row.from === ""
        ? Either.map(_insert(document, row.path, structuredClone(document)), Option.some)
        : _delegated(document, row),
      move: (row) => row.from === ""
        ? Either.left(new InvalidOperationError(row))
        : _delegated(document, row),
    }),
  ))
}

const Patch: Patch.Shape = {
  Operation: _PatchOperation,
  Document: _PatchDocument,
  apply: (target, patch) => Effect.suspend(() => {
    const draft = structuredClone({ target, patch })
    return Array.reduce(
      draft.patch,
      Either.right(Option.some(draft.target)) as Either.Either<Option.Option<Shape.Json>, readonly [Patch.Slot, number]>,
      (held, operation, index) => Either.flatMap(
        held,
        (current) => Either.mapLeft(_rooted(current, operation), (slot) => [slot, index] as const),
      ),
    ).pipe(Effect.fromEither)
  }),
}
```

## [07]-[JSON_ENGINE]

- Owner: `Format.json` owns bounded strict UTF-8 JSON composition and refused-octet rendering.
- Law: `Shape.Ingress.floor.bytes` admits raw octets before UTF-8 allocation.
- Law: structural decode and encode share `Schema.parseJson`; parity compares semantics, not member order.
- Law: transport adapters frame NDJSON and feed complete records.
- Packages: `effect` (`Schema`); `../value/schema.ts` (`Shape`).

```typescript signature
const _TEXT = { fatal: true } as const

const _strict = new TextDecoder("utf-8", _TEXT)
// The lossy twin exists for `text` alone: a quarantined frame renders where the strict pair already refused,
// and no decode path reaches it.
const _render = new TextDecoder("utf-8")
const _jsonEncoder = new TextEncoder()

const _jsonBytes: Schema.Schema<unknown, Uint8Array> =
  _lifted((octets) => _strict.decode(octets), (value) => _jsonEncoder.encode(value as string))

const Json: {
  readonly schema: <A, I>(owned: Schema.Schema<A, I>) => Schema.Schema<A, Uint8Array>
  readonly text: (octets: Uint8Array) => string
} = {
  schema: (owned) => _jsonBytes.pipe(Schema.compose(Schema.parseJson(owned), { strict: false })),
  text: (octets) => _render.decode(octets),
}
```

## [08]-[ARM_ROWS]

- Owner: `_armRows` carries every fact a consumer reads about an encoding without naming the encoding.
- Law: `fits` is the one sentence a reader selects an arm on; `admit` is the entry binding an owned schema to it.
- Law: `compatibility` names the contract-descriptor token the arm's bytes present.
- Law: `selfDescribing` states whether a payload decodes with no owned schema; the proto arm alone needs a descriptor.
- Law: `render` prints a held frame for an operator and is total — a failed decode yields absence, never a throw.
- Law: `degrade` names what the arm gives up; no row leaves it blank and no row spells a capability there.
- Law: an arm decides nothing about tenancy or lifetime, and carries no column stating either.
- Growth: a new encoding is one arm row; a consumer selecting on an arm name reads a column that already exists.
- Boundary: `Wire` owns family-to-arm assignment and supplies the descriptor a proto render needs.

```typescript signature
// One row per encoding arm — the columns every consumer of this plane reads off the arm ALONE, so quarantine
// rendering, contract compatibility, and schema-free reachability stop being three name ladders on three pages.
// `admit` and `render` both take the family's descriptor as an argument rather than resolving one, because a
// descriptor is a FAMILY fact and this table is keyed on the arm: passing it in keeps both members total for the
// three arms that ignore it and keeps the proto row honest about the one thing it cannot supply itself.
//
// TENANCY and LIFETIME are absent as columns because an arm DECIDES NOTHING about either, and a column stating a
// value it does not decide states a guess. An arm is a pure byte-to-value transform holding nothing across calls:
// tenancy rides `Carrier` baggage as `Identity.Tenant` and never enters an encoding, and a decoded value's lifetime
// belongs to whichever consumer bound it. `Wire.Quarantine` is the plane that genuinely decides both, and answers
// them at its own owner.
const _arms = ["proto", "json", "cbor", "msgpack"] as const

declare namespace Arm {
  type Kind = (typeof _arms)[number]
  type Row = {
    readonly fits: string
    readonly admit: <A, I>(
      owned: Schema.Schema<A, I>,
      descriptor: Option.Option<DescMessage>,
    ) => Option.Option<Schema.Schema<A, Uint8Array>>
    readonly compatibility: "binary" | "json"
    readonly selfDescribing: boolean
    readonly render: (octets: Uint8Array, descriptor: Option.Option<DescMessage>) => Option.Option<string>
    readonly degrade: string
  }
  type _Rows<T extends { readonly [K in Kind]: Row } = typeof _armRows> = T
}

// Held frames render for an operator alone, so the printer is TOTAL over what the binary arms decode: `useBigInt64`
// puts real bigints on the op-log, commit, and vector families and a bare `JSON.stringify` throws on every one of
// them, while byte cells would print as objects with numeric keys. Ext cells keep their type byte, so a positional
// record whose slots drifted reads apart from a malformed one at a glance.
const _printed = (value: unknown): string =>
  JSON.stringify(value, (_key, held: unknown) =>
    typeof held === "bigint"
      ? `${held}n`
      : held instanceof Uint8Array
      ? `<bytes:${held.byteLength}>`
      : held instanceof ExtData
      ? { ext: held.type, cell: `<bytes:${(held.data instanceof Uint8Array ? held.data : held.data(0)).byteLength}>` }
      : held, 2) ?? "<unrenderable>"

const _decoded = (schema: Schema.Schema<unknown, Uint8Array>) => (octets: Uint8Array): Option.Option<string> =>
  Option.map(Either.getRight(Schema.decodeUnknownEither(schema)(octets)), _printed)

const _armRows = {
  proto: {
    fits: "<schema-evolving-cross-language-payload-with-a-declared-descriptor>",
    // The one arm whose admission can REFUSE: without its family's descriptor there is no schema to bind, and the
    // three self-describing arms return the composition unconditionally.
    admit: (owned, descriptor) => Option.map(descriptor, (gen) => Proto.family(gen, owned)),
    compatibility: "binary",
    selfDescribing: false,
    // Field names live in the descriptor, never in the bytes, so this row yields absence without one rather than
    // printing a tag-to-value map no operator can read against the `.proto` source.
    render: (octets, descriptor) =>
      Option.flatMap(descriptor, (gen) =>
        Option.map(
          Either.getRight(Schema.decodeUnknownEither(_frame(gen))(octets)),
          (message) => toJsonString(gen, message, { prettySpaces: 2 }),
        )),
    degrade: "<no-self-description>",
  },
  json: {
    fits: "<operator-readable-payload-whose-producer-emits-text>",
    admit: (owned) => Option.some(Json.schema(owned)),
    compatibility: "json",
    selfDescribing: true,
    // Held octets ARE the document, so this render survives the malformed case the other three arms cannot.
    render: (octets) => Option.some(Json.text(octets)),
    degrade: "<base64-inflated-octets>",
  },
  cbor: {
    fits: "<canonical-binary-payload-a-foreign-writer-mints-and-this-branch-only-reads>",
    admit: (owned) => Option.some(Cbor.schema(owned)),
    compatibility: "binary",
    selfDescribing: true,
    render: (octets) => _decoded(Cbor.frame)(octets),
    degrade: "<encode-refused-unproven-map-order>",
  },
  msgpack: {
    fits: "<compact-binary-payload-carrying-extension-cells-and-i64-magnitudes>",
    admit: (owned) => Option.some(Pack.schema(owned)),
    compatibility: "binary",
    selfDescribing: true,
    render: (octets) => _decoded(Pack.frame)(octets),
    degrade: "<arm-local-key-sort-only>",
  },
} as const satisfies { readonly [K in Arm.Kind]: Arm.Row }

type _ArmKind = Arm.Kind
type _ArmRow = Arm.Row

// --- [EXPORTS] --------------------------------------------------------------------------

declare namespace Format {
  type Arm = _ArmKind
  namespace Arm {
    type Row = _ArmRow
  }
  type Shape = {
    readonly arms: typeof _arms
    readonly rows: { readonly arm: typeof _armRows }
    readonly proto: typeof Proto
    readonly cbor: typeof Cbor
    readonly msgpack: typeof Pack
    readonly Patch: typeof Patch
    readonly json: typeof Json
  }
}

const Format: Format.Shape = {
  arms: _arms,
  rows: { arm: _armRows },
  proto: Proto,
  cbor: Cbor,
  msgpack: Pack,
  Patch,
  json: Json,
}

export { Format }
```

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
