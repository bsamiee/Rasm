# [RASM_APPHOST_API_SERIALPORT]

`System.IO.Ports` owns BCL serial-fieldbus transport: `SerialPort` opens an RS-232/422/485 line over a named port, reads and writes line-framed or raw bytes synchronously or through the `DataReceived` event, and exposes the underlying `Stream` for binary protocols. AppHost's live-wire `serial` transport row binds it behind the one `TransportRow` adapter, and `ErrorReceived` faults project to `WireFault` at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Ports`
- package: `System.IO.Ports`
- assembly: `System.IO.Ports`
- namespace: `System.IO.Ports`
- asset: runtime library
- rail: live-wire

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: port, policy, and event-argument surfaces

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `SerialPort`                   | component     | the serial line — open, read, write, configure          |
|  [02]   | `Parity`                       | enum          | `None`/`Odd`/`Even`/`Mark`/`Space`                      |
|  [03]   | `StopBits`                     | enum          | `None`/`One`/`Two`/`OnePointFive`                       |
|  [04]   | `Handshake`                    | enum          | `None`/`XOnXOff`/`RequestToSend`/`RequestToSendXOnXOff` |
|  [05]   | `SerialData`                   | enum          | `Chars`/`Eof` received-data trigger                     |
|  [06]   | `SerialError`                  | enum          | `Frame`/`Overrun`/`RXOver`/`RXParity`/`TXFull`          |
|  [07]   | `SerialPinChange`              | enum          | `CtsChanged`/`DsrChanged`/`CDChanged`/`Ring`/`Break`    |
|  [08]   | `SerialDataReceivedEventArgs`  | event args    | `EventType` (`SerialData`)                              |
|  [09]   | `SerialErrorReceivedEventArgs` | event args    | `EventType` (`SerialError`)                             |
|  [10]   | `SerialPinChangedEventArgs`    | event args    | `EventType` (`SerialPinChange`)                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: lifecycle and configuration

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `SerialPort(string, int, Parity, int, StopBits)`                 | ctor     | construct a port under line policy |
|  [02]   | `SerialPort.Open()`                                              | instance | open the configured port           |
|  [03]   | `SerialPort.Close()`                                             | instance | close the port                     |
|  [04]   | `SerialPort.GetPortNames() -> string[]`                          | static   | enumerate host port names          |
|  [05]   | `SerialPort.IsOpen -> bool`                                      | property | open-state check                   |
|  [06]   | `SerialPort.BaudRate`/`Parity`/`DataBits`/`StopBits`/`Handshake` | property | line policy                        |
|  [07]   | `SerialPort.ReadTimeout`/`WriteTimeout`                          | property | timeout, ms                        |
|  [08]   | `SerialPort.NewLine`/`Encoding`                                  | property | line framing                       |
|  [09]   | `SerialPort.DtrEnable`/`RtsEnable`                               | property | flow control                       |

[ENTRYPOINT_SCOPE]: read, write, and events

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `SerialPort.ReadLine() -> string`                   | instance | read one `NewLine`-framed line     |
|  [02]   | `SerialPort.ReadExisting() -> string`               | instance | drain the receive buffer to text   |
|  [03]   | `SerialPort.Read(byte[], int, int) -> int`          | instance | read raw bytes into a window       |
|  [04]   | `SerialPort.WriteLine(string)`                      | instance | write text framed by `NewLine`     |
|  [05]   | `SerialPort.Write(byte[], int, int)`                | instance | write raw bytes from a window      |
|  [06]   | `SerialPort.BaseStream -> Stream`                   | property | raw binary-protocol stream         |
|  [07]   | `SerialPort.BytesToRead`/`BytesToWrite`             | property | buffered byte counts               |
|  [08]   | `SerialPort.DataReceived`                           | event    | `SerialDataReceivedEventHandler`   |
|  [09]   | `SerialPort.ErrorReceived`                          | event    | `SerialErrorReceivedEventHandler`  |
|  [10]   | `SerialPort.PinChanged`                             | event    | `SerialPinChangedEventHandler`     |
|  [11]   | `SerialPort.DiscardInBuffer()`/`DiscardOutBuffer()` | instance | flush the receive and send buffers |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SerialPort` is `IDisposable`; the AppHost binding holds it in a token-gated state cell, so a reconnect replaces the whole cell and a stale teardown never disposes a fresh port.
- `DataReceived` fires on a `ThreadPool` thread; the handler decodes the frame and `TryWrite`s one `ExternalValue` into the bounded lane at the boundary, never running the interior on the event thread.
- `NewLine`/`Encoding` frame a line protocol read through `ReadLine`, while a binary protocol reads `BaseStream` directly; the choice is a binding-spec column.
- `ErrorReceived` projects a frame, overrun, or parity fault to `WireFault.ReadFailed` at the boundary, never propagating into the interior.

[STACKING]:
- `FluentModbus`(`.api/api-modbus.md`): `ModbusRtuClient` rides this serial line for Modbus-RTU, so the serial owner carries both the raw-line and RTU-carrier roles with no second serial surface.
- `TransportRow` adapter: the live-wire `serial` row binds `SerialPort` through one `Read`/`Write` seam, the leg taking `OutboundHop.CompanionSpawn` where the device sits behind a companion process or the port directly where the host owns the line.

[LOCAL_ADMISSION]:
- `BaudRate`/`Parity`/`DataBits`/`StopBits`/`Handshake` and the line grammar are binding-spec policy data; the per-row retry is the `OutboundHop` redial, never a serial reconnect loop or a call-site literal.

[RAIL_LAW]:
- Package: `System.IO.Ports`
- Owns: BCL serial-line transport — named-port open, byte and line framing, and the pin, data, and error signal events.
- Accept: one `TransportRow` serial row projecting each decoded frame to one `ExternalValue`.
- Reject: a serial-specific poller, a reconnect loop, or a second serial surface beside the one `TransportRow` adapter.
