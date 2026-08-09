# [CORE_CONTRACT]

`core/src/interchange/contract.ts` compares pinned and shipped contracts, grades binary, JSON, and source compatibility, and exposes one typed gate. `Contract.Drift` derives every verdict; `Contract.Descriptor` refreshes the declared census.

## [01]-[INDEX]

- [02]-[DRIFT_VERDICT]: evidence, compatibility grades, and the derived receipt; `Contract.Drift`.
- [03]-[GENERATION_DIFF]: protobuf descriptor and canonical capability-document walks.
- [04]-[GATE_SERVICE]: declared census order, typed admission, and refresh policy; `Contract.Descriptor`.

## [02]-[DRIFT_VERDICT]

- Owner: `Contract.Drift` stores one evidence set and the derived verdict for each compatibility axis.
- Law: the class filter re-derives every stored verdict, so decoded receipts cannot detach claims from evidence.
- Law: `identical` and `compatible` admit; `breaking` returns `Contract.Refusal` through `Contract.Gate<A>`.
- Law: `_dominant` folds each axis independently; an empty evidence set is `identical` on every axis.
- Law: removals, renames, JSON-name changes, option changes, wire facts, and capability-row changes remain distinct evidence.
- Law: `_grade` is conservative: option changes break every axis, and enum changes break JSON and source compatibility.
- Growth: one change case requires one complete `_grade` row and one detecting lane.
- Boundary: the consuming wire owner maps `Contract.Refusal` into its fault vocabulary; this page never imports the codec.
- Packages: `effect` (`Array`, `Effect`, `Order`, `Schema`).

