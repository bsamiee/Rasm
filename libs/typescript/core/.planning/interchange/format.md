# [CORE_FORMAT]

`Format` owns the branch's encoding engines. One defect-normalizing transform lifts every third-party decoder onto the typed `ParseError` rail, four arms — protobuf, CBOR, MessagePack, JSON — publish bounded complete-payload codecs, the closed RFC 6902 algebra applies patches prototype-safely, and the announced-fact media roster names which arm renders each event format. Module `core/src/interchange/format.ts` admits an encoding as one arm row, a descriptor family as one `_suite` key, a MessagePack extension as one type-byte registration, and an event format as one media row.

`Format` composes the `value` floor's `Shape`, `Clock.Hlc`, and `Fault.Class` beside the descriptor modules the root `buf.gen.yaml` emits, and hands `interchange/codec` the arm rows that select a family's codec, name its contract-compatibility token, and render a quarantined frame. Every engine and ceiling configures once at module initialization, so an ingress arms its bounds before the first untrusted byte.

## [01]-[INDEX]

- [02]-[ENGINE_FOLD]: one defect-normalizing fold every arm's transform composes; interior.
- [03]-[PROTO_ENGINE]: semantic protobuf and the singular descriptor registry; `Format.proto`.
- [04]-[CBOR_ENGINE]: bounded complete-payload decode; `Format.cbor`.
- [05]-[MSGPACK_ENGINE]: bounded complete payloads and the `Clock.Hlc` extension; `Format.msgpack`.
- [06]-[JSONPATCH_ENGINE]: typed operations and prototype-safe rooted application; `Format.Patch`.
- [07]-[JSON_ENGINE]: bounded UTF-8 JSON and refused-octet rendering; `Format.json`.
- [08]-[ARM_ROWS]: fits, admit, compatibility, self-description, render, and degrade per arm; `Format.arms`.
- [09]-[EVENT_FORMAT]: announced-fact media rows, prefix framing, the demand vocabulary, and the one seat-discriminated gate; `Format.event`.

## [02]-[ENGINE_FOLD]

- Owner: `_lifted` admits raw octets, normalizes engine defects, and bounds encoded output.
- Law: engines and registries configure once at module initialization.
- Law: a decode-only engine refuses encode with `ParseResult.Forbidden`.
- Packages: `effect` (`Schema`, `ParseResult`, `Either`).

