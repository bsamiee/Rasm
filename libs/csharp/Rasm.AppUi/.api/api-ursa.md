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
|  [04]   | `Drawer`                                        | class         | OBSOLETE forwarder onto `OverlayDrawer`        |
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

`NumericUpDownBase<T>` clamps every commit through `protected virtual T? CoerceCurrentValue(T?)` — a null becomes `EmptyInputValue`, an out-of-range value becomes `Minimum` or `Maximum` — and `NumericDoubleUpDown`/`NumericFloatUpDown` override it to pass `NaN` through unclamped.

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

`KeyGestureInput : TemplatedControl, IClearControl, IInnerContentControl` is the shipped chord-capture surface: `StyledProperty<KeyGesture?> GestureProperty` (default one-way, so a consumer binds two-way explicitly and the control writes through `SetCurrentValue`), `IList<Key>? AcceptableKeys` (null admits every key), `bool ConsiderKeyModifiers` (default true), `object? InnerLeftContent`/`InnerRightContent`, the alignment pair added from `ContentControl`, `void Clear()` nulling the gesture, and the `PC_Empty = ":empty"` pseudo-class the static ctor keeps in step with the gesture on every change and `OnApplyTemplate` re-asserts.

- `ConsiderKeyModifiers: false` does NOT strip modifiers. `OnKeyDown` writes the bare-key gesture inside that branch and then FALLS THROUGH into the modifier switch, which overwrites it with `new KeyGesture(e.Key, e.KeyModifiers)`; the branch's only lasting effect is an early return for `LWin`/`RWin` and the six modifier keys. The false posture therefore yields exactly what the true posture yields, minus a silently dropped keystroke class.
- A LONE modifier press under only its own modifier records as a BARE-key gesture of that modifier key — `Alt`+`LeftAlt`, `Control`+`LeftCtrl`, `Shift`+`LeftShift`, and `Meta`+`LWin` each land `new KeyGesture(e.Key, KeyModifiers.None)` — so a consumer that treats every published value as bindable commits chords no key binding can match; the refusal belongs to the consumer, because the control publishes the value either way.
- `OnKeyDown` marks `Handled` only on the paths that WRITE; both early returns (an unadmitted `AcceptableKeys` key, and a modifier key under the false posture) leave the event to continue up the tree.

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
|  [07]   | `MarkdownLine`                               | class         | inline-markdown selectable text block      |
|  [08]   | `Descriptions` / `DescriptionsItem`          | class         | key-value grid + entry                     |
|  [09]   | `Divider`                                    | class         | titled divider                             |
|  [10]   | `ImageViewer` / `ImageViewerPresenter`       | class         | pan-zoom image viewer + presenter          |
|  [11]   | `PanZoomGestureHandler`                      | class         | pan/zoom gesture recognizer                |
|  [12]   | `PinchUpdateEventArgs`                       | class         | pinch-gesture event                        |
|  [13]   | `QRCode`                                     | class         | QR renderer (`EccLevel`)                   |
|  [14]   | `IconButton`                                 | class         | icon-leading button, icon quartet owner    |
|  [15]   | `IconToggleButton` / `IconToggleSplitButton` | class         | icon toggle and toggle-split buttons       |
|  [16]   | `IconSplitButton` / `IconDropDownButton`     | class         | icon split and drop-down buttons           |
|  [17]   | `IconRepeatButton`                           | class         | icon repeat button                         |
|  [18]   | `TwoTonePathIcon`                            | class         | fill + stroke two-tone glyph               |
|  [19]   | `LabeledContentControl`                      | class         | abstract labeled container                 |
|  [20]   | `GroupBoxBorder` / `UrsaGroupBox`            | class         | notched border decorator + group container |

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

`ProportionalCanvas` places each child by the attached `RelativeScalar` quartet `Left`/`Top`/`Right`/`Bottom` (`ProportionalCanvas.SetLeft(Control, RelativeScalar)` and its `Get*` peer per edge): `RelativeUnit.Relative` reads the scalar as a fraction of the arranged bound, an opposed pair stretches the child across that axis, and a lone edge seats it at desired size.

