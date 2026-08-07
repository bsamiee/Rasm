# [CORE_CODEC]

Keyed decoding drives the interchange plane: ONE closed census of every contract wire family — arm, mint source, consuming surface, and owning page as data columns — and ONE polymorphic registry whose mapped landing table resolves each codec-homed family to its decoded type, so `Wire.decode(family, octets)` is the single decode entry the branch owns and a per-family codec page is unspellable. Families landing in core vocabulary decode INTO the `value`/`state` owners with zero local twins; families whose consumers live outside core land wire-owned shapes declared here once, adopted-verbatim on the decode-boundary names the producing peer mints. Beside the registry sit the four cross-cutting mechanics every row shares, each spelled once: the reason-discriminated `WireFault` rail with its bounded, replayable poison quarantine; the `Parity` combinator family — content-key mint-and-compare, golden-byte roundtrip, and the reflection walk over key cells; the divert-and-dedup `feed` combinator whose per-family transition policy is a row; and the sequence-gap Mealy the oplog watermark and the frame ordinal chain both mint evidence through. Module `core/src/interchange/codec.ts` owns it; a new wire family is one census row with one landing row, a new failure cause is one policy row, and a new feed is one `_feeds` row — never a sibling page, never a second rail.

## [01]-[INDEX]

- [02]-[WIRE_CENSUS]: the closed family tuple, arm/source/consumer/home columns, the wire literal; `Wire` (census reads).
- [03]-[FAULT_RAIL]: `WireFault` policy table, poison intake, budgeted replay, the divert; `WireFault`, `Quarantine`.
- [04]-[PARITY_VERIFY]: content-key mint delegate, verify, roundtrip, the key-cell reflection walk; `Parity`.
- [05]-[LANDING_EVIDENCE]: evidence/identity/version landings into core vocabulary + the CRDT op union; `CrdtOp`, `OpLog` shapes.
- [06]-[LANDING_WIRE]: wire-owned decoded shapes for later-wave consumers; landing classes on `Wire`.
- [07]-[KEYED_REGISTRY]: the mapped landing table, the polymorphic decode/encode/stream entrypoints; `Wire`.
- [08]-[FEED_DEDUP]: the divert+dedup stream combinator and its per-family policy rows; `feed`.
- [09]-[SEQUENCE_GAP]: the gap Mealy, the resumable oplog stream, the frontier read; `Gap`, `OpLog`.

## [02]-[WIRE_CENSUS]

[WIRE_CENSUS]:
- Owner: the census anchors — `_families`, the ordered key tuple of every contract wire family; `_census`, the fact table carrying `arm` (the closed five-value format axis), `source` (the peer mint whose bytes this landing decodes), `consumer` (the surface reading the decoded value: `value`, `state`, `observe`, `interchange`, `security`, `data`, `runtime`, `ui`), and `home` (the interchange page owning the landing: `codec`, `format`, `contract`, `frame`, `invoke`); `_wireLiteral`, the family-name schema every fault, feed, and verdict types `family` fields with. Merged-hub guard pairs tie tuple and table closed in both directions, and the `Home`/`Consumer` type anchors govern the fact columns — a census row naming a page or surface outside either closed set fails the row guard, never a review.
- Law: the census is the plane's single source of truth for which wire families exist — a decode surface for a family absent from the census, or a census row with no landing at its home page, is the defect the contract gate's coverage walk surfaces.
- Law: `arm` is closed at five — `proto`, `cbor`, `msgpack`, `jsonpatch`, `json` — one arm per peer mint format, and the arm fixes which engine the family's byte schema composes, so a row's arm and its `_schemas` entry are one fact spelled twice and cannot disagree; a family under two arms is a census error, never a dispatch case.
- Law: `json` is the AppHost mint — its runtime-evidence roster crosses as source-generated `System.Text.Json` under `JsonSerializerDefaults.Web`, one `[JsonSerializable]` row per family — so those rows compose `Json.schema` and owe no descriptor source. A generated descriptor may still stand beside such a row for this branch's own `-bin` carriage, so the `arm` column is what every decode path reads while the suite roster narrows a key it never decides; probing the roster instead is what leaves a protobuf reader standing under a JSON producer with both ends reading correct.
- Law: `source` names where a decoded value came from, never who owns the shape — a family whose `tests/contracts/` entry classes it `infrastructure` is co-minted, so this branch mints it locally as well and the column still records the peer whose bytes cross; reading the column as sole authorship is what lets two ends drift into incompatible shapes under one family name.
- Law: `source` prints the peer's own identity — a C# project path stands bare, every other estate qualifies with its language (`python:artifacts/graphic/texture`) — so one column addresses every producing estate and a second producer axis beside it is unspellable.
- Law: `home` partitions the landing obligation — `codec` rows resolve in this module's landing table; `format`, `contract`, `frame`, and `invoke` rows land at their owning page, and the census still names them so coverage is one walk over one tuple.
- Growth: a new peer wire family — C#-minted or python-minted alike — is one tuple entry, one census row, and one landing row at its home; the guards break every stale projection at compile time; never a new folder, never a parallel list.
- Boundary: verdict grading over descriptor generations is the contract page's; the proto `GenMessage` suite the census's proto rows bind is `format#PROTO_ENGINE`'s.
- Packages: `effect` (`Schema`, `Array`).

```typescript signature
import { Array, type ParseResult, Schema, type Types } from "effect"

const _arms = ["proto", "cbor", "msgpack", "jsonpatch", "json"] as const

const _families = [
  "ReceiptEnvelopeWire", "HlcStampWire", "TenantContextWire", "CommandAvailabilityWire",
  "FaultDetailWire", "QuantityWire",
  "ElementGraphWire", "GraphDeltaWire", "NodeWire", "RelationshipWire",
  "OpLogWire", "SnapshotHeader", "CrdtOpWire",
  "CommitWire", "BranchWire", "VersionVectorWire", "MerkleSummaryWire",
  "JsonPatchDocument", "ProgressMarkWire", "CredentialPemWire",
  "BenchmarkClaimWire", "HostFingerprintWire",
  "BindingStatusWire", "CoercedValueWire", "WriteReceiptWire",
  "FlagVerdictWire", "ControlIntentWire", "LayoutConstraintWire", "CommandGateWire", "EvidenceTimelineWire",
  "BcfTopicWire", "BcfViewpointWire", "GeoFeatureWire",
  "BimWire", "DiffWire", "IdsAuditWire", "PredicateWire",
  "MaterialWire", "OpenPbrGroupsWire", "TextureSetWire", "AssetSetManifest",
  "ArtifactFrameWire", "GeometryPayloadWire", "GeometryResidencyWire",
  "CommandPayloadWire", "SupportCaptureWire", "CapabilityDescriptorWire",
  "FileDescriptorSetWire",
] as const

const _census = {
  // The AppHost runtime-evidence set crosses as source-generated System.Text.Json, so every `Rasm.AppHost`-sourced row
  // below carries the `json` arm and composes `Json.schema`. `tests/contracts/MANIFEST.md` [02.21] registers the family
  // set on the `apphost-wire` seam against the producer roster at `csharp:Rasm.AppHost/Runtime/ports#WIRE_LAW` and
  // states outright that no descriptor source exists or is owed under [02.9]. Families the entry defers to a sibling
  // cite that sibling: `HlcStampWire` rides [02.7]'s two-half layout, `CapabilityDescriptorWire` [02.12],
  // `HostFingerprintWire` [02.15], and `BenchmarkClaimWire` [02.14] — whose C# minter is
  // `csharp:Rasm.Compute/Runtime/receipts#TS_PROJECTION`, so that row sources Compute and keeps the proto arm its
  // minter writes; an AppHost source there mis-names the producer. The AppHost `DegradationLevel` grade is no family of
  // its own: `CommandAvailabilityWire` is its one frozen name — the level beside the per-command verdict the
  // `Availability` landing decodes — so a second degradation row would mint a crossing [02.21] never registered.
  ReceiptEnvelopeWire: { arm: "json", source: "Rasm.AppHost", consumer: "state", home: "codec" },
  HlcStampWire: { arm: "json", source: "Rasm.AppHost", consumer: "value", home: "codec" },
  TenantContextWire: { arm: "json", source: "Rasm.AppHost", consumer: "value", home: "codec" },
  CommandAvailabilityWire: { arm: "json", source: "Rasm.AppHost/Observability", consumer: "state", home: "codec" },
  FaultDetailWire: { arm: "proto", source: "Rasm.Compute/Runtime", consumer: "interchange", home: "codec" },
  QuantityWire: { arm: "proto", source: "Rasm.Compute", consumer: "value", home: "codec" },
  // The `rasm.element.v1` crossing set is exactly four: the snapshot envelope, the delta the `delta#GRAPH_DELTA`
  // event body carries, and the two element messages the producer's own decode legs re-admit one at a time. Every
  // OTHER message in that contract is presence on an owner and earns NO census row, because no producer emits one as
  // a standalone document — `HeaderWire` on both envelopes, `GeoReferenceWire`/`StepHeaderWire`/`ProjectedCrsWire`
  // under it, `PlacementWire` on `ObjectWire` (field 12) inside the `NodeWire` oneof, `RedactionManifestWire` on
  // `ElementGraphWire` (field 4), `NodeRevisionWire` on `GraphDeltaWire` (field 3), and the
  // `MaterialUsageWire`/`MeasureValueWire`/`MeasureBandWire` payloads the associate edge reaches — so each mirrors at
  // `[06]`'s graph owner beside the family that carries it while the crossing stays these four names.
  ElementGraphWire: { arm: "proto", source: "Rasm.Element/Graph", consumer: "ui", home: "codec" },
  // The delta lands the same consumer as the snapshot it revises: a folded change record and the graph it folds onto
  // are one reader, and a second consumer here would claim a surface no fold reaches.
  GraphDeltaWire: { arm: "proto", source: "Rasm.Element/Graph", consumer: "ui", home: "codec" },
  NodeWire: { arm: "proto", source: "Rasm.Element/Graph", consumer: "ui", home: "codec" },
  RelationshipWire: { arm: "proto", source: "Rasm.Element/Graph", consumer: "ui", home: "codec" },
  OpLogWire: { arm: "msgpack", source: "Rasm.Persistence", consumer: "data", home: "codec" },
  SnapshotHeader: { arm: "cbor", source: "Rasm.Persistence/Element", consumer: "data", home: "codec" },
  CrdtOpWire: { arm: "msgpack", source: "Rasm.Persistence/Version", consumer: "state", home: "codec" },
  CommitWire: { arm: "msgpack", source: "Rasm.Persistence/Version", consumer: "state", home: "codec" },
  BranchWire: { arm: "msgpack", source: "Rasm.Persistence/Version", consumer: "state", home: "codec" },
  VersionVectorWire: { arm: "msgpack", source: "Rasm.Persistence/Version", consumer: "state", home: "codec" },
  MerkleSummaryWire: { arm: "msgpack", source: "Rasm.Persistence/Version", consumer: "state", home: "codec" },
  JsonPatchDocument: { arm: "jsonpatch", source: "Rasm.Persistence/Version", consumer: "data", home: "format" },
  ProgressMarkWire: { arm: "proto", source: "Rasm.Compute/Runtime", consumer: "state", home: "codec" },
  CredentialPemWire: { arm: "json", source: "Rasm.AppHost/Runtime", consumer: "security", home: "codec" },
  BenchmarkClaimWire: { arm: "proto", source: "Rasm.Compute/Runtime", consumer: "ui", home: "codec" },
  HostFingerprintWire: { arm: "json", source: "Rasm.AppHost/Runtime", consumer: "ui", home: "codec" },
  BindingStatusWire: { arm: "json", source: "Rasm.AppHost/Wire", consumer: "ui", home: "codec" },
  CoercedValueWire: { arm: "json", source: "Rasm.AppHost/Wire", consumer: "ui", home: "codec" },
  WriteReceiptWire: { arm: "json", source: "Rasm.AppHost/Wire", consumer: "ui", home: "codec" },
  FlagVerdictWire: { arm: "json", source: "Rasm.AppHost/Runtime", consumer: "runtime", home: "codec" },
  // The AppUi product-shell set crosses as source-generated System.Text.Json under the producer's own camelCase
  // Strict wire law (`AppUiWireContext`, one `[JsonSerializable]` row per family), so every `Rasm.AppUi`-sourced
  // row here carries the `json` arm and composes `Json.schema`. `tests/contracts/MANIFEST.md` [02.22] registers
  // the family set on the `appui-wire` seam and states outright that no descriptor source exists or is owed
  // under [02.9]: `rasm/channels.proto` declares the texture messages alone, and no AppUi page mints a
  // descriptor of its own, so a `proto` arm over these families named a generated schema that cannot exist and
  // the landing could never decode the producer's actual bytes — the identical mis-cut [02.21] already settled
  // for the AppHost set. `EvidenceTimelineWire` is the diagnostics crossing: a decoded wire family, never the
  // process-local `state/feed` composition, so it earns a census row like every other name a seam kinds `[WIRE]`.
  ControlIntentWire: { arm: "json", source: "Rasm.AppUi/Shell", consumer: "ui", home: "codec" },
  LayoutConstraintWire: { arm: "json", source: "Rasm.AppUi/Shell", consumer: "ui", home: "codec" },
  CommandGateWire: { arm: "json", source: "Rasm.AppUi/Shell", consumer: "ui", home: "codec" },
  EvidenceTimelineWire: { arm: "json", source: "Rasm.AppUi/Diagnostics", consumer: "ui", home: "codec" },
  BcfTopicWire: { arm: "proto", source: "Rasm.Bim", consumer: "ui", home: "codec" },
  BcfViewpointWire: { arm: "proto", source: "Rasm.Bim", consumer: "ui", home: "codec" },
  GeoFeatureWire: { arm: "proto", source: "Rasm.Bim/Semantics", consumer: "ui", home: "codec" },
  BimWire: { arm: "proto", source: "Rasm.Bim/Exchange", consumer: "ui", home: "codec" },
  DiffWire: { arm: "proto", source: "Rasm.Bim/Exchange", consumer: "ui", home: "codec" },
  IdsAuditWire: { arm: "proto", source: "Rasm.Bim/Exchange", consumer: "ui", home: "codec" },
  // `PredicateWire` stands as the one `Rasm.Bim` row off the proto arm: its producer's contract IS a
  // `[JsonPolymorphic]` discriminated record family (`arm` the discriminator column), so no `.proto` declares it and a
  // proto row here would decode bytes the mint never writes. It is also this plane's one CO-MINTED family — a browser
  // filter builder authors the arms `PredicateCodec.Admit` re-admits — so `source` records whose evaluation the wire
  // feeds, exactly as the co-mint law reads it, and this landing is egress-legal rather than decode-only.
  PredicateWire: { arm: "json", source: "Rasm.Bim/Model", consumer: "ui", home: "codec" },
  // The appearance families are MessagePack integer-keyed rosters mirrored field-for-field at every peer —
  // `tests/contracts/MANIFEST.md` [02.9] forecloses a proto declaration beside them ("a second schema for one
  // wire"), and `rasm/channels.proto` declares no message for either, so a proto arm here bound a generated
  // schema that cannot exist and the landing could never decode the producer's actual bytes.
  // The rosters are POSITIONAL: `[Key(n)]` is the array index, so the landings decode through the `[06]`
  // wire-twin tuples under the [MIRROR_ORDER] law, and the vector family crosses NESTED at `MaterialWire`
  // Key(1) — the producer's codec is `IAppearanceWire`-generic and emits no standalone vector document, so the
  // `OpenPbrGroupsWire` row binds the same nested declaration rather than a second schema. The seam SUMMARY is
  // NOT a family here: no producer emits a standalone summary document — its one wire leg is the
  // `rasm.element.v1` `AppearanceWire` payload nested at `NodeWire` field 7, which the `NodeWire` landing carries
  // whole and untyped, so `AppearanceSummary` below mirrors that nested payload and earns no census row.
  MaterialWire: { arm: "msgpack", source: "Rasm.Materials/Appearance", consumer: "ui", home: "codec" },
  OpenPbrGroupsWire: { arm: "msgpack", source: "Rasm.Materials/Appearance", consumer: "ui", home: "codec" },
  TextureSetWire: { arm: "proto", source: "Rasm.Materials/Raster", consumer: "ui", home: "codec" },
  AssetSetManifest: { arm: "proto", source: "python:artifacts/graphic/texture", consumer: "ui", home: "codec" },
  ArtifactFrameWire: { arm: "proto", source: "Rasm.Compute/Runtime", consumer: "runtime", home: "frame" },
  GeometryPayloadWire: { arm: "proto", source: "Rasm.Compute/Runtime", consumer: "ui", home: "frame" },
  GeometryResidencyWire: { arm: "json", source: "Rasm.AppUi/Render", consumer: "runtime", home: "frame" },
  CommandPayloadWire: { arm: "json", source: "Rasm.AppUi/Shell", consumer: "interchange", home: "invoke" },
  SupportCaptureWire: { arm: "json", source: "Rasm.AppHost/Observability", consumer: "ui", home: "codec" },
  CapabilityDescriptorWire: { arm: "json", source: "Rasm.AppHost/Agent", consumer: "interchange", home: "invoke" },
  FileDescriptorSetWire: { arm: "proto", source: "Rasm.Compute/Runtime", consumer: "interchange", home: "contract" },
} as const

const _wireLiteral: Schema.Literal<Wire.Families> = Schema.Literal(..._families)
```

## [03]-[FAULT_RAIL]

[FAULT_RAIL]:
- Owner: `WireFault`, the one reason-discriminated `Schema.TaggedError` for the whole plane over a `FaultClass.family` mint — the branch class per reason beside the plane's own retention and replay dispositions — with `fromSlot`, the patch-arm slot triage, riding it as a static; and `Quarantine`, the `Effect.Service` owning the bounded poison intake, the held-frame census, the held-frame `diagnostic` render, the budgeted replay drain, and the `divert` dual transformer every decode surface composes; `PoisonFrame` rides the service as `Quarantine.Frame`, and the divert is the branch's `rasm.core.interchange.quarantine` tap point — the `observe/tap` name row a subscription observes held-frame evidence through.
- Law: the family is sized by routing — every consumer routes on the one tag and reads `reason`/`family` as evidence; a per-cause class or a second plane fault family is the named spam defect, and `family` is typed by the census literal so an unnamed family is a compile error.
- Law: severity is the branch lattice, never a plane rank — `class` projects through the family mint and dominance is `Array.max` over `FaultClass.severity`, so a wire fault and a folder fault compare on ONE scale and the tuple that declares that scale is the only edit site. A local rank column reproduces the ordering in a second place where the two silently disagree, and the two columns that DO survive beside `class` are dispositions no class row answers: whether the frame is held, and whether re-decoding it can change the verdict.
- Law: evidence is data — `evidence` carries the `{ actual, expected }` pair for `stale`, `parity`, and `sequence`; `message` derives from fields and is never stored; classification of a `ParseError` into the family happens exactly once, at the intake seam where frame context exists to name `family` and `reason`.
- Law: `sequence` never quarantines — a gap has no frame to hold; `overrun` marks a pre-decode ceiling refusal the frame rail mints; engine-internal ceiling throws surface as `ParseError` and classify `malformed` at intake; a truncated size-delimited header triages through `format#PROTO_ENGINE`'s `peek` into `truncated`.
- Law: the patch arm's per-op error slots classify through `fromSlot` — `TestError` folds to `stale` carrying its `{ actual, expected }` pre-image evidence, `MissingError` to `conflict` naming the vanished path, and the residue to `malformed` — so the OCC refusal and the concurrent-edit divergence read as data on the one rail, and `stale`/`conflict` have exactly one mint site.
- Law: the intake is a bounded `TQueue` with transactional backpressure — a poison storm suspends its producer until replay takes capacity; `octets` arrive as a lazy thunk so the re-encode runs only on the failure path; `attempts` lives on the frame and replay re-enters a successor carrying `attempts + 1` with the original fault intact, so the terminal report names the first cause.
- Law: the held census is slot-keyed and settles — the service's transactional `TRef<bigint>` mints one collision-free `slot` per intake, replay successors retain that slot, and one STM transaction installs the held row and executes `TQueue.offer` against the bounded intake atomically, so simultaneous same-family frames cannot overwrite one another and a replay take cannot settle a frame before its census row exists. Delivered and retired frames remove the row transactionally, so `census` reads the live poison set exactly and the table cannot grow past the frames still owed a verdict; `release` removes the same slot, and replay rechecks membership immediately before decode so a queued foreign eviction cannot deliver later.
- Law: replay is generic over every row — the drain takes the family-keyed decode as a parameter, so the service imports no landing and the app root supplies the record it composed from `Wire.decode`; the pump cadence is unbounded `spaced` because the per-frame `attempts` budget is the bound, and the drain suspends on an empty intake rather than polling.
- Law: the drain grades the whole `Exit`, never the typed channel alone — the replay decode reifies through `Effect.exit` and folds through `Exit.match` interrupt-first: an interrupted attempt decided nothing and re-admits the frame unchanged, a typed failure re-admits a successor at `attempts + 1`, and a DEFECT retires the frame. `Effect.matchEffect` sees `E` alone, so one `Effect.orDie` or rejected promise on any landing's decode path would escape the fold, kill the enclosing `Effect.forEach`, and terminate the pump for the process lifetime — every held frame then keeps a census row it can never discharge, which is the unbounded growth the settle law forecloses.
- Law: a held frame is readable evidence, not opaque bytes — `diagnostic` renders a proto-armed frame through the family's own suite row and `toJsonString`, so the `rasm.core.interchange.quarantine` tap point and the operator report carry the decoded document beside the fault. The census `arm` decides the render and the suite roster only narrows the key, because a json family may still hold a generated descriptor its own `-bin` carriage mints and a roster-only probe would render JSON octets through a protobuf reader; the drift-tolerant `_READ` posture keeps unknown fields in the proto rendering and a decode that fails a second time answers absence rather than a nested fault, while a `json`-armed frame renders its own held octets as text through `Json.text`, so the arm whose bytes are already the document answers even where the decode refused — the one diagnostic a binary arm structurally cannot give — and a family on neither arm renders nothing.
- Growth: a new failure cause is one `_policy` row carrying its class; a retention or per-family cap axis is one `_INTAKE` field.
- Boundary: the wire-crossed `FaultDetail` altitude is `[05]`'s landing — a local rail importing it for a local failure is the altitude defect; availability degradation under a poison storm is `state` vocabulary wired at the app root.
- Packages: `@bufbuild/protobuf` (`toJsonString`); `effect` (`Schema`, `Effect`, `STM`, `TMap`, `TQueue`, `TRef`, `Cause`, `Exit`, `HashMap`, `Chunk`, `DateTime`, `Schedule`, `Order`, `Array`, `Either`, `Function`, `Match`, `Option`); `rfc6902/patch` (`MissingError`, `TestError`); `./format.ts` (`Json`, `Proto`, `Patch`); `../value/fault.ts` (`FaultClass`).

