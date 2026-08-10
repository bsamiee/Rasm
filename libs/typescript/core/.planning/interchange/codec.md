# [CORE_CODEC]

`Wire` owns the branch's closed wire vocabulary and everything derived from it: one ordered family roster, one row per family carrying direction, arm, schema, contract gate, and parity obligation, and the decoded landing each producer's shape resolves to. Fault classification, bounded quarantine with replay, content and semantic parity, keyed transition feeds, and sequence evidence all read that one table. Module `core/src/interchange/codec.ts` admits a family as one row, a fault cause as one policy row, and a transport status as one `Hops` row.

`Wire` composes the `value` floor's identity, clock, and schema owners, the `state` causal, commit, and evidence owners, and `observe`'s board claim; a quantity rides `MeasureValueWire` and binds no family here. Every codec arrives from `interchange/format`: arm selection, contract compatibility, and quarantined-frame rendering read `Format.rows.arm`, so no consumer here spells an encoding name. `interchange/carrier` binds `Wire.Family` for its typed-metadata table, and `interchange/frame` composes `Wire.Fault`, `Wire.Gap`, `Wire.Parity`, and `Wire.Quarantine` for its own bounded assemblers.

## [01]-[INDEX]

- [02]-[WIRE_REGISTRY]: ordered family vocabulary and exact row contract; `Wire`.
- [03]-[FAULT_RAIL]: fault policy, quarantine intake, replay, and divert; `Wire.Fault`, `Wire.Quarantine`.
- [04]-[PARITY_VERIFY]: content-key verification and semantic roundtrip; `Wire.Parity`.
- [05]-[LANDING_EVIDENCE]: evidence, identity, version, CRDT, and oplog landings; `Wire`.
- [06]-[LANDING_WIRE]: wire-owned decoded shapes for later-wave consumers; landing classes on `Wire`.
- [07]-[KEYED_REGISTRY]: mapped landing table, polymorphic decode/encode/stream entrypoints; `Wire`.
- [08]-[FEED_DEDUP]: quarantine diversion and family transition policies; `Wire.feed`.
- [09]-[SEQUENCE_GAP]: sequence evidence, oplog continuity, and frontier reads; `Wire.Gap`, `Wire.OpLog`.

## [02]-[WIRE_REGISTRY]

- Owner: `_families` and `_rows` close the ordered wire vocabulary and its typed registry.
- Law: each row preserves literal direction and arm and carries one schema, optional contract gate, and optional parity.
- Law: `_faultFamilies` widens the roster with families this page never decodes but whose owners raise faults against it.
- Law: `_faultArms` names the arm of every such family, so arm resolution stays total across the fault roster.
- Boundary: `Format` owns codec engines and the arm vocabulary; external producers own wire spellings.
- Boundary: `interchange/carrier` owns the message envelope and `Format.event` its media roster, so no row, landing class, fault cause, or parity obligation here names a CloudEvents shape.
- Boundary: a message envelope crossing this plane carries a wire family in its payload rather than being one.

```typescript signature
import { Array, type ParseResult, Schema, type Types } from "effect"

const _families = [
  "ReceiptEnvelopeWire", "HlcStampWire", "TenantContextWire", "CommandAvailabilityWire",
  "FaultDetail",
  "ElementGraphWire", "GraphDeltaWire", "NodeWire", "RelationshipWire",
  "OpLogWire", "SnapshotHeader", "CrdtOpWire",
  "CommitWire", "BranchWire", "VersionVectorWire", "MerkleSummaryWire",
  "EntityEditWire", "TallyWire", "CredentialPemWire", "DescriptorPinWire",
  "BenchmarkClaimWire", "HostFingerprintWire",
  "BindingStatusWire", "CoercedValueWire", "WriteReceiptWire",
  "FlagVerdictWire", "ControlIntentWire", "LayoutConstraintWire", "CommandGateWire", "EvidenceTimelineWire",
  "BcfTopicWire", "BcfViewpointWire", "ModelDiff", "PredicateWire",
  "MaterialWire", "OpenPbrGroupsWire", "TextureSetWire", "AssetSetManifest",
  "OrganizationWire",
  "SupportCaptureWire",
] as const

const _wireLiteral = Schema.Literal(..._families)
const _faultFamilies = [
  ..._families,
  "ArtifactFrame", "GeometryPayload", "GeometryResidencyWire", "IfcWire",
  "CommandPayloadWire",
] as const
const _faultLiteral = Schema.Literal(..._faultFamilies)

// Fault-only families decode at their OWN owner — `frame` assembles the artifact, geometry, residency, and IFC
// wire families and `invoke` decodes the command payload — so no `_rows` entry exists to read an arm from. Naming the
// arm here keeps arm resolution total across the whole fault roster, which is what lets a held frame from any of the
// five render: without it the quarantine census holds bytes it can print nothing about. The complement type
// closes the table both ways, so a sixth fault family fails at this declaration rather than at a silent absence.
const _faultArms = {
  ArtifactFrame: "proto",
  GeometryPayload: "proto",
  GeometryResidencyWire: "json",
  IfcWire: "json",
  CommandPayloadWire: "json",
} as const satisfies { readonly [K in Exclude<Wire.FaultFamily, Wire.Family>]: Format.Arm }
```

## [03]-[FAULT_RAIL]

- Owner: `Wire.Fault` classifies failures, and `Wire.Quarantine` owns bounded held-frame intake and replay.
- Law: `Fault.Class.order` supplies dominance; wire policy adds only retention and replay posture.
- Law: the census is the one bound — an insert past capacity evicts its oldest slot in the same transaction.
- Law: `divert` never re-fails; `held` decides octet retention alone, so one refused frame never ends a feed.
- Law: `diagnostic` renders through the family's arm row, so every arm and every fault family prints.
- Law: `held` admits a frame and exactly four owners end it — the pump on delivery, the pump on retirement, a caller through `release`, and intake's own eviction; no frame outlives all four.
- Law: the pump parks on an empty census and wakes on the first admission, so retention starts its clock at intake rather than at the next sweep.
- Law: the census is process-scoped and tenant-blind, so `diagnostic` renders one tenant's octets to whoever holds the service and no caller may treat it as a tenant-partitioned store.

