# [PY_COMPUTE_ARRAY_SOLVER]

Array and solver planning admits scientific payloads into explicit numeric intent. The package uses NumPy, SciPy, SymPy, Xarray, and Dask for their full domain surfaces instead of hand-rolled numeric kernels.

## [1]-[ARRAY_OWNER]

[ARRAY_PAYLOAD]:
- Owns: dtype, shape, named axes, finite policy, memory layout facts, chunking facts, and payload identity.
- API routes: `.api/api-numpy.md`, `.api/api-xarray.md`, `.api/api-dask.md`.
- Output: array admission record and rejection receipt.
- Boundary: no product tensor allocation, model session, or C# substrate row minting.

[NAMED_AXIS]:
- Owns: study dimension names, coordinate labels, and free-dimension evidence.
- API route: `.api/api-xarray.md`.
- Output: axis record consumed by study plans.
- Boundary: axes are study evidence, not wire vocabulary.

## [2]-[SOLVER_OWNER]

[NUMERIC_INTENT]:
- Owns: subject, route, input variables, output variables, tolerances, and admissible backend package.
- Cases: dense linear algebra, sparse solve, nonlinear optimize, integrate, interpolate, symbolic derivation.
- API routes: `.api/api-scipy.md`, `.api/api-sympy.md`, `.api/api-numpy.md`.
- Output: solver plan and study receipt.
- Boundary: product substrate selection and benchmark claims stay in `Rasm.Compute`.

[SYMBOLIC_DERIVATION]:
- Owns: symbolic inputs, expression, simplification path, lambdify/codegen evidence, and C# handoff notes.
- API route: `.api/api-sympy.md`.
- Output: derivation receipt plus graduation candidate.
- Boundary: Python symbolic code is evidence; product algebra belongs to C# owners after graduation.

## [3]-[ACCELERATOR_PENDING]

[JAX_NUMBA]:
- Owns: pending accelerator and compiled-kernel evidence only.
- API routes: `.api/api-jax.md`, `.api/api-numba.md`.
- Output: admission notes and future kernel graduation candidates.
- Boundary: no source import until root manifest admission and package-local evidence agree.

## [4]-[RED_TEAM]

- Reject numeric code that bypasses array admission.
- Reject custom solvers when SciPy or SymPy owns the route.
- Reject Python kernels that become product runtime dependencies.
- Reject benchmark claims without C# Compute ownership.
