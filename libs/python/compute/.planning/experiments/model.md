# [PY_COMPUTE_MODEL]

Classical-ML model-asset export, validation, and graduation owner: `ModelAsset` exports a fitted scikit-learn estimator graph to ONNX through `skl2onnx.to_onnx`, structurally checks it through `onnx`, runs it through an `onnxruntime.InferenceSession`, and folds every check into a typed evidence ledger that graduates on the `model_asset` `HandoffAxis` case. Authoring or training a neural model is out of charter.

Input and output are both parameterized: `ExportSource` discriminates the `to_onnx` source shapes and `ValidationCheck.run` folds each case to a `ValidationEvidence` carrier holding only the slots its kind names. `onnx` is core; `onnxruntime`, `skl2onnx`, and `scikit-learn` gate on the worker lane. This run rides the `EvidenceScope.MODEL` weave — span, `boundary` fence, beartype guard, fenced harvest of the manifest contributor.

## [01]-[INDEX]

- [01]-[ASSET]: the sklearn-to-ONNX export over `ExportSource`, the `ValidationCheck` fold to `ValidationEvidence` verdicts, and the graduation rail on one `ModelAsset` owner.

## [02]-[ASSET]

- Owner: `ModelAsset` — `ModelAssetManifest` is the io-names, op-types, providers, model-card, and per-check verdict value object backing the graduation seam; a failed check is a residual `1.0` above the default `0.0` ceiling on the shared `graduation/handoff.md#GRADUATION` fold, never a second admission body here, and the manifest crosses outward only through `graduates`.
- Cases: `ExportSource` — the sample drives `initial_types` inference, so a categorical or mixed-dtype source is the `columns` case, never a hand-built `FloatTensorType`; `OperatorGate` bounds the emitted operators, so a quantized or opset-restricted graph is a tighter row, never a converter fork.
- Output: the `ValidationEvidence` case IS the verdict row — its `tag` names the check and `passed` reads the outcome, no separate `CheckVerdict` carrier re-stamping the discriminant; a malformed graph and an unpropagated shape both land as one failed `structural` verdict on the domain rail, never an infrastructure `BoundaryFault`.
- Growth: a new validation check is one `ValidationCheck` case, one `ValidationEvidence` case, and one `run` arm; a new export source is one `ExportSource` case and one `convert` arm; a new parity probe verb is one `ProbeAttr` literal and one `PROBE_RANK` row; a stricter operator gate is one `OperatorGate` row; a stricter graduation bar is a tighter ceiling row the caller supplies.