| [INDEX] | [SYMBOL]                                      | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :-------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `AspectRatioLayout` / `AspectRatioLayoutItem` | class         | `AspectRatioMode` transitioning container |
|  [02]   | `ColumnWrapPanel`                             | class         | column-major navigable wrap panel         |
|  [03]   | `ElasticWrapPanel`                            | class         | stretch-to-fill wrap panel                |
|  [04]   | `OverflowStackPanel`                          | class         | overflow-aware stack panel                |
|  [05]   | `WrapPanelWithTrailingItem`                   | class         | wrap panel reserving a trailing slot      |
|  [06]   | `VirtualizingUniformGrid`                     | class         | virtualizing uniform grid with snap       |
|  [07]   | `ProportionalCanvas`                          | class         | `RelativeScalar` edge-anchored canvas     |
|  [08]   | `TimelinePanel`                               | class         | timeline arrange panel                    |
|  [09]   | `OffsetDefinition` / `OffsetDefinitions`      | class         | `OffsetDefinitionKind` offset spec + list |
|  [10]   | `OffsetValue`                                 | struct        | readonly offset scalar                    |
|  [11]   | `OffsetDefinitionConverter` / `...sConverter` | class         | XAML `TypeConverter` for offset specs     |
|  [12]   | `OffsetValueConverter`                        | class         | XAML `TypeConverter` for offset scalars   |

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

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                                                  |
| :-----: | :----------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `DialogButton`           | enum           | dialog button set                                             |
|  [02]   | `DialogResult`           | enum           | dialog result                                                 |
|  [03]   | `DialogMode`             | enum           | dialog modality                                               |
|  [04]   | `DialogLayerChangeType`  | enum           | dialog z-order change                                         |
|  [05]   | `MessageBoxButton`       | enum           | message-box button set                                        |
|  [06]   | `MessageBoxResult`       | enum           | message-box result                                            |
|  [07]   | `MessageBoxIcon`         | enum           | message-box icon                                              |
|  [08]   | `MessageCloseReason`     | enum           | `Timeout` / `UserAction` / `Displaced` dismissal cause        |
|  [09]   | `PinCodeMode`            | enum           | PIN entry mode                                                |
|  [10]   | `IPv4BoxInputMode`       | enum           | IPv4 entry mode                                               |
|  [11]   | `TimeBoxInputMode`       | enum           | time entry mode                                               |
|  [12]   | `TimeBoxDragOrientation` | enum           | time drag direction                                           |
|  [13]   | `TimelineDisplayMode`    | enum           | timeline layout                                               |
|  [14]   | `TimelineItemPosition`   | enum           | timeline position                                             |
|  [15]   | `TimelineItemType`       | enum           | timeline item kind                                            |
|  [16]   | `OverflowMode`           | enum           | `AsNeeded` / `Always` / `Never` tool-bar overflow policy      |
|  [17]   | `LostFocusBehavior`      | enum           | commit policy                                                 |
|  [18]   | `PopConfirmTriggerMode`  | enum           | confirmation trigger                                          |
|  [19]   | `ResizeDirection`        | `[Flags]` enum | eight edges plus `Sides` / `Corners` / `All` composites       |
|  [20]   | `Direction`              | enum           | direction policy                                              |
|  [21]   | `Position`               | enum           | `Left` / `Top` / `Right` / `Bottom` placement (`Ursa.Common`) |
|  [22]   | `ItemAlignment`          | enum           | common alignment (`Ursa.Common`)                              |
|  [23]   | `CornerPosition`         | enum           | common corner placement (`Ursa.Common`)                       |
|  [24]   | `HorizontalPosition`     | enum           | horizontal placement                                          |
|  [25]   | `VerticalPosition`       | enum           | vertical placement                                            |
|  [26]   | `UsePickerTypes`         | enum           | `PathPicker` file/folder/save policy                          |
|  [27]   | `ThemeSelectorMode`      | enum           | `Controller` / `Indicator` theme-selector role                |
|  [28]   | `AspectRatioMode`        | enum           | aspect-ratio policy                                           |
|  [29]   | `OffsetDefinitionKind`   | enum           | offset definition                                             |
|  [30]   | `EccLevel`               | enum           | QR correction level                                           |

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
|  [10]   | `TreeLevelToPaddingConverter`           | class         | tree-level to padding multi-value converter                 |

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
|  [07]   | `OverlayDialog.Show(TVm, string?, OverlayDialogOptions?)`                                         | static  | obsolete forwarder     |
|  [08]   | `OverlayDialog.ShowStandard(TVm, string?, OverlayDialogOptions?)`                                 | static  | overlay fire           |
|  [09]   | `OverlayDialog.ShowStandardAsync(TVm, string?, OverlayDialogOptions?, CancellationToken?)`        | static  | overlay awaited        |
|  [10]   | `OverlayDialog.ShowModal(TVm, string?, OverlayDialogOptions?, CancellationToken?)`                | static  | obsolete forwarder     |
|  [11]   | `OverlayDialog.ShowCustom(TVm, string?, OverlayDialogOptions?)`                                   | static  | overlay custom fire    |
|  [12]   | `OverlayDialog.ShowCustomAsync<TResult>(TVm, string?, OverlayDialogOptions?, CancellationToken?)` | static  | overlay custom awaited |
|  [13]   | `OverlayDialog.ShowCustomModal<TResult>(TVm, string?, OverlayDialogOptions?, CancellationToken?)` | static  | obsolete forwarder     |

- `OverlayDialog.ShowStandardAsync` returns `Task<DialogResult>`, `ShowCustomAsync<TResult>` returns `Task<TResult?>`, and the `ShowStandard`/`ShowCustom` fire shapes return void.
- MODALITY IS THE SHAPE, not the member name: the void fire shapes land on `AddDialog` and are the only plain in-canvas layer, while EVERY awaited overload lands on `AddModalDialog` and paints the host mask — so a co-resident layer takes a fire shape and an awaited one scrims whatever it sits over.
- `OverlayDialog.Show`, `ShowModal`, and `ShowCustomModal` carry `[Obsolete]` and forward verbatim onto `ShowStandard`, `ShowStandardAsync`, and `ShowCustomAsync`; binding one buys a deprecation with no capability in it, exactly as the `Drawer` forwarder does.

[DRAWER_DISPATCH]: `OverlayDrawer` is THE static in-canvas drawer dispatcher against `string? hostId`; the whole `Drawer` type carries `[Obsolete]` and every one of its members forwards verbatim onto an `OverlayDrawer` member, so the two candidate mechanisms are one mechanism and a deprecation. `DrawerControlBase` is the drawer control base, never a dispatcher

| [INDEX] | [SURFACE]                                                                                | [SHAPE] | [CAPABILITY]     |
| :-----: | :--------------------------------------------------------------------------------------- | :------ | :--------------- |
|  [01]   | `OverlayDrawer.ShowStandard(TVm, string?, DrawerOptions?)`                               | static  | standard fire    |
|  [02]   | `OverlayDrawer.ShowStandardAsync(TVm, string?, DrawerOptions?) -> Task<DialogResult>`    | static  | standard awaited |
|  [03]   | `OverlayDrawer.ShowCustom(TVm, string?, DrawerOptions?)`                                 | static  | custom fire      |
|  [04]   | `OverlayDrawer.ShowCustomAsync<TResult>(TVm, string?, DrawerOptions?) -> Task<TResult?>` | static  | custom awaited   |
|  [05]   | `OverlayDrawer.ShowCustom*(Control view, object? vm, string?, DrawerOptions?)`           | static  | view-first       |
|  [06]   | `Drawer.{Show,ShowModal,ShowCustom,ShowCustomModal}`                                     | static  | obsolete forward |