```typescript signature
import {
  createFileRegistry,
  type DescEnum,
  type DescField,
  type DescMessage,
  type DescMethod,
  type DescService,
  equals,
  type MessageShape,
  qualifiedName,
  type Registry,
  ScalarType,
} from "@bufbuild/protobuf"
import { FeatureSet_FieldPresence, FieldOptionsSchema, FileDescriptorSetSchema } from "@bufbuild/protobuf/wkt"
import {
  Array,
  Duration,
  Effect,
  HashMap,
  HashSet,
  Layer,
  Match,
  Option,
  Order,
  type ParseResult,
  Reloadable,
  Schedule,
  Schema,
} from "effect"
import { Digest } from "../value/contentKey.ts"
import { Shape } from "../value/schema.ts"
import { Format } from "./format.ts"

const _verdicts = ["identical", "compatible", "breaking"] as const
const _compatibilities = ["binary", "json", "source"] as const
const _capabilityFamily = "CapabilityDescriptorWire" as const
// Message shape is HALF the contract a client binds: `createClient` derives every member from the service's method
// roster, so a removed method, a flipped streaming kind, or a swapped request type breaks a caller no message diff
// can see. Services ride one custom-source family beside the capability document, coordinates carrying the detail.
const _serviceFamily = "ServiceSurfaceWire" as const
const _families = [...Format.proto.names, _capabilityFamily, _serviceFamily] as const
const _Family = Schema.Literal(..._families)

const _severity = {
  identical: { rank: 0, admitted: true, alarm: false },
  compatible: { rank: 1, admitted: true, alarm: false },
  breaking: { rank: 2, admitted: false, alarm: true },
} as const

const _grade = {
  FieldAdded: { binary: "compatible", json: "breaking", source: "compatible" },
  FieldRenamed: { binary: "compatible", json: "compatible", source: "breaking" },
  JsonNameChanged: { binary: "compatible", json: "breaking", source: "compatible" },
  EnumValueAdded: { binary: "compatible", json: "breaking", source: "breaking" },
  EnumValueRenamed: { binary: "compatible", json: "breaking", source: "breaking" },
  DescriptorAdded: { binary: "compatible", json: "compatible", source: "compatible" },
  OptionChanged: { binary: "breaking", json: "breaking", source: "breaking" },
  EnumValueRemoved: { binary: "compatible", json: "breaking", source: "breaking" },
  FieldRemoved: { binary: "compatible", json: "breaking", source: "breaking" },
  DescriptorRemoved: { binary: "breaking", json: "breaking", source: "breaking" },
  DescriptorChanged: { binary: "breaking", json: "breaking", source: "breaking" },
  DescriptorDocumentChanged: { binary: "breaking", json: "breaking", source: "breaking" },
  DescriptorAddressChanged: { binary: "breaking", json: "breaking", source: "breaking" },
  OneofChanged: { binary: "breaking", json: "breaking", source: "breaking" },
  TypeChanged: { binary: "breaking", json: "breaking", source: "breaking" },
  WireTypeChanged: { binary: "breaking", json: "breaking", source: "breaking" },
  NumberReused: { binary: "breaking", json: "breaking", source: "breaking" },
  FamilyMissing: { binary: "breaking", json: "breaking", source: "breaking" },
  PresenceChanged: { binary: "compatible", json: "breaking", source: "breaking" },
  RequiredFieldAdded: { binary: "breaking", json: "breaking", source: "breaking" },
  Utf8ValidationChanged: { binary: "breaking", json: "breaking", source: "breaking" },
  MethodAdded: { binary: "compatible", json: "compatible", source: "compatible" },
  MethodRemoved: { binary: "breaking", json: "breaking", source: "breaking" },
  MethodKindChanged: { binary: "breaking", json: "breaking", source: "breaking" },
  MethodSignatureChanged: { binary: "breaking", json: "breaking", source: "breaking" },
  // Losing a no-side-effects declaration leaves the POST route intact and retires the cacheable GET beside it, so the
  // wire holds on both encodings and a caller bound to that route is the one that must change.
  MethodIdempotencyChanged: { binary: "compatible", json: "compatible", source: "breaking" },
  ServiceRemoved: { binary: "breaking", json: "breaking", source: "breaking" },
} as const

const _FieldCoord = Schema.Struct({
  message: Schema.NonEmptyString,
  field: Schema.NonEmptyString,
  number: Schema.Int.pipe(Schema.positive()),
})

const _EnumCoord = Schema.Struct({
  enum: Schema.NonEmptyString,
  value: Schema.NonEmptyString,
  number: Schema.Int,
})

const _DescriptorCoord = Schema.Struct({ descriptor: Schema.NonEmptyString })
const _ServiceCoord = Schema.Struct({ service: Schema.NonEmptyString })
const _MethodCoord = Schema.Struct({ service: Schema.NonEmptyString, method: Schema.NonEmptyString })
const _methodSides = ["input", "output"] as const
const _methodKinds = ["unary", "server_streaming", "client_streaming", "bidi_streaming"] as const
const _descriptorFields = ["surface", "effect", "idempotency", "scope", "units", "input", "output"] as const
const _capabilityEffects = ["pure", "read", "write", "external", "irreversible"] as const
const _capabilityIdempotency = ["idempotent", "keyed", "single-shot", "non-idempotent"] as const
const _capabilityUnits = ["cpu-millis", "wall-millis", "bytes-egress", "model-tokens", "calls"] as const

const _Change = Schema.Union(
  Schema.TaggedStruct("FieldAdded", { at: _FieldCoord }),
  Schema.TaggedStruct("FieldRenamed", { at: _FieldCoord, from: Schema.NonEmptyString, to: Schema.NonEmptyString }),
  Schema.TaggedStruct("JsonNameChanged", { at: _FieldCoord, from: Schema.NonEmptyString, to: Schema.NonEmptyString }),
  Schema.TaggedStruct("EnumValueAdded", { at: _EnumCoord }),
  Schema.TaggedStruct("EnumValueRenamed", { at: _EnumCoord, from: Schema.NonEmptyString, to: Schema.NonEmptyString }),
  Schema.TaggedStruct("DescriptorAdded", { at: _DescriptorCoord }),
  Schema.TaggedStruct("OptionChanged", { at: _FieldCoord }),
  Schema.TaggedStruct("EnumValueRemoved", { at: _EnumCoord }),
  Schema.TaggedStruct("FieldRemoved", { at: _FieldCoord }),
  Schema.TaggedStruct("DescriptorRemoved", { at: _DescriptorCoord }),
  Schema.TaggedStruct("DescriptorChanged", { at: _DescriptorCoord, field: Schema.Literal(..._descriptorFields) }),
  Schema.TaggedStruct("DescriptorDocumentChanged", {}),
  Schema.TaggedStruct("DescriptorAddressChanged", {}),
  Schema.TaggedStruct("OneofChanged", {
    at: _FieldCoord,
    from: Schema.Option(Schema.NonEmptyString),
    to: Schema.Option(Schema.NonEmptyString),
  }),
  Schema.TaggedStruct("TypeChanged", { at: _FieldCoord, from: Schema.NonEmptyString, to: Schema.NonEmptyString }),
  Schema.TaggedStruct("WireTypeChanged", { at: _FieldCoord, from: Schema.NonEmptyString, to: Schema.NonEmptyString }),
  Schema.TaggedStruct("NumberReused", { at: _FieldCoord, retired: Schema.NonEmptyString }),
  Schema.TaggedStruct("FamilyMissing", { family: _Family }),
  Schema.TaggedStruct("PresenceChanged", { at: _FieldCoord, from: Schema.Int, to: Schema.Int }),
  Schema.TaggedStruct("RequiredFieldAdded", { at: _FieldCoord }),
  Schema.TaggedStruct("Utf8ValidationChanged", { at: _FieldCoord, from: Schema.Boolean, to: Schema.Boolean }),
  Schema.TaggedStruct("MethodAdded", { at: _MethodCoord }),
  Schema.TaggedStruct("MethodRemoved", { at: _MethodCoord }),
  Schema.TaggedStruct("MethodKindChanged", {
    at: _MethodCoord,
    from: Schema.Literal(..._methodKinds),
    to: Schema.Literal(..._methodKinds),
  }),
  Schema.TaggedStruct("MethodSignatureChanged", {
    at: _MethodCoord,
    side: Schema.Literal(..._methodSides),
    from: Schema.NonEmptyString,
    to: Schema.NonEmptyString,
  }),
  Schema.TaggedStruct("MethodIdempotencyChanged", { at: _MethodCoord, from: Schema.Int, to: Schema.Int }),
  Schema.TaggedStruct("ServiceRemoved", { at: _ServiceCoord }),
)

const _rank: Order.Order<ContractDrift.Verdict> = Order.mapInput(
  Order.number,
  (verdict: ContractDrift.Verdict) => _severity[verdict].rank,
)

const _graded = (
  change: ContractDrift.Change,
  compatibility: ContractDrift.Compatibility,
): ContractDrift.Verdict => _grade[change._tag][compatibility]

const _dominant = (
  changes: ReadonlyArray<ContractDrift.Change>,
  compatibility: ContractDrift.Compatibility,
): ContractDrift.Verdict =>
  Array.match(changes, {
    onEmpty: (): ContractDrift.Verdict => "identical",
    onNonEmpty: (present) => Array.max(Array.map(present, (change) => _graded(change, compatibility)), _rank),
  })

const _settled = (changes: ReadonlyArray<ContractDrift.Change>): ContractDrift.Verdicts => ({
  binary: _dominant(changes, "binary"),
  json: _dominant(changes, "json"),
  source: _dominant(changes, "source"),
})

class ContractDrift extends Schema.Class<ContractDrift>("ContractDrift")(
  Schema.Struct({
    family: _Family,
    verdicts: Schema.Struct({
      binary: Schema.Literal(..._verdicts),
      json: Schema.Literal(..._verdicts),
      source: Schema.Literal(..._verdicts),
    }),
    pinned: Schema.NonEmptyString,
    live: Schema.NonEmptyString,
    changes: Schema.Array(_Change),
  }).pipe(
    Schema.filter(
      (receipt) =>
        Array.every(
          _compatibilities,
          (compatibility) => receipt.verdicts[compatibility] === _dominant(receipt.changes, compatibility),
        ) || "<verdict-detached-from-changes>",
      { identifier: "VerdictDerived" },
    ),
  ),
) {
  static readonly Change: typeof _Change = _Change
  static readonly dominant: (
    changes: ReadonlyArray<ContractDrift.Change>,
    compatibility: ContractDrift.Compatibility,
  ) => ContractDrift.Verdict = _dominant
  static readonly graded: (
    change: ContractDrift.Change,
    compatibility: ContractDrift.Compatibility,
  ) => ContractDrift.Verdict = _graded
  static readonly rank: Order.Order<ContractDrift.Verdict> = _rank
  static readonly of = (
    family: ContractDrift.Family,
    pinned: string,
    live: string,
    changes: ReadonlyArray<ContractDrift.Change>,
  ): ContractDrift => ContractDrift.make({ family, verdicts: _settled(changes), pinned, live, changes })
  verdict(compatibility: ContractDrift.Compatibility): ContractDrift.Verdict {
    return this.verdicts[compatibility]
  }
  admitted(compatibility: ContractDrift.Compatibility): boolean {
    return _severity[this.verdict(compatibility)].admitted
  }
  alarm(compatibility: ContractDrift.Compatibility): boolean {
    return _severity[this.verdict(compatibility)].alarm
  }
}

declare namespace ContractDrift {
  type Family = (typeof _families)[number]
  type Verdict = keyof typeof _severity
  type Compatibility = (typeof _compatibilities)[number]
  type Verdicts = { readonly [Compatibility in ContractDrift.Compatibility]: Verdict }
  type Change = Schema.Schema.Type<typeof _Change>
  type _Rows<T extends Record<(typeof _verdicts)[number], { readonly rank: number; readonly admitted: boolean; readonly alarm: boolean }> = typeof _severity> = T
  type _Grades<T extends Record<Change["_tag"], Verdicts> = typeof _grade> = T
  type _Keys<K extends (typeof _verdicts)[number] = Verdict> = K
  type _GradeKeys<K extends Change["_tag"] = keyof typeof _grade> = K
}

class ContractRefusal extends Schema.TaggedError<ContractRefusal>()("ContractRefusal", {
  family: _Family,
  compatibility: Schema.Literal(..._compatibilities),
  verdict: Schema.Literal(..._verdicts),
  changes: Schema.Int.pipe(Schema.nonNegative()),
}) {}

class ContractFault extends Schema.TaggedError<ContractFault>()("Contract.Fault", {
  detail: Schema.String,
}) {
  static readonly from = (cause: unknown): ContractFault =>
    new ContractFault({ detail: cause instanceof Error ? cause.message : String(cause) })
}

const _Cadence = Schema.DurationFromSelf.pipe(
  Schema.filter(
    (cadence) => Duration.isFinite(cadence) && Duration.greaterThan(cadence, Duration.zero),
    { identifier: "PositiveFiniteCadence" },
  ),
)

class ContractRefresh extends Schema.Class<ContractRefresh>("Contract.Refresh")({ cadence: _Cadence }) {
  readonly schedule = Schedule.spaced(this.cadence)
}

class ContractPin extends Schema.Class<ContractPin>("Contract.Pin")({
  document: Schema.NonEmptyString,
  digest: Digest.codecs.content.wire,
  descriptors: Shape.Refined.OrdinalKey,
}) {}

const _Request = Schema.Struct({
  family: _Family,
  compatibility: Schema.Literal(..._compatibilities),
})

```

