# [RASM_GRASSHOPPER_API_ETO_FORMS]

`Eto.Forms` mints every native control, container, layout, window, dialog, and menu a GH2-hosted panel raises inside the Rhino process, and this catalog fixes the verified construction surface the folder composes into hosted panels. The control roster spans text, numeric, choice, colour, and boolean fields; the grid, tree, list, and property-grid data views; the tab, group, expander, scrollable, and splitter containers; and the rich-text, image, segmented, masked, progress, and browser controls. Windows, modal and modeless dialogs, the native file/colour/font pickers, and the menu/command vocabulary complete the panel chrome. Layout is `DynamicLayout`, `TableLayout`, `PixelLayout`, and `StackLayout`, each an immutable-composition owner a panel builds against.

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
- rail: native UI

| [INDEX] | [SYMBOL]                                                | [KIND]       | [CAPABILITY]                                                                                                                                                                                                    |
| :-----: | :------------------------------------------------------ | :----------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Control`                                               | control base | lifecycle/focus/size/mouse/key/drag event owner; `Enabled`/`Visible`/`Bounds`/`Cursor`/`ContextMenu`/`ToolTip`/`ParentWindow`, `Focus()`/`Invalidate()`/`CaptureMouse()`/`DoDragDrop(IDataObject, DragEffects)` |
|  [02]   | `TextBox`                                               | control      | single-line text; `PlaceholderText`/`MaxLength`/`ReadOnly`/`CaretIndex`/`SelectedText`, `Selection` (`Range<int>`), `SelectAll()`, `TextChanging` event                                                         |
|  [03]   | `TextArea`                                              | control      | multi-line text; `AcceptsReturn`/`AcceptsTab`/`SpellCheck`/`Wrap`/`CaretIndex`, `Selection`, `CaretIndexChanged`/`SelectionChanged`                                                                             |
|  [04]   | `NumericStepper`                                        | control      | bounded number field; `MinValue`/`MaxValue`/`Increment`/`Value`/`DecimalPlaces`/`FormatString`/`Wrap`, `ValueBinding`                                                                                           |
|  [05]   | `CheckBox`                                              | control      | tri-state boolean; `Checked` (`bool?`), `ThreeState`, `CheckedBinding`, `CheckedChanged`                                                                                                                        |
|  [06]   | `Slider`                                                | control      | ranged integer track; `MinValue`/`MaxValue`/`Value`/`Orientation`/`TickFrequency`/`SnapToTick`, `ValueBinding`, `ValueChanged`                                                                                  |
|  [07]   | `DropDown`                                              | control      | single-choice list; `ShowBorder`, `ItemImageBinding`, `DropDownOpening`/`DropDownClosed`/`FormatItem`                                                                                                           |
|  [08]   | `ComboBox`                                              | control      | editable single-choice text/list field                                                                                                                                                                          |
|  [09]   | `ColorPicker`                                           | control      | inline colour swatch and picker field                                                                                                                                                                           |
|  [10]   | `DateTimePicker`                                        | control      | date and/or time field with min/max range                                                                                                                                                                       |
|  [11]   | `Label` / `LinkButton`                                  | control      | static text; `LinkButton` raises a click on an inline hyperlink                                                                                                                                                 |
|  [12]   | `Button` / `ToggleButton`                               | control      | command trigger; `ToggleButton` holds a pressed state                                                                                                                                                           |
|  [13]   | `RadioButton` / `RadioButtonList` / `CheckBoxList`      | control      | mutually-exclusive and multi-select option groups                                                                                                                                                               |
|  [14]   | `PasswordBox` / `SearchBox`                             | control      | masked-entry and search-styled text fields                                                                                                                                                                      |
|  [15]   | `Stepper` / `TextStepper` / `NumericUpDown` / `Spinner` | control      | up/down increment and busy-indicator fields                                                                                                                                                                     |
|  [16]   | `FilePicker` / `FontPicker`                             | control      | inline file-path and font-selection fields                                                                                                                                                                      |
|  [17]   | `MaskedTextBox<T>` / `MaskedTextStepper<T>`             | control      | format-masked text and stepper over a typed provider                                                                                                                                                            |

[PUBLIC_TYPE_SCOPE]: data views — grid, tree, list, property grid
- rail: native UI

| [INDEX] | [SYMBOL]                                | [KIND]       | [CAPABILITY]                                                                                                                                                                                                                                        |
| :-----: | :-------------------------------------- | :----------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Grid`                                  | control base | column/row view base; `Columns` (`GridColumnCollection`), `ShowHeader`/`AllowColumnReordering`/`AllowMultipleSelection`/`RowHeight`/`GridLines`, `SelectedRows`/`SelectedItem`/`SelectedItems`, `SelectedItemBinding`, edit and format event family |
|  [02]   | `GridView`                              | control      | list-backed grid over an `IEnumerable` data store                                                                                                                                                                                                   |
|  [03]   | `TreeGridView`                          | control      | multi-column tree; `DataStore` (`ITreeGridStore<ITreeGridItem>`), `SelectedItem` (`ITreeGridItem`), `ReloadData()`/`ReloadItem(...)`/`GetCellAt(PointF)`/`GetDragInfo(...)`, expand/collapse/activate event family                                  |
|  [04]   | `GridColumn`                            | model        | one column; header text, `DataCell`, editable/resizable/sortable/visible/width                                                                                                                                                                      |
|  [05]   | `Cell` family                           | model        | `TextBoxCell`/`CheckBoxCell`/`ComboBoxCell`/`ImageViewCell`/`ImageTextCell`/`ProgressCell`/`DrawableCell`/`CustomCell` cell renderers                                                                                                               |
|  [06]   | `ListBox`                               | control      | single-column selectable list (`: ListControl`); `ItemImageBinding`, `Border`, `Activated`                                                                                                                                                          |
|  [07]   | `ListControl`                           | control base | list base; `DataStore`, `SelectedIndex`/`SelectedValue`/`SelectedKey`, `ItemTextBinding`/`ItemKeyBinding`, `SelectedIndexChanged`/`SelectedValueChanged`                                                                                            |
|  [08]   | `PropertyGrid`                          | control      | reflected property editor over a bound object graph                                                                                                                                                                                                 |
|  [09]   | `TreeGridCell` / `TreeGridViewDragInfo` | model        | hit-test result (`Item`/`Column`/`ColumnIndex`/`Type`) and drop-target descriptor (`Item`/`Parent`/`Position`/`InsertIndex`)                                                                                                                        |

