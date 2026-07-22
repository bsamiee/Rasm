# [RASM_BIM_API_HONEYBEE_SCHEMA]

`HoneybeeSchema` binds the Ladybug Tools Honeybee energy/daylight model as the HBJSON object graph: the geometry envelope (`Model` -> `Room` -> `Face` -> `Aperture`/`Door`/`Shade`), the energy/radiance/doe2 property stores, and the validation surface, every type deriving from one `OpenAPIGenBaseModel` base with `ToJson`/`FromJson` round-trip and DataAnnotations validation. This generated DTO-and-serialization binding is the HBJSON leg of the Bim energy-model exchange owner, the schema a Rhino/Pollination/Grasshopper energy model round-trips in.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HoneybeeSchema`
- package: `HoneybeeSchema` (MIT, Ladybug Tools)
- assembly: `HoneybeeSchema`
- namespace: `HoneybeeSchema` (the model graph, the enum vocabulary, `OpenAPIGenBaseModel`/`AnyOf`/`Extension`, the concrete construction/material/schedule/HVAC/program classes, the `IBoundarycondition` union, and the `IAltnumber` `Autocalculate`/`Autosize` sentinels)
- namespace: `HoneybeeSchema.Energy` / `HoneybeeSchema.Radiance` (the discriminant base interfaces — `IConstruction`/`IThermalConstruction`/`IWindowConstruction`, `IMaterial`, `ISchedule`, `IProgramtype`, `IHvac`/`IIdealair`, `IConstructionset`/`IGlobalConstructionset`, `ILoad`, `IModifier`/`IBuildingModifierSet` — the polymorphic keys the library `List<>`s and `Model.Add*` dispatch on)
- namespace: `HoneybeeSchema.Helper` (`EnergyLibrary`, `Paths`, `PythonCommand`, `SettingConfig` — the standards-library and Python-CLI bridge)
- asset: netstandard2.0 IL-only AnyCPU managed assembly, no native binaries; the net10.0 consumer binds the `lib/netstandard2.0` asset, and `Helper.PythonCommand` shells the external Ladybug Tools Python CLI
- serialization: `LBT.Newtonsoft.Json`, the Ladybug-Tools Newtonsoft fork; HBJSON emits through the model's own `ToJson`/`FromJson`, never `System.Text.Json` or stock `Newtonsoft.Json`
- dependency: `System.ComponentModel.Annotations` (the DataAnnotations `Validator` behind `Validate`/`IsValid`), `System.ValueTuple`
- consumer: `libs/csharp/Rasm.Bim`
- rail: energy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serialization base and polymorphic union
- note: the identified bases inline the round-trip and validate members; the `IBoundarycondition` closed union dispatches on `Type`, and the `Autocalculate`/`Autosize` sentinels resolve from geometry/standards at the boundary.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `OpenAPIGenBaseModel`  | class         | abstract root; `Type` is the discriminator `FromJson` validates against the name |
|  [02]   | `IHoneybeeObject`      | interface     | minimal contract — `ToJson`/`ToString(detailed)`/`Duplicate()`                   |
|  [03]   | `IDdBaseModel`         | interface     | adds the identified-object `Identifier`/`DisplayName`/`UserData`                 |
|  [04]   | `IDdEnergyBaseModel`   | interface     | energy identified base — constructions/materials/schedules derive from it        |
|  [05]   | `IDdRadianceBaseModel` | interface     | radiance identified base — modifiers derive from it                              |
|  [06]   | `AnyOf`                | class         | oneOf carrier — `Obj` boxed value, `ToJson`, implicit `string` cast, equality    |
|  [07]   | `ValidationReport`     | class         | structured `Validate` error tree (`ValidationError`/`ValidationParent`)          |
|  [08]   | `Autocalculate`        | class         | `AnyOf<Autocalculate,double>` sentinel — engine-computes marker, never `0`       |
|  [09]   | `Autosize`             | class         | `AnyOf<Autosize,double>` sizing sentinel — auto-sizing marker, never `0`         |
|  [10]   | `IBoundarycondition`   | interface     | closed `Face.BoundaryCondition` union of the 5 cases below, dispatched on `Type` |
|  [11]   | `Ground`               | class         | soil-contact boundary                                                            |
|  [12]   | `Outdoors`             | class         | `SunExposure`/`WindExposure`/`ViewFactor` (`AnyOf<Autocalculate,double>`)        |
|  [13]   | `Surface`              | class         | adjacency by `BoundaryConditionObjects` ids                                      |
|  [14]   | `Adiabatic`            | class         | no-flux boundary                                                                 |
|  [15]   | `OtherSideTemperature` | class         | `HeatTransferCoefficient` + `Temperature` (`AnyOf<Autocalculate,double>`)        |

[PUBLIC_TYPE_SCOPE]: geometry envelope (honeybee-core), generated model and geometry value-type classes
- note: the `Model` root carries `Version`, `Units` (default `Meters`), `Tolerance`/`AngleTolerance`, and the `Orphaned{Faces,Shades,Apertures,Doors}` lists beside `Rooms`; the geometry value types are the ladybug-geometry primitives Dragonfly's massing composes.

| [INDEX] | [SYMBOL]              | [CAPABILITY]                                                                                             |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Model`               | the HBJSON root; `Properties`/`Rooms`/`Orphaned*`/`ShadeMeshes`; `new static FromJson`, `DuplicateModel` |
|  [02]   | `Room`                | a closed volume of `Face`s; the thermal-zone-bearing space carrying `RoomEnergyPropertiesAbridged`       |
|  [03]   | `Face`                | a planar boundary; a `FaceType` discriminant; holds `Apertures`/`Doors` and a `BoundaryCondition`        |
|  [04]   | `Aperture` / `Door`   | glazed/opaque sub-faces with their own energy/radiance properties                                        |
|  [05]   | `Shade` / `ShadeMesh` | context/attached shading as a polyface (`Shade`) or a triangulated mesh (`ShadeMesh`)                    |
|  [06]   | `Face3D`              | a boundary-loop face geometry value type                                                                 |
|  [07]   | `Mesh3D`              | a triangulated mesh geometry value type                                                                  |
|  [08]   | `Point3D` / `Plane`   | anchor-point and plane geometry value types                                                              |
|  [09]   | `LineSegment3D`       | an edge-segment geometry value type                                                                      |
|  [10]   | `SensorGrid`          | a radiance sensor grid (positions + directions) for daylight simulation                                  |

