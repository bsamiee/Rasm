# [RASM_BIM_API_GEOMETRYGYM_IFC]

`GeometryGymIFC_Core` owns a pure-managed buildingSMART IFC object model: the `DatabaseIfc` repository, schema-versioned read/write across STEP, IFC-XML, and IFC-JSON, the full IFC4.3 entity vocabulary, and `Extract<T>` graph traversal feeding the geometry-interchange rail. It holds the STEP/IFC data graph alone — no tessellation, BREP evaluation, or geometry kernel.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `GeometryGymIFC_Core`
- package: `GeometryGymIFC_Core`
- assembly: `GeometryGymIFCcore`
- namespace: `GeometryGym.Ifc`
- namespace: `GeometryGym.STEP`
- asset: net8.0, net7.0, net6.0, netstandard2.0
- asset: IL-only AnyCPU managed assembly; no `runtimes/` folder, no native binaries
- asset: zero declared package dependencies on every target framework
- rail: geometry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: repository, factory, and serialization roots

| [INDEX] | [SYMBOL]              | [CAPABILITY]                                                                                 |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `DatabaseIfc`         | IFC model repository; owns all entities, schema, units, tolerance, and I/O                   |
|  [02]   | `DatabaseSTEP<T>`     | generic STEP record store; `IEnumerable<T>`, `this[int stepId]`, `NextObjectRecord`          |
|  [03]   | `BaseClassIfc`        | abstract root of every IFC entity; carries `Database`, `Extract<T>`, STEP/JSON serialization |
|  [04]   | `STEPEntity`          | base STEP record carrier under `BaseClassIfc`                                                |
|  [05]   | `FactoryIfc`          | per-database factory for canonical model primitives                                          |
|  [06]   | `ParserIfc`           | static STEP/enum/GUID codec; `ParseEnum<T>`, `DecodeGlobalID`, `EncodeGuid`                  |
|  [07]   | `ParserSTEP`          | static low-level STEP token parser                                                           |
|  [08]   | `STEPFileInformation` | originating-file header metadata on the database                                             |
|  [09]   | `DuplicateOptions`    | options carrier for cross-database entity duplication                                        |
|  [10]   | `DuplicateMapping`    | source-to-target entity map during duplication                                               |

[FACTORY_PRIMITIVES]: `FactoryIfc` owns canonical axes, origins, application, owner history, and placements; `RootPlacement` is an `IfcLocalPlacement` at world origin, and `XYPlanePlacement` is the default `IfcAxis2Placement3D` consumed by element and profile occurrence constructors.

[STEP_FILE_INFO_MEMBERS]: `STEPFileInformation`, reached through `DatabaseSTEP<T>.OriginatingFileInformation`, carries `FileDescriptions`/`FileDescriptionViewDefinition`/`FileDescriptionExchangeRequirements` and `Author`/`Organization` (`List<string>`), `FileImplementationLevel`/`FileName`/`PreProcessorVersion`/`OriginatingSystem`/`Authorization` (`string`), and `TimeStamp` (`DateTime`).

[PUBLIC_TYPE_SCOPE]: IFC schema vocabulary enums

| [INDEX] | [SYMBOL]                           | [CAPABILITY]                                                           |
| :-----: | :--------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `ReleaseVersion`                   | schema-version selector                                                |
|  [02]   | `ModelView`                        | model-view selector                                                    |
|  [03]   | `FormatIfcSerialization`           | serialization format for `DatabaseIfc.ToString`: `STEP`, `XML`, `JSON` |
|  [04]   | `IfcReflectanceMethodEnum`         | PBR/Phong reflectance model                                            |
|  [05]   | `IfcSurfaceSide`                   | surface-style application side: `POSITIVE`, `NEGATIVE`, `BOTH`         |
|  [06]   | `IfcZone`                          | functional-zone grouping without `PredefinedType`                      |
|  [07]   | `IfcSpatialZoneTypeEnum`           | spatial-zone kind                                                      |
|  [08]   | `IfcDistributionSystemEnum`        | distribution-system kind                                               |
|  [09]   | `IfcFlowDirectionEnum`             | port flow direction: `SOURCE`, `SINK`, `SOURCEANDSINK`, `NOTDEFINED`   |
|  [10]   | `IfcSequenceEnum`                  | task-dependency kind                                                   |
|  [11]   | `IfcTaskDurationEnum`              | task-duration interpretation: `ELAPSEDTIME`, `WORKTIME`, `NOTDEFINED`  |
|  [12]   | `IfcCostScheduleTypeEnum`          | cost-schedule kind                                                     |
|  [13]   | `IfcStructuralCurveMemberTypeEnum` | idealized 1D member kind                                               |
|  [14]   | `IfcLoadGroupTypeEnum`             | load-group kind                                                        |
|  [15]   | `IfcCardinalPointReference`        | profile-placement reference axis                                       |
|  [16]   | `IfcLogicalEnum`                   | three-valued logical                                                   |
|  [17]   | `IfcWorkScheduleTypeEnum`          | work-schedule kind                                                     |

[RELEASE_VALUES]: `ReleaseVersion` `IFC2x3` `IFC4A2` `IFC4X3` `IFC4X3_ADD2` `IFC4X4_DRAFT`.

[MODEL_VIEW_VALUES]: `ModelView` `Ifc4Reference` `Ifc4DesignTransfer` `Ifc2x3Coordination` `IFC4X3Reference` `IFC4X3AlignmentBasedView` `Ifc4X3NotAssigned`.

[REFLECTANCE_VALUES]: `IfcReflectanceMethodEnum` `BLINN` `FLAT` `GLASS` `MATT` `METAL` `MIRROR` `PHONG` `PLASTIC` `STRAUSS` `NOTDEFINED`.

[ZONE_KIND_BOUNDARY]: `IfcZone`, `IfcSystem`, and `IfcGroup` carry `LongName` and `Name` without `IfcZoneTypeEnum`; `IfcSpatialZone`, `IfcDistributionSystem`, and `IfcStructuralLoadGroup` carry predefined kinds.

[SPATIAL_ZONE_VALUES]: `IfcSpatialZoneTypeEnum` `NOTDEFINED` `USERDEFINED` `CONSTRUCTION` `FIRESAFETY` `LIGHTING` `OCCUPANCY` `SECURITY` `THERMAL` `VENTILATION` `TRANSPORT` `RESERVATION` `INTERFERENCE`.

[DISTRIBUTION_SYSTEM_VALUES]: `IfcDistributionSystemEnum` `AIRCONDITIONING` `ELECTRICAL` `DOMESTICCOLDWATER` `DOMESTICHOTWATER` `DRAINAGE` `FIREPROTECTION` `VENTILATION`.

[SEQUENCE_VALUES]: `IfcSequenceEnum` `START_START` `START_FINISH` `FINISH_START` `FINISH_FINISH` `USERDEFINED` `NOTDEFINED`.

[COST_SCHEDULE_VALUES]: `IfcCostScheduleTypeEnum` `BUDGET` `COSTPLAN` `ESTIMATE` `TENDER` `PRICEDBILLOFQUANTITIES` `SCHEDULEOFRATES`.

[STRUCTURAL_CURVE_VALUES]: `IfcStructuralCurveMemberTypeEnum` `RIGID_JOINED_MEMBER` `PIN_JOINED_MEMBER` `CABLE` `TENSION_MEMBER` `COMPRESSION_MEMBER`.

[LOAD_GROUP_VALUES]: `IfcLoadGroupTypeEnum` `LOAD_GROUP` `LOAD_CASE` `LOAD_COMBINATION` `USERDEFINED` `NOTDEFINED`.

[CARDINAL_POINT_VALUES]: `IfcCardinalPointReference` spans `DEFAULT` (0) through `TOPSHEAR` (19); `MID` (5) is the default and `TOPRIGHT` (9) the structural-grid reference.

[LOGICAL_VALUES]: `IfcLogicalEnum` `TRUE` `FALSE` `UNKNOWN`; `IfcMaterialLayer.IsVentilated` compares against `TRUE`.

[WORK_SCHEDULE_VALUES]: `IfcWorkScheduleTypeEnum` `ACTUAL` `BASELINE` `PLANNED`, read through `IfcWorkSchedule.PredefinedType`.

[PUBLIC_TYPE_SCOPE]: IFC kernel root entities

| [INDEX] | [SYMBOL]                   | [CAPABILITY]                                                                           |
| :-----: | :------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `IfcRoot`                  | rooted entity base; `GlobalId`, `Guid`, `OwnerHistory`, `Name`, `Description`          |
|  [02]   | `IfcObjectDefinition`      | object base; `Nests`, `Decomposes`, `AddAggregated`, `FindProperty`, `FindPropertySet` |
|  [03]   | `IfcContext`               | shared context; `UnitsInContext`, `AddDeclared`, `DeclaredTypes`                       |
|  [04]   | `IfcProject`               | project root context; ctors over building/site/facility/zone + units; `UppermostSite`  |
|  [05]   | `IfcProjectLibrary`        | shared declaration library context                                                     |
|  [06]   | `IfcObject`                | occurrence object base under `IfcObjectDefinition`                                     |
|  [07]   | `IfcProduct`               | spatially located product; `ObjectPlacement`, `Representation`, `AddElement`           |
|  [08]   | `IfcElement`               | physical element base; `Tag`, `MaterialSelect`, `SetMaterial`                          |
|  [09]   | `IfcOwnerHistory`          | change-tracking stamp; creation/modification metadata                                  |
|  [10]   | `IfcApplication`           | authoring-application identity                                                         |
|  [11]   | `IfcPerson`                | person actor record                                                                    |
|  [12]   | `IfcOrganization`          | organization actor record                                                              |
|  [13]   | `IfcPersonAndOrganization` | bound person + organization actor                                                      |

