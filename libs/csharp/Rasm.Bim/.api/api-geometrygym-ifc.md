# [RASM_BIM_API_GEOMETRYGYM_IFC]

`GeometryGymIFC_Core` supplies a pure-managed buildingSMART IFC object model:
the `DatabaseIfc` repository, schema-versioned read/write across STEP, IFC-XML,
and IFC-JSON, the full IFC4.3 entity vocabulary, and `Extract<T>` graph traversal
for the Compute geometry interchange rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `GeometryGymIFC_Core`
- package: `GeometryGymIFC_Core`
- version: `25.7.30`
- assembly: `GeometryGymIFCcore`
- namespace: `GeometryGym.Ifc`
- namespace: `GeometryGym.STEP`
- asset: net8.0, net7.0, net6.0, netstandard2.0
- asset: IL-only AnyCPU managed assembly; no `runtimes/` folder, no native binaries
- asset: zero declared package dependencies on every target framework
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: repository, factory, and serialization roots
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`, `GeometryGym.STEP`
- rail: geometry

| [INDEX] | [SYMBOL]              | [RAIL]   | [CAPABILITY]                                                                                 |
| :-----: | :-------------------- | :------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `DatabaseIfc`         | geometry | IFC model repository; owns all entities, schema, units, tolerance, and I/O                   |
|  [02]   | `DatabaseSTEP<T>`     | geometry | generic STEP record store; `IEnumerable<T>`, `this[int stepId]`, `NextObjectRecord`          |
|  [03]   | `BaseClassIfc`        | geometry | abstract root of every IFC entity; carries `Database`, `Extract<T>`, STEP/JSON serialization |
|  [04]   | `STEPEntity`          | geometry | base STEP record carrier under `BaseClassIfc`                                                |
|  [05]   | `FactoryIfc`          | geometry | per-database factory; canonical axes, origins, placements (`RootPlacement` → `IfcLocalPlacement` at world origin, `XYPlanePlacement` → `IfcAxis2Placement3D` — the default element/profile placements occurrence ctors take), application, owner history |
|  [06]   | `ParserIfc`           | geometry | static STEP/enum/GUID codec; `ParseEnum<T>`, `DecodeGlobalID`, `EncodeGuid`                  |
|  [07]   | `ParserSTEP`          | geometry | static low-level STEP token parser                                                           |
|  [08]   | `STEPFileInformation` | geometry | originating-file header metadata on the database                                             |
|  [09]   | `DuplicateOptions`    | geometry | options carrier for cross-database entity duplication                                        |
|  [10]   | `DuplicateMapping`    | geometry | source-to-target entity map during duplication                                               |

[PUBLIC_TYPE_SCOPE]: IFC schema vocabulary enums
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                           | [RAIL]   | [CAPABILITY]                                                                                                                                                                                                                                                                |
| :-----: | :--------------------------------- | :------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `ReleaseVersion`                   | geometry | schema version: `IFC2x3`, `IFC4A2`, `IFC4X3`, `IFC4X3_ADD2`, `IFC4X4_DRAFT` (plus retired/withdrawn `[Obsolete]` members)                                                                                                                                                   |
|  [02]   | `ModelView`                        | geometry | MVD selector: `Ifc4Reference`, `Ifc4DesignTransfer`, `Ifc2x3Coordination`, `IFC4X3Reference`, `IFC4X3AlignmentBasedView`, `Ifc4X3NotAssigned`                                                                                                                               |
|  [03]   | `FormatIfcSerialization`           | geometry | serialization format for `DatabaseIfc.ToString`: `STEP`, `XML`, `JSON`                                                                                                                                                                                                      |
|  [04]   | `IfcReflectanceMethodEnum`         | geometry | PBR/Phong reflectance model: `BLINN`, `FLAT`, `GLASS`, `MATT`, `METAL`, `MIRROR`, `PHONG`, `PLASTIC`, `STRAUSS`, `NOTDEFINED`                                                                                                                                               |
|  [05]   | `IfcSurfaceSide`                   | geometry | surface-style application side: `POSITIVE`, `NEGATIVE`, `BOTH`                                                                                                                                                                                                              |
|  [06]   | `IfcZone` (no `PredefinedType`)    | geometry | functional-zone grouping carries NO predefined enum in GeometryGym 25.7.30 (`IfcZone`/`IfcSystem`/`IfcGroup` expose only `LongName`/`Name`); there is no `IfcZoneTypeEnum` — only `IfcSpatialZone`/`IfcDistributionSystem`/`IfcStructuralLoadGroup` carry a predefined kind |
|  [07]   | `IfcSpatialZoneTypeEnum`           | geometry | spatial-zone kind: `NOTDEFINED`, `USERDEFINED`, `CONSTRUCTION`, `FIRESAFETY`, `LIGHTING`, `OCCUPANCY`, `SECURITY`, `THERMAL`, `VENTILATION`, `TRANSPORT`, `RESERVATION`, `INTERFERENCE`, …                                                                                  |
|  [08]   | `IfcDistributionSystemEnum`        | geometry | distribution-system kind: `AIRCONDITIONING`, `ELECTRICAL`, `DOMESTICCOLDWATER`/`DOMESTICHOTWATER`, `DRAINAGE`, `FIREPROTECTION`, `VENTILATION`, …                                                                                                                           |
|  [09]   | `IfcFlowDirectionEnum`             | geometry | port flow direction: `SOURCE`, `SINK`, `SOURCEANDSINK`, `NOTDEFINED`                                                                                                                                                                                                        |
|  [10]   | `IfcSequenceEnum`                  | geometry | task-dependency kind: `START_START`, `START_FINISH`, `FINISH_START`, `FINISH_FINISH`, `USERDEFINED`, `NOTDEFINED`                                                                                                                                                           |
|  [11]   | `IfcTaskDurationEnum`              | geometry | task-duration interpretation: `ELAPSEDTIME`, `WORKTIME`, `NOTDEFINED`                                                                                                                                                                                                       |
|  [12]   | `IfcCostScheduleTypeEnum`          | geometry | cost-schedule kind: `BUDGET`, `COSTPLAN`, `ESTIMATE`, `TENDER`, `PRICEDBILLOFQUANTITIES`, `SCHEDULEOFRATES`, …                                                                                                                                                              |
|  [13]   | `IfcStructuralCurveMemberTypeEnum` | geometry | idealized 1D member kind: `RIGID_JOINED_MEMBER`, `PIN_JOINED_MEMBER`, `CABLE`, `TENSION_MEMBER`, `COMPRESSION_MEMBER`, …                                                                                                                                                    |
|  [14]   | `IfcLoadGroupTypeEnum`             | geometry | load-group kind: `LOAD_GROUP`, `LOAD_CASE`, `LOAD_COMBINATION`, `USERDEFINED`, `NOTDEFINED`                                                                                                                                                                                 |
|  [15]   | `IfcCardinalPointReference`        | geometry | profile placement reference axis on `IfcMaterialProfileSetUsage.CardinalPoint`: `DEFAULT`(0)/`BOTLEFT`/`BOTMID`/`BOTRIGHT`/`MIDLEFT`/`MID`(5, default)/`MIDRIGHT`/`TOPLEFT`/`TOPMID`/`TOPRIGHT`(9 structural grid)/`CENTROID`/`BOTCENT`/`LEFTCENT`/`RIGHTCENT`/`TOPCENT`/`SHEARCENT`/`BOTSHEAR`/`LEFTSHEAR`/`RIGHTSHEAR`/`TOPSHEAR`(19) — the `CardinalPoint` smart-enum reciprocal |
|  [16]   | `IfcLogicalEnum`                   | geometry | three-valued logical: `TRUE`, `FALSE`, `UNKNOWN` — the `IfcMaterialLayer.IsVentilated` value, compared as `== IfcLogicalEnum.TRUE`                                                                                                                                          |
|  [17]   | `IfcWorkScheduleTypeEnum`          | geometry | work-schedule kind: `ACTUAL`, `BASELINE`, `PLANNED` — the `IfcWorkSchedule.PredefinedType`, lowered onto the `WorkScheduleKind` reciprocal                                                                                                                                  |

[PUBLIC_TYPE_SCOPE]: IFC kernel root entities
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                   | [RAIL]   | [CAPABILITY]                                                                           |
| :-----: | :------------------------- | :------- | :------------------------------------------------------------------------------------- |
|  [01]   | `IfcRoot`                  | geometry | rooted entity base; `GlobalId`, `Guid`, `OwnerHistory`, `Name`, `Description`          |
|  [02]   | `IfcObjectDefinition`      | geometry | object base; `Nests`, `Decomposes`, `AddAggregated`, `FindProperty`, `FindPropertySet` |
|  [03]   | `IfcContext`               | geometry | shared context; `UnitsInContext`, `AddDeclared`, `DeclaredTypes`                       |
|  [04]   | `IfcProject`               | geometry | project root context; ctors over building/site/facility/zone + units; `UppermostSite`  |
|  [05]   | `IfcProjectLibrary`        | geometry | shared declaration library context                                                     |
|  [06]   | `IfcObject`                | geometry | occurrence object base under `IfcObjectDefinition`                                     |
|  [07]   | `IfcProduct`               | geometry | spatially located product; `ObjectPlacement`, `Representation`, `AddElement`           |
|  [08]   | `IfcElement`               | geometry | physical element base; `Tag`, `MaterialSelect`, `SetMaterial`                          |
|  [09]   | `IfcOwnerHistory`          | geometry | change-tracking stamp; creation/modification metadata                                  |
|  [10]   | `IfcApplication`           | geometry | authoring-application identity                                                         |
|  [11]   | `IfcPerson`                | geometry | person actor record                                                                    |
|  [12]   | `IfcOrganization`          | geometry | organization actor record                                                              |
|  [13]   | `IfcPersonAndOrganization` | geometry | bound person + organization actor                                                      |

[PUBLIC_TYPE_SCOPE]: spatial structure and type-object families
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                     | [RAIL]   | [CAPABILITY]                                                            |
| :-----: | :--------------------------- | :------- | :---------------------------------------------------------------------- |
|  [01]   | `IfcSpatialElement`          | geometry | spatial container base; `LongName`, `ReferenceElement`                  |
|  [02]   | `IfcSpatialStructureElement` | geometry | spatial hierarchy node; `CompositionType` (`IfcElementCompositionEnum`) |
|  [03]   | `IfcSite`                    | geometry | site spatial structure element                                          |
|  [04]   | `IfcBuilding`                | geometry | building spatial structure element                                      |
|  [05]   | `IfcBuildingStorey`          | geometry | storey spatial structure element                                        |
|  [06]   | `IfcFacility`                | geometry | IFC4.3 facility spatial structure base                                  |
|  [07]   | `IfcFacilityPart`            | geometry | IFC4.3 facility-part subdivision                                        |
|  [08]   | `IfcExternalSpatialElement`  | geometry | external (outside-facility) spatial element                             |
|  [09]   | `IfcTypeObject`              | geometry | type-definition base under `IfcObjectDefinition`                        |
|  [10]   | `IfcTypeProduct`             | geometry | product type with representation maps                                   |
|  [11]   | `IfcElementType`             | geometry | element type base under `IfcTypeProduct`                                |
|  [12]   | `IfcBuiltElementType`        | geometry | built-element type base                                                 |

[PUBLIC_TYPE_SCOPE]: property-set, quantity, and material families
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                     | [RAIL]   | [CAPABILITY]                                                                      |
| :-----: | :--------------------------- | :------- | :-------------------------------------------------------------------------------- |
|  [01]   | `IfcPropertySetDefinition`   | geometry | property/quantity set base                                                        |
|  [02]   | `IfcPropertySet`             | geometry | named set of `IfcProperty` instances                                              |
|  [03]   | `IfcPropertySingleValue`     | geometry | single-value property (`IfcSimpleProperty` subtype)                               |
|  [04]   | `IfcPropertyEnumeratedValue` | geometry | enumeration-referenced property value                                             |
|  [05]   | `IfcComplexProperty`         | geometry | nested property aggregate                                                         |
|  [06]   | `IfcQuantitySet`             | geometry | quantity-set base under `IfcPropertySetDefinition`                                |
|  [07]   | `IfcElementQuantity`         | geometry | named set of physical quantities                                                  |
|  [08]   | `IfcPhysicalSimpleQuantity`  | geometry | simple quantity base; `IfcQuantityLength`/`Area`/`Volume`/`Weight`/`Count`/`Time` |
|  [09]   | `IfcPropertySetTemplate`     | geometry | reusable property-set template                                                    |
|  [10]   | `IfcMaterial`                | geometry | named material definition                                                         |
|  [11]   | `IfcMaterialLayerSet`        | geometry | ordered material layer assembly; `MaterialLayers` (`LIST<IfcMaterialLayer>`)      |
|  [12]   | `IfcMaterialLayerSetUsage`   | geometry | layer-set application to an element; `ForLayerSet` (`IfcMaterialLayerSet`), `LayerSetDirection` (`IfcLayerSetDirectionEnum`), `DirectionSense` (`IfcDirectionSenseEnum`), `OffsetFromReferenceLine` (`double`) |
|  [13]   | `IfcMaterialProfileSet`      | geometry | material-profile assembly for linear members                                      |
|  [14]   | `IfcMaterialConstituentSet`  | geometry | named constituent material set; `MaterialConstituents` (`Dictionary<string, IfcMaterialConstituent>`, iterate `.Values`) |
|  [15]   | `IfcMaterialProperties`      | geometry | `IfcExtendedProperties` subtype binding a named Pset to an `IfcMaterial`; public ctor `(string name, IfcMaterialDefinition mat)`, `Material` member, columns added to the inherited `Properties` dict |
|  [16]   | `IfcExtendedProperties`      | geometry | extended-property base; `Name`/`Description` plus `Properties` `Dictionary<string, IfcProperty>` and a `this[name]` indexer |
|  [17]   | `IfcProperty`                | geometry | abstract property root (`IfcPropertySingleValue` etc.); the `Properties`-dict element type |
|  [18]   | `IfcMaterialProfile`         | geometry | one profile-material row in an `IfcMaterialProfileSet.MaterialProfiles` (`LIST<IfcMaterialProfile>`); `Material` (`IfcMaterial`), `Profile` (`IfcProfileDef`, the subtype-discriminated section), `Name`/`Description`/`Category` (`string`), `Priority` (`int`) |
|  [19]   | `IfcMaterialLayer`           | geometry | one layer in `IfcMaterialLayerSet.MaterialLayers` (`: IfcMaterialDefinition`); `Material` (`IfcMaterial`), `LayerThickness` (`double`), `Category` (`string`), `IsVentilated` (`IfcLogicalEnum`); ctor `(IfcMaterial, double thickness, string name)` |
|  [20]   | `IfcMaterialConstituent`     | geometry | one constituent in `IfcMaterialConstituentSet.MaterialConstituents` (`: IfcMaterialDefinition`); `Material` (`IfcMaterial`), `Name` (`string`), `Fraction` (`double`); ctor `(string name, IfcMaterial)` |

[PUBLIC_TYPE_SCOPE]: relationship families
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                            | [RAIL]   | [CAPABILITY]                                              |
| :-----: | :---------------------------------- | :------- | :-------------------------------------------------------- |
|  [01]   | `IfcRelationship`                   | geometry | objectified-relationship root                             |
|  [02]   | `IfcRelAggregates`                  | geometry | whole-part decomposition (`IfcRelDecomposes` subtype)     |
|  [03]   | `IfcRelNests`                       | geometry | ordered nesting decomposition; `RelatingObject` (`IfcObjectDefinition`, the parent reached via `IfcObjectDefinition.Nests`) |
|  [04]   | `IfcRelContainedInSpatialStructure` | geometry | element-to-spatial containment (`IfcRelConnects` subtype) |
|  [05]   | `IfcRelDefinesByProperties`         | geometry | binds a property/quantity set to objects                  |
|  [06]   | `IfcRelDefinesByType`               | geometry | binds occurrences to a type object                        |
|  [07]   | `IfcRelAssociatesMaterial`          | geometry | associates a material definition with objects; `RelatingMaterial` (`IfcMaterialSelect`), `RelatedObjects` (`SET<IfcDefinitionSelect>`) |
|  [08]   | `IfcRelAssociatesClassification`    | geometry | associates a classification reference; ctor `(IfcClassificationSelect classification, IfcDefinitionSelect related)` |
|  [09]   | `IfcRelDeclares`                    | geometry | declares definitions within a context                     |
|  [10]   | `IfcRelVoidsElement`                | geometry | subtracts an opening from an element                      |
|  [11]   | `IfcRelConnectsElements`            | geometry | physical element-to-element connection                    |
|  [12]   | `IfcDefinitionSelect`               | geometry | SELECT (`interface`) of the objects a relationship relates — the `IfcRelAssociates.RelatedObjects` element type and the `(IfcDefinitionSelect)material` cast target in the classification-associate ctor |

[PUBLIC_TYPE_SCOPE]: architectural built-element family
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]         | [RAIL]   | [CAPABILITY]                                                                              |
| :-----: | :--------------- | :------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `IfcCovering`    | geometry | finish covering built element; `PredefinedType` (`IfcCoveringTypeEnum`)                   |
|  [02]   | `IfcCurtainWall` | geometry | curtain-wall built element aggregating plate/member units; `PredefinedType`               |
|  [03]   | `IfcRailing`     | geometry | railing built element; `PredefinedType` (`IfcRailingTypeEnum`)                            |
|  [04]   | `IfcRamp`        | geometry | ramp built element; `PredefinedType` (`IfcRampTypeEnum`)                                  |
|  [05]   | `IfcRoof`        | geometry | roof built element; `PredefinedType` (`IfcRoofTypeEnum`)                                  |
|  [06]   | `IfcStair`       | geometry | stair built element; `PredefinedType` (`IfcStairTypeEnum`)                                |
|  [07]   | `IfcPlate`       | geometry | planar built element (curtain-wall panel); `PredefinedType` (`IfcPlateTypeEnum`)          |
|  [08]   | `IfcMember`      | geometry | structural-member built element (mullion/post); `PredefinedType` (`IfcMemberTypeEnum`)    |
|  [09]   | `IfcFooting`     | geometry | foundation footing built element; `PredefinedType` (`IfcFootingTypeEnum`)                 |
|  [10]   | `IfcPile`        | geometry | foundation pile built element; `PredefinedType` (`IfcPileTypeEnum`)                       |
|  [11]   | `IfcWall`        | geometry | wall built element; `PredefinedType` (`IfcWallTypeEnum`: STANDARD/SHEAR/PARTITIONING/…)   |
|  [12]   | `IfcSlab`        | geometry | slab built element; `PredefinedType` (`IfcSlabTypeEnum`: FLOOR/ROOF/LANDING/BASESLAB)     |
|  [13]   | `IfcColumn`      | geometry | column built element; `PredefinedType` (`IfcColumnTypeEnum`)                              |
|  [14]   | `IfcBeam`        | geometry | beam built element; `PredefinedType` (`IfcBeamTypeEnum`)                                  |
|  [15]   | `IfcDoor`        | geometry | door built element; `PredefinedType` (`IfcDoorTypeEnum`) + `OperationType`                |
|  [16]   | `IfcWindow`      | geometry | window built element; `PredefinedType` (`IfcWindowTypeEnum`) + `PartitioningType`         |
|  [17]   | `IfcSpace`       | geometry | space spatial element; `PredefinedType` (`IfcSpaceTypeEnum`: INTERNAL/EXTERNAL/PARKING/…) |
|  [18]   | `IfcBuildingElementProxy` | geometry | generic proxy built element (`: IfcBuiltElement`); `PredefinedType` (`IfcBuildingElementProxyTypeEnum`: COMPLEX/ELEMENT/PARTIAL/PROVISIONFORVOID/…); ctor `(IfcObjectDefinition host, IfcObjectPlacement, IfcProductDefinitionShape)` — the unclassified-product carrier the canonical→IFC export authors |

[PUBLIC_TYPE_SCOPE]: MEP distribution-element family
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                    | [RAIL]   | [CAPABILITY]                                                                                         |
| :-----: | :-------------------------- | :------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `IfcDistributionElement`    | geometry | MEP distribution-element base under `IfcElement`; carries `HasPorts` (`IfcRelConnectsPortToElement`) |
|  [02]   | `IfcFlowSegment`            | geometry | flow segment (duct/pipe/cable run); `PredefinedType`                                                 |
|  [03]   | `IfcFlowFitting`            | geometry | flow fitting (elbow/tee/junction); `PredefinedType`                                                  |
|  [04]   | `IfcFlowTerminal`           | geometry | flow terminal (air/sanitary/light fixture); `PredefinedType` (AIRTERMINAL/SANITARYTERMINAL/…)        |
|  [05]   | `IfcFlowController`         | geometry | flow controller (valve/damper/switch); `PredefinedType`                                              |
|  [06]   | `IfcFlowMovingDevice`       | geometry | flow moving device (pump/fan/compressor); `PredefinedType`                                           |
|  [07]   | `IfcFlowStorageDevice`      | geometry | flow storage device (tank/battery); `PredefinedType`                                                 |
|  [08]   | `IfcEnergyConversionDevice` | geometry | energy conversion device (boiler/chiller/coil); `PredefinedType`                                     |
|  [09]   | `IfcDistributionPort`       | geometry | typed connection port; `FlowDirection` (`IfcFlowDirectionEnum`), `SystemType`, `PredefinedType`      |
|  [10]   | `IfcPort`                   | geometry | abstract connection-port base; `ContainedIn`, `ConnectedFrom`, `ConnectedTo`                         |

[PUBLIC_TYPE_SCOPE]: structural-analysis-domain family
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                           | [RAIL]   | [CAPABILITY]                                                                                            |
| :-----: | :--------------------------------- | :------- | :------------------------------------------------------------------------------------------------------ |
|  [01]   | `IfcStructuralAnalysisModel`       | geometry | structural-analysis container; `OrientationOf2DPlane`, `LoadedBy`, `HasResults`, `SharedPlacement`      |
|  [02]   | `IfcStructuralItem`                | geometry | structural-item base; `AssignedStructuralActivity`, `AssignedToStructuralItem`                          |
|  [03]   | `IfcStructuralMember`              | geometry | idealized structural member base under `IfcStructuralItem`                                              |
|  [04]   | `IfcStructuralCurveMember`         | geometry | 1D idealized member (beam/column line); `PredefinedType` (`IfcStructuralCurveMemberTypeEnum`)           |
|  [05]   | `IfcStructuralSurfaceMember`       | geometry | 2D idealized member (slab/wall surface); `PredefinedType`, `Thickness`                                  |
|  [06]   | `IfcStructuralConnection`          | geometry | structural connection base; `AppliedCondition` (`IfcBoundaryCondition`)                                 |
|  [07]   | `IfcStructuralPointConnection`     | geometry | point support/connection node                                                                           |
|  [08]   | `IfcStructuralCurveConnection`     | geometry | curve support/connection edge                                                                           |
|  [09]   | `IfcStructuralSurfaceConnection`   | geometry | surface support/connection                                                                              |
|  [10]   | `IfcStructuralLoadGroup`           | geometry | grouped structural loads; `PredefinedType`, `ActionType`, `ActionSource`, `SourceOfResultGroup`         |
|  [11]   | `IfcStructuralLoadCase`            | geometry | load case under `IfcStructuralLoadGroup`; `SelfWeightCoefficients`                                      |
|  [12]   | `IfcStructuralResultGroup`         | geometry | grouped analysis results                                                                                |
|  [13]   | `IfcBoundaryCondition`             | geometry | boundary-condition base (`IfcBoundaryNodeCondition`/`IfcBoundaryEdgeCondition`)                         |
|  [14]   | `IfcRelConnectsStructuralMember`   | geometry | connects an idealized member to a connection; `RelatingStructuralMember`, `RelatedStructuralConnection` |
|  [15]   | `IfcRelConnectsStructuralActivity` | geometry | binds a load/result activity to a structural item                                                       |
|  [16]   | `IfcRelAssignsToGroup`             | geometry | assigns objects to an `IfcGroup`/`IfcSystem`/`IfcStructuralLoadGroup`; `RelatedObjects` (`SET<IfcObjectDefinition>`, the assigned members reached via `IfcGroup.IsGroupedBy`) |

[PUBLIC_TYPE_SCOPE]: grouping, zone, and distribution-system family
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                             | [RAIL]   | [CAPABILITY]                                                                                |
| :-----: | :----------------------------------- | :------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `IfcGroup`                           | geometry | non-spatial logical grouping base; `IsGroupedBy` (`IfcRelAssignsToGroup`)                   |
|  [02]   | `IfcSystem`                          | geometry | functional system grouping under `IfcGroup`; `ServicesBuildings`                            |
|  [03]   | `IfcBuildingSystem`                  | geometry | building-system grouping; `PredefinedType` (`IfcBuildingSystemTypeEnum`)                    |
|  [04]   | `IfcDistributionSystem`              | geometry | MEP distribution system; `PredefinedType` (`IfcDistributionSystemEnum`), member element set |
|  [05]   | `IfcZone`                            | geometry | functional zone aggregating spaces across storeys; `LongName` + ctor `(IfcSpatialElement, string, List<IfcSpace>)` — carries NO `PredefinedType` in 25.7.30 (see enum row `IfcZone`) |
|  [06]   | `IfcSpatialZone`                     | geometry | fire/thermal/construction/occupancy zone; `PredefinedType` (`IfcSpatialZoneTypeEnum`)       |
|  [07]   | `IfcRelReferencedInSpatialStructure` | geometry | references an element into a spatial structure it is not contained in; many-to-many overlay |
|  [08]   | `IfcRelServicesBuildings`            | geometry | binds a system to the spatial structures it serves                                          |
|  [09]   | `IfcRelConnectsPortToElement`        | geometry | connects an `IfcDistributionPort` to its owning distribution element                        |
|  [10]   | `IfcRelConnectsPorts`                | geometry | port-to-port connection edge; `RelatingPort`, `RelatedPort`, `RealizingElement`             |

[PUBLIC_TYPE_SCOPE]: scheduling, cost, and resource family
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                           | [RAIL]   | [CAPABILITY]                                                                                           |
| :-----: | :--------------------------------- | :------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `IfcProcess`                       | geometry | process base; `IsSuccessorFrom`/`IsPredecessorTo` (`IfcRelSequence`), `OperatesOn`                     |
|  [02]   | `IfcTask`                          | geometry | scheduled task; `Status`, `WorkMethod`, `IsMilestone`, `TaskTime`, `PredefinedType`                    |
|  [03]   | `IfcTaskTime`                      | geometry | task schedule times; `ScheduleStart`/`ScheduleFinish`/`ScheduleDuration`, `ActualStart`/`ActualFinish` |
|  [04]   | `IfcWorkSchedule`                  | geometry | work schedule under `IfcWorkControl`; `Controls`, `PredefinedType`                                     |
|  [05]   | `IfcWorkPlan`                      | geometry | work plan grouping schedules under `IfcWorkControl`; `PredefinedType`                                  |
|  [06]   | `IfcWorkCalendar`                  | geometry | working/exception time calendar; `WorkingTimes`, `ExceptionTimes`                                      |
|  [07]   | `IfcRelSequence`                   | geometry | task dependency edge; `RelatingProcess`, `RelatedProcess`, `TimeLag`, `SequenceType`                   |
|  [08]   | `IfcRelAssignsToProcess`           | geometry | assigns products/resources to a process; `RelatingProcess`, `RelatedObjects`                           |
|  [09]   | `IfcCostSchedule`                  | geometry | cost schedule under `IfcControl`; `Controls`, `PredefinedType`, `SubmittedOn`                          |
|  [10]   | `IfcCostItem`                      | geometry | cost line item; `CostValues` (`IfcCostValue`), `CostQuantities`, `PredefinedType`                      |
|  [11]   | `IfcCostValue`                     | geometry | applied cost value/rate; `AppliedValue` (`IfcAppliedValue`), `UnitBasis`, `Category`                   |
|  [12]   | `IfcConstructionResource`          | geometry | construction-resource base; `Usage`, `BaseCosts`, `BaseQuantity`                                       |
|  [13]   | `IfcLaborResource`                 | geometry | labor resource; `PredefinedType` (`IfcLaborResourceTypeEnum`)                                          |
|  [14]   | `IfcConstructionMaterialResource`  | geometry | material resource; `PredefinedType` (`IfcConstructionMaterialResourceTypeEnum`)                        |
|  [15]   | `IfcConstructionEquipmentResource` | geometry | equipment resource; `PredefinedType`                                                                   |
|  [16]   | `IfcRelAssignsToControl`           | geometry | assigns objects to a control (cost item/schedule); `RelatingControl`, `RelatedObjects`                 |
|  [17]   | `IfcWorkTime`                      | geometry | one working/exception period in `IfcWorkCalendar.WorkingTimes`/`ExceptionTimes` (`: IfcSchedulingTime`); public `StartDate`/`FinishDate` (`DateTime`) |

[PUBLIC_TYPE_SCOPE]: georeferencing and map-conversion entities
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                       | [RAIL]   | [CAPABILITY]                                                                                                        |
| :-----: | :----------------------------- | :------- | :------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `IfcCoordinateOperation`       | geometry | coordinate-operation base; `SourceCRS`, `TargetCRS`                                                                 |
|  [02]   | `IfcMapConversion`             | geometry | rigid map-conversion offset; `Eastings`, `Northings`, `OrthogonalHeight`, `XAxisAbscissa`, `XAxisOrdinate`, `Scale` |
|  [03]   | `IfcCoordinateReferenceSystem` | geometry | CRS base; `Name`, `GeodeticDatum`, `VerticalDatum`                                                                  |
|  [04]   | `IfcProjectedCRS`              | geometry | projected CRS; `Name` (EPSG), `GeodeticDatum`, `VerticalDatum`, `MapProjection`, `MapZone`, `MapUnit`               |
|  [05]   | `IfcMapConversionScaled`       | geometry | per-axis scaled map conversion (IFC4.3 ADD2); `FactorX`/`FactorY`/`FactorZ`                                         |

[PUBLIC_TYPE_SCOPE]: IFC4.3 infrastructure entities — alignment and facility
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                      | [RAIL]   | [CAPABILITY]                                     |
| :-----: | :---------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `IfcAlignment`                | geometry | linear-referencing alignment positioning element |
|  [02]   | `IfcAlignmentHorizontal`      | geometry | horizontal alignment layout                      |
|  [03]   | `IfcAlignmentVertical`        | geometry | vertical alignment layout                        |
|  [04]   | `IfcAlignmentCant`            | geometry | rail cant alignment layout                       |
|  [05]   | `IfcAlignmentSegment`         | geometry | one parameterized alignment segment              |
|  [06]   | `IfcLinearPlacement`          | geometry | placement along a curve via distance expression  |
|  [07]   | `IfcLinearPositioningElement` | geometry | base for linear referencing positioning          |
|  [08]   | `IfcReferent`                 | geometry | referent point along an alignment                |
|  [09]   | `IfcBridge`                   | geometry | bridge facility                                  |
|  [10]   | `IfcRailway`                  | geometry | railway facility                                 |
|  [11]   | `IfcRoad`                     | geometry | road facility                                    |
|  [12]   | `IfcMarineFacility`           | geometry | marine facility                                  |

[PUBLIC_TYPE_SCOPE]: IFC4.3 infrastructure entities — earthworks and geotechnics
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                 | [RAIL]   | [CAPABILITY]                                    |
| :-----: | :----------------------- | :------- | :---------------------------------------------- |
|  [01]   | `IfcCourse`              | geometry | layered pavement/earthwork course built element |
|  [02]   | `IfcPavement`            | geometry | pavement built element                          |
|  [03]   | `IfcRail`                | geometry | rail built element                              |
|  [04]   | `IfcEarthworksFill`      | geometry | earthworks fill element                         |
|  [05]   | `IfcEarthworksCut`       | geometry | earthworks excavation element                   |
|  [06]   | `IfcGeotechnicalStratum` | geometry | geotechnical soil/rock stratum                  |
|  [07]   | `IfcBorehole`            | geometry | geotechnical borehole assembly                  |

[PUBLIC_TYPE_SCOPE]: geometry representation entities
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                            | [RAIL]   | [CAPABILITY]                                                                                                                                                                              |
| :-----: | :---------------------------------- | :------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IfcProductDefinitionShape`         | geometry | product shape; aggregates representations                                                                                                                                                 |
|  [02]   | `IfcRepresentation`                 | geometry | one geometric/topological representation                                                                                                                                                  |
|  [03]   | `IfcGeometricRepresentationContext` | geometry | coordinate space + precision for representations                                                                                                                                          |
|  [04]   | `IfcCartesianPoint`                 | geometry | n-D cartesian point                                                                                                                                                                       |
|  [05]   | `IfcCartesianPointList3D`           | geometry | packed 3D point list                                                                                                                                                                      |
|  [06]   | `IfcDirection`                      | geometry | direction vector                                                                                                                                                                          |
|  [07]   | `IfcAxis2Placement3D`               | geometry | 3D placement (location + axes)                                                                                                                                                            |
|  [08]   | `IfcLocalPlacement`                 | geometry | relative object placement                                                                                                                                                                 |
|  [09]   | `IfcExtrudedAreaSolid`              | geometry | swept-area extrusion solid                                                                                                                                                                |
|  [10]   | `IfcPolygonalFaceSet`               | geometry | indexed polygonal face mesh                                                                                                                                                               |
|  [11]   | `IfcTriangulatedFaceSet`            | geometry | indexed triangle mesh                                                                                                                                                                     |
|  [12]   | `IfcFacetedBrep`                    | geometry | faceted boundary-representation solid                                                                                                                                                     |
|  [13]   | `IfcAdvancedBrep`                   | geometry | NURBS-faced boundary-representation solid                                                                                                                                                 |
|  [14]   | `IfcBooleanResult`                  | geometry | CSG boolean operation result                                                                                                                                                              |
|  [15]   | `IfcMappedItem`                     | geometry | instanced representation map reference                                                                                                                                                    |
|  [16]   | `IfcRepresentationMap`              | geometry | reusable type-bound geometry library; `MappingOrigin` (`IfcAxis2Placement`), `MappedRepresentation` (`IfcRepresentation`), `HasShapeAspects`, referenced by `IfcMappedItem.MappingSource` |

