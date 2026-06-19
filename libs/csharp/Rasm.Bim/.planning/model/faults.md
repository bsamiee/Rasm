# [BIM_FAULTS]

The BIM fault rail: `BimFault` a closed `[Union]` band for the BIM-and-exchange failures the IFC/glTF/STEP/Speckle pages produce — `ModelRejected` (a foreign payload the codec admitted but the semantic projection rejects, or a native package exception captured at the import/export boundary), `UnmappedClass` (an IFC entity-type string the closed `IfcClass`/`Classification` vocabulary does not carry), `DanglingReference` (a GlobalId a spatial host, type binding, or decomposition relationship names but the model graph never declares), `CodecReject` (a write-only/read-only capability miss, a catalogue-pending codec, or a format the `Detect` row index does not resolve), and `CapabilityMiss` (an in-process evaluation the managed branch owns no kernel for — a managed IFC/STEP BRep tessellation or a managed Speckle NURBS evaluation the companion rail must serve). The band-ownership resolution: every Bim page already constructs a `BimFault.ModelRejected` carrying a stringly-typed `<discriminant:detail>` token; the token IS the case, so the five tokens (`element-class-miss`/`spatial-class-miss`, `classification-code-reject`/`interchange-format-miss`, `import-catalogue-pending`/`tessellation-not-required`, dangling spatial/type/decomposition reference, companion-required) lift onto typed arms recoverable by code, never by message matching.

Wire posture: HOST-LOCAL. `BimFault` rides the `Fin<T>` rail every Bim entrypoint returns — `BimModel.Project`, `ElementSet.Query` (total, no rail), `Classification.Classify`, `BimAssembly.Assemble`, `BimIo.ImportGeometry`/`ImportIfc`/`ImportSpeckle`/`ImportSpeckleSemantic`, `BimExport.Export`/`ExportIfc`, `TessellationRequest.Plan`, `InterchangeFormat.Detect`, `KhrExtension.Register`, the `Review/validation#IDS_FACETS` audit, the `Review/issues#BCF_ARCHIVE` codec, the `Semantics/properties#PROPERTY_SETS` round-trip, the `Semantics/georeference#GEO_REFERENCE` projection, and the `Exchange/wire#WIRE_PROJECTION` admission — and never sits between wire and rail. The folder carries no fault-band string-comparer accessor: `BimFault` dispatches through its generated total `Switch`, keys no `FrozenDictionary`, and a `[KeyMemberComparer]` on the fault is the deleted form.

## [1]-[INDEX]

- [1]-[FAULT_BAND]: owns the `BimFault` `[Union]` band-2600 (`ModelRejected`/`UnmappedClass`/`DanglingReference`/`CodecReject`/`CapabilityMiss`) with `Code`/`ToError`, the typed band every Bim `Fin.Fail` lowers through `.ToError()`, composing the kernel band `GeometryFault` for the shared degenerate-geometry failure a tessellated artifact carries.

## [2]-[FAULT_BAND]

