# [RASM_APPUI_API_SEMI]

`Semi.Avalonia` is the design-token theme layer over the `Avalonia.Themes.Fluent` floor: a `SemiTheme : Styles` dictionary with the `Tokens.Variables`/`Tokens.Palette.*` resource sets re-skinning the admitted control roster to ONE token system keyed onto `ThemeVariant`s — resource-dictionary vocabulary, never a code API. Skin packages and `UrsaSemiTheme` extend the same tokens under one `semi:` xmlns. Semi token keys are the slots the `Wacton.Unicolour` OKLCH ramp writes; the Fluent-templated `bodong.PropertyGrid`/`DialogHost` stay untouched.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Semi.Avalonia`
- package: `Semi.Avalonia` (MIT)
- floor: `net10.0` consumer (`lib/net10.0/Semi.Avalonia.dll`); multi-targets net8.0 / net10.0, `net10.0` bound
- assembly: `Semi.Avalonia`
- surface: compiled-AXAML resource dictionaries — `SemiTheme` (the `Styles` entry added via `<semi:SemiTheme/>`), `Tokens.Variables`, `Tokens.Palette.Light`, `Tokens.Palette.Dark`, `SemiPopupAnimations`, `Icons`, the `Locale.*` resource set, and three converters (`ItemConverter`, `PositionToAngleConverter`, `PlacementToRenderTransformOriginConverter`). No domain CODE surface
- depends: `Avalonia`, `Irihi.Avalonia.Shared` / `Irihi.Avalonia.Shared.Contracts` (shared primitive closure with Ursa)
- rail: theme

[PACKAGE_SURFACE]: `Semi.Avalonia.DataGrid` / `Semi.Avalonia.ColorPicker` / `Semi.Avalonia.Dock` / `Semi.Avalonia.AvaloniaEdit`
- packages: `Semi.Avalonia.{DataGrid,ColorPicker,Dock,AvaloniaEdit}` (MIT)
- floor: `net10.0` consumer per package
- surface: per-control compiled-AXAML skin dictionaries, each added to `Application.Styles` AFTER `SemiTheme` to re-skin `Avalonia.Controls.DataGrid`, `Avalonia.Controls.ColorPicker`, `Dock.Avalonia` (`api-dock.md`), and `AvaloniaEdit` (`api-avaloniaedit.md`) onto the Semi tokens. Each exposes exactly one `Styles` entry type (`DataGridSemiTheme`/`ColorPickerSemiTheme`/`DockSemiTheme`/`AvaloniaEditSemiTheme`); no other CODE surface
- rail: theme

## [02]-[PUBLIC_TYPES]

[THEME_ENTRIES]: `Semi.Avalonia` resource-dictionary entries (the public surface is XAML keys, not types)
- rail: theme

| [INDEX] | [SYMBOL]                                       | [KIND]                                       |
| :-----: | :--------------------------------------------- | :------------------------------------------- |
|  [01]   | `SemiTheme : Styles`                           | templates, tokens, and locale resources      |
|  [02]   | `Tokens.Variables`                             | dimensions, typography, and animation tokens |
|  [03]   | `Tokens.Palette.Light` / `Tokens.Palette.Dark` | base palettes from which brushes derive      |
|  [04]   | `SemiPopupAnimations`                          | popup and flyout animation resources         |
|  [05]   | `Icons`                                        | built-in geometry and path resources         |
|  [06]   | `ApplicationExtension`                         | OS theme-follow registration                 |
|  [07]   | `Locale.*`                                     | localized strings for templated controls     |

[SEMI_THEME_ENTRY]: `<semi:SemiTheme Locale="…"/>` loads the control templates, token dictionary, and locale resource map.

[SYSTEM_THEME_METHODS]: `RegisterFollowSystemTheme(this Application)` and `UnregisterFollowSystemTheme(...)` form the sole code entrypoint.

[LOCALE_KEYS]: `Locale.{en_us,en_gb,zh_cn,zh_tw,ja_jp,ko_kr,de_de,fr_fr,es_es,it_it,it_ch,nl_nl,nl_be,pl_pl,ru_ru,uk_ua}` carries the built-in locale set.

[SKIN_THEME_ENTRIES]: the per-control `Semi.Avalonia.*` skin `Styles` types — each a `<semi:…/>` entry added to `Application.Styles` AFTER `SemiTheme` so the tokens resolve (the code form is `new …SemiTheme()`)
- rail: theme

| [INDEX] | [SYMBOL]                                                    | [KIND]                                            |
| :-----: | :---------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `Semi.Avalonia.DataGrid.DataGridSemiTheme : Styles`         | `Avalonia.Controls.DataGrid` skin entry           |
|  [02]   | `Semi.Avalonia.ColorPicker.ColorPickerSemiTheme : Styles`   | `Avalonia.Controls.ColorPicker` skin entry        |
|  [03]   | `Semi.Avalonia.Dock.DockSemiTheme : Styles`                 | `Dock.Avalonia` (`api-dock.md`) skin entry        |
|  [04]   | `Semi.Avalonia.AvaloniaEdit.AvaloniaEditSemiTheme : Styles` | `AvaloniaEdit` (`api-avaloniaedit.md`) skin entry |

[THEME_VARIANTS]: `SemiTheme` ships four named `ThemeVariant`s beyond Light/Dark
- rail: theme

| [INDEX] | [SYMBOL]             | [KIND]                                                                         |
| :-----: | :------------------- | :----------------------------------------------------------------------------- |
|  [01]   | `SemiTheme.Aquatic`  | dark-derived `ThemeVariant` (`new ThemeVariant("Aquatic", ThemeVariant.Dark)`) |
|  [02]   | `SemiTheme.Desert`   | light-derived `ThemeVariant`                                                   |
|  [03]   | `SemiTheme.Dusk`     | dark-derived `ThemeVariant`                                                    |
|  [04]   | `SemiTheme.NightSky` | dark-derived `ThemeVariant`                                                    |

[THEME_CONVERTERS]: `Semi.Avalonia.Converters` 'template-internal value converters, public for XAML binding reuse'
- rail: theme

| [INDEX] | [SYMBOL]                                    | [KIND]                                  |
| :-----: | :------------------------------------------ | :-------------------------------------- |
|  [01]   | `ItemConverter`                             | items-presenter content converter       |
|  [02]   | `PositionToAngleConverter`                  | placement-to-rotation-angle converter   |
|  [03]   | `PlacementToRenderTransformOriginConverter` | placement-to-transform-origin converter |

## [03]-[ENTRYPOINTS]

[THEME_INSTALL]: `SemiTheme` load + locale + OS-follow entrypoints (the only CODE surface; everything else is XAML resource lookup)
- rail: theme

| [INDEX] | [SURFACE]                   | [SURFACE_ROOT]         | [RAIL]                                    |
| :-----: | :-------------------------- | :--------------------- | :---------------------------------------- |
|  [01]   | `<semi:SemiTheme/>`         | `SemiTheme`            | install tokens over the Fluent floor      |
|  [02]   | `Locale`                    | `SemiTheme`            | select the template-string culture        |
|  [03]   | `OverrideLocaleResources`   | `SemiTheme`            | replace app- or element-scoped strings    |
|  [04]   | `RegisterFollowSystemTheme` | `ApplicationExtension` | bind `ActualThemeVariant` to the OS theme |

[LOCALE_SURFACE]: a nullable `CultureInfo` property and the `Locale="zh_CN"` attribute select a built-in culture.

[LOCALE_OVERRIDE_OVERLOADS]: `OverrideLocaleResources(Application, CultureInfo?)` and `OverrideLocaleResources(StyledElement, CultureInfo?)` replace localized strings at their respective scopes.

[SYSTEM_THEME_SURFACE]: `RegisterFollowSystemTheme(this Application)` reads `PlatformColorValues`; `UnregisterFollowSystemTheme(...)` removes the binding.

## [04]-[IMPLEMENTATION_LAW]

[THEME_LAW]:
- Package: `Semi.Avalonia` (+ the per-control skins)
- Owns: the active design-token theme over the retained `Avalonia.Themes.Fluent` base — `SemiTheme` (control templates + token dictionary), `Tokens.Variables`/`Tokens.Palette.Light`/`Tokens.Palette.Dark` (the named slots), `SemiPopupAnimations`, `Icons`, the built-in `Locale.*` strings, and the four extra `ThemeVariant`s (Aquatic/Desert/Dusk/NightSky). Skin packages extend the same tokens to `DataGrid`, `ColorPicker`, `Dock.Avalonia` (`api-dock.md`), and `AvaloniaEdit` (`api-avaloniaedit.md`).
- Accept: the single `Application.Styles` chain is ordered `FluentTheme` floor -> `<semi:SemiTheme/>` -> the per-control `Semi.Avalonia.*` skins -> `Irihi.Ursa.Themes.Semi`'s `<semi:UrsaSemiTheme/>` (`api-ursa.md` `THEME_BRIDGE_LAW` carries the same chain), every Ursa/skin entry strictly BELOW `SemiTheme` so its tokens resolve; the `Theme/tokens` owner reads/writes the Semi token slots, and `RegisterFollowSystemTheme` binds the active variant to the OS where the host exposes it.
- Reject: hand-authoring a parallel control-template set or a second token dictionary; loading a per-control skin (`Semi.Avalonia.Dock`/`.DataGrid`/`.ColorPicker`/`.AvaloniaEdit`) or `UrsaSemiTheme` WITHOUT `SemiTheme`, or ahead of it in the chain (the tokens resolve to nothing); using the obsolete `Ursa.Themes.Semi.Legacy.SemiTheme` (`u-semi:`) in place of `UrsaSemiTheme`; displacing the Fluent-templated `bodong.PropertyGrid`/`DialogHost`, which intentionally keep the Fluent base.

[TOKEN_PIPELINE_LAW]:
- Semi token keys (`Tokens.Variables` + `Tokens.Palette.Light`/`Dark`) are the named slots the `Wacton.Unicolour` (`libs/csharp/.api/api-unicolour.md`, shared tier) OKLCH pipeline writes: the `ControlIntent` + `Theme/tokens` vocabulary materializes an OKLCH ramp into the Semi color/brush slots, so a derived or branded variant is produced by overriding the palette slots, never by re-templating controls.
- Accept: the OKLCH pipeline overrides `ThemeVariant`-scoped palette resources; a new brand theme is a fifth `ThemeVariant` whose palette the Unicolour ramp populates, parallel to Aquatic/Desert/Dusk/NightSky.
- Reject: computing control colors outside the Unicolour OKLCH owner; encoding raw hex literals in product XAML where a Semi token slot exists; duplicating the palette across packages instead of overriding the shared slots.
