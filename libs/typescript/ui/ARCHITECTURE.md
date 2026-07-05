# [TS_UI_ARCHITECTURE]

The domain map of `ui` — the wave-4 interface package and its sibling `viewer` Nx project. Three sub-domains (`system`, `view`, `viewer`) meet through one atom binding, one styled recipe, one motion vocabulary, and one selection plane; the viewer renders decoded wire vocabularies and owns zero semantics.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, camelCase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
ui/
├── src/
│   ├── system/                # The component system: tokens, interaction, state binding, locale, primitives
│   │   ├── token.ts           # The design-token authority: OKLCH ramps APCA-gated at decode, dimension vocabulary, Theme.linear egress
│   │   ├── act.ts             # Motion and interaction: react-aria discrete events, use-gesture analog gestures, the MOTION_ROWS vocabulary
│   │   ├── atom.ts            # The one state binding: Store.make over the app Layer graph, REMOTE_BINDING, the LIVE_BRIDGE host-plane rows
│   │   ├── intl.ts            # The zero-i18n-package locale plane: I18nProvider locale spine + the per-locale native-Intl cache
│   │   └── primitive.ts       # The headless spine: the styled recipe fusing cva variants with RAC render state; the sanitize gate
│   └── view/                  # The view plane over the system owners
│       ├── form.ts            # Schema-driven forms: one kernel Schema owning wire decode AND live field validity via standardSchemaV1
│       ├── table.ts           # The data grid: TanStack models + Virtual windows + RAC grid semantics under ONE TableState atom
│       └── overlay.ts         # The overlay owner: floating-ui anchoring, vaul sheets, cmdk palette over Overlay.Command, presence cohort
└── viewer/
    └── src/                   # The spatial tier (second Nx project)
        ├── scene.ts           # The GLB scene: content-key mesh residency behind the GlbViewport port; three | model-viewer backend rows
        ├── geo.ts             # The geospatial surface: one maplibre Map + MapboxOverlay-interleaved deck.gl layers as a pure value tree
        ├── mark.ts            # The GlobalId mark plane: one HashSet selection atom, a closed op vocabulary, every pick pipeline folding into it
        ├── panel.ts           # The wire materializer: livewire triple, ControlIntent union, LayoutProgram rendered through the system owners
        └── probe.ts           # Render evidence: engine-counter benchmarks paired with wire-decoded receipts as operator evidence
```

## [02]-[SEAMS]

```text seams
view/table    ←  typescript:core/state      # [SHAPE]: Feed.Document column band (name/kind/dimension/nullable, Option-carried columns)
system/atom   ⇄  typescript:runtime/browser # [PORT]: Router/Install/Guard/Vault Subscribable planes over Atom.subscribable rows
system/token  →  typescript:ui/viewer       # [PROJECTION]: Theme.linear srgb-linear ingestion into the scene appearance leaves
viewer/scene  ←  typescript:runtime/browser # [PORT]: GlbViewport satisfied from Depot.haul verified arrivals + the residency ledger
viewer/scene  ←  typescript:runtime/serve   # [BOUNDARY]: self-hosted draco/basis/meshopt transcoder assets served byte-identical
viewer/scene  ←  csharp:Rasm.Materials      # [WIRE]: PbrGroups appearance decode
viewer/panel  ←  csharp:Rasm.AppHost        # [WIRE]: livewire triple BindingStatus/CoercedValue/WriteReceipt
viewer/panel  ←  csharp:Rasm.AppUi          # [WIRE]: ControlIntent six-kind union + the ordered LayoutProgram
viewer/mark   ←  csharp:Rasm.Bim            # [WIRE]: BcfTopic/BcfViewpoint marks + GlobalId selection sets
viewer/probe  ←  csharp:Rasm.AppUi          # [RECEIPT]: RenderReceipt claims paired with local render evidence
```

## [03]-[ORGANIZATION]

`system` is the capability floor the views instantiate: `token` computes color and dimension as decode-gated data, `act` splits interaction by kind (discrete accessible events vs continuous gestures) and owns the motion vocabulary, `atom` is the ONE state binding standing the app's Layer graph behind the registry, `intl` rides native `Intl` behind one cache, and `primitive` makes react-aria-components the single headless pattern. `view` composes those owners into the three dense view surfaces — form, grid, overlay — each a single owner where variation is rows (columns, commands, field kinds), never sibling components. `viewer` is the spatial tier as a separate Nx project: `scene` renders content-keyed GLB residency behind a port the browser composition root satisfies, `geo` shares one WebGL context between maplibre and deck.gl, `mark` holds the one selection atom every pick pipeline and echo consumer projects (the grid `RowSelectionState` and the `scrollToIndex` echo are projections of this atom, never a second selection), `panel` materializes the C#-minted control vocabularies, and `probe` renders evidence without gating.

## [04]-[BOUNDARIES]

- The folder evaluates no IFC semantics and owns no geometry; GLB, BCF, and selection vocabularies arrive decoded through the core interchange plane and are rendered, never re-authored.
- The browser composition root — satisfying `GlbViewport` from Depot arrivals and binding the host Subscribable planes into atoms — is app composition, out of this folder's scope.
- `EXT_meshopt_compression` assets refuse with the `codec-absent` reason until the iac plane admits the wasm decoder identity and its serving row.
- History consumers (selection sets, camera bookmarks) compose from the landed system pages; a second history owner never appears beside the selection atom.
