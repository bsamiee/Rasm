# [BIM_TAXONOMY_EMITTER]

`IfcVocabularyEmitter` PRODUCES the `Model/elements#IFC_CLASS` committed row region at design time, on a lifecycle of GeometryGym pin bumps and IFC releases rather than model sessions. It reflects the `IfcObjectDefinition` closure off the pinned GeometryGymIFC_Core assembly, INTERSECTS it with the published buildingSMART EXPRESS change index, resolves discipline through one QuikGraph inheritance DAG, sources class and token spans attribute-first under the index as correcting authority, audits against the Materials `IfcBinding` seed pairs, and renders deterministic declarations between the region's marker pair. The committed table stays the system of record, so no runtime reflection, Roslyn generator, or sidecar file exists. Exactly two data surfaces feed it: the change index — one dotted key space where PRESENCE is published membership and the value is the span pin outranking a divergent GG attribute — and `IfcOverlays`, the committed hand-judgment tiers the regeneration replays. `VocabularyRegeneration` is the runnable entry a pin bump executes, and without it the committed region drifts against the assembly it mirrors — the exact failure the audit arms exist to make loud.

## [01]-[INDEX]

- [02]-[VOCABULARY_OVERLAYS]: `OverlayVerdict` the ONE census verdict per overlay key — abstract supertype, deprecated survivor, retired, retired-unmarked, ghost — `ClaimTier` the discipline-claim order as a behaviour column, `DomainClaim` the per-entity claim row, `IfcVocabulary` the joined emitter input, and `IfcOverlays` the committed hand tiers.
- [03]-[TAXONOMY_EMITTER]: `IfcVocabularyEmitter` — `Emit` the one entrypoint, `Census` the accumulating `[Obsolete]` tripwire, `DomainAtlas` the QuikGraph inheritance DAG with its BFS claim fold, `RowOf`/`Tokens`/`Introduced` the span-sourcing path, `Audit` the accumulating Gate-0 drift audit, and `Render` the deterministic region fold; `VocabularyRow` the row currency one render line commits.
- [04]-[REGENERATION]: `VocabularyRegeneration` the design-time entry a pin bump runs — the assembly, change index, and stamp seeds admitted, the emit run, the marker-pair region spliced into the committed source, and `EmittedRegion` the committed region's identity its exit code projects.

## [02]-[VOCABULARY_OVERLAYS]

- Owner: `IfcOverlays` the committed hand-judgment tiers the pin-bump regeneration replays — `Verdicts`, one `OverlayVerdict` per overlay key, and `Claims`, one `DomainClaim` per discipline-claiming entity. `OverlayVerdict` is the closed census family: `AbstractSupertype` (the roster member whose CLR flag misreports EXPRESS abstractness), `DeprecatedSurvivor` (a deprecation-flagged member whose window lawfully stays open — DEPRECATED is not REMOVED), `Retired` (a schema-diff closed window GG marks `[Obsolete]`), `RetiredBare` (a closed window GG ships with no mark), and `Ghost` (a published-closure name GG never ships, whose closed window accounts the closure with NO committed row). `IfcVocabulary` is the joined emitter input: the EXPRESS change index beside those two tiers.
- Cases: the five verdicts partition by WHAT the census must agree with on the reflected surface — `AbstractSupertype` and `DeprecatedSurvivor` carry no window, the three retirement arms carry the `(IntroducedIn, RemovedIn)` pair the row's `RemovedIn` reads, and each arm fixes the `[Obsolete]` mark it demands, so mark presence stops being a second roster to join. `ClaimTier` names the two claim orders: `Root` claims a reachability closure through the inheritance DAG, `Pin` re-points a single entity last.
- Entry: `IfcOverlays.Vocabulary(FrozenDictionary<string, Option<ReleaseVersion>> spans)` joins the loaded change index with the committed tiers into the ONE `IfcVocabulary` `Emit` reads — the tiers are DATA on this page, so no caller assembles them and no emit takes them apart as parameters.
- Auto: the change index is ONE dotted key space — `"IfcWall"` at class grain and `"IfcWall.PARAPET"` at token grain — where key PRESENCE is the published-membership gate at both grains and the value discriminates the two span authorities: `Some(release)` is the EXPRESS-diff pin that outranks a divergent GG attribute, `None` is a published member whose span comes from `[VersionAdded]` (or, for a token, the class floor). `ClaimTier.Apply` carries the claim behaviour as a row column, so the atlas folds one rank-ordered pass rather than a root pass followed by a pin pass.
- Packages: Rasm.Element (the shared `ReleaseVersion`), Thinktecture.Runtime.Extensions (`[Union]`, `[SmartEnum<int>]`, `[UseDelegateFromConstructor]`), QuikGraph, LanguageExt.Core
- Growth: a new discipline claim is one `Claims` row carrying its tier; a newly deprecated schema-live member is one `DeprecatedSurvivor` row; a newly retired entity is one `Retired` row the census DEMANDS the moment its `[Obsolete]` mark lands; a GG attribution gap or a newly published entity or token is a regenerated change-index entry, never a row here.
- Boundary: one key carries at most ONE verdict — the arms are chosen so no key needs two, and the retirement window rides the arm rather than a parallel window table; a second membership set beside the index is the retired form, because a set whose presence means "published" and a dictionary whose presence means "pinned" over the SAME dotted keys let a pin exist for an unpublished name with nothing to catch it. The grouping `IfcElectricalCircuit`/`IfcCondition` closed windows are standing rows, so the `Model/zones#ZONE_GRAPH` grouping overlay derives its retired windows from this one roster. `Retirements` derives NO window from an `[Obsolete]` message: the message encodes the DEPRECATION release, not the removal one — `IfcBeamStandardCase` reads `DEPRECATED IFC4` though its window closes at 4X3 — so the mark gates PRESENCE alone.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using QuikGraph;
using Rasm.Element.Graph;
using Thinktecture;
using static LanguageExt.Prelude;
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;

