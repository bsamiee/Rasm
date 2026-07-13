# [CORE_CONTRACT]

The schema-drift authority of the interchange plane: pure reflection over two `FileDescriptorSet` generations — the build-pinned generation embedded in the generated proto suite and the live generation the C# runtime ships — folded into one graded `ContractDrift` verdict per proto census family at boot, so schema drift is a value the operator reads and a decode gate consumes, never a runtime decode failure. The diff walk pairs fields by number, gates on leaf identity, then folds the ordered lane table — wire facts, oneof membership, serialized field options, enum rosters, and recursive nested-message descent — while the RPC walk pairs the pinned `DescService` roster's methods by name and compares `methodKind`, `idempotency`, and the input/output signature; every disagreement is a typed `DriftChange` row. The verdict is derived, never asserted: `ContractDrift.of` is the one mint computing the dominant verdict from the change set, and the class declaration filter-proves `verdict === dominant(changes)` on every decode, so a serialized receipt that claims `identical` over a breaking change set refuses at admission and `admitted`/`alarm` can never disagree with the change lattice they project. The gate is proto-altitude only — the msgpack, cbor, and jsonpatch arms drift-check through their own vocabulary closures, an out-of-vocabulary RFC 6902 op and the msgpack `Alien` ext land through this same verdict vocabulary at their registry rows, and live-message unknown-field residue is `codec`'s `Wire.residue` read, the runtime complement of this boot-time grade. The module is `core/src/interchange/contract.ts`; a new detectable drift axis is one change case plus one grade row plus one lane row, and a new verdict policy axis is one severity column.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                    | [PUBLIC]         |
| :-----: | :---------------- | :------------------------------------------------------------------------ | :--------------- |
|  [01]   | `DRIFT_VERDICT`   | the change union, severity and grade tables, the verdict receipt and fold | `ContractDrift`  |
|  [02]   | `GENERATION_DIFF` | the field-signature walk, the wire-fact compare, the enum-roster walk     | interior         |
|  [03]   | `GATE_SERVICE`    | the boot verdict census, per-family admission, the coverage law           | `DescriptorGate` |

## [02]-[DRIFT_VERDICT]

- Owner: `ContractDrift`, the verdict receipt — one `Schema.Class` carrying the family, the pinned and live generation coordinates, and every field-level change as typed `DriftChange` union rows; `_severity` grades verdict policy (`rank`, `admitted`, `alarm`), `_grade` maps each change kind to its verdict, `_dominant` folds a change set through the rank lattice, `of` is the one mint, and `admitted`/`alarm` are row projections.
- Law: the verdict derives from the evidence at both trust directions — `ContractDrift.of(family, pinned, live, changes)` computes `verdict` as `_dominant(changes)` so interior mints cannot restate the fold, and the class's declaration filter re-proves the same equation on decode, so a wire-carried receipt whose stored verdict disagrees with its own change rows is a `ParseError` at admission, never a lying `admitted` projection downstream.
- Law: verdict admission is data — `identical` and `additive` admit decode, `breaking` refuses it; the refusal surfaces as a `WireFault` with reason `drift` at the consuming registry row, never a `ParseError` mid-decode.
- Law: severity folds by the lattice — one `breaking` change makes the family `breaking` regardless of additive siblings, `identical` is exactly the empty change set, and `_dominant` is a grade lookup per change plus `Array.max` over the rank order.
- Law: the change union carries the two enum-roster verdicts as first-class detected rows — `EnumValueAdded` grades `additive`, `EnumValueRemoved` grades `breaking` — and `WireTypeChanged` carries the delimited/packed wire-fact signatures both sides; every change kind the vocabulary declares is minted by the `[03]` walk, so no verdict row is dead vocabulary.
- Law: the union spans the field-contract axes independently — `OneofChanged`, resolved `PresenceChanged`, `Utf8ValidationChanged`, serialized `OptionChanged`, and `RequiredFieldAdded` distinguish an additive optional field from a legacy-required addition old writers cannot satisfy; `ServiceMissing` plus the method triple `MethodAdded`/`MethodRemoved`/`MethodChanged` grade the RPC plane, and nested-message drift recurses at the nested coordinates.
- Law: the verdict family is `Schema`-declared — verdicts serialize into CI artifacts and cross the reporting boundary; a process-local re-model is the parallel-shape defect.
- Growth: a new change kind is one union case plus one `_grade` row — the grade record's mapped contract breaks until the row lands; a new policy axis is one `_severity` column.
- Boundary: computing changes from descriptor generations is `[03]`'s walk; the `codec` registry consumes `admitted` through the gate service; the jsonpatch alien-op and msgpack `Alien` ext surfacings compose this vocabulary at their registry rows; live-message unknown-field residue is `codec`'s `Wire.residue` read.
- Packages: `effect` (`Schema`, `Array`, `Order`); `./codec.ts` (`Wire`).

