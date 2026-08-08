# [BIM_TESSELLATION_BRIDGE]

`TessellationRequest` crosses imported IFC/STEP/IGES/native geometry to the IfcOpenShell companion — `IfcConvert` producing GLB — and re-imports that GLB through the `import#IMPORT_RAIL` glTF path, minting the `TessellationOutcome` receipt over the dual content keys, the mesh evidence, the origin, and the monotonic latency. This bridge owns the cache-before-cross-and-store-before-return policy over two injected ports, so this AEC-DOMAIN owner mints no `Rasm.Persistence` or `Rasm.Compute` reference, depends strictly upward, and stays HOST-LOCAL.

Two injected ports carry the policy: the content-addressed `ITessellationStore` over `Rasm.Persistence/Store` and the `ITessellationCompanion` cross over `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION`. Composed as settled vocabulary: the `format#FORMAT_AXIS` `TessellationRequiresCompanion` gate, the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` content key, the `Rasm.Compute/Runtime/transport#TRANSPORT_AXIS` transport, and the `import#IMPORT_RAIL` `ImportedGeometry` re-entry.

## [01]-[INDEX]

- [02]-[TESSELLATION_BRIDGE]: imported IFC/STEP/IGES geometry crosses to the companion rail and re-imports as GLB; the dual-key `TessellationOutcome` receipt.
- [03]-[EXPLICIT_TESSELLATION]: `BimIo.ImportIfcTessellation` decodes the `IfcTessellatedFaceSet` family IN PROCESS onto the seam carrier — coordinates, per-vertex normals, the IFC-native `HasTextures` UV set, and the `HasColours` per-face radiometry, the corner-indexed pair emitting through one unwelded gather — routing the residue to the companion as a narrowed `TessellationScope.Elements`.

## [02]-[TESSELLATION_BRIDGE]

- Owner: `TessellationRequest` crosses imported geometry to the IfcOpenShell companion and re-imports its GLB through the `import#IMPORT_RAIL` glTF path; `TessellationOutcome` the typed receipt, `ITessellationStore`/`ITessellationCompanion` the injected ports the app-platform binds, `TessellationScope`/`TessellationSettings`/`TessellationOrigin` the request vocabulary. This bridge owns the cache-before-cross-and-store-before-return POLICY and the managed half of its governance (the boundary token reads); companion transport and durable GLB residence are bound port implementations carrying BCL `CancellationToken` currency, never types this AEC-DOMAIN owner mints. In-flight progress is absent from both port contracts because the two-hop transport publishes no progress channel — a sink there has no feeder.
- Entry: `Plan` gates on `source.TessellationRequiresCompanion` and mints the dual-key request, defaulting `settings`/`scope` to `TessellationSettings.Canonical`/`TessellationScope.Whole` so the whole-model canonical case is one call; `Resolve` reads the content-addressed store by the request's `Address` BEFORE the companion cross — a hit re-imports the cached GLB (`TessellationOrigin.Cached`, no round-trip), a miss crosses the companion over `Rasm.Compute/Runtime/transport#TRANSPORT_AXIS`, stores the fresh GLB write-blob-first, and re-imports it (`TessellationOrigin.Tessellated`).
- Auto: dual keys separate concerns. `SourceKey` is the PURE, tolerance-independent source identity — the cross-projection join the GLB row and the IFC-semantic-graph row of one source share, holding whatever deflection a tessellation runs at. `ContentKey` folds that `SourceKey` with every GLB-affecting dimension (the `InterchangePolicy` deflection/tolerance/angle and the order-stable `TessellationSettings`/`TessellationScope` config), so two tessellations differing only in a weld flag, a deflection, or an element filter never collide on the store address while source identity stays pure. `Address` is the `Energy/exchange#ENERGY_EXCHANGE` `ArtifactKey` value object minted off the `ContentKey` and the `InterchangeFormat.Glb` row — the ONE object-plane grammar owner, so the store address a lookup, a store write, and the receipt carry is admitted rather than rendered here; `Plan` reads the `format#FORMAT_AXIS` `TessellationRequiresCompanion` column so a managed format never crosses.
- Receipt: the in-flight fraction is the COMPANION's alone — it runs the work out of process — and this owner publishes none rather than fabricating a stage ratio for a store lookup, while the token is read at every boundary the policy owns so an abandoned request never pays for the next long leg. `TessellationOutcome` is typed tessellation evidence on the `Fin<T>` rail, never a generic `IReceipt`/ledger. One outcome shape surfaces both a cached reuse and a fresh cross, so a caller reads `TessellationOrigin` from the receipt rather than a side channel. Receipt columns carry coordinates, counts, and hashes — never payload bytes, the GLB riding the `ITessellationStore` port. Mesh-evidence columns read the tessellation yield without traversing the payload or the store.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, Rasm, Rasm.Element
- Growth: a new tessellation parameter is one column on `TessellationSettings` folded into `ContentKey` and one `ifcopenshell.geom.settings` key the companion maps; a new scope modality is one `TessellationScope` case with one `--include`/`--exclude` mapping; a new companion-evaluated source format is one `InterchangeFormat` row carrying `TessellationRequiresCompanion=true` on `format#FORMAT_AXIS`. Each extension is a row, case, or column on an existing owner, never a second bridge or in-process tessellator.
- Boundary: this companion bridge is the single imported-geometry-to-GLB path — GeometryGym (the IFC semantic graph) and the in-process `import#IMPORT_RAIL` `StepReader` carry no tessellation kernel, no in-process arm64 solid evaluator is admitted, and `IfcConvert`/ifcopenshell stays the permanent default because the only .NET web-ifc binding is Windows C++/CLI. Both content keys derive from the kernel `ContentHash` and the seam `Projection/address#CANONICAL_WRITER`, so this AEC-DOMAIN owner mints no `Rasm.Compute.InterchangeIdentity` call, keeping the content-identity strata sealed. `SourceKey` is the cross-projection join the app-platform `Rasm.Persistence/Store` artifact-index projection owns, so the IFC semantic graph and the tessellated geometry stay two projections of one content-keyed source. Store faults degrade to `BimFault.CodecReject` — the same arm `faults#FAULT_BAND` gives the bSDD service-unreachable degrade, never a sixth arm — and a companion-unreachable cross or a non-companion source format to `BimFault.CapabilityMiss`. `import#IMPORT_RAIL` `FrameNormalization` coerces the glTF-canonical Y-up GLB to the kernel Z-up frame by the `InterchangeFormat.Glb` row, so this page mints no frame transform. This bridge reaches the `python:geometry/ifc-companion` IfcOpenShell package only through Compute's companion rpc, which owns the `ifcopenshell.geom.settings` argument mapping, the `IfcConvert` filter grammar, and the GLB stream-back; the `step-iso10303` and `iges-ansi` source formats ride that same companion, so a companion-evaluated format is one `format#FORMAT_AXIS` row.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Globalization;
using System.Linq;
using System.Threading;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Bim.Model;                       // BimFault + the Detail roster this bridge raises through
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

    // Order-stable ContentKey contribution written STRAIGHT onto the one seam CanonicalWriter — a case ordinal,
    // then the count-prefixed ordered token run — so {a,b} and {b,a} key identically, include/exclude polarity
    // rides the ordinal, and no delimiter-joined intermediate string re-mints a second canonicalization scheme
    // beside the writer's own length-prefixed framing.
    public CanonicalWriter Write(CanonicalWriter w) =>
        Switch(
            state: w,
            wholeModel:      static (writer, _) => writer.Ordinal(0),
            elements:        static (writer, e) => Tokens(writer.Ordinal(1), e.GlobalIds),
            entities:        static (writer, e) => Tokens(writer.Ordinal(2), e.IfcTypes),
            excludeEntities: static (writer, e) => Tokens(writer.Ordinal(3), e.IfcTypes));

    static CanonicalWriter Tokens(CanonicalWriter w, Seq<string> raw) {
        var ordered = raw.OrderBy(static t => t, StringComparer.Ordinal).ToSeq();
        return ordered.Fold(w.Ordinal(ordered.Count), static (writer, token) => writer.String(token));
    }
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

    // ContentKey contribution on the one seam writer — seven flags and the dimensionality ordinal, no
    // intermediate token string; a new geometry knob is one more write in declaration order.
    public CanonicalWriter Write(CanonicalWriter w) => w
        .Bool(WeldVertices).Bool(UseWorldCoords).Bool(ApplyDefaultMaterials).Bool(GenerateUvs)
        .Bool(DisableOpeningSubtractions).Bool(UseElementGuids).Ordinal((byte)Dimensionality);
}

