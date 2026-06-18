# [PROJECTION_WATERMARK]

The event-time progress and late-arrival vocabulary the window engine folds against — event time reconstructed from the wire HLC pair, the monotonic `Watermark` mark, and the `allowedLateness` horizon that decides whether an out-of-order row is a fresh fold or a tracked retraction. Event time is the wire HLC `(physical, logical)`, never wall-clock arrival, so a window replays identically. A late row preceding the held watermark by less than `allowedLateness` is folded as a correction; the silent drop is the deleted form.

## [1]-[INDEX]

One cluster: `[2]-[WATERMARK]` owns `eventNanos`, the `Watermark` mark, `advance`, and `isLate`.

## [2]-[WATERMARK]

- Owner: `eventNanos`, the event-time projection from the wire HLC pair; `Watermark`, the monotonic `(eventNanos, processed, late)` progress mark mirroring the C# `Watermark(EventTime, Processed)`; `advance`, the merge step that lifts the mark and tallies a late correction; and `isLate`, the `allowedLateness` horizon test. `defaultLateness` is the one lateness default the window engine inherits.
- Cases: `eventNanos` reconstructs event time as the wire `physical` instant (ISO-8601 extended) scaled to nanos plus the HLC `logical` half as a sub-millisecond tiebreak; `advance` raises `eventNanos` monotonically (never regresses), increments `processed` per folded row, and increments `late` only on an out-of-order correction; `isLate` is the test against the held mark minus `allowedLateness`.
- Packages: `effect` for `Duration`.
- Growth: a new event-time field lands as one `eventNanos` projection arm; a new lateness posture lands as one `WindowSpec.allowedLateness` value.
- Boundary: the decode gate already admitted `physical` and `logical`, so this is projection, never re-validation; `processed` is the per-bucket throughput the `ui` reads off the cell, and `late` is the observable out-of-order count so a retraction is never silent.

```ts contract
import { Duration } from "effect";
import type { OpLogEntryWire } from "@rasm/ts";

const eventNanos = (entry: OpLogEntryWire): bigint =>
  BigInt(new Date(entry.physical).getTime()) * 1_000_000n + entry.logical;

interface Watermark {
  readonly eventNanos: bigint;
  readonly processed: bigint;
  readonly late: bigint;
}
const watermarkZero: Watermark = { eventNanos: 0n, processed: 0n, late: 0n };

const advance = (mark: Watermark, at: bigint, lateRow: boolean): Watermark => ({
  eventNanos: at > mark.eventNanos ? at : mark.eventNanos,
  processed: mark.processed + 1n,
  late: mark.late + (lateRow ? 1n : 0n),
});

const isLate = (mark: Watermark, at: bigint, allowed: Duration.Duration): boolean =>
  at < mark.eventNanos - Duration.unsafeToNanos(allowed);

const defaultLateness: Duration.Duration = Duration.seconds(30);
```
