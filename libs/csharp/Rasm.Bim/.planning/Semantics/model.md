# [BIM_GEOMODEL]

`GeoModel` is the indexed SET over the `Semantics/feature#GEO_FEATURE` row and the whole planar algebra computed on it: one `NtsGeometryServices.Instance` precision/SRID root, ONE `STRtree` bounding-envelope broad phase over the admitted ordinals, the `pocketken.H3` DGGS bucket keying bit-for-bit the same 64-bit v4 cell the `Rasm.Persistence` `h3-pg` server index computes, the `GeoPredicate`-parameterized DE-9IM join, the k-NN clash witness, the linear-referencing chainage answer, the linework assembly, the setback and dissolve overlays, the bounding-form family, and the MVT LOD pyramid the `Rasm.AppUi/Charts` Mapsui overlays consume. `GeoModel.Of` is the ONE admission — `GeometryFixer.Fix` runs exactly once and publishes its census — so no downstream leg re-scans validity.

Every read that returns features returns `Semantics/feature#GEO_FEATURE` rows; every projection returns a header-less `GraphDelta` the `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` fold composes. `GeoModel.Project` is a PARTITIONING admission: one malformed feature refuses on its own ordinal and the import lands every feature that admitted, because the folder's own resilience law says an unrecognized feature must never abort a site import and a `Traverse` that exits on the first defect publishes neither half. `GeoPredicate`, `GeoBound`, the `GeoAssembly`/`GeoAssembled` pair, and the `LinearProbe`/`LinearAnswer` pair are the four closed families the algebra dispatches; `TilePolicy` and `GeoTiles` own the MVT integer grid and its bytes.

## [01]-[INDEX]

- [02]-[GEO_ALGEBRA]: four closed families — `GeoPredicate` the DE-9IM table with its typed mask, `GeoBound` the bounding-form table with its budget, `GeoAssembly`/`GeoAssembled` the assembly pair, `LinearProbe`/`LinearAnswer` the chainage pair.
- [03]-[GEO_MODEL]: `GeoModel` the indexed feature set — admission and repair census, broad-then-narrow join, three-valued point location, k-NN and cross-tree clash, linear referencing, assembly, overlays, the DGGS cover, the LOD pyramid fold, and the partitioning projection.
- [04]-[TILE_PYRAMID]: `TilePolicy` the MVT integer-grid value and `GeoTiles` the byte codec and TileJSON catalog.

## [02]-[GEO_ALGEBRA]

- Owner: `GeoPredicate` the closed DE-9IM `[SmartEnum<string>]` delegate table dispatching the `IPreparedGeometry` narrow phase, `De9imMask` the admitted nine-character relation pattern its open tail reads; `GeoBound` the bounding-form delegate table over the dissolved set, `HullScale`/`HullBudget` the scale-qualified free scalar its rows read; `GeoAssembly`/`GeoAssembled` the linework assembly request-and-evidence pair; `LinearProbe`/`LinearAnswer` the chainage request-and-answer pair.
- Cases: `GeoAssembly` arms `Faces` (noded linework to polygons), `Merged` (degree-2 chains to maximal lines), `Sequenced` (merged lines ordered into one traversal), `Dissolved` (duplicate linework collapsed to a single-edge graph), `Cells` (Voronoi dual, optionally clipped); `GeoAssembled` arms `Faces` (polygons WITH the dangle and cut-edge residue), `Lines`, `Cells`; `LinearProbe` arms `Locate`, `Place`, `Carve`, `Edge`; `LinearAnswer` arms `At`, `Site`, `Run`.
- Law: `GeoAssembled` carries exactly the evidence its own algorithm produces — polygonization alone yields a residue, and one shared record with empty residue columns on the other arms publishes a measurement no algorithm took; `LinearAnswer.At` carries the DURABLE `LinearLocation` beside the derived station, because a length index shifts under any vertex-count change while the location still addresses the same place on the same segment.
- Entry: `GeoPredicate.Holds(prepared, candidate, mask)` — the nine named rows discard the mask, `Matches` reads it; `De9imMask.Of(pattern, key)` admits a pattern and `De9imMask.Any` is the value a named row takes; `GeoBound.Of(dissolved, members, scale, key)` constructs the bound under the row's own declared `HullScale`; `GeoAssembly` and `LinearProbe` select through their generated `Switch`.
- Packages: `NetTopologySuite`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new planar relation is one `GeoPredicate` row over the existing delegate column, and an ad-hoc relation is a `De9imMask` value on the `Matches` row — never a new row per experiment; a new bounding form is one `GeoBound` row declaring which `HullScale` it reads; a new linework assembly is one `GeoAssembly` case; a new chainage question is one `LinearProbe` case with its `LinearAnswer` peer, never a member beside the model's own reads, and a side-of-centreline discrimination is that same pair growth rather than a sign fabricated at the stamp.
- Boundary: `Within` is the one relation the prepared surface lacks and it reads the probe-side inverse off `IPreparedGeometry.Geometry`; a malformed relation pattern REFUSES at admission rather than silently matching nothing, because a nine-character grammar mistyped by one glyph is a query that returns an empty set and reads as an answer; the hull budget is SCALE-QUALIFIED because the family's operators split between scale-free ratios and absolute edge lengths — handing an alpha shape a `0.05` meant as a ratio produced a degenerate shape indistinguishable from a legitimate one; `Edge` reads the SAME `BufferParameters` policy the setback carve does, so one offset policy serves the folder.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Globalization;
using System.Text.Json;
using H3;
using H3.Algorithms;
using H3.Extensions;
using LanguageExt;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Dissolve;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO.VectorTiles;
using NetTopologySuite.IO.VectorTiles.Mapbox;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Operation.Valid;
using NetTopologySuite.Simplify;
using NetTopologySuite.Triangulate;
using Rasm;
using Rasm.Domain;
using Rasm.Element.Geospatial;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;
using OgcDimension = NetTopologySuite.Geometries.Dimension;

namespace Rasm.Bim;