```typescript signature
import { fromJson, toJson, type MessageShape } from "@bufbuild/protobuf"
import {
  Cause, Chunk, DateTime, Effect, Either, Exit, Function, HashMap, Option, Order, pipe, Predicate, Schedule,
  STM, TMap, TRef,
} from "effect"
import { Fault } from "../value/fault.ts"
import { Format } from "./format.ts"

const _causes = ["malformed", "truncated", "overrun", "sequence", "parity", "drift", "stale", "conflict"] as const

// This plane mints one family: `class` is the branch taxonomy every rail already grades and orders on, and the two
// columns beside it are genuinely this plane's — `held` whether the failing frame is RETAINED in the poison census
// (a frame-retention disposition, not the class table's repair-intake divert), `replayable` whether re-decoding the
// same octets can change the verdict at all (which no class-level retryability answers: unparseable bytes are
// non-retryable as transport and replayable as evidence the moment the producing peer is fixed). A local rank column
// beside `class` would fork the one severity lattice the branch tuple already declares.
const _policy = Fault.Class.family(_causes, {
  malformed: { class: "malformed", held: true, replayable: true },
  truncated: { class: "malformed", held: true, replayable: true },
  // Over-budget frames are the ONE cause whose evidence is its measurement: `Fault.evidence` already carries the
  // actual and expected extents, and retaining the octets would pin exactly the bytes the budget refused — a
  // census of oversized frames is the exhaustion the refusal exists to prevent, and `replayable: false` means the
  // retention buys no second verdict either.
  overrun: { class: "exhausted", held: false, replayable: false },
  sequence: { class: "absent", held: false, replayable: false },
  parity: { class: "breached", held: true, replayable: false },
  drift: { class: "invalid", held: true, replayable: true },
  stale: { class: "conflicted", held: false, replayable: true },
  conflict: { class: "conflicted", held: false, replayable: true },
})

class WireFault extends Schema.TaggedError<WireFault>()("WireFault", {
  family: _faultLiteral,
  reason: _policy.schema,
  detail: Schema.NonEmptyString,
  evidence: Schema.optionalWith(
    Schema.Union(
      Schema.Struct({ actual: Schema.Unknown, expected: Schema.Unknown }),
      Schema.Struct({
        artifact: Digest.codecs.content.wire,
        generation: Schema.Int.pipe(Schema.nonNegative()),
        actual: Schema.Unknown,
        expected: Schema.Unknown,
      }),
      Contract.Refusal,
    ),
    { as: "Option" },
  ),
}) {
  static readonly bySeverity: Order.Order<WireFault> = Order.mapInput(Fault.Class.order, (fault: WireFault) => fault.class)
  static readonly dominant = (faults: Array.NonEmptyReadonlyArray<WireFault>): WireFault =>
    Array.max(faults, WireFault.bySeverity)
  get class(): Fault.Class.Kind {
    return _policy.classOf(this.reason)
  }
  get policy(): WireFault.Row {
    return _policy.at(this.reason)
  }
  override get message(): string {
    return `<${this.family}:${this.reason}> ${this.detail}`
  }
}

declare namespace WireFault {
  type Reason = (typeof _policy.reasons)[number]
  type Row = ReturnType<typeof _policy.at>
}

// Capacity is this quarantine's own bound; the ATTEMPT bound is not, so it reads off the one retry owner rather
// than spelling a second ceiling here. `bulk` is the row a poison frame re-drives on — patient, wide-windowed work
// no caller waits on.
const _INTAKE = { capacity: 256, budget: "bulk" } as const satisfies { capacity: number; budget: Fault.Budget.Kind }
// Sweep CADENCE, never a retry curve: `Effect.repeat` runs the drain for the process lifetime, so a `Fault.Budget`
// row's attempt count and window would terminate the sweep instead of pacing it. Jitter is the one thing a fixed
// cadence cannot supply: every replica quarantines the same broken producer at the same moment, so an unjittered
// spacing re-sends the whole poison roster from every replica on one tick and the recovering peer meets a burst.
const _REPLAY: Schedule.Schedule<number> = Schedule.jittered(Schedule.spaced("30 seconds"))

class PoisonFrame extends Schema.Class<PoisonFrame>("PoisonFrame")({
  slot: Schema.BigIntFromSelf,
  family: _faultLiteral,
  octets: Schema.Uint8ArrayFromSelf,
  fault: WireFault,
  at: Schema.DateTimeUtcFromSelf,
  attempts: Schema.Int.pipe(Schema.nonNegative()),
}) {
  get replayable(): boolean {
    return this.fault.policy.replayable && this.attempts < Fault.Budget.at(_INTAKE.budget).attempts
  }
}

const _bySlot: Order.Order<PoisonFrame> = Order.mapInput(Order.bigint, (frame: PoisonFrame) => frame.slot)

const _oldest = (slots: ReadonlyArray<bigint>): Option.Option<bigint> =>
  Array.match(slots, { onEmpty: Option.none, onNonEmpty: (rows) => Option.some(Array.min(rows, Order.bigint)) })

// Arm resolution is total across the WHOLE fault roster: registered families read their registry row and the five
// families other pages decode read `_faultArms`. Totality is what makes rendering unconditional — a partial
// resolution leaves the census holding octets it can print nothing about, which is the one thing the census exists
// to prevent.
const _armOf = (family: Wire.FaultFamily): Format.Arm =>
  _family.is(family) ? _rows[family].arm : _faultArms[family]

class Quarantine extends Effect.Service<Quarantine>()("@rasm/ts/core/Quarantine", {
  scoped: Effect.gen(function* () {
    // ONE structure carries the census. A queue beside the map held the same frames under a second bound, and a
    // BOUNDED queue's offer SUSPENDS when full — parking the decoding fiber the moment nothing drains it, so an
    // ingress running no replay pump stopped rather than degraded, while the map itself grew unbounded beside it.
    // Bounding the map makes intake total and fixes the roster whether a pump runs or not.
    //
    // Retention is bounded in BOTH directions, which is what makes a held frame's lifetime a decision rather than
    // an accident: `intake` starts it and four owners end it — `replayed` on delivery, `replayed` on retirement
    // once attempts exhaust or the cause is unreplayable, a caller through `release`, and this eviction when a
    // newer frame needs the slot. The last owner is what makes the lifetime finite with no pump running at all.
    // Census reads stay process-scoped and tenant-blind by construction: `Wire.Fault` carries a family and an extent,
    // never an `Identity.Tenant`, so a partition here would key on a field the fault does not have.
    const held = yield* STM.commit(TMap.empty<bigint, PoisonFrame>())
    const serial = yield* STM.commit(TRef.make(0n))
    const admit = (frame: PoisonFrame): STM.STM<PoisonFrame> =>
      STM.gen(function* () {
        yield* TMap.set(held, frame.slot, frame)
        const slots = yield* TMap.keys(held)
        // eviction rides the SAME transaction as the insert, so no reader observes an over-capacity census
        if (slots.length > _INTAKE.capacity) {
          yield* Option.match(_oldest(slots), { onNone: () => STM.void, onSome: (slot) => TMap.remove(held, slot) })
        }
        return frame
      })
    const settled = (frame: PoisonFrame): Effect.Effect<void> => STM.commit(TMap.remove(held, frame.slot))
    // Slot order IS arrival order, so the pump works the roster oldest-first with no second structure recording it.
    // On an EMPTY census the pump parks rather than polls: `STM.retry` suspends this transaction until the map
    // changes, so a frame admitted a moment after a sweep is worked at once instead of waiting out the schedule —
    // prompt wake a blocking queue take used to supply, kept without the queue. Suspending here is the inverse
    // of suspending on a full queue: the transaction that waits belongs to the CONSUMER, never to a decoding
    // fiber, so ingress holds its own pace whatever the pump is doing.
    const due: Effect.Effect<ReadonlyArray<PoisonFrame>> =
      STM.commit(STM.flatMap(TMap.isEmpty(held), (empty) =>
        empty ? STM.retry : STM.map(TMap.values(held), (frames) => Array.sort(frames, _bySlot))))
    return {
      intake: (family: Wire.FaultFamily, octets: Uint8Array, fault: WireFault) =>
        Effect.flatMap(DateTime.now, (now) =>
          STM.commit(STM.gen(function* () {
            const slot = yield* TRef.get(serial)
            yield* TRef.set(serial, slot + 1n)
            return yield* admit(new PoisonFrame({ slot, family, octets, fault, at: now, attempts: 0 }))
          }))),
      census: STM.commit(TMap.values(held)),
      // Rows render and the family supplies only what a row cannot know. Branching on two arm names left the
      // nine msgpack and cbor families printing nothing at all — and the positional appearance records are exactly the
      // frames whose slot drift an operator has to see — while the five fault-only families printed nothing
      // either. The descriptor rides in for the one arm that cannot print without it.
      diagnostic: (frame: PoisonFrame): Option.Option<string> =>
        Format.rows.arm[_armOf(frame.family)].render(
          frame.octets,
          Option.map(
            Array.findFirst(Format.proto.names, (name) => name === frame.family),
            (name) => Format.proto.suite[name],
          ),
        ),
      release: (frame: PoisonFrame) => settled(frame),
      replayed: <A, R>(
        decode: (family: Wire.FaultFamily, octets: Uint8Array) => Effect.Effect<A, WireFault, R>,
        delivered: (value: A) => Effect.Effect<void, never, R>,
        retired: (frame: PoisonFrame) => Effect.Effect<void, never, R>,
      ): Effect.Effect<void, never, R> =>
        Effect.flatMap(due, (frames) =>
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
                          ? Effect.asVoid(STM.commit(admit(frame)))
                          : Option.isSome(Cause.failureOption(cause))
                          ? Effect.asVoid(STM.commit(admit(new PoisonFrame({ ...frame, attempts: frame.attempts + 1 }))))
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
    (context: { readonly family: Wire.FaultFamily; readonly octets: () => Uint8Array }): <A, R>(
      self: Effect.Effect<A, WireFault, R>,
    ) => Effect.Effect<Either.Either<A, WireFault>, never, R | Quarantine>
    <A, R>(
      self: Effect.Effect<A, WireFault, R>,
      context: { readonly family: Wire.FaultFamily; readonly octets: () => Uint8Array },
    ): Effect.Effect<Either.Either<A, WireFault>, never, R | Quarantine>
  } = Function.dual(
    2,
    <A, R>(
      self: Effect.Effect<A, WireFault, R>,
      context: { readonly family: Wire.FaultFamily; readonly octets: () => Uint8Array },
    ): Effect.Effect<Either.Either<A, WireFault>, never, R | Quarantine> =>
      self.pipe(
        Effect.map((value): Either.Either<A, WireFault> => Either.right(value)),
        // Every fault lands as a VALUE. `held` decides octet retention alone, never whether the caller survives:
        // re-failing a non-held fault ended the entire feed on one refused frame, and a sequence gap, a stale
        // arrival, and a budget overrun are all per-frame verdicts a following frame recovers from. The error
        // channel narrows to `never`, so the surviving stream error is the ingress iterator's alone.
        Effect.catchAll((fault) =>
          fault.policy.held
            ? Effect.as(Quarantine.intake(context.family, context.octets(), fault), Either.left(fault))
            : Effect.succeed(Either.left(fault))),
      ),
  )
}
```

## [04]-[PARITY_VERIFY]

- Owner: `Wire.Parity` verifies content identity, semantic round trips, and frozen fixture bytes.
- Law: protobuf parity is semantic; exact bytes apply only to frozen fixtures.

```typescript signature
import { Digest } from "../value/contentKey.ts"
import { Contract } from "./contract.ts"

const _mismatch = (family: Wire.FaultFamily, actual: unknown, expected: unknown, detail: string): WireFault =>
  new WireFault({ family, reason: "parity", detail, evidence: Option.some({ actual, expected }) })

const Parity = {
  key: (payload: Digest.Payload): Effect.Effect<Digest.Key<"content">> => Digest.mint("content", payload),
  matched: (
    family: Wire.FaultFamily,
    actual: Digest.Key<"content">,
    expected: Digest.Key<"content">,
  ): Effect.Effect<void, WireFault> =>
    actual === expected ? Effect.void : Effect.fail(_mismatch(family, actual, expected, "<key-mismatch>")),
  verified: (
    family: Wire.FaultFamily,
    expected: Digest.Key<"content">,
    payload: Digest.Payload,
  ): Effect.Effect<void, WireFault> =>
    Effect.flatMap(Digest.mint("content", payload), (minted) => Parity.matched(family, minted, expected)),
  roundtrip: <A>(
    family: Wire.FaultFamily,
    schema: Schema.Schema<A, Uint8Array>,
    octets: Uint8Array,
  ): Effect.Effect<void, ParseResult.ParseError | WireFault> =>
    Effect.gen(function* () {
      const decoded = yield* Schema.decodeUnknown(schema)(octets)
      const emitted = yield* Schema.encode(schema)(decoded)
      const mismatch = emitted.findIndex((byte, index) => byte !== octets[index])
      const offset = mismatch === -1 ? Math.min(emitted.length, octets.length) : mismatch
      if (mismatch === -1 && emitted.length === octets.length) return
      return yield* Effect.fail(_mismatch(
        family,
        { extent: emitted.length, offset, byte: emitted[offset] },
        { extent: octets.length, offset, byte: octets[offset] },
        "<golden-byte-divergence>",
      ))
    }),
} as const
```

## [05]-[LANDING_EVIDENCE]

- Owner: landed core values reuse their canonical owners without local twins.
- Owner: `CrdtOp` closes the producer's op union at ten arms — `Set`, `Write`, `Add`, `Remove`, `Increment`, `InsertAfter`, `Delete`, `Maintain`, `Beat`, `Leave` — each leading with the `Field` slot the producer keys first.
- Law: every arm frames FLAT — one MessagePack array whose slot 0 carries the integer union tag and whose remaining slots carry the producer's `[Key(n)]` roster in declaration order. `_op` owns that framing once, so no arm drifts its tag position or its slot order.
- Law: the union admits no passthrough arm, so a foreign tag refuses at decode rather than crossing the merge algebra as opaque bytes.
- Law: evidence render arms retain encoded `frameHash`, optional `drawHash`, and optional canonical pixel identity.

