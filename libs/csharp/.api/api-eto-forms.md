# [RASM_API_ETO_FORMS]

`Eto.Forms` is the host-neutral widget, layout, window, and command construction spine both Rhino host boundaries cross: one host-loaded `Eto.dll` resolves every control through the ambient platform handler, four orthogonal layout owners place them, the window and dialog hierarchy presents them, and one `Command` drives every chrome projection of an action. One construction row produces a native control on each host, and host divergence lives in the handler, never in the row. This branch catalogue owns the spine; each host-boundary folder registers it and tables only the widgets and seams its own boundary adds.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto` widget substrate
- host: Rhino host runtime, in-process; the same `Eto.dll` every boundary binds, never a second NuGet admission (BSD-3-Clause)
- assembly: `Eto` (`Eto.dll`) from the RhinoWIP `RhCore.framework` bundle
- namespace: `Eto.Forms`, `Eto` (widget base, `Padding`, `Size`)
- rail: native-ui

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: control base and the inherited event surface

`Control` is the common base carrying invalidation, native attachment, and the event families every widget inherits.

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :-------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Control` | control base  | lifecycle, focus, size, mouse, key, and drag event owner |

[CONTROL_STATE]: `Enabled` `Visible` `Bounds` `Cursor` `ContextMenu` `ToolTip` `ParentWindow`
[POINTER_EVENTS] (`EventHandler<MouseEventArgs>`): `MouseDown` `MouseUp` `MouseMove` `MouseEnter` `MouseLeave` `MouseDoubleClick` `MouseWheel`
[KEY_EVENTS]: `KeyDown` `KeyUp` (`EventHandler<KeyEventArgs>`); `TextInput` (`EventHandler<TextInputEventArgs>`)
[DRAG_EVENTS] (`EventHandler<DragEventArgs>`): `DragEnter` `DragOver` `DragLeave` `DragDrop` `DragEnd`
[LIFECYCLE_EVENTS] (`EventHandler`): `GotFocus` `LostFocus` `SizeChanged` `Load` `Shown`

[PUBLIC_TYPE_SCOPE]: text and value input

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :--------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `TextBox`        | text input    | single-line editable text                             |
|  [02]   | `TextArea`       | text input    | multi-line editable text with wrap and caret control  |
|  [03]   | `RichTextArea`   | text input    | styled rich-text editor over a formatted buffer       |
|  [04]   | `PasswordBox`    | text input    | masked secret entry                                   |
|  [05]   | `SearchBox`      | text input    | search-styled field with clear affordance             |
|  [06]   | `NumericStepper` | value input   | bounded numeric entry with increment stepping         |
|  [07]   | `TextStepper`    | text input    | `TextBox` carrying a stepper affordance and its raise |
|  [08]   | `Slider`         | value input   | ranged track selector                                 |
|  [09]   | `DateTimePicker` | value input   | date and time selection with a min/max range          |
|  [10]   | `Calendar`       | value input   | month-grid date or date-range selection               |
|  [11]   | `ColorPicker`    | value input   | inline colour swatch and picker field                 |
|  [12]   | `FontPicker`     | value input   | inline font-selection field                           |
|  [13]   | `FilePicker`     | value input   | inline file or folder path field                      |
|  [14]   | `Spinner`        | indicator     | indeterminate activity spinner                        |

[FIELD_STATE]:
- `TextBox`: `PlaceholderText` `MaxLength` `ReadOnly` `CaretIndex` `SelectedText` `Selection` `SelectAll()` `TextChanging`
- `TextArea`: `AcceptsReturn` `AcceptsTab` `SpellCheck` `Wrap` `CaretIndex` `Selection` `CaretIndexChanged` `SelectionChanged`
- `NumericStepper`: `MinValue` `MaxValue` `Increment` `Value` `DecimalPlaces` `FormatString` `Wrap`
- `Slider`: `MinValue` `MaxValue` `Value` `Orientation` `TickFrequency` `SnapToTick` `ValueChanged`
- `TextStepper`: inherits every `TextBox` member and adds `ValidDirection` (`StepperValidDirections`) `ShowStepper` `Step` (`EventHandler<StepperEventArgs>`)
- `DateTimePicker`: `Value` (`DateTime?`) `MinDate` `MaxDate` (both non-nullable) `Mode` `TextColor` `ShowBorder` `ValueChanged`
- `Calendar`: `SelectedDate` (`DateTime`) `SelectedRange` (`Range<DateTime>`) `MinDate` `MaxDate` `Mode` `SelectedDateChanged` `SelectedRangeChanged`
- `ColorPicker`: `Value` `AllowAlpha` `SupportsAllowAlpha` `ColorChanged`
- `FilePicker`: `Filters` owns the mutable file-filter collection

