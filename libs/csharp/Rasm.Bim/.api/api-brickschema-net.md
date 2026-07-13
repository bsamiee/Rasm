# [RASM_BIM_API_BRICKSCHEMA_NET]

`BrickSchema.Net` is the.NET runtime object model + JSON-LD codec for the Brick ontology — the
uniform building-systems metadata schema. It is not a static vocabulary: it is a live graph of
`BrickEntity` nodes managed by a `BrickSchemaManager`, where every node (the equipment/point/
location/measurable/collection/device taxonomy of ~280 generated classes) carries string-typed
`Properties`, typed `BrickRelationship` edges (`Fedby`/`PointOf`/`PartOf`/`LocationOf`/`MeterBy`/
`SubmeterOf`/`AssociatedWith`/`TagOf`), `BrickShape` analytic+validation rules, and runtime
`BrickBehavior` analytics. The canonical entry is the polymorphic `AddEntity<T>` /
`GetEntities<T>` / `SearchEntities(predicate)` — the hundreds of `AddEquipmentHVAC*` /
`AddCollection*` factory methods are taxonomy sugar over it. It owns the building-systems
semantic layer (BMS/digital-twin metadata, BACnet/Modbus point binding, runtime conformance
analytics); it is not a geometry, IFC-authoring, or simulation owner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `BrickSchema.Net`
- package: `BrickSchema.Net`
- license: MIT
- assembly: `BrickSchema.Net`
- namespace: `BrickSchema.Net` (core graph), `.Classes` (+ `.Equipments`/`.Equipments.HVACType`/`.Chillers`/`.TerminalUnits`, `.Points`, `.Locations`, `.Measureable`, `.Collection`/`.Loop`, `.Devices`), `.Relationships`, `.Behaviors`, `.EntityProperties`, `.Shapes`, `.Helpers`
- asset: `net7.0`; the `net10.0` consumer binds `lib/net7.0` (single TFM, binds forward)
- serialization: `Newtonsoft.Json` (the Brick graph persists as JSON-LD through `BrickSchemaManager.LoadSchema`/`SaveSchema`)
- transitive-floor: `Newtonsoft.Json`, `Microsoft.Extensions.Logging.Abstractions` ( — `ILogger` for `BrickBehavior` analytics)
- rail: building-systems

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: graph manager and entity base
- namespace: `BrickSchema.Net` (`BrickSchemaManager`/`BrickEntity`); `BrickClass`/`Tag` live in `.Classes`, `EntityProperty`/`PropertiesEnum` in `.EntityProperties`
- rail: building-systems

Everything is a `BrickEntity`; the four base derivations (`BrickClass`, `BrickRelationship`, `BrickShape`, `BrickBehavior`) are the kinds of node, and `BrickSchemaManager` is the graph that holds, persists, and queries them.

| [INDEX] | [SYMBOL]             | [CAPABILITY]                                                                                      |
| :-----: | :------------------- | :------------------------------------------------------------------------------------------------ |
|  [01]   | `BrickSchemaManager` | the graph — ctors, load/save, add/get/search/update, tag + behavior-registry queries — see [01]   |
|  [02]   | `BrickEntity`        | base node: `Id`, `Type`, typed `Properties`/`Relationships`/`Shapes`/`Behaviors` lists — see [02] |
|  [03]   | `BrickClass`         | `: BrickEntity` typed taxonomy node; `Tag: BrickClass` adds `GetEntities()` (tagged entities)     |
|  [04]   | `EntityProperty`     | `Id`/`Type`/`Name`/`Value` (string), `SetValue<T>`/`GetValue<T>`, `ctor(id,type,name,value)`      |
|  [05]   | `PropertiesEnum`     | the canonical property keys — see [05]                                                            |

- [01]-[BRICK_SCHEMA_MANAGER]: `ctor()`/`ctor(brickFilePath)`, `LoadSchema`/`SaveSchema` (JSON-LD), `AddEntity<T>`, `GetEntity`/`GetEntities`/`GetEntities<T>`, `SearchEntities(predicate)`, `UpdateEntity`, `IsEntity`/`IsTag`/`GetTag`, behavior registry queries.
- [05]-[PROPERTIES_ENUM]: `Name`/`Description`/`BrickClass`/`Info`/`Value`/`Timestamp`/`ValueQuality`/`Behaviors`/`PollRate`/`Running`/`Insight`/….
- [02]-[BRICK_ENTITY]: `Properties` (`List<EntityProperty>`), `Relationships` (`List<BrickRelationship>`), `Shapes` (`List<BrickShape>`), `Behaviors` (`List<BrickBehavior>`); typed property + relationship-traversal + behavior-value surface.