namespace Rasm.Bim.Model;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record OverlayVerdict {
    private OverlayVerdict() { }

    public sealed record AbstractSupertype : OverlayVerdict;
    public sealed record DeprecatedSurvivor : OverlayVerdict;
    public sealed record Retired(ReleaseVersion IntroducedIn, ReleaseVersion RemovedIn) : OverlayVerdict;
    public sealed record RetiredBare(ReleaseVersion IntroducedIn, ReleaseVersion RemovedIn) : OverlayVerdict;
    public sealed record Ghost(ReleaseVersion IntroducedIn, ReleaseVersion RemovedIn) : OverlayVerdict;
}

[SmartEnum<int>]
public sealed partial class ClaimTier {
    public static readonly ClaimTier Root = new(0, apply: static (dag, map, vertex, domain) => Reach(dag, map, vertex, domain));
    public static readonly ClaimTier Pin = new(1, apply: static (_, map, vertex, domain) => map.AddOrUpdate(vertex, domain));

    [UseDelegateFromConstructor]
    public partial HashMap<Type, IfcDomain> Apply(
        BidirectionalGraph<Type, SEdge<Type>> dag, HashMap<Type, IfcDomain> map, Type vertex, IfcDomain domain);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct DomainClaim(IfcDomain Domain, ClaimTier Tier);

public sealed record IfcVocabulary(
    FrozenDictionary<string, Option<ReleaseVersion>> Spans,
    FrozenDictionary<string, OverlayVerdict> Verdicts,
    FrozenDictionary<string, DomainClaim> Claims);

// --- [TABLES] --------------------------------------------------------------------------
internal static class IfcOverlays {
    public static IfcVocabulary Vocabulary(FrozenDictionary<string, Option<ReleaseVersion>> spans) =>
        new(spans, Verdicts, Claims);

