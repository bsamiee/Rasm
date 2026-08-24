# [RASM_APPUI_API_SEMI]

`Semi.Avalonia` re-skins the admitted Avalonia control roster onto one `ThemeVariant`-keyed token vocabulary over the retained `Avalonia.Themes.Fluent` floor — every palette brush, dimension variable, control theme, and glyph an `x:Key` that `SemiTheme : Styles` serves under the `semi:` xmlns.

Hosts instantiate the entry types and one `Application` extension; the rest is resource lookup, and the skins carry these slots onto `DataGrid`, `ColorPicker`, `Dock.Avalonia`, and `AvaloniaEdit`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Semi.Avalonia`
- package: `Semi.Avalonia` (MIT)
- assembly: `Semi.Avalonia` (`IsTrimmable`)
- namespace: `Semi.Avalonia`, `Semi.Avalonia.Tokens`, `Semi.Avalonia.Tokens.Palette`, `Semi.Avalonia.Converters` map to the `semi:` prefix at `https://irihi.tech/semi`; `Semi.Avalonia.Locale` carries the culture dictionaries
- target: `net10.0` and `net8.0` assets
- abi: compiled AXAML — every resource dictionary is IL and every `x:Key` lives inside a `CompiledAvaloniaXaml.!AvaloniaResources.XamlClosure_N` body, so no `.axaml` ships, `--list-resources` yields one opaque `!AvaloniaResources` blob, and no metadata read recovers the vocabulary; the LIVE object graph does carry it, so a product needing the roster as data instantiates the theme and walks it
- depends: `Avalonia`, `Irihi.Avalonia.Shared`
- rail: theme

[PACKAGE_SURFACE]: `Semi.Avalonia.DataGrid`
- package: `Semi.Avalonia.DataGrid` (MIT)
- assembly: `Semi.Avalonia.DataGrid` (`IsTrimmable`)
- namespace: `Semi.Avalonia.DataGrid` under `semi:`
- target: `net10.0` and `net8.0` assets
- depends: `Avalonia.Controls.DataGrid`
- rail: theme

[PACKAGE_SURFACE]: `Semi.Avalonia.ColorPicker`
- package: `Semi.Avalonia.ColorPicker` (MIT)
- assembly: `Semi.Avalonia.ColorPicker` (`IsTrimmable`)
- namespace: `Semi.Avalonia.ColorPicker`, `Semi.Avalonia.ColorPicker.Converters` under `semi:`
- target: `net10.0` and `net8.0` assets
- depends: `Avalonia.Controls.ColorPicker`, `Irihi.Avalonia.Shared`
- rail: theme

[PACKAGE_SURFACE]: `Semi.Avalonia.Dock`
- package: `Semi.Avalonia.Dock`
- assembly: `Semi.Avalonia.Dock`
- namespace: `Semi.Avalonia.Dock`, `Semi.Avalonia.Dock.Controls` under `semi:`
- target: `net10.0` and `net8.0` assets
- depends: `Dock.Avalonia`, `Irihi.Avalonia.Shared`
- rail: theme

[PACKAGE_SURFACE]: `Semi.Avalonia.AvaloniaEdit`
- package: `Semi.Avalonia.AvaloniaEdit`
- assembly: `Semi.Avalonia.AvaloniaEdit`
- namespace: `Semi.Avalonia.AvaloniaEdit` under `semi:`
- target: `net8.0` asset alone — a `net10.0` consumer binds the `net8.0` compile and runtime asset
- depends: `Avalonia.AvaloniaEdit`
- rail: theme

## [02]-[PUBLIC_TYPES]

[ENTRY_TYPE_SCOPE]: `Semi.Avalonia` code surface — the entries a host instantiates

| [INDEX] | [SYMBOL]                                       | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :--------------------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `SemiTheme`                                    | class         | `Styles` root — control themes, tokens, locale, icons, variants   |
|  [02]   | `Tokens.Variables`                             | class         | `ResourceDictionary` of variant-invariant dimension slots         |
|  [03]   | `Tokens.Palette.Light` / `Tokens.Palette.Dark` | class         | `ResourceDictionary` of one variant palette                       |
|  [04]   | `Icons`                                        | class         | `ResourceDictionary` merging the fill, stroked, and AI glyph sets |
|  [05]   | `SemiPopupAnimations`                          | class         | standalone `Styles` popup scale-and-origin animation set          |
|  [06]   | `ApplicationExtension`                         | class         | static `Application` extension binding the OS contrast preference |
|  [07]   | `Locale.<culture>`                             | class         | `ResourceDictionary` of template strings per built-in culture     |
|  [08]   | `ItemConverter`                                | class         | static holder of two `IValueConverter` fields                     |
|  [09]   | `PositionToAngleConverter`                     | class         | `MarkupValueConverter` — percent to degrees over `×3.6`           |
|  [10]   | `PlacementToRenderTransformOriginConverter`    | class         | `MarkupValueConverter` — `PlacementMode` to `RelativePoint`       |

