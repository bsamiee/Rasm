# [CORE_CODEC]

`Wire` owns the branch's closed wire vocabulary and everything derived from it: one ordered family roster, one row per family carrying direction, arm, schema, and parity obligation, and the decoded landing each producer's shape resolves to. Fault classification, bounded quarantine with replay, content and semantic parity, keyed transition feeds, and sequence evidence all read that one table. Module `core/src/interchange/codec.ts` admits a family as one row, a fault cause as one policy row, and a transport status as one `Hops` row.

`Wire` composes the `value` floor's identity, clock, and schema owners, the `state` causal, commit, and evidence owners, and `observe`'s board claim; a quantity rides `MeasureValueWire` and binds no family here. Every codec arrives from `interchange/format`: arm selection and quarantined-frame rendering read `Format.rows.arm`, so no consumer here spells an encoding name. `interchange/carrier` binds `Wire.Family` for its typed-metadata table, and `interchange/frame` composes `Wire.Fault`, `Wire.Gap`, `Wire.Parity`, and `Wire.Quarantine` for its own bounded assemblers.

## [01]-[INDEX]

- [02]-[WIRE_REGISTRY]: ordered family vocabulary and exact row contract; `Wire`.
- [03]-[FAULT_RAIL]: fault policy, quarantine intake, replay, and divert; `Wire.Fault`, `Wire.Quarantine`.
- [04]-[PARITY_VERIFY]: content-key verification and semantic roundtrip; `Wire.Parity`.
- [05]-[LANDING_EVIDENCE]: evidence, identity, version, CRDT, and oplog landings; `Wire`.
- [06]-[LANDING_WIRE]: wire-owned decoded shapes for later-wave consumers; landing classes on `Wire`.
- [07]-[KEYED_REGISTRY]: mapped landing table, polymorphic decode/encode/stream entrypoints, bounded tree walk; `Wire`, `Wire.Walk`.
- [08]-[FEED_DEDUP]: quarantine diversion and family transition policies; `Wire.feed`.
- [09]-[SEQUENCE_GAP]: sequence evidence, op-log continuity, and resume-cursor reads; `Wire.Gap`, `Wire.OpLog`.

## [02]-[WIRE_REGISTRY]

- Owner: `_families` and `_rows` close the ordered wire vocabulary and its typed registry.
- Law: each row preserves literal direction and arm and carries one schema and optional parity.
- Law: `_faultFamilies` widens the roster with families this page never decodes but whose owners raise faults against it.
- Law: `_faultArms` names the arm of every such family, so arm resolution stays total across the fault roster.
- Law: a frozen family KEEPS its byte shape when its interior owner moves down-strata, and the decode re-targets onto the new owner in ONE unit.
- Law: tear-then-rebuild is the barred order, because every peer decoder is stranded across the window between the tear and the re-land.
- Law: the re-target edits that family's `_schema` entry ALONE, never `_families`, so the census row and parity obligation survive the move.
- Boundary: `Format` owns codec engines and the arm vocabulary; external producers own wire spellings.
- Boundary: `interchange/carrier` owns the message envelope and `Format.event` its media roster, so no row, landing class, fault cause, or parity obligation here names a CloudEvents shape.
- Boundary: a message envelope crossing this plane carries a wire family in its payload rather than being one.

```typescript signature
import { Array, type ParseResult, Schema, type Types } from "effect"

// Keys transcribe the DECLARED message name of every corpus family this branch decodes, so a key is a spelling the
// corpus minted: each admitted family crosses under its generated package — UI under `ui.v1`, render under
// `render.v1` — appearance as the `appearance.v1` set document and material, and CRDT operations as the generated
// `crdt.v1.CrdtOpWire` oneof. The remaining MessagePack families keep their producer-owned names because their
// bytes carry no descriptor; the HLC cell rides its frozen two-half layout. Withdrawn this pass: `TallyWire` (no corpus family carries a progress
// tally; `Evidence.Tally` is a state owner, not a wire), `TenantContextWire` and `OpenPbrGroupsWire` (nested members
// of the receipt envelope and the material, which land as decoded shapes and register no census row), and the
// two handwritten set rows, which are one generated `Set.product` family under its structural oneof.
const _families = [
  "HlcStampWire", "CommandAvailability",
  "FaultDetail",
  "OpLogWire", "CrdtOpWire",
  "CommitWire", "BranchWire", "VersionVectorWire", "MerkleSummaryWire",
  "EntityEditWire", "CredentialPublicWire", "DescriptorPinWire",
  "BenchmarkClaimWire",
  "BindingStatus", "CoercedValueWire", "WriteReceiptWire",
  "FlagVerdictWire", "AppUiSurfaceProgram", "CommandGateWire", "EvidenceTimelineWire",
  "BcfTopicWire", "BcfViewpointWire", "ModelDiffWire",
  "Material", "Set",
] as const

const _wireLiteral = Schema.Literal(..._families)
const _faultFamilies = [
  ..._families,
  "ArtifactAssembly", "GeometryResidency", "IfcWire",
  "CommandInvocation",
] as const
const _faultLiteral = Schema.Literal(..._faultFamilies)

// Fault-only families decode at their OWN owner — `frame` assembles the artifact, geometry, residency, and IFC
// wire families and `invoke` decodes the command invocation — so no `_rows` entry exists to read an arm from. Naming
// the arm here keeps arm resolution total across the whole fault roster, which is what lets every held frame render:
// without it the quarantine census holds bytes it can print nothing about. The complement type closes the table
// both ways, so a new fault family fails at this declaration rather than at a silent absence.
// `IfcWire` alone stays a `json` arm: an IFC container admission carries no corpus descriptor.
const _faultArms = {
  ArtifactAssembly: "msgpack",
  GeometryResidency: "proto",
  IfcWire: "json",
  CommandInvocation: "proto",
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
import { fromJson, isMessage, type MessageInitShape, type MessageShape, type MessageValidType, toJson } from "@bufbuild/protobuf"
import {
  DurationSchema, durationFromMs, durationMs, EmptySchema, timestampFromMs, timestampMs, TimestampSchema, ValueSchema,
} from "@bufbuild/protobuf/wkt"
import { Code } from "@connectrpc/connect"
import { BadRequest_FieldViolationSchema, RetryInfoSchema } from "@rasm/contracts/google/rpc/error_details_pb"
import { DateSchema } from "@rasm/contracts/google/type/date_pb"
import { DateTimeSchema } from "@rasm/contracts/google/type/datetime_pb"
import { TimeOfDaySchema } from "@rasm/contracts/google/type/timeofday_pb"
import * as appearance from "@rasm/contracts/rasm/contracts/appearance/v1/appearance_pb"
import * as appearanceEnvironment from "@rasm/contracts/rasm/contracts/appearance/v1/environment_pb"
import * as appearanceSet from "@rasm/contracts/rasm/contracts/appearance/v1/set_pb"
import * as artifact from "@rasm/contracts/rasm/contracts/artifact/v1/artifact_pb"
import * as control from "@rasm/contracts/rasm/contracts/compute/v1/control_pb"
import { CrdtOpWireSchema } from "@rasm/contracts/rasm/contracts/crdt/v1/crdt_pb"
import * as graph from "@rasm/contracts/rasm/contracts/element/v1/graph_pb"
import * as property from "@rasm/contracts/rasm/contracts/element/v1/value_pb"
import { HlcSchema } from "@rasm/contracts/rasm/contracts/clock/v1/hlc_pb"
import { FaultDetailSchema, FaultRecoverySchema } from "@rasm/contracts/rasm/contracts/fault/v1/fault_pb"
import type { ControlIntentWireValid, MenuRowWireValid } from "@rasm/contracts/rasm/contracts/ui/v1/controls_pb"
import * as evidence from "@rasm/contracts/rasm/contracts/element/v1/edit_pb"
import {
  Brand, Cause, Chunk, DateTime, Duration, Effect, Either, Encoding, Exit, Function, HashMap, Match, Option, Order, pipe, Predicate,
  Record,
  Schedule, STM, TMap, TRef, type SchemaAST,
} from "effect"
import { Fault } from "../value/fault.ts"
import { Format } from "./format.ts"

const _causes = ["malformed", "overrun", "sequence", "parity", "drift", "stale", "conflict"] as const

// Budget axes close as ONE roster because every one of them was a `<walk-fan>`-shaped token inside a free
// string: five ceilings across this page and `frame` refuse under one cause, and naming which one broke is the
// single fact an operator acts on. The artifact coordinate is optional on this cause alone, since only the assembly
// legs know which artifact and generation a refused band belonged to.
const _overrunAxes = [
  "payload", "frames", "assembly",
  "walk-depth", "walk-fan",
] as const
// Every other subject roster the retired string encoded, named the same way: the raiser passes a roster member and the
// mint below takes that member's own type, so a token this page never declared refuses at the call rather than reaching
// a census as prose.
const _paritySubjects = ["key", "golden-bytes", "semantic", "merkle-root"] as const
const _gapSubjects = ["ordinal", "total", "tail"] as const
const _coordinate = Schema.OptionFromSelf(
  Schema.Struct({ artifact: Digest.codecs.content.wire, generation: Schema.Int.pipe(Schema.nonNegative()) }),
)

// One row declares the whole cause: the branch class, the surface leg that DECIDES it, the SUBJECT a raise must
// supply, and the renderer over that subject. The retired shape carried a free-string `detail` beside a three-arm
// `evidence` union every reason shared, so a cause's real columns were unrecoverable in both directions —
// `<total-drift>` and the remaining tokens were DISCRIMINANTS living inside a string no consumer could switch on
// and no compiler could spell-check, while the shared union offered a merkle
// refusal the `artifact`/`generation` pair only the assembly legs carry and offered a contract refusal to causes
// that never see one. Each subject below is exact at its own raise, so the axis a budget broke and the end a schema
// version moved are typed columns a board reads.
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
      // Drift raises carry their exact coordinate: advertised versus generated contract identity under one admitted
      // generation, a missing runtime adapter lane, a method-kind mismatch, or an unknown verb. `Dial.sdk` compares
      // package and service family and walks no descriptor — `buf breaking` FILE remains field compatibility authority.
      detail: Schema.Struct({
        divergence: Schema.Union(
          Schema.Struct({
            subject: Schema.Literal("contract"),
            advertised: Schema.Struct({ package: Schema.NonEmptyString, family: Schema.NonEmptyString }),
            generated: Schema.Struct({ package: Schema.NonEmptyString, family: Schema.NonEmptyString }),
            generation: Schema.NonEmptyString,
          }),
          Schema.Struct({ subject: Schema.Literal("adapter"), lane: Schema.NonEmptyString }),
          Schema.Struct({ subject: Schema.Literal("binding"), actual: Schema.Unknown, expected: Schema.Unknown }),
          Schema.Struct({ subject: Schema.Literal("verb"), key: Schema.NonEmptyString }),
        ),
      }),
      render: (issue) =>
        Match.value(issue.divergence).pipe(Match.discriminatorsExhaustive("subject")({
          contract: ({ advertised, generated, generation }) =>
            `<contract> ${advertised.package}/${advertised.family} diverges from ${generated.package}/${generated.family} at ${generation}`,
          adapter: ({ lane }) => `<adapter> ${lane} was configured but no runtime capability was supplied`,
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

// Arm resolution is total across the WHOLE fault roster: registered families read their registry row and the
// families other pages decode read `_faultArms`. Totality is what makes rendering unconditional — a partial
// resolution leaves the census holding octets it can print nothing about, which is the one thing the census exists
// to prevent.
const _armOf = (family: Wire.FaultFamily): Format.Arm =>
  _family.is(family) ? _rows[family].arm : _faultArms[family]

class Quarantine extends Effect.Service<Quarantine>()("@rasm/core/Quarantine", {
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
      // self-describing MessagePack families printing nothing at all — and the positional appearance records are exactly the
      // frames whose slot drift an operator has to see — while the four fault-only families printed nothing
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
import { ArtifactId, Digest } from "../value/contentKey.ts"

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
- Owner: generated `crdt.v1.CrdtOpWire` is the sole ten-arm operation vocabulary; `CrdtOp` is `Format.proto.message(CrdtOpWireSchema)` refined only by producer-canonical repeated-row order, never a hand TypeScript union.
- Law: the generic op-log stays the producer's thirteen-slot MessagePack record and retains its raw payload. Only a `family === "crdt"` entry admits that payload through the generated required oneof; every other family stays opaque and no `Any`, arm tag table, or string bag enters.
- Law: an unset or unknown oneof arm refuses at descriptor admission rather than crossing the merge algebra as opaque bytes; vector and observed-tag rows arrive in the corpus-declared strict order.
- Law: evidence render arms retain encoded `frameHash`, optional `drawHash`, and optional canonical pixel identity.

```typescript signature
import { Clock } from "../value/clock.ts"
import { Identity } from "../value/identity.ts"
import { Shape } from "../value/schema.ts"
import { Causal } from "../state/causal.ts"
import { Commit } from "../state/commit.ts"
import { Board } from "../observe/board.ts"

// Positional framing is ONE law this page reads for producer-owned MessagePack records, and the slot roster is where it lives: a positional
// tuple on the encoded side, a named owner on the interior side, and the roster carrying the producer's `[Key(n)]`
// ORDER and its column NAMES together. `_keyed` is the fold for every `[MessagePackObject]` record. A per-record hand
// transform reading `v[0]`…`v[30]` lets one slot drift off the roster naming it, and it restates that roster a
// second time to encode, so the two directions can disagree with nothing raising.
//
// APPENDED slots ride `Schema.optionalElement`, so pre-append bytes ending early decode unchanged and the named column
// carries the branch's own absence carrier rather than a hole — the producer's stated default is supplied at the ONE
// arm that reads the column, never fabricated inside this fold.
type _Cell = Schema.Schema.Any | Schema.Element<Schema.Schema.Any, "?">
type _Pairs = ReadonlyArray<readonly [string, _Cell]>
type _Slots<S extends _Pairs> = { readonly [I in keyof S]: S[I][1] }
type _Named<S extends _Pairs> = {
  readonly [E in S[number] as E[0]]: E[1] extends Schema.Element<infer T extends Schema.Schema.Any, "?">
    ? Schema.optionalWith<T, { readonly as: "Option"; readonly exact: true }>
    : E[1]
}

// Tuples take the cell verbatim and the named owner takes its unwrapped column, so one roster row states both
// sides of an appended slot. `Schema.isSchema` is the shipped discriminant: an `Element` carries the token, never the
// schema brand, so no local marker or arity knob decides which half a row is.
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

// Producer slot domains on the outer envelope: identities and payloads remain raw octets. MessagePack-CSharp writes
// the shortest lawful integer token, so compact positive integers arrive as exact JS numbers while uint64 tokens
// arrive as bigint under `useBigInt64`; one schema widens both representations to bigint before the interior reads
// them and emits bigint without narrowing.
const _text = Schema.String
const _octets = Schema.Uint8ArrayFromSelf
const _fixed16 = _octets.pipe(Schema.filter((value) => value.length === 16, { message: () => "<fixed-16-octets>" }))
const _traceId = _octets.pipe(Schema.filter((value) => value.length === 0 || value.length === 16, { message: () => "<trace-id-width>" }))
const _i63 = Schema.Union(Schema.BigIntFromSelf, Schema.BigIntFromNumber).pipe(
  Schema.betweenBigInt(0n, 9_223_372_036_854_775_807n),
)
const _CrdtOp = Format.proto.message(CrdtOpWireSchema)
type CrdtOp = typeof _CrdtOp.Type

