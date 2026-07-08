# [RASM_APPUI_API_AVALONIA_FLUENT]

`Avalonia.Themes.Fluent` is the Fluent control-theme `Styles` collection plus the `ColorPaletteResources` accent/system-token surface the AppUi theme rail resolves into. `FluentTheme` carries one runtime-switchable `DensityStyle` direct property and a `ThemeVariant`-keyed `Palettes` dictionary; `ColorPaletteResources` exposes the full Windows-Fluent system-color token set as settable `Color` properties, deriving the six accent shades from a single `Accent` set. The `Theme/tokens.md` resolve fold projects its `Paint` anchors into a `ColorPaletteResources` instance per variant and maps its `DensityRow` onto `DensityStyle`, so this package is the host theme spine that the page's `ResolvedTheme` writes through rather than a parallel token framework.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Themes.Fluent`
- package: `Avalonia.Themes.Fluent`
- license: `MIT`
- assembly: `Avalonia.Themes.Fluent`
- target: `net10.0` (multi-targets `net8.0`/`net10.0`; the AppUi consumer binds the `net10.0` asset)
- namespace: `Avalonia.Themes.Fluent`
- namespace: `Avalonia.Themes.Fluent.Accents`
- asset: managed runtime library
- asset: embedded `avares://` XAML resources (compiled into the assembly, not loose files)
- depends: `Avalonia` (`Styles`, `ResourceProvider`, `ResourceDictionary`, `ThemeVariant`, `Color`, `DirectProperty`) — see `api-avalonia.md`
- rail: theme

## [02]-[PUBLIC_TYPES]

[THEME_TYPES]: public theme objects
- rail: theme

| [INDEX] | [SYMBOL]                | [BASE_KIND]                              | [RAIL]            |
| :-----: | :---------------------- | :----------------------------------------- | :---------------- |
|  [01]   | `FluentTheme`           | `: Styles, IResourceNode`                  | theme root        |
|  [02]   | `DensityStyle`          | `enum { Normal, Compact }`                 | density vocabulary|
|  [03]   | `ColorPaletteResources` | `: ResourceProvider` (ns `…Accents`)       | palette resource  |

[INTERNAL_TYPES]: not public — consume only the surface above
- rail: theme

| [INDEX] | [SYMBOL]                          | [VISIBILITY] | [NOTE]                                                              |
| :-----: | :-------------------------------- | :----------- | :----------------------------------------------------------------- |
|  [01]   | `ColorPaletteResourcesCollection` | `internal`   | the `IDictionary<ThemeVariant, ColorPaletteResources>` `FluentTheme.Palettes` returns; reached only through the `Palettes` property, never `new`-d |
|  [02]   | `SystemAccentColors`              | `internal`   | owns `CalculateAccentShades(Color)`; runs implicitly when `Accent` is set |

[THEME_ASSET_GROUPS]: embedded `avares://Avalonia.Themes.Fluent/…` XAML families (resource keys, not CLR types)
- rail: theme

| [INDEX] | [RESOURCE]                  | [RAIL]            |
| :-----: | :-------------------------- | :---------------- |
|  [01]   | `FluentTheme.xaml`          | theme root        |
|  [02]   | `Controls/*.xaml`           | control themes    |
|  [03]   | `DensityStyles/Compact.xaml`| compact overlay   |
|  [04]   | `Accents/*.xaml`            | system color/brush keys (`SystemAccentColor`, `SystemBaseHighColor`, …) |

## [03]-[ENTRYPOINTS]

[THEME_ENTRYPOINTS]: `FluentTheme` operations
- rail: theme
- surface: `FluentTheme`

