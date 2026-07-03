# [WIRE_DRIFT]

`contract/drift.ts` is the folder registry and the drift-verdict vocabulary: one closed `as const` census of every C#-minted wire family the folder decodes — codec engine, C# mint source, owning page, downstream consumer per row — and the three-verdict `ContractDrift` family the descriptor gate emits over it. Every codec page is a row of the census before it is a module; the descriptor gate proves its coverage against the census; verdict rows carry admission policy so a `breaking` wire refuses decode as a value, never a runtime crash. The census and the verdict are the two vocabularies every other `wire` page consumes as settled law.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                                       |
| :-----: | :--------------- | :------------------------------------------------------------------------------------------- |
|   [1]   | `WIRE_INVENTORY` | the closed census of C#-minted wire families; codec/source/page/consumer rows                 |
|   [2]   | `DRIFT_VERDICT`  | the `identical`/`additive`/`breaking` policy vocabulary + the `ContractDrift` verdict family  |

## [2]-[WIRE_INVENTORY]

- Owner: `Inventory` — one exported assembled owner; the `_families` key tuple anchors wire order and the `Schema.Literal` spread, the `_rows` table anchors every census fact, and the merged-hub guard pair ties tuple and table closed in both directions.
- Entry: `Inventory[family]` row lookup; `Inventory.wire` the family-name schema every sibling page types `family` fields with; `Inventory.of("<codec>")` the per-codec key projection, whose type-level twin `Inventory.Of` anchors `ProtoCodec.Suite` and through it the descriptor gate's coverage walk.
- Growth: a new C# wire family is one tuple entry plus one row — the row names its codec, mint source, owning page, and consumer, and the guards break every stale projection at compile time. Never a new folder, never a parallel list.
- Law: the census is the folder's single source of truth for which wire families exist; a codec page decoding a family absent from the census, or a census row with no owning page, is the defect the descriptor coverage check surfaces.
- Law: `codec` is a closed four-value axis — `proto`, `cbor`, `msgpack`, `jsonpatch` — one codec per C# mint format; a family appearing under two codecs is a census error, never a dispatch case.
- Law: `consumer` names the folder that reads the decoded value through `#vocab` or an app-root port — `kernel` and `state` rows decode INTO that folder's vocabulary, every other row lands a wire-owned decoded shape.
- Boundary: verdict computation over proto families is `contract/descriptor.ts`; this module owns the vocabulary the gate reads and writes.

