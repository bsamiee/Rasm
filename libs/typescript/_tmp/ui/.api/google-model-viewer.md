# [API_CATALOGUE] @google/model-viewer

`@google/model-viewer` is ONE custom element, `<model-viewer>`, composed by mixin from eight feature interfaces over a Lit `ReactiveElement` base driving a three.js renderer. Importing the module registers `<model-viewer>` (and `<extra-model>`) on `HTMLElementTagNameMap`; the element then exposes every capability through the uniform attribute⟷property⟷event triad, so a declarative HTML attribute, an imperative DOM property, and a `CustomEvent` are three faces of one reactive state. In the viewport it is the `render/glb.md` `RendererBackend` `"model-viewer"` literal — the read-only embed that holds no GL handle: `.src` takes a GLB object-URL, `camera-controls` enables orbit, and the element owns decode, upload, camera, and dispose internally. The eight feature interfaces are the mixin-capability axis; a new capability is a mixin facet, never a second element.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `@google/model-viewer`
- package: `@google/model-viewer`
- version: `4.3.1`
- license: `Apache-2.0`
- module: `lib/model-viewer.d.ts` entry; no `exports` map, so every `lib/**` deep path (the mixins + interface types) is importable, though the canonical surface is the composed element from the package root
- peer: `three ^0.183` — the element BUNDLES its own three (a SIBLING to the `render/glb.md` `three` `0.185.1` row, never a shared renderer/context); re-exports `CanvasTexture`/`FileLoader`/`Loader`/`NearestFilter` from its bundled copy
- deps: `lit ^3.2.1` (`ReactiveElement` base + reactive attributes), `@monogrid/gainmap-js ^3.1` (HDR gainmap environment decode)
- runtime: browser custom element — self-registers `<model-viewer>` + `<extra-model>` on `HTMLElementTagNameMap` at import; DRACO/KTX2/meshopt decoder locations default to the Google CDN and must be set on the static config before the first model (CSP)
- entry: `ModelViewerElement` (const + `InstanceType` type), the eight feature interfaces + their mixin factories (deep-import), the `three` re-exports (`CanvasTexture`/`FileLoader`/`Loader`/`NearestFilter`), and `RGB`/`RGBA` (glTF 2.0)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the element, its mixin factories, and value/coordinate types
- rail: viewer/scene
- `ModelViewerElement` is `AnnotationMixin ∘ SceneGraphMixin ∘ StagingMixin ∘ EnvironmentMixin ∘ ControlsMixin ∘ ARMixin ∘ LoadingMixin ∘ AnimationMixin` over `ModelViewerElementBase`. Each `*Mixin` is `<T extends Constructor<ModelViewerElementBase>>(Base: T) => Constructor<…Interface> & T` — the composition is the surface, and a custom subclass re-applies the mixins it needs.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                                        |
| :-----: | :---------------------------------------------------------------- | :---------------- | :-------------------------------------------------------------------------- |
|  [01]   | `ModelViewerElement` (const + `InstanceType`) / `HTMLElementTagNameMap['model-viewer']` | custom element | `render/glb.md` `GlbViewport` `"model-viewer"` backend row; `document.createElement('model-viewer')` and JSX both resolve the composed type |
|  [02]   | `ModelViewerElementBase`                                          | Lit base          | the `ReactiveElement` spine every mixin extends; the `loading`/`error`/`load`/`progress` event source |
|  [03]   | `Vector3D` / `Vector2D`                                           | coordinate value  | `{ x, y, z, toString() }` / `{ u, v, toString() }` — dimensions, camera target, hotspot UV (`model-viewer-base.d.ts`) |
|  [04]   | `RGB` / `RGBA` (re-exported glTF 2.0)                             | color value        | scene-graph material factor reads/writes |
|  [05]   | `AnnotationMixin` / `SceneGraphMixin` / `StagingMixin` / `EnvironmentMixin` / `ControlsMixin` / `ARMixin` / `LoadingMixin` / `AnimationMixin` | mixin factory | deep-import (`lib/features/*.js`) — compose a custom element subclass with a chosen capability subset |
|  [06]   | `CanvasTexture` / `FileLoader` / `Loader` / `NearestFilter` (re-exported from the bundled `three`) | three re-export | `three` primitives from the element's OWN bundled copy (peer `^0.183`) for `createCanvasTexture`/custom loaders — distinct from the `render/glb.md` `three` row's `0.185.1` classes |

