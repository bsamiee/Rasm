# [RASM_RHINO_API_ETO_FORMS]

`Eto.Forms` owns the native cross-platform widget surface the Rhino process embeds: one host-loaded `Eto.dll` resolves every control, layout region, window, dialog, menu, and command through the ambient platform handler. Binding threads a control property to a `DataContext` over the `IndirectBinding`/`DirectBinding`/`BindableBinding`/`DualBinding` rail, so a generator-shaped UI layer folds each widget, region, and command as a row rather than a hand-wired call site.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.Forms`
- package: `Eto.Forms` (BSD-3-Clause, host-provided)
- assembly: `Eto.dll` (Rhino `RhCore` framework)
- namespace: `Eto.Forms`, `Eto.Forms.ThemedControls`, `Eto.Threading`
- rail: native-ui

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: value and text input controls

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :--------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `TextBox`        | text input    | single-line editable text                            |
|  [02]   | `TextArea`       | text input    | multi-line editable text with wrap and caret control |
|  [03]   | `RichTextArea`   | text input    | styled rich-text editor over a formatted buffer      |
|  [04]   | `PasswordBox`    | text input    | masked secret entry                                  |
|  [05]   | `SearchBox`      | text input    | search-styled text field with clear affordance       |
|  [06]   | `NumericStepper` | value input   | bounded numeric entry with increment stepping        |
|  [07]   | `Spinner`        | indicator     | indeterminate activity spinner                       |
|  [08]   | `Slider`         | value input   | ranged track selector                                |
|  [09]   | `DateTimePicker` | value input   | date and time selection                              |
|  [10]   | `Calendar`       | value input   | month-grid date selection                            |
|  [11]   | `ColorPicker`    | value input   | native colour selection well                         |
|  [12]   | `FontPicker`     | value input   | native font selection well                           |
|  [13]   | `FilePicker`     | value input   | inline file/folder path selector                     |

[PUBLIC_TYPE_SCOPE]: choice, command, and media controls

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                |
| :-----: | :---------------- | :------------ | :------------------------------------------ |
|  [01]   | `CheckBox`        | choice        | tri-state boolean toggle                    |
|  [02]   | `RadioButton`     | choice        | mutually-exclusive selection within a group |
|  [03]   | `DropDown`        | choice        | single-selection collapsed list             |
|  [04]   | `ComboBox`        | choice        | editable-text dropdown                      |
|  [05]   | `ListBox`         | choice        | scrollable single-selection list            |
|  [06]   | `CheckBoxList`    | choice        | multi-check option group                    |
|  [07]   | `SegmentedButton` | choice        | linked multi-segment toggle bar             |
|  [08]   | `Button`          | command       | push-command control                        |
|  [09]   | `LinkButton`      | command       | hyperlink-styled command                    |
|  [10]   | `Label`           | display       | static or wrapping text label               |
|  [11]   | `ImageView`       | display       | static image presenter                      |
|  [12]   | `ProgressBar`     | indicator     | determinate/indeterminate progress track    |

[PUBLIC_TYPE_SCOPE]: container and host controls

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :------------------ | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `Panel`             | container     | single-child content host                                               |
|  [02]   | `GroupBox`          | container     | titled bordered content frame                                           |
|  [03]   | `Expander`          | container     | collapsible header/content region                                       |
|  [04]   | `Scrollable`        | container     | scrolling viewport over oversized content                               |
|  [05]   | `Splitter`          | container     | two-pane draggable split with `SplitterFixedPanel` sizing policy        |
|  [06]   | `TabControl`        | container     | tabbed page host                                                        |
|  [07]   | `DocumentControl`   | container     | closable-tab document host over `DocumentPage`                          |
|  [08]   | `Drawable`          | container     | custom-paint host issuing `Eto.Drawing.Graphics` (`api-eto-drawing.md`) |
|  [09]   | `WebView`           | container     | embedded browser surface                                                |
|  [10]   | `PropertyGrid`      | container     | reflected property editor over a bound object                           |
|  [11]   | `NativeControlHost` | container     | host-native control embedding (`api-eto-platform.md`)                   |

[PUBLIC_TYPE_SCOPE]: grid, tree, and cell families

`GridView` binds a flat `DataStore`, `TreeGridView` an `ITreeGridStore<ITreeGridItem>` hierarchy, `TreeView` an `ITreeItem` tree; a `GridColumn` carries one `Cell`, and the cell kind selects the in-cell editor.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :-------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `GridView`      | grid          | flat data grid over an ordered store                        |
|  [02]   | `TreeGridView`  | grid          | hierarchical grid over `ITreeGridItem` nodes                |
|  [03]   | `TreeView`      | tree          | node tree over `ITreeItem`                                  |
|  [04]   | `GridColumn`    | grid part     | one bound column carrying a `Cell`                          |
|  [05]   | `GridItem`      | grid part     | mutable flat row backing store item                         |
|  [06]   | `TreeGridItem`  | grid part     | mutable tree node with children, implements `ITreeGridItem` |
|  [07]   | `ITreeGridItem` | contract      | tree-grid node contract (parent, expanded, children)        |
|  [08]   | `TextBoxCell`   | cell          | inline editable-text cell                                   |
|  [09]   | `CheckBoxCell`  | cell          | inline boolean cell                                         |
|  [10]   | `ComboBoxCell`  | cell          | inline dropdown cell                                        |
|  [11]   | `ImageViewCell` | cell          | inline image cell                                           |
|  [12]   | `ImageTextCell` | cell          | combined image + text cell                                  |
|  [13]   | `ProgressCell`  | cell          | inline progress-bar cell                                    |
|  [14]   | `DrawableCell`  | cell          | owner-drawn cell issuing `Graphics`                         |
|  [15]   | `CustomCell`    | cell          | control-hosting cell over an arbitrary child                |

[PUBLIC_TYPE_SCOPE]: control base and event surface

`Control` is the common base carrying invalidation and native-attachment members and the event families every widget inherits: `Load`/`Shown` lifecycle, `GotFocus`/`LostFocus` focus, `MouseDown`/`MouseMove` mouse, `KeyDown` key, and `DragEnter`/`DragDrop` drag-drop.

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :----------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `Control`                            | base          | widget base carrying the inherited event families                 |
|  [02]   | `Control.Invalidate()`               | member        | full-surface repaint request                                      |
|  [03]   | `Control.Invalidate(Rectangle rect)` | member        | bounded-region repaint request                                    |
|  [04]   | `Control.AttachNative()`             | member        | attaches to an external native parent (`api-eto-platform.md`)     |
|  [05]   | `Control.DetachNative()`             | member        | detaches from the native parent                                   |
|  [06]   | `Control.TriggerStyleChanged()`      | member        | re-applies style handlers on theme change (`api-eto-platform.md`) |

`DoDragDrop` begins a drag with a `DataObject` payload, optionally carrying a drag image:

```csharp signature
Control.DoDragDrop(DataObject data, DragEffects allowedEffects)
Control.DoDragDrop(DataObject data, DragEffects allowedEffects, Image image, PointF cursorOffset)
```

[PUBLIC_TYPE_SCOPE]: layout containers

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                                               |
| :-----: | :---------------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `DynamicLayout`   | layout        | nested vertical/horizontal/group/scrollable region builder |
|  [02]   | `TableLayout`     | layout        | scaling cell grid over `TableRow`/`TableCell`              |
|  [03]   | `TableRow`        | layout part   | one grid row of cells with scale flags                     |
|  [04]   | `TableCell`       | layout part   | one grid cell with x-scale flag                            |
|  [05]   | `StackLayout`     | layout        | linear run over `StackLayoutItem` with alignment           |
|  [06]   | `StackLayoutItem` | layout part   | one stacked child with expand flag                         |
|  [07]   | `PixelLayout`     | layout        | absolute pixel-positioned placement                        |
|  [08]   | `Orientation`     | enum          | `Horizontal`/`Vertical` axis selector                      |
|  [09]   | `Padding`         | value         | four-edge inset value                                      |

[PUBLIC_TYPE_SCOPE]: windows, dialogs, and file/colour/font choosers

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                          |
| :-----: | :------------------- | :------------ | :-------------------------------------------------------------------- |
|  [01]   | `Window`             | window        | top-level window base with state/location/logical-pixel-size events   |
|  [02]   | `Form`               | window        | modeless top-level form                                               |
|  [03]   | `FloatingForm`       | window        | always-on-top utility form                                            |
|  [04]   | `Dialog<T>`          | window        | modal dialog returning a typed result through `Close(T)`              |
|  [05]   | `DialogResult`       | enum          | standard modal outcome vocabulary                                     |
|  [06]   | `WindowStyle`        | enum          | native chrome style selector                                          |
|  [07]   | `DialogDisplayMode`  | enum          | modal presentation policy                                             |
|  [08]   | `MessageBox`         | dialog        | static message presentation over `MessageBoxType`/`MessageBoxButtons` |
|  [09]   | `OpenFileDialog`     | dialog        | file-open chooser over `FileFilter`                                   |
|  [10]   | `SaveFileDialog`     | dialog        | file-save chooser                                                     |
|  [11]   | `SelectFolderDialog` | dialog        | folder chooser                                                        |
|  [12]   | `FileDialog`         | dialog        | file-chooser base carrying `Filters`                                  |
|  [13]   | `FileFilter`         | value         | file-extension filter row                                             |
|  [14]   | `ColorDialog`        | dialog        | native colour chooser                                                 |
|  [15]   | `FontDialog`         | dialog        | native font chooser                                                   |

[PUBLIC_TYPE_SCOPE]: menus, toolbars, and commands

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :------------------ | :------------ | :----------------------------------------------------------- |
|  [01]   | `MenuBar`           | menu          | top-level application menu                                   |
|  [02]   | `ButtonMenuItem`    | menu item     | invoking menu entry with submenu children                    |
|  [03]   | `CheckMenuItem`     | menu item     | checkable menu entry                                         |
|  [04]   | `RadioMenuItem`     | menu item     | radio-grouped menu entry                                     |
|  [05]   | `SeparatorMenuItem` | menu item     | menu divider                                                 |
|  [06]   | `SubMenuItem`       | menu item     | nested submenu container                                     |
|  [07]   | `ContextMenu`       | menu          | popup menu bound to a control                                |
|  [08]   | `ToolBar`           | toolbar       | control toolbar over `ToolItem` entries                      |
|  [09]   | `ButtonToolItem`    | tool item     | invoking toolbar button                                      |
|  [10]   | `CheckToolItem`     | tool item     | toggle toolbar button                                        |
|  [11]   | `DropDownToolItem`  | tool item     | toolbar button with a dropdown menu                          |
|  [12]   | `SeparatorToolItem` | tool item     | toolbar divider                                              |
|  [13]   | `Command`           | command       | shared invocation with `Enabled`, `Shortcut`, and `Executed` |
|  [14]   | `CheckCommand`      | command       | toggle command projected to check menu/tool items            |
|  [15]   | `RadioCommand`      | command       | radio-grouped command                                        |

[PUBLIC_TYPE_SCOPE]: data binding

`IndirectBinding<T>` reads and writes a value against an arbitrary data item and chains through `Convert`/`Child`/`AfterDelay`; `BindableBinding<T,TValue>` binds a control property to a `DataContext`; `DualBinding<T>` is the two-way link governed by a `DualBindingMode` policy.

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :-------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `IndirectBinding<T>`        | binding       | value binding against a supplied data item, chainable           |
|  [02]   | `DirectBinding<T>`          | binding       | value binding against a fixed source with change notification   |
|  [03]   | `BindableBinding<T,TValue>` | binding       | control-property binding to a `DataContext` source              |
|  [04]   | `DualBinding<T>`            | binding       | two-way source/destination link                                 |
|  [05]   | `DualBindingMode`           | enum          | `OneWay`/`OneWayToSource`/`TwoWay`/`OneTime` propagation policy |
|  [06]   | `BindableExtensions`        | extension     | `Bind`/`BindDataContext` fluent entry over any bindable widget  |
|  [07]   | `DataObject`                | transfer      | typed drag/clipboard payload consumed by `DoDragDrop`           |
|  [08]   | `DragEffects`               | enum          | permitted drag operation flags                                  |

[PUBLIC_TYPE_SCOPE]: themed dialogs and editors

`Eto.Forms.ThemedControls` mints the custom-drawn, cross-platform-uniform control family; its `Themed*Handler` backend classes register through the platform-handler seam (`api-eto-platform.md`), never a widget-construction row.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `ThemedMessageBox`       | dialog        | themed modal message box with arbitrary result-typed buttons    |
|  [02]   | `ThemedPropertyGrid`     | control       | themed reflected property editor over one or many bound objects |
|  [03]   | `ThemedCollectionEditor` | control       | themed add/remove editor over a homogeneous object collection   |
|  [04]   | `ThemedSpinnerMode`      | enum          | themed-spinner glyph shape (`Line`/`Circle`)                    |
|  [05]   | `ThemedSpinnerDirection` | enum          | themed-spinner rotation (`Clockwise`/`CounterClockwise`)        |

[PUBLIC_TYPE_SCOPE]: thread marshal

`Eto.Threading.Thread` is the managed thread abstraction carrying main-thread identity.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `Thread` | thread        | managed thread with `IsMain`/`MainThread` main-thread identity |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DynamicLayout region building

`Begin*` builders open a nested scope; `Add*` members place children into the open region:

```csharp signature
DynamicLayout.BeginVertical(Padding? padding = null, Size? spacing = null, bool? xscale = null, bool? yscale = null)
DynamicLayout.BeginHorizontal(bool? yscale = null)
DynamicLayout.BeginGroup(string title, Padding? padding = null, Size? spacing = null, bool? xscale = null, bool? yscale = null)
DynamicLayout.BeginScrollable(BorderType border = BorderType.Bezel, Padding? padding = null, Size? spacing = null, bool? xscale = null, bool? yscale = null)
DynamicLayout.Add(Control control, bool? xscale = null, bool? yscale = null)
DynamicLayout.AddRow(params Control[] controls)
DynamicLayout.AddColumn(params Control[] controls)
DynamicLayout.AddCentered(Control control, Padding? padding = null, Size? spacing = null, bool? xscale = null, bool? yscale = null, bool horizontalCenter = true, bool verticalCenter = true)
DynamicLayout.AddAutoSized(Control control, Padding? padding = null, Size? spacing = null, bool? xscale = null, bool? yscale = null, bool centered = false)
```

[ENTRYPOINT_SCOPE]: TableLayout and PixelLayout placement

| [INDEX] | [SURFACE]                                                                                | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------------------------------- | :--------------------------------------- |
|  [01]   | `TableLayout.Horizontal(params TableCell[] cells)`                                       | builds a single horizontal row table     |
|  [02]   | `TableLayout.HorizontalScaled(params TableCell[] cells)`                                 | builds an evenly-scaled horizontal table |
|  [03]   | `TableLayout.AutoSized(Control control, Padding? padding = null, bool centered = false)` | wraps one control at natural size        |
|  [04]   | `TableLayout.Add(Control control, int x, int y, bool xscale, bool yscale)`               | places a control at a grid cell          |
|  [05]   | `TableLayout.SetColumnScale(int column, bool scale = true)`                              | marks a column as scaling                |
|  [06]   | `TableLayout.SetRowScale(int row, bool scale = true)`                                    | marks a row as scaling                   |
|  [07]   | `PixelLayout.Add(Control control, int x, int y)`                                         | places a control at an absolute pixel    |
|  [08]   | `PixelLayout.Move(Control control, int x, int y)`                                        | repositions a placed control             |

[ENTRYPOINT_SCOPE]: control configuration and owned collections

| [INDEX] | [SURFACE]                                                                           | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------------------------------- | :----------------------------------------------- |
|  [01]   | `TextArea.Wrap { get; set; }`                                                       | controls line wrapping                           |
|  [02]   | `Calendar.MinDate { get; set; }` / `Calendar.MaxDate { get; set; }`                 | bounds selectable calendar dates                 |
|  [03]   | `FilePicker.Filters { get; }`                                                       | owns the mutable file-filter collection          |
|  [04]   | `GridItem(params object[] values)` / `GridItem.Tag { get; set; }`                   | constructs a row and retains typed identity      |
|  [05]   | `TreeGridItem(IEnumerable<ITreeGridItem> children, params object[] values)`         | constructs one hierarchical grid row             |
|  [06]   | `StackLayoutItem(Control control, bool expand = false)`                             | places one linear-layout child                   |
|  [07]   | `TableRow(IEnumerable<TableCell> cells)`                                            | constructs one table placement row               |
|  [08]   | `TableCell(Control control, bool scaleWidth = false)`                               | places one control in a table cell               |
|  [09]   | `CheckBox.ThreeState { get; set; }` / `CheckBox.Checked { get; set; }`              | selects binary or nullable toggle semantics      |
|  [10]   | `PropertyGrid.ShowCategories { get; set; }`                                         | selects categorized property presentation        |
|  [11]   | `GridColumn.Width / Resizable / AutoSize / Visible { get; set; }`                   | column width, resize, autosize, visibility       |
|  [12]   | `Splitter.FixedPanel { get; set; }` (`SplitterFixedPanel` `Panel1`/`Panel2`/`None`) | selects which pane keeps its size on resize      |
|  [13]   | `DocumentControl.Pages / SelectedIndex / AllowReordering { get; set; }`             | closable, reorderable document-tab host          |
|  [14]   | `DocumentPage.Content / Text / Closable { get; set; }`                              | one closable document tab over a content control |
|  [15]   | `Window.Location / WindowState / Icon { get; set; }`                                | window placement, min/max state, menu-bar icon   |

[ENTRYPOINT_SCOPE]: modal dialog result

| [INDEX] | [SURFACE]                                    | [SHAPE]                   | [CAPABILITY]                                |
| :-----: | :------------------------------------------- | :------------------------ | :------------------------------------------ |
|  [01]   | `Dialog<T>.ShowModal(Control owner)`         | instance call → `T`       | shows modal and blocks for the typed result |
|  [02]   | `Dialog<T>.ShowModalAsync(Control owner)`    | instance call → `Task<T>` | shows modal and awaits the typed result     |
|  [03]   | `Dialog<T>.Close(T result)`                  | instance call             | closes and sets the returned result         |
|  [04]   | `CommonDialog.ShowDialog(Control parent)`    | call → `DialogResult`     | shows against an owning window              |
|  [05]   | `FontDialog.Font { get; set; }`              | property                  | seeds and returns the selected font         |
|  [06]   | `SelectFolderDialog.Title { get; set; }`     | property                  | configures the chooser title                |
|  [07]   | `SelectFolderDialog.Directory { get; set; }` | property                  | seeds and returns the selected path         |

[ENTRYPOINT_SCOPE]: command projection and invocation

| [INDEX] | [SURFACE]                  | [SHAPE]                    | [CAPABILITY]                               |
| :-----: | :------------------------- | :------------------------- | :----------------------------------------- |
|  [01]   | `Command.Execute()`        | instance call              | raises the shared execution event          |
|  [02]   | `Command.CreateMenuItem()` | instance call → `MenuItem` | projects the command into one menu item    |
|  [03]   | `Command.CreateToolItem()` | instance call → `ToolItem` | projects the command into one toolbar item |

[ENTRYPOINT_SCOPE]: binding construction and transform

`Bind`/`BindDataContext` open a binding against a fixed source or the `DataContext`; the `IndirectBinding<T>` chain transforms it before it reaches the control:

```csharp signature
BindableExtensions.Bind<TWidget, TSource, TValue>(TWidget control, Expression<Func<TWidget, TValue>> controlProperty, TSource source, Expression<Func<TSource, TValue>> sourceProperty, DualBindingMode mode = DualBindingMode.TwoWay)
BindableExtensions.BindDataContext<TWidget, TContext, TValue>(TWidget control, Expression<Func<TWidget, TValue>> controlProperty, Expression<Func<TContext, TValue>> sourceProperty, DualBindingMode mode = DualBindingMode.TwoWay, TValue defaultControlValue = default, TValue defaultContextValue = default)
BindableBinding<T, TValue>.BindDataContext(IndirectBinding<TValue> dataContextBinding, DualBindingMode mode = DualBindingMode.TwoWay, TValue defaultControlValue = default, TValue defaultContextValue = default)
```

| [INDEX] | [SURFACE]                                                                                     | [CAPABILITY]                       |
| :-----: | :-------------------------------------------------------------------------------------------- | :--------------------------------- |
|  [01]   | `IndirectBinding<T>.GetValue(object dataItem)`                                                | reads the value from a data item   |
|  [02]   | `IndirectBinding<T>.SetValue(object dataItem, T value)`                                       | writes the value to a data item    |
|  [03]   | `IndirectBinding<T>.Convert<TValue>(Func<T,TValue> toValue, Func<TValue,T> fromValue = null)` | maps the bound value type          |
|  [04]   | `IndirectBinding<T>.Child<TNewValue>(Expression<Func<T,TNewValue>> property)`                 | descends into a bound-value member |
|  [05]   | `IndirectBinding<T>.AfterDelay(TimeSpan delay, bool reset = false)`                           | debounces write propagation        |

In every `Convert`/`Delegate` setter the first lambda argument is the source-side VALUE (or data item for the two-generic `Delegate`), never the binding instance:

```csharp signature
Binding.Delegate<TValue>(Func<TValue> getValue, Action<TValue> setValue = null, Action<EventHandler<EventArgs>> addChangeEvent = null, Action<EventHandler<EventArgs>> removeChangeEvent = null)
Binding.Delegate<T, TValue>(Func<T, TValue> getValue, Action<T, TValue> setValue = null, Action<T, EventHandler<EventArgs>> addChangeEvent = null, Action<T, EventHandler<EventArgs>> removeChangeEvent = null, TValue defaultGetValue = default, TValue defaultSetValue = default)
Binding.Property<T, TValue>(Expression<Func<T, TValue>> propertyExpression)
DirectBinding<T>.DataValue { get; set; }
DirectBinding<T>.DataValueChanged (event EventHandler<EventArgs>)
DirectBinding<T>.Convert<TValue>(Func<T, TValue> getValue, Action<T, TValue> setValue)
DirectBinding<T>.CatchException(Func<Exception, bool> exceptionHandler = null)
IndirectBinding<T>.Convert<TValue>(Func<T, TValue> getValue, Action<T, TValue> setValue)
IndirectBinding<T>.CatchException(Func<Exception, bool> exceptionHandler = null)
BindableBinding<T, TValue>.Bind(DirectBinding<TValue> sourceBinding, DualBindingMode mode = DualBindingMode.TwoWay)
DualBinding<T>.Unbind()
DualBinding<T>.Update(BindingUpdateMode mode = BindingUpdateMode.Destination)
BindableWidget.UpdateBindings(BindingUpdateMode mode = BindingUpdateMode.Source)
```

`CatchException` lives on `DirectBinding<T>`/`IndirectBinding<T>` and returns that binding shape; `BindableBinding` carries no such member, so the funnel attaches to the source side of a dual link, never to the control selector.

[ENTRYPOINT_SCOPE]: themed controls and thread identity

```csharp signature
ThemedMessageBox.AddButton(string text, object result, bool isDefault = false, bool isAbort = false)
ThemedMessageBox.Result / Text / TextAlignment / Image { get; set; }
ThemedPropertyGrid.SelectedObject / SelectedObjects / ShowCategories / ShowDescription { get; set; }
ThemedPropertyGrid.Refresh() ; event PropertyValueChanged
ThemedCollectionEditor.DataStore / ElementType / ExtraContent { get; set; }
Thread(Action action) ; Start() ; Abort() ; IsMain ; IsAlive
Thread.MainThread / CurrentThread / IsMainThread (static)
```

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every widget derives from `Control` and resolves a backend handler through the ambient platform; one construction row produces a native control on each host, and host divergence lives in the handler, never in the row.
- Layout owns four orthogonal placement strategies — `DynamicLayout` nested regions, `TableLayout` grid cells, `StackLayout` one-axis run, `PixelLayout` absolute — composed per screen, never merged.
- `GridView`/`TreeGridView`/`TreeView` separate the bound store from the `GridColumn`+`Cell` presentation, and one column definition drives every row.
- Binding is bidirectional through `IndirectBinding<T>` with a `DualBindingMode`; `Convert`/`Child`/`AfterDelay` chain the transform, and `BindDataContext` reuses one binding graph across every data item swapped into `DataContext`.
- One `Command` projects into both a menu item and a tool item, so a command row drives the menu bar, the context menu, and the toolbar from a single enablement and shortcut definition.

[STACKING]:
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): a `[SmartEnum]` owns the closed control-kind, cell-kind, layout-strategy, and dialog-outcome vocabularies a generator-shaped UI layer folds to rows, and a `[Union]` owns the discriminated screen-element tree; the generated `Switch`/`Map` drives construction dispatch instead of a hand-written control-type ladder.
- `LanguageExt.Core`(`../../.api/api-languageext.md`): `Fin<A>` rails `ShowModal`/`ShowModalAsync` outcomes and file-dialog results (cancellation is a `Fail`, never a null sentinel); `Option<A>` carries the nullable `bool?` scale flags and optional selection; `Eff<A>` wraps `DoDragDrop` and native-attach effects; `Seq<A>` is the child-collection carrier a layout region folds over.
- `Wacton.Unicolour`(`../../.api/api-unicolour.md`): the canonical colour value behind `ColorPicker` and `ColorDialog`; an `Eto.Drawing.Color` maps to and from `Unicolour` at the view edge (`api-eto-drawing.md`), keeping theme ramps and perceptual selection in the perceptual model.
- `api-eto-platform.md`: `NativeControlHost`, `Control.AttachNative`/`DetachNative`, and `TriggerStyleChanged` cross into the platform-handler and theme-transition seam, and the `Themed*Handler` backend classes register through `Platform.Add<TWidget.IHandler>` at that same seam.

[LOCAL_ADMISSION]:
- Eto is admitted from the Rhino-loaded `Eto.dll`; Rasm.Rhino references that instance so its widgets share the host application, dispatcher, and platform handler, and a second copy never enters through NuGet.
- A screen is built once from generated element rows against these construction, layout, and binding surfaces; `Eto.Forms.*` types stay behind the Rasm.Rhino UI owner, and downstream code composes screen definitions rather than raw widget calls.
- `Eto.Threading.Thread` stays subordinate to the Rhino host marshal owner (`RhinoApp.InvokeOnUiThread`/`InvokeAndWait`, `api-rhino-ui.md`); an Eto-level `Thread.IsMainThread` test never replaces the host marshal seam.

[RAIL_LAW]:
- Package: `Eto.Forms`
- Owns: the native widget roster, the cell/item/grid families, the layout containers, the window/dialog/menu/toolbar/command hierarchy, the `IndirectBinding`/`DirectBinding`/`BindableBinding`/`DualBinding` data-binding surface, the `Eto.Forms.ThemedControls` themed dialog/editor family, and `Eto.Threading.Thread` main-thread identity.
- Accept: native UI construction, layout composition, modal/modeless presentation, menu and command chrome, control-to-model binding through `DataContext`, and a themed message-box, property-grid, or collection-editor over the construction surface.
- Reject: immediate 2D painting (`api-eto-drawing.md` owns `Graphics`), platform-handler and native-hosting selection plus the `Themed*Handler` backend classes (`api-eto-platform.md`), document-owned Rhino windows and panels (`api-rhino-ui.md`), the `Eto.IO` icon surface (`api-eto-drawing.md` owns `SystemImages`), and leaking `Eto.Forms.*` types past the UI owner.
