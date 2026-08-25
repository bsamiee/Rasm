# [DOTNET_API_MTCONNECT]

`MTConnect.NET-Common` (TrakHound) carries two disjoint namespace partitions two folders compose. CONNECTIVITY holds the observation/device/asset/streams graph, the `ResponseDocumentFormatter` agent-document parse, the `MTConnectAdapter` SHDR relay, and the `MTConnectClientInformation` poll cursor; `Rasm.AppHost` binds it behind one `TransportRow` through the `mtconnect` live-wire row. CUTTING-TOOL holds the ISO-13399 `MTConnect.Assets.CuttingTools` graph a `CuttingToolAsset` roots; `Rasm.Fabrication` binds it as the tool-data MODEL half of `Tooling/magazine`. `GenerateHash` digests structure and `ContentHash.Of` mints catalogue identity.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: input and adapter surfaces

Every observation input carries `DeviceKey`, `DataItemKey`, `Timestamp`, and `Values`; `IsUnavailable` marks a dropped point.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]         |
| :-----: | :------------------------------------------- | :------------ | :------------------- |
|  [01]   | `MTConnect.Adapters.MTConnectAdapter`        | SHDR adapter  | buffered relay       |
|  [02]   | `MTConnect.Input.IObservationInput`          | interface     | observation contract |
|  [03]   | `MTConnect.Input.ObservationInput`           | input         | scalar observation   |
|  [04]   | `MTConnect.Input.ConditionObservationInput`  | input         | condition state      |
|  [05]   | `MTConnect.Input.DataSetObservationInput`    | input         | data-set observation |
|  [06]   | `MTConnect.Input.TableObservationInput`      | input         | table observation    |
|  [07]   | `MTConnect.Input.TimeSeriesObservationInput` | input         | time-series values   |
|  [08]   | `MTConnect.Input.AssetInput`                 | input         | asset model          |
|  [09]   | `MTConnect.Input.DeviceInput`                | input         | device model         |

[PUBLIC_TYPE_SCOPE]: streams model, client-state, and asset surfaces

`MTConnectClientInformation` carries `DeviceKey`, `ChangeToken`, and the `long` `InstanceId`/`LastSequence` pair as durable incremental-poll cursor state. Cursor and observation do NOT share a numeric type; the table below spells both sides, so a consumer writes its narrowing rather than inferring one.

| [INDEX] | [SLOT]                                    | [OWNER]                        | [TYPE]  |
| :-----: | :---------------------------------------- | :----------------------------- | :------ |
|  [01]   | `MTConnectClientInformation.InstanceId`   | poll cursor (durable, on disk) | `long`  |
|  [02]   | `MTConnectClientInformation.LastSequence` | poll cursor (durable, on disk) | `long`  |
|  [03]   | `IObservation.InstanceId`                 | observation (agent response)   | `ulong` |
|  [04]   | `IObservation.Sequence`                   | observation (agent response)   | `ulong` |

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]     | [CAPABILITY]              |
| :-----: | :----------------------------------------------- | :---------------- | :------------------------ |
|  [01]   | `MTConnect.Streams.IStreamsResponseDocument`     | response document | parsed agent response     |
|  [02]   | `MTConnect.Streams.StreamsResponseDocument`      | response document | agent response            |
|  [03]   | `MTConnect.Streams.DeviceStream`                 | stream node       | device grouping           |
|  [04]   | `MTConnect.Streams.ComponentStream`              | stream node       | component grouping        |
|  [05]   | `MTConnect.Observations.IObservation`            | interface         | read-side observation     |
|  [06]   | `MTConnect.Observations.Observation`             | observation       | decoded data-item value   |
|  [07]   | `MTConnect.Observations.ObservationValue`        | struct            | one keyed value slot      |
|  [08]   | `MTConnect.Observations.ValueKeys`               | constants         | named value-slot keys     |
|  [09]   | `MTConnect.Observations.Quality`                 | enum              | three-state value quality |
|  [10]   | `MTConnect.Formatters.ResponseDocumentFormatter` | formatter         | XML/JSON parser           |
|  [11]   | `MTConnect.Clients.MTConnectClientInformation`   | poll cursor       | incremental state         |
|  [12]   | `MTConnect.Assets.CuttingTools.CuttingToolAsset` | asset             | cutting-tool model        |

