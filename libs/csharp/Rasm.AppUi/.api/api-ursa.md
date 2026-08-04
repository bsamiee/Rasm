# [RASM_APPUI_API_URSA]

`Irihi.Ursa` is the extended-control suite (assembly `Ursa.dll`, xmlns `https://irihi.tech/ursa`) filling the control families the curated Avalonia roster lacks. Control visuals live in the sibling `Irihi.Ursa.Themes.Semi` (`UrsaSemiTheme : Styles`) under the `Semi.Avalonia` token system, and `Irihi.Ursa.ReactiveUIExtension` binds the Ursa view bases onto the admitted ReactiveUI rail. Every overlay — dialog, drawer, message box — is raised vm-first through a static dispatcher against a registered host id, the in-canvas counterpart to `DialogHost.Avalonia`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Irihi.Ursa`
- package: `Irihi.Ursa` (MIT)
- assembly: `Ursa`
- namespace: `Ursa.Controls`, `Ursa.Controls.Options`, `Ursa.Controls.Layout`, `Ursa.Controls.OverlayShared`, `Ursa.Common`, `Ursa.Converters`, `Ursa.EventArgs`, `Ursa.Helpers`
- depends: `Avalonia`, `Irihi.Avalonia.Shared`, `Irihi.Avalonia.Shared.Contracts` (the shared primitive closure `Semi.Avalonia` also floors on); `QRCode` vendors `Gma.QrCodeNet` internally, so no QR dependency surfaces
- rail: controls

[PACKAGE_SURFACE]: `Irihi.Ursa.Themes.Semi`
- package: `Irihi.Ursa.Themes.Semi` (MIT)
- assembly: `Ursa.Themes.Semi`
- namespace: `Ursa.Themes.Semi` (`UrsaSemiTheme : Styles`), `Ursa.Themes.Semi.Converters`, `Ursa.Themes.Semi.Locale`, `Ursa.Themes.Semi.SizeAnimations`
- depends: `Ursa`, `Semi.Avalonia`; publishes control themes under the `https://irihi.tech/semi` (`semi:`) xmlns `Semi.Avalonia` shares
- rail: theme

[PACKAGE_SURFACE]: `Irihi.Ursa.ReactiveUIExtension`
- package: `Irihi.Ursa.ReactiveUIExtension` (MIT)
- assembly: `Ursa.ReactiveUIExtension`
- namespace: `Ursa.ReactiveUIExtension`
- depends: `Ursa`, `ReactiveUI.Avalonia`
- rail: mvvm-bridge

## [02]-[PUBLIC_TYPES]

[NAVIGATION_CONTROLS]: navigation and wayfinding — `Ursa.Controls`

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `NavMenu` / `NavMenuItem`                       | class         | hierarchical side/top navigation menu + item       |
|  [02]   | `Breadcrumb` / `BreadcrumbItem`                 | class         | breadcrumb trail                                   |
|  [03]   | `Pagination` / `PaginationButton`               | class         | page-navigation strip                              |
|  [04]   | `Anchor` / `AnchorItem`                         | class         | scroll-spy anchor index over a scroll region       |
|  [05]   | `ScrollTo` / `ScrollToButton`                   | class         | attached `Direction`/`ButtonTheme` hint + button   |
|  [06]   | `ToolBar` / `ToolBarPanel` / `ToolBarSeparator` | class         | overflow-aware tool bar with a popup overflow well |
|  [07]   | `TitleBar`                                      | class         | left/center/right window title bar                 |

[FEEDBACK_CONTROLS]: toast, notification, banner, message, busy — `Ursa.Controls`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :-------------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `IMessage`                  | interface     | base message contract: severity, lifetime, callbacks |
|  [02]   | `IToast` / `INotification`  | interface     | content-only and title+content message contracts     |
|  [03]   | `Toast` / `Notification`    | class         | mutable `INotifyPropertyChanged` message payloads    |
|  [04]   | `IToastManager`             | interface     | toast queue contract                                 |
|  [05]   | `INotificationManager`      | interface     | notification queue contract                          |
|  [06]   | `WindowMessageManager`      | class         | abstract host-installed queue base (`MaxItems`)      |
|  [07]   | `WindowToastManager`        | class         | transient toast queue manager                        |
|  [08]   | `WindowNotificationManager` | class         | corner notification manager (`NotificationPosition`) |
|  [09]   | `MessageCard`               | class         | abstract severity-styled dismissible card            |
|  [10]   | `ToastCard`                 | class         | toast presentation card                              |
|  [11]   | `NotificationCard`          | class         | corner-positioned notification card                  |
|  [12]   | `MessageClosedEventArgs`    | class         | dismissal event carrying `MessageCloseReason`        |
|  [13]   | `Banner`                    | class         | inline `NotificationType` banner strip               |
|  [14]   | `MessageBoxControl`         | class         | message-box body control                             |
|  [15]   | `MessageBoxWindow`          | class         | windowed message box                                 |
|  [16]   | `Loading`                   | class         | busy spinner over content                            |
|  [17]   | `LoadingContainer`          | class         | content busy overlay with message                    |
|  [18]   | `LoadingIcon`               | class         | busy indicator glyph                                 |
|  [19]   | `Skeleton`                  | class         | shimmer loading placeholder                          |
|  [20]   | `PopConfirm`                | class         | `PopConfirmTriggerMode` inline confirmation          |

[OVERLAY_CONTROLS]: in-canvas dialog and drawer host layer — `Ursa.Controls`, `Ursa.Controls.Options`, `Ursa.Controls.Layout`

