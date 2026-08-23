# [PY_COMPUTE_MODEL]

Classical-ML model-asset export, validation, and graduation owner: `ModelAsset` exports a fitted scikit-learn estimator graph to ONNX through `skl2onnx.to_onnx`, structurally checks it through `onnx`, runs it through an `onnxruntime.InferenceSession`, and folds every check into a typed evidence ledger that graduates on the `model_asset` `HandoffAxis` case. Authoring or training a neural model is out of charter.

Input and output are both parameterized: `ExportSource` discriminates the `to_onnx` source shapes and `ValidationCheck.run` folds each case to a `ValidationEvidence` carrier holding only the slots its kind names. `onnx`, `onnxruntime`, and `skl2onnx` ride module-scope `lazy` binds, so the export stack loads on first dereference inside the worker; the `scikit-learn` names stay annotation-only under `TYPE_CHECKING`; `h5py` imports module-top for the envelope container. This run rides the `EvidenceScope.MODEL` weave — span, narrowed `boundary` fence naming the ONNX stack's own raise set, beartype guard, fenced harvest of the manifest contributor onto the one runtime receipt spine. `[03]-[ENVELOPE]` seats the drift-envelope companion here because only this owner holds the training columns the bands fit from; its container layout is the C# ingest fence's law, hand-copied.

## [01]-[INDEX]

- [02]-[ASSET]: the sklearn-to-ONNX export over `ExportSource`, the `ValidationCheck` fold to `ValidationEvidence` verdicts, and the graduation rail on one `ModelAsset` owner.
- [03]-[ENVELOPE]: the drift-envelope companion — reference bands fitted from the training population and written as the HDF5 container the C# admission gate ingests, its `write_async` twin the crossing's one movement record.

## [02]-[ASSET]

- Owner: `ModelAsset` — `ModelAssetManifest` is the io-names, op-types, providers, model-card, and per-check verdict value object backing the graduation seam; a failed check is a residual `1.0` above its governed `_CHECK_CEILING` row on the shared `graduation/handoff#GRADUATION` fold, never a second admission body here and never a bar derived from the run's own residual roster, and the manifest crosses outward only through `graduates` under the caller's composition key.
- Cases: `ExportSource` — the sample drives `initial_types` inference, so a categorical or mixed-dtype source is the `columns` case, never a hand-built `FloatTensorType`; `OperatorGate` bounds the emitted operators, so a quantized or opset-restricted graph is a tighter row, never a converter fork.
- Output: the `ValidationEvidence` case IS the verdict row — its `tag` names the check and `passed` reads the outcome, no separate `CheckVerdict` carrier re-stamping the discriminant; a malformed graph and an unpropagated shape both land as one failed `structural` verdict on the domain rail, never an infrastructure `BoundaryFault`.
- Receipt: the manifest settles on the ONE runtime spine, and its failed-check roster IS the spine's warning band — the retired `validated: bool` answered THAT a check failed and erased WHICH, so a parity failure and a structural failure read identically to every consumer downstream of the flag. The producer name rides a `Posture`: a graph whose metadata named none is ABSENT, and the subject falls back to the checksum's own wire render rather than a `<anonymous>` literal every unnamed asset would collide onto.
- Growth: a new validation check is one `ValidationCheck` case, one `ValidationEvidence` case, one `run` arm, and one `_CHECK_CEILING` row; a new refusal is one `FaultRow` anchor in `RAISES`; a new export source is one `ExportSource` case and one `convert` arm; a new parity probe verb is one `ProbeAttr` literal and one `PROBE_RANK` row; a stricter operator gate is one `OperatorGate` row; a stricter graduation bar is a tighter `_CHECK_CEILING` row or the caller's override.

