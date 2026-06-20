# [BIM_ZONE_GRAPH]

The host-neutral cross-cutting grouping overlay: one `BimZone` record carrying a closed `BimZoneKind` `[SmartEnum<string>]` partition over the IFC `IfcGroup`/`IfcSystem`/`IfcBuildingSystem`/`IfcZone`/`IfcSpatialZone`/`IfcDistributionSystem`/`IfcStructuralLoadGroup` grouping family and a closed `ZoneAssignment` `[Union]` (`AssignsToGroup`/`ReferencedInSpatialStructure`) discriminating the two many-to-many membership relationships the IFC schema models — `IfcRelAssignsToGroup` (an element/space logically assigned into a non-spatial group) and `IfcRelReferencedInSpatialStructure` (an element referenced into a spatial structure it is not contained in) — never the single-parent containment the `Model/structure#ASSEMBLY_TREE` `BimAssembly` tree already owns. A fire compartment spanning three storeys, a thermal zone aggregating spaces across a building, an HVAC distribution system threading every air terminal, and a structural load group binding a set of members each carry their full member set on one record, projected from the GeometryGym `IfcGroup.IsGroupedBy` `IfcRelAssignsToGroup` member-set surface the `Exchange/import#IMPORT_RAIL` fold already flattens into the thin `IfcSemanticModel.ZoneRow(GlobalId, EntityType, Name, PredefinedType, AssignedGlobalIds)` projection. The overlay is HOST-NEUTRAL — it joins elements by stable `GlobalId` and never carries a RhinoCommon `Layer`/`InstanceDefinition`, the named seam violation the assembly tree also rejects — and is the orthogonal companion to the containment hierarchy: an element sits in exactly one spatial container yet belongs to arbitrarily many zones, so the grouping graph is the many-to-many algebra the single-parent tree structurally cannot express. The typed `Map<string, Seq<string>>` zone-membership index this owner produces through `BimZone.IndexOf` is the `Model/elements#ELEMENT_MODEL` `BimModel.Zones` column `BimModel.Project` populates, the `Model/query#ELEMENT_SET` `ByZone` predicate arm joins by `ZoneGlobalId`, and the typed `Seq<BimZone>` projection the `Model/structural#ANALYSIS_MODEL` thermal/load-group selection and the `Model/systems#SYSTEM_TRACE` MEP-system grouping read by reference; a zone rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` via `.ToError()`, never a new fault family.

## [01]-[INDEX]

- [01]-[ZONE_GRAPH]: `BimZone` record, the `BimZoneKind` `[SmartEnum<string>]` grouping-family partition, the `ZoneAssignment` `[Union]` (`AssignsToGroup`/`ReferencedInSpatialStructure`), the `ZoneProjection.Project` typed-zone fold, and the `BimZone.IndexOf` membership-index fold the `BimModel.Zones` column carries.

## [02]-[ZONE_GRAPH]

- Owner: `BimZone` the single host-neutral grouping record carrying the stable group `GlobalId`, the `BimZoneKind` discriminant, the group name, the admitted `PredefinedType` sub-kind, the `ZoneAssignment` membership-relationship case, and the assigned member `GlobalId` set; `BimZoneKind` the `[SmartEnum<string>]` closed grouping-family vocabulary keyed on the IFC group/zone entity-type string, each row carrying its `IfcDomain` and its `IsSpatial` flag distinguishing the spatial-reference overlay (`IfcSpatialZone`) from the logical-assignment overlay (`IfcGroup`/`IfcZone`/`IfcSystem`); `ZoneAssignment` the closed `[Union]` discriminating the two many-to-many membership relationships the IFC schema models; `ZoneProjection` the static fold over the thin `IfcSemanticModel.ZoneRow` family the import rail flattens, and `BimZone.IndexOf` the membership-index fold the `BimModel` element-collection owner carries.
- Cases: `ZoneAssignment` arms `AssignsToGroup` (the `IfcRelAssignsToGroup` logical assignment — an element/space assigned into a non-spatial `IfcGroup`/`IfcSystem`/`IfcZone`/`IfcStructuralLoadGroup`, carrying the `RelatingGroupId` and the assigned `MemberIds`) · `ReferencedInSpatialStructure` (the `IfcRelReferencedInSpatialStructure` cross-containment reference — an element referenced into an `IfcSpatialStructureElement` it is not contained in, carrying the `RelatingStructureId` and the referenced `ElementIds`) (2); `BimZoneKind` rows `Group`/`System`/`BuildingSystem`/`Zone`/`SpatialZone`/`DistributionSystem`/`StructuralLoadGroup` (7) each frozen with its `IfcDomain`, its `IsSpatial` flag, and its `ValidPredefined` valid-token set, the `SpatialZone` row the sole `IsSpatial = true` arm so a fire/thermal `IfcSpatialZone` reference reads the referenced spatial-structure overlay rather than the logical-assignment overlay, and only the `SpatialZone` (`IfcSpatialZoneTypeEnum`), `DistributionSystem` (`IfcDistributionSystemEnum`), and `StructuralLoadGroup` (`IfcLoadGroupTypeEnum`) rows carry a non-empty `ValidPredefined` set — the four logical-group rows carry the empty set because the IFC schema gives `IfcGroup`/`IfcSystem`/`IfcBuildingSystem`/`IfcZone` no predefined enum.
- Entry: `ZoneProjection.Project(IfcSemanticModel.ZoneRow row)` projects one flattened zone row into the typed `BimZone` — resolving the `BimZoneKind` from the row's `EntityType` entity string, admitting the row's predefined token, selecting the `ZoneAssignment` case from the resolved kind's `IsSpatial` flag, and carrying the `AssignedGlobalIds` member set — `Fin<T>` aborting on an unmapped grouping class (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) lowered with `.ToError()`; `ZoneProjection.ProjectAll(Seq<IfcSemanticModel.ZoneRow> rows)` traverses the row family into the typed `Seq<BimZone>` the analysis/systems consumers read, and `BimZone.IndexOf(Seq<IfcSemanticModel.ZoneRow> rows)` folds the same rows into the `Map<string, Seq<string>>` zone-membership index keyed by group `GlobalId` the `Model/elements#ELEMENT_MODEL` `BimModel.Zones` column carries — total, pure, no rail, since the index discards an unmapped class rather than aborting the whole model projection on a single unknown group.
- Auto: `Project` reads the `ZoneRow.EntityType` IFC group/zone entity string through `BimZoneKind.TryGet`, admits the `ZoneRow.PredefinedType` token through the kind-keyed `BimZoneKind.AdmitPredefined` valid-set fold (the shared predefined-admission idiom mirroring `Model/elements#ELEMENT_MODEL` `IfcClass.AdmitPredefined`, the empty/`NOTDEFINED`/`USERDEFINED` token falling to `PredefinedType.NotDefined` and a real token validating against the row's `ValidPredefined` set), and builds the `ZoneAssignment` case from the kind's `IsSpatial` flag — a `SpatialZone` row folds to `ReferencedInSpatialStructure(GlobalId, AssignedGlobalIds)` and every logical-grouping row folds to `AssignsToGroup(GlobalId, AssignedGlobalIds)` — so the two membership modalities discriminate on the grouping family rather than a parallel relationship store; `BimZone.IndexOf` folds the row family into the membership map by projecting each successfully-resolved row to its `(GlobalId, AssignedGlobalIds)` entry and discarding the rare unmapped grouping class through `Choose`, so a model carrying an unknown extension group still indexes every known zone; `BimZone.Members` reads the `ZoneAssignment` member set through the union `Switch` so a consumer reads the assigned/referenced element set without re-discriminating the case, and `BimZone.IsSpatial` projects the kind flag so a thermal/fire-compartment query selects the spatial-reference overlay arm.
- Receipt: the typed `Seq<BimZone>` is the grouping evidence the `Model/structural#ANALYSIS_MODEL` thermal-zone/load-group selection and the `Model/systems#SYSTEM_TRACE` MEP distribution-system grouping read by reference, and the `Map<string, Seq<string>>` membership index is the join surface the `Model/query#ELEMENT_SET` `ByZone` arm folds — a fire-compartment / thermal-zone / program-area selection reads `model.Zones.Find(zoneId)` returning the member `Seq<string>` the predicate `Contains`-tests against each element's `GlobalId`, the grouping graph the single-parent `BySpatialContainer` containment arm structurally cannot express; the `ZoneAssignment` case is the typed membership evidence on the record, never a stringly-typed relationship-name column.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new grouping family is one `BimZoneKind` row carrying its domain and spatial flag (an `IfcStructuralResultGroup` or an IFC4.3 infrastructure grouping rides the same projection on one new row); a new membership relationship is one `ZoneAssignment` union arm — the Thinktecture generated total `Switch` breaks every `Members` site at compile time until the arm is added, so a missing modality is a build error not a silent fallthrough; a new per-zone binding is one column on `BimZone` projected from the existing `ZoneRow` family; never a per-zone-kind record, never a parallel `IfcGroup`/`IfcZone`/`IfcSystem` type family, and never a `GetFireZones`/`GetByZoneKind` operation family.
- Boundary: `BimZone` is ONE record discriminated by the `BimZoneKind` row data and the `ZoneAssignment` union — a `FireZone`/`ThermalZone`/`MepSystem`/`LoadGroup` class family or seven sibling grouping types is the deleted form mirroring the no-per-element-class law at `Model/elements#ELEMENT_MODEL`; the predefined sub-kind is admitted once at projection through the kind-keyed `BimZoneKind.AdmitPredefined` valid-set fold against the row's `ValidPredefined` set — the same canonical admission idiom the `Model/elements#ELEMENT_MODEL` `IfcClass.AdmitPredefined` owns, so a `SpatialZone` `FIRESAFETY`/`OCCUPANCY` token validates against the real `IfcSpatialZoneTypeEnum` valid set rather than a bare `ToUpperInvariant` pass-through, and an inline `Trim`/`ToUpper` ternary or a per-call predefined regex on the projection is the named deleted form; the four logical-group rows carry the empty `ValidPredefined` set and the import rail extracts a predefined token only for the spatial-zone arm (`IfcZone`/`IfcSystem`/`IfcGroup` have no `IfcZoneTypeEnum` in the GeometryGym schema), so the generic-group `ZoneRow.PredefinedType` stays empty and admits to `PredefinedType.NotDefined`; the two many-to-many membership relationships are the closed `ZoneAssignment` union, never a per-relationship class and never an `Option<ZoneAssignment>` escape; the GeometryGym `IfcGroup`/`IfcSystem`/`IfcBuildingSystem`/`IfcZone`/`IfcSpatialZone`/`IfcDistributionSystem`/`IfcStructuralLoadGroup` and `IfcRelAssignsToGroup`/`IfcRelReferencedInSpatialStructure` surface (`.api/api-geometrygym-ifc` grouping-zone family rows 1-8, relationship row 16) is consumed through the flattened `Exchange/import#IMPORT_RAIL` `IfcSemanticModel.ZoneRow(GlobalId, EntityType, Name, PredefinedType, AssignedGlobalIds)` projection the import fold already builds from `IfcGroup.IsGroupedBy.RelatedObjects.GlobalId`, so a second hand-rolled grouping reader is the deleted form; this overlay is the many-to-many companion to the single-parent `Model/structure#ASSEMBLY_TREE` `BimAssembly` containment tree — an element's `BimElement.SpatialContainerId` is its one containment parent and its zone memberships are arbitrarily many, the two coexisting and never collapsed; the overlay joins by stable `GlobalId` and a RhinoCommon `Layer`/`InstanceDefinition` binding is the named seam violation; the `BimModel.Zones` `Map<string, Seq<string>>` index is the membership join the `Model/query#ELEMENT_SET` `ByZone` arm reads and the `analysis`/`systems` consumers read the typed `Seq<BimZone>` projection, never re-deriving the grouping graph; a grouping rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()` and a bare `Fin.Fail` without that lowering is the named seam defect.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class BimZoneKind {
    public static readonly BimZoneKind Group              = new("IfcGroup", IfcDomain.Architecture, IsSpatial: false, Seq<string>());
    public static readonly BimZoneKind System             = new("IfcSystem", IfcDomain.HvacFire, IsSpatial: false, Seq<string>());
    public static readonly BimZoneKind BuildingSystem     = new("IfcBuildingSystem", IfcDomain.Architecture, IsSpatial: false, Seq<string>());
    public static readonly BimZoneKind Zone               = new("IfcZone", IfcDomain.Architecture, IsSpatial: false, Seq<string>());
    public static readonly BimZoneKind SpatialZone        = new("IfcSpatialZone", IfcDomain.Architecture, IsSpatial: true,
        Seq("CONSTRUCTION", "FIRESAFETY", "LIGHTING", "OCCUPANCY", "SECURITY", "THERMAL", "VENTILATION", "TRANSPORT", "RESERVATION", "INTERFERENCE", "MAPPEDZONE", "TESTEDZONE", "COMPARTMENT", "ANNULARGAP", "CLEARANCE", "INSTALLATION", "INTERIOR", "INVERT", "LINING", "SERVICE"));
    public static readonly BimZoneKind DistributionSystem = new("IfcDistributionSystem", IfcDomain.HvacFire, IsSpatial: false,
        Seq("AIRCONDITIONING", "AUDIOVISUAL", "CHEMICAL", "CHILLEDWATER", "COMMUNICATION", "COMPRESSEDAIR", "CONDENSERWATER", "CONTROL", "CONVEYING", "DATA", "DISPOSAL", "DOMESTICCOLDWATER", "DOMESTICHOTWATER", "DRAINAGE", "EARTHING", "ELECTRICAL", "ELECTROACOUSTIC", "EXHAUST", "FIREPROTECTION", "FUEL", "GAS", "HAZARDOUS", "HEATING", "LIGHTING", "LIGHTNINGPROTECTION", "MUNICIPALSOLIDWASTE", "OIL", "OPERATIONAL", "POWERGENERATION", "RAINWATER", "REFRIGERATION", "SECURITY", "SEWAGE", "SIGNAL", "STORMWATER", "TELEPHONE", "TV", "VACUUM", "VENT", "VENTILATION", "WASTEWATER", "WATERSUPPLY", "CATENARY_SYSTEM", "OVERHEAD_CONTACTLINE_SYSTEM", "RETURN_CIRCUIT", "FIXEDTRANSMISSIONNETWORK", "OPERATIONALTELEPHONYSYSTEM", "MOBILENETWORK", "MONITORINGSYSTEM", "SAFETY"));
    public static readonly BimZoneKind StructuralLoadGroup= new("IfcStructuralLoadGroup", IfcDomain.Structural, IsSpatial: false,
        Seq("LOAD_GROUP", "LOAD_CASE", "LOAD_COMBINATION", "LOAD_COMBINATION_GROUP"));

    public IfcDomain Domain { get; }
    public bool IsSpatial { get; }
    public Seq<string> ValidPredefined { get; }

    public Fin<PredefinedType> AdmitPredefined(string token) =>
        token.Trim().ToUpperInvariant() switch {
            "" or "NOTDEFINED" or "USERDEFINED"                                     => Fin.Succ(PredefinedType.NotDefined),
            var value when ValidPredefined.IsEmpty || ValidPredefined.Contains(value) => Fin.Succ(PredefinedType.Create(value)),
            var value                                                               => Fin.Fail<PredefinedType>(new BimFault.UnmappedClass($"zone-predefined-reject:{Key}:{value}").ToError()),
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record ZoneAssignment {
    partial record AssignsToGroup(string RelatingGroupId, Seq<string> MemberIds);
    partial record ReferencedInSpatialStructure(string RelatingStructureId, Seq<string> ElementIds);

    public string RelatingId => Switch(
        assignsToGroup:               static a => a.RelatingGroupId,
        referencedInSpatialStructure: static a => a.RelatingStructureId);

    public Seq<string> Members => Switch(
        assignsToGroup:               static a => a.MemberIds,
        referencedInSpatialStructure: static a => a.ElementIds);
}

public sealed record BimZone(
    string GlobalId,
    BimZoneKind Kind,
    string Name,
    PredefinedType Predefined,
    ZoneAssignment Assignment) {
    public bool IsSpatial => Kind.IsSpatial;
    public Seq<string> Members => Assignment.Members;
    public IfcDomain Domain => Kind.Domain;

    public static Map<string, Seq<string>> IndexOf(Seq<IfcSemanticModel.ZoneRow> rows) =>
        rows.Choose(static row => BimZoneKind.TryGet(row.EntityType)
                .Map(_ => (row.GlobalId, row.AssignedGlobalIds)))
            .Fold(Map<string, Seq<string>>(), static (index, entry) => index.AddOrUpdate(entry.Item1, entry.Item2));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ZoneProjection {
    public static Fin<BimZone> Project(IfcSemanticModel.ZoneRow row) =>
        BimZoneKind.TryGet(row.EntityType)
            .ToFin(new BimFault.UnmappedClass($"zone-class-miss:{row.EntityType}").ToError())
            .Bind(kind => kind.AdmitPredefined(row.PredefinedType)
                .Map(predefined => new BimZone(
                    row.GlobalId,
                    kind,
                    row.Name,
                    predefined,
                    kind.IsSpatial
                        ? new ZoneAssignment.ReferencedInSpatialStructure(row.GlobalId, row.AssignedGlobalIds)
                        : new ZoneAssignment.AssignsToGroup(row.GlobalId, row.AssignedGlobalIds))));

    public static Fin<Seq<BimZone>> ProjectAll(Seq<IfcSemanticModel.ZoneRow> rows) =>
        rows.TraverseM(Project).As();
}
```

## [03]-[RESEARCH]

- [ZONE_PROJECTION]: the `ZoneProjection.Project`/`BimZone.IndexOf` folds over the flattened `IfcSemanticModel.ZoneRow` family and the `BimZoneKind` closed-vocabulary case list ground against the GeometryGym grouping-zone surface (`.api/api-geometrygym-ifc` grouping-zone-distribution-system family rows 1-8: `IfcGroup`/`IfcSystem`/`IfcBuildingSystem`/`IfcDistributionSystem`/`IfcZone`/`IfcSpatialZone`, the `IfcRelReferencedInSpatialStructure` overlay row 7, and the `IfcRelAssignsToGroup` assignment row 16) so the seven `BimZoneKind` rows and the `IfcGroup.IsGroupedBy` member-set flattening confirm against the real grouping family; the `Exchange/import#IMPORT_RAIL` `Semantic` fold populates the `ZoneRow.AssignedGlobalIds` member set from `IfcGroup.IsGroupedBy.SelectMany(rel => rel.RelatedObjects.Select(o => o.GlobalId))` (`.api/api-geometrygym-ifc` traversal row 7 `IfcGroup.IsGroupedBy → IfcRelAssignsToGroup` set), so the `AssignsToGroup` member ids the typed projection carries match the relationship's related-object set the import fold already flattens; the three `BimZoneKind.ValidPredefined` sets are decompile-verified against the live GeometryGym 25.7.30 enums (`.api/api-geometrygym-ifc` rows 07/08/14): `IfcSpatialZoneTypeEnum` (`CONSTRUCTION`/`FIRESAFETY`/`LIGHTING`/`OCCUPANCY`/`SECURITY`/`THERMAL`/`VENTILATION`/`TRANSPORT`/`RESERVATION`/`INTERFERENCE`/`MAPPEDZONE`/`TESTEDZONE`/`COMPARTMENT`/`ANNULARGAP`/`CLEARANCE`/`INSTALLATION`/`INTERIOR`/`INVERT`/`LINING`/`SERVICE`), `IfcDistributionSystemEnum` (the 50-row air/water/electrical/communication/IFC4.3-infrastructure/IFC4.4-safety set), and `IfcLoadGroupTypeEnum` (`LOAD_GROUP`/`LOAD_CASE`/`LOAD_COMBINATION`/`LOAD_COMBINATION_GROUP`), each set excluding the `NOTDEFINED`/`USERDEFINED` sentinels the `AdmitPredefined` fold maps to `PredefinedType.NotDefined`; `IfcZone`/`IfcSystem`/`IfcGroup`/`IfcBuildingSystem` carry NO `PredefinedType` (there is no `IfcZoneTypeEnum` in GeometryGym 25.7.30), so those four `BimZoneKind` rows carry the empty `ValidPredefined` set, the import rail extracts the predefined token only for the spatial-zone arm, and the generic logical-group `ZoneRow.PredefinedType` stays empty and admits to `PredefinedType.NotDefined` through the shared `AdmitPredefined` idiom rather than the prior inline `Trim`/`ToUpper` ternary.
- [MEMBERSHIP_INDEX]: the `BimZone.IndexOf` `Map<string, Seq<string>>` membership index the `Model/elements#ELEMENT_MODEL` `BimModel.Project` calls as `BimZone.IndexOf(semantic.Zones)` to populate the `BimModel.Zones` column grounds against the `Model/query#ELEMENT_SET` `ByZone` arm contract — the arm reads `model.Zones.Find(p.ZoneGlobalId)` returning the `Option<Seq<string>>` member set and `Contains`-tests the element's `GlobalId` — so the index key (group `GlobalId`) and value (member `GlobalId` set) match the join the predicate folds; the `Choose`-then-`Fold` index build discards an unmapped grouping class rather than aborting the whole model projection, confirming the total no-rail posture the `BimModel.Project` `Map<string, Seq<string>>` column requires against the `IfcClass`-aborting product/spatial folds that do rail.
- [GROUPING_OVERLAY_ORTHOGONALITY]: the many-to-many grouping overlay's orthogonality to the single-parent `Model/structure#ASSEMBLY_TREE` `BimAssembly` containment tree confirms against the `Model/elements#ELEMENT_MODEL` `BimElement.SpatialContainerId` `Option<string>` single-parent column — an element carries exactly one containment parent and arbitrarily many zone memberships joined by the `BimModel.Zones` index — so the `ByZone` arm and the `BySpatialContainer` arm select disjoint membership semantics over the same element collection, never a re-derivation of the containment tree as a grouping graph; the `Model/structural#ANALYSIS_MODEL` thermal-zone/load-group selection and the `Model/systems#SYSTEM_TRACE` MEP distribution-system grouping read the typed `Seq<BimZone>` `ProjectAll` projection by reference, confirming the cross-page contract that the grouping owner produces the typed zone family those consumers fold rather than re-projecting the `ZoneRow` family per consumer.