[PUBLIC_TYPE_SCOPE]: parameterized profile-definition family
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry
- note: the `IfcMaterialProfile.Profile` cross-section subtype axis the `Semantics/composition#MATERIAL_COMPOSITION` `ProfileDefKind`/`DimsOf` reciprocal discriminates against the `Rasm.Materials/Profiles/steel#STEEL_FAMILY` `SteelClass.IfcSubtype` egress; hollow subtypes derive from their solid bases (discriminate the wall section first)

| [INDEX] | [SYMBOL]                       | [RAIL]   | [CAPABILITY]                                                                                                                                                                  |
| :-----: | :----------------------------- | :------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IfcProfileDef`                | geometry | cross-section profile root; `ProfileType` (`IfcProfileTypeEnum`), `ProfileName` (`string`) — the subtype runtime type the `MaterialProfile.Kind` reciprocal discriminates    |
|  [02]   | `IfcProfileTypeEnum`           | geometry | profile-use kind: `AREA` (solid swept section, default), `CURVE` (open section)                                                                                              |
|  [03]   | `IfcParameterizedProfileDef`   | geometry | abstract base for dimensioned parametric sections under `IfcProfileDef`; `Position` (`IfcAxis2Placement2D`)                                                                   |
|  [04]   | `IfcIShapeProfileDef`          | geometry | I/H wide-flange: `OverallWidth`, `OverallDepth`, `WebThickness`, `FlangeThickness`, `FilletRadius`, `FlangeEdgeRadius`, `FlangeSlope` (`double`)                              |
|  [05]   | `IfcUShapeProfileDef`          | geometry | channel: `Depth`, `FlangeWidth`, `WebThickness`, `FlangeThickness`, `FilletRadius`, `EdgeRadius`, `FlangeSlope` (`double`)                                                    |
|  [06]   | `IfcLShapeProfileDef`          | geometry | angle: `Depth`, `Width`, `Thickness`, `FilletRadius`, `EdgeRadius`, `LegSlope`, `CentreOfGravityInX`/`InY` (`double`)                                                         |
|  [07]   | `IfcTShapeProfileDef`          | geometry | tee: `Depth`, `FlangeWidth`, `WebThickness`, `FlangeThickness`, `FilletRadius`, `FlangeEdgeRadius`, `WebEdgeRadius`, `WebSlope`, `FlangeSlope` (`double`)                      |
|  [08]   | `IfcRectangleProfileDef`       | geometry | solid rectangle: `XDim`, `YDim` (`double`); base of `IfcRectangleHollowProfileDef`                                                                                            |
|  [09]   | `IfcRectangleHollowProfileDef` | geometry | rectangular HSS: inherits `XDim`/`YDim`; `WallThickness`, `InnerFilletRadius`, `OuterFilletRadius` (`double`)                                                                 |
|  [10]   | `IfcCircleProfileDef`          | geometry | solid circle: `Radius` (`double`); base of `IfcCircleHollowProfileDef`; the fastener nominal-diameter carrier                                                                 |
|  [11]   | `IfcCircleHollowProfileDef`    | geometry | round HSS/pipe: inherits `Radius`; `WallThickness` (`double`)                                                                                                                 |
|  [12]   | `IfcArbitraryClosedProfileDef` | geometry | non-parametric closed section under `IfcProfileDef`; `OuterCurve` (`IfcCurve`) — the `DoubleL`/composite carrier with no single parametric form (back-to-back rides a column) |

[PUBLIC_TYPE_SCOPE]: tessellation geometry — AP242/IFC4.3 mesh interchange
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                  | [RAIL]   | [CAPABILITY]                                                                           |
| :-----: | :------------------------ | :------- | :------------------------------------------------------------------------------------- |
|  [01]   | `IfcTessellatedItem`      | geometry | abstract tessellated geometry item base; derives from `IfcGeometricRepresentationItem` |
|  [02]   | `IfcTessellatedFaceSet`   | geometry | abstract indexed face mesh base; `Closed`, `HasColours`, `HasTextures` properties      |
|  [03]   | `IfcTriangulatedFaceSet`  | geometry | triangle mesh: `CoordIndex`, `Normals`, `NormalIndex`, `PnIndex`                       |
|  [04]   | `IfcPolygonalFaceSet`     | geometry | polygon mesh (already catalogued); paired here as `IfcTessellatedFaceSet` subtype      |
|  [05]   | `IfcCartesianPointList`   | geometry | abstract packed point list base                                                        |
|  [06]   | `IfcCartesianPointList3D` | geometry | packed 3D point list; used as `Coordinates` by tessellated face sets                   |
|  [07]   | `IfcCartesianPointList2D` | geometry | packed 2D point list                                                                   |

[PUBLIC_TYPE_SCOPE]: material appearance and presentation interchange
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                              | [RAIL]   | [CAPABILITY]                                                                                                             |
| :-----: | :------------------------------------ | :------- | :----------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IfcPresentationStyle`                | geometry | abstract presentation style root                                                                                         |
|  [02]   | `IfcPresentationItem`                 | geometry | abstract item within a presentation style                                                                                |
|  [03]   | `IfcSurfaceStyle`                     | geometry | surface style; holds `Side` (`IfcSurfaceSide`) and `Styles` element set                                                  |
|  [04]   | `IfcSurfaceStyleShading`              | geometry | base shading style; `SurfaceColour` (`IfcColourRgb`), `Transparency` (`double`)                                          |
|  [05]   | `IfcSurfaceStyleRendering`            | geometry | extends shading with PBR parameters: `DiffuseColour`, `SpecularColour`, `ReflectanceMethod` (`IfcReflectanceMethodEnum`) |
|  [06]   | `IfcSurfaceStyleWithTextures`         | geometry | texture style; references `IfcSurfaceTexture` instances                                                                  |
|  [07]   | `IfcSurfaceStyleLighting`             | geometry | additional lighting coefficients: ambient, diffuse, transmission, reflectance                                            |
|  [08]   | `IfcSurfaceStyleRefraction`           | geometry | refraction index and light-transmission factor for optical materials                                                     |
|  [09]   | `IfcSurfaceTexture`                   | geometry | abstract surface texture; `RepeatS`, `RepeatT`, `Mode`, `TextureTransform`                                               |
|  [10]   | `IfcImageTexture`                     | geometry | file-path-referenced image texture (`IfcSurfaceTexture` subtype)                                                         |
|  [11]   | `IfcPixelTexture`                     | geometry | inline pixel-encoded texture (`IfcSurfaceTexture` subtype)                                                               |
|  [12]   | `IfcBlobTexture`                      | geometry | binary blob texture (`IfcSurfaceTexture` subtype); `RasterCode`, `RasterFormat`                                          |
|  [13]   | `IfcStyledItem`                       | geometry | binds a style to a representation item                                                                                   |
|  [14]   | `IfcStyledRepresentation`             | geometry | representation holding only styled items                                                                                 |
|  [15]   | `IfcMaterialDefinitionRepresentation` | geometry | links an `IfcMaterial` to its `IfcStyledRepresentation` set                                                              |
|  [16]   | `IfcColourRgb`                        | geometry | RGB colour value; `Red`/`Green`/`Blue` (`double`, normalized 0..1); ctor `(DatabaseIfc, double red, double green, double blue)` and `(DatabaseIfc, System.Drawing.Color)` |
|  [17]   | `IfcColourRgbList`                    | geometry | packed list of RGB colour triples for indexed colour sets                                                                |