[PUBLIC_TYPE_SCOPE]: the eight feature interfaces + their config/event value types
- rail: viewer/scene
- Each interface is one capability facet of the composed element; the config records parameterize its operations. The interface members are real element instance members whether or not the interface type is separately imported.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY]     | [CONSUMER / BOUNDARY]                                                        |
| :-----: | :---------------------------------------------------------------- | :---------------- | :-------------------------------------------------------------------------- |
|  [01]   | `LoadingInterface` + `LoadingStaticInterface` / `RevealAttributeValue` (`'auto'\|'manual'`) / `LoadingAttributeValue` (`'auto'\|'lazy'\|'eager'`) / `ModelViewerGlobalConfig` | load config | `src`/poster/reveal + decoder-location statics + `powerPreference` |
|  [02]   | `ControlsInterface` / `SphericalPosition` (`theta`,`phi`,`radius`,`toString()`) / `CameraChangeDetails` / `InteractionPromptStrategy` (`'auto'\|'none'`) / `InteractionPromptStyle` (`'basic'\|'wiggle'`) / `TouchAction` (`'pan-y'\|'pan-x'\|'none'`) / `Finger` | camera config | orbit/target/FOV + `camera-change` event; `binding/atom.md` camera fold |
|  [03]   | `SceneGraphInterface` / `SceneExportOptions` (`binary`,`trs`,`onlyVisible`,`maxTextureSize`,`includeCustomExtensions`,`forceIndices`) | scene mutation | `exportScene`/variants/`materialFromPoint` |
|  [04]   | `AnnotationInterface` / `HotspotData` (`position`,`normal`,`canvasPosition`,`facingCamera`) | hotspot + ray | screen-pixel → 3D surface hit + normal for hotspot anchors / selection |
|  [05]   | `ARInterface` / `ARMode` (`'quick-look'\|'scene-viewer'\|'webxr'\|'none'`) / `ARStatusDetails` / `ARTrackingDetails` | AR activation | mobile AR launch; `iosSrc` USDZ for Quick Look |
|  [06]   | `EnvironmentInterface` / `ToneMappingValue` (`'auto'\|'aces'\|'agx'\|'commerce'\|'neutral'\|'reinhard'\|'cineon'\|'linear'\|'none'`) | IBL lighting | `environmentImage`/`skyboxImage`/`shadowIntensity`/`exposure` — the `tone-mapping` attribute union |
|  [07]   | `AnimationInterface` / `PlayAnimationOptions` (`repetitions`,`pingpong`,`modelIndex?`) / `AppendAnimationOptions` (`fade`,`warp`,`weight`,`timeScale`,`relativeWarp`,`time`,`repetitions`) / `DetachAnimationOptions` (`fade`,`modelIndex?`) | animation | clip playback + additive blend |
|  [08]   | `StagingInterface`                                                | auto-stage        | `autoRotate` + `autoRotateDelay` idle turntable |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load, reveal, and camera control — the GlbViewport embed surface
- rail: viewer/scene
- The declarative row: set `.src` to a GLB object-URL, `camera-controls` for orbit; the element decodes, uploads, and renders with no GL handle exposed. Every property has an attribute twin (`camera-orbit`, `shadow-intensity`, …).

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                                    |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------------- |
|  [01]   | `.src` / `.poster` / `.reveal` / `.alt` / readonly `.loaded` / `.modelIsVisible` | load state | `render/glb.md` `acquireBackend` sets `.src` to a `model/gltf-binary` object-URL over the `ArtifactBlob` bytes, `canvas.replaceWith(el)` |
|  [02]   | `dismissPoster()` / `showPoster()` / `getDimensions(): Vector3D` / `getBoundingBoxCenter(): Vector3D` | reveal + bounds | manual reveal gating; frame-to-fit |
|  [03]   | `.dracoDecoderLocation` / `.ktx2TranscoderLocation` / `.meshoptDecoderLocation` (static) / `mapURLs(cb)` / `ModelViewerGlobalConfig.powerPreference` | decoder config | pin decoders to a self-hosted path before first model (CSP); `mapURLs` rewrites asset URLs |
|  [04]   | `.cameraControls` / `.cameraOrbit` / `.cameraTarget` / `.fieldOfView` / `.min\|maxCameraOrbit` / `.touchAction` | camera props | `camera-controls` enables orbit; `binding/atom.md` camera fold |
|  [05]   | `getCameraOrbit(): SphericalPosition` / `getCameraTarget(): Vector3D` / `jumpCameraToGoal()` | camera query | read live camera for the `binding/atom.md` `AtomBinding`; snap to goal |

