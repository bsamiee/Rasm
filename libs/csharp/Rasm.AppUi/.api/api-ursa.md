# [RASM_APPUI_API_URSA]

`Irihi.Ursa` is the extended-control suite (assembly `Ursa.dll`) filling the control families the curated Avalonia roster lacks. Control visuals live in the sibling `Irihi.Ursa.Themes.Semi` (`UrsaSemiTheme : Styles`) under the `Semi.Avalonia` token system, and `Irihi.Ursa.ReactiveUIExtension` binds the Ursa view bases onto the admitted ReactiveUI rail. Every overlay — dialog, drawer, message box — is raised vm-first through a static dispatcher against a registered host id, the in-canvas counterpart to `DialogHost.Avalonia`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Irihi.Ursa`
- package: `Irihi.Ursa` (MIT)
- assembly: `Ursa`
- namespace: `Ursa.Controls`, `Ursa.Controls.Options`, `Ursa.Common`, `Ursa.Converters`, `Ursa.Helpers`, `Ursa.EventArgs`
- depends: `Avalonia`, `Irihi.Avalonia.Shared`, `Irihi.Avalonia.Shared.Contracts` (the shared primitive closure `Semi.Avalonia` also floors on); `QRCode` vendors `Gma.QrCodeNet` internally, so no QR dependency surfaces
- rail: controls

[PACKAGE_SURFACE]: `Irihi.Ursa.Themes.Semi`
- package: `Irihi.Ursa.Themes.Semi` (MIT)
- assembly: `Ursa.Themes.Semi`
- namespace: `Ursa.Themes.Semi` (`UrsaSemiTheme : Styles`), `Ursa.Themes.Semi.Converters`, `Ursa.Themes.Semi.Locale`
- depends: `Ursa`; publishes control themes under the `https://irihi.tech/semi` (`semi:`) xmlns `Semi.Avalonia` shares
- rail: theme

[PACKAGE_SURFACE]: `Irihi.Ursa.ReactiveUIExtension`
- package: `Irihi.Ursa.ReactiveUIExtension` (MIT)
- assembly: `Ursa.ReactiveUIExtension`
- namespace: `Ursa.ReactiveUIExtension`
- depends: `Ursa`, `ReactiveUI`
- rail: mvvm-bridge

## [02]-[PUBLIC_TYPES]

[NAVIGATION_CONTROLS]: navigation and wayfinding — `Ursa.Controls`

| [INDEX] | [SYMBOL]                                        | [CAPABILITY]                                                  |
| :-----: | :---------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `NavMenu` / `NavMenuItem`                       | hierarchical side/top navigation menu (`ItemsControl`) + item |
|  [02]   | `Breadcrumb` / `BreadcrumbItem`                 | breadcrumb trail                                              |
|  [03]   | `Pagination` / `PaginationButton`               | page-navigation strip                                         |
|  [04]   | `Anchor` / `AnchorItem`                         | scroll-spy anchor index over a scroll region                  |
|  [05]   | `ScrollTo` / `ScrollToButton`                   | programmatic scroll-into-view target + trigger                |
|  [06]   | `ToolBar` / `ToolBarPanel` / `ToolBarSeparator` | overflow-aware tool bar (`OverflowMode`)                      |
|  [07]   | `TitleBar`                                      | custom window title bar (chromeless host)                     |

[FEEDBACK_CONTROLS]: toast, notification, banner, message, busy — `Ursa.Controls`

| [INDEX] | [SYMBOL]                    | [CAPABILITY]                   |
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

[OVERLAY_CONTROLS]: in-canvas dialog and drawer host layer — `Ursa.Controls`

`OverlayDialogHost` is the registered-id target for `OverlayDialog`, `Drawer`, and `MessageBox.ShowOverlayAsync`; a consumer sets its `HostId` to register the host.

