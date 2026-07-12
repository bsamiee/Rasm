# [RASM_APPUI_API_URSA]

`Irihi.Ursa` is the extended-control suite (assembly `Ursa.dll`) that fills the families the curated Avalonia roster lacks — navigation (`NavMenu`/`Breadcrumb`/`Pagination`/`Anchor`), feedback (`Toast`/`Notification`/`Banner`/`MessageBox`/`Loading`/`Skeleton`), overlay (`OverlayDialogHost`/`Drawer`/`PopConfirm`), data entry (`Form`/`NumericUpDown`/`PinCode`/`IPv4Box`/`TimeBox`/`TagInput`/`KeyGestureInput`/the full date/time/range picker family), and display (`Timeline`/`Avatar`/`Badge`/`Rating`/`Marquee`/`Descriptions`/`Divider`/`QRCode`). The control _visuals_ live in the sibling `Irihi.Ursa.Themes.Semi` (assembly `Ursa.Themes.Semi.dll`, the `UrsaSemiTheme : Styles` control-theme dictionary added via `<semi:UrsaSemiTheme/>`, one compiled `.axaml` per control) layered under the `Semi.Avalonia` token system (`api-semi.md`) — both suites bind the SAME `https://irihi.tech/semi` (`semi:`) xmlns, so the canonical `Application.Styles` chain is `FluentTheme` floor -> `<semi:SemiTheme/>` -> the per-control `Semi.Avalonia.*` skins -> `<semi:UrsaSemiTheme/>`; `Irihi.Ursa.ReactiveUIExtension` (assembly `Ursa.ReactiveUIExtension.dll`) bridges the two view bases onto the admitted ReactiveUI rail. The overlay families (`OverlayDialog`/`Drawer`/`MessageBox.ShowOverlayAsync`/`WindowToastManager`/`WindowNotificationManager`) are the in-canvas counterpart to `DialogHost.Avalonia` (`api-dialoghost.md`): a view-model raises an overlay through a static dispatch helper against a host id, never by hand-building a popup.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Irihi.Ursa`

- package: `Irihi.Ursa`
- license: MIT
- floor: `net10.0` consumer (`lib/net10.0/Ursa.dll`); the package multi-targets net8.0 / net10.0 and the `net10.0` asset is bound
- assembly: `Ursa`
- namespace: `Ursa.Controls` (the full control roster — 244 control/option/event types), `Ursa.Common` (`Position`/`ItemAlignment`/`CornerPosition` placement vocabulary, `ObservableHelper`), `Ursa.Converters`, `Ursa.Helpers`, `Ursa.EventArgs`
- depends: `Avalonia` (12.x — its `>= 11.x` floor rises to the admitted line), `Irihi.Avalonia.Shared` + `Irihi.Avalonia.Shared.Contracts` (the shared primitive closure also floored by `Semi.Avalonia`); vendors `Gma.QrCodeNet` internally for the `QRCode` control (no QR dependency surfaces)
- rail: controls

[PACKAGE_SURFACE]: `Irihi.Ursa.Themes.Semi`

- package: `Irihi.Ursa.Themes.Semi`
- license: MIT
- floor: `net10.0` consumer (`lib/net10.0/Ursa.Themes.Semi.dll`)
- assembly: `Ursa.Themes.Semi`
- namespace: `Ursa.Themes.Semi` owns `UrsaSemiTheme : Styles`; its `https://irihi.tech/semi` xmlns and `semi` prefix are shared with `Semi.Avalonia`
- converters: `Ursa.Themes.Semi.Converters` owns the public template converters
- resources: `Ursa.Themes.Semi.Locale` and `Ursa.Themes.Semi.SizeAnimations` own the resource dictionaries
- obsolete: `Ursa.Themes.Semi.Legacy.SemiTheme` uses the `https://irihi.tech/ursa/themes/semi` xmlns and `u-semi` prefix; `[Obsolete("…use UrsaSemiTheme instead")]` excludes it
- surface: the `UrsaSemiTheme : Styles` control-theme dictionary added via `<semi:UrsaSemiTheme/>` — one compiled `.axaml` `ControlTheme` per `Ursa.Controls` type keyed onto the `Semi.Avalonia` `SemiTheme` tokens — plus a thin CODE surface MIRRORING `Semi.Avalonia.SemiTheme`: ctor `UrsaSemiTheme(IServiceProvider? provider = null)`, `Locale` (`CultureInfo?` property, defaults `zh-CN` on unknown), static `OverrideLocaleResources(Application, CultureInfo?)` / `OverrideLocaleResources(StyledElement, CultureInfo?)`, and the public `Ursa.Themes.Semi.Converters` set (`BooleansToOpacityConverter`, `BrushToColorConverter`, `ClockHandLengthConverter(double ratio)`, `FormContentHeightToAlignmentConverter`, `FormContentHeightToMarginConverter`, `NavMenuMarginConverter`, `TreeLevelToPaddingConverter`). Its built-in locale set (`zh_cn`/`en_us`/`de_de`/`fr_fr`/`ru_ru`/`pl_pl`/`cs_cz`) is NARROWER than `Semi.Avalonia`'s 16-culture set, so an unmatched culture falls back to `zh-CN` rather than the Semi default
- rail: controls