```typescript signature
import { toJsonString } from "@bufbuild/protobuf"
import {
  Cause, Chunk, DateTime, Effect, Either, Exit, Function, HashMap, Match, Option, Order, pipe, Predicate, Schedule,
  STM, TMap, TQueue, TRef,
} from "effect"
import { MissingError, TestError } from "rfc6902/patch"
import { FaultClass } from "../value/fault.ts"
import { Json, Proto } from "./format.ts"
import type { Patch } from "./format.ts"

const _causes = ["malformed", "truncated", "overrun", "sequence", "parity", "drift", "stale", "conflict"] as const

// The plane's one family mint: `class` is the branch taxonomy every rail already grades and orders on, and the two
// columns beside it are genuinely this plane's — `held` whether the failing frame is RETAINED in the poison census
// (a frame-retention disposition, not the class table's repair-intake divert), `replayable` whether re-decoding the
// same octets can change the verdict at all (which no class-level retryability answers: unparseable bytes are
// non-retryable as transport and replayable as evidence the moment the producing peer is fixed). A local rank column
// beside `class` would fork the one severity lattice the branch tuple already declares.
const _policy = FaultClass.family(_causes, {
  malformed: { class: "malformed", held: true, replayable: true },
  truncated: { class: "malformed", held: true, replayable: true },
  overrun: { class: "exhausted", held: true, replayable: false },
  sequence: { class: "absent", held: false, replayable: false },
  parity: { class: "breached", held: true, replayable: false },
  drift: { class: "invalid", held: true, replayable: true },
  stale: { class: "conflicted", held: false, replayable: true },
  conflict: { class: "conflicted", held: false, replayable: true },
})

class WireFault extends Schema.TaggedError<WireFault>()("WireFault", {
  family: _wireLiteral,
  reason: _policy.schema,
  detail: Schema.NonEmptyString,
  evidence: Schema.optionalWith(Schema.Struct({ actual: Schema.Unknown, expected: Schema.Unknown }), { as: "Option" }),
}) {
  static readonly bySeverity: Order.Order<WireFault> = Order.mapInput(FaultClass.severity, (fault: WireFault) => fault.class)
  static readonly dominant = (faults: Array.NonEmptyReadonlyArray<WireFault>): WireFault =>
    Array.max(faults, WireFault.bySeverity)
  static readonly fromSlot = (family: Wire.Family, slot: Patch.Slot, at: number): WireFault =>
    Match.value(slot).pipe(
      Match.when(Match.instanceOf(TestError), (test) =>
        new WireFault({ family, reason: "stale", detail: `<test@${at}>`, evidence: Option.some({ actual: test.actual, expected: test.expected }) })),
      Match.when(Match.instanceOf(MissingError), (missing) =>
        new WireFault({ family, reason: "conflict", detail: `<missing@${at}:${missing.path}>`, evidence: Option.none() })),
      Match.orElse((residue) =>
        new WireFault({ family, reason: "malformed", detail: `<op@${at}:${String(residue)}>`, evidence: Option.none() })),
    )
  get class(): FaultClass.Kind {
    return _policy.classOf(this.reason)
  }
  get policy(): WireFault.Row {
    return _policy.rows[this.reason]
  }
  override get message(): string {
    return `<${this.family}:${this.reason}> ${this.detail}`
  }
}

declare namespace WireFault {
  type Reason = (typeof _policy.reasons)[number]
  type Row = (typeof _policy.rows)[Reason]
}

const _INTAKE = { capacity: 256, attempts: 3 } as const
const _REPLAY: Schedule.Schedule<number> = Schedule.spaced("30 seconds")

class PoisonFrame extends Schema.Class<PoisonFrame>("PoisonFrame")({
  slot: Schema.BigIntFromSelf,
  family: _wireLiteral,
  octets: Schema.Uint8ArrayFromSelf,
  fault: WireFault,
  at: Schema.DateTimeUtcFromSelf,
  attempts: Schema.Int.pipe(Schema.nonNegative()),
}) {
  get replayable(): boolean {
    return this.fault.policy.replayable && this.attempts < _INTAKE.attempts
  }
}

class Quarantine extends Effect.Service<Quarantine>()("@rasm/ts/core/Quarantine", {
  scoped: Effect.gen(function* () {
    const box = yield* STM.commit(TQueue.bounded<PoisonFrame>(_INTAKE.capacity))
    const held = yield* STM.commit(TMap.empty<bigint, PoisonFrame>())
    const serial = yield* STM.commit(TRef.make(0n))
    const admit = (frame: PoisonFrame): Effect.Effect<PoisonFrame> =>
      STM.commit(STM.gen(function* () {
        yield* TMap.set(held, frame.slot, frame)
        yield* TQueue.offer(box, frame)
        return frame
      }))
    const settled = (frame: PoisonFrame): Effect.Effect<void> => STM.commit(TMap.remove(held, frame.slot))
    const take = STM.commit(STM.gen(function* () {
      const first = yield* TQueue.take(box)
      const rest = yield* TQueue.takeAll(box)
      return Array.prepend(rest, first)
    }))
    return {
      intake: (family: Wire.Family, octets: Uint8Array, fault: WireFault) =>
        Effect.flatMap(DateTime.now, (now) =>
          STM.commit(STM.gen(function* () {
            const slot = yield* TRef.get(serial)
            yield* TRef.set(serial, slot + 1n)
            const frame = new PoisonFrame({ slot, family, octets, fault, at: now, attempts: 0 })
            yield* TMap.set(held, slot, frame)
            yield* TQueue.offer(box, frame)
            return frame
          }))),
      census: STM.commit(TMap.values(held)),
      // the arm decides and the roster only narrows the key: a json frame's held octets ARE the document, so the render
      // needs no decode and survives the malformed case, while a proto frame renders through its own suite row
      diagnostic: (frame: PoisonFrame): Option.Option<string> =>
        Option.match(
          Option.filter(
            Array.findFirst(Proto.names, (name) => name === frame.family),
            () => _census[frame.family].arm === "proto",
          ),
          {
            onNone: () => (_census[frame.family].arm === "json" ? Option.some(Json.text(frame.octets)) : Option.none()),
            onSome: (name) =>
              Option.map(
                Either.getRight(Schema.decodeUnknownEither(Proto.frame(Proto.suite[name]))(frame.octets)),
                (message) => toJsonString(Proto.suite[name], message, { prettySpaces: 2 }),
              ),
          },
        ),
      release: (frame: PoisonFrame) => settled(frame),
      replayed: <A, R>(
        decode: (family: Wire.Family, octets: Uint8Array) => Effect.Effect<A, WireFault, R>,
        delivered: (value: A) => Effect.Effect<void, never, R>,
        retired: (frame: PoisonFrame) => Effect.Effect<void, never, R>,
      ): Effect.Effect<void, never, R> =>
        Effect.flatMap(take, (frames) =>
          Effect.forEach(frames, (frame) =>
            Effect.flatMap(STM.commit(TMap.has(held, frame.slot)), (pending) =>
              !pending
                ? Effect.void
                : frame.replayable
                ? Effect.flatMap(
                    Effect.exit(decode(frame.family, frame.octets)),
                    Exit.match({
                      // interrupt-first: an interrupted attempt graded nothing, a typed failure spends one attempt,
                      // and a defect retires the frame instead of escaping the fold and killing the pump
                      onFailure: (cause) =>
                        Cause.isInterruptedOnly(cause)
                          ? Effect.asVoid(admit(frame))
                          : Option.isSome(Cause.failureOption(cause))
                          ? Effect.asVoid(admit(new PoisonFrame({ ...frame, attempts: frame.attempts + 1 })))
                          : Effect.andThen(retired(frame), settled(frame)),
                      onSuccess: (value: A) => Effect.andThen(delivered(value), settled(frame)),
                    }),
                  )
                : Effect.andThen(retired(frame), settled(frame))), { concurrency: 1, discard: true })).pipe(
          Effect.repeat(_REPLAY),
          Effect.asVoid,
        ),
    }
  }),
  accessors: true,
}) {
  static readonly Frame: typeof PoisonFrame = PoisonFrame
  static readonly divert: {
    (context: { readonly family: Wire.Family; readonly octets: () => Uint8Array }): <A, R>(
      self: Effect.Effect<A, WireFault, R>,
    ) => Effect.Effect<Either.Either<A, WireFault>, WireFault, R | Quarantine>
    <A, R>(
      self: Effect.Effect<A, WireFault, R>,
      context: { readonly family: Wire.Family; readonly octets: () => Uint8Array },
    ): Effect.Effect<Either.Either<A, WireFault>, WireFault, R | Quarantine>
  } = Function.dual(
    2,
    <A, R>(
      self: Effect.Effect<A, WireFault, R>,
      context: { readonly family: Wire.Family; readonly octets: () => Uint8Array },
    ): Effect.Effect<Either.Either<A, WireFault>, WireFault, R | Quarantine> =>
      self.pipe(
        Effect.map(Either.right),
        Effect.catchIf(
          (fault) => fault.policy.held,
          (fault) => Effect.as(Quarantine.intake(context.family, context.octets(), fault), Either.left(fault)),
        ),
      ),
  )
}
```

## [04]-[PARITY_VERIFY]

[PARITY_VERIFY]:
- Owner: `Parity`, the one verify combinator family — `key(payload)` the delegated content mint, `verified(family, expected, payload)` the mint-and-compare gate every content-addressed row shares, `matched(family, actual, expected)` the pure key-pair gate for pre-minted comparisons, `roundtrip(family, schema, octets)` the golden-byte decode-encode-compare proof generic over any byte schema, and `cells(family, gen, fields)` the reflection walk extracting content-key byte cells off a decoded proto message for field-level parity.
- Law: the mint is delegated, never local — `Digest.mint("content", payload)` is the branch's one `XxHash128` seed-zero fold with the canonical `:x32` spelling, branded keys compare by bare `===`, and a second mint or normalize step anywhere on a verify path is the cross-language drift defect.
- Law: the payload is `Digest.Payload` — a whole buffer or a band iterable riding the mint's own chunk-walk modality — so a multi-frame artifact verifies over its held bands with no joined re-hash and a parity miss refuses before any summed allocation exists; the streaming lane is the same single delegation site, never a second verify.
- Law: a parity miss is evidence, not a crash — the fault holds both keys or, for the byte proof, both `{ extent, offset, byte }` observations at the first divergence, so an equal-length mismatch never reports two equal extents as if they explained the failure and the operator report and quarantine row read the disagreement as data.
- Law: the reflection walk ACCUMULATES, never short-circuits — both legs partition their roster through `Array.partitionMap`, so a resolve carrying three renamed fields and a read carrying three non-bytes cells each report their whole missing set in ONE `drift` fault rather than the first member; `Either.all` aborts at the first left, which makes the operator re-run the proof once per disagreement and turns the complete-roster claim into a claim the code does not keep.
- Law: a drift fault names its coordinate — `pathToString(buildPath(gen).field(field).toPath())` is the `detail` of every resolve and read refusal, so the field-mask address the walk already computes IS the evidence a report joins on and the returned `paths` array is the same addressing spelled once.
- Law: the read arm dispatches on `fieldKind` and reflects ONCE — `ReflectMessage.get` narrows its own return off the descriptor's discriminant, answering a `ReflectList` (itself an `Iterable`) on a list field and the scalar cell on a singular one, so one reflected view serves the whole roster and the list run folds into the cell array with no second accessor; without the list arm every repeated key column (`BimDiff.added`/`removed`, `IdsAudit` verdict anchors) fails as `drift` on a document that is exactly correct, and reflecting per field charges one view per roster entry over a document the walk already holds whole.
- Growth: a second content-keyed proto family composes `cells` with its own field roster — one call, zero new walks.
- Boundary: the `Digest` table, session algebra, and binary key twin are `value/contentKey.ts`'s; the frame rail's whole-artifact verify and the invoke page's descriptor admission compose `verified` and add nothing to it.
- Packages: `@bufbuild/protobuf` (`DescField`, `DescMessage`); `@bufbuild/protobuf/reflect` (`buildPath`, `pathToString`, `reflect`, `ReflectMessage`, `Path`); `effect` (`Schema`, `Effect`, `Either`, `Option`, `Array`); `../value/contentKey.ts` (`ContentKey`, `Digest`).

```typescript signature
import { type DescField, type DescMessage, type Message, type UnknownField } from "@bufbuild/protobuf"
import { buildPath, type Path, pathToString, reflect, type ReflectMessage } from "@bufbuild/protobuf/reflect"
import { ContentKey, Digest } from "../value/contentKey.ts"
import { Cbor, Pack, Proto } from "./format.ts"

const _mismatch = (family: Wire.Family, actual: unknown, expected: unknown, detail: string): WireFault =>
  new WireFault({ family, reason: "parity", detail, evidence: Option.some({ actual, expected }) })

// The field-mask address the walk already computes IS the refusal's coordinate, so `paths` and `detail` are one
// spelling and a report joins a drift row to the field it names without re-deriving the address.
const _addressed = (gen: DescMessage, field: DescField): string => pathToString(buildPath(gen).field(field).toPath())

const _drifted = (family: Wire.Family, coordinates: Array.NonEmptyReadonlyArray<string>): WireFault =>
  new WireFault({ family, reason: "drift", detail: `<fields:${Array.join(coordinates, ",")}>`, evidence: Option.none() })

// Repeated key columns are the roster families this walk exists for, so the read dispatches on the descriptor's own
// `fieldKind`: a list cell folds its whole run into the cell array through the list accessor, a singular cell
// contributes one. Reading every field through the singular accessor refuses a correct roster document as `drift`.
// The reflected view is the WALK's, never the field's: reflecting per field costs one view per roster entry on a
// document the walk already holds whole, so the read arm takes the view and dispatches on the descriptor's own kind.
const _keyCells = (view: ReflectMessage, field: DescField): Option.Option<ReadonlyArray<Uint8Array>> =>
  field.fieldKind === "list"
    ? Option.liftPredicate(
        Array.fromIterable(view.get(field)),
        (run): run is ReadonlyArray<Uint8Array> => Array.every(run, (cell) => cell instanceof Uint8Array),
      )
    : Option.map(
        Option.liftPredicate(view.get(field), (cell): cell is Uint8Array => cell instanceof Uint8Array),
        Array.of,
      )

const Parity: {
  readonly key: (payload: Digest.Payload) => Effect.Effect<ContentKey>
  readonly matched: (family: Wire.Family, actual: ContentKey, expected: ContentKey) => Effect.Effect<void, WireFault>
  readonly verified: (family: Wire.Family, expected: ContentKey, payload: Digest.Payload) => Effect.Effect<void, WireFault>
  readonly roundtrip: <A>(
    family: Wire.Family,
    schema: Schema.Schema<A, Uint8Array>,
    octets: Uint8Array,
  ) => Effect.Effect<void, ParseResult.ParseError | WireFault>
  readonly cells: (family: Wire.Family, gen: DescMessage, fields: ReadonlyArray<string>) => Either.Either<{
    readonly paths: ReadonlyArray<Path>
    readonly read: (octets: Uint8Array) => Either.Either<ReadonlyArray<Uint8Array>, ParseResult.ParseError | WireFault>
  }, WireFault>
} = {
  key: (payload) => Digest.mint("content", payload),
  matched: (family, actual, expected) =>
    actual === expected ? Effect.void : Effect.fail(_mismatch(family, actual, expected, "<key-mismatch>")),
  verified: (family, expected, payload) =>
    Effect.flatMap(Digest.mint("content", payload), (minted) => Parity.matched(family, minted, expected)),
  roundtrip: (family, schema, octets) =>
    Effect.gen(function* () {
      const decoded = yield* Schema.decodeUnknown(schema)(octets)
      const emitted = yield* Schema.encode(schema)(decoded)
      const mismatch = emitted.findIndex((byte, index) => byte !== octets[index])
      const offset = mismatch === -1 ? Math.min(emitted.length, octets.length) : mismatch
      return mismatch === -1 && emitted.length === octets.length
        ? undefined
        : yield* Effect.fail(_mismatch(
            family,
            { extent: emitted.length, offset, byte: emitted[offset] },
            { extent: octets.length, offset, byte: octets[offset] },
            "<golden-byte-divergence>",
          ))
    }),
  cells: (family, gen, fields) =>
    pipe(
      Array.partitionMap(fields, (name) =>
        Either.fromOption(Array.findFirst(gen.fields, (field) => field.name === name), () => name)),
      ([absent, resolved]: readonly [ReadonlyArray<string>, ReadonlyArray<DescField>]) =>
        Array.isNonEmptyReadonlyArray(absent)
          ? Either.left(_drifted(family, absent)) // the WHOLE unresolved roster in one fault: an aborting walk costs one re-run per renamed field
          : pipe(Proto.frame(gen), (framed) => // the byte schema and the field-mask addresses resolve ONCE per walk, never per read
            Either.right({
              paths: Array.map(resolved, (field) => buildPath(gen).field(field).toPath()),
              read: (octets: Uint8Array) =>
                Either.flatMap(Schema.decodeUnknownEither(framed)(octets), (message) =>
                  pipe(
                    reflect(gen, message), // one reflected view per document, shared across the whole roster read
                    (view) =>
                      Array.partitionMap(resolved, (field) =>
                        Either.fromOption(_keyCells(view, field), () => _addressed(gen, field))),
                    ([forked, runs]: readonly [ReadonlyArray<string>, ReadonlyArray<ReadonlyArray<Uint8Array>>]) =>
                      Array.isNonEmptyReadonlyArray(forked)
                        ? Either.left(_drifted(family, forked))
                        : Either.right(Array.flatten(runs)),
                  )),
            })),
    ),
}
```

## [05]-[LANDING_EVIDENCE]

[LANDING_EVIDENCE]:
- Owner: the core-landing rows and the CRDT op union — `ReceiptEnvelopeWire`, `HlcStampWire`, `TenantContextWire`, `CommandAvailabilityWire`, `QuantityWire`, `ProgressMarkWire` decode INTO `state`/`value` owners whole with zero local twins; `CommitWire`/`BranchWire`/`VersionVectorWire`/`MerkleSummaryWire` land the `state` version plane over the msgpack arm; `CrdtOp` is the tagged six-op journal union — `Assign`, `Adjoin`, `Retire`, `Splice`, `Tick`, and the `Alien` foreign-ext landing — whose `hlc` cells intern through the `format#MSGPACK_ENGINE` extension row and whose per-case merge instances bind at `state/merge.ts`'s algebra.
- Law: the typed families never erase — the envelope's `receipt` field decodes as `state`'s tagged receipt union with every kind distinct, the stamp decodes through the kernel `Hlc` class shape (physical half first, logical second), and `TenantContext` crosses verbatim as the one tenancy value; a flattened `{ kind, payload }` landing is the collapse defect.
- Law: nested case families carry their `_tag` on the C# emit — the receipt kinds and availability verdicts mint the discriminant wire-side as part of the adopted-verbatim contract, pinned by the roster-parity corpus fixtures; a nested family shipped untagged gains its discriminant at the landing exactly as `[06]`'s `_stamp` law spells.
- Law: a new receipt kind, availability level, or version-plane axis is a C# case with a `state` vocabulary row and zero edits here — the landings compose the sibling owners whole, so roster parity pins at this seam by construction.
- Law: `Tick.delta` is `bigint` — i64 counters ride the msgpack `useBigInt64` posture; a `Number`-typed delta is the precision defect.
- Law: an unregistered msgpack ext at op position lands the `Alien` case — `Pack.Alien` admits the engine's `ExtData` by identity, the transform carries ext type and cell verbatim, and `Pack.alien` re-mints on encode — so a newer peer's op family surfaces as typed contract-drift material the operator grades beside the descriptor verdicts, never a dropped byte and never a decode fault; a merge consumer treats `Alien` as a hold-and-report row, never a mergeable op.
- Boundary: merge lawfulness, convergence proofs, and the corpus fixtures binding the op family are `state/merge.ts`'s `Converge` surface; the SI scalar crossed by `QuantityWire` canonicalized once at C# admission and never re-converts here.
- Packages: `effect` (`Schema`); `./format.ts` (`Pack`); `../value/clock.ts` (`Hlc`); `../state/evidence.ts` (`ReceiptEnvelope`, `ProgressMark`, `Availability`); `../state/commit.ts` (`Commit`); `../state/causal.ts` (`Vector`); `../value/quantity.ts` (`Quantity`); `../value/identity.ts` (`TenantContext`).

```typescript signature
import { Hlc } from "../value/clock.ts"
import { TenantContext } from "../value/identity.ts"
import { Quantity } from "../value/quantity.ts"
import { Vector } from "../state/causal.ts"
import { Commit } from "../state/commit.ts"
import { Availability, ProgressMark, ReceiptEnvelope } from "../state/evidence.ts"

const _Assign = Schema.TaggedStruct("Assign", {
  key: Schema.NonEmptyString,
  path: Schema.NonEmptyString,
  value: Schema.Unknown,
  hlc: Hlc,
  actor: Vector.Replica,
})
const _Adjoin = Schema.TaggedStruct("Adjoin", {
  key: Schema.NonEmptyString,
  member: Schema.NonEmptyString,
  hlc: Hlc,
  actor: Vector.Replica,
})
const _Retire = Schema.TaggedStruct("Retire", {
  key: Schema.NonEmptyString,
  member: Schema.NonEmptyString,
  observed: Schema.Array(Hlc),
  hlc: Hlc,
  actor: Vector.Replica,
})
const _Splice = Schema.TaggedStruct("Splice", {
  key: Schema.NonEmptyString,
  anchor: Schema.NonEmptyString,
  run: Schema.Array(Schema.Unknown),
  hlc: Hlc,
  actor: Vector.Replica,
})
const _Tick = Schema.TaggedStruct("Tick", {
  key: Schema.NonEmptyString,
  delta: Schema.BigIntFromSelf,
  hlc: Hlc,
  actor: Vector.Replica,
})
const _Alien = Schema.transform(
  Pack.Alien,
  Schema.TaggedStruct("Alien", { ext: Schema.Int, cell: Schema.Uint8ArrayFromSelf }),
  {
    strict: true,
    decode: (foreign) => ({
      _tag: "Alien" as const,
      ext: foreign.type,
      cell: typeof foreign.data === "function" ? foreign.data(0) : foreign.data,
    }),
    encode: ({ cell, ext }) => Pack.alien(ext, cell),
  },
)

const CrdtOp: Schema.Union<[typeof _Assign, typeof _Adjoin, typeof _Retire, typeof _Splice, typeof _Tick, typeof _Alien]> =
  Schema.Union(_Assign, _Adjoin, _Retire, _Splice, _Tick, _Alien)
type CrdtOp = typeof CrdtOp.Type
```

## [06]-[LANDING_WIRE]