[PUBLIC_TYPE_SCOPE]: units, presentation, and attributes
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                     | [RAIL]   | [CAPABILITY]                                                 |
| :-----: | :--------------------------- | :------- | :----------------------------------------------------------- |
|  [01]   | `IfcUnitAssignment`          | geometry | per-context unit set; nested `Length` enum for project ctors |
|  [02]   | `IfcSIUnit`                  | geometry | SI base/derived unit                                         |
|  [03]   | `IfcConversionBasedUnit`     | geometry | unit defined by conversion factor                            |
|  [04]   | `IfcDerivedUnit`             | geometry | compound derived unit                                        |
|  [05]   | `IfcMonetaryUnit`            | geometry | currency unit; `Currency` (`string`, ISO 4217) — the `IfcMeasureWithUnit.UnitComponent` of a priced `IfcCostValue` |
|  [06]   | `IfcMeasureWithUnit`         | geometry | value bound to a unit; `ValueComponent` (`IfcValue`), `UnitComponent` (`IfcUnit`) — the `IfcCostValue.UnitBasis` carrier |
|  [07]   | `IfcClassificationReference` | geometry | reference to an external classification; `ReferencedSource`/`Identification`/`Location`/`Name` (the `IfcExternalReference.Name` label — the resolved concept title `Ingest` lowers onto seam `Classification.Title`) |
|  [08]   | `IfcClassification`          | geometry | the classification source `IfcClassificationReference.ReferencedSource` names (Uniclass2015/OmniClass) |
|  [09]   | `VersionAddedAttribute`      | geometry | reflection attribute marking schema-version availability     |
|  [10]   | `IfcMeasureValue`            | geometry | abstract measure-value base (`: IfcValue`); `Measure` (`double`) — the `IfcMeasureWithUnit.ValueComponent` narrow target (the per-basis denominator) |
|  [11]   | `IfcMonetaryMeasure`         | geometry | monetary amount (`: IfcDerivedMeasureValue : IfcValue`); `Measure` (`double`) — the `IfcCostValue.AppliedValue` narrow target (the applied cost amount) |