// Mesh evidence (VertexCount/TriangleCount/GlbByteCount) rides typed receipt columns readable without the
// Geometry payload or the store-resident GLB; Took is the BCL TimeProvider monotonic timestamp/elapsed pair over the whole Resolve.
public sealed record TessellationOutcome(
    ImportedGeometry Geometry,
    ArtifactKey Address,
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
            : Fin.Fail<TessellationRequest>(Detail.TessellationNotRequired.At(key, source.Key));

    // SourceKey is the PURE source-artifact identity — the kernel seed-zero content-hash over the source key
    // and the raw bytes, NO tolerances — so the GLB projection and the IFC-semantic-graph projection of one
    // source re-derive the identical join key independent of the deflection any tessellation runs at; the
    // kernel ContentHash + seam CanonicalWriter are the ONE hasher, never the app-platform InterchangeIdentity.
    static TessellationRequest Keyed(InterchangeFormat source, ReadOnlyMemory<byte> sourceBytes, InterchangePolicy policy, TessellationSettings settings, TessellationScope scope) {
        UInt128 sourceKey = ContentHash.Of(new CanonicalWriter(0.0).String(source.Key).Raw(sourceBytes.Span).ToBytes().Span);
        return new(sourceKey, Fold(sourceKey, policy, settings, scope), source, sourceBytes, policy, settings, scope);
    }

    // Store addresses MINT through the grammar owner off the two facts each IS, so this bridge holds no
    // separator position, hex width, or format token of its own and a grammar change lands in exactly one place.
    public ArtifactKey Address => ArtifactKey.Of(ContentKey, InterchangeFormat.Glb);

    // Cache-before-cross-and-store-before-return: a content-key hit re-imports the cached GLB (Cached, no
    // round-trip); a miss crosses the companion, stores the fresh GLB write-blob-first so the next Resolve
    // hits, then re-imports it (Tessellated). A store-unreachable lookup and a store-write reject are the
    // DECLARED BimFault.CodecReject degrade; a companion-unreachable cross is BimFault.CapabilityMiss (the
    // companion the rail cannot reach) — each lifts BARE (band 2600 IS the Expected Code, no .ToError() hop). A
    // degenerate re-imported vertex set lowers the kernel GeometryFault.DegenerateInput(...).ToError() (the kernel
    // band is not Expected-derived) onto the shared Fin<ImportedGeometry> rail.
    // Governance rides the PORT CONTRACT in BCL currency — the token is a `System` type the binding implementation
    // already speaks, so the out-of-process companion carries it into its own transport without this AEC-DOMAIN
    // owner minting a cancellation type of its own. The managed half is the token read AT each boundary the policy
    // owns — before the cross, before the store write, before the re-import — so an abandoned request stops before the
    // next long leg instead of paying for a GLB nobody will read. Abandonment lowers the kernel `Rasm.Domain`
    // `Fault.Cancelled`, the branch's one cancellation spelling.
    // This contract carries NO progress sink: the companion is the only party that can measure an in-flight fraction,
    // it DOES measure one, and that fraction already has a home that is not this rail: the tessellating daemon beats
    // its own graduation pulse onto the companion's observability tap under that lane's lossy-drop law, while the
    // two-hop rpc this cross rides (`Rasm.Compute/Runtime/transport#TRANSPORT_AXIS`) publishes no progress channel in
    // either direction. So a sink declared here is a slot the implementation can only ignore — a governance column no
    // producer feeds, reading complete to a scan while reporting nothing — and threading one would fork a fraction
    // that already publishes elsewhere into a second channel with a worse drop law. A managed progress lane lands as
    // a transport row FIRST and reaches this contract second, never as a parameter anticipating one.
    public Fin<TessellationOutcome> Resolve(
        ITessellationStore store, ITessellationCompanion companion,
        CancellationToken cancel, IClock clock, TimeProvider time, Op key) {
        var at = clock.GetCurrentInstant();
        long mark = time.GetTimestamp();
        return Live(cancel)
            .Bind(_ => store.Lookup(Address).MapFail(error => (Error)Detail.GlbStoreUnreachable.At(key, error.Message)))
            .Bind(hit => hit.Match(
                Some: glb => Live(cancel).Bind(_ => Reenter(glb, TessellationOrigin.Cached, clock, time, at, mark, key)),
                // Store writes are the RELEASE half of the cross's bracket, so they run on the ABANDONMENT path
                // too: a caller who walked away still paid for the companion round trip, and discarding the GLB
                // there makes the next Resolve pay it again. Cancellation is read AFTER the write, so an abandoned
                // request lands its cache and then stops — the retired form gated the write behind a token read and
                // threw away exactly the artifact the abandonment had already bought.
                None: () => Live(cancel)
                    .Bind(_ => companion.Cross(this, cancel)
                        .MapFail(error => (Error)Detail.CompanionUnreachable.At(key, error.Message)))
                    .Bind(glb => store.Store(Address, glb)
                        .MapFail(error => (Error)Detail.GlbStoreReject.At(key, error.Message))
                        .Bind(_ => Live(cancel))
                        .Bind(_ => Reenter(glb, TessellationOrigin.Tessellated, clock, time, at, mark, key)))));
    }

    static Fin<Unit> Live(CancellationToken cancel) =>
        cancel.IsCancellationRequested ? Fin.Fail<Unit>(new Fault.Cancelled()) : Fin.Succ(unit);

    Fin<TessellationOutcome> Reenter(ReadOnlyMemory<byte> glb, TessellationOrigin origin, IClock clock, TimeProvider time, Instant at, long mark, Op key) =>
        BimIo.ImportGeometry(InterchangeFormat.Glb, glb, clock, key)
            .Bind(geometry => Sound(geometry)
                ? Fin.Succ(new TessellationOutcome(
                    geometry, Address, ContentKey, SourceKey, Policy.Deflection,
                    geometry.VertexCount, geometry.TriangleCount, glb.Length, origin, time.GetElapsedTime(mark).ToDuration(), at))
                : Fin.Fail<TessellationOutcome>(new GeometryFault.DegenerateInput(Kind.Mesh, None, Detail.Of(Detail.TessellationDegenerate, Address.Value)).ToError()));

    // Re-imported GLBs are degenerate when empty, when the arena fails its own validity claim, or when a position
    // carries a non-finite coordinate; all three lower the kernel GeometryFault on the shared rail rather than
    // minting a hollow mesh — the boundary-validation scan is the named statement exemption. The arena's IsValid
    // already proves descriptor/payload agreement and a lossless witness, so this guard adds only the finiteness
    // check IsValid does not make, reading the position lane through its typed view rather than a raw span.
    static bool Sound(ImportedGeometry geometry) {
        if (geometry is not { VertexCount: > 0, TriangleCount: > 0 } || !geometry.Lanes.IsValid) { return false; }
        // Position lanes read through the Descriptors/Channel pair every sibling page composes — the arena's
        // one addressed read — rather than a View<T> projection no other consumer on this branch spells.
        return geometry.Lanes.Descriptors.Find(static d => d.Channel == EncodingChannel.Position).Match(
            Some: descriptor => {
                float[] raw = new float[descriptor.Floats];
                descriptor.Dtype.Unpack(geometry.Lanes.Channel(EncodingChannel.Position).Span, raw);
                return raw.All(static coordinate => float.IsFinite(coordinate));
            },
            None: static () => false);
    }

    // ContentKey folds the PURE SourceKey with EVERY GLB-affecting dimension — the InterchangePolicy
    // deflection/tolerance/angle AND the order-stable config (settings + scope), each written STRAIGHT onto the
    // one kernel seed-zero fold through its own Write — so the store address partitions on every input that
    // changes the GLB while SourceKey stays the tolerance-independent join; no second hasher and no
    // delimiter-joined intermediate token.
    static UInt128 Fold(UInt128 sourceKey, InterchangePolicy policy, TessellationSettings settings, TessellationScope scope) =>
        ContentHash.Of(scope.Write(settings.Write(new CanonicalWriter(0.0)
                .U128(sourceKey)
                .Double(policy.Deflection).Double(policy.Tolerance).Double(policy.AngleTolerance)))
            .ToBytes().Span);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Injected ports the app-platform binds at the composition edge. Bim mints no Persistence or Compute
// reference; it owns the cache/cross/store POLICY through these contracts while durable residence and
// transport are the bound implementations, composing upward with no downstream package edge. Lookup
// separates a store fault (Fin failure) from a normal miss (None); Store is the write-blob-first put.
public interface ITessellationStore {
    Fin<Option<ReadOnlyMemory<byte>>> Lookup(ArtifactKey address);
    Fin<Unit> Store(ArtifactKey address, ReadOnlyMemory<byte> glb);
}

// Crosses carry their governance on the contract in BCL currency: the token the transport threads, and nothing
// else. A Bim-minted lane or cancellation type crossing here is the AEC-DOMAIN leak the port exists to foreclose,
// and an `IProgress<double>` beside the token is the SINK WITH NO FEEDER the port equally forecloses — the
// companion runs out of process behind a transport carrying no progress channel, so the contract declares
// cancellation, which the transport threads, and declares no fraction, which nothing could report.
public interface ITessellationCompanion {
    Fin<ReadOnlyMemory<byte>> Cross(TessellationRequest request, CancellationToken cancel);
}
```

## [03]-[EXPLICIT_TESSELLATION]

- Owner: `BimIo.ImportIfcTessellation` the in-process decode of the ALREADY-TESSELLATED IFC representation family — `IfcTriangulatedFaceSet` and `IfcPolygonalFaceSet` over their shared `IfcCartesianPointList3D` coordinate store — onto the seam `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry`, contributing an `EncodingChannel.Uv` lane from the face set's OWN `HasTextures` texture map and an `EncodingChannel.ColorRgba` lane from its OWN `HasColours` map exactly when it declares one — an untextured or unpainted face set contributes no lane, never an empty column; the colour read composes the `Semantics/appearance#APPEARANCE_PROJECTION` `IndexedColour` value that owns both directions of the per-face radiometry, so this walk declares no colour shape and mints no accessor; `ExplicitTessellation` the split product pairing that geometry with the `GlobalId` residue the companion still owns.
- Entry: `BimIo.ImportIfcTessellation(DatabaseIfc db, IClock clock, Op key)` returns `Fin<ExplicitTessellation>`, walking the live graph once and partitioning every product's representation items into the explicitly-tessellated set this page decodes and the evaluated set the `TESSELLATION_BRIDGE` crosses; the caller hands `ExplicitTessellation.Deferred` straight to `TessellationScope.Elements` so `Plan` narrows the companion cross to exactly the products that need an evaluator. Malformed index runs — a corner past the coordinate count, a texture-coordinate index past the vertex list, a colour ordinal past the palette, a colour run shorter than the face count — rail `Model/faults#FAULT_BAND` `BimFault.ModelRejected` off `key`, lifted BARE (band 2600 IS the `Expected` `Code`, no `.ToError()` hop), so every bound belongs to the one `Try.lift` envelope and no read carries a guard of its own.
- Auto: `IfcTessellatedFaceSet` IS explicit mesh data — coordinates, corner indices, optional authored normals, an optional texture-coordinate list, and an optional per-face colour map — so evaluating it needs no solid kernel and crossing it to the companion is a round trip that COSTS a whole transport hop and DESTROYS both the IFC-native UV set and the radiometry, neither of which any glTF the companion returns carries. `HasTextures` is a SET of `IfcIndexedTextureMap`, each pairing a `TexCoords` `IfcTextureVertexList` with the `Maps` list naming WHICH `IfcSurfaceTexture` rows that parameterization serves, so the decode joins the UV set to the appearance roster by texture identity rather than by position. `HasColours` is a SINGLE `IfcIndexedColourMap` binding a palette, a one-based index run with one entry per FACE, and one `Opacity` the schema applies to every face alike, `NaN` reading as fully opaque; the `IndexedColour` value owns that read whole — unit-valued triples lowered to scene-linear through the appearance projector's own sRGB EOTF, so an IFC vertex colour and an IFC base colour reach a consumer in ONE space and this walk applies no transfer of its own. Both index forms address CORNERS where the coordinate store addresses VERTICES — the triangulated subtype's `TexCoordIndex` triples are per-triangle, the polygonal subtype's `TexCoordIndices` per-face, and the colour run per-face — so ONE gather decision owns them: a face set declaring either emits one vertex per corner, a face set declaring neither keeps the packed per-coordinate emit, and the per-coordinate texture-vertex form lands through the same gather at either length. Broadcasting a per-face colour or a per-corner UV onto a shared coordinate is the deleted form: the last face to write wins, which bleeds colour across a material boundary and tears the UV at every seam, and both defects render without a diagnostic.
- Receipt: `ExplicitTessellation` carries the decoded `ImportedGeometry`, the decoded product count, and the deferred `GlobalId` set — the split evidence a composition reads to know how much of a model needed an evaluator at all, and the reason a texture-bearing or colour-bearing IFC now round-trips its parameterization and its radiometry when the companion path cannot.
- Packages: GeometryGymIFC_Core (`IfcTessellatedFaceSet`/`IfcTextureVertexList`/`IfcIndexedTriangleTextureMap` — the triangulated UV-index payload reached through the `Semantics/appearance#APPEARANCE_PROJECTION` `IfcInternals` capsule under that catalog's `[INTERNAL_ACCESS_LAW]`; the polygonal `IfcTextureCoordinateIndices` row is PUBLIC (`TexCoordIndex` + the face's own `HasTexCoords` slot) and needs no capsule, as does the `IfcTextureCoordinate.Maps` bound-texture list the identity read composes; the colour payload crosses through that page's `IndexedColour`), Rasm.Element, Rasm (`EncodingChannel`), NodaTime, LanguageExt.Core
- Growth: a new tessellated subtype is one arm on the total representation-item dispatch; a new attribute lane is one `MeshChunk.Attributes` entry the SAME walk fills, the seam carrier already declaring both `EncodingChannel.Uv` and `EncodingChannel.ColorRgba`; a corner-indexed presentation payload beyond these joins the existing unweld discriminant rather than adding a second emit path, and an n-gon's arity is absorbed by the ONE `Fan` owner before `Slot` so every fan projection stays three-cornered; never a second IFC mesh decoder and never an in-process evaluator for a swept, BREP, or voided-face item.
- Boundary: `ImportIfcTessellation` decodes EXPLICIT indexed meshes and nothing else — an `IfcExtrudedAreaSolid`, an `IfcAdvancedBrep`, or any item requiring a solid kernel routes to `[02]-[TESSELLATION_BRIDGE]` unchanged, so the "no in-process arm64 solid evaluator" law is untouched and `TessellationRequiresCompanion` keeps its meaning. Reading a UV coordinate list or a colour palette is neither a codec nor a texel resample, so the `Rasm.Bim` "CLASSIFIES and CARRIES texture payloads and decodes none" ruling holds whole; the internal-payload reach is the `Semantics/appearance#APPEARANCE_PROJECTION` `IfcInternals` capsule and the colour shape is that page's `IndexedColour`, so this page declares neither — a presentation item seats on the presentation owner and `ARCHITECTURE.md` `[02]-[STRATA]` puts `Exchange` above `Semantics`, so composing them here is the downward edge the acyclic law admits while the reverse seating inverts the strata AND forks the palette fold into an ingest copy and an egress copy; one capsule pinned to one manifest version is the whole of this branch's `internal` GeometryGym surface and a second copy beside it forks the version pin. Decoded geometry lands the SAME `ImportedGeometry` the managed arms produce (via the `import#IMPORT_RAIL` pool builder), so no second carrier exists; frame normalization does not apply because IFC coordinates are already in the model frame the seam header declares. Texture PAYLOAD stays the `Semantics/appearance#APPEARANCE_PROJECTION` roster's — this owner carries only the coordinate set and the texture identity it binds to, so the two halves meet at the app-root edge exactly where the roster already crosses.

```csharp signature
// Split product: what decoded here, and what still needs the evaluator. Deferred is a GlobalId set precisely so
// it drops straight into TessellationScope.Elements — the companion cross narrows to the residue instead of
// re-evaluating a model whose tessellated majority is already in hand.
public sealed record ExplicitTessellation(
    ImportedGeometry Geometry, int DecodedProducts, Seq<string> Deferred, Seq<string> Textures);

// BimIo owns this decode as an arm, never a sibling class: it is the ONE bytes-and-graph->carrier decode owner and
// it alone holds the MeshSoup pool builder every arm folds into, so a second class here would either re-mint that
// builder or reach a private one. The partial is the same shape BimExport takes across its own section fences.
public static partial class BimIo {
    public static Fin<ExplicitTessellation> ImportIfcTessellation(DatabaseIfc db, IClock clock, Op key) =>
        Try.lift(() => Partition(db, clock, key)).Run().MapFail(error => (Error)Detail.IfcTessellation.At(key, error.Message));

    // One walk, one partition. A product whose representation items are ALL fan-decodable tessellations decodes
    // whole; a product carrying any evaluator-bound item defers WHOLE, because a half-decoded product would place
    // two fragments of one element under two content keys. A polygonal set holding an
    // IfcIndexedPolygonalFaceWithVoids defers with them — a face with interior voids needs a real triangulator,
    // and the companion IS the triangulator, so fanning around a hole here would seal the void shut and render
    // wrong. The clock is the caller's injected IClock — the carrier's At feeds a content key, so an ambient
    // SystemClock read here would break replay determinism.
    // Pool builders own pinned native-sized buffers, so this one is BRACKETED here exactly as every import arm
    // brackets its own — the walk is the named boundary-statement exemption, the custody is not.
    static ExplicitTessellation Partition(DatabaseIfc db, IClock clock, Op key) {
        using var soup = new MeshSoup();
        var deferred = Seq<string>();
        var textures = Seq<string>();
        int decoded = 0;
        foreach (var product in db.Project.Extract<IfcProduct>()) {
            var items = Optional(product.Representation).Match(
                None: () => Seq<IfcRepresentationItem>(),
                Some: shape => shape.Representations.AsIterable().Bind(static rep => rep.Items.AsIterable()).ToSeq());
            if (items.IsEmpty || !items.ForAll(static item => item is IfcTessellatedFaceSet set && Fannable(set))) {
                if (!items.IsEmpty) { deferred = deferred.Add(product.GlobalId); }
                continue;
            }
            items.Iter(item => {
                (MeshChunk chunk, Option<string> texture) = Decode((IfcTessellatedFaceSet)item);
                ignore(soup.Baked(chunk));
                texture.Iter(id => textures = textures.Add(id));
            });
            decoded++;
        }
        // Bound texture identities ride BESIDE the geometry: a landed Uv lane is only half the fact a consumer
        // needs, because the export binder must know WHICH texture the coordinates parameterize before it can set a
        // ChannelImage.CoordinateSet. Emitting the lane alone left that correspondence unowned at the composing edge.
        return new ExplicitTessellation(
            soup.ToGeometry(InterchangeFormat.Ifc, clock.GetCurrentInstant(), None, key), decoded, deferred, textures.Distinct());
    }

    static bool Fannable(IfcTessellatedFaceSet faceSet) =>
        faceSet is not IfcPolygonalFaceSet poly || poly.Faces.All(static face => face is not IfcIndexedPolygonalFaceWithVoids);

    // Coordinates are the packed IfcCartesianPointList3D store shared by both subtypes; the corner run is the
    // subtype's own index list, fan-triangulated for the polygonal case, each emitted triangle carrying the ordinal of the
    // SOURCE face it came from. GATHER is this walk's one shape decision, and it is forced by the two
    // CORNER-indexed presentation payloads IFC binds to a face set — the per-face colour run and the per-triangle UV
    // index triples: either present emits ONE VERTEX PER CORNER, because broadcasting a per-face colour or a
    // per-corner UV onto a coordinate two faces share hands that vertex whichever face writes last, a colour bleed
    // and a UV seam tear that render wrong and read right. Absent both, the gather is the coordinate identity and the
    // packed welded emit every per-coordinate face set wants stands unchanged. Authored normals ride the SAME gather
    // at per-coordinate arity; an absent set takes the up-normal the seam lane's absent case already carries.
    static (MeshChunk Chunk, Option<string> Texture) Decode(IfcTessellatedFaceSet faceSet) {
        var points = Coordinates(faceSet);
        var authored = Normals(faceSet);
        Option<Seq<(int A, int B, int C)>> normalRun = NormalIndex(faceSet);
        (long[] Corner, int[] Face) mesh = Corners(faceSet);
        Option<IndexedColour> colour = IndexedColour.Of(faceSet);
        // ONE plan answers the whole parameterization question BEFORE the gather is chosen: which index form the
        // set carries, which vertex list serves it, whether the two agree in arity, and which texture identity the
        // map binds. The retired order resolved the index form first and gated its arity inside the coordinate
        // read, so an index-bearing set whose vertex list failed the arity check UNWELDED every corner and then
        // landed no lane at all — paying the emit cost of a parameterization it had already refused.
        Option<UvPlan> uv = Uv(faceSet, mesh.Corner.Length / 3, points.Count);
        // Corner-addressed payloads — per-face colour, an INDEXED UV plan, or a NormalIndex re-index — force the
        // one-vertex-per-corner emit; broadcasting any of the three onto a shared coordinate is the last-write-wins
        // defect the gather discriminant exists to foreclose. A per-COORDINATE plan needs no unweld.
        bool unweld = colour.IsSome || normalRun.IsSome || uv.Exists(static plan => plan.Indexed);
        long[] gather = unweld ? mesh.Corner : Ordinals(points.Count);
        long[] corners = unweld ? Ordinals(mesh.Corner.Length) : mesh.Corner;
        var vertices = new float[gather.Length * 3];
        var normals = new float[gather.Length * 3];
        for (int v = 0; v < gather.Length; v++) {
            int source = (int)gather[v];
            (vertices[v * 3], vertices[(v * 3) + 1], vertices[(v * 3) + 2]) = points[source];
            // NormalIndex-bearing sets address normals by CORNER (v/3 the triangle, v%3 the corner, one-based);
            // an index-free authored set parallels the coordinate store; absence takes the seam's up-normal case.
            (normals[v * 3], normals[(v * 3) + 1], normals[(v * 3) + 2]) = normalRun.Match(
                Some: run => authored.Map(store => store[Slot(run[v / 3], v % 3) - 1]),
                None: () => authored.Map(store => store[source])).IfNone((0f, 0f, 1f));
        }
        float[] uvs = uv.Map(plan => Sampled(plan, gather)).IfNone([]);
        float[] paint = colour.Map(read => Painted(read, mesh.Face, gather.Length)).IfNone([]);
        return (new MeshChunk(vertices, normals, corners,
            (uvs.Length > 0 ? Seq((EncodingChannel.Uv, uvs)) : Seq<(EncodingChannel, float[])>())
            + (paint.Length > 0 ? Seq((EncodingChannel.ColorRgba, paint)) : Seq<(EncodingChannel, float[])>())),
            uv.Bind(static plan => plan.Texture));
    }

    // UvPlan is the resolved parameterization: its index run (EMPTY for the per-coordinate form), the vertex list it samples,
    // and the texture identity the map binds. Indexed is the unweld discriminant, so one value carries both facts
    // and neither can be decided without the other.
    readonly record struct UvPlan(Seq<(int A, int B, int C)> Index, Seq<(double U, double V)> List, Option<string> Texture) {
        public bool Indexed => !Index.IsEmpty;
    }

    // Ordinals is the identity gather — the welded coordinate order and the unwelded corner order are the same
    // expression at two lengths, so neither branch spells its own loop.
    static long[] Ordinals(int count) => [.. Enumerable.Range(0, count).Select(static i => (long)i)];

    // Painted lowers the per-FACE colour onto the per-vertex ColorRgba lane: emitted vertex v belongs to triangle v/3,
    // that triangle names its source face, and IndexedColour.Rgba resolves that face's palette row plus the map's
    // single Opacity into the four channels. The value owns the whole transfer — its palette is already scene-linear
    // and already unit-interval — so this walk scales nothing, and the /255 the byte-valued PLY arm needs is exactly the
    // defect here.
    static float[] Painted(IndexedColour colour, int[] faces, int vertexCount) {
        var lane = new float[vertexCount * 4];
        for (int v = 0; v < vertexCount; v++) {
            (double r, double g, double b, double a) = colour.Rgba(faces[v / 3]);
            (lane[v * 4], lane[(v * 4) + 1], lane[(v * 4) + 2], lane[(v * 4) + 3]) = ((float)r, (float)g, (float)b, (float)a);
        }
        return lane;
    }

    // Uv resolves the WHOLE plan in one pass. HasTextures is a SET, so a face set parameterized for several
    // textures carries one map per texture identity; the seam carrier declares ONE coordinate lane, so the FIRST
    // map whose form and arity both admit lands and the rest ride the appearance roster's own texture identity —
    // a further lane is one carrier column, never a second decode. Both index forms yield per-emitted-TRIANGLE
    // triples: the triangulated subtype's own run crosses through the IfcInternals capsule, and the polygonal
    // subtype's per-face IfcTextureCoordinateIndices (public TexCoordIndex, joined through the face's OWN public
    // HasTexCoords slot — no reverse TexCoordsOf walk) projects through the ONE Fan owner, each fan triangle taking
    // its corner slots off the face's UV row, so the polygon's arity is absorbed BEFORE Slot. An absent index form
    // is the schema's OTHER shape — a per-COORDINATE vertex list parallel to the coordinate store. Arity is the
    // LAST gate and it refuses the whole plan, so a mismatch yields the seam's typed absence rather than a
    // truncated or zero-padded lane AND leaves the gather welded.
    static Option<UvPlan> Uv(IfcTessellatedFaceSet faceSet, int triangles, int coordinateCount) =>
        faceSet.HasTextures.AsIterable()
            .Choose(map => Optional(map.TexCoords)
                .Map(list => new UvPlan(
                    Index(faceSet, map),
                    toSeq(list.TexCoordsList).Map(static uv => (U: uv.Item1, V: uv.Item2)),
                    // Bound identity is the map's OWN public Maps list — the IfcSurfaceTexture rows this
                    // parameterization serves — read as the StepId the appearance roster's SurfaceTexture.Of already
                    // carries for exactly this join, so the two halves meet at the app-root edge on one key. The
                    // capsule reaches internal members alone and Maps is public, so no accessor exists here.
                    map.Maps.AsIterable().Head.Map(static texture =>
                        texture.StepId.ToString(CultureInfo.InvariantCulture)))))
            .Filter(plan => plan.Indexed ? plan.Index.Count == triangles : plan.List.Count == coordinateCount)
            .Head;

    static Seq<(int A, int B, int C)> Index(IfcTessellatedFaceSet faceSet, IfcIndexedTextureMap map) => (faceSet, map) switch {
        (IfcTriangulatedFaceSet, IfcIndexedTriangleTextureMap triangle) => IfcInternals.TexCoordRun(triangle),
        // All-or-nothing by the applicative Traverse: one face without a UV row makes the whole run EMPTY — which the
        // arity gate then reads as the per-coordinate form and refuses — never a zero-triple standing in.
        (IfcPolygonalFaceSet poly, _) => Fan(poly)
            .Traverse(static tri => Optional(tri.Face.HasTexCoords)
                .Map(row => (A: row.TexCoordIndex[tri.I0], B: row.TexCoordIndex[tri.I1], C: row.TexCoordIndex[tri.I2])))
            .As().IfNone(Seq<(int, int, int)>()),
        _ => Seq<(int A, int B, int C)>(),
    };

    // Sampled writes the UV lane in emitted-vertex order: an INDEXED plan takes the ordinate off the vertex's own
    // corner slot — v/3 the triangle, v%3 the corner, one-based into the vertex list — and a per-coordinate plan
    // takes the gathered coordinate. A vertex list too short for an index is a malformed file and throws inside
    // Partition's Try.lift envelope beside the colour-run bound, so neither read needs a guard of its own.
    static float[] Sampled(UvPlan plan, long[] gather) {
        var uvs = new float[gather.Length * 2];
        for (int v = 0; v < gather.Length; v++) {
            (double s, double t) = plan.List[plan.Indexed ? Slot(plan.Index[v / 3], v % 3) - 1 : (int)gather[v]];
            (uvs[v * 2], uvs[(v * 2) + 1]) = ((float)s, (float)t);
        }
        return uvs;
    }

    // Slot picks a positional triple's ordinate by corner index — one expression instead of a three-arm ladder at
    // every read site, and the shape a quad or n-gon index list widens through.
    static int Slot((int A, int B, int C) triple, int corner) => corner switch { 0 => triple.A, 1 => triple.B, _ => triple.C };

    // Point payloads discriminate ONCE: IfcTessellatedFaceSet.Coordinates is typed to the abstract
    // IfcCartesianPointList base, the 3D subtype carries CoordList (List<Tuple<double,double,double>>, one tuple
    // per point), and a 2D list is a curve-set payload no face-set body legally carries — it yields the empty
    // store the arity gates downstream refuse.
    static System.Collections.Generic.IReadOnlyList<(float X, float Y, float Z)> Coordinates(IfcTessellatedFaceSet faceSet) =>
        faceSet.Coordinates is IfcCartesianPointList3D list
            ? list.CoordList.ConvertAll(static p => ((float)p.Item1, (float)p.Item2, (float)p.Item3))
            : [];

    // Authored per-COORDINATE (or, with NormalIndex, corner-addressed) normal store — the triangulated
    // subtype's own get-only List<Tuple<double,double,double>>; the polygonal subtype declares none.
    static Option<System.Collections.Generic.IReadOnlyList<(float X, float Y, float Z)>> Normals(IfcTessellatedFaceSet faceSet) =>
        faceSet is IfcTriangulatedFaceSet { Normals.Count: > 0 } tri
            ? Some<System.Collections.Generic.IReadOnlyList<(float X, float Y, float Z)>>(
                tri.Normals.ConvertAll(static n => ((float)n.Item1, (float)n.Item2, (float)n.Item3)))
            : None;

    // Optional per-triangle corner re-index for authored normals (List<Tuple<int,int,int>>, one-based) —
    // present forces the unweld gather exactly as the colour and UV runs do.
    static Option<Seq<(int A, int B, int C)>> NormalIndex(IfcTessellatedFaceSet faceSet) =>
        faceSet is IfcTriangulatedFaceSet { NormalIndex.Count: > 0 } tri
            ? Some(toSeq(tri.NormalIndex).Map(static t => (A: t.Item1, B: t.Item2, C: t.Item3)))
            : None;

    // Corner run and its per-emitted-triangle source-face ordinals: the triangulated subtype's CoordIndex is
    // a one-based per-triangle triple run (Face[t] = t); the polygonal subtype fans through the ONE Fan owner
    // (the polygon ordinal repeated across its fan). BOTH subtypes carry an optional PnIndex indirection — the
    // triangulated a List<int>, the polygonal GeometryGym's own LIST<int> — and every index resolves through it
    // exactly once, so a shared Point resolver per subtype is the whole indirection.
    static (long[] Corner, int[] Face) Corners(IfcTessellatedFaceSet faceSet) => faceSet switch {
        IfcTriangulatedFaceSet tri => (
            [.. tri.CoordIndex.SelectMany(t => new long[] { Point(tri.PnIndex, t.Item1), Point(tri.PnIndex, t.Item2), Point(tri.PnIndex, t.Item3) })],
            [.. Enumerable.Range(0, tri.CoordIndex.Count)]),
        IfcPolygonalFaceSet poly => Fanned(Fan(poly), poly.PnIndex),
        _ => ([], []),
    };

    // ONE fan walk feeds BOTH polygonal projections. Calling the fan owner once per projection built two walks whose
    // agreement about triangle order was incidental rather than structural — exactly what a single fan owner exists
    // to foreclose — and paid the polygon traversal twice on every decode.
    static (long[] Corner, int[] Face) Fanned(
        Seq<(IfcIndexedPolygonalFace Face, int Ordinal, int I0, int I1, int I2)> fan,
        System.Collections.Generic.IReadOnlyList<int> pnIndex) => (
        [.. fan.Bind(tri => Seq(
            Point(pnIndex, tri.Face.CoordIndex[tri.I0]),
            Point(pnIndex, tri.Face.CoordIndex[tri.I1]),
            Point(pnIndex, tri.Face.CoordIndex[tri.I2])))],
        [.. fan.Map(static tri => tri.Ordinal)]);

    // One-based IFC index -> zero-based point ordinal, through the optional PnIndex indirection exactly once.
    static long Point(System.Collections.Generic.IReadOnlyList<int> pnIndex, int index) =>
        pnIndex is { Count: > 0 } ? pnIndex[index - 1] - 1 : index - 1;

    // THE one fan owner: each polygonal face fans (0, i, i+1) over its own corner-slot ordinals, every projection
    // — the coordinate run, the face-ordinal run, the per-face UV row read — deriving from this single walk so no
    // two consumers can disagree about the fan structure. Slots are POSITIONS within the face's CoordIndex (and
    // its parallel TexCoordIndex), so the polygon's arity never reaches Slot.
    static Seq<(IfcIndexedPolygonalFace Face, int Ordinal, int I0, int I1, int I2)> Fan(IfcPolygonalFaceSet poly) =>
        toSeq(poly.Faces).Map(static (face, ordinal) => (Face: face, Ordinal: ordinal))
            .Bind(static entry => toSeq(Enumerable.Range(1, entry.Face.CoordIndex.Count - 2))
                .Map(i => (entry.Face, entry.Ordinal, I0: 0, I1: i, I2: i + 1)));
}
```

## [04]-[RESEARCH]

(none)
