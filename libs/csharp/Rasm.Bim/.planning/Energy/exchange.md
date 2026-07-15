# [BIM_ENERGY_EXCHANGE]

The building-energy-model exchange entry: one `EnergyExchange.Apply(EnergyOp)` raising HBJSON/DFJSON/OSM/gbXML/IDF documents onto the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` as graph content, lowering graph content back to the two managed authoring schemas, and translating between the OSM-centric formats — the semantic MODEL-EXCHANGE leg the `[ENERGY_MODEL_EXCHANGE]` package group stands for. The three arms are sibling owners this entry dispatches: the raise is `Energy/projector#ENERGY_PROJECTOR` `EnergyProjector : IElementProjection` (five decode arms, one Compute-readable landing shape), the lower is `Energy/derive#MODEL_DERIVE` `EnergyDerive` (graph → honeybee envelope + energy library, graph → dragonfly massing), and the translation is `Energy/derive#TRANSLATE_MATRIX` `EnergyTranslate` (the frozen OSM-centric `(source, target)` row table). Three admitted packages realize the folder: `HoneybeeSchema` (the HBJSON object graph — 296 model classes over `OpenAPIGenBaseModel`, the `Model.FromJson`/`ToJson` LBT-Newtonsoft codec, the `IConstruction`/`IMaterial` energy library and the abridged-reference model), `DragonflySchema` (the DFJSON `Model`→`Building`→`Story`→`Room2D` urban-massing layer composing the honeybee vocabulary by identifier, never re-declaring it), and `NREL.OpenStudio.macOS-arm64` (the SWIG OSM/IDF object store plus the `VersionTranslator`/`GbXMLReverseTranslator`/`EnergyPlusForwardTranslator`/`EnergyPlusReverseTranslator`/`GbXMLForwardTranslator` matrix). Energy SIMULATION — building a conditioned annual OSM from the graph, running EnergyPlus, reading the results `SqlFile` — is `csharp:Rasm.Compute/Analysis/energy`'s and never re-authored here: this folder moves MODELS, that page runs them, the two aligned by the seam graph and never coupled.

Identity is dual-keyed on the tessellation-bridge pattern: `EnergyDoc.SourceKey` and `EnergyArtifact.ContentKey` are ONE derivation — the kernel seed-zero `ContentHash.Of` over the seam `CanonicalWriter` `String(format.Key).Raw(bytes)` fold — so a lowered artifact re-admitted as a document carries the identical key and the reuse join holds across the raise/lower round trip. Every emitted document is a content-keyed `EnergyArtifact` (`{ContentKey:x32}:{form}` — the `Rasm.Persistence` object-plane/`ArtifactIndexRow` join key); the artifact bytes land on the Persistence object plane through the caller, never a Persistence reference minted here, and the `python:geometry` energy plane meets this folder ONLY at those content-keyed document bytes and the wire — the HBJSON→OSM whole-translation (`honeybee-openstudio`) is the python peer's leg, never a shared client.

Wire posture: HOST-LOCAL, foreign types boundary-confined. `HoneybeeSchema.*`/`DragonflySchema.*` DTOs and their LBT-Newtonsoft codec never escape the arm fences (`Model.FromJson`/`ToJson` are the ONLY spec-valid HBJSON/DFJSON codec — an STJ or stock-Newtonsoft pipeline over these types is the rejected form); every `OpenStudio.*` SWIG wrapper owns a native handle, is bracketed `using` per arm, never crosses a signature, and its `Optional<T>` lowers at the boundary. Faults route the existing `Model/faults#FAULT_BAND` `BimFault` arms (band 2600, `Expected`-derived, bare lift, zero new cases): a decode/validation reject or a null-parse is `ModelRejected` (`energy-decode`), an unmappable `FaceType`/surface class is `UnmappedClass` (`energy-face-miss`/`energy-class-miss`), an abridged identifier resolving nothing is `DanglingReference` (`energy-construction-absent`), an unserved form or translator-matrix miss is `CodecReject` (`energy-form-miss`/`energy-lower-unsupported`/`energy-translate-miss`), and the graph→OSM/gbXML/IDF egress request is `CapabilityMiss` (`energy-graph-egress-pending`) until its arm lands.

## [01]-[INDEX]

- [01]-[ENERGY_EXCHANGE]: `EnergyExchange.Apply` the ONE entry over the closed `EnergyOp` `[Union]` (`Raise`/`Lower`/`Translate`), the `EnergyDoc`/`EnergyArtifact` content-keyed carriers, the `EnergyScope` request vocabulary, and the `EnergyReceipt` typed evidence.

## [02]-[ENERGY_EXCHANGE]

