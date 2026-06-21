# [PY_COMPUTE_MODEL]

The one classical-ML model-asset validation, export, and graduation owner sitting adjacent to the graduation rail. `ModelAsset` carries the file identity, checksum, io-names, graph op-types, execution providers, model-card, and validation over onnx and onnxruntime, and exports a fitted scikit-learn estimator to ONNX through skl2onnx. `ValidationCheck` is the discriminant that DRIVES validation and OWNS its fold: `ValidationCheck.run` is the closed-family case method resolving each case to one `CheckVerdict` through a total `match`, so the union is the execution axis carrying its own behavior rather than dead surface beside a detached function. The `tag` literal `CheckKind` is the one check vocabulary the verdict carries — there is no parallel `StrEnum` re-encoding the same three kinds to drift against the discriminant. `ModelAssetManifest` carries the per-check verdict ledger and feeds the one graduation rail: `graduates` lowers the verdict ledger to the measured-residual map `graduation/handoff.md#GRADUATION` admits on the `model_asset` `HandoffAxis` case, so "graduates only after validation passes" is a typed rail from this owner into the one admission gate, never prose. The owner validates and exports a classical estimator graph; authoring or training a neural model is out of charter. onnx is cp315-clean; onnxruntime and skl2onnx gate on the `python_version<'3.15'` marker.

## [01]-[INDEX]

- [01]-[ASSET]: ONNX graph validation by the `ValidationCheck` fold, io-binding, smoke inference, the sklearn-to-ONNX export, and the model-asset graduation rail on one `ModelAsset` owner.

## [02]-[ASSET]