[LANDING_WIRE]:
- Owner: the wire-owned decoded shapes — decode-boundary vocabulary the live consumers bind (`ui/viewer/scene` seats `TextureSet` through `Pbr.seat`/`Pbr.index`; `runtime/browser/fetch` decodes `AssetSetManifest` on the dome lane), adopted verbatim from the C# mints and declared exactly once. Evidence plane: `RenderReceipt` (the frame-hash compare shape `ui/viewer/probe` binds; `matched` is C#-computed and never re-hashed — NO C# page mints a `RenderReceiptWire` family, so the shape carries no census row and stays consumer vocabulary until the `[RENDER_RECEIPT_RECONCILE]` producer lands), `FaultDetail` over the `Hops` sixteen-row vocabulary with the `FaultEnricher` Layer, `FlagVerdict` (the OpenFeature evaluation projection the runtime flag service consumes), `EvidenceTimeline` (the AppUi diagnostics crossing — HLC-ordered rows each composing the `ReceiptEnvelope` landing whole beside the producer's own skew band and overlap group, so the dashboard renders a server-computed fold rather than re-deriving one). Shell plane: `BindingStatus`/`CoercedValue`/`WriteReceipt` live-binding triple, `CommandGate` (the per-row CanExecute verdict whose `level` derives from the `Availability` landing's one degradation vocabulary — distinct carriers, one level spelling), `ControlIntent` (the producer's twenty-nine-kind WIDGET vocabulary — the shell's whole control surface, decoding on the `kind` discriminant the producer ships, with its binding, icon, hint, option, option-source, numeric-range, column, menu, toolbar-row, crumb, avatar, filter, and window-spec siblings riding inside the family payload rather than earning census rows; the viewer-interaction union that wore this family name is `ui/viewer/panel`'s own locally-minted vocabulary, since no C# page produces it), `LayoutProgram` (order-preserving Cassowary constraint program, decode-only, never solved here). Graph plane: `ElementGraph` over its `Header`, its `Node` rows, and its six-arm `Relation` union, beside `GraphDelta` the change record a stream folds onto it; every nested `rasm.element.v1` message rides as a static on the owner that carries it — `Header` seating `GeoReference` (itself seating `ProjectedCrs`) and `Step`, `MeasureValue` seating `Band`, `Placement` the `ObjectWire` pose frame, `Usage` the associate edge's three-arm material usage, `Redaction` the scoped-egress manifest, and `GraphDelta.Revision` the before/after pair — since a nested payload is presence on its owner's message and never a family of its own. BIM plane: `BcfTopic`/`BcfViewpoint` over the one `_GlobalId` brand, `BimModel`/`BimDiff`/`IdsAudit`, `PredicateWire` the closed selection algebra a browser composes and the model owner evaluates, with its `ValueMatch` restriction, `NodeMatch` incidence target, and `Measure` triple riding the family's own merged namespace rather than earning census rows. Appearance plane: `Material`/`PbrGroups` mirroring the OpenPBR projection field-for-field, `AppearanceSummary` the mirror of the seam summary the element suite nests (the `rasm.element.v1` `AppearanceWire` payload at `NodeWire` field 7, which that landing carries opaque — never a wire family of its own), `Texture` the one exported anchor for the frozen shared texture vocabulary, `TextureSet` the C#-baked plane-set document riding behind the appearance key, and `AssetSetManifest` the python-assembled ingest/IBL set manifest — the two set documents transcribing that anchor's rosters and carrying their own wire-legality columns beside them. Geo plane: `GeoFeature` with the opaque WKB band, the seven-kind geometry union, the CRS rows, the tile quadkey algebra, and the `WkbParser` port. Identity plane: `SnapshotHeader` (canonical-CBOR, segment roster), `Claim`/`HostFingerprint` with the boot-identity admission gate, `SupportExport`/`SupportExport.Entry` (the producer's flattened bundle export with its per-artifact manifest roster — evidence LEAVING the host, never the report arriving at `invoke`'s gateway; `ui/viewer/probe` binds it as the display-only support-evidence roster beside the claim board), `Credential` (the public PEM carrier — the published chain with its RFC-7468 label set, per-block digests, and redacted key-id; no private block crosses, so rotation compares the producer's bundle digest).
- Law: `_GlobalId` is one anchor — the twenty-two-character IFC base64 identity brands once and both the BCF and BIM planes compose it; a per-plane re-declaration is the split-brain defect this collapse killed. `BcfViewpoint.GlobalId` is the exported decode surface: the ui selection plane resolves raw pick material through `Schema.decodeUnknownOption(BcfViewpoint.GlobalId)`, so a locally-minted brand beside it is unspellable.
- Law: a discriminant is DECODED where the producer ships one and MINTED only where it does not — a family carrying its own kind column lands on that column untouched and its consumers match on it, while a tagged landing over a wire the producer ships untagged decodes through its `FromWire` twin, `_stamp` minting `_tag` at the seam because `Schema.tag` demands it on decode input; the stamp overwrites nothing a tagged wire already carries, encode passes through, and the twin rides the owner as a static so one import serves class and wire. Minting a second discriminant beside a shipped one is the defect — the two spellings then need a mapping table, and the arm a consumer matches stops being the arm the producer named.
- Law: the landing-class roster is a ratified co-located owner family — the census demands every wire-owned decoded shape in this one module, each class is an independent decode owner its consumers import directly, and collapsing the roster onto `Wire.*` statics trades one-hop resolution for a cosmetic export count; the charter accepts the wide export tail and the census guard keeps it closed.
- Law: `Hops` mints through the one `FaultClass.family` seam and carries four columns — `class`, the branch classification each hop reason projects, beside the gRPC peer's own `code`, `retryable`, and `terminal` — so `FaultDetail` satisfies the branch classification convention structurally, every compiled `Budget` schedule gates it with zero adapter, and the local reason-roster guard pair the seam already owns does not re-appear here. The peer columns stay because the protocol's retryability genuinely diverges from its class default (an already-exists refusal never succeeds on a re-send) and the code-to-reason projection generates from the `code` column, so both are adopted wire facts rather than a second taxonomy.
- Law: `FaultDetail` is wire-only altitude — constructed at exactly two sites: the `FaultDetailWire` decode row and the invoke page's transport fold; a third construction site in the branch is the defect the architecture suite audits. `EnricherLive` satisfies the `value/fault` `FaultEnricher` endo-arrow by reading the structured `wire.reason` attribute the crash boundary preserves from a `FaultDetail`; a capture without an admitted reason passes through untouched, so enrichment degrades to identity and never parses message prose. Stamped keys ARE the `_WIRE_ATTR` vocabulary rows — this enricher's owned `wire.*` axis beside the corpus-wide registry the observe convention page owns — never free string literals at the call site.
- Law: the predicate family is CLOSED at both discriminant levels and an unrostered `arm` or `match` value refuses as a `ParseError` the intake classes `malformed` — never a widening arm. Its producer's lowering is total for exactly this reason: a match-all fallthrough there once lowered an unrostered restriction to the present-match and handed back a predicate selecting the whole graph, so a permissive landing re-mints that defect one runtime over, silently and on the authoring side where nothing re-checks it. `node-match-exclusive` carries the same refusal for the incidence target's both-and-neither shapes, and every payload the wire cannot type — a pattern's compilability, a vocabulary key's membership, a measure's dimension — re-admits at the model owner's standing gates, so the landing types the question and admits none of it.
- Law: selection semantics carry ONE definition across runtimes — a filter builder, a saved view, and a coordination rule are all this one family, so the arms a browser composes are the arms the graph fold and the store lowering evaluate, and a locally-minted query vocabulary beside this landing is the fork that leaves two runtimes disagreeing about what a selection means. Egress legality follows by construction rather than by a second declaration: the producer's payloads are primitives, so decoded and encoded shapes coincide and an authored predicate re-encodes through `Wire.encode` with no projection twin.
- Law: `Credential` carries NO private key material and no column can hold any — the producer publishes the public chain, the RFC-7468 label set, the per-block digests, and the redacted key-id, with the private block staying host-side under its own lease, so the landing is public evidence a log may meet whole and `fingerprint` renames the producer's `keyId` at the field rather than through a second identity. A `Redacted` material column here would type the wire to carry what the mint refuses to write and hand a consumer a private-import path over bytes that never cross; host-held key material is `security/crypt/secret`'s own source and reaches `security/crypt/sign#KEY_MATERIAL` through it, never through this landing.
- Law: `sealed` is the broken-producer read, not a decode branch — the label vocabulary carries the producer's own `secret` column, so a landing whose set contains a private label is evidence the mint leaked and the consuming admission refuses it as data; rotation compares `bundleDigest`, the producer's ordered `(label, block digest)` preimage, so a re-split of one bundle's bytes reads as a rotation exactly where a whole-text compare read equal.
- Law: the benchmark claim is the one host-admitted document `tests/contracts/` `BENCHMARK_CLAIM` fixes — `suite`, `host`, `minted`, and metric rows under the `fn`/`iter`/`yield` modality discriminant, each carrying its subject, its positive sample count beside the at-least-one-rung map its own harness measured — the two structural floors that keep an evidence-free band from grading as a passing claim — its optional `ticks` and raw `samples` timings, the honestly-optional `gc`/`heap`/`counters` enrichment bands (`counters` flattening the addon's `cycles`/`instructions`/`cache`/`cacheMisses`/`branchMisses` averages), and its warmup, allocation, and operation columns — so a TS-lane mitata run and a C#-side equivalence sweep land in ONE family with neither fabricating the other's statistic, the observe `bench` fold grades the single rung its tolerance names, and `admit` refuses a host print unequal to `AppIdentity.host`, making cross-host comparison unspellable at the landing.
- Law: a metric row's `unit` is the MINTING HARNESS's vocabulary, deliberately outside the telemetry unit roster — a timing harness spells nanoseconds, a render probe spells a per-second rate beside a bare count, a C# sweep spells its own — so the column stays a free non-empty string the grade compares verbatim as an equality axis; narrowing it onto `Convention.units` refuses every measure that roster was never built to carry, and the claim plane measures what a harness measures rather than what the instrument census mounts.
- Law: the claim subject carries its own tag because the selection coordinate belongs to a kernel run alone — a flat row widening `input`, `substrate`, `route`, `provider`, and the equivalence columns to optional admits a probe row claiming a substrate it never ran, and the tag refuses that shape at decode rather than at a downstream gate.
- Law: the set documents TRANSCRIBE the frozen shared texture vocabulary as a ROW TABLE, never a name tuple — `_roles` fixes the canonical snake_case order that IS the set-key preimage rank, and `_channelRows` carries the roster's five wire-bearing columns beside it: `ch` the semantic component count, `transfer` the tag the channel is authored under, `neutral` the constant an absent packed slot, a mip gutter, and a UDIM hole fill with, `unit` the physical unit the value is expressed in, and `mip` its declared fold. Every plane law READS those five and declares nothing of its own: the colorimetric class is `transfer === "srgb"`, the direction class is `mip === "normalRenormalize"`, the storage-width floor follows both, the false-slot companion a consumer stamps is `neutral`, the millimetre and nanometre carriers name themselves through `unit`, and a derived boolean column beside them is the lossy projection that admits `specular_ior` as a light quantity. `_transferRows`, `_payloadRows`, `_planeRows`, `_depthRows`, `_containerRows`, `_layerRows`, and `_packRows` carry the rest of the fragment's vocabularies whole, each with the legality column its own refusals read — a document-local re-spelling of a channel, transfer, pack, format, or payload row is the fork the fragment forecloses.
- Law: `Texture` is the ONE exported anchor for that vocabulary — the thirteen roster tuples with their derived key types and nothing else, so a later-wave module keys its own column table off the same tuples and never re-spells a roster. The anchor carries tuples alone because a COLUMN is the owner's legality, not the vocabulary's: this page's wire columns and a consuming plane's tool columns describe the same rows for different refusals, and merging them mints a table neither owner can read. A derived subset — the scene-referred transfers, the wire-legal payloads, the four-wide pack formats — closes against its own anchor row under the two-way guard pair and never enters the anchor a second time.
- Law: a channel's `neutral` arity is the roster's `ch` column made structural — the fact type distributes over the two component counts once, so a three-band neutral on a one-component channel is unwritable and a consumer stamping a false pack slot's companion reads a tuple whose length its own channel already fixed. A free `ReadonlyArray<number>` column re-opens exactly the disagreement the roster closed.
- Law: legality carves ride generated COLUMNS on the roster they refuse, never hand exclusions in a lawfulness chain — `_containerRows.plane` carries the producer's own refusal of the eight-bit preview container from any channel plane, and `_planeRows.web` carries the browser transcoder's unreachable stores. Both read exactly where `_payloadRows.wire` and `_transferRows.plane` read, so a second preview-class container or a newly-decodable store is one column value and every refusal follows it with no literal to chase.
- Law: every admitted SUBSET derives from its roster's own legality column under a two-way guard pair, never as a hand-picked twin — `_PlaneTagged` reads the transfer roster's `plane` column and `_Wired` reads the payload roster's `wire` column, each closed against its declared tuple in both directions, so `pq`/`hlg` reach no channel plane (the environment products ride `AssetSetManifest.ibl`, which declares no transfer) and `rawBcn`/`astc` reach no wire row, while the branch still carries both vocabularies whole. Admitting a further transfer or a future transcodable payload is one row with one column value; a one-way subset guard lets the roster grow a legal member the subset never gains.
- Law: `AssetSetManifest.maps` keys by `(role, container)`, never role alone — the producer's `CompanionPolicy.RENDER` publishes a sampled twin beside an unsampled primary for a render-bound slot, differing by container with the same role — so every consumer selecting a plane reads the pair and a role-keyed lookup that assumes uniqueness silently takes whichever entry sorted first; the `TextureSetWire` channel roster stays role-unique because its C# producer mints no companions.
- Law: hex-key spelling splits by producer and folds once — `TextureSet` hex fields carry the C# `ContentAddress` X32 spelling and land through `Digest.FromX32` (UPPERCASE admitted, branded lowercase, re-emitted UPPERCASE on encode); `AssetSetManifest` digests carry the lowercase `ContentKey` spelling and land on the brand directly. Consumers joining a key to the served-asset path lower the branded key at path construction alone, so the `assets/<digest>/<file>` directory join reads these landings as its third consumer with zero new derivations — each level entry's `file` is the egress leaf verbatim and `<digest>` is the SET key, never a per-plane digest directory.
- Law: every addressed plane is a LEVEL-ORDERED list of address triples, never a scalar address beside a level count — `_leveled` generates the length law off the container's own `pyramid` column: a self-pyramiding container holds its whole pyramid in one file so the list holds ONE entry whatever `mips` declares, and every other container supplies one file per level so the length EQUALS `mips`. A scalar address beside a `mips` count names files it cannot address and leaves every level past the base undigested; the same triple types the IBL products, whose prefilter pyramid is levels its roughness ladder indexes position for position.
- Law: the triple is ONE variant declaration and each producer's schema is an `extract` — the two documents differ in exactly one field-level fact, the address column's key and encoding (`blob` on the C# X32 spelling, `digest` on the python lowercase brand), so `file` and `byteLength` are shared and the decoded shape names the address `address` on both. The pack row takes the same variant with its level list as the varying field, so the schema-parameterized row factory and its address parameter both delete, and the level-list refusal is one filter applied to each extract. Two parallel struct declarations that a reader must diff to find the one differing column is the shape this collapse deletes, and a third producer is one variant key with one field entry.
- Law: field names land camelCase on BOTH documents and the projection is mechanical at each producer — the C# emit spells its PascalCase members under `JsonSerializerDefaults.Web`, and the python producer's snake_case proto fields reach this landing through the generated message's own lowerCamel locals — so one spelling serves two estates and a snake_case landing field beside them is the third spelling the transcription law deletes.
- Law: `_absent` folds the producer's empty-string absence once through the shipped operator — proto3 emits `""` for an unset singular string, so an authored material's `emissionUnit`, an acquired set's `materialId`, and a dielectric's `conductor` all arrive empty and read as `Option.none()`; a message-typed field (`press`, `ibl`, `luminanceCdf`) is absence-capable on the wire itself and lands as `Option` through `optionalWith`, so the two absence mechanics split by field class and neither guesses. An absent `emissionUnit` is what keeps `emissionValue` honest: a bare authored multiplier and an admitted photometric magnitude stay apart where a lone scalar collapses them.
- Law: the pack roster carries its glTF read-order legality as a column both documents' consumers refuse against — the occlusion-first order IS the glTF KHR occlusion-plus-metallic-roughness read order and matches a three-component sampler's channel convention position for position, while the inverted order swaps its first and third slots, so binding it to those same slots reads occlusion as metalness with nothing raising. A bare role triple leaves that refusal undeclarable, and a consumer then either binds the wrong plane or hardcodes the pack name the roster already knows.
- Law: refusals the freeze names land structurally, never as gates — `press.backend` admits `cpu` alone (a GPU-minted set never reaches the wire), `channels`/`maps` rows hold roster order because the order IS the set-key preimage, a plane row's `channels` equals its role's roster count, a block payload admits the measured 8-bit store alone and rides a `ktx2` container, a payload column on any other container reads as vacancy (`none`), a packed slot's channel carries no standalone plane row, a `pbr_set` carries no `ibl` entry, `udimTiles` ascend from the Mari floor and agree with the manifest's own `udim` declaration, `sh9` is exactly 27 band-major values with `upAxis` frozen `z`, and the specular pyramid's `roughnessPerMip` matches its level roster — each violation a decode `ParseError` carrying the filter's own identifier.
- Law: five plane-row refusals generate off the per-channel columns and land structurally at the row, never as a consumer's re-derivation — a color channel's transfer FOLLOWS its store (`srgb` at integer depth, `linear` at float; every other row transfer-invariant), a DIMENSIONED channel refuses an integer store outright because the roster names its unit and no wire column carries a scale to normalize it against, a fold policy is the roster's own or the `box` floor with `none` reserved for the single-level plane, a storage width carries the semantic count with the two-component reconstruction carved for direction planes ALONE, and a declared block format names a payload holding block data — which no wire-legal payload does, so the refusal generates rather than hardcoding `none`. Reading the roster's transfer column raw admits an 8-bit linear `base_color` a shading rail then decodes twice; leaving the unit column unread admits a millimetre mean-free-path carrier and a cd/m2 emission floor at `u8`, where the stored integer expresses no physical magnitude at all and `height` alone — normalized on the row with its span riding `heightScale` — proves the pattern the other columns follow; leaving width free admits a three-band millimetre carrier in `rg16` with its third band unrecoverable; leaving the fold free admits `roughnessVariance` on a color plane and a pyramid depth its policy denies.
- Law: association crosses at the SET and narrows at the row, and `container` is a REQUIRED column on every channel, pack, and map row on BOTH documents — the association gate selects on it, and a row recovering the container by string-parsing its own egress extension is the unspellable form the column replaces. Every one of the twelve `_containerRows` fixes its canonical association (the `jxl`/`jxl_f16`/`avif12` rows are the MEASURED posture of the provisioned encoders — `imagecodecs.jpegxl_encode` and `imagecodecs.avif_encode` expose no premultiplication seat, so those containers write straight alpha and `associated` on one is unrepresentable rather than merely lossy), and a plane whose declared mode differs from its container's canonical association admits at a deep store alone, because a straight-to-associated conversion at 8 bits quantizes catastrophically at low alpha; a `none` plane carries nothing to convert and passes whole. A container one branch alone writes still rides the roster — the peer refuses it by roster membership, never by an unknown key.
- Law: the graph landing is a DECODE-ONLY mirror and every column traces to a declared producer field — a snapshot identity, a synthesized kind token, or a flattened endpoint pair beside the arm that already names its endpoints are columns the `.proto` never wrote, and a consumer reading one reads a fact no producer can be held to. The mirror re-censuses against the whole `rasm.element.v1` roster on every move: a message the four families transitively reach either lands arm for arm or lands as its owner's own untyped carriage, never as a routed subset of the columns one consumer happened to want.
- Law: a `oneof` ships its DISCRIMINANT as the case name, so `kind` DERIVES at the lift and the landing mints nothing — `_cased` reads the generated `{ case, value }` envelope onto a `kind` literal closed at the producer's own arm roster, and `seat` is its one policy column: the relationship envelope hoists its arm's columns because the oneof IS its whole content, the node envelope keeps its case value whole because the eight payload messages carry no family. An unset oneof carries no case, matches no arm, and refuses as the `ParseError` the intake classes `malformed` — the producer's `<wire-*-none>` rail one runtime over.
- Law: a node identity crosses as the producer's X32 `NodeId` TEXT and every endpoint column is that same text — `Digest.FromX32` lands them all on the one `ContentKey` brand, so an edge endpoint, a removed-node id, a realizing id, a participant, and the redaction roster all compare by bare `===` with no join table; the byte twin belongs to the `UInt128` columns alone (`interfaceKey`, the appearance key), and reading a text id through it decodes hex characters as raw bytes and strands every join.
- Law: the `Header` crosses whole and reaches a consumer — the release and view keys, the survey frame, the mint instant, the STEP file header, the TOLERANCE the producer's own address verification ran at, and the UNIT SCHEME mapping each quantity token to its registry unit member, whose EMPTY map reads as SI rather than as an absent column. A landing dropping the header hands a viewer magnitudes with no scheme to render them under and no tolerance to grade an address against, so the two columns that make the crossing interpretable would exist only on the producing side.
- Law: the delta is the snapshot's twin, not its subset — `GraphDelta` carries the producer's five sections beside an OPTIONAL header where the snapshot's is required, because a delta re-headers only where the producer's own reheader ran. Its sections re-admit through the same `Node`/`Relation` gates the snapshot takes, so one landing pair serves both crossings; the unique-per-id normal form is the producer's gate before the bytes leave and this end reads the sections as declared data.
- Law: the graph pose crosses as DECODED DATA and re-derives nowhere — `Placement` carries the producer's nine ordered doubles (the location origin, the axis local-Z, the ref-direction local-X) off `ObjectWire`'s own presence, free reals under no seam gate, so a viewer renders a `DiffWire` `Moved` relocation off the node that moved rather than reconstructing a transform from geometry. The frame stays OUT of the producer's canonical bytes, which is what keeps that discriminant alive across the crossing: a landing folding pose into the identity would collapse a move into a content change.
- Law: a crossing's `Redaction` manifest is the producer's egress receipt this end READS, never re-derives — the policy identity, the owner-qualified cleared paths, and the node roster whose cleared columns re-keyed. Its named nodes are DECLARED-UNSTABLE for address verification, so `addressable` is the COMPLEMENT any verify here grades and a drift outside that roster is real. A cleared column carries no explicit presence and reads as its proto3 default, so the DECLARED roster — never the message — separates a cleared column from an authored default, and re-minting a key over redacted bytes grades a document the producer never claimed.
- Law: `GeoFeature`'s WKB band is opaque carriage under the gated `WkbParser` port — geometry materializes only through the port the ui wave satisfies, and the tile algebra (`quadkey`, `parent`, `children`) is total over the zoom-bounded grid refinement. The `_CRS` row table is the branch's one SRID authority — `srid` admits any positive int at decode while `Crs.of` resolves only the declared rows, so a served SRID outside them refuses at the ui admission fold as evidence; growth is one `_CRS` row HERE (`kind` + `unit`), never a consumer-side projection table.
- Exemption: `Crs.of`'s `in`-probe key narrowing, the `EnricherLive` structured-reason probe (`token in _hops` behind its refinement), and the `Tile.quadkey` bit walk are marked kernels — the checker cannot carry the probe onto the key type, and only immutable values leave.
- Growth: a new shell intent, appearance block, BCF axis, predicate arm, graph edge arm, node payload case, or fault evidence field is one case or field mirroring the producing peer's emit; a texture channel, pack, transfer, or payload row is one shared-fragment row re-frozen then transcribed into one `Texture` tuple entry and one row on the tables that carry columns for it; a new landing plane is one owner block here with its census rows; a third plane-address producer is one variant key.
- Boundary: rollout targeting and flag evaluation are the runtime wave's service over this decoded verdict; GLB parsing, kiwi solving, BCF re-location, and OpenPBR rendering are ui-wave consumers of these values; the browser-store refusal `_planeRows.web` declares and the pack read-order refusal `_packRows.gltf` declares are the ui bind's to raise over these columns; predicate admission — pattern compilation, vocabulary resolution, measure re-admission — and every evaluation, folded over the live graph or lowered to store SQL, are `csharp:Rasm.Bim/Model/query#PREDICATE_WIRE`'s.
- Packages: `@effect/experimental` (`VariantSchema`); `effect` (`Schema`, `Effect`, `Layer`, `Context`, `Array`, `Function`, `Predicate`, `HashMap`, `Option`); `../value/contentKey.ts` (`ContentKey`, `Digest`); `../value/clock.ts` (`Hlc`); `../value/identity.ts` (`AppIdentity`); `../value/fault.ts` (`FaultClass`, `FaultEnricher`).

```typescript signature
import * as VariantSchema from "@effect/experimental/VariantSchema"
import { Context, Layer } from "effect"
import { AppIdentity } from "../value/identity.ts"
import { FaultClass, FaultEnricher } from "../value/fault.ts"

// No wire family carries this shape — `ui/viewer/probe` binds it process-locally for the frame-hash
// compare; the census row retired because no C# page mints `RenderReceiptWire`, and it returns only
// when the `[RENDER_RECEIPT_RECONCILE]` producer lands.
class RenderReceipt extends Schema.Class<RenderReceipt>("RenderReceipt")({
  view: Schema.NonEmptyString,
  key: Digest.FromBytes,
  matched: Schema.Boolean,
  at: Schema.DateTimeUtc,
}) {}

const _reasons = [
  "canceled", "unknown", "invalid", "deadline", "notfound", "exists", "denied", "exhausted",
  "precondition", "aborted", "range", "unimplemented", "internal", "unavailable", "dataloss", "unauthenticated",
] as const

// The transport family mints through the same seam every folder family takes: `class` is the branch taxonomy, and
// `code`/`retryable`/`terminal` are the gRPC peer's OWN columns adopted verbatim — the wire's retryability diverges
// from its class default where the protocol says so (an already-exists refusal never succeeds on a re-send), so
// these are peer facts the landing carries, never a second taxonomy this branch mints.
const _hops = FaultClass.family(_reasons, {
  canceled: { code: 1, retryable: false, terminal: false, class: "defect" },
  unknown: { code: 2, retryable: false, terminal: false, class: "defect" },
  invalid: { code: 3, retryable: false, terminal: false, class: "invalid" },
  deadline: { code: 4, retryable: true, terminal: false, class: "expired" },
  notfound: { code: 5, retryable: false, terminal: false, class: "absent" },
  exists: { code: 6, retryable: false, terminal: false, class: "conflicted" },
  denied: { code: 7, retryable: false, terminal: true, class: "denied" },
  exhausted: { code: 8, retryable: true, terminal: false, class: "exhausted" },
  precondition: { code: 9, retryable: false, terminal: false, class: "invalid" },
  aborted: { code: 10, retryable: true, terminal: false, class: "conflicted" },
  range: { code: 11, retryable: false, terminal: false, class: "invalid" },
  unimplemented: { code: 12, retryable: false, terminal: true, class: "defect" },
  internal: { code: 13, retryable: false, terminal: false, class: "defect" },
  unavailable: { code: 14, retryable: true, terminal: false, class: "unavailable" },
  dataloss: { code: 15, retryable: false, terminal: true, class: "breached" },
  unauthenticated: { code: 16, retryable: false, terminal: true, class: "denied" },
})

declare namespace Hops {
  type Reason = (typeof _hops.reasons)[number]
  type Row = (typeof _hops.rows)[Reason]
  type Shape = Types.Simplify<typeof _hops.rows & {
    readonly reasons: typeof _reasons
    readonly wire: typeof _hops.schema
    readonly fromCode: (code: number) => Reason
  }>
}

const _byCode: HashMap.HashMap<number, Hops.Reason> = Array.reduce(
  _reasons,
  HashMap.empty<number, Hops.Reason>(),
  (acc, reason) => HashMap.set(acc, _hops.rows[reason].code, reason),
)

const Hops: Hops.Shape = {
  ..._hops.rows,
  reasons: _reasons,
  wire: _hops.schema,
  fromCode: (code) => Option.getOrElse(HashMap.get(_byCode, code), () => "unknown"),
}

const _stamp = (tag: string): Schema.Schema<unknown, unknown> =>
  Schema.transform(Schema.Unknown, Schema.Unknown, {
    strict: true,
    decode: (raw) => (Predicate.isRecord(raw) ? { ...raw, _tag: tag } : raw),
    encode: Function.identity,
  })

class Hop extends Schema.Class<Hop>("Hop")({
  site: Schema.NonEmptyString,
  reason: Hops.wire,
  elapsed: Schema.DurationFromMillis,
}) {}

const _WIRE_ATTR = { reason: "wire.reason", retryable: "wire.retryable", terminal: "wire.terminal" } as const

class FaultDetail extends Schema.TaggedError<FaultDetail>()("FaultDetail", {
  reason: Hops.wire,
  surface: Schema.NonEmptyString,
  detail: Schema.NonEmptyString,
  hops: Schema.Array(Hop),
  tenant: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {
  static readonly Hop: typeof Hop = Hop
  static readonly FromWire: Schema.Schema<FaultDetail, unknown> = Schema.compose(_stamp("FaultDetail"), FaultDetail, { strict: false })
  static readonly EnricherLive: Layer.Layer<FaultEnricher> = Layer.succeed(
    FaultEnricher,
    FaultEnricher.of({
      enrich: (capture) =>
        Effect.succeed(
          Option.match(
            Option.filter(
              Option.fromNullable(capture.attributes[_WIRE_ATTR.reason]),
              (token): token is Hops.Reason => typeof token === "string" && token in _hops.rows,
            ),
            {
              onNone: () => capture,
              onSome: (reason) =>
                capture.enriched({
                  [_WIRE_ATTR.reason]: reason,
                  [_WIRE_ATTR.retryable]: _hops.rows[reason].retryable,
                  [_WIRE_ATTR.terminal]: _hops.rows[reason].terminal,
                }),
            },
          ),
        ),
    }),
  )
  get class(): FaultClass.Kind {
    return _hops.classOf(this.reason)
  }
  get retryable(): boolean {
    return _hops.rows[this.reason].retryable
  }
  get terminal(): boolean {
    return _hops.rows[this.reason].terminal
  }
  get origin(): Option.Option<Hop> {
    return Array.head(this.hops)
  }
  override get message(): string {
    return `<${this.surface}:${this.reason}> ${this.detail}`
  }
}

const _flagReasons = ["static", "default", "targeting", "split", "cached", "disabled", "stale", "error", "unknown"] as const

class FlagVerdict extends Schema.Class<FlagVerdict>("FlagVerdict")({
  flag: Schema.NonEmptyString,
  value: Schema.Union(Schema.Boolean, Schema.NonEmptyString, Schema.Number),
  variant: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  reason: Schema.Literal(..._flagReasons),
}) {}

// The AppUi diagnostics timeline: HLC-ordered receipt rows carrying the producer's OWN skew band per row, so a
// dashboard renders server-computed overlap components and never re-folds the clock. `envelope` composes the
// `ReceiptEnvelope` landing whole — the timeline carries the sibling owner and mints no second envelope twin —
// and `uncertaintyGroup` is the producer's overlap partition, a group ordinal rather than a severity rank.
const _SkewBand = Schema.Struct({ earliest: Schema.DateTimeUtc, latest: Schema.DateTimeUtc })

const _EvidenceRow = Schema.Struct({
  ordinal: Schema.Int.pipe(Schema.nonNegative()),
  uncertaintyGroup: Schema.Int.pipe(Schema.nonNegative()),
  envelope: ReceiptEnvelope,
  band: _SkewBand,
})

class EvidenceTimeline extends Schema.Class<EvidenceTimeline>("EvidenceTimeline")({
  correlation: Schema.NonEmptyString,
  rows: Schema.Array(_EvidenceRow),
}) {}

class BindingStatus extends Schema.TaggedClass<BindingStatus>()("BindingStatus", {
  binding: Schema.NonEmptyString,
  phase: Schema.Literal("bound", "coercing", "refused", "detached"),
  detail: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
}) {
  static readonly FromWire: Schema.Schema<BindingStatus, unknown> = Schema.compose(_stamp("BindingStatus"), BindingStatus, { strict: false })
}
class CoercedValue extends Schema.TaggedClass<CoercedValue>()("CoercedValue", {
  binding: Schema.NonEmptyString,
  offered: Schema.Unknown,
  landed: Schema.Unknown,
  path: Schema.NonEmptyString,
}) {
  static readonly FromWire: Schema.Schema<CoercedValue, unknown> = Schema.compose(_stamp("CoercedValue"), CoercedValue, { strict: false })
}
class WriteReceipt extends Schema.TaggedClass<WriteReceipt>()("WriteReceipt", {
  binding: Schema.NonEmptyString,
  landed: Schema.Unknown,
  stamp: Hlc,
}) {
  static readonly FromWire: Schema.Schema<WriteReceipt, unknown> = Schema.compose(_stamp("WriteReceipt"), WriteReceipt, { strict: false })
}
// The per-row CanExecute gate verdict the palette reads — `level` derives from the ONE degradation vocabulary the
// `Availability` landing owns, so the two carriers stay distinct documents over one level spelling and neither shadows
// the other (the C# owner states the non-shadowing law at its mint).
class CommandGate extends Schema.TaggedClass<CommandGate>()("CommandGate", {
  key: Schema.NonEmptyString,
  available: Schema.Boolean,
  level: Availability.fields.level,
}) {
  static readonly FromWire: Schema.Schema<CommandGate, unknown> = Schema.compose(_stamp("CommandGate"), CommandGate, { strict: false })
}

const _Vec3 = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number)

// The AppUi shell's widget vocabulary: twenty-nine locked kind literals, each arm carrying its typed shape beside
// the one `IntentBinding` carrier. The producer SHIPS the discriminant, so this landing decodes on the `kind`
// column the wire already carries and mints no second tag. Key-grade columns take `NonEmptyString` because an
// empty key resolves against no label catalog, command registry, or automation id on either head; display text
// takes `String`, since the producer's own text columns admit it.
const _Emphasis = Schema.Literal("quiet", "secondary", "primary", "danger", "inverted", "link")
const _Orientation = Schema.Literal("Horizontal", "Vertical")
// the picker modality is the shipped `UsePickerTypes` roster whole, so a fourth posture breaks here
const _PickerMode = Schema.Literal("OpenFile", "SaveFile", "OpenFolder")

const _IconSlot = Schema.Struct({
  asset: Schema.NonEmptyString,
  placement: Schema.Literal("Left", "Top", "Right", "Bottom"),
  size: Schema.Int.pipe(Schema.positive()),
  pending: Schema.NullOr(Schema.NonEmptyString),
})

const _HintRow = Schema.Struct({ body: Schema.String, gesture: Schema.NullOr(Schema.NonEmptyString) })

// `role` is the producer's `PaintRole` key — a growable theme roster each head reads as a style class, so it stays
// an open key where every closed producer table below decodes as its own literal union; no automation-name column
// crosses, because both heads derive the announced name from `key` through their own locale resolver.
const _Binding = Schema.Struct({
  role: Schema.NonEmptyString,
  emphasis: _Emphasis,
  command: Schema.NullOr(Schema.NonEmptyString),
  valueKey: Schema.NullOr(Schema.NonEmptyString),
  trigger: Schema.NullOr(Schema.Literal("activate", "change", "commit")),
  icon: Schema.NullOr(_IconSlot),
  hint: Schema.NullOr(_HintRow),
})

const _Window = Schema.Struct({
  extent: Schema.Number,
  overscan: Schema.Number,
  mode: Schema.Literal("fixed", "measured"),
  fixedItemExtent: Schema.Number,
})

// The integral, unsigned, and precise arms cross as ORDINAL DECIMAL STRINGS because a sixty-four-bit bound and a
// decimal significand both exceed this head's native number, so they land on `bigint` and `BigDecimal` where the
// real arm lands on `number` — decoding the string arms as numbers silently rounds the top decade of a `ulong`
// spinner and the tail digits of a `decimal` one, which is exactly the bound a checked narrowing exists to keep.
const _NumericRange = Schema.Union(
  Schema.Struct({ form: Schema.Literal("integral"), min: Schema.BigInt, max: Schema.BigInt, step: Schema.BigInt }),
  Schema.Struct({ form: Schema.Literal("unsigned"), min: Schema.BigInt, max: Schema.BigInt, step: Schema.BigInt }),
  Schema.Struct({ form: Schema.Literal("real"), min: Schema.Number, max: Schema.Number, step: Schema.Number }),
  Schema.Struct({ form: Schema.Literal("precise"), min: Schema.BigDecimal, max: Schema.BigDecimal, step: Schema.BigDecimal }),
)

const _NumericKind = Schema.Literal(
  "byte", "sbyte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "decimal",
)

// temporal bounds cross as calendar text, never an instant — the producer's bound is a plain date on every
// temporal kind, so decoding it as a moment would fabricate a zone the wire never states
const _PlainDate = Schema.String.pipe(Schema.pattern(/^\d{4}-\d{2}-\d{2}$/), Schema.brand("PlainDate"))

const _OptionRow = Schema.Struct({
  value: Schema.NonEmptyString,
  labelKey: Schema.NonEmptyString,
  group: Schema.NullOr(Schema.NonEmptyString),
  icon: Schema.NullOr(_IconSlot),
})

const _OptionSource = Schema.Union(
  Schema.Struct({ form: Schema.Literal("inline"), rows: Schema.Array(_OptionRow) }),
  Schema.Struct({ form: Schema.Literal("bound"), sourceKey: Schema.NonEmptyString }),
)

const _CrumbRow = Schema.Struct({
  value: Schema.NonEmptyString,
  labelKey: Schema.NonEmptyString,
  icon: Schema.NullOr(_IconSlot),
  command: Schema.NullOr(Schema.NonEmptyString),
})

const _AvatarRow = Schema.Struct({ labelKey: Schema.NonEmptyString, portrait: Schema.NullOr(Schema.NonEmptyString) })

// the pattern list lands non-empty because the producer's own filter encoder refuses an empty one before the
// picker mounts, so the landing states the emission's shape rather than admitting a document it never writes
const _FileFilterRow = Schema.Struct({ label: Schema.String, patterns: Schema.NonEmptyArray(Schema.NonEmptyString) })

// a menu row is a ROW one level down, never a child intent, so its recursion closes on itself; every column is
// representation-invariant, so ONE interface annotates both sides of the suspended reference
interface _MenuRow {
  readonly key: string
  readonly labelKey: string
  readonly posture: "command" | "check" | "radio" | "separator"
  readonly icon: typeof _IconSlot.Type | null
  readonly gesture: string | null
  readonly command: string | null
  readonly checkedKey: string | null
  readonly rows: ReadonlyArray<_MenuRow>
}

const _MenuRow: Schema.Schema<_MenuRow> = Schema.Struct({
  key: Schema.NonEmptyString,
  labelKey: Schema.NonEmptyString,
  posture: Schema.Literal("command", "check", "radio", "separator"),
  icon: Schema.NullOr(_IconSlot),
  gesture: Schema.NullOr(Schema.NonEmptyString),
  command: Schema.NullOr(Schema.NonEmptyString),
  checkedKey: Schema.NullOr(Schema.NonEmptyString),
  rows: Schema.Array(Schema.suspend((): Schema.Schema<_MenuRow> => _MenuRow)),
})

// The leaf half of the family: twenty arms whose shapes bottom out, so both representations DERIVE from the union
// rather than being spelled twice — the numeric and temporal columns are what make the two sides differ at all.
const _leaves = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("button"), key: Schema.NonEmptyString, labelKey: Schema.NonEmptyString, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("label"), key: Schema.NonEmptyString, textKey: Schema.NonEmptyString, role: Schema.NonEmptyString, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("textInput"), key: Schema.NonEmptyString, watermark: Schema.String, multiline: Schema.Boolean, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("numberInput"), key: Schema.NonEmptyString, numericKind: _NumericKind, range: _NumericRange, binding: _Binding }),
  Schema.Struct({
    kind: Schema.Literal("dateInput"),
    key: Schema.NonEmptyString,
    temporalKind: Schema.Literal("date", "time", "datetime", "range"),
    from: Schema.NullOr(_PlainDate),
    until: Schema.NullOr(_PlainDate),
    upperKey: Schema.NullOr(Schema.NonEmptyString),
    binding: _Binding,
  }),
  Schema.Struct({ kind: Schema.Literal("pathInput"), key: Schema.NonEmptyString, mode: _PickerMode, filters: Schema.Array(_FileFilterRow), multiple: Schema.Boolean, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("colorInput"), key: Schema.NonEmptyString, posture: Schema.Literal("inline", "flyout"), alpha: Schema.Boolean, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("select"), key: Schema.NonEmptyString, posture: Schema.Literal("closed", "editable"), options: _OptionSource, window: _Window, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("multiSelect"), key: Schema.NonEmptyString, posture: Schema.Literal("bound", "free"), options: _OptionSource, window: _Window, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("slider"), key: Schema.NonEmptyString, min: Schema.Number, max: Schema.Number, step: Schema.Number, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("range"), key: Schema.NonEmptyString, min: Schema.Number, max: Schema.Number, step: Schema.Number, upperKey: Schema.NonEmptyString, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("toggle"), key: Schema.NonEmptyString, labelKey: Schema.NonEmptyString, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("radio"), key: Schema.NonEmptyString, options: Schema.Array(_OptionRow), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("segmented"), key: Schema.NonEmptyString, posture: Schema.Literal("select", "command"), options: Schema.Array(_OptionRow), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("chip"), key: Schema.NonEmptyString, textKey: Schema.NonEmptyString, posture: Schema.Literal("static", "toggle", "removable"), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("progress"), key: Schema.NonEmptyString, form: Schema.Literal("bar", "ring", "skeleton"), fraction: Schema.NullOr(Schema.Number), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("avatar"), key: Schema.NonEmptyString, members: Schema.Array(_AvatarRow), visible: Schema.Int.pipe(Schema.nonNegative()), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("breadcrumb"), key: Schema.NonEmptyString, crumbs: Schema.Array(_CrumbRow), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("tooltip"), key: Schema.NonEmptyString, hint: _HintRow, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("menu"), key: Schema.NonEmptyString, rows: Schema.Array(_MenuRow), binding: _Binding }),
)

// The nesting half is spelled ONCE and instantiated per representation: the child type is the only axis that
// moves, because `_Binding`, `_Window`, and the extent/align columns are representation-invariant by
// construction — every one of their columns is a string, a literal, or a nullable of one.
type _BindingRow = typeof _Binding.Type
type _WindowRow = typeof _Window.Type

type _ColumnOf<T> = {
  readonly headerKey: string
  readonly cell: T
  readonly editor: T | null
  readonly extent: { readonly value: number; readonly unit: "auto" | "pixel" | "star" | "sizeToCells" | "sizeToHeader" }
  readonly sortKey: string | null
  readonly align: "Left" | "Center" | "Right" | "Stretch"
}

type _Nest<T> =
  | { readonly kind: "emptyState"; readonly key: string; readonly headlineKey: string; readonly bodyKey: string; readonly action: T | null; readonly binding: _BindingRow }
  | { readonly kind: "grid"; readonly key: string; readonly columns: ReadonlyArray<_ColumnOf<T>>; readonly window: _WindowRow; readonly binding: _BindingRow }
  | { readonly kind: "tree"; readonly key: string; readonly item: T; readonly expansionCommand: string; readonly window: _WindowRow; readonly binding: _BindingRow }
  | { readonly kind: "toolbar"; readonly key: string; readonly rows: ReadonlyArray<{ readonly item: T; readonly overflow: "AsNeeded" | "Always" | "Never" }>; readonly orientation: "Horizontal" | "Vertical"; readonly binding: _BindingRow }
  | { readonly kind: "tab"; readonly key: string; readonly pages: ReadonlyArray<{ readonly headerKey: string; readonly body: T }>; readonly binding: _BindingRow }
  | { readonly kind: "accordion"; readonly key: string; readonly sections: ReadonlyArray<{ readonly headerKey: string; readonly body: T }>; readonly binding: _BindingRow }
  | { readonly kind: "panel"; readonly key: string; readonly children: ReadonlyArray<T>; readonly constraintProgram: string; readonly binding: _BindingRow }
  | { readonly kind: "dock"; readonly key: string; readonly regions: ReadonlyArray<T>; readonly constraintProgram: string; readonly binding: _BindingRow }
  | { readonly kind: "splitter"; readonly key: string; readonly first: T; readonly second: T; readonly orientation: "Horizontal" | "Vertical"; readonly binding: _BindingRow }

type ControlIntent = typeof _leaves.Type | _Nest<ControlIntent>
type ControlIntentWire = typeof _leaves.Encoded | _Nest<ControlIntentWire>

const _child: Schema.Schema<ControlIntent, ControlIntentWire> = Schema.suspend(() => ControlIntent)

const _Column = Schema.Struct({
  headerKey: Schema.NonEmptyString,
  cell: _child,
  editor: Schema.NullOr(_child),
  extent: Schema.Struct({ value: Schema.Number, unit: Schema.Literal("auto", "pixel", "star", "sizeToCells", "sizeToHeader") }),
  sortKey: Schema.NullOr(Schema.NonEmptyString),
  align: Schema.Literal("Left", "Center", "Right", "Stretch"),
})

const _Section = Schema.Struct({ headerKey: Schema.NonEmptyString, body: _child })

const ControlIntent: Schema.Schema<ControlIntent, ControlIntentWire> = Schema.Union(
  _leaves,
  Schema.Struct({ kind: Schema.Literal("emptyState"), key: Schema.NonEmptyString, headlineKey: Schema.NonEmptyString, bodyKey: Schema.NonEmptyString, action: Schema.NullOr(_child), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("grid"), key: Schema.NonEmptyString, columns: Schema.Array(_Column), window: _Window, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("tree"), key: Schema.NonEmptyString, item: _child, expansionCommand: Schema.NonEmptyString, window: _Window, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("toolbar"), key: Schema.NonEmptyString, rows: Schema.Array(Schema.Struct({ item: _child, overflow: Schema.Literal("AsNeeded", "Always", "Never") })), orientation: _Orientation, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("tab"), key: Schema.NonEmptyString, pages: Schema.Array(_Section), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("accordion"), key: Schema.NonEmptyString, sections: Schema.Array(_Section), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("panel"), key: Schema.NonEmptyString, children: Schema.Array(_child), constraintProgram: Schema.NonEmptyString, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("dock"), key: Schema.NonEmptyString, regions: Schema.Array(_child), constraintProgram: Schema.NonEmptyString, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("splitter"), key: Schema.NonEmptyString, first: _child, second: _child, orientation: _Orientation, binding: _Binding }),
)

const _Term = Schema.Struct({ variable: Schema.NonEmptyString, coefficient: Schema.Number })
const _Constraint = Schema.Struct({
  relation: Schema.Literal("le", "ge", "eq"),
  strength: Schema.Literal("required", "strong", "medium", "weak"),
  terms: Schema.NonEmptyArray(_Term),
  constant: Schema.Number,
})

class LayoutProgram extends Schema.Class<LayoutProgram>("LayoutProgram")({
  surface: Schema.NonEmptyString,
  edits: Schema.Array(Schema.NonEmptyString),
  constraints: Schema.NonEmptyArray(_Constraint),
}) {}

const _GlobalId = Schema.String.pipe(Schema.length(22), Schema.pattern(/^[0-9A-Za-z_$]{22}$/), Schema.brand("GlobalId"))

const _Comment = Schema.Struct({
  author: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
  body: Schema.NonEmptyString,
  viewpoint: Schema.optionalWith(Schema.UUID, { as: "Option" }),
})

class BcfTopic extends Schema.Class<BcfTopic>("BcfTopic")({
  guid: Schema.UUID,
  title: Schema.NonEmptyString,
  status: Schema.Literal("open", "in-progress", "resolved", "closed"),
  priority: Schema.Literal("low", "normal", "high", "critical"),
  labels: Schema.Array(Schema.NonEmptyString),
  assignee: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  due: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option" }),
  comments: Schema.Array(_Comment),
}) {}

const _Camera = Schema.Struct({
  position: _Vec3,
  direction: _Vec3,
  up: _Vec3,
  fieldOfView: Schema.Number.pipe(Schema.positive()),
})
const _Plane = Schema.Struct({ origin: _Vec3, normal: _Vec3 })

class BcfViewpoint extends Schema.Class<BcfViewpoint>("BcfViewpoint")({
  guid: Schema.UUID,
  topic: Schema.UUID,
  camera: _Camera,
  selection: Schema.Array(_GlobalId),
  clipping: Schema.Array(_Plane),
}) {
  static readonly GlobalId: typeof _GlobalId = _GlobalId
}

class BimModel extends Schema.Class<BimModel>("BimModel")({
  key: Digest.FromBytes,
  dialect: Schema.NonEmptyString,
  elements: Schema.Int.pipe(Schema.nonNegative()),
  minted: Schema.DateTimeUtc,
}) {}
class BimDiff extends Schema.Class<BimDiff>("BimDiff")({
  base: Digest.FromBytes,
  next: Digest.FromBytes,
  added: Schema.Array(_GlobalId),
  removed: Schema.Array(_GlobalId),
  modified: Schema.Array(Schema.Struct({ anchor: _GlobalId, attributes: Schema.Array(Schema.NonEmptyString) })),
}) {}
class IdsAudit extends Schema.Class<IdsAudit>("IdsAudit")({
  specification: Schema.NonEmptyString,
  verdicts: Schema.Array(Schema.Struct({
    requirement: Schema.NonEmptyString,
    verdict: Schema.Literal("pass", "fail", "unapplicable"),
    anchors: Schema.Array(_GlobalId),
  })),
}) {}

// Selection crosses as data: the producer's polymorphic family lands on ITS OWN `arm` and `match` discriminant
// columns, so a browser filter builder authors the exact arms `PredicateCodec.Admit` re-admits. This landing keeps its
// family spelling because `Predicate` is the shipped `effect` module this page already composes.
const _Measure = Schema.Struct({
  si: Schema.Number,
  type: Schema.NonEmptyString,
  // seven SI base exponents in producer order — arity IS the refusal, landing structurally here where its producer
  // needs a dimension guard on its own rail
  dimension: Schema.Tuple(Schema.Int, Schema.Int, Schema.Int, Schema.Int, Schema.Int, Schema.Int, Schema.Int),
})
const _Bound = Schema.Struct({ value: _Measure, inclusive: Schema.Boolean })

// Value restrictions mirror the producer's IDS-derived family: exact splits by candidate class (a rendered text
// compare against an SI magnitude compare), and every open bound is a null the producer's own optional carries.
const _ValueMatch = Schema.Union(
  Schema.Struct({ match: Schema.Literal("present") }),
  Schema.Struct({ match: Schema.Literal("exact"), value: Schema.String }),
  Schema.Struct({ match: Schema.Literal("exactMeasure"), value: _Measure }),
  Schema.Struct({ match: Schema.Literal("pattern"), expression: Schema.String }),
  Schema.Struct({ match: Schema.Literal("range"), lower: Schema.NullOr(_Bound), upper: Schema.NullOr(_Bound) }),
  Schema.Struct({ match: Schema.Literal("oneOf"), allowed: Schema.Array(Schema.String) }),
  Schema.Struct({ match: Schema.Literal("length"), min: Schema.NullOr(Schema.Int), max: Schema.NullOr(Schema.Int) }),
  Schema.Struct({ match: Schema.Literal("digits"), total: Schema.NullOr(Schema.Int), fraction: Schema.NullOr(Schema.Int) }),
)

const _predicateLeaves = Schema.Union(
  Schema.Struct({ arm: Schema.Literal("class"), class: Schema.NonEmptyString }),
  Schema.Struct({ arm: Schema.Literal("domain"), domain: Schema.NonEmptyString }),
  Schema.Struct({ arm: Schema.Literal("predefined"), class: Schema.NonEmptyString, token: Schema.NonEmptyString }),
  Schema.Struct({ arm: Schema.Literal("classification"), system: Schema.NonEmptyString, code: Schema.NonEmptyString }),
  Schema.Struct({ arm: Schema.Literal("classificationSystem"), system: Schema.NonEmptyString }),
  Schema.Struct({ arm: Schema.Literal("kind"), kind: Schema.NonEmptyString }),
  Schema.Struct({ arm: Schema.Literal("attribute"), attribute: _ValueMatch, restriction: _ValueMatch }),
  Schema.Struct({ arm: Schema.Literal("property"), set: _ValueMatch, name: _ValueMatch, restriction: _ValueMatch }),
  Schema.Struct({ arm: Schema.Literal("material"), restriction: _ValueMatch }),
)

// Only the incidence and boolean arms recurse, and every incidence arm recurses through the SAME target carrier, so the
// nesting half is spelled once over its child type exactly as the shell intent family spells its own. Every column is a
// primitive by the producer's wire law, so decoded and encoded shapes coincide and one type serves both sides of the
// row — an authored predicate re-encodes with no projection twin to keep in step.
type _NodeMatchOf<T> = { readonly exact: string | null; readonly matching: T | null }

type _Nest<T> =
  | { readonly arm: "spatialContainer"; readonly container: _NodeMatchOf<T>; readonly reach: string }
  | { readonly arm: "composed"; readonly subKind: string; readonly whole: _NodeMatchOf<T> }
  | { readonly arm: "type"; readonly type: _NodeMatchOf<T> }
  | { readonly arm: "zone"; readonly group: _NodeMatchOf<T> }
  | { readonly arm: "connected"; readonly other: _NodeMatchOf<T>; readonly kind: string | null }
  | { readonly arm: "voided"; readonly subKind: string; readonly other: _NodeMatchOf<T> }
  | { readonly arm: "generic"; readonly wireName: string; readonly other: _NodeMatchOf<T> }
  | { readonly arm: "all"; readonly operands: ReadonlyArray<T> }
  | { readonly arm: "any"; readonly operands: ReadonlyArray<T> }
  | { readonly arm: "not"; readonly operand: T }

type PredicateWire = typeof _predicateLeaves.Type | _Nest<PredicateWire>

declare namespace PredicateWire {
  type ValueMatch = typeof _ValueMatch.Type
  type NodeMatch = _NodeMatchOf<PredicateWire>
  type Measure = typeof _Measure.Type
}

const _predicate: Schema.Schema<PredicateWire, PredicateWire> = Schema.suspend(() => PredicateWire)

// Exactly one leg populated — the producer refuses the both-and-neither shapes on its own rail, so the landing
// carries the same refusal as a filter whose identifier IS the refusal's coordinate in the `ParseError`.
const _NodeMatch = Schema.Struct({
  exact: Schema.NullOr(Schema.NonEmptyString),
  matching: Schema.NullOr(_predicate),
}).pipe(Schema.filter(
  (node) => (node.exact === null) !== (node.matching === null),
  { identifier: "node-match-exclusive" },
))

const PredicateWire: Schema.Schema<PredicateWire, PredicateWire> = Schema.Union(
  _predicateLeaves,
  Schema.Struct({ arm: Schema.Literal("spatialContainer"), container: _NodeMatch, reach: Schema.NonEmptyString }),
  Schema.Struct({ arm: Schema.Literal("composed"), subKind: Schema.NonEmptyString, whole: _NodeMatch }),
  Schema.Struct({ arm: Schema.Literal("type"), type: _NodeMatch }),
  Schema.Struct({ arm: Schema.Literal("zone"), group: _NodeMatch }),
  Schema.Struct({ arm: Schema.Literal("connected"), other: _NodeMatch, kind: Schema.NullOr(Schema.NonEmptyString) }),
  Schema.Struct({ arm: Schema.Literal("voided"), subKind: Schema.NonEmptyString, other: _NodeMatch }),
  Schema.Struct({ arm: Schema.Literal("generic"), wireName: Schema.NonEmptyString, other: _NodeMatch }),
  Schema.Struct({ arm: Schema.Literal("all"), operands: Schema.Array(_predicate) }),
  Schema.Struct({ arm: Schema.Literal("any"), operands: Schema.Array(_predicate) }),
  Schema.Struct({ arm: Schema.Literal("not"), operand: _predicate }),
)

// `_absent` folds the producer's typed absence on a scalar string column once: proto3 emits `""` for an unset
// singular string, so an authored material's `emissionUnit`, an acquired set's `materialId`, and a dielectric's
// `conductor` all arrive empty and read as `Option.none()`. The shipped operator owns it; a local twin is the drift defect.
const _absent: typeof Schema.OptionFromNonEmptyTrimmedString = Schema.OptionFromNonEmptyTrimmedString

const _Color = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number)
const _Weight = Schema.Number.pipe(Schema.between(0, 1)) // the unit interval every OpenPBR weight, ratio, and grain azimuth rides

// `MaterialWire` lands its receipt verbatim — capture evidence, fit conditioning, chromaticity/CCT grounding,
// the chart-solve tail separating a colour-corrected capture from a camera's guess, and model attribution a neural
// capture fills; an empty string is the producer's typed absence, never a hole. One decoded receipt serves BOTH
// wire dialects — the proto leg lands it by name on `TextureSetWire`, the msgpack leg by position at `MaterialWire`
// slot 3 — so no second provenance vocabulary exists.
const _Provenance = Schema.Struct({
  device: Schema.String,
  wavelengthCount: Schema.Int.pipe(Schema.nonNegative()),
  fitResidual: Schema.Number, // +Inf is a legal conditioning report; no finite() constraint belongs here
  measured: Schema.Boolean,
  method: Schema.String,
  angularSamples: Schema.Int.pipe(Schema.nonNegative()),
  fitConditionNumber: Schema.Number,
  fitRank: Schema.Int.pipe(Schema.nonNegative()),
  dominantWavelengthNm: Schema.Number,
  excitationPurity: Schema.Number,
  cctKelvin: Schema.Number,
  cctDuv: Schema.Number,
  modelCard: Schema.String,
  license: Schema.String,
  // The chart-solve tail: `calibrated` separates a chart-corrected capture from a camera's guess and
  // `calibrationDeltaE` carries the mean CIEDE2000 residual over the producer's measured patch set, so a receipt
  // reading `measured` on a photographed base colour is still gradeable; `modelArtefact` digests the inferred row's
  // own weights beside the `modelCard` naming them, so two revisions of one card separate without resolving bytes.
  // `calibrationDeltaE` carries EXPLICIT PRESENCE — the producer's `double?` writes nil for an uncalibrated
  // capture, and a zero here would read to any divergence gate as a perfect chart fit no solve produced.
  calibrated: Schema.Boolean,
  calibrationDeltaE: Schema.NullOr(Schema.Number),
  modelArtefact: Schema.String,
})

// [MIRROR_ORDER] — the msgpack appearance wires are POSITIONAL `[MessagePackObject]` records: the producer's
// `[Key(n)]` index IS the array position, so every wire tuple below spells its slots in KEY order and NEVER in the
// producer's declaration order — `OpenPbrGroupsWire` declares `SpecularRotation` mid-record yet keys it 29 and
// `GeometryThinWalled` last at 30, both APPENDED past the frozen block, so pre-append bytes decode unchanged when
// the missing trailing slot folds to the producer's stated default (rotation 0; thinWalled false, the OpenPBR
// closed-solid default). Position is the whole mirror contract because the array carries no names: a slot re-seated
// to its reading position decodes a neighbour's value silently. The reshape arms move POSITION to NAME only; every
// refinement re-proves on the named class after the mapping runs, exactly once.
const _ColorWire = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number, Schema.String) // WireColor Key(0..3): scene-linear r/g/b + the clipped hex the web swatch reads
const _Shade = Schema.Struct({ rgb: _Color, hex: Schema.String })
const _shaded = ([r, g, b, hex]: typeof _ColorWire.Type): typeof _Shade.Encoded => ({ rgb: [r, g, b], hex })
const _unshaded = ({ rgb: [r, g, b], hex }: typeof _Shade.Encoded): typeof _ColorWire.Type => [r, g, b, hex]

// `WireProvenance` Key(0..16) in index order; slot 15 is the producer's `double?` nil — the one numeric slot
// carrying explicit presence, decoded by the SAME `NullOr` the named field declares.
const _ProvenanceWire = Schema.Tuple(
  Schema.String, Schema.Number, Schema.Number, Schema.Boolean, Schema.String, Schema.Number, // Key 0..5: device, wavelengthCount, fitResidual, measured, method, angularSamples
  Schema.Number, Schema.Number, Schema.Number, Schema.Number, Schema.Number, Schema.Number, // Key 6..11: fitConditionNumber, fitRank, dominantWavelengthNm, excitationPurity, cctKelvin, cctDuv
  Schema.String, Schema.String, Schema.Boolean, Schema.NullOr(Schema.Number), Schema.String, // Key 12..16: modelCard, license, calibrated, calibrationDeltaE, modelArtefact
)
const _proved = (v: typeof _ProvenanceWire.Type): typeof _Provenance.Encoded => ({
  device: v[0], wavelengthCount: v[1], fitResidual: v[2], measured: v[3], method: v[4], angularSamples: v[5],
  fitConditionNumber: v[6], fitRank: v[7], dominantWavelengthNm: v[8], excitationPurity: v[9], cctKelvin: v[10],
  cctDuv: v[11], modelCard: v[12], license: v[13], calibrated: v[14], calibrationDeltaE: v[15], modelArtefact: v[16],
})
const _unproved = (p: typeof _Provenance.Encoded): typeof _ProvenanceWire.Type => [
  p.device, p.wavelengthCount, p.fitResidual, p.measured, p.method, p.angularSamples, p.fitConditionNumber,
  p.fitRank, p.dominantWavelengthNm, p.excitationPurity, p.cctKelvin, p.cctDuv, p.modelCard, p.license,
  p.calibrated, p.calibrationDeltaE, p.modelArtefact,
]

// `OpenPbrGroupsWire` Key(0..30) — the FULL OpenPBR Surface 1.1 parameter vector, one-for-one: the producer
// flattens `OpenPbrSurface` so a peer reconstructs the exact slab stack, never a lossy subset, and this mirror
// carries every band — subsurface, coat, fuzz, and thin-film included — because a dropped band repaints the
// producer's surface silently. The vector crosses NESTED at `MaterialWire` Key(1); the standalone census row binds
// this same declaration so the nested slot and the family row cannot drift.
const _PbrVector = Schema.Tuple(
  Schema.Number, _ColorWire, Schema.Number, Schema.Number, Schema.Number, // Key 0..4: baseWeight, baseColor, baseMetalness, baseDiffuseRoughness, baseSpecularTint
  Schema.Number, _ColorWire, Schema.Number, Schema.Number, Schema.Number, // Key 5..9: specularWeight, specularColor, specularRoughness, specularIor, specularAnisotropy
  Schema.Number, Schema.Number, // Key 10..11: transmissionWeight, transmissionRoughness
  Schema.Number, Schema.Number, Schema.Number, Schema.Number, // Key 12..15: subsurfaceWeight, subsurfaceRadiusR/G/B — mean-free-path scalars, never unit-interval
  Schema.Number, _ColorWire, Schema.Number, Schema.Number, // Key 16..19: coatWeight, coatColor, coatRoughness, coatIor
  Schema.Number, _ColorWire, Schema.Number, // Key 20..22: fuzzWeight, fuzzColor, fuzzRoughness
  Schema.Number, Schema.Number, Schema.Number, // Key 23..25: thinFilmWeight, thinFilmThickness, thinFilmIor
  _ColorWire, Schema.Number, Schema.Number, // Key 26..28: emissionColor, emissionLuminance, geometryOpacity
  Schema.optionalElement(Schema.Number), // Key 29: specularRotation — appended past the frozen block; absent decodes 0
  Schema.optionalElement(Schema.Boolean), // Key 30: geometryThinWalled — appended; absent decodes false, the closed-solid default
)
// `WireEmission` Key(0..5) — the admitted-emission receipt nested at `MaterialWire` Key(7); the positional twin the
// class field reshapes to names, per-field refinements re-proving on the named side.
const _EmissionVector = Schema.Tuple(
  Schema.Number, Schema.Number, // Key 0..1: dominantWavelengthNm, excitationPurity
  Schema.Number, Schema.Number, // Key 2..3: cctKelvin, cctDuv
  Schema.Number, Schema.Boolean, // Key 4..5: relativeLuminance, gamutMapped
)
const _vectored = (v: typeof _PbrVector.Type): typeof PbrGroups.Encoded => ({
  base: { weight: v[0], color: _shaded(v[1]), metalness: v[2], diffuseRoughness: v[3], specularTint: v[4] },
  specular: { weight: v[5], color: _shaded(v[6]), roughness: v[7], ior: v[8], anisotropy: v[9], rotation: v[29] ?? 0 },
  transmission: { weight: v[10], roughness: v[11] },
  subsurface: { weight: v[12], radius: [v[13], v[14], v[15]] },
  coat: { weight: v[16], color: _shaded(v[17]), roughness: v[18], ior: v[19] },
  fuzz: { weight: v[20], color: _shaded(v[21]), roughness: v[22] },
  thinFilm: { weight: v[23], thickness: v[24], ior: v[25] },
  emission: { color: _shaded(v[26]), luminance: v[27] },
  geometry: { opacity: v[28], thinWalled: v[30] ?? false },
})
const _unvectored = (g: typeof PbrGroups.Encoded): typeof _PbrVector.Type => [
  g.base.weight, _unshaded(g.base.color), g.base.metalness, g.base.diffuseRoughness, g.base.specularTint,
  g.specular.weight, _unshaded(g.specular.color), g.specular.roughness, g.specular.ior, g.specular.anisotropy,
  g.transmission.weight, g.transmission.roughness,
  g.subsurface.weight, g.subsurface.radius[0], g.subsurface.radius[1], g.subsurface.radius[2],
  g.coat.weight, _unshaded(g.coat.color), g.coat.roughness, g.coat.ior,
  g.fuzz.weight, _unshaded(g.fuzz.color), g.fuzz.roughness,
  g.thinFilm.weight, g.thinFilm.thickness, g.thinFilm.ior,
  _unshaded(g.emission.color), g.emission.luminance, g.geometry.opacity,
  g.specular.rotation, g.geometry.thinWalled,
]

class PbrGroups extends Schema.Class<PbrGroups>("PbrGroups")({
  base: Schema.Struct({ weight: _Weight, color: _Shade, metalness: _Weight, diffuseRoughness: _Weight, specularTint: _Weight }),
  // `anisotropy` and `rotation` are one grain: the ratio shapes the specular lobe and the azimuth orients it, with
  // `1` a HALF TURN on the OpenPBR/`.mtlx` convention the producer converts to radians at its own lower.
  specular: Schema.Struct({
    weight: _Weight,
    color: _Shade,
    roughness: _Weight,
    ior: Schema.Number.pipe(Schema.positive()),
    anisotropy: _Weight,
    rotation: _Weight,
  }),
  transmission: Schema.Struct({ weight: _Weight, roughness: _Weight }),
  subsurface: Schema.Struct({ weight: _Weight, radius: _Color }), // per-channel mean-free-path, nonNegative by physics not by unit interval
  coat: Schema.Struct({ weight: _Weight, color: _Shade, roughness: _Weight, ior: Schema.Number.pipe(Schema.positive()) }),
  fuzz: Schema.Struct({ weight: _Weight, color: _Shade, roughness: _Weight }),
  thinFilm: Schema.Struct({ weight: _Weight, thickness: Schema.Number.pipe(Schema.nonNegative()), ior: Schema.Number.pipe(Schema.positive()) }),
  emission: Schema.Struct({ color: _Shade, luminance: Schema.Number.pipe(Schema.nonNegative()) }),
  geometry: Schema.Struct({ opacity: _Weight, thinWalled: Schema.Boolean }),
}) {
  // The wire twin rides the owner: position moves to name in the reshape arm, every refinement re-proves here.
  static readonly FromVector: Schema.Schema<PbrGroups, typeof _PbrVector.Encoded> = Schema.transform(
    _PbrVector, PbrGroups, { strict: true, decode: _vectored, encode: _unvectored },
  )
}
class Material extends Schema.Class<Material>("Material")({
  // `MaterialWire` Key(0..6) by name: the `MaterialId` `family.name` seam identity crosses as the string it is —
  // never a digest; the mesh-to-appearance pairing key lives on the element graph, not on this wire.
  id: Schema.NonEmptyString,
  openPbr: PbrGroups, // Key(1): the full vector nests INLINE — no digest indirection exists on the producer's wire
  conductor: _absent, // Key(2): the `ConductorMetal` key, empty for a dielectric — the producer's typed absence
  provenance: _Provenance, // Key(3): the capture receipt, verbatim
  preview: _Shade, // Key(4): the resolved `SurfaceShade` scene-linear triple + clipped hex
  // The photometric grounding the producer's admission recorded: an ABSENT unit spells an authored emission whose
  // magnitude reads unread, so a bare multiplier and an admitted cd/m2 stay apart where a lone scalar collapses them.
  emissionUnit: _absent, // Key(5)
  emissionValue: Schema.Number.pipe(Schema.nonNegative()), // Key(6)
  // Key(7): the whole admitted-emission receipt — the producer's photometric resolve readouts (chromaticity, CCT+Duv
  // on the capture receipt's spelling, the MEASURED relative luminance its construction divided out, the gamut-map
  // witness no peer can re-derive). A trailing nullable record: absence — pre-widening bytes, or an authored
  // emission — reads Option.none(), never a zero-filled receipt claiming a measurement no admission took.
  emission: Schema.optionalWith(Schema.Struct({
    dominantWavelengthNm: Schema.Number, excitationPurity: Schema.Number,
    cctKelvin: Schema.Number, cctDuv: Schema.Number,
    relativeLuminance: Schema.Number.pipe(Schema.nonNegative()), gamutMapped: Schema.Boolean,
  }), { as: "Option" }),
}) {
  static readonly FromWire: Schema.Schema<Material, readonly [string, typeof _PbrVector.Encoded, string, typeof _ProvenanceWire.Encoded, typeof _ColorWire.Encoded, string, number, typeof _EmissionVector.Encoded | null | undefined]> = Schema.transform(
    Schema.Tuple(Schema.String, _PbrVector, Schema.String, _ProvenanceWire, _ColorWire, Schema.String, Schema.Number, Schema.optionalElement(Schema.NullOr(_EmissionVector))),
    Material,
    {
      strict: true,
      decode: ([id, vector, conductor, receipt, shade, emissionUnit, emissionValue, emission]) => ({
        id, openPbr: _vectored(vector), conductor, provenance: _proved(receipt), preview: _shaded(shade), emissionUnit, emissionValue,
        ...(emission == null ? {} : { emission: {
          dominantWavelengthNm: emission[0], excitationPurity: emission[1],
          cctKelvin: emission[2], cctDuv: emission[3],
          relativeLuminance: emission[4], gamutMapped: emission[5],
        } }),
      }),
      encode: (wire) => [
        wire.id, _unvectored(wire.openPbr), wire.conductor, _unproved(wire.provenance), _unshaded(wire.preview),
        wire.emissionUnit, wire.emissionValue,
        wire.emission == null ? null : [
          wire.emission.dominantWavelengthNm, wire.emission.excitationPurity,
          wire.emission.cctKelvin, wire.emission.cctDuv,
          wire.emission.relativeLuminance, wire.emission.gamutMapped,
        ],
      ],
    },
  )
}
// Field-for-field mirror of the C# seam record (`Rasm.Element` NODE_MODEL mint): its one wire leg is the
// `rasm.element.v1` `AppearanceWire` payload nested at `NodeWire` field 7 — never a wire family of its own,
// because no producer emits a standalone summary document. `Node` carries that payload whole and untyped, so no
// arm on this page seats a summary: this class is the shape a reader of that payload decodes against, and
// `ui/viewer/scene`'s `Pbr.index` consumes the roster off its own `GlbViewport.Appearance` document.
// The shape carries the XxHash128 dedup key plus the flat seven-value preview — scene-linear base
// color, the two lobe scalars, opacity, and the refractive flag DISTINCT from opacity. `appearanceKey` rides that
// payload's field 1 as 16 big-endian BYTES, so it lands through the byte twin and still meets
// `TextureSet.appearanceKey` on the one `ContentKey` brand the hex twin lands its own producer's spelling on; the
// flat scalars serve a consumer reading without the lobe graph, so no field re-derives from `PbrGroups` and no
// clamp, remap, or grouping forks the producer's semantics.
class AppearanceSummary extends Schema.Class<AppearanceSummary>("AppearanceSummary")({
  appearanceKey: Digest.FromBytes,
  baseColorR: Schema.Number.pipe(Schema.nonNegative()),
  baseColorG: Schema.Number.pipe(Schema.nonNegative()),
  baseColorB: Schema.Number.pipe(Schema.nonNegative()),
  metallic: _Weight,
  roughness: _Weight,
  opacity: _Weight,
  transmissive: Schema.Boolean,
}) {}

// `_transferRows` carries the five-tag vocabulary whole beside the one column the frozen fragment's own legality
// clause states: `plane` is true where the tag reaches a channel plane at all. The scene-referred subset DERIVES
// from that column under a two-way guard, so a sixth tag is one row and neither the roster nor the subset can drift
// off the other. `pq`/`hlg` are display transfers the C#-interior environment wire alone admits — both set documents
// carry roster-keyed CHANNEL planes, and a display-referred channel plane forks its stored value from its shading value.
const _transfers = ["linear", "srgb", "raw", "pq", "hlg"] as const
const _transferRows = {
  linear: { plane: true },
  srgb: { plane: true },
  raw: { plane: true },
  pq: { plane: false },
  hlg: { plane: false },
} as const satisfies { readonly [K in Texture.Transfer]: { readonly plane: boolean } }
type _PlaneTagged = {
  readonly [K in Texture.Transfer]: (typeof _transferRows)[K]["plane"] extends true ? K : never
}[Texture.Transfer]
const _sceneTransfers = ["linear", "srgb", "raw"] as const
type _SceneTransfer = (typeof _sceneTransfers)[number]
type _SceneWhole<K extends _PlaneTagged = _SceneTransfer> = K
type _SceneClosed<K extends _SceneTransfer = _PlaneTagged> = K

// `_depthRows` splits the store class on the two axes the wire laws read: `integer` decides which transfer a color
// channel is authored under, and `deep` decides what an 8-bit-only encoder leg and a lossy association conversion admit.
const _depths = ["u8", "u16", "f16", "f32"] as const
const _depthRows = {
  u8: { integer: true, deep: false },
  u16: { integer: true, deep: true },
  f16: { integer: false, deep: true },
  f32: { integer: false, deep: true },
} as const satisfies { readonly [K in Texture.Depth]: { readonly integer: boolean; readonly deep: boolean } }

const _mipPolicies = ["box", "kaiser", "normalRenormalize", "roughnessVariance", "none"] as const

// The three physical units the roster's own channels carry; every other channel is a dimensionless ratio, an index,
// or a normalized field and declares none. The column is what gives the millimetre height span, the nanometre
// thin-film thickness, and the photometric emission floor a declared home instead of a bind-site guess.
const _units = ["mm", "nm", "cd/m2"] as const

// Roster order carries the canonical channels — OpenPBR rows, then geometry, then derived; tuple position IS the
// set-key preimage rank both set documents order their rows by.
const _roles = [
  "base_weight", "base_color", "base_metalness", "base_diffuse_roughness", "base_specular_tint",
  "specular_weight", "specular_color", "specular_roughness", "specular_roughness_anisotropy",
  "specular_roughness_anisotropy_rotation", "specular_ior",
  "transmission_weight", "transmission_roughness", "subsurface_weight", "subsurface_radius",
  "coat_weight", "coat_color", "coat_roughness", "coat_ior", "fuzz_weight", "fuzz_color", "fuzz_roughness",
  "thin_film_weight", "thin_film_thickness", "thin_film_ior", "emission_color", "emission_luminance",
  "geometry_opacity", "geometry_normal", "geometry_coat_normal", "geometry_tangent", "geometry_coat_tangent",
  "height", "occlusion", "curvature",
] as const

// The channel fact carries the roster's five wire-bearing columns and NOTHING derived from them: `ch` the semantic
// component count, `transfer` the tag the channel is authored under, `neutral` the constant an absent packed slot, a
// mip gutter, and a UDIM hole fill with, `unit` the physical unit the value is expressed in, and `mip` the declared
// fold. Every plane law READS those five — the colorimetric class, the depth-coupled transfer, the storage-width
// floor, the admissible fold, and the scalar companion a false pack slot stamps — so a new channel is ONE row and no
// predicate widens. The arity distributes ONCE here, so a three-band neutral on a one-component channel cannot be
// written; a boolean standing in for `transfer` folds `linear` and `raw` into one class and admits `specular_ior` as
// a light quantity, and `null` on `unit` names a dimensionless ratio, an index, or a normalized field.
type _ChannelFacts<C extends 1 | 3 = 1 | 3> = C extends unknown ? {
    readonly ch: C
    readonly transfer: _SceneTransfer
    readonly neutral: C extends 1 ? readonly [number] : readonly [number, number, number]
    readonly unit: Texture.Unit | null
    readonly mip: Exclude<Texture.MipPolicy, "none">
  }
  : never

const _channelRows = {
  base_weight: { ch: 1, transfer: "linear", neutral: [1], unit: null, mip: "box" },
  base_color: { ch: 3, transfer: "srgb", neutral: [0.8, 0.8, 0.8], unit: null, mip: "kaiser" },
  base_metalness: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "box" },
  base_diffuse_roughness: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "roughnessVariance" },
  base_specular_tint: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "box" },
  specular_weight: { ch: 1, transfer: "linear", neutral: [1], unit: null, mip: "box" },
  specular_color: { ch: 3, transfer: "srgb", neutral: [1, 1, 1], unit: null, mip: "kaiser" },
  specular_roughness: { ch: 1, transfer: "linear", neutral: [0.3], unit: null, mip: "roughnessVariance" },
  specular_roughness_anisotropy: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "box" },
  // the scalar anisotropy-direction plane; mips correctly under box where a tangent vector plane cancels
  specular_roughness_anisotropy_rotation: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "box" },
  specular_ior: { ch: 1, transfer: "raw", neutral: [1.5], unit: null, mip: "box" },
  transmission_weight: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "box" },
  transmission_roughness: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "roughnessVariance" },
  subsurface_weight: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "box" },
  // a 3-band mean-free-path carrier in millimetres, never a colorimetric triple
  subsurface_radius: { ch: 3, transfer: "raw", neutral: [1, 0.5, 0.25], unit: "mm", mip: "box" },
  coat_weight: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "box" },
  coat_color: { ch: 3, transfer: "srgb", neutral: [1, 1, 1], unit: null, mip: "kaiser" },
  coat_roughness: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "roughnessVariance" },
  coat_ior: { ch: 1, transfer: "raw", neutral: [1.6], unit: null, mip: "box" },
  fuzz_weight: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "box" },
  fuzz_color: { ch: 3, transfer: "srgb", neutral: [1, 1, 1], unit: null, mip: "kaiser" },
  fuzz_roughness: { ch: 1, transfer: "linear", neutral: [0.5], unit: null, mip: "roughnessVariance" },
  thin_film_weight: { ch: 1, transfer: "linear", neutral: [0], unit: null, mip: "box" },
  // nanometres on the column; the micrometre divide is the `.mtlx` egress edge's
  thin_film_thickness: { ch: 1, transfer: "raw", neutral: [500], unit: "nm", mip: "box" },
  thin_film_ior: { ch: 1, transfer: "raw", neutral: [1.4], unit: null, mip: "box" },
  emission_color: { ch: 3, transfer: "srgb", neutral: [1, 1, 1], unit: null, mip: "kaiser" },
  emission_luminance: { ch: 1, transfer: "linear", neutral: [0], unit: "cd/m2", mip: "box" },
  geometry_opacity: { ch: 1, transfer: "linear", neutral: [1], unit: null, mip: "box" },
  geometry_normal: { ch: 3, transfer: "raw", neutral: [0, 0, 1], unit: null, mip: "normalRenormalize" },
  geometry_coat_normal: { ch: 3, transfer: "raw", neutral: [0, 0, 1], unit: null, mip: "normalRenormalize" },
  geometry_tangent: { ch: 3, transfer: "raw", neutral: [1, 0, 0], unit: null, mip: "normalRenormalize" },
  geometry_coat_tangent: { ch: 3, transfer: "raw", neutral: [1, 0, 0], unit: null, mip: "normalRenormalize" },
  // normalized [0,1]; the millimetre span rides the document's `heightScale`, never the column
  height: { ch: 1, transfer: "raw", neutral: [0.5], unit: null, mip: "box" },
  occlusion: { ch: 1, transfer: "linear", neutral: [1], unit: null, mip: "box" },
  // signed [-1,1]; an integer store carries the halved encoding
  curvature: { ch: 1, transfer: "raw", neutral: [0], unit: null, mip: "box" },
} as const satisfies { readonly [K in Texture.Role]: _ChannelFacts }

// Color channels author their transfer to FOLLOW their store — the roster's `srgb` rows encode display-referred at integer
// depth and scene-linear at float depth, and every other row is transfer-invariant across depth. Reading the roster
// column raw admits `base_color` as a linear 8-bit plane the shading rail then decodes a second time.
const _authored = (role: Texture.Role, depth: Texture.Depth): _SceneTransfer =>
  _channelRows[role].transfer === "srgb" && !_depthRows[depth].integer ? "linear" : _channelRows[role].transfer

// Roster folds declare a channel's DEFAULT and `box` its floor; `none` names the single-level plane alone, so
// a pyramid depth and a fold policy never disagree. That same column fixes the direction class — the
// `normalRenormalize` rows ARE the direction triples, and only a direction plane may store two components and
// reconstruct the third, so a three-band millimetre carrier in `rg16` loses its third band with nothing to recover it.
const _mipLawful = (role: Texture.Role, mips: number, policy: Texture.MipPolicy): boolean =>
  mips === 1 ? policy === "none" : policy === "box" || policy === _channelRows[role].mip
const _widthFloor = (role: Texture.Role): 1 | 2 | 4 =>
  _channelRows[role].ch === 1 ? 1 : _channelRows[role].mip === "normalRenormalize" ? 2 : 4

// `_planeRows` projects each storage key onto the three facts the wire laws read: its store class, its texel width,
// and `web` — whether a browser transcoder can reach the store at all. The one-and-two-component sixteen-bit integer
// stores have no Vulkan format row in the KTX2 read path, so they are producer-side and desktop-native only, which
// is load-bearing precisely where `_widthFloor` routes every direction plane to width 2: the natural high-precision
// normal store is the undecodable one. The column generates that refusal exactly like `_payloadRows.wire` and
// `_transferRows.plane` generate theirs, so a browser-decodable store set is derived and never a hand list.
const _planeFormats = ["r8", "r16", "r16f", "r32f", "rg8", "rg16", "rg16f", "rg32f", "rgba8", "rgba16", "rgba16f", "rgba32f"] as const
const _planeRows = {
  r8: { depth: "u8", width: 1, web: true }, r16: { depth: "u16", width: 1, web: false },
  r16f: { depth: "f16", width: 1, web: true }, r32f: { depth: "f32", width: 1, web: true },
  rg8: { depth: "u8", width: 2, web: true }, rg16: { depth: "u16", width: 2, web: false },
  // the float two-component stores carry Vulkan format rows, so rg16f is the web re-route for the direction planes rg16 cannot serve
  rg16f: { depth: "f16", width: 2, web: true }, rg32f: { depth: "f32", width: 2, web: true },
  rgba8: { depth: "u8", width: 4, web: true }, rgba16: { depth: "u16", width: 4, web: true },
  rgba16f: { depth: "f16", width: 4, web: true }, rgba32f: { depth: "f32", width: 4, web: true },
} as const satisfies { readonly [K in Texture.PlaneFormat]: { readonly depth: Texture.Depth; readonly width: 1 | 2 | 4; readonly web: boolean } }

// `_payloadRows` carries all five KTX2 payload classes beside the three columns their refusals read: `wire` the
// legality the viewer's Basis transcoder path decides, `block` whether the file holds block data direct, and `ldr` the
// MEASURED 8-bit store bound both encoder legs raise on. The wire subset derives from `wire` under a two-way guard, so
// admitting a future transcodable payload is one column flip and every filter follows it with no literal to chase.
const _payloads = ["rawBcn", "uastc", "etc1s", "astc", "none"] as const
const _payloadRows = {
  rawBcn: { wire: false, block: true, ldr: true },
  uastc: { wire: true, block: false, ldr: true },
  etc1s: { wire: true, block: false, ldr: true },
  astc: { wire: false, block: true, ldr: true },
  none: { wire: true, block: false, ldr: false },
} as const satisfies { readonly [K in Texture.Payload]: { readonly wire: boolean; readonly block: boolean; readonly ldr: boolean } }
type _Wired = {
  readonly [K in Texture.Payload]: (typeof _payloadRows)[K]["wire"] extends true ? K : never
}[Texture.Payload]
const _blockFormats = ["bc1", "bc2", "bc3", "bc4", "bc5", "bc6h", "bc7", "none"] as const
const _wirePayloads = ["uastc", "etc1s", "none"] as const
type _PayloadWired<K extends _Wired = Texture.WirePayload> = K
type _PayloadClosed<K extends Texture.WirePayload = _Wired> = K

const _alphaModes = ["straight", "associated", "none"] as const
const _conventions = ["gl", "dx"] as const

// `_containerRows` carries the frozen fragment's twelve-row file-container roster WHOLE — a container one branch
// alone writes still rides it, refused by roster membership rather than by an unknown key — beside the three columns
// the wire laws read: `alpha` the canonical association encode converts to (the jxl/avif rows are the measured
// no-premultiplication-seat posture of the provisioned encoders), `pyramid` whether the file holds its OWN mip chain,
// which is the column the plane-level list law generates its length off, and `plane` whether the container reaches a
// CHANNEL plane at all. The eight-bit lossless preview row admits for a thumbnail egress and never for a channel, and
// its own producer rules the set egress grammar unable to mint one — so the carve generates like every other roster
// refusal and a second preview-class container is one column value, never a hand exclusion in the lawfulness chain.
const _containers = ["png16", "tiff16", "tiff_f32", "webp", "qoi", "exr", "exr_deep", "hdr", "ktx2", "jxl", "jxl_f16", "avif12"] as const
const _containerRows = {
  png16: { alpha: "straight", pyramid: false, plane: true },
  tiff16: { alpha: "straight", pyramid: false, plane: true },
  tiff_f32: { alpha: "straight", pyramid: false, plane: true },
  webp: { alpha: "straight", pyramid: false, plane: true },
  qoi: { alpha: "straight", pyramid: false, plane: false },
  exr: { alpha: "associated", pyramid: false, plane: true },
  exr_deep: { alpha: "associated", pyramid: false, plane: true },
  hdr: { alpha: "none", pyramid: false, plane: true },
  ktx2: { alpha: "straight", pyramid: true, plane: true },
  jxl: { alpha: "straight", pyramid: false, plane: true },
  jxl_f16: { alpha: "straight", pyramid: false, plane: true },
  avif12: { alpha: "straight", pyramid: false, plane: true },
} as const satisfies { readonly [K in Texture.Container]: { readonly alpha: Texture.AlphaMode; readonly pyramid: boolean; readonly plane: boolean } }
// Straight-to-associated conversion quantizes catastrophically at low alpha below 16 bits, so a plane whose container
// fixes an association differing from its declared mode admits at a deep store alone; a `none` plane carries nothing
// to convert and passes whole.
const _associationLawful = (mode: Texture.AlphaMode, container: Texture.Container, depth: Texture.Depth): boolean =>
  mode === "none" || _containerRows[container].alpha === mode || _depthRows[depth].deep

// `_layerRows` fixes the extent each layer law admits where the concept has one — an unlayered set holds one plane and
// a cube holds six faces; the open laws bound their extent at the producer, so `null` imposes nothing here.
const _layerLaws = ["none", "cubeFaces", "array", "volume", "frames"] as const
const _layerRows = {
  none: { extent: 1 }, cubeFaces: { extent: 6 },
  array: { extent: null }, volume: { extent: null }, frames: { extent: null },
} as const satisfies { readonly [K in Texture.LayerLaw]: { readonly extent: number | null } }
// `_packRows` fixes each packing order in slot order beside the ONE legality column the fragment states: `slots` is
// the roster `present` indexes, so a packed channel is addressed by its pack row and the roster names which
// standalone plane row then cannot exist, and `gltf` is whether the order crosses to a glTF consumer at all. The
// occlusion-first order IS the glTF KHR occlusion-plus-metallic-roughness read order and matches a three-component
// sampler's `.r`/`.g`/`.b` convention; the inverted order swaps R and B, so a consumer binding it to those slots
// reads occlusion as metalness — a refusal the consumer can only declare off a column it can read.
const _packs = ["orm", "mra"] as const
const _packRows = {
  orm: { slots: ["occlusion", "specular_roughness", "base_metalness"], gltf: true },
  mra: { slots: ["base_metalness", "specular_roughness", "occlusion"], gltf: false },
} as const satisfies { readonly [K in Texture.Pack]: { readonly slots: readonly [Texture.Role, Texture.Role, Texture.Role]; readonly gltf: boolean } }

// One exported anchor for the frozen shared texture vocabulary: the roster TUPLES, each key type derived off its
// own tuple, PLUS this page's own wire-legality column tables on `rows` — channel, container, depth, plane, pack —
// because the refusals those columns declare (`_planeRows.web`, `_packRows.gltf`, the channel neutrals) are the ui
// bind's to RAISE, and a consumer told to raise over a column must be able to read it. FOREIGN owners' columns
// (the data plane's CLI and data-format columns) stay with their owners and key off these tuples, so a fragment
// re-freeze breaks at ONE declaration in every module rather than forking toward whichever page a writer opened.
// Derived subsets (`_sceneTransfers`, `_wirePayloads`, `_packFormats`) close against their own anchor row and
// never enter the anchor a second time. Every row table is `as const satisfies` — a mapped ANNOTATION erases the
// row literals, collapsing every `extends true` derivation beside it (`_Wired`) to `never` while it reads correct.
declare namespace Texture {
  type AlphaMode = (typeof _alphaModes)[number]
  type Container = (typeof _containers)[number]
  type Convention = (typeof _conventions)[number]
  type Depth = (typeof _depths)[number]
  type LayerLaw = (typeof _layerLaws)[number]
  type MipPolicy = (typeof _mipPolicies)[number]
  type Pack = (typeof _packs)[number]
  type Payload = (typeof _payloads)[number]
  type PlaneFormat = (typeof _planeFormats)[number]
  type Role = (typeof _roles)[number]
  type Transfer = (typeof _transfers)[number]
  type Unit = (typeof _units)[number]
  type WirePayload = (typeof _wirePayloads)[number]
  type Shape = Types.Simplify<{
    readonly alphaModes: typeof _alphaModes
    readonly containers: typeof _containers
    readonly conventions: typeof _conventions
    readonly depths: typeof _depths
    readonly layerLaws: typeof _layerLaws
    readonly mipPolicies: typeof _mipPolicies
    readonly packs: typeof _packs
    readonly payloads: typeof _payloads
    readonly planeFormats: typeof _planeFormats
    readonly roles: typeof _roles
    readonly transfers: typeof _transfers
    readonly units: typeof _units
    readonly wirePayloads: typeof _wirePayloads
    readonly rows: Types.Simplify<{
      readonly channel: typeof _channelRows
      readonly container: typeof _containerRows
      readonly depth: typeof _depthRows
      readonly plane: typeof _planeRows
      readonly pack: typeof _packRows
    }>
  }>
}

const Texture: Texture.Shape = {
  alphaModes: _alphaModes,
  containers: _containers,
  conventions: _conventions,
  depths: _depths,
  layerLaws: _layerLaws,
  mipPolicies: _mipPolicies,
  packs: _packs,
  payloads: _payloads,
  planeFormats: _planeFormats,
  roles: _roles,
  transfers: _transfers,
  units: _units,
  wirePayloads: _wirePayloads,
  rows: { channel: _channelRows, container: _containerRows, depth: _depthRows, plane: _planeRows, pack: _packRows },
}

// ONE address triple per stored plane FILE. The two producers differ in exactly one field-level fact — the C#
// document names the address `blob` on the X32 spelling, the python document names it `digest` on the lowercase
// brand — so the triple is ONE variant declaration whose address column carries both encodings and whose `file` and
// `byteLength` are shared; each document's schema is an `extract`, and a third producer is one variant key. Two
// parallel struct declarations beside a schema-parameterized factory is the hand-rolled shape this deletes.
// Every addressed plane is a LEVEL-ORDERED list of these triples — entry 0 the base level — and `_leveled` generates
// the length law off the container's `pyramid` column: a self-pyramiding container holds ONE entry whatever `mips`
// declares, every other container one entry per level. A scalar address beside a `mips` count is the
// undigested-pyramid shape the list replaces.
const _producer = VariantSchema.make({ variants: ["web", "proto"], defaultVariant: "web" })

const _PlaneRef = _producer.Struct({
  file: Schema.NonEmptyString, // the egress leaf relative to the set directory; the served-asset join consumes it verbatim
  address: _producer.Field({ web: Digest.FromX32, proto: ContentKey }).pipe(
    _producer.fieldFromKey({ web: "blob", proto: "digest" }),
  ),
  byteLength: Schema.BigIntFromSelf, // the python wire's snake_case byte_length arrives through the generated message's lowerCamel local
})

const _PlaneRefWeb = _producer.extract("web")(_PlaneRef)
const _PlaneRefProto = _producer.extract("proto")(_PlaneRef)

const _leveled = (container: Texture.Container, mips: number, held: number): boolean =>
  held === (_containerRows[container].pyramid ? 1 : mips)

const _ascending = (strict: boolean) => (values: ReadonlyArray<number>): boolean =>
  Array.every(
    Array.zipWith(values, Array.drop(values, 1), (prior, next) => (strict ? prior < next : prior <= next)),
    Function.identity,
  )
const _rosterOrdered = (rows: ReadonlyArray<{ readonly role: Texture.Role }>): boolean =>
  _ascending(true)(Array.map(rows, (row) => _roles.indexOf(row.role)))

// The python producer's CompanionPolicy.RENDER emits TWO entries for one role — the primary plus a sampled
// companion, distinguished by `container` — so the manifest's map key is `(role, container)`, never role alone:
// roster order holds non-strictly across the role axis while each equal-role run keeps its containers distinct.
const _companionKeyed = (rows: ReadonlyArray<{ readonly role: Texture.Role; readonly container: string }>): boolean =>
  _ascending(false)(Array.map(rows, (row) => _roles.indexOf(row.role)))
    && Array.every(
      Array.zipWith(rows, Array.drop(rows, 1), (prior, next) => prior.role !== next.role || prior.container !== next.container),
      Function.identity,
    )

// Plane-row laws span both documents, projected off each row's own column names: roster semantic count,
// depth-coupled authored transfer, the measured 8-bit store every block-compressed payload admits, and the
// container's own channel-plane legality — the preview-class row its producer declares unminteable from a set leaf.
const _planeLawful = (
  role: Texture.Role,
  channels: number,
  transfer: _SceneTransfer,
  depth: Texture.Depth,
  payload: Texture.WirePayload,
  container: Texture.Container,
): boolean =>
  channels === _channelRows[role].ch
  && transfer === _authored(role, depth)
  && (!_payloadRows[payload].ldr || !_depthRows[depth].deep)
  // a DIMENSIONED channel carries no normalization: the roster declares millimetres, nanometres, and cd/m2 outright
  // and no wire column carries a scale, so an integer store has nothing to express them in — `height` proves the
  // pattern from the other side, normalized on the row with its physical span riding the set's own `heightScale`
  && (_channelRows[role].unit === null || !_depthRows[depth].integer)
  && _containerRows[container].plane

// `_packDisjoint` refuses a channel addressed twice under one set key — a packed slot's channel is carried by its
// pack row ALONE, and a standalone plane row beside it leaves a consumer reading whichever it resolved first.
const _packDisjoint = (
  rows: ReadonlyArray<{ readonly role: Texture.Role }>,
  packs: ReadonlyArray<{ readonly pack: Texture.Pack; readonly present: readonly [boolean, boolean, boolean] }>,
): boolean =>
  Array.every(packs, (entry) =>
    Array.every(
      _packRows[entry.pack].slots,
      (role, slot) => !entry.present[slot] || !Array.some(rows, (row) => row.role === role),
    ))

const _MariTiles = Schema.Array(Schema.Int.pipe(Schema.greaterThanOrEqualTo(1001))).pipe(
  Schema.filter((tiles) => _ascending(true)(tiles) || "<udim-tiles-unordered>", { identifier: "MariAscending" }),
)

// Packs occupy every component, so a storage row DERIVES as the four-wide half of the format roster under the
// same two-way close every other subset takes. Both documents' pack rows are otherwise one shape whose only axis is
// the producer's address spelling — which is why the row is the same variant declaration the triple is — and the row
// carries NO mip-policy column by design: each slot mips under its own channel's roster fold, so one policy across a
// pack is the defect a policy column would invite.
type _PackFormat = {
  readonly [K in Texture.PlaneFormat]: (typeof _planeRows)[K]["width"] extends 4 ? K : never
}[Texture.PlaneFormat]
const _packFormats = ["rgba8", "rgba16", "rgba16f", "rgba32f"] as const
type _PackWidened<K extends _PackFormat = (typeof _packFormats)[number]> = K
type _PackClosed<K extends (typeof _packFormats)[number] = _PackFormat> = K

// The level-list law is ONE filter both documents' pack rows take, applied after `extract` because a variant
// declaration carries fields and a refusal rides a schema.
const _packLeveled = <
  A extends { readonly container: Texture.Container; readonly mips: number; readonly levels: ReadonlyArray<unknown> },
  I,
  R,
>(row: Schema.Schema<A, I, R>): Schema.Schema<A, I, R> =>
  row.pipe(
    Schema.filter((entry) => _leveled(entry.container, entry.mips, entry.levels.length) || "<pack-levels-unaddressed>", {
      identifier: "PlaneLevels",
    }),
  )

const _PackRow = _producer.Struct({
  pack: Schema.Literal(..._packs),
  present: Schema.Tuple(Schema.Boolean, Schema.Boolean, Schema.Boolean), // three flags in slot order; a false slot carries its channel neutral
  format: Schema.Literal(..._packFormats),
  container: Schema.Literal(..._containers),
  mips: Schema.Int.pipe(Schema.positive()),
  // level-ordered; the pack name is the <channel> slot of each leaf, and the address spelling is the document's own
  levels: _producer.Field({
    web: Schema.NonEmptyArray(_PlaneRefWeb),
    proto: Schema.NonEmptyArray(_PlaneRefProto),
  }),
})

const _PackRowWeb = _packLeveled(_producer.extract("web")(_PackRow))
const _PackRowProto = _packLeveled(_producer.extract("proto")(_PackRow))

const _ChannelRow = Schema.Struct({
  role: Schema.Literal(..._roles),
  transfer: Schema.Literal(..._sceneTransfers),
  format: Schema.Literal(..._planeFormats),
  container: Schema.Literal(..._containers), // the FILE container; the association gate, the plane carve, and the level-list law all select on it
  channels: Schema.Literal(1, 3), // the SEMANTIC component count — the roster's own column image; storage width is `format`'s
  alphaMode: Schema.Literal(..._alphaModes),
  mips: Schema.Int.pipe(Schema.positive()),
  mipPolicy: Schema.Literal(..._mipPolicies),
  blockFormat: Schema.Literal(..._blockFormats),
  ktxPayload: Schema.Literal(..._wirePayloads),
  levels: Schema.NonEmptyArray(_PlaneRefWeb), // level-ordered addresses; entry 0 is the base level
}).pipe(
  Schema.filter(
    (row) =>
      (_planeLawful(row.role, row.channels, row.transfer, _planeRows[row.format].depth, row.ktxPayload, row.container)
        && _planeRows[row.format].width >= _widthFloor(row.role)
        && _mipLawful(row.role, row.mips, row.mipPolicy)
        // block data rides `rawBcn` alone and `rawBcn` never crosses, so the refusal generates off the payload table
        && (row.blockFormat === "none" || _payloadRows[row.ktxPayload].block)
        && (_planeRows[row.format].width === 4 || row.alphaMode === "none")
        // a payload column is the container's own: it reads as vacancy off a non-KTX2 file and names a payload on one
        && (row.ktxPayload === "none" || row.container === "ktx2")
        && _associationLawful(row.alphaMode, row.container, _planeRows[row.format].depth)
        && _leveled(row.container, row.mips, row.levels.length))
        || "<channel-row-unlawful>",
    { identifier: "PlaneLawful" },
  ),
)

const _PressReceipt = Schema.Struct({
  backend: Schema.Literal("cpu"), // a GPU press yields a preview carrying no set and no key, so `webgpu` on a persisted receipt is the decode refusal
  planKey: Digest.FromX32,
  graphKey: Digest.FromX32,
  seed: Schema.BigIntFromSelf, // the splitmix64 seed replaying the per-texel jitter
  texels: Schema.BigIntFromSelf,
  elapsedMs: Schema.Number.pipe(Schema.nonNegative()),
  gpuDeltaMax: Schema.optionalWith(Schema.Number, { as: "Option" }), // absent until a parity run measures it; telemetry, never a key input
  // The press's two quality tallies at wire grain: `downgraded` COUNTS the channels whose paired mip policy fell to
  // the box floor and `faultedTexels` SUMS the neutral-filled texels across every channel, so a set that pressed
  // clean and one that degraded per plane read apart on the analytics plane rather than on `elapsedMs`.
  downgraded: Schema.Int.pipe(Schema.nonNegative()),
  faultedTexels: Schema.BigIntFromSelf,
})

class TextureSet extends Schema.Class<TextureSet>("TextureSet")(Schema.Struct({
  appearanceKey: Digest.FromX32, // the seam key this set hangs BEHIND, never a column of it
  setKey: Digest.FromX32, // streaming fold over the channel-ordered plane digests, seed zero
  materialId: _absent, // the producer writes `family.name`, or empty for an acquired set
  conductor: _absent, // the `ConductorMetal` key, or empty for a dielectric
  width: Schema.Int.pipe(Schema.positive()),
  height: Schema.Int.pipe(Schema.positive()),
  layers: Schema.Int.pipe(Schema.positive()), // the producer admits >= 1 and proto3 elides only zero, so an absent field is the invalid document
  layerLaw: Schema.Literal(..._layerLaws),
  normalConvention: Schema.Literal(..._conventions), // ingest-source record; the plane bytes are always gl
  alphaMode: Schema.Literal(..._alphaModes), // set-level declaration; a channel row may narrow to none
  heightScale: Schema.Number.pipe(Schema.nonNegative()), // the mm span the [0,1] height plane normalizes against
  tiled: Schema.Boolean, // TileGate-proven coherence carried from the producer, never a caller assertion
  udimTiles: _MariTiles,
  channels: Schema.Array(_ChannelRow).pipe(
    Schema.filter((rows) => _rosterOrdered(rows) || "<channel-roster-disorder>", { identifier: "RosterOrdered" }),
  ),
  packs: Schema.Array(_PackRowWeb),
  provenance: _Provenance,
  press: Schema.optionalWith(_PressReceipt, { as: "Option" }), // absent for an ingested set
}).pipe(
  Schema.filter((set) => _packDisjoint(set.channels, set.packs) || "<packed-channel-duplicated>", {
    identifier: "PackDisjoint",
  }),
  // Layer laws naming a fixed extent and a `layers` count disagreeing with it are two readings of one set, and every
  // consumer resolves whichever it read first — a five-face cube renders as an array nothing raises on.
  Schema.filter(
    (set) => _layerRows[set.layerLaw].extent === null || set.layers === _layerRows[set.layerLaw].extent
      || "<layer-extent-mismatch>",
    { identifier: "LayerExtent" },
  ),
  // One association governs the whole set: a channel row NARROWS to `none` and never declares a different mode, so a
  // consumer un-premultiplying against the set's declaration cannot meet a plane authored under the other one.
  Schema.filter(
    (set) =>
      Array.every(set.channels, (row) => row.alphaMode === set.alphaMode || row.alphaMode === "none")
        || "<channel-association-fork>",
    { identifier: "AlphaNarrowed" },
  ),
)) {}

const _MapRow = Schema.Struct({
  role: Schema.Literal(..._roles),
  colorSpace: Schema.Literal(..._sceneTransfers), // a roster-keyed channel plane; the dome products ride `ibl`, which declares no transfer
  depth: Schema.Literal(..._depths),
  container: Schema.Literal(..._containers), // the wire's own column name; the `DeepFormat` roster is its python transcription
  channels: Schema.Literal(1, 3),
  mips: Schema.Int.pipe(Schema.positive()),
  ktxPayload: Schema.Literal(..._wirePayloads),
  levels: Schema.NonEmptyArray(_PlaneRefProto), // level-ordered addresses; each entry's `file` is the egress leaf the served-asset join consumes verbatim
  tool: Schema.Literal("ktx", "imagecodecs", "pyvips", "openexr"), // the map's OWN producing tool
  toolVersion: Schema.NonEmptyString, // the leg version the producer's probe recorded for THIS map
}).pipe(
  Schema.filter(
    (row) =>
      (_planeLawful(row.role, row.channels, row.colorSpace, row.depth, row.ktxPayload, row.container)
        // a payload column is the container's own: it reads as vacancy off a non-KTX2 file and names a payload on one
        && (row.ktxPayload === "none" || row.container === "ktx2")
        && _leveled(row.container, row.mips, row.levels.length))
        || "<map-row-unlawful>",
    { identifier: "PlaneLawful" },
  ),
)

const _Ibl = Schema.Struct({
  sh9: Schema.Array(Schema.Number).pipe(Schema.itemsCount(27)), // band-major, RGB interleaved, under the frozen SH9 layout
  equirect: _PlaneRefProto, // the source equirect plane; 2:1 extent enforced at the producer's admit
  cubemap: Schema.optionalWith(_PlaneRefProto, { as: "Option" }), // ONE address — a single self-pyramiding KTX2 cube container holding all six faces
  preview: Schema.optionalWith(_PlaneRefProto, { as: "Option" }), // the display-referred gain-map preview; the one product whose read-side intensity is baked
  specular: Schema.Array(_PlaneRefProto), // GGX prefilter pyramid — LEVELS, level-ordered under the plane-level list law
  roughnessPerMip: Schema.Array(Schema.Number.pipe(Schema.between(0, 1))),
  brdfLut: _PlaneRefProto, // the split-sum BRDF LUT
  luminanceCdf: Schema.optionalWith(_PlaneRefProto, { as: "Option" }), // absent disables importance sampling
  intensity: Schema.Number.pipe(Schema.nonNegative()), // applied on read, never baked into the planes
  upAxis: Schema.Literal("z"), // frozen; a y document is the decode refusal, and a Y-up runtime remaps the direction basis at the read
  rotation: Schema.Number.pipe(Schema.filter((rad) => rad >= 0 && rad < 2 * Math.PI, { identifier: "RadianTurn" })), // about +Z, applied on read
}).pipe(
  Schema.filter(
    (entry) =>
      (entry.roughnessPerMip.length === entry.specular.length && _ascending(false)(entry.roughnessPerMip))
        || "<specular-pyramid-mismatch>",
    { identifier: "MipRoster" },
  ),
)

class AssetSetManifest extends Schema.Class<AssetSetManifest>("AssetSetManifest")(Schema.Struct({
  manifestKey: ContentKey, // merkle fold over the roster-ordered plane digests; the lowercase python spelling lands the brand directly
  kind: Schema.Literal("pbr_set", "hdri", "ibl"),
  source: Schema.NonEmptyString.pipe(
    Schema.filter((root) => !root.startsWith("/") || "<absolute-host-path>", { identifier: "PortableSource" }),
  ), // ingest root or generator id; never a host path
  width: Schema.Int.pipe(Schema.positive()),
  height: Schema.Int.pipe(Schema.positive()),
  normalConvention: Schema.Literal(..._conventions),
  alphaMode: Schema.Literal(..._alphaModes),
  udim: Schema.Literal("none", "mari"),
  udimTiles: _MariTiles,
  tiled: Schema.Boolean, // DECLARED, carried from producer or verifier — python synthesizes no tiling
  maps: Schema.Array(_MapRow).pipe(
    // `(role, container)` is the map key — a CompanionPolicy.RENDER set legitimately carries a primary and a
    // sampled companion for one role, so the strict per-role order gate is the TextureSetWire roster's, not this one.
    Schema.filter((rows) => _companionKeyed(rows) || "<map-roster-disorder>", { identifier: "CompanionKeyed" }),
  ),
  packs: Schema.Array(_PackRowProto),
  ibl: Schema.optionalWith(_Ibl, { as: "Option" }),
  unresolved: Schema.Array(Schema.NonEmptyString), // filename stems no alias claimed — the classify fault-monoid accumulation
  heightScale: Schema.Number.pipe(Schema.nonNegative()), // 0.0 = no height plane
  licenseClass: Schema.Literal("permissive", "copyleft", "open_rail", "research", "blocked"),
}).pipe(
  Schema.filter((manifest) => _packDisjoint(manifest.maps, manifest.packs) || "<packed-channel-duplicated>", {
    identifier: "PackDisjoint",
  }),
  // `ibl` is the ONLY address of a dome plane — `maps` rows are roster channels — so a `pbr_set` carrying one claims
  // a product it never assembled; the dome kinds admit it, and whether they REQUIRE it stays the producer's.
  Schema.filter(
    (manifest) => manifest.kind !== "pbr_set" || Option.isNone(manifest.ibl) || "<ibl-on-pbr-set>",
    { identifier: "IblKind" },
  ),
  // Tile rosters ARE the UDIM declaration — the C# document carries no `udim` column and reads emptiness as the
  // discriminant, so a manifest declaring one and filling the other hands its two consumers opposite grammars.
  Schema.filter(
    (manifest) => (manifest.udim === "mari") === Array.isNonEmptyReadonlyArray(manifest.udimTiles)
      || "<udim-declaration-fork>",
    { identifier: "UdimDeclared" },
  ),
  Schema.filter(
    (manifest) =>
      Array.every(manifest.maps, (row) => _associationLawful(manifest.alphaMode, row.container, row.depth))
        || "<association-conversion-quantized>",
    { identifier: "AssociationLawful" },
  ),
)) {}

const _Position = Schema.Tuple(Schema.Number, Schema.Number, Schema.optionalElement(Schema.Number))
const _Point = Schema.TaggedStruct("Point", { coordinates: _Position })
const _MultiPoint = Schema.TaggedStruct("MultiPoint", { coordinates: Schema.Array(_Position) })
const _LineString = Schema.TaggedStruct("LineString", { coordinates: Schema.Array(_Position) })
const _MultiLineString = Schema.TaggedStruct("MultiLineString", { coordinates: Schema.Array(Schema.Array(_Position)) })
const _Polygon = Schema.TaggedStruct("Polygon", { coordinates: Schema.Array(Schema.Array(_Position)) })
const _MultiPolygon = Schema.TaggedStruct("MultiPolygon", { coordinates: Schema.Array(Schema.Array(Schema.Array(_Position))) })
const _Collection = Schema.TaggedStruct("GeometryCollection", {
  geometries: Schema.Array(Schema.suspend((): Schema.Schema<GeoFeature.Geometry> => _Geometry)),
})
const _Geometry = Schema.Union(_Point, _MultiPoint, _LineString, _MultiLineString, _Polygon, _MultiPolygon, _Collection)

const _CRS = {
  4326: { kind: "geographic", unit: "degree" },
  3857: { kind: "projected", unit: "metre" },
  4979: { kind: "geographic", unit: "degree" },
} as const

const _ZOOM_CEILING = 30
const _Tile = Schema.Struct({
  zoom: Schema.Int.pipe(Schema.between(0, _ZOOM_CEILING)),
  x: Schema.Int.pipe(Schema.nonNegative()),
  y: Schema.Int.pipe(Schema.nonNegative()),
}).pipe(Schema.filter((tile) => tile.x < 2 ** tile.zoom && tile.y < 2 ** tile.zoom, { identifier: "TileInGrid" }))

class GeoFeature extends Schema.Class<GeoFeature>("GeoFeature")({
  key: Schema.NonEmptyString,
  srid: Schema.Int.pipe(Schema.positive()),
  wkb: Schema.Uint8ArrayFromSelf,
  properties: Schema.Record({ key: Schema.String, value: Schema.Unknown }),
}) {
  static readonly Geometry: typeof _Geometry = _Geometry
  static readonly Extent = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number, Schema.Number)
  static readonly Crs: {
    readonly rows: typeof _CRS
    readonly of: (srid: number) => Option.Option<GeoFeature.Crs>
  } = {
    rows: _CRS,
    of: (srid) => (srid in _CRS ? Option.some(_CRS[srid as GeoFeature.Srid]) : Option.none()),
  }
  static readonly Tile: {
    readonly schema: typeof _Tile
    readonly quadkey: (tile: GeoFeature.Tile) => string
    readonly parent: (tile: GeoFeature.Tile) => Option.Option<GeoFeature.Tile>
    readonly children: (tile: GeoFeature.Tile) => ReadonlyArray<GeoFeature.Tile>
  } = {
    schema: _Tile,
    quadkey: (tile) =>
      tile.zoom === 0
        ? ""
        : Array.join(
            Array.makeBy(tile.zoom, (rank) => {
              const bit = tile.zoom - rank - 1
              return String((((tile.y >> bit) & 1) << 1) | ((tile.x >> bit) & 1))
            }),
            "",
          ),
    parent: (tile) =>
      tile.zoom === 0 ? Option.none() : Option.some(_Tile.make({ zoom: tile.zoom - 1, x: tile.x >> 1, y: tile.y >> 1 })),
    children: (tile) =>
      tile.zoom === _ZOOM_CEILING
        ? []
        : Array.map(
            [[0, 0], [1, 0], [0, 1], [1, 1]] as const,
            ([dx, dy]) => _Tile.make({ zoom: tile.zoom + 1, x: tile.x * 2 + dx, y: tile.y * 2 + dy }),
          ),
  }
  static readonly geometry = (feature: GeoFeature): Effect.Effect<GeoFeature.Geometry, WireFault, WkbParser> =>
    Effect.flatMap(WkbParser, (parser) => parser.parse(feature.wkb, feature.srid))
}

declare namespace GeoFeature {
  type Extent = typeof GeoFeature.Extent.Type
  type Position = typeof _Position.Type
  type Geometry =
    | typeof _Point.Type
    | typeof _MultiPoint.Type
    | typeof _LineString.Type
    | typeof _MultiLineString.Type
    | typeof _Polygon.Type
    | typeof _MultiPolygon.Type
    | { readonly _tag: "GeometryCollection"; readonly geometries: ReadonlyArray<Geometry> }
  type Srid = keyof typeof _CRS
  type Crs = (typeof _CRS)[Srid]
  type Tile = typeof _Tile.Type
}

class WkbParser extends Context.Tag("@rasm/ts/core/WkbParser")<WkbParser, {
  readonly parse: (wkb: Uint8Array, srid: number) => Effect.Effect<GeoFeature.Geometry, WireFault>
}>() {}

// The generated oneof envelope: `{ case, value }` where an UNSET oneof carries no case at all. `_caseOf` reads it as
// presence, so an unset case yields none, the message falls through unlifted, no arm matches it, and the union's own
// refusal is the answer — the producer's `<wire-*-none>` rail one runtime over.
const _caseOf = (raw: unknown): Option.Option<{ readonly case: string; readonly value: Record<string, unknown> }> =>
  Predicate.isRecord(raw) && Predicate.isString(raw.case) && Predicate.isRecord(raw.value)
    ? Option.some({ case: raw.case, value: raw.value })
    : Option.none()

// The oneof lift beside `_stamp`: a protobuf arm ships its DISCRIMINANT as the case name, so `kind` derives here and
// the landing mints nothing the producer's `.proto` never declared. `seat` is the one policy column — `hoist` spreads
// the arm's own columns beside `kind` (a message whose oneof IS its whole content), `keep` leaves the case value whole
// under its own field (a message whose arms this landing carries untyped). Encode passes through, exactly as the
// stamp does, because these rows are decode-only.
const _cased = (field: string, seat: "hoist" | "keep"): Schema.Schema<unknown, unknown> =>
  Schema.transform(Schema.Unknown, Schema.Unknown, {
    strict: true,
    decode: (raw) =>
      !Predicate.isRecord(raw) ? raw : Option.match(_caseOf(raw[field]), {
        onNone: () => raw,
        onSome: (arm) => (seat === "hoist" ? { ...arm.value, kind: arm.case } : { ...raw, [field]: arm.value, kind: arm.case }),
      }),
    encode: Function.identity,
  })

// The projected frame at `GeoReferenceWire` field 11: the authority name beside the optional EPSG code, the WKT
// definition, the projection and zone labels a legacy IFC map conversion carries, and the producer's own resolution
// token naming which of those the frame actually resolved through.
class ProjectedCrs extends Schema.Class<ProjectedCrs>("ProjectedCrs")({
  name: Schema.NonEmptyString,
  epsg: Schema.optionalWith(Schema.Int, { as: "Option" }),
  wkt: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  mapProjection: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  mapZone: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
  resolution: Schema.NonEmptyString,
}) {}

// The survey frame at `HeaderWire` field 3 — the map-conversion origin, the X-axis abscissa/ordinate pair, the three
// scale columns, and the datum tokens. A blank `verticalDatum` with no `verticalEpsg` IS the absent vertical frame,
// the producer's own reading, so no second absence spelling lands beside it.
class GeoReference extends Schema.Class<GeoReference>("GeoReference")({
  eastings: Schema.Number,
  northings: Schema.Number,
  orthogonalHeight: Schema.Number,
  xAxisAbscissa: Schema.Number,
  xAxisOrdinate: Schema.Number,
  scaleX: Schema.Number,
  scaleY: Schema.Number,
  scaleZ: Schema.Number,
  geodeticDatum: Schema.String,
  verticalDatum: Schema.String,
  crs: Schema.optionalWith(ProjectedCrs, { as: "Option" }),
  epoch: Schema.optionalWith(Schema.Number, { as: "Option" }),
  verticalEpsg: Schema.optionalWith(Schema.Int, { as: "Option" }),
}) {
  static readonly Crs: typeof ProjectedCrs = ProjectedCrs
}

// The STEP file header at `HeaderWire` field 6. Its `authors` and `organizations` rosters are the producer's own
// personal-sensitivity columns, so a scoped egress clears them to the proto3 default and they land as plain strings
// whose emptiness the crossing's `Redaction` manifest — never the message — separates from an authored blank.
class StepHeader extends Schema.Class<StepHeader>("StepHeader")({
  descriptions: Schema.Array(Schema.String),
  name: Schema.String,
  timeStamp: Schema.DateTimeUtc,
  authors: Schema.Array(Schema.String),
  organizations: Schema.Array(Schema.String),
  preprocessor: Schema.String,
  originatingSystem: Schema.String,
  schema: Schema.Array(Schema.String),
}) {}

// The crossing's header — `ElementGraphWire` field 1, `GraphDeltaWire` field 6. `tolerance` lands as the producer
// lands it, a free real under no seam gate, and it is the tolerance any address verification here grades at.
// `unitScheme` maps a quantity token to its registry unit-enum member and an EMPTY map reads as SI, so a consumer
// renders a magnitude under the producer's own scheme rather than guessing one.
class Header extends Schema.Class<Header>("Header")({
  schema: Schema.NonEmptyString,
  view: Schema.NonEmptyString,
  geoReference: GeoReference,
  tolerance: Schema.Number,
  at: Schema.DateTimeUtc,
  step: StepHeader,
  unitScheme: Schema.Record({ key: Schema.NonEmptyString, value: Schema.NonEmptyString }),
}) {
  static readonly GeoReference: typeof GeoReference = GeoReference
  static readonly Step: typeof StepHeader = StepHeader
}

// The uncertainty band at `MeasureValueWire` field 10 — the interval in SI beside the producer's own kind token, with
// the standard deviation and coverage factor a stated statistical band carries and a bare interval does not.
class MeasureBand extends Schema.Class<MeasureBand>("MeasureBand")({
  kind: Schema.NonEmptyString,
  lowerSi: Schema.Number,
  upperSi: Schema.Number,
  standardDeviationSi: Schema.optionalWith(Schema.Number, { as: "Option" }),
  coverageFactor: Schema.optionalWith(Schema.Number, { as: "Option" }),
}) {}

// The SI-coerced identity columns the producer hashes: the quantity token, the SI magnitude, and the seven base
// dimension exponents in producer order. The registry unit re-mints at the producer's own SI admission, so no
// `{ value, unit }` pair crosses and no column here carries one.
class MeasureValue extends Schema.Class<MeasureValue>("MeasureValue")({
  quantityType: Schema.NonEmptyString,
  si: Schema.Number,
  dimLength: Schema.Int,
  dimMass: Schema.Int,
  dimTime: Schema.Int,
  dimCurrent: Schema.Int,
  dimTemperature: Schema.Int,
  dimAmount: Schema.Int,
  dimLuminousIntensity: Schema.Int,
  uncertainty: Schema.optionalWith(MeasureBand, { as: "Option" }),
}) {
  static readonly Band: typeof MeasureBand = MeasureBand
}

// `MaterialUsageWire` at `AssociateWire` field 3 — the explicit three-arm family whose `none` is an ARM, so an unset
// oneof is malformed foreign input at both ends rather than an absent usage.
const _usages = Schema.Union(
  Schema.Struct({
    kind: Schema.Literal("layerSet"),
    direction: Schema.NonEmptyString,
    sense: Schema.NonEmptyString,
    offsetFromReferenceLine: Schema.optionalWith(MeasureValue, { as: "Option" }),
    referenceExtent: Schema.optionalWith(MeasureValue, { as: "Option" }),
  }),
  Schema.Struct({
    kind: Schema.Literal("profileSet"),
    cardinalPoint: Schema.optionalWith(Schema.Int, { as: "Option" }),
    referenceExtent: Schema.optionalWith(MeasureValue, { as: "Option" }),
  }),
  Schema.Struct({ kind: Schema.Literal("none") }),
)

const MaterialUsage: Schema.Schema<typeof _usages.Type, unknown> =
  Schema.compose(_cased("usage", "hoist"), _usages, { strict: false })

// The `ObjectWire` pose frame at field 12: the producer's `PlacementTransform` flattened to its nine ordered
// doubles — the location origin, the axis local-Z, the ref-direction local-X — free reals its kernel factory
// re-admits at the far end. This is the shape a reader of the `object` payload decodes a pose against.
class Placement extends Schema.Class<Placement>("Placement")({
  locationX: Schema.Number,
  locationY: Schema.Number,
  locationZ: Schema.Number,
  axisX: Schema.Number,
  axisY: Schema.Number,
  axisZ: Schema.Number,
  refDirectionX: Schema.Number,
  refDirectionY: Schema.Number,
  refDirectionZ: Schema.Number,
}) {}

// `NodeWire` crosses its id VERBATIM as the producer's X32 `NodeId` text and its payload as the eight-arm oneof —
// object, material, property set, quantity set, assessment, appearance, coverage, observation. `kind` IS that oneof's
// case, derived at the lift rather than read off a column the `.proto` never declared, and the payload rides WHOLE
// and untyped because each of those eight messages is presence on this owner that the census declares no family for,
// where a landing arm per case would mint eight shapes the closed `_families` tuple forecloses. A consumer needing
// one decodes it against the shape mirroring that payload (`AppearanceSummary` for field 7, `Placement` for the
// object payload's own field 12).
class Node extends Schema.Class<Node>("Node")({
  id: Digest.FromX32,
  kind: Schema.Literal(
    "object", "material", "propertySet", "quantitySet", "assessment", "appearance", "coverage", "observation",
  ),
  payload: Schema.Record({ key: Schema.String, value: Schema.Unknown }),
}) {
  static readonly FromWire: Schema.Schema<Node, unknown> = Schema.compose(_cased("payload", "keep"), Node, { strict: false })
}

// `RelationshipWire` is a six-arm oneof and every arm carries its OWN endpoint pair beside its own payload columns,
// so the landing is the union those arms already are: a flat source/target pair erases which endpoint role each arm
// names — a whole and its part, a subject and its definition, a host and its feature are three different relations —
// and drops the ordinal, sub-kind, usage, realizing, interface, attribute, and participant columns beside them.
// `subKind` is the arm's own token column, admitted at the producer's smart-enum gate. The generic arm's `attributes`
// map carries the recursive fourteen-case value family untyped for the reason `Node.payload` does, and its
// `relatingId`/`relatedId` are the wire spellings of the seam's source and target.
const _edges = Schema.Union(
  Schema.Struct({
    kind: Schema.Literal("compose"),
    wholeId: Digest.FromX32,
    partId: Digest.FromX32,
    subKind: Schema.NonEmptyString,
    ordinal: Schema.optionalWith(Schema.Int, { as: "Option" }),
  }),
  Schema.Struct({
    kind: Schema.Literal("assign"),
    subjectId: Digest.FromX32,
    definitionId: Digest.FromX32,
    subKind: Schema.NonEmptyString,
  }),
  Schema.Struct({
    kind: Schema.Literal("associate"),
    subjectId: Digest.FromX32,
    resourceId: Digest.FromX32,
    usage: MaterialUsage,
  }),
  Schema.Struct({
    kind: Schema.Literal("connect"),
    fromId: Digest.FromX32,
    toId: Digest.FromX32,
    subKind: Schema.NonEmptyString,
    realizingId: Schema.optionalWith(Digest.FromX32, { as: "Option" }),
    interfaceKey: Schema.optionalWith(Digest.FromBytes, { as: "Option" }),
  }),
  Schema.Struct({
    kind: Schema.Literal("void"),
    hostId: Digest.FromX32,
    featureId: Digest.FromX32,
    subKind: Schema.NonEmptyString,
  }),
  Schema.Struct({
    kind: Schema.Literal("generic"),
    wireName: Schema.NonEmptyString,
    relatingId: Digest.FromX32,
    relatedId: Digest.FromX32,
    attributes: Schema.Record({ key: Schema.String, value: Schema.Unknown }),
    participants: Schema.Array(Schema.Struct({
      nodeId: Digest.FromX32,
      role: Schema.NonEmptyString,
      ordinal: Schema.optionalWith(Schema.Int, { as: "Option" }),
    })),
  }),
)

const Relation: Schema.Schema<typeof _edges.Type, unknown> =
  Schema.compose(_cased("edge", "hoist"), _edges, { strict: false })

// The scoped-egress receipt at `ElementGraphWire` field 4 — present only where a policy cleared columns, so its
// absence IS the unredacted crossing. `unstableNodeIds` carries the producer's X32 node ids, landing on the same
// `ContentKey` brand `Node.id` wears, so the complement compares by bare `===` with no join table.
class RedactionManifest extends Schema.Class<RedactionManifest>("RedactionManifest")({
  policy: Schema.NonEmptyString,
  clearedPaths: Schema.Array(Schema.NonEmptyString),
  unstableNodeIds: Schema.Array(Digest.FromX32),
}) {}

// The snapshot a peer decodes into its own graph mirror without re-deriving an identity: the producer declares NO key
// column on this envelope — the ids and content keys inside it are the identity — so the landing carries none and a
// consumer owing a document address takes it from the transport that carried the bytes.
class ElementGraph extends Schema.Class<ElementGraph>("ElementGraph")({
  header: Header,
  nodes: Schema.Array(Node.FromWire),
  relations: Schema.Array(Relation),
  redaction: Schema.optionalWith(RedactionManifest, { as: "Option" }),
}) {
  static readonly Header: typeof Header = Header
  static readonly Measure: typeof MeasureValue = MeasureValue
  static readonly Node: typeof Node = Node
  static readonly Placement: typeof Placement = Placement
  static readonly Redaction: typeof RedactionManifest = RedactionManifest
  static readonly Relation: Schema.Schema<typeof _edges.Type, unknown> = Relation
  static readonly Usage: Schema.Schema<typeof _usages.Type, unknown> = MaterialUsage
  get byId(): HashMap.HashMap<ContentKey, Node> {
    return Array.reduce(this.nodes, HashMap.empty<ContentKey, Node>(), (acc, node) => HashMap.set(acc, node.id, node))
  }
  // the manifest's roster is the DECLARED-UNSTABLE set, so the complement is the only address a content-keyed
  // consumer verifies; an unredacted crossing declares nothing and every node stands
  get addressable(): ReadonlyArray<Node> {
    return Option.match(this.redaction, {
      onNone: () => this.nodes,
      onSome: (manifest) => Array.filter(this.nodes, (node) => !Array.contains(manifest.unstableNodeIds, node.id)),
    })
  }
}

declare namespace ElementGraph {
  type Kind = Node["kind"]
  type Relation = typeof _edges.Type
  type Usage = typeof _usages.Type
}

// The before/after pair at `GraphDeltaWire` field 3 — a revision the producer's normal form keys unique per id, so a
// consumer folds the pair off one row rather than diffing two rosters for the node it names.
class NodeRevision extends Schema.Class<NodeRevision>("NodeRevision")({
  before: Node.FromWire,
  after: Node.FromWire,
}) {}

// The `delta#GRAPH_DELTA` event body: the change record a streaming consumer folds onto the snapshot it holds. The
// header is OPTIONAL here where the snapshot's is required, because a delta re-headers the graph only where the
// producer's own reheader ran, and the five sections re-admit through the same `Node`/`Relation` gates the snapshot
// takes — one landing pair, two crossings.
class GraphDelta extends Schema.Class<GraphDelta>("GraphDelta")({
  addedNodes: Schema.Array(Node.FromWire),
  removedNodeIds: Schema.Array(Digest.FromX32),
  revisedNodes: Schema.Array(NodeRevision),
  addedEdges: Schema.Array(Relation),
  removedEdges: Schema.Array(Relation),
  header: Schema.optionalWith(Header, { as: "Option" }),
}) {
  static readonly Revision: typeof NodeRevision = NodeRevision
}

const _Segment = Schema.Struct({
  ordinal: Schema.Int.pipe(Schema.nonNegative()),
  extent: Schema.Int.pipe(Schema.positive()),
  key: Digest.FromBytes,
})

class SnapshotHeader extends Schema.Class<SnapshotHeader>("SnapshotHeader")({
  key: Digest.FromBytes,
  element: Schema.Int.pipe(Schema.nonNegative()),
  frontier: Hlc.FromBytes,
  segments: Schema.NonEmptyArray(_Segment),
  minted: Schema.DateTimeUtc,
}) {}

// `print` is the one rendered identity every gate compares on; `stamps` is the open extension bag a
// minting runtime fills with what its own probe reached, so a new host fact needs no schema edit here.
class HostFingerprint extends Schema.Class<HostFingerprint>("HostFingerprint")({
  print: Schema.NonEmptyString,
  machine: Schema.NonEmptyString,
  os: Schema.NonEmptyString,
  arch: Schema.NonEmptyString,
  processors: Schema.Int.pipe(Schema.positive()),
  runtime: Schema.NonEmptyString,
  stamps: Schema.Record({ key: Schema.NonEmptyString, value: Schema.String }),
}) {}

const _triggers = ["user-requested", "fault-transition", "health-threshold", "watchdog-timeout", "external-command", "scheduled"] as const

// One manifest entry per captured artifact: the producer's per-artifact evidence is the whole reason a dashboard
// reads this family rather than the zip it describes. `fault` is the contributor recovery arm's own row — a
// faulting producer lands a zero-byte entry naming its fault, so an absent `fault` and a zero `bytes` are
// different facts and neither is an error.
class SupportEntry extends Schema.Class<SupportEntry>("SupportEntry")({
  name: Schema.NonEmptyString,
  classification: Schema.NonEmptyString,
  bytes: Schema.Int.pipe(Schema.nonNegative()),
  truncatedBytes: Schema.Int.pipe(Schema.nonNegative()),
  redactions: Schema.Int.pipe(Schema.nonNegative()),
  // The archive identity every consumer keys on — 32 lowercase hex digits of the producer's seed-zero digest over
  // the bytes THIS entry wrote, POST-redaction and POST-cap. It crosses as text because a UInt128 exceeds this
  // runtime's exact-integer range, and it is omitted exactly where the entry wrote no bytes (a faulted
  // contributor, a refused cleanup, a bundle-cap drop), so presence and a written payload stay one fact. The
  // AppUi `BundleMember.ContentKey` is the PRE-redaction identity of the same payload, so the two agree only
  // where nothing was masked or truncated and an inequality names redaction or a cap, never corruption.
  contentKey: Schema.optional(Schema.String.pipe(Schema.pattern(/^[0-9a-f]{32}$/))),
  fault: Schema.optional(Schema.NonEmptyString),
}) {}

// The producer's FLATTENED export projection, never its receipt union: a coalesced or evicted receipt names no
// bundle, so a decoder branching on a kind discriminant to find three quarters of its fields absent is exactly
// the shape the producer flattened away. This is the AppHost bundle leaving the host toward a dashboard — the
// opposite direction from `invoke`'s `SupportCapture`, which is a report arriving at this branch's gateway.
class SupportExport extends Schema.Class<SupportExport>("SupportExport")({
  trigger: Schema.Literal(..._triggers),
  reason: Schema.NonEmptyString,
  correlation: Schema.NonEmptyString,
  windowStart: Schema.DateTimeUtc,
  windowEnd: Schema.DateTimeUtc,
  bundlePath: Schema.NonEmptyString,
  totalBytes: Schema.Int.pipe(Schema.nonNegative()),
  // The producer crosses this as NodaTime round-trip TEXT and no effect Duration codec reads that dialect —
  // `DurationFromMillis` wants a number and `Duration` wants the encoded object or a `[seconds, nanos]` pair — so
  // the landing carries the text the producer actually writes and a consumer needing arithmetic parses at its own
  // seam. Binding a Duration schema here would refuse every real payload while the census read correct.
  elapsed: Schema.NonEmptyString,
  redactions: Schema.Int.pipe(Schema.nonNegative()),
  entries: Schema.Array(SupportEntry),
}) {
  static readonly Entry: typeof SupportEntry = SupportEntry
}

// Benchmark measures are physical quantities — nanoseconds, bytes, hardware counts: a negative or
// non-finite value is corrupt evidence, refused at the codec boundary before any claim gate reads it.
const _Measure = Schema.Number.pipe(Schema.finite(), Schema.nonNegative())
const _Aggregate = Schema.Struct({
  avg: _Measure,
  min: _Measure,
  max: _Measure,
  total: _Measure,
})
const _Counters = Schema.Struct({
  cycles: _Measure,
  instructions: _Measure,
  cache: _Measure,
  cacheMisses: _Measure,
  branchMisses: _Measure,
})
const _RUNGS = ["min", "max", "avg", "p25", "p50", "p75", "p95", "p99", "p999", "stdDev"] as const
// One rung vocabulary spans every minting harness: mitata computes the whole sampling ladder beside its
// raw vector, a C#-side equivalence sweep persists p50, p95, and stdDev alone, so the map carries what
// its own harness measured and a grader naming an absent rung refuses at its own axis. A required rung
// would force one harness to fabricate the other's statistic; a fixed struct would strand both halves.
const _Rungs = Schema.Record({ key: Schema.Literal(..._RUNGS), value: _Measure }).pipe(
  Schema.partialWith({ exact: true }),
  // Bands ASSERT a measurement, so each reports at least one rung off the roster: an empty map computes nothing
  // while the sample count claims a run, and an empty evidence fold then grades as a passing benchmark claim. WHICH rungs
  // stay the harness's own — the floor is one, never a named one, so no harness fabricates the other's statistic.
  Schema.filter((rungs) => Array.some(_RUNGS, (rung) => rungs[rung] !== undefined) || "<rungless-band>", { identifier: "MeasuredRungs" }),
)
const _Band = Schema.Struct({
  // Zero samples is not a measurement: every rung beside it is a statistic over nothing, so the floor is structural
  // here exactly as `_Measure` refuses a negative quantity, and no grade arm re-decides it.
  sampleCount: Schema.Int.pipe(Schema.positive()),
  rungs: _Rungs,
  ticks: Schema.optionalWith(Schema.Int.pipe(Schema.nonNegative()), { as: "Option" }),
  samples: Schema.optionalWith(Schema.Array(_Measure), { as: "Option" }),
  gc: Schema.optionalWith(_Aggregate, { as: "Option" }),
  heap: Schema.optionalWith(_Aggregate, { as: "Option" }),
  counters: Schema.optionalWith(_Counters, { as: "Option" }),
})

// 64-bit columns land `BigIntFromSelf`: the proto engine already carries them as `bigint` under `protoInt64`, so a
// string-encoded `Schema.BigInt` here would refuse every valid document at the transform's own input type.
const _Input = Schema.Struct({
  payloadBytes: Schema.BigIntFromSelf,
  band: Schema.Literal("micro", "small", "medium", "large"),
  dtype: Schema.NonEmptyString,
  shape: Schema.Array(Schema.BigIntFromSelf),
  strides: Schema.Array(Schema.BigIntFromSelf),
  batch: Schema.Int.pipe(Schema.positive()),
  density: Schema.Number.pipe(Schema.between(0, 1)),
  rank: Schema.Int.pipe(Schema.nonNegative()),
  contiguous: Schema.Boolean,
})

// Subjects discriminate what a row measured: a bare probe carries the shared label/unit/modality
// triple alone, while a kernel run carries the selection coordinate a route, provider, or encoding
// decision resolves on. Widening every kernel column to optional on one flat row admits a probe row
// claiming a substrate it never ran, which is the exact mismatch the tag forecloses at decode.
const _Subject = Schema.Union(
  Schema.Struct({ subject: Schema.Literal("probe") }),
  Schema.Struct({
    subject: Schema.Literal("kernel"),
    input: _Input,
    substrate: Schema.NonEmptyString,
    family: Schema.NonEmptyString,
    case: Schema.NonEmptyString,
    route: Schema.NonEmptyString,
    provider: Schema.NonEmptyString,
    corpus: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    artifactKey: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }),
    equivalenceMaxDeviation: Schema.Number.pipe(Schema.finite(), Schema.nonNegative()),
    toleranceClass: Schema.NonEmptyString,
  }),
)

