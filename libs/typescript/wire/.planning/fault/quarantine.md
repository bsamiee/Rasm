# [WIRE_QUARANTINE]

`fault/quarantine.ts` is the folder's local fault rail and the poison-frame quarantine: one reason-discriminated `WireFault` family carries every beyond-admission failure the folder mints — parity misses, sequence gaps, ceiling overruns, drift refusals, OCC staleness — and one `Quarantine` service diverts the frame that produced it into a bounded, replayable intake instead of dropping evidence or crashing a stream. Admission failure itself stays `ParseResult.ParseError` on the one decode rail; this module classifies it into the family only at the quarantine seam. `WireFault` is the per-folder `Data.TaggedError`-altitude rail of invariant 6 — distinct from the C#-minted `FaultDetail` reconstruction at `fault/detail.ts` and never a substitute for it.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]       | [OWNS]                                                                             |
| :-----: | :-------------- | :---------------------------------------------------------------------------------- |
|   [1]   | `WIRE_FAULT`    | the folder fault family: reason policy table, evidence fields, rank fold             |
|   [2]   | `POISON_INTAKE` | the `PoisonFrame` row, the `Quarantine` service, the `divert` stream/rail transformer |
|   [3]   | `REPLAY`        | budgeted re-decode of quarantined frames; release and retirement policy              |

## [2]-[WIRE_FAULT]

- Owner: `WireFault` — one `Schema.TaggedError` class for the whole folder, reason-discriminated over the `_policy` table; the class serializes because quarantine rows persist and replay across sessions.
- Entry: `new WireFault({ family, reason, detail, evidence })` at every minting page; `fault.policy` projects the row; `WireFault.dominant` collapses an accumulated non-empty fault set to the highest-ranked representative.
- Growth: a new failure cause is one `_policy` row — routing, quarantine disposition, and replayability land as data; zero new classes, zero new catch arms.
- Law: the family is sized by routing, not by cause — every consumer routes on the one `WireFault` tag and reads `reason`/`family` as evidence; a per-cause class or a second folder fault family is the named spam defect.
- Law: `family` is typed by the `Inventory` census literal, so a fault names which wire family produced it and the census closure makes an unnamed family a compile error.
- Law: evidence is data — `evidence` carries the OCC `{ actual, expected }` pair for `stale`, the key pair for `parity`, the coordinate pair for `sequence`; `message` derives from fields and is never stored.
- Law: `sequence` never quarantines — a gap has no frame to hold; the fault is evidence the consumer's re-pull decision reads, and the policy row states it.
- Law: `ParseError` is not re-wrapped in flow — decode surfaces keep `ParseResult.ParseError` on their error channel; classification into `WireFault` happens exactly once, at the quarantine intake, where the frame context exists to name `family` and `reason`.
- Boundary: the wire-crossed fault altitude is `fault/detail.ts`; a node rail importing `FaultDetail` for a local failure is the invariant-6 defect this family exists to prevent.

```typescript
import { Array, Option, Order, Schema } from "effect"
import { Inventory } from "../contract/drift.ts"

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
  family: Inventory.wire,
  reason: Schema.Literal("malformed", "truncated", "overrun", "sequence", "parity", "drift", "stale", "conflict"),
  detail: Schema.NonEmptyString,
  evidence: Schema.optionalWith(Schema.Struct({ actual: Schema.Unknown, expected: Schema.Unknown }), { as: "Option" }),
}) {
  static readonly byRank: Order.Order<WireFault> = Order.mapInput(Order.number, (fault: WireFault) => fault.policy.rank)
  static readonly dominant = (faults: Array.NonEmptyReadonlyArray<WireFault>): WireFault => Array.max(faults, WireFault.byRank)
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
  type _Rows<T extends Record<Reason, Row> = typeof _policy> = T
}
```

## [3]-[POISON_INTAKE]

- Owner: `Quarantine` — one `Effect.Service` whose scoped construction owns the bounded intake `Mailbox`, the held-frame census `Ref`, and the drain fiber; `PoisonFrame` rides the class as the `Quarantine.Frame` static so one import carries the service and its row shape.
- Entry: `Quarantine.divert` — the one dual transformer every decode surface composes: a `WireFault` whose policy row says `quarantine: true` is delivered to the intake with its frame bytes and the rail continues as `Either.left`; a non-quarantining fault propagates typed. `ParseError` diverts through the same transformer with the classifying fold applied at the seam.
- Receipt: `intake` answers with the stored `PoisonFrame` — coordinate, held octets, fault, attempt count, mint instant — the evidence a drain, a dashboard, or a replay reads.
- Growth: a new intake policy axis — retention ceiling, per-family cap — is one `_INTAKE` field consumed inside the constructor; consumers never re-tune the channel.
- Law: quarantine is a typed divert, never a dropped element — the faulted frame's octets are held verbatim (`Uint8ArrayFromSelf`), the rail proceeds, and the evidence survives to the drain; a recovery substituting a default value where the fault feeds a report is rejected.
- Law: the intake is bounded with `strategy: "suspend"` — a poison storm backpressures its producer instead of exhausting memory; the ceiling is an `_INTAKE` policy value.
- Law: `attempts` lives on the frame, not in a side table — replay increments it through the census `Ref`, and retirement is a policy read (`replayable` false, or attempts spent).
- Boundary: which stream composes `divert` is each codec page's decision; the availability degradation a poison storm should trigger is `state`'s vocabulary, wired at the app root.

