# [RASM_APPUI_API_HIDSHARP]

`HidSharp` is the cross-platform managed HID stack with no vendor driver: `DeviceList.Local` enumerates connected `HidDevice` handles, `HidDevice.Open` mints a duplex `HidStream` over the win/linux/macos backend, `HidStream.Read`/`Write` move raw input and output reports, and the `HidSharp.Reports` descriptor parser turns a device's report descriptor into typed `DataValue` fields. The InputFabric consumes this to enumerate and read a SpaceMouse or other 6-DOF HID device, decode its multi-axis input reports through `DeviceItemInputParser`, and write LED/feature output reports back.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HidSharp`
- package: `HidSharp`
- assembly: `HidSharp`
- namespace: `HidSharp`
- namespace: `HidSharp.Reports`
- namespace: `HidSharp.Reports.Input`
- asset: runtime library (managed, no native dependency)
- rail: input

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: device enumeration and access
- rail: input

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]    | [RAIL]                          |
| :-----: | :------------------- | :--------------- | :------------------------------ |
|  [01]   | `DeviceList`         | abstract owner   | connected-device enumeration    |
|  [02]   | `Device`             | abstract handle  | open, identity, string query    |
|  [03]   | `HidDevice`          | device handle    | HID identity, report capacities |
|  [04]   | `DeviceStream`       | abstract stream  | duplex device I/O base          |
|  [05]   | `HidStream`          | device stream    | report read/write, feature I/O  |
|  [06]   | `HidDeviceLoader`    | enumeration shim | legacy device-listing entry     |
|  [07]   | `DeviceFilterHelper` | static matcher   | vendor/product/serial predicate |

[PUBLIC_TYPE_SCOPE]: open configuration
- rail: input

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [RAIL]                        |
| :-----: | :------------------ | :----------------- | :---------------------------- |
|  [01]   | `OpenConfiguration` | option bag         | per-open policy carrier       |
|  [02]   | `OpenOption`        | option key         | typed open-policy slot        |
|  [03]   | `OpenPriority`      | priority enum      | interruptible-open precedence |
|  [04]   | `GetStringFlags`    | string-flag enum   | manufacturer/product query    |
|  [05]   | `DeviceFilter`      | predicate delegate | enumeration filter            |

[PUBLIC_TYPE_SCOPE]: report descriptor and typed decoding
- rail: input

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]    | [RAIL]                             |
| :-----: | :----------------- | :--------------- | :--------------------------------- |
|  [01]   | `ReportDescriptor` | descriptor model | parsed report-descriptor tree      |
|  [02]   | `DeviceItem`       | collection node  | top-level application collection   |
|  [03]   | `Report`           | report model     | one input/output/feature report    |
|  [04]   | `DataItem`         | field model      | bit-packed field, logical/physical |
|  [05]   | `DataValue`        | value struct     | one decoded field value            |
|  [06]   | `DataConvert`      | static converter | logical/physical/scaled mapping    |
|  [07]   | `DataItemFlags`    | flags enum       | variable/relative/constant/array   |
|  [08]   | `ReportType`       | enum             | input/output/feature classifier    |
|  [09]   | `Usage`            | enum             | HID usage-page constants           |

[PUBLIC_TYPE_SCOPE]: input-report receivers
- rail: input

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [RAIL]                          |
| :-----: | :----------------------- | :---------------- | :------------------------------ |
|  [01]   | `HidDeviceInputReceiver` | background reader | event-driven report pump        |
|  [02]   | `DeviceItemInputParser`  | report decoder    | per-collection value extraction |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: enumeration and filtering
- rail: input

| [INDEX] | [SURFACE]                                                                          | [SURFACE_ROOT]       | [RAIL]                        |
| :-----: | :--------------------------------------------------------------------------------- | :------------------- | :---------------------------- |
|  [01]   | `DeviceList.Local`                                                                 | `DeviceList`         | process device-list singleton |
|  [02]   | `GetHidDevices()`                                                                  | `DeviceList`         | all HID devices               |
|  [03]   | `GetHidDevices(vendorID?, productID?, releaseNumberBcd?, serialNumber?)`           | `DeviceList`         | filtered HID enumeration      |
|  [04]   | `GetHidDeviceOrNull(vendorID?, productID?, releaseNumberBcd?, serialNumber?)`      | `DeviceList`         | first matching HID device     |
|  [05]   | `TryGetHidDevice(out HidDevice, vendorID?, productID?, ...)`                       | `DeviceList`         | guarded first-match           |
|  [06]   | `GetAllDevices()` / `GetAllDevices(DeviceFilter)`                                  | `DeviceList`         | HID, BLE, and serial union    |
|  [07]   | `Changed` / `DeviceListChanged`                                                    | `DeviceList`         | hot-plug change events        |
|  [08]   | `MatchHidDevices(device, vendorID?, productID?, releaseNumberBcd?, serialNumber?)` | `DeviceFilterHelper` | reusable filter predicate     |

[ENTRYPOINT_SCOPE]: open and stream lifecycle
- rail: input

