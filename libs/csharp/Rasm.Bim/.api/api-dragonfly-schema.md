# [RASM_BIM_API_DRAGONFLY_SCHEMA]

`DragonflySchema` is the .NET object model + DFJSON codec for the Ladybug Tools Dragonfly
schema — the URBAN building-energy massing layer: a `Model` → `Building` → `Story` → `Room2D`
floor-plate hierarchy where rooms are extruded 2D footprint polygons (not detailed surfaces),
window/shading/skylight/boundary parameters are `AnyOf<…>` discriminated unions, and the
heavy energy/radiance content is referenced by abridged identifier. It is OpenAPI-generated:
every type derives from `OpenAPIGenBaseModel` (Newtonsoft `ToJson`/`FromJson` + DataAnnotations
`Validate`/`IsValid` + structural `Duplicate`/`Equals`), and the entire detailed-physics
vocabulary — `AnyOf`, the geometry primitives (`Face3D`/`Mesh3D`/`Room`), boundary conditions,
and the energy library (constructions, materials, HVAC, schedules, program types) — is OWNED by
`HoneybeeSchema` and composed in. Dragonfly is the massing/urban front of one stack:
Dragonfly (this) → Honeybee (room-level model) → OpenStudio/EnergyPlus (simulation). It owns
the wire and the massing geometry; it never re-implements the Honeybee physics vocabulary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DragonflySchema`
- package: `DragonflySchema`
- version: `2.201.0`
- license: MIT
- assembly: `DragonflySchema`
- namespace: `DragonflySchema` (core geometry + parameters), `DragonflySchema.Energy`, `DragonflySchema.Radiance`, `DragonflySchema.Doe2`, `DragonflySchema.Comparison` (the extension-property sub-models)
- asset: `netstandard2.0` only; the `net10.0` consumer binds `lib/netstandard2.0` (single TFM, binds forward)
- serialization: `LBT.Newtonsoft.Json` (Ladybug's Newtonsoft fork) via `OpenAPIGenBaseModel.ToJson`/`FromJson` — the DFJSON wire is Newtonsoft, NOT `System.Text.Json`
- validation: `System.ComponentModel.Annotations` (`ValidationResult` / `[Required]`/`[Range]` attributes feed `Validate()`)
- transitive-floor: `HoneybeeSchema` (`2.102.0` — the physics vocabulary), `LBT.Newtonsoft.Json` (`13.0.3.2`), `System.ComponentModel.Annotations` (`5.0.0`)
- rail: energy-model

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: generated base and serialization root
- namespace: `DragonflySchema`
- rail: energy-model

Every Dragonfly type derives from `OpenAPIGenBaseModel`, so the JSON round-trip, validation,
duplication, and value equality are uniform across the schema — there is no per-type codec.

| [INDEX] | [SYMBOL]              | [RAIL]       | [CAPABILITY]                                                                                                       |
| :-----: | :-------------------- | :----------- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | `OpenAPIGenBaseModel` | energy-model | `abstract` base (`IEquatable<…>`): `ToJson(bool indented=false)`, static `FromJson(string)`, `Duplicate()`, `IsValid(bool throwException=false)`, `Validate() → IEnumerable<ValidationResult>`, value `Equals`/`GetHashCode`/`==`/`!=`, `ToString(bool detailed)` |

[PUBLIC_TYPE_SCOPE]: massing geometry hierarchy
- namespace: `DragonflySchema`
- rail: energy-model

The hierarchy is the urban form: a `Model` of `Building`s, each a stack of `Story`s, each a
set of `Room2D` floor-plates (extruded footprint polygons). `Building.room3ds` and the
boundary/geometry references are HoneybeeSchema types — Dragonfly is the massing wrapper over
the Honeybee room model.

| [INDEX] | [SYMBOL]        | [RAIL]       | [CAPABILITY]                                                                                                                                   |
| :-----: | :-------------- | :----------- | :------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Model`         | energy-model | root (`IDdBaseModel`,`IModel`): `Identifier`, `Properties` (`ModelProperties`), `Buildings`, `ContextShades`, `Units` (`Units`), `Tolerance`/`AngleTolerance`, `ReferenceVector`, `Version` |
|  [02]   | `Building`      | energy-model | `UniqueStories` (`List<Story>`), `Room3ds` (`List<HoneybeeSchema.Room>` — full 3D rooms), `Roof` (`RoofSpecification`), `Properties` (`BuildingPropertiesAbridged`) |
|  [03]   | `Story`         | energy-model | `Room2ds` (`List<Room2D>`), `FloorToFloorHeight`/`FloorHeight` (`AnyOf<Autocalculate,double>`), `Multiplier` (vertical repeat), `Roof`, `StoryType` (`StoryType`) |
|  [04]   | `Room2D`        | energy-model | floor-plate: `FloorBoundary` (`List<List<double>>`), `FloorHoles`, `FloorHeight`/`FloorToCeilingHeight`, `IsGroundContact`/`IsTopExposed`/`HasFloor`/`HasCeiling`, `…PlenumDepth`, `Zone`, `BoundaryConditions`/`WindowParameters`/`ShadingParameters`/`SkylightParameters` (all `AnyOf<…>`), `AirBoundaries` (`List<bool>`) |
|  [05]   | `ContextShade`  | energy-model | site context shading geometry (`Face3D`/`Mesh3D` faces) outside the conditioned model                                                          |

