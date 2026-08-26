# [RASM_APPHOST_API_BACNET]

`BACnet` (ela-compil `System.IO.BACnet`) owns managed building-automation capability: one `BacnetClient` bound to an `IBacnetTransport` discovers devices, reads and writes object properties, and subscribes to change-of-value pushes. Confirmed services ride awaitable `*Async` members that throw, a `Begin`/`End` pair returning refusal through `out Exception`, and typed client events carrying the device verdict as enums. AppHost binds the `bacnet` live-wire row behind one `TransportRow` adapter, decoding metered observations to `ExternalValue` for the twin-calibration lane.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client and transport surfaces

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :------------------------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `BacnetClient`                   | class         | discovery + confirmed/unconfirmed service requests over a transport    |
|  [02]   | `IBacnetTransport`               | interface     | `Start`/`Send`, `MessageRecieved` event, `MaxAdpuLength`, `Type`       |
|  [03]   | `BacnetTransportBase`            | abstract base | shared transport state every concrete transport extends                |
|  [04]   | `BacnetIpUdpProtocolTransport`   | class         | BACnet/IP over UDP on port 47808, the AppHost binding                  |
|  [05]   | `BacnetIpV6UdpProtocolTransport` | class         | BACnet/IPv6 over UDP                                                   |
|  [06]   | `BacnetMstpProtocolTransport`    | class         | MS/TP over a caller-supplied `IBacnetSerialTransport`                  |
|  [07]   | `BacnetPtpProtocolTransport`     | class         | point-to-point over a caller-supplied `IBacnetSerialTransport`         |
|  [08]   | `IBacnetSerialTransport`         | interface     | the serial line MS/TP and PTP consume, host-implemented, `IDisposable` |
|  [09]   | `BacnetLogging`                  | static class  | `Factory` + `CreateLogger<T>()` — the stack-wide `ILoggerFactory` seat |

[PUBLIC_TYPE_SCOPE]: address, object, and value surfaces

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `BacnetAddress`                  | class         | device network address (network number + MAC)                  |
|  [02]   | `BacnetObjectId`                 | struct        | object type + instance (`AnalogInput:0`, `Device:12345`)       |
|  [03]   | `BacnetPropertyIds`              | enum          | property identifier (`PROP_PRESENT_VALUE`, `PROP_OBJECT_NAME`) |
|  [04]   | `BacnetValue`                    | struct        | tagged value (`Tag` + boxed `Value`), the read/write unit      |
|  [05]   | `BacnetPropertyReference`        | struct        | property id + array index for multi-property reads             |
|  [06]   | `BacnetEventNotificationData`    | struct        | COV/alarm notification payload                                 |
|  [07]   | `BacnetWriteAccessSpecification` | struct        | object id + `ICollection<BacnetPropertyValue>` for batch write |

[PUBLIC_TYPE_SCOPE]: confirmed-request result carriers, each the awaited return of its `*Async` member

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]   | [CAPABILITY]                                                       |
| :-----: | :---------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `BacnetReadRangeResult`       | readonly struct | `Range` (raw trend bytes) + `ItemCount`                            |
|  [02]   | `BacnetReadFileResult`        | readonly struct | `Position`, `Count`, `EndOfFile`, `FileBuffer`, `FileBufferOffset` |
|  [03]   | `BacnetPrivateTransferResult` | readonly struct | `VendorId`, `ServiceNumber`, `ResultBlock`                         |

[PUBLIC_TYPE_SCOPE]: refusal carriers — the typed error surface the event callbacks and the `Begin`/`End` pair expose

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------ | :------------ | :-------------------------------------------- |
|  [01]   | `BacnetAsyncResult`                               | class         | `IAsyncResult` + `IDisposable` request handle |
|  [02]   | `ErrorHandler` / `AbortHandler` / `RejectHandler` | delegate      | typed refusal callbacks off the client        |
|  [03]   | `BacnetErrorClasses`                              | enum          | device error class, the `OnError` first half  |
|  [04]   | `BacnetErrorCodes`                                | enum          | device error code, the `OnError` second half  |
|  [05]   | `BacnetRejectReason`                              | enum (byte)   | APDU reject reason off `OnReject`             |
|  [06]   | `BacnetAbortReason`                               | enum          | transfer abort reason off `OnAbort`           |
|  [07]   | `Storage.DeviceStorage.ErrorCodes`                | enum          | server-side write verdict, negative-valued    |

