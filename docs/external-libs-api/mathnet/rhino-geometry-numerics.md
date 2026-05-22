# [H1][MATHNET_RHINO_GEOMETRY_NUMERICS]
>**Dictum:** *RhinoCommon owns geometry; MathNet owns managed numerical projection.*

<br>

[IMPORTANT] Convert between Rhino and MathNet only at the numerical boundary. Preserve Rhino tolerance, validity, and unit semantics through `Context` and `Op`.

[IMPORTANT] Baseline: RhinoCommon remains geometry authority; MathNet owns managed numerical projection; `docs/system-api-map/api.md` owns BCL numeric primitives and `TensorPrimitives` candidate policy.

---
## [1][BOUNDARY_TYPES]
>**Dictum:** *Native geometry remains the source of spatial truth.*

<br>

| [INDEX] | [RHINO_TYPE] | [MATHNET_SHAPE] | [RULE] |
| :-----: | ------------ | --------------- | ------ |
| [1] | `Point3d` | `Vector<double>` length 3 plus affine `w=1` intent | Never mix with free-vector semantics after conversion. |
| [2] | `Vector3d` | `Vector<double>` length 3 plus affine `w=0` intent | Require `IsValid`, non-zero, and `!IsTiny(context.Absolute.Value)` when direction matters. |
| [3] | `Plane` | Origin plus right-handed orthonormal basis | `CreateFromFrame`, `CreateFromNormal`, and `CreateFromPoints` can yield `Plane.Unset`; reject unset before MathNet use. |
| [4] | `Transform` | Row-indexed `Matrix<double>` 4x4 | Translation is right column `M03`, `M13`, `M23`; preserve multiplication order and affine/non-affine status. |
| [5] | `BoundingBox` | min/max vectors and corners | Keep origin, min/max, containment, and validity in RhinoCommon. |

[CRITICAL] Do not replace RhinoCommon geometry operations with `MathNet.Spatial`. RhinoCommon remains authoritative for RhinoWIP and GH2 interop.

---
## [2][STORAGE]
>**Dictum:** *Storage convention mistakes create silent geometry bugs.*

<br>

| [INDEX] | [CONCERN] | [POLICY] |
| :-----: | --------- | -------- |
| [1] | Rasm matrix entries | Rasm indexes entries as row-major `Arr<double>`. |
| [2] | MathNet matrices | Use MathNet builders and indexers; never assume external storage layout. |
| [3] | Transform conversion | `transform * point/vector` applies transform; `A * B` applies `B` then `A`. |
| [4] | Transform arrays | Use `ToDoubleArray(true)` for row-dominant and `false` for column-dominant arrays. |
| [5] | Basis semantics | `PlaneToPlane` orients geometry; `ChangeBasis` changes coordinate descriptions. Do not substitute one for the other. |
| [6] | Complex operators | Keep complex values inside spectral/Hermitian algorithms. |
| [7] | Serialization | Serialize Rasm value records, not MathNet objects. |

---
## [3][TOLERANCE]
>**Dictum:** *Numerical algorithms respect Rhino model tolerance.*

<br>

| [INDEX] | [SOURCE] | [USE] |
| :-----: | -------- | ----- |
| [1] | `Context.Absolute` | Geometric closeness, closure, and length thresholds. |
| [2] | `Context.Relative` | Scale-aware residual and convergence thresholds. |
| [3] | `Context.Angle` | Angular comparisons and frame rotation admission. |
| [4] | `RhinoMath.IsValidDouble` | Scalar validity before MathNet conversion and before accepting MathNet output. |
| [5] | `RhinoMath.UnsetValue` | Treat unset as invalid even when `double.IsFinite` passes. |
| [6] | `Op.AcceptValue` | Final validation of projected Rhino values. |

---
## [4][HOST_BEHAVIOR]
>**Dictum:** *Rhino mutability and GH2 access shape conversion policy.*

<br>

| [INDEX] | [CONCERN] | [RULE] |
| :-----: | --------- | ------ |
| [1] | `Vector3d.Unitize` | Mutates and fails for invalid or zero vectors; prefer validated copies. |
| [2] | `Vector3d.VectorAngle` | Returns `RhinoMath.UnsetValue` on failure; route through validity rails. |
| [3] | `Transform.TryGetInverse` | Boolean `false` can still assign a pseudo-inverse for valid transforms; accept inverse-dependent work only when the result is `true`. |
| [4] | `GeometryBase.Transform` | Duplicate geometry before mutation when input ownership is external. |
| [5] | GH2 data access | Prefer `GetPear`/`GetPears`, `GetTwig`, and `GetTree` when metadata or null state matters; returned values are live references. |
| [6] | GH2 tolerances | Prefer `IDataAccess` tolerance, angle, and unit access inside components. |

---
## [5][INTEGRATION_TARGETS]
>**Dictum:** *MathNet lifts Rasm math where RhinoCommon has no native owner.*

<br>

| [INDEX] | [TARGET] | [MATHNET_USE] | [RHINO_BOUNDARY] |
| :-----: | -------- | ------------- | ---------------- |
| [1] | Best-fit frames | Covariance plus eigenvectors. | Validate returned `Plane`. |
| [2] | Principal axes | Mass properties or matrix eigensolve. | Prefer Rhino mass properties when available. |
| [3] | Curvature samples | Statistics and interpolation. | Rhino surfaces and curves own derivative source. |
| [4] | Registration transforms | SVD least-squares. | Return `Transform` or Rasm pose atom. |
| [5] | Mesh spectra | Sparse storage and matvec plus Rasm-owned eigen algorithms. | Rhino `Mesh` owns topology and geometry source. |
| [6] | Signed-distance fields | Symbolics formulas, root finding, interpolation, and gradients. | Emit Rhino contours, vectors, meshes, or preview values. |
| [7] | Spectral descriptors | FFT, EVD/SVD, histograms, covariance, and kernel density. | Emit typed descriptors and deterministic GH2 branches. |
| [8] | Vector-field streamlines | Fixed-step ODE solvers, interpolation, integration, and event root finding. | Emit validated curves and frame samples. |

---
## [6][GEOMETRY_FORMULAS]
>**Dictum:** *Geometry formulas must state affine intent before numerical execution.*

<br>

| [INDEX] | [KERNEL] | [FORMULA] | [OUTPUT_RULE] |
| :-----: | -------- | --------- | ------------- |
| [1] | Best-fit plane | `c = mean(p_i)`, `C = sum((p_i-c)(p_i-c)^T)`, normal = eigenvector for smallest eigenvalue. | Build with `Plane.CreateFromNormal`; reject `Plane.Unset`. |
| [2] | Principal frame | Eigenvectors of covariance or Rhino mass properties where available. | Enforce right-handed frame before emitting `Plane`. |
| [3] | Rigid registration | Kabsch or Umeyama SVD over centered point pairs. | Emit Rhino `Transform`; validate determinant, residual, and affine status. |
| [4] | Transform solve | Prefer direct solve or SVD pseudo-inverse over `Inverse`. | Reject non-finite entries and false inverse results. |
