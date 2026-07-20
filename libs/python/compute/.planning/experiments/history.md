# [PY_COMPUTE_HISTORY]

Experiment-run persistence, resume, and comparison rail on the study spine: `experiments/study.md#STUDY` owns one grid evaluation, `RunHistory` owns the multi-run cohort that persists, resumes, and compares those evaluations, never a parallel experiment tracker. A `Partial` resume evaluates only the remaining grid rows yet recomputes the sensitivity indices over the whole reconstituted response vector — a SALib variance-, moment-, or derivative-based index is undefined over a design tail slice — so a resumed receipt is statistically indistinguishable from an unbroken run. Compute owns no durable run store: the resume proof is key equality over caller-supplied evidence, never storage.

Response caching is one `Map[ContentKey, np.ndarray]` keyed by `Study.spec_key` — axes, method, mode, the objective's full identity (row/batch scorer shipping identity and the jit route row), and design bytes in one preimage — so a data, method, mode, scorer, or jit/batch configuration change keys distinctly and never collides to a stale hit. `resume` evaluates its remaining rows through the same `HOSTILE`-trait `Kernel` crossing `experiments/study.md#STUDY` `Study.run` rides — the module-level `_resume_kernel` ships `REFERENCE`, a closure-bearing objective crosses on the pool's cloudpickle wire — while `compare` stays the sync `_traced` weave; both run under the `EvidenceScope.HISTORY` span with the `boundary` fence over beartype-guarded bodies, receipts harvested through the weave's fenced emit at the `runtime/observability/receipts#RECEIPT` owner.

## [01]-[INDEX]

- [01]-[RUN_HISTORY]: content-keyed run persistence, `ResumePlan`-discriminated resume, and `CrossStat` cohort comparison on one `RunHistory` owner.

## [02]-[RUN_HISTORY]

- Owner: `RunHistory` — the study receipt is the per-grid evidence, `RunHistory` the cohort that keys, resumes, and compares those receipts.
- Cases: `ResumePlan` discriminates `Complete`/`Partial`/`Fresh` against the prior run through one total `match`, so a new resume policy is one plan case and one `match` arm, never a new entrypoint.
- Output: `CrossStat` parameterizes the comparison on both axes — the variadic `*keys` cohort in, the per-statistic agreement table out — reading run concurrence as per-axis sensitivity-ordering agreement, never a side-by-side index transpose.
- Growth: a new resume outcome is one `ResumePlan` case and its `match` arm; a new comparison projection is one `RunProjection` field; a new cross-run statistic is one `CrossStat` member and one `_KERNELS` row; a new sync entrypoint shares the `_traced` weave by passing its `Traceable`-returning thunk, and an evaluating entrypoint crosses on the study kernel's lane.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import Final, Literal, Protocol, assert_never, runtime_checkable

import numpy as np
from beartype import beartype
from expression import Error, Nothing, Ok, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.experiments.study import Measured, Objective, Study, StudyReceipt
from rasm.compute.graduation.handoff import EvidenceScope, SpanFacts, evidence_run
from rasm.runtime.identity import ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt, ReceiptContributor
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] ----------------------------------------------------------------------------

# every kernel reads the raw shared-axis index vectors AND their ordinal ranks, so one signature serves the rank and raw rows.
type CrossKernel = Callable[[np.ndarray, np.ndarray, np.ndarray, np.ndarray], float]


# `_traced` egress bound: a receipt streams its `contribute` facts AND projects the bounded scalars the `Ok` arm writes
# onto the span; `StudyReceipt` and `ComparisonReceipt` both satisfy it, so one bound `E` carries either egress.
@runtime_checkable
class Traceable(ReceiptContributor, Protocol):
    @property
    def span_facts(self) -> dict[str, str | int | float]: ...