// Stamped arms compose clock.v1.Hlc directly. Non-stamped generated arms return absence; a fabricated zero would make
// add/remove/counter/sequence ops look temporally ordered when the producer declared no stamp.
const _hlc = Schema.decodeSync(Clock.Hlc)
const _stamped = (op: CrdtOp): Option.Option<Clock.Hlc> => {
  switch (op.arm.case) {
    case "set":
    case "write":
    case "beat":
    case "leave":
      return Option.fromNullable(op.arm.value.stamp).pipe(Option.map(_hlc))
    default:
      return Option.none()
  }
}

const _byteOrder = (left: Uint8Array, right: Uint8Array): number => {
  const extent = Math.min(left.length, right.length)
  for (let at = 0; at < extent; at += 1) {
    const order = (left[at] ?? 0) - (right[at] ?? 0)
    if (order !== 0) return order
  }
  return left.length - right.length
}

const _strictRows = <A>(rows: ReadonlyArray<A>, order: (left: A, right: A) => number): boolean =>
  rows.slice(1).every((right, at) => {
    const left = rows[at]
    return left !== undefined && order(left, right) < 0
  })

// Protovalidate owns field domains and required presence; producer-canonical repeated-row order is a relation
// between neighbours and admits here in every runtime before the operation reaches state.
const _orderedCrdt = (op: CrdtOp): boolean => {
  switch (op.arm.case) {
    case "write":
      return _strictRows(op.arm.value.context, (left, right) => _byteOrder(left.origin, right.origin))
    case "maintain":
      return _strictRows(op.arm.value.quiescent, (left, right) => _byteOrder(left.origin, right.origin))
    case "remove":
      return _strictRows(op.arm.value.observedTags, (left, right) => {
        const origin = _byteOrder(left.origin, right.origin)
        return origin !== 0 ? origin : left.logical < right.logical ? -1 : left.logical > right.logical ? 1 : 0
      })
    default:
      return true
  }
}

// The refinement sits on the registered schema, so TypeScript minters and readers spend the same order gate.
const CrdtOp = _CrdtOp.pipe(Schema.filter(_orderedCrdt, { message: () => "<crdt-row-order>" }))
```

## [06]-[LANDING_WIRE]

- Owner: `Wire` lands producer-exact domain values for graph, edits, appearance, and the fault detail, and seats the one hop table every transport outcome grades through.
- Law: generated families land once at their real reader — the live-wire trio, control surface, evidence timeline, BCF pair, model diff, benchmark claim, credential, flag verdict, availability, entity edit, appearance set, and material — and the corpus's rules validate each descriptor before the consumer lifts absence or enums.
- Law: `Hops` is ONE table keyed on the closed `Code` enum the transport ships — reason, class, transport kind, and the peer's re-send and failover verdicts — and every code question in the branch reads that row; a second grading beside it is the deleted form.
- Law: `RemoteDetail` carries the D6 roster — `domain` the producing family, `case` its closed ordinal, never a transport code — and a remote fault's class elects off the producer's typed recovery arm; the domain case reaches no retry band.
- Law: that recovery arm IS `google.rpc.RetryInfo`, so the estate detail and the standard `Status.details` seat carry ONE message; `_Advice` is the branch's single crossing between it and a `Duration`, and an arm stating no `retryDelay` refuses at admission instead of reaching the interior as absence.
- Law: the three invocation outcomes elect by trailer SHAPE — one decodable detail is remote, undecodable or plural is malformed, absent is transport.
- Law: well-known stamps type against the package's own `TimestampSchema`/`DurationSchema`, range-refined above them, and cross to the branch clock through `timestampMs`/`durationMs`; the HLC message lands as `Clock.Hlc` with no scaling.
- Law: the producer's `NodeId` and its `ContentAddress` — the C# bare-digest brand this branch spells `ContentKey`, never C#'s own composite of that name — land as distinct brands off sixteen-byte keys, and `Node` retains the producer-carried authoritative content address.
- Law: entity edits land the generated two-arm oneof as the branch's closed RFC 6902 document, every `Value` crossing `Shape.Json` through the generated codec and every pointer passing `Format.Patch`'s prototype-safe refinement.
- Law: the appearance vocabulary IS the generated `appearance.v1` enum set — every roster derives from its enum, every legality column this page owns closes against the enum's defined members, and the plane laws no field rule states refine the generated `Set` above the descriptor.
- Law: `Wire.Artifact.reference` lands the exact 32-byte `ArtifactRef.sha256` as one `ArtifactSha256` identity and retains `artifactBytes`; SHA-256 hashes the ordered raw artifact octets with no semantic prefix or frame bytes.
- Law: `Wire.Artifact.frame` lands the generated nested reference whole, and `Wire.Artifact.mint` proves a stream's ordered payload through the protocol-fixed SHA-256 owner; SHA-256 never enters `Digest.Kind` or replaces XXH3 semantic/cache keys.
- Law: `Wire.Texture.reference` lands `PlaneRef.artifact` whole; no digest, extent, path, or file-derived identity sits beside it.
- Law: the IFC typed-value fold lands as the producer's own fourteen-arm `{ case, value }` face, closed both ways against its roster.
- Law: a record with a REFINED key closes through `Shape.Record`, so a refused key fails the decode instead of vanishing from the map.
- Law: the control-intent family closes THIRTY-ONE arms at the corpus; the walk projection reads the generated `arm` face and an unset arm yields no child.
- Boundary: raw GeoJSON text and CloudEvents remain outside the registry because no typed family crosses.
- Boundary: a nested family member registers no census row, so a tenant context, a command payload, an OpenPBR vector, and a plane carry no gate or parity.

```typescript signature
import { VariantSchema } from "@effect/experimental"
import { Context, Layer } from "effect"

// `_absent` leads this section because every landing below reads it: it folds the producer's typed absence on a
// scalar string column once, since proto3 emits `""` for an unset singular string — a remote detail's `tenant`, a
// verdict's `variant`, an authored material's `emissionUnit`, an acquired set's `materialId`, and a dielectric's
// `conductor` all arrive empty and read as `Option.none()`. Declared below its first reader it sat in the temporal dead
// zone of every eagerly-evaluated `Schema.Class` above it. The shipped operator owns the fold; a local twin is the
// drift defect.
const _absent: typeof Schema.OptionFromNonEmptyTrimmedString = Schema.OptionFromNonEmptyTrimmedString

// ONE table keyed on the enum the transport ships CLOSED: `Code` carries the sixteen integers, so a row never
// restates one and no inverse map stands beside the table — a code reads its row by index and a word reads its row
// through the derivation below. `class` is the branch taxonomy this roster ADOPTS; `retryable` and `terminal` are the
// gRPC peer's OWN columns carried verbatim, and neither derives from the class beside it. `retryable` is the peer's
// re-send verdict and it diverges from `Fault.Class.retryable` at exactly ONE row — `AlreadyExists` grades
// `conflicted`, whose branch band is `transient`, while an already-exists refusal never succeeds on a re-send — so
// folding the column into that projection would flip that row silently and stamp a re-drive an operator's own
// protocol forbids. `terminal` is a FAILOVER fact rather than a severity: it says this peer will refuse the call
// again, which is why four rows carry it where eleven carry a terminal class. `transport` is the fourth column and it
// exists so ONE table answers every code question: the egress fault below reads it instead of re-deriving a kind
// from bare code literals, and the runtime dial's retry gate reads `class` off this same row — a second grading beside
// it once disagreed on three codes, so an identical refusal retried or not by which module had dialled.
// `reason` is the estate's telemetry word for the code, the `wire.reason` attribute a capture band stamps.
const _hops = {
  [Code.Canceled]: { reason: "canceled", retryable: false, terminal: false, class: "defect", transport: "deadline" },
  [Code.Unknown]: { reason: "unknown", retryable: false, terminal: false, class: "defect", transport: "connectivity" },
  [Code.InvalidArgument]: { reason: "invalid", retryable: false, terminal: false, class: "invalid", transport: "connectivity" },
  [Code.DeadlineExceeded]: { reason: "deadline", retryable: true, terminal: false, class: "expired", transport: "deadline" },
  [Code.NotFound]: { reason: "notfound", retryable: false, terminal: false, class: "absent", transport: "connectivity" },
  [Code.AlreadyExists]: { reason: "exists", retryable: false, terminal: false, class: "conflicted", transport: "connectivity" },
  [Code.PermissionDenied]: { reason: "denied", retryable: false, terminal: true, class: "denied", transport: "connectivity" },
  [Code.ResourceExhausted]: { reason: "exhausted", retryable: true, terminal: false, class: "exhausted", transport: "ceiling" },
  [Code.FailedPrecondition]: { reason: "precondition", retryable: false, terminal: false, class: "invalid", transport: "connectivity" },
  [Code.Aborted]: { reason: "aborted", retryable: true, terminal: false, class: "conflicted", transport: "connectivity" },
  [Code.OutOfRange]: { reason: "range", retryable: false, terminal: false, class: "invalid", transport: "connectivity" },
  [Code.Unimplemented]: { reason: "unimplemented", retryable: false, terminal: true, class: "defect", transport: "connectivity" },
  [Code.Internal]: { reason: "internal", retryable: false, terminal: false, class: "defect", transport: "connectivity" },
  [Code.Unavailable]: { reason: "unavailable", retryable: true, terminal: false, class: "unavailable", transport: "connectivity" },
  [Code.DataLoss]: { reason: "dataloss", retryable: false, terminal: true, class: "breached", transport: "connectivity" },
  [Code.Unauthenticated]: { reason: "unauthenticated", retryable: false, terminal: true, class: "denied", transport: "connectivity" },
} as const satisfies Record<Code, {
  readonly reason: string
  readonly retryable: boolean
  readonly terminal: boolean
  readonly class: Fault.Class.Kind
  readonly transport: TransportKind
}>

declare namespace Hops {
  type Row = (typeof _hops)[Code]
  type Reason = Row["reason"]
  type Shape = {
    readonly at: (code: number) => Row
    readonly is: (input: unknown) => input is Reason
    readonly word: (reason: Reason) => Row
  }
}

// The enum's own membership guard is the one total read: a number no row names resolves through the `Unknown` row
// rather than a fallback spelled here, and a word reads its row off a derivation of the same table.
const _code = Schema.is(Schema.Enums(Code))
const _byWord = Record.fromEntries(
  Array.map(Record.values(_hops), (row) => [row.reason, row] as const),
) as { readonly [R in Hops.Reason]: Extract<Hops.Row, { readonly reason: R }> }
const _reason = Schema.is(Schema.Literal(...Array.map(Record.values(_hops), (row) => row.reason)))

const Hops: Hops.Shape = {
  at: (code) => (_code(code) ? _hops[code] : _hops[Code.Unknown]),
  is: (input): input is Hops.Reason => _reason(input),
  word: (reason) => _byWord[reason],
}

const _stamp = (tag: string): Schema.Schema<unknown, unknown> =>
  Schema.transform(Schema.Unknown, Schema.Unknown, {
    strict: true,
    decode: (raw) => (Predicate.isRecord(raw) ? { ...raw, _tag: tag } : raw),
    encode: Function.identity,
  })

const _WIRE_ATTR = { reason: "wire.reason", retryable: "wire.retryable", terminal: "wire.terminal" } as const

// Enrichment seats on the ROSTER rather than on any fault class, because what it enriches is a capture band the
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
              [_WIRE_ATTR.retryable]: Hops.word(reason).retryable,
              [_WIRE_ATTR.terminal]: Hops.word(reason).terminal,
            }),
        }),
      ),
  }),
)

// Well-known stamps land as the GENERATED shape, typed against the package's own descriptor so a field move at the
// well-known source breaks here, and the range refinements ride as filters over that shape because neither
// `fromBinary` nor the `wkt` bridges range-check: a stamp past the proto3 timestamp domain decodes clean and
// `timestampMs` folds it to a finite number that means nothing. Instants cross to the branch clock through the
// shipped `timestampMs`/`durationMs` bridges, never hand arithmetic over `seconds` and `nanos`.
const _ProtoTimestamp: Schema.Schema<MessageShape<typeof TimestampSchema>> = Format.proto.message(TimestampSchema).pipe(
  Schema.filter((stamp) =>
    stamp.seconds >= -62135596800n && stamp.seconds <= 253402300799n && stamp.nanos >= 0 && stamp.nanos <= 999999999
      || "<timestamp-range>"),
)
const _ProtoDuration: Schema.Schema<MessageShape<typeof DurationSchema>> = Format.proto.message(DurationSchema).pipe(
  Schema.filter((span) =>
    span.seconds >= 0n && span.seconds <= 315576000000n && span.nanos >= 0 && span.nanos <= 999999999
      || "<duration-range>"),
)
const _Instant: Schema.Schema<DateTime.Utc, MessageShape<typeof TimestampSchema>> = Schema.transform(
  _ProtoTimestamp,
  Schema.DateTimeUtcFromSelf,
  { strict: true, decode: (stamp) => DateTime.unsafeMake(timestampMs(stamp)), encode: (instant) => timestampFromMs(DateTime.toEpochMillis(instant)) },
)
const _Span: Schema.Schema<Duration.Duration, MessageShape<typeof DurationSchema>> = Schema.transform(
  _ProtoDuration,
  Schema.DurationFromSelf,
  { strict: true, decode: (span) => Duration.millis(durationMs(span)), encode: (span) => durationFromMs(Duration.toMillis(span)) },
)

// `FaultRecovery`'s throttled arm IS `google.rpc.RetryInfo`, so this estate detail and the standard `Status.details`
// seat generic middleware reads carry ONE message rather than two projections that can disagree. `_Advice` is this
// branch's single crossing between that message and a `Duration`, both directions on one owner. `retryDelay` is a
// message slot generators spell optional while the corpus rule forces it present, so that optionality collapses HERE
// at admission and no interior reader ever holds an absent window or a sentinel standing in for one.
const _Advice: Schema.Schema<Duration.Duration, MessageShape<typeof RetryInfoSchema>> = Schema.transformOrFail(
  Format.proto.message(RetryInfoSchema),
  Schema.DurationFromSelf,
  {
    strict: true,
    decode: (advice, _options, ast) =>
      advice.retryDelay === undefined
        ? Either.left(new ParseResult.Type(ast, advice, "<retry-delay-unset>"))
        : Either.mapLeft(Schema.decodeEither(_Span)(advice.retryDelay), (issue) => issue.issue),
    encode: (delay) =>
      Either.right(Format.proto.create(RetryInfoSchema, { retryDelay: Schema.encodeSync(_Span)(delay) })),
  },
)

