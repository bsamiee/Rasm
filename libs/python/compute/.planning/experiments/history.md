# [PY_COMPUTE_HISTORY]

Experiment-run persistence, resume, and comparison rail on the study spine: `experiments/study#STUDY` owns one grid evaluation, `RunHistory` owns the multi-run cohort that persists, resumes, and compares those evaluations. `StudyRun` retains the sampled design and measured response vector beside its derived facts; `Partial` resume evaluates only the remaining rows and recomputes sensitivity indices over the whole reconstituted vector. Run-scoped census stays the resume's own — zero elapsed, absent speedup, `evaluated_cells` counting only the rows admitted fresh — so a resume never re-bills or re-benches the held prefix.

`Study.spec_key` binds axes, method, mode, objective identity, seed, and design bytes to each run, so `RunHistory` selects held responses through the run's own key and keeps no parallel response map. `resume` evaluates the remaining rows through the same `HOSTILE`-trait `Kernel` crossing `Study.run` rides; `compare` stays on the synchronous weave. Both entries bind the caller's `ScopeKey`, and each settled value stamps its attributes on that live span. `scipy.stats` supplies the rank-correlation family the cohort comparison reads.

## [01]-[INDEX]

- [02]-[RUN_HISTORY]: content-keyed run persistence, `ResumePlan`-discriminated resume, and `CrossStat` cohort comparison on one `RunHistory` owner.

## [02]-[RUN_HISTORY]

- Owner: `RunHistory` — `StudyRun` is the per-grid product and `RunHistory` keys, resumes, and compares those runs.
- Cases: `ResumePlan` discriminates `Complete`/`Partial`/`Fresh` against the prior run through one total `match`, so a new resume policy is one plan case and one `match` arm, never a new entrypoint.
- Law: the async `resume` fold settles one `Resource.RECORD` `MeterFact` off the cleared run's `meter` projection — `Partial` bills the design tail, `Fresh` the whole grid, and `Complete` re-stamps the held run's census and timing to zero. Degenerate comparison operands drop from the agreement map through `Option[float]`, keeping undefined correlations out of span attributes.
- Output: `CrossStat` parameterizes the comparison on both axes — the variadic `*keys` cohort in, the per-statistic agreement table out — reading run concurrence as per-axis sensitivity-ordering agreement, never a side-by-side index transpose. Its kernels ARE the `scipy.stats` estimators: `spearmanr`, `kendalltau`, and `pearsonr` each answer their `.statistic` with the tie correction a local double-`argsort` transform silently drops, and `kendalltau`'s merge is O(n log n) where a sign-matrix contraction materializes two O(n²) operands per pair. Only the footrule distance has no scipy estimator, so it alone composes `rankdata` — one row, never a rank transform standing beside the provider's.
- Stage: `resume` reports THREE named positions off its own closed `ResumeStage` roster — the regenerated design and its minted key, the tail evaluation, and the whole-vector recompute — and the per-row beat carries the RUNNING total across the cached prefix, so a resumed fold reports progress against the whole grid rather than restarting at the tail's first row. The mark is ONE `StageTap` the entry opens with an absent census and the worker re-stamps.
- Output: `Comparison` carries the cohort key — a merkle address over its member keys, order-sensitive by construction — and its `attributes` name those members, so the span and the value address one cohort.
- Growth: a new resume state is one `ResumePlan` case and its `match` arm; a new comparison projection is one `RunProjection` field; a new cross-run statistic is one `CrossStat` member and one `_KERNELS` row naming its estimator; a new interior position is one `ResumeStage` member and one `beat` call; a new sync entrypoint shares the `_traced` weave by passing its own `FaultRow`, provider raise set, and rail-returning thunk.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable
from enum import StrEnum
from math import isfinite
from typing import Final, Literal, Protocol, assert_never, runtime_checkable

import numpy as np
from beartype import beartype
from beartype.roar import BeartypeCallHintViolation
from expression import Error, Nothing, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

