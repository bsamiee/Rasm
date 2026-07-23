# [RASM_RHINO_API_RHINO_UI_CONTROLS]

`Rhino.UI.dll` owns Rhino's reusable Eto control library: `Rhino.UI.Controls` collapsible sections, padding/spacing-typed layouts, panels, button/label/colour widgets, unit-parsing numeric and text inputs, an embeddable viewport, and range/content-menu dialogs; the `Rhino.UI.Forms` dialog bases, colour palette, and export façade; the read-only `Rhino.UI.Theme` colour-model tree; and the `Rhino.UI.Runtime` platform-service contracts. Every widget specializes an `Eto.Forms` base; only the constructible or subclassable surface lands, the internal implementation excluded.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Rhino.UI.dll` control library
- package: `RhinoCommon` (proprietary McNeel SDK)
- assembly: `Rhino.UI.dll` (reusable Eto controls, themed dialogs, theme model, platform-service contracts)
- namespace: `Rhino.UI.Controls` (collapsible sections, layouts, panels, buttons, labels, colour, list/grid, numeric/text, viewport, dialogs, menu)
- namespace: `Rhino.UI.Forms` (dialog bases, named-colour palette, print/PDF/SVG export façade)
- namespace: `Rhino.UI.Theme` (zone/element/state colour-model tree)
- namespace: `Rhino.UI.Runtime` (platform-service, toolbar-service, service-locator contracts)
- rail: host-boundary native-ui

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: `Rhino.UI.Controls` collapsible-section family

A plugin subclasses the narrowest section base its section needs and adds it to a holder; the `2`/`3` bases add holder-attach lifecycle and an `UpdateView` refresh, `EtoContentUISection*` binds a `RenderContentCollection`, and `EtoPostEffectCollapsibleSection` binds a post-effect.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                    |
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

[PUBLIC_TYPE_SCOPE]: `Rhino.UI.Controls` layout controls

`RhinoLayout` is the static padding/spacing/size vocabulary every Rhino-styled layout reads through its constructor.

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY]         | [CAPABILITY]                                                   |
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

[PUBLIC_TYPE_SCOPE]: `Rhino.UI.Controls` panel, host, and scrollable containers

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]        | [CAPABILITY]                                        |
| :-----: | :--------------------------- | :------------------- | :-------------------------------------------------- |
|  [01]   | `RhinoGroupBox`              | control (Panel)      | group box with styled background, content, padding  |
|  [02]   | `RhinoDialogPanel`           | control (Panel)      | dialog-styled panel                                 |
|  [03]   | `RhinoIndentedPanel`         | control (Panel)      | indented panel                                      |
|  [04]   | `PanelWithBorder`            | control (Panel)      | panel with configurable border colour and thickness |
|  [05]   | `RhinoPanelScrollable`       | control (Scrollable) | Rhino-styled scrollable panel                       |
|  [06]   | `RhinoScrollableDialogPanel` | control (Scrollable) | scrollable dialog panel                             |

[PUBLIC_TYPE_SCOPE]: `Rhino.UI.Controls` buttons, labels, dividers, and colour

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]             | [CAPABILITY]                                                |
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

[PUBLIC_TYPE_SCOPE]: `Rhino.UI.Controls` list, grid, numeric, and text controls

`NumericUpDownWithUnitParsing` and its `UnitParsingMaskedTextProvider` parse and format unit-aware values against a document or explicit unit system; `Slider` is a one- or two-value slider with markers and persistent settings.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]               | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------- | :-------------------------- | :------------------------------------------------------------- |
|  [01]   | `LineTypeGridView`                      | control (GridView)          | linetype grid bound to a `RhinoDoc`                            |
|  [02]   | `LineTypeItem`                          | row model (ViewModel)       | one linetype row (name, pattern, index, id)                    |
|  [03]   | `PrintWidthGridView`                    | control (GridView)          | print/plot width grid                                          |
|  [04]   | `NumericUpDownWithUnitParsing`          | control (MaskedTextStepper) | unit-parsing numeric stepper with binding and formatting       |
|  [05]   | `UnitParsingMaskedTextProvider`         | provider                    | mask-text provider backing the numeric stepper                 |
|  [06]   | `RichTextAreaWithAlternateText`         | control (Panel)             | rich text area with a swap-in alternate-text area              |
|  [07]   | `Slider`                                | control (TableLayout)       | one/two-value slider with markers, ranges, persistent settings |
|  [08]   | `NumericUpDownWithUnitParsingEventArgs` | eventargs                   | carries `PreviousValue`, `NewValue`, `StepperArgs`             |