- Owner: `EnergyExchange` the one exchange entry; `EnergyOp` the closed request `[Union]` (`Raise(EnergyDoc, ElementGraph, ProjectionContext)` / `Lower(ElementGraph, InterchangeFormat, EnergyScope, GeometrySource, Instant, Op)` / `Translate(EnergyDoc, InterchangeFormat, Instant, Op)`); `EnergyOutcome` the two-case result (`Raised` carrying graph+delta+the analytical footprint blobs, `Emitted` carrying the content-keyed artifact); `EnergyDoc` the foreign-document carrier with its PURE byte-content `SourceKey`; `EnergyArtifact` the content-keyed emitted document carrying the optional seam graph pedigree; `EnergyScope`/`EnergyLeg` the request/receipt vocabulary; `EnergyReceipt` the typed exchange evidence.
- Entry: `EnergyExchange.Apply(EnergyOp op)` → `Fin<EnergyOutcome>` — the generated total `Switch` routes `Raise` through a fresh `Energy/projector#ENERGY_PROJECTOR` `EnergyProjector` under `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `ProjectionAssembly.Assemble` with the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` constraint composed (the raise mints `Classification("ifc", code)`/`PredefinedType` values, so the legality vocabulary arms gate an out-of-roster class or token exactly as an IFC ingest is gated), `Lower` through the `Energy/derive#MODEL_DERIVE` `EnergyDerive` BIM-to-BEM fold, and `Translate` through the `Energy/derive#TRANSLATE_MATRIX` `EnergyTranslate` matrix; each typed `BimFault` case lifts BARE onto the `Fin<T>` rail (band 2600 IS the `Expected` `Code`, no `.ToError()` hop).
- Auto: identity is dual-keyed on the tessellation-bridge pattern — `EnergyDoc.SourceKey` and `EnergyArtifact.ContentKey` are ONE derivation (the kernel seed-zero `ContentHash.Of` over the seam `CanonicalWriter` `String(format.Key).Raw(bytes)` fold), so a lowered artifact re-admitted as a document carries the identical key and the reuse join holds across the raise/lower round trip; `EnergyArtifact.Graph` carries the `ContentAddress.OfGraph` SEMANTIC pedigree (`Some` exactly on graph-lowered artifacts) so the Persistence artifact index joins a derived energy model back to its source graph without parsing it; `ArtifactKey` resolves `$"{ContentKey:x32}:{format.Key}"` — the same `key:kind` address grammar the `Exchange/tessellation#TESSELLATION_BRIDGE` `:glb` artifacts use, one object-plane address space.
- Receipt: `EnergyReceipt` is the typed exchange evidence on the rail — the `EnergyLeg` direction, the form and (translate-only) target rows, the `Spaces`/`Surfaces`/`Openings`/`Constructions` fold counts, the `Warnings` tally (the lowered models' `Validate()` DataAnnotations results, OpenStudio translator `warnings()`, degraded-construction notes — the raise gates DataAnnotations inside `FromJson`, so its tally carries only degrade warnings), the content key, and the `Instant` stamp — counts, keys, and evidence only, never payload bytes.
- Packages: HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, GeometryGymIFC_Core (roster vocabulary via `IfcClass`), Rasm.Element, Rasm, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions
- Growth: a new energy-model form (an ISO 52016 XML, a FloorspaceJS floorplan) is one `Exchange/format#FORMAT_AXIS` `InterchangeFormat` row on the `energy-model` codec plus one `Energy/projector` arm row or one `Energy/derive` matrix row — never a new entry, never a per-form `HbjsonImporter`/`OsmExporter` family; a new lower target is one `EnergyDerive` arm row; a new scope modality (a storey filter, a zone filter) is one `EnergyScope` case; the graph→OSM/gbXML egress is the declared growth arm — it lands as ONE matrix column fed by the lowered honeybee leg the moment an in-process HBJSON→OSM translation is admitted, and until then the request faults `CapabilityMiss` loudly, never a silent partial.
- Boundary: the five energy rows (`hbjson`/`dfjson`/`osm`/`gbxml`/`idf`) live on the ONE `Exchange/format#FORMAT_AXIS` `InterchangeFormat` table under an `energy-model` `InterchangeCodec` row — a page-local format vocabulary beside the axis is the deleted form, and every capability column names this folder's realizing arm (raise = `CanImport`, the hbjson/dfjson lower = `CanExport`, the osm/gbxml/idf emit rides the `Translate` matrix over an admitted OSM-family source, so those rows carry `CanExport=false` against the graph). The artifact HANDOFF is a seam, not an import: `Rasm.Bim` mints no `Rasm.Persistence` reference — the `EnergyOutcome` returns the content-keyed artifact (and, on a raise, the analytical footprint blobs) to the caller, and the app-platform lands bytes WRITE-BLOB-FIRST on the object plane before the `ArtifactKey`/`Representations.FootPrint` references circulate, the same ordering law the tessellation bridge states. Alignment with the python energy plane is content-keyed artifacts + the wire ONLY — the py `honeybee-core` stack and this `HoneybeeSchema` binding meet at HBJSON document bytes sharing one `XxHash128` identity, never a shared client, never a cross-language import; alignment with `Rasm.Compute` is the seam graph ONLY — Compute simulates what the projector raises, the derive lowers what any projector landed, and a Compute project reference in either direction is the named strata defect. `SimulationParameter`, run periods, conditioning policy, and weather are SIMULATION context and never authored here — a lowered model is the semantic envelope + library, the run context is Compute's (locally) or the python recipe plane's (via the wire).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Text;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
// The exchange scope: whole model or a GlobalId-selected space subset; the case IS the modality
// (TessellationScope's law) — a filter-mode flag beside the values is the deleted form. The scope shapes
// the emitted BYTES, so the artifact's byte-content key already partitions per scope with no scope token.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnergyScope {
    private EnergyScope() { }

    public sealed record WholeModel : EnergyScope;
    public sealed record Spaces(Seq<string> GlobalIds) : EnergyScope;

    public static readonly EnergyScope Whole = new WholeModel();
}

