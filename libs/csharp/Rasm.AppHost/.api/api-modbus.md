# [RASM_APPHOST_API_MODBUS]

`FluentModbus` owns managed Modbus TCP, RTU, and ASCII client transport through one `ModbusClient` register and coil surface windowed as typed `Span<T>`. `ModbusEndianness` fixes register byte order at `Connect`, a `ModbusException` carries the reason-coded protocol fault, and every client folds behind the AppHost live-wire `TransportRow` adapter.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentModbus`
- package: `FluentModbus` (MIT)
- assembly: `FluentModbus`
- namespace: `FluentModbus`
- target: `netstandard2.1`
- rail: live-wire

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and server surfaces

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                                  |
| :-----: | :----------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `ModbusClient`           | abstract class | shared register and coil function-code ops    |
|  [02]   | `ModbusTcpClient`        | class          | Modbus-TCP transport over a socket            |
|  [03]   | `ModbusRtuClient`        | class          | Modbus-RTU/ASCII transport over a serial port |
|  [04]   | `ModbusRtuOverTcpClient` | class          | RTU frames tunneled over a TCP socket         |
|  [05]   | `ModbusTcpServer`        | class          | in-process Modbus-TCP server                  |
|  [06]   | `ModbusRtuServer`        | class          | in-process Modbus-RTU server                  |

[PUBLIC_TYPE_SCOPE]: policy and fault surfaces

RTU line format rides `ModbusRtuClient` `BaudRate`/`Parity`/`StopBits`/`Handshake` properties, the `Parity`/`StopBits`/`Handshake` enums owned by `System.IO.Ports`.

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :-------------------- | :------------ | :---------------------------------------- |
|  [01]   | `ModbusEndianness`    | enum          | `LittleEndian`/`BigEndian` register order |
|  [02]   | `ModbusFunctionCode`  | enum          | protocol function-code vocabulary         |
|  [03]   | `ModbusExceptionCode` | enum          | protocol exception codes                  |
|  [04]   | `ModbusException`     | exception     | reason-coded protocol fault               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection lifecycle

Timeout properties carry milliseconds; `ModbusEndianness` defaults to `LittleEndian` when the overload omits it.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :------------------------------------------------------------ | :------- | :----------------------------------- |
|  [01]   | `ModbusTcpClient.Connect(IPEndPoint, ModbusEndianness)`       | instance | open the TCP socket, set byte order  |
|  [02]   | `ModbusTcpClient.Connect(IPAddress, ModbusEndianness)`        | instance | open the TCP socket to an address    |
|  [03]   | `ModbusRtuClient.Connect(string, ModbusEndianness)`           | instance | open the serial port, set byte order |
|  [04]   | `ModbusTcpClient.Disconnect()`                                | instance | close the TCP socket                 |
|  [05]   | `ModbusRtuClient.Close()`                                     | instance | close the serial port                |
|  [06]   | `ModbusClient.IsConnected`                                    | property | live connection status               |
|  [07]   | `ModbusRtuClient.BaudRate / Parity / StopBits / Handshake`    | property | RTU serial line format               |
|  [08]   | `ModbusTcpClient.ConnectTimeout / ReadTimeout / WriteTimeout` | property | TCP timeout policy                   |

[ENTRYPOINT_SCOPE]: register and coil operations

`ModbusClient` owns these; each leads with `int unitIdentifier` and an address window, and every generic element is `T : unmanaged`. `WriteSingleRegister` also takes `ushort` and `byte[]` overloads, and `ReadWriteMultipleRegisters` carries a read window then a write window with its `TWrite[]` dataset.

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `ReadHoldingRegisters<T>(int, int, int) -> Span<T>`             | instance | read holding registers, typed      |
|  [02]   | `ReadInputRegisters<T>(int, int, int) -> Span<T>`               | instance | read input registers, typed        |
|  [03]   | `WriteSingleRegister(int, int, short)`                          | instance | write one register                 |
|  [04]   | `WriteMultipleRegisters<T>(int, int, T[])`                      | instance | write a typed register block       |
|  [05]   | `ReadWriteMultipleRegisters<TRead, TWrite>(...) -> Span<TRead>` | instance | write-then-read in one transaction |
|  [06]   | `ReadCoils(int, int, int) -> Span<byte>`                        | instance | read coils, one bit per coil       |
|  [07]   | `ReadDiscreteInputs(int, int, int) -> Span<byte>`               | instance | read discrete inputs               |
|  [08]   | `WriteSingleCoil(int, int, bool)`                               | instance | write one coil                     |
|  [09]   | `WriteMultipleCoils(int, int, bool[])`                          | instance | write a coil block                 |
|  [10]   | `ReadHoldingRegisters(byte, ushort, ushort) -> Span<byte>`      | instance | raw untyped register window        |

[ENTRYPOINT_SCOPE]: async operations

Each async op mirrors its sync member with a trailing `CancellationToken = default` and returns `Task<Memory<T>>`, so register storage survives `await` where the sync path returns `Span<T>`.

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `ReadHoldingRegistersAsync<T>(int, int, int, CancellationToken) -> Task<Memory<T>>` | instance | await-safe holding-register read |
|  [02]   | `ReadInputRegistersAsync<T>(int, int, int, CancellationToken) -> Task<Memory<T>>`   | instance | await-safe input-register read   |
|  [03]   | `WriteMultipleRegistersAsync<T>(int, int, T[], CancellationToken) -> Task`          | instance | await-safe register block write  |
|  [04]   | `ReadWriteMultipleRegistersAsync<TRead, TWrite>(...) -> Task<Memory<TRead>>`        | instance | await-safe write-then-read       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ModbusEndianness` fixes at `Connect` and governs the byte order of every multi-byte register `T`; the AppHost binding carries it as a `TransportRow` policy column, never a per-read flag.
- `ReadHoldingRegisters<T>` reinterprets each `T : unmanaged` register window as a `Span<T>` over `short`, `ushort`, `int`, `float`, or `double`, decoded to one `ExternalValue`; the async mirror returns `Task<Memory<T>>` for storage that outlives `await`.
- `unitIdentifier` is the slave address and the address window (`startingAddress`, `count`) is binding-spec policy data, never a parallel poller.
- one `ModbusClient` register and coil surface serves the TCP, RTU, and RTU-over-TCP transports alike.
- a `ModbusException` carrying a `ModbusExceptionCode` projects to `WireFault.ReadFailed`/`WriteRejected` at the boundary, never propagating into the interior.

