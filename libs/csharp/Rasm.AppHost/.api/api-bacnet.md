# [RASM_APPHOST_API_BACNET]

`BACnet` (ela-compil `System.IO.BACnet`) owns managed building-automation-protocol capability: one `BacnetClient` bound to an `IBacnetTransport` discovers devices, reads and writes object properties, and subscribes to change-of-value pushes over the live wire. AppHost binds it behind the one `TransportRow` adapter through the `bacnet` live-wire transport row, decoding metered building observations to `ExternalValue` at the boundary for the twin-calibration lane.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `BACnet`
- package: `BACnet` (`MIT`, ela-compil)
- assembly: `BACnet`
- namespace: `System.IO.BACnet`, `System.IO.BACnet.Serialize`, `System.IO.BACnet.Storage`, `System.IO.BACnet.Base`
- target: `netstandard2.0`
- depends: `Common.Logging` (logging abstraction), `System.IO.Ports` (MS/TP and serial transports, serial owner at `api-serialport.md`), `PacketDotNet`/`SharpPcap` (raw-Ethernet transport over the native libpcap line)
- asset: runtime library
- rail: live-wire

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and transport surfaces

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :-------------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `BacnetClient`                    | class         | discovery + confirmed/unconfirmed service requests over a transport |
|  [02]   | `IBacnetTransport`                | interface     | `Start`/`Send`, `MessageRecieved` event, `MaxAdpuLength`, `Type`    |
|  [03]   | `BacnetIpUdpProtocolTransport`    | class         | BACnet/IP over UDP on port 47808, the AppHost binding               |
|  [04]   | `BacnetMstpProtocolTransport`     | class         | MS/TP over a serial line                                            |
|  [05]   | `BacnetSerialPortTransport`       | class         | point-to-point serial                                               |
|  [06]   | `BacnetEthernetProtocolTransport` | class         | raw Ethernet over libpcap                                           |

[PUBLIC_TYPE_SCOPE]: address, object, and value surfaces

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :---------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `BacnetAddress`               | class         | device network address (network number + MAC)                  |
|  [02]   | `BacnetObjectId`              | struct        | object type + instance (`AnalogInput:0`, `Device:12345`)       |
|  [03]   | `BacnetPropertyIds`           | enum          | property identifier (`PROP_PRESENT_VALUE`, `PROP_OBJECT_NAME`) |
|  [04]   | `BacnetValue`                 | struct        | tagged value (`Tag` + boxed `Value`), the read/write unit      |
|  [05]   | `BacnetPropertyReference`     | struct        | property id + array index for multi-property reads             |
|  [06]   | `BacnetEventNotificationData` | struct        | COV/alarm notification payload                                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: lifecycle and discovery

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :----------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `BacnetClient(IBacnetTransport, int, int)`                   | ctor     | bind a supplied transport, timeout, retry |
|  [02]   | `BacnetClient(int, int, int)`                                | ctor     | bind the default BACnet/IP UDP transport  |
|  [03]   | `BacnetClient.Start()`                                       | instance | open the bound transport                  |
|  [04]   | `BacnetClient.WhoIs(int, int, BacnetAddress, BacnetAddress)` | instance | broadcast device discovery                |
|  [05]   | `BacnetClient.RegisterAsForeignDevice(string, short, int)`   | instance | register with a BBMD for routed networks  |
|  [06]   | `BacnetClient.OnIam`                                         | event    | device-announcement callback              |
|  [07]   | `BacnetClient.OnWhoIs`                                       | event    | discovery-request callback                |
|  [08]   | `BacnetClient.OnCOVNotification`                             | event    | change-of-value push callback             |
|  [09]   | `BacnetClient.OnEventNotify`                                 | event    | alarm/event-notification callback         |

[ENTRYPOINT_SCOPE]: property read/write and COV

