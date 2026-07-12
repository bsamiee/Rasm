# [RASM_APPUI_API_ASYNCIMAGELOADER]

`AsyncImageLoader.Avalonia` (MIT, `net8.0` lib bound under the net10 consumer) supplies the `AdvancedImage` content control, two attached-loading statics (`ImageLoader` on `Image`, `ImageBrushLoader` on `ImageBrush`), the `IAsyncImageLoader`/`IAdvancedAsyncImageLoader` loader contracts, and a three-level web-image-loader hierarchy (`BaseWebImageLoader` → `RamCachedWebImageLoader` → `DiskCachedWebImageLoader`) for asynchronous, cached bitmap sourcing off the UI thread. Cache policy belongs to the chosen loader instance; the loader takes an injectable `HttpClient`, so it composes with the AppHost networking stack rather than owning a private one.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AsyncImageLoader.Avalonia`
- package / license: `AsyncImageLoader.Avalonia` / MIT
- assembly: `AsyncImageLoader.Avalonia`
- asset: `net8.0` (only lib; consumer-bound under net10)
- namespace: `AsyncImageLoader`, `AsyncImageLoader.Loaders`
- rail: images

## [02]-[PUBLIC_TYPES]

[CONTROL_TYPES]: image control and attached surfaces
- rail: images

`AdvancedImage` owns self-loading image control, and its internally constructed `ImageWrapper` wraps the resolved `IImage`. The static loaders own attached loading and their global loader instances for `Image` and `ImageBrush`.

| [INDEX] | [SYMBOL]                     | [SHAPE]            | [RAIL]                  |
| :-----: | :--------------------------- | :----------------- | :---------------------- |
|  [01]   | `AdvancedImage`              | `: ContentControl` | async image control     |
|  [02]   | `AdvancedImage.ImageWrapper` | `sealed : IImage`  | drawable wrapper        |
|  [03]   | `ImageLoader`                | `static`           | `Image` attachment      |
|  [04]   | `ImageBrushLoader`           | `static`           | `ImageBrush` attachment |

[LOADER_TYPES]: loader contracts and cache hierarchy
- rail: images

The two interfaces extend `IDisposable`. `IAsyncImageLoader` declares `Task<Bitmap?> ProvideImageAsync(string url)`, and `IAdvancedAsyncImageLoader` declares `Task<Bitmap?> ProvideImageAsync(string url, IStorageProvider? = null)` for `avares:` and storage-scoped URIs. `BaseWebImageLoader` implements both interfaces as an HTTP loader with an injectable `HttpClient`. The concrete hierarchy is linear: `RamCachedWebImageLoader` adds an in-memory bitmap cache, and `DiskCachedWebImageLoader` inherits that cache while adding an on-disk cache folder.

| [INDEX] | [SYMBOL]                    | [ROLE]                 |
| :-----: | :-------------------------- | :--------------------- |
|  [01]   | `IAsyncImageLoader`         | loader contract        |
|  [02]   | `IAdvancedAsyncImageLoader` | storage-aware contract |
|  [03]   | `BaseWebImageLoader`        | HTTP loader            |
|  [04]   | `RamCachedWebImageLoader`   | memory cache           |
|  [05]   | `DiskCachedWebImageLoader`  | disk cache             |

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: `AdvancedImage` construction + properties
- rail: images

`AdvancedImage` has no parameterless constructor. `.ctor(Uri? baseUri)` constructs the control directly, and `baseUri` roots the relative `avares:` and `Source` references it resolves; `new AdvancedImage(new Uri(source))` is the direct form. `.ctor(IServiceProvider)` is the XAML/DI form and pulls `IUriContext.BaseUri`. `Source` accepts URL or URI input. `Loader`, `Source`, `FallbackImage`, `Stretch`, and `StretchDirection` are `StyledProperty` surfaces. `IsLoading`, `CurrentImage`, and `ShouldLoaderChangeTriggerUpdate` are read-projecting `DirectProperty` surfaces that bind one-way into view state.

Every surface in the table belongs to `AdvancedImage`.

| [INDEX] | [SURFACE]                         | [TYPE]               | [RAIL]               |
| :-----: | :-------------------------------- | :------------------- | :------------------- |
|  [01]   | `Source`                          | `string?`            | source input         |
|  [02]   | `Loader`                          | `IAsyncImageLoader?` | per-control override |
|  [03]   | `FallbackImage`                   | `Bitmap?`            | failure bitmap       |
|  [04]   | `CurrentImage`                    | `IImage?`            | resolved image       |
|  [05]   | `IsLoading`                       | `bool`               | load state           |
|  [06]   | `Stretch`                         | `Stretch`            | layout policy        |
|  [07]   | `StretchDirection`                | `StretchDirection`   | layout policy        |
|  [08]   | `ShouldLoaderChangeTriggerUpdate` | `bool`               | loader-change reload |
|  [09]   | `.ctor(Uri? baseUri)`             | construction         | direct construction  |
|  [10]   | `.ctor(IServiceProvider)`         | construction         | service construction |

[ATTACHED_ENTRYPOINTS]: attached loading on plain `Image` / `ImageBrush`
- rail: images

`ImageLoader` targets `Image`; `ImageBrushLoader` targets `ImageBrush`. Both expose a settable static `AsyncImageLoader` of type `IAsyncImageLoader` whose default is a `RamCachedWebImageLoader`. The `Source` change handler dispatches to `IAdvancedAsyncImageLoader` with the host `TopLevel.StorageProvider` when the active loader implements it. `ImageBrushLoader.SourceProperty`, `FallbackImageProperty`, and `IsLoadingProperty` are attached properties.

| [INDEX] | [SURFACE]                   | [SURFACE_ROOT]     | [RAIL]              |
| :-----: | :-------------------------- | :----------------- | :------------------ |
|  [01]   | `SetSource(Image, string?)` | `ImageLoader`      | attached URL write  |
|  [02]   | `GetSource(Image)`          | `ImageLoader`      | attached URL read   |
|  [03]   | `GetIsLoading(Image)`       | `ImageLoader`      | attached load state |
|  [04]   | `AsyncImageLoader`          | `ImageLoader`      | global loader       |
|  [05]   | `AsyncImageLoaderLogArea`   | `ImageLoader`      | `const string` key  |
|  [06]   | `SourceProperty`            | `ImageBrushLoader` | attached source     |
|  [07]   | `FallbackImageProperty`     | `ImageBrushLoader` | attached fallback   |
|  [08]   | `IsLoadingProperty`         | `ImageBrushLoader` | attached load state |
|  [09]   | `AsyncImageLoader`          | `ImageBrushLoader` | global loader       |

[LOADER_ENTRYPOINTS]: loader construction and resolution
- rail: images

`BaseWebImageLoader` and `RamCachedWebImageLoader` expose both loader constructors. `DiskCachedWebImageLoader` exposes the two cache-folder constructors. Every `ProvideImageAsync` overload returns `Task<Bitmap?>`. `HttpClient` is protected, and the three load extension points are protected virtual members of `BaseWebImageLoader`. `Dispose()` tears down HTTP and cache resources.

| [INDEX] | [SURFACE]                                      | [SURFACE_ROOT]                    | [RAIL]                      |
| :-----: | :--------------------------------------------- | :-------------------------------- | :-------------------------- |
|  [01]   | `.ctor()`                                      | `BaseWebImageLoader`              | default construction        |
|  [02]   | `.ctor(HttpClient, bool disposeHttpClient)`    | `BaseWebImageLoader`              | injected construction       |
|  [03]   | `.ctor()`                                      | `RamCachedWebImageLoader`         | default construction        |
|  [04]   | `.ctor(HttpClient, bool disposeHttpClient)`    | `RamCachedWebImageLoader`         | injected construction       |
|  [05]   | `.ctor(string cacheFolder = "Cache/Images/")`  | `DiskCachedWebImageLoader`        | cache construction          |
|  [06]   | `.ctor(HttpClient, bool, string cacheFolder)`  | `DiskCachedWebImageLoader`        | injected cache construction |
|  [07]   | `ProvideImageAsync(string)`                    | `IAsyncImageLoader` impls         | bitmap resolution           |
|  [08]   | `ProvideImageAsync(string, IStorageProvider?)` | `IAdvancedAsyncImageLoader` impls | storage resolution          |
|  [09]   | `HttpClient`                                   | `BaseWebImageLoader`              | protected client            |
|  [10]   | `LoadAsync`                                    | `BaseWebImageLoader`              | protected extension         |
|  [11]   | `LoadFromLocalAsync`                           | `BaseWebImageLoader`              | protected extension         |
|  [12]   | `LoadFromGlobalCache`                          | `BaseWebImageLoader`              | protected extension         |
|  [13]   | `Dispose()`                                    | `BaseWebImageLoader`              | loader teardown             |

## [04]-[INTEGRATION]

[STACK_HTTPCLIENT]:
- The `BaseWebImageLoader(HttpClient, bool disposeHttpClient)` ctor is the integration
  seam: inject the AppHost-owned `HttpClient` (shared handler, auth headers, proxy,
  resilience policy) and pass `disposeHttpClient: false` so the loader rides the shared
  connection pool instead of opening a private `HttpClient`. The parameterless ctor owns
  and disposes its own client — use it only for isolated/throwaway loaders.

[STACK_LOADER_SELECTION]:
- Cache policy is loader-typed, never per-call: `RamCachedWebImageLoader` for
  session-scoped thumbnails, `DiskCachedWebImageLoader(cacheFolder)` for assets that must
  survive restart (root the cache folder under the AppHost cache path, not the default
  relative `"Cache/Images/"`). Set the chosen loader once on `ImageLoader.AsyncImageLoader`
  for the global default, or per-control on `AdvancedImage.Loader` to override.

[STACK_STORAGE_PROVIDER]:
- A loader implementing `IAdvancedAsyncImageLoader` resolves storage/`avares:` URIs via
  the control's `TopLevel.StorageProvider` (`api-avalonia.md`) — the same `IStorageProvider`
  the behavior-rail pickers (`api-behaviors.md`) use. This lets one URL string address both
  remote HTTP images and app/storage-scoped resources through a single `Source` binding.

[STACK_FALLBACK_STATE]:
- `AdvancedImage.IsLoading`/`CurrentImage` are `DirectProperty` read projections; bind a
  ReactiveUI view-model loading flag (`api-reactiveui.md`) to `IsLoading` for spinners, and
  set `FallbackImage` to a themed placeholder `Bitmap` so failed loads degrade visibly
  without a code-behind error path.

## [05]-[IMPLEMENTATION_LAW]

[IMAGE_LAW]:
- Package: `AsyncImageLoader.Avalonia` (MIT, net8.0)
- Owns: asynchronous off-UI-thread bitmap sourcing, load state, fallbacks, and cache policy
- Accept: remote/async image intent maps to `AdvancedImage` or attached `ImageLoader`/`ImageBrushLoader`; loaders take an injected `HttpClient`
- Reject: blocking bitmap loads on the UI thread; a private `HttpClient` when the AppHost networking stack exists

[CACHE_LAW]:
- Package: `AsyncImageLoader.Avalonia`
- Owns: RAM and disk caching through the linear `Base → RamCached → DiskCached` loader hierarchy
- Accept: cache behavior selected by loader instance/type (RAM-only vs RAM+disk), not per-call flags; disk cache rooted at an explicit `cacheFolder`
- Reject: bespoke image-cache layers beside the loader hierarchy; treating `DiskCachedWebImageLoader` as RAM-bypassing (it inherits RAM caching)