```typescript signature
import {
  createFileRegistry,
  type DescEnum,
  type DescField,
  type DescMessage,
  type DescMethod,
  type DescService,
  equals,
  isMessage,
  type MessageShape,
  qualifiedName,
  ScalarType,
} from "@bufbuild/protobuf"
import { FeatureSet_FieldPresence, FieldOptionsSchema, FileDescriptorSetSchema } from "@bufbuild/protobuf/wkt"
import { Array, Effect, HashMap, HashSet, Match, Option, Order, type ParseResult, Schema } from "effect"
import { Wire, WireFault } from "./codec.ts"
import { Proto } from "./format.ts"

const _verdicts = ["identical", "additive", "breaking"] as const

const _severity = {
  identical: { rank: 0, admitted: true, alarm: false },
  additive: { rank: 1, admitted: true, alarm: false },
  breaking: { rank: 2, admitted: false, alarm: true },
} as const

const _grade = {
  FieldAdded: "additive",
  EnumValueAdded: "additive",
  MethodAdded: "additive",
  OptionChanged: "additive",
  EnumValueRemoved: "breaking",
  FieldRemoved: "breaking",
  MethodRemoved: "breaking",
  MethodChanged: "breaking",
  OneofChanged: "breaking",
  TypeChanged: "breaking",
  WireTypeChanged: "breaking",
  NumberReused: "breaking",
  FamilyMissing: "breaking",
  ServiceMissing: "breaking",
  PresenceChanged: "breaking",
  RequiredFieldAdded: "breaking",
  Utf8ValidationChanged: "breaking",
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

const _MethodCoord = Schema.Struct({
  service: Schema.NonEmptyString,
  method: Schema.NonEmptyString,
})

const _Change = Schema.Union(
  Schema.TaggedStruct("FieldAdded", { at: _FieldCoord }),
  Schema.TaggedStruct("EnumValueAdded", { at: _EnumCoord }),
  Schema.TaggedStruct("MethodAdded", { at: _MethodCoord }),
  Schema.TaggedStruct("OptionChanged", { at: _FieldCoord }),
  Schema.TaggedStruct("EnumValueRemoved", { at: _EnumCoord }),
  Schema.TaggedStruct("FieldRemoved", { at: _FieldCoord }),
  Schema.TaggedStruct("MethodRemoved", { at: _MethodCoord }),
  Schema.TaggedStruct("MethodChanged", { at: _MethodCoord, from: Schema.NonEmptyString, to: Schema.NonEmptyString }),
  Schema.TaggedStruct("OneofChanged", {
    at: _FieldCoord,
    from: Schema.Option(Schema.NonEmptyString),
    to: Schema.Option(Schema.NonEmptyString),
  }),
  Schema.TaggedStruct("TypeChanged", { at: _FieldCoord, from: Schema.NonEmptyString, to: Schema.NonEmptyString }),
  Schema.TaggedStruct("WireTypeChanged", { at: _FieldCoord, from: Schema.NonEmptyString, to: Schema.NonEmptyString }),
  Schema.TaggedStruct("NumberReused", { at: _FieldCoord, retired: Schema.NonEmptyString }),
  Schema.TaggedStruct("FamilyMissing", { family: Wire.wire }),
  Schema.TaggedStruct("ServiceMissing", { service: Schema.NonEmptyString }),
  Schema.TaggedStruct("PresenceChanged", { at: _FieldCoord, from: Schema.Int, to: Schema.Int }),
  Schema.TaggedStruct("RequiredFieldAdded", { at: _FieldCoord }),
  Schema.TaggedStruct("Utf8ValidationChanged", { at: _FieldCoord, from: Schema.Boolean, to: Schema.Boolean }),
)

const _rank: Order.Order<ContractDrift.Verdict> = Order.mapInput(
  Order.number,
  (verdict: ContractDrift.Verdict) => _severity[verdict].rank,
)

const _graded = (change: ContractDrift.Change): ContractDrift.Verdict => _grade[change._tag]

const _dominant = (changes: ReadonlyArray<ContractDrift.Change>): ContractDrift.Verdict =>
  Array.match(changes, {
    onEmpty: (): ContractDrift.Verdict => "identical",
    onNonEmpty: (present) => Array.max(Array.map(present, _graded), _rank),
  })

class ContractDrift extends Schema.Class<ContractDrift>("ContractDrift")(
  Schema.Struct({
    family: Wire.wire,
    verdict: Schema.Literal(..._verdicts),
    pinned: Schema.NonEmptyString,
    live: Schema.NonEmptyString,
    changes: Schema.Array(_Change),
  }).pipe(
    Schema.filter((receipt) => receipt.verdict === _dominant(receipt.changes) || "<verdict-detached-from-changes>", {
      identifier: "VerdictDerived", // the receipt cannot lie: a stored verdict its own change rows do not dominate refuses at decode and construction alike
    }),
  ),
) {
  static readonly Change: typeof _Change = _Change
  static readonly dominant: (changes: ReadonlyArray<ContractDrift.Change>) => ContractDrift.Verdict = _dominant
  static readonly graded: (change: ContractDrift.Change) => ContractDrift.Verdict = _graded
  static readonly rank: Order.Order<ContractDrift.Verdict> = _rank
  static readonly of = (
    family: Wire.Family,
    pinned: string,
    live: string,
    changes: ReadonlyArray<ContractDrift.Change>,
  ): ContractDrift => ContractDrift.make({ family, verdict: _dominant(changes), pinned, live, changes })
  get admitted(): boolean {
    return _severity[this.verdict].admitted
  }
  get alarm(): boolean {
    return _severity[this.verdict].alarm
  }
}

declare namespace ContractDrift {
  type Verdict = keyof typeof _severity
  type Change = Schema.Schema.Type<typeof _Change>
  type _Rows<T extends Record<(typeof _verdicts)[number], { readonly rank: number; readonly admitted: boolean; readonly alarm: boolean }> = typeof _severity> = T
  type _Grades<T extends Record<Change["_tag"], Verdict> = typeof _grade> = T
  type _Keys<K extends (typeof _verdicts)[number] = Verdict> = K
  type _GradeKeys<K extends Change["_tag"] = keyof typeof _grade> = K
}
```

