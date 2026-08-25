# [CORE_FORMAT]

`Format` owns the branch's encoding engines. One defect-normalizing transform lifts every third-party decoder onto the typed `ParseError` rail, three arms — protobuf, MessagePack, JSON — publish bounded complete-payload codecs, the closed RFC 6902 algebra applies patches prototype-safely, and the announced-fact media roster names which arm renders each event format. Module `core/src/interchange/format.ts` admits an encoding as one arm row, an `Any`-visible descriptor family as one `_suite` key, a MessagePack extension as one type-byte registration, and an event format as one media row.

`Format` composes the value floor beside generated descriptor modules and hands `interchange/codec` the arm rows that select a family's codec and render a quarantined frame. Every engine and ceiling configures once at module initialization, so an ingress arms its bounds before the first untrusted byte.

## [01]-[INDEX]

- [02]-[ENGINE_FOLD]: one defect-normalizing fold every arm's transform composes; interior.
- [03]-[PROTO_ENGINE]: semantic protobuf and the singular descriptor registry; `Format.proto`.
- [04]-[MSGPACK_ENGINE]: bounded complete payloads and the `Clock.Hlc` extension; `Format.msgpack`.
- [05]-[JSONPATCH_ENGINE]: typed operations and prototype-safe rooted application; `Format.Patch`.
- [06]-[JSON_ENGINE]: bounded UTF-8 JSON and refused-octet rendering; `Format.json`.
- [07]-[ARM_ROWS]: fits, admit, self-description, render, and degrade per arm; `Format.arms`.
- [08]-[EVENT_FORMAT]: announced-fact media rows, exact media identity, the demand vocabulary, and the one seat-discriminated gate; `Format.event`.

## [02]-[ENGINE_FOLD]

- Owner: `_lifted` admits raw octets, normalizes engine defects, and bounds encoded output.
- Law: engines and registries configure once at module initialization.
- Law: a decode-only engine refuses encode with `ParseResult.Forbidden`.
- Packages: `effect` (`Schema`, `ParseResult`, `Either`).

```typescript
import { Array, Effect, Either, Match, Option, ParseResult, Predicate, Record, Schema, SchemaAST, type Types } from "effect"

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

- Owner: `Format.proto` composes complete protobuf messages with owned schemas, validates every message at the one admission point, frames the artifact stream, and exposes one descriptor registry.
- Law: every decoded message passes the corpus's own `buf.validate` rules through one `Validator` before it lands, and every encoded message passes them before it leaves; a violation refuses on the `ParseError` rail and a rule that fails to compile refuses as `Forbidden`, never as a silent admission.
- Law: binary and JSON decode share one admission posture — unknown fields survive under both `_READ` and `_JSON_READ`, recursion bounds at one depth under both, and `_JSON_READ` is passed at every `fromJson` site because the package's own default REFUSES an unknown field.
- Law: `framing` is a row of the descriptor family — `binary` for every proto-binary crossing, `json` for every ProtoJSON document a host emits — and the landed value is one shape under both, so a consumer never reads which framing carried it.
- Law: protobuf parity is semantic; only frozen map and unknown-field posture fixtures may claim exact bytes.
- Law: registry-visible generated descriptors enter through `_suite`; a direct owner composes its generated descriptor without registering a second name.
- Law: `interchange/codec#LANDING_EVIDENCE` is such a direct owner for generated `CrdtOpWire`: `message(CrdtOpWireSchema)` validates the required oneof after the op-log owner extracts slot-seven bytes, while this engine learns no CRDT arm roster.
- Law: `_suite` keys transcribe declared message names verbatim; a family owning no descriptor source rides a `codec` arm.
- Law: `_suite` and `registry` hold estate-declared families alone; a foreign publisher's descriptor reaches `frame`/`family` as an argument off its own generated module, so the vendored CloudEvents module never enters this registry.
- Law: `_suite` binds each family through the generated module its own `.proto` declares, with no barrel between — the `contracts/` folder is the one producer of every symbol it binds.
- Law: `framed` is the size-delimited stream frame — egress through `sizeDelimitedEncode`, ingress through a `sizeDelimitedPeek` fold that refuses a frame past the ingress ceiling BEFORE buffering it, and a partial tail at stream end refuses as a truncation; the Connect envelope never enters here because a Connect transport already frames its own streams.
- Law: the well-known bridges seat here once — `any` packs and unpacks against THIS registry, `struct` and `value` cross `Shape.Json` through the generated `Struct`/`Value` codecs — so a field the corpus declares as `Any`, `Struct`, or `Value` is read at one owner and never through a hand JSON walk.
- Packages: `@bufbuild/protobuf`; `@bufbuild/protobuf/wire`; `@bufbuild/protobuf/wkt`; `@bufbuild/protovalidate`; `effect`; `../value/schema.ts`; the generated `@rasm\/contracts/rasm/contracts/<family>/v1/<file>_pb` modules.