[PUBLIC_TYPE_SCOPE]: spatial structure and type-object families

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                                                            |
| :-----: | :--------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `IfcSpatialElement`          | spatial container base; `LongName`, `ReferenceElement`                  |
|  [02]   | `IfcSpatialStructureElement` | spatial hierarchy node; `CompositionType` (`IfcElementCompositionEnum`) |
|  [03]   | `IfcSite`                    | site spatial structure element                                          |
|  [04]   | `IfcBuilding`                | building spatial structure element                                      |
|  [05]   | `IfcBuildingStorey`          | storey spatial structure element                                        |
|  [06]   | `IfcFacility`                | IFC4.3 facility spatial structure base                                  |
|  [07]   | `IfcFacilityPart`            | IFC4.3 facility-part subdivision                                        |
|  [08]   | `IfcExternalSpatialElement`  | external (outside-facility) spatial element                             |
|  [09]   | `IfcTypeObject`              | type-definition base under `IfcObjectDefinition`                        |
|  [10]   | `IfcTypeProduct`             | product type with representation maps                                   |
|  [11]   | `IfcElementType`             | element type base under `IfcTypeProduct`                                |
|  [12]   | `IfcBuiltElementType`        | built-element type base                                                 |

[PUBLIC_TYPE_SCOPE]: property-set, quantity, and material families

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                                                                               |
| :-----: | :--------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `IfcPropertySetDefinition`   | property/quantity set base                                                                 |
|  [02]   | `IfcPropertySet`             | named set of `IfcProperty` instances                                                       |
|  [03]   | `IfcPropertySingleValue`     | single-value property (`IfcSimpleProperty` subtype)                                        |
|  [04]   | `IfcPropertyEnumeratedValue` | enumeration-referenced property value                                                      |
|  [05]   | `IfcComplexProperty`         | nested property aggregate                                                                  |
|  [06]   | `IfcQuantitySet`             | quantity-set base under `IfcPropertySetDefinition`                                         |
|  [07]   | `IfcElementQuantity`         | named set of physical quantities                                                           |
|  [08]   | `IfcPhysicalSimpleQuantity`  | simple physical-quantity base                                                              |
|  [09]   | `IfcPropertySetTemplate`     | reusable property-set template                                                             |
|  [10]   | `IfcMaterial`                | named material definition                                                                  |
|  [11]   | `IfcMaterialLayerSet`        | ordered material layer assembly; `MaterialLayers` (`LIST<IfcMaterialLayer>`)               |
|  [12]   | `IfcMaterialLayerSetUsage`   | layer-set application to an element                                                        |
|  [13]   | `IfcMaterialProfileSet`      | material-profile assembly for linear members                                               |
|  [14]   | `IfcMaterialConstituentSet`  | named constituent material set                                                             |
|  [15]   | `IfcMaterialProperties`      | named material property-set binding                                                        |
|  [16]   | `IfcExtendedProperties`      | extended-property base and dictionary owner                                                |
|  [17]   | `IfcProperty`                | abstract property root (`IfcPropertySingleValue` etc.); the `Properties`-dict element type |
|  [18]   | `IfcMaterialProfile`         | profile-material row                                                                       |
|  [19]   | `IfcMaterialLayer`           | material-layer row                                                                         |
|  [20]   | `IfcMaterialConstituent`     | material-constituent row                                                                   |
|  [21]   | `IfcPropertyReferenceValue`  | reference property with an `IfcObjectReferenceSelect` target                               |
|  [22]   | `IfcInteger`                 | signed integer scalar                                                                      |
|  [23]   | `IfcReal`                    | finite real scalar                                                                         |
|  [24]   | `IfcBinary`                  | binary scalar                                                                              |
|  [25]   | `IfcDate`                    | calendar-date scalar                                                                       |
|  [26]   | `IfcDateTime`                | local date-time scalar                                                                     |
|  [27]   | `IfcTime`                    | local-time scalar                                                                          |
|  [28]   | `IfcDuration`                | ISO duration scalar                                                                        |
|  [29]   | `IfcTimeStamp`               | Unix-time scalar                                                                           |

[PROPERTY_REFERENCE_MEMBERS]: `IfcPropertyReferenceValue` constructs from `(DatabaseIfc db, string name)` or `(string name, IfcObjectReferenceSelect obj)` and carries settable `UsageName` and `PropertyReference`.

[SCALAR_VALUE_MEMBERS]: `IfcInteger(long)` and `IfcReal(double)` carry `Magnitude`; `IfcBinary(byte[])` carries `Binary`; `IfcDate(DateTime)`, `IfcDateTime(DateTime)`, `IfcTime` (parameterless, settable `Value`), `IfcDuration` (settable `Years`/`Months`/`Days`/`Hours`/`Minutes`/`Seconds`), and `IfcTimeStamp(int)` carry the neutral typed-value scalars.

[LAYER_SET_USAGE_MEMBERS]: `IfcMaterialLayerSetUsage` carries `ForLayerSet` (`IfcMaterialLayerSet`), `LayerSetDirection` (`IfcLayerSetDirectionEnum`), `DirectionSense` (`IfcDirectionSenseEnum`), `OffsetFromReferenceLine` (`double`), and `ReferenceExtent` (`double`), the layer-set size perpendicular to the layers and the fourth `MaterialUsage.LayerSet` argument.

[MATERIAL_PROPERTIES_MEMBERS]: `IfcMaterialProperties` extends `IfcExtendedProperties`, binds `Material`, constructs from `(string name, IfcMaterialDefinition mat)`, and adds columns to inherited `Properties`.

[EXTENDED_PROPERTIES_MEMBERS]: `IfcExtendedProperties` carries `Name`, `Description`, `Properties` as `Dictionary<string, IfcProperty>`, and a `this[name]` indexer.

[MATERIAL_PROFILE_MEMBERS]: `IfcMaterialProfileSet.MaterialProfiles` is a `LIST<IfcMaterialProfile>`; each row carries `Material`, subtype-discriminated `Profile`, string `Name`, `Description`, `Category`, and integer `Priority`.

[MATERIAL_LAYER_MEMBERS]: Each `IfcMaterialLayerSet.MaterialLayers` entry extends `IfcMaterialDefinition`, constructs from `(IfcMaterial, double thickness, string name)`, and carries `Material`, `LayerThickness`, `Priority`, `Category`, and `IsVentilated`.

[MATERIAL_CONSTITUENT_MEMBERS]: Each `IfcMaterialConstituentSet.MaterialConstituents` entry extends `IfcMaterialDefinition`, constructs from `(string name, IfcMaterial)`, and carries `Material`, `Name`, and `Fraction`.

[SIMPLE_QUANTITY_TYPES]: `IfcPhysicalSimpleQuantity` covers `IfcQuantityLength` `IfcQuantityArea` `IfcQuantityVolume` `IfcQuantityWeight` `IfcQuantityCount` `IfcQuantityTime`.

[CONSTITUENT_SET_MEMBERS]: `IfcMaterialConstituentSet.MaterialConstituents` is a `Dictionary<string, IfcMaterialConstituent>` traversed through `.Values`.

[PUBLIC_TYPE_SCOPE]: relationship families

| [INDEX] | [SYMBOL]                            | [CAPABILITY]                                              |
| :-----: | :---------------------------------- | :-------------------------------------------------------- |
|  [01]   | `IfcRelationship`                   | objectified-relationship root                             |
|  [02]   | `IfcRelAggregates`                  | whole-part decomposition (`IfcRelDecomposes` subtype)     |
|  [03]   | `IfcRelNests`                       | ordered nesting decomposition                             |
|  [04]   | `IfcRelContainedInSpatialStructure` | element-to-spatial containment (`IfcRelConnects` subtype) |
|  [05]   | `IfcRelDefinesByProperties`         | binds a property/quantity set to objects                  |
|  [06]   | `IfcRelDefinesByType`               | binds occurrences to a type object                        |
|  [07]   | `IfcRelAssociatesMaterial`          | material-definition association                           |
|  [08]   | `IfcRelAssociatesClassification`    | classification-reference association                      |
|  [09]   | `IfcRelDeclares`                    | declares definitions within a context                     |
|  [10]   | `IfcRelVoidsElement`                | subtracts an opening from an element                      |
|  [11]   | `IfcRelConnectsElements`            | physical element-to-element connection                    |
|  [12]   | `IfcDefinitionSelect`               | relationship-object SELECT interface                      |

[DEFINITION_SELECT_USE]: `IfcDefinitionSelect` is the `IfcRelAssociates.RelatedObjects` element type and the `(IfcDefinitionSelect)material` cast target in the classification-association constructor.

[NESTING_MEMBERS]: `IfcRelNests.RelatingObject` is the `IfcObjectDefinition` parent reached through `IfcObjectDefinition.Nests`.

[MATERIAL_ASSOCIATION_MEMBERS]: `IfcRelAssociatesMaterial` exposes `RelatingMaterial` as `IfcMaterialSelect` and `RelatedObjects` as `SET<IfcDefinitionSelect>`.

[CLASSIFICATION_ASSOCIATION_CTOR]: `IfcRelAssociatesClassification` constructs from `(IfcClassificationSelect classification, IfcDefinitionSelect related)`.

[PUBLIC_TYPE_SCOPE]: architectural built-element family