[IStreamsResponseDocument]: `Header : IMTConnectStreamsHeader` `Streams : IEnumerable<IDeviceStream>` `Version` `GetObservations() -> IEnumerable<IObservation>`
[IObservation]: `DeviceUuid` `DataItem : IDataItem` `Category` `Type` `SubType` `DataItemId` `Name` `InstanceId : ulong` `Sequence : ulong` `Timestamp : DateTime` `CompositionId` `Representation` `Quality` `Deprecated` `Extended` `IsUnavailable : bool` `Values : IEnumerable<ObservationValue>` `GetValue(string) -> string` `Validate() -> ValidationResult`
[ObservationValue]: `Key : string` `Value : string` `HasValue()`
[ValueKeys]: `Result` `Level` `ConditionId` `NativeCode` `NativeSeverity` `Qualifier` `Message` `Statistic` `SampleRate` `SampleCount` `Count` `Duration` `AssetType` `DeviceType` `Hash` `ResetTriggered` `TimeSeriesPrefix` `DataSetPrefix` `TablePrefix`
[Quality]: `INVALID` `UNVERIFIABLE` `VALID` — three states, so a good flag is a real three-way read beside `IsUnavailable`, never a bool alias
[IDataItem]: `Id` `Type` `Units` `NativeUnits` `NativeScale : int` `SignificantDigits : int?`

[PUBLIC_TYPE_SCOPE]: cutting-tool asset graph (`MTConnect.Assets.CuttingTools`)

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]   | [CAPABILITY]                     |
| :-----: | :------------------------------ | :-------------- | :------------------------------- |
|  [01]   | `CuttingToolAsset`              | tool asset      | physical tool model              |
|  [02]   | `CuttingToolLifeCycle`          | tool state      | operational lifecycle            |
|  [03]   | `CuttingItem`                   | insert or edge  | item-level state                 |
|  [04]   | `CuttingToolDefinition`         | tool definition | format-tagged payload            |
|  [05]   | `CuttingToolArchetypeAsset`     | archetype       | shared tool template             |
|  [06]   | `CuttingToolArchetypeReference` | archetype       | instance template link           |
|  [07]   | `ToolLife`                      | life budget     | tool-life accumulator            |
|  [08]   | `ItemLife`                      | life budget     | insert-life accumulator          |
|  [09]   | `Location`                      | magazine slot   | physical slot address            |
|  [10]   | `ProcessFeedRate`               | process range   | feed-rate operating envelope     |
|  [11]   | `ProcessSpindleSpeed`           | process range   | spindle-speed operating envelope |
|  [12]   | `ReconditionCount`              | counter         | recondition state                |
|  [13]   | `Measurement`                   | measurement     | common measurement state         |
|  [14]   | `ToolingMeasurement`            | measurement     | ISO-13399 measurement            |

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

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]                  |
| :-----: | :---------------------- | :---------------- | :---------------------------- |
|  [01]   | `Asset`                 | asset base        | cutting-tool asset graph root |
|  [02]   | `IAsset`                | interface         | asset contract                |
|  [03]   | `AssetValidationResult` | validation result | conformance outcome           |
|  [04]   | `CutterStatusType`      | enum              | lifecycle state               |
|  [05]   | `ToolLifeType`          | enum              | life basis                    |
|  [06]   | `CountDirectionType`    | enum              | counting direction            |
|  [07]   | `LocationType`          | enum              | magazine address kind         |
|  [08]   | `MTConnectVersions`     | constants         | schema versions               |