| [INDEX] | [SYMBOL]                | [CAPABILITY]                  |
| :-----: | :---------------------- | :---------------------------- |
|  [01]   | `OverlayDialogHost`     | in-canvas overlay host        |
|  [02]   | `Dialog`                | windowed dialog dispatcher    |
|  [03]   | `DialogControlBase`     | dialog control base           |
|  [04]   | `DialogOptions`         | windowed dialog options       |
|  [05]   | `DialogResult`          | dialog result                 |
|  [06]   | `DialogButton`          | dialog button set             |
|  [07]   | `DialogMode`            | dialog modality               |
|  [08]   | `OverlayDialog`         | overlay dialog dispatcher     |
|  [09]   | `OverlayDialogOptions`  | overlay dialog options        |
|  [10]   | `Drawer`                | drawer dispatcher             |
|  [11]   | `DrawerControlBase`     | drawer control base           |
|  [12]   | `OverlayDrawer`         | slide-in drawer surface       |
|  [13]   | `DialogResizer`         | edge-resize surface           |
|  [14]   | `DrawerOptions`         | drawer options                |
|  [15]   | `CustomDialogControl`   | custom dialog shell           |
|  [16]   | `StandardDialogControl` | titled, buttoned dialog shell |
|  [17]   | `CustomDrawerControl`   | custom drawer shell           |
|  [18]   | `StandardDrawerControl` | titled, buttoned drawer shell |

[DATA_ENTRY_CONTROLS]: form, numeric, masked, gesture, tag entry — `Ursa.Controls`

`Numeric<T>UpDown` admits `Byte`, `SByte`, `Short`, `UShort`, `Int`, `UInt`, `Long`, `ULong`, `Float`, `Double`, and `Decimal`; `<T>Displayer` admits `Double`, `Int32`, and `Int64`.

| [INDEX] | [SYMBOL]              | [CAPABILITY]                    |
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

[SELECTION_CONTROLS]: multi-select, combo, rating, button-group — `Ursa.Controls`

| [INDEX] | [SYMBOL]                            | [CAPABILITY]                |
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
|  [11]   | `SelectionChangingEventArgs`        | selection-change event      |
|  [12]   | `ButtonGroup`                       | mutually exclusive segments |
|  [13]   | `Rating`                            | star-or-character input     |
|  [14]   | `RatingCharacter`                   | rating glyph                |
|  [15]   | `RangeSlider`                       | dual-thumb range input      |
|  [16]   | `RangeTrack`                        | range track                 |
|  [17]   | `RangeValueChangedEventArgs`        | range-change event          |
|  [18]   | `ClosableTag`                       | dismissible tag chip        |

[DISPLAY_CONTROLS]: timeline, avatar, badge, marquee, descriptions, QR — `Ursa.Controls`

| [INDEX] | [SYMBOL]                | [CAPABILITY]             |
| :-----: | :---------------------- | :----------------------- |
|  [01]   | `Timeline`              | event timeline           |
|  [02]   | `TimelineItem`          | timeline event           |
|  [03]   | `TimelinePanel`         | timeline layout panel    |
|  [04]   | `Avatar`                | image-or-initial avatar  |
|  [05]   | `Badge`                 | count-or-dot badge       |
|  [06]   | `DualBadge`             | dual-segment badge       |
|  [07]   | `Marquee`               | scrolling-text ticker    |
|  [08]   | `Descriptions`          | key-value grid           |
|  [09]   | `DescriptionsItem`      | description entry        |
|  [10]   | `Divider`               | titled divider           |
|  [11]   | `ImageViewer`           | pan-zoom image viewer    |
|  [12]   | `QRCode`                | QR renderer (`EccLevel`) |
|  [13]   | `IconButton`            | icon-leading button      |
|  [14]   | `IconToggleButton`      | icon toggle button       |
|  [15]   | `IconSplitButton`       | icon split button        |
|  [16]   | `IconToggleSplitButton` | icon toggle split        |
|  [17]   | `IconDropDownButton`    | icon drop-down button    |
|  [18]   | `IconRepeatButton`      | icon repeat button       |
|  [19]   | `TwoTonePathIcon`       | two-tone glyph           |
|  [20]   | `LabeledContentControl` | labeled container        |
|  [21]   | `GroupBoxBorder`        | group border             |
|  [22]   | `UrsaGroupBox`          | group container          |

[PICKER_CONTROLS]: date / time / range picker family — `Ursa.Controls`

