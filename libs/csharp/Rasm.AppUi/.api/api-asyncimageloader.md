# [RASM_APPUI_API_ASYNCIMAGELOADER]

`AsyncImageLoader.Avalonia` supplies the `AdvancedImage` control, attached `ImageLoader` properties, and cached web image loaders for asynchronous bitmap sourcing.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AsyncImageLoader.Avalonia`
- package: `AsyncImageLoader.Avalonia`
- assembly: `AsyncImageLoader.Avalonia`
- namespace: `AsyncImageLoader`
- namespace: `AsyncImageLoader.Loaders`
- asset: runtime library
- rail: images

## [02]-[PUBLIC_TYPES]

[CONTROL_TYPES]: image control and attached surfaces
- rail: images

| [INDEX] | [SYMBOL]                     | [RAIL]              |
| :-----: | :--------------------------- | :------------------ |
|  [01]   | `AdvancedImage`              | async image control |
|  [02]   | `AdvancedImage.ImageWrapper` | drawable wrapper    |
|  [03]   | `ImageLoader`                | attached loader     |
|  [04]   | `ImageBrushLoader`           | brush loader        |

[LOADER_TYPES]: loader contracts and caches
- rail: images

| [INDEX] | [SYMBOL]                    | [RAIL]          |
| :-----: | :-------------------------- | :-------------- |
|  [01]   | `IAsyncImageLoader`         | loader contract |
|  [02]   | `IAdvancedAsyncImageLoader` | storage-aware   |
|  [03]   | `BaseWebImageLoader`        | HTTP loader     |
|  [04]   | `RamCachedWebImageLoader`   | memory cache    |
|  [05]   | `DiskCachedWebImageLoader`  | disk cache      |

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: AdvancedImage properties
- rail: images

| [INDEX] | [SURFACE]                         | [SURFACE_ROOT]  | [RAIL]          |
| :-----: | :-------------------------------- | :-------------- | :-------------- |
|  [01]   | `Source`                          | `AdvancedImage` | URL/URI input   |
|  [02]   | `Loader`                          | `AdvancedImage` | loader override |
|  [03]   | `FallbackImage`                   | `AdvancedImage` | failure bitmap  |
|  [04]   | `CurrentImage`                    | `AdvancedImage` | resolved image  |
|  [05]   | `IsLoading`                       | `AdvancedImage` | load state      |
|  [06]   | `Stretch` / `StretchDirection`    | `AdvancedImage` | layout policy   |
|  [07]   | `ShouldLoaderChangeTriggerUpdate` | `AdvancedImage` | reload policy   |

[ATTACHED_ENTRYPOINTS]: attached loading on plain Image controls
- rail: images
- surface: `ImageLoader`

| [INDEX] | [SURFACE]                 | [RAIL]          |
| :-----: | :------------------------ | :-------------- |
|  [01]   | `SetSource` / `GetSource` | attached source |
|  [02]   | `GetIsLoading`            | load state      |
|  [03]   | `AsyncImageLoader`        | global loader   |

[LOADER_ENTRYPOINTS]: loader operations
- rail: images

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]            | [RAIL]            |
| :-----: | :------------------ | :------------------------ | :---------------- |
|  [01]   | `ProvideImageAsync` | `BaseWebImageLoader`      | bitmap resolution |
|  [02]   | `ProvideImageAsync` | `RamCachedWebImageLoader` | cached resolution |
|  [03]   | `Dispose`           | `BaseWebImageLoader`      | HTTP teardown     |

## [04]-[IMPLEMENTATION_LAW]

[IMAGE_LAW]:
- Package: `AsyncImageLoader.Avalonia`
- Owns: asynchronous bitmap sourcing, load state, fallbacks, and cache policy
- Accept: remote or async image intent maps to `AdvancedImage` or attached `ImageLoader` properties
- Reject: blocking bitmap loads on the UI thread

[CACHE_LAW]:
- Package: `AsyncImageLoader.Avalonia`
- Owns: RAM and disk caching through the loader hierarchy
- Accept: cache behavior is selected by loader instance, not per-call flags
- Reject: bespoke image cache layers beside the loader hierarchy