    public static readonly FrozenDictionary<string, OverlayVerdict> Verdicts = new Dictionary<string, OverlayVerdict> {
        ["IfcTransportationDevice"] = new OverlayVerdict.AbstractSupertype(),

        ["IfcWallStandardCase"] = new OverlayVerdict.DeprecatedSurvivor(),
        ["IfcFilter"] = new OverlayVerdict.DeprecatedSurvivor(),
        ["IfcVibrationIsolator"] = new OverlayVerdict.DeprecatedSurvivor(),
        ["IfcVibrationIsolatorType"] = new OverlayVerdict.DeprecatedSurvivor(),
        ["IfcReinforcedSoil"] = new OverlayVerdict.DeprecatedSurvivor(),

        ["IfcBuildingElementComponent"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcChamferEdgeFeature"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcCondition"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcConditionCriterion"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcEdgeFeature"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcElectricDistributionPoint"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcElectricHeaterType"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcElectricalCircuit"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcElectricalElement"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcEquipmentElement"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcEquipmentStandard"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcFurnitureStandard"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcGasTerminalType"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcProjectOrderRecord"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcScheduleTimeControl"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcSpaceProgram"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcStructuralLinearActionVarying"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),

        ["IfcBeamStandardCase"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X3),
        ["IfcColumnStandardCase"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X3),
        ["IfcDoorStandardCase"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X3),
        ["IfcMemberStandardCase"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X3),
        ["IfcOpeningStandardCase"] = new OverlayVerdict.RetiredBare(ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X3),
        ["IfcPlateStandardCase"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X3),
        ["IfcSlabElementedCase"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X3),
        ["IfcSlabStandardCase"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X3),
        ["IfcWallElementedCase"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X3),
        ["IfcWindowStandardCase"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X3),
        ["IfcDoorStyle"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4X3),
        ["IfcWindowStyle"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4X3),
        ["IfcProxy"] = new OverlayVerdict.Retired(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4X3),

        ["IfcMove"] = new OverlayVerdict.Ghost(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcOrderAction"] = new OverlayVerdict.Ghost(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcRoundedEdgeFeature"] = new OverlayVerdict.Ghost(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcServiceLife"] = new OverlayVerdict.Ghost(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcStructuralPlanarActionVarying"] = new OverlayVerdict.Ghost(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcTimeSeriesSchedule"] = new OverlayVerdict.Ghost(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4),
        ["IfcBuildingElement"] = new OverlayVerdict.Ghost(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4X3),
        ["IfcBuildingElementType"] = new OverlayVerdict.Ghost(ReleaseVersion.Ifc2X3, ReleaseVersion.Ifc4X3),
        ["IfcBuildingSystem"] = new OverlayVerdict.Ghost(ReleaseVersion.Ifc4, ReleaseVersion.Ifc4X3),
    }.ToFrozenDictionary();

    public static readonly FrozenDictionary<string, DomainClaim> Claims = new Dictionary<string, DomainClaim> {
        ["IfcObjectDefinition"] = new(IfcDomain.General, ClaimTier.Root),
        ["IfcProcess"] = new(IfcDomain.Construction, ClaimTier.Root),
        ["IfcResource"] = new(IfcDomain.Construction, ClaimTier.Root),
        ["IfcDistributionControlElement"] = new(IfcDomain.Controls, ClaimTier.Root),
        ["IfcDistributionControlElementType"] = new(IfcDomain.Controls, ClaimTier.Root),
        ["IfcBuiltElement"] = new(IfcDomain.Architecture, ClaimTier.Root),
        ["IfcBuiltElementType"] = new(IfcDomain.Architecture, ClaimTier.Root),
        ["IfcFurnishingElement"] = new(IfcDomain.Architecture, ClaimTier.Root),
        ["IfcFurnishingElementType"] = new(IfcDomain.Architecture, ClaimTier.Root),
        ["IfcDistributionElement"] = new(IfcDomain.HvacFire, ClaimTier.Root),
        ["IfcDistributionElementType"] = new(IfcDomain.HvacFire, ClaimTier.Root),
        ["IfcStructuralActivity"] = new(IfcDomain.Structural, ClaimTier.Root),
        ["IfcStructuralItem"] = new(IfcDomain.Structural, ClaimTier.Root),
        ["IfcStructuralLoadGroup"] = new(IfcDomain.Structural, ClaimTier.Root),
        ["IfcStructuralResultGroup"] = new(IfcDomain.Structural, ClaimTier.Root),
        ["IfcStructuralAnalysisModel"] = new(IfcDomain.Structural, ClaimTier.Root),
        ["IfcElementComponent"] = new(IfcDomain.Structural, ClaimTier.Root),
        ["IfcElementComponentType"] = new(IfcDomain.Structural, ClaimTier.Root),
        ["IfcReinforcingElement"] = new(IfcDomain.Structural, ClaimTier.Root),
        ["IfcReinforcingElementType"] = new(IfcDomain.Structural, ClaimTier.Root),
        ["IfcDeepFoundation"] = new(IfcDomain.Structural, ClaimTier.Root),
        ["IfcDeepFoundationType"] = new(IfcDomain.Structural, ClaimTier.Root),
        ["IfcLinearElement"] = new(IfcDomain.Infrastructure, ClaimTier.Root),
        ["IfcPositioningElement"] = new(IfcDomain.Infrastructure, ClaimTier.Root),
        ["IfcCivilElement"] = new(IfcDomain.Infrastructure, ClaimTier.Root),
        ["IfcCivilElementType"] = new(IfcDomain.Infrastructure, ClaimTier.Root),
        ["IfcTransportationDevice"] = new(IfcDomain.Infrastructure, ClaimTier.Root),
        ["IfcTransportationDeviceType"] = new(IfcDomain.Infrastructure, ClaimTier.Root),
        ["IfcFacility"] = new(IfcDomain.Infrastructure, ClaimTier.Root),
        ["IfcFacilityPart"] = new(IfcDomain.Infrastructure, ClaimTier.Root),
        ["IfcGeotechnicalElement"] = new(IfcDomain.Geotechnical, ClaimTier.Root),
        ["IfcEarthworksElement"] = new(IfcDomain.Geotechnical, ClaimTier.Root),

        ["IfcBuilding"] = new(IfcDomain.Architecture, ClaimTier.Pin),
        ["IfcBuildingStorey"] = new(IfcDomain.Architecture, ClaimTier.Pin),

        ["IfcAudioVisualAppliance"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcAudioVisualApplianceType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcCableCarrierFitting"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcCableCarrierFittingType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcCableCarrierSegment"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcCableCarrierSegmentType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcCableFitting"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcCableFittingType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcCableSegment"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcCableSegmentType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcCommunicationsAppliance"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcCommunicationsApplianceType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricAppliance"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricApplianceType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricDistributionBoard"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricDistributionBoardType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricDistributionPoint"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricFlowStorageDevice"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricFlowStorageDeviceType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricFlowTreatmentDevice"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricFlowTreatmentDeviceType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricGenerator"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricGeneratorType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricHeaterType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricMotor"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricMotorType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricTimeControlType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricalCircuit"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcElectricalElement"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcJunctionBox"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcJunctionBoxType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcLamp"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcLampType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcLightFixture"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcLightFixtureType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcMotorConnection"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcMotorConnectionType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcOutlet"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcOutletType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcProtectiveDevice"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcProtectiveDeviceType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcProtectiveDeviceTrippingUnit"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcProtectiveDeviceTrippingUnitType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcSolarDevice"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcSolarDeviceType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcSwitchingDevice"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcSwitchingDeviceType"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcTransformer"] = new(IfcDomain.Electrical, ClaimTier.Pin),
        ["IfcTransformerType"] = new(IfcDomain.Electrical, ClaimTier.Pin),

        ["IfcDistributionChamberElement"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcDistributionChamberElementType"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcFlowMeter"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcFlowMeterType"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcInterceptor"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcInterceptorType"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcPipeFitting"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcPipeFittingType"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcPipeSegment"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcPipeSegmentType"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcPump"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcPumpType"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcSanitaryTerminal"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcSanitaryTerminalType"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcTank"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcTankType"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcValve"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcValveType"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcWasteTerminal"] = new(IfcDomain.Plumbing, ClaimTier.Pin),
        ["IfcWasteTerminalType"] = new(IfcDomain.Plumbing, ClaimTier.Pin),

        ["IfcBearing"] = new(IfcDomain.Structural, ClaimTier.Pin),
        ["IfcBearingType"] = new(IfcDomain.Structural, ClaimTier.Pin),
        ["IfcFooting"] = new(IfcDomain.Structural, ClaimTier.Pin),
        ["IfcFootingType"] = new(IfcDomain.Structural, ClaimTier.Pin),
        ["IfcCourse"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcCourseType"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcKerb"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcKerbType"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcMooringDevice"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcMooringDeviceType"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcNavigationElement"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcNavigationElementType"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcPavement"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcPavementType"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcRail"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcRailType"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcSignal"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcSignalType"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcTrackElement"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcTrackElementType"] = new(IfcDomain.Infrastructure, ClaimTier.Pin),
        ["IfcReinforcedSoil"] = new(IfcDomain.Geotechnical, ClaimTier.Pin),
    }.ToFrozenDictionary();
}
```

## [03]-[TAXONOMY_EMITTER]

- Owner: `IfcVocabularyEmitter` the OFFLINE BCL-reflection emitter whose output is the committed `Model/elements#IFC_CLASS` row region; `VocabularyRow` the row currency one render line commits. Span derivation is ATTRIBUTE-FIRST, index-corrected: class-level `[VersionAdded]` rides 85 roster types (`IfcBridge` IFC4X2, `IfcAlignment` IFC4X1, `IfcRoad` IFC4X3) and lowers release-exact through `Model/elements#IFC_CLASS` `ReleaseMap`, while the change index covers the unattributed majority at class grain and CORRECTS the token grain, where GG attributes diverge from schema truth both ways.
- Entry: `Emit(Assembly gg, IfcVocabulary vocabulary, FrozenSet<(string Entity, string Predefined)> stamps)` returns `Fin<string>` — ONE entrypoint owning the whole regeneration: reflect the roster, gate the published membership, run the census, resolve disciplines, source spans, audit, render. The eight overlay parameters it once carried are the `IfcVocabulary` the caller already holds, so the signature states what the emit needs and nothing the data reconstructs.
- Auto: `Emit` reflects the `IfcObjectDefinition` closure through `Op.Catch`, drops the unpublished draft surface at the index membership gate (a verdict, never a fault), runs `Census` over the reflected `[Obsolete]` marks, resolves domain inheritance through one QuikGraph DAG, lowers class and token release attributes through `ReleaseMap`, audits orphaned stamps and identifier collisions, then renders deterministic declarations under stable markers. `Census` and `Audit` ACCUMULATE on `Validation` and cross back to the `Fin` result through the folder's `Error.Many`, so a pin bump reads every disagreement in one run instead of the first: a stale overlay row no longer hides an orphan stamp, and one entity's missing window no longer hides another's missing mark. The committed rows carry capability; the marker carries no version or provenance fact.
- Packages: GeometryGymIFC_Core (the reflected attribute surface), QuikGraph (`BidirectionalGraph`, `TopologicalSort`, `BreadthFirstSearchAlgorithm`), `Rasm` (the kernel `Op`), Rasm.Element (the shared `ReleaseVersion`), LanguageExt.Core
- Growth: a new IFC release is one `ReleaseMap` row over a shared roster row that ranks itself; every other change is a regenerated change index or one `IfcOverlays` row — the emit body absorbs none of them.
- Boundary: the emitter never runs at runtime and its output is never hand-edited; the `Model/emitter#VOCABULARY_OVERLAYS` tiers are the sole hand surface. Sourcing eligibility from `!Type.IsAbstract` ALONE is the named defect — the CLR flag misreports EXPRESS abstractness on `IfcTransportationDevice`, and only the overlay carries that one. `IFC4X4_DRAFT` is excluded by law: the membership gate DROPS the draft schema at both grains and never faults, while a PUBLISHED member `ReleaseMap.Lower` omits FAILS the emit, the `?? ReleaseVersion.Ifc4X3Add2` / `GGRelease.IFC4X3_ADD2` silent fallbacks staying the retired forms. The stamp audit reads the Materials seed pairs as DATA (Materials never references `Rasm.Bim`). A per-entity hand base-chain walk beside the `DomainAtlas` DAG is the rejected second walk, and the per-root claim is the `BreadthFirstSearchAlgorithm` `DiscoverVertex` event fold — the all-vertex `TryFunc` path-probe sweep is deleted here as it is at the spatial view.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Reflection;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
using Rasm.Element.Graph;
using static LanguageExt.Prelude;
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;

namespace Rasm.Bim.Model;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct VocabularyRow(
    string Entity, IfcDomain Domain, ReleaseVersion IntroducedIn, Option<ReleaseVersion> RemovedIn,
    EgressEligibility Eligibility, Seq<PredefinedRow> Tokens);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class IfcVocabularyEmitter {
    public static Fin<string> Emit(
        Assembly gg, IfcVocabulary vocabulary, FrozenSet<(string Entity, string Predefined)> stamps) =>
        Try.lift(() => toSeq(gg.GetExportedTypes()
                .Where(t => typeof(IfcObjectDefinition).IsAssignableFrom(t) && !t.IsGenericType && vocabulary.Spans.ContainsKey(t.Name)))).Run().Bind(static inner => inner)
            .Bind(roster => (Census(roster, vocabulary)).ToFin().Map(_ => roster))
            .Bind(roster => DomainAtlas(roster, vocabulary.Claims)
                .Bind(domains => roster
                    .Traverse(entity => RowOf(entity, domains, vocabulary)).As()
                    .Bind(rows => (Audit(rows, stamps, vocabulary)).ToFin())
                    .Map(rows => Render(rows.OrderBy(static row => row.Entity, StringComparer.Ordinal)))));

    // --- [CENSUS]

    private static Validation<Error, Unit> Census(Seq<Type> roster, IfcVocabulary vocabulary) =>
        roster.Traverse(entity => Agrees(entity, vocabulary)).As().Map(static _ => unit);

    private static Validation<Error, Unit> Agrees(Type entity, IfcVocabulary vocabulary) {
        (Type Entity, bool Marked) census = (entity, entity.GetCustomAttribute<ObsoleteAttribute>() is not null);
        return Verdict(vocabulary, entity.Name).Match(
            None: () => census.Marked ? Refuse(census, "window") : Success<Error, Unit>(unit),
            Some: verdict => verdict.Switch(
                state: census,
                abstractSupertype:  static (s, _) => s.Marked ? Refuse(s, "window") : Success<Error, Unit>(unit),
                deprecatedSurvivor: static (s, _) => s.Marked ? Success<Error, Unit>(unit) : Refuse(s, "survivor-mark"),
                retired:            static (s, _) => s.Marked ? Success<Error, Unit>(unit) : Refuse(s, "mark"),
                retiredBare:        static (s, _) => s.Marked ? Refuse(s, "bare-mark") : Success<Error, Unit>(unit),
                ghost:              static (s, _) => Refuse(s, "ghost-shipped")));
    }

    private static Validation<Error, Unit> Refuse((Type Entity, bool Marked) census, string arm) =>
        Fail<Error, Unit>(new BimFault.Refused(BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "retirement-miss", arm, census.Entity.Name })));

    private static Option<OverlayVerdict> Verdict(IfcVocabulary vocabulary, string entity) =>
        Optional(vocabulary.Verdicts.GetValueOrDefault(entity));

    // --- [DOMAIN_ATLAS]

    private static Fin<HashMap<Type, IfcDomain>> DomainAtlas(
        Seq<Type> roster, FrozenDictionary<string, DomainClaim> claims) {
        BidirectionalGraph<Type, SEdge<Type>> dag = new(allowParallelEdges: false);
        dag.AddVertexRange(roster);
        roster.Iter(entity => Optional(entity.BaseType).Filter(dag.ContainsVertex).Iter(super => dag.AddEdge(new SEdge<Type>(super, entity))));
        HashMap<Type, IfcDomain> resolved = dag.TopologicalSort().AsIterable()
            .Choose(vertex => Optional(claims.GetValueOrDefault(vertex.Name)).Map(claim => (Vertex: vertex, Claim: claim)))
            .OrderBy(static row => row.Claim.Tier.Key)
            .Fold(HashMap<Type, IfcDomain>(), (map, row) => row.Claim.Tier.Apply(dag, map, row.Vertex, row.Claim.Domain));
        return roster.Filter(vertex => !resolved.ContainsKey(vertex)) is { IsEmpty: false } uncovered
            ? Fin.Fail<HashMap<Type, IfcDomain>>(new BimFault.Refused(BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "domain-root-miss", string.Join(',', uncovered.Map(static v => v.Name)) })))
            : Fin.Succ(resolved);
    }

