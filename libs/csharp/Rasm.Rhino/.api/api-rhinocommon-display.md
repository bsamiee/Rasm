# [RASM_RHINO_API_RHINOCOMMON_DISPLAY]

`Rhino.Display` owns everything between document geometry and pixels: the view/viewport/camera pose surface, the two frame-phase participation shapes (`DisplayConduit` subclassing and `DisplayPipeline` event subscription), the direct per-frame draw families and their push/pop render-state stacks, the retained `CustomDisplay` and `VisualAnalysisMode` overlays, the display-mode and display-attribute vocabulary, and the `ViewCapture` bitmap/SVG/print egress. Every symbol resolves against the current WIP `RhinoCommon` assembly, and every drawn primitive consumes a `Rhino.Geometry` carrier (`api-rhinocommon-geometry.md`) under the active viewport, and the pipeline's overlay phases are the surface the interactive getter feedback of `api-rhinocommon-commands.md` and the realtime framebuffer of `api-rhinocommon-render.md` both draw into.

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

`DrawEventArgs` exposes `Viewport : RhinoViewport`, `RhinoDoc : RhinoDoc`, and `Display : DisplayPipeline`. `DrawObjectEventArgs.RhinoObject : RhinoObject` names the object entering `PreDrawObject`/`PostDrawObject`; `CullObjectEventArgs.RhinoObject : RhinoObject` and `RhinoObjectSerialNumber : uint` name the object under the `ObjectCulling` decision. `DrawObjectEventArgs.DrawObject : bool` and `CullObjectEventArgs.CullObject : bool` are per-phase draw-suppression flags an observer never mutates.

[PUBLIC_TYPE_SCOPE]: draw appearance, effect, and attribute vocabulary
- rail: host-boundary display

`DisplayPen` constructs through `FromLinetype(Linetype, Color[, double])`; `CapStyle`/`JoinStyle`, `SetPattern(IEnumerable<float> dashesAndGaps)` (eight entries maximum), `HaloThickness`/`HaloColor`, `SetTaper(float startThickness, float endThickness, Point2f taperPoint)`, `ThicknessSpace`, and `PatternAutoscale`/`PatternScale`/`PatternOffset`/`PatternBySegment`/`PatternLengthInWorldUnits` own its stroke. `DisplayPipelineAttributes` carries `ShowEdges`/`ShowCreases`/`ShowSeams`/`ShowSilhouttes`/`ShowIntersections`/`ShowHiddenLines`/`ShowLighting`.

`DisplayMaterial` carries diffuse, specular, emission, and transparency channels. `DisplayBitmap` draws as a screen or world sprite; `LineCapStyle` carries `Round`/`Flat`/`Square`, and `LineJoinStyle` carries `Round`/`Miter`/`Bevel`.

| [INDEX] | [SYMBOL]                                                             | [KIND]           | [CAPABILITY]                      |
| :-----: | :------------------------------------------------------------------- | :--------------- | :-------------------------------- |
|  [01]   | `DisplayMaterial`                                                    | shaded material  | shaded-family channels            |
|  [02]   | `DisplayPen`                                                         | vector pen       | configurable draw stroke          |
|  [03]   | `DisplayBitmap`                                                      | sprite texture   | GPU sprite texture                |
|  [04]   | `DisplayBitmapDrawList`                                              | sprite batch     | sorted `DrawSprites` point cloud  |
|  [05]   | `DisplayPipelineAttributes`                                          | display settings | per-mode attribute set            |
|  [06]   | `IsoDrawEffect`                                                      | banded shading   | directional colored iso bands     |
|  [07]   | `BlendMode`                                                          | draw enum        | alpha-blend vocabulary            |
|  [08]   | `LineCapStyle`                                                       | draw enum        | pen-cap vocabulary                |
|  [09]   | `LineJoinStyle`                                                      | draw enum        | pen-join vocabulary               |
|  [10]   | `CullFaceMode`                                                       | draw enum        | state-stack face culling          |
|  [11]   | `PointStyle`                                                         | draw enum        | `CustomDisplay` point markers     |
|  [12]   | `CullFaceMode.DrawFrontAndBack` / `DrawFrontFaces` / `DrawBackFaces` | enum rows        | complete cull policy              |
|  [13]   | `PointStyle.Simple` through `PointStyle.None`                        | enum rows        | complete marker policy            |
|  [14]   | `BlendMode.Zero` through `BlendMode.SourceAlphaSaturate`             | enum rows        | complete blend-factor policy      |
|  [15]   | `Text3d`                                                             | world text       | `CustomDisplay.AddText` primitive |

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
|  [05]   | `ZBufferCapture`      | depth capture    | per-pixel z-buffer field       |

`ViewCaptureSettings` owns media size, layout, margins, model scale, color mode, and view-area window mapping. `ZBufferCapture` is the `IDisposable` per-viewport depth-buffer capsule — channel toggles, per-pixel depth reads, screen-to-world recovery, and a grayscale rendering.

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

`DisplayConduit.GeometryFilter : ObjectType` scopes the per-object phases to a geometry-type mask; `DisplayConduit.SpaceFilter : ActiveSpace` scopes participation to model or page space.

[ENTRYPOINT_SCOPE]: `DisplayPipeline` — events, state stacks, and draw families
- rail: host-boundary display