```typescript signature
import { Clock } from "../value/clock.ts"
import { Identity } from "../value/identity.ts"
import { Shape } from "../value/schema.ts"
import { Causal } from "../state/causal.ts"
import { Commit } from "../state/commit.ts"
import { Evidence } from "../state/evidence.ts"
import { Board } from "../observe/board.ts"

// `_op` is the ONE place the producer's framing lives: a positional tuple on the encoded side, a tagged struct on the
// interior side, and the slot list carrying both the order and the names. A `Schema.TaggedStruct` alone encodes a map
// keyed by `_tag` and refuses the producer's first byte, while a per-arm hand transform lets one arm drift its tag
// position off the other nine.
type _Pairs = ReadonlyArray<readonly [string, Schema.Schema.Any]>
type _Slots<S extends _Pairs> = { readonly [I in keyof S]: S[I][1] }
type _Named<S extends _Pairs> = { readonly [E in S[number] as E[0]]: E[1] }

const _op = <const T extends number, const N extends string, const S extends _Pairs>(tag: T, name: N, slots: S) =>
  Schema.transform(
    Schema.Tuple(Schema.Literal(tag), ...(slots.map(([, slot]) => slot) as unknown as _Slots<S>)),
    Schema.TaggedStruct(name, Object.fromEntries(slots) as _Named<S> & Schema.Struct.Fields),
    {
      strict: false,
      decode: (wire) => ({ _tag: name, ...Object.fromEntries(slots.map(([key], index) => [key, wire[index + 1]])) }),
      encode: (op) => [tag, ...slots.map(([key]) => op[key])],
    },
  )

// Producer slot domains: a `Guid`, a `UInt128` element id, and a payload all cross as raw octets; every tick count and
// 64-bit half crosses as a bigint under the decoder's i64 fidelity; every `(origin, counter)` roster as a pair array.
const _text = Schema.String
const _octets = Schema.Uint8ArrayFromSelf
const _u64 = Schema.BigIntFromSelf
const _dots = Schema.Array(Schema.Tuple(_octets, _u64))

const _Set = _op(0, "Set", [["field", _text], ["value", _octets], ["physicalTicks", _u64], ["logical", _u64], ["origin", _octets]])
const _Write = _op(1, "Write", [
  ["field", _text], ["value", _octets], ["context", _dots], ["physicalTicks", _u64], ["logical", _u64], ["origin", _octets],
])
const _Add = _op(2, "Add", [["field", _text], ["element", _octets], ["tagOrigin", _octets], ["tagLogical", _u64]])
const _Remove = _op(3, "Remove", [["field", _text], ["element", _octets], ["observedTags", _dots]])
const _Increment = _op(4, "Increment", [["field", _text], ["origin", _octets], ["sequence", _u64], ["positive", _u64], ["negative", _u64]])
const _InsertAfter = _op(5, "InsertAfter", [
  ["field", _text], ["predOrigin", _octets], ["predLogical", _u64], ["idOrigin", _octets], ["idLogical", _u64], ["value", _octets],
])
const _Delete = _op(6, "Delete", [["field", _text], ["idOrigin", _octets], ["idLogical", _u64]])
const _Maintain = _op(7, "Maintain", [["field", _text], ["quiescent", _dots], ["livenessTicks", _u64]])
const _Beat = _op(8, "Beat", [["field", _text], ["origin", _octets], ["state", _octets], ["physicalTicks", _u64], ["logical", _u64]])
const _Leave = _op(9, "Leave", [["field", _text], ["origin", _octets], ["physicalTicks", _u64], ["logical", _u64]])

const CrdtOp: Schema.Union<[
  typeof _Set, typeof _Write, typeof _Add, typeof _Remove, typeof _Increment,
  typeof _InsertAfter, typeof _Delete, typeof _Maintain, typeof _Beat, typeof _Leave,
]> = Schema.Union(_Set, _Write, _Add, _Remove, _Increment, _InsertAfter, _Delete, _Maintain, _Beat, _Leave)
type CrdtOp = typeof CrdtOp.Type

// Stamped arms carry their two halves as the producer's own slots, so the 16-byte cell is a derived view over the
// canonical owner rather than a local twin: the producer domain is single-mint and each half already landed inside its
// unsigned 64-bit slot, which is exactly the interior lift the clock owner's tick law licenses.
const _hlc = Schema.decodeSync(Clock.Hlc)
const _stamped = (op: Extract<CrdtOp, { readonly physicalTicks: bigint }>): Clock.Hlc =>
  _hlc({ physical: op.physicalTicks, logical: op.logical })
```

## [06]-[LANDING_WIRE]

- Owner: `Wire` lands producer-exact domain values for graph, edits, BCF, model diff, appearance, and support evidence.
- Law: the producer's `NodeId` and its `ContentAddress` — the C# bare-digest brand this branch spells `ContentKey`, never C#'s own composite of that name — land as distinct brands, and `Node` retains the producer-carried authoritative content address.
- Law: entity edits apply a closed JSON Patch document to exact `NodeWire` ProtoJSON under per-node base-address OCC.
- Boundary: raw GeoJSON text and CloudEvents remain outside the registry because no typed family crosses.