## [03]-[GENERATION_DIFF]

- Owner: `_paired` is the keyed roster fold; `_diffed`, `_enumChanges`, and `_descriptorChanges` supply typed arms.
- Law: fields pair by number; stable-number name and JSON-name changes emit independent evidence.
- Law: a changed leaf emits `TypeChanged` or `NumberReused`; an unchanged leaf folds every comparison lane.
- Law: field names affect source compatibility, and `jsonName` affects JSON compatibility; neither changes protobuf wire identity.
- Law: typed `Option` reads guard descriptor options, messages, enums, and registry lookups.
- Law: enum values pair by number; stable-number renames emit `EnumValueRenamed`.
- Law: nested messages recurse behind a qualified-name visited set; recursive descriptors terminate.
- Law: capability rows pair by descriptor; the fixed producer fields remain separate evidence coordinates.
- Law: methods pair by name, because protobuf assigns no method number; a rename reads as a removal beside an addition.
- Law: method kind, both signature sides, and the idempotency declaration are independent evidence under one coordinate.
- Law: an unresolved pinned service reads `ServiceRemoved`; a shipped-only service stays invisible and gates nothing.
- Law: each capability document must match its declared row count and carry strictly ordinal descriptor and unit keys.
- Packages: `@bufbuild/protobuf`, `@bufbuild/protobuf/wkt`, and `effect` descriptor, schema, and collection owners.

