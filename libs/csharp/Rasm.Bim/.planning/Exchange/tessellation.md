# [BIM_TESSELLATION_BRIDGE]

The imported-geometry tessellation bridge: one `TessellationRequest` crossing IFC/STEP/IGES/native geometry evaluation to the `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` companion rail (IfcOpenShell `IfcConvert` producing GLB) and re-importing the GLB through the `import#IMPORT_RAIL` glTF path, plus the `TessellationOutcome` typed receipt carrying the dual keys, the mesh evidence, the origin, and the monotonic latency. The bridge owns the cache-before-cross-and-store-before-return policy over two injected ports — the content-addressed `TessellationStore` and the `TessellationCompanion` cross — so the durable GLB residence and the companion transport stay app-platform-bound while this AEC-DOMAIN owner mints no `Rasm.Persistence` or `Rasm.Compute` reference and depends strictly upward. A dual content key separates the pure `SourceKey` (the cross-projection join one source artifact shares between its GLB and semantic-graph projections) from the `ContentKey` (the GLB identity folding the full tessellation config), and the typed `TessellationScope`/`TessellationSettings` carry the `IfcConvert` filter and geometry-settings surface the companion maps. The page composes the `format#FORMAT_AXIS` `TessellationRequiresCompanion` gate, the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` content key, the `Rasm.Compute/Runtime/channels#TRANSPORT_AXIS` transport, and the `import#IMPORT_RAIL` `ImportedGeometry` re-entry as settled vocabulary. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[TESSELLATION_BRIDGE]: IFC/STEP/IGES/native geometry crosses to the companion rail, never in-process; the dual-key `TessellationOutcome` carries the re-imported geometry, the mesh-evidence and latency columns, the content-key store reuse, and the cross-projection `SourceKey` join.

## [02]-[TESSELLATION_BRIDGE]