```python signature
from collections.abc import Callable, Iterable
from math import isfinite
from typing import TYPE_CHECKING, Annotated, Final, Literal, assert_never

import numpy as np
from beartype import beartype
from beartype.vale import Is
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from upath import UPath

from rasm.compute.graduation.handoff import EvidenceScope, GraduationReceipt, HandoffAxis, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt
from rasm.runtime.roots import ResourceRef
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:
    from onnx import ModelProto
    from onnxruntime import InferenceSession
    from sklearn.base import BaseEstimator
    from sklearn.compose import ColumnTransformer
    from sklearn.pipeline import Pipeline

# --- [TYPES] ----------------------------------------------------------------------------

type CheckKind = Literal["structural", "io_binding", "smoke", "parity"]
type Predictor = BaseEstimator | Pipeline | ColumnTransformer
type ProbeAttr = Literal["predict_proba", "decision_function", "predict"]
# a `NaN`/`±inf` produced or reference array raises inside the `boundary` fence and folds onto the rail, rather than a silent
# `NaN <= tol` reject the receipt cannot distinguish from a real numeric divergence.
type ParityArray = Annotated[np.ndarray, Is[lambda a: bool(np.isfinite(a).all())]]
type Residual = Annotated[dict[str, float], Is[lambda m: all(isfinite(v) for v in m.values())]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_PARITY_TOL: Final[float] = 1e-4  # converter float32-vs-float64 numeric drift tolerance

# each row pairs a sklearn verb with the ONNX output index it rides — a `zipmap`-off classifier emits `label` at 0 and dense
# scores at 1, `predict` rides 0 — so the parity diff reads the matching column, never an int64 label against a float reference.
PROBE_RANK: Final[Block[tuple[ProbeAttr, int]]] = Block.of_seq([("predict_proba", 1), ("decision_function", 1), ("predict", 0)])

# --- [MODELS] ---------------------------------------------------------------------------


# an empty set leaves the operator vocabulary unrestricted; `gc=False` — a container-free `frozenset`-pair leaf.
class OperatorGate(Struct, frozen=True, gc=False):
    white: frozenset[str] = frozenset()
    black: frozenset[str] = frozenset()


# estimator's richest probabilistic output paired with the ONNX output index it rides; holds a tracked `ndarray`, so no `gc=False`.
class ProbeRef(Struct, frozen=True):
    index: int
    reference: np.ndarray


@tagged_union(frozen=True)
class ExportSource:
    tag: Literal["estimator", "pipeline", "columns"] = tag()
    estimator: tuple[BaseEstimator, np.ndarray] = case()
    pipeline: tuple[Pipeline, np.ndarray] = case()
    columns: tuple[ColumnTransformer, np.ndarray] = case()

    @property
    def fitted(self) -> tuple[Predictor, np.ndarray]:
        # one or-pattern binds the `(model, sample)` pair off whichever case the tag selects; the three
        # cases share the projection, so they collapse to one `pair` capture closed by `assert_never`.
        match self:
            case (
                ExportSource(tag="estimator", estimator=pair)
                | ExportSource(tag="pipeline", pipeline=pair)
                | ExportSource(tag="columns", columns=pair)
            ):
                return pair
            case _ as unreachable:
                assert_never(unreachable)

    def convert(self, target_opset: int, gating: OperatorGate) -> "ModelProto":
        from skl2onnx import to_onnx

        model, sample = self.fitted
        # `to_onnx(model, X)` infers `initial_types` from the trained schema; `zipmap` off keeps a
        # classifier's probability output a dense `np.ndarray` the parity `np.abs`-diff can consume.
        return to_onnx(
            model, X=sample, target_opset=target_opset, options={"zipmap": False}, white_op=gating.white or None, black_op=gating.black or None
        )

    def reference(self) -> ProbeRef:
        # `PROBE_RANK` pairs the richest verb the estimator exposes with the output index it rides; the
        # `Block.choose`/`try_head`/`default_value` fold is total over the closed rank with `predict`
        # (output 0) the catch-all, never a `next(...)` that raises `StopIteration` on a dropped tail row.
        model, sample = self.fitted
        attr, index = PROBE_RANK.choose(lambda row: Some(row) if hasattr(model, row[0]) else Nothing).try_head().default_value(("predict", 0))
        return ProbeRef(index=index, reference=np.asarray(getattr(model, attr)(sample), dtype=float))


@tagged_union(frozen=True)
class ValidationEvidence:
    # keyword-constructed off the case name; `passed` and `facts` are the two total projections the ledger and receipt read.
    tag: CheckKind = tag()
    structural: tuple[bool, str] = case()
    io_binding: tuple[tuple[str, ...], tuple[str, ...]] = case()
    smoke: tuple[int, bool] = case()
    parity: tuple[float, float] = case()

    @property
    def passed(self) -> bool:
        match self:
            case ValidationEvidence(tag="structural", structural=(ok, _)):
                return ok
            case ValidationEvidence(tag="io_binding", io_binding=(declared, resolved)):
                return set(declared) == set(resolved)
            case ValidationEvidence(tag="smoke", smoke=(_, finite)):
                return finite
            case ValidationEvidence(tag="parity", parity=(delta, tol)):
                return delta <= tol
            case _ as unreachable:
                assert_never(unreachable)

    def facts(self) -> dict[str, object]:
        match self:
            case ValidationEvidence(tag="structural", structural=(_, detail)):
                return {"detail": detail}
            case ValidationEvidence(tag="io_binding", io_binding=(declared, resolved)):
                return {"declared": sorted(declared), "resolved": sorted(resolved)}
            case ValidationEvidence(tag="smoke", smoke=(count, _)):
                return {"outputs": count}
            case ValidationEvidence(tag="parity", parity=(delta, tol)):
                return {"max_abs_delta": delta, "tolerance": tol}
            case _ as unreachable:
                assert_never(unreachable)


@tagged_union(frozen=True)
class ValidationCheck:
    # check INPUT union, distinct from the `ValidationEvidence` verdict output — two parameterized shapes, never one fat
    # carrier; only the refinement-bearing `parity` case keeps a fenced factory.
    tag: CheckKind = tag()
    structural: ModelProto = case()
    io_binding: tuple[tuple[str, ...], tuple[str, ...]] = case()
    smoke: tuple[np.ndarray, ...] = case()
    parity: tuple[np.ndarray, np.ndarray] = case()

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def Parity(session_output: ParityArray, reference: ParityArray) -> ValidationCheck:
        # sole factory the union keeps — the `ParityArray` finiteness refinement must fire inside the `boundary` fence.
        return ValidationCheck(parity=(session_output, reference))

    def run(self) -> ValidationEvidence:
        match self:
            case ValidationCheck(tag="structural", structural=model):
                import onnx

                try:
                    onnx.checker.check_model(model, full_check=True)
                    onnx.shape_inference.infer_shapes(model, check_type=True, strict_mode=True)
                except (onnx.checker.ValidationError, onnx.shape_inference.InferenceError) as err:
                    return ValidationEvidence(structural=(False, str(err)))
                return ValidationEvidence(structural=(True, "well-formed shapes-inferred"))
            case ValidationCheck(tag="io_binding", io_binding=(declared, resolved)):
                return ValidationEvidence(io_binding=(declared, resolved))
            case ValidationCheck(tag="smoke", smoke=outputs):
                # `np.isfinite` raises on a `tensor(string)`/object output — the label column a `zipmap`-off classifier emits —
                # so the finite test gates on a numeric dtype and counts a categorical label finite.
                finite = all(bool(np.isfinite(r).all()) if np.issubdtype(r.dtype, np.number) else True for r in outputs)
                return ValidationEvidence(smoke=(len(outputs), finite))
            case ValidationCheck(tag="parity", parity=(produced, reference)):
                # max-abs-delta the receipt records, never a bare `allclose` bool that drops the actual divergence.
                delta = float(np.abs(produced.ravel() - reference.ravel()).max()) if reference.size else 0.0
                return ValidationEvidence(parity=(delta, _PARITY_TOL))
            case _ as unreachable:
                assert_never(unreachable)


# holds a tracked `model_card` dict and the verdict tuple, so no `gc=False`.
class ModelAssetManifest(Struct, frozen=True):
    checksum: ContentKey
    input_names: tuple[str, ...]
    output_names: tuple[str, ...]
    opset: int
    op_types: tuple[str, ...]
    providers: tuple[str, ...]
    model_card: dict[str, str]
    verdicts: tuple[ValidationEvidence, ...]

    @property
    def validated(self) -> bool:
        return all(v.passed for v in self.verdicts)

    @property
    def residuals(self) -> Residual:
        # verdict ledger lowered to the measured-residual map — `0.0` on pass, `1.0` on fail, keyed by each verdict's own `tag`.
        return {v.tag: 0.0 if v.passed else 1.0 for v in self.verdicts}

    @property
    def span_facts(self) -> dict[str, str | int | bool]:
        # bounded scalars only — the per-check ledger rides the receipt facts, never the span.
        return {"subject": self.subject(), "validated": self.validated, "opset": self.opset, "providers": ",".join(self.providers)}

    def subject(self) -> str:
        return self.model_card.get("producer", "<anonymous>")

    def graduates(self, ceiling: dict[str, float] | None = None) -> RuntimeRail[GraduationReceipt]:
        residuals = self.residuals
        return GraduationReceipt.graduates(
            EvidenceScope.MODEL.value, HandoffAxis(model_asset=self.subject()), self.checksum, residuals, ceiling or dict.fromkeys(residuals, 0.0)
        )

    def contribute(self) -> Iterable[Receipt]:
        facts: dict[str, object] = {
            "inputs": ",".join(self.input_names),
            "outputs": ",".join(self.output_names),
            "opset": self.opset,
            "op_types": ",".join(self.op_types),
            "providers": ",".join(self.providers),
            "validated": self.validated,
            **{f"check[{v.tag}]": v.facts() for v in self.verdicts},
            **self.model_card,
        }
        return (Receipt.of(EvidenceScope.MODEL.value, ("emitted", self.subject(), facts)),)


def _validate_kernel(asset: "ModelAsset") -> "RuntimeRail[ModelAssetManifest]":
    # module-level so the worker resolves both kernels by import; the fence converts a converter/checker/session raise.
    return boundary("model.validate", lambda: asset._load_and_run(asset.ref.path, None))


def _export_kernel(asset: "ModelAsset", source: "ExportSource", gating: "OperatorGate") -> "RuntimeRail[ModelAssetManifest]":
    return boundary(f"model.export.{source.tag}", lambda: asset._export(source, gating))


class ModelAsset(Struct, frozen=True):  # holds a `ResourceRef` and a providers tuple — container fields keep it GC-tracked
    ref: ResourceRef
    providers: tuple[str, ...] = ()

    async def validate(self, lane: LanePolicy) -> RuntimeRail[ModelAssetManifest]:
        return await self._traced(lane, "validate", _validate_kernel, self)

    async def export(self, lane: LanePolicy, source: ExportSource, /, *, gating: OperatorGate = OperatorGate()) -> RuntimeRail[ModelAssetManifest]:
        return await self._traced(lane, f"export.{source.tag}", _export_kernel, self, source, gating)

    async def _traced(
        self, lane: LanePolicy, op: str, kernel: Callable[..., "RuntimeRail[ModelAssetManifest]"], *args: object
    ) -> RuntimeRail[ModelAssetManifest]:
        # weave owns span, fence, and the fenced contributor harvest.
        async def dispatch() -> RuntimeRail[ModelAssetManifest]:
            return (await lane.offload(Kernel.of(kernel, KernelTrait.RELEASING), *args)).bind(lambda rail: rail)

        return await evidence_run(EvidenceScope.MODEL, f"model.{op}", dispatch, facts={"op": op, "providers": ",".join(self.providers)})

    @beartype(conf=FAULT_CONF)
    def _load_and_run(self, path: UPath, source: ExportSource | None) -> ModelAssetManifest:
        import onnx

        model = onnx.load(str(path))
        session = self._session(path)
        meta = session.get_modelmeta()
        args = session.get_inputs()  # one signature read; both feeds and the binding set key off it
        inputs = tuple(a.name for a in args)
        outputs = tuple(o.name for o in session.get_outputs())
        # `helper.tensor_dtype_to_np_dtype` over the declared element-type enum keys both feeds, so an
        # int64/double input matches the session signature; an unmatched name defaults to `FLOAT`.
        declared = {i.name: i.type.tensor_type.elem_type for i in model.graph.input}
        element = {a.name: onnx.helper.tensor_dtype_to_np_dtype(declared.get(a.name, onnx.TensorProto.FLOAT)) for a in args}
        # symbolic dims zero to unit length; the parity feed below casts the real `sample` to the same
        # element dtype, so neither run is two parallel constructions nor a diff against the zero feed.
        smoke_feed = {a.name: np.zeros(tuple(d if isinstance(d, int) else 1 for d in a.shape), dtype=element[a.name]) for a in args}
        produced = tuple(session.run(list(outputs), smoke_feed))
        # parity check is `Some` only when a source feeds the real `sample`, so the verdict tuple is one `Block` fold over an
        # `Option`-tailed sequence, never a mutable `list` + conditional `append`.
        parity = Option.of_optional(source).map(
            lambda src: ValidationCheck.Parity(
                np.asarray(
                    session.run([outputs[(probe := src.reference()).index]], {a.name: src.fitted[1].astype(element[a.name]) for a in args})[0],
                    dtype=float,
                ),
                probe.reference,
            )
        )
        checks = Block.of_seq([
            ValidationCheck(structural=model),
            ValidationCheck(io_binding=(tuple(n.name for n in model.graph.input), inputs)),
            ValidationCheck(smoke=produced),
            *parity.to_list(),
        ])
        # key is `match`ed off the rail inside the already-fenced body, so a hash `Error` re-raises onto the fence rather than
        # masking the checksum behind a fabricated zero key.
        match ContentIdentity.of("onnx", path.read_bytes()):
            case Result(tag="ok", ok=checksum):
                pass
            case Result(tag="error", error=fault):
                raise RuntimeError(fault)
        return ModelAssetManifest(
            checksum=checksum,
            input_names=inputs,
            output_names=outputs,
            opset=int(max(o.version for o in model.opset_import)),
            op_types=tuple(n.op_type for n in model.graph.node),
            providers=tuple(session.get_providers()),
            model_card={
                "producer": meta.producer_name or "<anonymous>",
                "domain": meta.domain or "",
                "version": str(meta.version),
                "graph_name": meta.graph_name or "",
                "ir_version": str(model.ir_version),
                **{p.key: p.value for p in model.metadata_props},
                **{k: str(v) for k, v in meta.custom_metadata_map.items()},
            },
            verdicts=tuple(checks.map(ValidationCheck.run)),
        )

    def _session(self, path: UPath) -> "InferenceSession":
        import onnxruntime

        options = onnxruntime.SessionOptions()
        options.graph_optimization_level = onnxruntime.GraphOptimizationLevel.ORT_ENABLE_ALL
        available = set(onnxruntime.get_available_providers())
        preference = [p for p in self.providers if p in available] or None
        return onnxruntime.InferenceSession(str(path), sess_options=options, providers=preference)

    @beartype(conf=FAULT_CONF)
    def _export(self, source: ExportSource, gating: OperatorGate) -> ModelAssetManifest:
        from skl2onnx import get_latest_tested_opset_version

        graph = source.convert(get_latest_tested_opset_version(), gating)
        self.ref.path.write_bytes(graph.SerializeToString())
        return self._load_and_run(self.ref.path, source)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