Each render-state push unwinds through its matched `Pop*` operation.

Static events admit frame-phase participation without a conduit subclass.

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
|  [47]   | `DrawCurve(Curve, DisplayPen)`                                                 | vector draw   | pen-stroked curve            |
|  [48]   | `DisplayBitmapDrawList.SetPoints(IEnumerable<Point3d>[, IEnumerable<Color>])`  | sprite draw   | cloud points, per-point tint |

[ENTRYPOINT_SCOPE]: view, viewport, and page-view
- rail: host-boundary display

`TryGetPaperLength` and `TryGetModelLength` expose the paper↔model length correspondence under the live detail scale. `DetailViewObject.GetFormattedScale(ScaleFormat, out string)` returns `true` with the human-readable scale string; the nested `DetailViewObject.ScaleFormat` enum selects the rendering, `OneToModelLength` spelling the `1:N` model ratio.

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
|  [28]   | `DetailViewObject.GetFormattedScale(ScaleFormat, out string) : bool`               | detail scale  | formatted scale text     |

[ENTRYPOINT_SCOPE]: camera pose, frustum, depth, and gesture families
- rail: host-boundary display

Bare members below live on `RhinoViewport`; `ViewportInfo`, `ViewInfo`, and `Rhino.ApplicationSettings.ViewSettings` rows carry their owner. `PopViewProjection`/`NextViewProjection`/`PreviousViewProjection`/`PopConstructionPlane` return `false` at the stack boundary or when the popped projection equals the current one — a benign no-op, never an error. `ViewportInfo.GetXform` returns `Transform.Unset` on failure where `RhinoViewport.GetTransform` returns `Identity`, so `GetXform` is the rail-detectable transform read. `ChangeToPerspectiveProjection` orders `(targetDistance, symmetricFrustum, lensLength)`; `ChangeToTwoPointPerspectiveProjection` orders `(targetDistance, up, lensLength)`; `SetProjection`'s tail is `(viewName, updateConstructionPlane)`; `GetFramePlaneCorners(depth)` returns the quad ordered bottom-left, bottom-right, top-left, top-right.

| [INDEX] | [SURFACE]                                                                    | [CALL_SHAPE]  | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------------------------- | :------------ | :---------------------------------- |
|  [01]   | `GetCameraFrame(out Plane frame)`                                            | camera read   | pose frame, `bool`                  |
|  [02]   | `CameraTarget` / `CameraUp`                                                  | camera state  | target and up vectors               |
|  [03]   | `SetCameraLocations(Point3d targetLocation, Point3d cameraLocation)`         | camera set    | atomic target+location, `void`      |
|  [04]   | `SetCameraDirection(Vector3d, bool updateTargetLocation)`                    | camera set    | direction write, `void`             |
|  [05]   | `GetDepth(Point3d point, out double distance)`                               | depth read    | single-point depth                  |
|  [06]   | `GetDepth(BoundingBox, out double nearDistance, out double farDistance)`     | depth read    | box near/far pair                   |
|  [07]   | `GetDepth(Sphere, out double nearDistance, out double farDistance)`          | depth read    | sphere near/far pair                |
|  [08]   | `IsVisible(Point3d)` / `IsVisible(BoundingBox)`                              | visibility    | frustum containment probes          |
|  [09]   | `GetFrustum(out double left, right, bottom, top, nearDistance, farDistance)` | frustum read  | six-plane read, `bool`              |
|  [10]   | `FrustumAspect` / `GetFrustumBoundingBox()`                                  | frustum read  | aspect and frustum bounds           |
|  [11]   | `GetFrustumLine(double screenX, double screenY, out Line worldLine)`         | frustum read  | screen-point world ray              |
|  [12]   | `GetWorldToScreenScale(Point3d pointInFrustum, out double pixelsPerUnit)`    | frustum read  | pixels-per-unit scale               |
|  [13]   | `GetConstructionPlane()`                                                     | cplane read   | live construction plane             |
|  [14]   | `SetConstructionPlane(ConstructionPlane)`                                    | cplane write  | non-stack cplane set, `void`        |
|  [15]   | `PushConstructionPlane(ConstructionPlane)` / `PopConstructionPlane()`        | cplane stack  | push `void`, pop benign `bool`      |
|  [16]   | `PushViewProjection()`                                                       | view stack    | projection push, `void`             |
|  [17]   | `PopViewProjection()` / `NextViewProjection()` / `PreviousViewProjection()`  | view stack    | benign-`false` stack moves          |
|  [18]   | `KeyboardRotate(bool leftRight, double angleRadians)`                        | gesture       | keyboard rotate, `bool`             |
|  [19]   | `KeyboardDolly(bool leftRight, double amount)`                               | gesture       | keyboard dolly, `bool`              |
|  [20]   | `KeyboardDollyInOut(double amount)`                                          | gesture       | keyboard in/out dolly, `bool`       |
|  [21]   | `MouseRotateAroundTarget(Point mousePreviousPoint, Point mouseCurrentPoint)` | gesture       | drag orbit, `bool`                  |
|  [22]   | `MouseRotateCamera` / `MouseTilt` / `MouseMagnify`                           | gesture       | drag rotate/tilt/magnify            |
|  [23]   | `MouseInOutDolly` / `MouseDollyZoom` / `MouseLateralDolly`                   | gesture       | drag dolly family                   |
|  [24]   | `LockedProjection`                                                           | projection    | mutable projection lock             |
|  [25]   | `ChangeToParallelProjection(bool symmetricFrustum)`                          | projection    | parallel change, `bool`             |
|  [26]   | `ChangeToPerspectiveProjection(double, bool, double)`                        | projection    | perspective change                  |
|  [27]   | `ChangeToTwoPointPerspectiveProjection(double, Vector3d, double)`            | projection    | two-point change                    |
|  [28]   | `ChangeToParallelReflectedProjection()`                                      | projection    | reflected change, `bool`            |
|  [29]   | `SetProjection(DefinedViewportProjection, string, bool)`                     | projection    | defined preset, `bool`              |
|  [30]   | `ZoomBoundingBox(BoundingBox box)`                                           | framing       | subject framing, `bool`             |
|  [31]   | `ChangeCounter`                                                              | staleness     | `uint` mutation serial              |
|  [32]   | `ViewportInfo.GetFramePlaneCorners(double depth)`                            | snapshot read | host-ordered corner quad            |
|  [33]   | `ViewportInfo.CalculateCameraUpDirection(Point3d, Vector3d, double)`         | snapshot read | static up derivation                |
|  [34]   | `ViewportInfo.GetXform(CoordinateSystem, CoordinateSystem)`                  | snapshot read | Unset-on-failure transform          |
|  [35]   | `ViewInfo.FocalBlurMode`                                                     | dof state     | `ViewInfoFocalBlurModes` row        |
|  [36]   | `ViewInfo.FocalBlurDistance` / `FocalBlurAperture` / `FocalBlurJitter`       | dof state     | focal-blur scalars                  |
|  [37]   | `ViewInfo.FocalBlurSampleCount`                                              | dof state     | `uint` sample count                 |
|  [38]   | `Rhino.ApplicationSettings.ViewSettings.DefinedViewSetCPlane`                | restore scope | static defined-view cplane flag     |
|  [39]   | `Rhino.ApplicationSettings.ViewSettings.DefinedViewSetProjection`            | restore scope | static defined-view projection flag |
|  [40]   | `Rhino.ApplicationSettings.ViewSettings.DefinedViewSetClippingPlanes`        | restore scope | static defined-view clipping flag   |
|  [41]   | `Rhino.ApplicationSettings.ViewSettings.DefinedViewSetDisplayMode`           | restore scope | static defined-view mode flag       |
|  [42]   | `IsParallelProjection` / `IsPerspectiveProjection`                           | camera read   | `bool` projection-class reads       |
|  [43]   | `IsTwoPointPerspectiveProjection`                                            | camera read   | two-point read; reflected has none  |