[PUBLIC_TYPE_SCOPE]: schedule and calendar objects — the BACnet Schedule/Calendar encode-decode family

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :------------------------------ | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `BacnetCalendarEntry`           | class         | one date, `BacnetDateRange`, or `BacnetWeekNDay` period; `IsAFittingDate(DateTime)` |
|  [02]   | `BacnetWeekNDay`                | struct        | month + week-of-month + day-of-week recurrence; `IsAFittingDate(DateTime)`          |
|  [03]   | `BacnetDailySchedule`           | class         | `List<BacnetTimeValue> DaySchedule` — one weekday's setpoint ladder                 |
|  [04]   | `BacnetSpecialEvent`            | class         | a calendar entry or `CalendarReference` plus time values at `EventPriority`         |
|  [05]   | `BacnetTimeValue`               | struct        | `TimeSpan Time` + `BacnetValue Value` — one schedule transition                     |
|  [06]   | `BacnetTime` / `BacnetDateTime` | struct        | wire time and date-time primitives                                                  |
|  [07]   | `BacnetMonthOptions`            | enum          | month wildcard (`AnyMonth`) `BacnetWeekNDay` composes                               |
|  [08]   | `BacnetWeekOfMonthOptions`      | enum          | week-of-month wildcard (`AnyWeek`)                                                  |
|  [09]   | `BacnetDayOfWeekOptions`        | enum          | day-of-week wildcard                                                                |

- Every member of this family implements `ASN1.IEncode`/`ASN1.IDecode`, so a schedule reads and writes through the same `Encode(EncodeBuffer)`/`Decode(byte[], int, uint)` pair the property codec uses — no second serializer.

## [02]-[ENTRYPOINTS]

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
|  [10]   | `BacnetClient.OnPrivateTransfer`                             | event    | vendor-proprietary request callback       |
|  [11]   | `BacnetClient.Log`                                           | property | the instance `ILogger` override           |
|  [12]   | `BacnetLogging.Factory`                                      | property | stack-wide `ILoggerFactory`, set at start |

- `BacnetLogging.Factory` defaults to `NullLoggerFactory.Instance`, so the stack logs nothing until the host assigns it; every `BacnetClient`, transport, and BVLC layer constructed AFTER the assignment reads it, and an instance already built keeps whatever `Log` it captured.

[ENTRYPOINT_SCOPE]: property read/write and COV

`BacnetClient` confirmed services all await: each is an instance member opening on `BacnetAddress`, trails `byte invokeId = 0` and `CancellationToken`, returns its decoded result, and THROWS on failure.

| [INDEX] | [SURFACE]                                                                                             | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------------------------------------- | :------------------------------- |
|  [01]   | `ReadPropertyAsync(BacnetObjectId, BacnetPropertyIds, uint) -> IList<BacnetValue>`                    | confirmed poll at an array index |
|  [02]   | `WritePropertyAsync(BacnetObjectId, BacnetPropertyIds, IEnumerable<BacnetValue>, byte?, uint)`        | confirmed priority-slot write    |
|  [03]   | `SubscribeCOVAsync(BacnetObjectId, uint, bool, bool, uint)`                                           | subscribe or cancel object COV   |
|  [04]   | `SubscribePropertyAsync(BacnetObjectId, BacnetPropertyReference, uint, bool, bool)`                   | subscribe or cancel property COV |
|  [05]   | `ReadRangeAsync(BacnetObjectId, DateTime, uint) -> BacnetReadRangeResult`                             | trend range read by time         |
|  [06]   | `ReadRangeAsync(BacnetObjectId, uint, uint) -> BacnetReadRangeResult`                                 | trend range read by position     |
|  [07]   | `ReadPropertyMultipleAsync(IList<BacnetReadAccessSpecification>) -> IList<BacnetReadAccessResult>`    | batched access-spec read         |
|  [08]   | `ReadPropertyMultipleAsync(BacnetObjectId, params BacnetPropertyIds[]) -> IList<BacnetPropertyValue>` | one object's property set        |
|  [09]   | `WritePropertyMultipleAsync(BacnetObjectId, ICollection<BacnetPropertyValue>)`                        | batched write on one object      |
|  [10]   | `WritePropertyMultipleAsync(ICollection<BacnetReadAccessResult>)`                                     | batched write across objects     |