[PUBLIC_TYPE_SCOPE]: containers and rich controls
- rail: native UI

| [INDEX] | [SYMBOL]            | [KIND]    | [CAPABILITY]                                                                                                                                                   |
| :-----: | :------------------ | :-------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Panel`             | container | single-child content host; the panel-subclass base                                                                                                             |
|  [02]   | `Scrollable`        | container | scrolling viewport; border, scroll size, expand-content flags                                                                                                  |
|  [03]   | `Splitter`          | container | two-panel split; `Panel1`/`Panel2`/`Orientation`/`Position`/`FixedPanel`/`SplitterWidth`                                                                       |
|  [04]   | `TabControl`        | container | tabbed pages; `Pages` (`Collection<TabPage>`), `SelectedIndex`/`SelectedPage`/`TabPosition`, `SelectedIndexBinding`, `SelectedIndexChanged`                    |
|  [05]   | `TabPage`           | container | one tab; text, image, child content                                                                                                                            |
|  [06]   | `GroupBox`          | container | titled bordered frame around child content                                                                                                                     |
|  [07]   | `Expander`          | container | collapsible header + content region; `Expanded`, `ExpandedChanged`                                                                                             |
|  [08]   | `Drawable`          | control   | owner-drawn surface; `CanFocus`/`SupportsCreateGraphics`, `Paint` (`PaintEventArgs`), `TextComposition`/`TextInsertionBoundsRequested`                         |
|  [09]   | `RichTextArea`      | control   | formatted text (`: TextArea`); `Selection*` font/colour/bold/italic/underline/strikethrough, `Buffer` (`ITextBuffer`), `Rtf`                                   |
|  [10]   | `ITextBuffer`       | contract  | range formatting and IO; `SetBold`/`SetItalic`/`SetFont`/`SetForeground`/`SetBackground`, `Load`/`Save(Stream, RichTextAreaFormat)`, `Insert`/`Delete`/`Clear` |
|  [11]   | `ImageView`         | control   | static bitmap display                                                                                                                                          |
|  [12]   | `SegmentedButton`   | control   | multi-segment toggle bar; per-segment items and selection modes                                                                                                |
|  [13]   | `ProgressBar`       | control   | determinate/indeterminate progress; `MinValue`/`MaxValue`/`Value`/`Indeterminate`                                                                              |
|  [14]   | `WebView`           | control   | embedded browser; navigation, script execution, document title                                                                                                 |
|  [15]   | `NativeControlHost` | control   | host-native view embedding seam                                                                                                                                |

[PUBLIC_TYPE_SCOPE]: layout owners
- rail: native UI

| [INDEX] | [SYMBOL]                                                                     | [KIND] | [CAPABILITY]                                                                                                                                                                                     |
| :-----: | :--------------------------------------------------------------------------- | :----- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `DynamicLayout`                                                              | layout | fluent row/column builder; `Padding`/`Spacing`/`DefaultPadding`/`DefaultSpacing`, `AddRow`/`AddSeparateRow`/`AddSeparateColumn`, `BeginHorizontal`/`EndHorizontal`/`BeginVertical`/`EndVertical` |
|  [02]   | `TableLayout`                                                                | layout | fixed grid; `Rows` (`TableRowCollection`), `Dimensions`, `Add(Control, int x, int y)`/`Move`/`SetColumnScale`/`SetRowScale`/`SetCellSize`                                                        |
|  [03]   | `PixelLayout`                                                                | layout | absolute placement; `Add(Control, int x, int y)`/`Move`/`GetLocation`, `Controls`                                                                                                                |
|  [04]   | `StackLayout`                                                                | layout | linear stack; `Orientation`/`Spacing`/`HorizontalContentAlignment`/`VerticalContentAlignment`, `Items` (`StackLayoutItemCollection`)                                                             |
|  [05]   | `Orientation` / `HorizontalAlignment` / `VerticalAlignment` / `DockPosition` | enum   | axis and alignment vocabularies the layout owners discriminate on                                                                                                                                |
|  [06]   | `Padding` / `Size` (`Eto`)                                                   | value  | edge inset and integer extent value types                                                                                                                                                        |

[PUBLIC_TYPE_SCOPE]: windows, dialogs, and native pickers
- rail: native UI

| [INDEX] | [SYMBOL]                                                             | [KIND]      | [CAPABILITY]                                                                                                                                                                                                                                             |
| :-----: | :------------------------------------------------------------------- | :---------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Window`                                                             | window base | `Title`/`Location`/`Bounds`/`Opacity`/`Resizable`/`Topmost`/`WindowState`/`WindowStyle`/`LogicalPixelSize`, `Close()`/`BringToFront()`/`SetOwner(Window)`, static `FromPoint(PointF)`, `Closing`/`Closed`/`WindowStateChanged`/`LogicalPixelSizeChanged` |
|  [02]   | `Form`                                                               | window      | modeless top-level window; `ShowActivated`, `Show()`                                                                                                                                                                                                     |
|  [03]   | `FloatingForm`                                                       | window      | always-on-top modeless window                                                                                                                                                                                                                            |
|  [04]   | `Dialog`                                                             | window      | modal window; `DisplayMode` (`DialogDisplayMode`), `DefaultButton`/`AbortButton`, `PositiveButtons`/`NegativeButtons`, `ShowModal(Control)`/`ShowModalAsync(Control)`                                                                                    |
|  [05]   | `Dialog<T>`                                                          | window      | typed-result modal; `Result` (`T`)                                                                                                                                                                                                                       |
|  [06]   | `MessageBox`                                                         | dialog      | static `Show(...)` overload family for text/type/buttons prompts                                                                                                                                                                                         |
|  [07]   | `FileDialog`                                                         | dialog base | `Directory`/`FileName`/`Title`/`CheckFileExists`, `Filters` (`FilterCollection<FileFilter>`), `CurrentFilter`/`CurrentFilterIndex`, `ShowDialog(Control)`                                                                                                |
|  [08]   | `OpenFileDialog` / `SaveFileDialog`                                  | dialog      | file open (`MultiSelect`, `Filenames`) and save pickers                                                                                                                                                                                                  |
|  [09]   | `SelectFolderDialog`                                                 | dialog      | folder picker; `Directory`/`Title`, `ShowDialog(Control)`                                                                                                                                                                                                |
|  [10]   | `ColorDialog` / `FontDialog`                                         | dialog      | native colour (`Color`/`AllowAlpha`/`ColorChanged`) and font (`Font`/`FontChanged`) pickers                                                                                                                                                              |
|  [11]   | `FileFilter`                                                         | model       | extension filter row for the file dialogs                                                                                                                                                                                                                |
|  [12]   | `DialogResult` / `WindowState` / `WindowStyle` / `DialogDisplayMode` | enum        | dialog outcome, window presentation, and modal-display vocabularies                                                                                                                                                                                      |