```typescript
import {
  create,
  createRegistry,
  type DescMessage,
  fromBinary,
  fromJson,
  isMessage,
  type JsonValue,
  type Message,
  type MessageShape,
  type MessageValidType,
  type Registry,
  toBinary,
  toJson,
  toJsonString,
} from "@bufbuild/protobuf"
import { pathToString } from "@bufbuild/protobuf/reflect"
import { sizeDelimitedEncode, sizeDelimitedPeek } from "@bufbuild/protobuf/wire"
import { type Any, anyIs, anyPack, anyUnpack, type Struct, StructSchema, type Value, ValueSchema } from "@bufbuild/protobuf/wkt"
import { createValidator, type Violation } from "@bufbuild/protovalidate"
import { Channel, Chunk } from "effect"
import { Shape } from "../value/schema.ts"
import * as appearanceMaterial from "@rasm\/contracts/rasm/contracts/appearance/material_pb"
import * as appearanceSet from "@rasm\/contracts/rasm/contracts/appearance/set_pb"
import * as graph from "@rasm\/contracts/rasm/contracts/element/graph_pb"
import * as fault from "@rasm\/contracts/rasm/contracts/fault/fault_pb"
import * as availability from "@rasm\/contracts/rasm/contracts/availability/availability_pb"
import * as benchmarkClaim from "@rasm\/contracts/rasm/contracts/benchmark/claim_pb"
import * as binding from "@rasm\/contracts/rasm/contracts/binding/status_pb"
import * as bindingWriteback from "@rasm\/contracts/rasm/contracts/binding/writeback_pb"
import * as capability from "@rasm\/contracts/rasm/contracts/capability/descriptor_pb"
import * as credential from "@rasm\/contracts/rasm/contracts/credential/public_pb"
import * as edit from "@rasm\/contracts/rasm/contracts/element/edit_pb"
import * as feature from "@rasm\/contracts/rasm/contracts/feature/verdict_pb"
import * as appuiCommands from "@rasm\/contracts/rasm/contracts/ui/commands_pb"
import * as appuiEvidence from "@rasm\/contracts/rasm/contracts/ui/evidence_pb"
import * as appuiSurface from "@rasm\/contracts/rasm/contracts/ui/surface_pb"
import * as bcf from "@rasm\/contracts/rasm/contracts/bcf/bcf_pb"
import * as bimDiff from "@rasm\/contracts/rasm/contracts/bim/diff_pb"
import {
  CloudEventBatchSchema,
  CloudEventSchema,
} from "@rasm\/contracts/io/cloudevents/v1/cloudevents_pb"

const _suite = {
  FaultDetail: fault.FaultDetailSchema,
  NodeWire: graph.NodeWireSchema,
  Set: appearanceSet.SetSchema,
  Material: appearanceMaterial.MaterialSchema,
  CommandAvailability: availability.CommandAvailabilitySchema,
  CredentialPublicWire: credential.CredentialPublicWireSchema,
  FlagVerdictWire: feature.FlagVerdictWireSchema,
  BindingStatus: binding.BindingStatusSchema,
  CoercedValueWire: bindingWriteback.CoercedValueWireSchema,
  WriteOutcomeWire: bindingWriteback.WriteOutcomeWireSchema,
  CommandGateWire: appuiCommands.CommandGateWireSchema,
  AppUiSurfaceProgram: appuiSurface.AppUiSurfaceProgramSchema,
  EvidenceTimelineWire: appuiEvidence.EvidenceTimelineWireSchema,
  BcfTopicWire: bcf.BcfTopicWireSchema,
  BcfViewpointWire: bcf.BcfViewpointWireSchema,
  ModelDiffWire: bimDiff.ModelDiffWireSchema,
  BenchmarkClaimWire: benchmarkClaim.BenchmarkClaimWireSchema,
  DescriptorPinWire: capability.DescriptorPinWireSchema,
  EntityEditWire: edit.EntityEditWireSchema,
} as const

const _names = Record.keys(_suite)

const _registry: Registry = createRegistry(...Record.values(_suite))

const _READ = { readUnknownFields: true, recursionLimit: 24 } as const
const _JSON_READ = { ignoreUnknownFields: true, recursionLimit: 24, registry: _registry } as const
const _WRITE = { writeUnknownFields: true } as const
const _JSON_WRITE = { registry: _registry } as const

const _validator = createValidator({ registry: _registry })

const _issues = (violations: ReadonlyArray<Violation>): ReadonlyArray<Schema.FilterIssue> =>
  Array.map(violations, (violation) => ({
    path: [pathToString(violation.field)],
    message: `<${violation.ruleId}> ${violation.message}`,
  }))

const _admittedMessage = <Desc extends DescMessage>(gen: Desc, message: MessageShape<Desc>, ast: SchemaAST.AST) => {
  const verdict = _validator.validate(gen, message)
  return verdict.kind === "valid"
    ? ParseResult.succeed(verdict.message)
    : verdict.kind === "invalid"
      ? ParseResult.fail(new ParseResult.Type(
          ast,
          message,
          Array.map(_issues(verdict.violations), (issue) => `${issue.path.join(".")} ${issue.message}`).join("; "),
        ))
      : ParseResult.fail(new ParseResult.Forbidden(ast, message, `<${verdict.error.name}> ${verdict.error.message}`))
}

const _message = <Desc extends DescMessage>(gen: Desc): Schema.Schema<MessageValidType<Desc>, MessageShape<Desc>> =>
  Schema.transformOrFail(
    Schema.declare((input: unknown): input is MessageShape<Desc> => isMessage(input, gen), { identifier: gen.typeName }),
    Schema.declare((input: unknown): input is MessageValidType<Desc> => isMessage(input, gen), {
      identifier: `${gen.typeName}Valid`,
    }),
    {
      strict: true,
      decode: (message, _options, ast) => _admittedMessage(gen, message, ast),
      encode: (message, _options, ast) => _admittedMessage(gen, message, ast),
    },
  )

const _JsonValue: Schema.Schema<JsonValue> = Schema.declare(
  (input: unknown): input is JsonValue => Schema.is(Shape.Json)(input),
  { identifier: "JsonValue" },
)

const _jsonMessage = <Desc extends DescMessage>(gen: Desc): Schema.Schema<MessageValidType<Desc>, JsonValue> =>
  Schema.transformOrFail(_JsonValue, _message(gen), {
    strict: true,
    decode: (json, _options, ast) =>
      Either.try({ try: () => fromJson(gen, json, _JSON_READ), catch: (defect) => new ParseResult.Type(ast, json, String(defect)) }),
    encode: (message, _options, ast) =>
      Either.try({ try: () => toJson(gen, message, _JSON_WRITE), catch: (defect) => new ParseResult.Type(ast, message, String(defect)) }),
  })

const _framings = {
  binary: <Desc extends DescMessage>(gen: Desc): Schema.Schema<MessageValidType<Desc>, Uint8Array> =>
    _lifted(
      (octets) => fromBinary(gen, octets, _READ),
      (value) => (isMessage(value, gen) ? toBinary(gen, value, _WRITE) : undefined),
    ).pipe(Schema.compose(_message(gen), { strict: false })),
  json: <Desc extends DescMessage>(gen: Desc): Schema.Schema<MessageValidType<Desc>, Uint8Array> =>
    Json.schema(_jsonMessage(gen)),
} as const

const _framingKinds = Record.keys(_framings)

const _frame = <Desc extends DescMessage>(
  gen: Desc,
  framing: Proto.Framing = "binary",
): Schema.Schema<MessageValidType<Desc>, Uint8Array> =>
  _framings[framing](gen)

// --- [STREAM_FRAME]

const _pack = <Desc extends DescMessage, IE = never, Done = unknown>(
  gen: Desc,
): Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<MessageValidType<Desc>>, IE, IE, Done, Done> =>
  Channel.suspend(() => {
    const loop: Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<MessageValidType<Desc>>, IE, IE, Done, Done> =
      Channel.readWithCause({
        onInput: (input: Chunk.Chunk<MessageValidType<Desc>>) =>
          Channel.zipRight(Channel.write(Chunk.map(input, (message) => sizeDelimitedEncode(gen, message, _WRITE))), loop),
        onFailure: Channel.failCause,
        onDone: Channel.succeed,
      })
    return loop
  })

const _joined = (held: Uint8Array, arriving: Chunk.Chunk<Uint8Array>): Uint8Array => {
  const extent = held.byteLength + Chunk.reduce(arriving, 0, (total, part) => total + part.byteLength)
  const joined = new Uint8Array(extent)
  joined.set(held, 0)
  Chunk.reduce(arriving, held.byteLength, (offset, part) => {
    joined.set(part, offset)
    return offset + part.byteLength
  })
  return joined
}

const _split = <Desc extends DescMessage>(
  gen: Desc,
  ceiling: number,
  bytes: Uint8Array,
  landed: Chunk.Chunk<MessageValidType<Desc>>,
): Either.Either<readonly [Chunk.Chunk<MessageValidType<Desc>>, Uint8Array], ParseResult.ParseIssue> => {
  let rest = bytes
  let messages = landed
  while (true) {
    const head = sizeDelimitedPeek(rest)
    if (head.eof) return Either.right([messages, rest] as const)
    if (head.size > ceiling) {
      return Either.left(new ParseResult.Type(Schema.Uint8ArrayFromSelf.ast, rest, `<frame-overrun:${head.size}>${ceiling}`))
    }
    const end = head.offset + head.size
    if (rest.byteLength < end) return Either.right([messages, rest] as const)
    const decoded = Either.try({
      try: () => fromBinary(gen, rest.subarray(head.offset, end), _READ),
      catch: (defect) => new ParseResult.Type(Schema.Uint8ArrayFromSelf.ast, rest, String(defect)),
    })
    if (Either.isLeft(decoded)) return decoded
    const admitted = Schema.decodeUnknownEither(_message(gen))(decoded.right)
    if (Either.isLeft(admitted)) return Either.left(admitted.left.issue)
    messages = Chunk.append(messages, admitted.right)
    rest = rest.subarray(end)
  }
}

const _unpack = <Desc extends DescMessage, IE = never, Done = unknown>(
  gen: Desc,
  ceiling: number,
): Channel.Channel<Chunk.Chunk<MessageValidType<Desc>>, Chunk.Chunk<Uint8Array>, IE | ParseResult.ParseError, IE, Done, Done> =>
  Channel.suspend(() => {
    const loop = (
      held: Uint8Array,
    ): Channel.Channel<Chunk.Chunk<MessageValidType<Desc>>, Chunk.Chunk<Uint8Array>, IE | ParseResult.ParseError, IE, Done, Done> =>
      Channel.readWithCause({
        onInput: (input: Chunk.Chunk<Uint8Array>) =>
          Either.match(_split(gen, ceiling, _joined(held, input), Chunk.empty()), {
            onLeft: (issue) => Channel.fail(new ParseResult.ParseError({ issue })),
            onRight: ([messages, rest]) => Channel.zipRight(Channel.write(messages), loop(rest)),
          }),
        onFailure: Channel.failCause,
        onDone: (done) =>
          held.byteLength === 0
            ? Channel.succeed(done)
            : Channel.fail(new ParseResult.ParseError({ issue: new ParseResult.Type(Schema.Uint8ArrayFromSelf.ast, held, "<frame-truncated>") })),
      })
    return loop(new Uint8Array(0))
  })

const _framed = <Desc extends DescMessage>(gen: Desc, ceiling: number = Shape.Ingress.floor.bytes) =>
  <R, IE, OE, OutDone, InDone>(
    self: Channel.Channel<Chunk.Chunk<Uint8Array>, Chunk.Chunk<Uint8Array>, OE, IE, OutDone, InDone, R>,
  ): Channel.Channel<Chunk.Chunk<MessageValidType<Desc>>, Chunk.Chunk<MessageValidType<Desc>>, OE | ParseResult.ParseError, IE, OutDone, InDone, R> =>
    Channel.pipeTo(Channel.pipeTo(_pack<Desc, IE, InDone>(gen), self), _unpack<Desc, OE, OutDone>(gen, ceiling))

// --- [WELL_KNOWN]

function _unpackAny(any: Any): Option.Option<Message>
function _unpackAny<Desc extends DescMessage>(any: Any, gen: Desc): Option.Option<MessageShape<Desc>>
function _unpackAny<Desc extends DescMessage>(any: Any, gen?: Desc): Option.Option<Message> {
  return Option.fromNullable(gen === undefined ? anyUnpack(any, _registry) : anyUnpack(any, gen))
}

const _any = {
  pack: <Desc extends DescMessage>(gen: Desc, message: MessageShape<Desc>): Any => anyPack(gen, message),
  unpack: _unpackAny,
  is: (any: Any, gen: DescMessage): boolean => anyIs(any, gen),
} as const

const _StructJson: Schema.Schema<Struct, Shape.Json> = Schema.transformOrFail(Shape.Json, _message(StructSchema), {
  strict: true,
  decode: (json, _options, ast) =>
    Either.try({ try: () => fromJson(StructSchema, json, _JSON_READ), catch: (defect) => new ParseResult.Type(ast, json, String(defect)) }),
  encode: (struct, _options, ast) =>
    Either.try({ try: () => toJson(StructSchema, struct, _JSON_WRITE), catch: (defect) => new ParseResult.Type(ast, struct, String(defect)) }),
})

const _ValueJson: Schema.Schema<Value, Shape.Json> = Schema.transformOrFail(Shape.Json, _message(ValueSchema), {
  strict: true,
  decode: (json, _options, ast) =>
    Either.try({ try: () => fromJson(ValueSchema, json, _JSON_READ), catch: (defect) => new ParseResult.Type(ast, json, String(defect)) }),
  encode: (value, _options, ast) =>
    Either.try({ try: () => toJson(ValueSchema, value, _JSON_WRITE), catch: (defect) => new ParseResult.Type(ast, value, String(defect)) }),
})

declare namespace Proto {
  type Name = keyof typeof _suite
  type Framing = (typeof _framingKinds)[number]
  type Shape = Types.Simplify<{
    readonly names: typeof _names
    readonly suite: typeof _suite
    readonly registry: Registry
    readonly framings: typeof _framingKinds
    readonly read: typeof _READ
    readonly jsonRead: typeof _JSON_READ
    readonly write: typeof _WRITE
    readonly jsonWrite: typeof _JSON_WRITE
    readonly create: typeof create
    readonly message: typeof _message
    readonly json: typeof _jsonMessage
    readonly frame: typeof _frame
    readonly family: <Desc extends DescMessage, A, I>(gen: Desc, owned: Schema.Schema<A, I>, framing?: Framing) => Schema.Schema<A, Uint8Array>
    readonly framed: typeof _framed
    readonly any: typeof _any
    readonly struct: typeof _StructJson
    readonly value: typeof _ValueJson
  }>
  type _Rows<T extends Record<Name, DescMessage> = typeof _suite> = T
  type _Keys<K extends Name = Name> = K
}

const Proto: Proto.Shape = {
  names: _names,
  suite: _suite,
  registry: _registry,
  framings: _framingKinds,
  read: _READ,
  jsonRead: _JSON_READ,
  write: _WRITE,
  jsonWrite: _JSON_WRITE,
  create,
  message: _message,
  json: _jsonMessage,
  frame: _frame,
  family: (gen, owned, framing = "binary") => _frame(gen, framing).pipe(Schema.compose(owned, { strict: false })),
  framed: _framed,
  any: _any,
  struct: _StructJson,
  value: _ValueJson,
}
```

