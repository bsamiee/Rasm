# [UI_ARCHITECTURE]

The professional domain map of `ui` — the host-free browser UI/UX/component library. One `AtomBinding` reactive spine, a headless `interaction` tier, the OKLCH `theming` engine, the `overlay` owner, and read-only `render` leaves over the decoded wire.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
ui/
├── binding/           # the single sanctioned reactive-binding spine
│   └── atom.ts        # AtomBinding over Atom.searchParam/kvs/family/pull + the UndoStack fold
├── interaction/       # the headless behavior tier: roles, accessibility, form/picker, command, motion
│   ├── role.ts        # the _Roles/InteractionRole vocabulary owner-block + RoleBehavior contract
│   ├── announce.ts    # the live-region accessibility-broadcast path + ToastQueue store
│   ├── form.ts        # the schema-driven FormBinding folding decode into the aria validity map
│   ├── picker.ts      # the perceptual color/date/file PickerBehavior family over react-aria
│   ├── command.ts     # the CommandAction AEC action-lexicon, cmdk palette, and variant-recipe owner
│   ├── gesture.ts     # the shared CameraGesture/GestureFold pointer-gesture algebra
│   └── transition.ts  # the SurfaceTransition route/layout transition with reduced-motion gate
├── theming/           # the OKLCH design-token engine and CSS-variable runtime sync
│   └── tokens.ts      # ThemeTokens OKLCH scale + CssVarSync Tailwind sync + tw-animate-css layer
├── overlay/           # the floating/presence/anchor owner the surfaces compose
│   ├── floating.ts    # useFloatingAnchor placement, the CSS Anchor bridge, and the dismiss law
│   ├── presence.ts    # the PresenceOverlay collaborator-cursor cohort over the convergence changefeed
│   └── bcf.ts         # the BCF viewpoint/issue-anchored overlay decoding BcfTopicWire/BcfViewpointWire
└── render/            # the read-only render leaves over the projection folds and interchange rails
    ├── glb.ts         # the 3D mesh render leaf over the ArtifactFrameRail GLB blob
    ├── geo.ts         # the 2D geospatial GeoSeriesLayer over the GeometryRail
    ├── dashboard.ts   # the live-wire binding-studio cockpit over decoded status/receipt rows
    └── routes.ts      # the read-only observation routes: EvidenceTimeline, Benchmark, CollectorPanel
```

`interaction` unifies the headless behavior tier — the `command`/styling-recipe surface, the `gesture`/`transition` motion owners, the interaction-role vocabulary, and the form/picker behaviors. `render` unifies the read-only leaves (`glb`, `geo`, `dashboard`, `routes`) that project the `projection` folds and the `interchange` rails. `binding` and `theming` stay single-file foundational owners — the one `AtomBinding` spine and the OKLCH token engine every surface composes.

## [2]-[SEAMS]

```text seams
overlay/bcf          ←  csharp:Rasm.Bim/Review             # BcfTopicWire / BcfCommentWire / BcfViewpointWire (wire)
render/dashboard     ←  csharp:Rasm.AppHost/Wire           # BindingStatusWire / CoercedValueWire / WriteReceiptWire (wire)
render/glb           ←  csharp:Rasm.Compute/Runtime        # GeometryPayload proto descriptor / MeshTensor view (wire)
render/routes        ←  csharp:Rasm.AppHost/Observability  # BenchmarkClaimWire / HostFingerprintWire identity gate (wire)
render/glb           ←  csharp:Rasm.AppUi/Render           # ResidencyManifest content-key-keyed mesh residency (receipt)
overlay/bcf          ←  csharp:Rasm.Bim/Model              # GlobalId element selection set (shape)
render/glb           ⇄  typescript:platform/transport      # ContentKey mint and tile keying (content-key)
binding/atom         →  typescript:platform/persistence    # LocalPersistence KeyValueStore via Atom.kvs (port)
interaction/command  ←  typescript:interchange/transport   # CommandGateway / IntentRegistry intent dial (port)
interaction/command  ←  typescript:projection/evidence     # AvailabilityStore.isEnabled dial-time gate (port)
interaction/form     →  typescript:interchange/transport   # FormBinding intent dial via CommandGateway (port)
render/glb           ←  typescript:platform/transport      # ViewportHost / DecodeWorkerPool / BrowserPlatform (port)
*                    ←  typescript:interchange             # URL-resident state via Atom.searchParam (wire)
```