- The fire shapes land on `AddDrawer` (non-modal, mask only when light dismiss is admitted) and the awaited shapes on `AddModalDrawer`; every one first resolves the host through the INTERNAL `OverlayDialogManager.GetHost(hostId, topLevelHash)`.
- An UNREGISTERED host id is silent: the void shapes return having done nothing and the awaited shapes return `Task.FromResult(DialogResult.None)` — the same value a user cancel produces — so a consumer proves registration from its own mount fact before dispatching, the registry being unreachable by name.

[MESSAGEBOX_DISPATCH]: `MessageBox` windowed and overlay confirmation — message/title/icon/button-first, each returning `Task<MessageBoxResult>`

| [INDEX] | [SURFACE]                                                                                                | [SHAPE] | [CAPABILITY]       |
| :-----: | :------------------------------------------------------------------------------------------------------- | :------ | :----------------- |
|  [01]   | `MessageBox.ShowAsync(string, string?, MessageBoxIcon, MessageBoxButton, string?)`                       | static  | windowed           |
|  [02]   | `MessageBox.ShowAsync(Window, string, string, MessageBoxIcon, MessageBoxButton, string?)`                | static  | owner-anchored     |
|  [03]   | `MessageBox.ShowAsync(IObservable<string>, IObservable<string>?, MessageBoxIcon, …)`                     | static  | windowed live-text |
|  [04]   | `MessageBox.ShowAsync(Window, IObservable<string>, IObservable<string>, MessageBoxIcon, …)`              | static  | anchored live-text |
|  [05]   | `MessageBox.ShowOverlayAsync(string, string?, string?, MessageBoxIcon, MessageBoxButton, int?, string?)` | static  | registered host    |
|  [06]   | `OverlayMessageBox.ShowAsync(string, string?, string?, MessageBoxIcon, MessageBoxButton, int?, string?)` | static  | registered host    |
|  [07]   | `OverlayMessageBox.ShowAsync(IObservable<string>, IObservable<string>?, string?, MessageBoxIcon, …)`     | static  | host live-text     |

- Each overload takes its trailing `string?` as a style class stamped on the dialog shell, and the `int?` as a top-level hash code selecting one of several open windows.
- `MessageBoxControl` and `MessageBoxWindow` expose `ContentSource` and `TitleSource` (`IObservable<string>?`) that the observable overloads bind, so a localized or streaming caption re-renders in the open box and each subscription disposes on detach or close. `ShowOverlayAsync` carries the string shape alone; `OverlayMessageBox.ShowAsync` runs the observable host-targeted path.

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

- `WindowToastManager.Show` COPIES the message's type, expiration, icon, close, click, and close-callback onto a freshly built `ToastCard` whose `Content` is the message object, then posts the add and awaits a bare delay before closing with `Timeout`. A later mutation of the payload's `Type` or `Expiration` therefore re-tints and re-times nothing, so a product that must morph a live note or pause its clock presents with `TimeSpan.Zero` — the manager's own never-auto-close posture — and owns the timer itself.
- On overflow the show path closes the FIRST non-closing card with `MessageCloseReason.Displaced`; `MaxItems` is the styled ceiling and `_items` is the `protected IList?` a derived manager reaches for its own seating and reflow.
- `Show`, `Close`, and `CloseAll` each call `Dispatcher.UIThread.VerifyAccess()`, so every crossing marshals; the wide `Show` overload is `async void`, so its delay runs detached.
- `IToastManager.Close(toast)` matches the card whose `Content` is that payload by REFERENCE and closes it with the default cause; `MessageCard.Close(MessageCloseReason reason = UserAction)` is the public close, so a product-initiated close states its own cause rather than inheriting the user-action default.

[BUTTON_PROPERTIES]: `IconButton` registers the icon quartet once, and its static `IconButton.Get*`/`Set*` pair reaches any `ContentControl`

- `IconButton`: `Icon:object?` `IconTemplate:IDataTemplate?` `IconPlacement:Position` (`Left`) `IsLoading:bool`; part `PART_RootPanel`; classes `:left` `:right` `:top` `:bottom` `:empty` `:empty-content`
- `IconToggleButton` `IconSplitButton` `IconToggleSplitButton` `IconDropDownButton` `IconRepeatButton`: the same quartet by `AddOwner`, over `ToggleButton` `SplitButton` `ToggleSplitButton` `DropDownButton` `RepeatButton`
- `BreadcrumbItem : IconButton`: inherits the icon quartet and `Button.Command`/`CommandParameter`, and adds `Separator:object?` `IsReadOnly:bool`; class `:last`. `Breadcrumb` projects its `IconBinding`/`CommandBinding`/`CommandParameterBinding` onto those inherited properties, so a breadcrumb entry styles and dispatches as an icon button
- `ButtonGroup`: `CommandBinding:BindingBase?` `CommandParameterBinding:BindingBase?` `ContentBinding:BindingBase?` — one binding triple projects every item, and item visuals resolve `ButtonGroupItemTheme`
- `SelectionList`: `Indicator:Control?`; part `PART_Indicator`; `SelectionMode` is coerced to `Single`, so the indicator slides between exactly one selected segment
- `SelectionListItem`: `IsSelected:bool` (`ISelectable`)

[OPTION_PROPERTIES]: the two option records the overlay dispatchers configure their shells from; every column is a plain settable property and each record's `Default` is internal, so a caller constructs its own

