# [RASM_APPUI_API_FLUENTICONS]

`FluentIcons.Common` (MIT) supplies the platform-neutral icon vocabulary: the `Symbol` (2789 members, `int`-backed) and `Icon` enums, the `IconVariant` (byte, 4 cases) and `IconSize` (byte, 9 cases) axes, and their static enumeration helpers. `FluentIcons.Avalonia` (MIT) supplies the font-backed icon controls (`SymbolIcon`/`SymbolImage`/`FluentIcon`/`FluentImage`), the `Outline` attached-property accessor, and four XAML markup extensions. `Common` is multi-target (`netstandard2.1` primary, `netstandard2.0` fallback) and binds `netstandard2.1` under the net10 consumer; `Avalonia` ships a `net10.0` library alongside `net8.0` and binds `net10.0`. The vocabulary is the canonical icon-identity surface; controls render a glyph by enum member rather than by string key or bitmap path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentIcons.Common`

- package / license: `FluentIcons.Common` / MIT
- assembly: `FluentIcons.Common`
- asset: `netstandard2.1` (consumer-bound) + `netstandard2.0` fallback
- namespace: `FluentIcons.Common`
- rail: icons

[PACKAGE_SURFACE]: `FluentIcons.Avalonia`

- package / license: `FluentIcons.Avalonia` / MIT
- assembly: `FluentIcons.Avalonia`
- asset: `net10.0` (consumer-bound) + `net8.0`
- namespace: `FluentIcons.Avalonia`, `FluentIcons.Avalonia.Markup` (public); `FluentIcons.Avalonia.Internals` (bases/converters)
- rail: icons

## [02]-[PUBLIC_TYPES]

[COMMON_TYPES]: shared vocabulary — `FluentIcons.Common`

- rail: icons

`Symbol` and `Icon` are distinct glyph enums. `Symbol` is the full named-glyph set used by Avalonia controls; `Icon` is the smaller resizable-icon set whose non-resizable members carry the `[NonResizable]` attribute, such as `AppStore`, so size selection applies only to unattributed `Icon` members.

| [INDEX] | [SYMBOL]            | [SHAPE]                                | [RAIL]                        |
| :-----: | :------------------ | :------------------------------------- | :---------------------------- |
|  [01]   | `Symbol`            | `int` enum; 2789 members               | Avalonia icon identity        |
|  [02]   | `Icon`              | enum with `[NonResizable]` member tags | resizable-icon identity       |
|  [03]   | `IconVariant`       | `byte` enum; 4 members                 | rendering-variant axis        |
|  [04]   | `IconSize`          | `byte` enum; 9 members                 | pixel-size axis               |
|  [05]   | `IconSizeValues`    | `static` `IEnumerable<IconSize>`       | size binding and iteration    |
|  [06]   | `IconVariantValues` | `static` `IEnumerable<IconVariant>`    | variant binding and iteration |

[ICON_VARIANT_MEMBERS]: `IconVariant` carries `Regular`, `Filled`, `Color`, and `Light`.

[ICON_SIZE_MEMBERS]: `IconSize` carries `Resizable = 0`, `Size10 = 10`, `Size12`, `Size16`, `Size20`, `Size24`, `Size28`, `Size32`, and `Size48 = 48`; each `SizeNN` value equals its pixel size.

[ENUMERATION_MEMBERS]: `IconSizeValues.Enumerable` exposes all 9 sizes, and `IconVariantValues.Enumerable` exposes all 4 variants.

[ICON_CONTROL_TYPES]: icon controls and image sources — `FluentIcons.Avalonia`

- rail: icons

Controls bind to the vocabulary through the internal `IValue<V>`/`GenericIcon` base; `IconVariant` and `FontSize` are inherited base properties, and `Symbol` is the leaf identity property.

| [INDEX] | [SYMBOL]                     | [SHAPE]                          | [RAIL]                     |
| :-----: | :--------------------------- | :------------------------------- | :------------------------- |
|  [01]   | `SymbolIcon`                 | `GenericIcon`, `IValue<Symbol>`  | symbol control             |
|  [02]   | `SymbolImage`                | `GenericImage`, `IValue<Symbol>` | symbol image               |
|  [03]   | `FluentIcon`                 | `GenericIcon`, `IValue<Icon>`    | resizable-icon control     |
|  [04]   | `FluentImage`                | `GenericImage`, `IValue<Icon>`   | resizable-icon image       |
|  [05]   | `Outline`                    | `abstract` attached accessor     | outline-icon attachment    |
|  [06]   | `GenericIcon`                | internal abstract base           | icon control base          |
|  [07]   | `GenericImage`               | internal abstract base           | icon image base            |
|  [08]   | `GenericIconConverter<V,T>`  | internal `TypeConverter` base    | string-to-icon conversion  |
|  [09]   | `GenericImageConverter<V,T>` | internal `TypeConverter` base    | string-to-image conversion |

[OUTLINE_MEMBERS]: `Outline` attaches `Symbol`, `Icon`, `IconVariant`, and `Foreground` properties to any `GenericIcon`.

[INTERNAL_BASES]: `GenericIcon` and `GenericImage` live in `FluentIcons.Avalonia.Internals` and expose the inherited `IconVariant` and `FontSize` properties; consumers do not derive from these bases directly.

[CONVERTER_BASES]: `GenericIconConverter<V,T>` and `GenericImageConverter<V,T>` live in `FluentIcons.Avalonia.Internals` and convert strings to enum-keyed controls and images for XAML.

[MARKUP_TYPES]: XAML markup extensions — `FluentIcons.Avalonia.Markup`

- rail: icons

Each extension is `sealed` with nullable axis properties and a `(member)` constructor; `ProvideValue` returns the corresponding control instance.

| [INDEX] | [SYMBOL]               | [PROVIDES]    | [RAIL]                        |
| :-----: | :--------------------- | :------------ | :---------------------------- |
|  [01]   | `SymbolIconExtension`  | `SymbolIcon`  | inline symbol control         |
|  [02]   | `SymbolImageExtension` | `SymbolImage` | inline symbol image           |
|  [03]   | `FluentIconExtension`  | `FluentIcon`  | inline resizable-icon control |
|  [04]   | `FluentImageExtension` | `FluentImage` | inline resizable-icon image   |

## [03]-[ENTRYPOINTS]

[COMMON_ENTRYPOINTS]: `FluentIcons.Common` vocabulary surfaces

- rail: icons

| [INDEX] | [SURFACE]                      | [SURFACE_ROOT]      | [RAIL]                  |
| :-----: | :----------------------------- | :------------------ | :---------------------- |
|  [01]   | `Symbol.<member>`              | `Symbol`            | primary glyph identity  |
|  [02]   | `Icon.<member>`                | `Icon`              | resizable-icon identity |
|  [03]   | `IconVariant.<member>`         | `IconVariant`       | variant selection       |
|  [04]   | `IconSize.<member>`            | `IconSize`          | pixel-size selection    |
|  [05]   | `IconSizeValues.Enumerable`    | `IconSizeValues`    | all sizes               |
|  [06]   | `IconVariantValues.Enumerable` | `IconVariantValues` | all variants            |

[CONTROL_ENTRYPOINTS]: icon control properties — `FluentIcons.Avalonia`

- rail: icons

| [INDEX] | [SURFACE]                           | [SURFACE_ROOT]               | [RAIL]                      |
| :-----: | :---------------------------------- | :--------------------------- | :-------------------------- |
|  [01]   | `Symbol` (`StyledProperty<Symbol>`) | `SymbolIcon` / `SymbolImage` | glyph identity              |
|  [02]   | `Icon` (`StyledProperty<Icon>`)     | `FluentIcon` / `FluentImage` | resizable-icon identity     |
|  [03]   | `IconVariant`                       | `GenericIcon` base           | variant on any icon control |
|  [04]   | `FontSize`                          | `GenericIcon` base           | glyph scale                 |
|  [05]   | `Outline.{Get,Set}Foreground`       | `Outline.ForegroundProperty` | attached outline brush      |
|  [06]   | `Outline.{Get,Set}Symbol`           | `Outline.SymbolProperty`     | attached symbol identity    |
|  [07]   | `Outline.IconProperty`              | `Outline`                    | attached icon identity      |
|  [08]   | `Outline.IconVariantProperty`       | `Outline`                    | attached rendering variant  |

[CONTROL_PROPERTY_TYPES]: `IconVariant` is a `StyledProperty<IconVariant>`, and `FontSize` is a `StyledProperty<double>`.

[MARKUP_ENTRYPOINTS]: inline XAML construction — `SymbolIconExtension` (representative)

- rail: icons

| [INDEX] | [SURFACE]                                      | [RAIL]           |
| :-----: | :--------------------------------------------- | :--------------- |
|  [01]   | `.ctor(Symbol)` / `.ctor()`                    | inline construct |
|  [02]   | `Symbol` (`Symbol?`)                           | glyph select     |
|  [03]   | `IconVariant` (`IconVariant?`)                 | variant select   |
|  [04]   | `FontSize` (`double?`)                         | glyph scale      |
|  [05]   | `Foreground` (`Brush?`)                        | glyph brush      |
|  [06]   | `ProvideValue(IServiceProvider) -> SymbolIcon` | markup resolve   |

## [04]-[INTEGRATION]

[STACK_THEME_BRUSH]:

- `SymbolIcon.Foreground` / `Outline.ForegroundProperty` accept any Avalonia `IBrush`,
  so theme `DynamicResource` brushes (`api-avalonia-fluent.md`) drive icon colour;
  `IconVariant.Color` selects the multi-colour glyph variant where the font ships one,
  otherwise `Regular`/`Filled` honour the bound brush.

[STACK_NEUTRAL_VOCABULARY]:

- `FluentIcons.Common` carries no Avalonia dependency, so AppHost/Compute/Persistence
  model icon intent as a `Symbol` (or `Icon`) enum value end-to-end; only the AppUi view
  layer references `FluentIcons.Avalonia` to render it. A view-model exposes the enum;
  the view binds it to `SymbolIcon.Symbol`. `IconVariantValues.Enumerable` /
  `IconSizeValues.Enumerable` feed `ItemsSource` for variant/size pickers without
  re-listing the enum.

[STACK_TYPECONVERTER]:

- `GenericIconConverter<V,T>` is the `TypeConverter` that parses a string attribute
  (`Symbol="Save"`) into the enum-keyed control, so XAML authoring stays string-literal
  while the runtime model stays the typed enum — no string-keyed icon registry is needed.

## [05]-[IMPLEMENTATION_LAW]

[COMMON_LAW]:

- Package: `FluentIcons.Common` (MIT, netstandard2.1/2.0)
- Owns: `Symbol`, `Icon`, `IconVariant`, `IconSize`, and the `*Values.Enumerable` helpers as the platform-neutral icon vocabulary
- Accept: reference from any layer needing icon identity without Avalonia; icon intent is a `Symbol`/`Icon` enum value
- Reject: string-keyed icon registries, parallel enum re-declarations, treating `Icon` as a `Symbol` alias

[ICON_LAW]:

- Package: `FluentIcons.Avalonia` (MIT, net10.0/net8.0)
- Owns: font-backed `Symbol`/`Icon` rendering through controls, image sources, the `Outline` attached accessor, and markup extensions
- Accept: icon intent maps to a `Symbol`/`Icon` member with an `IconVariant`; bind colour through `Foreground`/`Outline`
- Reject: ad-hoc glyph bitmaps, path-drawn copies of Fluent symbols, deriving from the `Internals` `GenericIcon`/`GenericImage` bases (use `SymbolIcon`/`FluentIcon`)

[VOCABULARY_LAW]:

- `Symbol` (2789 members, `int`-backed) is the primary control surface; `Icon` is a distinct resizable-icon enum where `[NonResizable]` members have no resizable glyph
- `IconVariant`: `Regular`, `Filled`, `Color`, `Light`
- `IconSize`: `Resizable` (0), then `Size10`..`Size48` whose numeric value is the pixel size (`Size16` = 16)
- `GenericIcon`/`GenericImage`/`GenericIconConverter` live in `FluentIcons.Avalonia.Internals` and are base/conversion infrastructure, not the public selection surface
