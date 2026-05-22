# [H1][MATHNET_RHINO]
>**Dictum:** *RhinoCommon owns model geometry; MathNet executes selected numerical kernels.*

<br>

[IMPORTANT] Verify Rhino 9/WIP API claims with local RhinoWIP XML/decompile. Public McNeel docs are secondary context.

---
## [1][RHINO_FIRST]
>**Dictum:** *Native semantic owners beat numerical reimplementation.*

<br>

| [INDEX] | [OWNER] | [EXAMPLES] |
| :-----: | ------- | ---------- |
| [1] | Rhino validity and tolerances | `RhinoMath`, `IsValid`, unset sentinels, distance/angle tolerance. |
| [2] | Rhino transforms | `Transform`, `PlaneToPlane`, `ChangeBasis`, affine/rigid/similarity checks. |
| [3] | Rhino curves and surfaces | Derivatives, tangents, frames, closest points, length parameters. |
| [4] | Rhino meshes | Topology, normals, ngons, cleanup, closest points, remesh/reduce operations. |
| [5] | MathNet | Higher-dimensional solves, fitting, symbolic formulas, statistics, diagnostics. |

---
## [2][BOUNDARY]
>**Dictum:** *MathNet receives explicit numeric projections.*

<br>

Convert native values into ordered coordinates, units, and weights before algorithm execution. Project algorithm output back into Rhino types only after checking finiteness, tolerance, dimensionality, and native validity. Do not document MathNet precision as a substitute for Rhino model tolerance.

---
## [3][WIP_FACTS]
>**Dictum:** *WIP-only members need local proof.*

<br>

When documenting Rhino 9/WIP members such as transform scale classification, vector decomposition, pseudo-inverse behavior, or new mesh operations, cite the local `scripts/rhino.sh api xml` or `decompile` command used to prove them. Do not infer availability from older public pages.

---
## [4][RULES]
>**Dictum:** *Geometry meaning stays native.*

<br>

- Prefer Rhino analytic APIs for geometry/topology work.
- Use MathNet when the operation is numerical algorithm work, not native modeling.
- Preserve Rhino sentinels as host facts; do not treat every unset value as generic failure.
- Keep ownership/disposal policy outside MathNet data structures.
