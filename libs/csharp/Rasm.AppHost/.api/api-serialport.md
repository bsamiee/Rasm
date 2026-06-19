# [RASM_APPHOST_API_SERIALPORT]

`System.IO.Ports` is the BCL serial-communication owner: a `SerialPort` opens an RS-232/422/485 line over a named port, configures baud/parity/data-bits/stop-bits/handshake, reads and writes line-framed or raw bytes synchronously or through the `DataReceived` event, and exposes the underlying `Stream` for binary protocols, the serial fieldbus owner the AppHost live-wire `serial` transport row binds behind the one `TransportRow` adapter (and the line carrier `ModbusRtuClient` rides for Modbus-RTU).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Ports`
- package: `System.IO.Ports`
- assembly: `System.IO.Ports`
- namespace: `System.IO.Ports`
- asset: runtime library
- rail: live-wire

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: port and policy surfaces
- rail: live-wire

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]     | [RAIL]                                            |
| :-----: | :-------------------------------- | :---------------- | :----------------------------------------------- |
|   [1]   | `SerialPort`                      | component         | the serial line — open, read, write, configure   |
|   [2]   | `Parity`                          | enum              | `None`/`Odd`/`Even`/`Mark`/`Space`               |
|   [3]   | `StopBits`                        | enum              | `None`/`One`/`Two`/`OnePointFive`                |
|   [4]   | `Handshake`                       | enum              | `None`/`XOnXOff`/`RequestToSend`/`RequestToSendXOnXOff` |
|   [5]   | `SerialData`                      | enum              | `Chars`/`Eof` received-data trigger              |
|   [6]   | `SerialError`                     | enum              | `Frame`/`Overrun`/`RXOver`/`RXParity`/`TXFull`   |
|   [7]   | `SerialPinChange`                 | enum              | `CtsChanged`/`DsrChanged`/`CDChanged`/`Ring`/`Break` |

[PUBLIC_TYPE_SCOPE]: event-argument surfaces
- rail: live-wire

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]   | [RAIL]                            |
| :-----: | :---------------------------------- | :-------------- | :------------------------------- |
|   [1]   | `SerialDataReceivedEventArgs`       | event args      | `EventType` (`SerialData`)       |
|   [2]   | `SerialErrorReceivedEventArgs`      | event args      | `EventType` (`SerialError`)      |
|   [3]   | `SerialPinChangedEventArgs`         | event args      | `EventType` (`SerialPinChange`)  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: lifecycle and configuration
- rail: live-wire

| [INDEX] | [MEMBER]                                                                  | [KIND]      | [RETURN]    |
| :-----: | :------------------------------------------------------------------------ | :---------- | :---------- |
|   [1]   | `new SerialPort(string portName, int baudRate, Parity, int dataBits, StopBits)` | ctor  | `SerialPort` |
|   [2]   | `SerialPort.Open()`                                                       | call        | `void`      |
|   [3]   | `SerialPort.Close()`                                                      | call        | `void`      |
|   [4]   | `SerialPort.GetPortNames()`                                              | static call | `string[]`  |
|   [5]   | `SerialPort.IsOpen`                                                       | property    | `bool`      |
|   [6]   | `SerialPort.BaudRate` / `Parity` / `DataBits` / `StopBits` / `Handshake`  | property    | line policy |
|   [7]   | `SerialPort.ReadTimeout` / `WriteTimeout`                                 | property    | `int` (ms)  |
|   [8]   | `SerialPort.NewLine` / `Encoding`                                         | property    | framing     |
|   [9]   | `SerialPort.DtrEnable` / `RtsEnable`                                      | property    | flow control |

[ENTRYPOINT_SCOPE]: read/write and events
- rail: live-wire

| [INDEX] | [MEMBER]                                              | [KIND]      | [RETURN]   |
| :-----: | :---------------------------------------------------- | :---------- | :--------- |
|   [1]   | `SerialPort.ReadLine()`                              | read        | `string`   |
|   [2]   | `SerialPort.ReadExisting()`                          | read        | `string`   |
|   [3]   | `SerialPort.Read(byte[] buffer, int offset, int count)` | read     | `int`      |
|   [4]   | `SerialPort.WriteLine(string text)`                  | write       | `void`     |
|   [5]   | `SerialPort.Write(byte[] buffer, int offset, int count)` | write    | `void`     |
|   [6]   | `SerialPort.BaseStream`                              | property    | `Stream`   |
|   [7]   | `SerialPort.BytesToRead` / `BytesToWrite`            | property    | `int`      |
|   [8]   | `SerialPort.DataReceived`                            | event       | `SerialDataReceivedEventHandler` |
|   [9]   | `SerialPort.ErrorReceived`                           | event       | `SerialErrorReceivedEventHandler` |
|  [10]   | `SerialPort.PinChanged`                              | event       | `SerialPinChangedEventHandler` |
|  [11]   | `SerialPort.DiscardInBuffer()` / `DiscardOutBuffer()` | call      | `void`     |

## [4]-[IMPLEMENTATION_LAW]

[IMPLEMENTATION_LAW]: port semantics
- rail: live-wire

- `SerialPort` is `IDisposable`; the AppHost binding holds it in the same token-gated state cell the OPC-UA/MQTT held clients ride, so a reconnect replaces the whole cell and a stale teardown never disposes a fresh port.
- the `DataReceived` event fires on a `ThreadPool` thread when data arrives; the handler decodes the line frame and `TryWrite`s one `ExternalValue` into the bounded lane at the boundary, never running the interior on the event thread — mirroring the OPC-UA/MQTT callback law.
- line framing is `NewLine`/`Encoding` policy; a binary protocol reads `BaseStream` directly, a line protocol reads `ReadLine`, the choice a binding-spec column.
- `BaudRate`/`Parity`/`DataBits`/`StopBits`/`Handshake` are line-policy values carried on the binding spec, never literals at the call site.
- a frame/overrun/parity error surfaces through `ErrorReceived`; the AppHost binding projects it to `WireFault.ReadFailed`, never propagating into the interior.

[IMPLEMENTATION_LAW]: AppHost usage
- rail: live-wire

- the live-wire `serial` transport row binds `SerialPort` behind the one `TransportRow.Read`/`Write` adapter; the serial leg attaches through the `Wire/companion#CONTROL_SERVICE` spawn-attach where the device lives behind a companion process, or binds the port directly where the host owns the line.
- a line grammar (NMEA/ASCII/binary frame) is binding-spec policy data, never a serial-specific poller; the per-row retry is the `OutboundHop` redial, never a serial reconnect loop.
- the `ModbusRtuClient` (`api-modbus.md`) rides this same serial line for Modbus-RTU, so the serial owner carries both the raw-line and the RTU-carrier role with no second serial surface.
