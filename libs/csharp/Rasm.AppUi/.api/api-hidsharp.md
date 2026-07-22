# [RASM_APPUI_API_HIDSHARP]

`HidSharp` owns the AppUi raw-HID surface: driver-free cross-platform device enumeration, duplex `HidStream` open over the win/linux/macos backend, raw input/output/feature report I/O, and the `HidSharp.Reports` descriptor parser that decodes a report into typed `DataValue` fields. `Hid` folds onto the single `InputFabric` edge every device rail shares, decoding a 6-DOF HID device through `DeviceItemInputParser`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HidSharp`
- package: `HidSharp` (Apache-2.0)
- assembly: `HidSharp`
- consumer-tfm: `netstandard2.0` (package ships `netstandard2.0`/`net35`; `net10.0` binds the `netstandard2.0` asset)
- namespace: `HidSharp`, `HidSharp.Reports`, `HidSharp.Reports.Input`
- asset: managed runtime library; P/Invokes the OS HID stack (Win32 `hid.dll`/SetupAPI, Linux `udev`/`hidraw`, macOS IOKit) at runtime, ships no native asset
- rail: input

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: device enumeration and access

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [CAPABILITY]                    |
| :-----: | :------------------- | :-------------- | :------------------------------ |
|  [01]   | `DeviceList`         | abstract owner  | connected-device enumeration    |
|  [02]   | `Device`             | abstract handle | open, identity, string query    |
|  [03]   | `HidDevice`          | device handle   | HID identity, report capacities |
|  [04]   | `DeviceStream`       | abstract stream | duplex device I/O base          |
|  [05]   | `HidStream`          | device stream   | report read/write, feature I/O  |
|  [06]   | `DeviceFilterHelper` | static matcher  | vendor/product/serial predicate |

[PUBLIC_TYPE_SCOPE]: open configuration

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]      | [CAPABILITY]                  |
| :-----: | :------------------ | :----------------- | :---------------------------- |
|  [01]   | `OpenConfiguration` | option bag         | per-open policy carrier       |
|  [02]   | `OpenOption`        | option key         | typed open-policy slot        |
|  [03]   | `OpenPriority`      | priority enum      | interruptible-open precedence |
|  [04]   | `GetStringFlags`    | string-flag enum   | manufacturer/product query    |
|  [05]   | `DeviceFilter`      | predicate delegate | enumeration filter            |

[PUBLIC_TYPE_SCOPE]: report descriptor and typed decoding

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]    | [CAPABILITY]                       |
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

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [CAPABILITY]                    |
| :-----: | :----------------------- | :---------------- | :------------------------------ |
|  [01]   | `HidDeviceInputReceiver` | background reader | event-driven report pump        |
|  [02]   | `DeviceItemInputParser`  | report decoder    | per-collection value extraction |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: enumeration and filtering

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]               |
| :-----: | :----------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `DeviceList.Local`                                           | static   | process device singleton   |
|  [02]   | `DeviceList.GetHidDevices()`                                 | instance | all HID devices            |
|  [03]   | `DeviceList.GetHidDevices(int?, int?, int?, string)`         | instance | filtered HID enumeration   |
|  [04]   | `DeviceList.GetHidDeviceOrNull(int?, int?, int?, string)`    | instance | first matching HID device  |
|  [05]   | `DeviceList.TryGetHidDevice(out HidDevice, int?, ...)`       | instance | guarded first-match        |
|  [06]   | `DeviceList.GetAllDevices()` / `GetAllDevices(DeviceFilter)` | instance | HID, BLE, and serial union |
|  [07]   | `DeviceList.Changed` / `DeviceListChanged`                   | event    | hot-plug change events     |
|  [08]   | `DeviceFilterHelper.MatchHidDevices(HidDevice, int?, ...)`   | static   | reusable filter predicate  |

[ENTRYPOINT_SCOPE]: open and stream lifecycle

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]              |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `HidDevice.Open()` / `Open(OpenConfiguration)`                        | instance | open, returns `HidStream` |
|  [02]   | `HidDevice.TryOpen(out HidStream)`                                    | instance | guarded open              |
|  [03]   | `HidDevice.TryOpen(OpenConfiguration, out HidStream)`                 | instance | guarded configured open   |
|  [04]   | `OpenConfiguration.SetOption(OpenOption, object)`                     | instance | set typed open policy     |
|  [05]   | `OpenConfiguration.GetOption(OpenOption)` / `IsOptionSet(OpenOption)` | instance | read open policy          |
|  [06]   | `OpenOption.Exclusive` / `Interruptible` / `Priority` / `Transient`   | static   | typed option keys         |
|  [07]   | `DeviceStream.ReadTimeout` / `WriteTimeout`                           | property | per-stream timeout (ms)   |
|  [08]   | `DeviceStream.Closed` / `InterruptRequested`                          | event    | stream lifecycle events   |
|  [09]   | `DeviceStream.Dispose()`                                              | instance | release the open handle   |

[ENTRYPOINT_SCOPE]: report I/O

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :-------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `HidStream.Read()`                                              | instance | one input report, new buffer |
|  [02]   | `HidStream.Read(byte[])`                                        | instance | one input report into buffer |
|  [03]   | `HidStream.Write(byte[])`                                       | instance | one output report            |
|  [04]   | `HidStream.GetFeature(byte[])` / `GetFeature(byte[], int, int)` | instance | Get Feature setup request    |
|  [05]   | `HidStream.SetFeature(byte[])` / `SetFeature(byte[], int, int)` | instance | Set Feature setup request    |
|  [06]   | `HidStream.Flush()`                                             | instance | flush pending output         |
|  [07]   | `HidDevice.GetMaxInputReportLength()`                           | instance | input report buffer size     |
|  [08]   | `HidDevice.GetMaxOutputReportLength()`                          | instance | output report buffer size    |
|  [09]   | `HidDevice.GetMaxFeatureReportLength()`                         | instance | feature report buffer size   |

[ENTRYPOINT_SCOPE]: descriptor parse and typed decode

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `HidDevice.GetReportDescriptor()`                                    | instance | parsed `ReportDescriptor`      |
|  [02]   | `HidDevice.GetRawReportDescriptor()`                                 | instance | raw descriptor bytes           |
|  [03]   | `new ReportDescriptor(byte[])`                                       | ctor     | parse from raw bytes           |
|  [04]   | `ReportDescriptor.DeviceItems` / `Reports` / `ReportsUseID`          | property | descriptor topology            |
|  [05]   | `ReportDescriptor.MaxInputReportLength` / `MaxOutputReportLength`    | property | report buffer sizes            |
|  [06]   | `ReportDescriptor.MaxFeatureReportLength`                            | property | feature buffer size            |
|  [07]   | `ReportDescriptor.GetReport(ReportType, byte)` / `TryGetReport(...)` | instance | report lookup by type and id   |
|  [08]   | `DeviceItem.InputReports` / `OutputReports` / `FeatureReports`       | property | reports by type                |
|  [09]   | `DeviceItem.CreateDeviceItemInputParser()`                           | instance | per-collection value parser    |
|  [10]   | `ReportDescriptor.CreateHidDeviceInputReceiver()`                    | instance | background report pump         |
|  [11]   | `DeviceItemInputParser.TryParseReport(byte[], int, Report)`          | instance | decode report into values      |
|  [12]   | `DeviceItemInputParser.GetValue(int)` / `GetPreviousValue(int)`      | instance | read current/prior `DataValue` |
|  [13]   | `DeviceItemInputParser.HasChanged` / `GetNextChangedIndex()`         | instance | changed-field iteration        |
|  [14]   | `DeviceItemInputParser.ValueCount` / `DeviceItem`                    | property | parser bounds and collection   |
|  [15]   | `DataValue.GetLogicalValue()` / `GetScaledValue(double, double)`     | instance | raw and scaled projection      |
|  [16]   | `DataValue.GetFractionalValue()` / `GetPhysicalValue()`              | instance | fractional and physical value  |
|  [17]   | `DataValue.Usages` / `DataItem` / `DataIndex` / `Report` / `IsValid` | property | field identity and usage codes |

[ENTRYPOINT_SCOPE]: field model and callback decode

| [INDEX] | [SURFACE]                                                                                    | [SHAPE]  | [CAPABILITY]                |
| :-----: | :------------------------------------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `Report.DataItems` / `Length` / `ReportID` / `ReportType`                                    | property | report shape and id         |
|  [02]   | `Report.Read(byte[], int, ReportValueCallback)` / `GetAllUsages()`                           | instance | parser-free callback decode |
|  [03]   | `DataItem.ElementCount` / `ElementBits` / `TotalBits`                                        | property | field bit geometry          |
|  [04]   | `DataItem.LogicalMinimum` / `LogicalMaximum` / `LogicalRange` / `IsLogicalSigned`            | property | logical range for scaling   |
|  [05]   | `DataItem.PhysicalMinimum` / `PhysicalMaximum` / `Unit`                                      | property | physical units              |
|  [06]   | `DataItem.IsVariable` / `IsArray` / `IsRelative` / `IsAbsolute` / `IsConstant` / `IsBoolean` | property | flag-derived field class    |

[ENTRYPOINT_SCOPE]: event-driven receiver

| [INDEX] | [SURFACE]                                                    | [SHAPE]  | [CAPABILITY]                |
| :-----: | :----------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `HidDeviceInputReceiver.Start(HidStream)`                    | instance | begin background reads      |
|  [02]   | `HidDeviceInputReceiver.TryRead(byte[], int, out Report)`    | instance | dequeue one buffered report |
|  [03]   | `HidDeviceInputReceiver.Received` / `Started` / `Stopped`    | event    | pump lifecycle events       |
|  [04]   | `HidDeviceInputReceiver.WaitHandle` / `IsRunning` / `Stream` | property | wait gate and running state |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DeviceList.Local` roots process-wide enumeration; source-side `vendorID`/`productID` filters select through `DeviceFilterHelper.MatchHidDevices`, and `HidDevice` carries identity and report capacities before `Open` acquires the one OS handle.
- `HidDevice.Open` returns a `HidStream` (`System.IO.Stream`): `Read` blocks for one input report to `ReadTimeout` (default 3000 ms, `Timeout.Infinite` disables), `Write` sends one output report, and `GetFeature`/`SetFeature` run Get/Set Feature control transfers off the report path.
- `OpenConfiguration` keys open policy by `OpenOption`; `Interruptible` with `OpenPriority` (`Idle` to `VeryHigh`) lets a higher-priority opener interrupt a lower one through `DeviceStream.InterruptRequested`, and `Exclusive`/`Transient` govern shared access and transient tolerance.
- `GetReportDescriptor` parses the descriptor into the `ReportDescriptor` → `DeviceItem` → `Report` → `DataItem` tree; `DeviceItemInputParser.TryParseReport` fills the value table, `HasChanged`/`GetNextChangedIndex` walk only fields that moved, and `DataValue` projects one field as logical, scaled (`GetScaledValue` over `DataConvert.CustomFromLogical`), fractional, or physical.

