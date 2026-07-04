# [BIM_ENERGY_EXCHANGE]

The building-energy-model exchange owner: one `EnergyExchange.Apply(EnergyOp)` entry raising HBJSON/DFJSON/OSM/gbXML/IDF documents onto the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` as graph content, lowering graph content back to the two managed authoring schemas (honeybee HBJSON, dragonfly DFJSON), and translating between the OSM-centric formats through the OpenStudio translator matrix — the semantic MODEL-EXCHANGE leg the `[ENERGY_MODEL_EXCHANGE]` package group stands for. Three admitted packages realize it: `HoneybeeSchema` (the HBJSON object graph — 296 model classes over `OpenAPIGenBaseModel`, the `Model.FromJson`/`ToJson` LBT-Newtonsoft codec, the `IConstruction`/`IMaterial` energy library and the abridged-reference model), `DragonflySchema` (the DFJSON `Model`→`Building`→`Story`→`Room2D` urban-massing layer composing the honeybee vocabulary by identifier, never re-declaring it), and `NREL.OpenStudio.macOS-arm64` (the SWIG OSM/IDF object store plus the `VersionTranslator`/`GbXMLReverseTranslator`/`EnergyPlusForwardTranslator`/`EnergyPlusReverseTranslator`/`GbXMLForwardTranslator` matrix). Energy SIMULATION — building a conditioned annual OSM from the graph, running EnergyPlus, reading the results `SqlFile` — is `csharp:Rasm.Compute/Analysis/energy`'s and never re-authored here: this page moves MODELS, that page runs them, the two aligned by the seam graph and never coupled.

The raise is a second Bim `EnergyProjector : IElementProjection` beside the `Projection/semantic#SEMANTIC_PROJECTOR` (the `Exchange/reconstruct#RECONSTRUCTION` precedent): five decode arms converge on one projection landing the EXACT graph shape the Compute energy runner already reads — `IfcSpace`-classified `Object` nodes, bounding-surface Objects joined by `IfcRelSpaceBoundary`-named neutral `Generic` edges carrying the `BoundaryLevel` payload, analytical footprints content-keyed into `Representations.FootPrint`, and seam `MaterialComposition.LayerSet`/`MaterialPropertySet.Thermal` material evidence — so an HBJSON raised by this page is simulable by `Rasm.Compute` with zero adapter, the wire-name/Qto/Pset contract being the load-bearing alignment. The lower is the BIM-to-BEM derivation: `IfcSpace` nodes (landed by the `SemanticProjector` IFC ingest or any other projector) fold to honeybee `Room`/`Face`/`Aperture` envelopes with the layered seam composition lowered onto `EnergyMaterial`/`OpaqueConstructionAbridged` library entries under the abridged-reference law, or to dragonfly `Story`/`Room2D` massing plates. Every emitted document is a content-keyed `EnergyArtifact` (`{ContentKey:x32}:{form}` — the kernel seed-zero `ContentHash` fold, the `Rasm.Persistence` object-plane/`ArtifactIndexRow` join key); the artifact bytes land on the Persistence object plane through the caller, never a Persistence reference minted here, and the `python:geometry` energy plane meets this page ONLY at those content-keyed document bytes and the wire — the HBJSON→OSM whole-translation (`honeybee-openstudio`) is the python peer's leg, never a shared client.

Wire posture: HOST-LOCAL, foreign types boundary-confined. `HoneybeeSchema.*`/`DragonflySchema.*` DTOs and their LBT-Newtonsoft codec never escape the exchange fences (`Model.FromJson`/`ToJson` are the ONLY spec-valid HBJSON/DFJSON codec — an STJ or stock-Newtonsoft pipeline over these types is the rejected form); every `OpenStudio.*` SWIG wrapper owns a native handle, is bracketed `using` per arm, never crosses a signature, and its `Optional<T>` lowers at the boundary. Faults route the existing `Model/faults#FAULT_BAND` `BimFault` arms (band 2600, `Expected`-derived, bare lift, zero new cases): a decode/validation reject or a null-parse is `ModelRejected` (`energy-decode`), an unmappable `FaceType`/surface class is `UnmappedClass` (`energy-face-miss`/`energy-class-miss`), an abridged identifier resolving nothing is `DanglingReference` (`energy-construction-absent`), an unserved form or translator-matrix miss is `CodecReject` (`energy-form-miss`/`energy-lower-unsupported`/`energy-translate-miss`), and the graph→OSM/gbXML/IDF egress request is `CapabilityMiss` (`energy-graph-egress-pending`) until its arm lands.

## [01]-[INDEX]

- [01]-[ENERGY_EXCHANGE]: `EnergyExchange.Apply` the ONE entry over the closed `EnergyOp` `[Union]` (`Raise`/`Lower`/`Translate`), the `EnergyDoc`/`EnergyArtifact` content-keyed carriers, the `EnergyScope` request vocabulary, and the `EnergyReceipt` typed evidence.
- [02]-[ENERGY_PROJECTOR]: `EnergyProjector : IElementProjection` — five format arms (HBJSON/DFJSON managed decode, OSM/gbXML/IDF SWIG decode) onto one raise fold landing the Compute-readable seam shape, the `EnergyClassRows` one-table FaceType↔`IfcClass` correspondence, and the seam material/footprint minting.
- [03]-[MODEL_DERIVE]: the BIM-to-BEM lower fold (graph → honeybee envelope + energy library, graph → dragonfly massing) over the seam `GeometrySource` port, and the `EnergyTranslate` OSM-centric translator matrix (osm↔gbxml, osm↔idf, osm version-upgrade) as frozen row data.

## [02]-[ENERGY_EXCHANGE]