| [INDEX] | [SYMBOL]                       | [CAPABILITY]                                    |
| :-----: | :----------------------------- | :---------------------------------------------- |
|  [01]   | `DatePicker`                   | calendar date picker                            |
|  [02]   | `DateRangePicker`              | calendar range picker                           |
|  [03]   | `DateOnlyPicker`               | `DateOnly` picker                               |
|  [04]   | `DateOnlyRangePicker`          | `DateOnly` range picker                         |
|  [05]   | `DateTimePicker`               | date-time picker                                |
|  [06]   | `DateTimeOffsetPicker`         | date-time-offset picker                         |
|  [07]   | `DateOffsetPicker`             | date-offset picker                              |
|  [08]   | `DateOffsetRangePicker`        | date-offset range picker                        |
|  [09]   | `TimePicker`                   | time picker                                     |
|  [10]   | `TimeRangePicker`              | time range picker                               |
|  [11]   | `TimeOnlyPicker`               | `TimeOnly` picker                               |
|  [12]   | `TimeOnlyRangePicker`          | `TimeOnly` range picker                         |
|  [13]   | `TimePickerPresenter`          | time picker presenter                           |
|  [14]   | `Clock`                        | analog clock                                    |
|  [15]   | `ClockTicks`                   | clock tick face                                 |
|  [16]   | `DatePickerCalendarView`       | calendar surface (`DatePickerCalendarViewMode`) |
|  [17]   | `DatePickerCalendarDayButton`  | calendar day button                             |
|  [18]   | `DatePickerCalendarYearButton` | calendar year button                            |
|  [19]   | `DatePickerCalendarContext`    | calendar context                                |
|  [20]   | `DateRange`                    | date-range value                                |
|  [21]   | `DateRangeExtension`           | date-range extension                            |
|  [22]   | `IDateSelector`                | date-selector contract                          |
|  [23]   | `WeekendDateSelector`          | weekend selector                                |

[SHELL_CONTROLS]: window and view bases — `Ursa.Controls` / `Ursa.ReactiveUIExtension`

| [INDEX] | [SYMBOL]                         | [CAPABILITY]                              |
| :-----: | :------------------------------- | :---------------------------------------- |
|  [01]   | `UrsaWindow`                     | themed chromeless top-level window        |
|  [02]   | `UrsaView`                       | themed user-control base                  |
|  [03]   | `SplashWindow`                   | startup splash window                     |
|  [04]   | `ReactiveUrsaWindow<TViewModel>` | reactive window base                      |
|  [05]   | `ReactiveUrsaView<TViewModel>`   | reactive view base                        |
|  [06]   | `ThemeToggleButton`              | theme-variant toggle                      |
|  [07]   | `ThemeSelectorBase`              | theme selector base (`ThemeSelectorMode`) |
|  [08]   | `ThemeVariantMapper`             | theme-variant mapper                      |
|  [09]   | `ThemeVariantMapping`            | theme-variant mapping                     |
|  [10]   | `DisableContainer`               | content-disable overlay                   |
|  [11]   | `DisabledAdorner`                | disabled-content adorner                  |

