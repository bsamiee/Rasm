# [BIM_TESSELLATION_BRIDGE]

`TessellationRequest` crosses imported IFC/STEP/IGES/native geometry to the IfcOpenShell companion — `IfcConvert` producing GLB — and re-imports that GLB through the `import#IMPORT_RAIL` glTF path, minting the `TessellationOutcome` receipt over the dual content keys, the mesh evidence, the origin, and the monotonic latency. This bridge owns the cache-before-cross-and-store-before-return policy over two injected ports, so this AEC-DOMAIN owner mints no `Rasm.Persistence` or `Rasm.Compute` reference, depends strictly upward, and stays HOST-LOCAL.

Two injected ports carry the policy: the content-addressed `TessellationStore` over `csharp:Rasm.Persistence/Store` and the `TessellationCompanion` cross over `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION`. Composed as settled vocabulary: the `format#FORMAT_AXIS` `TessellationRequiresCompanion` gate, the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` content key, the `Rasm.Compute/Runtime/transport#TRANSPORT_AXIS` transport, and the `import#IMPORT_RAIL` `ImportedGeometry` re-entry.

## [01]-[INDEX]

- [02]-[TESSELLATION_BRIDGE]: imported IFC/STEP/IGES geometry crosses to the companion rail and re-imports as GLB; the dual-key `TessellationOutcome` receipt.
- [03]-[EXPLICIT_TESSELLATION]: `BimIo.ImportIfcTessellation` decodes the `IfcTessellatedFaceSet` family IN PROCESS onto the seam carrier — coordinates, per-vertex normals, the IFC-native `HasTextures` UV set, and the `HasColours` per-face radiometry, the corner-indexed pair emitting through one unwelded gather — routing the residue to the companion as a narrowed `TessellationScope.Elements`.

## [02]-[TESSELLATION_BRIDGE]

- Owner: `TessellationRequest` crosses imported geometry to the IfcOpenShell companion and re-imports its GLB through the `import#IMPORT_RAIL` glTF path; `TessellationOutcome` the typed receipt, `TessellationStore`/`TessellationCompanion` the injected ports the app-platform binds, `TessellationScope`/`TessellationSettings`/`TessellationOrigin` the request vocabulary. This bridge owns the cache-before-cross-and-store-before-return POLICY; companion transport and durable GLB residence are bound port implementations, never types this AEC-DOMAIN owner mints.
- Entry: `Plan` gates on `source.TessellationRequiresCompanion` and mints the dual-key request, defaulting `settings`/`scope` to `TessellationSettings.Canonical`/`TessellationScope.Whole` so the whole-model canonical case is one call; `Resolve` reads the content-addressed store by `ArtifactKey` BEFORE the companion cross — a hit re-imports the cached GLB (`TessellationOrigin.Cached`, no round-trip), a miss crosses the companion over `Rasm.Compute/Runtime/transport#TRANSPORT_AXIS`, stores the fresh GLB write-blob-first, and re-imports it (`TessellationOrigin.Tessellated`).
- Auto: dual keys separate concerns. `SourceKey` is the PURE, tolerance-independent source identity — the cross-projection join the GLB row and the IFC-semantic-graph row of one source share, holding whatever deflection a tessellation runs at. `ContentKey` folds that `SourceKey` with every GLB-affecting dimension (the `InterchangePolicy` deflection/tolerance/angle and the order-stable `TessellationSettings`/`TessellationScope` config), so two tessellations differing only in a weld flag, a deflection, or an element filter never collide on the store address while source identity stays pure. `ArtifactKey` resolves to the store lookup address carrying the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `:glb` suffix; `Plan` reads the `format#FORMAT_AXIS` `TessellationRequiresCompanion` column so a managed format never crosses.
- Receipt: `TessellationOutcome` is typed tessellation evidence on the `Fin<T>` rail, never a generic `IReceipt`/ledger. One outcome shape surfaces both a cached reuse and a fresh cross, so a caller reads `TessellationOrigin` from the receipt rather than a side channel. Receipt columns carry coordinates, counts, and hashes — never payload bytes, the GLB riding the `TessellationStore` port. Mesh-evidence columns read the tessellation yield without traversing the payload or the store.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, Rasm, Rasm.Element
- Growth: a new tessellation parameter is one column on `TessellationSettings` folded into `ContentKey` and one `ifcopenshell.geom.settings` key the companion maps; a new scope modality is one `TessellationScope` case with one `--include`/`--exclude` mapping; a new companion-evaluated source format is one `InterchangeFormat` row carrying `TessellationRequiresCompanion=true` on `format#FORMAT_AXIS`. Each extension is a row, case, or column on an existing owner, never a second bridge or in-process tessellator.
- Boundary: this companion bridge is the single imported-geometry-to-GLB path — GeometryGym (the IFC semantic graph) and the in-process `import#IMPORT_RAIL` `StepReader` carry no tessellation kernel, no in-process arm64 solid evaluator is admitted, and `IfcConvert`/ifcopenshell stays the permanent default because the only .NET web-ifc binding is Windows C++/CLI. Both content keys derive from the kernel `ContentHash` and the seam `Projection/address#CANONICAL_WRITER`, so this AEC-DOMAIN owner mints no `Rasm.Compute.InterchangeIdentity` call, keeping the content-identity strata sealed. `SourceKey` is the cross-projection join the app-platform `csharp:Rasm.Persistence/Store` artifact-index projection owns, so the IFC semantic graph and the tessellated geometry stay two projections of one content-keyed source. A store fault degrades to `BimFault.CodecReject` — the same arm `faults#FAULT_BAND` gives the bSDD service-unreachable degrade, never a sixth arm — and a companion-unreachable cross or a non-companion source format to `BimFault.CapabilityMiss`. `import#IMPORT_RAIL` `FrameNormalization` coerces the glTF-canonical Y-up GLB to the kernel Z-up frame by the `InterchangeFormat.Glb` row, so this page mints no frame transform. This bridge reaches the `python:geometry/ifc-companion` IfcOpenShell package only through Compute's companion rpc, which owns the `ifcopenshell.geom.settings` argument mapping, the `IfcConvert` filter grammar, and the GLB stream-back; the `step-iso10303` and `iges-ansi` source formats ride that same companion, so a companion-evaluated format is one `format#FORMAT_AXIS` row.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Linq;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// ifcopenshell `dimensionality` value (CURVES=0, SURFACES_AND_SOLIDS=1 default, CURVES_SURFACES_AND_SOLIDS=2);
// byte value IS the companion key, so names track ifcopenshell — no solids-only mode.
public enum Dimensionality : byte { Curves = 0, SurfacesAndSolids = 1, CurvesSurfacesAndSolids = 2 }