[PUBLIC_TYPE_SCOPE]: structural-connection realizing-element surface
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                              | [RAIL]   | [CAPABILITY]                                                                                                                                                       |
| :-----: | :------------------------------------ | :------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IfcRelConnectsElements`              | geometry | abstract connection between two elements; `RelatingElement`/`RelatedElement` (`IfcElement`), `ConnectionGeometry` (`IfcConnectionGeometry`)                         |
|  [02]   | `IfcRelConnectsWithRealizingElements` | geometry | `IfcRelConnectsElements` subtype; `RealizingElements` (`SET<IfcElement>`, read-only) plus internal `ConnectionType` string — the realizing-fastener joint relation |
|  [03]   | `IfcMechanicalFastener`               | geometry | `IfcElementComponent`; only public scalar `PredefinedType` (`IfcMechanicalFastenerTypeEnum`) — `mNominalDiameter`/`mNominalLength` are `internal` (no public getter)|
|  [04]   | `IfcMechanicalFastenerTypeEnum`       | geometry | `NOTDEFINED`/`USERDEFINED`/`ANCHORBOLT`/`BOLT`/`DOWEL`/`NAIL`/`NAILPLATE`/`RIVET`/`SCREW`/`SHEARCONNECTOR`/`STAPLE`/`STUDSHEARCONNECTOR`/`COUPLER`(4x2)/4x3 rail set  |
|  [05]   | `IfcReinforcingElement`               | geometry | abstract reinforcing root under `IfcElementComponent`; `SteelGrade` carrier for the bar/mesh subtypes                                                              |
|  [06]   | `IfcReinforcingBar`                   | geometry | `IfcReinforcingElement`; public `NominalDiameter` (type-fallback get) / `CrossSectionArea` / `BarLength` (`double`), `PredefinedType` (`IfcReinforcingBarTypeEnum`) |
|  [07]   | `IfcReinforcingBarTypeEnum`           | geometry | `NOTDEFINED`/`USERDEFINED`/`MAIN`/`SHEAR`/`LIGATURE`/`STUD`/`PUNCHING`/`EDGE`/`RING`/`ANCHORING`/`SPACEBAR`(4x2) — `STUD` is the cast-in bar, NOT the welded connector |
|  [08]   | `IfcReinforcingMesh`                  | geometry | `IfcReinforcingElement`; public `MeshLength`/`MeshWidth`/`LongitudinalBarNominalDiameter`/`TransverseBarNominalDiameter`/`LongitudinalBarCrossSectionArea` (`double`) |
|  [09]   | `IfcMaterialProfileSetUsage`          | geometry | binds an `IfcMaterialProfileSet` to an element via `ForProfileSet`; `CardinalPoint` (`IfcCardinalPointReference`, default `MID`) + `ReferenceExtent` (`double`, default `NaN`) carry the profile placement the `Semantics/composition` `ProfileSetUsage` reciprocal reads, and the fastener nominal-diameter circle-profile channel  |
|  [10]   | `IfcCircleProfileDef`                 | geometry | parametric circular profile; public `Radius` (`double`) — the fastener nominal-diameter carrier reached through `IfcRelAssociatesMaterial.RelatingMaterial`         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DatabaseIfc — construction and read
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                             | [CAPABILITY]                                           |
| :-----: | :------------------------- | :--------------------------------------- | :----------------------------------------------------- |
|  [01]   | `new DatabaseIfc`          | `()`                                     | empty database at default schema                       |
|  [02]   | `new DatabaseIfc`          | `(string filePath)`                      | reads and parses an IFC file by path (format inferred) |
|  [03]   | `new DatabaseIfc`          | `(TextReader stream)`                    | reads and parses an IFC stream                         |
|  [04]   | `new DatabaseIfc`          | `(ReleaseVersion schema)`                | empty database at a chosen schema version              |
|  [05]   | `new DatabaseIfc`          | `(ModelView view)`                       | empty database at a chosen model view                  |
|  [06]   | `new DatabaseIfc`          | `(bool generate, ReleaseVersion schema)` | new database, optionally seeding common entities       |
|  [07]   | `new DatabaseIfc`          | `(bool generate, ModelView view)`        | new database for a model view, optionally seeded       |
|  [08]   | `new DatabaseIfc`          | `(DatabaseIfc db)`                       | copy-construct a database                              |
|  [09]   | `DatabaseIfc.ParseString`  | `(string)` → `DatabaseIfc` (static)      | parses an in-memory STEP/IFC string                    |
|  [10]   | `DatabaseIfc.ReadJSONFile` | `(string)` or `(TextReader)`             | loads IFC-JSON into the database                       |
|  [11]   | `DatabaseIfc.ReadJSON`     | `(JsonObject)` → `List<IBaseClassIfc>`   | parses an IFC-JSON document object                     |
|  [12]   | `DatabaseIfc.ReadXMLFile`  | `(string)`                               | loads IFC-XML into the database                        |
|  [13]   | `DatabaseIfc.ReadXMLDoc`   | `(XmlDocument)`                          | loads IFC-XML from a DOM                               |

[ENTRYPOINT_SCOPE]: DatabaseIfc — write and serialize
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SURFACE]                      | [CALL_SHAPE]                                | [CAPABILITY]                                |
| :-----: | :----------------------------- | :------------------------------------------ | :------------------------------------------ |
|  [01]   | `DatabaseIfc.WriteFile`        | `(string filePath)` → `bool`                | writes a STEP physical file by path         |
|  [02]   | `DatabaseIfc.WriteSTEPFile`    | `(string, SetProgressBarCallback)` → `bool` | writes STEP with progress callback          |
|  [03]   | `DatabaseIfc.WriteSTEPZipFile` | `(string, SetProgressBarCallback)` → `bool` | writes a zipped STEP file                   |
|  [04]   | `DatabaseIfc.WriteStream`      | `(Stream, string filename)` → `bool`        | writes STEP to a stream                     |
|  [05]   | `DatabaseIfc.ToString`         | `(FormatIfcSerialization)` → `string`       | serializes to `STEP`, `XML`, or `JSON` text |
|  [06]   | `DatabaseIfc.ToJSON`           | `(string filename)` → `JsonObject`          | builds an IFC-JSON document object          |
|  [07]   | `DatabaseIfc.ToJSON`           | `(string, BaseClassIfc.SetJsonOptions)`     | builds IFC-JSON with serialization options  |
|  [08]   | `DatabaseIfc.JSON`             | `()` → `JsonObject`                         | serializes the whole database to IFC-JSON   |
|  [09]   | `DatabaseIfc.XmlDocument`      | `()` → `XmlDocument`                        | builds an IFC-XML DOM                       |
|  [10]   | `DatabaseIfc.WriteXmlFile`     | `(string filename)` → `bool`                | writes an IFC-XML file                      |
|  [11]   | `DatabaseIfc.XmlString`        | `()` → `string`                             | serializes to IFC-XML text                  |

[ENTRYPOINT_SCOPE]: DatabaseIfc — model access and policy
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

`DatabaseIfc` exposes context, factory, policy, index, and enumeration surfaces; property return types stay implied by the symbol names and implementation topology.

| [INDEX] | [SURFACE]                                    | [CALL_SHAPE] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------- | :----------- | :----------------------------------------------- |
|  [01]   | `DatabaseIfc.Project`                        | property     | the root context as `IfcProject` when applicable |
|  [02]   | `DatabaseIfc.Context`                        | property     | the active context                               |
|  [03]   | `DatabaseIfc.Factory`                        | property     | per-database entity factory                      |
|  [04]   | `DatabaseIfc.Release`                        | property     | active schema version                            |
|  [05]   | `DatabaseIfc.ModelView`                      | property     | active model view                                |
|  [06]   | `DatabaseIfc.Tolerance`                      | property     | geometric tolerance                              |
|  [07]   | `DatabaseIfc.ToleranceAngleRadians`          | property     | angular tolerance in radians                     |
|  [08]   | `DatabaseIfc.ScaleAngle()`                   | method       | active angle scale factor (arity 0)              |
|  [09]   | `IfcUnitAssignment.ScaleSI(IfcUnitEnum)` / `(IfcDerivedUnitEnum)` | method | unit→SI scale factor; lives on the context's `IfcUnitAssignment`, NOT on `DatabaseIfc` |
|  [10]   | `DatabaseIfc.this[int stepId]`               | indexer      | entity by STEP record id                         |
|  [11]   | `DatabaseIfc.this[string globalID]`          | indexer      | entity by IFC GlobalId                           |
|  [12]   | `DatabaseIfc` enumeration                    | enumeration  | iterates all entities                            |
|  [13]   | `DatabaseSTEP<T>.NextObjectRecord`           | property     | next STEP record id                              |
|  [14]   | `DatabaseSTEP<T>.OriginatingFileInformation` | field        | source-file header metadata                      |

[ENTRYPOINT_SCOPE]: BaseClassIfc — traversal and entity serialization
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

`BaseClassIfc.Extract<T>` is constrained to `IBaseClassIfc`; lookup and serialization rows keep parameter detail out of cells.

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]           | [CAPABILITY]                                        |
| :-----: | :------------------------------------ | :--------------------- | :-------------------------------------------------- |
|  [01]   | `BaseClassIfc.Extract<T>`             | typed traversal        | collects all reachable entities of a type           |
|  [02]   | `BaseClassIfc.Database`               | property               | owning database                                     |
|  [03]   | `BaseClassIfc.GetType`                | static lookup          | resolves a CLR `Type` from an IFC class name        |
|  [04]   | `BaseClassIfc.Construct`              | static construction    | constructs an entity by IFC class name              |
|  [05]   | `BaseClassIfc.StringSTEP`             | STEP serialization     | serializes one entity to a STEP record line         |
|  [06]   | `BaseClassIfc.getJson`                | IFC-JSON serialization | serializes one entity to IFC-JSON                   |
|  [07]   | `IfcObjectDefinition.FindPropertySet` | named lookup           | resolves a property set by name                     |
|  [08]   | `IfcObjectDefinition.FindProperty`    | named lookup           | resolves a single property by name                  |
|  [09]   | `IfcObjectDefinition.AddAggregated`   | relationship mutation  | adds a part to a `IfcRelAggregates` decomposition   |
|  [10]   | `IfcProduct.AddElement`               | spatial mutation       | adds a contained product into the spatial structure |
|  [11]   | `IfcElement.SetMaterial`              | material assignment    | associates a material with an element               |

[ENTRYPOINT_SCOPE]: ParserIfc — codec operations
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                         | [CAPABILITY]                              |
| :-----: | :--------------------------- | :--------------------------------------------------- | :---------------------------------------- |
|  [01]   | `ParserIfc.ParseEnum<T>`     | `(string)` or `(string, string enumName)` → `T`      | parses an IFC enum literal                |
|  [02]   | `ParserIfc.DecodeGlobalID`   | `(string)` → `Guid`                                  | decodes a base64 IFC GlobalId to a `Guid` |
|  [03]   | `ParserIfc.EncodeGuid`       | `(Guid)` → `string`                                  | encodes a `Guid` to an IFC GlobalId       |
|  [04]   | `ParserIfc.HashGlobalID`     | `(string uniqueString)` → `string`                   | deterministic GlobalId from a stable key  |
|  [05]   | `ParserIfc.FormatLength`     | `(double, DatabaseIfc)` → `string`                   | formats a length per database units       |
|  [06]   | `ParserIfc.IdentifyIfcClass` | `(string, out string predefinedConstant)` → `string` | splits class name and predefined type     |

[ENTRYPOINT_SCOPE]: type-occurrence, predefined-type, and domain traversal
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

`PredefinedType` is a strongly-typed per-class enum member (`IfcWall.PredefinedType` is `IfcWallTypeEnum`, etc.); `IdentifyIfcClass` splits the predefined token from the class name at parse so the occurrence reads its predefined string without a per-class branch.

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]                                  | [CAPABILITY]                                              |
| :-----: | :----------------------------------- | :-------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `IfcObject.IsTypedBy`                | `IfcRelDefinesByType` set                     | the type-binding relationships of an occurrence           |
|  [02]   | `IfcRelDefinesByType.RelatingType`   | `IfcTypeObject` property                      | the type object an occurrence is bound to                 |
|  [03]   | `IfcTypeObject.HasPropertySets`      | `IfcPropertySetDefinition` set                | the type-bound property/quantity sets occurrences inherit |
|  [04]   | `IfcTypeProduct.RepresentationMaps`  | `IfcRepresentationMap` set                    | the type's reusable instanced-geometry library            |
|  [05]   | `IfcElementType.ElementType`         | `string` property                             | the type's predefined element-type identifier             |
|  [06]   | `IfcMappedItem.MappingSource`        | `IfcRepresentationMap` property               | the representation map an occurrence instances            |
|  [07]   | `IfcMappedItem.MappingTarget`        | `IfcCartesianTransformationOperator` property | the per-occurrence instance transform                     |
|  [08]   | `IfcWall.PredefinedType` (per class) | `IfcWallTypeEnum` (etc.) property             | the strongly-typed predefined-type member on each element |
|  [09]   | `IfcObject.ObjectType`               | `string` property                             | the user-defined type string for `USERDEFINED` predefined |

[ENTRYPOINT_SCOPE]: georeferencing, scheduling, and grouping traversal
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SURFACE]                                                  | [CALL_SHAPE]                                                                 | [CAPABILITY]                                                                                                                      |
| :-----: | :--------------------------------------------------------- | :--------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `IfcGeometricRepresentationContext.HasCoordinateOperation` | `IfcCoordinateOperation` property                                            | the single map-conversion/CRS operation on a context (`IfcMapConversion` is itself an `IfcCoordinateOperation`, narrowed by `as`) |
|  [02]   | `IfcMapConversion.SourceCRS`/`TargetCRS`                   | `IfcCoordinateReferenceSystemSelect`/`IfcCoordinateReferenceSystem` property | the source engineering frame and target projected CRS                                                                             |
|  [03]   | `IfcProject.RepresentationContexts`                        | `IfcRepresentationContext` set                                               | the project's representation contexts (model/plan)                                                                                |
|  [04]   | `IfcTask.TaskTime`                                         | `IfcTaskTime` property                                                       | the task's schedule/actual start-finish-duration                                                                                  |
|  [05]   | `IfcProcess.IsSuccessorFrom`/`IsPredecessorTo`             | `IfcRelSequence` set                                                         | the task's predecessor/successor dependency edges                                                                                 |
|  [06]   | `IfcRelSequence.TimeLag`                                   | `IfcLagTime` property                                                        | the dependency lag (duration + `IfcTaskDurationEnum`)                                                                             |
|  [07]   | `IfcGroup.IsGroupedBy`                                     | `IfcRelAssignsToGroup` set                                                   | the assignment relationships grouping objects into a group                                                                        |
|  [08]   | `IfcCostItem.CostValues`                                   | `IfcCostValue` set                                                           | the cost rates/values applied to a cost line item                                                                                 |
|  [09]   | `IfcDistributionPort.ContainedIn`                          | `IfcRelConnectsPortToElement` property                                       | the distribution element a port belongs to                                                                                        |

[ENTRYPOINT_SCOPE]: structural-connection realizing-element construction and read
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]                                                                                | [CAPABILITY]                                                                                                                       |
| :-----: | :----------------------------------------- | :----------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `new IfcRelConnectsWithRealizingElements`  | `(IfcConnectionGeometry cg, IfcElement relating, IfcElement related, IfcElement realizing)` | the single-realizing-element joint relation; auto-registers the back-pointer `IfcElement.IsConnectionRealization`                  |
|  [02]   | `IfcRelConnectsWithRealizingElements.RealizingElements` | `SET<IfcElement>` property (read-only; `.Add`/`.AddRange` mutate)               | the realizing-fastener/reinforcing set the `Projection/semantic#RELATION_ALGEBRA` `EdgeProjection` reads into the seam `Connect.Realizing` head                  |
|  [03]   | `new IfcMechanicalFastener`                | `(IfcObjectDefinition host, IfcObjectPlacement placement, IfcProductDefinitionShape rep)`   | the generic occurrence ctor (no single-arg `(DatabaseIfc)` ctor exists); host on `db.Project` at the factory root placement       |
|  [04]   | `new IfcMechanicalFastener`                | `(IfcProduct host, IfcMaterialProfileSetUsage profile, IfcAxis2Placement3D placement, double length)` | the profile-hosted ctor wiring the `IfcMaterialProfileSetUsage` nominal-diameter channel + length directly — the wire `Profiled` path |
|  [05]   | `IfcMechanicalFastener.PredefinedType`     | `IfcMechanicalFastenerTypeEnum` property (get/set; schema-validated on set)                  | the only public fastener scalar — the welded stud is `STUDSHEARCONNECTOR`, a discrete fastener `BOLT`/`ANCHORBOLT`                 |
|  [06]   | `IfcElement.HasAssociations`               | `SET<IfcRelAssociates>` property                                                            | the association set carrying `IfcRelAssociatesMaterial` — the read path to the fastener's profile-usage nominal diameter            |
|  [07]   | `IfcRelAssociatesMaterial.RelatingMaterial`| `IfcMaterialSelect` property (`IfcMaterialProfileSetUsage` arm)                              | the material-profile chain the `DiameterOf` projection narrows to recover the fastener nominal scalars                              |
|  [08]   | `IfcMaterialProfileSetUsage.ForProfileSet` | `IfcMaterialProfileSet` → `MaterialProfiles` (`IfcMaterialProfile` list) → `Profile`         | the profile chain to the `IfcCircleProfileDef.Radius` carrying the fastener nominal diameter (radius × 2)                          |
|  [09]   | `IfcReinforcingBar.NominalDiameter`        | `double` property (get falls back to `IfcReinforcingBarType.NominalDiameter`)               | the public bar diameter the `Semantics/connection#CONNECTION_DETAIL` `ConnectionProjection.Detail` reads onto the seam connection-detail bag                      |