| [INDEX] | [SYMBOL]                  | [CAPABILITY]                                                                              |
| :-----: | :------------------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `IfcCovering`             | finish covering built element; `PredefinedType` (`IfcCoveringTypeEnum`)                   |
|  [02]   | `IfcCurtainWall`          | curtain-wall built element aggregating plate/member units; `PredefinedType`               |
|  [03]   | `IfcRailing`              | railing built element; `PredefinedType` (`IfcRailingTypeEnum`)                            |
|  [04]   | `IfcRamp`                 | ramp built element; `PredefinedType` (`IfcRampTypeEnum`)                                  |
|  [05]   | `IfcRoof`                 | roof built element; `PredefinedType` (`IfcRoofTypeEnum`)                                  |
|  [06]   | `IfcStair`                | stair built element; `PredefinedType` (`IfcStairTypeEnum`)                                |
|  [07]   | `IfcPlate`                | planar built element (curtain-wall panel); `PredefinedType` (`IfcPlateTypeEnum`)          |
|  [08]   | `IfcMember`               | structural-member built element (mullion/post); `PredefinedType` (`IfcMemberTypeEnum`)    |
|  [09]   | `IfcFooting`              | foundation footing built element; `PredefinedType` (`IfcFootingTypeEnum`)                 |
|  [10]   | `IfcPile`                 | foundation pile built element; `PredefinedType` (`IfcPileTypeEnum`)                       |
|  [11]   | `IfcWall`                 | wall built element; `PredefinedType` (`IfcWallTypeEnum`: STANDARD/SHEAR/PARTITIONING/…)   |
|  [12]   | `IfcSlab`                 | slab built element; `PredefinedType` (`IfcSlabTypeEnum`: FLOOR/ROOF/LANDING/BASESLAB)     |
|  [13]   | `IfcColumn`               | column built element; `PredefinedType` (`IfcColumnTypeEnum`)                              |
|  [14]   | `IfcBeam`                 | beam built element; `PredefinedType` (`IfcBeamTypeEnum`)                                  |
|  [15]   | `IfcDoor`                 | door built element; `PredefinedType` (`IfcDoorTypeEnum`) + `OperationType`                |
|  [16]   | `IfcWindow`               | window built element; `PredefinedType` (`IfcWindowTypeEnum`) + `PartitioningType`         |
|  [17]   | `IfcSpace`                | space spatial element; `PredefinedType` (`IfcSpaceTypeEnum`: INTERNAL/EXTERNAL/PARKING/…) |
|  [18]   | `IfcBuildingElementProxy` | generic proxy built element and unclassified-product carrier                              |

[BUILDING_ELEMENT_PROXY_MEMBERS]: `IfcBuildingElementProxy` extends `IfcBuiltElement`, constructs from `(IfcObjectDefinition host, IfcObjectPlacement, IfcProductDefinitionShape)`, and carries `PredefinedType` values including `COMPLEX`, `ELEMENT`, `PARTIAL`, and `PROVISIONFORVOID`.

[PUBLIC_TYPE_SCOPE]: MEP distribution-element family

| [INDEX] | [SYMBOL]                    | [CAPABILITY]                                                                                    |
| :-----: | :-------------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `IfcDistributionElement`    | MEP distribution-element base with `HasPorts`                                                   |
|  [02]   | `IfcFlowSegment`            | flow segment (duct/pipe/cable run); `PredefinedType`                                            |
|  [03]   | `IfcFlowFitting`            | flow fitting (elbow/tee/junction); `PredefinedType`                                             |
|  [04]   | `IfcFlowTerminal`           | flow terminal (air/sanitary/light fixture); `PredefinedType` (AIRTERMINAL/SANITARYTERMINAL/…)   |
|  [05]   | `IfcFlowController`         | flow controller (valve/damper/switch); `PredefinedType`                                         |
|  [06]   | `IfcFlowMovingDevice`       | flow moving device (pump/fan/compressor); `PredefinedType`                                      |
|  [07]   | `IfcFlowStorageDevice`      | flow storage device (tank/battery); `PredefinedType`                                            |
|  [08]   | `IfcEnergyConversionDevice` | energy conversion device (boiler/chiller/coil); `PredefinedType`                                |
|  [09]   | `IfcDistributionPort`       | typed connection port; `FlowDirection` (`IfcFlowDirectionEnum`), `SystemType`, `PredefinedType` |
|  [10]   | `IfcPort`                   | abstract connection-port base; `ContainedIn`, `ConnectedFrom`, `ConnectedTo`                    |

[DISTRIBUTION_ELEMENT_PORTS]: `IfcDistributionElement.HasPorts` carries `IfcRelConnectsPortToElement` relationships.

[PUBLIC_TYPE_SCOPE]: structural-analysis-domain family

| [INDEX] | [SYMBOL]                           | [CAPABILITY]                                                                                    |
| :-----: | :--------------------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `IfcStructuralAnalysisModel`       | structural-analysis container                                                                   |
|  [02]   | `IfcStructuralItem`                | structural-item base; `AssignedStructuralActivity`, `AssignedToStructuralItem`                  |
|  [03]   | `IfcStructuralMember`              | idealized structural member base under `IfcStructuralItem`                                      |
|  [04]   | `IfcStructuralCurveMember`         | 1D idealized member (beam/column line); `PredefinedType` (`IfcStructuralCurveMemberTypeEnum`)   |
|  [05]   | `IfcStructuralSurfaceMember`       | 2D idealized member (slab/wall surface); `PredefinedType`, `Thickness`                          |
|  [06]   | `IfcStructuralConnection`          | structural connection base; `AppliedCondition` (`IfcBoundaryCondition`)                         |
|  [07]   | `IfcStructuralPointConnection`     | point support/connection node                                                                   |
|  [08]   | `IfcStructuralCurveConnection`     | curve support/connection edge                                                                   |
|  [09]   | `IfcStructuralSurfaceConnection`   | surface support/connection                                                                      |
|  [10]   | `IfcStructuralLoadGroup`           | grouped structural loads; `PredefinedType`, `ActionType`, `ActionSource`, `SourceOfResultGroup` |
|  [11]   | `IfcStructuralLoadCase`            | load case under `IfcStructuralLoadGroup`; `SelfWeightCoefficients`                              |
|  [12]   | `IfcStructuralResultGroup`         | grouped analysis results                                                                        |
|  [13]   | `IfcBoundaryCondition`             | boundary-condition base                                                                         |
|  [14]   | `IfcStructuralLoadConfiguration`   | varying structural line action                                                                  |
|  [15]   | `IfcRelConnectsStructuralMember`   | member-to-connection relation                                                                   |
|  [16]   | `IfcRelConnectsWithEccentricity`   | eccentric member-to-connection relation under `IfcRelConnectsStructuralMember`                  |
|  [17]   | `IfcRelConnectsStructuralActivity` | structural-activity binding                                                                     |
|  [18]   | `IfcRelAssignsToGroup`             | group membership assignment                                                                     |

[ECCENTRIC_CONNECTION_MEMBERS]: `IfcRelConnectsWithEccentricity` extends `IfcRelConnectsStructuralMember` and carries public settable `ConnectionConstraint` as `IfcConnectionGeometry` (schema-mandatory — `BuildStringSTEP` writes its StepId unconditionally); the public ctor is `(IfcStructuralMember, IfcStructuralConnection, IfcConnectionGeometry)`.

[BOUNDARY_CONDITION_CTORS]: `IfcBoundaryCondition` spans `IfcBoundaryNodeCondition` and `IfcBoundaryEdgeCondition`; `IfcBoundaryNodeCondition` constructs from `(db)`, `(db, name, 6x bool restraints)`, or `(db, name, 3x IfcTranslationalStiffnessSelect + 3x IfcRotationalStiffnessSelect)`.

[STRUCTURAL_LOAD_CONFIGURATION_MEMBERS]: `IfcStructuralLoadConfiguration` extends `IfcStructuralLoad`, exposes `Values` as `LIST<IfcStructuralLoadOrResult>` and `Locations` as `List<List<double>>`, and constructs from `(val, length)`, `(vals, locations)`, or `(val1, loc1, val2, loc2)` for trapezoid vector lowering.

[GROUP_ASSIGNMENT_MEMBERS]: `IfcRelAssignsToGroup.RelatedObjects` is the `SET<IfcObjectDefinition>` reached through `IfcGroup.IsGroupedBy` for `IfcGroup`, `IfcSystem`, and `IfcStructuralLoadGroup` membership.

[STRUCTURAL_ANALYSIS_MODEL_MEMBERS]: `IfcStructuralAnalysisModel` carries `OrientationOf2DPlane`, `LoadedBy`, `HasResults`, and `SharedPlacement`.

[STRUCTURAL_MEMBER_CONNECTION_MEMBERS]: `IfcRelConnectsStructuralMember` carries `RelatingStructuralMember` and `RelatedStructuralConnection`.

[PUBLIC_TYPE_SCOPE]: grouping, zone, and distribution-system family