- `WritePropertyAsync(BacnetAddress adr, BacnetObjectId objectId, BacnetPropertyIds propertyId, IEnumerable<BacnetValue> valueList, byte invokeId = 0, byte? priority = null, uint arrayIndex = uint.MaxValue, CancellationToken cancellationToken = default)` spells the write in full and returns a bare `Task` carrying no result.
- `WritePropertyAsync`'s `byte? priority` selects the BACnet command priority-array slot and throws `ArgumentOutOfRangeException` outside 1-16; a null value at a held slot is the RELEASE, so take-and-release is one member, and a priority-less write lands at the device default no later write can distinguish.
- `SubscribeCOVAsync`/`SubscribePropertyAsync` carry the unsubscribe on their `cancel` parameter, so subscribe and detach are one member, and `issueConfirmedNotifications` selects the confirmed versus unconfirmed notification service.
- `ReadPropertyMultipleAsync(BacnetAddress, BacnetObjectId, params BacnetPropertyIds[])` is the ONE member carrying no `CancellationToken`: its params array occupies the trailing slot, so it runs to the client's own timeout-and-retry bound and a caller needing cancellation takes the `IList<BacnetPropertyReference>` overload.
- Correlation is by invoke id alone, so many requests ride one `BacnetClient` concurrently with no caller lock, and a segmented response re-arms the timeout PER SEGMENT — the bound is per segment, never per transfer.

[ENTRYPOINT_SCOPE]: typed refusal events — the one un-stringified carrier

`ProcessError` decodes through `Services.DecodeError(buffer, offset, out errorClass, out errorCode)` and fires `OnError` carrying both enums intact, so a subscriber reads the device verdict as values. Every handler trails the raw `byte[] buffer, int offset, int length` frame beside its typed columns, and `OnError` also hands `BacnetPduTypes type`, `BacnetConfirmedServices service`, and `byte invokeId`.

| [INDEX] | [SURFACE]                           | [SHAPE] | [CAPABILITY]                                           |
| :-----: | :---------------------------------- | :------ | :----------------------------------------------------- |
|  [01]   | `OnError -> ErrorHandler`           | event   | `BacnetErrorClasses` + `BacnetErrorCodes`, both intact |
|  [02]   | `OnReject -> RejectHandler`         | event   | `BacnetRejectReason` off the rejected APDU             |
|  [03]   | `OnAbort -> AbortHandler`           | event   | `BacnetAbortReason` off the aborted transfer           |
|  [04]   | `OnSimpleAck -> SimpleAckHandler`   | event   | acknowledgement carrying no result body                |
|  [05]   | `OnComplexAck -> ComplexAckHandler` | event   | acknowledgement carrying the encoded result            |
|  [06]   | `OnSegment -> SegmentHandler`       | event   | one segment of a segmented response                    |

[ENTRYPOINT_SCOPE]: returned-refusal `Begin`/`End` pair

Beneath every `*Async` member sits a `Begin`/`End` pair reporting refusal as a returned value rather than a throw.

