# [BIM_ENERGY_EXCHANGE]

`EnergyExchange.Apply(EnergyOp)` is the building-energy-model exchange entry: it raises HBJSON/DFJSON/OSM/gbXML/IDF documents onto the seam `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` as graph content, lowers graph content back to the two managed authoring schemas, and translates between the OSM-centric formats — the semantic MODEL-EXCHANGE leg of the `[ENERGY_MODEL_EXCHANGE]` group. Energy SIMULATION — the conditioned annual OSM, the EnergyPlus run, the results `SqlFile` — is `Rasm.Compute/Analysis/energy`'s and never re-authored here: this folder moves MODELS, that page runs them, aligned by the seam graph and never coupled.

Three sibling owners realize the arms: `Energy/projector#ENERGY_PROJECTOR` `EnergyProjector : IElementProjection` raises, `Energy/derive#MODEL_DERIVE` `EnergyDerive` lowers to a honeybee building envelope or dragonfly massing, and `Energy/derive#TRANSLATE_MATRIX` `EnergyTranslate` translates over the OSM-centric `(source, target)` table.

`HoneybeeSchema` binds the HBJSON object graph and its energy library, `DragonflySchema` composes the DFJSON urban-massing layer over the honeybee vocabulary by identifier without re-declaring it, and `NREL.OpenStudio.macOS-arm64` owns the SWIG OSM/IDF object store and its translator matrix.

Every emitted `EnergyArtifact` is content-keyed and round-trips as a re-admitted `EnergyDoc` under the identical key. Caller-side, artifact bytes land WRITE-BLOB-FIRST on the `Rasm.Persistence` object plane through `ArtifactIndexRow` and `Rasm.Bim` mints no Persistence reference; the `python:geometry` energy plane meets this folder ONLY at content-keyed document bytes and the wire, `honeybee-openstudio` OSM/IDF translation being the python peer's leg and never a shared client.

Wire posture is HOST-LOCAL, foreign types boundary-confined: `HoneybeeSchema.*`/`DragonflySchema.*` DTOs and their LBT-Newtonsoft codec never escape the arm fences, `Model.FromJson`/`ToJson` being the ONLY spec-valid HBJSON/DFJSON codec; every `OpenStudio.*` SWIG wrapper owns a native handle, brackets `using` per arm, never crosses a signature, and lowers its `Optional<T>` at the boundary.

Faults route the existing `Model/faults#FAULT_BAND` `BimFault` arms — band 2600, `Fault`-derived, bare lift, zero new cases: every terminal refusal carries its closed `BimScope.Energy` and `BimReason` axes, while captured Honeybee, Dragonfly, OpenStudio, scratch-write, and tessellation failures retain the original `Error` under `BimFault.BoundaryFailed`; the binding ships no HBJSON ingest, so graph→OSM/gbXML/IDF remains the explicit `BimReason.Capability` refusal.

## [01]-[INDEX]

- [02]-[ENERGY_EXCHANGE]: `EnergyExchange.Apply` over `EnergyOp` `[Union]`, the `EnergyDoc`/`EnergyArtifact` carriers, `EnergyScope`, `EnergyCensus`.

## [02]-[ENERGY_EXCHANGE]

