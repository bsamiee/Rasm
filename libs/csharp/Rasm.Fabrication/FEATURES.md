# [FABRICATION_FEATURES]

The realized fabrication-frontier capability list. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[FABRICATION_FRONTIER]

The one polymorphic fabrication owner dispatching hidden-line, CAM motion, and nesting through one `Run` fold.

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                     |
| :-----: | :------------------------------------------------------------------------------ | :--------------------------------- |
|   [1]   | One `Fabrication` owner — `FrontierKind`/`FrontierPolicy`/`FrontierResult` over one `Run` data-table dispatch | hidden-line#FABRICATION_OWNER      |
|   [2]   | Exact hidden-line — BSP visibility + Weiler-Atherton clip, world-space edge sets for `Viewport2D` | hidden-line#PROJECTION_HIDDEN_LINE |
|   [3]   | CAM toolpath motion — contour/pocket/drill over DH forward kinematics and damped-pseudoinverse IK | toolpath#CAM_MOTION                |
|   [4]   | 2D true-shape nesting — author-kernel no-fit-polygon with bottom-left/genetic placement | nesting#NESTING                    |