- Owner: `TessellationRequest` — the Bim-side request the imported-geometry tessellation bridge crosses to the IfcOpenShell companion (`IfcConvert` producing GLB) and re-imports through the `import#IMPORT_RAIL` glTF path; `TessellationOutcome` the typed receipt; `TessellationStore`/`TessellationCompanion` the injected ports the app-platform binds; `TessellationScope`/`TessellationSettings`/`TessellationOrigin` the request vocabulary. The bridge owns the cache-before-cross-and-store-before-return POLICY; the companion transport and the durable GLB residence are the bound port implementations, never types this AEC-DOMAIN owner mints.
- Entry: `TessellationRequest.Plan(InterchangeFormat source, ReadOnlyMemory<byte> sourceBytes, InterchangePolicy policy, Op key, Option<TessellationSettings> settings, Option<TessellationScope> scope)` gates on `source.TessellationRequiresCompanion` and mints the dual-key request, defaulting `settings`/`scope` to `TessellationSettings.Canonical`/`TessellationScope.Whole` so the whole-model canonical case is one call; `request.Resolve(TessellationStore store, TessellationCompanion companion, ClockPolicy clocks, Op key)` reads the content-addressed GLB store by `ArtifactKey` BEFORE the companion cross — a hit re-imports the cached GLB with `TessellationOrigin.Cached` and no round-trip, a miss crosses the companion over `Rasm.Compute/Runtime/channels#TRANSPORT_AXIS`, stores the fresh GLB write-blob-first, and re-imports it with `TessellationOrigin.Tessellated`.
- Auto: the dual-key derivation separates concerns — `SourceKey` the PURE source-artifact identity (the kernel `Rasm.Domain.ContentHash` seed-zero `XxHash128` over the source key plus the raw source bytes — NO tolerances) is the cross-projection join the GLB row and the IFC-semantic-graph row of one source share, so the join holds tolerance-INDEPENDENTLY whatever deflection a tessellation runs at; `ContentKey` folds that `SourceKey` with EVERY GLB-affecting dimension — the `InterchangePolicy` deflection/tolerance/angle AND the order-stable case-preserved config (`TessellationSettings` plus `TessellationScope`) — through the same kernel content-hash so two tessellations of one model differing only in a weld flag, a deflection, or an element filter never collide on the store address, the content key encoding every dimension that changes the GLB while the source identity stays pure; `ArtifactKey` resolves to `$"{ContentKey:x32}:glb"`, the store lookup address and the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` `:glb` artifact suffix. `Plan` reads the `format#FORMAT_AXIS` `TessellationRequiresCompanion` column so a managed format never crosses, and the companion maps the typed `TessellationSettings` onto the `ifcopenshell.geom.settings` keys (`mesher-linear-deflection`, `weld-vertices`, `use-world-coords`, `apply-default-materials`, `generate-uvs`, `disable-opening-subtractions`, `dimensionality`) and the `TessellationScope` onto the `IfcConvert` filters (`--include attribute GlobalId`, `--include entities`, `--exclude entities`), the wire detail owned by the Python companion and ridden over Compute's existing rpc.
- Receipt: `TessellationOutcome` is the typed tessellation evidence on the `Fin<T>` rail — never a generic `IReceipt`/ledger — carrying the re-imported `ImportedGeometry` mesh-scene, the `ArtifactKey`/`ContentKey`/`SourceKey` coordinates, the `Deflection` the content-key folded, the mesh-evidence columns (`VertexCount`/`TriangleCount` off the SharpGLTF-decoded re-import, `GlbByteCount` off the store blob — an evidence projection of the receipt reads the tessellation yield without traversing the payload or the store), the `TessellationOrigin` discriminating a content-key store reuse from a fresh companion cross, the `Duration Took` monotonic latency (`ClockPolicy.Mark`/`Elapsed` spanning the whole `Resolve` — a `Cached` hit and a `Tessellated` cross read as one column beside `Origin`, the cache-effectiveness read), and the `Instant At` stamp; a cached reuse and a fresh cross surface one outcome shape so a caller reads the origin from the receipt rather than a side channel, and the receipt carries coordinates, counts, and hashes, never payload bytes — the GLB rides the `TessellationStore` port, not the receipt.
- Packages: LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions, Rasm, Rasm.Element
- Growth: a new tessellation parameter is one column on `TessellationSettings` folded into `ContentKey` and one `ifcopenshell.geom.settings` key the companion maps; a new scope modality (a layer filter, an exclude polarity) is one `TessellationScope` case plus one `--include`/`--exclude` mapping; a new companion-evaluated source format is one `InterchangeFormat` row carrying `TessellationRequiresCompanion=true` on `format#FORMAT_AXIS`; never a new transport, never a second bridge, never a managed in-process tessellator.
- Boundary: the companion bridge is the single imported-geometry-to-GLB path because GeometryGym (the IFC semantic graph) and the in-process `import#IMPORT_RAIL` `StepReader` (the STEP semantic leg) carry no tessellation kernel and no in-process arm64 IFC/STEP/IGES solid evaluator is admitted — a managed BRep tessellator is the deleted form, and the `IfcConvert`/ifcopenshell companion is the permanent default because the only .NET web-ifc binding is Windows C++/CLI. The bridge depends strictly UPWARD and mints no `Rasm.Persistence` or `Rasm.Compute` reference: the durable GLB store (the `csharp:Rasm.Persistence/Store` content-addressed object store, write-blob-first) and the companion cross (the `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` rpc) are the injected `TessellationStore`/`TessellationCompanion` ports the app-platform binds at the composition edge, so the bridge owns the cache-before-cross-and-store-before-return policy without leaking a store or transport type into this owner — and because both keys derive from the kernel `ContentHash` and the seam `Projection/address#CANONICAL_WRITER` `CanonicalWriter`, this AEC-DOMAIN owner mints no `Rasm.Compute` content-identity reference (the strata leak a `Rasm.Compute.InterchangeIdentity` call would re-open). Write-blob-first: the fresh GLB is durably stored before the `ArtifactKey` the outcome carries becomes a valid reference, so a store-write reject fails the cross rather than minting a dangling reference. A store-unreachable lookup and a store-write reject are the DECLARED `BimFault.CodecReject` degrade — the same `CodecReject` arm `faults#FAULT_BAND` assigns the bSDD service-unreachable degradation, never a sixth arm; a companion-unreachable cross and a non-companion source format are the `BimFault.CapabilityMiss` arm `faults#FAULT_BAND` declares (the companion the rail cannot reach, the in-process evaluation owning no kernel), the `Plan` gate railing the latter; each typed `BimFault` case lifts BARE onto the `Fin<T>` rail (band 2600 IS the `Expected` `Code`, no `.ToError()` hop). An empty or non-finite re-imported vertex set lowers the kernel `Rasm` `GeometryFault.DegenerateInput(...).ToError()` onto the shared `Fin<ImportedGeometry>` rail per `faults#FAULT_BAND` (the kernel band is not `Expected`-derived, so it keeps its own `.ToError()` lowering), never re-cased in Bim — never an exception, never a silent empty mesh. The `SourceKey` is the cross-projection join the app-platform `csharp:Rasm.Persistence/Store` artifact-index projection owns: the GLB row and the IFC-semantic-graph row of one source share one PURE `SourceKey` (the kernel content-hash over the source bytes — tolerance-independent, so the in-process semantic-graph ingest re-derives the identical key without knowing the tessellation deflection) while the bridge reads only its own `ArtifactKey` address, so the IFC semantic graph (from the `import#IMPORT_RAIL` in-process ingest) and the tessellated geometry (from this hop) stay two projections of one content-keyed source artifact joined at the seam. The re-imported GLB is glTF-canonical Y-up; the `import#IMPORT_RAIL` `FrameNormalization` coerces it to the kernel Z-up frame by the `InterchangeFormat.Glb` row, so this page mints no frame transform. The companion is the IfcOpenShell PyPI package living in `libs/python/geometry` (`python:geometry/ifc-companion`), never a NuGet pin, reached only through Compute's existing companion rpc so this page mints no transport, no channel, and no second wire vocabulary; the STEP (`step-iso10303`) and ANSI IGES (`iges-ansi`) geometry legs ride the same companion shape over `format#FORMAT_AXIS` (OpenCascade serving the STEP/IGES solid read), so a new companion-evaluated solid format is one row gated by `TessellationRequiresCompanion`, never a second bridge.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Linq;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Element;
using Thinktecture;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The ifcopenshell `dimensionality` value (CURVES=0, SURFACES_AND_SOLIDS=1 default, CURVES_SURFACES_AND_SOLIDS=2);
// the byte value IS the companion key, so the names track ifcopenshell — there is no solids-only mode.
public enum Dimensionality : byte { Curves = 0, SurfacesAndSolids = 1, CurvesSurfacesAndSolids = 2 }

