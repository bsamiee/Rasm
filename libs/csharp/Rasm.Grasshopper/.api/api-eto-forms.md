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

| [INDEX] | [SYMBOL]               | [KIND]       | [CAPABILITY]                                             |
| :-----: | :--------------------- | :----------- | :------------------------------------------------------- |
|  [01]   | `Control`              | control base | lifecycle, focus, size, mouse, key, and drag event owner |
|  [02]   | `Control`              | property     | `Enabled`/`Visible`/`Bounds`/`Cursor`                    |
|  [03]   | `Control`              | property     | `ContextMenu`/`ToolTip`/`ParentWindow`                   |
|  [04]   | `Control`              | method       | `Focus()`/`Invalidate()`/`CaptureMouse()`                |
|  [05]   | `Control`              | method       | `DoDragDrop(IDataObject, DragEffects)`                   |
|  [06]   | `TextBox`              | control      | single-line text                                         |
|  [07]   | `TextBox`              | property     | `PlaceholderText`/`MaxLength`/`ReadOnly`                 |
|  [08]   | `TextBox`              | property     | `CaretIndex`/`SelectedText`/`Selection` (`Range<int>`)   |
|  [09]   | `TextBox`              | method/event | `SelectAll()`/`TextChanging`                             |
|  [10]   | `TextArea`             | control      | multi-line text                                          |
|  [11]   | `TextArea`             | property     | `AcceptsReturn`/`AcceptsTab`/`SpellCheck`/`Wrap`         |
|  [12]   | `TextArea`             | property     | `CaretIndex`/`Selection`                                 |
|  [13]   | `TextArea`             | event        | `CaretIndexChanged`/`SelectionChanged`                   |
|  [14]   | `NumericStepper`       | control      | bounded number field                                     |
|  [15]   | `NumericStepper`       | property     | `MinValue`/`MaxValue`/`Increment`/`Value`                |
|  [16]   | `NumericStepper`       | property     | `DecimalPlaces`/`FormatString`/`Wrap`/`ValueBinding`     |
|  [17]   | `CheckBox`             | control      | tri-state boolean                                        |
|  [18]   | `CheckBox`             | property     | `Checked` (`bool?`)/`ThreeState`/`CheckedBinding`        |
|  [19]   | `CheckBox`             | event        | `CheckedChanged`                                         |
|  [20]   | `Slider`               | control      | ranged integer track                                     |
|  [21]   | `Slider`               | property     | `MinValue`/`MaxValue`/`Value`/`Orientation`              |
|  [22]   | `Slider`               | property     | `TickFrequency`/`SnapToTick`/`ValueBinding`              |
|  [23]   | `Slider`               | event        | `ValueChanged`                                           |
|  [24]   | `DropDown`             | control      | single-choice list                                       |
|  [25]   | `DropDown`             | property     | `ShowBorder`/`ItemImageBinding`                          |
|  [26]   | `DropDown`             | event        | `DropDownOpening`/`DropDownClosed`/`FormatItem`          |
|  [27]   | `ComboBox`             | control      | editable single-choice text/list field                   |
|  [28]   | `ColorPicker`          | control      | inline colour swatch and picker field                    |
|  [29]   | `DateTimePicker`       | control      | date and/or time field with min/max range                |
|  [30]   | `Label`                | control      | static text                                              |
|  [31]   | `LinkButton`           | control      | click-raising inline hyperlink                           |
|  [32]   | `Button`               | control      | command trigger                                          |
|  [33]   | `ToggleButton`         | control      | command trigger with pressed state                       |
|  [34]   | `RadioButton`          | control      | mutually-exclusive option                                |
|  [35]   | `RadioButtonList`      | control      | mutually-exclusive option group                          |
|  [36]   | `CheckBoxList`         | control      | multi-select option group                                |
|  [37]   | `PasswordBox`          | control      | masked-entry text field                                  |
|  [38]   | `SearchBox`            | control      | search-styled text field                                 |
|  [39]   | `Stepper`              | control      | up/down increment field                                  |
|  [40]   | `TextStepper`          | control      | up/down increment text field                             |
|  [41]   | `NumericUpDown`        | control      | up/down increment numeric field                          |
|  [42]   | `Spinner`              | control      | busy-indicator field                                     |
|  [43]   | `FilePicker`           | control      | inline file-path field                                   |
|  [44]   | `FontPicker`           | control      | inline font-selection field                              |
|  [45]   | `MaskedTextBox<T>`     | control      | format-masked text over a typed provider                 |
|  [46]   | `MaskedTextStepper<T>` | control      | format-masked stepper over a typed provider              |