[PUBLIC_TYPE_SCOPE]: `Rhino.UI.Controls` viewport, dialog, and menu

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]           | [CAPABILITY]                                        |
| :-----: | :------------------ | :---------------------- | :-------------------------------------------------- |
|  [01]   | `ViewportControl`   | control (Control)       | embeddable `RhinoViewport` control with `Refresh()` |
|  [02]   | `RangeDialog`       | dialog (`Dialog<bool>`) | min/max range entry dialog                          |
|  [03]   | `RenderContentMenu` | static menu registrar   | registers an RDK content context-menu command       |

[PUBLIC_TYPE_SCOPE]: `Rhino.UI.Forms` reusable dialogs and palette

`BaseDialog`/`CommandDialog` are the reusable Eto dialog bases; the `ColorList*` trio is the named-colour palette, and `PrintDialogUi` is the static print/PDF/SVG export façade over `ViewCaptureSettings`.

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]                    | [CAPABILITY]                                                     |
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

Every `Rhino.UI.Theme` constructor is internal: a consumer receives a `ThemeZone` and reads the zone -> element -> state colour tree, never constructs it. `ThemeElement<TState>`/`CheckedThemeElement<TState>`/`ThemeEntry<T>` are the generic bases; the concrete leaves close their state accessors.

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]            | [CAPABILITY]                                                          |
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

`IPlatformService` is the broad platform abstraction a consumer resolves through `PlatformServiceProvider.Service`; `IToolbar`/`IToolbarsService` are the toolbar contracts, and `RhinoUiServiceLocator` resolves a typed service.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [CAPABILITY]                                                 |
| :-----: | :------------------------ | :-------------- | :----------------------------------------------------------- |
|  [01]   | `IPlatformService`        | interface       | main window, window pos, bitmap/font/icon conversions, sound |
|  [02]   | `IToolbar`                | interface       | toolbar identity, control, and panel lifecycle callbacks     |
|  [03]   | `IToolbarsService`        | interface       | `UseNewStuff()` and `GetToolbar(Guid)`                       |
|  [04]   | `PlatformServiceProvider` | static provider | `Service` accessor and `ProcessArchitecture`                 |
|  [05]   | `RhinoUiServiceLocator`   | service locator | `GetService<T>() where T : class`                            |
|  [06]   | `HostUtils`               | static utility  | `WindowsRhinoActivated` event and its delegate               |

[ENUM_ROSTERS]:
- `borderSide`: `Left` `Top` `Right` `Bottom` (host spelling is lower-case `borderSide`).
- `GridWrapMode`: `Horizontal` `Vertical`.
- `DisplayAndPrintColorPickerMode`: `Single` `Dual`.
- `DisplayAndPrintColorSourceMode`: `Color` `ByLayer` `ByObject` `ByParent` `Viewport` `None`.
- `NumericUpDownWithUnitParsingUpdateMode` (`[Flags] : uint`): `OnValueChange=1` `OnEnterOrLoseFocus=2` `WhenDoneChanging=8` `Always=uint.MaxValue`.
- `RhinoLayout.PaddingType`: `None` `Dialog` `Indented` `RhinoPanel` `RhinoPropertiesPage` `ButtonRow` `Table`.
- `RhinoLayout.SpacingType`: `Dialog` `Panel` `PropertiesPage` `ButtonRow` `Table`.
- `RhinoLayout.WidthControlType`: `Numeric` `OrderOfMagnitude` `Text` `AutoSize`.
- `RhinoLayout.DisablePanelColorStylingProperty` (`[Flags]`): `None=0` `Foreground=1` `Background=2` `All=0xFFFFFF`.
- `RenderContentMenu.SeparatorStyle`: `None` `Before` `After` `Both`.
- `RenderContentMenu.Context`: `Unknown` `MainThumbnailList` `MainTree` `EditorPreview` `SubNodeControl` `ColorButton` `CreateNewButton` `ContentControl` `NewContentControl` `NewContentControlDropDown` `BreadcrumbControl` `FloatingPreview` `Spanner` `SpannerModal` `ContentTypeSection` `ContentTypeBrowserNew` `ContentTypeBrowserExisting` `ContentInstanceBrowser` `ToolTipPreview`.
- `Rhino.UI.Forms.CommandDialog.ShowButtons`: `Close=1` `OKCancel=3` `CloseHelp=5` `OKCancelHelp=7`.
- `Rhino.UI.Forms.SectionSource`: `ByClippingPlane` `Custom`.

