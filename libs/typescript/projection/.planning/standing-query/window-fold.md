# [PROJECTION_WINDOW_FOLD]

The event-time windowing and incremental-view-maintenance engine — `WindowKind` (Tumbling/Sliding/Session), the `bucketSet` fan-out, and the signed-delta `windowFold` over the decoded changefeed. The signed delta is Z-set arithmetic: an insert is `+1` multiplicity, a delete or retraction is negative, and a sliding aggregate maintains by adding entering and subtracting leaving rows rather than re-scanning the window — the DBSP linear-operator lineage the fold mirrors. A poll loop or a full re-query per row is the deleted form. The engine is a host-free peer of the C# standing-query owner, not a consumer of it: it folds the decoded op-log changefeed projected at `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` over wire fields only, re-implements windowing for the browser, and crosses no `IQueryable` shape.

## [1]-[INDEX]

One cluster: `[2]-[WINDOW_FOLD]` owns `WindowKind`, `WindowSpec`, `bucketSet`, `signOf`, and `windowFold`.

## [2]-[WINDOW_FOLD]

- Owner: `WindowKind`, the closed `Data.TaggedEnum` window family; `WindowSpec`, the kind plus its `watermark#WATERMARK` `allowedLateness`; `bucketSet`, the total dispatch returning the full set of buckets an event belongs to; `signOf`, the Z-set sign over the decoded wire kind; and `windowFold`, the one `keyed-fold#KEYED_FOLD` window arm keyed by event-time bucket.
- Cases: `bucketSet` floors a Tumbling event to exactly one bucket; a Sliding event belongs to every overlapping window whose `[start, start + size)` span covers it, striding by `slide` and spanning by `size` so a sliding window genuinely overlaps rather than aliasing to a tumbling one; a Session event is its own gap-start. `signOf` is total dispatch over the closed three-kind wire literal — upsert and presence add `+1`, delete subtracts `-1` — so a fourth kind breaks at compile time. `windowMerge` folds a fresh op's signed delta into its bucket count and advances the watermark; a late op folds the same signed delta into the same bucket cell (retraction-plus-restate) and tracks the correction in the watermark `late` tally.
- Packages: `effect` for `Data.TaggedEnum`, `Match`, `Stream`, `SubscriptionRef`, and `Duration`.
- Growth: a new window kind lands as one `WindowKind` variant breaking the `bucketSet` `$match` at compile time; a new aggregation lands as one signed-delta arm over the existing `WindowCell`.
- Boundary: the fold reads event time through `eventNanos` (a decode-admitted projection, never re-validated); a processing-time bucket is the deleted form — the engine folds event time only; the changefeed pipes through `stream-policy#STREAM_POLICY` so reconnect-replay re-buckets identically.

```ts contract
import { Data, Duration, Effect, Match, Option, Scope, Stream, SubscriptionRef } from "effect";
import type { OpLogEntryWire } from "@rasm/ts";
import { keyedFold } from "../fold-core/keyed-fold";
import type { StreamPolicy } from "../fold-core/stream-policy";
import { advance, eventNanos, isLate, watermarkZero, type Watermark } from "./watermark";

type WindowKind = Data.TaggedEnum<{
  readonly Tumbling: { readonly size: Duration.Duration };
  readonly Sliding: { readonly size: Duration.Duration; readonly slide: Duration.Duration };
  readonly Session: { readonly gap: Duration.Duration };
}>;
const WindowKind = Data.taggedEnum<WindowKind>();

interface WindowSpec {
  readonly kind: WindowKind;
  readonly allowedLateness: Duration.Duration;
}

const bucketSet = (spec: WindowSpec, at: bigint): ReadonlyArray<bigint> =>
  WindowKind.$match(spec.kind, {
    Tumbling: ({ size }) => [at - (at % BigInt(Math.trunc(Duration.toNanos(size))))],
    Sliding: ({ size, slide }) => {
      const span = BigInt(Math.trunc(Duration.toNanos(size)));
      const stride = BigInt(Math.trunc(Duration.toNanos(slide)));
      const first = at - (((at % stride) + stride) % stride);
      return Array.from({ length: Number((span + stride - 1n) / stride) }, (_, i) => first - BigInt(i) * stride)
        .filter((start) => at >= start && at < start + span);
    },
    Session: () => [at],
  });

interface WindowCell {
  readonly bucket: bigint;
  readonly count: bigint;
  readonly mark: Watermark;
}

const signOf = (kind: OpLogEntryWire["kind"]): bigint =>
  Match.value(kind).pipe(
    Match.when("delete", () => -1n),
    Match.when("upsert", () => 1n),
    Match.when("presence", () => 1n),
    Match.exhaustive,
  );

interface BucketRow {
  readonly bucket: bigint;
  readonly at: bigint;
  readonly kind: OpLogEntryWire["kind"];
}

const windowMerge =
  (spec: WindowSpec) =>
  (prior: Option.Option<WindowCell>, row: BucketRow): WindowCell => {
    const sign = signOf(row.kind);
    return Option.match(prior, {
      onNone: () => ({ bucket: row.bucket, count: sign, mark: advance(watermarkZero, row.at, false) }),
      onSome: (cell) => ({
        bucket: row.bucket,
        count: cell.count + sign,
        mark: advance(cell.mark, row.at, isLate(cell.mark, row.at, spec.allowedLateness)),
      }),
    });
  };

const windowFold = (
  changefeed: Stream.Stream<OpLogEntryWire>,
  spec: WindowSpec,
  policy: StreamPolicy,
): Effect.Effect<SubscriptionRef.SubscriptionRef<ReadonlyMap<bigint, WindowCell>>, never, Scope.Scope> =>
  keyedFold(
    changefeed.pipe(
      Stream.mapConcat((wire) => {
        const at = eventNanos(wire);
        return bucketSet(spec, at).map((bucket) => ({ bucket, at, kind: wire.kind }) satisfies BucketRow);
      }),
    ),
    (row) => row.bucket,
    windowMerge(spec),
    policy,
  );
```