// Tessellation scope is the IfcConvert geometry filter: whole model, an explicit GlobalId set
// (--include attribute GlobalId, the per-element/instancing modality), an entity-type set to keep
// (--include entities), or one to drop (--exclude entities — IfcSpace/IfcOpeningElement/IfcAnnotation
// off the tessellation, the dominant IFC-to-GLB cull). Case IS the modality; a layer filter or an
// exclude-by-GlobalId set is one further case, never a filter-mode flag beside the values.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TessellationScope {
    private TessellationScope() { }

    public sealed record WholeModel : TessellationScope;
    public sealed record Elements(Seq<string> GlobalIds) : TessellationScope;
    public sealed record Entities(Seq<string> IfcTypes) : TessellationScope;
    public sealed record ExcludeEntities(Seq<string> IfcTypes) : TessellationScope;

    public static readonly TessellationScope Whole = new WholeModel();

    // Order-stable, case-preserving token folded into ContentKey so {a,b} and {b,a} key identically; the
    // include/exclude polarity prefixes the token so a kept-set and a dropped-set of one type list never collide.
    public string Canon =>
        Switch(
            wholeModel:      static _ => "whole",
            elements:        static e => $"gid:{string.Join(',', e.GlobalIds.OrderBy(static id => id, StringComparer.Ordinal))}",
            entities:        static e => $"ent:{string.Join(',', e.IfcTypes.OrderBy(static t => t, StringComparer.Ordinal))}",
            excludeEntities: static e => $"xent:{string.Join(',', e.IfcTypes.OrderBy(static t => t, StringComparer.Ordinal))}");
}

[SmartEnum<string>]
public sealed partial class TessellationOrigin {
    public static readonly TessellationOrigin Cached = new("cached");
    public static readonly TessellationOrigin Tessellated = new("tessellated");
}