- Owner: `ModelAsset` — the file-identity, checksum, io-names, graph op-types, execution-providers, model-card, validation, and graduation owner over onnx, onnxruntime, and skl2onnx; `ModelAssetManifest` is the io-names, op-types, providers, model-card, and per-check verdict value object backing the graduation seam, every field carried by the struct. `ValidationCheck` discriminates `Structural` (`onnx.checker.check_model` graph well-formedness folded with `onnx.shape_inference.infer_shapes(strict_mode=True)` so an unbound or unpropagated shape is a structural reject, not a deferred surprise at the session), `IoBinding` (the graph-declared `graph.input` names match the `InferenceSession` `NodeArg` signature), and `Smoke` (a zero-tensor `run` returns finite output over every declared output, each input zero tensor allocated at the declared `NodeArg.type` element dtype through the `_ORT_DTYPE` table so an int64 or double input feed matches the session signature rather than a hardcoded float32 that the runtime would reject into the boundary). The union OWNS its fold: `ValidationCheck.run` is the closed-family case method, so `validate` builds the three checks and maps each through `check.run()` to a `CheckVerdict`, never a detached free function reaching back into the union.
- Check fold: `ValidationCheck.run` is the one total `match` on the owner resolving each case to a `CheckVerdict(check, passed, detail)` keyed by the `tag` literal directly — the `structural` case folds the `checker.check_model` plus strict `shape_inference.infer_shapes` outcome over both `checker.ValidationError` and `shape_inference.InferenceError`, so a malformed graph and an unpropagated strict-inference shape both fold into one failed structural verdict on the domain rail rather than a malformed-graph reject and a shape failure that escapes into the `boundary` as an infrastructure `BoundaryFault`; the `io_binding` case folds the graph-declared-versus-session io-name set equality, and the `smoke` case folds `np.isfinite` over every output array, closed by `assert_never` so a new check kind is a compile-surfaced gap. `validate` maps the check tuple to the verdict ledger and the `validated` aggregate `all(v.passed for v in verdicts)`; the union is read in exactly this one place and the manifest carries the per-check verdict tuple, so the receipt and the graduation residual ledger both project off the `residuals` view over the same verdicts rather than re-deriving them.
- Entry: `ModelAsset.validate` loads the ONNX graph through `onnx.load`, opens an `onnxruntime.InferenceSession`, reads the embedded `get_modelmeta()` `ModelMetadata` and the assigned `get_providers()`, builds the `Structural`/`IoBinding`/`Smoke` checks from the loaded graph and the session signature, maps them through `ValidationCheck.run`, derives the checksum through `ContentIdentity.of`, and returns `RuntimeRail[ModelAssetManifest]` proving the asset loads and infers before the C# model lane accepts it; the model-card folds the runtime `ModelMetadata` producer/domain/version/graph-name and `custom_metadata_map` over the proto `ir_version` and `metadata_props` so the card is the session's authoritative metadata, not a proto-only guess. `ModelAsset.export` matches an `ExportSource`, runs the skl2onnx export at `get_latest_tested_opset_version()`, and re-validates the written graph so an export returns a manifest whose verdicts gate graduation.
- Graduation: `ModelAssetManifest.graduates(ceiling)` is the typed rail from this owner into the one admission gate — it lowers the per-check verdict ledger to the measured-residual map (`{check.value: 0.0 on pass, 1.0 on fail}`) and calls `GraduationReceipt.graduates("compute", HandoffAxis(model_asset=self.subject()), self.checksum, measured, ceiling)`, returning `RuntimeRail[GraduationReceipt]` on the `model_asset` `HandoffAxis` case. A failed check is a residual `1.0` above the default `0.0` ceiling, so a model asset whose structural, io-binding, or smoke verdict failed is an `Error(BoundaryFault)` admission rejection rather than a graduated handoff — the four sibling rejection clauses are the one residual-over-ceiling fold the handoff owner declares, never a second admission body here. The subject is the canonical `producer` model-card entry.
- Receipt: `ModelAssetManifest.contribute` emits one `Receipt.of("emitted", ...)` row carrying the io-names, opset, graph op-types, assigned execution providers, model-card, and the per-check verdict ledger; the manifest crosses outward through `graduates` into `graduation/handoff.md#GRADUATION` on the `model_asset` axis only after every verdict passes, never a parallel emission path.
- Packages: `onnx` (`load`, `checker.check_model`, `checker.ValidationError`, `shape_inference.infer_shapes`, `shape_inference.InferenceError`, `ModelProto`, `opset_import`, `graph.node`, `graph.input`, `metadata_props`, `ir_version`), `onnxruntime` (`InferenceSession`, `get_inputs`, `get_outputs`, `get_providers`, `get_modelmeta`, `run`, `NodeArg`, `ModelMetadata`), `skl2onnx` (`convert_sklearn`, `get_latest_tested_opset_version`, `common.data_types.FloatTensorType`), `scikit-learn` (`base.BaseEstimator` as the typed export source), `numpy` (`zeros` for the smoke tensor at the declared element dtype, `isfinite`), `graduation/handoff.md#GRADUATION` (`GraduationReceipt`, `HandoffAxis`), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `ResourceRef`, `Receipt`/`ReceiptContributor`).
- Growth: a new validation check is one `ValidationCheck` case plus one `ValidationCheck.run` match arm; a new export source is one `ExportSource` row; a stricter graduation bar is one tighter ceiling row the caller supplies; zero new surface.
- Boundary: no production inference-session runtime; production tensor-session authority stays in C#, and an unvalidated graduation, a validation union read by nothing, and a manifest emitting outward on a path other than `graduates` are the deleted forms. Validating and exporting a classical scikit-learn estimator graph is in-scope; authoring or training a neural or generative model is not. onnx ships a cp315 wheel; onnxruntime and skl2onnx carry no cp315 wheel, so the session and export bodies are authored against the documented API and the manifest fold runs once the runtime wheel resolves.