```typescript signature
import { VariantSchema } from "@effect/experimental"
import { Context, Layer } from "effect"

const _reasons = [
  "canceled", "unknown", "invalid", "deadline", "notfound", "exists", "denied", "exhausted",
  "precondition", "aborted", "range", "unimplemented", "internal", "unavailable", "dataloss", "unauthenticated",
] as const

// Transport families mint through the same seam every folder family takes: `class` is the branch taxonomy, and
// `code`/`retryable`/`terminal` are the gRPC peer's OWN columns adopted verbatim — the wire's retryability diverges
// from its class default where the protocol says so (an already-exists refusal never succeeds on a re-send), so
// these are peer facts the landing carries, never a second taxonomy this branch mints.
const _hopRows = {
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
const _hopVocabulary = Shape.vocabulary(_reasons, _hopRows)
const _hops = Fault.Class.family(_reasons, Record.map(_hopRows, (row) => ({ class: row.class })))

declare namespace Hops {
  type Reason = (typeof _hops.reasons)[number]
  type Row = (typeof _hopRows)[Reason]
  type Shape = {
    readonly reasons: typeof _reasons
    readonly wire: typeof _hops.schema
    readonly is: (input: unknown) => input is Reason
    readonly at: (reason: Reason) => Row
    readonly fromCode: (code: number) => Reason
  }
}

const _byCode: HashMap.HashMap<number, Hops.Reason> = Array.reduce(
  _reasons,
  HashMap.empty<number, Hops.Reason>(),
  (acc, reason) => HashMap.set(acc, _hopVocabulary.at(reason).code, reason),
)

const Hops: Hops.Shape = {
  reasons: _reasons,
  wire: _hops.schema,
  is: _hopVocabulary.is,
  at: _hopVocabulary.at,
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
  static readonly EnricherLive: Layer.Layer<Fault.Enricher> = Layer.succeed(
    Fault.Enricher,
    Fault.Enricher.of({
      enrich: (capture) =>
        Effect.succeed(
          Option.match(
            Option.filter(
              Option.fromNullable(capture.attributes[_WIRE_ATTR.reason]),
              Hops.is,
            ),
            {
              onNone: () => capture,
              onSome: (reason) =>
                capture.enriched({
                  [_WIRE_ATTR.reason]: reason,
                  [_WIRE_ATTR.retryable]: Hops.at(reason).retryable,
                  [_WIRE_ATTR.terminal]: Hops.at(reason).terminal,
                }),
            },
          ),
        ),
    }),
  )
  get class(): Fault.Class.Kind {
    return _hops.classOf(this.reason)
  }
  get retryable(): boolean {
    return Hops.at(this.reason).retryable
  }
  get terminal(): boolean {
    return Hops.at(this.reason).terminal
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

const _PixelIdentity = Schema.Struct({
  version: Schema.Literal("rgba8-srgb-straight-top-left-v1"),
  width: Shape.Refined.OrdinalKey.pipe(Schema.positive()),
  height: Shape.Refined.OrdinalKey.pipe(Schema.positive()),
  hash: Digest.codecs.content.wire,
})
const _EvidenceReceipt = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("surface"), host: Schema.String, descriptor: Schema.String, scale: Schema.Number, at: Schema.DateTimeUtc, correlation: Schema.String, handle: Schema.optional(Schema.String) }),
  Schema.Struct({ kind: Schema.Literal("focus"), target: Schema.String, focused: Schema.Boolean }),
  Schema.Struct({ kind: Schema.Literal("render"), slot: Schema.String, format: Schema.String, frameHash: Schema.String, drawHash: Schema.optional(Schema.String), pixels: Schema.optional(_PixelIdentity), bytes: Schema.String.pipe(Schema.pattern(/^\d+$/)), elapsed: Schema.String, colorSpace: Schema.String, destination: Schema.optional(Schema.String) }),
  Schema.Struct({ kind: Schema.Literal("disposal"), screenId: Schema.String, active: Schema.String, disposables: Schema.Int }),
  Schema.Struct({ kind: Schema.Literal("edit"), slot: Schema.String, surface: Schema.String, target: Schema.String, editor: Schema.String, outcome: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("command"), receipt: Schema.Unknown }),
  Schema.Struct({ kind: Schema.Literal("native-asset"), fact: Schema.Unknown }),
  Schema.Struct({ kind: Schema.Literal("theme"), variant: Schema.String, density: Schema.String, trigger: Schema.String, changedKeys: Schema.Int }),
  Schema.Struct({ kind: Schema.Literal("motion"), token: Schema.String, resolved: Schema.String, reduced: Schema.Boolean }),
  Schema.Struct({ kind: Schema.Literal("effect"), plane: Schema.String, key: Schema.String, outcome: Schema.String, flag: Schema.Boolean, count: Schema.Int, magnitude: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("asset"), key: Schema.String, assetKind: Schema.String, origin: Schema.String, scale: Schema.Number, contentHash: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("live-data"), slot: Schema.String, adds: Schema.Int, updates: Schema.Int, removes: Schema.Int, refreshes: Schema.Int }),
  Schema.Struct({ kind: Schema.Literal("collab-sync"), docKey: Schema.String, deltas: Schema.Int, bytes: Schema.String, pending: Schema.Int, applied: Schema.Boolean }),
  Schema.Struct({ kind: Schema.Literal("collab-revert"), docKey: Schema.String, frontierDigest: Schema.String, inverseOps: Schema.Int }),
  Schema.Struct({ kind: Schema.Literal("media"), key: Schema.String, codec: Schema.String, source: Schema.String, outcome: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("quality"), tier: Schema.String, pathTraceSamples: Schema.Int, watermarkFactor: Schema.Number, motion: Schema.String, foveationLevel: Schema.Int, refreshHz: Schema.Number }),
  Schema.Struct({ kind: Schema.Literal("gpu-frame"), frameOrdinal: Schema.String, passes: Schema.Int, unmeasured: Schema.Int, measuredNanoseconds: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("layout"), panel: Schema.String, constraints: Schema.Int, elapsed: Schema.String, fault: Schema.optional(Schema.String) }),
  Schema.Struct({ kind: Schema.Literal("dispatcher-lag"), boundary: Schema.String, elapsed: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("collab-precommit"), docKey: Schema.String, lamport: Schema.Int, ops: Schema.Int, origin: Schema.String, message: Schema.optional(Schema.String) }),
)
const _RasmPackage = Schema.Literal(
  "rasm.kernel", "Rasm.Element", "Rasm.AppHost", "Rasm.Materials", "Rasm.Bim", "Rasm.Fabrication",
  "Rasm.Persistence", "Rasm.Compute", "Rasm.Generation", "Rasm.AppUi", "Rasm.Rhino", "Rasm.Grasshopper",
)
const _JsonHlcStamp = Schema.Struct({ physical: Schema.String, logical: Schema.Int.pipe(Schema.nonNegative()), skewBound: Schema.String })
const _TenantContextWire = Schema.Struct({ tenantId: Schema.String.pipe(Schema.pattern(/^\d+$/)), slug: Schema.NonEmptyString })

const _receiptEnvelope = <A, I>(payload: Schema.Schema<A, I>) => Schema.Struct({
  ..._JsonHlcStamp.fields,
  correlation: Schema.NonEmptyString,
  tenant: _TenantContextWire,
  package: _RasmPackage,
  kind: Schema.NonEmptyString,
  payload,
})
const _SkewBand = Schema.Struct({ earliest: Schema.DateTimeUtc, latest: Schema.DateTimeUtc })

const _EvidenceRow = Schema.Struct({
  ordinal: Schema.Int.pipe(Schema.nonNegative()),
  uncertaintyGroup: Schema.Int.pipe(Schema.nonNegative()),
  envelope: _receiptEnvelope(_EvidenceReceipt),
  band: _SkewBand,
})

class EvidenceTimeline extends Schema.Class<EvidenceTimeline>("EvidenceTimeline")({
  correlation: Schema.NonEmptyString,
  rows: Schema.Array(_EvidenceRow),
}) {
  static readonly Receipt: typeof _EvidenceReceipt = _EvidenceReceipt
  static readonly Pixel: typeof _PixelIdentity = _PixelIdentity
}

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
  stamp: Clock.Hlc,
}) {
  static readonly FromWire: Schema.Schema<WriteReceipt, unknown> = Schema.compose(_stamp("WriteReceipt"), WriteReceipt, { strict: false })
}
// Palette gating reuses `Evidence.Availability`'s level vocabulary without sharing its document.
class CommandGate extends Schema.TaggedClass<CommandGate>()("CommandGate", {
  key: Schema.NonEmptyString,
  available: Schema.Boolean,
  level: Evidence.Availability.fields.level,
}) {
  static readonly FromWire: Schema.Schema<CommandGate, unknown> = Schema.compose(_stamp("CommandGate"), CommandGate, { strict: false })
}

const _Vec3 = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number)

// AppUi's shell publishes this widget vocabulary: twenty-nine locked kind literals, each arm carrying its typed shape beside the
// one `IntentBinding` carrier. The producer SHIPS the discriminant, so this landing decodes on the `kind`
// column the wire already carries and mints no second tag. Key-grade columns take `NonEmptyString` because an
// empty key resolves against no label catalog, command registry, or automation id on either head; display text
// takes `String`, since the producer's own text columns admit it.
const _Emphasis = Schema.Literal("quiet", "secondary", "primary", "danger", "inverted", "link")
const _Orientation = Schema.Literal("Horizontal", "Vertical")
// picker modality is the shipped `UsePickerTypes` roster whole, so a fourth posture breaks here
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

// Integral, unsigned, and precise arms cross as ORDINAL DECIMAL STRINGS because a sixty-four-bit bound and a
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

// pattern lists land non-empty because the producer's own filter encoder refuses an empty one before the
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

// Leaf arms close the family: twenty shapes bottom out, so both representations DERIVE from the union
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

// Nesting spells ONCE and instantiates per representation: the child type is the only axis that
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

const _BcfVec3 = Schema.Struct({ x: Schema.Number, y: Schema.Number, z: Schema.Number })
const _BcfCamera = Schema.Struct({
  kind: Schema.Literal("perspective", "orthogonal"),
  position: _BcfVec3,
  direction: _BcfVec3,
  up: _BcfVec3,
  fieldOfView: Schema.Number,
  viewToWorldScale: Schema.Number,
  aspectRatio: Schema.Number,
})
const _BcfColoring = Schema.Struct({ color: Schema.NonEmptyString, globalIds: Schema.Array(_GlobalId) })
const _BcfLine = Schema.Struct({ start: _BcfVec3, end: _BcfVec3 })
const _BcfClipping = Schema.Struct({ location: _BcfVec3, direction: _BcfVec3 })
const _BcfBitmap = Schema.Struct({
  format: Schema.NonEmptyString,
  reference: Schema.NonEmptyString,
  location: _BcfVec3,
  normal: _BcfVec3,
  up: _BcfVec3,
  height: Schema.Number.pipe(Schema.positive()),
})
const _BcfHints = Schema.Struct({ spacesVisible: Schema.Boolean, spaceBoundariesVisible: Schema.Boolean, openingsVisible: Schema.Boolean })

class BcfTopic extends Schema.Class<BcfTopic>("BcfTopic")({
  guid: Schema.NonEmptyString,
  title: Schema.NonEmptyString,
  status: Schema.Literal("open", "in-progress", "resolved", "closed", "reopened"),
  topicType: Schema.String,
  priority: Schema.String,
  author: Schema.String,
  creationDate: Schema.DateTimeUtc,
  comments: Schema.Array(Schema.Struct({
    guid: Schema.NonEmptyString,
    author: Schema.String,
    text: Schema.String,
    viewpointGuid: Schema.optionalWith(Schema.String, { as: "Option", nullable: true }),
    date: Schema.DateTimeUtc,
    modifiedDate: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option", nullable: true }),
    modifiedAuthor: Schema.String,
    replyToGuid: Schema.optionalWith(Schema.String, { as: "Option", nullable: true }),
  })),
  viewpoints: Schema.Array(Schema.suspend((): typeof BcfViewpoint => BcfViewpoint)),
  description: Schema.String,
  assignedTo: Schema.String,
  stage: Schema.String,
  dueDate: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option", nullable: true }),
  labels: Schema.Array(Schema.String),
  index: Schema.optionalWith(Schema.Int, { as: "Option", nullable: true }),
  modifiedDate: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option", nullable: true }),
  modifiedAuthor: Schema.String,
  serverAssignedId: Schema.String,
  referenceLinks: Schema.Array(Schema.String),
  relatedTopics: Schema.Array(Schema.String),
  documentReferences: Schema.Array(Schema.Struct({ guid: Schema.String, documentGuid: Schema.optionalWith(Schema.String, { as: "Option", nullable: true }), url: Schema.optionalWith(Schema.String, { as: "Option", nullable: true }), description: Schema.String })),
  bimSnippet: Schema.optionalWith(Schema.Struct({ snippetType: Schema.String, reference: Schema.String, referenceSchema: Schema.String, isExternal: Schema.Boolean }), { as: "Option", nullable: true }),
  files: Schema.Array(Schema.Struct({ filename: Schema.String, date: Schema.optionalWith(Schema.DateTimeUtc, { as: "Option", nullable: true }), reference: Schema.String, ifcProject: Schema.String, ifcSpatialStructureElement: Schema.String, isExternal: Schema.Boolean })),
  statusLabel: Schema.String,
}) {}

class BcfViewpoint extends Schema.Class<BcfViewpoint>("BcfViewpoint")({
  guid: Schema.NonEmptyString,
  camera: Schema.optionalWith(_BcfCamera, { as: "Option", nullable: true }),
  selectedGlobalIds: Schema.Array(_GlobalId),
  visibilityExceptions: Schema.Array(_GlobalId),
  defaultVisibility: Schema.Boolean,
  coloring: Schema.Array(_BcfColoring),
  lines: Schema.Array(_BcfLine),
  clippingPlanes: Schema.Array(_BcfClipping),
  bitmaps: Schema.Array(_BcfBitmap),
  index: Schema.optionalWith(Schema.Int, { as: "Option", nullable: true }),
  viewSetupHints: Schema.optionalWith(_BcfHints, { as: "Option", nullable: true }),
}) {
  static readonly GlobalId: typeof _GlobalId = _GlobalId
}

const _ContentAddress = Digest.codecs.content.wire
const _NodeId = Schema.String.pipe(Schema.pattern(/^[0-9A-F]{32}$/), Schema.brand("NodeId"))
const _Classification = Schema.Struct({
  system: Schema.NonEmptyString,
  code: Schema.NonEmptyString,
  edition: Schema.String,
  source: Schema.optionalWith(Schema.String, { as: "Option", nullable: true }),
  editionDate: Schema.optionalWith(Schema.String, { as: "Option", nullable: true }),
  title: Schema.optionalWith(Schema.String, { as: "Option", nullable: true }),
})
const _Pose = Schema.Struct({
  locationX: Schema.Number, locationY: Schema.Number, locationZ: Schema.Number,
  axisX: Schema.Number, axisY: Schema.Number, axisZ: Schema.Number,
  refDirectionX: Schema.Number, refDirectionY: Schema.Number, refDirectionZ: Schema.Number,
})
const _DeltaMeasure = Schema.Struct({
  si: Schema.Number,
  type: Schema.NonEmptyString,
  dimension: Schema.Tuple(Schema.Int, Schema.Int, Schema.Int, Schema.Int, Schema.Int, Schema.Int, Schema.Int),
})
const _DeltaValue = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("measure"), value: _DeltaMeasure }),
  Schema.Struct({ kind: Schema.Literal("address"), value: _ContentAddress }),
  Schema.Struct({ kind: Schema.Literal("label"), value: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("absent") }),
)
const _AspectDelta = Schema.Struct({
  path: Schema.String,
  shape: Schema.Literal("replace", "index", "key", "added", "removed", "unknown"),
  before: _DeltaValue,
  after: _DeltaValue,
})
const _Counterparts = Schema.NonEmptyArray(_GlobalId).pipe(
  Schema.filter((ids) => ids.length > 1 && ids.every((id, at) => at === 0 || ids[at - 1]! < id) || "<counterparts-unordered>"),
)
const _ElementChange = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("Added"), globalId: _GlobalId, class: _Classification, predefined: Schema.NonEmptyString, content: _ContentAddress }),
  Schema.Struct({ kind: Schema.Literal("Removed"), globalId: _GlobalId, class: _Classification, predefined: Schema.NonEmptyString, content: _ContentAddress }),
  Schema.Struct({ kind: Schema.Literal("Modified"), globalId: _GlobalId, baselineContent: _ContentAddress, revisionContent: _ContentAddress, baselinePlacement: _ContentAddress, revisionPlacement: _ContentAddress, deltas: Schema.Array(_AspectDelta) }),
  Schema.Struct({ kind: Schema.Literal("Moved"), globalId: _GlobalId, baselinePlacement: _ContentAddress, revisionPlacement: _ContentAddress, baselinePose: Schema.optionalWith(_Pose, { as: "Option", nullable: true }), revisionPose: Schema.optionalWith(_Pose, { as: "Option", nullable: true }) }),
  Schema.Struct({ kind: Schema.Literal("Split"), globalId: _GlobalId, content: _ContentAddress, into: _Counterparts }),
  Schema.Struct({ kind: Schema.Literal("Merged"), globalId: _GlobalId, content: _ContentAddress, from: _Counterparts }),
)

class ModelDiff extends Schema.Class<ModelDiff>("ModelDiff")({
  baseline: _ContentAddress,
  revision: _ContentAddress,
  changes: Schema.Array(_ElementChange),
  unchangedCount: Schema.Int.pipe(Schema.nonNegative()),
}) {
  static readonly Change: typeof _ElementChange = _ElementChange
}

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

// `MaterialWire` lands its receipt verbatim — capture evidence, fit conditioning, chromaticity/CCT grounding, the
// chart-solve tail separating a colour-corrected capture from a camera's guess, and model attribution a neural
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
  // Chart solving closes the tail: `calibrated` separates a chart-corrected capture from a camera's guess and
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
// `GeometryThinWalled` last at 30, both APPENDED past the frozen block, so pre-append bytes decode unchanged when the
// missing trailing slot folds to the producer's stated default (rotation 0; thinWalled false, the OpenPBR
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
  // Wire twins ride the owner: position moves to name in the reshape arm, every refinement re-proves here.
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
  // Photometric grounding records what the producer's admission read: an ABSENT unit spells an authored emission whose
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
// `AppearanceWire` is the typed `rasm.element.v1` payload nested at `NodeWire` field 7.
// Its byte and text identities both land on `Digest.Key<"content">` through the owner's codecs.
class AppearanceSummary extends Schema.Class<AppearanceSummary>("AppearanceSummary")({
  appearanceKey: Digest.codecs.content.bytes,
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

// Three physical units ride the roster's own channels; every other channel is a dimensionless ratio, an index,
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

// Channel facts carry the roster's five wire-bearing columns and NOTHING derived from them: `ch` the semantic
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
  // scalar anisotropy-direction planes mip correctly under box where a tangent vector plane cancels
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
//
// Widths run 1, 2, and 4 because those ARE the uncompressed texel widths a GPU accepts: every three-component
// entry in the browser format list is block-compressed or a packed thirty-two-bit texel, so no target API admits
// plain twenty-four-bit color. Three channels are refused on the STORE's own ground and not for want of a tool —
// `ktx create --format` admits `R8G8B8_UNORM` and `R8G8B8_SRGB`, and this branch already spells `rgb8` where it
// is legal, as a `ktx transcode` target on the asset plane. Opaque derivatives take the quarter they save at that
// egress boundary; seating one here would mint a store no consumer can upload, and would fork a frozen fragment
// whose C# and python mirrors carry no three-component row either.
const _planeFormats = ["r8", "r16", "r16f", "r32f", "rg8", "rg16", "rg16f", "rg32f", "rgba8", "rgba16", "rgba16f", "rgba32f"] as const
const _planeRows = {
  r8: { depth: "u8", width: 1, web: true }, r16: { depth: "u16", width: 1, web: false },
  r16f: { depth: "f16", width: 1, web: true }, r32f: { depth: "f32", width: 1, web: true },
  rg8: { depth: "u8", width: 2, web: true }, rg16: { depth: "u16", width: 2, web: false },
  // float two-component stores carry Vulkan format rows, so rg16f re-routes the direction planes rg16 cannot serve
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
// alone writes still rides it, refused by roster membership rather than by an unknown key — beside the three columns the
// wire laws read: `alpha` the canonical association encode converts to (the jxl/avif rows are the measured
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
// This axis counts STACKED planes alone, so no `1d` row seats here: base-level dimensionality rides width and height,
// orthogonal to stacking at the encoder itself — `ktx create --1d --layers 4` mints a four-layer 1D array, one shape
// fusing the two axes has no name for. Reading gates refuse a 1D container on its own extent evidence, `pixelHeight`
// reported ZERO, because every extent owner across the branches floors an axis at one.
// `gltf` is whether a 2D bind reaches the law at all, exactly as `_packRows.gltf` is for slot order — glTF carries
// plain 2D textures alone, so `none` is the one seatable row and a viewer seating raises over this column rather
// than over a member name, which is the same refusal a roster rename silently inverts.
const _layerLaws = ["none", "cubeFaces", "array", "volume", "frames"] as const
const _layerRows = {
  none: { extent: 1, gltf: true }, cubeFaces: { extent: 6, gltf: false },
  array: { extent: null, gltf: false }, volume: { extent: null, gltf: false }, frames: { extent: null, gltf: false },
} as const satisfies { readonly [K in Texture.LayerLaw]: { readonly extent: number | null; readonly gltf: boolean } }
// `_packRows` fixes each packing order in slot order beside the ONE legality column the fragment states: `slots` is the
// roster `present` indexes, so a packed channel is addressed by its pack row and the roster names which
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
// own tuple, PLUS this page's own wire-legality column tables on `rows` — channel, container, depth, layer, plane, pack —
// because the refusals those columns declare (`_planeRows.web`, `_packRows.gltf`, `_layerRows.gltf`, the channel
// neutrals) are the ui bind's to RAISE, and a consumer raising over a column must be able to read it. FOREIGN columns
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
      readonly layer: typeof _layerRows
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
  rows: { channel: _channelRows, container: _containerRows, depth: _depthRows, layer: _layerRows, plane: _planeRows, pack: _packRows },
}

// ONE address triple per stored plane FILE. The two producers differ in exactly one field-level fact — the C#
// document names the address `blob` on the X32 spelling, the python document names it `digest` on the lowercase
// brand — so the triple is ONE variant declaration whose address column carries both encodings and whose `file` and
// `byteLength` are shared; each document's schema is an `extract`, and a third producer is one variant key. Two
// parallel struct declarations beside a schema-parameterized factory is the hand-rolled shape this deletes.
// Every addressed plane is a LEVEL-ORDERED list of these triples — entry 0 the base level — and `_leveled` generates the
// length law off the container's `pyramid` column: a self-pyramiding container holds ONE entry whatever `mips`
// declares, every other container one entry per level. A scalar address beside a `mips` count is the
// undigested-pyramid shape the list replaces.
const _producer = VariantSchema.make({ variants: ["web", "proto"], defaultVariant: "web" })

const _PlaneRef = _producer.Struct({
  file: Schema.NonEmptyString, // the egress leaf relative to the set directory; the served-asset join consumes it verbatim
  address: _producer.Field({ web: Digest.codecs.content.wire, proto: Digest.Key.content }).pipe(
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

// Python's producer emits TWO entries for one role under CompanionPolicy.RENDER — the primary plus a sampled
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
// same two-way close every other subset takes. Both documents' pack rows are otherwise one shape whose only axis is the
// producer's address spelling — which is why the row is the same variant declaration the triple is — and the row
// carries NO mip-policy column by design: each slot mips under its own channel's roster fold, so one policy across a
// pack is the defect a policy column would invite.
type _PackFormat = {
  readonly [K in Texture.PlaneFormat]: (typeof _planeRows)[K]["width"] extends 4 ? K : never
}[Texture.PlaneFormat]
const _packFormats = ["rgba8", "rgba16", "rgba16f", "rgba32f"] as const
type _PackWidened<K extends _PackFormat = (typeof _packFormats)[number]> = K
type _PackClosed<K extends (typeof _packFormats)[number] = _PackFormat> = K

// Level lists take ONE filter across both documents' pack rows, applied after `extract` because a variant
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
  planKey: Digest.codecs.content.wire,
  graphKey: Digest.codecs.content.wire,
  seed: Schema.BigIntFromSelf, // the splitmix64 seed replaying the per-texel jitter
  texels: Schema.BigIntFromSelf,
  elapsedMs: Schema.Number.pipe(Schema.nonNegative()),
  gpuDeltaMax: Schema.optionalWith(Schema.Number, { as: "Option" }), // absent until a parity run measures it; telemetry, never a key input
  // Pressing reports two quality tallies at wire grain: `downgraded` COUNTS the channels whose paired mip policy fell to the
  // box floor and `faultedTexels` SUMS the neutral-filled texels across every channel, so a set that pressed
  // clean and one that degraded per plane read apart on the analytics plane rather than on `elapsedMs`.
  downgraded: Schema.Int.pipe(Schema.nonNegative()),
  faultedTexels: Schema.BigIntFromSelf,
})

class TextureSet extends Schema.Class<TextureSet>("TextureSet")(Schema.Struct({
  appearanceKey: Digest.codecs.content.wire, // the seam key this set hangs BEHIND, never a column of it
  setKey: Digest.codecs.content.wire, // streaming fold over the channel-ordered plane digests, seed zero
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
  manifestKey: Digest.Key.content, // merkle fold over the roster-ordered plane digests; the lowercase python spelling lands the brand directly
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

// Generated oneof wrappers read `{ case, value }`, where an UNSET oneof carries no case at all. `_caseOf` reads it as
// presence, so an unset case yields none, the message falls through unlifted, no arm matches it, and the union's own
// refusal is the answer — the producer's `<wire-*-none>` rail one runtime over.
const _caseOf = (raw: unknown): Option.Option<{ readonly case: string; readonly value: Record<string, unknown> }> =>
  Predicate.isRecord(raw) && Predicate.isString(raw.case) && Predicate.isRecord(raw.value)
    ? Option.some({ case: raw.case, value: raw.value })
    : Option.none()

// Oneof lifting sits beside `_stamp`: a protobuf arm ships its DISCRIMINANT as the case name, so `kind` derives here and the
// landing mints nothing the producer's `.proto` never declared. `seat` is the one policy column — `hoist` spreads the
// arm's own columns beside `kind` (a message whose oneof IS its whole content), `keep` leaves the case value whole
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

// Projected frames land at `GeoReferenceWire` field 11: the authority name beside the optional EPSG code, the WKT
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

// Survey frames land at `HeaderWire` field 3 — the map-conversion origin, the X-axis abscissa/ordinate pair, the three
// scale columns, and the datum tokens. A blank `verticalDatum` with no `verticalEpsg` IS the absent vertical frame, the
// producer's own reading, so no second absence spelling lands beside it.
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

// STEP file headers land at `HeaderWire` field 6. Its `authors` and `organizations` rosters are the producer's own
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

// Crossings head at `ElementGraphWire` field 1 and `GraphDeltaWire` field 6. `tolerance` lands as the producer
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

// Uncertainty bands land at `MeasureValueWire` field 10 — the interval in SI beside the producer's own kind token, with the
// standard deviation and coverage factor a stated statistical band carries and a bare interval does not.
class MeasureBand extends Schema.Class<MeasureBand>("MeasureBand")({
  kind: Schema.NonEmptyString,
  lowerSi: Schema.Number,
  upperSi: Schema.Number,
  standardDeviationSi: Schema.optionalWith(Schema.Number, { as: "Option" }),
  coverageFactor: Schema.optionalWith(Schema.Number, { as: "Option" }),
}) {}

// SI-coerced identity columns are what the producer hashes: the quantity token, the SI magnitude, and the seven base
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

// Pose frames land at `ObjectWire` field 12: the producer's `PlacementTransform` flattened to its nine ordered
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
  id: _NodeId,
  contentAddress: _ContentAddress,
  kind: Schema.Literal(
    "object", "material", "propertySet", "quantitySet", "assessment", "appearance", "coverage", "observation",
  ),
  payload: Schema.Record({ key: Schema.String, value: Schema.Unknown }),
}) {
  static readonly FromWire: Schema.Schema<Node, unknown> = Schema.compose(_cased("payload", "keep"), Node, { strict: false })
  static readonly Json: Schema.Schema<Node, Shape.Json> = Schema.suspend(() => _NodeJson)
}

const _NodeJson: Schema.Schema<Node, Shape.Json> = Schema.transformOrFail(Shape.Json, Node.FromWire, {
  strict: true,
  decode: (json, _options, ast) => Either.try({
    try: () => fromJson(Format.proto.suite.NodeWire, json),
    catch: () => new ParseResult.Type(ast, json, "<node-json>"),
  }),
  encode: (wire, _options, ast) => Either.try({
    try: () => toJson(
      Format.proto.suite.NodeWire,
      wire as MessageShape<typeof Format.proto.suite.NodeWire>,
    ),
    catch: () => new ParseResult.Type(ast, wire, "<node-json>"),
  }),
})

const EntityEditWire = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("tombstone"), key: _NodeId, base: _ContentAddress }),
  Schema.Struct({
    kind: Schema.Literal("members"),
    key: _NodeId,
    base: _ContentAddress,
    patch: Format.Patch.Document,
  }),
)

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
    wholeId: _NodeId,
    partId: _NodeId,
    subKind: Schema.NonEmptyString,
    ordinal: Schema.optionalWith(Schema.Int, { as: "Option" }),
  }),
  Schema.Struct({
    kind: Schema.Literal("assign"),
    subjectId: _NodeId,
    definitionId: _NodeId,
    subKind: Schema.NonEmptyString,
  }),
  Schema.Struct({
    kind: Schema.Literal("associate"),
    subjectId: _NodeId,
    resourceId: _NodeId,
    usage: MaterialUsage,
  }),
  Schema.Struct({
    kind: Schema.Literal("connect"),
    fromId: _NodeId,
    toId: _NodeId,
    subKind: Schema.NonEmptyString,
    realizingId: Schema.optionalWith(_NodeId, { as: "Option" }),
    interfaceKey: Schema.optionalWith(Digest.codecs.content.bytes, { as: "Option" }),
  }),
  Schema.Struct({
    kind: Schema.Literal("void"),
    hostId: _NodeId,
    featureId: _NodeId,
    subKind: Schema.NonEmptyString,
  }),
  Schema.Struct({
    kind: Schema.Literal("generic"),
    wireName: Schema.NonEmptyString,
    relatingId: _NodeId,
    relatedId: _NodeId,
    attributes: Schema.Record({ key: Schema.String, value: Schema.Unknown }),
    participants: Schema.Array(Schema.Struct({
      nodeId: _NodeId,
      role: Schema.NonEmptyString,
      ordinal: Schema.optionalWith(Schema.Int, { as: "Option" }),
    })),
  }),
)

const Relation: Schema.Schema<typeof _edges.Type, unknown> =
  Schema.compose(_cased("edge", "hoist"), _edges, { strict: false })

// `unstableNodeIds` carries canonical X32 node identities and makes those nodes ineligible for address-based OCC.
class RedactionManifest extends Schema.Class<RedactionManifest>("RedactionManifest")({
  policy: Schema.NonEmptyString,
  clearedPaths: Schema.Array(Schema.NonEmptyString),
  unstableNodeIds: Schema.Array(_NodeId),
}) {}

// Model organization as the host-boundary producer states it: `EntityWire` keys and the source key arrive as 16
// big-endian bytes and land here as the branch's lowercase content-key face, so byte order and hex case both settle
// at this ONE decode and every consumer joins strings. Containment is a tagged pair because the wire's oneof IS the
// key space — `entity` resolves in this document, `member` in the federation space its authority issued — so a
// landing collapsing both onto one string field hands every consumer a target it cannot route.
const _OrgAddress = Digest.Key.content

const _Contained = Schema.Union(
  Schema.TaggedStruct("entity", { entity: _OrgAddress }),
  Schema.TaggedStruct("member", { member: Schema.NonEmptyString }),
)

class Organization extends Schema.Class<Organization>("Organization")({
  source: _OrgAddress,
  authority: Schema.NonEmptyString,
  entities: Schema.Array(Schema.Struct({
    address: _OrgAddress,
    name: Schema.NonEmptyString,
    ordinal: Schema.Int.pipe(Schema.nonNegative()),
    visible: Schema.Boolean,
    locked: Schema.Boolean,
  })),
  containment: Schema.Array(Schema.Struct({ container: _OrgAddress, target: _Contained })),
  overrides: Schema.Array(Schema.Struct({
    entity: _OrgAddress,
    view: Schema.NonEmptyString,
    visible: Schema.Boolean,
  })),
  current: Schema.optionalWith(_OrgAddress, { as: "Option" }),
}) {}

// Peers decode this snapshot into their own graph mirror without re-deriving an identity: the producer declares NO key
// column on this snapshot — the ids and content keys inside it are the identity — so the landing carries none and a
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
  get byId(): HashMap.HashMap<typeof _NodeId.Type, Node> {
    return Array.reduce(this.nodes, HashMap.empty<typeof _NodeId.Type, Node>(), (acc, node) => HashMap.set(acc, node.id, node))
  }
  // manifest rosters name the DECLARED-UNSTABLE set, so the complement is the only address a content-keyed
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

// Before/after pairs land at `GraphDeltaWire` field 3 — a revision the producer's normal form keys unique per id, so a
// consumer folds the pair off one row rather than diffing two rosters for the node it names.
class NodeRevision extends Schema.Class<NodeRevision>("NodeRevision")({
  before: Node.FromWire,
  after: Node.FromWire,
}) {}

// `delta#GRAPH_DELTA` carries the change record a streaming consumer folds onto the snapshot it holds. The
// header is OPTIONAL here where the snapshot's is required, because a delta re-headers the graph only where the
// producer's own reheader ran, and the five sections re-admit through the same `Node`/`Relation` gates the snapshot
// takes — one landing pair, two crossings.
class GraphDelta extends Schema.Class<GraphDelta>("GraphDelta")({
  addedNodes: Schema.Array(Node.FromWire),
  removedNodeIds: Schema.Array(_NodeId),
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
  key: Digest.codecs.content.bytes,
})

