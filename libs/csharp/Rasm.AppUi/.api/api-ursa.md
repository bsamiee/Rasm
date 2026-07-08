# [RASM_APPUI_API_URSA]

`Irihi.Ursa` is the extended-control suite (assembly `Ursa.dll`) that fills the families the curated Avalonia roster lacks — navigation (`NavMenu`/`Breadcrumb`/`Pagination`/`Anchor`), feedback (`Toast`/`Notification`/`Banner`/`MessageBox`/`Loading`/`Skeleton`), overlay (`OverlayDialogHost`/`Drawer`/`PopConfirm`), data entry (`Form`/`NumericUpDown`/`PinCode`/`IPv4Box`/`TimeBox`/`TagInput`/`KeyGestureInput`/the full date/time/range picker family), and display (`Timeline`/`Avatar`/`Badge`/`Rating`/`Marquee`/`Descriptions`/`Divider`/`QRCode`). The control *visuals* live in the sibling `Irihi.Ursa.Themes.Semi` (assembly `Ursa.Themes.Semi.dll`, the `UrsaSemiTheme : Styles` control-theme dictionary added via `<semi:UrsaSemiTheme/>`, one compiled `.axaml` per control) layered under the `Semi.Avalonia` token system (`api-semi.md`) — both suites bind the SAME `https://irihi.tech/semi` (`semi:`) xmlns, so the canonical `Application.Styles` chain is `FluentTheme` floor -> `<semi:SemiTheme/>` -> the per-control `Semi.Avalonia.*` skins -> `<semi:UrsaSemiTheme/>`; `Irihi.Ursa.ReactiveUIExtension` (assembly `Ursa.ReactiveUIExtension.dll`) bridges the two view bases onto the admitted ReactiveUI rail. The overlay families (`OverlayDialog`/`Drawer`/`MessageBox.ShowOverlayAsync`/`WindowToastManager`/`WindowNotificationManager`) are the in-canvas counterpart to `DialogHost.Avalonia` (`api-dialoghost.md`): a view-model raises an overlay through a static dispatch helper against a host id, never by hand-building a popup.

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
- namespace: `Ursa.Themes.Semi` (the `UrsaSemiTheme : Styles` entry, xmlns `https://irihi.tech/semi` / prefix `semi` — the SAME xmlns `Semi.Avalonia` publishes, so `<semi:.../>` resolves both), `Ursa.Themes.Semi.Converters` (the public template converters), `Ursa.Themes.Semi.Locale` / `Ursa.Themes.Semi.SizeAnimations` (resource dictionaries); the obsolete `Ursa.Themes.Semi.Legacy.SemiTheme` (xmlns `https://irihi.tech/ursa/themes/semi` / prefix `u-semi`) is `[Obsolete("…use UrsaSemiTheme instead")]` and is NOT admitted
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

| [INDEX] | [SYMBOL]                                  | [KIND]                                                          |
| :-----: | :---------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `NavMenu` / `NavMenuItem`                 | hierarchical side/top navigation menu (`ItemsControl`) + item  |
|  [02]   | `Breadcrumb` / `BreadcrumbItem`           | breadcrumb trail                                               |
|  [03]   | `Pagination` / `PaginationButton`         | page-navigation strip                                          |
|  [04]   | `Anchor` / `AnchorItem`                   | scroll-spy anchor index over a scroll region                   |
|  [05]   | `ScrollTo` / `ScrollToButton`             | programmatic scroll-into-view target + trigger                 |
|  [06]   | `ToolBar` / `ToolBarPanel` / `ToolBarSeparator` | overflow-aware tool bar (`OverflowMode`)                  |
|  [07]   | `TitleBar`                                | custom window title bar (chromeless host)                      |

[FEEDBACK_CONTROLS]: toast, notification, banner, message, busy — `Ursa.Controls`
- rail: controls

