# [RASM_RHINO_API_RHINOCOMMON_DISPLAY]

The `Rhino.Display` boundary owns everything between document geometry and pixels: the view/viewport/camera pose surface, the two frame-phase participation shapes (`DisplayConduit` subclassing and `DisplayPipeline` event subscription), the direct per-frame draw families and their push/pop render-state stacks, the retained `CustomDisplay` and `VisualAnalysisMode` overlays, the display-mode and display-attribute vocabulary, and the `ViewCapture` bitmap/SVG/print egress. The host resolves against the current WIP `RhinoCommon` assembly; every drawn primitive consumes a `Rhino.Geometry` carrier (`api-rhinocommon-geometry.md`) under the active viewport, and the pipeline's overlay phases are the surface the interactive getter feedback of `api-rhinocommon-commands.md` and the realtime framebuffer of `api-rhinocommon-render.md` both draw into.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon` '`Rhino.Display`'

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

`RhinoView` owns the `Modified`/`Create`/`Destroy`/`SetActive`/`Rename` events plus `Redraw` and `ShowToast`. `RhinoPageView` owns page lifecycle, detail views, page-view groups, space-change/properties events, and preview images; `RhinoViewport` owns camera pose/projection/frustum, dolly/zoom/gesture families, construction planes, and screen transforms.

`ViewportInfo` carries serializable frustum/projection state detached from a live viewport. `DetailView` is a model-in-page viewport with independent scale, clipping, and page/model transforms; `ClippingPlaneObject` is the document clipping plane driving per-viewport clipping.

| [INDEX] | [SYMBOL]                    | [KIND]             | [CAPABILITY]                   |
| :-----: | :-------------------------- | :----------------- | :----------------------------- |
|  [01]   | `RhinoView`                 | view host          | active-view identity           |
|  [02]   | `RhinoPageView`             | layout view        | page-detail management         |
|  [03]   | `RhinoViewport`             | camera surface     | live viewport control          |
|  [04]   | `ViewportInfo`              | camera snapshot    | detached camera state          |
|  [05]   | `ViewInfo`                  | named-view state   | focal-depth carriers           |
|  [06]   | `DisplayModeDescription`    | mode descriptor    | named pipeline settings roster |
|  [07]   | `DefinedViewportProjection` | projection enum    | standard projection presets    |
|  [08]   | `IsometricCamera`           | axonometric camera | `SetProjection` subject        |
|  [09]   | `DetailViewObject`          | page detail object | document detail owner          |
|  [10]   | `DetailView`                | page detail        | model-in-page viewport         |
|  [11]   | `ClippingPlaneObject`       | clip object        | clipping participation         |

[PUBLIC_TYPE_SCOPE]: pipeline, conduit, and draw-event carriers

- rail: host-boundary display

`DisplayPipeline` owns geometry/sprite/text/annotation draw families, render-state stacks, and pass state. `DisplayConduit` binds a viewport and overrides ordered draw phases.

| [INDEX] | [SYMBOL]                        | [KIND]            | [CAPABILITY]                 |
| :-----: | :------------------------------ | :---------------- | :--------------------------- |
|  [01]   | `DisplayPipeline`               | frame pipeline    | per-frame draw surface       |
|  [02]   | `DisplayConduit`                | frame participant | viewport-bound phase hook    |
|  [03]   | `DrawEventArgs`                 | phase args        | pipeline viewport frustum    |
|  [04]   | `DrawObjectEventArgs`           | per-object args   | object draw phase variant    |
|  [05]   | `CullObjectEventArgs`           | culling args      | per-object visibility handle |
|  [06]   | `CalculateBoundingBoxEventArgs` | bounds args       | zoom-extents contribution    |

[PUBLIC_TYPE_SCOPE]: draw appearance, effect, and attribute vocabulary

- rail: host-boundary display

`DisplayPen` constructs through `FromLinetype(Linetype, Color[, double])`; `CapStyle`/`JoinStyle`, `SetPattern(IEnumerable<float> dashesAndGaps)`, `HaloThickness`/`HaloColor`, `SetTaper`, `ThicknessSpace`, and `PatternAutoscale`/`PatternScale`/`PatternLengthInWorldUnits` own its stroke. `DisplayPipelineAttributes` carries `ShowEdges`/`ShowCreases`/`ShowSeams`/`ShowSilhouttes`/`ShowIntersections`/`ShowHiddenLines`/`ShowLighting`.

