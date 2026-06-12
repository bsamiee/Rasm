# [RASM_APPUI_API_FLUENTICONS]

`FluentIcons.Avalonia` supplies `SymbolIcon`/`SymbolImage` controls, font icon variants, and XAML markup extensions over the 2785-member `Symbol` glyph vocabulary.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentIcons.Avalonia`
- package: `FluentIcons.Avalonia`
- assembly: `FluentIcons.Avalonia`
- namespace: `FluentIcons.Avalonia`
- namespace: `FluentIcons.Avalonia.Markup`
- asset: runtime library
- asset: `Symbol`/`IconVariant` vocabulary from `FluentIcons.Common`
- rail: icons

## [2]-[PUBLIC_TYPES]

[ICON_TYPES]: icon controls and image sources
- rail: icons

| [INDEX] | [SYMBOL]      | [RAIL]            |
| :-----: | :------------ | :---------------- |
|   [1]   | `SymbolIcon`  | symbol control    |
|   [2]   | `SymbolImage` | symbol image      |
|   [3]   | `FluentIcon`  | glyph control     |
|   [4]   | `FluentImage` | glyph image       |
|   [5]   | `Outline`     | outline rendering |
|   [6]   | `GenericIcon` | icon base         |

[MARKUP_TYPES]: XAML markup extensions
- rail: icons

| [INDEX] | [SYMBOL]               | [RAIL]           |
| :-----: | :--------------------- | :--------------- |
|   [1]   | `SymbolIconExtension`  | inline icon      |
|   [2]   | `SymbolImageExtension` | inline image     |
|   [3]   | `FluentIconExtension`  | inline glyph     |
|   [4]   | `FluentImageExtension` | inline glyph img |

[VOCABULARY_TYPES]: glyph vocabulary
- rail: icons

| [INDEX] | [SYMBOL]      | [RAIL]                    |
| :-----: | :------------ | :------------------------ |
|   [1]   | `Symbol`      | 2785-member glyph enum    |
|   [2]   | `IconVariant` | variant axis (filled etc) |

## [3]-[ENTRYPOINTS]

[ICON_ENTRYPOINTS]: icon control properties
- rail: icons

| [INDEX] | [SURFACE]     | [SURFACE_ROOT] | [RAIL]         |
| :-----: | :------------ | :------------- | :------------- |
|   [1]   | `Symbol`      | `SymbolIcon`   | glyph select   |
|   [2]   | `Symbol`      | `SymbolImage`  | glyph select   |
|   [3]   | `IconVariant` | `GenericIcon`  | variant select |
|   [4]   | `FontSize`    | `GenericIcon`  | glyph scale    |
|   [5]   | `Converter`   | `SymbolIcon`   | string convert |

[MARKUP_ENTRYPOINTS]: inline XAML construction
- rail: icons

| [INDEX] | [SURFACE]      | [SURFACE_ROOT]        | [RAIL]         |
| :-----: | :------------- | :-------------------- | :------------- |
|   [1]   | `ProvideValue` | `SymbolIconExtension` | icon construct |
|   [2]   | `Symbol`       | `SymbolIconExtension` | glyph select   |
|   [3]   | `IconVariant`  | `SymbolIconExtension` | variant select |
|   [4]   | `FontSize`     | `SymbolIconExtension` | glyph scale    |
|   [5]   | `Foreground`   | `SymbolIconExtension` | glyph brush    |

## [4]-[IMPLEMENTATION_LAW]

[ICON_LAW]:
- Package: `FluentIcons.Avalonia`
- Owns: Fluent symbol rendering through font-backed icon controls and markup extensions
- Accept: icon intent maps to a `Symbol` member with an `IconVariant`
- Reject: ad-hoc glyph bitmaps or path-drawn copies of Fluent symbols

[VOCABULARY_LAW]:
- Package: `FluentIcons.Avalonia`
- Owns: the bounded `Symbol` vocabulary as the only icon naming surface
- Accept: icon identity is a `Symbol` enum member end-to-end
- Reject: string-keyed icon registries duplicating the enum