`out Exception ex` is the uniform shape across `EndReadPropertyRequest`, `EndReadPropertyMultipleRequest`, `EndWritePropertyRequest`, `EndCreateObjectRequest`, `EndDeleteObjectRequest`, `EndSubscribeCOVRequest`, `EndReadFileRequest`, `EndWriteFileRequest`, `EndReadRangeRequest`, `EndPrivateTransferRequest`, `EndAddListElementRequest`, `EndDeviceCommunicationControlRequest`, `EndRawEncodedDecodedPropertyConfirmedRequest`; the write row below stands for the family.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `BeginWritePropertyRequest(...) -> BacnetAsyncResult`      | instance | issue the APDU without awaiting |
|  [02]   | `EndWritePropertyRequest(IAsyncResult, out Exception ex)`  | instance | null `ex` is acceptance         |
|  [03]   | `WritePropertyRequest(...) -> bool`                        | instance | FALSE on retry exhaustion       |
|  [04]   | `BacnetAsyncResult.Error -> Exception`                     | property | the folded refusal, get and set |
|  [05]   | `BacnetAsyncResult.Result -> byte[]`                       | property | raw response body               |
|  [06]   | `BacnetAsyncResult.Segmented -> bool`                      | property | segmented-response flag         |
|  [07]   | `BacnetAsyncResult.Address -> BacnetAddress`               | property | the answering device            |
|  [08]   | `GetResultOrTimeout(int, CancellationToken) -> Task<bool>` | instance | await completion under a bound  |
|  [09]   | `Resend()`                                                 | instance | re-issue the pending APDU       |

[ENTRYPOINT_SCOPE]: device-refusal rosters

`BacnetErrorClasses` prefixes every member `ERROR_CLASS_` and bounds the space with `MAX_BACNET_ERROR_CLASS = 8`, `ERROR_CLASS_PROPRIETARY_FIRST = 64`, and `ERROR_CLASS_PROPRIETARY_LAST = 65535`; rows below drop the shared prefix.

| [INDEX] | [MEMBER]        | [VALUE] | [MEANING]                                          |
| :-----: | :-------------- | :-----: | :------------------------------------------------- |
|  [01]   | `DEVICE`        |    0    | device-level refusal                               |
|  [02]   | `OBJECT`        |    1    | object-level refusal                               |
|  [03]   | `PROPERTY`      |    2    | property-level refusal, the write path's own class |
|  [04]   | `RESOURCES`     |    3    | device resource exhaustion                         |
|  [05]   | `SECURITY`      |    4    | security refusal                                   |
|  [06]   | `SERVICES`      |    5    | service-level refusal                              |
|  [07]   | `VT`            |    6    | virtual-terminal refusal                           |
|  [08]   | `COMMUNICATION` |    7    | communication-layer refusal                        |

`BacnetErrorCodes` prefixes every member `ERROR_CODE_` and runs 206 names wide (`MAX_BACNET_ERROR_CODE = 206`), reserved through `ERROR_CODE_RESERVED_MAX = 255` and vendor-proprietary from `ERROR_CODE_PROPRIETARY_FIRST = 256` to `ERROR_CODE_PROPRIETARY_LAST = 65535`. Write-relevant members land below under the same dropped prefix.

