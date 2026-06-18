# [UI_TASKLOG]

Open and closed work for the host-free browser UI library, distilled from the ideas in `IDEAS.md`. Each open card carries a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` — plus the capability or file to build, the external packages to integrate, the integration points and boundaries or wires, and the key considerations. Cross-branch wire preconditions are tracked as blocked tasks against the C# fence that unblocks them.

## [1]-[OPEN]

[T-BINDING-COLLAPSE] [QUEUED] — atom-native binding spine
- Author `binding/atom-binding.md` to its full surface: `Atom.searchParam` for URL state, `Atom.kvs` for the offline cell, `Atom.family` for keyed subscription, `Atom.pull` for the `projection` feeds, the `UndoStack` history fold, and the `Result.builder` status-render chain; the dev-build atom inspector is one `Atom.runtime` row.
- Integrate `@effect-atom/atom`, `@effect-atom/atom-react`, `effect`; the offline cell binds the `platform` `LocalPersistence` `KeyValueStore` through the `Atom.kvs` runtime option.
- Wires internal to `binding/`; the offline cell reads the `platform/local-persistence#LOCAL_PERSISTENCE` `LocalPersistence` `KeyValueStore` as a ui-declared requirement the SPA root satisfies, never a `platform` import; the streamed feeds source the `projection` folds, and mutations leave through the `interchange` `CommandGateway`, never a transport directly.
- Constraints: `Atom.searchParam` returns `Writable<Option<A>>` under a schema and `Atom.kvs` requires the runtime/key/schema/defaultValue quad; the React Compiler removes manual memoization so no leaf carries `useMemo`/`useCallback`.