- `OverlayDialogOptions` (`Ursa.Controls`): `FullScreen:bool` `HorizontalAnchor:HorizontalPosition` (`Center`) `VerticalAnchor:VerticalPosition` (`Center`) `HorizontalOffset:double?` `VerticalOffset:double?` `Mode:DialogMode` (`None`) `Buttons:DialogButton` (`OKCancel`) `Title:string?` `IsCloseButtonVisible:bool?` (`true`) `CanLightDismiss:bool` `CanDragMove:bool` (`true`) `TopLevelHashCode:int?` `CanResize:bool` `StyleClass:string?` `Horizontal`/`VerticalScrollBarVisibility:ScrollBarVisibility`; `ShowCloseButton` is obsolete
- `DrawerOptions` (`Ursa.Controls.Options`): `Position:Position` (`Right`) `CanLightDismiss:bool` (`true`) `IsCloseButtonVisible:bool?` (`true`) `MinWidth`/`MinHeight`/`MaxWidth`/`MaxHeight:double?` `Buttons:DialogButton` (`OKCancel`) `Title:string?` `TopLevelHashCode:int?` `CanResize:bool` `StyleClass:string?` and the same scrollbar pair; `ShowCloseButton` is obsolete — the drawer's placement is this `Position` edge and NOT an `IDialogPopupPositioner`, which belongs to the DialogHost overlay alone
- `StyleClass` stamps one class on the constructed shell, so a product theme selects on it; `TopLevelHashCode` picks among several registered hosts sharing an id

[NAVIGATION_PROPERTIES]: `NavMenu` projects an arbitrary item graph through binding properties and collapses horizontally between `CollapseWidth` and `ExpandWidth`

- `NavMenu`: `Header:object?` `HeaderTemplate:IDataTemplate?` `Footer:object?` `SelectedItem:object?` `IconBinding` `HeaderBinding` `SubMenuBinding` `CommandBinding` (`BindingBase?`) `IconTemplate:IDataTemplate?` `SubMenuIndent:double` `IsHorizontalCollapsed:bool` `CollapseWidth:double` `ExpandWidth:double` `CanToggle:bool`
- `NavMenu` events and parts: `SelectionChangedEvent` `SelectionChangingEvent`; part `PART_ItemsPresenter`; class `:horizontal-collapsed`
- `NavMenuItem`: `Icon:object?` `IconTemplate:IDataTemplate?` `Command:ICommand?` `CommandParameter:object?` `IsSelected:bool` `IsHighlighted:bool` (direct) `Level:int` (direct, depth from the owning `NavMenu`) `IsHorizontalCollapsed:bool` `IsVerticalCollapsed:bool` `SubMenuIndent:double` `IsSeparator:bool`
- `NavMenuItem` events and classes: `IsSelectedChangedEvent`; classes `:highlighted` `:first-level` `:horizontal-collapsed` `:vertical-collapsed` `:selector`
- `Anchor`: `TargetContainer:ScrollViewer?` `TopOffset:double` `AnchorIdMemberBinding:BindingBase?`; the attached `Anchor.SetId(Visual, string?)`/`GetId(Visual)` marks each scroll target, and `InvalidatePositions()` re-measures after the scrolled content changes
- `AnchorItem : HeaderedItemsControl, ISelectable`: `AnchorId:string?` `IsSelected:bool` `Level:int` — `AnchorIdMemberBinding` binds `AnchorId` off each bound item, so a projected item graph needs no `AnchorItem` per entry
- `ToolBar`: `Orientation:Orientation` `PopupPlacement:PlacementMode`, and the attached `OverflowMode` (`ToolBar.SetOverflowMode(Control, OverflowMode)`, default `AsNeeded`) per child; part `PART_OverflowPanel`; class `:overflow`
- `TitleBar`: `LeftContent:object?` `CenterContent:object?` `RightContent:object?`
- `UrsaWindow : Window` — the caption surface below is the Ursa family's alone, so a policy writer spanning it, `SplashWindow`, and a foreign `Window` subclass constrains at `Window` and gates these six on the type: `IsTitleBarVisible` (`true`) `IsMinimizeButtonVisible` (`true`) `IsRestoreButtonVisible` (`true`) `IsFullScreenButtonVisible` (`false`) `IsCloseButtonVisible` (`true`) `IsManagedResizerVisible` (`false`) `TitleBarContent:object?` `LeftContent:object?` `RightContent:object?` `TitleBarMargin:Thickness`; part `PART_DialogHost`; consts `PART_DialogHost` and `KEY_URSAWINDOW_DRAWN_DECORATIONS`; `protected virtual Task<bool> CanClose()` is the async close veto `OnClosing` awaits, so a dirty-state prompt overrides it rather than handling the event
- `SplashWindow : Window` (abstract) — NOT a `UrsaWindow`, so it carries none of the caption or resizer properties and its chromelessness comes from `Window.WindowDecorations` alone: `CountDown:TimeSpan?` is the MINIMUM display span; `protected abstract Task<Window?> CreateNextWindow()` returns the window the splash hands off to and `protected object? DialogResult { get; private set; }` carries its outcome; `protected virtual Task<bool> CanClose()` gates dismissal and `OnClosing` is `sealed`, so the handoff order is the package's and a boot-time timer beside it is unrepresentable
- `WindowResizer` is a bare `TemplatedControl` whose template seats `WindowResizerThumb : Thumb`, which carries `ResizeDirection:ResizeDirection` alone; `ResizeDirection` is a `[Flags]` enum — `Top=1` `Bottom=2` `Left=4` `Right=8` `TopLeft=0x10` `TopRight=0x20` `BottomLeft=0x40` `BottomRight=0x80` with the composites `Sides=0xF`, `Corners=0xF0`, `All=0xFF`
- `ReactiveUrsaWindow<TViewModel>` / `ReactiveUrsaView<TViewModel>` (`where TViewModel : class`) derive `UrsaWindow`/`UrsaView` and add `ViewModel:TViewModel?` as a styled property under `IViewFor<T>` and `IActivatableView`; `RoutedViewHost`/`ViewModelViewHost` resolve a view through `IViewFor<T>` alone, so a shell view off these bases is unresolvable by the router