## [04]-[MSGPACK_ENGINE]

- Owner: `Format.msgpack` owns bounded complete-payload decode, encode, and the `Clock.Hlc` extension.
- Law: extension decode delegates to `Clock.Hlc.FromBytes`; int64 and uint64 tokens remain `bigint`, while owner schemas widen exact compact integers to `bigint`.
- Law: transport framing supplies complete payloads because package stream decoders accept incomplete EOF tails.
- Law: encoder key sorting stabilizes this arm's egress without claiming a cross-implementation canonical encoding.
- Law: `frame` decodes a payload with no owned schema, so a held frame renders where its family's schema already refused.
- Packages: `@msgpack/msgpack` (`Decoder`, `Encoder`, `ExtData`, `ExtensionCodec`); `effect` (`Schema`); `../value/clock.ts` (`Clock.Hlc`).

```typescript
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

const _packFrame: Schema.Schema<unknown, Uint8Array> =
  _lifted((octets) => _packDecoder.decode(octets), (value) => _packEncoder.encode(value))

const Pack: Pack.Shape = {
  frame: _packFrame,
  schema: (owned) => _packFrame.pipe(Schema.compose(owned, { strict: false })),
}
```

## [05]-[JSONPATCH_ENGINE]

- Owner: `Format.Patch` owns the closed RFC 6902 operation union and rooted immutable application.
- Law: paths reject prototype tokens, one structured clone isolates the input pair, and non-root ops delegate to `rfc6902`.
- Law: root removal returns `Option.none`; the EntityEdit members arm requires a present successor.

