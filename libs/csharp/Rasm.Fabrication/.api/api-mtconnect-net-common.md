# [RASM_FABRICATION_API_MTCONNECT_NET_COMMON]

`MTConnect.NET-Common` (TrakHound) is the ISO-13399-aligned MTConnect cutting-tool asset model — the in-memory `MTConnect.Assets.CuttingTools` graph backing the tool-data MODEL half of the `Process/magazine` `Magazine`/`ToolAssembly` catalogue. `Rasm.Fabrication` consumes ONLY the cutting-tool asset slice: a `CuttingToolAsset` carrying a `CuttingToolLifeCycle` (cutter status, tool-life budget, process feed/speed envelope, magazine `Location`, the ISO-13399-derived `Measurements` family) plus its `CuttingItem` inserts. The package is the full MTConnect information model (devices, observations, streams, the in-process `MTConnectAgent` buffer); the fabrication folder treats it as the cutting-tool data model and leaves the agent/stream machinery unconsumed. The network TRANSPORT (HTTP/MQTT/SHDR) lives in SEPARATE companion packages and is not admitted. The asset's `GenerateHash` content identity meets the in-folder `System.IO.Hashing` `XxHash128` keying at the catalogue seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MTConnect.NET-Common`
- package: `MTConnect.NET-Common`
- version: `6.9.0.2` (centrally pinned)
- license: `MIT` (TrakHound/MTConnect.NET)
- assembly: `MTConnect.NET-Common`
- namespace consumed: `MTConnect.Assets.CuttingTools`, `.CuttingTools.Measurements`, `MTConnect.Assets` (the `Asset` base + `AssetValidationResult`), `MTConnect` (root — the `MTConnectVersions` schema-version constants the `IsValid(Version)` check reads) — the broader `MTConnect.Devices`/`.Observations`/`.Streams`/`.Agents`/`.Configurations` namespaces ship in this same assembly but are OUT of the fabrication scope
- asset: pure-managed AnyCPU IL, multi-target (`net9.0`/`net8.0`/`net7.0`/`net6.0`/`netstandard2.0`/`net46x`/`net47x`/`net48`); the `net10.0` consumer binds `lib/net9.0/MTConnect.NET-Common.dll` (the highest applicable TFM)
- transitive floor: `YamlDotNet` >=13.7.1 (declared floor; central row resolves 18.1.0), `System.Text.Json` 8.0.5, `System.Buffers` 4.5.1, `System.Runtime.InteropServices.RuntimeInformation` 4.3.0 — all centrally floor-pinned; pure-managed, no native asset, ALC-safe
- scope: the cutting-tool ASSET MODEL only (data shapes + content hash + version validation); NOT the XML/JSON wire serializer (separate `MTConnect.NET-XML`/`-JSON` packages) and NOT any network transport
- rail: fabrication (`Process/magazine` tool-data model — the `ToolAssembly`/`Magazine` catalogue's typed cutting-tool half)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cutting-tool asset graph (`MTConnect.Assets.CuttingTools`)
- rail: fabrication
- note: a `CuttingToolAsset` is the physical tool; its `CuttingToolLifeCycle` carries the in-use state (status, life, location, measurements); a `CuttingItem` is one insert/edge on the tool. Every model type is paired with an `I…` interface (`ICuttingToolAsset`/`ICuttingToolLifeCycle`/`ICuttingItem`/…) — bind to the interface in domain code.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]   | [CAPABILITY]                                                              |
| :-----: | :----------------------------- | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `CuttingToolAsset`             | tool asset      | `: Asset` — `ToolId`, `SerialNumber`, `CuttingToolLifeCycle`, `CuttingToolDefinition`, `CuttingToolArchetypeReference`; `IsValid(Version)`, `GenerateHash` |
|  [02]   | `CuttingToolLifeCycle`         | tool state      | `CutterStatus`, `CuttingItems`, `Location`, `Measurements`, `ProcessFeedRate`, `ProcessSpindleSpeed`, `ProgramToolNumber`/`ProgramToolGroup`, `ReconditionCount`, `ToolLife`, `ConnectionCodeMachineSide` |
|  [03]   | `CuttingItem`                  | insert/edge     | `ItemId`, `Indices`, `Grade`, `Manufacturers`, `CutterStatus`, `ItemLife`, `Measurements`, `Locus`, `ProgramToolGroup` |
|  [04]   | `CuttingToolDefinition`        | tool definition | `Format : FormatType` + `Value : string` — a format-tagged definition payload (the `Format` selects the encoding, `Value` carries the body) attached to the tool |
|  [05]   | `CuttingToolArchetypeAsset` / `CuttingToolArchetypeReference` | archetype | the shared tool-archetype asset and the per-instance reference to it (the template/instance split) |
|  [06]   | `ToolLife` / `ItemLife`        | life budget     | `Type` (`ToolLifeType`), `Value`, `Initial`/`Limit`/`Warning`, `CountDirection` — the tool/insert life accumulator |
|  [07]   | `Location`                     | magazine slot   | `Type` (`LocationType`), `ToolMagazine`/`ToolBar`/`ToolRack`/`Turret`/`AutomaticToolChanger`, `PositiveOverlap`/`NegativeOverlap`, `Value` — the physical slot address |
|  [08]   | `ProcessFeedRate` / `ProcessSpindleSpeed` | process envelope | `Minimum`/`Maximum`/`Nominal`/`Value` — the recommended feed-rate and spindle-speed RANGE for the tool |
|  [09]   | `ReconditionCount`             | recondition     | `Value`, `MaximumCount` — the regrind/recondition counter |
|  [10]   | `Measurement` / `ToolingMeasurement` | measurement base | `Type`, `Code`, `Value`, `Minimum`/`Maximum`/`Nominal`, `Units`/`NativeUnits`, `SignificantDigits` — the base measurement (a `ToolingMeasurement` adds the ISO-13399 `Code`) |

