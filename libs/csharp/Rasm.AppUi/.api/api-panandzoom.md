# [RASM_APPUI_API_PANANDZOOM]

`PanAndZoom` owns the `ZoomBorder` viewport control: the affine `Matrix` and every pan, zoom, rotate, and fit gesture over a single `Child`. `ZoomBorder` is the host frame the `Wgpu`/Skia render surface mounts inside — the control owns the camera transform, the child owns the pixels.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PanAndZoom`
- package: `PanAndZoom` (MIT)
- assembly: `PanAndZoom`
- namespace: `Avalonia.Controls.PanAndZoom`
- asset: managed library; `lib/net10.0` binds the consumer, `lib/net8.0` fallback
- depends: `Avalonia` — `ZoomBorder` derives `Border`; surfaces carry `StyledProperty`/`DirectProperty`/`Matrix`/`Point`/`Rect`/`Vector`/`Thickness`/`ICommand`
- rail: viewport

## [02]-[PUBLIC_TYPES]

[VIEWPORT_TYPES]: the control, its state carriers, and matrix algebra

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :------------------ | :------------ | :---------------------------------- |
|  [01]   | `ZoomBorder`        | class         | the viewport control                |
|  [02]   | `ZoomBorderState`   | class         | exportable viewport-state carrier   |
|  [03]   | `SavedView`         | struct        | named view snapshot                 |
|  [04]   | `ViewState`         | struct        | history entry                       |
|  [05]   | `ZoomBorderCommand` | class         | `ICommand` relay for bound commands |
|  [06]   | `MatrixHelper`      | static class  | affine matrix builders              |

- `ZoomBorder`: `Border, ILogicalScrollable, IScrollable`.
- `ZoomBorderState`: `Matrix`/`Stretch`/`Rotation`/clamps/anim + `Timestamp`, all public setters, so it serializes directly.
- `SavedView`: `Name`/`Matrix`/`Stretch`/`Description`/`Timestamp`. `ViewState`: `Matrix`/`Stretch`/`Timestamp`.
- `MatrixHelper`: `Translate`/`Scale`/`ScaleAt`/`Rotation`/`TransformPoint`.

[MODE_TYPES]: behavior vocabularies feeding the styled policy properties

[StretchMode]: `None` `Fill` `Uniform` `UniformToFill`
[ButtonName]: `Left` `Right` `Middle`
[WheelBehaviorMode]: `Zoom` `PanVertical` `PanHorizontal` `None`
[DoubleClickZoomMode]: `ZoomIn` `ZoomOut` `ZoomInOut` `ZoomToFit` `None`
[ContentBoundsMode]: `Unrestricted` `KeepContentVisible` `FillViewport` `KeepCentered` `Custom`
[ResizeBehaviorMode]: `None` `MaintainCenter` `MaintainTopLeft` `MaintainZoom` `ReapplyStretch` `Custom`
[ZoomIndicatorPosition]: `TopLeft` `TopRight` `BottomLeft` `BottomRight` `Custom`

[EVENT_TYPES]: each `*EventArgs` pairs its `*EventHandler` delegate; `ZoomBorder` raises

| [INDEX] | [SYMBOL]                      | [FIRES]                                      |
| :-----: | :---------------------------- | :------------------------------------------- |
|  [01]   | `ZoomChangedEventArgs`        | `ZoomChanged` (ZoomX/Y, OffsetX/Y)           |
|  [02]   | `ZoomEventArgs`               | `ZoomStarted`/`ZoomEnded`/`ZoomDeltaChanged` |
|  [03]   | `PanEventArgs`                | `PanStarted`/`PanContinued`/`PanEnded`       |
|  [04]   | `MatrixChangedEventArgs`      | `MatrixChanged`/`MatrixReset`                |
|  [05]   | `StretchModeChangedEventArgs` | `StretchModeChanged`/`AutoFitApplied`        |
|  [06]   | `GestureEventArgs`            | `GestureStarted`/`GestureEnded` (touch)      |

## [03]-[ENTRYPOINTS]

Every surface hangs off `ZoomBorder`. Transform methods trail `bool skipTransitions = false`; history, rotation, and view methods trail `bool animate = true`.

[ZOOM_ENTRYPOINTS]: zoom, pan, and drag operations

| [INDEX] | [SURFACE]                                                         | [CAPABILITY]                |
| :-----: | :---------------------------------------------------------------- | :-------------------------- |
|  [01]   | `Zoom(double zoom, double x, double y, bool)`                     | absolute content-point zoom |
|  [02]   | `ZoomTo(double ratio, double x, double y, bool)`                  | relative content-point zoom |
|  [03]   | `ZoomDeltaTo(double delta, double x, double y, bool)`             | wheel-delta zoom            |
|  [04]   | `ZoomIn(bool)`                                                    | stepped center zoom in      |
|  [05]   | `ZoomOut(bool)`                                                   | stepped center zoom out     |
|  [06]   | `ZoomToLevel(double level, double centerX, double centerY, bool)` | absolute zoom-level jump    |
|  [07]   | `ZoomToRectangle(Rect rect, Thickness? padding, bool)`            | padded rectangle focus      |
|  [08]   | `ZoomToRectangleExact(Rect rect, Rect viewportRect, bool)`        | exact rectangle focus       |
|  [09]   | `Pan(double x, double y, bool)`                                   | absolute pan                |
|  [10]   | `PanDelta(double dx, double dy, bool)`                            | relative pan                |
|  [11]   | `BeginPanTo(double x, double y)`                                  | drag-pan start              |
|  [12]   | `ContinuePanTo(double x, double y, bool)`                         | drag-pan continuation       |
|  [13]   | `CenterOn(Point\|Rect\|Control, [double zoom], bool animate)`     | optional-zoom centering     |

`ZoomDeltaTo` applies `ZoomSpeed` and `PowerFactor`.

[LAYOUT_ENTRYPOINTS]: stretch fitting, matrix, and rotation

| [INDEX] | [SURFACE]                                                            | [CAPABILITY]             |
| :-----: | :------------------------------------------------------------------- | :----------------------- |
|  [01]   | `AutoFit(double panelW, panelH, elementW, elementH, bool)`           | sized child fit          |
|  [02]   | `AutoFit(bool)`                                                      | measured child fit       |
|  [03]   | `Uniform`/`UniformToFill`/`Fill`/`None` sized and `(bool)` overloads | `StretchMode` fit        |
|  [04]   | `ToggleStretchMode()`                                                | next `StretchMode`       |
|  [05]   | `SetMatrix(Matrix matrix, bool skipTransitions)`                     | direct matrix set        |
|  [06]   | `ResetMatrix([bool])`                                                | identity reset           |
|  [07]   | `Rotate(double degrees, bool animate)`                               | center rotation          |
|  [08]   | `RotateAt(double degrees, Point center, bool animate)`               | point rotation           |
|  [09]   | `ResetRotation(bool animate)`                                        | rotation reset           |
|  [10]   | `SnapRotation()`                                                     | `RotationSnapAngle` snap |

`AutoFit` raises `AutoFitApplied`.

[TRANSFORM_ENTRYPOINTS]: coordinate mapping and visible-bounds queries

| [INDEX] | [SURFACE]                                                           | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------ | :------------------------------------------ |
|  [01]   | `ViewportToContent(Point\|Rect)` / `ContentToViewport(Point\|Rect)` | map viewport <-> content space              |
|  [02]   | `ScreenToContent(Vector)` / `ContentToScreen(Vector)`               | map screen-delta <-> content-delta vectors  |
|  [03]   | `GetContentToScreenMatrix()` / `GetScreenToContentMatrix()`         | the live affine and its inverse (`Matrix`)  |
|  [04]   | `GetVisibleContentBounds()` / `GetViewportBounds()`                 | visible content `Rect` / viewport `Rect`    |
|  [05]   | `IsPointVisible(Point)` / `IsRectangleVisible(Rect)`                | hit-test against the visible content bounds |

[STATE_ENTRYPOINTS]: saved views, history, discrete levels, grid, and state export

| [INDEX] | [SURFACE]                                           | [CAPABILITY]                 |
| :-----: | :-------------------------------------------------- | :--------------------------- |
|  [01]   | `SaveView(string name, string? description)`        | named-view persistence       |
|  [02]   | `RestoreView(string name, bool animate) -> bool`    | named-view restoration       |
|  [03]   | `GetSavedView(string) -> SavedView?`                | named-view lookup            |
|  [04]   | `GetSavedViews() -> IReadOnlyCollection<SavedView>` | saved-view enumeration       |
|  [05]   | `GetSavedViewNames() -> string[]`                   | saved-view name enumeration  |
|  [06]   | `DeleteSavedView(string) -> bool`                   | named-view deletion          |
|  [07]   | `ClearSavedViews()`                                 | saved-view clearing          |
|  [08]   | `NavigateBack(bool animate)`                        | backward history navigation  |
|  [09]   | `NavigateForward(bool animate)`                     | forward history navigation   |
|  [10]   | `ClearViewHistory()`                                | history clearing             |
|  [11]   | `GetNextDiscreteZoomLevel() -> double`              | next discrete zoom level     |
|  [12]   | `GetPreviousDiscreteZoomLevel() -> double`          | previous discrete zoom level |
|  [13]   | `SnapToGrid(double\|Point\|Rect)`                   | `GridSize` snapping          |
|  [14]   | `ExportState() -> ZoomBorderState`                  | viewport-state export        |
|  [15]   | `ImportState(ZoomBorderState state, bool animate)`  | viewport-state import        |
|  [16]   | `UpdateAccessibilityDescriptions()`                 | accessibility refresh        |

[COMMAND_ENTRYPOINTS]: each a lazily-built `ZoomBorderCommand` exposed as a `DirectProperty<ZoomBorder, ICommand>` for code-behind-free XAML binding

| [INDEX] | [SURFACE]                                                                                         | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------------------------------------ | :--------------------------------- |
|  [01]   | `ZoomInCommand` / `ZoomOutCommand` / `ResetCommand`                                               | `ZoomIn`/`ZoomOut`/reset matrix    |
|  [02]   | `FitCommand` / `FillCommand` / `UniformCommand` / `UniformToFillCommand` / `ToggleStretchCommand` | stretch fits + `ToggleStretchMode` |
|  [03]   | `NavigateBackCommand` / `NavigateForwardCommand`                                                  | history undo/redo                  |

[PROPERTY_ENTRYPOINTS]: styled behavior properties, each a `StyledProperty`/`DirectProperty` for XAML binding

| [INDEX] | [SURFACE]                                                 | [CAPABILITY]                 |
| :-----: | :-------------------------------------------------------- | :--------------------------- |
|  [01]   | `Stretch`                                                 | `StretchMode`                |
|  [02]   | `PanButton`                                               | `ButtonName`                 |
|  [03]   | `ZoomSpeed`/`PowerFactor`/`TransitionThreshold`           | `double` transform policy    |
|  [04]   | `ZoomX`/`ZoomY`/`OffsetX`/`OffsetY`                       | read-only transform          |
|  [05]   | `Matrix`                                                  | read-only `Matrix`           |
|  [06]   | `EnablePan`/`EnableZoom`                                  | `bool` input gates           |
|  [07]   | `EnableGestures`/`EnableGestureZoom`                      | `bool` gesture gates         |
|  [08]   | `EnableGestureRotation`/`EnableGestureTranslation`        | `bool` gesture gates         |
|  [09]   | `EnableConstrains`                                        | clamp gate                   |
|  [10]   | `MinZoomX`/`MaxZoomX`/`MinZoomY`/`MaxZoomY`               | zoom clamps                  |
|  [11]   | `MinOffsetX`/`MaxOffsetX`/`MinOffsetY`/`MaxOffsetY`       | offset clamps                |
|  [12]   | `AutoCalculateMinZoom`/`AutoCalculateMaxZoom`             | automatic zoom clamps        |
|  [13]   | `MaxZoomPixelSize`                                        | pixel-size clamp             |
|  [14]   | `BoundsMode`/`BoundsPadding`                              | content-bounds policy        |
|  [15]   | `MinimumVisibleContentPercentage`                         | visibility floor             |
|  [16]   | `ResizeBehavior`/`CenterPadding`                          | resize policy                |
|  [17]   | `WheelBehavior`/`WheelWithCtrl`/`WheelWithShift`          | modal wheel routing          |
|  [18]   | `WheelZoomSensitivity`/`WheelPanSensitivity`              | wheel sensitivity            |
|  [19]   | `EnableKeyboardNavigation`                                | keyboard gate                |
|  [20]   | `KeyboardPanStep`/`KeyboardZoomStep`                      | keyboard navigation          |
|  [21]   | `EnableAnimations`                                        | transition gate              |
|  [22]   | `AnimationDuration`                                       | `TimeSpan` transition period |
|  [23]   | `EnableDoubleClickZoom`/`DoubleClickZoomMode`             | double-click routing         |
|  [24]   | `DoubleClickZoomFactor`                                   | double-click zoom            |
|  [25]   | `EnableDiscreteZoomLevels`                                | discrete zoom gate           |
|  [26]   | `DiscreteZoomLevels`                                      | `double[]?` zoom ladder      |
|  [27]   | `EnableViewHistory`/`ViewHistorySize`                     | undo/redo history depth      |
|  [28]   | `ShowGrid`/`EnableSnapToGrid`/`GridSize`                  | grid visibility and snapping |
|  [29]   | `GridBrush`/`GridThickness`/`GridOpacity`                 | minor-grid rendering         |
|  [30]   | `MajorGridInterval`/`MajorGridBrush`/`MajorGridThickness` | major-grid rendering         |
|  [31]   | `Rotation`/`MinRotation`/`MaxRotation`                    | rotation policy              |
|  [32]   | `EnableRotationSnapping`/`RotationSnapAngle`              | rotation snapping            |
|  [33]   | `ShowZoomIndicator`/`ZoomIndicatorPosition`               | zoom HUD placement           |
|  [34]   | `ZoomIndicatorFormat`/`ZoomIndicatorAutoHideDuration`     | zoom HUD formatting          |
|  [35]   | `IsZoomIndicatorVisible`                                  | zoom HUD state               |
|  [36]   | `EnableSimultaneousPanZoom`                               | multi-touch gesture gate     |
|  [37]   | `MinimumTouchPoints`/`MaximumTouchPoints`                 | multi-touch bounds           |
|  [38]   | `CanHorizontallyScroll`/`CanVerticallyScroll`             | `ILogicalScrollable` query   |
|  [39]   | `CanNavigateBack`/`CanNavigateForward`                    | history query                |
|  [40]   | `ZoomLevelDescription`/`PanPositionDescription`           | accessibility descriptions   |
|  [41]   | `UseHighContrastMode`                                     | accessibility contrast       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ZoomBorder` wraps one `Child` and applies the affine `Matrix` to it; the render surface is that `Child`, so the control owns the camera and the child owns the pixels.
- `Matrix`/`ZoomX`/`ZoomY`/`OffsetX`/`OffsetY` (`DirectProperty`) expose the live transform read-only; mutation flows only through the operation methods, which raise the matching events.
- Coordinate mapping is the input-to-render seam: `ViewportToContent`/`ScreenToContent` carry a pointer hit or screen-delta into content space for selection and measurement, and `GetContentToScreenMatrix()` with its inverse drives a camera-tracking overlay — pick in content space, render the overlay in viewport space.
- View history and named `SavedView`s are two registries: history is the implicit `ViewHistorySize`-bounded `ViewState` ring auto-pushed on view change (`ViewHistoryChanged`), saved views are explicit named bookmarks. Discrete-zoom (`DiscreteZoomLevels`) and grid-snap (`GridSize`) ladders are opt-in; with `EnableDiscreteZoomLevels` and `EnableSnapToGrid` both off the control is continuous.