[PUBLIC_TYPE_SCOPE]: data views — grid, tree, list, property grid
- rail: native UI

| [INDEX] | [SYMBOL]                    | [KIND]       | [CAPABILITY]                                        |
| :-----: | :-------------------------- | :----------- | :-------------------------------------------------- |
|  [01]   | `Grid`                      | control base | column/row view base                                |
|  [02]   | `Grid.Columns`              | property     | `GridColumnCollection`                              |
|  [03]   | `Grid`                      | property     | `ShowHeader`/`AllowColumnReordering`                |
|  [04]   | `Grid`                      | property     | `AllowMultipleSelection`/`RowHeight`/`GridLines`    |
|  [05]   | `Grid`                      | property     | `SelectedRows`/`SelectedItem`/`SelectedItems`       |
|  [06]   | `Grid.SelectedItemBinding`  | binding      | selected-item binding                               |
|  [07]   | `Grid`                      | event        | edit and format event family                        |
|  [08]   | `GridView`                  | control      | list-backed grid over an `IEnumerable` data store   |
|  [09]   | `TreeGridView`              | control      | multi-column tree                                   |
|  [10]   | `TreeGridView.DataStore`    | property     | `ITreeGridStore<ITreeGridItem>`                     |
|  [11]   | `TreeGridView.SelectedItem` | property     | `ITreeGridItem`                                     |
|  [12]   | `TreeGridView`              | method       | `ReloadData()`/`ReloadItem(...)`                    |
|  [13]   | `TreeGridView`              | method       | `GetCellAt(PointF)`/`GetDragInfo(...)`              |
|  [14]   | `TreeGridView`              | event        | expand, collapse, and activate event family         |
|  [15]   | `GridColumn`                | model        | header text and `DataCell`                          |
|  [16]   | `GridColumn`                | property     | editable/resizable/sortable/visible/width           |
|  [17]   | `TextBoxCell`               | model        | cell renderer                                       |
|  [18]   | `CheckBoxCell`              | model        | cell renderer                                       |
|  [19]   | `ComboBoxCell`              | model        | cell renderer                                       |
|  [20]   | `ImageViewCell`             | model        | cell renderer                                       |
|  [21]   | `ImageTextCell`             | model        | cell renderer                                       |
|  [22]   | `ProgressCell`              | model        | cell renderer                                       |
|  [23]   | `DrawableCell`              | model        | cell renderer                                       |
|  [24]   | `CustomCell`                | model        | cell renderer                                       |
|  [25]   | `ListBox`                   | control      | single-column selectable list (`: ListControl`)     |
|  [26]   | `ListBox`                   | property     | `ItemImageBinding`/`Border`                         |
|  [27]   | `ListBox.Activated`         | event        | list activation                                     |
|  [28]   | `ListControl`               | control base | list base                                           |
|  [29]   | `ListControl.DataStore`     | property     | data store                                          |
|  [30]   | `ListControl`               | property     | `SelectedIndex`/`SelectedValue`/`SelectedKey`       |
|  [31]   | `ListControl`               | binding      | `ItemTextBinding`/`ItemKeyBinding`                  |
|  [32]   | `ListControl`               | event        | `SelectedIndexChanged`/`SelectedValueChanged`       |
|  [33]   | `PropertyGrid`              | control      | reflected property editor over a bound object graph |
|  [34]   | `TreeGridCell`              | model        | hit-test result                                     |
|  [35]   | `TreeGridCell`              | property     | `Item`/`Column`/`ColumnIndex`/`Type`                |
|  [36]   | `TreeGridViewDragInfo`      | model        | drop-target descriptor                              |
|  [37]   | `TreeGridViewDragInfo`      | property     | `Item`/`Parent`/`Position`/`InsertIndex`            |

