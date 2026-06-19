# [RASM_APPHOST_API_MODBUS]

`FluentModbus` is a managed Modbus TCP, RTU, and ASCII client/server library: a `ModbusTcpClient`/`ModbusRtuClient` connects to a Modbus device and reads or writes coil and register spaces through typed `Span<T>`-windowed function-code operations, with `ModbusEndianness` byte-order control and reason-coded faults, the certified managed owner the AppHost live-wire `modbus` and serial-Modbus transport rows bind behind the one `TransportRow` adapter.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentModbus`
- package: `FluentModbus`
- assembly: `FluentModbus`
- namespace: `FluentModbus`
- asset: runtime library
- rail: live-wire

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and base surfaces
- rail: live-wire

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                                        |
| :-----: | :---------------- | :------------ | :-------------------------------------------- |
|  [01]   | `ModbusClient`    | abstract base | shared read/write function-code operations    |
|  [02]   | `ModbusTcpClient` | client        | Modbus-TCP transport over a socket            |
|  [03]   | `ModbusRtuClient` | client        | Modbus-RTU/ASCII transport over a serial port |
|  [04]   | `ModbusTcpServer` | server        | in-process Modbus-TCP server (test/loopback)  |
|  [05]   | `ModbusRtuServer` | server        | in-process Modbus-RTU server                  |

[PUBLIC_TYPE_SCOPE]: policy and fault surfaces
- rail: live-wire

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :---------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `ModbusEndianness`      | enum          | `LittleEndian`/`BigEndian` register byte order |
|  [02]   | `ModbusFrameError`      | enum          | frame-level error classification               |
|  [03]   | `ModbusExceptionCode`   | enum          | Modbus protocol exception codes                |
|  [04]   | `ModbusException`       | exception     | reason-coded protocol fault                    |
|  [05]   | `ModbusRtuSerialFormat` | enum/options  | baud, parity, stop-bit RTU line format         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection lifecycle
- rail: live-wire

| [INDEX] | [MEMBER]                                                 | [KIND]      | [RETURN]   |
| :-----: | :------------------------------------------------------- | :---------- | :--------- |
|  [01]   | `ModbusTcpClient.Connect(IPEndPoint, ModbusEndianness)`  | client call | `void`     |
|  [02]   | `ModbusTcpClient.Connect(IPAddress, ModbusEndianness)`   | client call | `void`     |
|  [03]   | `ModbusRtuClient.Connect(string port, ModbusEndianness)` | client call | `void`     |
|  [04]   | `ModbusClient.Disconnect()`                              | client call | `void`     |
|  [05]   | `ModbusTcpClient.ConnectTimeout`                         | property    | `int` (ms) |
|  [06]   | `ModbusTcpClient.ReadTimeout` / `WriteTimeout`           | property    | `int` (ms) |

[ENTRYPOINT_SCOPE]: register and coil operations
- rail: live-wire

| [INDEX] | [MEMBER]                                                                                   | [KIND]     | [RETURN]      |
| :-----: | :----------------------------------------------------------------------------------------- | :--------- | :------------ |
|  [01]   | `ModbusClient.ReadHoldingRegisters<T>(int unitId, int startingAddress, int count)`         | read       | `Span<T>`     |
|  [02]   | `ModbusClient.ReadInputRegisters<T>(int unitId, int startingAddress, int count)`           | read       | `Span<T>`     |
|  [03]   | `ModbusClient.WriteSingleRegister(int unitId, int registerAddress, short value)`           | write      | `void`        |
|  [04]   | `ModbusClient.WriteMultipleRegisters<T>(int unitId, int startingAddress, ReadOnlySpan<T>)` | write      | `void`        |
|  [05]   | `ModbusClient.ReadWriteMultipleRegisters<TRead,TWrite>(...)`                               | read+write | `Span<TRead>` |
|  [06]   | `ModbusClient.ReadCoils(int unitId, int startingAddress, int count)`                       | read       | `Span<byte>`  |
|  [07]   | `ModbusClient.ReadDiscreteInputs(int unitId, int startingAddress, int count)`              | read       | `Span<byte>`  |
|  [08]   | `ModbusClient.WriteSingleCoil(int unitId, int registerAddress, bool value)`                | write      | `void`        |
|  [09]   | `ModbusClient.WriteMultipleCoils(int unitId, int startingAddress, bool[] values)`          | write      | `void`        |

[ENTRYPOINT_SCOPE]: async operations
- rail: live-wire

| [INDEX] | [MEMBER]                                                                                                       | [KIND] | [RETURN]          |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :----- | :---------------- |
|  [01]   | `ModbusClient.ReadHoldingRegistersAsync<T>(int unitId, int startingAddress, int count, CancellationToken)`     | read   | `Task<Memory<T>>` |
|  [02]   | `ModbusClient.WriteMultipleRegistersAsync<T>(int unitId, int startingAddress, T[] dataset, CancellationToken)` | write  | `Task`            |

## [04]-[IMPLEMENTATION_LAW]

[IMPLEMENTATION_LAW]: client semantics
- rail: live-wire

- `ModbusEndianness` is set at `Connect` and governs the byte order of every multi-byte register `T`; the AppHost binding carries it as a `TransportRow`/binding-spec policy column, never a per-read flag.
- the typed `ReadHoldingRegisters<T>` overload reinterprets the register window as `Span<T>` (`short`/`ushort`/`int`/`float`/`double`), so a register block decodes to one `ExternalValue` without manual byte juggling.
- `unitId` is the Modbus slave address; `startingAddress`/`count` define the register window the binding spec carries.
- a protocol exception surfaces as `ModbusException` carrying a `ModbusExceptionCode`; the AppHost binding projects it to `WireFault.ReadFailed`/`WriteRejected` at the boundary, never propagating the exception into the interior.
- `ModbusRtuClient` binds over a serial port (the `System.IO.Ports` owner at `api-serialport.md`) for RTU/ASCII fieldbus, sharing the one register-operation surface with the TCP client.

[IMPLEMENTATION_LAW]: AppHost usage
- rail: live-wire

- the live-wire `modbus` transport row binds `ModbusTcpClient`/`ModbusRtuClient` behind the one `TransportRow.Read`/`Write` adapter; the register window read projects one `ExternalValue` (raw register value, declared unit, good flag, source instant) riding the row's `OutboundHop`.
- a register map (start address, count, endianness, unit id) is binding-spec policy data, never a parallel Modbus poller; the per-row retry is the `OutboundHop` breaker, never a FluentModbus retry loop.

## [05]-[RESEARCH]

- [GENERIC_SPAN_ARITY]: the exact generic arity and `Span<T>`-versus-`Memory<T>` return shape of `ReadHoldingRegisters<T>`/`WriteMultipleRegisters<T>`/`ReadWriteMultipleRegisters<TRead,TWrite>` and the async `*Async<T>` overloads confirm against the pinned `FluentModbus` assembly at admission through the assay binder; the synchronous reads return `Span<T>` and the async reads return `Task<Memory<T>>` per the package surface, the precise overload set the catalogue settles when the manifest row lands.
