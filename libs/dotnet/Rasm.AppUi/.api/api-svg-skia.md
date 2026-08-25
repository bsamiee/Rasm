# [RASM_APPUI_API_SVG_SKIA]

`Svg.Controls.Skia.Avalonia` mints Avalonia SVG and Android-VectorDrawable asset controls over the `Svg.Skia` `SKSvg` document engine, folding every source path onto one asset rail beside raster and generated evidence. `SKSvg` owns a retained element-addressable scene graph whose mutation API re-renders only the dirty region, and drives SMIL animation, interaction, the built-in viewer transform, text selection, and `SKPicture` output composited into Avalonia through the SkiaSharp draw-lease. Control types are the public surface; every render impl below the lease stays internal.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: Avalonia control, image, source, and draw-operation surfaces (`Avalonia.Svg.Skia`)

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]          | [CAPABILITY]                             |
| :-----: | :----------------------------- | :--------------------- | :--------------------------------------- |
|  [01]   | `Svg`                          | control                | interactive SVG view                     |
|  [02]   | `SvgImage`                     | `IImage` object        | brush and image-source surface           |
|  [03]   | `SvgSource`                    | sealed IDisposable     | loaded document handle (`SKSvg` picture) |
|  [04]   | `SvgCustomDrawOperation`       | `ICustomDrawOperation` | leased-canvas draw op over `SKSvg.Draw`  |
|  [05]   | `SvgSourceCustomDrawOperation` | `ICustomDrawOperation` | source-backed draw op                    |
|  [06]   | `SvgResourceExtension`         | markup extension       | `{SvgResource …}` resource binding       |
|  [07]   | `SvgImageExtension`            | markup extension       | `{SvgImage …}` image binding             |
|  [08]   | `SvgSourceTypeConverter`       | type converter         | string or URI to `SvgSource`             |
|  [09]   | `ServiceProviderExtensions`    | static                 | base-URI and asset-loader resolution     |

[PUBLIC_TYPE_SCOPE]: `Svg.Skia` engine, scene, selection, and interaction model with the `Svg.Model` load-parameter carrier

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]     | [CAPABILITY]                   |
| :-----: | :------------------------------------- | :---------------- | :----------------------------- |
|  [01]   | `SKSvg`                                | disposable class  | SVG document engine and viewer |
|  [02]   | `SvgParameters`                        | record struct     | per-load overrides             |
|  [03]   | `SvgSceneDocument`                     | retained document | source and scene graph         |
|  [04]   | `SvgSceneNode`                         | retained node     | element mutation target        |
|  [05]   | `SvgSceneResource`                     | retained resource | addressable definition         |
|  [06]   | `SvgSceneMutationResult`               | mutation result   | dirty-region render result     |
|  [07]   | `SvgTextSelectionRange`                | readonly struct   | selection and caret extents    |
|  [08]   | `SvgTextSelectionDirection`            | enum              | selection direction            |
|  [09]   | `SKSvgDrawEventArgs`                   | event args        | `OnDraw` payload               |
|  [10]   | `SvgAnimationFrameChangedEventArgs`    | event args        | frame invalidation payload     |
|  [11]   | `SKSvgViewerTransformChangedEventArgs` | event args        | viewer-transform payload       |
|  [12]   | `SKSvgSettings`                        | settings bag      | engine settings                |
|  [13]   | `ITypefaceProvider`                    | interface         | typeface resolution contract   |
|  [14]   | `FontManagerTypefaceProvider`          | class             | `SKFontManager` provider       |
|  [15]   | `SvgInteractionDispatcher`             | sealed dispatcher | pointer and focus dispatch     |
|  [16]   | `SvgAnimationController`               | class             | pause, seek, and target keys   |
|  [17]   | `SvgPointerInput`                      | input carrier     | pointer dispatch argument      |
|  [18]   | `SvgInteractionDispatchResult`         | result carrier    | pointer dispatch outcome       |

