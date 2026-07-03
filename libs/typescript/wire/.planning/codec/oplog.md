# [WIRE_OPLOG]

`codec/oplog.ts` is the CRDT log rail: the C#-minted `OpLog` wire — a length-open MessagePack sequence of journal entries from `Rasm.Persistence` — decodes as one backpressured `Stream` over the `Pack` engine, each entry landing a sequence coordinate plus a decoded `CrdtOp`, with resume-watermark dropping, gap evidence, and poison-entry quarantine folded into the pipeline itself. `store/journal` consumes the decoded entry stream through `#vocab` wiring at the app root; the log arrives, decodes, and flows without a monolithic buffer, and the resume coordinate lives in the source cursor, never in a downstream dedup set.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]    | [OWNS]                                                                       |
| :-----: | :----------- | :------------------------------------------------------------------------------ |
|   [1]   | `LOG_STREAM` | the entry shape, the per-entry decode with poison divert, the two-lane contract  |
|   [2]   | `WATERMARK`  | resume dropping, sequence-continuity evidence, the frontier fold                 |

## [2]-[LOG_STREAM]

- Owner: `Entry` — the journal entry class `{ seq, op }` where `seq` is the journal's dense i64 coordinate and `op` is the `CrdtOp` family value; `_diverted`, the per-entry admission that classifies a malformed entry and holds it in quarantine while the log continues.
- Entry: interior — the assembled `OpLog` owner in `[3]` is the module's one surface.
- Receipt: the survivor lane is `Either.right<Entry>` in arrival order — `store/journal` appends the rights; the left lane carries typed faults (malformed entries already quarantined, gap evidence) to the consumer's lag telemetry. Both lanes are typed; nothing drops silently.
- Growth: an entry envelope axis (a shard coordinate, a compression flag) is one field on `Entry` mirroring the C# mint; the pipeline shape never changes.
- Law: the codec stays a pure function under the stream — `decodeMultiStream` yields raw values through `Pack.stream`, the Effect `Stream` owns backpressure and the halt; an event-emitter frame loop or a node `Transform` re-invents the rail and is rejected.
- Law: poison is per-entry, never per-log — one malformed entry quarantines with its re-encoded octets and the log continues; only a transport-level fault (the source iterable itself failing) ends the stream, typed as the `WireFault` the `Pack.stream` lift minted.
- Law: `seq` is `bigint` — the i64 coordinate rides `useBigInt64` fidelity end to end; arithmetic stays in `bigint` and a `Number` window is the precision defect.
- Boundary: op semantics are `codec/crdt.ts`'s family; durable append, retention, and snapshot interplay are `store/journal`'s pages, met at the app root; the snapshot that bounds a log replay is `codec/snapshot.ts`'s `frontier`.

```typescript
import { Effect, Either, Option, type ParseResult, Schema } from "effect"
import { Quarantine, WireFault } from "../fault/quarantine.ts"
import { CrdtOp, Pack } from "./crdt.ts"

class Entry extends Schema.Class<Entry>("OpLogEntry")({
  seq: Schema.BigIntFromSelf,
  op: CrdtOp,
}) {}

const _diverted = (raw: unknown): Effect.Effect<Either.Either<Entry, WireFault>, WireFault, Quarantine> =>
  Schema.decodeUnknown(Entry)(raw).pipe(
    Effect.mapError((issue: ParseResult.ParseError) =>
      new WireFault({ family: "OpLogWire", reason: "malformed", detail: issue.message, evidence: Option.none() }),
    ),
    Quarantine.divert({ family: "OpLogWire", octets: Pack.encode(raw) }),
  )
```

## [3]-[WATERMARK]

- Owner: `OpLog` — the assembled owner: the streaming entrypoint with the resume drop and the continuity Mealy step, plus the frontier fold a drained batch collapses to.
- Entry: `OpLog.stream(frames, resume)` — one signature owns the feed: bytes walk `Pack.stream`, entries decode and divert per `[2]`, entries at or below `resume` drop before any downstream work, and the Mealy step threads the last-seen coordinate, replacing a discontinuous entry's lane with `sequence` gap evidence carrying both coordinates. `OpLog.frontier(entries)` folds a batch to its high-water `seq`.
- Growth: a second continuity policy (per-shard watermarks) generalizes the accumulator to a keyed `HashMap` seed — one fold edit, signature unchanged.
- Law: continuity is evidence, not repair — a gap emits `WireFault` reason `sequence` on the left lane (never quarantined: there is no frame to hold) and the stream continues from the gap's far side; the consumer's re-pull decision reads the evidence, this rail never re-fetches.
- Law: the resume coordinate lives in the source — the caller's `resume` watermark drives the drop; a downstream dedup set over entry identity is unbounded memory and the rejected form.
- Law: ordering within one stream is the wire's promise — entries arrive in journal order per source; cross-source merge order is causal and belongs to `state/causal`, never re-derived here.

```typescript
import { Array, Order, Stream } from "effect"

const _BY_SEQ: Order.Order<Entry> = Order.mapInput(Order.bigint, (entry: Entry) => entry.seq)

const _gap = (actual: bigint, expected: bigint): WireFault =>
  new WireFault({ family: "OpLogWire", reason: "sequence", detail: "<gap>", evidence: Option.some({ actual, expected }) })

const OpLog: {
  readonly Entry: typeof Entry
  readonly stream: (
    frames: ReadableStream<Uint8Array> | AsyncIterable<Uint8Array>,
    resume: bigint,
  ) => Stream.Stream<Either.Either<Entry, WireFault>, WireFault, Quarantine>
  readonly frontier: (entries: ReadonlyArray<Entry>) => Option.Option<bigint>
} = {
  Entry,
  stream: (frames, resume) =>
    Pack.stream("OpLogWire")(frames).pipe(
      Stream.mapEffect(_diverted, { concurrency: 1 }),
      Stream.filter((lane) => Either.isLeft(lane) || lane.right.seq > resume),
      Stream.mapAccum(resume, (last, lane): readonly [bigint, Either.Either<Entry, WireFault>] =>
        Either.match(lane, {
          onLeft: (): readonly [bigint, Either.Either<Entry, WireFault>] => [last, lane],
          onRight: (entry) =>
            last === resume || entry.seq === last + 1n
              ? ([entry.seq, lane] as const)
              : ([entry.seq, Either.left(_gap(entry.seq, last + 1n))] as const),
        }),
      ),
    ),
  frontier: (entries) =>
    Array.isNonEmptyReadonlyArray(entries) ? Option.some(Array.max(entries, _BY_SEQ).seq) : Option.none(),
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { OpLog }
```