```typescript signature
const _leaf = (field: DescField): string =>
  Match.value(field).pipe(
    Match.discriminatorsExhaustive("fieldKind")({
      scalar: (arm) => `scalar:${ScalarType[arm.scalar]}`,
      enum: (arm) => `enum:${qualifiedName(arm.enum)}`,
      message: (arm) => `message:${qualifiedName(arm.message)}`,
      list: (arm) =>
        arm.listKind === "scalar"
          ? `list:scalar:${ScalarType[arm.scalar]}`
          : arm.listKind === "enum"
            ? `list:enum:${qualifiedName(arm.enum)}`
            : `list:message:${qualifiedName(arm.message)}`,
      map: (arm) =>
        arm.mapKind === "scalar"
          ? `map:${ScalarType[arm.mapKey]}:${ScalarType[arm.scalar]}`
          : arm.mapKind === "enum"
            ? `map:${ScalarType[arm.mapKey]}:${qualifiedName(arm.enum)}`
            : `map:${ScalarType[arm.mapKey]}:${qualifiedName(arm.message)}`,
    }),
  )

const _wireFacts = (field: DescField): string =>
  `delimited:${field.delimitedEncoding === true}|packed:${field.packed === true}`

const _coord = (message: DescMessage, field: DescField): typeof _FieldCoord.Type =>
  _FieldCoord.make({ message: qualifiedName(message), field: field.name, number: field.number })

const _enumOf = (field: DescField): Option.Option<DescEnum> =>
  field.fieldKind === "enum" || (field.fieldKind === "list" && field.listKind === "enum")
    || (field.fieldKind === "map" && field.mapKind === "enum")
    ? Option.some(field.enum)
    : Option.none()

const _messageOf = (field: DescField): Option.Option<DescMessage> =>
  field.fieldKind === "message" || (field.fieldKind === "list" && field.listKind === "message")
    || (field.fieldKind === "map" && field.mapKind === "message")
    ? Option.some(field.message)
    : Option.none()

const _optionsOf = (field: DescField): Option.Option<MessageShape<typeof FieldOptionsSchema>> =>
  Option.fromNullable(field.proto.options)

const _optionsAlike = (was: DescField, is: DescField): boolean =>
  Option.match(_optionsOf(was), {
    onNone: () => Option.isNone(_optionsOf(is)),
    onSome: (before) =>
      Option.match(_optionsOf(is), {
        onNone: () => false,
        onSome: (after) => equals(FieldOptionsSchema, before, after),
      }),
  })

const _strictlyOrdered = (values: ReadonlyArray<string>): boolean =>
  Array.every(Array.zip(values, Array.drop(values, 1)), ([was, is]) => was < is)

const _DescriptorRow = Schema.Struct({
  descriptor: Schema.NonEmptyString,
  surface: Schema.NonEmptyString,
  effect: Schema.Literal(..._capabilityEffects),
  idempotency: Schema.Literal(..._capabilityIdempotency),
  scope: Schema.NonEmptyString,
  units: Schema.Array(Schema.Literal(..._capabilityUnits)).pipe(
    Schema.filter((units) => _strictlyOrdered(units) || "<capability-units-order>"),
  ),
  input: Schema.Unknown,
  output: Schema.Unknown,
})

const _paired = <A>(
  before: ReadonlyArray<A>,
  after: ReadonlyArray<A>,
  key: (row: A) => string | number,
  arms: {
    readonly added: (row: A) => ReadonlyArray<ContractDrift.Change>
    readonly removed: (row: A) => ReadonlyArray<ContractDrift.Change>
    readonly shared: (was: A, is: A) => ReadonlyArray<ContractDrift.Change>
  },
): ReadonlyArray<ContractDrift.Change> => {
  const held = HashMap.fromIterable(Array.map(before, (row) => [key(row), row] as const))
  const landed = HashMap.fromIterable(Array.map(after, (row) => [key(row), row] as const))
  return Array.flatMap(
    Array.dedupe([...Array.map(before, key), ...Array.map(after, key)]),
    (at): ReadonlyArray<ContractDrift.Change> =>
      Option.match(HashMap.get(held, at), {
        onNone: () => Option.match(HashMap.get(landed, at), { onNone: () => [], onSome: arms.added }),
        onSome: (was) =>
          Option.match(HashMap.get(landed, at), { onNone: () => arms.removed(was), onSome: (is) => arms.shared(was, is) }),
      }),
  )
}

const _enumChanges = (pinned: DescEnum, live: DescEnum): ReadonlyArray<ContractDrift.Change> =>
  _paired(pinned.values, live.values, (value) => value.number, {
    added: (value) => [{
      _tag: "EnumValueAdded" as const,
      at: _EnumCoord.make({ enum: qualifiedName(live), value: value.name, number: value.number }),
    }],
    removed: (value) => [{
      _tag: "EnumValueRemoved" as const,
      at: _EnumCoord.make({ enum: qualifiedName(pinned), value: value.name, number: value.number }),
    }],
    shared: (was, is) =>
      was.name === is.name
        ? []
        : [{
            _tag: "EnumValueRenamed" as const,
            at: _EnumCoord.make({ enum: qualifiedName(live), value: is.name, number: is.number }),
            from: was.name,
            to: is.name,
          }],
  })

const _documentRows = (pin: ContractPin) =>
  Schema.Array(_DescriptorRow).pipe(
    Schema.filter(
      (rows) =>
        rows.length === pin.descriptors
        && _strictlyOrdered(Array.map(rows, (row) => row.descriptor))
        && Array.every(rows, (row) => _strictlyOrdered(row.units))
        || "<noncanonical-descriptor-pin>",
      { identifier: "CanonicalDescriptorPin" },
    ),
  )

const _decodePin = (octets: Uint8Array): Effect.Effect<ContractPin, ParseResult.ParseError> =>
  Schema.decodeUnknown(Format.json.schema(ContractPin))(octets)

const _decodeDocument = (
  pin: ContractPin,
): Effect.Effect<ReadonlyArray<typeof _DescriptorRow.Type>, ParseResult.ParseError> =>
  Schema.decodeUnknown(Format.json.schema(_documentRows(pin)))(new TextEncoder().encode(pin.document))

const _descriptorValues: ReadonlyArray<
  readonly [(typeof _descriptorFields)[number], (row: typeof _DescriptorRow.Type) => string]
> = [
  ["surface", (row) => row.surface],
  ["effect", (row) => row.effect],
  ["idempotency", (row) => row.idempotency],
  ["scope", (row) => row.scope],
  ["units", (row) => JSON.stringify(row.units)],
  ["input", (row) => JSON.stringify(row.input) ?? "<undefined>"],
  ["output", (row) => JSON.stringify(row.output) ?? "<undefined>"],
]

const _descriptorChanges = (
  pinned: ContractPin,
  live: ContractPin,
  before: ReadonlyArray<typeof _DescriptorRow.Type>,
  after: ReadonlyArray<typeof _DescriptorRow.Type>,
): ReadonlyArray<ContractDrift.Change> => {
  const rows = _paired(before, after, (row) => row.descriptor, {
    added: (row) => [{ _tag: "DescriptorAdded" as const, at: _DescriptorCoord.make({ descriptor: row.descriptor }) }],
    removed: (row) => [{ _tag: "DescriptorRemoved" as const, at: _DescriptorCoord.make({ descriptor: row.descriptor }) }],
    shared: (was, is) =>
      Array.filterMap(_descriptorValues, ([field, value]) =>
        value(was) === value(is)
          ? Option.none()
          : Option.some({ _tag: "DescriptorChanged" as const, at: _DescriptorCoord.make({ descriptor: is.descriptor }), field }),
      ),
  })
  return [
    ...rows,
    ...(pinned.document !== live.document && rows.length === 0 ? [{ _tag: "DescriptorDocumentChanged" as const }] : []),
    ...((pinned.document === live.document) === (pinned.digest === live.digest)
      ? []
      : [{ _tag: "DescriptorAddressChanged" as const }]),
  ]
}

// Methods pair by NAME, because protobuf assigns no method number — a rename is therefore a removal beside an
// addition, and this page mints no rename case it cannot prove. Request and response ride ONE side roster, so a
// swapped message reads as the same evidence under a different coordinate instead of two hand-written arms.
const _methodSideValues: ReadonlyArray<
  readonly [(typeof _methodSides)[number], (method: DescMethod) => DescMessage]
> = [
  ["input", (method) => method.input],
  ["output", (method) => method.output],
]

const _methodChanges = (
  service: string,
  pinned: DescService,
  live: DescService,
): ReadonlyArray<ContractDrift.Change> =>
  _paired(pinned.methods, live.methods, (method) => method.name, {
    added: (method) => [{ _tag: "MethodAdded" as const, at: _MethodCoord.make({ service, method: method.name }) }],
    removed: (method) => [{ _tag: "MethodRemoved" as const, at: _MethodCoord.make({ service, method: method.name }) }],
    shared: (was, is) => [
      ...(was.methodKind === is.methodKind
        ? []
        : [{
            _tag: "MethodKindChanged" as const,
            at: _MethodCoord.make({ service, method: is.name }),
            from: was.methodKind,
            to: is.methodKind,
          }]),
      ...Array.filterMap(_methodSideValues, ([side, read]) =>
        qualifiedName(read(was)) === qualifiedName(read(is))
          ? Option.none()
          : Option.some({
              _tag: "MethodSignatureChanged" as const,
              at: _MethodCoord.make({ service, method: is.name }),
              side,
              from: qualifiedName(read(was)),
              to: qualifiedName(read(is)),
            })),
      ...(was.idempotency === is.idempotency
        ? []
        : [{
            _tag: "MethodIdempotencyChanged" as const,
            at: _MethodCoord.make({ service, method: is.name }),
            from: was.idempotency,
            to: is.idempotency,
          }]),
    ],
  })

// Pinned services are the baseline roster, so a service the shipped registry cannot resolve reads `ServiceRemoved`
// exactly as an unresolved message family reads `FamilyMissing`. Services present only on the shipped side stay
// invisible here by construction — the census walks a pin, and an unpinned addition breaks no caller this page gates.
const _serviceChanges = (
  pinned: ReadonlyArray<DescService>,
  registry: Registry,
): ReadonlyArray<ContractDrift.Change> =>
  Array.flatMap(pinned, (service) =>
    Option.match(Option.fromNullable(registry.getService(service.typeName)), {
      onNone: (): ReadonlyArray<ContractDrift.Change> =>
        [{ _tag: "ServiceRemoved" as const, at: _ServiceCoord.make({ service: service.typeName }) }],
      onSome: (live) => _methodChanges(service.typeName, service, live),
    }))

type _Lane = (
  pair: readonly [DescField, DescField],
  at: typeof _FieldCoord.Type,
  descend: (pinned: DescMessage, live: DescMessage) => ReadonlyArray<ContractDrift.Change>,
) => ReadonlyArray<ContractDrift.Change>

const _lanes: ReadonlyArray<_Lane> = [
  ([was, is], at) =>
    was.name === is.name
      ? []
      : [{ _tag: "FieldRenamed" as const, at, from: was.name, to: is.name }],
  ([was, is], at) =>
    was.jsonName === is.jsonName
      ? []
      : [{ _tag: "JsonNameChanged" as const, at, from: was.jsonName, to: is.jsonName }],
  ([was, is], at) =>
    _wireFacts(was) === _wireFacts(is)
      ? []
      : [{ _tag: "WireTypeChanged" as const, at, from: _wireFacts(was), to: _wireFacts(is) }],
  ([was, is], at) =>
    was.oneof?.name === is.oneof?.name
      ? []
      : [{
          _tag: "OneofChanged" as const,
          at,
          from: Option.map(Option.fromNullable(was.oneof), (group) => group.name),
          to: Option.map(Option.fromNullable(is.oneof), (group) => group.name),
        }],
  ([was, is], at) => (_optionsAlike(was, is) ? [] : [{ _tag: "OptionChanged" as const, at }]),
  ([was, is], at) =>
    was.presence === is.presence
      ? []
      : [{ _tag: "PresenceChanged" as const, at, from: was.presence, to: is.presence }],
  ([was, is], at) =>
    was.utf8Validation === is.utf8Validation
      ? []
      : [{ _tag: "Utf8ValidationChanged" as const, at, from: was.utf8Validation, to: is.utf8Validation }],
  ([was, is]) =>
    Option.match(Option.all([_enumOf(was), _enumOf(is)]), {
      onNone: (): ReadonlyArray<ContractDrift.Change> => [],
      onSome: ([pinnedEnum, liveEnum]) => _enumChanges(pinnedEnum, liveEnum),
    }),
  ([was, is], _at, descend) =>
    Option.match(Option.all([_messageOf(was), _messageOf(is)]), {
      onNone: (): ReadonlyArray<ContractDrift.Change> => [],
      onSome: ([pinnedNested, liveNested]) => descend(pinnedNested, liveNested),
    }),
]

const _diffed = (
  pinned: DescMessage,
  live: DescMessage,
  visited: HashSet.HashSet<string> = HashSet.empty(),
): ReadonlyArray<ContractDrift.Change> => {
  const seen = HashSet.add(visited, qualifiedName(pinned))
  const descend = (pinnedNested: DescMessage, liveNested: DescMessage): ReadonlyArray<ContractDrift.Change> =>
    HashSet.has(seen, qualifiedName(pinnedNested)) ? [] : _diffed(pinnedNested, liveNested, seen)
  return _paired(pinned.fields, live.fields, (field) => field.number, {
    added: (field) => [{
      _tag: field.presence === FeatureSet_FieldPresence.LEGACY_REQUIRED ? "RequiredFieldAdded" as const : "FieldAdded" as const,
      at: _coord(live, field),
    }],
    removed: (field) => [{ _tag: "FieldRemoved" as const, at: _coord(pinned, field) }],
    shared: (was, is) =>
      _leaf(was) !== _leaf(is)
        ? [
            was.name === is.name
              ? { _tag: "TypeChanged" as const, at: _coord(live, is), from: _leaf(was), to: _leaf(is) }
              : { _tag: "NumberReused" as const, at: _coord(live, is), retired: was.name },
          ]
        : Array.flatMap(_lanes, (lane) => lane([was, is], _coord(live, is), descend)),
  })
}

```