## [03]-[ENTRYPOINTS]

[COLLAPSIBLE_SECTIONS]:
- `EtoCollapsibleSection` subclass overrides: `Caption` `SectionHeight` `Collapsible` `Hidden` `InitiallyExpanded` `SettingsTag` `CommandOptionName` `PlugInId` `ViewModelId`; state `ViewModel` `CppPointer`; events `DataChanged` `ViewModelActivated`.
- `EtoCollapsibleSection2` holder lifecycle: `OnAttachedToHolder(ICollapsibleSectionHolder2)` `OnAttachingToHolder(...)` `OnDetachedFromHolder(...)` `OnDetachingFromHolder(...)` `HolderVisible(bool)`.

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `EtoCollapsibleSection.ApplyStyles()`                                            | instance | apply Rhino styling to the section |
|  [02]   | `EtoCollapsibleSection.RunScript(IRdkViewModel) -> int`                          | override | run the section's RDK view-model   |
|  [03]   | `EtoCollapsibleSection2.EnableHeaderButton(int, bool) -> bool`                   | override | enable one header button           |
|  [04]   | `EtoCollapsibleSection2.ShowHeaderButton(int, bool) -> bool`                     | override | show one header button             |
|  [05]   | `EtoCollapsibleSection2.NewHeaderButtonHandler() -> IHeaderButtonHandler`        | override | mint the header-button handler     |
|  [06]   | `EtoCollapsibleSection3.UpdateView(uint)`                                        | override | refresh hook on flags              |
|  [07]   | `EtoContentUISection.GetSelection() -> RenderContentCollection`                  | instance | read content selection             |
|  [08]   | `EtoContentUISection.SetSelection(RenderContentCollection) -> bool`              | instance | bind content selection             |
|  [09]   | `EtoPostEffectCollapsibleSection.GetParameter(string, object) -> object`         | instance | read a post-effect parameter       |
|  [10]   | `EtoPostEffectCollapsibleSection.SetParameter(string, object) -> bool`           | instance | write a post-effect parameter      |
|  [11]   | `EtoPostEffectCollapsibleSection.GetPostEffects(PostEffectType) -> PostEffect[]` | instance | list post-effects by type          |

- `EtoPostEffectCollapsibleSection.PostEffectId` is the abstract identity a subclass overrides.

[SECTION_HOLDER]:

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `EtoCollapsibleSectionHolder()`                                                   | ctor     | new section stack              |
|  [02]   | `EtoCollapsibleSectionHolder.Add(ICollapsibleSection)`                            | instance | add a section                  |
|  [03]   | `EtoCollapsibleSectionHolder.Remove(ICollapsibleSection)`                         | instance | remove a section               |
|  [04]   | `EtoCollapsibleSectionHolder.SectionAt(int) -> ICollapsibleSection`               | instance | section by index               |
|  [05]   | `EtoCollapsibleSectionHolder.FindSectionIndex(ICollapsibleSection) -> int`        | instance | index of a section             |
|  [06]   | `EtoCollapsibleSectionHolder.SectionCheckBox(string) -> CheckBox`                 | instance | check box for a caption        |
|  [07]   | `EtoCollapsibleSectionHolder.RegisterSectionCheckBoxes()`                         | instance | register check-box mode        |
|  [08]   | `EtoCollapsibleSectionHolder.UnRegisterSectionCheckBoxes()`                       | instance | tear down check-box mode       |
|  [09]   | `EtoCollapsibleSectionHolder2.EnableHeaderButton(ICollapsibleSection, int, bool)` | instance | enable a section header button |
|  [10]   | `EtoCollapsibleSectionHolder2.ShowHeaderButton(ICollapsibleSection, int, bool)`   | instance | show a section header button   |
|  [11]   | `EtoCollapsibleSectionHolder2.SetFullHeightSection(ICollapsibleSection)`          | instance | mark the full-height section   |