[MODE_ROSTERS]:
- `DateTimePickerMode` is `[Flags]`: `Date = 1` `Time = 2` `DateTime = 3` — the composite is a declared member, never a caller's own OR
- `CalendarMode`: `Single` `Range`
- `SegmentedSelectionMode`: `None` `Single` `Multiple`
- `FileAction` (namespace `Eto`, NOT `Eto.Forms`): `OpenFile` `SaveFile` `SelectFolder`

[PUBLIC_TYPE_SCOPE]: choice, command, and display

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                  |
| :-----: | :-------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `CheckBox`            | choice        | tri-state boolean toggle                      |
|  [02]   | `RadioButton`         | choice        | mutually-exclusive selection within a group   |
|  [03]   | `DropDown`            | choice        | single-selection collapsed list               |
|  [04]   | `ComboBox`            | choice        | editable-text dropdown                        |
|  [05]   | `ListControl`         | choice base   | list base carrying store and selection        |
|  [06]   | `ListBox`             | choice        | scrollable single-selection list              |
|  [07]   | `CheckBoxList`        | choice        | multi-check option group                      |
|  [08]   | `SegmentedButton`     | choice        | linked multi-segment toggle bar               |
|  [09]   | `SegmentedItem`       | choice part   | one segment base carrying text and command    |
|  [10]   | `ButtonSegmentedItem` | choice part   | push-shaped segment, optionally command-bound |
|  [11]   | `Button`              | command       | push-command control                          |
|  [12]   | `ToggleButton`        | command       | push command carrying pressed state           |
|  [13]   | `LinkButton`          | command       | hyperlink-styled command                      |
|  [14]   | `Label`               | display       | static or wrapping text label                 |
|  [15]   | `ImageView`           | display       | static image presenter                        |
|  [16]   | `ProgressBar`         | indicator     | determinate or indeterminate progress track   |

[CHOICE_STATE]:
- `CheckBox`: `Checked` (`bool?`) `ThreeState` `CheckedChanged`
- `DropDown`: `ShowBorder` `ItemImageBinding` `DropDownOpening` `DropDownClosed` `FormatItem`
- `ListControl`: `DataStore` `SelectedIndex` `SelectedValue` `SelectedKey` `SelectedIndexChanged` `SelectedValueChanged` — `SelectedKey` is SINGULAR here; the plural lives on `CheckBoxList`
- `CheckBoxList` (a `Panel`, not a `ListControl`): `SelectedKeys` (`IEnumerable<string>`) `SelectedValues` (`IEnumerable<object>`) `Orientation` `SelectedValuesChanged` `SelectedKeysChanged`
- `SegmentedButton`: `Items` `SelectionMode` `SelectedIndex` `SelectedIndexes` (`IEnumerable<int>`) `SelectedIndexesChanged`; `SegmentedItem`: `Text` `Command`
- `ProgressBar`: `MinValue` `MaxValue` `Value` `Indeterminate`

[PUBLIC_TYPE_SCOPE]: containers and host surfaces

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------------- |
|  [01]   | `Panel`             | container     | single-child content host and panel-subclass base                  |
|  [02]   | `GroupBox`          | container     | titled bordered frame around child content                         |
|  [03]   | `Expander`          | container     | collapsible header and content region                              |
|  [04]   | `Scrollable`        | container     | scrolling viewport with border and expand flags                    |
|  [05]   | `Splitter`          | container     | two-pane draggable split under a `SplitterFixedPanel` policy       |
|  [06]   | `TabControl`        | container     | tabbed page host                                                   |
|  [07]   | `TabPage`           | container     | one tab with text, image, and child content                        |
|  [08]   | `Drawable`          | container     | owner-drawn surface issuing `Graphics` (`.api/api-eto-drawing.md`) |
|  [09]   | `WebView`           | container     | embedded browser: navigation, script execution, title              |
|  [10]   | `PropertyGrid`      | container     | reflected property editor over a bound object graph                |
|  [11]   | `NativeControlHost` | container     | host-native view embedding seam (`.api/api-eto-platform.md`)       |

