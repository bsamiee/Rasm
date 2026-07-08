# [RASM_APPUI_API_PANANDZOOM]

`PanAndZoom` supplies the `ZoomBorder` Avalonia `Decorator` viewport control: a matrix-driven pan/zoom/rotate surface with constraint clamps, stretch fitting, discrete zoom levels, view history, named saved views, snap-to-grid, keyboard navigation, an `ICommand` MVVM rail, exportable `ZoomBorderState`, accessibility descriptions, and zoom indicators. It is the host-side viewport frame the `Wgpu`/Skia render surface mounts inside — `ZoomBorder` owns the affine `Matrix` and the input gestures, the child content owns the pixels.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PanAndZoom`
- package: `PanAndZoom` (MIT, © wieslawsoltes)
- assembly: `PanAndZoom`
- namespace: `Avalonia.Controls.PanAndZoom`
- asset: managed runtime library (`lib/net10.0` binds the `net10.0` consumer; `lib/net8.0` fallback)
- depends: `Avalonia` — `ZoomBorder` is `Decorator, ILogicalScrollable`; types are `StyledProperty`/`DirectProperty`/`Matrix`/`Point`/`Rect`/`Vector`/`Thickness`/`ICommand`-shaped
- rail: viewport

## [02]-[PUBLIC_TYPES]

[VIEWPORT_TYPES]: viewport control, state carriers, and matrix algebra
- rail: viewport

| [INDEX] | [SYMBOL]            | [SHAPE]                       | [CAPABILITY]                                                              |
| :-----: | :------------------ | :---------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `ZoomBorder`        | `Decorator, ILogicalScrollable` | the viewport control: owns `Matrix`, input, history, indicators, commands |
|  [02]   | `ZoomBorderState`   | mutable class                 | full export carrier: `Matrix`/`Stretch`/`Rotation`/clamps/anim + `Timestamp` |
|  [03]   | `SavedView`         | mutable struct                | named snapshot: `Name`/`Matrix`/`Stretch`/`Description`/`Timestamp`       |
|  [04]   | `ViewState`         | mutable struct                | history entry: `Matrix`/`Stretch`/`Timestamp`                            |
|  [05]   | `ZoomBorderCommand` | `ICommand`                    | `(Action execute, Func<bool>? canExecute)` relay-command for the bound commands |
|  [06]   | `MatrixHelper`      | static algebra                | `Translate`/`Scale`/`Rotation`/`ScaleAt`/`TransformPoint` matrix builders |

[MODE_TYPES]: behavior vocabularies (enums)
- rail: viewport

| [INDEX] | [SYMBOL]                | [MEMBERS]                                            | [CAPABILITY]                                  |
| :-----: | :---------------------- | :-------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `StretchMode`           | `None`/`Fill`/`Uniform`/`UniformToFill`             | content-fit policy (matches Avalonia `Stretch`) |
|  [02]   | `ButtonName`            | `Left`/`Right`/`Middle`                             | pan-drag mouse button                         |
|  [03]   | `WheelBehaviorMode`     | `Zoom`/`PanVertical`/`PanHorizontal`/`None`         | base + `WheelWithCtrl`/`WheelWithShift` modal wheel |
|  [04]   | `DoubleClickZoomMode`   | `ZoomIn`/`ZoomOut`/`ZoomInOut`/`ZoomToFit`/`None`   | double-click intent                           |
|  [05]   | `ContentBoundsMode`     | `Unrestricted`/`KeepContentVisible`/`FillViewport`/`KeepCentered`/`Custom` | feeds `BoundsMode`/`BoundsPadding` |
|  [06]   | `ResizeBehaviorMode`    | `None`/`MaintainCenter`/`MaintainTopLeft`/`MaintainZoom`/`ReapplyStretch`/`Custom` | feeds `ResizeBehavior` |
|  [07]   | `ZoomIndicatorPosition` | `TopLeft`/`TopRight`/`BottomLeft`/`BottomRight`/`Custom` | indicator placement                       |

[EVENT_TYPES]: change-notification args and delegate handlers
- rail: viewport

| [INDEX] | [SYMBOL]                                                  | [KIND]    | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------------- | :-------- | :------------------------------------------ |
|  [01]   | `ZoomChangedEventArgs` / `ZoomChangedEventHandler`        | args + delegate | `ZoomChanged` (ZoomX/Y, OffsetX/Y)    |
|  [02]   | `ZoomEventArgs` / `ZoomEventHandler`                      | args + delegate | `ZoomStarted`/`ZoomEnded`/`ZoomDeltaChanged` |
|  [03]   | `PanEventArgs` / `PanEventHandler`                        | args + delegate | `PanStarted`/`PanContinued`/`PanEnded`  |
|  [04]   | `MatrixChangedEventArgs` / `MatrixChangedEventHandler`    | args + delegate | `MatrixChanged`/`MatrixReset`         |
|  [05]   | `StretchModeChangedEventArgs` / `StretchModeChangedEventHandler` | args + delegate | `StretchModeChanged`/`AutoFitApplied` |
|  [06]   | `GestureEventArgs` / `GestureEventHandler`                | args + delegate | `GestureStarted`/`GestureEnded` (touch) |

## [03]-[ENTRYPOINTS]

[ZOOM_ENTRYPOINTS]: zoom, pan, and drag operations
- rail: viewport
- surface-root: `ZoomBorder` (all take `bool skipTransitions = false` unless noted)

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `Zoom(double zoom, double x, double y, bool)`          | absolute zoom about content point             |
|  [02]   | `ZoomTo(double ratio, double x, double y, bool)`       | ratio-relative zoom about `(x,y)`             |
|  [03]   | `ZoomDeltaTo(double delta, double x, double y, bool)`  | wheel-delta zoom (uses `ZoomSpeed`/`PowerFactor`) |
|  [04]   | `ZoomIn(bool)` / `ZoomOut(bool)`                       | stepped zoom about viewport center            |
|  [05]   | `ZoomToLevel(double level, double centerX, double centerY, bool)` | jump to an absolute zoom level     |
|  [06]   | `ZoomToRectangle(Rect rect, Thickness? padding, bool)` / `ZoomToRectangleExact(Rect rect, Rect viewportRect, bool)` | rubber-band rect focus |
|  [07]   | `Pan(double x, double y, bool)` / `PanDelta(double dx, double dy, bool)` | absolute / relative pan       |
|  [08]   | `BeginPanTo(double x, double y)` / `ContinuePanTo(double x, double y, bool)` | drag-pan begin / continue   |
|  [09]   | `CenterOn(Point\|Rect\|Control, [double zoom], bool animate)` | center on point/rect/element, optional zoom |

[LAYOUT_ENTRYPOINTS]: stretch fitting, matrix, and rotation
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `AutoFit(double panelW, panelH, elementW, elementH, bool)` / `AutoFit(bool)` | fit child into panel (raises `AutoFitApplied`) |
|  [02]   | `Uniform`/`UniformToFill`/`Fill`/`None` (sized + `(bool)` overloads)        | apply one `StretchMode` fit                   |
|  [03]   | `ToggleStretchMode()`                                                       | cycle to the next `StretchMode`               |
|  [04]   | `SetMatrix(Matrix matrix, bool skipTransitions)` / `ResetMatrix([bool])`    | direct matrix set / reset to identity         |
|  [05]   | `Rotate(double degrees, bool animate)` / `RotateAt(double degrees, Point center, bool animate)` | rotate about center / arbitrary point |
|  [06]   | `ResetRotation(bool animate)` / `SnapRotation()`                            | clear / snap rotation to `RotationSnapAngle`  |

[TRANSFORM_ENTRYPOINTS]: coordinate mapping and visible-bounds queries
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `ViewportToContent(Point\|Rect)` / `ContentToViewport(Point\|Rect)` | map viewport <-> content space                 |
|  [02]   | `ScreenToContent(Vector)` / `ContentToScreen(Vector)`              | map screen-delta <-> content-delta vectors     |
|  [03]   | `GetContentToScreenMatrix()` / `GetScreenToContentMatrix()`        | the live affine + its inverse (`Matrix`)       |
|  [04]   | `GetVisibleContentBounds()` / `GetViewportBounds()`               | visible content `Rect` / viewport `Rect`       |
|  [05]   | `IsPointVisible(Point)` / `IsRectangleVisible(Rect)`              | hit-test against the visible content bounds    |

[STATE_ENTRYPOINTS]: saved views, history, discrete levels, grid, and state export
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                                                                  | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `SaveView(string name, string? description)` / `RestoreView(string name, bool animate) -> bool` | persist / recall a named `SavedView` |
|  [02]   | `GetSavedView(string) -> SavedView?` / `GetSavedViews() -> IReadOnlyCollection<SavedView>` / `GetSavedViewNames() -> string[]` | enumerate the view registry |
|  [03]   | `DeleteSavedView(string) -> bool` / `ClearSavedViews()`                     | mutate the view registry                       |
|  [04]   | `NavigateBack(bool animate)` / `NavigateForward(bool animate)` / `ClearViewHistory()` | undo/redo through `ViewState` history (`ViewHistorySize`) |
|  [05]   | `GetNextDiscreteZoomLevel()` / `GetPreviousDiscreteZoomLevel() -> double`  | step through `DiscreteZoomLevels` (gated by `EnableDiscreteZoomLevels`) |
|  [06]   | `SnapToGrid(double value) -> double`                                       | snap a coordinate to `GridSize` (gated by `EnableSnapToGrid`) |
|  [07]   | `ExportState() -> ZoomBorderState` / `ImportState(ZoomBorderState state, bool animate)` | round-trip the full viewport state |
|  [08]   | `UpdateAccessibilityDescriptions()`                                        | refresh `ZoomLevelDescription`/`PanPositionDescription` |

[COMMAND_ENTRYPOINTS]: `ICommand` MVVM bindings (no code-behind)
- rail: viewport
- surface-root: `ZoomBorder` (each a `DirectProperty<ZoomBorder, ICommand>`, lazily a `ZoomBorderCommand`)

| [INDEX] | [SURFACE]                                                                  | [BINDS]                                        |
| :-----: | :------------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `ZoomInCommand` / `ZoomOutCommand` / `ResetCommand`                        | `ZoomIn`/`ZoomOut`/reset matrix                |
|  [02]   | `FitCommand` / `FillCommand` / `UniformCommand` / `UniformToFillCommand` / `ToggleStretchCommand` | stretch fits + `ToggleStretchMode` |
|  [03]   | `NavigateBackCommand` / `NavigateForwardCommand`                          | history undo/redo                              |

[PROPERTY_ENTRYPOINTS]: styled behavior properties (each a `StyledProperty`/`DirectProperty` for XAML + binding)
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                                                                  | [TYPE]                                         |
| :-----: | :------------------------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `Stretch` / `PanButton` / `ZoomSpeed` / `PowerFactor` / `TransitionThreshold` | `StretchMode`/`ButtonName`/`double`×3        |
|  [02]   | `ZoomX` / `ZoomY` / `OffsetX` / `OffsetY` / `Matrix`                       | live read-only transform (`DirectProperty` + `Matrix`) |
|  [03]   | `EnablePan` / `EnableZoom` / `EnableGestures` / `EnableGestureZoom` / `EnableGestureRotation` / `EnableGestureTranslation` | input gates (`bool`) |
|  [04]   | `EnableConstrains` + `MinZoomX`..`MaxZoomY` + `MinOffsetX`..`MaxOffsetY` + `AutoCalculateMinZoom`/`AutoCalculateMaxZoom` + `MaxZoomPixelSize` | clamp policy |
|  [05]   | `BoundsMode` / `BoundsPadding` / `MinimumVisibleContentPercentage` / `ResizeBehavior` / `CenterPadding` | content-bounds + resize policy |
|  [06]   | `WheelBehavior` / `WheelWithCtrl` / `WheelWithShift` / `WheelZoomSensitivity` / `WheelPanSensitivity` | modal wheel routing |
|  [07]   | `EnableKeyboardNavigation` / `KeyboardPanStep` / `KeyboardZoomStep`         | keyboard navigation                            |
|  [08]   | `EnableAnimations` / `AnimationDuration` (`TimeSpan`)                       | transition animation                           |
|  [09]   | `EnableDoubleClickZoom` / `DoubleClickZoomMode` / `DoubleClickZoomFactor`   | double-click zoom                              |
|  [10]   | `EnableDiscreteZoomLevels` / `DiscreteZoomLevels` (`double[]?`)             | discrete zoom ladder                           |
|  [11]   | `EnableViewHistory` / `ViewHistorySize`                                     | undo/redo history depth                        |
|  [12]   | `ShowGrid` / `EnableSnapToGrid` / `GridSize` / `GridBrush` / `GridThickness` / `GridOpacity` / `MajorGridInterval`/`MajorGridBrush`/`MajorGridThickness` | overlay grid + snap |
|  [13]   | `Rotation` / `MinRotation` / `MaxRotation` / `EnableRotationSnapping` / `RotationSnapAngle` | rotation policy                  |
|  [14]   | `ShowZoomIndicator` / `ZoomIndicatorPosition` / `ZoomIndicatorFormat` / `ZoomIndicatorAutoHideDuration` / `IsZoomIndicatorVisible` | zoom HUD |
|  [15]   | `EnableSimultaneousPanZoom` / `MinimumTouchPoints` / `MaximumTouchPoints`   | multi-touch gesture policy                     |
|  [16]   | `CanHorizontallyScroll` / `CanVerticallyScroll` / `CanNavigateBack` / `CanNavigateForward` | `ILogicalScrollable` + history query |
|  [17]   | `ZoomLevelDescription` / `PanPositionDescription` / `UseHighContrastMode`   | accessibility surface                          |

## [04]-[IMPLEMENTATION_LAW]

[VIEWPORT_TOPOLOGY]:
- `ZoomBorder` is an Avalonia `Decorator`: it wraps one `Child` and applies the affine `Matrix` to it. The render surface (`Wgpu`/`Skia` swap-chain control, `api-silk-webgpu-wgpu.md`) is the `Child`; `ZoomBorder` owns the camera transform and the child owns the pixels — never compose a hand-rolled `MatrixTransform` on the render control when `ZoomBorder` already owns the affine.
- The live transform is the read-only `Matrix`/`ZoomX`/`ZoomY`/`OffsetX`/`OffsetY` (`DirectProperty`); mutation flows only through the operation methods (`Zoom`/`Pan`/`Rotate`/`SetMatrix`/stretch fits), which raise the matching events. Bind `ZoomX`/`OffsetX` one-way to a status readout; never set them directly.
- Coordinate mapping is the seam between input/picking and the rendered scene: `ViewportToContent`/`ScreenToContent` map a pointer hit or a screen-delta into content space for selection and measurement; `GetContentToScreenMatrix()` and its inverse expose the affine for a custom overlay (rulers, snap dots) that must track the camera. Pick in content space, render the overlay in viewport space.

[STATE_AND_HISTORY]:
- `ZoomBorderState` is the durable carrier: `ExportState()` snapshots `Matrix`/`Stretch`/`Rotation`/clamps/anim + `Timestamp`; `ImportState(state, animate)` restores it. Persist that struct through the same `System.Text.Json`/Thinktecture-JSON rail the rest of AppUi state uses (`libs/csharp/.api/api-thinktecture-json.md`, shared tier) — `SavedView`/`ViewState` are plain mutable structs with public setters, so they serialize directly. Never scrape private transform fields.
- View history (`NavigateBack`/`NavigateForward` over a `ViewHistorySize`-bounded `ViewState` ring) and named `SavedView`s are two distinct registries: history is the implicit undo stack auto-pushed on view change (`ViewHistoryChanged` fires); saved views are explicit named bookmarks (`SaveView`/`RestoreView`).
- Discrete zoom (`DiscreteZoomLevels` + `GetNext`/`GetPreviousDiscreteZoomLevel`) and snap-to-grid (`GridSize` + `SnapToGrid`) are opt-in ladders gated by `EnableDiscreteZoomLevels`/`EnableSnapToGrid`; with both off the control is continuous.

[STACKING]:
- ReactiveUI (`api-reactiveui-avalonia.md`): the ten `ICommand` properties (`ZoomInCommand`/`FitCommand`/`NavigateBackCommand`/…) bind straight to toolbar buttons in XAML — drive zoom/fit/history from the view-model with zero code-behind. For richer reactive flows wrap a `ReactiveCommand` and call the imperative method, or observe the change events (`ZoomChanged`/`MatrixChanged`/`StretchModeChanged`) as `IObservable` via `Observable.FromEventPattern` and project into the view-model.
- The change events carry the post-change transform; a measurement/annotation tool subscribes `MatrixChanged`/`ZoomChanged` to invalidate its overlay cache, and `GestureStarted`/`GestureEnded` to gate live picking during a touch gesture.
- `UnitsNet` (`libs/csharp/.api/api-unitsnet.md`, shared tier): when the viewport content is real-world geometry, map a content-space `Rect` through `GetVisibleContentBounds()` and project the extents into a `Length` for an on-screen scale-bar — the affine is the px/content ratio, `Length.ToUnit(UnitSystem.SI)` formats the bar label.
- Avalonia accessibility: `ZoomLevelDescription`/`PanPositionDescription` feed the automation peer; call `UpdateAccessibilityDescriptions()` after a programmatic view change so screen readers track the camera, and honor `UseHighContrastMode` for the grid/indicator brushes.

[VIEWPORT_LAW]:
- Package: `PanAndZoom`
- Owns: the viewport affine transform, input gestures (mouse/wheel/keyboard/touch), stretch fitting, constraint clamps, discrete-zoom and grid ladders, view history, named saved views, the `ICommand` rail, exportable `ZoomBorderState`, accessibility descriptions, and the zoom indicator.
- Accept: viewport intent expressed as `ZoomBorder` operations, styled properties, bound `ICommand`s, and `ExportState`/`ImportState` round-trips; coordinate mapping through `ViewportToContent`/`GetContentToScreenMatrix`.
- Reject: a hand-rolled `MatrixTransform` pan/zoom on the render control; direct mutation of `ZoomX`/`OffsetX`; scraping private transform fields for persistence; a parallel saved-view store when the registry already owns it.