// --- [TYPES] --------------------------------------------------------------------------------
// The DE-9IM pattern as an ADMITTED value: nine glyphs drawn from the intersection-matrix alphabet, so a mistyped
// mask refuses at construction instead of joining nothing and reading as an empty answer.
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class De9imMask {
    const int Cells = 9;
    const string Alphabet = "TF*012";

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (value is not { Length: Cells } || !value.All(static glyph => Alphabet.Contains(char.ToUpperInvariant(glyph)))) {
            validationError = new ValidationError("de9im-mask-grammar");
            return;
        }
        value = value.ToUpperInvariant();
    }

    // The pattern every named row takes and discards — nine wildcards match every relation, so the value is inert
    // wherever the row never reads it and never a null threaded through a delegate column.
    public static readonly De9imMask Any = Create("*********");

    public static Fin<De9imMask> Of(string pattern, Op key) =>
        Validate(pattern, out De9imMask? mask) is null && mask is { } admitted
            ? Fin.Succ(admitted)
            : Fin.Fail<De9imMask>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "geo-format-lane", "de9im", "mask", pattern })));
}

// GeoPredicate rows form the closed DE-9IM join vocabulary as a delegate-column policy table: the narrow phase
// dispatches the row, so a new spatial relation is one row and never a per-predicate join family. Within is the one
// relation the prepared surface lacks; it reads the probe-side inverse off IPreparedGeometry.Geometry. Matches is
// the OPEN tail on the SAME entry — the mask rides the free argument the named rows discard, exactly as GeoBound's
// budget does — so an ad-hoc relation never mints a vocabulary row and never a second join member.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class GeoPredicate {
    public static readonly GeoPredicate Intersects       = new("intersects",       static (p, g, _) => p.Intersects(g));
    public static readonly GeoPredicate Contains         = new("contains",         static (p, g, _) => p.Contains(g));
    public static readonly GeoPredicate ContainsProperly = new("containsproperly", static (p, g, _) => p.ContainsProperly(g));
    public static readonly GeoPredicate Covers           = new("covers",           static (p, g, _) => p.Covers(g));
    public static readonly GeoPredicate CoveredBy        = new("coveredby",        static (p, g, _) => p.CoveredBy(g));
    public static readonly GeoPredicate Crosses          = new("crosses",          static (p, g, _) => p.Crosses(g));
    public static readonly GeoPredicate Overlaps         = new("overlaps",         static (p, g, _) => p.Overlaps(g));
    public static readonly GeoPredicate Touches          = new("touches",          static (p, g, _) => p.Touches(g));
    public static readonly GeoPredicate Within           = new("within",           static (p, g, _) => p.Geometry.Within(g));
    public static readonly GeoPredicate Matches          = new("matches",          static (p, g, mask) => p.Geometry.Relate(g, mask.Value));

    [UseDelegateFromConstructor]
    public partial bool Holds(IPreparedGeometry prepared, Geometry candidate, De9imMask mask);
}

// The two readings a hull budget can carry, because the family's operators split between them: a ratio is scale-free
// and a length is in the set's own units. Carrying one bare double meant an alpha shape and a concave-hull ratio read
// the same argument and neither could refuse the other's value.
[SmartEnum<string>]
public sealed partial class HullScale {
    public static readonly HullScale Fraction = new("fraction");
    public static readonly HullScale Length = new("length");
}

public sealed record HullBudget(HullScale Scale, double Value) {
    public static HullBudget Ratio(double value) => new(HullScale.Fraction, value);
    public static HullBudget Metres(double value) => new(HullScale.Length, value);

    // The parameterless rows discard the budget, so the identity is the value a Convex or Rectangle call states.
    public static readonly HullBudget None = new(HullScale.Fraction, 0.0);
}

// An axis-aligned Envelope is correct only for a north-up site: a minimum-area ORIENTED rectangle is what a setback
// envelope, a hoarding line, or a crane-radius check reads. Two geometries reach every row because the family splits
// on what it operates over — the DISSOLVED union for a single-body hull, the MEMBER collection for the polygon-set
// operators, where the single-geometry hull over a dissolved multi-parcel set bridged parcels the site never owned.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class GeoBound {
    public static readonly GeoBound Convex     = new("convex",     HullScale.Fraction, static (dissolved, _, _) => dissolved.ConvexHull());
    public static readonly GeoBound Rectangle  = new("rectangle",  HullScale.Fraction, static (dissolved, _, _) => MinimumDiameter.GetMinimumRectangle(dissolved));
    public static readonly GeoBound Width      = new("width",      HullScale.Fraction, static (dissolved, _, _) => MinimumDiameter.GetMinimumDiameter(dissolved));
    public static readonly GeoBound Circle     = new("circle",     HullScale.Fraction, static (dissolved, _, _) => new MinimumBoundingCircle(dissolved).GetCircle());
    public static readonly GeoBound Concave    = new("concave",    HullScale.Fraction, static (dissolved, _, scale) => ConcaveHull.ConcaveHullByLengthRatio(dissolved, scale, true));
    public static readonly GeoBound Simplified = new("simplified", HullScale.Fraction, static (dissolved, _, scale) => PolygonHullSimplifier.HullByAreaDelta(dissolved, true, scale));
    // The polygon-SET operators read the members: ConcaveHullOfPolygons keeps every parcel's own boundary and fills
    // only the gaps between them, where the dissolved single-geometry hull erases the interior boundaries first.
    public static readonly GeoBound Parcels    = new("parcels",    HullScale.Length,   static (_, members, scale) => ConcaveHullOfPolygons.ConcaveHullByLength(members, scale, false, true));
    public static readonly GeoBound Filled     = new("filled",     HullScale.Length,   static (_, members, scale) => ConcaveHullOfPolygons.ConcaveFillByLength(members, scale));
    public static readonly GeoBound Alpha      = new("alpha",      HullScale.Length,   static (_, members, scale) => ConcaveHull.AlphaShape(members, scale, true));

    public HullScale Scale { get; }

    [UseDelegateFromConstructor]
    public partial Geometry Of(Geometry dissolved, Geometry members, double scale);
}

