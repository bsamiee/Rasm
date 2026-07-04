# [CORE_CONTRACT]

The schema-drift authority of the interchange plane: pure reflection over two `FileDescriptorSet` generations — the build-pinned generation embedded in the generated proto suite and the live generation the C# runtime ships — folded into one graded `ContractDrift` verdict per proto census family at boot, so schema drift is a value the operator reads and a decode gate consumes, never a runtime decode failure. The diff walk pairs fields by number and compares the full wire signature — kind, leaf identity, and the delimited/packed wire facts — walks enum value rosters, and emits typed `DriftChange` rows; the severity table grades each change, the rank lattice folds a family's change set to its dominant verdict, and `admitted`/`alarm` are policy projections a `breaking` family refuses decode through as a `drift`-reasoned `WireFault`. The gate is proto-altitude only — the msgpack, cbor, and jsonpatch arms drift-check through their own vocabulary closures, and an out-of-vocabulary RFC 6902 op surfaces through this same verdict family. The module is `core/src/interchange/contract.ts`; a new detectable drift axis is one change case plus one grade row plus one comparison line, and a new verdict policy axis is one severity column.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                    | [PUBLIC]         |
| :-----: | :---------------- | :--------------------------------------------------------------------------- | :--------------- |
|  [01]   | `DRIFT_VERDICT`   | the change union, severity and grade tables, the verdict receipt and fold    | `ContractDrift`  |
|  [02]   | `GENERATION_DIFF` | the field-signature walk, the wire-fact compare, the enum-roster walk        | interior         |
|  [03]   | `GATE_SERVICE`    | the boot verdict census, per-family admission, the coverage law              | `DescriptorGate` |

## [2]-[DRIFT_VERDICT]

[DRIFT_VERDICT]:
- Owner: `ContractDrift`, the verdict receipt — one `Schema.Class` carrying the family, the pinned and live generation coordinates, and every field-level change as typed `DriftChange` union rows; `_severity` grades verdict policy (`rank`, `admitted`, `alarm`), `_grade` maps each change kind to its verdict, `dominant` folds a change set through the rank lattice, and `admitted`/`alarm` are row projections.
- Law: verdict admission is data — `identical` and `additive` admit decode, `breaking` refuses it; the refusal surfaces as a `WireFault` with reason `drift` at the consuming registry row, never a `ParseError` mid-decode.
- Law: severity folds by the lattice — one `breaking` change makes the family `breaking` regardless of additive siblings, `identical` is exactly the empty change set, and `dominant` is a grade lookup per change plus `Array.max` over the rank order.
- Law: the change union carries the two enum-roster verdicts as first-class detected rows — `EnumValueAdded` grades `additive`, `EnumValueRemoved` grades `breaking` — and `WireTypeChanged` carries the delimited/packed wire-fact signatures both sides; every change kind the vocabulary declares is minted by the `[03]` walk, so no verdict row is dead vocabulary.
- Law: the verdict family is `Schema`-declared — verdicts serialize into CI artifacts and cross the reporting boundary; a process-local re-model is the parallel-shape defect.
- Growth: a new change kind is one union case plus one `_grade` row — the grade record's mapped contract breaks until the row lands; a new policy axis is one `_severity` column.
- Boundary: computing changes from descriptor generations is `[03]`'s walk; the `codec` registry consumes `admitted` through the gate service; the jsonpatch alien-op surfacing composes this vocabulary at its registry row.
- Packages: `effect` (`Schema`, `Array`, `Order`); `./codec.ts` (`Wire`).

