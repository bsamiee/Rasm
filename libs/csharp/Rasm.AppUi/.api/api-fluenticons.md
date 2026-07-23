# [RASM_APPUI_API_FLUENTICONS]

`FluentIcons.Common` owns the platform-neutral icon vocabulary — the `Symbol` and `Icon` glyph enums with the `IconVariant` and `IconSize` axes — as enum-member identity any layer models without an Avalonia reference. `FluentIcons.Avalonia` renders that vocabulary through font-backed controls, image sources, the `Outline` attached accessor, and XAML markup extensions, binding a glyph by enum member rather than string key or bitmap path.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `FluentIcons.Common`
- package: `FluentIcons.Common` (MIT)
- assembly: `FluentIcons.Common`
- namespace: `FluentIcons.Common`
- target: `netstandard2.1`
- rail: icons

[PACKAGE_SURFACE]: `FluentIcons.Avalonia`
- package: `FluentIcons.Avalonia` (MIT)
- assembly: `FluentIcons.Avalonia`
- namespace: `FluentIcons.Avalonia`, `FluentIcons.Avalonia.Markup`; `FluentIcons.Avalonia.Internals` (bases, converters)
- target: `net10.0`
- depends: `Avalonia`, `FluentIcons.Common`, `FluentIcons.Resources.Avalonia` (glyph font)
- rail: icons

## [02]-[PUBLIC_TYPES]

[COMMON_TYPES]: platform-neutral vocabulary — `FluentIcons.Common`

`Symbol` is the full named-glyph set the Avalonia controls render; `Icon` is the smaller resizable set whose `[NonResizable]`-attributed members carry no resizable glyph, so `IconSize` selection binds only unattributed `Icon` members.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------ | :------------ | :---------------------------------------------------- |
|  [01]   | `Symbol`            | `int` enum    | named-glyph identity for controls                     |
|  [02]   | `Icon`              | `int` enum    | resizable-icon identity, `[NonResizable]` member tags |
|  [03]   | `IconVariant`       | `byte` enum   | rendering-variant axis                                |
|  [04]   | `IconSize`          | `byte` enum   | pixel-size axis                                       |
|  [05]   | `IconSizeValues`    | static class  | `IEnumerable<IconSize>` binding source                |
|  [06]   | `IconVariantValues` | static class  | `IEnumerable<IconVariant>` binding source             |

[ICON_VARIANT_MEMBERS]: `Regular` `Filled` `Color` `Light`
[ICON_SIZE_MEMBERS]: `Resizable` `Size10` `Size12` `Size16` `Size20` `Size24` `Size28` `Size32` `Size48` — `Resizable` is `0`, every `SizeNN` equals its pixel size.

[ICON_CONTROL_TYPES]: icon controls and image sources — `FluentIcons.Avalonia`

Each leaf implements `IValue<V>` over its identity enum and carries the identity property; the `GenericIcon`/`GenericImage` bases own the inherited `IconVariant` and `FontSize`.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                       |
| :-----: | :--------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `SymbolIcon`                 | class          | `Symbol` glyph control                             |
|  [02]   | `SymbolImage`                | class          | `Symbol` glyph image source                        |
|  [03]   | `FluentIcon`                 | class          | `Icon` resizable control                           |
|  [04]   | `FluentImage`                | class          | `Icon` resizable image source                      |
|  [05]   | `Outline`                    | abstract class | attached identity and brush accessor               |
|  [06]   | `GenericIcon`                | abstract class | control base (Internals); `IconVariant`/`FontSize` |
|  [07]   | `GenericImage`               | abstract class | image-source base (Internals)                      |
|  [08]   | `GenericIconConverter<V,T>`  | class          | `TypeConverter` string-to-control (Internals)      |
|  [09]   | `GenericImageConverter<V,T>` | class          | `TypeConverter` string-to-image (Internals)        |

[MARKUP_TYPES]: XAML markup extensions — `FluentIcons.Avalonia.Markup`

Each `sealed` extension carries nullable axis properties and a `(member)` constructor; `ProvideValue` returns the matching control instance.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]         |
| :-----: | :--------------------- | :------------ | :------------------- |
|  [01]   | `SymbolIconExtension`  | sealed class  | inline `SymbolIcon`  |
|  [02]   | `SymbolImageExtension` | sealed class  | inline `SymbolImage` |
|  [03]   | `FluentIconExtension`  | sealed class  | inline `FluentIcon`  |
|  [04]   | `FluentImageExtension` | sealed class  | inline `FluentImage` |

## [03]-[ENTRYPOINTS]

[COMMON_ENTRYPOINTS]: vocabulary selection — `FluentIcons.Common`

Enum members select identity and axis value; the `*Values.Enumerable` fields feed picker `ItemsSource`.