// The HLC crosses as the corpus's own two-half message and lands as the branch clock owner: the physical half is
// already the 100-ns tick axis `Clock.Hlc` mints on, so no scaling stands between the wire and the value.
const _Stamp: Schema.Schema<Clock.Hlc, MessageShape<typeof HlcSchema>> = Schema.transform(
  Format.proto.message(HlcSchema),
  Clock.Hlc,
  {
    strict: true,
    decode: (hlc) => new Clock.Hlc({ physical: hlc.physical, logical: hlc.logical }),
    encode: (hlc) => Format.proto.create(HlcSchema, { physical: hlc.physical, logical: hlc.logical }),
  },
)

const _Recovery = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("terminal") }),
  Schema.Struct({ kind: Schema.Literal("transient") }),
  Schema.Struct({ kind: Schema.Literal("retryAfter"), delay: Schema.DurationFromSelf }),
)
// Remote fault CLASS elects off the producer's own typed recovery arm and never off the domain case beside it: a
// case band reaching a class is a second answer to retriability that contradicts the arm the producer sent — an
// `unavailable` hop arriving under a `terminal` arm graded `transient` through the hop roster and handed
// `Fault.Budget` a re-drive the peer had already refused. The case stays IDENTITY and the arm stays RECOVERY.
// `retryAfter` lands `exhausted` because that is the one branch band whose producers state their own window and whose
// raise carries `Fault.Class.After`; `transient` lands `unavailable`, the transient band a caller re-invokes
// identically under the budget; `terminal` lands `invalid`, the terminal caller-blamed band whose re-offer route is
// the material the caller must re-author. The keys are the generated oneof's own case names.
const _remoteClasses = {
  terminal: "invalid",
  transient: "unavailable",
  retryAfter: "exhausted",
} as const satisfies { readonly [K in (typeof _Recovery.Type)["kind"]]: Fault.Class.Kind }

// `FaultDetail` crosses SEVEN columns and the landing carries all seven off the generated message: `domain` names the
// producing family and `case` its closed ordinal under that domain — NEVER a transport code, which rides the Connect
// trailer beside it — so owner and case stay opaque here and no decoder rehydrates a remote taxonomy. `message` rides
// the `Status` the transport carries, which is why no column here holds one. `tenant` takes `_absent`, because the
// empty string a proto3 singular column carries for an unset value reads `Option.none()` at ONE seat. `violations`
// are the producer's `BadRequest.FieldViolation` rows, the protovalidate evidence a caller re-authors against.
class RemoteDetail extends Schema.Class<RemoteDetail>("RemoteDetail")({
  domain: Schema.NonEmptyString,
  case: Schema.Int.pipe(Schema.nonNegative()),
  correlation: Schema.Uint8ArrayFromSelf,
  stamp: Clock.Hlc,
  tenant: _absent,
  violations: Schema.Array(Format.proto.message(BadRequest_FieldViolationSchema)),
}) {}

class Remote extends Schema.TaggedError<Remote>()("Remote", {
  detail: RemoteDetail,
  recovery: _Recovery,
}) {
  // The landing reads the GENERATED detail and never a hand struct in producer order: the descriptor proves every
  // column and protovalidate proves every rule before this transform runs, so the two arms below move `recovery`
  // between the oneof face and the branch union and nothing else. A detail whose recovery arm is unset is refused by
  // the corpus rule and named here for the one path the type still admits.
  static readonly FromWire: Schema.Schema<Remote, MessageShape<typeof FaultDetailSchema>> = Schema.transformOrFail(
    Format.proto.message(FaultDetailSchema),
    Remote,
    {
      strict: true,
      decode: (wire, _options, ast) =>
        Either.map(
          Either.all({
            recovery: Match.value(wire.recovery.kind).pipe(
              Match.when({ case: "terminal" }, () => Either.right({ kind: "terminal" as const })),
              Match.when({ case: "transient" }, () => Either.right({ kind: "transient" as const })),
              Match.when({ case: "retryAfter" }, ({ value }) =>
                Either.map(Schema.decodeEither(_Advice)(value), (delay) => ({ kind: "retryAfter" as const, delay }))),
              Match.orElse(() => Either.left(new ParseResult.Type(ast, wire, "<recovery-unset>"))),
            ),
            stamp: Schema.decodeEither(_Stamp)(wire.stamp),
            tenant: Schema.decodeEither(_absent)(wire.tenant),
          }),
          ({ recovery, stamp, tenant }) => new Remote({
            detail: new RemoteDetail({
              domain: wire.domain,
              case: wire.case,
              correlation: wire.correlation,
              stamp,
              tenant,
              violations: wire.violations,
            }),
            recovery,
          }),
        ).pipe(Either.mapLeft((issue) => issue instanceof ParseResult.ParseError ? issue.issue : issue)),
      encode: (fault) =>
        Either.right(Format.proto.create(FaultDetailSchema, {
          domain: fault.detail.domain,
          case: fault.detail.case,
          correlation: fault.detail.correlation,
          stamp: Schema.encodeSync(_Stamp)(fault.detail.stamp),
          tenant: Option.getOrElse(fault.detail.tenant, () => ""),
          recovery: Format.proto.create(FaultRecoverySchema, {
            kind: fault.recovery.kind === "retryAfter"
              ? { case: "retryAfter", value: Schema.encodeSync(_Advice)(fault.recovery.delay) }
              : { case: fault.recovery.kind, value: Format.proto.create(EmptySchema) },
          }),
          violations: fault.detail.violations,
        })),
    },
  )
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
  get after(): Fault.Class.After {
    return this.recovery.kind === "retryAfter" ? Option.some(this.recovery.delay) : Option.none()
  }
  override get message(): string {
    return `<remote:${this.detail.domain}/${this.detail.case}>`
  }
}

const _transportKinds = ["connectivity", "deadline", "ceiling"] as const
// Both reads DERIVE off the one code roster: `kindOf` spends its `transport` column and `denied` its `class`, so a
// gRPC code is answered by the table that already rows it and never by a literal ladder beside it. A code no row
// names resolves through the roster's own `Unknown` arm rather than through a fallback spelled here.
class Transport extends Schema.TaggedError<Transport>()("Transport", {
  kind: Schema.Literal(..._transportKinds),
  // the hop's class rides the VALUE so the dial's retry gate and the problem door read one row's verdict off the
  // fault itself; it is the table's column, never a raise-supplied grade
  class: Fault.Class.schema,
  detail: Schema.String,
}) {
  static kindOf(code: number): TransportKind {
    return Hops.at(code).transport
  }
  static denied(code: number): boolean {
    return Hops.at(code).class === "denied"
  }
  static classOf(code: number): Fault.Class.Kind {
    return Hops.at(code).class
  }
}

class MalformedDetail extends Schema.TaggedError<MalformedDetail>()("MalformedDetail", {
  detail: Schema.String,
}) {}

// Invocation-boundary outcomes are one closed reason space, so the egress census mounts ONE roster and
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

// `_invokeCensus` publishes the roster as a VOCABULARY rather than the bare tuple it was: `Convention.tracked` and
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

// --- [APPEARANCE]

// The appearance vocabulary is the corpus's own: every roster below is a generated enum of `rasm.contracts.appearance.v1`,
// so the tuple of admitted members DERIVES from the descriptor and this page mints no spelling. What this page OWNS is
// the legality column beside each member — whether a transfer reaches a channel plane, which store a browser
// transcoder can read, the neutral a packed slot fills with — and every column table closes against the enum's own
// defined members in both directions, so a member the corpus adds fails the table at its declaration and a row this
// page invents fails the enum. `UNSPECIFIED` is excluded at the type: protovalidate's `not_in: [0]` refuses it at
// admission, and the type-level exclusion is what lets a row table index by a decoded member with no guard.
type _Defined<E extends { readonly UNSPECIFIED: 0 }> = Exclude<E[keyof E], E["UNSPECIFIED"]>
const _defined = <E extends { readonly UNSPECIFIED: 0 }>(members: E): ReadonlyArray<_Defined<E>> =>
  Array.filter(Record.values(members), (member): member is _Defined<E> => member !== members.UNSPECIFIED)

// `_transferRows` carries the transfer roster beside the one column the frozen fragment's own legality clause states:
// `plane` is true where the tag reaches a channel plane at all. The scene-referred subset DERIVES from that column
// under a two-way guard, so a sixth tag is one row and neither the roster nor the subset can drift off the other.
// `PQ`/`HLG` are display transfers the C#-interior environment wire alone admits — both set documents carry
// roster-keyed CHANNEL planes, and a display-referred channel plane forks its stored value from its shading value.
const _transferRows = {
  [appearance.Transfer.LINEAR]: { plane: true },
  [appearance.Transfer.SRGB]: { plane: true },
  [appearance.Transfer.RAW]: { plane: true },
  [appearance.Transfer.PQ]: { plane: false },
  [appearance.Transfer.HLG]: { plane: false },
} as const satisfies { readonly [K in _Defined<typeof appearance.Transfer>]: { readonly plane: boolean } }
type _PlaneTagged = {
  readonly [K in _Defined<typeof appearance.Transfer>]: (typeof _transferRows)[K]["plane"] extends true ? K : never
}[_Defined<typeof appearance.Transfer>]
const _sceneTransfers = [appearance.Transfer.LINEAR, appearance.Transfer.SRGB, appearance.Transfer.RAW] as const
type _SceneTransfer = (typeof _sceneTransfers)[number]
type _SceneWhole<K extends _PlaneTagged = _SceneTransfer> = K
type _SceneClosed<K extends _SceneTransfer = _PlaneTagged> = K

// `_depthRows` splits the store class on the two axes the wire laws read: `integer` decides which transfer a color
// channel is authored under, and `deep` decides what an 8-bit-only encoder leg and a lossy association conversion admit.
const _depthRows = {
  [appearance.Depth.U8]: { integer: true, deep: false },
  [appearance.Depth.U16]: { integer: true, deep: true },
  [appearance.Depth.F16]: { integer: false, deep: true },
  [appearance.Depth.F32]: { integer: false, deep: true },
} as const satisfies { readonly [K in _Defined<typeof appearance.Depth>]: { readonly integer: boolean; readonly deep: boolean } }

// Three physical units ride the roster's own channels; every other channel is a dimensionless ratio, an index,
// or a normalized field and declares none. The column is what gives the millimetre height span, the nanometre
// thin-film thickness, and the photometric emission floor a declared home instead of a bind-site guess.
const _units = ["mm", "nm", "cd/m2"] as const

// Channel facts carry the roster's five wire-bearing columns and NOTHING derived from them: `ch` the semantic
// component count, `transfer` the tag the channel is authored under, `neutral` the constant an absent packed slot, a
// mip gutter, and a UDIM hole fill with, `unit` the physical unit the value is expressed in, and `mip` the declared
// fold. Every plane law READS those five — the colorimetric class, the depth-coupled transfer, the storage-width
// floor, the admissible fold, and the scalar companion a false pack slot stamps — so a new channel is ONE row and no
// predicate widens. The arity distributes ONCE here, so a three-band neutral on a one-component channel cannot be
// written; a boolean standing in for `transfer` folds `LINEAR` and `RAW` into one class and admits the IOR rows as
// light quantities, and `null` on `unit` names a dimensionless ratio, an index, or a normalized field.
type _ChannelFacts<C extends 1 | 3 = 1 | 3> = C extends unknown ? {
    readonly ch: C
    readonly transfer: _SceneTransfer
    readonly neutral: C extends 1 ? readonly [number] : readonly [number, number, number]
    readonly unit: Texture.Unit | null
    readonly mip: Exclude<Texture.MipPolicy, typeof appearance.MipPolicy.NONE>
  }
  : never

const { Role: _R, MipPolicy: _M, Transfer: _T } = appearance
const _channelRows = {
  [_R.BASE_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [1], unit: null, mip: _M.BOX },
  [_R.BASE_COLOR]: { ch: 3, transfer: _T.SRGB, neutral: [0.8, 0.8, 0.8], unit: null, mip: _M.KAISER },
  [_R.BASE_METALNESS]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.BASE_DIFFUSE_ROUGHNESS]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.ROUGHNESS_VARIANCE },
  [_R.BASE_SPECULAR_TINT]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.SPECULAR_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [1], unit: null, mip: _M.BOX },
  [_R.SPECULAR_COLOR]: { ch: 3, transfer: _T.SRGB, neutral: [1, 1, 1], unit: null, mip: _M.KAISER },
  [_R.SPECULAR_ROUGHNESS]: { ch: 1, transfer: _T.LINEAR, neutral: [0.3], unit: null, mip: _M.ROUGHNESS_VARIANCE },
  [_R.SPECULAR_ROUGHNESS_ANISOTROPY]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  // scalar anisotropy-direction planes mip correctly under box where a tangent vector plane cancels
  [_R.SPECULAR_ROUGHNESS_ANISOTROPY_ROTATION]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.SPECULAR_IOR]: { ch: 1, transfer: _T.RAW, neutral: [1.5], unit: null, mip: _M.BOX },
  [_R.TRANSMISSION_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.TRANSMISSION_ROUGHNESS]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.ROUGHNESS_VARIANCE },
  [_R.SUBSURFACE_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  // a 3-band mean-free-path carrier in millimetres, never a colorimetric triple
  [_R.SUBSURFACE_RADIUS]: { ch: 3, transfer: _T.RAW, neutral: [1, 0.5, 0.25], unit: "mm", mip: _M.BOX },
  [_R.COAT_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.COAT_COLOR]: { ch: 3, transfer: _T.SRGB, neutral: [1, 1, 1], unit: null, mip: _M.KAISER },
  [_R.COAT_ROUGHNESS]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.ROUGHNESS_VARIANCE },
  [_R.COAT_IOR]: { ch: 1, transfer: _T.RAW, neutral: [1.6], unit: null, mip: _M.BOX },
  [_R.FUZZ_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.FUZZ_COLOR]: { ch: 3, transfer: _T.SRGB, neutral: [1, 1, 1], unit: null, mip: _M.KAISER },
  [_R.FUZZ_ROUGHNESS]: { ch: 1, transfer: _T.LINEAR, neutral: [0.5], unit: null, mip: _M.ROUGHNESS_VARIANCE },
  [_R.THIN_FILM_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  // nanometres on the column; the micrometre divide is the `.mtlx` egress edge's
  [_R.THIN_FILM_THICKNESS]: { ch: 1, transfer: _T.RAW, neutral: [500], unit: "nm", mip: _M.BOX },
  [_R.THIN_FILM_IOR]: { ch: 1, transfer: _T.RAW, neutral: [1.4], unit: null, mip: _M.BOX },
  [_R.EMISSION_COLOR]: { ch: 3, transfer: _T.SRGB, neutral: [1, 1, 1], unit: null, mip: _M.KAISER },
  [_R.EMISSION_LUMINANCE]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: "cd/m2", mip: _M.BOX },
  [_R.GEOMETRY_OPACITY]: { ch: 1, transfer: _T.LINEAR, neutral: [1], unit: null, mip: _M.BOX },
  [_R.GEOMETRY_NORMAL]: { ch: 3, transfer: _T.RAW, neutral: [0, 0, 1], unit: null, mip: _M.NORMAL_RENORMALIZE },
  [_R.GEOMETRY_COAT_NORMAL]: { ch: 3, transfer: _T.RAW, neutral: [0, 0, 1], unit: null, mip: _M.NORMAL_RENORMALIZE },
  [_R.GEOMETRY_TANGENT]: { ch: 3, transfer: _T.RAW, neutral: [1, 0, 0], unit: null, mip: _M.NORMAL_RENORMALIZE },
  [_R.GEOMETRY_COAT_TANGENT]: { ch: 3, transfer: _T.RAW, neutral: [1, 0, 0], unit: null, mip: _M.NORMAL_RENORMALIZE },
  // normalized [0,1]; the millimetre span rides the document's `heightScaleMm`, never the column
  [_R.HEIGHT]: { ch: 1, transfer: _T.RAW, neutral: [0.5], unit: null, mip: _M.BOX },
  [_R.OCCLUSION]: { ch: 1, transfer: _T.LINEAR, neutral: [1], unit: null, mip: _M.BOX },
  // signed [-1,1]; an integer store carries the halved encoding
  [_R.CURVATURE]: { ch: 1, transfer: _T.RAW, neutral: [0], unit: null, mip: _M.BOX },
} as const satisfies { readonly [K in Texture.Role]: _ChannelFacts }