| [INDEX] | [SYMBOL]                             | [CAPABILITY]                                                                                |
| :-----: | :----------------------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `IfcGroup`                           | non-spatial logical grouping base; `IsGroupedBy` (`IfcRelAssignsToGroup`)                   |
|  [02]   | `IfcSystem`                          | functional system grouping under `IfcGroup`; `ServicesBuildings`                            |
|  [03]   | `IfcBuiltSystem`                     | IFC4.3 built-system grouping under `IfcSystem`                                              |
|  [04]   | `IfcDistributionSystem`              | MEP distribution system; `PredefinedType` (`IfcDistributionSystemEnum`), member element set |
|  [05]   | `IfcZone`                            | functional zone aggregating spaces across storeys                                           |
|  [06]   | `IfcSpatialZone`                     | fire/thermal/construction/occupancy zone; `PredefinedType` (`IfcSpatialZoneTypeEnum`)       |
|  [07]   | `IfcRelReferencedInSpatialStructure` | spatial-reference overlay                                                                   |
|  [08]   | `IfcRelServicesBuildings`            | binds a system to the spatial structures it serves                                          |
|  [09]   | `IfcRelConnectsPortToElement`        | connects an `IfcDistributionPort` to its owning distribution element                        |
|  [10]   | `IfcRelConnectsPorts`                | port-to-port connection edge; `RelatingPort`, `RelatedPort`, `RealizingElement`             |
|  [11]   | `IfcRelFlowControlElements`          | flow-control binding under `IfcRelConnects`; public pair is `RelatingPort`/`RelatedElement` |

[BUILT_SYSTEM_MEMBERS]: `IfcBuiltSystem` carries `PredefinedType` as `IfcBuiltSystemTypeEnum`; the assembly contains no `IfcBuildingSystem` member.

[FLOW_CONTROL_MEMBERS]: `IfcRelFlowControlElements` extends `IfcRelConnects` and carries settable `RelatingPort` (`IfcPort`) and `RelatedElement` (`IfcElement`).

[ZONE_MEMBERS]: `IfcZone` carries `LongName`, constructs from `(IfcSpatialElement, string, List<IfcSpace>)`, and has no `PredefinedType`.

[SPATIAL_REFERENCE_USE]: `IfcRelReferencedInSpatialStructure` references an element into a spatial structure that does not contain it.

[PUBLIC_TYPE_SCOPE]: scheduling, cost, and resource family

| [INDEX] | [SYMBOL]                           | [CAPABILITY]                                                                           |
| :-----: | :--------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `IfcProcess`                       | process base                                                                           |
|  [02]   | `IfcTask`                          | scheduled task; `Status`, `WorkMethod`, `IsMilestone`, `TaskTime`, `PredefinedType`    |
|  [03]   | `IfcTaskTime`                      | task schedule and actual times                                                         |
|  [04]   | `IfcWorkSchedule`                  | work schedule under `IfcWorkControl`; `Controls`, `PredefinedType`                     |
|  [05]   | `IfcWorkPlan`                      | work plan grouping schedules under `IfcWorkControl`; `PredefinedType`                  |
|  [06]   | `IfcWorkCalendar`                  | working/exception time calendar; `WorkingTimes`, `ExceptionTimes`                      |
|  [07]   | `IfcRelSequence`                   | task dependency edge                                                                   |
|  [08]   | `IfcRelAssignsToProcess`           | assigns products/resources to a process; `RelatingProcess`, `RelatedObjects`           |
|  [09]   | `IfcCostSchedule`                  | cost schedule under `IfcControl`; `Controls`, `PredefinedType`, `SubmittedOn`          |
|  [10]   | `IfcCostItem`                      | cost line item; `CostValues` (`IfcCostValue`), `CostQuantities`, `PredefinedType`      |
|  [11]   | `IfcCostValue`                     | applied cost value/rate; `AppliedValue` (`IfcAppliedValue`), `UnitBasis`, `Category`   |
|  [12]   | `IfcConstructionResource`          | construction-resource base; `Usage`, `BaseCosts`, `BaseQuantity`                       |
|  [13]   | `IfcLaborResource`                 | labor resource; `PredefinedType` (`IfcLaborResourceTypeEnum`)                          |
|  [14]   | `IfcConstructionMaterialResource`  | material resource; `PredefinedType` (`IfcConstructionMaterialResourceTypeEnum`)        |
|  [15]   | `IfcConstructionEquipmentResource` | equipment resource; `PredefinedType`                                                   |
|  [16]   | `IfcRelAssignsToControl`           | assigns objects to a control (cost item/schedule); `RelatingControl`, `RelatedObjects` |
|  [17]   | `IfcWorkTime`                      | working or exception period under `IfcSchedulingTime`                                  |

[WORK_TIME_MEMBERS]: `IfcWorkCalendar.WorkingTimes` and `ExceptionTimes` contain `IfcWorkTime` entries with public `DateTime` members `StartDate` and `FinishDate`.

[PROCESS_MEMBERS]: `IfcProcess` carries `IsSuccessorFrom`, `IsPredecessorTo` (`IfcRelSequence` collections), and `OperatesOn`.

[TASK_TIME_MEMBERS]: `IfcTaskTime` carries `ScheduleStart`, `ScheduleFinish`, `ScheduleDuration`, `ActualStart`, and `ActualFinish`.

[REL_SEQUENCE_MEMBERS]: `IfcRelSequence` carries `RelatingProcess`, `RelatedProcess`, `TimeLag`, and `SequenceType`.

[PUBLIC_TYPE_SCOPE]: georeferencing and map-conversion entities

| [INDEX] | [SYMBOL]                       | [CAPABILITY]                                                                |
| :-----: | :----------------------------- | :-------------------------------------------------------------------------- |
|  [01]   | `IfcCoordinateOperation`       | coordinate-operation base; `SourceCRS`, `TargetCRS`                         |
|  [02]   | `IfcMapConversion`             | rigid map-conversion offset                                                 |
|  [03]   | `IfcCoordinateReferenceSystem` | CRS base; `Name`, `GeodeticDatum`, `VerticalDatum`                          |
|  [04]   | `IfcProjectedCRS`              | projected coordinate reference system                                       |
|  [05]   | `IfcMapConversionScaled`       | per-axis scaled map conversion (IFC4.3 ADD2); `FactorX`/`FactorY`/`FactorZ` |

[MAP_CONVERSION_MEMBERS]: `IfcMapConversion` carries `Eastings`, `Northings`, `OrthogonalHeight`, `XAxisAbscissa`, `XAxisOrdinate`, and `Scale`.

[PROJECTED_CRS_MEMBERS]: `IfcProjectedCRS` carries EPSG `Name`, `GeodeticDatum`, `VerticalDatum`, `MapProjection`, `MapZone`, and `MapUnit`.

[PUBLIC_TYPE_SCOPE]: IFC4.3 infrastructure entities — alignment and facility

| [INDEX] | [SYMBOL]                      | [CAPABILITY]                                     |
| :-----: | :---------------------------- | :----------------------------------------------- |
|  [01]   | `IfcAlignment`                | linear-referencing alignment positioning element |
|  [02]   | `IfcAlignmentHorizontal`      | horizontal alignment layout                      |
|  [03]   | `IfcAlignmentVertical`        | vertical alignment layout                        |
|  [04]   | `IfcAlignmentCant`            | rail cant alignment layout                       |
|  [05]   | `IfcAlignmentSegment`         | one parameterized alignment segment              |
|  [06]   | `IfcLinearPlacement`          | placement along a curve via distance expression  |
|  [07]   | `IfcLinearPositioningElement` | base for linear referencing positioning          |
|  [08]   | `IfcReferent`                 | referent point along an alignment                |
|  [09]   | `IfcBridge`                   | bridge facility                                  |
|  [10]   | `IfcRailway`                  | railway facility                                 |
|  [11]   | `IfcRoad`                     | road facility                                    |
|  [12]   | `IfcMarineFacility`           | marine facility                                  |

[PUBLIC_TYPE_SCOPE]: IFC4.3 infrastructure entities — earthworks and geotechnics

| [INDEX] | [SYMBOL]                 | [CAPABILITY]                                    |
| :-----: | :----------------------- | :---------------------------------------------- |
|  [01]   | `IfcCourse`              | layered pavement/earthwork course built element |
|  [02]   | `IfcPavement`            | pavement built element                          |
|  [03]   | `IfcRail`                | rail built element                              |
|  [04]   | `IfcEarthworksFill`      | earthworks fill element                         |
|  [05]   | `IfcEarthworksCut`       | earthworks excavation element                   |
|  [06]   | `IfcGeotechnicalStratum` | geotechnical soil/rock stratum                  |
|  [07]   | `IfcBorehole`            | geotechnical borehole assembly                  |

[PUBLIC_TYPE_SCOPE]: geometry representation entities

| [INDEX] | [SYMBOL]                            | [CAPABILITY]                                     |
| :-----: | :---------------------------------- | :----------------------------------------------- |
|  [01]   | `IfcProductDefinitionShape`         | product shape; aggregates representations        |
|  [02]   | `IfcRepresentation`                 | one geometric/topological representation         |
|  [03]   | `IfcGeometricRepresentationContext` | coordinate space + precision for representations |
|  [04]   | `IfcCartesianPoint`                 | n-D cartesian point                              |
|  [05]   | `IfcCartesianPointList3D`           | packed 3D point list                             |
|  [06]   | `IfcDirection`                      | direction vector                                 |
|  [07]   | `IfcAxis2Placement3D`               | 3D placement (location + axes)                   |
|  [08]   | `IfcLocalPlacement`                 | relative object placement                        |
|  [09]   | `IfcExtrudedAreaSolid`              | swept-area extrusion solid                       |
|  [10]   | `IfcPolygonalFaceSet`               | indexed polygonal face mesh                      |
|  [11]   | `IfcTriangulatedFaceSet`            | indexed triangle mesh                            |
|  [12]   | `IfcFacetedBrep`                    | faceted boundary-representation solid            |
|  [13]   | `IfcAdvancedBrep`                   | NURBS-faced boundary-representation solid        |
|  [14]   | `IfcBooleanResult`                  | CSG boolean operation result                     |
|  [15]   | `IfcMappedItem`                     | instanced representation map reference           |
|  [16]   | `IfcRepresentationMap`              | reusable type-bound geometry library             |