// GeoAssembly rosters the linework-assembly requests — one closed request rides ONE Assemble entry, so a new assembly
// is a case and never a fourth sibling builder; every case runs against the canonical GeoServices factory, so
// assembled geometry carries the precision model and SRID its source set did.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeoAssembly {
    private GeoAssembly() { }

    // Noded linework -> faces. A cadastral or utility delivery ships lot lines, kerb lines, or a pipe network as
    // LINES; the parcels, blocks, and service areas they bound exist only after polygonization.
    public sealed record Faces : GeoAssembly;

    // Degree-2 chains merged into maximal lines — the fragmented-centreline repair every OSM-derived or
    // tile-clipped corridor set needs before it can be linearly referenced at all.
    public sealed record Merged : GeoAssembly;

    // Merged lines ORDERED into one traversal — the sequence a chainage origin, a route emit, or a survey
    // walk reads, where a merge alone leaves the pieces in arbitrary order.
    public sealed record Sequenced : GeoAssembly;

    // Duplicate and overlapping linework collapsed to a single-edge graph. A cadastral merge of two adjacent
    // extracts ships every shared lot line TWICE, and Merged cannot see the duplication (both copies are
    // degree-2 chains, so it merges them into two coincident lines); dissolving first is the repair.
    public sealed record Dissolved : GeoAssembly;

    // Voronoi dual over the feature sites, optionally clipped to a study extent — the medial-axis and
    // service-area input (nearest-facility catchments, spacing analysis) the substrate names by that role.
    public sealed record Cells(Option<Envelope> Clip) : GeoAssembly;
}

// Dangles name linework that bounded nothing and CutEdges linework traversed twice — two diagnostics that say WHY a
// parcel set came back short, which a bare polygon list cannot.
[Union]
public abstract partial record GeoAssembled {
    public sealed record Faces(Seq<GeoFeature> Polygons, Seq<Geometry> Dangles, Seq<Geometry> CutEdges) : GeoAssembled;
    public sealed record Lines(Seq<GeoFeature> Parts) : GeoAssembled;
    public sealed record Cells(Seq<GeoFeature> Duals) : GeoAssembled;
}

// LinearProbe rosters the chainage algebra IFC4.3's linear-infrastructure facilities address themselves by.
// GeoClassifier mints IfcRoad, IfcRailway, IfcBridge, and IfcMarineFacility — four facility classes whose native
// addressing system is a station along a centreline rather than a coordinate pair — and every question a road or rail
// project asks ("what sits at station 2+400", "carve the deck between these two stations", "how far off centreline is
// this drain", "where does the kerb run") is a linear-referencing operation. Each probe case selects its answer, so
// one corridor operation serves the family.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinearProbe {
    private LinearProbe() { }

    public sealed record Locate(Coordinate Point) : LinearProbe;
    // Locate's inverse on the SAME owner — station-and-offset back to a coordinate — because a correspondence
    // whose domain admits an inverse carries both directions rather than splitting into direction-named siblings.
    public sealed record Place(double Station, double Offset) : LinearProbe;
    public sealed record Carve(double From, double To) : LinearProbe;
    // Parallel line at a signed offset — a kerb, a right-of-way edge, a track centre in a multi-track corridor.
    public sealed record Edge(double Distance) : LinearProbe;
}

// A located probe additionally carries the DURABLE LinearLocation (a component index plus a segment fraction),
// because a length index shifts under any vertex-count change the corridor takes — a densify before reprojection, a
// per-zoom simplify, a re-import — while the location still addresses the same place on the same segment.
[Union]
public abstract partial record LinearAnswer {
    public sealed record At(double Station, double Offset, LinearLocation Durable) : LinearAnswer;
    public sealed record Site(Coordinate Coordinate) : LinearAnswer;
    public sealed record Run(Geometry Line) : LinearAnswer;
}
```

## [03]-[GEO_MODEL]

- Owner: `GeoModel` the admitted feature set under one precision/SRID root carrying the built-once `STRtree` broad phase over ordinals, the per-feature `IndexedPointInAreaLocator` memo, the `Repairs` census, the `GeoPredicate`-parameterized `SpatialJoin`, the three-valued `Locate`, the `Nearest` k-NN witness and the cross-tree `Clash`, the `Along` linear-referencing answer, the `Assemble` linework assembly, the `Bound`/`Setback`/`Dissolve` overlays, the H3 `Bucket`/`Cover`/`Within` coarse index, the `ToTiles` MVT LOD pyramid, and the partitioning `Project`; `GeoRepair` one row of the admission census; `GeoClash` the cross-model closest-pair evidence; `GeoImport`/`GeoRefusal` the partitioning projection outcome.
- Law: `Of` is the ONE admission and `GeoModel` is a `sealed class` precisely so the compiler mints no `with` — a copy aliases the built-once index against a different feature set and surfaces stale broad-phase candidates, which the `Graph/element#ELEMENT_GRAPH` frozen-snapshot guard forbids; features are repaired ONCE at admission, so every downstream read is a double-admission if it re-scans validity.
- Entry: `GeoModel.Of(features)` repairs, indexes, and publishes the census; `SpatialJoin(probe, predicate, mask)` runs one fused broad-and-narrow pass; `Locate(point)` answers the three-valued `Location` per areal candidate; `Nearest(probe, k)` stamps each hit with its component-indexed witness; `Clash(other, budget)` answers the cross-tree closest pair inside a budget; `Along(corridor, probe, key)` answers chainage; `Assemble(assembly, key)` folds linework; `Bound(form, budget, key)`, `Setback(parcel, distance, policy)`, and `Dissolve()` compose the overlays; `Bucket(resolution, key)`, `Cover(probe, crs, resolution, ring, key, test, shards)`, and `Within(cover, resolution, key)` are the DGGS index; `ToTiles(route, policy, key)` folds the LOD pyramid; `Corridors(reference, schema, source, reach, key)` resolves the indexed alignment roster; `Project(reference, schema, source, reach, token, ctx)` partitions the import.
- Auto: the LOD pass simplifies each zoom's areal members as ONE coverage AFTER `CoverageValidator` proves they form one — `CoverageSimplifier` reassigns a shared edge to both polygons, so it silently repairs a set it never verified and a gapped or overlapping parcel set simplifies into geometry neither source polygon had; `Cover` tests `IsTransMeridian` BEFORE any fill and admits its result through `GeoCover.Of`, so an antimeridian-crossing site refuses by name rather than mis-covering and an uncanonical cell list never reaches the binary-search membership test; `Project` resolves the corridor roster ONCE before the fold, so every stamped occurrence stations against the same reprojected centrelines.
- Receipt: `Repairs` is the admission's own census — one typed `TopologyValidationError` per feature the fixer had to touch, so an import states which features arrived broken and why, and an empty census means every feature admitted valid rather than that nothing was checked; `Nearest` carries `GeometryLocation` witnesses whose component and segment index name WHICH part of a multi-part feature the gap sits on; `GeoImport` states the accepted count, the merged delta, and every refusal beside its own feature ordinal; the `STRtree` broad phase and the H3 `Bucket`/`Cover` bucket key the same server-side cell, so an in-process membership test and the `h3-pg` SQL prefilter agree.
- Packages: `NetTopologySuite`, `pocketken.H3`, `Rasm.Element`, `Rasm`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new overlay op is one `Geometry` instance method on the existing algebra; a new DGGS resolution, ring radius, or parallel-fill shard width is an argument, never a member; a new corridor reach posture is a `Corridors` argument; never a hand-rolled planar intersection, never a second R-tree beside the `STRtree`, and never a per-feature-kind read family.
- Boundary: the spatial index stays `STRtree` because the k-NN and cross-tree legs bind it CONCRETELY — `NearestNeighbour` and `IsWithinDistance` are not on the `ISpatialIndex<T>` floor `Quadtree` and `HPRtree` share — so a Hilbert-packed swap forfeits both, and a second index beside it is the deleted form; the tree indexes ORDINALS beside their rows so the point-locator memo, the repair census, and every refusal name one position vocabulary; the DGGS cell algebra is `pocketken.H3`'s under the v4-canonical spellings and a live mutable `H3Index` never stores; the polyfill acceptance predicate is LOAD-BEARING and its package default is wrong for this contract — under `VertexTestMode.Center` a cell joins the cover only when its CENTRE falls inside the probe, so any probe smaller than one cell yields ZERO cells and the caller reads "no site context here" for a probe sitting squarely on a cell, which is why a REGION KEY defaults to `Any`; `Bucket` keys features by ANCHOR cell, so `Contains` must go through the canonical binary search rather than an exact-key lookup that misses every cell the compaction rolled up; the empty-set arm of `Dissolve` mints the valid empty polygon because `OverlayNGRobust.Union` of zero geometries returns NULL; the setback carve reads the mitred `BufferParameters` because a setback is a LEGAL offset whose buildable region is bounded by lines PARALLEL to each lot line — the round-join default replaces every corner with an eight-segment arc and returns less area than the code requires with no diagnostic; the mitre limit is bounded because an acute corner mitres to an unbounded spike; the token gates the PER-FEATURE boundary and nothing deeper, because each feature's reprojection is a single `ProjNET`/OSR call publishing no interrupt and a claim of finer abort is the overclaim the folder's native-lane ruling names; both long folds are SPAN-grade under `[MODEL_SLOT_RULING]` — features and pyramid levels mint unbounded, so a per-feature instrument multiplies every series by the source's row count.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// One row of the admission census: the feature's position in the admitted set and the typed defect that caused the
// repair. The bare tuple this replaces gave the two columns no name at either end.
public sealed record GeoRepair(int Index, TopologyValidationError Defect);