[CONTAINER_STATE]:
- `Splitter`: `Panel1` `Panel2` `Orientation` `Position` `FixedPanel` `SplitterWidth`
- `TabControl`: `Pages` `SelectedIndex` `SelectedPage` `TabPosition` `SelectedIndexChanged`
- `Expander`: `Expanded` `ExpandedChanged`; `PropertyGrid`: `SelectedObject` `SelectedObjects` `ShowCategories` `ShowDescription`
- `RichTextArea`: `SelectionFont` `SelectionForeground` `SelectionBackground` `SelectionBold` `SelectionItalic` `SelectionUnderline` `SelectionStrikethrough` `Buffer` `Rtf`

[PUBLIC_TYPE_SCOPE]: grid, tree, and cell families

`GridView` binds a flat store, `TreeGridView` an `ITreeGridStore<ITreeGridItem>` hierarchy; a `GridColumn` carries one cell, and the cell kind selects the in-cell editor.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :-------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `Grid`          | grid base     | column, row, selection, edit, and format-event base         |
|  [02]   | `GridView`      | grid          | flat data grid over an ordered store                        |
|  [03]   | `TreeGridView`  | grid          | hierarchical grid over `ITreeGridItem` nodes                |
|  [04]   | `GridColumn`    | grid part     | one bound column carrying a cell                            |
|  [05]   | `GridItem`      | grid part     | mutable flat row backing-store item                         |
|  [06]   | `TreeGridItem`  | grid part     | mutable tree node with children, implements `ITreeGridItem` |
|  [07]   | `ITreeGridItem` | contract      | tree-grid node contract: parent, expanded, children         |
|  [08]   | `TextBoxCell`   | cell          | inline editable-text cell                                   |
|  [09]   | `CheckBoxCell`  | cell          | inline boolean cell                                         |
|  [10]   | `ComboBoxCell`  | cell          | inline dropdown cell                                        |
|  [11]   | `ImageViewCell` | cell          | inline image cell                                           |
|  [12]   | `ImageTextCell` | cell          | combined image and text cell                                |
|  [13]   | `ProgressCell`  | cell          | inline progress-bar cell                                    |
|  [14]   | `DrawableCell`  | cell          | owner-drawn cell issuing `Graphics`                         |
|  [15]   | `CustomCell`    | cell          | control-hosting cell over an arbitrary child                |

[GRID_STATE]:
- `Grid`: `Columns` `ShowHeader` `AllowColumnReordering` `AllowMultipleSelection` `RowHeight` `GridLines` `SelectedRows` `SelectedItem` `SelectedItems` `IsEditing`
- `Grid` events: `CellEditing` `CellEdited` `CellClick` `CellFormatting` `RowFormatting`
- `GridColumn`: `HeaderText` `DataCell` `Editable` `Resizable` `Sortable` `AutoSize` `Visible` `Width`
- `TreeGridView`: `DataStore` (`ITreeGridStore<ITreeGridItem>`) `SelectedItem` (`ITreeGridItem`); the expand, collapse, and activate event family
- `CellEventArgs`: `Row` `Item` `CellState` `IsEditing` `IsSelected` `CellTextColor`, raising `INotifyPropertyChanged` as the state moves
- `CellPaintEventArgs` (a `PaintEventArgs`): `Graphics` `ClipRectangle` `CellState` `Item` `IsEditing` `IsSelected` — the LIVE owner-draw payload
- `DrawableCellPaintEventArgs` is `[Obsolete]` since 2.2 and survives only on `DrawableCell.Paint`; the non-obsolete route is overriding `DrawableCell.OnPaint(CellPaintEventArgs)`