[REPRESENTATION_MAP_MEMBERS]: `IfcRepresentationMap` carries `MappingOrigin` as `IfcAxis2Placement`, `MappedRepresentation` as `IfcRepresentation`, and `HasShapeAspects`; `IfcMappedItem.MappingSource` references it.

[PUBLIC_TYPE_SCOPE]: parameterized profile-definition family

| [INDEX] | [SYMBOL]                       | [CAPABILITY]                                                                    |
| :-----: | :----------------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `IfcProfileDef`                | cross-section profile root                                                      |
|  [02]   | `IfcProfileTypeEnum`           | profile-use kind: `AREA` (solid swept section, default), `CURVE` (open section) |
|  [03]   | `IfcParameterizedProfileDef`   | dimensioned parametric-section base                                             |
|  [04]   | `IfcIShapeProfileDef`          | I or H wide-flange section                                                      |
|  [05]   | `IfcUShapeProfileDef`          | channel section                                                                 |
|  [06]   | `IfcLShapeProfileDef`          | angle section                                                                   |
|  [07]   | `IfcTShapeProfileDef`          | tee section                                                                     |
|  [08]   | `IfcRectangleProfileDef`       | solid rectangle section                                                         |
|  [09]   | `IfcRectangleHollowProfileDef` | rectangular hollow section                                                      |
|  [10]   | `IfcCircleProfileDef`          | solid circle section                                                            |
|  [11]   | `IfcCircleHollowProfileDef`    | round hollow section                                                            |
|  [12]   | `IfcArbitraryClosedProfileDef` | non-parametric closed section                                                   |

[PROFILE_DISCRIMINATION]: `IfcMaterialProfile.Profile` discriminates the cross-section subtype at runtime; hollow subtypes derive from their solid bases, so the wall section resolves before its subtype.

[PROFILE_ROOT_MEMBERS]: `IfcProfileDef` carries `ProfileType` as `IfcProfileTypeEnum` and `ProfileName` as `string`; `MaterialProfile.Kind` discriminates the subtype runtime type.

[PARAMETERIZED_PROFILE_MEMBERS]: `IfcParameterizedProfileDef` extends `IfcProfileDef` and carries `Position` as `IfcAxis2Placement2D`.

[PARAMETRIC_PROFILE_DIMENSIONS]: every I/U/L/T parametric-section dimension is `double`.
- `IfcIShapeProfileDef`: `OverallWidth` `OverallDepth` `WebThickness` `FlangeThickness` `FilletRadius` `FlangeEdgeRadius` `FlangeSlope`
- `IfcUShapeProfileDef`: `Depth` `FlangeWidth` `WebThickness` `FlangeThickness` `FilletRadius` `EdgeRadius` `FlangeSlope`
- `IfcLShapeProfileDef`: `Depth` `Width` `Thickness` `FilletRadius` `EdgeRadius` `LegSlope` `CentreOfGravityInX` `CentreOfGravityInY`
- `IfcTShapeProfileDef`: `Depth` `FlangeWidth` `WebThickness` `FlangeThickness` `FilletRadius` `FlangeEdgeRadius` `WebEdgeRadius` `WebSlope` `FlangeSlope`

[RECTANGLE_PROFILE_DIMENSIONS]: `IfcRectangleProfileDef` carries double `XDim` and `YDim` and is the base of `IfcRectangleHollowProfileDef`; the hollow subtype adds double `WallThickness`, `InnerFilletRadius`, and `OuterFilletRadius`.

[CIRCLE_PROFILE_DIMENSIONS]: `IfcCircleProfileDef` carries double `Radius`, is the fastener nominal-diameter carrier, and is the base of `IfcCircleHollowProfileDef`; the hollow subtype adds double `WallThickness`.

[ARBITRARY_PROFILE_MEMBERS]: `IfcArbitraryClosedProfileDef` carries `OuterCurve` as `IfcCurve` for `DoubleL` and composite sections without a single parametric form; back-to-back sections ride a column.

[PUBLIC_TYPE_SCOPE]: tessellation geometry — AP242/IFC4.3 mesh interchange

| [INDEX] | [SYMBOL]                  | [CAPABILITY]                                                                           |
| :-----: | :------------------------ | :------------------------------------------------------------------------------------- |
|  [01]   | `IfcTessellatedItem`      | abstract tessellated geometry item base; derives from `IfcGeometricRepresentationItem` |
|  [02]   | `IfcTessellatedFaceSet`   | abstract indexed face mesh base; `Closed`, `HasColours`, `HasTextures` properties      |
|  [03]   | `IfcTriangulatedFaceSet`  | triangle mesh: `CoordIndex`, `Normals`, `NormalIndex`, `PnIndex`                       |
|  [04]   | `IfcPolygonalFaceSet`     | polygon mesh; `IfcTessellatedFaceSet` subtype                                          |
|  [05]   | `IfcCartesianPointList`   | abstract packed point list base                                                        |
|  [06]   | `IfcCartesianPointList3D` | packed 3D point list; used as `Coordinates` by tessellated face sets                   |
|  [07]   | `IfcCartesianPointList2D` | packed 2D point list                                                                   |

[PUBLIC_TYPE_SCOPE]: material appearance and presentation interchange

| [INDEX] | [SYMBOL]                              | [CAPABILITY]                                                                    |
| :-----: | :------------------------------------ | :------------------------------------------------------------------------------ |
|  [01]   | `IfcPresentationStyle`                | abstract presentation style root                                                |
|  [02]   | `IfcPresentationItem`                 | abstract item within a presentation style                                       |
|  [03]   | `IfcSurfaceStyle`                     | surface style; holds `Side` (`IfcSurfaceSide`) and `Styles` element set         |
|  [04]   | `IfcSurfaceStyleShading`              | base shading style; `SurfaceColour` (`IfcColourRgb`), `Transparency` (`double`) |
|  [05]   | `IfcSurfaceStyleRendering`            | PBR extension of surface shading                                                |
|  [06]   | `IfcSurfaceStyleWithTextures`         | texture style; references `IfcSurfaceTexture` instances                         |
|  [07]   | `IfcSurfaceStyleLighting`             | additional lighting coefficients: ambient, diffuse, transmission, reflectance   |
|  [08]   | `IfcSurfaceStyleRefraction`           | refraction index and light-transmission factor for optical materials            |
|  [09]   | `IfcSurfaceTexture`                   | abstract surface texture; `RepeatS`, `RepeatT`, `Mode`, `TextureTransform`      |
|  [10]   | `IfcImageTexture`                     | file-path-referenced image texture (`IfcSurfaceTexture` subtype)                |
|  [11]   | `IfcPixelTexture`                     | inline pixel-encoded texture (`IfcSurfaceTexture` subtype)                      |
|  [12]   | `IfcBlobTexture`                      | binary blob texture (`IfcSurfaceTexture` subtype); `RasterCode`, `RasterFormat` |
|  [13]   | `IfcStyledItem`                       | binds a style to a representation item                                          |
|  [14]   | `IfcStyledRepresentation`             | representation holding only styled items                                        |
|  [15]   | `IfcMaterialDefinitionRepresentation` | links an `IfcMaterial` to its `IfcStyledRepresentation` set                     |
|  [16]   | `IfcColourRgb`                        | normalized RGB colour value                                                     |
|  [17]   | `IfcColourRgbList`                    | packed list of RGB colour triples for indexed colour sets                       |

[COLOUR_RGB_MEMBERS]: `IfcColourRgb` carries normalized double `Red`, `Green`, and `Blue`, and constructs from `(DatabaseIfc, double red, double green, double blue)` or `(DatabaseIfc, System.Drawing.Color)`.

[SURFACE_RENDERING_MEMBERS]: `IfcSurfaceStyleRendering` extends shading with PBR `DiffuseColour`, `SpecularColour`, and `ReflectanceMethod` as `IfcReflectanceMethodEnum`.

[PUBLIC_TYPE_SCOPE]: units, presentation, and attributes

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                                                 |
| :-----: | :--------------------------- | :----------------------------------------------------------- |
|  [01]   | `IfcUnitAssignment`          | per-context unit set; nested `Length` enum for project ctors |
|  [02]   | `IfcSIUnit`                  | SI base/derived unit                                         |
|  [03]   | `IfcConversionBasedUnit`     | unit defined by conversion factor                            |
|  [04]   | `IfcDerivedUnit`             | compound derived unit                                        |
|  [05]   | `IfcMonetaryUnit`            | ISO 4217 currency unit                                       |
|  [06]   | `IfcMeasureWithUnit`         | value bound to a unit                                        |
|  [07]   | `IfcClassificationReference` | external-classification reference                            |
|  [08]   | `IfcClassification`          | classification source dictionary                             |
|  [09]   | `VersionAddedAttribute`      | reflection attribute marking schema-version availability     |
|  [10]   | `IfcMeasureValue`            | abstract numeric measure-value base                          |
|  [11]   | `IfcMonetaryMeasure`         | monetary amount measure                                      |

[MONETARY_UNIT_MEMBERS]: `IfcMonetaryUnit.Currency` is an ISO 4217 string and supplies the `IfcMeasureWithUnit.UnitComponent` of a priced `IfcCostValue`.

[MEASURE_WITH_UNIT_MEMBERS]: `IfcMeasureWithUnit` carries `ValueComponent` as `IfcValue` and `UnitComponent` as `IfcUnit`, and it supplies `IfcCostValue.UnitBasis`.