`ViewInfo` lives in `Rhino.DocObjects`; `ViewTypeFilter` (`Model`/`Page`/`ModelStyleViews`/`All`/`None`) lives in `Rhino.Display` and feeds `ViewTable.GetViewList`.

[ENTRYPOINT_SCOPE]: display modes, retained overlays, and capture
- rail: host-boundary display

`ViewCaptureSettings.RasterMode` selects raster or vector egress. Grid/axis decor spells differently per owner — `ViewCaptureSettings.DrawAxis` singular, instance `ViewCapture.DrawAxes` plural — and transparency exists only on the instance facade. `SetUpDisplayAttributes` takes `(RhinoObject, DisplayPipelineAttributes)`.

| [INDEX] | [SURFACE]                                                                    | [CALL_SHAPE]   | [CAPABILITY]                          |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `DisplayModeDescription.GetDisplayModes()`                                   | mode read      | mode enumeration                      |
|  [02]   | `DisplayModeDescription.GetDisplayMode(Guid)`                                | mode read      | identifier resolution                 |
|  [03]   | `DisplayModeDescription.FindByName(string)`                                  | mode read      | name resolution                       |
|  [04]   | `DisplayModeDescription.AddDisplayMode(DisplayModeDescription)`              | mode write     | mode creation                         |
|  [05]   | `DisplayModeDescription.UpdateDisplayMode(...)`                              | mode write     | mode update                           |
|  [06]   | `DisplayModeDescription.ImportFromFile(string, bool)`                        | mode write     | `.ini` import                         |
|  [07]   | `DisplayModeDescription.ExportToFile(DisplayModeDescription, string)`        | mode write     | `.ini` export                         |
|  [08]   | `IsoDrawEffect.GetBandColor(int)`                                            | effect         | band-color read                       |
|  [09]   | `IsoDrawEffect.SetBandColor(int, Color)`                                     | effect         | band-color write                      |
|  [10]   | `new CustomDisplay(bool)`                                                    | retained       | overlay construction                  |
|  [11]   | `CustomDisplay.Clear()`                                                      | retained       | overlay clearing                      |
|  [12]   | `CustomDisplay.AddPoints(IEnumerable<Point3d>, Color, PointStyle, int)`      | retained       | point accumulation                    |
|  [13]   | `CustomDisplay.AddPolygon(IEnumerable<Point3d>, Color, Color, bool, bool)`   | retained       | polygon accumulation                  |
|  [14]   | `CustomDisplay.AddText(Text3d, Color)`                                       | retained       | text accumulation                     |
|  [15]   | `CustomDisplay.Dispose()`                                                    | retained       | overlay retirement                    |
|  [16]   | `VisualAnalysisMode.Register(Type)`                                          | analysis       | mode registration                     |
|  [17]   | `VisualAnalysisMode.Find(Guid)`                                              | analysis       | mode resolution                       |
|  [18]   | `VisualAnalysisMode.SetUpDisplayAttributes(…)`                               | analysis       | display attributes                    |
|  [19]   | `VisualAnalysisMode.UpdateVertexColors(RhinoObject, Mesh[])`                 | analysis       | false-color vertex update             |
|  [20]   | `VisualAnalysisMode.DrawMesh(RhinoObject, Mesh, DisplayPipeline)`            | analysis       | analysis mesh draw                    |
|  [21]   | `RhinoDirectionAnalysisModeId`                                               | analysis id    | built-in direction mode               |
|  [22]   | `RhinoEndAnalysisModeId`                                                     | analysis id    | built-in end mode                     |
|  [23]   | `ViewCapture.CaptureToBitmap(ViewCaptureSettings)`                           | capture        | raster egress                         |
|  [24]   | `ViewCapture.CaptureToSvg(ViewCaptureSettings)`                              | capture        | vector egress                         |
|  [25]   | `ViewCapture.SendToPrinter(string, ViewCaptureSettings[], int)`              | capture        | printer egress copy count             |
|  [26]   | `new ViewCaptureSettings(RhinoView, Size, double)`                           | capture spec   | view settings                         |
|  [27]   | `new ViewCaptureSettings(RhinoPageView, double)`                             | capture spec   | page settings                         |
|  [28]   | `ViewCaptureSettings.CreatePreviewSettings(Size)`                            | capture spec   | preview-scaled derivation             |
|  [29]   | `ViewCaptureSettings.SetViewport(RhinoViewport)`                             | capture spec   | viewport binding                      |
|  [30]   | `ViewCaptureSettings.SetLayout(Size, Rectangle)`                             | capture spec   | page layout                           |
|  [31]   | `ViewCaptureSettings.SetModelScaleToValue(double)`                           | capture spec   | fixed model scale                     |
|  [32]   | `ViewCaptureSettings.SetModelScaleToFit(bool)`                               | capture spec   | fit model scale                       |
|  [33]   | `ViewCaptureSettings.SetWindowRect(Point2d, Point2d)`                        | capture spec   | screen window                         |
|  [34]   | `ViewCaptureSettings.SetWindowRect(Point3d, Point3d)`                        | capture spec   | world window                          |
|  [35]   | `ViewCaptureSettings.RasterMode`                                             | capture spec   | egress mode                           |
|  [36]   | `ViewCaptureSettings.OutputColor`                                            | capture spec   | color mode                            |
|  [37]   | `ViewCaptureSettings.OffsetAnchor`                                           | capture spec   | offset anchor                         |
|  [38]   | `ViewCaptureSettings.DrawGrid`                                               | capture flag   | grid draw                             |
|  [39]   | `ViewCaptureSettings.DrawAxis`                                               | capture flag   | axis draw                             |
|  [40]   | `ViewCaptureSettings.DrawBackground`                                         | capture flag   | background draw                       |
|  [41]   | `ViewCaptureSettings.DrawBackgroundBitmap`                                   | capture flag   | bitmap draw                           |
|  [42]   | `ViewCaptureSettings.DrawWallpaper`                                          | capture flag   | wallpaper draw                        |
|  [43]   | `ViewCaptureSettings.DrawLockedObjects`                                      | capture flag   | locked-object draw                    |
|  [44]   | `ViewCaptureSettings.DrawSelectedObjectsOnly`                                | capture flag   | selected-object filter                |
|  [45]   | `ViewCaptureSettings.DrawClippingPlanes`                                     | capture flag   | clipping-plane draw                   |
|  [46]   | `ViewCaptureSettings.DrawLights`                                             | capture flag   | light draw                            |
|  [47]   | `ViewCaptureSettings.DrawMargins`                                            | capture flag   | margin draw                           |
|  [48]   | `ViewCaptureSettings.HeaderText`                                             | capture spec   | page header                           |
|  [49]   | `ViewCaptureSettings.FooterText`                                             | capture spec   | page footer                           |
|  [50]   | `ViewCaptureSettings.UsePrintWidths`                                         | capture spec   | print-width selection                 |
|  [51]   | `ViewCaptureSettings.WireThicknessScale`                                     | capture spec   | wire-width scale                      |
|  [52]   | `ViewCaptureSettings.PointSizeMillimeters`                                   | capture spec   | point size                            |
|  [53]   | `ViewCaptureSettings.ArrowheadSizeMillimeters`                               | capture spec   | arrowhead size                        |
|  [54]   | `ViewCaptureSettings.TextDotPointSize`                                       | capture spec   | text-dot size                         |
|  [55]   | `ViewCaptureSettings.DefaultPrintWidthMillimeters`                           | capture spec   | default print width                   |
|  [56]   | `CustomDisplay.AddLine(Line, Color, int)`                                    | retained       | line accumulation                     |
|  [57]   | `CustomDisplay.AddVector(Point3d, Vector3d, Color, bool)`                    | retained       | anchored vector                       |
|  [58]   | `CustomDisplay.AddArc(Arc, Color, int)`                                      | retained       | arc accumulation                      |
|  [59]   | `CustomDisplay.AddCircle(Circle, Color, int)`                                | retained       | circle accumulation                   |
|  [60]   | `CustomDisplay.AddCurve(Curve, Color, int)`                                  | retained       | curve accumulation                    |
|  [61]   | `CustomDisplay.AddText(string, Plane, double, Color)`                        | retained       | planar text accumulation              |
|  [62]   | `CustomDisplay.Enabled`                                                      | retained       | overlay visibility toggle             |
|  [63]   | `ViewCaptureSettings.ViewArea`                                               | capture spec   | `ViewAreaMapping` View/Extents/Window |
|  [64]   | `ViewCaptureSettings.SetMargins(UnitSystem, double, double, double, double)` | capture spec   | margin write, `bool`                  |
|  [65]   | `ViewCaptureSettings.SetOffset(UnitSystem, bool, double, double)`            | capture spec   | offset write, `void`                  |
|  [66]   | `ViewCaptureSettings.MaximizePrintableArea()`                                | capture spec   | printable maximization                |
|  [67]   | `ViewCaptureSettings.MatchViewportAspectRatio()`                             | capture spec   | aspect match, `bool`                  |
|  [68]   | `ViewCaptureSettings.IsValid`                                                | capture spec   | configured-state validity             |
|  [69]   | `new ViewCapture()`                                                          | capture facade | instance raster bag                   |
|  [70]   | `ViewCapture.Width` / `Height`                                               | capture facade | pixel extent                          |
|  [71]   | `ViewCapture.TransparentBackground`                                          | capture facade | alpha egress                          |
|  [72]   | `ViewCapture.DrawGrid` / `DrawAxes` / `DrawGridAxes`                         | capture facade | facade decor                          |
|  [73]   | `ViewCapture.ScaleScreenItems`                                               | capture facade | screen-item scaling                   |
|  [74]   | `ViewCapture.RealtimeRenderPasses`                                           | capture facade | raytrace pass count                   |
|  [75]   | `ViewCapture.CaptureToBitmap(RhinoView sourceView)`                          | capture        | instance raster egress                |