| [INDEX] | [SURFACE]                                                           | [SURFACE_ROOT]      | [RAIL]                    |
| :-----: | :------------------------------------------------------------------ | :------------------ | :------------------------ |
|  [01]   | `Open()` / `Open(OpenConfiguration)`                                | `HidDevice`         | open, returns `HidStream` |
|  [02]   | `TryOpen(out HidStream)`                                            | `HidDevice`         | guarded open              |
|  [03]   | `TryOpen(OpenConfiguration, out HidStream)`                         | `HidDevice`         | guarded configured open   |
|  [04]   | `SetOption(OpenOption, object)`                                     | `OpenConfiguration` | set typed open policy     |
|  [05]   | `GetOption(OpenOption)` / `IsOptionSet(option)`                     | `OpenConfiguration` | read open policy          |
|  [06]   | `OpenOption.Exclusive` / `Interruptible` / `Priority` / `Transient` | `OpenOption`        | static typed option keys  |
|  [07]   | `ReadTimeout` / `WriteTimeout`                                      | `DeviceStream`      | per-stream timeout (ms)   |
|  [08]   | `Closed` / `InterruptRequested`                                     | `DeviceStream`      | stream lifecycle events   |
|  [09]   | `Dispose()`                                                         | `DeviceStream`      | release the open handle   |

[ENTRYPOINT_SCOPE]: report I/O
- rail: input

| [INDEX] | [SURFACE]                                             | [SURFACE_ROOT] | [RAIL]                       |
| :-----: | :---------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `Read()`                                              | `HidStream`    | one input report, new buffer |
|  [02]   | `Read(byte[] buffer)`                                 | `HidStream`    | one input report into buffer |
|  [03]   | `Write(byte[] buffer)`                                | `HidStream`    | one output report            |
|  [04]   | `GetFeature(byte[])` / `GetFeature(byte[], int, int)` | `HidStream`    | Get Feature setup request    |
|  [05]   | `SetFeature(byte[])` / `SetFeature(byte[], int, int)` | `HidStream`    | Set Feature setup request    |
|  [06]   | `Flush()`                                             | `HidStream`    | flush pending output         |
|  [07]   | `GetMaxInputReportLength()`                           | `HidDevice`    | input report buffer size     |
|  [08]   | `GetMaxOutputReportLength()`                          | `HidDevice`    | output report buffer size    |
|  [09]   | `GetMaxFeatureReportLength()`                         | `HidDevice`    | feature report buffer size   |

[ENTRYPOINT_SCOPE]: descriptor parse and typed decode
- rail: input

| [INDEX] | [SURFACE]                                                               | [SURFACE_ROOT]          | [RAIL]                         |
| :-----: | :---------------------------------------------------------------------- | :---------------------- | :----------------------------- |
|  [01]   | `GetReportDescriptor()`                                                 | `HidDevice`             | parsed `ReportDescriptor`      |
|  [02]   | `GetRawReportDescriptor()`                                              | `HidDevice`             | raw descriptor bytes           |
|  [03]   | `new ReportDescriptor(byte[])`                                          | `ReportDescriptor`      | parse from raw bytes           |
|  [04]   | `DeviceItems`                                                           | `ReportDescriptor`      | top-level collection list      |
|  [05]   | `InputReports` / `OutputReports` / `FeatureReports`                     | `DeviceItem`            | reports by type                |
|  [06]   | `CreateDeviceItemInputParser()`                                         | `DeviceItem`            | per-collection value parser    |
|  [07]   | `CreateHidDeviceInputReceiver()`                                        | `ReportDescriptor`      | background report pump         |
|  [08]   | `TryParseReport(byte[] buffer, int offset, Report)`                     | `DeviceItemInputParser` | decode report into values      |
|  [09]   | `GetValue(int index)` / `GetPreviousValue(int index)`                   | `DeviceItemInputParser` | read current/prior `DataValue` |
|  [10]   | `HasChanged` / `GetNextChangedIndex()` / `ValueCount`                   | `DeviceItemInputParser` | changed-field iteration        |
|  [11]   | `GetLogicalValue()` / `GetScaledValue(min, max)` / `GetPhysicalValue()` | `DataValue`             | per-axis value projection      |
|  [12]   | `Usages` / `DataItem` / `DataIndex`                                     | `DataValue`             | field identity and usage codes |

[ENTRYPOINT_SCOPE]: event-driven receiver
- rail: input

| [INDEX] | [SURFACE]                                        | [SURFACE_ROOT]           | [RAIL]                      |
| :-----: | :----------------------------------------------- | :----------------------- | :-------------------------- |
|  [01]   | `Start(HidStream)`                               | `HidDeviceInputReceiver` | begin background reads      |
|  [02]   | `TryRead(byte[] buffer, int offset, out Report)` | `HidDeviceInputReceiver` | dequeue one buffered report |
|  [03]   | `Received` / `Started` / `Stopped`               | `HidDeviceInputReceiver` | pump lifecycle events       |
|  [04]   | `WaitHandle` / `IsRunning` / `Stream`            | `HidDeviceInputReceiver` | wait gate and running state |

