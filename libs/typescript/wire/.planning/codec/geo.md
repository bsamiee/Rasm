# [WIRE_GEO]

`codec/geo.ts` decodes `GeoFeatureWire` from `Rasm.Bim/Semantics` and owns the branch geospatial vocabulary: a geospatial feature whose geometry band is WKB octets held opaque at admission, whose spatial reference is an explicit SRID, and whose properties land as typed carriage — plus the decoded-value shapes every geo consumer types against through `#vocab`: the exhaustive `Geometry` union, the `Extent` bounds tuple, the `Crs` reference rows, and the `Tile` grid coordinate with its pure integer addressing. The WKB parse is its own gated row — `[R6]`: the parser identity (`wkx` 0.5.0 is the verified candidate, maintenance judgment open) rides a wire-declared port whose Layer lands when the judgment closes — and planar operations NEVER appear here: `@turf/turf` is `scope:viewer` and owns planar ops in `ui/viewer` `geo/layers` over the parsed features this rail publishes through `#vocab`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                       |
| :-----: | :--------------- | :----------------------------------------------------------------------------- |
|   [1]   | `GEO_VOCABULARY` | the exhaustive geometry union, extent bounds, CRS rows, tile-grid addressing    |
|   [2]   | `FEATURE_DECODE` | the `GeoFeature` owner with the opaque WKB band and the `WkbParser` port        |

## [2]-[GEO_VOCABULARY]

