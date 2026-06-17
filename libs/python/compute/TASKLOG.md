# [PY_COMPUTE_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`.

## [1]-[WHEEL_FLOOR_PROBES]

Probes gated on a distribution installing on the marker floor and `assay api query` filling the catalogue; each is named in its page RESEARCH cluster.

| [INDEX] | [ITEM]                                                                                          | [PAGE#CLUSTER]               | [STATUS] |
| :-----: | :--------------------------------------------------------------------------------------------- | :--------------------------- | :------: |
|   [1]   | The solver routes and accelerator rows verify against scipy/sympy/numba/jax on the marker floor | array-solver#SOLVER          | SPIKE    |
|   [2]   | The gated reverse-mode VJP (optimistix) verifies against the numpy-floor finite-difference branch | array-solver#DIFFERENTIATION | SPIKE    |
|   [3]   | Validated/interval numerics (python-flint Arb ball) verify against the nextafter floor          | array-solver#RIGOR           | SPIKE    |
|   [4]   | The DSP rows verify against scipy.signal on the marker floor                                    | array-solver#SIGNAL          | SPIKE    |
|   [5]   | The pint Measurement bridge threading correlated UFloat magnitudes verifies on the floor        | units-study#QUANTITY         | SPIKE    |
|   [6]   | The study-method members (LHS/Sobol/Morris/GP) verify against numpy/scipy/sklearn               | units-study#STUDY            | SPIKE    |
|   [7]   | The sklearn-to-ONNX export and runtime-check path verifies (onnx/onnxruntime/skl2onnx deploy-gated) | units-study#MODEL            | SPIKE    |
|   [8]   | The `Inference` member spellings re-reflect once `llvmlite`/`scipy` resolve a cp315 wheel and `arviz.rhat`/`arviz.ess` confirm the diagnostic projection | graduation#INFERENCE | SPIKE    |

## [2]-[SEAM_AND_CATALOGUE_GATES]

The graduation-seam proofs and manifest/`.api` gaps; the catalogue fills once each distribution installs and `assay api` reflects it.

| [INDEX] | [ITEM]                                                                                          | [PAGE#CLUSTER]        | [STATUS] |
| :-----: | :--------------------------------------------------------------------------------------------- | :-------------------- | :------: |
|   [1]   | The lambdify-to-managed codegen handoff and the geometry `HandoffAxis` case prove against the managed owner-row contract | graduation#GRADUATION | SPIKE    |
|   [2]   | `StubCodegen` proves the evidence-bundle decode against a real graduation-evidence sample at the seam | graduation#CODEGEN    | SPIKE    |
|   [3]   | numpy/scipy/pint/uncertainties resolve to `.api` rows once their cp315 wheels install            | array-solver#ARRAY    | BLOCKED  |
|   [4]   | numba/jax/onnx/onnxruntime/scikit-learn resolve to `.api` rows; carry the `python_version<'3.15'` marker | array-solver#SOLVER   | BLOCKED  |
|   [5]   | scikit-fem/python-flint/optimistix and pymc/arviz resolve to `.api` rows on the deploy-asset floor | array-solver#RIGOR    | BLOCKED  |

## [3]-[TRANSCRIPTION]

The implementation sequence is the `ARCHITECTURE.md` `[SOURCE_TREE]` build order (`arrays.py` through `inference.py`); each file transcribes its page clusters verbatim and resolves the RESEARCH rows those pages carry. Production source is absent.

| [INDEX] | [ITEM]                                                                          | [PAGE#CLUSTER]     | [STATUS] |
| :-----: | :----------------------------------------------------------------------------- | :----------------- | :------: |
|   [1]   | Transcribe the BUILD_ORDER files per `ARCHITECTURE.md` `[SOURCE_TREE]`         | array-solver#ARRAY | QUEUED   |