`OverlayDialogHost` is the registered-id target for `OverlayDialog`, `OverlayDrawer`, `Drawer`, and `MessageBox.ShowOverlayAsync`; a consumer sets its `HostId` to register the host.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :---------------------------------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `OverlayDialogHost`                             | class         | in-canvas overlay host (`Canvas`)              |
|  [02]   | `Dialog`                                        | class         | static windowed dialog dispatcher              |
|  [03]   | `OverlayDialog`                                 | class         | static in-canvas dialog dispatcher             |
|  [04]   | `Drawer`                                        | class         | static modal/non-modal drawer dispatcher       |
|  [05]   | `OverlayDrawer`                                 | class         | static standard/custom drawer dispatcher       |
|  [06]   | `MessageBox`                                    | class         | static windowed + overlay confirmation         |
|  [07]   | `OverlayMessageBox`                             | class         | static host-targeted confirmation              |
|  [08]   | `OverlayFeedbackElement`                        | class         | abstract closable overlay content base         |
|  [09]   | `DialogControlBase` / `DrawerControlBase`       | class         | abstract dialog and drawer control bases       |
|  [10]   | `CustomDialogControl` / `StandardDialogControl` | class         | untitled and titled+buttoned dialog shells     |
|  [11]   | `CustomDrawerControl` / `StandardDrawerControl` | class         | untitled and titled+buttoned drawer shells     |
|  [12]   | `CustomDialogWindow` / `StandardDialogWindow`   | class         | windowed dialog shells                         |
|  [13]   | `DialogOptions` / `OverlayDialogOptions`        | class         | windowed and in-canvas dialog option records   |
|  [14]   | `DrawerOptions`                                 | class         | drawer option record (`Ursa.Controls.Options`) |
|  [15]   | `DefaultDialogLayout`                           | class         | dialog layout host (`Ursa.Controls.Layout`)    |
|  [16]   | `DialogResizer` / `DialogResizerThumb`          | class         | edge-resize surface and thumb                  |
|  [17]   | `DialogLayerChangeEventArgs`                    | class         | z-order change event                           |

[DATA_ENTRY_CONTROLS]: form, numeric, masked, gesture, tag entry — `Ursa.Controls`

`Numeric<T>UpDown` admits `Byte`, `SByte`, `Short`, `UShort`, `Int`, `UInt`, `Long`, `ULong`, `Float`, `Double`, and `Decimal`; `<T>Displayer` admits `Double`, `Int32`, `Int64`, and `DateDisplay` carries `DateTime`.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `Form`                                       | class         | declarative label-aligned form layout        |
|  [02]   | `FormGroup`                                  | class         | headered form field group                    |
|  [03]   | `FormItem`                                   | class         | labeled, required-marked field slot          |
|  [04]   | `NumericUpDown` / `NumericUpDownBase<T>`     | class         | abstract numeric spinner bases               |
|  [05]   | `Numeric<T>UpDown`                           | class         | CLR-typed numeric spinner                    |
|  [06]   | `NumberDisplayerBase` / `NumberDisplayer<T>` | class         | abstract rolling-number bases                |
|  [07]   | `<T>Displayer` / `DateDisplay`               | class         | CLR-typed rolling display                    |
|  [08]   | `PinCode` / `PinCodeItem`                    | class         | segmented `PinCodeMode` entry + segment      |
|  [09]   | `PinCodeCollection`                          | class         | PIN segment collection                       |
|  [10]   | `PinCodeCompleteEventArgs`                   | class         | PIN completion event                         |
|  [11]   | `IPv4Box` / `IPv4BoxInputMethodClient`       | class         | masked `IPv4BoxInputMode` entry + IME client |
|  [12]   | `TimeBox` / `TimeChangedEventArgs`           | class         | masked time-component entry + change event   |
|  [13]   | `KeyGestureInput`                            | class         | keyboard-shortcut capture                    |
|  [14]   | `TagInput`                                   | class         | token entry                                  |
|  [15]   | `NumPad` / `NumPadButton`                    | class         | on-screen numeric keypad + key               |
|  [16]   | `EnumSelector` / `EnumItemTuple`             | class         | enum-member picker + member tuple            |
|  [17]   | `ControlClassesInput`                        | class         | Avalonia `StyleClass` editor                 |
|  [18]   | `PathPicker`                                 | class         | file-or-folder field (`UsePickerTypes`)      |
|  [19]   | `ValueChangedEventArgs<T>`                   | class         | struct-value change event                    |

[SELECTION_CONTROLS]: multi-select, combo, rating, segmented — `Ursa.Controls`

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------------------------ | :------------ | :--------------------------------------- |
|  [01]   | `MultiComboBox`                       | class         | multi-select combo with chip readout     |
|  [02]   | `MultiComboBoxItem`                   | class         | combo item                               |
|  [03]   | `MultiComboBoxSelectedItemList`       | class         | selected-chip readout                    |
|  [04]   | `TreeComboBox` / `TreeComboBoxItem`   | class         | hierarchical drop-down + item            |
|  [05]   | `AutoCompleteBox`                     | class         | Avalonia type-ahead plus `IClearControl` |
|  [06]   | `MultiAutoCompleteBox`                | class         | multi type-ahead selector                |
|  [07]   | `MultiAutoCompleteSelectionAdapter`   | class         | multi-selection adapter                  |
|  [08]   | `SelectionList` / `SelectionListItem` | class         | single-select segmented list with slide  |
|  [09]   | `SelectionChangingEventArgs`          | class         | cancellable selection-change event       |
|  [10]   | `ButtonGroup`                         | class         | binding-driven segmented button strip    |
|  [11]   | `Rating` / `RatingCharacter`          | class         | star-or-character input + glyph          |
|  [12]   | `RangeSlider` / `RangeTrack`          | class         | dual-thumb range input + track           |
|  [13]   | `RangeValueChangedEventArgs`          | class         | range-change event                       |
|  [14]   | `ClosableTag`                         | class         | dismissible tag chip                     |

[DISPLAY_CONTROLS]: timeline, avatar, badge, marquee, descriptions, QR — `Ursa.Controls`

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------- | :------------ | :----------------------------------------- |
|  [01]   | `Timeline` / `TimelineItem`                  | class         | binding-driven event timeline + event      |
|  [02]   | `TimelineFormatConverter`                    | class         | timeline time-format converter             |
|  [03]   | `Avatar`                                     | class         | image-or-initial avatar                    |
|  [04]   | `Badge`                                      | class         | count-or-dot badge over content            |
|  [05]   | `DualBadge`                                  | class         | icon + header + content badge              |
|  [06]   | `Marquee`                                    | class         | scrolling-content ticker                   |
|  [07]   | `Descriptions` / `DescriptionsItem`          | class         | key-value grid + entry                     |
|  [08]   | `Divider`                                    | class         | titled divider                             |
|  [09]   | `ImageViewer` / `ImageViewerPresenter`       | class         | pan-zoom image viewer + presenter          |
|  [10]   | `PanZoomGestureHandler`                      | class         | pan/zoom gesture recognizer                |
|  [11]   | `PinchUpdateEventArgs`                       | class         | pinch-gesture event                        |
|  [12]   | `QRCode`                                     | class         | QR renderer (`EccLevel`)                   |
|  [13]   | `IconButton`                                 | class         | icon-leading button, icon quartet owner    |
|  [14]   | `IconToggleButton` / `IconToggleSplitButton` | class         | icon toggle and toggle-split buttons       |
|  [15]   | `IconSplitButton` / `IconDropDownButton`     | class         | icon split and drop-down buttons           |
|  [16]   | `IconRepeatButton`                           | class         | icon repeat button                         |
|  [17]   | `TwoTonePathIcon`                            | class         | fill + stroke two-tone glyph               |
|  [18]   | `LabeledContentControl`                      | class         | abstract labeled container                 |
|  [19]   | `GroupBoxBorder` / `UrsaGroupBox`            | class         | notched border decorator + group container |