[PACKAGE_SURFACE]: `Irihi.Ursa.ReactiveUIExtension`

- package: `Irihi.Ursa.ReactiveUIExtension`
- license: MIT
- floor: `net10.0` consumer (`lib/net10.0/Ursa.ReactiveUIExtension.dll`); its `ReactiveUI.Avalonia >= 11.3.0` floor rises to the admitted 12.x line
- assembly: `Ursa.ReactiveUIExtension`
- namespace: `Ursa.ReactiveUIExtension` — exactly two types: `ReactiveUrsaView` and `ReactiveUrsaWindow`
- depends: `Ursa`, `reactiveui`
- rail: controls

## [02]-[PUBLIC_TYPES]

[NAVIGATION_CONTROLS]: navigation + wayfinding — `Ursa.Controls`

- rail: controls

| [INDEX] | [SYMBOL]                                        | [KIND]                                                        |
| :-----: | :---------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `NavMenu` / `NavMenuItem`                       | hierarchical side/top navigation menu (`ItemsControl`) + item |
|  [02]   | `Breadcrumb` / `BreadcrumbItem`                 | breadcrumb trail                                              |
|  [03]   | `Pagination` / `PaginationButton`               | page-navigation strip                                         |
|  [04]   | `Anchor` / `AnchorItem`                         | scroll-spy anchor index over a scroll region                  |
|  [05]   | `ScrollTo` / `ScrollToButton`                   | programmatic scroll-into-view target + trigger                |
|  [06]   | `ToolBar` / `ToolBarPanel` / `ToolBarSeparator` | overflow-aware tool bar (`OverflowMode`)                      |
|  [07]   | `TitleBar`                                      | custom window title bar (chromeless host)                     |

[FEEDBACK_CONTROLS]: toast, notification, banner, message, busy — `Ursa.Controls`

- rail: controls

The toast and notification managers expose `Show`, `Close`, and `CloseAll`; `MessageBox` exposes `ShowAsync` and `ShowOverlayAsync`.

| [INDEX] | [SYMBOL]                    | [KIND]                         |
| :-----: | :-------------------------- | :----------------------------- |
|  [01]   | `WindowToastManager`        | transient toast queue manager  |
|  [02]   | `Toast`                     | toast message                  |
|  [03]   | `ToastCard`                 | toast presentation card        |
|  [04]   | `IToast`                    | toast contract                 |
|  [05]   | `IToastManager`             | toast manager contract         |
|  [06]   | `WindowNotificationManager` | corner notification manager    |
|  [07]   | `Notification`              | notification message           |
|  [08]   | `NotificationCard`          | notification presentation card |
|  [09]   | `INotification`             | notification contract          |
|  [10]   | `INotificationManager`      | notification manager contract  |
|  [11]   | `WindowMessageManager`      | message-stack manager          |
|  [12]   | `MessageCard`               | message presentation card      |
|  [13]   | `IMessage`                  | base message contract          |
|  [14]   | `Banner`                    | inline `Status` banner         |
|  [15]   | `MessageBox`                | modal message dispatcher       |
|  [16]   | `MessageBoxControl`         | message-box control            |
|  [17]   | `MessageBoxWindow`          | windowed message box           |
|  [18]   | `OverlayMessageBox`         | overlay message box            |
|  [19]   | `Loading`                   | busy spinner                   |
|  [20]   | `LoadingContainer`          | content busy overlay           |
|  [21]   | `LoadingIcon`               | busy indicator                 |
|  [22]   | `Skeleton`                  | shimmer loading placeholder    |
|  [23]   | `PopConfirm`                | `PopConfirmTriggerMode` prompt |

[OVERLAY_CONTROLS]: in-canvas dialog + drawer host layer — `Ursa.Controls`

- rail: controls