class Claim extends Schema.Class<Claim>("Claim")({
  suite: Schema.NonEmptyString,
  metrics: Schema.NonEmptyArray(Schema.Struct({
    label: Schema.NonEmptyString,
    // Each minting harness spells its own vocabulary, never the instrument census's — a render probe spells `1/s` and a
    // bare count, neither carried by the telemetry unit roster — so the grade compares this column verbatim
    unit: Schema.NonEmptyString,
    kind: Schema.Literal("fn", "iter", "yield"),
    subject: _Subject,
    band: _Band,
    warmups: Schema.optionalWith(Schema.Int.pipe(Schema.nonNegative()), { as: "Option" }),
    allocatedBytes: Schema.optionalWith(Schema.BigIntFromSelf, { as: "Option" }),
    operations: Schema.optionalWith(Schema.BigIntFromSelf, { as: "Option" }),
  })),
  host: HostFingerprint,
  minted: Schema.DateTimeUtc,
}) {
  static readonly RUNGS: typeof _RUNGS = _RUNGS
  static readonly Band: typeof _Band = _Band
  static readonly Subject: typeof _Subject = _Subject
  static readonly Host: typeof HostFingerprint = HostFingerprint
  static readonly admit = (claim: Claim, identity: AppIdentity): Effect.Effect<Claim, WireFault> =>
    claim.host.print === identity.host
      ? Effect.succeed(claim)
      : Effect.fail(_mismatch("BenchmarkClaimWire", claim.host.print, identity.host, "<foreign-host-claim>"))
}