[PICKER_CONTROLS]: date / time / range picker family — `Ursa.Controls`

| [INDEX] | [SYMBOL]                                          | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :------------------------------------------------ | :------------ | :-------------------------------- |
|  [01]   | `DatePickerBase` / `DatePickerBase<T>`            | class         | abstract date picker bases        |
|  [02]   | `DateRangePickerBase` / `DateRangePickerBase<T>`  | class         | abstract date range bases         |
|  [03]   | `DateTimePickerBase` / `DateTimePickerBase<T>`    | class         | abstract date-time bases          |
|  [04]   | `TimePickerBase` / `TimePickerBase<T>`            | class         | abstract time picker bases        |
|  [05]   | `TimeRangePickerBase` / `TimeRangePickerBase<T>`  | class         | abstract time range bases         |
|  [06]   | `DatePicker` / `DateRangePicker`                  | class         | calendar date and range pickers   |
|  [07]   | `DateOnlyPicker` / `DateOnlyRangePicker`          | class         | `DateOnly` pickers                |
|  [08]   | `DateOffsetPicker` / `DateOffsetRangePicker`      | class         | `DateTimeOffset` date pickers     |
|  [09]   | `DateTimePicker` / `DateTimeOffsetPicker`         | class         | date-time pickers                 |
|  [10]   | `TimePicker` / `TimeRangePicker`                  | class         | time and time-range pickers       |
|  [11]   | `TimeOnlyPicker` / `TimeOnlyRangePicker`          | class         | `TimeOnly` pickers                |
|  [12]   | `TimePickerPresenter` / `UrsaDateTimeScrollPanel` | class         | scroll-wheel time surface + panel |
|  [13]   | `Clock` / `ClockTicks`                            | class         | analog clock + tick face          |
|  [14]   | `DatePickerCalendarView`                          | class         | month/year calendar surface       |
|  [15]   | `DatePickerCalendarDayButton` / `...YearButton`   | class         | calendar day and year buttons     |
|  [16]   | `DatePickerCalendarDayButtonEventArgs`            | class         | calendar day event                |
|  [17]   | `DatePickerCalendarYearButtonEventArgs`           | class         | calendar year event               |
|  [18]   | `DateRange`                                       | record        | sealed start/end date range value |
|  [19]   | `IDateSelector` / `WeekendDateSelector`           | interface     | blackout-date predicate + weekend |

[LAYOUT_PANELS]: measure/arrange panels and offset geometry — `Ursa.Controls`

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :-------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `AspectRatioLayout` / `AspectRatioLayoutItem` | class         | `AspectRatioMode` transitioning container |
|  [02]   | `ColumnWrapPanel`                             | class         | column-major navigable wrap panel         |
|  [03]   | `ElasticWrapPanel`                            | class         | stretch-to-fill wrap panel                |
|  [04]   | `OverflowStackPanel`                          | class         | overflow-aware stack panel                |
|  [05]   | `WrapPanelWithTrailingItem`                   | class         | wrap panel reserving a trailing slot      |
|  [06]   | `VirtualizingUniformGrid`                     | class         | virtualizing uniform grid with snap       |
|  [07]   | `TimelinePanel`                               | class         | timeline arrange panel                    |
|  [08]   | `OffsetDefinition` / `OffsetDefinitions`      | class         | `OffsetDefinitionKind` offset spec + list |
|  [09]   | `OffsetValue`                                 | struct        | readonly offset scalar                    |
|  [10]   | `OffsetDefinitionConverter` / `...sConverter` | class         | XAML `TypeConverter` for offset specs     |
|  [11]   | `OffsetValueConverter`                        | class         | XAML `TypeConverter` for offset scalars   |

[SHELL_CONTROLS]: window, view, theme, disable bases — `Ursa.Controls` / `Ursa.ReactiveUIExtension`

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :------------------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `UrsaWindow`                                 | class         | themed chromeless top-level with dialog host  |
|  [02]   | `UrsaView`                                   | class         | themed user-control base                      |
|  [03]   | `SplashWindow`                               | class         | abstract startup splash window                |
|  [04]   | `WindowResizer` / `WindowResizerThumb`       | class         | managed window resize grip + thumb            |
|  [05]   | `WindowThumb`                                | class         | window drag/caption thumb                     |
|  [06]   | `ReactiveUrsaWindow<TViewModel>`             | class         | `IViewFor<T>` window base                     |
|  [07]   | `ReactiveUrsaView<TViewModel>`               | class         | `IViewFor<T>` view base                       |
|  [08]   | `ThemeSelectorBase`                          | class         | abstract theme selector (`ThemeSelectorMode`) |
|  [09]   | `ThemeToggleButton`                          | class         | two- or three-state theme-variant toggle      |
|  [10]   | `ThemeVariantMapper` / `ThemeVariantMapping` | class         | variant remap scope + source/target pair      |
|  [11]   | `DisableContainer` / `DisabledAdorner`       | class         | disable overlay + attached `DisabledTip`      |

