# [CORE_CODEC]

`Wire` owns the branch's closed wire vocabulary and everything derived from it: one ordered family roster, one row per family carrying direction, arm, schema, contract gate, and parity obligation, and the decoded landing each producer's shape resolves to. Fault classification, bounded quarantine with replay, content and semantic parity, keyed transition feeds, and sequence evidence all read that one table. Module `core/src/interchange/codec.ts` admits a family as one row, a fault cause as one policy row, and a transport status as one `Hops` row.

`Wire` composes the `value` floor's identity, clock, and schema owners, the `state` causal, commit, and evidence owners, and `observe`'s board claim; a quantity rides `MeasureValueWire` and binds no family here. Every codec arrives from `interchange/format`: arm selection, contract compatibility, and quarantined-frame rendering read `Format.rows.arm`, so no consumer here spells an encoding name. `interchange/carrier` binds `Wire.Family` for its typed-metadata table, and `interchange/frame` composes `Wire.Fault`, `Wire.Gap`, `Wire.Parity`, and `Wire.Quarantine` for its own bounded assemblers.

## [01]-[INDEX]

- [02]-[WIRE_REGISTRY]: ordered family vocabulary and exact row contract; `Wire`.
- [03]-[FAULT_RAIL]: fault policy, quarantine intake, replay, and divert; `Wire.Fault`, `Wire.Quarantine`.
- [04]-[PARITY_VERIFY]: content-key verification and semantic roundtrip; `Wire.Parity`.
- [05]-[LANDING_EVIDENCE]: evidence, identity, version, CRDT, and oplog landings; `Wire`.
- [06]-[LANDING_WIRE]: wire-owned decoded shapes for later-wave consumers; landing classes on `Wire`.
- [07]-[KEYED_REGISTRY]: mapped landing table, polymorphic decode/encode/stream entrypoints, bounded tree walk; `Wire`, `Wire.Walk`.
- [08]-[FEED_DEDUP]: quarantine diversion and family transition policies; `Wire.feed`.
- [09]-[SEQUENCE_GAP]: sequence evidence, oplog continuity, and frontier reads; `Wire.Gap`, `Wire.OpLog`.

## [02]-[WIRE_REGISTRY]

- Owner: `_families` and `_rows` close the ordered wire vocabulary and its typed registry.
- Law: each row preserves literal direction and arm and carries one schema, optional contract gate, and optional parity.
- Law: `_faultFamilies` widens the roster with families this page never decodes but whose owners raise faults against it.
- Law: `_faultArms` names the arm of every such family, so arm resolution stays total across the fault roster.
- Law: a frozen family KEEPS its byte shape when its interior owner moves down-strata, and the decode re-targets onto the new owner in ONE unit.
- Law: tear-then-rebuild is the barred order, because every peer decoder is stranded across the window between the tear and the re-land.
- Law: the re-target edits that family's `_schema` entry ALONE, never `_families`, so the census row, contract gate, and parity obligation survive the move.
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
  "HopReceiptWire", "DeliveryReceiptWire", "DropReceiptWire",
  "OutboxRowWire", "DeadLetterRowWire", "ReplayTallyWire", "OutboxLaneWire", "OutboxSweepWire",
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
import { fromJson, isMessage, toJson } from "@bufbuild/protobuf"
import {
  Cause, Chunk, DateTime, Effect, Either, Exit, Function, HashMap, Match, Option, Order, pipe, Predicate, Record,
  Schedule, STM, TMap, TRef,
} from "effect"
import { Fault } from "../value/fault.ts"
import { Format } from "./format.ts"

const _causes = ["malformed", "truncated", "overrun", "sequence", "parity", "drift", "stale", "conflict"] as const

// The budget axes are a CLOSED roster because every one of them was a `<walk-fan>`-shaped token inside a free
// string: nine ceilings across this page and `frame` refuse under one cause, and naming which one broke is the
// single fact an operator acts on. The artifact coordinate is optional on this cause alone, since only the assembly
// legs know which artifact and generation a refused band belonged to.
const _overrunAxes = [
  "payload", "frames", "assembly", "rendezvous",
  "walk-depth", "walk-fan",
  "tensor-extent", "tensor-stride", "tensor-span",
] as const
// Every other subject roster the retired string encoded, named the same way: the raiser passes a roster member and
// the mint below takes that member's own type, so a token this page never declared refuses at the call rather than
// reaching a census as prose.
const _paritySubjects = ["key", "golden-bytes", "semantic", "merkle-root"] as const
const _gapSubjects = ["ordinal", "total", "tail"] as const
const _coordinate = Schema.OptionFromSelf(
  Schema.Struct({ artifact: Digest.codecs.content.wire, generation: Schema.Int.pipe(Schema.nonNegative()) }),
)

// One row declares the whole cause: the branch class, the surface leg that DECIDES it, the SUBJECT a raise must
// supply, and the renderer over that subject. The retired shape carried a free-string `detail` beside a three-arm
// `evidence` union every reason shared, so a cause's real columns were unrecoverable in both directions —
// `<total-drift>`, `<tensor-stride>`, `<unmatched-envelope:…>`, and nine more were DISCRIMINANTS living inside a
// string no consumer could switch on and no compiler could spell-check, while the shared union offered a merkle
// refusal the `artifact`/`generation` pair only the assembly legs carry and offered a contract refusal to causes
// that never see one. Each subject below is exact at its own raise, so the axis a budget broke, the end a schema
// version moved, and the side of a rendezvous that never matched are typed columns a board reads.
//
// Beside the owner's four columns sit the two genuinely this plane's — `held` whether the failing frame is RETAINED
// in the poison census (a frame-retention disposition, not the class table's repair-intake divert), `replayable`
// whether re-decoding the same octets can change the verdict at all (which no class-level retryability answers:
// unparseable bytes are non-retryable as transport and replayable as evidence the moment the producing peer is
// fixed). They ride BESIDE `Fault.Class.row` rather than inside it, so the owner stays the one exactness gate over
// class, leg, subject, and renderer and neither plane-local column can be mistaken for part of that contract. A
// local rank column beside `class` would fork the one severity lattice the branch tuple already declares.
const _policy = Fault.Class.family(_causes, {
  malformed: {
    ...Fault.Class.row({
      class: "malformed",
      leg: "codec",
      // `at` separates the two refusals that shared one reason: the ARRIVAL iterator threw, or the octets did not
      // conform. Both fold to one class and one posture, and only the column says which end a repair belongs at.
      detail: Schema.Struct({ at: Schema.Literal("source", "decode"), issue: Schema.String }),
      render: ({ at, issue }) => `<${at}> refused the frame — ${issue}`,
    }),
    held: true,
    replayable: true,
  },
  truncated: {
    ...Fault.Class.row({
      class: "malformed",
      leg: "frame",
      detail: Schema.Struct({ side: Schema.Literal("envelope", "artifact"), coordinate: Schema.NonEmptyString }),
      render: ({ side, coordinate }) => `${coordinate} held an unmatched ${side} when the stream ended`,
    }),
    held: true,
    replayable: true,
  },
  // Over-budget frames are the ONE cause whose evidence is its measurement: the subject already carries the actual
  // and expected extents, and retaining the octets would pin exactly the bytes the budget refused — a census of
  // oversized frames is the exhaustion the refusal exists to prevent, and `replayable: false` means the retention
  // buys no second verdict either.
  overrun: {
    ...Fault.Class.row({
      class: "exhausted",
      leg: "budget",
      detail: Schema.Struct({
        axis: Schema.Literal(..._overrunAxes),
        actual: Schema.Number,
        expected: Schema.Number,
        at: _coordinate,
      }),
      render: ({ axis, actual, expected }) => `<${axis}> spent ${actual} against a ceiling of ${expected}`,
    }),
    held: false,
    replayable: false,
  },
  sequence: {
    ...Fault.Class.row({
      class: "absent",
      leg: "frame",
      detail: Schema.Struct({
        subject: Schema.Literal(..._gapSubjects),
        actual: Schema.BigIntFromSelf,
        expected: Schema.BigIntFromSelf,
      }),
      render: ({ subject, actual, expected }) => `<${subject}> read ${actual} where ${expected} was owed`,
    }),
    held: false,
    replayable: false,
  },
  parity: {
    ...Fault.Class.row({
      class: "breached",
      leg: "identity",
      detail: Schema.Struct({
        subject: Schema.Literal(..._paritySubjects),
        actual: Schema.Unknown,
        expected: Schema.Unknown,
      }),
      render: ({ subject }) => `<${subject}> re-derived to a value its own producer did not send`,
    }),
    held: true,
    replayable: false,
  },
  drift: {
    ...Fault.Class.row({
      class: "invalid",
      leg: "contract",
      // The three drift raises name UNLIKE coordinates — a descriptor comparison answers a typed `Contract.Refusal`,
      // a method-kind mismatch a shipped-against-pinned pair, an unknown verb the key alone — so the discriminant
      // carries the arm rather than one pair widened to `unknown` for all three. The refusal stays TYPED where one
      // exists, and the verb arm carries no pair at all rather than an absent one.
      detail: Schema.Struct({
        divergence: Schema.Union(
          Schema.Struct({ subject: Schema.Literal("contract"), refusal: Contract.Refusal }),
          Schema.Struct({ subject: Schema.Literal("binding"), actual: Schema.Unknown, expected: Schema.Unknown }),
          Schema.Struct({ subject: Schema.Literal("verb"), key: Schema.NonEmptyString }),
        ),
      }),
      render: (issue) =>
        Match.value(issue.divergence).pipe(Match.discriminatorsExhaustive("subject")({
          contract: ({ refusal }) => `<contract:${refusal.compatibility}:${refusal.verdict}> ${refusal.changes}`,
          binding: () => "<binding> the shipped surface no longer carries the kind the pin admitted",
          verb: ({ key }) => `<verb> ${key} names no row on the frozen deck`,
        })),
    }),
    held: true,
    replayable: true,
  },
  // Both version verdicts are ONE defect — a cluster roster read at the wrong offset — told apart by which END
  // moved, so they share a subject and split on the reason the raiser elects off the same comparison.
  stale: {
    ...Fault.Class.row({
      class: "conflicted",
      leg: "residency",
      detail: Schema.Struct({ pinned: Schema.Int, arrived: Schema.Int }),
      render: ({ pinned, arrived }) => `schema ${arrived} is superseded by the pinned ${pinned}`,
    }),
    held: false,
    replayable: true,
  },
  conflict: {
    ...Fault.Class.row({
      class: "conflicted",
      leg: "residency",
      detail: Schema.Struct({ pinned: Schema.Int, arrived: Schema.Int }),
      render: ({ pinned, arrived }) => `schema ${arrived} carries columns the pinned ${pinned} has never seen`,
    }),
    held: false,
    replayable: true,
  },
})

// `case` is the whole refusal: the reason and exactly the columns that reason declares, admitted together by the
// roster's own payload schema. A raise cannot supply a subject its cause does not own, and `message` renders
// through the cause's own arm rather than through a template this class hand-writes over a free string.
class WireFault extends Schema.TaggedError<WireFault>()("WireFault", {
  family: _faultLiteral,
  case: _policy.payload,
}) {
  static readonly bySeverity: Order.Order<WireFault> = Order.mapInput(Fault.Class.order, (fault: WireFault) => fault.class)
  static readonly dominant = (faults: Array.NonEmptyReadonlyArray<WireFault>): WireFault =>
    Array.max(faults, WireFault.bySeverity)
  // Reason, class, and leg publish AS MEMBERS because every altitude above grades this value structurally — the
  // quarantine census keys retention on the policy row, and the invoke emission names the reason in its own label
  // space — so a fact reachable only by re-reading `case` outside the class is a fact those altitudes cannot spend.
  get reason(): WireFault.Reason {
    return this.case.reason
  }
  get class(): Fault.Class.Kind {
    return _policy.classOf(this.case.reason)
  }
  get leg(): string {
    return _policy.legOf(this.case.reason)
  }
  get policy(): WireFault.Row {
    return _policy.at(this.case.reason)
  }
  override get message(): string {
    return `<${this.family}> ${_policy.render(this.case)}`
  }
}

