# [WIRE_DESCRIPTOR]

`contract/descriptor.ts` is the `FileDescriptorSet` drift gate: pure reflection over two descriptor generations — the build-pinned generation embedded in the generated suite, and the live generation the C# runtime ships — folded into one `ContractDrift` verdict per proto census family. Schema drift surfaces as a value at boot, never as a runtime decode failure: the gate service computes every verdict once, content-keyed consumers (`codec/graph.ts` first) require admission before decoding, and a `breaking` family refuses with a typed `WireFault` while `identical` and `additive` families decode under the preserved-unknown-field posture the proto engine already holds. The gate is proto-altitude only — CBOR, MessagePack, and JSON-Patch families drift-check through their own vocabulary gates, and `codec/patch.ts` surfaces an out-of-vocabulary op through the same verdict family.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                                        |
| :-----: | :---------------- | :------------------------------------------------------------------------------ |
|   [1]   | `GENERATION_DIFF` | the field-level diff fold: two descriptor walks into `DriftChange` rows          |
|   [2]   | `GATE_SERVICE`    | the `DescriptorGate` service: verdict census, per-family admission, coverage law |

## [2]-[GENERATION_DIFF]

- Owner: the interior diff fold — `_leaf` classifies a `DescField` to its wire signature through the `fieldKind` record dispatch, `_diffed` walks one pinned `DescMessage` against its live counterpart by field number, `ContractDrift.dominant` folds the change rows.
- Entry: interior only — the gate service is the sole caller; the fold's output vocabulary is `contract/drift.ts`'s `DriftChange` rows, never a local change shape.
- Growth: a new detectable drift axis (an option-level change, a reserved-range violation, a `delimitedEncoding` flip) is one `DriftChange` case at `contract/drift.ts` plus one comparison line in `_diffed`; the walk shape never changes.
- Law: fields pair by number, never by name — the wire is number-addressed, so a renamed field with a stable number and signature is `identical`, a re-numbered field is a remove-plus-add pair, and a reused number whose signature changed is `NumberReused`, the severest field-level lie.
- Law: `_leaf` is the equality the wire cares about — `fieldKind` plus the leaf identity: `ScalarType` for scalar arms, `qualifiedName` for message and enum leaves, the `mapKey` scalar plus value leaf for maps, the `listKind` leaf for lists; presentation facts (json name, comments) never enter the signature.
- Law: both directions run in one fold over the deduped union of field numbers — pinned-only numbers emit `FieldRemoved`, live-only numbers emit `FieldAdded`, shared numbers compare signatures then names.
- Boundary: change grading and the rank fold are `contract/drift.ts`'s vocabulary; the reflect-based content-key field walk over an admitted family is `codec/graph.ts`'s parity projection.

```typescript
import { qualifiedName, ScalarType, type DescField, type DescMessage } from "@bufbuild/protobuf"
import { Array, HashMap, Match, Option } from "effect"
import { ContractDrift } from "./drift.ts"

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

const _coord = (message: DescMessage, field: DescField): { message: string; field: string; number: number } => ({
  message: qualifiedName(message),
  field: field.name,
  number: field.number,
})

const _diffed = (pinned: DescMessage, live: DescMessage): ReadonlyArray<ContractDrift.Change> => {
  const before = HashMap.fromIterable(Array.map(pinned.fields, (field) => [field.number, field] as const))
  const after = HashMap.fromIterable(Array.map(live.fields, (field) => [field.number, field] as const))
  const numbers = Array.dedupe([...Array.map(pinned.fields, (field) => field.number), ...Array.map(live.fields, (field) => field.number)])
  return Array.filterMap(numbers, (number): Option.Option<ContractDrift.Change> =>
    Option.match(HashMap.get(before, number), {
      onNone: () =>
        Option.map(HashMap.get(after, number), (added) => ({ _tag: "FieldAdded" as const, at: _coord(live, added) })),
      onSome: (was) =>
        Option.match(HashMap.get(after, number), {
          onNone: () => Option.some({ _tag: "FieldRemoved" as const, at: _coord(pinned, was) }),
          onSome: (is) =>
            _leaf(was) === _leaf(is)
              ? Option.none()
              : was.name === is.name
                ? Option.some({ _tag: "TypeChanged" as const, at: _coord(live, is), from: _leaf(was), to: _leaf(is) })
                : Option.some({ _tag: "NumberReused" as const, at: _coord(live, is), retired: was.name }),
        }),
    }))
}
```