## [03]-[GENERATION_DIFF]

- Owner: the interior walk — `_leaf` classifies a `DescField` to its type signature through the `fieldKind` record dispatch, `_wireFacts` renders the delimited/packed encoding posture, `_signature` renders a `DescMethod` to its RPC signature, `_enumOf`/`_messageOf` project the roster-carrying descriptor off any field kind, `_lanes` is the ordered comparison-lane table every shared field pair folds through, and `_paired` is the one keyed roster fold — `added`, `removed`, `shared` arms over two generations — that `_enumChanges` instantiates by value number, `_serviced` by method name, and `_diffed` by field number, the field walk recursing over message-typed leaves under a visited-set guard; every disagreement folds into `DriftChange` rows.
- Law: fields pair by number, never by name — the wire is number-addressed, so a renamed field with a stable number and signature is `identical`, a re-numbered field is a remove-plus-add pair, and a reused number whose signature changed is `NumberReused`, the severest field-level lie.
- Law: leaf identity gates, lanes accumulate — a leaf disagreement is exclusive (`TypeChanged` under a stable name, `NumberReused` under a new one — deeper comparison across changed types is noise), and an agreeing leaf folds the whole `_lanes` table so one field pair can carry wire-fact, oneof, option, roster, and nested rows together; a ternary ladder that reports only the first disagreement is the rejected shape.
- Law: `_wireFacts` is encoding equality separate from type equality — a flipped `delimitedEncoding` or `packed` posture emits `WireTypeChanged` carrying both fact signatures, because the bytes change while the type story claims stability; presentation facts (json name, comments) never enter any signature.
- Law: field policy has three independent lanes — serialized `FieldOptions` equality emits `OptionChanged`, resolved `presence` emits `PresenceChanged`, and resolved `utf8Validation` emits `Utf8ValidationChanged`; oneof membership remains its own `OneofChanged` evidence, so edition resolution and option bytes cannot mask each other.
- Law: enum rosters walk on every shared enum-carrying field — singular, list, and map-valued kinds reach the roster through the one `_enumOf` projection; a live value number absent pinned emits `EnumValueAdded`, a pinned number absent live emits `EnumValueRemoved`, keyed by value number so a renamed value with a stable number is `identical`.
- Law: nested descent is the same fold — `_messageOf` mirrors `_enumOf`, an agreeing message-typed leaf recurses `_diffed` into the nested pair at its own coordinates, and the visited set keyed by pinned `qualifiedName` breaks recursive message cycles; a nested drift is therefore never invisible behind a stable qualified name.
- Law: methods pair by name — the RPC path is name-addressed, unlike fields — and `_signature` compares `methodKind`, the input/output qualified names, and `idempotency` as one rendered string; a missing live service emits `ServiceMissing` before the method walk, so even an empty pinned service cannot pass as present.
- Growth: a new drift axis is one change case plus one `_grade` row plus one `_lanes` row; the fold shape never changes.
- Packages: `@bufbuild/protobuf` (`DescEnum`, `DescField`, `DescMessage`, `DescMethod`, `DescService`, `ScalarType`, `equals`, `qualifiedName`), `@bufbuild/protobuf/wkt` (`FieldOptionsSchema`); `effect` (`Array`, `HashMap`, `HashSet`, `Match`, `Option`).

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

