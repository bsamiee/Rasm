# [RASM_APPHOST_API_SERIALPORT]

`System.IO.Ports` owns BCL serial-fieldbus transport: `SerialPort` opens an RS-232/422/485 line over a named port, reads and writes line-framed or raw bytes synchronously or through the `DataReceived` event, and exposes the underlying `Stream` for binary protocols. AppHost's live-wire `serial` transport row binds it behind the one `TransportRow` adapter, and a thrown `TimeoutException` projects to `WireFault` at the boundary — the unix runtime raises no `ErrorReceived` event.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `System.IO.Ports`
- package: `System.IO.Ports`
- assembly: `System.IO.Ports`
- namespace: `System.IO.Ports`
- asset: runtime library
- resolve: `lib/net10.0/System.IO.Ports.dll` is the `PlatformNotSupportedException` facade; host truth decompiles from `runtimes/unix/lib/net10.0/`, and Windows-only members live in `runtimes/win/lib/net10.0/`
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

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------------------------- | :------- | :----------------------------------------------------- |
|  [01]   | `SerialPort(string, int, Parity, int, StopBits)`                 | ctor     | construct a port under line policy                     |
|  [02]   | `SerialPort.Open()`                                              | instance | open the configured port                               |
|  [03]   | `SerialPort.Close()`                                             | instance | close the port                                         |
|  [04]   | `SerialPort.GetPortNames() -> string[]`                          | static   | enumerate host port names                              |
|  [05]   | `SerialPort.IsOpen -> bool`                                      | property | open-state check                                       |
|  [06]   | `SerialPort.BaudRate`/`Parity`/`DataBits`/`StopBits`/`Handshake` | property | line policy                                            |
|  [07]   | `SerialPort.ReadTimeout`/`WriteTimeout`                          | property | timeout, ms                                            |
|  [08]   | `SerialPort.NewLine`/`Encoding`                                  | property | line framing                                           |
|  [09]   | `SerialPort.DtrEnable`/`RtsEnable`                               | property | modem lines; RTS is the RS-485 DE/RE half-duplex drive |

[ENTRYPOINT_SCOPE]: read, write, and events

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `SerialPort.ReadLine() -> string`                   | instance | read one `NewLine`-framed line              |
|  [02]   | `SerialPort.ReadExisting() -> string`               | instance | drain the receive buffer to text            |
|  [03]   | `SerialPort.Read(byte[], int, int) -> int`          | instance | read raw bytes into a window                |
|  [04]   | `SerialPort.WriteLine(string)`                      | instance | write text framed by `NewLine`              |
|  [05]   | `SerialPort.Write(byte[], int, int)`                | instance | write raw bytes from a window               |
|  [06]   | `SerialPort.BaseStream -> Stream`                   | property | raw binary-protocol stream                  |
|  [07]   | `SerialPort.BytesToRead`/`BytesToWrite`             | property | buffered byte counts                        |
|  [08]   | `SerialPort.DataReceived`                           | event    | `SerialDataReceivedEventHandler`            |
|  [09]   | `SerialPort.ErrorReceived`                          | event    | `SerialErrorReceivedEventHandler`, win-only |
|  [10]   | `SerialPort.PinChanged`                             | event    | `SerialPinChangedEventHandler`              |
|  [11]   | `SerialPort.DiscardInBuffer()`/`DiscardOutBuffer()` | instance | flush the receive and send buffers          |

[ENTRYPOINT_SCOPE]: `SerialError` flag roster

`SerialError` values are FLAGS, so one `SerialErrorReceivedEventArgs.EventType` reads as a set rather than a single case.

| [INDEX] | [MEMBER]   | [VALUE] | [MEANING]                                 |
| :-----: | :--------- | :-----: | :---------------------------------------- |
|  [01]   | `RXOver`   |    1    | receive buffer overflowed                 |
|  [02]   | `Overrun`  |    2    | character overran before it was read      |
|  [03]   | `RXParity` |    4    | parity mismatch on a received character   |
|  [04]   | `Frame`    |    8    | framing error on a received character     |
|  [05]   | `TXFull`   |  0x100  | transmit buffer full, unreachable on unix |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SerialPort` is `IDisposable`; the AppHost binding holds it in a token-gated state cell, so a reconnect replaces the whole cell and a stale teardown never disposes a fresh port.
- `DataReceived` fires on a `ThreadPool` thread; the handler decodes the frame and `TryWrite`s one `ExternalValue` into the bounded lane at the boundary, never running the interior on the event thread.
- `NewLine`/`Encoding` frame a line protocol read through `ReadLine`, while a binary protocol reads `BaseStream` directly; the choice is a binding-spec column.
- TRAP: `ErrorReceived` NEVER fires on macOS or Linux. All five `new SerialErrorReceivedEventArgs(...)` construction sites live in `runtimes/win/lib/net10.0/System.IO.Ports.dll`; the unix runtime declares `SerialStream.ErrorReceived` and raises it nowhere, so `TXFull` is unreachable and a frame, overrun, or parity fault surfaces only as a read timeout.
- `SerialPort.Write` refuses by `TimeoutException` alone — `SerialStream.Write` wraps it from an `OperationCanceledException` — and a closed port raises `InvalidOperationException`; no path returns a status value.
- `SerialPort` streams untyped bytes and publishes no per-write echo token; the read instant is the host clock, so a serial binding's echo axis takes `EchoClass.Absent` and write proof is a value read-back.
- `SerialPort` itself carries NO async read/write — `BaseStream.ReadAsync`/`WriteAsync` is the only async path — and `Read` on an armed `ReadTimeout` signals expiry by THROWING `TimeoutException`, never a zero return; setting `RtsEnable` by hand while `Handshake` is `RequestToSend`/`RequestToSendXOnXOff` throws, so a half-duplex RS-485 line drives RTS manually under a `Handshake` that leaves the pin free.

[STACKING]:
- `FluentModbus`(`.api/api-modbus.md`): `ModbusRtuClient` rides this serial line for Modbus-RTU, so the serial owner carries both the raw-line and RTU-carrier roles with no second serial surface.
- `BACnet`(`api-bacnet.md`): the composition's own `IBacnetSerialTransport` line rides this same held port under that catalog's `[SERIAL_LINE_CONTRACT]` sentinel law — seated through `Wire/livewire#LANE_SUBSTRATE` `TransportSeat.Bacnet.Open`, which supplies the whole transport chain — so one line owner serves the raw `serial` row, the RTU carrier, and the MS/TP bus.
- `TransportRow` adapter: the live-wire `serial` row binds `SerialPort` through one `Read`/`Write` seam, the leg taking `OutboundHop.CompanionSpawn` where the device sits behind a companion process or the port directly where the host owns the line.

[LOCAL_ADMISSION]:
- `BaudRate`/`Parity`/`DataBits`/`StopBits`/`Handshake` and the line grammar are binding-spec policy data; the per-row retry is the `OutboundHop` redial, never a serial reconnect loop or a call-site literal.

[RAIL_LAW]:
- Package: `System.IO.Ports`
- Owns: BCL serial-line transport — named-port open, byte and line framing, and the pin, data, and error signal events.
- Accept: one `TransportRow` serial row projecting each decoded frame to one `ExternalValue`.
- Reject: a serial-specific poller, a reconnect loop, or a second serial surface beside the one `TransportRow` adapter.
