# [RASM_BIM_API_BRICKSCHEMA_NET]

`BrickSchema.Net` owns the Brick ontology graph and its JSON-LD codec: `BrickEntity` nodes on a `BrickSchemaManager`, each carrying `Properties`, typed `BrickRelationship` edges, `BrickShape` rules, and timer-driven `BrickBehavior` analytics. Its canonical entry is the polymorphic `AddEntity<T>` / `GetEntities<T>` / `SearchEntities` triad discriminating by type argument, with `AddEquipment*`/`AddCollection*` factories as sugar over it. Owned scope is building-systems semantics — BMS metadata, BACnet/Modbus point binding, conformance analytics — never geometry, IFC authoring, or simulation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `BrickSchema.Net`
- package: `BrickSchema.Net` (MIT)
- assembly: `BrickSchema.Net`
- namespace: `BrickSchema.Net` (core graph), `.Classes` (+ `.Equipments`/`.Equipments.HVACType`/`.Chillers`/`.TerminalUnits`, `.Points`, `.Locations`, `.Measureable`, `.Collection`/`.Loop`, `.Devices`), `.Relationships`, `.Behaviors`, `.EntityProperties`, `.Shapes`, `.Helpers`
- asset: `net7.0`; the `net10.0` consumer binds `lib/net7.0` (single TFM, binds forward)
- depends: `Newtonsoft.Json` (JSON-LD codec), `Microsoft.Extensions.Logging.Abstractions` (`ILogger` behavior diagnostics)
- rail: building-systems

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph manager and entity base

Every node is a `BrickEntity`; the four base derivations are the node kinds, and `BrickSchemaManager` holds, persists, and queries the graph.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :------------------- | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `BrickSchemaManager` | class         | the graph — ctors, load/save, add/get/search/update, registry queries            |
|  [02]   | `BrickEntity`        | class         | base node: `Id`, `Type`, typed `Properties`/`Relationships`/`Shapes`/`Behaviors` |
|  [03]   | `BrickClass`         | class         | `: BrickEntity` taxonomy node; `Tag : BrickClass` carries tagged-entity lookup   |
|  [04]   | `EntityProperty`     | class         | string-backed `Id`/`Type`/`Name`/`Value` with `SetValue<T>`/`GetValue<T>`        |
|  [05]   | `PropertiesEnum`     | enum          | the canonical property-key vocabulary                                            |

[PUBLIC_TYPE_SCOPE]: relationship edges

Each `BrickRelationship : BrickEntity` is a directed edge between two nodes, traversed through the `BrickEntity.Get*` helpers.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :--------------- | :------------ | :---------------------------- |
|  [01]   | `Fedby`          | class         | feeds — flow edge             |
|  [02]   | `PointOf`        | class         | has-point — telemetry edge    |
|  [03]   | `PartOf`         | class         | is-part-of — composition edge |
|  [04]   | `LocationOf`     | class         | located-in — spatial edge     |
|  [05]   | `MeterBy`        | class         | load metered by a meter       |
|  [06]   | `SubmeterOf`     | class         | submeter under a meter        |
|  [07]   | `AssociatedWith` | class         | generic association           |
|  [08]   | `TagOf`          | class         | tag membership (`Tag`↔entity) |

[PUBLIC_TYPE_SCOPE]: shapes — validation and analytic rules

`BrickShape : BrickEntity` is the SHACL-like constraint node; concrete shapes are built-in analytic and lifecycle rules attached through `AddShape<T>` and read via `BrickEntity.Shapes`.

| [INDEX] | [SYMBOL]                                                          | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `BrickShape`                                                      | class         | base validation/rule shape (`Clone()`)           |
|  [02]   | `Aggregation`                                                     | class         | time-series roll-up (`AggregateByInterval`)      |
|  [03]   | `AggregationMode`                                                 | enum          | `Min`/`Count`/`Mean`/`Sum`/`Median` roll-up mode |
|  [04]   | `BuildingMeterRule`                                               | class         | building-level metering reconciliation           |
|  [05]   | `Deprecation` / `DeprecationRule` / `DeprecationRuleForInstances` | class         | mark and rewrite deprecated classes/instances    |
|  [06]   | `BACnetReference`                                                 | class         | BACnet object binding for a Brick `Point`        |