// Color channels author their transfer to FOLLOW their store — the roster's `SRGB` rows encode display-referred at
// integer depth and scene-linear at float depth, and every other row is transfer-invariant across depth. Reading the
// roster column raw admits `BASE_COLOR` as a linear 8-bit plane the shading rail then decodes a second time.
const _authored = (role: Texture.Role, depth: Texture.Depth): _SceneTransfer =>
  _channelRows[role].transfer === _T.SRGB && !_depthRows[depth].integer ? _T.LINEAR : _channelRows[role].transfer

// Roster folds declare a channel's DEFAULT and `BOX` its floor; `NONE` names the single-level plane alone, so a
// pyramid depth and a fold policy never disagree. That same column fixes the direction class — the
// `NORMAL_RENORMALIZE` rows ARE the direction triples, and only a direction plane may store two components and
// reconstruct the third, so a three-band millimetre carrier in `RG16` loses its third band with nothing to recover it.
const _mipLawful = (role: Texture.Role, mips: number, policy: Texture.MipPolicy): boolean =>
  mips === 1 ? policy === _M.NONE : policy === _M.BOX || policy === _channelRows[role].mip
const _widthFloor = (role: Texture.Role): 1 | 2 | 4 =>
  _channelRows[role].ch === 1 ? 1 : _channelRows[role].mip === _M.NORMAL_RENORMALIZE ? 2 : 4

// `_planeRows` projects each storage key onto the three facts the wire laws read: its store class, its texel width,
// and `web` — whether a browser transcoder can reach the store at all. The one-and-two-component sixteen-bit integer
// stores have no Vulkan format row in the KTX2 read path, so they are producer-side and desktop-native only, which
// is load-bearing precisely where `_widthFloor` routes every direction plane to width 2: the natural high-precision
// normal store is the undecodable one. Widths run 1, 2, and 4 because those ARE the uncompressed texel widths a GPU
// accepts; three channels are refused on the STORE's own ground, and the corpus roster carries no three-component row.
const { PlaneFormat: _P, Depth: _D } = appearance
const _planeRows = {
  [_P.R8]: { depth: _D.U8, width: 1, web: true }, [_P.R16]: { depth: _D.U16, width: 1, web: false },
  [_P.R16F]: { depth: _D.F16, width: 1, web: true }, [_P.R32F]: { depth: _D.F32, width: 1, web: true },
  [_P.RG8]: { depth: _D.U8, width: 2, web: true }, [_P.RG16]: { depth: _D.U16, width: 2, web: false },
  // float two-component stores carry Vulkan format rows, so RG16F re-routes the direction planes RG16 cannot serve
  [_P.RG16F]: { depth: _D.F16, width: 2, web: true }, [_P.RG32F]: { depth: _D.F32, width: 2, web: true },
  [_P.RGBA8]: { depth: _D.U8, width: 4, web: true }, [_P.RGBA16]: { depth: _D.U16, width: 4, web: true },
  [_P.RGBA16F]: { depth: _D.F16, width: 4, web: true }, [_P.RGBA32F]: { depth: _D.F32, width: 4, web: true },
} as const satisfies { readonly [K in Texture.PlaneFormat]: { readonly depth: Texture.Depth; readonly width: 1 | 2 | 4; readonly web: boolean } }

// `_payloadRows` carries all five KTX2 payload classes beside the three columns their refusals read: `wire` the
// legality the viewer's Basis transcoder path decides, `block` whether the file holds block data direct, and `ldr` the
// MEASURED 8-bit store bound both encoder legs raise on. The wire subset derives from `wire` under a two-way guard, so
// admitting a future transcodable payload is one column flip and every filter follows it with no literal to chase.
const _K = appearance.KtxPayload
const _payloadRows = {
  [_K.RAW_BCN]: { wire: false, block: true, ldr: true },
  [_K.UASTC]: { wire: true, block: false, ldr: true },
  [_K.ETC1S]: { wire: true, block: false, ldr: true },
  [_K.ASTC]: { wire: false, block: true, ldr: true },
  [_K.NONE]: { wire: true, block: false, ldr: false },
} as const satisfies { readonly [K in Texture.Payload]: { readonly wire: boolean; readonly block: boolean; readonly ldr: boolean } }
type _Wired = {
  readonly [K in Texture.Payload]: (typeof _payloadRows)[K]["wire"] extends true ? K : never
}[Texture.Payload]
const _wirePayloads = [_K.UASTC, _K.ETC1S, _K.NONE] as const
type _PayloadWired<K extends _Wired = Texture.WirePayload> = K
type _PayloadClosed<K extends Texture.WirePayload = _Wired> = K

// `_containerRows` carries the corpus's file-container roster WHOLE beside the three columns the wire laws read:
// `alpha` the canonical association encode converts to (the jxl/avif rows are the measured no-premultiplication-seat
// posture of the provisioned encoders), `pyramid` whether the file holds its OWN mip chain, which is the column the
// plane-level list law generates its length off, and `plane` whether the container reaches a CHANNEL plane at all.
const { Container: _C, AlphaMode: _A } = appearance
const _containerRows = {
  [_C.PNG16]: { alpha: _A.STRAIGHT, pyramid: false, plane: true },
  [_C.TIFF16]: { alpha: _A.STRAIGHT, pyramid: false, plane: true },
  [_C.TIFF_F32]: { alpha: _A.STRAIGHT, pyramid: false, plane: true },
  [_C.WEBP]: { alpha: _A.STRAIGHT, pyramid: false, plane: true },
  [_C.QOI]: { alpha: _A.STRAIGHT, pyramid: false, plane: false },
  [_C.EXR]: { alpha: _A.ASSOCIATED, pyramid: false, plane: true },
  [_C.EXR_DEEP]: { alpha: _A.ASSOCIATED, pyramid: false, plane: true },
  [_C.HDR]: { alpha: _A.NONE, pyramid: false, plane: true },
  [_C.KTX2]: { alpha: _A.STRAIGHT, pyramid: true, plane: true },
  [_C.JXL]: { alpha: _A.STRAIGHT, pyramid: false, plane: true },
  [_C.JXL_F16]: { alpha: _A.STRAIGHT, pyramid: false, plane: true },
  [_C.AVIF12]: { alpha: _A.STRAIGHT, pyramid: false, plane: true },
} as const satisfies { readonly [K in Texture.Container]: { readonly alpha: Texture.AlphaMode; readonly pyramid: boolean; readonly plane: boolean } }
// Straight-to-associated conversion quantizes catastrophically at low alpha below 16 bits, so a plane whose container
// fixes an association differing from its declared mode admits at a deep store alone; a `NONE` plane carries nothing
// to convert and passes whole.
const _associationLawful = (mode: Texture.AlphaMode, container: Texture.Container, depth: Texture.Depth): boolean =>
  mode === _A.NONE || _containerRows[container].alpha === mode || _depthRows[depth].deep

// `_layerRows` fixes the extent each layer law admits where the concept has one — an unlayered set holds one plane and
// a cube holds six faces; the open laws bound their extent at the producer, so `null` imposes nothing here. `gltf` is
// whether a 2D bind reaches the law at all, exactly as `_packRows.gltf` is for slot order.
const _L = appearance.LayerLaw
const _layerRows = {
  [_L.NONE]: { extent: 1, gltf: true }, [_L.CUBE_FACES]: { extent: 6, gltf: false },
  [_L.ARRAY]: { extent: null, gltf: false }, [_L.VOLUME]: { extent: null, gltf: false }, [_L.FRAMES]: { extent: null, gltf: false },
} as const satisfies { readonly [K in Texture.LayerLaw]: { readonly extent: number | null; readonly gltf: boolean } }
// `_packRows` fixes each packing order in slot order beside the ONE legality column the fragment states: `slots` is the
// roster `present` indexes, so a packed channel is addressed by its pack row and the roster names which standalone
// plane row then cannot exist, and `gltf` is whether the order crosses to a glTF consumer at all.
const _packRows = {
  [appearance.Pack.ORM]: { slots: [_R.OCCLUSION, _R.SPECULAR_ROUGHNESS, _R.BASE_METALNESS], gltf: true },
  [appearance.Pack.MRA]: { slots: [_R.BASE_METALNESS, _R.SPECULAR_ROUGHNESS, _R.OCCLUSION], gltf: false },
} as const satisfies { readonly [K in Texture.Pack]: { readonly slots: readonly [Texture.Role, Texture.Role, Texture.Role]; readonly gltf: boolean } }

const _contentKey = Schema.is(Schema.typeSchema(Digest.codecs.content.bytes))
const _artifactIdentity = Schema.is(ArtifactId.Key)
type _ArtifactRefWire = MessageShape<typeof artifact.ArtifactRefSchema>
type _ArtifactRefValid = MessageValidType<typeof artifact.ArtifactRefSchema>
type _ArtifactRefLanded = Omit<_ArtifactRefValid, "sha256"> & { readonly sha256: ArtifactId.Identity }
const _artifactRefLanded = (input: unknown): input is _ArtifactRefLanded =>
  Predicate.isRecord(input) && _artifactIdentity(input.sha256) && typeof input.artifactBytes === "bigint"
const _ArtifactRefLanded = Schema.declare(_artifactRefLanded, { identifier: "ArtifactReference" })
const _artifactRef: Schema.Schema<_ArtifactRefLanded, _ArtifactRefWire> = Schema.transformOrFail(
  Format.proto.message(artifact.ArtifactRefSchema),
  _ArtifactRefLanded,
  {
    strict: true,
    decode: (reference) => Either.map(
      Either.mapLeft(Schema.decodeEither(ArtifactId.codec.bytes)(reference.sha256), (error) => error.issue),
      (sha256) => ({ ...reference, sha256 }),
    ),
    encode: (reference) => Either.map(
      Either.mapLeft(Schema.encodeEither(ArtifactId.codec.bytes)(reference.sha256), (error) => error.issue),
      (sha256) => ({ ...reference, sha256 }),
    ),
  },
)

type _ArtifactFrameWire = MessageShape<typeof artifact.ArtifactFrameSchema>
type _ArtifactFrameValid = MessageValidType<typeof artifact.ArtifactFrameSchema>
type _ArtifactFrameLanded = Omit<_ArtifactFrameValid, "artifact"> & { readonly artifact: _ArtifactRefLanded }
const _artifactFrameLanded = (input: unknown): input is _ArtifactFrameLanded =>
  Predicate.isRecord(input) && Predicate.isUint8Array(input.payload) && _artifactRefLanded(input.artifact)
const _ArtifactFrameLanded = Schema.declare(_artifactFrameLanded, { identifier: "ArtifactFrame" })
const _artifactFrame: Schema.Schema<_ArtifactFrameLanded, _ArtifactFrameWire> = Schema.transformOrFail(
  Format.proto.message(artifact.ArtifactFrameSchema),
  _ArtifactFrameLanded,
  {
    strict: true,
    decode: (frame) => Either.map(
      Either.mapLeft(Schema.decodeEither(_artifactRef)(frame.artifact), (error) => error.issue),
      (artifact) => ({ ...frame, artifact }),
    ),
    encode: (frame) => Either.map(
      Either.mapLeft(Schema.encodeEither(_artifactRef)(frame.artifact), (error) => error.issue),
      (artifact) => ({ ...frame, artifact }),
    ),
  },
)

type _PlaneRefWire = MessageShape<typeof appearance.PlaneRefSchema>
type _PlaneRefValid = MessageValidType<typeof appearance.PlaneRefSchema>
type _PlaneRefLanded = Omit<_PlaneRefValid, "artifact"> & { readonly artifact: _ArtifactRefLanded }
const _referenceLanded = (input: unknown): input is _PlaneRefLanded =>
  Predicate.isRecord(input) && _artifactRefLanded(input.artifact)
const _ReferenceLanded = Schema.declare(_referenceLanded, { identifier: "PlaneReference" })
const _reference: Schema.Schema<_PlaneRefLanded, _PlaneRefWire> = Schema.transformOrFail(
  Format.proto.message(appearance.PlaneRefSchema),
  _ReferenceLanded,
  {
    strict: true,
    decode: (reference) => Either.map(
      Either.mapLeft(Schema.decodeEither(_artifactRef)(reference.artifact), (error) => error.issue),
      (artifact) => ({ ...reference, artifact }),
    ),
    encode: (reference) => Either.map(
      Either.mapLeft(Schema.encodeEither(_artifactRef)(reference.artifact), (error) => error.issue),
      (artifact) => ({ ...reference, artifact }),
    ),
  },
)

const Artifact = {
  identity: ArtifactId.Key,
  reference: _artifactRef,
  frame: _artifactFrame,
  mint: ArtifactId.mint,
} as const

declare namespace Artifact {
  type Identity = ArtifactId.Identity
  type Reference = _ArtifactRefLanded
  type Frame = _ArtifactFrameLanded
}

