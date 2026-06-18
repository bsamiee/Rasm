# [PROJECTION_SKEW_BAND]

`SkewBand` is the HLC skew-band confidence projection — `skewInterval` derives the correlation-keyed earliest/latest pair into a `{ midpointMs, radiusMs }` render model surfaced as "within +/-N ms" product UI. This is the distributed-systems clock-uncertainty capability the C# desktop AppUi structurally cannot own: the browser dashboard renders honest interval confidence about event timing the host face has no read model to produce. The derivation reads the decode-admitted earliest/latest HLC instants the `evidence#EVIDENCE_CORRELATION` correlation cell carries; an HLC implementation that throws on excessive drift or counter overflow routes through the fault rail, never swallowed.

## [1]-[INDEX]

One cluster: `[2]-[SKEW_BAND]` owns `SkewBandWire`, `SkewInterval`, and the `skewInterval` confidence-interval derivation.

## [2]-[SKEW_BAND]

- Owner: `SkewBandWire`, the earliest/latest pair shape; `skewInterval`, the derivation of the `{ midpointMs, radiusMs }` render model — the pair midpoint as the best-estimate instant and half the band span as the symmetric uncertainty the dashboard renders directly as "within +/-`radiusMs` ms".
- Cases: `skewInterval` reads the correlation-keyed earliest and latest HLC instants and derives the center-plus-radius render model so a dashboard renders "within +/-N ms" without recomputing the HLC fold or re-deriving the bound subtraction at the render site; `radiusMs` is the one place the `ui` domain surfaces clock-uncertainty as product UI.
- Packages: `effect` for `Schema`.
- Growth: a new confidence projection lands as one `SkewBand` derivation arm; promoting the band into an ordering input (two rows whose midpoint-distance is within the summed `radiusMs` marked concurrent-uncertain) lands as one additional derivation feeding `convergence/lww-merge#LWW_MERGE` and the evidence timeline.
- Boundary: the band reads decode-admitted HLC instants, never re-validated; the projection is render-only today and becomes load-bearing in ordering once skew-aware ordering lands; the domain dials no transport.

```ts contract
import { Schema } from "effect";

const SkewBandWire = Schema.Struct({ earliest: Schema.String, latest: Schema.String });
type SkewBandWire = Schema.Schema.Type<typeof SkewBandWire>;

interface SkewInterval {
  readonly midpointMs: number;
  readonly radiusMs: number;
}

const skewInterval = (band: SkewBandWire): SkewInterval => {
  const lo = Date.parse(band.earliest);
  const hi = Date.parse(band.latest);
  return { midpointMs: (lo + hi) / 2, radiusMs: (hi - lo) / 2 };
};
```