- `ItemConverter` exposes `ItemVisibleConverter` (`int?` over 1) and `ItemToObjectConverter` (`int?` to a repeated-object sequence) as static fields; the type itself converts nothing.

[THEME_VARIANT_SCOPE]: `SemiTheme` seats `ThemeVariant.Default` and `.Light` on `Themes/Light`, `.Dark` on `Themes/Dark`, and each variant below on the one shared `Themes/HighContrast` dictionary with its own `Tokens/HighContrast` slot override, inheriting every unlisted key from its parent

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `SemiTheme.Aquatic`  | property      | high-contrast variant inheriting `ThemeVariant.Dark`  |
|  [02]   | `SemiTheme.Desert`   | property      | high-contrast variant inheriting `ThemeVariant.Light` |
|  [03]   | `SemiTheme.Dusk`     | property      | high-contrast variant inheriting `ThemeVariant.Dark`  |
|  [04]   | `SemiTheme.NightSky` | property      | high-contrast variant inheriting `ThemeVariant.Dark`  |

- Each `Tokens/HighContrast/<Variant>` dictionary resolves to EXACTLY sixteen live keys — `{Window,WindowText,Hotlight,GrayText,Highlight,HighlightText,ButtonText,ButtonFace}Color` and their `SemiColor<Name>` aliases — and carries NO palette rows whatsoever; every other token inherits from the parent variant, so the four are high-contrast system-color mappings over a shared dictionary and never brand palettes.
- Live variant partitions: `Default`, `Light`, and `Dark` carry 449 keys each; `Aquatic`, `Desert`, `Dusk`, and `NightSky` carry 16 each. The whole theme resolves to 1065 plain resource keys, 83 `ControlTheme` target types, and 7 theme-variant dictionaries.

[PALETTE_KEY_SCOPE]: `Tokens.Palette.Light` and `.Dark` carry identical 449-key sets — a bare key is the `SolidColorBrush`, its `<key>Color` twin the `Color` that brush binds

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `SemiColor<Role><State>`                                     | brush family  | 42 slots — 7 roles over 6 states                        |
|  [02]   | `SemiColor<Role>Disabled`                                    | brush family  | 4 slots — `Primary` `Secondary` `Success` `Information` |
|  [03]   | `SemiColor{Background0..4,Fill0..2,Text0..3}`                | brush family  | 12 slots — numbered surface, fill, and text ramps       |
|  [04]   | `SemiColor{Border,FocusBorder,Shadow}`                       | brush family  | 3 slots — chrome outline and shadow tint                |
|  [05]   | `SemiColor{NavBackground,OverlayBackground}`                 | brush family  | 2 slots — the whole scrim and nav vocabulary            |
|  [06]   | `SemiColorLink[Pointerover\|Active\|Visited]`                | brush family  | 4 slots — hyperlink state set                           |
|  [07]   | `SemiColorDisabled{Background,Border,Fill,Text}`             | brush family  | 4 slots — global disabled set                           |
|  [08]   | `SemiColor{Highlight,HighlightBackground,Black,White}`       | brush family  | 4 slots — selection and absolute anchors                |
|  [09]   | `SemiColorAI{General,Purple}[Pointerover\|Active\|Disabled]` | brush family  | 8 slots — AI accent set                                 |
|  [10]   | `SemiColorAIBackground{Top,Bottom}[Pointerover\|Active]`     | brush family  | 6 slots — AI surface gradient stops                     |
|  [11]   | `Semi<Hue><0..9>` and `Semi<Hue><0..9>Color`                 | brush + color | 320 slots — 16 hues over 10 steps, twinned              |
|  [12]   | `SemiAIPurple<0..9>` and `SemiAIPurple<0..9>Color`           | brush + color | 20 slots — AI hue scale, twinned                        |
|  [13]   | `SemiAIGeneral<0..9>`                                        | brush family  | 10 slots — gradient-valued AI scale, no `Color` twin    |
|  [14]   | `SemiBackground<0..4>Color`                                  | color family  | 5 slots — surface-ramp `Color`s, no brush twin          |
|  [15]   | `Semi{Black,White}` and `Semi{Black,White}Color`             | brush + color | 4 slots — absolute anchors                              |
|  [16]   | `SemiShadowElevated`                                         | boxshadows    | the sole global elevation token                         |