    internal static HashMap<Type, IfcDomain> Reach(
        BidirectionalGraph<Type, SEdge<Type>> dag, HashMap<Type, IfcDomain> map, Type root, IfcDomain domain) {
        BreadthFirstSearchAlgorithm<Type, SEdge<Type>> search = new(dag);
        HashMap<Type, IfcDomain> claimed = map;
        search.DiscoverVertex += vertex => claimed = claimed.AddOrUpdate(vertex, domain);
        search.Compute(root);
        return claimed;
    }

    // --- [ROW_SOURCING]

    private static Fin<VocabularyRow> RowOf(Type entity, HashMap<Type, IfcDomain> domains, IfcVocabulary vocabulary) =>
        Introduced(entity.Name, entity.GetCustomAttribute<VersionAddedAttribute>(), vocabulary, inherit: None)
            .Bind(introduced => Tokens(entity, introduced, vocabulary)
                .Map(tokens => new VocabularyRow(
                    entity.Name, domains[entity], introduced, RemovedIn(Verdict(vocabulary, entity.Name)),
                    Eligibility(entity, vocabulary), tokens)));

    private static Option<ReleaseVersion> RemovedIn(Option<OverlayVerdict> verdict) =>
        verdict.Bind(static v => v.Switch(
            abstractSupertype:  static _ => Option<ReleaseVersion>.None,
            deprecatedSurvivor: static _ => Option<ReleaseVersion>.None,
            retired:            static r => Some(r.RemovedIn),
            retiredBare:        static r => Some(r.RemovedIn),
            ghost:              static g => Some(g.RemovedIn)));

