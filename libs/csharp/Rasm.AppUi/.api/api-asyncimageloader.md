# [RASM_APPUI_API_ASYNCIMAGELOADER]

`AsyncImageLoader.Avalonia` (MIT, `net8.0` lib bound under the net10 consumer)
supplies the `AdvancedImage` content control, two attached-loading statics
(`ImageLoader` on `Image`, `ImageBrushLoader` on `ImageBrush`), the
`IAsyncImageLoader`/`IAdvancedAsyncImageLoader` loader contracts, and a three-level
web-image-loader hierarchy (`BaseWebImageLoader` → `RamCachedWebImageLoader` →
`DiskCachedWebImageLoader`) for asynchronous, cached bitmap sourcing off the UI
thread. Cache policy is a property of the chosen loader instance; the loader takes an
injectable `HttpClient`, so it composes with the AppHost networking stack rather than
owning a private one.

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

| [INDEX] | [SYMBOL]                     | [SHAPE]                                          | [RAIL]              |
| :-----: | :--------------------------- | :----------------------------------------------- | :------------------ |
|  [01]   | `AdvancedImage`              | `: ContentControl` — self-loading async image    | async image control |
|  [02]   | `AdvancedImage.ImageWrapper` | `sealed : IImage` (internal ctor) — wraps resolved `IImage` | drawable wrapper |
|  [03]   | `ImageLoader`                | `static` — attached loading on `Image` + global loader | attached loader (Image) |
|  [04]   | `ImageBrushLoader`           | `static` — attached loading on `ImageBrush` + global loader | attached loader (ImageBrush) |

[LOADER_TYPES]: loader contracts and cache hierarchy
- rail: images

The two interfaces both extend `IDisposable`; `IAdvancedAsyncImageLoader` adds the
`IStorageProvider` overload so a loader can resolve `avares:`/storage-scoped URIs.
The concrete hierarchy is linear inheritance, not parallel siblings: disk caching
inherits RAM caching.

| [INDEX] | [SYMBOL]                    | [SHAPE]                                                  | [RAIL]          |
| :-----: | :-------------------------- | :------------------------------------------------------ | :-------------- |
|  [01]   | `IAsyncImageLoader`         | `: IDisposable` — `Task<Bitmap?> ProvideImageAsync(string url)` | loader contract |
|  [02]   | `IAdvancedAsyncImageLoader` | `: IDisposable` — `Task<Bitmap?> ProvideImageAsync(string url, IStorageProvider? = null)` | storage-aware loader contract |
|  [03]   | `BaseWebImageLoader`        | `: IAsyncImageLoader, IAdvancedAsyncImageLoader` — HTTP loader, injectable `HttpClient` | HTTP loader |
|  [04]   | `RamCachedWebImageLoader`   | `: BaseWebImageLoader` — in-memory bitmap cache         | memory cache    |
|  [05]   | `DiskCachedWebImageLoader`  | `: RamCachedWebImageLoader` — RAM + on-disk cache folder | disk + memory cache |

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: `AdvancedImage` construction + properties
- rail: images

`AdvancedImage` has no parameterless constructor: `.ctor(Uri? baseUri)` is the direct
construction surface (`new AdvancedImage(new Uri(source))` — `baseUri` roots the relative
`avares:`/`Source` references the control resolves), and `.ctor(IServiceProvider)` is the
XAML/DI form that pulls `IUriContext.BaseUri`. `Loader`/`Source`/`FallbackImage`/`Stretch`/`StretchDirection`
are `StyledProperty`; `IsLoading`/`CurrentImage`/`ShouldLoaderChangeTriggerUpdate` are
read-projecting `DirectProperty` so they bind one-way into view state.

| [INDEX] | [SURFACE]                         | [TYPE]               | [SURFACE_ROOT]  | [RAIL]          |
| :-----: | :-------------------------------- | :------------------- | :-------------- | :-------------- |
|  [01]   | `Source`                          | `string?`            | `AdvancedImage` | URL/URI input   |
|  [02]   | `Loader`                          | `IAsyncImageLoader?` | `AdvancedImage` | per-control loader override |
|  [03]   | `FallbackImage`                   | `Bitmap?`            | `AdvancedImage` | failure bitmap  |
|  [04]   | `CurrentImage`                    | `IImage?` (Direct)   | `AdvancedImage` | resolved image  |
|  [05]   | `IsLoading`                       | `bool` (Direct)      | `AdvancedImage` | load state      |
|  [06]   | `Stretch` / `StretchDirection`    | `Stretch` / `StretchDirection` | `AdvancedImage` | layout policy   |
|  [07]   | `ShouldLoaderChangeTriggerUpdate` | `bool` (Direct)      | `AdvancedImage` | reload-on-loader-change policy |
|  [08]   | `.ctor(Uri? baseUri)` / `.ctor(IServiceProvider)` | construction | `AdvancedImage` | construct the control — no parameterless ctor; `baseUri` roots relative `avares:`/`Source` URIs |

[ATTACHED_ENTRYPOINTS]: attached loading on plain `Image` / `ImageBrush`
- rail: images

`ImageLoader` targets `Image`; `ImageBrushLoader` targets `ImageBrush`. Both expose a
settable static `AsyncImageLoader` whose default is a `RamCachedWebImageLoader`. The
`Source` change handler auto-dispatches to `IAdvancedAsyncImageLoader` with the host
`TopLevel.StorageProvider` when the active loader implements it.

| [INDEX] | [SURFACE]                                   | [SURFACE_ROOT]     | [RAIL]             |
| :-----: | :------------------------------------------ | :----------------- | :----------------- |
|  [01]   | `SetSource(Image, string?)` / `GetSource(Image)` | `ImageLoader`  | attached URL source |
|  [02]   | `GetIsLoading(Image)`                       | `ImageLoader`      | attached load state |
|  [03]   | `AsyncImageLoader` (`IAsyncImageLoader`, settable; default `RamCachedWebImageLoader`) | `ImageLoader` | global default loader |
|  [04]   | `AsyncImageLoaderLogArea` (`const string`)  | `ImageLoader`      | log-area key       |
|  [05]   | `SourceProperty` / `FallbackImageProperty` / `IsLoadingProperty` (attached) + `AsyncImageLoader` | `ImageBrushLoader` | attached loading on `ImageBrush` |

[LOADER_ENTRYPOINTS]: loader construction and resolution
- rail: images

| [INDEX] | [SURFACE]                                            | [SURFACE_ROOT]            | [RAIL]            |
| :-----: | :--------------------------------------------------- | :------------------------ | :--------------- |
|  [01]   | `.ctor()` / `.ctor(HttpClient, bool disposeHttpClient)` | `BaseWebImageLoader` / `RamCachedWebImageLoader` | loader construction with `HttpClient` injection |
|  [02]   | `.ctor(string cacheFolder = "Cache/Images/")` / `.ctor(HttpClient, bool, string cacheFolder)` | `DiskCachedWebImageLoader` | disk-cache-rooted construction |
|  [03]   | `ProvideImageAsync(string) -> Task<Bitmap?>`         | `IAsyncImageLoader` impls | bitmap resolution |
|  [04]   | `ProvideImageAsync(string, IStorageProvider?) -> Task<Bitmap?>` | `IAdvancedAsyncImageLoader` impls | storage-scoped resolution |
|  [05]   | `HttpClient` (`protected`), `LoadAsync`/`LoadFromLocalAsync`/`LoadFromGlobalCache` (`protected virtual`) | `BaseWebImageLoader` | subclass cache extension points |
|  [06]   | `Dispose()`                                          | `BaseWebImageLoader`      | HTTP/cache teardown |

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
