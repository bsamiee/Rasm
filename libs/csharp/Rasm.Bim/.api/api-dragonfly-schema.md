# [RASM_BIM_API_DRAGONFLY_SCHEMA]

`DragonflySchema` binds the Ladybug Tools Dragonfly urban building-energy schema and its DFJSON Newtonsoft codec — a `Model` → `Building` → `Story` → `Room2D` hierarchy of extruded footprint polygons whose parameter slots are `AnyOf<…>` unions and whose detailed physics is referenced by abridged identifier. Every type derives from `OpenAPIGenBaseModel`, so one generated round-trip, validation, duplication, and equality surface spans the schema. It fronts the energy-model rail with the massing wire and footprint geometry, composing the Honeybee physics vocabulary rather than re-declaring it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DragonflySchema`
- package: `DragonflySchema` (MIT, Ladybug Tools)
- assembly: `DragonflySchema`
- namespace: `DragonflySchema` (flat — the OpenAPI generator emits no sub-namespaces; `ModelEnergyProperties`/`ModelRadianceProperties`/`ModelDoe2Properties`/`ModelComparisonProperties` live here, `Energy`/`Radiance`/`Doe2`/`Comparison` naming the `ModelProperties` sub-object axis, not namespaces)
- asset: `netstandard2.0` single TFM, binds forward under net10.0
- serialization: `LBT.Newtonsoft.Json` (the Ladybug Newtonsoft fork) via `OpenAPIGenBaseModel.ToJson`/`FromJson`; DFJSON is Newtonsoft, never `System.Text.Json`
- validation: `System.ComponentModel.Annotations` — `[Required]`/`[Range]` attributes feed `Validate()`
- consumer: `libs/csharp/Rasm.Bim`
- rail: energy-model

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: generated serialization base

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [CAPABILITY]                                                                               |
| :-----: | :-------------------- | :------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `OpenAPIGenBaseModel` | abstract class | `ToJson`/`FromJson`, `Duplicate`, `IsValid`/`Validate`, `Equals`/`GetHashCode`, `ToString` |

[PUBLIC_TYPE_SCOPE]: massing geometry hierarchy

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `Model`        | class         | root massing model (`IDdBaseModel`)                 |
|  [02]   | `Building`     | class         | story stack plus optional full 3D rooms             |
|  [03]   | `Story`        | class         | floor-plate stack with vertical `Multiplier` repeat |
|  [04]   | `Room2D`       | class         | extruded footprint floor-plate                      |
|  [05]   | `ContextShade` | class         | unconditioned site-context shading geometry         |

- `Model`: `Buildings`, `ContextShades`, `Properties`, `Units`, `Tolerance`/`AngleTolerance`, `ReferenceVector`, `Version`.
- `Building`: `UniqueStories`, `Room3ds` (`List<HoneybeeSchema.Room>`), `Roof`, `Properties` (`BuildingPropertiesAbridged`).
- `Story`: `Room2ds`, `FloorToFloorHeight`/`FloorHeight` (`AnyOf<Autocalculate,double>`), `Multiplier`, `Roof`, `StoryType`.
- `Room2D`: `FloorBoundary` (`List<List<double>>`), `FloorHoles`, `FloorHeight`/`FloorToCeilingHeight`, the `BoundaryConditions`/`WindowParameters`/`ShadingParameters`/`SkylightParameters` `AnyOf<…>` slots, `IsGroundContact`/`IsTopExposed`/`HasFloor`/`HasCeiling`, `Zone`, `AirBoundaries`.
- `ContextShade`: `Face3D`/`Mesh3D` faces outside the conditioned model.

[PUBLIC_TYPE_SCOPE]: aperture, shading, skylight, roof parameter unions — the `Room2D`/`RoofSpecification` `AnyOf<…>` discriminant roots

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `IWindowParameter`   | interface     | per-wall-segment window placement strategy      |
|  [02]   | `IShadingParameter`  | interface     | per-wall-segment exterior shading device        |
|  [03]   | `ISkylightParameter` | interface     | roof glazing on a `Room2D`                      |
|  [04]   | `IRoof`              | interface     | per-story roof geometry plus clerestory glazing |

- [WINDOW]: `SingleWindow` `SimpleWindowArea` `SimpleWindowRatio` `RepeatingWindowRatio` `RepeatingWindowWidthHeight` `RectangularWindows` `DetailedWindows`.
- [SHADING]: `Overhang` `ExtrudedBorder` `LouversByCount` `LouversByDistance`.
- [SKYLIGHT]: `GriddedSkylightArea` `GriddedSkylightRatio` `DetailedSkylights`.
- [ROOF]: `RoofSpecification` `DetailedClearstory`.

[PUBLIC_TYPE_SCOPE]: extension properties and Radiance grids

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :---------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `ModelProperties`       | class         | the extension hub                             |
|  [02]   | `ModelEnergyProperties` | class         | the model-level energy library                |
|  [03]   | `*PropertiesAbridged`   | class         | per-object abridged-by-identifier carriers    |
|  [04]   | `IGridpar`              | interface     | Radiance sensor-grid parameters on a `Room2D` |

- `ModelProperties`: `Energy` (`ModelEnergyProperties`), `Radiance` (`ModelRadianceProperties`), `Doe2` (`ModelDoe2Properties`), `Comparison` (`ModelComparisonProperties`).
- `ModelEnergyProperties`: `ConstructionSets`, `Constructions`, `Materials`, `Hvacs`, `Shws`, `ProgramTypes`, `Schedules` (each `List<AnyOf<…HoneybeeSchema…>>`), and the baked `GlobalConstructionSet`.
- [GRIDPAR]: `RoomGridParameter` `RoomRadialGridParameter` `ExteriorFaceGridParameter` `ExteriorApertureGridParameter`.

[PUBLIC_TYPE_SCOPE]: enumerations — `[EnumMember]` Newtonsoft string vocabulary

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Units`              | enum          | `Meters`(=1)/`Millimeters`/`Feet`/`Inches`/`Centimeters` |
|  [02]   | `StoryType`          | enum          | `Standard`/`CeilingPlenum`/`FloorPlenum`                 |
|  [03]   | HVAC equipment enums | enum          | per-template equipment and vintage/control vocabulary    |