from opentelemetry import trace

from rasm.compute.experiments.study import Measured, Objective, Study, StudyRun
from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, SpanFacts, StageTap, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import FAULT_CONF, TERMINAL, Catch, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.journal import Journal
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy from scipy import stats

# --- [TYPES] ----------------------------------------------------------------------------

type CrossKernel = Callable[[np.ndarray, np.ndarray], float]


class ResumeStage(StrEnum):
    PLANNED = "planned"
    EVALUATED = "evaluated"
    RECOMPUTED = "recomputed"


@runtime_checkable
class Traceable(Protocol):
    @property
    def content_key(self) -> ContentKey: ...

    @property
    def attributes(self) -> dict[str, str | int | float]: ...


class CrossStat(StrEnum):
    RANK_CORRELATION = "rank_correlation"
    RANK_DISTANCE = "rank_distance"
    KENDALL_TAU = "kendall_tau"
    LINEAR_CORRELATION = "linear_correlation"

    def score(self, u_idx: np.ndarray, v_idx: np.ndarray) -> Option[float]:
        scored = CrossStat._KERNELS[self](u_idx, v_idx)
        return Some(scored) if isfinite(scored) else Nothing

    def agreement(self, rows: "Block[RunProjection]") -> dict[str, float]:
        shared = sorted(rows.map(lambda r: frozenset(r.indices)).reduce(frozenset.intersection)) if rows else []
        if len(shared) < 2:
            return {}

        def vector(run: "RunProjection") -> np.ndarray:
            return np.asarray([run.indices[axis] for axis in shared], dtype=float)

        pairs = rows.mapi(
            lambda i, a: rows.skip(i + 1).choose(lambda b: self.score(vector(a), vector(b)).map(lambda s: (f"{a.name}~{b.name}", s)))
        ).collect(lambda p: p)
        return dict(pairs)

    _KERNELS: "Map[CrossStat, CrossKernel]"


def _footrule(u: np.ndarray, v: np.ndarray) -> float:
    u_rank, v_rank = stats.rankdata(u), stats.rankdata(v)
    return 1.0 - float(np.abs(u_rank - v_rank).sum()) / max(len(u_rank) ** 2 // 2, 1)


CrossStat._KERNELS = Map.of_seq([
    (CrossStat.RANK_CORRELATION, lambda u, v: float(stats.spearmanr(u, v).statistic)),
    (CrossStat.RANK_DISTANCE, _footrule),
    (CrossStat.KENDALL_TAU, lambda u, v: float(stats.kendalltau(u, v).statistic)),
    (CrossStat.LINEAR_CORRELATION, lambda u, v: float(stats.pearsonr(u, v).statistic)),
])

# --- [TABLES] ---------------------------------------------------------------------------

_RESUME_RAISES: Final[Catch] = (BeartypeCallHintViolation, AssertionError, RuntimeError, ValueError)

_COMPARE_RAISES: Final[Catch] = (BeartypeCallHintViolation, KeyError, RuntimeError, ValueError)

HISTORY_RESUME: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.HISTORY, point="resume", arm="boundary", defect="resume-evaluate", retriability=TERMINAL
)
HISTORY_COMPARE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.HISTORY, point="compare", arm="boundary", defect="cohort-compare", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([HISTORY_RESUME, HISTORY_COMPARE]))

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class ResumePlan:
    tag: Literal["complete", "partial", "fresh"] = tag()
    complete: StudyRun = case()
    partial: tuple[StudyRun, int] = case()
    fresh: tuple[()] = case()

    @staticmethod
    def Complete(run: StudyRun) -> "ResumePlan":
        return ResumePlan(complete=run)

    @staticmethod
    def Partial(run: StudyRun, done: int) -> "ResumePlan":
        return ResumePlan(partial=(run, done))

    @staticmethod
    def Fresh() -> "ResumePlan":
        return ResumePlan(fresh=())

    @staticmethod
    def of(prior: StudyRun | None, total: int) -> "ResumePlan":
        match prior:
            case StudyRun() as run if len(run.responses) >= total:
                return ResumePlan.Complete(run)
            case StudyRun() as run:
                return ResumePlan.Partial(run, len(run.responses))
            case _:
                return ResumePlan.Fresh()


