# [RASM_APPHOST_API_MODBUS]

`FluentModbus` owns managed Modbus TCP, RTU, and ASCII client/server transport through typed, `Span<T>`-windowed coil and register operations. `ModbusEndianness` governs byte order, reason-coded faults cross the live-wire seam, and the `modbus` and serial-Modbus transport rows bind through one `TransportRow` adapter.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentModbus`
- package: `FluentModbus`
- license: `MIT`
- assembly: `FluentModbus`
- namespace: `FluentModbus`
- target: `netstandard2.1`; the `net10.0` consumer binds it
- asset: runtime library
- rail: live-wire

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and base surfaces
- rail: live-wire

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :---------------- | :------------ | :-------------------------------------------- |
|  [01]   | `ModbusClient`    | abstract base | shared read/write function-code operations    |
|  [02]   | `ModbusTcpClient` | client        | Modbus-TCP transport over a socket            |
|  [03]   | `ModbusRtuClient` | client        | Modbus-RTU/ASCII transport over a serial port |
|  [04]   | `ModbusTcpServer` | server        | in-process Modbus-TCP server (test/loopback)  |
|  [05]   | `ModbusRtuServer` | server        | in-process Modbus-RTU server                  |

[PUBLIC_TYPE_SCOPE]: policy and fault surfaces
- rail: live-wire

`ModbusEndianness` selects `LittleEndian` or `BigEndian` register order, and `ModbusRtuSerialFormat` governs baud, parity, and stop-bit settings.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :---------------------- | :------------ | :------------------------------- |
|  [01]   | `ModbusEndianness`      | enum          | register byte-order policy       |
|  [02]   | `ModbusFrameError`      | enum          | frame-level error classification |
|  [03]   | `ModbusExceptionCode`   | enum          | Modbus protocol exception codes  |
|  [04]   | `ModbusException`       | exception     | reason-coded protocol fault      |
|  [05]   | `ModbusRtuSerialFormat` | enum/options  | RTU line format                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection lifecycle
- rail: live-wire

Timeout properties use milliseconds.

| [INDEX] | [MEMBER]                                                 | [KIND]      | [TYPE] |
| :-----: | :------------------------------------------------------- | :---------- | :----- |
|  [01]   | `ModbusTcpClient.Connect(IPEndPoint, ModbusEndianness)`  | client call | `void` |
|  [02]   | `ModbusTcpClient.Connect(IPAddress, ModbusEndianness)`   | client call | `void` |
|  [03]   | `ModbusRtuClient.Connect(string port, ModbusEndianness)` | client call | `void` |
|  [04]   | `ModbusClient.Disconnect()`                              | client call | `void` |
|  [05]   | `ModbusTcpClient.ConnectTimeout`                         | property    | `int`  |
|  [06]   | `ModbusTcpClient.ReadTimeout`                            | property    | `int`  |
|  [07]   | `ModbusTcpClient.WriteTimeout`                           | property    | `int`  |

[ENTRYPOINT_SCOPE]: register and coil operations
- rail: live-wire

`ModbusClient` owns register and coil operations through shared unit, address-window, and payload parameters.

| [INDEX] | [MEMBER]                                    | [KIND]   | [RETURN]      |
| :-----: | :------------------------------------------ | :------- | :------------ |
|  [01]   | `ReadHoldingRegisters<T>`                   | read     | `Span<T>`     |
|  [02]   | `ReadInputRegisters<T>`                     | read     | `Span<T>`     |
|  [03]   | `WriteSingleRegister`                       | write    | `void`        |
|  [04]   | `WriteMultipleRegisters<T>`                 | write    | `void`        |
|  [05]   | `ReadWriteMultipleRegisters<TRead, TWrite>` | exchange | `Span<TRead>` |
|  [06]   | `ReadCoils`                                 | read     | `Span<byte>`  |
|  [07]   | `ReadDiscreteInputs`                        | read     | `Span<byte>`  |
|  [08]   | `WriteSingleCoil`                           | write    | `void`        |
|  [09]   | `WriteMultipleCoils`                        | write    | `void`        |
|  [10]   | `ReadHoldingRegisters`                      | raw read | `Span<byte>`  |

[READ_REGISTER_PARAMETERS]:
- Members: `ReadHoldingRegisters<T>`, `ReadInputRegisters<T>`
- Parameters: `int unitIdentifier`, `int startingAddress`, `int count`
- Constraint: `T : unmanaged`

[WRITE_REGISTER_PARAMETERS]:
- `WriteSingleRegister`: `int unitIdentifier`, `int registerAddress`, `short value`; overloads accept `ushort` and `byte[]` values.
- `WriteMultipleRegisters<T>`: `int unitIdentifier`, `int startingAddress`, `T[] dataset`; `T : unmanaged`.

[READ_WRITE_REGISTER_PARAMETERS]:
- Parameters: `int unitIdentifier`, `int readStartingAddress`, `int readCount`, `int writeStartingAddress`, `TWrite[] dataset`
- Constraints: `TRead : unmanaged`, `TWrite : unmanaged`

[COIL_PARAMETERS]:
- `ReadCoils` and `ReadDiscreteInputs`: `int unitIdentifier`, `int startingAddress`, `int quantity`.
- `WriteSingleCoil`: `int unitIdentifier`, `int registerAddress`, `bool value`.
- `WriteMultipleCoils`: `int unitIdentifier`, `int startingAddress`, `bool[] values`.

[RAW_REGISTER_PARAMETERS]:
- `ReadHoldingRegisters`: `byte unitIdentifier`, `ushort startingAddress`, `ushort quantity`.

[ENTRYPOINT_SCOPE]: async operations
- rail: live-wire

`ModbusClient` owns cancellation-aware asynchronous register operations and await-safe return storage.

| [INDEX] | [MEMBER]                                              | [KIND]   | [RETURN]              |
| :-----: | :---------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `ReadHoldingRegistersAsync<T>`                        | read     | `Task<Memory<T>>`     |
|  [02]   | `WriteMultipleRegistersAsync<T>`                      | write    | `Task`                |
|  [03]   | `ReadInputRegistersAsync<T>`                          | read     | `Task<Memory<T>>`     |
|  [04]   | `ReadWriteMultipleRegistersAsync<TRead, TWrite>(...)` | exchange | `Task<Memory<TRead>>` |

[ASYNC_READ_PARAMETERS]:
- Members: `ReadHoldingRegistersAsync<T>`, `ReadInputRegistersAsync<T>`
- Parameters: `int unitIdentifier`, `int startingAddress`, `int count`, `CancellationToken = default`
- Constraint: `T : unmanaged`

[ASYNC_WRITE_PARAMETERS]:
- Parameters: `int unitIdentifier`, `int startingAddress`, `T[] dataset`, `CancellationToken = default`
- Constraint: `T : unmanaged`

## [04]-[IMPLEMENTATION_LAW]

[IMPLEMENTATION_LAW]: client semantics
- rail: live-wire

- `ModbusEndianness` is set at `Connect` and governs the byte order of every multi-byte register `T`; the AppHost binding carries it as a `TransportRow`/binding-spec policy column, never a per-read flag.
- `unitId` is the Modbus slave address; `startingAddress`/`count` define the register window the binding spec carries.
- a protocol exception surfaces as `ModbusException` carrying a `ModbusExceptionCode`; the AppHost binding projects it to `WireFault.ReadFailed`/`WriteRejected` at the boundary, never propagating the exception into the interior.
- `ModbusRtuClient` binds over `System.IO.Ports` for RTU/ASCII fieldbus and shares one register-operation surface with the TCP client.

[TYPED_REGISTER_WINDOWS]:
- Sync: `ReadHoldingRegisters<T>` reinterprets each `T : unmanaged` register window as a `Span<T>` over `short`, `ushort`, `int`, `float`, or `double`, then decodes the window to one `ExternalValue`.
- Async: the async mirror returns `Task<Memory<T>>`, so register storage survives `await` while the synchronous path remains `Span<T>`.
- Write: `WriteMultipleRegisters<T>` takes a `T[] dataset`.
- Raw: the non-generic `byte` and `ushort` overloads carry untyped register windows.

[IMPLEMENTATION_LAW]: AppHost usage
- rail: live-wire

- the live-wire `modbus` transport row binds `ModbusTcpClient`/`ModbusRtuClient` behind the one `TransportRow.Read`/`Write` adapter; the register window read projects one `ExternalValue` (raw register value, declared unit, good flag, source instant) riding the row's `OutboundHop`.
- a register map (start address, count, endianness, unit id) is binding-spec policy data, never a parallel Modbus poller; the per-row retry is the `OutboundHop` breaker, never a FluentModbus retry loop.

[STACK]:
- transport axis: the `modbus` transport row is one `ExternalTransport` `[SmartEnum<string>]` case with `ReadShape.Poll` and `Writable: true`; `ModbusTcpClient` binds an `OutboundHop.ServerStream` direct-TCP hop, and `ModbusRtuClient` binds an `OutboundHop.CompanionSpawn` over `System.IO.Ports`.
- value projection: a typed `Span<T>` register read decodes to one `ExternalValue` (raw value, declared unit, good flag, source instant) at the boundary; the boxed register never enters the interior.
- fault seam: a `ModbusException`/`ModbusExceptionCode` projects to `WireFault.ReadFailed`/`WriteRejected` at the boundary, folded through the live-wire registry band, never propagating into the interior.
