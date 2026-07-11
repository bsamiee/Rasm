# [RASM_RHINO_API_RHINOCOMMON_DISPLAY]

The `Rhino.Display` boundary owns everything between document geometry and pixels: the view/viewport/camera pose surface, the two frame-phase participation shapes (`DisplayConduit` subclassing and `DisplayPipeline` event subscription), the direct per-frame draw families and their push/pop render-state stacks, the retained `CustomDisplay` and `VisualAnalysisMode` overlays, the display-mode and display-attribute vocabulary, and the `ViewCapture` bitmap/SVG/print egress. The host resolves against the current WIP `RhinoCommon` assembly; every drawn primitive consumes a `Rhino.Geometry` carrier (`api-rhinocommon-geometry.md`) under the active viewport, and the pipeline's overlay phases are the surface the interactive getter feedback of `api-rhinocommon-commands.md` and the realtime framebuffer of `api-rhinocommon-render.md` both draw into.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon` (`Rhino.Display`)
- package: `RhinoCommon`
- license: proprietary McNeel SDK (host-provided, not centrally pinned)
- assembly: `RhinoCommon.dll` (current WIP host framework, resolved from the installed Rhino application)
- namespace: `Rhino.Display` (view, viewport, pipeline, conduit, capture, display material/mode/attributes)
- namespace: `Rhino.DocObjects` (`DetailViewObject`, `ClippingPlaneObject` display-adjacent objects)
- asset: host-resolved managed reference; the boundary composes it, the manifest never pins it
- rail: host-boundary display

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: view, viewport, and camera pose
- rail: host-boundary display

| [INDEX] | [SYMBOL]                          | [KIND]             | [CAPABILITY]                                                                                           |
| :-----: | :-------------------------------- | :----------------- | :----------------------------------------------------------------------------------------------------- |
|  [01]   | `RhinoView`                       | view host          | active-view identity, `Modified`/`Create`/`Destroy`/`SetActive`/`Rename` events, `Redraw`, `ShowToast` |
|  [02]   | `RhinoPageView`                   | layout view        | page lifecycle, detail views, page-view groups, space-change/properties events, preview image          |
|  [03]   | `RhinoViewport`                   | camera surface     | camera pose/projection/frustum, dolly/zoom/gesture families, construction-plane and screen transforms  |
|  [04]   | `ViewportInfo`                    | camera snapshot    | serializable frustum/projection state detached from a live viewport                                    |
|  [05]   | `ViewInfo`                        | named-view state   | stored view with focal-blur and depth-of-field carriers                                                |
|  [06]   | `DisplayModeDescription`          | mode descriptor    | a named display mode with pipeline settings; the mode table roster                                     |
|  [07]   | `DefinedViewportProjection`       | projection enum    | the standard viewport projection presets                                                               |
|  [08]   | `IsometricCamera`                 | axonometric camera | isometric/axonometric projection subject fed to `SetProjection`                                        |
|  [09]   | `DetailViewObject` / `DetailView` | page detail        | model-in-page viewport with its own scale, clipping, and page/model transforms                         |
|  [10]   | `ClippingPlaneObject`             | clip object        | a document clipping plane whose participation drives per-viewport clipping                             |

[PUBLIC_TYPE_SCOPE]: pipeline, conduit, and draw-event carriers
- rail: host-boundary display

| [INDEX] | [SYMBOL]                        | [KIND]            | [CAPABILITY]                                                                                   |
| :-----: | :------------------------------ | :---------------- | :--------------------------------------------------------------------------------------------- |
|  [01]   | `DisplayPipeline`               | frame pipeline    | the per-frame draw surface: geometry/sprite/text/annotation families, state stacks, pass state |
|  [02]   | `DisplayConduit`                | frame participant | the subclassing hook — bind to a viewport and override the ordered draw phases                 |
|  [03]   | `DrawEventArgs`                 | phase args        | the pipeline handle, viewport, and frustum passed to each draw phase                           |
|  [04]   | `DrawObjectEventArgs`           | per-object args   | the single-object variant carried by `PreDrawObject`/`PostDrawObject`                          |
|  [05]   | `CullObjectEventArgs`           | culling args      | the `ObjectCulling` phase handle for per-object visibility decisions                           |
|  [06]   | `CalculateBoundingBoxEventArgs` | bounds args       | the conduit bounds contribution that keeps geometry inside zoom-extents                        |