[ENTRYPOINT_SCOPE]: scene-graph mutation, hotspot ray-casting, environment, animation, AR — the full advanced surface
- rail: viewer/scene

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] | [CONSUMER / BOUNDARY]                                                    |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------------- |
|  [01]   | `exportScene(options?: SceneExportOptions): Promise<Blob>` / readonly `.model?: Model` / `.availableVariants` / `.variantName` / `.orientation` / `.scale` | scene graph | GLB round-trip export lifted via `Effect.tryPromise`; KHR_materials_variants switch; model-root access |
|  [02]   | `createTexture(uri, type?)` / `createCanvasTexture()` / `createVideoTexture(uri)` → `ModelViewerTexture` / `materialFromPoint(px, py): Material \| null` | texture + material | runtime texture authoring; screen-pixel → material pick |
|  [03]   | `updateHotspot(config)` / `queryHotspot(name): HotspotData \| null` / `positionAndNormalFromPoint(px, py)` / `surfaceFromPoint(px, py): string \| null` | hotspot + ray | screen-pixel → 3D surface hit + normal — the available picking capability for hotspot/selection surfaces (not currently wired: `overlay/bcf.md` derives viewpoints from `BcfViewpointWire` wire-math, not ray-casts) |
|  [04]   | `.environmentImage` / `.skyboxImage` / `.skyboxHeight` / `.exposure` / `.shadowIntensity` / `.shadowSoftness` / `hasBakedShadow()` | environment | OpenPBR IBL/exposure binding |
|  [05]   | `play(options?)` / `pause()` / `appendAnimation(name, options?)` / `detachAnimation(name, options?)` / `.animationName` / readonly `.availableAnimations` / `.currentTime` / `.timeScale` / `.autoplay` / readonly `.paused` | animation | clip playback + additive blend for animated GLB |
|  [06]   | `activateAR(): Promise<void>` / readonly `.canActivateAR` / `.ar` / `.arModes` / `.arScale` / `.arPlacement` / `.iosSrc` | AR | mobile AR (quick-look/scene-viewer/webxr) launch |
|  [07]   | `<*>Mixin<T>(Base): Constructor<…Interface> & T` (per feature, deep-import)   | mixin compose  | build a custom element subclass with a chosen capability subset |

## [04]-[IMPLEMENTATION_LAW]

[ELEMENT_TOPOLOGY]:
- one element, mixin-composed: `ModelViewerElement` is the intersection of eight mixin constructors over `ModelViewerElementBase` (a Lit `ReactiveElement`). Capabilities are facets of one element, not sibling components; `HTMLElementTagNameMap` is augmented so `document.createElement('model-viewer')` and JSX both resolve the composed type.
- attribute⟷property⟷event triad: a declarative attribute (`camera-orbit`), an imperative property (`.cameraOrbit`), and a `CustomEvent` (`camera-change` → `CameraChangeDetails`) are three faces of one reactive state; bind whichever the surface needs and never mirror the state in a parallel field.
- registration + decoder statics at import: importing the module runs `customElements.define`; DRACO/KTX2/meshopt decoder URLs are static config that MUST be set before the first model loads, or the element side-loads decoders from the Google CDN (a CSP breach).
- the `"model-viewer"` viewport row is read-only: it holds no GL context handle — `.src` (GLB object-URL) and `camera-controls` are the whole binding; the element owns decode/upload/camera/dispose internally, so it degrades gracefully where the three `WebGPURenderer` row cannot mount.