| [INDEX] | [MEMBER]                               | [VALUE] | [MEANING]                                   |
| :-----: | :------------------------------------- | :-----: | :------------------------------------------ |
|  [01]   | `OTHER`                                |    0    | unclassified refusal                        |
|  [02]   | `DEVICE_BUSY`                          |    3    | device declines while busy                  |
|  [03]   | `INVALID_DATA_TYPE`                    |    9    | value tag mismatches the property           |
|  [04]   | `NO_SPACE_TO_WRITE_PROPERTY`           |   20    | no storage for the written value            |
|  [05]   | `READ_ACCESS_DENIED`                   |   27    | property refuses the read                   |
|  [06]   | `UNKNOWN_OBJECT`                       |   31    | object id resolves to nothing               |
|  [07]   | `UNKNOWN_PROPERTY`                     |   32    | property id resolves to nothing             |
|  [08]   | `VALUE_OUT_OF_RANGE`                   |   37    | value outside the property range            |
|  [09]   | `WRITE_ACCESS_DENIED`                  |   40    | property refuses the write                  |
|  [10]   | `INVALID_ARRAY_INDEX`                  |   42    | array index outside the property            |
|  [11]   | `OPTIONAL_FUNCTIONALITY_NOT_SUPPORTED` |   45    | service is optional and absent              |
|  [12]   | `PROPERTY_IS_NOT_AN_ARRAY`             |   50    | array index on a scalar property            |
|  [13]   | `COMMUNICATION_DISABLED`               |   83    | `DeviceCommunicationControl` silenced it    |
|  [14]   | `SUCCESS`                              |   84    | acceptance spelled as a code                |
|  [15]   | `ACCESS_DENIED`                        |   85    | access refusal carrying no narrower class   |
|  [16]   | `VALUE_TOO_LONG`                       |   134   | encoded value exceeds the property          |
|  [17]   | `INVALID_VALUE_IN_THIS_STATE`          |   138   | value inadmissible at the current state     |
|  [18]   | `INVALID_OPERATION_IN_THIS_STATE`      |   139   | operation inadmissible at the current state |

[ENTRYPOINT_SCOPE]: APDU and server-side refusal rosters

`BacnetRejectReason` and `BacnetAbortReason` share one sequential value space, so one table carries both rosters whole under the `[REJECT]` and `[ABORT]` columns.

| [INDEX] | [VALUE] | [REJECT]                      | [ABORT]                             |
| :-----: | :-----: | :---------------------------- | :---------------------------------- |
|  [01]   |    0    | `OTHER`                       | `OTHER`                             |
|  [02]   |    1    | `BUFFER_OVERFLOW`             | `BUFFER_OVERFLOW`                   |
|  [03]   |    2    | `INCONSISTENT_PARAMETERS`     | `INVALID_APDU_IN_THIS_STATE`        |
|  [04]   |    3    | `INVALID_PARAMETER_DATA_TYPE` | `PREEMPTED_BY_HIGHER_PRIORITY_TASK` |
|  [05]   |    4    | `INVALID_TAG`                 | `SEGMENTATION_NOT_SUPPORTED`        |
|  [06]   |    5    | `MISSING_REQUIRED_PARAMETER`  | `SECURITY_ERROR`                    |
|  [07]   |    6    | `PARAMETER_OUT_OF_RANGE`      | `INSUFFICIENT_SECURITY`             |
|  [08]   |    7    | `TOO_MANY_ARGUMENTS`          | `WINDOW_SIZE_OUT_OF_RANGE`          |
|  [09]   |    8    | `UNDEFINED_ENUMERATION`       | `APPLICATION_EXCEEDED_REPLY_TIME`   |
|  [10]   |    9    | `UNRECOGNIZED_SERVICE`        | `OUT_OF_RESOURCES`                  |
|  [11]   |   10    |                               | `TSM_TIMEOUT`                       |
|  [12]   |   11    |                               | `APDU_TOO_LONG`                     |

`Storage.DeviceStorage` rides its own negative-valued verdict on `WriteOverrideHandler(..., out ErrorCodes status, out bool handled)`, the server-side counterpart to the client API above.

| [INDEX] | [MEMBER]            | [VALUE] | [MEANING]                        |
| :-----: | :------------------ | :-----: | :------------------------------- |
|  [01]   | `Good`              |    0    | write accepted                   |
|  [02]   | `GenericError`      |   -1    | unclassified server refusal      |
|  [03]   | `NotExist`          |   -2    | addressed slot holds nothing     |
|  [04]   | `NotForMe`          |   -3    | request addresses another device |
|  [05]   | `WriteAccessDenied` |   -4    | property refuses the write       |
|  [06]   | `UnknownObject`     |   -5    | object id resolves to nothing    |
|  [07]   | `UnknownProperty`   |   -6    | property id resolves to nothing  |

[ENTRYPOINT_SCOPE]: object lifecycle, alarm, file, and vendor services — same awaited shape and throw contract, every member an instance call opening on `BacnetAddress`