// The cross-model closest pair WITH its witness: Left and Right name the two features, Witness the two
// GeometryLocations whose component and segment index say which part of each carried the gap.
public sealed record GeoClash(GeoFeature Left, GeoFeature Right, double Gap, GeometryLocation[] Witness);

// One refused feature: the ordinal it sat at in the admitted set and the typed fault that refused it. Threading the
// ordinal keeps a caller from parsing it back out of a diagnostic token.
public sealed record GeoRefusal(int Index, Error Fault);

// The site import's whole outcome: the merged delta of every feature that admitted, the count it merged, and one
// refusal row per feature that did not. A first-defect Traverse published NEITHER half — a single malformed parcel
// in a cadastral extract returned one error and zero nodes, against the folder's own resilience law that an
// unrecognized feature must never abort an import.
public sealed record GeoImport(GraphDelta Delta, int Accepted, Seq<GeoRefusal> Refused) {
    public static readonly GeoImport Empty = new(GraphDelta.Empty, 0, Seq<GeoRefusal>());

    public GeoImport Accept(GraphDelta delta) => this with { Delta = Delta.Merge(delta), Accepted = Accepted + 1 };

    public GeoImport Refuse(int index, Error fault) => this with { Refused = Refused.Add(new GeoRefusal(index, fault)) };
}

// Class, not record: a compiler-generated `with` would alias the built-once STRtree against a different Features set,
// so the copy the private guard used to THROW on is now unrepresentable and the guard has nothing to say.
public sealed class GeoModel {
    GeoModel(Seq<GeoFeature> features, Seq<GeoRepair> repairs) {
        Features = features;
        Repairs = repairs;
        index = new STRtree<(int Ordinal, GeoFeature Feature)>();
        features.Iter((ordinal, f) => index.Insert(f.Bounds, (ordinal, f)));
    }

    public Seq<GeoFeature> Features { get; }

    public Seq<GeoRepair> Repairs { get; }

    // Default STR node capacity (10): the ctor arg is per-NODE fan-out, not item count — seeding it with the feature
    // count flattens the tree to one node and degrades every Query to a linear scan.
    readonly STRtree<(int Ordinal, GeoFeature Feature)> index;

    // Built on FIRST Locate under the locator's own synchronized initializer and held per ordinal, so a survey of
    // many points against one parcel set pays the segment-index build once per parcel rather than once per probe.
    readonly AtomHashMap<int, IndexedPointInAreaLocator> locators = AtomHashMap<int, IndexedPointInAreaLocator>();

    static readonly GeoDistance<(int Ordinal, GeoFeature Feature)> Placed = new(static row => row.Feature.Geometry);

    public static GeoModel Of(Seq<GeoFeature> features) {
        var audited = features.Map(static (f, i) => (Index: i, Repair: f.Repair()));
        return new GeoModel(
            audited.Map(static row => row.Repair.Feature),
            audited.Choose(static row => row.Repair.Defect.Map(defect => new GeoRepair(row.Index, defect))));
    }

    // --- [BROAD_AND_NARROW]
    // ONE fused pass: STRtree.Query(Envelope, IItemVisitor<T>) STREAMS each envelope candidate to the visitor, so the
    // narrow-phase test runs as the tree walks and no per-join candidate IList materializes. The DE-9IM relation is a
    // policy value and the mask its open tail, so neither an extra join member nor a second entry exists.
    public Seq<GeoFeature> SpatialJoin(Geometry probe, GeoPredicate predicate, De9imMask mask) {
        var prepared = PreparedGeometryFactory.Prepare(probe);
        var visitor = new Narrowed(row => predicate.Holds(prepared, row.Feature.Geometry, mask));
        index.Query(probe.EnvelopeInternal, visitor);
        return visitor.Hits.Map(static row => row.Feature);
    }

