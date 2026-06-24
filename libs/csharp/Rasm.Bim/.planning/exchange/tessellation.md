# [BIM_TESSELLATION_BRIDGE]

The IFC/AP242/native geometry tessellation bridge: one `TessellationRequest` shape crossing geometry evaluation to the `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` companion rail (IfcOpenShell `IfcConvert` producing GLB) and re-importing the GLB through the `import#IMPORT_RAIL` glTF path, plus the `TessellationOutcome` typed receipt the `Resolve` cache leg returns. The request is host-local in posture and rides Compute's existing companion transport, never a new transport and never the orchestration itself; the outcome carries the re-imported `ImportedGeometry`, the resolved GLB `ArtifactKey`, the deflection the content-key folded, a `CacheHit` discriminant, and the `Instant` stamp. The page composes the `format#FORMAT_AXIS` `TessellationRequiresCompanion` gate, the `Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` content key, the `Rasm.Compute/Runtime/channels#TRANSPORT_AXIS` transport, and the `import#IMPORT_RAIL` `ImportedGeometry` re-entry as settled vocabulary. The page is HOST-LOCAL.

## [01]-[INDEX]

- [01]-[TESSELLATION_BRIDGE]: IFC/AP242/native geometry crosses to the Compute companion rail, never in-process; the `TessellationOutcome` receipt carries the re-imported GLB and the content-key cache-hit fact.

## [02]-[TESSELLATION_BRIDGE]