## [04]-[IMPLEMENTATION_LAW]

[HIDSHARP_TOPOLOGY]:
- `DeviceList.Local` is the process-wide enumeration root; `GetHidDevices` returns `IEnumerable<HidDevice>` and the `vendorID`/`productID`/`releaseNumberBcd`/`serialNumber` overload filters at the source through `DeviceFilterHelper.MatchHidDevices`, so SpaceMouse selection passes the 3DConnexion vendor id rather than post-filtering a full list.
- `HidDevice` carries identity (`VendorID`, `ProductID`, `ReleaseNumber`, `Manufacturer`, `ProductName`, `SerialNumber`) and report capacities (`MaxInputReportLength`, `MaxOutputReportLength`, `MaxFeatureReportLength`) without opening the device; `Open` is the only call that acquires the OS handle.
- `HidDevice.Open` returns a `HidStream` (the `HidDevice`-typed override of the `Device.Open` -> `DeviceStream` base), and `HidStream` is a `System.IO.Stream`: `Read(byte[])` blocks for one input report up to `ReadTimeout` (default 3000 ms, `Timeout.Infinite` disables), `Write(byte[])` sends one output report, and `GetFeature`/`SetFeature` issue Get/Set Feature control transfers off the report path.
- `OpenConfiguration` is an option bag keyed by `OpenOption` static keys; `Interruptible` plus `Priority` (`OpenPriority` from `Idle` to `VeryHigh`) let a higher-priority opener interrupt a lower one, surfaced through the `DeviceStream.InterruptRequested` event, and `Exclusive`/`Transient` govern shared access and transient-device tolerance.
- `HidDevice.GetReportDescriptor()` parses the raw descriptor into a `ReportDescriptor` whose `DeviceItems` are application collections; each `DeviceItem` groups `Report`s by `ReportType`, and a `Report` owns `DataItem`s describing bit offset, `ElementCount`, `LogicalMinimum`/`LogicalMaximum`, `PhysicalMinimum`/`PhysicalMaximum`, `Unit`, and `DataItemFlags` (`Variable`, `Relative`, `Constant`, `Array`).
- `DeviceItemInputParser` from `DeviceItem.CreateDeviceItemInputParser()` decodes a raw input report: `TryParseReport(buffer, offset, report)` populates the value table, `ValueCount` bounds the index space, `GetValue(index)` returns a `DataValue`, and `HasChanged`/`GetNextChangedIndex()` walk only the fields that moved since the prior report — the 6-DOF SpaceMouse path reads six translation/rotation axes plus button bits per report this way.
- `DataValue` projects one field three ways: `GetLogicalValue()` returns the raw signed logical integer, `GetScaledValue(min, max)` maps the logical range linearly onto an arbitrary range through `DataConvert.CustomFromLogical`, and `GetPhysicalValue()` applies the descriptor's physical units; `Usages` exposes the HID usage codes that identify which axis or button the value carries.
- `HidDeviceInputReceiver` from `ReportDescriptor.CreateHidDeviceInputReceiver()` is the event-driven alternative to a manual `Read` loop: `Start(stream)` pumps reports on a background thread, `Received` fires per report, `TryRead` dequeues buffered reports, and `WaitHandle` gates a consumer that parks until data arrives.

[LOCAL_ADMISSION]:
- Device enumeration uses `DeviceList.Local.GetHidDevices(vendorID, productID)` with the target vendor and product ids; the boundary capsule never enumerates the full device set and post-filters.
- `Open` and `TryOpen` results are scoped: the `HidStream` is a `Stream` and the boundary capsule pairs `Open` with `Dispose` in a scoped fold, never leaking the OS handle past the read loop.
- Raw report bytes cross the boundary once: `HidStream.Read` fills a buffer sized by `GetMaxInputReportLength()`, and decoding lives entirely inside `DeviceItemInputParser`/`DataValue`, so canonical 6-DOF axis values leave the capsule, not raw HID byte arrays.
- `ReadTimeout` is set explicitly per stream; a polling input loop uses a finite timeout and the `HidDeviceInputReceiver` event path uses `WaitHandle` rather than a busy-wait `Read`.
- Hot-plug is observed through `DeviceList.Changed`; the InputFabric re-enumerates on that event rather than re-opening a stale handle.

[RAIL_LAW]:
- Package: `HidSharp`
- Owns: cross-platform raw HID device access — enumeration, open/configuration, raw input/output/feature report I/O, report-descriptor parsing, and typed multi-axis value decoding for the InputFabric `Hid` input source.
- Accept: `DeviceList.Local` enumeration with source-side vendor/product filters; scoped `HidStream` open-and-dispose pairs; descriptor-driven decoding through `DeviceItemInputParser` and `DataValue`.
- Reject: a vendor-specific 3DConnexion driver dependency; manual bit-shifting of raw report bytes when `DataItem`/`DataValue` already model the field; a busy-wait `Read` loop when `HidDeviceInputReceiver` plus `WaitHandle` carries the same input stream.