```typescript
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

## [06]-[JSON_ENGINE]

- Owner: `Format.json` owns bounded strict UTF-8 JSON composition and refused-octet rendering.
- Law: `Shape.Ingress.floor.bytes` admits raw octets before UTF-8 allocation.
- Law: structural decode and encode share `Schema.parseJson`; parity compares semantics, not member order.
- Law: transport adapters frame NDJSON and feed complete records.
- Packages: `effect` (`Schema`); `../value/schema.ts` (`Shape`).

```typescript
const _TEXT = { fatal: true } as const

const _strict = new TextDecoder("utf-8", _TEXT)
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

## [07]-[ARM_ROWS]

- Owner: `_armRows` carries every fact a consumer reads about an encoding without naming the encoding.
- Law: `fits` is the one sentence a reader selects an arm on; `admit` binds an owned schema and NAMES the one row it can lack.
- Law: `_armAbsences` is the arm plane's share of `[08]`'s capability vocabulary, so an arm's refusal and a format's refusal read from one closed set.
- Law: `selfDescribing` states whether a payload decodes with no owned schema; the proto arm alone needs a descriptor.
- Law: `render` prints a held frame for an operator and is total — a failed decode yields absence, never a throw.
- Law: `degrade` names what the arm gives up; no row leaves it blank and no row spells a capability there.
- Law: an arm decides nothing about tenancy or lifetime, and carries no column stating either.
- Growth: a new encoding is one arm row; a consumer selecting on an arm name reads a column that already exists.
- Boundary: `Wire` owns family-to-arm assignment and supplies the descriptor a proto render needs.