```typescript
import { Array, Order, Schema } from "effect"
import { Wire } from "./codec.ts"

const _severity = {
  identical: { rank: 0, admitted: true, alarm: false },
  additive: { rank: 1, admitted: true, alarm: false },
  breaking: { rank: 2, admitted: false, alarm: true },
} as const

const _grade = {
  FieldAdded: "additive",
  EnumValueAdded: "additive",
  EnumValueRemoved: "breaking",
  FieldRemoved: "breaking",
  TypeChanged: "breaking",
  WireTypeChanged: "breaking",
  NumberReused: "breaking",
  FamilyMissing: "breaking",
} as const

const _FieldCoord = Schema.Struct({
  message: Schema.NonEmptyString,
  field: Schema.NonEmptyString,
  number: Schema.Int,
})

const _EnumCoord = Schema.Struct({
  enum: Schema.NonEmptyString,
  value: Schema.NonEmptyString,
  number: Schema.Int,
})

const _Change = Schema.Union(
  Schema.TaggedStruct("FieldAdded", { at: _FieldCoord }),
  Schema.TaggedStruct("EnumValueAdded", { at: _EnumCoord }),
  Schema.TaggedStruct("EnumValueRemoved", { at: _EnumCoord }),
  Schema.TaggedStruct("FieldRemoved", { at: _FieldCoord }),
  Schema.TaggedStruct("TypeChanged", { at: _FieldCoord, from: Schema.NonEmptyString, to: Schema.NonEmptyString }),
  Schema.TaggedStruct("WireTypeChanged", { at: _FieldCoord, from: Schema.NonEmptyString, to: Schema.NonEmptyString }),
  Schema.TaggedStruct("NumberReused", { at: _FieldCoord, retired: Schema.NonEmptyString }),
  Schema.TaggedStruct("FamilyMissing", { family: Wire.wire }),
)

class ContractDrift extends Schema.Class<ContractDrift>("ContractDrift")({
  family: Wire.wire,
  verdict: Schema.Literal("identical", "additive", "breaking"),
  pinned: Schema.NonEmptyString,
  live: Schema.NonEmptyString,
  changes: Schema.Array(_Change),
}) {
  static readonly Change: typeof _Change = _Change
  static readonly rank: Order.Order<ContractDrift.Verdict> = Order.mapInput(
    Order.number,
    (verdict: ContractDrift.Verdict) => _severity[verdict].rank,
  )
  static readonly graded = (change: ContractDrift.Change): ContractDrift.Verdict => _grade[change._tag]
  static readonly dominant = (changes: ReadonlyArray<ContractDrift.Change>): ContractDrift.Verdict =>
    Array.match(changes, {
      onEmpty: (): ContractDrift.Verdict => "identical",
      onNonEmpty: (present) => Array.max(Array.map(present, ContractDrift.graded), ContractDrift.rank),
    })
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
  type _Grades<T extends Record<Change["_tag"], Verdict> = typeof _grade> = T
}
```

## [3]-[GENERATION_DIFF]

[GENERATION_DIFF]:
- Owner: the interior walk — `_leaf` classifies a `DescField` to its type signature through the `fieldKind` record dispatch, `_wireFacts` renders the delimited/packed encoding posture, `_enumChanges` pairs enum value rosters by number, and `_diffed` walks one pinned `DescMessage` against its live counterpart by field number, folding every disagreement into `DriftChange` rows.
- Law: fields pair by number, never by name — the wire is number-addressed, so a renamed field with a stable number and signature is `identical`, a re-numbered field is a remove-plus-add pair, and a reused number whose signature changed is `NumberReused`, the severest field-level lie.
- Law: `_leaf` is type equality and `_wireFacts` is encoding equality, compared separately — a field whose leaf identity holds but whose `delimitedEncoding` or `packed` posture flipped emits `WireTypeChanged` carrying both fact signatures, because the bytes on the wire change while the type story claims stability; presentation facts (json name, comments) never enter either signature.
- Law: enum rosters walk on every shared enum-kind field — a live value number absent from the pinned roster emits `EnumValueAdded`, a pinned number absent live emits `EnumValueRemoved`; the walk keys by value number, so a renamed enum value with a stable number is `identical`.
- Law: both directions run in one fold over the deduped union of field numbers — pinned-only numbers emit `FieldRemoved`, live-only numbers emit `FieldAdded`, shared numbers compare signature, wire facts, enum rosters, then names.
- Growth: an option-level change or reserved-range violation is one change case plus one comparison line here; the walk shape never changes.
- Packages: `@bufbuild/protobuf` (`DescEnum`, `DescField`, `DescMessage`, `ScalarType`, `qualifiedName`); `effect` (`Array`, `HashMap`, `Match`, `Option`).

