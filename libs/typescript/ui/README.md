# [UI]

`ui` is the browser UI/UX/components library of the TypeScript branch — the AppUi-analog, the lower browser-stratum library beneath the `platform` AppHost-analog entry. It is the single sanctioned `AtomBinding` reactive-binding spine, the leaf render-surfaces over the decoded wire vocabulary, and the interaction-role component-system vocabulary. It holds no domain state and authors no decode: every domain read flows through a `projection` store and the one `AtomBinding`, every geometry read through the `interchange` `GeometryRail`, and every mutation leaves only through the `interchange` `CommandGateway`. Zero consumers exist; implementation is full-capability with no holding back; `.planning/` pages are transcribed, never re-designed. `ui/**` never imports `platform/**`; the `platform` CompositionRoot composes this library, never the reverse. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                  | [OWNS]                                                                            |
| :-----: | :----------------------------------------------------- | :------------------------------------------------------------------------------- |
|   [1]   | [binding](.planning/binding.md)                       | AtomBinding, DeepLinkBinding, UndoStack, OfflineState, the dev-build atom inspector |
|   [2]   | [render-surfaces](.planning/render-surfaces.md)       | the observation routes, GeoSeriesSurface/GeoSeriesLayer, the GlbViewport WebGL render |
|   [3]   | [component-system](.planning/component-system.md)     | the InteractionRole owner-block, RoleBehavior, ThemeTokens, CssVarSync             |

## [2]-[ADMISSIONS_RECORD]

Each package maps to its consuming page, central catalogue at `libs/typescript/.api/`, and admission status. Concrete coordinates live in the workspace catalog (`pnpm-workspace.yaml` `catalog:`); this table never carries a pin. `[STATUS]` is one of `admitted`, `admitted-on-precondition`.

| [INDEX] | [PACKAGE]                                        | [PAGE]                            | [CATALOGUE]                       | [STATUS]                  |
| :-----: | :---------------------------------------------- | :------------------------------- | :-------------------------------- | :------------------------ |
|   [1]   | react + react-dom                               | binding, render, component-system | `.api/api-ui-stack.md`           | admitted                  |
|   [2]   | @effect-atom/atom + @effect-atom/atom-react     | binding                          | `.api/api-effect-atom.md`         | admitted                  |
|   [3]   | maplibre-gl                                     | render-surfaces                  | `.api/api-infra-data.md`          | admitted                  |
|   [4]   | @deck.gl/core + @deck.gl/layers + @deck.gl/react | render-surfaces                  | `.api/api-infra-data.md`          | admitted                  |
|   [5]   | @tanstack/react-virtual                         | render-surfaces                  | `.api/api-ui-stack.md`            | admitted                  |
|   [6]   | @effect/opentelemetry                           | render-surfaces                  | `.api/api-effect-opentelemetry.md` | admitted (collector reader) |
|   [7]   | react-aria + react-aria-components + react-stately | component-system               | `.api/api-ui-stack.md`            | admitted                  |
|   [8]   | colorjs.io                                      | component-system                 | `.api/api-ui-stack.md`            | admitted                  |
|   [9]   | tailwindcss                                     | component-system                 | `.api/api-ui-stack.md`            | admitted                  |
|  [10]   | isomorphic-dompurify                            | component-system                 | `.api/api-ui-stack.md`            | admitted                  |
|  [11]   | effect                                          | all pages                        | `.api/api-effect.md`              | admitted                  |
|  [12]   | three / @babylonjs/core / @google/model-viewer / @webgpu/types | render-surfaces    | `.api/api-ui-stack.md`            | admitted-on-precondition  |

## [3]-[PROOF_GATES]

`[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]          | [RAIL]                            | [EVIDENCE]                                                  |
| :-----: | :-------------- | :-------------------------------- | :--------------------------------------------------------- |
|  [G1]   | catalog resolve | `pnpm` install/restore            | `catalogMode` strict resolves the browser-stratum admissions |
|  [G2]   | typecheck       | `tsgo` typecheck                  | zero diagnostics over the domain                            |
|  [G3]   | stratum lint    | centralized lint config over `ui/**` | no `ui`->`platform` import, no node-platform import      |
|  [G4]   | unit-pbt        | `vitest` project `browser`        | the `UndoStack` fold and `announceFor` total-dispatch laws pass |
|  [G5]   | browser-e2e     | `vitest` browser-mode (playwright) | leaf subscriber renders the projection fold via `AtomBinding` |
|  [G6]   | page render     | local mermaid-cli                 | page diagrams render through the local renderer             |
