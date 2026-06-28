# [RASM_APPUI_API_SEMI]

`Semi.Avalonia` is the active design-token theme layer (assembly `Semi.Avalonia.dll`) over the retained `Avalonia.Themes.Fluent` floor: a `SemiTheme : Styles` dictionary plus the `Tokens.Variables`/`Tokens.Palette.Light`/`Tokens.Palette.Dark` resource sets that re-skin the entire admitted control roster to ONE token system. It is overwhelmingly XAML/AXAML — the value is the resource-dictionary token vocabulary (colors, brushes, corner radii, thicknesses, font sizes, the per-control template overrides) keyed onto Avalonia `ThemeVariant`s, NOT a code API. The per-control skin packages (`Semi.Avalonia.DataGrid`/`.ColorPicker`/`.Dock`/`.AvaloniaEdit`) extend the same token system to `DataGrid`, `ColorPicker`, `Dock.Avalonia` (`api-dock.md`), and `AvaloniaEdit` (`api-avaloniaedit.md`); `Irihi.Ursa.Themes.Semi`'s `UrsaSemiTheme : Styles` (`api-ursa.md`) extends it to the Ursa control suite under the SAME `https://irihi.tech/semi` (`semi:`) xmlns this package publishes, so the canonical `Application.Styles` chain is `FluentTheme` floor -> `<semi:SemiTheme/>` -> the per-control `Semi.Avalonia.*` skins -> `<semi:UrsaSemiTheme/>`. This is the upstream of the `Wacton.Unicolour` (`api-unicolour.md`) OKLCH pipeline that materializes the `ControlIntent` + `Theme/tokens` vocabulary — the Semi token keys are the named slots the OKLCH ramp writes, and the theme never displaces the Fluent-templated `bodong.PropertyGrid`/`DialogHost`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Semi.Avalonia` 12.0.3
- package: `Semi.Avalonia`
- license: MIT
- floor: `net10.0` consumer (`lib/net10.0/Semi.Avalonia.dll`); multi-targets net8.0 / net10.0, `net10.0` bound
- assembly: `Semi.Avalonia`
- surface: compiled-AXAML resource dictionaries — `SemiTheme` (the `Styles` entry added via `<semi:SemiTheme/>`), `Tokens.Variables`, `Tokens.Palette.Light`, `Tokens.Palette.Dark`, `SemiPopupAnimations`, `Icons`, the `Locale.*` resource set, and three converters (`ItemConverter`, `PositionToAngleConverter`, `PlacementToRenderTransformOriginConverter`). No domain CODE surface
- depends: `Avalonia` (12.x), `Irihi.Avalonia.Shared` / `Irihi.Avalonia.Shared.Contracts` 0.4.0 (shared primitive closure with Ursa)
- rail: theme

[PACKAGE_SURFACE]: `Semi.Avalonia.DataGrid` 12.0.0 / `Semi.Avalonia.ColorPicker` 12.0.3 / `Semi.Avalonia.Dock` 12.0.0.2 / `Semi.Avalonia.AvaloniaEdit` 12.0.0
- packages: `Semi.Avalonia.{DataGrid,ColorPicker,Dock,AvaloniaEdit}`
- license: MIT
- floor: `net10.0` consumer per package
- surface: per-control compiled-AXAML skin dictionaries, each added to `Application.Styles` AFTER `SemiTheme` to re-skin `Avalonia.Controls.DataGrid`, `Avalonia.Controls.ColorPicker`, `Dock.Avalonia` (`api-dock.md`), and `AvaloniaEdit` (`api-avaloniaedit.md`) onto the Semi tokens. Each exposes exactly one `Styles` entry type (`DataGridSemiTheme`/`ColorPickerSemiTheme`/`DockSemiTheme`/`AvaloniaEditSemiTheme`); no other CODE surface
- rail: theme

## [02]-[PUBLIC_TYPES]

[THEME_ENTRIES]: `Semi.Avalonia` resource-dictionary entries (the public surface is XAML keys, not types)
- rail: theme

| [INDEX] | [SYMBOL]                                            | [KIND]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `SemiTheme : Styles`                                | the theme `Styles` entry (`<semi:SemiTheme Locale="…"/>`); carries the full control-template + token dictionary and the locale resource map |
|  [02]   | `Tokens.Variables`                                  | the named token slots (corner radii, thicknesses, spacing, font-size scale, animation durations) the control templates reference |
|  [03]   | `Tokens.Palette.Light` / `Tokens.Palette.Dark`      | the light/dark base color palettes the brushes derive from     |
|  [04]   | `SemiPopupAnimations`                               | shared popup/flyout open/close animation resource set          |
|  [05]   | `Icons`                                             | the built-in geometry/path icon resource set                   |
|  [06]   | `ApplicationExtension`                              | `RegisterFollowSystemTheme(this Application)` / `UnregisterFollowSystemTheme(...)` — the one code entrypoint, OS dark/light follow |
|  [07]   | `Locale.{en_us,en_gb,zh_cn,zh_tw,ja_jp,ko_kr,de_de,fr_fr,es_es,it_it,it_ch,nl_nl,nl_be,pl_pl,ru_ru,uk_ua}` | built-in localized string resources for templated controls |

[SKIN_THEME_ENTRIES]: the per-control `Semi.Avalonia.*` skin `Styles` types — each a `<semi:…/>` entry added to `Application.Styles` AFTER `SemiTheme` so the tokens resolve (the code form is `new …SemiTheme()`)
- rail: theme