```typescript
import { type DescEnum, type DescField, type DescMessage, qualifiedName, ScalarType } from "@bufbuild/protobuf"
import { HashMap, Match, Option } from "effect"

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
    ? Option.some(field.enum)
    : Option.none()

const _enumChanges = (pinned: DescEnum, live: DescEnum): ReadonlyArray<ContractDrift.Change> => {
  const before = HashMap.fromIterable(Array.map(pinned.values, (value) => [value.number, value] as const))
  const after = HashMap.fromIterable(Array.map(live.values, (value) => [value.number, value] as const))
  const numbers = Array.dedupe([
    ...Array.map(pinned.values, (value) => value.number),
    ...Array.map(live.values, (value) => value.number),
  ])
  return Array.filterMap(numbers, (number): Option.Option<ContractDrift.Change> =>
    Option.match(HashMap.get(before, number), {
      onNone: () =>
        Option.map(HashMap.get(after, number), (added) => ({
          _tag: "EnumValueAdded" as const,
          at: _EnumCoord.make({ enum: qualifiedName(live), value: added.name, number }),
        })),
      onSome: (was) =>
        Option.isNone(HashMap.get(after, number))
          ? Option.some({
              _tag: "EnumValueRemoved" as const,
              at: _EnumCoord.make({ enum: qualifiedName(pinned), value: was.name, number }),
            })
          : Option.none(),
    }))
}

const _diffed = (pinned: DescMessage, live: DescMessage): ReadonlyArray<ContractDrift.Change> => {
  const before = HashMap.fromIterable(Array.map(pinned.fields, (field) => [field.number, field] as const))
  const after = HashMap.fromIterable(Array.map(live.fields, (field) => [field.number, field] as const))
  const numbers = Array.dedupe([
    ...Array.map(pinned.fields, (field) => field.number),
    ...Array.map(live.fields, (field) => field.number),
  ])
  return Array.flatMap(numbers, (number): ReadonlyArray<ContractDrift.Change> =>
    Option.match(HashMap.get(before, number), {
      onNone: () =>
        Option.match(HashMap.get(after, number), {
          onNone: () => [],
          onSome: (added) => [{ _tag: "FieldAdded" as const, at: _coord(live, added) }],
        }),
      onSome: (was) =>
        Option.match(HashMap.get(after, number), {
          onNone: () => [{ _tag: "FieldRemoved" as const, at: _coord(pinned, was) }],
          onSome: (is) =>
            _leaf(was) !== _leaf(is)
              ? was.name === is.name
                ? [{ _tag: "TypeChanged" as const, at: _coord(live, is), from: _leaf(was), to: _leaf(is) }]
                : [{ _tag: "NumberReused" as const, at: _coord(live, is), retired: was.name }]
              : _wireFacts(was) !== _wireFacts(is)
                ? [{ _tag: "WireTypeChanged" as const, at: _coord(live, is), from: _wireFacts(was), to: _wireFacts(is) }]
                : Option.match(Option.all([_enumOf(was), _enumOf(is)]), {
                    onNone: (): ReadonlyArray<ContractDrift.Change> => [],
                    onSome: ([pinnedEnum, liveEnum]) => _enumChanges(pinnedEnum, liveEnum),
                  }),
        }),
    }))
}
```

## [4]-[GATE_SERVICE]