[PUBLIC_TYPE_SCOPE]: draw appearance, effect, and attribute vocabulary
- rail: host-boundary display

| [INDEX] | [SYMBOL]                                       | [KIND]           | [CAPABILITY]                                                                                                                                                                                                                                |
| :-----: | :--------------------------------------------- | :--------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `DisplayMaterial`                              | shaded material  | the per-draw material (diffuse/specular/emission/transparency) for shaded families                                                                                                                                                          |
|  [02]   | `DisplayPen`                                   | vector pen       | `FromLinetype(Linetype, Color[, double])`, `CapStyle`/`JoinStyle`, `SetPattern(IEnumerable<float> dashesAndGaps)`, `HaloThickness`/`HaloColor`, `SetTaper`, `ThicknessSpace`, `PatternAutoscale`/`PatternScale`/`PatternLengthInWorldUnits` |
|  [03]   | `DisplayBitmap`                                | sprite texture   | a GPU-uploaded bitmap drawn as a screen or world sprite                                                                                                                                                                                     |
|  [04]   | `DisplayBitmapDrawList`                        | sprite batch     | a sorted point-cloud sprite batch for one `DrawSprites` call                                                                                                                                                                                |
|  [05]   | `DisplayPipelineAttributes`                    | display settings | the per-mode attribute set: `ShowEdges`/`ShowCreases`/`ShowSeams`/`ShowSilhouttes`/`ShowIntersections`/`ShowHiddenLines`/`ShowLighting`                                                                                                     |
|  [06]   | `IsoDrawEffect`                                | banded shading   | contour/iso banding: `IsoDrawMode`, `Direction`, `Frequency`, per-band color get/set                                                                                                                                                        |
|  [07]   | `BlendMode` / `LineCapStyle` / `LineJoinStyle` | draw enums       | alpha-blend vocabulary plus the `Rhino.DocObjects` cap (`Round`/`Flat`/`Square`) and join (`Round`/`Miter`/`Bevel`) enums for pen draws                                                                                                     |
|  [08]   | `CullFaceMode` / `PointStyle`                  | draw enums       | face-culling mode for the state stack and point-marker style for `CustomDisplay`                                                                                                                                                            |
|  [09]   | `Text3d`                                       | world text       | a placed 3D text primitive for `CustomDisplay.AddText`                                                                                                                                                                                      |

[PUBLIC_TYPE_SCOPE]: retained overlays and capture
- rail: host-boundary display

| [INDEX] | [SYMBOL]              | [KIND]           | [CAPABILITY]                                                                                  |
| :-----: | :-------------------- | :--------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `CustomDisplay`       | retained overlay | a `IDisposable` document-lifetime overlay accumulating points/polygons/text without a conduit |
|  [02]   | `VisualAnalysisMode`  | analysis overlay | a registered false-color analysis mode driving vertex colors and mesh draws per object        |
|  [03]   | `ViewCapture`         | capture facade   | `CaptureToBitmap`/`CaptureToSvg`/`SendToPrinter` egress from a settings object                |
|  [04]   | `ViewCaptureSettings` | capture spec     | media size, layout, margins, model scale, color mode, and view-area window mapping            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `DisplayConduit` — subclass and bind
- rail: host-boundary display