[T-ROLE-VOCABULARY] [QUEUED] — interaction-role vocabulary and accessibility broadcast
- Author `component-system/role-behavior.md` (the behavior-bearing `_Roles` `as const satisfies Record` owner-block over eight roles carrying `politeness`/`behaviors`/`focusable` columns, the `InteractionRole` `keyof typeof`-projected literal, and the `RoleBehavior` contract) and `component-system/accessibility-broadcast.md` (the live-region path composing the role vocabulary's `announceFor` politeness projection by reference, and the external-store `ToastBroadcast` queue).
- Integrate `react-aria`, `react-aria-components`, `react-stately`, `@react-aria/live-announcer`, the `@radix-ui` slot/label/separator/visually-hidden primitives, `isomorphic-dompurify`, `effect`.
- Wires internal to `component-system/`; stateful roles read state through `binding/atom-binding#ATOM_BINDING`, overlay placement composes `overlay/floating-anchor#FLOATING_ANCHOR`, the gesture algebra composes `motion/gesture-algebra#GESTURE_ALGEBRA`, and tokens compose `theming/theme-tokens#THEME_TOKENS`.
- Constraints: only the role vocabulary and headless-behavior contract are owned, never a per-component `.tsx`; the politeness column lives on the role-vocabulary row so the announce path is total over the eight roles and a new role without a politeness column is a typecheck failure.

[T-THEME-TOKENS] [QUEUED] — OKLCH token engine and CSS-variable sync
- Author `theming/theme-tokens.md`: `ThemeTokens` generating the perceptually-uniform OKLCH scale and the derived contrast/dark/high-contrast records, and `CssVarSync` as the single Tailwind CSS-variable runtime path over one `Stream` fold.
- Integrate `colorjs.io` for the OKLCH ramp and contrast pairing, `tailwindcss`/`tailwind-merge`/`class-variance-authority` for utility token consumption, `effect` for the `Stream` sync.
- Wires internal to `theming/`; the active-theme cell reads `binding/atom-binding#ATOM_BINDING`; the token record feeds both the Tailwind `@theme` layer and the runtime `:root` custom properties.
- Constraints: the CSS-var sync is the single theme-to-runtime path so a theme swap is one record value with no re-render cascade; a direct `document.documentElement.style` write outside `CssVarSync` is the named defect.

[T-OBSERVATION-ROUTES] [QUEUED] — read-only dashboard routes
- Author `observation/observation-routes.md`: `EvidenceTimelineRoute` (HLC order + `SkewBand`), `BenchmarkRoute` (fingerprint-gated through the `stampLine` projection), and `CollectorPanel` as leaf subscribers that read and never emit.
- Integrate `react`, `react-dom`, `@tanstack/react-virtual` for the timeline, `@tanstack/react-table` for tabular reads, `@effect/opentelemetry` strictly as a collector reader, `effect`.
- Wires the routes subscribe through `binding/atom-binding#ATOM_BINDING` to the `projection` evidence and receipt stores; the benchmark gate reads the host-fingerprint shape on `interchange` `decode-rail#TS_PROJECTION`; instrumentation belongs to `platform`, never a route.
- Constraints: a benchmark claim displayed without the fingerprint gate is the named defect; the `stampLine` projection reproduces the upstream `HostFingerprint.StampLine()` verbatim.

[T-CARTOGRAPHY] [QUEUED] — GeoSeriesLayer cartographic surface
- Author `cartography/geo-series-layer.md`: the one `GeoSeriesLayer` closed `Data.TaggedEnum` family over the maplibre base substrate and the deck.gl overlay layers, deepened with the GeoArrow layer family and `TileLayer` for out-of-core data keyed by `featureKind`.
- Integrate `maplibre-gl`, `@deck.gl/core`, `@deck.gl/layers`, `@deck.gl/mapbox`, `effect`.
- Wires the surface sources geometry only through `interchange` `GeometryRail` decoded on `interchange` `decode-rail#TS_PROJECTION`; the maplibre `Map` is an `Effect.acquireRelease` resource over a ui-owned host capability the SPA root satisfies with `platform/browser-platform#BROWSER_PLATFORM` `BrowserPlatform`, never a `platform` import; the view state reads `binding/atom-binding#ATOM_BINDING`.
- Constraints: the four prior geo aliases are collapsed into the one tagged family dispatched by `$match`, never a hand-rolled `Schema.Union` of `_tag`-bearing structs; a second decode beside `GeometryRail` and a free-React-ref `Map` are the named defects; the GeoArrow/`TileLayer` member spellings are a catalogue-verification dependency (see `T-API-CATALOGUE`).

[T-VIEW-TRANSITIONS] [QUEUED] — surface-transition lifecycle union
- Author `motion/view-transitions.md`: one `SurfaceTransition` `Data.TaggedEnum` over the `swap` (`<ViewTransition>` keyed by the route axis) and `preserve` (`<Activity mode>` mounted-but-hidden) lifecycles under one total `mountSurface` `$match`, with the non-reactive transition callback through `useEffectEvent`.
- Integrate `react`, `react-dom`, `effect`; no gesture package on this card.
- Wires internal to `motion/`; the `preserve` case preserves the `viewport/glb-viewport#GLB_VIEWPORT` GL context, the tabbed `observation` routes, and the background `cartography` layers; the route cell reads `binding/atom-binding#ATOM_BINDING`.
- Constraints: the `preserve` case's `<Activity>` mode preserves atom subscriptions and GPU buffers while effects unmount, so a re-show is instant; a `RouteTransition`/`ActivitySurface` sibling-component pair, or a teardown/rebuild on route or tab switch, is the named defect; the React `<Activity>`/`<ViewTransition>`/`useEffectEvent` and `document.startViewTransition` member spellings are a catalogue-verification dependency (see `T-API-CATALOGUE`).

[T-GESTURE-ALGEBRA] [QUEUED] — shared pointer-gesture algebra
- Author `motion/gesture-algebra.md`: the `CameraGesture` `Data.TaggedEnum` over orbit/pan/dolly/frame, the total `applyGesture` fold under `Match.tagsExhaustive`, and the `GestureFold` recognizer binding mapping raw drag/pinch/wheel onto the gesture tags.
- Integrate `@use-gesture/react`, `effect`.
- Wires internal to `motion/`; the gesture fold drives state through `binding/atom-binding#ATOM_BINDING` and the `viewport/glb-viewport#GLB_VIEWPORT` camera `RoleBehavior` composes it by reference, never re-declaring it.
- Constraints: the algebra is owned once in `motion/` and a `CameraGesture`/`applyGesture` re-declaration on the viewport leaf is the named defect; the fold is pure and a state mutation inside `applyGesture` is the named defect; the `@use-gesture/react` recognizer-hook spellings are a catalogue-verification dependency (see `T-API-CATALOGUE`).

[T-OVERLAY] [QUEUED] — floating-anchor positioning owner
- Author `overlay/floating-anchor.md`: `FloatingAnchor` over the @floating-ui/react middleware stack keyed by the overlay kind, the CSS Anchor Positioning bridge, and the one focus-trap/dismiss contract.
- Integrate `@floating-ui/react`, `@floating-ui/react-dom`, `react-aria`, `react-stately`, `effect`.
- Wires internal to `overlay/`; the overlays and navigation `RoleBehavior` rows compose one `FloatingAnchor`; the open state reads `binding/atom-binding#ATOM_BINDING`.
- Constraints: placement is owned once, not per overlay; a hand-rolled `getBoundingClientRect` placement is the named defect; the floating-ui hook and middleware spellings are a catalogue-verification dependency (see `T-API-CATALOGUE`).

[T-VIEWPORT-BACKEND] [QUEUED] — Three.js auto-backend collapse
- Author the `viewport/glb-viewport.md` `RendererBackend` axis as the two-row reality: the `three` row over the `WebGPURenderer` with `init()` auto-detect and WebGL fallback, and the `model-viewer` zero-GL-handle embed row; the `acquireRelease` GL context and the camera `RoleBehavior` row are authored against the in-memory `MeshView`.
- Integrate `three`, `@google/model-viewer`, `@webgpu/types`; each is admitted-on-precondition until the mesh-wire promotion lands.
- Wires the camera fold composes `motion/gesture-algebra#GESTURE_ALGEBRA`; the GL context binds a ui-owned `ViewportHost` capability the SPA root satisfies with `platform/browser-platform#BROWSER_PLATFORM` `BrowserPlatform`, never a `platform` import; the backgrounded viewport composes `motion/view-transitions#VIEW_TRANSITIONS` `<Activity>`.
- Constraints: the Babylon and raw-WebGPU rows are deleted given universal WebGPU; the meshlet/cluster-LOD ambition rides Three.js TSL compute on the `three` row; the `WebGPURenderer` import path and `init()` contract are a catalogue-verification dependency (see `T-API-CATALOGUE`).

[T-MESH-DECODE-SEAM] [BLOCKED] — GlbViewport mesh-DECODE seam
- Wire `viewport/glb-viewport.md` `decodeMeshView` to consume the generated `GeometryPayload(mesh)`/`MeshTensor` descriptor by reference once the C# `csharp:Rasm.Compute/remote/channels#TS_PROJECTION` cluster promotes it out of the proto vocabulary into the projection fence; the backend/draw/camera owners already author against the in-memory `MeshView`.
- Integrate the generated `remote_lane_pb` descriptor by reference; no branch-side wire struct.
- Wires to the cross-language C# wire only — the GLB BYTE layer is unblocked through `interchange` `ArtifactFrameRail`, the mesh-TENSOR projection over the blob bytes is the blocked end; the camera state reads `binding/atom-binding#ATOM_BINDING`.
- Blocked on the upstream C# mesh-shape promotion; re-authoring a branch-side `MeshTensorWire`, reaching a C# geometry interior, or a second GLB decode beside the rail is the named defect. The `point_cloud`/`voxel` oneof arms re-enter the same decode fold as sibling cases once their arms ride a draw row.

[T-BCF-ANCHOR-SURFACE] [BLOCKED] — BCF anchor-algebra render surface
- Author the BCF anchor-algebra render surface (a viewpoint/issue-anchored observation overlay) once the upstream C# anchor-algebra fence promotes the anchor shape into the projection fence.
- Integrate the generated anchor descriptor by reference; reuses the existing `observation`/`viewport` leaves, never a new decode.
- Wires to the cross-language C# wire only — the surface reads the promoted anchor shape through the `interchange` rail and composes the `viewport` viewpoint and the `observation` evidence fold.
- Blocked on the upstream anchor-algebra fence; the surface is a projection over settled owners, not a second decode.

[T-API-CATALOGUE] [BLOCKED] — folder .api catalogue rows for the new admissions
- Add catalogue rows to the folder's own `ui/.api/ui-stack.md` for the members the new pages name as RESEARCH: effect-atom advanced constructors are verified, but the `react-aria-components` `ToastQueue`/`useToastRegion`/`Toast`, `@react-aria/live-announcer` `announce`, Three.js `WebGPURenderer`/`init`, `@google/model-viewer` element attributes, `@floating-ui/react` hooks/middleware, `@use-gesture/react` recognizers, `colorjs.io` OKLCH ramp/contrast, the deck.gl GeoArrow/`TileLayer` constructors, and the React `<Activity>`/`<ViewTransition>`/`useEffectEvent` surface are unverified.
- Integrate the decompile/reflection evidence per package; no source change in `ui/`.
- Wires to the folder `ui/.api/` catalogue, never a consolidated branch-level home; each RESEARCH fence member promotes to prose only after its catalogue row exists.
- Blocked on admitting the rows into `ui/.api/`; an unverified member stays a RESEARCH item per the standard, never a transcribed signature.

[T-LIVE-BINDING-DASHBOARD] [BLOCKED] — live-wire binding-studio observation surface (from LIVE_BINDING_STUDIO_DASHBOARD)
- Author an `observation` leaf rendering the live-wire binding studio: a per-binding connect/subscribe/stale/fault health table, the source-versus-canonical unit coercion read off `CoercedValueWire`, and the write-back disposition as a literal-discriminated outcome, each binding-status key the decoded smart-enum string verbatim.
- Integrate `react`, `react-dom`, `@tanstack/react-table` for the binding-status table, `@tanstack/react-virtual` for a high-cardinality binding set, `effect`; no transport package on this card.
- Wires the leaf subscribes through `binding/atom-binding#ATOM_BINDING` to a `projection` live cell folding the binding-status changefeed; reads only the C# `csharp:Rasm.AppHost/live-wire#TS_PROJECTION` `BindingStatusWire`/`WriteReceiptWire`/`CoercedValueWire` decoded shapes; the write receipt reconstructs through the existing `ReceiptEnvelopeWire`; the leaf reads and never emits.
- Blocked on the upstream C# live-wire `TS_PROJECTION` fence promoting the `BindingStatusWire`/`WriteReceiptWire`/`CoercedValueWire` shapes into the projection fence and a `projection` live cell row carrying the binding-status changefeed; a branch-side binding-status enum, a second decode beside the rail, or a transport dial from the dashboard is the named defect.

## [2]-[CLOSED]

None.
