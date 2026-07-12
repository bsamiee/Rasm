# [CORE_CODEC]

The keyed-decode engine of the interchange plane: ONE closed census of every C#-minted wire family — arm, mint source, consuming surface, and owning page as data columns — and ONE polymorphic registry whose mapped landing table resolves each codec-homed family to its decoded type, so `Wire.decode(family, octets)` is the single decode entry the branch owns and a per-family codec page is unspellable. Families landing in core vocabulary decode INTO the `value`/`state` owners with zero local twins; families whose consumers live in later waves land wire-owned shapes declared here once, adopted-verbatim on the decode-boundary names the C# side mints. Beside the registry sit the four cross-cutting mechanics every row shares, each spelled once: the reason-discriminated `WireFault` rail with its bounded, replayable poison quarantine; the `Parity` combinator family — content-key mint-and-compare, golden-byte roundtrip, and the reflection walk over key cells; the divert-and-dedup `feed` combinator whose per-family transition policy is a row; and the sequence-gap Mealy the oplog watermark and the frame ordinal chain both mint evidence through. The module is `core/src/interchange/codec.ts`; a new wire family is one census row plus one landing row, a new failure cause is one policy row, and a new feed is one `_feeds` row — never a sibling page, never a second rail.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                      | [PUBLIC]                  |
| :-----: | :----------------- | :-------------------------------------------------------------------------- | :------------------------ |
|  [01]   | `WIRE_CENSUS`      | the closed family tuple, arm/source/consumer/home columns, the wire literal | `Wire` (census reads)     |
|  [02]   | `FAULT_RAIL`       | `WireFault` policy table, poison intake, budgeted replay, the divert        | `WireFault`, `Quarantine` |
|  [03]   | `PARITY_VERIFY`    | content-key mint delegate, verify, roundtrip, the key-cell reflection walk  | `Parity`                  |
|  [04]   | `LANDING_EVIDENCE` | evidence/identity/version landings into core vocabulary + the CRDT op union | `CrdtOp`, `OpLog` shapes  |
|  [05]   | `LANDING_WIRE`     | wire-owned decoded shapes for later-wave consumers                          | landing classes on `Wire` |
|  [06]   | `KEYED_REGISTRY`   | the mapped landing table, the polymorphic decode/encode/stream entrypoints  | `Wire`                    |
|  [07]   | `FEED_DEDUP`       | the divert+dedup stream combinator and its per-family policy rows           | `feed`                    |
|  [08]   | `SEQUENCE_GAP`     | the gap Mealy, the resumable oplog stream, the frontier read                | `Gap`, `OpLog`            |

## [02]-[WIRE_CENSUS]

[WIRE_CENSUS]:
- Owner: the census anchors — `_families`, the ordered key tuple of every C#-minted wire family; `_census`, the fact table carrying `arm` (the closed four-value format axis), `source` (the C# mint), `consumer` (the surface reading the decoded value: `value`, `state`, `observe`, `interchange`, `security`, `data`, `runtime`, `ui`), and `home` (the interchange page owning the landing: `codec`, `format`, `contract`, `frame`, `invoke`); `_wireLiteral`, the family-name schema every fault, feed, and verdict types `family` fields with. The merged-hub guard pair ties tuple and table closed in both directions, and the `Home`/`Consumer` type anchors govern the fact columns — a census row naming a page or surface outside either closed set fails the row guard, never a review.
- Law: the census is the plane's single source of truth for which wire families exist — a decode surface for a family absent from the census, or a census row with no landing at its home page, is the defect the contract gate's coverage walk surfaces.
- Law: `arm` is closed at four — `proto`, `cbor`, `msgpack`, `jsonpatch` — one arm per C# mint format; a family under two arms is a census error, never a dispatch case.
- Law: `home` partitions the landing obligation — `codec` rows resolve in this module's landing table; `format`, `contract`, `frame`, and `invoke` rows land at their owning page, and the census still names them so coverage is one walk over one tuple.
- Growth: a new C# wire family is one tuple entry plus one census row plus one landing row at its home — the guards break every stale projection at compile time; never a new folder, never a parallel list.
- Boundary: verdict grading over descriptor generations is the contract page's; the proto `GenMessage` suite the census's proto rows bind is `format#PROTO_ENGINE`'s.
- Packages: `effect` (`Schema`, `Array`).

