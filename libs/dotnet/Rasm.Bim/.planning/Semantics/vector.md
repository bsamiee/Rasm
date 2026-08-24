# [BIM_GEOVECTOR]

`GeoVector` is the universal vector ingest-and-egress fold: one `GeoVectorSource` `[SmartEnum<string>]` table whose rows carry the `decode`/`encode` codec-pair columns, dedicated managed codecs (shapefile, GeoJSON, CityJSON ingest-only by row law, FlatGeobuf, GeoParquet, KML/KMZ, MVT) and the `MaxRev.Gdal.Core` OGR universal reader for the long tail, every arm producing the canonical `Semantics/feature#GEO_FEATURE` row. `GeoKml` is the MANAGED KML/KMZ codec and styled-presentation emit over `SharpKml.Core` — the GDAL OGR `KML` driver is the rejected style-and-extended-data-losing form.

Both codec columns ride the `Fin` rail, so a row's own decode refuses by name instead of throwing into an enclosing trap, and both take one `GeoWindow` carrying the spatial clip beside the optional attribute filter — a managed codec has no attribute engine, so it REFUSES a filter it cannot push down rather than returning rows the caller believes were filtered. Every OGR field crosses at its declared `FieldType` onto the `GeoFeature.Typed` seam vocabulary, never flattened to text; the `GeoWkb` bridge on `Semantics/feature#GEO_BOUNDARY` is the ONE OGR↔NTS crossing, and every `/vsimem` leg runs inside the one `GeoGdal` bracket that page owns.

## [01]-[INDEX]

- [02]-[VECTOR_SOURCE]: `GeoVectorSource` the format table with its railed codec pair, `GeoWindow`/`AttributeFilter` the push-down request every arm reads.
- [03]-[KML_CODEC]: `KmlElevation` the altitude-posture roster, `OrthoDrape` the ortho carrier, `GeoKml` the managed KML/KMZ codec and styled site emit.
- [04]-[VECTOR_FOLD]: `GeoVector` the ingest and egress arms — managed codecs, the remote-`.fgb` range read, the typed OGR field crossing, and the symmetric egress.

## [02]-[VECTOR_SOURCE]

- Owner: `GeoVectorSource` the `[SmartEnum<string>]` format table whose rows carry the railed `decode`/`encode` codec pair; `AttributeFilter` the admitted non-blank OGR-SQL restriction; `GeoWindow` the ONE push-down request every decode arm reads.
- Law: the encode column is OPTIONAL because ingest-only is a real row state, not an error state — a planar `GeoFeature` set cannot re-emit a 3D city model, so `CityJson` declares NO encoder and the absence rails typed at `Write`; a throwing delegate in a policy row makes an absent capability indistinguishable from a broken one and reads as support to anyone scanning the roster.
- Entry: `Decode(bytes, window, key)` and `Encoder` are the row's two columns; `GeoWindow.Whole` is the unfiltered read and `GeoWindow.At(clip)` the spatial window; `AttributeFilter.Of(restriction, key)` admits an OGR-SQL `WHERE` body.
- Packages: `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new vector format is one row carrying its `decode`/`encode` pair, an OGR row closing over its driver token, with zero entry-point edits; a new push-down axis is one `GeoWindow` column every arm already receives; never a per-format importer family and never a boolean op on the OGR side.
- Boundary: the row's delegate columns route decode AND encode with no call-site branch, so a call-site if-ladder over formats is the deleted form; the managed shapefile/FlatGeobuf/GeoParquet/KML codecs are the pure-managed defaults and admitting GDAL for a format a managed codec reads is the rejected form; a `managed` column beside the delegate restated which delegate the row already binds and no fence ever read it, so the managed/OGR partition is stated at this boundary and carried by `Exchange/format#FORMAT_AXIS`; an attribute filter reaches only a row whose codec can push it down, because a filter silently dropped returns a superset the caller cannot detect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers.Binary;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using FlatGeobuf.Index;
using GISBlox.IO.GeoParquet.Extensions;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Element.Geospatial;
using Rasm.Element.Projection;                     // TextureWrap — the seam sampler-addressing roster the CityJSON wrap mode lands on
using SharpKml.Base;
using SharpKml.Dom.GX;
using SharpKml.Engine;
using Thinktecture;
using static LanguageExt.Prelude;
using KmlDom = SharpKml.Dom;   // aliased — SharpKml.Dom.Feature/Geometry/Point/Polygon would shadow the NTS vocabulary

namespace Rasm.Bim;

// --- [TYPES] --------------------------------------------------------------------------------
// The OGR-SQL restriction as an ADMITTED value: a blank filter is not "no filter" to OGR, it is a parse error at the
// layer, so absence rides the Option and presence is a non-blank body.
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AttributeFilter {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? "";
        if (value.Length == 0) { validationError = new ValidationError("attribute-filter-blank"); }
    }

    public static Fin<AttributeFilter> Of(string restriction, Op key) =>
        Validate(restriction, out AttributeFilter? filter) is null && filter is { } admitted
            ? Fin.Succ(admitted)
            : Fin.Fail<AttributeFilter>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "geo-format-lane", "vector", "attribute-filter", restriction })));
}

// ONE push-down request every decode arm reads: the spatial clip a shapefile MbrFilter, a FlatGeobuf
// Packed-Hilbert-R-tree, or an OGR SetSpatialFilterRect answers server-side, and the attribute restriction only an
// OGR layer can. Carrying them together is what lets a managed arm REFUSE the half it cannot serve instead of
// answering half the question.
public sealed record GeoWindow(Option<Envelope> Clip, Option<AttributeFilter> Where) {
    public static readonly GeoWindow Whole = new(None, None);