[PUBLIC_TYPE_SCOPE]: domain properties (honeybee-energy / -radiance / -doe2)

Every geometry object carries a `Properties` member splitting into per-extension objects; `*Abridged` variants hold identifier references into the model-level lists, the full variants inline. `ModelEnergyProperties` is the canonical store — `Constructions`/`Materials`/`Schedules`/`ScheduleTypeLimits`/`ConstructionSets`/`ProgramTypes`/`Hvacs`/`Shws` — that abridged children point into, and each family's full and `*Abridged` forms implement the same `HoneybeeSchema.Energy.I*` discriminant.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :---------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `ModelProperties`       | class         | property root; the `Energy`/`Radiance`/`Doe2` stores `Model.Add*` appends into      |
|  [02]   | `ModelEnergyProperties` | class         | the energy store the abridged children reference by id (lists in the lead)          |
|  [03]   | `IConstruction`         | interface     | `Opaque`/`Window`(+`Dynamic`/`Shade`)/`AirBoundary`/`ShadeConstruction`             |
|  [04]   | `IConstructionset`      | interface     | `ConstructionSet`(+`Abridged`), the wall/floor/roof/aperture/door sets, global      |
|  [05]   | `IMaterial` (opaque)    | interface     | `EnergyMaterial`/`EnergyMaterialNoMass`/`EnergyMaterialVegetation`                  |
|  [06]   | `EnergyWindowMaterial*` | class         | `Glazing`/`Gas`/`GasCustom`/`GasMixture`/`Blind`/`Shade`/`SimpleGlazSys`            |
|  [07]   | `ISchedule`             | interface     | `ScheduleRuleset`/`ScheduleFixedInterval` (+`Abridged`), `ScheduleTypeLimit`        |
|  [08]   | `IProgramtype`          | interface     | `ProgramType`(+`Abridged`) bundling the per-program load assignment                 |
|  [09]   | loads (occupancy)       | class         | `PeopleAbridged`/`LightingAbridged`/`ElectricEquipmentAbridged`/`GasEquipment…`     |
|  [10]   | loads (flow)            | class         | `InfiltrationAbridged`/`VentilationAbridged`/`SetpointAbridged`                     |
|  [11]   | `IHvac` base            | interface     | `IdealAirSystemAbridged`, `AllAirBase`/`DOASBase`/`HeatCoolBase`/`Equipment…`       |
|  [12]   | `IHvac` all-air         | class         | `VAV`/`PVAV`/`PSZ`/`PTAC` templates keyed by the `*EquipmentType` enums             |
|  [13]   | `IHvac` DOAS-capable    | class         | `FCU`/`VRF`/`WSHP`/`Radiant`, each with a `withDOAS` variant                        |
|  [14]   | `IHvac` heat-cool       | class         | `Baseboard`/`ForcedAirFurnace`/`GasUnitHeater`/`WindowAC`/`Residential`, `Detailed` |
|  [15]   | `SimulationParameter`   | class         | run-control: `RunPeriod`/`SimulationOutput`/`SizingParameter`/`ShadowCalculation`   |
|  [16]   | radiance/doe2 stores    | class         | `ModelRadianceProperties` (`Modifiers`/`ModifierSets`/`SensorGrids`/`Views`), doe2  |