```typescript
import { Array, type ParseResult, Schema, type Types } from "effect"

const _arms = ["proto", "cbor", "msgpack", "jsonpatch"] as const

const _families = [
  "ReceiptEnvelopeWire", "HlcStampWire", "TenantContextWire", "CommandAvailabilityWire", "RenderReceiptWire",
  "FaultDetailWire", "QuantityWire",
  "ElementGraphWire", "NodeWire", "RelationshipWire",
  "OpLogWire", "SnapshotHeader", "CrdtOpWire",
  "CommitWire", "BranchWire", "VersionVectorWire", "MerkleSummaryWire",
  "JsonPatchDocument", "ProgressMarkWire", "CredentialPemWire",
  "BenchmarkClaimWire", "HostFingerprintWire",
  "BindingStatusWire", "CoercedValueWire", "WriteReceiptWire",
  "FlagVerdictWire", "ControlIntentWire", "LayoutConstraintWire", "CommandGateWire",
  "BcfTopicWire", "BcfViewpointWire", "GeoFeatureWire",
  "BimWire", "DiffWire", "IdsAuditWire",
  "MaterialWire", "OpenPbrGroupsWire", "AppearanceSummaryWire",
  "ArtifactFrameWire", "GeometryPayloadWire", "GeometryResidencyWire",
  "CommandPayloadWire", "SupportCaptureWire", "CapabilityDescriptorWire",
  "FileDescriptorSetWire",
] as const

const _census = {
  ReceiptEnvelopeWire: { arm: "proto", source: "Rasm.AppHost", consumer: "state", home: "codec" },
  HlcStampWire: { arm: "proto", source: "Rasm.AppHost", consumer: "value", home: "codec" },
  TenantContextWire: { arm: "proto", source: "Rasm.AppHost", consumer: "value", home: "codec" },
  CommandAvailabilityWire: { arm: "proto", source: "Rasm.AppHost/Observability", consumer: "state", home: "codec" },
  RenderReceiptWire: { arm: "proto", source: "Rasm.AppUi/Render", consumer: "ui", home: "codec" },
  FaultDetailWire: { arm: "proto", source: "Rasm.Compute/Runtime", consumer: "interchange", home: "codec" },
  QuantityWire: { arm: "proto", source: "Rasm.Compute", consumer: "value", home: "codec" },
  ElementGraphWire: { arm: "proto", source: "Rasm.Element/Graph", consumer: "ui", home: "codec" },
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
  CredentialPemWire: { arm: "proto", source: "Rasm.AppHost", consumer: "security", home: "codec" },
  BenchmarkClaimWire: { arm: "proto", source: "Rasm.AppHost/Observability", consumer: "ui", home: "codec" },
  HostFingerprintWire: { arm: "proto", source: "Rasm.AppHost/Runtime", consumer: "ui", home: "codec" },
  BindingStatusWire: { arm: "proto", source: "Rasm.AppHost/Wire", consumer: "ui", home: "codec" },
  CoercedValueWire: { arm: "proto", source: "Rasm.AppHost/Wire", consumer: "ui", home: "codec" },
  WriteReceiptWire: { arm: "proto", source: "Rasm.AppHost/Wire", consumer: "ui", home: "codec" },
  FlagVerdictWire: { arm: "proto", source: "Rasm.AppHost", consumer: "runtime", home: "codec" },
  ControlIntentWire: { arm: "proto", source: "Rasm.AppUi/Shell", consumer: "ui", home: "codec" },
  LayoutConstraintWire: { arm: "proto", source: "Rasm.AppUi/Shell", consumer: "ui", home: "codec" },
  CommandGateWire: { arm: "proto", source: "Rasm.AppUi/Shell", consumer: "ui", home: "codec" },
  BcfTopicWire: { arm: "proto", source: "Rasm.Bim", consumer: "ui", home: "codec" },
  BcfViewpointWire: { arm: "proto", source: "Rasm.Bim", consumer: "ui", home: "codec" },
  GeoFeatureWire: { arm: "proto", source: "Rasm.Bim/Semantics", consumer: "ui", home: "codec" },
  BimWire: { arm: "proto", source: "Rasm.Bim/Exchange", consumer: "ui", home: "codec" },
  DiffWire: { arm: "proto", source: "Rasm.Bim/Exchange", consumer: "ui", home: "codec" },
  IdsAuditWire: { arm: "proto", source: "Rasm.Bim/Exchange", consumer: "ui", home: "codec" },
  MaterialWire: { arm: "proto", source: "Rasm.Materials/Appearance", consumer: "ui", home: "codec" },
  OpenPbrGroupsWire: { arm: "proto", source: "Rasm.Materials/Appearance", consumer: "ui", home: "codec" },
  AppearanceSummaryWire: { arm: "proto", source: "Rasm.Materials/Appearance", consumer: "ui", home: "codec" },
  ArtifactFrameWire: { arm: "proto", source: "Rasm.Compute/Runtime", consumer: "runtime", home: "frame" },
  GeometryPayloadWire: { arm: "proto", source: "Rasm.Compute/Runtime", consumer: "ui", home: "frame" },
  GeometryResidencyWire: { arm: "proto", source: "Rasm.AppUi/Render", consumer: "runtime", home: "frame" },
  CommandPayloadWire: { arm: "proto", source: "Rasm.AppUi/Shell", consumer: "interchange", home: "invoke" },
  SupportCaptureWire: { arm: "proto", source: "Rasm.AppHost", consumer: "observe", home: "invoke" },
  CapabilityDescriptorWire: { arm: "proto", source: "Rasm.AppHost/Agent", consumer: "interchange", home: "invoke" },
  FileDescriptorSetWire: { arm: "proto", source: "Rasm.Compute/Runtime", consumer: "interchange", home: "contract" },
} as const

const _wireLiteral: Schema.Literal<Wire.Families> = Schema.Literal(..._families)
```

## [03]-[FAULT_RAIL]

[FAULT_RAIL]:
- Owner: `WireFault`, the one reason-discriminated `Schema.TaggedError` for the whole plane over the `_policy` table — rank, quarantine disposition, replayability per reason — with `fromSlot`, the patch-arm slot triage, riding it as a static; and `Quarantine`, the `Effect.Service` owning the bounded poison intake, the held-frame census, the budgeted replay drain, and the `divert` dual transformer every decode surface composes; `PoisonFrame` rides the service as `Quarantine.Frame`.
- Law: the family is sized by routing — every consumer routes on the one tag and reads `reason`/`family` as evidence; a per-cause class or a second plane fault family is the named spam defect, and `family` is typed by the census literal so an unnamed family is a compile error.
- Law: evidence is data — `evidence` carries the `{ actual, expected }` pair for `stale`, `parity`, and `sequence`; `message` derives from fields and is never stored; classification of a `ParseError` into the family happens exactly once, at the intake seam where frame context exists to name `family` and `reason`.
- Law: `sequence` never quarantines — a gap has no frame to hold; `overrun` marks a pre-decode ceiling refusal the frame rail mints; engine-internal ceiling throws surface as `ParseError` and classify `malformed` at intake; a truncated size-delimited header triages through `format#PROTO_ENGINE`'s `peek` into `truncated`.
- Law: the patch arm's per-op error slots classify through `fromSlot` — `TestError` folds to `stale` carrying its `{ actual, expected }` pre-image evidence, `MissingError` to `conflict` naming the vanished path, and the residue to `malformed` — so the OCC refusal and the concurrent-edit divergence read as data on the one rail, and `stale`/`conflict` have exactly one mint site.
- Law: the intake is bounded with `strategy: "suspend"` — a poison storm backpressures its producer; `octets` arrive as a lazy thunk so the re-encode runs only on the failure path; `attempts` lives on the frame and replay re-enters a successor carrying `attempts + 1` with the original fault intact, so the terminal report names the first cause.
- Law: the held census is slot-keyed and settles — the slot is `family` plus intake instant, a replay successor overwrites its predecessor's slot, and a delivered or retired frame leaves the census in the same drain, so `census` reads the live poison set exactly and the table cannot grow past the frames still owed a verdict; `release` is the foreign-eviction verb over the same slot.
- Law: replay is generic over every row — the drain takes the family-keyed decode as a parameter, so the service imports no landing and the app root supplies the record it composed from `Wire.decode`; the pump cadence is unbounded `spaced` because the per-frame `attempts` budget is the bound, and the drain suspends on an empty intake rather than polling.
- Growth: a new failure cause is one `_policy` row; a retention or per-family cap axis is one `_INTAKE` field.
- Boundary: the wire-crossed `FaultDetail` altitude is `[05]`'s landing — a local rail importing it for a local failure is the altitude defect; availability degradation under a poison storm is `state` vocabulary wired at the app root.
- Packages: `effect` (`Schema`, `Effect`, `Mailbox`, `Ref`, `HashMap`, `Chunk`, `DateTime`, `Schedule`, `Order`, `Array`, `Either`, `Function`, `Match`, `Option`); `rfc6902/patch` (`MissingError`, `TestError`); `./format.ts` (`Patch`).