declare namespace WireFault {
  type Case = typeof _policy.payload.Type
  type Reason = (typeof _policy.kinds)[number]
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

const _mismatch = (
  family: Wire.FaultFamily,
  subject: (typeof _paritySubjects)[number],
  actual: unknown,
  expected: unknown,
): WireFault => new WireFault({ family, case: { reason: "parity", subject, actual, expected } })

const Parity = {
  key: (payload: Digest.Payload): Effect.Effect<Digest.Key<"content">> => Digest.mint("content", payload),
  matched: (
    family: Wire.FaultFamily,
    actual: Digest.Key<"content">,
    expected: Digest.Key<"content">,
  ): Effect.Effect<void, WireFault> =>
    actual === expected ? Effect.void : Effect.fail(_mismatch(family, "key", actual, expected)),
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
        "golden-bytes",
        { extent: emitted.length, offset, byte: emitted[offset] },
        { extent: octets.length, offset, byte: octets[offset] },
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

// Positional framing is ONE law this page reads at two grains, and the slot roster is where it lives: a positional
// tuple on the encoded side, a named owner on the interior side, and the roster carrying the producer's `[Key(n)]`
// ORDER and its column NAMES together. `_op` prefixes the integer union tag its ten CRDT arms lead with; `_keyed` is
// the same fold untagged, which is exactly what every `[MessagePackObject]` appearance record is. A
// `Schema.TaggedStruct` alone encodes a map keyed by `_tag` and refuses the producer's first byte; a per-record hand
// transform reading `v[0]`…`v[30]` lets one slot drift off the roster naming it, and it restates that roster a
// second time to encode, so the two directions can disagree with nothing raising.
//
// An APPENDED slot rides `Schema.optionalElement`, so pre-append bytes ending early decode unchanged and the named
// column carries the branch's own absence carrier rather than a hole — the producer's stated default is supplied at
// the ONE arm that reads the column, never fabricated inside this fold.
type _Cell = Schema.Schema.Any | Schema.Element<Schema.Schema.Any, "?">
type _Pairs = ReadonlyArray<readonly [string, _Cell]>
type _Slots<S extends _Pairs> = { readonly [I in keyof S]: S[I][1] }
type _Named<S extends _Pairs> = {
  readonly [E in S[number] as E[0]]: E[1] extends Schema.Element<infer T extends Schema.Schema.Any, "?">
    ? Schema.optionalWith<T, { readonly as: "Option"; readonly exact: true }>
    : E[1]
}

// The tuple takes the cell verbatim and the named owner takes its unwrapped column, so one roster row states both
// sides of an appended slot. `Schema.isSchema` is the shipped discriminant: an `Element` carries the token, never
// the schema brand, so no local marker or arity knob decides which half a row is.
const _held = (cell: _Cell): Schema.Schema.Any =>
  Schema.isSchema(cell) ? cell : Schema.optionalWith(cell.from, { as: "Option", exact: true })
const _fields = <const S extends _Pairs>(slots: S): _Named<S> & Schema.Struct.Fields =>
  Record.map(Record.fromEntries(slots), _held) as _Named<S> & Schema.Struct.Fields
// Both directions read the roster and NEVER a position literal, and both drop an absent slot rather than seating a
// hole: `Array.get` answers `Option` for a tuple that ended early and `Record.get` answers `Option` for the column
// its Option-carried absence omitted. Omission is safe in exactly one direction — `Schema.optionalElement` admits
// trailing slots alone — so a middle column can never go missing and shift every later slot onto its neighbour.
const _read = (slots: _Pairs, cells: ReadonlyArray<unknown>): Record.ReadonlyRecord<string, unknown> =>
  Record.fromEntries(Array.filterMap(slots, ([key], at) => Option.map(Array.get(cells, at), (cell) => [key, cell] as const)))
const _write = (slots: _Pairs, named: Record.ReadonlyRecord<string, unknown>): ReadonlyArray<unknown> =>
  Array.filterMap(slots, ([key]) => Record.get(named, key))

const _keyed = <const N extends string, const S extends _Pairs>(name: N, slots: S) =>
  Schema.transform(
    Schema.Tuple(...(Array.map(slots, ([, cell]) => cell) as unknown as _Slots<S>)),
    Schema.Struct(_fields(slots)).annotations({ identifier: name }),
    {
      strict: false,
      decode: (wire) => _read(slots, wire),
      encode: (named) => _write(slots, named),
    },
  )

const _op = <const T extends number, const N extends string, const S extends _Pairs>(tag: T, name: N, slots: S) =>
  Schema.transform(
    Schema.Tuple(Schema.Literal(tag), ...(Array.map(slots, ([, cell]) => cell) as unknown as _Slots<S>)),
    Schema.TaggedStruct(name, _fields(slots)),
    {
      strict: false,
      decode: (wire) => ({ _tag: name, ..._read(slots, Array.drop(wire, 1)) }),
      encode: (op) => [tag, ..._write(slots, op)],
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
- Law: `RemoteDetail` carries the COMPACT fault roster — `code` is the whole transported identity, and owner and case stay local derivations.
- Law: one gRPC code roster answers every TRANSPORT-status question, so the egress fault reads its columns and no literal ladder re-derives a kind.
- Law: a remote fault's class elects off the producer's typed recovery arm; the domain code stays identity and reaches no retry band.
- Law: the three invocation outcomes elect by trailer SHAPE — one decodable detail is remote, undecodable or plural is malformed, absent is transport.
- Law: the IFC typed-value fold lands as the producer's own fourteen-arm `{ case, value }` face, closed both ways against its roster.
- Law: the control-intent family closes THIRTY-ONE arms; the wire stays open, so decoding a subset is lawful and re-authoring it is not.
- Law: banner dismissibility and overview frame geometry never cross; each head derives them from the severity literal and the named source key.
- Law: a cold materialization and a pool re-entry seal one control receipt, so a recycling regression reads as `controlType` against `intentKey`.
- Law: canonical pixel identity pins the kernel-framed preimage version, so a digest minted under the unframed prior preimage refuses at the column.
- Law: a record with a REFINED key closes through `Shape.Record`, so a refused key fails the decode instead of vanishing from the map.
- Law: the write disposition lands as the producer's closed FOUR-arm union, so a rejection, a rollback, and an indeterminate write stay distinct.
- Law: the live-wire trio decodes the producer's own smart-enum tokens for transport, state, direction, and echo class; none is minted at this end.
- Law: live-wire absence rides the UNDEFINED encoding, since the producer's merge omits an unset slot rather than null-filling it.
- Law: receipt kinds this page decodes against are literals, and a kind whose payload no branch decodes is named nowhere here.
- Law: ONE decoded fault observation serves every seam carrying one, so no arm re-declares it and none carries a code or a sentence beside it.
- Law: the command settlement closes FIVE arms; a rejection carries its observation, and a rollback and a compensation stay two reasons.
- Law: evidence arms carry their producer's own record, so no timeline slot decodes as an untyped payload.
- Law: an optional evidence column rides the branch option carrier, so no consumer asserts presence the decode already proved.
- Law: apphost and appui wire absence is OMISSION, settled once at the producer's merge, so no landing here binds a `null` token no minter writes.
- Law: the message plane's eight families decode as one cluster; a coordinate addressing a row crosses as decimal text and a magnitude as a number.
- Law: hop and delivery legs omit their attempt and elapsed readings TOGETHER, so one without the other names a dial that never happened.
- Law: the control family spells one roster per representation, since an omitted key lands as an option inside and as an absent column on the wire.
- Boundary: raw GeoJSON text and CloudEvents remain outside the registry because no typed family crosses.
- Boundary: a nested family member registers no census row, so an intent binding, a menu row, and the control receipt carry no gate or parity.

```typescript signature
import { VariantSchema } from "@effect/experimental"
import { Context, Layer } from "effect"

// The absence policy leads this section because every landing below reads it: `_absent` folds the producer's typed
// absence on a scalar string column once, since proto3 emits `""` for an unset singular string — a remote detail's
// `tenant`, a verdict's `variant`, an authored material's `emissionUnit`, an acquired set's `materialId`, and a
// dielectric's `conductor` all arrive empty and read as `Option.none()`. Declared below its first reader it sat in
// the temporal dead zone of every eagerly-evaluated `Schema.Class` above it. The shipped operator owns the fold; a
// local twin is the drift defect.
const _absent: typeof Schema.OptionFromNonEmptyTrimmedString = Schema.OptionFromNonEmptyTrimmedString

const _reasons = [
  "canceled", "unknown", "invalid", "deadline", "notfound", "exists", "denied", "exhausted",
  "precondition", "aborted", "range", "unimplemented", "internal", "unavailable", "dataloss", "unauthenticated",
] as const

// `class` is the branch taxonomy this roster ADOPTS; `code`, `retryable`, and `terminal` are the gRPC peer's OWN
// columns carried verbatim, and neither derives from the class beside it. `retryable` is the peer's re-send verdict
// and it diverges from `Fault.Class.retryable` at exactly ONE row — `exists` grades `conflicted`, whose branch band
// is `transient`, while an already-exists refusal never succeeds on a re-send — so folding the column into that
// projection would flip that row silently and stamp a re-drive an operator's own protocol forbids. `terminal` is a
// FAILOVER fact rather than a severity: it says this peer will refuse the call again, which is why four rows carry
// it where eleven carry a terminal class. `transport` is the fourth column and it exists so ONE table answers every
// code question: the egress fault below reads it instead of re-deriving a kind from bare code literals, which is
// the second code roster this column forecloses.
const _hopRows = {
  canceled: { code: 1, retryable: false, terminal: false, class: "defect", transport: "deadline" },
  unknown: { code: 2, retryable: false, terminal: false, class: "defect", transport: "connectivity" },
  invalid: { code: 3, retryable: false, terminal: false, class: "invalid", transport: "connectivity" },
  deadline: { code: 4, retryable: true, terminal: false, class: "expired", transport: "deadline" },
  notfound: { code: 5, retryable: false, terminal: false, class: "absent", transport: "connectivity" },
  exists: { code: 6, retryable: false, terminal: false, class: "conflicted", transport: "connectivity" },
  denied: { code: 7, retryable: false, terminal: true, class: "denied", transport: "connectivity" },
  exhausted: { code: 8, retryable: true, terminal: false, class: "exhausted", transport: "ceiling" },
  precondition: { code: 9, retryable: false, terminal: false, class: "invalid", transport: "connectivity" },
  aborted: { code: 10, retryable: true, terminal: false, class: "conflicted", transport: "connectivity" },
  range: { code: 11, retryable: false, terminal: false, class: "invalid", transport: "connectivity" },
  unimplemented: { code: 12, retryable: false, terminal: true, class: "defect", transport: "connectivity" },
  internal: { code: 13, retryable: false, terminal: false, class: "defect", transport: "connectivity" },
  unavailable: { code: 14, retryable: true, terminal: false, class: "unavailable", transport: "connectivity" },
  dataloss: { code: 15, retryable: false, terminal: true, class: "breached", transport: "connectivity" },
  unauthenticated: { code: 16, retryable: false, terminal: true, class: "denied", transport: "connectivity" },
} as const satisfies { readonly [K in (typeof _reasons)[number]]: {
  readonly code: number
  readonly retryable: boolean
  readonly terminal: boolean
  readonly class: Fault.Class.Kind
  readonly transport: TransportKind
} }
// ONE vocabulary over ONE roster. `Shape.vocabulary` already publishes every projection this plane spends — the
// literal schema, the row read, the guard, and the declaration order — so a second owner folded over the same rows
// to re-answer `class` was a mirror with nothing holding the two in step. `class` stays a COLUMN rather than a
// family mint because no refusal on this page raises a hop reason: `Remote` is the fault and the peer's code is its
// whole reason space, so a reason roster with a per-reason subject and renderer would have no raise to serve.
const _hopVocabulary = Shape.vocabulary(_reasons, _hopRows)

declare namespace Hops {
  type Reason = (typeof _reasons)[number]
  type Row = (typeof _hopRows)[Reason]
  type Shape = {
    readonly reasons: typeof _reasons
    readonly wire: typeof _hopVocabulary.schema
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
  wire: _hopVocabulary.schema,
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

const _WIRE_ATTR = { reason: "wire.reason", retryable: "wire.retryable", terminal: "wire.terminal" } as const

// The enricher seats on the ROSTER rather than on any fault class, because what it enriches is a capture band the
// producing surface already stamped with a reason token: it reads the roster's own columns and mints nothing, so a
// class carrying the same three columns beside it would be a second answer to one question.
const _EnricherLive: Layer.Layer<Fault.Enricher> = Layer.succeed(
  Fault.Enricher,
  Fault.Enricher.of({
    enrich: (capture) =>
      Effect.succeed(
        Option.match(Option.filter(Option.fromNullable(capture.attributes[_WIRE_ATTR.reason]), Hops.is), {
          onNone: () => capture,
          onSome: (reason) =>
            capture.enriched({
              [_WIRE_ATTR.reason]: reason,
              [_WIRE_ATTR.retryable]: Hops.at(reason).retryable,
              [_WIRE_ATTR.terminal]: Hops.at(reason).terminal,
            }),
        }),
      ),
  }),
)

const _ProtoTimestamp = Schema.Struct({
  seconds: Schema.BigIntFromSelf.pipe(Schema.betweenBigInt(-62135596800n, 253402300799n)),
  nanos: Schema.Int.pipe(Schema.between(0, 999999999)),
})
const _ProtoDuration = Schema.Struct({
  seconds: Schema.BigIntFromSelf.pipe(Schema.betweenBigInt(0n, 315576000000n)),
  nanos: Schema.Int.pipe(Schema.between(0, 999999999)),
})
const _Correlation = Schema.String.pipe(
  Schema.pattern(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i),
)
const _Recovery = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("terminal") }),
  Schema.Struct({ kind: Schema.Literal("transient") }),
  Schema.Struct({ kind: Schema.Literal("retryAfter"), delay: _ProtoDuration }),
)
// A remote fault's CLASS elects off the producer's own typed recovery arm and never off the domain code beside it.
// A code band reaching a class is a second answer to retriability that contradicts the arm the producer sent: an
// `unavailable` code arriving under a `terminal` arm graded `transient` through the hop roster and handed
// `Fault.Budget` a re-drive the peer had already refused, while `retryable` and `terminal` beside it read the arm
// and disagreed. The code stays IDENTITY and the arm stays RECOVERY. `retryAfter` lands `exhausted` because that is
// the one branch band whose producers state their own window and whose raise carries `Fault.Class.After`;
// `transient` lands `unavailable`, the transient band a caller re-invokes identically under the budget; `terminal`
// lands `invalid`, the terminal caller-blamed band whose re-offer route is the material the caller must re-author.
const _remoteClasses = {
  terminal: "invalid",
  transient: "unavailable",
  retryAfter: "exhausted",
} as const satisfies { readonly [K in (typeof _Recovery.Type)["kind"]]: Fault.Class.Kind }
// Generated oneof faces name the set arm on `case` and carry its message on `value`. Both verdict arms carry
// `google.protobuf.Empty`, a message whose whole content is its `$typeName` brand, so their payload stays DECLARED
// untyped rather than seated — this landing reads the arm name and never the arm's value, and a typed `Empty` shape
// would assert a generated spelling no catalog row publishes. `retryAfter` alone carries a schema, because
// `retryAfter` alone carries a value this end reads.
const _WireRecovery = Schema.Struct({
  kind: Schema.Union(
    Schema.Struct({ case: Schema.Literal("terminal"), value: Schema.Unknown }),
    Schema.Struct({ case: Schema.Literal("transient"), value: Schema.Unknown }),
    Schema.Struct({ case: Schema.Literal("retryAfter"), value: _ProtoDuration }),
  ),
})

// `FaultDetail` crosses SEVEN columns and this decode-only mirror carries all seven. `code` is the whole transported
// identity — the corpus contract compacted `package`, `case`, and `evidence` away at every producer and consumer in
// one move, so owner and case are LOCAL derivations off the code and no column here rehydrates the producer's own
// union. `tenant` takes `_absent`, because the empty string a proto3 singular column carries for an unset value
// reads `Option.none()` at ONE seat; `message` stays required, since a producer's blank message is an authored
// blank rather than an unstamped column.
class RemoteDetail extends Schema.Class<RemoteDetail>("RemoteDetail")({
  code: Schema.Int.pipe(Schema.positive()),
  message: Schema.String,
  correlation: _Correlation,
  hlcPhysical: _ProtoTimestamp,
  hlcLogical: Schema.BigIntFromSelf.pipe(Schema.betweenBigInt(0n, 18446744073709551615n)),
  tenant: _absent,
}) {}

// Slots run in the producer's own field order, so the mirror reads against the `.proto` top to bottom and an
// appended column lands at the tail rather than wherever a reader opened the file. Fields bind by NAME on both
// sides, so the concurrent proto renumbering moves no line here.
const _RemoteWire = Schema.Struct({
  code: RemoteDetail.fields.code,
  message: RemoteDetail.fields.message,
  correlation: RemoteDetail.fields.correlation,
  hlcPhysical: RemoteDetail.fields.hlcPhysical,
  hlcLogical: RemoteDetail.fields.hlcLogical,
  tenant: RemoteDetail.fields.tenant,
  recovery: _WireRecovery,
})

class Remote extends Schema.TaggedError<Remote>()("Remote", {
  detail: RemoteDetail,
  recovery: _Recovery,
}) {
  // Wire and detail differ by ONE column, so these arms move `recovery` and nothing else: every detail column is
  // one field declaration serving both sides, which is what makes a per-column copy unspellable rather than merely
  // absent. Growth lands on `RemoteDetail` alone and crosses both directions with no arm edited.
  static readonly FromWire: Schema.Schema<Remote, typeof _RemoteWire.Encoded> = Schema.transform(_RemoteWire, Remote, {
    strict: true,
    decode: ({ recovery, ...detail }) => new Remote({
      detail: new RemoteDetail(detail),
      recovery: recovery.kind.case === "retryAfter"
        ? { kind: "retryAfter", delay: recovery.kind.value }
        : { kind: recovery.kind.case },
    }),
    encode: (fault) => ({
      ...fault.detail,
      recovery: {
        kind: fault.recovery.kind === "retryAfter"
          ? { case: "retryAfter" as const, value: fault.recovery.delay }
          : { case: fault.recovery.kind, value: {} },
      },
    }),
  })
  // Both re-drive facts publish AS MEMBERS because the egress altitude grades this value STRUCTURALLY: the problem
  // ladder reads the pair off the instance by name and imports nothing from here, so a fact answered only by
  // reading `recovery` outside the class is a fact that altitude cannot reach. Complementary here and only here —
  // `recovery` is one closed three-arm union — while the ladder keeps them independent, because a foreign hop may
  // claim retryability on a verdict its own producer already called final.
  get retryable(): boolean {
    return this.recovery.kind !== "terminal"
  }
  get terminal(): boolean {
    return this.recovery.kind === "terminal"
  }
  get class(): Fault.Class.Kind {
    return _remoteClasses[this.recovery.kind]
  }
  override get message(): string {
    return `<remote:${this.detail.code}> ${this.detail.message}`
  }
}

const _transportKinds = ["connectivity", "deadline", "ceiling"] as const
// Both reads DERIVE off the one code roster: `kindOf` spends its `transport` column and `denied` its `class`, so a
// gRPC code is answered by the table that already rows it and never by a literal ladder beside it. A code no row
// names resolves through the roster's own `unknown` arm rather than through a fallback spelled here.
class Transport extends Schema.TaggedError<Transport>()("Transport", {
  kind: Schema.Literal(..._transportKinds),
  detail: Schema.String,
}) {
  static kindOf(code: number): TransportKind {
    return Hops.at(Hops.fromCode(code)).transport
  }
  static denied(code: number): boolean {
    return Hops.at(Hops.fromCode(code)).class === "denied"
  }
}

class MalformedDetail extends Schema.TaggedError<MalformedDetail>()("MalformedDetail", {
  detail: Schema.String,
}) {}

// The invocation boundary's three outcomes are one closed reason space, so the egress census mounts ONE roster and
// no consumer folds a fault instance into a label of its own. `Remote` contributes its recovery arms, `Transport`
// its kinds, and `MalformedDetail` its single terminal reason — the roster IS the sum, never a hand list beside it.
type InvokeFault = Remote | Transport | MalformedDetail
const _invokeReasons = [
  "remote-terminal", "remote-transient", "remote-retry-after",
  ..._transportKinds,
  "malformed-detail",
] as const
type InvokeReason = (typeof _invokeReasons)[number]
type TransportKind = (typeof _transportKinds)[number]

// The roster publishes as a VOCABULARY rather than the bare tuple it was: `Convention.tracked` and
// `Convention.outcome` take a `Convention.Census` — the ordered roster its own owner minted — so a duplicate word
// refuses at this mint and neither aspect re-proves membership at its own call. Rows carry nothing because the
// words ARE the contract here: order and membership are all a census aspect reads, exactly as the recovery,
// re-offer, and blame vocabularies at the fault floor declare theirs.
const _invokeCensus = Shape.vocabulary(_invokeReasons, {
  "remote-terminal": {},
  "remote-transient": {},
  "remote-retry-after": {},
  connectivity: {},
  deadline: {},
  ceiling: {},
  "malformed-detail": {},
})

const _invokeReason = (fault: InvokeFault): InvokeReason =>
  fault instanceof Remote
    ? fault.recovery.kind === "retryAfter" ? "remote-retry-after" : `remote-${fault.recovery.kind}`
    : fault instanceof Transport ? fault.kind : "malformed-detail"

// The producer's EIGHT frozen wire tokens, transcribed whole. Three lowercase reason vocabularies neighbour this
// column and none of them is it: `Convention.FlagReason` is the OpenTelemetry `feature_flag.result.reason` value
// family, which spells the targeting arm `targeting_match` and carries a ninth `stale` row; the OpenFeature SDK
// resolves its own uppercase constants; this roster is `FlagReason.Wire`, the column the producer's `[Mapper]`
// actually writes. Deriving this literal from the telemetry family would refuse every real `targeting` document and
// admit a `stale` token whose own producer declares no such row — the emitting producer owns the wire vocabulary.
const _flagReasons = ["static", "default", "targeting", "split", "cached", "disabled", "error", "unknown"] as const

// Four columns, field for field against the producer record. `value` is the evaluated BOOLEAN — the producer maps
// its own `Enabled` onto it, so a string or number here is a shape no minter writes. `variant` is REQUIRED: the
// producer's own absent arm is the named token `Variant.Absent` rather than a missing column, so the undefined-
// reading optional matched nothing this seam carries; `_absent` folds a structurally empty column from a foreign
// provider echo to `Option.none()` while a named variant — the absent arm included — lands as itself.
class FlagVerdict extends Schema.Class<FlagVerdict>("FlagVerdict")({
  flag: Schema.NonEmptyString,
  value: Schema.Boolean,
  variant: _absent,
  reason: Schema.Literal(..._flagReasons),
}) {}

// Absence OMITS on this producer and does not null-fill: the app-root merge declares `WhenWritingNull` and the
// resolver's own `OmitAbsent` modifier drops an unset `Option<T>` member outright, so every optional slot below
// reads the UNDEFINED encoding. A `NullOr` here would declare a token the merge posture guarantees never appears,
// and the empty-string fold `_absent` owns belongs to proto3 singular columns on another producer entirely.
const _omitted = <A, I, R>(value: Schema.Schema<A, I, R>) => Schema.optionalWith(value, { as: "Option" })

// NodaTime round-trip `Duration` text, not an ISO-8601 duration: the producer prints through the round-trip pattern
// and a consumer parsing this as ISO refuses every value. It lands as text because no branch owner decodes that
// grammar, and inventing one here would fork the temporal alphabet the producer's converter set already fixes.
const _elapsed = Schema.NonEmptyString

// The bounded fault observation the producer lowers every `Error` through, and the ONE compact fault shape this
// seam carries: `code` plus a typed recovery arm, with no package, family, band, offset, or case column — the same
// compaction `RemoteDetail` states above, reached here through the producer's own `AppHostFaultMap`. `causes` is a
// BOUNDED stamp chain and `truncated` says the chain was cut, so a reader knows whether it holds the whole cause
// list; folding that flag away would make a cut chain and a short one read alike.
const _FaultCause = Schema.Struct({
  code: _omitted(Schema.Int),
  exceptionType: _omitted(Schema.NonEmptyString),
  hResult: _omitted(Schema.Int),
})
const _FaultRecovery = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("terminal") }),
  Schema.Struct({ kind: Schema.Literal("transient") }),
  Schema.Struct({ kind: Schema.Literal("throttled"), retryAfter: _elapsed }),
)
const _FaultObservation = Schema.Struct({
  recovery: _FaultRecovery,
  causes: Schema.Array(_FaultCause),
  truncated: Schema.Boolean,
  code: _omitted(Schema.Int),
})

// The command deck's own settlement union, five arms on the producer's `[JsonDerivedType]` roster. `rejected`
// carries the STRUCTURED observation the deck lowered its refusal through, never a detail string beside a numeric
// code: a code is telemetry identity and a sentence is prose, and neither answers whether the refusal is worth
// re-offering — the recovery arm inside the observation does. `rolled-back` and `compensated` each carry the reason
// their own transaction settled under, and they stay two arms because an undone command and a compensated one owe
// the user different words. Both verdict arms carry nothing: a completed command IS its receipt.
const _CommandOutcome = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("completed") }),
  Schema.Struct({ kind: Schema.Literal("cancelled") }),
  Schema.Struct({ kind: Schema.Literal("rejected"), fault: _FaultObservation }),
  Schema.Struct({ kind: Schema.Literal("rolled-back"), reason: Schema.NonEmptyString }),
  Schema.Struct({ kind: Schema.Literal("compensated"), reason: Schema.NonEmptyString }),
)

// The deck receipt and the native-asset fact are nested family members with no census row of their own, landed
// here because the evidence timeline's own arms carry them: an untyped `Unknown` in those two slots asserted that
// no producer shape existed, where both are declared records on the pages this page already mirrors.
const _DeckReceipt = Schema.Struct({
  key: Schema.NonEmptyString,
  surface: Schema.NonEmptyString,
  elapsed: _elapsed,
  outcome: _CommandOutcome,
  payloadDigest: Schema.NonEmptyString,
  correlation: Schema.NonEmptyString,
})
const _NativeAssetFact = Schema.Struct({
  library: Schema.NonEmptyString,
  version: Schema.NonEmptyString,
  path: Schema.NonEmptyString,
  rid: Schema.NonEmptyString,
})

// The version literal names a PREIMAGE LAW, never a field roster: the producer keys the plane through the kernel's
// canonical framing — the length-framed version string, each extent as a little-endian ordinal, then the tight
// top-left RGBA plane as the trailing raw leaf whose extent the two ordinals already recover. The v1 literal named
// the same pixels under an UNFRAMED version prefix, so the two versions key one plane to two digests and a decoder
// pinning the older literal admits a hash from a preimage no producer writes. Pinning the literal is what makes that
// a refusal at the column rather than a silent parity pass on a digest nothing can reproduce.
const _PixelIdentity = Schema.Struct({
  version: Schema.Literal("rgba8-srgb-straight-top-left-v2"),
  width: Shape.Refined.OrdinalKey.pipe(Schema.positive()),
  height: Shape.Refined.OrdinalKey.pipe(Schema.positive()),
  hash: Digest.codecs.content.wire,
})
const _EvidenceReceipt = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("surface"), host: Schema.String, descriptor: Schema.String, scale: Schema.Number, at: Schema.DateTimeUtc, correlation: Schema.String, handle: _omitted(Schema.NonEmptyString) }),
  Schema.Struct({ kind: Schema.Literal("focus"), target: Schema.String, focused: Schema.Boolean }),
  Schema.Struct({ kind: Schema.Literal("render"), slot: Schema.String, format: Schema.String, frameHash: Schema.String, drawHash: _omitted(Schema.NonEmptyString), pixels: _omitted(_PixelIdentity), bytes: Schema.String.pipe(Schema.pattern(/^\d+$/)), elapsed: _elapsed, colorSpace: Schema.String, destination: _omitted(Schema.NonEmptyString) }),
  Schema.Struct({ kind: Schema.Literal("disposal"), screenId: Schema.String, active: Schema.String, disposables: Schema.Int }),
  Schema.Struct({ kind: Schema.Literal("edit"), slot: Schema.String, surface: Schema.String, target: Schema.String, editor: Schema.String, outcome: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("command"), receipt: _DeckReceipt }),
  Schema.Struct({ kind: Schema.Literal("native-asset"), fact: _NativeAssetFact }),
  Schema.Struct({ kind: Schema.Literal("theme"), variant: Schema.String, density: Schema.String, trigger: Schema.String, changedKeys: Schema.Int }),
  Schema.Struct({ kind: Schema.Literal("motion"), token: Schema.String, resolved: Schema.String, reduced: Schema.Boolean }),
  Schema.Struct({ kind: Schema.Literal("effect"), plane: Schema.String, key: Schema.String, outcome: Schema.String, flag: Schema.Boolean, count: Schema.Int, magnitude: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("asset"), key: Schema.String, assetKind: Schema.String, origin: Schema.String, scale: Schema.Number, contentHash: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("live-data"), slot: Schema.String, adds: Schema.Int, updates: Schema.Int, removes: Schema.Int, refreshes: Schema.Int }),
  Schema.Struct({ kind: Schema.Literal("collab-sync"), docKey: Schema.String, deltas: Schema.Int, bytes: Schema.String, pending: Schema.Int, applied: Schema.Boolean }),
  Schema.Struct({ kind: Schema.Literal("collab-revert"), docKey: Schema.String, frontierDigest: Schema.String, inverseOps: Schema.Int }),
  // `outcome` and `fault` are COMPLEMENTARY by the producer's own projection — a ready media outcome lowers to no
  // observation and a failed one always carries its error — so the pair is one fact read two ways and a `failed`
  // arriving without a fault is a producer defect this shape makes visible instead of a numeric code standing in.
  Schema.Struct({ kind: Schema.Literal("media"), key: Schema.String, codec: Schema.String, source: Schema.String, outcome: Schema.Literal("ready", "failed"), fault: _omitted(_FaultObservation) }),
  Schema.Struct({ kind: Schema.Literal("quality"), tier: Schema.String, pathTraceSamples: Schema.Int, watermarkFactor: Schema.Number, motion: Schema.String, foveationLevel: Schema.Int, refreshHz: Schema.Number }),
  Schema.Struct({ kind: Schema.Literal("gpu-frame"), frameOrdinal: Schema.String, passes: Schema.Int, unmeasured: Schema.Int, measuredNanoseconds: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("layout"), panel: Schema.String, constraints: Schema.Int, elapsed: _elapsed, fault: _omitted(_FaultObservation) }),
  Schema.Struct({ kind: Schema.Literal("dispatcher-lag"), boundary: Schema.String, elapsed: Schema.String }),
  Schema.Struct({ kind: Schema.Literal("collab-precommit"), docKey: Schema.String, lamport: Schema.Int, ops: Schema.Int, origin: Schema.String, message: _omitted(Schema.NonEmptyString) }),
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
  static readonly Command: typeof _DeckReceipt = _DeckReceipt
  static readonly Outcome: typeof _CommandOutcome = _CommandOutcome
  static readonly NativeAsset: typeof _NativeAssetFact = _NativeAssetFact
  static readonly Fault: typeof _FaultObservation = _FaultObservation
  static readonly Pixel: typeof _PixelIdentity = _PixelIdentity
}

// The live-wire trio decodes the producer's OWN rosters, each one the smart-enum `Key` its row carries rather than a
// vocabulary this end invented: eleven transports, five lifecycle states, three directions, four echo classes. An
// ordinal-keyed enum crossing this seam is the named producer violation, so every one of these is a token.
const _transports = [
  "opc-ua", "opc-ua-pubsub", "modbus", "mqtt", "serial", "bacnet",
  "mtconnect", "rest", "graphql", "spreadsheet", "erp-plm",
] as const
const _bindingStates = ["connecting", "subscribed", "polling", "stale", "faulted"] as const
const _bindingDirections = ["inbound", "outbound", "bidirectional"] as const
const _echoKinds = ["absent", "stamped", "tokened", "slotted"] as const

// The write disposition is a CLOSED four-arm union the producer projects once and this end reconstructs on the
// `kind` literal — never re-minted branch-side, and never flattened to a boolean. The two fault-bearing arms are
// what make the flattening a defect: `rejected` says the edge refused and the prior value stands, while
// `indeterminate` says the attempt AND its rollback both failed, so the external value is unknown to this process.
// A success flag fuses those two with `rolled-back`, whose prior value IS recoverable, and a dashboard would show
// one repair for three unlike states. `acknowledged` carries the echo CLASS its own arm proved, so the strength of
// the acknowledgement — a bare ack, a stamp, a token, a slot read-back — crosses beside the fact of it.
const _WriteBack = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("acknowledged"), echo: Schema.Literal(..._echoKinds) }),
  Schema.Struct({ kind: Schema.Literal("rejected"), fault: _FaultObservation }),
  Schema.Struct({ kind: Schema.Literal("rolled-back"), priorValue: Schema.Number }),
  Schema.Struct({
    kind: Schema.Literal("indeterminate"),
    attempt: _FaultObservation,
    rollback: _FaultObservation,
  }),
)

class BindingStatus extends Schema.TaggedClass<BindingStatus>()("BindingStatus", {
  bindingId: Schema.NonEmptyString,
  transport: Schema.Literal(..._transports),
  state: Schema.Literal(..._bindingStates),
  direction: Schema.Literal(..._bindingDirections),
  lastGoodAt: _omitted(Schema.DateTimeUtc),
}) {
  static readonly FromWire: Schema.Schema<BindingStatus, unknown> = Schema.compose(_stamp("BindingStatus"), BindingStatus, { strict: false })
}
// The coercion crosses as the CANONICAL value beside both units, never as an offered/landed pair of opaque values:
// what a reader needs is the magnitude it must render and the unit the source published it under, and the seam's
// conversion receipt stays at the producer where the evidence lives.
class CoercedValue extends Schema.TaggedClass<CoercedValue>()("CoercedValue", {
  // The KEY the value was coerced for, projected at the producer's mapper off the binding spec rather than stored on
  // the coercion record — the domain value stays identity-free and the wire carries the identity a consumer needs.
  // Without it a decoded coercion could be attributed to nothing: the push envelope's descriptor addresses the
  // invocation and never enters the serialized element, so this column is what lets the value key a board slot.
  bindingId: Schema.NonEmptyString,
  canonical: Schema.Number,
  canonicalUnit: Schema.NonEmptyString,
  sourceUnit: Schema.NonEmptyString,
  sourceAt: Schema.DateTimeUtc,
}) {
  static readonly FromWire: Schema.Schema<CoercedValue, unknown> = Schema.compose(_stamp("CoercedValue"), CoercedValue, { strict: false })
}
// `rendered` and `renderedUnit` cross as a PAIR or not at all — they are the value as the external edge received
// it, absent wherever the write needed no rendering — so a reader holding one without the other reads a write that
// never happened.
class WriteReceipt extends Schema.TaggedClass<WriteReceipt>()("WriteReceipt", {
  bindingId: Schema.NonEmptyString,
  canonical: Schema.Number,
  rendered: _omitted(Schema.Number),
  renderedUnit: _omitted(Schema.NonEmptyString),
  disposition: _WriteBack,
  elapsed: _elapsed,
  correlation: Schema.NonEmptyString,
}) {
  static readonly Disposition: typeof _WriteBack = _WriteBack
  static readonly Fault: typeof _FaultObservation = _FaultObservation
  static readonly FromWire: Schema.Schema<WriteReceipt, unknown> = Schema.compose(_stamp("WriteReceipt"), WriteReceipt, { strict: false })
}

// The receipt kinds these families ride under, as LITERALS. The envelope's own `kind` column stays a free string
// because that roster is the producing package's and open across packages, but the kinds THIS page decodes against
// are closed and named here, so a consumer routing an envelope to a landing matches a token this page declares
// rather than one it spells at the call. Two kinds, not one: a committed transition and a committed write are
// different documents, and a reader keying both on one token cannot route them. The producer's third live-wire
// kind, `wire-rejection`, is deliberately ABSENT — it carries `WireRejectionWire`, an in-process refusal receipt
// whose producer, decoder, and readers are all C#, so naming it here would assert a decoder no branch declares.
const _receiptKinds = {
  BindingStatusWire: "wire-status",
  WriteReceiptWire: "write-back",
} as const satisfies { readonly [K in "BindingStatusWire" | "WriteReceiptWire"]: string }

// The AppHost MESSAGE PLANE: eight families the outbound hop, the topic bus, and the outbox pump mint, landed as
// one cluster because they share three narrowings and one absence law. Every optional slot OMITS — the producer's
// merge encodes absence once at `SuiteContracts` and every peer face spells `field?: T` — so a `null` here would
// bind a token no minter writes. A coordinate that ADDRESSES a row crosses as a decimal STRING, because a `ulong`
// past 2^53 loses precision in a JSON number and exact comparison is the whole purpose of an ordinal; a lag, an
// age, or a tally is a magnitude no reader compares for identity and stays a number.
const _ordinalText = Schema.String.pipe(Schema.pattern(/^\d+$/))
const _count = Schema.Int.pipe(Schema.nonNegative())
const _hopOutcomes = ["delivered", "refused", "faulted"] as const
const _dialDispositions = ["dialed", "suppressed", "unbound"] as const
const _outboxDispositions = ["pending", "deferred", "dead-lettered"] as const
const _dropClasses = ["shed", "missed"] as const

// `attempts` and `elapsedSeconds` are ABSENT TOGETHER on any leg that reached no pipeline. They stay two flat
// columns because the producer emits two flat columns, so a nested cell here would decode nothing it sends; the
// pairing is the invariant a reader honours, and one column present without the other names a producer that
// reported a dial which never happened. `fault` is complementary to `outcome`: `delivered` omits it, and the two
// failing arms carry the exact observation the hop lowered.
class HopReceipt extends Schema.Class<HopReceipt>("HopReceipt")({
  hop: Schema.NonEmptyString,
  outcome: Schema.Literal(..._hopOutcomes),
  attempts: _omitted(_count),
  elapsedSeconds: _omitted(Schema.Number),
  breaker: _omitted(Schema.NonEmptyString),
  fault: _omitted(_FaultObservation),
}) {}

// The dedupe watermark is an HLC ORDINAL and takes the same decimal-text narrowing its three outbox siblings take:
// it can exceed 2^53, where a JSON number loses precision and exact comparison is the whole purpose of an ordinal.
// Reading it as a magnitude would have admitted a watermark that compares equal to its own neighbour.
class DeliveryReceipt extends Schema.Class<DeliveryReceipt>("DeliveryReceipt")({
  channel: Schema.NonEmptyString,
  outcome: Schema.Literal(..._hopOutcomes),
  disposition: Schema.Literal(..._dialDispositions),
  attempts: _omitted(_count),
  elapsedSeconds: _omitted(Schema.Number),
  watermark: _omitted(_ordinalText),
  fault: _omitted(_FaultObservation),
}) {}

// `first` and `last` bound the dropped RUN and `count` measures it, so a shed burst and a missed one are one row
// with a span rather than a receipt per lost message; `resent` says the pump re-drove that span.
class DropReceipt extends Schema.Class<DropReceipt>("DropReceipt")({
  topic: Schema.NonEmptyString,
  subscription: _omitted(Schema.NonEmptyString),
  class: Schema.Literal(..._dropClasses),
  first: _count,
  last: _count,
  count: _count,
  resent: Schema.Boolean,
}) {}

class OutboxRow extends Schema.Class<OutboxRow>("OutboxRow")({
  topic: Schema.NonEmptyString,
  dedupKey: Schema.NonEmptyString,
  disposition: Schema.Literal(..._outboxDispositions),
  attempt: _count,
  ordinal: _ordinalText,
  physical: Schema.NonEmptyString,
  at: _omitted(Schema.DateTimeUtc),
  // absent while the row is pending, since a row that has not failed has nothing to observe
  fault: _omitted(_FaultObservation),
  // the two W3C members cross under their own names, so a board row deep-links to the producing trace and an
  // unlistened producer omits both rather than carrying a trace id no backend resolves
  traceParent: _omitted(Schema.NonEmptyString),
  traceState: _omitted(Schema.NonEmptyString),
}) {}

// `fault` is REQUIRED here and optional on the row above, and the split is the table's own meaning: a letter
// reaches this table only by failing, so a dead letter carrying no observation names a producer that lost its own
// evidence. The content key addresses the letter the pump replays, so it lands on the branch's content-key brand
// rather than as loose text.
class DeadLetterRow extends Schema.Class<DeadLetterRow>("DeadLetterRow")({
  contentKey: Digest.codecs.content.wire,
  sink: Schema.NonEmptyString,
  ordinal: _ordinalText,
  fault: _FaultObservation,
  attempts: _count,
  at: Schema.DateTimeUtc,
}) {}

class ReplayTally extends Schema.Class<ReplayTally>("ReplayTally")({
  delivered: _count,
  held: _count,
  dead: _count,
}) {}

// The lane holds a census row of its own AND rides the sweep's roster, so it is declared once and composed: the
// sweep's `lanes` column is the same shape a lane row decodes to, and a second declaration would let the two drift.
const _OutboxLane = Schema.Struct({
  topic: Schema.NonEmptyString,
  lag: _count,
  oldestAgeSeconds: Schema.Number,
})
class OutboxLane extends Schema.Class<OutboxLane>("OutboxLane")(_OutboxLane.fields) {}

class OutboxSweep extends Schema.Class<OutboxSweep>("OutboxSweep")({
  lag: _count,
  oldestAgeSeconds: Schema.Number,
  watermark: _ordinalText,
  relayed: _count,
  duplicates: _count,
  deferred: _count,
  at: Schema.DateTimeUtc,
  lanes: Schema.Array(_OutboxLane),
}) {}
// This verdict is the SHELL's, materialized at `ui/viewer/panel` as a slot on the board row its `key` addresses,
// never the palette's: palette legality is this branch's own `Overlay.Grant` set, derived per render at
// `ui/view/overlay` off grants nothing here publishes. What crosses is the producer's degradation `level`, which
// adopts `Evidence.Availability`'s level vocabulary through that owner's own field without sharing its document, so
// a producer level added at core breaks the consuming degradation table at its declaration rather than at a render.
class CommandGate extends Schema.TaggedClass<CommandGate>()("CommandGate", {
  key: Schema.NonEmptyString,
  available: Schema.Boolean,
  level: Evidence.Availability.fields.level,
}) {
  static readonly FromWire: Schema.Schema<CommandGate, unknown> = Schema.compose(_stamp("CommandGate"), CommandGate, { strict: false })
}

// The deck ROW a head mounts a verb from, decode-only and column for column against the producer record. Every
// column is STATIC: the producer's availability delegate never crosses, so `requires` names the capability keys the
// gate READ and never a verdict — the verdict rides `CommandGateWire` beside it, and a head re-running the
// predicate against a capability set it does not hold is the second availability algebra this seam forecloses.
// `targets` names the palette KINDS a verb acts on as a contextual action, keys rather than rows, because the set
// is what crosses. `arguments` names the form schema a parameterized verb collects through, and that schema's own
// key IS this row's key by construction, so the column carries a key rather than a nested document. Both absent
// columns take the NULL encoding the producer's own absence mappers write. A nested member registers no census row.
class CommandRow extends Schema.Class<CommandRow>("CommandRow")({
  key: Schema.NonEmptyString,
  scope: Schema.Literal("global", "screen", "viewport", "dialog"),
  requires: Schema.Array(Schema.NonEmptyString),
  gesture: Schema.OptionFromNullOr(Schema.NonEmptyString),
  targets: Schema.Array(Schema.NonEmptyString),
  arguments: Schema.OptionFromNullOr(Schema.NonEmptyString),
}) {}

const _Vec3 = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number)

// AppUi's shell publishes this widget vocabulary: thirty-one locked kind literals, each arm carrying its typed shape beside the
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
  pending: _omitted(Schema.NonEmptyString),
})

const _HintRow = Schema.Struct({ body: Schema.String, gesture: _omitted(Schema.NonEmptyString) })

// `role` is the producer's `PaintRole` key — a growable theme roster each head reads as a style class, so it stays
// an open key where every closed producer table below decodes as its own literal union; no automation-name column
// crosses, because both heads derive the announced name from `key` through their own locale resolver.
const _Binding = Schema.Struct({
  role: Schema.NonEmptyString,
  emphasis: _Emphasis,
  command: _omitted(Schema.NonEmptyString),
  valueKey: _omitted(Schema.NonEmptyString),
  trigger: _omitted(Schema.Literal("activate", "change", "commit")),
  icon: _omitted(_IconSlot),
  hint: _omitted(_HintRow),
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
  group: _omitted(Schema.NonEmptyString),
  icon: _omitted(_IconSlot),
})

const _OptionSource = Schema.Union(
  Schema.Struct({ form: Schema.Literal("inline"), rows: Schema.Array(_OptionRow) }),
  Schema.Struct({ form: Schema.Literal("bound"), sourceKey: Schema.NonEmptyString }),
)

const _CrumbRow = Schema.Struct({
  value: Schema.NonEmptyString,
  labelKey: Schema.NonEmptyString,
  icon: _omitted(_IconSlot),
  command: _omitted(Schema.NonEmptyString),
})

const _AvatarRow = Schema.Struct({ labelKey: Schema.NonEmptyString, portrait: _omitted(Schema.NonEmptyString) })

// pattern lists land non-empty because the producer's own filter encoder refuses an empty one before the
// picker mounts, so the landing states the emission's shape rather than admitting a document it never writes
const _FileFilterRow = Schema.Struct({ label: Schema.String, patterns: Schema.NonEmptyArray(Schema.NonEmptyString) })

// a menu row is a ROW one level down, never a child intent, so its recursion closes on itself; every column is
// ABSENCE is the second axis this family spells over, and it is what ended the representation invariance the row
// once had: under the producer's settled encoding an unset key is OMITTED, so the wire side of every optional
// column is `A | undefined` while the landed side is the branch's own `Option`. One mode parameter carries that
// split, so each roster is still declared ONCE and instantiated per representation rather than transcribed twice.
// `_Held` applies the split to a column and `_Of` to a whole nested schema, which is what lets a row hold an icon
// slot whose own columns split the same way.
type _Mode = "type" | "encoded"
type _Held<M extends _Mode, A> = M extends "type" ? Option.Option<A> : A | undefined
type _Of<M extends _Mode, S extends Schema.Schema.Any> = M extends "type" ? Schema.Schema.Type<S>
  : Schema.Schema.Encoded<S>

interface _MenuRowOf<M extends _Mode> {
  readonly key: string
  readonly labelKey: string
  readonly posture: "command" | "check" | "radio" | "separator"
  readonly icon: _Held<M, _Of<M, typeof _IconSlot>>
  readonly gesture: _Held<M, string>
  readonly command: _Held<M, string>
  readonly checkedKey: _Held<M, string>
  readonly rows: ReadonlyArray<_MenuRowOf<M>>
}

const _MenuRow: Schema.Schema<_MenuRowOf<"type">, _MenuRowOf<"encoded">> = Schema.Struct({
  key: Schema.NonEmptyString,
  labelKey: Schema.NonEmptyString,
  posture: Schema.Literal("command", "check", "radio", "separator"),
  icon: _omitted(_IconSlot),
  gesture: _omitted(Schema.NonEmptyString),
  command: _omitted(Schema.NonEmptyString),
  checkedKey: _omitted(Schema.NonEmptyString),
  rows: Schema.Array(
    Schema.suspend((): Schema.Schema<_MenuRowOf<"type">, _MenuRowOf<"encoded">> => _MenuRow),
  ),
})

// Leaf arms close the family: twenty-one shapes bottom out, so both representations DERIVE from the union
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
    from: _omitted(_PlainDate),
    until: _omitted(_PlainDate),
    upperKey: _omitted(Schema.NonEmptyString),
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
  Schema.Struct({ kind: Schema.Literal("progress"), key: Schema.NonEmptyString, form: Schema.Literal("bar", "ring", "skeleton"), fraction: _omitted(Schema.Number), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("avatar"), key: Schema.NonEmptyString, members: Schema.Array(_AvatarRow), visible: Schema.Int.pipe(Schema.nonNegative()), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("breadcrumb"), key: Schema.NonEmptyString, crumbs: Schema.Array(_CrumbRow), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("tooltip"), key: Schema.NonEmptyString, hint: _HintRow, binding: _Binding }),
  // The minimap strip bottoms out: it names its frame producer and its jump verb by KEY and mounts no child intent,
  // so the recursion never enters it and the walk projection answers zero children by construction rather than by a
  // roster row. Carrying the frame geometry here instead would put a viewport stream on a control declaration.
  Schema.Struct({ kind: Schema.Literal("overview"), key: Schema.NonEmptyString, axis: Schema.Literal("vertical", "horizontal", "plane"), sourceKey: Schema.NonEmptyString, jumpCommand: Schema.NonEmptyString, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("menu"), key: Schema.NonEmptyString, rows: Schema.Array(_MenuRow), binding: _Binding }),
)

// Nesting spells ONCE and instantiates per representation: the child type is the only axis that
// moves, because `_Binding`, `_Window`, and the extent/align columns are representation-invariant by
// construction — every one of their columns is a string, a literal, or a nullable of one.
type _BindingOf<M extends _Mode> = _Of<M, typeof _Binding>
type _WindowRow = typeof _Window.Type

type _ColumnOf<M extends _Mode, T> = {
  readonly headerKey: string
  readonly cell: T
  readonly editor: _Held<M, T>
  readonly extent: { readonly value: number; readonly unit: "auto" | "pixel" | "star" | "sizeToCells" | "sizeToHeader" }
  readonly sortKey: _Held<M, string>
  readonly align: "Left" | "Center" | "Right" | "Stretch"
}

type _Nest<M extends _Mode, T> =
  | {
    readonly kind: "banner"
    readonly key: string
    readonly headlineKey: string
    readonly bodyKey: string
    readonly severity: "information" | "success" | "warning" | "error"
    readonly placement: "page" | "section"
    readonly actions: ReadonlyArray<T>
    readonly evidence: _Held<M, T>
    readonly binding: _BindingOf<M>
  }
  | { readonly kind: "emptyState"; readonly key: string; readonly headlineKey: string; readonly bodyKey: string; readonly action: _Held<M, T>; readonly binding: _BindingOf<M> }
  | { readonly kind: "grid"; readonly key: string; readonly columns: ReadonlyArray<_ColumnOf<M, T>>; readonly window: _WindowRow; readonly binding: _BindingOf<M> }
  | { readonly kind: "tree"; readonly key: string; readonly item: T; readonly expansionCommand: string; readonly window: _WindowRow; readonly binding: _BindingOf<M> }
  | { readonly kind: "toolbar"; readonly key: string; readonly rows: ReadonlyArray<{ readonly item: T; readonly overflow: "AsNeeded" | "Always" | "Never" }>; readonly orientation: "Horizontal" | "Vertical"; readonly binding: _BindingOf<M> }
  | { readonly kind: "tab"; readonly key: string; readonly pages: ReadonlyArray<{ readonly headerKey: string; readonly body: T }>; readonly binding: _BindingOf<M> }
  | { readonly kind: "accordion"; readonly key: string; readonly sections: ReadonlyArray<{ readonly headerKey: string; readonly body: T }>; readonly binding: _BindingOf<M> }
  | { readonly kind: "panel"; readonly key: string; readonly children: ReadonlyArray<T>; readonly constraintProgram: string; readonly binding: _BindingOf<M> }
  | { readonly kind: "dock"; readonly key: string; readonly regions: ReadonlyArray<T>; readonly constraintProgram: string; readonly binding: _BindingOf<M> }
  | { readonly kind: "splitter"; readonly key: string; readonly first: T; readonly second: T; readonly orientation: "Horizontal" | "Vertical"; readonly binding: _BindingOf<M> }

type ControlIntent = typeof _leaves.Type | _Nest<"type", ControlIntent>
type ControlIntentWire = typeof _leaves.Encoded | _Nest<"encoded", ControlIntentWire>

const _child: Schema.Schema<ControlIntent, ControlIntentWire> = Schema.suspend(() => ControlIntent)

const _Column = Schema.Struct({
  headerKey: Schema.NonEmptyString,
  cell: _child,
  editor: _omitted(_child),
  extent: Schema.Struct({ value: Schema.Number, unit: Schema.Literal("auto", "pixel", "star", "sizeToCells", "sizeToHeader") }),
  sortKey: _omitted(Schema.NonEmptyString),
  align: Schema.Literal("Left", "Center", "Right", "Stretch"),
})

const _Section = Schema.Struct({ headerKey: Schema.NonEmptyString, body: _child })

const ControlIntent: Schema.Schema<ControlIntent, ControlIntentWire> = Schema.Union(
  _leaves,
  // The condition strip NESTS: its verbs and its evidence are child intents, so a retry button inside a banner and
  // one inside a form decode through this same union and resolve their command keys against the same deck. A
  // banner-local verb roster would be a second availability algebra over one command vocabulary. `severity` carries
  // the producer's four-row ladder and `placement` its two; DISMISSIBILITY is a producer column that never crosses,
  // so each head resolves it from the severity literal rather than from a bit the wire would let drift.
  Schema.Struct({ kind: Schema.Literal("banner"), key: Schema.NonEmptyString, headlineKey: Schema.NonEmptyString, bodyKey: Schema.NonEmptyString, severity: Schema.Literal("information", "success", "warning", "error"), placement: Schema.Literal("page", "section"), actions: Schema.Array(_child), evidence: _omitted(_child), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("emptyState"), key: Schema.NonEmptyString, headlineKey: Schema.NonEmptyString, bodyKey: Schema.NonEmptyString, action: _omitted(_child), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("grid"), key: Schema.NonEmptyString, columns: Schema.Array(_Column), window: _Window, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("tree"), key: Schema.NonEmptyString, item: _child, expansionCommand: Schema.NonEmptyString, window: _Window, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("toolbar"), key: Schema.NonEmptyString, rows: Schema.Array(Schema.Struct({ item: _child, overflow: Schema.Literal("AsNeeded", "Always", "Never") })), orientation: _Orientation, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("tab"), key: Schema.NonEmptyString, pages: Schema.Array(_Section), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("accordion"), key: Schema.NonEmptyString, sections: Schema.Array(_Section), binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("panel"), key: Schema.NonEmptyString, children: Schema.Array(_child), constraintProgram: Schema.NonEmptyString, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("dock"), key: Schema.NonEmptyString, regions: Schema.Array(_child), constraintProgram: Schema.NonEmptyString, binding: _Binding }),
  Schema.Struct({ kind: Schema.Literal("splitter"), key: Schema.NonEmptyString, first: _child, second: _child, orientation: _Orientation, binding: _Binding }),
)

// The materialization face of the intent family, decode-only and column for column against the producer record. A
// COLD fold and a pool re-entry seal the SAME row, which is the whole point of the shape: a recycling regression
// shows as a receipt whose `controlType` disagrees with its `intentKey`, and that comparison only holds while both
// columns cross verbatim — a receipt carrying a re-entry marker would let the two states be told apart at the reader
// and the regression would stop showing. `kind` is the producer's own constant column rather than a census
// discriminant, because this face is a `ReceiptEnvelopeWire` PAYLOAD: it names itself across a seam whose envelope
// `kind` is a free string, so the landing pins the literal instead of trusting the envelope's word for it. Nested
// family members register no `_families` row, so no census entry, contract gate, or parity obligation crosses here.
class ControlReceipt extends Schema.Class<ControlReceipt>("ControlReceipt")({
  kind: Schema.Literal("control"),
  intentKey: Schema.NonEmptyString,
  controlType: Schema.NonEmptyString,
  // the producer's `Option<string>` command key crosses through its own `Absent` mapper as an explicit null, so the
  // landing reads the NULL encoding — the empty-string form `_absent` folds belongs to proto3 singular columns
  command: Schema.OptionFromNullOr(Schema.NonEmptyString),
  emphasis: _Emphasis,
  at: Schema.DateTimeUtc,
}) {}

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

type _PredicateNest<T> =
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

type PredicateWire = typeof _predicateLeaves.Type | _PredicateNest<PredicateWire>

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
  // The producer DERIVES this column off its own `CaptureMethod`, and that column crosses beside it as `method`, so
  // the two states a lone bool would fuse stay separable at this end: a blank `method` beside `false` is a receipt
  // that recorded no capture method at all, a named one beside `false` is a method that measures nothing. No third
  // posture token belongs here — it would restate what the pair already carries.
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
// to its reading position decodes a neighbour's value silently. Every record below is therefore one `_keyed` ROSTER
// in Key order — the producer's index list and its column names as one declaration — so appending Key(31) is one
// roster row rather than a tuple slot, a decode line, an encode line, and a comment ladder that can disagree. What
// survives the roster is the TREE: `PbrGroups` bands thirty-one sibling slots into nine groups and `_Shade` folds
// three sibling channels into one triple, so the two reshape arms carry that nesting and nothing else, reading the
// roster's own column names. Every refinement re-proves on the named class after the mapping runs, exactly once.
const _ColorWire = _keyed("WireColor", [
  ["r", Schema.Number], ["g", Schema.Number], ["b", Schema.Number], // Key 0..2: scene-linear channels
  ["hex", Schema.String], // Key 3: the clipped hex the web swatch reads
])
const _Shade = Schema.Struct({ rgb: _Color, hex: Schema.String })
const _shaded = ({ r, g, b, hex }: typeof _ColorWire.Type): typeof _Shade.Encoded => ({ rgb: [r, g, b], hex })
const _unshaded = ({ rgb: [r, g, b], hex }: typeof _Shade.Encoded): typeof _ColorWire.Type => ({ r, g, b, hex })

// `WireProvenance` Key(0..16). Column names match `_Provenance` one for one, so the receipt crosses the seam with
// NO reshape arm at all; slot 15 is the producer's `double?` nil, decoded by the SAME `NullOr` the named field
// declares, which is the one column where wire and interior disagree about presence rather than about spelling.
const _ProvenanceWire = _keyed("WireProvenance", [
  ["device", Schema.String], ["wavelengthCount", Schema.Number], ["fitResidual", Schema.Number],
  ["measured", Schema.Boolean], ["method", Schema.String], ["angularSamples", Schema.Number],
  ["fitConditionNumber", Schema.Number], ["fitRank", Schema.Number], ["dominantWavelengthNm", Schema.Number],
  ["excitationPurity", Schema.Number], ["cctKelvin", Schema.Number], ["cctDuv", Schema.Number],
  ["modelCard", Schema.String], ["license", Schema.String], ["calibrated", Schema.Boolean],
  ["calibrationDeltaE", Schema.NullOr(Schema.Number)], ["modelArtefact", Schema.String],
])

// `OpenPbrGroupsWire` Key(0..30) — the FULL OpenPBR Surface 1.1 parameter vector, one-for-one: the producer
// flattens `OpenPbrSurface` so a peer reconstructs the exact slab stack, never a lossy subset, and this mirror
// carries every band — subsurface, coat, fuzz, and thin-film included — because a dropped band repaints the
// producer's surface silently. The vector crosses NESTED at `MaterialWire` Key(1); the standalone census row binds
// this same declaration so the nested slot and the family row cannot drift. The two appended slots ride
// `Schema.optionalElement`, so pre-append bytes land `Option.none()` and the arm below supplies the producer's own
// stated default at the one site that reads the column.
const _PbrVector = _keyed("OpenPbrGroupsWire", [
  ["baseWeight", Schema.Number], ["baseColor", _ColorWire], ["baseMetalness", Schema.Number],
  ["baseDiffuseRoughness", Schema.Number], ["baseSpecularTint", Schema.Number],
  ["specularWeight", Schema.Number], ["specularColor", _ColorWire], ["specularRoughness", Schema.Number],
  ["specularIor", Schema.Number], ["specularAnisotropy", Schema.Number],
  ["transmissionWeight", Schema.Number], ["transmissionRoughness", Schema.Number],
  // mean-free-path scalars, never unit-interval, and three sibling columns rather than one wire triple
  ["subsurfaceWeight", Schema.Number], ["subsurfaceRadiusR", Schema.Number],
  ["subsurfaceRadiusG", Schema.Number], ["subsurfaceRadiusB", Schema.Number],
  ["coatWeight", Schema.Number], ["coatColor", _ColorWire], ["coatRoughness", Schema.Number], ["coatIor", Schema.Number],
  ["fuzzWeight", Schema.Number], ["fuzzColor", _ColorWire], ["fuzzRoughness", Schema.Number],
  ["thinFilmWeight", Schema.Number], ["thinFilmThickness", Schema.Number], ["thinFilmIor", Schema.Number],
  ["emissionColor", _ColorWire], ["emissionLuminance", Schema.Number], ["geometryOpacity", Schema.Number],
  ["specularRotation", Schema.optionalElement(Schema.Number)], // Key 29: appended past the frozen block
  ["geometryThinWalled", Schema.optionalElement(Schema.Boolean)], // Key 30: appended
])
// `WireEmission` Key(0..5) — the admitted-emission receipt nested at `MaterialWire` Key(7). Its column names match
// `Material.emission` one for one, so the receipt crosses with no reshape arm and no second spelling of the roster.
const _EmissionVector = _keyed("WireEmission", [
  ["dominantWavelengthNm", Schema.Number], ["excitationPurity", Schema.Number],
  ["cctKelvin", Schema.Number], ["cctDuv", Schema.Number],
  ["relativeLuminance", Schema.Number], ["gamutMapped", Schema.Boolean],
])
const _vectored = (v: typeof _PbrVector.Type): typeof PbrGroups.Encoded => ({
  base: { weight: v.baseWeight, color: _shaded(v.baseColor), metalness: v.baseMetalness, diffuseRoughness: v.baseDiffuseRoughness, specularTint: v.baseSpecularTint },
  specular: {
    weight: v.specularWeight, color: _shaded(v.specularColor), roughness: v.specularRoughness,
    ior: v.specularIor, anisotropy: v.specularAnisotropy,
    rotation: Option.getOrElse(v.specularRotation, () => 0), // the producer's stated pre-append value
  },
  transmission: { weight: v.transmissionWeight, roughness: v.transmissionRoughness },
  subsurface: { weight: v.subsurfaceWeight, radius: [v.subsurfaceRadiusR, v.subsurfaceRadiusG, v.subsurfaceRadiusB] },
  coat: { weight: v.coatWeight, color: _shaded(v.coatColor), roughness: v.coatRoughness, ior: v.coatIor },
  fuzz: { weight: v.fuzzWeight, color: _shaded(v.fuzzColor), roughness: v.fuzzRoughness },
  thinFilm: { weight: v.thinFilmWeight, thickness: v.thinFilmThickness, ior: v.thinFilmIor },
  emission: { color: _shaded(v.emissionColor), luminance: v.emissionLuminance },
  geometry: {
    opacity: v.geometryOpacity,
    thinWalled: Option.getOrElse(v.geometryThinWalled, () => false), // the OpenPBR closed-solid default the producer states
  },
})
const _unvectored = (g: typeof PbrGroups.Encoded): typeof _PbrVector.Type => ({
  baseWeight: g.base.weight, baseColor: _unshaded(g.base.color), baseMetalness: g.base.metalness,
  baseDiffuseRoughness: g.base.diffuseRoughness, baseSpecularTint: g.base.specularTint,
  specularWeight: g.specular.weight, specularColor: _unshaded(g.specular.color),
  specularRoughness: g.specular.roughness, specularIor: g.specular.ior, specularAnisotropy: g.specular.anisotropy,
  transmissionWeight: g.transmission.weight, transmissionRoughness: g.transmission.roughness,
  subsurfaceWeight: g.subsurface.weight, subsurfaceRadiusR: g.subsurface.radius[0],
  subsurfaceRadiusG: g.subsurface.radius[1], subsurfaceRadiusB: g.subsurface.radius[2],
  coatWeight: g.coat.weight, coatColor: _unshaded(g.coat.color), coatRoughness: g.coat.roughness, coatIor: g.coat.ior,
  fuzzWeight: g.fuzz.weight, fuzzColor: _unshaded(g.fuzz.color), fuzzRoughness: g.fuzz.roughness,
  thinFilmWeight: g.thinFilm.weight, thinFilmThickness: g.thinFilm.thickness, thinFilmIor: g.thinFilm.ior,
  emissionColor: _unshaded(g.emission.color), emissionLuminance: g.emission.luminance,
  geometryOpacity: g.geometry.opacity,
  // Re-emission always writes both appended slots: the interior column is total, so an encode that omitted them
  // would mint a shorter record than the value it holds and lose a producer-authored rotation on the round trip.
  specularRotation: Option.some(g.specular.rotation),
  geometryThinWalled: Option.some(g.geometry.thinWalled),
})

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
      // Slot names bind at the destructure and the two roster-named records — the capture receipt and the emission
      // receipt — cross with no arm at all, so this transform carries the two reshapes that are genuinely its own:
      // the OpenPBR banding and the shade triple. The emission column is the one absence the wire spells twice, an
      // absent trailing slot or an explicit nil, and both read as the interior's own missing column.
      strict: true,
      decode: ([id, vector, conductor, receipt, shade, emissionUnit, emissionValue, emission]) => ({
        id, openPbr: _vectored(vector), conductor, provenance: receipt, preview: _shaded(shade), emissionUnit, emissionValue,
        ...(emission == null ? {} : { emission }),
      }),
      encode: (wire) => [
        wire.id, _unvectored(wire.openPbr), wire.conductor, wire.provenance, _unshaded(wire.preview),
        wire.emissionUnit, wire.emissionValue, wire.emission ?? null,
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
  // DECLARED PROJECTION, not a two-state fact. The producer holds `Evidence<TileProof>` — absent until its gate
  // grades, measured carrying the proof's own acceptance, refused carrying the spectral band's cause — and its own
  // wire law projects the measured-and-accepted read onto this bool. The bytes therefore CANNOT separate an
  // ungraded set from a graded-and-rejected one, and no posture vocabulary at this end recovers a state the
  // producer never sent; widening it here would assert evidence off the wire. `false` is that whole complement.
  tiled: Schema.Boolean,
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
  // DECLARATION-SOURCED PERMANENTLY at its producer, which settled the question rather than deferring it: a
  // labelled calibration measured the tiled and cut populations overlapping WHOLE under its own seam probe, so no
  // cutoff separates them and the producer publishes its `tile_score` as receipt-band evidence a consumer
  // thresholds against family knowledge. That score never enters this frozen wire, so the claim's own provenance —
  // classifier verdict or caller declaration — stays unreadable here and a third state has nothing to decode from.
  tiled: Schema.Boolean,
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
// renders a magnitude under the producer's own scheme rather than guessing one — which is exactly why the record
// REFUSES a key its own refinement rejects. Left at the default a `Schema.Record` DROPS such a key and answers
// success, so a blank token would delete one scheme entry and that quantity would silently render as SI: the one
// wrong answer this column exists to prevent, reached through a decode that reported nothing.
class Header extends Schema.Class<Header>("Header")({
  schema: Schema.NonEmptyString,
  view: Schema.NonEmptyString,
  geoReference: GeoReference,
  tolerance: Schema.Number,
  at: Schema.DateTimeUtc,
  step: StepHeader,
  unitScheme: Shape.Record(Schema.NonEmptyString, Schema.NonEmptyString),
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
  // `Node.FromWire`'s encoded side is `unknown` by construction — the oneof lift crosses through `Schema.Unknown` —
  // so the emitter is handed a value the type system cannot vouch for. `isMessage(value, descriptor)` is the shipped
  // NARROWING guard over the generated `$typeName` brand, so the descriptor proves the shape and a foreign value
  // becomes a refusal carrying its own coordinate; the cast that used to stand here asserted the same fact and
  // produced no evidence when it was wrong, which is exactly what a proto engine's own guard exists to prevent.
  encode: (wire, _options, ast) =>
    isMessage(wire, Format.proto.suite.NodeWire)
      ? Either.try({
        try: () => toJson(Format.proto.suite.NodeWire, wire),
        catch: () => new ParseResult.Type(ast, wire, "<node-json>"),
      })
      : Either.left(new ParseResult.Type(ast, wire, "<node-message>")),
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

// `PropertyValueWire` is the producer's recursive FOURTEEN-arm typed-value fold — `csharp:Rasm.Element/Graph/wirevalue`
// owns the roster and `RelationshipWire`'s generic arm carries it as `map<string, PropertyValueWire>`. It lands as
// the generated `{ case, value }` oneof face verbatim, the same choice `Organization.containment` takes and for a
// sharper reason: five arms carry a SCALAR, so the oneof-hoisting lift has no record to spread and the case name IS
// the discriminant space. Ten arms bottom out and four recurse through the same carrier, so the nesting half spells
// once over its child type exactly as the shell intent family does.
const _attrCases = [
  "text", "measure", "boolean", "logical", "reference", "bounded", "temporal", "integer", "number", "binary",
  "enumerated", "list", "table", "complex",
] as const

// The producer's `LogicalWire` carries an OPTIONAL bool, so IFC's third LOGICAL state crosses as an absent column
// and never as a third literal this end would have to invent. `integer` crosses as big-endian two's-complement
// octets because the producer's arm is a `BigInteger` no JSON number holds, and `binary` is the opaque octet arm
// beside it — two arms with one wire shape and two meanings, which is why the case name and not the shape routes.
const _Logical = Schema.Struct({ value: Schema.optionalWith(Schema.Boolean, { as: "Option" }) })
const _AttrStamp = Schema.Struct({
  seconds: Schema.BigIntFromSelf,
  nanos: Schema.Int.pipe(Schema.between(0, 999999999)),
})
// The temporal arm is the producer's own five-leaf sub-oneof: four ISO TEXT crossings under NodaTime patterns, and
// one epoch stamp riding the well-known adapter. Text is where the producer re-admits, so this end carries the token.
const _AttrTemporal = Schema.Union(
  Schema.Struct({ case: Schema.Literal("date"), value: Schema.NonEmptyString }),
  Schema.Struct({ case: Schema.Literal("moment"), value: Schema.NonEmptyString }),
  Schema.Struct({ case: Schema.Literal("time"), value: Schema.NonEmptyString }),
  Schema.Struct({ case: Schema.Literal("span"), value: Schema.NonEmptyString }),
  Schema.Struct({ case: Schema.Literal("stamp"), value: _AttrStamp }),
)
const _attrLeaves = Schema.Union(
  Schema.Struct({ case: Schema.Literal("text"), value: Schema.String }),
  Schema.Struct({ case: Schema.Literal("measure"), value: MeasureValue }),
  Schema.Struct({ case: Schema.Literal("boolean"), value: Schema.Boolean }),
  Schema.Struct({ case: Schema.Literal("logical"), value: _Logical }),
  Schema.Struct({
    case: Schema.Literal("reference"),
    value: Schema.Struct({ targetId: _NodeId, usageName: Schema.optionalWith(Schema.NonEmptyString, { as: "Option" }) }),
  }),
  Schema.Struct({
    case: Schema.Literal("bounded"),
    value: Schema.Struct({
      lower: Schema.optionalWith(MeasureValue, { as: "Option" }),
      upper: Schema.optionalWith(MeasureValue, { as: "Option" }),
      setPoint: Schema.optionalWith(MeasureValue, { as: "Option" }),
    }),
  }),
  Schema.Struct({ case: Schema.Literal("temporal"), value: _AttrTemporal }),
  Schema.Struct({ case: Schema.Literal("integer"), value: Schema.Uint8ArrayFromSelf }),
  Schema.Struct({ case: Schema.Literal("number"), value: Schema.Number }),
  Schema.Struct({ case: Schema.Literal("binary"), value: Schema.Uint8ArrayFromSelf }),
)

type _AttrNest<T> =
  | { readonly case: "enumerated"; readonly value: { readonly selected: ReadonlyArray<T>; readonly allowed: ReadonlyArray<T> } }
  | { readonly case: "list"; readonly value: { readonly values: ReadonlyArray<T> } }
  | { readonly case: "table"; readonly value: { readonly interpolation: string; readonly rows: ReadonlyArray<{ readonly defining: T; readonly defined: T }> } }
  | { readonly case: "complex"; readonly value: { readonly usageName: string; readonly properties: { readonly [key: string]: T } } }

type _AttrValue = typeof _attrLeaves.Type | _AttrNest<_AttrValue>
type _AttrValueWire = typeof _attrLeaves.Encoded | _AttrNest<_AttrValueWire>

const _attr: Schema.Schema<_AttrValue, _AttrValueWire> = Schema.suspend(() => _AttrValue)
const _AttrRow = Schema.Struct({ defining: _attr, defined: _attr })

const _AttrValue: Schema.Schema<_AttrValue, _AttrValueWire> = Schema.Union(
  _attrLeaves,
  Schema.Struct({
    case: Schema.Literal("enumerated"),
    value: Schema.Struct({ selected: Schema.Array(_attr), allowed: Schema.Array(_attr) }),
  }),
  Schema.Struct({ case: Schema.Literal("list"), value: Schema.Struct({ values: Schema.Array(_attr) }) }),
  // `interpolation` is roster-gated at the producer exactly as the generic arm's `wireName` is, so this decode
  // RE-QUOTES that roster rather than copying it here to drift, and an unrostered token is refused by the
  // producer's own row lookup on re-entry.
  Schema.Struct({
    case: Schema.Literal("table"),
    value: Schema.Struct({ interpolation: Schema.NonEmptyString, rows: Schema.Array(_AttrRow) }),
  }),
  Schema.Struct({
    case: Schema.Literal("complex"),
    value: Schema.Struct({ usageName: Schema.String, properties: Schema.Record({ key: Schema.String, value: _attr }) }),
  }),
)

// Two-way close over the producer's own roster: a fifteenth arm on the union fails the first alias, and a roster
// token no arm lands fails the second, so neither half can drift off the other and a renamed producer case breaks
// every dispatching consumer at compile time instead of reaching `ui` as an untyped record.
type _AttrArm = _AttrValue["case"]
type _AttrWidened<K extends _AttrArm = (typeof _attrCases)[number]> = K
type _AttrClosed<K extends (typeof _attrCases)[number] = _AttrArm> = K

// `RelationshipWire` is a six-arm oneof and every arm carries its OWN endpoint pair beside its own payload columns,
// so the landing is the union those arms already are: a flat source/target pair erases which endpoint role each arm
// names — a whole and its part, a subject and its definition, a host and its feature are three different relations —
// and drops the ordinal, sub-kind, usage, realizing, interface, attribute, and participant columns beside them.
// `subKind` is the arm's own token column, admitted at the producer's smart-enum gate; the generic arm's
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
    attributes: Schema.Record({ key: Schema.String, value: _attr }),
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
// minting runtime fills with what its own probe reached, so a new host fact needs no schema edit here. Open in its
// KEY SPACE is not open in its key SHAPE: the record refuses a key its refinement rejects rather than dropping it,
// because the default drop answers success on a document this end never received and `print` is compared against
// exactly that document.
class HostFingerprint extends Schema.Class<HostFingerprint>("HostFingerprint")({
  print: Schema.NonEmptyString,
  machine: Schema.NonEmptyString,
  os: Schema.NonEmptyString,
  arch: Schema.NonEmptyString,
  processors: Schema.Int.pipe(Schema.positive()),
  runtime: Schema.NonEmptyString,
  stamps: Shape.Record(Schema.NonEmptyString, Schema.String),
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
- Owner: `Wire.Walk` holds one child projection per recursive landed tree and the one bounded pre-order fold over them.
- Law: every recursive landed family carries a walk row, so no consumer hand-recurses a foreign tree and none goes unbounded.
- Law: walk budgets are unit-carrying — depth spends `Shape.Bound<"hops">` and fan the ingress collection ceiling.
- Law: a walk projection is total against its family's own closed split, so a new arm breaks the table rather than reading as a leaf.
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

// The envelope's `payload` is DECLARED untyped at this field: the receipt roster is the producing package's own and
// open by construction — a sandbox receipt, an update receipt, a lease fence all ride this one envelope without a
// census family — so a typed seat here would claim a closed shape no producer publishes. A consumer owing one
// decodes it against the family that mirrors its own payload, exactly as `Node.payload` is read.
const _ReceiptEnvelopeWire = _receiptEnvelope(Schema.Unknown)
const _schema = {
  ReceiptEnvelopeWire: Format.json.schema(_ReceiptEnvelopeWire),
  // Seam [02.7] fixes a two-64-bit-half cell, so this stamp rides the extension byte the msgpack engine already
  // registers for `Clock.Hlc` — a descriptor displaces that frozen layout with tag bytes no peer minter emits.
  HlcStampWire: Format.msgpack.schema(Clock.Hlc),
  TenantContextWire: Format.json.schema(_TenantContextWire),
  CommandAvailabilityWire: Format.json.schema(Evidence.Availability),
  FaultDetail: Format.proto.family(Format.proto.suite.FaultDetail, Remote.FromWire),
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
  HopReceiptWire: Format.json.schema(HopReceipt),
  DeliveryReceiptWire: Format.json.schema(DeliveryReceipt),
  DropReceiptWire: Format.json.schema(DropReceipt),
  OutboxRowWire: Format.json.schema(OutboxRow),
  DeadLetterRowWire: Format.json.schema(DeadLetterRow),
  ReplayTallyWire: Format.json.schema(ReplayTally),
  OutboxLaneWire: Format.json.schema(OutboxLane),
  OutboxSweepWire: Format.json.schema(OutboxSweep),
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
        : Effect.fail(_mismatch(family, "semantic", value, decoded))))

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
      // the summary owner already measured the divergence, so its refusal RIDES here; discarding it and raising a
      // bare token made the one fault an operator drills on the one fault carrying no coordinate to drill
      Effect.mapError((refusal) => _mismatch("MerkleSummaryWire", "merkle-root", refusal.actual, refusal.expected)),
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
  HopReceiptWire: _row("decode", "json", _schema.HopReceiptWire),
  DeliveryReceiptWire: _row("decode", "json", _schema.DeliveryReceiptWire),
  DropReceiptWire: _row("decode", "json", _schema.DropReceiptWire),
  OutboxRowWire: _row("decode", "json", _schema.OutboxRowWire),
  DeadLetterRowWire: _row("decode", "json", _schema.DeadLetterRowWire),
  ReplayTallyWire: _row("decode", "json", _schema.ReplayTallyWire),
  OutboxLaneWire: _row("decode", "json", _schema.OutboxLaneWire),
  OutboxSweepWire: _row("decode", "json", _schema.OutboxSweepWire),
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

// ONE bounded traversal owner over every recursive foreign tree this page lands. Five families recurse — the shell
// menu strip, the control-intent tree, the selection predicate fold, GeoJSON geometry collections, and the IFC
// typed-value fold — and each reached its consumers with no walk, no depth bound, and a hand recursion per reader,
// so a hostile document was bounded by whichever consumer happened to guard. The roster is ONE child projection per
// family, seated at the registry rather than inside any one landing because it serves them all: a sixth recursive
// family lands as one row and inherits the budget, where today it would inherit nothing.
//
// Projections are TOTAL by construction rather than by a catch-all. Each family already carries a closed split — a
// leaf union beside a nesting union, or a `case` roster — so the projection reads that split and a new arm breaks
// the table at its declaration. A `Match.orElse` here would turn exactly that compile break into a silent zero-child
// answer, which reads as a leaf and truncates the walk.
const _walkRows = {
  ControlIntentWire: {
    children: (node: ControlIntent): ReadonlyArray<ControlIntent> =>
      Schema.is(_leaves)(node) ? Array.empty() : Match.value(node).pipe(Match.discriminatorsExhaustive("kind")({
        // a banner's verbs and its evidence are both child intents, so the strip contributes its whole verb row
        // plus the optional evidence intent — a projection naming only `actions` truncates the walk at the evidence
        banner: (arm) => Array.appendAll(arm.actions, Array.fromOption(arm.evidence)),
        emptyState: (arm) => Array.fromOption(arm.action),
        grid: (arm) => Array.flatMap(arm.columns, (column) => Array.appendAll([column.cell], Array.fromOption(column.editor))),
        tree: (arm) => [arm.item],
        toolbar: (arm) => Array.map(arm.rows, (row) => row.item),
        tab: (arm) => Array.map(arm.pages, (page) => page.body),
        accordion: (arm) => Array.map(arm.sections, (section) => section.body),
        panel: (arm) => arm.children,
        dock: (arm) => arm.regions,
        splitter: (arm) => [arm.first, arm.second],
      })),
  },
  // Menu rows recurse on THEMSELVES rather than on a child intent, so the strip is its own family: a walk that
  // followed the intent projection into a menu would stop at the arm and never see the rows under it.
  ControlMenuRow: {
    children: (node: _MenuRowOf<"type">): ReadonlyArray<_MenuRowOf<"type">> => node.rows,
  },
  PredicateWire: {
    children: (node: PredicateWire): ReadonlyArray<PredicateWire> =>
      Schema.is(_predicateLeaves)(node) ? Array.empty() : Match.value(node).pipe(Match.discriminatorsExhaustive("arm")({
        // every incidence arm recurses through the SAME target carrier, so the match half reads off one projection
        spatialContainer: (op) => Array.fromNullable(op.container.matching),
        composed: (op) => Array.fromNullable(op.whole.matching),
        type: (op) => Array.fromNullable(op.type.matching),
        zone: (op) => Array.fromNullable(op.group.matching),
        connected: (op) => Array.fromNullable(op.other.matching),
        voided: (op) => Array.fromNullable(op.other.matching),
        generic: (op) => Array.fromNullable(op.other.matching),
        all: (op) => op.operands,
        any: (op) => op.operands,
        not: (op) => [op.operand],
      })),
  },
  // GeoJSON nests through ONE arm and the six coordinate arms bottom out, so the collection is the whole recursion.
  GeoFeatureGeometry: {
    children: (node: GeoFeature.Geometry): ReadonlyArray<GeoFeature.Geometry> =>
      node._tag === "GeometryCollection" ? node.geometries : Array.empty(),
  },
  RelationshipAttribute: {
    children: (node: _AttrValue): ReadonlyArray<_AttrValue> =>
      Schema.is(_attrLeaves)(node) ? Array.empty() : Match.value(node).pipe(Match.discriminatorsExhaustive("case")({
        enumerated: (arm) => Array.appendAll(arm.value.selected, arm.value.allowed),
        list: (arm) => arm.value.values,
        table: (arm) => Array.flatMap(arm.value.rows, (row) => [row.defining, row.defined]),
        complex: (arm) => Record.values(arm.value.properties),
      })),
  },
} as const

// Budgets are UNIT-CARRYING: the traversal ceiling is `Shape.Bound<"hops">`, so a hop budget can never be spent
// where a fanout or fuel budget belongs, and the fan ceiling stays `Shape.Ingress.floor.collection` — the same
// number the ingress gate already refuses a too-wide document on, read here rather than restated.
const _WALK: Shape.Bound<"hops"> = Shape.Bound.of("hops", Shape.Ingress.floor.depth)

// One typed refusal per budget carrying the measured extent beside the ceiling it broke, on the `overrun` cause the
// policy table already rows — so a walk refusal grades, retains, and replays exactly as every other bounded refusal
// on this page does, instead of minting a second exhaustion vocabulary beside it.
const _overspent = (
  family: Wire.FaultFamily,
  axis: "walk-depth" | "walk-fan",
  actual: number,
  expected: number,
): WireFault => new WireFault({ family, case: { reason: "overrun", axis, actual, expected, at: Option.none() } })

// BOUNDARY ADAPTER: the explicit stack is what bounds a hostile depth without consuming the JS call stack — the
// same posture `Shape.Ingress`'s own probe takes over a foreign graph — where a recursive fold exhausts the engine
// before the declared budget ever refuses. Order is PRE-ORDER: children enter ahead of the remaining frontier, so a
// consumer folding the result sees a parent before anything under it.
const _walked = <A>(
  family: Wire.FaultFamily,
  row: Wire.Walk.Row<A>,
  root: A,
  bound: Shape.Bound<"hops">,
): Effect.Effect<ReadonlyArray<A>, WireFault> =>
  Effect.map(
    Effect.iterate(
      { held: Array.empty<A>(), pending: Array.of([root, 0] as readonly [A, number]) as ReadonlyArray<readonly [A, number]> },
      {
        while: ({ pending }) => Array.isNonEmptyReadonlyArray(pending),
        body: ({ held, pending }) =>
          Array.matchLeft(pending, {
            onEmpty: () => Effect.succeed({ held, pending }),
            onNonEmpty: ([node, hops], rest) =>
              hops > bound
                ? Effect.fail(_overspent(family, "walk-depth", hops, bound))
                : pipe(row.children(node), (children) =>
                  children.length > Shape.Ingress.floor.collection
                    ? Effect.fail(_overspent(family, "walk-fan", children.length, Shape.Ingress.floor.collection))
                    : Effect.succeed({
                      held: Array.append(held, node),
                      pending: Array.appendAll(Array.map(children, (child) => [child, hops + 1] as const), rest),
                    })),
          }),
      },
    ),
    ({ held }) => held,
  )

type _OverrunAxis = (typeof _overrunAxes)[number]
type _ReceiptKind = (typeof _receiptKinds)[keyof typeof _receiptKinds]
type _CommandOutcome = typeof _CommandOutcome.Type
type _DeckReceipt = typeof _DeckReceipt.Type
type _WriteBack = typeof _WriteBack.Type
type _FaultObservation = typeof _FaultObservation.Type
type _WireFaultCase = WireFault.Case
type _WireFaultReason = WireFault.Reason
type _Credential = Schema.Schema.Type<(typeof _rows)["CredentialPemWire"]["schema"]>
type _CredentialLabel = Credential.Label
type _GeoFeature = Schema.Schema.Type<typeof GeoFeature>
type _GeoFeatureCrs = GeoFeature.Crs
type _GeoFeatureExtent = GeoFeature.Extent
type _GeoFeatureTile = GeoFeature.Tile
type _HopsReason = Hops.Reason
type _HopsRow = Hops.Row
type _InvokeFault = InvokeFault
type _InvokeReason = InvokeReason
type _RemoteDetail = RemoteDetail
type _TransportKind = TransportKind
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
  namespace Fault {
    // The refusal payload publishes so a peer assembler raises through the roster rather than re-spelling a cause's
    // columns, and the budget axis publishes because `frame` refuses on ceilings this page never sees — a raiser
    // there names a member of THIS roster or breaks, which is what keeps nine ceilings under one vocabulary.
    type Case = _WireFaultCase
    type Reason = _WireFaultReason
  }
  type OverrunAxis = _OverrunAxis
  type ReceiptKind = _ReceiptKind
  type CommandOutcome = _CommandOutcome
  type DeckReceipt = _DeckReceipt
  type WriteBack = _WriteBack
  type FaultObservation = _FaultObservation
  type FaultDetail = Decoded<"FaultDetail">
  type InvokeFault = _InvokeFault
  type InvokeReason = _InvokeReason
  type RemoteDetail = _RemoteDetail
  type TransportKind = _TransportKind
  type BindingStatus = Decoded<"BindingStatusWire">
  type CoercedValue = Decoded<"CoercedValueWire">
  type WriteReceipt = Decoded<"WriteReceiptWire">
  type HopReceipt = Decoded<"HopReceiptWire">
  type DeliveryReceipt = Decoded<"DeliveryReceiptWire">
  type DropReceipt = Decoded<"DropReceiptWire">
  type OutboxRow = Decoded<"OutboxRowWire">
  type DeadLetterRow = Decoded<"DeadLetterRowWire">
  type ReplayTally = Decoded<"ReplayTallyWire">
  type OutboxLane = Decoded<"OutboxLaneWire">
  type OutboxSweep = Decoded<"OutboxSweepWire">
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
  // The recursive families are their OWN key space: a walk row keys on the tree, not on the census family that
  // happens to carry it, because one census row can nest two unlike trees (the intent family carries its menu
  // strip) and one tree can ride two census rows.
  namespace Walk {
    type Family = keyof typeof _walkRows
    type Row<A> = { readonly children: (node: A) => ReadonlyArray<A> }
    type Node<K extends Family> = (typeof _walkRows)[K] extends { readonly children: (node: infer N) => unknown } ? N
      : never
    // `Surface`, never `Shape`: a member named `Shape` inside this namespace shadows the value floor's own owner at
    // every reference below it, so the budget type would silently resolve against this declaration.
    type Surface = {
      readonly families: ReadonlyArray<Family>
      readonly floor: Shape.Bound<"hops">
      readonly nodes: <K extends Family>(
        family: K,
        root: Node<K>,
        bound: Shape.Bound<"hops">,
      ) => Effect.Effect<ReadonlyArray<Node<K>>, WireFault>
    }
  }
}

// The refusal rides TYPED on its own arm rather than being flattened into a token and re-offered as loose evidence
// beside it: the retired form spelled three of the refusal's columns into a string and handed the whole value over
// again, so one fact crossed twice under two shapes and a reader had to parse the string to learn which.
const _refused = (family: Wire.Family, refusal: Contract.Refusal): WireFault =>
  new WireFault({ family, case: { reason: "drift", divergence: { subject: "contract", refusal } } })

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
    case: { reason: "overrun", axis: "payload", actual, expected: Shape.Ingress.floor.bytes, at: Option.none() },
  })

const _completeStream = <K extends Wire.Ingress>(family: K, frames: AsyncIterable<Uint8Array>) =>
  Stream.fromAsyncIterable(frames, (defect) =>
    new WireFault({ family, case: { reason: "malformed", at: "source", issue: String(defect) } })).pipe(
    Stream.mapEffect(
      (octets) =>
        (octets.byteLength > Shape.Ingress.floor.bytes
          ? Effect.fail(_overrun(family, octets.byteLength))
          : _decode(family, octets)).pipe(
          Effect.mapError((issue) => issue instanceof WireFault
            ? issue
            : new WireFault({ family, case: { reason: "malformed", at: "decode", issue: issue.message } })),
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
- Law: every bounded loss RETURNS as a fact on the band beside the value, so a quiet subject and a coalesced burst never read alike.
- Law: the census DERIVES by folding that fact band through the ledger monoid; a counter kept beside the stream is the deleted form.

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
    subject: (status) => status.bindingId,
    alike: Schema.equivalence(BindingStatus),
    band: Option.some("control"),
  },
  CommandGateWire: {
    subject: (gate) => gate.key,
    alike: Schema.equivalence(CommandGate),
    band: Option.some("control"),
  },
}

// The restatement the fold drops RETURNS as a fact on the same band the survivors ride, so a subject that went
// quiet and a subject whose burst coalesced are two readings a consumer can tell apart. The fact carries the
// subject it dropped and the extent it dropped, so the census below derives from occurrences rather than from a
// tally kept beside the stream that no arrival can reconcile against.
const _transitions = <A>(row: feed.Row<A>) =>
<E, R>(marks: Stream.Stream<A, E, R>): Stream.Stream<Either.Either<A, Fault.Drop.Fact>, E, R> =>
  marks.pipe(
    // subject projection binds ONCE per element: a re-read per arm charges the keying fold three projections on the
    // feed's own hot path, where the declared cadence is hundreds of marks a second
    Stream.mapAccum(HashMap.empty<string, A>(), (seen, value) =>
      pipe(row.subject(value), (subject) =>
        Option.match(HashMap.get(seen, subject), {
          onNone: () => [HashMap.set(seen, subject, value), Either.right(value)] as const,
          onSome: (prior) =>
            row.alike(prior, value)
              ? ([seen, Either.left(Fault.Drop.fact("coalesced", subject, 1))] as const)
              : ([HashMap.set(seen, subject, value), Either.right(value)] as const),
        }))),
  )

const feed = <K extends feed.Family>(
  family: K,
  frames: AsyncIterable<Uint8Array>,
): Stream.Stream<
  Either.Either<Wire.Decoded<K>, Fault.Drop.Fact>, Wire.Fault, Quarantine | Context.Tag.Service<typeof Contract.Descriptor>
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

// The census is a FOLD of the fact band and nothing else, through the ledger's own monoid: `empty` is the zero
// every reason answers and `combineAll` walks the band once, so a caller reads one census and no counter rides
// beside the stream to disagree with it. `Fault.Ledger` is the estate's one drop-census owner — the closed reason
// roster, the per-reason `{ count, extent }` cell, and the monoid over them — seated at the value floor because
// this page and `interchange/carrier` both drop under it, and `carrier` holds only a TYPE edge to this one.
const _dropped = <A, E, R>(
  band: Stream.Stream<Either.Either<A, Fault.Drop.Fact>, E, R>,
): Effect.Effect<Fault.Ledger.Census, E, R> =>
  Stream.runFold(band, Fault.Ledger.monoid.empty, (held, lane) =>
    Either.match(lane, {
      onLeft: (fact) => Fault.Ledger.monoid.combine(held, Fault.Ledger.of(fact)),
      onRight: () => held,
    }))
```

## [09]-[SEQUENCE_GAP]

- Owner: `Wire.Gap` owns sequence evidence and resumable ordered delivery.
- Law: gaps emit evidence once and valid arrivals still deliver.
- Law: a replayed coordinate RETURNS a drop fact carrying the coordinate it refused, so at-least-once redelivery reads as a measured extent.
- Law: a gap is a FAULT and a replay is a DROP — one is missing evidence the peer owes, the other is lawful duplication under the delivery contract.

```typescript signature
const _bySeq: Order.Order<OpLogEntry> = Order.mapInput(Order.bigint, (entry: OpLogEntry) => entry.seq)

// The refused band carries two unlike verdicts and keeps them unlike: a sequence GAP is evidence the producing peer
// owes a frame this consumer never saw, while a replayed coordinate is the delivery contract working as declared.
// Folding both onto the fault rail made the second read as breakage; dropping the second silently — the
// `Chunk.empty` this replaces — left a caller unable to tell a quiet producer from one re-driving its whole tail.
type _Lane<A> = Either.Either<A, WireFault | Fault.Drop.Fact>

const Gap: {
  readonly evidence: (
    family: Wire.FaultFamily,
    subject: (typeof _gapSubjects)[number],
    expected: bigint,
    actual: bigint,
  ) => WireFault
  readonly sequential: (
    family: Wire.FaultFamily,
    resume: bigint,
  ) => <A extends { readonly seq: bigint }, E, R>(
    entries: Stream.Stream<Either.Either<A, WireFault>, E, R>,
  ) => Stream.Stream<_Lane<A>, E, R>
} = {
  evidence: (family, subject, expected, actual) =>
    new WireFault({ family, case: { reason: "sequence", subject, actual, expected } }),
  sequential: (family, resume) => <A extends { readonly seq: bigint }, E, R>(entries: Stream.Stream<Either.Either<A, WireFault>, E, R>) =>
    entries.pipe(
      Stream.mapAccum(resume, (last, lane): readonly [bigint, Chunk.Chunk<_Lane<A>>] =>
        Either.match(lane, {
          onLeft: (): readonly [bigint, Chunk.Chunk<_Lane<A>>] => [last, Chunk.of(lane)],
          onRight: (entry) =>
            entry.seq <= last
              // the coordinate names the drop, so a replayed tail folds to an extent rather than to silence
              ? ([last, Chunk.of(Either.left(Fault.Drop.fact("replayed", String(entry.seq), 1)))] as const)
              : entry.seq === last + 1n
                ? ([entry.seq, Chunk.of(lane)] as const)
                : ([entry.seq, Chunk.make(Either.left(Gap.evidence(family, "ordinal", last + 1n, entry.seq)), lane)] as const),
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
  ) => Stream.Stream<_Lane<OpLogEntry>, WireFault, Quarantine>
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

// The roster publishes BESIDE the entry, so a consumer choosing a projection reads which trees are walkable rather
// than discovering it by a refused key, and `floor` is the page's own budget a caller either spends or narrows.
const _walk: Wire.Walk.Surface = {
  families: Record.keys(_walkRows),
  floor: _WALK,
  nodes: (family, root, bound) => _walked(family, _walkRows[family] as Wire.Walk.Row<typeof root>, root, bound),
}

const Wire = {
  ..._registry,
  Walk: _walk,
  Fault: WireFault,
  Quarantine,
  Parity,
  feed,
  Gap,
  dropped: _dropped,
  OpLog,
  CrdtOp,
  Hops,
  Remote,
  Transport,
  MalformedDetail,
  invokeReasons: _invokeCensus,
  receiptKinds: _receiptKinds,
  invokeReason: _invokeReason,
  EnricherLive: _EnricherLive,
  Credential,
  FlagVerdict,
  EvidenceTimeline,
  BindingStatus,
  CoercedValue,
  WriteReceipt,
  HopReceipt,
  DeliveryReceipt,
  DropReceipt,
  OutboxRow,
  DeadLetterRow,
  ReplayTally,
  OutboxLane,
  OutboxSweep,
  CommandGate,
  CommandRow,
  ControlIntent,
  ControlReceipt,
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