[PUBLIC_TYPE_SCOPE]: containers and rich controls
- rail: native UI

| [INDEX] | [SYMBOL]              | [KIND]     | [CAPABILITY]                                      |
| :-----: | :-------------------- | :--------- | :------------------------------------------------ |
|  [01]   | `Panel`               | container  | single-child content host and panel-subclass base |
|  [02]   | `Scrollable`          | container  | scrolling viewport                                |
|  [03]   | `Scrollable`          | property   | border, scroll size, and expand-content flags     |
|  [04]   | `Splitter`            | container  | two-panel split                                   |
|  [05]   | `Splitter`            | property   | `Panel1`/`Panel2`/`Orientation`/`Position`        |
|  [06]   | `Splitter`            | property   | `FixedPanel`/`SplitterWidth`                      |
|  [07]   | `TabControl`          | container  | tabbed pages                                      |
|  [08]   | `TabControl.Pages`    | property   | `Collection<TabPage>`                             |
|  [09]   | `TabControl`          | property   | `SelectedIndex`/`SelectedPage`/`TabPosition`      |
|  [10]   | `TabControl`          | binding    | `SelectedIndexBinding`                            |
|  [11]   | `TabControl`          | event      | `SelectedIndexChanged`                            |
|  [12]   | `TabPage`             | container  | one tab with text, image, and child content       |
|  [13]   | `GroupBox`            | container  | titled bordered frame around child content        |
|  [14]   | `Expander`            | container  | collapsible header and content region             |
|  [15]   | `Expander`            | property   | `Expanded`                                        |
|  [16]   | `Expander`            | event      | `ExpandedChanged`                                 |
|  [17]   | `Drawable`            | control    | owner-drawn surface                               |
|  [18]   | `Drawable`            | property   | `CanFocus`/`SupportsCreateGraphics`               |
|  [19]   | `Drawable.Paint`      | event      | `PaintEventArgs`                                  |
|  [20]   | `Drawable`            | event      | `TextComposition`/`TextInsertionBoundsRequested`  |
|  [21]   | `RichTextArea`        | control    | formatted text (`: TextArea`)                     |
|  [22]   | `RichTextArea`        | property   | `Selection*` font/colour/bold/italic              |
|  [23]   | `RichTextArea`        | property   | `Selection*` underline/strikethrough              |
|  [24]   | `RichTextArea.Buffer` | property   | `ITextBuffer`                                     |
|  [25]   | `RichTextArea.Rtf`    | property   | rich-text format                                  |
|  [26]   | `ITextBuffer`         | contract   | range formatting and IO                           |
|  [27]   | `ITextBuffer`         | method     | `SetBold`/`SetItalic`/`SetFont`                   |
|  [28]   | `ITextBuffer`         | method     | `SetForeground`/`SetBackground`                   |
|  [29]   | `ITextBuffer`         | method     | `Load`/`Save(Stream, RichTextAreaFormat)`         |
|  [30]   | `ITextBuffer`         | method     | `Insert`/`Delete`/`Clear`                         |
|  [31]   | `ImageView`           | control    | static bitmap display                             |
|  [32]   | `SegmentedButton`     | control    | multi-segment toggle bar                          |
|  [33]   | `SegmentedButton`     | property   | per-segment items and selection modes             |
|  [34]   | `ProgressBar`         | control    | determinate/indeterminate progress                |
|  [35]   | `ProgressBar`         | property   | `MinValue`/`MaxValue`/`Value`/`Indeterminate`     |
|  [36]   | `WebView`             | control    | embedded browser                                  |
|  [37]   | `WebView`             | capability | navigation, script execution, and document title  |
|  [38]   | `NativeControlHost`   | control    | host-native view embedding seam                   |