| [INDEX] | [SURFACE]                                                                                    | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `CreateObjectAsync(BacnetObjectId, ICollection<BacnetPropertyValue>)`                        | create a device object                   |
|  [02]   | `DeleteObjectAsync(BacnetObjectId)`                                                          | delete a device object                   |
|  [03]   | `{Add, Remove}ListElementAsync(BacnetObjectId, BacnetPropertyReference, IList<BacnetValue>)` | mutate a list-valued property            |
|  [04]   | `GetAlarmSummaryOrEventAsync(bool) -> IList<BacnetGetEventInformationData>`                  | alarm summary or event information       |
|  [05]   | `AlarmAcknowledgementAsync(BacnetObjectId, BacnetEventStates, string, BacnetGenericTime ×2)` | acknowledge an alarm                     |
|  [06]   | `SendConfirmedEventNotificationAsync(BacnetEventNotificationData)`                           | push a confirmed event notification      |
|  [07]   | `NotifyAsync(uint, uint, BacnetObjectId, uint, bool, IList<BacnetPropertyValue>)`            | issue a COV notification as server       |
|  [08]   | `ReadFileAsync(BacnetObjectId, int, uint) -> BacnetReadFileResult`                           | atomic file read                         |
|  [09]   | `WriteFileAsync(BacnetObjectId, int, int, byte[]) -> int`                                    | atomic file write                        |
|  [10]   | `PrivateTransferAsync(uint, uint, byte[]) -> BacnetPrivateTransferResult`                    | vendor-proprietary confirmed transfer    |
|  [11]   | `ReinitializeAsync(BacnetReinitializedStates, string)`                                       | device reinitialize under a password     |
|  [12]   | `DeviceCommunicationControlAsync(uint, uint, string)`                                        | enable/disable device communication      |
|  [13]   | `LifeSafetyOperationAsync(BacnetObjectId, string, BacnetLifeSafetyOperations)`               | life-safety command                      |
|  [14]   | `RawEncodedDecodedPropertyConfirmedAsync(…)`                                                 | raw-APDU escape for an uncovered service |

- `RawEncodedDecodedPropertyConfirmedAsync(BacnetObjectId, BacnetPropertyIds, BacnetConfirmedServices, byte[]) -> byte[]` carries the escape signature in full.

[ENTRYPOINT_SCOPE]: trend-log decode

| [INDEX] | [SURFACE]                                                                          | [SHAPE] | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------------------------------- | :------ | :------------------------------- |
|  [01]   | `Serialize.Services.DecodeLogRecord(byte[], int, int, int, out BacnetLogRecord[])` | static  | lift a trend buffer into records |
|  [02]   | `BacnetLogRecord(BacnetTrendLogValueType, object, DateTime, uint)`                 | ctor    | one decoded log sample           |

