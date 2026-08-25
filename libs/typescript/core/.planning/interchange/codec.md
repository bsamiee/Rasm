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

```typescript
import { Array, type ParseResult, Schema, type Types } from "effect"

const _families = [
  "HlcStampWire", "CommandAvailability",
  "FaultDetail",
  "OpLogWire", "CrdtOpWire",
  "CommitWire", "BranchWire", "VersionVectorWire", "MerkleSummaryWire",
  "EntityEditWire", "CredentialPublicWire", "DescriptorPinWire",
  "BenchmarkClaimWire",
  "BindingStatus", "CoercedValueWire", "WriteOutcomeWire",
  "FlagVerdictWire", "AppUiSurfaceProgram", "CommandGateWire", "EvidenceTimelineWire",
  "BcfTopicWire", "BcfViewpointWire", "ModelDiffWire",
  "Material", "Set",
  "BoardPackWire",
] as const

const _wireLiteral = Schema.Literal(..._families)
const _faultFamilies = [
  ..._families,
  "ArtifactAssembly", "GeometryResidency", "IfcWire",
  "CommandInvocation",
] as const
const _faultLiteral = Schema.Literal(..._faultFamilies)

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

```typescript
import { fromJson, isMessage, type MessageInitShape, type MessageShape, type MessageValidType, toJson } from "@bufbuild/protobuf"
import {
  DurationSchema, durationFromMs, durationMs, EmptySchema, timestampFromMs, timestampMs, TimestampSchema, ValueSchema,
} from "@bufbuild/protobuf/wkt"
import { Code } from "@connectrpc/connect"
import { BadRequest_FieldViolationSchema, RetryInfoSchema } from "@rasm/contracts/google/rpc/error_details_pb"
import { DateSchema } from "@rasm/contracts/google/type/date_pb"
import { DateTimeSchema } from "@rasm/contracts/google/type/datetime_pb"
import { TimeOfDaySchema } from "@rasm/contracts/google/type/timeofday_pb"
import * as appearance from "@rasm/contracts/rasm/contracts/appearance/appearance_pb"
import * as appearanceEnvironment from "@rasm/contracts/rasm/contracts/appearance/environment_pb"
import * as appearanceSet from "@rasm/contracts/rasm/contracts/appearance/set_pb"
import * as artifact from "@rasm/contracts/rasm/contracts/artifact/artifact_pb"
import * as control from "@rasm/contracts/rasm/contracts/compute/control_pb"
import { CrdtOpWireSchema } from "@rasm/contracts/rasm/contracts/crdt/crdt_pb"
import * as graph from "@rasm/contracts/rasm/contracts/element/graph_pb"
import * as property from "@rasm/contracts/rasm/contracts/element/value_pb"
import { HlcSchema } from "@rasm/contracts/rasm/contracts/clock/hlc_pb"
import { FaultDetailSchema, FaultRecoverySchema } from "@rasm/contracts/rasm/contracts/fault/fault_pb"
import type { ControlIntentWireValid, MenuRowWireValid } from "@rasm/contracts/rasm/contracts/ui/controls_pb"
import * as evidence from "@rasm/contracts/rasm/contracts/element/edit_pb"
import {
  Brand, Cause, Chunk, DateTime, Duration, Effect, Either, Encoding, Exit, Function, HashMap, Match, Option, Order, pipe, Predicate,
  Record,
  Schedule, STM, TMap, TRef, type SchemaAST,
} from "effect"
import { Fault } from "../value/fault.ts"
import { Format } from "./format.ts"

const _causes = ["malformed", "overrun", "sequence", "parity", "drift", "stale", "conflict"] as const

const _overrunAxes = [
  "payload", "frames", "assembly",
  "walk-depth", "walk-fan",
] as const
const _paritySubjects = ["key", "golden-bytes", "semantic", "merkle-root"] as const
const _gapSubjects = ["ordinal", "total", "tail"] as const
const _coordinate = Schema.OptionFromSelf(
  Schema.Struct({ artifact: Digest.codecs.content.wire, generation: Schema.Int.pipe(Schema.nonNegative()) }),
)