```typescript
import { Chunk, DateTime, Effect, Either, Function, HashMap, Mailbox, Match, Option, Order, Predicate, Ref, Schedule } from "effect"
import { MissingError, TestError } from "rfc6902/patch"
import type { Patch } from "./format.ts"

const _causes = ["malformed", "truncated", "overrun", "sequence", "parity", "drift", "stale", "conflict"] as const

const _policy = {
  malformed: { rank: 4, quarantine: true, replayable: true },
  truncated: { rank: 3, quarantine: true, replayable: true },
  overrun: { rank: 5, quarantine: true, replayable: false },
  sequence: { rank: 3, quarantine: false, replayable: false },
  parity: { rank: 6, quarantine: true, replayable: false },
  drift: { rank: 5, quarantine: true, replayable: true },
  stale: { rank: 2, quarantine: false, replayable: true },
  conflict: { rank: 2, quarantine: false, replayable: true },
} as const

class WireFault extends Schema.TaggedError<WireFault>()("WireFault", {
  family: _wireLiteral,
  reason: Schema.Literal(..._causes),
  detail: Schema.NonEmptyString,
  evidence: Schema.optionalWith(Schema.Struct({ actual: Schema.Unknown, expected: Schema.Unknown }), { as: "Option" }),
}) {
  static readonly byRank: Order.Order<WireFault> = Order.mapInput(Order.number, (fault: WireFault) => fault.policy.rank)
  static readonly dominant = (faults: Array.NonEmptyReadonlyArray<WireFault>): WireFault =>
    Array.max(faults, WireFault.byRank)
  static readonly fromSlot = (family: Wire.Family, slot: Patch.Slot, at: number): WireFault =>
    Match.value(slot).pipe(
      Match.when(Match.instanceOf(TestError), (test) =>
        new WireFault({ family, reason: "stale", detail: `<test@${at}>`, evidence: Option.some({ actual: test.actual, expected: test.expected }) })),
      Match.when(Match.instanceOf(MissingError), (missing) =>
        new WireFault({ family, reason: "conflict", detail: `<missing@${at}:${missing.path}>`, evidence: Option.none() })),
      Match.orElse((residue) =>
        new WireFault({ family, reason: "malformed", detail: `<op@${at}:${String(residue)}>`, evidence: Option.none() })),
    )
  get policy(): (typeof _policy)[WireFault.Reason] {
    return _policy[this.reason]
  }
  override get message(): string {
    return `<${this.family}:${this.reason}> ${this.detail}`
  }
}

declare namespace WireFault {
  type Reason = keyof typeof _policy
  type Row = { readonly rank: number; readonly quarantine: boolean; readonly replayable: boolean }
  type _Rows<T extends Record<(typeof _causes)[number], Row> = typeof _policy> = T
  type _Keys<K extends (typeof _causes)[number] = Reason> = K
}

const _INTAKE = { capacity: 256, attempts: 3 } as const
const _REPLAY: Schedule.Schedule<number> = Schedule.spaced("30 seconds")

class PoisonFrame extends Schema.Class<PoisonFrame>("PoisonFrame")({
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
    const box = yield* Mailbox.make<PoisonFrame>({ capacity: _INTAKE.capacity, strategy: "suspend" })
    const held = yield* Ref.make(HashMap.empty<string, PoisonFrame>())
    const slot = (frame: PoisonFrame): string => `${frame.family}:${DateTime.formatIso(frame.at)}`
    const admit = (frame: PoisonFrame): Effect.Effect<PoisonFrame> =>
      box.offer(frame).pipe(
        Effect.andThen(Ref.update(held, HashMap.set(slot(frame), frame))),
        Effect.as(frame),
      )
    const settled = (frame: PoisonFrame): Effect.Effect<void> => Ref.update(held, HashMap.remove(slot(frame)))
    return {
      intake: (family: Wire.Family, octets: Uint8Array, fault: WireFault) =>
        Effect.flatMap(DateTime.now, (now) => admit(new PoisonFrame({ family, octets, fault, at: now, attempts: 0 }))),
      census: Ref.get(held).pipe(Effect.map((table) => Array.fromIterable(HashMap.values(table)))),
      release: (frame: PoisonFrame) => settled(frame),
      replayed: <A, R>(
        decode: (family: Wire.Family, octets: Uint8Array) => Effect.Effect<A, WireFault, R>,
        delivered: (value: A) => Effect.Effect<void, never, R>,
        retired: (frame: PoisonFrame) => Effect.Effect<void, never, R>,
      ): Effect.Effect<void, never, R> =>
        Effect.flatMap(Effect.orDie(box.take), (first) =>
          Effect.flatMap(box.takeAll, ([rest]) =>
            Effect.forEach(Chunk.prepend(rest, first), (frame) =>
              frame.replayable
                ? decode(frame.family, frame.octets).pipe(Effect.matchEffect({
                    onFailure: () => Effect.asVoid(admit(new PoisonFrame({ ...frame, attempts: frame.attempts + 1 }))),
                    onSuccess: (value) => Effect.andThen(delivered(value), settled(frame)),
                  }))
                : Effect.andThen(retired(frame), settled(frame)), { concurrency: 1, discard: true }))).pipe(Effect.repeat(_REPLAY), Effect.asVoid),
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
          (fault) => fault.policy.quarantine,
          (fault) => Effect.as(Quarantine.intake(context.family, context.octets(), fault), Either.left(fault)),
        ),
      ),
  )
}
```

## [04]-[PARITY_VERIFY]

[PARITY_VERIFY]:
- Owner: `Parity`, the one verify combinator family — `key(payload)` the delegated content mint, `verified(family, expected, payload)` the mint-and-compare gate every content-addressed row shares, `matched(family, actual, expected)` the pure key-pair gate for pre-minted comparisons, `roundtrip(family, schema, octets)` the golden-byte decode-encode-compare proof generic over any byte schema, and `cells(gen, fields)` the reflection walk extracting content-key byte cells off a decoded proto message for field-level parity.
- Law: the mint is delegated, never local — `Digest.mint("content", payload)` is the branch's one `XxHash128` seed-zero fold with the canonical `:x32` spelling, branded keys compare by bare `===`, and a second mint or normalize step anywhere on a verify path is the cross-language drift defect.
- Law: the payload is `Digest.Payload` — a whole buffer or a band iterable riding the mint's own chunk-walk modality — so a multi-frame artifact verifies over its held bands with no joined re-hash and a parity miss refuses before any summed allocation exists; the streaming lane is the same single delegation site, never a second verify.
- Law: a parity miss is evidence, not a crash — the fault holds both keys (or both extents for the byte proof) so the operator report and the quarantine row read the disagreement as data.
- Law: the reflection walk resolves fields once at composition — `buildPath` addresses each named key cell against the `GenMessage`, `reflect` reads by descriptor with no generated type, and only `Uint8Array` cells survive the filter; verification of the extracted cells is `verified` on the same combinator, so extraction and proof never fork.
- Growth: a second content-keyed proto family composes `cells` with its own field roster — one call, zero new walks.
- Boundary: the `Digest` table, session algebra, and binary key twin are `value/contentKey.ts`'s; the frame rail's whole-artifact verify and the invoke page's descriptor admission compose `verified` and add nothing to it.
- Packages: `@bufbuild/protobuf` (`buildPath`, `reflect`, `DescField`, `DescMessage`, `Path`); `effect` (`Schema`, `Effect`, `Either`, `Option`, `Array`); `../value/contentKey.ts` (`ContentKey`, `Digest`).