    sealed class Narrowed(Func<(int Ordinal, GeoFeature Feature), bool> holds) : IItemVisitor<(int Ordinal, GeoFeature Feature)> {
        Seq<(int Ordinal, GeoFeature Feature)> hits = Seq<(int, GeoFeature)>();
        public Seq<(int Ordinal, GeoFeature Feature)> Hits => hits;
        public void VisitItem((int Ordinal, GeoFeature Feature) item) { if (holds(item)) { hits = hits.Add(item); } }
    }

    // Point-in-area is THREE-valued, and the predicate rows answer two: a probe on a shared lot line is Boundary to
    // both parcels, and a bool forces the caller to invent which one owns it. IndexedPointInAreaLocator is the
    // one-area-many-points counterpart to the prepared probe's one-probe-many-areas — the prepared surface buys
    // nothing for a bare point, where the locator's segment index answers every subsequent probe against the same
    // parcel in log time.
    public Seq<(GeoFeature Feature, Location Where)> Locate(Coordinate probe) {
        var visitor = new Narrowed(static row => row.Feature.Geometry is IPolygonal or LinearRing);
        index.Query(new Envelope(probe), visitor);
        return visitor.Hits.Map(row => (row.Feature, Locator(row).Locate(probe)));
    }

    IndexedPointInAreaLocator Locator((int Ordinal, GeoFeature Feature) row) =>
        locators.FindOrAdd(row.Ordinal, () => new IndexedPointInAreaLocator(row.Feature.Geometry));

    // --- [PROXIMITY]
    // Each hit carries the DistanceOp closest-pair witness as GeometryLocations, so a clash report names the
    // component and segment the gap sits on rather than two bare coordinates a reader must re-locate by hand. The
    // empty-model guard is load-bearing: NearestNeighbour on an empty STRtree throws, never returns empty.
    public Seq<(GeoFeature Feature, double Distance, GeometryLocation[] Witness)> Nearest(Geometry probe, int k) =>
        Features.IsEmpty
            ? Seq<(GeoFeature, double, GeometryLocation[])>()
            : toSeq(index.NearestNeighbour(probe.EnvelopeInternal, (-1, new GeoFeature(probe, new AttributesTable(), Option<ProjectedCrs>.None)), Placed, k))
                .Map(row => {
                    GeometryLocation[] witness = new DistanceOp(probe, row.Feature.Geometry).NearestLocations();
                    return (row.Feature, witness[0].Coordinate.Distance(witness[1].Coordinate), witness);
                });

    // Cross-tree proximity between two site models: IsWithinDistance short-circuits at the FIRST pair inside the
    // budget, so a federation check that only asks "does anything breach" never computes an exact separation, and
    // NearestNeighbour(STRtree, IItemDistance) descends BOTH packed trees once where a per-feature k-NN re-descends
    // the other tree once per probe.
    public Option<GeoClash> Clash(GeoModel other, double budget) =>
        Features.IsEmpty || other.Features.IsEmpty || !index.IsWithinDistance(other.index, Placed, Math.Abs(budget))
            ? None
            : toSeq(index.NearestNeighbour(other.index, Placed)) is [var left, var right]
                ? Some(Witnessed(left.Feature, right.Feature))
                : None;

    static GeoClash Witnessed(GeoFeature left, GeoFeature right) {
        GeometryLocation[] witness = new DistanceOp(left.Geometry, right.Geometry).NearestLocations();
        return new GeoClash(left, right, witness[0].Coordinate.Distance(witness[1].Coordinate), witness);
    }