[PUBLIC_TYPE_SCOPE]: ISO-13399 measurement subtypes (`MTConnect.Assets.CuttingTools.Measurements`)
- rail: fabrication
- note: each subtype `: ToolingMeasurement` fixes its ISO-13399 `TypeId` and two-letter `CodeId` (e.g. `CornerRadius`/`RE`) and takes a `(double value)` ctor — the dimensional cutting-geometry vocabulary the toolpath generators read. The set is closed and named, not stringly-typed.

| [INDEX] | [SYMBOL]                              | [CODE] | [CAPABILITY]                                            |
| :-----: | :------------------------------------ | :----: | :----------------------------------------------------- |
|  [01]   | `CuttingDiameterMeasurement` / `CuttingDiameterMaxMeasurement` | DCx / — | the effective / maximum cutting diameter |
|  [02]   | `CornerRadiusMeasurement`             |   RE   | the insert corner radius                                |
|  [03]   | `CuttingEdgeLengthMeasurement` / `UsableLengthMaxMeasurement` | — / LUX | edge / usable cutting length |
|  [04]   | `FunctionalLengthMeasurement` / `OverallToolLengthMeasurement` | LF / OAL | functional / overall tool length |
|  [05]   | `ShankDiameterMeasurement` / `ShankLengthMeasurement` / `ShankHeightMeasurement` | — | shank diameter / length / height |
|  [06]   | `ToolCuttingEdgeAngleMeasurement` / `ToolLeadAngleMeasurement` / `PointAngleMeasurement` / `DriveAngleMeasurement` | KAPR / — | cutting-edge / lead / point / drive angle |
|  [07]   | `BodyLengthMaxMeasurement` / `BodyDiameterMaxMeasurement` / `DepthOfCutMaxMeasurement` | LBX / — / APMX | body length/diameter, max depth of cut |
|  [08]   | `IncribedCircleDiameterMeasurement` / `InsertWidthMeasurement` / `WiperEdgeLengthMeasurement` | BS (wiper) | insert IC, width, wiper-edge length (the package spells it `Incribed…`) |
|  [09]   | `WeightMeasurement` / `ProtrudingLengthMeasurement` / `FlangeDiameterMeasurement` / `FlangeDiameterMaxMeasurement` | — | tool mass, protruding length, flange diameter |
|  [10]   | `ChamferWidthMeasurement` / `ChamferFlatLengthMeasurement` / `CuttingHeightMeasurement` / `StepDiameterLengthMeasurement` / `StepIncludedAngleMeasurement` / `CuttingReferencePointMeasurement` / `ToolOrientationMeasurement` | — | chamfer, cutting height, step, reference-point, orientation |

