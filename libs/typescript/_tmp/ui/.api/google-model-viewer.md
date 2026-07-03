# [API_CATALOGUE] @google/model-viewer

`@google/model-viewer` supplies a declarative custom element (`<model-viewer>`) that composes animation, annotation/hotspot, AR (Quick Look / Scene Viewer / WebXR), camera controls, environment lighting, loading management, and scene-graph mutation APIs over a three.js renderer for the `ui` stack.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@google/model-viewer`
- package: `@google/model-viewer`
- module: `@google/model-viewer` (typed by `lib/model-viewer.d.ts`)
- asset: `<model-viewer>` custom element, mixin feature interfaces, scene-graph API
- rail: render

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: custom element
- rail: render

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [RAIL]                           |
| :-----: | :------------------- | :----------------- | :------------------------------- |
|  [01]   | `ModelViewerElement` | custom element     | composed from all feature mixins |
|  [02]   | `Vector3D`           | 3D coordinate type | `{ x, y, z, toString() }`        |
|  [03]   | `Vector2D`           | 2D UV type         | `{ u, v, toString() }`           |
|  [04]   | `RGB`                | color re-export    | re-exported from glTF 2.0 types  |
|  [05]   | `RGBA`               | color + alpha      | re-exported from glTF 2.0 types  |

[PUBLIC_TYPE_SCOPE]: animation feature
- rail: render

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [RAIL]                                   |
| :-----: | :----------------------- | :---------------- | :--------------------------------------- |
|  [01]   | `AnimationInterface`     | feature interface | playback control and state properties    |
|  [02]   | `PlayAnimationOptions`   | play config       | `repetitions`, `pingpong`, `modelIndex?` |
|  [03]   | `AppendAnimationOptions` | append config     | `fade`, `warp`, `weight`, `timeScale`    |
|  [04]   | `DetachAnimationOptions` | detach config     | `fade`, `modelIndex?`                    |

[PUBLIC_TYPE_SCOPE]: annotation / hotspot feature
- rail: render

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [RAIL]                                                 |
| :-----: | :-------------------- | :---------------- | :----------------------------------------------------- |
|  [01]   | `AnnotationInterface` | feature interface | hotspot query and surface intersection                 |
|  [02]   | `HotspotData`         | query result      | `position`, `normal`, `canvasPosition`, `facingCamera` |

[PUBLIC_TYPE_SCOPE]: AR feature
- rail: render

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]     | [RAIL]                                                |
| :-----: | :---------------- | :---------------- | :---------------------------------------------------- |
|  [01]   | `ARInterface`     | feature interface | AR activation, mode, scale, placement properties      |
|  [02]   | `ARMode`          | mode union        | `'quick-look' \| 'scene-viewer' \| 'webxr' \| 'none'` |
|  [03]   | `ARStatusDetails` | event payload     | `status: ARStatus`                                    |

[PUBLIC_TYPE_SCOPE]: camera controls feature
- rail: render

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [RAIL]                                       |
| :-----: | :-------------------------- | :---------------- | :------------------------------------------- |
|  [01]   | `ControlsInterface`         | feature interface | orbit, target, FOV, interaction prompt props |
|  [02]   | `SphericalPosition`         | camera position   | `theta`, `phi`, `radius`, `toString()`       |
|  [03]   | `CameraChangeDetails`       | event payload     | `source: ChangeSource`                       |
|  [04]   | `InteractionPromptStrategy` | strategy enum     | `'auto' \| 'none'`                           |
|  [05]   | `InteractionPromptStyle`    | style enum        | `'basic' \| 'wiggle'`                        |
|  [06]   | `TouchAction`               | touch mode        | `'pan-y' \| 'pan-x' \| 'none'`               |

[PUBLIC_TYPE_SCOPE]: environment feature
- rail: render

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [RAIL]                                          |
| :-----: | :--------------------- | :---------------- | :---------------------------------------------- |
|  [01]   | `EnvironmentInterface` | feature interface | IBL image, skybox, shadow, exposure properties  |
|  [02]   | `ToneMappingValue`     | tone map union    | `'auto' \| 'aces' \| 'agx' \| 'neutral' \| ...` |

[PUBLIC_TYPE_SCOPE]: loading feature
- rail: render

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [RAIL]                                      |
| :-----: | :------------------------ | :---------------- | :------------------------------------------ |
|  [01]   | `LoadingInterface`        | feature interface | poster, reveal, dimensions, bounding center |
|  [02]   | `LoadingStaticInterface`  | static config     | decoder URLs, `mapURLs` callback            |
|  [03]   | `RevealAttributeValue`    | reveal mode       | `'auto' \| 'manual'`                        |
|  [04]   | `LoadingAttributeValue`   | loading mode      | `'auto' \| 'lazy' \| 'eager'`               |
|  [05]   | `ModelViewerGlobalConfig` | global config     | `dracoDecoderLocation`, power preference    |

[PUBLIC_TYPE_SCOPE]: scene graph feature
- rail: render

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [RAIL]                                           |
| :-----: | :-------------------- | :---------------- | :----------------------------------------------- |
|  [01]   | `SceneGraphInterface` | feature interface | model access, variant, orientation, export       |
|  [02]   | `SceneExportOptions`  | export config     | `binary`, `trs`, `onlyVisible`, `maxTextureSize` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: AnimationInterface — playback
- rail: render

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------- | :------------- | :------------------------------ |
|  [01]   | `play(options?)`                  | playback       | start animation                 |
|  [02]   | `pause()`                         | playback       | freeze animation                |
|  [03]   | `appendAnimation(name, options?)` | blend          | blend additional animation clip |
|  [04]   | `detachAnimation(name, options?)` | blend          | remove blended clip             |
|  [05]   | `.animationName`                  | property       | select active animation by name |
|  [06]   | `.availableAnimations` (readonly) | property       | array of clip names from model  |
|  [07]   | `.currentTime`                    | property       | seek to playback position       |
|  [08]   | `.timeScale`                      | property       | playback speed multiplier       |

[ENTRYPOINT_SCOPE]: AnnotationInterface — hotspot and ray-surface
- rail: render

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `updateHotspot(config)`                      | hotspot write  | update hotspot position/normal      |
|  [02]   | `queryHotspot(name)`                         | hotspot read   | returns `HotspotData \| null`       |
|  [03]   | `positionAndNormalFromPoint(pixelX, pixelY)` | ray cast       | screen pixel -> 3D surface hit      |
|  [04]   | `surfaceFromPoint(pixelX, pixelY)`           | ray cast       | screen pixel -> surface name string |

[ENTRYPOINT_SCOPE]: ARInterface — augmented reality
- rail: render

| [INDEX] | [SURFACE]        | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :--------------- | :------------- | :---------------------------- |
|  [01]   | `activateAR()`   | AR entry       | launch AR viewer session      |
|  [02]   | `.canActivateAR` | capability     | `true` when AR is available   |
|  [03]   | `.arModes`       | property       | space-separated mode priority |
|  [04]   | `.iosSrc`        | property       | USDZ source for Quick Look    |

[ENTRYPOINT_SCOPE]: ControlsInterface — camera
- rail: render

| [INDEX] | [SURFACE]            | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------- | :------------- | :------------------------------- |
|  [01]   | `.cameraOrbit`       | property       | spherical orbit string `"θ φ r"` |
|  [02]   | `.cameraTarget`      | property       | look-at point string `"x y z"`   |
|  [03]   | `.fieldOfView`       | property       | FOV string with units            |
|  [04]   | `.getCameraOrbit()`  | orbit query    | current `SphericalPosition`      |
|  [05]   | `.getCameraTarget()` | target query   | current `Vector3D` look-at       |
|  [06]   | `.cameraControls`    | property       | enable/disable orbit controls    |

[ENTRYPOINT_SCOPE]: LoadingInterface — model loading
- rail: render

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :----------------------- | :------------- | :------------------------------- |
|  [01]   | `dismissPoster()`        | reveal control | hide poster and reveal model     |
|  [02]   | `showPoster()`           | reveal control | show poster, hide model          |
|  [03]   | `getDimensions()`        | bounds query   | model bounding box as `Vector3D` |
|  [04]   | `getBoundingBoxCenter()` | bounds query   | model center as `Vector3D`       |
|  [05]   | `.loaded` (readonly)     | state          | `true` when model is loaded      |

[ENTRYPOINT_SCOPE]: SceneGraphInterface — model mutation
- rail: render

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [RAIL]                                |
| :-----: | :---------------------------------- | :-------------- | :------------------------------------ |
|  [01]   | `exportScene(options?)`             | scene export    | returns `Promise<Blob>` (glTF/GLB)    |
|  [02]   | `createTexture(uri, type?)`         | texture factory | `Promise<ModelViewerTexture \| null>` |
|  [03]   | `createCanvasTexture()`             | texture factory | canvas-backed `ModelViewerTexture`    |
|  [04]   | `createVideoTexture(uri)`           | texture factory | video-backed `ModelViewerTexture`     |
|  [05]   | `materialFromPoint(pixelX, pixelY)` | ray cast        | `Material \| null` at screen pixel    |
|  [06]   | `.model` (readonly)                 | property        | `Model` scene-graph root              |
|  [07]   | `.availableVariants` (readonly)     | property        | glTF KHR_materials_variants list      |
|  [08]   | `.variantName`                      | property        | activate named material variant       |

[ENTRYPOINT_SCOPE]: EnvironmentInterface — lighting
- rail: render

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------ | :------------- | :---------------------------------- |
|  [01]   | `.environmentImage` | property       | HDR/EXR/JPG IBL environment URL     |
|  [02]   | `.skyboxImage`      | property       | skybox background image URL         |
|  [03]   | `.shadowIntensity`  | property       | contact shadow strength `0`–`1`     |
|  [04]   | `.exposure`         | property       | scene exposure multiplier           |
|  [05]   | `hasBakedShadow()`  | query          | `true` if model has baked occlusion |

## [04]-[IMPLEMENTATION_LAW]

[ELEMENT_TOPOLOGY]:
- Custom element tag: `<model-viewer>` registered via `customElements.define` on module import
- `ModelViewerElement` is a mixin composition of `AnnotationMixin`, `SceneGraphMixin`, `StagingMixin`, `EnvironmentMixin`, `ControlsMixin`, `ARMixin`, `LoadingMixin`, `AnimationMixin`, and `ModelViewerElementBase` (which extends `ReactiveElement`)
- `src` attribute accepts glTF (`.gltf` / `.glb`) URLs; `ios-src` accepts USDZ for Quick Look
- `HTMElementTagNameMap` is augmented: `'model-viewer'` resolves to `ModelViewerElement`
- DRACO / KTX2 / meshopt decoder locations must be configured on `ModelViewerElement` static properties before first element creation

[LOCAL_ADMISSION]:
- Import `@google/model-viewer` once at the entry boundary to trigger custom element registration.
- Decoder URLs (`dracoDecoderLocation`, `ktx2TranscoderLocation`) default to Google CDN; override via `LoadingStaticInterface` statics before rendering any model.
- `exportScene()` returns a GLB `Blob` by default (`binary: true`); pass `{ binary: false }` for glTF + resources.
- Animation `play()` requires `autoplay: false` to be set first when selecting a non-default clip; `animationName` selects the clip before calling `play()`.

[RAIL_LAW]:
- Package: `@google/model-viewer`
- Owns: declarative glTF/GLB viewer, AR activation, IBL environment, camera orbit, animation playback, hotspot annotation, scene-graph mutation, material variants
- Accept: `<model-viewer>` HTML element usage and `ModelViewerElement` TypeScript interface access
- Reject: direct three.js renderer construction for gltf model display when the element interface covers the use case
