# [RASM_APPHOST_API_MODBUS]

`FluentModbus` owns managed Modbus TCP, RTU, and ASCII client transport through one `ModbusClient` register and coil surface windowed as typed `Span<T>`. `ModbusEndianness` fixes register byte order at `Connect`, a `ModbusException` carries the reason-coded protocol fault, and every client folds behind the AppHost live-wire `TransportRow` adapter.

## [01]-[PUBLIC_TYPES]

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

## [02]-[ENTRYPOINTS]

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
|  [05]   | `WriteSingleRegisterAsync(int, int, short, CancellationToken) -> Task`              | instance | await-safe one-register write    |
|  [06]   | `ReadCoilsAsync(int, int, int, CancellationToken) -> Task<Memory<byte>>`            | instance | await-safe coil read, bit-packed |
|  [07]   | `ReadDiscreteInputsAsync(int, int, int, CancellationToken) -> Task<Memory<byte>>`   | instance | await-safe discrete-input read   |
|  [08]   | `WriteSingleCoilAsync(int, int, bool, CancellationToken) -> Task`                   | instance | await-safe one-coil write        |
|  [09]   | `WriteMultipleCoilsAsync(int, int, bool[], CancellationToken) -> Task`              | instance | await-safe coil block write      |

- `WriteSingleRegisterAsync` also carries `ushort` and `byte[]` overloads mirroring its synchronous family; the `short` form is function 06, distinct from a one-element function-16 block.
- `ReadCoilsAsync`/`ReadDiscreteInputsAsync` return one bit per point packed into bytes low-bit-first, so a single-point window reads the low bit of the first byte; input registers and discrete inputs are read-only by protocol and expose no write member.

[ENTRYPOINT_SCOPE]: fault surface

`ModbusException : Exception` carries `public ModbusExceptionCode ExceptionCode { get; }`, raised by `internal void ModbusClient.ProcessError(ModbusFunctionCode, ModbusExceptionCode)` as a pure table dispatch. Every write member returns a bare `Task` or `void`, so this exception is the only carrier a refusal reaches a caller through. `ModbusExceptionCode` derives `byte`.

| [INDEX] | [MEMBER]                             | [VALUE] | [MEANING]                                   |
| :-----: | :----------------------------------- | :-----: | :------------------------------------------ |
|  [01]   | `OK`                                 |    0    | no protocol fault                           |
|  [02]   | `IllegalFunction`                    |    1    | function code unsupported by the server     |
|  [03]   | `IllegalDataAddress`                 |    2    | address window outside the server map       |
|  [04]   | `IllegalDataValue`                   |    3    | value outside the register's admitted range |
|  [05]   | `ServerDeviceFailure`                |    4    | unrecoverable server-side fault             |
|  [06]   | `Acknowledge`                        |    5    | request accepted, completion deferred       |
|  [07]   | `ServerDeviceBusy`                   |    6    | request declined, re-offer admitted         |
|  [08]   | `MemoryParityError`                  |    8    | parity fault reading extended memory        |
|  [09]   | `GatewayPathUnavailable`             |   10    | gateway cannot route to the target          |
|  [10]   | `GatewayTargetDeviceFailedToRespond` |   11    | gateway routed, the target stayed silent    |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ModbusEndianness` fixes at `Connect` — the overload sets the client's byte-swap state for the whole connection — and governs the byte order of every multi-byte register `T`; the AppHost binding carries it on the held-client composition seat that dialed the connection, never a window column or a per-read flag.
- Modbus publishes NO per-write echo token: every write member returns a bare `Task` and every read a raw register or bit window carrying no id, stamp, or sequence, so a written value is provable only by reading it back — `ReadWriteMultipleRegisters` (function 23) is the one atomic write-then-read-back in a single transaction, a correlation by construction rather than a token. AppHost's echo axis takes its `Absent` arm on this row for that reason.
- `ReadHoldingRegisters<T>` reinterprets each `T : unmanaged` register window as a `Span<T>` over `short`, `ushort`, `int`, `float`, or `double`, decoded to one `ExternalValue`; the async mirror returns `Task<Memory<T>>` for storage that outlives `await`.
- `unitIdentifier` is the slave address and the address window (`startingAddress`, `count`) is binding-spec policy data, never a parallel poller.
- one `ModbusClient` register and coil surface serves the TCP, RTU, and RTU-over-TCP transports alike.
- `ModbusException` carrying a `ModbusExceptionCode` projects to `WireFault.ReadFailed`/`WriteRejected` at the boundary, never propagating into the interior.
- TRAP: the message-only `ModbusException(string)` constructor emits `ldc.i4.1; sub; conv.u1` over a zero backing field, so a TRANSPORT-level fault — an invalid protocol identifier, an invalid response function code — carries `ExceptionCode = (ModbusExceptionCode)255`, an unnamed sentinel meaning "not a protocol code". Reading `ExceptionCode` without that guard mis-routes a framing fault as a device refusal.
- TRAP: `Acknowledge` (5) and `ServerDeviceBusy` (6) are DEFERRED-ACCEPTANCE codes, not refusals, so folding them into a rejection arm loses the re-offer class the server asked for.

[STACKING]:
- `System.IO.Ports`(`.api/api-serialport.md`): `ModbusRtuClient` binds a `SerialPort` line for RTU/ASCII fieldbus, its `Parity`/`StopBits`/`Handshake` line policy carried by that owner.
- within-lib: the live-wire `modbus` transport row composes the base `ModbusClient` surface deliberately — the TCP/RTU clients inherit the function-code operations, so one adapter binds both — behind one `TransportRow.Read`/`Write` adapter, a typed `Span<T>` read projecting one `ExternalValue` (raw value, declared unit, good flag, source instant) onto the row's `OutboundHop`, the boxed register never entering the interior.

[LOCAL_ADMISSION]:
- `ModbusTcpClient` binds an `OutboundHop.ServerStream` direct-TCP hop and `ModbusRtuClient` an `OutboundHop.CompanionSpawn` over the serial owner; the `modbus` transport row is one `ExternalTransport` `[SmartEnum<string>]` case with `ReadShape.Poll` and `Writable: true`, its register map binding-spec policy carried on the row.