`OverlayDialogHost` is the registered-id target for `OverlayDialog`, `Drawer`, and `MessageBox.ShowOverlayAsync`. `OverlayDialogManager` owns `RegisterHost`, `UnregisterHost`, and `GetHost(id, hash)`.

| [INDEX] | [SYMBOL]                | [KIND]                     |
| :-----: | :---------------------- | :------------------------- |
|  [01]   | `OverlayDialogHost`     | in-canvas overlay host     |
|  [02]   | `OverlayDialogManager`  | static host registry       |
|  [03]   | `Dialog`                | windowed dialog dispatcher |
|  [04]   | `DialogControlBase`     | dialog control base        |
|  [05]   | `DialogOptions`         | windowed dialog options    |
|  [06]   | `DialogResult`          | dialog result              |
|  [07]   | `DialogButton`          | dialog button set          |
|  [08]   | `DialogMode`            | dialog modality            |
|  [09]   | `OverlayDialog`         | overlay dialog dispatcher  |
|  [10]   | `OverlayDialogOptions`  | overlay dialog options     |
|  [11]   | `Drawer`                | drawer dispatcher          |
|  [12]   | `DrawerControlBase`     | drawer control base        |
|  [13]   | `OverlayDrawer`         | slide-in drawer surface    |
|  [14]   | `DialogResizer`         | edge-resize surface        |
|  [15]   | `DrawerOptions`         | drawer options             |
|  [16]   | `CustomDialogControl`   | custom dialog shell        |
|  [17]   | `StandardDialogControl` | titled dialog shell        |
|  [18]   | `CustomDrawerControl`   | custom drawer shell        |
|  [19]   | `StandardDrawerControl` | titled drawer shell        |

[DRAWER_OPTIONS]: `DrawerOptions` resides in `Ursa.Controls.Options`.

[STANDARD_OVERLAY_SHELLS]: `StandardDialogControl` and `StandardDrawerControl` are titled, buttoned shells.

[DATA_ENTRY_CONTROLS]: form, numeric, masked, gesture, tag entry — `Ursa.Controls`

- rail: controls

`Numeric<T>UpDown` admits `Byte`, `SByte`, `Short`, `UShort`, `Int`, `UInt`, `Long`, `ULong`, `Float`, `Double`, and `Decimal`. `<T>Displayer` admits `Double`, `Int32`, and `Int64`.

| [INDEX] | [SYMBOL]              | [KIND]                          |
| :-----: | :-------------------- | :------------------------------ |
|  [01]   | `Form`                | declarative form layout         |
|  [02]   | `FormGroup`           | form field group                |
|  [03]   | `FormItem`            | labeled validated field         |
|  [04]   | `NumericUpDown`       | numeric spinner                 |
|  [05]   | `NumericUpDownBase`   | numeric spinner base            |
|  [06]   | `Numeric<T>UpDown`    | CLR-typed numeric spinner       |
|  [07]   | `NumberDisplayer`     | rolling-number display          |
|  [08]   | `NumberDisplayerBase` | rolling-number base             |
|  [09]   | `<T>Displayer`        | CLR-typed rolling display       |
|  [10]   | `PinCode`             | segmented `PinCodeMode` entry   |
|  [11]   | `PinCodeItem`         | PIN segment                     |
|  [12]   | `PinCodeCollection`   | PIN segment collection          |
|  [13]   | `IPv4Box`             | masked `IPv4BoxInputMode` entry |
|  [14]   | `TimeBox`             | masked time-component entry     |
|  [15]   | `KeyGestureInput`     | keyboard-shortcut capture       |
|  [16]   | `TagInput`            | token entry                     |
|  [17]   | `NumPad`              | on-screen numeric keypad        |
|  [18]   | `EnumSelector`        | enum-member picker              |
|  [19]   | `EnumItemTuple`       | enum-member tuple               |
|  [20]   | `ControlClassesInput` | Avalonia `StyleClass` editor    |
|  [21]   | `PathPicker`          | file-or-folder field            |

[TIME_BOX_MODES]: `TimeBoxInputMode` selects components, and `TimeBoxDragOrientation` selects drag direction.

[SELECTION_CONTROLS]: multi-select, combo, rating, button-group — `Ursa.Controls`

- rail: controls

