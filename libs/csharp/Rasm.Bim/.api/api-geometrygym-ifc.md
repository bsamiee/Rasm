# [RASM_BIM_API_GEOMETRYGYM_IFC]

`GeometryGymIFC_Core` supplies a pure-managed buildingSMART IFC object model:
the `DatabaseIfc` repository, schema-versioned read/write across STEP, IFC-XML,
and IFC-JSON, the full IFC4.3 entity vocabulary, and `Extract<T>` graph traversal
for the Compute geometry interchange rail.

## [1]-[PACKAGE_SURFACE]

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

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: repository, factory, and serialization roots
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`, `GeometryGym.STEP`
- rail: geometry

| [INDEX] | [SYMBOL]              | [RAIL]   | [CAPABILITY]                                                                                 |
| :-----: | :-------------------- | :------- | :------------------------------------------------------------------------------------------- |
|   [1]   | `DatabaseIfc`         | geometry | IFC model repository; owns all entities, schema, units, tolerance, and I/O                   |
|   [2]   | `DatabaseSTEP<T>`     | geometry | generic STEP record store; `IEnumerable<T>`, `this[int stepId]`, `NextObjectRecord`          |
|   [3]   | `BaseClassIfc`        | geometry | abstract root of every IFC entity; carries `Database`, `Extract<T>`, STEP/JSON serialization |
|   [4]   | `STEPEntity`          | geometry | base STEP record carrier under `BaseClassIfc`                                                |
|   [5]   | `FactoryIfc`          | geometry | per-database factory; canonical axes, origins, placements, application, owner history        |
|   [6]   | `ParserIfc`           | geometry | static STEP/enum/GUID codec; `ParseEnum<T>`, `DecodeGlobalID`, `EncodeGuid`                  |
|   [7]   | `ParserSTEP`          | geometry | static low-level STEP token parser                                                           |
|   [8]   | `STEPFileInformation` | geometry | originating-file header metadata on the database                                             |
|   [9]   | `DuplicateOptions`    | geometry | options carrier for cross-database entity duplication                                        |
|  [10]   | `DuplicateMapping`    | geometry | source-to-target entity map during duplication                                               |

[PUBLIC_TYPE_SCOPE]: IFC schema vocabulary enums
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                   | [RAIL]   | [CAPABILITY]                                                                                                                                  |
| :-----: | :------------------------- | :------- | :-------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `ReleaseVersion`           | geometry | schema version: `IFC2x3`, `IFC4A2`, `IFC4X3`, `IFC4X3_ADD2`, `IFC4X4_DRAFT` (plus retired/withdrawn `[Obsolete]` members)                     |
|   [2]   | `ModelView`                | geometry | MVD selector: `Ifc4Reference`, `Ifc4DesignTransfer`, `Ifc2x3Coordination`, `IFC4X3Reference`, `IFC4X3AlignmentBasedView`, `Ifc4X3NotAssigned` |
|   [3]   | `FormatIfcSerialization`   | geometry | serialization format for `DatabaseIfc.ToString`: `STEP`, `XML`, `JSON`                                                                        |
|   [4]   | `IfcReflectanceMethodEnum` | geometry | PBR/Phong reflectance model: `BLINN`, `FLAT`, `GLASS`, `MATT`, `METAL`, `MIRROR`, `PHONG`, `PLASTIC`, `STRAUSS`, `NOTDEFINED`                 |
|   [5]   | `IfcSurfaceSide`           | geometry | surface-style application side: `POSITIVE`, `NEGATIVE`, `BOTH`                                                                                |
|   [6]   | `IfcZone` (no `PredefinedType`) | geometry | functional-zone grouping carries NO predefined enum in GeometryGym 25.7.30 (`IfcZone`/`IfcSystem`/`IfcGroup` expose only `LongName`/`Name`); there is no `IfcZoneTypeEnum` — only `IfcSpatialZone`/`IfcDistributionSystem`/`IfcStructuralLoadGroup` carry a predefined kind |
|   [7]   | `IfcSpatialZoneTypeEnum`   | geometry | spatial-zone kind: `NOTDEFINED`, `USERDEFINED`, `CONSTRUCTION`, `FIRESAFETY`, `LIGHTING`, `OCCUPANCY`, `SECURITY`, `THERMAL`, `VENTILATION`, `TRANSPORT`, `RESERVATION`, `INTERFERENCE`, … |
|   [8]   | `IfcDistributionSystemEnum`| geometry | distribution-system kind: `AIRCONDITIONING`, `ELECTRICAL`, `DOMESTICCOLDWATER`/`DOMESTICHOTWATER`, `DRAINAGE`, `FIREPROTECTION`, `VENTILATION`, … |
|   [9]   | `IfcFlowDirectionEnum`     | geometry | port flow direction: `SOURCE`, `SINK`, `SOURCEANDSINK`, `NOTDEFINED`                                                                          |
|  [10]   | `IfcSequenceEnum`          | geometry | task-dependency kind: `START_START`, `START_FINISH`, `FINISH_START`, `FINISH_FINISH`, `USERDEFINED`, `NOTDEFINED`                             |
|  [11]   | `IfcTaskDurationEnum`      | geometry | task-duration interpretation: `ELAPSEDTIME`, `WORKTIME`, `NOTDEFINED`                                                                         |
|  [12]   | `IfcCostScheduleTypeEnum`  | geometry | cost-schedule kind: `BUDGET`, `COSTPLAN`, `ESTIMATE`, `TENDER`, `PRICEDBILLOFQUANTITIES`, `SCHEDULEOFRATES`, …                                |
|  [13]   | `IfcStructuralCurveMemberTypeEnum` | geometry | idealized 1D member kind: `RIGID_JOINED_MEMBER`, `PIN_JOINED_MEMBER`, `CABLE`, `TENSION_MEMBER`, `COMPRESSION_MEMBER`, …                |
|  [14]   | `IfcLoadGroupTypeEnum`     | geometry | load-group kind: `LOAD_GROUP`, `LOAD_CASE`, `LOAD_COMBINATION`, `USERDEFINED`, `NOTDEFINED`                                                   |

[PUBLIC_TYPE_SCOPE]: IFC kernel root entities
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                   | [RAIL]   | [CAPABILITY]                                                                           |
| :-----: | :------------------------- | :------- | :------------------------------------------------------------------------------------- |
|   [1]   | `IfcRoot`                  | geometry | rooted entity base; `GlobalId`, `Guid`, `OwnerHistory`, `Name`, `Description`          |
|   [2]   | `IfcObjectDefinition`      | geometry | object base; `Nests`, `Decomposes`, `AddAggregated`, `FindProperty`, `FindPropertySet` |
|   [3]   | `IfcContext`               | geometry | shared context; `UnitsInContext`, `AddDeclared`, `DeclaredTypes`                       |
|   [4]   | `IfcProject`               | geometry | project root context; ctors over building/site/facility/zone + units; `UppermostSite`  |
|   [5]   | `IfcProjectLibrary`        | geometry | shared declaration library context                                                     |
|   [6]   | `IfcObject`                | geometry | occurrence object base under `IfcObjectDefinition`                                     |
|   [7]   | `IfcProduct`               | geometry | spatially located product; `ObjectPlacement`, `Representation`, `AddElement`           |
|   [8]   | `IfcElement`               | geometry | physical element base; `Tag`, `MaterialSelect`, `SetMaterial`                          |
|   [9]   | `IfcOwnerHistory`          | geometry | change-tracking stamp; creation/modification metadata                                  |
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
|   [1]   | `IfcSpatialElement`          | geometry | spatial container base; `LongName`, `ReferenceElement`                  |
|   [2]   | `IfcSpatialStructureElement` | geometry | spatial hierarchy node; `CompositionType` (`IfcElementCompositionEnum`) |
|   [3]   | `IfcSite`                    | geometry | site spatial structure element                                          |
|   [4]   | `IfcBuilding`                | geometry | building spatial structure element                                      |
|   [5]   | `IfcBuildingStorey`          | geometry | storey spatial structure element                                        |
|   [6]   | `IfcFacility`                | geometry | IFC4.3 facility spatial structure base                                  |
|   [7]   | `IfcFacilityPart`            | geometry | IFC4.3 facility-part subdivision                                        |
|   [8]   | `IfcExternalSpatialElement`  | geometry | external (outside-facility) spatial element                             |
|   [9]   | `IfcTypeObject`              | geometry | type-definition base under `IfcObjectDefinition`                        |
|  [10]   | `IfcTypeProduct`             | geometry | product type with representation maps                                   |
|  [11]   | `IfcElementType`             | geometry | element type base under `IfcTypeProduct`                                |
|  [12]   | `IfcBuiltElementType`        | geometry | built-element type base                                                 |

[PUBLIC_TYPE_SCOPE]: property-set, quantity, and material families
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                     | [RAIL]   | [CAPABILITY]                                                                      |
| :-----: | :--------------------------- | :------- | :-------------------------------------------------------------------------------- |
|   [1]   | `IfcPropertySetDefinition`   | geometry | property/quantity set base                                                        |
|   [2]   | `IfcPropertySet`             | geometry | named set of `IfcProperty` instances                                              |
|   [3]   | `IfcPropertySingleValue`     | geometry | single-value property (`IfcSimpleProperty` subtype)                               |
|   [4]   | `IfcPropertyEnumeratedValue` | geometry | enumeration-referenced property value                                             |
|   [5]   | `IfcComplexProperty`         | geometry | nested property aggregate                                                         |
|   [6]   | `IfcQuantitySet`             | geometry | quantity-set base under `IfcPropertySetDefinition`                                |
|   [7]   | `IfcElementQuantity`         | geometry | named set of physical quantities                                                  |
|   [8]   | `IfcPhysicalSimpleQuantity`  | geometry | simple quantity base; `IfcQuantityLength`/`Area`/`Volume`/`Weight`/`Count`/`Time` |
|   [9]   | `IfcPropertySetTemplate`     | geometry | reusable property-set template                                                    |
|  [10]   | `IfcMaterial`                | geometry | named material definition                                                         |
|  [11]   | `IfcMaterialLayerSet`        | geometry | ordered material layer assembly                                                   |
|  [12]   | `IfcMaterialLayerSetUsage`   | geometry | layer-set application to an element                                               |
|  [13]   | `IfcMaterialProfileSet`      | geometry | material-profile assembly for linear members                                      |
|  [14]   | `IfcMaterialConstituentSet`  | geometry | named constituent material set                                                    |

[PUBLIC_TYPE_SCOPE]: relationship families
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                            | [RAIL]   | [CAPABILITY]                                              |
| :-----: | :---------------------------------- | :------- | :-------------------------------------------------------- |
|   [1]   | `IfcRelationship`                   | geometry | objectified-relationship root                             |
|   [2]   | `IfcRelAggregates`                  | geometry | whole-part decomposition (`IfcRelDecomposes` subtype)     |
|   [3]   | `IfcRelNests`                       | geometry | ordered nesting decomposition                             |
|   [4]   | `IfcRelContainedInSpatialStructure` | geometry | element-to-spatial containment (`IfcRelConnects` subtype) |
|   [5]   | `IfcRelDefinesByProperties`         | geometry | binds a property/quantity set to objects                  |
|   [6]   | `IfcRelDefinesByType`               | geometry | binds occurrences to a type object                        |
|   [7]   | `IfcRelAssociatesMaterial`          | geometry | associates a material definition with objects             |
|   [8]   | `IfcRelAssociatesClassification`    | geometry | associates a classification reference                     |
|   [9]   | `IfcRelDeclares`                    | geometry | declares definitions within a context                     |
|  [10]   | `IfcRelVoidsElement`                | geometry | subtracts an opening from an element                      |
|  [11]   | `IfcRelConnectsElements`            | geometry | physical element-to-element connection                    |

[PUBLIC_TYPE_SCOPE]: architectural built-element family
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]          | [RAIL]   | [CAPABILITY]                                                                                |
| :-----: | :---------------- | :------- | :------------------------------------------------------------------------------------------ |
|   [1]   | `IfcCovering`     | geometry | finish covering built element; `PredefinedType` (`IfcCoveringTypeEnum`)                      |
|   [2]   | `IfcCurtainWall`  | geometry | curtain-wall built element aggregating plate/member units; `PredefinedType`                  |
|   [3]   | `IfcRailing`      | geometry | railing built element; `PredefinedType` (`IfcRailingTypeEnum`)                               |
|   [4]   | `IfcRamp`         | geometry | ramp built element; `PredefinedType` (`IfcRampTypeEnum`)                                     |
|   [5]   | `IfcRoof`         | geometry | roof built element; `PredefinedType` (`IfcRoofTypeEnum`)                                     |
|   [6]   | `IfcStair`        | geometry | stair built element; `PredefinedType` (`IfcStairTypeEnum`)                                   |
|   [7]   | `IfcPlate`        | geometry | planar built element (curtain-wall panel); `PredefinedType` (`IfcPlateTypeEnum`)             |
|   [8]   | `IfcMember`       | geometry | structural-member built element (mullion/post); `PredefinedType` (`IfcMemberTypeEnum`)       |
|   [9]   | `IfcFooting`      | geometry | foundation footing built element; `PredefinedType` (`IfcFootingTypeEnum`)                    |
|  [10]   | `IfcPile`         | geometry | foundation pile built element; `PredefinedType` (`IfcPileTypeEnum`)                          |
|  [11]   | `IfcWall`         | geometry | wall built element; `PredefinedType` (`IfcWallTypeEnum`: STANDARD/SHEAR/PARTITIONING/…)      |
|  [12]   | `IfcSlab`         | geometry | slab built element; `PredefinedType` (`IfcSlabTypeEnum`: FLOOR/ROOF/LANDING/BASESLAB)        |
|  [13]   | `IfcColumn`       | geometry | column built element; `PredefinedType` (`IfcColumnTypeEnum`)                                 |
|  [14]   | `IfcBeam`         | geometry | beam built element; `PredefinedType` (`IfcBeamTypeEnum`)                                     |
|  [15]   | `IfcDoor`         | geometry | door built element; `PredefinedType` (`IfcDoorTypeEnum`) + `OperationType`                   |
|  [16]   | `IfcWindow`       | geometry | window built element; `PredefinedType` (`IfcWindowTypeEnum`) + `PartitioningType`            |
|  [17]   | `IfcSpace`        | geometry | space spatial element; `PredefinedType` (`IfcSpaceTypeEnum`: INTERNAL/EXTERNAL/PARKING/…)    |

[PUBLIC_TYPE_SCOPE]: MEP distribution-element family
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                | [RAIL]   | [CAPABILITY]                                                                                       |
| :-----: | :---------------------- | :------- | :------------------------------------------------------------------------------------------------- |
|   [1]   | `IfcDistributionElement`| geometry | MEP distribution-element base under `IfcElement`; carries `HasPorts` (`IfcRelConnectsPortToElement`)|
|   [2]   | `IfcFlowSegment`        | geometry | flow segment (duct/pipe/cable run); `PredefinedType`                                                |
|   [3]   | `IfcFlowFitting`        | geometry | flow fitting (elbow/tee/junction); `PredefinedType`                                                 |
|   [4]   | `IfcFlowTerminal`       | geometry | flow terminal (air/sanitary/light fixture); `PredefinedType` (AIRTERMINAL/SANITARYTERMINAL/…)      |
|   [5]   | `IfcFlowController`     | geometry | flow controller (valve/damper/switch); `PredefinedType`                                             |
|   [6]   | `IfcFlowMovingDevice`   | geometry | flow moving device (pump/fan/compressor); `PredefinedType`                                          |
|   [7]   | `IfcFlowStorageDevice`  | geometry | flow storage device (tank/battery); `PredefinedType`                                                |
|   [8]   | `IfcEnergyConversionDevice`| geometry | energy conversion device (boiler/chiller/coil); `PredefinedType`                                  |
|   [9]   | `IfcDistributionPort`   | geometry | typed connection port; `FlowDirection` (`IfcFlowDirectionEnum`), `SystemType`, `PredefinedType`     |
|  [10]   | `IfcPort`               | geometry | abstract connection-port base; `ContainedIn`, `ConnectedFrom`, `ConnectedTo`                        |

[PUBLIC_TYPE_SCOPE]: structural-analysis-domain family
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                          | [RAIL]   | [CAPABILITY]                                                                                  |
| :-----: | :-------------------------------- | :------- | :-------------------------------------------------------------------------------------------- |
|   [1]   | `IfcStructuralAnalysisModel`      | geometry | structural-analysis container; `OrientationOf2DPlane`, `LoadedBy`, `HasResults`, `SharedPlacement`|
|   [2]   | `IfcStructuralItem`               | geometry | structural-item base; `AssignedStructuralActivity`, `AssignedToStructuralItem`                 |
|   [3]   | `IfcStructuralMember`             | geometry | idealized structural member base under `IfcStructuralItem`                                     |
|   [4]   | `IfcStructuralCurveMember`        | geometry | 1D idealized member (beam/column line); `PredefinedType` (`IfcStructuralCurveMemberTypeEnum`)  |
|   [5]   | `IfcStructuralSurfaceMember`      | geometry | 2D idealized member (slab/wall surface); `PredefinedType`, `Thickness`                         |
|   [6]   | `IfcStructuralConnection`        | geometry | structural connection base; `AppliedCondition` (`IfcBoundaryCondition`)                        |
|   [7]   | `IfcStructuralPointConnection`    | geometry | point support/connection node                                                                 |
|   [8]   | `IfcStructuralCurveConnection`    | geometry | curve support/connection edge                                                                 |
|   [9]   | `IfcStructuralSurfaceConnection`  | geometry | surface support/connection                                                                     |
|  [10]   | `IfcStructuralLoadGroup`          | geometry | grouped structural loads; `PredefinedType`, `ActionType`, `ActionSource`, `SourceOfResultGroup`|
|  [11]   | `IfcStructuralLoadCase`           | geometry | load case under `IfcStructuralLoadGroup`; `SelfWeightCoefficients`                             |
|  [12]   | `IfcStructuralResultGroup`        | geometry | grouped analysis results                                                                       |
|  [13]   | `IfcBoundaryCondition`            | geometry | boundary-condition base (`IfcBoundaryNodeCondition`/`IfcBoundaryEdgeCondition`)                |
|  [14]   | `IfcRelConnectsStructuralMember`  | geometry | connects an idealized member to a connection; `RelatingStructuralMember`, `RelatedStructuralConnection`|
|  [15]   | `IfcRelConnectsStructuralActivity`| geometry | binds a load/result activity to a structural item                                              |
|  [16]   | `IfcRelAssignsToGroup`            | geometry | assigns objects to an `IfcGroup`/`IfcSystem`/`IfcStructuralLoadGroup`                          |

[PUBLIC_TYPE_SCOPE]: grouping, zone, and distribution-system family
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                              | [RAIL]   | [CAPABILITY]                                                                                  |
| :-----: | :------------------------------------ | :------- | :-------------------------------------------------------------------------------------------- |
|   [1]   | `IfcGroup`                            | geometry | non-spatial logical grouping base; `IsGroupedBy` (`IfcRelAssignsToGroup`)                      |
|   [2]   | `IfcSystem`                           | geometry | functional system grouping under `IfcGroup`; `ServicesBuildings`                              |
|   [3]   | `IfcBuildingSystem`                   | geometry | building-system grouping; `PredefinedType` (`IfcBuildingSystemTypeEnum`)                       |
|   [4]   | `IfcDistributionSystem`               | geometry | MEP distribution system; `PredefinedType` (`IfcDistributionSystemEnum`), member element set    |
|   [5]   | `IfcZone`                             | geometry | functional zone aggregating spaces across storeys; `PredefinedType`                            |
|   [6]   | `IfcSpatialZone`                      | geometry | fire/thermal/construction/occupancy zone; `PredefinedType` (`IfcSpatialZoneTypeEnum`)          |
|   [7]   | `IfcRelReferencedInSpatialStructure`  | geometry | references an element into a spatial structure it is not contained in; many-to-many overlay     |
|   [8]   | `IfcRelServicesBuildings`             | geometry | binds a system to the spatial structures it serves                                             |
|   [9]   | `IfcRelConnectsPortToElement`         | geometry | connects an `IfcDistributionPort` to its owning distribution element                           |
|  [10]   | `IfcRelConnectsPorts`                 | geometry | port-to-port connection edge; `RelatingPort`, `RelatedPort`, `RealizingElement`                |

[PUBLIC_TYPE_SCOPE]: scheduling, cost, and resource family
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                       | [RAIL]   | [CAPABILITY]                                                                                  |
| :-----: | :----------------------------- | :------- | :-------------------------------------------------------------------------------------------- |
|   [1]   | `IfcProcess`                   | geometry | process base; `IsSuccessorFrom`/`IsPredecessorTo` (`IfcRelSequence`), `OperatesOn`            |
|   [2]   | `IfcTask`                      | geometry | scheduled task; `Status`, `WorkMethod`, `IsMilestone`, `TaskTime`, `PredefinedType`           |
|   [3]   | `IfcTaskTime`                  | geometry | task schedule times; `ScheduleStart`/`ScheduleFinish`/`ScheduleDuration`, `ActualStart`/`ActualFinish`|
|   [4]   | `IfcWorkSchedule`             | geometry | work schedule under `IfcWorkControl`; `Controls`, `PredefinedType`                            |
|   [5]   | `IfcWorkPlan`                  | geometry | work plan grouping schedules under `IfcWorkControl`; `PredefinedType`                          |
|   [6]   | `IfcWorkCalendar`             | geometry | working/exception time calendar; `WorkingTimes`, `ExceptionTimes`                             |
|   [7]   | `IfcRelSequence`               | geometry | task dependency edge; `RelatingProcess`, `RelatedProcess`, `TimeLag`, `SequenceType`           |
|   [8]   | `IfcRelAssignsToProcess`       | geometry | assigns products/resources to a process; `RelatingProcess`, `RelatedObjects`                  |
|   [9]   | `IfcCostSchedule`             | geometry | cost schedule under `IfcControl`; `Controls`, `PredefinedType`, `SubmittedOn`                  |
|  [10]   | `IfcCostItem`                  | geometry | cost line item; `CostValues` (`IfcCostValue`), `CostQuantities`, `PredefinedType`              |
|  [11]   | `IfcCostValue`                 | geometry | applied cost value/rate; `AppliedValue` (`IfcAppliedValue`), `UnitBasis`, `Category`           |
|  [12]   | `IfcConstructionResource`      | geometry | construction-resource base; `Usage`, `BaseCosts`, `BaseQuantity`                              |
|  [13]   | `IfcLaborResource`             | geometry | labor resource; `PredefinedType` (`IfcLaborResourceTypeEnum`)                                 |
|  [14]   | `IfcConstructionMaterialResource`| geometry | material resource; `PredefinedType` (`IfcConstructionMaterialResourceTypeEnum`)             |
|  [15]   | `IfcConstructionEquipmentResource`| geometry | equipment resource; `PredefinedType`                                                        |
|  [16]   | `IfcRelAssignsToControl`       | geometry | assigns objects to a control (cost item/schedule); `RelatingControl`, `RelatedObjects`        |

[PUBLIC_TYPE_SCOPE]: georeferencing and map-conversion entities
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                          | [RAIL]   | [CAPABILITY]                                                                                  |
| :-----: | :-------------------------------- | :------- | :-------------------------------------------------------------------------------------------- |
|   [1]   | `IfcCoordinateOperation`          | geometry | coordinate-operation base; `SourceCRS`, `TargetCRS`                                            |
|   [2]   | `IfcMapConversion`                | geometry | rigid map-conversion offset; `Eastings`, `Northings`, `OrthogonalHeight`, `XAxisAbscissa`, `XAxisOrdinate`, `Scale`|
|   [3]   | `IfcCoordinateReferenceSystem`    | geometry | CRS base; `Name`, `GeodeticDatum`, `VerticalDatum`                                             |
|   [4]   | `IfcProjectedCRS`                 | geometry | projected CRS; `Name` (EPSG), `GeodeticDatum`, `VerticalDatum`, `MapProjection`, `MapZone`, `MapUnit`|
|   [5]   | `IfcMapConversionScaled`          | geometry | per-axis scaled map conversion (IFC4.3 ADD2); `FactorX`/`FactorY`/`FactorZ`                    |

[PUBLIC_TYPE_SCOPE]: IFC4.3 infrastructure entities — alignment and facility
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                      | [RAIL]   | [CAPABILITY]                                     |
| :-----: | :---------------------------- | :------- | :----------------------------------------------- |
|   [1]   | `IfcAlignment`                | geometry | linear-referencing alignment positioning element |
|   [2]   | `IfcAlignmentHorizontal`      | geometry | horizontal alignment layout                      |
|   [3]   | `IfcAlignmentVertical`        | geometry | vertical alignment layout                        |
|   [4]   | `IfcAlignmentCant`            | geometry | rail cant alignment layout                       |
|   [5]   | `IfcAlignmentSegment`         | geometry | one parameterized alignment segment              |
|   [6]   | `IfcLinearPlacement`          | geometry | placement along a curve via distance expression  |
|   [7]   | `IfcLinearPositioningElement` | geometry | base for linear referencing positioning          |
|   [8]   | `IfcReferent`                 | geometry | referent point along an alignment                |
|   [9]   | `IfcBridge`                   | geometry | bridge facility                                  |
|  [10]   | `IfcRailway`                  | geometry | railway facility                                 |
|  [11]   | `IfcRoad`                     | geometry | road facility                                    |
|  [12]   | `IfcMarineFacility`           | geometry | marine facility                                  |

[PUBLIC_TYPE_SCOPE]: IFC4.3 infrastructure entities — earthworks and geotechnics
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                 | [RAIL]   | [CAPABILITY]                                    |
| :-----: | :----------------------- | :------- | :---------------------------------------------- |
|   [1]   | `IfcCourse`              | geometry | layered pavement/earthwork course built element |
|   [2]   | `IfcPavement`            | geometry | pavement built element                          |
|   [3]   | `IfcRail`                | geometry | rail built element                              |
|   [4]   | `IfcEarthworksFill`      | geometry | earthworks fill element                         |
|   [5]   | `IfcEarthworksCut`       | geometry | earthworks excavation element                   |
|   [6]   | `IfcGeotechnicalStratum` | geometry | geotechnical soil/rock stratum                  |
|   [7]   | `IfcBorehole`            | geometry | geotechnical borehole assembly                  |

[PUBLIC_TYPE_SCOPE]: geometry representation entities
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                            | [RAIL]   | [CAPABILITY]                                     |
| :-----: | :---------------------------------- | :------- | :----------------------------------------------- |
|   [1]   | `IfcProductDefinitionShape`         | geometry | product shape; aggregates representations        |
|   [2]   | `IfcRepresentation`                 | geometry | one geometric/topological representation         |
|   [3]   | `IfcGeometricRepresentationContext` | geometry | coordinate space + precision for representations |
|   [4]   | `IfcCartesianPoint`                 | geometry | n-D cartesian point                              |
|   [5]   | `IfcCartesianPointList3D`           | geometry | packed 3D point list                             |
|   [6]   | `IfcDirection`                      | geometry | direction vector                                 |
|   [7]   | `IfcAxis2Placement3D`               | geometry | 3D placement (location + axes)                   |
|   [8]   | `IfcLocalPlacement`                 | geometry | relative object placement                        |
|   [9]   | `IfcExtrudedAreaSolid`              | geometry | swept-area extrusion solid                       |
|  [10]   | `IfcPolygonalFaceSet`               | geometry | indexed polygonal face mesh                      |
|  [11]   | `IfcTriangulatedFaceSet`            | geometry | indexed triangle mesh                            |
|  [12]   | `IfcFacetedBrep`                    | geometry | faceted boundary-representation solid            |
|  [13]   | `IfcAdvancedBrep`                   | geometry | NURBS-faced boundary-representation solid        |
|  [14]   | `IfcBooleanResult`                  | geometry | CSG boolean operation result                     |
|  [15]   | `IfcMappedItem`                     | geometry | instanced representation map reference            |
|  [16]   | `IfcRepresentationMap`              | geometry | reusable type-bound geometry library; `MappingOrigin` (`IfcAxis2Placement`), `MappedRepresentation` (`IfcRepresentation`), `HasShapeAspects`, referenced by `IfcMappedItem.MappingSource`|

[PUBLIC_TYPE_SCOPE]: tessellation geometry — AP242/IFC4.3 mesh interchange
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                  | [RAIL]   | [CAPABILITY]                                                                           |
| :-----: | :------------------------ | :------- | :------------------------------------------------------------------------------------- |
|   [1]   | `IfcTessellatedItem`      | geometry | abstract tessellated geometry item base; derives from `IfcGeometricRepresentationItem` |
|   [2]   | `IfcTessellatedFaceSet`   | geometry | abstract indexed face mesh base; `Closed`, `HasColours`, `HasTextures` properties      |
|   [3]   | `IfcTriangulatedFaceSet`  | geometry | triangle mesh: `CoordIndex`, `Normals`, `NormalIndex`, `PnIndex`                       |
|   [4]   | `IfcPolygonalFaceSet`     | geometry | polygon mesh (already catalogued); paired here as `IfcTessellatedFaceSet` subtype      |
|   [5]   | `IfcCartesianPointList`   | geometry | abstract packed point list base                                                        |
|   [6]   | `IfcCartesianPointList3D` | geometry | packed 3D point list; used as `Coordinates` by tessellated face sets                   |
|   [7]   | `IfcCartesianPointList2D` | geometry | packed 2D point list                                                                   |

[PUBLIC_TYPE_SCOPE]: material appearance and presentation interchange
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                              | [RAIL]   | [CAPABILITY]                                                                                                             |
| :-----: | :------------------------------------ | :------- | :----------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `IfcPresentationStyle`                | geometry | abstract presentation style root                                                                                         |
|   [2]   | `IfcPresentationItem`                 | geometry | abstract item within a presentation style                                                                                |
|   [3]   | `IfcSurfaceStyle`                     | geometry | surface style; holds `Side` (`IfcSurfaceSide`) and `Styles` element set                                                  |
|   [4]   | `IfcSurfaceStyleShading`              | geometry | base shading style; `SurfaceColour` (`IfcColourRgb`), `Transparency` (`double`)                                          |
|   [5]   | `IfcSurfaceStyleRendering`            | geometry | extends shading with PBR parameters: `DiffuseColour`, `SpecularColour`, `ReflectanceMethod` (`IfcReflectanceMethodEnum`) |
|   [6]   | `IfcSurfaceStyleWithTextures`         | geometry | texture style; references `IfcSurfaceTexture` instances                                                                  |
|   [7]   | `IfcSurfaceStyleLighting`             | geometry | additional lighting coefficients: ambient, diffuse, transmission, reflectance                                            |
|   [8]   | `IfcSurfaceStyleRefraction`           | geometry | refraction index and light-transmission factor for optical materials                                                     |
|   [9]   | `IfcSurfaceTexture`                   | geometry | abstract surface texture; `RepeatS`, `RepeatT`, `Mode`, `TextureTransform`                                               |
|  [10]   | `IfcImageTexture`                     | geometry | file-path-referenced image texture (`IfcSurfaceTexture` subtype)                                                         |
|  [11]   | `IfcPixelTexture`                     | geometry | inline pixel-encoded texture (`IfcSurfaceTexture` subtype)                                                               |
|  [12]   | `IfcBlobTexture`                      | geometry | binary blob texture (`IfcSurfaceTexture` subtype); `RasterCode`, `RasterFormat`                                          |
|  [13]   | `IfcStyledItem`                       | geometry | binds a style to a representation item                                                                                   |
|  [14]   | `IfcStyledRepresentation`             | geometry | representation holding only styled items                                                                                 |
|  [15]   | `IfcMaterialDefinitionRepresentation` | geometry | links an `IfcMaterial` to its `IfcStyledRepresentation` set                                                              |
|  [16]   | `IfcColourRgb`                        | geometry | RGB colour value                                                                                                         |
|  [17]   | `IfcColourRgbList`                    | geometry | packed list of RGB colour triples for indexed colour sets                                                                |

[PUBLIC_TYPE_SCOPE]: units, presentation, and attributes
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SYMBOL]                     | [RAIL]   | [CAPABILITY]                                                 |
| :-----: | :--------------------------- | :------- | :----------------------------------------------------------- |
|   [1]   | `IfcUnitAssignment`          | geometry | per-context unit set; nested `Length` enum for project ctors |
|   [2]   | `IfcSIUnit`                  | geometry | SI base/derived unit                                         |
|   [3]   | `IfcConversionBasedUnit`     | geometry | unit defined by conversion factor                            |
|   [4]   | `IfcDerivedUnit`             | geometry | compound derived unit                                        |
|   [5]   | `IfcMonetaryUnit`            | geometry | currency unit                                                |
|   [6]   | `IfcMeasureWithUnit`         | geometry | value bound to a unit                                        |
|   [7]   | `IfcClassificationReference` | geometry | reference to an external classification                      |
|   [8]   | `VersionAddedAttribute`      | geometry | reflection attribute marking schema-version availability     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DatabaseIfc — construction and read
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]                             | [CAPABILITY]                                           |
| :-----: | :------------------------- | :--------------------------------------- | :----------------------------------------------------- |
|   [1]   | `new DatabaseIfc`          | `()`                                     | empty database at default schema                       |
|   [2]   | `new DatabaseIfc`          | `(string filePath)`                      | reads and parses an IFC file by path (format inferred) |
|   [3]   | `new DatabaseIfc`          | `(TextReader stream)`                    | reads and parses an IFC stream                         |
|   [4]   | `new DatabaseIfc`          | `(ReleaseVersion schema)`                | empty database at a chosen schema version              |
|   [5]   | `new DatabaseIfc`          | `(ModelView view)`                       | empty database at a chosen model view                  |
|   [6]   | `new DatabaseIfc`          | `(bool generate, ReleaseVersion schema)` | new database, optionally seeding common entities       |
|   [7]   | `new DatabaseIfc`          | `(bool generate, ModelView view)`        | new database for a model view, optionally seeded       |
|   [8]   | `new DatabaseIfc`          | `(DatabaseIfc db)`                       | copy-construct a database                              |
|   [9]   | `DatabaseIfc.ParseString`  | `(string)` → `DatabaseIfc` (static)      | parses an in-memory STEP/IFC string                    |
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
|   [1]   | `DatabaseIfc.WriteFile`        | `(string filePath)` → `bool`                | writes a STEP physical file by path         |
|   [2]   | `DatabaseIfc.WriteSTEPFile`    | `(string, SetProgressBarCallback)` → `bool` | writes STEP with progress callback          |
|   [3]   | `DatabaseIfc.WriteSTEPZipFile` | `(string, SetProgressBarCallback)` → `bool` | writes a zipped STEP file                   |
|   [4]   | `DatabaseIfc.WriteStream`      | `(Stream, string filename)` → `bool`        | writes STEP to a stream                     |
|   [5]   | `DatabaseIfc.ToString`         | `(FormatIfcSerialization)` → `string`       | serializes to `STEP`, `XML`, or `JSON` text |
|   [6]   | `DatabaseIfc.ToJSON`           | `(string filename)` → `JsonObject`          | builds an IFC-JSON document object          |
|   [7]   | `DatabaseIfc.ToJSON`           | `(string, BaseClassIfc.SetJsonOptions)`     | builds IFC-JSON with serialization options  |
|   [8]   | `DatabaseIfc.JSON`             | `()` → `JsonObject`                         | serializes the whole database to IFC-JSON   |
|   [9]   | `DatabaseIfc.XmlDocument`      | `()` → `XmlDocument`                        | builds an IFC-XML DOM                       |
|  [10]   | `DatabaseIfc.WriteXmlFile`     | `(string filename)` → `bool`                | writes an IFC-XML file                      |
|  [11]   | `DatabaseIfc.XmlString`        | `()` → `string`                             | serializes to IFC-XML text                  |

[ENTRYPOINT_SCOPE]: DatabaseIfc — model access and policy
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

`DatabaseIfc` exposes context, factory, policy, index, and enumeration surfaces; property return types stay implied by the symbol names and implementation topology.

| [INDEX] | [SURFACE]                                    | [CALL_SHAPE] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------- | :----------- | :----------------------------------------------- |
|   [1]   | `DatabaseIfc.Project`                        | property     | the root context as `IfcProject` when applicable |
|   [2]   | `DatabaseIfc.Context`                        | property     | the active context                               |
|   [3]   | `DatabaseIfc.Factory`                        | property     | per-database entity factory                      |
|   [4]   | `DatabaseIfc.Release`                        | property     | active schema version                            |
|   [5]   | `DatabaseIfc.ModelView`                      | property     | active model view                                |
|   [6]   | `DatabaseIfc.Tolerance`                      | property     | geometric tolerance                              |
|   [7]   | `DatabaseIfc.ToleranceAngleRadians`          | property     | angular tolerance in radians                     |
|   [8]   | `DatabaseIfc.ScaleSI`                        | property     | SI length scale                                  |
|   [9]   | `DatabaseIfc.ScaleAngle`                     | scale call   | active angle scale factor                        |
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
|   [1]   | `BaseClassIfc.Extract<T>`             | typed traversal        | collects all reachable entities of a type           |
|   [2]   | `BaseClassIfc.Database`               | property               | owning database                                     |
|   [3]   | `BaseClassIfc.GetType`                | static lookup          | resolves a CLR `Type` from an IFC class name        |
|   [4]   | `BaseClassIfc.Construct`              | static construction    | constructs an entity by IFC class name              |
|   [5]   | `BaseClassIfc.StringSTEP`             | STEP serialization     | serializes one entity to a STEP record line         |
|   [6]   | `BaseClassIfc.getJson`                | IFC-JSON serialization | serializes one entity to IFC-JSON                   |
|   [7]   | `IfcObjectDefinition.FindPropertySet` | named lookup           | resolves a property set by name                     |
|   [8]   | `IfcObjectDefinition.FindProperty`    | named lookup           | resolves a single property by name                  |
|   [9]   | `IfcObjectDefinition.AddAggregated`   | relationship mutation  | adds a part to a `IfcRelAggregates` decomposition   |
|  [10]   | `IfcProduct.AddElement`               | spatial mutation       | adds a contained product into the spatial structure |
|  [11]   | `IfcElement.SetMaterial`              | material assignment    | associates a material with an element               |

[ENTRYPOINT_SCOPE]: ParserIfc — codec operations
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                         | [CAPABILITY]                              |
| :-----: | :--------------------------- | :--------------------------------------------------- | :---------------------------------------- |
|   [1]   | `ParserIfc.ParseEnum<T>`     | `(string)` or `(string, string enumName)` → `T`      | parses an IFC enum literal                |
|   [2]   | `ParserIfc.DecodeGlobalID`   | `(string)` → `Guid`                                  | decodes a base64 IFC GlobalId to a `Guid` |
|   [3]   | `ParserIfc.EncodeGuid`       | `(Guid)` → `string`                                  | encodes a `Guid` to an IFC GlobalId       |
|   [4]   | `ParserIfc.HashGlobalID`     | `(string uniqueString)` → `string`                   | deterministic GlobalId from a stable key  |
|   [5]   | `ParserIfc.FormatLength`     | `(double, DatabaseIfc)` → `string`                   | formats a length per database units       |
|   [6]   | `ParserIfc.IdentifyIfcClass` | `(string, out string predefinedConstant)` → `string` | splits class name and predefined type     |

[ENTRYPOINT_SCOPE]: type-occurrence, predefined-type, and domain traversal
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

`PredefinedType` is a strongly-typed per-class enum member (`IfcWall.PredefinedType` is `IfcWallTypeEnum`, etc.); `IdentifyIfcClass` splits the predefined token from the class name at parse so the occurrence reads its predefined string without a per-class branch.

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]                                          | [CAPABILITY]                                                  |
| :-----: | :------------------------------------- | :--------------------------------------------------- | :----------------------------------------------------------- |
|   [1]   | `IfcObject.IsTypedBy`                  | `IfcRelDefinesByType` set                            | the type-binding relationships of an occurrence              |
|   [2]   | `IfcRelDefinesByType.RelatingType`     | `IfcTypeObject` property                             | the type object an occurrence is bound to                    |
|   [3]   | `IfcTypeObject.HasPropertySets`        | `IfcPropertySetDefinition` set                       | the type-bound property/quantity sets occurrences inherit    |
|   [4]   | `IfcTypeProduct.RepresentationMaps`    | `IfcRepresentationMap` set                           | the type's reusable instanced-geometry library               |
|   [5]   | `IfcElementType.ElementType`           | `string` property                                    | the type's predefined element-type identifier                |
|   [6]   | `IfcMappedItem.MappingSource`          | `IfcRepresentationMap` property                      | the representation map an occurrence instances               |
|   [7]   | `IfcMappedItem.MappingTarget`          | `IfcCartesianTransformationOperator` property        | the per-occurrence instance transform                        |
|   [8]   | `IfcWall.PredefinedType` (per class)   | `IfcWallTypeEnum` (etc.) property                    | the strongly-typed predefined-type member on each element    |
|   [9]   | `IfcObject.ObjectType`                 | `string` property                                    | the user-defined type string for `USERDEFINED` predefined    |

[ENTRYPOINT_SCOPE]: georeferencing, scheduling, and grouping traversal
- package: `GeometryGymIFC_Core`
- namespace: `GeometryGym.Ifc`
- rail: geometry

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]                              | [CAPABILITY]                                                  |
| :-----: | :----------------------------------------------------- | :--------------------------------------- | :----------------------------------------------------------- |
|   [1]   | `IfcGeometricRepresentationContext.HasCoordinateOperation` | `IfcCoordinateOperation` property    | the single map-conversion/CRS operation on a context (`IfcMapConversion` is itself an `IfcCoordinateOperation`, narrowed by `as`) |
|   [2]   | `IfcMapConversion.SourceCRS`/`TargetCRS`               | `IfcCoordinateReferenceSystemSelect`/`IfcCoordinateReferenceSystem` property | the source engineering frame and target projected CRS |
|   [3]   | `IfcProject.RepresentationContexts`                    | `IfcRepresentationContext` set           | the project's representation contexts (model/plan)           |
|   [4]   | `IfcTask.TaskTime`                                     | `IfcTaskTime` property                   | the task's schedule/actual start-finish-duration            |
|   [5]   | `IfcProcess.IsSuccessorFrom`/`IsPredecessorTo`         | `IfcRelSequence` set                     | the task's predecessor/successor dependency edges            |
|   [6]   | `IfcRelSequence.TimeLag`                               | `IfcLagTime` property                    | the dependency lag (duration + `IfcTaskDurationEnum`)        |
|   [7]   | `IfcGroup.IsGroupedBy`                                 | `IfcRelAssignsToGroup` set               | the assignment relationships grouping objects into a group   |
|   [8]   | `IfcCostItem.CostValues`                               | `IfcCostValue` set                       | the cost rates/values applied to a cost line item            |
|   [9]   | `IfcDistributionPort.ContainedIn`                      | `IfcRelConnectsPortToElement` property   | the distribution element a port belongs to                   |

## [4]-[IMPLEMENTATION_LAW]

[PLATFORM_BOUNDARY]:
- ships a single managed `GeometryGymIFCcore.dll` per target framework; no P/Invoke runtime assets, no architecture-specific native binaries
- STEP/IFC data object graph only; no tessellation, no BREP evaluation, no geometry kernel
- I/O is text-format serialization (STEP physical file, IFC-XML, IFC-JSON) over `string`, `Stream`, and `TextReader`/`XmlDocument`; no binary IFC format

[IFC_IO]:
- read root: `new DatabaseIfc(path)` / `new DatabaseIfc(TextReader)` infer STEP/XML/JSON; `DatabaseIfc.ParseString` reads in-memory STEP/IFC text
- format-explicit read: `ReadJSONFile`/`ReadJSON` for IFC-JSON, `ReadXMLFile`/`ReadXMLDoc` for IFC-XML
- write root: `DatabaseIfc.WriteFile` emits STEP physical file; `DatabaseIfc.ToString(FormatIfcSerialization)` selects `STEP`, `XML`, or `JSON`
- schema is database-level state: set via `DatabaseIfc(ReleaseVersion)` / `DatabaseIfc(ModelView)` and read via `Release` / `ModelView`
- STEP physical-file header metadata is exposed via `STEPFileInformation` on `DatabaseSTEP<T>.OriginatingFileInformation`; fields: `FileDescriptionViewDefinition`, `FileName`, `TimeStamp`, `Author`, `Organization`, `OriginatingSystem`, `Authorization`

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
