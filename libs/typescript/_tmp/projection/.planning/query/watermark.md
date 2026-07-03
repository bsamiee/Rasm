# [PROJECTION_WATERMARK]

The event-time progress and late-arrival vocabulary the window engine folds against — event time reconstructed from the wire HLC pair, the monotonic `Watermark` mark, and the `allowedLateness` horizon that decides whether an out-of-order row is a fresh fold or a tracked retraction. Event time is the wire HLC `(physical, logical)`, never wall-clock arrival, so a window replays identically. A late row preceding the held watermark by less than `allowedLateness` is folded as a correction; the silent drop is the deleted form.

## [01]-[INDEX]

- [01]-[WATERMARK]: Owns `eventNanos`, the `Watermark` mark, `advance`, and `isLate`.

## [02]-[WATERMARK]

- Owner: `eventNanos`, the event-time projection from the wire HLC pair; `Watermark`, the monotonic `(eventNanos, processed, late)` progress mark mirroring the C# `Watermark(EventTime, Processed)`; `advance`, the merge step that lifts the mark and tallies a late correction; and `isLate`, the `allowedLateness` horizon test. The lateness horizon is not a literal baked beside the projection — it is `LatenessProfile`, the one `as const satisfies Record<LatenessProfileKey, number>` table whose every row is a horizon in seconds for one fold class, and `lateness` materializes a row into the `Duration` a `window#WINDOW_FOLD` `WindowSpec.allowedLateness` field carries, so a new horizon is a row value rather than a re-baked `Duration.seconds`. `defaultLateness` is the `standard` row materialized — the one default the window engine inherits, recovered from the table.
- Cases: `eventNanos` reconstructs event time as the wire `physical` instant (ISO-8601 extended) scaled to nanos plus the HLC `logical` half as a sub-millisecond tiebreak, flooring an unparseable instant to the zero epoch so the projection is total and a malformed stamp sorts as oldest rather than throwing `RangeError` off `BigInt(NaN)` and tearing down the fold fiber; `advance` raises `eventNanos` monotonically (never regresses), increments `processed` per folded row, and increments `late` only on an out-of-order correction; `isLate` is the test against the held mark minus `allowedLateness`. `LatenessProfile` carries three rows — `tight` (a `5s` horizon for an interactive surface that finalizes aggressively), `standard` (the `30s` horizon every fold inherits by default), and `relaxed` (a `300s` horizon for a resumable bulk replay that tolerates a wide out-of-order spread); `lateness` reads a row through indexed access and lifts the second count into `Duration.seconds`, so the horizon is a table number every profile shares the construction of, and `keyof typeof LatenessProfile` projects `LatenessProfileKey` so a fourth horizon breaks every selection site at compile time.
- Packages: `effect` for `Duration`.
- Growth: a new event-time field lands as one `eventNanos` projection arm; a new lateness posture lands as one `LatenessProfile` row a `WindowSpec.allowedLateness` field inherits, never a re-baked `Duration.seconds` literal.
- Boundary: the decode gate already admitted `physical` and `logical`, so this is projection, never re-validation; `processed` is the per-bucket throughput the `ui` reads off the cell, and `late` is the observable out-of-order count so a retraction is never silent; the lateness horizon is a `LatenessProfile` row value the window spec carries, never a hardcode baked into the fold.

```ts contract
import { Duration } from "effect";
import type { OpLogEntryWire } from "@rasm/interchange";

const eventNanos = (entry: OpLogEntryWire): bigint => {
  const ms = Date.parse(entry.physical);
  return (Number.isNaN(ms) ? 0n : BigInt(ms)) * 1_000_000n + entry.logical;
};

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

const LatenessProfile = {
  tight: 5,
  standard: 30,
  relaxed: 300,
} as const satisfies Record<string, number>;

type LatenessProfileKey = keyof typeof LatenessProfile;

const lateness = (key: LatenessProfileKey): Duration.Duration => Duration.seconds(LatenessProfile[key]);

const defaultLateness: Duration.Duration = lateness("standard");
```
