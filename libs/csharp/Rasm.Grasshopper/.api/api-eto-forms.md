# [RASM_GRASSHOPPER_API_ETO_FORMS]

`Eto.Forms` mints the native widget, layout, and window-chrome surface a GH2-hosted panel raises inside the Rhino process. Construction is direct composition — a panel subclasses a control or stacks the roster against one layout owner, never a wrapper renaming a host member.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto`
- package: `Eto` (the cross-platform Eto.Forms UI framework, host-provided by RhinoWIP)
- license: BSD-3-Clause
- assembly: `Eto` (`Eto.dll`)
- namespace: `Eto.Forms`, `Eto` (widget base, `Padding`, `Size`)
- asset: host-provided — RhinoWIP ships `Eto.dll` under `RhCore.framework/Versions/A/Resources`; no NuGet admission
- rail: native UI

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: control base and the panel-field roster

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Control`              | control base  | lifecycle, focus, size, mouse, key, and drag event owner |
|  [02]   | `TextBox`              | control       | single-line text                                         |
|  [03]   | `TextArea`             | control       | multi-line text                                          |
|  [04]   | `NumericStepper`       | control       | bounded number field                                     |
|  [05]   | `CheckBox`             | control       | tri-state boolean                                        |
|  [06]   | `Slider`               | control       | ranged integer track                                     |
|  [07]   | `DropDown`             | control       | single-choice list                                       |
|  [08]   | `ComboBox`             | control       | editable single-choice text/list field                   |
|  [09]   | `ColorPicker`          | control       | inline colour swatch and picker field                    |
|  [10]   | `DateTimePicker`       | control       | date and/or time field with min/max range                |
|  [11]   | `Label`                | control       | static text                                              |
|  [12]   | `LinkButton`           | control       | click-raising inline hyperlink                           |
|  [13]   | `Button`               | control       | command trigger                                          |
|  [14]   | `ToggleButton`         | control       | command trigger with pressed state                       |
|  [15]   | `RadioButton`          | control       | mutually-exclusive option                                |
|  [16]   | `RadioButtonList`      | control       | mutually-exclusive option group                          |
|  [17]   | `CheckBoxList`         | control       | multi-select option group                                |
|  [18]   | `PasswordBox`          | control       | masked-entry text field                                  |
|  [19]   | `SearchBox`            | control       | search-styled text field                                 |
|  [20]   | `Stepper`              | control       | up/down increment field                                  |
|  [21]   | `TextStepper`          | control       | up/down increment text field                             |
|  [22]   | `NumericUpDown`        | control       | up/down increment numeric field                          |
|  [23]   | `Spinner`              | control       | busy-indicator field                                     |
|  [24]   | `FilePicker`           | control       | inline file-path field                                   |
|  [25]   | `FontPicker`           | control       | inline font-selection field                              |
|  [26]   | `MaskedTextBox<T>`     | control       | format-masked text over a typed provider                 |
|  [27]   | `MaskedTextStepper<T>` | control       | format-masked stepper over a typed provider              |

[PUBLIC_TYPE_SCOPE]: data views — grid, tree, list, property grid

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :--------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `Grid`                 | control base  | column/row view base                                |
|  [02]   | `GridView`             | control       | list-backed grid over an `IEnumerable` store        |
|  [03]   | `TreeGridView`         | control       | multi-column tree                                   |
|  [04]   | `ListControl`          | control base  | list base                                           |
|  [05]   | `ListBox`              | control       | single-column selectable list                       |
|  [06]   | `PropertyGrid`         | control       | reflected property editor over a bound object graph |
|  [07]   | `GridColumn`           | model         | header text and `DataCell`                          |
|  [08]   | `TreeGridCell`         | model         | hit-test result                                     |
|  [09]   | `TreeGridViewDragInfo` | model         | drop-target descriptor                              |

- cell renderers (`GridColumn.DataCell`): `TextBoxCell` `CheckBoxCell` `ComboBoxCell` `ImageViewCell` `ImageTextCell` `ProgressCell` `DrawableCell` `CustomCell`