- `<Role>`: `Primary` `Secondary` `Tertiary` `Success` `Warning` `Danger` `Information`. `<State>`: bare `Pointerover` `Active` `Light` `LightPointerover` `LightActive`.
- `<Hue>`: `Amber` `Blue` `Cyan` `Green` `Grey` `Indigo` `LightBlue` `LightGreen` `Lime` `Orange` `Pink` `Purple` `Red` `Teal` `Violet` `Yellow`.
- Every semantic brush binds its color through `{StaticResource Semi<Hue><N>Color}` resolved once at dictionary load, so re-seeding a hue-scale slot afterwards re-tints nothing.
- Exactly 20 palette slots hold a `LinearGradientBrush` — `SemiAIGeneral<0..9>`, `SemiColorAIBackground*`, and `SemiColorAIGeneral*`; every other brush slot is a `SolidColorBrush`, so a consumer typing one as `SolidColorBrush` breaks on the AI set alone.

[VARIABLE_KEY_SCOPE]: `Tokens.Variables` — 56 variant-invariant slots merged outside every `ThemeDictionaries` lookup

| [INDEX] | [SYMBOL]                                                  | [TYPE_FAMILY]        | [CAPABILITY]                                         |
| :-----: | :-------------------------------------------------------- | :------------------- | :--------------------------------------------------- |
|  [01]   | `SemiSpacing<Step>`                                       | double family        | 10 slots — 0 2 4 8 12 16 20 24 32 40                 |
|  [02]   | `SemiThickness<Step>`                                     | thickness family     | 10 slots — the same ladder as uniform `Thickness`    |
|  [03]   | `SemiBorderRadius{ExtraSmall,Small,Medium,Large,Full}`    | corner-radius family | 5 slots — 3 3 6 12 9999                              |
|  [04]   | `SemiBorderRadiusSpacing<Size>`                           | double family        | 5 slots — the same scalars unwrapped                 |
|  [05]   | `SemiBorderSpacing[Control\|ControlFocus]`                | double family        | 3 slots — 0 1 1                                      |
|  [06]   | `SemiBorderThickness[Control\|ControlFocus]`              | thickness family     | 3 slots — 0 1 1 uniform                              |
|  [07]   | `SemiHeightControl{Small,Default,Large}`                  | double family        | 3 slots — 24 32 40                                   |
|  [08]   | `SemiWidthIcon{ExtraSmall..ExtraLarge}`                   | double family        | 5 slots — 8 12 16 20 24                              |
|  [09]   | `SemiFontSize{Small,Regular}`, `SemiFontSizeHeader<1..6>` | double family        | 8 slots — 12 14, then 32 28 24 20 18 16              |
|  [10]   | `SemiFontWeight{Light,Regular,Bold}`                      | font-weight family   | 3 slots — 200 400 600                                |
|  [11]   | `SemiFontFamilyRegular`                                   | font-family          | `fonts:Inter#Inter` ahead of the platform sans chain |

- `<Step>`: `None` `SuperTight` `ExtraTight` `Tight` `BaseTight` `Base` `BaseLoose` `Loose` `ExtraLoose` `SuperLoose`. `<Size>`: `ExtraSmall` `Small` `Medium` `Large` `Full`.