| [INDEX] | [SURFACE]                                                                                        | [CALL_SHAPE] | [CAPABILITY]                                                                      |
| :-----: | :----------------------------------------------------------------------------------------------- | :----------- | :-------------------------------------------------------------------------------- |
|  [01]   | `Bind(RhinoViewport viewport)` / `ExclusiveBind(RhinoViewport)` / `UnbindAll()`                  | binding      | attach the conduit to one viewport, seize exclusive draw, or detach every binding |
|  [02]   | `SetSelectionFilter(bool, bool)` / `SetObjectIdFilter(IEnumerable<Guid>)`                        | filter       | scope the conduit to selection state or a fixed object-id set                     |
|  [03]   | `ObjectCulling(CullObjectEventArgs e)`                                                           | override     | per-object visibility decision before the draw walk                               |
|  [04]   | `CalculateBoundingBox(CalculateBoundingBoxEventArgs e)` / `CalculateBoundingBoxZoomExtents(...)` | override     | contribute conduit geometry bounds to framing and zoom-extents                    |
|  [05]   | `PreDrawObjects(DrawEventArgs e)` / `PreDrawObject(DrawObjectEventArgs e)`                       | override     | draw before the object walk or intercept a single object's draw                   |
|  [06]   | `PostDrawObjects(DrawEventArgs e)`                                                               | override     | draw after opaque geometry, inside depth                                          |
|  [07]   | `DrawForeground(DrawEventArgs e)` / `DrawOverlay(DrawEventArgs e)`                               | override     | 2D foreground and depth-free overlay passes                                       |

[ENTRYPOINT_SCOPE]: `DisplayPipeline` — events, state stacks, and draw families
- rail: host-boundary display

| [INDEX] | [SURFACE]                                                                                                                                                                                                                             | [CALL_SHAPE]  | [CAPABILITY]                                                                        |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `ObjectCulling` / `InitFrameBuffer` / `PreDrawObjects` / `PreDrawTransparentObjects` / `PreDrawObject` / `PostDrawObject` / `PostDrawObjects` / `DrawForeground` / `DrawOverlay`                                                      | static events | the frame-phase subscription shape — participate without subclassing a conduit      |
|  [02]   | `ViewportProjectionChanged` / `DisplayModeChanged`                                                                                                                                                                                    | static events | camera-projection and active-mode change notifications                              |
|  [03]   | `PushModelTransform(Transform)` / `PopModelTransform()`                                                                                                                                                                               | state stack   | push/pop a world transform around a draw block                                      |
|  [04]   | `PushDepthTesting(bool)` / `PushDepthWriting(bool)` / `PushClipTesting(bool)` / `PushCullFaceMode(CullFaceMode)` (+ `Pop*`)                                                                                                           | state stack   | scoped depth, clip, and cull-mode overrides                                         |
|  [05]   | `Push2dProjection()` / `PopProjection()`                                                                                                                                                                                              | state stack   | enter/exit a screen-space 2D projection for overlay draws                           |
|  [06]   | `DrawMeshShaded(Mesh, DisplayMaterial)` / `DrawMeshShaded(Mesh, Color, IsoDrawEffect)` / `DrawMeshFalseColors(Mesh)`                                                                                                                  | mesh draw     | shaded, banded, and vertex-color mesh draws                                         |
|  [07]   | `DrawSubDShaded(SubD, DisplayMaterial)` / `DrawSubDWires(SubD, Color, float)`                                                                                                                                                         | subd draw     | shaded and wire SubD draws                                                          |
|  [08]   | `DrawBrepShaded(Brep, DisplayMaterial)` / `DrawBrepWires(Brep, Color, int)`                                                                                                                                                           | brep draw     | shaded and wire-density brep draws                                                  |
|  [09]   | `DrawInstanceDefinitionShaded(InstanceDefinition, DisplayMaterial, Transform)`                                                                                                                                                        | block draw    | a placed block definition drawn shaded under a transform                            |
|  [10]   | `DrawClippingPlaneWires(ClippingPlaneSurface, Color)` / `DrawHatch(Hatch, Color, Color)`                                                                                                                                              | vector draw   | clipping-plane and hatch outline draws                                              |
|  [11]   | `DrawText(TextEntity, Color)` / `Draw2dText(string, Color, Point2d, bool, int, string)` / `DrawAnnotation(AnnotationBase, RhinoObject, Color)`                                                                                        | text draw     | world text, screen text, and dimension/annotation draws                             |
|  [12]   | `DrawSprite(DisplayBitmap, Point2d, float, Color)` / `DrawSprite(DisplayBitmap, Point2d, float, float)` / `DrawSprite(DisplayBitmap, Point3d, float, Color, bool)` / `DrawSprites(DisplayBitmap, DisplayBitmapDrawList, float, bool)` | sprite draw   | screen sprite by size or width/height, world sprite, and batched sprite-cloud draws |
|  [13]   | `DrawSurfaceDirectionIndicators(SurfaceDirectionIndicators)` / `DrawCurvaturePreview(Brep, Color)` / `DrawDraftAnglePreview(Mesh, Color)`                                                                                             | analysis draw | direction, curvature, and draft-angle preview overlays                              |
|  [14]   | `IsInViewCapture` / `IsPrinting` / `IsDynamicDisplay` / `RenderPass` / `NestLevel` / `DpiScale` / `FrameBuffer`                                                                                                                       | pass state    | the current-frame context read by conduit and analysis draws                        |

