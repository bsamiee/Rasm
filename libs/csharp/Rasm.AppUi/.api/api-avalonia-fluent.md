# [RASM_APPUI_API_AVALONIA_FLUENT]

`Avalonia.Themes.Fluent` owns the Fluent control-theme floor: a `FluentTheme : Styles, IResourceNode` collection over the `ColorPaletteResources` Windows-Fluent system-token surface, keyed per `ThemeVariant` through `Palettes` and switched between `Normal` and `Compact` density through one direct property. `ColorPaletteResources` derives the six accent shades from a single `Accent` set, so the AppUi `Theme/tokens` resolve fold writes this surface as the host theme spine rather than a parallel token framework.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Themes.Fluent`
- package: `Avalonia.Themes.Fluent` (`MIT`)
- assembly: `Avalonia.Themes.Fluent`
- target: `net10.0`
- namespace: `Avalonia.Themes.Fluent`, `Avalonia.Themes.Fluent.Accents`
- asset: managed runtime library
- asset: embedded `avares://` XAML resources compiled into the assembly
- depends: `Avalonia` (`Styles`, `ResourceProvider`, `ResourceDictionary`, `ThemeVariant`, `Color`, `DirectProperty`) — see `api-avalonia.md`
- rail: theme

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: theme objects and the density vocabulary

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :---------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `FluentTheme`           | class         | `Styles`/`IResourceNode` theme root                     |
|  [02]   | `DensityStyle`          | enum          | `Normal`/`Compact` density vocabulary                   |
|  [03]   | `ColorPaletteResources` | class         | `ResourceProvider` system-token surface (ns `.Accents`) |

[THEME_ASSETS]: embedded `avares://Avalonia.Themes.Fluent/…` XAML families (resource keys, not CLR types)

| [INDEX] | [RESOURCE]                   | [CAPABILITY]                                                            |
| :-----: | :--------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `FluentTheme.xaml`           | theme root dictionary                                                   |
|  [02]   | `Controls/*.xaml`            | per-control themes                                                      |
|  [03]   | `DensityStyles/Compact.xaml` | compact density overlay                                                 |
|  [04]   | `Accents/*.xaml`             | system color/brush keys (`SystemAccentColor`, `SystemBaseHighColor`, …) |

## [03]-[ENTRYPOINTS]

[THEME_ENTRYPOINTS]: `FluentTheme` operations

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `FluentTheme(IServiceProvider?)`                                 | ctor     | admit the theme to `Application.Styles` |
|  [02]   | `FluentTheme.DensityStyle`                                       | property | select `Normal`/`Compact` at runtime    |
|  [03]   | `FluentTheme.DensityStyleProperty`                               | static   | bind target for the density selection   |
|  [04]   | `FluentTheme.Palettes`                                           | property | per-variant accent/system-color map     |
|  [05]   | `FluentTheme.TryGetResource(object, ThemeVariant?, out object?)` | instance | explicit `IResourceNode` resolution     |

- `FluentTheme(...)`: throws `InvalidOperationException` when the embedded `ColorPaletteResourcesCollection` is absent.
- `FluentTheme.DensityStyleProperty`: `DirectProperty<FluentTheme, DensityStyle>`, `BindingMode.OneWay`, default `Normal`.
- `FluentTheme.DensityStyle`: a runtime change swaps the `Compact.xaml` overlay and notifies host resources.
- `FluentTheme.Palettes`: keys accent and system-color overrides by `ThemeVariant.Light`, `.Dark`, or `.Default`.
- `FluentTheme.TryGetResource`: checks the `Compact.xaml` overlay first when `DensityStyle == Compact`.

[PALETTE_TOKENS]: `ColorPaletteResources` settable `Color` properties, each backing XAML key `System<Property>Color`; assigning the type-default `Color` clears the override

| [INDEX] | [SURFACE]                                 | [SHAPE]  | [CAPABILITY]                                     |
| :-----: | :---------------------------------------- | :------- | :----------------------------------------------- |
|  [01]   | `ColorPaletteResources.Accent`            | property | accent seed override                             |
|  [02]   | `ColorPaletteResources.AccentProperty`    | static   | bindable `DirectProperty<…, Color>`              |
|  [03]   | `ColorPaletteResources.HasResources`      | property | true when `Accent` is set or any override exists |
|  [04]   | `ColorPaletteResources.TryGetResource(…)` | instance | resolves derived and overridden token keys       |