[Asset]: `AssetId` `Type` `InstanceId` `Timestamp` `DeviceUuid` `SerialNumber` `Station` `Model` `Manufacturers` `Hash` `Removed` `Configuration`
[AssetValidationResult]: `bool IsValid` `string Message`; ctor `(bool isValid, string message = null)`
[CutterStatusType]: `NEW` `AVAILABLE` `USED` `MEASURED` `RECONDITIONED` `EXPIRED` `BROKEN` `ALLOCATED` `UNALLOCATED` `NOT_REGISTERED` `UNAVAILABLE` `UNKNOWN`
[ToolLifeType]: `MINUTES` `PART_COUNT` `WEAR`
[CountDirectionType]: `UP` counts toward `Limit`, `DOWN` counts toward zero
[LocationType]: `POT` `STATION` `SPINDLE` `CRIB` `END_EFFECTOR` `EXPIRED_POT` `REMOVAL_POT` `RETURN_POT` `STAGING_POT` `TRANSFER_POT` — carried on `Location.Type`
[MTConnectVersions]: static `Version` constants `Version10`…`Version25` and `Max`; the cutting-tool `Admit` boundary validates against `Version24`

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: SHDR adapter (observation relay)

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                |
| :-----: | :------------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `MTConnectAdapter(int?, bool)`                                       | ctor     | buffered SHDR relay         |
|  [02]   | `MTConnectAdapter.Start()` / `Stop()`                                | instance | open / close the SHDR line  |
|  [03]   | `MTConnectAdapter.AddObservation(string, object, long)`              | instance | buffer a scalar observation |
|  [04]   | `MTConnectAdapter.AddObservation(IObservationInput)`                 | instance | buffer a typed observation  |
|  [05]   | `MTConnectAdapter.AddObservations(IEnumerable<IObservationInput>)`   | instance | buffer an observation batch |
|  [06]   | `MTConnectAdapter.AddAsset(IAssetInput)` / `AddDevice(IDeviceInput)` | instance | buffer asset / device model |
|  [07]   | `MTConnectAdapter.SetUnavailable(long)`                              | instance | mark all points unavailable |
|  [08]   | `MTConnectAdapter.SendChanged()` / `SendBuffer() -> bool`            | instance | flush changed / full buffer |

[ENTRYPOINT_SCOPE]: consume path (poll + decode)

Decode traverses `StreamsResponseDocument` through `DeviceStream` and `ComponentStream` to each `Observation`.

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------- |
|  [01]   | `ResponseDocumentFormatter.CreateStreamsResponseDocument(string, Stream)`       | static   | parse an agent document          |
|  [02]   | `IStreamsResponseDocument.GetObservations() -> IEnumerable<IObservation>`       | instance | flatten every device stream      |
|  [03]   | `IObservation.GetValue(string) -> string`                                       | instance | read one named value slot        |
|  [04]   | `IObservation.Values -> IEnumerable<ObservationValue>`                          | property | every keyed slot on the point    |
|  [05]   | `IObservation.Quality` / `.IsUnavailable`                                       | property | value trust, three-state plus up |
|  [06]   | `IObservation.Timestamp -> DateTime`                                            | property | source instant                   |
|  [07]   | `IObservation.Sequence` / `.InstanceId` (`ulong`)                               | property | poll ordering, agent generation  |
|  [08]   | `IObservation.DeviceUuid` / `.DataItemId`                                       | property | machine and point identity       |
|  [09]   | `IObservation.DataItem.Units`                                                   | property | the point's declared unit        |
|  [10]   | `IObservation.Validate() -> ValidationResult`                                   | instance | refuse a malformed observation   |
|  [11]   | `MTConnectClientInformation.Read(string, string) -> MTConnectClientInformation` | static   | restore the poll cursor          |
|  [12]   | `MTConnectClientInformation.Save(string)`                                       | instance | persist `LastSequence` on drain  |
|  [13]   | `IObservationInput.GetValue(string) -> string`                                  | instance | extract one named value, WRITE   |
|  [14]   | `DocumentFormat.XML` / `.JSON`                                                  | constant | the formatter-id argument        |