`BacnetClient` confirmed read/write requests share the `(BacnetAddress, BacnetObjectId, BacnetPropertyIds, …)` head and return `bool`, false on a BACnet Error/Reject/Abort.

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `ReadPropertyRequest(…, out IList<BacnetValue>)`                                  | instance | confirmed synchronous poll   |
|  [02]   | `BeginReadPropertyRequest(…, bool) -> IAsyncResult`                               | instance | async property read          |
|  [03]   | `WritePropertyRequest(…, IEnumerable<BacnetValue>)`                               | instance | confirmed property write     |
|  [04]   | `SubscribeCOVRequest(BacnetAddress, BacnetObjectId, uint, bool, bool, uint)`      | instance | change-of-value subscription |
|  [05]   | `ReadRangeRequest(BacnetAddress, BacnetObjectId, DateTime, ref uint, out byte[])` | instance | trend/log range read         |
|  [06]   | `ReadPropertyMultipleRequest(...)`                                                | instance | batched multi-property read  |

- `ReadPropertyMultipleRequest(IList<BacnetReadAccessSpecification>, out IList<BacnetReadAccessResult>) -> bool`: one confirmed round trip over an access-spec list.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `BacnetClient` binds one `IBacnetTransport` and is `IDisposable`; the AppHost binding holds it in the token-gated state cell the OPC-UA/MQTT/serial clients share, so a reconnect replaces the whole cell.
- Read shape is dual: `ReadPropertyRequest` is the confirmed poll path returning `IList<BacnetValue>`, and `SubscribeCOVRequest` with the `OnCOVNotification` event is the push path — COV binds metered points, poll binds on-demand reads.
- One property read projects to one `ExternalValue` (raw value, declared unit from `PROP_UNITS`, good flag from the read status, source instant); the boxed `BacnetValue` tag never enters the interior.
- A confirmed-request failure (`bool false`, timeout, or a BACnet Error/Reject/Abort) surfaces at the boundary as `WireFault.ReadFailed`/`WriteRejected`, never into the interior; `RegisterAsForeignDevice` routes BBMD networks off the local broadcast domain.
- `OnCOVNotification`/`OnIam` fire on a transport thread; the handler decodes the notification and `TryWrite`s one `ExternalValue` into the bounded lane at the boundary, never running the interior on the callback thread.

[STACKING]:
- `api-serialport.md`(`System.IO.Ports`): `BacnetMstpProtocolTransport`/`BacnetSerialPortTransport` bind the shared serial line the serialport owner carries, so the MS/TP row and the `serial` row select one line owner with no second serial surface.
- `api-mtconnect.md`: building-automation observations decode at this seam exactly as MTConnect machine-tool observations feed Fabrication — one decode boundary, the observation crossing as a wire row.
- within-lib: the `bacnet` row is one `ExternalTransport` `[SmartEnum<string>]` case with its `TransportRow` (`ReadShape.Poll` with COV subscribe, `Writable: true`, an `OutboundHop` hop) and one `LiveClient` case wrapping `BacnetClient`, no bespoke poller beyond the client's confirmed-request retry.

[LOCAL_ADMISSION]:
- A point map (object id, property id, COV lifetime, unit id) is binding-spec policy data, never a parallel BACnet loop; the per-row retry is the `OutboundHop` breaker.
- A host owning the native UDP line binds `BacnetIpUdpProtocolTransport` directly; a host lacking the libpcap or serial line selects the `OutboundHop.CompanionSpawn` hop, and the raw-Ethernet and MS/TP transports are peer rows that host fact selects.

[RAIL_LAW]:
- Package: `BACnet`
- Owns: BACnet/IP device discovery, property read/write, and COV subscription as one live-wire transport row
- Accept: a `BacnetIpUdpProtocolTransport`-bound `BacnetClient`, a point map as binding-spec policy, and COV push decoded to `ExternalValue` at the boundary
- Reject: a second BACnet poller, a confirmed-request failure crossing into the interior, or the boxed `BacnetValue` tag entering the interior