- Owner: `EnergyExchange` the one exchange entry; `EnergyOp` the closed request `[Union]` (`Raise`/`Lower`/`Translate`); `EnergyOutcome` the two-case result (`Raised` graph+delta+footprint blobs, `Emitted` the content-keyed artifact); `EnergyDoc` the foreign-document carrier keyed on pure byte content; `EnergyArtifact` the emitted document carrying the optional seam graph pedigree; `EnergyScope`/`EnergyLeg` the request vocabulary; `EnergyCensus` the per-leg counts, keys, and degrade rows every outcome carries; `EnergyReason`/`EnergyNote` the degrade vocabulary all three legs note into that census; `ArtifactKey` the object-plane address value object the events message envelope and the results join both read.
- Entry: `EnergyExchange.Apply(EnergyOp op)` → `Fin<EnergyOutcome>` over the generated total `Switch`. Raise composes the `Projection/semantic#GRAPH_LEGALITY` `IfcLegality` constraint under `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `ProjectionAssembly.Assemble` because it mints `Classification("ifc", code)`/`PredefinedType` values — the legality arms gate an out-of-roster class or token exactly as an IFC ingest. Each typed `BimFault` lifts BARE onto the `Fin<T>` rail: band 2600 owns the generated `Code`, no `.ToError()` hop.
- Auto: identity is dual-keyed on the tessellation-bridge pattern — `EnergyDoc.SourceKey` and `EnergyArtifact.ContentKey` are ONE derivation, so the raise/lower round trip keys identically and the reuse join holds. `EnergyArtifact.Graph` is `Some` exactly on graph-lowered artifacts, so the Persistence artifact index joins a derived model back to its source graph without a parse. `ArtifactKey` shares the `Exchange/tessellation#TESSELLATION_BRIDGE` `:glb` `key:kind` address grammar — one object-plane address space.
- Output: `EnergyCensus` carries counts, keys, and typed degrade rows only, never payload bytes. Every leg notes an `EnergyNote` — a lowered model's `Validate()` DataAnnotations results, the OpenStudio translator `warnings()`/`errors()` tally, each degraded construction, unmatched segment, or unmapped face — and `Warnings` derives as the tally OF those rows, so the census states which evidence a document lost rather than how many times. Raise gates DataAnnotations inside `FromJson`, so its rows are degrade rows alone; a native diagnostic vector no managed read enumerates lands ONE row carrying its own count, which is why `EnergyNote.Tally` is a column.
- Events: an `Emitted` outcome fires the `Model/observability#HOOK_RAIL` `rasm.bim.energy.emitted` point with `BimFact.Emitted` — the `ArtifactKey` value, the `EnergyLeg` key, the format key, and the warnings tally off the `EnergyCensus` — at the one `EnergyOutcome` edge; the CloudEvents announcement is `Exchange/events#EVENT_PROJECTION`'s observe subscription over that point, subject the artifact's content-key head, and a message envelope minted at this rail is the deleted form.
- Packages: HoneybeeSchema, DragonflySchema, NREL.OpenStudio.macOS-arm64, GeometryGymIFC_Core (roster vocabulary via `IfcClass`), Rasm.Element, Rasm, LanguageExt.Core, NodaTime, Thinktecture.Runtime.Extensions
- Growth: a new energy-model form (an ISO 52016 XML, a FloorspaceJS floorplan) is one `Exchange/format#FORMAT_AXIS` `InterchangeFormat` row on the `energy-model` codec with one `Energy/projector` arm row or one `Energy/derive` matrix row, never a per-form `HbjsonImporter`/`OsmExporter` class; a new lower target is one `EnergyDerive` arm row; a new scope modality (a storey filter, a zone filter) is one `EnergyScope` case. Graph→OSM/gbXML/IDF egress is a SETTLED negative, never a growth arm: the OpenStudio binding's reverse-translator roster holds gbXML/IDF/SDD and no HBJSON reader, HBJSON→OSM is the python peer's `honeybee-openstudio` wire leg, and an in-process graph→OSM build is Compute's simulation-scoped `BuildModel` — so the request faults `Refused/BimReason.Capability`, never a silent partial, and the canonical egress is the two-hop composition `Lower` HBJSON → peer wire → `Translate` fan-out to gbXML/IDF.
- Boundary: five energy rows (`hbjson`/`dfjson`/`osm`/`gbxml`/`idf`) live on the ONE `Exchange/format#FORMAT_AXIS` `InterchangeFormat` table under an `energy-model` `InterchangeCodec` row, every capability column naming this folder's realizing arm — raise = `CanImport`, the hbjson/dfjson lower = `CanExport`, the osm/gbxml/idf emit rides the `Translate` matrix over an admitted OSM-family source, so those rows hold `CanExport=false` against the graph. `Rasm.Compute` project references in either direction are the named strata defect: Compute simulates what the projector raises and the derive lowers what any projector landed, so the two align on the seam graph alone. `SimulationParameter`, run periods, conditioning policy, and weather are SIMULATION context never authored here — a lowered model is the semantic building envelope and library, its run context Compute's locally or the python recipe plane's over the wire.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Text;
using LanguageExt;
using NodaTime;
using Rasm;
using Rasm.Bim.Model;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnergyScope {
    private EnergyScope() { }

    public sealed record WholeModel : EnergyScope;
    public sealed record Spaces(Seq<string> GlobalIds) : EnergyScope;

    public static readonly EnergyScope Whole = new WholeModel();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EnergyLeg {
    public static readonly EnergyLeg Raised     = new("raised");
    public static readonly EnergyLeg Lowered    = new("lowered");
    public static readonly EnergyLeg Translated = new("translated");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EnergyReason {
    public static readonly EnergyReason FootprintMissing   = new("footprint-missing");
    public static readonly EnergyReason ClassUnmapped      = new("class-unmapped");
    public static readonly EnergyReason PropertyIncomplete = new("property-incomplete");
    public static readonly EnergyReason CompositionMixed   = new("composition-mixed");
    public static readonly EnergyReason LayerUnresolved    = new("layer-unresolved");
    public static readonly EnergyReason SegmentUnmatched   = new("segment-unmatched");
    public static readonly EnergyReason OpeningTypeMiss    = new("opening-type-miss");
    public static readonly EnergyReason MeasureRejected    = new("measure-rejected");
    public static readonly EnergyReason SchemaAnnotation   = new("schema-annotation");
    public static readonly EnergyReason TranslatorLog      = new("translator-log");
}

[ValueObject<string>]
public sealed partial class ArtifactKey {
    const int HexWidth = 32;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        validationError =
            value is { } text
            && text.Length > HexWidth + 1
            && text.IndexOf(':', StringComparison.Ordinal) == HexWidth
            && UInt128.TryParse(text.AsSpan(0, HexWidth), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt128 content)
            && StringComparer.Ordinal.Equals(text[..HexWidth], content.ToString("x32", CultureInfo.InvariantCulture))
            && InterchangeFormat.TryGet(text[(HexWidth + 1)..], out InterchangeFormat? format)
            && format is { } admitted
            && StringComparer.Ordinal.Equals(text[(HexWidth + 1)..], admitted.Key)
                ? validationError
                : new ValidationError("artifact-key-grammar");
    }

    public UInt128 ContentKey =>
        UInt128.Parse(Value.AsSpan(0, HexWidth), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    public string FormatKey => Value[(HexWidth + 1)..];

    public static ArtifactKey Of(UInt128 contentKey, InterchangeFormat format) =>
        Create($"{contentKey.ToString("x32", CultureInfo.InvariantCulture)}:{format.Key}");

    const string AbsentValue = "<absent>";

    public static Fin<ArtifactKey> Admit(string? value, Op key) =>
        Optional(value)
            .Bind(static text => TryCreate(text, out ArtifactKey? row) && row is { } admitted ? Some(admitted) : None)
            .Match(
                Some: Fin.Succ,
                None: () => Fin.Fail<ArtifactKey>(
                    new BimFault.Refused(key, BimScope.Events, BimReason.Codec, string.Join(':', new object?[] { "event-artifact-key-malformed", Optional(value).IfNone(AbsentValue) }))));
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct EnergyNote(EnergyReason Reason, string Subject, int Tally);
public sealed record EnergyDoc(InterchangeFormat Format, ReadOnlyMemory<byte> Bytes) {
    public UInt128 SourceKey { get; } = KeyOf(Format, Bytes);

    public string Text { get; } = Encoding.UTF8.GetString(Bytes.Span);

    internal static UInt128 KeyOf(InterchangeFormat format, ReadOnlyMemory<byte> bytes) =>
        ContentHash.Of((format, bytes), static (s, writer) => writer.String(s.format.Key).Raw(s.bytes.Span));
}

public sealed record EnergyArtifact(
    InterchangeFormat Format, ReadOnlyMemory<byte> Bytes, UInt128 ContentKey,
    Option<ContentAddress> Graph, Instant At) {

    public ArtifactKey Address => ArtifactKey.Of(ContentKey, Format);
    public long ByteCount => Bytes.Length;

    public static EnergyArtifact Of(InterchangeFormat format, ReadOnlyMemory<byte> bytes, Option<ContentAddress> graph, Instant at) =>
        new(format, bytes, EnergyDoc.KeyOf(format, bytes), graph, at);
}

public sealed record EnergyCensus(
    EnergyLeg Leg, InterchangeFormat Form, Option<InterchangeFormat> Target,
    int Spaces, int Surfaces, int Openings, int Constructions, Seq<EnergyNote> Notes,
    UInt128 Key, Instant At) {
    public int Warnings => Notes.Fold(0, static (sum, note) => sum + note.Tally);
}

[Union]
public abstract partial record EnergyOutcome {
    private EnergyOutcome() { }

    public sealed record Raised(ElementGraph Graph, GraphDelta Delta, Seq<(UInt128 Key, FootprintPolygon Ring)> Footprints, EnergyCensus Census) : EnergyOutcome;
    public sealed record Emitted(EnergyArtifact Artifact, EnergyCensus Census) : EnergyOutcome;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EnergyOp {
    private EnergyOp() { }

    public sealed record Raise(EnergyDoc Source, ElementGraph Seed, ProjectionContext Ctx) : EnergyOp;
    public sealed record Lower(ElementGraph Graph, InterchangeFormat Target, EnergyScope Scope, GeometrySource Geometry, Instant At, Op Key) : EnergyOp;
    public sealed record Translate(EnergyDoc Source, InterchangeFormat Target, Instant At, Op Key, TranslateLane Lane) : EnergyOp;
}

public static class EnergyExchange {
    public static Fin<EnergyOutcome> Apply(EnergyOp op) => op.Switch(
        raise: static r => EnergyProjector.Of(r.Source, r.Ctx.Key).Bind(projector =>
            ProjectionAssembly.Assemble(
                    ProjectionSuite.Of(Seq<IElementProjection>(projector), Seq(ConstraintRegistration.Of(new IfcLegality()))),
                    r.Seed, r.Ctx)
                .Map(result => (EnergyOutcome)new EnergyOutcome.Raised(
                    result.Graph, result.Delta, projector.Footprints, projector.Census(r.Ctx.At)))),
        lower: static l => EnergyDerive.Lower(l.Graph, l.Target, l.Scope, l.Geometry, l.At, l.Key)
            .Map(static emitted => (EnergyOutcome)emitted),
        translate: static t => EnergyTranslate.Run(t.Source, t.Target, t.At, t.Key, t.Lane)
            .Map(static emitted => (EnergyOutcome)emitted));
}
```

## [03]-[RESEARCH]

(none)