- `EtoCollapsibleSectionHolder` config: `UseCheckBoxes` `UseScrollbars` `Uuid` `Sections`; event `OnCheckBoxChecked`.

[LAYOUT_FACTORY]:
- `RhinoLayout` vocabulary: `Padding(PaddingType) -> Padding` `Spacing(SpacingType) -> Size` `StackedSpacing(Orientation, SpacingType) -> int` `FixedWidth(WidthControlType) -> int` `FixedSize(WidthControlType) -> Size`.
- `RhinoLayout` label factories: `LabelRow(string, Control, bool) -> TableRow` `LabelTableLayout(string, Control, bool, SpacingType) -> TableLayout` `NewLabel(string) -> Label` `NewLabelSeparator(string) -> Label`.
- `RhinoLayout` colour styling: `DisablePanelColorStyling(Control, DisablePanelColorStylingProperty)` `EnablePanelColorStyling(Panel, bool)`.

| [INDEX] | [SURFACE]                                                    | [SHAPE] | [CAPABILITY]             |
| :-----: | :----------------------------------------------------------- | :------ | :----------------------- |
|  [01]   | `RhinoTableLayout(PaddingType, SpacingType)`                 | ctor    | Rhino-typed table layout |
|  [02]   | `RhinoNestedTableLayout(SpacingType, TableRow[])`            | ctor    | nested table from rows   |
|  [03]   | `RhinoButtonTableLayout(Orientation, SpacingType, Button[])` | ctor    | table of buttons         |
|  [04]   | `RhinoButtonStackLayout(Orientation, SpacingType, Button[])` | ctor    | stack of buttons         |
|  [05]   | `RhinoPanelStackLayout(Orientation)`                         | ctor    | panel stack layout       |

- `ControlGridLayout` config: `GridWrapMode` `ItemSize` `ItemPadding` `Wrap` `StretchItemsToWidth` `Items` `Rows` `Columns`; events `RowsChanged` `ColumnsChanged`.

[PANELS]:

| [INDEX] | [SURFACE]                                 | [SHAPE] | [CAPABILITY]                       |
| :-----: | :---------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `RhinoGroupBox(SpacingType)`              | ctor    | styled group box                   |
|  [02]   | `PanelWithBorder()`                       | ctor    | bordered panel                     |
|  [03]   | `RhinoScrollableDialogPanel(PaddingType)` | ctor    | scrollable dialog panel by padding |
|  [04]   | `RhinoScrollableDialogPanel(bool)`        | ctor    | scrollable dialog panel by border  |

- `RhinoGroupBox` props: `Text` `BackgroundColor` `Content` `Padding`. `PanelWithBorder` props: `BorderColor` `BorderThickness` `BackgroundColor`.

[BUTTONS]:

| [INDEX] | [SURFACE]                                                                 | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------------ | :------- | :-------------------------------- |
|  [01]   | `ImageButton()` / `ImageButton(bool)`                                     | ctor     | command-bound image button        |
|  [02]   | `AddRemoveButton(string)`                                                 | ctor     | add/remove segmented button       |
|  [03]   | `AddRemoveButton.InsertButton(string, string, Action<object, EventArgs>)` | instance | insert a segment                  |
|  [04]   | `ImageToolTipButton(string, string)`                                      | ctor     | dual-tooltip image button         |
|  [05]   | `ImageToolTipButton(string, string, string, string)`                      | ctor     | dual-tooltip with image resources |
|  [06]   | `RhinoButtonRow()`                                                        | ctor     | button row                        |
|  [07]   | `RhinoButtonRow.AddButton(Button)`                                        | instance | add an Eto button                 |
|  [08]   | `RhinoButtonRow.AddButton(string, string) -> ImageButton`                 | instance | add an image button by resource   |
|  [09]   | `RhinoButtonRow.AddButton(Image, bool, string) -> ImageButton`            | instance | add an image button with overlay  |
|  [10]   | `ButtonDrawable(IHeaderButtonHandler, int, Bitmap, string)`               | ctor     | header-button drawable            |