```typescript
const _arms = ["proto", "json", "msgpack"] as const

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
    readonly selfDescribing: boolean
    readonly render: (octets: Uint8Array, descriptor: Option.Option<DescMessage>) => Option.Option<string>
    readonly degrade: string
  }
  type _Rows<T extends { readonly [K in Kind]: Row } = typeof _armRows> = T
}

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
    admit: (owned, descriptor) =>
      Option.match(descriptor, {
        onNone: () => Either.left<Arm.Absent>("descriptor"),
        onSome: (gen) => Either.right(Proto.family(gen, owned)),
      }),
    selfDescribing: false,
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
    selfDescribing: true,
    render: (octets) => Option.some(Json.text(octets)),
    degrade: "<base64-inflated-octets>",
  },
  msgpack: {
    fits: "<compact-binary-payload-carrying-extension-cells-and-i64-magnitudes>",
    admit: (owned) => Either.right(Pack.schema(owned)),
    selfDescribing: true,
    render: (octets) => _decoded(Pack.frame)(octets),
    degrade: "<arm-local-key-sort-only>",
  },
} as const satisfies { readonly [K in Arm.Kind]: Arm.Row }

type _ArmKind = Arm.Kind
type _ArmRow = Arm.Row
type _ArmAbsent = Arm.Absent
```