[PUBLIC_TYPE_SCOPE]: layout owners

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :---------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `DynamicLayout`   | layout        | nested vertical, horizontal, group, and scrollable regions |
|  [02]   | `TableLayout`     | layout        | scaling cell grid over `TableRow`/`TableCell`              |
|  [03]   | `TableRow`        | layout part   | one grid row of cells with scale flags                     |
|  [04]   | `TableCell`       | layout part   | one grid cell with an x-scale flag (`ScaleWidth`)          |
|  [05]   | `StackLayout`     | layout        | linear run over `StackLayoutItem` with alignment           |
|  [06]   | `StackLayoutItem` | layout part   | one stacked child with an expand flag (`Expand`)           |
|  [07]   | `PixelLayout`     | layout        | absolute pixel-positioned placement                        |
|  [08]   | `Padding` (`Eto`) | value         | four-edge inset                                            |
|  [09]   | `Size` (`Eto`)    | value         | integer extent                                             |

[LAYOUT_VOCABULARY]: `Orientation` `HorizontalAlignment` `VerticalAlignment` `DockPosition` `SplitterFixedPanel`
[LAYOUT_STATE]: `DynamicLayout.Padding`/`Spacing`/`DefaultPadding`/`DefaultSpacing`; `TableLayout.Rows`/`Dimensions`/`SetCellSize`; `PixelLayout.Controls`; `StackLayout.Items`/`Orientation`/`Spacing`/`HorizontalContentAlignment`/`VerticalContentAlignment`

[PUBLIC_TYPE_SCOPE]: windows, dialogs, and native choosers

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :------------------- | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `Window`             | window base   | top-level window base with state, location, and logical-pixel-size events |
|  [02]   | `Form`               | window        | modeless top-level window                                                 |
|  [03]   | `FloatingForm`       | window        | always-on-top utility form                                                |
|  [04]   | `Dialog`             | window        | modal window                                                              |
|  [05]   | `Dialog<T>`          | window        | modal returning a typed result through `Close(T)`                         |
|  [06]   | `MessageBox`         | dialog        | text, type, and buttons prompt overload family                            |
|  [07]   | `CommonDialog`       | dialog base   | owner-parented chooser base                                               |
|  [08]   | `FileDialog`         | dialog base   | file-chooser base carrying `Filters`                                      |
|  [09]   | `OpenFileDialog`     | dialog        | file-open chooser                                                         |
|  [10]   | `SaveFileDialog`     | dialog        | file-save chooser                                                         |
|  [11]   | `SelectFolderDialog` | dialog        | folder chooser                                                            |
|  [12]   | `ColorDialog`        | dialog        | native colour chooser                                                     |
|  [13]   | `FontDialog`         | dialog        | native font chooser                                                       |
|  [14]   | `FileFilter`         | value         | file-extension filter row                                                 |

[WINDOW_VOCABULARY]: `DialogResult` `WindowState` `WindowStyle` `DialogDisplayMode` `MessageBoxType` `MessageBoxButtons` `MessageBoxDefaultButton`
[WINDOW_STATE]:
- `Window`: `Title` `Location` `Bounds` `Opacity` `Resizable` `Topmost` `WindowState` `WindowStyle` `Icon` `LogicalPixelSize`
- `Window` events: `Closing` `Closed` `WindowStateChanged` `LogicalPixelSizeChanged`; `Form`: `ShowActivated`
- `Dialog`: `DisplayMode` `DefaultButton` `AbortButton` `PositiveButtons` `NegativeButtons`
- `FileDialog`: `Directory` `FileName` `Title` `CheckFileExists` `Filters` `CurrentFilter` `CurrentFilterIndex`; `OpenFileDialog`: `MultiSelect` `Filenames`
- `ColorDialog`: `Color` `AllowAlpha` `ColorChanged`; `FontDialog`: `Font` `FontChanged`; `SelectFolderDialog`: `Directory` `Title`