| [INDEX] | [SURFACE]                                                       | [SHAPE]                                                        | [RAIL]            |
| :-----: | :-------------------------------------------------------------- | :------------------------------------------------------------ | :---------------- |
|  [01]   | `FluentTheme(IServiceProvider? sp = null)`                      | ctor; throws `InvalidOperationException` if the embedded `ColorPaletteResourcesCollection` is absent | theme admission   |
|  [02]   | `DensityStyle DensityStyle { get; set; }`                       | direct property; swaps the `Compact.xaml` overlay and notifies host resources at runtime | density selection |
|  [03]   | `DirectProperty<FluentTheme, DensityStyle> DensityStyleProperty`| registered direct property, default `DensityStyle.Normal`, `BindingMode.OneWay` — the bind target for the page `DensityRow` | density binding   |
|  [04]   | `IDictionary<ThemeVariant, ColorPaletteResources> Palettes { get; }` | per-variant accent/system-color override map, keyed by `ThemeVariant.Light/Dark/Default` | palette mapping    |
|  [05]   | `IResourceNode.TryGetResource(object key, ThemeVariant?, out object?)` | explicit-interface resolve; overlays `Compact.xaml` first when `DensityStyle == Compact` | resource exposure |

[PALETTE_ENTRYPOINTS]: `ColorPaletteResources` settable system-color tokens (all `Color`, default-`Color` set clears the override)
- rail: theme
- surface: `ColorPaletteResources`

| [INDEX] | [SURFACE]                                                  | [BACKING_KEY_NOTE]                                              | [RAIL]            |
| :-----: | :--------------------------------------------------------- | :--------------------------------------------------------------- | :---------------- |
|  [01]   | `Accent`                                                   | `DirectProperty` `AccentProperty`, bindable; setting it derives `SystemAccentColorDark1/2/3` + `Light1/2/3` via `SystemAccentColors.CalculateAccentShades` and raises resources-changed | accent token      |
|  [02]   | `AltHigh` `AltMediumHigh` `AltMedium` `AltMediumLow` `AltLow` | `SystemAlt*Color`                                              | alternate tones   |
|  [03]   | `BaseHigh` `BaseMediumHigh` `BaseMedium` `BaseMediumLow` `BaseLow` | `SystemBase*Color`                                          | base tones        |
|  [04]   | `ChromeHigh` `ChromeMedium` `ChromeMediumLow` `ChromeLow` `ChromeAltLow` | `SystemChrome*Color`                                     | chrome tones      |
|  [05]   | `ChromeWhite` `ChromeGray` `ChromeBlackHigh` `ChromeBlackMedium` `ChromeBlackMediumLow` `ChromeBlackLow` | `SystemChrome*Color`        | chrome neutrals   |
|  [06]   | `ChromeDisabledHigh` `ChromeDisabledLow`                   | `SystemChromeDisabled*Color`                                     | disabled tones    |
|  [07]   | `ListLow` `ListMedium`                                     | `SystemList*Color`                                               | list tones        |
|  [08]   | `ErrorText`                                                | `SystemErrorTextColor`                                           | error token       |
|  [09]   | `RegionColor`                                              | `SystemRegionColor`                                              | region token      |
|  [10]   | `HasResources { get; }`                                    | `override bool` — true when `Accent` is set or any token override exists | resource gate     |
|  [11]   | `TryGetResource(object key, ThemeVariant?, out object?)`   | `override bool` — resolves `SystemAccentColor*` (including derived shades) and any overridden `System*Color` key | token resolve     |

## [04]-[INTEGRATION]

[TOKEN_PROJECTION]: the page resolve fold writes this surface
- The `Theme/tokens.md` `ResolvedTheme.Resolve(ThemeVariantRow, DensityRow, mix)` fold projects its `TokenRow.Paint` anchors into one `ColorPaletteResources` per variant: `new() { Accent = …, BaseHigh = …, BaseMedium = …, BaseLow = …, AltHigh = …, AltMedium = …, ChromeHigh = …, ChromeMedium = …, ChromeLow = …, ErrorText = …, ListLow = …, RegionColor = … }`. Every member it sets is a verified `ColorPaletteResources` property; the un-set tokens (the full Alt/Base/Chrome ladders above) are the in-place growth headroom for that projection, not absent capability.
- Setting `Accent` once yields the six accent shades for free — the page does not author `Dark1/Light1/…`; `SystemAccentColors.CalculateAccentShades` fills them and `TryGetResource` serves them.