[FEEDBACK_PROPERTIES]: severity rides Avalonia's `NotificationType` on every Ursa feedback surface, and the theme selects the matching pseudo-class

- `Banner : HeaderedContentControl`: `Type:NotificationType` `Icon:object?` `ShowIcon:bool` (`true`) `CanClose:bool` (`false`); part `PART_CloseButton`; class `:icon` — the header is the title and the content the body, so verbs and evidence ride the content and the strip carries no action slot of its own
- `PopConfirm : ContentControl`: `PopupHeader`/`PopupContent:object?` with their `IDataTemplate?` twins, `ConfirmCommand`/`CancelCommand:ICommand?` with their `object?` parameters, `TriggerMode:PopConfirmTriggerMode` (`[Flags]` `Click`=1 `Focus`=2), `HandleAsyncCommand:bool`, `IsDropdownOpen:bool`, `Placement:PlacementMode`, `Icon:object?`; parts `PART_Popup` `PART_ConfirmButton` `PART_CancelButton` `PART_CloseButton`; class `:dropdownopen` — the WRAPPED content is the trigger, so a consumer authors the wrapper around the trigger in its own tree and drives `IsDropdownOpen` rather than re-parenting a live control
- `MessageCard`: `NotificationType:NotificationType` `ShowIcon:bool` `ShowClose:bool` `IsClosed:bool` `IsClosing:bool` (direct); event `MessageClosed` (`MessageClosedEventArgs`, bubbling); classes `:information` `:success` `:warning` `:error`
- `MessageCard.CloseOnClick`: attached on `Button` (`MessageCard.SetCloseOnClick(Button, bool)`) — any templated button closes the owning card
- `ToastCard`: inherits `MessageCard` whole; `NotificationCard` adds `Position:NotificationPosition` (direct) with classes `:topleft` `:topright` `:bottomleft` `:bottomright` `:topcenter` `:bottomcenter`
- `WindowNotificationManager`: `Position:NotificationPosition` and the same six corner classes
- `Loading`: `Indicator:object?` `IsLoading:bool`; `LoadingIcon`: `IsLoading:bool`
- `LoadingContainer`: `Indicator:object?` `LoadingMessage:object?` `LoadingMessageTemplate:IDataTemplate` `MessageForeground:IBrush?` `IsLoading:bool`; class `:loading`
- `Skeleton`: `IsActive:bool` (shimmer animation) `IsLoading:bool` (placeholder vs content)
- `Badge`: `BadgeTheme:ControlTheme` `Dot:bool` `CornerPosition:CornerPosition` `OverflowCount:int` `BadgeFontSize:double`; parts `PART_BadgeContainer` `PART_HeaderPresenter` `PART_ContentPresenter`
- `DualBadge`: `Icon:object?` `IconTemplate:IDataTemplate?` `IconForeground` `HeaderForeground` `HeaderBackground` (`IBrush?`); part `PART_Icon`; classes `:icon-empty` `:header-empty` `:content-empty`

[FORM_PROPERTIES]: `Form : ItemsControl` owns label geometry across its whole item tree, `FormItem : ContentControl` carries one field row, and `FormGroup : HeaderedItemsControl` collects rows under a header alone

- `Form`: `LabelPosition:Position` (`Top`) `LabelWidth:GridLength` (unset) `LabelAlignment:HorizontalAlignment` (`Left`); class `:fixed-width` whenever `LabelWidth` `IsStar` OR `IsAbsolute`
- `FormItem`: `LabelWidth:double` `LabelAlignment:HorizontalAlignment` styled, plus the ATTACHED trio `Label:object?` `IsRequired:bool` `NoLabel:bool` registered `FormItem`-to-`Control` with `FormItem.SetLabel`/`SetIsRequired`/`SetNoLabel` and their `Get*` peers; part `PART_Label` (an Avalonia `Label`); classes `:horizontal` (`LabelPosition.Left`) `:no-label` (`NoLabel`, an `AffectsPseudoClass` registration) — and NO `:required` state exists, so the required mark reaches appearance through the template's own asterisk under `FormAsteriskForeground` and a `:required` selector matches nothing
- `Form.NeedsContainerOverride` admits a `FormItem` or `FormGroup` child as its own container and wraps every other `Control` in a fresh `FormItem` whose `Content` is that child, COPYING the child's attached `Label`, `IsRequired`, and `NoLabel` across — so a consumer stamps the attached trio on any control and hands it to `Items`; `FormGroup` does the same for `Label`/`IsRequired`, and both propagate their own `ItemTemplate` onto a container that carries none
- `FormItem.OnAttachedToVisualTree` resolves the nearest `Form` ANCESTOR (a visual-ancestor walk, not a direct parent read) and subscribes its three geometry properties: `Form.LabelWidth` republishes as `IsAbsolute ? Value : double.NaN`, `LabelPosition` drives the row's `:horizontal` state, and `LabelAlignment` republishes whole — so an intervening panel keeps a row's geometry live, and the subscriptions dispose on detach
- `FormItem` re-targets its `PART_Label` on every `Label` and `Content` change and again at load: the target is the content when the content is an `InputElement`, else the first focusable logical child — so label-for association is the control's own and an authored association column has nothing to add. `LabeledContentControl` (`DescriptionsItem`'s base) runs the same hook over its presenter's logical descendants and exposes `Label:object?` `LabelTemplate:IDataTemplate?` `LabelHost:Label?`