## [04]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- ships a single managed `GeometryGymIFCcore.dll` per target framework; no P/Invoke runtime assets, no architecture-specific native binaries
- STEP/IFC data object graph only; no tessellation, no BREP evaluation, no geometry kernel
- I/O is text-format serialization (STEP physical file, IFC-XML, IFC-JSON) over `string`, `Stream`, and `TextReader`/`XmlDocument`; no binary IFC format

[IFC_IO]:
- read root: `new DatabaseIfc(path)` / `new DatabaseIfc(TextReader)` infer STEP/XML/JSON; `DatabaseIfc.ParseString` reads in-memory STEP/IFC text
- format-explicit read: `ReadJSONFile`/`ReadJSON` for IFC-JSON, `ReadXMLFile`/`ReadXMLDoc` for IFC-XML
- write root: `DatabaseIfc.WriteFile` emits STEP physical file; `DatabaseIfc.ToString(FormatIfcSerialization)` selects `STEP`, `XML`, or `JSON`
- schema is database-level state: set via `DatabaseIfc(ReleaseVersion)` / `DatabaseIfc(ModelView)` and read via `Release` / `ModelView`
- STEP physical-file header metadata is exposed via `STEPFileInformation` on `DatabaseSTEP<T>.OriginatingFileInformation`; fields: `FileDescriptionViewDefinition`/`FileDescriptionExchangeRequirements`/`FileDescriptions` (`List<string>`), `FileImplementationLevel` (`string`), `FileName` (`string`), `TimeStamp` (`DateTime`), `Author`/`Organization` (`List<string>`), `PreProcessorVersion`, `OriginatingSystem`, `Authorization` (`string`)