[ENTRYPOINT_SCOPE]: `ZBufferCapture` — depth-field capture
- rail: host-boundary display

Configuration invalidates the cached grayscale bitmap, so mode and channels write before any read; `GrayscaleDib` caches and returns one bitmap instance that survives capsule disposal.

| [INDEX] | [SURFACE]                                             | [CALL_SHAPE]  | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------- | :------------ | :--------------------------- |
|  [01]   | `new ZBufferCapture(RhinoViewport viewport)`          | capsule       | viewport-bound depth capture |
|  [02]   | `ZBufferCapture.SetDisplayMode(Guid modeId)`          | depth config  | capture display mode         |
|  [03]   | `ZBufferCapture.ShowIsocurves(bool on)`               | depth channel | isocurve depth writes        |
|  [04]   | `ZBufferCapture.ShowMeshWires(bool on)`               | depth channel | mesh-wire depth writes       |
|  [05]   | `ZBufferCapture.ShowCurves(bool on)`                  | depth channel | curve depth writes           |
|  [06]   | `ZBufferCapture.ShowPoints(bool on)`                  | depth channel | point depth writes           |
|  [07]   | `ZBufferCapture.ShowText(bool on)`                    | depth channel | text depth writes            |
|  [08]   | `ZBufferCapture.ShowAnnotations(bool on)`             | depth channel | annotation depth writes      |
|  [09]   | `ZBufferCapture.ShowLights(bool on)`                  | depth channel | light depth writes           |
|  [10]   | `ZBufferCapture.HitCount() : int`                     | depth read    | populated-pixel census       |
|  [11]   | `ZBufferCapture.MinZ() : float`                       | depth read    | nearest buffer value         |
|  [12]   | `ZBufferCapture.MaxZ() : float`                       | depth read    | farthest buffer value        |
|  [13]   | `ZBufferCapture.ZValueAt(int x, int y) : float`       | depth read    | per-pixel depth              |
|  [14]   | `ZBufferCapture.WorldPointAt(int x, int y) : Point3d` | depth read    | screen-to-world unprojection |
|  [15]   | `ZBufferCapture.GrayscaleDib() : Bitmap`              | depth read    | cached grayscale rendering   |