- `ResponseDocumentFormatter.CreateStreamsResponseDocument`: returns `FormatReadResult<IStreamsResponseDocument>` (`Content` `Success` `Messages` `Warnings` `Errors` `ResponseDuration`), the result-wrapped streams graph; the formatter id is a `DocumentFormat` constant.
- `GetObservations()` returns null, never an empty sequence, when the document carries no device stream — the ordinary steady-state `/sample` response once the cursor has caught up. Consumers fold that null through an empty arm; an unguarded traversal null-refs on the common path.
- Every observation value crosses as TEXT: `ObservationValue.Value`, `IObservation.GetValue(string)`, and `ISampleValueObservation.Result` are all `string` and no numeric accessor exists anywhere on the graph, so a numeric consumer parses under invariant culture into an optional value rather than assuming a number.
- `MTConnectClientInformation.Read` returns null on any read or deserialize failure and `Save` swallows every IO failure while minting a fresh `ChangeToken`; the default path is `<BaseDirectory>/clients/client.information.<deviceKey>.json`.

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

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `-Common` owns the observation/device/asset/streams object graph and the `ResponseDocumentFormatter` parse; it decodes the agent document AppHost fetches over the `OutboundHop`, never opening an HTTP or MQTT socket.
- `MTConnectClientInformation` drives the incremental consume path: `InstanceId` and `LastSequence` cursor state, a poll requesting `from=LastSequence+1`, decode advancing and `Save`ing the cursor, so a restart resumes from the committed sequence and an agent `InstanceId` change forces a full re-current — the outbox-watermark durable-cursor discipline.
- Cursor state is `long` and observation state `ulong`, so every cursor advance and every re-`current` instance comparison crosses an explicit `(long)` narrowing; a fence omitting it does not compile, and the two owners drift by design rather than by defect.
- One `IObservation` decodes to one `ExternalValue` at the boundary: the value parses invariant-culture out of `GetValue(ValueKeys.Result)` into an optional double because the graph publishes no numeric accessor, the unit reads `DataItem?.Units` with the binding family the fallback, the good flag is `!IsUnavailable && Quality == Quality.VALID` (two of the three quality states stamp not-good), and the source instant is `Timestamp`; the boxed MTConnect model type never enters the interior.
- `MTConnectAdapter` is the SHDR relay case: AppHost re-publishes observations to a downstream agent, `AddObservation`/`SendChanged` buffering and flushing on the SHDR line, a distinct row shape from the consume path sharing the one transport row's binding spec.

- `CuttingToolAsset : Asset` is the physical tool: `ToolId` (program tool-number space), `SerialNumber` (instance), one `CuttingToolLifeCycle`, optionally a `CuttingToolDefinition` (ISO-13399 definition body) and a `CuttingToolArchetypeReference` (shared template)
- `CuttingToolLifeCycle` carries operational state: `CutterStatus` (a SET of `CutterStatusType`, simultaneous `AVAILABLE`+`MEASURED`), `CuttingItems`, `Location`, body-level `Measurements`, the `ProcessFeedRate`/`ProcessSpindleSpeed` operating envelopes, `ToolLife` budget, and `ProgramToolNumber`/`ProgramToolGroup` NC binding
- `CuttingItem` is one insert/edge with its own `Indices`, `Grade`, `ItemLife`, and edge-level `Measurements`; a multi-insert body holds several
- every measurement is a typed `Measurements.*` subtype `: ToolingMeasurement` fixing `TypeId`/`CodeId` and carrying `Value` with `Minimum`/`Maximum`/`Nominal`/`Units`/`NativeUnits`/`SignificantDigits` — `CornerRadiusMeasurement(2.0)` is the corner radius, not a `Measurement` with a stringly-set `Type`/`Code`
- domain code binds the `I…` interfaces (`ICuttingToolAsset`/`ICuttingToolLifeCycle`/`ICuttingItem`/`IToolingMeasurement`); the concrete classes are the mutable authoring shapes
- `MTConnect.NET-Common` ships the full MTConnect information model and the in-process `MTConnectAgent` buffer; this folder consumes the `MTConnect.Assets.CuttingTools` slice and the `Asset` base alone
- `ProcessFeedRate`/`ProcessSpindleSpeed` and `Measurements.*` are the typed container for feeds/speeds and geometry; the package ships no numeric dataset, so a machining-data source populates the `Nominal`/`Value` fields