[CONTROL_THEME_SCOPE]: `Controls/*.axaml` — 65 dictionaries carrying 83 implicit `{x:Type}` themes and 111 named keys an element attaches through `Theme`

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]       | [CAPABILITY]                                        |
| :-----: | :------------------------------------------ | :------------------ | :-------------------------------------------------- |
|  [01]   | `{Solid,Outline,Borderless}<ButtonKind>`    | ControlTheme family | 12 keys — 3 fills over `Button` and the three kinds |
|  [02]   | `<Chrome>Tab<Part>`                         | ControlTheme family | 22 keys — chrome prefix over the tab parts          |
|  [03]   | `<Card>{RadioButton,CheckBox}`              | ControlTheme family | 6 keys — single radio and check chrome              |
|  [04]   | `[<Card>]{Radio,Check}GroupListBox[Item]`   | ControlTheme family | 12 keys — grouped selection hosts and their items   |
|  [05]   | `{Card,Split,CardSplit}Expander`            | ControlTheme family | 4 keys — expander chrome and its header toggle      |
|  [06]   | `<Owner>{Flyout,ContextFlyout,Presenter}`   | ControlTheme family | 8 keys — flyout and context-menu presenters         |
|  [07]   | `Title<TextKind>` / `TagLabel` / `*TextBox` | ControlTheme family | 6 keys — title, tag, lookless, and non-error text   |
|  [08]   | `Carousel*` / `PipsPager*`                  | ControlTheme family | 7 keys — carousel and pips indicator arms           |
|  [09]   | `DateTimePicker{Button,Up,Down,Item}`       | ControlTheme family | 4 keys — picker spinner and item parts              |
|  [10]   | `{Menu,Static}ScrollViewer` / `ScrollBar*`  | ControlTheme family | 4 keys — scroll host and repeat-button arms         |
|  [11]   | the per-control remainder                   | ControlTheme family | 26 keys, spelled below                              |

- `<ButtonKind>`: `Button` `DropDownButton` `RepeatButton` `SplitButton`. `<Chrome>`: `Base` `Button` `Card` `Line` `Scroll`. `Tab<Part>`: `TabControl` `TabItem` `TabStrip` `TabStripItem` `TabbedPage` — the prefix crosses the parts without minting every pairing.
- `<Card>`: `Button` `Card` `PureCard` `Simple`; the group row also admits the bare prefix. `Title<TextKind>`: `TitleTextBlock` `TitleLabel` `TitleSelectableTextBlock`, beside `LooklessTextBox` and `NonErrorTextBox`.
- Remainder: `AdornerLayerBorder` `ButtonSpinnerRepeatButton` `ButtonToggleSwitch` `CaptionButton` `CardBorder` `CommandBarButtonBaseTheme` `GroupBox` `Icons` `InnerIconButton` `InnerPathIcon` `InputToggleButton` `KeyGestureConverter` `ProgressRing` `RadioButtonGroupBorder` `SemiSplitButtonElement` `SilentDataValidationErrors` `SimpleToggleSwitch` `SliderHorizontalRepeatButton` `SliderThumbTheme` `SliderVerticalRepeatButton` `SplitButtonSpinner` `StringFormatConverter` `TableViewColumnHeaderResizerTemplate` `ToggleButtonTreeViewItemIconButton` `TooltipDataValidationErrors` `TopLevelMenuItem`.
- Intent arms select inside a theme by class: `Primary` `Secondary` `Tertiary` `Quaternary` `Success` `Warning` `Danger` `Colorful` `Ghost` `Solid` `Light` `Accent` `Bordered` `Underline`, sized by `Small` `Large` `ExtraSmall` `ExtraLarge`.
- `Button` carries `Large`/`Small` alongside every intent; `SolidButton` drops the size arms, `OutlineButton` keeps `Primary` `Success` `Warning` `Danger` `Colorful`, and `BorderlessButton` carries `:disabled` alone.

[CONTROL_SLOT_SCOPE]: per-control slots outside the `Semi*` vocabulary, named `<Control><Part><State>` and resolved through the variant dictionaries

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]    | [CAPABILITY]                                                 |
| :-----: | :------------------------------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `Themes/Shared/<Control>.axaml`        | dimension slots  | 359 variant-invariant padding, size, margin, and radius keys |
|  [02]   | `Themes/{Light,Dark}/<Control>.axaml`  | brush slots      | 624 brush keys in `Light`, 625 in `Dark`, same roster        |
|  [03]   | `Themes/HighContrast/<Control>.axaml`  | brush slots      | 411 keys serving all four Semi-owned variants                |
|  [04]   | `<Control>BoxShadow[s]`                | boxshadows       | 11 elevation slots carved from `Light` and `Dark` alone      |
|  [05]   | `LabelTagColorfulGradient<Fill><Part>` | gradient brushes | 7 keys — the only control-scoped gradient slots              |