[PUBLIC_TYPE_SCOPE]: enum vocabulary, string-backed
- note: every enum is `[DataContract]` + `[JsonConverter(typeof(StringEnumConverter))]` with `[EnumMember(Value = "...")]` per member, serializing as its string name; fold onto a `[SmartEnum]`/match at the Exchange boundary.

| [INDEX] | [SYMBOL]         | [CAPABILITY]                                                                                                 |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `FaceType`       | `Wall`/`Floor`/`RoofCeiling`/`AirBoundary` — the `Face` discriminant mapped to an `IfcClass`/surface kind    |
|  [02]   | `Units`          | `Meters`/`Millimeters`/`Feet`/`Inches`/`Centimeters` — the `Model.Units` length unit                         |
|  [03]   | `Roughness`      | the `EnergyMaterial` surface-roughness band (`VeryRough`…`VerySmooth`)                                       |
|  [04]   | `*EquipmentType` | one `<Template>EquipmentType` per HVAC template and the `*withDOASEquipmentType` mirrors                     |
|  [05]   | standards enums  | `Vintages`/`ClimateZones`/`EfficiencyStandards`/`BuildingTypes`/`BuildingType` — `Helper.EnergyLibrary` keys |
|  [06]   | schedule enums   | `ScheduleNumericType`/`ScheduleUnitType`/`DaysOfWeek` — the temporal vocabulary                              |
|  [07]   | control enums    | `ControlType`/`VentilationControlType`/`EconomizerType`/`AllAirEconomizerType`                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: HBJSON round-trip
- note: `FromJson` is static; deep `Duplicate` runs a `ToJson`/`FromJson` round-trip.

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `Model.FromJson(string) -> Model`                   | static   | parse HBJSON into the typed graph                 |
|  [02]   | `Model.ToJson(bool) -> string`                      | instance | serialize to HBJSON via `LBT.Newtonsoft.Json`     |
|  [03]   | `OpenAPIGenBaseModel.FromJson(string)`              | static   | polymorphic parse; `Type` must match the type     |
|  [04]   | `OpenAPIGenBaseModel.Duplicate()`                   | instance | deep clone of any object (`Model.DuplicateModel`) |
|  [05]   | `model.IsValid(bool) -> bool`                       | instance | the boolean DataAnnotations validity gate         |
|  [06]   | `model.Validate() -> IEnumerable<ValidationResult>` | instance | the per-property DataAnnotations results          |

[ENTRYPOINT_SCOPE]: model assembly and standards library
- note: the `Model.Add*` energy mutators are `AddConstructions`/`AddMaterials`/`AddSchedules`/`AddScheduleTypeLimits`/`AddConstructionSets`/`AddProgramTypes`/`AddHVACs`, the radiance mutators `AddModifiers`/`AddModifierSets`; `Helper.EnergyLibrary` exposes `StandardEnergyLibrary`/`DefaultModelEnergyProperties`/`UserEnergyLibrary` and the `Get<X>ByIdentifier` resolvers.

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :---------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `new Model(string, ModelProperties, …)`         | ctor     | build from rooms and property store; non-null     |
|  [02]   | `Model.Add*(IEnumerable<I…>)`                   | instance | append into `Properties.Energy.*` (note mutators) |
|  [03]   | `Model.Add*(IEnumerable<I…>)`                   | instance | append into `Properties.Radiance.*`               |
|  [04]   | `Model.MergeModelProperties(Model)`             | instance | fold another model's property store into this one |
|  [05]   | `Helper.EnergyLibrary -> ModelEnergyProperties` | static   | standards library / defaults / user library       |

[ENTRYPOINT_SCOPE]: `Extension` static helpers

`Extension` is the cross-cutting static helper beneath the `Model.Add*` instance mutators — the generic codec entry, the interface-typed deep copy, the store-keyed library mutators `Model.Add*` delegates to, and the abridged-reference resolvers. `Extension.Duplicate` clones `ILoad`/`IMaterial`/`IConstruction`/`IConstructionset`; `Extension.GetAll*` reads a `ConstructionSetAbridged`/`IConstruction`/`ProgramTypeAbridged`/`ModifierSetAbridged`.

