# [PY_COMPUTE_ASSET]

The one classical-ML model-asset validation and export owner. `ModelAsset` carries the file identity, checksum, io-names, preprocessing, model-card, and validation over onnx and onnxruntime, and exports a fitted scikit-learn estimator to ONNX through skl2onnx. `ValidationCheck` folds graph well-formedness, io-name binding, and a smoke inference over the declared io-names; `ModelAssetManifest` is the validation value object that backs the graduation seam. The owner validates and exports a classical estimator graph; authoring or training a neural model is out of charter. onnx, onnxruntime, and skl2onnx gate on the `python_version<'3.15'` marker.

## [1]-[INDEX]

[ASSET]: ONNX graph validation, io-binding, smoke inference, and the sklearn-to-ONNX export on one `ModelAsset` owner.

## [2]-[ASSET]

- Owner: `ModelAsset` — the file-identity, checksum, io-names, preprocessing, model-card, and validation owner over onnx, onnxruntime, and skl2onnx; `ModelAssetManifest` is the io-names, preprocessing, model-card, and validation value object backing the graduation seam, every field carried by the struct. `ValidationCheck` discriminates `Structural` (`onnx.checker.check_model` graph well-formedness), `IoBinding` (declared io-names match the `InferenceSession` graph), and `Smoke` (a zero-tensor inference returns finite output), folded total in `validate`. `ExportSource` discriminates the export entry, currently `Sklearn` (a fitted estimator with its input feature shape).
- Entry: `ModelAsset.validate` loads the ONNX graph, folds the `ValidationCheck` cases over the declared io-names through an `onnxruntime.InferenceSession`, derives the checksum through `ContentIdentity.of`, and returns `RuntimeRail[ModelAssetManifest]` proving the asset loads and infers before the C# model lane accepts it; `ModelAsset.export` matches an `ExportSource` and runs the skl2onnx export validated before graduation.
- Receipt: `ModelAssetManifest.contribute` emits one `Receipt.of("emitted", ...)` row carrying the io-names, opset, preprocessing, model-card, and validation verdict; the manifest graduates through `graduation/receipt.md#GRADUATION` on the model-asset axis only after validation passes.
- Packages: `onnx` (`load`, `checker.check_model`, `ModelProto`, `opset_import`), `onnxruntime` (`InferenceSession`, `get_inputs`, `get_outputs`, `run`), `skl2onnx` (`convert_sklearn`, `to_onnx`, `FloatTensorType`), `numpy` (`zeros` for the smoke tensor), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `ResourceRef`, `Receipt`/`ReceiptContributor`).
- Growth: a new validation check is one `ValidationCheck` row; a new export source is one `ExportSource` row; zero new surface.
- Boundary: no production inference-session runtime; production tensor-session authority stays in C#, and an unvalidated graduation is the deleted form. Validating and exporting a classical scikit-learn estimator graph is in-scope; authoring or training a neural or generative model is not. onnx, onnxruntime, and skl2onnx carry no cp315 wheel, so the validate and export bodies are authored against the documented API.

```python signature
from __future__ import annotations

from pathlib import Path
from typing import Any, Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef


@tagged_union(frozen=True)
class ValidationCheck:
    tag: Literal["structural", "io_binding", "smoke"] = tag()
    structural: None = case()
    io_binding: tuple[str, ...] = case()
    smoke: tuple[str, ...] = case()

    @staticmethod
    def Structural() -> ValidationCheck:
        return ValidationCheck(structural=None)

    @staticmethod
    def IoBinding(inputs: tuple[str, ...]) -> ValidationCheck:
        return ValidationCheck(io_binding=inputs)

    @staticmethod
    def Smoke(outputs: tuple[str, ...]) -> ValidationCheck:
        return ValidationCheck(smoke=outputs)


@tagged_union(frozen=True)
class ExportSource:
    tag: Literal["sklearn"] = tag()
    sklearn: tuple[Any, tuple[int, ...]] = case()

    @staticmethod
    def Sklearn(estimator: Any, feature_shape: tuple[int, ...]) -> ExportSource:
        return ExportSource(sklearn=(estimator, feature_shape))


class ModelAssetManifest(Struct, frozen=True):
    checksum: ContentKey
    input_names: tuple[str, ...]
    output_names: tuple[str, ...]
    opset: int
    preprocessing: tuple[str, ...]
    model_card: dict[str, str]
    validated: bool

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted",
            "compute.model",
            str(self.checksum.value),
            {
                "inputs": ",".join(self.input_names),
                "outputs": ",".join(self.output_names),
                "opset": repr(self.opset),
                "preprocessing": ",".join(self.preprocessing),
                "validated": repr(self.validated),
                **self.model_card,
            },
        )


class ModelAsset(Struct, frozen=True):
    ref: ResourceRef

    def validate(self) -> RuntimeRail[ModelAssetManifest]:
        return boundary("model.validate", lambda: self._load_and_run(self.ref.path))

    def export(self, source: ExportSource, /) -> RuntimeRail[ModelAssetManifest]:
        match source:
            case ExportSource(tag="sklearn", sklearn=(estimator, feature_shape)):
                return boundary("model.export.sklearn", lambda: self._export_sklearn(estimator, feature_shape))
            case unreachable:
                assert_never(unreachable)

    def _load_and_run(self, path: Path) -> ModelAssetManifest:
        import onnx
        import onnxruntime

        model = onnx.load(str(path))
        onnx.checker.check_model(model)
        session = onnxruntime.InferenceSession(str(path))
        inputs = tuple(i.name for i in session.get_inputs())
        outputs = tuple(o.name for o in session.get_outputs())
        feed = {i.name: np.zeros([d if isinstance(d, int) else 1 for d in i.shape], dtype=np.float32) for i in session.get_inputs()}
        result = session.run(list(outputs), feed)
        opset = max(o.version for o in model.opset_import)
        return ModelAssetManifest(
            checksum=ContentIdentity.of("onnx", path.read_bytes(), IdentityPolicy()),
            input_names=inputs,
            output_names=outputs,
            opset=int(opset),
            preprocessing=tuple(n.op_type for n in model.graph.node),
            model_card={"producer": model.producer_name, "ir_version": str(model.ir_version)},
            validated=all(bool(np.isfinite(np.asarray(r)).all()) for r in result),
        )

    def _export_sklearn(self, estimator: Any, feature_shape: tuple[int, ...]) -> ModelAssetManifest:
        from skl2onnx import convert_sklearn
        from skl2onnx.common.data_types import FloatTensorType

        onnx_model = convert_sklearn(estimator, initial_types=[("input", FloatTensorType([None, *feature_shape]))])
        self.ref.path.write_bytes(onnx_model.SerializeToString())
        return self._load_and_run(self.ref.path)
```

## [3]-[RESEARCH]

- [ONNX_SESSION]: the `onnx.load`/`checker.check_model`/`ModelProto.opset_import`/`graph.node`, `onnxruntime.InferenceSession`/`get_inputs`/`get_outputs`/`run`, and `skl2onnx.convert_sklearn`/`common.data_types.FloatTensorType` spellings carry the `python_version<'3.15'` marker (no cp315 wheel); the validate and export bodies verify against the `.api` catalogue once the onnx, onnxruntime, and skl2onnx wheels resolve.
