# [RASM_RHINO_API_RHINOCOMMON_DISPLAY]

Catalog scope: the view, viewport, camera, display-pipeline, conduit, and capture surface — everything between document geometry and pixels.

[NAMESPACES]:
- `Rhino.Display` viewports — `RhinoView`/`RhinoPageView` (view events, detail views, page lifecycle), `RhinoViewport` (camera pose/projection/lock/frustum/dolly/zoom/gesture-rail families, construction-plane and view-projection stacks, screen transforms), `ViewportInfo`, `ViewInfo` (focal blur), `DisplayModeDescription`, `DefinedViewportProjection`, `IsometricCamera`.
- `Rhino.Display` pipeline — `DisplayPipeline` (draw families: curve/mesh/brep/subd/surface/point/pointcloud/hatch/dot/text/annotation/light/gradient/sprite/2d primitives; push/pop depth/cull/transform/projection state; `IsInViewCapture`/`Printing`/`DynamicDisplay`, `DpiScale`, `NestLevel`), `DisplayConduit` (`ObjectCulling`, `PreDrawObjects`/`PreDrawObject`, `DrawForeground`/`DrawOverlay`, `PostDrawObjects`, `CalculateBoundingBox` families), `DrawEventArgs` families, `DisplayMaterial`, `DisplayBitmap`/`DisplayBitmapDrawList`, `DisplayPen`, `LineCapStyle`/`LineJoinStyle`, `BlendMode`.
- `Rhino.Display` pipeline events — `ViewportProjectionChanged`, `DisplayModeChanged`, `DrawForeground`, `DrawOverlay`; `RhinoView` `Modified`/`Create`/`Destroy`/`SetActive`/`Rename`; `RhinoPageView` `PageViewSpaceChange`/`PageViewPropertiesChange`.
- `Rhino.Display` capture — `ViewCapture` (`CaptureToBitmap`/`CaptureToSvg`/`SendToPrinter`), `ViewCaptureSettings` (media size, layout, margins, scale, color mode, view-area mapping).
- `Rhino.DocObjects` display-adjacent — `DetailViewObject`/`DetailView` (page/model transforms, scale, clipping), `ClippingPlaneObject`, `ClippingPlaneSurface`.