| [INDEX] | [SYMBOL]                            | [KIND]                      |
| :-----: | :---------------------------------- | :-------------------------- |
|  [01]   | `MultiComboBox`                     | multi-select combo          |
|  [02]   | `MultiComboBoxItem`                 | combo item                  |
|  [03]   | `MultiComboBoxSelectedItemList`     | selected-chip readout       |
|  [04]   | `TreeComboBox`                      | hierarchical drop-down      |
|  [05]   | `TreeComboBoxItem`                  | tree selector item          |
|  [06]   | `AutoCompleteBox`                   | single type-ahead selector  |
|  [07]   | `MultiAutoCompleteBox`              | multi type-ahead selector   |
|  [08]   | `MultiAutoCompleteSelectionAdapter` | multi-selection adapter     |
|  [09]   | `SelectionList`                     | single-or-range list        |
|  [10]   | `SelectionListItem`                 | selection-list item         |
|  [11]   | `ButtonGroup`                       | mutually exclusive segments |
|  [12]   | `Rating`                            | star-or-character input     |
|  [13]   | `RatingCharacter`                   | rating glyph                |
|  [14]   | `RangeSlider`                       | dual-thumb range input      |
|  [15]   | `RangeTrack`                        | range track                 |
|  [16]   | `RangeValueChangedEventArgs`        | range-change event          |
|  [17]   | `ClosableTag`                       | dismissible tag chip        |

[SELECTION_EVENT]: `SelectionChangingEventArgs` carries `SelectionList` changes.

[DISPLAY_CONTROLS]: timeline, avatar, badge, marquee, descriptions, QR — `Ursa.Controls`

- rail: controls

| [INDEX] | [SYMBOL]                | [KIND]                  |
| :-----: | :---------------------- | :---------------------- |
|  [01]   | `Timeline`              | event timeline          |
|  [02]   | `TimelineItem`          | timeline event          |
|  [03]   | `TimelinePanel`         | timeline layout panel   |
|  [04]   | `Avatar`                | image-or-initial avatar |
|  [05]   | `Badge`                 | count-or-dot badge      |
|  [06]   | `DualBadge`             | dual-segment badge      |
|  [07]   | `Marquee`               | scrolling-text ticker   |
|  [08]   | `Descriptions`          | key-value grid          |
|  [09]   | `DescriptionsItem`      | description entry       |
|  [10]   | `Divider`               | titled divider          |
|  [11]   | `ImageViewer`           | pan-zoom image viewer   |
|  [12]   | `QRCode`                | QR renderer             |
|  [13]   | `IconButton`            | icon-leading button     |
|  [14]   | `IconToggleButton`      | icon toggle button      |
|  [15]   | `IconSplitButton`       | icon split button       |
|  [16]   | `IconToggleSplitButton` | icon toggle split       |
|  [17]   | `IconDropDownButton`    | icon drop-down button   |
|  [18]   | `IconRepeatButton`      | icon repeat button      |
|  [19]   | `TwoTonePathIcon`       | two-tone glyph          |
|  [20]   | `LabeledContentControl` | labeled container       |
|  [21]   | `GroupBoxBorder`        | group border            |
|  [22]   | `UrsaGroupBox`          | group container         |

[TIMELINE_POLICY]: `TimelineDisplayMode`, `TimelineItemPosition`, and `TimelineItemType` govern vertical or horizontal timelines.

[QR_POLICY]: `QRCode` consumes `EccLevel` and vendors `Gma.QrCodeNet` internally.

[PICKER_CONTROLS]: full date / time / range picker family — `Ursa.Controls`

- rail: controls

| [INDEX] | [SYMBOL]                       | [KIND]                   |
| :-----: | :----------------------------- | :----------------------- |
|  [01]   | `DatePicker`                   | calendar date picker     |
|  [02]   | `DateRangePicker`              | calendar range picker    |
|  [03]   | `DateOnlyPicker`               | `DateOnly` picker        |
|  [04]   | `DateOnlyRangePicker`          | `DateOnly` range picker  |
|  [05]   | `DateTimePicker`               | date-time picker         |
|  [06]   | `DateTimeOffsetPicker`         | date-time-offset picker  |
|  [07]   | `DateOffsetPicker`             | date-offset picker       |
|  [08]   | `DateOffsetRangePicker`        | date-offset range picker |
|  [09]   | `TimePicker`                   | time picker              |
|  [10]   | `TimeRangePicker`              | time range picker        |
|  [11]   | `TimeOnlyPicker`               | `TimeOnly` picker        |
|  [12]   | `TimeOnlyRangePicker`          | `TimeOnly` range picker  |
|  [13]   | `TimePickerPresenter`          | time picker presenter    |
|  [14]   | `Clock`                        | analog clock             |
|  [15]   | `ClockTicks`                   | clock tick face          |
|  [16]   | `DatePickerCalendarView`       | calendar surface         |
|  [17]   | `DatePickerCalendarDayButton`  | calendar day button      |
|  [18]   | `DatePickerCalendarYearButton` | calendar year button     |
|  [19]   | `DatePickerCalendarContext`    | calendar context         |
|  [20]   | `DateRange`                    | date-range value         |
|  [21]   | `DateRangeExtension`           | date-range extension     |
|  [22]   | `IDateSelector`                | date-selector contract   |
|  [23]   | `WeekendDateSelector`          | weekend selector         |

