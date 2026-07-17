# [RASM_RHINO_API_RHINO_UI_CONTROLS]

`Rhino.UI.dll` carries Rhino's own reusable Eto control library: the layered collapsible-section family that hosts RDK and properties-page sections, the padding/spacing-typed layout and panel controls, the button/label/divider/colour widgets, the unit-parsing numeric and rich-text inputs, the embeddable viewport control, and the range and content-menu dialogs — plus the `Rhino.UI.Forms` dialog bases and named-colour palette, the read-only `Rhino.UI.Theme` colour-model tree, and the `Rhino.UI.Runtime` platform-service contracts. Every widget is an `Eto.Forms` control specialized with Rhino styling and settings, so `api-eto-forms.md` owns the base `Eto.Forms` roster while this catalog owns the Rhino-specific control library the host bridge of `api-rhino-ui.md` seats inside a panel, page, or dialog. Access is the ruling filter: `Rhino.UI.Controls` is largely internal dialog and panel implementation, so only the genuinely-public, constructible or subclassable surface lands, and the internal panel namespaces (`Rhino.UI.DialogPanels`, `ViewModels`, `ObjectProperties`, `ObjectManager`, `Annotations`) contribute an exclusion note, never a row.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Rhino.UI.dll` control library
- package: `RhinoCommon` (with `Rhino.UI` companion assembly)
- license: proprietary McNeel SDK (host-provided, not centrally pinned)
- assembly: `Rhino.UI.dll` (reusable Eto control library, themed dialogs, theme model, platform-service contracts)
- namespace: `Rhino.UI.Controls` (collapsible-section family, layouts, panels, buttons, labels, colour, list/grid, numeric/text, viewport, dialogs, menu)
- namespace: `Rhino.UI.Forms` (reusable dialog bases, the named-colour palette, and the print/PDF export façade)
- namespace: `Rhino.UI.Theme` (read-only zone/element/state colour-model tree)
- namespace: `Rhino.UI.Runtime` (platform-service, toolbar-service, and service-locator contracts)
- asset: host-resolved managed reference; the boundary composes it, the manifest never pins it
- verify: `tools.assay api --key rhino-ui` decompiles `Rhino.UI.dll`; `Rhino.UI.Controls` is heavily internal, so every row here is decompile-access-verified and the internal mass is excluded
- rail: host-boundary native-ui

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: collapsible-section family
- namespace: `Rhino.UI.Controls`
- rail: host-boundary native-ui