| [INDEX] | [SURFACE]                                                            | [SHAPE] | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------------- | :------ | :--------------------------------------------------- |
|  [01]   | `Extension.To<T>(this string) -> T`                                  | static  | generic HBJSON parse; reflects `T.FromJson`          |
|  [02]   | `Extension.Add*(this ModelEnergyProperties, I…)`                     | static  | store-keyed library mutators (dedupe-by-id)          |
|  [03]   | `Extension.Duplicate(this I…)`                                       | static  | interface-typed deep clone keeping the concrete case |
|  [04]   | `Extension.GetAll*(this Abridged) -> List<string>`                   | static  | the library ids a `*Abridged` object references      |
|  [05]   | `Extension.GetUserData/AddUserData/ToDictionary(this IIDdBaseModel)` | static  | over the `UserData` dict                             |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- every object carries a `Type` string discriminator: `FromJson` checks it against the runtime type name and rejects a mismatch, and `AnyOf` slots resolve their concrete case through it — drive the Exchange-boundary map off `Type`, never a downcast chain.
- HBJSON I/O folds through the model's own `ToJson`/`FromJson` over `LBT.Newtonsoft.Json`; the `[JsonConverter(typeof(StringEnumConverter))]`/`[JsonProperty]` attributes reference the fork's types, so only the bundled codec emits spec-valid HBJSON and enums serialize by their `[EnumMember]` string.
- `*Abridged` objects reference the model-level `ModelEnergyProperties`/`ModelRadianceProperties` lists the full objects inline; `Extension.GetAllConstructions`/`GetAllSchedules`/`GetAbridgedConstructionMaterials`/`GetAllModifiers` enumerate which library ids an abridged object references, and a round-trip preserves the abridged form rather than re-inlining it.
- `Validate()`/`IsValid()` run DataAnnotations over the generator-stamped `[Required]`/range attributes, `ValidationReport`/`ValidationError`/`ValidationParent` the structured tree for cross-object checks — a typed result feeding the Bim audit rail, with `IsValid(throwException: true)` the gate.

[STACKING]:
- `OpenStudio`(`.api/api-openstudio.md`): the parallel EnergyPlus runtime binding; the two energy DTOs meet at the canonical Bim energy model over gbXML (`OpenStudio.GbXMLReverseTranslator`), the end-to-end `honeybee-openstudio` Python translation reaching through `Helper.PythonCommand`.
- `DragonflySchema`(`.api/api-dragonfly-schema.md`): the urban-massing schema whose `Building`/`Story`/`Room2D` generate honeybee `Room`/`Model` and reference this package's shared vocabulary — the `AnyOf` carrier, the `Autocalculate` sentinel, the `IBoundarycondition` union, the geometry primitives, and the full energy library — by identifier.
- `GeometryGym.Ifc`(`.api/api-geometrygym-ifc.md`): an `IfcSpace`/`IfcBuildingStorey` maps onto a honeybee `Room` at the `Exchange/import` boundary, `FaceType` mapping to the IFC element class through the `ElementPredicate` table.
- `System.IO.Hashing`(`libs/csharp/.api/api-hashing.md`): a `Model.ToJson()` UTF-8 string feeds `XxHash3` for the fast fingerprint and `XxHash128` for the persisted content key, the same identity rail the IFC/glTF exports ride; an HBJSON document rides a `Speckle`(`.api/api-speckle.md`) commit or lands in the `Rasm.Persistence` artifact index keyed by that hash.
- Rasm.Bim Exchange: HBJSON parses through `Model.FromJson` onto canonical Bim carriers and builds back through `Model.Add*` then `Model.ToJson`; internal code holds canonical shapes, `HoneybeeSchema.*` types never crossing the codec boundary.

[LOCAL_ADMISSION]:
- HBJSON import enters through `Model.FromJson`; export builds canonical -> `Model` through `Model.Add*` then `Model.ToJson`. Validation enters through `Validate()`/`IsValid()`, the standards baseline through `Helper.EnergyLibrary.StandardEnergyLibrary`.

[RAIL_LAW]:
- Package: `HoneybeeSchema`
- Owns: the Honeybee HBJSON energy/daylight schema — the `OpenAPIGenBaseModel`/`AnyOf` serialization base, the geometry envelope, the `IBoundarycondition` union and `IAltnumber` sentinels, the energy/radiance/doe2 property stores and full energy library Dragonfly references by id, the abridged-reference model and `Extension` resolver/codec helpers, the enum vocabulary, validation, and the standards-library helper
- Accept: HBJSON serialize/parse/validate/duplicate, energy-model assembly, standards-library lookup, the Exchange-boundary map to canonical Bim carriers
- Reject: energy simulation (`OpenStudio` and EnergyPlus own it), HBJSON->OSM translation (the external `honeybee-openstudio` Python step), `System.Text.Json`/stock-Newtonsoft serialization of these types, and leaking `HoneybeeSchema.*` types past the codec boundary