[DENSITY_BINDING]: one density vocabulary, one direct property
- The page `DensityRow` `[SmartEnum<string>]` (two rows, `tokens.md` `[04]-[DENSITY_AXIS]`) binds `DensityStyle` and selects `Metric` columns orthogonally to the variant axis. `DensityStyle { Normal, Compact }` is the closed host vocabulary, and `DensityStyleProperty` (`BindingMode.OneWay`, default `Normal`) is the bind target so a density flip swaps the `Compact.xaml` overlay through one host property with no per-control spacing system.

[VARIANT_KEYING]: `ThemeVariant` is the cross-axis discriminant
- `FluentTheme.Palettes` is keyed by `Avalonia.Styling.ThemeVariant` (`Light/Dark/Default`, owned by `api-avalonia.md`). The page `ThemeVariantRow` `[SmartEnum<string>]` carries `ThemeVariant Variant`, constructs high-contrast as `new ThemeVariant("high-contrast", ThemeVariant.Dark)` (the `InheritVariant` chain), and resolves `host-matched` to `ThemeVariant.Default` — so a resolved variant indexes directly into `Palettes` and the host appearance probe (`RunningInDarkMode`, `IPlatformSettings.GetColorValues()`) re-resolves the matching palette.

[EDITOR_PALETTE_SEAM]: distinct from the color-editor palettes
- `FluentTheme.Palettes` (theme accent/system tokens) is NOT the `IColorPalette` swatch family. The editor `FluentColorPalette`/`MaterialColorPalette`/`FlatColorPalette` consumed by `Editing/inspector.md` come from `Avalonia.Controls.ColorPicker` (`api-avalonia-color.md`). Keep the theme palette (resource resolution) and the picker palette (user swatch grid) on their own owners; they meet only at the resolved `Color` value.

## [05]-[IMPLEMENTATION_LAW]

[THEME_LAW]:
- Package: `Avalonia.Themes.Fluent`
- Owns: the Fluent control-theme `Styles` collection, the embedded control/accent XAML, and the `ColorPaletteResources` system-token surface
- Accept: the page `ResolvedTheme` resolves and overrides through `ColorPaletteResources` and `FluentTheme.Palettes`; `Accent` carries the derive-shades contract
- Reject: a parallel theme framework, a second control-theme dictionary, or hand-rolled accent-shade math when `Accent` already derives them

[RETAINED_FLOOR]: the charter is LIVE — one composition-level wiring
- `FluentTheme` is the RETAINED control-theme floor BENEATH Semi (`api-semi.md`): `SemiTheme` restyles the admitted roster to the `Wacton.Unicolour` token system, but the Fluent `Styles` floor stays composed first so the Fluent-templated controls Semi does not skin (`bodong.PropertyGrid`, `DialogHost`) keep a valid control theme — a single App-level `Styles` order (`FluentTheme` then `SemiTheme`), never a second theme framework. The floor is NOT dead: `ResolvedTheme.Resolve` writes its `ColorPaletteResources`/`Palettes` surface every variant flip, and the un-skinned controls resolve their templates from it.

[DENSITY_LAW]:
- Package: `Avalonia.Themes.Fluent`
- Owns: the `DensityStyle { Normal, Compact }` vocabulary and the `Compact.xaml` runtime overlay behind `DensityStyleProperty`
- Accept: shell, sidecar, panel, diagnostics, and support views bind the one `DensityRow` onto `DensityStyle`
- Reject: host-specific spacing systems or a per-view density literal outside the `DensityRow`

[VARIANT_LAW]:
- Package: `Avalonia.Themes.Fluent` (keyed by `ThemeVariant` from `api-avalonia.md`)
- Owns: per-variant accent/system-color override via `Palettes[ThemeVariant]`
- Accept: one `ThemeVariantRow` per host variant indexes `Palettes`; high-contrast rides the `InheritVariant` dark chain
- Reject: a per-variant palette object constructed outside the resolve fold, or string-keyed variant lookup bypassing `ThemeVariant`