[PUBLIC_TYPE_SCOPE]: asset base, status, and enums (`MTConnect.Assets`, `.CuttingTools`)
- rail: fabrication

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]   | [CAPABILITY]                                                              |
| :-----: | :--------------------- | :-------------- | :----------------------------------------------------------------------- |
|  [01]   | `Asset` / `IAsset`     | asset base      | `AssetId`, `Type`, `InstanceId`, `Timestamp`, `DeviceUuid`, `SerialNumber`, `Station`, `Model`, `Manufacturers`, `Hash`, `Removed`, `Configuration` — the MTConnect asset envelope every cutting tool inherits |
|  [02]   | `AssetValidationResult` | validation result | the `IsValid(Version)` outcome `struct` — `bool IsValid` (the schema-conformance verdict the `Admit` boundary reads via `asset.IsValid(version).IsValid`) + `string Message` (the failure detail); ctor `(bool isValid, string message = null)` |
|  [03]   | `CutterStatusType`     | enum            | `NEW`/`AVAILABLE`/`USED`/`MEASURED`/`RECONDITIONED`/`EXPIRED`/`BROKEN`/`ALLOCATED`/`UNALLOCATED`/`NOT_REGISTERED`/`UNAVAILABLE`/`UNKNOWN` — the cutter lifecycle state |
|  [04]   | `ToolLifeType`         | enum            | `MINUTES`/`PART_COUNT`/`WEAR` — the tool-life measurement basis |
|  [05]   | `CountDirectionType`   | enum            | `UP`/`DOWN` — whether life counts up to a limit or down to zero |
|  [06]   | `LocationType`         | enum            | the magazine-location kind (`POT`/`STATION`/`SPINDLE`/…) the `Location.Type` carries |
|  [07]   | `MTConnectVersions`    | version constants | `static class` (root `MTConnect` namespace) of `static readonly Version Version10`…`Version25` (e.g. `Version24 = new Version(2, 4)`) plus `static Version Max` — the `System.Version` schema set `IsValid(Version)` is keyed on; the cutting-tool `Admit` validates against `MTConnectVersions.Version24` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cutting-tool authoring and read
- rail: fabrication
- note: the model is plain mutable POCOs with interface contracts; author a tool by setting the lifecycle, items, and measurements, read it back through the same properties. `Process()` normalizes a partially-populated lifecycle/item.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `new CuttingToolAsset { ToolId, SerialNumber, CuttingToolLifeCycle }` | author      | construct a cutting-tool asset                          |
|  [02]   | `CuttingToolLifeCycle { CutterStatus, CuttingItems, Location, Measurements, ProcessFeedRate, ProcessSpindleSpeed, ToolLife, ProgramToolNumber }` | author | populate the in-use lifecycle |
|  [03]   | `new CuttingItem { ItemId, Indices, Grade, Measurements, ItemLife }` | author     | construct an insert/edge                                |
|  [04]   | `new CornerRadiusMeasurement(double value)` / `new CuttingDiameterMeasurement(double value)` (and every `Measurements.*` subtype) | author | a typed ISO-13399 cutting-geometry measurement |
|  [05]   | `new Location { Type, ToolMagazine, Turret, PositiveOverlap }`     | author         | the physical magazine slot address                      |
|  [06]   | `new ToolLife { Type = ToolLifeType.MINUTES, Limit, Warning, Value, CountDirection }` | author | a tool-life budget |
|  [07]   | `lifeCycle.Process()` / `cuttingItem.Process()`                   | normalize      | normalize a partially-populated lifecycle/item before use |