[CONTROL_ENUMS]: `Ursa.Controls` / `Ursa.Common` policy vocabulary

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `DialogButton`           | enum          | dialog button set                                             |
|  [02]   | `DialogResult`           | enum          | dialog result                                                 |
|  [03]   | `DialogMode`             | enum          | dialog modality                                               |
|  [04]   | `DialogLayerChangeType`  | enum          | dialog z-order change                                         |
|  [05]   | `MessageBoxButton`       | enum          | message-box button set                                        |
|  [06]   | `MessageBoxResult`       | enum          | message-box result                                            |
|  [07]   | `MessageBoxIcon`         | enum          | message-box icon                                              |
|  [08]   | `MessageCloseReason`     | enum          | `Timeout` / `UserAction` / `Displaced` dismissal cause        |
|  [09]   | `PinCodeMode`            | enum          | PIN entry mode                                                |
|  [10]   | `IPv4BoxInputMode`       | enum          | IPv4 entry mode                                               |
|  [11]   | `TimeBoxInputMode`       | enum          | time entry mode                                               |
|  [12]   | `TimeBoxDragOrientation` | enum          | time drag direction                                           |
|  [13]   | `TimelineDisplayMode`    | enum          | timeline layout                                               |
|  [14]   | `TimelineItemPosition`   | enum          | timeline position                                             |
|  [15]   | `TimelineItemType`       | enum          | timeline item kind                                            |
|  [16]   | `OverflowMode`           | enum          | `AsNeeded` / `Always` / `Never` tool-bar overflow policy      |
|  [17]   | `LostFocusBehavior`      | enum          | commit policy                                                 |
|  [18]   | `PopConfirmTriggerMode`  | enum          | confirmation trigger                                          |
|  [19]   | `ResizeDirection`        | enum          | resize policy                                                 |
|  [20]   | `Direction`              | enum          | direction policy                                              |
|  [21]   | `Position`               | enum          | `Left` / `Top` / `Right` / `Bottom` placement (`Ursa.Common`) |
|  [22]   | `ItemAlignment`          | enum          | common alignment (`Ursa.Common`)                              |
|  [23]   | `CornerPosition`         | enum          | common corner placement (`Ursa.Common`)                       |
|  [24]   | `HorizontalPosition`     | enum          | horizontal placement                                          |
|  [25]   | `VerticalPosition`       | enum          | vertical placement                                            |
|  [26]   | `UsePickerTypes`         | enum          | `PathPicker` file/folder/save policy                          |
|  [27]   | `ThemeSelectorMode`      | enum          | `Controller` / `Indicator` theme-selector role                |
|  [28]   | `AspectRatioMode`        | enum          | aspect-ratio policy                                           |
|  [29]   | `OffsetDefinitionKind`   | enum          | offset definition                                             |
|  [30]   | `EccLevel`               | enum          | QR correction level                                           |

[SUPPORT_TYPES]: converters, helpers, and shared event args — `Ursa.Converters`, `Ursa.Helpers`, `Ursa.EventArgs`, `Ursa.Common`

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :---------------------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `BadgeContentOverflowConverter`                 | class         | badge overflow-count markup converter   |
|  [02]   | `SelectionBoxTemplateConverter`                 | class         | selection-box template markup converter |
|  [03]   | `FocusHelper`                                   | class         | attached `DialogFocusHint` focus marker |
|  [04]   | `SizeAnimationHelper`                           | class         | attached width/height animation driver  |
|  [05]   | `SizeAnimationHelperAnimationGeneratorDelegate` | delegate      | per-target animation factory            |
|  [06]   | `ResultEventArgs`                               | class         | overlay close-result event              |
|  [07]   | `LogicalHelpers` / `VisualHelpers`              | class         | logical- and visual-tree walk helpers   |

[THEME_TYPES]: control-theme bridge and template converters — `Ursa.Themes.Semi`

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :-------------------------------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `UrsaSemiTheme`                         | class         | `Styles` control-theme dictionary (`<semi:UrsaSemiTheme/>`) |
|  [02]   | `DefaultSizeAnimations`                 | class         | shared size-animation resource dictionary                   |
|  [03]   | `NavMenuSizeAnimations`                 | class         | nav-menu collapse/expand animation dictionary               |
|  [04]   | `BooleansToOpacityConverter`            | class         | booleans-to-opacity converter                               |
|  [05]   | `BrushToColorConverter`                 | class         | brush-to-color converter                                    |
|  [06]   | `ClockHandLengthConverter`              | class         | clock-hand-length converter (`double ratio`)                |
|  [07]   | `FormContentHeightToAlignmentConverter` | class         | form content-height to alignment                            |
|  [08]   | `FormContentHeightToMarginConverter`    | class         | form content-height to margin                               |
|  [09]   | `NavMenuMarginConverter`                | class         | nav-menu margin converter                                   |
|  [10]   | `TreeLevelToPaddingConverter`           | class         | tree-level to padding converter                             |

## [03]-[ENTRYPOINTS]

[DIALOG_DISPATCH]: `Dialog` (windowed, owner `Window?`) and `OverlayDialog` (in-canvas, `string? hostId`, threading a `CancellationToken?`) dispatch vm-first through `<TView,TViewModel>` generic overloads (custom overloads add `,TResult>`); each also carries `(Control view, object? vm, …)` and bare `(object? vm, …)` shapes

| [INDEX] | [SURFACE]                                                                                         | [SHAPE] | [CAPABILITY]           |
| :-----: | :------------------------------------------------------------------------------------------------ | :------ | :--------------------- |
|  [01]   | `Dialog.ShowStandard(TVm, Window?, DialogOptions?)`                                               | static  | windowed fire          |
|  [02]   | `Dialog.ShowStandardAsync(TVm, Window?, DialogOptions?) -> Task<DialogResult>`                    | static  | windowed awaited       |
|  [03]   | `Dialog.ShowModal(TVm, Window?, DialogOptions?) -> Task<DialogResult>`                            | static  | windowed modal         |
|  [04]   | `Dialog.ShowCustom(TVm, Window?, DialogOptions?)`                                                 | static  | custom fire            |
|  [05]   | `Dialog.ShowCustomAsync<TResult>(TVm, Window?, DialogOptions?) -> Task<TResult?>`                 | static  | custom awaited         |
|  [06]   | `Dialog.ShowCustomModal<TResult>(TVm, Window?, DialogOptions?) -> Task<TResult?>`                 | static  | custom modal           |
|  [07]   | `OverlayDialog.Show(TVm, string?, OverlayDialogOptions?)`                                         | static  | overlay non-modal      |
|  [08]   | `OverlayDialog.ShowStandard(TVm, string?, OverlayDialogOptions?)`                                 | static  | overlay fire           |
|  [09]   | `OverlayDialog.ShowStandardAsync(TVm, string?, OverlayDialogOptions?, CancellationToken?)`        | static  | overlay awaited        |
|  [10]   | `OverlayDialog.ShowModal(TVm, string?, OverlayDialogOptions?, CancellationToken?)`                | static  | overlay modal          |
|  [11]   | `OverlayDialog.ShowCustom(TVm, string?, OverlayDialogOptions?)`                                   | static  | overlay custom fire    |
|  [12]   | `OverlayDialog.ShowCustomAsync<TResult>(TVm, string?, OverlayDialogOptions?, CancellationToken?)` | static  | overlay custom awaited |
|  [13]   | `OverlayDialog.ShowCustomModal<TResult>(TVm, string?, OverlayDialogOptions?, CancellationToken?)` | static  | overlay custom modal   |

