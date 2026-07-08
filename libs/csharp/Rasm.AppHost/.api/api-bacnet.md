# [RASM_APPHOST_API_BACNET]

`BACnet` (ela-compil `System.IO.BACnet`) is a managed BACnet stack: a `BacnetClient` bound to a protocol transport (`BacnetIpUdpProtocolTransport` for BACnet/IP, MS/TP, PTP, or serial) discovers devices (`WhoIs`/`Iam`), reads and writes object properties (`ReadPropertyRequest`/`WritePropertyRequest`), and subscribes to change-of-value pushes (`SubscribeCOVRequest` → `OnCOVNotification`) — the building-management-protocol owner the AppHost live-wire `bacnet` transport row binds behind the one `TransportRow` adapter, feeding metered building-automation observations to the twin-calibration lane at the seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `BACnet`
- package: `BACnet`
- license: `MIT`
- assembly: `BACnet`
- namespace: `System.IO.BACnet` (client/transports/types), `System.IO.BACnet.Serialize`, `System.IO.BACnet.Storage`, `System.IO.BACnet.Base`
- target: `netstandard2.0`; 164 types across 7 namespaces
- dependency floor: `Common.Logging` (logging abstraction), `System.IO.Ports` (the MS/TP + serial transports; the shared serial owner at `api-serialport.md`), `PacketDotNet`/`SharpPcap` (the raw-Ethernet transport ONLY — requires native libpcap; the BACnet/IP UDP transport the AppHost binding uses needs neither)
- asset: runtime library
- rail: live-wire

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and transport surfaces
- rail: live-wire

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]   | [RAIL]                                                    |
| :-----: | :-------------------------------- | :-------------- | :------------------------------------------------------- |
|  [01]   | `BacnetClient`                    | client          | discovery + confirmed/unconfirmed service requests over a transport |
|  [02]   | `IBacnetTransport`                | transport iface | `Start`/`Send` + `MessageRecieved` event, `MaxAdpuLength`, `Type` |
|  [03]   | `BacnetIpUdpProtocolTransport`    | transport        | BACnet/IP over UDP (default port 47808) — the AppHost binding |
|  [04]   | `BacnetMstpProtocolTransport`     | transport        | MS/TP over a serial line (`System.IO.Ports`)             |
|  [05]   | `BacnetSerialPortTransport`       | transport        | point-to-point serial                                    |
|  [06]   | `BacnetEthernetProtocolTransport` | transport        | raw Ethernet (requires SharpPcap/libpcap)                |

[PUBLIC_TYPE_SCOPE]: address, object, and value surfaces
- rail: live-wire

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :---------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `BacnetAddress`               | struct        | device network address (network number + MAC)              |
|  [02]   | `BacnetObjectId`              | struct        | object type + instance (`AnalogInput:0`, `Device:12345`)   |
|  [03]   | `BacnetPropertyIds`           | enum          | property identifier (`PROP_PRESENT_VALUE`, `PROP_OBJECT_NAME`) |
|  [04]   | `BacnetValue`                 | struct        | tagged value (`Tag` + boxed `Value`) — the read/write unit  |
|  [05]   | `BacnetPropertyReference`     | struct        | property id + array index for multi-property reads          |
|  [06]   | `BacnetEventNotificationData` | struct        | COV / alarm notification payload                            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: lifecycle and discovery
- rail: live-wire

| [INDEX] | [MEMBER]                                                                        | [KIND]  | [RETURN]       |
| :-----: | :------------------------------------------------------------------------------ | :------ | :------------- |
|  [01]   | `new BacnetClient(IBacnetTransport transport, int timeout = 1000, int retries = 3)` | ctor    | `BacnetClient` |
|  [02]   | `new BacnetClient(int port = 47808, int timeout = 1000, int retries = 3)`        | ctor    | `BacnetClient` — default BACnet/IP UDP transport |
|  [03]   | `BacnetClient.Start()`                                                           | call    | `void`         |
|  [04]   | `BacnetClient.WhoIs(int lowLimit = -1, int highLimit = -1, BacnetAddress receiver = null, BacnetAddress source = null)` | call | `void` — broadcast device discovery |
|  [05]   | `BacnetClient.OnIam` / `OnWhoIs` / `OnCOVNotification` / `OnEventNotify`         | event   | discovery + push callbacks |
|  [06]   | `BacnetClient.RegisterAsForeignDevice(string bbmdIp, short ttl, int port = 47808)` | call  | `void` — BBMD foreign-device registration for routed networks |

[ENTRYPOINT_SCOPE]: property read/write and COV
- rail: live-wire

