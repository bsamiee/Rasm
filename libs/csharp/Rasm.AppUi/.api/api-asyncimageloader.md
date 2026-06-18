# [RASM_APPUI_API_ASYNCIMAGELOADER]

`AsyncImageLoader.Avalonia` supplies the `AdvancedImage` control, attached `ImageLoader` properties, and cached web image loaders for asynchronous bitmap sourcing.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AsyncImageLoader.Avalonia`
- package: `AsyncImageLoader.Avalonia`
- assembly: `AsyncImageLoader.Avalonia`
- namespace: `AsyncImageLoader`
- namespace: `AsyncImageLoader.Loaders`
- asset: runtime library
- rail: images

## [2]-[PUBLIC_TYPES]

[CONTROL_TYPES]: image control and attached surfaces
- rail: images

| [INDEX] | [SYMBOL]                     | [RAIL]              |
| :-----: | :--------------------------- | :------------------ |
|   [1]   | `AdvancedImage`              | async image control |
|   [2]   | `AdvancedImage.ImageWrapper` | drawable wrapper    |
|   [3]   | `ImageLoader`                | attached loader     |
|   [4]   | `ImageBrushLoader`           | brush loader        |

[LOADER_TYPES]: loader contracts and caches
- rail: images

| [INDEX] | [SYMBOL]                    | [RAIL]          |
| :-----: | :-------------------------- | :-------------- |
|   [1]   | `IAsyncImageLoader`         | loader contract |
|   [2]   | `IAdvancedAsyncImageLoader` | storage-aware   |
|   [3]   | `BaseWebImageLoader`        | HTTP loader     |
|   [4]   | `RamCachedWebImageLoader`   | memory cache    |
|   [5]   | `DiskCachedWebImageLoader`  | disk cache      |

## [3]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: AdvancedImage properties
- rail: images

| [INDEX] | [SURFACE]                         | [SURFACE_ROOT]  | [RAIL]          |
| :-----: | :-------------------------------- | :-------------- | :-------------- |
|   [1]   | `Source`                          | `AdvancedImage` | URL/URI input   |
|   [2]   | `Loader`                          | `AdvancedImage` | loader override |
|   [3]   | `FallbackImage`                   | `AdvancedImage` | failure bitmap  |
|   [4]   | `CurrentImage`                    | `AdvancedImage` | resolved image  |
|   [5]   | `IsLoading`                       | `AdvancedImage` | load state      |
|   [6]   | `Stretch` / `StretchDirection`    | `AdvancedImage` | layout policy   |
|   [7]   | `ShouldLoaderChangeTriggerUpdate` | `AdvancedImage` | reload policy   |

[ATTACHED_ENTRYPOINTS]: attached loading on plain Image controls
- rail: images
- surface: `ImageLoader`

| [INDEX] | [SURFACE]                 | [RAIL]          |
| :-----: | :------------------------ | :-------------- |
|   [1]   | `SetSource` / `GetSource` | attached source |
|   [2]   | `GetIsLoading`            | load state      |
|   [3]   | `AsyncImageLoader`        | global loader   |

[LOADER_ENTRYPOINTS]: loader operations
- rail: images

| [INDEX] | [SURFACE]           | [SURFACE_ROOT]            | [RAIL]            |
| :-----: | :------------------ | :------------------------ | :---------------- |
|   [1]   | `ProvideImageAsync` | `BaseWebImageLoader`      | bitmap resolution |
|   [2]   | `ProvideImageAsync` | `RamCachedWebImageLoader` | cached resolution |
|   [3]   | `Dispose`           | `BaseWebImageLoader`      | HTTP teardown     |

## [4]-[IMPLEMENTATION_LAW]

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