// The tessellation scope is the IfcConvert geometry filter: the whole model, an explicit GlobalId set
// (--include attribute GlobalId, the per-element/instancing modality), an entity-type set to keep
// (--include entities), or an entity-type set to drop (--exclude entities — IfcSpace/IfcOpeningElement/
// IfcAnnotation off the tessellation, the dominant IFC-to-GLB cull). The case IS the modality — a filter-mode
// flag beside the values is the deleted form; a layer filter or an exclude-by-GlobalId set is one further case.
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
// The geometry-evaluation settings the companion maps onto ifcopenshell.geom.settings; ApplyDefaultMaterials
// is required for glTF serialisation, UseElementGuids lands the GlobalId on the GLB node so the per-element
// metadata join holds. Deflection/tolerance/angle live on InterchangePolicy and fold into ContentKey beside
// these flags; a further geometry-affecting knob (e.g. layerset slicing, local placement) lands as one more
// column the companion maps, per the Growth rule — never a second request shape.
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
// Geometry payload or the store-resident GLB; Took is the ClockPolicy monotonic Mark/Elapsed over the whole Resolve.
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
    // plus the raw bytes, NO tolerances — so the GLB projection and the IFC-semantic-graph projection of one
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
    public Fin<TessellationOutcome> Resolve(TessellationStore store, TessellationCompanion companion, ClockPolicy clocks, Op key) {
        var at = clocks.Now;
        long mark = clocks.Mark();
        return store.Lookup(ArtifactKey)
            .MapFail(error => new BimFault.CodecReject(key, $"glb-store-unreachable:{error.Message}"))
            .Bind(hit => hit.Match(
                Some: glb => Reenter(glb, TessellationOrigin.Cached, clocks, at, mark, key),
                None: () => companion.Cross(this)
                    .MapFail(error => new BimFault.CapabilityMiss(key, $"companion-unreachable:{error.Message}"))
                    .Bind(glb => store.Store(ArtifactKey, glb)
                        .MapFail(error => new BimFault.CodecReject(key, $"glb-store-reject:{error.Message}"))
                        .Bind(_ => Reenter(glb, TessellationOrigin.Tessellated, clocks, at, mark, key)))));
    }

    Fin<TessellationOutcome> Reenter(ReadOnlyMemory<byte> glb, TessellationOrigin origin, ClockPolicy clocks, Instant at, long mark, Op key) =>
        BimIo.ImportGeometry(InterchangeFormat.Glb, glb, clocks, key)
            .Bind(geometry => Sound(geometry)
                ? Fin.Succ(new TessellationOutcome(
                    geometry, ArtifactKey, ContentKey, SourceKey, Policy.Deflection,
                    geometry.VertexCount, geometry.TriangleCount, glb.Length, origin, clocks.Elapsed(mark), at))
                : Fin.Fail<TessellationOutcome>(GeometryFault.DegenerateInput($"tessellation-degenerate:{ArtifactKey}").ToError()));

    // A re-imported GLB is degenerate when empty or carrying a non-finite coordinate; both lower the kernel
    // GeometryFault on the shared rail rather than minting a hollow mesh — the boundary-validation scan is the
    // named statement exemption.
    static bool Sound(ImportedGeometry geometry) {
        if (geometry is not { VertexCount: > 0, TriangleCount: > 0 }) { return false; }
        foreach (float coordinate in geometry.Vertices.Span) { if (!float.IsFinite(coordinate)) { return false; } }
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
// The injected ports the app-platform binds at the composition edge: the content-addressed GLB store over
// csharp:Rasm.Persistence/Store and the companion cross over Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION.
// Bim mints no Persistence or Compute reference — it owns the cache/cross/store POLICY through these contracts
// while the durable residence and the transport are the bound implementations, so the bridge composes upward
// without a downstream package edge. Lookup separates a store fault (Fin failure) from a normal miss (None);
// Store is the write-blob-first put.
public interface TessellationStore {
    Fin<Option<ReadOnlyMemory<byte>>> Lookup(string artifactKey);
    Fin<Unit> Store(string artifactKey, ReadOnlyMemory<byte> glb);
}

public interface TessellationCompanion {
    Fin<ReadOnlyMemory<byte>> Cross(TessellationRequest request);
}
```

## [03]-[RESEARCH]

- [COMPANION_SETTINGS]: the typed `TessellationSettings`/`TessellationScope` map onto the IfcOpenShell companion's geometry contract — `ifcopenshell.geom.settings().set(...)` over the string keys `mesher-linear-deflection`/`mesher-angular-deflection` (the `InterchangePolicy.Deflection`/`AngleTolerance`), `weld-vertices`, `use-world-coords`, `apply-default-materials` (required for the GLB serialiser), `generate-uvs`, `disable-opening-subtractions`, and `dimensionality` (`CURVES_SURFACES_AND_SOLIDS`), plus the serialiser `use-element-guids` so the GLB node carries the IFC `GlobalId` for the per-element metadata join — and the `TessellationScope` onto the `IfcConvert` filter grammar (`--include attribute GlobalId <id>...`, `--include entities <IfcType>...`, `--exclude entities <IfcType>...`); the argument mapping and the GLB stream-back contract are owned by `python:geometry/ifc-companion` and orchestrated by `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION`, this page owning only the typed request shape, the dual content key, and the re-entry receipt — a new setting is one `TessellationSettings` column the companion reads, never a second invocation shape.
- [STEP_IGES_GEOMETRY_LEG]: the three ISO 10303 STEP application protocols (AP203/AP214/AP242) share the `step-iso10303` codec and the ANSI IGES file rides the distinct `iges-ansi` codec on `format#FORMAT_AXIS`, all carrying `TessellationRequiresCompanion=true` — their B-rep/NURBS solid evaluation crosses THIS bridge identically to the IFC geometry request because no managed STEP/IGES solid evaluator is admitted (OpenCascade serves the STEP/IGES solid read in the companion), while the AP242 PMI/product-structure semantic leg lands in-process through the `import#IMPORT_RAIL` `StepReader`; so the single `TessellationRequest` shape serves every companion-evaluated source format (`InterchangeFormat.Source` discriminating the companion reader) and a per-protocol or per-format bridge is the deleted form.
