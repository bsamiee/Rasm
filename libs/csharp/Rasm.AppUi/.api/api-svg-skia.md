# [RASM_APPUI_API_SVG_SKIA]

`Svg.Controls.Skia.Avalonia` supplies Avalonia SVG controls backed by the `Svg.Skia` `SKSvg` document engine: a full SVG/Android-VectorDrawable load family, a retained scene-graph with incremental mutation-and-render, an SMIL animation host, pointer/access-key interaction, a built-in pan/zoom viewer transform, text selection, and `SKPicture` output drawn into Avalonia through the SkiaSharp draw-lease feature or a compositor custom-visual layer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Svg.Controls.Skia.Avalonia` 12.0.0.13 + `Svg.Skia` 5.1.1
- package: `Svg.Controls.Skia.Avalonia` (Avalonia control layer); the `SKSvg` engine ships in the separately-pinned `Svg.Skia` 5.1.1 package, with `Svg.Model` / `Svg.Custom` / the Fizzler-CSS and `ShimSkiaSharp` model assemblies transitive under it
- license: MIT (`Svg.Skia` family)
- floor: `net10.0` consumer; both assemblies resolve from `lib/net10.0` (`Svg.Controls.Skia.Avalonia.dll`, `Svg.Skia.dll`)
- assembly: `Svg.Controls.Skia.Avalonia` (control/image/source/draw-op), `Svg.Skia` (`SKSvg` engine, parameters, scene/selection model)
- namespace: `Avalonia.Svg.Skia` (control surface), `Svg.Skia` (`SKSvg`, `SvgParameters`, scene/selection types)
- asset: runtime library (managed; Skia native assets arrive via the central SkiaSharp pin, not this package)
- rail: assets

## [02]-[PUBLIC_TYPES]

[AVALONIA_SVG_TYPES]: Avalonia control, image, source, and draw-operation surfaces — `Avalonia.Svg.Skia`
- rail: assets

| [INDEX] | [SYMBOL]                       | [KIND]                                         | [RAIL]                                              |
| :-----: | :----------------------------- | :--------------------------------------------- | :-------------------------------------------------- |
|  [01]   | `Svg`                          | `Control` (interactive SVG view)               | path/source/stretch/wireframe/zoom/pan/animation    |
|  [02]   | `SvgImage`                     | `AvaloniaObject, IImage`                       | brush/image-source surface (`Source`, `Size`)       |
|  [03]   | `SvgSource`                    | `sealed IDisposable`                           | loaded SVG document handle (`SKSvg` + `SKPicture`)   |
|  [04]   | `SvgCustomDrawOperation`       | `ICustomDrawOperation`                         | leased-canvas draw op binding `SKSvg.Draw`          |
|  [05]   | `SvgSourceCustomDrawOperation` | `ICustomDrawOperation`                         | source-backed draw op                               |
|  [06]   | `SvgResourceExtension`         | `MarkupExtension`                              | `{SvgResource …}` XAML resource binding             |
|  [07]   | `SvgImageExtension`            | `MarkupExtension`                              | `{SvgImage …}` XAML image binding                   |
|  [08]   | `SvgSourceTypeConverter`       | `TypeConverter`                                | string/URI -> `SvgSource` conversion                |
|  [09]   | `ServiceProviderExtensions`    | static extensions                              | `IServiceProvider` base-URI/asset-loader resolution |

[SVG_SKIA_TYPES]: document engine, parameters, scene, selection, and interaction model — `Svg.Skia`
- rail: assets

| [INDEX] | [SYMBOL]                   | [KIND]                            | [RAIL]                                                |
| :-----: | :------------------------- | :-------------------------------- | :---------------------------------------------------- |
|  [01]   | `SKSvg`                    | `IDisposable, ISKSvgJavaScriptViewerHost` | the SVG document engine (load/render/animate/interact) |
|  [02]   | `SvgParameters`            | parameter bag                     | per-load CSS / entities / culture overrides            |
|  [03]   | `SvgSceneDocument`         | retained scene document           | source document + asset loader + scene-node graph      |
|  [04]   | `SvgSceneNode`             | retained scene node               | element-addressable rendered node (mutation target)    |
|  [05]   | `SvgSceneResource`         | retained scene resource           | def/resource node resolvable by id/address             |
|  [06]   | `SvgSceneMutationResult`   | mutation receipt                  | dirty-region + re-render result of a retained mutation |
|  [07]   | `SvgTextSelectionRange`    | readonly struct                   | selection extents, caret, anchor/focus charnum         |
|  [08]   | `SvgTextSelectionDirection` | enum                             | `None` / `Forward` / `Backward`                        |
|  [09]   | `SKSvgDrawEventArgs`       | event args                        | `OnDraw` canvas-draw hook payload                      |
|  [10]   | `SvgAnimationFrameChangedEventArgs` | event args               | `AnimationInvalidated` frame payload                   |
|  [11]   | `SKSvgViewerTransformChangedEventArgs` | event args            | `ViewerTransformChanged` pan/zoom payload              |

Phantom-strip note: the prior `SvgSceneRenderer`, `SvgAnimationController`, `SvgInteractionDispatcher`, `SkiaSvgAssetLoader`, and `ISvgAssetLoader`-under-`Avalonia.Svg.Skia` rows do not exist. Asset loading is resolved through `Svg.Model.ISvgAssetLoader` (referenced by `SvgSource.s_assetLoader` and `SKSvg` statics), not an `Avalonia.Svg.Skia` loader type. The native composition layer is `internal SvgCompositionVisualScene` (a `CompositionCustomVisualHandler`), not a public scene type.

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: `Svg` control properties and operations
- rail: assets

| [INDEX] | [SURFACE]                                                | [SURFACE_ROOT] | [RAIL]                                              |
| :-----: | :------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `Path` / `Source` / `SvgSource`                          | `Svg`          | string path / object source / typed `SvgSource?`    |
|  [02]   | `Stretch` / `StretchDirection`                           | `Svg`          | layout fit and upscale policy                       |
|  [03]   | `EnableCache` / `Wireframe` / `DisableFilters`           | `Svg`          | source cache, wireframe overlay, filter bypass      |
|  [04]   | `Zoom` / `PanX` / `PanY`                                 | `Svg`          | viewer transform direct-properties                  |
|  [05]   | `ZoomToPoint(double newZoom, Point)`                    | `Svg`          | zoom anchored at a control point                    |
|  [06]   | `AnimationBackend` / `AnimationFrameInterval` / `AnimationPlaybackRate` | `Svg` | animation host backend + cadence + rate          |
|  [07]   | `ActualAnimationBackend` / `AnimationBackendFallbackReason` | `Svg`       | resolved backend + degrade reason                   |
|  [08]   | `SkSvg : SKSvg?`                                         | `Svg`          | underlying engine handle for direct interaction     |
|  [09]   | `TryGetPicturePoint(Point, out SKPoint)`               | `Svg`          | map control point into picture space                |
|  [10]   | `LoadFromSvgDocument(SvgDocument?, SvgParameters?)`     | `Svg`          | load a pre-parsed `Svg.SvgDocument`                 |
|  [11]   | `GetCss/SetCss`, `GetCurrentCss/SetCurrentCss`, `SetCurrentColor` (attached) | `Svg` | per-element CSS / current-color override        |

[ENGINE_LOAD_ENTRYPOINTS]: `SKSvg` document load and output
- rail: assets

| [INDEX] | [SURFACE]                                                                     | [SURFACE_ROOT] | [RAIL]                                          |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `Load(string\|Stream\|XmlReader, SvgParameters?, Uri? baseUri) : SKPicture?`   | `SKSvg`        | SVG load (path/stream/reader overloads)        |
|  [02]   | `FromSvg(string) : SKPicture?` / `FromSvgDocument(SvgDocument?) : SKPicture?`  | `SKSvg`        | in-memory string / pre-parsed document load    |
|  [03]   | `FromVectorDrawable(string)` / `LoadVectorDrawable(stream\|path\|reader)`      | `SKSvg`        | Android VectorDrawable XML load                |
|  [04]   | `ReLoad(SvgParameters?)` / `RefreshFromSourceDocument()` / `RebuildFromModel()` | `SKSvg`       | re-apply parameters / re-record from source/model |
|  [05]   | `Model` / `WireframePicture` (`SKPicture?`)                                    | `SKSvg`        | recorded source model / wireframe picture      |
|  [06]   | `Draw(SKCanvas)` / `Save(Stream\|string, SKColor bg, …)`                       | `SKSvg`        | canvas draw / encoded raster export            |
|  [07]   | `Clone()` / `Sync` / `Dispose()`                                               | `SKSvg`        | deep clone, render-lock object, teardown       |
|  [08]   | `static CreateFromFile/Stream/Svg/SvgDocument/VectorDrawable/XmlReader(…)`     | `SKSvg`        | one-call factory mirrors of the load family    |
|  [09]   | `TryGetPicturePoint(SKPoint, SKMatrix, out SKPoint)` / `TryGetPictureRect(…)`  | `SKSvg`        | canvas-to-picture coordinate mapping           |

[RETAINED_SCENE_ENTRYPOINTS]: incremental retained scene graph + mutation
- rail: assets

| [INDEX] | [SURFACE]                                                                                          | [SURFACE_ROOT] | [RAIL]                                              |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `TryEnsureRetainedSceneGraph(out SvgSceneDocument?)` / `HasRetainedSceneGraph`                      | `SKSvg`        | build / probe the retained scene document          |
|  [02]   | `TryApplyRetainedSceneMutationAndRender(SvgElement\|addressKey, IReadOnlyCollection<string>? changedAttributes, out SvgSceneMutationResult?)` | `SKSvg` | re-render only the dirty region after an element/attribute change |
|  [03]   | `TryApplyRetainedSceneMutationByIdAndRender(string id, …, out SvgSceneMutationResult?)`             | `SKSvg`        | mutate-and-render addressed by element id           |
|  [04]   | `TryGetRetainedSceneNode(s)(SvgElement\|addressKey, out SvgSceneNode[?])` / `…ById(string id, …)`  | `SKSvg`        | resolve scene nodes by element/address/id           |
|  [05]   | `TryGetRetainedSceneResource(addressKey\|id, out SvgSceneResource?)`                                | `SKSvg`        | resolve a def/resource node                         |
|  [06]   | `CreateRetainedScene{Model,Picture}(SvgElement, SKRect? clip)` / `CreateRetainedSceneNode{Model,Picture}(SvgSceneNode, …)` | `SKSvg` | record an `SKPicture` for a subtree/node       |
|  [07]   | `CreateRetainedSceneGraph{Model,Picture}()`                                                         | `SKSvg`        | record the whole retained graph                     |

[ANIMATION_AND_INTERACTION_ENTRYPOINTS]: SMIL animation, pointer/access-key, viewer transform, text selection
- rail: assets

| [INDEX] | [SURFACE]                                                                                  | [SURFACE_ROOT] | [RAIL]                                             |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `BeginAnimationElement(SvgAnimationElement\|string id, TimeSpan offset)` / `EndAnimationElement(…)` | `SKSvg` | start/stop an SMIL animation element             |
|  [02]   | `FlushPendingAnimationFrame()` / `HasPendingAnimationFrame` / `LastAnimationDirtyTargetCount` | `SKSvg`     | drive and observe the animation frame             |
|  [03]   | `AnimationInvalidated : EventHandler<SvgAnimationFrameChangedEventArgs>`                    | `SKSvg`        | per-frame invalidation signal                      |
|  [04]   | `NotifyPointerEvent(SvgElement?, SvgPointerEventType, TimeSpan? presentationTime)`          | `SKSvg`        | dispatch a DOM pointer event into the scene        |
|  [05]   | `NotifyAccessKey(string? accessKey, TimeSpan? presentationTime)`                            | `SKSvg`        | dispatch an access-key activation                  |
|  [06]   | `ZoomBy/ZoomTo(double)` / `PanBy/PanTo(SKPoint)` / `SetViewerTransform(double, SKPoint)` / `ResetViewerTransform()` | `SKSvg` | built-in pan/zoom viewer model              |
|  [07]   | `ViewerTransformChanged : EventHandler<SKSvgViewerTransformChangedEventArgs>`               | `SKSvg`        | viewer transform changed signal                    |
|  [08]   | `PictureToViewerPoint(SKPoint)` / `TryGetViewerPicturePoint(SKPoint, out SKPoint)`          | `SKSvg`        | picture<->viewer coordinate mapping                |
|  [09]   | `TryBeginTextSelection/TryExtendTextSelection(SvgTextBase, int charnum\|SKPoint)` / `TrySelectTextRange(…)` / `TrySelectTextSubString(…)` | `SKSvg` | text caret/range selection      |
|  [10]   | `TryGetTextSelection(out SvgTextSelectionRange)` / `OnDraw : EventHandler<SKSvgDrawEventArgs>` | `SKSvg`     | read current selection / pre/post-draw hook        |
|  [11]   | `TryCreateNativeCompositionFrame(out SvgNativeCompositionFrame?)` / `TryCreateNativeCompositionScene(out SvgNativeCompositionScene?)` | `SKSvg` | export a compositor-ready frame/scene |

[SOURCE_ENTRYPOINTS]: `SvgSource` / `SvgImage` document and brush surfaces
- rail: assets

| [INDEX] | [SURFACE]                                                                  | [SURFACE_ROOT] | [RAIL]                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `static Load(string path, Uri? baseUri, SvgParameters?)`                   | `SvgSource`    | path/URI load (HTTP via internal `HttpClient`) |
|  [02]   | `static LoadFromStream(Stream, SvgParameters?)`                            | `SvgSource`    | stream load                                    |
|  [03]   | `static LoadFromSvg(string svg, SvgParameters?)`                           | `SvgSource`    | in-memory SVG string load                      |
|  [04]   | `static LoadFromSvgDocument(SvgDocument, SvgParameters?)`                  | `SvgSource`    | pre-parsed document load                       |
|  [05]   | `Path`(init) / `Entities` / `Css` / `CurrentColor`                         | `SvgSource`    | source inputs (CSS, entity table, recolor)     |
|  [06]   | `Svg : SKSvg?` / `Picture : SKPicture?` / `Parameters : SvgParameters?`    | `SvgSource`    | engine handle / rendered picture / parameters  |
|  [07]   | `Clone()` / `ReLoad(SvgParameters?)` / `RebuildFromModel()` / `Dispose()`  | `SvgSource`    | re-parameterize / re-record / teardown         |
|  [08]   | `Source`(`[Content]`) / `Css` / `CurrentCss` / `CurrentColor` / `Size`     | `SvgImage`     | image-source properties + measured size        |
|  [09]   | `Invalidated : EventHandler`                                               | `SvgImage`     | image-invalidation signal for binding hosts    |

## [04]-[IMPLEMENTATION_LAW]

[SVG_ASSET_LAW]:
- Package: `Svg.Controls.Skia.Avalonia` over `Svg.Skia` 5.1.1
- Owns: SVG and Android-VectorDrawable asset controls, source loading (path / stream / string / URI / pre-parsed document / `IServiceProvider` base-URI), retained scene rendering with incremental mutation, SMIL animation, pointer/access-key interaction, built-in viewer pan/zoom, text selection, and `SKPicture` output.
- Accept: SVG and VectorDrawable assets enter the same asset rail as raster and generated visual evidence; a vector asset retains `SvgSource` (`Svg`/`Picture`/`Parameters`) and exposes a live `SKSvg` engine.
- Reject: bitmap-only asset policy; treating the SVG as an opaque blob without `SvgSource`/`SKSvg` state; re-rendering the whole picture on a single-element change when `TryApplyRetainedSceneMutationAndRender` re-renders only the dirty region.

[RETAINED_MUTATION_LAW]:
- The retained scene graph is the incremental render owner: `TryEnsureRetainedSceneGraph` builds an `SvgSceneDocument` (source document + asset loader + element-addressable `SvgSceneNode` graph), `TryGetRetainedSceneNode(s)`/`…ById`/`TryGetRetainedSceneResource` resolve nodes by `SvgElement`, address key, or id, and `TryApplyRetainedSceneMutationAndRender(element|addressKey, changedAttributes, out SvgSceneMutationResult)` re-records only the affected subtree and returns the dirty-region receipt.
- A live model edit that drops the retained graph and reloads is the rejected form — the mutation API, not a full `Load`, owns single-element/attribute updates, and `SvgSceneMutationResult` is the receipt a viewport invalidation keys on.
- `Sync` is the engine's render-lock object: a producer that mutates the scene off the render thread takes `lock (skSvg.Sync)` so a concurrent `Draw`/`UpdateWith*` does not observe a half-applied mutation.

[STACKING]:
- Stacks ONTO `api-avalonia-skia` / `api-skiasharp`: `SvgCustomDrawOperation` (and `SvgSourceCustomDrawOperation`) implement `ICustomDrawOperation`; `Render(ImmediateDrawingContext)` resolves `ISkiaSharpApiLeaseFeature.Lease()` to an `ISkiaSharpApiLease`, reads `lease.SkCanvas`, and calls `SKSvg.Draw(canvas)` — the SVG composites into Avalonia's own Skia surface through the documented draw-lease feature, never a side bitmap. The internal `SvgCompositionVisualScene` does the same through a `CompositionCustomVisualHandler.OnRender` lease for a native composition layer when `AnimationBackend` selects it.
- Stacks ONTO the Avalonia image rail: `SvgImage : IImage` is an image-source the same way a `Bitmap` is, so an `Image.Source`/`ImageBrush` binds an `SvgImage` and its `Size` (the `SKPicture.CullRect`) drives layout; `SvgSource` is the `[Content]`/`TypeConverter` target so `{SvgImage Source=…}`/`Svg Path=…` XAML resolves a string or URI through `SvgSourceTypeConverter`.
- Stacks ONTO `api-asyncimageloader` / the asset pipeline: a URL `SvgSource.Load(path)` uses the engine's internal `HttpClient`, so remote SVG assets resolve on the same asset rail as async-loaded raster; the `IServiceProvider` `SvgSource` ctor resolves the Avalonia base URI and asset loader through `ServiceProviderExtensions`.

[RENDER_LAW]:
- Package: `Svg.Controls.Skia.Avalonia`
- Owns: SVG rendering for panels, companion windows, sidecars, diagnostics, support views, and downstream shells
- Accept: vector assets retain document, scene, animation, interaction, viewer-transform, selection, and rendered-picture state; the control's `Zoom`/`PanX`/`PanY` + `ZoomToPoint` drive the built-in `SKSvg` viewer model, and `AnimationBackend` selects the animation host (with `ActualAnimationBackend`/`AnimationBackendFallbackReason` reporting the resolved tier).
- Reject: opaque SVG blobs without renderable state; a hand-rolled pan/zoom or animation loop when the `SKSvg` viewer-transform and SMIL animation surfaces own them; drawing through a private bitmap instead of the `ISkiaSharpApiLease` canvas.
