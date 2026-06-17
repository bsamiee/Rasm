# [UI_FEATURES]

The realized capability list for the browser UI library. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[BINDING]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]  |
| :-----: | :----------------------------------------------------------------------------- | :-------------- |
|   [1]   | One sanctioned `AtomBinding` reactive spine over the atom layer                  | binding#BINDING |
|   [2]   | URL-resident state: one Schema-round-tripped key per surface, survives reload    | binding#BINDING |
|   [3]   | Bounded undo/redo/push history fold and last-good offline cell                   | binding#BINDING |
|   [4]   | Dev-build atom inspector as one row stripped from the shipped bundle             | binding#BINDING |

## [2]-[RENDER_SURFACES]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                  |
| :-----: | :----------------------------------------------------------------------------- | :------------------------------ |
|   [5]   | HLC-ordered evidence-timeline route with SkewBand clock-uncertainty render       | render-surfaces#RENDER_SURFACES |
|   [6]   | Fingerprint-gated benchmark route over the receipt store                          | render-surfaces#RENDER_SURFACES |
|   [7]   | Telemetry collector-panel reader leaf                                             | render-surfaces#RENDER_SURFACES |
|   [8]   | Cartographic surface: base map vs overlay layer set over one `GeoSeriesLayer` union | render-surfaces#RENDER_SURFACES |
|   [9]   | WebGL mesh viewport over a Schema.Literal renderer-backend axis                   | render-surfaces#GLB_VIEWPORT    |
|  [10]   | Viewport camera as one `RoleBehavior` row with a total `CameraGesture` fold        | render-surfaces#GLB_VIEWPORT    |

## [3]-[COMPONENT_SYSTEM]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                    |
| :-----: | :----------------------------------------------------------------------------- | :------------------------------- |
|  [11]   | Eight-role headless interaction-role vocabulary owner-block                      | component-system#COMPONENT_SYSTEM |
|  [12]   | Per-role headless `RoleBehavior` contract with exhaustive announce dispatch       | component-system#COMPONENT_SYSTEM |
|  [13]   | Color-space theme tokens with `CssVarSync` as the single theme-to-runtime path    | component-system#COMPONENT_SYSTEM |