- `SvgTextSelectionDirection`: `None` `Forward` `Backward`.
- `SvgParameters` homes in `Svg.Model`, never `Svg.Skia` or `Avalonia.Svg.Skia`: `readonly record struct SvgParameters(Dictionary<string, string>? Entities, string? Css, Color? CurrentColor = null, SvgDocumentLoadOptions? LoadOptions = null)`, its `Color` the `System.Drawing.Color` the parser reads — the `Avalonia.Media.Color` a control or `SvgImage` carries converts at the boundary.

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Svg` control properties and operations

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `Path` / `Source` / `SvgSource`                                         | property | string, object, or typed source   |
|  [02]   | `Stretch` / `StretchDirection`                                          | property | layout fit and upscale policy     |
|  [03]   | `EnableCache` / `Wireframe` / `DisableFilters`                          | property | cache, overlay, and filter policy |
|  [04]   | `Zoom` / `PanX` / `PanY`                                                | property | viewer-transform                  |
|  [05]   | `ZoomToPoint(double, Point)`                                            | instance | point-anchored zoom               |
|  [06]   | `AnimationBackend` / `AnimationFrameInterval` / `AnimationPlaybackRate` | property | animation host, cadence, rate     |
|  [07]   | `ActualAnimationBackend` / `AnimationBackendFallbackReason`             | property | resolved backend and fallback     |
|  [08]   | `SkSvg : SKSvg?`                                                        | property | interactive engine handle         |
|  [09]   | `TryGetPicturePoint(Point, out SKPoint)`                                | instance | control-to-picture mapping        |
|  [10]   | `LoadFromSvgDocument(SvgDocument?, SvgParameters?)`                     | instance | pre-parsed document load          |
|  [11]   | `Svg.GetCss` / `SetCss` / `GetCurrentCss` / `SetCurrentCss`             | static   | attached CSS override             |
|  [12]   | `Svg.GetCurrentColor` / `SetCurrentColor`                               | static   | attached current-color override   |
|  [13]   | `CurrentColor` (`Color?`)                                               | property | instance accessor over row [12]   |

- `Svg.CssProperty` / `CurrentCssProperty` / `CurrentColorProperty` register as `AttachedProperty<T>` on `AvaloniaObject` with inheritance on, so a `Setter` in any `Style` or `ControlTheme` sets them and an ancestor value flows to every descendant `Svg` — one theme-variant `Style` recolors a whole icon tree.

[ENTRYPOINT_SCOPE]: `SKSvg` document loading and output

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `Load(string\|Stream\|XmlReader, SvgParameters?, Uri?) -> SKPicture?`           | instance | path, stream, or reader load          |
|  [02]   | `FromSvg(string)` / `FromSvgDocument(SvgDocument?) -> SKPicture?`               | instance | string or parsed-document load        |
|  [03]   | `FromVectorDrawable(string)` / `LoadVectorDrawable(stream\|path\|reader)`       | instance | Android VectorDrawable load           |
|  [04]   | `ReLoad(SvgParameters?)` / `RefreshFromSourceDocument()` / `RebuildFromModel()` | instance | parameter reapply and model re-record |
|  [05]   | `Model` / `WireframePicture` (`SKPicture?`) / `SourceDocument` (`SvgDocument?`) | property | model, wireframe, and parsed document |
|  [06]   | `Draw(SKCanvas)` / `Save(Stream\|string, SKColor, …)`                           | instance | canvas draw and raster export         |
|  [07]   | `Clone()` / `Sync` / `Dispose()`                                                | instance | clone, render lock, and teardown      |
|  [08]   | `static CreateFromFile/Stream/Svg/SvgDocument/VectorDrawable/XmlReader(…)`      | factory  | one-call load factories               |
|  [09]   | `TryGetPicturePoint(SKPoint, SKMatrix, out SKPoint)` / `TryGetPictureRect(…)`   | instance | canvas-to-picture mapping             |
|  [10]   | `Settings` (`SKSvgSettings`)                                                    | property | engine settings and typeface chain    |

- `SKSvg.Settings.TypefaceProviders?.Add(ITypefaceProvider)` admits the font-chain provider (`ITypefaceProvider`: `FromFamilyName`/`CreateTypeface`; `FontManagerTypefaceProvider.FontManager` is a settable `SKFontManager`); `Settings` carries color-managed working spaces.

[ENTRYPOINT_SCOPE]: `SKSvg` retained scene graph and incremental mutation rail; the mutation-and-render pair returns `out SvgSceneMutationResult?`

| [INDEX] | [SURFACE]                                                                                         | [SHAPE]  | [CAPABILITY]              |
| :-----: | :------------------------------------------------------------------------------------------------ | :------- | :------------------------ |
|  [01]   | `TryEnsureRetainedSceneGraph(out SvgSceneDocument?)`                                              | instance | build the scene graph     |
|  [02]   | `HasRetainedSceneGraph`                                                                           | property | scene availability        |
|  [03]   | `RetainedSceneGraph` (`SvgSceneDocument?`)                                                        | property | retained document         |
|  [04]   | `TryApplyRetainedSceneMutationAndRender(SvgElement\|addressKey, IReadOnlyCollection<string>?)`    | instance | addressed dirty render    |
|  [05]   | `TryApplyRetainedSceneMutationByIdAndRender(string id, …)`                                        | instance | id-addressed dirty render |
|  [06]   | `TryGetRetainedSceneNode(s)(SvgElement\|addressKey, out SvgSceneNode[?])` / `…ById(string id, …)` | instance | scene-node resolution     |
|  [07]   | `TryGetRetainedSceneResource(addressKey\|id, out SvgSceneResource?)`                              | instance | resource resolution       |
|  [08]   | `CreateRetainedScene{Model,Picture}(SvgElement, SKRect? clip)`                                    | instance | subtree recording         |
|  [09]   | `CreateRetainedSceneNode{Model,Picture}(SvgSceneNode, …)`                                         | instance | node recording            |
|  [10]   | `CreateRetainedSceneGraph{Model,Picture}()`                                                       | instance | whole-graph recording     |

[ENTRYPOINT_SCOPE]: `SKSvg` SMIL animation, interaction, viewer transform, selection, composition, and hit testing

| [INDEX] | [SURFACE]                                                                                | [SHAPE]  | [CAPABILITY]               |
| :-----: | :--------------------------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `BeginAnimationElement(SvgAnimationElement\|string id, TimeSpan)`                        | instance | start animation element    |
|  [02]   | `EndAnimationElement(…)`                                                                 | instance | stop animation element     |
|  [03]   | `FlushPendingAnimationFrame()`                                                           | instance | drive pending frame        |
|  [04]   | `HasPendingAnimationFrame` / `LastAnimationDirtyTargetCount`                             | property | pending and dirty state    |
|  [05]   | `AnimationInvalidated` (`EventHandler<SvgAnimationFrameChangedEventArgs>`)               | event    | frame invalidation         |
|  [06]   | `NotifyPointerEvent(SvgElement?, SvgPointerEventType, TimeSpan?)`                        | instance | DOM pointer dispatch       |
|  [07]   | `NotifyAccessKey(string?, TimeSpan?)`                                                    | instance | access-key dispatch        |
|  [08]   | `ZoomBy/ZoomTo(double)` / `PanBy/PanTo(SKPoint)`                                         | instance | viewer zoom and pan        |
|  [09]   | `SetViewerTransform(double, SKPoint)` / `ResetViewerTransform()`                         | instance | viewer-transform mutate    |
|  [10]   | `ViewerTransformChanged` (`EventHandler<SKSvgViewerTransformChangedEventArgs>`)          | event    | transform-change           |
|  [11]   | `PictureToViewerPoint(SKPoint)` / `TryGetViewerPicturePoint(SKPoint, out SKPoint)`       | instance | picture/viewer mapping     |
|  [12]   | `TryBeginTextSelection(SvgTextBase, int charnum\|SKPoint)` / `TryExtendTextSelection(…)` | instance | selection start and extend |
|  [13]   | `TrySelectTextRange(…)` / `TrySelectTextSubString(…)`                                    | instance | range and substring select |
|  [14]   | `TryGetTextSelection(out SvgTextSelectionRange)`                                         | instance | selection read             |
|  [15]   | `OnDraw` (`EventHandler<SKSvgDrawEventArgs>`)                                            | event    | canvas draw                |
|  [16]   | `TryCreateNativeCompositionFrame(out SvgNativeCompositionFrame?)`                        | instance | composition-frame export   |
|  [17]   | `TryCreateNativeCompositionScene(out SvgNativeCompositionScene?)`                        | instance | composition-scene export   |
|  [18]   | `AnimationController` (`SvgAnimationController`)                                         | property | animation-controller read  |
|  [19]   | `HitTestSceneNodes(SKPoint\|SKRect[, SKMatrix]) -> IEnumerable<SvgSceneNode>`            | instance | scene-node hit set         |
|  [20]   | `HitTestTopmostSceneNode(SKPoint[, SKMatrix]) -> SvgSceneNode?`                          | instance | topmost scene node         |
|  [21]   | `HitTestElements(SKPoint\|SKRect[, SKMatrix]) -> IEnumerable<SvgElement>`                | instance | element hit set            |
|  [22]   | `HitTestTopmostElement(SKPoint[, SKMatrix]) -> SvgElement?`                              | instance | topmost element            |

[ENTRYPOINT_SCOPE]: `SvgInteractionDispatcher` DOM pointer and focus dispatch — every dispatch returns `SvgInteractionDispatchResult`

| [INDEX] | [SURFACE]                                                                                    | [SHAPE]  | [CAPABILITY]            |
| :-----: | :------------------------------------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `HitTestTopmostElement(SKSvg?, SKPoint) -> SvgElement?`                                      | instance | dispatcher hit test     |
|  [02]   | `DispatchPointer{Moved,Pressed,Click,Released,WheelChanged,Exited}(SKSvg?, SvgPointerInput)` | instance | pointer dispatch family |
|  [03]   | `FocusElement(SKSvg?, SvgElement?, SvgPointerInput)`                                         | instance | element focus           |
|  [04]   | `BlurFocusedElement(SKSvg?, SvgPointerInput)`                                                | instance | element blur            |

[ENTRYPOINT_SCOPE]: `SvgSource` load factories and document state

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------- |
|  [01]   | `Load(string, Uri?, SvgParameters?)`                                            | static   | path or URI load                 |
|  [02]   | `LoadAsync(string, Uri?, SvgParameters?, CancellationToken) -> Task<SvgSource>` | static   | off-thread path or URI load      |
|  [03]   | `LoadFromStream(Stream, SvgParameters?)`                                        | static   | stream load                      |
|  [04]   | `LoadFromSvg(string, SvgParameters?)`                                           | static   | in-memory SVG string load        |
|  [05]   | `LoadFromSvgDocument(SvgDocument, SvgParameters?)`                              | static   | pre-parsed document load         |
|  [06]   | `Path`(`[Content]`) / `Entities` / `Css` / `CurrentColor`                       | property | init-only CSS and recolor inputs |
|  [07]   | `Svg : SKSvg?` / `Picture : SKPicture?` / `Parameters`                          | property | engine, picture, parameters      |
|  [08]   | `Clone()` / `ReLoad(SvgParameters?)` / `RebuildFromModel()` / `Dispose()`       | instance | re-parameterize and teardown     |
|  [09]   | `ReLoadAsync(SvgParameters?, CancellationToken) -> Task`                        | instance | off-thread re-parameterize       |

- `SvgSource.Path`/`Entities`/`Css`/`CurrentColor` are `init`-only: a post-construction restyle rides `ReLoad`/`ReLoadAsync` with a fresh `SvgParameters`, never a property write.

[ENTRYPOINT_SCOPE]: `SvgImage` image-source properties and invalidation

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]               |
| :-----: | :------------------------------------------------------------ | :------- | :------------------------- |
|  [01]   | `Source`(`[Content]`) / `Css` / `CurrentCss` / `CurrentColor` | property | source and style overrides |
|  [02]   | `Size` (from `SKPicture.CullRect`)                            | property | layout extent              |
|  [03]   | `Clone() -> SvgImage`                                         | instance | source and override copy   |
|  [04]   | `Invalidated` (`EventHandler`)                                | event    | image-invalidation signal  |

- `SvgImage.SourceProperty`/`CssProperty`/`CurrentCssProperty`/`CurrentColorProperty` are `StyledProperty<T>` on an `AvaloniaObject`, so each takes a `DynamicResource` and a theme-variant swap; a change to any of the three overrides calls `SvgSource.ReLoad` on the bound source in place and raises `Invalidated`, re-rendering every `Image`/`ImageBrush` holding it.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SKSvg` owns incremental render through the retained scene graph: `TryEnsureRetainedSceneGraph` builds the `SvgSceneDocument` (source document, asset loader, element-addressable `SvgSceneNode` graph), and `TryApplyRetainedSceneMutationAndRender(element|addressKey, changedAttributes, out SvgSceneMutationResult)` re-records only the affected subtree and returns the dirty-region result a viewport invalidation keys on. Every single-element and attribute edit routes through the mutation API, never a full `Load` that drops and rebuilds the graph.
- `Sync` is the engine render-lock: a producer mutating the scene off the render thread takes `lock (skSvg.Sync)`, so a concurrent `Draw` never observes a half-applied mutation.
- `SourceDocument` is the parsed `SvgDocument` the engine retains beside the recorded `SKPicture?` `Model`: a re-parameterized or recolored `SvgSource.LoadFromSvgDocument(SvgDocument, SvgParameters?)` binds `SourceDocument`, and `RefreshFromSourceDocument()` re-records the picture from it in place — the picture-typed `Model` binds neither.
- `Svg` and `SvgImage` each fold their style overrides into one `SvgParameters.Css` string, space-joining the non-blank layers in `source-css -> Css -> CurrentCss` order and resolving `CurrentColor` as host value, then `SvgParameters.CurrentColor`, then `SvgSource.CurrentColor`.
- `Svg` appends all three CSS layers; `SvgImage` drops the source layer whenever either of its own two is non-blank, so `SvgImage.Css` REPLACES the source stylesheet where the same string on `Svg` extends it — a shared `SvgSource` restyled through `SvgImage` states its rules whole.

