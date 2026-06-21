# [PY_COMPUTE_MODEL]

The classical-ML model-asset export, validation, and graduation owner. `ModelAsset` exports a fitted scikit-learn estimator graph to ONNX through `skl2onnx.to_onnx`, structurally checks it through `onnx`, runs it through an `onnxruntime.InferenceSession`, and folds every check into a typed evidence ledger that graduates on the `model_asset` `HandoffAxis` case. Input and output are both parameterized: `ExportSource` discriminates the three `to_onnx` source shapes, and `ValidationCheck.run` folds each case to a `ValidationEvidence` carrier so a verdict holds only the slots its kind names rather than one fat default-zero struct. Authoring or training a neural model is out of charter. `onnx` is cp315-clean; `onnxruntime`, `skl2onnx`, and `scikit-learn` gate on `python_version<'3.15'`.

## [01]-[INDEX]

- [01]-[ASSET]: the sklearn-to-ONNX export over `ExportSource`, validation by the `ValidationCheck` fold over `ValidationEvidence`, the `onnxruntime`-session smoke-and-parity check, the woven span/`@receipted`/`beartype` rail, and the graduation rail on one `ModelAsset` owner.

## [02]-[ASSET]

- Owner: `ModelAsset` drives export, validation, and graduation over `onnx`, `onnxruntime`, `skl2onnx`, and `scikit-learn`. `ModelAssetManifest` is the io-names, op-types, providers, model-card, and per-check verdict value object backing the graduation seam. Source, check, and result are all discriminated unions, so output is parameterized as tightly as input.
- Source union: `ExportSource` is the one `@tagged_union` over the `skl2onnx` export routes. `ExportSource.convert(target_opset, gating)` reads the `(model, sample)` pair off the `fitted` projection and runs one `to_onnx(model, X=sample, target_opset=..., options={"zipmap": False}, white_op=gating.white or None, black_op=gating.black or None)` across the `estimator`, `pipeline`, and `columns` cases, so a new source is one case and one arm. The sample drives `initial_types` inference rather than a hand-built `FloatTensorType`; a categorical or mixed-dtype source is the `columns` case `to_onnx` types through `guess_data_type`. `options={"zipmap": False}` forces a classifier's probability output to a dense `np.ndarray` rather than the `ZipMap` list-of-dicts the parity diff cannot consume. `gating` is the `OperatorGate` row whose `white_op`/`black_op` sets bound the emitted operators, so a quantized or opset-restricted graph is a tighter row, never a converter fork.
- Check union: `ValidationCheck` discriminates `structural` (`onnx.checker.check_model(full_check=True)` folded with `onnx.shape_inference.infer_shapes(strict_mode=True)`, so an unpropagated shape is a structural reject), `io_binding` (proto `model.graph.input` names equal the resolved `InferenceSession.get_inputs()` `NodeArg` names, so an initializer leaking into the declared set, a renamed input, or a divergent session signature is a binding failure — set equality across two distinct sources, never a tautology over one), `smoke` (a zero-tensor `run` is finite over every output, each input zeroed at the declared element dtype through `onnx.helper.tensor_dtype_to_np_dtype`), and `parity` (the session output over the real `sample` matches the estimator within tolerance). `PROBE_RANK` pairs the parity verb with the ONNX output index the `ExportSource.reference` `Block.choose`/`try_head` fold reads: `predict_proba`/`decision_function` ride output `1` and `predict` rides output `0`, because a `zipmap`-off classifier emits `label` at `0` and dense scores at `1`, so the check diffs `produced[probe.index]` rather than a hardcoded `produced[0]` comparing the int64 label against a float-probability reference.
- Output union: `ValidationEvidence` parameterizes the verdict — `Structural(passed, detail)`, `Binding(declared, resolved)`, `Smoke(output_count, finite)`, `Parity(max_abs_delta, tolerance)` — and owns its `passed` predicate and `facts()` projection, so `CheckVerdict.of(check, evidence)` folds each shape to one typed receipt row rather than one default-zero struct, as `analysis/signal.md#SIGNAL` collapses `Spectral`/`Multiresolution`/`Scale`/`Packet`. The verdict ledger and the graduation residual map both project off the same evidence.
- Check fold: `ValidationCheck.run` is the one total `match` to a `CheckVerdict`. The `structural` arm folds the `checker.check_model` plus strict `shape_inference.infer_shapes` outcome over both `checker.ValidationError` and `shape_inference.InferenceError`, so a malformed graph and an unpropagated shape both land as one failed `Structural` verdict on the domain rail rather than an infrastructure `BoundaryFault`. The remaining arms fold the io-name set equality, `np.isfinite` over every output, and the max-abs-delta against the estimator, closed by `assert_never`. `validate` is the one place the union is read.
- Entry: `ModelAsset.validate` opens the `content.model.validate` span, loads through `onnx.load`, opens an `InferenceSession` under `SessionOptions(graph_optimization_level=GraphOptimizationLevel.ORT_ENABLE_ALL)` over the provider preference, reads `get_modelmeta()` and the assigned `get_providers()`, runs the zero-tensor smoke feed for `Structural`/`Binding`/`Smoke`, runs the real-`sample` feed for `Parity` when a source is in hand, maps every check through `ValidationCheck.run`, derives the checksum through `ContentIdentity.of`, and returns `RuntimeRail[ModelAssetManifest]`. Both feeds route through one element-dtype-keyed construction, so int64/double declared types match the session signature. The model-card folds runtime `ModelMetadata` producer/domain/version/graph-name and `custom_metadata_map` over the proto `ir_version` and `metadata_props`. `ModelAsset.export` runs `ExportSource.convert` at `get_latest_tested_opset_version()`, writes the graph, and re-validates with the source in hand so the manifest's `parity` verdict gates graduation.
- Woven rail: validate and export run inside one OTel span weaving `beartype`, `onnx`/`onnxruntime`/`skl2onnx`, the `boundary` fault fence, and the `@receipted` egress aspect, the `_traced`-shaped discipline `graduation/handoff.md#GRADUATION` and `analysis/signal.md#SIGNAL` hold. `boundary(f"model.{op}", thunk)` runs the `@beartype(conf=FAULT_CONF)`-fenced inner thunk inside the span, so a converter raise, a checker `ValidationError`, or a session-construction failure folds onto the rail. The `Ok` arm sets `Status(StatusCode.OK)` and writes the bounded `ModelAssetManifest.span_facts` scalars behind the `is_recording()` gate the sibling `experiments/inference.md#BAYESIAN` and `graduation/handoff.md#GRADUATION` owners hold, so a no-op span pays no attribute build and the per-check `ValidationEvidence` ledger rides the receipt facts rather than the span. `@receipted(_REDACTION)` harvests the contributor stream, so the bodies thread no inline `Signals.emit`; the fence's `_convert` already records the cause and sets ERROR on the span.
- Graduation: `ModelAssetManifest.graduates(ceiling)` lowers the verdict ledger to the `residuals` map (`0.0` on pass, `1.0` on fail) and calls `GraduationReceipt.graduates("compute", HandoffAxis(model_asset=self.subject()), self.checksum, residuals, ceiling)`. A failed check is a residual `1.0` above the default `0.0` ceiling, so any failed verdict is an `Error(BoundaryFault)` rejection on the one residual-over-ceiling fold the handoff owner declares, never a second admission body here. The subject is the `producer` model-card entry.
- Receipt: `ModelAssetManifest.contribute` yields one `Receipt.of("compute.model", ("emitted", subject, facts))` row (the `tuple[Receipt, ...]` the `ReceiptContributor` port streams) carrying io-names, opset, op-types, assigned providers, model-card, and the per-check ledger spread through each `ValidationEvidence.facts()`. The manifest crosses outward only through `graduates`, never a parallel emission path or a four-positional `Receipt.of` the factory does not admit.
- Packages: `onnx` (`load`, `checker.check_model`, `checker.ValidationError`, `shape_inference.infer_shapes`, `shape_inference.InferenceError`, `helper.tensor_dtype_to_np_dtype`, `defs.onnx_opset_version`, `ModelProto`, `opset_import`, `graph.node`, `graph.input`, `metadata_props`, `ir_version`), `onnxruntime` (`InferenceSession`, `SessionOptions`, `GraphOptimizationLevel`, `get_inputs`, `get_outputs`, `get_providers`, `get_available_providers`, `get_modelmeta`, `run`, `NodeArg`, `ModelMetadata`), `skl2onnx` (`to_onnx` with `options={"zipmap": False}`, `get_latest_tested_opset_version`, `common.data_types.guess_data_type`, `sklapi.CastTransformer`), `scikit-learn` (`base.BaseEstimator`, `pipeline.Pipeline`, `compose.ColumnTransformer`, the `predict`/`predict_proba`/`decision_function` parity surface paired through `PROBE_RANK`), `numpy` (`zeros` at the declared element dtype, `astype` casting the parity feed, `isfinite`, `abs`, `asarray`), `expression` (`tagged_union`/`case`/`tag`, `Block.of_seq`/`choose`/`try_head` the `PROBE_RANK` Option fold, `Some`/`Nothing`/`Option.default_value` the verb-rank catch-all), `msgspec` (`Struct`, `gc=False` on the container-free `OperatorGate`/`CheckVerdict`/`ModelAsset` leaves, the tracked `ProbeRef` `ndarray` and the `ModelAssetManifest` `model_card` `dict` staying GC-tracked), `beartype` (`@beartype(conf=FAULT_CONF)`, `beartype.vale.Is` the `ParityArray`/`Residual` finiteness refinement), stdlib `math.isfinite` (the `Residual` predicate), `opentelemetry-api` (`trace.get_tracer`/`start_as_current_span`/`Status`/`StatusCode`), `graduation/handoff.md#GRADUATION` (`GraduationReceipt`, `HandoffAxis`), runtime (`RuntimeRail`, `boundary`, `FAULT_CONF`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`/`Redaction`/`receipted`, `ResourceRef`).
- Growth: a new validation check is one `ValidationCheck` case, one `ValidationEvidence` case, and one `run` arm; a new export source is one `ExportSource` case and one `convert` arm; a new parity probe verb is one `ProbeAttr` literal plus one `PROBE_RANK` row; a stricter operator gate is one `OperatorGate` row; a stricter graduation bar is one tighter ceiling row the caller supplies. Zero new surface, no `convert_<kind>` family, no second emit method.
- Boundary: no production inference-session runtime; production tensor-session authority stays in C#. Validating and exporting a classical scikit-learn estimator graph is in-scope; authoring or training a neural or generative model is not. `onnx` ships a cp315 wheel while `onnxruntime`/`skl2onnx`/`scikit-learn` carry none, so the session, export, and parity bodies are authored against the documented API and the manifest fold runs once the runtime wheel resolves. Deleted forms: an unvalidated graduation; a validation union read by nothing; a manifest emitting outward on a path other than `graduates`; a hand-rolled dtype table where `helper.tensor_dtype_to_np_dtype` owns the element-type map; a hand-built `FloatTensorType` `initial_types` where `to_onnx(model, X)` infers it; a fat default-zero verdict struct where `ValidationEvidence` parameterizes the output; a four-positional `Receipt.of("emitted", owner, subject, facts)` against the two-argument `of(owner, evidence)` contract; a `ContentKey` bound off the bare rail-typed `ContentIdentity.of` return or masked behind an empty-key `default_value` where the in-fence `match` re-raises onto the `boundary`; a single-caller `_elem_type`/`_key` free function where the dtype lookup and checksum `match` are owner-local body code; an inline `Signals.emit` where `@receipted` owns egress; a hardcoded `produced[0]` parity index where `PROBE_RANK` pairs the verb to its ONNX output; a parity diff against the zero smoke feed where the parity run feeds the real `sample`; a default `ZipMap` classifier output where `options={"zipmap": False}` keeps it dense; a `next(getattr(...) for ... if hasattr(...))` verb selection raising `StopIteration` where the `Block.choose`/`try_head`/`default_value` fold is total; a bare `np.ndarray` parity arg where `ParityArray` refines a `NaN`/`±inf` array onto the rail at the `@beartype`-fenced `Parity` factory; a `gc=False` `ModelAssetManifest` opting a `model_card`-`dict` struct out of GC tracking against the container-free-leaf opt-out (or a tracked `OperatorGate`/`CheckVerdict`/`ModelAsset` leaf left on the cyclic-GC set); an un-gated `span.set_attributes` on the `Ok` arm where the sibling owners gate the write behind `is_recording()` so a no-op span builds no attribute map, or a raw `ValidationEvidence` ledger dumped onto the span where only the bounded `span_facts` `str | int | bool` scalars are admissible; and `from __future__ import annotations` against the 3.15 runtime.

```python signature
from collections.abc import Callable
from math import isfinite
from pathlib import Path
from typing import TYPE_CHECKING, Annotated, Final, Literal, assert_never

import numpy as np
from beartype import beartype
from beartype.vale import Is
from expression import Error, Nothing, Ok, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

from rasm.compute.graduation.handoff import GraduationReceipt, HandoffAxis
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, Redaction, receipted
from rasm.runtime.roots import ResourceRef

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
# the parity-feed numeric refinement the `@beartype(conf=FAULT_CONF)` fence checks on the diff arrays,
# the same `Is`-finiteness discipline `graduation/handoff.md#GRADUATION` holds on its `Ledger`/`Ceiling`:
# a `NaN`/`±inf` produced or reference array raises `BeartypeCallHintViolation` inside the `boundary`
# fence and the `CLASSIFY` `api` row folds it onto the rail rather than a silent `NaN <= tol` reject
# the receipt cannot distinguish from a real numeric divergence.
type ParityArray = Annotated[np.ndarray, Is[lambda a: bool(np.isfinite(a).all())]]
type Residual = Annotated[dict[str, float], Is[lambda m: all(isfinite(v) for v in m.values())]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_TRACER: Final = trace.get_tracer("compute.model")

_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # model facts carry no secret field

_PARITY_TOL: Final[float] = 1e-4  # converter float32-vs-float64 numeric drift tolerance

# the ordered parity probe rank: each row pairs a sklearn mixin verb with the ONNX output index
# `skl2onnx` emits it at — `predict_proba`/`decision_function` ride output 1 (a classifier emits
# `label` at 0 and the dense `probabilities`/scores at 1 once `zipmap` is off), `predict` rides
# output 0 (a regressor's sole output, the catch-all every estimator exposes). The fold keeps the
# first row whose verb the estimator implements, so the parity diff reads the matching column.
PROBE_RANK: Final[Block[tuple[ProbeAttr, int]]] = Block.of_seq(
    [("predict_proba", 1), ("decision_function", 1), ("predict", 0)]
)

# --- [MODELS] ---------------------------------------------------------------------------


# `white_op`/`black_op` bound which ONNX operators the converter may emit; an empty set leaves
# the operator vocabulary unrestricted. A quantized or opset-restricted graph is a tighter row,
# never a converter fork. `gc=False`: a container-free `frozenset`-pair leaf, off the tracked set.
class OperatorGate(Struct, frozen=True, gc=False):
    white: frozenset[str] = frozenset()
    black: frozenset[str] = frozenset()


# the parity probe the `ExportSource.reference` fold yields: the estimator's richest probabilistic
# output array paired with the ONNX output `index` `skl2onnx` emits it at, so the parity check diffs
# `produced[index]` against `reference` rather than assuming the session's first output is the score.
# No `gc=False`: the record holds a tracked `np.ndarray`, so the leaf-only opt-out does not apply.
class ProbeRef(Struct, frozen=True):
    index: int
    reference: np.ndarray


@tagged_union(frozen=True)
class ExportSource:
    tag: Literal["estimator", "pipeline", "columns"] = tag()
    estimator: tuple[BaseEstimator, np.ndarray] = case()
    pipeline: tuple[Pipeline, np.ndarray] = case()
    columns: tuple[ColumnTransformer, np.ndarray] = case()

    @staticmethod
    def Estimator(model: BaseEstimator, sample: np.ndarray) -> ExportSource:
        return ExportSource(estimator=(model, sample))

    @staticmethod
    def Pipeline(steps: Pipeline, sample: np.ndarray) -> ExportSource:
        return ExportSource(pipeline=(steps, sample))

    @staticmethod
    def Columns(transformer: ColumnTransformer, sample: np.ndarray) -> ExportSource:
        return ExportSource(columns=(transformer, sample))

    @property
    def fitted(self) -> tuple[Predictor, np.ndarray]:
        match self:
            case ExportSource(tag="estimator", estimator=pair):
                return pair
            case ExportSource(tag="pipeline", pipeline=pair):
                return pair
            case ExportSource(tag="columns", columns=pair):
                return pair
            case _ as unreachable:
                assert_never(unreachable)

    def convert(self, target_opset: int, gating: OperatorGate) -> "ModelProto":
        from skl2onnx import to_onnx

        model, sample = self.fitted
        # `to_onnx(model, X)` infers `initial_types` from the trained schema; `zipmap` off keeps a
        # classifier's probability output a dense `np.ndarray` the parity `np.abs`-diff can consume.
        return to_onnx(
            model,
            X=sample,
            target_opset=target_opset,
            options={"zipmap": False},
            white_op=gating.white or None,
            black_op=gating.black or None,
        )

    def reference(self) -> ProbeRef:
        # `PROBE_RANK` pairs the richest verb the estimator exposes with the output index it rides; the
        # `Block.choose`/`try_head`/`default_value` fold is total over the closed rank with `predict`
        # (output 0) the catch-all, never a `next(...)` that raises `StopIteration` on a dropped tail row.
        model, sample = self.fitted
        attr, index = (
            PROBE_RANK.choose(lambda row: Some(row) if hasattr(model, row[0]) else Nothing)
            .try_head()
            .default_value(("predict", 0))
        )
        return ProbeRef(index=index, reference=np.asarray(getattr(model, attr)(sample), dtype=float))


@tagged_union(frozen=True)
class ValidationEvidence:
    tag: CheckKind = tag()
    structural: tuple[bool, str] = case()
    io_binding: tuple[tuple[str, ...], tuple[str, ...]] = case()
    smoke: tuple[int, bool] = case()
    parity: tuple[float, float] = case()

    @staticmethod
    def Structural(passed: bool, detail: str) -> ValidationEvidence:
        return ValidationEvidence(structural=(passed, detail))

    @staticmethod
    def Binding(declared: tuple[str, ...], resolved: tuple[str, ...]) -> ValidationEvidence:
        return ValidationEvidence(io_binding=(declared, resolved))

    @staticmethod
    def Smoke(output_count: int, finite: bool) -> ValidationEvidence:
        return ValidationEvidence(smoke=(output_count, finite))

    @staticmethod
    def Parity(max_abs_delta: float, tolerance: float) -> ValidationEvidence:
        return ValidationEvidence(parity=(max_abs_delta, tolerance))

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
    tag: CheckKind = tag()
    structural: ModelProto = case()
    io_binding: tuple[tuple[str, ...], tuple[str, ...]] = case()
    smoke: tuple[np.ndarray, ...] = case()
    parity: tuple[np.ndarray, np.ndarray] = case()

    @staticmethod
    def Structural(model: ModelProto) -> ValidationCheck:
        return ValidationCheck(structural=model)

    @staticmethod
    def IoBinding(declared: tuple[str, ...], resolved: tuple[str, ...]) -> ValidationCheck:
        return ValidationCheck(io_binding=(declared, resolved))

    @staticmethod
    def Smoke(outputs: tuple[np.ndarray, ...]) -> ValidationCheck:
        return ValidationCheck(smoke=outputs)

    @staticmethod
    @beartype(conf=FAULT_CONF)
    def Parity(session_output: ParityArray, reference: ParityArray) -> ValidationCheck:
        # the one `@beartype`-fenced factory: the `ParityArray` `Is`-finiteness refinement raises inside
        # the `boundary` fence on a `NaN`/`±inf` session score or estimator reference, so a non-finite
        # diff folds onto the rail rather than a silent `NaN <= tol` reject the receipt cannot tell from
        # a real divergence; the sibling structural/binding/smoke factories carry no array refinement.
        return ValidationCheck(parity=(session_output, reference))

    def run(self) -> CheckVerdict:
        match self:
            case ValidationCheck(tag="structural", structural=model):
                import onnx

                try:
                    onnx.checker.check_model(model, full_check=True)
                    onnx.shape_inference.infer_shapes(model, check_type=True, strict_mode=True)
                except (onnx.checker.ValidationError, onnx.shape_inference.InferenceError) as err:
                    return CheckVerdict.of("structural", ValidationEvidence.Structural(False, str(err)))
                return CheckVerdict.of("structural", ValidationEvidence.Structural(True, "well-formed shapes-inferred"))
            case ValidationCheck(tag="io_binding", io_binding=(declared, resolved)):
                return CheckVerdict.of("io_binding", ValidationEvidence.Binding(declared, resolved))
            case ValidationCheck(tag="smoke", smoke=outputs):
                finite = all(bool(np.isfinite(r).all()) for r in outputs)
                return CheckVerdict.of("smoke", ValidationEvidence.Smoke(len(outputs), finite))
            case ValidationCheck(tag="parity", parity=(produced, reference)):
                # both arrays are `ParityArray`-refined at the `Parity` factory, so the diff is over
                # finite operands; `np.abs(... ).max()` is the max-abs-delta the receipt records, never a
                # bare `allclose` bool that would drop the actual divergence the model card carries.
                delta = float(np.abs(produced.ravel() - reference.ravel()).max()) if reference.size else 0.0
                return CheckVerdict.of("parity", ValidationEvidence.Parity(delta, _PARITY_TOL))
            case _ as unreachable:
                assert_never(unreachable)


# `gc=False`: the row carries a `CheckKind` literal and a frozen `ValidationEvidence` case, no mutable
# container, so it joins the container-free-leaf opt-out the sibling evidence rows take.
class CheckVerdict(Struct, frozen=True, gc=False):
    check: CheckKind
    evidence: ValidationEvidence

    @staticmethod
    def of(check: CheckKind, evidence: ValidationEvidence) -> CheckVerdict:
        return CheckVerdict(check, evidence)

    @property
    def passed(self) -> bool:
        return self.evidence.passed


# No `gc=False`: the manifest holds a tracked `model_card` `dict` and the verdict tuple, so it stays
# GC-tracked exactly as the sibling `dict`-carrying receipts do, never the leaf-only opt-out.
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
    def residuals(self) -> Residual:
        # the per-check verdict ledger lowered to the measured-residual map the one admission gate folds
        # against its ceiling — `0.0` on pass, `1.0` on fail, finite by construction so the handoff
        # `Ledger` `Is`-finiteness refinement admits it rather than railing at the `graduates` fence.
        return {v.check: 0.0 if v.passed else 1.0 for v in self.verdicts}

    @property
    def span_facts(self) -> dict[str, str | int | bool]:
        # the bounded `str | int | bool` scalars `Span.set_attributes` admits, the one source both the
        # span and any future reader share; the per-check `ValidationEvidence` ledger is a `dict` value
        # `set_attributes` rejects and rides the receipt facts only.
        return {
            "subject": self.subject(),
            "validated": self.validated,
            "opset": self.opset,
            "providers": ",".join(self.providers),
        }

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

    def contribute(self) -> tuple[Receipt, ...]:
        facts: dict[str, object] = {
            "inputs": ",".join(self.input_names),
            "outputs": ",".join(self.output_names),
            "opset": self.opset,
            "op_types": ",".join(self.op_types),
            "providers": ",".join(self.providers),
            "validated": self.validated,
            **{f"check[{v.check}]": v.evidence.facts() for v in self.verdicts},
            **self.model_card,
        }
        return (Receipt.of("compute.model", ("emitted", self.subject(), facts)),)


# `gc=False`: a container-free leaf over the frozen `ResourceRef` handle and a `str`-tuple, off the
# tracked set; the entry owner carries no per-instance mutable container.
class ModelAsset(Struct, frozen=True, gc=False):
    ref: ResourceRef
    providers: tuple[str, ...] = ()

    def validate(self) -> RuntimeRail[ModelAssetManifest]:
        return self._traced("validate", lambda: self._load_and_run(self.ref.path, None))

    def export(self, source: ExportSource, /, *, gating: OperatorGate = OperatorGate()) -> RuntimeRail[ModelAssetManifest]:
        return self._traced(f"export.{source.tag}", lambda: self._export(source, gating))

    def _traced(self, op: str, thunk: Callable[[], ModelAssetManifest]) -> RuntimeRail[ModelAssetManifest]:
        # one woven rail: the `boundary` fence wraps the `@beartype(conf=FAULT_CONF)`-guarded thunk
        # inside the one span, so a converter/checker/session raise folds onto the rail rather than
        # escaping; the `Ok` arm sets OK and `@receipted` harvests the contributor stream on exit.
        with _TRACER.start_as_current_span(f"content.model.{op}") as span:
            rail = boundary(f"model.{op}", lambda: self._emit(thunk()))
            match rail:
                case Ok(manifest):
                    if span.is_recording():
                        span.set_attributes(manifest.span_facts)
                    span.set_status(Status(StatusCode.OK))
                case Error(_):
                    pass
            return rail

    @staticmethod
    @receipted(_REDACTION)
    def _emit(manifest: ModelAssetManifest) -> ModelAssetManifest:
        return manifest

    @beartype(conf=FAULT_CONF)
    def _load_and_run(self, path: Path, source: ExportSource | None) -> ModelAssetManifest:
        import onnx

        model = onnx.load(str(path))
        session = self._session(path)
        meta = session.get_modelmeta()
        inputs = tuple(i.name for i in session.get_inputs())
        outputs = tuple(o.name for o in session.get_outputs())
        # `helper.tensor_dtype_to_np_dtype` over the declared element-type enum keys both feeds, so an
        # int64/double input matches the session signature; an unmatched name defaults to `FLOAT`.
        declared = {i.name: i.type.tensor_type.elem_type for i in model.graph.input}
        element = {a.name: onnx.helper.tensor_dtype_to_np_dtype(declared.get(a.name, onnx.TensorProto.FLOAT)) for a in session.get_inputs()}
        # symbolic dims zero to unit length; the parity feed below casts the real `sample` to the same
        # element dtype, so neither run is two parallel constructions nor a diff against the zero feed.
        smoke_feed = {a.name: np.zeros(tuple(d if isinstance(d, int) else 1 for d in a.shape), dtype=element[a.name]) for a in session.get_inputs()}
        produced = tuple(session.run(list(outputs), smoke_feed))
        checks: list[ValidationCheck] = [
            ValidationCheck.Structural(model),
            ValidationCheck.IoBinding(tuple(n.name for n in model.graph.input), inputs),
            ValidationCheck.Smoke(produced),
        ]
        if source is not None:
            # the parity run scores the real `sample` and reads `outputs[probe.index]` — the output the
            # chosen verb rides — so the diff is against the estimator's own output, never `produced[0]`.
            _, sample = source.fitted
            probe = source.reference()
            parity_feed = {a.name: sample.astype(element[a.name]) for a in session.get_inputs()}
            scored = np.asarray(session.run([outputs[probe.index]], parity_feed)[0], dtype=float)
            checks.append(ValidationCheck.Parity(scored, probe.reference))
        # `ContentIdentity.of` returns `RuntimeRail[ContentKey]`; the `boundary` fence already wraps
        # this body, so a hash `Error` re-raises onto the fence rather than masking the validated
        # checksum behind a fabricated zero key.
        match ContentIdentity.of("onnx", path.read_bytes(), IdentityPolicy()):
            case Ok(checksum):
                pass
            case Error(fault):
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
            verdicts=tuple(check.run() for check in checks),
        )

    def _session(self, path: Path) -> "InferenceSession":
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

`ExportSource` owns the export, `ValidationCheck` owns the fold, and `ModelAsset.validate`/`export` weave the `boundary` fence, the `@beartype(conf=FAULT_CONF)` body, and the `@receipted` egress aspect under one `content.model.{op}` span. Both feeds route through one element-dtype-keyed construction off `onnx.helper.tensor_dtype_to_np_dtype`, so the parity run scores the estimator's own input rather than the zero feed, and the checksum is `match`ed off `ContentIdentity.of`'s `RuntimeRail[ContentKey]` inside the already-fenced body so an `Error` re-raises onto the `boundary`. The charter boundary holds at the model seam: validating and exporting a classical scikit-learn estimator graph is in-scope; authoring or training a neural model is not.
