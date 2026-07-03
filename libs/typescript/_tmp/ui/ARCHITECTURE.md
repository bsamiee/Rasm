# [UI_ARCHITECTURE]

The domain map of `ui` — the host-free browser UI/UX/component library. One `AtomBinding` reactive spine, a headless `interaction` tier, the OKLCH `theming` engine, the `overlay` owner, and read-only `render` leaves over the decoded wire.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
ui/
├── binding/           # Single sanctioned reactive-binding spine
│   └── atom.ts        # AtomBinding over Atom.searchParam/kvs/family/pull + UndoStack fold
├── interaction/       # Headless behavior tier: roles, accessibility, form/picker, command, motion
│   ├── role.ts        # _Roles/InteractionRole vocabulary owner-block + RoleBehavior contract
│   ├── announce.ts    # Live-region accessibility-broadcast path + ToastQueue store
│   ├── form.ts        # Schema-driven FormBinding folding decode into aria validity map
│   ├── picker.ts      # Perceptual color/date/file PickerBehavior family over react-aria
│   ├── command.ts     # CommandAction AEC action-lexicon, cmdk palette, and variant-recipe owner
│   ├── gesture.ts     # Shared CameraGesture/GestureFold pointer-gesture algebra
│   └── transition.ts  # SurfaceTransition route/layout transition with reduced-motion gate
├── theming/           # OKLCH design-token engine and CSS-variable runtime sync
│   └── tokens.ts      # ThemeTokens OKLCH scale + CssVarSync Tailwind sync + tw-animate-css layer
├── overlay/           # Floating/presence/anchor owner surfaces compose
│   ├── floating.ts    # useFloatingAnchor placement, CSS Anchor bridge, and dismiss law
│   ├── presence.ts    # PresenceOverlay collaborator-cursor cohort over convergence changefeed
│   └── bcf.ts         # BCF viewpoint/issue-anchored overlay decoding BcfTopicWire/BcfViewpointWire
└── render/            # Read-only render leaves over projection folds and interchange rails
    ├── glb.ts         # 3D mesh render leaf over ArtifactFrameRail GLB blob
    ├── geo.ts         # 2D geospatial GeoSeriesLayer over GeometryRail
    ├── dashboard.ts   # Live-wire binding-studio cockpit over decoded status/receipt rows
    └── routes.ts      # Read-only observation routes: EvidenceTimeline, Benchmark, CollectorPanel
```

`interaction` unifies the headless behavior tier — the `command`/styling-recipe surface, the `gesture`/`transition` motion owners, the interaction-role vocabulary, and the form/picker behaviors. `render` unifies the read-only leaves (`glb`, `geo`, `dashboard`, `routes`) that project the `projection` folds and the `interchange` rails. `binding` and `theming` stay single-file foundational owners — the one `AtomBinding` spine and the OKLCH token engine every surface composes.

## [02]-[SEAMS]

```text seams
overlay/bcf          ←  csharp:Rasm.Bim/Review             # [WIRE]: BcfTopicWire / BcfCommentWire / BcfViewpointWire
render/dashboard     ←  csharp:Rasm.AppHost/Wire           # [WIRE]: BindingStatusWire / CoercedValueWire / WriteReceiptWire
render/glb           ←  csharp:Rasm.Compute/Runtime        # [WIRE]: GeometryPayload proto descriptor / MeshTensor view
render/routes        ←  csharp:Rasm.AppHost/Observability  # [WIRE]: BenchmarkClaimWire / HostFingerprintWire identity gate
render/glb           ←  csharp:Rasm.AppUi/Render           # [RECEIPT]: ResidencyManifest content-key-keyed mesh residency
overlay/bcf          ←  csharp:Rasm.Bim/Model              # [SHAPE]: GlobalId element selection set
render/glb           ⇄  typescript:platform/transport      # [CONTENT_KEY]: ContentKey mint and tile keying
binding/atom         →  typescript:platform/persistence    # [PORT]: LocalPersistence KeyValueStore via Atom.kvs
interaction/command  ←  typescript:interchange/transport   # [PORT]: CommandGateway / IntentRegistry intent dial
interaction/command  ←  typescript:projection/evidence     # [PORT]: AvailabilityStore.isEnabled dial-time gate
interaction/form     →  typescript:interchange/transport   # [PORT]: FormBinding intent dial via CommandGateway
render/glb           ←  typescript:platform/transport      # [PORT]: ViewportHost / DecodeWorkerPool / BrowserPlatform
*                    ←  typescript:interchange             # [WIRE]: URL-resident state via Atom.searchParam
```