- [HVAC]: `VAVEquipmentType` `PSZEquipmentType` `FCUEquipmentType` `VRFEquipmentType` `Vintages` `EconomizerType`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DFJSON round-trip and validation — the `OpenAPIGenBaseModel` base surface, uniform across every schema type

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Model.FromJson(string) -> OpenAPIGenBaseModel?`   | static   | parse and validate DFJSON to the typed graph   |
|  [02]   | `node.ToJson(bool) -> string`                      | instance | serialize any node to DFJSON (Newtonsoft fork) |
|  [03]   | `node.IsValid(bool) -> bool`                       | instance | DataAnnotations range/required gate            |
|  [04]   | `node.Validate() -> IEnumerable<ValidationResult>` | instance | the validation-result enumeration              |
|  [05]   | `node.Duplicate() -> OpenAPIGenBaseModel`          | instance | deep structural copy (immutable-edit seam)     |
|  [06]   | `node.Equals(OpenAPIGenBaseModel) -> bool`         | operator | structural value equality (model diffing)      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DragonflySchema` code-generates from the Dragonfly OpenAPI spec; the round-trip is `FromJson` → edit-via-`Duplicate` → `ToJson`, never a hand-edit or subclass. Each object's `"type"` JSON discriminator is the case tag the `AnyOf<…>` resolver reads.
- `FromJson` deserializes through the `AnyOf` converter (`JsonSetting.AnyOfConvertSetting`), enforces the `"type"` discriminator equals the runtime type name, and runs `IsValid(throwException: true)`: a structurally-invalid document throws `ArgumentException`, a type-mismatch or failed parse returns `null`. Explicit `IsValid`/`Validate` re-gates a model the caller built or mutated after construction — generated subtypes do not re-validate on property set — before it crosses into the Honeybee translation.
- A `Room2D` window/shading/skylight/boundary slot is `AnyOf<…>` closed by the schema: read the concrete case by matching the carried `"type"`, set it by assigning a concrete instance, and dispatch every documented case rather than assuming a default. `Story.FloorToFloorHeight`/`FloorHeight` are `AnyOf<Autocalculate,double>` — resolve `Autocalculate` from room geometry, never as `0`.
- Each `*PropertiesAbridged` carrier references the model-level `ModelEnergyProperties`/`ModelRadianceProperties` library by string identifier; the model-level stores hold the actual Honeybee objects, so an abridged property resolves by identifier lookup, never inline expansion.