[DESCRIPTION_PROPERTIES]: the read-only key-value surfaces beside the form family

- `Descriptions : ItemsControl`: `LabelTemplate:IDataTemplate?` `LabelMemberBinding:BindingBase?` `LabelPosition:Position` `LabelWidth:GridLength` `ItemAlignment:ItemAlignment` `Orientation:Orientation`; class `:fixed-width`; a non-`DescriptionsItem` item wraps into one and a `DescriptionsItem` item takes its bindings in place
- `DescriptionsItem : LabeledContentControl`: `LabelPosition:Position` (`Left`) `ItemAlignment:ItemAlignment` (`Center`) `LabelWidth:double`; classes `:horizontal` `:vertical`
- `UrsaGroupBox : HeaderedContentControl`: `HeaderSpacing:double` added by owner off `GroupBoxBorder`; `GroupBoxBorder : Decorator` carries `Background`/`BorderBrush:IBrush?` `BorderThickness:Thickness` `CornerRadius:CornerRadius` `Header:Control?` `HeaderSpacing:double` and draws the notched geometry itself

[OVERLAY_PROPERTIES]: `OverlayDialogHost` paints the modal scrim and reports modal state up an attached scope

- `OverlayDialogHost : Canvas`: `OverlayMaskBrush:IBrush?` `SafePadding:Thickness` `IsModalStatusReporter:bool` (styled) with attached `IsModalStatusScope:bool`/`IsInModalStatus:bool` and read-back `OverlayDialogHost.GetIsInModalStatus(Control)`; plus the plain CLR columns `HostId:string?` `IsTopLevel:bool` `IsAnimationDisabled:bool` `SnapThickness:Thickness` `DialogDataTemplates:DataTemplates`
- Registration is the framework's: `OnAttachedToVisualTree` captures the top-level hash and TRY-ADDS `(HostId, hash)` into the internal registry, so a duplicate key keeps the FIRST host and silently drops the second, and `OnDetachedFromVisualTree` closes every open layer then unregisters under the CURRENT `HostId` — a post-attach write to that plain property therefore strands the host under a key the registry never held
- The host keeps an ORDERED layer list with a per-layer mask and its own modal count, so co-resident layers are the design and z-order is list order; `IsInModalStatus` tracks that count and, where an ancestor carries `IsModalStatusScope`, exactly one reporter per scope may set `IsModalStatusReporter` because the subscription writes the scope flag unconditionally
- `DialogControlBase`: `IsFullScreen:bool` `CanResize:bool`, attached `CanDragMove`/`CanClose`, event `LayerChanged` (`DialogLayerChangeEventArgs`), and the PUBLIC `UpdateLayer(object?)` which raises a boxed `DialogLayerChangeType` (`BringForward` `SendBackward` `BringToFront` `SendToBack`) the host folds into list order; anchor, offset, light-dismiss, and close-button columns are INTERNAL and set by the dispatcher from its options record
- `OverlayFeedbackElement`: `IsClosed:bool`, event `Closed` (`ResultEventArgs`), `Task<T?> ShowAsync<T>(CancellationToken?)`, and the abstract `Close()` — the awaited result handle every layer carries, which is why two open layers hold two distinct handles and neither is ambiguous
- `DrawerControlBase`: `Position:Position` (`Right`) `CanResize:bool` `IsOpen:bool`; part `PART_CloseButton`
- `OverlayFeedbackElement`: `IsClosed:bool` — the shared closable base under `DialogControlBase` and `DrawerControlBase`

[DISPLAY_PROPERTIES]: binding-driven display controls project an item graph through `BindingBase?` members rather than a typed item class

- `Timeline`: `IconMemberBinding` `HeaderMemberBinding` `ContentMemberBinding` `TimeMemberBinding` (`BindingBase?`) `IconTemplate` `DescriptionTemplate` (`IDataTemplate?`) `TimeFormat:string?` `Mode:TimelineDisplayMode`
- `TimelineItem`: `Icon:object?` `IconTemplate:IDataTemplate?` `Type:TimelineItemType` `Position:TimelineItemPosition` `Time:DateTime` `TimeFormat:string?` `LeftWidth` `IconWidth` `RightWidth` (`double`, direct); parts `PART_RootGrid` `PART_Header` `PART_Icon` `PART_Content` `PART_Time`; classes `:first` `:last` `:empty-icon` `:all-left` `:all-right` `:separate`
- `Divider : ContentControl`: `Orientation:Orientation` — content is the inline title with `HorizontalContentAlignment` defaulted to `Center`, themed by `DividerLeftLine` and `DividerRightLine`
- `MarkdownLine : SelectableTextBlock`: `Markdown:string?` `CodeBackground:IBrush?` `CodeFontFamily:FontFamily?` (`Consolas, Menlo, monospace` fallback) — each assignment reparses `Markdown` into `Run` inlines and overrides `TextWrapping` to `Wrap`; the parser spans `**`/`__` bold, `*`/`_` italic, `~~` strikethrough, and backtick inline code alone, so block markdown, links, and images render as literal text
- `TwoTonePathIcon`: `Data:Geometry` `StrokeBrush:IBrush?` `StrokeThickness:double` `IsActive:bool` `ActiveForeground:IBrush?` `ActiveStrokeBrush:IBrush?`; class `:active`
- `Avatar : Button`: `Source:IImage?` `HoverMask:object?` — the inherited `Content` is the initial fallback the theme shows when `Source` is unset, and the inherited `Command` makes an avatar a command surface; the suite ships no cluster or overflow control, so a stacked group is a panel of these plus one trailing avatar carrying its own remainder count
- `ClosableTag : ContentControl`: `Command:ICommand?` — the dismiss verb the templated close button raises; part `PART_CloseButton`
- `SelectionListItem : ContentControl, ISelectable`: `IsSelected:bool` — the segment `SelectionList` slides its indicator to
- `RangeSlider`: `Minimum` `Maximum` `LowerValue` `UpperValue` `TrackWidth` `TickFrequency` (`double`) `Ticks:AvaloniaList<double>?` `TickPlacement:TickPlacement` `IsSnapToTick:bool` `IsDirectionReversed:bool` `Orientation:Orientation`; part `PART_Track`; event `ValueChangedEvent`
- `ThemeSelectorBase`: `Mode:ThemeSelectorMode` `SelectedTheme:ThemeVariant?` `TargetScope:ThemeVariantScope?`; `ThemeToggleButton` adds `IsThreeState:bool`, part `PART_ThemeButton`, classes `:light` `:dark` `:default`
- `ThemeVariantMapper : ThemeVariantScope`: `Mappings:AvaloniaList<ThemeVariantMapping>` — the subtree re-map a scoped surface posture rides, where `ThemeVariantMapping` carries `Source:ThemeVariant?` and `Target:ThemeVariant?`; a bare `ThemeVariantScope.RequestedThemeVariant` PINS one variant and stops tracking the application variant, so a subtree that must both follow the app variant and carry its own posture maps every concrete source onto its posture target here