[PUBLIC_TYPE_SCOPE]: menus and commands
- rail: native UI

| [INDEX] | [SYMBOL]                                      | [KIND]  | [CAPABILITY]                                                                                                         |
| :-----: | :-------------------------------------------- | :------ | :------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `ContextMenu`                                 | menu    | popup menu; `Items` (`MenuItemCollection`), `Trim`, `Show(Control, PointF)`, `Opening`/`Closing`/`Closed`            |
|  [02]   | `MenuItem` / `ButtonMenuItem` / `SubMenuItem` | menu    | leaf, command-bound, and nested menu entries                                                                         |
|  [03]   | `Command`                                     | command | reusable action; `ID`/`MenuText`/`ToolBarText`/`ToolTip`/`Enabled`/`Shortcut`/`DataContext`, `Execute()`, `Executed` |
|  [04]   | `CheckCommand`                                | command | toggling command; `Checked`, `CheckedChanged`                                                                        |
|  [05]   | `RadioCommand`                                | command | grouped exclusive command; `Controller`                                                                              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: control lifecycle, input, and drag
- rail: native UI

| [INDEX] | [SURFACE]                                                           | [CALL_SHAPE]                                          | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------------ | :---------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `Control.Focus` / `Invalidate` / `UpdateLayout`                     | `()`                                                  | focus, repaint, and re-measure requests           |
|  [02]   | `Control.CaptureMouse` / `ReleaseMouseCapture`                      | `()` → `bool` / `()`                                  | pointer-capture lifecycle for a drag gesture      |
|  [03]   | `Control.DoDragDrop`                                                | `(IDataObject, DragEffects)` → `DragEffects`          | starts a drag with a typed payload                |
|  [04]   | `Control.KeyDown` / `KeyUp` / `TextInput`                           | `EventHandler<KeyEventArgs>` / `<TextInputEventArgs>` | keyboard and composed-text input                  |
|  [05]   | `Control.MouseDown` … `MouseWheel`                                  | `EventHandler<MouseEventArgs>`                        | pointer press/move/enter/leave/double-click/wheel |
|  [06]   | `Control.DragEnter` … `DragEnd`                                     | `EventHandler<DragEventArgs>`                         | drag-over and drop lifecycle                      |
|  [07]   | `Control.GotFocus` / `LostFocus` / `SizeChanged` / `Load` / `Shown` | `EventHandler`                                        | focus, geometry, and attach lifecycle             |