const _labels = ["CERTIFICATE", "PUBLIC KEY", "PKCS7", "PRIVATE KEY", "EC PRIVATE KEY", "RSA PRIVATE KEY"] as const

// The producer's own RFC-7468 vocabulary with its own `secret` column: the mint refuses to cross a block whose
// label carries it, so a `sealed` landing is broken-producer evidence rather than a decode this end must handle.
const _PEM = {
  "CERTIFICATE": { secret: false },
  "PUBLIC KEY": { secret: false },
  "PKCS7": { secret: false },
  "PRIVATE KEY": { secret: true },
  "EC PRIVATE KEY": { secret: true },
  "RSA PRIVATE KEY": { secret: true },
} as const satisfies Record<(typeof _labels)[number], { readonly secret: boolean }>

class Credential extends Schema.Class<Credential>("Credential")({
  fingerprint: Schema.propertySignature(Schema.NonEmptyString).pipe(Schema.fromKey("keyId")),
  labels: Schema.NonEmptyArray(Schema.Literal(..._labels)),
  chain: Schema.NonEmptyString,
  blockDigests: Schema.NonEmptyArray(Schema.NonEmptyString),
  bundleDigest: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
}) {
  static readonly Label: Schema.Literal<typeof _labels> = Schema.Literal(..._labels)
  get sealed(): boolean {
    return Array.some(this.labels, (label) => _PEM[label].secret)
  }
  static readonly rotated = (live: Credential, next: Credential): boolean => live.bundleDigest !== next.bundleDigest
}

