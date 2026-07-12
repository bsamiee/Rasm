# [RASM_FABRICATION_API_MTCONNECT_NET_COMMON]

`MTConnect.NET-Common` (TrakHound) is the ISO-13399-aligned MTConnect cutting-tool asset model — the in-memory `MTConnect.Assets.CuttingTools` graph backing the tool-data MODEL half of the `Tooling/magazine` `Magazine`/`ToolAssembly` catalogue. `Rasm.Fabrication` consumes ONLY the cutting-tool asset slice: a `CuttingToolAsset` carrying a `CuttingToolLifeCycle` (cutter status, tool-life budget, process feed/speed envelope, magazine `Location`, the ISO-13399-derived `Measurements` family) plus its `CuttingItem` inserts. The package is the full MTConnect information model (devices, observations, streams, the in-process `MTConnectAgent` buffer); the fabrication folder treats it as the cutting-tool data model and leaves the agent/stream machinery unconsumed. The network TRANSPORT (HTTP/MQTT/SHDR) lives in SEPARATE companion packages and is not admitted. The asset's `GenerateHash` structural digest is boundary evidence; durable catalogue identity mints through `ContentHash.Of`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MTConnect.NET-Common`

- package: `MTConnect.NET-Common`
- license: `MIT` (TrakHound/MTConnect.NET)
- assembly: `MTConnect.NET-Common`
- namespace consumed: `MTConnect.Assets.CuttingTools`, `.CuttingTools.Measurements`, `MTConnect.Assets` (the `Asset` base + `AssetValidationResult`), `MTConnect` (root — the `MTConnectVersions` schema-version constants the `IsValid(Version)` check reads) — the broader `MTConnect.Devices`/`.Observations`/`.Streams`/`.Agents`/`.Configurations` namespaces ship in this same assembly but are OUT of the fabrication scope
- asset: pure-managed AnyCPU IL, multi-target (`net9.0`/`net8.0`/`net7.0`/`net6.0`/`netstandard2.0`/`net46x`/`net47x`/`net48`); the `net10.0` consumer binds `lib/net9.0/MTConnect.NET-Common.dll` (the highest applicable TFM)
- transitive floor: `YamlDotNet`, `System.Text.Json`, `System.Buffers`, and `System.Runtime.InteropServices.RuntimeInformation` are centrally floor-pinned; pure-managed, no native asset, ALC-safe
- scope: the cutting-tool ASSET MODEL only (data shapes + content hash + version validation); NOT the XML/JSON wire serializer (separate `MTConnect.NET-XML`/`-JSON` packages) and NOT any network transport
- rail: fabrication (`Tooling/magazine` tool-data model — the `ToolAssembly`/`Magazine` catalogue's typed cutting-tool half)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: cutting-tool asset graph (`MTConnect.Assets.CuttingTools`)

- rail: fabrication
- note: a `CuttingToolAsset` is the physical tool; its `CuttingToolLifeCycle` carries the in-use state (status, life, location, measurements); a `CuttingItem` is one insert/edge on the tool. Every model type is paired with an `I…` interface (`ICuttingToolAsset`/`ICuttingToolLifeCycle`/`ICuttingItem`/…) — bind to the interface in domain code.

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

[CUTTING_TOOL_ASSET_MEMBERS]:

- `CuttingToolAsset : Asset`: `ToolId`, `SerialNumber`, `CuttingToolLifeCycle`, `CuttingToolDefinition`, `CuttingToolArchetypeReference`, `IsValid(Version)`, and `GenerateHash`.
- `CuttingToolLifeCycle`: `CutterStatus`, `CuttingItems`, `Location`, `Measurements`, `ProcessFeedRate`, `ProcessSpindleSpeed`, `ProgramToolNumber`, `ProgramToolGroup`, `ReconditionCount`, `ToolLife`, and `ConnectionCodeMachineSide`.
- `CuttingItem`: `ItemId`, `Indices`, `Grade`, `Manufacturers`, `CutterStatus`, `ItemLife`, `Measurements`, `Locus`, and `ProgramToolGroup`.
- `CuttingToolDefinition`: `Format : FormatType` selects the payload encoding, and `Value : string` carries its body.
- `ToolLife` and `ItemLife`: `Type : ToolLifeType`, `Value`, `Initial`, `Limit`, `Warning`, and `CountDirection`.
- `Location`: `Type : LocationType`, `ToolMagazine`, `ToolBar`, `ToolRack`, `Turret`, `AutomaticToolChanger`, `PositiveOverlap`, `NegativeOverlap`, and `Value`.
- `ProcessFeedRate` and `ProcessSpindleSpeed`: `Minimum`, `Maximum`, `Nominal`, and `Value`.
- `ReconditionCount`: `Value` and `MaximumCount`.
- `Measurement`: `Type`, `Code`, `Value`, `Minimum`, `Maximum`, `Nominal`, `Units`, `NativeUnits`, and `SignificantDigits`; `ToolingMeasurement` adds the ISO-13399 `Code`.

[PUBLIC_TYPE_SCOPE]: ISO-13399 measurement subtypes (`MTConnect.Assets.CuttingTools.Measurements`)

- rail: fabrication
- note: each subtype `: ToolingMeasurement` fixes its ISO-13399 `TypeId` and two-letter `CodeId` (e.g. `CornerRadius`/`RE`) and takes a `(double value)` ctor — the dimensional cutting-geometry vocabulary the toolpath generators read. The set is closed and named, not stringly-typed.

| [INDEX] | [SYMBOL]                            | [CAPABILITY]            |
| :-----: | :---------------------------------- | :---------------------- |
|  [01]   | `CuttingDiameterMeasurement`        | effective diameter      |
|  [02]   | `CuttingDiameterMaxMeasurement`     | maximum diameter        |
|  [03]   | `CornerRadiusMeasurement`           | insert corner radius    |
|  [04]   | `CuttingEdgeLengthMeasurement`      | cutting-edge length     |
|  [05]   | `UsableLengthMaxMeasurement`        | usable cutting length   |
|  [06]   | `FunctionalLengthMeasurement`       | functional length       |
|  [07]   | `OverallToolLengthMeasurement`      | overall tool length     |
|  [08]   | `ShankDiameterMeasurement`          | shank diameter          |
|  [09]   | `ShankLengthMeasurement`            | shank length            |
|  [10]   | `ShankHeightMeasurement`            | shank height            |
|  [11]   | `ToolCuttingEdgeAngleMeasurement`   | cutting-edge angle      |
|  [12]   | `ToolLeadAngleMeasurement`          | tool lead angle         |
|  [13]   | `PointAngleMeasurement`             | point angle             |
|  [14]   | `DriveAngleMeasurement`             | drive angle             |
|  [15]   | `BodyLengthMaxMeasurement`          | maximum body length     |
|  [16]   | `BodyDiameterMaxMeasurement`        | maximum body diameter   |
|  [17]   | `DepthOfCutMaxMeasurement`          | maximum cut depth       |
|  [18]   | `IncribedCircleDiameterMeasurement` | insert circle diameter  |
|  [19]   | `InsertWidthMeasurement`            | insert width            |
|  [20]   | `WiperEdgeLengthMeasurement`        | wiper-edge length       |
|  [21]   | `WeightMeasurement`                 | tool mass               |
|  [22]   | `ProtrudingLengthMeasurement`       | protruding length       |
|  [23]   | `FlangeDiameterMeasurement`         | flange diameter         |
|  [24]   | `FlangeDiameterMaxMeasurement`      | maximum flange diameter |
|  [25]   | `ChamferWidthMeasurement`           | chamfer width           |
|  [26]   | `ChamferFlatLengthMeasurement`      | chamfer-flat length     |
|  [27]   | `CuttingHeightMeasurement`          | cutting height          |
|  [28]   | `StepDiameterLengthMeasurement`     | step-diameter length    |
|  [29]   | `StepIncludedAngleMeasurement`      | step included angle     |
|  [30]   | `CuttingReferencePointMeasurement`  | cutting reference point |
|  [31]   | `ToolOrientationMeasurement`        | tool orientation        |

[MEASUREMENT_CODES]:

- `CuttingDiameterMeasurement` and `CuttingDiameterMaxMeasurement`: `DCx` and no declared code, respectively.
- `CornerRadiusMeasurement`: `RE`.
- `CuttingEdgeLengthMeasurement` and `UsableLengthMaxMeasurement`: no declared code and `LUX`, respectively.
- `FunctionalLengthMeasurement` and `OverallToolLengthMeasurement`: `LF` and `OAL`, respectively.
- `ToolCuttingEdgeAngleMeasurement`: `KAPR`; the grouped lead, point, and drive-angle measurements carry no declared code.
- `BodyLengthMaxMeasurement`, `BodyDiameterMaxMeasurement`, and `DepthOfCutMaxMeasurement`: `LBX`, no declared code, and `APMX`, respectively.
- `WiperEdgeLengthMeasurement`: `BS`; the grouped insert-circle and insert-width measurements carry no declared code.
- The package spells `IncribedCircleDiameterMeasurement` without the second `s`.

[PUBLIC_TYPE_SCOPE]: asset base, status, and enums (`MTConnect.Assets`, `.CuttingTools`)

- rail: fabrication

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

[ASSET_SUPPORT_DETAILS]:

- `Asset`: `AssetId`, `Type`, `InstanceId`, `Timestamp`, `DeviceUuid`, `SerialNumber`, `Station`, `Model`, `Manufacturers`, `Hash`, `Removed`, and `Configuration`.
- `AssetValidationResult`: the `IsValid(Version)` outcome struct exposes `bool IsValid` and `string Message`; its constructor is `(bool isValid, string message = null)`.
- `CutterStatusType`: `NEW`, `AVAILABLE`, `USED`, `MEASURED`, `RECONDITIONED`, `EXPIRED`, `BROKEN`, `ALLOCATED`, `UNALLOCATED`, `NOT_REGISTERED`, `UNAVAILABLE`, and `UNKNOWN`.
- `ToolLifeType`: `MINUTES`, `PART_COUNT`, and `WEAR`.
- `CountDirectionType`: `UP` counts toward a limit, and `DOWN` counts toward zero.
- `LocationType`: the magazine-location kind carried by `Location.Type`, including `POT`, `STATION`, and `SPINDLE`.
- `MTConnectVersions`: the root `MTConnect` static class exposes `static readonly Version` schema constants from `Version10` through `Version25` plus `static Version Max`; `Version24 = new Version(2, 4)`, and the cutting-tool `Admit` boundary validates against `MTConnectVersions.Version24`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: cutting-tool authoring and read

- rail: fabrication
- note: the model is plain mutable POCOs with interface contracts; author a tool by setting the lifecycle, items, and measurements, read it back through the same properties. `Process()` normalizes a partially-populated lifecycle/item.

| [INDEX] | [SURFACE]                    | [ENTRY_FAMILY] | [CAPABILITY]       |
| :-----: | :--------------------------- | :------------- | :----------------- |
|  [01]   | `CuttingToolAsset`           | author         | tool asset         |
|  [02]   | `CuttingToolLifeCycle`       | author         | operational state  |
|  [03]   | `CuttingItem`                | author         | insert or edge     |
|  [04]   | `CornerRadiusMeasurement`    | author         | corner radius      |
|  [05]   | `CuttingDiameterMeasurement` | author         | cutting diameter   |
|  [06]   | `Location`                   | author         | magazine slot      |
|  [07]   | `ToolLife`                   | author         | tool-life budget   |
|  [08]   | `lifeCycle.Process()`        | normalize      | lifecycle defaults |
|  [09]   | `cuttingItem.Process()`      | normalize      | item defaults      |

[AUTHORING_SIGNATURES]:

- `new CuttingToolAsset { ToolId, SerialNumber, CuttingToolLifeCycle }`.
- `CuttingToolLifeCycle { CutterStatus, CuttingItems, Location, Measurements, ProcessFeedRate, ProcessSpindleSpeed, ToolLife, ProgramToolNumber }`.
- `new CuttingItem { ItemId, Indices, Grade, Measurements, ItemLife }`.
- `new CornerRadiusMeasurement(double value)` and `new CuttingDiameterMeasurement(double value)` represent the constructor shape shared by every `Measurements.*` subtype.
- `new Location { Type, ToolMagazine, Turret, PositiveOverlap }`.
- `new ToolLife { Type = ToolLifeType.MINUTES, Limit, Warning, Value, CountDirection }`.

[ENTRYPOINT_SCOPE]: content identity and validation

- rail: fabrication
- note: every asset and sub-component computes a deterministic content hash and validates against an MTConnect schema version — the content hash is the catalogue-key seam.

Provider digests are boundary evidence only; `ContentHash.Of` mints catalogue identity over canonical asset bytes.

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [CAPABILITY]        |
| :-----: | :---------------------------------- | :------------- | :------------------ |
|  [01]   | `cuttingToolAsset.GenerateHash`     | digest         | asset evidence      |
|  [02]   | `CuttingToolAsset.GenerateHash`     | digest         | static asset digest |
|  [03]   | `CuttingToolLifeCycle.GenerateHash` | digest         | lifecycle evidence  |
|  [04]   | `CuttingItem.GenerateHash`          | digest         | item evidence       |
|  [05]   | `ToolLife.GenerateHash`             | digest         | life evidence       |
|  [06]   | `cuttingToolAsset.IsValid`          | validate       | schema conformance  |
|  [07]   | `asset.Hash`                        | read           | stamped hash        |
|  [08]   | `asset.InstanceId`                  | read           | stamped instance    |
|  [09]   | `asset.Timestamp`                   | read           | stamped time        |

[DIGEST_AND_VALIDATION_SIGNATURES]:

- `cuttingToolAsset.GenerateHash(bool includeTimestamp = true)` and static `CuttingToolAsset.GenerateHash(asset, includeTimestamp)` compute the asset digest.
- `CuttingToolLifeCycle.GenerateHash(lifeCycle)`, `CuttingItem.GenerateHash(item)`, and `ToolLife.GenerateHash(toolLife)` compute component digests that never mint identity.
- `cuttingToolAsset.IsValid(Version mtconnectVersion)` returns `AssetValidationResult`; the boundary reads `.IsValid` and passes `MTConnectVersions.Version24`.

## [04]-[IMPLEMENTATION_LAW]

[MODEL_TOPOLOGY]:

- a `CuttingToolAsset : Asset` is the physical tool: `ToolId` (the program tool number space) + `SerialNumber` (the unique instance) + a `CuttingToolLifeCycle` (the in-use state) + optionally a `CuttingToolDefinition` (the ISO-13399 definition body) and a `CuttingToolArchetypeReference` (the shared template)
- the `CuttingToolLifeCycle` carries the operational state: `CutterStatus` (a SET of `CutterStatusType` with simultaneous `AVAILABLE` and `MEASURED` states), `CuttingItems` (the inserts), `Location` (the magazine slot), `Measurements` (the body-level ISO-13399 geometry), the `ProcessFeedRate`/`ProcessSpindleSpeed` declared envelopes, `ToolLife` (the consumed/limit budget), and `ProgramToolNumber`/`ProgramToolGroup` (the NC program binding)
- a `CuttingItem` is one insert/edge with its own `Indices`, `Grade`, `ItemLife`, and `Measurements` (the edge-level geometry) — a multi-insert tool body holds several
- every measurement is a typed `Measurements.*` subtype `: ToolingMeasurement` fixing its ISO-13399 `TypeId`/`CodeId` and carrying `Value` plus `Minimum`/`Maximum`/`Nominal`/`Units`/`NativeUnits`/`SignificantDigits` — a `CornerRadiusMeasurement(2.0)` is the corner radius, NOT a stringly-typed `{ "type": "RE", "value": 2.0 }`
- bind domain code to the `I…` interfaces (`ICuttingToolAsset`/`ICuttingToolLifeCycle`/`ICuttingItem`/`IToolingMeasurement`) — the concrete classes are the mutable authoring shapes

[SCOPE_BOUNDARY]:

- this assembly is the full MTConnect information model AND the in-process `MTConnect.Agents.MTConnectAgent` buffer; the fabrication folder consumes ONLY the `MTConnect.Assets.CuttingTools` slice and the `Asset` base — the devices/observations/streams/agent machinery is present but UNCONSUMED here
- the XML/JSON wire (de)serialization is NOT in this package — `MTConnect.NET-Common` is the model + the `GenerateHash` structural digest + `IsValid` schema validation; round-tripping a `CuttingToolAsset` to the MTConnect XML/JSON wire requires the separate `MTConnect.NET-XML`/`-JSON` packages (not admitted — the fabrication rail holds the model in-memory, not over a wire)
- the network TRANSPORT (HTTP agent, MQTT broker, SHDR adapter) lives in `MTConnect.NET-HTTP`/`-MQTT`/`-SHDR` — none admitted; the fabrication folder is not an MTConnect endpoint, it consumes the asset MODEL shape
- the `ProcessFeedRate`/`ProcessSpindleSpeed` and the `Measurements.*` geometry are the SHAPE for feeds/speeds and cutting geometry — the package carries no published numeric feeds/speeds DATASET (the recorded gap), so a real machining-data source must populate the `Nominal`/`Value` fields; the model is the typed container, not the data

[LOCAL_ADMISSION]:

- the `Tooling/magazine` `ToolAssembly` composes a `CuttingToolAsset` as its tool-data model — the holder geometry the `Toolpath/guard` swept-volume reads is the in-folder geometry, the cutting parameters (diameter, corner radius, flute/edge length, feed/speed envelope, tool-life budget) are the `CuttingToolLifeCycle` measurements/process fields
- the `Magazine` slot map reads the `Location` (`ToolMagazine`/`Turret`/`POT` + overlap) so the minimal-swap tool-change `Schedule` the posting `G43`/`M6` emits is keyed on the real magazine address, not an ad-hoc int
- a typed cutting-geometry measurement is the named `Measurements.*` subtype (`CuttingDiameterMeasurement`, `CornerRadiusMeasurement`, …), never a `Measurement` with a stringly-set `Type`/`Code`
- the tool-life budget is `ToolLife { Type, Limit, Warning, Value, CountDirection }` — the toolpath generator reads the remaining life against the `Limit` to decide a mid-program tool change, the `ToolLifeType` selecting minutes/part-count/wear

[INTEGRATION_STACK]:

- content identity seam: `CuttingToolAsset.GenerateHash(includeTimestamp: false)` yields the stable structural digest of a tool definition.
- federation seam: the structural digest is never a second mint; the durable content key mints through the kernel `ContentHash.Of` seed-zero federation entry, which reaches `XxHash128.HashToUInt128` through itself.
- second-hasher defect: the cutting-tool catalogue and the `Remnant`/`Stock` lineage share that single federation entry over their own canonical digests; raw `System.IO.Hashing` or raw `GenerateHash` identity mints are forbidden.
- quantity seam: the `Measurements.*` values are bare `double` + a `Units`/`NativeUnits` string — at the `ToolAssembly` boundary they coerce to `UnitsNet` typed `Length`/`Angle` (`api-unitsnet`) so the holder geometry and the swept-volume clearance are dimensioned, never a raw `double` mm/inch ambiguity; the `NativeUnits` string drives the `UnitParser` coercion
- magazine/posting seam: the `Location` magazine address and the `ProgramToolNumber`/`ProgramToolGroup` bind the `Tooling/magazine` `Schedule` to the `Posting/program` `G43` (tool-length offset) / `M6` (tool change) emission — the tool number in the posted G-code IS the asset's `ProgramToolNumber`
- persistence: a `CuttingToolAsset` and its `ContentHash.Of` content key flow to the Persistence artifact index as a content-addressed durable tool-catalogue row alongside the `CutProgram` AST; the tool library is a content-keyed artifact-index table and the asset key is its row key
- definition body: the `CuttingToolDefinition` carries a `Format`-tagged `Value` body (a vendor tool-library export — its `FormatType` naming the encoding) — the fabrication folder reads the structured `Measurements`/`CuttingItems` rather than re-parsing the raw `Value` string

[RAIL_LAW]:

- Package: `MTConnect.NET-Common` (centrally pinned, MIT, pure-managed highest-applicable AnyCPU IL; transitive floor `YamlDotNet` + `System.Text.Json` + `System.Buffers` + `System.Runtime.InteropServices.RuntimeInformation`)
- Owns: the ISO-13399-aligned MTConnect cutting-tool asset MODEL — `CuttingToolAsset`/`CuttingToolLifeCycle`/`CuttingItem`, the typed `Measurements.*` ISO-13399 geometry family, `ToolLife`/`ItemLife`, `Location`, `ProcessFeedRate`/`ProcessSpindleSpeed`, `ReconditionCount`, `CutterStatusType`, the `Asset` base, and the `GenerateHash` structural digest + `IsValid` schema validation
- Accept: a `CuttingToolAsset` authored through the lifecycle/items/measurements POCOs (bound by `I…` interface), the typed `Measurements.*` subtypes for cutting geometry, the `Location` for the magazine address, the `ToolLife` budget, and the `GenerateHash` structural digest feeding `ContentHash.Of` at the catalogue/persistence seam
- Reject: a stringly-typed `Measurement` where a named `Measurements.*` subtype exists; consuming the devices/observations/streams/agent machinery (out of the fabrication scope); seeking the XML/JSON wire serializer or any network transport from this package (separate companion packages, not admitted); a bare `double` cutting dimension that bypasses the `UnitsNet` coercion at the `ToolAssembly` boundary; hand-rolling a tool-data model beside `CuttingToolAsset`