- `Aggregation.AggregateByInterval(series, intervalMinutes, mode)` resamples then folds point streams; `ChillerLoadCOP` is a plain `Load`/`COP` value pair under `.Classes.Equipments.HVACType.Chillers`, not a shape.

[PUBLIC_TYPE_SCOPE]: behaviors — runtime analytics

`BrickBehavior : BrickEntity` is the executable analytic attached to an entity — a timer-driven computation emitting an `Insight`/`Resolution` and a conformance `Weight`.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                                                |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------------------------------------------ |
|  [01]   | `BrickBehavior`       | class         | lifecycle — ctor, run/timer, value/conformance/logger setters, insight fields               |
|  [02]   | `BehaviorValue`       | class         | an output cell (value + weight) the entity stores and reads back                            |
|  [03]   | `BehaviorReturnCodes` | enum          | `NotImplemented`=-1, `Good`=0, `Skip`=1, `HasWarning`=10, `HasError`=50, `HasException`=100 |

[PUBLIC_TYPE_SCOPE]: taxonomy and device classes

Generated `BrickClass` subclasses are the vocabulary instantiated through `AddEntity<T>` — one polymorphic family discriminated by the `<T>` argument, never separate call surfaces.

| [INDEX] | [NAMESPACE]            | [CAPABILITY]                                                                                        |
| :-----: | :--------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `.Classes.Equipments`  | HVAC + plant (+ `.HVACType`/`.Chillers`/`.TerminalUnits`) — `AHU`/`VAV`/`Chiller`/`Boiler`/`Pump`/… |
|  [02]   | `.Classes.Points`      | telemetry — sensors, setpoints, commands, status, alarms, parameters                                |
|  [03]   | `.Classes.Locations`   | spatial — `Building`/`Floor`/`Room`/`Zone`/`Space`/`CommonSpace`                                    |
|  [04]   | `.Classes.Measureable` | quantities + substances — `Temperature`/`Pressure`/`ActivePower`/`Air`/`ChilledWater`/…             |
|  [05]   | `.Classes.Collection`  | grouping (+ `.Loop`) — `Loop`/`System`/`Portfolio`/`PVArray`                                        |
|  [06]   | `.Classes.Devices`     | BMS hardware — `BACnetDevice`/`ModbusDevice`/`Workstation`/`Server`/`Camera`                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: graph build, persist, query (`BrickSchemaManager`); `AddEntity<T>` constrains `where T : BrickEntity, new()`

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :----------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `BrickSchemaManager()` / `(string brickFilePath)`            | ctor     | empty graph / load-on-construct                       |
|  [02]   | `AddEntity<T>(string? id, string? name) -> T`                | instance | polymorphic create — `<T>` is the Brick class         |
|  [03]   | `GetEntity(string id, bool byRef) -> BrickEntity?`           | instance | id lookup                                             |
|  [04]   | `GetEntities(bool byRef) -> List<BrickEntity>`               | instance | full roster                                           |
|  [05]   | `GetEntities<T>(bool byRef) -> List<BrickEntity>`            | instance | type-filtered roster                                  |
|  [06]   | `SearchEntities(Func<dynamic,bool>) -> List<dynamic>`        | instance | the single predicate query                            |
|  [07]   | `UpdateEntity(dynamic) -> bool`                              | instance | upsert by `Id`                                        |
|  [08]   | `IsEntity(string) -> bool` / `IsTag(string) -> bool`         | instance | existence / tag test                                  |
|  [09]   | `GetTag(string, bool byRef) -> Tag?`                         | instance | tag lookup by name                                    |
|  [10]   | `LoadSchema(string)` / `SaveSchema()` / `SaveSchema(string)` | instance | JSON-LD whole-graph read / write                      |
|  [11]   | `GetBehaviors(List<string>, bool) -> List<BrickBehavior>`    | instance | behavior-registry query                               |
|  [12]   | `GetEquipmentBehaviors(string, bool) -> List<BrickBehavior>` | instance | per-equipment behaviors                               |
|  [13]   | `GetRegisteredEquipmentBehaviors(string, bool)`              | instance | registered-behavior map (`Dictionary<string,string>`) |