`DisplayMaterial` carries diffuse, specular, emission, and transparency channels. `DisplayBitmap` draws as a screen or world sprite; `LineCapStyle` carries `Round`/`Flat`/`Square`, and `LineJoinStyle` carries `Round`/`Miter`/`Bevel`.

| [INDEX] | [SYMBOL]                    | [KIND]           | [CAPABILITY]                      |
| :-----: | :-------------------------- | :--------------- | :-------------------------------- |
|  [01]   | `DisplayMaterial`           | shaded material  | shaded-family channels            |
|  [02]   | `DisplayPen`                | vector pen       | configurable draw stroke          |
|  [03]   | `DisplayBitmap`             | sprite texture   | GPU sprite texture                |
|  [04]   | `DisplayBitmapDrawList`     | sprite batch     | sorted `DrawSprites` point cloud  |
|  [05]   | `DisplayPipelineAttributes` | display settings | per-mode attribute set            |
|  [06]   | `IsoDrawEffect`             | banded shading   | directional colored iso bands     |
|  [07]   | `BlendMode`                 | draw enum        | alpha-blend vocabulary            |
|  [08]   | `LineCapStyle`              | draw enum        | pen-cap vocabulary                |
|  [09]   | `LineJoinStyle`             | draw enum        | pen-join vocabulary               |
|  [10]   | `CullFaceMode`              | draw enum        | state-stack face culling          |
|  [11]   | `PointStyle`                | draw enum        | `CustomDisplay` point markers     |
|  [12]   | `Text3d`                    | world text       | `CustomDisplay.AddText` primitive |

`IsoDrawEffect` carries `IsoDrawMode`, `Direction`, `Frequency`, and per-band color access.

[PUBLIC_TYPE_SCOPE]: retained overlays and capture

- rail: host-boundary display

`CustomDisplay` accumulates points, polygons, and text for its `IDisposable` document lifetime. `ViewCapture` selects `CaptureToBitmap`/`CaptureToSvg`/`SendToPrinter` egress from one settings object.

| [INDEX] | [SYMBOL]              | [KIND]           | [CAPABILITY]                   |
| :-----: | :-------------------- | :--------------- | :----------------------------- |
|  [01]   | `CustomDisplay`       | retained overlay | conduit-free retained draws    |
|  [02]   | `VisualAnalysisMode`  | analysis overlay | per-object false-color mesh    |
|  [03]   | `ViewCapture`         | capture facade   | settings-driven capture egress |
|  [04]   | `ViewCaptureSettings` | capture spec     | capture-layout mapping         |

`ViewCaptureSettings` owns media size, layout, margins, model scale, color mode, and view-area window mapping.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `DisplayConduit` — subclass and bind

- rail: host-boundary display

| [INDEX] | [SURFACE]                                               | [CALL_SHAPE] | [CAPABILITY]             |
| :-----: | :------------------------------------------------------ | :----------- | :----------------------- |
|  [01]   | `Bind(RhinoViewport viewport)`                          | binding      | viewport attachment      |
|  [02]   | `ExclusiveBind(RhinoViewport)`                          | binding      | exclusive draw seizure   |
|  [03]   | `UnbindAll()`                                           | binding      | complete detachment      |
|  [04]   | `SetSelectionFilter(bool, bool)`                        | filter       | selection-state scope    |
|  [05]   | `SetObjectIdFilter(IEnumerable<Guid>)`                  | filter       | fixed object-id scope    |
|  [06]   | `ObjectCulling(CullObjectEventArgs e)`                  | override     | pre-walk visibility      |
|  [07]   | `CalculateBoundingBox(CalculateBoundingBoxEventArgs e)` | override     | framing bounds           |
|  [08]   | `CalculateBoundingBoxZoomExtents(...)`                  | override     | zoom-extents bounds      |
|  [09]   | `PreDrawObjects(DrawEventArgs e)`                       | override     | pre-object-walk draw     |
|  [10]   | `PreDrawObject(DrawObjectEventArgs e)`                  | override     | single-object intercept  |
|  [11]   | `PostDrawObjects(DrawEventArgs e)`                      | override     | depth-tested opaque draw |
|  [12]   | `DrawForeground(DrawEventArgs e)`                       | override     | 2D foreground draw       |
|  [13]   | `DrawOverlay(DrawEventArgs e)`                          | override     | depth-free overlay       |

