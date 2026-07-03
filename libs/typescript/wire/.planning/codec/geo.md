# [WIRE_GEO]

`codec/geo.ts` decodes `GeoFeatureWire` from `Rasm.Bim/Semantics`: a geospatial feature whose geometry band is WKB octets held opaque at admission, whose spatial reference is an explicit SRID, and whose properties land as typed carriage. The WKB parse is its own gated row — `[R6]`: the parser identity (`wkx` 0.5.0 is the verified candidate, maintenance judgment open) rides a wire-declared port whose Layer lands when the judgment closes — and planar operations NEVER appear here: `@turf/turf` is `scope:viewer` and owns planar ops in `ui/viewer` `geo/layers` over the parsed features this rail publishes through `#vocab`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                              |
| :-----: | :--------------- | :------------------------------------------------------------------------ |
|   [1]   | `FEATURE_DECODE` | the `GeoFeature` owner with the opaque WKB band and the `WkbParser` port    |

## [2]-[FEATURE_DECODE]

- Owner: `GeoFeature` — one `Schema.Class`: SRID, the opaque `wkb` band (`Schema.Uint8ArrayFromSelf`, held verbatim), and the property record; `WkbParser`, the `Context.Tag` port whose one member parses held WKB into GeoJSON-shaped geometry values, declared here and satisfied by the `[R6]` Layer at the app root.
- Entry: `GeoFeature.FromBytes` composing the proto engine; `GeoFeature.decode(octets)` the one-shot rail; `GeoFeature.geometry(feature)` the port-requiring parse — `Effect<Geometry, WireFault, WkbParser>` — the only line that touches WKB content.
- Receipt: the parsed geometry is GeoJSON-shaped data the viewer's layers consume; the held band remains the identity material — byte-stable, forwardable, hashable through the kernel mint without a re-encode.
- Growth: a new property axis is a C# field mirrored here; a new geometry kind is the parser's concern, not the carrier's — the band stays opaque regardless.
- Law: `[R6]` gates the parser, not the carrier — the feature decodes, holds, and forwards today; `geometry` is the one surface that suspends on the port, so the research row blocks exactly the parse and nothing upstream.
- Law: the band is held opaque under the byte-identity law — parse-then-reserialize of signed or content-keyed material respells float forms and is rejected; forwarding emits the held octets verbatim.
- Law: planar ops are fenced — buffer, union, intersect, area live in `ui/viewer` `geo/layers` on `@turf/turf`; a planar computation in `wire` violates the census fence and the viewer's `scope:viewer` admission both.
- Law: SRID is explicit carriage — the consumer decides projection handling; a silent re-projection here would be a geometry operation (invariant 7).
- Boundary: the parser Layer's package identity closes with `[R6]`; viewer layer composition is `ui/viewer` `geo/layers`; the census fences `GeoFeatureWire` to this page.

```typescript
import { Context, Effect, type ParseResult, Schema } from "effect"
import type { WireFault } from "../fault/quarantine.ts"
import { ProtoCodec } from "./proto.ts"

const _Position = Schema.Tuple(Schema.Number, Schema.Number)

const _Geometry = Schema.Union(
  Schema.TaggedStruct("Point", { coordinates: _Position }),
  Schema.TaggedStruct("LineString", { coordinates: Schema.Array(_Position) }),
  Schema.TaggedStruct("Polygon", { coordinates: Schema.Array(Schema.Array(_Position)) }),
  Schema.TaggedStruct("MultiPolygon", { coordinates: Schema.Array(Schema.Array(Schema.Array(_Position))) }),
)

class GeoFeature extends Schema.Class<GeoFeature>("GeoFeature")({
  key: Schema.NonEmptyString,
  srid: Schema.Int.pipe(Schema.positive()),
  wkb: Schema.Uint8ArrayFromSelf,
  properties: Schema.Record({ key: Schema.String, value: Schema.Unknown }),
}) {
  static readonly Geometry: typeof _Geometry = _Geometry
  static readonly FromBytes: Schema.Schema<GeoFeature, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.GeoFeatureWire, GeoFeature)
  static readonly decode: (octets: Uint8Array) => Effect.Effect<GeoFeature, ParseResult.ParseError> = Schema.decodeUnknown(GeoFeature.FromBytes)
  static readonly geometry = (feature: GeoFeature): Effect.Effect<GeoFeature.Geometry, WireFault, WkbParser> =>
    Effect.flatMap(WkbParser, (parser) => parser.parse(feature.wkb, feature.srid))
}

declare namespace GeoFeature {
  type Geometry = Schema.Schema.Type<typeof _Geometry>
}

class WkbParser extends Context.Tag("wire/WkbParser")<WkbParser, {
  readonly parse: (wkb: Uint8Array, srid: number) => Effect.Effect<GeoFeature.Geometry, WireFault>
}>() {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { GeoFeature, WkbParser }
```