| [INDEX] | [SURFACE]                      | [SHAPE] | [CAPABILITY]                           |
| :-----: | :----------------------------- | :------ | :------------------------------------- |
|  [01]   | `Symbol.<member>`              | static  | named-glyph selection                  |
|  [02]   | `Icon.<member>`                | static  | resizable-icon selection               |
|  [03]   | `IconVariant.<member>`         | static  | variant selection                      |
|  [04]   | `IconSize.<member>`            | static  | pixel-size selection                   |
|  [05]   | `IconSizeValues.Enumerable`    | static  | `IEnumerable<IconSize>` for pickers    |
|  [06]   | `IconVariantValues.Enumerable` | static  | `IEnumerable<IconVariant>` for pickers |

[CONTROL_ENTRYPOINTS]: icon control properties — `FluentIcons.Avalonia`

`SymbolImage` mirrors `SymbolIcon.Symbol` and `FluentImage` mirrors `FluentIcon.Icon`; every control inherits `IconVariant` and `FontSize` from `GenericIcon`, and `Outline` attaches the same axes onto a control instance.

| [INDEX] | [SURFACE]                      | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :----------------------------- | :------- | :---------------------------------------- |
|  [01]   | `SymbolIcon.Symbol`            | property | `StyledProperty<Symbol>` glyph identity   |
|  [02]   | `FluentIcon.Icon`              | property | `StyledProperty<Icon>` resizable identity |
|  [03]   | `GenericIcon.IconVariant`      | property | `StyledProperty<IconVariant>` variant     |
|  [04]   | `GenericIcon.FontSize`         | property | `StyledProperty<double>` glyph scale      |
|  [05]   | `Outline.{Get,Set}Foreground`  | static   | `AttachedProperty<IBrush?>` brush         |
|  [06]   | `Outline.{Get,Set}Symbol`      | static   | `AttachedProperty<Symbol?>` symbol        |
|  [07]   | `Outline.{Get,Set}Icon`        | static   | `AttachedProperty<Icon?>` icon            |
|  [08]   | `Outline.{Get,Set}IconVariant` | static   | `AttachedProperty<IconVariant>` variant   |

[MARKUP_ENTRYPOINTS]: inline XAML construction — `SymbolIconExtension` (representative; the other three mirror it over their identity enum)

| [INDEX] | [SURFACE]                                      | [SHAPE]  | [CAPABILITY]     |
| :-----: | :--------------------------------------------- | :------- | :--------------- |
|  [01]   | `.ctor(Symbol)` / `.ctor()`                    | ctor     | inline construct |
|  [02]   | `Symbol` (`Symbol?`)                           | property | glyph select     |
|  [03]   | `IconVariant` (`IconVariant?`)                 | property | variant select   |
|  [04]   | `FontSize` (`double?`)                         | property | glyph scale      |
|  [05]   | `Foreground` (`Brush?`)                        | property | glyph brush      |
|  [06]   | `ProvideValue(IServiceProvider) -> SymbolIcon` | instance | markup resolve   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every icon binds one `Symbol`/`Icon` member and one `IconVariant`, and rendering resolves a font glyph by member — the leaf control carries identity, `GenericIcon` carries `IconVariant` and `FontSize`, and `Icon` sizing binds only members without the `[NonResizable]` tag.

[STACKING]:
- `api-avalonia-fluent.md`: `SymbolIcon.Foreground` and `Outline.ForegroundProperty` bind any Avalonia `IBrush`, so theme `DynamicResource` brushes drive glyph colour; `IconVariant.Color` selects the multi-colour glyph where the font ships one, else `Regular`/`Filled` honour the bound brush.
- AppHost, Compute, and Persistence model icon intent as a bare `Symbol`/`Icon` value end-to-end because `FluentIcons.Common` carries no Avalonia dependency; the AppUi view binds it to `SymbolIcon.Symbol`, and `IconVariantValues.Enumerable`/`IconSizeValues.Enumerable` feed picker `ItemsSource` without re-listing the enum.
- `GenericIconConverter<V,T>` parses a string attribute (`Symbol="Save"`) into the enum-keyed control, so XAML authoring stays string-literal while the runtime model stays the typed enum.

[LOCAL_ADMISSION]:
- Icon intent enters as a `Symbol`/`Icon` value in any layer; the AppUi view maps it to a `SymbolIcon`/`FluentIcon` with an `IconVariant` and binds colour through `Foreground` or `Outline`.

[RAIL_LAW]:
- Package: `FluentIcons.Common`
- Owns: the platform-neutral `Symbol`/`Icon`/`IconVariant`/`IconSize` vocabulary and the `*Values.Enumerable` binding sources
- Accept: reference from any layer needing icon identity without Avalonia; icon intent is a `Symbol`/`Icon` value
- Reject: string-keyed icon registries, parallel enum re-declarations, treating `Icon` as a `Symbol` alias

- Package: `FluentIcons.Avalonia`
- Owns: font-backed `Symbol`/`Icon` rendering through controls, image sources, the `Outline` attached accessor, and the markup extensions
- Accept: a `Symbol`/`Icon` member with an `IconVariant`, colour bound through `Foreground`/`Outline`
- Reject: ad-hoc glyph bitmaps, path-drawn copies of Fluent symbols, deriving from the `Internals` `GenericIcon`/`GenericImage` bases instead of `SymbolIcon`/`FluentIcon`