const _policy = Fault.Class.family(_causes, {
  malformed: {
    ...Fault.Class.row({
      class: "malformed",
      leg: "codec",
      detail: Schema.Struct({ at: Schema.Literal("source", "decode"), issue: Schema.String }),
      render: ({ at, issue }) => `<${at}> refused the frame — ${issue}`,
    }),
    held: true,
    replayable: true,
  },
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

class WireFault extends Schema.TaggedError<WireFault>()("WireFault", {
  family: _faultLiteral,
  case: _policy.payload,
}) {
  static readonly bySeverity: Order.Order<WireFault> = Order.mapInput(Fault.Class.order, (fault: WireFault) => fault.class)
  static readonly dominant = (faults: Array.NonEmptyReadonlyArray<WireFault>): WireFault =>
    Array.max(faults, WireFault.bySeverity)
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

const _INTAKE = { capacity: 256, budget: "bulk" } as const satisfies { capacity: number; budget: Fault.Budget.Kind }
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

const _armOf = (family: Wire.FaultFamily): Format.Arm =>
  _family.is(family) ? _rows[family].arm : _faultArms[family]

class Quarantine extends Effect.Service<Quarantine>()("@rasm/core/Quarantine", {
  scoped: Effect.gen(function* () {
    const held = yield* STM.commit(TMap.empty<bigint, PoisonFrame>())
    const serial = yield* STM.commit(TRef.make(0n))
    const admit = (frame: PoisonFrame): STM.STM<PoisonFrame> =>
      STM.gen(function* () {
        yield* TMap.set(held, frame.slot, frame)
        const slots = yield* TMap.keys(held)
        if (slots.length > _INTAKE.capacity) {
          yield* Option.match(_oldest(slots), { onNone: () => STM.void, onSome: (slot) => TMap.remove(held, slot) })
        }
        return frame
      })
    const settled = (frame: PoisonFrame): Effect.Effect<void> => STM.commit(TMap.remove(held, frame.slot))
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

```typescript
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
- Owner: generated `crdt.CrdtOpWire` is the sole ten-arm operation vocabulary; `CrdtOp` is `Format.proto.message(CrdtOpWireSchema)` refined only by producer-canonical repeated-row order, never a hand TypeScript union.
- Law: the generic op-log stays the producer's thirteen-slot MessagePack record and retains its raw payload. Only a `family === "crdt"` entry admits that payload through the generated required oneof; every other family stays opaque and no `Any`, arm tag table, or string bag enters.
- Law: an unset or unknown oneof arm refuses at descriptor admission rather than crossing the merge algebra as opaque bytes; vector and observed-tag rows arrive in the corpus-declared strict order.
- Law: evidence render arms retain encoded `frameHash`, optional `drawHash`, and optional canonical pixel identity.

```typescript
import { Clock } from "../value/clock.ts"
import { Identity } from "../value/identity.ts"
import { Shape } from "../value/schema.ts"
import { Causal } from "../state/causal.ts"
import { Commit } from "../state/commit.ts"
import { Board } from "../observe/board.ts"

type _Cell = Schema.Schema.Any | Schema.Element<Schema.Schema.Any, "?">
type _Pairs = ReadonlyArray<readonly [string, _Cell]>
type _Slots<S extends _Pairs> = { readonly [I in keyof S]: S[I][1] }
type _Named<S extends _Pairs> = {
  readonly [E in S[number] as E[0]]: E[1] extends Schema.Element<infer T extends Schema.Schema.Any, "?">
    ? Schema.optionalWith<T, { readonly as: "Option"; readonly exact: true }>
    : E[1]
}

const _held = (cell: _Cell): Schema.Schema.Any =>
  Schema.isSchema(cell) ? cell : Schema.optionalWith(cell.from, { as: "Option", exact: true })
const _fields = <const S extends _Pairs>(slots: S): _Named<S> & Schema.Struct.Fields =>
  Record.map(Record.fromEntries(slots), _held) as _Named<S> & Schema.Struct.Fields
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

const _text = Schema.String
const _octets = Schema.Uint8ArrayFromSelf
const _fixed16 = _octets.pipe(Schema.filter((value) => value.length === 16, { message: () => "<fixed-16-octets>" }))
const _traceId = _octets.pipe(Schema.filter((value) => value.length === 0 || value.length === 16, { message: () => "<trace-id-width>" }))
const _i63 = Schema.Union(Schema.BigIntFromSelf, Schema.BigIntFromNumber).pipe(
  Schema.betweenBigInt(0n, 9_223_372_036_854_775_807n),
)
const _CrdtOp = Format.proto.message(CrdtOpWireSchema)
type CrdtOp = typeof _CrdtOp.Type

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
- Law: the appearance vocabulary IS the generated `appearance` enum set — every roster derives from its enum, every legality column this page owns closes against the enum's defined members, and the plane laws no field rule states refine the generated `Set` above the descriptor.
- Law: `Wire.Artifact.reference` lands the exact 32-byte `ArtifactRef.sha256` as one `ArtifactSha256` identity and retains `artifactBytes`; SHA-256 hashes the ordered raw artifact octets with no semantic prefix or frame bytes.
- Law: `Wire.Artifact.frame` lands the generated nested reference whole, and `Wire.Artifact.mint` proves a stream's ordered payload through the protocol-fixed SHA-256 owner; SHA-256 never enters `Digest.Kind` or replaces XXH3 semantic/cache keys.
- Law: `Wire.Texture.reference` lands `PlaneRef.artifact` whole; no digest, extent, path, or file-derived identity sits beside it.
- Law: the IFC typed-value fold lands as the producer's own fourteen-arm `{ case, value }` face, closed both ways against its roster.
- Law: a record with a REFINED key closes through `Shape.Record`, so a refused key fails the decode instead of vanishing from the map.
- Law: the control-intent family closes THIRTY-ONE arms at the corpus; the walk projection reads the generated `arm` face and an unset arm yields no child.
- Boundary: raw GeoJSON text and CloudEvents remain outside the registry because no typed family crosses.
- Boundary: a nested family member registers no census row, so a tenant context, a command payload, an OpenPBR vector, and a plane carry no gate or parity.

```typescript
import { VariantSchema } from "@effect/experimental"
import { Context, Layer } from "effect"

const _absent: typeof Schema.OptionFromNonEmptyTrimmedString = Schema.OptionFromNonEmptyTrimmedString

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
const _remoteClasses = {
  terminal: "invalid",
  transient: "unavailable",
  retryAfter: "exhausted",
} as const satisfies { readonly [K in (typeof _Recovery.Type)["kind"]]: Fault.Class.Kind }

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
class Transport extends Schema.TaggedError<Transport>()("Transport", {
  kind: Schema.Literal(..._transportKinds),
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

type InvokeFault = Remote | Transport | MalformedDetail
const _invokeReasons = [
  "remote-terminal", "remote-transient", "remote-retry-after",
  ..._transportKinds,
  "malformed-detail",
] as const
type InvokeReason = (typeof _invokeReasons)[number]
type TransportKind = (typeof _transportKinds)[number]

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

type _Defined<E extends { readonly UNSPECIFIED: 0 }> = Exclude<E[keyof E], E["UNSPECIFIED"]>
const _defined = <E extends { readonly UNSPECIFIED: 0 }>(members: E): ReadonlyArray<_Defined<E>> =>
  Array.filter(Record.values(members), (member): member is _Defined<E> => member !== members.UNSPECIFIED)

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

const _depthRows = {
  [appearance.Depth.U8]: { integer: true, deep: false },
  [appearance.Depth.U16]: { integer: true, deep: true },
  [appearance.Depth.F16]: { integer: false, deep: true },
  [appearance.Depth.F32]: { integer: false, deep: true },
} as const satisfies { readonly [K in _Defined<typeof appearance.Depth>]: { readonly integer: boolean; readonly deep: boolean } }

const _units = ["mm", "nm", "cd/m2"] as const

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
  [_R.SPECULAR_ROUGHNESS_ANISOTROPY_ROTATION]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.SPECULAR_IOR]: { ch: 1, transfer: _T.RAW, neutral: [1.5], unit: null, mip: _M.BOX },
  [_R.TRANSMISSION_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.TRANSMISSION_ROUGHNESS]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.ROUGHNESS_VARIANCE },
  [_R.SUBSURFACE_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.SUBSURFACE_RADIUS]: { ch: 3, transfer: _T.RAW, neutral: [1, 0.5, 0.25], unit: "mm", mip: _M.BOX },
  [_R.COAT_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.COAT_COLOR]: { ch: 3, transfer: _T.SRGB, neutral: [1, 1, 1], unit: null, mip: _M.KAISER },
  [_R.COAT_ROUGHNESS]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.ROUGHNESS_VARIANCE },
  [_R.COAT_IOR]: { ch: 1, transfer: _T.RAW, neutral: [1.6], unit: null, mip: _M.BOX },
  [_R.FUZZ_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.FUZZ_COLOR]: { ch: 3, transfer: _T.SRGB, neutral: [1, 1, 1], unit: null, mip: _M.KAISER },
  [_R.FUZZ_ROUGHNESS]: { ch: 1, transfer: _T.LINEAR, neutral: [0.5], unit: null, mip: _M.ROUGHNESS_VARIANCE },
  [_R.THIN_FILM_WEIGHT]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: null, mip: _M.BOX },
  [_R.THIN_FILM_THICKNESS]: { ch: 1, transfer: _T.RAW, neutral: [500], unit: "nm", mip: _M.BOX },
  [_R.THIN_FILM_IOR]: { ch: 1, transfer: _T.RAW, neutral: [1.4], unit: null, mip: _M.BOX },
  [_R.EMISSION_COLOR]: { ch: 3, transfer: _T.SRGB, neutral: [1, 1, 1], unit: null, mip: _M.KAISER },
  [_R.EMISSION_LUMINANCE]: { ch: 1, transfer: _T.LINEAR, neutral: [0], unit: "cd/m2", mip: _M.BOX },
  [_R.GEOMETRY_OPACITY]: { ch: 1, transfer: _T.LINEAR, neutral: [1], unit: null, mip: _M.BOX },
  [_R.GEOMETRY_NORMAL]: { ch: 3, transfer: _T.RAW, neutral: [0, 0, 1], unit: null, mip: _M.NORMAL_RENORMALIZE },
  [_R.GEOMETRY_COAT_NORMAL]: { ch: 3, transfer: _T.RAW, neutral: [0, 0, 1], unit: null, mip: _M.NORMAL_RENORMALIZE },
  [_R.GEOMETRY_TANGENT]: { ch: 3, transfer: _T.RAW, neutral: [1, 0, 0], unit: null, mip: _M.NORMAL_RENORMALIZE },
  [_R.GEOMETRY_COAT_TANGENT]: { ch: 3, transfer: _T.RAW, neutral: [1, 0, 0], unit: null, mip: _M.NORMAL_RENORMALIZE },
  [_R.HEIGHT]: { ch: 1, transfer: _T.RAW, neutral: [0.5], unit: null, mip: _M.BOX },
  [_R.OCCLUSION]: { ch: 1, transfer: _T.LINEAR, neutral: [1], unit: null, mip: _M.BOX },
  [_R.CURVATURE]: { ch: 1, transfer: _T.RAW, neutral: [0], unit: null, mip: _M.BOX },
} as const satisfies { readonly [K in Texture.Role]: _ChannelFacts }

const _authored = (role: Texture.Role, depth: Texture.Depth): _SceneTransfer =>
  _channelRows[role].transfer === _T.SRGB && !_depthRows[depth].integer ? _T.LINEAR : _channelRows[role].transfer

const _mipLawful = (role: Texture.Role, mips: number, policy: Texture.MipPolicy): boolean =>
  mips === 1 ? policy === _M.NONE : policy === _M.BOX || policy === _channelRows[role].mip
const _widthFloor = (role: Texture.Role): 1 | 2 | 4 =>
  _channelRows[role].ch === 1 ? 1 : _channelRows[role].mip === _M.NORMAL_RENORMALIZE ? 2 : 4

const { PlaneFormat: _P, Depth: _D } = appearance
const _planeRows = {
  [_P.R8]: { depth: _D.U8, width: 1, web: true }, [_P.R16]: { depth: _D.U16, width: 1, web: false },
  [_P.R16F]: { depth: _D.F16, width: 1, web: true }, [_P.R32F]: { depth: _D.F32, width: 1, web: true },
  [_P.RG8]: { depth: _D.U8, width: 2, web: true }, [_P.RG16]: { depth: _D.U16, width: 2, web: false },
  [_P.RG16F]: { depth: _D.F16, width: 2, web: true }, [_P.RG32F]: { depth: _D.F32, width: 2, web: true },
  [_P.RGBA8]: { depth: _D.U8, width: 4, web: true }, [_P.RGBA16]: { depth: _D.U16, width: 4, web: true },
  [_P.RGBA16F]: { depth: _D.F16, width: 4, web: true }, [_P.RGBA32F]: { depth: _D.F32, width: 4, web: true },
} as const satisfies { readonly [K in Texture.PlaneFormat]: { readonly depth: Texture.Depth; readonly width: 1 | 2 | 4; readonly web: boolean } }

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
const _associationLawful = (mode: Texture.AlphaMode, container: Texture.Container, depth: Texture.Depth): boolean =>
  mode === _A.NONE || _containerRows[container].alpha === mode || _depthRows[depth].deep

const _L = appearance.LayerLaw
const _layerRows = {
  [_L.NONE]: { extent: 1, gltf: true }, [_L.CUBE_FACES]: { extent: 6, gltf: false },
  [_L.ARRAY]: { extent: null, gltf: false }, [_L.VOLUME]: { extent: null, gltf: false }, [_L.FRAMES]: { extent: null, gltf: false },
} as const satisfies { readonly [K in Texture.LayerLaw]: { readonly extent: number | null; readonly gltf: boolean } }
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

class MeasureBand extends Schema.Class<MeasureBand>("MeasureBand")({
  kind: _enum(property.UncertaintyKind),
  lowerSi: Schema.Number,
  upperSi: Schema.Number,
  standardDeviationSi: _option(Schema.Number),
  coverageFactor: _option(Schema.Number),
}) {}

class MeasureValue extends Schema.Class<MeasureValue>("MeasureValue")({
  dimension: _member(property.DimensionWireSchema),
  si: Schema.Number,
  uncertainty: _option(MeasureBand),
}) {}

const _attrCases = [
  "text", "measure", "boolean", "logical", "reference", "bounded", "temporal", "integer", "number", "binary",
  "enumerated", "list", "table", "complex",
] as const

const _Logical = Schema.Struct({ value: _option(Schema.Boolean) })
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

type _AttrArm = _AttrValue["case"]
type _AttrWidened<K extends _AttrArm = (typeof _attrCases)[number]> = K
type _AttrClosed<K extends (typeof _attrCases)[number] = _AttrArm> = K

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
  static readonly Json: Schema.Schema<Node, Shape.Json> = Schema.compose(
    Schema.compose(Shape.Json, Format.proto.json(graph.NodeWireSchema), { strict: false }),
    Node.FromWire,
  )
}

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

```typescript
import { Stream } from "effect"

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

const _schema = {
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
  WriteOutcomeWire: Format.proto.frame(Format.proto.suite.WriteOutcomeWire, "json"),
  FlagVerdictWire: Format.proto.frame(Format.proto.suite.FlagVerdictWire, "json"),
  AppUiSurfaceProgram: Format.proto.frame(Format.proto.suite.AppUiSurfaceProgram, "json"),
  CommandGateWire: Format.proto.frame(Format.proto.suite.CommandGateWire, "json"),
  EvidenceTimelineWire: Format.proto.frame(Format.proto.suite.EvidenceTimelineWire, "json"),
  BcfTopicWire: Format.proto.frame(Format.proto.suite.BcfTopicWire, "json"),
  BcfViewpointWire: Format.proto.frame(Format.proto.suite.BcfViewpointWire, "json"),
  ModelDiffWire: Format.proto.frame(Format.proto.suite.ModelDiffWire, "json"),
  Material: Format.proto.frame(Format.proto.suite.Material),
  Set: Format.proto.family(Format.proto.suite.Set, _AppearanceSet),
  BoardPackWire: Board.Pack.Landed,
} as const

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
  WriteOutcomeWire: _proto("WriteOutcomeWire"),
  FlagVerdictWire: _proto("FlagVerdictWire"),
  AppUiSurfaceProgram: _proto("AppUiSurfaceProgram"),
  CommandGateWire: _proto("CommandGateWire"),
  EvidenceTimelineWire: _proto("EvidenceTimelineWire"),
  BcfTopicWire: _proto("BcfTopicWire"),
  BcfViewpointWire: _proto("BcfViewpointWire"),
  ModelDiffWire: _proto("ModelDiffWire"),
  Material: _proto("Material"),
  Set: _proto("Set"),
  BoardPackWire: _row("decode", "proto", _schema.BoardPackWire),
} as const

const _family = Shape.vocabulary(_families, _rows)

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
  MenuRowWire: {
    children: (node: MenuRowWireValid): ReadonlyArray<MenuRowWireValid> => node.rows,
  },
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

const _WALK: Shape.Bound<"hops"> = Shape.Bound.of("hops", Shape.Ingress.floor.depth)

const _overspent = (
  family: Wire.FaultFamily,
  axis: "walk-depth" | "walk-fan",
  actual: number,
  expected: number,
): WireFault => new WireFault({ family, case: { reason: "overrun", axis, actual, expected, at: Option.none() } })

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
    type Case = _WireFaultCase
    type Reason = _WireFaultReason
  }
  type OverrunAxis = _OverrunAxis
  type FaultDetail = Decoded<"FaultDetail">
  type InvokeFault = _InvokeFault
  type InvokeReason = _InvokeReason
  type RemoteDetail = _RemoteDetail
  type TransportKind = _TransportKind
  type CommandAvailability = Decoded<"CommandAvailability">
  type Credential = Decoded<"CredentialPublicWire">
  type DescriptorPin = Decoded<"DescriptorPinWire">
  type BenchmarkClaim = Decoded<"BenchmarkClaimWire">
  type BindingStatus = Decoded<"BindingStatus">
  type CoercedValue = Decoded<"CoercedValueWire">
  type WriteOutcome = Decoded<"WriteOutcomeWire">
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
  namespace Walk {
    type Family = keyof typeof _walkRows
    type Row<A> = { readonly children: (node: A) => ReadonlyArray<A> }
    type Node<K extends Family> = (typeof _walkRows)[K] extends { readonly children: (node: infer N) => unknown } ? N
      : never
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

```typescript
import type { Duration, Equivalence } from "effect"

const _feedKeys = ["FlagVerdictWire", "BindingStatus", "CommandGateWire"] as const

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

const _CADENCE = {
  display: { units: 240, per: "1 second", burst: 60 },
  control: { units: 60, per: "1 second", burst: 20 },
} as const satisfies { readonly [K in feed.Band]: feed.Flow }

const _alike = <K extends feed.Family>(family: K) => Schema.equivalence(Schema.typeSchema(_rows[family].schema))
const _feeds: { readonly [K in feed.Family]: feed.Row<Wire.Decoded<K>> } = {
  FlagVerdictWire: {
    subject: (verdict) => verdict.flag,
    alike: _alike("FlagVerdictWire"),
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

const _transitions = <A>(row: feed.Row<A>) =>
<E, R>(marks: Stream.Stream<A, E, R>): Stream.Stream<Either.Either<A, Fault.Drop.Fact>, E, R> =>
  marks.pipe(
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

```typescript
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { Wire }
```

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