| [INDEX] | [SYMBOL]                                                    | [KIND]                                                   |
| :-----: | :---------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `Semi.Avalonia.DataGrid.DataGridSemiTheme : Styles`         | `Avalonia.Controls.DataGrid` skin entry                 |
|  [02]   | `Semi.Avalonia.ColorPicker.ColorPickerSemiTheme : Styles`   | `Avalonia.Controls.ColorPicker` skin entry             |
|  [03]   | `Semi.Avalonia.Dock.DockSemiTheme : Styles`                 | `Dock.Avalonia` (`api-dock.md`) skin entry             |
|  [04]   | `Semi.Avalonia.AvaloniaEdit.AvaloniaEditSemiTheme : Styles` | `AvaloniaEdit` (`api-avaloniaedit.md`) skin entry      |

[THEME_VARIANTS]: `SemiTheme` ships four named `ThemeVariant`s beyond Light/Dark
- rail: theme

| [INDEX] | [SYMBOL]                                            | [KIND]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `SemiTheme.Aquatic`                                 | dark-derived `ThemeVariant` (`new ThemeVariant("Aquatic", ThemeVariant.Dark)`) |
|  [02]   | `SemiTheme.Desert`                                  | light-derived `ThemeVariant`                                   |
|  [03]   | `SemiTheme.Dusk`                                    | dark-derived `ThemeVariant`                                    |
|  [04]   | `SemiTheme.NightSky`                                | dark-derived `ThemeVariant`                                    |

[THEME_CONVERTERS]: `Semi.Avalonia.Converters` (template-internal value converters, public for XAML binding reuse)
- rail: theme

| [INDEX] | [SYMBOL]                                            | [KIND]                                                          |
| :-----: | :-------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `ItemConverter`                                     | items-presenter content converter                              |
|  [02]   | `PositionToAngleConverter`                          | placement-to-rotation-angle converter                          |
|  [03]   | `PlacementToRenderTransformOriginConverter`         | placement-to-transform-origin converter                        |

## [03]-[ENTRYPOINTS]

[THEME_INSTALL]: `SemiTheme` load + locale + OS-follow entrypoints (the only CODE surface; everything else is XAML resource lookup)
- rail: theme

| [INDEX] | [SURFACE]                                                              | [SURFACE_ROOT]         | [RAIL]                                              |
| :-----: | :-------------------------------------------------------------------- | :--------------------- | :------------------------------------------------- |
|  [01]   | `<semi:SemiTheme/>` in `Application.Styles`                            | `SemiTheme`            | install the token theme over the Fluent floor      |
|  [02]   | `Locale` (CultureInfo? property / `Locale="zh_CN"` attribute)         | `SemiTheme`            | select the built-in template-string culture        |
|  [03]   | `OverrideLocaleResources(Application, CultureInfo?)` / `OverrideLocaleResources(StyledElement, CultureInfo?)` | `SemiTheme` (static) | swap localized template strings app- or element-wide |
|  [04]   | `RegisterFollowSystemTheme(this Application)` / `UnregisterFollowSystemTheme(...)` | `ApplicationExtension` | bind `ActualThemeVariant` to OS dark/light (`PlatformColorValues`) |

## [04]-[IMPLEMENTATION_LAW]

[THEME_LAW]:
- Package: `Semi.Avalonia` (+ the per-control skins)
- Owns: the active design-token theme over the retained `Avalonia.Themes.Fluent` base — `SemiTheme` (control templates + token dictionary), `Tokens.Variables`/`Tokens.Palette.Light`/`Tokens.Palette.Dark` (the named slots), `SemiPopupAnimations`, `Icons`, the built-in `Locale.*` strings, and the four extra `ThemeVariant`s (Aquatic/Desert/Dusk/NightSky). The skin packages extend the same tokens to `DataGrid`, `ColorPicker`, `Dock.Avalonia` (`api-dock.md`), and `AvaloniaEdit` (`api-avaloniaedit.md`).
- Accept: the single `Application.Styles` chain is ordered `FluentTheme` floor -> `<semi:SemiTheme/>` -> the per-control `Semi.Avalonia.*` skins -> `Irihi.Ursa.Themes.Semi`'s `<semi:UrsaSemiTheme/>` (`api-ursa.md` `THEME_BRIDGE_LAW` carries the same chain), every Ursa/skin entry strictly BELOW `SemiTheme` so its tokens resolve; the `Theme/tokens` owner reads/writes the Semi token slots, and `RegisterFollowSystemTheme` binds the active variant to the OS where the host exposes it.
- Reject: hand-authoring a parallel control-template set or a second token dictionary; loading a per-control skin (`Semi.Avalonia.Dock`/`.DataGrid`/`.ColorPicker`/`.AvaloniaEdit`) or `UrsaSemiTheme` WITHOUT `SemiTheme`, or ahead of it in the chain (the tokens resolve to nothing); using the obsolete `Ursa.Themes.Semi.Legacy.SemiTheme` (`u-semi:`) in place of `UrsaSemiTheme`; displacing the Fluent-templated `bodong.PropertyGrid`/`DialogHost`, which intentionally keep the Fluent base.

[TOKEN_PIPELINE_LAW]:
- The Semi token keys (`Tokens.Variables` + `Tokens.Palette.Light`/`Dark`) are the named slots the `Wacton.Unicolour` (`api-unicolour.md`) OKLCH pipeline writes: the `ControlIntent` + `Theme/tokens` vocabulary materializes an OKLCH ramp into the Semi color/brush slots, so a derived or branded variant is produced by overriding the palette slots, never by re-templating controls.
- Accept: the OKLCH pipeline overrides `ThemeVariant`-scoped palette resources; a new brand theme is a fifth `ThemeVariant` whose palette the Unicolour ramp populates, parallel to Aquatic/Desert/Dusk/NightSky.
- Reject: computing control colors outside the Unicolour OKLCH owner; encoding raw hex literals in product XAML where a Semi token slot exists; duplicating the palette across packages instead of overriding the shared slots.