declare namespace Credential {
  type Label = (typeof _labels)[number]
}
```

## [07]-[KEYED_REGISTRY]

[KEYED_REGISTRY]:
- Owner: `Wire`, the assembled registry — `_landingRows`, the ONE value anchor mapping every codec-homed family to its landing schema, from which `_Landing` derives by `Schema.Schema.Type` projection and `_landings` re-binds under the derived mapped annotation so the generic indexed message decode resolves one correlated signature per key; the `_schemas` byte-row table annotated by the same mapped contract; and the polymorphic entrypoints: `decode(family, octets)`, `encode(family, value)` for the egress-legal rows, `schema(family)` the raw byte schema for field composition, `stream(family, frames)` the framed feed with quarantine divert, `diverted` the one framed-divert combinator, `residue(message)` the preserved unknown-field read, `by(column, value)` the one census projection over any fact column, `verifiedSnapshot` and the `admittedGraph`/`admittedDelta` gated pair, and the census facts and the wire literal spread onto the owner.
- Law: one keyed decode, spelled once — the landing correspondence is a value anchor and its type derives, so a hand-written landing type cannot drift from the table, the `_schemas` annotation ties the byte rows to the same anchor, and the per-page `_Landing`/`_rows` restatement this collapse killed is unspellable because a family's landing exists in exactly one table; the `_Landed`/`_LandingKeys` guard pair closes the landing table against the census `home` column in both directions — a codec-homed census row missing its landing line, or a landing line for a family homed elsewhere, fails at the declaration, never at the gate's runtime coverage walk.
- Law: rows landing sibling vocabulary compose the sibling owner whole (`Proto.family(Proto.suite.QuantityWire, Quantity)`); rows landing wire-owned shapes compose `[06]`'s classes; the byte row is the census `arm`'s engine and never a writer's habit — proto rows ride `Proto.family`, json rows `Json.schema`, msgpack rows `Pack.schema`, the cbor row `Cbor.frame` composed with its header class, and the jsonpatch row delegates to `format#JSONPATCH_ENGINE`'s document schema under home `format`.
- Law: `diverted` is the one framed-divert spelling — source fault to `malformed`, landing decode, quarantine divert, in one combinator over `(family, source, landing, octets)` — and every framed ingress instantiates it: `stream` over the ARM's own walk (proto size-delimited frames, json newline-delimited documents) with the landing decode of the already-parsed value, so the byte schema never re-parses a frame; the oplog stream over the msgpack walk; the frame page's envelope streams. Each arm instantiates the combinator at its own raw shape rather than joining the two through an erased walk, because an `unknown`-typed emit beside an `unknown`-typed walk lets a proto re-frame run over a json value with nothing raising; a hand pipeline beside it re-derives the walk.
- Law: content-verified rows compose `Parity` at the entry — `verifiedSnapshot` re-proves the header key over the held octets, and `admittedGraph`/`admittedDelta` yield the contract gate before decoding under the drift verdict, one entry per element crossing since the two carry different landings and a shared entry would erase which; verification is entry composition, never a per-row re-implementation.
- Law: `residue` reads the `$unknown` rows the `_READ` posture preserved on any decoded proto message — live-message drift a partial peer emits, the runtime complement of the contract gate's boot descriptor grade; a consumer surfaces a non-empty residue beside the family's drift verdict as evidence, the rows never mutate the landing, and re-emission through the same suite row round-trips them under `writeUnknownFields`.
- Law: quarantine thunks hold the whole-document byte form — `Wire.decode`'s own replay coordinate — so each arm's emit and the replay drain agree by construction; `Proto.delimit` re-frames octets only where an egress joins a size-delimited transport, never on the replay path.
- Law: the census reads through ONE column-parameterized projection — `by("arm", …)`, `by("home", …)`, `by("consumer", …)`, `by("source", …)` are one member over the fact table's own key space, so every column a row declares is a column a caller can read and a new fact column is readable the moment it lands. A member per column mints one surface per axis and leaves the columns no member spelled — `consumer` and `source` are exactly the rows the branch and folder seam registries join on, and a census column no read reaches governs nothing.
- Growth: a new family is one `_Landing` line and one `_schemas` row beside its census row; a new census FACT is one column on `Row`, readable through the same projection with no member added.
- Boundary: the contract gate service this registry's gated rows require is the contract page's; frame reassembly and the invoke verbs consume `schema`/`stream` and land their own shapes at their homes.
- Packages: `effect` (`Schema`, `Effect`, `Stream`, `Either`, `Option`, `Array`); `@bufbuild/protobuf` (`Message`, `UnknownField`); `./format.ts` (`Cbor`, `Json`, `Pack`, `Proto`).