| [INDEX] | [SYMBOL]                                            | [KIND]                                                           |
| :-----: | :-------------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `WindowToastManager` / `Toast` / `ToastCard` / `IToast` / `IToastManager` | transient toast queue + host manager (`Show`/`Close`/`CloseAll`) |
|  [02]   | `WindowNotificationManager` / `Notification` / `NotificationCard` / `INotification` / `INotificationManager` | corner notification queue + host manager |
|  [03]   | `WindowMessageManager` / `MessageCard` / `IMessage` | message-stack manager + base message contract                  |
|  [04]   | `Banner`                                            | inline page-level banner (`Status` severity)                   |
|  [05]   | `MessageBox` / `MessageBoxControl` / `MessageBoxWindow` / `OverlayMessageBox` | windowed + overlay modal message box (static `ShowAsync`/`ShowOverlayAsync`) |
|  [06]   | `Loading` / `LoadingContainer` / `LoadingIcon`      | busy spinner + content-wrapping busy overlay                   |
|  [07]   | `Skeleton`                                          | shimmer placeholder for loading content                        |
|  [08]   | `PopConfirm`                                        | inline confirmation popover (`PopConfirmTriggerMode`)          |

[OVERLAY_CONTROLS]: in-canvas dialog + drawer host layer — `Ursa.Controls`
- rail: controls

| [INDEX] | [SYMBOL]                                                          | [KIND]                                                          |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `OverlayDialogHost`                                              | the in-canvas dialog/drawer host (registered by id; the surface `OverlayDialog`/`Drawer`/`MessageBox.ShowOverlayAsync` target) |
|  [02]   | `OverlayDialogManager`                                           | static host registry — `RegisterHost`/`UnregisterHost`/`GetHost(id, hash)` |
|  [03]   | `Dialog` (static) / `DialogControlBase` / `DialogOptions` / `DialogResult` / `DialogButton` / `DialogMode` | windowed dialog dispatch + result vocabulary |
|  [04]   | `OverlayDialog` (static) / `OverlayDialogOptions`               | overlay (in-canvas) dialog dispatch                            |
|  [05]   | `Drawer` (static) / `DrawerControlBase` / `OverlayDrawer` / `DialogResizer` (+ `Ursa.Controls.Options.DrawerOptions`) | slide-in drawer dispatch + edge-resizable surface |
|  [06]   | `CustomDialogControl` / `StandardDialogControl` / `CustomDrawerControl` / `StandardDrawerControl` | custom vs standard (titled+buttoned) dialog/drawer shells |

[DATA_ENTRY_CONTROLS]: form, numeric, masked, gesture, tag entry — `Ursa.Controls`
- rail: controls