[PUBLIC_TYPE_SCOPE]: containers and rich controls

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------ | :------------ | :------------------------------------------------ |
|  [01]   | `Panel`             | container     | single-child content host and panel-subclass base |
|  [02]   | `Scrollable`        | container     | scrolling viewport with border and expand flags   |
|  [03]   | `Splitter`          | container     | two-panel split                                   |
|  [04]   | `TabControl`        | container     | tabbed pages                                      |
|  [05]   | `TabPage`           | container     | one tab with text, image, and child content       |
|  [06]   | `GroupBox`          | container     | titled bordered frame around child content        |
|  [07]   | `Expander`          | container     | collapsible header and content region             |
|  [08]   | `Drawable`          | control       | owner-drawn surface                               |
|  [09]   | `RichTextArea`      | control       | formatted text (`: TextArea`)                     |
|  [10]   | `ITextBuffer`       | interface     | range formatting and IO                           |
|  [11]   | `ImageView`         | control       | static bitmap display                             |
|  [12]   | `SegmentedButton`   | control       | multi-segment toggle bar                          |
|  [13]   | `ProgressBar`       | control       | determinate/indeterminate progress                |
|  [14]   | `WebView`           | control       | embedded browser: navigation, script exec, title  |
|  [15]   | `NativeControlHost` | control       | host-native view embedding seam                   |

[PUBLIC_TYPE_SCOPE]: layout owners

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :---------------- | :------------ | :------------------------ |
|  [01]   | `DynamicLayout`   | layout        | fluent row/column builder |
|  [02]   | `TableLayout`     | layout        | fixed grid                |
|  [03]   | `PixelLayout`     | layout        | absolute placement        |
|  [04]   | `StackLayout`     | layout        | linear stack              |
|  [05]   | `Padding` (`Eto`) | value         | edge inset                |
|  [06]   | `Size` (`Eto`)    | value         | integer extent            |

- vocabulary enums: `Orientation` `HorizontalAlignment` `VerticalAlignment` `DockPosition`

[PUBLIC_TYPE_SCOPE]: windows, dialogs, and native pickers

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------- | :------------ | :--------------------------------------- |
|  [01]   | `Window`             | window base   | top-level window base                    |
|  [02]   | `Form`               | window        | modeless top-level window                |
|  [03]   | `FloatingForm`       | window        | always-on-top modeless window            |
|  [04]   | `Dialog`             | window        | modal window                             |
|  [05]   | `Dialog<T>`          | window        | typed-result modal                       |
|  [06]   | `MessageBox`         | static        | text/type/buttons prompt overload family |
|  [07]   | `FileDialog`         | dialog base   | file-dialog base                         |
|  [08]   | `OpenFileDialog`     | dialog        | file-open picker                         |
|  [09]   | `SaveFileDialog`     | dialog        | file-save picker                         |
|  [10]   | `SelectFolderDialog` | dialog        | folder picker                            |
|  [11]   | `ColorDialog`        | dialog        | native colour picker                     |
|  [12]   | `FontDialog`         | dialog        | native font picker                       |
|  [13]   | `FileFilter`         | model         | extension filter row for file dialogs    |

- vocabulary enums: `DialogResult` `WindowState` `WindowStyle` `DialogDisplayMode`

[PUBLIC_TYPE_SCOPE]: menus and commands

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :--------------- | :------------ | :------------------------ |
|  [01]   | `ContextMenu`    | menu          | popup menu                |
|  [02]   | `MenuItem`       | menu          | leaf menu entry           |
|  [03]   | `ButtonMenuItem` | menu          | command-bound menu entry  |
|  [04]   | `SubMenuItem`    | menu          | nested menu entry         |
|  [05]   | `Command`        | command       | reusable action           |
|  [06]   | `CheckCommand`   | command       | toggling command          |
|  [07]   | `RadioCommand`   | command       | grouped exclusive command |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: control lifecycle, input, and drag

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :-------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `Control.Focus()`                             | instance | focus request                      |
|  [02]   | `Control.Invalidate()`                        | instance | repaint request                    |
|  [03]   | `Control.UpdateLayout()`                      | instance | re-measure request                 |
|  [04]   | `Control.CaptureMouse() -> bool`              | instance | begins pointer capture             |
|  [05]   | `Control.ReleaseMouseCapture()`               | instance | ends pointer capture               |
|  [06]   | `Control.DoDragDrop(DataObject, DragEffects)` | instance | starts a drag with a typed payload |

