# [RASM_APPUI_API_AVALONIA_FONTS]

`Avalonia.Fonts.Inter` is a one-type asset shim: it embeds the six Inter TTF faces as `avares://` resources and exposes a single `AppBuilder.WithInterFont()` sugar that registers an `InterFontCollection` (a thin `EmbeddedFontCollection` subclass) under the `fonts:Inter` family key. The entire font-collection machinery it composes against — `EmbeddedFontCollection`, `FontCollectionBase`, `IFontCollection`, `FontManager`, `ConfigureFonts` — lives in `Avalonia.Base`, not in this assembly; this catalog marks that seam explicitly so the Theme typography owner stacks onto the Avalonia font registry rather than re-deriving an asset loader.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia.Fonts.Inter`

- package: `Avalonia.Fonts.Inter`
- license: `MIT` (package); the embedded Inter faces ship under the SIL Open Font License 1.1
- assembly: `Avalonia.Fonts.Inter` (AnyCPU IL, managed-only)
- build-floor: `net10.0` (consumer-bound asset; `net8.0` fallback present, not bound)
- namespace: `Avalonia.Fonts.Inter` (`InterFontCollection`)
- namespace: `Avalonia` (`AppBuilderExtension.WithInterFont`)
- asset: runtime library plus `!AvaloniaResources` container holding `avares://Avalonia.Fonts.Inter/Assets/*.ttf`
- rail: typography

## [02]-[PUBLIC_TYPES]

[FONT_TYPES]: the only two types this assembly defines

- rail: typography

| [INDEX] | [SYMBOL]              | [SIGNATURE]                                                 | [RAIL]             |
| :-----: | :-------------------- | :---------------------------------------------------------- | :----------------- |
|  [01]   | `InterFontCollection` | `sealed class InterFontCollection : EmbeddedFontCollection` | Inter family owner |
|  [02]   | `AppBuilderExtension` | `static class` (namespace `Avalonia`)                       | builder admission  |

`InterFontCollection()` is parameterless and hard-codes its two URIs to the base: `base(new Uri("fonts:Inter"), new Uri("avares://Avalonia.Fonts.Inter/Assets"))`. The first is the family key (`FontFamily = "fonts:Inter#Inter"` once loaded); the second is the asset root the base scans.

[BOUNDARY_TYPES]: composed from `Avalonia.Base`, NOT defined here — the typography owner binds these directly

- rail: typography

| [INDEX] | [SYMBOL]                 | [SIGNATURE]                                                              | [BOUNDARY]               |
| :-----: | :----------------------- | :----------------------------------------------------------------------- | :----------------------- |
|  [01]   | `EmbeddedFontCollection` | `class EmbeddedFontCollection(Uri key, Uri source) : FontCollectionBase` | `avares:` scan base type |
|  [02]   | `IFontCollection`        | `interface IFontCollection : IReadOnlyList<FontFamily>, IDisposable`     | collection contract      |
|  [03]   | `FontManager`            | `sealed class FontManager` (`FontManager.Current`)                       | process font registry    |
|  [04]   | `FontFamily`             | `class FontFamily` (constructed from `fonts:Inter#Inter`)                | resolved family handle   |

[FONT_ASSETS]: the six embedded faces under `avares://Avalonia.Fonts.Inter/Assets/`

- rail: typography

| [INDEX] | [ASSET]              | [WEIGHT]                    |
| :-----: | :------------------- | :-------------------------- |
|  [01]   | `Inter-Thin.ttf`     | `FontWeight.Thin` (100)     |
|  [02]   | `Inter-Light.ttf`    | `FontWeight.Light` (300)    |
|  [03]   | `Inter-Regular.ttf`  | `FontWeight.Normal` (400)   |
|  [04]   | `Inter-Medium.ttf`   | `FontWeight.Medium` (500)   |
|  [05]   | `Inter-SemiBold.ttf` | `FontWeight.SemiBold` (600) |
|  [06]   | `Inter-Bold.ttf`     | `FontWeight.Bold` (700)     |

The collection synthesizes intermediate weights/italics from these six faces through Avalonia's font-matching; only the listed files exist in the assembly.

## [03]-[ENTRYPOINTS]

[FONT_ENTRYPOINTS]: the single owned operation plus the base-class operations it forwards to

- rail: typography

| [INDEX] | [SURFACE]           | [SIGNATURE]                                                       | [CAPABILITY]       |
| :-----: | :------------------ | :---------------------------------------------------------------- | :----------------- |
|  [01]   | `WithInterFont`     | `static AppBuilder WithInterFont(this AppBuilder)`                | Inter admission    |
|  [02]   | `ConfigureFonts`    | `AppBuilder ConfigureFonts(this AppBuilder, Action<FontManager>)` | font configuration |
|  [03]   | `AddFontCollection` | `void AddFontCollection(IFontCollection)`                         | collection load    |
|  [04]   | `.ctor`             | `InterFontCollection()`                                           | family owner       |

[SURFACE_OWNERS]: `AppBuilderExtension` owns `WithInterFont`, Avalonia's boundary `AppBuilderExtensions` owns `ConfigureFonts`, `FontManager` owns `AddFontCollection`, and `InterFontCollection` owns its constructor.

`WithInterFont()` is exactly `appBuilder.ConfigureFonts(fm => fm.AddFontCollection(new InterFontCollection()))` — no more. A typography owner that needs additional families (icon fonts, monospace for the code editor) calls `ConfigureFonts` directly and `AddFontCollection`s several collections in one builder pass rather than chaining package-specific sugar.

## [04]-[INTEGRATION_LAW]

[TYPOGRAPHY_RAIL_LAW]:

- Stack: `WithInterFont` is the default-family leg of the app-builder font pass; the Theme typography owner composes it alongside `ConfigureFonts`-registered icon/mono collections so every AppUi modality resolves type through one registry, then sets `Application.Resources` / control `FontFamily` to `fonts:Inter#Inter`.
- Accept: typography roles bind to the embedded `fonts:Inter` family (or a sibling embedded collection) so render output is byte-identical across the macOS desktop and the headless raster backend — no host font probing.
- Reject: system-font assumptions, per-host fallback families as public package behavior, or a hand-rolled `EmbeddedFontCollection` subclass when `InterFontCollection` already owns the Inter asset root.

[DETERMINISM_LAW]:

- The `avares:` resource family makes Inter availability a compile-time fact, not a deployment probe; this is the property the headless capture/evidence rail relies on for stable golden-image text metrics. `FontManager.Current.DefaultFontFamily` stays Avalonia's built-in default unless the theme explicitly elevates `fonts:Inter`, so admitting this package does not silently re-key the default — the elevation is the typography owner's deliberate act.
