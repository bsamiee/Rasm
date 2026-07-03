# [WIRE_PROGRESS]

`codec/progress.ts` decodes the `ProgressStore` stream projection — `ProgressMarkWire` length-prefixed frames from `Rasm.Compute/Runtime` — INTO `state/evidence/progress`'s `ProgressMark` vocabulary as one backpressured stream: framed decode over the proto engine, per-mark admission, consecutive-repeat suppression so downstream folds consume transitions rather than samples, and declared rate shaping. Compute-side progress is a projection feed; this rail lands it as state evidence with zero wire residue.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]     | [OWNS]                                                                 |
| :-----: | :------------ | :-------------------------------------------------------------------------- |
|   [1]   | `MARK_STREAM` | the framed decode into `ProgressMark`, dedup and shaping policy, the owner    |

## [2]-[MARK_STREAM]

- Owner: `Progress` — one assembled owner: the single-mark schema and the stream entrypoint; the mark shape is `state`-owned and never re-declared here.
- Entry: `Progress.stream(frames)` — bytes to `Stream<ProgressMark>`: `ProtoCodec.stream` walks the length-prefixed frames, each raw message admits through the composed `state` schema, a poison frame diverts to `Quarantine` and the feed continues, adjacent equal marks suppress, and the throttle prices volume under the `_FLOW` policy row. `Progress.mark` is the single-frame schema for point reads.
- Receipt: each survivor is a `ProgressMark` — the state evidence row `state/evidence/progress` folds into completion surfaces; `ui` progress displays read the state fold, never this rail.
- Growth: a new mark axis (a phase label, a sub-task coordinate) is a C# field plus the mirroring `state` vocabulary row; the pipeline is untouched. A new shaping need is a `_FLOW` field, never a second stream spelling.
- Law: transitions, not samples — `Stream.changesWith` under `_alike` suppresses consecutive repeats, so a chatty producer costs no downstream fold work; the equivalence is a composed projection instance over operation, stage, done, and total — stamp and tenant excluded, so a re-broadcast sample at a fresh stamp is still a repeat — because the derived whole-shape equivalence would suppress nothing.
- Law: shaping is declared — `Stream.throttle` with chunk-priced cost shapes the feed; a consumer-side sleep loop or counter re-derives the operator.
- Law: poison is per-frame — one malformed mark quarantines with its proto re-emit (the engine's own frame encode, computed only on the failure path), the feed continues; the stream ends only on transport fault.
- Boundary: the proto engine is `codec/proto.ts`; fold semantics over marks are `state/evidence/progress`; the compute channel that carries the frames arrives via `host/net` at the app root.

```typescript
import type { Message } from "@bufbuild/protobuf"
import { ProgressMark } from "@rasm/ts/state"
import { Chunk, Effect, Either, Equivalence, Option, type ParseResult, Schema, Stream } from "effect"
import { Quarantine, WireFault } from "../fault/quarantine.ts"
import { ProtoCodec } from "./proto.ts"

const _FLOW = { units: 240, per: "1 second", burst: 60 } as const

const _emit = Schema.encodeSync(ProtoCodec.frame(ProtoCodec.suite.ProgressMarkWire))

const _alike: Equivalence.Equivalence<ProgressMark> = Equivalence.struct({
  operation: Equivalence.string,
  stage: Equivalence.string,
  done: Equivalence.number,
  total: Option.getEquivalence(Equivalence.number),
})

const _admitted = (message: Message): Effect.Effect<Either.Either<ProgressMark, WireFault>, WireFault, Quarantine> =>
  Schema.decodeUnknown(ProgressMark)(message).pipe(
    Effect.mapError((issue: ParseResult.ParseError) =>
      new WireFault({ family: "ProgressMarkWire", reason: "malformed", detail: issue.message, evidence: Option.none() }),
    ),
    Quarantine.divert({ family: "ProgressMarkWire", octets: () => _emit(message) }),
  )

const Progress: {
  readonly mark: Schema.Schema<ProgressMark, Uint8Array>
  readonly stream: (
    frames: AsyncIterable<Uint8Array>,
  ) => Stream.Stream<ProgressMark, WireFault, Quarantine>
} = {
  mark: ProtoCodec.family(ProtoCodec.suite.ProgressMarkWire, ProgressMark),
  stream: (frames) =>
    ProtoCodec.stream(ProtoCodec.suite.ProgressMarkWire, "ProgressMarkWire")(frames).pipe(
      Stream.mapEffect(_admitted, { concurrency: 1 }),
      Stream.filterMap(Either.getRight),
      Stream.changesWith(_alike),
      Stream.throttle({ cost: Chunk.size, units: _FLOW.units, duration: _FLOW.per, burst: _FLOW.burst, strategy: "shape" }),
    ),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Progress }
```