- `OverlayDialog.ShowStandardAsync` and `ShowModal` return `Task<DialogResult>`; the `ShowCustom*` pair returns `Task<TResult?>`, and the `Show`/`ShowStandard`/`ShowCustom` fire shapes return void.

[DRAWER_DISPATCH]: `Drawer` and `OverlayDrawer` are both static in-canvas dispatchers against `string? hostId` — `Drawer` carries the modal pair, `OverlayDrawer` the standard/custom pair; `DrawerControlBase` is the drawer control base, never a dispatcher

| [INDEX] | [SURFACE]                                                                                | [SHAPE] | [CAPABILITY]     |
| :-----: | :--------------------------------------------------------------------------------------- | :------ | :--------------- |
|  [01]   | `Drawer.Show(TVm, string?, DrawerOptions?)`                                              | static  | non-modal        |
|  [02]   | `Drawer.ShowModal(TVm, string?, DrawerOptions?) -> Task<DialogResult>`                   | static  | modal            |
|  [03]   | `Drawer.ShowCustom(TVm, string?, DrawerOptions?)`                                        | static  | custom fire      |
|  [04]   | `Drawer.ShowCustomModal<TResult>(TVm, string?, DrawerOptions?) -> Task<TResult?>`        | static  | custom modal     |
|  [05]   | `OverlayDrawer.ShowStandard(TVm, string?, DrawerOptions?)`                               | static  | standard fire    |
|  [06]   | `OverlayDrawer.ShowStandardAsync(TVm, string?, DrawerOptions?) -> Task<DialogResult>`    | static  | standard awaited |
|  [07]   | `OverlayDrawer.ShowCustom(TVm, string?, DrawerOptions?)`                                 | static  | custom fire      |
|  [08]   | `OverlayDrawer.ShowCustomAsync<TResult>(TVm, string?, DrawerOptions?) -> Task<TResult?>` | static  | custom awaited   |

[MESSAGEBOX_DISPATCH]: `MessageBox` windowed and overlay confirmation — message/title/icon/button-first, each returning `Task<MessageBoxResult>`

| [INDEX] | [SURFACE]                                                                                                | [SHAPE] | [CAPABILITY]    |
| :-----: | :------------------------------------------------------------------------------------------------------- | :------ | :-------------- |
|  [01]   | `MessageBox.ShowAsync(string, string?, MessageBoxIcon, MessageBoxButton, string?)`                       | static  | windowed        |
|  [02]   | `MessageBox.ShowAsync(Window, string, string, MessageBoxIcon, MessageBoxButton, string?)`                | static  | owner-anchored  |
|  [03]   | `MessageBox.ShowOverlayAsync(string, string?, string?, MessageBoxIcon, MessageBoxButton, int?, string?)` | static  | registered host |
|  [04]   | `OverlayMessageBox.ShowAsync(string, string?, string?, MessageBoxIcon, MessageBoxButton, int?, string?)` | static  | registered host |

- Each overload takes its trailing `string?` as a style class stamped on the dialog shell, and the `int?` as a top-level hash code selecting one of several open windows.

[TOAST_NOTIFICATION_DISPATCH]: `IToastManager` / `INotificationManager` queue managers — constructed over a `TopLevel?` or `VisualLayerManager?`, resolved back off any visual, then `Show`

`WindowToastManager` and `WindowNotificationManager` both derive `WindowMessageManager`, which owns `MaxItems`, `Uninstall()`, and the abstract `Show(object)`; `IMessage` is the base of `IToast` and `INotification`.

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]              |
| :-----: | :-------------------------------------------------------------------- | :------- | :------------------------ |
|  [01]   | `WindowToastManager(TopLevel?)`                                       | ctor     | install onto a top level  |
|  [02]   | `WindowToastManager.TryGetToastManager(Visual?, out …)`               | static   | resolve an installed host |
|  [03]   | `WindowNotificationManager.TryGetNotificationManager(Visual?, out …)` | static   | resolve an installed host |
|  [04]   | `WindowMessageManager.MaxItems : int`                                 | property | queue depth cap           |
|  [05]   | `WindowMessageManager.Uninstall()`                                    | instance | detach from the host      |
|  [06]   | `IToastManager.Show(IToast)`                                          | instance | enqueue toast             |
|  [07]   | `IToastManager.Close(IToast)`                                         | instance | dismiss toast             |
|  [08]   | `IToastManager.CloseAll()`                                            | instance | drain toast queue         |
|  [09]   | `INotificationManager.Show(INotification)`                            | instance | enqueue notification      |
|  [10]   | `INotificationManager.Close(INotification)`                           | instance | dismiss notification      |
|  [11]   | `INotificationManager.CloseAll()`                                     | instance | drain notification queue  |
|  [12]   | `IMessage.Type : NotificationType`                                    | property | message severity          |
|  [13]   | `IMessage.Expiration : TimeSpan`                                      | property | message lifetime          |
|  [14]   | `IMessage.ShowIcon` / `IMessage.ShowClose`                            | property | icon and close visibility |
|  [15]   | `IMessage.OnClick : Action?`                                          | property | click callback            |
|  [16]   | `IMessage.OnClose : Action<MessageCloseReason>?`                      | property | close callback            |

- `IMessage` declares every member get-only and `Toast`/`Notification` implement them as settable auto-properties; `OnClick` and `OnClose` are delegate properties the message carries, never manager methods.
- Both managers add a wide `Show(object content, NotificationType, TimeSpan?, bool showIcon, bool showClose, Action?, Action<MessageCloseReason>?, string[]? classes)` overload that builds the card in place.

[BUTTON_PROPERTIES]: `IconButton` registers the icon quartet once, and its static `IconButton.Get*`/`Set*` pair reaches any `ContentControl`