    // --- [LINEAR_REFERENCING]
    // Stations arrive CLAMPED by LengthIndexedLine's own contract, so an out-of-range query answers at the terminus
    // rather than faulting. Non-lineal corridors have no chainage at all: an areal or point feature RAILS rather than
    // answering off its boundary, because a station measured around a polygon ring is a number with no alignment
    // behind it and a consumer cannot tell it from a real one.
    public static Fin<LinearAnswer> Along(GeoFeature corridor, LinearProbe probe, Op key) =>
        corridor.Geometry.Dimension != OgcDimension.Curve
            ? Fin.Fail<LinearAnswer>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Codec, string.Join(':', new object?[] { "geo-linear-noncurve", corridor.Kind.ToString() })))
            : Fin.Succ(probe.Switch(
                state: corridor.Geometry,
                locate: static (line, p) => {
                    var indexed = new LengthIndexedLine(line);
                    double station = indexed.Project(p.Point);
                    return (LinearAnswer)new LinearAnswer.At(
                        station,
                        p.Point.Distance(indexed.ExtractPoint(station)),
                        new LocationIndexedLine(line).Project(p.Point));
                },
                place:  static (line, p) => new LinearAnswer.Site(new LengthIndexedLine(line).ExtractPoint(p.Station, p.Offset)),
                carve:  static (line, p) => new LinearAnswer.Run(new LengthIndexedLine(line).ExtractLine(p.From, p.To)),
                edge:   static (line, p) => new LinearAnswer.Run(new OffsetCurve(line, p.Distance, OffsetParameters).GetCurve())));

    // --- [ASSEMBLY]
    // Polygonization keeps its residue because dangles and cut edges explain a short parcel count; merge and sequence
    // share one merged run because sequencing IS ordering that run, and re-merging inside the sequence arm would fold
    // the linework twice.
    public Fin<GeoAssembled> Assemble(GeoAssembly assembly, Op key) =>
        key.Catch(() => assembly.Switch(
            state: Features,
            faces: static (features, _) => {
                var polygonizer = new Polygonizer();
                features.Iter(f => polygonizer.Add(f.Geometry));
                return (GeoAssembled)new GeoAssembled.Faces(
                    Carry(features, polygonizer.GetPolygons().AsIterable().ToSeq()),
                    polygonizer.GetDangles().AsIterable().Map(static line => (Geometry)line).ToSeq(),
                    polygonizer.GetCutEdges().AsIterable().Map(static line => (Geometry)line).ToSeq());
            },
            merged:    static (features, _) => new GeoAssembled.Lines(Carry(features, Merge(features))),
            sequenced: static (features, _) => new GeoAssembled.Lines(Carry(features, Sequence(Merge(features)))),
            dissolved: static (features, _) => new GeoAssembled.Lines(Carry(features,
                Parts(LineDissolver.Dissolve(GeoServices.Factory.BuildGeometry(features.Map(static f => f.Geometry)))))),
            cells:     static (features, c) => {
                var builder = new VoronoiDiagramBuilder();
                builder.SetSites(GeoServices.Factory.BuildGeometry(features.Map(static f => f.Geometry)));
                c.Clip.Iter(extent => builder.ClipEnvelope = extent);
                return new GeoAssembled.Cells(Carry(features, Parts(builder.GetDiagram(GeoServices.Factory))));
            }));

    static Seq<Geometry> Merge(Seq<GeoFeature> features) {
        var merger = new LineMerger();
        features.Iter(f => merger.Add(f.Geometry));
        return merger.GetMergedLineStrings().AsIterable().ToSeq();
    }

    static Seq<Geometry> Sequence(Seq<Geometry> merged) {
        var sequencer = new LineSequencer();
        merged.Iter(sequencer.Add);
        return Parts(sequencer.GetSequencedLineStrings());
    }

    static Seq<Geometry> Parts(Geometry collection) =>
        Enumerable.Range(0, collection.NumGeometries).AsIterable().Map(collection.GetGeometryN).ToSeq();

    // Carry re-seats topology the ASSEMBLY derived, so each result inherits the SOURCE CRS the set carried and an
    // EMPTY attribute table — never the first input's attributes, which would attribute a merged corridor or a
    // polygonized block to whichever fragment happened to sort first.
    static Seq<GeoFeature> Carry(Seq<GeoFeature> features, Seq<Geometry> assembled) =>
        assembled.Map(geometry => new GeoFeature(geometry, new AttributesTable(),
            features.Head.Bind(static f => f.SourceCrs)));

    // --- [OVERLAY]
    // The budget's scale is proved against the row's OWN declared reading exactly once, so every delegate reads a
    // bare double it can only have been handed in its own units.
    public Fin<Geometry> Bound(GeoBound form, HullBudget budget, Op key) =>
        form.Scale == budget.Scale
            ? Fin.Succ(form.Of(Dissolve(), GeoServices.Factory.BuildGeometry(Features.Map(static f => f.Geometry)), budget.Value))
            : Fin.Fail<Geometry>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "geo-bound-budget", form.Key, form.Scale.Key, budget.Scale.Key })));

    public static readonly BufferParameters OffsetParameters = new() {
        JoinStyle = JoinStyle.Mitre,
        MitreLimit = 2.0,
        EndCapStyle = EndCapStyle.Flat,
    };

    public Geometry Setback(Geometry parcel, double distance, Option<BufferParameters> policy = default) =>
        GeometryFixer.Fix(parcel).Buffer(-Math.Abs(distance), policy.IfNone(OffsetParameters)).Difference(Dissolve());

    public Geometry Dissolve() =>
        Features.IsEmpty
            ? GeoServices.Factory.CreatePolygon()
            : OverlayNGRobust.Union(Features.Map(static f => f.Geometry).ToArray());

    // --- [DGGS]
    // A regional query is a cell-membership read, never a per-feature scan; a cell-less feature (invalid anchor) drops.
    public Fin<HashMap<ulong, Seq<GeoFeature>>> Bucket(int resolution, Op key) =>
        Features.Traverse(f => f.Cell(resolution, key).Map(cell => (Cell: cell, Feature: f))).As()
            .Map(static pairs => pairs.Fold(
                HashMap<ulong, Seq<GeoFeature>>(),
                static (acc, pair) => pair.Cell.Match(
                    Some: id => acc.AddOrUpdate(id, Some: s => s.Add(pair.Feature), None: () => Seq(pair.Feature)),
                    None: () => acc)));

    // In-process twin of the Persistence `h3_cell = ANY(@cells)` prefilter, and the ONE join site that reads a cover.
    public Fin<Seq<GeoFeature>> Within(GeoCover cover, int resolution, Op key) =>
        Bucket(resolution, key).Map(buckets => toSeq(buckets)
            .Filter(entry => cover.Contains(entry.Key))
            .Bind(static entry => entry.Value));

    // A probe crossing the antimeridian has a lon/lat envelope spanning nearly the whole globe, so the fill either
    // covers a hemisphere it never touched or returns the wrong side entirely — a silent, geography-dependent wrong
    // answer the region key then carries into the SQL prefilter. IsTransMeridian is the package's own detection and
    // it runs BEFORE any fill. ParallelFill shards the same cell set by bounding envelope for a continental probe;
    // shard absence is the single-threaded fill, never a fabricated width.
    public static Fin<GeoCover> Cover(
        Geometry probe, Option<ProjectedCrs> crs, int resolution, int ring, Op key,
        VertexTestMode test = VertexTestMode.Any, Option<int> shards = default) =>
        GeoServices.Wgs84
            .Bind(frame => new GeoFeature(probe, new AttributesTable(), crs).Reproject(frame, key))
            .Bind(wgs => wgs.Geometry.IsTransMeridian()
                ? Fin.Fail<GeoCover>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "geo-cover-rejected", "transmeridian", wgs.Bounds.MinX.ToString("F6", CultureInfo.InvariantCulture), wgs.Bounds.MaxX.ToString("F6", CultureInfo.InvariantCulture) })))
                : Fin.Succ(wgs))
            .Bind(wgs => {
                var fill = toSeq(shards.Match(
                    Some: width => wgs.Geometry.ParallelFill(resolution, test, width),
                    None: () => wgs.Geometry.Fill(resolution, test)));
                var expanded = ring > 0
                    ? fill.Bind(cell => cell.GridDiskDistances(ring).AsIterable().Map(static r => r.Index).ToSeq())
                    : fill;
                return GeoCover.Of(expanded.Distinct().CanonicalizeCells(), key);
            });

    // --- [TILE_FOLD]
    // The set reprojects onto Wgs84 through the ONE datum leg, the route delegate assigns each feature its
    // (zoom, layer) slots, each slot's geometry passes the zoom-matched simplify, and the tupled stream folds through
    // the ONE VectorTileTree.Add. GeoTiles owns the bytes; this owner never touches the protobuf.
    public Fin<VectorTileTree> ToTiles(Func<GeoFeature, Seq<(int Zoom, string Layer)>> route, TilePolicy policy, Op key) =>
        GeoServices.Wgs84
            .Bind(frame => Features.Traverse(f => f.Reproject(frame, key)).As())
            .Bind(wgs => {
                var zooms = wgs.Bind(f => route(f).Map(static slot => slot.Zoom)).Distinct().ToSeq();
                return zooms.Traverse(zoom => Simplified(wgs, policy.ToleranceAt(zoom), key).Map(placed => (zoom, placed))).As()
                    .Map(byZoom => {
                        var lod = byZoom.ToMap();
                        var tree = new VectorTileTree();
                        tree.Add(wgs.Map((f, ordinal) => route(f).Map(slot =>
                            ((IFeature)new Feature(lod[slot.Zoom][ordinal], f.Attributes), slot.Zoom, slot.Layer))).Flatten());
                        return tree;
                    });
            });

    // TopologyPreservingSimplifier preserves each geometry's OWN topology and knows nothing about its neighbours, so
    // two parcels sharing a boundary shed DIFFERENT vertices from that shared edge and the low-zoom tile ships
    // slivers and overlaps along every lot line. CoverageSimplifier simplifies the shared edge once and reassigns it
    // to both polygons — but it REPAIRS a set it never proved was a coverage, so CoverageValidator runs first and
    // names the polygons whose edges do not match: Repairs is a per-FEATURE IsValidOp verdict and cannot see an
    // unmatched shared edge at all. Areal members route through the coverage pass and every other dimension keeps the
    // per-geometry simplifier, because a non-coverage layer has no shared edge to preserve — the discriminant is the
    // geometry's own dimension, never a caller flag.
    static Fin<Seq<Geometry>> Simplified(Seq<GeoFeature> features, double tolerance, Op key) {
        var areal = features.Map(static (f, i) => (Ordinal: i, f.Geometry))
            .Filter(static row => row.Geometry.Dimension == OgcDimension.Surface);
        if (areal.IsEmpty) {
            return Fin.Succ(features.Map(f => TopologyPreservingSimplifier.Simplify(f.Geometry, tolerance)));
        }
        Geometry[] patch = areal.Map(static row => row.Geometry).ToArray();
        Geometry[] unmatched = CoverageValidator.Validate(patch, tolerance);
        return toSeq(unmatched).Map(static (edges, i) => (Index: i, Edges: Optional(edges)))
                .Filter(static row => row.Edges.IsSome)
                .Head
                .Match(
                    Some: row => Fin.Fail<Seq<Geometry>>(new BimFault.Refused(key, BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "geo-coverage-unmatched", areal[row.Index].Ordinal.ToString(CultureInfo.InvariantCulture), tolerance.ToString("G17", CultureInfo.InvariantCulture) }))),
                    None: () => {
                        Geometry[] coverage = CoverageSimplifier.SimplifyInner(patch, tolerance);
                        var placed = areal.Map((row, ordinal) => (row.Ordinal, Geometry: coverage[ordinal])).ToMap();
                        return Fin.Succ(features.Map((f, ordinal) =>
                            placed.Find(ordinal).IfNone(() => TopologyPreservingSimplifier.Simplify(f.Geometry, tolerance))));
                    });
    }

    // --- [PROJECTION]
    // The lineal-facility gate is the SAME one Along rails on, because an areal facility classifies IfcRoad and
    // carries no centreline to station along. Resolution and reprojection run ONCE per projection rather than once
    // per stamped feature, and the roster arrives INDEXED so the per-feature nearest-corridor read is a tree query.
    public Fin<GeoCorridors> Corridors(GeoReference reference, GeoSchema schema, Option<GeoVectorSource> source, double reach, Op key) =>
        Features
            .Filter(static f => f.Geometry.Dimension == OgcDimension.Curve)
            .Filter(f => GeoClassifier.Classify(f, schema, source, key).ToOption()
                .Exists(row => GeoClassifier.CorridorClasses.Contains(row.Class)))
            .Choose(static f => f.Text("id").Map(id => (Id: id, Feature: f)))
            .Traverse(pair => pair.Feature.Reproject(reference, key).Map(line => new GeoCorridor(pair.Id, line))).As()
            .Map(corridors => GeoCorridors.Of(corridors, reach));

    // Each feature's own outcome lands on its own ordinal: a refused feature accumulates a GeoRefusal and the fold
    // continues, so a cadastral extract with six broken parcels lands the other twelve thousand and names the six.
    // CANCELLATION is a different fact and stays on the rail — a caller reading a partial import must be able to tell
    // "these features refused" from "the import was abandoned", which one refusal roster cannot say.
    public Fin<GeoImport> Project(
        GeoReference reference, GeoSchema schema, Option<GeoVectorSource> source,
        double reach, CancellationToken token, ProjectionContext ctx) =>
        Corridors(reference, schema, source, reach, ctx.Key).Bind(corridors =>
            Features.Map(static (f, ordinal) => (Ordinal: ordinal, Feature: f))
                .Fold(Fin.Succ(GeoImport.Empty), (held, row) => held.Bind(import =>
                    token.IsCancellationRequested
                        ? Fin.Fail<GeoImport>(Errors.Cancelled)
                        : Fin.Succ(row.Feature.ToObject(reference, schema, source, ctx, corridors).Match(
                            Succ: import.Accept,
                            Fail: fault => import.Refuse(row.Ordinal, fault))))));
}
```

## [04]-[TILE_PYRAMID]

- Owner: `TilePolicy` the ONE MVT tile-grid value the pyramid derives from; `GeoTiles` the MVT byte codec and the TileJSON catalog.
- Entry: `TilePolicy.For(schema)` derives the policy from the source schema and `ToleranceAt(zoom)` answers one grid cell in degrees at that zoom; `GeoTiles.Encode(tree, policy, key)` streams every populated tile, `EncodeWorldTile(features, key)` emits the single anchored z0 tile, `Decode(bytes, x, y, zoom, key)` re-anchors stored bytes, and `Catalog(tree, name, urlTemplate, layers)` emits the TileJSON descriptor.
- Auto: `Extent` is the tile-local integer grid the writer quantizes onto AND the divisor the per-zoom simplify tolerance reads — they are the SAME quantity, and carrying it in two places meant a caller who narrowed the grid for a high-precision layer got a tolerance computed against the other one; `IdAttributeName` is READ off `GeoSchema`, never spelled here, so a tile the AppUi Mapsui overlay picks joins back to a seam node with no second attribute lookup.
- Receipt: the `Encode` byte rows keyed by tile id are the `{z}/{x}/{y}.mvt` delivery an object store serves, and `Catalog` is the TileJSON descriptor a MapLibre/Mapsui renderer discovers the pyramid through — its bounds and zoom span read off `GetExtents`, never hand-authored beside the pyramid.
- Packages: `NetTopologySuite.IO.VectorTiles`, `NetTopologySuite.IO.VectorTiles.Mapbox`, `System.Text.Json`, `LanguageExt.Core`
- Growth: a new tile LOD or layer policy is one route delegate value, never a second pyramid builder; a new tile grid is one `TilePolicy` value.
- Boundary: the MVT object model and protobuf are `NetTopologySuite.IO.VectorTiles`'s — geometry enters the tile cut ALREADY 4326 (the datum leg runs before tiling, never inside the codec), the 2D `.mvt` pyramid stays orthogonal to the 3D-Tiles glTF stack, and a hand-spelled MVT protobuf is the deleted form; `Encode` streams each tile through the per-tile `VectorTile.Write` because a `tree.Write(path, …)` filesystem pyramid is the rejected form for a store-backed host; MVT bytes carry only tile-local integer coordinates, so `Decode` re-anchors against its `Tile(x, y, zoom)` definition and the bare-bytes row pins the one self-describing world anchor; the sub-pixel culls are `MapboxTileWriter`'s OWN named constants and a policy column mirroring them was a knob no caller ever varied.

```csharp signature
// --- [POLICIES] -----------------------------------------------------------------------------
public sealed record TilePolicy(uint Extent, string IdAttributeName) {
    public static readonly TilePolicy Canonical = For(GeoSchema.Default);