- Owner: the geospatial decoded-value vocabulary — the interior anchors `_Geometry` (the closed seven-kind union over 2D/3D positions), `_Extent` (the `[west, south, east, north]` bounds tuple), `_CRS` (the well-known reference rows over the open SRID axis), and `_Tile` (the `{zoom, x, y}` grid coordinate with quadkey, parent, and children addressing) — every one reaching consumers only as a `GeoFeature` static plus its merged-namespace type, so one `#vocab` import carries the feature, the parse port, and every shape a geo consumer types against.
- Law: the geometry union is exhaustive over the closed WKB kind set (Point through GeometryCollection, kinds 1-7) — every kind the parser can emit has a landing case, dispatch sites close with `Match.exhaustive` or record totality, and a parser emitting an unlisted kind is a `WireFault` at the port, never a silent widening; Z-carrying variants land in the position's optional third slot, and the parser projects a WKB M ordinate away because measure is not position.
- Law: `Extent` is bounds carriage, not computation — `west > east` is the legal antimeridian crossing, so no order refinement exists; DERIVING an extent from a geometry is a planar fold fenced to `ui/viewer` `geo/layers` (`bbox`), and the camera fit that consumes one is `ui/viewer` `geo/project`'s `FitBounds` intent.
- Law: `Crs.of(srid)` resolves the well-known rows — `kind` (`geographic` | `projected`) and `unit` (`degree` | `metre`) are the columns projection handling dispatches on: a `geographic` feature feeds the map directly, a `projected` one crosses through the viewer's mercator rows once at the boundary — and an unlisted SRID answers `Option.none`, the explicit no-known-handling verdict a consumer surfaces as evidence; a new well-known system is one row.
- Law: `Tile` addressing is integer arithmetic only — `quadkey` interleaves the x/y bits per zoom level and the zoom-0 root answers the empty key, `parent`/`children` shift the grid with both boundary answers explicit (no parent above the root, no children below the depth ceiling — the refinement's own bound, never a throw), and the refinement pins `x, y < 2^zoom` at admission; tile-to-extent conversion is inverse web-mercator projection and therefore the viewer's (`WebMercatorViewport` at `ui/viewer` `geo/project`), never this page's.
- Growth: a new geometry kind is one union member plus its hand-stated arm when the WKB spec grows; a new CRS is one `_CRS` row; a new tile read (a sibling test, a zoom clamp) is one member on the tile owner.
- Boundary: `ui/viewer` `geo/layers` streams tiles through these coordinates and renders these geometries; `ui/viewer` `geo/project` owns every projection crossing; feature identity stays the held band under the byte-identity law.

```typescript
import { Array, Context, Effect, Option, type ParseResult, Schema } from "effect"
import type { WireFault } from "../fault/quarantine.ts"
import { ProtoCodec } from "./proto.ts"

const _Position = Schema.Tuple(Schema.Number, Schema.Number, Schema.optionalElement(Schema.Number)) // [lon, lat, altitude?] — the third slot is carriage, never re-projected here

const _Point = Schema.TaggedStruct("Point", { coordinates: _Position })
const _MultiPoint = Schema.TaggedStruct("MultiPoint", { coordinates: Schema.Array(_Position) })
const _LineString = Schema.TaggedStruct("LineString", { coordinates: Schema.Array(_Position) })
const _MultiLineString = Schema.TaggedStruct("MultiLineString", { coordinates: Schema.Array(Schema.Array(_Position)) })
const _Polygon = Schema.TaggedStruct("Polygon", { coordinates: Schema.Array(Schema.Array(_Position)) })
const _MultiPolygon = Schema.TaggedStruct("MultiPolygon", { coordinates: Schema.Array(Schema.Array(Schema.Array(_Position))) })
const _Collection = Schema.TaggedStruct("GeometryCollection", {
  geometries: Schema.Array(Schema.suspend((): Schema.Schema<GeoFeature.Geometry> => _Geometry)), // the sanctioned suspend twin: the recursive member is the one hand-stated arm
})

const _Geometry = Schema.Union(_Point, _MultiPoint, _LineString, _MultiLineString, _Polygon, _MultiPolygon, _Collection)

const _Extent = Schema.Tuple(Schema.Number, Schema.Number, Schema.Number, Schema.Number) // [west, south, east, north]

const _CRS = {
  4326: { kind: "geographic", unit: "degree" },
  3857: { kind: "projected", unit: "metre" },
  4979: { kind: "geographic", unit: "degree" }, // 3D WGS84: the altitude slot carries ellipsoidal height
} as const

const _CrsOwner: {
  readonly rows: typeof _CRS
  readonly of: (srid: number) => Option.Option<GeoFeature.Crs>
} = {
  rows: _CRS,
  of: (srid) => (srid in _CRS ? Option.some(_CRS[srid as GeoFeature.Srid]) : Option.none()), // BOUNDARY ADAPTER: the in-probe is the evidence the checker cannot carry onto the key
}

const _ZOOM_CEILING = 30 // web-mercator grid depth: 2^30 tiles per axis is the deepest addressable rank

const _Tile = Schema.Struct({
  zoom: Schema.Int.pipe(Schema.between(0, _ZOOM_CEILING)),
  x: Schema.Int.pipe(Schema.nonNegative()),
  y: Schema.Int.pipe(Schema.nonNegative()),
}).pipe(Schema.filter((tile) => tile.x < 2 ** tile.zoom && tile.y < 2 ** tile.zoom, { identifier: "TileInGrid" }))

const _TileOwner: {
  readonly schema: typeof _Tile
  readonly quadkey: (tile: GeoFeature.Tile) => string
  readonly parent: (tile: GeoFeature.Tile) => Option.Option<GeoFeature.Tile>
  readonly children: (tile: GeoFeature.Tile) => ReadonlyArray<GeoFeature.Tile>
} = {
  schema: _Tile,
  quadkey: (tile) =>
    tile.zoom === 0
      ? "" // the root's key is empty by the quadkey grammar; Array.makeBy floors its count at one, so the guard answers, never the fold
      : Array.join(
          Array.makeBy(tile.zoom, (rank) => {
            const bit = tile.zoom - rank - 1
            return String((((tile.y >> bit) & 1) << 1) | ((tile.x >> bit) & 1))
          }),
          "",
        ),
  parent: (tile) =>
    tile.zoom === 0
      ? Option.none()
      : Option.some(_Tile.make({ zoom: tile.zoom - 1, x: tile.x >> 1, y: tile.y >> 1 })),
  children: (tile) =>
    tile.zoom === _ZOOM_CEILING
      ? [] // the ceiling has no finer rank: an empty family is the boundary answer, a zoom-31 mint would breach the refinement
      : Array.map(
          [[0, 0], [1, 0], [0, 1], [1, 1]] as const,
          ([dx, dy]) => _Tile.make({ zoom: tile.zoom + 1, x: tile.x * 2 + dx, y: tile.y * 2 + dy }),
        ),
}
```

## [3]-[FEATURE_DECODE]

- Owner: `GeoFeature` — one `Schema.Class`: SRID, the opaque `wkb` band (`Schema.Uint8ArrayFromSelf`, held verbatim), and the property record; the `[2]` vocabulary rides it as statics; `WkbParser`, the `Context.Tag` port whose one member parses held WKB into the geometry union, declared here and satisfied by the `[R6]` Layer at the app root.
- Entry: `GeoFeature.FromBytes` composing the proto engine; `GeoFeature.decode(octets)` the one-shot rail; `GeoFeature.geometry(feature)` the port-requiring parse — `Effect<Geometry, WireFault, WkbParser>` — the only line that touches WKB content.
- Receipt: the parsed geometry is GeoJSON-shaped data the viewer's layers consume; the held band remains the identity material — byte-stable, forwardable, hashable through the kernel mint without a re-encode.
- Growth: a new property axis is a C# field mirrored here; the geometry union is closed at the WKB kind set, so parser growth lands as `[2]` union coverage, never a carrier change — the band stays opaque regardless.
- Law: `[R6]` gates the parser, not the carrier — the feature decodes, holds, and forwards today; `geometry` is the one surface that suspends on the port, so the research row blocks exactly the parse and nothing upstream.
- Law: the band is held opaque under the byte-identity law — parse-then-reserialize of signed or content-keyed material respells float forms and is rejected; forwarding emits the held octets verbatim.
- Law: planar ops are fenced — buffer, union, intersect, area live in `ui/viewer` `geo/layers` on `@turf/turf`; a planar computation in `wire` violates the census fence and the viewer's `scope:viewer` admission both.
- Law: SRID is explicit carriage — the consumer decides projection handling by dispatching on the `Crs` row the SRID resolves; a silent re-projection here would be a geometry operation (invariant 7).
- Boundary: the parser Layer's package identity closes with `[R6]`; viewer layer composition is `ui/viewer` `geo/layers`; the census fences `GeoFeatureWire` to this page.

```typescript
class GeoFeature extends Schema.Class<GeoFeature>("GeoFeature")({
  key: Schema.NonEmptyString,
  srid: Schema.Int.pipe(Schema.positive()),
  wkb: Schema.Uint8ArrayFromSelf,
  properties: Schema.Record({ key: Schema.String, value: Schema.Unknown }),
}) {
  static readonly Geometry: typeof _Geometry = _Geometry
  static readonly Extent: typeof _Extent = _Extent
  static readonly Crs: typeof _CrsOwner = _CrsOwner
  static readonly Tile: typeof _TileOwner = _TileOwner
  static readonly FromBytes: Schema.Schema<GeoFeature, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.GeoFeatureWire, GeoFeature)
  static readonly decode: (octets: Uint8Array) => Effect.Effect<GeoFeature, ParseResult.ParseError> = Schema.decodeUnknown(GeoFeature.FromBytes)
  static readonly geometry = (feature: GeoFeature): Effect.Effect<GeoFeature.Geometry, WireFault, WkbParser> =>
    Effect.flatMap(WkbParser, (parser) => parser.parse(feature.wkb, feature.srid))
}

declare namespace GeoFeature {
  type Position = typeof _Position.Type
  type Geometry =
    | typeof _Point.Type
    | typeof _MultiPoint.Type
    | typeof _LineString.Type
    | typeof _MultiLineString.Type
    | typeof _Polygon.Type
    | typeof _MultiPolygon.Type
    | { readonly _tag: "GeometryCollection"; readonly geometries: ReadonlyArray<Geometry> }
  type Extent = typeof _Extent.Type
  type Srid = keyof typeof _CRS
  type Crs = (typeof _CRS)[Srid]
  type Tile = typeof _Tile.Type
}

class WkbParser extends Context.Tag("wire/WkbParser")<WkbParser, {
  readonly parse: (wkb: Uint8Array, srid: number) => Effect.Effect<GeoFeature.Geometry, WireFault>
}>() {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { GeoFeature, WkbParser }
```