## [08]-[EVENT_FORMAT]

- Owner: EventFormat publishes each format's raw structured single and optional batch codec; carrier's `Event.format` composes semantic admission before a binding can consume JSON or Protobuf.
- Law: JSON uses the shared bounded UTF-8 JSON engine; singular versus batch is a structural arity fact, and every decoded tree crosses strict SDK admission at `Event.format` rather than fabricating a transport message here.
- Law: Protobuf uses generated CloudEventSchema and CloudEventBatchSchema through the validated proto frame and never admits either descriptor to Proto.registry.
- Law: Avro accepts one lane-owned schema compiled from the frozen publisher asset and has no batch member.
- Law: binary content mode belongs to a transport binding, not an event-format codec; no demand set or caller-selected descriptor exists here.
- Law: `framed` parses the RFC media type once, compares its case-normalized type/subtype identity exactly after parameters are removed, reads batch before single, and returns the row-derived `Single | Batch` frame without trial decoding. Arity is the frame's closed discriminant; no consumer can pair a format with a boolean that admits the producerless Avro-batch state.
- Law: `EventFormat.Media` is the one precise media grammar used by addressed attributes and binding detection; nonempty text is not media evidence.
- Tests: JSON single/batch cover `data` and `data_base64`; Protobuf single/batch round-trip semantically across the carrier transform; publisher descriptors work directly while remaining absent from Proto.registry; Avro has no batch.
- Boundary: JSON lands event trees and Protobuf lands generated wire messages; these raw codecs are inner engines, while carrier's `Event.format` is the public admitted event-wire surface.
- Packages: `cloudevents` (`CloudEventV1`, `CONSTANTS`); `effect`; generated CloudEvents descriptors.

