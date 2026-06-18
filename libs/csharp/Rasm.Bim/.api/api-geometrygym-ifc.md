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

| [INDEX] | [SYMBOL]               | [RAIL]   | [CAPABILITY]                                                                                 |
| :-----: | :--------------------- | :------- | :------------------------------------------------------------------------------------------- |
|   [1]   | `DatabaseIfc`          | geometry | IFC model repository; owns all entities, schema, units, tolerance, and I/O                   |
|   [2]   | `DatabaseSTEP<T>`      | geometry | generic STEP record store; `IEnumerable<T>`, `this[int stepId]`, `NextObjectRecord`          |
|   [3]   | `BaseClassIfc`         | geometry | abstract root of every IFC entity; carries `Database`, `Extract<T>`, STEP/JSON serialization |
|   [4]   | `STEPEntity`           | geometry | base STEP record carrier under `BaseClassIfc`                                                |
|   [5]   | `FactoryIfc`           | geometry | per-database factory; canonical axes, origins, placements, application, owner history        |
|   [6]   | `ParserIfc`            | geometry | static STEP/enum/GUID codec; `ParseEnum<T>`, `DecodeGlobalID`, `EncodeGuid`                  |
|   [7]   | `ParserSTEP`           | geometry | static low-level STEP token parser                                                           |
|   [8]   | `SerializationIfc`     | geometry | serialization base for the IFC formats                                                       |
|   [9]   | `SerializationIfcSTEP` | geometry | STEP physical-file serialization implementation                                              |
|  [10]   | `STEPFileInformation`  | geometry | originating-file header metadata on the database                                             |
|  [11]   | `DuplicateOptions`     | geometry | options carrier for cross-database entity duplication                                        |
|  [12]   | `DuplicateMapping`     | geometry | source-to-target entity map during duplication                                               |

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
|  [15]   | `IfcMappedItem`                     | geometry | instanced representation map reference           |

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