[CONTROL_ENUMS]: `Ursa.Controls` / `Ursa.Common` policy vocabulary

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                                           |
| :-----: | :--------------------------- | :----------------------------------------------------- |
|  [01]   | `DialogButton`               | dialog button set                                      |
|  [02]   | `DialogResult`               | dialog result                                          |
|  [03]   | `DialogMode`                 | dialog modality                                        |
|  [04]   | `DialogLayerChangeType`      | dialog layer change                                    |
|  [05]   | `MessageBoxButton`           | message-box button set                                 |
|  [06]   | `MessageBoxResult`           | message-box result                                     |
|  [07]   | `MessageBoxIcon`             | message-box icon                                       |
|  [08]   | `MessageCloseReason`         | message close cause                                    |
|  [09]   | `Status`                     | feedback severity (`Info`/`Success`/`Warning`/`Error`) |
|  [10]   | `PinCodeMode`                | PIN entry mode                                         |
|  [11]   | `IPv4BoxInputMode`           | IPv4 entry mode                                        |
|  [12]   | `TimeBoxInputMode`           | time entry mode                                        |
|  [13]   | `TimeBoxDragOrientation`     | time drag direction                                    |
|  [14]   | `TimelineDisplayMode`        | timeline layout                                        |
|  [15]   | `TimelineItemPosition`       | timeline position                                      |
|  [16]   | `TimelineItemType`           | timeline item kind                                     |
|  [17]   | `OverflowMode`               | overflow policy                                        |
|  [18]   | `LostFocusBehavior`          | commit policy                                          |
|  [19]   | `PopConfirmTriggerMode`      | confirmation trigger                                   |
|  [20]   | `ResizeDirection`            | resize policy                                          |
|  [21]   | `Direction`                  | direction policy                                       |
|  [22]   | `Position`                   | common placement                                       |
|  [23]   | `ItemAlignment`              | common alignment                                       |
|  [24]   | `CornerPosition`             | common corner placement                                |
|  [25]   | `HorizontalPosition`         | horizontal placement                                   |
|  [26]   | `VerticalPosition`           | vertical placement                                     |
|  [27]   | `DatePickerCalendarViewMode` | calendar view mode                                     |
|  [28]   | `UsePickerTypes`             | picker type policy                                     |
|  [29]   | `ThemeSelectorMode`          | theme selector mode                                    |
|  [30]   | `AspectRatioMode`            | aspect-ratio policy                                    |
|  [31]   | `OffsetDefinitionKind`       | offset definition                                      |
|  [32]   | `EccLevel`                   | QR correction level                                    |

[THEME_TYPES]: control-theme bridge and template converters — `Ursa.Themes.Semi`

| [INDEX] | [SYMBOL]                                | [CAPABILITY]                                                |
| :-----: | :-------------------------------------- | :---------------------------------------------------------- |
|  [01]   | `UrsaSemiTheme`                         | `Styles` control-theme dictionary (`<semi:UrsaSemiTheme/>`) |
|  [02]   | `BooleansToOpacityConverter`            | booleans-to-opacity converter                               |
|  [03]   | `BrushToColorConverter`                 | brush-to-color converter                                    |
|  [04]   | `ClockHandLengthConverter`              | clock-hand-length converter (`double ratio`)                |
|  [05]   | `FormContentHeightToAlignmentConverter` | form content-height to alignment                            |
|  [06]   | `FormContentHeightToMarginConverter`    | form content-height to margin                               |
|  [07]   | `NavMenuMarginConverter`                | nav-menu margin converter                                   |
|  [08]   | `TreeLevelToPaddingConverter`           | tree-level to padding converter                             |

## [03]-[ENTRYPOINTS]

[DIALOG_DISPATCH]: `Dialog` (windowed, owner `Window?`) and `OverlayDialog` (in-canvas, `string? hostId`, threading a `CancellationToken?`) dispatch vm-first through `<TView,TViewModel>` generic overloads (custom overloads add `,TResult>`); modal and awaited overloads return `Task<DialogResult>`, custom overloads `Task<TResult?>`, fire overloads void

| [INDEX] | [SURFACE]                                                                                    | [CAPABILITY]               |
| :-----: | :------------------------------------------------------------------------------------------- | :------------------------- |
|  [01]   | `Dialog.ShowModal(TVm, Window?, DialogOptions?)`                                             | windowed modal             |
|  [02]   | `Dialog.ShowCustomModal(TVm, Window?, DialogOptions?)`                                       | windowed custom modal      |
|  [03]   | `Dialog.ShowStandard(TVm, Window?, DialogOptions?)`                                          | windowed fire              |
|  [04]   | `Dialog.ShowStandardAsync(TVm, Window?, DialogOptions?)`                                     | windowed awaited           |
|  [05]   | `Dialog.ShowCustom(TVm, Window?, DialogOptions?)`                                            | custom fire                |
|  [06]   | `Dialog.ShowCustomAsync(TVm, Window?, DialogOptions?)`                                       | custom awaited             |
|  [07]   | `OverlayDialog.ShowModal(TVm, string?, OverlayDialogOptions?, CancellationToken?)`           | overlay modal              |
|  [08]   | `OverlayDialog.ShowCustomModal(object?, string?, OverlayDialogOptions?, CancellationToken?)` | overlay cancellable custom |
|  [09]   | `OverlayDialog.ShowStandardAsync(TVm, string?, OverlayDialogOptions?, CancellationToken?)`   | overlay awaited            |
|  [10]   | `OverlayDialog.Show(TVm, string?, OverlayDialogOptions?)`                                    | overlay non-modal          |