```typescript
import { Array, Schema } from "effect"

const _codecs = ["proto", "cbor", "msgpack", "jsonpatch"] as const

const _families = [
  "ReceiptEnvelopeWire", "HlcStampWire", "TenantContextWire", "RenderReceiptWire",
  "FaultDetailWire", "QuantityWire",
  "ElementGraphWire", "NodeWire", "RelationshipWire",
  "OpLogWire", "SnapshotHeader", "CrdtOpWire",
  "CommitWire", "BranchWire", "VersionVectorWire", "MerkleSummaryWire",
  "JsonPatchDocument", "ProgressMarkWire", "CredentialPemWire",
  "BenchmarkClaimWire", "HostFingerprintWire",
  "BindingStatusWire", "CoercedValueWire", "WriteReceiptWire",
  "FlagVerdictWire", "ControlIntentWire", "LayoutConstraintWire",
  "BcfTopicWire", "BcfViewpointWire", "GeoFeatureWire",
  "BimWire", "DiffWire", "IdsAuditWire",
  "MaterialWire", "OpenPbrGroupsWire", "AppearanceSummaryWire",
  "ArtifactFrameWire", "GeometryPayloadWire", "GeometryResidencyWire",
  "CommandPayloadWire", "SupportCaptureWire", "CapabilityDescriptorWire",
  "FileDescriptorSetWire",
] as const

const _rows = {
  ReceiptEnvelopeWire: { codec: "proto", source: "Rasm.AppHost", page: "codec/envelope", consumer: "state" },
  HlcStampWire: { codec: "proto", source: "Rasm.AppHost", page: "codec/envelope", consumer: "kernel" },
  TenantContextWire: { codec: "proto", source: "Rasm.AppHost", page: "codec/envelope", consumer: "kernel" },
  RenderReceiptWire: { codec: "proto", source: "Rasm.AppUi/Render", page: "codec/envelope", consumer: "ui" },
  FaultDetailWire: { codec: "proto", source: "Rasm.Compute/Runtime", page: "fault/detail", consumer: "wire" },
  QuantityWire: { codec: "proto", source: "Rasm.Compute", page: "codec/proto", consumer: "kernel" },
  ElementGraphWire: { codec: "proto", source: "Rasm.Element/Graph", page: "codec/graph", consumer: "ui" },
  NodeWire: { codec: "proto", source: "Rasm.Element/Graph", page: "codec/graph", consumer: "ui" },
  RelationshipWire: { codec: "proto", source: "Rasm.Element/Graph", page: "codec/graph", consumer: "ui" },
  OpLogWire: { codec: "msgpack", source: "Rasm.Persistence", page: "codec/oplog", consumer: "store" },
  SnapshotHeader: { codec: "cbor", source: "Rasm.Persistence/Element", page: "codec/snapshot", consumer: "store" },
  CrdtOpWire: { codec: "msgpack", source: "Rasm.Persistence/Version", page: "codec/crdt", consumer: "state" },
  CommitWire: { codec: "msgpack", source: "Rasm.Persistence/Version", page: "codec/version", consumer: "state" },
  BranchWire: { codec: "msgpack", source: "Rasm.Persistence/Version", page: "codec/version", consumer: "state" },
  VersionVectorWire: { codec: "msgpack", source: "Rasm.Persistence/Version", page: "codec/version", consumer: "state" },
  MerkleSummaryWire: { codec: "msgpack", source: "Rasm.Persistence/Version", page: "codec/version", consumer: "state" },
  JsonPatchDocument: { codec: "jsonpatch", source: "Rasm.Persistence/Version", page: "codec/patch", consumer: "wire" },
  ProgressMarkWire: { codec: "proto", source: "Rasm.Compute/Runtime", page: "codec/progress", consumer: "state" },
  CredentialPemWire: { codec: "proto", source: "Rasm.AppHost", page: "codec/credential", consumer: "security" },
  BenchmarkClaimWire: { codec: "proto", source: "Rasm.AppHost/Observability", page: "codec/claim", consumer: "ui" },
  HostFingerprintWire: { codec: "proto", source: "Rasm.AppHost/Observability", page: "codec/claim", consumer: "ui" },
  BindingStatusWire: { codec: "proto", source: "Rasm.AppHost/Wire", page: "codec/livewire", consumer: "ui" },
  CoercedValueWire: { codec: "proto", source: "Rasm.AppHost/Wire", page: "codec/livewire", consumer: "ui" },
  WriteReceiptWire: { codec: "proto", source: "Rasm.AppHost/Wire", page: "codec/livewire", consumer: "ui" },
  FlagVerdictWire: { codec: "proto", source: "Rasm.AppHost", page: "codec/flag", consumer: "host" },
  ControlIntentWire: { codec: "proto", source: "Rasm.AppUi/Shell", page: "codec/control", consumer: "ui" },
  LayoutConstraintWire: { codec: "proto", source: "Rasm.AppUi/Shell", page: "codec/layout", consumer: "ui" },
  BcfTopicWire: { codec: "proto", source: "Rasm.Bim", page: "codec/bcf", consumer: "ui" },
  BcfViewpointWire: { codec: "proto", source: "Rasm.Bim", page: "codec/bcf", consumer: "ui" },
  GeoFeatureWire: { codec: "proto", source: "Rasm.Bim/Semantics", page: "codec/geo", consumer: "ui" },
  BimWire: { codec: "proto", source: "Rasm.Bim/Exchange", page: "codec/bim", consumer: "ui" },
  DiffWire: { codec: "proto", source: "Rasm.Bim/Exchange", page: "codec/bim", consumer: "ui" },
  IdsAuditWire: { codec: "proto", source: "Rasm.Bim/Exchange", page: "codec/bim", consumer: "ui" },
  MaterialWire: { codec: "proto", source: "Rasm.Materials/Appearance", page: "codec/appearance", consumer: "ui" },
  OpenPbrGroupsWire: { codec: "proto", source: "Rasm.Materials/Appearance", page: "codec/appearance", consumer: "ui" },
  AppearanceSummaryWire: { codec: "proto", source: "Rasm.Materials/Appearance", page: "codec/appearance", consumer: "ui" },
  ArtifactFrameWire: { codec: "proto", source: "Rasm.Compute/Runtime", page: "frame/artifact", consumer: "browser" },
  GeometryPayloadWire: { codec: "proto", source: "Rasm.Compute/Runtime", page: "frame/geometry", consumer: "ui" },
  GeometryResidencyWire: { codec: "proto", source: "Rasm.AppUi/Render", page: "frame/residency", consumer: "browser" },
  CommandPayloadWire: { codec: "proto", source: "Rasm.AppUi/Shell", page: "gateway/command", consumer: "wire" },
  SupportCaptureWire: { codec: "proto", source: "Rasm.AppHost", page: "gateway/support", consumer: "telemetry" },
  CapabilityDescriptorWire: { codec: "proto", source: "Rasm.AppHost/Agent", page: "invoke/capability", consumer: "wire" },
  FileDescriptorSetWire: { codec: "proto", source: "Rasm.Compute/Runtime", page: "contract/descriptor", consumer: "wire" },
} as const

const _wire: Schema.Literal<Inventory.Families> = Schema.Literal(..._families)

declare namespace Inventory {
  type Codec = (typeof _codecs)[number]
  type Families = typeof _families
  type Family = keyof typeof _rows
  type Row = { readonly codec: Codec; readonly source: string; readonly page: string; readonly consumer: string }
  type Of<C extends Codec> = { readonly [K in Family as (typeof _rows)[K]["codec"] extends C ? K : never]: (typeof _rows)[K] }
  type Shape = typeof _rows & {
    readonly codecs: typeof _codecs
    readonly families: Families
    readonly wire: Schema.Literal<Families>
    readonly of: <C extends Codec>(codec: C) => ReadonlyArray<Family>
  }
  type _Rows<T extends Record<Family, Row> = typeof _rows> = T
  type _Keys<K extends Family = Families[number]> = K
}

const Inventory: Inventory.Shape = {
  ..._rows,
  codecs: _codecs,
  families: _families,
  wire: _wire,
  of: <C extends Inventory.Codec>(codec: C): ReadonlyArray<Inventory.Family> =>
    Array.filter(_families, (family) => _rows[family].codec === codec),
}
```

