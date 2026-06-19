# [RASM_APPUI_API_PANANDZOOM]

`PanAndZoom` supplies the `ZoomBorder` viewport control with matrix-driven pan, zoom, rotation, stretch modes, view history, saved views, and zoom indicators.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `PanAndZoom`
- package: `PanAndZoom`
- assembly: `PanAndZoom`
- namespace: `Avalonia.Controls.PanAndZoom`
- asset: runtime library
- rail: viewport

## [02]-[PUBLIC_TYPES]

[VIEWPORT_TYPES]: viewport control and state
- rail: viewport

| [INDEX] | [SYMBOL]            | [RAIL]           |
| :-----: | :------------------ | :--------------- |
|  [01]   | `ZoomBorder`        | viewport control |
|  [02]   | `ZoomBorderState`   | exportable state |
|  [03]   | `ZoomBorderCommand` | command surface  |
|  [04]   | `ViewState`         | view snapshot    |
|  [05]   | `SavedView`         | named view       |
|  [06]   | `MatrixHelper`      | matrix algebra   |

[MODE_TYPES]: behavior vocabularies
- rail: viewport

| [INDEX] | [SYMBOL]                | [RAIL]              |
| :-----: | :---------------------- | :------------------ |
|  [01]   | `StretchMode`           | stretch policy      |
|  [02]   | `ButtonName`            | pan button          |
|  [03]   | `WheelBehaviorMode`     | wheel policy        |
|  [04]   | `DoubleClickZoomMode`   | double-click policy |
|  [05]   | `ContentBoundsMode`     | bounds policy       |
|  [06]   | `ResizeBehaviorMode`    | resize policy       |
|  [07]   | `ZoomIndicatorPosition` | indicator anchor    |

[EVENT_TYPES]: change notification surfaces
- rail: viewport

| [INDEX] | [SYMBOL]                      | [RAIL]         |
| :-----: | :---------------------------- | :------------- |
|  [01]   | `ZoomChangedEventArgs`        | zoom change    |
|  [02]   | `PanEventArgs`                | pan change     |
|  [03]   | `MatrixChangedEventArgs`      | matrix change  |
|  [04]   | `GestureEventArgs`            | gesture input  |
|  [05]   | `StretchModeChangedEventArgs` | stretch change |
|  [06]   | `ZoomEventArgs`               | zoom input     |

## [03]-[ENTRYPOINTS]

[ZOOM_ENTRYPOINTS]: zoom and pan operations
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                      | [RAIL]         |
| :-----: | :----------------------------- | :------------- |
|  [01]   | `ZoomTo`                       | ratio zoom     |
|  [02]   | `ZoomDeltaTo`                  | delta zoom     |
|  [03]   | `ZoomIn` / `ZoomOut`           | stepped zoom   |
|  [04]   | `ZoomToLevel`                  | discrete level |
|  [05]   | `ZoomToRectangle`              | rect focus     |
|  [06]   | `Pan` / `PanDelta`             | pan move       |
|  [07]   | `BeginPanTo` / `ContinuePanTo` | drag pan       |
|  [08]   | `CenterOn`                     | center focus   |

[LAYOUT_ENTRYPOINTS]: stretch, matrix, and rotation operations
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                        | [RAIL]         |
| :-----: | :------------------------------- | :------------- |
|  [01]   | `AutoFit`                        | fit content    |
|  [02]   | `Uniform` / `UniformToFill`      | stretch apply  |
|  [03]   | `Fill` / `None`                  | stretch apply  |
|  [04]   | `ToggleStretchMode`              | mode cycle     |
|  [05]   | `SetMatrix` / `ResetMatrix`      | matrix control |
|  [06]   | `Rotate` / `RotateAt`            | rotation       |
|  [07]   | `ResetRotation` / `SnapRotation` | rotation snap  |

[STATE_ENTRYPOINTS]: history, saved views, and visibility queries
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                               | [RAIL]        |
| :-----: | :-------------------------------------- | :------------ |
|  [01]   | `SaveView` / `RestoreView`              | named views   |
|  [02]   | `DeleteSavedView` / `ClearSavedViews`   | view registry |
|  [03]   | `NavigateBack` / `NavigateForward`      | view history  |
|  [04]   | `ClearViewHistory`                      | history reset |
|  [05]   | `ImportState`                           | state restore |
|  [06]   | `IsPointVisible` / `IsRectangleVisible` | visibility    |
|  [07]   | `GetNextDiscreteZoomLevel`              | level query   |

[PROPERTY_ENTRYPOINTS]: styled behavior properties
- rail: viewport
- surface-root: `ZoomBorder`

| [INDEX] | [SURFACE]                                     | [RAIL]         |
| :-----: | :-------------------------------------------- | :------------- |
|  [01]   | `Stretch`                                     | stretch mode   |
|  [02]   | `PanButton`                                   | pan trigger    |
|  [03]   | `ZoomSpeed`                                   | wheel speed    |
|  [04]   | `ZoomX` / `ZoomY` / `OffsetX` / `OffsetY`     | live transform |
|  [05]   | `EnablePan` / `EnableZoom` / `EnableGestures` | input gates    |
|  [06]   | `EnableConstrains` + `MinZoomX`..`MaxOffsetY` | bounds clamps  |
|  [07]   | `EnableAnimations` / `AnimationDuration`      | transitions    |
|  [08]   | `ShowZoomIndicator` / `ZoomIndicatorPosition` | indicator      |

## [04]-[IMPLEMENTATION_LAW]

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