[ENTRYPOINT_SCOPE]: content identity and validation
- rail: fabrication
- note: every asset and sub-component computes a deterministic content hash and validates against an MTConnect schema version — the content hash is the catalogue-key seam.

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `cuttingToolAsset.GenerateHash(bool includeTimestamp = true)` / static `CuttingToolAsset.GenerateHash(asset, includeTimestamp)` | identity | the deterministic asset content hash |
|  [02]   | `CuttingToolLifeCycle.GenerateHash(lifeCycle)` / `CuttingItem.GenerateHash(item)` / `ToolLife.GenerateHash(toolLife)` | identity | per-component content hashes |
|  [03]   | `cuttingToolAsset.IsValid(Version mtconnectVersion)` → `AssetValidationResult` (read `.IsValid`); the `Version` from `MTConnectVersions.Version24` | validate | schema-version conformance of the authored asset against an MTConnect `MTConnectVersions.*` schema version |
|  [04]   | `asset.Hash` / `asset.InstanceId` / `asset.Timestamp`             | read           | the asset's stamped identity/version/time fields        |

## [04]-[IMPLEMENTATION_LAW]

[MODEL_TOPOLOGY]:
- a `CuttingToolAsset : Asset` is the physical tool: `ToolId` (the program tool number space) + `SerialNumber` (the unique instance) + a `CuttingToolLifeCycle` (the in-use state) + optionally a `CuttingToolDefinition` (the ISO-13399 definition body) and a `CuttingToolArchetypeReference` (the shared template)
- the `CuttingToolLifeCycle` carries the operational state: `CutterStatus` (a SET of `CutterStatusType` — a tool can be both `AVAILABLE` and `MEASURED`), `CuttingItems` (the inserts), `Location` (the magazine slot), `Measurements` (the body-level ISO-13399 geometry), the `ProcessFeedRate`/`ProcessSpindleSpeed` recommended envelopes, `ToolLife` (the consumed/limit budget), and `ProgramToolNumber`/`ProgramToolGroup` (the NC program binding)
- a `CuttingItem` is one insert/edge with its own `Indices`, `Grade`, `ItemLife`, and `Measurements` (the edge-level geometry) — a multi-insert tool body holds several
- every measurement is a typed `Measurements.*` subtype `: ToolingMeasurement` fixing its ISO-13399 `TypeId`/`CodeId` and carrying `Value` plus `Minimum`/`Maximum`/`Nominal`/`Units`/`NativeUnits`/`SignificantDigits` — a `CornerRadiusMeasurement(2.0)` is the corner radius, NOT a stringly-typed `{ "type": "RE", "value": 2.0 }`
- bind domain code to the `I…` interfaces (`ICuttingToolAsset`/`ICuttingToolLifeCycle`/`ICuttingItem`/`IToolingMeasurement`) — the concrete classes are the mutable authoring shapes

[SCOPE_BOUNDARY]:
- this assembly is the full MTConnect information model AND the in-process `MTConnect.Agents.MTConnectAgent` buffer; the fabrication folder consumes ONLY the `MTConnect.Assets.CuttingTools` slice and the `Asset` base — the devices/observations/streams/agent machinery is present but UNCONSUMED here
- the XML/JSON wire (de)serialization is NOT in this package — `MTConnect.NET-Common` is the model + the `GenerateHash` content identity + `IsValid` schema validation; round-tripping a `CuttingToolAsset` to the MTConnect XML/JSON wire requires the separate `MTConnect.NET-XML`/`-JSON` packages (not admitted — the fabrication rail holds the model in-memory, not over a wire)
- the network TRANSPORT (HTTP agent, MQTT broker, SHDR adapter) lives in `MTConnect.NET-HTTP`/`-MQTT`/`-SHDR` — none admitted; the fabrication folder is not an MTConnect endpoint, it consumes the asset MODEL shape
- the `ProcessFeedRate`/`ProcessSpindleSpeed` and the `Measurements.*` geometry are the SHAPE for feeds/speeds and cutting geometry — the package carries no published numeric feeds/speeds DATASET (the recorded gap), so a real machining-data source must populate the `Nominal`/`Value` fields; the model is the typed container, not the data