[PUBLIC_TYPE_SCOPE]: relationship edges
- namespace: `BrickSchema.Net.Relationships`
- rail: building-systems

The Brick relationship algebra — each is a `BrickRelationship: BrickEntity` directed edge between two nodes; traversal is via the `BrickEntity.Get*` helpers, not raw list walking.

| [INDEX] | [SYMBOL]                                      | [CAPABILITY]                                                                      |
| :-----: | :-------------------------------------------- | :-------------------------------------------------------------------------------- |
|  [01]   | `Fedby` / `PointOf` / `PartOf` / `LocationOf` | feeds (flow), has-point (telemetry), is-part-of (composition), located-in (space) |
|  [02]   | `MeterBy` / `SubmeterOf`                      | metering hierarchy (a load metered by a meter; a submeter under a meter)          |
|  [03]   | `AssociatedWith` / `TagOf`                    | generic association; tag membership (`Tag`↔entity)                                |

[PUBLIC_TYPE_SCOPE]: shapes — validation and analytic rules
- namespace: `BrickSchema.Net.Shapes`, `BrickSchema.Net`
- rail: building-systems

`BrickShape: BrickEntity` is the SHACL-like constraint/rule node; the concrete shapes are the built-in analytic and lifecycle rules attached to entities.

| [INDEX] | [SYMBOL]                                                          | [CAPABILITY]                                                     |
| :-----: | :---------------------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `BrickShape`                                                      | base validation/rule shape (`Clone()`), via `BrickEntity.Shapes` |
|  [02]   | `Aggregation` / `AggregationMode`                                 | time-series roll-up shape — see [02]                             |
|  [03]   | `BuildingMeterRule`                                               | building-level metering rule — see [03]                          |
|  [04]   | `Deprecation` / `DeprecationRule` / `DeprecationRuleForInstances` | shapes marking/rewriting deprecated classes/instances            |
|  [05]   | `BACnetReference`                                                 | BACnet object-binding (Brick `Point` → BACnet object)            |

- [02]-[AGGREGATION]: `Aggregation.AggregateByInterval(series, intervalMinutes, mode)` resamples then folds point streams; nested `AggregationMode` = `Min`/`Count`/`Mean`/`Sum`/`Median` (the complete set).
- [03]-[BUILDING_METER_RULE]: domain analytic reconciling submetered loads against the building meter (`ChillerLoadCOP` is NOT a shape: a plain `Load`/`COP` value pair under `.Classes.Equipments.HVACType.Chillers`).

[PUBLIC_TYPE_SCOPE]: behaviors — runtime analytics
- namespace: `BrickSchema.Net`, `BrickSchema.Net.Behaviors`
- rail: building-systems

`BrickBehavior: BrickEntity` is the executable analytic attached to an entity — a timer-driven computation that emits an `Insight`/`Resolution` and a conformance/`Weight`.

| [INDEX] | [SYMBOL]              | [CAPABILITY]                                                                                        |
| :-----: | :-------------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `BrickBehavior`       | lifecycle — ctor, run/timer, value/conformance/logger setters, insight fields — see [01]            |
|  [02]   | `BehaviorValue`       | a behavior output cell (value + weight) the entity stores and reads back                            |
|  [03]   | `BehaviorReturnCodes` | result: `NotImplemented`=-1, `Good`=0, `Skip`=1, `HasWarning`=10, `HasError`=50, `HasException`=100 |

- [01]-[BRICK_BEHAVIOR]: `ctor(behaviorMode, behaviorName, weight, ILogger?)`, `Start`/`Stop`/`Execute`/`OnTimerTick`, `IsRunning`, `SetLogger(ILogger?)`, `SetConformance(double)`, `SetBehaviorValue<T>`, `Description`/`Insight`/`Resolution`/`Info`/`Weight`/`LastExecutionStart`/`End`/`Parent`.

