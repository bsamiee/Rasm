# [RASM_APPUI_API_FLUENTICONS]

`FluentIcons.Avalonia` supplies `SymbolIcon`/`SymbolImage` controls, font icon variants, and XAML markup extensions over the Fluent icon vocabulary. `FluentIcons.Common` supplies the shared vocabulary types — `Symbol`, `Icon`, `IconVariant`, `IconSize`, and their enumeration helpers — consumed by both the Avalonia layer and any platform-neutral code.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentIcons.Common`
- package: `FluentIcons.Common`
- assembly: `FluentIcons.Common`
- namespace: `FluentIcons.Common`
- asset: runtime library (netstandard2.0 / netstandard2.1)
- rail: icons

[PACKAGE_SURFACE]: `FluentIcons.Avalonia`
- package: `FluentIcons.Avalonia`
- assembly: `FluentIcons.Avalonia`
- namespace: `FluentIcons.Avalonia`
- namespace: `FluentIcons.Avalonia.Markup`
- asset: runtime library
- rail: icons

## [02]-[PUBLIC_TYPES]

[COMMON_TYPES]: shared vocabulary — `FluentIcons.Common`
- rail: icons

| [INDEX] | [SYMBOL]            | [KIND]                          | [RAIL]                                   |
| :-----: | :------------------ | :------------------------------ | :--------------------------------------- |
|  [01]   | `Symbol`            | int-backed enum, 2785 members   | icon identity by name                    |
|  [02]   | `Icon`              | int-backed enum (alias surface) | resizable icon identity                  |
|  [03]   | `IconVariant`       | byte enum (4 cases)             | rendering variant axis                   |
|  [04]   | `IconSize`          | byte enum (9 sizes)             | discrete size axis                       |
|  [05]   | `IconSizeValues`    | static enumeration helper       | all `IconSize` cases as `IEnumerable`    |
|  [06]   | `IconVariantValues` | static enumeration helper       | all `IconVariant` cases as `IEnumerable` |

[ICON_TYPES]: icon controls and image sources — `FluentIcons.Avalonia`
- rail: icons

| [INDEX] | [SYMBOL]      | [RAIL]            |
| :-----: | :------------ | :---------------- |
|  [01]   | `SymbolIcon`  | symbol control    |
|  [02]   | `SymbolImage` | symbol image      |
|  [03]   | `FluentIcon`  | glyph control     |
|  [04]   | `FluentImage` | glyph image       |
|  [05]   | `Outline`     | outline rendering |
|  [06]   | `GenericIcon` | icon base         |

[MARKUP_TYPES]: XAML markup extensions — `FluentIcons.Avalonia`
- rail: icons

| [INDEX] | [SYMBOL]               | [RAIL]           |
| :-----: | :--------------------- | :--------------- |
|  [01]   | `SymbolIconExtension`  | inline icon      |
|  [02]   | `SymbolImageExtension` | inline image     |
|  [03]   | `FluentIconExtension`  | inline glyph     |
|  [04]   | `FluentImageExtension` | inline glyph img |

## [03]-[ENTRYPOINTS]

[COMMON_ENTRYPOINTS]: `FluentIcons.Common` vocabulary surfaces
- rail: icons

| [INDEX] | [SURFACE]                      | [SURFACE_ROOT]      | [RAIL]                                     |
| :-----: | :----------------------------- | :------------------ | :----------------------------------------- |
|  [01]   | `Symbol.<member>`              | `Symbol`            | icon identity selection                    |
|  [02]   | `Icon.<member>`                | `Icon`              | resizable icon identity                    |
|  [03]   | `IconVariant.Regular`          | `IconVariant`       | regular variant                            |
|  [04]   | `IconVariant.Filled`           | `IconVariant`       | filled variant                             |
|  [05]   | `IconVariant.Color`            | `IconVariant`       | color variant                              |
|  [06]   | `IconVariant.Light`            | `IconVariant`       | light variant                              |
|  [07]   | `IconSize.Resizable`..`Size48` | `IconSize`          | discrete size cases                        |
|  [08]   | `IconSizeValues.Enumerable`    | `IconSizeValues`    | all sizes as `IEnumerable<IconSize>`       |
|  [09]   | `IconVariantValues.Enumerable` | `IconVariantValues` | all variants as `IEnumerable<IconVariant>` |

[ICON_ENTRYPOINTS]: icon control properties — `FluentIcons.Avalonia`
- rail: icons

| [INDEX] | [SURFACE]     | [SURFACE_ROOT] | [RAIL]         |
| :-----: | :------------ | :------------- | :------------- |
|  [01]   | `Symbol`      | `SymbolIcon`   | glyph select   |
|  [02]   | `Symbol`      | `SymbolImage`  | glyph select   |
|  [03]   | `IconVariant` | `GenericIcon`  | variant select |
|  [04]   | `FontSize`    | `GenericIcon`  | glyph scale    |
|  [05]   | `Converter`   | `SymbolIcon`   | string convert |

[MARKUP_ENTRYPOINTS]: inline XAML construction
- rail: icons
- surface-root: `SymbolIconExtension`

| [INDEX] | [SURFACE]      | [RAIL]         |
| :-----: | :------------- | :------------- |
|  [01]   | `ProvideValue` | icon construct |
|  [02]   | `Symbol`       | glyph select   |
|  [03]   | `IconVariant`  | variant select |
|  [04]   | `FontSize`     | glyph scale    |
|  [05]   | `Foreground`   | glyph brush    |

## [04]-[IMPLEMENTATION_LAW]

[COMMON_LAW]:
- Package: `FluentIcons.Common`
- Owns: `Symbol`, `Icon`, `IconVariant`, `IconSize`, and their enumeration helpers as the shared, platform-neutral vocabulary
- Accept: reference `FluentIcons.Common` from platform-neutral layers that need icon identity without Avalonia
- Reject: re-declaring icon vocabularies, string-keyed registries, or parallel enum definitions

[ICON_LAW]:
- Package: `FluentIcons.Avalonia`
- Owns: Fluent symbol rendering through font-backed icon controls and markup extensions
- Accept: icon intent maps to a `Symbol` or `Icon` member with an `IconVariant`
- Reject: ad-hoc glyph bitmaps or path-drawn copies of Fluent symbols

[VOCABULARY_LAW]:
- `Symbol` (2785 members, `int`-backed) and `Icon` are parallel naming surfaces for the same glyph set; `Symbol` is the primary selection surface in Avalonia controls
- `IconVariant` cases: `Regular`, `Filled`, `Color`, `Light`
- `IconSize` cases: `Resizable` (0), `Size10`, `Size12`, `Size16`, `Size20`, `Size24`, `Size28`, `Size32`, `Size48`
- Accept: icon identity is a `Symbol` or `Icon` enum member end-to-end
- Reject: string-keyed icon registries duplicating the enum
