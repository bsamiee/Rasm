# [TS_UI_API_GOOGLE_MODEL_VIEWER]

`@google/model-viewer` is ONE custom element, `<model-viewer>`, composed by mixin from eight feature interfaces over a Lit `ReactiveElement` base driving a three.js renderer. Importing the module registers `<model-viewer>` (and `<extra-model>`) on `HTMLElementTagNameMap`; the element then exposes every capability through the uniform attribute⟷property⟷event triad, so a declarative HTML attribute, an imperative DOM property, and a `CustomEvent` are three faces of one state. In the viewer it is the `RendererBackend` `"model-viewer"` literal — the read-only embed that holds no GL handle: `.src` takes a GLB object-URL, `camera-controls` enables orbit, and the element owns the renderer, decode, and camera internally. That internal renderer runs on the workspace's SHARED `three@catalog` (`three` is a peerDependency `^catalog`, NOT bundled — pnpm dedupes the element onto the one physical three copy), so it is a renderer-level SIBLING to the imperative `three` row (`.api/three.md`): one shared three module, two independent renderer instances, never a second three copy nor a shared GL context. The feature interfaces below are the mixin-capability axis; a new capability is a mixin facet, never a second element.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@google/model-viewer`
- package: `@google/model-viewer`
- license: `Apache-2.0`
- peer: `three ^catalog` (`.api/three.md`) — declared a peerDependency, NEVER bundled; the workspace's single `three@catalog` fills the peer (pnpm symlinks the element's `three` onto the one shared copy, a two-minor overshoot three's breaking- catalog cadence tolerates). The element is a renderer-level SIBLING to the viewer `three` row, not a layer: it drives its OWN internal `WebGLRenderer` + `<canvas>` + GL context, independent of the three row's renderer — never a shared renderer or GL context, but the SAME three module. Re-exports `CanvasTexture`/`FileLoader`/`Loader`/`NearestFilter` bare `from 'three'`, so they resolve to the viewer three row's exact `catalog` classes, directly interoperable
- deps: `lit ^catalog` (`ReactiveElement` base + reactive attributes), `@monogrid/gainmap-js ^catalog` (HDR gainmap environment decode) — the ONLY bundled runtime deps; three is peer-provided, not here
- catalog-verdict: KEEP
- runtime: browser custom element (`scope:viewer`) — self-registers `<model-viewer>` + `<extra-model>` on `HTMLElementTagNameMap` at import; DRACO/KTX catalog/meshopt decoder locations default to the Google CDN and must be set on the static config before the first model
- entry: `lib/model-viewer.d.ts` — `ModelViewerElement` (const + instance type), the eight feature interfaces + their mixin factories, and the `three` re-exports

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the element, its mixin factories, and value/coordinate types
- rail: viewer/scene
- `ModelViewerElement` is `AnnotationMixin ∘ SceneGraphMixin ∘ StagingMixin ∘ EnvironmentMixin ∘ ControlsMixin ∘ ARMixin ∘ LoadingMixin ∘ AnimationMixin` over `ModelViewerElementBase`. Each `*Mixin` is a `<T extends Constructor<ModelViewerElementBase>>(Base: T) => Constructor<…Interface> & T` — the composition is the surface, and a custom subclass re-applies the mixins it needs.

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]    | [CONSUMER_BOUNDARY]                                            |
| :-----: | :-------------------------------------- | :--------------- | :------------------------------------------------------------- |
|  [01]   | `ModelViewerElement`                    | custom element   | the `"model-viewer"` GlbViewport backend; JSX `ref` type       |
|  [02]   | `HTMLElementTagNameMap['model-viewer']` | element map      | the augmented tag type `document.createElement` resolves       |
|  [03]   | `ModelViewerElementBase`                | Lit base         | `ReactiveElement` spine; `loading`/`error`/`load` source       |
|  [04]   | `Vector3D` / `Vector2D`                 | coordinate value | `{x,y,z}`/`{u,v}` + `toString()`; dims, target, hotspot UV     |
|  [05]   | `RGB` / `RGBA`                          | color value      | re-exported glTF 2.0; scene-graph material factor reads/writes |
|  [06]   | the eight `*Mixin` factories            | mixin factory    | compose a subclass with a chosen capability subset (see lead)  |
|  [07]   | `CanvasTexture` / `FileLoader`          | three re-export  | bare `from 'three'`; the shared `three@catalog` classes        |
|  [08]   | `Loader` / `NearestFilter`              | three re-export  | bare `from 'three'`; the shared `three@catalog` classes        |