- `AddRemoveButton.InsertButton` returns `ButtonSegmentedItem`.
- `ImageButton` props/events: `Command` `CommandParameter` `Image` `DisabledImage` `MaskImageWithBackgroundColorWhenDisabled`; event `Click`. `AddRemoveButton` props/events: `AddEnabled` `RemoveEnabled` `AddCommand` `RemoveCommand`; events `Added` `Removed`.

[LABELS_AND_COLOUR]:

| [INDEX] | [SURFACE]                           | [SHAPE] | [CAPABILITY]              |
| :-----: | :---------------------------------- | :------ | :------------------------ |
|  [01]   | `Divider()`                         | ctor    | auto-orient divider       |
|  [02]   | `LabelSeparator()`                  | ctor    | labelled separator        |
|  [03]   | `StaticAlignedLabel(TextAlignment)` | ctor    | fixed-alignment label     |
|  [04]   | `DisplayAndPrintColorPicker()`      | ctor    | dual display/print picker |

- `Divider` props: `Color` `ForceHorizontalLine` `Orientation`. `LabelSeparator` props: `Text` `Color` `UseRhinoColorScheme`.
- `DisplayAndPrintColorPicker` props: `PickerMode` `DisplaySourceMode` `PrintSourceMode` `DisplayColor` `PrintColor` `DisplayResolvedColor` `PrintResolvedColor` `LinkPrintToDisplay`; event `PropertyChanged`.

[NUMERIC_AND_TEXT]:

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `NumericUpDownWithUnitParsing(bool)`                                                 | ctor     | unit-parsing numeric stepper      |
|  [02]   | `NumericUpDownWithUnitParsing.SetFormatUnitSystem(RhinoDoc)`                         | instance | bind format units to a document   |
|  [03]   | `NumericUpDownWithUnitParsing.SetFormatUnitSystem(UnitSystem, DistanceDisplayMode)`  | instance | bind explicit unit system         |
|  [04]   | `NumericUpDownWithUnitParsing.SetFormatLengthUnits(LengthUnit, DistanceDisplayMode)` | instance | bind explicit length units        |
|  [05]   | `NumericUpDownWithUnitParsing.UseViewModelUnits(ViewModel)`                          | instance | bind format units to a view-model |
|  [06]   | `UnitParsingMaskedTextProvider()`                                                    | ctor     | masked-text provider              |
|  [07]   | `UnitParsingMaskedTextProvider.RevertValue()`                                        | instance | revert to the previous value      |
|  [08]   | `UnitParsingMaskedTextProvider.SetFormatUnitSystem(UnitSystem, DistanceDisplayMode)` | instance | bind provider unit system         |
|  [09]   | `RichTextAreaWithAlternateText()`                                                    | ctor     | rich text with alternate area     |
|  [10]   | `Slider(string, Control, bool, string, string)`                                      | ctor     | one/two-value slider              |
|  [11]   | `Slider.SetMinMax(double, double)`                                                   | instance | set slider range                  |
|  [12]   | `Slider.SetVaries(bool)`                                                             | instance | mark an indeterminate value       |
|  [13]   | `Slider.Settings(string, string) -> PersistentSettings`                              | instance | persistent settings access        |