[PUBLIC_TYPE_SCOPE]: layout owners
- rail: native UI

| [INDEX] | [SYMBOL]                          | [KIND]   | [CAPABILITY]                                 |
| :-----: | :-------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `DynamicLayout`                   | layout   | fluent row/column builder                    |
|  [02]   | `DynamicLayout`                   | property | `Padding`/`Spacing`                          |
|  [03]   | `DynamicLayout`                   | property | `DefaultPadding`/`DefaultSpacing`            |
|  [04]   | `DynamicLayout`                   | method   | `AddRow`/`AddSeparateRow`                    |
|  [05]   | `DynamicLayout.AddSeparateColumn` | method   | separate-column composition                  |
|  [06]   | `DynamicLayout`                   | method   | `BeginHorizontal`/`EndHorizontal`            |
|  [07]   | `DynamicLayout`                   | method   | `BeginVertical`/`EndVertical`                |
|  [08]   | `TableLayout`                     | layout   | fixed grid                                   |
|  [09]   | `TableLayout.Rows`                | property | `TableRowCollection`                         |
|  [10]   | `TableLayout.Dimensions`          | property | grid dimensions                              |
|  [11]   | `TableLayout`                     | method   | `Add(Control, int x, int y)`/`Move`          |
|  [12]   | `TableLayout`                     | method   | `SetColumnScale`/`SetRowScale`/`SetCellSize` |
|  [13]   | `PixelLayout`                     | layout   | absolute placement                           |
|  [14]   | `PixelLayout`                     | method   | `Add(Control, int x, int y)`/`Move`          |
|  [15]   | `PixelLayout.GetLocation`         | method   | control location                             |
|  [16]   | `PixelLayout.Controls`            | property | placed controls                              |
|  [17]   | `StackLayout`                     | layout   | linear stack                                 |
|  [18]   | `StackLayout`                     | property | `Orientation`/`Spacing`                      |
|  [19]   | `StackLayout`                     | property | `HorizontalContentAlignment`                 |
|  [20]   | `StackLayout`                     | property | `VerticalContentAlignment`                   |
|  [21]   | `StackLayout.Items`               | property | `StackLayoutItemCollection`                  |
|  [22]   | `Orientation`                     | enum     | axis vocabulary                              |
|  [23]   | `HorizontalAlignment`             | enum     | horizontal-alignment vocabulary              |
|  [24]   | `VerticalAlignment`               | enum     | vertical-alignment vocabulary                |
|  [25]   | `DockPosition`                    | enum     | dock-position vocabulary                     |
|  [26]   | `Padding` (`Eto`)                 | value    | edge inset                                   |
|  [27]   | `Size` (`Eto`)                    | value    | integer extent                               |

[PUBLIC_TYPE_SCOPE]: windows, dialogs, and native pickers
- rail: native UI