[PUBLIC_TYPE_SCOPE]: the eight feature interfaces + their config/event value types
- rail: viewer/scene
- Each interface is one capability facet of the composed element; the config records parameterize its operations.
- `RevealAttributeValue` = `'auto'\|'manual'`; `LoadingAttributeValue` = `'auto'\|'lazy'\|'eager'`; `ARMode` = `'quick-look'\|'scene-viewer'\|'webxr'\|'none'`.
- `ToneMappingValue` = `'auto'\|'aces'\|'agx'\|'commerce'\|'neutral'\|'reinhard'\|'cineon'\|'linear'\|'none'` (the `tone-mapping` attribute union).

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY]  | [CONSUMER_BOUNDARY]                                   |
| :-----: | :----------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `LoadingInterface` + `LoadingStaticInterface`          | load config    | `src`/poster/reveal + decoder-location statics        |
|  [02]   | `RevealAttributeValue` / `LoadingAttributeValue`       | load config    | reveal + loading attribute unions (values in lead)    |
|  [03]   | `ModelViewerGlobalConfig`                              | load config    | global `powerPreference` + decoder statics            |
|  [04]   | `ControlsInterface` / `SphericalPosition`              | camera config  | `theta`/`phi`/`radius`; orbit/target/FOV              |
|  [05]   | `CameraChangeDetails`                                  | camera config  | the `camera-change` event payload                     |
|  [06]   | `InteractionPromptStrategy` / `InteractionPromptStyle` | camera config  | interaction-prompt strategy + style policy            |
|  [07]   | `TouchAction`                                          | camera config  | the touch-action policy; `viewer/geo/project.md` sync |
|  [08]   | `SceneGraphInterface` / `SceneExportOptions`           | scene mutation | `binary`/`trs`/`onlyVisible`/`maxTextureSize`         |
|  [09]   | `AnnotationInterface` / `HotspotData`                  | hotspot + ray  | `position`/`normal`/`canvasPosition`/`facingCamera`   |
|  [10]   | `ARInterface` / `ARMode` / `ARStatusDetails`           | AR activation  | mobile AR launch; `iosSrc` USDZ for Quick Look        |
|  [11]   | `EnvironmentInterface` / `ToneMappingValue`            | IBL lighting   | `environmentImage`/`skyboxImage`/`shadowIntensity`    |
|  [12]   | `AnimationInterface` / `PlayAnimationOptions`          | animation      | `repetitions`/`pingpong` clip playback                |
|  [13]   | `AppendAnimationOptions` / `DetachAnimationOptions`    | animation      | `fade`/`warp`/`weight`/`timeScale` additive blend     |
|  [14]   | `StagingInterface`                                     | auto-stage     | `autoRotate` + `autoRotateDelay` idle turntable       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load, reveal, and camera control — the GlbViewport embed surface
- rail: viewer/scene
- The declarative row: set `.src` to a GLB object-URL, `camera-controls` for orbit; the element decodes, uploads, and renders with no GL handle exposed. Every property has an attribute twin (`camera-orbit`, `shadow-intensity`, …).

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                                   |
| :-----: | :------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `.src` / `.poster` / `.reveal` / `.alt`                  | load state     | `glb.md` `draw` sets `.src` to a GLB object-URL       |
|  [02]   | readonly `.loaded` / `.modelIsVisible`                   | load state     | load + visibility flags                               |
|  [03]   | `dismissPoster()` / `showPoster()`                       | reveal         | manual reveal gating                                  |
|  [04]   | `getDimensions(): Vector3D`                              | bounds         | model dimensions                                      |
|  [05]   | `getBoundingBoxCenter(): Vector3D`                       | bounds         | frame-to-fit center                                   |
|  [06]   | `.dracoDecoderLocation` / `.ktx2TranscoderLocation`      | decoder static | pin decoders to a self-hosted path before first model |
|  [07]   | `.meshoptDecoderLocation` / `mapURLs(cb)`                | decoder config | meshopt path; `mapURLs` rewrites asset URLs (CSP)     |
|  [08]   | `ModelViewerGlobalConfig.powerPreference`                | decoder config | the global GPU `powerPreference`                      |
|  [09]   | `.cameraControls` / `.cameraOrbit` / `.cameraTarget`     | camera props   | `camera-controls` enables orbit                       |
|  [10]   | `.fieldOfView` / `.min\|maxCameraOrbit` / `.touchAction` | camera props   | FOV, orbit clamps, touch-action; project camera fold  |
|  [11]   | `getCameraOrbit(): SphericalPosition`                    | camera query   | read live orbit for the `atom/binding.md` AtomBinding |
|  [12]   | `getCameraTarget(): Vector3D` / `jumpCameraToGoal()`     | camera query   | read target; snap to goal                             |