    private static EgressEligibility Eligibility(Type entity, IfcVocabulary vocabulary) =>
        entity.IsAbstract || Verdict(vocabulary, entity.Name).Exists(static v => v.IsAbstractSupertype)
            ? EgressEligibility.Vocabulary
            : EgressEligibility.Authorable;

    private static Option<Type> TokenEnum(Type? entity) =>
        Optional(entity).Bind(t =>
            Optional(t.GetProperty(nameof(IfcWall.PredefinedType), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Map(static property => property.PropertyType).Filter(static pt => pt.IsEnum)
            | TokenEnum(t.BaseType));

    private static Fin<Seq<PredefinedRow>> Tokens(Type entity, ReleaseVersion classIntroduced, IfcVocabulary vocabulary) =>
        TokenEnum(entity).Match(
            None: () => Fin.Succ(Seq<PredefinedRow>()),
            Some: tokenEnum => toSeq(Enum.GetNames(tokenEnum))
                .Filter(name => name is not ("NOTDEFINED" or "USERDEFINED") && vocabulary.Spans.ContainsKey(string.Concat(entity.Name, ".", name)))
                .Traverse(name => Introduced(
                        string.Concat(entity.Name, ".", name), tokenEnum.GetField(name)?.GetCustomAttribute<VersionAddedAttribute>(),
                        vocabulary, inherit: Some(classIntroduced))
                    .Map(introduced => new PredefinedRow(name, introduced))).As());

    private static Fin<ReleaseVersion> Introduced(
        string indexKey, VersionAddedAttribute? attribute, IfcVocabulary vocabulary, Option<ReleaseVersion> inherit) =>
        (vocabulary.Spans.TryGetValue(indexKey, out Option<ReleaseVersion> pinned) ? pinned : None).Match(
            Some: Fin.Succ,
            None: () => attribute is { Release: var release }
                ? ReleaseMap.Lower.TryGetValue(release, out ReleaseVersion? lowered) && lowered is { } low
                    ? Fin.Succ(low)
                    : Fin.Fail<ReleaseVersion>(new BimFault.Refused(BimScope.Model, BimReason.Codec, string.Join(':', new object?[] { "release-unmapped", indexKey, release })))
                : inherit.ToFin(new BimFault.Refused(BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "introduction-miss", indexKey }))));

