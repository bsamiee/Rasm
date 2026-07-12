# [RASM_APPUI_API_SVG_SKIA]

`Svg.Controls.Skia.Avalonia` supplies Avalonia SVG controls backed by the `Svg.Skia` `SKSvg` document engine: a full SVG/Android-VectorDrawable load family, a retained scene-graph with incremental mutation-and-render, an SMIL animation host, pointer/access-key interaction, a built-in pan/zoom viewer transform, text selection, and `SKPicture` output drawn into Avalonia through the SkiaSharp draw-lease feature or a compositor custom-visual layer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Svg.Controls.Skia.Avalonia` + `Svg.Skia`

- package: `Svg.Controls.Skia.Avalonia` (Avalonia control layer); the `SKSvg` engine ships in the separately-pinned `Svg.Skia` package, with `Svg.Model` / `Svg.Custom` / the Fizzler-CSS and `ShimSkiaSharp` model assemblies transitive under it
- license: MIT (`Svg.Skia` family)
- floor: `net10.0` consumer; both assemblies resolve from `lib/net10.0` (`Svg.Controls.Skia.Avalonia.dll`, `Svg.Skia.dll`)
- assembly: `Svg.Controls.Skia.Avalonia` (control/image/source/draw-op), `Svg.Skia` (`SKSvg` engine, parameters, scene/selection model)
- namespace: `Avalonia.Svg.Skia` (control surface), `Svg.Skia` (`SKSvg`, `SvgParameters`, scene/selection types)
- asset: runtime library (managed; Skia native assets arrive via the central SkiaSharp pin, not this package)
- rail: assets

## [02]-[PUBLIC_TYPES]

[AVALONIA_SVG_TYPES]: Avalonia control, image, source, and draw-operation surfaces — `Avalonia.Svg.Skia`

- rail: assets

| [INDEX] | [SYMBOL]                       | [KIND]                           | [RAIL]                                              |
| :-----: | :----------------------------- | :------------------------------- | :-------------------------------------------------- |
|  [01]   | `Svg`                          | `Control` (interactive SVG view) | path/source/stretch/wireframe/zoom/pan/animation    |
|  [02]   | `SvgImage`                     | `AvaloniaObject, IImage`         | brush/image-source surface (`Source`, `Size`)       |
|  [03]   | `SvgSource`                    | `sealed IDisposable`             | loaded SVG document handle (`SKSvg` + `SKPicture`)  |
|  [04]   | `SvgCustomDrawOperation`       | `ICustomDrawOperation`           | leased-canvas draw op binding `SKSvg.Draw`          |
|  [05]   | `SvgSourceCustomDrawOperation` | `ICustomDrawOperation`           | source-backed draw op                               |
|  [06]   | `SvgResourceExtension`         | `MarkupExtension`                | `{SvgResource …}` XAML resource binding             |
|  [07]   | `SvgImageExtension`            | `MarkupExtension`                | `{SvgImage …}` XAML image binding                   |
|  [08]   | `SvgSourceTypeConverter`       | `TypeConverter`                  | string/URI -> `SvgSource` conversion                |
|  [09]   | `ServiceProviderExtensions`    | static extensions                | `IServiceProvider` base-URI/asset-loader resolution |

[SVG_SKIA_TYPES]: `Svg.Skia` owns the document engine, parameters, scene, selection, and interaction model; typeface providers live under `Svg.Skia.TypefaceProviders`.

- rail: assets

| [INDEX] | [SYMBOL]                               | [KIND]                 | [ROLE]                       |
| :-----: | :------------------------------------- | :--------------------- | :--------------------------- |
|  [01]   | `SKSvg`                                | disposable viewer host | SVG document engine          |
|  [02]   | `SvgParameters`                        | parameter bag          | per-load overrides           |
|  [03]   | `SvgSceneDocument`                     | retained document      | source and scene graph       |
|  [04]   | `SvgSceneNode`                         | retained node          | element mutation target      |
|  [05]   | `SvgSceneResource`                     | retained resource      | addressable definition       |
|  [06]   | `SvgSceneMutationResult`               | mutation receipt       | dirty-region render result   |
|  [07]   | `SvgTextSelectionRange`                | readonly struct        | selection and caret extents  |
|  [08]   | `SvgTextSelectionDirection`            | enum                   | selection direction          |
|  [09]   | `SKSvgDrawEventArgs`                   | event args             | `OnDraw` payload             |
|  [10]   | `SvgAnimationFrameChangedEventArgs`    | event args             | frame invalidation payload   |
|  [11]   | `SKSvgViewerTransformChangedEventArgs` | event args             | viewer-transform payload     |
|  [12]   | `SKSvgSettings`                        | settings bag           | engine settings              |
|  [13]   | `ITypefaceProvider`                    | contract               | typeface resolution          |
|  [14]   | `FontManagerTypefaceProvider`          | implementation         | `SKFontManager` provider     |
|  [15]   | `SvgInteractionDispatcher`             | sealed dispatcher      | pointer and focus dispatch   |
|  [16]   | `SvgAnimationController`               | animation controller   | pause, seek, and target keys |
|  [17]   | `SvgPointerInput`                      | input carrier          | pointer dispatch argument    |
|  [18]   | `SvgInteractionDispatchResult`         | result carrier         | pointer dispatch outcome     |

[SELECTION_DIRECTION_VALUES]: `SvgTextSelectionDirection` admits `None`, `Forward`, and `Backward`.

[ENGINE_SETTINGS]: `SKSvg.Settings` exposes `TypefaceProviders` (`IList<ITypefaceProvider>?`) and color-managed working spaces.

[TYPEFACE_PROVIDER_SURFACE]: `ITypefaceProvider` defines `FromFamilyName(...)` and `CreateTypeface(...)`; `FontManagerTypefaceProvider.FontManager` is a settable `SKFontManager`.

[ANIMATION_CONTROLLER_SURFACE]: The `SKSvg.AnimationController` handle is a `SvgAnimationController` from the transitive `Svg.Animation` assembly.

Assembly-provenance note: the scene / animation / interaction types live in the `Svg.Skia` namespace across the transitively-restored `Svg.SceneGraph` (`SvgSceneDocument`/`SvgSceneNode`/`SvgSceneResource`/`SvgSceneRenderer`) and `Svg.Animation` (`SvgAnimationController`) assemblies, while `SvgInteractionDispatcher` and `SkiaSvgAssetLoader` ship in `Svg.Skia.dll` itself — consume them all as `Svg.Skia.*` types. Asset loading also flows through `Svg.Model.ISvgAssetLoader` (referenced by `SvgSource.s_assetLoader` and `SKSvg` statics); there is no `ISvgAssetLoader` type under `Avalonia.Svg.Skia`. The native composition layer is `internal SvgCompositionVisualScene` (a `CompositionCustomVisualHandler`), not a public scene type.

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: `Svg` owns these control properties and operations.

- rail: assets

| [INDEX] | [SURFACE]                                                                    | [RAIL]                                 |
| :-----: | :--------------------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `Path` / `Source` / `SvgSource`                                              | string, object, or typed source        |
|  [02]   | `Stretch` / `StretchDirection`                                               | layout fit and upscale policy          |
|  [03]   | `EnableCache` / `Wireframe` / `DisableFilters`                               | cache, overlay, and filter policy      |
|  [04]   | `Zoom` / `PanX` / `PanY`                                                     | viewer-transform properties            |
|  [05]   | `ZoomToPoint(double newZoom, Point)`                                         | point-anchored zoom                    |
|  [06]   | `AnimationBackend` / `AnimationFrameInterval` / `AnimationPlaybackRate`      | animation host, cadence, and rate      |
|  [07]   | `ActualAnimationBackend` / `AnimationBackendFallbackReason`                  | resolved backend and fallback reason   |
|  [08]   | `SkSvg : SKSvg?`                                                             | interactive engine handle              |
|  [09]   | `TryGetPicturePoint(Point, out SKPoint)`                                     | control-to-picture mapping             |
|  [10]   | `LoadFromSvgDocument(SvgDocument?, SvgParameters?)`                          | pre-parsed document load               |
|  [11]   | `GetCss/SetCss`, `GetCurrentCss/SetCurrentCss`, `SetCurrentColor` (attached) | element CSS and current-color override |

[ENGINE_LOAD_ENTRYPOINTS]: `SKSvg` owns document loading and output.

- rail: assets

| [INDEX] | [SURFACE]                                                                       | [RAIL]                                |
| :-----: | :------------------------------------------------------------------------------ | :------------------------------------ |
|  [01]   | `Load(string\|Stream\|XmlReader, SvgParameters?, Uri? baseUri) : SKPicture?`    | path, stream, or reader SVG load      |
|  [02]   | `FromSvg(string) : SKPicture?` / `FromSvgDocument(SvgDocument?) : SKPicture?`   | string or parsed-document load        |
|  [03]   | `FromVectorDrawable(string)` / `LoadVectorDrawable(stream\|path\|reader)`       | Android VectorDrawable load           |
|  [04]   | `ReLoad(SvgParameters?)` / `RefreshFromSourceDocument()` / `RebuildFromModel()` | parameter reapply and model re-record |
|  [05]   | `Model` / `WireframePicture` (`SKPicture?`)                                     | source model and wireframe picture    |
|  [06]   | `Draw(SKCanvas)` / `Save(Stream\|string, SKColor bg, …)`                        | canvas draw and raster export         |
|  [07]   | `Clone()` / `Sync` / `Dispose()`                                                | clone, render lock, and teardown      |
|  [08]   | `static CreateFromFile/Stream/Svg/SvgDocument/VectorDrawable/XmlReader(…)`      | one-call load factories               |
|  [09]   | `TryGetPicturePoint(SKPoint, SKMatrix, out SKPoint)` / `TryGetPictureRect(…)`   | canvas-to-picture mapping             |
|  [10]   | `Settings` (`SKSvgSettings`)                                                    | engine settings                       |

[TYPEFACE_CHAIN]: `Settings.TypefaceProviders?.Add(ITypefaceProvider)` admits the font-chain provider, and `Settings` carries color-managed working spaces.

[RETAINED_SCENE_ENTRYPOINTS]: `SKSvg` owns the incremental retained scene graph and its mutation rail.

- rail: assets

| [INDEX] | [SURFACE]                                    | [RAIL]                    |
| :-----: | :------------------------------------------- | :------------------------ |
|  [01]   | `TryEnsureRetainedSceneGraph`                | scene construction        |
|  [02]   | `HasRetainedSceneGraph`                      | scene availability        |
|  [03]   | `RetainedSceneGraph`                         | retained document         |
|  [04]   | `TryApplyRetainedSceneMutationAndRender`     | addressed dirty render    |
|  [05]   | `TryApplyRetainedSceneMutationByIdAndRender` | id-addressed dirty render |
|  [06]   | `TryGetRetainedSceneNode(s)`                 | scene-node resolution     |
|  [07]   | `TryGetRetainedSceneResource`                | resource resolution       |
|  [08]   | `CreateRetainedScene{Model,Picture}`         | subtree recording         |
|  [09]   | `CreateRetainedSceneNode{Model,Picture}`     | node recording            |
|  [10]   | `CreateRetainedSceneGraph{Model,Picture}`    | graph recording           |

[RETAINED_SCENE_ACCESS]: `TryEnsureRetainedSceneGraph(out SvgSceneDocument?)` builds the graph; `RetainedSceneGraph` exposes the nullable `SvgSceneDocument`.

[RETAINED_SCENE_MUTATION]: `TryApplyRetainedSceneMutationAndRender(SvgElement|addressKey, IReadOnlyCollection<string>? changedAttributes, out SvgSceneMutationResult?)` targets an element or address key, while `TryApplyRetainedSceneMutationByIdAndRender(string id, …, out SvgSceneMutationResult?)` targets an element id.

[RETAINED_SCENE_RESOLUTION]: `TryGetRetainedSceneNode(s)(SvgElement|addressKey, out SvgSceneNode[?])` and its `…ById(string id, …)` variants resolve scene nodes; `TryGetRetainedSceneResource(addressKey|id, out SvgSceneResource?)` resolves a definition or resource.

[RETAINED_SCENE_RECORDING]: `CreateRetainedScene{Model,Picture}(SvgElement, SKRect? clip)` records a subtree, `CreateRetainedSceneNode{Model,Picture}(SvgSceneNode, …)` records a node, and `CreateRetainedSceneGraph{Model,Picture}()` records the whole graph as a model or `SKPicture`.

[ANIMATION_AND_INTERACTION_ENTRYPOINTS]: `SKSvg` owns SMIL animation, pointer and access-key interaction, viewer transforms, text selection, composition export, and hit testing; `SvgInteractionDispatcher` owns DOM pointer and focus dispatch.

- rail: assets

| [INDEX] | [SURFACE]                         | [OWNER]                    | [RAIL]                    |
| :-----: | :-------------------------------- | :------------------------- | :------------------------ |
|  [01]   | `BeginAnimationElement`           | `SKSvg`                    | start animation element   |
|  [02]   | `EndAnimationElement`             | `SKSvg`                    | stop animation element    |
|  [03]   | `FlushPendingAnimationFrame`      | `SKSvg`                    | drive pending frame       |
|  [04]   | `HasPendingAnimationFrame`        | `SKSvg`                    | pending-frame state       |
|  [05]   | `LastAnimationDirtyTargetCount`   | `SKSvg`                    | dirty-target count        |
|  [06]   | `AnimationInvalidated`            | `SKSvg`                    | frame invalidation event  |
|  [07]   | `NotifyPointerEvent`              | `SKSvg`                    | DOM pointer dispatch      |
|  [08]   | `NotifyAccessKey`                 | `SKSvg`                    | access-key dispatch       |
|  [09]   | `ZoomBy` / `ZoomTo`               | `SKSvg`                    | viewer zoom               |
|  [10]   | `PanBy` / `PanTo`                 | `SKSvg`                    | viewer pan                |
|  [11]   | `SetViewerTransform`              | `SKSvg`                    | viewer-transform update   |
|  [12]   | `ResetViewerTransform`            | `SKSvg`                    | viewer-transform reset    |
|  [13]   | `ViewerTransformChanged`          | `SKSvg`                    | transform-change event    |
|  [14]   | `PictureToViewerPoint`            | `SKSvg`                    | picture-to-viewer mapping |
|  [15]   | `TryGetViewerPicturePoint`        | `SKSvg`                    | viewer-to-picture mapping |
|  [16]   | `TryBeginTextSelection`           | `SKSvg`                    | selection start           |
|  [17]   | `TryExtendTextSelection`          | `SKSvg`                    | selection extension       |
|  [18]   | `TrySelectTextRange`              | `SKSvg`                    | range selection           |
|  [19]   | `TrySelectTextSubString`          | `SKSvg`                    | substring selection       |
|  [20]   | `TryGetTextSelection`             | `SKSvg`                    | selection read            |
|  [21]   | `OnDraw`                          | `SKSvg`                    | canvas-draw event         |
|  [22]   | `TryCreateNativeCompositionFrame` | `SKSvg`                    | composition-frame export  |
|  [23]   | `TryCreateNativeCompositionScene` | `SKSvg`                    | composition-scene export  |
|  [24]   | `AnimationController`             | `SKSvg`                    | animation-controller read |
|  [25]   | `HitTestSceneNodes`               | `SKSvg`                    | scene-node hit set        |
|  [26]   | `HitTestTopmostSceneNode`         | `SKSvg`                    | topmost scene node        |
|  [27]   | `HitTestElements`                 | `SKSvg`                    | element hit set           |
|  [28]   | `HitTestTopmostElement`           | `SKSvg`                    | topmost element           |
|  [29]   | `HitTestTopmostElement`           | `SvgInteractionDispatcher` | dispatcher hit test       |
|  [30]   | `DispatchPointer{Moved,…,Exited}` | `SvgInteractionDispatcher` | pointer dispatch family   |
|  [31]   | `FocusElement`                    | `SvgInteractionDispatcher` | element focus             |
|  [32]   | `BlurFocusedElement`              | `SvgInteractionDispatcher` | element blur              |

[ANIMATION_ELEMENT_CONTROL]: `BeginAnimationElement(SvgAnimationElement|string id, TimeSpan offset)` and `EndAnimationElement(…)` address an SMIL animation element by object or id.

[ANIMATION_FRAME_SURFACE]: `AnimationInvalidated` is an `EventHandler<SvgAnimationFrameChangedEventArgs>`; `FlushPendingAnimationFrame()`, `HasPendingAnimationFrame`, and `LastAnimationDirtyTargetCount` drive and observe frame state.

[INTERACTION_NOTIFICATION]: `NotifyPointerEvent(SvgElement?, SvgPointerEventType, TimeSpan? presentationTime)` dispatches DOM pointer input, and `NotifyAccessKey(string? accessKey, TimeSpan? presentationTime)` dispatches access-key activation.

[VIEWER_TRANSFORM_SURFACE]: `ZoomBy/ZoomTo(double)`, `PanBy/PanTo(SKPoint)`, `SetViewerTransform(double, SKPoint)`, and `ResetViewerTransform()` mutate the viewer model; `ViewerTransformChanged` is an `EventHandler<SKSvgViewerTransformChangedEventArgs>`.

[VIEWER_COORDINATE_SURFACE]: `PictureToViewerPoint(SKPoint)` and `TryGetViewerPicturePoint(SKPoint, out SKPoint)` map between picture and viewer coordinates.

[TEXT_SELECTION_SURFACE]: `TryBeginTextSelection` and `TryExtendTextSelection` accept `SvgTextBase` with an `int charnum` or `SKPoint`; `TrySelectTextRange(…)` and `TrySelectTextSubString(…)` select ranges, while `TryGetTextSelection(out SvgTextSelectionRange)` reads the current selection.

[DRAW_AND_COMPOSITION_SURFACE]: `OnDraw` is an `EventHandler<SKSvgDrawEventArgs>`; `TryCreateNativeCompositionFrame(out SvgNativeCompositionFrame?)` and `TryCreateNativeCompositionScene(out SvgNativeCompositionScene?)` export compositor-ready representations.

[HIT_TEST_SURFACE]: `HitTestSceneNodes(SKPoint|SKRect[, SKMatrix])` returns `IEnumerable<SvgSceneNode>`, `HitTestTopmostSceneNode(SKPoint[, SKMatrix])` returns `SvgSceneNode?`, `HitTestElements(SKPoint|SKRect[, SKMatrix])` returns `IEnumerable<SvgElement>`, and `HitTestTopmostElement(SKPoint[, SKMatrix])` returns `SvgElement?`.

[DISPATCHER_SURFACE]: `HitTestTopmostElement(SKSvg?, SKPoint)` returns `SvgElement?`; `DispatchPointer{Moved,Pressed,Click,Released,WheelChanged,Exited}(SKSvg?, SvgPointerInput)` returns `SvgInteractionDispatchResult`; `FocusElement(SKSvg?, SvgElement?, SvgPointerInput)` and `BlurFocusedElement(SKSvg?, SvgPointerInput)` own focus transitions.

[SOURCE_ENTRYPOINTS]: `SvgSource` exposes static load factories and document state, while `SvgImage` owns image-source properties and invalidation.

- rail: assets

| [INDEX] | [SURFACE]                                                                 | [OWNER]     | [RAIL]                                        |
| :-----: | :------------------------------------------------------------------------ | :---------- | :-------------------------------------------- |
|  [01]   | `Load(string path, Uri? baseUri, SvgParameters?)`                         | `SvgSource` | path/URI load (internal `HttpClient`)         |
|  [02]   | `LoadFromStream(Stream, SvgParameters?)`                                  | `SvgSource` | stream load                                   |
|  [03]   | `LoadFromSvg(string svg, SvgParameters?)`                                 | `SvgSource` | in-memory SVG string load                     |
|  [04]   | `LoadFromSvgDocument(SvgDocument, SvgParameters?)`                        | `SvgSource` | pre-parsed document load                      |
|  [05]   | `Path`(init) / `Entities` / `Css` / `CurrentColor`                        | `SvgSource` | source inputs (CSS, entity table, recolor)    |
|  [06]   | `Svg : SKSvg?` / `Picture : SKPicture?` / `Parameters : SvgParameters?`   | `SvgSource` | engine handle / rendered picture / parameters |
|  [07]   | `Clone()` / `ReLoad(SvgParameters?)` / `RebuildFromModel()` / `Dispose()` | `SvgSource` | re-parameterize / re-record / teardown        |
|  [08]   | `Source`(`[Content]`) / `Css` / `CurrentCss` / `CurrentColor` / `Size`    | `SvgImage`  | image-source properties + measured size       |
|  [09]   | `Invalidated : EventHandler`                                              | `SvgImage`  | image-invalidation signal for binding hosts   |

## [04]-[IMPLEMENTATION_LAW]

[SVG_ASSET_LAW]:

- Package: `Svg.Controls.Skia.Avalonia` over `Svg.Skia`
- Owns: SVG and Android-VectorDrawable asset controls, source loading (path / stream / string / URI / pre-parsed document / `IServiceProvider` base-URI), retained scene rendering with incremental mutation, SMIL animation, pointer/access-key interaction, built-in viewer pan/zoom, text selection, and `SKPicture` output.
- Accept: SVG and VectorDrawable assets enter the same asset rail as raster and generated visual evidence; a vector asset retains `SvgSource` (`Svg`/`Picture`/`Parameters`) and exposes a live `SKSvg` engine.
- Reject: bitmap-only asset policy; treating the SVG as an opaque blob without `SvgSource`/`SKSvg` state; re-rendering the whole picture on a single-element change when `TryApplyRetainedSceneMutationAndRender` re-renders only the dirty region.

[RETAINED_MUTATION_LAW]:

- The retained scene graph is the incremental render owner: `TryEnsureRetainedSceneGraph` builds an `SvgSceneDocument` (source document + asset loader + element-addressable `SvgSceneNode` graph), `TryGetRetainedSceneNode(s)`/`…ById`/`TryGetRetainedSceneResource` resolve nodes by `SvgElement`, address key, or id, and `TryApplyRetainedSceneMutationAndRender(element|addressKey, changedAttributes, out SvgSceneMutationResult)` re-records only the affected subtree and returns the dirty-region receipt.
- A live model edit that drops the retained graph and reloads is the rejected form — the mutation API, not a full `Load`, owns single-element/attribute updates, and `SvgSceneMutationResult` is the receipt a viewport invalidation keys on.
- `Sync` is the engine's render-lock object: a producer that mutates the scene off the render thread takes `lock (skSvg.Sync)` so a concurrent `Draw`/`UpdateWith*` does not observe a half-applied mutation.

[STACKING]:

- Stacks ONTO `api-avalonia-skia` / `api-skiasharp`: `SvgCustomDrawOperation` and `SvgSourceCustomDrawOperation` implement `ICustomDrawOperation`. `Render(ImmediateDrawingContext)` resolves `ISkiaSharpApiLeaseFeature.Lease()` to an `ISkiaSharpApiLease`, reads `lease.SkCanvas`, and calls `SKSvg.Draw(canvas)`, so the SVG composites into Avalonia's Skia surface without a side bitmap.
- Native composition: the internal `SvgCompositionVisualScene` acquires the same lease through `CompositionCustomVisualHandler.OnRender` when `AnimationBackend` selects the native composition layer.
- Stacks ONTO the Avalonia image rail: `SvgImage : IImage` is an image-source the same way a `Bitmap` is, so an `Image.Source`/`ImageBrush` binds an `SvgImage` and its `Size` (the `SKPicture.CullRect`) drives layout; `SvgSource` is the `[Content]`/`TypeConverter` target so `{SvgImage Source=…}`/`Svg Path=…` XAML resolves a string or URI through `SvgSourceTypeConverter`.
- Stacks ONTO `api-asyncimageloader` / the asset pipeline: a URL `SvgSource.Load(path)` uses the engine's internal `HttpClient`, so remote SVG assets resolve on the same asset rail as async-loaded raster; the `IServiceProvider` `SvgSource` ctor resolves the Avalonia base URI and asset loader through `ServiceProviderExtensions`.

[RENDER_LAW]:

- Package: `Svg.Controls.Skia.Avalonia`
- Owns: SVG rendering for panels, companion windows, sidecars, diagnostics, support views, and downstream shells
- Accept: vector assets retain document, scene, animation, interaction, viewer-transform, selection, and rendered-picture state; the control's `Zoom`/`PanX`/`PanY` + `ZoomToPoint` drive the built-in `SKSvg` viewer model, and `AnimationBackend` selects the animation host (with `ActualAnimationBackend`/`AnimationBackendFallbackReason` reporting the resolved tier).
- Reject: opaque SVG blobs without renderable state; a hand-rolled pan/zoom or animation loop when the `SKSvg` viewer-transform and SMIL animation surfaces own them; drawing through a private bitmap instead of the `ISkiaSharpApiLease` canvas.