```typescript
import { buildPath, type DescField, type DescMessage, type Message, type Path, reflect, type UnknownField } from "@bufbuild/protobuf"
import { ContentKey, Digest } from "../value/contentKey.ts"
import { Cbor, Pack, Proto } from "./format.ts"

const _mismatch = (family: Wire.Family, actual: unknown, expected: unknown, detail: string): WireFault =>
  new WireFault({ family, reason: "parity", detail, evidence: Option.some({ actual, expected }) })

const Parity: {
  readonly key: (payload: Digest.Payload) => Effect.Effect<ContentKey>
  readonly matched: (family: Wire.Family, actual: ContentKey, expected: ContentKey) => Effect.Effect<void, WireFault>
  readonly verified: (family: Wire.Family, expected: ContentKey, payload: Digest.Payload) => Effect.Effect<void, WireFault>
  readonly roundtrip: <A>(
    family: Wire.Family,
    schema: Schema.Schema<A, Uint8Array>,
    octets: Uint8Array,
  ) => Effect.Effect<void, ParseResult.ParseError | WireFault>
  readonly cells: (gen: DescMessage, fields: ReadonlyArray<string>) => {
    readonly paths: ReadonlyArray<Path>
    readonly read: (octets: Uint8Array) => Either.Either<ReadonlyArray<Uint8Array>, ParseResult.ParseError>
  }
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
      const identical = emitted.length === octets.length && emitted.every((byte, index) => byte === octets[index])
      return identical
        ? undefined
        : yield* _mismatch(family, emitted.length, octets.length, "<golden-byte-divergence>")
    }),
  cells: (gen, fields) => {
    const resolved: ReadonlyArray<DescField> = Array.filterMap(fields, (name) =>
      Array.findFirst(gen.fields, (field) => field.name === name))
    return {
      paths: Array.map(resolved, (field) => buildPath(gen).field(field).toPath()),
      read: (octets) =>
        Either.map(Schema.decodeUnknownEither(Proto.frame(gen))(octets), (message) =>
          Array.filterMap(resolved, (field) =>
            Option.liftPredicate(reflect(gen, message).get(field), (cell): cell is Uint8Array => cell instanceof Uint8Array))),
    }
  },
}
```

## [05]-[LANDING_EVIDENCE]

[LANDING_EVIDENCE]:
- Owner: the core-landing rows and the CRDT op union — `ReceiptEnvelopeWire`, `HlcStampWire`, `TenantContextWire`, `CommandAvailabilityWire`, `QuantityWire`, `ProgressMarkWire` decode INTO `state`/`value` owners whole with zero local twins; `CommitWire`/`BranchWire`/`VersionVectorWire`/`MerkleSummaryWire` land the `state` version plane over the msgpack arm; `CrdtOp` is the tagged six-op journal union — `Assign`, `Adjoin`, `Retire`, `Splice`, `Tick`, and the `Alien` foreign-ext landing — whose `hlc` cells intern through the `format#MSGPACK_ENGINE` extension row and whose per-case merge instances bind at `state/merge.ts`'s algebra.
- Law: the typed families never erase — the envelope's `receipt` field decodes as `state`'s tagged receipt union with every kind distinct, the stamp decodes through the kernel `Hlc` class shape (physical half first, logical second), and `TenantContext` crosses verbatim as the one tenancy value; a flattened `{ kind, payload }` landing is the collapse defect.
- Law: nested case families carry their `_tag` on the C# emit — the receipt kinds and availability verdicts mint the discriminant wire-side as part of the adopted-verbatim contract, pinned by the roster-parity corpus fixtures; a nested family shipped untagged gains its discriminant at the landing exactly as `[06]`'s `_stamp` law spells.
- Law: a new receipt kind, availability level, or version-plane axis is a C# case plus a `state` vocabulary row and zero edits here — the landings compose the sibling owners whole, so roster parity pins at this seam by construction.
- Law: `Tick.delta` is `bigint` — i64 counters ride the msgpack `useBigInt64` posture; a `Number`-typed delta is the precision defect.
- Law: an unregistered msgpack ext at op position lands the `Alien` case — `Pack.Alien` admits the engine's `ExtData` by identity, the transform carries ext type and cell verbatim, and `Pack.alien` re-mints on encode — so a newer peer's op family surfaces as typed contract-drift material the operator grades beside the descriptor verdicts, never a dropped byte and never a decode fault; a merge consumer treats `Alien` as a hold-and-report row, never a mergeable op.
- Boundary: merge lawfulness, convergence proofs, and the corpus fixtures binding the op family are `state/merge.ts`'s `Converge` surface; the SI scalar crossed by `QuantityWire` canonicalized once at C# admission and never re-converts here.
- Packages: `effect` (`Schema`); `./format.ts` (`Pack`); `../value/clock.ts` (`Hlc`); `../state/evidence.ts` (`ReceiptEnvelope`, `ProgressMark`, `Availability`); `../state/commit.ts` (`Commit`); `../state/causal.ts` (`Vector`); `../value/quantity.ts` (`Quantity`); `../value/identity.ts` (`TenantContext`).