- `BacnetLogRecord` is a struct carrying `DateTime timestamp`, `BacnetTrendLogValueType type`, the `object Value` its type column decodes (`TL_TYPE_ANY`/`BITS`/`BOOL`/`DELTA`/`ENUM`/`ERROR`/`REAL`/`SIGN`/`STATUS`/`UNSIGN`), and `BacnetStatusFlags statusFlags`; `GetValue<T>()` is the typed read off the boxed value, and the ctor is `BacnetLogRecord(BacnetTrendLogValueType, object, DateTime, BacnetStatusFlags)`.
- `DecodeLogRecord` returns the consumed byte count and fills `nCurves` records, so the `BacnetReadRangeResult.Range` a `ReadRangeAsync` produces decodes with no second parser and its `ItemCount` bounds the record count.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `BacnetClient` binds one `IBacnetTransport` and is `IDisposable`; the AppHost binding holds it in the token-gated state cell the OPC-UA/MQTT/serial clients share, so a reconnect replaces the whole cell.
- Read shape is dual: `ReadPropertyAsync` is the confirmed poll path awaiting `IList<BacnetValue>`, and `SubscribeCOVAsync` with the `OnCOVNotification` event is the push path — COV binds metered points, poll binds on-demand reads.
- One property read projects to one `ExternalValue` (raw value, declared unit from `PROP_UNITS`, good flag from the read status, source instant); the boxed `BacnetValue` tag never enters the interior.
- Failure reaches a caller on three paths, and only the `*Async` path stringifies. `OnError`/`OnReject`/`OnAbort` hand `BacnetErrorClasses`, `BacnetErrorCodes`, `BacnetRejectReason`, and `BacnetAbortReason` as values; the `Begin`/`End` pair returns its refusal through `out Exception ex`, null meaning acceptance; the `*Async` members THROW under a BCL-only vocabulary.
- `BacnetAsyncResult`'s constructor subscribes those same events and folds each verdict to text — `Error = new Exception($"Error from device: {errorClass} - {errorCode}")`, `$"Reject from device, reason: {reason}"`, `$"Abort from device, reason: {reason}"` — and `Resend()` sets `new IOException("Write Timeout")` or `new Exception("Write Exception: " + ex.Message)`, so the awaited path inherits a message where the event path held two enums.
- `*Async` throw vocabulary: a device Error, Reject, or Abort surfaces as a bare `Exception` carrying only the folded message, an exhausted retry raises `TimeoutException` except on `ReadPropertyAsync` and the params `ReadPropertyMultipleAsync` which raise a bare `Exception`, and a tripped token raises `OperationCanceledException`.
- Every awaited call sits inside one boundary catch projecting to `WireFault.ReadFailed`/`WriteRejected`; a leg needing the error class and code as values subscribes `OnError` beside the await rather than parsing the message.
- TRAP: `SendRequestAsync` answers TRUE when the `Error` setter calls `MarkDone()`, so TRUE never means success on the `Begin`/`End` path — the `out Exception` reads on every path.
- Cancellation is the caller's `CancellationToken` on the same member, never a second timeout knob beside the client's `timeout`/`retries` construction pair.
- `BacnetIpUdpProtocolTransport.Start()` throws `InvalidOperationException` listing the candidates when several IPv4 interfaces exist and none was named, so a multi-homed host pins `localEndpointIp` at construction; the ctor also carries `maxApdu` and `dontFragment`, and `ReceiveBufferSize` (1 MB) is settable after `Start()`.
- TRAP: `RegisterAsForeignDevice` routes BBMD networks off the local broadcast domain and returns `void`, logging its own transport mismatch — a non-IP transport is a log line, never a thrown fault the caller can branch on.
- `OnCOVNotification`/`OnIam` fire on a transport thread; the handler decodes the notification and `TryWrite`s one `ExternalValue` into the bounded lane at the boundary, never running the interior on the callback thread.
- Logging is `Microsoft.Extensions.Logging`: `BacnetLogging.Factory` seats one `ILoggerFactory` for every stack object built after it, and `BacnetClient.Log` overrides one instance.

[STACKING]:
- `api-serialport.md`(`System.IO.Ports`): the package carries NO serial line of its own — `BacnetMstpProtocolTransport(IBacnetSerialTransport, short, byte, byte)` and `BacnetPtpProtocolTransport` take the line as a host-implemented `IBacnetSerialTransport`, so the MS/TP adapter wraps the same `SerialPort` the serialport owner opens under its `SerialPort.BaudRate`/`Parity`/`RtsEnable` line policy, one line owner serving both the `serial` row and the MS/TP row.
- `api-serilog-hosting.md`: `BacnetLogging.Factory` takes the host's composed `ILoggerFactory`, so BACnet transport and BVLC diagnostics land on the one host log pipeline rather than a package-private sink.
- `api-mtconnect.md`: building-automation observations decode at this adapter exactly as MTConnect machine-tool observations feed Fabrication — one decode boundary, the observation crossing as a wire row.
- within-lib: the `bacnet` row is one `ExternalTransport` `[SmartEnum<string>]` case with its `TransportRow` (`ReadShape.Subscribe`, `Writable: true`, an `OutboundHop` hop) and one `LiveClient` case wrapping `BacnetClient`, no bespoke poller beyond the client's confirmed-request retry.
- `SubscribeCOV` establishes a device-side subscription pushing without a client cadence, so the row's steady-state read is push and `ReadPropertyAsync` serves the stale-lane fallback alone, never a declared cadence.