[STACKING]:
- `api-silk-input.md` / `api-silk-sdl.md` / `api-drywetmidi.md`: the `Hid` case joins `Gamepad`/`Haptic`/`Midi` on the single `InputFabric` edge, every capsule folding `DataValue.GetScaledValue(-1, 1)` normalized `DeviceAxis` samples onto the one `CommandIntent` table; unlike the SDL2 `Gamepad`/`Haptic` pair the `Hid` capsule binds no native bundle, sharing only that fold.
- `api-reactive.md`: `HidDeviceInputReceiver.Received`, or a `WaitHandle`-gated `Read` loop, projects into `IObservable<Seq<DeviceAxis>>` through `Observable.FromEventPattern`/`Create`, decoding off the render thread and marshaling axes once at the bind edge.
- within-lib: `DeviceList.Local.GetHidDevices(vendorId, productId)` → scoped `HidStream` → `DeviceItemInputParser`/`DataValue` decode is one fold; `HasChanged`/`GetNextChangedIndex` bound per-report work to moved fields, and `DeviceList.Changed` re-enumerates on hot-plug while `Open` → `HidStream` → `Dispose` is one scoped teardown.

[LOCAL_ADMISSION]:
- Enumeration passes vendor and product ids to `GetHidDevices`; the capsule never enumerates the full set and post-filters.
- `Open`/`TryOpen` pair with `Dispose` in a scoped fold; raw report bytes decode inside `DeviceItemInputParser`/`DataValue`, so canonical `DeviceAxis` values leave the capsule, never raw HID arrays.
- A polling loop sets a finite `ReadTimeout`; the event path parks on `HidDeviceInputReceiver.WaitHandle`, never a busy-wait `Read`.

[RAIL_LAW]:
- Package: `HidSharp`
- Owns: cross-platform raw HID access — enumeration, open/configuration, raw input/output/feature report I/O, report-descriptor parsing, and typed multi-axis value decoding for the InputFabric `Hid` source.
- Accept: `DeviceList.Local` enumeration with source-side vendor/product filters, scoped `HidStream` open-and-dispose pairs, descriptor-driven `DeviceItemInputParser`/`DataValue` decoding, and the `Hid` case on the single `InputFabric` edge beside `Gamepad`/`Haptic`/`Midi`, folding normalized `DeviceAxis` samples onto the one `CommandIntent` table.
- Reject: a vendor-specific 3DConnexion driver dependency, manual bit-shifting of raw report bytes when `DataItem`/`DataValue` model the field, a busy-wait `Read` loop when `HidDeviceInputReceiver` plus `WaitHandle` carries the stream, and a parallel HID→intent edge beside the shared `InputFabric` fold.