- Elevation slots: `BorderCardBoxShadow` `FlyoutBorderBoxShadow` `MenuFlyoutBorderBoxShadow` `ComboBoxPopupBoxShadow` `AutoCompleteBoxPopupBoxShadow` `CommandBarOverflowBoxShadow` `CalendarDatePickerPopupBoxShadows` `DateTimePickerFlyoutBoxShadow` `NotificationCardBoxShadows` `ToggleSwitchIndicatorBoxShadow` `WindowBorderShadow`.
- `<Fill>`: `Light` `Ghost` `Solid`. `<Part>`: `Foreground` `Background`, with `BorderBrush` minted for `Ghost` alone.

[ICON_KEY_SCOPE]: `Icons` — 507 `Geometry` path resources across three merged sets, consumed as `PathIcon.Data`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [CAPABILITY]                                    |
| :-----: | :---------------------- | :-------------- | :---------------------------------------------- |
|  [01]   | `SemiIcon<Name>`        | geometry family | 309 filled glyphs — `Icons/FillIcons.axaml`     |
|  [02]   | `SemiIcon<Name>Stroked` | geometry family | 190 outline glyphs — `Icons/StrokedIcons.axaml` |
|  [03]   | `SemiIconAI<Name>`      | geometry family | 8 AI glyphs — `Icons/AIIcons.axaml`             |

- AI set: `SemiIconAIBell` `SemiIconAIEdit` `SemiIconAIFile` `SemiIconAIFilled` `SemiIconAIImage` `SemiIconAISearch` `SemiIconAIStroked` `SemiIconAIWand`.
- `Semi.Avalonia.Icons : ResourceDictionary` is public with a parameterless constructor merging the three sets, so the whole glyph vocabulary is addressable as DATA through one instance and `TryGetValue` — a product needing the geometries never transcribes a path roster.
- Every entry registers through `ResourceDictionary.AddDeferred` and its factory answers `StreamGeometry.Parse(<path data>)`, so a key resolves to an `Avalonia.Media.StreamGeometry` and never to the raw string; the dictionary builds each value on first read and REPLACES the deferred factory with it, so the geometry is created once and shared by every later reader.
- That replacement is an unguarded dictionary write on the read path, and the same read parks a re-entrancy key while the factory runs, so glyph lookup belongs to the UI thread; a concurrent first read from a background thread corrupts the backing dictionary.

[SKIN_TYPE_SCOPE]: the per-control skin packages' code surface

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `DataGrid.DataGridSemiTheme`                     | class         | `Styles` entry re-skinning `Avalonia.Controls.DataGrid`         |
|  [02]   | `ColorPicker.ColorPickerSemiTheme`               | class         | `Styles` entry re-skinning `Avalonia.Controls.ColorPicker`      |
|  [03]   | `ColorPicker.SemiColor{Light,Dark}Palette`       | class         | `IColorPalette` — 17 colors over 10 shades, index-clamped       |
|  [04]   | `ColorPicker.Converters.ColorToTextConverter`    | class         | `MarkupValueConverter` for the hex entry field                  |
|  [05]   | `ColorPicker.Converters.HsvColorToTextConverter` | class         | `MarkupValueConverter` for the HSV entry field                  |
|  [06]   | `Dock.DockSemiTheme`                             | class         | `Styles` entry re-skinning `Dock.Avalonia`, carrying its locale |
|  [07]   | `Dock.Controls.DropAdornerShape`                 | class         | `Control` rendering the drop target — `DockPosition`, brushes   |
|  [08]   | `AvaloniaEdit.AvaloniaEditSemiTheme`             | class         | `Styles` entry re-skinning `AvaloniaEdit`                       |

## [03]-[ENTRYPOINTS]

[THEME_INSTALL]: `Semi.Avalonia` code surface in full — every other capability is resource lookup

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `SemiTheme()`                                                    | ctor     | `<semi:SemiTheme/>` installs themes, tokens, icons |
|  [02]   | `SemiTheme.Locale`                                               | property | `CultureInfo?` selecting the built-in culture      |
|  [03]   | `SemiTheme.OverrideLocaleResources(Application, CultureInfo?)`   | static   | replace app-scoped localized strings               |
|  [04]   | `SemiTheme.OverrideLocaleResources(StyledElement, CultureInfo?)` | static   | replace element-scoped localized strings           |
|  [05]   | `SemiTheme.{Aquatic,Desert,Dusk,NightSky}`                       | property | the four high-contrast `ThemeVariant` keys         |
|  [06]   | `Application.RegisterFollowSystemTheme()`                        | static   | subscribe `IPlatformSettings.ColorValuesChanged`   |
|  [07]   | `Application.UnregisterFollowSystemTheme()`                      | static   | drop that subscription                             |
|  [08]   | `SemiPopupAnimations()`                                          | ctor     | mount the popup animation set as its own `Styles`  |
|  [09]   | `Icons()` / `Tokens.Variables()` / `Tokens.Palette.Light()`      | ctor     | merge one glyph or token dictionary standalone     |