[PUBLIC_TYPE_SCOPE]: taxonomy and device classes
- namespace: `BrickSchema.Net.Classes.*`
- rail: building-systems

The generated Brick class taxonomy (`BrickClass` subclasses) — the vocabulary the caller instantiate through `AddEntity<T>`. Do not enumerate them as separate surfaces; they are one polymorphic family discriminated by the `<T>` argument.

| [INDEX] | [NAMESPACE]            | [CAPABILITY]                                                                                           |
| :-----: | :--------------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `.Classes.Equipments`  | HVAC + plant (+ `.HVACType`/`.Chillers`/`.TerminalUnits`) — see [01]                                   |
|  [02]   | `.Classes.Points`      | telemetry vocabulary — sensors, setpoints, commands, status, alarms, parameters                        |
|  [03]   | `.Classes.Locations`   | spatial — `Building`/`Floor`/`Room`/`Zone`/`Space`/`CommonSpace`                                       |
|  [04]   | `.Classes.Measureable` | quantities + substances — `Temperature`/`Pressure`/`ActivePower`/`ActiveEnergy`/`Air`/`ChilledWater`/… |
|  [05]   | `.Classes.Collection`  | grouping (+ `.Loop`) — `Loop`(`Air`/`Water`/`ChilledWater`/`HotWater`)/`System`/`Portfolio`/`PVArray`  |
|  [06]   | `.Classes.Devices`     | BMS hardware — `BACnetDevice`/`ModbusDevice`/`Workstation`/`Server`/`Camera`                           |

- [01]-[EQUIPMENTS]: `AHU`/`VAV`/`CAV`/`Chiller`(`Absorption`/`Centrifugal`)/`Boiler`/`CoolingTower`/`Pump`/`Fan`/`Damper`/`Valve`/`Compressor`/`Condenser`/….

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: graph build, persist, query (`BrickSchemaManager`); `AddEntity<T>` constrains `where T:BrickEntity,new()`
- namespace: `BrickSchema.Net`
- rail: building-systems

| [INDEX] | [SURFACE]                                           | [CAPABILITY]                                                                     |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | `AddEntity<T>`                                      | canonical polymorphic create — `<T>` IS the Brick class (factory sugar wraps it) |
|  [02]   | `GetEntity` / `GetEntities` / `GetEntities<T>`      | id lookup / full roster / type-filtered roster                                   |
|  [03]   | `SearchEntities`                                    | predicate query — the single search entry                                        |
|  [04]   | `UpdateEntity` / `IsEntity` / `IsTag` / `GetTag`    | upsert / existence / tag membership                                              |
|  [05]   | `LoadSchema` / `SaveSchema`                         | JSON-LD graph read / write (whole entity graph)                                  |
|  [06]   | `GetBehaviors` / `GetEquipmentBehaviors`            | behavior registry queries                                                        |
|  [07]   | `GetRegisteredEquipmentBehaviors` / `GetEquipments` | registered-behavior + equipment queries                                          |

Call shapes, keyed to the rows above:
- [01]: `<T>(string? id = null, string? name = null) → T`.
- [02]: `(string id[, bool byReference]) → BrickEntity?` · `<T>(bool byReference=false) → List<BrickEntity>`.
- [03]: `(Func<dynamic,bool> predicate) → List<dynamic>`.
- [04]: `(dynamic) → bool` · `(string) → bool` · `(string, bool=false) → Tag?`.
- [05]: `(string jsonLdFilePath)` · `() / (string jsonLdFilePath)`.
- [06]: `(List<string> ids, bool byReference=false)`.
- [07]: `(string equipmentId, …)`.

[ENTRYPOINT_SCOPE]: entity properties, relationships, behaviors (`BrickEntity`)
- namespace: `BrickSchema.Net`
- rail: building-systems

| [INDEX] | [SURFACE]                                                               | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `AddOrUpdateProperty<T>` / `GetProperty<T>`                             | typed property set/get (string-backed)           |
|  [02]   | relationship traversal (`GetFedEntitites`/`GetPointEntities`/…)         | walk typed edges either direction — see [02]     |
|  [03]   | `AddRelationship<T>` / `AddShape<T>`                                    | edge/shape create (sugar wraps) — see [03]       |
|  [04]   | `GetRelationships` / `GetShapes` / `GetTags`                            | edge / shape / tag enumeration                   |
|  [05]   | `AddBehavior` / `RemoveBehavior` / `GetBehaviors`                       | attach/detach/read runtime analytics             |
|  [06]   | `SetBehaviorValue<T>` / `GetBehaviorValue<T>` / `GetBehaviorValue<T,U>` | behavior-output cells (value, value+weight)      |
|  [07]   | `Clone`                                                                 | `() → BrickEntity` covariant deep copy of a node |

