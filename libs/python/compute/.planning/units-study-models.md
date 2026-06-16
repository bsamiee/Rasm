# [PY_COMPUTE_UNITS_STUDY_MODELS]

Units, uncertainty, studies, and model assets turn exploratory scientific work into evidence. They do not create production execution, scheduling, or inference ownership.

## [1]-[UNITS_OWNER]

[UNIT_CLAIM]:
- Owns: quantity family, unit expression, conversion path, magnitude type, and source evidence.
- API route: `.api/api-pint.md`.
- Output: unit claim and conversion receipt.
- Boundary: product unit wire law remains C# Compute-owned.

[UNCERTAINTY_CLAIM]:
- Owns: nominal value, standard deviation, correlation/covariance notes, and propagation path.
- API route: `.api/api-uncertainties.md`.
- Output: uncertainty envelope for study evidence.
- Boundary: uncertainty evidence never becomes a product receipt vocabulary by itself.

## [2]-[STUDY_OWNER]

[STUDY_PLAN]:
- Owns: parameter axes, sample grids, solver routes, input datasets, artifact outputs, and termination criteria.
- API routes: `.api/api-dask.md`, `.api/api-xarray.md`, `.api/api-numpy.md`, `.api/api-scipy.md`.
- Output: study receipt with input, route, output, and artifact references.
- Boundary: no production scheduling, no remote compute service, no farm queue.

[BENCHMARK_DATA]:
- Owns: repeatable local study measurements and comparison evidence for later C# review.
- Output: evidence bundle with environment facts, sampling policy, and artifact digest route.
- Boundary: C# Compute owns benchmark claim authority.

## [3]-[MODEL_OWNER]

[MODEL_ASSET]:
- Owns: file identity, checksum, model card, input/output names, preprocessing artifact references, and validation evidence.
- API routes: pending `.api/api-onnx.md`, `.api/api-onnxruntime.md`, `.api/api-scikit-learn.md`.
- Output: model asset manifest and graduation candidate.
- Boundary: no Python inference service, no product model session, no runtime tensor owner.

## [4]-[RED_TEAM]

- Reject unit conversions that do not produce a unit claim.
- Reject uncertainty data that disappears into floats.
- Reject study orchestration that becomes a durable job framework.
- Reject model sessions that run as product runtime.
