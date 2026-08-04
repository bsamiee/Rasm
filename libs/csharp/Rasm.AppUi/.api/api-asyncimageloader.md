# [RASM_APPUI_API_ASYNCIMAGELOADER]

`AsyncImageLoader.Avalonia` sources bitmaps asynchronously off the UI thread, carrying load state, fallbacks, and loader-typed cache policy. `AdvancedImage` and the attached `ImageLoader`/`ImageBrushLoader` statics bind a URL or URI `Source` to a cached bitmap through an `IAsyncImageLoader`, and the web-image-loader hierarchy owns HTTP fetch with RAM and disk caching. `AdvancedImage` draws and corner-clips the resolved bitmap in its own `Render`; placeholder, spinner, and transition presentation stay consumer-owned.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `AsyncImageLoader.Avalonia`
- package: `AsyncImageLoader.Avalonia` (MIT)
- assembly: `AsyncImageLoader.Avalonia`
- namespace: `AsyncImageLoader`, `AsyncImageLoader.Loaders`
- target: `net8.0` lib bound under the net10 consumer
- rail: images

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: image control, attached statics, loader contracts, and cache hierarchy

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :--------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `AdvancedImage`              | class         | self-loading `ContentControl` image      |
|  [02]   | `AdvancedImage.ImageWrapper` | sealed class  | `IImage` wrapper over the resolved image |
|  [03]   | `ImageLoader`                | static class  | attached loading on `Image`              |
|  [04]   | `ImageBrushLoader`           | static class  | attached loading on `ImageBrush`         |
|  [05]   | `IAsyncImageLoader`          | interface     | `IDisposable` URL-to-bitmap contract     |
|  [06]   | `IAdvancedAsyncImageLoader`  | interface     | storage-aware URL-to-bitmap contract     |
|  [07]   | `BaseWebImageLoader`         | class         | HTTP loader over injectable `HttpClient` |
|  [08]   | `RamCachedWebImageLoader`    | class         | in-memory bitmap cache                   |
|  [09]   | `DiskCachedWebImageLoader`   | class         | on-disk cache over the RAM cache         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: control construction and properties, attached statics, loader construction, resolution, and protected extension points

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `AdvancedImage.ctor(Uri?)`                                              | ctor     | direct construction                |
|  [02]   | `AdvancedImage.ctor(IServiceProvider)`                                  | ctor     | XAML/DI construction               |
|  [03]   | `AdvancedImage.Source` (`string?`)                                      | property | URL or URI source input            |
|  [04]   | `AdvancedImage.Loader` (`IAsyncImageLoader?`)                           | property | per-control loader override        |
|  [05]   | `AdvancedImage.FallbackImage` (`Bitmap?`)                               | property | failure bitmap                     |
|  [06]   | `AdvancedImage.Stretch` (`Stretch`)                                     | property | stretch policy                     |
|  [07]   | `AdvancedImage.StretchDirection` (`StretchDirection`)                   | property | stretch direction                  |
|  [08]   | `AdvancedImage.ShouldLoaderChangeTriggerUpdate` (`bool`)                | property | reload on loader swap              |
|  [09]   | `AdvancedImage.CurrentImage` (`IImage?`)                                | property | resolved image projection          |
|  [10]   | `AdvancedImage.IsLoading` (`bool`)                                      | property | read-only load state               |
|  [11]   | `ImageLoader.SetSource(Image, string?)`                                 | static   | attach URL to `Image`              |
|  [12]   | `ImageLoader.GetSource(Image)`                                          | static   | read attached URL                  |
|  [13]   | `ImageLoader.GetIsLoading(Image)`                                       | static   | read attached load state           |
|  [14]   | `ImageLoader.AsyncImageLoader` (`IAsyncImageLoader`)                    | property | global `Image` loader              |
|  [15]   | `ImageLoader.AsyncImageLoaderLogArea` (`const string`)                  | static   | log-area key                       |
|  [16]   | `ImageBrushLoader.SetSource(ImageBrush, string?)`                       | static   | attach URL to `ImageBrush`         |
|  [17]   | `ImageBrushLoader.GetSource(ImageBrush)`                                | static   | read attached URL                  |
|  [18]   | `ImageBrushLoader.SetFallbackImage(ImageBrush, Bitmap?)`                | static   | attach fallback bitmap             |
|  [19]   | `ImageBrushLoader.GetFallbackImage(ImageBrush)`                         | static   | read attached fallback             |
|  [20]   | `ImageBrushLoader.GetIsLoading(ImageBrush)`                             | static   | read attached load state           |
|  [21]   | `ImageBrushLoader.AsyncImageLoader` (`IAsyncImageLoader`)               | property | global `ImageBrush` loader         |
|  [22]   | `BaseWebImageLoader.ctor()`                                             | ctor     | owns and disposes a private client |
|  [23]   | `BaseWebImageLoader.ctor(HttpClient, bool)`                             | ctor     | injected client with dispose flag  |
|  [24]   | `RamCachedWebImageLoader.ctor()`                                        | ctor     | private client, RAM cache          |
|  [25]   | `RamCachedWebImageLoader.ctor(HttpClient, bool)`                        | ctor     | injected client, RAM cache         |
|  [26]   | `DiskCachedWebImageLoader.ctor(string)`                                 | ctor     | cache-folder construction          |
|  [27]   | `DiskCachedWebImageLoader.ctor(HttpClient, bool, string)`               | ctor     | injected cache construction        |
|  [28]   | `ProvideImageAsync(string) -> Task<Bitmap?>`                            | instance | bitmap resolution                  |
|  [29]   | `ProvideImageAsync(string, IStorageProvider?) -> Task<Bitmap?>`         | instance | storage-scoped resolution          |
|  [30]   | `RamCachedWebImageLoader.ClearRamCache()`                               | instance | evict the in-memory cache          |
|  [31]   | `BaseWebImageLoader.HttpClient` (`protected`)                           | property | shared client handle               |
|  [32]   | `BaseWebImageLoader.LoadAsync(string, IStorageProvider?)`               | instance | protected resolve override         |
|  [33]   | `BaseWebImageLoader.LoadFromInternalAsync(string)`                      | instance | protected local/`avares:` resolve  |
|  [34]   | `BaseWebImageLoader.LoadDataFromExternalAsync(string) -> Task<byte[]?>` | instance | protected HTTP fetch               |
|  [35]   | `BaseWebImageLoader.LoadFromGlobalCache(string)`                        | instance | protected cache read               |
|  [36]   | `BaseWebImageLoader.SaveToGlobalCache(string, byte[])`                  | instance | protected cache write              |
|  [37]   | `BaseWebImageLoader.Dispose()`                                          | instance | HTTP and cache teardown            |