- [01]: `<T>(string\| PropertiesEnum, T)` · `<T>(string\| PropertiesEnum) → T?`.
- [02]-[RELATIONSHIP_TRAVERSAL]: `GetChildEntities`/`GetFedEntitites`/`GetMeterEntities`/`GetPartEntitites`/`GetPointEntities`/`GetPointEntity(tag)` · `GetFeedingParent`/`GetMeetingParent`/`GetPartOfParent`/`GetPointOfParent`.
- [03]-[ADD_RELATIONSHIP_SHAPE]: `<T>(string parentId) where T:BrickRelationship,new() → T` · `<T>() where T:BrickShape,new() → T`; enumeration `() → List<BrickRelationship>` \| `List<BrickShape>` \| `List<Tag>`; the typed `AddRelationshipFedBy`/`AddRelationshipPointOf`/… and `AddShapeAggregation`/… sugar wraps these.
- [05]: `(BrickBehavior) → BrickBehavior` · `(string\| BrickBehavior)` · `(bool\| string type)`.
- [06]: `(behaviorId, valueName, T)` · `<T>(behaviorId, valueName) → T?` · `<T,U>(…) → (T? Value, U? Weight)`.

[ENTRYPOINT_SCOPE]: behavior execution (`BrickBehavior`)
- namespace: `BrickSchema.Net`
- rail: building-systems

| [INDEX] | [SURFACE]                                    | [CALL_SHAPE]                     | [CAPABILITY]                                 |
| :-----: | :------------------------------------------- | :------------------------------- | :------------------------------------------- |
|  [01]   | `Start` / `Stop` / `Execute` / `OnTimerTick` | `()`                             | lifecycle (timer or explicit run)            |
|  [02]   | `SetConformance` / `SetBehaviorValue<T>`     | `(double)` · `<T>(valueName, T)` | conformance / named output → `BehaviorValue` |
|  [03]   | `SetLogger`                                  | `(ILogger?)`                     | bind `ILogger` for behavior diagnostics      |

## [04]-[IMPLEMENTATION_LAW]

[POLYMORPHIC_ENTITY_LAW]:
- the create entry is `AddEntity<T>()` where `<T>` is the Brick class; the ~200 `AddEquipment*`/`AddCollection*`/`AddBACnetDevice`/… helpers are convenience overloads over it. Prefer `AddEntity<T>` (and `GetEntities<T>`) so the call discriminates by type argument, not by a hand-picked factory name.
- the same polymorphic shape repeats on `BrickEntity`: `AddRelationship<T>(parentId)` wires a directed edge to a parent (the `AddRelationshipFedBy`/`AddRelationshipPointOf`/`AddRelationshipLocationOf`/`AddRelationshipMeterBy`/`AddRelationshipSubmeterOf`/`AddRelationshipPartOf`/`AddRelationshipAssociatedWith`/`AddRelationshipTagOf` sugar wraps it) and `AddShape<T>()` attaches a `BrickShape` (the ~120 `AddShape*` quantity/rule helpers wrap it). Discriminate by `<T>`, never by the named helper.
- query is one entry: `SearchEntities(Func<dynamic,bool>)` for predicate queries, `GetEntities<T>` for type filters, `GetEntity(id)` for id lookup — there is no `GetByName`/`GetByType` family to reach for.
- `byReference` selects the live graph node vs a clone: pass `true` to mutate in place, `false` (default) for a read-only copy. Mutating a `false`-fetched node does not touch the graph.

[GRAPH_TOPOLOGY_LAW]:
- a node's neighborhood is the typed relationship set; traverse via the `BrickEntity.Get*` helpers (`GetFedEntitites`/`GetPointEntities`/`GetPartEntitites`/`GetFeedingParent`/…), which resolve `Fedby`/`PointOf`/`PartOf`/`LocationOf`/`MeterBy` edges in the requested direction — never walk `Relationships` raw.
- a `Point` is reached from its equipment via `GetPointEntities()` / `GetPointEntity(tagName)`; a meter hierarchy via `GetMeterEntities`/`MeterBy`/`SubmeterOf`; spatial containment via `LocationOf`.