    public static GeoWindow At(Envelope clip) => new(Some(clip), None);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class GeoVectorSource {
    public static readonly GeoVectorSource Shapefile  = new("shapefile",
        decode: static (bytes, window, key) => GeoVector.Shapefile(bytes, window, key),
        encode: Some<Func<Seq<GeoFeature>, Option<ProjectedCrs>, Op, Fin<byte[]>>>(static (features, crs, key) => GeoVector.WriteShapefile(features, crs, key)));
    public static readonly GeoVectorSource GeoJson    = new("geojson",
        decode: static (bytes, window, key) => GeoVector.GeoJson(bytes, window, key),
        encode: Some<Func<Seq<GeoFeature>, Option<ProjectedCrs>, Op, Fin<byte[]>>>(static (features, _, key) => GeoVector.WriteGeoJson(features, key)));
    public static readonly GeoVectorSource CityJson   = new("cityjson",
        decode: static (bytes, window, key) => GeoVector.CityJson(bytes, window, key),
        encode: None);
    public static readonly GeoVectorSource FlatGeobuf = new("flatgeobuf",
        decode: static (bytes, window, key) => GeoVector.FlatGeobuf(bytes, window, key),
        encode: Some<Func<Seq<GeoFeature>, Option<ProjectedCrs>, Op, Fin<byte[]>>>(static (features, _, key) => GeoVector.WriteFlatGeobuf(features, key)));
    public static readonly GeoVectorSource GeoParquet = new("geoparquet",
        decode: static (bytes, window, key) => GeoVector.GeoParquet(bytes, window, key),
        encode: Some<Func<Seq<GeoFeature>, Option<ProjectedCrs>, Op, Fin<byte[]>>>(static (features, _, key) => GeoVector.WriteGeoParquet(features, key)));
    public static readonly GeoVectorSource Kml        = new("kml",
        decode: static (bytes, window, key) => GeoKml.Read(bytes, window, key),
        encode: Some<Func<Seq<GeoFeature>, Option<ProjectedCrs>, Op, Fin<byte[]>>>(static (features, _, key) => GeoKml.Write(features, key)));
    // KMZ shares the zip-discriminating GeoKml.Read (the kmz magic routes KmzFile.Open internally); its encode is a
    // symmetric bare-KMZ zip of the SAME document build — format#FORMAT_AXIS Kmz capability columns are arm-backed here.
    public static readonly GeoVectorSource Kmz        = new("kmz",
        decode: static (bytes, window, key) => GeoKml.Read(bytes, window, key),
        encode: Some<Func<Seq<GeoFeature>, Option<ProjectedCrs>, Op, Fin<byte[]>>>(static (features, _, key) => GeoKml.WriteKmz(features, key)));
    // Single-tile MVT byte codec anchored at the world tile (0,0,0) — MVT bytes carry only tile-local integer
    // coordinates, so the bare-bytes row pins the one self-describing anchor; the multi-tile pyramid + TileJSON
    // catalog stay Semantics/model#TILE_PYRAMID.
    public static readonly GeoVectorSource Mvt        = new("mvt",
        decode: static (bytes, window, key) => GeoTiles.Decode(bytes, 0, 0, 0, key).Map(static rows => rows.Map(static r => r.Feature)),
        encode: Some<Func<Seq<GeoFeature>, Option<ProjectedCrs>, Op, Fin<byte[]>>>(static (features, _, key) => GeoTiles.EncodeWorldTile(features, key)));
    public static readonly GeoVectorSource GeoPackage = new("geopackage",
        decode: static (bytes, window, key) => GeoVector.Universal(bytes, window, key),
        encode: Some<Func<Seq<GeoFeature>, Option<ProjectedCrs>, Op, Fin<byte[]>>>(static (features, crs, key) => GeoVector.WriteUniversal("GPKG", features, crs, key)));
    public static readonly GeoVectorSource Gml        = new("gml",
        decode: static (bytes, window, key) => GeoVector.Universal(bytes, window, key),
        encode: Some<Func<Seq<GeoFeature>, Option<ProjectedCrs>, Op, Fin<byte[]>>>(static (features, crs, key) => GeoVector.WriteUniversal("GML", features, crs, key)));
    public static readonly GeoVectorSource FileGdb    = new("filegdb",
        decode: static (bytes, window, key) => GeoVector.Universal(bytes, window, key),
        encode: Some<Func<Seq<GeoFeature>, Option<ProjectedCrs>, Op, Fin<byte[]>>>(static (features, crs, key) => GeoVector.WriteUniversal("OpenFileGDB", features, crs, key)));

    [UseDelegateFromConstructor]
    public partial Fin<Seq<GeoFeature>> Decode(ReadOnlyMemory<byte> bytes, GeoWindow window, Op key);

    public Option<Func<Seq<GeoFeature>, Option<ProjectedCrs>, Op, Fin<byte[]>>> Encoder { get; }
}
```

## [03]-[KML_CODEC]

- Owner: `KmlElevation` the closed altitude-posture roster every raised KML geometry reads; `OrthoDrape` the ortho a site KMZ carries; `GeoKml` the managed KML/KMZ codec and the styled-presentation `Site` emit.
- Cases: `OrthoDrape` arms `Boxed` (the OGC-portable `LatLonBox` for a north-up overlay) and `Quad` (the four georeferenced corners in KML's own `gx:LatLonQuad` counter-clockwise order, which the raster geo-transform corner fold already produces).
- Law: KML's own default CLAMPS every coordinate to terrain and IGNORES its altitude, so a pipeline that preserved Z end to end — stride-3 reprojection buffers, `GeoWkb`'s `Ordinates.XYZ` against `AsBinary`'s 2D default — hands this bridge real elevations and a bare `Placemark` discards them twice: once by writing a two-ordinate `Vector`, once by leaving the mode unset; every raise stamps a roster row.
- Entry: `GeoKml.Read(bytes, window, key)` decodes bare KML or KMZ off the zip magic, `Write`/`WriteKmz` emit the ONE unstyled document build, and `Site(features, styleOf, elevationOf, styles, ortho, tour, key)` is the composed styled-KMZ presentation entry owning its `Wgs84` reprojection.
- Auto: `Tessellates` is DERIVED off the row's mode rather than declared, because tessellation means "drape this line over the terrain surface" and an absolute or relative geometry must not do it — a hard-set `Tessellate` re-flattened every elevated footprint onto the ground; the styled emit lands styles ONCE as shared `Document` styles the placemarks reference by `#id`.
- Packages: `SharpKml.Core`, `NetTopologySuite`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new altitude posture is one `KmlElevation` row carrying its mode and extrude; a new symbology is one `Site` styles row and its routing, never a second emit path; a new overlay carrier is one `OrthoDrape` case.
- Boundary: SharpKml carries its OWN geographic geometry (`Vector` is `(lat, lon[, alt])`, the INVERSE of an NTS `Coordinate(X=lon, Y=lat)`), so the bridge swaps ordinates in both directions and a cast is unrepresentable; KML is geographic by definition, so every decoded row tags the structurally-admitting EPSG:4326 source CRS and the styled emit reprojects first; the absent KML altitude admits as the NTS `Coordinate.NullOrdinate` the whole planar vocabulary already spells absence with, so the write-side finiteness test reads one convention rather than a local sentinel; `Write` and `WriteKmz` differ ONLY by the `KmzFile.Create` wrap over an IDENTICAL document, so the build is ONE owner and the two emitters carry the wrap alone — a `zipped` bool selecting two bodies is the deleted form; the ortho's case is the PRODUCER's choice because only the raster leg holds the affine whose two rotation terms decide it, and an axis-aligned box discards them, mislanding a rotated aerial by the rotation angle with no diagnostic; a strict OGC-KML reader ignores the `gx` extension, so an overlay written only as a quad drapes nowhere at all; the `Site` KMZ is a DELIVERY projection, never the cross-runtime peer wire.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
// Absolute is the vertical-datum reading a surveyed terrain or a flood surface needs, RelativeToGround the offset a
// canopy or a mast takes, ClampToGround the drape a landuse polygon wants — and extrusion is orthogonal to all three
// (a mast extrudes to ground, a canopy does not), so the roster names the four postures that exist rather than
// handing every raise a mode argument beside a loose bool.
[SmartEnum<string>]
public sealed partial class KmlElevation {
    public static readonly KmlElevation Draped = new("draped", AltitudeMode.ClampToGround, extrudes: false);
    public static readonly KmlElevation Massed = new("massed", AltitudeMode.Absolute, extrudes: true);
    public static readonly KmlElevation Floated = new("floated", AltitudeMode.RelativeToGround, extrudes: false);
    public static readonly KmlElevation Masted = new("masted", AltitudeMode.RelativeToGround, extrudes: true);

    public AltitudeMode Mode { get; }

    public bool Extrudes { get; }

    // Tessellation follows the CLAMP: a clamped coordinate has no height to drape from and an absolute one must not
    // be re-flattened onto terrain.
    public bool Tessellates => Mode == AltitudeMode.ClampToGround;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OrthoDrape {
    private OrthoDrape() { }
    public sealed record Boxed(byte[] Png, Envelope Bounds) : OrthoDrape;
    public sealed record Quad(byte[] Png, Coordinate LowerLeft, Coordinate LowerRight, Coordinate UpperRight, Coordinate UpperLeft) : OrthoDrape;
}

// --- [BOUNDARIES] ---------------------------------------------------------------------------
public static class GeoKml {
    const string OrthoEntry = "files/ortho.png";

    // KMZ discriminates on the zip magic; KmzFile.GetDefaultKmlFile resolves the doc.kml. Containers walk
    // recursively, each Placemark lowering to one GeoFeature: geometry through the lat/lon-swapped bridge,
    // name/id/description with the ExtendedData Data rows and SchemaData SimpleData rows onto the attribute bag.
    internal static Fin<Seq<GeoFeature>> Read(ReadOnlyMemory<byte> bytes, GeoWindow window, Op key) =>
        GeoVector.Planar(window, "kml", key, () => {
            using var stream = new MemoryStream(bytes.ToArray());
            KmlFile file;
            if (bytes.Span is [0x50, 0x4B, ..]) { using var kmz = KmzFile.Open(stream); file = kmz.GetDefaultKmlFile(); }
            else { file = KmlFile.Load(stream); }
            Option<ProjectedCrs> crs = ProjectedCrs.Of("EPSG:4326", "", "", "", key)
                .Match(Succ: static c => Some(c), Fail: static _ => Option<ProjectedCrs>.None);
            return Optional(file.Root switch { KmlDom.Kml k => k.Feature, KmlDom.Feature f => f, _ => null })
                .Match(Some: root => Walk(root, crs), None: static () => Seq<GeoFeature>());
        });