- `Control` state: `Enabled` `Visible` `Bounds` `Cursor` `ContextMenu` `ToolTip` `ParentWindow`
- `Control` pointer (`EventHandler<MouseEventArgs>`): `MouseDown` `MouseUp` `MouseMove` `MouseEnter` `MouseLeave` `MouseDoubleClick` `MouseWheel`
- `Control` keyboard: `KeyDown` `KeyUp` (`EventHandler<KeyEventArgs>`), `TextInput` (`EventHandler<TextInputEventArgs>`)
- `Control` drag (`EventHandler<DragEventArgs>`): `DragEnter` `DragOver` `DragLeave` `DragDrop` `DragEnd`
- `Control` lifecycle (`EventHandler`): `GotFocus` `LostFocus` `SizeChanged` `Load` `Shown`

[ENTRYPOINT_SCOPE]: grid, tree, and list selection, edit, and reload

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `Grid.SelectRow(int)`                                             | instance | selects a row                |
|  [02]   | `Grid.UnselectRow(int)`                                           | instance | unselects a row              |
|  [03]   | `Grid.SelectAll()`                                                | instance | selects every row            |
|  [04]   | `Grid.UnselectAll()`                                              | instance | unselects every row          |
|  [05]   | `Grid.BeginEdit(int, int)`                                        | instance | begins inline editing        |
|  [06]   | `Grid.CommitEdit() -> bool`                                       | instance | commits inline editing       |
|  [07]   | `Grid.CancelEdit() -> bool`                                       | instance | cancels inline editing       |
|  [08]   | `Grid.ScrollToRow(int)`                                           | instance | brings a row into view       |
|  [09]   | `Grid.IsEditing`                                                  | property | gates the edit lifecycle     |
|  [10]   | `TreeGridView.ReloadData()`                                       | instance | refreshes, keeping selection |
|  [11]   | `TreeGridView.ReloadItem(ITreeGridItem, bool)`                    | instance | refreshes a subtree          |
|  [12]   | `TreeGridView.GetCellAt(PointF) -> TreeGridCell`                  | instance | hit-test resolution          |
|  [13]   | `TreeGridView.GetDragInfo(DragEventArgs) -> TreeGridViewDragInfo` | instance | drop-target resolution       |

- `Grid`: `Columns` `ShowHeader` `AllowColumnReordering` `AllowMultipleSelection` `RowHeight` `GridLines` `SelectedRows` `SelectedItem` `SelectedItems`
- `Grid` events: `CellEditing` `CellEdited` `CellClick` `CellFormatting` `RowFormatting`
- `GridColumn`: `HeaderText` `DataCell` `Editable` `Resizable` `Sortable` `Visible` `Width`
- `TreeGridView`: `DataStore` (`ITreeGridStore<ITreeGridItem>`) `SelectedItem` (`ITreeGridItem`); expand, collapse, and activate event family
- `ListControl`: `DataStore` `SelectedIndex` `SelectedValue` `SelectedKey` `SelectedIndexChanged` `SelectedValueChanged`
- `ListBox`: `ItemImageBinding` `Border` `Activated`
- `TreeGridCell`: `Item` `Column` `ColumnIndex` `Type`; `TreeGridViewDragInfo`: `Item` `Parent` `Position` `InsertIndex`

[ENTRYPOINT_SCOPE]: field state
- `TextBox`: `PlaceholderText` `MaxLength` `ReadOnly` `CaretIndex` `SelectedText` `Selection` `SelectAll()` `TextChanging`
- `TextArea`: `AcceptsReturn` `AcceptsTab` `SpellCheck` `Wrap` `CaretIndex` `Selection` `CaretIndexChanged` `SelectionChanged`
- `NumericStepper`: `MinValue` `MaxValue` `Increment` `Value` `DecimalPlaces` `FormatString` `Wrap`
- `CheckBox`: `Checked` (`bool?`) `ThreeState` `CheckedChanged`
- `Slider`: `MinValue` `MaxValue` `Value` `Orientation` `TickFrequency` `SnapToTick` `ValueChanged`
- `DropDown`: `ShowBorder` `ItemImageBinding` `DropDownOpening` `DropDownClosed` `FormatItem`

