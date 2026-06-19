# [PROJECTION_SKEW_BAND]

`SkewBand` is the HLC skew-band confidence projection load-bearing in both render and ordering — `skewInterval` derives the correlation-keyed earliest/latest pair into a `{ midpointMs, radiusMs }` model surfaced as "within +/-N ms" product UI, and `bandsOverlap` lifts that interval into an ordering input so two rows whose midpoint distance falls within the summed `radiusMs` read as statistically indistinguishable rather than forced into a spurious HLC total order. This is the distributed-systems clock-uncertainty capability the C# desktop AppUi structurally cannot own: the browser dashboard renders honest interval confidence about event timing the host face has no read model to produce, and the convergence adjudication weighs concurrent-uncertain distinctly from definitively-ordered. The derivation reads the decode-admitted earliest/latest HLC instants the `evidence#EVIDENCE_CORRELATION` correlation cell carries; an HLC implementation that throws on excessive drift or counter overflow routes through the fault rail, never swallowed.

## [1]-[INDEX]

One cluster: `[2]-[SKEW_BAND]` owns `SkewBandWire`, `SkewInterval`, the `skewInterval` confidence-interval derivation, and the `bandsOverlap` ordering predicate.

## [2]-[SKEW_BAND]

- Owner: `SkewBandWire`, the earliest/latest pair shape; `skewInterval`, the derivation of the `{ midpointMs, radiusMs }` model — the pair midpoint as the best-estimate instant and half the band span as the symmetric uncertainty the dashboard renders directly as "within +/-`radiusMs` ms"; `bandsOverlap`, the symmetric predicate the ordering input reads — true exactly when the midpoint distance is within the summed radii so the two rows are concurrent-uncertain.
- Cases: `skewInterval` reads the correlation-keyed earliest and latest HLC instants and derives the center-plus-radius model so a dashboard renders "within +/-N ms" without recomputing the HLC fold or re-deriving the bound subtraction at the render site; `radiusMs` is the one place the `ui` domain surfaces clock-uncertainty as product UI; `bandsOverlap` is the one place the ordering domain reads the band, composed by `causality-graph/version-vector#VERSION_VECTOR` `skewVerdict` and the evidence timeline rather than re-derived as an inline midpoint comparison.
- Packages: `effect` for `Schema`.
- Growth: a new confidence projection lands as one `SkewBand` derivation arm; a new ordering posture lands as one predicate beside `bandsOverlap` over the same `SkewInterval`, never a parallel band shape.
- Boundary: the band reads decode-admitted HLC instants, never re-validated; the projection is load-bearing in both render and ordering — the render site reads `radiusMs` and the ordering site reads `bandsOverlap`, one interval owner serving both; the domain dials no transport.

```ts contract
import { Schema } from "effect";

export const SkewBandWire = Schema.Struct({ earliest: Schema.String, latest: Schema.String });
export type SkewBandWire = Schema.Schema.Type<typeof SkewBandWire>;

export interface SkewInterval {
  readonly midpointMs: number;
  readonly radiusMs: number;
}

export const skewInterval = (band: SkewBandWire): SkewInterval => {
  const lo = Date.parse(band.earliest);
  const hi = Date.parse(band.latest);
  return { midpointMs: (lo + hi) / 2, radiusMs: (hi - lo) / 2 };
};

export const bandsOverlap = (a: SkewInterval, b: SkewInterval): boolean =>
  Math.abs(a.midpointMs - b.midpointMs) <= a.radiusMs + b.radiusMs;
```