```typescript signature
import { Array, Data, Effect, Either, HashSet, Match, Option, ParseResult, Schema, type Types } from "effect"

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
- Law: `_suite` and `registry` hold estate-declared families alone; a foreign publisher's descriptor reaches `frame`/`family` as an argument, so the CloudEvents generation never enters this registry.
- Law: the root `buf.gen.yaml` `protoc-gen-es` row is the ONE producer of every symbol `_suite` binds; `nx run workspace-foundation:proto` is its runnable entry, cached on corpus and template and restored before `typecheck`.
- Law: the plugin Renders one `_pb.ts` per source at the module-relative path, so a family binds through the module its own `.proto` declares.
- Law: no barrel stands between; an aggregate re-export would be a forwarding shell whose roster drifts off the one the generator emits.
- Law: regeneration triggers on a moved `rasm.<family>.v1` source under `tests/contracts/rasm/`, or a `@bufbuild/protoc-gen-es` version or option change; every emitted file's own header stamps both, so a stale tree announces its own drift.
- Packages: `@bufbuild/protobuf`; `effect`; `../value/schema.ts`; the generated `./gen/rasm/<family>/v1/<family>_pb.ts` modules.

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
import * as channels from "./gen/rasm/channels/v1/channels_pb.ts"
import * as compute from "./gen/rasm/compute/v1/compute_pb.ts"
import * as element from "./gen/rasm/element/v1/element_pb.ts"
import * as organization from "./gen/rasm/organization/v1/organization_pb.ts"

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
// re-spells none. The generator Renders one module per source, so the QUALIFIER on each value below is the declaring
// source and the grouping is the import graph rather than a comment claiming one: element the graph snapshots and
// their node and edge payloads, channels the texture families, compute the suite service vocabulary, organization the
// containment message. `rasm.scene.v1` generates beside them and binds no key, because this branch decodes no
// captured-scene family — an inert emission the whole-module run costs, where a path-filtered run would instead cost a
// SOURCE the day a roster reaches one.
// Compute's messages carry NO `Wire` suffix while element's carry it on every message, because that suffix breaks a
// COLLISION rather than marking a projection — element seats a wire type beside a domain twin per message, where
// nothing co-resident with compute's collides. `AssetSetManifest` reads unsuffixed for that one reason.
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
  ElementGraphWire: element.ElementGraphWireSchema,
  GraphDeltaWire: element.GraphDeltaWireSchema,
  NodeWire: element.NodeWireSchema,
  RelationshipWire: element.RelationshipWireSchema,
  TextureSetWire: channels.TextureSetWireSchema,
  AssetSetManifest: channels.AssetSetManifestSchema,
  FaultDetail: compute.FaultDetailSchema,
  ArtifactFrame: compute.ArtifactFrameSchema,
  GeometryPayload: compute.GeometryPayloadSchema,
  OrganizationWire: organization.OrganizationWireSchema,
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
// `text` alone owns the lossy twin: a quarantined frame renders where the strict pair already refused, and no
// decode path reaches it.
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
- Law: `fits` is the one sentence a reader selects an arm on; `admit` binds an owned schema and NAMES the one row it can lack.
- Law: `_armAbsences` is the arm plane's share of `[09]`'s capability vocabulary, so an arm's refusal and a format's refusal read from one closed set.
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

// The one row an ARM can lack. Field names live in the descriptor and never in the bytes, so the proto arm binds
// nothing without one while the three self-describing arms lack nothing at all. Naming that row HERE is what lets
// `[09]`'s demand vocabulary fold an arm's refusal and a format's refusal into one closed set, instead of a caller
// reading a bare absence off this table and a second bare absence off that one.
const _armAbsences = ["descriptor"] as const

declare namespace Arm {
  type Kind = (typeof _arms)[number]
  type Absent = (typeof _armAbsences)[number]
  type Row = {
    readonly fits: string
    readonly admit: <A, I>(
      owned: Schema.Schema<A, I>,
      descriptor: Option.Option<DescMessage>,
    ) => Either.Either<Schema.Schema<A, Uint8Array>, Absent>
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
    // This arm alone can REFUSE its admission, and it names the row it lacks rather than answering a bare absence:
    // without its family's descriptor no schema binds, where the three self-describing arms bind unconditionally.
    admit: (owned, descriptor) =>
      Option.match(descriptor, {
        onNone: () => Either.left<Arm.Absent>("descriptor"),
        onSome: (gen) => Either.right(Proto.family(gen, owned)),
      }),
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
    admit: (owned) => Either.right(Json.schema(owned)),
    compatibility: "json",
    selfDescribing: true,
    // Held octets ARE the document, so this render survives the malformed case the other three arms cannot.
    render: (octets) => Option.some(Json.text(octets)),
    degrade: "<base64-inflated-octets>",
  },
  cbor: {
    fits: "<canonical-binary-payload-a-foreign-writer-mints-and-this-branch-only-reads>",
    admit: (owned) => Either.right(Cbor.schema(owned)),
    compatibility: "binary",
    selfDescribing: true,
    render: (octets) => _decoded(Cbor.frame)(octets),
    degrade: "<encode-refused-unproven-map-order>",
  },
  msgpack: {
    fits: "<compact-binary-payload-carrying-extension-cells-and-i64-magnitudes>",
    admit: (owned) => Either.right(Pack.schema(owned)),
    compatibility: "binary",
    selfDescribing: true,
    render: (octets) => _decoded(Pack.frame)(octets),
    degrade: "<arm-local-key-sort-only>",
  },
} as const satisfies { readonly [K in Arm.Kind]: Arm.Row }

type _ArmKind = Arm.Kind
type _ArmRow = Arm.Row
type _ArmAbsent = Arm.Absent
```

## [09]-[EVENT_FORMAT]