class SnapshotHeader extends Schema.Class<SnapshotHeader>("SnapshotHeader")({
  key: Digest.codecs.content.bytes,
  element: Schema.Int.pipe(Schema.nonNegative()),
  frontier: Clock.Hlc.FromBytes,
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
  // Post-redaction archive identity stays optional and distinct from the producer member's pre-redaction key.
  contentKey: Schema.optional(Schema.String.pipe(Schema.pattern(/^[0-9a-f]{32}$/))),
  fault: Schema.optional(Schema.NonEmptyString),
}) {}

// Producers cross their FLATTENED export projection, never a receipt union: a coalesced or evicted receipt names no
// bundle, so a decoder branching on a kind discriminant to find three quarters of its fields absent is exactly the
// shape the producer flattened away. This is the AppHost bundle leaving the host toward a dashboard — the
// opposite direction from `invoke`'s `SupportCapture`, which is a report arriving at this branch's gateway.
class SupportExport extends Schema.Class<SupportExport>("SupportExport")({
  trigger: Schema.Literal(..._triggers),
  reason: Schema.NonEmptyString,
  correlation: Schema.NonEmptyString,
  windowStart: Schema.DateTimeUtc,
  windowEnd: Schema.DateTimeUtc,
  bundlePath: Schema.NonEmptyString,
  totalBytes: Schema.Int.pipe(Schema.nonNegative()),
  // Producers cross this as NodaTime round-trip TEXT and no effect Duration codec reads that dialect —
  // `DurationFromMillis` wants a number and `Duration` wants the encoded object or a `[seconds, nanos]` pair — so the
  // landing carries the text the producer actually writes and a consumer needing arithmetic parses at its own
  // seam. Binding a Duration schema here would refuse every real payload while the census read correct.
  elapsed: Schema.NonEmptyString,
  redactions: Schema.Int.pipe(Schema.nonNegative()),
  entries: Schema.Array(SupportEntry),
}) {
  static readonly Entry: typeof SupportEntry = SupportEntry
}
const _labels = ["CERTIFICATE", "PUBLIC KEY", "PKCS7", "PRIVATE KEY", "EC PRIVATE KEY", "RSA PRIVATE KEY"] as const