[ANALYTIC_LAW]:
- two extensibility kinds: `BrickShape` (declarative constraint/analytic rule attached via `AddShape<T>()`/`AddShape*` and enumerated through `Entity.Shapes` — `Aggregation`/`BuildingMeterRule`/`Deprecation*`/`BACnetReference` plus the ~120 generated quantity shapes) and `BrickBehavior` (imperative timer-driven computation attached via `AddBehavior`, emitting `Insight`/`Resolution`/conformance through `BehaviorReturnCodes`). Behavior diagnostics route through the injected `ILogger`; behavior outputs are read back as typed `BehaviorValue` cells.
- the wire is JSON-LD: the whole graph (entities + properties + relationships + shapes + behaviors) round-trips through `LoadSchema`/`SaveSchema` (Newtonsoft); persist the manager, not individual nodes.

[INTEGRATION_STACK]:
- IFC-federation leg: Brick is the systems-semantic overlay on the `GeometryGym` IFC model (`api-geometrygym-ifc`) — Brick `Building`/`Floor`/`Room`/`Zone` locations bind to `IfcBuildingStorey`/`IfcSpace` (via `LocationOf`), and Brick equipment/points bind to `IfcDistributionElement`/`IfcSensor`; the IFC geometry/spatial structure is authoritative, Brick adds the operational systems graph and live telemetry binding. `Point` ↔ IFC element identity is the join key.
- property/asset leg: IFC Psets (`api-xbim-properties`) seed Brick `EntityProperty` values (the static design metadata), and the asset/handover view (`api-xbim-cobieexpress`, COBie) shares the equipment roster — Brick is the runtime systems graph, COBie the asset-handover snapshot of the same equipment.
- units leg: a Brick `Measureable` point value (`Temperature`/`ActivePower`/`Pressure`) reconciles to a canonical quantity through `UnitsNet` (`api-unitsnet`) at the boundary, so a BACnet engineering-unit reading and a model expectation agree before a `BrickBehavior` computes conformance.
- live-binding leg: `BACnetReference` (and the `BACnetDevice`/`ModbusDevice` device classes) map a Brick `Point` to a live BMS object; the AppHost industrial-protocol owners (OPC UA / MQTT / Modbus) are the transport, Brick is the semantic target the streamed values land on through the point binding.
- identity leg: a `SaveSchema` JSON-LD document's UTF-8 bytes feed `System.IO.Hashing` `XxHash3`/`XxHash128` (`api-hashing`) for the systems-graph snapshot content key, joining the IFC/energy-model exports on one content-identity rail; the `Deprecation*` shapes carry the schema-version migration the diff reads.

[LOCAL_ADMISSION]:
- a Brick graph enters through `BrickSchemaManager.LoadSchema` (JSON-LD) or is built via `AddEntity<T>` + typed `AddOrUpdateProperty`/relationship wiring, then maps onto the canonical Bim systems carriers keyed by entity `Id`; `BrickSchema.Net.*` types stay at the systems-exchange boundary.
- a Brick graph exports through canonical→entity construction (location/equipment/point nodes, `Fedby`/`PointOf`/`LocationOf` edges, IFC-seeded `EntityProperty` values) then `SaveSchema`.

[RAIL_LAW]:
- Package: `BrickSchema.Net`
- Owns: the Brick ontology runtime graph + JSON-LD codec — the `BrickEntity` taxonomy, the typed `BrickRelationship` edges, the `BrickShape` analytic/validation rules, the `BrickBehavior` runtime analytics, and the BACnet/Modbus device + point-binding model
- Accept: building-systems semantic metadata, BMS/digital-twin graphs, systems classification, runtime conformance analytics, live point binding
- Reject: geometry/meshes (the kernel + GeometryGym own them), IFC authoring (GeometryGym is the IFC system of record — Brick overlays, never re-authors), energy simulation (Dragonfly/Honeybee/OpenStudio), protocol transport (the AppHost OPC UA/MQTT/Modbus owners), and leaking `BrickSchema.Net.*` types past the systems-exchange boundary