class RunProjection(Struct, frozen=True):
    name: str
    key: ContentKey
    design_cells: int
    response_width: Option[int]
    indices: dict[str, float]

    @staticmethod
    def of(run: StudyRun) -> "RunProjection":
        return RunProjection(
            name=f"{run.method}:{run.content_key.hex[:8]}",
            key=run.content_key,
            design_cells=run.design_cells,
            response_width=run.response_width,
            indices=run.indices,
        )

    @property
    def width_ratio(self) -> Option[float]:
        return self.response_width.bind(lambda width: Some(width / self.design_cells) if self.design_cells else Nothing)


class Comparison(Struct, frozen=True):
    names: tuple[str, ...]
    members: tuple[ContentKey, ...]
    cohort_key: ContentKey
    cells: dict[str, int]
    widths: dict[str, int]
    ratios: dict[str, float]
    indices: dict[str, dict[str, float]]
    agreement: dict[str, dict[str, float]]

    @property
    def band(self) -> Block[str]:
        return Block.of_seq(f"unmeasured-width:{name}" for name in self.names if name not in self.widths)

    @property
    def content_key(self) -> ContentKey:
        return self.cohort_key

    @property
    def attributes(self) -> dict[str, str | int | float]:
        shared = sorted(frozenset.intersection(*(frozenset(idx) for idx in self.indices.values()))) if self.indices else []
        return {
            "runs": len(self.names),
            "key": self.cohort_key.hex,
            "members": ",".join(member.hex for member in self.members),
            "shared_axes": len(shared),
            "stats": len(self.agreement),
            "band": ";".join(self.band),
        }

    def _noted(self) -> "Comparison":
        trace.get_current_span().set_attributes(self.attributes)
        return self

# --- [OPERATIONS] -----------------------------------------------------------------------


@beartype(conf=FAULT_CONF)
def _compare(by_key: Map[ContentKey, StudyRun], keys: tuple[ContentKey, ...], stats: frozenset[CrossStat]) -> "RuntimeRail[Comparison]":
    found, missing = Block.of_seq(keys).map(lambda k: (k, by_key.try_find(k))).partition(lambda kv: kv[1].is_some())
    if missing:
        raise KeyError(", ".join(k.hex for k, _ in missing))
    rows: Block[RunProjection] = found.choose(lambda kv: kv[1].map(RunProjection.of))
    members = tuple(row.key for row in rows)
    return ContentIdentity.of("study-cohort", members).map(lambda cohort_key: _compared(rows, members, cohort_key, stats))


def _compared(
    rows: "Block[RunProjection]", members: tuple[ContentKey, ...], cohort_key: ContentKey, stats: frozenset[CrossStat]
) -> Comparison:
    return Comparison(
        names=tuple(row.name for row in rows),
        members=members,
        cohort_key=cohort_key,
        cells={row.name: row.design_cells for row in rows},
        widths=dict(rows.choose(lambda row: row.response_width.map(lambda width: (row.name, width)))),
        ratios=dict(rows.choose(lambda row: row.width_ratio.map(lambda ratio: (row.name, ratio)))),
        indices={row.name: row.indices for row in rows},
        agreement={stat.value: stat.agreement(rows) for stat in stats},
    )._noted()