[AP242_STEP_READ]:
- The package reads all STEP physical file (`.ifc`, `.stp`) versions through `new DatabaseIfc(filePath)` without schema pre-selection; schema is auto-resolved from the file header
- IFC4.3 (`ReleaseVersion.IFC4X3_ADD2`) is the delivered IFC schema mapped to the AP242 domain overlap for building infrastructure; select this `ReleaseVersion` for infrastructure interchange or alignment geometry
- STEP read populates the in-memory entity graph; entities are accessible via `DatabaseIfc[stepId]` (integer), `DatabaseIfc[globalId]` (string), or full enumeration over `IEnumerable<BaseClassIfc>`
- AP242 geometry exchange topology: STEP entities map to `IfcTriangulatedFaceSet` / `IfcPolygonalFaceSet` (tessellation), `IfcExtrudedAreaSolid` (swept solid), `IfcAdvancedBrep` (NURBS BREP), `IfcFacetedBrep` (faceted BREP), and `IfcMappedItem` (instanced geometry)
- Material appearance on STEP import: `IfcStyledItem` → `IfcSurfaceStyle` → `IfcSurfaceStyleShading` / `IfcSurfaceStyleRendering` / `IfcSurfaceStyleWithTextures`; traverse via `BaseClassIfc.Extract<IfcStyledItem>()` then follow `.Item` to the geometry owner
- `IfcMaterialDefinitionRepresentation` binds an `IfcMaterial` to its `IfcStyledRepresentation`; retrieve via `material.HasRepresentation`

