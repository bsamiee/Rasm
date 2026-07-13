# [RASM_BIM_API_HONEYBEE_SCHEMA]

`HoneybeeSchema` is the C# binding of the Ladybug Tools Honeybee energy-model schema — the
HBJSON object graph that carries a building's energy/daylight model end to end: the geometry
envelope (`Model` -> `Room` -> `Face` -> `Aperture`/`Door`/`Shade`), the energy properties
(constructions, materials, schedules, loads, HVAC templates, simulation parameters), the
radiance properties (modifiers, sensor grids), and the validation surface. It is a generated
DTO + serialization package: 296 model classes and 57 string-backed enums over one
`OpenAPIGenBaseModel` base with `ToJson`/`FromJson` round-trip, deep `Duplicate`, and
DataAnnotations validation. It is the HBJSON leg of the Bim energy-model exchange owner —
the schema a Rhino/Pollination/Grasshopper energy model is authored and round-tripped in,
sibling to the `OpenStudio` (`api-openstudio`) OSM/EnergyPlus leg and the `DragonflySchema`
(`api-dragonfly-schema`) urban-massing leg that generates Honeybee models.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HoneybeeSchema`
- package: `HoneybeeSchema`
- license: MIT (`requireLicenseAcceptance=true`)
- assembly: `HoneybeeSchema`
- namespace: `HoneybeeSchema` (the model graph + the 57 enums + `OpenAPIGenBaseModel`/`AnyOf`/`Extension`; the concrete construction/material/schedule/HVAC/program classes, the `IBoundarycondition` boundary-condition union, and the `IAltnumber` `Autocalculate`/`Autosize` sentinels all live in this root namespace)
- namespace: `HoneybeeSchema.Energy` / `HoneybeeSchema.Radiance` (the discriminant base interfaces the concrete classes implement — `IConstruction`/`IThermalConstruction`/`IWindowConstruction`, `IMaterial`/`IOpaqueMaterial`, `ISchedule`, `IProgramtype`, `IHvac`/`IIdealair`, `IConstructionset`/`IGlobalConstructionset`, `ILoad`; `IModifier`/`IBuildingModifierSet` on the radiance side — the polymorphic keys the energy/radiance library `List<>`s and `Model.Add*` dispatch on)
- namespace: `HoneybeeSchema.Helper` (`EnergyLibrary`, `Paths`, `PythonCommand`, `SettingConfig` — the standards-library + Python-CLI bridge)
- asset: netstandard2.0 ONLY (single TFM); the net10.0 consumer binds the `lib/netstandard2.0` asset
- asset: IL-only AnyCPU managed assembly; no native binaries (the `Helper.PythonCommand` bridge shells the external Ladybug Tools Python CLI, not an in-proc native lib)
- serialization: `LBT.Newtonsoft.Json` — the Ladybug-Tools fork of Newtonsoft under the `LBT.Newtonsoft.Json` namespace; HBJSON is emitted by the model's own `ToJson`/`FromJson`, NOT by `System.Text.Json` and NOT by stock `Newtonsoft.Json`
- dependency: `System.ComponentModel.Annotations` (the DataAnnotations `Validator` the model `Validate`/`IsValid` runs), `System.ValueTuple`
- consumer: `libs/csharp/Rasm.Bim` (the HBJSON energy-model exchange owner)
- rail: energy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serialization base and polymorphic union
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy
- note: the identified bases inline the round-trip/validate members [03] carries; the `IBoundarycondition` closed union dispatches on `Type`, never a downcast, and the `Autocalculate`/`Autosize` sentinels resolve from geometry/standards at the boundary.

| [INDEX] | [SYMBOL]               | [CAPABILITY]                                                                                                    |
| :-----: | :--------------------- | :-------------------------------------------------------------------------------------------------------------- |
|  [01]   | `OpenAPIGenBaseModel`  | `abstract` model-class root; `Type` is the discriminator `FromJson` validates against the runtime type name     |
|  [02]   | `IHoneybeeObject`      | the minimal contract: `ToJson`/`ToString(detailed)`/`Duplicate() -> OpenAPIGenBaseModel`                        |
|  [03]   | `IDdBaseModel`         | `OpenAPIGenBaseModel` + the identified-object fields `Identifier`/`DisplayName`/`UserData`                      |
|  [04]   | `IDdEnergyBaseModel`   | the energy identified base — constructions/materials/schedules derive from it                                   |
|  [05]   | `IDdRadianceBaseModel` | the radiance identified base — modifiers derive from it                                                         |
|  [06]   | `AnyOf`                | `anyOf`/`oneOf` carrier: `Obj` boxed value, `ToJson`, implicit `string` cast, type-aware equality               |
|  [07]   | `ValidationReport`     | + `ValidationError`/`ValidationParent`; the structured `Validate` error tree feeding `IdsAudit`, not exceptions |
|  [08]   | `Autocalculate`        | `IAltnumber` sentinel in `AnyOf<Autocalculate,double>` slots — the engine-computes marker, never read as `0`    |
|  [09]   | `Autosize`             | `IAltnumber` sentinel in `AnyOf<Autosize,double>` sizing slots — the auto-sizing marker, never read as `0`      |
|  [10]   | `IBoundarycondition`   | the closed `Face.BoundaryCondition` `AnyOf` union of the 5 cases below, dispatched on `Type`                    |
|  [11]   | `Ground`               | soil-contact boundary                                                                                           |
|  [12]   | `Outdoors`             | `SunExposure`/`WindExposure`/`ViewFactor` (`AnyOf<Autocalculate,double>`)                                       |
|  [13]   | `Surface`              | adjacency by `BoundaryConditionObjects` ids                                                                     |
|  [14]   | `Adiabatic`            | no-flux boundary                                                                                                |
|  [15]   | `OtherSideTemperature` | `HeatTransferCoefficient` + `Temperature` (`AnyOf<Autocalculate,double>`)                                       |

[PUBLIC_TYPE_SCOPE]: geometry envelope (honeybee-core)
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy
- note: the `Model` root carries `Version`, `Units` (default `Meters`), `Tolerance`/`AngleTolerance`, and the `Orphaned{Faces,Shades,Apertures,Doors}` surface lists beside `Rooms`; the geometry value types are the ladybug-geometry primitives Dragonfly's massing geometry also composes.

| [INDEX] | [SYMBOL]              | [CAPABILITY]                                                                                             |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Model`               | the HBJSON root; `Properties`/`Rooms`/`Orphaned*`/`ShadeMeshes`; `new static FromJson`, `DuplicateModel` |
|  [02]   | `Room`                | a closed volume of `Face`s; the thermal-zone-bearing space carrying `RoomEnergyPropertiesAbridged`       |
|  [03]   | `Face`                | a planar boundary; a `FaceType` discriminant; holds `Apertures`/`Doors` + a `BoundaryCondition`          |
|  [04]   | `Aperture` / `Door`   | glazed/opaque sub-faces with their own energy/radiance properties (the window/door surfaces)             |
|  [05]   | `Shade` / `ShadeMesh` | context/attached shading as a polyface (`Shade`) or a triangulated mesh (`ShadeMesh`)                    |
|  [06]   | `Face3D`              | a boundary-loop face geometry value type                                                                 |
|  [07]   | `Mesh3D`              | a triangulated mesh geometry value type                                                                  |
|  [08]   | `Point3D` / `Plane`   | anchor-point and plane geometry value types                                                              |
|  [09]   | `LineSegment3D`       | an edge-segment geometry value type                                                                      |
|  [10]   | `SensorGrid`          | a radiance sensor grid (positions + directions) attached for daylight simulation                         |