```typescript
import { DateTime, Effect, Either, Function, HashMap, Mailbox, Option, Ref, Schema } from "effect"
import { Inventory } from "../contract/drift.ts"

const _INTAKE = { capacity: 256, attempts: 3 } as const

class PoisonFrame extends Schema.Class<PoisonFrame>("PoisonFrame")({
  family: Inventory.wire,
  octets: Schema.Uint8ArrayFromSelf,
  fault: WireFault,
  at: Schema.DateTimeUtc,
  attempts: Schema.Int.pipe(Schema.nonNegative()),
}) {
  get replayable(): boolean {
    return this.fault.policy.replayable && this.attempts < _INTAKE.attempts
  }
}

class Quarantine extends Effect.Service<Quarantine>()("wire/Quarantine", {
  scoped: Effect.gen(function* () {
    const box = yield* Mailbox.make<PoisonFrame>({ capacity: _INTAKE.capacity, strategy: "suspend" })
    const held = yield* Ref.make(HashMap.empty<string, PoisonFrame>())
    const intake = (family: Inventory.Family, octets: Uint8Array, fault: WireFault): Effect.Effect<PoisonFrame> =>
      Effect.gen(function* () {
        const now = yield* DateTime.now
        const frame = new PoisonFrame({ family, octets, fault, at: now, attempts: 0 })
        yield* box.offer(frame)
        yield* Ref.update(held, HashMap.set(`${family}:${DateTime.formatIso(now)}`, frame))
        return frame
      })
    return {
      intake,
      census: Ref.get(held).pipe(Effect.map(HashMap.size)),
      taken: box.takeAll,
      release: (key: string) => Ref.update(held, HashMap.remove(key)),
    }
  }),
  accessors: true,
}) {
  static readonly Frame: typeof PoisonFrame = PoisonFrame
  static readonly divert: {
    (context: { readonly family: Inventory.Family; readonly octets: Uint8Array }): <A, R>(
      self: Effect.Effect<A, WireFault, R>,
    ) => Effect.Effect<Either.Either<A, WireFault>, WireFault, R | Quarantine>
    <A, R>(
      self: Effect.Effect<A, WireFault, R>,
      context: { readonly family: Inventory.Family; readonly octets: Uint8Array },
    ): Effect.Effect<Either.Either<A, WireFault>, WireFault, R | Quarantine>
  } = Function.dual(
    2,
    <A, R>(
      self: Effect.Effect<A, WireFault, R>,
      context: { readonly family: Inventory.Family; readonly octets: Uint8Array },
    ): Effect.Effect<Either.Either<A, WireFault>, WireFault, R | Quarantine> =>
      self.pipe(
        Effect.map(Either.right),
        Effect.catchIf(
          (fault) => fault.policy.quarantine,
          (fault) => Effect.as(Quarantine.intake(context.family, context.octets, fault), Either.left(fault)),
        ),
      ),
  )
  static readonly classified = (family: Inventory.Family, detail: string): WireFault =>
    new WireFault({ family, reason: "malformed", detail, evidence: Option.none() })
}
```

## [4]-[REPLAY]

- Owner: the replay fold on the same module — quarantined frames re-offer to their family decode under a budget `Schedule`, success releases the frame, exhaustion retires it terminally.
- Entry: `replayed(decode)` — the decode arrives as a parameter keyed by family, so replay stays generic over every codec page and imports none of them; the app root supplies the family-indexed decode record it composed from the codec pages' `#vocab` surfaces.
- Growth: replay pacing is the `_REPLAY` schedule value; a per-family pacing override is a future policy column on `_policy`, never a second replay path.
- Law: replay consumes the intake as a drain — each taken frame either decodes (released, decoded value delivered to the supplied sink) or re-enters with `attempts + 1`; a frame whose policy is non-replayable or whose attempts are spent is retired to the terminal sink with its fault intact.
- Law: replay never re-classifies — the original fault travels with the frame through every attempt, so the terminal report shows the first cause, not the last symptom.

```typescript
import { Array, Effect, Match, Schedule } from "effect"
import type { Inventory } from "../contract/drift.ts"

const _REPLAY: Schedule.Schedule<number> = Schedule.spaced("30 seconds").pipe(Schedule.intersect(Schedule.recurs(8)), Schedule.map(([, count]) => count))

const replayed = <A, R>(
  decode: (family: Inventory.Family, octets: Uint8Array) => Effect.Effect<A, WireFault, R>,
  delivered: (value: A) => Effect.Effect<void, never, R>,
  retired: (frame: PoisonFrame) => Effect.Effect<void, never, R>,
): Effect.Effect<void, never, R | Quarantine> =>
  Effect.gen(function* () {
    const quarantine = yield* Quarantine
    const [frames] = yield* quarantine.taken
    yield* Effect.forEach(
      frames,
      (frame) =>
        Match.value(frame.replayable).pipe(
          Match.when(false, () => retired(frame)),
          Match.when(true, () =>
            decode(frame.family, frame.octets).pipe(
              Effect.matchEffect({
                onFailure: () => quarantine.intake(frame.family, frame.octets, frame.fault).pipe(Effect.asVoid),
                onSuccess: delivered,
              }),
            ),
          ),
          Match.exhaustive,
        ),
      { concurrency: 1, discard: true },
    )
  }).pipe(Effect.repeat(_REPLAY), Effect.asVoid)

// --- [EXPORTS] --------------------------------------------------------------------------

export { Quarantine, replayed, WireFault }
```
