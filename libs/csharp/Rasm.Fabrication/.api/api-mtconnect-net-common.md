# [RASM_FABRICATION_API_MTCONNECT_NET_COMMON]

`MTConnect.NET-Common` (TrakHound, `MIT`) is the ISO-13399-aligned MTConnect cutting-tool asset model — the in-memory `MTConnect.Assets.CuttingTools` graph backing the tool-data MODEL half of the `Tooling/magazine` `Magazine`/`ToolAssembly` catalogue. A `CuttingToolAsset` carries a `CuttingToolLifeCycle` (cutter status, tool-life budget, feed/speed envelope, magazine `Location`, the ISO-13399 `Measurements` family) and its `CuttingItem` inserts, held in memory rather than over a wire. `GenerateHash` is the structural boundary digest; catalogue identity mints through `ContentHash.Of`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MTConnect.NET-Common`
- package: `MTConnect.NET-Common` (MIT)
- assembly: `MTConnect.NET-Common`
- namespace: `MTConnect.Assets.CuttingTools`, `.CuttingTools.Measurements`, `MTConnect.Assets`, `MTConnect`
- asset: pure-managed AnyCPU IL, ALC-safe, no native asset; the `net9.0` TFM binds under the `net10.0` consumer
- rail: fabrication — the `Tooling/magazine` tool-data model, the typed cutting-tool half of the `ToolAssembly`/`Magazine` catalogue

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cutting-tool asset graph (`MTConnect.Assets.CuttingTools`)

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]   | [CAPABILITY]             |
| :-----: | :------------------------------ | :-------------- | :----------------------- |
|  [01]   | `CuttingToolAsset`              | tool asset      | physical tool model      |
|  [02]   | `CuttingToolLifeCycle`          | tool state      | operational lifecycle    |
|  [03]   | `CuttingItem`                   | insert or edge  | item-level state         |
|  [04]   | `CuttingToolDefinition`         | tool definition | format-tagged payload    |
|  [05]   | `CuttingToolArchetypeAsset`     | archetype       | shared tool template     |
|  [06]   | `CuttingToolArchetypeReference` | archetype       | instance template link   |
|  [07]   | `ToolLife`                      | life budget     | tool-life accumulator    |
|  [08]   | `ItemLife`                      | life budget     | insert-life accumulator  |
|  [09]   | `Location`                      | magazine slot   | physical slot address    |
|  [10]   | `ProcessFeedRate`               | process range   | feed-rate envelope       |
|  [11]   | `ProcessSpindleSpeed`           | process range   | spindle-speed envelope   |
|  [12]   | `ReconditionCount`              | counter         | recondition state        |
|  [13]   | `Measurement`                   | measurement     | common measurement state |
|  [14]   | `ToolingMeasurement`            | measurement     | ISO-13399 measurement    |

[CuttingToolAsset]: `ToolId` `SerialNumber` `CuttingToolLifeCycle` `CuttingToolDefinition` `CuttingToolArchetypeReference` `IsValid(Version)` `GenerateHash`
[CuttingToolLifeCycle]: `CutterStatus` `CuttingItems` `Location` `Measurements` `ProcessFeedRate` `ProcessSpindleSpeed` `ProgramToolNumber` `ProgramToolGroup` `ReconditionCount` `ToolLife` `ConnectionCodeMachineSide`
[CuttingItem]: `ItemId` `Indices` `Grade` `Manufacturers` `CutterStatus` `ItemLife` `Measurements` `Locus` `ProgramToolGroup`
[CuttingToolDefinition]: `Format : FormatType` `Value : string`
[ToolLife] · [ItemLife]: `Type : ToolLifeType` `Value` `Initial` `Limit` `Warning` `CountDirection`
[Location]: `Type : LocationType` `ToolMagazine` `ToolBar` `ToolRack` `Turret` `AutomaticToolChanger` `PositiveOverlap` `NegativeOverlap` `Value`
[ProcessFeedRate] · [ProcessSpindleSpeed]: `Minimum` `Maximum` `Nominal` `Value`
[ReconditionCount]: `Value` `MaximumCount`
[Measurement]: `Type` `Code` `Value` `Minimum` `Maximum` `Nominal` `Units` `NativeUnits` `SignificantDigits`

[PUBLIC_TYPE_SCOPE]: ISO-13399 measurement subtypes (`MTConnect.Assets.CuttingTools.Measurements`)
- each subtype `: ToolingMeasurement` fixes its ISO-13399 `TypeId`/`CodeId` and takes a `(double value)` ctor; the set is closed and named, never stringly-typed.

| [INDEX] | [SYMBOL]                            | [CODE] | [CAPABILITY]            |
| :-----: | :---------------------------------- | :----- | :---------------------- |
|  [01]   | `CuttingDiameterMeasurement`        | `DCx`  | effective diameter      |
|  [02]   | `CuttingDiameterMaxMeasurement`     | `DC`   | maximum diameter        |
|  [03]   | `CornerRadiusMeasurement`           | `RE`   | insert corner radius    |
|  [04]   | `CuttingEdgeLengthMeasurement`      | `L`    | cutting-edge length     |
|  [05]   | `UsableLengthMaxMeasurement`        | `LUX`  | usable cutting length   |
|  [06]   | `FunctionalLengthMeasurement`       | `LF`   | functional length       |
|  [07]   | `FunctionalWidthMeasurement`        | `WF`   | functional width        |
|  [08]   | `OverallToolLengthMeasurement`      | `OAL`  | overall tool length     |
|  [09]   | `ShankDiameterMeasurement`          | `DMM`  | shank diameter          |
|  [10]   | `ShankLengthMeasurement`            | `LS`   | shank length            |
|  [11]   | `ShankHeightMeasurement`            | `H`    | shank height            |
|  [12]   | `ToolCuttingEdgeAngleMeasurement`   | `KAPR` | cutting-edge angle      |
|  [13]   | `ToolLeadAngleMeasurement`          | `PSIR` | tool lead angle         |
|  [14]   | `PointAngleMeasurement`             | `SIG`  | point angle             |
|  [15]   | `DriveAngleMeasurement`             | `DRVA` | drive angle             |
|  [16]   | `BodyLengthMaxMeasurement`          | `LBX`  | maximum body length     |
|  [17]   | `BodyDiameterMaxMeasurement`        | `BDX`  | maximum body diameter   |
|  [18]   | `DepthOfCutMaxMeasurement`          | `APMX` | maximum cut depth       |
|  [19]   | `IncribedCircleDiameterMeasurement` | `IC`   | insert circle diameter  |
|  [20]   | `InsertWidthMeasurement`            | `W1`   | insert width            |
|  [21]   | `WiperEdgeLengthMeasurement`        | `BS`   | wiper-edge length       |
|  [22]   | `WeightMeasurement`                 | `WT`   | tool mass               |
|  [23]   | `ProtrudingLengthMeasurement`       | `LPR`  | protruding length       |
|  [24]   | `FlangeDiameterMeasurement`         | `DF`   | flange diameter         |
|  [25]   | `FlangeDiameterMaxMeasurement`      | `DF`   | maximum flange diameter |
|  [26]   | `ChamferWidthMeasurement`           | `CHW`  | chamfer width           |
|  [27]   | `ChamferFlatLengthMeasurement`      | `BCH`  | chamfer-flat length     |
|  [28]   | `CuttingHeightMeasurement`          | `HF`   | cutting height          |
|  [29]   | `StepDiameterLengthMeasurement`     | `SDLx` | step-diameter length    |
|  [30]   | `StepIncludedAngleMeasurement`      | `STAx` | step included angle     |
|  [31]   | `CuttingReferencePointMeasurement`  | `CRP`  | cutting reference point |
|  [32]   | `ToolOrientationMeasurement`        | `N/A`  | tool orientation        |

[PUBLIC_TYPE_SCOPE]: asset base, status, and enums (`MTConnect.Assets`, `.CuttingTools`)

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]          |
| :-----: | :---------------------- | :---------------- | :-------------------- |
|  [01]   | `Asset`                 | asset base        | cutting-tool envelope |
|  [02]   | `IAsset`                | interface         | asset contract        |
|  [03]   | `AssetValidationResult` | validation result | conformance outcome   |
|  [04]   | `CutterStatusType`      | enum              | lifecycle state       |
|  [05]   | `ToolLifeType`          | enum              | life basis            |
|  [06]   | `CountDirectionType`    | enum              | counting direction    |
|  [07]   | `LocationType`          | enum              | magazine address kind |
|  [08]   | `MTConnectVersions`     | constants         | schema versions       |

[Asset]: `AssetId` `Type` `InstanceId` `Timestamp` `DeviceUuid` `SerialNumber` `Station` `Model` `Manufacturers` `Hash` `Removed` `Configuration`
[AssetValidationResult]: `bool IsValid` `string Message`; ctor `(bool isValid, string message = null)`
[CutterStatusType]: `NEW` `AVAILABLE` `USED` `MEASURED` `RECONDITIONED` `EXPIRED` `BROKEN` `ALLOCATED` `UNALLOCATED` `NOT_REGISTERED` `UNAVAILABLE` `UNKNOWN`
[ToolLifeType]: `MINUTES` `PART_COUNT` `WEAR`
[CountDirectionType]: `UP` counts toward `Limit`, `DOWN` counts toward zero
[LocationType]: `POT` `STATION` `SPINDLE` `CRIB` `END_EFFECTOR` `EXPIRED_POT` `REMOVAL_POT` `RETURN_POT` `STAGING_POT` `TRANSFER_POT` — carried on `Location.Type`
[MTConnectVersions]: static `Version` constants `Version10`…`Version25` and `Max`; the cutting-tool `Admit` boundary validates against `Version24`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cutting-tool authoring and read
- model types are mutable POCOs behind `I…` contracts: author by setting the lifecycle, items, and measurements; `Process()` normalizes a partial lifecycle or item.

| [INDEX] | [SURFACE]                            | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :----------------------------------- | :------- | :------------------------------- |
|  [01]   | `CuttingToolAsset`                   | ctor     | author tool asset                |
|  [02]   | `CuttingToolLifeCycle`               | ctor     | author operational state         |
|  [03]   | `CuttingItem`                        | ctor     | author insert or edge            |
|  [04]   | `CornerRadiusMeasurement(double)`    | ctor     | author corner-radius geometry    |
|  [05]   | `CuttingDiameterMeasurement(double)` | ctor     | author cutting-diameter geometry |
|  [06]   | `Location`                           | ctor     | author magazine slot             |
|  [07]   | `ToolLife`                           | ctor     | author tool-life budget          |
|  [08]   | `lifeCycle.Process()`                | instance | normalize lifecycle defaults     |
|  [09]   | `cuttingItem.Process()`              | instance | normalize item defaults          |

[ENTRYPOINT_SCOPE]: content identity and validation
- every asset and sub-component computes a deterministic `GenerateHash` structural digest and validates against an MTConnect schema version; the digest is boundary evidence, `ContentHash.Of` mints catalogue identity.

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]            |
| :-----: | :----------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `cuttingToolAsset.GenerateHash(bool)`                        | instance | asset structural digest |
|  [02]   | `CuttingToolAsset.GenerateHash(CuttingToolAsset, bool)`      | static   | static asset digest     |
|  [03]   | `CuttingToolLifeCycle.GenerateHash(ICuttingToolLifeCycle)`   | static   | lifecycle digest        |
|  [04]   | `CuttingItem.GenerateHash(ICuttingItem)`                     | static   | item digest             |
|  [05]   | `ToolLife.GenerateHash(IToolLife)`                           | static   | life digest             |
|  [06]   | `cuttingToolAsset.IsValid(Version) -> AssetValidationResult` | instance | schema conformance      |
|  [07]   | `asset.Hash`                                                 | property | stamped hash            |
|  [08]   | `asset.InstanceId`                                           | property | stamped instance        |
|  [09]   | `asset.Timestamp`                                            | property | stamped time            |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `CuttingToolAsset : Asset` is the physical tool: `ToolId` (program tool-number space), `SerialNumber` (instance), one `CuttingToolLifeCycle`, optionally a `CuttingToolDefinition` (ISO-13399 definition body) and a `CuttingToolArchetypeReference` (shared template)
- `CuttingToolLifeCycle` carries operational state: `CutterStatus` (a SET of `CutterStatusType`, simultaneous `AVAILABLE`+`MEASURED`), `CuttingItems`, `Location`, body-level `Measurements`, the `ProcessFeedRate`/`ProcessSpindleSpeed` envelopes, `ToolLife` budget, and `ProgramToolNumber`/`ProgramToolGroup` NC binding
- `CuttingItem` is one insert/edge with its own `Indices`, `Grade`, `ItemLife`, and edge-level `Measurements`; a multi-insert body holds several
- every measurement is a typed `Measurements.*` subtype `: ToolingMeasurement` fixing `TypeId`/`CodeId` and carrying `Value` with `Minimum`/`Maximum`/`Nominal`/`Units`/`NativeUnits`/`SignificantDigits` — `CornerRadiusMeasurement(2.0)` is the corner radius, not a `Measurement` with a stringly-set `Type`/`Code`
- domain code binds the `I…` interfaces (`ICuttingToolAsset`/`ICuttingToolLifeCycle`/`ICuttingItem`/`IToolingMeasurement`); the concrete classes are the mutable authoring shapes
- `MTConnect.NET-Common` ships the full MTConnect information model and the in-process `MTConnectAgent` buffer; this folder consumes the `MTConnect.Assets.CuttingTools` slice and the `Asset` base alone
- `ProcessFeedRate`/`ProcessSpindleSpeed` and `Measurements.*` are the typed container for feeds/speeds and geometry; the package ships no numeric dataset, so a machining-data source populates the `Nominal`/`Value` fields

[STACKING]:
- `ContentHash.Of` (kernel content mint): `CuttingToolAsset.GenerateHash(includeTimestamp: false)` yields the stable structural digest; the durable catalogue key mints through the seed-zero federation entry over `XxHash128.HashToUInt128`, shared with the `Remnant`/`Stock` lineage — component `GenerateHash` and raw `System.IO.Hashing` never mint identity
- `UnitsNet` (`libs/csharp/.api/api-unitsnet.md`): the `Measurements.*` bare `double` with a `Units`/`NativeUnits` string coerces to typed `Length`/`Angle` through `UnitParser` at the `ToolAssembly` boundary, so holder geometry and swept-volume clearance are dimensioned
- `Tooling/magazine` `Schedule` → `Posting/program`: the `Location` magazine address and `ProgramToolNumber`/`ProgramToolGroup` key the minimal-swap schedule to the `G43`/`M6` emission — the posted tool number IS the asset's `ProgramToolNumber`
- Persistence artifact index: a `CuttingToolAsset` and its `ContentHash.Of` key land as a content-addressed durable tool-catalogue row alongside the `CutProgram` AST
- within-lib: the `Tooling/magazine` `ToolAssembly` composes `CuttingToolAsset` as its tool-data model; the toolpath generator reads `ToolLife` remaining life against `Limit` for a mid-program tool change, and reads the `CuttingToolDefinition` `Format`-tagged `Value` through the structured `Measurements`/`CuttingItems`, never re-parsing the raw string

[LOCAL_ADMISSION]:
- a typed cutting-geometry measurement is the named `Measurements.*` subtype, never a `Measurement` with a stringly-set `Type`/`Code`
- `ToolLife { Type, Limit, Warning, Value, CountDirection }` is the tool-life budget, `ToolLifeType` selecting minutes/part-count/wear
- `Magazine` slot mapping reads `Location` (`ToolMagazine`/`Turret`/`POT` with overlap) so the tool-change schedule keys on the real magazine address, not an ad-hoc int

[RAIL_LAW]:
- Package: `MTConnect.NET-Common` (MIT)
- Owns: the ISO-13399 cutting-tool asset MODEL — the asset/lifecycle/item graph, the typed `Measurements.*` ISO-13399 geometry family, `ToolLife`/`ItemLife`, `Location`, `ProcessFeedRate`/`ProcessSpindleSpeed`, `ReconditionCount`, `CutterStatusType`, the `Asset` base, and the `GenerateHash` structural digest with `IsValid` schema validation
- Accept: a `CuttingToolAsset` authored through the lifecycle/items/measurements POCOs (bound by `I…` interface), the typed `Measurements.*` subtypes, the `Location`, the `ToolLife` budget, and `GenerateHash` feeding `ContentHash.Of`
- Reject: a stringly-typed `Measurement` where a named subtype exists; the devices/observations/streams/agent machinery; the XML/JSON wire serializer or any network transport (separate `MTConnect.NET-XML`/`-JSON`/`-HTTP`/`-MQTT`/`-SHDR`, not admitted); a bare `double` cutting dimension bypassing `UnitsNet` at the `ToolAssembly` boundary; a hand-rolled tool-data model beside `CuttingToolAsset`