- `IconButton`: `Icon:object?` `IconTemplate:IDataTemplate?` `IconPlacement:Position` (`Left`) `IsLoading:bool`; part `PART_RootPanel`; classes `:left` `:right` `:top` `:bottom` `:empty` `:empty-content`
- `IconToggleButton` `IconSplitButton` `IconToggleSplitButton` `IconDropDownButton` `IconRepeatButton`: the same quartet by `AddOwner`, over `ToggleButton` `SplitButton` `ToggleSplitButton` `DropDownButton` `RepeatButton`
- `ButtonGroup`: `CommandBinding:BindingBase?` `CommandParameterBinding:BindingBase?` `ContentBinding:BindingBase?` — one binding triple projects every item, and item visuals resolve `ButtonGroupItemTheme`
- `SelectionList`: `Indicator:Control?`; part `PART_Indicator`; `SelectionMode` is coerced to `Single`, so the indicator slides between exactly one selected segment
- `SelectionListItem`: `IsSelected:bool` (`ISelectable`)

[NAVIGATION_PROPERTIES]: `NavMenu` projects an arbitrary item graph through binding properties and collapses horizontally between `CollapseWidth` and `ExpandWidth`

- `NavMenu`: `Header:object?` `HeaderTemplate:IDataTemplate?` `Footer:object?` `SelectedItem:object?` `IconBinding` `HeaderBinding` `SubMenuBinding` `CommandBinding` (`BindingBase?`) `IconTemplate:IDataTemplate?` `SubMenuIndent:double` `IsHorizontalCollapsed:bool` `CollapseWidth:double` `ExpandWidth:double` `CanToggle:bool`
- `NavMenu` events and parts: `SelectionChangedEvent` `SelectionChangingEvent`; part `PART_ItemsPresenter`; class `:horizontal-collapsed`
- `NavMenuItem`: `Icon:object?` `IconTemplate:IDataTemplate?` `Command:ICommand?` `CommandParameter:object?` `IsSelected:bool` `IsHighlighted:bool` (direct) `Level:int` (direct, depth from the owning `NavMenu`) `IsHorizontalCollapsed:bool` `IsVerticalCollapsed:bool` `SubMenuIndent:double` `IsSeparator:bool`
- `NavMenuItem` events and classes: `IsSelectedChangedEvent`; classes `:highlighted` `:first-level` `:horizontal-collapsed` `:vertical-collapsed` `:selector`
- `ToolBar`: `Orientation:Orientation` `PopupPlacement:PlacementMode`, and the attached `OverflowMode` (`ToolBar.SetOverflowMode(Control, OverflowMode)`, default `AsNeeded`) per child; part `PART_OverflowPanel`; class `:overflow`
- `TitleBar`: `LeftContent:object?` `CenterContent:object?` `RightContent:object?`
- `UrsaWindow`: `IsTitleBarVisible` `IsMinimizeButtonVisible` `IsRestoreButtonVisible` `IsFullScreenButtonVisible` `IsCloseButtonVisible` `IsManagedResizerVisible` (`bool`) `TitleBarContent:object?` `LeftContent:object?` `RightContent:object?` `TitleBarMargin:Thickness`; part `PART_DialogHost`

[FEEDBACK_PROPERTIES]: severity rides Avalonia's `NotificationType` on every Ursa feedback surface, and the theme selects the matching pseudo-class

- `Banner`: `Type:NotificationType` `Icon:object?` `ShowIcon:bool` (`true`) `CanClose:bool`; part `PART_CloseButton`; class `:icon`
- `MessageCard`: `NotificationType:NotificationType` `ShowIcon:bool` `ShowClose:bool` `IsClosed:bool` `IsClosing:bool` (direct); event `MessageClosed` (`MessageClosedEventArgs`, bubbling); classes `:information` `:success` `:warning` `:error`
- `MessageCard.CloseOnClick`: attached on `Button` (`MessageCard.SetCloseOnClick(Button, bool)`) — any templated button closes the owning card
- `ToastCard`: inherits `MessageCard` whole; `NotificationCard` adds `Position:NotificationPosition` (direct) with classes `:topleft` `:topright` `:bottomleft` `:bottomright` `:topcenter` `:bottomcenter`
- `WindowNotificationManager`: `Position:NotificationPosition` and the same six corner classes
- `Loading`: `Indicator:object?` `IsLoading:bool`; `LoadingIcon`: `IsLoading:bool`
- `LoadingContainer`: `Indicator:object?` `LoadingMessage:object?` `LoadingMessageTemplate:IDataTemplate` `MessageForeground:IBrush?` `IsLoading:bool`; class `:loading`
- `Skeleton`: `IsActive:bool` (shimmer animation) `IsLoading:bool` (placeholder vs content)
- `Badge`: `BadgeTheme:ControlTheme` `Dot:bool` `CornerPosition:CornerPosition` `OverflowCount:int` `BadgeFontSize:double`; parts `PART_BadgeContainer` `PART_HeaderPresenter` `PART_ContentPresenter`
- `DualBadge`: `Icon:object?` `IconTemplate:IDataTemplate?` `IconForeground` `HeaderForeground` `HeaderBackground` (`IBrush?`); part `PART_Icon`; classes `:icon-empty` `:header-empty` `:content-empty`

[FORM_PROPERTIES]: `Form` owns label geometry across its whole item tree, `FormItem` overrides it per field, and `FormGroup` collects items under a header alone

- `Form`: `LabelPosition:Position` `LabelWidth:GridLength` `LabelAlignment:HorizontalAlignment`; class `:fixed-width` (a non-`Auto`, non-star `LabelWidth`)
- `FormItem`: `Label:object?` `IsRequired:bool` `NoLabel:bool` `LabelWidth:double` `LabelAlignment:HorizontalAlignment`; part `PART_Label`; classes `:horizontal` (`LabelPosition.Left`) `:no-label`

[OVERLAY_PROPERTIES]: `OverlayDialogHost` paints the modal scrim and reports modal state up an attached scope

- `OverlayDialogHost`: `OverlayMaskBrush:IBrush?` `SafePadding:Thickness` `IsModalStatusReporter:bool` with attached `IsModalStatusScope:bool` and read-back `OverlayDialogHost.GetIsInModalStatus(Control)`
- `DrawerControlBase`: `Position:Position` (`Right`) `CanResize:bool` `IsOpen:bool`; part `PART_CloseButton`
- `OverlayFeedbackElement`: `IsClosed:bool` — the shared closable base under `DialogControlBase` and `DrawerControlBase`

[DISPLAY_PROPERTIES]: binding-driven display controls project an item graph through `BindingBase?` members rather than a typed item class