[ENTRYPOINT_SCOPE]: entity properties, relationships, behaviors (`BrickEntity`)

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `AddOrUpdateProperty<T>(string\|PropertiesEnum, T)` | instance | typed property set (string-backed) |
|  [02]   | `GetProperty<T>(string\|PropertiesEnum) -> T?`      | instance | typed property get                 |
|  [03]   | `GetPointEntity(string tagName) -> Point?`          | instance | resolve one `Point` by tag         |
|  [04]   | `AddRelationship<T>(string parentId) -> T`          | instance | directed edge to a parent          |
|  [05]   | `AddShape<T>() -> T`                                | instance | attach a `BrickShape`              |
|  [06]   | `GetRelationships()` / `GetShapes()` / `GetTags()`  | instance | edge / shape / tag enumeration     |
|  [07]   | `AddBehavior(BrickBehavior) -> BrickBehavior`       | instance | attach an analytic                 |
|  [08]   | `RemoveBehavior(string\|BrickBehavior)`             | instance | detach an analytic                 |
|  [09]   | `GetBehaviors(bool)` / `GetBehaviors(string, bool)` | instance | read attached analytics            |
|  [10]   | `Clone() -> BrickEntity`                            | instance | covariant deep copy of a node      |

- Edge traversal (`-> List<BrickEntity>`): downstream `GetChildEntities`/`GetFedEntitites`/`GetMeterEntities`/`GetPartEntitites`/`GetPointEntities`, upstream `GetFeedingParent`/`GetMeetingParent`/`GetPartOfParent`/`GetPointOfParent`.
- Behavior-output cells: `SetBehaviorValue<T>(BrickBehavior, string, T)` and `SetBehaviorValue(BehaviorValue\|List<BehaviorValue>)` write; `GetBehaviorValue<T>(string, string) -> T?` reads the value, `GetBehaviorValue<T,U>(string, string) -> (T?,U?)` the value with weight.

[ENTRYPOINT_SCOPE]: behavior execution (`BrickBehavior`)

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `BrickBehavior(string mode, string name, double weight, ILogger?)` | ctor     | construct a timer-driven analytic             |
|  [02]   | `Start()` / `Stop()` / `Execute()` / `OnTimerTick()`               | instance | lifecycle — timer or explicit run             |
|  [03]   | `SetConformance(double) -> BehaviorValue`                          | instance | set the conformance cell                      |
|  [04]   | `SetBehaviorValue<T>(string\|PropertiesEnum, T) -> BehaviorValue`  | instance | set a named output cell                       |
|  [05]   | `SetLogger(ILogger?)`                                              | instance | bind `ILogger` diagnostics                    |
|  [06]   | `IsRunning` / `Description` / `Insight` / `Resolution` / `Weight`  | property | run-state flag and insight/conformance fields |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One polymorphic entry owns each mutation, discriminated by `<T>`: `AddEntity<T>()` creates a node, `AddRelationship<T>(parentId)` wires a directed edge, `AddShape<T>()` attaches a rule shape; the `AddEquipment*`/`AddCollection*`/`AddRelationship*`/`AddShape*` named helpers are sugar over them, so a call discriminates by type argument, never a hand-picked factory name.
- Query is one entry per shape: `SearchEntities(Func<dynamic,bool>)` for predicates, `GetEntities<T>` for type filters, `GetEntity(id)` for id lookup — there is no `GetByName`/`GetByType` family.
- `byReference` selects the live graph node versus a clone: `true` mutates in place, `false` (default) returns a read-only copy, and mutating a `false`-fetched node never touches the graph.
- A node's neighborhood is its typed relationship set, traversed through the `BrickEntity.Get*` helpers that resolve `Fedby`/`PointOf`/`PartOf`/`LocationOf`/`MeterBy` edges in the requested direction — `Point` access via `GetPointEntities`/`GetPointEntity(tagName)`, meter hierarchy via `GetMeterEntities`/`MeterBy`/`SubmeterOf`, spatial containment via `LocationOf` — never a raw `Relationships` walk.
- Extensibility splits two kinds: `BrickShape` is the declarative constraint/analytic rule attached via `AddShape<T>` and enumerated through `Entity.Shapes`, and `BrickBehavior` is the imperative timer computation attached via `AddBehavior` that emits `Insight`/`Resolution`/conformance through `BehaviorReturnCodes`, routes diagnostics through the injected `ILogger`, and returns outputs as typed `BehaviorValue` cells.
- JSON-LD is the wire: the whole graph — entities, properties, relationships, shapes, behaviors — round-trips through `LoadSchema`/`SaveSchema`; persist the manager, never individual nodes.