[PUBLIC_TYPE_SCOPE]: aperture, shading, skylight, roof parameter unions
- namespace: `DragonflySchema`
- rail: energy-model

These are the discriminated members of the `Room2D`/`RoofSpecification` `AnyOf<…>` slots —
the catalog of ways a wall gets glazed, shaded, and skylit. Each base interface
(`IWindowParameter`/`IShadingParameter`/`ISkylightParameter`/`IClearstoryParameter`/`IRoof`)
is the discriminant root.

| [INDEX] | [SYMBOL]                                        | [RAIL]       | [CAPABILITY]                                                                                          |
| :-----: | :---------------------------------------------- | :----------- | :--------------------------------------------------------------------------------------------------- |
|  [01]   | `SingleWindow` / `SimpleWindowArea` / `SimpleWindowRatio` / `RepeatingWindowRatio` / `RectangularWindows` / `DetailedWindows` | energy-model | `WindowParameterBase`/`IWindowParameter` cases — one window placement strategy per `Room2D` wall segment (`SingleWindow(width,height,sillHeight)`, `RepeatingWindowRatio(windowRatio,windowHeight,sillHeight,horizontalSeparation,verticalSeparation)`, …) |
|  [02]   | `Overhang` / `ExtrudedBorder` / `LouversByCount` / `LouversByDistance` | energy-model | `LouversBase`/`IShadingParameter` cases — exterior shading device per wall segment                    |
|  [03]   | `GriddedSkylightArea` / `GriddedSkylightRatio` / `DetailedSkylights` | energy-model | `ISkylightParameter` cases — roof glazing on a `Room2D`                                              |
|  [04]   | `RoofSpecification` / `DetailedClearstory`      | energy-model | `IRoof`/`IClearstoryParameter`: `RoofSpecification(geometry: List<AnyOf<Face3D,Mesh3D>>, clearstoryParameters: List<DetailedClearstory>)` — the per-story roof geometry + clerestory glazing |

[PUBLIC_TYPE_SCOPE]: extension properties and Radiance grids
- namespace: `DragonflySchema`, `DragonflySchema.Energy`, `DragonflySchema.Radiance`
- rail: energy-model

The extension-property pattern: every geometry object carries a `*PropertiesAbridged` whose
`.Energy`/`.Radiance` sub-objects reference the model-level library by identifier; the
model-level `ModelEnergyProperties`/`ModelRadianceProperties` hold the actual (Honeybee) objects.

| [INDEX] | [SYMBOL]                                                          | [RAIL]       | [CAPABILITY]                                                                                                  |
| :-----: | :-------------------------------------------------------------- | :----------- | :---------------------------------------------------------------------------------------------------------- |
|  [01]   | `ModelProperties`                                               | energy-model | the extension hub: `Energy` (`ModelEnergyProperties`), `Radiance` (`ModelRadianceProperties`), `Doe2` (`ModelDoe2Properties`), `Comparison` (`ModelComparisonProperties`) |
|  [02]   | `ModelEnergyProperties`                                          | energy-model | the energy library (all `List<AnyOf<…HoneybeeSchema…>>`): `ConstructionSets`, `Constructions`, `Materials`, `Hvacs`, `Shws`, `ProgramTypes`, `Schedules`, plus the baked `GlobalConstructionSet` default |
|  [03]   | `*PropertiesAbridged` (`Building…`/`Story…`/`Room2D…`/`ContextShade…`) | energy-model | per-object property carriers (`.Energy`/`.Radiance`) holding abridged-by-identifier references into the model library |
|  [04]   | `RoomGridParameter` / `RoomRadialGridParameter` / `ExteriorFaceGridParameter` / `ExteriorApertureGridParameter` | energy-model | `GridParameterBase`/`IGridpar` cases — Radiance sensor-grid generation parameters on a `Room2D` |

