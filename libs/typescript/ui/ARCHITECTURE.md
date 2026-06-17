# [UI_ARCHITECTURE]

The professional domain map for the host-free browser UI library. Each sub-domain is a genuine browser-UI capability folder mirroring the eventual source tree under `.planning/`, named by its domain concept with a one-line charter. Dependency direction across the TypeScript strata is stated once in the branch `ARCHITECTURE.md`; boundaries and wires live on the task cards that build them.

## [1]-[DOMAIN_MAP]

The binding spine precedes every surface that subscribes through it; the vocabulary owners (component-system, theming) precede the surfaces that compose them; the render leaves (observation, cartography, viewport) are projections over the `projection` folds, the `interchange` `GeometryRail`, and the `interchange` `ArtifactFrameRail`; motion and overlay are cross-cutting interaction owners the surfaces compose.

```text codemap
ui/
├── binding/            # The single sanctioned reactive-binding spine over the effect-atom React hooks and native state constructors
├── component-system/   # The headless interaction-role vocabulary and the live-region accessibility-broadcast path every role reuses
├── theming/            # The OKLCH design-token engine and the CSS-variable runtime sync, distinct from interaction behavior
├── observation/        # The read-only dashboard routes over the projection folds — leaves that read and never emit
├── cartography/        # The 2D geospatial surface over the interchange GeometryRail GeoJSON projection
├── viewport/           # The 3D mesh render leaf over the interchange ArtifactFrameRail GLB blob
├── motion/             # The route/layout transition and activity-preservation owner and the shared pointer-gesture algebra
└── overlay/            # The floating-surface positioning, anchor-bridge, and focus-trap/dismiss owner
```

Each page is one transcription unit per eventual source file. A new capability deepens the owning sub-domain through rows, cases, and policy values rather than a new public surface beside it. The page clusters transcribe into the TypeScript workspace at web app-root creation; `ui/**` never imports `platform/**`.