[STACKING]:
- within-lib: the `mtconnect` row is one `ExternalTransport` `[SmartEnum<string>]` case with its `TransportRow` (`ReadShape.Poll` over an `OutboundHop.HttpApi` for the `/sample` cursor poll, `Subscribe` for an MQTT-relay agent, `Writable: false` for pure consume) and one `LiveClient` case wrapping the poll-decode-cursor loop, no bespoke poller beyond the `OutboundHop`; the SHDR relay case binds `Writable: true` over the same row.

- `ContentHash.Of` (kernel content mint): `CuttingToolAsset.GenerateHash(includeTimestamp: false)` yields the stable structural digest; the durable catalogue key mints through the seed-zero federation entry over `XxHash128.HashToUInt128`, shared with the `Remnant`/`Stock` lineage — component `GenerateHash` and raw `System.IO.Hashing` never mint identity
- `UnitsNet` (`libs/dotnet/.api/api-unitsnet.md`): the `Measurements.*` bare `double` with a `Units`/`NativeUnits` string coerces to typed `Length`/`Angle` through `UnitParser` at the `ToolAssembly` boundary, so holder geometry and swept-volume clearance are dimensioned
- `Tooling/magazine` `Schedule` → `Posting/program`: the `Location` magazine address and `ProgramToolNumber`/`ProgramToolGroup` key the minimal-swap schedule to the `G43`/`M6` emission — the posted tool number IS the asset's `ProgramToolNumber`
- Persistence artifact index: a `CuttingToolAsset` and its `ContentHash.Of` key land as a content-addressed durable tool-catalogue row alongside the `CutProgram` AST
- within-lib: the `Tooling/magazine` `ToolAssembly` composes `CuttingToolAsset` as its tool-data model; the toolpath generator reads `ToolLife` remaining life against `Limit` for a mid-program tool change, and reads the `CuttingToolDefinition` `Format`-tagged `Value` through the structured `Measurements`/`CuttingItems`, never re-parsing the raw string

[LOCAL_ADMISSION]:
- Partitions bind per consuming folder and never cross: `Rasm.AppHost` reaches the connectivity namespaces alone and `Rasm.Fabrication` the `MTConnect.Assets.CuttingTools` slice with the `Asset` base, so a fabrication fence touching the devices/observations/streams/agent machinery, or a live-wire fence authoring a cutting-tool asset, reaches past the surface its own rail admits.
- Data-item maps (device key, data-item keys, poll interval, sequence cursor) carry binding-spec policy data; the per-row retry is the `OutboundHop` breaker, never an MTConnect re-poll loop.
- Fabrication `Tooling/magazine` mid-job tool-life reload decodes `CuttingToolAsset` life/wear observations, and `Verify/probing` binds measured-feature/work-offset observations; both pin the `-Common` model slice and firewall transport to the `OutboundHop`. OPC-UA/umati machine data stays on the `OPCFoundation` runtime, never re-homed here.

- Typed cutting geometry spells its named `Measurements.*` subtype, never a `Measurement` with a stringly-set `Type`/`Code`
- `ToolLife { Type, Limit, Warning, Value, CountDirection }` is the tool-life budget, `ToolLifeType` selecting minutes/part-count/wear
- `Magazine` slot mapping reads `Location` (`ToolMagazine`/`Turret`/`POT` with overlap) so the tool-change schedule keys on the real magazine address, not an ad-hoc int