- `SemiTheme.Locale` resolves `zh-CN` for an unset or unrecognized culture, so an English host SETS the property; a throwing culture parks `CultureInfo.InvariantCulture` and swaps no dictionary.
- `Application.RegisterFollowSystemTheme` guards on `OSPlatform.Windows` and no-ops elsewhere; it tracks contrast, never light and dark, assigning `RequestedThemeVariant` only while `PlatformColorValues.ContrastPreference` reads high and mapping `AccentColor1` onto Aquatic, Desert, Dusk, or NightSky, else `ThemeVariant.Default`.
- `SemiPopupAnimations` names no duration resource: both arms run a `0:0:0.1` inline literal over `ScaleX`/`ScaleY` 0.98 to 1.0, `CubicEaseIn` opening and `CubicEaseOut` closing, so retiming a popup replaces the whole style.

[LOCALE_CULTURES]: `zh-CN` (the fallback) `zh-TW` `en-US` `en-GB` `de-DE` `es-ES` `fr-FR` `it-IT` `it-CH` `ja-JP` `ko-KR` `nl-BE` `nl-NL` `pl-PL` `ru-RU` `uk-UA`

[SKIN_INSTALL]: skin entries, each added to `Application.Styles` below `SemiTheme`

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `DataGridSemiTheme()`                                              | ctor     | `<semi:DataGridSemiTheme/>`                 |
|  [02]   | `ColorPickerSemiTheme()`                                           | ctor     | `<semi:ColorPickerSemiTheme/>`              |
|  [03]   | `DockSemiTheme()`                                                  | ctor     | `<semi:DockSemiTheme/>`                     |
|  [04]   | `DockSemiTheme.Locale`                                             | property | `CultureInfo?` over `zh-CN` `en-US` `ru-RU` |
|  [05]   | `DockSemiTheme.OverrideLocaleResources(Application, CultureInfo?)` | static   | app-scoped Dock string override             |
|  [06]   | `AvaloniaEditSemiTheme()`                                          | ctor     | `<semi:AvaloniaEditSemiTheme/>`             |
|  [07]   | `SemiColorLightPalette()` / `SemiColorDarkPalette()`               | ctor     | drop into `ColorView.Palette`               |
|  [08]   | `SemiColorLightPalette.GetColor(int, int)`                         | instance | clamped color at a color and shade index    |