[ENTRYPOINT_SCOPE]: display-mode appearance model, policy flags, and per-viewport assignment
- rail: host-boundary display

`DisplayPipelineAttributes` has no public constructor — it is reached through `DisplayModeDescription.DisplayAttributes` or handed in by host hooks (`VisualAnalysisMode.SetUpDisplayAttributes`, `RealtimeDisplayMode.OnDisplayPipelineSettingsChanged`). `CopyDisplayMode` registers in memory only; `UpdateDisplayMode` persists to disk. Its technical color/thickness members and technical-parameter mask are internal — the seven since-9.0 `Show*` toggles (`ShowHiddenLines`/`ShowEdges`/`ShowSilhouttes`/`ShowCreases`/`ShowSeams`/`ShowIntersections`/`ShowLighting`) are the whole public technical axis. `FlairDefinition`/`FlairParameters` are internal; `FlairLayer` alone is public and reaches no behavior. Usage members pair `Set*` with a `Get*` twin; the three dynamic effects add a `Has*` probe.

`BackfaceStyle` defines `UseFrontFaceSettings`, `CullBackfaces`, `UseObjectColor`, `SingleColorAllBackfaces`, `UseRenderMaterial`, and `CustomMaterialAllBackfaces`. `LightingSchema` defines `None`, `DefaultLighting`, `SceneLighting`, `CustomLighting`, and `AmbientOcclusion`. `ClippingPlaneFillColorUse` defines `ViewportColor`, `RenderMaterialColor`, `PlaneMaterialColor`, and `SolidColor`; `ClippingFillColorUse` does not exist. `ClippingEdgeColorUse` defines `PlaneColor`, `SolidColor`, and `ObjectColor`. `LockedObjectUse` defines `UseObjectAttributes`, `SpecifyColor`, and `UseAppSettings`.