- Owner: `_eventFormatRows` carries the announced-fact media roster — one row per format, the batch message envelope it defines, the content modes it codes, the arm that renders it, and what the format forfeits.
- Law: the roster is JSON, Protobuf, and Avro; CBOR, XML, and avro-compact are working drafts and no row admits one.
- Law: `batch` and `binary` are CAPABILITY columns, not universal fields — the Avro format defines neither a batch message envelope nor a binary-mode payload codec, so its `batch` reads `Option.none()` and its `binary` reads false, and a consumer routing `application/cloudevents-batch+avro` addresses a media type the specification never minted.
- Law: a row's `arm` names a core engine or stands empty, so the ONE format whose engine is host-bound declares that on its `degrade` rather than putting a `Buffer` in a core signature.
- Law: `framed` reads the media-type PREFIX, so one read recovers both the format and the arity and a format's batch message envelope needs no second dispatch; a row carrying no batch message envelope is skipped by the batch pass rather than compared against a spelling it never publishes.
- Law: `_needs` is the ONE capability vocabulary both planes name — an arm's absent descriptor and a format's absent content mode are rows in one closed set, never two refusal ladders.
- Law: a `Demand` is the content-mode set a caller states; the codec pair rides every demand, so no call site restates it and none can omit it.
- Law: `admitted` is the ONE gate and is the ONE batch-encode owner, since no `Binding` the package ships carries a batch serializer at any transport.
- Law: the seat's own tag says whether a core arm or a lane engine mints the codec, so no caller reads the arm column to choose an entry.
- Law: refusal is the family CENSUS naming `demand \ (format ∩ seat)` in roster order, graded at the dominant `Fault.Class` across those rows; a bare absence naming no row is the deleted form.
- Law: a lane engine offered to a core-armed row and a core seat offered to an arm-less row both refuse as `codec` — the format contract owns that codec identity and a second mint beside it is denied.
- Law: batch arity is the caller's schema, never a second entrypoint — a producer encoding a sequence passes `Schema.Array` and reads the row's own `batch` media type, which a format defining none refuses at the read.
- Law: a producer past the transport budget splits before encoding, since a relay re-framing a batch cannot re-sign what it respelled.
- Law: the JSON row delegates the message-envelope body to the package's own structured mode; the Protobuf row takes the CloudEvents descriptor through its `Core` seat, which the wire registry never admits.
- Growth: a format is one row; a format gaining a core engine fills its `arm` and empties its `degrade`, while a host-bound engine stays at its lane and enters through a `Lane` seat, the row's arm staying empty.
- Growth: a new capability is one `_needs` row carrying its class and its sentence, plus one `_modeHeld` reader where a format's own cell answers it.
- Boundary: content mode, transport headers, and per-binding thresholds seat at the consuming binding; this cluster owns the media roster and its framing alone.
- Blocker: the CloudEvents descriptor has NO producer row — root `buf.yaml` excludes `tests/contracts/io` from buf's one module, so `buf generate` targets zero files under it.
- Blocker: the protobuf row's descriptor therefore arrives as a caller-supplied argument until the corpus seats that vendored path as its own module carved out of the format lane.
- Packages: `effect`; `@bufbuild/protobuf` (`DescMessage`); `../value/fault.ts` (`Fault.Class`).

