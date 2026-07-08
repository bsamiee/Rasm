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

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------- |:---------------- |:-------------------------------------------------------------------------- |
| [01] | `ModelViewerElement` (const + `InstanceType`) / `HTMLElementTagNameMap['model-viewer']` | custom element | `viewer/scene/glb.md` GlbViewport `"model-viewer"` backend row; the JSX `ref` type |
| [02] | `ModelViewerElementBase` | Lit base | `ReactiveElement` spine every mixin extends; the `loading`/`error`/`load` event source |
| [03] | `Vector3D` / `Vector2D` | coordinate value | `{ x, y, z, toString() }` / `{ u, v, toString() }` — dimensions, camera target, hotspot UV |
| [04] | `RGB` / `RGBA` (re-exported glTF 2.0) | color value | scene-graph material factor reads/writes |
| [05] | `AnnotationMixin` / `SceneGraphMixin` / `StagingMixin` / `EnvironmentMixin` / `ControlsMixin` / `ARMixin` / `LoadingMixin` / `AnimationMixin` | mixin factory | compose a custom element subclass with a chosen capability subset |
| [06] | `CanvasTexture` / `FileLoader` / `Loader` / `NearestFilter` (re-exported bare `from 'three'`) | three re-export | primitives from the SHARED peer `three@catalog` (not a bundled copy) for `createCanvasTexture`/custom loaders — the SAME class identities as the viewer `three` row, `instanceof`-compatible and directly interoperable |

[PUBLIC_TYPE_SCOPE]: the eight feature interfaces + their config/event value types
- rail: viewer/scene
- Each interface is one capability facet of the composed element; the config records parameterize its operations.

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------- |:---------------- |:-------------------------------------------------------------------------- |
| [01] | `LoadingInterface` + `LoadingStaticInterface` / `RevealAttributeValue` (`'auto'\|'manual'`) / `LoadingAttributeValue` (`'auto'\|'lazy'\|'eager'`) / `ModelViewerGlobalConfig` | load config | `src`/poster/reveal + decoder-location statics; `powerPreference` |
| [02] | `ControlsInterface` / `SphericalPosition` (`theta`,`phi`,`radius`,`toString()`) / `CameraChangeDetails` / `InteractionPromptStrategy` / `InteractionPromptStyle` / `TouchAction` | camera config | orbit/target/FOV + `camera-change` event; `viewer/geo/project.md` camera sync |
| [03] | `SceneGraphInterface` / `SceneExportOptions` (`binary`,`trs`,`onlyVisible`,`maxTextureSize`) | scene mutation | `exportScene`/variants/`materialFromPoint`; `viewer/scene/appearance.md` material reads |
| [04] | `AnnotationInterface` / `HotspotData` (`position`,`normal`,`canvasPosition`,`facingCamera`) | hotspot + ray | `viewer/mark/bcf.md` viewpoint anchors via `positionAndNormalFromPoint` |
| [05] | `ARInterface` / `ARMode` (`'quick-look'\|'scene-viewer'\|'webxr'\|'none'`) / `ARStatusDetails` | AR activation | mobile AR launch; `iosSrc` USDZ for Quick Look |
| [06] | `EnvironmentInterface` / `ToneMappingValue` (`'auto'\|'aces'\|'agx'\|'commerce'\|'neutral'\|'reinhard'\|'cineon'\|'linear'\|'none'`, the `tone-mapping` attribute union) | IBL lighting | `environmentImage`/`skyboxImage`/`shadowIntensity`/`exposure` — OpenPBR env binding |
| [07] | `AnimationInterface` / `PlayAnimationOptions` (`repetitions`,`pingpong`) / `AppendAnimationOptions` (`fade`,`warp`,`weight`,`timeScale`) / `DetachAnimationOptions` | animation | clip playback + additive blend |
| [08] | `StagingInterface` | auto-stage | `autoRotate` + `autoRotateDelay` idle turntable |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: load, reveal, and camera control — the GlbViewport embed surface
- rail: viewer/scene
- The declarative row: set `.src` to a GLB object-URL, `camera-controls` for orbit; the element decodes, uploads, and renders with no GL handle exposed. Every property has an attribute twin (`camera-orbit`, `shadow-intensity`, …).

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------- |:------------- |:---------------------------------------------------------------------- |
| [01] | `.src` / `.poster` / `.reveal` / `.alt` / readonly `.loaded` / `.modelIsVisible` | load state | `glb.md` `draw` sets `src` to a `model/gltf-binary` object-URL over the `ArtifactBlob` bytes |
| [02] | `dismissPoster()` / `showPoster()` / `getDimensions(): Vector3D` / `getBoundingBoxCenter(): Vector3D` | reveal + bounds | manual reveal gating; frame-to-fit |
| [03] | `.dracoDecoderLocation` / `.ktx2TranscoderLocation` / `.meshoptDecoderLocation` (static) / `mapURLs(cb)` / `ModelViewerGlobalConfig.powerPreference` | decoder config | pin decoders to a self-hosted path before first model (CSP); `mapURLs` rewrites asset URLs |
| [04] | `.cameraControls` / `.cameraOrbit` / `.cameraTarget` / `.fieldOfView` / `.min\|maxCameraOrbit` / `.touchAction` | camera props | `camera-controls` enables orbit; `viewer/geo/project.md` camera fold |
| [05] | `getCameraOrbit(): SphericalPosition` / `getCameraTarget(): Vector3D` / `jumpCameraToGoal()` | camera query | read live camera for the `atom/binding.md` AtomBinding; snap to goal |

[ENTRYPOINT_SCOPE]: scene-graph mutation, hotspot ray-casting, environment, animation, AR — the full advanced surface
- rail: viewer/scene

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER_BOUNDARY] |
|:-----: |:---------------------------------------------------------------------------- |:------------- |:---------------------------------------------------------------------- |
| [01] | `exportScene(options?: SceneExportOptions): Promise<Blob>` / readonly `.model?: Model` / `.availableVariants` / `.variantName` / `.orientation` / `.scale` | scene graph | GLB round-trip export; KHR_materials_variants switch; model-root access |
| [02] | `createTexture(uri, type?)` / `createCanvasTexture()` / `createVideoTexture(uri)` → `ModelViewerTexture` / `materialFromPoint(px, py): Material \| null` | texture + material | runtime texture authoring; screen-pixel → material pick |
| [03] | `updateHotspot(config)` / `queryHotspot(name): HotspotData \| null` / `positionAndNormalFromPoint(px, py)` / `surfaceFromPoint(px, py): string \| null` | hotspot + ray | `viewer/mark/bcf.md` — screen pixel → 3D surface hit + normal for viewpoint anchors |
| [04] | `.environmentImage` / `.skyboxImage` / `.skyboxHeight` / `.exposure` / `.shadowIntensity` / `.shadowSoftness` / `hasBakedShadow()` | environment | `viewer/scene/appearance.md` OpenPBR IBL/exposure binding |
| [05] | `play(options?)` / `pause()` / `appendAnimation(name, options?)` / `detachAnimation(name, options?)` / `.animationName` / readonly `.availableAnimations` / `.currentTime` / `.timeScale` | animation | clip playback + additive blend for animated GLB |
| [06] | `activateAR(): Promise<void>` / readonly `.canActivateAR` / `.ar` / `.arModes` / `.arScale` / `.arPlacement` / `.iosSrc` | AR | mobile AR (quick-look/scene-viewer/webxr) launch |
| [07] | `<*>Mixin<T>(Base): Constructor<…Interface> & T` (per feature) | mixin compose | build a custom element subclass with a chosen capability subset |

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