- Owner: `BimFault` the closed `[Union]` fault band (band 2600) for BIM-and-exchange failures, mirroring the sibling `Rasm.Fabrication/Process/faults#FAULT_BAND` `FabricationFault` and `Rasm.Materials` `ProfileFault` band shapes — one case per failure carrying its detail, a `Code`/`Message` state-threaded `Switch`, and a `ToError()` that lowers the case into the `Fin<T>` failure channel.
- Cases: `BimFault` arms `ModelRejected` (a codec-admitted payload the semantic projection rejects or a captured native package exception, produced by `Exchange/import#IMPORT_RAIL`·`Exchange/export#EXPORT_RAIL`·`Exchange/format#FORMAT_AXIS` `KhrExtension.Register`) · `UnmappedClass` (an IFC entity-type string the closed `IfcClass`/`Classification` vocabulary does not carry, produced by `Model/elements#ELEMENT_MODEL` `Project`·`Semantics/classification#CLASSIFICATION_AXIS` `Classify`·`Model/zones#ZONE_GRAPH` `Project`·`Model/structural#ANALYSIS_MODEL` `MemberOf`) · `DanglingReference` (a GlobalId a spatial host, type binding, or `AssemblyRel` arm names but the graph never declares, produced by `Model/structure#ASSEMBLY_TREE`·`Review/diff#MODEL_DIFF`) · `CodecReject` (a capability miss, a catalogue-pending codec, or an unresolved `Detect` format, produced by `Exchange/import#IMPORT_RAIL`·`Exchange/export#EXPORT_RAIL`·`Exchange/format#FORMAT_AXIS` `Detect`) · `CapabilityMiss` (an in-process evaluation the managed branch owns no kernel for, produced by `Exchange/import#IMPORT_RAIL`·`Exchange/tessellation#TESSELLATION_BRIDGE` `Plan`) (5); the shared degenerate-geometry failure (an empty or non-finite tessellated vertex set re-imported from the companion GLB) routes the kernel `Rasm` `GeometryFault`, never re-cased here.
- Entry: the union cases are the fault constructors the `Fin<T>` rail carries; both `BimFault` and the kernel `GeometryFault` are union values lowered onto the rail through `<Fault>.<Case>(...).ToError()` — `BimFault.ModelRejected(detail).ToError()` and `GeometryFault.DegenerateInput(detail).ToError()` build the band-coded `Error` the `Fin<T>` failure channel carries — so one lowering idiom serves both families on the single `Fin<BimModel>`/`Fin<ImportedGeometry>`/`Fin<ExportArtifact>` rail every Bim entrypoint returns, and a model route and an exchange route compose without a second rail.
- Auto: each Bim owner routes the most specific fault, lowering it with `.ToError()` — `BimModel.Project` routes `BimFault.UnmappedClass(entityType).ToError()` on an `IfcClass.TryGet` miss, and `BimAssembly.Assemble` routes `BimFault.DanglingReference("spatial-root-miss").ToError()` when no `IfcProject` root resolves (the spatial-container vocabulary stays the raw entity-type string, distinct from the `IfcClass` element vocabulary, so the tree never faults on an unmapped spatial class); `Classification.Classify` routes `BimFault.UnmappedClass(code).ToError()` on a `ClassificationCode.TryCreate` reject; `BimIo.ImportGeometry`/`ImportIfc` capture the SharpGLTF `ModelException` and the GeometryGym parse fault into `BimFault.ModelRejected(error.Message).ToError()` at the `Boundary` funnel and route `BimFault.CodecReject(format.Key).ToError()` on a capability or catalogue-pending miss and `BimFault.CapabilityMiss(format.Key).ToError()` on an IFC/native geometry request; `BimExport.Export`/`ExportIfc` route `BimFault.CodecReject(format.Key).ToError()` on a write-only row and capture the GeometryGym serialization fault into `BimFault.ModelRejected`; `KhrExtension.Register` captures a registration fault into `BimFault.ModelRejected`; `InterchangeFormat.Detect` routes `BimFault.CodecReject(pathOrMediaTypeOrKey).ToError()` on an unresolved row; `TessellationRequest.Plan` routes `BimFault.CapabilityMiss(source.Key).ToError()` on a non-companion format.
- Receipt: `BimFault` is the typed fault evidence on the `Fin<T>` failure rail; no generic `IFault`/error-code abstraction, the union cases stay typed per BIM concern, and a recovery reads `error.HasCode(2603)` for the dangling-reference arm or `error.Is<...>` rather than matching the message substring.
- Packages: `Rasm` (the kernel `GeometryFault` band — composed for the shared degenerate-geometry failure a re-imported tessellated artifact carries), Thinktecture.Runtime.Extensions (`[Union]`), LanguageExt.Core (`Error`/`Fin`/`Try`), BCL inbox.
- Growth: a new BIM-and-exchange failure is one `BimFault` arm carrying the next ordinal in the 2600 band; a shared geometry failure is never re-cased here and routes the kernel `GeometryFault`; zero new band. A new sub-domain (`validation`, `coordination`, `properties`, `georeferencing`, `Exchange/wire`) routes its rejection onto one of the five existing arms — an IDS facet miss and a wire admission reject are `ModelRejected`, an unknown classification system or property template is `UnmappedClass`, a BCF viewpoint naming an absent element is `DanglingReference`, a bSDD service-unreachable degradation is `CodecReject`, a CRS the kernel transform algebra cannot reconcile is `CapabilityMiss` — never a sixth arm per sub-domain.
- Boundary: `BimFault` mints ONLY the five BIM-and-exchange cases and composes the kernel `GeometryFault` for the shared geometry failure — a parallel band that re-cases `DegenerateInput`, or a synthesized `GeometryFault.ModelRejected` case the kernel union never declares, is the deleted form; every Bim page that today constructs `new BimFault.ModelRejected($"<discriminant:detail>")` and passes it bare into `Fin.Fail<T>(...)` is the named seam defect this owner closes — the bare-string `<element-class-miss:...>`/`<spatial-class-miss:...>` tokens lift to `BimFault.UnmappedClass`, the `<spatial-root-miss>` token lifts to `BimFault.DanglingReference`, the `<import-unsupported:...>`/`<import-catalogue-pending:...>`/`<ifc-codec-miss:...>`/`<export-unsupported:...>`/`<interchange-format-miss:...>` tokens lift to `BimFault.CodecReject`, the `<import-needs-companion:...>`/`<tessellation-not-required:...>` tokens lift to `BimFault.CapabilityMiss`, and the captured-exception `error.Message` paths stay `BimFault.ModelRejected` — and every site lowers through `.ToError()` so a case passed bare into `Fin.Fail<T>(...)` without that lowering is the named seam defect; faults route through the `Fin`/`Validation`/`Eff` rails and exception-style control flow in domain logic is the named defect; the captured native fault enters through the `Try.lift(decode).Run().MapFail(static error => new BimFault.ModelRejected(error.Message).ToError())` funnel so the SharpGLTF `ModelException`, the GeometryGym parse fault, and the `Speckle.Sdk.SpeckleException` never cross a domain signature.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;