// ONE exported anchor for the appearance vocabulary: every roster tuple derives from its generated enum through
// `_defined`, each key type derives from the enum, and this page's own wire-legality column tables ride `rows` —
// channel, container, depth, layer, plane, pack — because the refusals those columns declare are the ui bind's to
// RAISE, and a consumer raising over a column must be able to read it. FOREIGN columns (the data plane's CLI and
// data-format columns) stay with their owners and key off these enums, so a corpus roster move breaks at ONE
// declaration in every module. Every row table is `as const satisfies` — a mapped ANNOTATION erases the row literals,
// collapsing every `extends true` derivation beside it (`_Wired`) to `never` while it reads correct.
declare namespace Texture {
  type AlphaMode = _Defined<typeof appearance.AlphaMode>
  type Container = _Defined<typeof appearance.Container>
  type Convention = _Defined<typeof appearance.NormalConvention>
  type Depth = _Defined<typeof appearance.Depth>
  type LayerLaw = _Defined<typeof appearance.LayerLaw>
  type MipPolicy = _Defined<typeof appearance.MipPolicy>
  type Pack = _Defined<typeof appearance.Pack>
  type Payload = _Defined<typeof appearance.KtxPayload>
  type PlaneFormat = _Defined<typeof appearance.PlaneFormat>
  type Primaries = _Defined<typeof appearance.Primaries>
  type Role = _Defined<typeof appearance.Role>
  type Transfer = _Defined<typeof appearance.Transfer>
  type Unit = (typeof _units)[number]
  type WirePayload = (typeof _wirePayloads)[number]
  type Shape = Types.Simplify<{
    readonly alphaModes: ReadonlyArray<AlphaMode>
    readonly containers: ReadonlyArray<Container>
    readonly conventions: ReadonlyArray<Convention>
    readonly depths: ReadonlyArray<Depth>
    readonly layerLaws: ReadonlyArray<LayerLaw>
    readonly mipPolicies: ReadonlyArray<MipPolicy>
    readonly packs: ReadonlyArray<Pack>
    readonly payloads: ReadonlyArray<Payload>
    readonly planeFormats: ReadonlyArray<PlaneFormat>
    readonly primaries: ReadonlyArray<Primaries>
    readonly roles: ReadonlyArray<Role>
    readonly transfers: ReadonlyArray<Transfer>
    readonly units: typeof _units
    readonly wirePayloads: typeof _wirePayloads
    readonly rows: Types.Simplify<{
      readonly channel: typeof _channelRows
      readonly container: typeof _containerRows
      readonly depth: typeof _depthRows
      readonly layer: typeof _layerRows
      readonly plane: typeof _planeRows
      readonly pack: typeof _packRows
      readonly payload: typeof _payloadRows
      readonly transfer: typeof _transferRows
    }>
    readonly authored: typeof _authored
    readonly reference: typeof _reference
    readonly mipLawful: typeof _mipLawful
    readonly widthFloor: typeof _widthFloor
    readonly associationLawful: typeof _associationLawful
  }>
}

const Texture: Texture.Shape = {
  alphaModes: _defined(appearance.AlphaMode),
  containers: _defined(appearance.Container),
  conventions: _defined(appearance.NormalConvention),
  depths: _defined(appearance.Depth),
  layerLaws: _defined(appearance.LayerLaw),
  mipPolicies: _defined(appearance.MipPolicy),
  packs: _defined(appearance.Pack),
  payloads: _defined(appearance.KtxPayload),
  planeFormats: _defined(appearance.PlaneFormat),
  primaries: _defined(appearance.Primaries),
  roles: _defined(appearance.Role),
  transfers: _defined(appearance.Transfer),
  units: _units,
  wirePayloads: _wirePayloads,
  rows: {
    channel: _channelRows, container: _containerRows, depth: _depthRows, layer: _layerRows, plane: _planeRows,
    pack: _packRows, payload: _payloadRows, transfer: _transferRows,
  },
  authored: _authored,
  reference: _reference,
  mipLawful: _mipLawful,
  widthFloor: _widthFloor,
  associationLawful: _associationLawful,
}

// The plane-set document lands under its generated product oneof; no enum can contradict the selected arm. The
// refinements above the descriptor are the laws no field rule can state: a plane's fold policy agrees with its role's
// declared fold, its container's association agrees with its alpha mode at its depth, a self-pyramiding container
// holds one level where every other holds one per level, and the channel roster runs in roster order. Every scalar
// rule — ranges, sixteen-byte keys, unique UDIM tiles, the baked-set and environment-kind couplings — is
// protovalidate's and is not restated here. Enum columns arrive already narrowed by `defined_only`/`not_in: [0]`,
// and the filters read them by their generated members.
const _planeLawful = (plane: _PlaneDefined): boolean =>
  Option.match(Option.all([Option.fromNullable(plane.mipPolicy), Option.fromNullable(plane.depth), Option.fromNullable(plane.alphaMode)]), {
    onNone: () => true,
    onSome: ([policy, depth, alpha]) =>
      _reads(plane.role, policy, depth, alpha, plane.container, plane.mips, plane.levels.length),
  })
const _reads = (
  role: Texture.Role,
  policy: appearance.MipPolicy,
  depth: appearance.Depth,
  alpha: appearance.AlphaMode,
  container: Texture.Container,
  mips: number,
  held: number,
): boolean =>
  _mip(policy) && _depth(depth) && _alpha(alpha)
  && _mipLawful(role, mips, policy)
  && _associationLawful(alpha, container, depth)
  && (_containerRows[container].pyramid ? held === 1 : held === mips)
const _role = Schema.is(Schema.Literal(..._defined(appearance.Role)))
const _mip = Schema.is(Schema.Literal(..._defined(appearance.MipPolicy)))
const _depth = Schema.is(Schema.Literal(..._defined(appearance.Depth)))
const _alpha = Schema.is(Schema.Literal(..._defined(appearance.AlphaMode)))
const _container = Schema.is(Schema.Literal(..._defined(appearance.Container)))

const _rosterOrdered = (planes: ReadonlyArray<{ readonly role: appearance.Role }>): boolean =>
  Array.every(Array.zip(planes, Array.drop(planes, 1)), ([was, is]) => was.role < is.role)

// The landed TYPE narrows what the corpus rules prove: `defined_only`/`not_in: [0]` refuse an unknown member at
// admission, and the predicate below carries that proof into the type so every consumer indexes a legality row
// by a decoded column with no guard of its own. A plane's optional columns stay optional — the producer omits them
// where a container fixes them — and a consumer lifts absence at its seam.
type _SetWire = MessageShape<typeof appearanceSet.SetSchema>
type _SetValid = MessageValidType<typeof appearanceSet.SetSchema>
type _SurfaceValid = MessageValidType<typeof appearanceSet.SurfaceSetSchema>
type _EnvironmentPlaneValid = MessageValidType<typeof appearanceEnvironment.EnvironmentPlaneSchema>
type _PlaneDefined = _SurfaceValid["planes"][number] & {
  readonly role: Texture.Role
  readonly format: Texture.PlaneFormat
  readonly container: Texture.Container
  readonly ktxPayload: Texture.Payload
}
type _PackDefined = _SurfaceValid["packs"][number] & {
  readonly pack: Texture.Pack
  readonly present: ReadonlyArray<Texture.Role>
  readonly format: Texture.PlaneFormat
  readonly container: Texture.Container
}
type _SurfaceDefined = Omit<_SurfaceValid, "layerLaw" | "normalConvention" | "alphaMode" | "planes" | "packs"> & {
  readonly layerLaw: Texture.LayerLaw
  readonly normalConvention: Texture.Convention
  readonly alphaMode: Texture.AlphaMode
  readonly planes: ReadonlyArray<_PlaneDefined>
  readonly packs: ReadonlyArray<_PackDefined>
}
type _EnvironmentPlaneDefined = Omit<_EnvironmentPlaneValid, "container" | "format" | "transfer" | "primaries" | "depth" | "layerLaw" | "ktxPayload" | "alphaMode"> & {
  readonly container: Texture.Container
  readonly format: Texture.PlaneFormat
  readonly transfer: Texture.Transfer
  readonly primaries: Texture.Primaries
  readonly depth: Texture.Depth
  readonly layerLaw: Texture.LayerLaw
  readonly ktxPayload: Texture.Payload
  readonly alphaMode: Texture.AlphaMode
}
type _EnvironmentSourceDefined = Omit<MessageValidType<typeof appearanceEnvironment.EnvironmentSourceSchema>, "equirect" | "cubemap" | "preview"> & {
  readonly equirect: _EnvironmentPlaneDefined
  readonly cubemap?: _EnvironmentPlaneDefined
  readonly preview?: _EnvironmentPlaneDefined
}
type _IblDefined = Omit<MessageValidType<typeof appearanceEnvironment.IblSchema>, "source" | "specular" | "brdfLut" | "luminanceCdf"> & {
  readonly source: _EnvironmentSourceDefined
  readonly specular: ReadonlyArray<_EnvironmentPlaneDefined>
  readonly brdfLut: _EnvironmentPlaneDefined
  readonly luminanceCdf?: _EnvironmentPlaneDefined
}
type _EnvironmentDefined = Omit<MessageValidType<typeof appearanceSet.EnvironmentSetSchema>, "product"> & {
  readonly product:
    | { readonly case: "hdri"; readonly value: Omit<MessageValidType<typeof appearanceEnvironment.HdriSchema>, "source"> & { readonly source: _EnvironmentSourceDefined } }
    | { readonly case: "ibl"; readonly value: _IblDefined }
}
type _SetDefined = Omit<_SetValid, "product"> & {
  readonly product:
    | { readonly case: "pbr"; readonly value: _SurfaceDefined }
    | { readonly case: "baked"; readonly value: Omit<MessageValidType<typeof appearanceSet.BakedSetSchema>, "surface"> & { readonly surface: _SurfaceDefined } }
    | { readonly case: "environment"; readonly value: _EnvironmentDefined }
}
const _layer = Schema.is(Schema.Literal(..._defined(appearance.LayerLaw)))
const _convention = Schema.is(Schema.Literal(..._defined(appearance.NormalConvention)))
const _format = Schema.is(Schema.Literal(..._defined(appearance.PlaneFormat)))
const _payload = Schema.is(Schema.Literal(..._defined(appearance.KtxPayload)))
const _pack = Schema.is(Schema.Literal(..._defined(appearance.Pack)))
const _transfer = Schema.is(Schema.Literal(..._defined(appearance.Transfer)))
const _primaries = Schema.is(Schema.Literal(..._defined(appearance.Primaries)))
const _planeDefined = (plane: _SurfaceValid["planes"][number]): plane is _PlaneDefined =>
  _role(plane.role) && _format(plane.format) && _container(plane.container) && _payload(plane.ktxPayload)
const _packDefined = (pack: _SurfaceValid["packs"][number]): pack is _PackDefined =>
  _pack(pack.pack) && Array.every(pack.present, _role) && _format(pack.format) && _container(pack.container)
const _surfaceDefined = (surface: _SurfaceValid): surface is _SurfaceDefined =>
  _layer(surface.layerLaw) && _convention(surface.normalConvention) && _alpha(surface.alphaMode)
  && Array.every(surface.planes, _planeDefined) && Array.every(surface.packs, _packDefined)
const _environmentPlaneDefined = (plane: _EnvironmentPlaneValid): plane is _EnvironmentPlaneDefined =>
  _container(plane.container) && _format(plane.format) && _transfer(plane.transfer) && _primaries(plane.primaries)
  && _depth(plane.depth) && _layer(plane.layerLaw) && _payload(plane.ktxPayload)
  && _alpha(plane.alphaMode)
const _environmentSourceDefined = (source: MessageValidType<typeof appearanceEnvironment.EnvironmentSourceSchema>): source is _EnvironmentSourceDefined =>
  _environmentPlaneDefined(source.equirect)
  && (source.cubemap === undefined || _environmentPlaneDefined(source.cubemap))
  && (source.preview === undefined || _environmentPlaneDefined(source.preview))
const _iblDefined = (ibl: MessageValidType<typeof appearanceEnvironment.IblSchema>): ibl is _IblDefined =>
  _environmentSourceDefined(ibl.source) && Array.every(ibl.specular, _environmentPlaneDefined)
  && _environmentPlaneDefined(ibl.brdfLut)
  && (ibl.luminanceCdf === undefined || _environmentPlaneDefined(ibl.luminanceCdf))
const _environmentDefined = (environment: MessageValidType<typeof appearanceSet.EnvironmentSetSchema>): environment is _EnvironmentDefined =>
  Match.value(environment.product).pipe(Match.discriminatorsExhaustive("case")({
    hdri: ({ value }) => _environmentSourceDefined(value.source),
    ibl: ({ value }) => _iblDefined(value),
    undefined: () => false,
  }))
const _setDefined = (set: _SetValid): set is _SetDefined =>
  Match.value(set.product).pipe(Match.discriminatorsExhaustive("case")({
    pbr: ({ value }) => _surfaceDefined(value),
    baked: ({ value }) => _surfaceDefined(value.surface),
    environment: ({ value }) => _environmentDefined(value),
    undefined: () => false,
  }))
const _surfaceOf = (set: _SetDefined): _SurfaceDefined | undefined =>
  Match.value(set.product).pipe(Match.discriminatorsExhaustive("case")({
    pbr: ({ value }) => value,
    baked: ({ value }) => value.surface,
    environment: () => undefined,
  }))

const _AppearanceSet: Schema.Schema<_SetDefined, _SetWire> = Format.proto.message(appearanceSet.SetSchema).pipe(
  Schema.filter(_setDefined, { identifier: "SetDefined" }),
  Schema.filter((set) => Option.fromNullable(_surfaceOf(set)).pipe(
    Option.match({ onNone: () => true, onSome: (surface) => Array.every(surface.planes, _planeLawful) })) || "<plane-law>",
  { identifier: "PlaneLawful" }),
  Schema.filter((set) => Option.fromNullable(_surfaceOf(set)).pipe(
    Option.match({ onNone: () => true, onSome: (surface) => _rosterOrdered(surface.planes) })) || "<plane-roster-order>",
  { identifier: "RosterOrdered" }),
)

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

class WkbParser extends Context.Tag("@rasm/core/WkbParser")<WkbParser, {
  readonly parse: (wkb: Uint8Array, srid: number) => Effect.Effect<GeoFeature.Geometry, WireFault>
}>() {}

// Every identity on the element plane crosses as sixteen big-endian bytes. A content address lands as the branch's
// lowercase content-key face through the digest owner's own byte codec; a node identity lands as the uppercase X32
// spelling the C# producer mints its ids in — so byte order and hex case both settle at this ONE decode, every
// consumer joins strings, and an encode re-mints the producer's bytes from the same two codecs.
const _ContentAddress = Digest.codecs.content.bytes
const _NodeId: Schema.Schema<string & Brand.Brand<"NodeId">, Uint8Array> = Schema.transformOrFail(
  Schema.Uint8ArrayFromSelf,
  Schema.String.pipe(Schema.pattern(/^[0-9A-F]{32}$/), Schema.brand("NodeId")),
  {
    strict: true,
    decode: (octets) => ParseResult.succeed(Encoding.encodeHex(octets).toUpperCase()),
    encode: (hex, _options, ast) =>
      Either.mapLeft(Encoding.decodeHex(hex), () => new ParseResult.Type(ast, hex, "<node-id-hex>")),
  },
)

// --- [ELEMENT_LIFTS]