@beartype(conf=FAULT_CONF)
def _resume(
    by_key: Map[ContentKey, StudyRun],
    study: Study,
    objective: Objective,
    design: np.ndarray,
    key: ContentKey,
    seed: int,
    mark: StageTap,
) -> StudyRun:
    prior = by_key.try_find(key).to_optional()
    match ResumePlan.of(prior, len(design)):
        case ResumePlan(tag="complete", complete=done_run):
            return structs.replace(done_run, evaluated_cells=0, elapsed=0.0, speedup=Nothing)
        case ResumePlan(tag="partial", partial=(held, done)):
            tail = objective.rows(design[done:], lambda scored: mark.beat(ResumeStage.EVALUATED, done + scored))
            return _recompute(study, design, np.concatenate([held.responses, tail]), key, seed, len(design) - done, mark)
        case ResumePlan(tag="fresh"):
            fresh = objective.rows(design, lambda scored: mark.beat(ResumeStage.EVALUATED, scored))
            return _recompute(study, design, fresh, key, seed, len(design), mark)
        case _ as unreachable:
            assert_never(unreachable)


def _recompute(study: Study, design: np.ndarray, responses: np.ndarray, key: ContentKey, seed: int, evaluated: int, mark: StageTap) -> StudyRun:
    graded = StudyRun.graded(study, design, Measured(responses, 0.0, Nothing), key, seed, evaluated=Some(evaluated))
    mark.beat(ResumeStage.RECOMPUTED, len(design))
    return graded


def _resume_kernel(
    by_key: Map[ContentKey, StudyRun], study: Study, objective: Objective, seed: int, mark: StageTap
) -> RuntimeRail[StudyRun]:
    def worked() -> RuntimeRail[StudyRun]:
        design = study.method.design(study.axes, seed)
        staged = structs.replace(mark, total=Some(len(design)))
        return study.spec_key(design, Some(objective), seed=seed).map(
            lambda key: _resume(by_key, study, objective, design, key, seed, _planned(staged, len(design)))
        )

    return boundary(HISTORY_RESUME, worked, catch=_RESUME_RAISES).bind(lambda rail: rail)


def _planned(mark: StageTap, cells: int) -> StageTap:
    mark.beat(ResumeStage.PLANNED, cells)
    return mark


# --- [COMPOSITION] ----------------------------------------------------------------------


class RunHistory(Struct, frozen=True):
    runs: tuple[StudyRun, ...]

    @property
    def _by_key(self) -> Map[ContentKey, StudyRun]:
        return Map.of_seq((r.content_key, r) for r in self.runs)

    async def resume(
        self, study: Study, objective: Objective, lane: LanePolicy, /, *, seed: int = 0, composition: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[StudyRun]:
        mark = StageTap.of(EvidenceScope.HISTORY, lane.pulses.tap)

        async def dispatch() -> RuntimeRail[StudyRun]:
            kernel = Kernel.of(_resume_kernel, KernelTrait.HOSTILE)
            return (await lane.offload(kernel, self._by_key, study, objective, seed, mark)).bind(lambda rail: rail)

        facts = {"method": study.method.tag, "runs": len(self.runs), "seed": seed}
        settled = await evidence_run(
            EvidenceScope.HISTORY, "history.resume", dispatch, facts=facts, composition=composition, stage=Some(mark)
        )
        match settled:
            case Result(tag="ok", ok=run):
                return (await Journal.record(run.meter().to_list(), scope=composition)).map(lambda _landed: run)
            case refused:
                return Error(refused.error)

    def compare(
        self,
        *keys: ContentKey,
        stats: frozenset[CrossStat] = frozenset({CrossStat.RANK_CORRELATION}),
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> RuntimeRail[Comparison]:
        return self._traced(HISTORY_COMPARE, _COMPARE_RAISES, lambda: _compare(self._by_key, keys, stats), {"cohort": len(keys), "stats": len(stats)}, composition)

    def _traced[E: Traceable](
        self, row: FaultRow[ComputeLeg], catch: Catch, thunk: "Callable[[], RuntimeRail[E]]", facts: SpanFacts = Map.empty(),
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> RuntimeRail[E]:
        return evidence_run(
            EvidenceScope.HISTORY, f"history.{row.point}", lambda: boundary(row, thunk, catch=catch).bind(lambda value: value),
            facts=facts, composition=composition,
        )
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
