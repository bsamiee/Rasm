# [RASM_APPHOST_API_MODBUS]

`FluentModbus` is a managed Modbus TCP, RTU, and ASCII client/server library: a `ModbusTcpClient`/`ModbusRtuClient` connects to a Modbus device and reads or writes coil and register spaces through typed `Span<T>`-windowed function-code operations, with `ModbusEndianness` byte-order control and reason-coded faults, the certified managed owner the AppHost live-wire `modbus` and serial-Modbus transport rows bind behind the one `TransportRow` adapter.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentModbus`
- package: `FluentModbus`
- version: `5.3.2`
- license: `MIT`
- assembly: `FluentModbus`
- namespace: `FluentModbus`
- target: `netstandard2.1`; the `net10.0` consumer binds it
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
|  [01]   | `ModbusClient.ReadHoldingRegisters<T>(int unitIdentifier, int startingAddress, int count) where T : unmanaged` | read       | `Span<T>`     |
|  [02]   | `ModbusClient.ReadInputRegisters<T>(int unitIdentifier, int startingAddress, int count) where T : unmanaged` | read       | `Span<T>`     |
|  [03]   | `ModbusClient.WriteSingleRegister(int unitIdentifier, int registerAddress, short value)` (+ `ushort`/`byte[]` overloads) | write      | `void`        |
|  [04]   | `ModbusClient.WriteMultipleRegisters<T>(int unitIdentifier, int startingAddress, T[] dataset) where T : unmanaged` | write      | `void`        |
|  [05]   | `ModbusClient.ReadWriteMultipleRegisters<TRead,TWrite>(int unitIdentifier, int readStartingAddress, int readCount, int writeStartingAddress, TWrite[] dataset) where TRead,TWrite : unmanaged` | read+write | `Span<TRead>` |
|  [06]   | `ModbusClient.ReadCoils(int unitIdentifier, int startingAddress, int quantity)`            | read       | `Span<byte>`  |
|  [07]   | `ModbusClient.ReadDiscreteInputs(int unitIdentifier, int startingAddress, int quantity)`   | read       | `Span<byte>`  |
|  [08]   | `ModbusClient.WriteSingleCoil(int unitIdentifier, int registerAddress, bool value)`        | write      | `void`        |
|  [09]   | `ModbusClient.WriteMultipleCoils(int unitIdentifier, int startingAddress, bool[] values)`  | write      | `void`        |
|  [10]   | `ModbusClient.ReadHoldingRegisters(byte unitIdentifier, ushort startingAddress, ushort quantity)` | read | `Span<byte>` (raw non-generic) |

[ENTRYPOINT_SCOPE]: async operations
- rail: live-wire

| [INDEX] | [MEMBER]                                                                                                       | [KIND] | [RETURN]          |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :----- | :---------------- |
|  [01]   | `ModbusClient.ReadHoldingRegistersAsync<T>(int unitIdentifier, int startingAddress, int count, CancellationToken = default) where T : unmanaged` | read | `Task<Memory<T>>` |
|  [02]   | `ModbusClient.WriteMultipleRegistersAsync<T>(int unitIdentifier, int startingAddress, T[] dataset, CancellationToken = default) where T : unmanaged` | write | `Task` |
|  [03]   | `ModbusClient.ReadInputRegistersAsync<T>(int unitIdentifier, int startingAddress, int count, CancellationToken = default) where T : unmanaged` | read | `Task<Memory<T>>` |
|  [04]   | `ModbusClient.ReadWriteMultipleRegistersAsync<TRead,TWrite>(...)`                                               | read+write | `Task<Memory<TRead>>` |

## [04]-[IMPLEMENTATION_LAW]

[IMPLEMENTATION_LAW]: client semantics
- rail: live-wire

- `ModbusEndianness` is set at `Connect` and governs the byte order of every multi-byte register `T`; the AppHost binding carries it as a `TransportRow`/binding-spec policy column, never a per-read flag.
- the typed `ReadHoldingRegisters<T>` overload (`where T : unmanaged`) reinterprets the register window as `Span<T>` (`short`/`ushort`/`int`/`float`/`double`), so a register block decodes to one `ExternalValue` without manual byte juggling; the async mirror returns `Task<Memory<T>>` (the sync path is `Span<T>`, the async path `Memory<T>` because a span cannot cross an await). `WriteMultipleRegisters<T>` takes a `T[] dataset`, and the raw non-generic `byte`/`ushort` overloads carry the untyped register window.
- `unitId` is the Modbus slave address; `startingAddress`/`count` define the register window the binding spec carries.
- a protocol exception surfaces as `ModbusException` carrying a `ModbusExceptionCode`; the AppHost binding projects it to `WireFault.ReadFailed`/`WriteRejected` at the boundary, never propagating the exception into the interior.
- `ModbusRtuClient` binds over a serial port (the `System.IO.Ports` owner at `api-serialport.md`) for RTU/ASCII fieldbus, sharing the one register-operation surface with the TCP client.

[IMPLEMENTATION_LAW]: AppHost usage
- rail: live-wire

- the live-wire `modbus` transport row binds `ModbusTcpClient`/`ModbusRtuClient` behind the one `TransportRow.Read`/`Write` adapter; the register window read projects one `ExternalValue` (raw register value, declared unit, good flag, source instant) riding the row's `OutboundHop`.
- a register map (start address, count, endianness, unit id) is binding-spec policy data, never a parallel Modbus poller; the per-row retry is the `OutboundHop` breaker, never a FluentModbus retry loop.

[STACK]:
- transport axis: the `modbus` transport row is one `ExternalTransport` `[SmartEnum<string>]` case (`Wire/livewire#TRANSPORT_AXIS`) with `ReadShape.Poll`/`Writable: true`; `ModbusTcpClient` binds an `OutboundHop.ServerStream`/direct-TCP hop, `ModbusRtuClient` an `OutboundHop.CompanionSpawn` over the serial owner (`api-serialport.md`).
- value projection: a typed `Span<T>` register read decodes to one `ExternalValue` (raw value, declared unit, good flag, source instant) at the boundary; the boxed register never enters the interior.
- fault seam: a `ModbusException`/`ModbusExceptionCode` projects to `WireFault.ReadFailed`/`WriteRejected` at the boundary, folded through the live-wire registry band, never propagating into the interior.
