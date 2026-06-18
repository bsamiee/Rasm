# [API_CATALOGUE] @google/model-viewer

`@google/model-viewer` supplies a declarative custom element (`<model-viewer>`) that composes animation, annotation/hotspot, AR (Quick Look / Scene Viewer / WebXR), camera controls, environment lighting, loading management, and scene-graph mutation APIs over a three.js renderer for the `ui` stack.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@google/model-viewer`
- package: `@google/model-viewer`
- module: `@google/model-viewer` (typed by `lib/model-viewer.d.ts`)
- asset: `<model-viewer>` custom element, mixin feature interfaces, scene-graph API
- rail: render

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: custom element
- rail: render

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]      | [RAIL]                           |
| :-----: | :------------------- | :----------------- | :------------------------------- |
|   [1]   | `ModelViewerElement` | custom element     | composed from all feature mixins |
|   [2]   | `Vector3D`           | 3D coordinate type | `{ x, y, z, toString() }`        |
|   [3]   | `Vector2D`           | 2D UV type         | `{ u, v, toString() }`           |
|   [4]   | `RGB`                | color re-export    | re-exported from glTF 2.0 types  |
|   [5]   | `RGBA`               | color + alpha      | re-exported from glTF 2.0 types  |

[PUBLIC_TYPE_SCOPE]: animation feature
- rail: render

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [RAIL]                                   |
| :-----: | :----------------------- | :---------------- | :--------------------------------------- |
|   [1]   | `AnimationInterface`     | feature interface | playback control and state properties    |
|   [2]   | `PlayAnimationOptions`   | play config       | `repetitions`, `pingpong`, `modelIndex?` |
|   [3]   | `AppendAnimationOptions` | append config     | `fade`, `warp`, `weight`, `timeScale`    |
|   [4]   | `DetachAnimationOptions` | detach config     | `fade`, `modelIndex?`                    |

[PUBLIC_TYPE_SCOPE]: annotation / hotspot feature
- rail: render

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [RAIL]                                                 |
| :-----: | :-------------------- | :---------------- | :----------------------------------------------------- |
|   [1]   | `AnnotationInterface` | feature interface | hotspot query and surface intersection                 |
|   [2]   | `HotspotData`         | query result      | `position`, `normal`, `canvasPosition`, `facingCamera` |

[PUBLIC_TYPE_SCOPE]: AR feature
- rail: render

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]     | [RAIL]                                                |
| :-----: | :---------------- | :---------------- | :---------------------------------------------------- |
|   [1]   | `ARInterface`     | feature interface | AR activation, mode, scale, placement properties      |
|   [2]   | `ARMode`          | mode union        | `'quick-look' \| 'scene-viewer' \| 'webxr' \| 'none'` |
|   [3]   | `ARStatusDetails` | event payload     | `status: ARStatus`                                    |

[PUBLIC_TYPE_SCOPE]: camera controls feature
- rail: render

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY]     | [RAIL]                                       |
| :-----: | :-------------------------- | :---------------- | :------------------------------------------- |
|   [1]   | `ControlsInterface`         | feature interface | orbit, target, FOV, interaction prompt props |
|   [2]   | `SphericalPosition`         | camera position   | `theta`, `phi`, `radius`, `toString()`       |
|   [3]   | `CameraChangeDetails`       | event payload     | `source: ChangeSource`                       |
|   [4]   | `InteractionPromptStrategy` | strategy enum     | `'auto' \| 'none'`                           |
|   [5]   | `InteractionPromptStyle`    | style enum        | `'basic' \| 'wiggle'`                        |
|   [6]   | `TouchAction`               | touch mode        | `'pan-y' \| 'pan-x' \| 'none'`               |

[PUBLIC_TYPE_SCOPE]: environment feature
- rail: render

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]     | [RAIL]                                          |
| :-----: | :--------------------- | :---------------- | :---------------------------------------------- |
|   [1]   | `EnvironmentInterface` | feature interface | IBL image, skybox, shadow, exposure properties  |
|   [2]   | `ToneMappingValue`     | tone map union    | `'auto' \| 'aces' \| 'agx' \| 'neutral' \| ...` |

[PUBLIC_TYPE_SCOPE]: loading feature
- rail: render

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]     | [RAIL]                                      |
| :-----: | :------------------------ | :---------------- | :------------------------------------------ |
|   [1]   | `LoadingInterface`        | feature interface | poster, reveal, dimensions, bounding center |
|   [2]   | `LoadingStaticInterface`  | static config     | decoder URLs, `mapURLs` callback            |
|   [3]   | `RevealAttributeValue`    | reveal mode       | `'auto' \| 'manual'`                        |
|   [4]   | `LoadingAttributeValue`   | loading mode      | `'auto' \| 'lazy' \| 'eager'`               |
|   [5]   | `ModelViewerGlobalConfig` | global config     | `dracoDecoderLocation`, power preference    |

[PUBLIC_TYPE_SCOPE]: scene graph feature
- rail: render

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]     | [RAIL]                                           |
| :-----: | :-------------------- | :---------------- | :----------------------------------------------- |
|   [1]   | `SceneGraphInterface` | feature interface | model access, variant, orientation, export       |
|   [2]   | `SceneExportOptions`  | export config     | `binary`, `trs`, `onlyVisible`, `maxTextureSize` |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: AnimationInterface — playback
- rail: render

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------- | :------------- | :------------------------------ |
|   [1]   | `play(options?)`                  | playback       | start animation                 |
|   [2]   | `pause()`                         | playback       | freeze animation                |
|   [3]   | `appendAnimation(name, options?)` | blend          | blend additional animation clip |
|   [4]   | `detachAnimation(name, options?)` | blend          | remove blended clip             |
|   [5]   | `.animationName`                  | property       | select active animation by name |
|   [6]   | `.availableAnimations` (readonly) | property       | array of clip names from model  |
|   [7]   | `.currentTime`                    | property       | seek to playback position       |
|   [8]   | `.timeScale`                      | property       | playback speed multiplier       |

[ENTRYPOINT_SCOPE]: AnnotationInterface — hotspot and ray-surface
- rail: render

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------------------------------- | :------------- | :---------------------------------- |
|   [1]   | `updateHotspot(config)`                      | hotspot write  | update hotspot position/normal      |
|   [2]   | `queryHotspot(name)`                         | hotspot read   | returns `HotspotData \| null`       |
|   [3]   | `positionAndNormalFromPoint(pixelX, pixelY)` | ray cast       | screen pixel -> 3D surface hit      |
|   [4]   | `surfaceFromPoint(pixelX, pixelY)`           | ray cast       | screen pixel -> surface name string |

[ENTRYPOINT_SCOPE]: ARInterface — augmented reality
- rail: render

| [INDEX] | [SURFACE]        | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :--------------- | :------------- | :---------------------------- |
|   [1]   | `activateAR()`   | AR entry       | launch AR viewer session      |
|   [2]   | `.canActivateAR` | capability     | `true` when AR is available   |
|   [3]   | `.arModes`       | property       | space-separated mode priority |
|   [4]   | `.iosSrc`        | property       | USDZ source for Quick Look    |

[ENTRYPOINT_SCOPE]: ControlsInterface — camera
- rail: render

| [INDEX] | [SURFACE]            | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------- | :------------- | :------------------------------- |
|   [1]   | `.cameraOrbit`       | property       | spherical orbit string `"θ φ r"` |
|   [2]   | `.cameraTarget`      | property       | look-at point string `"x y z"`   |
|   [3]   | `.fieldOfView`       | property       | FOV string with units            |
|   [4]   | `.getCameraOrbit()`  | orbit query    | current `SphericalPosition`      |
|   [5]   | `.getCameraTarget()` | target query   | current `Vector3D` look-at       |
|   [6]   | `.cameraControls`    | property       | enable/disable orbit controls    |

[ENTRYPOINT_SCOPE]: LoadingInterface — model loading
- rail: render

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :----------------------- | :------------- | :------------------------------- |
|   [1]   | `dismissPoster()`        | reveal control | hide poster and reveal model     |
|   [2]   | `showPoster()`           | reveal control | show poster, hide model          |
|   [3]   | `getDimensions()`        | bounds query   | model bounding box as `Vector3D` |
|   [4]   | `getBoundingBoxCenter()` | bounds query   | model center as `Vector3D`       |
|   [5]   | `.loaded` (readonly)     | state          | `true` when model is loaded      |

[ENTRYPOINT_SCOPE]: SceneGraphInterface — model mutation
- rail: render

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY]  | [RAIL]                                |
| :-----: | :---------------------------------- | :-------------- | :------------------------------------ |
|   [1]   | `exportScene(options?)`             | scene export    | returns `Promise<Blob>` (glTF/GLB)    |
|   [2]   | `createTexture(uri, type?)`         | texture factory | `Promise<ModelViewerTexture \| null>` |
|   [3]   | `createCanvasTexture()`             | texture factory | canvas-backed `ModelViewerTexture`    |
|   [4]   | `createVideoTexture(uri)`           | texture factory | video-backed `ModelViewerTexture`     |
|   [5]   | `materialFromPoint(pixelX, pixelY)` | ray cast        | `Material \| null` at screen pixel    |
|   [6]   | `.model` (readonly)                 | property        | `Model` scene-graph root              |
|   [7]   | `.availableVariants` (readonly)     | property        | glTF KHR_materials_variants list      |
|   [8]   | `.variantName`                      | property        | activate named material variant       |

[ENTRYPOINT_SCOPE]: EnvironmentInterface — lighting
- rail: render

| [INDEX] | [SURFACE]           | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :------------------ | :------------- | :---------------------------------- |
|   [1]   | `.environmentImage` | property       | HDR/EXR/JPG IBL environment URL     |
|   [2]   | `.skyboxImage`      | property       | skybox background image URL         |
|   [3]   | `.shadowIntensity`  | property       | contact shadow strength `0`–`1`     |
|   [4]   | `.exposure`         | property       | scene exposure multiplier           |
|   [5]   | `hasBakedShadow()`  | query          | `true` if model has baked occlusion |

## [4]-[IMPLEMENTATION_LAW]

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