[CALENDAR_POLICY]: `DatePickerCalendarViewMode` governs the calendar surface.

[SHELL_CONTROLS]: window + view bases — `Ursa.Controls` / `Ursa.ReactiveUIExtension`

- rail: controls

`Ursa.ReactiveUIExtension` binds `ReactiveUrsaWindow` and `ReactiveUrsaView` to `UrsaWindow` and `UrsaView` as the admitted MVVM bases.

| [INDEX] | [SYMBOL]              | [KIND]                   |
| :-----: | :-------------------- | :----------------------- |
|  [01]   | `UrsaWindow`          | themed top-level window  |
|  [02]   | `UrsaView`            | themed user-control base |
|  [03]   | `SplashWindow`        | startup splash window    |
|  [04]   | `ReactiveUrsaWindow`  | reactive window base     |
|  [05]   | `ReactiveUrsaView`    | reactive view base       |
|  [06]   | `ThemeToggleButton`   | theme-variant toggle     |
|  [07]   | `ThemeSelectorBase`   | theme selector base      |
|  [08]   | `ThemeVariantMapper`  | theme-variant mapper     |
|  [09]   | `ThemeVariantMapping` | theme-variant mapping    |
|  [10]   | `DisableContainer`    | content-disable overlay  |
|  [11]   | `DisabledAdorner`     | disabled-content adorner |

[WINDOW_CHROME]: `UrsaWindow` composes a custom title bar with `WindowResizer` for its chromeless host.

[THEME_SELECTOR_POLICY]: `ThemeSelectorMode` governs theme-variant selection.

[CONTROL_ENUMS]: `Ursa.Controls` / `Ursa.Common` vocabulary

- rail: controls

| [INDEX] | [SYMBOL]                     | [RAIL]                  |
| :-----: | :--------------------------- | :---------------------- |
|  [01]   | `DialogButton`               | dialog button set       |
|  [02]   | `DialogResult`               | dialog result           |
|  [03]   | `DialogMode`                 | dialog modality         |
|  [04]   | `DialogLayerChangeType`      | dialog layer change     |
|  [05]   | `MessageBoxButton`           | message-box button set  |
|  [06]   | `MessageBoxResult`           | message-box result      |
|  [07]   | `MessageBoxIcon`             | message-box icon        |
|  [08]   | `MessageCloseReason`         | message close cause     |
|  [09]   | `Status`                     | feedback severity       |
|  [10]   | `PinCodeMode`                | PIN entry mode          |
|  [11]   | `IPv4BoxInputMode`           | IPv4 entry mode         |
|  [12]   | `TimeBoxInputMode`           | time entry mode         |
|  [13]   | `TimeBoxDragOrientation`     | time drag direction     |
|  [14]   | `TimelineDisplayMode`        | timeline layout         |
|  [15]   | `TimelineItemPosition`       | timeline position       |
|  [16]   | `TimelineItemType`           | timeline item kind      |
|  [17]   | `OverflowMode`               | overflow policy         |
|  [18]   | `LostFocusBehavior`          | commit policy           |
|  [19]   | `PopConfirmTriggerMode`      | confirmation trigger    |
|  [20]   | `ResizeDirection`            | resize policy           |
|  [21]   | `Direction`                  | direction policy        |
|  [22]   | `Position`                   | common placement        |
|  [23]   | `ItemAlignment`              | common alignment        |
|  [24]   | `CornerPosition`             | common corner placement |
|  [25]   | `HorizontalPosition`         | horizontal placement    |
|  [26]   | `VerticalPosition`           | vertical placement      |
|  [27]   | `DatePickerCalendarViewMode` | calendar view mode      |
|  [28]   | `UsePickerTypes`             | picker type policy      |
|  [29]   | `ThemeSelectorMode`          | theme selector mode     |
|  [30]   | `AspectRatioMode`            | aspect-ratio policy     |
|  [31]   | `OffsetDefinitionKind`       | offset definition       |
|  [32]   | `EccLevel`                   | QR correction level     |

[STATUS_VALUES]: `Status` admits `Info`, `Success`, `Warning`, and `Error`.

## [03]-[ENTRYPOINTS]