    static Seq<GeoFeature> Walk(KmlDom.Feature feature, Option<ProjectedCrs> crs) => feature switch {
        KmlDom.Container container => container.Features.AsIterable().Bind(child => Walk(child, crs)).ToSeq(),
        KmlDom.Placemark mark => Lower(mark.Geometry).Map(geometry => new GeoFeature(geometry, Attributes(mark), crs)).ToSeq(),
        _ => Seq<GeoFeature>(),
    };

    // KML DOM -> NTS: Point/LineString/LinearRing/Polygon(+holes)/MultipleGeometry, each Vector swapped onto
    // Coordinate(X=Longitude, Y=Latitude); an open KML ring closes before CreateLinearRing admits it.
    static Option<Geometry> Lower(KmlDom.Geometry? geometry) => geometry switch {
        KmlDom.Point p when p.Coordinate is { } v => Some((Geometry)GeoServices.Factory.CreatePoint(Coord(v))),
        KmlDom.LineString l => Some((Geometry)GeoServices.Factory.CreateLineString(l.Coordinates.AsIterable().Map(Coord).ToArray())),
        KmlDom.LinearRing r => Some((Geometry)GeoServices.Factory.CreatePolygon(Ring(r))),
        KmlDom.Polygon poly when poly.OuterBoundary?.LinearRing is { } shell =>
            Some((Geometry)GeoServices.Factory.CreatePolygon(
                Ring(shell),
                poly.InnerBoundary.Select(static h => h.LinearRing).OfType<KmlDom.LinearRing>().Select(Ring).ToArray())),
        KmlDom.MultipleGeometry multi =>
            Some((Geometry)GeoServices.Factory.CreateGeometryCollection(
                toSeq(multi.Geometry).Choose(Lower).ToArray())),
        _ => None,
    };

    // NTS spells an absent ordinate as Coordinate.NullOrdinate, so the KML absence lands on the vocabulary the whole
    // planar stack already reads rather than a package-local sentinel the write path must re-learn.
    static Coordinate Coord(Vector v) => new(v.Longitude, v.Latitude, v.Altitude ?? Coordinate.NullOrdinate);

    static LinearRing Ring(KmlDom.LinearRing ring) {
        Coordinate[] shell = ring.Coordinates.AsIterable().Map(Coord).ToArray();
        return GeoServices.Factory.CreateLinearRing(
            shell.Length >= 3 && !shell[0].Equals2D(shell[^1]) ? [.. shell, shell[0]] : shell);
    }

    // Indexer sets, never Add: a KML Data row legitimately named "name"/"id" must overwrite, not throw. The four
    // SharpKml nullable reads are ONE admission at ONE boundary, which is why they coalesce here and nowhere else.
    static IAttributesTable Attributes(KmlDom.Placemark mark) {
        var table = new AttributesTable { ["name"] = mark.Name ?? "", ["id"] = mark.Id ?? "" };
        if (mark.Description?.Text is { Length: > 0 } text) { table["description"] = text; }
        mark.ExtendedData?.Data.AsIterable().Iter(d => { if (d.Name is { Length: > 0 } name) { table[name] = d.Value ?? ""; } });
        mark.ExtendedData?.SchemaData.AsIterable()
            .Bind(static s => s.SimpleData.AsIterable())
            .Iter(d => { if (d.Name is { Length: > 0 } name) { table[name] = d.Text ?? ""; } });
        return table;
    }

    // ONE document build both bare emitters wrap: the unstyled Placemark set with its ExtendedData. The styled
    // ortho/tour presentation is Site's and is never re-spelled here.
    static KmlDom.Document Build(Seq<GeoFeature> features) {
        var document = new KmlDom.Document { Name = "features" };
        features.Iter(f => document.AddFeature(Mark(f, Option<string>.None, KmlElevation.Draped)));
        return document;
    }

    internal static Fin<byte[]> Write(Seq<GeoFeature> features, Op key) =>
        key.Catch(() => {
            using var output = new MemoryStream();
            KmlFile.Create(new KmlDom.Kml { Feature = Build(features) }, duplicates: false).Save(output);
            return output.ToArray();
        });

    internal static Fin<byte[]> WriteKmz(Seq<GeoFeature> features, Op key) =>
        key.Catch(() => {
            using var kmz = KmzFile.Create(KmlFile.Create(new KmlDom.Kml { Feature = Build(features) }, duplicates: false));
            using var output = new MemoryStream();
            kmz.Save(output);
            return output.ToArray();
        });

    // Styles land ONCE as shared Document styles the placemarks reference by #id StyleUrl, the ortho drapes as a
    // GroundOverlay with its image archived beside doc.kml, and the tour stops become a gx:Tour Playlist of FlyTo
    // primitives framed by each stop's CalculateLookAt.
    public static Fin<byte[]> Site(
        Seq<GeoFeature> features,
        Func<GeoFeature, string> styleOf,
        Func<GeoFeature, KmlElevation> elevationOf,
        Map<string, (Color32 Line, double WidthPx, Color32 Fill)> styles,
        Option<OrthoDrape> ortho,
        Seq<GeoFeature> tour,
        Op key) =>
        GeoServices.Wgs84
            .Bind(frame => features.Traverse(f => f.Reproject(frame, key)).As()
                .Bind(wgs => tour.Traverse(t => t.Reproject(frame, key)).As().Map(route => (Wgs: wgs, Route: route))))
            .Bind(site => key.Catch(() => {
                var document = new KmlDom.Document { Name = "site-context" };
                styles.Iter((id, row) => document.AddStyle(new KmlDom.Style {
                    Id = id,
                    Line = new KmlDom.LineStyle { Color = row.Line, Width = row.WidthPx },
                    Polygon = new KmlDom.PolygonStyle { Color = row.Fill, Fill = true, Outline = true },
                }));
                site.Wgs.Iter(f => document.AddFeature(Mark(f, Some(styleOf(f)), elevationOf(f))));
                ortho.Iter(o => document.AddFeature(Drape(o)));
                if (!site.Route.IsEmpty) { document.AddFeature(TourOf(site.Route)); }
                var kml = new KmlDom.Kml { Feature = document };
                kml.AddNamespacePrefix("gx", "http://www.google.com/kml/ext/2.2");
                using var kmz = KmzFile.Create(KmlFile.Create(kml, duplicates: false));
                ortho.Iter(o => kmz.AddFile(OrthoEntry, o.Switch(boxed: static b => b.Png, quad: static q => q.Png)));
                using var output = new MemoryStream();
                kmz.Save(output);
                return output.ToArray();
            }));

    static KmlDom.GroundOverlay Drape(OrthoDrape ortho) {
        var overlay = new KmlDom.GroundOverlay {
            Icon = new KmlDom.Icon { Href = new Uri(OrthoEntry, UriKind.Relative) },
            DrawOrder = -1,
        };
        // Block-bodied arms bind the VOID Switch: the two assignments land different carrier types, so an
        // expression-bodied pair would ask the generated func form to infer one result type from two.
        ortho.Switch(
            state: overlay,
            boxed: static (o, b) => {
                o.Bounds = new KmlDom.LatLonBox {
                    North = b.Bounds.MaxY, South = b.Bounds.MinY, East = b.Bounds.MaxX, West = b.Bounds.MinX,
                };
            },
            quad: static (o, q) => {
                o.GXLatLonQuad = new LatLonQuad {
                    Coordinates = Vectors([q.LowerLeft, q.LowerRight, q.UpperRight, q.UpperLeft]),
                };
            });
        return overlay;
    }

