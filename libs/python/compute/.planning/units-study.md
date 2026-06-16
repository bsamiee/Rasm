# [PY_COMPUTE_UNITS_STUDY]

Quantity claims, study orchestration, and model-asset validation. `QuantityClaim` is one quantity-evidence owner pair (unit over pint, uncertainty over uncertainties); `StudyPlan` owns param-axis/sample-grid/solver-route/termination orchestration over dask/xarray/numpy/scipy, with `BenchmarkData` collapsed in as a measurement-mode discriminant and `RunHistory` owning experiment-run persistence/resume/comparison on the same study spine; `ModelAsset` owns file identity/checksum/io-names/validation over onnx + onnxruntime + scikit-learn (the sklearn-to-ONNX export + runtime-check path) backing the graduation seam.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                        |
| :-----: | :-------- | :------------------------------------------------------------ |
|   [1]   | QUANTITY  | unit + uncertainty quantity claims                            |
|   [2]   | STUDY     | study orchestration, benchmark mode, run history              |
|   [3]   | MODEL     | model-asset file identity, validation, sklearn-to-ONNX export |

## [2]-[QUANTITY]

- Owner: `QuantityClaim` — the quantity-evidence owner pair: `UnitClaim` over pint (quantity-family/unit-expr/conversion-path/magnitude-type) and `UncertaintyClaim` over uncertainties (nominal/stddev/correlation/propagation).
- Entry: `QuantityClaim.convert` runs the pint conversion returning a `RuntimeRail`; `QuantityClaim.propagate` folds the uncertainties propagation over a derived expression.
- Packages: `pint` (`UnitRegistry`/`Quantity`/`to`/`dimensionality`), `uncertainties` (`ufloat`/`correlated_values`/`std_dev`), runtime (`RuntimeRail`).
- Growth: a new quantity family is one `UnitClaim` row; a new propagation mode is one `UncertaintyClaim` branch; zero new surface.
- Boundary: no C# quantity/unit owner minting; evidence propagation only; a hand-rolled unit conversion table is the deleted form. `SPIKE` on the marker floor.

```python signature
from msgspec import Struct

from rasm.runtime.rails_resilience import RuntimeRail, boundary


class QuantityClaim(Struct, frozen=True):
    family: str
    unit_expr: str
    conversion_path: tuple[str, ...]
    nominal: float
    std_dev: float

    def convert(self, target_unit: str) -> "RuntimeRail[QuantityClaim]":
        return boundary("quantity.convert", lambda: self._to(target_unit))
```

## [3]-[STUDY]

- Owner: `StudyPlan` — the param-axis/sample-grid/solver-route/input-dataset/artifact-output/termination orchestration over dask/xarray/numpy/scipy; `RunHistory` the experiment-run persistence/resume/cross-run comparison on the same study spine; `BenchmarkData` collapses into the study receipt with a `MeasurementMode` discriminant.
- Cases: `MeasurementMode` rows `RESULT` · `WALLCLOCK` · `SPEEDUP` — one discriminant on the study receipt, not a parallel benchmark owner.
- Entry: `StudyPlan.run` walks the sample grid over a dask/anyio lane returning a `RuntimeRail[StudyReceipt]`; `RunHistory.resume` re-runs only the incomplete grid cells; `RunHistory.compare` joins two run receipts.
- Packages: `dask` (delayed/compute over the grid), `xarray` (sample-grid coordinates), `numpy`, `scipy`, runtime (`LanePolicy`/`RuntimeRail`/`ReceiptContributor`).
- Growth: a new param axis is one coordinate on the sample grid; a new measurement mode is one `MeasurementMode` row; zero new surface.
- Boundary: no job framework, farm scheduler, or C# `ComputeReceipt` minting; a standalone benchmark owner and a parallel experiment tracker are the deleted forms. `SPIKE` on the marker floor.

```python signature
from enum import StrEnum

from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.observability import Receipt


class MeasurementMode(StrEnum):
    RESULT = "result"
    WALLCLOCK = "wallclock"
    SPEEDUP = "speedup"


class StudyReceipt(Struct, frozen=True):
    route: str
    mode: MeasurementMode
    cells_completed: int
    cells_total: int
    content_key: ContentKey

    def contribute(self) -> Receipt:
        return Receipt.Emitted(self.route, self.mode, {"cells": str(self.cells_completed)})


class StudyPlan(Struct, frozen=True):
    param_axes: tuple[NamedAxis, ...]
    route: NumericIntent
    mode: MeasurementMode
```

## [4]-[MODEL]

- Owner: `ModelAsset` — file identity/checksum/model-card/io-names/preprocessing/validation over onnx + onnxruntime + scikit-learn; `ModelAssetManifest` the io-names/preprocessing/validation value object directly backing the graduation seam.
- Entry: `ModelAsset.validate` loads the ONNX graph, runs an `onnxruntime.InferenceSession` smoke run over the declared io-names, and returns a `RuntimeRail[ModelAssetManifest]` proving the asset loads and infers before the C# Rasm.Compute row accepts it; `ModelAsset.export` runs the sklearn-to-ONNX export validated before graduation.
- Packages: `onnx` (`load`/`checker.check_model`/`ModelProto`), `onnxruntime` (`InferenceSession`/`run`), `scikit-learn` (`Pipeline`/estimators + skl2onnx export), runtime (`RuntimeRail`/`ContentIdentity`).
- Growth: a new validation check is one branch in `validate`; a new export source is one `export` arm; zero new surface.
- Boundary: no production inference session runtime; production tensor-session authority stays in C#; an unvalidated graduation is the deleted form. `SPIKE` on the marker floor.

```python signature
from msgspec import Struct

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.rails_resilience import RuntimeRail, boundary
from rasm.runtime.resources_lanes import ResourceRef


class ModelAssetManifest(Struct, frozen=True):
    checksum: ContentKey
    input_names: tuple[str, ...]
    output_names: tuple[str, ...]
    opset: int
    validated: bool


class ModelAsset(Struct, frozen=True):
    ref: ResourceRef

    def validate(self) -> "RuntimeRail[ModelAssetManifest]":
        return boundary("model.validate", self._load_and_run)
```

## [5]-[RESEARCH]

- [ONNXRUNTIME_SESSION]: the `onnxruntime.InferenceSession.run` io-name binding, the `onnx.checker.check_model` validation, the sklearn-to-ONNX export entry, and the pint/uncertainties propagation spellings are verified against `.api/api-onnxruntime.md`, `.api/api-onnx.md`, `.api/api-scikit-learn.md`, `.api/api-pint.md`, `.api/api-uncertainties.md` once the marker-floor environment installs them (suite TASKLOG `PY_API_003`).