[DIALOG_DISPATCH]: `Dialog` (windowed) + `OverlayDialog` (in-canvas) static dispatch — vm-first, owner/host-id targeted, sync + async mirrors

- rail: controls

| [INDEX] | [MEMBER]                                    | [ROOT]                 | [MODE]             | [RESULT]             |
| :-----: | :------------------------------------------ | :--------------------- | :----------------- | :------------------- |
|  [01]   | `ShowModal<TView,TViewModel>`               | `Dialog`               | windowed modal     | `Task<DialogResult>` |
|  [02]   | `ShowCustomModal<TView,TViewModel,TResult>` | `Dialog`               | windowed custom    | `Task<TResult?>`     |
|  [03]   | `ShowStandard<TView,TViewModel>`            | `Dialog`               | windowed fire      | none                 |
|  [04]   | `ShowStandardAsync`                         | `Dialog`               | windowed awaited   | `Task<DialogResult>` |
|  [05]   | `ShowCustom<TView,TViewModel>`              | `Dialog`               | custom fire        | none                 |
|  [06]   | `ShowCustomAsync<...,TResult>`              | `Dialog`               | custom awaited     | `Task<TResult?>`     |
|  [07]   | `ShowModal<TView,TViewModel>`               | `OverlayDialog`        | overlay modal      | `Task<DialogResult>` |
|  [08]   | `ShowCustomModal<TResult>`                  | `OverlayDialog`        | cancellable custom | `Task<TResult?>`     |
|  [09]   | `ShowStandardAsync<TView,TViewModel>`       | `OverlayDialog`        | cancellable modal  | `Task<DialogResult>` |
|  [10]   | `RegisterHost`                              | `OverlayDialogManager` | register host      | none                 |
|  [11]   | `GetHost`                                   | `OverlayDialogManager` | resolve host       | host                 |
|  [12]   | `UnregisterHost`                            | `OverlayDialogManager` | unregister host    | none                 |

[DIALOG_SIGNATURE_01]: `ShowModal<TView,TViewModel>(vm, Window? owner, DialogOptions?) : Task<DialogResult>`.

[DIALOG_SIGNATURE_02]: `ShowCustomModal<TView,TViewModel,TResult>(vm, owner, DialogOptions?) : Task<TResult?>`.

[DIALOG_SIGNATURE_03]: `ShowStandard<TView,TViewModel>(vm, owner, DialogOptions?)`.

[DIALOG_SIGNATURE_04]: `ShowStandardAsync(...) : Task<DialogResult>`.

[DIALOG_SIGNATURE_05]: `ShowCustom<TView,TViewModel>(vm, owner, DialogOptions?)`.

[DIALOG_SIGNATURE_06]: `ShowCustomAsync<...,TResult>(...) : Task<TResult?>`.

[DIALOG_SIGNATURE_07]: `ShowModal<TView,TViewModel>(vm, string? hostId, OverlayDialogOptions?) : Task<DialogResult>`.

[DIALOG_SIGNATURE_08]: `ShowCustomModal<TResult>(vm, hostId, OverlayDialogOptions?, CancellationToken?) : Task<TResult?>`.

[DIALOG_SIGNATURE_09]: `ShowStandardAsync<TView,TViewModel>(vm, hostId, OverlayDialogOptions?, CancellationToken?) : Task<DialogResult>`.

[DIALOG_SIGNATURE_10]: `RegisterHost(OverlayDialogHost, id, hash)`.

[DIALOG_SIGNATURE_11]: `GetHost(id, hash)`.

[DIALOG_SIGNATURE_12]: `UnregisterHost(id, hash)`.

[DRAWER_DISPATCH]: `Drawer` slide-in panel dispatch — same vm-first shape, host-id targeted

- rail: controls

| [INDEX] | [MEMBER]                                    | [MODE]       | [RESULT]             |
| :-----: | :------------------------------------------ | :----------- | :------------------- |
|  [01]   | `Show<TView,TViewModel>`                    | non-modal    | none                 |
|  [02]   | `ShowModal<TView,TViewModel>`               | modal        | `Task<DialogResult>` |
|  [03]   | `ShowCustomModal<TView,TViewModel,TResult>` | custom modal | `Task<TResult?>`     |

[DRAWER_SIGNATURE_01]: `Show<TView,TViewModel>(vm, string? hostId, DrawerOptions?)`.

[DRAWER_SIGNATURE_02]: `ShowModal<TView,TViewModel>(vm, hostId, DrawerOptions?) : Task<DialogResult>`.

