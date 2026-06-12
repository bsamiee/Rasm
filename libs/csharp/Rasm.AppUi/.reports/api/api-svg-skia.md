# [RASM_APPUI_API_SVG_SKIA]

`Svg.Controls.Skia.Avalonia` supplies Avalonia SVG controls backed by Svg.Skia document loading, scene graph rendering, animation invalidation, and Skia picture output.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Svg.Controls.Skia.Avalonia`
- package: `Svg.Controls.Skia.Avalonia`
- assembly: `Svg.Controls.Skia.Avalonia`
- assembly: `Svg.Skia`
- namespace: `Svg.Controls.Skia.Avalonia`
- namespace: `Avalonia.Svg.Skia`
- namespace: `Svg.Skia`
- asset: runtime library
- asset: restored SVG rendering library
- rail: assets

## [2]-[PUBLIC_TYPES]

[AVALONIA_SVG_TYPES]: Avalonia control and image surfaces
- rail: assets

| [INDEX] | [SYMBOL]                       | [RAIL]         |
| :-----: | :----------------------------- | :------------- |
|   [1]   | `Svg`                          | SVG control    |
|   [2]   | `SvgImage`                     | image surface  |
|   [3]   | `SvgSource`                    | SVG source     |
|   [4]   | `SvgResourceExtension`         | XAML resource  |
|   [5]   | `SvgImageExtension`            | XAML image     |
|   [6]   | `SvgSourceTypeConverter`       | source convert |
|   [7]   | `SvgCustomDrawOperation`       | draw operation |
|   [8]   | `SvgSourceCustomDrawOperation` | source draw    |

[SVG_SKIA_TYPES]: document, scene, animation, and rendering surfaces
- rail: assets

| [INDEX] | [SYMBOL]                   | [RAIL]                |
| :-----: | :------------------------- | :-------------------- |
|   [1]   | `SKSvg`                    | SVG document          |
|   [2]   | `SKSvgSettings`            | load settings         |
|   [3]   | `SvgDocumentLoadOptions`   | load options          |
|   [4]   | `SvgParameters`            | render parameters     |
|   [5]   | `SvgSceneDocument`         | retained document     |
|   [6]   | `SvgSceneRenderer`         | scene renderer        |
|   [7]   | `SvgAnimationController`   | animation control     |
|   [8]   | `SvgInteractionDispatcher` | interaction dispatch  |
|   [9]   | `SkiaSvgAssetLoader`       | asset loading         |
|  [10]   | `ISvgAssetLoader`          | asset loader contract |

## [3]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: Avalonia SVG control operations
- rail: assets

| [INDEX] | [SURFACE]          | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :----------------- | :------------- | :---------------- |
|   [1]   | `Path`             | `Svg`          | path source       |
|   [2]   | `Source`           | `Svg`          | object source     |
|   [3]   | `Stretch`          | `Svg`          | stretch policy    |
|   [4]   | `StretchDirection` | `Svg`          | stretch direction |
|   [5]   | `SourceRect`       | `Svg`          | source bounds     |
|   [6]   | `Picture`          | `SvgImage`     | picture surface   |
|   [7]   | `SkSvg`            | `SvgImage`     | document surface  |
|   [8]   | `InvalidateVisual` | `Svg`          | redraw request    |

[LOAD_AND_RENDER_ENTRYPOINTS]: document load and render operations
- rail: assets

| [INDEX] | [SURFACE]               | [SURFACE_ROOT]      | [RAIL]          |
| :-----: | :---------------------- | :------------------ | :-------------- |
|   [1]   | `LoadFromSourceAsync`   | `Svg`               | async load      |
|   [2]   | `LoadFromPath`          | `Svg`               | path load       |
|   [3]   | `LoadFromStream`        | `Svg`               | stream load     |
|   [4]   | `LoadSvgDocument`       | `SKSvg`             | document load   |
|   [5]   | `CreateFromSvgDocument` | `SKSvg`             | document create |
|   [6]   | `RenderSvgDocument`     | `SKSvg`             | document render |
|   [7]   | `TryGetPicturePoint`    | `SKSvg`             | hit coordinates |
|   [8]   | `DrawPicture`           | SVG draw operations | picture draw    |

[SCENE_ENTRYPOINTS]: retained scene and animation operations
- rail: assets

| [INDEX] | [SURFACE]                                | [SURFACE_ROOT] | [RAIL]             |
| :-----: | :--------------------------------------- | :------------- | :----------------- |
|   [1]   | `TryEnsureRetainedSceneGraph`            | `SKSvg`        | scene build        |
|   [2]   | `InvalidateRetainedSceneGraph`           | `SKSvg`        | scene invalidation |
|   [3]   | `TryApplyRetainedSceneMutationAndRender` | `SKSvg`        | scene mutation     |
|   [4]   | `DrawAnimationLayer`                     | `SKSvg`        | animation draw     |
|   [5]   | `AnimationInvalidated`                   | `Svg`          | animation event    |
|   [6]   | `DispatchSvgMouseEvent`                  | `SKSvg`        | pointer dispatch   |
|   [7]   | `HitTestSceneNodes`                      | `SKSvg`        | hit testing        |

## [4]-[IMPLEMENTATION_LAW]

[SVG_ASSET_LAW]:
- Package: `Svg.Controls.Skia.Avalonia`
- Owns: SVG asset controls, source loading, retained scene rendering, animation invalidation, and Skia picture output
- Accept: SVG assets enter the same asset rail as raster and generated visual evidence
- Reject: bitmap-only asset policy

[RENDER_LAW]:
- Package: `Svg.Controls.Skia.Avalonia`
- Owns: SVG rendering for panels, companion windows, sidecars, diagnostics, support views, and downstream shells
- Accept: vector assets retain document, scene, animation, interaction, and rendered-picture state
- Reject: opaque SVG blobs without renderable state