class CrossStat(StrEnum):
    RANK_CORRELATION = "rank_correlation"  # Spearman rho: Pearson over the no-ties argsort ranks
    RANK_DISTANCE = "rank_distance"  # Spearman footrule: 1 - normalized L1 rank displacement
    KENDALL_TAU = "kendall_tau"  # concordant-minus-discordant pair fraction over the ranks
    LINEAR_CORRELATION = "linear_correlation"  # Pearson over the raw shared-axis index magnitudes

    # statistic family OWNS its kernels: the rank transform, the closed-form kernels, and the `_KERNELS` dispatch table are
    # static members of this owner, never module-level free functions the enum reaches back down into.
    def score(self, u_idx: np.ndarray, v_idx: np.ndarray) -> float:
        u_rank, v_rank = CrossStat._rank(u_idx), CrossStat._rank(v_idx)
        return CrossStat._KERNELS[self](u_idx, v_idx, u_rank, v_rank)

    # every shared-axis run pair scores through `self.score` and lands in one agreement map.
    def agreement(self, rows: "Block[RunProjection]") -> dict[str, float]:
        shared = sorted(rows.map(lambda r: frozenset(r.indices)).reduce(frozenset.intersection)) if rows else []
        if len(shared) < 2:
            return {}

        def vector(run: "RunProjection") -> np.ndarray:
            return np.asarray([run.indices[axis] for axis in shared], dtype=float)

        pairs = rows.mapi(lambda i, a: rows.skip(i + 1).map(lambda b: (f"{a.name}~{b.name}", self.score(vector(a), vector(b))))).collect(lambda p: p)
        return dict(pairs)

    @staticmethod
    def _rank(values: np.ndarray) -> np.ndarray:
        # double-argsort ordinal rank transform: no-ties ranks for the shared-axis index vector.
        return np.argsort(np.argsort(values))

    @staticmethod
    def _correlation(u: np.ndarray, v: np.ndarray) -> float:
        # Pearson over `u`/`v`; the population-std ddof cancels in the ratio. A zero-variance operand
        # (constant index column) folds to perfect agreement `1.0` rather than a `0/0` NaN.
        du, dv = u - u.mean(), v - v.mean()
        denom = float(du.std() * dv.std())
        return float((du * dv).mean() / denom) if denom else 1.0

    @staticmethod
    def _tau(u_rank: np.ndarray, v_rank: np.ndarray) -> float:
        # `einsum("ij,ij->", su, sv)` contracts the two sign matrices without materializing their product; symmetric with a zero
        # diagonal under no-ties ranks, so `/ 2` is the upper-triangular pair sum with no explicit `triu` extract.
        su = np.sign(np.subtract.outer(u_rank, u_rank))
        sv = np.sign(np.subtract.outer(v_rank, v_rank))
        n = len(u_rank)
        return float(np.einsum("ij,ij->", su, sv)) / 2.0 / max(n * (n - 1) // 2, 1)

    # a new statistic is one enum member plus one row, never a per-stat method.
    _KERNELS: "Map[CrossStat, CrossKernel]"


CrossStat._KERNELS = Map.of_seq([
    (CrossStat.RANK_CORRELATION, lambda _u, _v, ur, vr: CrossStat._correlation(ur, vr)),
    (CrossStat.RANK_DISTANCE, lambda _u, _v, ur, vr: 1.0 - float(np.abs(ur - vr).sum()) / max(len(ur) ** 2 // 2, 1)),
    (CrossStat.KENDALL_TAU, lambda _u, _v, ur, vr: CrossStat._tau(ur, vr)),
    (CrossStat.LINEAR_CORRELATION, lambda u, v, _ur, _vr: CrossStat._correlation(u, v)),
])

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class ResumePlan:
    tag: Literal["complete", "partial", "fresh"] = tag()
    complete: StudyReceipt = case()
    partial: tuple[StudyReceipt, int, np.ndarray] = case()
    fresh: tuple[()] = case()

    @staticmethod
    def Complete(prior: StudyReceipt) -> "ResumePlan":
        return ResumePlan(complete=prior)

    @staticmethod
    def Partial(prior: StudyReceipt, done: int, cached: np.ndarray) -> "ResumePlan":
        return ResumePlan(partial=(prior, done, cached))

    @staticmethod
    def Fresh() -> "ResumePlan":
        return ResumePlan(fresh=())

    @staticmethod
    def of(prior: StudyReceipt | None, cached: np.ndarray | None, total: int) -> "ResumePlan":
        # resume index is the cached vector's ROW count `len(prefix)` against the design height `total` — `design[done:]` and
        # `prefix[:done]` address rows, never the scalar element count `StudyReceipt.response_width` carries.
        match prior, cached:
            case StudyReceipt(), np.ndarray() as prefix if len(prefix) >= total:
                return ResumePlan.Complete(prior)
            case StudyReceipt(), np.ndarray() as prefix:
                return ResumePlan.Partial(prior, len(prefix), prefix)
            case _:
                return ResumePlan.Fresh()


class RunProjection(Struct, frozen=True):
    name: str
    design_cells: int  # evaluated design rows, read off the prior `StudyReceipt.design_cells`
    response_width: int  # per-cell output arity, read off the prior `StudyReceipt.response_width`
    indices: dict[str, float]

    @staticmethod
    def of(receipt: StudyReceipt) -> "RunProjection":
        return RunProjection(
            name=f"{receipt.method}:{receipt.content_key.hex[:8]}",
            design_cells=receipt.design_cells,
            response_width=receipt.response_width,
            indices=receipt.indices,
        )

    @property
    def width_ratio(self) -> float:
        # per-cell output arity normalized to the row count — a parameterized fact, never a completion fraction.
        return self.response_width / self.design_cells if self.design_cells else 0.0


class ComparisonReceipt(Struct, frozen=True):
    names: tuple[str, ...]
    cells: dict[str, tuple[int, int]]  # name -> (design_cells, response_width)
    ratios: dict[str, float]  # name -> RunProjection.width_ratio, the per-cell arity normalization
    indices: dict[str, dict[str, float]]
    agreement: dict[str, dict[str, float]]  # stat -> {pair -> score}, the per-CrossStat matrix

    @property
    def span_facts(self) -> dict[str, str | int | float]:
        # bounded scalars only — the full per-pair agreement matrix and per-run index ledger ride the receipt facts, never the span.
        shared = sorted(frozenset.intersection(*(frozenset(idx) for idx in self.indices.values()))) if self.indices else []
        return {"runs": len(self.names), "shared_axes": len(shared), "stats": len(self.agreement)}

    def contribute(self) -> Iterable[Receipt]:
        # counts ride as native ints and scores as native floats — no `str()`/`f"{rows}x{width}"` pre-format where the
        # deterministic renderer keeps types.
        facts: dict[str, object] = {
            **{f"rows[{k}]": rows for k, (rows, _) in self.cells.items()},
            **{f"width[{k}]": width for k, (_, width) in self.cells.items()},
            **{f"ratio[{k}]": ratio for k, ratio in self.ratios.items()},
            **{f"agree[{stat}:{pair}]": score for stat, m in self.agreement.items() for pair, score in m.items()},
        }
        return (Receipt.of(EvidenceScope.HISTORY.value, ("emitted", ",".join(self.names), facts)),)


# --- [OPERATIONS] -----------------------------------------------------------------------


@beartype(conf=FAULT_CONF)
def _compare(by_key: Map[ContentKey, StudyReceipt], keys: tuple[ContentKey, ...], stats: frozenset[CrossStat]) -> ComparisonReceipt:
    # `Block.partition` over `try_find` splits resolved receipts from unresolved keys in one pass, so a missing cohort names EVERY
    # absent hex in one `KeyError` the fence folds to a typed fault, never a first-miss raise; the resolved side lowers through
    # `Block.choose`, never a raw `Option.value` read.
    found, missing = Block.of_seq(keys).map(lambda k: (k, by_key.try_find(k))).partition(lambda kv: kv[1].is_some())
    if missing:
        raise KeyError(", ".join(k.hex for k, _ in missing))
    rows: Block[RunProjection] = found.choose(lambda kv: kv[1].map(RunProjection.of))
    return ComparisonReceipt(
        names=tuple(row.name for row in rows),
        cells={row.name: (row.design_cells, row.response_width) for row in rows},
        ratios={row.name: row.width_ratio for row in rows},
        indices={row.name: row.indices for row in rows},
        agreement={stat.value: stat.agreement(rows) for stat in stats},
    )


@beartype(conf=FAULT_CONF)
def _resume(
    by_key: Map[ContentKey, StudyReceipt], cache: Map[ContentKey, np.ndarray], study: Study, objective: Objective, design: np.ndarray, key: ContentKey
) -> StudyReceipt:
    prior = by_key.try_find(key).to_optional()
    cached = cache.try_find(key).to_optional()
    match ResumePlan.of(prior, cached, len(design)):
        case ResumePlan(tag="complete", complete=done_run):
            return done_run
        case ResumePlan(tag="partial", partial=(_, done, prefix)):
            # only the rows the prior run left undone ride the study owner's `Objective.rows` serial
            # stack; `concatenate` reconstitutes the full vector a single unbroken run would produce.
            return _recompute(study, design, np.concatenate([prefix, objective.rows(design[done:])]), key)
        case ResumePlan(tag="fresh"):
            return _recompute(study, design, objective.rows(design), key)
        case _ as unreachable:
            assert_never(unreachable)


def _recompute(study: Study, design: np.ndarray, responses: np.ndarray, key: ContentKey) -> StudyReceipt:
    # `graded` re-derives indices and discrepancy through the `StudyMethod` union folds, so this owner re-declares no design
    # algebra; elapsed is zero and speedup absent because the timing belongs to the original evaluation.
    return StudyReceipt.graded(study, design, Measured(responses, 0.0, Nothing), key)


def _resume_kernel(
    by_key: Map[ContentKey, StudyReceipt], cache: Map[ContentKey, np.ndarray], study: Study, objective: Objective, seed: int
) -> RuntimeRail[StudyReceipt]:
    # module-level so REFERENCE shipping resolves it by import — the crossing law `study._study_kernel` holds; the fence
    # converts a design/scorer/analyzer raise, and a closure-bearing objective crosses on the pool's cloudpickle wire.
    # design generation and the key mint run worker-side: `method.design` is CPU work an awaiting caller must never host,
    # and an encode refusal rails through the same fence as every other worker fault.
    def worked() -> RuntimeRail[StudyReceipt]:
        design = study.method.design(study.axes, seed)
        return study.spec_key(design, Some(objective)).map(lambda key: _resume(by_key, cache, study, objective, design, key))

    return boundary("history.resume", worked).bind(lambda rail: rail)


# --- [COMPOSITION] ----------------------------------------------------------------------


class RunHistory(Struct, frozen=True):
    runs: tuple[StudyReceipt, ...]
    responses: Map[ContentKey, np.ndarray] = Map.empty()  # content-keyed cache; a run absent here falls to `Fresh`

    @property
    def _by_key(self) -> Map[ContentKey, StudyReceipt]:
        return Map.of_seq((r.content_key, r) for r in self.runs)

    async def resume(self, study: Study, objective: Objective, lane: LanePolicy, /, *, seed: int = 0) -> RuntimeRail[StudyReceipt]:
        # remaining-row evaluation is the same HOSTILE crossing `Study.run` rides — `objective.rows` AND `method.design`
        # on the loop would stall it, so the kernel derives design and key worker-side through `Study.spec_key`, the ONE
        # mint both owners share (axes, method, mode, objective label, design bytes), so the key equals the original
        # run's by construction and the response cache hits — and `_recompute` grades zero elapsed, so a worker-death
        # re-run reproduces the receipt and the retry default stands.
        async def dispatch() -> RuntimeRail[StudyReceipt]:
            kernel = Kernel.of(_resume_kernel, KernelTrait.HOSTILE)
            return (await lane.offload(kernel, self._by_key, self.responses, study, objective, seed)).bind(lambda rail: rail)

        facts = {"method": study.method.tag, "runs": len(self.runs)}
        return await evidence_run(EvidenceScope.HISTORY, "history.resume", dispatch, facts=facts)

    def compare(self, *keys: ContentKey, stats: frozenset[CrossStat] = frozenset({CrossStat.RANK_CORRELATION})) -> RuntimeRail[ComparisonReceipt]:
        # single-pair join is the two-key cohort; the statistic family is the `stats` parameter.
        return self._traced("compare", lambda: _compare(self._by_key, keys, stats), {"cohort": len(keys), "stats": len(stats)})

    def _traced[E: Traceable](self, op: str, thunk: Callable[[], E], facts: SpanFacts = Map.empty()) -> RuntimeRail[E]:
        # sync weave — span, fence over the beartype-guarded body, fenced receipt harvest — so a contract violation folds
        # through the `CLASSIFY` `api` row and a missing-cohort `KeyError` through the `boundary` row.
        return evidence_run(EvidenceScope.HISTORY, f"history.{op}", lambda: boundary(f"history.{op}", thunk), facts=facts)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