    public static TilePolicy For(GeoSchema schema) => new(Extent: 4096u, IdAttributeName: schema.Identity);

    // One MVT integer-grid cell expressed in degrees at this zoom — the tolerance below which a vertex cannot
    // survive quantization anyway, so shedding it costs nothing and keeping it costs bytes.
    public double ToleranceAt(int zoom) => 360.0 / (Extent * (1L << zoom));
}

// --- [BOUNDARIES] ---------------------------------------------------------------------------
public static class GeoTiles {
    public static Fin<Seq<(ulong TileId, byte[] Bytes)>> Encode(VectorTileTree tree, TilePolicy policy, Op key) =>
        key.Catch(() => toSeq(tree.GetTileIds()
            .Select(id => {
                using var buffer = new MemoryStream();
                tree[id].Write(buffer, MapboxTileWriter.DefaultMinLinealExtent, MapboxTileWriter.DefaultMinPolygonalExtent,
                    policy.Extent, policy.IdAttributeName);
                return (TileId: id, Bytes: buffer.ToArray());
            })));

    // The single world-tile emit the GeoVectorSource.Mvt encode column binds: the set reprojects onto the ONE Wgs84
    // frame (MVT quantizes lon/lat) and lands one z0 (0,0,0) tile through the same VectorTile.Write path. It RAILS on
    // a reprojection reject — the throw that used to re-enter the row's enclosing Try was a rail collapsed into an
    // exception purely to feed another rail.
    internal static Fin<byte[]> EncodeWorldTile(Seq<GeoFeature> features, Op key) =>
        GeoServices.Wgs84
            .Bind(frame => features.Traverse(f => f.Reproject(frame, key)).As())
            .Bind(wgs => key.Catch(() => {
                var tile = new VectorTile { TileId = new NetTopologySuite.IO.VectorTiles.Tiles.Tile(0, 0, 0).Id };
                var layer = new Layer { Name = "features" };
                wgs.Iter(f => layer.Features.Add(new Feature(f.Geometry, f.Attributes)));
                tile.Layers.Add(layer);
                using var buffer = new MemoryStream();
                var policy = TilePolicy.Canonical;
                tile.Write(buffer, MapboxTileWriter.DefaultMinLinealExtent, MapboxTileWriter.DefaultMinPolygonalExtent,
                    policy.Extent, policy.IdAttributeName);
                return buffer.ToArray();
            }));