A plugin subclasses one section base and adds it to a holder; `EtoCollapsibleSection` is the leaf section, the `2`/`3` bases add holder-attach lifecycle and a refresh hook, `EtoContentUISection*` binds a `RenderContentCollection`, and `EtoPostEffectCollapsibleSection` binds a post-effect. `api-rhino-ui.md` owns the page-section hosting seam (`EtoCollapsibleSection`(+`Holder`) as a page's section stack); this catalog owns the full family as the control library.

| [INDEX] | [SYMBOL]                          | [KIND]        | [CAPABILITY]                                                    |
| :-----: | :-------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `EtoCollapsibleSection`           | abstract base | one collapsible RDK/properties page section                     |
|  [02]   | `EtoCollapsibleSection2`          | abstract base | section plus holder attach/detach lifecycle and header buttons  |
|  [03]   | `EtoCollapsibleSection3`          | abstract base | section2 plus an `UpdateView(uint)` refresh hook                |
|  [04]   | `EtoContentUISection`             | abstract base | render-content section with `RenderContentCollection` selection |
|  [05]   | `EtoContentUISection2`            | abstract base | content section on the `2` lifecycle base                       |
|  [06]   | `EtoContentUISection3`            | abstract base | content section on the `3` lifecycle base                       |
|  [07]   | `EtoPostEffectCollapsibleSection` | abstract base | post-effect section with parameter get/set and effect list      |
|  [08]   | `EtoCollapsibleSectionHolder`     | section host  | stacks `ICollapsibleSection`s with scrollbars and check boxes   |
|  [09]   | `EtoCollapsibleSectionHolder2`    | section host  | holder plus per-section header button and a full-height section |

[PUBLIC_TYPE_SCOPE]: layout controls
- namespace: `Rhino.UI.Controls`
- rail: host-boundary native-ui

`RhinoLayout` is the static padding/spacing/size vocabulary and label-row/table factory every Rhino-styled layout reads; the table and stack layouts and the wrapping control grid carry that vocabulary in their constructors.

| [INDEX] | [SYMBOL]                         | [KIND]                | [CAPABILITY]                                                   |
| :-----: | :------------------------------- | :-------------------- | :------------------------------------------------------------- |
|  [01]   | `RhinoLayout`                    | static helper         | padding/spacing/size vocabulary and label-row/table factories  |
|  [02]   | `ControlGridLayout`              | control (Panel)       | wrapping grid of equal-size controls, horizontal/vertical wrap |
|  [03]   | `TopRowButtonLayout`             | control               | `ControlGridLayout` specialized for a top button row           |
|  [04]   | `RhinoTableLayout`               | control (TableLayout) | table layout carrying Rhino padding/spacing types              |
|  [05]   | `RhinoDialogTableLayout`         | control               | table layout for dialog panels                                 |
|  [06]   | `RhinoIndentedTableLayout`       | control               | indented table layout                                          |
|  [07]   | `RhinoNestedTableLayout`         | control               | nested table layout with a rows-params constructor             |
|  [08]   | `RhinoPanelTableLayout`          | control               | table layout for panels                                        |
|  [09]   | `RhinoPropertiesPageTableLayout` | control               | table layout for properties pages                              |
|  [10]   | `RhinoButtonTableLayout`         | control               | table layout of buttons                                        |
|  [11]   | `RhinoButtonStackLayout`         | control (StackLayout) | stack of buttons with orientation and spacing type             |
|  [12]   | `RhinoNestedStackLayout`         | control               | nested stack layout                                            |
|  [13]   | `RhinoPanelStackLayout`          | control               | stack layout for panels                                        |

[PUBLIC_TYPE_SCOPE]: panel, host, and scrollable containers
- namespace: `Rhino.UI.Controls`
- rail: host-boundary native-ui

| [INDEX] | [SYMBOL]                     | [KIND]               | [CAPABILITY]                                        |
| :-----: | :--------------------------- | :------------------- | :-------------------------------------------------- |
|  [01]   | `RhinoGroupBox`              | control (Panel)      | group box with styled background, content, padding  |
|  [02]   | `RhinoDialogPanel`           | control (Panel)      | dialog-styled panel                                 |
|  [03]   | `RhinoIndentedPanel`         | control (Panel)      | indented panel                                      |
|  [04]   | `PanelWithBorder`            | control (Panel)      | panel with configurable border colour and thickness |
|  [05]   | `RhinoPanelScrollable`       | control (Scrollable) | Rhino-styled scrollable panel                       |
|  [06]   | `RhinoScrollableDialogPanel` | control (Scrollable) | scrollable dialog panel                             |

[PUBLIC_TYPE_SCOPE]: buttons, labels, dividers, and colour
- namespace: `Rhino.UI.Controls`
- rail: host-boundary native-ui

| [INDEX] | [SYMBOL]                     | [KIND]                    | [CAPABILITY]                                                |
| :-----: | :--------------------------- | :------------------------ | :---------------------------------------------------------- |
|  [01]   | `AddRemoveButton`            | control (SegmentedButton) | add/remove segmented button with commands and events        |
|  [02]   | `ImageButton`                | control (Drawable)        | image button bound to `ICommand` with disabled image        |
|  [03]   | `ImageToolTipButton`         | control                   | `ImageButton` with dual left/right tooltips                 |
|  [04]   | `ButtonDrawable`             | control (Drawable)        | header-button drawable over an `IHeaderButtonHandler`       |
|  [05]   | `RhinoButtonRow`             | control (StackLayout)     | row of buttons with `AddButton` overloads                   |
|  [06]   | `Divider`                    | control (Drawable)        | 1px auto-orient divider line                                |
|  [07]   | `LabelSeparator`             | control (Panel)           | label plus separator line                                   |
|  [08]   | `StaticAlignedLabel`         | control (Label)           | label with a fixed text alignment                           |
|  [09]   | `DisplayAndPrintColorPicker` | control (Panel)           | dual display/print colour picker with source modes and link |

[PUBLIC_TYPE_SCOPE]: list, grid, numeric, and text controls
- namespace: `Rhino.UI.Controls`
- rail: host-boundary native-ui

`NumericUpDownWithUnitParsing` and its `UnitParsingMaskedTextProvider` parse and format unit-aware values against a document or explicit unit system; `Slider` is a one- or two-value slider with markers and persistent settings.

| [INDEX] | [SYMBOL]                                | [KIND]                      | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------- | :-------------------------- | :------------------------------------------------------------- |
|  [01]   | `LineTypeGridView`                      | control (GridView)          | linetype grid bound to a `RhinoDoc`                            |
|  [02]   | `LineTypeItem`                          | row model (ViewModel)       | one linetype row (name, pattern, index, id)                    |
|  [03]   | `PrintWidthGridView`                    | control (GridView)          | print/plot width grid                                          |
|  [04]   | `NumericUpDownWithUnitParsing`          | control (MaskedTextStepper) | unit-parsing numeric stepper with binding and formatting       |
|  [05]   | `UnitParsingMaskedTextProvider`         | provider                    | mask-text provider backing the numeric stepper                 |
|  [06]   | `RichTextAreaWithAlternateText`         | control (Panel)             | rich text area with a swap-in alternate-text area              |
|  [07]   | `Slider`                                | control (TableLayout)       | one/two-value slider with markers, ranges, persistent settings |
|  [08]   | `NumericUpDownWithUnitParsingEventArgs` | eventargs                   | carries `PreviousValue`, `NewValue`, `StepperArgs`             |

[PUBLIC_TYPE_SCOPE]: viewport, dialog, and menu
- namespace: `Rhino.UI.Controls`
- rail: host-boundary native-ui

| [INDEX] | [SYMBOL]            | [KIND]                  | [CAPABILITY]                                        |
| :-----: | :------------------ | :---------------------- | :-------------------------------------------------- |
|  [01]   | `ViewportControl`   | control (Control)       | embeddable `RhinoViewport` control with `Refresh()` |
|  [02]   | `RangeDialog`       | dialog (`Dialog<bool>`) | min/max range entry dialog                          |
|  [03]   | `RenderContentMenu` | static menu registrar   | registers an RDK content context-menu command       |

[PUBLIC_TYPE_SCOPE]: `Rhino.UI.Forms` reusable dialogs and palette
- namespace: `Rhino.UI.Forms`
- rail: host-boundary native-ui

`BaseDialog` and `CommandDialog` are the reusable Eto dialog bases (a plain header/content dialog and an OK/Cancel/Help command dialog returning a `Rhino.Commands.Result`); the `ColorList*` trio is the named-colour palette, and `PrintDialogUi` is the static print/PDF/SVG export façade over `ViewCaptureSettings`. The other ~50 `Rhino.UI.Forms` types are internal Rhino dialog implementations, excluded.

| [INDEX] | [SYMBOL]                | [KIND]                           | [CAPABILITY]                                                     |
| :-----: | :---------------------- | :------------------------------- | :--------------------------------------------------------------- |
|  [01]   | `BaseDialog`            | dialog base (`Dialog<bool>`)     | reusable dialog with a `Message` header and swappable `Content`  |
|  [02]   | `CommandDialog`         | dialog base (`Dialog<Result>`)   | OK/Cancel/Help command dialog returning a `Result`               |
|  [03]   | `PrintWidthDialog`      | dialog (`BaseDialog`)            | print-width picker returning `SelectedWidth`                     |
|  [04]   | `PropertyListBoxDialog` | dialog (`BaseDialog`)            | name/value property-list editor                                  |
|  [05]   | `SectionStyleDialog`    | dialog (`CommandDialog`)         | section-fill style editor                                        |
|  [06]   | `ColorListDialog`       | dialog (`ColorDialog`)           | named-colour-palette picker over Eto's `ColorDialog`             |
|  [07]   | `ColorList`             | palette (`List<ColorListEntry>`) | named-colour collection with a `Default` factory                 |
|  [08]   | `ColorListEntry`        | palette entry                    | immutable `Name`/`Color` pair                                    |
|  [09]   | `PrintDialogUi`         | static export façade             | print/PDF/SVG export and print-dialog over `ViewCaptureSettings` |

[PUBLIC_TYPE_SCOPE]: `Rhino.UI.Theme` colour-model tree
- namespace: `Rhino.UI.Theme`
- rail: host-boundary native-ui

Every `Rhino.UI.Theme` type is public but all constructors are internal: a consumer receives a `ThemeZone` and reads the zone -> element -> state colour tree, never constructs it. `ThemeElement<TState>`/`CheckedThemeElement<TState>`/`ThemeEntry<T>` are the generic bases whose per-element state accessors live on the base (arity-stripped decompile leaves the generic members unresolvable; the concrete leaves close them empty-bodied).

| [INDEX] | [SYMBOL]                      | [KIND]                   | [CAPABILITY]                                                          |
| :-----: | :---------------------------- | :----------------------- | :-------------------------------------------------------------------- |
|  [01]   | `IThemeEntry`                 | interface                | leaf entry contract (`Id`, `Value`)                                   |
|  [02]   | `ThemeBase`                   | abstract root            | `Id` plus `Enumerate() : IEnumerable<IThemeEntry>`                    |
|  [03]   | `ThemeZone`                   | class (`ThemeBase`)      | top zone: background/highlight/divider colours plus element accessors |
|  [04]   | `ContentThemeZone`            | class (`ThemeZone`)      | content-area zone specialization                                      |
|  [05]   | `FrameThemeZone`              | class (`ThemeZone`)      | frame/chrome zone specialization                                      |
|  [06]   | `ThemeElementBase`            | abstract element         | element marker base                                                   |
|  [07]   | `ThemeStateBase`              | abstract state           | state base with default-state fallback                                |
|  [08]   | `ThemeState`                  | abstract state           | `Border`/`Background`/`Text` colours                                  |
|  [09]   | `CheckedThemeState`           | class (`ThemeState`)     | checked/unchecked-capable state                                       |
|  [10]   | `ButtonThemeState`            | class                    | button state                                                          |
|  [11]   | `EntryThemeState`             | class (`ThemeState`)     | adds `PlaceholderText`                                                |
|  [12]   | `LinkThemeState`              | class (`ThemeState`)     | link state                                                            |
|  [13]   | `ListThemeState`              | class                    | list-row state                                                        |
|  [14]   | `ScrollbarThemeState`         | class (`ThemeStateBase`) | scrollbar `Background`/`Border`/`Glyph`/`Thumb`                       |
|  [15]   | `ButtonThemeElement`          | class                    | button element states                                                 |
|  [16]   | `CheckBoxThemeElement`        | class                    | checkbox background/border/glyph colours                              |
|  [17]   | `EntryThemeElement`           | class                    | text-entry element                                                    |
|  [18]   | `ListThemeElement`            | class                    | list element with focus states                                        |
|  [19]   | `TextThemeElement`            | class                    | text element colours                                                  |
|  [20]   | `LinkThemeElement`            | class                    | link element                                                          |
|  [21]   | `ScrollbarThemeElement`       | class                    | scrollbar `Size`/`ArrowSize`/`Radius`                                 |
|  [22]   | `ThemeElement<TState>`        | generic base             | per-element `State` carrier                                           |
|  [23]   | `CheckedThemeElement<TState>` | generic base             | checked/unchecked element base                                        |
|  [24]   | `ThemeEntry<T>`               | generic entry            | typed `IThemeEntry` value carrier                                     |

[PUBLIC_TYPE_SCOPE]: `Rhino.UI.Runtime` platform-service contracts
- namespace: `Rhino.UI.Runtime`
- rail: host-boundary native-ui

`IPlatformService` is the broad platform abstraction a consumer resolves through `PlatformServiceProvider.Service`; `IToolbar`/`IToolbarsService` are the toolbar contracts, and `RhinoUiServiceLocator` resolves a typed service.

| [INDEX] | [SYMBOL]                  | [KIND]          | [CAPABILITY]                                                 |
| :-----: | :------------------------ | :-------------- | :----------------------------------------------------------- |
|  [01]   | `IPlatformService`        | interface       | main window, window pos, bitmap/font/icon conversions, sound |
|  [02]   | `IToolbar`                | interface       | toolbar identity, control, and panel lifecycle callbacks     |
|  [03]   | `IToolbarsService`        | interface       | `UseNewStuff()` and `GetToolbar(Guid)`                       |
|  [04]   | `PlatformServiceProvider` | static provider | `Service` accessor and `ProcessArchitecture`                 |
|  [05]   | `RhinoUiServiceLocator`   | service locator | `GetService<T>() where T : class`                            |
|  [06]   | `HostUtils`               | static utility  | `WindowsRhinoActivated` event and its delegate               |

[ENUM_ROSTERS]:
- `borderSide`: `Left`, `Top`, `Right`, `Bottom` (host spelling is lower-case `borderSide`).
- `GridWrapMode`: `Horizontal`, `Vertical`.
- `DisplayAndPrintColorPickerMode`: `Single`, `Dual`.
- `DisplayAndPrintColorSourceMode`: `Color`, `ByLayer`, `ByObject`, `ByParent`, `Viewport`, `None`.
- `NumericUpDownWithUnitParsingUpdateMode` (`[Flags] : uint`): `OnValueChange=1`, `OnEnterOrLoseFocus=2`, `WhenDoneChanging=8`, `Always=uint.MaxValue`.
- `RhinoLayout.PaddingType`: `None`, `Dialog`, `Indented`, `RhinoPanel`, `RhinoPropertiesPage`, `ButtonRow`, `Table`.
- `RhinoLayout.SpacingType`: `Dialog`, `Panel`, `PropertiesPage`, `ButtonRow`, `Table`.
- `RhinoLayout.WidthControlType`: `Numeric`, `OrderOfMagnitude`, `Text`, `AutoSize`.
- `RhinoLayout.DisablePanelColorStylingProperty` (`[Flags]`): `None=0`, `Foreground=1`, `Background=2`, `All=0xFFFFFF`.
- `RenderContentMenu.SeparatorStyle`: `None`, `Before`, `After`, `Both`.
- `RenderContentMenu.Context`: `Unknown`, `MainThumbnailList`, `MainTree`, `EditorPreview`, `SubNodeControl`, `ColorButton`, `CreateNewButton`, `ContentControl`, `NewContentControl`, `NewContentControlDropDown`, `BreadcrumbControl`, `FloatingPreview`, `Spanner`, `SpannerModal`, `ContentTypeSection`, `ContentTypeBrowserNew`, `ContentTypeBrowserExisting`, `ContentInstanceBrowser`, `ToolTipPreview`.
- `Rhino.UI.Forms.CommandDialog.ShowButtons`: `Close=1`, `OKCancel=3`, `CloseHelp=5`, `OKCancelHelp=7`.
- `Rhino.UI.Forms.SectionSource`: `ByClippingPlane`, `Custom`.

## [03]-[ENTRYPOINTS]

[COLLAPSIBLE_SECTIONS]:
- `EtoCollapsibleSection` (subclass overrides): `abstract LocalizeStringPair Caption { get; }`, `abstract int SectionHeight { get; }`, `virtual bool Collapsible`, `virtual bool Hidden`, `virtual bool InitiallyExpanded`, `virtual string SettingsTag`, `virtual LocalizeStringPair CommandOptionName`, `virtual Guid PlugInId`, `virtual Guid ViewModelId`.
- `EtoCollapsibleSection` (state and events): `IRdkViewModel ViewModel { get; set; }`, `nint CppPointer { get; }`, `event EventHandler<Rhino.UI.Controls.DataSource.EventArgs> DataChanged`, `event EventHandler ViewModelActivated`, `void ApplyStyles()`, `virtual int RunScript(IRdkViewModel)`.
- `EtoCollapsibleSection2` (holder lifecycle): `virtual void OnAttachedToHolder(ICollapsibleSectionHolder2)`, `virtual void OnAttachingToHolder(ICollapsibleSectionHolder2)`, `virtual void OnDetachedFromHolder(ICollapsibleSectionHolder2)`, `virtual void OnDetachingFromHolder(ICollapsibleSectionHolder2)`, `virtual bool EnableHeaderButton(int index, bool)`, `virtual bool ShowHeaderButton(int index, bool)`, `virtual IHeaderButtonHandler NewHeaderButtonHandler()`, `virtual void HolderVisible(bool)`.
- `EtoCollapsibleSection3.UpdateView(uint flags)` — refresh hook.
- `EtoContentUISection.GetSelection() : RenderContentCollection` / `SetSelection(RenderContentCollection) : bool` — content selection binding (the `2`/`3` bases add the constructor and `override bool Hidden`).
- `EtoPostEffectCollapsibleSection`: `abstract Guid PostEffectId { get; }`, `object GetParameter(string paramName, object defaultValue)`, `bool SetParameter(string paramName, object value)`, `PostEffect[] GetPostEffects(PostEffectType type)`.

[SECTION_HOLDER]:
- `EtoCollapsibleSectionHolder()` / `void Add(ICollapsibleSection)` / `void Remove(ICollapsibleSection)` / `ICollapsibleSection SectionAt(int)` / `int FindSectionIndex(ICollapsibleSection)` — section stack authoring.
- `EtoCollapsibleSectionHolder.UseCheckBoxes { get; set; }` / `UseScrollbars { get; set; }` / `Guid Uuid { get; set; }` / `IEnumerable<ICollapsibleSection> Sections { get; }` — holder configuration.
- `EtoCollapsibleSectionHolder.SectionCheckBox(string caption) : CheckBox` / `RegisterSectionCheckBoxes()` / `UnRegisterSectionCheckBoxes()` / `event OnCheckboxCheckedHandler OnCheckBoxChecked` — check-box mode.
- `EtoCollapsibleSectionHolder2.EnableHeaderButton(ICollapsibleSection s, int index, bool)` / `ShowHeaderButton(ICollapsibleSection s, int index, bool)` / `SetFullHeightSection(ICollapsibleSection)` — per-section header button and full-height section.

[LAYOUT_FACTORY]:
- `RhinoLayout.Padding(PaddingType) : Padding` / `Spacing(SpacingType) : Size` / `StackedSpacing(Orientation, SpacingType) : int` / `FixedWidth(WidthControlType) : int` / `FixedSize(WidthControlType) : Size` — the padding/spacing/size vocabulary.
- `RhinoLayout.LabelRow(string text, Control editControl, bool stretch) : TableRow` / `LabelTableLayout(string text, Control editControl, bool stretch, SpacingType) : TableLayout` / `NewLabel(string) : Label` / `NewLabelSeparator(string) : Label` — label-row and label-table factories.
- `RhinoLayout.DisablePanelColorStyling(Control, DisablePanelColorStylingProperty) : void` / `EnablePanelColorStyling(Panel, bool invalidate = true) : void` — panel colour-styling control.

```csharp signature
RhinoTableLayout(RhinoLayout.PaddingType padding, RhinoLayout.SpacingType spacing)
RhinoNestedTableLayout(RhinoLayout.SpacingType spacing, params TableRow[] rows)
RhinoButtonTableLayout(Orientation orientation, RhinoLayout.SpacingType spacing, params Button[] buttons)
RhinoButtonStackLayout(Orientation orientation, RhinoLayout.SpacingType spacing, params Button[] buttons)
RhinoPanelStackLayout(Orientation orientation = Orientation.Vertical)
ControlGridLayout.GridWrapMode / ItemSize / ItemPadding / Wrap / StretchItemsToWidth { get; set; }
ControlGridLayout.Items { get; }  Rows / Columns { get; }  event RowsChanged / ColumnsChanged
```

[PANELS]:
- `RhinoGroupBox(RhinoLayout.SpacingType)` / `RhinoGroupBox.Text { get; set; }` / `new Color BackgroundColor` / `new Control Content` / `new Padding Padding` — styled group box.
- `PanelWithBorder()` / `PanelWithBorder.BorderColor { get; set; }` / `BorderThickness { get; set; }` / `new Color BackgroundColor` — bordered panel.
- `RhinoScrollableDialogPanel(RhinoLayout.PaddingType paddingType = RhinoLayout.PaddingType.Dialog)` / `RhinoScrollableDialogPanel(bool border)` — scrollable dialog panel.

[BUTTONS]:
- `ImageButton()` / `ImageButton(bool useFrameColors)` / `ImageButton.Command : ICommand` / `CommandParameter : object` / `Image` / `DisabledImage` / `MaskImageWithBackgroundColorWhenDisabled : bool` / `event EventHandler<EventArgs> Click` — command-bound image button.
- `AddRemoveButton(string style = "mac-small")` / `AddEnabled` / `RemoveEnabled : bool` / `AddCommand` / `RemoveCommand : Command` / `event EventHandler Added` / `Removed` / `InsertButton(string resourceId, string toolTip, Action<object, EventArgs> click) : ButtonSegmentedItem` — add/remove segmented button.
- `ImageToolTipButton(string leftToolTip, string rightToolTip)` / `ImageToolTipButton(string normalImageResourceId, string disabledImageResourceId, string leftToolTip, string rightToolTip)` — dual-tooltip image button.
- `RhinoButtonRow()` / `RhinoButtonRow.AddButton(Button)` / `AddButton(ImageButton)` / `AddButton(string imageResource, string tooltip) : ImageButton` / `AddButton(Image, bool useOverlay, string tooltip) : ImageButton` — button row.
- `ButtonDrawable(IHeaderButtonHandler hbh, int index, Eto.Drawing.Bitmap image, string tooltip)` — header-button drawable.

[LABELS_AND_COLOUR]:
- `Divider()` / `Divider.Color { get; set; }` / `ForceHorizontalLine { get; set; }` / `Orientation { get; }` — auto-orient divider.
- `LabelSeparator()` / `LabelSeparator.Text` / `Color` / `UseRhinoColorScheme { get; set; }` — labelled separator.
- `StaticAlignedLabel(TextAlignment align)` — fixed-alignment label.
- `DisplayAndPrintColorPicker()` / `PickerMode : DisplayAndPrintColorPickerMode` / `DisplaySourceMode` / `PrintSourceMode : DisplayAndPrintColorSourceMode` / `DisplayColor` / `PrintColor` / `DisplayResolvedColor` / `PrintResolvedColor : Color` / `LinkPrintToDisplay : bool` / `event PropertyChangedEventHandler PropertyChanged` — dual display/print colour picker.

[NUMERIC_AND_TEXT]:
- `NumericUpDownWithUnitParsing(bool showStepper = true)` / `Value : double` / `ValueBinding : BindableBinding<..., double>` / `ValueUpdateMode : NumericUpDownWithUnitParsingUpdateMode` / `Prefix` / `Suffix` / `AlternateText : string` / `Increment` / `MinValue` / `MaxValue : double` / `DecimalPlaces : int`.
- `NumericUpDownWithUnitParsing.SetFormatUnitSystem(RhinoDoc) : void` / `SetFormatUnitSystem(UnitSystem, DistanceDisplayMode) : void` / `SetFormatLengthUnits(LengthUnit, DistanceDisplayMode) : void` / `UseViewModelUnits(ViewModel) : void` — unit-format binding; events `SteppingStarted`/`SteppingDone`/`TextChangingStarted`/`TextChangingDone` are `EventHandler<NumericUpDownWithUnitParsingEventArgs>`.
- `UnitParsingMaskedTextProvider()` / `Value : double` / `PreviousValue { get; private set; }` / `Increment` / `MinValue` / `MaxValue : double` / `RevertValue() : void` / `SetFormatUnitSystem(UnitSystem, DistanceDisplayMode) : void` — masked-text provider.
- `RichTextAreaWithAlternateText()` / `RichTextArea { get; }` / `AlternateTextArea { get; }` / `ShowAlternateText : bool` / `AlternateText : string` / `ReadOnly : bool` — rich text with alternate area.
- `Slider(string settingsName, Control parent, bool usePercentageSign, string text1, string text2)` / `Value1` / `Value2 : double?` / `SetMinMax(double, double) : void` / `SetVaries(bool) : void` / `Settings(string key, string item) : PersistentSettings` — one/two-value slider.

[LIST_GRID]:
- `LineTypeGridView(RhinoDoc doc)` / `LineTypeCollection : FilterCollection<LineTypeItem>` / `Document : RhinoDoc` / `AddLineTypeItem(Linetype lt, string linetypename) : void` — linetype grid.
- `LineTypeItem(Guid linetypeId)` / `Name` / `Pattern : string` / `LineTypeId : Guid` / `LinetypeIndex : int` / `AlwaysModelDistances : bool` — linetype row.

[VIEWPORT_DIALOG_MENU]:
- `ViewportControl()` / `ViewportControl(string viewportTitle)` / `Viewport : RhinoViewport` / `Refresh() : void` — embeddable viewport.
- `RangeDialog(double min, double max, int decimals, int increment, bool min_range, bool max_range)` / public fields `Min` / `Max : double`, `Increment` / `Decimals : int` — range dialog.
- `RenderContentMenu.AddMenuItem(Guid plugInId, string menuItemName, int menuOrder, SeparatorStyle separatorStyle, bool isToplevel, Icon icon, Func<RenderContentCollection, Result> executeCommandCallback, Func<RenderContentCollection, Context, bool> isEnabledCallback) : void` — content context-menu registration.

[FORMS_DIALOGS]:
- `BaseDialog()` / `BaseDialog.Message { get; set; }` / `new Control Content { get; set; }` — header/content dialog base.
- `CommandDialog()` / `Buttons : ShowButtons` / `ShowHelpButton : bool` / `SavePosition : bool` / `UpdateSourceOnApply : bool` / `event EventHandler<EventArgs> HelpButtonClick` — command dialog base.
- `PrintWidthDialog(string title, string message, double selectedWidth)` / `SelectedWidth : double` — print-width picker.
- `PropertyListBoxDialog(string title, string message, IList items, IList<string> values)` / `Collection : ObservableCollection<ListProperty>` / `Values : string[]`; `ListProperty.PropertyName` / `Value : string` — property-list editor.
- `SectionStyleDialog(uint documentSerialNumber, int hatchIndex, double rotation, double scale, Color backfillColor, bool showBoundary, ObjectSectionFillRule fillRule)` / `SectionSource : SectionSource` / `HatchIndex : int` / `Scale : double` / `SectionFillRule : ObjectSectionFillRule` — section-fill editor.

[FORMS_COLOUR_AND_EXPORT]:
- `ColorList(string name, IEnumerable<ColorListEntry> entries)` / `ColorList.Default : ColorList` / `ColorList.Name : string`; `ColorListEntry(string name, Color color)` / `Name : string` / `Color : Color` — named-colour palette.
- `ColorListDialog.ColorList { get; set; }` / `SelectedEntry : ColorListEntry` / `event EventHandler<EventArgs> SelectedEntryChanged` / `new DialogResult ShowDialog(Control parent)` — palette picker.
- `PrintDialogUi(uint documentRuntimeSerialNumber, PersistentSettings settings)` — export façade constructor.

```csharp signature
PrintDialogUi.EtoExportSvg(uint documentRuntimeSerial, PersistentSettings settings) : ViewCaptureSettings
PrintDialogUi.EtoExportSvgArray(uint documentRuntimeSerial, PersistentSettings settings, FileWriteOptions options) : ViewCaptureSettings[]
PrintDialogUi.EtoExportPdf(uint documentRuntimeSerial, PersistentSettings settings, bool selectedObjectsOnly) : List<KeyValuePair<string, ViewCaptureSettings>>
PrintDialogUi.ShowSavePdfFileDialog(string documentName, uint documentRuntimeSerial, out string path) : DialogResult
PrintDialogUi.ShowPrintDialog(string dialogTitle, uint documentRuntimeSerialNumber, PersistentSettings settings, bool selectedObjectsOnly, bool showPrinterDestinations) : Result
```

[THEME_MODEL]:
- `IThemeEntry.Id : string` / `Value : object` — leaf entry.
- `ThemeBase.Id : string` / `Enumerate() : IEnumerable<IThemeEntry>` — tree root.
- `ThemeState.Border` / `Background` / `Text : Color` (get-only); `EntryThemeState.PlaceholderText : Color`; `ScrollbarThemeState.Background` / `Border` / `Glyph` / `Thumb : Color`.
- `ThemeZone.Background` / `Highlight` / `HighlightHover` / `GripperDot` / `Divider : Color` (get-only) — zone colours.
- `ThemeZone.Button` / `Tab : ButtonThemeElement` / `CheckBox : CheckBoxThemeElement` / `Entry : EntryThemeElement` / `List : ListThemeElement` / `Text : TextThemeElement` / `Link : LinkThemeElement` / `Scrollbar : ScrollbarThemeElement` (get-only) — element accessors.
- `ButtonThemeElement.Default` / `DefaultHover` / `DefaultPressed` / `Unchecked` / `UncheckedtHover` / `UncheckedPressed : ButtonThemeState` — button states (host spelling `UncheckedtHover` is reproduced verbatim).
- `TextThemeElement.Enabled` / `Disabled` / `Highlight` / `HighlightHover` / `Secondary : Color`; `ScrollbarThemeElement.Size` / `ArrowSize` / `Radius : double`.

[RUNTIME_SERVICES]:
- `PlatformServiceProvider.Service : IPlatformService` (static get/set) / `ProcessArchitecture : string` (static, `"x86"`/`"x64"`/`"arm64"`/`"armv7l"`/`"unknown"`).
- `RhinoUiServiceLocator.GetService<T>() where T : class : T` — typed service resolution.
- `IToolbarsService.UseNewStuff() : bool` / `GetToolbar(Guid id) : IToolbar`.
- `IToolbar.Id : Guid` / `FileId : Guid` / `GetControl(uint documentSerialNumber, bool popperUp) : Control` / `PanelVisibilityChanged(bool shown, uint documentSerialNumber, ShowPanelReason resaon) : void` / `PanelClosing(uint documentSerialNumber, bool onCloseDocument) : void` (host param spelling `resaon` reproduced verbatim).
- `IPlatformService.MainRhinoWindow : Window` plus the bitmap/font/icon conversion, `SetWindowPos`, `ShowSemiModal`, control-padding, and `PlaySoundFile` members.
- `HostUtils.WindowsRhinoActivated : event WindowsRhinoActivatedEvent` / `delegate void WindowsRhinoActivatedEvent(bool activated)`.

## [04]-[IMPLEMENTATION_LAW]

[CONTROLS_TOPOLOGY]:
- Collapsible sections layer by capability: a leaf `EtoCollapsibleSection` overrides `Caption`/`SectionHeight`, the `2`/`3` bases add holder-attach lifecycle and an `UpdateView` refresh, `EtoContentUISection*` binds a `RenderContentCollection`, and `EtoPostEffectCollapsibleSection` binds a post-effect; a plugin subclasses the narrowest base its section needs and adds it to an `EtoCollapsibleSectionHolder`. `api-rhino-ui.md` owns the page-hosting seam that stacks these sections in a properties or options page.
- Layout is padding/spacing-typed, never hardcoded: `RhinoLayout` is the one vocabulary owner (`PaddingType`/`SpacingType`/`WidthControlType`) every Rhino-styled table, stack, panel, and control grid reads through its constructor, so a screen selects a padding row, never a pixel literal.
- Numeric input is unit-aware at the control: `NumericUpDownWithUnitParsing` and `UnitParsingMaskedTextProvider` parse and format against a `RhinoDoc` or an explicit `UnitSystem`/`LengthUnit` + `DistanceDisplayMode`, so a length field reads and writes model units without a call-site parse.
- Theme is a read-only model: a consumer receives a `ThemeZone` and folds its `Enumerate()` tree of `IThemeEntry` colours; the internal constructors mean the theme is host-owned and read, never authored, and a widget styles itself from the zone rather than a hardcoded palette.
- Platform capability resolves through a contract: `PlatformServiceProvider.Service` and `RhinoUiServiceLocator.GetService<T>()` hand a consumer the `IPlatformService`/`IToolbarsService` abstraction, so a cross-platform host concern is one interface call, never a per-OS branch.

[STACKING]:
- `api-eto-forms.md`: every control here derives from an `Eto.Forms` type (`Panel`, `Drawable`, `TableLayout`, `StackLayout`, `Scrollable`, `GridView`, `Dialog<T>`) — the base widget roster, layout, and binding surface is that catalog's, and this library adds Rhino styling, settings, and unit-parsing on top, never a re-implemented control.
- `api-rhino-ui.md`: the host bridge seats a control library screen inside a document-owned panel, page, or semi-modal dialog through `RhinoEtoApp`/`EtoExtensions`, and the `EtoCollapsibleSection`(+`Holder`) page-section seam lives there; a `DataSource.ProviderIds` `Guid` and a `DataSource.EventArgs`/`EventInfoArgs` payload cross from that catalog's RDK data-source surface.
- `api-languageext.md`(`../../.api/api-languageext.md`): a `RangeDialog`/`ColorListDialog`/`PrintDialogUi` outcome and a `NumericUpDownWithUnitParsing` parse land on `Fin<A>` (a cancelled dialog is a `Fail`, not a null), a `ThemeZone.Enumerate()` folds as `Seq<IThemeEntry>`, and a `PlatformServiceProvider.Service` or `RhinoUiServiceLocator.GetService<T>()` lookup crosses as `Option<A>`.
- `api-thinktecture-runtime-extensions.md`(`../../.api/api-thinktecture-runtime-extensions.md`): the control enums (`RhinoLayout.PaddingType`/`SpacingType`/`WidthControlType`, `DisplayAndPrintColorSourceMode`, `GridWrapMode`, `RenderContentMenu.Context`, `Forms.CommandDialog.ShowButtons`, `Forms.SectionSource`) map at the edge to `[SmartEnum]` owners and the `[Flags]` `NumericUpDownWithUnitParsingUpdateMode`/`DisablePanelColorStylingProperty` to flag owners, so the domain composes the bounded vocabulary, never the raw host enum.
- `api-unicolour.md`(`../../.api/api-unicolour.md`): the `DisplayAndPrintColorPicker`/`ColorList` colour values map to and from `Unicolour` at the view edge, so display and print colour selection stays in the perceptual model.

[LOCAL_ADMISSION]:
- The control library is host-provided under `Rhino.UI.dll`; a control is trapped and mapped at the boundary, its `Eto.Forms.*` base type stays behind the Rasm.Rhino UI owner, and a `nint CppPointer`/`EventInfoPtr` native handle never appears in a domain signature.
- Access is the ruling filter: `Rhino.UI.Controls` is largely internal panel and dialog implementation, so only the genuinely-public constructible or subclassable surface lands; the internal panel namespaces contribute nothing.
- `Rhino.UI.Annotations` is excluded: every type is an internal formatting or dictionary helper, no public consumable surface.
- `Rhino.UI.DialogPanels`, `Rhino.UI.ViewModels`, `Rhino.UI.ObjectProperties`, and `Rhino.UI.ObjectManager` are excluded: their public types are Rhino's own registered dockable panels, view-models, and widgets, `public` only for the `IPanel` host and the Eto control hierarchy, never a consumable contract a plugin subclasses or receives; the plugin-facing object-properties API is `ObjectPropertiesPage` in `api-rhino-ui.md`.

[RAIL_LAW]:
- Package: `RhinoCommon` + `Rhino.UI` (`Rhino.UI.dll` control library)
- Owns: the `Rhino.UI.Controls` collapsible-section family, layout/panel/button/label/colour/list/numeric-text controls, the viewport control and range/content-menu dialogs, the `Rhino.UI.Forms` dialog bases and colour palette and export façade, the read-only `Rhino.UI.Theme` colour-model tree, and the `Rhino.UI.Runtime` platform-service contracts.
- Accept: a Rhino-styled control composed from an `Eto.Forms` base and seated through the host bridge, a padding/spacing-typed layout, a unit-aware numeric field, a themed read of a `ThemeZone`, and a platform capability resolved through `IPlatformService`.
- Reject: re-implementing an `Eto.Forms` base control (`api-eto-forms.md` owns it), a pixel-literal layout where a `RhinoLayout` type fits, authoring a `ThemeZone` (internal-constructed, read-only), a `CppPointer`/`EventInfoPtr` native handle escaping into a domain signature, and admitting an internal panel/view-model/object-properties type or the dead `ThumbnailUI` surface.