| [INDEX] | [SYMBOL]                   | [KIND]      | [CAPABILITY]                                      |
| :-----: | :------------------------- | :---------- | :------------------------------------------------ |
|  [01]   | `Window`                   | window base | top-level window base                             |
|  [02]   | `Window`                   | property    | `Title`/`Location`/`Bounds`/`Opacity`             |
|  [03]   | `Window`                   | property    | `Resizable`/`Topmost`/`WindowState`/`WindowStyle` |
|  [04]   | `Window.LogicalPixelSize`  | property    | logical pixel dimensions                          |
|  [05]   | `Window`                   | method      | `Close()`/`BringToFront()`/`SetOwner(Window)`     |
|  [06]   | `Window.FromPoint`         | static      | `(PointF)` window lookup                          |
|  [07]   | `Window`                   | event       | `Closing`/`Closed`                                |
|  [08]   | `Window`                   | event       | `WindowStateChanged`/`LogicalPixelSizeChanged`    |
|  [09]   | `Form`                     | window      | modeless top-level window                         |
|  [10]   | `Form.ShowActivated`       | property    | activation policy                                 |
|  [11]   | `Form.Show`                | method      | modeless presentation                             |
|  [12]   | `FloatingForm`             | window      | always-on-top modeless window                     |
|  [13]   | `Dialog`                   | window      | modal window                                      |
|  [14]   | `Dialog.DisplayMode`       | property    | `DialogDisplayMode`                               |
|  [15]   | `Dialog`                   | property    | `DefaultButton`/`AbortButton`                     |
|  [16]   | `Dialog`                   | property    | `PositiveButtons`/`NegativeButtons`               |
|  [17]   | `Dialog`                   | method      | `ShowModal(Control)`/`ShowModalAsync(Control)`    |
|  [18]   | `Dialog<T>`                | window      | typed-result modal                                |
|  [19]   | `Dialog<T>.Result`         | property    | `T`                                               |
|  [20]   | `MessageBox.Show`          | static      | text/type/buttons prompt overload family          |
|  [21]   | `FileDialog`               | dialog base | file-dialog base                                  |
|  [22]   | `FileDialog`               | property    | `Directory`/`FileName`/`Title`/`CheckFileExists`  |
|  [23]   | `FileDialog.Filters`       | property    | `FilterCollection<FileFilter>`                    |
|  [24]   | `FileDialog`               | property    | `CurrentFilter`/`CurrentFilterIndex`              |
|  [25]   | `FileDialog.ShowDialog`    | method      | `(Control)` presentation                          |
|  [26]   | `OpenFileDialog`           | dialog      | file-open picker                                  |
|  [27]   | `OpenFileDialog`           | property    | `MultiSelect`/`Filenames`                         |
|  [28]   | `SaveFileDialog`           | dialog      | file-save picker                                  |
|  [29]   | `SelectFolderDialog`       | dialog      | folder picker                                     |
|  [30]   | `SelectFolderDialog`       | property    | `Directory`/`Title`                               |
|  [31]   | `SelectFolderDialog`       | method      | `ShowDialog(Control)`                             |
|  [32]   | `ColorDialog`              | dialog      | native colour picker                              |
|  [33]   | `ColorDialog`              | property    | `Color`/`AllowAlpha`                              |
|  [34]   | `ColorDialog.ColorChanged` | event       | colour change                                     |
|  [35]   | `FontDialog`               | dialog      | native font picker                                |
|  [36]   | `FontDialog.Font`          | property    | selected font                                     |
|  [37]   | `FontDialog.FontChanged`   | event       | font change                                       |
|  [38]   | `FileFilter`               | model       | extension filter row for file dialogs             |
|  [39]   | `DialogResult`             | enum        | dialog-outcome vocabulary                         |
|  [40]   | `WindowState`              | enum        | window-state vocabulary                           |
|  [41]   | `WindowStyle`              | enum        | window-presentation vocabulary                    |
|  [42]   | `DialogDisplayMode`        | enum        | modal-display vocabulary                          |

[PUBLIC_TYPE_SCOPE]: menus and commands
- rail: native UI

