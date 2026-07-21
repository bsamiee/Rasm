# [BIM_ENERGY_EXCHANGE]

`EnergyExchange.Apply(EnergyOp)` is the building-energy-model exchange entry: it raises HBJSON/DFJSON/OSM/gbXML/IDF documents onto the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` as graph content, lowers graph content back to the two managed authoring schemas, and translates between the OSM-centric formats — the semantic MODEL-EXCHANGE leg of the `[ENERGY_MODEL_EXCHANGE]` group. Energy SIMULATION — the conditioned annual OSM, the EnergyPlus run, the results `SqlFile` — is `csharp:Rasm.Compute/Analysis/energy`'s and never re-authored here: this folder moves MODELS, that page runs them, aligned by the seam graph and never coupled.

Three sibling owners realize the arms: `Energy/projector#ENERGY_PROJECTOR` `EnergyProjector : IElementProjection` raises, `Energy/derive#MODEL_DERIVE` `EnergyDerive` lowers to a honeybee envelope or dragonfly massing, and `Energy/derive#TRANSLATE_MATRIX` `EnergyTranslate` translates over the OSM-centric `(source, target)` table.

`HoneybeeSchema` binds the HBJSON object graph and its energy library, `DragonflySchema` composes the DFJSON urban-massing layer over the honeybee vocabulary by identifier without re-declaring it, and `NREL.OpenStudio.macOS-arm64` owns the SWIG OSM/IDF object store and its translator matrix.

Every emitted `EnergyArtifact` is content-keyed and round-trips as a re-admitted `EnergyDoc` under the identical key. Caller-side, artifact bytes land WRITE-BLOB-FIRST on the `Rasm.Persistence` object plane through `ArtifactIndexRow` and `Rasm.Bim` mints no Persistence reference; the `python:geometry` energy plane meets this folder ONLY at content-keyed document bytes and the wire, `honeybee-openstudio` OSM/IDF translation being the python peer's leg and never a shared client.

Wire posture is HOST-LOCAL, foreign types boundary-confined: `HoneybeeSchema.*`/`DragonflySchema.*` DTOs and their LBT-Newtonsoft codec never escape the arm fences, `Model.FromJson`/`ToJson` being the ONLY spec-valid HBJSON/DFJSON codec; every `OpenStudio.*` SWIG wrapper owns a native handle, brackets `using` per arm, never crosses a signature, and lowers its `Optional<T>` at the boundary.

Faults route the existing `Model/faults#FAULT_BAND` `BimFault` arms — band 2600, `Expected`-derived, bare lift, zero new cases: a decode/validation reject or null-parse lifts `ModelRejected` (`energy-decode`), an unmappable `FaceType`/surface class lifts `UnmappedClass` (`energy-face-miss`/`energy-class-miss`), an abridged identifier resolving nothing lifts `DanglingReference` (`energy-construction-absent`), an unserved form or translator-matrix miss lifts `CodecReject` (`energy-form-miss`/`energy-lower-unsupported`/`energy-translate-miss`), and a graph→OSM/gbXML/IDF egress request lifts `CapabilityMiss` (`energy-graph-egress-pending`) until its arm lands.

## [01]-[INDEX]

- [01]-[ENERGY_EXCHANGE]: `EnergyExchange.Apply` over `EnergyOp` `[Union]`, the `EnergyDoc`/`EnergyArtifact` carriers, `EnergyScope`, `EnergyReceipt`.

## [02]-[ENERGY_EXCHANGE]