[CLASSIFICATION_REFERENCE_MEMBERS]: `IfcClassificationReference` carries `ReferencedSource` (`IfcClassificationReferenceSelect`) and inherits `Identification`, `Location`, and `Name` from `IfcExternalReference`.

[CLASSIFICATION_MEMBERS]: `IfcClassificationReference.ReferencedSource` names an `IfcClassification` dictionary; `Name` carries the dictionary title, `Source` and `Edition` the publisher and edition, `EditionDate` defaulting to `DateTime.MinValue` when unset, and `Location` the dictionary URI.

[MEASURE_VALUE_MEMBERS]: `IfcMeasureValue` extends `IfcValue`, carries double `Measure`, and is the per-basis denominator narrow target for `IfcMeasureWithUnit.ValueComponent`.

[MONETARY_MEASURE_MEMBERS]: `IfcMonetaryMeasure` extends `IfcDerivedMeasureValue` and `IfcValue`, carries double `Measure`, and is the applied-cost narrow target for `IfcCostValue.AppliedValue`.

[PUBLIC_TYPE_SCOPE]: structural-connection realizing-element surface

| [INDEX] | [SYMBOL]                              | [CAPABILITY]                                  |
| :-----: | :------------------------------------ | :-------------------------------------------- |
|  [01]   | `IfcRelConnectsElements`              | abstract two-element connection               |
|  [02]   | `IfcRelConnectsWithRealizingElements` | realizing-element connection relation         |
|  [03]   | `IfcMechanicalFastener`               | mechanical fastener element component         |
|  [04]   | `IfcMechanicalFastenerTypeEnum`       | mechanical-fastener kind                      |
|  [05]   | `IfcFastener`                         | non-mechanical fastener element component     |
|  [06]   | `IfcFastenerTypeEnum`                 | non-mechanical-fastener kind                  |
|  [07]   | `IfcDiscreteAccessory`                | fabricated accessory element component        |
|  [08]   | `IfcDiscreteAccessoryTypeEnum`        | discrete-accessory kind                       |
|  [09]   | `IfcReinforcingElement`               | abstract reinforcing element component        |
|  [10]   | `IfcReinforcingBar`                   | reinforcing bar                               |
|  [11]   | `IfcReinforcingBarTypeEnum`           | reinforcing-bar kind                          |
|  [12]   | `IfcReinforcingBarSurfaceEnum`        | reinforcing-bar surface finish                |
|  [13]   | `IfcReinforcingMesh`                  | reinforcing mesh                              |
|  [14]   | `IfcReinforcingMeshTypeEnum`          | reinforcing-mesh kind                         |
|  [15]   | `IfcTendon`                           | post-tensioning prestressing element          |
|  [16]   | `IfcTendonTypeEnum`                   | tendon kind                                   |
|  [17]   | `IfcTendonAnchor`                     | tendon anchorage element                      |
|  [18]   | `IfcTendonAnchorTypeEnum`             | tendon-anchor kind                            |
|  [19]   | `IfcBearing`                          | structural support bearing                    |
|  [20]   | `IfcBearingTypeEnum`                  | bearing kind                                  |
|  [21]   | `IfcMaterialProfileSetUsage`          | element profile-set usage                     |
|  [22]   | `IfcCircleProfileDef`                 | circular profile and nominal-diameter carrier |
|  [23]   | `IfcVibrationIsolator`                | isolation-bearing element component           |
|  [24]   | `IfcVibrationIsolatorTypeEnum`        | vibration-isolator kind                       |

[CONNECTS_ELEMENTS_MEMBERS]: `IfcRelConnectsElements` carries `RelatingElement`/`RelatedElement` (`IfcElement`) and `ConnectionGeometry` (`IfcConnectionGeometry`).

[VIBRATION_ISOLATOR_MEMBERS]: `IfcVibrationIsolator` extends `IfcElementComponent` and carries `PredefinedType` as `IfcVibrationIsolatorTypeEnum` (`NOTDEFINED` `USERDEFINED` `COMPRESSION` `SPRING` `BASE`).

[REALIZING_CONNECTION_MEMBERS]: `IfcRelConnectsWithRealizingElements` extends `IfcRelConnectsElements`, carries read-only `RealizingElements` as `SET<IfcElement>`, and holds internal `ConnectionType` for the realizing-fastener relation.

[MECHANICAL_FASTENER_MEMBERS]: `IfcMechanicalFastener` extends `IfcElementComponent`; its only public scalar is `PredefinedType` as `IfcMechanicalFastenerTypeEnum`, while `mNominalDiameter` and `mNominalLength` are internal without public getters.

[MECHANICAL_FASTENER_VALUES]: `IfcMechanicalFastenerTypeEnum` `NOTDEFINED` `USERDEFINED` `ANCHORBOLT` `BOLT` `DOWEL` `NAIL` `NAILPLATE` `RIVET` `SCREW` `SHEARCONNECTOR` `STAPLE` `STUDSHEARCONNECTOR` `COUPLER`.

[FASTENER_MEMBERS]: `IfcFastener` is the non-mechanical `IfcElementComponent`; its only public scalar is `PredefinedType` as `IfcFastenerTypeEnum`, and it carries no public diameter.

[FASTENER_VALUES]: `IfcFastenerTypeEnum` `NOTDEFINED` `USERDEFINED` `GLUE` `MORTAR` `WELD`.

[DISCRETE_ACCESSORY_MEMBERS]: `IfcDiscreteAccessory` extends `IfcElementComponent`, carries `PredefinedType` as `IfcDiscreteAccessoryTypeEnum`, models the framing-connector body, and binds its separate `IfcMechanicalFastener` through `IfcRelConnectsWithRealizingElements`.

[DISCRETE_ACCESSORY_VALUES]: `IfcDiscreteAccessoryTypeEnum` `NOTDEFINED` `USERDEFINED` `ANCHORPLATE` `BRACKET` `SHOE` `EXPANSION_JOINT_DEVICE` `BIRDPROTECTION` `CABLEARRANGER` `INSULATOR` `LOCK` `TENSIONINGEQUIPMENT` `RAILPAD` `SLIDINGCHAIR`.

[REINFORCING_ELEMENT_MEMBERS]: `IfcReinforcingElement` extends `IfcElementComponent` and carries `SteelGrade` for bar and mesh subtypes.

[REINFORCING_BAR_MEMBERS]: `IfcReinforcingBar` extends `IfcReinforcingElement` and carries double `NominalDiameter` (type fallback), `CrossSectionArea`, `BarLength`, `PredefinedType` (`IfcReinforcingBarTypeEnum`), and `BarSurface` (`IfcReinforcingBarSurfaceEnum`).

[REINFORCING_BAR_VALUES]: `IfcReinforcingBarTypeEnum` `NOTDEFINED` `USERDEFINED` `MAIN` `SHEAR` `LIGATURE` `STUD` `PUNCHING` `EDGE` `RING` `ANCHORING` `SPACEBAR`.

[REINFORCING_SURFACE_VALUES]: `IfcReinforcingBarSurfaceEnum` `NOTDEFINED` `PLAIN` `TEXTURED`, on `IfcReinforcingBar.BarSurface`.

[REINFORCING_MESH_MEMBERS]: `IfcReinforcingMesh` extends `IfcReinforcingElement` and exposes `PredefinedType`, `MeshLength`, `MeshWidth`, longitudinal and transverse bar nominal diameter, cross-section area, and spacing; the double measurements default to `NaN` and the reader `Filter` drops non-finite values.

[REINFORCING_MESH_VALUES]: `IfcReinforcingMeshTypeEnum` carries `NOTDEFINED` and `USERDEFINED` through `IfcReinforcingMesh.PredefinedType`.

[TENDON_MEMBERS]: `IfcTendon` extends `IfcReinforcingElement` and exposes only `PredefinedType`; nominal diameter, cross-section area, tension force, prestress, friction coefficient, anchorage slip, and minimum curvature radius are internal without public getters. Constructor `(IfcObjectDefinition, IfcObjectPlacement, IfcProductDefinitionShape, double diam, double area)` sets diameter and area; reads recover diameter through `IfcCircleProfileDef.Radius`.

[TENDON_VALUES]: `IfcTendonTypeEnum` `NOTDEFINED` `USERDEFINED` `STRAND` `WIRE` `BAR` `COATED`.

[TENDON_ANCHOR_MEMBERS]: `IfcTendonAnchor` extends `IfcReinforcingElement`, carries `PredefinedType` as `IfcTendonAnchorTypeEnum`, constructs from `(IfcObjectDefinition host, IfcObjectPlacement, IfcProductDefinitionShape)`, and maps to a `Cast` joint.

[TENDON_ANCHOR_VALUES]: `IfcTendonAnchorTypeEnum` `NOTDEFINED` `USERDEFINED` `COUPLER` `FIXED_END` `TENSIONING_END`.

[BEARING_MEMBERS]: `IfcBearing` extends `IfcBuiltElement`, carries `PredefinedType` as `IfcBearingTypeEnum`, constructs from `()`, `(DatabaseIfc)`, or `(IfcObjectDefinition host, IfcObjectPlacement, IfcProductDefinitionShape)`, and maps to a `Bearing` joint rather than `IfcElementComponent`.

[BEARING_VALUES]: `IfcBearingTypeEnum` `NOTDEFINED` `USERDEFINED` `CYLINDRICAL` `SPHERICAL` `ELASTOMERIC` `POT` `GUIDE` `ROCKER` `ROLLER` `DISK`.

