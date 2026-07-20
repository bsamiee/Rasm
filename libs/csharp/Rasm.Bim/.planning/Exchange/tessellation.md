# [BIM_TESSELLATION_BRIDGE]

`TessellationRequest` crosses imported IFC/STEP/IGES/native geometry to the IfcOpenShell companion — `IfcConvert` producing GLB — and re-imports that GLB through the `import#IMPORT_RAIL` glTF path, minting the `TessellationOutcome` receipt over the dual content keys, the mesh evidence, the origin, and the monotonic latency. This bridge owns the cache-before-cross-and-store-before-return policy over two injected ports, so this AEC-DOMAIN owner mints no `Rasm.Persistence` or `Rasm.Compute` reference, depends strictly upward, and stays HOST-LOCAL.

Two injected ports carry the policy: the content-addressed `TessellationStore` over `csharp:Rasm.Persistence/Store` and the `TessellationCompanion` cross over `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION`. Composed as settled vocabulary: the `format#FORMAT_AXIS` `TessellationRequiresCompanion` gate, the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` content key, the `Rasm.Compute/Runtime/transport#TRANSPORT_AXIS` transport, and the `import#IMPORT_RAIL` `ImportedGeometry` re-entry.

## [01]-[INDEX]

- [01]-[TESSELLATION_BRIDGE]: imported IFC/STEP/IGES geometry crosses to the companion rail and re-imports as GLB; the dual-key `TessellationOutcome` receipt.

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
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Element.Projection;
using Thinktecture;

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

## [03]-[RESEARCH]

(none)