- `DockSemiTheme.OverrideLocaleResources` also takes `(StyledElement, CultureInfo?)`, and its `Locale` falls back to `zh-CN` on the same unrecognized-culture path `SemiTheme` takes.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Application.Styles` carries one chain ordering the `FluentTheme` floor, `<semi:SemiTheme/>`, the per-control `Semi.Avalonia.*` skins, then `Irihi.Ursa.Themes.Semi`'s `<semi:UrsaSemiTheme/>`; every skin and Ursa entry sits strictly below `SemiTheme` so its tokens resolve.
- Product controls read every `SemiColor*` and control-scoped slot through `{DynamicResource}` — `{StaticResource}` freezes the load-time variant and survives no `ThemeVariant` switch, while the variant-invariant `Tokens.Variables` slots take either form.
- Product themes extend a named or implicit Semi theme through `ControlTheme` `BasedOn` and attach at `StyledElement.Theme`; intent and size ride the `Classes` vocabulary the theme already discriminates.
- Palette re-tints write `ThemeVariant`-scoped overrides of the `SemiColor*` semantic slots, and a brand variant lands as a fifth `ThemeVariant` whose palette the OKLCH ramp populates alongside Aquatic, Desert, Dusk, and NightSky.
- Elevation rides `SemiShadowElevated` for the one global token and the eleven control-scoped `BoxShadow` slots for everything else; the package names no elevation ladder, no acrylic, blur, or backdrop token, and `SemiColorOverlayBackground` is the whole overlay vocabulary.

[STACKING]:
- `libs/dotnet/.api/api-unicolour.md`: `Unicolour.Palette(Unicolour, ColourSpace, int, HueSpan, bool)` ramps the `ControlIntent` vocabulary `Theme/tokens` owns, its `.Rgb.Hex` steps landing as the `Semi<Hue><N>Color` twins and the `SemiColor<Role><State>` brushes reading them; `Contrast(Unicolour)` gates the pairs a high-contrast variant admits.
- `api-ursa.md` `[THEME_INSTALL]`: `<semi:UrsaSemiTheme/>` extends these same slots under the shared `semi:` xmlns, and `UrsaSemiTheme.OverrideLocaleResources` mirrors `SemiTheme.OverrideLocaleResources` so one culture swap drives both dictionaries.
- `api-dock.md` / `api-avaloniaedit.md` / `api-avalonia-grid.md` / `api-avalonia-color.md`: `DockSemiTheme`, `AvaloniaEditSemiTheme`, `DataGridSemiTheme`, and `ColorPickerSemiTheme` re-key those control rosters onto this vocabulary; `SemiColorLightPalette`/`SemiColorDarkPalette` satisfy `ColorView.Palette` as the token-native swatch source.
- within-lib: `Theme/tokens` owns the slot roster and writes the OKLCH output into it, `Theme/typography` binds `SemiFontSize*`/`SemiFontWeight*`/`SemiFontFamilyRegular`, `Theme/assets` reads the `SemiIcon*` geometries as the shipped glyph source, and `Theme/motion` owns popup timing because Semi exposes no duration slot.

[LOCAL_ADMISSION]:
- `Semi.Avalonia.Dock` and `Semi.Avalonia.AvaloniaEdit` ship a file-form end-user licence rather than an SPDX expression; both stay admitted for the theming they own.
- `Semi.Avalonia.DataGrid` and `Semi.Avalonia.AvaloniaEdit` declare no `Irihi.Avalonia.Shared`, so that primitive closure pins against the core and `Semi.Avalonia.{ColorPicker,Dock}` alone.
- `Semi.Avalonia.AvaloniaEdit` ships a `net8.0` asset alone, so its version moves independently of the `12.1.x` core; pin it against the `Avalonia.AvaloniaEdit` it declares.
- `RegisterFollowSystemTheme` decides nothing on the macOS Rhino host — OS light and dark following is the shell's own `RequestedThemeVariant` write against `PlatformSettings.GetColorValues()`.
- `SemiTheme` compiles every dictionary to IL, so no metadata read recovers the slot roster; a product that needs the vocabulary as data instantiates the theme and descends `Styles.Resources` -> `ResourceDictionary.MergedDictionaries` -> `ThemeDictionaries` -> `ControlTheme.Resources`, splitting the `Type`-keyed entries (control themes) from the string-keyed ones (tokens). `MergedDictionaries` and `ThemeDictionaries` sit on the CONCRETE `ResourceDictionary` and not on the `IResourceDictionary` the style surfaces return, so a descent typed on the interface reaches the top-level keys and silently misses every merged and variant-scoped partition.
- `bodong.PropertyGrid` and `DialogHost.Avalonia` keep their Fluent templates and stay off this chain.

[RAIL_LAW]:
- Package: `Semi.Avalonia` + `Semi.Avalonia.{DataGrid,ColorPicker,Dock,AvaloniaEdit}`
- Owns: the active design-token vocabulary over the retained Fluent floor — the palette and variable slots, the implicit and named control themes, the class-driven intent arms, the glyph geometries, the built-in locale strings, and the four high-contrast `ThemeVariant`s
- Accept: one `Application.Styles` chain with every skin below `SemiTheme`; `{DynamicResource}` reads of variant-scoped slots; a product `ControlTheme` with `BasedOn` a Semi theme attached through `StyledElement.Theme`; a brand palette landed as `ThemeVariant`-scoped `SemiColor*` overrides
- Reject: hand-authoring a parallel control-template set or a second token dictionary; loading a skin or `UrsaSemiTheme` without `SemiTheme` or ahead of it; re-seeding a `Semi<Hue><N>` scale to re-tint semantic brushes the load already froze; hex literals in product XAML where a slot exists; a hand-rolled elevation ladder, acrylic token, or swatch table over the shipped shadow, overlay, and `IColorPalette` surfaces