[STACKING]:
- `api-avalonia-skia`(`.api/api-avalonia-skia.md`) / `api-skiasharp`(`.api/api-skiasharp.md`): `SvgCustomDrawOperation`/`SvgSourceCustomDrawOperation` implement `ICustomDrawOperation`; `Render(ImmediateDrawingContext)` resolves `ISkiaSharpApiLeaseFeature.Lease()`, reads `lease.SkCanvas`, and calls `SKSvg.Draw(canvas)`, compositing the SVG into Avalonia's Skia surface with no side bitmap; the internal `SvgCompositionVisualScene` acquires the same lease through `CompositionCustomVisualHandler.OnRender` when `AnimationBackend` selects the native composition layer.
- `api-avalonia`(`.api/api-avalonia.md`): `SvgImage : IImage` binds an `Image.Source`/`ImageBrush` exactly as a `Bitmap` does, its `Size` (the `SKPicture.CullRect`) driving layout, and `SvgSource` is the `[Content]`/`TypeConverter` target so `{SvgImage Source=…}`/`Svg Path=…` XAML resolves a string or URI through `SvgSourceTypeConverter`.
- `api-asyncimageloader`(`.api/api-asyncimageloader.md`): a URL `SvgSource.Load(path)` resolves remote SVG on the same asset rail as async-loaded raster through the engine's internal `HttpClient`, and the `IServiceProvider` `SvgSource` ctor resolves the Avalonia base URI and asset loader through `ServiceProviderExtensions`.