[SERIAL_LINE_CONTRACT]:
- `IBacnetSerialTransport : IDisposable` is five members — `int BytesToRead { get; }`, `void Open()`, `void Close()`, `int Read(byte[] buffer, int offset, int length, int timeoutMs)`, `void Write(byte[] buffer, int offset, int length)`.
- `Read`'s return is a sentinel dispatch, never a plain count: exactly `-110` (negative ETIMEDOUT; the transport's own `ETIMEDOUT = 110` const) reads as MS/TP `Timeout`/`SubTimeout` — the arm a sole master claims the token through — while any OTHER negative reads `ConnectionError` and `0` reads `ConnectionClose`, and BOTH of those log and re-enter the master loop without stopping it, so a host line answering a dead or idle bus with `0` spins a `ThreadPriority.Highest` thread at full burn; a benign timeout AND every port fault therefore answer `-110`.
- Partial returns are legal — the 8-byte header loop and the body loop both accumulate offsets, and a started frame collapses the wait to the 80 ms inter-character gap — and `timeoutMs` is per call, so a `SerialPort`-backed line re-arms `ReadTimeout` on each entry.
- Nothing catches around `Read`/`Write`: a thrown line fault ends the MS/TP thread permanently (`IsRunning = false`, no restart), so a host line never throws, and a swallowed write surfaces as the awaited confirmed service's own `TimeoutException` at the boundary that carries it.
- `Start()` and `StartSpyMode()` call `Open()` unguarded, so `Open` is idempotent on an open line; `Close` is reached only from `Dispose`, and ONE dedicated `IsBackground`, `ThreadPriority.Highest` thread drives every member synchronously — `FrameRecieved` alone re-dispatches on the pool.
- `BacnetMstpProtocolTransport(IBacnetSerialTransport transport, short sourceAddress = -1, byte maxMaster = 127, byte maxInfoFrames = 1)` also exposes settable `SourceAddress`/`MaxMaster`, `IsRunning`, and the timing consts `T_NO_TOKEN` 500 / `T_REPLY_TIMEOUT` 295 / `T_USAGE_TIMEOUT` 95 / `T_FRAME_ABORT` 80 / `T_REPLY_DELAY` 250; `BacnetPtpProtocolTransport(IBacnetSerialTransport transport, bool isServer)` carries the dial-up `Password` knob.

[LOCAL_ADMISSION]:
- Point maps (object id, property id, COV lifetime, unit id) carry binding-spec policy data, never a parallel BACnet loop; the per-row retry is the `OutboundHop` breaker.
- Hosts owning the native UDP line bind `BacnetIpUdpProtocolTransport` (or `BacnetIpV6UdpProtocolTransport`) directly; a host reaching a bus instead supplies its own `IBacnetSerialTransport` to the MS/TP or PTP transport, and a host reaching neither selects the `OutboundHop.CompanionSpawn` hop — three peer rows host fact selects.
- `BACnet` is the ONLY admitted identifier of the line: its physical companions `BACnet.Serial` (the `System.IO.Ports` `IBacnetSerialTransport` implementation and the `SerialTransport.Mstp`/`Ptp` factories) and `BACnet.Ethernet` (raw Ethernet over libpcap, carrying `PacketDotNet`/`SharpPcap`) stay unadmitted, so an MS/TP binding implements `IBacnetSerialTransport` over the serialport owner's already-admitted `SerialPort` rather than pulling a fourth package for five members.
- Schedule and calendar objects read and write through the `BacnetCalendarEntry`/`BacnetDailySchedule`/`BacnetSpecialEvent` family over the same property API, so a setpoint ladder crosses as its own encoded object and never as a hand-built ASN.1 buffer.