// --- [MODELS] -----------------------------------------------------------------------------
// Geometry-evaluation settings the companion maps onto ifcopenshell.geom.settings: ApplyDefaultMaterials
// is required for glTF serialisation, UseElementGuids lands the GlobalId on the GLB node for the per-element
// metadata join. Deflection/tolerance/angle live on InterchangePolicy, folding into ContentKey beside these
// flags; a further geometry knob lands as one more column the companion maps, never a second request shape.
public sealed record TessellationSettings(
    bool WeldVertices,
    bool UseWorldCoords,
    bool ApplyDefaultMaterials,
    bool GenerateUvs,
    bool DisableOpeningSubtractions,
    bool UseElementGuids,
    Dimensionality Dimensionality) {
    public static readonly TessellationSettings Canonical = new(
        WeldVertices: true, UseWorldCoords: true, ApplyDefaultMaterials: true, GenerateUvs: false,
        DisableOpeningSubtractions: false, UseElementGuids: true, Dimensionality: Dimensionality.SurfacesAndSolids);

    public string Canon =>
        $"w{Bit(WeldVertices)}c{Bit(UseWorldCoords)}m{Bit(ApplyDefaultMaterials)}u{Bit(GenerateUvs)}o{Bit(DisableOpeningSubtractions)}g{Bit(UseElementGuids)}d{(byte)Dimensionality}";

    static char Bit(bool flag) => flag ? '1' : '0';
}

// Mesh evidence (VertexCount/TriangleCount/GlbByteCount) rides typed receipt columns readable without the
// Geometry payload or the store-resident GLB; Took is the BCL TimeProvider monotonic timestamp/elapsed pair over the whole Resolve.
public sealed record TessellationOutcome(
    ImportedGeometry Geometry,
    string ArtifactKey,
    UInt128 ContentKey,
    UInt128 SourceKey,
    double Deflection,
    int VertexCount,
    int TriangleCount,
    long GlbByteCount,
    TessellationOrigin Origin,
    Duration Took,
    Instant At);