- Owner: `EnergyExchange` the one exchange entry; `EnergyOp` the closed request `[Union]` (`Raise(EnergyDoc, ElementGraph, ProjectionContext)` / `Lower(ElementGraph, InterchangeFormat, EnergyScope, GeometrySource, Instant, Op)` / `Translate(EnergyDoc, InterchangeFormat, Instant, Op)`); `EnergyOutcome` the two-case result (`Raised` carrying graph+delta+the analytical footprint blobs, `Emitted` carrying the content-keyed artifact); `EnergyDoc` the foreign-document carrier with its PURE byte-content `SourceKey`; `EnergyArtifact` the content-keyed emitted document carrying the optional seam graph pedigree; `EnergyScope`/`EnergyLeg` the request/receipt vocabulary; `EnergyReceipt` the typed exchange evidence.
- Entry: `EnergyExchange.Apply(EnergyOp op)` → `Fin<EnergyOutcome>` — the generated total `Switch` routes `Raise` through a fresh `EnergyProjector` under `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `ProjectionAssembly.Assemble` with the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` constraint composed (the raise mints `Classification("ifc", code)`/`PredefinedType` values, so the legality vocabulary arms gate an out-of-roster class or token exactly as an IFC ingest is gated), `Lower` through the `EnergyDerive` BIM-to-BEM fold, and `Translate` through the `EnergyTranslate` matrix; each typed `BimFault` case lifts BARE onto the `Fin<T>` rail (band 2600 IS the `Expected` `Code`, no `.ToError()` hop).
- Auto: identity is dual-keyed on the tessellation-bridge pattern — `EnergyDoc.SourceKey` and `EnergyArtifact.ContentKey` are ONE derivation (the kernel seed-zero `ContentHash.Of` over the seam `CanonicalWriter` `String(format.Key).Raw(bytes)` fold), so a lowered artifact re-admitted as a document carries the identical key and the reuse join holds across the raise/lower round trip; `EnergyArtifact.Graph` carries the `ContentAddress.OfGraph` SEMANTIC pedigree (`Some` exactly on graph-lowered artifacts) so the Persistence artifact index joins a derived energy model back to its source graph without parsing it; `ArtifactKey` resolves `$"{ContentKey:x32}:{format.Key}"` — the same `key:kind` address grammar the `Exchange/tessellation#TESSELLATION_BRIDGE` `:glb` artifacts use, one object-plane address space.
- Receipt: `EnergyReceipt` is the typed exchange evidence on the rail — the `EnergyLeg` direction, the form and (translate-only) target rows, the `Spaces`/`Surfaces`/`Openings`/`Constructions` fold counts, the `Warnings` tally (honeybee `Validate()` DataAnnotations results, OpenStudio translator `warnings()`, degraded-construction notes), the content key, and the `Instant` stamp — counts, keys, and evidence only, never payload bytes.
- Packages: HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, GeometryGymIFC_Core (roster vocabulary via `IfcClass`), Rasm.Element, Rasm, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions
- Growth: a new energy-model form (an ISO 52016 XML, a FloorspaceJS floorplan) is one `Exchange/format#FORMAT_AXIS` `InterchangeFormat` row on the `energy-model` codec plus one projector arm row or one matrix row — never a new entry, never a per-form `HbjsonImporter`/`OsmExporter` family; a new lower target is one `EnergyDerive` arm row; a new scope modality (a storey filter, a zone filter) is one `EnergyScope` case; the graph→OSM/gbXML egress is the declared growth arm — it lands as ONE matrix column fed by the lowered honeybee leg the moment an in-process HBJSON→OSM translation is admitted, and until then the request faults `CapabilityMiss` loudly, never a silent partial.
- Boundary: the five energy rows (`hbjson`/`dfjson`/`osm`/`gbxml`/`idf`) live on the ONE `Exchange/format#FORMAT_AXIS` `InterchangeFormat` table under an `energy-model` `InterchangeCodec` row — a page-local format vocabulary beside the axis is the deleted form, and every capability column names THIS page's realizing arm (raise = `CanImport`, the hbjson/dfjson lower = `CanExport`, the osm/gbxml/idf emit rides the `Translate` matrix over an admitted OSM-family source, so those rows carry `CanExport=false` against the graph). The artifact HANDOFF is a seam, not an import: `Rasm.Bim` mints no `Rasm.Persistence` reference — the `EnergyOutcome` returns the content-keyed artifact (and, on a raise, the analytical footprint blobs) to the caller, and the app-platform lands bytes WRITE-BLOB-FIRST on the object plane before the `ArtifactKey`/`Representations.FootPrint` references circulate, the same ordering law the tessellation bridge states. Alignment with the python energy plane is content-keyed artifacts + the wire ONLY — the py `honeybee-core` stack and this `HoneybeeSchema` binding meet at HBJSON document bytes sharing one `XxHash128` identity, never a shared client, never a cross-language import; alignment with `Rasm.Compute` is the seam graph ONLY — Compute simulates what this page raises, this page lowers what any projector landed, and a Compute project reference in either direction is the named strata defect. `SimulationParameter`, run periods, conditioning policy, and weather are SIMULATION context and never authored here — a lowered model is the semantic envelope + library, the run context is Compute's (locally) or the python recipe plane's (via the wire).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Linq;
using System.Text;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Element;
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
        raise: r => {
            var projector = new EnergyProjector(r.Source);
            return EnergyProjector.Serves(r.Source.Format)
                ? ProjectionAssembly.Assemble(
                        Seq<IElementProjection>(projector), Seq<IGraphConstraint>(new IfcLegality()), r.Seed, r.Ctx)
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

## [03]-[ENERGY_PROJECTOR]

- Owner: `EnergyProjector : IElementProjection` the energy-model raise (the second Bim projector beside `SemanticProjector`, the `ReconstructionProjector` precedent — the raw `EnergyDoc` captured internally, the seam contract carrying only `Node`/`Relationship`/`GraphDelta`); `EnergyClassRows` the ONE FaceType↔`IfcClass` correspondence table both directions derive from; the frozen `Arms` format→arm index the `Serves` capability predicate reads.
- Entry: `EnergyProjector.Project(ProjectionContext ctx)` → `Fin<GraphDelta>` — the frozen arm index dispatches the captured document's format row: `Hbjson` decodes through `HoneybeeSchema.Model.FromJson` inside the `Try.lift` funnel (the LBT-Newtonsoft parse throw and the DataAnnotations reject both land `BimFault.ModelRejected` `energy-decode`, the raw message preserved; a `"type"`-mismatch null-parse faults explicitly, never a null flowing on), `Dfjson` through `DragonflySchema.Model.FromJson` (same funnel — its parse VALIDATES on construction, throwing `ArgumentException` on a structurally-invalid document), and `Osm`/`GbXml`/`Idf` through the bracketed SWIG decode trio (`VersionTranslator.loadModelFromString` — the version-upgrading in-string OSM read; `GbXMLReverseTranslator.loadModel(Path)`/`EnergyPlusReverseTranslator.loadModel(Path)` — path-bound readers crossed via a bracketed temp path, the dotbim/usd-stage precedent) converging on ONE `RaiseOsm` fold.
- Auto: every arm lands the Compute-readable seam shape — a room/space mints an `IfcSpace`-classified rooted `Object` (`ExternalId` = the schema identifier, so a re-raise correlates), each bounding face a surface `Object` through the `EnergyClassRows` row (`Wall`→`IfcWall`, `Floor`→`IfcSlab`+`FLOOR`, `RoofCeiling`→`IfcSlab`+`ROOF`, `AirBoundary`→`IfcVirtualElement`), joined by the `IfcRelKind.SpaceBoundary`-named neutral `Generic` edge carrying `BoundaryLevel: "2nd"` (a honeybee/OSM face is a per-space bounded surface — the second-level shape `Rasm.Compute` `EnergyGraphReads.BoundingSurfacesOf` prefers) plus the `BoundaryCondition` text payload; the face ring content-keys into `Representations.FootPrint` through the seam footprint byte projection (recorded on the projector's `Footprints` side-channel for the caller's write-blob-first landing); apertures and doors mint `IfcWindow`/`IfcDoor` Objects joined to the space by the SAME boundary edge shape with a `Host` correlation attribute naming their face (an opening IS a space boundary in energy modeling; the typed `Void`/fill lowering demands the `IfcOpeningElement` intermediary no energy schema carries); an abridged construction resolves through the model-level canonical lists (the `[ABRIDGED_REFERENCE_MODEL]` law — `OpaqueConstructionAbridged.Materials` ids into `ModelEnergyProperties.Materials`), each `EnergyMaterial` minting a content-keyed seam `Node.Material` carrying `MaterialPropertySet.OfThermal(conductivity, specificHeat, conductivity/thickness, 1.0, key)` (the per-layer U derived, the vapour factor floored at the vapour-open 1.0 the schema does not carry) and the surface an `Associate` edge over `MaterialComposition.OfLayerSet`; the OSM arm reads `Model.getSpaces()`/`Space.surfaces`/`PlanarSurface.vertices()`/`Surface.surfaceType()`/`outsideBoundaryCondition()` and resolves typed layer materials through `Construction.layers()` + `Model.getStandardOpaqueMaterial(handle)` (the SWIG vector element is statically `Material` — the handle re-read is the ONLY typed downcast the binding owns).
- Receipt: the projector tallies `Spaces`/`Surfaces`/`Openings`/`Constructions` and the degraded-material warnings as owner-local fold state projected once into the `EnergyReceipt`; the honeybee `Validate()` results fold into the warning tally as evidence, never an exception.
- Packages: HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, Rasm.Element, Rasm, LanguageExt.Core, Thinktecture.Runtime.Extensions
- Growth: a new face/class correspondence is one `EnergyClassRows` row (both directions derive); a new energy form is one `Arms` row; a dragonfly parameter (window ratios, shading, skylights) deepens the massing arm as row folds over the `Room2D` `AnyOf` unions; a NoMass/Vegetation material arm is one typed-layer row the moment the seam carries an R-value-only thermal case.
- Boundary: the projector decodes INSIDE `Project`, so no LBT DTO or SWIG handle outlives the projection (every OpenStudio wrapper — the model, translators, `Optional*`, `*Vector`, per-element handles — is `using`-bracketed; the SWIG index-loop with per-element disposal is the named marshaling exemption); the `EnergyMaterial` density has NO seam thermal column and a fabricated `OfMechanical` stiffness is the rejected form — the density is a recorded warning until a seam thermal-density column lands, and the OSM rebuild's documented 1000 kg/m³ fallback carries the consequence; an OSM layer whose typed re-read misses (a massless/airgap/fenestration layer) degrades to the assembly `ConstructionBase.uFactor()` landed as `Pset_EnergyModel` bag evidence plus a warning, never a fabricated layer set; the structural-graph legality (endpoints, ids) is the seam's `ElementFault`, the IFC-semantic legality is the composed `IfcLegality` → `BimFault.ModelRejected`, and this projector re-checks neither; the rooted `NodeId` is LOCAL per raise (Guid-v7), the schema identifier riding `ExternalId` for correlation — the wire law verbatim.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.IO;
using System.Linq;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Element;
using static LanguageExt.Prelude;
using Df = DragonflySchema;      // boundary-only aliases: the DTO namespaces never escape this file
using Hb = HoneybeeSchema;
using Os = OpenStudio;

namespace Rasm.Bim;

// --- [TABLES] -----------------------------------------------------------------------------
// ONE primary FaceType<->IfcClass correspondence; the raise map and the lower map are two frozen indexes
// DERIVED from it plus the lower-side overlay (IFC subtypes that fold onto a face kind), so the two
// directions can never drift. Predefined tokens gate at Emit through AdmitPredefined, never here.
static class EnergyClassRows {
    internal static readonly (Hb.FaceType Face, IfcClass Class, string Predefined)[] Rows = [
        (Hb.FaceType.Wall,        IfcClass.Wall,           ""),
        (Hb.FaceType.Floor,       IfcClass.Slab,           "FLOOR"),
        (Hb.FaceType.RoofCeiling, IfcClass.Slab,           "ROOF"),
        (Hb.FaceType.AirBoundary, IfcClass.VirtualElement, ""),
    ];

    // Lower-side widening: IFC classes with no primary row still derive a face kind (one row each).
    internal static readonly (IfcClass Class, Hb.FaceType Face)[] LowerOverlay = [
        (IfcClass.WallStandardCase, Hb.FaceType.Wall),
        (IfcClass.CurtainWall,      Hb.FaceType.Wall),
        (IfcClass.Roof,             Hb.FaceType.RoofCeiling),
    ];

    internal static readonly FrozenDictionary<Hb.FaceType, (IfcClass Class, string Predefined)> ToClass =
        Rows.ToFrozenDictionary(static r => r.Face, static r => (r.Class, r.Predefined));

    internal static readonly FrozenDictionary<(string Code, PredefinedType Token), Hb.FaceType> ToFace =
        Rows.Select(static r => ((r.Class.Key, PredefinedType.Create(r.Predefined)), r.Face))
            .Concat(LowerOverlay.Select(static r => ((r.Class.Key, PredefinedType.Create("")), r.Face)))
            .ToFrozenDictionary(static p => p.Item1, static p => p.Item2);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// The energy-model PRIMARY projector: the raw document captured internally, five arms converging on the
// seam shape the Compute energy runner reads. Footprints is the write-blob-first side-channel; the counters
// are owner-local fold state the receipt projects once.
public sealed class EnergyProjector(EnergyDoc doc) : IElementProjection {
    readonly List<(UInt128 Key, FootprintPolygon Ring)> footprints = [];
    int spaces, surfaces, openings, constructions, warnings;

    public Seq<(UInt128 Key, FootprintPolygon Ring)> Footprints => toSeq(footprints);

    public EnergyReceipt Receipt(Instant at) =>
        new(EnergyLeg.Raised, doc.Format, None, spaces, surfaces, openings, constructions, warnings, doc.SourceKey, at);

    static readonly FrozenDictionary<InterchangeFormat, Func<EnergyProjector, ProjectionContext, Fin<GraphDelta>>> Arms =
        new KeyValuePair<InterchangeFormat, Func<EnergyProjector, ProjectionContext, Fin<GraphDelta>>>[] {
            new(InterchangeFormat.Hbjson, static (p, ctx) => p.Honeybee(ctx)),
            new(InterchangeFormat.Dfjson, static (p, ctx) => p.Dragonfly(ctx)),
            new(InterchangeFormat.Osm,    static (p, ctx) => p.OsmFamily(ctx)),
            new(InterchangeFormat.GbXml,  static (p, ctx) => p.OsmFamily(ctx)),
            new(InterchangeFormat.Idf,    static (p, ctx) => p.OsmFamily(ctx)),
        }.ToFrozenDictionary();

    internal static bool Serves(InterchangeFormat format) => Arms.ContainsKey(format);

    public Fin<GraphDelta> Project(ProjectionContext ctx) =>
        Arms.TryGetValue(doc.Format, out var arm)
            ? arm(this, ctx)
            : Fin.Fail<GraphDelta>(new BimFault.CodecReject(ctx.Key, $"energy-form-miss:{doc.Format.Key}"));

    // --- [HONEYBEE_ARM]
    // FromJson parse + a null "type"-mismatch both fault at admission; Validate() results fold into the
    // warning tally as evidence (the ValidationReport law) rather than gating a structurally-parseable model.
    Fin<GraphDelta> Honeybee(ProjectionContext ctx) =>
        Try.lift(() => Hb.Model.FromJson(doc.Text)).Run()
            .MapFail(error => new BimFault.ModelRejected(ctx.Key, $"energy-decode:{error.Message}"))
            .Bind(model => model is null
                ? Fin.Fail<GraphDelta>(new BimFault.ModelRejected(ctx.Key, "energy-decode:<type-mismatch>"))
                : Tally(model).RaiseRooms(model, toSeq(model.Rooms ?? []), ctx));

    EnergyProjector Tally(Hb.Model model) { warnings += model.Validate().Count(); return this; }

    Fin<GraphDelta> RaiseRooms(Hb.Model model, Seq<Hb.Room> rooms, ProjectionContext ctx) =>
        rooms.Fold(
            Fin.Succ(GraphDelta.Empty.Reheader(ctx.Header)),
            (acc, room) => acc.Bind(delta => RaiseRoom(model, room, ctx).Map(delta.Merge)));

    Fin<GraphDelta> RaiseRoom(Hb.Model model, Hb.Room room, ProjectionContext ctx) {
        NodeId spaceId = NodeId.Rooted();
        spaces++;
        return toSeq(room.Faces ?? []).Fold(
            Fin.Succ(GraphDelta.Empty.Put(Element(spaceId, IfcClass.Space, "", room.Identifier, ctx))),
            (acc, face) => acc.Bind(delta => RaiseFace(model, spaceId, face, ctx).Map(delta.Merge)));
    }

    // A face mints the surface Object + FootPrint representation key + the SpaceBoundary Generic edge the
    // Compute reads consume; apertures/doors mint filled-opening Objects; the construction associates the
    // seam layer composition. BoundaryLevel is "2nd" — a honeybee face IS the per-space bounded surface.
    Fin<GraphDelta> RaiseFace(Hb.Model model, NodeId spaceId, Hb.Face face, ProjectionContext ctx) {
        if (!EnergyClassRows.ToClass.TryGetValue(face.FaceType, out var row)) {
            return Fin.Fail<GraphDelta>(new BimFault.UnmappedClass(ctx.Key, $"energy-face-miss:{face.FaceType}"));
        }
        NodeId surfaceId = NodeId.Rooted();
        surfaces++;
        UInt128 footprint = Footprint(Ring(face.Geometry.Boundary), ctx.Header.Tolerance);
        var delta = GraphDelta.Empty
            .Put(Element(surfaceId, row.Class, row.Predefined, face.Identifier, ctx, footprint))
            .Link(Boundary(spaceId, surfaceId,
                face.BoundaryCondition.Obj is Hb.OpenAPIGenBaseModel bc ? bc.Type : "Outdoors"));   // the schema Type discriminator, never a downcast chain
        delta = toSeq(face.Apertures ?? []).Fold(delta, (d, aperture) =>
            Opening(d, spaceId, face.Identifier, IfcClass.Window, aperture.Identifier, Ring(aperture.Geometry.Boundary), ctx));
        delta = toSeq(face.Doors ?? []).Fold(delta, (d, door) =>
            Opening(d, spaceId, face.Identifier, IfcClass.Door, door.Identifier, Ring(door.Geometry.Boundary), ctx));
        return Composition(model, face.Properties?.Energy?.Construction, ctx).Map(material => material.Match(
            Some: set => { constructions++; return set.Layers.Fold(delta.Put(set.LayerSetNode), static (d, n) => d.Put(n))
                .Link(new Relationship.Associate(surfaceId, set.LayerSetNode.Id, new MaterialUsage.None())); },
            None: () => delta));
    }

    // The abridged-reference resolve: construction id -> OpaqueConstructionAbridged -> material ids ->
    // per-material Single nodes + ONE LayerSet Material node the surface associates (the seam composition
    // shape graph.CompositionOf resolves). A dangling id faults; a NoMass/Vegetation layer degrades to a warning.
    Fin<Option<(Node.Material LayerSetNode, Seq<Node.Material> Layers)>> Composition(Hb.Model model, string? construction, ProjectionContext ctx) =>
        Optional(construction).Match(
            None: () => Fin.Succ(Option<(Node.Material, Seq<Node.Material>)>.None),
            Some: id => toSeq(model.Properties.Energy?.Constructions ?? [])
                .Choose(static any => any.Obj is Hb.OpaqueConstructionAbridged oc ? Some(oc) : None)
                .Find(oc => oc.Identifier == id)
                .ToFin(new BimFault.DanglingReference(ctx.Key, $"energy-construction-absent:{id}"))
                .Bind(oc => toSeq(oc.Materials)
                    .Traverse(materialId => SeamMaterial(model, materialId, ctx)).As().ToFin()
                    .Bind(rows => rows.Somes().ToSeq() is { IsEmpty: false } typed
                        ? MaterialComposition.OfLayerSet(typed.Map(static r => r.Layer), ctx.Key)
                            .Map(set => Some((Mint(id, set, Seq<MaterialPropertySet>(), ctx.Header.Tolerance), typed.Map(static r => r.Node))))
                        : Fin.Succ(Option<(Node.Material, Seq<Node.Material>)>.None))));

    Validation<Error, Option<(Node.Material Node, MaterialLayer Layer)>> SeamMaterial(Hb.Model model, string materialId, ProjectionContext ctx) =>
        toSeq(model.Properties.Energy?.Materials ?? [])
            .Choose(static any => any.Obj is Hb.EnergyMaterial m ? Some(m) : None)
            .Find(m => m.Identifier == materialId)
            .Match(
                None: () => { warnings++; return Success<Error, Option<(Node.Material, MaterialLayer)>>(None); },
                Some: m => MaterialPropertySet
                    .OfThermal(m.Conductivity, m.SpecificHeat, m.Conductivity / m.Thickness, 1.0, ctx.Key)
                    .ToValidation()
                    .Map(thermal => Some((
                        Mint(m.Identifier, MaterialComposition.OfSingle(MaterialId.Create(m.Identifier)), Seq(thermal), ctx.Header.Tolerance),
                        new MaterialLayer(MaterialId.Create(m.Identifier), MeasureValue.OfSi(Dimension.LengthDim, m.Thickness), m.Identifier)))));

    // Content-keyed Material mint: probe id overwritten by the canonical-bytes content hash so identical
    // materials and layer sets dedup across raises (the composition-page mint law).
    static Node.Material Mint(string identifier, MaterialComposition composition, Seq<MaterialPropertySet> properties, double tolerance) {
        Node.Material probe = new(NodeId.Content([]), MaterialId.Create(identifier), composition, properties);
        return new(NodeId.Content(probe.ToCanonicalBytes(tolerance).Span), MaterialId.Create(identifier), composition, properties);
    }

    // --- [DRAGONFLY_ARM]
    // The massing raise: Building/Story/Room2D lower onto an IfcBuilding/IfcBuildingStorey/IfcSpace Compose
    // tree with the floor plate content-keyed as the space FootPrint; Building.Room3ds (full honeybee rooms)
    // route through the SAME honeybee room fold — dragonfly composes the honeybee vocabulary, never re-mints it.
    Fin<GraphDelta> Dragonfly(ProjectionContext ctx) =>
        Try.lift(() => Df.Model.FromJson(doc.Text)).Run()
            .MapFail(error => new BimFault.ModelRejected(ctx.Key, $"energy-decode:{error.Message}"))
            .Bind(model => model is null
                ? Fin.Fail<GraphDelta>(new BimFault.ModelRejected(ctx.Key, "energy-decode:<type-mismatch>"))
                : toSeq(model.Buildings ?? []).Fold(
                    Fin.Succ(GraphDelta.Empty.Reheader(ctx.Header)),
                    (acc, building) => acc.Bind(delta => RaiseBuilding(building, ctx).Map(delta.Merge))));

    Fin<GraphDelta> RaiseBuilding(Df.Building building, ProjectionContext ctx) {
        NodeId buildingId = NodeId.Rooted();
        var seed = GraphDelta.Empty.Put(Element(buildingId, IfcClass.Building, "", building.Identifier, ctx));
        return toSeq(building.UniqueStories ?? []).Fold(Fin.Succ(seed), (acc, story) => acc.Map(delta => {
            NodeId storeyId = NodeId.Rooted();
            var d = delta.Put(Element(storeyId, IfcClass.BuildingStorey, "", story.Identifier, ctx))
                .Link(new Relationship.Compose(buildingId, storeyId, ComposeKind.Aggregate));
            return toSeq(story.Room2ds ?? []).Fold(d, (dd, room) => {
                NodeId spaceId = NodeId.Rooted();
                spaces++;
                UInt128 plate = Footprint(PlateRing(room), ctx.Header.Tolerance);
                return dd.Put(Element(spaceId, IfcClass.Space, "", room.Identifier, ctx, plate))
                    .Link(new Relationship.Compose(storeyId, spaceId, ComposeKind.Contain));
            });
        }));
    }

    static FootprintPolygon PlateRing(Df.Room2D room) =>
        new(toSeq(room.FloorBoundary).Map(p => new Vector3(p[0], p[1], room.FloorHeight)));

    // --- [OSM_ARM]
    // Three decode arms, one raise fold. loadModelFromString upgrades any older .osm in-string; the gbXML/IDF
    // readers are Path-bound, crossed via a bracketed temp path (Exemption: SWIG + filesystem boundary).
    Fin<GraphDelta> OsmFamily(ProjectionContext ctx) =>
        Decode(ctx).Bind(model => { using (model) { return RaiseOsm(model, ctx); } });

    Fin<Os.Model> Decode(ProjectionContext ctx) {
        try {
            if (doc.Format == InterchangeFormat.Osm) {
                using Os.VersionTranslator vt = new();
                using Os.OptionalModel osm = vt.loadModelFromString(doc.Text);
                return Lowered(osm, ctx, "osm");
            }
            string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try {
                File.WriteAllBytes(temp, doc.Bytes.ToArray());
                Os.Path path = Os.OpenStudioUtilitiesCore.toPath(temp);
                if (doc.Format == InterchangeFormat.GbXml) {
                    using Os.GbXMLReverseTranslator gb = new();
                    using Os.OptionalModel fromGb = gb.loadModel(path);
                    return Lowered(fromGb, ctx, "gbxml");
                }
                using Os.EnergyPlusReverseTranslator ep = new();
                using Os.OptionalModel fromIdf = ep.loadModel(path);
                return Lowered(fromIdf, ctx, "idf");
            }
            finally { File.Delete(temp); }
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<Os.Model>(new BimFault.ModelRejected(ctx.Key, $"energy-decode:{ex.Message}"));
        }
    }

    static Fin<Os.Model> Lowered(Os.OptionalModel optional, ProjectionContext ctx, string arm) =>
        optional.is_initialized()
            ? Fin.Succ(optional.get())
            : Fin.Fail<Os.Model>(new BimFault.ModelRejected(ctx.Key, $"energy-decode:<{arm}-unreadable>"));

    // The OSM raise: spaces/surfaces/boundary edges land the same seam shape as the honeybee arm; a typed
    // layer set resolves via the handle re-read (the only SWIG downcast), else the assembly uFactor lands as
    // Pset_EnergyModel bag evidence + a warning. Index-loop + per-element using = the SWIG marshaling exemption.
    Fin<GraphDelta> RaiseOsm(Os.Model model, ProjectionContext ctx) {
        var delta = GraphDelta.Empty.Reheader(ctx.Header);
        using Os.SpaceVector osSpaces = model.getSpaces();
        for (int i = 0; i < osSpaces.Count; i++) {
            Os.Space osSpace = osSpaces[i];
            NodeId spaceId = NodeId.Rooted();
            spaces++;
            delta = delta.Put(Element(spaceId, IfcClass.Space, "", osSpace.nameString(), ctx));
            using Os.SurfaceVector surfs = osSpace.surfaces;
            for (int j = 0; j < surfs.Count; j++) {
                Os.Surface surf = surfs[j];
                var face = toSeq(EnergyClassRows.Rows).Find(r => r.Face.ToString() == surf.surfaceType());
                if (face.IsNone) {
                    return Fin.Fail<GraphDelta>(new BimFault.UnmappedClass(ctx.Key, $"energy-class-miss:{surf.surfaceType()}"));
                }
                var row = face.ValueUnsafe();
                NodeId surfaceId = NodeId.Rooted();
                surfaces++;
                using Os.Point3dVector vertices = surf.vertices();
                UInt128 footprint = Footprint(OsmRing(vertices), ctx.Header.Tolerance);
                delta = delta
                    .Put(Element(surfaceId, row.Class, row.Predefined, surf.nameString(), ctx, footprint))
                    .Link(Boundary(spaceId, surfaceId, surf.outsideBoundaryCondition()));
                delta = OsmComposition(model, surf, surfaceId, delta, ctx);
            }
        }
        return Fin.Succ(delta);
    }

    static FootprintPolygon OsmRing(Os.Point3dVector vertices) {
        var ring = Seq<Vector3>();
        for (int i = 0; i < vertices.Count; i++) {
            using Os.Point3d p = vertices[i];
            ring = ring.Add(new Vector3(p.x(), p.y(), p.z()));
        }
        return new FootprintPolygon(ring);
    }

    // --- [SHARED_MINTS]
    static Node.Object Element(NodeId id, IfcClass @class, string predefined, string identifier, ProjectionContext ctx, UInt128 footprint = default) =>
        new(Id: id, Kind: ObjectKind.Occurrence,
            ExternalId: Optional(identifier).Filter(static s => s.Length > 0),
            Classification: Classification.Create("ifc", @class.Key, "", None, None, None),
            PredefinedType: PredefinedType.Create(predefined),
            Name: identifier, Tag: "",
            Representations: footprint == default
                ? RepresentationContentHash.Empty
                : RepresentationContentHash.Empty.With("FootPrint", footprint),
            History: None, Span: SchemaSpan.From(ctx.Header.Schema));

    static Relationship Boundary(NodeId space, NodeId surface, string condition) =>
        new Relationship.Generic(IfcRelKind.SpaceBoundary.Key, space, surface, Map(
            (PropertyName.Create("BoundaryLevel"),     (PropertyValue)new PropertyValue.Text("2nd")),
            (PropertyName.Create("BoundaryCondition"), (PropertyValue)new PropertyValue.Text(condition))));

    // An aperture/door IS a space boundary in energy modeling (the IfcRelSpaceBoundary related element may be a
    // window/door), so the opening joins the SPACE by the same edge shape with a Host correlation attribute — the
    // typed Void/fill lowering demands an IfcOpeningElement intermediary no energy schema carries, the rejected form.
    GraphDelta Opening(GraphDelta delta, NodeId space, string hostIdentifier, IfcClass @class, string identifier, FootprintPolygon ring, ProjectionContext ctx) {
        NodeId openingId = NodeId.Rooted();
        openings++;
        return delta
            .Put(Element(openingId, @class, "", identifier, ctx, Footprint(ring, ctx.Header.Tolerance)))
            .Link(new Relationship.Generic(IfcRelKind.SpaceBoundary.Key, space, openingId, Map(
                (PropertyName.Create("BoundaryLevel"), (PropertyValue)new PropertyValue.Text("2nd")),
                (PropertyName.Create("Host"),          (PropertyValue)new PropertyValue.Text(hostIdentifier)))));
    }

    static FootprintPolygon Ring(List<List<double>> boundary) =>
        new(toSeq(boundary).Map(static p => new Vector3(p[0], p[1], p.Count > 2 ? p[2] : 0.0)));

    // The footprint blob key rides the seam analytical byte projection (the ONE CanonicalWriter layout the
    // SemanticProjector [M2] mint owns — a page-local byte order is the cross-projector divergence defect);
    // the ring is recorded for the caller's write-blob-first landing.
    UInt128 Footprint(FootprintPolygon ring, double tolerance) {
        var writer = new CanonicalWriter(tolerance);
        foreach (Vector3 p in ring.Ring) { writer = writer.Double(p.X).Double(p.Y).Double(p.Z); }
        UInt128 key = ContentHash.Of(writer.ToBytes().Span);
        footprints.Add((key, ring));
        return key;
    }

    GraphDelta OsmComposition(Os.Model model, Os.Surface surf, NodeId surfaceId, GraphDelta delta, ProjectionContext ctx) {
        using Os.OptionalConstructionBase cb = surf.construction();
        if (!cb.is_initialized()) { return delta; }
        using Os.ConstructionBase constructionBase = cb.get();
        using Os.OptionalConstruction layered = model.getConstruction(constructionBase.handle());
        if (!layered.is_initialized()) { warnings++; return delta; }
        using Os.Construction construction = layered.get();
        using Os.MaterialVector layers = construction.layers();
        var rows = Seq<(Node.Material Node, MaterialLayer Layer)>();
        for (int i = 0; i < layers.Count; i++) {
            using Os.OptionalStandardOpaqueMaterial typed = model.getStandardOpaqueMaterial(layers[i].handle());
            if (!typed.is_initialized()) { warnings++; return UFactorEvidence(delta, surfaceId, constructionBase, ctx); }
            using Os.StandardOpaqueMaterial m = typed.get();
            var row = MaterialPropertySet
                .OfThermal(m.conductivity(), m.specificHeat(), m.conductivity() / m.thickness(), 1.0, ctx.Key)
                .Map(thermal => (
                    Mint(m.nameString(), MaterialComposition.OfSingle(MaterialId.Create(m.nameString())), Seq(thermal), ctx.Header.Tolerance),
                    new MaterialLayer(MaterialId.Create(m.nameString()), MeasureValue.OfSi(Dimension.LengthDim, m.thickness()), m.nameString())));
            if (row.IsFail) { warnings++; return UFactorEvidence(delta, surfaceId, constructionBase, ctx); }
            rows = rows.Add(row.ThrowIfFail());
        }
        return MaterialComposition.OfLayerSet(rows.Map(static r => r.Layer), ctx.Key).Match(
            Succ: set => {
                constructions++;
                Node.Material layerSet = Mint(constructionBase.nameString(), set, Seq<MaterialPropertySet>(), ctx.Header.Tolerance);
                return rows.Fold(delta.Put(layerSet), static (d, r) => d.Put(r.Node))
                    .Link(new Relationship.Associate(surfaceId, layerSet.Id, new MaterialUsage.None()));
            },
            Fail: _ => { warnings++; return UFactorEvidence(delta, surfaceId, constructionBase, ctx); });
    }

    // The degrade row: no typed layer read -> the assembly uFactor lands as bag evidence the lower/review
    // reads, never a fabricated layer set (PropertySource.Derived — computed evidence, not authored data).
    GraphDelta UFactorEvidence(GraphDelta delta, NodeId surfaceId, Os.ConstructionBase construction, ProjectionContext ctx) {
        using Os.OptionalDouble u = construction.uFactor();
        if (!u.is_initialized()) { return delta; }
        return MeasureValue.Of(u.get(), UnitsNet.Units.HeatTransferCoefficientUnit.WattPerSquareMeterKelvin, ctx.Key).Match(
            Fail: _ => { warnings++; return delta; },
            Succ: uValue => {
                PropertyBag bag = new("Pset_EnergyModel", Map(
                    (PropertyName.Create("UFactor"),          (PropertyValue)new PropertyValue.Measure(uValue)),
                    (PropertyName.Create("ConstructionName"), (PropertyValue)new PropertyValue.Text(construction.nameString()))),
                    InheritanceMode.OccurrenceWins, PropertySource.Derived);
                Node.PropertySet probe = new(NodeId.Content([]), bag);
                Node.PropertySet node = new(NodeId.Content(probe.ToCanonicalBytes(ctx.Header.Tolerance).Span), bag);
                return delta.Put(node).Link(new Relationship.Assign(surfaceId, node.Id, AssignKind.PropertyDefinition));
            });
    }
}
```

## [04]-[MODEL_DERIVE]

- Owner: `EnergyDerive` the BIM-to-BEM lower fold (graph → honeybee HBJSON envelope + energy library, graph → dragonfly DFJSON massing); `EnergyTranslate` the OSM-centric translator matrix as one frozen `(source, target)` row table over the OpenStudio translators.
- Entry: `EnergyDerive.Lower(ElementGraph graph, InterchangeFormat target, EnergyScope scope, GeometrySource geometry, Instant at, Op key)` → `Fin<EnergyOutcome.Emitted>` — the `hbjson` arm selects the `IfcSpace`-classified `Object` nodes under the scope, folds each space's `IfcRelSpaceBoundary` `Generic`-edged bounding surfaces into honeybee `Face`s (`Face3D` boundary from the seam `GeometrySource`-resolved `FootprintPolygon` — the same one-hop content-key resolve the Compute build takes; `FaceType` from the derived `EnergyClassRows.ToFace` index; the `Host`-attributed opening boundaries into `Aperture`/`Door` sub-faces), lowers the surface's `MaterialComposition.LayerSet` onto `EnergyMaterial` + `OpaqueConstructionAbridged` entries populated into the model store through `Model.AddMaterials`/`AddConstructions` (the abridged form referenced from `FaceEnergyPropertiesAbridged.Construction` — the abridged-reference law, never expand-then-reinline), assembles `new Hb.Model(identifier, properties, rooms:, units: Meters, tolerance: Header.Tolerance)` and emits `model.ToJson()`; the `dfjson` arm folds the `IfcBuilding`/`IfcBuildingStorey` `Compose` tree into `Building`/`Story` with each space's footprint ring flattened to a `Room2D` floor plate; `EnergyTranslate.Run(EnergyDoc source, InterchangeFormat target, Instant at, Op key)` resolves the `(source, target)` matrix row — `osm→gbxml` (`GbXMLForwardTranslator.modelToGbXMLString`), `osm→idf` (`EnergyPlusForwardTranslator.translateModel` + `Workspace.save`), `gbxml→osm`/`idf→osm` (the reverse readers + `Model.save`), `osm→osm` (the `VersionTranslator` version-upgrade row) — and emits the translated bytes as an `EnergyArtifact` with the translator `warnings()`/`errors()` tallied.
- Auto: the lowered model carries the SEMANTIC envelope and library only — no `SimulationParameter`, no run period, no conditioning, no weather (simulation context is Compute's or the python recipe plane's); boundary conditions derive from the boundary edge's `BoundaryCondition` payload where the raise stamped one (`Ground`/`Adiabatic`/`Outdoors` text → the `IBoundarycondition` case) and default `Outdoors` otherwise; the artifact content-key is the emitted-bytes derivation with `Graph = Some(ContentAddress.OfGraph(graph))` stamping the semantic pedigree.
- Receipt: one `EnergyReceipt` per emit — `Lowered` legs count the folded spaces/surfaces/openings/constructions, `Translated` legs count the translator warnings; the emitted `IsValid(throwException: false)` miss folds into `Warnings`, never an exception.
- Packages: HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, Rasm.Element, Rasm, LanguageExt.Core, NodaTime
- Growth: a new lower target is one `EnergyDerive` arm row; a new translation is one `Matrix` row over a verified translator member (SDD via `SddForwardTranslator`/`SddReverseTranslator` is the named next row); the glazing lower is the declared deepening — the seam `MaterialPropertySet.Optical` case (`Discipline.Energy`) lowers onto `EnergyWindowMaterialGlazing` + `WindowConstruction` as one arm row the moment aperture composition evidence lands on raised/projected graphs; per-space program/loads lower as `ProgramTypeAbridged` rows once the seam carries occupancy evidence.
- Boundary: the lower reads the graph through seam-owned surfaces (`ObjectNodes`, `EdgesAt`, `CompositionOf`, `Material`, the `GeometrySource` port) and the `Model/query#ELEMENT_SET` algebra for scope selection — a Compute-owned discipline read (`SpacesOf`/`BoundingSurfacesOf`) is never referenced (Compute is a peer stratum, not a dependency); a space whose footprint blob is absent lowers as a logged warning and a skipped room, never a zero-area fabrication; the graph→OSM/gbXML/IDF DIRECT egress is deliberately absent — no in-process HBJSON→OSM translation is admitted (`honeybee-openstudio` is the python peer's leg) and a second graph→OSM builder beside Compute's simulation-scoped `BuildModel` would be the duplicate-fold defect, so the request rails `BimFault.CapabilityMiss` (`energy-graph-egress-pending`) and the capability lands as one matrix column when its translation is admitted; the translate temp-path crossings and the SWIG handle brackets are the named platform-forced statement seam; `Workspace.save`/`Model.save` path-bound emits cross a bracketed scratch file exactly as the decode arms do.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Frozen;
using System.IO;
using System.Linq;
using System.Text;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Element;
using static LanguageExt.Prelude;
using Df = DragonflySchema;
using Hb = HoneybeeSchema;
using Os = OpenStudio;

namespace Rasm.Bim;

// --- [OPERATIONS] --------------------------------------------------------------------------
// The BIM-to-BEM lower: graph -> honeybee envelope + library (hbjson) or dragonfly massing (dfjson). The
// model store populates through Model.Add* (the canonical lists), faces reference by abridged id.
public static class EnergyDerive {
    internal static Fin<EnergyOutcome.Emitted> Lower(
        ElementGraph graph, InterchangeFormat target, EnergyScope scope, GeometrySource geometry, Instant at, Op key) =>
        target == InterchangeFormat.Hbjson ? Honeybee(graph, scope, geometry, at, key)
        : target == InterchangeFormat.Dfjson ? Dragonfly(graph, scope, geometry, at, key)
        : EnergyProjector.Serves(target)
            ? Fin.Fail<EnergyOutcome.Emitted>(new BimFault.CapabilityMiss(key, $"energy-graph-egress-pending:{target.Key}"))
            : Fin.Fail<EnergyOutcome.Emitted>(new BimFault.CodecReject(key, $"energy-lower-unsupported:{target.Key}"));

    static Fin<EnergyOutcome.Emitted> Honeybee(ElementGraph graph, EnergyScope scope, GeometrySource geometry, Instant at, Op key) {
        var store = new Hb.ModelEnergyProperties();
        var state = (Rooms: Seq<Hb.Room>(), Surfaces: 0, Openings: 0, Constructions: 0, Warnings: 0);
        foreach (Node.Object space in SpacesUnder(graph, scope)) {
            var faces = Seq<Hb.Face>();
            foreach ((Relationship.Generic edge, Node.Object surface) in Boundaries(graph, space.Id)) {
                Option<FootprintPolygon> ring = geometry.Footprint(surface.Representations);
                if (ring.IsNone) { state.Warnings++; continue; }
                if (!EnergyClassRows.ToFace.TryGetValue((surface.Classification.Code, surface.PredefinedType), out var faceType)) {
                    state.Warnings++; continue;
                }
                state.Surfaces++;
                Option<string> construction = LowerComposition(graph, surface.Id, store)
                    .Do(_ => state.Constructions++);
                faces = faces.Add(new Hb.Face(
                    surface.ExternalId.IfNone(surface.Name), Face3D(ring.ValueUnsafe()), faceType,
                    Condition(edge), new Hb.FacePropertiesAbridged(
                        energy: construction.Match(
                            Some: static id => new Hb.FaceEnergyPropertiesAbridged(construction: id),
                            None: static () => (Hb.FaceEnergyPropertiesAbridged?)null)),
                    apertures: Apertures(graph, space.Id, surface.ExternalId.IfNone(surface.Name), geometry, ref state.Openings)));
            }
            state.Rooms = state.Rooms.Add(new Hb.Room(
                space.ExternalId.IfNone(space.Name), [.. faces], new Hb.RoomPropertiesAbridged()));
        }
        var model = new Hb.Model($"rasm-energy-{at.ToUnixTimeSeconds()}", new Hb.ModelProperties(energy: store),
            rooms: [.. state.Rooms], units: Hb.Units.Meters, tolerance: graph.Header.Tolerance);
        int warnings = state.Warnings + model.Validate().Count();
        return Fin.Succ(Emit(InterchangeFormat.Hbjson, Encoding.UTF8.GetBytes(model.ToJson()), graph, at,
            new EnergyReceipt(EnergyLeg.Lowered, InterchangeFormat.Hbjson, None,
                state.Rooms.Count, state.Surfaces, state.Openings, state.Constructions, warnings,
                default, at)));
    }

    // LayerSet -> EnergyMaterial rows + one OpaqueConstructionAbridged, dedup-appended through the
    // Extension-backed Add* mutators; the construction id is the surface composition's material-key join. The
    // 1000 kg/m3 density fallback mirrors the Compute OSM-build row — the seam Thermal case carries no density.
    static Option<string> LowerComposition(ElementGraph graph, NodeId surface, Hb.ModelEnergyProperties store) =>
        graph.CompositionOf(surface).Bind(static c => c is MaterialComposition.LayerSet set ? Some(set) : None)
            .Bind(set => set.Layers
                .Traverse(layer => graph.Material(layer.Material)
                    .Bind(node => node.Properties.ToSeq().Choose(static p => p is MaterialPropertySet.Thermal t ? Some(t) : None).Head
                        .Map(thermal => new Hb.EnergyMaterial(
                            layer.Material.ToString(), layer.Thickness.Si, thermal.Conductivity.Si,
                            1000.0, thermal.SpecificHeat.Si))))
                .Map(materials => {
                    string id = $"con-{surface}";
                    foreach (var m in materials) { store.AddMaterial(m); }
                    store.AddConstruction(new Hb.OpaqueConstructionAbridged(id, [.. materials.Map(static m => m.Identifier)]));
                    return id;
                }));

    static Hb.Face3D Face3D(FootprintPolygon ring) =>
        new([.. ring.Ring.Map(static p => (List<double>)[p.X, p.Y, p.Z])]);

    // Boundary-condition derivation: the edge payload the raise stamped, defaulting Outdoors — the closed
    // IBoundarycondition set dispatched by text, never a downcast chain.
    static Hb.AnyOf<Hb.Ground, Hb.Outdoors, Hb.Adiabatic, Hb.Surface, Hb.OtherSideTemperature> Condition(Relationship.Generic edge) =>
        edge.Attributes.Find(PropertyName.Create("BoundaryCondition"))
            .Bind(static v => v is PropertyValue.Text t ? Some(t.Value) : None)
            .Match<Hb.AnyOf<Hb.Ground, Hb.Outdoors, Hb.Adiabatic, Hb.Surface, Hb.OtherSideTemperature>>(
                Some: static text => text switch {
                    "Ground"    => new Hb.Ground(),
                    "Adiabatic" => new Hb.Adiabatic(),
                    _           => new Hb.Outdoors(),
                },
                None: static () => new Hb.Outdoors());

    static Seq<Node.Object> SpacesUnder(ElementGraph graph, EnergyScope scope) =>
        graph.ObjectNodes.Filter(o => o.Classification.Code == IfcClass.Space.Key)
            .Filter(o => scope.Switch(
                wholeModel: static _ => true,
                spaces:     s => o.ExternalId.Exists(s.GlobalIds.Contains)))
            .ToSeq();

    // Host-attributed boundary edges are OPENING boundaries (the raise's correlation idiom) — excluded here so
    // a window never folds as an opaque face; the Apertures read consumes them.
    static Seq<(Relationship.Generic Edge, Node.Object Surface)> Boundaries(ElementGraph graph, NodeId space) =>
        graph.EdgesAt(space).Choose(e =>
            e is Relationship.Generic g && g.WireName == IfcRelKind.SpaceBoundary.Key && g.Relating == space
                && g.Attributes.Find(PropertyName.Create("Host")).IsNone
                ? graph.Find<Node.Object>(g.Related).Map(s => (g, s))
                : None).ToSeq();

    // The openings of one face: the space's boundary edges whose Host attribute names the face identifier —
    // the raise's correlation idiom read back, never a NodeId join (rooted ids are raise-local).
    static List<Hb.Aperture> Apertures(ElementGraph graph, NodeId space, string hostIdentifier, GeometrySource geometry, ref int openings) {
        var apertures = new List<Hb.Aperture>();
        foreach (var opening in graph.EdgesAt(space).Choose(e =>
            e is Relationship.Generic g && g.WireName == IfcRelKind.SpaceBoundary.Key && g.Relating == space
                && g.Attributes.Find(PropertyName.Create("Host")).Exists(v => v is PropertyValue.Text t && t.Value == hostIdentifier)
                ? graph.Find<Node.Object>(g.Related) : None)) {
            if (opening.Classification.Code != IfcClass.Window.Key) { continue; }
            foreach (var ring in geometry.Footprint(opening.Representations)) {
                apertures.Add(new Hb.Aperture(opening.ExternalId.IfNone(opening.Name), Face3D(ring),
                    new Hb.Outdoors(), new Hb.AperturePropertiesAbridged()));
                openings++;
            }
        }
        return apertures;
    }

    // Dragonfly massing: the OWNING Compose tree lowers Building/Story shells and each space's footprint plate
    // flattens onto a Room2D floor boundary — massing altitude only, no energy library, the honeybee shape inverted.
    static Fin<EnergyOutcome.Emitted> Dragonfly(ElementGraph graph, EnergyScope scope, GeometrySource geometry, Instant at, Op key) {
        var buildings = Seq<Df.Building>();
        int spaces = 0, warnings = 0;
        foreach (Node.Object building in graph.ObjectNodes.Filter(o => o.Classification.Code == IfcClass.Building.Key)) {
            var stories = Seq<Df.Story>();
            foreach (Node.Object storey in Parts(graph, building.Id, IfcClass.BuildingStorey)) {
                var plates = Seq<Df.Room2D>();
                foreach (Node.Object space in Parts(graph, storey.Id, IfcClass.Space).Filter(s => InScope(s, scope))) {
                    Option<FootprintPolygon> ring = geometry.Footprint(space.Representations);
                    if (ring.IsNone) { warnings++; continue; }
                    Seq<Vector3> plate = ring.ValueUnsafe().Ring;
                    spaces++;
                    plates = plates.Add(new Df.Room2D(
                        space.ExternalId.IfNone(space.Name),
                        [.. plate.Map(static p => (List<double>)[p.X, p.Y])],
                        plate.Head.Map(static p => p.Z).IfNone(0.0),
                        Height(graph, space.Id).IfNone(DefaultFloorToCeiling),
                        new Df.Room2DPropertiesAbridged()));
                }
                if (!plates.IsEmpty) {
                    stories = stories.Add(new Df.Story(storey.ExternalId.IfNone(storey.Name), [.. plates], new Df.StoryPropertiesAbridged()));
                }
            }
            if (!stories.IsEmpty) {
                buildings = buildings.Add(new Df.Building(building.ExternalId.IfNone(building.Name),
                    new Df.BuildingPropertiesAbridged(), uniqueStories: [.. stories]));
            }
        }
        var model = new Df.Model($"rasm-massing-{at.ToUnixTimeSeconds()}", new Df.ModelProperties(),
            buildings: [.. buildings], units: Df.Units.Meters, tolerance: graph.Header.Tolerance);
        warnings += model.Validate().Count();
        return Fin.Succ(Emit(InterchangeFormat.Dfjson, Encoding.UTF8.GetBytes(model.ToJson()), graph, at,
            new EnergyReceipt(EnergyLeg.Lowered, InterchangeFormat.Dfjson, None, spaces, 0, 0, 0, warnings, default, at)));
    }

    // The documented massing fallback when a space carries no Qto height — a named policy default (the Compute
    // density-1000 precedent), never a silent zero; a real height reads the Qto_SpaceBaseQuantities bag first.
    const double DefaultFloorToCeiling = 3.0;

    static Option<double> Height(ElementGraph graph, NodeId space) =>
        graph.EdgesAt(space).Choose(e =>
            e is Relationship.Assign { SubKind: var k } a && k == AssignKind.PropertyDefinition && a.Subject == space
                ? graph.Find<Node.QuantitySet>(a.Definition) : None)
            .Filter(static qs => qs.Bag.SetName == "Qto_SpaceBaseQuantities").Head
            .Bind(static qs => qs.Bag.Values.Find(PropertyName.Create("Height")))
            .Bind(static m => m.Length);

    // Transitive OWNING decomposition step (aggregate/nest/contain, never Reference), class-filtered — the
    // same descent law the Compute spatial reads state.
    static Seq<Node.Object> Parts(ElementGraph graph, NodeId whole, IfcClass @class) =>
        graph.EdgesAt(whole).Choose(e =>
            e is Relationship.Compose c && c.Whole == whole && c.SubKind != ComposeKind.Reference
                ? graph.Find<Node.Object>(c.Part).Filter(o => o.Classification.Code == @class.Key)
                : None).ToSeq();

    static bool InScope(Node.Object space, EnergyScope scope) => scope.Switch(
        wholeModel: static _ => true,
        spaces:     s => space.ExternalId.Exists(s.GlobalIds.Contains));

    static EnergyOutcome.Emitted Emit(InterchangeFormat format, byte[] bytes, ElementGraph graph, Instant at, EnergyReceipt receipt) {
        EnergyArtifact artifact = EnergyArtifact.Of(format, bytes, Some(ContentAddress.OfGraph(graph)), at);
        return new EnergyOutcome.Emitted(artifact, receipt with { Key = artifact.ContentKey });
    }
}

// The OSM-centric translator matrix: (source, target) rows over verified OpenStudio members; a translation
// is one row, never a per-pair method family. Path-bound emits cross a bracketed scratch file.
public static class EnergyTranslate {
    static readonly FrozenDictionary<(InterchangeFormat Source, InterchangeFormat Target), Func<EnergyDoc, Op, Fin<(byte[] Bytes, int Warnings)>>> Matrix =
        new KeyValuePair<(InterchangeFormat, InterchangeFormat), Func<EnergyDoc, Op, Fin<(byte[], int)>>>[] {
            new((InterchangeFormat.Osm,   InterchangeFormat.GbXml), static (doc, key) => OsmTo(doc, key, static (model, tally) => {
                using Os.GbXMLForwardTranslator gb = new();
                string xml = gb.modelToGbXMLString(model);
                return (Encoding.UTF8.GetBytes(xml), tally + Tally(gb.warnings(), gb.errors()));
            })),
            new((InterchangeFormat.Osm,   InterchangeFormat.Idf),   static (doc, key) => OsmTo(doc, key, static (model, tally) => {
                using Os.EnergyPlusForwardTranslator ep = new();
                using Os.Workspace idf = ep.translateModel(model);
                return (Saved(w => idf.save(w, true)), tally + Tally(ep.warnings(), ep.errors()));
            })),
            new((InterchangeFormat.Osm,   InterchangeFormat.Osm),   static (doc, key) => OsmTo(doc, key, static (model, tally) =>
                (Saved(w => model.save(w, true)), tally))),   // the VersionTranslator upgrade row: decode already upgraded
            new((InterchangeFormat.GbXml, InterchangeFormat.Osm),   static (doc, key) => ReverseTo(doc, key)),
            new((InterchangeFormat.Idf,   InterchangeFormat.Osm),   static (doc, key) => ReverseTo(doc, key)),
        }.ToFrozenDictionary();

    internal static Fin<EnergyOutcome.Emitted> Run(EnergyDoc source, InterchangeFormat target, Instant at, Op key) =>
        Matrix.TryGetValue((source.Format, target), out var row)
            ? row(source, key).Map(result => {
                EnergyArtifact artifact = EnergyArtifact.Of(target, result.Bytes, None, at);
                return new EnergyOutcome.Emitted(artifact, new EnergyReceipt(
                    EnergyLeg.Translated, source.Format, Some(target), 0, 0, 0, 0, result.Warnings, artifact.ContentKey, at));
            })
            : Fin.Fail<EnergyOutcome.Emitted>(new BimFault.CodecReject(key, $"energy-translate-miss:{source.Format.Key}->{target.Key}"));

    // Decode-then-emit over the version-upgrading in-string read; the emit lambda owns its translator brackets.
    static Fin<(byte[], int)> OsmTo(EnergyDoc doc, Op key, Func<Os.Model, int, (byte[], int)> emit) {
        try {
            using Os.VersionTranslator vt = new();
            using Os.OptionalModel optional = vt.loadModelFromString(doc.Text);
            if (!optional.is_initialized()) {
                return Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, "energy-decode:<osm-unreadable>"));
            }
            using Os.Model model = optional.get();
            return Fin.Succ(emit(model, Tally(vt.warnings(), vt.errors())));
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, $"energy-translate:{ex.Message}"));
        }
    }

    // gbXML/IDF -> OSM: the Path-bound reverse readers over a bracketed temp file, saved back as .osm bytes.
    static Fin<(byte[], int)> ReverseTo(EnergyDoc doc, Op key) {
        string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            File.WriteAllBytes(temp, doc.Bytes.ToArray());
            Os.Path path = Os.OpenStudioUtilitiesCore.toPath(temp);
            if (doc.Format == InterchangeFormat.GbXml) {
                using Os.GbXMLReverseTranslator gb = new();
                using Os.OptionalModel fromGb = gb.loadModel(path);
                return fromGb.is_initialized()
                    ? Save(fromGb.get(), Tally(gb.warnings(), gb.errors()))
                    : Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, "energy-decode:<gbxml-unreadable>"));
            }
            using Os.EnergyPlusReverseTranslator ep = new();
            using Os.OptionalModel fromIdf = ep.loadModel(path);
            return fromIdf.is_initialized()
                ? Save(fromIdf.get(), Tally(ep.warnings(), ep.errors()))
                : Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, "energy-decode:<idf-unreadable>"));
        }
        catch (Exception ex) when (ex is SystemException or ApplicationException) {
            return Fin.Fail<(byte[], int)>(new BimFault.ModelRejected(key, $"energy-translate:{ex.Message}"));
        }
        finally { File.Delete(temp); }
    }

    static Fin<(byte[], int)> Save(Os.Model model, int warnings) { using (model) { return Fin.Succ((Saved(w => model.save(w, true)), warnings)); } }

    // Path-bound emit crossed via a bracketed scratch path (Exemption: SWIG + filesystem boundary).
    static byte[] Saved(Action<Os.Path> save) {
        string temp = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try { save(Os.OpenStudioUtilitiesCore.toPath(temp)); return File.ReadAllBytes(temp); }
        finally { File.Delete(temp); }
    }

    static int Tally(Os.LogMessageVector warnings, Os.LogMessageVector errors) {
        using (warnings) using (errors) { return (int)(warnings.Count + errors.Count); }
    }
}
```

## [05]-[RESEARCH]

- [SCHEMA_STACK]: the three-package stack grounds against the folder catalogs and the decompile tier — `HoneybeeSchema 2.102.0` (`.api/api-honeybee-schema`): `Model.FromJson(string)`/`ToJson(bool)`, the required-`properties` `Model` ctor (`identifier, ModelProperties, …, Units units = Meters, double tolerance = 0.01`), `Room(identifier, List<Face>, RoomPropertiesAbridged, …)`, `Face(identifier, Face3D, FaceType, AnyOf<Ground,Outdoors,Adiabatic,Surface,OtherSideTemperature>, FacePropertiesAbridged, …)`, `Aperture(identifier, Face3D, AnyOf<Outdoors,Surface>, AperturePropertiesAbridged, …)`, `Face3D(List<List<double>> boundary, …)`, `EnergyMaterial(identifier, thickness, conductivity, density, specificHeat, …)`, `OpaqueConstructionAbridged(identifier, List<string> materials, …)`, `FaceEnergyPropertiesAbridged(string construction = null, …)`, the `Model.AddMaterials`/`AddConstructions` canonical-store mutators, `Validate()`/`IsValid(bool)`, and the `AnyOf.Obj` boxed read — all decompile-verified (`assay api query --key HoneybeeSchema`); `DragonflySchema 2.201.0` (`.api/api-dragonfly-schema`): `Model(identifier, ModelProperties, …, List<Building>, …)`, `Building(identifier, BuildingPropertiesAbridged, …, List<Story> uniqueStories, List<Room> room3ds, …)`, `Story(identifier, List<Room2D>, StoryPropertiesAbridged, …)`, `Room2D(identifier, List<List<double>> floorBoundary, floorHeight, floorToCeilingHeight, Room2DPropertiesAbridged, …)` — dragonfly composes the honeybee vocabulary by identifier (`[HONEYBEE_STACK_LAW]`), so the massing arm re-declares nothing.
- [TRANSLATOR_MATRIX]: the OSM-family members ground against the OpenStudio 3.11.0 decompile — `VersionTranslator.loadModelFromString(string) → OptionalModel` (the version-upgrading in-string read; `loadModel(Path)`/`originalVersion()` beside it), `GbXMLReverseTranslator.loadModel(Path) → OptionalModel`, `GbXMLForwardTranslator.modelToGbXML(Model, Path) → bool` / `modelToGbXMLString(Model) → string`, `EnergyPlusReverseTranslator.loadModel(Path) → OptionalModel` / `translateWorkspace(Workspace) → Model`, `EnergyPlusForwardTranslator.translateModel(Model) → Workspace`, every translator carrying `warnings()`/`errors()` `LogMessageVector`; the raise reads `Model.getSpaces() → SpaceVector`, the `Space.surfaces` PROPERTY (not a method), `PlanarSurface.vertices() → Point3dVector`/`.construction() → OptionalConstructionBase`, `Surface.surfaceType()`/`outsideBoundaryCondition()` strings, `Construction.layers() → MaterialVector`, and the typed-downcast pair `Model.getConstruction(UUID)`/`getStandardOpaqueMaterial(UUID)` over `IdfObject.handle()` — the SWIG binding constructs a vector element at its STATIC type with no `to_*` cast surface, so the handle re-read is the ONLY typed layer read and a C# `as StandardOpaqueMaterial` downcast is the verified-absent phantom; `StandardOpaqueMaterial.conductivity()/density()/specificHeat()/thickness()` getters and the 1-arg `StandardOpaqueMaterial(Model)` ctor both exist on the decompile.
- [SEAM_ALIGNMENT]: the raise lands the exact contract `csharp:Rasm.Compute/Analysis/energy` `EnergyGraphReads` consumes — `IfcSpace` classification codes, `IfcRelSpaceBoundary`-named `Generic` edges with the three-valued `BoundaryLevel` payload (this page stamps `"2nd"`: a honeybee/OSM face is a per-space bounded surface, the second-level shape the Compute read prefers when both levels exist), and `Representations.FootPrint` content keys the seam `GeometrySource` resolves — so raised-then-simulated needs zero adapter and the alignment is the seam row `Exchange/energy ⇄ csharp:Rasm.Compute/Analysis`, never a reference; the footprint blob layout rides the seam `Rasm.Element/Projection/address#CANONICAL_WRITER` projection the `Projection/semantic` [M2] mint owns, and the raise returns the `(key, ring)` pairs so the caller lands blobs WRITE-BLOB-FIRST before the delta applies; the lower reads `CompositionOf`/`Material`/`MaterialPropertySet.Thermal` exactly as the Compute OSM build does, so the two derivations of one graph agree on the layer physics by construction. The seam `MaterialPropertySet` carries `Thermal` (no density column) and the new `Optical` case (`Discipline.Energy`) — the glazing lower (`EnergyWindowMaterialGlazing`/`WindowConstruction`, ctors decompile-verified) is the chartered deepening over it.
- [PYTHON_PLANE]: the cross-language alignment is document bytes + one content identity, never a shared client — the py `honeybee-core`/`dragonfly-core` stack (RASM-PY-GEOMETRY `[V1_ENERGY_PLANE]` `energy/model`/`energy/district`) reads and writes the same HBJSON/DFJSON documents this page exchanges, `honeybee-openstudio` OSM/IDF translation and the recipe execution (`RASM-PY-RUNTIME` `[V3_RECIPE_OWNER]`) are the python plane's legs, and an `EnergyArtifact` landed content-keyed on the object plane is reusable by either runtime through the one seed-zero `XxHash128` regime; the deliberate C#-side absence — no in-process HBJSON→OSM translation — is what keeps the two OpenStudio integrations (py `honeybee-openstudio`, C# SWIG) aligned-not-coupled.
- [FORMAT_ROWS]: the five energy rows land on the `Exchange/format#FORMAT_AXIS` per its own growth law — one `InterchangeCodec.EnergyModel` row (`energy-model`, managed, no companion) and five `InterchangeFormat` rows: `Hbjson` (`application/vnd.ladybug.hbjson+json`, `.hbjson`, import+export), `Dfjson` (`application/vnd.ladybug.dfjson+json`, `.dfjson`, import+export), `Osm` (`application/vnd.openstudio.osm`, `.osm`, import-only against the graph), `GbXml` (`application/vnd.gbxml+xml`, `.gbxml`, import-only), `Idf` (`application/vnd.energyplus.idf`, `.idf`, import-only) — all `TessellationRequiresCompanion=false`, `frame: BasisChange.Identity` (ladybug, OSM, and gbXML are Z-up right-handed), `StepProtocol.None`; every capability column names this page's realizing arm (`CanImport` = the projector arms, `CanExport` = the `EnergyDerive` hbjson/dfjson arms; the osm/gbxml/idf EMIT capability rides the `Translate` matrix over an admitted OSM-family source, not the graph, so their `CanExport` stays false until the graph-egress arm lands).