[ENTRYPOINT_SCOPE]: `DisplayPipeline` — events, state stacks, and draw families

- rail: host-boundary display

Each render-state push unwinds through its matched `Pop*` operation.

The static events admit frame-phase participation without a conduit subclass.

| [INDEX] | [SURFACE]                                                                      | [CALL_SHAPE]  | [CAPABILITY]                 |
| :-----: | :----------------------------------------------------------------------------- | :------------ | :--------------------------- |
|  [01]   | `ObjectCulling`                                                                | static event  | object visibility phase      |
|  [02]   | `InitFrameBuffer`                                                              | static event  | framebuffer initialization   |
|  [03]   | `PreDrawObjects`                                                               | static event  | pre-object phase             |
|  [04]   | `PreDrawTransparentObjects`                                                    | static event  | transparent pre-draw phase   |
|  [05]   | `PreDrawObject`                                                                | static event  | per-object pre-draw phase    |
|  [06]   | `PostDrawObject`                                                               | static event  | per-object post-draw phase   |
|  [07]   | `PostDrawObjects`                                                              | static event  | post-object phase            |
|  [08]   | `DrawForeground`                                                               | static event  | foreground phase             |
|  [09]   | `DrawOverlay`                                                                  | static event  | depth-free overlay phase     |
|  [10]   | `ViewportProjectionChanged`                                                    | static event  | projection change            |
|  [11]   | `DisplayModeChanged`                                                           | static event  | active-mode change           |
|  [12]   | `PushModelTransform(Transform)`                                                | state stack   | world-transform push         |
|  [13]   | `PopModelTransform()`                                                          | state stack   | world-transform pop          |
|  [14]   | `PushDepthTesting(bool)`                                                       | state stack   | depth-testing override       |
|  [15]   | `PushDepthWriting(bool)`                                                       | state stack   | depth-writing override       |
|  [16]   | `PushClipTesting(bool)`                                                        | state stack   | clip-testing override        |
|  [17]   | `PushCullFaceMode(CullFaceMode)`                                               | state stack   | cull-mode override           |
|  [18]   | `Push2dProjection()`                                                           | state stack   | screen-space projection push |
|  [19]   | `PopProjection()`                                                              | state stack   | projection pop               |
|  [20]   | `DrawMeshShaded(Mesh, DisplayMaterial)`                                        | mesh draw     | material-shaded mesh         |
|  [21]   | `DrawMeshShaded(Mesh, Color, IsoDrawEffect)`                                   | mesh draw     | banded colored mesh          |
|  [22]   | `DrawMeshFalseColors(Mesh)`                                                    | mesh draw     | vertex-color mesh            |
|  [23]   | `DrawSubDShaded(SubD, DisplayMaterial)`                                        | subd draw     | shaded SubD                  |
|  [24]   | `DrawSubDWires(SubD, Color, float)`                                            | subd draw     | wire SubD                    |
|  [25]   | `DrawBrepShaded(Brep, DisplayMaterial)`                                        | brep draw     | shaded brep                  |
|  [26]   | `DrawBrepWires(Brep, Color, int)`                                              | brep draw     | wire-density brep            |
|  [27]   | `DrawInstanceDefinitionShaded(InstanceDefinition, DisplayMaterial, Transform)` | block draw    | transformed shaded block     |
|  [28]   | `DrawClippingPlaneWires(ClippingPlaneSurface, Color)`                          | vector draw   | clipping-plane wires         |
|  [29]   | `DrawHatch(Hatch, Color, Color)`                                               | vector draw   | hatch outlines               |
|  [30]   | `DrawText(TextEntity, Color)`                                                  | text draw     | world text                   |
|  [31]   | `Draw2dText(string, Color, Point2d, bool, int, string)`                        | text draw     | screen text                  |
|  [32]   | `DrawAnnotation(AnnotationBase, RhinoObject, Color)`                           | text draw     | dimension-annotation draw    |
|  [33]   | `DrawSprite(DisplayBitmap, Point2d, float, Color)`                             | sprite draw   | sized screen sprite          |
|  [34]   | `DrawSprite(DisplayBitmap, Point2d, float, float)`                             | sprite draw   | dimensioned screen sprite    |
|  [35]   | `DrawSprite(DisplayBitmap, Point3d, float, Color, bool)`                       | sprite draw   | world sprite                 |
|  [36]   | `DrawSprites(DisplayBitmap, DisplayBitmapDrawList, float, bool)`               | sprite draw   | batched sprite cloud         |
|  [37]   | `DrawSurfaceDirectionIndicators(SurfaceDirectionIndicators)`                   | analysis draw | direction overlay            |
|  [38]   | `DrawCurvaturePreview(Brep, Color)`                                            | analysis draw | curvature overlay            |
|  [39]   | `DrawDraftAnglePreview(Mesh, Color)`                                           | analysis draw | draft-angle overlay          |
|  [40]   | `IsInViewCapture`                                                              | pass state    | capture-frame flag           |
|  [41]   | `IsPrinting`                                                                   | pass state    | print-frame flag             |
|  [42]   | `IsDynamicDisplay`                                                             | pass state    | dynamic-frame flag           |
|  [43]   | `RenderPass`                                                                   | pass state    | render-pass identity         |
|  [44]   | `NestLevel`                                                                    | pass state    | nesting depth                |
|  [45]   | `DpiScale`                                                                     | pass state    | display scale                |
|  [46]   | `FrameBuffer`                                                                  | pass state    | active framebuffer           |