[PUBLIC_TYPE_SCOPE]: menus and commands

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------- |
|  [01]   | `ContextMenu`       | menu          | popup menu bound to a control                                |
|  [02]   | `MenuItem`          | menu item     | leaf menu entry                                              |
|  [03]   | `ButtonMenuItem`    | menu item     | invoking menu entry carrying submenu children                |
|  [04]   | `SubMenuItem`       | menu item     | nested submenu container                                     |
|  [05]   | `Command`           | command       | shared invocation with `Enabled`, `Shortcut`, and `Executed` |
|  [06]   | `CheckCommand`      | command       | toggling command                                             |
|  [07]   | `RadioCommand`      | command       | radio-grouped command                                        |
|  [08]   | `MenuBar`           | menu          | top-level application menu (`Interaction/chrome` `MenuOf`)   |
|  [09]   | `CheckMenuItem`     | menu item     | checkable menu entry                                         |
|  [10]   | `RadioMenuItem`     | menu item     | radio-grouped menu entry                                     |
|  [11]   | `SeparatorMenuItem` | menu item     | menu divider                                                 |
|  [12]   | `ToolBar`           | toolbar       | control toolbar over `ToolItem` entries (`chrome` `BarOf`)   |
|  [13]   | `ButtonToolItem`    | tool item     | invoking toolbar button                                      |
|  [14]   | `CheckToolItem`     | tool item     | toggle toolbar button                                        |
|  [15]   | `DropDownToolItem`  | tool item     | toolbar button carrying a dropdown menu                      |
|  [16]   | `SeparatorToolItem` | tool item     | toolbar divider                                              |

[COMMAND_STATE]: `Command.ID`/`MenuText`/`ToolBarText`/`ToolTip`/`Enabled`/`Shortcut`/`Executed`; `Command.CreateMenuItem()`/`CreateToolItem()` project ONE command into each chrome; `CheckCommand.Checked`/`CheckedChanged`; `RadioCommand.Controller`; `ContextMenu.Items`/`Trim`/`Opening`/`Closing`/`Closed`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: control lifecycle, input, and drag

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :----------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `Control.Focus()`                                | instance | focus request                       |
|  [02]   | `Control.Invalidate()` / `Invalidate(Rectangle)` | instance | full or bounded repaint request     |
|  [03]   | `Control.UpdateLayout()`                         | instance | re-measure request                  |
|  [04]   | `Control.CaptureMouse() -> bool`                 | instance | begin pointer capture               |
|  [05]   | `Control.ReleaseMouseCapture()`                  | instance | end pointer capture                 |
|  [06]   | `Control.DoDragDrop(DataObject, DragEffects, …)` | instance | start a drag with an optional image |