[MODEL_GRAPH]:
- repository: `DatabaseIfc` is the entity store, enumerable as `IEnumerable<BaseClassIfc>` and indexable by STEP id or GlobalId
- context: `DatabaseIfc.Context` is the active `IfcContext`; `DatabaseIfc.Project` narrows to `IfcProject` when the context is a project
- factory: `DatabaseIfc.Factory` (`FactoryIfc`) vends canonical axes, origins, placements, application, and owner history bound to that database
- traversal: `BaseClassIfc.Extract<T>()` collects all reachable entities assignable to `T`; entity reflection uses `BaseClassIfc.GetType`/`Construct`

[IDENTITY]:
- every `IfcRoot` carries `GlobalId`, `Guid`, `OwnerHistory`, `Name`, `Description`
- GlobalId codec lives on `ParserIfc`: `DecodeGlobalID`, `EncodeGuid`, `HashGlobalID`
- ownership stamp is `IfcOwnerHistory`, sourced through `FactoryIfc.OwnerHistoryAdded`

[GREENFIELD_AUTHORING]:
- project bootstrap: `new IfcProject(IfcBuilding|IfcSite|IfcFacility|IfcSpatialZone, name[, IfcUnitAssignment | IfcUnitAssignment.Length])` or `new IfcProject(DatabaseIfc, name)`
- spatial nesting: `IfcRelAggregates` via `IfcObjectDefinition.AddAggregated`; element placement via `IfcProduct.AddElement`
- property attachment: `IfcRelDefinesByProperties` binds an `IfcPropertySet`/`IfcElementQuantity` to objects; resolve with `FindPropertySet`/`FindProperty`
- material/type: `IfcRelAssociatesMaterial` and `IfcRelDefinesByType` bind material definitions and type objects to occurrences

[LOCAL_ADMISSION]:
- IFC import enters through `new DatabaseIfc(path|stream)` or format-explicit `Read*` calls.
- IFC export enters through `DatabaseIfc.WriteFile` or `DatabaseIfc.ToString(FormatIfcSerialization)`.
- model queries enter through `DatabaseIfc` indexing/enumeration and `BaseClassIfc.Extract<T>`.
- schema and model-view selection is database-level policy data, not a per-call argument.

[RAIL_LAW]:
- Package: `GeometryGymIFC_Core`
- Owns: buildingSMART IFC object model, schema-versioned STEP/XML/JSON serialization
- Accept: IFC data exchange, model authoring, model traversal and query
- Reject: tessellation, BREP evaluation, geometry kernel, native rendering