```typescript
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
- Owner: the wire-owned decoded shapes — decode-boundary vocabulary for consumers in later waves, adopted verbatim from the C# mints and declared exactly once. The evidence plane: `RenderReceipt` (the frame-hash proof; `matched` is C#-computed and never re-hashed), `FaultDetail` over the `Hops` sixteen-row vocabulary with the `FaultEnricher` Layer, `FlagVerdict` (the OpenFeature evaluation projection the runtime flag service consumes). The shell plane: `BindingStatus`/`CoercedValue`/`WriteReceipt` live-binding triple, the closed `ControlIntent` union gaining its `_tag` at the declaration, `LayoutProgram` (order-preserving Cassowary constraint program, decode-only, never solved here). The BIM plane: `BcfTopic`/`BcfViewpoint` over the one `_GlobalId` brand, `BimModel`/`BimDiff`/`IdsAudit`. The appearance plane: `Material`/`PbrGroups`/`AppearanceSummary` mirroring the OpenPBR projection field-for-field. The geo plane: `GeoFeature` with the opaque WKB band, the seven-kind geometry union, the CRS rows, the tile quadkey algebra, and the `WkbParser` port. The identity plane: `SnapshotHeader` (canonical-CBOR, segment roster), `Claim`/`HostFingerprint` with the boot-identity admission gate, `Credential` (the sealed PEM carrier — secret sealed AT the decode transform, fingerprint-only audit identity, sealed rotation compare).
- Law: `_GlobalId` is one anchor — the twenty-two-character IFC base64 identity brands once and both the BCF and BIM planes compose it; a per-plane re-declaration is the split-brain defect this collapse killed. `BcfViewpoint.GlobalId` is the exported decode surface: the ui selection plane resolves raw pick material through `Schema.decodeUnknownOption(BcfViewpoint.GlobalId)`, so a locally-minted brand beside it is unspellable.
- Law: the wire ships tagged families untagged — `Schema.tag` demands `_tag` on decode input, so every tagged landing decodes through its `FromWire` twin, `_stamp` minting the discriminant at the seam exactly as `ControlIntent` attaches its own; the stamp overwrites nothing a tagged wire already carries, encode passes through, and the twin rides the owner as a static so one import serves class and wire. Discriminant-attach has exactly these two spellings by structural necessity — `Schema.attachPropertySignature` where the input is a `Struct`, `_stamp` where the landing is a `Schema.Class` the combinator cannot prepend to — one concept, never drift, and a third spelling is the defect.
- Law: the landing-class roster is a ratified co-located owner family — the census demands every wire-owned decoded shape in this one module, each class is an independent decode owner a later wave consumes directly, and collapsing the roster onto `Wire.*` statics trades one-hop resolution for a cosmetic export count; the charter accepts the wide export tail and the census guard keeps it closed.
- Law: `Hops` carries four columns — gRPC `code`, `retryable`, `terminal`, and `class`, the `value/fault` classification each hop reason projects — so `FaultDetail` satisfies the branch classification convention structurally and every compiled `Budget` schedule gates it with zero adapter; the code-to-reason projection generates from the table's own `code` column and cannot drift.
- Law: `FaultDetail` is wire-only altitude — constructed at exactly two sites: the `FaultDetailWire` decode row and the invoke page's transport fold; a third construction site in the branch is the defect the architecture suite audits. `EnricherLive` satisfies the `value/fault` `FaultEnricher` endo-arrow — a capture whose `tag` is not `FaultDetail` passes through untouched, so enrichment degrades to identity and never breaks crash capture; the stamped keys are the `_WIRE_ATTR` vocabulary rows — this enricher's owned `wire.*` axis beside the corpus-wide registry the observe convention page owns — never free string literals at the call site.
- Law: `Credential.material` is `Schema.Redacted` — the secret never exists raw past the decode transform, rotation compares sealed through `_sameMaterial`, the equivalence `Schema.equivalence` derives from the field's own `Schema.Redacted` declaration so equality has no second spelling beside the schema, and `fingerprint` is the only audit identity a log meets.
- Law: `GeoFeature`'s WKB band is opaque carriage under the gated `WkbParser` port — geometry materializes only through the port the ui wave satisfies, and the tile algebra (`quadkey`, `parent`, `children`) is total over the zoom-bounded grid refinement.
- Exemption: `Crs.of`'s `in`-probe key narrowing, the `EnricherLive` reason-token probe (`token in _hops` behind its refinement), and the `Tile.quadkey` bit walk are marked kernels — the checker cannot carry the probe onto the key type, and only immutable values leave.
- Growth: a new shell intent, appearance block, BCF axis, or fault evidence field is one case or field mirroring the C# emit; a new landing plane is one owner block here plus its census rows.
- Boundary: rollout targeting and flag evaluation are the runtime wave's service over this decoded verdict; GLB parsing, kiwi solving, BCF re-location, and OpenPBR rendering are ui-wave consumers of these values.
- Packages: `effect` (`Schema`, `Effect`, `Layer`, `Context`, `Equivalence`, `Function`, `Predicate`, `Redacted`, `HashMap`, `Option`); `../value/contentKey.ts` (`Digest`); `../value/clock.ts` (`Hlc`); `../value/identity.ts` (`AppIdentity`); `../value/fault.ts` (`FaultClass`, `FaultEnricher`).

```typescript
import { Context, Equivalence, Layer, Redacted } from "effect"
import { AppIdentity } from "../value/identity.ts"
import { FaultClass, FaultEnricher } from "../value/fault.ts"

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

const _hops = {
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
} as const

declare namespace Hops {
  type Reason = keyof typeof _hops
  type Row = { readonly code: number; readonly retryable: boolean; readonly terminal: boolean; readonly class: FaultClass.Kind }
  type Shape = Types.Simplify<typeof _hops & {
    readonly reasons: typeof _reasons
    readonly wire: Schema.Literal<typeof _reasons>
    readonly fromCode: (code: number) => Reason
  }>
  type _Rows<T extends Record<(typeof _reasons)[number], Row> = typeof _hops> = T
  type _Keys<K extends (typeof _reasons)[number] = Reason> = K
}

const _byCode: HashMap.HashMap<number, Hops.Reason> = Array.reduce(
  _reasons,
  HashMap.empty<number, Hops.Reason>(),
  (acc, reason) => HashMap.set(acc, _hops[reason].code, reason),
)

const Hops: Hops.Shape = {
  ..._hops,
  reasons: _reasons,
  wire: Schema.Literal(..._reasons),
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

const _SPELLING = /^<[^:>]+:([a-z]+)>/
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
          capture.tag === "FaultDetail"
            ? Option.match(
                Option.filter(
                  Option.fromNullable(_SPELLING.exec(capture.detail)?.[1]),
                  (token): token is Hops.Reason => token in _hops,
                ),
                {
                  onNone: () => capture,
                  onSome: (reason) =>
                    capture.enriched({
                      [_WIRE_ATTR.reason]: reason,
                      [_WIRE_ATTR.retryable]: String(_hops[reason].retryable),
                      [_WIRE_ATTR.terminal]: String(_hops[reason].terminal),
                    }),
                },
              )
            : capture,
        ),
    }),
  )
  get class(): FaultClass.Kind {
    return _hops[this.reason].class
  }
  get retryable(): boolean {
    return _hops[this.reason].retryable
  }
  get terminal(): boolean {
    return _hops[this.reason].terminal
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

const _Vec3 = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number)

const _orbit = Schema.Struct({ kind: Schema.Literal("orbit"), yaw: Schema.Number, pitch: Schema.Number }).pipe(Schema.attachPropertySignature("_tag", "Orbit"))
const _pan = Schema.Struct({ kind: Schema.Literal("pan"), dx: Schema.Number, dy: Schema.Number }).pipe(Schema.attachPropertySignature("_tag", "Pan"))
const _select = Schema.Struct({ kind: Schema.Literal("select"), targets: Schema.Array(Schema.NonEmptyString), additive: Schema.Boolean }).pipe(Schema.attachPropertySignature("_tag", "Select"))
const _section = Schema.Struct({ kind: Schema.Literal("section"), origin: _Vec3, normal: _Vec3 }).pipe(Schema.attachPropertySignature("_tag", "Section"))
const _measure = Schema.Struct({ kind: Schema.Literal("measure"), from: _Vec3, to: _Vec3 }).pipe(Schema.attachPropertySignature("_tag", "Measure"))
const _focus = Schema.Struct({ kind: Schema.Literal("focus"), target: Schema.NonEmptyString }).pipe(Schema.attachPropertySignature("_tag", "Focus"))