[ENTRYPOINT_SCOPE]: view, viewport, and page-view
- rail: host-boundary display

| [INDEX] | [SURFACE]                                                                                                                                    | [CALL_SHAPE]    | [CAPABILITY]                                                       |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------------- |
|  [01]   | `RhinoViewport.SetProjection(IsometricCamera, string, bool)` / `RhinoViewport.CameraAngle`                                                   | camera set      | drive an axonometric projection and read/set the camera lens angle |
|  [02]   | `RhinoPageView.AddDetailView(string, Point2d, Point2d, DefinedViewportProjection)` / `GetDetailViews()`                                      | detail          | add and enumerate model detail viewports on a layout page          |
|  [03]   | `RhinoPageView.AddToPageViewGroup(int)` / `RemoveFromPageViewGroup(int)` / `IsInPageViewGroup(int)` / `GetPageViewGroupList()`               | page group      | page-view group membership management                              |
|  [04]   | `RhinoPageView.GetPreviewImage(Size, bool)`                                                                                                  | preview         | render a page preview bitmap at a target size                      |
|  [05]   | `RhinoPageView.PageName` / `PageNumber` / `PageWidth` / `PageHeight` (get/set)                                                               | page state      | page identity, ordinal, and page-unit dimensions                   |
|  [06]   | `RhinoPageView.Duplicate(bool duplicatePageGeometry)` / `SetPageAsActive()` / `SetActiveDetail(Guid)` / `ActiveDetailId`                     | page ops        | clone a layout, activate the page or one detail                    |
|  [07]   | `DetailViewObject.CommitChanges()` / `CommitViewportChanges()` / `Attributes` / `Id`                                                         | detail commit   | flush geometry-side and viewport-side detail edits                 |
|  [08]   | `DetailViewObject.TryGetPaperLength(double, out double)` / `TryGetModelLength(double, out double)`                                           | detail scale    | the paper↔model length correspondence under the live detail scale  |
|  [09]   | `DetailView.IsParallelProjection` / `IsProjectionLocked` (get/set) / `PageToModelRatio` / `SetScale(double, LengthUnit, double, LengthUnit)` | detail geometry | projection class, lock, live ratio, and the scale write            |

[ENTRYPOINT_SCOPE]: display modes, retained overlays, and capture
- rail: host-boundary display