[PUBLIC_TYPE_SCOPE]: enumerations
- namespace: `DragonflySchema`
- rail: energy-model

| [INDEX] | [SYMBOL]      | [RAIL]       | [CAPABILITY]                                                                                                        |
| :-----: | :------------ | :----------- | :----------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Units`       | energy-model | `Meters`(=1)/`Millimeters`/`Feet`/`Inches`/`Centimeters` — the model unit system (`[EnumMember]` Newtonsoft names)  |
|  [02]   | `StoryType`   | energy-model | `Standard`/`CeilingPlenum`/`FloorPlenum` — the `Story` role                                                         |
|  [03]   | HVAC equipment-type enums | energy-model | `VAVEquipmentType`/`PSZEquipmentType`/`FCUEquipmentType`/`VRFEquipmentType`/`Vintages`/`EconomizerType`/… — the equipment template vocabulary for the energy HVAC cases |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DFJSON round-trip and validation (the `OpenAPIGenBaseModel` surface)
- namespace: `DragonflySchema`
- rail: energy-model

The codec is the base-class surface — uniform across every schema type. There is no separate
serializer; `ToJson`/`FromJson` wrap the Newtonsoft fork with the schema's converters.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                              | [CAPABILITY]                                                                       |
| :-----: | :------------------------------ | :------------------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `Model.FromJson` / `<T>.FromJson` | `static (string json) → OpenAPIGenBaseModel`            | parse a DFJSON string into the typed graph (downcast to `Model`)                  |
|  [02]   | `.ToJson`                       | `(bool indented = false) → string`                      | serialize any node to DFJSON (Newtonsoft fork)                                     |
|  [03]   | `.IsValid` / `.Validate`        | `(bool throwException = false) → bool` · `() → IEnumerable<ValidationResult>` | DataAnnotations validation (range/required) — gate before handing to Honeybee/OpenStudio |
|  [04]   | `.Duplicate`                    | `() → OpenAPIGenBaseModel`                              | deep structural copy (the immutable-edit seam)                                     |
|  [05]   | `.Equals` / `==` / `GetHashCode` | `(OpenAPIGenBaseModel) → bool`                          | structural value equality (model diffing / `Comparison` properties)               |

## [04]-[IMPLEMENTATION_LAW]

[OPENAPI_GEN_LAW]:
- the schema is code-generated from the Dragonfly OpenAPI spec; do not hand-edit or subclass the generated types — round-trip is `FromJson`→edit-via-`Duplicate`→`ToJson`. The `"type"` JSON discriminator on each object is the case tag the `AnyOf<…>` resolver reads.
- validation is opt-in: `FromJson` does not validate; call `IsValid(throwException:true)` or fold `Validate()` before a node crosses into translation, because Honeybee/OpenStudio assume a valid model.

[ANYOF_DISCRIMINATION_LAW]:
- a `Room2D` window/shading/skylight/boundary slot is `AnyOf<…>` (a HoneybeeSchema oneOf carrier). Read its concrete case by matching the carried `"type"` (e.g. `SingleWindow` vs `RepeatingWindowRatio`); set it by assigning a concrete instance. The union is closed by the schema — exhaustively dispatch the documented cases, never assume a default.
- `Story.FloorToFloorHeight`/`FloorHeight` are `AnyOf<Autocalculate,double>`: a value or the `Autocalculate` sentinel; resolve `Autocalculate` from the room geometry, never treat it as `0`.

[HONEYBEE_STACK_LAW]:
- the detailed-physics vocabulary is HoneybeeSchema, not Dragonfly: `AnyOf<…>`, `Autocalculate`, the geometry primitives (`Point3D`/`Face3D`/`Mesh3D`/`LineSegment3D`), `Building.room3ds` (`HoneybeeSchema.Room`), the boundary conditions (`Ground`/`Outdoors`/`Surface`/`Adiabatic`/`OtherSideTemperature`), and the entire energy library (`OpaqueConstruction`/`WindowConstruction`/`EnergyMaterial…`/`VAV`/`PSZ`/`IdealAirSystemAbridged`/`ScheduleRuleset…`/`ProgramType…`) are all owned by `api-honeybee-schema`. `ModelEnergyProperties.Constructions`/`Materials`/`Hvacs`/`Schedules`/`ProgramTypes` are `List<AnyOf<…HoneybeeSchema…>>` keyed by identifier; the per-object `*PropertiesAbridged` reference those entries by string id. Translation to a Honeybee `Model` (the room-level explosion of the 2D plates) is the next leg — Dragonfly never re-declares a Honeybee construction or HVAC type.

[INTEGRATION_STACK]:
- energy-model stack (the rail): Dragonfly massing (`Model`/`Building`/`Story`/`Room2D`) → HoneybeeSchema room model (`api-honeybee-schema`) → `OpenStudio` (`api-openstudio`, `NREL.OpenStudio.macOS-arm64`) → EnergyPlus simulation. Dragonfly is the first hop; the `Multiplier`/`StoryType` expansion and the 2D-plate-to-3D-room translation feed Honeybee, which OpenStudio translates to the IDF/OSM. Read simulation results back against the Dragonfly `Identifier`s.
- georeference leg: `Room2D.FloorBoundary` is a `List<List<double>>` polygon ring; project it onto the `NetTopologySuite` planar algebra (`api-nettopologysuite`) for footprint area/overlap/adjacency and onto the canonical Bim site frame — the same `Geometry`/`Envelope` surface the geospatial codecs feed.
- BIM-source leg: the authoritative `GeometryGym` IFC model (`api-geometrygym-ifc`) is the geometry SOURCE — `IfcSpace`/`IfcBuildingStorey` footprints and storey heights map onto Dragonfly `Room2D`/`Story` at the `Exchange` boundary; `DragonflySchema.*` types stay at the energy-exchange edge and never become the internal carrier. IFC Psets (`api-xbim-properties`) supply the construction/program assignments that select the abridged Honeybee library entries.
- units leg: `Units` is the model's unit system; cross-model quantity reconciliation rides `UnitsNet` (`api-unitsnet`) so a Dragonfly meters model and an imperial source agree on one canonical unit before translation.
- identity leg: a `Model.ToJson()` string (UTF-8) feeds `System.IO.Hashing` `XxHash3`/`XxHash128` (`api-hashing`) for the energy-model snapshot content key, joining the IFC/glTF exports on one content-identity rail; the `Comparison` extension properties carry the structured model-diff alongside the hash.

[LOCAL_ADMISSION]:
- DFJSON import enters through `Model.FromJson`, is gated by `IsValid`/`Validate`, and maps the massing hierarchy + Honeybee-referenced properties onto canonical Bim/energy carriers; `AnyOf<…>` slots are resolved by `"type"` discrimination at the boundary.
- DFJSON export enters through a canonical→`Model` build (footprint polygons → `Room2D.FloorBoundary`, storey grouping → `Story` with `Multiplier`, library assembly → `ModelEnergyProperties` Honeybee entries) then `Model.ToJson()`.

[RAIL_LAW]:
- Package: `DragonflySchema`
- Owns: the Dragonfly (DFJSON) urban energy-massing schema + Newtonsoft codec — the `Model`/`Building`/`Story`/`Room2D` hierarchy, the `AnyOf<…>` window/shading/skylight/boundary parameter unions, the Radiance grid parameters, and the extension-property hub referencing the Honeybee library
- Accept: urban building-energy model interchange, massing-to-room translation input, footprint/storey geometry exchange, DataAnnotations validation
- Reject: re-implementing the Honeybee physics vocabulary (constructions/materials/HVAC/schedules — `api-honeybee-schema` owns it), running the simulation (OpenStudio/EnergyPlus), authoring the source geometry (GeometryGym IFC), `System.Text.Json` serialization (the wire is the Newtonsoft fork), and leaking `DragonflySchema.*` types past the energy-exchange boundary