- `ColorPaletteResources.Accent`: a set derives `SystemAccentColor{Dark,Light}{1,2,3}` through `SystemAccentColors.CalculateAccentShades(Color)` and raises the resources-changed signal.
- `ColorPaletteResources.TryGetResource(object, ThemeVariant?, out object?)`: resolves derived `SystemAccentColor*` shades and every overridden `System*Color` key.

[ALTERNATE]: `AltHigh` `AltMediumHigh` `AltMedium` `AltMediumLow` `AltLow`
[BASE]: `BaseHigh` `BaseMediumHigh` `BaseMedium` `BaseMediumLow` `BaseLow`
[CHROME]: `ChromeHigh` `ChromeMedium` `ChromeMediumLow` `ChromeLow` `ChromeAltLow`
[CHROME_NEUTRAL]: `ChromeWhite` `ChromeGray` `ChromeBlackHigh` `ChromeBlackMedium` `ChromeBlackMediumLow` `ChromeBlackLow`
[CHROME_DISABLED]: `ChromeDisabledHigh` `ChromeDisabledLow`
[LIST]: `ListLow` `ListMedium`
[SINGLE]: `ErrorText` `RegionColor`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `FluentTheme` composes first in `Application.Styles` as the retained control-theme floor; every accent and system-color override flows through a `ColorPaletteResources` keyed per `ThemeVariant` in `Palettes`, a single `Accent` set derives the six shades that `TryGetResource` serves, and `DensityStyle` swaps the `Compact.xaml` overlay through the one `DensityStyleProperty`.

[STACKING]:
- `api-avalonia.md`: `FluentTheme` extends `Styles`/`IResourceNode` and `Palettes` keys on `Avalonia.Styling.ThemeVariant`; a resolved `ThemeVariantRow` indexes `Palettes` directly, high-contrast riding `new ThemeVariant("high-contrast", ThemeVariant.Dark)` on the `InheritVariant` chain and `host-matched` resolving to `ThemeVariant.Default`.
- `api-semi.md`: `SemiTheme` restyles above this floor in one `Application.Styles` order (`FluentTheme` then `SemiTheme`); the Fluent-templated controls Semi leaves unskinned (`bodong.PropertyGrid`, `DialogHost`) resolve their templates from the floor.
- `api-avalonia-color.md`: `FluentTheme.Palettes` (theme tokens) stays distinct from the editor `IColorPalette` swatch family (`FluentColorPalette`/`MaterialColorPalette`/`FlatColorPalette`); they meet only at the resolved `Color`.
- `Theme/tokens.md`: `ResolvedTheme.Resolve(ThemeVariantRow, DensityRow, mix)` projects `TokenRow.Paint` anchors into one `ColorPaletteResources` per variant and maps `DensityRow` onto `DensityStyle` through `DensityStyleProperty`.

[LOCAL_ADMISSION]:
- `ResolvedTheme` resolves and overrides through `ColorPaletteResources` and `FluentTheme.Palettes`; `Accent` carries the derive-shades contract, one `DensityRow` binds `DensityStyleProperty`, and one `ThemeVariantRow` per host variant indexes `Palettes`.

[RAIL_LAW]:
- Package: `Avalonia.Themes.Fluent`
- Owns: the Fluent control-theme `Styles`/`IResourceNode` floor, the embedded control/accent XAML, the `ColorPaletteResources` system-token surface, the `DensityStyle` vocabulary behind `DensityStyleProperty`, and per-variant override via `Palettes[ThemeVariant]`
- Accept: `ResolvedTheme` resolves and overrides through `ColorPaletteResources`/`Palettes`; shell, sidecar, panel, diagnostics, and support views bind one `DensityRow` onto `DensityStyle`; one `ThemeVariantRow` per host variant indexes `Palettes`, high-contrast riding the `InheritVariant` dark chain
- Reject: a parallel theme framework or second control-theme dictionary; hand-rolled accent-shade math when `Accent` derives it; a host-specific spacing system or per-view density literal outside `DensityRow`; a per-variant palette constructed outside the resolve fold or a string-keyed variant lookup bypassing `ThemeVariant`