// Every element landing is a lawful descriptor-bound pair: an OWNED domain schema whose ENCODED side is the generated
// message, so the domain reads branded ids, `Option` absence, and defined enum members while the wire stays the
// producer's own shape. Five lifts serve every landing and mint no shape of their own. `_member` admits a nested
// generated message by its `$typeName` brand alone — the root's validator already walked it, so a second rule pass
// here would charge every nested message twice. `_enum` narrows a generated enum to its defined members, which is
// the TYPE of what `defined_only`/`not_in: [0]` already proved. `_list` keeps the producer's mutable array, because
// the generated init takes `Array<T>` and a readonly list is unspellable against it. `_option` lifts the producer's
// omission posture onto the branch carrier. `_oneof` lifts an UNSET oneof face to `Option.none()` and a set one to
// its typed arm, so no landing reads a `case: undefined` member past this seam. `_bound` is the pair itself: the
// validated message in, the owned schema above it, and `create` re-minting the message on egress so every landing
// round-trips through the one constructor.
const _member = <Desc extends DescMessage>(gen: Desc): Schema.Schema<MessageValidType<Desc>, MessageShape<Desc>> =>
  Schema.transform(
    Schema.declare((input: unknown): input is MessageShape<Desc> => isMessage(input, gen), { identifier: gen.typeName }),
    Schema.declare((input: unknown): input is MessageValidType<Desc> => isMessage(input, gen), {
      identifier: `${gen.typeName}Valid`,
    }),
    { strict: true, decode: (message) => message, encode: (message) => message },
  )
const _enum = <E extends { readonly UNSPECIFIED: 0 }>(members: E): Schema.Schema<_Defined<E>> =>
  Schema.Literal(..._defined(members))
const _list = <A, I>(item: Schema.Schema<A, I>) => Schema.mutable(Schema.Array(item))
const _option = <A, I>(value: Schema.Schema<A, I>) => Schema.optionalWith(value, { as: "Option" })
const _unset = Schema.Struct({ case: Schema.Undefined, value: Schema.optional(Schema.Undefined) })
const _oneof = <A extends { readonly case: string }, I>(
  arms: Schema.Schema<A, I>,
): Schema.Schema<Option.Option<A>, I | typeof _unset.Encoded> =>
  Schema.transform(Schema.Union(arms, _unset), Schema.OptionFromSelf(Schema.typeSchema(arms)), {
    strict: true,
    decode: (face) => (face.case === undefined ? Option.none() : Option.some(face)),
    encode: (held) => Option.getOrElse(held, () => ({ case: undefined })),
  })
const _bound = <Desc extends DescMessage, A>(
  gen: Desc,
  owned: Schema.Schema<A, MessageInitShape<Desc>>,
): Schema.Schema<A, MessageShape<Desc>> =>
  Schema.transform(Format.proto.message(gen), owned, {
    strict: true,
    decode: (wire) => wire,
    encode: (init) => Format.proto.create(gen, init),
  })

// --- [ELEMENT_VALUE]

// Uncertainty bands ride `MeasureValueWire.uncertainty` — the interval in SI beside the producer's own kind member,
// with the standard deviation and coverage factor a stated statistical band carries and a bare interval does not.
class MeasureBand extends Schema.Class<MeasureBand>("MeasureBand")({
  kind: _enum(property.UncertaintyKind),
  lowerSi: Schema.Number,
  upperSi: Schema.Number,
  standardDeviationSi: _option(Schema.Number),
  coverageFactor: _option(Schema.Number),
}) {}

// SI-coerced identity columns are what the producer hashes: the dimension signature — quantity token beside the seven
// base exponents — and the SI magnitude. The registry unit re-mints at the producer's own SI admission, so no
// `{ value, unit }` pair crosses and no column here carries one; the signature lands as the generated member.
class MeasureValue extends Schema.Class<MeasureValue>("MeasureValue")({
  dimension: _member(property.DimensionWireSchema),
  si: Schema.Number,
  uncertainty: _option(MeasureBand),
}) {}

// `PropertyValueWire` is the producer's recursive FOURTEEN-arm typed-value fold — `csharp:Rasm.Element/Graph/wirevalue`
// owns the roster and `GenericWire.attributes` carries it as named rows. It lands as the generated `{ case, value }`
// oneof face verbatim: five arms carry a SCALAR, so the oneof-hoisting lift has no record to spread and the case
// name IS the discriminant space. Ten arms bottom out and four recurse through the same carrier, so the nesting half
// spells once over its child type exactly as the shell intent family does.
const _attrCases = [
  "text", "measure", "boolean", "logical", "reference", "bounded", "temporal", "integer", "number", "binary",
  "enumerated", "list", "table", "complex",
] as const

// Producer `LogicalWire` carries an OPTIONAL bool, so IFC's third LOGICAL state crosses as an absent column
// and never as a third literal this end would have to invent. `integer` crosses as big-endian two's-complement
// octets because the producer's arm is a `BigInteger` no JSON number holds, and `binary` is the opaque octet arm
// beside it — two arms with one wire shape and two meanings, which is why the case name and not the shape routes.
const _Logical = Schema.Struct({ value: _option(Schema.Boolean) })
// Temporal arms are the producer's own five-leaf sub-oneof over the well-known and `google.type` messages: the
// calendar date, the civil date-time, and the time of day land as their generated members, the span as the branch
// `Duration`, and the epoch stamp as the branch instant — each through the bridge its well-known type ships.
const _AttrTemporal = Schema.Union(
  Schema.Struct({ case: Schema.Literal("date"), value: _member(DateSchema) }),
  Schema.Struct({ case: Schema.Literal("moment"), value: _member(DateTimeSchema) }),
  Schema.Struct({ case: Schema.Literal("time"), value: _member(TimeOfDaySchema) }),
  Schema.Struct({ case: Schema.Literal("span"), value: _Span }),
  Schema.Struct({ case: Schema.Literal("stamp"), value: _Instant }),
)
const _attrLeaves = Schema.Union(
  Schema.Struct({ case: Schema.Literal("text"), value: Schema.String }),
  Schema.Struct({ case: Schema.Literal("measure"), value: MeasureValue }),
  Schema.Struct({ case: Schema.Literal("boolean"), value: Schema.Boolean }),
  Schema.Struct({ case: Schema.Literal("logical"), value: _Logical }),
  Schema.Struct({
    case: Schema.Literal("reference"),
    value: Schema.Struct({ target: _NodeId, usageName: _option(Schema.String) }),
  }),
  Schema.Struct({
    case: Schema.Literal("bounded"),
    value: Schema.Struct({
      lower: _option(MeasureValue),
      upper: _option(MeasureValue),
      setPoint: _option(MeasureValue),
    }),
  }),
  Schema.Struct({ case: Schema.Literal("temporal"), value: _AttrTemporal }),
  Schema.Struct({ case: Schema.Literal("integer"), value: Schema.Uint8ArrayFromSelf }),
  Schema.Struct({ case: Schema.Literal("number"), value: Schema.Number }),
  Schema.Struct({ case: Schema.Literal("binary"), value: Schema.Uint8ArrayFromSelf }),
)

// Named rows cross as the producer's `repeated NamedValueWire` and land KEYED, because every reader joins on the
// name; the two shapes are one transform, so the interior never re-spells the row and egress re-mints it.
type _AttrNest<T> =
  | { readonly case: "enumerated"; readonly value: { readonly selected: Array<T>; readonly allowed: Array<T> } }
  | { readonly case: "list"; readonly value: { readonly values: Array<T> } }
  | {
    readonly case: "table"
    readonly value: {
      readonly rows: Array<{ readonly defining: T; readonly defined: T }>
      readonly interpolation: _Defined<typeof property.Interpolation>
    }
  }
  | { readonly case: "complex"; readonly value: { readonly usageName: string; readonly properties: { readonly [key: string]: T } } }
type _AttrNestWire<T> =
  | { readonly case: "enumerated"; readonly value: { readonly selected: Array<T>; readonly allowed: Array<T> } }
  | { readonly case: "list"; readonly value: { readonly values: Array<T> } }
  | {
    readonly case: "table"
    readonly value: {
      readonly rows: Array<{ readonly defining: T; readonly defined: T }>
      readonly interpolation: _Defined<typeof property.Interpolation>
    }
  }
  | {
    readonly case: "complex"
    readonly value: { readonly usageName: string; readonly properties: Array<{ readonly name: string; readonly value: T }> }
  }

type _AttrValue = typeof _attrLeaves.Type | _AttrNest<_AttrValue>
type _AttrValueWire = typeof _attrLeaves.Encoded | _AttrNestWire<_AttrValueWire>

const _attr: Schema.Schema<_AttrValue, _AttrValueWire> = Schema.suspend(() => _AttrValue)
const _AttrRow = Schema.Struct({ defining: _attr, defined: _attr })
const _NamedRow = Schema.Struct({ name: Schema.String, value: _attr })
const _named: Schema.Schema<{ readonly [key: string]: _AttrValue }, Array<typeof _NamedRow.Encoded>> = Schema.transform(
  _list(_NamedRow),
  Schema.Record({ key: Schema.String, value: Schema.typeSchema(_attr) }),
  {
    strict: true,
    decode: (rows) => Record.fromEntries(Array.map(rows, (row) => [row.name, row.value] as const)),
    encode: (record) => Array.map(Record.toEntries(record), ([name, value]) => ({ name, value })),
  },
)

const _AttrValue: Schema.Schema<_AttrValue, _AttrValueWire> = Schema.Union(
  _attrLeaves,
  Schema.Struct({
    case: Schema.Literal("enumerated"),
    value: Schema.Struct({ selected: _list(_attr), allowed: _list(_attr) }),
  }),
  Schema.Struct({ case: Schema.Literal("list"), value: Schema.Struct({ values: _list(_attr) }) }),
  Schema.Struct({
    case: Schema.Literal("table"),
    value: Schema.Struct({ rows: _list(_AttrRow), interpolation: _enum(property.Interpolation) }),
  }),
  Schema.Struct({
    case: Schema.Literal("complex"),
    value: Schema.Struct({ usageName: Schema.String, properties: _named }),
  }),
)

// Two-way close over the producer's own roster: a fifteenth arm on the union fails the first alias, and a roster
// token no arm lands fails the second, so neither half can drift off the other and a renamed producer case breaks
// every dispatching consumer at compile time instead of reaching `ui` as an untyped record.
type _AttrArm = _AttrValue["case"]
type _AttrWidened<K extends _AttrArm = (typeof _attrCases)[number]> = K
type _AttrClosed<K extends (typeof _attrCases)[number] = _AttrArm> = K

// `NodeWire` crosses its id as sixteen bytes and its payload as the eight-arm oneof — object, material, property set,
// quantity set, assessment, appearance, coverage, observation. The payload lands as the generated face WHOLE and
// typed: each arm is presence on this owner that the census declares no family for, so a consumer reads the
// generated member the case names and no landing arm per case is minted here. `kind` is the face's own case, read
// off the value rather than a column the `.proto` never declared; the unset member is excluded at the type because
// the corpus rule already refused it.
type _NodePayload = Exclude<MessageValidType<typeof graph.NodeWireSchema>["payload"], { readonly case: undefined }>
const _payload: Schema.Schema<_NodePayload> = Schema.declare(
  (input: unknown): input is _NodePayload => Predicate.isRecord(input) && Predicate.isString(input.case),
  { identifier: "NodePayload" },
)

class Node extends Schema.Class<Node>("Node")({
  id: _NodeId,
  contentAddress: _ContentAddress,
  payload: _payload,
}) {
  get kind(): _NodePayload["case"] {
    return this.payload.case
  }
  static readonly FromWire: Schema.Schema<Node, MessageShape<typeof graph.NodeWireSchema>> = _bound(graph.NodeWireSchema, Node)
  // ProtoJSON for the entity-edit patch: the one JSON codec under the one read posture, the branch's JSON tree as
  // its encoded side, composed onto the same bound pair every other crossing of this node takes
  static readonly Json: Schema.Schema<Node, Shape.Json> = Schema.compose(
    Schema.compose(Shape.Json, Format.proto.json(graph.NodeWireSchema), { strict: false }),
    Node.FromWire,
  )
}

// Entity edits cross as the generated `EntityEditWire` — a two-arm oneof whose members arm carries the corpus's own
// `PatchOp` rows — and land as the branch's closed RFC 6902 document, so the prototype-safe pointer law `Format.Patch`
// owns is the one gate every patch passes and every `Value` crosses `Shape.Json` through the generated codec. The
// transform is total both ways: a decoded edit re-encodes to the same message, so the data plane's egress of an
// authored edit rides this one owner.
const _patchOps = ["add", "remove", "replace", "move", "copy", "test"] as const
const _op = (op: MessageInitShape<typeof control.PatchOpSchema>["op"]): MessageShape<typeof control.PatchOpSchema> =>
  Format.proto.create(control.PatchOpSchema, { op })
const _PatchOp: Schema.Schema<Format.Patch.Operation, MessageShape<typeof control.PatchOpSchema>> = Schema.transformOrFail(
  Format.proto.message(control.PatchOpSchema),
  Format.Patch.Operation,
  {
    strict: true,
    decode: (op, _options, ast) =>
      Either.flatMap(
        Match.value(op.op).pipe(
          Match.when({ case: "add" }, ({ value }) =>
            Either.map(_json(value.value), (json) => ({ op: "add" as const, path: value.path, value: json }))),
          Match.when({ case: "remove" }, ({ value }) => Either.right({ op: "remove" as const, path: value.path })),
          Match.when({ case: "replace" }, ({ value }) =>
            Either.map(_json(value.value), (json) => ({ op: "replace" as const, path: value.path, value: json }))),
          Match.when({ case: "move" }, ({ value }) => Either.right({ op: "move" as const, from: value.fromPath, path: value.path })),
          Match.when({ case: "copy" }, ({ value }) => Either.right({ op: "copy" as const, from: value.fromPath, path: value.path })),
          Match.when({ case: "test" }, ({ value }) =>
            Either.map(_json(value.value), (json) => ({ op: "test" as const, path: value.path, value: json }))),
          Match.orElse(() => Either.left(new ParseResult.Type(ast, op, "<patch-op-unset>"))),
        ),
        // the pointer refinements run HERE, so a prototype token refuses at the wire and never reaches the apply fold
        (candidate) => Either.mapLeft(Schema.decodeUnknownEither(Format.Patch.Operation)(candidate), (error) => error.issue),
      ),
    encode: (operation, _options, ast) =>
      Match.value(operation).pipe(
        Match.discriminatorsExhaustive("op")({
          add: ({ path, value }) =>
            Either.map(_value(value, ast), (held) =>
              _op({ case: "add", value: Format.proto.create(control.PatchAddSchema, { path, value: held }) })),
          remove: ({ path }) => Either.right(_op({ case: "remove", value: Format.proto.create(control.PatchRemoveSchema, { path }) })),
          replace: ({ path, value }) =>
            Either.map(_value(value, ast), (held) =>
              _op({ case: "replace", value: Format.proto.create(control.PatchReplaceSchema, { path, value: held }) })),
          move: ({ from, path }) =>
            Either.right(_op({ case: "move", value: Format.proto.create(control.PatchMoveSchema, { fromPath: from, path }) })),
          copy: ({ from, path }) =>
            Either.right(_op({ case: "copy", value: Format.proto.create(control.PatchCopySchema, { fromPath: from, path }) })),
          test: ({ path, value }) =>
            Either.map(_value(value, ast), (held) =>
              _op({ case: "test", value: Format.proto.create(control.PatchTestSchema, { path, value: held }) })),
        }),
      ),
  },
)
type _PatchOpClosed<K extends (typeof _patchOps)[number] = Format.Patch.Operation["op"]> = K
type _PatchOpWhole<K extends Format.Patch.Operation["op"] = (typeof _patchOps)[number]> = K