- Owner: `TessellationRequest` — the Bim-side request shape crossing IFC/AP242/native geometry evaluation to the `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` companion rail (IfcOpenShell `IfcConvert` producing GLB) and re-importing the GLB through the `import#IMPORT_RAIL` glTF path; the request is host-local in posture and rides Compute's existing companion transport, never a new transport and never the orchestration itself; `TessellationOutcome` the typed tessellation receipt carrying the re-imported `ImportedGeometry`, the resolved GLB `ArtifactKey`, the deflection the content-key folded, the `CacheHit` discriminant, and the `Instant` stamp.
- Entry: `TessellationRequest.Plan(InterchangeFormat source, ReadOnlyMemory<byte> ifcBytes, InterchangePolicy policy)` builds the request keyed on the IFC content and the deflection/tolerance policy; `TessellationRequest.Resolve(InterchangeIdentity cache)` reads the durable artifact index by `ArtifactKey` BEFORE the companion cross and returns a `TessellationOutcome` — a content-key hit re-imports the cached GLB with `CacheHit = true` and no companion round-trip, a miss issues the request over `Rasm.Compute/Runtime/channels#TRANSPORT_AXIS` and re-enters the fresh GLB through `BimIo.ImportGeometry(InterchangeFormat.Glb, ...)` with `CacheHit = false`.
- Auto: `Plan` reads `InterchangeFormat.TessellationRequiresCompanion` to gate the hop so a non-IFC format never crosses; the request carries the IFC bytes, the deflection and tolerance from `InterchangePolicy`, and the Compute content-key so a re-tessellation of the same model at the same deflection reuses the cached GLB by reference rather than re-crossing the companion; `Resolve` looks the prior GLB up in the durable artifact index by the string `ArtifactKey` (`$"{IfcContentKey:x32}:glb"`, the row's `Path` lookup address) riding the same `InterchangeIdentity.Key` content-key the artifact is addressed by per the ARCHITECTURE `[CONTENT_KEY_IDIOM]`; the `IfcContentKey` `UInt128` Bim folds is also the value that becomes the index row's `SourceKey` (the cross-projection join the Persistence `ArtifactIndexRow.Project` fold owns, never a Bim concern), so the GLB row and the IFC-semantic graph row of one source IFC share one `SourceKey` while Bim reads only its own `ArtifactKey` address — the index lookup, the `SourceKey` join, and the durable residence are the app-platform caller's concern at the seam, Bim minting no `Rasm.Persistence` reference and depending strictly upward.
- Receipt: `TessellationOutcome` is the typed tessellation evidence on the `Fin<T>` rail — never a generic `IReceipt`/ledger — carrying the re-imported `ImportedGeometry` mesh-scene, the resolved `ArtifactKey`, the `Deflection` the content-key folded, the `CacheHit` fact discriminating a content-key reuse from a fresh companion cross, and the `Instant At` stamp; a cache-hit reuse and a fresh cross both surface one outcome shape so a caller reads the cache fact from the receipt rather than a side channel.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing, Rasm
- Growth: a new evaluation parameter is one column on `TessellationRequest` folded into the Compute content-key and one column on `TessellationOutcome` if it carries forward; never a new transport.
- Boundary: the companion bridge is the single IFC-to-geometry path because GeometryGym carries no tessellation kernel — a managed IFC BRep evaluator is the deleted form; the tessellation REQUEST orchestration (issuing `TessellationRequest` over the companion rpc, re-importing the GLB, and the content-key cache reuse) is owned at `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION` and Bim builds only the request shape, the `Resolve` cache read, and the `TessellationOutcome` receipt, consuming the re-imported GLB through `import#IMPORT_RAIL`; a companion-unreachable model surfaces the DECLARED `BimFault.CapabilityMiss($"companion-unreachable:{source.Key}")` degrade (band 2605, the same `CapabilityMiss` arm `Plan` rails for `tessellation-not-required` — reuse the settled arm, never a sixth band arm), never an exception, never a silent empty mesh; the shared degenerate-geometry failure (an empty or non-finite re-imported vertex set) routes the kernel `Rasm` `GeometryFault.DegenerateInput(...).ToError()` onto the same `Fin<ImportedGeometry>` rail per `faults#FAULT_BAND`, never re-cased in Bim; the companion is the IfcOpenShell PyPI package living in `libs/python/geometry` (`python:geometry/ifc-companion`), never a NuGet pin, reached only through Compute's existing companion rpc so this page mints no transport, no channel, and no second wire vocabulary; the durable artifact index lookup `Resolve` joins by `ArtifactKey` is the app-platform caller's concern at the `Exchange/tessellation -> csharp:Rasm.Persistence/Query [CONTENT_KEY]` seam, never a `Rasm.Persistence` reference here — Bim is AEC-DOMAIN and depends strictly upward; the IFC semantic graph (from the `import#IMPORT_RAIL` in-process ingest) and the tessellated geometry (from this hop) are two projections of one content-keyed IFC artifact joined by the Compute content-key; the AP242 CAD-STEP bridge and the ANSI IGES `iges-ansi` codec ride the same companion shape over `format#FORMAT_AXIS` (OpenCascade serves the STEP/IGES solid read), so a new companion-evaluated solid format is one row gated by `TessellationRequiresCompanion`, never a second bridge.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// The artifact-index + companion seam handles. Bim depends strictly UPWARD and mints no
// Rasm.Persistence reference: the cache lookup and the companion cross are injected ports the
// app-platform caller binds at the composition edge (the artifact index over Persistence/Query,
// the cross over Compute's TWO_HOP_TESSELLATION rail), so Resolve owns the cache-before-cross
// policy without leaking a durable-store or transport type into this AEC-DOMAIN owner.
public interface TessellationCache {
    Option<ReadOnlyMemory<byte>> Lookup(string artifactKey);
}

public interface TessellationCompanion {
    Fin<ReadOnlyMemory<byte>> Cross(TessellationRequest request);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record TessellationOutcome(
    ImportedGeometry Geometry,
    string ArtifactKey,
    double Deflection,
    bool CacheHit,
    Instant At);

public sealed record TessellationRequest(
    UInt128 IfcContentKey,
    ReadOnlyMemory<byte> IfcBytes,
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    InterchangeFormat Result) {
    public static Fin<TessellationRequest> Plan(InterchangeFormat source, ReadOnlyMemory<byte> ifcBytes, InterchangePolicy policy) =>
        source.TessellationRequiresCompanion
            ? Fin.Succ(new TessellationRequest(
                InterchangeIdentity.Key(source.Key, ifcBytes.ToArray(), policy.Deflection, policy.Tolerance, policy.AngleTolerance), ifcBytes,
                policy.Deflection, policy.Tolerance, policy.AngleTolerance, InterchangeFormat.Glb))
            : Fin.Fail<TessellationRequest>(new BimFault.CapabilityMiss($"tessellation-not-required:{source.Key}").ToError());

    public string ArtifactKey => $"{IfcContentKey:x32}:glb";

    // Cache-before-cross: a content-key hit re-imports the cached GLB with CacheHit = true and no
    // companion round-trip; a miss crosses Compute's companion rail and re-imports the fresh GLB
    // with CacheHit = false. A companion-unreachable miss is the DECLARED CapabilityMiss degrade,
    // never an exception or a silent empty mesh; a degenerate re-imported vertex set lowers the
    // kernel GeometryFault.DegenerateInput on the shared Fin<ImportedGeometry> rail.
    public Fin<TessellationOutcome> Resolve(TessellationCache cache, TessellationCompanion companion, ClockPolicy clocks) =>
        cache.Lookup(ArtifactKey).Match(
            Some: glb => Reenter(glb, cacheHit: true, clocks.Now),
            None: () => companion.Cross(this)
                .MapFail(static error => new BimFault.CapabilityMiss($"companion-unreachable:{error.Message}").ToError())
                .Bind(glb => Reenter(glb, cacheHit: false, clocks.Now)));

    Fin<TessellationOutcome> Reenter(ReadOnlyMemory<byte> glb, bool cacheHit, Instant at) =>
        BimIo.ImportGeometry(InterchangeFormat.Glb, glb, ClockPolicy.At(at))
            .Map(geometry => new TessellationOutcome(geometry, ArtifactKey, Deflection, cacheHit, at));
}
```

## [03]-[RESEARCH]

- [AP242_CODEC]: the ISO 10303 AP203/AP214/AP242 STEP solid-model reader/writer member spellings (entity-instance parse, B-rep advanced_brep extraction, NURBS surface read) confirm against the STEP codec surface — the three protocols ride one `step-iso10303` codec discriminated by the `StepProtocol` column, all routing geometry evaluation through the same Compute companion rail GeometryGym IFC uses because managed STEP solid evaluation has no in-process kernel; the `step-ap242-reader-pending` `CataloguePackage` marker names the unadmitted managed STEP reader and the row, codec, protocol, and frame columns are settled with the semantic-read body grounding at the cross-folder Python-companion alignment.
- [COMPANION_PROTOCOL]: the IfcOpenShell companion-daemon request/response protocol for the tessellation hop — the `IfcConvert`-to-GLB invocation shape, the deflection/tolerance argument mapping, and the GLB streaming-back contract — is owned by `libs/python/geometry` (`python:geometry/ifc-companion`) and orchestrated by `Rasm.Compute/Runtime/codecs#TWO_HOP_TESSELLATION`; the Bim `TessellationRequest` shape is settled and the companion wire detail rides Compute's existing companion rpc.