```python signature
from collections.abc import Callable, Iterable
from math import isfinite
from typing import TYPE_CHECKING, Annotated, Final, Literal, assert_never

import numpy as np
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from google.protobuf.message import DecodeError
from msgspec import Struct
from upath import UPath

from rasm.compute.graduation.handoff import EVIDENCE_DOMAIN, ComputeLeg, EvidenceScope, GraduationReceipt, HandoffAxis, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, TERMINAL, Catch, FaultRow, Posture, RuntimeRail, boundary, rostered
from rasm.runtime.journal import Actor, Assigned, AuditFact, Fact, Journal, MeterFact, Party, Resource, Retain
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey
from rasm.runtime.roots import ResourceRef
from rasm.runtime.workers import Kernel, KernelTrait

# ONNX export stack defers: the converter, the graph checker, and the runtime session are heavy native loads no caller
# pays until an export or validation arm first dereferences one inside the worker.
lazy import onnx
lazy import onnxruntime
lazy from skl2onnx import get_latest_tested_opset_version, to_onnx

if TYPE_CHECKING:
    from onnx import ModelProto
    from onnxruntime import InferenceSession, ModelMetadata
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

# `model_asset` family's DEFAULT graduation ceiling, one governed row PER CHECK rather than a bar derived off whatever
# the run happened to measure: a derived bar makes every future non-binary verdict its own ceiling and clears by
# construction. The default projects over the verdicts the manifest actually carries, so a validation that never ran
# the parity probe bars three checks rather than naming a fourth the hub's key-coverage gate would then refuse.
_CHECK_CEILING: Final[Map[CheckKind, float]] = Map.of_seq([("structural", 0.0), ("io_binding", 0.0), ("smoke", 0.0), ("parity", 0.0)])

# each row pairs a sklearn verb with the ONNX output index it rides — a `zipmap`-off classifier emits `label` at 0 and dense
# scores at 1, `predict` rides 0 — so the parity diff reads the matching column, never an int64 label against a float reference.
PROBE_RANK: Final[Block[tuple[ProbeAttr, int]]] = Block.of_seq([("predict_proba", 1), ("decision_function", 1), ("predict", 0)])

# --- [TABLES] ---------------------------------------------------------------------------

# the ONNX stack's raise surface, proved against the installed distributions rather than authored: `onnx.load` raises
# `TypeError`/`ValueError` on a malformed argument and the protobuf `DecodeError` on torn wire bytes; `check_model`
# and `infer_shapes(strict_mode=True)` raise the two rostered exception rows (`.api/onnx.md:30-31`), which the
# `structural` arm already catches INSIDE the check and never lets reach this fence; `skl2onnx` raises
# `MissingConverter`/`MissingShapeCalculator` (both `RuntimeError` subclasses) beside `ValueError` from `to_onnx`;
# the artifact read and write raise `OSError`. `RuntimeError` is the converter family's own — the key-mint rail
# returns its refusal typed rather than re-raising it here. The `onnxruntime` session family is ABSENT — the
# distribution is not installed and
# `.api/onnxruntime.md` rosters no exception row — so its `capi` failure classes are owed a catalog row before a
# narrowing can name them; `RuntimeError`/`OSError`/`ValueError` cover the surface its documented failures share.
_ONNX_RAISES: Final[Catch] = (BeartypeCallHintViolation, DecodeError, OSError, RuntimeError, TypeError, ValueError)

# the envelope's own surface: `fit` RAISES `ValueError` on a non-finite column and on a refused band roster, its
# `@beartype(conf=FAULT_CONF)` contract raises the canonical violation, and `numpy`'s quantile and histogram folds
# raise `ValueError` on a degenerate column. `write` is create-only h5py, whose HDF5 failures surface as `OSError`
# (an existing container is `FileExistsError`) beside the `TypeError`/`ValueError` its dataset constructors raise.
_ENVELOPE_FIT_RAISES: Final[Catch] = (BeartypeCallHintViolation, ValueError)
_ENVELOPE_WRITE_RAISES: Final[Catch] = (OSError, TypeError, ValueError)

# this page's raise-side roster under the hub `ComputeLeg` roster: the retired `f"model.export.{source.tag}"` subject
# forked one refusal law into three coordinates, and the export source is already a span fact and a receipt column.
MODEL_VALIDATE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.MODEL, point="validate", arm="boundary", defect="session-validate", retriability=TERMINAL
)
MODEL_EXPORT: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.MODEL, point="export", arm="boundary", defect="graph-export", retriability=TERMINAL
)
ENVELOPE_FIT: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.MODEL, point="envelope", arm="config", defect="band-admission", retriability=TERMINAL
)
ENVELOPE_WRITE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.MODEL, point="container", arm="resource", defect="container-write", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([MODEL_VALIDATE, MODEL_EXPORT, ENVELOPE_FIT, ENVELOPE_WRITE]))

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
    producer: Posture[str]  # the graph's OWN declared producer; ABSENT where the ONNX metadata named none
    model_card: dict[str, str]
    verdicts: tuple[ValidationEvidence, ...]

    @property
    def band(self) -> Block[str]:
        # the spine's warning roster, and what the retired `validated: bool` collapsed to a single bit: a bool
        # answers THAT a check failed and erases WHICH, so a manifest failing the parity probe and one failing the
        # structural check read identically to every consumer downstream of the flag.
        return Block.of_seq(f"failed:{verdict.tag}" for verdict in self.verdicts if not verdict.passed)

    @property
    def residuals(self) -> Residual:
        # verdict ledger lowered to the measured-residual map — `0.0` on pass, `1.0` on fail, keyed by each verdict's own `tag`.
        return {v.tag: 0.0 if v.passed else 1.0 for v in self.verdicts}

    @property
    def span_facts(self) -> dict[str, str | int | bool]:
        # bounded scalars only — the per-check ledger rides the receipt facts, never the span — and not the spine's
        # own columns: the subject is the settlement's `concern` and the failed-check roster its `band`.
        return {"opset": self.opset, "providers": ",".join(self.providers), "breaches": len(self.band)}

    def subject(self) -> str:
        # an anonymous graph names itself by its OWN content address rather than by a fabricated `<anonymous>` label
        # every unnamed asset would share — a graduation axis keyed on one shared literal collides every unnamed
        # crossing onto one subject, and the address is a real identity the C# consumer can already join on.
        return self.producer.option().default_value(self.checksum.project("wire"))

    def graduates(self, ceiling: dict[str, float] | None = None, *, composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[GraduationReceipt]:
        # the governed `_ASSET_CEILING` row is the family default and a caller's tighter row the override; deriving the
        # bar from the residual roster instead would make every check's bar whatever that run happened to measure, so a
        # future non-binary verdict would bar itself at its own value and clear by construction. `composition` is the
        # caller's custody key, so an embedded composition's admission and refusal facts key to it.
        return GraduationReceipt.graduates(
            EvidenceScope.MODEL.value,
            HandoffAxis(model_asset=self.subject()),
            self.checksum,
            self.residuals,
            ceiling or {verdict.tag: _CHECK_CEILING[verdict.tag] for verdict in self.verdicts},
            composition=composition,
        )

    def contribute(self) -> Iterable[Receipt]:
        # ONE settled-receipt spine: the payload is this producer's own per-check ledger and graph census, while the
        # key, the provenance pair, the failed-check band, and the stamp are the spine's columns. Provenance names
        # the produced checksum alone — the manifest is derived from the artifact this key addresses and consumes no
        # upstream key — and `producer` rides its posture, so a card without a producer omits the slot rather than
        # publishing a name no metadata carried.
        facts: dict[str, object] = {
            "inputs": ",".join(self.input_names),
            "outputs": ",".join(self.output_names),
            "opset": self.opset,
            "op_types": ",".join(self.op_types),
            "providers": ",".join(self.providers),
            **{f"check[{v.tag}]": v.facts() for v in self.verdicts},
            **self.model_card,
        }
        return (
            Receipt.of(
                EvidenceScope.MODEL.value,
                ("emitted", self.subject(), facts),
                key=Some(self.checksum),
                provenance=Some(Provenance(consumed=Block.empty(), produced=self.checksum)),
                band=self.band,
            ),
        )


def _validate_kernel(asset: "ModelAsset") -> "RuntimeRail[ModelAssetManifest]":
    # module-level so the worker resolves both kernels by import; the fence converts a converter/checker/session raise.
    return boundary(MODEL_VALIDATE, lambda: asset._load_and_run(asset.ref.path, None), catch=_ONNX_RAISES).bind(lambda outcome: outcome)


def _export_kernel(asset: "ModelAsset", source: "ExportSource", gating: "OperatorGate") -> "RuntimeRail[ModelAssetManifest]":
    return boundary(MODEL_EXPORT, lambda: asset._export(source, gating), catch=_ONNX_RAISES).bind(lambda outcome: outcome)


class ModelAsset(Struct, frozen=True):  # holds a `ResourceRef` and a providers tuple — container fields keep it GC-tracked
    ref: ResourceRef
    providers: tuple[str, ...] = ()

    async def validate(self, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[ModelAssetManifest]:
        return await self._traced(lane, "validate", _validate_kernel, self, composition=composition)

    async def export(
        self, lane: LanePolicy, source: ExportSource, /, *, gating: OperatorGate = OperatorGate(), composition: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[ModelAssetManifest]:
        return await self._traced(lane, f"export.{source.tag}", _export_kernel, self, source, gating, composition=composition)

    async def _traced(
        self, lane: LanePolicy, op: str, kernel: Callable[..., "RuntimeRail[ModelAssetManifest]"], *args: object, composition: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[ModelAssetManifest]:
        # weave owns span, fence, and the fenced contributor harvest.
        async def dispatch() -> RuntimeRail[ModelAssetManifest]:
            return (await lane.offload(Kernel.of(kernel, KernelTrait.RELEASING), *args)).bind(lambda rail: rail)

        return await evidence_run(EvidenceScope.MODEL, f"model.{op}", dispatch, facts={"op": op, "providers": ",".join(self.providers)}, composition=composition)

    @beartype(conf=FAULT_CONF)
    def _load_and_run(self, path: UPath, source: ExportSource | None) -> "RuntimeRail[ModelAssetManifest]":
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
        # the checksum rail THREADS rather than re-raising: the retired `raise RuntimeError(fault)` handed an
        # already-typed `BoundaryFault` to this body's own fence to re-classify, and the conversion keeps
        # `str(cause)` — so a digest refusal reached its consumer as a message string with its subject, leg, arm, and
        # defect token erased. Returning the rail also keeps the checksum from ever being masked by a fabricated key.
        return ContentIdentity.of("onnx", path.read_bytes()).map(lambda checksum: self._manifested(checksum, model, session, meta, inputs, outputs, checks))

    def _manifested(
        self, checksum: ContentKey, model: "ModelProto", session: "InferenceSession", meta: "ModelMetadata",
        inputs: tuple[str, ...], outputs: tuple[str, ...], checks: "Block[ValidationCheck]",
    ) -> ModelAssetManifest:
        # the settled projection, seated apart so the keyed body stays one expression on the rail.
        return ModelAssetManifest(
            checksum=checksum,
            input_names=inputs,
            output_names=outputs,
            opset=int(max(o.version for o in model.opset_import)),
            op_types=tuple(n.op_type for n in model.graph.node),
            providers=tuple(session.get_providers()),
            producer=Posture.of_optional(meta.producer_name or None),
            # a metadata field ONNX never carried is OMITTED rather than defaulted: the retired `or ""` wrote an
            # empty string into the card, and a consumer reading a blank domain cannot tell an unset field from a
            # graph that declared one and left it blank. The two always-present fields stay unconditional.
            model_card={
                **dict(Block.of_seq((("domain", meta.domain), ("graph_name", meta.graph_name))).filter(lambda pair: bool(pair[1]))),
                "version": str(meta.version),
                "ir_version": str(model.ir_version),
                **{p.key: p.value for p in model.metadata_props},
                **{k: str(v) for k, v in meta.custom_metadata_map.items()},
            },
            verdicts=tuple(checks.map(ValidationCheck.run)),
        )

    def _session(self, path: UPath) -> "InferenceSession":
        options = onnxruntime.SessionOptions()
        options.graph_optimization_level = onnxruntime.GraphOptimizationLevel.ORT_ENABLE_ALL
        available = set(onnxruntime.get_available_providers())
        preference = [p for p in self.providers if p in available] or None
        return onnxruntime.InferenceSession(str(path), sess_options=options, providers=preference)

    @beartype(conf=FAULT_CONF)
    def _export(self, source: ExportSource, gating: OperatorGate) -> "RuntimeRail[ModelAssetManifest]":
        graph = source.convert(get_latest_tested_opset_version(), gating)
        self.ref.path.write_bytes(graph.SerializeToString())
        return self._load_and_run(self.ref.path, source)
```