| [INDEX] | [SURFACE]                                                                                                                                                                                                                                                         | [CALL_SHAPE] | [CAPABILITY]                                                                                                                 |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------- | :--------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `DisplayModeDescription.GetDisplayModes()` / `GetDisplayMode(Guid)` / `FindByName(string)`                                                                                                                                                                        | mode read    | enumerate and resolve display modes                                                                                          |
|  [02]   | `DisplayModeDescription.AddDisplayMode(DisplayModeDescription)` / `UpdateDisplayMode(...)` / `ImportFromFile(string, bool)` / `ExportToFile(DisplayModeDescription, string)`                                                                                      | mode write   | create, update, and round-trip display modes to `.ini`                                                                       |
|  [03]   | `IsoDrawEffect.GetBandColor(int)` / `SetBandColor(int, Color)`                                                                                                                                                                                                    | effect       | read/set banded iso-shading colors                                                                                           |
|  [04]   | `new CustomDisplay(bool)` / `Clear()` / `AddPoints(IEnumerable<Point3d>, Color, PointStyle, int)` / `AddPolygon(IEnumerable<Point3d>, Color, Color, bool, bool)` / `AddText(Text3d, Color)`                                                                       | retained     | build a conduit-free document-lifetime overlay; `Dispose` retires it                                                         |
|  [05]   | `VisualAnalysisMode.Register(Type)` / `Find(Guid)` / `SetUpDisplayAttributes(RhinoObject, DisplayPipelineAttributes)` / `UpdateVertexColors(RhinoObject, Mesh[])` / `DrawMesh(RhinoObject, Mesh, DisplayPipeline)`                                                | analysis     | register and drive a false-color analysis mode; `RhinoDirectionAnalysisModeId`/`RhinoEndAnalysisModeId` are the built-in ids |
|  [06]   | `ViewCapture.CaptureToBitmap(ViewCaptureSettings)` / `CaptureToSvg(ViewCaptureSettings)` / `SendToPrinter(string, ViewCaptureSettings[], int)`                                                                                                                    | capture      | raster, vector, and printer-addressed print egress with a copy count                                                         |
|  [07]   | `new ViewCaptureSettings(RhinoView, Size, double)` / `new ViewCaptureSettings(RhinoPageView, double)` / instance `CreatePreviewSettings(Size)`                                                                                                                    | capture spec | construct capture settings from a view or page; derive a preview-scaled settings instance                                    |
|  [08]   | `ViewCaptureSettings.SetViewport(RhinoViewport)` / `SetLayout(Size, Rectangle)` / `SetModelScaleToValue(double)` / `SetModelScaleToFit(bool)` / `SetWindowRect(Point2d, Point2d)` / `SetWindowRect(Point3d, Point3d)`                                             | capture spec | bind viewport, page layout, model scale, and captured window rect                                                            |
|  [09]   | `ViewCaptureSettings.RasterMode` / `OutputColor` / `OffsetAnchor` / `DrawGrid` / `DrawAxis` / `DrawBackground` / `DrawBackgroundBitmap` / `DrawWallpaper` / `DrawLockedObjects` / `DrawSelectedObjectsOnly` / `DrawClippingPlanes` / `DrawLights` / `DrawMargins` | capture spec | raster/vector path, color mode, offset anchor, and the per-capture draw-flag family                                          |
|  [10]   | `ViewCaptureSettings.HeaderText` / `FooterText` / `UsePrintWidths` / `WireThicknessScale` / `PointSizeMillimeters` / `ArrowheadSizeMillimeters` / `TextDotPointSize` / `DefaultPrintWidthMillimeters`                                                             | capture spec | page banner text and the print-fidelity width/size family                                                                    |

## [04]-[IMPLEMENTATION_LAW]

[DISPLAY_TOPOLOGY]:
- Frame participation has two shapes with one phase order: subclass `DisplayConduit` and override the phases, or subscribe the mirror `DisplayPipeline` static events — the boundary picks subclassing when it owns per-instance state (filters, cached geometry) and events for a stateless tap. The ordered phases (`ObjectCulling` → `CalculateBoundingBox` → `PreDrawObjects`/`PreDrawObject` → object walk → `PostDrawObjects` → `DrawForeground` → `DrawOverlay`) are the same for both.
- Direct draws run only inside a phase against the phase's `DisplayPipeline`; a draw family consumes a `Rhino.Geometry` carrier plus a `DisplayMaterial`/`Color`/`DisplayPen`, and any transform or depth/cull override wraps the draw in a matched `Push*`/`Pop*` pair so the pipeline state unwinds exactly.
- A conduit-free overlay is `CustomDisplay` (document-lifetime, `IDisposable`); a per-object false-color surface is a registered `VisualAnalysisMode`; a per-frame interactive overlay is a bound conduit. The three never overlap — retained accumulation, registered analysis, and live participation are distinct owners.
- Pass state (`IsInViewCapture`, `IsPrinting`, `IsDynamicDisplay`, `RenderPass`, `NestLevel`, `DpiScale`) is read, never assumed: a draw that differs between interactive, capture, and print frames branches on the flags rather than duplicating conduits.