    static KmlDom.Placemark Mark(GeoFeature feature, Option<string> styleId, KmlElevation elevation) {
        var mark = new KmlDom.Placemark {
            Name = feature.Text("name").IfNone(""),
            Geometry = Raise(feature.Geometry, elevation),
            StyleUrl = styleId.Map(static id => new Uri($"#{id}", UriKind.Relative)).ValueUnsafe(),
        };
        var data = new KmlDom.ExtendedData();
        feature.Attributes.GetNames().AsIterable()
            .Iter(name => data.AddData(new KmlDom.Data { Name = name, Value = feature.Text(name).IfNone("") }));
        mark.ExtendedData = data;
        return mark;
    }

    // NTS -> KML DOM: the inverse bridge (Coordinate(X,Y) -> Vector(lat: Y, lon: X)); a polygon carries its holes as
    // InnerBoundary rings, a collection recurses through MultipleGeometry. The Point fallback anchors on the
    // geometry's INTERIOR point, guaranteed on the shape, where a centroid can land off it entirely.
    static KmlDom.Geometry Raise(Geometry geometry, KmlElevation elevation) => geometry switch {
        Point p => new KmlDom.Point {
            Coordinate = Vector3(p.Coordinate), AltitudeMode = elevation.Mode, Extrude = elevation.Extrudes,
        },
        LineString l => new KmlDom.LineString {
            Coordinates = Vectors(l.Coordinates), AltitudeMode = elevation.Mode,
            Extrude = elevation.Extrudes, Tessellate = elevation.Tessellates,
        },
        Polygon poly => RaisePolygon(poly, elevation),
        GeometryCollection collection => RaiseCollection(collection, elevation),
        var other => new KmlDom.Point {
            Coordinate = Vector3(other.InteriorPoint.Coordinate), AltitudeMode = elevation.Mode, Extrude = elevation.Extrudes,
        },
    };

    static KmlDom.Polygon RaisePolygon(Polygon polygon, KmlElevation elevation) {
        var raised = new KmlDom.Polygon {
            AltitudeMode = elevation.Mode,
            Extrude = elevation.Extrudes,
            Tessellate = elevation.Tessellates,
            OuterBoundary = new KmlDom.OuterBoundary { LinearRing = new KmlDom.LinearRing { Coordinates = Vectors(polygon.ExteriorRing.Coordinates) } },
        };
        Enumerable.Range(0, polygon.NumInteriorRings).AsIterable()
            .Iter(i => raised.AddInnerBoundary(new KmlDom.InnerBoundary {
                LinearRing = new KmlDom.LinearRing { Coordinates = Vectors(polygon.GetInteriorRingN(i).Coordinates) },
            }));
        return raised;
    }

    static KmlDom.MultipleGeometry RaiseCollection(GeometryCollection collection, KmlElevation elevation) {
        var multi = new KmlDom.MultipleGeometry();
        Enumerable.Range(0, collection.NumGeometries).AsIterable()
            .Iter(i => multi.AddGeometry(Raise(collection.GetGeometryN(i), elevation)));
        return multi;
    }

    static KmlDom.CoordinateCollection Vectors(Coordinate[] coordinates) => new(coordinates.Select(Vector3));

    // Z rides the third Vector slot exactly where it is FINITE: the read path admits an absent KML altitude as the
    // NTS null ordinate, so a 2D coordinate round-trips as two ordinates and a 3D one as three, where writing the
    // null ordinate into the altitude slot would emit a coordinate no reader parses.
    static Vector Vector3(Coordinate c) =>
        double.IsFinite(c.Z) ? new Vector(c.Y, c.X, c.Z) : new Vector(c.Y, c.X);