[ENTRYPOINT_SCOPE]: container and rich-control state
- `Splitter`: `Panel1` `Panel2` `Orientation` `Position` `FixedPanel` `SplitterWidth`
- `TabControl`: `Pages` `SelectedIndex` `SelectedPage` `TabPosition` `SelectedIndexChanged`
- `Expander`: `Expanded` `ExpandedChanged`
- `Drawable`: `CanFocus` `SupportsCreateGraphics` `Paint` (`PaintEventArgs`) `TextComposition` `TextInsertionBoundsRequested`
- `RichTextArea`: `SelectionFont` `SelectionForeground` `SelectionBackground` `SelectionBold` `SelectionItalic` `SelectionUnderline` `SelectionStrikethrough` `Buffer` `Rtf`
- `ITextBuffer`: `SetBold` `SetItalic` `SetFont` `SetForeground` `SetBackground` `Insert` `Delete` `Clear` `Load` `Save(Stream, RichTextAreaFormat)`
- `ProgressBar`: `MinValue` `MaxValue` `Value` `Indeterminate`

[ENTRYPOINT_SCOPE]: window, dialog, and picker presentation

`Dialog<T>.Result` carries the typed outcome from either modal presentation surface.

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `Form.Show()`                                            | instance | modeless window         |
|  [02]   | `Dialog.ShowModal(Control)`                              | instance | modal loop              |
|  [03]   | `Dialog.ShowModalAsync(Control) -> Task`                 | instance | asynchronous modal loop |
|  [04]   | `MessageBox.Show(Control, string, ...) -> DialogResult`  | static   | prompt overload family  |
|  [05]   | `FileDialog.ShowDialog(Control) -> DialogResult`         | instance | file open/save picker   |
|  [06]   | `SelectFolderDialog.ShowDialog(Control) -> DialogResult` | instance | folder picker           |
|  [07]   | `ColorDialog.ShowDialog(Control) -> DialogResult`        | instance | native colour picker    |
|  [08]   | `FontDialog.ShowDialog(Control) -> DialogResult`         | instance | native font picker      |
|  [09]   | `Window.Close()`                                         | instance | window teardown         |
|  [10]   | `Window.BringToFront()`                                  | instance | z-order promotion       |
|  [11]   | `Window.SetOwner(Window)`                                | instance | owner assignment        |
|  [12]   | `Window.FromPoint(PointF) -> Window`                     | static   | window lookup           |

- `Window`: `Title` `Location` `Bounds` `Opacity` `Resizable` `Topmost` `WindowState` `WindowStyle` `LogicalPixelSize`
- `Window` events: `Closing` `Closed` `WindowStateChanged` `LogicalPixelSizeChanged`; `Form`: `ShowActivated`
- `Dialog`: `DisplayMode` `DefaultButton` `AbortButton` `PositiveButtons` `NegativeButtons`
- `FileDialog`: `Directory` `FileName` `Title` `CheckFileExists` `Filters` `CurrentFilter` `CurrentFilterIndex`
- `OpenFileDialog`: `MultiSelect` `Filenames`; `SelectFolderDialog`: `Directory` `Title`
- `ColorDialog`: `Color` `AllowAlpha` `ColorChanged`; `FontDialog`: `Font` `FontChanged`