// Producers publish their own RFC-7468 vocabulary with its own `secret` column: the mint refuses to cross a block whose
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
  labels: Schema.NonEmptyArray(Schema.Literal(..._labels)).pipe(
    Schema.filter((labels) => Array.every(labels, (label) => !_PEM[label].secret) || "<private-pem-label>"),
  ),
  chain: Schema.NonEmptyString,
  blockDigests: Schema.NonEmptyArray(Schema.NonEmptyString),
  bundleDigest: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
}) {
  static readonly Label: Schema.Literal<typeof _labels> = Schema.Literal(..._labels)
  static readonly rotated = (live: Credential, next: Credential): boolean => live.bundleDigest !== next.bundleDigest
}

declare namespace Credential {
  type Label = (typeof _labels)[number]
}
```

## [07]-[KEYED_REGISTRY]

- Owner: one closed row table derives `Wire.schema`, `decode`, `encode`, `audited`, and complete-frame `stream`.
- Law: contract compatibility reads the arm row, and structured refusals remain typed fault evidence.
- Law: `frame` parity gates every decode; Merkle summary parity calls `Commit.admit` before a decoded value returns.
- Law: `suite` parity grades the schema, so `audited` spends it and the decode path never charges it per payload.
- Law: the stream grades extent before decode, so a budget refusal carries `overrun` rather than a parse verdict.

```typescript signature
import { Stream } from "effect"

// Each entry frames FLAT beside its op — `[seq, [tag, field, …]]` — so the `seq` column `Gap` resumes on survives the
// crossing as a positional slot every peer reads at the same offset, rather than a keyed column one runtime spells.
const OpLogEntry = Schema.transform(
  Schema.Tuple(Schema.BigIntFromSelf, CrdtOp),
  Schema.Struct({ seq: Schema.BigIntFromSelf, op: Schema.typeSchema(CrdtOp) }),
  {
    strict: true,
    decode: ([seq, op]) => ({ seq, op }),
    encode: ({ seq, op }) => [seq, op] as const,
  },
)
type OpLogEntry = typeof OpLogEntry.Type

const _ReceiptEnvelopeWire = _receiptEnvelope(Schema.Unknown)
const _schema = {
  ReceiptEnvelopeWire: Format.json.schema(_ReceiptEnvelopeWire),
  // Seam [02.7] fixes a two-64-bit-half cell, so this stamp rides the extension byte the msgpack engine already
  // registers for `Clock.Hlc` — a descriptor displaces that frozen layout with tag bytes no peer minter emits.
  HlcStampWire: Format.msgpack.schema(Clock.Hlc),
  TenantContextWire: Format.json.schema(_TenantContextWire),
  CommandAvailabilityWire: Format.json.schema(Evidence.Availability),
  FaultDetail: Format.proto.family(Format.proto.suite.FaultDetail, FaultDetail.FromWire),
  ElementGraphWire: Format.proto.family(Format.proto.suite.ElementGraphWire, ElementGraph),
  GraphDeltaWire: Format.proto.family(Format.proto.suite.GraphDeltaWire, GraphDelta),
  NodeWire: Format.proto.family(Format.proto.suite.NodeWire, Node.FromWire),
  RelationshipWire: Format.proto.family(Format.proto.suite.RelationshipWire, Relation),
  OpLogWire: Format.msgpack.schema(OpLogEntry),
  SnapshotHeader: Format.cbor.schema(SnapshotHeader),
  CrdtOpWire: Format.msgpack.schema(CrdtOp),
  CommitWire: Format.msgpack.schema(Commit),
  BranchWire: Format.msgpack.schema(Commit.Branch),
  VersionVectorWire: Format.msgpack.schema(Causal.Vector),
  MerkleSummaryWire: Format.msgpack.schema(Commit.Merkle),
  EntityEditWire: Format.json.schema(EntityEditWire),
  // `Evidence.Tally` counts done-against-total over an operation tree, where Compute's phase frame crosses as
  // `ProgressUpdateWire` and shares no column with it, so the two families resolve to two descriptors and no reader
  // binds one to the other's columns; this one reads json, as its three feed siblings do.
  TallyWire: Format.json.schema(Evidence.Tally),
  CredentialPemWire: Format.json.schema(Credential),
  DescriptorPinWire: Format.json.schema(Contract.Pin),
  // Compute's minter serializes this claim through a `JsonSerializerContext` whose longs cross as decimal strings and
  // whose absence crosses as explicit null, and the claim's own `host` column IS the row below — one document, one arm.
  BenchmarkClaimWire: Format.json.schema(Board.Claim),
  HostFingerprintWire: Format.json.schema(HostFingerprint),
  BindingStatusWire: Format.json.schema(BindingStatus.FromWire),
  CoercedValueWire: Format.json.schema(CoercedValue.FromWire),
  WriteReceiptWire: Format.json.schema(WriteReceipt.FromWire),
  FlagVerdictWire: Format.json.schema(FlagVerdict),
  ControlIntentWire: Format.json.schema(ControlIntent),
  LayoutConstraintWire: Format.json.schema(LayoutProgram),
  CommandGateWire: Format.json.schema(CommandGate.FromWire),
  EvidenceTimelineWire: Format.json.schema(EvidenceTimeline),
  BcfTopicWire: Format.json.schema(BcfTopic),
  BcfViewpointWire: Format.json.schema(BcfViewpoint),
  ModelDiff: Format.json.schema(ModelDiff),
  PredicateWire: Format.json.schema(PredicateWire),
  MaterialWire: Format.msgpack.schema(Material.FromWire),
  OpenPbrGroupsWire: Format.msgpack.schema(PbrGroups.FromVector),
  TextureSetWire: Format.proto.family(Format.proto.suite.TextureSetWire, TextureSet),
  AssetSetManifest: Format.proto.family(Format.proto.suite.AssetSetManifest, AssetSetManifest),
  OrganizationWire: Format.proto.family(Format.proto.suite.OrganizationWire, Organization),
  SupportCaptureWire: Format.json.schema(SupportExport),
} as const