public sealed record TessellationRequest(
    UInt128 SourceKey,
    UInt128 ContentKey,
    InterchangeFormat Source,
    ReadOnlyMemory<byte> SourceBytes,
    InterchangePolicy Policy,
    TessellationSettings Settings,
    TessellationScope Scope) {
    public static Fin<TessellationRequest> Plan(
        InterchangeFormat source, ReadOnlyMemory<byte> sourceBytes, InterchangePolicy policy, Op key,
        Option<TessellationSettings> settings = default, Option<TessellationScope> scope = default) =>
        source.TessellationRequiresCompanion
            ? Fin.Succ(Keyed(source, sourceBytes, policy, settings.IfNone(TessellationSettings.Canonical), scope.IfNone(TessellationScope.Whole)))
            : Fin.Fail<TessellationRequest>(new BimFault.CapabilityMiss(key, $"tessellation-not-required:{source.Key}"));

    // SourceKey is the PURE source-artifact identity — the kernel seed-zero content-hash over the source key
    // and the raw bytes, NO tolerances — so the GLB projection and the IFC-semantic-graph projection of one
    // source re-derive the identical join key independent of the deflection any tessellation runs at; the
    // kernel ContentHash + seam CanonicalWriter are the ONE hasher, never the app-platform InterchangeIdentity.
    static TessellationRequest Keyed(InterchangeFormat source, ReadOnlyMemory<byte> sourceBytes, InterchangePolicy policy, TessellationSettings settings, TessellationScope scope) {
        UInt128 sourceKey = ContentHash.Of(new CanonicalWriter(0.0).String(source.Key).Raw(sourceBytes.Span).ToBytes().Span);
        return new(sourceKey, Fold(sourceKey, policy, settings, scope), source, sourceBytes, policy, settings, scope);
    }

    public string ArtifactKey => $"{ContentKey:x32}:glb";

    // Cache-before-cross-and-store-before-return: a content-key hit re-imports the cached GLB (Cached, no
    // round-trip); a miss crosses the companion, stores the fresh GLB write-blob-first so the next Resolve
    // hits, then re-imports it (Tessellated). A store-unreachable lookup and a store-write reject are the
    // DECLARED BimFault.CodecReject degrade; a companion-unreachable cross is BimFault.CapabilityMiss (the
    // companion the rail cannot reach) — each lifts BARE (band 2600 IS the Expected Code, no .ToError() hop). A
    // degenerate re-imported vertex set lowers the kernel GeometryFault.DegenerateInput(...).ToError() (the kernel
    // band is not Expected-derived) onto the shared Fin<ImportedGeometry> rail.
    public Fin<TessellationOutcome> Resolve(TessellationStore store, TessellationCompanion companion, IClock clock, TimeProvider time, Op key) {
        var at = clock.GetCurrentInstant();
        long mark = time.GetTimestamp();
        return store.Lookup(ArtifactKey)
            .MapFail(error => new BimFault.CodecReject(key, $"glb-store-unreachable:{error.Message}"))
            .Bind(hit => hit.Match(
                Some: glb => Reenter(glb, TessellationOrigin.Cached, clock, time, at, mark, key),
                None: () => companion.Cross(this)
                    .MapFail(error => new BimFault.CapabilityMiss(key, $"companion-unreachable:{error.Message}"))
                    .Bind(glb => store.Store(ArtifactKey, glb)
                        .MapFail(error => new BimFault.CodecReject(key, $"glb-store-reject:{error.Message}"))
                        .Bind(_ => Reenter(glb, TessellationOrigin.Tessellated, clock, time, at, mark, key)))));
    }

    Fin<TessellationOutcome> Reenter(ReadOnlyMemory<byte> glb, TessellationOrigin origin, IClock clock, TimeProvider time, Instant at, long mark, Op key) =>
        BimIo.ImportGeometry(InterchangeFormat.Glb, glb, clock, key)
            .Bind(geometry => Sound(geometry)
                ? Fin.Succ(new TessellationOutcome(
                    geometry, ArtifactKey, ContentKey, SourceKey, Policy.Deflection,
                    geometry.VertexCount, geometry.TriangleCount, glb.Length, origin, time.GetElapsedTime(mark).ToDuration(), at))
                : Fin.Fail<TessellationOutcome>(new GeometryFault.DegenerateInput(Kind.Mesh, None, $"tessellation-degenerate:{ArtifactKey}").ToError()));

    // A re-imported GLB is degenerate when empty, when its arena fails its own validity claim, or when a position
    // carries a non-finite coordinate; all three lower the kernel GeometryFault on the shared rail rather than
    // minting a hollow mesh — the boundary-validation scan is the named statement exemption. The arena's IsValid
    // already proves descriptor/payload agreement and a lossless witness, so this guard adds only the finiteness
    // check IsValid does not make, reading the position lane through its typed view rather than a raw span.
    static bool Sound(ImportedGeometry geometry) {
        if (geometry is not { VertexCount: > 0, TriangleCount: > 0 } || !geometry.Lanes.IsValid) { return false; }
        foreach (float coordinate in geometry.Lanes.View<float>(EncodingChannel.Position)) {
            if (!float.IsFinite(coordinate)) { return false; }
        }
        return true;
    }

    // ContentKey folds the PURE SourceKey with EVERY GLB-affecting dimension — the InterchangePolicy
    // deflection/tolerance/angle AND the order-stable case-preserved config (settings + scope) — through the
    // ONE kernel seed-zero content-hash, so the store address partitions on every input that changes the GLB
    // while SourceKey stays the tolerance-independent join. The seam CanonicalWriter length-prefixes each
    // String, so a Canon-token delimiter collision cannot forge a store-address equality; no second hasher.
    static UInt128 Fold(UInt128 sourceKey, InterchangePolicy policy, TessellationSettings settings, TessellationScope scope) =>
        ContentHash.Of(new CanonicalWriter(0.0)
            .U128(sourceKey)
            .Double(policy.Deflection).Double(policy.Tolerance).Double(policy.AngleTolerance)
            .String(settings.Canon).String(scope.Canon)
            .ToBytes().Span);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Injected ports the app-platform binds at the composition edge. Bim mints no Persistence or Compute
// reference; it owns the cache/cross/store POLICY through these contracts while durable residence and
// transport are the bound implementations, composing upward with no downstream package edge. Lookup
// separates a store fault (Fin failure) from a normal miss (None); Store is the write-blob-first put.
public interface TessellationStore {
    Fin<Option<ReadOnlyMemory<byte>>> Lookup(string artifactKey);
    Fin<Unit> Store(string artifactKey, ReadOnlyMemory<byte> glb);
}

public interface TessellationCompanion {
    Fin<ReadOnlyMemory<byte>> Cross(TessellationRequest request);
}
```

## [03]-[EXPLICIT_TESSELLATION]

- Owner: `BimIo.ImportIfcTessellation` the in-process decode of the ALREADY-TESSELLATED IFC representation family — `IfcTriangulatedFaceSet` and `IfcPolygonalFaceSet` over their shared `IfcCartesianPointList3D` coordinate store — onto the seam `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry`, contributing an `EncodingChannel.Uv` lane from the face set's OWN `HasTextures` texture map and an `EncodingChannel.ColorRgba` lane from its OWN `HasColours` map exactly when it declares one — an untextured or unpainted face set contributes no lane, never an empty column; the colour read composes the `Semantics/appearance#APPEARANCE_PROJECTION` `IndexedColour` value that owns both directions of the per-face radiometry, so this walk declares no colour shape and mints no accessor; `ExplicitTessellation` the split product pairing that geometry with the `GlobalId` residue the companion still owns.
- Entry: `BimIo.ImportIfcTessellation(DatabaseIfc db, Op key)` returns `Fin<ExplicitTessellation>`, walking the live graph once and partitioning every product's representation items into the explicitly-tessellated set this page decodes and the evaluated set the `TESSELLATION_BRIDGE` crosses; the caller hands `ExplicitTessellation.Deferred` straight to `TessellationScope.Elements` so `Plan` narrows the companion cross to exactly the products that need an evaluator. A malformed index run — a corner past the coordinate count, a texture-coordinate index past the vertex list, a colour ordinal past the palette, a colour run shorter than the face count — rails `Model/faults#FAULT_BAND` `BimFault.ModelRejected` off `key`, lifted BARE (band 2600 IS the `Expected` `Code`, no `.ToError()` hop), so every bound belongs to the one `Try.lift` envelope and no read carries a guard of its own.
- Auto: `IfcTessellatedFaceSet` IS explicit mesh data — coordinates, corner indices, optional authored normals, an optional texture-coordinate list, and an optional per-face colour map — so evaluating it needs no solid kernel and crossing it to the companion is a round trip that COSTS a whole transport hop and DESTROYS both the IFC-native UV set and the radiometry, neither of which any glTF the companion returns carries. `HasTextures` is a SET of `IfcIndexedTextureMap`, each pairing a `TexCoords` `IfcTextureVertexList` with the `Maps` list naming WHICH `IfcSurfaceTexture` rows that parameterization serves, so the decode joins the UV set to the appearance roster by texture identity rather than by position. `HasColours` is a SINGLE `IfcIndexedColourMap` binding a palette, a one-based index run with one entry per FACE, and one `Opacity` the schema applies to every face alike, `NaN` reading as fully opaque; the `IndexedColour` value owns that read whole — unit-valued triples lowered to scene-linear through the appearance projector's own sRGB EOTF, so an IFC vertex colour and an IFC base colour reach a consumer in ONE space and this walk applies no transfer of its own. Both index forms address CORNERS where the coordinate store addresses VERTICES — the triangulated subtype's `TexCoordIndex` triples are per-triangle, the polygonal subtype's `TexCoordIndices` per-face, and the colour run per-face — so ONE gather decision owns them: a face set declaring either emits one vertex per corner, a face set declaring neither keeps the packed per-coordinate emit, and the per-coordinate texture-vertex form lands through the same gather at either length. Broadcasting a per-face colour or a per-corner UV onto a shared coordinate is the deleted form: the last face to write wins, which bleeds colour across a material boundary and tears the UV at every seam, and both defects render without a diagnostic.
- Receipt: `ExplicitTessellation` carries the decoded `ImportedGeometry`, the decoded product count, and the deferred `GlobalId` set — the split evidence a composition reads to know how much of a model needed an evaluator at all, and the reason a texture-bearing or colour-bearing IFC now round-trips its parameterization and its radiometry when the companion path cannot.
- Packages: GeometryGymIFC_Core (`IfcTessellatedFaceSet`/`IfcTextureVertexList`/`IfcIndexedTriangleTextureMap` — the UV-index payload reached through the `Semantics/appearance#APPEARANCE_PROJECTION` `IfcInternals` capsule under that catalog's `[INTERNAL_ACCESS_LAW]`, the colour payload through that page's `IndexedColour`), Rasm.Element, Rasm (`EncodingChannel`), NodaTime, LanguageExt.Core
- Growth: a new tessellated subtype is one arm on the total representation-item dispatch; a new attribute lane is one `MeshChunk.Attributes` entry the SAME walk fills, the seam carrier already declaring both `EncodingChannel.Uv` and `EncodingChannel.ColorRgba`; a corner-indexed presentation payload beyond these two joins the existing unweld discriminant rather than adding a second emit path, and an n-gon index list widens `Slot`; never a second IFC mesh decoder and never an in-process evaluator for a swept or BREP item.
- Boundary: `ImportIfcTessellation` decodes EXPLICIT indexed meshes and nothing else — an `IfcExtrudedAreaSolid`, an `IfcAdvancedBrep`, or any item requiring a solid kernel routes to `[02]-[TESSELLATION_BRIDGE]` unchanged, so the "no in-process arm64 solid evaluator" law is untouched and `TessellationRequiresCompanion` keeps its meaning. Reading a UV coordinate list or a colour palette is neither a codec nor a texel resample, so the `Rasm.Bim` "CLASSIFIES and CARRIES texture payloads and decodes none" ruling holds whole; the internal-payload reach is the `Semantics/appearance#APPEARANCE_PROJECTION` `IfcInternals` capsule and the colour shape is that page's `IndexedColour`, so this page declares neither — a presentation item seats on the presentation owner and `ARCHITECTURE.md` `[02]-[STRATA]` puts `Exchange` above `Semantics`, so composing them here is the downward edge the acyclic law admits while the reverse seating inverts the strata AND forks the palette fold into an ingest copy and an egress copy; one capsule pinned to one manifest version is the whole of this branch's `internal` GeometryGym surface and a second copy beside it forks the version pin. The decoded geometry lands the SAME `ImportedGeometry` the managed arms produce (via the `import#IMPORT_RAIL` pool builder), so no second carrier exists; frame normalization does not apply because IFC coordinates are already in the model frame the seam header declares. The texture PAYLOAD stays the `Semantics/appearance#APPEARANCE_PROJECTION` roster's — this owner carries only the coordinate set and the texture identity it binds to, so the two halves meet at the app-root edge exactly where the roster already crosses.

```csharp signature
// Split product: what decoded here, and what still needs the evaluator. Deferred is a GlobalId set precisely so
// it drops straight into TessellationScope.Elements — the companion cross narrows to the residue instead of
// re-evaluating a model whose tessellated majority is already in hand.
public sealed record ExplicitTessellation(ImportedGeometry Geometry, int DecodedProducts, Seq<string> Deferred);

// BimIo owns this decode as an arm, never a sibling class: it is the ONE bytes-and-graph->carrier decode owner and
// it alone holds the MeshSoup pool builder every arm folds into, so a second class here would either re-mint that
// builder or reach a private one. The partial is the same shape BimExport takes across its own section fences.
public static partial class BimIo {
    public static Fin<ExplicitTessellation> ImportIfcTessellation(DatabaseIfc db, Op key) =>
        Try.lift(() => Partition(db)).Run().MapFail(error => new BimFault.ModelRejected(key, $"ifc-tessellation:{error.Message}"));

    // One walk, one partition. A product whose representation items are ALL tessellated decodes whole; a product
    // carrying any evaluator-bound item defers WHOLE, because a half-decoded product would place two fragments of
    // one element under two content keys.
    static ExplicitTessellation Partition(DatabaseIfc db) {
        var soup = new MeshSoup();
        var deferred = Seq<string>();
        int decoded = 0;
        foreach (var product in db.Project.Extract<IfcProduct>()) {
            var items = Optional(product.Representation).Match(
                None: () => Seq<IfcRepresentationItem>(),
                Some: shape => shape.Representations.AsIterable().Bind(static rep => rep.Items.AsIterable()).ToSeq());
            if (items.IsEmpty || !items.ForAll(static item => item is IfcTessellatedFaceSet)) {
                if (!items.IsEmpty) { deferred = deferred.Add(product.GlobalId); }
                continue;
            }
            items.Iter(item => soup.Baked(Decode((IfcTessellatedFaceSet)item)));
            decoded++;
        }
        return new ExplicitTessellation(soup.ToGeometry(InterchangeFormat.Ifc, SystemClock.Instance.GetCurrentInstant()), decoded, deferred);
    }

    // Coordinates are the packed IfcCartesianPointList3D store shared by both subtypes; the corner run is the
    // subtype's own index list, fan-triangulated for the polygonal case, each emitted triangle carrying the ordinal of
    // the SOURCE face it came from. The GATHER is this walk's one shape decision, and it is forced by the two
    // CORNER-indexed presentation payloads IFC binds to a face set — the per-face colour run and the per-triangle UV
    // index triples: either present emits ONE VERTEX PER CORNER, because broadcasting a per-face colour or a
    // per-corner UV onto a coordinate two faces share hands that vertex whichever face writes last, a colour bleed
    // and a UV seam tear that render wrong and read right. Absent both, the gather is the coordinate identity and the
    // packed welded emit every per-coordinate face set wants stands unchanged. Authored normals ride the SAME gather
    // at per-coordinate arity; an absent set takes the up-normal the seam lane's absent case already carries.
    static MeshChunk Decode(IfcTessellatedFaceSet faceSet) {
        var points = Coordinates(faceSet);
        var authored = Normals(faceSet);
        (long[] Corner, int[] Face) mesh = Corners(faceSet);
        Option<IndexedColour> colour = IndexedColour.Of(faceSet);
        Option<Seq<(int A, int B, int C)>> mapped = TextureIndex(faceSet);
        bool unweld = colour.IsSome || mapped.IsSome;
        long[] gather = unweld ? mesh.Corner : Ordinals(points.Count);
        long[] corners = unweld ? Ordinals(mesh.Corner.Length) : mesh.Corner;
        var vertices = new float[gather.Length * 3];
        var normals = new float[gather.Length * 3];
        for (int v = 0; v < gather.Length; v++) {
            int source = (int)gather[v];
            (vertices[v * 3], vertices[(v * 3) + 1], vertices[(v * 3) + 2]) = points[source];
            (normals[v * 3], normals[(v * 3) + 1], normals[(v * 3) + 2]) = authored.Map(store => store[source]).IfNone((0f, 0f, 1f));
        }
        float[] uvs = TextureCoordinates(faceSet, gather, points.Count, mapped);
        float[] paint = colour.Map(read => Painted(read, mesh.Face, gather.Length)).IfNone([]);
        return new MeshChunk(vertices, normals, corners,
            (uvs.Length > 0 ? Seq((EncodingChannel.Uv, uvs)) : Seq<(EncodingChannel, float[])>())
            + (paint.Length > 0 ? Seq((EncodingChannel.ColorRgba, paint)) : Seq<(EncodingChannel, float[])>()));
    }

    // Ordinals is the identity gather — the welded coordinate order and the unwelded corner order are the same
    // expression at two lengths, so neither branch spells its own loop.
    static long[] Ordinals(int count) => [.. Enumerable.Range(0, count).Select(static i => (long)i)];

    // Painted lowers the per-FACE colour onto the per-vertex ColorRgba lane: emitted vertex v belongs to triangle v/3,
    // that triangle names its source face, and IndexedColour.Rgba resolves that face's palette row plus the map's
    // single Opacity into the four channels. The value owns the whole transfer — its palette is already scene-linear
    // and already unit-interval — so this walk scales nothing, and the /255 the byte-valued PLY arm needs is exactly
    // the defect here.
    static float[] Painted(IndexedColour colour, int[] faces, int vertexCount) {
        var lane = new float[vertexCount * 4];
        for (int v = 0; v < vertexCount; v++) {
            (double r, double g, double b, double a) = colour.Rgba(faces[v / 3]);
            (lane[v * 4], lane[(v * 4) + 1], lane[(v * 4) + 2], lane[(v * 4) + 3]) = ((float)r, (float)g, (float)b, (float)a);
        }
        return lane;
    }

    // TextureIndex reads the triangulated subtype's per-TRIANGLE UV index triples through the same IfcInternals
    // capsule the colour payload crosses. An absent triple list is the schema's OTHER form — a per-COORDINATE vertex
    // list parallel to the coordinate store — which lands through the coordinate gather with no re-index.
    static Option<Seq<(int A, int B, int C)>> TextureIndex(IfcTessellatedFaceSet faceSet) =>
        faceSet.HasTextures.AsIterable()
            .Choose(static map => map is IfcIndexedTriangleTextureMap triangle
                ? Some(IfcInternals.TexCoordRun(triangle))
                : Option<Seq<(int A, int B, int C)>>.None)
            .Filter(static run => !run.IsEmpty)
            .Head;

    // HasTextures is the IFC-native UV set and it is a SET, so a face set parameterized for several textures carries one
    // map per texture identity; the seam carrier declares ONE coordinate lane, so the FIRST map whose vertex list the
    // arity admits lands and the rest ride the appearance roster's own texture identity — a further lane is one
    // carrier column, never a second decode. The arity the gate reads follows the form: an indexed map's triple run
    // must cover every emitted triangle, an unindexed map's vertex list every coordinate. An arity mismatch yields
    // the seam's typed absence rather than a truncated or zero-padded lane.
    static float[] TextureCoordinates(IfcTessellatedFaceSet faceSet, long[] gather, int coordinateCount, Option<Seq<(int A, int B, int C)>> mapped) =>
        faceSet.HasTextures.AsIterable()
            .Choose(static map => Optional(map.TexCoords).Map(static list => toSeq(list.TexCoordsList).Map(static uv => (U: uv.Item1, V: uv.Item2))))
            .Filter(list => mapped.Match(Some: run => run.Count == gather.Length / 3, None: () => list.Count == coordinateCount))
            .Head
            .Map(list => Sampled(list, gather, mapped))
            .IfNone([]);

    // Sampled writes the UV lane in emitted-vertex order: an indexed map takes the ordinate off the vertex's own
    // corner slot — v/3 the triangle, v%3 the corner, one-based into the vertex list — and an unindexed map takes the
    // gathered coordinate. A vertex list too short for an index is a malformed file and throws inside Partition's
    // Try.lift envelope beside the colour-run bound, so neither read needs a guard of its own.
    static float[] Sampled(Seq<(double U, double V)> list, long[] gather, Option<Seq<(int A, int B, int C)>> mapped) {
        var uvs = new float[gather.Length * 2];
        for (int v = 0; v < gather.Length; v++) {
            (double s, double t) = list[mapped.Match(Some: run => Slot(run[v / 3], v % 3) - 1, None: () => (int)gather[v])];
            (uvs[v * 2], uvs[(v * 2) + 1]) = ((float)s, (float)t);
        }
        return uvs;
    }

    // Slot picks a positional triple's ordinate by corner index — one expression instead of a three-arm ladder at
    // every read site, and the shape a quad or n-gon index list widens through.
    static int Slot((int A, int B, int C) triple, int corner) => corner switch { 0 => triple.A, 1 => triple.B, _ => triple.C };

    // <Coordinates: the IfcCartesianPointList3D packed-store read; Normals: the IfcTriangulatedFaceSet.Normals store
    // after its optional NormalIndex re-index; Corners: the IfcTriangulatedFaceSet.CoordIndex run and the
    // IfcPolygonalFaceSet fan paired with each emitted triangle's source-face ordinal — all spelled at realization
    // against the catalogue, [04]-[RESEARCH]>
    static System.Collections.Generic.IReadOnlyList<(float X, float Y, float Z)> Coordinates(IfcTessellatedFaceSet faceSet) => throw new NotImplementedException();
    static Option<System.Collections.Generic.IReadOnlyList<(float X, float Y, float Z)>> Normals(IfcTessellatedFaceSet faceSet) => throw new NotImplementedException();
    static (long[] Corner, int[] Face) Corners(IfcTessellatedFaceSet faceSet) => throw new NotImplementedException();
}
```

## [04]-[RESEARCH]

- [TESSELLATED_INDEX_MEMBERS]-[OPEN]: which element type and arity does `IfcCartesianPointList3D.Coordinates` carry, what shape are `IfcTriangulatedFaceSet.Normals`/`NormalIndex`, and how do `IfcPolygonalFaceSet.Faces`/`IfcIndexedPolygonalFace.CoordIndex` spell the polygonal index run the catalogue leaves unrostered; `uv run python -m tools.assay api` over `GeometryGymIFC_Core` for those signatures, then bake the spellings into `Coordinates`/`Normals`/`Corners` — `Corners` also pairing each emitted triangle with its source-face ordinal, which is the identity for the triangulated run and the polygon ordinal repeated across a fan.
- [POLYGONAL_TEXTURE_REINDEX]-[OPEN]: `IfcIndexedPolygonalTextureMap.TexCoordIndices` is decompile-verified PUBLIC as `LIST<IfcTextureCoordinateIndices>`, each row carrying public `TexCoordIndex` (`List<int>`) and `TexCoordsOf` (`IfcIndexedPolygonalFace`), so the polygonal per-face UV re-index needs no accessor — it needs the join from `TexCoordsOf` back to the `Faces` ordinal the corner fan emits; Route: the same assay `api` run over `IfcIndexedPolygonalFace` and `IfcPolygonalFaceSet.Faces` fixes that ordinal, then `TextureIndex` gains the polygonal arm returning per-triangle triples off the fan exactly as the triangulated arm does, and `Slot` widens past three corners for a polygon read that keeps its own arity.