[DRAWER_SIGNATURE_03]: `ShowCustomModal<TView,TViewModel,TResult>(vm, hostId, DrawerOptions?) : Task<TResult?>`.

[MESSAGEBOX_DISPATCH]: `MessageBox` windowed + overlay confirmation — message/title/icon/button-first

- rail: controls

| [INDEX] | [MEMBER]           | [MODE]          | [RESULT]                 |
| :-----: | :----------------- | :-------------- | :----------------------- |
|  [01]   | `ShowAsync`        | windowed        | `Task<MessageBoxResult>` |
|  [02]   | `ShowAsync`        | owner-anchored  | `Task<MessageBoxResult>` |
|  [03]   | `ShowOverlayAsync` | registered host | `Task<MessageBoxResult>` |

[MESSAGEBOX_SIGNATURE_01]: `ShowAsync(string message, string? title, MessageBoxIcon, MessageBoxButton, string? styleClass) : Task<MessageBoxResult>`.

[MESSAGEBOX_SIGNATURE_02]: `ShowAsync(Window owner, string message, string title, MessageBoxIcon, MessageBoxButton, string?) : Task<MessageBoxResult>`.

[MESSAGEBOX_SIGNATURE_03]: `ShowOverlayAsync(string message, string? title, string? hostId, MessageBoxIcon, MessageBoxButton, int? toplevelHashCode, string?) : Task<MessageBoxResult>`.

[TOAST_NOTIFICATION_DISPATCH]: `IToastManager` / `INotificationManager` queue managers (installed onto a host visual, then `Show`)

- rail: controls

`WindowToastManager` implements `IToastManager`, `WindowNotificationManager` implements `INotificationManager`, and `IMessage` is the base of `IToast` and `INotification`.

| [INDEX] | [MEMBER]                              | [ROOT]                 | [RAIL]               |
| :-----: | :------------------------------------ | :--------------------- | :------------------- |
|  [01]   | `Show(IToast)`                        | `IToastManager`        | enqueue toast        |
|  [02]   | `Close(IToast)`                       | `IToastManager`        | dismiss toast        |
|  [03]   | `CloseAll()`                          | `IToastManager`        | drain toast queue    |
|  [04]   | `Show(INotification)`                 | `INotificationManager` | enqueue notification |
|  [05]   | `Close(...)`                          | `INotificationManager` | dismiss notification |
|  [06]   | `CloseAll()`                          | `INotificationManager` | drain notification   |
|  [07]   | `Type : NotificationType`             | `IMessage`             | message severity     |
|  [08]   | `Expiration`                          | `IMessage`             | message lifetime     |
|  [09]   | `ShowIcon`                            | `IMessage`             | icon visibility      |
|  [10]   | `ShowClose`                           | `IMessage`             | close visibility     |
|  [11]   | `OnClick`                             | `IMessage`             | click callback       |
|  [12]   | `OnClose(Action<MessageCloseReason>)` | `IMessage`             | close callback       |

[MESSAGE_SEVERITY]: `IMessage.Type` uses `Avalonia.Controls.Notifications.NotificationType`.

## [04]-[IMPLEMENTATION_LAW]

[CONTROLS_LAW]:

- Package: `Irihi.Ursa`
- Owns: the extended-control families the curated Avalonia + `bodong.PropertyGrid` + `Dock` + `DataGrid` roster does not — navigation (`NavMenu`/`Breadcrumb`/`Pagination`/`Anchor`/`ToolBar`), feedback (`Toast`/`Notification`/`Banner`/`MessageBox`/`Loading`/`Skeleton`/`PopConfirm`), overlay (`OverlayDialogHost`/`Drawer`), data entry (`Form`/the typed `NumericUpDown` family/`PinCode`/`IPv4Box`/`TimeBox`/`TagInput`/`KeyGestureInput`/`PathPicker`), selection (`MultiComboBox`/`TreeComboBox`/`SelectionList`/`Rating`/`RangeSlider`), display (`Timeline`/`Avatar`/`Badge`/`Marquee`/`Descriptions`/`QRCode`), and the full date/time/range picker family.
- Accept: `Shell/Controls` composes Ursa controls for these families and reuses `Ursa.Common` placement (`Position`/`ItemAlignment`/`CornerPosition`) and the per-control enums as the policy vocabulary; the typed `Numeric<T>UpDown` variant matching the bound CLR type is used, never a string-parsed generic spinner.
- Reject: hand-rolling a nav menu, timeline, OTP field, masked IP/time box, multi-select combo, or rating control that Ursa already owns; declaring a parallel placement enum where `Ursa.Common`/the control enum already names the axis.