```typescript
import { CONSTANTS, type CloudEventV1 } from "cloudevents"

const _EventTree = Schema.declare(
  (input: unknown): input is CloudEventV1<unknown> => Predicate.isRecord(input),
  { identifier: "CloudEventJsonObject" },
)
const _jsonSingle = Json.schema(_EventTree)
const _jsonBatch = Json.schema(Schema.Array(_EventTree))

const _jsonEvent = {
  media: CONSTANTS.MIME_CE_JSON,
  single: _jsonSingle,
  batch: Option.some({ media: CONSTANTS.MIME_CE_BATCH, codec: _jsonBatch }),
} as const

const _protobufEvent = {
  media: "application/cloudevents+protobuf",
  single: _frame(CloudEventSchema),
  batch: Option.some({
    media: "application/cloudevents-batch+protobuf",
    codec: _frame(CloudEventBatchSchema),
  }),
  schemas: { single: CloudEventSchema, batch: CloudEventBatchSchema },
} as const

const _avroEvent = {
  media: "application/cloudevents+avro",
  batch: Option.none<never>(),
  bind: <A>(single: Schema.Schema<A, Uint8Array>) => ({
    media: "application/cloudevents+avro",
    single,
    batch: Option.none<never>(),
  } as const),
} as const

const _eventRows = {
  avro: _avroEvent,
  json: _jsonEvent,
  protobuf: _protobufEvent,
} as const
const _eventFormats = Record.keys(_eventRows)

const _MEDIA_TOKEN = "[!#$%&'*+.^_`|~0-9A-Za-z-]+"
const _MEDIA = new RegExp(
  `^(${_MEDIA_TOKEN})/(${_MEDIA_TOKEN})(?:[ \\t]*;[ \\t]*${_MEDIA_TOKEN}[ \\t]*=[ \\t]*(?:${_MEDIA_TOKEN}|\"(?:[^\"\\\\\\r\\n]|\\\\[\\t -~])*\"))*[ \\t]*$`,
)
const _media = (offered: string): Option.Option<string> =>
  Option.flatMap(Option.fromNullable(_MEDIA.exec(offered)), (matched) => {
    const type = matched[1]
    const subtype = matched[2]
    return type === undefined || subtype === undefined
      ? Option.none()
      : Option.some(`${type.toLowerCase()}/${subtype.toLowerCase()}`)
  })