[ENTRYPOINT_SCOPE]: view, viewport, and page-view

- rail: host-boundary display

`TryGetPaperLength` and `TryGetModelLength` expose the paper↔model length correspondence under the live detail scale.

| [INDEX] | [SURFACE]                                                                          | [CALL_SHAPE]  | [CAPABILITY]             |
| :-----: | :--------------------------------------------------------------------------------- | :------------ | :----------------------- |
|  [01]   | `RhinoViewport.SetProjection(IsometricCamera, string, bool)`                       | camera set    | axonometric projection   |
|  [02]   | `RhinoViewport.CameraAngle`                                                        | camera state  | mutable lens angle       |
|  [03]   | `RhinoPageView.AddDetailView(string, Point2d, Point2d, DefinedViewportProjection)` | detail        | model detail creation    |
|  [04]   | `RhinoPageView.GetDetailViews()`                                                   | detail        | detail enumeration       |
|  [05]   | `RhinoPageView.AddToPageViewGroup(int)`                                            | page group    | membership add           |
|  [06]   | `RhinoPageView.RemoveFromPageViewGroup(int)`                                       | page group    | membership removal       |
|  [07]   | `RhinoPageView.IsInPageViewGroup(int)`                                             | page group    | membership query         |
|  [08]   | `RhinoPageView.GetPageViewGroupList()`                                             | page group    | group-list query         |
|  [09]   | `RhinoPageView.GetPreviewImage(Size, bool)`                                        | preview       | sized page bitmap        |
|  [10]   | `RhinoPageView.PageName`                                                           | page state    | mutable page identity    |
|  [11]   | `RhinoPageView.PageNumber`                                                         | page state    | mutable page ordinal     |
|  [12]   | `RhinoPageView.PageWidth`                                                          | page state    | mutable page-unit width  |
|  [13]   | `RhinoPageView.PageHeight`                                                         | page state    | mutable page-unit height |
|  [14]   | `RhinoPageView.Duplicate(bool duplicatePageGeometry)`                              | page ops      | layout clone             |
|  [15]   | `RhinoPageView.SetPageAsActive()`                                                  | page ops      | page activation          |
|  [16]   | `RhinoPageView.SetActiveDetail(Guid)`                                              | page ops      | detail activation        |
|  [17]   | `RhinoPageView.ActiveDetailId`                                                     | page state    | active-detail identity   |
|  [18]   | `DetailViewObject.CommitChanges()`                                                 | detail commit | geometry-side flush      |
|  [19]   | `DetailViewObject.CommitViewportChanges()`                                         | detail commit | viewport-side flush      |
|  [20]   | `DetailViewObject.Attributes`                                                      | detail state  | object attributes        |
|  [21]   | `DetailViewObject.Id`                                                              | detail state  | object identity          |
|  [22]   | `DetailViewObject.TryGetPaperLength(double, out double)`                           | detail scale  | length conversion        |
|  [23]   | `DetailViewObject.TryGetModelLength(double, out double)`                           | detail scale  | length conversion        |
|  [24]   | `DetailView.IsParallelProjection`                                                  | detail state  | projection class         |
|  [25]   | `DetailView.IsProjectionLocked`                                                    | detail state  | mutable projection lock  |
|  [26]   | `DetailView.PageToModelRatio`                                                      | detail scale  | live scale ratio         |
|  [27]   | `DetailView.SetScale(double, LengthUnit, double, LengthUnit)`                      | detail scale  | detail scale write       |