[PROFILE_SET_USAGE_MEMBERS]: `IfcMaterialProfileSetUsage.ForProfileSet` binds an `IfcMaterialProfileSet` to an element; `CardinalPoint` defaults to `MID`, and `ReferenceExtent` defaults to `NaN`, carrying profile placement and the fastener or tendon nominal-diameter circle-profile channel.

[CIRCLE_PROFILE_USE]: `IfcCircleProfileDef.Radius` is the fastener or tendon nominal-diameter carrier reached through `IfcRelAssociatesMaterial.RelatingMaterial`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DatabaseIfc — construction and read

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `new DatabaseIfc()`                                       | ctor     | empty database at default schema                 |
|  [02]   | `new DatabaseIfc(string)`                                 | ctor     | reads an IFC file by path, format inferred       |
|  [03]   | `new DatabaseIfc(TextReader)`                             | ctor     | reads an IFC stream                              |
|  [04]   | `new DatabaseIfc(ReleaseVersion)`                         | ctor     | empty database at a chosen schema version        |
|  [05]   | `new DatabaseIfc(ModelView)`                              | ctor     | empty database at a chosen model view            |
|  [06]   | `new DatabaseIfc(bool, ReleaseVersion)`                   | ctor     | new database, optionally seeding common entities |
|  [07]   | `new DatabaseIfc(bool, ModelView)`                        | ctor     | new database for a model view, optionally seeded |
|  [08]   | `new DatabaseIfc(DatabaseIfc)`                            | ctor     | copy-construct a database                        |
|  [09]   | `DatabaseIfc.ParseString(string) -> DatabaseIfc`          | static   | parses an in-memory STEP/IFC string              |
|  [10]   | `DatabaseIfc.ReadJSONFile(string\|TextReader)`            | instance | loads IFC-JSON into the database                 |
|  [11]   | `DatabaseIfc.ReadJSON(JsonObject) -> List<IBaseClassIfc>` | instance | parses an IFC-JSON document object               |
|  [12]   | `DatabaseIfc.ReadXMLFile(string)`                         | instance | loads IFC-XML into the database                  |
|  [13]   | `DatabaseIfc.ReadXMLDoc(XmlDocument)`                     | instance | loads IFC-XML from a DOM                         |

[ENTRYPOINT_SCOPE]: DatabaseIfc — write and serialize

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `DatabaseIfc.WriteFile(string) -> bool`                                | instance | writes a STEP physical file by path         |
|  [02]   | `DatabaseIfc.WriteSTEPFile(string, SetProgressBarCallback) -> bool`    | instance | writes STEP with progress callback          |
|  [03]   | `DatabaseIfc.WriteSTEPZipFile(string, SetProgressBarCallback) -> bool` | instance | writes a zipped STEP file                   |
|  [04]   | `DatabaseIfc.WriteStream(Stream, string) -> bool`                      | instance | writes STEP to a stream                     |
|  [05]   | `DatabaseIfc.ToString(FormatIfcSerialization) -> string`               | instance | serializes to `STEP`, `XML`, or `JSON` text |
|  [06]   | `DatabaseIfc.ToJSON(string) -> JsonObject`                             | instance | builds an IFC-JSON document object          |
|  [07]   | `DatabaseIfc.ToJSON(string, BaseClassIfc.SetJsonOptions)`              | instance | builds IFC-JSON with serialization options  |
|  [08]   | `DatabaseIfc.JSON() -> JsonObject`                                     | instance | serializes the whole database to IFC-JSON   |
|  [09]   | `DatabaseIfc.XmlDocument() -> XmlDocument`                             | instance | builds an IFC-XML DOM                       |
|  [10]   | `DatabaseIfc.WriteXmlFile(string) -> bool`                             | instance | writes an IFC-XML file                      |
|  [11]   | `DatabaseIfc.XmlString() -> string`                                    | instance | serializes to IFC-XML text                  |

[ENTRYPOINT_SCOPE]: DatabaseIfc — model access and policy

| [INDEX] | [SURFACE]                                    | [SHAPE]     | [CAPABILITY]                                     |
| :-----: | :------------------------------------------- | :---------- | :----------------------------------------------- |
|  [01]   | `DatabaseIfc.Project`                        | property    | the root context as `IfcProject` when applicable |
|  [02]   | `DatabaseIfc.Context`                        | property    | the active context                               |
|  [03]   | `DatabaseIfc.Factory`                        | property    | per-database entity factory                      |
|  [04]   | `DatabaseIfc.Release`                        | property    | active schema version                            |
|  [05]   | `DatabaseIfc.ModelView`                      | property    | active model view                                |
|  [06]   | `DatabaseIfc.Tolerance`                      | property    | geometric tolerance                              |
|  [07]   | `DatabaseIfc.ToleranceAngleRadians`          | property    | angular tolerance in radians                     |
|  [08]   | `DatabaseIfc.ScaleAngle()`                   | method      | active angle scale factor (arity 0)              |
|  [09]   | `IfcUnitAssignment.ScaleSI`                  | method      | context-unit to SI scale factor                  |
|  [10]   | `DatabaseIfc.this[int stepId]`               | indexer     | entity by STEP record id                         |
|  [11]   | `DatabaseIfc.this[string globalID]`          | indexer     | entity by IFC GlobalId                           |
|  [12]   | `DatabaseIfc` enumeration                    | enumeration | iterates all entities                            |
|  [13]   | `DatabaseSTEP<T>.NextObjectRecord`           | property    | next STEP record id                              |
|  [14]   | `DatabaseSTEP<T>.OriginatingFileInformation` | field       | source-file header metadata                      |

[SCALE_SI_OVERLOADS]: `IfcUnitAssignment.ScaleSI` accepts `IfcUnitEnum` or `IfcDerivedUnitEnum` and lives on the context's `IfcUnitAssignment` rather than `DatabaseIfc`.

[ENTRYPOINT_SCOPE]: BaseClassIfc — traversal and entity serialization

`BaseClassIfc.Extract<T>` is constrained to `IBaseClassIfc`.

| [INDEX] | [SURFACE]                             | [SHAPE]                | [CAPABILITY]                                        |
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

| [INDEX] | [SURFACE]                                                  | [SHAPE] | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------------- | :------ | :---------------------------------------- |
|  [01]   | `ParserIfc.ParseEnum<T>(string, string?) -> T`             | static  | parses an IFC enum literal                |
|  [02]   | `ParserIfc.DecodeGlobalID(string) -> Guid`                 | static  | decodes a base64 IFC GlobalId to a `Guid` |
|  [03]   | `ParserIfc.EncodeGuid(Guid) -> string`                     | static  | encodes a `Guid` to an IFC GlobalId       |
|  [04]   | `ParserIfc.HashGlobalID(string) -> string`                 | static  | deterministic GlobalId from a stable key  |
|  [05]   | `ParserIfc.FormatLength(double, DatabaseIfc) -> string`    | static  | formats a length per database units       |
|  [06]   | `ParserIfc.IdentifyIfcClass(string, out string) -> string` | static  | splits class name and predefined type     |

[ENTRYPOINT_SCOPE]: type-occurrence, predefined-type, and domain traversal

`PredefinedType` is a strongly-typed per-class enum member (`IfcWall.PredefinedType` is `IfcWallTypeEnum`); `IdentifyIfcClass` splits the predefined token from the class name at parse, so the occurrence reads its predefined string without a per-class branch.

| [INDEX] | [SURFACE]                           | [SHAPE]                         | [CAPABILITY]                                              |
| :-----: | :---------------------------------- | :------------------------------ | :-------------------------------------------------------- |
|  [01]   | `IfcObject.IsTypedBy`               | `IfcRelDefinesByType` set       | the type-binding relationships of an occurrence           |
|  [02]   | `IfcRelDefinesByType.RelatingType`  | `IfcTypeObject` property        | the type object an occurrence is bound to                 |
|  [03]   | `IfcTypeObject.HasPropertySets`     | `IfcPropertySetDefinition` set  | the type-bound property/quantity sets occurrences inherit |
|  [04]   | `IfcTypeProduct.RepresentationMaps` | `IfcRepresentationMap` set      | the type's reusable instanced-geometry library            |
|  [05]   | `IfcElementType.ElementType`        | `string` property               | the type's predefined element-type identifier             |
|  [06]   | `IfcMappedItem.MappingSource`       | `IfcRepresentationMap` property | the representation map an occurrence instances            |
|  [07]   | `IfcMappedItem.MappingTarget`       | transform property              | the per-occurrence instance transform                     |
|  [08]   | `IfcWall.PredefinedType`            | class-specific enum property    | strongly-typed element predefined kind                    |
|  [09]   | `IfcObject.ObjectType`              | `string` property               | the user-defined type string for `USERDEFINED` predefined |

[MAPPING_TARGET_TYPE]: `IfcMappedItem.MappingTarget` is an `IfcCartesianTransformationOperator` property.

[ENTRYPOINT_SCOPE]: georeferencing, scheduling, and grouping traversal

| [INDEX] | [SURFACE]                           | [SHAPE]                           | [CAPABILITY]                                               |
| :-----: | :---------------------------------- | :-------------------------------- | :--------------------------------------------------------- |
|  [01]   | `HasCoordinateOperation`            | `IfcCoordinateOperation` property | context coordinate operation                               |
|  [02]   | `SourceCRS`/`TargetCRS`             | CRS properties                    | engineering and projected frames                           |
|  [03]   | `IfcProject.RepresentationContexts` | `IfcRepresentationContext` set    | the project's representation contexts (model/plan)         |
|  [04]   | `IfcTask.TaskTime`                  | `IfcTaskTime` property            | the task's schedule/actual start-finish-duration           |
|  [05]   | `IsSuccessorFrom`/`IsPredecessorTo` | `IfcRelSequence` set              | task dependency edges                                      |
|  [06]   | `IfcRelSequence.TimeLag`            | `IfcLagTime` property             | the dependency lag (duration + `IfcTaskDurationEnum`)      |
|  [07]   | `IfcGroup.IsGroupedBy`              | `IfcRelAssignsToGroup` set        | the assignment relationships grouping objects into a group |
|  [08]   | `IfcCostItem.CostValues`            | `IfcCostValue` set                | the cost rates/values applied to a cost line item          |
|  [09]   | `IfcDistributionPort.ContainedIn`   | containment relation              | the owning distribution element                            |