```python signature
from __future__ import annotations

from pathlib import Path
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.compute.graduation.handoff import GraduationReceipt, HandoffAxis
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef

if TYPE_CHECKING:
    from onnx import ModelProto
    from sklearn.base import BaseEstimator


type CheckKind = Literal["structural", "io_binding", "smoke"]


_ORT_DTYPE: dict[str, type[np.generic]] = {
    "tensor(float)": np.float32,
    "tensor(double)": np.float64,
    "tensor(float16)": np.float16,
    "tensor(int64)": np.int64,
    "tensor(int32)": np.int32,
    "tensor(int8)": np.int8,
    "tensor(uint8)": np.uint8,
    "tensor(bool)": np.bool_,
}


@tagged_union(frozen=True)
class ValidationCheck:
    tag: CheckKind = tag()
    structural: ModelProto = case()
    io_binding: tuple[tuple[str, ...], tuple[str, ...]] = case()
    smoke: tuple[np.ndarray, ...] = case()

    @staticmethod
    def Structural(model: ModelProto) -> ValidationCheck:
        return ValidationCheck(structural=model)

    @staticmethod
    def IoBinding(declared: tuple[str, ...], session: tuple[str, ...]) -> ValidationCheck:
        return ValidationCheck(io_binding=(declared, session))

    @staticmethod
    def Smoke(outputs: tuple[np.ndarray, ...]) -> ValidationCheck:
        return ValidationCheck(smoke=outputs)

    def run(self) -> CheckVerdict:
        match self:
            case ValidationCheck(tag="structural", structural=model):
                import onnx

                try:
                    onnx.checker.check_model(model, full_check=True)
                    onnx.shape_inference.infer_shapes(model, check_type=True, strict_mode=True)
                except (onnx.checker.ValidationError, onnx.shape_inference.InferenceError) as err:
                    return CheckVerdict("structural", False, str(err))
                return CheckVerdict("structural", True, "well-formed shapes-inferred")
            case ValidationCheck(tag="io_binding", io_binding=(declared, session)):
                ok = set(declared) == set(session)
                return CheckVerdict("io_binding", ok, f"declared={sorted(declared)} session={sorted(session)}")
            case ValidationCheck(tag="smoke", smoke=outputs):
                ok = all(bool(np.isfinite(r).all()) for r in outputs)
                return CheckVerdict("smoke", ok, f"outputs={len(outputs)} finite={ok}")
            case unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class ExportSource:
    tag: Literal["sklearn"] = tag()
    sklearn: tuple[BaseEstimator, tuple[int, ...]] = case()

    @staticmethod
    def Sklearn(estimator: BaseEstimator, feature_shape: tuple[int, ...]) -> ExportSource:
        return ExportSource(sklearn=(estimator, feature_shape))


class CheckVerdict(Struct, frozen=True):
    check: CheckKind
    passed: bool
    detail: str


class ModelAssetManifest(Struct, frozen=True):
    checksum: ContentKey
    input_names: tuple[str, ...]
    output_names: tuple[str, ...]
    opset: int
    op_types: tuple[str, ...]
    providers: tuple[str, ...]
    model_card: dict[str, str]
    verdicts: tuple[CheckVerdict, ...]

    @property
    def validated(self) -> bool:
        return all(v.passed for v in self.verdicts)

    @property
    def residuals(self) -> dict[str, float]:
        return {v.check: 0.0 if v.passed else 1.0 for v in self.verdicts}

    def subject(self) -> str:
        return self.model_card.get("producer", "<anonymous>")

    def graduates(self, ceiling: dict[str, float] | None = None) -> RuntimeRail[GraduationReceipt]:
        residuals = self.residuals
        return GraduationReceipt.graduates(
            "compute",
            HandoffAxis(model_asset=self.subject()),
            self.checksum,
            residuals,
            ceiling or dict.fromkeys(residuals, 0.0),
        )

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted",
            "compute.model",
            self.subject(),
            {
                "inputs": ",".join(self.input_names),
                "outputs": ",".join(self.output_names),
                "opset": repr(self.opset),
                "op_types": ",".join(self.op_types),
                "providers": ",".join(self.providers),
                "validated": repr(self.validated),
                **{f"check[{v.check}]": v.detail for v in self.verdicts},
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
        session = onnxruntime.InferenceSession(str(path))
        meta = session.get_modelmeta()
        inputs = tuple(i.name for i in session.get_inputs())
        outputs = tuple(o.name for o in session.get_outputs())
        feed = {
            arg.name: np.zeros(
                tuple(d if isinstance(d, int) else 1 for d in arg.shape),
                dtype=_ORT_DTYPE.get(arg.type, np.float32),
            )
            for arg in session.get_inputs()
        }
        result = tuple(session.run(list(outputs), feed))
        verdicts = tuple(
            check.run()
            for check in (
                ValidationCheck.Structural(model),
                ValidationCheck.IoBinding(tuple(n.name for n in model.graph.input), inputs),
                ValidationCheck.Smoke(result),
            )
        )
        return ModelAssetManifest(
            checksum=ContentIdentity.of("onnx", path.read_bytes(), IdentityPolicy()),
            input_names=inputs,
            output_names=outputs,
            opset=int(max(o.version for o in model.opset_import)),
            op_types=tuple(n.op_type for n in model.graph.node),
            providers=tuple(session.get_providers()),
            model_card={
                "producer": meta.producer_name or "<anonymous>",
                "domain": meta.domain,
                "version": str(meta.version),
                "graph_name": meta.graph_name,
                "ir_version": str(model.ir_version),
                **{p.key: p.value for p in model.metadata_props},
                **{k: str(v) for k, v in meta.custom_metadata_map.items()},
            },
            verdicts=verdicts,
        )

    def _export_sklearn(self, estimator: BaseEstimator, feature_shape: tuple[int, ...]) -> ModelAssetManifest:
        from skl2onnx import convert_sklearn, get_latest_tested_opset_version
        from skl2onnx.common.data_types import FloatTensorType

        onnx_model = convert_sklearn(
            estimator,
            initial_types=[("input", FloatTensorType([None, *feature_shape]))],
            target_opset=get_latest_tested_opset_version(),
        )
        self.ref.path.write_bytes(onnx_model.SerializeToString())
        return self._load_and_run(self.ref.path)
```

