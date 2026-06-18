# [BIM_TESSELLATION_BRIDGE]

The IFC/AP242/native geometry tessellation bridge: one `TessellationRequest` shape crossing geometry evaluation to the `Rasm.Compute/interchange/codecs#TWO_HOP_TESSELLATION` companion rail (IfcOpenShell `IfcConvert` producing GLB) and re-importing the GLB through the `import-rail#IMPORT_RAIL` glTF path. The request is host-local in posture and rides Compute's existing companion transport, never a new transport and never the orchestration itself. The page composes the `format-axis#FORMAT_AXIS` `TessellationRequiresCompanion` gate, the `Rasm.Compute/interchange/codecs#CONTENT_ADDRESSING` content key, and the `Rasm.Compute/remote/channels#TRANSPORT_AXIS` transport as settled vocabulary. The page is HOST-LOCAL.

## [1]-[INDEX]

- [2]-[TESSELLATION_BRIDGE]: IFC/AP242/native geometry crosses to the Compute companion rail, never in-process.

## [2]-[TESSELLATION_BRIDGE]

- Owner: `TessellationRequest` — the Bim-side request shape crossing IFC/AP242/native geometry evaluation to the `Rasm.Compute/interchange/codecs#TWO_HOP_TESSELLATION` companion rail (IfcOpenShell `IfcConvert` producing GLB) and re-importing the GLB through the `import-rail#IMPORT_RAIL` glTF path; the request is host-local in posture and rides Compute's existing companion transport, never a new transport and never the orchestration itself.
- Entry: `TessellationRequest.Plan(InterchangeFormat source, ReadOnlyMemory<byte> ifcBytes, InterchangePolicy policy)` builds the request keyed on the IFC content and the deflection/tolerance policy; Compute issues the request over `Rasm.Compute/remote/channels#TRANSPORT_AXIS` and the GLB result re-enters through `BimIo.ImportGeometry(InterchangeFormat.Glb, ...)`.
- Auto: `Plan` reads `InterchangeFormat.TessellationRequiresCompanion` to gate the hop so a non-IFC format never crosses; the request carries the IFC bytes, the deflection and tolerance from `InterchangePolicy`, and the Compute content-key so a re-tessellation of the same model at the same deflection reuses the cached GLB by reference to the Persistence artifact index rather than re-crossing the companion.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing
- Growth: a new evaluation parameter is one column on `TessellationRequest` folded into the Compute content-key; never a new transport.
- Boundary: the companion bridge is the single IFC-to-geometry path because GeometryGym carries no tessellation kernel — a managed IFC BRep evaluator is the deleted form; the tessellation REQUEST orchestration (issuing `TessellationRequest` over the companion rpc, re-importing the GLB, and the content-key cache reuse) is owned at `Rasm.Compute/interchange/codecs#TWO_HOP_TESSELLATION` and Bim builds only the request shape and consumes the re-imported GLB through `import-rail#IMPORT_RAIL`; the companion is the IfcOpenShell PyPI package living in `libs/python/geometry` (`python:geometry/ifc-companion`), never a NuGet pin, reached only through Compute's existing companion rpc so this page mints no transport, no channel, and no second wire vocabulary; the IFC semantic graph (from the `import-rail#IMPORT_RAIL` in-process ingest) and the tessellated geometry (from this hop) are two projections of one content-keyed IFC artifact joined by the Compute content-key; the AP242 CAD-STEP bridge rides the same companion shape over `format-axis#FORMAT_AXIS`.

```csharp signature
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
            : Fin.Fail<TessellationRequest>(new BimFault.ModelRejected($"<tessellation-not-required:{source.Key}>"));

    public string ArtifactKey => $"{IfcContentKey:x32}:glb";
}
```

## [3]-[RESEARCH]

- [AP242_CODEC]: the ISO 10303 AP203/AP214/AP242 STEP solid-model reader/writer member spellings (entity-instance parse, B-rep advanced_brep extraction, NURBS surface read) confirm against the STEP codec surface — the three protocols ride one `step-iso10303` codec discriminated by the `StepProtocol` column, all routing geometry evaluation through the same Compute companion rail GeometryGym IFC uses because managed STEP solid evaluation has no in-process kernel; the `step-ap242-reader-pending` `CataloguePackage` marker names the unadmitted managed STEP reader and the row, codec, protocol, and frame columns are settled with the semantic-read body grounding at the cross-folder Python-companion alignment.
- [COMPANION_PROTOCOL]: the IfcOpenShell companion-daemon request/response protocol for the tessellation hop — the `IfcConvert`-to-GLB invocation shape, the deflection/tolerance argument mapping, and the GLB streaming-back contract — is owned by `libs/python/geometry` (`python:geometry/ifc-companion`) and orchestrated by `Rasm.Compute/interchange/codecs#TWO_HOP_TESSELLATION`; the Bim `TessellationRequest` shape is settled and the companion wire detail rides Compute's existing companion rpc.