```typescript signature
import { Stream } from "effect"

class OpLogEntry extends Schema.Class<OpLogEntry>("OpLogEntry")({
  seq: Schema.BigIntFromSelf,
  op: CrdtOp,
}) {}

const _landingRows = {
  ReceiptEnvelopeWire: ReceiptEnvelope,
  HlcStampWire: Hlc,
  TenantContextWire: TenantContext,
  CommandAvailabilityWire: Availability,
  FaultDetailWire: FaultDetail.FromWire,
  QuantityWire: Quantity,
  ElementGraphWire: ElementGraph,
  GraphDeltaWire: GraphDelta,
  NodeWire: Node.FromWire,
  RelationshipWire: Relation,
  OpLogWire: OpLogEntry,
  SnapshotHeader: SnapshotHeader,
  CrdtOpWire: CrdtOp,
  CommitWire: Commit,
  BranchWire: Commit.Branch,
  VersionVectorWire: Vector,
  MerkleSummaryWire: Commit.Merkle,
  ProgressMarkWire: ProgressMark,
  CredentialPemWire: Credential,
  BenchmarkClaimWire: Claim,
  HostFingerprintWire: HostFingerprint,
  SupportCaptureWire: SupportExport,
  BindingStatusWire: BindingStatus.FromWire,
  CoercedValueWire: CoercedValue.FromWire,
  WriteReceiptWire: WriteReceipt.FromWire,
  CommandGateWire: CommandGate.FromWire,
  FlagVerdictWire: FlagVerdict,
  ControlIntentWire: ControlIntent,
  LayoutConstraintWire: LayoutProgram,
  EvidenceTimelineWire: EvidenceTimeline,
  BcfTopicWire: BcfTopic,
  BcfViewpointWire: BcfViewpoint,
  GeoFeatureWire: GeoFeature,
  BimWire: BimModel,
  DiffWire: BimDiff,
  IdsAuditWire: IdsAudit,
  PredicateWire: PredicateWire,
  MaterialWire: Material,
  OpenPbrGroupsWire: PbrGroups,
  TextureSetWire: TextureSet,
  AssetSetManifest: AssetSetManifest,
} as const

type _LandingRows = typeof _landingRows
type _Landing = { readonly [K in keyof _LandingRows]: Schema.Schema.Type<_LandingRows[K]> }

const _landings: { readonly [K in keyof _LandingRows]: Schema.Schema<_Landing[K], Schema.Schema.Encoded<_LandingRows[K]>> } = _landingRows

const _schemas: { readonly [K in keyof _Landing]: Schema.Schema<_Landing[K], Uint8Array> } = {
  // every `json`-armed row composes `Json.schema` — the AppHost mint is source-generated System.Text.Json, so a proto
  // suite row here would decode bytes the producer never writes while the census read correct
  ReceiptEnvelopeWire: Json.schema(ReceiptEnvelope),
  HlcStampWire: Json.schema(Hlc),
  TenantContextWire: Json.schema(TenantContext),
  CommandAvailabilityWire: Json.schema(Availability),
  FaultDetailWire: Proto.family(Proto.suite.FaultDetailWire, FaultDetail.FromWire),
  QuantityWire: Proto.family(Proto.suite.QuantityWire, Quantity),
  // the element rows ride the oneof-lifting twins: the byte schema hands the generated message to the landing, and
  // `_cased` derives the arm's `kind` off its `{ case, value }` envelope before the union or the class sees it
  ElementGraphWire: Proto.family(Proto.suite.ElementGraphWire, ElementGraph),
  GraphDeltaWire: Proto.family(Proto.suite.GraphDeltaWire, GraphDelta),
  NodeWire: Proto.family(Proto.suite.NodeWire, Node.FromWire),
  RelationshipWire: Proto.family(Proto.suite.RelationshipWire, Relation),
  OpLogWire: Pack.schema(OpLogEntry),
  // the cbor arm carries a real inverse, so this row takes the golden-byte roundtrip proof like every other
  SnapshotHeader: Cbor.frame.pipe(Schema.compose(SnapshotHeader, { strict: false })),
  CrdtOpWire: Pack.schema(CrdtOp),
  CommitWire: Pack.schema(Commit),
  BranchWire: Pack.schema(Commit.Branch),
  VersionVectorWire: Pack.schema(Vector),
  MerkleSummaryWire: Pack.schema(Commit.Merkle),
  ProgressMarkWire: Proto.family(Proto.suite.ProgressMarkWire, ProgressMark),
  CredentialPemWire: Json.schema(Credential),
  BenchmarkClaimWire: Proto.family(Proto.suite.BenchmarkClaimWire, Claim), // Compute-minted per MANIFEST [02.14]; its arm is its minter's
  HostFingerprintWire: Json.schema(HostFingerprint),
  SupportCaptureWire: Json.schema(SupportExport),
  BindingStatusWire: Json.schema(BindingStatus.FromWire),
  CoercedValueWire: Json.schema(CoercedValue.FromWire),
  WriteReceiptWire: Json.schema(WriteReceipt.FromWire),
  // the AppUi product-shell rows ride `Json.schema` for the same reason the AppHost rows above do — the
  // producer's mint is source-generated System.Text.Json and no `.proto` declares these messages (see the census)
  CommandGateWire: Json.schema(CommandGate.FromWire),
  FlagVerdictWire: Json.schema(FlagVerdict),
  ControlIntentWire: Json.schema(ControlIntent),
  LayoutConstraintWire: Json.schema(LayoutProgram),
  EvidenceTimelineWire: Json.schema(EvidenceTimeline),
  BcfTopicWire: Proto.family(Proto.suite.BcfTopicWire, BcfTopic),
  BcfViewpointWire: Proto.family(Proto.suite.BcfViewpointWire, BcfViewpoint),
  GeoFeatureWire: Proto.family(Proto.suite.GeoFeatureWire, GeoFeature),
  BimWire: Proto.family(Proto.suite.BimWire, BimModel),
  DiffWire: Proto.family(Proto.suite.DiffWire, BimDiff),
  IdsAuditWire: Proto.family(Proto.suite.IdsAuditWire, IdsAudit),
  // this row rides `Json.schema` for the same reason the shell rows above do — its producer's mint is the
  // `[JsonPolymorphic]` record family and no `.proto` declares it (see the census)
  PredicateWire: Json.schema(PredicateWire),
  // the appearance families decode the producer's MessagePack integer-keyed roster — the same `Pack` arm the
  // Persistence `[Key(n)]` families ride; no proto suite row exists for them (see the census comment). The
  // seam summary holds no row: it crosses inside the `NodeWire` proto landing, never as its own document.
  MaterialWire: Pack.schema(Material.FromWire),
  OpenPbrGroupsWire: Pack.schema(PbrGroups.FromVector),
  TextureSetWire: Proto.family(Proto.suite.TextureSetWire, TextureSet),
  AssetSetManifest: Proto.family(Proto.suite.AssetSetManifest, AssetSetManifest),
}

declare namespace Wire {
  type Arm = (typeof _arms)[number]
  type Families = typeof _families
  type Family = keyof typeof _census
  type Home = "codec" | "contract" | "format" | "frame" | "invoke"
  type Consumer = "value" | "state" | "observe" | "interchange" | "security" | "data" | "runtime" | "ui"
  type Homed = keyof _Landing
  type Decoded<K extends Homed> = _Landing[K]
  type Row = { readonly arm: Arm; readonly source: string; readonly consumer: Consumer; readonly home: Home }
  type Framed = { readonly [K in Homed]: (typeof _census)[K]["arm"] extends "proto" | "json" ? K : never }[Homed]
  type Shape = Types.Simplify<typeof _census & {
    readonly arms: typeof _arms
    readonly families: Families
    readonly wire: Schema.Literal<Families>
    readonly by: <C extends keyof Row>(column: C, value: Row[C]) => ReadonlyArray<Family>
    readonly schema: <K extends Homed>(family: K) => Schema.Schema<Decoded<K>, Uint8Array>
    readonly decode: <K extends Homed>(family: K, octets: Uint8Array) => Effect.Effect<Decoded<K>, ParseResult.ParseError>
    readonly encode: <K extends Homed>(family: K, value: Decoded<K>) => Effect.Effect<Uint8Array, ParseResult.ParseError>
    readonly stream: <K extends Framed>(
      family: K,
      frames: AsyncIterable<Uint8Array>,
    ) => Stream.Stream<Either.Either<Decoded<K>, WireFault>, WireFault, Quarantine>
    readonly diverted: <Raw, A>(
      family: Family,
      source: Stream.Stream<Raw, unknown>,
      landing: (raw: Raw) => Effect.Effect<A, ParseResult.ParseError>,
      octets: (raw: Raw) => Uint8Array,
    ) => Stream.Stream<Either.Either<A, WireFault>, WireFault, Quarantine>
    readonly residue: (message: Message) => ReadonlyArray<UnknownField>
    readonly verifiedSnapshot: (octets: Uint8Array) => Effect.Effect<SnapshotHeader, ParseResult.ParseError | WireFault>
    readonly admittedGraph: (
      gate: Effect.Effect<void, WireFault>,
      octets: Uint8Array,
    ) => Effect.Effect<ElementGraph, ParseResult.ParseError | WireFault>
    readonly admittedDelta: (
      gate: Effect.Effect<void, WireFault>,
      octets: Uint8Array,
    ) => Effect.Effect<GraphDelta, ParseResult.ParseError | WireFault>
  }>
  type _Rows<T extends Record<Families[number], Row> = typeof _census> = T
  type _Keys<K extends Families[number] = Family> = K
  type _CodecHomed = { readonly [K in Family]: (typeof _census)[K]["home"] extends "codec" ? K : never }[Family]
  type _Landed<K extends Homed = _CodecHomed> = K
  type _LandingKeys<K extends _CodecHomed = Homed> = K
}

const _diverted = <Raw, A>(
  family: Wire.Family,
  source: Stream.Stream<Raw, unknown>,
  landing: (raw: Raw) => Effect.Effect<A, ParseResult.ParseError>,
  octets: (raw: Raw) => Uint8Array,
): Stream.Stream<Either.Either<A, WireFault>, WireFault, Quarantine> =>
  source.pipe(
    Stream.mapError((defect) =>
      new WireFault({ family, reason: "malformed", detail: String(defect), evidence: Option.none() })),
    Stream.mapEffect(
      (raw) =>
        landing(raw).pipe(
          Effect.mapError((issue) =>
            new WireFault({ family, reason: "malformed", detail: issue.message, evidence: Option.none() })),
          Quarantine.divert({ family, octets: () => octets(raw) }),
        ),
      { concurrency: 1 },
    ),
  )

// The arm owns the frame walk and its replay emit: a proto family walks size-delimited frames and re-frames its parsed
// message, a json family walks newline-delimited documents and re-emits the parsed value. The census `arm` DECIDES and
// the suite roster only NARROWS the key — a json family may still hold a generated descriptor its own `-bin` carriage
// mints, so probing the roster alone would run the protobuf walk over JSON octets. Each arm instantiates `_diverted` at
// its OWN raw shape, so the walk and the quarantine thunk stay typed as a pair with no erased emit between them.
const _framedStream = <K extends Wire.Framed>(
  family: K,
  frames: AsyncIterable<Uint8Array>,
): Stream.Stream<Either.Either<Wire.Decoded<K>, WireFault>, WireFault, Quarantine> =>
  Option.match(
    Option.filter(Array.findFirst(Proto.names, (name) => name === family), () => _census[family].arm === "proto"),
    {
      onNone: () => _diverted(family, Json.stream(frames), Schema.decodeUnknown(_landings[family]), Json.encode),
      onSome: (name) =>
        _diverted(
          family,
          Proto.stream(Proto.suite[name])(frames),
          Schema.decodeUnknown(_landings[family]),
          Schema.encodeSync(Proto.frame(Proto.suite[name])),
        ),
    },
  )

const Wire: Wire.Shape = {
  ..._census,
  arms: _arms,
  families: _families,
  wire: _wireLiteral,
  by: (column, value) => Array.filter(_families, (family) => _census[family][column] === value),
  schema: (family) => _schemas[family],
  decode: (family, octets) => Schema.decodeUnknown(_schemas[family])(octets),
  encode: (family, value) => Schema.encode(_schemas[family])(value),
  stream: _framedStream,
  diverted: _diverted,
  residue: (message) => message.$unknown ?? [],
  verifiedSnapshot: (octets) =>
    Effect.tap(
      Schema.decodeUnknown(_schemas.SnapshotHeader)(octets),
      (header) => Parity.verified("SnapshotHeader", header.key, octets),
    ),
  admittedGraph: (gate, octets) => Effect.andThen(gate, Schema.decodeUnknown(_schemas.ElementGraphWire)(octets)),
  admittedDelta: (gate, octets) => Effect.andThen(gate, Schema.decodeUnknown(_schemas.GraphDeltaWire)(octets)),
}
```