## [03]-[RESEARCH]

- [ONNX_SESSION]: `onnx` ships a cp315 wheel (manifest pin `onnx>=1.22.0`, no marker — cp312-abi3 runs on CPython 3.15); the `onnx.load`/`checker.check_model(full_check=True)`/`checker.ValidationError`/`shape_inference.infer_shapes(strict_mode=True)`/`shape_inference.InferenceError`/`ModelProto.opset_import`/`graph.node`/`graph.input`/`metadata_props`/`ir_version` spellings verify against `compute/.api/onnx.md` (ENTRYPOINTS [01]/[05]/[06], PUBLIC_TYPES [01]/[02]/[12]/[13], IMPLEMENTATION_LAW VALIDATION_TOPOLOGY). `onnxruntime.InferenceSession`/`get_inputs`/`get_outputs`/`get_providers`/`get_modelmeta`/`run`/`NodeArg`/`ModelMetadata` carry the `python_version<'3.15'` marker (cp313-only, no cp315 wheel) and verify against `compute/.api/onnxruntime.md` (ENTRYPOINTS [01]/[02]/[05]/[07]/[08], PUBLIC_TYPES [04]/[07]); the `ModelMetadata` `producer_name`/`domain`/`version`/`graph_name`/`custom_metadata_map` fields the model-card folds are the captured-metadata receipt the IMPLEMENTATION_LAW STUDY_ROUTING declares. `skl2onnx.convert_sklearn`/`get_latest_tested_opset_version`/`common.data_types.FloatTensorType` carry the same gated band (CAPTURE [01]/[04]), and `sklearn.base.BaseEstimator` (the typed export source, `compute/.api/scikit-learn.md` PUBLIC_TYPES [01]) shares it under the `python_version<'3.15'` marker as the converter's classical-ML floor; the `BaseEstimator` annotation is `TYPE_CHECKING`-only under `from __future__ import annotations`, so it imposes no runtime cp315 obligation beyond the already-gated export body, and the session and export bodies verify against the catalogues once the runtime and converter wheels resolve.
- [GRADUATION_SEAM]: `ModelAssetManifest.graduates` feeds the `residuals` view over the per-check verdict ledger to `GraduationReceipt.graduates` on the `model_asset` `HandoffAxis` case from `graduation/handoff.md#GRADUATION`; a failed verdict is a residual `1.0` above the default `0.0` ceiling and rejects on the rail, so the model-asset admission is the one residual-over-ceiling fold the handoff owner declares rather than a second gate. `ContentKey` rides as `evidence_key`, rendered through `ContentKey.hex` by the handoff receipt's `contribute`.
- [IO_BINDING_CHECK]: the `IoBinding` case compares the proto `model.graph.input` names against the resolved `InferenceSession.get_inputs()` `NodeArg` names, so an initializer leaking into the declared input set, a renamed input, or a session that resolves a different signature than the graph declares is a structural binding failure — the set equality is load-bearing across two distinct sources, not a tautology over one. The `graph.input`/`get_inputs` pairing verifies against `compute/.api/onnx.md` PUBLIC_TYPES [02] and `compute/.api/onnxruntime.md` ENTRYPOINTS [05].