// Parity obligations split on WHEN they are owed, because two unlike checks were riding one column. A `frame` row
// grades THIS payload — a merkle root re-derives from the summary's own rows and disagrees per arrival — so the
// decode path owes it before a value returns. A `suite` row grades the SCHEMA: `_semantic` re-encodes a decoded
// value and compares it against itself, never against the arriving octets, so its verdict is constant across every
// input. Running it per frame charged an encode, a decode, and a deep structural compare on every ingress frame —
// on `ElementGraphWire` and `AssetSetManifest`, the branch's largest payloads — to re-derive a fact one conformance
// run settles per family, and encoded families whose own row declares `decode`.
const _whens = ["frame", "suite"] as const

const _semantic = <A, I>(family: Wire.Family, schema: Schema.Schema<A, I>): Wire.Parity<A>["run"] => (value) =>
  Effect.flatMap(Schema.encode(schema)(value), (encoded) =>
    Effect.flatMap(Schema.decodeUnknown(schema)(encoded), (decoded) =>
      Schema.equivalence(schema)(value, decoded)
        ? Effect.void
        : Effect.fail(_mismatch(family, value, decoded, "<semantic-divergence>"))))

const _descriptor = <A>(family: Contract.Family): Contract.Gate<A> => ({ compatibility }) =>
  Contract.Descriptor.gate({ family, compatibility })

const _row = <A, I, const D extends Wire.Direction, const F extends Wire.Arm>(
  direction: D,
  arm: F,
  schema: Schema.Schema<A, I>,
  gate: Option.Option<Contract.Gate<A>> = Option.none(),
  parity: Option.Option<Wire.Parity<A>> = Option.none(),
): Wire.Row<A, I, D, F> => ({ direction, arm, schema, gate, parity })

const _proto = <K extends Extract<keyof typeof _schema, Contract.Family>, const D extends Wire.Direction = "decode">(
  family: K,
  direction: D = "decode" as D,
): Wire.Row<Schema.Schema.Type<(typeof _schema)[K]>, Uint8Array, D, "proto"> =>
  _row(
    direction,
    "proto",
    _schema[family],
    Option.some(_descriptor(family)),
    Option.some({ when: "suite", run: _semantic(family, _schema[family]) }),
  )

const _merkleParity: Wire.Parity<Commit.Merkle> = {
  when: "frame",
  run: (summary) =>
    Commit.admit(summary).pipe(
      Effect.asVoid,
      Effect.mapError(() => new WireFault({
        family: "MerkleSummaryWire",
        reason: "parity",
        detail: "<merkle-root>",
        evidence: Option.none(),
      })),
    ),
}

const _rows = {
  ReceiptEnvelopeWire: _row("decode", "json", _schema.ReceiptEnvelopeWire),
  HlcStampWire: _row("duplex", "msgpack", _schema.HlcStampWire),
  TenantContextWire: _row("decode", "json", _schema.TenantContextWire),
  CommandAvailabilityWire: _row("decode", "json", _schema.CommandAvailabilityWire),
  FaultDetail: _proto("FaultDetail"),
  ElementGraphWire: _proto("ElementGraphWire"),
  GraphDeltaWire: _proto("GraphDeltaWire"),
  NodeWire: _proto("NodeWire"),
  RelationshipWire: _proto("RelationshipWire"),
  OpLogWire: _row("duplex", "msgpack", _schema.OpLogWire),
  SnapshotHeader: _row("decode", "cbor", _schema.SnapshotHeader),
  CrdtOpWire: _row("duplex", "msgpack", _schema.CrdtOpWire),
  CommitWire: _row("duplex", "msgpack", _schema.CommitWire),
  BranchWire: _row("duplex", "msgpack", _schema.BranchWire),
  VersionVectorWire: _row("duplex", "msgpack", _schema.VersionVectorWire),
  MerkleSummaryWire: _row(
    "duplex",
    "msgpack",
    _schema.MerkleSummaryWire,
    Option.none(),
    Option.some(_merkleParity),
  ),
  EntityEditWire: _row("decode", "json", _schema.EntityEditWire),
  TallyWire: _row("decode", "json", _schema.TallyWire),
  CredentialPemWire: _row("decode", "json", _schema.CredentialPemWire),
  DescriptorPinWire: _row(
    "decode",
    "json",
    _schema.DescriptorPinWire,
    Option.some(_descriptor("CapabilityDescriptorWire")),
  ),
  BenchmarkClaimWire: _row("decode", "json", _schema.BenchmarkClaimWire),
  HostFingerprintWire: _row("decode", "json", _schema.HostFingerprintWire),
  BindingStatusWire: _row("decode", "json", _schema.BindingStatusWire),
  CoercedValueWire: _row("decode", "json", _schema.CoercedValueWire),
  WriteReceiptWire: _row("decode", "json", _schema.WriteReceiptWire),
  FlagVerdictWire: _row("decode", "json", _schema.FlagVerdictWire),
  ControlIntentWire: _row("decode", "json", _schema.ControlIntentWire),
  LayoutConstraintWire: _row("decode", "json", _schema.LayoutConstraintWire),
  CommandGateWire: _row("decode", "json", _schema.CommandGateWire),
  EvidenceTimelineWire: _row("decode", "json", _schema.EvidenceTimelineWire),
  BcfTopicWire: _row("decode", "json", _schema.BcfTopicWire),
  BcfViewpointWire: _row("decode", "json", _schema.BcfViewpointWire),
  ModelDiff: _row("decode", "json", _schema.ModelDiff),
  PredicateWire: _row("duplex", "json", _schema.PredicateWire),
  MaterialWire: _row("decode", "msgpack", _schema.MaterialWire),
  OpenPbrGroupsWire: _row("decode", "msgpack", _schema.OpenPbrGroupsWire),
  TextureSetWire: _proto("TextureSetWire"),
  AssetSetManifest: _proto("AssetSetManifest"),
  OrganizationWire: _proto("OrganizationWire"),
  SupportCaptureWire: _row("decode", "json", _schema.SupportCaptureWire),
} as const

const _family = Shape.vocabulary(_families, _rows)

type _Credential = Schema.Schema.Type<(typeof _rows)["CredentialPemWire"]["schema"]>
type _CredentialLabel = Credential.Label
type _GeoFeature = Schema.Schema.Type<typeof GeoFeature>
type _GeoFeatureCrs = GeoFeature.Crs
type _GeoFeatureExtent = GeoFeature.Extent
type _GeoFeatureTile = GeoFeature.Tile
type _HopsReason = Hops.Reason
type _HopsRow = Hops.Row
type _TextureAlphaMode = Texture.AlphaMode
type _TextureLayerLaw = Texture.LayerLaw
type _TextureMipPolicy = Texture.MipPolicy
type _TexturePack = Texture.Pack
type _TexturePayload = Texture.Payload
type _TexturePlaneFormat = Texture.PlaneFormat
type _TextureTransfer = Texture.Transfer
type _TextureWirePayload = Texture.WirePayload

declare namespace Wire {
  type Direction = "decode" | "encode" | "duplex"
  // Arm vocabulary belongs to the plane owning the engines; spelling it a second time here let a row name an
  // encoding `Format` never published, and let the compatibility token drift off the arm it describes.
  type Arm = Format.Arm
  type Family = (typeof _families)[number]
  type FaultFamily = (typeof _faultFamilies)[number]
  type Decoded<K extends Family> = Schema.Schema.Type<(typeof _rows)[K]["schema"]>
  type When = (typeof _whens)[number]
  type Parity<A> = {
    readonly when: When
    readonly run: (value: A, octets: Uint8Array) => Effect.Effect<void, ParseResult.ParseError | WireFault>
  }
  type Row<A, I = Uint8Array, D extends Direction = Direction, F extends Arm = Arm> = {
    readonly direction: D
    readonly arm: F
    readonly schema: Schema.Schema<A, I>
    readonly gate: Option.Option<Contract.Gate<A>>
    readonly parity: Option.Option<Parity<A>>
  }
  type Ingress = { readonly [K in Family]: (typeof _rows)[K]["direction"] extends "encode" ? never : K }[Family]
  type Egress = { readonly [K in Family]: (typeof _rows)[K]["direction"] extends "decode" ? never : K }[Family]
  type Fault = WireFault
  type FaultDetail = Decoded<"FaultDetail">
  type BindingStatus = Decoded<"BindingStatusWire">
  type CoercedValue = Decoded<"CoercedValueWire">
  type WriteReceipt = Decoded<"WriteReceiptWire">
  type CommandGate = Decoded<"CommandGateWire">
  type ControlIntent = Decoded<"ControlIntentWire">
  type LayoutProgram = Decoded<"LayoutConstraintWire">
  type Credential = _Credential
  namespace Credential {
    type Label = _CredentialLabel
  }
  type GeoFeature = _GeoFeature
  namespace GeoFeature {
    type Crs = _GeoFeatureCrs
    type Extent = _GeoFeatureExtent
    type Tile = _GeoFeatureTile
  }
  namespace Hops {
    type Reason = _HopsReason
    type Row = _HopsRow
  }
  namespace Texture {
    type AlphaMode = _TextureAlphaMode
    type LayerLaw = _TextureLayerLaw
    type MipPolicy = _TextureMipPolicy
    type Pack = _TexturePack
    type Payload = _TexturePayload
    type PlaneFormat = _TexturePlaneFormat
    type Transfer = _TextureTransfer
    type WirePayload = _TextureWirePayload
  }
  type BcfTopic = Decoded<"BcfTopicWire">
  type BcfViewpoint = Decoded<"BcfViewpointWire">
  type ModelDiff = Decoded<"ModelDiff">
  type EvidenceTimeline = Decoded<"EvidenceTimelineWire">
  type EntityEdit = Decoded<"EntityEditWire">
  type Shape = {
    readonly families: typeof _families
    readonly wire: Schema.Literal<readonly [Family, ...Family[]]>
    readonly is: (input: unknown) => input is Family
    readonly schema: <K extends Family>(family: K) => (typeof _rows)[K]["schema"]
    readonly decode: <K extends Ingress>(family: K, octets: Uint8Array) => Effect.Effect<
      Decoded<K>, ParseResult.ParseError | WireFault, Context.Tag.Service<typeof Contract.Descriptor>
    >
    readonly encode: <K extends Egress>(family: K, value: Decoded<K>) => Effect.Effect<Uint8Array, ParseResult.ParseError>
    readonly audited: <K extends Family>(
      family: K,
      value: Decoded<K>,
      octets: Uint8Array,
    ) => Effect.Effect<void, ParseResult.ParseError | WireFault>
    readonly stream: <K extends Ingress>(family: K, frames: AsyncIterable<Uint8Array>) => Stream.Stream<
      Either.Either<Decoded<K>, WireFault>, WireFault, Quarantine | Context.Tag.Service<typeof Contract.Descriptor>
    >
  }
}