[OVERLAY_LAW]:

- The overlay families are the in-canvas counterpart to `DialogHost.Avalonia` (`api-dialoghost.md`): one `OverlayDialogHost` registered per shell region (id via `OverlayDialogManager.RegisterHost`), and view-models raise dialogs/drawers/confirms through the static `OverlayDialog.*`/`Drawer.*`/`MessageBox.ShowOverlayAsync` dispatch against that id — never by instantiating a popup or mutating the visual tree.
- Accept: the awaited `ShowModal`/`ShowCustomModal<TResult>`/`ShowStandardAsync` overloads return `Task<DialogResult>`/`Task<TResult?>` and bind into a LanguageExt `Eff`/`OptionT` rail at the boundary, threading the supplied `CancellationToken` through the overlay's lifetime; `DialogOptions`/`OverlayDialogOptions`/`DrawerOptions` carry button set, modality, placement, and resize policy.
- Reject: a second overlay-host layer beside `OverlayDialogHost` (or beside `DialogHost`); blocking on `.Result` instead of awaiting the dispatch; constructing dialog views by hand instead of the `<TView,TViewModel>` vm-first generic overloads.

[FEEDBACK_LAW]:

- `WindowToastManager`/`WindowNotificationManager`/`WindowMessageManager` are installed once onto a host visual; product code resolves the matching `IToastManager`/`INotificationManager` and calls `Show(IToast/INotification)`/`Close`/`CloseAll`. Each message implements `IMessage` (`Type` severity, `Expiration`, `OnClick`, `OnClose(Action<MessageCloseReason>)`), so transient feedback is queued through one manager rather than ad-hoc adorners.
- Accept: notification severity maps from the product `Status`/`ControlIntent` vocabulary onto `IMessage.Type` (Avalonia's `NotificationType`); `OnClose` receives `MessageCloseReason` (user vs timeout vs programmatic) to drive analytics or follow-up.
- Reject: per-screen toast stacks; raising a `Notification`/`Toast` control directly into the visual tree instead of through its manager; ignoring `MessageCloseReason` where dismissal cause is load-bearing.

[THEME_BRIDGE_LAW]:

- Package: `Irihi.Ursa.Themes.Semi` + `Irihi.Ursa.ReactiveUIExtension`
  [URSA_SEMI_THEME]:
  `Irihi.Ursa.Themes.Semi` is the `UrsaSemiTheme: Styles` compiled-AXAML control-theme dictionary (`<semi:UrsaSemiTheme/>`, plus the thin `Locale`/`OverrideLocaleResources` code surface and the public `Ursa.Themes.Semi.Converters` set). It sits at the END of the single `Application.Styles` chain `FluentTheme floor -> <semi:SemiTheme/> -> the per-control Semi.Avalonia.* skins -> <semi:UrsaSemiTheme/>` so every Ursa control resolves the shared OKLCH token system the `Wacton.Unicolour` pipeline materializes into the `SemiTheme` slots.

[URSA_SHARED_PRIMITIVES]:
The theme binds the SAME `https://irihi.tech/semi` (`semi:`) xmlns `Semi.Avalonia` publishes; the obsolete `Ursa.Themes.Semi.Legacy.SemiTheme` (`u-semi:`) is NOT used. `Irihi.Avalonia.Shared`/`Irihi.Avalonia.Shared.Contracts` is the shared primitive closure both suites floor on; its `UrsaSemiTheme.OverrideLocaleResources` mirrors `Semi.Avalonia.SemiTheme.OverrideLocaleResources`, so a culture swap must drive BOTH locale dictionaries.

- `ReactiveUrsaWindow`/`ReactiveUrsaView` (`Irihi.Ursa.ReactiveUIExtension`) are the admitted MVVM view bases — they bind `UrsaWindow`/`UrsaView` onto the admitted `ReactiveUI`/`ReactiveUI.Avalonia` rail (`api-reactiveui-avalonia.md`); shell views derive from these, not from the plain `UrsaView` or a hand-wired `ReactiveUserControl`.
- Reject: loading `UrsaSemiTheme` without `SemiTheme`, or ahead of the per-control Semi skins, instead of last in the chain (controls render unstyled or skin-shadowed); deriving shell views from the non-reactive bases when the ReactiveUI bridge is admitted; re-pinning `Irihi.Avalonia.Shared` to a divergent version from the Semi closure; using the obsolete `Ursa.Themes.Semi.Legacy.SemiTheme` (`u-semi:`) in place of `UrsaSemiTheme`.