- `NumericUpDownWithUnitParsing` props: `Value` `ValueBinding` `ValueUpdateMode` `Prefix` `Suffix` `AlternateText` `Increment` `MinValue` `MaxValue` `DecimalPlaces`; events `SteppingStarted` `SteppingDone` `TextChangingStarted` `TextChangingDone` carry `NumericUpDownWithUnitParsingEventArgs`.
- `UnitParsingMaskedTextProvider` props: `Value` `PreviousValue` (private set) `Increment` `MinValue` `MaxValue`. `RichTextAreaWithAlternateText` props: `RichTextArea` `AlternateTextArea` `ShowAlternateText` `AlternateText` `ReadOnly`. `Slider` props: `Value1` `Value2`.

[LIST_GRID]:

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `LineTypeGridView(RhinoDoc)`                         | ctor     | linetype grid for a document |
|  [02]   | `LineTypeGridView.AddLineTypeItem(Linetype, string)` | instance | add a linetype row           |
|  [03]   | `LineTypeItem(Guid)`                                 | ctor     | one linetype row             |

- `LineTypeGridView` props: `LineTypeCollection` `Document`. `LineTypeItem` props: `Name` `Pattern` `LineTypeId` `LinetypeIndex` `AlwaysModelDistances`.

[VIEWPORT_DIALOG_MENU]:

| [INDEX] | [SURFACE]                                           | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :-------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `ViewportControl()` / `ViewportControl(string)`     | ctor     | embeddable viewport                     |
|  [02]   | `ViewportControl.Refresh()`                         | instance | redraw the viewport                     |
|  [03]   | `RangeDialog(double, double, int, int, bool, bool)` | ctor     | min/max range entry dialog              |
|  [04]   | `RenderContentMenu.AddMenuItem(…)`                  | static   | register a content context-menu command |

- `RenderContentMenu.AddMenuItem` params: `Guid`, `string`, `int`, `SeparatorStyle`, `bool`, `Icon`, `Func<RenderContentCollection, Result>`, `Func<RenderContentCollection, Context, bool>`.
- `ViewportControl.Viewport : RhinoViewport`. `RangeDialog` public fields: `Min` `Max` `Increment` `Decimals`.

[FORMS_DIALOGS]:

| [INDEX] | [SURFACE]                                                                           | [SHAPE] | [CAPABILITY]               |
| :-----: | :---------------------------------------------------------------------------------- | :------ | :------------------------- |
|  [01]   | `BaseDialog()`                                                                      | ctor    | header/content dialog base |
|  [02]   | `CommandDialog()`                                                                   | ctor    | command dialog base        |
|  [03]   | `PrintWidthDialog(string, string, double)`                                          | ctor    | print-width picker         |
|  [04]   | `PropertyListBoxDialog(string, string, IList, IList<string>)`                       | ctor    | property-list editor       |
|  [05]   | `SectionStyleDialog(uint, int, double, double, Color, bool, ObjectSectionFillRule)` | ctor    | section-fill editor        |

- `BaseDialog.Message` `BaseDialog.Content`. `CommandDialog` props/events: `Buttons` `ShowHelpButton` `SavePosition` `UpdateSourceOnApply`; event `HelpButtonClick`.
- `PrintWidthDialog.SelectedWidth`. `PropertyListBoxDialog` props: `Collection` `Values`; `ListProperty.PropertyName` `ListProperty.Value`. `SectionStyleDialog` props: `SectionSource` `HatchIndex` `Scale` `SectionFillRule`.

[FORMS_COLOUR_AND_EXPORT]:

| [INDEX] | [SURFACE]                                                                               | [SHAPE]  | [CAPABILITY]            |
| :-----: | :-------------------------------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `ColorList(string, IEnumerable<ColorListEntry>)`                                        | ctor     | named-colour palette    |
|  [02]   | `ColorList.Default -> ColorList`                                                        | static   | the default palette     |
|  [03]   | `ColorListEntry(string, Color)`                                                         | ctor     | one named-colour entry  |
|  [04]   | `ColorListDialog.ShowDialog(Control) -> DialogResult`                                   | instance | show the palette picker |
|  [05]   | `PrintDialogUi(uint, PersistentSettings)`                                               | ctor     | export façade           |
|  [06]   | `PrintDialogUi.EtoExportSvg(uint, PersistentSettings) -> ViewCaptureSettings`           | static   | SVG export settings     |
|  [07]   | `PrintDialogUi.EtoExportSvgArray(uint, PersistentSettings, FileWriteOptions)`           | static   | multi-view SVG export   |
|  [08]   | `PrintDialogUi.EtoExportPdf(uint, PersistentSettings, bool)`                            | static   | PDF export settings     |
|  [09]   | `PrintDialogUi.ShowSavePdfFileDialog(string, uint, out string) -> DialogResult`         | static   | PDF save dialog         |
|  [10]   | `PrintDialogUi.ShowPrintDialog(string, uint, PersistentSettings, bool, bool) -> Result` | static   | print dialog            |