| [INDEX] | [SYMBOL]                                                          | [KIND]                                                          |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Form` / `FormGroup` / `FormItem`                               | declarative label+field+validation form layout                |
|  [02]   | `NumericUpDown` / `NumericUpDownBase` + per-CLR-type `Numeric{Byte,SByte,Short,UShort,Int,UInt,Long,ULong,Float,Double,Decimal}UpDown` | strongly-typed numeric spinners |
|  [03]   | `NumberDisplayer` / `NumberDisplayerBase` + `{Double,Int32,Int64}Displayer` | animated rolling-number display                       |
|  [04]   | `PinCode` / `PinCodeItem` / `PinCodeCollection`                | segmented PIN/OTP entry (`PinCodeMode`)                        |
|  [05]   | `IPv4Box`                                                       | masked IPv4 octet entry (`IPv4BoxInputMode`)                   |
|  [06]   | `TimeBox`                                                       | masked time-component entry (`TimeBoxInputMode`/`TimeBoxDragOrientation`) |
|  [07]   | `KeyGestureInput`                                              | live keyboard-shortcut capture field                          |
|  [08]   | `TagInput`                                                     | token/tag chip entry                                          |
|  [09]   | `NumPad`                                                       | on-screen numeric keypad                                      |
|  [10]   | `EnumSelector` / `EnumItemTuple`                               | enum-member picker                                           |
|  [11]   | `ControlClassesInput`                                         | live Avalonia style-class editor (`StyleClass` authoring)     |
|  [12]   | `PathPicker`                                                  | file/folder browse field                                     |

[SELECTION_CONTROLS]: multi-select, combo, rating, button-group — `Ursa.Controls`
- rail: controls

| [INDEX] | [SYMBOL]                                                          | [KIND]                                                          |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `MultiComboBox` / `MultiComboBoxItem` / `MultiComboBoxSelectedItemList` | multi-select combo with chip readout                  |
|  [02]   | `TreeComboBox` / `TreeComboBoxItem`                            | hierarchical tree drop-down selector                          |
|  [03]   | `AutoCompleteBox` / `MultiAutoCompleteBox` / `MultiAutoCompleteSelectionAdapter` | type-ahead single + multi auto-complete             |
|  [04]   | `SelectionList` / `SelectionListItem`                         | single/range selection list (`SelectionChangingEventArgs`)    |
|  [05]   | `ButtonGroup`                                                 | segmented mutually-exclusive button group                     |
|  [06]   | `Rating` / `RatingCharacter`                                  | star/character rating input                                   |
|  [07]   | `RangeSlider` / `RangeTrack` / `RangeValueChangedEventArgs`   | dual-thumb range slider                                       |
|  [08]   | `ClosableTag` / `ClosableTag` set                            | dismissible tag chip                                          |

[DISPLAY_CONTROLS]: timeline, avatar, badge, marquee, descriptions, QR — `Ursa.Controls`
- rail: controls

| [INDEX] | [SYMBOL]                                                          | [KIND]                                                          |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `Timeline` / `TimelineItem` / `TimelinePanel`                  | vertical/horizontal event timeline (`TimelineDisplayMode`/`TimelineItemPosition`/`TimelineItemType`) |
|  [02]   | `Avatar`                                                       | user avatar (image/initials)                                   |
|  [03]   | `Badge` / `DualBadge`                                          | overlay count/dot badge + dual-segment badge                  |
|  [04]   | `Marquee`                                                      | scrolling-text ticker                                          |
|  [05]   | `Descriptions` / `DescriptionsItem`                           | key/value description grid                                     |
|  [06]   | `Divider`                                                      | titled section divider                                        |
|  [07]   | `ImageViewer`                                                  | pan/zoom image viewer                                          |
|  [08]   | `QRCode`                                                       | QR-code render control (`EccLevel`, internal `Gma.QrCodeNet`) |
|  [09]   | `Icon{Button,ToggleButton,SplitButton,ToggleSplitButton,DropDownButton,RepeatButton}` | icon-leading button family                |
|  [10]   | `TwoTonePathIcon` / `LabeledContentControl` / `GroupBoxBorder` / `UrsaGroupBox` | two-tone glyph + labeled/group containers   |

[PICKER_CONTROLS]: full date / time / range picker family — `Ursa.Controls`
- rail: controls

| [INDEX] | [SYMBOL]                                                          | [KIND]                                                          |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `DatePicker` / `DateRangePicker` / `DateOnlyPicker` / `DateOnlyRangePicker` | calendar date + range pickers (`DateOnly` variants)   |
|  [02]   | `DateTimePicker` / `DateTimeOffsetPicker` / `DateOffsetPicker` / `DateOffsetRangePicker` | date+time + offset pickers                  |
|  [03]   | `TimePicker` / `TimeRangePicker` / `TimeOnlyPicker` / `TimeOnlyRangePicker` / `TimePickerPresenter` | time + range pickers (`TimeOnly` variants) |
|  [04]   | `Clock` / `ClockTicks`                                         | analog clock + tick face                                       |
|  [05]   | `DatePickerCalendarView` / `DatePickerCalendarDayButton` / `DatePickerCalendarYearButton` / `DatePickerCalendarContext` | calendar surface internals (`DatePickerCalendarViewMode`) |
|  [06]   | `DateRange` / `DateRangeExtension` / `IDateSelector` / `WeekendDateSelector` | range value + selector contract                       |

[SHELL_CONTROLS]: window + view bases — `Ursa.Controls` / `Ursa.ReactiveUIExtension`
- rail: controls

| [INDEX] | [SYMBOL]                                  | [KIND]                                                          |
| :-----: | :---------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `UrsaWindow`                              | themed top-level window (custom title bar, `WindowResizer`)    |
|  [02]   | `UrsaView`                                | themed user-control base                                       |
|  [03]   | `SplashWindow`                            | startup splash window                                          |
|  [04]   | `ReactiveUrsaWindow` / `ReactiveUrsaView` | the ReactiveUI bindings of `UrsaWindow`/`UrsaView` (`Ursa.ReactiveUIExtension`) — the admitted MVVM view bases |
|  [05]   | `ThemeToggleButton` / `ThemeSelectorBase` / `ThemeVariantMapper` / `ThemeVariantMapping` | theme-variant toggle + mapping (`ThemeSelectorMode`) |
|  [06]   | `DisableContainer` / `DisabledAdorner`    | content-disable overlay                                        |

[CONTROL_ENUMS]: `Ursa.Controls` / `Ursa.Common` vocabulary
- rail: controls

| [INDEX] | [SYMBOL]                                                          | [RAIL]                                                         |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | `DialogButton` / `DialogResult` / `DialogMode` / `DialogLayerChangeType` | dialog button set / result / modality / layer change |
|  [02]   | `MessageBoxButton` / `MessageBoxResult` / `MessageBoxIcon` / `MessageCloseReason` | message-box buttons / result / icon / close cause |
|  [03]   | `Status`                                                        | severity (Banner/feedback) — Info/Success/Warning/Error       |
|  [04]   | `PinCodeMode` / `IPv4BoxInputMode` / `TimeBoxInputMode` / `TimeBoxDragOrientation` | masked-entry modes                          |
|  [05]   | `TimelineDisplayMode` / `TimelineItemPosition` / `TimelineItemType` | timeline layout vocabulary                              |
|  [06]   | `OverflowMode` / `LostFocusBehavior` / `PopConfirmTriggerMode` / `ResizeDirection` / `Direction` | overflow / commit / trigger / resize policy |
|  [07]   | `Position` / `ItemAlignment` / `CornerPosition` / `HorizontalPosition` / `VerticalPosition` | `Ursa.Common` placement vocabulary       |
|  [08]   | `DatePickerCalendarViewMode` / `UsePickerTypes` / `ThemeSelectorMode` / `AspectRatioMode` / `OffsetDefinitionKind` / `EccLevel` | picker / theme / layout / QR policy |

## [03]-[ENTRYPOINTS]

[DIALOG_DISPATCH]: `Dialog` (windowed) + `OverlayDialog` (in-canvas) static dispatch — vm-first, owner/host-id targeted, sync + async mirrors
- rail: controls

| [INDEX] | [SURFACE]                                                                                                   | [SURFACE_ROOT]  | [RAIL]                                              |
| :-----: | :---------------------------------------------------------------------------------------------------------- | :-------------- | :------------------------------------------------- |
|  [01]   | `ShowModal<TView,TViewModel>(vm, Window? owner, DialogOptions?) : Task<DialogResult>`                        | `Dialog`        | windowed modal dialog, result-typed                |
|  [02]   | `ShowCustomModal<TView,TViewModel,TResult>(vm, owner, DialogOptions?) : Task<TResult?>`                      | `Dialog`        | windowed modal with custom result payload          |
|  [03]   | `ShowStandard<TView,TViewModel>(vm, owner, DialogOptions?)` / `ShowStandardAsync(...) : Task<DialogResult>`  | `Dialog`        | non-modal standard dialog, fire / awaited          |
|  [04]   | `ShowCustom<TView,TViewModel>(vm, owner, DialogOptions?)` / `ShowCustomAsync<...,TResult>(...) : Task<TResult?>` | `Dialog`    | non-modal custom dialog, fire / awaited            |
|  [05]   | `ShowModal<TView,TViewModel>(vm, string? hostId, OverlayDialogOptions?) : Task<DialogResult>`                | `OverlayDialog` | in-canvas modal against a registered host id       |
|  [06]   | `ShowCustomModal<TResult>(vm, hostId, OverlayDialogOptions?, CancellationToken?) : Task<TResult?>`           | `OverlayDialog` | in-canvas modal, cancellable, custom result        |
|  [07]   | `ShowStandardAsync<TView,TViewModel>(vm, hostId, OverlayDialogOptions?, CancellationToken?) : Task<DialogResult>` | `OverlayDialog` | in-canvas standard dialog, cancellable          |
|  [08]   | `RegisterHost(OverlayDialogHost, id, hash)` / `GetHost(id, hash)` / `UnregisterHost(id, hash)`              | `OverlayDialogManager` | host-id registration the dispatch resolves   |

[DRAWER_DISPATCH]: `Drawer` slide-in panel dispatch — same vm-first shape, host-id targeted
- rail: controls

| [INDEX] | [SURFACE]                                                                            | [SURFACE_ROOT] | [RAIL]                                |
| :-----: | :----------------------------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `Show<TView,TViewModel>(vm, string? hostId, DrawerOptions?)`                          | `Drawer`       | non-modal slide-in drawer             |
|  [02]   | `ShowModal<TView,TViewModel>(vm, hostId, DrawerOptions?) : Task<DialogResult>`        | `Drawer`       | modal drawer, result-typed            |
|  [03]   | `ShowCustomModal<TView,TViewModel,TResult>(vm, hostId, DrawerOptions?) : Task<TResult?>` | `Drawer`    | modal drawer, custom result payload   |

[MESSAGEBOX_DISPATCH]: `MessageBox` windowed + overlay confirmation — message/title/icon/button-first
- rail: controls

| [INDEX] | [SURFACE]                                                                                                          | [SURFACE_ROOT] | [RAIL]                              |
| :-----: | :--------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `ShowAsync(string message, string? title, MessageBoxIcon, MessageBoxButton, string? styleClass) : Task<MessageBoxResult>` | `MessageBox` | windowed confirm                |
|  [02]   | `ShowAsync(Window owner, string message, string title, MessageBoxIcon, MessageBoxButton, string?) : Task<MessageBoxResult>` | `MessageBox` | owner-anchored windowed confirm |
|  [03]   | `ShowOverlayAsync(string message, string? title, string? hostId, MessageBoxIcon, MessageBoxButton, int? toplevelHashCode, string?) : Task<MessageBoxResult>` | `MessageBox` | in-canvas confirm against a host id |

[TOAST_NOTIFICATION_DISPATCH]: `IToastManager` / `INotificationManager` queue managers (installed onto a host visual, then `Show`)
- rail: controls

| [INDEX] | [SURFACE]                                       | [SURFACE_ROOT]                  | [RAIL]                                                  |
| :-----: | :---------------------------------------------- | :------------------------------ | :----------------------------------------------------- |
|  [01]   | `Show(IToast)` / `Close(IToast)` / `CloseAll()` | `IToastManager` (`WindowToastManager`) | enqueue / dismiss / drain transient toasts      |
|  [02]   | `Show(INotification)` / `Close(...)` / `CloseAll()` | `INotificationManager` (`WindowNotificationManager`) | enqueue / dismiss / drain corner notifications |
|  [03]   | `Type : NotificationType` / `Expiration` / `ShowIcon` / `ShowClose` / `OnClick` / `OnClose(Action<MessageCloseReason>)` | `IMessage` (base of `IToast`/`INotification`) | per-message severity (Avalonia's `Avalonia.Controls.Notifications.NotificationType`), lifetime, and click/close callbacks |

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