[ENTRYPOINT_SCOPE]: scene-graph mutation, hotspot ray-casting, environment, animation, AR — the full advanced surface
- rail: viewer/scene

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY]                              |
| :-----: | :---------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `exportScene(options?): Promise<Blob>`                            | scene graph    | GLB round-trip export                            |
|  [02]   | readonly `.model?: Model` / `.orientation` / `.scale`             | scene graph    | model-root access + transform                    |
|  [03]   | `.availableVariants` / `.variantName`                             | scene graph    | KHR_materials_variants switch                    |
|  [04]   | `createTexture(uri, type?)` / `createCanvasTexture()`             | texture        | runtime `ModelViewerTexture` authoring           |
|  [05]   | `createVideoTexture(uri)`                                         | texture        | a video-backed `ModelViewerTexture`              |
|  [06]   | `materialFromPoint(px, py): Material \| null`                     | material pick  | screen-pixel → material                          |
|  [07]   | `updateHotspot(config)` / `queryHotspot(name)`                    | hotspot        | hotspot update + query                           |
|  [08]   | `positionAndNormalFromPoint(px, py)`                              | ray            | pixel → 3D position + normal                     |
|  [09]   | `surfaceFromPoint(px, py): string \| null`                        | ray            | pixel → surface node; `viewer/mark/bcf.md`       |
|  [10]   | `.environmentImage` / `.skyboxImage` / `.skyboxHeight`            | environment    | IBL + skybox binding                             |
|  [11]   | `.exposure` / `.shadowIntensity`                                  | environment    | OpenPBR exposure + shadow intensity              |
|  [12]   | `.shadowSoftness` / `hasBakedShadow()`                            | environment    | shadow softness; baked-shadow probe              |
|  [13]   | `play(options?)` / `pause()` / `.currentTime` / `.timeScale`      | animation      | clip playback control                            |
|  [14]   | `appendAnimation(name, options?)`                                 | animation      | additive blend layer                             |
|  [15]   | `detachAnimation(name, options?)`                                 | animation      | remove a blended layer                           |
|  [16]   | `.animationName` / readonly `.availableAnimations`                | animation      | clip selection                                   |
|  [17]   | `activateAR(): Promise<void>` / readonly `.canActivateAR` / `.ar` | AR             | mobile AR launch                                 |
|  [18]   | `.arModes` / `.arScale` / `.arPlacement` / `.iosSrc`              | AR             | AR mode/scale/placement + USDZ                   |
|  [19]   | `<*>Mixin<T>(Base): Constructor<…Interface> & T`                  | mixin compose  | build a subclass with a chosen capability subset |

## [04]-[IMPLEMENTATION_LAW]

[ELEMENT_TOPOLOGY]:
- one element, mixin-composed: `ModelViewerElement` is the intersection of eight mixin constructors over `ModelViewerElementBase` (a Lit `ReactiveElement`). Capabilities are facets of one element, not sibling components; `HTMLElementTagNameMap` is augmented so `document.createElement('model-viewer')` and JSX both resolve the composed type.
- attribute⟷property⟷event triad: a declarative attribute (`camera-orbit`), an imperative property (`.cameraOrbit`), and a `CustomEvent` (`camera-change` → `CameraChangeDetails`) are three faces of one reactive state; bind whichever the surface needs and never mirror the state in a parallel field.
- registration + decoder statics at import: importing the module runs `customElements.define`; DRACO/KTX2/meshopt decoder URLs are static config that MUST be set before the first model loads, or the element side-loads decoders from the Google CDN (a CSP breach).
- the `"model-viewer"` viewer row is read-only: it holds no GL context handle — `.src` (GLB object-URL) and `camera-controls` are the whole binding; the element owns decode/upload/camera/dispose internally, so it degrades gracefully where the three `WebGPURenderer` row cannot mount.

