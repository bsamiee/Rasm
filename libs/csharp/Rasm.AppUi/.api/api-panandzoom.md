# [RASM_APPUI_API_PANANDZOOM]

`PanAndZoom` supplies the `ZoomBorder` viewport control with matrix-driven pan, zoom, rotation, stretch modes, view history, saved views, and zoom indicators.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PanAndZoom`
- package: `PanAndZoom`
- assembly: `PanAndZoom`
- namespace: `Avalonia.Controls.PanAndZoom`
- asset: runtime library
- rail: viewport

## [2]-[PUBLIC_TYPES]

[VIEWPORT_TYPES]: viewport control and state
- rail: viewport

| [INDEX] | [SYMBOL]            | [RAIL]           |
| :-----: | :------------------ | :--------------- |
|   [1]   | `ZoomBorder`        | viewport control |
|   [2]   | `ZoomBorderState`   | exportable state |
|   [3]   | `ZoomBorderCommand` | command surface  |
|   [4]   | `ViewState`         | view snapshot    |
|   [5]   | `SavedView`         | named view       |
|   [6]   | `MatrixHelper`      | matrix algebra   |

[MODE_TYPES]: behavior vocabularies
- rail: viewport

| [INDEX] | [SYMBOL]                | [RAIL]              |
| :-----: | :---------------------- | :------------------ |
|   [1]   | `StretchMode`           | stretch policy      |
|   [2]   | `ButtonName`            | pan button          |
|   [3]   | `WheelBehaviorMode`     | wheel policy        |
|   [4]   | `DoubleClickZoomMode`   | double-click policy |
|   [5]   | `ContentBoundsMode`     | bounds policy       |
|   [6]   | `ResizeBehaviorMode`    | resize policy       |
|   [7]   | `ZoomIndicatorPosition` | indicator anchor    |

[EVENT_TYPES]: change notification surfaces
- rail: viewport

| [INDEX] | [SYMBOL]                      | [RAIL]         |
| :-----: | :---------------------------- | :------------- |
|   [1]   | `ZoomChangedEventArgs`        | zoom change    |
|   [2]   | `PanEventArgs`                | pan change     |
|   [3]   | `MatrixChangedEventArgs`      | matrix change  |
|   [4]   | `GestureEventArgs`            | gesture input  |
|   [5]   | `StretchModeChangedEventArgs` | stretch change |
|   [6]   | `ZoomEventArgs`               | zoom input     |

## [3]-[ENTRYPOINTS]

[ZOOM_ENTRYPOINTS]: zoom and pan operations
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                      | [RAIL]         |
| :-----: | :----------------------------- | :------------- |
|   [1]   | `ZoomTo`                       | ratio zoom     |
|   [2]   | `ZoomDeltaTo`                  | delta zoom     |
|   [3]   | `ZoomIn` / `ZoomOut`           | stepped zoom   |
|   [4]   | `ZoomToLevel`                  | discrete level |
|   [5]   | `ZoomToRectangle`              | rect focus     |
|   [6]   | `Pan` / `PanDelta`             | pan move       |
|   [7]   | `BeginPanTo` / `ContinuePanTo` | drag pan       |
|   [8]   | `CenterOn`                     | center focus   |

[LAYOUT_ENTRYPOINTS]: stretch, matrix, and rotation operations
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                        | [RAIL]         |
| :-----: | :------------------------------- | :------------- |
|   [1]   | `AutoFit`                        | fit content    |
|   [2]   | `Uniform` / `UniformToFill`      | stretch apply  |
|   [3]   | `Fill` / `None`                  | stretch apply  |
|   [4]   | `ToggleStretchMode`              | mode cycle     |
|   [5]   | `SetMatrix` / `ResetMatrix`      | matrix control |
|   [6]   | `Rotate` / `RotateAt`            | rotation       |
|   [7]   | `ResetRotation` / `SnapRotation` | rotation snap  |

[STATE_ENTRYPOINTS]: history, saved views, and visibility queries
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                               | [RAIL]        |
| :-----: | :-------------------------------------- | :------------ |
|   [1]   | `SaveView` / `RestoreView`              | named views   |
|   [2]   | `DeleteSavedView` / `ClearSavedViews`   | view registry |
|   [3]   | `NavigateBack` / `NavigateForward`      | view history  |
|   [4]   | `ClearViewHistory`                      | history reset |
|   [5]   | `ImportState`                           | state restore |
|   [6]   | `IsPointVisible` / `IsRectangleVisible` | visibility    |
|   [7]   | `GetNextDiscreteZoomLevel`              | level query   |

[PROPERTY_ENTRYPOINTS]: styled behavior properties
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                                     | [RAIL]         |
| :-----: | :-------------------------------------------- | :------------- |
|   [1]   | `Stretch`                                     | stretch mode   |
|   [2]   | `PanButton`                                   | pan trigger    |
|   [3]   | `ZoomSpeed`                                   | wheel speed    |
|   [4]   | `ZoomX` / `ZoomY` / `OffsetX` / `OffsetY`     | live transform |
|   [5]   | `EnablePan` / `EnableZoom` / `EnableGestures` | input gates    |
|   [6]   | `EnableConstrains` + `MinZoomX`..`MaxOffsetY` | bounds clamps  |
|   [7]   | `EnableAnimations` / `AnimationDuration`      | transitions    |
|   [8]   | `ShowZoomIndicator` / `ZoomIndicatorPosition` | indicator      |

## [4]-[IMPLEMENTATION_LAW]

[VIEWPORT_LAW]:
- Package: `PanAndZoom`
- Owns: viewport transform state, input gestures, stretch policy, view history, and indicators
- Accept: viewport intent maps to `ZoomBorder` operations and styled properties
- Reject: hand-rolled matrix pan/zoom on canvas controls

[STATE_LAW]:
- Package: `PanAndZoom`
- Owns: exportable viewport state through `ZoomBorderState`, `SavedView`, and `ImportState`
- Accept: persisted camera state round-trips through the state surfaces
- Reject: scraping private transform fields for persistence
