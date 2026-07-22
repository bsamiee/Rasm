# [RASM_APPUI_API_SVG_SKIA]

`Svg.Controls.Skia.Avalonia` mints Avalonia SVG and Android-VectorDrawable asset controls over the `Svg.Skia` `SKSvg` document engine, folding every source path onto one asset rail beside raster and generated evidence. `SKSvg` owns a retained element-addressable scene graph whose mutation API re-renders only the dirty region, and drives SMIL animation, interaction, the built-in viewer transform, text selection, and `SKPicture` output composited into Avalonia through the SkiaSharp draw-lease. Control types are the public surface; every render impl below the lease stays internal.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Svg.Controls.Skia.Avalonia`
- package: `Svg.Controls.Skia.Avalonia` (MIT)
- assembly: `Svg.Controls.Skia.Avalonia` (control, image, source, draw-op, markup extensions)
- namespace: `Avalonia.Svg.Skia`
- asset: managed runtime library; Skia natives bind through the central SkiaSharp pin, not this package
- rail: assets

[PACKAGE_SURFACE]: `Svg.Skia`
- package: `Svg.Skia` (MIT)
- assembly: `Svg.Skia` (`SKSvg` engine, `SvgParameters`, scene, selection, and interaction model); `Svg.SceneGraph`, `Svg.Animation`, `Svg.Model`/`Svg.Custom`, Fizzler-CSS, and `ShimSkiaSharp` restore transitively
- namespace: `Svg.Skia`, `Svg.Skia.TypefaceProviders`
- asset: managed runtime library
- rail: assets

## [02]-[PUBLIC_TYPES]

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

[PUBLIC_TYPE_SCOPE]: `Svg.Skia` engine, parameters, scene, selection, and interaction model

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]     | [CAPABILITY]                   |
| :-----: | :------------------------------------- | :---------------- | :----------------------------- |
|  [01]   | `SKSvg`                                | disposable class  | SVG document engine and viewer |
|  [02]   | `SvgParameters`                        | parameter bag     | per-load overrides             |
|  [03]   | `SvgSceneDocument`                     | retained document | source and scene graph         |
|  [04]   | `SvgSceneNode`                         | retained node     | element mutation target        |
|  [05]   | `SvgSceneResource`                     | retained resource | addressable definition         |
|  [06]   | `SvgSceneMutationResult`               | mutation receipt  | dirty-region render result     |
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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Svg` control properties and operations

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `Path` / `Source` / `SvgSource`                                         | property | string, object, or typed source        |
|  [02]   | `Stretch` / `StretchDirection`                                          | property | layout fit and upscale policy          |
|  [03]   | `EnableCache` / `Wireframe` / `DisableFilters`                          | property | cache, overlay, and filter policy      |
|  [04]   | `Zoom` / `PanX` / `PanY`                                                | property | viewer-transform                       |
|  [05]   | `ZoomToPoint(double, Point)`                                            | instance | point-anchored zoom                    |
|  [06]   | `AnimationBackend` / `AnimationFrameInterval` / `AnimationPlaybackRate` | property | animation host, cadence, rate          |
|  [07]   | `ActualAnimationBackend` / `AnimationBackendFallbackReason`             | property | resolved backend and fallback          |
|  [08]   | `SkSvg : SKSvg?`                                                        | property | interactive engine handle              |
|  [09]   | `TryGetPicturePoint(Point, out SKPoint)`                                | instance | control-to-picture mapping             |
|  [10]   | `LoadFromSvgDocument(SvgDocument?, SvgParameters?)`                     | instance | pre-parsed document load               |
|  [11]   | `GetCss/SetCss` / `GetCurrentCss/SetCurrentCss` / `SetCurrentColor`     | attached | element CSS and current-color override |

[ENTRYPOINT_SCOPE]: `SKSvg` document loading and output

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------------ |
|  [01]   | `Load(string\|Stream\|XmlReader, SvgParameters?, Uri?) -> SKPicture?`           | instance | path, stream, or reader load          |
|  [02]   | `FromSvg(string)` / `FromSvgDocument(SvgDocument?) -> SKPicture?`               | instance | string or parsed-document load        |
|  [03]   | `FromVectorDrawable(string)` / `LoadVectorDrawable(stream\|path\|reader)`       | instance | Android VectorDrawable load           |
|  [04]   | `ReLoad(SvgParameters?)` / `RefreshFromSourceDocument()` / `RebuildFromModel()` | instance | parameter reapply and model re-record |
|  [05]   | `Model` / `WireframePicture` (`SKPicture?`)                                     | property | source model and wireframe picture    |
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

[ENTRYPOINT_SCOPE]: `SvgSource` load factories and document state; `SvgImage` image-source properties