[INTEGRATION_LAW]:
- Stack with `viewer/scene/glb.md` GlbViewport: the `RendererBackend` `Schema.Literal("three", "model-viewer")` axis folds this element into the `"model-viewer"` arm — `Effect.acquireRelease` wraps `URL.createObjectURL(new Blob([bytes], { type: "model/gltf-binary" }))` (release = `revokeObjectURL`), `draw` sets `.src`, and the element replaces the canvas; one literal value swaps backends, never a parallel viewport.
- Stack with `three` (`.api/three.md`): SIBLINGS, not layers, over ONE shared three module. `<model-viewer>` peers `three@^catalog`, filled by the workspace's single `three@catalog` — pnpm dedupes the element onto the SAME physical copy the `viewer/scene/glb.md` `three` row runs, NOT a bundled second three. They share the three MODULE but own independent renderer INSTANCES: the element drives its own internal `WebGLRenderer` + `<canvas>` + WebGL context, the three row its own `WebGPURenderer`/`WebGLRenderer` + context — never a shared renderer nor GL/GPU context. Because the module is shared, the re-exported `CanvasTexture`/`FileLoader`/`Loader`/`NearestFilter` (`from 'three'`) ARE the viewer three row's `catalog` classes, `instanceof`-compatible across the seam. The `three` `WebGPURenderer` row is the imperative full-control GPU path (custom materials, compute, selection ray-cast, render receipt); `<model-viewer>` is the declarative zero-GL-handle embed — the `RendererBackend` `Schema.Literal("three", "model-viewer")` literal picks which draws a given GLB, and they meet at the GLB wire.
- Stack with `effect` (`libs/typescript/.api/effect.md`): the element lifecycle is an `Effect.acquireRelease` scoped resource; DOM `CustomEvent`s (`load`, `camera-change`, `ar-status`, `progress`) become `Stream` sources via `BrowserStream.fromEventListener*`; the camera state reads/writes only through the `atom/binding.md` `AtomBinding`, never a free ref. `exportScene` returns a `Promise<Blob>` lifted at the seam with `Effect.tryPromise`.
- Stack with `react` (`.api/react.md`): rendered as a custom element in JSX with a `ref: ModelViewerElement`; React 19 sets non-string properties on custom elements directly, so `.src`/`.cameraOrbit` bind as props without a manual `ref` effect. It is the `viewer/scene/glb.md` `"model-viewer"` row's rendered surface, mounted-but-hidden across visibility toggles under React `<Activity>` (`act/transition.md`).
- Stack with `viewer/scene/appearance.md`: `environmentImage`/`exposure`/`shadowIntensity` bind the decoded OpenPBR appearance IBL; `materialFromPoint`/`.model` read the scene-graph materials the appearance projection mirrors — the wire `MaterialWire` is decode-only, never re-authored here.

[LOCAL_ADMISSION]:
- Admit `<model-viewer>` as the declarative read-only GLB embed row; set decoder-location statics once at boot before the first model.
- Feed GLB bytes as a `model/gltf-binary` object-URL and revoke it on scope exit under `Effect.acquireRelease`; never leak the URL or hold the element outside a scoped resource.
- Read camera/hotspot/load state through events-as-`Stream` and the `AtomBinding`; never poll properties in a render loop or mirror element state in a React ref.
- Reach for the three `WebGPURenderer` row for GPU-heavy paths (meshlet LOD, splats); use this element for the zero-config declarative embed and AR.

[RAIL_LAW]:
- Package: `@google/model-viewer`
- Owns: the declarative `<model-viewer>` custom element, the eight mixin feature interfaces (load/controls/scene-graph/annotation/AR/environment/animation/staging), GLB import + `exportScene`, hotspot ray-casting, IBL environment, camera orbit, material variants, and AR activation
- Accept: `.src` GLB object-URL + `camera-controls` as the viewer embed row, the attribute⟷property⟷event triad, decoder statics before first model, `Effect.acquireRelease` element lifetime, events-as-`Stream`, camera state through the `AtomBinding`
- Reject: a hand-rolled three renderer for a declarative-embed use case, direct three GL construction when the element covers it, decoder side-loading from the CDN under CSP, camera state in a free ref, importing outside `scope:viewer`