[ENTRY_PROPERTIES]: `PathPicker`, `MultiComboBox`, and `EnumSelector` carry their whole policy as styled properties, so a consumer configures each in XAML alone

- `UsePickerTypes` is a THREE-member enum in declaration order `OpenFile` `SaveFile` `OpenFolder`, so the field's whole modality domain is that roster and a fourth picker posture does not exist to be reached for
- `PathPicker`: `UsePickerType:UsePickerTypes` `Title:string` `SuggestedStartPath:string` `SuggestedFileName:string` `DefaultFileExtension:string` `FileFilter:string` `AllowMultiple:bool` `ButtonContent:object?` `Command:ICommand?` `SelectedPaths:IReadOnlyList<string>` (direct) `SelectedPathsText:string?` `IsOmitCommandOnCancel:bool` `IsClearSelectionOnCancel:bool`; part `PART_Button`
- `MultiComboBox`: `SelectedItems:IList?` `SelectedItemTemplate:IDataTemplate?` `IsDropDownOpen:bool` `MaxDropDownHeight:double` `MaxSelectionBoxHeight:double` `PlaceholderText:string?` `PlaceholderForeground:IBrush?` `Watermark:string?` `InnerLeftContent` `InnerRightContent` `PopupInnerTopContent` `PopupInnerBottomContent` (`object?`); part `PART_BackgroundBorder`; classes `:dropdownopen` `:selection-empty`
- `EnumSelector`: `EnumType:Type?` `Value:object?` `DisplayDescription:bool` `EnumValues:IList?` `SelectedValue:EnumItemTuple?` (direct) `Values:IList<EnumItemTuple>?` (direct)
- `TagInput`: `Tags:IList<string>?` `MaxCount:int` (`int.MaxValue`) `Separator:string` `AllowDuplicates:bool` (`true`) `LostFocusBehavior:LostFocusBehavior` `ItemTemplate:IDataTemplate?` `AcceptsReturn:bool` `PlaceholderText`/`Watermark:string?` (one property, two names) `PlaceholderForeground:IBrush?` `InnerLeftContent`/`InnerRightContent` (`object?`)
- `PathPicker.FileFilter` takes a BRACKETED grammar, not a pattern list: `[Name,*.a,*.b]` groups concatenate, and the bare names `All` `Pdf` `ImageAll` `ImageJpg` `ImagePng` `ImageWebp` `TextPlain` resolve to the shipped `FilePickerFileTypes` rows. The whole string is regex-validated at picker launch and a malformed one THROWS `ArgumentException` from the click handler, so a caller encodes from typed rows and refuses an unencodable label before the control mounts; a group name carrying `*`, `.`, or `,` breaks the first alternation.

[NUMERIC_PROPERTIES]: `NumericUpDown` is the abstract non-generic spinner base and `NumericUpDownBase<T>` the typed one; every concrete row overrides `StyleKeyOverride` to `NumericUpDown`, so the eleven typed spinners share one control theme

- `NumericUpDown` (abstract): `AllowDrag:bool` `IsReadOnly:bool` `HorizontalContentAlignment:HorizontalAlignment` `InnerLeftContent`/`InnerRightContent` (`object?`) `PlaceholderText`/`Watermark:string?` `PlaceholderForeground:IBrush?` `NumberFormat:NumberFormatInfo?` `FormatString:string` `ParsingNumberStyle:NumberStyles` `TextConverter:IValueConverter?` `AllowSpin:bool` `ShowButtonSpinner:bool`; parts `PART_Spinner` `PART_TextBox` `PART_DragPanel`; event `Spinned` (`SpinEventArgs`)
- `NumericUpDownBase<T> : NumericUpDown where T : struct, IComparable<T>`: `Value:T?` `Minimum:T` `Maximum:T` `Step:T` `EmptyInputValue:T?` `Command:ICommand?` `CommandParameter:object?`; event `ValueChanged` (`ValueChangedEventArgs<T>`) — the increment slot is `Step`, and every property is registered PER CLOSED GENERIC, so `NumericUpDownBase<int>.ValueProperty` and `NumericUpDownBase<double>.ValueProperty` are distinct identities no type probe recovers
- `NumericDoubleUpDown`/`NumericFloatUpDown` default `Minimum`/`Maximum` to the type extrema and `Step` to one, and their `CoerceCurrentValue` override returns `NaN` unchanged; every other row clamps through the base coercion
- `TextConverter` REPLACES the whole text leg when set: `ConvertTextToValue` calls `Convert(text, typeof(T?), null, CultureInfo.CurrentCulture)` and hard-casts the answer to `T?`, so the converter must return a boxed value of the spinner's exact closed generic and must ignore the supplied culture (the control passes ambient culture unconditionally); `ConvertValueToText` calls `ConvertBack(Value, typeof(int), null, CultureInfo.CurrentCulture)` with `typeof(int)` regardless of `T`, so the back leg formats from the value and never from the target type. Absent a converter the text leg runs `TrimString` under `ParsingNumberStyle` into the abstract `protected ParseText(string?, out T)` each concrete row implements, and formatting runs `FormatString` through `NumberFormat`