[STACKING]:
- `System.IO.Ports`(`.api/api-serialport.md`): `ModbusRtuClient` binds a `SerialPort` line for RTU/ASCII fieldbus, its `Parity`/`StopBits`/`Handshake` line policy carried by that owner.
- within-lib: the live-wire `modbus` transport row binds `ModbusTcpClient`/`ModbusRtuClient` behind one `TransportRow.Read`/`Write` adapter, a typed `Span<T>` read projecting one `ExternalValue` (raw value, declared unit, good flag, source instant) onto the row's `OutboundHop`, the boxed register never entering the interior.

[LOCAL_ADMISSION]:
- `ModbusTcpClient` binds an `OutboundHop.ServerStream` direct-TCP hop and `ModbusRtuClient` an `OutboundHop.CompanionSpawn` over the serial owner; the `modbus` transport row is one `ExternalTransport` `[SmartEnum<string>]` case with `ReadShape.Poll` and `Writable: true`, its register map binding-spec policy carried on the row.

[RAIL_LAW]:
- Package: `FluentModbus`
- Owns: managed Modbus TCP/RTU/ASCII client transport and typed `Span<T>` register and coil windows
- Accept: a typed register window decoded to one `ExternalValue`, and a `ModbusException`/`ModbusExceptionCode` projected to `WireFault.ReadFailed`/`WriteRejected` at the boundary
- Reject: a hand-rolled Modbus frame codec, a boxed per-register allocation, or a FluentModbus-internal retry loop the `OutboundHop` breaker forecloses
