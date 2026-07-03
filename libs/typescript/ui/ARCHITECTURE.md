# [UI_ARCHITECTURE]

The domain map of `ui` — the W4 component-capability folder, two Nx projects in one folder: the `ui` core carrying the atom, token, act, view, and intl planes on the React 19 spine, and `ui/viewer` (`scope:viewer`) carrying the spatial/GLB/geo/BCF tier compile-time excluded from non-spatial apps. Components are capability, boot is runtime — `ui` and `browser` are peers an app composes; the permitted folder edges are `kernel`, `state`, and `wire` through the `#vocab` subpath only.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
libs/typescript/ui/
├── src/                   # ui core Nx project — React 19 spine, react-compiler enabled
│   ├── atom/              # @effect-atom state-binding plane
│   │   ├── binding.ts     # @effect-atom one-binding law (ONE_FOLD_ONE_BINDING) — the one state binding; AtomHttpApi/AtomRpc direct-binding rows
│   │   └── derive.ts      # derived atoms/selectors over state folds + undo/redo stack folds
│   ├── token/             # design-token plane
│   │   ├── theme.ts       # design tokens + theming rows
│   │   └── scale.ts       # spacing/typography scale vocabulary + motion token rows (tw-animate)
│   ├── act/               # interaction plane
│   │   ├── transition.ts  # native View Transitions API; <ViewTransition>/<Activity> gated upgrade row
│   │   └── gesture.ts     # interaction/gesture rows (react-aria)
│   ├── view/              # headless view plane
│   │   ├── primitive.ts   # react-aria headless component spine + live-region announce/toast rows
│   │   └── compose.ts     # composition/slot patterns + Schema→aria FormBinding/picker rows; command-palette (cmdk), table/virtual collection, floating-anchor/sheet, presence-cursor cohort rows
│   └── intl/              # localization plane — zero i18n package
│       ├── message.ts     # Schema-typed message-catalog rows keyed by the kernel Locale brand + plural/select folds over native Intl — catalogs are app data
│       └── format.ts      # number/date/list/relative-time format rows composing react-aria I18nProvider/useLocale over native Intl
└── viewer/src/            # second Nx project — scope:viewer, spatial tier compile-time excluded
    ├── scene/             # GLB scene plane
    │   ├── glb.ts         # GLB_VIEWPORT scene residency + three rows; consumes the browser decode-worker port; meshopt decode gated
    │   └── appearance.ts  # OpenPBR appearance binding over wire#vocab appearance
    ├── geo/               # geospatial plane
    │   ├── layers.ts      # maplibre/deck.gl geo layers + turf planar ops (WKB decode stays in wire)
    │   └── project.ts     # projection/camera sync rows
    ├── mark/              # overlay-mark plane
    │   ├── bcf.ts         # BCF topic/viewpoint anchors (GlobalId)
    │   └── selection.ts   # GlobalId selection sets
    ├── probe/             # render-evidence plane
    │   ├── receipt.ts     # RenderReceipt frame-hash probes
    │   └── benchmark.ts   # BenchmarkClaim/HostFingerprint probes
    └── panel/             # wire-bound panel plane
        ├── binding.ts     # livewire binding panels
        ├── control.ts     # ControlIntent panels
        └── layout.ts      # @lume/kiwi Cassowary layout re-solve to identical positions
```

Growth lands on owning rows: a new component family is a `view` row; a new token axis is a `token` row; a new interaction pattern is an `act` row; a new locale is `intl` catalog data, never a lib edit; a new viewer overlay is a `mark` row; a new panel kind is one `panel` row over its wire vocabulary.

## [02]-[SEAMS]

```text seams
viewer/scene/glb         ←  csharp:Rasm.AppUi/Render/glb                    # [RECEIPT]: ResidencyManifest content-key-keyed mesh residency
viewer/scene/appearance  ←  csharp:Rasm.Materials/Appearance/interchange    # [WIRE]: decode-only MaterialWire/OpenPbrGroupsWire/AppearanceSummary mirroring the C# projection field-for-field; a peer re-mint of the OpenPBR algebra is the CROSS_LANGUAGE_WIRE drift defect
viewer/mark/bcf          ←  csharp:Rasm.Bim/Review/issues                   # [WIRE]: BcfTopicWire / BcfViewpointWire
viewer/mark/bcf          ←  csharp:Rasm.Bim/Exchange/wire                   # [WIRE]: BcfWire/DiffWire GlobalId anchor decode
viewer/mark/selection    ←  csharp:Rasm.Bim/Model/elements                  # [SHAPE]: GlobalId element selection set
viewer/probe/benchmark   ←  csharp:Rasm.AppHost/Observability/Telemetry.cs  # [WIRE]: BenchmarkClaimWire / HostFingerprintWire identity gate
viewer/panel/binding     ←  csharp:Rasm.AppHost/Wire/Livewire.cs            # [WIRE]: BindingStatusWire / CoercedValueWire / WriteReceiptWire
viewer/panel/control     ←  csharp:Rasm.AppUi/Shell/controls                # [WIRE]: ControlIntentWire kind-discriminated control vocabulary
viewer/panel/layout      ←  csharp:Rasm.AppUi/Shell/solver                  # [WIRE]: LayoutConstraintWire ordered Kiwi constraint program
```

Every `[WIRE]` row decodes once at `wire`; `ui` types the decoded values through the `wire` `#vocab` subpath only — the codec machinery interior is unexported, so a `ui` module physically cannot resolve it. `ui` and `browser` never import each other: where a component needs a runtime capability, `ui` declares the port record (`GlbViewport` decode-worker residency) and `browser` provides the Layer at app composition; the app root wires the port.
