# [UI_ARCHITECTURE]

The professional domain map for the host-free browser UI library. Each sub-domain is a genuine browser-UI capability folder mirroring the eventual source tree under `.planning/`, named by its domain concept with a one-line charter. Dependency direction across the TypeScript strata is stated once in the branch `ARCHITECTURE.md`; boundaries and wires live on the task cards that build them.

## [1]-[DOMAIN_MAP]

The binding spine precedes every surface that subscribes through it; the vocabulary owners (component-system, theming) precede the surfaces that compose them; the render leaves (observation, cartography, viewport) are projections over the `projection` folds, the `interchange` `GeometryRail`, and the `interchange` `ArtifactFrameRail`; motion and overlay are cross-cutting interaction owners the surfaces compose.

```text codemap
ui/
├── binding/            # The single sanctioned reactive-binding spine over the effect-atom React hooks and native state constructors
├── component-system/   # The headless interaction-role vocabulary, the live-region accessibility-broadcast path, the schema-driven FormBinding validity fold, and the perceptual color/date/file PickerBehavior family
├── content/            # The CommandAction AEC action-lexicon vocabulary, the cmdk palette and vaul drawer surfaces, the lucide-react icon vocabulary, and the one cva+twMerge variant-recipe owner
├── theming/            # The OKLCH design-token engine, the CSS-variable runtime sync, and the tw-animate-css enter/exit utility layer, distinct from interaction behavior
├── observation/        # The read-only dashboard routes over the projection folds — leaves that read and never emit, including the HLC skew-band confidence-interval render
├── cartography/        # The 2D geospatial surface over the interchange GeometryRail, with the GeoArrow/TileLayer out-of-core and cell-index aggregation arms
├── viewport/           # The 3D mesh render leaf over the interchange ArtifactFrameRail GLB blob, consuming the residency manifest by content key
├── motion/             # The route/layout transition and activity-preservation owner, the reduced-motion gate, and the shared pointer-gesture algebra
└── overlay/            # The floating-surface positioning/anchor-bridge/dismiss owner and the collaborator-presence cohort over the convergence changefeed
```

The `content` sub-domain is a genuine higher-order capability folder, not a top-level candidate: it owns the in-app command and styling-recipe surface (`cmdk`/`vaul`/`lucide-react`/`cva`/`twMerge`) at the same browser-library stratum as the other sub-domains, dialing only through the `interchange` `CommandGateway` and holding no domain state.

Each page is one transcription unit per eventual source file. A new capability deepens the owning sub-domain through rows, cases, and policy values rather than a new public surface beside it. The page clusters transcribe into the TypeScript workspace at web app-root creation; `ui/**` never imports `platform/**`.