`ActiveSpace` defines `None`, `ModelSpace`, `PageSpace`, `UVEditorSpace`, and `BlockEditorSpace`; `ActiveSpaceUse` maps the complete family for conduit filtering and gumball seating.

Beyond the method rows below, the attribute model is property families written by direct assignment: shading/material (`ShadingEnabled`, `ShadeVertexColors`, `FrontFlatShaded`, `UseAssignedObjectMaterial`/`UseCustomObjectColor`/`UseCustomObjectMaterial`, `ObjectColor`, `BackfaceDisplayStyle`, `CullBackfaces`, `Front`/`BackMaterialShine`/`Transparency`, `FrontDiffuse`, `BackMaterialDiffuseColor`, `HighlightSurfaces`, backface material source (`UseBackfaceMaterial`, `UseObjectBackfaceMaterial`, `UseCustomBackface`, `UseCustomObjectColorBackfaces`) and front/back object overrides (`FrontOverrideObject{Color,Transparency,Reflectivity}`, `BackOverrideObject{Transparency,Reflectivity}`)), curve/surface/iso edges (`ShowCurves`, `CurveThickness`/`Scale`/`Color`, `UseSingleCurveColor`, `ShowSurfaceEdges`, `ShowSurfaceNakedEdge`, `ShowTangentEdges`/`Seams`, `ShowIsoCurves`, `Surface*Edge*` color/thickness/reduction/pattern, `SurfaceIso*` UV colors and thicknesses), lighting/shadows (`LightingScheme`, `AmbientLightingColor`, `UseLightColor`, `ShowLights`, `CastShadows`, `Shadows*`, `SkylightShadowQuality`), ground plane (`GroundPlaneUsage`, `CustomGroundPlane*`), grid (`GridTransparency`, `GridPlaneTransparency`/`Visibility`/`Color`, `WorldAxesIconColorUsage`, `PlaneUsesGridColor`, `AxesSizePercentage`, plus the nested `ViewSpecificAttributes` grid/axes/scale members incl. Rhino 9 `GridFade`/`GridCornerRadius`/`GridBoundaryThickness`), SubD edge classes (`SubD{SmoothInterior,CreaseInterior,NonManifold,Boundary}*` color/usage/reduction/thickness/scale/pattern, `ShowSubD*`, reflection-plane members), mesh edges (`Mesh{,Naked,Nonmanifold}Edge*`, `MeshVertexSize`, nested `MeshSpecificAttributes`), clipping (`ShowClipping*`, `ShowClipIntersection*`, `Clipping*Color`/`Usage`/`Thickness`/`Transparency`, `UseSectionStyles`), locked objects (`LockedObjectUsage`/`Transparency`, `LockedColor`, `GhostLockedObjects`, `LockedObjectsDrawBehindOthers`, `LayersFollowLockUsage`), points/grips (`ShowPoints`/`PointStyle`/`PointRadius`, `ShowPointClouds`, `PointCloud*`, `ShowGrips`, `ControlPolygon*`), and scene/pipeline (`XrayAllObjects`, `IgnoreHighlights`, `DisableConduits`, `DisableTransparency`, `ShowText`, `ShowAnnotations`, `BoundingBoxMode`, `DynamicDisplayUsage`, `LinearWorkflowUsage`, `PreProcess*`, `PostProcess*`). Policy vocabularies are the public nested enums (`FrameBufferFillMode`, `BoundingBoxDisplayMode`, `BackfaceStyle`, `LightingSchema`, `ContextsForDraw`, the `*Use` families, `GridPlaneVisibilityMode`, `WorldAxesIconColorUse`, `GroundPlaneUsages`, `LinearWorkflowUsages`).