[PUBLIC_TYPE_SCOPE]: domain properties (honeybee-energy / -radiance / -doe2)
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy

Every geometry object carries a `Properties` member splitting into per-extension property
objects. The `*Abridged` variants hold identifier REFERENCES into the model-level property
lists; the full variants inline the referenced object. `ModelEnergyProperties` holds the
`Constructions`/`Materials`/`Schedules`/`ScheduleTypeLimits`/`ConstructionSets`/`ProgramTypes`/`Hvacs`/`Shws`
lists — the canonical store the abridged children point into. Each family's full and
`*Abridged` forms implement the same `HoneybeeSchema.Energy.I*` discriminant, and this is the
shared catalog Dragonfly's `ModelEnergyProperties` references by id and never re-declares.

| [INDEX] | [SYMBOL]                | [CAPABILITY]                                                                                                  |
| :-----: | :---------------------- | :------------------------------------------------------------------------------------------------------------ |
|  [01]   | `ModelProperties`       | the property root; the `Energy`/`Radiance`/`Doe2` extension stores `Model.Add*` appends into                  |
|  [02]   | `ModelEnergyProperties` | the energy store the abridged children reference by id (lists in the lead)                                    |
|  [03]   | `IConstruction`         | `OpaqueConstruction`/`WindowConstruction`(+`Dynamic`/`Shade`)/`AirBoundaryConstruction`/`ShadeConstruction`   |
|  [04]   | `IConstructionset`      | `ConstructionSet`(+`Abridged`), `Wall`/`Floor`/`RoofCeiling`/`Aperture`/`Door` sets + `GlobalConstructionSet` |
|  [05]   | `IMaterial` opaque      | `EnergyMaterial`/`EnergyMaterialNoMass`/`EnergyMaterialVegetation`                                            |
|  [06]   | `EnergyWindowMaterial*` | glazing set: `Glazing`/`Gas`/`GasCustom`/`GasMixture`/`Blind`/`Shade`/`SimpleGlazSys`                         |
|  [07]   | `ISchedule`             | `ScheduleRuleset`(+`Abridged`)/`ScheduleFixedInterval`(+`Abridged`) + the standalone `ScheduleTypeLimit`      |
|  [08]   | `IProgramtype`          | `ProgramType`(+`Abridged`) bundling the per-program load assignment                                           |
|  [09]   | loads (occupancy)       | `PeopleAbridged`/`LightingAbridged`/`ElectricEquipmentAbridged`/`GasEquipmentAbridged`                        |
|  [10]   | loads (flow)            | `InfiltrationAbridged`/`VentilationAbridged`/`SetpointAbridged`                                               |
|  [11]   | `IHvac` base            | `IdealAirSystemAbridged` (`IIdealair` baseline) + `AllAirBase`/`DOASBase`/`HeatCoolBase`/`EquipmentBase`      |
|  [12]   | `IHvac` all-air         | `VAV`/`PVAV`/`PSZ`/`PTAC` templates keyed by the `*EquipmentType` enums                                       |
|  [13]   | `IHvac` DOAS-capable    | `FCU`/`VRF`/`WSHP`/`Radiant`, each + a `withDOAS` variant (e.g. `FCUwithDOASAbridged`)                        |
|  [14]   | `IHvac` heat-cool       | `Baseboard`/`ForcedAirFurnace`/`GasUnitHeater`/`WindowAC`/`Residential` + freeform `DetailedHVAC`             |
|  [15]   | `SimulationParameter`   | run-control: `RunPeriod`/`SimulationOutput`/`SizingParameter`/`ShadowCalculation`/`Terrain` + sizing context  |
|  [16]   | radiance/doe2 stores    | `ModelRadianceProperties` (`Modifiers`/`ModifierSets`/`SensorGrids`/`Views`) and `ModelDoe2Properties`        |