- `PrintDialogUi.EtoExportSvgArray` returns `ViewCaptureSettings[]`; `EtoExportPdf` returns `List<KeyValuePair<string, ViewCaptureSettings>>` (per-view name → settings).
- `ColorList.Name`. `ColorListEntry` props: `Name` `Color`. `ColorListDialog` props/events: `ColorList` `SelectedEntry`; event `SelectedEntryChanged`.

[THEME_MODEL]:
- `IThemeEntry.Id : string` / `Value : object`; `ThemeBase.Id : string` / `Enumerate() -> IEnumerable<IThemeEntry>`.
- `ThemeState` colours (get-only): `Border` `Background` `Text`; `EntryThemeState.PlaceholderText`; `ScrollbarThemeState.Background` `Border` `Glyph` `Thumb`.
- `ThemeZone` colours (get-only): `Background` `Highlight` `HighlightHover` `GripperDot` `Divider`.
- `ThemeZone` element accessors (get-only): `Button` `Tab -> ButtonThemeElement`, `CheckBox -> CheckBoxThemeElement`, `Entry -> EntryThemeElement`, `List -> ListThemeElement`, `Text -> TextThemeElement`, `Link -> LinkThemeElement`, `Scrollbar -> ScrollbarThemeElement`.
- `ButtonThemeElement` states: `Default` `DefaultHover` `DefaultPressed` `Unchecked` `UncheckedtHover` `UncheckedPressed` (host spelling `UncheckedtHover` verbatim).
- `TextThemeElement`: `Enabled` `Disabled` `Highlight` `HighlightHover` `Secondary`; `ScrollbarThemeElement`: `Size` `ArrowSize` `Radius`.

[RUNTIME_SERVICES]:
- `PlatformServiceProvider.Service : IPlatformService` (static get/set); `ProcessArchitecture : string` (`"x86"`/`"x64"`/`"arm64"`/`"armv7l"`/`"unknown"`).
- `RhinoUiServiceLocator.GetService<T>() where T : class -> T` — typed service resolution.
- `IToolbarsService.UseNewStuff() -> bool` / `GetToolbar(Guid) -> IToolbar`.
- `IToolbar`: `Id` `FileId`; `GetControl(uint, bool) -> Control` / `PanelVisibilityChanged(bool, uint, ShowPanelReason)` / `PanelClosing(uint, bool)` (host param spelling `resaon` verbatim).
- `IPlatformService.MainRhinoWindow : Window` with bitmap/font/icon conversion, `SetWindowPos`, `ShowSemiModal`, control-padding, and `PlaySoundFile` members.
- `HostUtils.WindowsRhinoActivated : event WindowsRhinoActivatedEvent` / `delegate void WindowsRhinoActivatedEvent(bool)`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Collapsible sections layer by capability: a plugin subclasses the narrowest base its section needs and adds it to an `EtoCollapsibleSectionHolder`.
- `RhinoLayout` owns the one padding/spacing/size vocabulary every Rhino-styled table, stack, panel, and control grid reads through its constructor; a screen selects a `PaddingType`/`SpacingType` row, never a pixel literal.
- `NumericUpDownWithUnitParsing` and `UnitParsingMaskedTextProvider` parse and format against a `RhinoDoc` or an explicit `UnitSystem`/`LengthUnit` + `DistanceDisplayMode`, so a length field reads model units without a call-site parse.
- A consumer receives a `ThemeZone` and folds its `Enumerate()` tree of `IThemeEntry` colours; internal constructors make the theme host-owned and read, so a widget styles itself from the zone.
- `PlatformServiceProvider.Service` and `RhinoUiServiceLocator.GetService<T>()` hand a consumer the `IPlatformService`/`IToolbarsService` abstraction, so a cross-platform concern is one interface call, never a per-OS branch.