const ControlIntent: Schema.Union<[
  typeof _orbit, typeof _pan, typeof _select, typeof _section, typeof _measure, typeof _focus,
]> = Schema.Union(_orbit, _pan, _select, _section, _measure, _focus)
type ControlIntent = typeof ControlIntent.Type

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

const _Color = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number)
const _Weight = Schema.Number.pipe(Schema.between(0, 1))

class PbrGroups extends Schema.Class<PbrGroups>("PbrGroups")({
  key: Digest.FromBytes,
  base: Schema.Struct({ color: _Color, metalness: _Weight, roughness: _Weight, weight: _Weight }),
  specular: Schema.Struct({ color: _Color, weight: _Weight, ior: Schema.Number.pipe(Schema.positive()) }),
  transmission: Schema.Struct({ weight: _Weight, color: _Color, depth: Schema.Number.pipe(Schema.nonNegative()) }),
  emission: Schema.Struct({ color: _Color, luminance: Schema.Number.pipe(Schema.nonNegative()) }),
  geometry: Schema.Struct({ opacity: _Weight, thinWalled: Schema.Boolean }),
}) {}
class Material extends Schema.Class<Material>("Material")({
  key: Digest.FromBytes,
  name: Schema.NonEmptyString,
  groups: Digest.FromBytes,
}) {}
class AppearanceSummary extends Schema.Class<AppearanceSummary>("AppearanceSummary")({
  model: Digest.FromBytes,
  materials: Schema.Int.pipe(Schema.nonNegative()),
  groupKeys: Schema.Array(Digest.FromBytes),
}) {}

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