[STACKING]:
- `api-silk-webgpu-wgpu`(`.api/api-silk-webgpu-wgpu.md`): the `Wgpu`/Skia swap-chain control mounts as the `ZoomBorder` `Child`, so the affine transforms the render surface directly.
- `api-thinktecture-json`(`libs/csharp/.api/api-thinktecture-json.md`): `ExportState()`/`ImportState()` round-trip `ZoomBorderState` through the shared JSON rail; `SavedView`/`ViewState` are plain-setter structs that serialize without custom converters.
- `api-reactiveui-avalonia`(`.api/api-reactiveui-avalonia.md`): the `ICommand` properties bind to toolbar buttons in XAML for view-model-driven zoom, fit, and history, and the change events (`ZoomChanged`/`MatrixChanged`/`StretchModeChanged`) project into the view-model through `Observable.FromEventPattern` — a measurement overlay subscribes `MatrixChanged`/`ZoomChanged` to invalidate its cache and `GestureStarted`/`GestureEnded` to gate live picking.
- `api-unitsnet`(`libs/csharp/.api/api-unitsnet.md`): `GetVisibleContentBounds()` projected through the affine px/content ratio yields a `Length` for a scale-bar label via `Length.ToUnit(UnitSystem.SI)`.
- Avalonia accessibility: `ZoomLevelDescription`/`PanPositionDescription` feed the automation peer; `UpdateAccessibilityDescriptions()` refreshes them after a programmatic view change, and `UseHighContrastMode` recolors the grid and indicator brushes.

[LOCAL_ADMISSION]:
- Viewport pan, zoom, and rotate intent binds `ZoomBorder` as the render surface's parent; the affine, gestures, history, saved views, and constraint clamps route through its operations, styled properties, and bound `ICommand`s.

[RAIL_LAW]:
- Package: `PanAndZoom`
- Owns: the viewport affine, input gestures (mouse/wheel/keyboard/touch), stretch fitting, constraint clamps, discrete-zoom and grid ladders, view history, named saved views, the `ICommand` rail, exportable `ZoomBorderState`, accessibility descriptions, and the zoom indicator.
- Accept: viewport intent as `ZoomBorder` operations, styled properties, bound `ICommand`s, and `ExportState`/`ImportState` round-trips; coordinate mapping through `ViewportToContent`/`GetContentToScreenMatrix`.
- Reject: a hand-rolled `MatrixTransform` pan/zoom on the render control; direct mutation of `ZoomX`/`OffsetX`; scraping private transform fields for persistence; a parallel saved-view store.