## [04]-[GATE_SERVICE]

- Owner: `Contract.Descriptor` settles the descriptor census and implements `Contract.Gate<Contract.Family>`.
- Law: the protobuf census follows `Format.proto.names`; the capability pin and the service pin append one custom-source row each.
- Law: the service census walks the pinned `DescService` roster against the shipped registry, so message and RPC surfaces grade together.
- Law: unresolved proto descriptors emit `FamilyMissing`; capability evidence comes only from its canonical JSON document.
- Law: every read on this service reaches the declared `ContractFault | ParseResult.ParseError` channel; no decode path dies past it.
- Law: the generated suite and pinned capability document are baselines; both shipped reads map failures into `Contract.Fault`.
- Law: `Format.proto.frame(FileDescriptorSetSchema)` and `Format.json.schema(Contract.Pin)` decode their own ingress.
- Law: `gate({ family, compatibility })` is the only admission port and returns `Contract.Refusal` on a breaking axis.
- Law: `Contract` stays below wire vocabulary; codec maps the lower refusal into `Wire.Fault`.
- Law: `Contract.Refresh` admits a positive finite cadence and compiles its `Schedule.spaced` value once.
- Boundary: consumers resolve the current service through `Reloadable.get(Contract.Descriptor)` under the reloading layer.
- Packages: `@bufbuild/protobuf`, `@bufbuild/protobuf/wkt`, `effect`, and `./format.ts` (`Format.proto`, `Format.json`).