## [3]-[DRIFT_VERDICT]

- Owner: `ContractDrift` — one `Schema.Class` verdict receipt over one `_severity` policy table; the change evidence rides the value as typed `DriftChange` union rows, never a message string.
- Entry: `ContractDrift.dominant` folds a family's change set to one verdict through the `_grade` lookup and the rank lattice; `verdict.admitted`/`verdict.alarm` read the policy row so decode gating and alerting are projections, never re-derived branches.
- Receipt: a verdict value carries the family, the pinned and live generation coordinates it compared, and every field-level change — the CI report, the decode gate, and the quarantine detail all read the same value.
- Growth: a new change kind is one `DriftChange` case plus its severity row in `_grade`; a new verdict policy axis is a `_severity` column. Exhaustive consumers break loudly at the missing arm.
- Law: verdict admission is data — `identical` and `additive` admit decode, `breaking` refuses it; the refusal surfaces as a `WireFault` with reason `drift` at the consuming codec page, never as a `ParseError` mid-decode.
- Law: severity folds by the rank lattice — one `breaking` change makes the family `breaking` regardless of additive siblings; `dominant` is a vocabulary lookup per change plus `Array.max` over the rank `Order`, and `identical` is exactly the empty change set.
- Law: the verdict family is `Schema`-declared because verdicts serialize into CI artifacts and cross the reporting boundary; a process-local re-model of the verdict is the parallel-shape defect.
- Boundary: computing changes from two `FileRegistry` walks is `contract/descriptor.ts`; `codec/patch.ts` surfaces an out-of-vocabulary RFC 6902 op through this same verdict vocabulary; the `WireFault` reason row that carries a refusal is `fault/quarantine.ts`.

```typescript
import { Array, Order, Schema } from "effect"

const _severity = {
  identical: { rank: 0, admitted: true, alarm: false },
  additive: { rank: 1, admitted: true, alarm: false },
  breaking: { rank: 2, admitted: false, alarm: true },
} as const

const _grade = {
  FieldAdded: "additive",
  EnumValueAdded: "additive",
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

const _Change = Schema.Union(
  Schema.TaggedStruct("FieldAdded", { at: _FieldCoord }),
  Schema.TaggedStruct("EnumValueAdded", { at: _FieldCoord }),
  Schema.TaggedStruct("FieldRemoved", { at: _FieldCoord }),
  Schema.TaggedStruct("TypeChanged", { at: _FieldCoord, from: Schema.NonEmptyString, to: Schema.NonEmptyString }),
  Schema.TaggedStruct("WireTypeChanged", { at: _FieldCoord, from: Schema.Int, to: Schema.Int }),
  Schema.TaggedStruct("NumberReused", { at: _FieldCoord, retired: Schema.NonEmptyString }),
  Schema.TaggedStruct("FamilyMissing", { family: _wire }),
)

class ContractDrift extends Schema.Class<ContractDrift>("ContractDrift")({
  family: _wire,
  verdict: Schema.Literal("identical", "additive", "breaking"),
  pinned: Schema.NonEmptyString,
  live: Schema.NonEmptyString,
  changes: Schema.Array(_Change),
}) {
  static readonly Change: typeof _Change = _Change
  static readonly rank: Order.Order<ContractDrift.Verdict> = Order.mapInput(Order.number, (verdict: ContractDrift.Verdict) => _severity[verdict].rank)
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

// --- [EXPORTS] --------------------------------------------------------------------------

export { ContractDrift, Inventory }
```