- `AdvancedImage`: no parameterless ctor; `Uri? baseUri` roots relative `avares:` and `Source` references, `.ctor(IServiceProvider)` pulls `IUriContext.BaseUri`.
- `AdvancedImage.IsLoading` / `CurrentImage` / `ShouldLoaderChangeTriggerUpdate`: `DirectProperty`, so a style selector and a one-way binding both reach them; `IsLoading` registers with no setter and writes only from the load pipeline.
- `AdvancedImage.CurrentImage`: assigning any `IImage` that is not an `AdvancedImage.ImageWrapper` clears `Source`, so a hand-set image and a URL source never both hold.
- `AdvancedImage.Render(DrawingContext)`: draws `CurrentImage` itself and wraps `DrawImage` in `PushClip(RoundedRect)` whenever `CornerRadius` is non-default, re-deriving that rect on every `CornerRadius` and `Bounds` change — rounded corners need no `Border` host and no `OpacityMask`.
- `AdvancedImage.ArrangeOverride`: honors `StretchDirection` on measure and arranges on `Both` unconditionally, so `UpOnly`/`DownOnly` bounds the measured size alone.
- `DiskCachedWebImageLoader`: overrides `LoadFromGlobalCache`/`SaveToGlobalCache` against the disk cache folder while inheriting the RAM cache.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every image resolves off the UI thread through the active `IAsyncImageLoader`, which owns cache policy, load state, and fallback; the concrete loaders form the linear `BaseWebImageLoader -> RamCachedWebImageLoader -> DiskCachedWebImageLoader` chain where each level composes the level below.
- `AdvancedImage` splits pixels from chrome: the bitmap rides the `Render` override outside the template entirely, and the template owns everything else. `MeasureOverride`/`ArrangeOverride` fall through to the `ContentControl` base while `CurrentImage` is null, so `Content` both sizes and fills the control until the bitmap lands — the placeholder slot.
- `avares://AsyncImageLoader.Avalonia/AdvancedImage.axaml` is the one visual the package ships: a `Styles` holding a single `Style` on `AdvancedImage` whose `Template` setter builds a `Grid` around an indeterminate `ProgressBar` (`MaxWidth=100`, centered) template-bound `IsVisible` to `IsLoading`. It carries no `ControlTheme` and no `ContentPresenter`, and applies only under an explicit `StyleInclude`.
- Consumer templates present `Content` and own every fade, skeleton, and cross-dissolve — the type declares no transition or opacity surface.

[STACKING]:
- `api-avalonia`(`.api/api-avalonia.md`): a loader implementing `IAdvancedAsyncImageLoader` resolves `avares:` and storage URIs through the control's `TopLevel.StorageProvider` (`IStorageProvider`), so one `Source` string addresses both remote HTTP and app-scoped resources.
- `api-reactiveui`(`.api/api-reactiveui.md`): the `AdvancedImage.IsLoading`/`CurrentImage` `DirectProperty` projections bind one-way to a `ReactiveObject` view-model flag for spinner and image state.
- AppUi composition: `.ctor(HttpClient, false)` injects the AppHost-owned client so the loader rides the shared handler, auth, proxy, and resilience policy; the AppHost cache path roots `DiskCachedWebImageLoader(cacheFolder)` for restart-surviving assets while `RamCachedWebImageLoader` serves session thumbnails.

[LOCAL_ADMISSION]:
- Remote or async image intent binds `AdvancedImage.Loader` or an attached `ImageLoader`/`ImageBrushLoader.AsyncImageLoader`; the loader instance selects RAM-only or RAM-plus-disk caching by type.
- Rounded image corners set `AdvancedImage.CornerRadius` directly; placeholder, skeleton, and fade presentation land in the AppUi `ControlTheme` for `AdvancedImage`, keying off the `IsLoading`/`CurrentImage` `DirectProperty` selectors and presenting `Content` as the pre-resolution slot.

[RAIL_LAW]:
- Package: `AsyncImageLoader.Avalonia`
- Owns: off-UI-thread bitmap sourcing with load state, fallback, and loader-typed cache policy across the `Base -> RamCached -> DiskCached` hierarchy
- Accept: async image intent bound to `AdvancedImage` or an attached loader; cache behavior selected by loader type; an injected `HttpClient`; disk cache rooted at an explicit `cacheFolder`; corner rounding on `CornerRadius`, and placeholder plus transition presentation in a consumer `ControlTheme`
- Reject: blocking bitmap loads on the UI thread; a private `HttpClient` beside the AppHost networking stack; a bespoke image-cache layer beside the loader hierarchy; a `Border`, clip geometry, or `OpacityMask` wrapper reproducing the native `CornerRadius` clip; a fade or skeleton expected from the package