const _EventMedia = Schema.String.pipe(
  Schema.filter((offered) => Option.isSome(_media(offered)) || "<invalid-media-type>"),
)

declare namespace EventFormat {
  type Kind = keyof typeof _eventRows
  type BatchKind = {
    readonly [K in Kind]: (typeof _eventRows)[K]["batch"] extends Option.Some<unknown> ? K : never
  }[Kind]
  type Single = { readonly [K in Kind]: { readonly _tag: "Single"; readonly format: K } }[Kind]
  type Batch = { readonly [K in BatchKind]: { readonly _tag: "Batch"; readonly format: K } }[BatchKind]
  type Frame =
    | Single
    | Batch
  type Avro = ReturnType<typeof _avroEvent.bind>
  type Json = typeof _jsonEvent
  type Protobuf = typeof _protobufEvent
}

const _eventBatchFormats = Array.filter(
  _eventFormats,
  (format): format is EventFormat.BatchKind => Option.isSome(_eventRows[format].batch),
)

const _eventFramed = (contentType: string): Option.Option<EventFormat.Frame> =>
  Option.flatMap(_media(contentType), (media) =>
    Option.orElse(
      Option.map(
        Array.findFirst(_eventBatchFormats, (format) =>
          Option.exists(_eventRows[format].batch, (batch) => media === batch.media)),
        (format) => ({ _tag: "Batch", format } as const),
      ),
      () =>
        Option.map(
          Array.findFirst(_eventFormats, (format) => media === _eventRows[format].media),
          (format) => ({ _tag: "Single", format } as const),
        ),
    ))

const EventFormat = {
  Media: _EventMedia,
  formats: _eventFormats,
  rows: _eventRows,
  framed: _eventFramed,
  json: _jsonEvent,
  protobuf: _protobufEvent,
  avro: _avroEvent,
} as const
// --- [EXPORTS] -------------------------------------------------------------------------

declare namespace Format {
  type Arm = _ArmKind
  namespace Arm {
    type Row = _ArmRow
    type Absent = _ArmAbsent
  }
  type Event = EventFormat.Kind
  namespace Event {
    type Frame = EventFormat.Frame
    type Avro = EventFormat.Avro
    type Json = EventFormat.Json
    type Protobuf = EventFormat.Protobuf
  }
  type Shape = {
    readonly arms: typeof _arms
    readonly rows: { readonly arm: typeof _armRows; readonly event: typeof _eventRows }
    readonly proto: typeof Proto
    readonly msgpack: typeof Pack
    readonly Patch: typeof Patch
    readonly json: typeof Json
    readonly event: typeof EventFormat
  }
}

const Format: Format.Shape = {
  arms: _arms,
  rows: { arm: _armRows, event: _eventRows },
  proto: Proto,
  msgpack: Pack,
  Patch,
  json: Json,
  event: EventFormat,
}

export { Format }
```

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
