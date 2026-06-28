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
- version: `2.102.0`
- license: MIT (`requireLicenseAcceptance=true`)
- assembly: `HoneybeeSchema`
- namespace: `HoneybeeSchema` (the model graph + the 57 enums + `OpenAPIGenBaseModel`/`AnyOf`/`Extension`; the concrete construction/material/schedule/HVAC/program classes, the `IBoundarycondition` boundary-condition union, and the `IAltnumber` `Autocalculate`/`Autosize` sentinels all live in this root namespace)
- namespace: `HoneybeeSchema.Energy` / `HoneybeeSchema.Radiance` (the discriminant base interfaces the concrete classes implement — `IConstruction`/`IThermalConstruction`/`IWindowConstruction`, `IMaterial`/`IOpaqueMaterial`, `ISchedule`, `IProgramtype`, `IHvac`/`IIdealair`, `IConstructionset`/`IGlobalConstructionset`, `ILoad`; `IModifier`/`IBuildingModifierSet` on the radiance side — the polymorphic keys the energy/radiance library `List<>`s and `Model.Add*` dispatch on)
- namespace: `HoneybeeSchema.Helper` (`EnergyLibrary`, `Paths`, `PythonCommand`, `SettingConfig` — the standards-library + Python-CLI bridge)
- asset: netstandard2.0 ONLY (single TFM); the net10.0 consumer binds the `lib/netstandard2.0` asset
- asset: IL-only AnyCPU managed assembly; no native binaries (the `Helper.PythonCommand` bridge shells the external Ladybug Tools Python CLI, not an in-proc native lib)
- serialization: `LBT.Newtonsoft.Json` `13.0.3.2` — the Ladybug-Tools fork of Newtonsoft under the `LBT.Newtonsoft.Json` namespace; HBJSON is emitted by the model's own `ToJson`/`FromJson`, NOT by `System.Text.Json` and NOT by stock `Newtonsoft.Json`
- dependency: `System.ComponentModel.Annotations` `5.0.0` (the DataAnnotations `Validator` the model `Validate()`/`IsValid()` runs), `System.ValueTuple` `4.5.0`
- consumer: `libs/csharp/Rasm.Bim` (the HBJSON energy-model exchange owner)
- rail: energy

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serialization base and polymorphic union
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy

| [INDEX] | [SYMBOL]               | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :--------------------- | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `OpenAPIGenBaseModel`  | energy  | `abstract` root of every model class (`: IHoneybeeObject, IEquatable<OpenAPIGenBaseModel>`); `Type` (the discriminator string, e.g. `"Model"`/`"Room"`, defaults `"InvalidType"`), `ToJson(bool indented = false)`, static `FromJson(string)`, `Duplicate()`/`DuplicateOpenAPIGenBaseModel()`, `IsValid(bool throwException = false)`, `Validate()` -> `IEnumerable<ValidationResult>`, value `Equals`/`GetHashCode`/`==`/`!=`, `ToString(bool detailed)` |
|  [02]   | `IHoneybeeObject`      | energy  | the minimal contract every honeybee object honors: `ToJson(bool indented = false)`, `ToString(bool detailed)`, `Duplicate()` -> `OpenAPIGenBaseModel` |
|  [03]   | `IDdBaseModel`         | energy  | `OpenAPIGenBaseModel` + the identified-object fields (`Identifier`, `DisplayName`, `UserData`); the base for every named model object |
|  [04]   | `IDdEnergyBaseModel` / `IDdRadianceBaseModel` | energy | the energy-domain and radiance-domain identified bases — constructions/materials/schedules derive from the energy base, modifiers from the radiance base |
|  [05]   | `AnyOf`                | energy  | the schema `anyOf`/`oneOf` carrier: `Obj` (the boxed underlying value), `ToJson()`, an implicit `string` conversion, and type-aware equality — the polymorphic slot where a field admits several model types (e.g. an aperture's construction as inline-or-reference) |
|  [06]   | `ValidationReport` / `ValidationError` / `ValidationParent` | energy | the structured validation result graph (`Validate`/`ValidateModel` output) — the parent/child error tree feeding `IdsAudit`-style reconciliation rather than an exception |
|  [07]   | `Autocalculate` / `Autosize` (`: OpenAPIGenBaseModel, IAltnumber`) | energy | the alt-number sentinel union members carried in `AnyOf<Autocalculate, double>` / `AnyOf<Autosize, double>` slots (`Outdoors.ViewFactor`, `OtherSideTemperature.Temperature`, sizing fields, and Dragonfly `Story.FloorToFloorHeight`/`FloorHeight`) — the "let the engine compute this" marker; resolve it from geometry/standards at the boundary, never read the sentinel as `0` |
|  [08]   | `IBoundarycondition` union: `Ground` / `Outdoors` / `Surface` / `Adiabatic` / `OtherSideTemperature` | energy | the closed `Face.BoundaryCondition` `AnyOf` cases — `Ground` (soil contact), `Outdoors` (`SunExposure`/`WindExposure`/`ViewFactor` `AnyOf<Autocalculate,double>`), `Surface` (adjacency by `BoundaryConditionObjects` ids), `Adiabatic` (no-flux), `OtherSideTemperature` (`HeatTransferCoefficient` + `Temperature` `AnyOf<Autocalculate,double>`); the surface-to-environment thermal contract Dragonfly's `Room2D.BoundaryConditions` composes — dispatch the closed set on `Type`, never downcast |

[PUBLIC_TYPE_SCOPE]: geometry envelope (honeybee-core)
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy

| [INDEX] | [SYMBOL]               | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :--------------------- | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `Model`                | energy  | the HBJSON root (`: IDdBaseModel, IModel`); `Properties` (`ModelProperties`), `Version` (the baked schema version), `Rooms` (`List<Room>`), `OrphanedFaces`/`OrphanedShades`/`OrphanedApertures`/`OrphanedDoors`, `ShadeMeshes`, `Units` (`Units` enum, default `Meters`), `Tolerance` (0.01), `AngleTolerance` (1.0). `new static FromJson(string)` -> `Model`, `DuplicateModel()` |
|  [02]   | `Room`                 | energy  | a closed volume of `Face`s; the thermal-zone-bearing space carrying `RoomEnergyPropertiesAbridged` |
|  [03]   | `Face`                 | energy  | a planar boundary (`FaceType` = `Wall`/`Floor`/`RoofCeiling`/`AirBoundary`); holds `Apertures`/`Doors` and a `BoundaryCondition` |
|  [04]   | `Aperture` / `Door`    | energy  | glazed/opaque sub-faces with their own energy/radiance properties (the window/door surfaces) |
|  [05]   | `Shade` / `ShadeMesh`  | energy  | context/attached shading as a polyface (`Shade`) or a triangulated mesh (`ShadeMesh`) |
|  [06]   | `Face3D` / `Mesh3D` / `Point3D` / `Plane` / `LineSegment3D` | energy | the ladybug-geometry value types every envelope surface is built from (boundary loops, meshes, anchor points/planes, edge segments) — also the primitives Dragonfly's massing geometry composes |
|  [07]   | `SensorGrid`           | energy  | a radiance sensor grid (positions + directions) attached to the model for daylight simulation |

[PUBLIC_TYPE_SCOPE]: domain properties (honeybee-energy / -radiance / -doe2)
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy

Every geometry object carries a `Properties` member splitting into per-extension property
objects. The `*Abridged` variants hold identifier REFERENCES into the model-level property
lists; the full variants inline the referenced object. The model-level lists are the
canonical store; abridged children point into them.

| [INDEX] | [SYMBOL]                  | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :------------------------ | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `ModelProperties`         | energy  | the model-level property root (`: OpenAPIGenBaseModel, IModel`); `Energy` (`ModelEnergyProperties`), `Radiance` (`ModelRadianceProperties`), `Doe2` (`ModelDoe2Properties`) — the three extension stores `Model.Add*` methods append into |
|  [02]   | `ModelEnergyProperties`   | energy  | the energy store: the model-wide `Constructions`, `Materials`, `Schedules`, `ScheduleTypeLimits`, `ConstructionSets`, `ProgramTypes`, `Hvacs`, `Shws` lists the abridged room/face properties reference by id |
|  [03]   | energy library — construction / material / schedule / program families | energy | the `IConstruction` set: `OpaqueConstruction`(+`Abridged`), `WindowConstruction`(+`Abridged`/`Dynamic`/`Shade` + their `Abridged`), `AirBoundaryConstruction`(+`Abridged`), `ShadeConstruction`; the `IConstructionset` set: `ConstructionSet`(+`Abridged`) with `Wall`/`Floor`/`RoofCeiling`/`Aperture`/`Door` specializations + the baked `GlobalConstructionSet` default; the `IMaterial` set: `EnergyMaterial`/`EnergyMaterialNoMass`/`EnergyMaterialVegetation` + the `EnergyWindowMaterial*` glazing set (`EnergyWindowMaterialGlazing`/`…Gas`/`…GasCustom`/`…GasMixture`/`…Blind`/`…Shade`/`…SimpleGlazSys`, every member carrying the full `EnergyWindowMaterial` prefix); the `ISchedule` set: `ScheduleRuleset`(+`Abridged`)/`ScheduleFixedInterval`(+`Abridged`) + the standalone `ScheduleTypeLimit`; the `IProgramtype` set: `ProgramType`(+`Abridged`) bundling the per-program load assignment. The full form inlines, the `*Abridged` form references by identifier, and both implement the same `HoneybeeSchema.Energy.I*` discriminant — this is the shared catalog Dragonfly's `ModelEnergyProperties` references by id and never re-declares |
|  [04]   | load families             | energy  | `PeopleAbridged`, `LightingAbridged`, `ElectricEquipmentAbridged`, `GasEquipmentAbridged`, `InfiltrationAbridged`, `VentilationAbridged`, `SetpointAbridged` — the per-program internal loads |
|  [05]   | HVAC template families (`IHvac`) | energy  | `IdealAirSystemAbridged` (the `IIdealair` baseline system) plus the `AllAirBase`/`DOASBase`/`HeatCoolBase`/`EquipmentBase` template hierarchy and the concrete templates keyed by the `*EquipmentType` enums — the all-air `VAV`/`PVAV`/`PSZ`/`PTAC`, the DOAS-capable `FCU`/`VRF`/`WSHP`/`Radiant` (each additionally shipping a `withDOAS` variant, e.g. `FCUwithDOASAbridged` + `FCUwithDOASEquipmentType`), and the heat-cool/zone-equipment `Baseboard`/`ForcedAirFurnace`/`GasUnitHeater`/`WindowAC`/`Residential` — plus the freeform `DetailedHVAC`; all implement `HoneybeeSchema.Energy.IHvac` |
|  [06]   | `SimulationParameter`     | energy  | the run-control object (`RunPeriod`, `SimulationOutput`, `SizingParameter`, `ShadowCalculation`, `Terrain`, the `Vintages`/`ClimateZones`-typed sizing context) — the EnergyPlus simulation envelope |
|  [07]   | radiance/doe2 properties   | energy  | `ModelRadianceProperties` (`Modifiers`, `ModifierSets`, `SensorGrids`, `Views`) and `ModelDoe2Properties` — the daylight and DOE-2 extension stores |

[PUBLIC_TYPE_SCOPE]: enum vocabulary (string-backed)
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy

All 57 enums are `[DataContract]` + `[JsonConverter(typeof(StringEnumConverter))]` with
`[EnumMember(Value = "...")]` per member — they serialize as their string name, not an
ordinal. Fold them onto a `[SmartEnum]`/match at the Exchange boundary.

| [INDEX] | [SYMBOL]                  | [RAIL]  | [CAPABILITY]                                                                                              |
| :-----: | :------------------------ | :------ | :------------------------------------------------------------------------------------------------------- |
|  [01]   | `FaceType`                | energy  | `Wall`/`Floor`/`RoofCeiling`/`AirBoundary` — the `Face` discriminant mapped to an `IfcClass`/surface kind |
|  [02]   | `Units`                   | energy  | `Meters`/`Millimeters`/`Feet`/`Inches`/`Centimeters` — the `Model.Units` length unit |
|  [03]   | `Roughness`               | energy  | the `EnergyMaterial` surface-roughness band (`VeryRough`…`VerySmooth`) |
|  [04]   | `*EquipmentType` family   | energy  | `VAVEquipmentType`/`PVAVEquipmentType`/`PSZEquipmentType`/`PTACEquipmentType`/`FCUEquipmentType`/`VRFEquipmentType`/`WSHPEquipmentType`/`RadiantEquipmentType`/`BaseboardEquipmentType`/`FurnaceEquipmentType`/… plus the `*withDOASEquipmentType` mirrors — the HVAC-template selector enums |
|  [05]   | standards-context family   | energy  | `Vintages`, `ClimateZones`, `EfficiencyStandards`, `BuildingTypes`/`BuildingType` — the energy-standards lookup keys `Helper.EnergyLibrary` resolves |
|  [06]   | schedule/control family    | energy  | `ScheduleNumericType`, `ScheduleUnitType`, `DaysOfWeek`, `ControlType`, `VentilationControlType`, `EconomizerType`/`AllAirEconomizerType` — the temporal/control vocabulary |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: HBJSON round-trip
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                   | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :------------------------------------------------------------ | :----------------------------------------------------------- |
|  [01]   | `Model.FromJson`                | `(string json)` (static) -> `Model`                          | parse an HBJSON document into the typed model graph (the `new` static returns `Model`, not the base) |
|  [02]   | `Model.ToJson`                  | `(bool indented = false)` -> `string`                        | serialize the model to HBJSON via `LBT.Newtonsoft.Json` |
|  [03]   | `OpenAPIGenBaseModel.FromJson`  | `(string json)` (static) -> `OpenAPIGenBaseModel`            | polymorphic parse of any honeybee object; validates `Type` matches the runtime type name |
|  [04]   | `OpenAPIGenBaseModel.Duplicate` | `()` -> `OpenAPIGenBaseModel` (and `Model.DuplicateModel()`) | deep clone (a `ToJson`/`FromJson` round-trip under the hood) |
|  [05]   | `model.IsValid` / `model.Validate` | `(bool throwException = false)` -> `bool` / `()` -> `IEnumerable<ValidationResult>` | DataAnnotations validation; `Validate` yields the per-property results, `IsValid` is the boolean gate |

[ENTRYPOINT_SCOPE]: model assembly and standards library
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`, `HoneybeeSchema.Helper`
- rail: energy

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                                                 | [CAPABILITY]                                                  |
| :-----: | :--------------------------------- | :-------------------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | `new Model`                        | `(string identifier, ModelProperties properties, …, Units units = Meters, double tolerance = 0.01, double angleTolerance = 1.0)` | build a model from rooms/orphaned surfaces + the property store; `properties` is required (non-null) |
|  [02]   | `Model.AddConstructions` / `AddMaterials` / `AddSchedules` / `AddScheduleTypeLimits` / `AddConstructionSets` / `AddProgramTypes` / `AddHVACs` | `(IEnumerable<I…>)` | append energy objects into `Properties.Energy.*` (the model-level canonical lists the abridged children reference) |
|  [03]   | `Model.AddModifiers` / `AddModifierSets` | `(IEnumerable<I…>)`                                                    | append radiance objects into `Properties.Radiance.*` |
|  [04]   | `Model.MergeModelProperties`       | `(Model other)` -> `void`                                                   | fold another model's property store into this one (federating two authored models) |
|  [05]   | `Helper.EnergyLibrary` | `StandardEnergyLibrary`/`DefaultModelEnergyProperties`/`UserEnergyLibrary` (static) -> `ModelEnergyProperties`; `Get*ByIdentifier(string)` | the bundled standards energy library, the built-in defaults, and the user library; resolve one set by identifier via `GetResourcesByStandardConstructionSetIdentifier`/`GetConstructionSetByIdentifier`/`GetProgramTypeByIdentifier`/`GetHVACByIdentifier`/`Get{Opaque,Window}ConstructionByIdentifier`/`Get{Opaque,Window}MaterialByIdentifier`; `BuildingVintages` lists the available vintages, the `*Folder` statics locate the on-disk standards resources |

[ENTRYPOINT_SCOPE]: `Extension` static helpers (generic codec, library assembly, abridged resolution)
- package: `HoneybeeSchema`
- namespace: `HoneybeeSchema`
- rail: energy

`Extension` is the cross-cutting static helper beneath the `Model.Add*` instance methods — the generic JSON entry, the interface-typed deep copy, the `ModelEnergyProperties`/`ModelRadianceProperties`-keyed library mutators the `Model.Add*` methods delegate to, and the abridged-reference resolvers the `[ABRIDGED_REFERENCE_MODEL]` law reads. Dragonfly composes the same `Extension.To<T>` codec entry rather than re-deriving a parser.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                                                   | [CAPABILITY]                                                  |
| :-----: | :------------------------------ | :------------------------------------------------------------ | :----------------------------------------------------------- |
|  [01]   | `Extension.To<T>`               | `(this string json)` -> `T`                                  | generic HBJSON parse — reflects `T.FromJson(string)` and throws `InvalidOperationException` when `T` has no public static `FromJson`; the typed mirror of `OpenAPIGenBaseModel.FromJson` for a known target type |
|  [02]   | `Extension.Add*`                | `(this ModelEnergyProperties, I…)` / `(this ModelRadianceProperties, I…)` (+ plural mirrors) | `AddConstruction(Set)` / `AddMaterial` / `AddSchedule` / `AddScheduleTypeLimit` / `AddProgramType` / `AddHVAC` / `AddSHW` / `AddModifier(Set)` — the property-store-keyed library mutators (dedupe-by-identifier into the canonical list) that `Model.AddConstructions`/etc. forward to |
|  [03]   | `Extension.Duplicate`           | `(this ILoad / IMaterial / IConstruction / IConstructionset)` -> same | interface-typed deep clone preserving the concrete case (a `ToJson`/`FromJson` round-trip under the hood) |
|  [04]   | `Extension.GetAllConstructions` / `GetAbridgedConstructionMaterials` / `GetAllSchedules` / `GetAllModifiers` | `(this ConstructionSetAbridged / IConstruction / ProgramTypeAbridged / ModifierSetAbridged)` -> `List<string>` | abridged-reference resolution — enumerate the library identifiers an abridged object points at; the read side of the `[ABRIDGED_REFERENCE_MODEL]` law |
|  [05]   | `Extension.GetUserData` / `AddUserData` / `ToDictionary` | `(this IIDdBaseModel, …)`                                  | typed access to the `UserData` dictionary every identified object carries |

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
