# [TS_UI_API_GOOGLE_MODEL_VIEWER]

`@google/model-viewer` mints one custom element, `<model-viewer>`, mixin-composed from eight feature interfaces over a Lit `ReactiveElement` driving an internal three renderer. Importing the module registers `<model-viewer>` + `<extra-model>` on `HTMLElementTagNameMap`, and every capability rides the attribute⟷property⟷event triad. As the `RendererBackend` `"model-viewer"` arm it is the read-only GLB embed with no GL handle: `.src` takes a GLB object-URL, `camera-controls` orbits, owning decode, upload, and camera internally. One mixin facet lands a new capability, never a second element.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@google/model-viewer`
- package: `@google/model-viewer` (Apache-2.0)
- module: ESM `lib/model-viewer.js`, declarations `lib/model-viewer.d.ts` — the const + instance `ModelViewerElement`, the eight feature interfaces + their mixin factories, and the `three` re-exports
- runtime: browser custom element, peer `three` (`.api/three.md`, never bundled), `scope:viewer` — self-registers `<model-viewer>` + `<extra-model>` on `HTMLElementTagNameMap` at import; DRACO/KTX2/meshopt decoder locations default to the Google CDN and pin to a self-hosted path before the first model
- deps: `lit` (`ReactiveElement` base), `@monogrid/gainmap-js` (HDR gainmap decode) — the only bundled runtime deps
- rail: viewer/scene — the declarative read-only GLB embed backend

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the element, its mixin factories, and value/coordinate types

Each `*Mixin` is `<T extends Constructor<ModelViewerElementBase>>(Base: T) => Constructor<…Interface> & T`, so a custom subclass re-applies only the facets it needs.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]    | [CAPABILITY]                                             |
| :-----: | :-------------------------------------- | :--------------- | :------------------------------------------------------- |
|  [01]   | `ModelViewerElement`                    | custom element   | the `"model-viewer"` GlbViewport backend; JSX `ref` type |
|  [02]   | `HTMLElementTagNameMap['model-viewer']` | element map      | augmented tag `document.createElement` resolves          |
|  [03]   | `ModelViewerElementBase`                | Lit base         | `ReactiveElement` spine; `loading`/`error`/`load` source |
|  [04]   | `Vector3D` / `Vector2D`                 | coordinate value | `{x,y,z}`/`{u,v}` + `toString()`; dims, target, UV       |
|  [05]   | `RGB` / `RGBA`                          | color value      | glTF 2.0 re-export; material-factor reads/writes         |
|  [06]   | the eight `*Mixin` factories            | mixin factory    | compose a subclass with a chosen capability subset       |
|  [07]   | `CanvasTexture` / `FileLoader`          | three re-export  | bare `from 'three'`; the shared `three@catalog` classes  |
|  [08]   | `Loader` / `NearestFilter`              | three re-export  | bare `from 'three'`; the shared `three@catalog` classes  |

[PUBLIC_TYPE_SCOPE]: the eight feature interfaces + their config/event value types

Each interface is one capability facet; its config records parameterize the facet's operations, and the attribute-value unions are closed vocabularies: `RevealAttributeValue` `'auto'\|'manual'`, `LoadingAttributeValue` `'auto'\|'lazy'\|'eager'`, `ARMode` `'quick-look'\|'scene-viewer'\|'webxr'\|'none'`, `ToneMappingValue` `'auto'\|'aces'\|'agx'\|'commerce'\|'neutral'\|'reinhard'\|'cineon'\|'linear'\|'none'`.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]  | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `LoadingInterface` + `LoadingStaticInterface`          | load config    | `src`/poster/reveal + decoder-location statics |
|  [02]   | `RevealAttributeValue` / `LoadingAttributeValue`       | load config    | reveal + loading attribute unions              |
|  [03]   | `ModelViewerGlobalConfig`                              | load config    | global `powerPreference` + decoder statics     |
|  [04]   | `ControlsInterface` / `SphericalPosition`              | camera config  | `theta`/`phi`/`radius`; orbit/target/FOV       |
|  [05]   | `CameraChangeDetails`                                  | camera config  | the `camera-change` event payload              |
|  [06]   | `InteractionPromptStrategy` / `InteractionPromptStyle` | camera config  | interaction-prompt strategy + style            |
|  [07]   | `TouchAction`                                          | camera config  | touch-action policy; `viewer/geo/project.md`   |
|  [08]   | `SceneGraphInterface` / `SceneExportOptions`           | scene mutation | `binary`/`trs`/`onlyVisible`/`maxTextureSize`  |
|  [09]   | `AnnotationInterface` / `HotspotData`                  | hotspot + ray  | `position`/`normal`/`canvasPosition`/`facing`  |
|  [10]   | `ARInterface` / `ARMode` / `ARStatusDetails`           | AR activation  | mobile AR launch; `iosSrc` USDZ for Quick Look |
|  [11]   | `EnvironmentInterface` / `ToneMappingValue`            | IBL lighting   | `environmentImage`/`skyboxImage`/`shadow`      |
|  [12]   | `AnimationInterface` / `PlayAnimationOptions`          | animation      | `repetitions`/`pingpong` clip playback         |
|  [13]   | `AppendAnimationOptions` / `DetachAnimationOptions`    | animation      | `fade`/`warp`/`weight`/`timeScale` blend       |
|  [14]   | `StagingInterface`                                     | auto-stage     | `autoRotate` + `autoRotateDelay` turntable     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load, reveal, and camera control — the read-only GlbViewport embed

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `.src` / `.poster` / `.reveal` / `.alt`                  | load state     | `glb.md` `draw` sets `.src` to a GLB object-URL   |
|  [02]   | readonly `.loaded` / `.modelIsVisible`                   | load state     | load + visibility flags                           |
|  [03]   | `dismissPoster()` / `showPoster()`                       | reveal         | manual reveal gating                              |
|  [04]   | `getDimensions(): Vector3D`                              | bounds         | model dimensions                                  |
|  [05]   | `getBoundingBoxCenter(): Vector3D`                       | bounds         | frame-to-fit center                               |
|  [06]   | `.dracoDecoderLocation` / `.ktx2TranscoderLocation`      | decoder static | pin decoders to a self-hosted path before load    |
|  [07]   | `.meshoptDecoderLocation` / `mapURLs(cb)`                | decoder config | meshopt path; `mapURLs` rewrites asset URLs (CSP) |
|  [08]   | `ModelViewerGlobalConfig.powerPreference`                | decoder config | the global GPU `powerPreference`                  |
|  [09]   | `.cameraControls` / `.cameraOrbit` / `.cameraTarget`     | camera props   | `camera-controls` enables orbit                   |
|  [10]   | `.fieldOfView` / `.min\|maxCameraOrbit` / `.touchAction` | camera props   | FOV, orbit clamps, touch-action                   |
|  [11]   | `getCameraOrbit(): SphericalPosition`                    | camera query   | read live orbit for the `atom/binding.md` binding |
|  [12]   | `getCameraTarget(): Vector3D` / `jumpCameraToGoal()`     | camera query   | read target; snap to goal                         |

[ENTRYPOINT_SCOPE]: scene-graph mutation, hotspot ray-casting, environment, animation, and AR

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :---------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `exportScene(options?): Promise<Blob>`                            | scene graph    | GLB round-trip export                      |
|  [02]   | readonly `.model?: Model` / `.orientation` / `.scale`             | scene graph    | model-root access + transform              |
|  [03]   | `.availableVariants` / `.variantName`                             | scene graph    | KHR_materials_variants switch              |
|  [04]   | `createTexture(uri, type?)` / `createCanvasTexture()`             | texture        | runtime `ModelViewerTexture` authoring     |
|  [05]   | `createVideoTexture(uri)`                                         | texture        | a video-backed `ModelViewerTexture`        |
|  [06]   | `materialFromPoint(px, py): Material \| null`                     | material pick  | screen-pixel → material                    |
|  [07]   | `updateHotspot(config)` / `queryHotspot(name)`                    | hotspot        | hotspot update + query                     |
|  [08]   | `positionAndNormalFromPoint(px, py)`                              | ray            | pixel → 3D position + normal               |
|  [09]   | `surfaceFromPoint(px, py): string \| null`                        | ray            | pixel → surface node; `viewer/mark/bcf.md` |
|  [10]   | `.environmentImage` / `.skyboxImage` / `.skyboxHeight`            | environment    | IBL + skybox binding                       |
|  [11]   | `.exposure` / `.shadowIntensity`                                  | environment    | OpenPBR exposure + shadow intensity        |
|  [12]   | `.shadowSoftness` / `hasBakedShadow()`                            | environment    | shadow softness; baked-shadow probe        |
|  [13]   | `play(options?)` / `pause()` / `.currentTime` / `.timeScale`      | animation      | clip playback control                      |
|  [14]   | `appendAnimation(name, options?)`                                 | animation      | additive blend layer                       |
|  [15]   | `detachAnimation(name, options?)`                                 | animation      | remove a blended layer                     |
|  [16]   | `.animationName` / readonly `.availableAnimations`                | animation      | clip selection                             |
|  [17]   | `activateAR(): Promise<void>` / readonly `.canActivateAR` / `.ar` | AR             | mobile AR launch                           |
|  [18]   | `.arModes` / `.arScale` / `.arPlacement` / `.iosSrc`              | AR             | AR mode/scale/placement + USDZ             |
|  [19]   | `<*>Mixin<T>(Base): Constructor<…Interface> & T`                  | mixin compose  | build a subclass with a capability subset  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Capabilities are facets of one mixin-composed element, never sibling components; `HTMLElementTagNameMap` is augmented so `document.createElement('model-viewer')` and JSX both resolve the composed type.
- One reactive state wears three faces — a declarative attribute (`camera-orbit`), an imperative property (`.cameraOrbit`), and a `CustomEvent` (`camera-change` → `CameraChangeDetails`); bind whichever face the surface needs and mirror the state in no parallel field.
- Importing the module runs `customElements.define`; DRACO/KTX2/meshopt decoder URLs are static config set before the first model loads, or the element side-loads decoders from the Google CDN — a CSP breach.
- `<model-viewer>` holds no GL context handle — `.src` and `camera-controls` are the whole binding, and it owns decode/upload/camera/dispose internally, so it mounts where the three `WebGPURenderer` row cannot.

[STACKING]:
- `viewer/scene/glb.md` GlbViewport: the `RendererBackend` `Schema.Literal("three", "model-viewer")` axis folds this element into the `"model-viewer"` arm — `Effect.acquireRelease` wraps `URL.createObjectURL(new Blob([bytes], { type: "model/gltf-binary" }))` (release = `revokeObjectURL`), `draw` sets `.src`, and one literal value swaps backends.
- `three` (`.api/three.md`): SIBLINGS over ONE shared three module — the element drives its own internal `WebGLRenderer` + `<canvas>` + GL context, the three row its own `WebGPURenderer`/`WebGLRenderer` + context, so the re-exported `CanvasTexture`/`FileLoader`/`Loader`/`NearestFilter` ARE the three row's classes, `instanceof`-compatible across the seam. three is the imperative GPU path (custom materials, compute, selection ray-cast, render receipt); this element the declarative zero-GL-handle embed; they meet at the GLB wire.
- `effect` (`libs/typescript/.api/effect.md`): the element lifetime is an `Effect.acquireRelease` scoped resource; DOM `CustomEvent`s (`load`, `camera-change`, `ar-status`, `progress`) become `Stream` sources via `BrowserStream.fromEventListener*`; camera and hotspot state read only through the `atom/binding.md` `AtomBinding`; `exportScene` lifts its `Promise<Blob>` with `Effect.tryPromise`.
- `react` (`.api/react.md`): mounts as a JSX custom element with `ref: ModelViewerElement`; React 19 sets non-string properties directly, so `.src`/`.cameraOrbit` bind as props with no `ref` effect, and the element stays mounted-but-hidden across visibility toggles under `<Activity>` (`act/transition.md`).
- `viewer/scene/appearance.md`: `environmentImage`/`exposure`/`shadowIntensity` bind the decoded OpenPBR IBL and `materialFromPoint`/`.model` read the scene-graph materials the appearance projection mirrors; the `MaterialWire` is decode-only, re-authored nowhere here.

[LOCAL_ADMISSION]:
- Admit `<model-viewer>` as the declarative read-only GLB embed row; pin the decoder-location statics once at boot before the first model.
- Feed GLB bytes as a `model/gltf-binary` object-URL revoked on scope exit under `Effect.acquireRelease`, holding the element only inside a scoped resource.
- Reach for the three `WebGPURenderer` row on GPU-heavy paths (meshlet LOD, splats); this element owns the zero-config declarative embed and AR.

[RAIL_LAW]:
- Package: `@google/model-viewer`
- Owns: the declarative `<model-viewer>` element, its eight mixin feature interfaces, GLB import + `exportScene`, hotspot ray-casting, IBL environment, camera orbit, material variants, and AR activation
- Accept: `.src` GLB object-URL + `camera-controls` as the embed row, the attribute⟷property⟷event triad, decoder statics before first model, `Effect.acquireRelease` lifetime, events-as-`Stream`, camera state through the `AtomBinding`
- Reject: a hand-rolled three renderer for a declarative embed, direct three GL construction the element already covers, decoder side-loading from the CDN under CSP, camera state in a free ref, import outside `scope:viewer`
