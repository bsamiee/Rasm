# [RASM_APPUI_API_SVG_SKIA]

`Svg.Controls.Skia.Avalonia` supplies Avalonia SVG controls backed by Svg.Skia document loading, scene graph rendering, animation invalidation, and Skia picture output.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Svg.Controls.Skia.Avalonia`
- package: `Svg.Controls.Skia.Avalonia`
- assembly: `Svg.Controls.Skia.Avalonia`
- assembly: `Svg.Skia`
- assembly: `Svg.SceneGraph`
- namespace: `Avalonia.Svg.Skia`
- namespace: `Svg.Skia`
- namespace: `Svg.Model`
- asset: runtime library
- asset: restored SVG rendering library
- rail: assets

## [02]-[PUBLIC_TYPES]

[AVALONIA_SVG_TYPES]: Avalonia control and image surfaces — rail: assets

| [INDEX] | [SYMBOL]                       | [KIND]         |
| :-----: | :----------------------------- | :------------- |
|  [01]   | `Svg`                          | SVG control    |
|  [02]   | `SvgImage`                     | image surface  |
|  [03]   | `SvgSource`                    | SVG source     |
|  [04]   | `SvgResourceExtension`         | XAML resource  |
|  [05]   | `SvgImageExtension`            | XAML image     |
|  [06]   | `SvgSourceTypeConverter`       | source convert |
|  [07]   | `SvgCustomDrawOperation`       | draw operation |
|  [08]   | `SvgSourceCustomDrawOperation` | source draw    |

[SVG_SKIA_TYPES]: document, scene, animation, and rendering surfaces — rail: assets

| [INDEX] | [SYMBOL]                   | [KIND]                |
| :-----: | :------------------------- | :-------------------- |
|  [01]   | `SKSvg`                    | SVG document          |
|  [02]   | `SKSvgSettings`            | load settings         |
|  [03]   | `SvgSceneNode`             | scene node            |
|  [04]   | `SvgParameters`            | render parameters     |
|  [05]   | `SvgSceneDocument`         | retained document     |
|  [06]   | `SvgSceneRenderer`         | scene renderer        |
|  [07]   | `SvgAnimationController`   | animation control     |
|  [08]   | `SvgInteractionDispatcher` | interaction dispatch  |
|  [09]   | `SkiaSvgAssetLoader`       | asset loading         |
|  [10]   | `ISvgAssetLoader`          | asset loader contract |

## [03]-[ENTRYPOINTS]

[CONTROL_ENTRYPOINTS]: Avalonia SVG control operations
- rail: assets

| [INDEX] | [SURFACE]          | [SURFACE_ROOT] | [RAIL]            |
| :-----: | :----------------- | :------------- | :---------------- |
|  [01]   | `Path`             | `Svg`          | path source       |
|  [02]   | `Source`           | `Svg`          | object source     |
|  [03]   | `Stretch`          | `Svg`          | stretch policy    |
|  [04]   | `StretchDirection` | `Svg`          | stretch direction |
|  [05]   | `Size`             | `SvgImage`     | image size        |
|  [06]   | `Source`           | `SvgImage`     | image source      |
|  [07]   | `CurrentColor`     | `SvgImage`     | color override    |
|  [08]   | `InvalidateVisual` | `Svg`          | redraw request    |

[LOAD_AND_RENDER_ENTRYPOINTS]: document load and render operations
- rail: assets

| [INDEX] | [SURFACE]               | [SURFACE_ROOT]           | [RAIL]          |
| :-----: | :---------------------- | :----------------------- | :-------------- |
|  [01]   | `Load`                  | `SKSvg`                  | stream load     |
|  [02]   | `FromSvg`               | `SKSvg`                  | string load     |
|  [03]   | `CreateFromSvgDocument` | `SKSvg`                  | document create |
|  [04]   | `Save`                  | `SKSvg`                  | encoded output  |
|  [05]   | `Draw`                  | `SKSvg`                  | canvas draw     |
|  [06]   | `Picture`               | `SKSvg`                  | picture surface |
|  [07]   | `TryGetPicturePoint`    | `SKSvg`                  | hit coordinates |
|  [08]   | `Render`                | `SvgCustomDrawOperation` | picture draw    |

[SCENE_ENTRYPOINTS]: retained scene and animation operations
- rail: assets

| [INDEX] | [SURFACE]                     | [SURFACE_ROOT]             | [RAIL]            |
| :-----: | :---------------------------- | :------------------------- | :---------------- |
|  [01]   | `TryEnsureRetainedSceneGraph` | `SKSvg`                    | scene build       |
|  [02]   | `RetainedSceneGraph`          | `SKSvg`                    | scene access      |
|  [03]   | `HasRetainedSceneGraph`       | `SKSvg`                    | scene presence    |
|  [04]   | `AnimationController`         | `SKSvg`                    | animation control |
|  [05]   | `AnimationInvalidated`        | `SKSvg`                    | animation event   |
|  [06]   | `HitTestTopmostElement`       | `SvgInteractionDispatcher` | pointer dispatch  |
|  [07]   | `HitTestSceneNodes`           | `SKSvg`                    | hit testing       |

## [04]-[IMPLEMENTATION_LAW]

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