[ENTRYPOINT_SCOPE]: layout composition and command dispatch

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]             |
| :-----: | :----------------------------------------------- | :------- | :----------------------- |
|  [01]   | `DynamicLayout.AddRow(params Control[])`         | instance | fluent row composition   |
|  [02]   | `DynamicLayout.AddSeparateRow(params Control[])` | instance | separate-row composition |
|  [03]   | `DynamicLayout.BeginVertical()`                  | instance | begins vertical scope    |
|  [04]   | `DynamicLayout.EndVertical()`                    | instance | ends vertical scope      |
|  [05]   | `DynamicLayout.BeginHorizontal()`                | instance | begins horizontal scope  |
|  [06]   | `DynamicLayout.EndHorizontal()`                  | instance | ends horizontal scope    |
|  [07]   | `TableLayout.Add(Control, int, int)`             | instance | fixed-cell placement     |
|  [08]   | `TableLayout.Move(Control, int, int)`            | instance | fixed-cell movement      |
|  [09]   | `TableLayout.SetColumnScale(int, bool)`          | instance | column scaling           |
|  [10]   | `TableLayout.SetRowScale(int, bool)`             | instance | row scaling              |
|  [11]   | `PixelLayout.Add(Control, int, int)`             | instance | absolute placement       |
|  [12]   | `PixelLayout.Move(Control, int, int)`            | instance | absolute movement        |
|  [13]   | `PixelLayout.GetLocation(Control) -> Point`      | instance | location lookup          |
|  [14]   | `ContextMenu.Show(Control, PointF)`              | instance | canvas-point popup       |
|  [15]   | `Command.Execute()`                              | instance | action dispatch          |

- `DynamicLayout`: `Padding` `Spacing` `DefaultPadding` `DefaultSpacing` `AddSeparateColumn`
- `TableLayout`: `Rows` `Dimensions` `SetCellSize`; `PixelLayout`: `Controls`
- `StackLayout`: `Items` `Orientation` `Spacing` `HorizontalContentAlignment` `VerticalContentAlignment`
- `ContextMenu`: `Items` `Trim` `Opening` `Closing` `Closed`
- `Command`: `ID` `MenuText` `ToolBarText` `ToolTip` `Enabled` `Shortcut` `Executed`
- `CheckCommand`: `Checked` `CheckedChanged`; `RadioCommand`: `Controller`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- a hosted panel is an `Eto.Forms` composition: a `Panel`/`Scrollable` root holds one layout owner, and the layout holds the field, data-view, and container roster
- one control owns one concern — a field its typed value and `*Binding`, a data view its store and selection, a container its children and split/tab/expand state
- closed enum vocabularies discriminate every layout and window construction switch
- `Grid` is the shared base of `GridView` and `TreeGridView`: columns, cell renderers, selection, edit, and format events inherit from it, and `TreeGridView` adds the expand/collapse and tree-drag surface

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): a dialog show, file-dialog result, or control-property mutation that throws at the host boundary lands on `Fin<A>` through `Try.lift(() => dialog.ShowModal(owner)).Run()`; `Optional(grid.SelectedItem).ToFin(error)` null-gates a selection read; independent field reads lift `.ToValidation`, fan in through the tuple `.Apply(...)`, and exit `.ToFin()` before a panel commits its edit
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the host enum vocabularies project onto `[SmartEnum<TKey>]` owners where a panel attaches behaviour or a display label to a case; a bounded field value (a titled numeric range, a validated text token, a colour channel) is a `[ValueObject<T>]` the control `*Binding` reads and writes
- `api-eto-binding`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-binding.md`): every field and view exposes its `*Binding`, the seam the binding rail fuses to a `DataContext` model
- `api-eto-drawing`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-drawing.md`): `Drawable.Paint` hands `PaintEventArgs.Graphics` to the drawing surface for owner-drawn control content
- `api-eto-runtime`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-runtime.md`): dialog presentation, control invalidation, and cross-thread mutation marshal through `Application.Instance`

[LOCAL_ADMISSION]:
- `Eto.Forms` is host-provided; a panel subclasses a control or composes the roster directly
- a new control capability lands as a subclass or a composition of the admitted roster
- boundary faults lower onto the LanguageExt rail

[RAIL_LAW]:
- Package: `Eto`
- Owns: native control construction, layout, windows and dialogs, menus and commands for GH2-hosted panels
- Accept: panel chrome, form fields, grid/tree/list/property data views, modal and modeless dialogs, native file/colour/font pickers
- Reject: a local wrapper renaming a host member, a re-implemented native widget, an exception-style fault path beside the LanguageExt rail