| [INDEX] | [MEMBER]                                                                                                                       | [KIND]      | [RETURN] |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------- | :---------- | :------- |
|  [01]   | `ReadPropertyRequest(BacnetAddress adr, BacnetObjectId objectId, BacnetPropertyIds propertyId, out IList<BacnetValue> valueList, byte invokeId = 0)` | read | `bool`   |
|  [02]   | `BeginReadPropertyRequest(BacnetAddress address, BacnetObjectId objectId, BacnetPropertyIds propertyId, bool waitForTransmit, byte invokeId = 0)` | read (async) | `IAsyncResult` |
|  [03]   | `ReadPropertyMultipleRequest(BacnetAddress address, IList<BacnetReadAccessSpecification> properties, out IList<BacnetReadAccessResult> values, byte invokeId = 0)` | read | `bool` |
|  [04]   | `WritePropertyRequest(BacnetAddress adr, BacnetObjectId objectId, BacnetPropertyIds propertyId, IEnumerable<BacnetValue> valueList, byte invokeId = 0)` | write | `bool` |
|  [05]   | `SubscribeCOVRequest(BacnetAddress adr, BacnetObjectId objectId, uint subscribeId, bool cancel, bool issueConfirmedNotifications, uint lifetime, byte invokeId = 0)` | subscribe | `bool` |
|  [06]   | `ReadRangeRequest(BacnetAddress adr, BacnetObjectId objectId, DateTime readFrom, ref uint quantity, out byte[] range, byte invokeId = 0)` | read (trend) | `bool` |

## [04]-[IMPLEMENTATION_LAW]

[IMPLEMENTATION_LAW]: client semantics
- rail: live-wire

- `BacnetClient` binds one `IBacnetTransport`; the AppHost binding uses `BacnetIpUdpProtocolTransport` (BACnet/IP, no native dependency) — the raw-Ethernet transport (SharpPcap/libpcap) and MS/TP serial transport stay recorded growth options behind a companion process where the host lacks the native line. `Start()` opens the transport; the client is `IDisposable` and holds in the same token-gated state cell the OPC-UA/MQTT/serial clients ride, so a reconnect replaces the whole cell.
- read shape is dual: `ReadPropertyRequest` is the poll path (synchronous confirmed request returning `IList<BacnetValue>`), and `SubscribeCOVRequest` + the `OnCOVNotification` event is the subscribe path (the device pushes present-value changes) — the binding chooses per row, COV preferred for metered points, poll for on-demand.
- a `BacnetValue` is `Tag` + boxed value; the binding projects one register/property read into one `ExternalValue` (raw value, declared unit from `PROP_UNITS`, good flag from the read status, source instant), never surfacing the boxed tag into the interior.
- `OnCOVNotification`/`OnIam` fire on a transport thread; the handler decodes the notification and `TryWrite`s one `ExternalValue` into the bounded lane at the boundary, never running the interior on the callback thread — mirroring the OPC-UA/MQTT callback law.
- a confirmed-request failure (`bool false`, timeout, or a BACnet-Error/Reject/Abort) surfaces at the boundary as `WireFault.ReadFailed`/`WriteRejected`, never propagating into the interior; `RegisterAsForeignDevice` handles BBMD-routed networks where the device is not on the local broadcast domain.

[IMPLEMENTATION_LAW]: AppHost usage
- rail: live-wire

- the live-wire `bacnet` transport row is one `ExternalTransport` `[SmartEnum<string>]` case (`Wire/livewire#TRANSPORT_AXIS`) with its `TransportRow` (`ReadShape.Poll` with COV subscribe, `Writable: true`, an `OutboundHop.CompanionSpawn`/direct-UDP hop) and one `LiveClient` case wrapping `BacnetClient` — no second BACnet surface, no bespoke poller.
- a point map (object id, property id, COV lifetime, unit id) is binding-spec policy DATA, never a parallel BACnet loop; the per-row retry is the `OutboundHop` breaker, never a BACnet `retries` re-loop beyond the client's own confirmed-request retry.
- the named forward consumer is the Compute inverse-UQ / twin-calibration lane's metered-data ingress (`RASM-CS-COMPUTE-BRIEF.md` `[V12]` recorded growth): building-automation observations (temperature, occupancy, energy) decode at this seam exactly as MTConnect machine-tool observations (`api-mtconnect.md`) feed Fabrication — one decode boundary, the observation crossing as a wire row.

[RAIL_LAW]:
- Package: `BACnet`
- Owns: BACnet/IP device discovery, property read/write, and COV subscription as one live-wire transport row
- Accept: a `BacnetIpUdpProtocolTransport`-bound `BacnetClient`, a point map as binding-spec policy, and COV push decoded to `ExternalValue` at the boundary
- Reject: a second BACnet poller, the raw-Ethernet/libpcap transport on a host without the native line (companion-only), or a confirmed-request failure crossing into the interior