[STACKING]:
- `api-eto-forms.md`: every control derives from an `Eto.Forms` base (`Panel`, `Drawable`, `TableLayout`, `StackLayout`, `Scrollable`, `GridView`, `Dialog<T>`) — that catalog owns the base widget, layout, and binding surface, and this library adds Rhino styling, settings, and unit-parsing on top.
- `api-rhino-ui.md`: `RhinoEtoApp`/`EtoExtensions` seats a control-library screen inside a document-owned panel, page, or semi-modal dialog, owns the `EtoCollapsibleSection`(+`Holder`) page-section seam, and supplies the `DataSource.ProviderIds` `Guid` and `DataSource.EventArgs`/`EventInfoArgs` payload crossing from its RDK data-source surface.
- `api-languageext.md`(`../../.api/api-languageext.md`): a `RangeDialog`/`ColorListDialog`/`PrintDialogUi` outcome and a `NumericUpDownWithUnitParsing` parse land on `Fin<A>` (a cancelled dialog is a `Fail`), a `ThemeZone.Enumerate()` folds as `Seq<IThemeEntry>`, and a `PlatformServiceProvider.Service`/`RhinoUiServiceLocator.GetService<T>()` lookup crosses as `Option<A>`.
- `api-thinktecture-runtime-extensions.md`(`../../.api/api-thinktecture-runtime-extensions.md`): the control enums map at the edge to `[SmartEnum]` owners and the `[Flags]` `NumericUpDownWithUnitParsingUpdateMode`/`DisablePanelColorStylingProperty` to flag owners, so the domain composes the bounded vocabulary.
- `api-unicolour.md`(`../../.api/api-unicolour.md`): the `DisplayAndPrintColorPicker`/`ColorList` colour values map to and from `Unicolour` at the view edge, so display and print colour selection stays in the perceptual model.

[LOCAL_ADMISSION]:
- A control is trapped and mapped at the boundary; its `Eto.Forms.*` base stays behind the Rasm.Rhino UI owner, and a `nint CppPointer`/`EventInfoPtr` native handle never crosses into a domain signature.
- Access is the ruling filter: only the genuinely-public constructible or subclassable surface lands. `Rhino.UI.Annotations`, `Rhino.UI.DialogPanels`, `Rhino.UI.ViewModels`, `Rhino.UI.ObjectProperties`, and `Rhino.UI.ObjectManager` are excluded — internal formatting helpers and Rhino's own registered dockable panels, view-models, and widgets, `public` only for the `IPanel` host and the Eto control hierarchy.
- `ObjectPropertiesPage` in `api-rhino-ui.md` owns the plugin-facing object-properties API.

[RAIL_LAW]:
- Package: `RhinoCommon` + `Rhino.UI` (`Rhino.UI.dll` control library)
- Owns: the `Rhino.UI.Controls` collapsible-section family, layout/panel/button/label/colour/list/numeric-text controls, the viewport control and range/content-menu dialogs, the `Rhino.UI.Forms` dialog bases and colour palette and export façade, the read-only `Rhino.UI.Theme` colour-model tree, and the `Rhino.UI.Runtime` platform-service contracts.
- Accept: a Rhino-styled control composed from an `Eto.Forms` base and seated through the host bridge, a padding/spacing-typed layout, a unit-aware numeric field, a themed read of a `ThemeZone`, and a platform capability resolved through `IPlatformService`.
- Reject: re-implementing an `Eto.Forms` base control (`api-eto-forms.md` owns it), a pixel-literal layout where a `RhinoLayout` type fits, authoring a `ThemeZone` (internal-constructed, read-only), a `CppPointer`/`EventInfoPtr` native handle escaping into a domain signature, and admitting an internal panel, view-model, or object-properties type.