[LOCAL_ADMISSION]:
- the `Process/magazine` `ToolAssembly` composes a `CuttingToolAsset` as its tool-data model — the holder geometry the `Toolpath/guard` swept-volume reads is the in-folder geometry, the cutting parameters (diameter, corner radius, flute/edge length, feed/speed envelope, tool-life budget) are the `CuttingToolLifeCycle` measurements/process fields
- the `Magazine` slot map reads the `Location` (`ToolMagazine`/`Turret`/`POT` + overlap) so the minimal-swap tool-change `Schedule` the posting `G43`/`M6` emits is keyed on the real magazine address, not an ad-hoc int
- a typed cutting-geometry measurement is the named `Measurements.*` subtype (`CuttingDiameterMeasurement`, `CornerRadiusMeasurement`, …), never a `Measurement` with a stringly-set `Type`/`Code`
- the tool-life budget is `ToolLife { Type, Limit, Warning, Value, CountDirection }` — the toolpath generator reads the remaining life against the `Limit` to decide a mid-program tool change, the `ToolLifeType` selecting minutes/part-count/wear

[INTEGRATION_STACK]:
- content identity seam: `CuttingToolAsset.GenerateHash` produces a deterministic asset hash; the `Nesting` content-identity owner `System.IO.Hashing` `XxHash128` (`api`-side `System.IO.Hashing`) keys the in-folder `Remnant`/`Stock` content address — the cutting-tool catalogue and the stock/remnant lineage share the content-addressing DISCIPLINE (deterministic hash → durable key), each over its own digest; the `GenerateHash(includeTimestamp: false)` form is the stable structural key for a tool definition independent of when it was stamped
- quantity seam: the `Measurements.*` values are bare `double` + a `Units`/`NativeUnits` string — at the `ToolAssembly` boundary they coerce to `UnitsNet` typed `Length`/`Angle` (`api-unitsnet`) so the holder geometry and the swept-volume clearance are dimensioned, never a raw `double` mm/inch ambiguity; the `NativeUnits` string drives the `UnitParser` coercion
- magazine/posting seam: the `Location` magazine address and the `ProgramToolNumber`/`ProgramToolGroup` bind the `Process/magazine` `Schedule` to the `Posting/program` `G43` (tool-length offset) / `M6` (tool change) emission — the tool number in the posted G-code IS the asset's `ProgramToolNumber`
- persistence: a `CuttingToolAsset` and its `GenerateHash` content key flow to `Rasm.Persistence/Schema` as a content-addressed durable tool-catalogue row alongside the `CutProgram` AST — the tool library is a durable content-keyed table, the asset hash its row key
- definition body: the `CuttingToolDefinition` carries a `Format`-tagged `Value` body (a vendor tool-library export — its `FormatType` naming the encoding) — the fabrication folder reads the structured `Measurements`/`CuttingItems` rather than re-parsing the raw `Value` string

[RAIL_LAW]:
- Package: `MTConnect.NET-Common` (6.9.0.2, MIT, pure-managed `lib/net9.0` AnyCPU IL; transitive floor `YamlDotNet` 18.1.0 + `System.Text.Json` 8.0.5 + `System.Buffers` + `System.Runtime.InteropServices.RuntimeInformation`)
- Owns: the ISO-13399-aligned MTConnect cutting-tool asset MODEL — `CuttingToolAsset`/`CuttingToolLifeCycle`/`CuttingItem`, the typed `Measurements.*` ISO-13399 geometry family, `ToolLife`/`ItemLife`, `Location`, `ProcessFeedRate`/`ProcessSpindleSpeed`, `ReconditionCount`, `CutterStatusType`, the `Asset` base, and the `GenerateHash` content identity + `IsValid` schema validation
- Accept: a `CuttingToolAsset` authored through the lifecycle/items/measurements POCOs (bound by `I…` interface), the typed `Measurements.*` subtypes for cutting geometry, the `Location` for the magazine address, the `ToolLife` budget, and the `GenerateHash` content key feeding the catalogue/persistence seam
- Reject: a stringly-typed `Measurement` where a named `Measurements.*` subtype exists; consuming the devices/observations/streams/agent machinery (out of the fabrication scope); seeking the XML/JSON wire serializer or any network transport from this package (separate companion packages, not admitted); a bare `double` cutting dimension that bypasses the `UnitsNet` coercion at the `ToolAssembly` boundary; hand-rolling a tool-data model beside `CuttingToolAsset`