| [INDEX] | [SYMBOL]                      | [KIND]   | [CAPABILITY]                            |
| :-----: | :---------------------------- | :------- | :-------------------------------------- |
|  [01]   | `ContextMenu`                 | menu     | popup menu                              |
|  [02]   | `ContextMenu.Items`           | property | `MenuItemCollection`                    |
|  [03]   | `ContextMenu.Trim`            | property | menu trim                               |
|  [04]   | `ContextMenu.Show`            | method   | `(Control, PointF)` presentation        |
|  [05]   | `ContextMenu`                 | event    | `Opening`/`Closing`/`Closed`            |
|  [06]   | `MenuItem`                    | menu     | leaf menu entry                         |
|  [07]   | `ButtonMenuItem`              | menu     | command-bound menu entry                |
|  [08]   | `SubMenuItem`                 | menu     | nested menu entry                       |
|  [09]   | `Command`                     | command  | reusable action                         |
|  [10]   | `Command`                     | property | `ID`/`MenuText`/`ToolBarText`/`ToolTip` |
|  [11]   | `Command`                     | property | `Enabled`/`Shortcut`/`DataContext`      |
|  [12]   | `Command.Execute`             | method   | action execution                        |
|  [13]   | `Command.Executed`            | event    | execution observation                   |
|  [14]   | `CheckCommand`                | command  | toggling command                        |
|  [15]   | `CheckCommand.Checked`        | property | toggled state                           |
|  [16]   | `CheckCommand.CheckedChanged` | event    | toggled-state change                    |
|  [17]   | `RadioCommand`                | command  | grouped exclusive command               |
|  [18]   | `RadioCommand.Controller`     | property | exclusivity controller                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: control lifecycle, input, and drag
- rail: native UI

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                                 | [CAPABILITY]                              |
| :-----: | :--------------------------------- | :------------------------------------------- | :---------------------------------------- |
|  [01]   | `Control.Focus`                    | `()`                                         | focus request                             |
|  [02]   | `Control.Invalidate`               | `()`                                         | repaint request                           |
|  [03]   | `Control.UpdateLayout`             | `()`                                         | re-measure request                        |
|  [04]   | `Control.CaptureMouse`             | `()` → `bool`                                | begins pointer capture                    |
|  [05]   | `Control.ReleaseMouseCapture`      | `()`                                         | ends pointer capture                      |
|  [06]   | `Control.DoDragDrop`               | `(IDataObject, DragEffects)` → `DragEffects` | starts a drag with a typed payload        |
|  [07]   | `Control.KeyDown`/`KeyUp`          | `EventHandler<KeyEventArgs>`                 | keyboard input                            |
|  [08]   | `Control.TextInput`                | `EventHandler<TextInputEventArgs>`           | composed-text input                       |
|  [09]   | `Control.MouseDown` … `MouseWheel` | `EventHandler<MouseEventArgs>`               | press/move/enter/leave/double-click/wheel |
|  [10]   | `Control.DragEnter` … `DragEnd`    | `EventHandler<DragEventArgs>`                | drag-over and drop lifecycle              |
|  [11]   | `Control.GotFocus`/`LostFocus`     | `EventHandler`                               | focus lifecycle                           |
|  [12]   | `Control.SizeChanged`              | `EventHandler`                               | geometry lifecycle                        |
|  [13]   | `Control.Load`/`Shown`             | `EventHandler`                               | attach lifecycle                          |

[ENTRYPOINT_SCOPE]: grid and tree selection, edit, and reload
- rail: native UI

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                               | [CAPABILITY]                      |
| :-----: | :------------------------------ | :----------------------------------------- | :-------------------------------- |
|  [01]   | `Grid.SelectRow`                | `(int)`                                    | selects a row                     |
|  [02]   | `Grid.UnselectRow`              | `(int)`                                    | unselects a row                   |
|  [03]   | `Grid.SelectAll`                | `()`                                       | selects every row                 |
|  [04]   | `Grid.UnselectAll`              | `()`                                       | unselects every row               |
|  [05]   | `Grid.BeginEdit`                | `(int row, int column)`                    | begins inline editing             |
|  [06]   | `Grid.CommitEdit`               | `()` → `bool`                              | commits inline editing            |
|  [07]   | `Grid.CancelEdit`               | `()` → `bool`                              | cancels inline editing            |
|  [08]   | `Grid.IsEditing`                | state                                      | gates the edit lifecycle          |
|  [09]   | `Grid.ScrollToRow`              | `(int)`                                    | brings a row into view            |
|  [10]   | `Grid.CellEditing`/`CellEdited` | event                                      | per-cell edit hooks               |
|  [11]   | `Grid.CellClick`                | event                                      | per-cell click hook               |
|  [12]   | `Grid.CellFormatting`           | event                                      | per-cell format hook              |
|  [13]   | `Grid.RowFormatting`            | event                                      | per-row format hook               |
|  [14]   | `TreeGridView.ReloadData`       | `()`                                       | refreshes while keeping selection |
|  [15]   | `TreeGridView.ReloadItem`       | `(ITreeGridItem, bool reloadChildren)`     | refreshes while keeping selection |
|  [16]   | `TreeGridView.GetCellAt`        | `(PointF)` → `TreeGridCell`                | hit-test resolution               |
|  [17]   | `TreeGridView.GetDragInfo`      | `(DragEventArgs)` → `TreeGridViewDragInfo` | drop-target resolution            |