## [08]-[FEED_DEDUP]

[FEED_DEDUP]:
- Owner: `feed`, the one transition-feed entry, and its policy rows — `_feeds` carries one row per feed family: the keying `subject` projection, the transition `alike` equivalence, and the optional `flow` policy carrying both its coalescing window and its token bucket; the combinator composes `Wire.stream`'s framed divert with one keyed transition Mealy — per-subject last-value state, an arrival equivalent to its subject's incumbent drops, a transition emits and replaces — then coalesces each declared window to its current value and shapes the survivors. Merging `feed` carries the row vocabulary on the entry's own name.
- Law: the pipeline is spelled once — framed decode, poison divert, keyed dedup, throttle; the per-family variation is three row columns, so the progress and flag feeds that restated this pipeline as sibling pages are two rows here and a third feed is one row.
- Law: dedup is keyed, never global — the Mealy state maps subject to last emission, so interleaved subjects cannot mask each other's transitions; the state is fold-interior and single-fiber by construction, the ruled form over `Stream.groupByKey`, whose per-subject fiber fan-out re-merges without cross-subject order and buys nothing a last-value map needs. The subject projection binds once per element and the fold pays one probe plus one write on a transition: `HashMap.modifyAt` cannot serve here because a Mealy step owes an EMISSION beside its state and the fused write returns the map alone.
- Law: `alike` derives, never restates — whole-schema equality for `FlagVerdict`, and `ProgressMark.transition` from the evidence owner for progress (the operation, parent, stage, done, and total transition axes with stamp and tenant transport noise excluded); a parent rebind therefore survives dedup, and a hand-written projection beside either Schema owner is a second unverified equality truth.
- Law: throttle is a declared token bucket — `cost` prices a whole chunk, `"shape"` delays and never drops; a feed without a `flow` row passes unshapen.
- Law: a flow row declaring a `window` COALESCES before it shapes — `Stream.aggregateWithin` over a last-value sink keeps the CURRENT arrival per window and the token bucket then prices the survivors, so a genuinely-advancing burst reaches a reader as its latest coordinate. Dedup alone drops equal neighbours and the bucket then delays what advanced, which turns a burst into a slow tail of values already superseded; the window is the grain a reader wants, sits at the bucket's own token period, and a row declaring none keeps the bare-throttle shape.
- Growth: a new feed family is one `_feeds` row; a new flow axis is one field on the row's `flow` record.
- Boundary: what a consumer folds the deduped feed into — the runtime flag cell, the state progress table — is the consumer's plan; this combinator owns only the wire-to-transition geometry.
- Packages: `effect` (`Stream`, `Sink`, `Schedule`, `Schema`, `Equivalence`, `HashMap`, `Chunk`, `Either`, `Duration`, `Option`).

```typescript signature
import { Sink } from "effect"
import type { Duration } from "effect"

const _feedKeys = ["ProgressMarkWire", "FlagVerdictWire"] as const

declare namespace feed {
  type Family = (typeof _feedKeys)[number]
  type Flow = {
    readonly units: number
    readonly per: Duration.DurationInput
    readonly burst: number
    readonly window: Option.Option<Duration.DurationInput>
  }
  type Row<A> = {
    readonly subject: (value: A) => string
    readonly alike: Equivalence.Equivalence<A>
    readonly flow: Option.Option<Flow>
  }
}

const _feeds: { readonly [K in feed.Family]: feed.Row<Wire.Decoded<K>> } = {
  ProgressMarkWire: {
    subject: (mark) => mark.operation,
    alike: ProgressMark.transition,
    // the coalescing grain sits at the bucket's own token period, so a burst collapses to its CURRENT mark inside each
    // window and the bucket prices the residue instead of delaying a tail of superseded ones
    flow: Option.some({ units: 240, per: "1 second", burst: 60, window: Option.some("4 millis") }),
  },
  FlagVerdictWire: {
    subject: (verdict) => verdict.flag,
    alike: Schema.equivalence(FlagVerdict),
    flow: Option.none(),
  },
}

const _transitions = <A>(row: feed.Row<A>) => <E, R>(marks: Stream.Stream<A, E, R>): Stream.Stream<A, E, R> =>
  marks.pipe(
    // the subject projection binds ONCE per element: a re-read per arm charges the keying fold three projections on
    // the feed's own hot path, where the declared cadence is hundreds of marks a second
    Stream.mapAccum(HashMap.empty<string, A>(), (seen, value) =>
      pipe(row.subject(value), (subject) =>
        Option.match(HashMap.get(seen, subject), {
          onNone: () => [HashMap.set(seen, subject, value), Option.some(value)] as const,
          onSome: (prior) =>
            row.alike(prior, value)
              ? ([seen, Option.none<A>()] as const)
              : ([HashMap.set(seen, subject, value), Option.some(value)] as const),
        }))),
    Stream.filterMap((held) => held),
  )

const feed = <K extends feed.Family>(
  family: K,
  frames: AsyncIterable<Uint8Array>,
): Stream.Stream<Wire.Decoded<K>, WireFault, Quarantine> => {
  const row = _feeds[family]
  return Wire.stream(family, frames).pipe(
    Stream.filterMap(Either.getRight),
    _transitions(row),
    (deduped) =>
      Option.match(row.flow, {
        onNone: () => deduped,
        onSome: (flow) =>
          Stream.throttle(
            Option.match(flow.window, {
              onNone: () => deduped,
              onSome: (window) =>
                Stream.aggregateWithin(deduped, Sink.last<Wire.Decoded<K>>(), Schedule.spaced(window)).pipe(
                  Stream.filterMap((held) => held), // an empty window closes with `None`, so a quiet grain emits nothing
                ),
            }),
            { cost: Chunk.size, units: flow.units, duration: flow.per, burst: flow.burst, strategy: "shape" },
          ),
      }),
  )
}
```

## [09]-[SEQUENCE_GAP]

[SEQUENCE_GAP]:
- Owner: `Gap`, the sequence-evidence vocabulary — `evidence(family, expected, actual, detail?)` the one sequence-fault mint (`<gap>` unless the chain names its own violation), and `sequential(family, resume)` the bigint watermark Mealy generic over any `seq`-carrying entry: entries at or below the running watermark — the seeded resume and every advance — drop inside the fold as replays, so a late out-of-order duplicate can neither re-anchor the watermark nor double-mint evidence; a successor exactly one past the watermark advances it, and a jump emits `sequence` evidence ahead of the jumped entry — both coordinates on the evidence, the entry still delivered — while the watermark re-anchors so one gap reports once and no arriving entry is lost; `OpLog` rides it — the resumable CRDT journal stream over the msgpack arm with the `frontier` read.
- Law: the Mealy is the shared sequence law — the oplog watermark and the frame page's ordinal chain mint through the same `evidence` spelling, so sequence forensics read one shape branch-wide; `sequence` faults never quarantine because a gap has no frame to hold.
- Law: resume is the source's coordinate — the caller passes the last durably applied `seq`, so reconnect replays drop structurally and no downstream dedup set exists.
- Law: the frontier is the durable handoff — `Array.max` over the seq order on a non-empty batch, `Option.none` on empty, the value the data wave's journal persists as its resume coordinate.
- Growth: a per-family gap posture (a tolerated reorder window) is one parameter on `sequential`; a second sequential family composes the same Mealy.
- Boundary: what the delivered entries fold into is `state`'s plan altitude; durable journal positions are the data wave's.
- Packages: `effect` (`Stream`, `Order`, `Array`, `Chunk`, `Either`, `Option`).

```typescript signature
const _bySeq: Order.Order<OpLogEntry> = Order.mapInput(Order.bigint, (entry: OpLogEntry) => entry.seq)

const Gap: {
  readonly evidence: (family: Wire.Family, expected: bigint, actual: bigint, detail?: string) => WireFault
  readonly sequential: (
    family: Wire.Family,
    resume: bigint,
  ) => <A extends { readonly seq: bigint }, E, R>(
    entries: Stream.Stream<Either.Either<A, WireFault>, E, R>,
  ) => Stream.Stream<Either.Either<A, WireFault>, E, R>
} = {
  evidence: (family, expected, actual, detail = "<gap>") =>
    new WireFault({ family, reason: "sequence", detail, evidence: Option.some({ actual, expected }) }),
  sequential: (family, resume) => <A extends { readonly seq: bigint }, E, R>(entries: Stream.Stream<Either.Either<A, WireFault>, E, R>) =>
    entries.pipe(
      Stream.mapAccum(resume, (last, lane): readonly [bigint, Chunk.Chunk<Either.Either<A, WireFault>>] =>
        Either.match(lane, {
          onLeft: (): readonly [bigint, Chunk.Chunk<Either.Either<A, WireFault>>] => [last, Chunk.of(lane)],
          onRight: (entry) =>
            entry.seq <= last
              ? ([last, Chunk.empty<Either.Either<A, WireFault>>()] as const)
              : entry.seq === last + 1n
                ? ([entry.seq, Chunk.of(lane)] as const)
                : ([entry.seq, Chunk.make(Either.left(Gap.evidence(family, last + 1n, entry.seq)), lane)] as const),
        })),
      Stream.flattenChunks,
    ),
}

const OpLog: {
  readonly Entry: typeof OpLogEntry
  readonly stream: (
    frames: ReadableStream<Uint8Array> | AsyncIterable<Uint8Array>,
    resume: bigint,
  ) => Stream.Stream<Either.Either<OpLogEntry, WireFault>, WireFault, Quarantine>
  readonly frontier: (entries: ReadonlyArray<OpLogEntry>) => Option.Option<bigint>
} = {
  Entry: OpLogEntry,
  stream: (frames, resume) =>
    _diverted("OpLogWire", Pack.stream(frames), Schema.decodeUnknown(OpLogEntry), Pack.encode).pipe(
      Gap.sequential("OpLogWire", resume),
    ),
  frontier: (entries) =>
    Array.isNonEmptyReadonlyArray(entries) ? Option.some(Array.max(entries, _bySeq).seq) : Option.none(),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export {
  AppearanceSummary, AssetSetManifest, BcfTopic, BcfViewpoint, BimDiff, BimModel, BindingStatus, Claim,
  CoercedValue, CommandGate, ControlIntent, Credential, CrdtOp, ElementGraph, EvidenceTimeline, FaultDetail, feed,
  FlagVerdict, Gap, GeoFeature, GraphDelta, Hops, IdsAudit, LayoutProgram, Material, OpLog, Parity, PbrGroups, PredicateWire,
  Quarantine, RenderReceipt,
  SnapshotHeader, SupportExport, Texture, TextureSet, Wire, WireFault, WkbParser, WriteReceipt,
}
```

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