[ENTRYPOINT_SCOPE]: grid and tree selection, edit, and reload
- rail: native UI

| [INDEX] | [SURFACE]                                                                            | [CALL_SHAPE]                                                             | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------------------------------- | :----------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `Grid.SelectRow` / `UnselectRow` / `SelectAll` / `UnselectAll`                       | `(int)` / `(int)` / `()` / `()`                                          | selection mutation                             |
|  [02]   | `Grid.BeginEdit` / `CommitEdit` / `CancelEdit`                                       | `(int row, int column)` / `()` → `bool`                                  | inline-edit lifecycle; `IsEditing` gates state |
|  [03]   | `Grid.ScrollToRow`                                                                   | `(int)`                                                                  | brings a row into view                         |
|  [04]   | `Grid.CellEditing` / `CellEdited` / `CellClick` / `CellFormatting` / `RowFormatting` | event                                                                    | per-cell edit and per-row format hooks         |
|  [05]   | `TreeGridView.ReloadData` / `ReloadItem`                                             | `()` / `(ITreeGridItem, bool reloadChildren)`                            | refresh keeping selection                      |
|  [06]   | `TreeGridView.GetCellAt` / `GetDragInfo`                                             | `(PointF)` → `TreeGridCell` / `(DragEventArgs)` → `TreeGridViewDragInfo` | hit-test and drop-target resolution            |

[ENTRYPOINT_SCOPE]: window, dialog, and picker presentation
- rail: native UI

| [INDEX] | [SURFACE]                                          | [CALL_SHAPE]                            | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------- | :-------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `Form.Show`                                        | `()`                                    | shows a modeless window                                         |
|  [02]   | `Dialog.ShowModal` / `ShowModalAsync`              | `(Control owner)` / `()` → `Task`       | runs a modal loop; `Dialog<T>.Result` carries the typed outcome |
|  [03]   | `MessageBox.Show`                                  | `(Control, string, …)` → `DialogResult` | static prompt overload family                                   |
|  [04]   | `FileDialog.ShowDialog`                            | `(Control parent)` → `DialogResult`     | file open/save picker                                           |
|  [05]   | `SelectFolderDialog.ShowDialog`                    | `(Control parent)` → `DialogResult`     | folder picker                                                   |
|  [06]   | `ColorDialog.ShowDialog` / `FontDialog.ShowDialog` | `(Control parent)` → `DialogResult`     | native colour and font pickers                                  |
|  [07]   | `Window.Close` / `BringToFront` / `SetOwner`       | `()` / `()` / `(Window)`                | window teardown and z-order                                     |