    // Each stop frames itself: CalculateLookAt derives the camera from the stop's own bounds, so the fly-through
    // needs no hand-authored viewpoints; the 3-second dwell is the FlyTo Duration.
    static Tour TourOf(Seq<GeoFeature> route) {
        var playlist = new Playlist();
        route.Iter(stop => playlist.AddTourPrimitive(new FlyTo {
            Duration = 3.0, View = Mark(stop, Option<string>.None, KmlElevation.Draped).CalculateLookAt(),
        }));
        return new Tour { Name = "site-tour", Playlist = playlist };
    }
}
```

## [04]-[VECTOR_FOLD]

- Owner: `GeoVector` the ingest-and-egress fold over the `GeoVectorSource` table — the managed shapefile/GeoJSON/CityJSON/FlatGeobuf/GeoParquet arms, the remote-`.fgb` `PackedRTree.StreamSearch` range read, the OGR universal arm with its typed field crossing, and the symmetric egress; `OgrField` the OGR `FieldType`-keyed read roster; `CityJsonHeader` the ONE CityJSON metadata-and-appearance admission; `GeoParquetSchema` and `HeaderCrs` the two columnar/header admissions.
- Law: every OGR field crosses at its DECLARED `FieldType` — an integer column lands `PropertyValue.Integer`, a real lands `Number`, a date lands `Temporal` — because flattening to text makes "9" sort after "1250.5", makes a null and an empty string one value, and makes a date unorderable, so an IDS facet, a `Model/query` predicate, and a `Planning/cost` quantity all compare wrong on the same column the codec had already decoded correctly.
- Entry: `Read(source, bytes, window, key)` dispatches the row's decode column; `Stream(fetch, window, key)` is the remote-`.fgb` range-read escalation; `CityJsonAppearance(bytes, key)` is the texture-roster entry over the same document the geometry arm decodes; `Write(source, features, crs, key)` dispatches the row's encoder and rails typed on its absence.
- Auto: `Planar` is the ONE managed-codec admission — it refuses an attribute filter no managed codec can push down and traps the codec's own throw onto the typed lane fault, so no arm re-spells either; every produced `GeoFeature` re-enters `GeoModel.Of`, which is where `GeometryFixer.Fix` runs.
- Receipt: the `Read` `Seq<GeoFeature>` is the universal vector ingest evidence `Semantics/model#GEO_MODEL` indexes and `Semantics/feature#GEO_FEATURE` `ToObject` lowers onto seam `Object` nodes; the `GeoVectorSource` row records which codec decoded, so the reader is one table read.
- Packages: `NetTopologySuite`, `NetTopologySuite.IO.Esri.Shapefile`, `bertt.CityJSON`, `FlatGeobuf`, `GISBlox.IO.GeoParquet`, `MaxRev.Gdal.Core`, `NodaTime`, `Rasm.Element`, `LanguageExt.Core`
- Growth: a new attribute push-down is one `Layer.SetAttributeFilter` argument the `GeoWindow` already carries; a new OGR field type is one `OgrField` row; a new remote transport is one `PackedRTree.ReadNode` delegate value; never a hand-rolled binary record.
- Boundary: the managed codec output IS the canonical `NetTopologySuite.Features.Feature`; the shapefile byte form is the zipped `.shp`/`.shx`/`.dbf`/`.prj` quartet BOTH directions — a `Stream.Null` dbf read that drops every attribute and a bare-`.shp` egress that strands the offset index and attribute table are the deleted fragment forms; a zip with no `.shp` REFUSES typed rather than throwing, and the absent `.dbf`/`.prj` degrade (geometry-only, no CRS) because those two are genuinely optional; a corrupt GeoJSON document that deserializes to null REFUSES rather than becoming an EMPTY SET, because a caller cannot tell an empty collection from a payload the parser rejected; the OGR↔NTS bridge is the ONE `Semantics/feature#GEO_BOUNDARY` `GeoWkb` owner and running planar boolean ops on the OGR side fragments the one topology owner; `CityJSON.*`/`OSGeo.*`/`FlatGeobuf.*`/`GISBlox.*`/`SharpKml.*` types never leak past this fold; the CityJSON quantization is lossless (integer indices into `Vertices`, recovered through `Transform`) and tessellating it in the codec is the deleted form — `ToFeatures` IS the codec's own NTS projection rail, where a convex hull over every vertex of a building solid is not a footprint but the plan projection of its convex envelope, strictly LARGER than the truth for any L-shaped, courtyard, or cantilevered massing; the `.fgb` header CARRIES its CRS and reading past it hands every feature a `None` frame, which the datum leg then treats as Unreferenced — so a `.fgb` in a projected national grid lands raw eastings on the WGS84 frame at H3, MVT, and KML time, silently, with the correcting fact free in the buffer the arm already holds; the CityJSON header is derived ONCE per document because three inline re-derivations of one metadata read drift the day the spec moves; the OGR universal egress writes its driver output to a real temp file through the `GeoGdal` bracket, because this GDAL SWIG build exposes only `VSIFWriteL(string, …)` and no `byte[]` `VSIFReadL`.

```csharp signature
// --- [TABLES] -------------------------------------------------------------------------------
// The OGR field roster: one row per FieldType carrying the read that preserves the field's own CLR type, so the
// GeoFeature.Typed seam arm receives the truth OGR already decoded. GetFieldAsString over every field was the
// discarded discriminant this table restores; OFTInteger additionally reads its FieldSubType, because OGR spells a
// boolean column as an integer field with the OFSTBoolean subtype and nothing else distinguishes the two.
public static class OgrField {
    static readonly Map<OSGeo.OGR.FieldType, Func<OSGeo.OGR.Feature, int, OSGeo.OGR.FieldDefn, object>> Rows = Map(
        (OSGeo.OGR.FieldType.OFTInteger,   static (f, i, defn) => defn.GetSubType() == OSGeo.OGR.FieldSubType.OFSTBoolean
                                                                     ? f.GetFieldAsInteger(i) != 0
                                                                     : (object)f.GetFieldAsInteger(i)),
        (OSGeo.OGR.FieldType.OFTInteger64, static (f, i, _) => f.GetFieldAsInteger64(i)),
        (OSGeo.OGR.FieldType.OFTReal,      static (f, i, _) => f.GetFieldAsDouble(i)),
        (OSGeo.OGR.FieldType.OFTString,    static (f, i, _) => f.GetFieldAsString(i)),
        (OSGeo.OGR.FieldType.OFTBinary,    static (f, i, _) => f.GetFieldAsString(i)),
        (OSGeo.OGR.FieldType.OFTDate,      static (f, i, _) => Stamped(f, i).Date),
        (OSGeo.OGR.FieldType.OFTTime,      static (f, i, _) => Stamped(f, i).Clock),
        (OSGeo.OGR.FieldType.OFTDateTime,  static (f, i, _) => Stamped(f, i).Moment));

    // An unrostered type keeps the string read, so an exotic OGR column never faults an import — the fallback is the
    // ONE place text survives, not the default every column took.
    public static object Read(OSGeo.OGR.Feature feature, int index, OSGeo.OGR.FieldDefn defn) =>
        Rows.Find(defn.GetFieldType()).Match(
            Some: row => row(feature, index, defn),
            None: () => feature.GetFieldAsString(index));

    // GetFieldAsDateTime hands back the parts plus a TZ flag whose values are OGR's own: 0 unknown, 1 local, and 100
    // upward the UTC offset in quarter-hours off 100. Zone evidence therefore EXISTS or it does not, and the two
    // land different TemporalValue arms — stamping an unknown-zone field UTC fabricates an Instant that re-bases
    // every date by the reader's offset and type-checks either way.
    static (LocalDate Date, LocalTime Clock, object Moment) Stamped(OSGeo.OGR.Feature feature, int index) {
        feature.GetFieldAsDateTime(index, out int year, out int month, out int day,
            out int hour, out int minute, out float second, out int zone);
        var date = new LocalDate(year, month, day);
        var clock = new LocalTime(hour, minute, (int)second);
        var moment = date.At(clock);
        return (date, clock, zone >= 100
            ? new DateTimeOffset(moment.ToDateTimeUnspecified(), TimeSpan.FromMinutes((zone - 100) * 15))
            : moment);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoVector {
    // ONE decode entry over the source table's delegate column — zero call-site branch; every arm already carries its
    // own rail, so the entry adds the roster and nothing else.
    public static Fin<Seq<GeoFeature>> Read(GeoVectorSource source, ReadOnlyMemory<byte> bytes, GeoWindow window, Op key) =>
        source.Decode(bytes, window, key);

    // The ONE managed-codec admission: a managed codec has no attribute engine, so a Where it silently ignored would
    // return a superset the caller believes was filtered, and the refusal names the source instead. The throwing
    // codec body lands on the typed lane fault here rather than in seven arms.
    internal static Fin<Seq<GeoFeature>> Planar(GeoWindow window, string source, Op key, Func<Seq<GeoFeature>> decode) =>
        window.Where.IsSome
            ? Fin.Fail<Seq<GeoFeature>>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "geo-format-lane", "vector", source, "attribute-pushdown-unsupported" })))
            : key.Catch(decode);

    // The client-side residual for the arms whose codec carries no spatial index.
    static Seq<GeoFeature> Clipped(Seq<GeoFeature> features, GeoWindow window) =>
        window.Clip.Match(None: () => features, Some: env => features.Filter(f => f.Bounds.Intersects(env)));

    // --- [FLATGEOBUF]
    // The clip pushes DOWN through the Packed-Hilbert-R-tree bbox index so a continental .fgb decodes only the
    // overlapping feature runs — the managed equivalent of Layer.SetSpatialFilterRect, never an Ogr.Open over
    // /vsimem. Helpers.ReadHeader is the same header read Stream performs, so the CRS costs one read on a buffer
    // the arm already holds.
    internal static Fin<Seq<GeoFeature>> FlatGeobuf(ReadOnlyMemory<byte> bytes, GeoWindow window, Op key) =>
        Planar(window, "flatgeobuf", key, () => {
            using var fgb = new MemoryStream(bytes.ToArray());
            var crs = HeaderCrs(global::FlatGeobuf.Helpers.ReadHeader(fgb, out int _).UnPack(), key);
            fgb.Position = 0;
            var rect = window.Clip.Match<Envelope?>(env => env, () => null);
            return global::FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(fgb, rect).AsIterable()
                .Map(f => new GeoFeature(f.Geometry, f.Attributes, crs)).ToSeq();
        });

    // The header's CRS lowers through the SAME three-state ProjectedCrs.Of the shapefile .prj and CityJSON
    // referenceSystem arms compose — the EPSG authority name when the code is set, the inline WKT when it is, and
    // the no-CRS None when the header declared neither (Of faults an all-blank product, so the blank case never
    // enters admission). ONE admission owner, so the two nullable header reads coalesce exactly here.
    static Option<ProjectedCrs> HeaderCrs(global::FlatGeobuf.HeaderT header, Op key) =>
        Optional(header.Crs)
            .Bind(crs => crs.Code > 0
                ? Some(($"EPSG:{crs.Code}", ""))
                : Optional(crs.Wkt).Filter(static wkt => wkt.Length > 0).Map(static wkt => ("", wkt)))
            .Bind(pair => ProjectedCrs.Of(pair.Item1, "", "", pair.Item2, key)
                .Match(Succ: static c => Some(c), Fail: static _ => Option<ProjectedCrs>.None));

    // Remote-.fgb range-read escalation: fetch is the consumer's byte-range read (HTTP Range / object-store seek).
    // Helpers.ReadHeader consumes the 8-byte magic + 4-byte size prefix + headerSize blob off the range stream, so
    // the index seats at 12 + headerSize (the codec's own Deserialize seeks exactly that); PackedRTree.StreamSearch
    // walks ONLY the index nodes it needs (its ReadNode offsets are index-relative, re-based here), CalcSize seats
    // the feature body, and each hit reads its 4-byte record length then fetches that record body alone —
    // FromByteBuffer lowers the PREFIX-STRIPPED buffer against the canonical factory and the packed sequence layout.
    public static Fin<Seq<GeoFeature>> Stream(PackedRTree.ReadNode fetch, Envelope window, Op key) =>
        key.Catch(() => {
            using var head = fetch(0, HeaderProbeBytes);
            var header = global::FlatGeobuf.Helpers.ReadHeader(head, out int headerSize);
            var schema = header.UnPack();
            ulong indexOrigin = 12uL + (ulong)headerSize;
            ulong bodyOrigin = indexOrigin + PackedRTree.CalcSize(header.FeaturesCount, header.IndexNodeSize);
            var sequences = new global::FlatGeobuf.NTS.FlatGeobufCoordinateSequenceFactory();
            var crs = HeaderCrs(schema, key);
            return PackedRTree.StreamSearch(header.FeaturesCount, header.IndexNodeSize, window,
                    (offset, length) => fetch(indexOrigin + offset, length))
                .AsIterable()
                .Map(hit => Record(fetch, bodyOrigin + hit.Item1, sequences, schema, crs))
                .ToSeq();
        });

    static GeoFeature Record(
        PackedRTree.ReadNode fetch, ulong origin,
        global::FlatGeobuf.NTS.FlatGeobufCoordinateSequenceFactory sequences,
        global::FlatGeobuf.HeaderT schema, Option<ProjectedCrs> crs) {
        var prefix = new byte[4];
        using (var sized = fetch(origin, 4)) { sized.ReadExactly(prefix); }
        var body = new byte[BinaryPrimitives.ReadUInt32LittleEndian(prefix)];
        using (var record = fetch(origin + 4, (ulong)body.Length)) { record.ReadExactly(body); }
        IFeature feature = global::FlatGeobuf.NTS.FeatureConversions.FromByteBuffer(
            GeoServices.Factory, sequences, new global::Google.FlatBuffers.ByteBuffer(body), schema);
        return new GeoFeature(feature.Geometry, feature.Attributes, crs);
    }

    // One range read covers magic + the size-prefixed header for any real-world schema; ReadHeader stops at the
    // header end regardless of the over-fetch.
    const ulong HeaderProbeBytes = 1UL << 16;

    // --- [GEOPARQUET]
    // ReadColumns is the COLUMN push-down (the columnar analog of the FGB bbox ROW push-down), the geo column holds
    // WKB the GeoWkb bridge crosses. GeoParquet carries no server-side bbox filter, so the clip filters client-side.
    internal static Fin<Seq<GeoFeature>> GeoParquet(ReadOnlyMemory<byte> bytes, GeoWindow window, Op key) =>
        Planar(window, "geoparquet", key, () => {
            string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.parquet");
            File.WriteAllBytes(path, bytes.ToArray());
            try {
                string primary = PrimaryColumn(path);
                var table = GISBlox.IO.GeoParquet.GeoParquetReader.ReadAll(path, GISBlox.IO.GeoParquet.Common.GeometryFormat.WKB);
                return Clipped(table.AsEnumerable().AsIterable()
                    .Map(row => new GeoFeature(GeoWkb.ToNts((byte[])row[primary]), RowAttributes(table, row, primary), Option<ProjectedCrs>.None))
                    .ToSeq(), window);
            } finally { File.Delete(path); }
        });

    // The ONE GeoParquet metadata admission: the spec's own default geometry-column name applies when the file
    // declares no metadata block or no primary column, and it is spelled once rather than at each read.
    static string PrimaryColumn(string path) =>
        Optional(GISBlox.IO.GeoParquet.GeoParquetReader.ReadGeoMetadata(path))
            .Bind(static meta => Optional(meta.Primary_column))
            .Filter(static column => column.Length > 0)
            .IfNone("geometry");

    // DataColumn.DataType is the column's own CLR type, so the value crosses at that type onto the Typed seam arm;
    // a null cell is the empty text the seam already spells for absence at this grain.
    static IAttributesTable RowAttributes(System.Data.DataTable table, System.Data.DataRow row, string primary) {
        var attributes = new AttributesTable();
        table.Columns.Cast<System.Data.DataColumn>().AsIterable()
            .Filter(column => column.ColumnName != primary)
            .Iter(column => attributes.Add(column.ColumnName, row.IsNull(column) ? "" : row[column]));
        return attributes;
    }

    // --- [GEOJSON]
    // A null deserialization is a CORRUPT payload, and turning it into an empty FeatureCollection made a rejected
    // document indistinguishable from a document with no features — the one outcome a site import cannot recover.
    internal static Fin<Seq<GeoFeature>> GeoJson(ReadOnlyMemory<byte> bytes, GeoWindow window, Op key) =>
        Planar(window, "geojson", key, () => JsonSerializer.Deserialize<FeatureCollection>(bytes.Span, GeoWire.Json))
            .Bind(collection => Optional(collection)
                .ToFin(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "geo-format-lane", "vector", "geojson", "null-document" }))))
            .Map(collection => Clipped(collection.AsIterable()
                .Map(f => new GeoFeature(f.Geometry, f.Attributes, Option<ProjectedCrs>.None)).ToSeq(), window));

    // --- [SHAPEFILE]
    // A shapefile is the .shp/.shx/.dbf/.prj QUARTET — one byte buffer carries it as the standard zip transport,
    // discriminated on the zip magic exactly like the KMZ arm; a bare .shp buffer degrades to geometry-only. The
    // .dbf stream feeds OpenRead so attributes survive, and the .prj WKT tags the seam SourceCrs so a projected
    // shapefile datum-shifts at projection instead of silently landing raw ordinates on the model frame.
    internal static Fin<Seq<GeoFeature>> Shapefile(ReadOnlyMemory<byte> bytes, GeoWindow window, Op key) =>
        Quartet(bytes, key).Bind(parts => Planar(window, "shapefile", key, () => {
            var options = new ShapefileReaderOptions {
                Factory = GeoServices.Factory,
                MbrFilter = window.Clip.Match<Envelope?>(env => env, () => null),
                GeometryBuilderMode = GeometryBuilderMode.FixInvalidShapes,
            };
            Option<ProjectedCrs> crs = parts.Prj.Length == 0
                ? Option<ProjectedCrs>.None
                : ProjectedCrs.Of("", "", "", parts.Prj, key)
                    .Match(Succ: static c => Some(c), Fail: static _ => Option<ProjectedCrs>.None);
            using var shp = new MemoryStream(parts.Shp);
            using Stream dbf = parts.Dbf.Match(Some: static buffer => new MemoryStream(buffer), None: static () => Stream.Null);
            using var reader = NetTopologySuite.IO.Esri.Shapefile.OpenRead(shp, dbf, options);
            return reader.AsIterable().Map(feature => new GeoFeature(feature.Geometry, feature.Attributes, crs)).ToSeq();
        }));

    // Zip-quartet split: entries resolve by extension (any base name). A zip with no .shp is a corrupt container and
    // REFUSES by name; the absent .dbf and .prj degrade because both are genuinely optional parts of the transport.
    static Fin<(byte[] Shp, Option<byte[]> Dbf, string Prj)> Quartet(ReadOnlyMemory<byte> bytes, Op key) =>
        bytes.Span is not [0x50, 0x4B, ..]
            ? Fin.Succ((bytes.ToArray(), Option<byte[]>.None, ""))
            : key.Catch(() => {
                using var archive = new ZipArchive(new MemoryStream(bytes.ToArray()), ZipArchiveMode.Read);
                Option<byte[]> Entry(string extension) =>
                    Optional(archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
                        .Map(static entry => {
                            using var source = entry.Open();
                            using var buffer = new MemoryStream();
                            source.CopyTo(buffer);
                            return buffer.ToArray();
                        });
                return (Shp: Entry(".shp"), Dbf: Entry(".dbf"), Prj: Entry(".prj").Map(Encoding.UTF8.GetString).IfNone(""));
            })
            .Bind(parts => parts.Shp.Match(
                Some: shp => Fin.Succ((shp, parts.Dbf, parts.Prj)),
                None: () => Fin.Fail<(byte[], Option<byte[]>, string)>(
                    new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "geo-format-lane", "vector", "shapefile", "zip-missing-shp" })))));

    // --- [CITYJSON]
    // ToFeatures IS the codec's own NTS projection rail: it walks CityObjects, applies Transform per CoordinateZ,
    // triangulates each Geometry into a MultiPolygon, and packs CityObject.Attributes into an AttributesTable. A null
    // lod selects the codec's own highest-LoD pick; a caller narrowing to one LoD threads it through the source row.
    internal static Fin<Seq<GeoFeature>> CityJson(ReadOnlyMemory<byte> bytes, GeoWindow window, Op key) =>
        Planar(window, "cityjson", key, () => {
            var document = Document(bytes);
            var header = CityJsonHeader.Of(document, key);
            return Clipped(document.ToFeatures(lod: null).AsIterable()
                .Map(f => new GeoFeature(f.Geometry, f.Attributes, header.Crs)).ToSeq(), window);
        });

    // The appearance entry over the SAME document the geometry arm decodes: CityJSON is the one non-glTF ingest whose
    // spec carries a COMPLETE texture appearance beside the geometry — payload, wrap mode, per-surface UV indices,
    // and a shared UV pool — and the source row's decode column carries features alone, so the roster reaches its
    // Semantics/appearance#APPEARANCE_PROJECTION consumer through its own entry rather than through a document no
    // caller holds. Reading a UV index array and a payload URL is neither a codec nor a resample, so the
    // "CLASSIFIES and CARRIES, decodes none" ruling holds.
    public static Fin<Seq<SurfaceTexture>> CityJsonAppearance(ReadOnlyMemory<byte> bytes, Op key) =>
        key.Catch(() => CityJsonHeader.Of(Document(bytes), key).Textures)
            ;

    static CityJSON.CityJsonDocument Document(ReadOnlyMemory<byte> bytes) =>
        Newtonsoft.Json.JsonConvert.DeserializeObject<CityJSON.CityJsonDocument>(Encoding.UTF8.GetString(bytes.Span))!;

    // --- [OGR]
    // Ogr.Open auto-detects the container, so the universal decode carries no driver token; the clip and the
    // attribute restriction both push DOWN to the layer, and every geometry crosses the ONE GeoWkb bridge. The
    // per-layer read is a generator fold rather than a Seq.Add accumulator, which was quadratic over a continental
    // extract.
    internal static Fin<Seq<GeoFeature>> Universal(ReadOnlyMemory<byte> bytes, GeoWindow window, Op key) =>
        GeoGdal.Vector(bytes, data => key.Catch(() =>
            Enumerable.Range(0, data.GetLayerCount()).AsIterable()
                .Bind(l => {
                    var layer = data.GetLayerByIndex(l);
                    window.Clip.Iter(env => layer.SetSpatialFilterRect(env.MinX, env.MinY, env.MaxX, env.MaxY));
                    window.Where.Iter(filter => layer.SetAttributeFilter(filter.Value));
                    layer.ResetReading();
                    return Cursor(layer);
                })
                .ToSeq())
            ,
            "ogr", key);

    static IEnumerable<GeoFeature> Cursor(OSGeo.OGR.Layer layer) {
        for (var feature = layer.GetNextFeature(); feature is not null; feature = layer.GetNextFeature()) {
            yield return new GeoFeature(GeoWkb.ToNts(feature.GetGeometryRef()), AttributesOf(feature), Option<ProjectedCrs>.None);
        }
    }

    static IAttributesTable AttributesOf(OSGeo.OGR.Feature feature) {
        var table = new AttributesTable();
        Enumerable.Range(0, feature.GetFieldCount()).AsIterable().Iter(f => {
            using var defn = feature.GetFieldDefnRef(f);
            table.Add(defn.GetName(), feature.IsFieldSetAndNotNull(f) ? OgrField.Read(feature, f, defn) : null);
        });
        return table;
    }

    // --- [EGRESS]
    // Symmetric egress dispatches the SAME row's ENCODER column Read's decode rides. A row carrying NO encoder rails
    // typed off the absence itself, so the roster states its own write capability and no exception ever crosses to
    // say what a column already declares.
    public static Fin<byte[]> Write(GeoVectorSource source, Seq<GeoFeature> features, Option<ProjectedCrs> crs, Op key) =>
        source.Encoder.Match(
            None: () => Fin.Fail<byte[]>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "geo-vector-write-unsupported", source.Key }))),
            Some: encode => encode(features, crs, key));

    internal static Fin<byte[]> WriteFlatGeobuf(Seq<GeoFeature> features, Op key) =>
        key.Catch(() => {
            using var output = new MemoryStream();
            var kind = features.Head
                .Map(static f => global::FlatGeobuf.NTS.GeometryConversions.ToGeometryType(f.Geometry))
                .IfNone(global::FlatGeobuf.GeometryType.Unknown);
            global::FlatGeobuf.NTS.FeatureCollectionConversions.Serialize(
                output, features.Map(static f => (IFeature)new Feature(f.Geometry, f.Attributes)), kind, dimensions: 3, columns: null);
            return output.ToArray();
        });

    // Every attribute column is declared at the CLR type its values carry, resolved off the first row that answers
    // the name: declaring every column typeof(string) erased the decode the ingest arms had just performed, so a
    // columnar round-trip through this folder narrowed every integer, real, and date to text.
    internal static Fin<byte[]> WriteGeoParquet(Seq<GeoFeature> features, Op key) =>
        key.Catch(() => {
            const string geoColumn = "geometry";
            var table = new System.Data.DataTable();
            table.AddGeoColumn(geoColumn, 0, GISBlox.IO.GeoParquet.Common.GeometryFormat.WKB);
            table.Columns[geoColumn]!.SetAsPrimaryGeoColumn();
            var columns = features
                .Bind(static f => toSeq(f.Attributes.GetNames()).Map(name => (Name: name, Value: f.Attr(name))))
                .Filter(static column => column.Value.IsSome)
                .Distinct(static (a, b) => string.Equals(a.Name, b.Name, StringComparison.Ordinal))
                .Map(static column => (column.Name, Type: column.Value.Map(static v => v!.GetType()).IfNone(typeof(string))))
                .ToSeq();
            columns.Iter(column => table.Columns.Add(column.Name, column.Type));
            table.AddGeoProcessingMetadata([geoColumn], geoColumn);
            features.Iter(f => {
                var row = table.NewRow();
                row[geoColumn] = GeoWkb.FromNts(f.Geometry);
                columns.Iter(column => row[column.Name] = f.Attr(column.Name).Match(Some: static v => v, None: static () => DBNull.Value));
                table.Rows.Add(row);
            });
            using var output = new MemoryStream();
            GISBlox.IO.GeoParquet.GeoParquetWriter.Write(output, table, geoColumn);
            return output.ToArray();
        });

    // Shapefile egress is the QUARTET zipped into ONE archive buffer (the transport the read arm discriminates):
    // returning the bare .shp alone strands the .shx offset index and the .dbf attributes. .prj carries the seam CRS:
    // the inline Wkt when a GIS-origin CRS defined it, else the authority Name.
    internal static Fin<byte[]> WriteShapefile(Seq<GeoFeature> features, Option<ProjectedCrs> crs, Op key) =>
        key.Catch(() => {
            using var shp = new MemoryStream();
            using var shx = new MemoryStream();
            using var dbf = new MemoryStream();
            using var prj = new MemoryStream();
            NetTopologySuite.IO.Esri.Shapefile.WriteAllFeatures(
                features.Map(static f => (IFeature)new Feature(f.Geometry, f.Attributes)),
                shp, shx, dbf, prj, crs.Map(static c => c.Wkt.Length > 0 ? c.Wkt : c.Name).IfNone(""), null);
            using var output = new MemoryStream();
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true)) {
                toSeq([("features.shp", shp), ("features.shx", shx), ("features.dbf", dbf), ("features.prj", prj)])
                    .Filter(static part => part.Item2.Length > 0)
                    .Iter(part => {
                        using var entry = archive.CreateEntry(part.Item1).Open();
                        entry.Write(part.Item2.GetBuffer().AsSpan(0, (int)part.Item2.Length));
                    });
            }
            return output.ToArray();
        });

    internal static Fin<byte[]> WriteGeoJson(Seq<GeoFeature> features, Op key) =>
        key.Catch(() => {
            var collection = new FeatureCollection();
            features.Iter(f => collection.Add(new Feature(f.Geometry, f.Attributes)));
            return JsonSerializer.SerializeToUtf8Bytes(collection, GeoWire.Json);
        });

    // The OGR universal egress declares a FieldDefn per attribute at its OGR-side type and SETS every value, where
    // writing GEOMETRY alone shipped attribute-less features through a driver family whose whole point is carrying
    // them. The sink is a real temp file the managed read-back recovers, inside the ONE GeoGdal bracket.
    internal static Fin<byte[]> WriteUniversal(string ogrDriver, Seq<GeoFeature> features, Option<ProjectedCrs> crs, Op key) =>
        GeoGdal.Author(GdalSink.Temp, "", path => key.Catch(() => {
            var columns = features
                .Bind(static f => toSeq(f.Attributes.GetNames()).Map(name => (Name: name, Value: f.Attr(name))))
                .Filter(static column => column.Value.IsSome)
                .Distinct(static (a, b) => string.Equals(a.Name, b.Name, StringComparison.Ordinal))
                .Map(static column => (column.Name, Type: OgrType(column.Value)))
                .ToSeq();
            using (var driver = OSGeo.OGR.Ogr.GetDriverByName(ogrDriver))
            using (var data = driver.CreateDataSource(path, [])) {
                using var srs = crs.Match(Some: SpatialRef, None: () => (OSGeo.OSR.SpatialReference?)null);
                using var layer = data.CreateLayer("features", srs, OSGeo.OGR.wkbGeometryType.wkbUnknown, []);
                columns.Iter(column => {
                    using var defn = new OSGeo.OGR.FieldDefn(column.Name, column.Type);
                    layer.CreateField(defn, 1);
                });
                using var schema = layer.GetLayerDefn();
                features.Iter(f => {
                    using var feature = new OSGeo.OGR.Feature(schema);
                    using var geom = GeoWkb.ToOgr(f.Geometry);
                    feature.SetGeometry(geom);
                    columns.Iter((ordinal, column) => f.Attr(column.Name).Match(
                        Some: value => Set(feature, ordinal, value),
                        None: () => feature.SetFieldNull(ordinal)));
                    layer.CreateFeature(feature);
                });
            }
            return File.ReadAllBytes(path);
        }),
        "ogr-write", key);

    // The egress inverse of the OgrField roster: a CLR type resolves the OGR field type it declares, and an
    // unresolved type falls to string exactly where the read fallback does, so a round-trip through this folder
    // narrows nothing it did not already narrow on the way in.
    static OSGeo.OGR.FieldType OgrType(Option<object> sample) => sample.Match(
        Some: static value => value switch {
            bool or sbyte or byte or short or ushort or int => OSGeo.OGR.FieldType.OFTInteger,
            long or uint or ulong                           => OSGeo.OGR.FieldType.OFTInteger64,
            double or float or decimal                      => OSGeo.OGR.FieldType.OFTReal,
            LocalDate                                       => OSGeo.OGR.FieldType.OFTDate,
            LocalTime                                       => OSGeo.OGR.FieldType.OFTTime,
            LocalDateTime or Instant or DateTimeOffset or DateTime => OSGeo.OGR.FieldType.OFTDateTime,
            _                                               => OSGeo.OGR.FieldType.OFTString,
        },
        None: static () => OSGeo.OGR.FieldType.OFTString);

    // SetField's overload family is per-type on the SWIG surface and the date arm takes its parts plus a TZ flag,
    // so the write mirrors the read's own zone discrimination: 100 is UTC, 0 is unknown.
    static void Set(OSGeo.OGR.Feature feature, int ordinal, object value) {
        switch (value) {
            case bool flag: feature.SetField(ordinal, flag ? 1 : 0); break;
            case sbyte or byte or short or ushort or int: feature.SetField(ordinal, Convert.ToInt32(value, CultureInfo.InvariantCulture)); break;
            case long or uint or ulong: feature.SetFieldInteger64(ordinal, Convert.ToInt64(value, CultureInfo.InvariantCulture)); break;
            case double or float or decimal: feature.SetField(ordinal, Convert.ToDouble(value, CultureInfo.InvariantCulture)); break;
            case LocalDate date: feature.SetField(ordinal, date.Year, date.Month, date.Day, 0, 0, 0f, 0); break;
            case LocalTime clock: feature.SetField(ordinal, 0, 0, 0, clock.Hour, clock.Minute, clock.Second, 0); break;
            case LocalDateTime moment: feature.SetField(ordinal, moment.Year, moment.Month, moment.Day, moment.Hour, moment.Minute, moment.Second, 0); break;
            case Instant instant: Set(feature, ordinal, instant.ToDateTimeOffset()); break;
            case DateTimeOffset stamp: feature.SetField(ordinal, stamp.Year, stamp.Month, stamp.Day, stamp.Hour, stamp.Minute, stamp.Second, 100); break;
            default: feature.SetField(ordinal, value.ToString()); break;
        }
    }

    static OSGeo.OSR.SpatialReference SpatialRef(ProjectedCrs crs) {
        var srs = new OSGeo.OSR.SpatialReference("");
        // SetFromUserInput is OSR's universal CRS parser (an EPSG/URN authority code OR an inline WKT); the seam
        // three-state ProjectedCrs carries the inline Wkt when present, else the authority Name (no single-string
        // .Value member exists on the [ComplexValueObject] — that spelling is the deleted phantom).
        srs.SetFromUserInput(crs.Wkt.Length > 0 ? crs.Wkt : crs.Name);
        srs.SetAxisMappingStrategy(OSGeo.OSR.AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
        return srs;
    }
}

// The ONE CityJSON metadata-and-appearance admission: the referenceSystem URN and the Appearance texture roster are
// both header-grade facts, and three inline re-derivations of the same metadata read across the geometry and texture
// arms drifted the day the spec moved. metadata.referenceSystem is an OGC URN (urn:ogc:def:crs:EPSG::25832) the seam
// ProjectedCrs.Of admits on its Name carrier; a blank one yields the no-CRS None because Of faults an all-blank
// product, so the blank case never enters admission.
public sealed record CityJsonHeader(Option<ProjectedCrs> Crs, Seq<SurfaceTexture> Textures) {
    public static CityJsonHeader Of(CityJSON.CityJsonDocument document, Op key) =>
        new(Optional(document.Metadata)
                .Bind(static meta => Optional(meta.ReferenceSystem))
                .Filter(static system => system.Length > 0)
                .Bind(system => ProjectedCrs.Of(system, "", "", "", key)
                    .Match(Succ: static c => Some(c), Fail: static _ => Option<ProjectedCrs>.None)),
            Textures(document));

    // Appearance.Textures lowers each Texture onto the Semantics/appearance#APPEARANCE_PROJECTION SurfaceTexture
    // roster BESIDE the summary: Image rides the URL payload case, and the theme ORDINAL is the coordinate set — a
    // CityJSON geometry's Texture map is keyed by theme and each theme indexes its own run of VerticesTexture, so
    // roster position and UV pool partition are one axis. CityJSON declares ONE WrapMode for both axes, so both
    // read one row, and `mirror` now survives: seam TextureWrap carries MirroredRepeat where the retired boolean
    // pair could only report it as plain repeat.
    static Seq<SurfaceTexture> Textures(CityJSON.CityJsonDocument document) =>
        Optional(document.Appearance).Map(static appearance => toSeq(appearance.Textures)
            .Map(static (texture, set) => Wrap(texture.WrapMode) switch {
                var wrap => (SurfaceTexture)new SurfaceTexture.Url(
                    TextureMode.From(Optional(texture.TextureType).Map(static t => t.ToString()).IfNone("")),
                    WrapU: wrap,
                    WrapV: wrap,
                    Uv: Option<UvTransform>.None,
                    CoordinateSet: set,
                    Reference: Optional(texture.Image).IfNone("")),
            })
            .ToSeq())
            .IfNone(Seq<SurfaceTexture>());

    // NAMED LOSS: CityJSON's `none` declares that the texture is not applied outside [0,1] at all, and the seam
    // roster carries no border row, so it lands on the nearest sampler policy the vocabulary states — the edge
    // texel clamps where CityJSON left the surface untextured.
    static TextureWrap Wrap(CityJSON.TextureWrapMode mode) => mode switch {
        CityJSON.TextureWrapMode.wrap => TextureWrap.Repeat,
        CityJSON.TextureWrapMode.mirror => TextureWrap.MirroredRepeat,
        _ => TextureWrap.ClampToEdge,
    };
}
```

## [05]-[RESEARCH]

(none)
