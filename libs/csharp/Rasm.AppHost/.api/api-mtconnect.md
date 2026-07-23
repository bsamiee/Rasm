# [RASM_APPHOST_API_MTCONNECT]

`MTConnect.NET-Common` (TrakHound) owns the machine-tool connectivity model slice: the observation/device/asset/streams object graph, the `ResponseDocumentFormatter` parse of an agent `/current`/`/sample` document, the `MTConnectAdapter` SHDR relay, and `MTConnectClientInformation` incremental-poll cursor state. AppHost binds it behind the one `TransportRow` adapter through the `mtconnect` live-wire row, decoding the fetched document to `ExternalValue` at the boundary while the HTTP/MQTT transport firewalls to the `OutboundHop`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MTConnect.NET-Common`
- package: `MTConnect.NET-Common` (MIT)
- assembly: `MTConnect.NET-Common`
- namespace: `MTConnect.Adapters`, `MTConnect.Input`, `MTConnect.Observations`, `MTConnect.Streams`, `MTConnect.Devices`, `MTConnect.Assets.CuttingTools`, `MTConnect.Clients`, `MTConnect.Formatters`
- target: `net9.0` (multi-tfm to `netstandard2.0`); the `net10.0` consumer binds `net9.0`
- asset: runtime library
- rail: live-wire

## [02]-[PUBLIC_TYPES]

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

`MTConnectClientInformation` carries `DeviceKey`, `InstanceId`, `LastSequence`, and `ChangeToken` as durable incremental-poll cursor state.

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]     | [CAPABILITY]            |
| :-----: | :----------------------------------------------- | :---------------- | :---------------------- |
|  [01]   | `MTConnect.Streams.StreamsResponseDocument`      | response document | agent response          |
|  [02]   | `MTConnect.Streams.DeviceStream`                 | stream node       | device grouping         |
|  [03]   | `MTConnect.Streams.ComponentStream`              | stream node       | component grouping      |
|  [04]   | `MTConnect.Observations.Observation`             | observation       | decoded data-item value |
|  [05]   | `MTConnect.Formatters.ResponseDocumentFormatter` | formatter         | XML/JSON parser         |
|  [06]   | `MTConnect.Clients.MTConnectClientInformation`   | poll cursor       | incremental state       |
|  [07]   | `MTConnect.Assets.CuttingTools.CuttingToolAsset` | asset             | cutting-tool model      |

## [03]-[ENTRYPOINTS]

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

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------ |
|  [01]   | `ResponseDocumentFormatter.CreateStreamsResponseDocument(string, Stream)`       | static   | parse an agent document         |
|  [02]   | `MTConnectClientInformation.Read(string, string) -> MTConnectClientInformation` | static   | restore the poll cursor         |
|  [03]   | `MTConnectClientInformation.Save(string)`                                       | instance | persist `LastSequence` on drain |
|  [04]   | `IObservationInput.GetValue(string) -> string`                                  | instance | extract one named value         |

- `ResponseDocumentFormatter.CreateStreamsResponseDocument`: returns `FormatReadResult<IStreamsResponseDocument>`, the result-wrapped streams graph.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `-Common` owns the observation/device/asset/streams object graph and the `ResponseDocumentFormatter` parse; it decodes the agent document AppHost fetches over the `OutboundHop`, never opening an HTTP or MQTT socket.
- `MTConnectClientInformation` drives the incremental consume path: `InstanceId` and `LastSequence` cursor state, a poll requesting `from=LastSequence+1`, decode advancing and `Save`ing the cursor, so a restart resumes from the committed sequence and an agent `InstanceId` change forces a full re-current — the outbox-watermark durable-cursor discipline.
- One `Observation` decodes to one `ExternalValue` (data-item value, declared unit/type from the device model, good flag from observation quality, source instant from the observation timestamp) at the boundary; the boxed MTConnect model type never enters the interior.
- `MTConnectAdapter` is the SHDR relay case: AppHost re-publishes observations to a downstream agent, `AddObservation`/`SendChanged` buffering and flushing on the SHDR line, a distinct row shape from the consume path sharing the one transport row's binding spec.

[STACKING]:
- within-lib: the `mtconnect` row is one `ExternalTransport` `[SmartEnum<string>]` case with its `TransportRow` (`ReadShape.Poll` over an `OutboundHop.HttpApi` for the `/sample` cursor poll, `Subscribe` for an MQTT-relay agent, `Writable: false` for pure consume) and one `LiveClient` case wrapping the poll-decode-cursor loop, no bespoke poller beyond the `OutboundHop`; the SHDR relay case binds `Writable: true` over the same row.

[LOCAL_ADMISSION]:
- A data-item map (device key, data-item keys, poll interval, sequence cursor) is binding-spec policy data; the per-row retry is the `OutboundHop` breaker, never an MTConnect re-poll loop.
- Fabrication `Tooling/magazine` mid-job tool-life reload decodes `CuttingToolAsset` life/wear observations, and `Verify/probing` binds measured-feature/work-offset observations; both pin the `-Common` model slice and firewall transport to the `OutboundHop`. OPC-UA/umati machine data stays on the `OPCFoundation` runtime, never re-homed here.

[RAIL_LAW]:
- Package: `MTConnect.NET-Common`
- Owns: the MTConnect observation/device/asset/streams model, response-document parse, SHDR adapter relay, and incremental-poll cursor state
- Accept: an agent document fetched over the AppHost `OutboundHop`, decoded to `ExternalValue` at the boundary, with `MTConnectClientInformation` as the durable sequence cursor
- Reject: a bundled HTTP/MQTT transport client, a second MTConnect poller, or the boxed model type crossing into the interior