[STACKING_LAW]:
- `render/glb.md` `GlbViewport`: the `RendererBackend = Schema.Literal("three", "model-viewer")` axis folds this element into the `"model-viewer"` `acquireBackend` arm — `Effect.acquireRelease` wraps `URL.createObjectURL(new Blob([blob.bytes], { type: "model/gltf-binary" }))` (release = `revokeObjectURL`), sets `el.setAttribute("camera-controls", "")` + `el.src = src`, and `canvas.replaceWith(el)`; one literal value swaps backends, never a parallel viewport.
- `three.md`: SIBLINGS, not layers — `<model-viewer>` bundles its OWN `three` (peer `^0.183`) while the `render/glb.md` `three` row runs `0.185.1` (`three/webgpu` `WebGPURenderer`); the two are independent renderer instances that never share a context. This element re-exports `CanvasTexture`/`FileLoader`/`Loader`/`NearestFilter` from its bundled `three` for `createCanvasTexture`/custom loaders. The `three` `WebGPURenderer` row is the imperative full-control GPU path (custom materials, TSL compute, meshlet LOD, selection ray-cast); `<model-viewer>` is the declarative zero-config embed — one `RendererBackend` literal picks which, and they meet at the GLB wire.
- universal `libs/typescript/.api/effect.md`: the element lifecycle is an `Effect.acquireRelease` scoped resource; DOM `CustomEvent`s (`load`, `camera-change`, `ar-status`, `progress`) become `Stream` sources; the camera state reads/writes only through the `binding/atom.md` `AtomBinding`, never a free ref; `exportScene` returns a `Promise<Blob>` lifted at the seam with `Effect.tryPromise`; the `RendererBackend` dispatch is a `Match.value(backend)`.
- `interaction/transition.md`: the element is rendered in JSX with a `ref: ModelViewerElement` (React 19 sets non-string properties on custom elements directly, so `.src`/`.cameraOrbit` bind as props); a backgrounded viewport stays mounted-but-hidden across visibility toggles under React `<Activity>`, preserving the element (decode + camera) instead of teardown.
- hotspot/ray-cast surface (`positionAndNormalFromPoint`/`surfaceFromPoint`/`materialFromPoint`) is the available screen-pixel → surface/material picking capability for any selection or annotation surface; it is not currently a load-bearing consumer (`overlay/bcf.md` builds its `CameraState` from `BcfViewpointWire.cameraPosition`/`cameraDirection` math, not element ray-casts).

[LOCAL_ADMISSION]:
- Admit `<model-viewer>` as the declarative read-only GLB embed row; set decoder-location statics once at boot before the first model.
- Feed GLB bytes as a `model/gltf-binary` object-URL and revoke it on scope exit under `Effect.acquireRelease`; never leak the URL or hold the element outside a scoped resource.
- Read camera/hotspot/load state through events-as-`Stream` and the `AtomBinding`; never poll properties in a render loop or mirror element state in a React ref.
- Reach for the three `WebGPURenderer` row for GPU-heavy paths (meshlet LOD, splats, compute); use this element for the zero-config declarative embed and AR.

[RAIL_LAW]:
- Package: `@google/model-viewer`
- Owns: the declarative `<model-viewer>` custom element, the eight mixin feature interfaces (load/controls/scene-graph/annotation/AR/environment/animation/staging), GLB import + `exportScene`, hotspot ray-casting, IBL environment, camera orbit, material variants, and AR activation
- Accept: `.src` GLB object-URL + `camera-controls` as the viewport embed row, the attribute⟷property⟷event triad, decoder statics before first model, `Effect.acquireRelease` element lifetime, events-as-`Stream`, camera state through the `AtomBinding`
- Reject: a hand-rolled three renderer for a declarative-embed use case, direct three GL construction when the element covers it, decoder side-loading from the CDN under CSP, camera state in a free ref, treating its bundled `three` as the `render/glb.md` renderer
