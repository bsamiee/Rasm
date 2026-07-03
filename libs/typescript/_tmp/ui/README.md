# [UI]

`ui` is the host-free browser UI/UX/components library of the TypeScript branch — the AppUi-analog, the lower browser-stratum library beneath the `platform` AppHost-analog. It owns the single sanctioned `AtomBinding` reactive-binding spine, the headless interaction-role component vocabulary, and the leaf render-surfaces over the decoded C# wire. The library holds no domain state and authors no decode: every domain read flows through a `projection` store and the one `AtomBinding`, every geometry read through the `interchange` `GeometryRail`, and every mutation leaves only through the `interchange` `CommandGateway`. This is a pure browser DOM/WebGL/WebGPU library, distinct in kind from the C# Avalonia AppUi, which is a desktop host-bound surface. `ui/**` never imports `platform/**`. The `.planning/` design pages are routed below; every external package the folder draws on is registered under `## [2]-[DOMAIN_PACKAGES]` and `## [3]-[SUBSTRATE_PACKAGES]`. The domain folder-map lives in `ARCHITECTURE.md`, the forward concept pool in `IDEAS.md`, and open work in `TASKLOG.md`.

## [01]-[ROUTER]

- [01]-[ATOM](.planning/binding/atom.md): the `AtomBinding` spine over `Atom.searchParam`/`kvs`/`family`/`pull`, the `UndoStack` fold, and the `Result.builder` render chain.
- [02]-[ROLE](.planning/interaction/role.md): the behavior-bearing `_Roles`/`InteractionRole` vocabulary owner-block and the `RoleBehavior` contract.
- [03]-[ANNOUNCE](.planning/interaction/announce.md): the live-region announce path over the role `politeness` column and the `ToastQueue` external-store queue.
- [04]-[FORM](.planning/interaction/form.md): the headless `FormBinding` folding `Schema.decodeUnknownEither` validation into the `Form` `validationBehavior: "aria"` error map.
- [05]-[PICKER](.planning/interaction/picker.md): the headless `PickerBehavior` family realizing the `pickers` role's color/date/file behaviors over react-stately color state and react-aria-components calendar/date/file widgets, with color channels projected through the OKLCH token space.
- [06]-[COMMAND](.planning/interaction/command.md): the `CommandAction` AEC action-lexicon vocabulary rendered through the `cmdk` palette and the `vaul` drawer, the `lucide-react` icon vocabulary keyed by action, the one `cva`+`twMerge` variant-recipe owner, and the `useFilter`/`UNSTABLE_useFilteredListState` filtering surface, dialing only through the `interchange` `CommandGateway`.
- [07]-[GESTURE](.planning/interaction/gesture.md): the shared `CameraGesture`/`GestureFold` pointer-gesture algebra.
- [08]-[TRANSITION](.planning/interaction/transition.md): the `SurfaceTransition` union over `<ViewTransition>` swap and `<Activity>` preserve.
- [09]-[TOKENS](.planning/theming/tokens.md): the `ThemeTokens` OKLCH scale, the `CssVarSync` Tailwind CSS-variable sync, and the `tw-animate-css` enter/exit utility layer.
- [10]-[FLOATING](.planning/overlay/floating.md): `useFloatingAnchor` placement over `useFloating`/`useInteractions`, the CSS Anchor bridge, and the dismiss law.
- [11]-[PRESENCE](.planning/overlay/presence.md): the `PresenceOverlay` collaborator-cursor cohort over the `projection` convergence changefeed, with the `[BEAT_PAYLOAD]` residual on the producer `Awareness` beat byte-encoding pin.
- [12]-[BCF](.planning/overlay/bcf.md): the BCF viewpoint/issue-anchored observation overlay decoding the `BcfTopicWire`/`BcfViewpointWire` rows, the `BcfStatusKey` vocabulary, and the `anchorViewpoint` camera-and-GlobalId projection.
- [13]-[GLB](.planning/render/glb.md): `GlbViewport`, `RendererBackend`, `ViewportResource`, and the camera row.
- [14]-[GEO](.planning/render/geo.md): the `GeoSeriesLayer` `$match` over the maplibre base substrate and the deck.gl overlay set, where the `tile` arm streams `TileLayer`/`MVTLayer` and the `geoarrow` arm renders the `@geoarrow/deck.gl-geoarrow` `RecordBatch` layers out of core.
- [15]-[DASHBOARD](.planning/render/dashboard.md): the live-wire binding-studio cockpit over the decoded `BindingStatusWire`/`WriteReceiptWire`/`CoercedValueWire` rows, the `@tanstack/react-table`+`react-virtual` health table, and the `WriteBackWire` `kind`-discriminated disposition.
- [16]-[ROUTES](.planning/render/routes.md): `EvidenceTimelineRoute`, `BenchmarkRoute`, and `CollectorPanel`.

## [02]-[DOMAIN_PACKAGES]

Domain packages owned by this folder; versions are in the workspace catalog (`pnpm-workspace.yaml` `catalog:`) and corroborated by `libs/typescript/ui/.api/`.

[REACT]:
- `@types/react`
- `@types/react-dom`
- `babel-plugin-react-compiler`
- `react-compiler-runtime`

[HEADLESS_A11Y]:
- `react-aria`
- `react-aria-components`
- `react-stately`
- `@react-aria/live-announcer`
- `tailwindcss-react-aria-components`
- `@radix-ui/react-slot`
- `@radix-ui/react-label`
- `@radix-ui/react-separator`
- `@radix-ui/react-visually-hidden`

[POSITIONING_GESTURE]:
- `@floating-ui/react`
- `@floating-ui/react-dom`
- `@use-gesture/react`

[THEMING]:
- `colorjs.io`
- `tailwindcss`
- `tailwind-merge`
- `class-variance-authority`
- `tw-animate-css`

[CONTENT]:
- `lucide-react`
- `cmdk`
- `vaul`

[DATA_SURFACES]:
- `@tanstack/react-virtual`
- `@tanstack/react-table`
- `maplibre-gl`
- `@deck.gl/core`
- `@deck.gl/layers`
- `@deck.gl/geo-layers`
- `@deck.gl/mapbox`
- `@geoarrow/deck.gl-geoarrow`

[VIEWPORT]:
- `three`
- `@types/three`
- `@google/model-viewer`
- `@webgpu/types`

## [03]-[SUBSTRATE_PACKAGES]

Branch-level substrate packages this folder consumes. Substrate policy and full registry live in `libs/typescript/.planning/README.md`; decompile evidence lives in `libs/typescript/.api/`.

[RUNTIME_CORE]:
- `effect` — `Effect.Service` and `Schema` substrate underlying the `AtomBinding` spine and result carriers
- `@effect/platform` — `KeyValueStore` contract the `AtomBinding` `Atom.kvs` offline cell binds at the runtime seam

[REACTIVE_BRIDGE]:
- `@effect-atom/atom` — reactive-store primitives (`Atom.subscriptionRef`, `Atom.subscribable`, `Atom.kvs`, `Atom.family`)
- `@effect-atom/atom-react` — React binding layer (`useAtom`, `useAtomValue`) over the atom store

[OBSERVABILITY]:
- `@effect/opentelemetry` — OTel trace/span integration at the observation boundary

[VIEW_CORE]:
- `react` — React 19 core render substrate (`ReactNode`, hooks, context, suspense) for every component owner on the stack
- `react-dom` — browser DOM portals and form-status hooks (`createPortal`, `useFormStatus`) the component surfaces consume

[SECURITY_SUBSTRATE]:
- `isomorphic-dompurify` — DOM-bound text sanitization for content render surfaces