    // Decode threads the layer name beside each row — the renderer's style key is load-bearing round-trip evidence;
    // the reader de-quantizes against the canonical GeoServices.Factory precision.
    public static Fin<Seq<(string Layer, GeoFeature Feature)>> Decode(ReadOnlyMemory<byte> bytes, int x, int y, int zoom, Op key) =>
        key.Catch(() => {
            using var stream = new MemoryStream(bytes.ToArray());
            VectorTile tile = new MapboxTileReader(GeoServices.Factory)
                .Read(stream, new NetTopologySuite.IO.VectorTiles.Tiles.Tile(x, y, zoom), TilePolicy.Canonical.IdAttributeName);
            return tile.Layers.AsIterable()
                .Bind(layer => layer.Features.AsIterable()
                    .Map(f => (layer.Name, new GeoFeature(f.Geometry, f.Attributes, Option<ProjectedCrs>.None))))
                .ToSeq();
        });

    // TileJSON 2.0.0 source descriptor: tiles is the {z}/{x}/{y}.mvt URL template, bounds/minzoom/maxzoom read off
    // the populated tree, vector_layers carry the per-layer zoom advertisement; format "pbf"/tilejson "2.0.0" ride
    // the DTO defaults, field names the wire snake_case the DTO declares.
    public static string Catalog(VectorTileTree tree, string name, string urlTemplate, Seq<VectorLayer> layers) {
        tree.GetExtents(out double[] bounds, out int minZoom, out int maxZoom);
        return JsonSerializer.Serialize(new VectorTileSource {
            id = name,
            name = name,
            tiles = [urlTemplate],
            bounds = bounds,
            minzoom = minZoom,
            maxzoom = maxZoom,
            vector_layers = layers.ToArray(),
        });
    }
}
```

## [05]-[RESEARCH]

(none)