[CAPTURE_TOPOLOGY]:
- `ViewCapture` is settings-driven: one `ViewCaptureSettings` fixes media size, layout, model scale, color mode, and captured window, then `CaptureToBitmap`/`CaptureToSvg`/`SendToPrinter` selects the egress — the same settings object drives every target, so raster/vector/print never fork the capture configuration.
- Page-view capture composes `RhinoPageView` detail views and page-view groups; PDF page egress is the `FilePdf.AddPage(ViewCaptureSettings)` seam of `api-rhinocommon-fileio.md`, which consumes the identical settings object.

[STACKING]:
- `api-languageext.md`(`../../.api/api-languageext.md`): every host call that can fail or return null is trapped onto the rail — `Try.lift(() => DisplayModeDescription.FindByName(name)).Run()` and `Optional(ViewCapture.CaptureToBitmap(settings)).ToFin(error)` are the boundary spellings; a captured bitmap, a resolved mode, or a bound conduit crosses into domain code as `Fin<A>`, never as a nullable host handle.
- `api-thinktecture-runtime-extensions.md`(`../../.api/api-thinktecture-runtime-extensions.md`): the host draw enums (`BlendMode`, `CullFaceMode`, `LineCapStyle`, `LineJoinStyle`, `PointStyle`, `IsoDrawMode`) and mode/attribute selectors are mapped at the edge to `[SmartEnum]` owners, and a display-mode or analysis-mode `Guid` is a `[ValueObject<Guid>]` — the domain composes the bounded owner, the host enum lives only in the adapter.
- `api-rhinocommon-geometry.md`: every draw family consumes a `Rhino.Geometry` carrier (`Mesh`/`Brep`/`SubD`/`Hatch`/`TextEntity`/`ClippingPlaneSurface`); the pipeline is the sink, the geometry catalog the source.
- `api-macos-native.md`: sprite-cloud and dynamic-display animation pace off the host `CADisplayLink` frame clock rather than a wall timer, and perceptual color blending of `Color4f`/`IsoDrawEffect` band colors composes the Rasm kernel color rail, never a host-side channel-average.

[LOCAL_ADMISSION]:
- The `Rhino.Display` types are host handles trapped and mapped at the boundary; a `DisplayPipeline`, `RhinoViewport`, or `ViewCaptureSettings` never appears in a domain signature — the domain sees a `Fin<A>`, a bounded owner, or a canonical shape.
- A conduit or `CustomDisplay` is the single retained owner for its overlay concern; a second parallel conduit drawing the same overlay is the collapsed form.

[RAIL_LAW]:
- Package: `RhinoCommon` (`Rhino.Display`)
- Owns: view/viewport/camera pose, the two frame-participation shapes and their draw families, render-state stacks, retained `CustomDisplay`/`VisualAnalysisMode` overlays, display-mode and display-attribute vocabulary, and `ViewCapture` egress
- Accept: a bound conduit or subscribed pipeline event drawing geometry carriers under matched state pushes; a settings-driven capture; a registered analysis mode; host handles trapped through `Try.lift(...).Run()` and enums mapped to bounded owners at the edge
- Reject: a host draw call outside a frame phase, an unmatched `Push*`/`Pop*` leaving pipeline state dirty, a `DisplayPipeline`/`RhinoViewport`/`DisplayModeDescription` escaping into a domain signature, a hand-rolled overlay where `CustomDisplay` fits, a wall-clock sprite animation where the frame clock is available, and a re-derived color blend where the kernel color rail is composed
