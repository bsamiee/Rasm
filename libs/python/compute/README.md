# [PY_COMPUTE]

`compute` owns offline scientific evidence that graduates into managed owner rows: array admission, one polymorphic numeric-intent solver/symbolic dispatch with accelerator rows, units and uncertainty claims, study and experiment-run orchestration, model-asset validation, and the graduation receipt with a geometry handoff case. It has zero consumers today and implementation is full-capability. Owner state and the axis registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`. The design pages in `.planning/` are decision-complete blueprints an implementation agent transcribes; the package catalogues in `.api/` carry the external-surface evidence each page consumes.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                  | [OWNS]                                                             |
| :-----: | :-------------------------------------- | :---------------------------------------------------------------- |
|   [1]   | [array-solver](.planning/array-solver.md) | array admission, the numeric-intent solver, symbolic, accelerators |
|   [2]   | [units-study](.planning/units-study.md)   | units/uncertainty claims, study + run-history, model assets        |
|   [3]   | [graduation](.planning/graduation.md)     | the graduation receipt, the inference owner, the typed-stub codegen |

## [2]-[ADMISSIONS_RECORD]

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in the root manifest; this table never carries a pin. `[STATUS]` is one of `admitted`, `catalogue-pending`, `deploy-asset-gated`. Distributions carrying a `python_version<'3.15'` marker or a scientific transitive that fails the cp315 source build verify out-of-band on the marker floor.

| [INDEX] | [PACKAGE]                            | [PAGE]       | [CATALOGUE]                                              | [STATUS]           |
| :-----: | :----------------------------------- | :----------- | :------------------------------------------------------ | :----------------- |
|   [1]   | numpy                                | array-solver | api-numpy.md                                            | catalogue-pending  |
|   [2]   | scipy, sympy                         | array-solver | api-scipy.md, api-sympy.md                              | catalogue-pending  |
|   [3]   | numba, jax                           | array-solver | api-numba.md, api-jax.md                                | catalogue-pending  |
|   [4]   | scikit-fem, python-flint, optimistix | array-solver | api-scikit-fem.md, api-python-flint.md, api-optimistix.md | deploy-asset-gated |
|   [5]   | pint, uncertainties                  | units-study  | api-pint.md, api-uncertainties.md                       | catalogue-pending  |
|   [6]   | onnx, onnxruntime, scikit-learn      | units-study  | api-onnx.md, api-onnxruntime.md, api-scikit-learn.md    | catalogue-pending  |
|   [7]   | pymc, arviz                          | graduation   | api-pymc.md, api-arviz.md                               | deploy-asset-gated |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                | [RAIL]      | [EVIDENCE]                                          |
| :-----: | :-------------------- | :---------- | :------------------------------------------------- |
|  [G1]   | locked restore        | uv          | compute pins resolve against the root manifest      |
|  [G2]   | API catalogue resolve | assay api   | every fence member resolves to an `.api` row        |
|  [G3]   | type check            | ty          | typed-signature transcription resolves clean        |
|  [G4]   | lint and format       | ruff        | routed closure, zero diagnostics                    |
|  [G5]   | spec law-matrix       | pytest      | compute law-matrix specs pass                       |
|  [G6]   | wheel floor           | uv          | cp315/marker-floor wheels install before re-reflect |
|  [G7]   | page diagram render   | mermaid-cli | page diagrams render through the local renderer      |