[SmartEnum<string>]
public sealed partial class EnergyLeg {
    public static readonly EnergyLeg Raised     = new("raised");
    public static readonly EnergyLeg Lowered    = new("lowered");
    public static readonly EnergyLeg Translated = new("translated");
}

// --- [MODELS] -----------------------------------------------------------------------------
// A foreign energy-model document: raw bytes + its format-axis row. SourceKey is the PURE byte-content
// identity (kernel seed-zero ContentHash over the seam CanonicalWriter fold — format key + raw bytes, no
// scope/policy), the reuse join and the python-peer artifact key; Text is the UTF-8 read every managed
// codec and loadModelFromString consumes.
public sealed record EnergyDoc(InterchangeFormat Format, ReadOnlyMemory<byte> Bytes) {
    public UInt128 SourceKey =>
        ContentHash.Of(new CanonicalWriter(0.0).String(Format.Key).Raw(Bytes.Span).ToBytes().Span);

    public string Text => Encoding.UTF8.GetString(Bytes.Span);
}

// The content-keyed emitted document: ContentKey is the SAME derivation as EnergyDoc.SourceKey (one fold,
// so a lowered artifact re-admitted as a doc keys identically); Graph is the semantic pedigree — Some
// exactly on graph-lowered artifacts, the ArtifactIndexRow join back to the producing ElementGraph.
public sealed record EnergyArtifact(
    InterchangeFormat Format, ReadOnlyMemory<byte> Bytes, UInt128 ContentKey,
    Option<ContentAddress> Graph, Instant At) {

    public string ArtifactKey => $"{ContentKey:x32}:{Format.Key}";
    public long ByteCount => Bytes.Length;

    public static EnergyArtifact Of(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Option<ContentAddress> graph, Instant at) =>
        new(format, bytes, new EnergyDoc(format, bytes).SourceKey, graph, at);
}

// Typed exchange evidence: direction, form rows, fold counts, warning tally, content key, stamp —
// coordinates and counts, never payload bytes.
public sealed record EnergyReceipt(
    EnergyLeg Leg, InterchangeFormat Form, Option<InterchangeFormat> Target,
    int Spaces, int Surfaces, int Openings, int Constructions, int Warnings,
    UInt128 Key, Instant At);

[Union]
public abstract partial record EnergyOutcome {
    private EnergyOutcome() { }

    // The raised graph + event delta + the analytical footprint blobs the caller lands WRITE-BLOB-FIRST on the
    // object plane BEFORE applying the delta — the stamped Representations.FootPrint keys resolve through the
    // seam GeometrySource only after the blobs land, the tessellation write-blob-first law inverted to the caller.
    public sealed record Raised(ElementGraph Graph, GraphDelta Delta, Seq<(UInt128 Key, FootprintPolygon Ring)> Footprints, EnergyReceipt Receipt) : EnergyOutcome;
    public sealed record Emitted(EnergyArtifact Artifact, EnergyReceipt Receipt) : EnergyOutcome;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The ONE exchange request union: the case is the verb, the format row is the data — a RaiseHbjson/LowerOsm
// name family is the deleted form.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnergyOp {
    private EnergyOp() { }