[LOCAL_ADMISSION]:
- Consume the scene, animation, and interaction types as `Svg.Skia.*`: `SvgSceneDocument`/`SvgSceneNode`/`SvgSceneResource` ship in `Svg.SceneGraph`, `SvgAnimationController` in `Svg.Animation`, and `SvgInteractionDispatcher`/`SkiaSvgAssetLoader` in `Svg.Skia` itself.
- Asset loading flows through `Svg.Model.ISvgAssetLoader` (referenced by `SvgSource` and `SKSvg` statics); no `ISvgAssetLoader` type exists under `Avalonia.Svg.Skia`.
- `SvgCompositionVisualScene` (`CompositionCustomVisualHandler`) is the internal native composition layer, reached through `AnimationBackend`, never a public scene type.
- Vector assets enter the shared asset rail retaining `SvgSource` (`Svg`/`Picture`/`Parameters`) and a live `SKSvg` engine, never an opaque blob.
- Theme-driven recolor rides the declared surfaces: an `Svg` subtree takes one inherited `Svg.CurrentColor`/`Svg.Css` `Setter` at its highest common ancestor, and an `SvgImage` takes a `DynamicResource` on `Css`/`CurrentCss`/`CurrentColor`.
- UI-thread path and URI loads take `SvgSource.LoadAsync` and live restyles take `ReLoadAsync`; the synchronous twins parse and record on the calling thread.