## [03]-[ENVELOPE]

- Owner: `GraduationEnvelope` — the serving-population drift companion this owner fits at graduation and ships beside the ONNX artifact: `ReferenceBand` is the numeric-or-categorical band union, `fit` derives each feature's band from the training columns only this owner holds, and `write` is the `hdf5-exchange/graduation` domain producer whose container `csharp:Rasm.Compute/Model/identity#MODEL_IDENTITY` `GraduationEnvelope.Admit(HdfHandle)` consumes. The reverse JSON `EvidenceBundle` leg stays whole on `graduation/codegen#STUB_CODEGEN`, untouched.
- Cases: this producer's law is one root `bands` group carrying the `evidence-key` attribute as the 32-hex `ContentKey` rendering the C# parses `NumberStyles.HexNumber`; one group per feature carrying the `kind` attribute (`numeric`/`categorical`); numeric bands the explicit little-endian `edges` float64[k] and `mass` float64[k+1] datasets, categorical bands the vlen UTF-8 `categories` and little-endian float64 `mass` datasets.
- Law: `write_async` is the ONE durable seat and the crossing's only movement evidence — one `REGULATORY` `AuditFact` naming the destination beside a `STORAGE` `MeterFact` over the bytes landed. It is an awaitable twin because this owner carries no weave and `write` is synchronous whole while recording suspends; without the line, a reference population leaves for the peer's admission gate and neither branch records that it moved. `REGULATORY` is earned rather than inherited: a drift envelope is the population a served model is graded against for as long as that model serves. The record rail BINDS, so a container the plane refused never reads as written.
- Entry: `GraduationEnvelope.fit(evidence_key, numeric, categorical)` folds the training columns into admitted bands or a typed refusal; `write(ref)` is create-only h5py, one call landing roster, attributes, and datasets whole and answering the container's byte extent; `write_async(ref)` is its awaitable twin.
- Auto: `fit` mirrors every `Wellformed` gate BEFORE bytes land, so a container this writer emits never fails the peer's admission — finite strictly-increasing edges, mass length `edges + 1`, every mass strictly positive and summing to one within `1e-9`, non-blank unique features and categories, a non-zero evidence key. Numeric edges are interior training quantiles, so the k+1 mass vector covers BOTH outer bins the peer's half-open bisection addresses; `_edges` drops any edge bounding an empty bin until every bin holds mass, because duplicated quantiles over ties otherwise mint a zero-mass bin the peer's normalization gate refuses.
- Receipt: the envelope is a crossing artifact, not hub evidence — it graduates nothing itself; the `model_asset` axis crossing on `[02]-[ASSET]` stays the one graduation leg, and the envelope's container `ContentKey` pairs the artifact with that crossing's evidence key.
- Growth: a new band case is one `ReferenceBand` case with its `kind` literal and the C# peer's matching admission arm in the same contract change; a new fit policy is one parameter on `fit`; a newly audited container column is one `_evidence` `Change` row; zero new surface.
- Boundary: reference mass is fitted HERE and never at the peer — the C# comment pins that division; the statistic, thresholds, and sampling floors are `DriftPolicy` rows at the consumer, so no policy value crosses in the container; `h5py` composes under the compute-tier `.api/h5py.md` admission.