[PUBLIC_TYPE_SCOPE]: enum vocabulary (string-backed)
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy

All 57 enums are `[DataContract]` + `[JsonConverter(typeof(StringEnumConverter))]` with
`[EnumMember(Value = "...")]` per member — they serialize as their string name, not an
ordinal. Fold them onto a `[SmartEnum]`/match at the Exchange boundary.

| [INDEX] | [SYMBOL]         | [CAPABILITY]                                                                                                 |
| :-----: | :--------------- | :----------------------------------------------------------------------------------------------------------- |
|  [01]   | `FaceType`       | `Wall`/`Floor`/`RoofCeiling`/`AirBoundary` — the `Face` discriminant mapped to an `IfcClass`/surface kind    |
|  [02]   | `Units`          | `Meters`/`Millimeters`/`Feet`/`Inches`/`Centimeters` — the `Model.Units` length unit                         |
|  [03]   | `Roughness`      | the `EnergyMaterial` surface-roughness band (`VeryRough`…`VerySmooth`)                                       |
|  [04]   | `*EquipmentType` | one `<Template>EquipmentType` per HVAC template + the `*withDOASEquipmentType` mirrors                       |
|  [05]   | standards enums  | `Vintages`/`ClimateZones`/`EfficiencyStandards`/`BuildingTypes`/`BuildingType` — `Helper.EnergyLibrary` keys |
|  [06]   | schedule enums   | `ScheduleNumericType`/`ScheduleUnitType`/`DaysOfWeek` — the temporal vocabulary                              |
|  [07]   | control enums    | `ControlType`/`VentilationControlType`/`EconomizerType`/`AllAirEconomizerType`                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: HBJSON round-trip
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy
- note: `FromJson` is static; deep `Duplicate` runs a `ToJson`/`FromJson` round-trip under the hood.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                              | [CAPABILITY]                                          |
| :-----: | :------------------------------ | :---------------------------------------- | :---------------------------------------------------- |
|  [01]   | `Model.FromJson`                | `(string json)` static -> `Model`         | parse HBJSON into the typed graph                     |
|  [02]   | `Model.ToJson`                  | `(bool indented = false)` -> `string`     | serialize to HBJSON via `LBT.Newtonsoft.Json`         |
|  [03]   | `OpenAPIGenBaseModel.FromJson`  | `(string json)` static -> base            | polymorphic parse; `Type` must match the runtime type |
|  [04]   | `OpenAPIGenBaseModel.Duplicate` | `()` -> base (`Model.DuplicateModel()`)   | deep clone of any honeybee object                     |
|  [05]   | `model.IsValid`                 | `(bool throwException = false)` -> `bool` | the boolean DataAnnotations validity gate             |
|  [06]   | `model.Validate`                | `()` -> `IEnumerable<ValidationResult>`   | the per-property DataAnnotations results              |