[ENTRYPOINT_SCOPE]: layout composition and command dispatch
- rail: native UI

| [INDEX] | [SURFACE]                                                                           | [CALL_SHAPE]                                      | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------------------------------- | :------------------------------------------------ | :------------------------------ |
|  [01]   | `DynamicLayout.AddRow` / `AddSeparateRow`                                           | `(params Control[])`                              | fluent row composition          |
|  [02]   | `DynamicLayout.BeginVertical` / `EndVertical` / `BeginHorizontal` / `EndHorizontal` | `()`                                              | nested-section scoping          |
|  [03]   | `TableLayout.Add` / `Move` / `SetColumnScale` / `SetRowScale`                       | `(Control, int x, int y)` / `(int, bool)`         | fixed-cell placement and scale  |
|  [04]   | `PixelLayout.Add` / `Move` / `GetLocation`                                          | `(Control, int x, int y)` / `(Control)` → `Point` | absolute placement              |
|  [05]   | `ContextMenu.Show`                                                                  | `(Control, PointF)`                               | popup at a canvas point         |
|  [06]   | `Command.Execute` / `Executed`                                                      | `()` / `EventHandler`                             | action dispatch and observation |

## [04]-[IMPLEMENTATION_LAW]

[CONSTRUCTION_TOPOLOGY]:
- a hosted panel is an `Eto.Forms` control composition: a `Panel`/`Scrollable` root holds a layout owner (`DynamicLayout`/`TableLayout`/`PixelLayout`/`StackLayout`), and the layout holds the field, data-view, and container roster
- one control owns one concern: a field carries its typed value plus a `*Binding`, a data view carries a data store plus selection, a container carries children plus split/tab/expand state
- the enum vocabularies (`Orientation`, `WindowState`, `WindowStyle`, `DialogDisplayMode`, `DockPosition`, `GridLines`, `HorizontalAlignment`, `VerticalAlignment`) are the closed discriminants layout and window construction switch on
- `Grid` is the shared base of `GridView` and `TreeGridView`; column definitions, cell renderers, selection, edit, and format events are Grid-level and inherited, with `TreeGridView` adding the expand/collapse and tree-drag surface

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): a dialog show, file-dialog result, or control-property mutation that throws at the host boundary lands on `Fin<A>` through `Try.lift(() => dialog.ShowModal(owner)).Run()`; `Optional(grid.SelectedItem).ToFin(error)` null-gates a selection read; independent field reads lift `.ToValidation`, fan in through the tuple `.Apply(...)`, and exit `.ToFin()` before a panel commits its edit
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the host enum vocabularies project onto `[SmartEnum<TKey>]` owners where a panel attaches behaviour or a display label to a case; a bounded field value (a titled numeric range, a validated text token, a colour channel) is a `[ValueObject<T>]` the control `*Binding` reads and writes
- `api-eto-binding`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-binding.md`): every field and view exposes its `*Binding` (`TextBinding`, `CheckedBinding`, `ValueBinding`, `SelectedItemBinding`, `SelectedIndexBinding`), the seam the binding rail fuses to a `DataContext` model
- `api-eto-drawing`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-drawing.md`): `Drawable.Paint` hands `PaintEventArgs.Graphics` to the drawing surface for owner-drawn control content
- `api-eto-runtime`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-runtime.md`): dialog presentation, control invalidation, and cross-thread mutation marshal through `Application.Instance`

[LOCAL_ADMISSION]:
- `Eto.Forms` is host-provided and composed directly — a panel subclasses a control or composes the roster, never a local wrapper that renames or partially re-exports Eto members
- a new control capability is a subclass or a composition of the admitted roster, never a re-implemented native widget
- boundary faults ride the LanguageExt rail; the panel never carries an exception-style control flow beside it

[RAIL_LAW]:
- Package: `Eto`
- Owns: native control construction, layout, windows and dialogs, menus and commands for GH2-hosted panels
- Accept: panel chrome, form fields, grid/tree/list/property data views, modal and modeless dialogs, native file/colour/font pickers
- Reject: the runtime dispatch/timer/input/clipboard surface (`api-eto-runtime`), the 2D drawing surface (`api-eto-drawing`), the data-binding rail (`api-eto-binding`), macOS-native compositing (`api-macos-native`)