```python signature
# composes the [02]-[ASSET] prelude; `h5py` imports module-top beside it.
import h5py
from collections.abc import Mapping

_MASS_TOL: Final[float] = 1e-9  # the ingest fence's own normalization tolerance, transcribed
_BINS: Final[int] = 10  # default interior-quantile count; a caller's k overrides at `fit`


@tagged_union(frozen=True)
class ReferenceBand:
    tag: Literal["numeric", "categorical"] = tag()
    numeric: tuple[str, tuple[float, ...], tuple[float, ...]] = case()
    categorical: tuple[str, tuple[str, ...], tuple[float, ...]] = case()

    @property
    def feature(self) -> str:
        match self:
            case ReferenceBand(tag="numeric", numeric=(name, _, _)) | ReferenceBand(tag="categorical", categorical=(name, _, _)):
                return name
            case _ as unreachable:
                assert_never(unreachable)

    def wellformed(self) -> bool:
        # transcription of the peer `Band.Wellformed`, minus the roster-uniqueness half `fit` owns.
        match self:
            case ReferenceBand(tag="numeric", numeric=(name, edges, mass)):
                increasing = all(isfinite(e) for e in edges) and all(a < b for a, b in zip(edges, edges[1:], strict=False))
                return bool(name.strip()) and len(mass) == len(edges) + 1 and increasing and _normalized(mass)
            case ReferenceBand(tag="categorical", categorical=(name, categories, mass)):
                labelled = bool(categories) and len(categories) == len(mass) and all(c.strip() for c in categories)
                return bool(name.strip()) and labelled and len(set(categories)) == len(categories) and _normalized(mass)
            case _ as unreachable:
                assert_never(unreachable)


def _normalized(mass: tuple[float, ...]) -> bool:
    return all(isfinite(m) and m > 0.0 for m in mass) and abs(sum(mass) - 1.0) <= _MASS_TOL


def _evidence(envelope: "GraduationEnvelope", ref: ResourceRef, written: int) -> Block[Fact]:
    # the crossing's only movement record: this container leaves the process for the C# ingest fence, and the
    # envelope graduates nothing itself, so no hub receipt names it and no span brackets it. `REGULATORY` is the
    # class — a drift envelope is the reference population a served model is graded against for as long as that
    # model serves, so its arrival is evidence read back years later. The meter carries the bytes the writer
    # actually landed, keyed on the same destination the audit target names, so one row answers what moved and where.
    audited = AuditFact(
        action=f"{EVIDENCE_DOMAIN}.envelope",
        actor=Party(kind=Actor.SERVICE, key=EvidenceScope.MODEL.value),
        target=Party(kind="artifact", key=str(ref.path)),
        retention=Retain.REGULATORY,
        change=(
            Assigned(path="/evidence_key", next=envelope.evidence_key.hex),
            Assigned(path="/bands", next=str(len(envelope.bands))),
        ),
    )
    return Block.of_seq((audited, MeterFact(resource=Resource.STORAGE, quantity=written, surface=str(ref.path))))


def _edges(values: np.ndarray, bins: int) -> np.ndarray:
    # interior quantiles deduped, then any edge bounding an empty bin drops until every bin holds mass —
    # ties in the training column otherwise mint a zero-mass bin the peer's `Normalized` gate refuses.
    edges = np.unique(np.quantile(values, np.linspace(0.0, 1.0, bins + 1)[1:-1]))
    while edges.size:
        counts, _ = np.histogram(values, bins=np.concatenate(([-np.inf], edges, [np.inf])))
        if (counts > 0).all():
            return edges
        edges = np.delete(edges, max(int(np.argmin(counts)) - 1, 0))
    return edges


class GraduationEnvelope(Struct, frozen=True):
    evidence_key: ContentKey
    bands: tuple[ReferenceBand, ...]

    @classmethod
    @beartype(conf=FAULT_CONF)
    def fit(
        cls, evidence_key: ContentKey, numeric: Mapping[str, np.ndarray], categorical: Mapping[str, np.ndarray], *, bins: int = _BINS
    ) -> "RuntimeRail[GraduationEnvelope]":
        def build() -> GraduationEnvelope:
            rows: list[ReferenceBand] = []
            for name, column in numeric.items():
                values = np.asarray(column, dtype=float)
                if not np.isfinite(values).all():
                    raise ValueError(f"envelope column {name}: non-finite training value")
                edges = _edges(values, bins)
                counts, _ = np.histogram(values, bins=np.concatenate(([-np.inf], edges, [np.inf])))
                rows.append(ReferenceBand(numeric=(name, tuple(map(float, edges)), tuple(counts / values.size))))
            for name, column in categorical.items():
                labels, tallies = np.unique(np.asarray(column, dtype=str), return_counts=True)
                rows.append(ReferenceBand(categorical=(name, tuple(map(str, labels)), tuple(tallies / tallies.sum()))))
            features = [row.feature for row in rows]
            admitted = evidence_key.value != 0 and rows and len(set(features)) == len(features) and all(row.wellformed() for row in rows)
            if not admitted:
                raise ValueError(f"envelope admission: features={features}")
            return cls(evidence_key=evidence_key, bands=tuple(rows))

        return boundary(ENVELOPE_FIT, build, catch=_ENVELOPE_FIT_RAISES)

    def write(self, ref: ResourceRef) -> "RuntimeRail[int]":
        # create-only domain mint: root `bands` group, `evidence-key` hex attribute,
        # per-feature `kind` beside its case datasets — float64 exact, categories vlen-string. The byte extent
        # returns rather than `None`: the storage charge and any caller reconciling the artifact both need what
        # actually landed, and a writer answering nothing forces a second stat at every consumer.
        def emit() -> int:
            with h5py.File(str(ref.path), "x") as file:
                root = file.create_group("bands")
                root.attrs["evidence-key"] = self.evidence_key.hex
                for band in self.bands:
                    node = root.create_group(band.feature)
                    match band:
                        case ReferenceBand(tag="numeric", numeric=(_, edges, mass)):
                            node.attrs["kind"] = "numeric"
                            node.create_dataset("edges", data=np.asarray(edges, dtype="<f8"))
                            node.create_dataset("mass", data=np.asarray(mass, dtype="<f8"))
                        case ReferenceBand(tag="categorical", categorical=(_, categories, mass)):
                            node.attrs["kind"] = "categorical"
                            node.create_dataset("categories", data=list(categories), dtype=h5py.string_dtype(encoding="utf-8"))
                            node.create_dataset("mass", data=np.asarray(mass, dtype="<f8"))
                        case _ as unreachable:
                            assert_never(unreachable)
            # read past the close, so the extent names a flushed container rather than an open handle's buffer.
            return ref.path.stat().st_size

        return boundary(ENVELOPE_WRITE, emit, catch=_ENVELOPE_WRITE_RAISES)

    async def write_async(self, ref: ResourceRef, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[int]":
        # the awaitable twin over the band hop, and the ONLY movement evidence this crossing carries: the envelope
        # graduates nothing, so no weave brackets it and no hub receipt names it, while `write` is synchronous whole
        # and recording suspends. Without this line neither branch records that a reference population left this
        # process for the peer's admission gate. The record rail BINDS: a container the plane could not account for
        # must not read as written, and an unjournalled composition folds to the lawful no-op at one map read.
        match self.write(ref):
            case Result(tag="ok", ok=written):
                return (await Journal.record(_evidence(self, ref, written), scope=composition)).map(lambda _landed: written)
            case refused:
                return Error(refused.error)
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
