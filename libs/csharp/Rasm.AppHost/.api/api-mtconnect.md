# [RASM_APPHOST_API_MTCONNECT]

`MTConnect.NET-Common` (TrakHound) is the model-and-adapter slice of the MTConnect stack: the observation/device/asset/streams object model, the `ResponseDocumentFormatter` that parses an agent's `/current`/`/sample` response document (XML or JSON), the `MTConnectAdapter` SHDR output relay, and `MTConnectClientInformation` incremental-poll cursor state. It is the machine-tool connectivity model the AppHost live-wire `mtconnect` transport row binds behind the one `TransportRow` adapter; the HTTP/MQTT transport itself is firewalled to the AppHost `OutboundHop` (this `-Common` pin carries the model slice ONLY, never a bundled HTTP/MQTT client package).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MTConnect.NET-Common`
- package: `MTConnect.NET-Common`
- license: `MIT`
- assembly: `MTConnect.NET-Common`
- namespace: `MTConnect.Adapters`, `MTConnect.Input`, `MTConnect.Observations`, `MTConnect.Streams`, `MTConnect.Devices`, `MTConnect.Assets.CuttingTools`, `MTConnect.Clients`, `MTConnect.Formatters`
- target: `net9.0` (multi-tfm down to `netstandard2.0`); 1183 types across 34 namespaces; the `net10.0` consumer binds `net9.0`
- dependency floor: `System.Text.Json`, `YamlDotNet` (agent/device config) — the `-Common` slice pulls no HTTP/MQTT transport package
- asset: runtime library
- rail: live-wire

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: input and adapter surfaces
- rail: live-wire

Observation inputs carry `DeviceKey`, `DataItemKey`, and `Timestamp`; scalar inputs add `Values` and `IsUnavailable`.

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
- rail: live-wire

`StreamsResponseDocument` groups observations by `DeviceStream` and then `ComponentStream`.

Each `Observation` carries the data-item value, timestamp, and sequence.

`MTConnectClientInformation` carries `DeviceKey`, `InstanceId`, `LastSequence`, and `ChangeToken` as incremental-poll state.

`CuttingToolAsset` supplies life and geometry to the magazine tool-life consumer.

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
- rail: live-wire

| [INDEX] | [MEMBER]                                                                            | [KIND] | [RETURN]           |
| :-----: | :---------------------------------------------------------------------------------- | :----- | :----------------- |
|  [01]   | `new MTConnectAdapter(int? interval = null, bool bufferEnabled = false)`            | ctor   | `MTConnectAdapter` |
|  [02]   | `MTConnectAdapter.Start()` / `Stop()`                                               | call   | `void`             |
|  [03]   | `MTConnectAdapter.AddObservation(string dataItemKey, object value, long timestamp)` | call   | `void`             |
|  [04]   | `MTConnectAdapter.AddObservation(IObservationInput observation)`                    | call   | `void`             |
|  [05]   | `MTConnectAdapter.AddObservations(IEnumerable<IObservationInput> observations)`     | call   | `void`             |
|  [06]   | `MTConnectAdapter.AddAsset(IAssetInput asset)` / `AddDevice(IDeviceInput device)`   | call   | `void`             |
|  [07]   | `MTConnectAdapter.SetUnavailable(long timestamp = 0)`                               | call   | `void`             |
|  [08]   | `MTConnectAdapter.SendChanged()` / `SendBuffer()`                                   | call   | `bool`             |

[ENTRYPOINT_SCOPE]: consume path (poll + decode)
- rail: live-wire

The decode path traverses `StreamsResponseDocument` through `DeviceStream` and `ComponentStream` to each `Observation`.

`Read` restores the poll cursor, `Save` persists `LastSequence` after a drain, and `GetValue` extracts one named observation value.

| [INDEX] | [MEMBER]                                                                | [KIND]      | [RETURN]                     |
| :-----: | :---------------------------------------------------------------------- | :---------- | :--------------------------- |
|  [01]   | `ResponseDocumentFormatter` parse over the fetched agent document       | call        | `StreamsResponseDocument`    |
|  [02]   | `MTConnectClientInformation.Read(string deviceKey, string path = null)` | static call | `MTConnectClientInformation` |
|  [03]   | `MTConnectClientInformation.Save(string path = null)`                   | call        | `void`                       |
|  [04]   | `IObservationInput.GetValue(string valueKey)`                           | call        | `string`                     |

## [04]-[IMPLEMENTATION_LAW]

[IMPLEMENTATION_LAW]: model semantics
- rail: live-wire

- `-Common` is the MODEL slice: it owns the observation/device/asset/streams object graph and the `ResponseDocumentFormatter` (XML/JSON parse), NOT an HTTP or MQTT client. The AppHost `mtconnect` transport row fetches the agent document over its own `OutboundHop` (HTTP `/sample` poll or MQTT subscribe), and `-Common` decodes the document into observations — the transport is firewalled here per the `-Common` pin.
- the consume path is incremental: `MTConnectClientInformation` carries `InstanceId` + `LastSequence`; a poll requests `from=LastSequence+1` and, on decode, advances the cursor and `Save`s it, so a restart resumes from the committed sequence and an agent `InstanceId` change (agent restart) forces a full re-current — the same durable-cursor discipline the outbox watermark rides.
- one `Observation` decodes to one `ExternalValue` (data-item value, declared unit/type from the device model, good flag from the observation quality, source instant from the observation timestamp) at the boundary; the boxed MTConnect model type never enters the interior.
- the `MTConnectAdapter` (SHDR producer) is the relay case: where AppHost RE-PUBLISHES observations to a downstream MTConnect agent, `AddObservation`/`SendChanged` buffers and flushes on the SHDR line — a distinct row shape from the consume/poll case, sharing the one transport row's binding spec.

[IMPLEMENTATION_LAW]: AppHost usage
- rail: live-wire

- the live-wire `mtconnect` transport row is one `ExternalTransport` `[SmartEnum<string>]` case with its `TransportRow` (`ReadShape.Poll` over an `OutboundHop.HttpApi` for the `/sample` cursor poll, or `Subscribe` for MQTT-relay agents, `Writable: false` for the pure-consume case) and one `LiveClient` case wrapping the poll-decode-cursor loop — no second MTConnect surface, no bespoke poller beyond the `OutboundHop`.
- the data-item map (device key, data-item keys, poll interval, sequence cursor) is binding-spec policy DATA; the per-row retry is the `OutboundHop` breaker, never an MTConnect re-poll loop.
- the named forward consumers are Fabrication `Tooling/magazine` mid-job tool-life reload — decoding `CuttingToolAsset` life/wear observations — and `Verify/probing` measured-feature/work-offset observations; both pin the `-Common` model slice and firewall transport here, the observation crossing the seam as a wire row exactly as `api-bacnet.md` feeds the twin-calibration lane. OPC-UA/umati machine data stays on the kept `OPCFoundation` runtime, never re-homed here.

[RAIL_LAW]:
- Package: `MTConnect.NET-Common`
- Owns: the MTConnect observation/device/asset/streams model, response-document parse, SHDR adapter relay, and incremental-poll cursor state
- Accept: an agent document fetched over the AppHost `OutboundHop`, decoded to `ExternalValue` at the boundary, with `MTConnectClientInformation` as the durable sequence cursor
- Reject: a bundled HTTP/MQTT transport client (firewalled — transport is the `OutboundHop`), a second MTConnect poller, or the boxed model type crossing into the interior