[STACKING]:
- `HoneybeeSchema`(`.api/api-honeybee-schema`): the detailed-physics vocabulary — `AnyOf<…>`, `Autocalculate`, the geometry primitives (`Point3D`/`Face3D`/`Mesh3D`/`LineSegment3D`), `Building.Room3ds` (`HoneybeeSchema.Room`), the boundary conditions, and the energy library (`OpaqueConstruction`/`WindowConstruction`/`EnergyMaterial…`/the `IHvac` templates/`ScheduleRuleset…`/`ProgramType…`). `ModelEnergyProperties.Constructions`/`Materials`/`Hvacs`/`Schedules`/`ProgramTypes` are `List<AnyOf<…HoneybeeSchema…>>` keyed by identifier; translation to a Honeybee `Model` is the room-level explosion of the 2D plates.
- `NREL.OpenStudio.macOS-arm64`(`.api/api-openstudio`): the energy-model rail — Dragonfly massing feeds the Honeybee room model, `OpenStudio` translates it to the IDF/OSM for EnergyPlus, and simulation results read back against the Dragonfly `Identifier`s.
- `GeometryGymIFC`(`.api/api-geometrygym-ifc`): the IFC model is the geometry source — `IfcSpace`/`IfcBuildingStorey` footprints and storey heights map onto `Room2D`/`Story` at the `Exchange` boundary; `DragonflySchema.*` types stay at the energy-exchange edge.
- `Xbim.InformationSpecifications`(`.api/api-xbim-properties`): IFC Psets supply the construction and program assignments that select the abridged Honeybee library entries.
- `NetTopologySuite`(`libs/csharp/.api/api-nettopologysuite`): `Room2D.FloorBoundary` polygon rings project onto the NTS planar algebra for footprint area, overlap, and adjacency on the canonical Bim site frame.
- `UnitsNet`(`libs/csharp/.api/api-unitsnet`): `Units` drives cross-model quantity reconciliation so a metric Dragonfly model and an imperial source agree on one canonical unit before translation.
- `System.IO.Hashing`(`libs/csharp/.api/api-hashing`): a `Model.ToJson()` UTF-8 string feeds the energy-model snapshot content key, joining the IFC/glTF exports on one content-identity rail; the `Comparison` extension properties carry the structured model-diff alongside the hash.
- within-lib: the Bim energy Exchange resolves each `AnyOf<…>` slot by `"type"` at the boundary and maps the massing hierarchy and Honeybee-referenced properties onto canonical Bim/energy carriers.

[LOCAL_ADMISSION]:
- DFJSON import enters through `Model.FromJson`, gated by `IsValid`/`Validate`, mapping the massing hierarchy and Honeybee-referenced properties onto canonical Bim/energy carriers.
- DFJSON export enters through a canonical build — footprint polygons to `Room2D.FloorBoundary`, storey grouping to `Story` with `Multiplier`, library assembly to `ModelEnergyProperties` Honeybee entries — then `Model.ToJson()`.

[RAIL_LAW]:
- Package: `DragonflySchema`
- Owns: the Dragonfly DFJSON urban energy-massing schema and Newtonsoft codec — the `Model`/`Building`/`Story`/`Room2D` hierarchy, the `AnyOf<…>` window/shading/skylight/boundary parameter unions, the Radiance grid parameters, and the extension-property hub referencing the Honeybee library
- Accept: urban building-energy model interchange, massing-to-room translation input, footprint and storey geometry exchange, DataAnnotations validation
- Reject: re-implementing the Honeybee physics vocabulary (`api-honeybee-schema` owns constructions/materials/HVAC/schedules), running the simulation (`api-openstudio` and EnergyPlus), authoring the source geometry (`api-geometrygym-ifc`), `System.Text.Json` serialization (the wire is the Newtonsoft fork), and leaking `DragonflySchema.*` types past the energy-exchange boundary