```typescript signature
// `Format.proto.frame` already types its output at the schema's own message, so an identity re-guard here decided
// nothing and its die was the one path out of this service's declared failure channel — a refresh tick that hit it
// would have killed the reloading fiber past every `ContractFault` handler watching for exactly that.
const _decodeSet = (octets: Uint8Array): Effect.Effect<MessageShape<typeof FileDescriptorSetSchema>, ParseResult.ParseError> =>
  Schema.decodeUnknown(Format.proto.frame(FileDescriptorSetSchema))(octets)

class DescriptorGate extends Effect.Service<DescriptorGate>()("@rasm/ts/core/DescriptorGate", {
  effect: (source: DescriptorGate.Source) =>
    Effect.gen(function* () {
      const [proto, capabilityOctets] = yield* Effect.all([
        source.proto.shipped,
        source.capability.shipped,
      ] as const)
      const registry = createFileRegistry(yield* _decodeSet(proto.octets))
      const [pinnedPin, livePin] = yield* Effect.all([
        _decodePin(source.capability.pinned),
        _decodePin(capabilityOctets),
      ] as const)
      const [pinnedRows, liveRows] = yield* Effect.all([
        _decodeDocument(pinnedPin),
        _decodeDocument(livePin),
      ] as const)
      const protoCensus = Array.map(Format.proto.names, (family): ContractDrift => {
        const pinned = Format.proto.suite[family]
        const changes = Option.match(Option.fromNullable(registry.getMessage(qualifiedName(pinned))), {
          onNone: (): ReadonlyArray<ContractDrift.Change> => [{ _tag: "FamilyMissing", family }],
          onSome: (current) => _diffed(pinned, current),
        })
        return ContractDrift.of(family, source.proto.pinnedGeneration, proto.generation, changes)
      })
      const census: ReadonlyArray<ContractDrift> = [
        ...protoCensus,
        ContractDrift.of(
          _capabilityFamily,
          pinnedPin.digest,
          livePin.digest,
          _descriptorChanges(pinnedPin, livePin, pinnedRows, liveRows),
        ),
        ContractDrift.of(
          _serviceFamily,
          source.proto.pinnedGeneration,
          proto.generation,
          _serviceChanges(source.proto.services, registry),
        ),
      ]
      const verdicts = HashMap.fromIterable(Array.map(census, (drift) => [drift.family, drift] as const))
      const gate: Contract.Gate<ContractDrift.Family> = (request) =>
        Option.match(HashMap.get(verdicts, request.family), {
          onNone: () => Effect.dieMessage(`<contract-family:${request.family}>`),
          onSome: (drift) =>
            drift.admitted(request.compatibility)
              ? Effect.void
              : Effect.fail(
                  new ContractRefusal({
                    family: request.family,
                    compatibility: request.compatibility,
                    verdict: drift.verdict(request.compatibility),
                    changes: drift.changes.length,
                  }),
                ),
        })
      return {
        verdict: (family: ContractDrift.Family): Option.Option<ContractDrift> => HashMap.get(verdicts, family),
        census,
        gate,
      }
    }),
  accessors: true,
}) {
  static readonly Row: typeof _DescriptorRow = _DescriptorRow
  static readonly reloading = (
    source: DescriptorGate.Source,
    refresh: ContractRefresh,
  ): Layer.Layer<Reloadable.Reloadable<DescriptorGate>, ContractFault | ParseResult.ParseError> =>
    Reloadable.auto(DescriptorGate, {
      layer: DescriptorGate.Default(source),
      schedule: refresh.schedule,
    })
}

declare namespace DescriptorGate {
  type Shipment = { readonly octets: Uint8Array; readonly generation: string }
  type Source = {
    readonly proto: {
      readonly shipped: Effect.Effect<Shipment, ContractFault>
      readonly pinnedGeneration: string
      // Generated `DescService` values are the RPC-surface pin, exactly as `Format.proto.suite` is the message pin;
      // both baselines are compiled in, so neither census reads a second shipped document to know what it expects.
      readonly services: ReadonlyArray<DescService>
    }
    readonly capability: {
      readonly pinned: Uint8Array
      readonly shipped: Effect.Effect<Uint8Array, ContractFault>
    }
  }
}

abstract class Contract {
  static readonly Drift: typeof ContractDrift = ContractDrift
  static readonly Refusal: typeof ContractRefusal = ContractRefusal
  static readonly Fault: typeof ContractFault = ContractFault
  static readonly Refresh: typeof ContractRefresh = ContractRefresh
  static readonly Pin: typeof ContractPin = ContractPin
  static readonly Descriptor: typeof DescriptorGate = DescriptorGate
  static readonly Family: typeof _Family = _Family
  static readonly Request: typeof _Request = _Request
}

declare namespace Contract {
  namespace Descriptor {
    type Row = Schema.Schema.Type<typeof _DescriptorRow>
  }
  type Drift = ContractDrift
  type Refusal = ContractRefusal
  type Fault = ContractFault
  type Refresh = ContractRefresh
  type Pin = ContractPin
  type Family = ContractDrift.Family
  type Compatibility = ContractDrift.Compatibility
  type Request<A> = { readonly family: A; readonly compatibility: Compatibility }
  type Gate<A> = (request: Request<A>) => Effect.Effect<void, Refusal>
  type Source = DescriptorGate.Source
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Contract }
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