- `Timeline`: `IconMemberBinding` `HeaderMemberBinding` `ContentMemberBinding` `TimeMemberBinding` (`BindingBase?`) `IconTemplate` `DescriptionTemplate` (`IDataTemplate?`) `TimeFormat:string?` `Mode:TimelineDisplayMode`
- `TimelineItem`: `Icon:object?` `IconTemplate:IDataTemplate?` `Type:TimelineItemType` `Position:TimelineItemPosition` `Time:DateTime` `TimeFormat:string?` `LeftWidth` `IconWidth` `RightWidth` (`double`, direct); parts `PART_RootGrid` `PART_Header` `PART_Icon` `PART_Content` `PART_Time`; classes `:first` `:last` `:empty-icon` `:all-left` `:all-right` `:separate`
- `Divider`: `Orientation:Orientation` — content is the inline title, themed by `DividerLeftLine` and `DividerRightLine`
- `TwoTonePathIcon`: `Data:Geometry` `StrokeBrush:IBrush?` `StrokeThickness:double` `IsActive:bool` `ActiveForeground:IBrush?` `ActiveStrokeBrush:IBrush?`; class `:active`
- `RangeSlider`: `Minimum` `Maximum` `LowerValue` `UpperValue` `TrackWidth` `TickFrequency` (`double`) `Ticks:AvaloniaList<double>?` `TickPlacement:TickPlacement` `IsSnapToTick:bool` `IsDirectionReversed:bool` `Orientation:Orientation`; part `PART_Track`; event `ValueChangedEvent`
- `ThemeSelectorBase`: `Mode:ThemeSelectorMode` `SelectedTheme:ThemeVariant?` `TargetScope:ThemeVariantScope?`; `ThemeToggleButton` adds `IsThreeState:bool`, part `PART_ThemeButton`, classes `:light` `:dark` `:default`

[ENTRY_PROPERTIES]: `PathPicker`, `MultiComboBox`, and `EnumSelector` carry their whole policy as styled properties, so a consumer configures each in XAML alone

- `PathPicker`: `UsePickerType:UsePickerTypes` `Title:string` `SuggestedStartPath:string` `SuggestedFileName:string` `DefaultFileExtension:string` `FileFilter:string` `AllowMultiple:bool` `ButtonContent:object?` `Command:ICommand?` `SelectedPaths:IReadOnlyList<string>` (direct) `SelectedPathsText:string?` `IsOmitCommandOnCancel:bool` `IsClearSelectionOnCancel:bool`; part `PART_Button`
- `MultiComboBox`: `SelectedItems:IList?` `SelectedItemTemplate:IDataTemplate?` `IsDropDownOpen:bool` `MaxDropDownHeight:double` `MaxSelectionBoxHeight:double` `PlaceholderText:string?` `PlaceholderForeground:IBrush?` `Watermark:string?` `InnerLeftContent` `InnerRightContent` `PopupInnerTopContent` `PopupInnerBottomContent` (`object?`); part `PART_BackgroundBorder`; classes `:dropdownopen` `:selection-empty`
- `EnumSelector`: `EnumType:Type?` `Value:object?` `DisplayDescription:bool` `EnumValues:IList?` `SelectedValue:EnumItemTuple?` (direct) `Values:IList<EnumItemTuple>?` (direct)

[THEME_INSTALL]: `UrsaSemiTheme` install and locale surface — `Ursa.Themes.Semi` (the only code surface; the rest is XAML resource lookup)

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------------- | :------- | :--------------------------------------------------- |
|  [01]   | `<semi:UrsaSemiTheme/>`                                              | ctor     | install Ursa control themes last in the chain        |
|  [02]   | `UrsaSemiTheme(IServiceProvider?)`                                   | ctor     | code-side install                                    |
|  [03]   | `UrsaSemiTheme.Locale`                                               | property | select the culture (`CultureInfo?`, `zh-CN` default) |
|  [04]   | `UrsaSemiTheme.OverrideLocaleResources(Application, CultureInfo?)`   | static   | app-scoped locale override                           |
|  [05]   | `UrsaSemiTheme.OverrideLocaleResources(StyledElement, CultureInfo?)` | static   | element-scoped locale override                       |

- `UrsaSemiTheme` ships locale dictionaries for `zh-CN` (default), `en-US`, `de-DE`, `fr-FR`, `ru-RU`, `pl-PL`, and `cs-CZ`, and its `ThemeDictionaries` bind `ThemeVariant.Default`, `.Light`, `.Dark`, and the four `SemiTheme` high-contrast variants (`Aquatic`, `Desert`, `Dusk`, `NightSky`).

[THEME_KEYS]: named `x:Key` entries a consumer applies with `Theme="{DynamicResource …}"` or overrides to re-skin; every unnamed control theme is keyed by control type and applies implicitly