| [INDEX] | [SURFACE]                                                                       | [CALL_SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------ | :------------ | :--------------------------------- |
|  [01]   | `DisplayModeDescription.CopyDisplayMode(Guid, string)`                          | mode write    | in-memory mode copy                |
|  [02]   | `DisplayModeDescription.DeleteDisplayMode(Guid)`                                | mode write    | mode removal                       |
|  [03]   | `DisplayModeDescription.AddDisplayMode(string)`                                 | mode write    | named blank mode creation          |
|  [04]   | `DisplayModeDescription.DisplayAttributes`                                      | mode state    | live attribute editor accessor     |
|  [05]   | `DisplayModeDescription.EnglishName`                                            | mode state    | mutable mode name                  |
|  [06]   | `DisplayModeDescription.Id` / `LocalName`                                       | mode state    | identity and localized name        |
|  [07]   | `DisplayModeDescription.InMenu`                                                 | mode policy   | menu listing flag                  |
|  [08]   | `DisplayModeDescription.SupportsShadeCommand`                                   | mode policy   | shade-command support flag         |
|  [09]   | `DisplayModeDescription.SupportsShading`                                        | mode policy   | shading support flag               |
|  [10]   | `DisplayModeDescription.AllowObjectAssignment`                                  | mode policy   | per-object assignment flag         |
|  [11]   | `DisplayModeDescription.ShadedPipelineRequired`                                 | mode policy   | shaded-pipeline requirement        |
|  [12]   | `DisplayModeDescription.WireframePipelineRequired`                              | mode policy   | wireframe-pipeline requirement     |
|  [13]   | `DisplayModeDescription.PipelineLocked`                                         | mode policy   | pipeline lock flag                 |
|  [14]   | `DisplayModeDescription.WireframeId` / `ShadedId` / `RenderedId`                | builtin id    | core special-mode ids              |
|  [15]   | `DisplayModeDescription.RenderedShadowsId` / `GhostedId` / `XRayId`             | builtin id    | shadowed/ghosted/x-ray ids         |
|  [16]   | `DisplayModeDescription.TechId` / `ArtisticId` / `PenId`                        | builtin id    | technical-family ids               |
|  [17]   | `DisplayModeDescription.MonochromeId` / `AmbientOcclusionId` / `RaytracedId`    | builtin id    | monochrome/AO/raytraced ids        |
|  [18]   | `DisplayPipelineAttributes.ViewSpecificAttributes`                              | nested editor | grid, axes, viewport-scale members |
|  [19]   | `DisplayPipelineAttributes.MeshSpecificAttributes`                              | nested editor | mesh-wire and vertex members       |
|  [20]   | `DisplayPipelineAttributes.SetFill(Color)`                                      | frame fill    | solid fill colors                  |
|  [21]   | `DisplayPipelineAttributes.SetFill(Color, Color)`                               | frame fill    | two-color gradient fill            |
|  [22]   | `DisplayPipelineAttributes.SetFill(Color, Color, Color, Color)`                 | frame fill    | four-corner gradient fill          |
|  [23]   | `DisplayPipelineAttributes.GetFill(out Color, out Color, out Color, out Color)` | frame fill    | fill color read-back               |
|  [24]   | `DisplayPipelineAttributes.FillMode`                                            | frame fill    | `FrameBufferFillMode` selector     |
|  [25]   | `DisplayPipelineAttributes.SetCurveThicknessUsage(CurveThicknessUse)`           | width use     | curve width discriminant           |
|  [26]   | `DisplayPipelineAttributes.SetSurfaceEdgeThicknessUsage(SurfaceThicknessUse)`   | width use     | surface-edge width discriminant    |
|  [27]   | `DisplayPipelineAttributes.SetSurfaceNakedEdgeThicknessUsage(...)`              | width use     | naked-edge width discriminant      |
|  [28]   | `DisplayPipelineAttributes.SetSurfaceIsoThicknessUsage(...)`                    | width use     | iso width discriminant             |
|  [29]   | `DisplayPipelineAttributes.SetSurfaceIsoColorUsage(SurfaceIsoColorUse)`         | color use     | iso color discriminant             |
|  [30]   | `DisplayPipelineAttributes.SetSurfaceIsoApplyPattern(bool, bool, bool)`         | pattern       | per-direction iso pattern          |
|  [31]   | `DisplayPipelineAttributes.SetColorFadeEffect(in Color, in float)`              | effect        | dynamic color-fade effect          |
|  [32]   | `DisplayPipelineAttributes.SetDitherTransparencyEffect(in float)`               | effect        | dynamic dither-transparency effect |
|  [33]   | `DisplayPipelineAttributes.SetDiagonalHatchEffect(in float, in float)`          | effect        | dynamic diagonal-hatch effect      |
|  [34]   | `RhinoViewport.DisplayMode`                                                     | viewport      | per-viewport mode binding          |
|  [35]   | `RhinoView.CaptureToBitmap(DisplayModeDescription)`                             | capture       | mode-scoped view capture           |
|  [36]   | `RhinoView.CaptureToBitmap(Size, DisplayModeDescription)`                       | capture       | sized mode-scoped capture          |
|  [37]   | `RhinoView.CaptureToBitmap(DisplayPipelineAttributes)`                          | capture       | attribute-scoped capture           |
|  [38]   | `RhinoView.CaptureToBitmap(Size, DisplayPipelineAttributes)`                    | capture       | sized attribute-scoped capture     |
|  [39]   | `RhinoView.RuntimeSerialNumber`                                                 | identity      | detached view identity             |
|  [40]   | `RhinoViewport.Id`                                                              | identity      | detached viewport identity         |

[ENTRYPOINT_SCOPE]: built-in visual-analysis ids and object enablement
- rail: host-boundary display

`RhinoCurvatureColorAnalyisModeId` and `RhinoEdgeContinuityAlalysisModeId` carry the host's own misspellings. `VisualAnalysisMode` nested enums: `AnalysisStyle` (`Wireframe`/`Texture`/`FalseColor`) and `EdgeContinuityMode` (`Distance`/`Tangency`/`Curvature`).

| [INDEX] | [SURFACE]                                                        | [CALL_SHAPE] | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------------- | :----------- | :------------------------------ |
|  [01]   | `VisualAnalysisMode.RhinoEdgeAnalysisModeId`                     | analysis id  | built-in edge mode              |
|  [02]   | `VisualAnalysisMode.RhinoCurvatureGraphAnalysisModeId`           | analysis id  | built-in curvature-graph mode   |
|  [03]   | `VisualAnalysisMode.RhinoZebraStripeAnalysisModeId`              | analysis id  | built-in zebra mode             |
|  [04]   | `VisualAnalysisMode.RhinoEmapAnalysisModeId`                     | analysis id  | built-in emap mode              |
|  [05]   | `VisualAnalysisMode.RhinoCurvatureColorAnalyisModeId`            | analysis id  | built-in curvature-color mode   |
|  [06]   | `VisualAnalysisMode.RhinoDraftAngleAnalysisModeId`               | analysis id  | built-in draft-angle mode       |
|  [07]   | `VisualAnalysisMode.RhinoThicknessAnalysisModeId`                | analysis id  | built-in thickness mode         |
|  [08]   | `VisualAnalysisMode.RhinoEdgeContinuityAlalysisModeId`           | analysis id  | built-in edge-continuity mode   |
|  [09]   | `VisualAnalysisMode.AdjustAnalysisMeshes(RhinoDoc, Guid)`        | analysis     | interactive mesh density adjust |
|  [10]   | `VisualAnalysisMode.CurvatureColorAutoRange()`                   | analysis     | curvature auto-range (9.0)      |
|  [11]   | `VisualAnalysisMode.CurvatureColorMaxRange()`                    | analysis     | curvature max-range (9.0)       |
|  [12]   | `VisualAnalysisMode.Find(Type)`                                  | analysis     | mode resolution by type         |
|  [13]   | `VisualAnalysisMode.EnableUserInterface(bool)`                   | analysis     | mode UI toggle                  |
|  [14]   | `VisualAnalysisMode.ObjectSupportsAnalysisMode(RhinoObject)`     | analysis     | per-object support gate         |
|  [15]   | `VisualAnalysisMode.Id` / `Name` / `Style`                       | analysis     | resolved-mode facts             |
|  [16]   | `RhinoObject.EnableVisualAnalysisMode(VisualAnalysisMode, bool)` | enablement   | per-object attach/detach        |
|  [17]   | `RhinoObject.InVisualAnalysisMode()`                             | enablement   | any-mode membership probe       |
|  [18]   | `RhinoObject.InVisualAnalysisMode(VisualAnalysisMode)`           | enablement   | specific-mode membership probe  |
|  [19]   | `RhinoObject.GetActiveVisualAnalysisModes()`                     | enablement   | active-mode census              |

## [04]-[IMPLEMENTATION_LAW]

[DISPLAY_TOPOLOGY]:
- Frame participation has two shapes with one phase order: subclass `DisplayConduit` and override the phases, or subscribe the mirror `DisplayPipeline` static events — the boundary picks subclassing when it owns per-instance state (filters, cached geometry) and events for a stateless tap. Phase order (`ObjectCulling` → `CalculateBoundingBox` → `PreDrawObjects`/`PreDrawObject` → object walk → `PostDrawObjects` → `DrawForeground` → `DrawOverlay`) is identical for both.
- Direct draws run only inside a phase against the phase's `DisplayPipeline`; a draw family consumes a `Rhino.Geometry` carrier plus a `DisplayMaterial`/`Color`/`DisplayPen`, and any transform or depth/cull override wraps the draw in a matched `Push*`/`Pop*` pair so the pipeline state unwinds exactly.
- A conduit-free overlay is `CustomDisplay` (document-lifetime, `IDisposable`); a per-object false-color surface is a registered `VisualAnalysisMode`; a per-frame interactive overlay is a bound conduit. Retained accumulation, registered analysis, and live participation are distinct owners that never overlap.
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
- `Rhino.Display` types are host handles trapped and mapped at the boundary; a `DisplayPipeline`, `RhinoViewport`, or `ViewCaptureSettings` never appears in a domain signature — the domain sees a `Fin<A>`, a bounded owner, or a canonical shape.
- A conduit or `CustomDisplay` is the single retained owner for its overlay concern; a second parallel conduit drawing the same overlay is the collapsed form.

[RAIL_LAW]:
- Package: `RhinoCommon` (`Rhino.Display`)
- Owns: view/viewport/camera pose, the two frame-participation shapes and their draw families, render-state stacks, retained `CustomDisplay`/`VisualAnalysisMode` overlays, display-mode and display-attribute vocabulary, and `ViewCapture` egress
- Accept: a bound conduit or subscribed pipeline event drawing geometry carriers under matched state pushes; a settings-driven capture; a registered analysis mode; host handles trapped through `Try.lift(...).Run()` and enums mapped to bounded owners at the edge
- Reject: a host draw call outside a frame phase, an unmatched `Push*`/`Pop*` leaving pipeline state dirty, a `DisplayPipeline`/`RhinoViewport`/`DisplayModeDescription` escaping into a domain signature, a hand-rolled overlay where `CustomDisplay` fits, a wall-clock sprite animation where the frame clock is available, and a re-derived color blend where the kernel color rail is composed