| [INDEX] | [SURFACE]                                                                 | [OWNER]     | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------------------------ | :---------- | :------- | :------------------------------ |
|  [01]   | `Load(string, Uri?, SvgParameters?)`                                      | `SvgSource` | static   | path or URI load                |
|  [02]   | `LoadFromStream(Stream, SvgParameters?)`                                  | `SvgSource` | static   | stream load                     |
|  [03]   | `LoadFromSvg(string, SvgParameters?)`                                     | `SvgSource` | static   | in-memory SVG string load       |
|  [04]   | `LoadFromSvgDocument(SvgDocument, SvgParameters?)`                        | `SvgSource` | static   | pre-parsed document load        |
|  [05]   | `Path`(init) / `Entities` / `Css` / `CurrentColor`                        | `SvgSource` | property | CSS, entity, and recolor inputs |
|  [06]   | `Svg : SKSvg?` / `Picture : SKPicture?` / `Parameters`                    | `SvgSource` | property | engine, picture, parameters     |
|  [07]   | `Clone()` / `ReLoad(SvgParameters?)` / `RebuildFromModel()` / `Dispose()` | `SvgSource` | instance | re-parameterize and teardown    |
|  [08]   | `Source`(`[Content]`) / `Css` / `CurrentCss` / `CurrentColor` / `Size`    | `SvgImage`  | property | image-source props and size     |
|  [09]   | `Invalidated` (`EventHandler`)                                            | `SvgImage`  | event    | image-invalidation signal       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `SKSvg` owns incremental render through the retained scene graph: `TryEnsureRetainedSceneGraph` builds the `SvgSceneDocument` (source document, asset loader, element-addressable `SvgSceneNode` graph), and `TryApplyRetainedSceneMutationAndRender(element|addressKey, changedAttributes, out SvgSceneMutationResult)` re-records only the affected subtree and returns the dirty-region receipt a viewport invalidation keys on. A single-element or attribute edit routes through the mutation API, never a full `Load` that drops and rebuilds the graph.
- `Sync` is the engine render-lock: a producer mutating the scene off the render thread takes `lock (skSvg.Sync)`, so a concurrent `Draw` never observes a half-applied mutation.

[STACKING]:
- `api-avalonia-skia`(`.api/api-avalonia-skia.md`) / `api-skiasharp`(`.api/api-skiasharp.md`): `SvgCustomDrawOperation`/`SvgSourceCustomDrawOperation` implement `ICustomDrawOperation`; `Render(ImmediateDrawingContext)` resolves `ISkiaSharpApiLeaseFeature.Lease()`, reads `lease.SkCanvas`, and calls `SKSvg.Draw(canvas)`, compositing the SVG into Avalonia's Skia surface with no side bitmap; the internal `SvgCompositionVisualScene` acquires the same lease through `CompositionCustomVisualHandler.OnRender` when `AnimationBackend` selects the native composition layer.
- `api-avalonia`(`.api/api-avalonia.md`): `SvgImage : IImage` binds an `Image.Source`/`ImageBrush` exactly as a `Bitmap` does, its `Size` (the `SKPicture.CullRect`) driving layout, and `SvgSource` is the `[Content]`/`TypeConverter` target so `{SvgImage Source=…}`/`Svg Path=…` XAML resolves a string or URI through `SvgSourceTypeConverter`.
- `api-asyncimageloader`(`.api/api-asyncimageloader.md`): a URL `SvgSource.Load(path)` resolves remote SVG on the same asset rail as async-loaded raster through the engine's internal `HttpClient`, and the `IServiceProvider` `SvgSource` ctor resolves the Avalonia base URI and asset loader through `ServiceProviderExtensions`.

[LOCAL_ADMISSION]:
- Consume the scene, animation, and interaction types as `Svg.Skia.*`: `SvgSceneDocument`/`SvgSceneNode`/`SvgSceneResource` ship in `Svg.SceneGraph`, `SvgAnimationController` in `Svg.Animation`, and `SvgInteractionDispatcher`/`SkiaSvgAssetLoader` in `Svg.Skia` itself.
- Asset loading flows through `Svg.Model.ISvgAssetLoader` (referenced by `SvgSource` and `SKSvg` statics); no `ISvgAssetLoader` type exists under `Avalonia.Svg.Skia`.
- `SvgCompositionVisualScene` (`CompositionCustomVisualHandler`) is the internal native composition layer, reached through `AnimationBackend`, never a public scene type.
- A vector asset enters the shared asset rail retaining `SvgSource` (`Svg`/`Picture`/`Parameters`) and a live `SKSvg` engine, never an opaque blob.

[RAIL_LAW]:
- Package: `Svg.Controls.Skia.Avalonia` over `Svg.Skia`
- Owns: SVG and Android-VectorDrawable asset controls — source loading (path, stream, string, URI, pre-parsed document, `IServiceProvider` base-URI), retained-scene incremental render, SMIL animation, pointer/access-key interaction, built-in viewer pan/zoom, text selection, and `SKPicture` output for panels, companion windows, sidecars, diagnostics, and downstream shells
- Accept: vector assets retain document, scene, animation, interaction, viewer-transform, selection, and picture state; the control's `Zoom`/`PanX`/`PanY` + `ZoomToPoint` drive the built-in `SKSvg` viewer model, and `AnimationBackend` selects the animation host with `ActualAnimationBackend`/`AnimationBackendFallbackReason` reporting the resolved tier
- Reject: a bitmap-only or opaque-blob SVG policy without `SvgSource`/`SKSvg` state; a full-picture re-render on a single-element change that `TryApplyRetainedSceneMutationAndRender` renders dirty-region-only; a hand-rolled pan/zoom or animation loop over the owned viewer-transform and SMIL surfaces; drawing through a private bitmap instead of the `ISkiaSharpApiLease` canvas