const _refused = (family: Wire.Family, refusal: Contract.Refusal): WireFault =>
  new WireFault({
    family,
    reason: "drift",
    detail: `<contract:${refusal.compatibility}:${refusal.verdict}:${refusal.changes}>`,
    evidence: Option.some(refusal),
  })

const _decode = <K extends Wire.Ingress>(family: K, octets: Uint8Array) =>
  Effect.flatMap(Schema.decodeUnknown(_rows[family].schema)(octets), (decoded) =>
    Effect.andThen(
      Option.match(_rows[family].gate, {
        onNone: () => Effect.void,
        onSome: (gate) => gate({ family: decoded, compatibility: Format.rows.arm[_rows[family].arm].compatibility }).pipe(
          Effect.mapError((refusal) => _refused(family, refusal)),
        ),
      }),
      Effect.as(
        Option.match(Option.filter(_rows[family].parity, (row) => row.when === "frame"), {
          onNone: () => Effect.void,
          onSome: (row) => row.run(decoded, octets),
        }),
        decoded,
      ),
    ))

// Byte ceilings grade HERE, where the extent is in hand and the cause is nameable. Left to the codec's own
// octet filter it reached this fold as an undifferentiated `ParseError` and folded to `malformed` — a cause whose
// row retains the frame and replays it three times over ninety seconds, which is the retention an oversized frame
// is refused to avoid, spent on a verdict no re-decode can move. Recovering the class from the issue's message
// would put the classification on a string; measuring the extent puts it on the number the refusal is about.
const _overrun = (family: Wire.FaultFamily, actual: number): WireFault =>
  new WireFault({
    family,
    reason: "overrun",
    detail: "<payload-overrun>",
    evidence: Option.some({ actual, expected: Shape.Ingress.floor.bytes }),
  })

const _completeStream = <K extends Wire.Ingress>(family: K, frames: AsyncIterable<Uint8Array>) =>
  Stream.fromAsyncIterable(frames, (defect) =>
    new WireFault({ family, reason: "malformed", detail: String(defect), evidence: Option.none() })).pipe(
    Stream.mapEffect(
      (octets) =>
        (octets.byteLength > Shape.Ingress.floor.bytes
          ? Effect.fail(_overrun(family, octets.byteLength))
          : _decode(family, octets)).pipe(
          Effect.mapError((issue) => issue instanceof WireFault
            ? issue
            : new WireFault({ family, reason: "malformed", detail: issue.message, evidence: Option.none() })),
          Quarantine.divert({ family, octets: () => octets }),
        ),
      { concurrency: 1 },
    ),
  )
}
```

## [08]-[FEED_DEDUP]

- Owner: `Wire.feed` performs complete-frame decode, quarantine, keyed transition deduplication, and declared shaping.
- Law: shaping delays values without cross-subject coalescing or loss.
- Law: a feed row admits a STATE family alone — one whose subject re-reports an unchanged reading.
- Law: `_CADENCE` bands price every shaped feed; a row selects a band and never spells its own triple.

```typescript signature
import type { Duration, Equivalence } from "effect"

// Transition dedup admits a STATE family and refuses an EVENT one. A binding re-reporting `bound`, a palette gate
// re-reporting `available`, a progress mark re-reporting its phase, and a flag re-reporting its verdict are all one
// reading restated, so the fold drops the restatement and the subject keeps its latest. `WriteReceiptWire` and
// `CoercedValueWire` are refused on that same ground and not for lack of a subject: each frame is a distinct
// occurrence carrying its own stamp or its own offered/landed pair, so no two ever compare alike and a row for
// either buys a per-element projection and hash write that can never drop anything.
const _feedKeys = ["TallyWire", "FlagVerdictWire", "BindingStatusWire", "CommandGateWire"] as const

// One cadence axis, two bands. `display` prices a feed a human reads as it redraws; `control` prices one a shell
// re-reports on every keystroke, where the reader is a gate rather than an eye and a quarter of the display budget
// still outruns typing. A per-row triple would seat unsourced numbers on every row and drift the moment one moved.
const _bands = ["display", "control"] as const

declare namespace feed {
  type Family = (typeof _feedKeys)[number]
  type Band = (typeof _bands)[number]
  type Flow = {
    readonly units: number
    readonly per: Duration.DurationInput
    readonly burst: number
  }
  type Row<A> = {
    readonly subject: (value: A) => string
    readonly alike: Equivalence.Equivalence<A>
    readonly band: Option.Option<Band>
  }
  type _Bands<T extends { readonly [K in Band]: Flow } = typeof _CADENCE> = T
}

// Coalescing grain sits at the bucket's own token period, so a burst collapses to its CURRENT reading inside
// each window and the bucket prices the residue instead of delaying a tail of superseded ones.
const _CADENCE = {
  display: { units: 240, per: "1 second", burst: 60 },
  control: { units: 60, per: "1 second", burst: 20 },
} as const satisfies { readonly [K in feed.Band]: feed.Flow }

const _feeds: { readonly [K in feed.Family]: feed.Row<Wire.Decoded<K>> } = {
  TallyWire: {
    subject: (mark) => mark.operation,
    alike: Evidence.Tally.transition,
    band: Option.some("display"),
  },
  FlagVerdictWire: {
    subject: (verdict) => verdict.flag,
    alike: Schema.equivalence(FlagVerdict),
    // Flag verdicts change on a rollout, never on a redraw, so the transition fold already floors the rate and a
    // bucket over it would only delay the one frame a consumer is waiting for.
    band: Option.none(),
  },
  BindingStatusWire: {
    subject: (status) => status.binding,
    alike: Schema.equivalence(BindingStatus),
    band: Option.some("control"),
  },
  CommandGateWire: {
    subject: (gate) => gate.key,
    alike: Schema.equivalence(CommandGate),
    band: Option.some("control"),
  },
}

const _transitions = <A>(row: feed.Row<A>) => <E, R>(marks: Stream.Stream<A, E, R>): Stream.Stream<A, E, R> =>
  marks.pipe(
    // subject projection binds ONCE per element: a re-read per arm charges the keying fold three projections on the
    // feed's own hot path, where the declared cadence is hundreds of marks a second
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
): Stream.Stream<
  Wire.Decoded<K>, Wire.Fault, Quarantine | Context.Tag.Service<typeof Contract.Descriptor>
> => {
  const row = _feeds[family]
  return Wire.stream(family, frames).pipe(
    Stream.filterMap(Either.getRight),
    _transitions(row),
    (deduped) =>
      Option.match(row.band, {
        onNone: () => deduped,
        onSome: (band) =>
          pipe(_CADENCE[band], (flow) =>
            Stream.throttle(deduped, {
              cost: Chunk.size,
              units: flow.units,
              duration: flow.per,
              burst: flow.burst,
              strategy: "shape",
            })),
      }),
  )
}
```

## [09]-[SEQUENCE_GAP]

- Owner: `Wire.Gap` owns sequence evidence and resumable ordered delivery.
- Law: replayed coordinates drop, gaps emit evidence once, and valid arrivals still deliver.

```typescript signature
const _bySeq: Order.Order<OpLogEntry> = Order.mapInput(Order.bigint, (entry: OpLogEntry) => entry.seq)

const Gap: {
  readonly evidence: (family: Wire.FaultFamily, expected: bigint, actual: bigint, detail?: string) => WireFault
  readonly sequential: (
    family: Wire.FaultFamily,
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
  readonly stamp: (op: Extract<CrdtOp, { readonly physicalTicks: bigint }>) => Clock.Hlc
  readonly stream: (
    frames: ReadableStream<Uint8Array> | AsyncIterable<Uint8Array>,
    resume: bigint,
  ) => Stream.Stream<Either.Either<OpLogEntry, WireFault>, WireFault, Quarantine>
  readonly frontier: (entries: ReadonlyArray<OpLogEntry>) => Option.Option<bigint>
} = {
  Entry: OpLogEntry,
  stamp: _stamped,
  stream: (frames, resume) =>
    _completeStream("OpLogWire", frames).pipe(Gap.sequential("OpLogWire", resume)),
  frontier: (entries) =>
    Array.isNonEmptyReadonlyArray(entries) ? Option.some(Array.max(entries, _bySeq).seq) : Option.none(),
}

const _registry: Wire.Shape = {
  families: _family.kinds,
  wire: _family.schema,
  is: _family.is,
  schema: (family) => _rows[family].schema,
  decode: _decode,
  encode: (family, value) => Schema.encode(_rows[family].schema)(value),
  // Conformance rails spend EVERY parity row a family carries, `suite` included, so the schema-level proof the
  // decode path no longer charges per frame is still owed — deliberately, once, where a fixture run can afford it.
  audited: (family, value, octets) =>
    Option.match(_rows[family].parity, { onNone: () => Effect.void, onSome: (row) => row.run(value, octets) }),
  stream: _completeStream,
}

const Wire = {
  ..._registry,
  Fault: WireFault,
  Quarantine,
  Parity,
  feed,
  Gap,
  OpLog,
  CrdtOp,
  Hops,
  FaultDetail,
  Credential,
  FlagVerdict,
  EvidenceTimeline,
  BindingStatus,
  CoercedValue,
  WriteReceipt,
  CommandGate,
  ControlIntent,
  LayoutProgram,
  BcfTopic,
  BcfViewpoint,
  ModelDiff,
  EntityEdit: EntityEditWire,
  Predicate: PredicateWire,
  PbrGroups,
  Material,
  AppearanceSummary,
  Texture,
  TextureSet,
  AssetSetManifest,
  GeoFeature,
  WkbParser,
  ElementGraph,
  GraphDelta,
  Organization,
  SnapshotHeader,
  SupportExport,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Wire }
```

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