- icon-button bases: `BaseIconButton` `BaseSolidIconButton` `BaseOutlineIconButton` `BaseBorderlessIconButton` — `IconButton` alone carries the four-base skeleton
- icon-button variants: `{Solid,Outline,Borderless}IconButton` and the same triple on `IconSplitButton` `IconDropDownButton` `IconRepeatButton`; `IconToggleButton` and `IconToggleSplitButton` ship the implicit type-keyed theme alone
- icon-button parts: `IconButtonTemplate` `IconSplitButtonTemplate` `IconDropDownButtonTemplate` `IconButtonInnerSpacing` `InnerIconButton`
- `ButtonGroup`: `ButtonGroupItemTheme` binds onto every item, and the brush grid composes as `ButtonGroup` + variant (`Default`, `Solid`) + intent (`Primary`, `Secondary`, `Tertiary`, `Success`, `Warning`, `Danger`) + state (`Pointerover`, `Pressed`, `Disabled`) + slot (`Background`, `BorderBrush`, `Foreground`); one new intent or state lands as a row on that grid
- `ButtonGroup` metrics: `ButtonGroup{CornerRadius,SeparatorForeground,SeparatorHeight,DefaultFontSize,DefaultFontWeight}` with `{Small,Default,Large}{MinHeight,Padding}`
- `Banner`: `Banner{Information,Success,Warning,Error}{Background,BorderBrush,IconGeometry}` with `Banner{BorderBrush,BorderPadding,BorderThickness,CornerRadius,IconMargin,TitleFontSize,CloseButtonForeground,CloseButtonMargin,CloseIconGeometry}`
- `NotificationCard`: `NotificationCardLight{,Information,Success,Warning,Error}{Background,BorderBrush}`, the frame keys `NotificationCard{Background,BorderThickness,BoxShadows,CornerRadius,Margin,MinWidth,Padding,IconMargin,TitleSpacing}`, the typography pair `NotificationCard{Title,Message}{FontSize,FontWeight,Foreground}`, and per-severity `NotificationCard{Information,Success,Warning,Error}Icon{Foreground,PathData}`
- `ToastCard`: `ToastCard{Background,BorderThickness,CornerRadius,Margin,MinHeight,Padding}`, `ToastCardIcon{Height,Width,Margin}`, `ToastCardContent{FontWeight,Foreground,Margin,MaxWidth}`
- `Skeleton`: `Skeleton{Default,StartAnimation,EndAnimation}Background` — the shimmer stops the theme animation interpolates
- `TwoTonePathIcon`: `TwoTonePathIcon{Foreground,StrokeBrush,ActiveForeground,ActiveStrokeBrush}`
- `SelectionList`: `SelectionListIndicatorBackground` fills the sliding indicator
- `Divider`: `DividerBorderBrush` rules the line, `DividerLeftLine` and `DividerRightLine` theme the two flanks
- `Form`: `FormAsteriskForeground` marks required, `FormGroupForeground` heads a group
- `Loading`: `LoadingIconForeground` `LoadingMaskBackground`
- named part themes: `ButtonPathPicker` `ListPathPicker` `TinyPagination` `PrimaryScrollToButton` `CloseButton` `OverlayCloseButton` `TagInputTextBoxTheme` `ToolBarExpandToggleButton` `DefaultNavMenuItemTemplate`
- Every Ursa brush resolves from the shared `SemiColor*` palette slots (`SemiColorPrimary`, `SemiColorDanger`, `SemiColorText0`, `SemiColorFill1`, …) and the `Semi*` ramp colors, so a palette override in `Application.Resources` re-tints every Ursa control without touching a control theme.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every overlay — dialog, drawer, confirm — is raised vm-first through the static `OverlayDialog`/`Drawer`/`OverlayDrawer`/`MessageBox.ShowOverlayAsync` dispatch against a registered host id; one `OverlayDialogHost` per shell region carries its `HostId`, and no code instantiates a popup or mutates the visual tree.
- Transient feedback queues through one installed `WindowToastManager`/`WindowNotificationManager`; product code resolves the matching `IToastManager`/`INotificationManager` and calls `Show`/`Close`/`CloseAll`, every message an `IMessage`.
- Severity is Avalonia's `NotificationType` end to end — `IMessage.Type`, `Banner.Type`, `MessageCard.NotificationType` — and the theme selects `:information`/`:success`/`:warning`/`:error` from it.
- Control visuals resolve the shared `semi:` OKLCH token system with `<semi:UrsaSemiTheme/>` last in the single `Application.Styles` chain.

[STACKING]:
- `api-semi.md`: `UrsaSemiTheme` extends the `Semi.Avalonia` token system under the shared `https://irihi.tech/semi` (`semi:`) xmlns; the chain is `FluentTheme` floor -> `<semi:SemiTheme/>` -> the per-control `Semi.Avalonia.*` skins -> `<semi:UrsaSemiTheme/>`, every Ursa entry below `SemiTheme` so its `SemiColor*` slots resolve, and `UrsaSemiTheme.OverrideLocaleResources` mirrors `SemiTheme.OverrideLocaleResources` so a culture swap drives both locale dictionaries.
- `api-dialoghost.md`: `OverlayDialog`/`Drawer`/`MessageBox` mirror `DialogHost.Avalonia` in-canvas, sharing its vm-first-against-a-host-id dispatch shape.
- `api-reactiveui-avalonia.md`: `ReactiveUrsaWindow<TViewModel>`/`ReactiveUrsaView<TViewModel>` bind `UrsaWindow`/`UrsaView` onto the `ReactiveUI.Avalonia` rail as the admitted MVVM view bases, each exposing `ViewModel` as a styled property under `IViewFor<T>`.
- within-lib: the awaited `ShowModal`/`ShowCustomModal<TResult>`/`ShowStandardAsync` overloads return `Task<DialogResult>`/`Task<TResult?>` and bind into a LanguageExt `Eff`/`OptionT` rail at the boundary with the supplied `CancellationToken` threaded; the product intent vocabulary maps onto `NotificationType` at the manager call, and `OnClose(MessageCloseReason)` drives dismissal-cause follow-up.

[LOCAL_ADMISSION]:
- `Shell/Controls` composes Ursa for the families the curated Avalonia + `bodong.PropertyGrid` + `Dock` + `DataGrid` roster lacks, reusing `Ursa.Common` placement (`Position`/`ItemAlignment`/`CornerPosition`) and the per-control enums as policy vocabulary; the typed `Numeric<T>UpDown` matching the bound CLR type carries numeric entry, and shell views derive from the `ReactiveUrsa` bases.
- `DialogOptions`/`OverlayDialogOptions`/`DrawerOptions` carry button set, modality, placement, and resize policy; `Irihi.Avalonia.Shared` stays the one shared primitive closure both suites floor on.
- Ursa ships no command palette, spotlight, or fuzzy-search control; that capability is sourced or built in the branch, never expected from this suite.

[RAIL_LAW]:
- Package: `Irihi.Ursa` + `Irihi.Ursa.Themes.Semi` + `Irihi.Ursa.ReactiveUIExtension`
- Owns: the extended-control families (navigation, feedback, overlay, data entry, selection, display, layout panels, the date/time/range pickers), in-canvas overlay/drawer/confirm dispatch, queued transient feedback, the `UrsaSemiTheme` control-theme bridge, and the `ReactiveUrsa` MVVM bases
- Accept: vm-first static dispatch against a host id returning `Task<DialogResult>`/`Task<TResult?>` awaited into an `Eff`/`OptionT` rail with the `CancellationToken` threaded; one installed manager per feedback family; binding-projected item graphs on `NavMenu`/`Timeline`/`ButtonGroup`; `<semi:UrsaSemiTheme/>` last in the `semi:` chain
- Reject: hand-rolling a nav menu, timeline, OTP field, masked IP/time box, multi-select combo, segmented selector, or rating Ursa owns; a second overlay-host layer or per-screen feedback stack; blocking on `.Result` instead of awaiting the dispatch; declaring a parallel placement or severity enum where `Ursa.Common` or `NotificationType` names the axis; restyling a control theme wholesale where a `SemiColor*` palette override re-tints it; loading `UrsaSemiTheme` without or ahead of `SemiTheme`; deriving shell views from the non-reactive bases when the ReactiveUI bridge is admitted; re-pinning `Irihi.Avalonia.Shared` divergent from the Semi closure