    // --- [GATE_ZERO]

    private static Validation<Error, Seq<VocabularyRow>> Audit(
        Seq<VocabularyRow> rows, FrozenSet<(string Entity, string Predefined)> stamps, IfcVocabulary vocabulary) {
        HashMap<string, VocabularyRow> byEntity = rows.Fold(HashMap<string, VocabularyRow>(), static (map, row) => map.AddOrUpdate(row.Entity, row));
        return (Duplicated(rows), Stale(byEntity, vocabulary), Orphans(byEntity, stamps), Collided(rows))
            .Apply(static (_, _, _, _) => unit).As().Map(_ => rows);
    }

    private static Validation<Error, Unit> Duplicated(Seq<VocabularyRow> rows) =>
        Reported("entity-duplicated", toSeq(rows.GroupBy(static row => row.Entity, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1).Select(static group => group.Key)));

    private static Validation<Error, Unit> Stale(HashMap<string, VocabularyRow> byEntity, IfcVocabulary vocabulary) =>
        Reported("overlay-stale", (toSeq(vocabulary.Spans).Filter(static row => row.Value.IsSome).Map(static row => row.Key)
                + toSeq(vocabulary.Verdicts.Keys).Filter(name => !Verdict(vocabulary, name).Exists(static v => v.IsGhost))
                + toSeq(vocabulary.Claims.Keys))
            .Filter(overlay => overlay.Split('.') is var half && byEntity.Find(half[0]).Match(
                Some: row => half.Length > 1 && !row.Tokens.Exists(t => t.Token == half[1]),
                None: static () => true)));