namespace Rasm.Bim;

// --- [ERRORS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BimFault {
    private BimFault() { }

    public sealed record ModelRejected(string Detail) : BimFault;
    public sealed record UnmappedClass(string Detail) : BimFault;
    public sealed record DanglingReference(string Detail) : BimFault;
    public sealed record CodecReject(string Detail) : BimFault;
    public sealed record CapabilityMiss(string Detail) : BimFault;

    public int Code =>
        Switch(
            modelRejected:     static _ => 2601,
            unmappedClass:     static _ => 2602,
            danglingReference: static _ => 2603,
            codecReject:       static _ => 2604,
            capabilityMiss:    static _ => 2605);

    public Error ToError() => Error.New(Code, Message);

    string Message =>
        Switch(
            modelRejected:     static f => $"bim:model-rejected:{f.Detail}",
            unmappedClass:     static f => $"bim:unmapped-class:{f.Detail}",
            danglingReference: static f => $"bim:dangling-reference:{f.Detail}",
            codecReject:       static f => $"bim:codec-reject:{f.Detail}",
            capabilityMiss:    static f => $"bim:capability-miss:{f.Detail}");
}
```

## [3]-[RESEARCH]

- [GEOMETRY_FAULT_COMPOSE]: the kernel `Rasm` `GeometryFault.DegenerateInput` member spelling and band the `BimFault` owner composes for the shared empty/non-finite tessellated-geometry failure confirms against the kernel `Rasm/faults#FAULT_BAND` owner at cross-folder alignment, so a re-imported companion GLB carrying a degenerate vertex set lowers the kernel `GeometryFault.DegenerateInput(...).ToError()` onto the same `Fin<ImportedGeometry>` rail rather than a sixth Bim arm; `BimFault` and `GeometryFault` are both `Expected`-derived union values whose band IS the `Expected` `Code`, so a bare typed case lifts directly with `.ToError()`.