[ENTRYPOINT_SCOPE]: window, dialog, and picker presentation
- rail: native UI

`Dialog<T>.Result` carries the typed outcome from either modal presentation surface.

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]                            | [CAPABILITY]            |
| :-----: | :------------------------------ | :-------------------------------------- | :---------------------- |
|  [01]   | `Form.Show`                     | `()`                                    | modeless window         |
|  [02]   | `Dialog.ShowModal`              | `(Control owner)`                       | modal loop              |
|  [03]   | `Dialog.ShowModalAsync`         | `()` → `Task`                           | asynchronous modal loop |
|  [04]   | `MessageBox.Show`               | `(Control, string, …)` → `DialogResult` | static prompt overload  |
|  [05]   | `FileDialog.ShowDialog`         | `(Control parent)` → `DialogResult`     | file open/save picker   |
|  [06]   | `SelectFolderDialog.ShowDialog` | `(Control parent)` → `DialogResult`     | folder picker           |
|  [07]   | `ColorDialog.ShowDialog`        | `(Control parent)` → `DialogResult`     | native colour picker    |
|  [08]   | `FontDialog.ShowDialog`         | `(Control parent)` → `DialogResult`     | native font picker      |
|  [09]   | `Window.Close`                  | `()`                                    | window teardown         |
|  [10]   | `Window.BringToFront`           | `()`                                    | z-order promotion       |
|  [11]   | `Window.SetOwner`               | `(Window)`                              | owner assignment        |

[ENTRYPOINT_SCOPE]: layout composition and command dispatch
- rail: native UI

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]              | [CAPABILITY]             |
| :-----: | :------------------------------ | :------------------------ | :----------------------- |
|  [01]   | `DynamicLayout.AddRow`          | `(params Control[])`      | fluent row composition   |
|  [02]   | `DynamicLayout.AddSeparateRow`  | `(params Control[])`      | separate-row composition |
|  [03]   | `DynamicLayout.BeginVertical`   | `()`                      | begins vertical scope    |
|  [04]   | `DynamicLayout.EndVertical`     | `()`                      | ends vertical scope      |
|  [05]   | `DynamicLayout.BeginHorizontal` | `()`                      | begins horizontal scope  |
|  [06]   | `DynamicLayout.EndHorizontal`   | `()`                      | ends horizontal scope    |
|  [07]   | `TableLayout.Add`               | `(Control, int x, int y)` | fixed-cell placement     |
|  [08]   | `TableLayout.Move`              | `(Control, int x, int y)` | fixed-cell movement      |
|  [09]   | `TableLayout.SetColumnScale`    | `(int, bool)`             | column scaling           |
|  [10]   | `TableLayout.SetRowScale`       | `(int, bool)`             | row scaling              |
|  [11]   | `PixelLayout.Add`               | `(Control, int x, int y)` | absolute placement       |
|  [12]   | `PixelLayout.Move`              | `(Control, int x, int y)` | absolute movement        |
|  [13]   | `PixelLayout.GetLocation`       | `(Control)` → `Point`     | location lookup          |
|  [14]   | `ContextMenu.Show`              | `(Control, PointF)`       | canvas-point popup       |
|  [15]   | `Command.Execute`               | `()`                      | action dispatch          |
|  [16]   | `Command.Executed`              | `EventHandler`            | action observation       |

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