// `Value` is REQUIRED on the three value-bearing arms by corpus rule and `valid_types=protovalidate_required`
// carries that proof into every decoded arm, so no nullable peer or fabricated null document exists here.
const _json = (value: MessageValidType<typeof ValueSchema>): Either.Either<Shape.Json, ParseResult.ParseIssue> =>
  Either.mapLeft(Schema.encodeEither(Format.proto.value)(value), (error) => error.issue)
const _value = (json: Shape.Json, ast: SchemaAST.AST): Either.Either<MessageShape<typeof ValueSchema>, ParseResult.ParseIssue> =>
  Either.mapLeft(Schema.decodeEither(Format.proto.value)(json), (error) => error.issue).pipe(
    Either.mapLeft((issue) => new ParseResult.Type(ast, json, `<patch-value>${issue._tag}`)),
  )

const _edits = Schema.Union(
  Schema.Struct({ kind: Schema.Literal("tombstone"), key: _NodeId, base: _ContentAddress }),
  Schema.Struct({ kind: Schema.Literal("members"), key: _NodeId, base: _ContentAddress, patch: Format.Patch.Document }),
)

const EntityEdit: Schema.Schema<typeof _edits.Type, MessageShape<typeof evidence.EntityEditWireSchema>> = Schema.transformOrFail(
  Format.proto.message(evidence.EntityEditWireSchema),
  Schema.typeSchema(_edits),
  {
    strict: true,
    decode: (wire, _options, ast) =>
      Match.value(wire.edit).pipe(
        Match.when({ case: "tombstone" }, ({ value }) =>
          Either.map(
            Either.all({ key: Schema.decodeEither(_NodeId)(value.key), base: Schema.decodeEither(_ContentAddress)(value.base) }),
            ({ key, base }) => ({ kind: "tombstone" as const, key, base }),
          )),
        Match.when({ case: "members" }, ({ value }) =>
          Either.map(
            Either.all({
              key: Schema.decodeEither(_NodeId)(value.key),
              base: Schema.decodeEither(_ContentAddress)(value.base),
              patch: Schema.decodeEither(Schema.Array(_PatchOp))(value.patch),
            }),
            ({ key, base, patch }) => ({ kind: "members" as const, key, base, patch }),
          )),
        Match.orElse(() => Either.left(new ParseResult.Type(ast, wire, "<edit-unset>"))),
      ).pipe(Either.mapLeft((issue) => issue instanceof ParseResult.ParseError ? issue.issue : issue)),
    encode: (edit) =>
      Either.map(
        Either.all({
          key: Schema.encodeEither(_NodeId)(edit.key),
          base: Schema.encodeEither(_ContentAddress)(edit.base),
          patch: edit.kind === "members" ? Schema.encodeEither(Schema.Array(_PatchOp))(edit.patch) : Either.right([]),
        }),
        ({ key, base, patch }) =>
          Format.proto.create(evidence.EntityEditWireSchema, {
            edit: edit.kind === "tombstone"
              ? { case: "tombstone", value: Format.proto.create(evidence.EditTombstoneSchema, { key, base }) }
              : { case: "members", value: Format.proto.create(evidence.EditMembersSchema, { key, base, patch: [...patch] }) },
          }),
      ).pipe(Either.mapLeft((error) => error.issue)),
  },
)

```

## [07]-[KEYED_REGISTRY]

- Owner: one closed row table derives `Wire.schema`, `decode`, `encode`, `audited`, and complete-frame `stream`.
- Owner: `Wire.Walk` holds one child projection per recursive landed tree and the one bounded pre-order fold over them.
- Law: every recursive landed family carries a walk row, so no consumer hand-recurses a foreign tree and none goes unbounded.
- Law: walk budgets are unit-carrying — depth spends `Shape.Bound<"hops">` and fan the ingress collection ceiling.
- Law: a walk projection is total against its family's own closed split, so a new arm breaks the table rather than reading as a leaf.
- Law: structured refusals remain typed fault evidence — divergent advertised and generated contract identities raise `drift` with package, service family, and admitted generation, never a descriptor walk.
- Law: `frame` parity gates every decode; Merkle summary parity calls `Commit.admit` before a decoded value returns.
- Law: `suite` parity grades the schema, so `audited` spends it and the decode path never charges it per payload.
- Law: the stream grades extent before decode, so a budget refusal carries `overrun` rather than a parse verdict.

```typescript signature
import { Stream } from "effect"

// The complete producer record, never the former short `[seq, op]` fiction. The seventh position (`Payload`, key 6)
// remains opaque bytes; only the explicit CRDT selector opens it as generated protobuf.
const _vector = Schema.Array(Schema.Tuple(_fixed16, _i63)).pipe(
  Schema.filter((rows) => _strictRows(rows, (left, right) => _byteOrder(left[0], right[0])), {
    message: () => "<oplog-context-order>",
  }),
)
const _dot = Schema.Tuple(_fixed16, _i63, _vector).pipe(
  Schema.filter(([origin, counter, context]) => {
    const observed = context.find(([held]) => _byteOrder(held, origin) === 0)?.[1] ?? 0n
    return counter > 0n && counter === observed + 1n && origin.some((byte) => byte !== 0)
  }, { message: () => "<oplog-dot-gap>" }),
)
const _trace = Schema.Tuple(_traceId, _octets)
const _oplogFamily = Schema.Literal("scalar", "crdt", "geometry", "presence", "commit", "branch", "attest")
const _oplogKind = Schema.Literal("upsert", "delete", "truncate", "presence")
const _closure = Schema.Array(_fixed16).pipe(
  Schema.filter((rows) => _strictRows(rows, _byteOrder), { message: () => "<oplog-closure-order>" }),
)
const OpLogEntry = _keyed("OpLogEntry", [
  ["seq", _i63],
  ["id", _dot],
  ["model", _fixed16],
  ["entity", _text],
  ["family", _oplogFamily],
  ["kind", _oplogKind],
  ["payload", _octets],
  ["contentKey", _fixed16],
  ["trace", _trace],
  ["closure", _closure],
  ["actor", _text],
  ["physicalTicks", _i63],
  ["logical", _i63],
] as const).pipe(
  Schema.filter((entry) => !entry.closure.some((key) => _byteOrder(key, entry.contentKey) === 0), {
    message: () => "<oplog-closure-root>",
  }),
)
type OpLogEntry = typeof OpLogEntry.Type

// Proto families name their FRAMING on the row: the element, artifact, appearance, and fault families
// cross as proto binary, and every host family crosses as the ProtoJSON document the C# host's STJ pipeline now emits
// through the generated message. The landed value is one shape under both framings, so a consumer never reads which
// carried it. Families landing the generated shape whole bind `Format.proto.frame`; families whose consumers need a
// domain owner above the wire bind `family` with that owner.
const _schema = {
  // The manifest `hlc-stamp/cell` case fixes a two-64-bit-half cell, so this stamp rides the extension byte the msgpack engine already
  // registers for `Clock.Hlc` — a descriptor displaces that frozen layout with tag bytes no peer minter emits.
  HlcStampWire: Format.msgpack.schema(Clock.Hlc),
  CommandAvailability: Format.proto.frame(Format.proto.suite.CommandAvailability, "json"),
  FaultDetail: Format.proto.family(Format.proto.suite.FaultDetail, Remote.FromWire),
  OpLogWire: Format.msgpack.schema(OpLogEntry),
  CrdtOpWire: CrdtOp,
  CommitWire: Format.msgpack.schema(Commit),
  BranchWire: Format.msgpack.schema(Commit.Branch),
  VersionVectorWire: Format.msgpack.schema(Causal.Vector),
  MerkleSummaryWire: Format.msgpack.schema(Commit.Merkle),
  EntityEditWire: Format.proto.family(Format.proto.suite.EntityEditWire, EntityEdit, "json"),
  CredentialPublicWire: Format.proto.frame(Format.proto.suite.CredentialPublicWire, "json"),
  DescriptorPinWire: Format.proto.frame(Format.proto.suite.DescriptorPinWire, "json"),
  BenchmarkClaimWire: Format.proto.family(Format.proto.suite.BenchmarkClaimWire, Board.Claim.FromWire, "json"),
  BindingStatus: Format.proto.frame(Format.proto.suite.BindingStatus, "json"),
  CoercedValueWire: Format.proto.frame(Format.proto.suite.CoercedValueWire, "json"),
  WriteReceiptWire: Format.proto.frame(Format.proto.suite.WriteReceiptWire, "json"),
  FlagVerdictWire: Format.proto.frame(Format.proto.suite.FlagVerdictWire, "json"),
  AppUiSurfaceProgram: Format.proto.frame(Format.proto.suite.AppUiSurfaceProgram, "json"),
  CommandGateWire: Format.proto.frame(Format.proto.suite.CommandGateWire, "json"),
  EvidenceTimelineWire: Format.proto.frame(Format.proto.suite.EvidenceTimelineWire, "json"),
  BcfTopicWire: Format.proto.frame(Format.proto.suite.BcfTopicWire, "json"),
  BcfViewpointWire: Format.proto.frame(Format.proto.suite.BcfViewpointWire, "json"),
  ModelDiffWire: Format.proto.frame(Format.proto.suite.ModelDiffWire, "json"),
  Material: Format.proto.frame(Format.proto.suite.Material),
  Set: Format.proto.family(Format.proto.suite.Set, _AppearanceSet),
} as const

// Parity obligations split on WHEN they are owed, because two unlike checks were riding one column. A `frame` row
// grades THIS payload — a merkle root re-derives from the summary's own rows and disagrees per arrival — so the
// decode path owes it before a value returns. A `suite` row grades the SCHEMA: `_semantic` re-encodes a decoded
// value and compares it against itself, never against the arriving octets, so its verdict is constant across every
// input. Running it per frame charged an encode, a decode, and a deep structural compare on every ingress frame —
// on `Set`, the branch's largest payload — to re-derive a fact one conformance run settles
// per family, and encoded families whose own row declares `decode`.
const _whens = ["frame", "suite"] as const

const _semantic = <A, I>(family: Wire.Family, schema: Schema.Schema<A, I>): Wire.Parity<A>["run"] => (value) =>
  Effect.flatMap(Schema.encode(schema)(value), (encoded) =>
    Effect.flatMap(Schema.decodeUnknown(schema)(encoded), (decoded) =>
      Schema.equivalence(schema)(value, decoded)
        ? Effect.void
        : Effect.fail(_mismatch(family, "semantic", value, decoded))))

const _row = <A, I, const D extends Wire.Direction, const F extends Wire.Arm>(
  direction: D,
  arm: F,
  schema: Schema.Schema<A, I>,
  parity: Option.Option<Wire.Parity<A>> = Option.none(),
): Wire.Row<A, I, D, F> => ({ direction, arm, schema, parity })

// Every descriptor-bound family rows through ONE fold: the arm is `proto` whatever the framing, because framing is
// a column of the family's schema and the arm names the ENGINE a quarantined frame renders through.
const _proto = <K extends Extract<keyof typeof _schema, (typeof Format.proto.names)[number]>, const D extends Wire.Direction = "decode">(
  family: K,
  direction: D = "decode" as D,
): Wire.Row<Schema.Schema.Type<(typeof _schema)[K]>, Uint8Array, D, "proto"> =>
  _row(
    direction,
    "proto",
    _schema[family],
    Option.some({ when: "suite", run: _semantic(family, _schema[family]) }),
  )

const _merkleParity: Wire.Parity<Commit.Merkle> = {
  when: "frame",
  run: (summary) =>
    Commit.admit(summary).pipe(
      Effect.asVoid,
      // summary owners already measured the divergence, so their refusal RIDES here; discarding it and raising a
      // bare token made the one fault an operator drills on the one fault carrying no coordinate to drill
      Effect.mapError((refusal) => _mismatch("MerkleSummaryWire", "merkle-root", refusal.actual, refusal.expected)),
    ),
}

const _oplogParity: Wire.Parity<OpLogEntry> = {
  when: "frame",
  run: (entry) => Effect.asVoid(_admittedEntry(entry)),
}

const _rows = {
  HlcStampWire: _row("duplex", "msgpack", _schema.HlcStampWire),
  CommandAvailability: _proto("CommandAvailability"),
  FaultDetail: _proto("FaultDetail"),
  OpLogWire: _row("decode", "msgpack", _schema.OpLogWire, Option.some(_oplogParity)),
  CrdtOpWire: _row("duplex", "proto", _schema.CrdtOpWire),
  CommitWire: _row("duplex", "msgpack", _schema.CommitWire),
  BranchWire: _row("duplex", "msgpack", _schema.BranchWire),
  VersionVectorWire: _row("duplex", "msgpack", _schema.VersionVectorWire),
  MerkleSummaryWire: _row("duplex", "msgpack", _schema.MerkleSummaryWire, Option.some(_merkleParity)),
  EntityEditWire: _proto("EntityEditWire", "duplex"),
  CredentialPublicWire: _proto("CredentialPublicWire"),
  DescriptorPinWire: _proto("DescriptorPinWire"),
  BenchmarkClaimWire: _proto("BenchmarkClaimWire"),
  BindingStatus: _proto("BindingStatus"),
  CoercedValueWire: _proto("CoercedValueWire"),
  WriteReceiptWire: _proto("WriteReceiptWire"),
  FlagVerdictWire: _proto("FlagVerdictWire"),
  AppUiSurfaceProgram: _proto("AppUiSurfaceProgram"),
  CommandGateWire: _proto("CommandGateWire"),
  EvidenceTimelineWire: _proto("EvidenceTimelineWire"),
  BcfTopicWire: _proto("BcfTopicWire"),
  BcfViewpointWire: _proto("BcfViewpointWire"),
  ModelDiffWire: _proto("ModelDiffWire"),
  Material: _proto("Material"),
  Set: _proto("Set"),
} as const

const _family = Shape.vocabulary(_families, _rows)