[DRAWER_DISPATCH]: `Drawer` slide-in panel static dispatch — vm-first `<TView,TViewModel>` overloads (custom adds `,TResult>`), `string? hostId` targeted; `ShowModal` returns `Task<DialogResult>`, `ShowCustomModal` `Task<TResult?>`, `Show` void

| [INDEX] | [SURFACE]                                              | [CAPABILITY] |
| :-----: | :----------------------------------------------------- | :----------- |
|  [01]   | `Drawer.Show(TVm, string?, DrawerOptions?)`            | non-modal    |
|  [02]   | `Drawer.ShowModal(TVm, string?, DrawerOptions?)`       | modal        |
|  [03]   | `Drawer.ShowCustomModal(TVm, string?, DrawerOptions?)` | custom modal |

[MESSAGEBOX_DISPATCH]: `MessageBox` windowed and overlay confirmation — message/title/icon/button-first, each returning `Task<MessageBoxResult>`

| [INDEX] | [SURFACE]                                                                                                | [CAPABILITY]    |
| :-----: | :------------------------------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `MessageBox.ShowAsync(string, string?, MessageBoxIcon, MessageBoxButton, string?)`                       | windowed        |
|  [02]   | `MessageBox.ShowAsync(Window, string, string, MessageBoxIcon, MessageBoxButton, string?)`                | owner-anchored  |
|  [03]   | `MessageBox.ShowOverlayAsync(string, string?, string?, MessageBoxIcon, MessageBoxButton, int?, string?)` | registered host |

[TOAST_NOTIFICATION_DISPATCH]: `IToastManager` / `INotificationManager` queue managers — installed onto a host visual, then `Show`

`WindowToastManager` implements `IToastManager`, `WindowNotificationManager` implements `INotificationManager`, and `IMessage` is the base of `IToast` and `INotification`.

| [INDEX] | [SURFACE]                                      | [CAPABILITY]         |
| :-----: | :--------------------------------------------- | :------------------- |
|  [01]   | `IToastManager.Show(IToast)`                   | enqueue toast        |
|  [02]   | `IToastManager.Close(IToast)`                  | dismiss toast        |
|  [03]   | `IToastManager.CloseAll()`                     | drain toast queue    |
|  [04]   | `INotificationManager.Show(INotification)`     | enqueue notification |
|  [05]   | `INotificationManager.Close(...)`              | dismiss notification |
|  [06]   | `INotificationManager.CloseAll()`              | drain notification   |
|  [07]   | `IMessage.Type : NotificationType`             | message severity     |
|  [08]   | `IMessage.Expiration`                          | message lifetime     |
|  [09]   | `IMessage.ShowIcon`                            | icon visibility      |
|  [10]   | `IMessage.ShowClose`                           | close visibility     |
|  [11]   | `IMessage.OnClick`                             | click callback       |
|  [12]   | `IMessage.OnClose(Action<MessageCloseReason>)` | close callback       |

[THEME_INSTALL]: `UrsaSemiTheme` install and locale surface — `Ursa.Themes.Semi` (the only code surface; the rest is XAML resource lookup)

| [INDEX] | [SURFACE]                                                            | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `<semi:UrsaSemiTheme/>`                                              | install Ursa control themes last in the chain           |
|  [02]   | `UrsaSemiTheme(IServiceProvider?)`                                   | ctor                                                    |
|  [03]   | `UrsaSemiTheme.Locale`                                               | select the culture (`CultureInfo?`, `zh-CN` on unknown) |
|  [04]   | `UrsaSemiTheme.OverrideLocaleResources(Application, CultureInfo?)`   | app-scoped locale override                              |
|  [05]   | `UrsaSemiTheme.OverrideLocaleResources(StyledElement, CultureInfo?)` | element-scoped locale override                          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every overlay — dialog, drawer, confirm — is raised vm-first through the static `OverlayDialog`/`Drawer`/`MessageBox.ShowOverlayAsync` dispatch against a registered host id; one `OverlayDialogHost` per shell region carries its `HostId`, and no code instantiates a popup or mutates the visual tree.
- Transient feedback queues through one installed `WindowToastManager`/`WindowNotificationManager`/`WindowMessageManager`; product code resolves the matching `IToastManager`/`INotificationManager` and calls `Show`/`Close`/`CloseAll`, every message an `IMessage`.
- Control visuals resolve the shared `semi:` OKLCH token system with `<semi:UrsaSemiTheme/>` last in the single `Application.Styles` chain.