[GATE_SERVICE]:
- Owner: `DescriptorGate`, the boot-time gate — one `Effect.Service` whose Layer factory takes the live descriptor-set octets plus the generation label, decodes through the proto engine and the shipped `./wkt` schema, builds the `FileRegistry`, and folds one verdict per suite family at construction; the verdict census is immutable for the service's life, and `verdict`/`census`/`admitted` are reads over it.
- Law: coverage is the suite key tuple — `Proto.names` is census-guarded at `format#PROTO_ENGINE`, so iterating it IS iterating every gated proto family; a suite family unresolved by `qualifiedName` in the live registry folds to a `FamilyMissing` breaking verdict, so silence cannot pass for compatibility. `FileDescriptorSetWire` never enters the verdict census — it is the gate's own transport.
- Law: the pinned side is the generated suite itself — each `GenMessage` is a `DescMessage`, so the build artifact is the baseline and no second pinned descriptor file exists to drift from the code that decodes with it.
- Law: the gate decodes its own ingress through the one admission rail — `Proto.frame(FileDescriptorSetSchema)` with the message identity narrowed by `isMessage`; a non-set payload at this seam is a wiring defect and dies, never a typed fault.
- Law: `admitted(family)` is the decode gate the registry's gated rows yield before decoding — a `breaking` family refuses with reason `drift` carrying the change count; the boot log, the CI artifact, and the refusal detail are projections of one verdict value.
- Growth: a second gated consumer composes `admitted` in its decode pipeline — one yield, zero gate edits; a new generation source (a registry endpoint over shipped bytes) is a new Layer factory shape at the app root, never a second gate.
- Boundary: the `codec` registry's `admittedGraph` entry takes this gate's `admitted` as its gate argument; the runtime wave's boot sequence provides the Layer with the runtime-shipped set.
- Packages: `@bufbuild/protobuf` (`createFileRegistry`, `isMessage`, `qualifiedName`, `MessageShape`), `@bufbuild/protobuf/wkt` (`FileDescriptorSetSchema`); `effect` (`Effect`, `HashMap`, `Option`, `Schema`); `./format.ts` (`Proto`); `./codec.ts` (`Wire`, `WireFault`).

```typescript
import { createFileRegistry, isMessage, type MessageShape } from "@bufbuild/protobuf"
import { FileDescriptorSetSchema } from "@bufbuild/protobuf/wkt"
import { Effect, type ParseResult } from "effect"
import { type Wire, WireFault } from "./codec.ts"
import { Proto } from "./format.ts"

const _PINNED = "<build-pin>"

const _decodeSet = (octets: Uint8Array): Effect.Effect<MessageShape<typeof FileDescriptorSetSchema>, ParseResult.ParseError> =>
  Schema.decodeUnknown(Proto.frame(FileDescriptorSetSchema))(octets).pipe(
    Effect.filterOrDieMessage(
      (message): message is MessageShape<typeof FileDescriptorSetSchema> => isMessage(message, FileDescriptorSetSchema),
      "<descriptor-set-identity>",
    ),
  )

class DescriptorGate extends Effect.Service<DescriptorGate>()("@rasm/ts/core/DescriptorGate", {
  effect: (live: Uint8Array, generation: string) =>
    Effect.gen(function* () {
      const registry = createFileRegistry(yield* _decodeSet(live))
      const verdicts = Array.reduce(
        Proto.names,
        HashMap.empty<Wire.Family, ContractDrift>(),
        (acc, family) => {
          const pinned = Proto.suite[family]
          const changes = Option.match(Option.fromNullable(registry.getMessage(qualifiedName(pinned))), {
            onNone: (): ReadonlyArray<ContractDrift.Change> => [{ _tag: "FamilyMissing", family }],
            onSome: (current) => _diffed(pinned, current),
          })
          return HashMap.set(
            acc,
            family,
            new ContractDrift({ family, verdict: ContractDrift.dominant(changes), pinned: _PINNED, live: generation, changes }),
          )
        },
      )
      return {
        verdict: (family: Wire.Family): Option.Option<ContractDrift> => HashMap.get(verdicts, family),
        census: Effect.succeed(Array.fromIterable(HashMap.values(verdicts))),
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

// --- [EXPORTS] --------------------------------------------------------------------------

export { ContractDrift, DescriptorGate }
```