// ONE bounded traversal owner over every recursive foreign tree this page lands. Four families recurse — the shell
// menu strip, the control-intent tree, GeoJSON geometry collections, and the IFC
// typed-value fold — and each reached its consumers with no walk, no depth bound, and a hand recursion per reader,
// so a hostile document was bounded by whichever consumer happened to guard. The roster is ONE child projection per
// family, seated at the registry rather than inside any one landing because it serves them all: a sixth recursive
// family lands as one row and inherits the budget, where today it would inherit nothing.
//
// Projections are TOTAL by construction rather than by a catch-all. Protovalidate closes the required oneof at
// admission, while the current generated valid face still represents its unset arm; that impossible admitted arm
// bottoms out explicitly. A new arm on the corpus breaks the exhaustive switch at its declaration, where a
// `Match.orElse` would turn that compile break into a silent zero-child answer, which reads as a leaf and truncates
// the walk. Every nesting arm reads its own generated valid child column rather than a family-wide convention the
// producer never declared.
const _walkRows = {
  ControlIntentWire: {
    children: (node: ControlIntentWireValid): ReadonlyArray<ControlIntentWireValid> => {
      switch (node.arm.case) {
        case "banner": return Array.appendAll(node.arm.value.actions, Array.fromNullable(node.arm.value.evidence))
        case "emptyState": return Array.fromNullable(node.arm.value.action)
        case "grid": return Array.flatMap(node.arm.value.columns, (column) =>
          Array.appendAll([column.cell], Array.fromNullable(column.editor)))
        case "tree": return [node.arm.value.item]
        case "toolbar": return Array.map(node.arm.value.rows, (row) => row.item)
        case "tab": return Array.map(node.arm.value.pages, (page) => page.body)
        case "accordion": return Array.map(node.arm.value.sections, (section) => section.body)
        case "panel": return node.arm.value.children
        case "dock": return node.arm.value.regions
        case "splitter": return [node.arm.value.first, node.arm.value.second]
        case "button":
        case "label":
        case "textInput":
        case "numberInput":
        case "dateInput":
        case "pathInput":
        case "colorInput":
        case "select":
        case "multiSelect":
        case "slider":
        case "range":
        case "toggle":
        case "radio":
        case "segmented":
        case "chip":
        case "progress":
        case "avatar":
        case "breadcrumb":
        case "tooltip":
        case "overview":
        case "menu":
        case undefined:
          return Array.empty()
      }
      const exhaustive: never = node.arm
      return exhaustive
    },
  },
  // Menu rows recurse on THEMSELVES rather than on a child intent, so the strip is its own family: a walk that
  // followed the intent projection into a menu would stop at the arm and never see the rows under it.
  MenuRowWire: {
    children: (node: MenuRowWireValid): ReadonlyArray<MenuRowWireValid> => node.rows,
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
type _WireFaultCase = WireFault.Case
type _WireFaultReason = WireFault.Reason
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
type _TextureContainer = Texture.Container
type _TextureDepth = Texture.Depth
type _TextureLayerLaw = Texture.LayerLaw
type _TextureMipPolicy = Texture.MipPolicy
type _TexturePack = Texture.Pack
type _TexturePayload = Texture.Payload
type _TexturePlaneFormat = Texture.PlaneFormat
type _TexturePrimaries = Texture.Primaries
type _TextureReference = _PlaneRefLanded
type _TextureRole = Texture.Role
type _TextureTransfer = Texture.Transfer
type _TextureWirePayload = Texture.WirePayload

declare namespace Wire {
  type Direction = "decode" | "encode" | "duplex"
  // Arm vocabulary belongs to the plane owning the engines; spelling it a second time here let a row name an
  // encoding `Format` never published.
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
    readonly parity: Option.Option<Parity<A>>
  }
  type Ingress = { readonly [K in Family]: (typeof _rows)[K]["direction"] extends "encode" ? never : K }[Family]
  type Egress = { readonly [K in Family]: (typeof _rows)[K]["direction"] extends "decode" ? never : K }[Family]
  type Fault = WireFault
  namespace Artifact {
    type Identity = ArtifactId.Identity
    type Reference = _ArtifactRefLanded
    type Frame = _ArtifactFrameLanded
  }
  namespace Fault {
    // Refusal payloads publish so a peer assembler raises through the roster rather than re-spelling a cause's
    // columns, and the budget axis publishes because `frame` refuses on ceilings this page never sees — a raiser
    // there names a member of THIS roster or breaks, which is what keeps nine ceilings under one vocabulary.
    type Case = _WireFaultCase
    type Reason = _WireFaultReason
  }
  type OverrunAxis = _OverrunAxis
  type FaultDetail = Decoded<"FaultDetail">
  type InvokeFault = _InvokeFault
  type InvokeReason = _InvokeReason
  type RemoteDetail = _RemoteDetail
  type TransportKind = _TransportKind
  // Host families land as the GENERATED shape, validated: every optional column reads `undefined` in the producer's
  // own omission posture, every enum reads its generated member, and a consumer lifts at its own seam.
  type CommandAvailability = Decoded<"CommandAvailability">
  type Credential = Decoded<"CredentialPublicWire">
  type DescriptorPin = Decoded<"DescriptorPinWire">
  type BenchmarkClaim = Decoded<"BenchmarkClaimWire">
  type BindingStatus = Decoded<"BindingStatus">
  type CoercedValue = Decoded<"CoercedValueWire">
  type WriteReceipt = Decoded<"WriteReceiptWire">
  type FlagVerdict = Decoded<"FlagVerdictWire">
  type AppUiSurface = Decoded<"AppUiSurfaceProgram">
  type ControlIntent = AppUiSurface["root"]
  type LayoutProgram = AppUiSurface["layouts"][number]
  type CommandGate = Decoded<"CommandGateWire">
  type EvidenceTimeline = Decoded<"EvidenceTimelineWire">
  type BcfTopic = Decoded<"BcfTopicWire">
  type BcfViewpoint = Decoded<"BcfViewpointWire">
  type ModelDiff = Decoded<"ModelDiffWire">
  type Material = Decoded<"Material">
  type Set = Decoded<"Set">
  type EntityEdit = Decoded<"EntityEditWire">
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
    type Container = _TextureContainer
    type Depth = _TextureDepth
    type LayerLaw = _TextureLayerLaw
    type MipPolicy = _TextureMipPolicy
    type Pack = _TexturePack
    type Payload = _TexturePayload
    type PlaneFormat = _TexturePlaneFormat
    type Primaries = _TexturePrimaries
    type Reference = _TextureReference
    type Role = _TextureRole
    type Transfer = _TextureTransfer
    type WirePayload = _TextureWirePayload
  }
  type Shape = {
    readonly families: typeof _families
    readonly wire: Schema.Literal<readonly [Family, ...Family[]]>
    readonly is: (input: unknown) => input is Family
    readonly schema: <K extends Family>(family: K) => (typeof _rows)[K]["schema"]
    readonly decode: <K extends Ingress>(family: K, octets: Uint8Array) => Effect.Effect<
      Decoded<K>, ParseResult.ParseError | WireFault
    >
    readonly encode: <K extends Egress>(family: K, value: Decoded<K>) => Effect.Effect<Uint8Array, ParseResult.ParseError>
    readonly audited: <K extends Family>(
      family: K,
      value: Decoded<K>,
      octets: Uint8Array,
    ) => Effect.Effect<void, ParseResult.ParseError | WireFault>
    readonly stream: <K extends Ingress>(family: K, frames: AsyncIterable<Uint8Array>) => Stream.Stream<
      Either.Either<Decoded<K>, WireFault>, WireFault, Quarantine
    >
  }
  // Recursive families are their OWN key space: a walk row keys on the tree, not on the census family that
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
      readonly children: <K extends Family>(family: K, node: Node<K>) => ReadonlyArray<Node<K>>
      readonly nodes: <K extends Family>(
        family: K,
        root: Node<K>,
        bound: Shape.Bound<"hops">,
      ) => Effect.Effect<ReadonlyArray<Node<K>>, WireFault>
    }
  }
}

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

const _completeStream = <K extends Wire.Ingress>(
  family: K,
  frames: AsyncIterable<Uint8Array>,
) =>
  Stream.fromAsyncIterable(frames, (defect) =>
    new WireFault({ family, case: { reason: "malformed", at: "source", issue: String(defect) } })).pipe(
    Stream.mapEffect(
      (octets) =>
        (octets.byteLength > Shape.Ingress.floor.bytes
          ? Effect.fail(_overrun(family, octets.byteLength))
          : Wire.decode(family, octets)).pipe(
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
// re-reporting `available`, and a flag re-reporting its verdict are all one reading restated, so the fold drops the
// restatement and the subject keeps its latest. `WriteReceiptWire` and
// `CoercedValueWire` are refused on that same ground and not for lack of a subject: each frame is a distinct
// occurrence carrying its own stamp or its own offered/landed pair, so no two ever compare alike and a row for
// either buys a per-element projection and hash write that can never drop anything.
const _feedKeys = ["FlagVerdictWire", "BindingStatus", "CommandGateWire"] as const

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

// `alike` reads each family's LANDED schema off the registry row — the type side of the bytes schema — so the
// equivalence is derived from the one declaration and no second structural compare is authored beside it.
const _alike = <K extends feed.Family>(family: K) => Schema.equivalence(Schema.typeSchema(_rows[family].schema))
const _feeds: { readonly [K in feed.Family]: feed.Row<Wire.Decoded<K>> } = {
  FlagVerdictWire: {
    subject: (verdict) => verdict.flag,
    alike: _alike("FlagVerdictWire"),
    // Flag verdicts change on a rollout, never on a redraw, so the transition fold already floors the rate and a
    // bucket over it would only delay the one frame a consumer is waiting for.
    band: Option.none(),
  },
  BindingStatus: {
    subject: (status) => status.bindingId,
    alike: _alike("BindingStatus"),
    band: Option.some("control"),
  },
  CommandGateWire: {
    subject: (gate) => gate.key,
    alike: _alike("CommandGateWire"),
    band: Option.some("control"),
  },
}

// Restatements the fold drops RETURN as facts on the same band the survivors ride, so a subject that went
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
  Either.Either<Wire.Decoded<K>, Fault.Drop.Fact>, Wire.Fault, Quarantine
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

// Census values are a FOLD of the fact band and nothing else, through the ledger's own monoid: `empty` is the zero
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
- Entry: `OpLog.stream` admits and sequences native outer envelopes; `OpLog.crdt` selects the CRDT lane and decodes its generated protobuf payload without re-authoring the envelope.

```typescript signature
type CrdtEntry = OpLogEntry & { readonly family: "crdt"; readonly op: CrdtOp }
const _bySeq: Order.Order<OpLogEntry> = Order.mapInput(Order.bigint, (entry: OpLogEntry) => entry.seq)

const _crdtEntry = (entry: OpLogEntry): Effect.Effect<CrdtEntry, WireFault> =>
  Schema.decodeUnknown(CrdtOp)(entry.payload).pipe(
    Effect.map((op) => ({ ...entry, family: "crdt" as const, op })),
    Effect.mapError((issue) => issue instanceof WireFault
      ? issue
      : new WireFault({
        family: "CrdtOpWire",
        case: { reason: "malformed", at: "decode", issue: issue.message },
      })),
  )

const _admittedEntry = (entry: OpLogEntry): Effect.Effect<OpLogEntry, WireFault> =>
  Effect.flatMap(
    Schema.decode(Digest.codecs.content.bytes)(entry.contentKey),
    (expected) => Effect.as(Parity.verified("OpLogWire", expected, entry.payload), entry),
  ).pipe(
    Effect.mapError((issue) => issue instanceof WireFault
      ? issue
      : new WireFault({
        family: "OpLogWire",
        case: { reason: "malformed", at: "decode", issue: issue.message },
      })),
  )

// Refused bands carry two unlike verdicts and keep them unlike: a sequence GAP is evidence the producing peer
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
              // coordinates name the drop, so a replayed tail folds to an extent rather than to silence
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
  readonly stamp: (op: CrdtOp) => Option.Option<Clock.Hlc>
  readonly encode: (entry: OpLogEntry) => Effect.Effect<Uint8Array, ParseResult.ParseError | WireFault>
  readonly stream: (
    frames: ReadableStream<Uint8Array> | AsyncIterable<Uint8Array>,
    resume: bigint,
  ) => Stream.Stream<_Lane<OpLogEntry>, WireFault, Quarantine>
  readonly crdt: <E, R>(
    entries: Stream.Stream<_Lane<OpLogEntry>, E, R>,
  ) => Stream.Stream<_Lane<CrdtEntry>, E | WireFault, R>
  readonly resume: (entries: ReadonlyArray<OpLogEntry>) => Option.Option<bigint>
} = {
  Entry: OpLogEntry,
  stamp: _stamped,
  encode: (entry) => Effect.flatMap(_admittedEntry(entry), Schema.encode(_schema.OpLogWire)),
  stream: (frames, resume) =>
    _completeStream("OpLogWire", frames).pipe(
      Gap.sequential("OpLogWire", resume),
    ),
  crdt: (entries) =>
    entries.pipe(
      Stream.filterMap((lane) => Either.match(lane, {
        onLeft: (fault) => Option.some(Either.left(fault)),
        onRight: (entry) => entry.family === "crdt" ? Option.some(Either.right(entry)) : Option.none(),
      })),
      Stream.mapEffect((lane) => Either.match(lane, {
        onLeft: (fault) => Effect.succeed(Either.left(fault)),
        onRight: (entry) => Effect.map(_crdtEntry(entry), Either.right),
      })),
    ),
  resume: (entries) =>
    Array.isNonEmptyReadonlyArray(entries) ? Option.some(Array.max(entries, _bySeq).seq) : Option.none(),
}

// Walk rosters publish BESIDE the entry, so a consumer choosing a projection reads which trees are walkable rather
// than discovering it by a refused key, and `floor` is the page's own budget a caller either spends or narrows.
const _walk: Wire.Walk.Surface = {
  families: Record.keys(_walkRows),
  floor: _WALK,
  children: (family, node) => (_walkRows[family] as Wire.Walk.Row<typeof node>).children(node),
  nodes: (family, root, bound) => _walked(family, _walkRows[family] as Wire.Walk.Row<typeof root>, root, bound),
}

const Wire = {
  families: _family.kinds,
  wire: _family.schema,
  is: _family.is,
  schema: <K extends Wire.Family>(family: K): (typeof _rows)[K]["schema"] => _rows[family].schema,
  decode: <K extends Wire.Ingress>(family: K, octets: Uint8Array) =>
    Effect.flatMap(Schema.decodeUnknown(_rows[family].schema)(octets), (decoded) =>
      Effect.as(
        Option.match(Option.filter(_rows[family].parity, (row) => row.when === "frame"), {
          onNone: () => Effect.void,
          onSome: (row) => row.run(decoded, octets),
        }),
        decoded,
      )),
  encode: <K extends Wire.Egress>(family: K, value: Wire.Decoded<K>) =>
    Schema.encode(_rows[family].schema)(value),
  // Conformance rails spend EVERY parity row a family carries, `suite` included, so the schema-level proof the
  // decode path no longer charges per frame is still owed — deliberately, once, where a fixture run can afford it.
  audited: <K extends Wire.Family>(family: K, value: Wire.Decoded<K>, octets: Uint8Array) =>
    Option.match(_rows[family].parity, { onNone: () => Effect.void, onSome: (row) => row.run(value, octets) }),
  stream: _completeStream,
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
  invokeReason: _invokeReason,
  EnricherLive: _EnricherLive,
  EntityEdit,
  Artifact,
  Texture,
  GeoFeature,
  WkbParser,
} as const satisfies Wire.Shape & { readonly Walk: Wire.Walk.Surface } & Record<string, unknown>

// --- [EXPORTS] --------------------------------------------------------------------------

export { Wire }
```

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