    private static Validation<Error, Unit> Orphans(
        HashMap<string, VocabularyRow> byEntity, FrozenSet<(string Entity, string Predefined)> stamps) =>
        Reported("stamp-orphan", stamps.AsIterable().ToSeq()
            .Filter(stamp => byEntity.Find(stamp.Entity).Match(
                Some: row => stamp.Predefined is not ("" or "NOTDEFINED" or "USERDEFINED")
                    && !row.Tokens.IsEmpty && !row.Tokens.Exists(t => t.Token == stamp.Predefined),
                None: static () => true))
            .Map(static stamp => $"{stamp.Entity}.{stamp.Predefined}"));

    private static Validation<Error, Unit> Collided(Seq<VocabularyRow> rows) =>
        Reported("identifier-collision", toSeq(rows.Map(Identifier).GroupBy(static id => id, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1).Select(static group => group.Key)));

    private static Validation<Error, Unit> Reported(string arm, Seq<string> subjects) =>
        subjects.IsEmpty ? Success<Error, Unit>(unit) : Fail<Error, Unit>(new BimFault.Refused(BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "vocabulary-audit", arm, string.Join(',', subjects) })));

    // --- [RENDER]

    private static string Identifier(VocabularyRow row) => row.Entity["Ifc".Length..];

    private static string Render(IEnumerable<VocabularyRow> rows) =>
        toSeq(rows)
            .Map(static row =>
                $"    public static readonly IfcClass {Identifier(row)} = new(\"{row.Entity}\", IfcDomain.{row.Domain}, {Window(row)}, EgressEligibility.{row.Eligibility}, {Tokens(row.Tokens)});")
            .Fold("    // <generated-rows>", static (region, line) => string.Concat(region, "\n", line))
        + "\n    // <end generated-rows>";

    private static string Window(VocabularyRow row) => row.RemovedIn.Match(
        Some: removed => $"new SchemaSpan(ReleaseVersion.{row.IntroducedIn.Key}, Some(ReleaseVersion.{removed.Key}))",
        None: () => $"IfcSchema.Of(ReleaseVersion.{row.IntroducedIn.Key})");

    private static string Tokens(Seq<PredefinedRow> tokens) => tokens.IsEmpty
        ? "Seq<PredefinedRow>()"
        : $"Seq({string.Join(", ", tokens.OrderBy(static t => t.Token, StringComparer.Ordinal).Select(static t => $"new PredefinedRow(\"{t.Token}\", ReleaseVersion.{t.IntroducedIn.Key})"))})";
}
```

## [04]-[REGENERATION]

- Owner: `VocabularyRegeneration` the design-time runner a GeometryGym pin bump or a new IFC release executes — the ONE producer of the committed region, so the generator stops being law without a producer; `RegenerationRequest` the four admitted seeds (pinned assembly, EXPRESS change index, Materials stamp seeds, the committed source file); `EmittedRegion` the assembly, row count, and region digest one run commits.
- Entry: `VocabularyRegeneration.Run(RegenerationRequest request)` returns `Fin<EmittedRegion>` and is the whole regeneration; `Main(string[] args)` is the process boundary the `Codegen` configuration starts, projecting that result onto an exit code. Invocation is `dotnet run --project libs/dotnet/Rasm.Bim/Rasm.Bim.csproj -c Codegen -- <assembly> <index> <stamps> <source>`; the configuration condition is the only thing it adds to the shipped library, whose `Debug`/`Release` output is unchanged.
- Auto: the run admits its four seeds at the boundary, mints the `IfcVocabulary` through `IfcOverlays.Vocabulary`, runs `Emit`, then SPLICES the returned region between the committed marker pair — a source whose markers are absent, out of order, or duplicated refuses `region-marker-miss` rather than writing a region into an unknown position. `EmittedRegion.Region` content-keys the emitted region through the kernel `ContentHash.Of` so two runs over one pin prove identical without a text diff.
- Packages: `Rasm` (the kernel `Op`, `ContentHash`), Rasm.Element (the shared `ReleaseVersion` the index admits to), LanguageExt.Core, BCL inbox
- Growth: a new seed is one `RegenerationRequest` column and one admission; the emit body never learns about a file.
- Boundary: the exit status is BINARY — a POSIX wait status keeps the low eight bits of what a process returns, and `FaultBand.Bim.Code(offset)` is 2600-decade, so `2600 & 0xFF` is 40 and a run returning its own band code reports an unrelated status to every shell and CI gate reading it; the STATUS carries the verdict (0 written, 1 refused) and the STREAM carries the identity. The runner writes ONE file and only between the markers — regenerating a whole source file, emitting a sidecar, or running at model time are each the deleted form; the committed table stays the system of record.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Reflection;
using LanguageExt;
using Rasm.Domain;
using static LanguageExt.Prelude;
using ReleaseVersion = Rasm.Element.Graph.ReleaseVersion;

namespace Rasm.Bim.Model;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct RegenerationRequest(string Assembly, string Index, string Stamps, string Source);

public readonly record struct EmittedRegion(string Assembly, int Rows, UInt128 Region);

// --- [ENTRY] ---------------------------------------------------------------------------
public static class VocabularyRegeneration {

    private const string Open = "    // <generated-rows>";
    private const string Close = "    // <end generated-rows>";

    public static int Main(string[] args) =>
        Admit(args).Bind(request => Run(request, Key)).Match(
            Succ: static emitted => {
                Console.Out.WriteLine($"{emitted.Rows} rows {ContentHash.Hex(emitted.Region)} {emitted.Assembly}");
                return 0;
            },
            Fail: static error => {
                Console.Error.WriteLine($"{error.Code} {error.Message}");
                return 1;
            });

    public static Fin<EmittedRegion> Run(RegenerationRequest request) =>
        from assembly in Try.lift(() => Assembly.LoadFrom(request.Assembly)).Run().Bind(static inner => inner)
        from spans in ChangeIndex(request.Index)
        from stamps in StampSeeds(request.Stamps)
        from source in Text(request.Source)
        from region in IfcVocabularyEmitter.Emit(assembly, IfcOverlays.Vocabulary(spans), stamps)
        from written in Splice(request.Source, source, region)
        select new EmittedRegion(
            assembly.GetName().Name ?? request.Assembly,
            written.Count(c => c == '\n') - 1,
            ContentHash.Of(region, static (text, writer) => writer.String(text)));

    private static Fin<string> Splice(string path, string source, string region) =>
        (source.IndexOf(Open, StringComparison.Ordinal), source.LastIndexOf(Open, StringComparison.Ordinal),
         source.IndexOf(Close, StringComparison.Ordinal), source.LastIndexOf(Close, StringComparison.Ordinal)) switch {
            (var open, var lastOpen, var close, var lastClose) when open >= 0 && open == lastOpen && close > open && close == lastClose =>
                Try.lift(() => {
                    string spliced = string.Concat(source[..open], region, source[(close + Close.Length)..]);
                    File.WriteAllText(path, spliced);
                    return region;
                }).Run().Bind(static inner => inner),
            _ => Fin.Fail<string>(new BimFault.Refused(BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "vocabulary-audit", "region-marker-miss", path }))),
        };

    // --- [ADMISSION]

    private static Fin<RegenerationRequest> Admit(string[] args) =>
        args is [var assembly, var index, var stamps, var source]
            ? Fin.Succ(new RegenerationRequest(assembly, index, stamps, source))
            : Fin.Fail<RegenerationRequest>(new BimFault.Refused(Key, BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "vocabulary-audit", "argument-arity", string.Join(' ', args) })));

    private static Fin<FrozenDictionary<string, Option<ReleaseVersion>>> ChangeIndex(string path) =>
        Rows(path).Bind(rows => rows
            .Traverse(row => row.Split('\t') switch {
                [var name] => Fin.Succ((Name: name, Release: Option<ReleaseVersion>.None)),
                [var name, var release] => ReleaseVersion.TryGet(release, out ReleaseVersion? row) && row is { } known
                    ? Fin.Succ((Name: name, Release: Some(known)))
                    : Fin.Fail<(string, Option<ReleaseVersion>)>(new BimFault.Refused(BimScope.Model, BimReason.Codec, string.Join(':', new object?[] { "release-unmapped", name, release }))),
                var malformed => Fin.Fail<(string, Option<ReleaseVersion>)>(new BimFault.Refused(BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "vocabulary-audit", "index-malformed", string.Join('\t', malformed) }))),
            }).As()
            .Map(static admitted => admitted.ToFrozenDictionary(static row => row.Name, static row => row.Release)));

    private static Fin<FrozenSet<(string Entity, string Predefined)>> StampSeeds(string path) =>
        Rows(path).Bind(rows => rows
            .Traverse(row => row.Split('\t') switch {
                [var entity, var predefined] => Fin.Succ((Entity: entity, Predefined: predefined)),
                var malformed => Fin.Fail<(string, string)>(new BimFault.Refused(BimScope.Model, BimReason.Unmapped, string.Join(':', new object?[] { "vocabulary-audit", "stamp-malformed", string.Join('\t', malformed) }))),
            }).As()
            .Map(static admitted => admitted.ToFrozenSet()));

    private static Fin<Seq<string>> Rows(string path) =>
        Text(path).Map(static text => toSeq(text.Split('\n')).Map(static row => row.Trim()).Filter(static row => row.Length > 0));

    private static Fin<string> Text(string path) =>
        Try.lift(() => File.ReadAllText(path)).Run().Bind(static inner => inner);
}
```

## [05]-[RESEARCH]

(none)