[STACKING]:
- `api-semi.md`: `UrsaSemiTheme` extends the `Semi.Avalonia` token system under the shared `https://irihi.tech/semi` (`semi:`) xmlns; the chain is `FluentTheme` floor -> `<semi:SemiTheme/>` -> the per-control `Semi.Avalonia.*` skins -> `<semi:UrsaSemiTheme/>`, every Ursa entry below `SemiTheme` so its tokens resolve, and `UrsaSemiTheme.OverrideLocaleResources` mirrors `SemiTheme.OverrideLocaleResources` so a culture swap drives both locale dictionaries.
- `api-dialoghost.md`: `OverlayDialog`/`Drawer`/`MessageBox` mirror `DialogHost.Avalonia` in-canvas, sharing its vm-first-against-a-host-id dispatch shape.
- `api-reactiveui-avalonia.md`: `ReactiveUrsaWindow<TViewModel>`/`ReactiveUrsaView<TViewModel>` bind `UrsaWindow`/`UrsaView` onto the `ReactiveUI.Avalonia` rail as the admitted MVVM view bases.
- within-lib: the awaited `ShowModal`/`ShowCustomModal<TResult>`/`ShowStandardAsync` overloads return `Task<DialogResult>`/`Task<TResult?>` and bind into a LanguageExt `Eff`/`OptionT` rail at the boundary with the supplied `CancellationToken` threaded; notification severity maps the product `Status`/`ControlIntent` vocabulary onto `IMessage.Type`, and `OnClose(MessageCloseReason)` drives dismissal-cause follow-up.

[LOCAL_ADMISSION]:
- `Shell/Controls` composes Ursa for the families the curated Avalonia + `bodong.PropertyGrid` + `Dock` + `DataGrid` roster lacks, reusing `Ursa.Common` placement (`Position`/`ItemAlignment`/`CornerPosition`) and the per-control enums as policy vocabulary; the typed `Numeric<T>UpDown` matching the bound CLR type carries numeric entry, and shell views derive from the `ReactiveUrsa` bases.
- `DialogOptions`/`OverlayDialogOptions`/`DrawerOptions` carry button set, modality, placement, and resize policy; `Irihi.Avalonia.Shared` stays the one shared primitive closure both suites floor on.

[RAIL_LAW]:
- Package: `Irihi.Ursa` + `Irihi.Ursa.Themes.Semi` + `Irihi.Ursa.ReactiveUIExtension`
- Owns: the extended-control families (navigation, feedback, overlay, data entry, selection, display, the date/time/range pickers), in-canvas overlay/drawer/confirm dispatch, queued transient feedback, the `UrsaSemiTheme` control-theme bridge, and the `ReactiveUrsa` MVVM bases
- Accept: vm-first static dispatch against a host id returning `Task<DialogResult>`/`Task<TResult?>` awaited into an `Eff`/`OptionT` rail with the `CancellationToken` threaded; one installed manager per feedback family; `<semi:UrsaSemiTheme/>` last in the `semi:` chain
- Reject: hand-rolling a nav menu, timeline, OTP field, masked IP/time box, multi-select combo, or rating Ursa owns; a second overlay-host layer or per-screen feedback stack; blocking on `.Result` instead of awaiting the dispatch; declaring a parallel placement enum where `Ursa.Common` names the axis; loading `UrsaSemiTheme` without or ahead of `SemiTheme`; deriving shell views from the non-reactive bases when the ReactiveUI bridge is admitted; re-pinning `Irihi.Avalonia.Shared` divergent from the Semi closure