[STACKING]:
- `GeometryGymIFC`(`api-geometrygym-ifc.md`): Brick `Building`/`Floor`/`Room`/`Zone` bind to `IfcBuildingStorey`/`IfcSpace` through `LocationOf` and equipment/points to `IfcDistributionElement`/`IfcSensor`; the IFC model is authoritative for geometry and spatial structure, Brick overlays the operational systems graph, and the `Point`↔IFC element identity is the join key.
- `Xbim.Properties`(`api-xbim-properties.md`): IFC Psets seed Brick `EntityProperty` values as the static design metadata.
- `Xbim.CobieExpress`(`api-xbim-cobieexpress.md`): the COBie asset-handover view shares the equipment roster; Brick is the runtime systems graph, COBie the handover snapshot of the same equipment.
- `UnitsNet`(`libs/csharp/.api/api-unitsnet.md`): a `Measureable` point value reconciles to a canonical quantity at the boundary, so a BACnet engineering-unit reading and a model expectation agree before a `BrickBehavior` computes conformance.
- `System.IO.Hashing`(`libs/csharp/.api/api-hashing.md`): a `SaveSchema` JSON-LD document's bytes mint the systems-graph snapshot content key on the shared content-identity rail, joining the IFC and energy-model exports; the `Deprecation*` shapes carry the schema-version migration a diff reads.
- within-library: AppHost OPC UA/MQTT/Modbus transport owners stream live values onto `BACnetReference`/`BACnetDevice`/`ModbusDevice` point bindings, Brick the semantic target they land on.

[LOCAL_ADMISSION]:
- A Brick graph enters through `BrickSchemaManager.LoadSchema` or a build via `AddEntity<T>` with typed `AddOrUpdateProperty`/relationship wiring, then maps onto the canonical Bim systems carriers keyed by entity `Id`; `BrickSchema.Net.*` types stay at the systems-exchange boundary.
- A Brick graph exports through canonical→entity construction — location/equipment/point nodes, `Fedby`/`PointOf`/`LocationOf` edges, IFC-seeded `EntityProperty` values — then `SaveSchema`.

[RAIL_LAW]:
- Package: `BrickSchema.Net`
- Owns: the Brick ontology runtime graph and JSON-LD codec — the `BrickEntity` taxonomy, typed `BrickRelationship` edges, `BrickShape` analytic/validation rules, `BrickBehavior` runtime analytics, and the BACnet/Modbus device and point-binding model
- Accept: building-systems semantic metadata, BMS/digital-twin graphs, systems classification, runtime conformance analytics, live point binding
- Reject: geometry and meshes (the kernel and GeometryGym own them), IFC authoring (GeometryGym is the system of record — Brick overlays, never re-authors), energy simulation (Dragonfly/Honeybee/OpenStudio), protocol transport (the AppHost OPC UA/MQTT/Modbus owners), and leaking `BrickSchema.Net.*` types past the systems-exchange boundary