[ENTRYPOINT_SCOPE]: display modes, retained overlays, and capture

- rail: host-boundary display

`ViewCaptureSettings.RasterMode` selects raster or vector egress.

| [INDEX] | [SURFACE]                                                                           | [CALL_SHAPE] | [CAPABILITY]              |
| :-----: | :---------------------------------------------------------------------------------- | :----------- | :------------------------ |
|  [01]   | `DisplayModeDescription.GetDisplayModes()`                                          | mode read    | mode enumeration          |
|  [02]   | `DisplayModeDescription.GetDisplayMode(Guid)`                                       | mode read    | identifier resolution     |
|  [03]   | `DisplayModeDescription.FindByName(string)`                                         | mode read    | name resolution           |
|  [04]   | `DisplayModeDescription.AddDisplayMode(DisplayModeDescription)`                     | mode write   | mode creation             |
|  [05]   | `DisplayModeDescription.UpdateDisplayMode(...)`                                     | mode write   | mode update               |
|  [06]   | `DisplayModeDescription.ImportFromFile(string, bool)`                               | mode write   | `.ini` import             |
|  [07]   | `DisplayModeDescription.ExportToFile(DisplayModeDescription, string)`               | mode write   | `.ini` export             |
|  [08]   | `IsoDrawEffect.GetBandColor(int)`                                                   | effect       | band-color read           |
|  [09]   | `IsoDrawEffect.SetBandColor(int, Color)`                                            | effect       | band-color write          |
|  [10]   | `new CustomDisplay(bool)`                                                           | retained     | overlay construction      |
|  [11]   | `CustomDisplay.Clear()`                                                             | retained     | overlay clearing          |
|  [12]   | `CustomDisplay.AddPoints(IEnumerable<Point3d>, Color, PointStyle, int)`             | retained     | point accumulation        |
|  [13]   | `CustomDisplay.AddPolygon(IEnumerable<Point3d>, Color, Color, bool, bool)`          | retained     | polygon accumulation      |
|  [14]   | `CustomDisplay.AddText(Text3d, Color)`                                              | retained     | text accumulation         |
|  [15]   | `CustomDisplay.Dispose()`                                                           | retained     | overlay retirement        |
|  [16]   | `VisualAnalysisMode.Register(Type)`                                                 | analysis     | mode registration         |
|  [17]   | `VisualAnalysisMode.Find(Guid)`                                                     | analysis     | mode resolution           |
|  [18]   | `VisualAnalysisMode.SetUpDisplayAttributes(RhinoObject, DisplayPipelineAttributes)` | analysis     | display attributes        |
|  [19]   | `VisualAnalysisMode.UpdateVertexColors(RhinoObject, Mesh[])`                        | analysis     | false-color vertex update |
|  [20]   | `VisualAnalysisMode.DrawMesh(RhinoObject, Mesh, DisplayPipeline)`                   | analysis     | analysis mesh draw        |
|  [21]   | `RhinoDirectionAnalysisModeId`                                                      | analysis id  | built-in direction mode   |
|  [22]   | `RhinoEndAnalysisModeId`                                                            | analysis id  | built-in end mode         |
|  [23]   | `ViewCapture.CaptureToBitmap(ViewCaptureSettings)`                                  | capture      | raster egress             |
|  [24]   | `ViewCapture.CaptureToSvg(ViewCaptureSettings)`                                     | capture      | vector egress             |
|  [25]   | `ViewCapture.SendToPrinter(string, ViewCaptureSettings[], int)`                     | capture      | printer egress copy count |
|  [26]   | `new ViewCaptureSettings(RhinoView, Size, double)`                                  | capture spec | view settings             |
|  [27]   | `new ViewCaptureSettings(RhinoPageView, double)`                                    | capture spec | page settings             |
|  [28]   | `ViewCaptureSettings.CreatePreviewSettings(Size)`                                   | capture spec | preview-scaled derivation |
|  [29]   | `ViewCaptureSettings.SetViewport(RhinoViewport)`                                    | capture spec | viewport binding          |
|  [30]   | `ViewCaptureSettings.SetLayout(Size, Rectangle)`                                    | capture spec | page layout               |
|  [31]   | `ViewCaptureSettings.SetModelScaleToValue(double)`                                  | capture spec | fixed model scale         |
|  [32]   | `ViewCaptureSettings.SetModelScaleToFit(bool)`                                      | capture spec | fit model scale           |
|  [33]   | `ViewCaptureSettings.SetWindowRect(Point2d, Point2d)`                               | capture spec | screen window             |
|  [34]   | `ViewCaptureSettings.SetWindowRect(Point3d, Point3d)`                               | capture spec | world window              |
|  [35]   | `ViewCaptureSettings.RasterMode`                                                    | capture spec | egress mode               |
|  [36]   | `ViewCaptureSettings.OutputColor`                                                   | capture spec | color mode                |
|  [37]   | `ViewCaptureSettings.OffsetAnchor`                                                  | capture spec | offset anchor             |
|  [38]   | `ViewCaptureSettings.DrawGrid`                                                      | capture flag | grid draw                 |
|  [39]   | `ViewCaptureSettings.DrawAxis`                                                      | capture flag | axis draw                 |
|  [40]   | `ViewCaptureSettings.DrawBackground`                                                | capture flag | background draw           |
|  [41]   | `ViewCaptureSettings.DrawBackgroundBitmap`                                          | capture flag | bitmap draw               |
|  [42]   | `ViewCaptureSettings.DrawWallpaper`                                                 | capture flag | wallpaper draw            |
|  [43]   | `ViewCaptureSettings.DrawLockedObjects`                                             | capture flag | locked-object draw        |
|  [44]   | `ViewCaptureSettings.DrawSelectedObjectsOnly`                                       | capture flag | selected-object filter    |
|  [45]   | `ViewCaptureSettings.DrawClippingPlanes`                                            | capture flag | clipping-plane draw       |
|  [46]   | `ViewCaptureSettings.DrawLights`                                                    | capture flag | light draw                |
|  [47]   | `ViewCaptureSettings.DrawMargins`                                                   | capture flag | margin draw               |
|  [48]   | `ViewCaptureSettings.HeaderText`                                                    | capture spec | page header               |
|  [49]   | `ViewCaptureSettings.FooterText`                                                    | capture spec | page footer               |
|  [50]   | `ViewCaptureSettings.UsePrintWidths`                                                | capture spec | print-width selection     |
|  [51]   | `ViewCaptureSettings.WireThicknessScale`                                            | capture spec | wire-width scale          |
|  [52]   | `ViewCaptureSettings.PointSizeMillimeters`                                          | capture spec | point size                |
|  [53]   | `ViewCaptureSettings.ArrowheadSizeMillimeters`                                      | capture spec | arrowhead size            |
|  [54]   | `ViewCaptureSettings.TextDotPointSize`                                              | capture spec | text-dot size             |
|  [55]   | `ViewCaptureSettings.DefaultPrintWidthMillimeters`                                  | capture spec | default print width       |

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
