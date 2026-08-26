# [RASM_APPUI_API_AVALONIA_FONTS]

`Avalonia.Fonts.Inter` embeds the six Inter TTF faces as `avares://` resources and mints one `AppBuilder.WithInterFont()` that registers `InterFontCollection` — a sealed `EmbeddedFontCollection` — under the `fonts:Inter` family key. Every font-collection type it composes against lives in `Avalonia.Base`; this catalog marks that boundary so the Theme typography owner stacks onto the Avalonia font registry rather than re-deriving an asset loader.

## [01]-[PUBLIC_TYPES]

[FONT_TYPES]: the two types this assembly defines

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :-------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `InterFontCollection` | class         | sealed `EmbeddedFontCollection` owning the `fonts:Inter` family |
|  [02]   | `AppBuilderExtension` | class         | static `WithInterFont` builder sugar (namespace `Avalonia`)     |

[BOUNDARY_TYPES]: composed from `Avalonia.Base`, not defined here — the typography owner binds them directly

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :----------------------- | :------------ | :------------------------------------------------------------------------ |
|  [01]   | `EmbeddedFontCollection` | class         | `avares:`-scanning base of `InterFontCollection` (`Avalonia.Media.Fonts`) |
|  [02]   | `IFontCollection`        | interface     | font-family collection contract `AddFontCollection` binds                 |
|  [03]   | `FontManager`            | class         | process font registry (`FontManager.Current`)                             |
|  [04]   | `FontFamily`             | class         | resolved family handle from `fonts:Inter#Inter`                           |

[FONT_ASSETS]: the six embedded faces under `avares://Avalonia.Fonts.Inter/Assets/`, from which Avalonia's font-matching synthesizes intermediate weights and italics

| [INDEX] | [ASSET]              | [WEIGHT]                    |
| :-----: | :------------------- | :-------------------------- |
|  [01]   | `Inter-Thin.ttf`     | `FontWeight.Thin` (100)     |
|  [02]   | `Inter-Light.ttf`    | `FontWeight.Light` (300)    |
|  [03]   | `Inter-Regular.ttf`  | `FontWeight.Normal` (400)   |
|  [04]   | `Inter-Medium.ttf`   | `FontWeight.Medium` (500)   |
|  [05]   | `Inter-SemiBold.ttf` | `FontWeight.SemiBold` (600) |
|  [06]   | `Inter-Bold.ttf`     | `FontWeight.Bold` (700)     |

## [02]-[ENTRYPOINTS]

[FONT_ENTRYPOINTS]: `WithInterFont` and the constructor are owned; `ConfigureFonts` and `AddFontCollection` forward to `Avalonia.Base`

| [INDEX] | [SURFACE]                                         | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :------------------------------------------------ | :------- | :---------------------------- |
|  [01]   | `WithInterFont(AppBuilder) -> AppBuilder`         | static   | Inter family admission        |
|  [02]   | `ConfigureFonts(AppBuilder, Action<FontManager>)` | static   | font configuration pass       |
|  [03]   | `AddFontCollection(IFontCollection)`              | instance | collection load onto registry |
|  [04]   | `InterFontCollection()`                           | ctor     | family owner                  |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `InterFontCollection()` hard-codes `base(new Uri("fonts:Inter"), new Uri("avares://Avalonia.Fonts.Inter/Assets"))`, so the family resolves as `fonts:Inter#Inter` and the base scans the asset root.
- `avares:` resources make Inter availability a compile-time fact, never a deployment probe.
- `FontManager.Current.DefaultFontFamily` stays Avalonia's built-in default until the theme elevates `fonts:Inter`, so admitting this package never silently re-keys the default.

[STACKING]:
- `api-headless`(`.api/api-headless.md`): `WithInterFont()` chains into `BuildAvaloniaApp().UseHeadless(...).UseSkia().WithInterFont()`, binding the embedded `fonts:Inter` family so headless text metrics stay host-independent across raster backends.
- within-lib: the Theme typography owner composes `WithInterFont` alongside `ConfigureFonts`-registered icon and monospace collections in one builder pass, then keys `Application.Resources` and control `FontFamily` to `fonts:Inter#Inter`.

[LOCAL_ADMISSION]:
- Typography roles bind the embedded `fonts:Inter` family or a sibling embedded collection, so render output is byte-identical across the macOS desktop and headless raster backends.
- A second family (icon, monospace) registers through `ConfigureFonts` and `AddFontCollection` in the same builder pass, never package-specific chaining sugar.
- The six shipped faces are STATIC, so optical sizing, true italics, and interpolated weights exist only through an owned variable asset registered as a second `EmbeddedFontCollection(Uri key, Uri source)` beside this one; the shipped ladder plus a declared synthetic embolden or skew is the stated fallback where the variable asset is absent, never a silent per-host difference.
- `FontManagerOptions.DefaultFamilyName` pins the family every unstyled surface resolves and `FontFallbacks` carries the ranked host tail, so a resolved style row names one family and the fallback chain lives on the registry rather than inside every style string.