- Owner: `EnergyExchange` the one exchange entry; `EnergyOp` the closed request `[Union]` (`Raise`/`Lower`/`Translate`); `EnergyOutcome` the two-case result (`Raised` graph+delta+footprint blobs, `Emitted` the content-keyed artifact); `EnergyDoc` the foreign-document carrier keyed on pure byte content; `EnergyArtifact` the emitted document carrying the optional seam graph pedigree; `EnergyScope`/`EnergyLeg`/`EnergyReceipt` the request and evidence vocabulary.
- Entry: `EnergyExchange.Apply(EnergyOp op)` → `Fin<EnergyOutcome>` over the generated total `Switch`. Raise composes the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` constraint under `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `ProjectionAssembly.Assemble` because it mints `Classification("ifc", code)`/`PredefinedType` values — the legality arms gate an out-of-roster class or token exactly as an IFC ingest. Each typed `BimFault` lifts BARE onto the `Fin<T>` rail: band 2600 IS the `Expected` `Code`, no `.ToError()` hop.
- Auto: identity is dual-keyed on the tessellation-bridge pattern — `EnergyDoc.SourceKey` and `EnergyArtifact.ContentKey` are ONE derivation, so the raise/lower round trip keys identically and the reuse join holds. `EnergyArtifact.Graph` is `Some` exactly on graph-lowered artifacts, so the Persistence artifact index joins a derived model back to its source graph without a parse. `ArtifactKey` shares the `Exchange/tessellation#TESSELLATION_BRIDGE` `:glb` `key:kind` address grammar — one object-plane address space.
- Receipt: `EnergyReceipt` carries counts, keys, and evidence only, never payload bytes. Its `Warnings` tally folds the lowered models' `Validate()` DataAnnotations results, OpenStudio translator `warnings()`, and degraded-construction notes; the raise gates DataAnnotations inside `FromJson`, so its tally carries only degrade warnings.
- Events: an `Emitted` outcome mints the `Exchange/events#EVENTS` `BimEvent.EnergyMinted` row at the composing rail's edge — the `ArtifactKey`, the `EnergyLeg` key, the format key, and the warnings tally off the `EnergyReceipt` — subject the `ArtifactKey`; envelope seal and transport are the events owner's, never a second emit path here.
- Packages: HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, GeometryGymIFC_Core (roster vocabulary via `IfcClass`), Rasm.Element, Rasm, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions
- Growth: a new energy-model form (an ISO 52016 XML, a FloorspaceJS floorplan) is one `Exchange/format#FORMAT_AXIS` `InterchangeFormat` row on the `energy-model` codec with one `Energy/projector` arm row or one `Energy/derive` matrix row, never a per-form `HbjsonImporter`/`OsmExporter` class; a new lower target is one `EnergyDerive` arm row; a new scope modality (a storey filter, a zone filter) is one `EnergyScope` case. Graph→OSM/gbXML egress is the declared growth arm, landing as ONE matrix column fed by the lowered honeybee leg once an in-process HBJSON→OSM translation is admitted; until then the request faults `CapabilityMiss`, never a silent partial.
- Boundary: five energy rows (`hbjson`/`dfjson`/`osm`/`gbxml`/`idf`) live on the ONE `Exchange/format#FORMAT_AXIS` `InterchangeFormat` table under an `energy-model` `InterchangeCodec` row, every capability column naming this folder's realizing arm — raise = `CanImport`, the hbjson/dfjson lower = `CanExport`, the osm/gbxml/idf emit rides the `Translate` matrix over an admitted OSM-family source, so those rows hold `CanExport=false` against the graph. `Rasm.Compute` project references in either direction are the named strata defect: Compute simulates what the projector raises and the derive lowers what any projector landed, so the two align on the seam graph alone. `SimulationParameter`, run periods, conditioning policy, and weather are SIMULATION context never authored here — a lowered model is the semantic envelope and library, its run context Compute's locally or the python recipe plane's over the wire.

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
// Exchange scope: whole model or a GlobalId-selected space subset, the case itself the modality per
// TessellationScope's law. Scope shapes the emitted BYTES, so the byte-content key partitions per scope
// with no scope token.
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

// Content-keyed emitted document: ContentKey is the SAME derivation as EnergyDoc.SourceKey, one fold.
// Graph is the semantic pedigree — Some exactly on graph-lowered artifacts, the ArtifactIndexRow join
// back to the producing ElementGraph.
public sealed record EnergyArtifact(
    InterchangeFormat Format, ReadOnlyMemory<byte> Bytes, UInt128 ContentKey,
    Option<ContentAddress> Graph, Instant At) {

    public string ArtifactKey => $"{ContentKey:x32}:{Format.Key}";
    public long ByteCount => Bytes.Length;

    public static EnergyArtifact Of(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Option<ContentAddress> graph, Instant at) =>
        new(format, bytes, new EnergyDoc(format, bytes).SourceKey, graph, at);
}

// Typed exchange evidence: coordinates and counts, never payload bytes.
public sealed record EnergyReceipt(
    EnergyLeg Leg, InterchangeFormat Form, Option<InterchangeFormat> Target,
    int Spaces, int Surfaces, int Openings, int Constructions, int Warnings,
    UInt128 Key, Instant At);

[Union]
public abstract partial record EnergyOutcome {
    private EnergyOutcome() { }

    // Raised graph, event delta, and analytical footprint blobs the caller lands WRITE-BLOB-FIRST on the
    // object plane BEFORE applying the delta; stamped Representations.FootPrint keys resolve through the seam
    // GeometrySource only after the blobs land — tessellation's write-blob-first law inverted to the caller.
    public sealed record Raised(ElementGraph Graph, GraphDelta Delta, Seq<(UInt128 Key, FootprintPolygon Ring)> Footprints, EnergyReceipt Receipt) : EnergyOutcome;
    public sealed record Emitted(EnergyArtifact Artifact, EnergyReceipt Receipt) : EnergyOutcome;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// ONE exchange request union: the case is the verb, the format row is the data.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnergyOp {
    private EnergyOp() { }

    public sealed record Raise(EnergyDoc Source, ElementGraph Seed, ProjectionContext Ctx) : EnergyOp;
    public sealed record Lower(ElementGraph Graph, InterchangeFormat Target, EnergyScope Scope, GeometrySource Geometry, Instant At, Op Key) : EnergyOp;
    public sealed record Translate(EnergyDoc Source, InterchangeFormat Target, Instant At, Op Key) : EnergyOp;
}

public static class EnergyExchange {
    // Raise runs under the seam Assemble fold with IfcLegality composed; Lower and Translate are artifact
    // emits. Every arm returns the one outcome union, and the fold counts ride the projector/derive state.
    public static Fin<EnergyOutcome> Apply(EnergyOp op) => op.Switch(
        raise: static r => {
            var projector = new EnergyProjector(r.Source);
            return EnergyProjector.Serves(r.Source.Format)
                ? ProjectionAssembly.Assemble(
                        ProjectionSuite.Of(Seq<IElementProjection>(projector), Seq(ConstraintRegistration.Of(new IfcLegality()))),
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

(none)
