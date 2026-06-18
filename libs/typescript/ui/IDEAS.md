# [UI_IDEAS]

The forward concept pool for the host-free browser UI library. Open ideas are higher-order folder concepts grounded in the browser-platform purpose and the current effect-atom, React concurrent-features, WebGPU, and design-system capability surfaces; each drives one or more task cards in `TASKLOG.md`. A finished or dropped idea moves to closed with a one-line disposition so it is never re-litigated.

## [1]-[OPEN]

[ATOM_NATIVE_BINDING_COLLAPSE]:
- Collapse the hand-rolled query-string and storage folds into effect-atom's native constructors at full power: `Atom.searchParam` owns URL-resident state, `Atom.kvs` owns the offline/localStorage cell, `Atom.family` owns keyed per-entity subscription, `Atom.pull` owns the streamed `projection` feeds, and `Result.builder` owns the status-discriminated leaf render so no surface re-implements a three-state match.
- Unlocks one binding vocabulary with no reinvented plumbing; deep-link and offline state survive reload through the library's own reactivity and keepAlive semantics; every leaf renders the loading/success/failure arms uniformly.
- Draws on effect-atom shipping `Atom.searchParam`/`kvs`/`family`/`pull`/`withReactivity` and the chainable `Result.builder` DSL; the prior binding page predated this surface and hand-rolled `DeepLinkBinding` and `OfflineState` as bespoke `SubscriptionRef` folds.

[NATIVE_VIEW_TRANSITIONS]:
- A `motion` sub-domain owning the React `<ViewTransition>` route and layout transition binding over the native View Transitions API, keyed by the route axis so enter/exit/update animations are declarative and the non-reactive transition callback rides `useEffectEvent`.
- Unlocks native, interruptible, GPU-composited route and layout transitions with zero manual FLIP code; a route swap is one declarative `<ViewTransition>` wrap rather than a hand-authored animation-frame choreography.
- Draws on the React `<ViewTransition>` component over the browser View Transitions API; transitions were entirely absent from the prior pages, leaving route changes as instantaneous teardown/rebuild.

[SHARED_GESTURE_ALGEBRA]:
- A `motion` pointer-gesture algebra owning one `CameraGesture` `Data.TaggedEnum` and one total `applyGesture` fold, with the `@use-gesture/react` recognizer mapping drag/pinch/wheel onto the gesture tags as one shared `GestureFold` consumed by both the viewport camera and any draggable surface, its result driving state through the `AtomBinding`.
- Unlocks one gesture algebra instead of per-surface pointer handlers so the viewport camera composes the shared fold rather than owning an orphaned per-surface recognizer, and a new draggable surface reuses the same recognizer-to-state loop.
- Draws on `@use-gesture/react` owning the pointer-gesture recognizer and the Effect `Data.TaggedEnum`/`Match.tagsExhaustive` total-fold pattern; the gesture fold was previously duplicated ad-hoc inside the viewport camera with no shared owner.

[ACTIVITY_PRESERVED_SURFACES]:
- Adopt the React `<Activity>` component to keep heavy, state-bearing surfaces — the `GlbViewport` GL context, the tabbed observation routes, the background cartography layers — mounted-but-hidden, preserving atom subscriptions and GPU resources while unmounting effects and deferring render, instead of teardown and rebuild on every route or tab switch.
- Unlocks a backgrounded 3D viewport that keeps its uploaded GPU buffers and camera state hot, instant tab switches, and pre-rendering of the next route without committing, so the `acquireRelease` GL resource is held across visibility toggles rather than churned.
- Draws on the React `<Activity>` component for state-preserving hidden UI; the prior viewport tore down its `Effect.acquireRelease` GL context and render-loop fiber on every scope exit, wasteful for a re-shown viewport.

[WEBGPU_BACKEND_COLLAPSE]:
- Collapse the four-row renderer-backend axis (three/babylon/model-viewer/webgpu) to a two-row reality given universal WebGPU: a single Three.js row whose `WebGPURenderer` auto-detects and falls back to WebGL with zero config, plus the `model-viewer` zero-GL-handle declarative embed row for the read-only path, deleting the separate Babylon and raw-WebGPU rows and the manual `navigator.gpu` degrade branch.
- Unlocks one render engine owning acquire, draw, and auto-backend in a single arm; the manual WebGPU adapter/device/degrade ceremony and the parallel Babylon `VertexData` path are deleted; the meshlet/cluster-LOD ambition rides Three.js TSL compute instead of a hand-authored `GPUDevice` path.
- Draws on WebGPU reaching universal cross-browser availability and the Three.js `WebGPURenderer` shipping zero-config WebGPU with automatic WebGL fallback; the prior four-backend axis with a hand-rolled webgpu arm and a catchAll-to-three degrade is now redundant ceremony.

[OKLCH_TOKEN_LAYER]:
- Promote theming to its own sub-domain whose `ThemeTokens` generates a perceptually-uniform OKLCH scale via colorjs.io and emits it as the Tailwind CSS-variable layer through one `CssVarSync` Stream fold, so a theme is one token-record swap and Tailwind utilities resolve the live variables at runtime with no re-render cascade.
- Unlocks theme switching, contrast-safe scales, and dark/high-contrast modes as one record swap; design tokens are the single source consumed by both Tailwind utilities and runtime CSS; token generation decouples from the role-behavior vocabulary.
- Draws on the Tailwind CSS-variable engine exposing every design token as a CSS variable in OKLCH natively, and colorjs.io owning the perceptual ramp generation; the prior component-system page fused tokens into the interaction-role page rather than owning theming as a distinct perceptual-color domain.

[LIVE_BINDING_STUDIO_DASHBOARD]:
- An `observation` leaf rendering the C# live-wire binding studio over the decoded `BindingStatusWire`/`WriteReceiptWire`/`CoercedValueWire` rows — a per-binding connect/subscribe/stale/fault health surface, the source-versus-canonical unit coercion read off `CoercedValueWire`, and the write-back disposition shown as a literal-discriminated outcome, every binding-status key the smart-enum string verbatim.
- Unlocks a browser cockpit for the industrial-transport edges (OPC-UA, Modbus, MQTT, serial, REST, GraphQL, spreadsheet, ERP/PLM) the C# `Rasm.AppHost` live-wire spine drives, so a divergence between source-declared and suite-canonical values and a write-back rejection surface in the SPA rather than only in the host log; a subscribe-shaped transport streams its reactive sequence into a live cell and a poll-shaped transport refreshes on its cadence, both rendered uniformly.
- Draws on the C# `Rasm.AppHost/live-wire#TS_PROJECTION` binding-status and write-receipt wire shapes (the write disposition crosses as a literal-discriminated union; the source timestamp crosses as extended-ISO text for freshness against host time); a read-only leaf over a `projection` live cell and the one `AtomBinding`, never a transport dial and never a second decode. The wire owner is the C# branch; the dashboard decodes the settled shape and never re-mints a binding-status enum.

## [2]-[CLOSED]

None.