```typescript signature
import { Fault } from "../value/fault.ts"

const _eventFormats = ["avro", "json", "protobuf"] as const

// The two needs a row's OWN cells answer with no seat in hand; the codec pair below is answered by the mint instead.
const _modes = ["batch", "binary"] as const

// ONE capability vocabulary spanning both planes — what a format's row declares and what a seat supplies — so a
// refusal names ROWS from one closed set instead of collapsing four distinct answers into one shapeless absence. The
// arm plane contributes its own share by spread, so a second row landing at `[08]` reaches the demand with no edit
// here. `class` grades each row onto the caller's fault ladder and `fits` is the sentence an operator reads.
// Every need refuses ABOUT one format, so that is the subject each row declares and each row renders the sentence its
// own need means. `leg` partitions the plane that DECIDES: a content mode is the format ROW's own cell, while the
// codec pair is the SEAT's, so a census reads which plane refused without re-deriving it from the word.
const _needs = [..._modes, "codec", ..._armAbsences] as const
const _Lacked = Schema.Struct({ format: Schema.Literal(..._eventFormats) })
const _need = Fault.Class.family(_needs, {
  batch: Fault.Class.row({
    class: "absent",
    leg: "row",
    detail: _Lacked,
    render: ({ format }) => `${format} <no-batch-message-envelope-this-format-defines>`,
  }),
  binary: Fault.Class.row({
    class: "absent",
    leg: "row",
    detail: _Lacked,
    render: ({ format }) => `${format} <no-binary-mode-payload-codec-this-format-defines>`,
  }),
  // `denied`, never `absent`: the codec identity EXISTS and the SEAT is refused against it. A lane engine offered to a
  // core-armed row is the second mint the format contract forbids, and a core seat offered to an arm-less row asks
  // this roster for an engine it holds deliberately empty. `denied` outranks `absent` on the class order, so a
  // refusal carrying both grades on the seat rather than on the content mode.
  codec: Fault.Class.row({
    class: "denied",
    leg: "seat",
    detail: _Lacked,
    render: ({ format }) => `${format} <seat-contradicts-the-row-arm-column>`,
  }),
  descriptor: Fault.Class.row({
    class: "absent",
    leg: "seat",
    detail: _Lacked,
    render: ({ format }) => `${format} <no-descriptor-for-the-descriptor-bound-arm>`,
  }),
})

// Each mode's cell carries its own shape — a batch envelope is the media type a format DEFINES, a binary mode is a
// plain capability bit — so the read is one row per mode rather than a uniform column the roster could not honestly
// carry. Every other need is proved by the mint, which is why no third reader lands beside these two.
const _modeHeld = {
  batch: (row: EventFormat.Row) => Option.isSome(row.batch),
  binary: (row: EventFormat.Row) => row.binary,
} as const satisfies { readonly [Mode in EventFormat.Mode]: (row: EventFormat.Row) => boolean }

// A caller states the CONTENT MODES it needs and the codec pair rides every demand, because a gate minting no schema
// answers nothing: seating the pair here is what keeps a call site from omitting the two needs it always has.
const _CODEC_NEEDS = HashSet.fromIterable<EventFormat.Need>(["codec", "descriptor"])
const _demand = (...modes: ReadonlyArray<EventFormat.Mode>): EventFormat.Demand =>
  HashSet.union(_CODEC_NEEDS, HashSet.fromIterable(modes))

// The seat is a TAGGED value, so the gate recovers which codec plane a caller brought from the value itself. A pair of
// entrypoints partitioned on the arm column would be two names for one question, and a caller would have to know which
// side of that column its format sits on before it could choose which name to ask with.
type _SeatUnion = Data.TaggedEnum<{
  Core: { readonly descriptor: Option.Option<DescMessage> }
  Lane: { readonly engine: EventFormat.Engine }
}>
const _Seat = Data.taggedEnum<_SeatUnion>()

// The refusal is a VALUE naming every row the demand asked for that the pair does not hold, in roster order, and it
// grades itself at the dominant class across those rows — so an Avro batch request and a protobuf-without-descriptor
// request stop reading identically at the call site. The carrier is the family's OWN census: dominance, class, leg,
// and the joined message all derive from the roster, and a local `{ needs, class, detail }` triple beside it would
// fork one taxonomy into two.
const _Missing = _need.census("EventFormatMissing")

declare namespace EventFormat {
  type Kind = (typeof _eventFormats)[number]
  type Mode = (typeof _modes)[number]
  type Need = (typeof _needs)[number]
  type Demand = HashSet.HashSet<Need>
  type Seat = _SeatUnion
  type Issue = typeof _need.payload.Type
  type Missing = InstanceType<typeof _Missing>
  type Row = {
    readonly media: string
    readonly batch: Option.Option<string> // the batch message envelope this format defines; a format defining none reads none
    readonly binary: boolean // whether this format codes a binary-mode payload, so a binding reads capability instead of attempting one
    readonly arm: Option.Option<Arm.Kind>
    readonly selfDescribing: boolean
    readonly degrade: string
  }
  type Framing = { readonly format: Kind; readonly batch: boolean }
  // Engine pairs cross a `Lane` seat on the Either rail: throws convert to that rail AT THE LANE, so no host type and
  // no exception channel enters a core signature.
  type Engine = {
    readonly read: (octets: Uint8Array) => Either.Either<unknown, string>
    readonly write: (value: unknown) => Either.Either<Uint8Array, string>
  }
  type _Rows<T extends { readonly [K in Kind]: Row } = typeof _eventFormatRows> = T
}

// Each defined batch message envelope spells its own format's media type with `-batch` before the `+suffix`, so the framing
// read below recovers format and arity from one prefix comparison and no arity column exists on the row.
const _eventFormatRows = {
  // Avro alone carries a host-bound engine: `avsc` reads `Buffer`-only slice methods, which core forbids, so
  // this arm stands empty and the node lane seats its engine rather than dragging a host type into S0. Its batch and
  // binary cells are the SPECIFICATION's own silence — the Avro format defines one structured message envelope and
  // nothing else — so no lane supplies them and a peer demanding either reads that need NAMED off the refusal.
  avro: {
    media: "application/cloudevents+avro",
    batch: Option.none<string>(),
    binary: false,
    arm: Option.none<Arm.Kind>(),
    selfDescribing: false,
    degrade: "<node-lane-engine-seat;structured-single-only>",
  },
  json: {
    media: "application/cloudevents+json",
    batch: Option.some("application/cloudevents-batch+json"),
    binary: true,
    arm: Option.some<Arm.Kind>("json"),
    selfDescribing: true,
    degrade: "<base64-inflated-binary-data>",
  },
  protobuf: {
    media: "application/cloudevents+protobuf",
    batch: Option.some("application/cloudevents-batch+protobuf"),
    binary: true,
    arm: Option.some<Arm.Kind>("proto"),
    selfDescribing: false,
    degrade: "<descriptor-required-per-decode>",
  },
} as const satisfies { readonly [K in EventFormat.Kind]: EventFormat.Row }

// Batch prefixes test first, so a media type answering both comparisons resolves to the wider frame rather
// than decoding a sequence as one message envelope; a format publishing no batch media type leaves the pass
// with nothing to compare, so its structured media type is the only spelling this read can recover.
const _framed = (contentType: string): Option.Option<EventFormat.Framing> =>
  Option.orElse(
    Option.map(
      Array.findFirst(_eventFormats, (format) =>
        Option.exists(_eventFormatRows[format].batch, (media) => contentType.startsWith(media))),
      (format) => ({ format, batch: true }),
    ),
    () =>
      Option.map(
        Array.findFirst(_eventFormats, (format) => contentType.startsWith(_eventFormatRows[format].media)),
        (format) => ({ format, batch: false }),
      ),
  )

// A lane's host codec throws AT THE LANE and crosses on the Either rail, so no host type and no exception channel
// enters this signature and the composed admission is indistinguishable from a core arm's at the intake gate.
const _laned = <A, I>(engine: EventFormat.Engine, owned: Schema.Schema<A, I>): Schema.Schema<A, Uint8Array> =>
  Schema.compose(
    Schema.transformOrFail(Schema.Uint8ArrayFromSelf, Schema.Unknown, {
      strict: true,
      decode: (octets, _, ast) =>
        Either.match(engine.read(octets), {
          onLeft: (refusal) => ParseResult.fail(new ParseResult.Type(ast, octets, refusal)),
          onRight: (tree) => ParseResult.succeed(tree),
        }),
      encode: (tree, _, ast) =>
        Either.match(engine.write(tree), {
          onLeft: (refusal) => ParseResult.fail(new ParseResult.Type(ast, tree, refusal)),
          onRight: (octets) => ParseResult.succeed(octets),
        }),
    }),
    owned,
    { strict: false },
  )

// The codec half answers with the schema itself or with the one row its refusal names, so the two refusals a bare
// `Option` fused stay apart: a seat contradicting the arm column, and an arm whose descriptor never arrived.
const _bound = <A, I>(
  format: EventFormat.Kind,
  owned: Schema.Schema<A, I>,
  seat: EventFormat.Seat,
): Either.Either<Schema.Schema<A, Uint8Array>, EventFormat.Missing> =>
  _Seat.$match(seat, {
    Core: ({ descriptor }) =>
      Option.match(_eventFormatRows[format].arm, {
        onNone: () => Either.left(new _Missing({ issues: [{ reason: "codec", format }] })),
        onSome: (arm) =>
          Either.mapLeft(_armRows[arm].admit(owned, descriptor), (need) => new _Missing({ issues: [{ reason: need, format }] })),
      }),
    Lane: ({ engine }) =>
      Option.match(_eventFormatRows[format].arm, {
        onNone: () => Either.right(_laned(engine, owned)),
        onSome: () => Either.left(new _Missing({ issues: [{ reason: "codec", format }] })),
      }),
  })

// ONE gate over both seats and both arities. `demand \ (format ∩ seat)` computes once: what the pair HOLDS is the
// row's own capability cells joined to whichever codec rows the mint proved, and the complement of the demand over
// that join is the refusal. Arity stays the caller's schema — a sequence passes `Schema.Array` and reads the row's own
// `batch` media type — so no arity column exists and no second entrypoint re-describes what the schema already states.
const _admitted = <A, I>(
  format: EventFormat.Kind,
  demand: EventFormat.Demand,
  owned: Schema.Schema<A, I>,
  seat: EventFormat.Seat,
): Either.Either<Schema.Schema<A, Uint8Array>, EventFormat.Missing> => {
  const bound = _bound(format, owned, seat)
  const held = HashSet.union(
    HashSet.fromIterable(Array.filter(_modes, (mode) => _modeHeld[mode](_eventFormatRows[format]))),
    Either.match(bound, {
      onLeft: (refusal) => HashSet.difference(_CODEC_NEEDS, HashSet.fromIterable(Array.map(refusal.issues, (issue) => issue.reason))),
      onRight: () => _CODEC_NEEDS,
    }),
  )
  const lacked = HashSet.difference(demand, held)
  return Array.match(Array.filter(_needs, (need) => HashSet.has(lacked, need)), {
    // Nothing the demand asked for is missing, so the mint IS the answer — every demand carries the codec pair, so
    // this arm is reachable only where `bound` already settled right.
    onEmpty: () => bound,
    onNonEmpty: (needs) => Either.left(new _Missing({ issues: Array.map(needs, (reason) => ({ reason, format })) })),
  })
}

const EventFormat: {
  readonly formats: typeof _eventFormats
  readonly rows: typeof _eventFormatRows
  readonly framed: typeof _framed
  readonly seat: typeof _Seat
  readonly demand: typeof _demand
  readonly admitted: typeof _admitted
} = {
  formats: _eventFormats,
  rows: _eventFormatRows,
  framed: _framed,
  seat: _Seat,
  demand: _demand,
  admitted: _admitted,
}

// --- [EXPORTS] --------------------------------------------------------------------------

declare namespace Format {
  type Arm = _ArmKind
  namespace Arm {
    type Row = _ArmRow
    type Absent = _ArmAbsent
  }
  type Event = EventFormat.Kind
  namespace Event {
    type Row = EventFormat.Row
    type Framing = EventFormat.Framing
    type Engine = EventFormat.Engine
    type Mode = EventFormat.Mode
    type Need = EventFormat.Need
    type Demand = EventFormat.Demand
    type Seat = EventFormat.Seat
    type Missing = EventFormat.Missing
  }
  type Shape = {
    readonly arms: typeof _arms
    readonly rows: { readonly arm: typeof _armRows; readonly event: typeof _eventFormatRows }
    readonly proto: typeof Proto
    readonly cbor: typeof Cbor
    readonly msgpack: typeof Pack
    readonly Patch: typeof Patch
    readonly json: typeof Json
    readonly event: typeof EventFormat
  }
}

const Format: Format.Shape = {
  arms: _arms,
  rows: { arm: _armRows, event: _eventFormatRows },
  proto: Proto,
  cbor: Cbor,
  msgpack: Pack,
  Patch,
  json: Json,
  event: EventFormat,
}

export { Format }
```

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