class Node extends Schema.Class<Node>("Node")({
  key: Digest.FromBytes,
  kind: Schema.NonEmptyString,
  payload: Schema.Record({ key: Schema.String, value: Schema.Unknown }),
}) {}
class Relation extends Schema.Class<Relation>("Relation")({
  key: Digest.FromBytes,
  kind: Schema.NonEmptyString,
  source: Digest.FromBytes,
  target: Digest.FromBytes,
}) {}
class ElementGraph extends Schema.Class<ElementGraph>("ElementGraph")({
  key: Digest.FromBytes,
  nodes: Schema.Array(Node),
  relations: Schema.Array(Relation),
}) {
  static readonly Node: typeof Node = Node
  static readonly Relation: typeof Relation = Relation
  get byKey(): HashMap.HashMap<ContentKey, Node> {
    return Array.reduce(this.nodes, HashMap.empty<ContentKey, Node>(), (acc, node) => HashMap.set(acc, node.key, node))
  }
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

class HostFingerprint extends Schema.Class<HostFingerprint>("HostFingerprint")({
  print: Schema.NonEmptyString,
  machine: Schema.NonEmptyString,
  arch: Schema.NonEmptyString,
  cores: Schema.Int.pipe(Schema.positive()),
  runtime: Schema.NonEmptyString,
}) {}

class Claim extends Schema.Class<Claim>("Claim")({
  suite: Schema.NonEmptyString,
  metrics: Schema.NonEmptyArray(Schema.Struct({ label: Schema.NonEmptyString, value: Schema.Number, unit: Schema.NonEmptyString })),
  host: HostFingerprint,
  minted: Schema.DateTimeUtc,
}) {
  static readonly Host: typeof HostFingerprint = HostFingerprint
  static readonly admit = (claim: Claim, identity: AppIdentity): Effect.Effect<Claim, WireFault> =>
    claim.host.print === identity.host
      ? Effect.succeed(claim)
      : Effect.fail(_mismatch("BenchmarkClaimWire", claim.host.print, identity.host, "<foreign-host-claim>"))
}

class Credential extends Schema.Class<Credential>("Credential")({
  kind: Schema.Literal("signing", "tls", "api"),
  material: Schema.Redacted(Schema.String),
  fingerprint: Schema.NonEmptyString,
  notBefore: Schema.DateTimeUtc,
  notAfter: Schema.DateTimeUtc,
}) {
  static readonly rotated = (live: Credential, next: Credential): boolean => !_sameMaterial(live.material, next.material)
}

const _sameMaterial: Equivalence.Equivalence<Redacted.Redacted<string>> = Schema.equivalence(Credential.fields.material)
```

## [07]-[KEYED_REGISTRY]

[KEYED_REGISTRY]:
- Owner: `Wire`, the assembled registry — `_landingRows`, the ONE value anchor mapping every codec-homed family to its landing schema, from which `_Landing` derives by `Schema.Schema.Type` projection and `_landings` re-binds under the derived mapped annotation so the generic indexed message decode resolves one correlated signature per key; the `_schemas` byte-row table annotated by the same mapped contract; and the polymorphic entrypoints: `decode(family, octets)`, `encode(family, value)` for the egress-legal rows, `schema(family)` the raw byte schema for field composition, `stream(family, frames)` the framed feed with quarantine divert, `diverted` the one framed-divert combinator, `residue(message)` the preserved unknown-field read, `of(arm)` and `homed(page)` the census projections, plus the census facts and the wire literal spread onto the owner.
- Law: one keyed decode, spelled once — the landing correspondence is a value anchor and its type derives, so a hand-written landing type cannot drift from the table, the `_schemas` annotation ties the byte rows to the same anchor, and the per-page `_Landing`/`_rows` restatement this collapse killed is unspellable because a family's landing exists in exactly one table; the `_Landed`/`_LandingKeys` guard pair closes the landing table against the census `home` column in both directions — a codec-homed census row missing its landing line, or a landing line for a family homed elsewhere, fails at the declaration, never at the gate's runtime coverage walk.
- Law: rows landing sibling vocabulary compose the sibling owner whole (`Proto.family(Proto.suite.ReceiptEnvelopeWire, ReceiptEnvelope)`); rows landing wire-owned shapes compose `[06]`'s classes; msgpack rows ride `Pack.schema`, the cbor row rides `Cbor.frame` composed with its header class, and the jsonpatch row delegates to `format#JSONPATCH_ENGINE`'s document schema under home `format`.
- Law: `diverted` is the one framed-divert spelling — source fault to `malformed`, landing decode, quarantine divert, in one combinator over `(family, source, landing, octets)` — and every framed ingress instantiates it: `stream` over the proto walk with the landing decode of the already-parsed message (the byte schema never re-parses a frame), the oplog stream over the msgpack walk, and the frame page's envelope streams; a hand pipeline beside it re-derives the walk.
- Law: content-verified rows compose `Parity` at the entry — `verifiedSnapshot` re-proves the header key over the held octets, `admittedGraph` yields the contract gate before decoding under the drift verdict; verification is entry composition, never a per-row re-implementation.
- Law: `residue` reads the `$unknown` rows the `_READ` posture preserved on any decoded proto message — live-message drift a partial peer emits, the runtime complement of the contract gate's boot descriptor grade; a consumer surfaces a non-empty residue beside the family's drift verdict as evidence, the rows never mutate the landing, and re-emission through the same suite row round-trips them under `writeUnknownFields`.
- Law: quarantine thunks hold the whole-message byte form — `Wire.decode`'s own replay coordinate — so `_framedEmit` and the replay drain agree by construction; `Proto.delimit` re-frames octets only where an egress joins a size-delimited transport, never on the replay path.
- Growth: a new family is one `_Landing` line plus one `_schemas` row beside its census row; a new projection over the census is one member.
- Boundary: the contract gate service this registry's gated rows require is the contract page's; frame reassembly and the invoke verbs consume `schema`/`stream` and land their own shapes at their homes.
- Packages: `effect` (`Schema`, `Effect`, `Stream`, `Either`, `Option`); `@bufbuild/protobuf` (`Message`, `UnknownField`); `./format.ts` (`Proto`, `Pack`, `Cbor`).

```typescript
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
  RenderReceiptWire: RenderReceipt,
  FaultDetailWire: FaultDetail.FromWire,
  QuantityWire: Quantity,
  ElementGraphWire: ElementGraph,
  NodeWire: Node,
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
  BindingStatusWire: BindingStatus.FromWire,
  CoercedValueWire: CoercedValue.FromWire,
  WriteReceiptWire: WriteReceipt.FromWire,
  FlagVerdictWire: FlagVerdict,
  ControlIntentWire: ControlIntent,
  LayoutConstraintWire: LayoutProgram,
  BcfTopicWire: BcfTopic,
  BcfViewpointWire: BcfViewpoint,
  GeoFeatureWire: GeoFeature,
  BimWire: BimModel,
  DiffWire: BimDiff,
  IdsAuditWire: IdsAudit,
  MaterialWire: Material,
  OpenPbrGroupsWire: PbrGroups,
  AppearanceSummaryWire: AppearanceSummary,
} as const

type _LandingRows = typeof _landingRows
type _Landing = { readonly [K in keyof _LandingRows]: Schema.Schema.Type<_LandingRows[K]> }

const _landings: { readonly [K in keyof _LandingRows]: Schema.Schema<_Landing[K], Schema.Schema.Encoded<_LandingRows[K]>> } = _landingRows

const _schemas: { readonly [K in keyof _Landing]: Schema.Schema<_Landing[K], Uint8Array> } = {
  ReceiptEnvelopeWire: Proto.family(Proto.suite.ReceiptEnvelopeWire, ReceiptEnvelope),
  HlcStampWire: Proto.family(Proto.suite.HlcStampWire, Hlc),
  TenantContextWire: Proto.family(Proto.suite.TenantContextWire, TenantContext),
  CommandAvailabilityWire: Proto.family(Proto.suite.CommandAvailabilityWire, Availability),
  RenderReceiptWire: Proto.family(Proto.suite.RenderReceiptWire, RenderReceipt),
  FaultDetailWire: Proto.family(Proto.suite.FaultDetailWire, FaultDetail.FromWire),
  QuantityWire: Proto.family(Proto.suite.QuantityWire, Quantity),
  ElementGraphWire: Proto.family(Proto.suite.ElementGraphWire, ElementGraph),
  NodeWire: Proto.family(Proto.suite.NodeWire, Node),
  RelationshipWire: Proto.family(Proto.suite.RelationshipWire, Relation),
  OpLogWire: Pack.schema(OpLogEntry),
  SnapshotHeader: Cbor.frame.pipe(Schema.compose(SnapshotHeader, { strict: false })),
  CrdtOpWire: Pack.schema(CrdtOp),
  CommitWire: Pack.schema(Commit),
  BranchWire: Pack.schema(Commit.Branch),
  VersionVectorWire: Pack.schema(Vector),
  MerkleSummaryWire: Pack.schema(Commit.Merkle),
  ProgressMarkWire: Proto.family(Proto.suite.ProgressMarkWire, ProgressMark),
  CredentialPemWire: Proto.family(Proto.suite.CredentialPemWire, Credential),
  BenchmarkClaimWire: Proto.family(Proto.suite.BenchmarkClaimWire, Claim),
  HostFingerprintWire: Proto.family(Proto.suite.HostFingerprintWire, HostFingerprint),
  BindingStatusWire: Proto.family(Proto.suite.BindingStatusWire, BindingStatus.FromWire),
  CoercedValueWire: Proto.family(Proto.suite.CoercedValueWire, CoercedValue.FromWire),
  WriteReceiptWire: Proto.family(Proto.suite.WriteReceiptWire, WriteReceipt.FromWire),
  FlagVerdictWire: Proto.family(Proto.suite.FlagVerdictWire, FlagVerdict),
  ControlIntentWire: Proto.family(Proto.suite.ControlIntentWire, ControlIntent),
  LayoutConstraintWire: Proto.family(Proto.suite.LayoutConstraintWire, LayoutProgram),
  BcfTopicWire: Proto.family(Proto.suite.BcfTopicWire, BcfTopic),
  BcfViewpointWire: Proto.family(Proto.suite.BcfViewpointWire, BcfViewpoint),
  GeoFeatureWire: Proto.family(Proto.suite.GeoFeatureWire, GeoFeature),
  BimWire: Proto.family(Proto.suite.BimWire, BimModel),
  DiffWire: Proto.family(Proto.suite.DiffWire, BimDiff),
  IdsAuditWire: Proto.family(Proto.suite.IdsAuditWire, IdsAudit),
  MaterialWire: Proto.family(Proto.suite.MaterialWire, Material),
  OpenPbrGroupsWire: Proto.family(Proto.suite.OpenPbrGroupsWire, PbrGroups),
  AppearanceSummaryWire: Proto.family(Proto.suite.AppearanceSummaryWire, AppearanceSummary),
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
  type Framed = Extract<Homed, keyof Proto.Shape["suite"]>
  type Shape = Types.Simplify<typeof _census & {
    readonly arms: typeof _arms
    readonly families: Families
    readonly wire: Schema.Literal<Families>
    readonly of: (arm: Arm) => ReadonlyArray<Family>
    readonly homed: (home: Home) => ReadonlyArray<Family>
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
  }>
  type _Rows<T extends Record<Families[number], Row> = typeof _census> = T
  type _Keys<K extends Families[number] = Family> = K
  type _CodecHomed = { readonly [K in Family]: (typeof _census)[K]["home"] extends "codec" ? K : never }[Family]
  type _Landed<K extends Homed = _CodecHomed> = K
  type _LandingKeys<K extends _CodecHomed = Homed> = K
}

const _framedEmit = <K extends Wire.Framed>(family: K) => Schema.encodeSync(Proto.frame(Proto.suite[family]))

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

const Wire: Wire.Shape = {
  ..._census,
  arms: _arms,
  families: _families,
  wire: _wireLiteral,
  of: (arm) => Array.filter(_families, (family) => _census[family].arm === arm),
  homed: (home) => Array.filter(_families, (family) => _census[family].home === home),
  schema: (family) => _schemas[family],
  decode: (family, octets) => Schema.decodeUnknown(_schemas[family])(octets),
  encode: (family, value) => Schema.encode(_schemas[family])(value),
  stream: (family, frames) =>
    _diverted(family, Proto.stream(Proto.suite[family])(frames), Schema.decodeUnknown(_landings[family]), _framedEmit(family)),
  diverted: _diverted,
  residue: (message) => message.$unknown ?? [],
  verifiedSnapshot: (octets) =>
    Effect.tap(
      Schema.decodeUnknown(_schemas.SnapshotHeader)(octets),
      (header) => Parity.verified("SnapshotHeader", header.key, octets),
    ),
  admittedGraph: (gate, octets) => Effect.andThen(gate, Schema.decodeUnknown(_schemas.ElementGraphWire)(octets)),
}
```

## [08]-[FEED_DEDUP]

[FEED_DEDUP]:
- Owner: `feed`, the one transition-feed entry, and its policy rows — `_feeds` carries one row per feed family: the coalescing `subject` projection, the transition `alike` equivalence, and the optional `flow` throttle; the combinator composes `Wire.stream`'s framed divert with one keyed transition Mealy — per-subject last-value state, an arrival equivalent to its subject's incumbent drops, a transition emits and replaces — and the declared throttle shapes the surviving flow. The merged `feed` namespace carries the row vocabulary on the entry's own name.
- Law: the pipeline is spelled once — framed decode, poison divert, keyed dedup, throttle; the per-family variation is three row columns, so the progress and flag feeds that restated this pipeline as sibling pages are two rows here and a third feed is one row.
- Law: dedup is keyed, never global — the Mealy state maps subject to last emission, so interleaved subjects cannot mask each other's transitions; the state is fold-interior and single-fiber by construction, the ruled form over `Stream.groupByKey`, whose per-subject fiber fan-out re-merges without cross-subject order and buys nothing a last-value map needs.
- Law: `alike` derives, never restates — `Schema.equivalence` over the landing owner: whole for `FlagVerdict`, the `Schema.pick` projection naming the transition-identity axes for `ProgressMark` (the `stamp`/`tenant` noise axes excluded by rule, or every mark would transition); a hand-written `Equivalence.struct` beside the Schema owner is a second unverified equality truth.
- Law: throttle is a declared token bucket — `cost` prices a whole chunk, `"shape"` delays and never drops; a feed without a `flow` row passes unshapen.
- Growth: a new feed family is one `_feeds` row; a new flow axis is one field on the row's `flow` record.
- Boundary: what a consumer folds the deduped feed into — the runtime flag cell, the state progress table — is the consumer's plan; this combinator owns only the wire-to-transition geometry.
- Packages: `effect` (`Stream`, `Schema`, `Equivalence`, `HashMap`, `Chunk`, `Either`, `Duration`, `Option`).

```typescript
import type { Duration } from "effect"

const _feedKeys = ["ProgressMarkWire", "FlagVerdictWire"] as const

declare namespace feed {
  type Family = (typeof _feedKeys)[number]
  type Flow = { readonly units: number; readonly per: Duration.DurationInput; readonly burst: number }
  type Row<A> = {
    readonly subject: (value: A) => string
    readonly alike: Equivalence.Equivalence<A>
    readonly flow: Option.Option<Flow>
  }
}

const _feeds: { readonly [K in feed.Family]: feed.Row<Wire.Decoded<K>> } = {
  ProgressMarkWire: {
    subject: (mark) => mark.operation,
    alike: Schema.equivalence(Schema.Struct(ProgressMark.fields).pipe(Schema.pick("operation", "stage", "done", "total"))),
    flow: Option.some({ units: 240, per: "1 second", burst: 60 }),
  },
  FlagVerdictWire: {
    subject: (verdict) => verdict.flag,
    alike: Schema.equivalence(FlagVerdict),
    flow: Option.none(),
  },
}

const _transitions = <A>(row: feed.Row<A>) => <E, R>(marks: Stream.Stream<A, E, R>): Stream.Stream<A, E, R> =>
  marks.pipe(
    Stream.mapAccum(HashMap.empty<string, A>(), (seen, value) =>
      Option.match(HashMap.get(seen, row.subject(value)), {
        onNone: () => [HashMap.set(seen, row.subject(value), value), Option.some(value)] as const,
        onSome: (prior) =>
          row.alike(prior, value)
            ? ([seen, Option.none<A>()] as const)
            : ([HashMap.set(seen, row.subject(value), value), Option.some(value)] as const),
      })),
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
          Stream.throttle(deduped, {
            cost: Chunk.size,
            units: flow.units,
            duration: flow.per,
            burst: flow.burst,
            strategy: "shape",
          }),
      }),
  )
}
```

## [09]-[SEQUENCE_GAP]

[SEQUENCE_GAP]:
- Owner: `Gap`, the sequence-evidence vocabulary — `evidence(family, expected, actual, detail?)` the one sequence-fault mint (`<gap>` unless the chain names its own violation), and `sequential(family, resume)` the bigint watermark Mealy generic over any `seq`-carrying entry: entries at or below the running watermark — the seeded resume and every advance — drop inside the fold as replays, so a late out-of-order duplicate can neither re-anchor the watermark nor double-mint evidence; a successor exactly one past the watermark advances it, and a jump emits `sequence` evidence ahead of the jumped entry — both coordinates on the evidence, the entry still delivered — while the watermark re-anchors so one gap reports once and no arriving entry is lost; `OpLog` rides it — the resumable CRDT journal stream over the msgpack arm plus the `frontier` read.
- Law: the Mealy is the shared sequence law — the oplog watermark and the frame page's ordinal chain mint through the same `evidence` spelling, so sequence forensics read one shape branch-wide; `sequence` faults never quarantine because a gap has no frame to hold.
- Law: resume is the source's coordinate — the caller passes the last durably applied `seq`, so reconnect replays drop structurally and no downstream dedup set exists.
- Law: the frontier is the durable handoff — `Array.max` over the seq order on a non-empty batch, `Option.none` on empty, the value the data wave's journal persists as its resume coordinate.
- Growth: a per-family gap posture (a tolerated reorder window) is one parameter on `sequential`; a second sequential family composes the same Mealy.
- Boundary: what the delivered entries fold into is `state`'s plan altitude; durable journal positions are the data wave's.
- Packages: `effect` (`Stream`, `Order`, `Array`, `Chunk`, `Either`, `Option`).

```typescript
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
  AppearanceSummary, BcfTopic, BcfViewpoint, BimDiff, BimModel, BindingStatus, Claim, CoercedValue,
  ControlIntent, Credential, CrdtOp, ElementGraph, FaultDetail, feed, FlagVerdict, Gap, GeoFeature,
  Hops, IdsAudit, LayoutProgram, Material, OpLog, Parity, PbrGroups, Quarantine, RenderReceipt,
  SnapshotHeader, Wire, WireFault, WkbParser, WriteReceipt,
}
```