[ENTRYPOINT_SCOPE]: grid selection, edit, and reload

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Grid.SelectRow(int)` / `UnselectRow(int)`                  | instance | select or unselect one row                |
|  [02]   | `Grid.SelectAll()` / `UnselectAll()`                        | instance | select or unselect every row              |
|  [03]   | `Grid.BeginEdit(int, int)`                                  | instance | begin inline editing                      |
|  [04]   | `Grid.CommitEdit() -> bool`                                 | instance | commit inline editing                     |
|  [05]   | `Grid.CancelEdit() -> bool`                                 | instance | cancel inline editing                     |
|  [06]   | `Grid.ScrollToRow(int)`                                     | instance | bring a row into view                     |
|  [07]   | `TreeGridView.ReloadData()`                                 | instance | refresh, keeping selection                |
|  [08]   | `TreeGridView.ReloadItem(ITreeGridItem, bool)`              | instance | refresh one subtree                       |
|  [09]   | `GridItem(params object[])` / `GridItem.Tag`                | ctor     | construct a row and retain typed identity |
|  [10]   | `TreeGridItem(IEnumerable<ITreeGridItem>, params object[])` | ctor     | construct one hierarchical row            |

[ENTRYPOINT_SCOPE]: layout composition

`Begin*` builders open a nested region; `Add*` members place children into the open region.

| [INDEX] | [SURFACE]                                                                  | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `DynamicLayout.BeginVertical(Padding?, Size?, bool?, bool?)`               | instance | open a vertical region            |
|  [02]   | `DynamicLayout.BeginHorizontal(bool?)`                                     | instance | open a horizontal region          |
|  [03]   | `DynamicLayout.BeginGroup(string, Padding?, Size?, bool?, bool?)`          | instance | open a titled region              |
|  [04]   | `DynamicLayout.BeginScrollable(BorderType, Padding?, Size?, bool?, bool?)` | instance | open a scrollable region          |
|  [05]   | `DynamicLayout.EndVertical()` / `EndHorizontal()`                          | instance | close the open region             |
|  [06]   | `DynamicLayout.Add(Control, bool?, bool?)`                                 | instance | place one child                   |
|  [07]   | `DynamicLayout.AddRow(params Control[])` / `AddColumn(params Control[])`   | instance | place a row or column             |
|  [08]   | `DynamicLayout.AddSeparateRow(params Control[])`                           | instance | place a separated row             |
|  [09]   | `DynamicLayout.AddCentered(Control, Padding?, Size?, …)`                   | instance | place a centred child             |
|  [10]   | `DynamicLayout.AddAutoSized(Control, Padding?, Size?, …)`                  | instance | place a natural-size child        |
|  [11]   | `TableLayout.Horizontal(params TableCell[])`                               | static   | build a single horizontal row     |
|  [12]   | `TableLayout.HorizontalScaled(params TableCell[])`                         | static   | build an evenly-scaled row        |
|  [13]   | `TableLayout.AutoSized(Control, Padding?, bool)`                           | static   | wrap one control at natural size  |
|  [14]   | `TableLayout.Add(Control, int, int, bool, bool)`                           | instance | place a control at a grid cell    |
|  [15]   | `TableLayout.Move(Control, int, int)`                                      | instance | move a placed control             |
|  [16]   | `TableLayout.SetColumnScale(int, bool)` / `SetRowScale(int, bool)`         | instance | mark a column or row scaling      |
|  [17]   | `PixelLayout.Add(Control, int, int)` / `Move(Control, int, int)`           | instance | absolute placement and movement   |
|  [18]   | `PixelLayout.GetLocation(Control) -> Point`                                | instance | location lookup                   |
|  [19]   | `StackLayoutItem(Control, bool)`                                           | ctor     | place one linear-layout child     |
|  [20]   | `TableRow(IEnumerable<TableCell>)` / `TableCell(Control, bool)`            | ctor     | construct a placement row or cell |

[ENTRYPOINT_SCOPE]: window, dialog, and chooser presentation

`Dialog<T>.Result` carries the typed outcome from either modal presentation surface.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `Form.Show()`                                            | instance | modeless window               |
|  [02]   | `Dialog.ShowModal(Control)`                              | instance | modal loop                    |
|  [03]   | `Dialog.ShowModalAsync(Control) -> Task`                 | instance | asynchronous modal loop       |
|  [04]   | `Dialog<T>.ShowModal(Control) -> T`                      | instance | modal blocking for a result   |
|  [05]   | `Dialog<T>.ShowModalAsync(Control) -> Task<T>`           | instance | awaited typed modal           |
|  [06]   | `Dialog<T>.Close(T)`                                     | instance | close and set the result      |
|  [07]   | `MessageBox.Show(Control, string, …) -> DialogResult`    | static   | prompt overload family        |
|  [08]   | `CommonDialog.ShowDialog(Control) -> DialogResult`       | instance | show against an owning window |
|  [09]   | `FileDialog.ShowDialog(Control) -> DialogResult`         | instance | file open or save chooser     |
|  [10]   | `SelectFolderDialog.ShowDialog(Control) -> DialogResult` | instance | folder chooser                |
|  [11]   | `ColorDialog.ShowDialog(Control) -> DialogResult`        | instance | native colour chooser         |
|  [12]   | `FontDialog.ShowDialog(Control) -> DialogResult`         | instance | native font chooser           |
|  [13]   | `Window.Close()`                                         | instance | window teardown               |
|  [14]   | `Window.BringToFront()`                                  | instance | z-order promotion             |
|  [15]   | `Window.SetOwner(Window)`                                | instance | owner assignment              |
|  [16]   | `Window.FromPoint(PointF) -> Window`                     | static   | window lookup                 |

[ENTRYPOINT_SCOPE]: menu and command dispatch

| [INDEX] | [SURFACE]                           | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :---------------------------------- | :------- | :------------------------------- |
|  [01]   | `ContextMenu.Show(Control, PointF)` | instance | popup at a control point         |
|  [02]   | `Command.Execute()`                 | instance | raise the shared execution event |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every widget derives from `Control` and resolves a backend handler through the ambient platform (`.api/api-eto-platform.md`); one construction row produces a native control on each host, and host divergence lives in the handler.
- One control owns one concern — a field its typed value and `*Binding`, a data view its store and selection, a container its children and split, tab, or expand state.
- Layout owns four orthogonal placement strategies — `DynamicLayout` nested regions, `TableLayout` grid cells, `StackLayout` one-axis run, `PixelLayout` absolute — composed per screen, never merged.
- `Grid` is the shared base of `GridView` and `TreeGridView`: columns, cell renderers, selection, edit, and format events inherit from it, and `TreeGridView` adds the expand, collapse, and tree-drag surface; one column definition drives every row.
- One `Command` projects into every chrome an action reaches, so a single enablement and shortcut definition drives the popup menu and each boundary's own menu and toolbar projection.
- Closed enum vocabularies discriminate every layout and window construction switch.

[STACKING]:
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): a `[SmartEnum]` owns the closed control-kind, cell-kind, layout-strategy, and dialog-outcome vocabularies a generator-shaped UI layer folds to rows, and a `[Union]` owns the discriminated screen-element tree, so the generated `Switch`/`Map` drives construction dispatch instead of a hand-written control-type ladder; a bounded field value is a `[ValueObject<T>]` the control `*Binding` reads and writes.
- `LanguageExt.Core`(`.api/api-languageext.md`): `Fin<A>` rails `ShowModal`/`ShowModalAsync` outcomes and chooser results, cancellation a `Fail` rather than a null sentinel; `Option<A>` carries the nullable `bool?` scale flags and every selection read; `Eff<A>` wraps `DoDragDrop` and native-attach effects; `Seq<A>` is the child-collection carrier a layout region folds over; independent field reads lift to `Validation`, fan in through the tuple apply, and exit `.ToFin()` before a panel commits its edit.
- `api-eto-drawing`(`.api/api-eto-drawing.md`): `Drawable.Paint` and `DrawableCell` hand `PaintEventArgs.Graphics` to the paint surface for owner-drawn content.
- `api-eto-runtime`(`.api/api-eto-runtime.md`): dialog presentation, control invalidation, and every cross-thread mutation marshal through `Application.Instance`.
- `Wacton.Unicolour`(`.api/api-unicolour.md`): the canonical colour value behind `ColorPicker` and `ColorDialog`; the paint-edge `Color` maps to and from `Unicolour`, keeping theme ramps and perceptual selection in the perceptual model.

[LOCAL_ADMISSION]:
- A hosted surface is an `Eto.Forms` composition: a `Panel`/`Scrollable` root holds one layout owner, and the layout holds the field, data-view, and container roster; a new control capability lands as a subclass or a composition of the admitted roster, never a wrapper renaming a host member or a re-implemented native widget.
- A screen is built once from element rows against these construction, layout, and presentation surfaces; `Eto.Forms.*` types stay behind the owning boundary and downstream code composes screen definitions rather than raw widget calls.
- Boundary faults lower onto the LanguageExt rail.

[RAIL_LAW]:
- Package: `Eto`
- Owns: the native widget roster, the cell, item, and grid families, the four layout owners, the window, dialog, and chooser hierarchy, the popup-menu and command surface, and the `Control` event families every widget inherits
- Accept: panel chrome, form fields, grid and list data views, modal and modeless presentation, native file, colour, and font choosers, command dispatch
- Reject: immediate 2D painting (`.api/api-eto-drawing.md`), platform-handler selection and native hosting (`.api/api-eto-platform.md`), UI-thread dispatch and ambient runtime state (`.api/api-eto-runtime.md`), a local wrapper renaming a host member, an exception-style fault path beside the LanguageExt rail, and a folder partition re-tabling this spine at member depth