    public sealed record Raise(EnergyDoc Source, ElementGraph Seed, ProjectionContext Ctx) : EnergyOp;
    public sealed record Lower(ElementGraph Graph, InterchangeFormat Target, EnergyScope Scope, GeometrySource Geometry, Instant At, Op Key) : EnergyOp;
    public sealed record Translate(EnergyDoc Source, InterchangeFormat Target, Instant At, Op Key) : EnergyOp;
}

public static class EnergyExchange {
    // Raise runs under the seam Assemble fold WITH IfcLegality composed: the projector mints roster-classified
    // nodes, so the vocabulary legality arms gate them exactly as an IFC ingest; Lower and Translate are
    // artifact emits. Every arm returns the one outcome union; the fold counts ride the projector/derive state.
    public static Fin<EnergyOutcome> Apply(EnergyOp op) => op.Switch(
        raise: static r => {
            var projector = new EnergyProjector(r.Source);
            return EnergyProjector.Serves(r.Source.Format)
                ? ProjectionAssembly.Assemble(
                        ProjectionSuite.Of(Seq<IElementProjection>(projector), Seq<IGraphConstraint>(new IfcLegality())),
                        r.Seed, r.Ctx)
                    .Map(result => (EnergyOutcome)new EnergyOutcome.Raised(
                        result.Graph, result.Delta, projector.Footprints, projector.Receipt(r.Ctx.At)))
                : Fin.Fail<EnergyOutcome>(new BimFault.CodecReject(r.Ctx.Key, $"energy-form-miss:{r.Source.Format.Key}"));
        },
        lower: static l => EnergyDerive.Lower(l.Graph, l.Target, l.Scope, l.Geometry, l.At, l.Key)
            .Map(static emitted => (EnergyOutcome)emitted),
        translate: static t => EnergyTranslate.Run(t.Source, t.Target, t.At, t.Key)
            .Map(static emitted => (EnergyOutcome)emitted));
}
```

## [03]-[RESEARCH]

- [ARTIFACT_IDENTITY]: the dual-key regime is one derivation — `EnergyDoc.SourceKey` and `EnergyArtifact.ContentKey` both fold `String(format.Key).Raw(bytes)` through the seam `CanonicalWriter` into the kernel seed-zero `ContentHash.Of`, so byte-identical documents key identically regardless of direction and the Persistence object-plane 412-noop dedups a re-emitted model; `EnergyArtifact.Graph` stamps `ContentAddress.OfGraph` on graph-lowered artifacts only, joining the artifact index to its source graph without a parse; the caller lands artifact bytes and raise footprint blobs WRITE-BLOB-FIRST before the `ArtifactKey`/`Representations.FootPrint` references circulate — the `Exchange/tessellation#TESSELLATION_BRIDGE` ordering law, with the raise inverting it to the caller because `Rasm.Bim` mints no Persistence reference.
- [PYTHON_PLANE]: the cross-language alignment is document bytes + one content identity, never a shared client — the py `honeybee-core`/`dragonfly-core` stack (RASM-PY-GEOMETRY `[V1_ENERGY_PLANE]` `energy/model`/`energy/district`) reads and writes the same HBJSON/DFJSON documents this folder exchanges, `honeybee-openstudio` OSM/IDF translation and the recipe execution (`RASM-PY-RUNTIME` `[V3_RECIPE_OWNER]`) are the python plane's legs, and an `EnergyArtifact` landed content-keyed on the object plane is reusable by either runtime through the one seed-zero `XxHash128` regime; the deliberate C#-side absence — no in-process HBJSON→OSM translation — is what keeps the two OpenStudio integrations (py `honeybee-openstudio`, C# SWIG) aligned-not-coupled.
- [FORMAT_ROWS]: the five energy rows land on the `Exchange/format#FORMAT_AXIS` per its own growth law — one `InterchangeCodec.EnergyModel` row (`energy-model`, managed, no companion) and five `InterchangeFormat` rows: `Hbjson` (`application/vnd.ladybug.hbjson+json`, `.hbjson`, import+export), `Dfjson` (`application/vnd.ladybug.dfjson+json`, `.dfjson`, import+export), `Osm` (`application/vnd.openstudio.osm`, `.osm`, import-only against the graph), `GbXml` (`application/vnd.gbxml+xml`, `.gbxml`, import-only), `Idf` (`application/vnd.energyplus.idf`, `.idf`, import-only) — all `TessellationRequiresCompanion=false`, `frame: BasisChange.Identity` (ladybug, OSM, and gbXML are Z-up right-handed), `StepProtocol.None`; every capability column names the realizing arm (`CanImport` = the `Energy/projector` arms, `CanExport` = the `Energy/derive` hbjson/dfjson arms; the osm/gbxml/idf EMIT capability rides the `Translate` matrix over an admitted OSM-family source, not the graph, so their `CanExport` stays false until the graph-egress arm lands).