const _signature = (method: DescMethod): string =>
  `${method.methodKind}:${qualifiedName(method.input)}->${qualifiedName(method.output)}|idempotency:${method.idempotency}`

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

const _optionsAlike = (was: DescField, is: DescField): boolean =>
  was.proto.options === undefined || is.proto.options === undefined
    ? was.proto.options === is.proto.options
    : equals(FieldOptionsSchema, was.proto.options, is.proto.options)

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
    shared: () => [],
  })

type _Lane = (
  pair: readonly [DescField, DescField],
  at: typeof _FieldCoord.Type,
  descend: (pinned: DescMessage, live: DescMessage) => ReadonlyArray<ContractDrift.Change>,
) => ReadonlyArray<ContractDrift.Change>

const _lanes: ReadonlyArray<_Lane> = [
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

const _serviced = (pinned: DescService, live: DescService | undefined): ReadonlyArray<ContractDrift.Change> => {
  const coordOf = (method: DescMethod): typeof _MethodCoord.Type =>
    _MethodCoord.make({ service: qualifiedName(pinned), method: method.name })
  return live === undefined ? [{ _tag: "ServiceMissing", service: qualifiedName(pinned) }] : _paired(pinned.methods, live.methods, (method) => method.name, {
    added: (method) => [{ _tag: "MethodAdded" as const, at: coordOf(method) }],
    removed: (method) => [{ _tag: "MethodRemoved" as const, at: coordOf(method) }],
    shared: (was, is) =>
      _signature(was) === _signature(is)
        ? []
        : [{ _tag: "MethodChanged" as const, at: coordOf(was), from: _signature(was), to: _signature(is) }],
  })
}
```

## [04]-[GATE_SERVICE]

- Owner: `DescriptorGate`, the boot-time gate — one `Effect.Service` whose `Source` carrier binds live descriptor-set octets, pinned and live generation coordinates, and the pinned `DescService` roster; construction decodes through the proto engine, builds the `FileRegistry`, and settles one immutable verdict per suite family, while `verdict`/`census`/`admitted` are reads over that census.
- Law: coverage is the suite key tuple plus the supplied RPC roster — `Proto.names` is census-guarded at `format#PROTO_ENGINE`, so iterating it IS iterating every gated proto family; a suite family unresolved by `qualifiedName` in the live registry folds to a `FamilyMissing` breaking verdict, so silence cannot pass for compatibility. `FileDescriptorSetWire` never enters the verdict census — it is the gate's own transport. A family outside the proto census (`OpLogWire`, the cbor and jsonpatch arms) answers `admitted` with `Effect.void` by construction: the gate is proto-altitude only, and those arms drift-check through their own vocabulary closures.
- Law: the RPC census is the pinned roster the composition root supplies — the same emitted `DescService` consts it hands the invoke `Dial`; `registry.getService` resolves each live counterpart, `_serviced` mints the method rows, and they fold into the `CapabilityDescriptorWire` verdict — the capability plane's one family — so the composition root sequences `admitted("CapabilityDescriptorWire")` ahead of the invoke `Capability.bind` with zero new gate surface, and an empty roster degrades the RPC axis to no coverage, never a false `identical` claim about services it was not given.
- Law: the pinned side is the generated suite itself — each `GenMessage` is a `DescMessage`, so the build artifact is the baseline and no second pinned descriptor file exists to drift from the code that decodes with it.
- Law: the gate decodes its own ingress through the one admission rail — `Proto.frame(FileDescriptorSetSchema)` with the message identity narrowed by `isMessage`; a non-set payload at this seam is a wiring defect and dies, never a typed fault.
- Law: `admitted(family)` is the decode gate the registry's gated rows yield before decoding — a `breaking` family refuses with reason `drift` carrying the change count; the boot log, the CI artifact, and the refusal detail are projections of one verdict value, and `census` is that value's plain array read — the verdicts settled at construction, so no read re-enters the rail.
- Growth: a second gated consumer composes `admitted` in its decode pipeline — one yield, zero gate edits; a new generation source (a registry endpoint over shipped bytes) is a new Layer factory shape at the app root, never a second gate.
- Boundary: the `codec` registry's `admittedGraph` entry takes this gate's `admitted` as its gate argument; the invoke page's `Capability.bind` composes `admitted("CapabilityDescriptorWire")` before binding; the runtime wave's boot sequence constructs the Layer from the runtime-shipped set and the pinned service consts.
- Packages: `@bufbuild/protobuf` (`createFileRegistry`, `isMessage`, `qualifiedName`, `DescService`, `MessageShape`), `@bufbuild/protobuf/wkt` (`FileDescriptorSetSchema`); `effect` (`Effect`, `Array`, `HashMap`, `Option`, `Schema`, `ParseResult`); `./format.ts` (`Proto`); `./codec.ts` (`Wire`, `WireFault`).

```typescript signature
const _decodeSet = (octets: Uint8Array): Effect.Effect<MessageShape<typeof FileDescriptorSetSchema>, ParseResult.ParseError> =>
  Schema.decodeUnknown(Proto.frame(FileDescriptorSetSchema))(octets).pipe(
    Effect.filterOrDieMessage(
      (message): message is MessageShape<typeof FileDescriptorSetSchema> => isMessage(message, FileDescriptorSetSchema),
      "<descriptor-set-identity>",
    ),
  )

class DescriptorGate extends Effect.Service<DescriptorGate>()("@rasm/ts/core/DescriptorGate", {
  effect: (source: DescriptorGate.Source) =>
    Effect.gen(function* () {
      const registry = createFileRegistry(yield* _decodeSet(source.live))
      const methodRows = Array.flatMap(source.rpc, (service) => _serviced(service, registry.getService(qualifiedName(service))))
      const verdicts = Array.reduce(
        Proto.names,
        HashMap.empty<Wire.Family, ContractDrift>(),
        (acc, family) => {
          const pinned = Proto.suite[family]
          const diffed = Option.match(Option.fromNullable(registry.getMessage(qualifiedName(pinned))), {
            onNone: (): ReadonlyArray<ContractDrift.Change> => [{ _tag: "FamilyMissing", family }],
            onSome: (current) => _diffed(pinned, current),
          })
          const changes = family === "CapabilityDescriptorWire" ? [...diffed, ...methodRows] : diffed
          return HashMap.set(acc, family, ContractDrift.of(family, source.pinnedGeneration, source.liveGeneration, changes))
        },
      )
      return {
        verdict: (family: Wire.Family): Option.Option<ContractDrift> => HashMap.get(verdicts, family),
        census: Array.fromIterable(HashMap.values(verdicts)),
        admitted: (family: Wire.Family): Effect.Effect<void, WireFault> =>
          Option.match(HashMap.get(verdicts, family), {
            onNone: () => Effect.void,
            onSome: (drift) =>
              drift.admitted
                ? Effect.void
                : Effect.fail(
                    new WireFault({
                      family,
                      reason: "drift",
                      detail: `<breaking:${drift.changes.length}>`,
                      evidence: Option.none(),
                    }),
                  ),
          }),
      }
    }),
  accessors: true,
}) {}

declare namespace DescriptorGate {
  type Source = {
    readonly live: Uint8Array
    readonly liveGeneration: string
    readonly pinnedGeneration: string
    readonly rpc: ReadonlyArray<DescService>
  }
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { ContractDrift, DescriptorGate }
```