## [3]-[GATE_SERVICE]

- Owner: `DescriptorGate` — one `Effect.Service` whose Layer factory takes the live descriptor-set octets plus the generation label, decodes through the proto engine and the shipped `./wkt` schema, builds the `FileRegistry`, and folds one verdict per suite family at construction; the verdict census is immutable for the service's life.
- Entry: `DescriptorGate.verdict(family)` the per-family verdict read; `DescriptorGate.admitted(family)` the decode gate — `Effect<void, WireFault>` refusing `breaking` families with reason `drift`; `DescriptorGate.census` the whole verdict set for the CI artifact.
- Receipt: each verdict is a `ContractDrift` value carrying the pinned and live generation labels and every field change — the boot log, the CI artifact, and the refusal detail are all projections of one value.
- Growth: a second gated consumer composes `admitted` in its decode pipeline — one yield, zero gate edits; a new generation source (a registry endpoint instead of shipped bytes) is a new Layer factory shape at the app root, never a second gate.
- Law: coverage is the suite key tuple — `ProtoCodec.names` is census-guarded at `codec/proto.ts`, so iterating it IS iterating every gated census family; a suite family unresolved by `qualifiedName` in the live registry folds to a `FamilyMissing` breaking verdict rather than an absent census row, so silence cannot pass for compatibility. `FileDescriptorSetWire` never enters the census: it is the gate's own transport.
- Law: the pinned side is the generated suite itself — each `GenMessage` is a `DescMessage`, so the build artifact is the baseline and no second pinned descriptor file exists to drift from the code that decodes with it.
- Law: the gate decodes its own ingress through the one admission rail — `ProtoCodec.frame(FileDescriptorSetSchema)` with the message identity narrowed by `isMessage` — never a bare `fromBinary` outside the engine fold; a non-set payload at this seam is a wiring defect and dies, never a typed fault.
- Boundary: `codec/graph.ts` requires this service in `R` and yields `admitted("ElementGraphWire")` before decoding; the app root provides the Layer with the runtime-shipped set; verdict vocabulary and policy live at `contract/drift.ts`.

```typescript
import { createFileRegistry, isMessage, qualifiedName, type MessageShape } from "@bufbuild/protobuf"
import { FileDescriptorSetSchema } from "@bufbuild/protobuf/wkt"
import { Array, Effect, HashMap, Option, type ParseResult, Schema } from "effect"
import { ProtoCodec } from "../codec/proto.ts"
import { WireFault } from "../fault/quarantine.ts"
import { ContractDrift, type Inventory } from "./drift.ts"

const _PINNED = "<build-pin>"

const _decodeSet = (octets: Uint8Array): Effect.Effect<MessageShape<typeof FileDescriptorSetSchema>, ParseResult.ParseError> =>
  Schema.decodeUnknown(ProtoCodec.frame(FileDescriptorSetSchema))(octets).pipe(
    Effect.filterOrDieMessage(
      (message): message is MessageShape<typeof FileDescriptorSetSchema> => isMessage(message, FileDescriptorSetSchema),
      "<descriptor-set-identity>",
    ),
  )

class DescriptorGate extends Effect.Service<DescriptorGate>()("wire/DescriptorGate", {
  effect: (live: Uint8Array, generation: string) =>
    Effect.gen(function* () {
      const registry = createFileRegistry(yield* _decodeSet(live))
      const verdicts = Array.reduce(
        ProtoCodec.names,
        HashMap.empty<Inventory.Family, ContractDrift>(),
        (acc, family) => {
          const pinned = ProtoCodec.suite[family]
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
        verdict: (family: Inventory.Family): Option.Option<ContractDrift> => HashMap.get(verdicts, family),
        census: Effect.succeed(Array.fromIterable(HashMap.values(verdicts))),
        admitted: (family: Inventory.Family): Effect.Effect<void, WireFault> =>
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

export { DescriptorGate }
```