[ENTRYPOINT_SCOPE]: model assembly and standards library
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`, `HoneybeeSchema.Helper`
- rail: energy
- note: the `Model.Add*` energy mutators are `AddConstructions`/`AddMaterials`/`AddSchedules`/`AddScheduleTypeLimits`/`AddConstructionSets`/`AddProgramTypes`/`AddHVACs`; the radiance mutators are `AddModifiers`/`AddModifierSets`. `Helper.EnergyLibrary` exposes the `StandardEnergyLibrary`/`DefaultModelEnergyProperties`/`UserEnergyLibrary` statics plus the `Get<X>ByIdentifier` resolvers (construction-set/program-type/HVAC/opaque+window construction/material).

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]                                 | [CAPABILITY]                                              |
| :-----: | :--------------------------- | :------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `new Model`                  | `(string id, ModelProperties properties, …)` | build from rooms + property store; `properties` non-null  |
|  [02]   | `Model.Add*` (energy)        | `(IEnumerable<I…>)`                          | append into `Properties.Energy.*` (mutators in the note)  |
|  [03]   | `Model.Add*` (radiance)      | `(IEnumerable<I…>)`                          | append into `Properties.Radiance.*`                       |
|  [04]   | `Model.MergeModelProperties` | `(Model other)` -> `void`                    | fold another model's property store into this one         |
|  [05]   | `Helper.EnergyLibrary`       | `-> ModelEnergyProperties`                   | standards library / defaults / user library (in the note) |

[ENTRYPOINT_SCOPE]: `Extension` static helpers (generic codec, library assembly, abridged resolution)
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy
- note: `Extension.Duplicate` clones `ILoad`/`IMaterial`/`IConstruction`/`IConstructionset`; `Extension.GetAll*` reads a `ConstructionSetAbridged`/`IConstruction`/`ProgramTypeAbridged`/`ModifierSetAbridged`.

`Extension` is the cross-cutting static helper beneath the `Model.Add*` instance methods — the generic JSON entry, the interface-typed deep copy, the `ModelEnergyProperties`/`ModelRadianceProperties`-keyed library mutators the `Model.Add*` methods delegate to, and the abridged-reference resolvers the `[ABRIDGED_REFERENCE_MODEL]` law reads. Dragonfly composes the same `Extension.To<T>` codec entry rather than re-deriving a parser.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                          | [CAPABILITY]                                                           |
| :-----: | :-------------------- | :------------------------------------ | :--------------------------------------------------------------------- |
|  [01]   | `Extension.To<T>`     | `(this string json)` -> `T`           | generic HBJSON parse; reflects `T.FromJson`, throws if absent          |
|  [02]   | `Extension.Add*`      | `(this ModelEnergyProperties, I…)`    | store-keyed library mutators (dedupe-by-id); energy + radiance         |
|  [03]   | `Extension.Duplicate` | `(this I…)` -> same                   | interface-typed deep clone, the concrete case kept (types in the note) |
|  [04]   | `Extension.GetAll*`   | `(this <abridged>)` -> `List<string>` | the library ids a `*Abridged` object references                        |
|  [05]   | `Extension` UserData  | `(this IIDdBaseModel)`                | `GetUserData`/`AddUserData`/`ToDictionary` over the `UserData` dict    |

## [04]-[IMPLEMENTATION_LAW]

[GENERATED_DTO]:
- this is a generated schema binding (296 classes / 57 enums) over one `OpenAPIGenBaseModel` base — it is DATA + serialization + validation, NOT a simulation engine. It authors, validates, serializes, and round-trips the energy model; running EnergyPlus is the `OpenStudio` leg (`api-openstudio`), and the HBJSON->OSM translation is the external `honeybee-openstudio` Python step (reached, if needed, through `Helper.PythonCommand`).
- the `Type` string on every object is the polymorphic discriminator: `FromJson` checks it against the runtime type name and rejects a mismatch, and `AnyOf` slots resolve their concrete type through it. Drive the Exchange-boundary mapping off `Type`, not a downcast chain.

[SERIALIZATION_BOUNDARY]:
- HBJSON I/O is the model's own `ToJson`/`FromJson` over `LBT.Newtonsoft.Json` (the LBT Newtonsoft fork). Do NOT register these types in a `System.Text.Json` pipeline or feed them to stock `Newtonsoft.Json` — the `[JsonConverter(typeof(StringEnumConverter))]` and `[JsonProperty]` attributes reference the `LBT.Newtonsoft.Json` types, so only the bundled `ToJson`/`FromJson` produce spec-valid HBJSON. The codec boundary stays inside this package; internal Rasm code holds canonical Bim shapes, never `HoneybeeSchema.*` types.
- enums serialize by their `[EnumMember]` string, so a forward-compatible reader tolerates an unknown member only if the design folds the enum onto a `[SmartEnum]` with a fallback at the boundary.

[ABRIDGED_REFERENCE_MODEL]:
- the `*Abridged` objects carry identifier references into the model-level `ModelEnergyProperties`/`ModelRadianceProperties` lists; the full objects inline. Resolve an abridged room/face property by looking its construction/schedule/program id up in `Properties.Energy.*` — the model-level lists are the canonical store, populated through `Model.AddConstructions`/`AddSchedules`/etc. The `Extension.GetAllConstructions`/`GetAllSchedules`/`GetAbridgedConstructionMaterials`/`GetAllModifiers` helpers enumerate exactly which library identifiers an abridged object references — drive the resolve off them, not a hand-rolled scan. A round-trip preserves the abridged form; do not expand-then-reinline blindly or the document doubles in size.

[VALIDATION]:
- `Validate()`/`IsValid()` run DataAnnotations (`System.ComponentModel.Annotations` `Validator.TryValidateObject`) over the `[Required]`/range attributes the generator stamped; `ValidationReport`/`ValidationError`/`ValidationParent` is the structured tree for the deeper cross-object checks. Treat the report as a typed result feeding the Bim audit rail, not an exception to swallow — `IsValid(throwException: true)` is the gate, the report is the evidence.

[STACK_INTEGRATION]:
- OpenStudio seam: `HoneybeeSchema` is the authored energy model; `OpenStudio` (`api-openstudio`) is the EnergyPlus runtime model. They are parallel energy DTOs/bindings meeting at the canonical Bim energy model — the language-neutral bridge both read is gbXML (`OpenStudio.GbXMLReverseTranslator`) and, end-to-end, the `honeybee-openstudio` Python translation reached via `Helper.PythonCommand`.
- Dragonfly seam: `DragonflySchema` (`api-dragonfly-schema`, sibling) is the urban-massing schema whose `Building`/`Story`/`Room2D` objects generate honeybee `Room`/`Model` objects — Dragonfly is the district level, Honeybee the building level. Dragonfly owns only the massing wire and footprint geometry; it COMPOSES this package's shared vocabulary rather than re-declaring it — the `AnyOf` carrier, the `Autocalculate` sentinel, the `IBoundarycondition` union (`Ground`/`Outdoors`/`Surface`/`Adiabatic`/`OtherSideTemperature`), the geometry primitives (`Point3D`/`Face3D`/`Mesh3D`/`LineSegment3D`), the `List<HoneybeeSchema.Room>` 3D rooms, and the entire energy library (`OpaqueConstruction`/`WindowConstruction`/`EnergyMaterial…`/the `IHvac` templates incl. `IdealAirSystemAbridged`/`ScheduleRuleset…`/`ProgramType…`) are all owned here and referenced from Dragonfly's `ModelEnergyProperties` `List<AnyOf<…>>` by identifier. The two share this base-model/serialization machinery; the energy-model design page composes these types from this Honeybee owner, never re-deriving them on the Dragonfly leg.
- IFC seam: an `IfcSpace`/`IfcBuildingStorey` from the GeometryGym graph (`api-geometrygym-ifc`) maps onto a honeybee `Room` at the `Exchange/import` boundary, and `FaceType` maps to/from the IFC element class via the `ElementPredicate` table — the energy model is derived from the BIM model.
- identity seam: a `Model.ToJson()` string (UTF-8 bytes) feeds `System.IO.Hashing` — `XxHash3` for the fast in-process fingerprint, `XxHash128` for the collision-resistant persisted content key (`api-hashing`) — the same content-identity rail (the `Rasm.Persistence` artifact index) as the IFC/glTF exports.
- transport seam: an HBJSON document can ride a `Speckle` commit (`api-speckle`) for collaboration, or land in the `Rasm.Persistence` artifact index keyed by the content hash.

[LOCAL_ADMISSION]:
- HBJSON import enters through `Model.FromJson`, then maps `Room`/`Face`/property objects onto canonical Bim carriers; HBJSON export enters through a canonical->`Model` build (using `Model.Add*` to populate the property store) then `Model.ToJson`.
- validation enters through `Validate()`/`IsValid()`; the standards baseline enters through `Helper.EnergyLibrary.StandardEnergyLibrary`. The rejected forms are an STJ/stock-Newtonsoft pipeline for HBJSON and leaking `HoneybeeSchema.*` types past the codec boundary.

[RAIL_LAW]:
- Package: `HoneybeeSchema`
- Owns: the Honeybee HBJSON energy/daylight model schema — the `OpenAPIGenBaseModel`/`AnyOf` serialization base, the geometry envelope (`Model`/`Room`/`Face`/`Aperture`/`Door`/`Shade`), the `IBoundarycondition` union and the `IAltnumber` (`Autocalculate`/`Autosize`) sentinels, the energy/radiance/doe2 property stores and the full energy library (the `IConstruction`/`IMaterial`/`ISchedule`/`IProgramtype`/`IHvac` families Dragonfly references by id), the abridged-reference model and the `Extension` resolver/codec helpers, the 57-enum vocabulary, validation, and the standards-library helper
- Accept: HBJSON serialize/parse/validate/duplicate, energy-model assembly, standards-library lookup, the Exchange-boundary map to canonical Bim carriers
- Reject: energy simulation (`OpenStudio` + EnergyPlus own it), HBJSON->OSM translation (the external `honeybee-openstudio` Python step), STJ/stock-Newtonsoft serialization of these types (only the bundled `ToJson`/`FromJson` are spec-valid), and leaking `HoneybeeSchema.*` types past the codec boundary