[CONTEXT_COORDINATE_OPERATION]: `IfcGeometricRepresentationContext.HasCoordinateOperation` carries the single map-conversion or CRS operation; `IfcMapConversion` extends `IfcCoordinateOperation` and narrows through `as`.

[MAP_CONVERSION_CRS_MEMBERS]: `IfcMapConversion.SourceCRS` is `IfcCoordinateReferenceSystemSelect`, and `TargetCRS` is `IfcCoordinateReferenceSystem`.

[PORT_CONTAINMENT_TYPE]: `IfcDistributionPort.ContainedIn` is an `IfcRelConnectsPortToElement` property that identifies the owning distribution element.

[PROCESS_DEPENDENCY_SURFACE]: `IfcProcess.IsSuccessorFrom` and `IfcProcess.IsPredecessorTo` carry predecessor and successor `IfcRelSequence` edges.

[ENTRYPOINT_SCOPE]: structural-connection realizing-element construction and read

| [INDEX] | [SURFACE]                                 | [SHAPE]                  | [CAPABILITY]                   |
| :-----: | :---------------------------------------- | :----------------------- | :----------------------------- |
|  [01]   | `new IfcRelConnectsWithRealizingElements` | relation constructor     | single-realizing-element joint |
|  [02]   | `RealizingElements`                       | mutable read-only set    | realizing elements             |
|  [03]   | `new IfcMechanicalFastener`               | occurrence constructor   | generic mechanical fastener    |
|  [04]   | `new IfcMechanicalFastener`               | profile constructor      | profiled mechanical fastener   |
|  [05]   | `IfcMechanicalFastener.PredefinedType`    | enum property            | public fastener kind           |
|  [06]   | `new IfcFastener`                         | occurrence constructor   | non-mechanical fastener        |
|  [07]   | `new IfcDiscreteAccessory`                | occurrence constructors  | framing accessory              |
|  [08]   | `IfcElement.HasAssociations`              | association set          | material read path             |
|  [09]   | `RelatingMaterial`                        | material-select property | profile-usage arm              |
|  [10]   | `ForProfileSet`                           | material-profile chain   | nominal-diameter profile       |
|  [11]   | `IfcReinforcingBar.NominalDiameter`       | double property          | public bar diameter            |
|  [12]   | `IfcReinforcingBar.BarSurface`            | enum property            | public bar finish              |

[REALIZING_RELATION_CTOR]: `IfcRelConnectsWithRealizingElements` constructs from `(IfcConnectionGeometry cg, IfcElement relating, IfcElement related, IfcElement realizing)` and registers `IfcElement.IsConnectionRealization`.

[REALIZING_ELEMENTS_MUTATION]: `IfcRelConnectsWithRealizingElements.RealizingElements` is a read-only `SET<IfcElement>` property mutated through `.Add` or `.AddRange`.

[MECHANICAL_FASTENER_OCCURRENCE_CTOR]: `IfcMechanicalFastener` constructs from `(IfcObjectDefinition host, IfcObjectPlacement, IfcProductDefinitionShape)` — no single-argument `(DatabaseIfc)` ctor exists; authoring hosts on `db.Project` at the factory root placement.

[MECHANICAL_FASTENER_PROFILE_CTOR]: `IfcMechanicalFastener` also constructs from `(IfcProduct host, IfcMaterialProfileSetUsage, IfcAxis2Placement3D, double length)`, binding nominal diameter and length through the `Profiled` path.

[MECHANICAL_FASTENER_PREDEFINED_TYPE]: `IfcMechanicalFastener.PredefinedType` is a schema-validated get/set `IfcMechanicalFastenerTypeEnum`; welded studs use `STUDSHEARCONNECTOR`, while discrete fasteners use `BOLT` or `ANCHORBOLT`.

[FASTENER_OCCURRENCE_CTOR]: `IfcFastener` constructs from `(IfcObjectDefinition host, IfcObjectPlacement placement, IfcProductDefinitionShape rep)` for weld or adhesive beads, then receives `PredefinedType` as `IfcFastenerTypeEnum`.

[DISCRETE_ACCESSORY_CTORS]: `IfcDiscreteAccessory` constructs from `(IfcObjectDefinition host, IfcObjectPlacement placement, IfcProductDefinitionShape rep)` or `(IfcProduct host, IfcMaterialProfileSetUsage profile, IfcAxis2Placement3D placement, double length)`, then receives `PredefinedType` as `SHOE`, `BRACKET`, or `ANCHORPLATE` in `IfcDiscreteAccessoryTypeEnum`.

[ELEMENT_ASSOCIATION_READ]: `IfcElement.HasAssociations` is a `SET<IfcRelAssociates>` carrying `IfcRelAssociatesMaterial` for the fastener profile-usage nominal-diameter path.

[RELATING_MATERIAL_READ]: `IfcRelAssociatesMaterial.RelatingMaterial` is an `IfcMaterialSelect`; its `IfcMaterialProfileSetUsage` arm recovers nominal fastener scalars.

[PROFILE_SET_DIAMETER_READ]: `IfcMaterialProfileSetUsage.ForProfileSet` reaches `IfcMaterialProfileSet.MaterialProfiles`, then each `IfcMaterialProfile.Profile`; `IfcCircleProfileDef.Radius` multiplied by two yields nominal diameter.

[REINFORCING_BAR_DIAMETER_READ]: `IfcReinforcingBar.NominalDiameter` is a double whose getter falls back to `IfcReinforcingBarType.NominalDiameter`.

[REINFORCING_BAR_SURFACE_READ]: `IfcReinforcingBar.BarSurface` is a get/set `IfcReinforcingBarSurfaceEnum`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One managed `GeometryGymIFCcore.dll` per TFM holds the STEP/IFC data graph alone — no P/Invoke, no native binary, no geometry kernel; I/O is text serialization (STEP, IFC-XML, IFC-JSON) over `string`/`Stream`/`TextReader`/`XmlDocument`.
- `DatabaseIfc` is the sole entity store: every entity is db-bound, indexed by STEP id or GlobalId, and enumerable as `IEnumerable<BaseClassIfc>`; `Release`/`ModelView` schema state is database-level, auto-resolved from the file header on read, never a per-call argument.
- `FactoryIfc.Construct`/`ConstructProduct`/`ConstructElement` mints entities registered on the owning database; `BaseClassIfc.Construct`/`GetType` is the db-free reflective mint, and `BaseClassIfc.Extract<T>` collects every reachable `T`.
- Every `IfcRoot` carries `GlobalId`/`Guid`/`OwnerHistory` identity; the GlobalId codec lives on `ParserIfc` and the ownership stamp threads from `FactoryIfc.OwnerHistoryAdded`.
- STEP geometry maps to tessellation (`IfcTriangulatedFaceSet`/`IfcPolygonalFaceSet`), swept (`IfcExtrudedAreaSolid`), BREP (`IfcAdvancedBrep`/`IfcFacetedBrep`), and instanced (`IfcMappedItem`) families, material appearance riding the `IfcStyledItem` -> `IfcSurfaceStyle` chain; the package evaluates none of it.
- Authoring bootstraps from `new IfcProject(...)`, nests through `IfcRelAggregates`/`IfcObjectDefinition.AddAggregated`, and binds property, material, and type definitions through the `IfcRelDefinesByProperties`/`IfcRelAssociatesMaterial`/`IfcRelDefinesByType` families.

[STACKING]:
- `geometry3sharp`(`api-geometry3sharp.md`): `IfcTriangulatedFaceSet.CoordIndex` over `IfcCartesianPointList3D.Coordinates` lowers to a `g3.DMesh3` indexed triangle mesh for host-free geometry.
- `sharpgltf`(`api-sharpgltf.md`): the same tessellated face-set stream feeds `SceneBuilder.AddRigidMesh(IMeshBuilder<M>, ...)` -> `ToGltf2` glTF export.
- `honeybee-schema`(`api-honeybee-schema.md`): the `IfcSpace`/`IfcBuildingStorey` spatial graph maps to `HoneybeeSchema.Room` on the energy-exchange rail.
- Bim `Exchange` owner: composes `DatabaseIfc` import/export and `BaseClassIfc.Extract<T>` traversal as the IFC leg of the model-exchange owner feeding the geometry-interchange rail.

[LOCAL_ADMISSION]:
- IFC import enters through `new DatabaseIfc(path\|stream)` or the format-explicit `Read*` calls.
- IFC export enters through `DatabaseIfc.WriteFile` or `DatabaseIfc.ToString(FormatIfcSerialization)`.
- Model queries enter through `DatabaseIfc` indexing/enumeration and `BaseClassIfc.Extract<T>`.

[RAIL_LAW]:
- Package: `GeometryGymIFC_Core`
- Owns: buildingSMART IFC object model, schema-versioned STEP/XML/JSON serialization
- Accept: IFC data exchange, model authoring, model traversal and query
- Reject: tessellation, BREP evaluation, geometry kernel, native rendering