[PICKER_PROPERTIES]: the picker bases carry blackout ranges instead of a minimum/maximum pair, so bounds are expressed as unreachable spans

- `DatePickerBase`: `DisplayFormat:string?` (`yyyy-MM-dd`) `BlackoutDates:AvaloniaList<DateRange>` `BlackoutDateRule:IDateSelector?` `FirstDayOfWeek:DayOfWeek` `IsTodayHighlighted:bool` `IsDropdownOpen:bool` `IsReadOnly:bool` `NeedConfirmation:bool` `InnerLeftContent`/`InnerRightContent`/`PopupInnerTopContent`/`PopupInnerBottomContent` (`object?`) `PlaceholderText`/`Watermark:string?` `PlaceholderForeground:IBrush?`; parts `PART_Popup` `PART_TextBox` `PART_Calendar`
- `TimePickerBase`: `DisplayFormat:string?` (`HH:mm:ss`) `PanelFormat:string` (`HH mm ss`) plus the same confirmation, dropdown, read-only, inner-content, and placeholder set — and NO blackout or bound surface at all
- typed value slots: `DatePickerBase<T>.SelectedDate:T?`, `TimePickerBase<T>.SelectedTime:T?`, `DateTimePickerBase<T>.SelectedDate:T?`, `DateRangePickerBase<T>.SelectedStartDate:T?` + `SelectedEndDate:T?` — every one two-way by default and registered per closed generic
- concrete bindings: `DatePicker : DatePickerBase<DateTime>`, `DateOnlyPicker : DatePickerBase<DateOnly>`, `DateOffsetPicker : DatePickerBase<DateTimeOffset>`, `TimePicker : TimePickerBase<TimeSpan>`, `TimeOnlyPicker : TimePickerBase<TimeOnly>`, `DateTimePicker : DateTimePickerBase<DateTime>`, `DateRangePicker : DateRangePickerBase<DateTime>`, `DateOnlyRangePicker : DateRangePickerBase<DateOnly>`, `TimeRangePicker : TimeRangePickerBase<TimeSpan>`
- `DateRange(DateTime day)` collapses to a single day and `DateRange(DateTime start, DateTime end)` normalizes an inverted pair to the start day; `Start` and `End` are date-truncated `DateTime` with private setters

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
- `MarkdownLine`: `MarkdownLineCodeBackground` fills the inline-code run; the surrounding text resolves the `Semi.Avalonia` `TextBlock{FontSize,SelectionBackground,SelectionForeground}` seats
- `SelectionList`: `SelectionListIndicatorBackground` fills the sliding indicator
- `TreeLevelToPaddingConverter : MarkupMultiValueConverter` takes `[int level, Thickness unit]` and returns `unit` with its LEFT edge multiplied by the clamped level, so a flat indented row binds its padding through one `MultiBinding` over the row's depth and a `{DynamicResource}` unit thickness; a non-matching value pair yields the default `Thickness`
- `Divider`: `DividerBorderBrush` rules the line, `DividerLeftLine` and `DividerRightLine` theme the two flanks
- `Form`: `FormAsteriskForeground` marks required, `FormGroupForeground` heads a group
- `Loading`: `LoadingIconForeground` `LoadingMaskBackground`
- named part themes: `ButtonPathPicker` `ListPathPicker` `TinyPagination` `PrimaryScrollToButton` `CloseButton` `OverlayCloseButton` `TagInputTextBoxTheme` `ToolBarExpandToggleButton` `DefaultNavMenuItemTemplate`
- Every Ursa brush resolves from the shared `SemiColor*` palette slots (`SemiColorPrimary`, `SemiColorDanger`, `SemiColorText0`, `SemiColorFill1`, …) and the `Semi*` ramp colors, so a palette override in `Application.Resources` re-tints every Ursa control without touching a control theme.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every overlay — dialog, drawer, confirm — is raised vm-first through the static `OverlayDialog`/`OverlayDrawer`/`MessageBox.ShowOverlayAsync` dispatch against a registered host id; one `OverlayDialogHost` per shell region carries its `HostId` before it attaches, the host registers and unregisters itself, and no code instantiates a popup or mutates the visual tree.
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
- Reject: binding the obsolete `Drawer` forwarder where `OverlayDrawer` is the owner; dispatching against a host id whose registration the caller has not proved, since an unregistered id is indistinguishable from a cancel; mutating a live toast payload and expecting the presented card to follow; hand-rolling a nav menu, timeline, OTP field, masked IP/time box, multi-select combo, segmented selector, or rating Ursa owns; a second overlay-host layer or per-screen feedback stack; blocking on `.Result` instead of awaiting the dispatch; declaring a parallel placement or severity enum where `Ursa.Common` or `NotificationType` names the axis; restyling a control theme wholesale where a `SemiColor*` palette override re-tints it; loading `UrsaSemiTheme` without or ahead of `SemiTheme`; deriving shell views from the non-reactive bases when the ReactiveUI bridge is admitted; re-pinning `Irihi.Avalonia.Shared` divergent from the Semi closure
