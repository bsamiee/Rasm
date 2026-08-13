# [PY_COMPUTE_HISTORY]

Experiment-run persistence, resume, and comparison rail on the study spine: `experiments/study#STUDY` owns one grid evaluation, `RunHistory` owns the multi-run cohort that persists, resumes, and compares those evaluations, never a parallel experiment tracker. `Partial` resume evaluates only the remaining grid rows yet recomputes the sensitivity indices over the whole reconstituted response vector — SALib variance-, moment-, and derivative-based indices are undefined over a design tail slice — so a resumed receipt is statistically indistinguishable from an unbroken run. Run-scoped census stays the resume's own — zero elapsed, absent speedup, `evaluated_cells` counting only the rows admitted fresh — the slot the `Resource.RECORD` settlement prices, so a resume never re-bills or re-benches the cached prefix the original run already charged. Compute owns no durable run store: the resume proof is key equality over caller-supplied evidence, never storage.

Response caching is one `Map[ContentKey, np.ndarray]` keyed by `Study.spec_key` — axes, method, mode, the objective's full identity (row/batch scorer shipping identity and the jit route row), the sampler-and-analyzer seed, and design bytes in one preimage — so a data, method, mode, scorer, seed, or jit/batch configuration change keys distinctly and never collides to a stale hit. Seed threading spans the WHOLE resume chain for that reason: the seed draws the design, folds into the key preimage, and drives the analyzer, so keying without it makes every non-zero-seed resume miss by construction and the plan answers `Fresh` forever. `resume` evaluates its remaining rows through the same `HOSTILE`-trait `Kernel` crossing `experiments/study#STUDY` `Study.run` rides — the module-level `_resume_kernel` ships `REFERENCE`, a closure-bearing objective crosses on the pool's cloudpickle wire — while `compare` stays the sync `_traced` weave; both run under the `EvidenceScope.HISTORY` span with the `boundary` fence over beartype-guarded bodies, both thread the caller's composition `ScopeKey` onto that weave, and receipts harvest through the weave's fenced emit at the `runtime/observability/receipts#RECEIPT` owner. `scipy.stats` supplies the rank-correlation family the cohort comparison reads.

## [01]-[INDEX]

- [02]-[RUN_HISTORY]: content-keyed run persistence, `ResumePlan`-discriminated resume, and `CrossStat` cohort comparison on one `RunHistory` owner.

## [02]-[RUN_HISTORY]

- Owner: `RunHistory` — the study receipt is the per-grid evidence, `RunHistory` the cohort that keys, resumes, and compares those receipts.
- Cases: `ResumePlan` discriminates `Complete`/`Partial`/`Fresh` against the prior run through one total `match`, so a new resume policy is one plan case and one `match` arm, never a new entrypoint.
- Law: the async `resume` fold settles one `Resource.RECORD` `MeterFact` off the cleared receipt's `meter` projection — the fresh-admission census `evaluated_cells` times response arity — so a resume charges only the rows it evaluated: `Partial` bills the design tail, `Fresh` the whole grid, and `Complete` re-stamps the cached receipt's run-scoped census to zero fresh cells, zero elapsed, and absent speedup, charging nothing and re-emitting no bench series the original run already contributed. Degenerate comparison operands — constant index columns — have no defined correlation, so the pair drops from the agreement map rather than being scored a fabricated perfect agreement or carrying a `nan` into the receipt facts and the span attributes; `score` returns `Option[float]` and the pair fold `choose`s over it, so absence is structural rather than a sentinel a reader must learn to disbelieve.
- Output: `CrossStat` parameterizes the comparison on both axes — the variadic `*keys` cohort in, the per-statistic agreement table out — reading run concurrence as per-axis sensitivity-ordering agreement, never a side-by-side index transpose. Its kernels ARE the `scipy.stats` estimators: `spearmanr`, `kendalltau`, and `pearsonr` each answer their `.statistic` with the tie correction a local double-`argsort` transform silently drops, and `kendalltau`'s merge is O(n log n) where a sign-matrix contraction materializes two O(n²) operands per pair. Only the footrule distance has no scipy estimator, so it alone composes `rankdata` — one row, never a rank transform standing beside the provider's.
- Growth: a new resume outcome is one `ResumePlan` case and its `match` arm; a new comparison projection is one `RunProjection` field; a new cross-run statistic is one `CrossStat` member and one `_KERNELS` row naming its estimator; a new sync entrypoint shares the `_traced` weave by passing its `Traceable`-returning thunk, and an evaluating entrypoint crosses on the study kernel's lane.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from math import isfinite
from typing import Final, Literal, Protocol, assert_never, runtime_checkable

import numpy as np
from beartype import beartype
from expression import Error, Nothing, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.compute.experiments.study import Measured, Objective, Study, StudyReceipt
from rasm.compute.graduation.handoff import EvidenceScope, SpanFacts, evidence_run
from rasm.runtime.identity import ContentKey
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.journal import Journal
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Receipt, ReceiptContributor, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy from scipy import stats

# --- [TYPES] ----------------------------------------------------------------------------

# every kernel reads the two shared-axis index vectors and nothing else: scipy's rank correlations take raw values and
# rank internally with the tie correction, so a rank pair threaded beside them was a second transform the provider owns.
type CrossKernel = Callable[[np.ndarray, np.ndarray], float]


# `_traced` egress bound: a receipt streams its `contribute` facts AND projects the bounded scalars the `Ok` arm writes
# onto the span; `StudyReceipt` and `ComparisonReceipt` both satisfy it, so one bound `E` carries either egress.
@runtime_checkable
class Traceable(ReceiptContributor, Protocol):
    @property
    def span_facts(self) -> dict[str, str | int | float]: ...


class CrossStat(StrEnum):
    RANK_CORRELATION = "rank_correlation"  # Spearman rho over tie-corrected ranks
    RANK_DISTANCE = "rank_distance"  # Spearman footrule: 1 - normalized L1 rank displacement
    KENDALL_TAU = "kendall_tau"  # tau-b concordant-minus-discordant fraction, tie-corrected
    LINEAR_CORRELATION = "linear_correlation"  # Pearson over the raw shared-axis index magnitudes

    # statistic family OWNS its dispatch table; the kernels themselves are the scipy estimators, never a local
    # re-derivation of a correlation the admitted package already ships at better complexity and with tie handling.
    def score(self, u_idx: np.ndarray, v_idx: np.ndarray) -> Option[float]:
        # a degenerate operand — a constant index column, where every run ranks every axis identically — has no
        # defined correlation and every estimator answers `nan` there, so the pair is ABSENT from the agreement map
        # rather than scored a fabricated perfect `1.0` or leaked as a `nan` into the receipt facts and the span.
        scored = CrossStat._KERNELS[self](u_idx, v_idx)
        return Some(scored) if isfinite(scored) else Nothing

    # every shared-axis run pair scores through `self.score` and the defined scores land in one agreement map.
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

    # a new statistic is one enum member plus one row, never a per-stat method.
    _KERNELS: "Map[CrossStat, CrossKernel]"


def _footrule(u: np.ndarray, v: np.ndarray) -> float:
    # Spearman footrule — normalized L1 rank displacement — is the one row scipy ships no estimator for, so it
    # composes `rankdata` (average-rank tie correction, matching what `spearmanr` ranks with internally) rather than
    # standing up a second rank transform beside the provider's.
    u_rank, v_rank = stats.rankdata(u), stats.rankdata(v)
    return 1.0 - float(np.abs(u_rank - v_rank).sum()) / max(len(u_rank) ** 2 // 2, 1)


# `spearmanr`/`kendalltau`/`pearsonr` each return a result object whose `.statistic` is the coefficient; kendall's
# default `variant="b"` is the tie-corrected form, and its O(n log n) merge beats the O(n²) sign-matrix contraction a
# local kernel would materialize twice per pair.
CrossStat._KERNELS = Map.of_seq([
    (CrossStat.RANK_CORRELATION, lambda u, v: float(stats.spearmanr(u, v).statistic)),
    (CrossStat.RANK_DISTANCE, _footrule),
    (CrossStat.KENDALL_TAU, lambda u, v: float(stats.kendalltau(u, v).statistic)),
    (CrossStat.LINEAR_CORRELATION, lambda u, v: float(stats.pearsonr(u, v).statistic)),
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
    design_cells: int  # whole-grid census in design rows, read off the prior `StudyReceipt.design_cells`
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
    by_key: Map[ContentKey, StudyReceipt],
    cache: Map[ContentKey, np.ndarray],
    study: Study,
    objective: Objective,
    design: np.ndarray,
    key: ContentKey,
    seed: int,
) -> StudyReceipt:
    prior = by_key.try_find(key).to_optional()
    cached = cache.try_find(key).to_optional()
    match ResumePlan.of(prior, cached, len(design)):
        case ResumePlan(tag="complete", complete=done_run):
            # cached content evidence stands whole while the run-scoped census re-stamps to THIS run's — zero fresh
            # admissions, zero elapsed, absent speedup — so the settlement charges nothing and the bench projection
            # re-emits no duration series the original run already contributed.
            return structs.replace(done_run, evaluated_cells=0, elapsed=0.0, speedup=Nothing)
        case ResumePlan(tag="partial", partial=(_, done, prefix)):
            # only the rows the prior run left undone ride the study owner's `Objective.rows` serial
            # stack; `concatenate` reconstitutes the full vector a single unbroken run would produce.
            return _recompute(study, design, np.concatenate([prefix, objective.rows(design[done:])]), key, seed, len(design) - done)
        case ResumePlan(tag="fresh"):
            return _recompute(study, design, objective.rows(design), key, seed, len(design))
        case _ as unreachable:
            assert_never(unreachable)


def _recompute(study: Study, design: np.ndarray, responses: np.ndarray, key: ContentKey, seed: int, evaluated: int) -> StudyReceipt:
    # `graded` re-derives indices and discrepancy through the `StudyMethod` union folds, so this owner re-declares no design
    # algebra; the SALib analyzers read the same seed the sampler drew under, so a resumed receipt's indices reproduce the
    # unbroken run's exactly. Elapsed is zero and speedup absent because the timing belongs to the original evaluation;
    # `evaluated` is the fresh-row census the RECORD settlement prices — the design tail on Partial, the whole grid on Fresh.
    return StudyReceipt.graded(study, design, Measured(responses, 0.0, Nothing), key, seed, evaluated=Some(evaluated))


def _resume_kernel(
    by_key: Map[ContentKey, StudyReceipt], cache: Map[ContentKey, np.ndarray], study: Study, objective: Objective, seed: int
) -> RuntimeRail[StudyReceipt]:
    # module-level so REFERENCE shipping resolves it by import — the crossing law `study._study_kernel` holds; the fence
    # converts a design/scorer/analyzer raise, and a closure-bearing objective crosses on the pool's cloudpickle wire.
    # design generation and the key mint run worker-side: `method.design` is CPU work an awaiting caller must never host,
    # and an encode refusal rails through the same fence as every other worker fault. Seed threading spans the WHOLE
    # chain — design draw, key preimage, and analyzer — because `spec_key` folds it: keying without it makes the
    # recomputed key differ from the original run's for every non-zero seed, so `ResumePlan.of` answers `Fresh` forever
    # and the response cache this owner exists to serve can never hit.
    def worked() -> RuntimeRail[StudyReceipt]:
        design = study.method.design(study.axes, seed)
        return study.spec_key(design, Some(objective), seed=seed).map(lambda key: _resume(by_key, cache, study, objective, design, key, seed))

    return boundary("history.resume", worked).bind(lambda rail: rail)


# --- [COMPOSITION] ----------------------------------------------------------------------


class RunHistory(Struct, frozen=True):
    runs: tuple[StudyReceipt, ...]
    responses: Map[ContentKey, np.ndarray] = Map.empty()  # content-keyed cache; a run absent here falls to `Fresh`

    @property
    def _by_key(self) -> Map[ContentKey, StudyReceipt]:
        return Map.of_seq((r.content_key, r) for r in self.runs)

    async def resume(
        self, study: Study, objective: Objective, lane: LanePolicy, /, *, seed: int = 0, composition: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[StudyReceipt]:
        # remaining-row evaluation is the same HOSTILE crossing `Study.run` rides — `objective.rows` AND `method.design`
        # on the loop would stall it, so the kernel derives design and key worker-side through `Study.spec_key`, the ONE
        # mint both owners share — the axes-method-mode spec, the objective's full identity, the seed, and the design
        # bytes — so the key equals the original
        # run's by construction and the response cache hits — and `_recompute` grades zero elapsed, so a worker-death
        # re-run reproduces the receipt and the retry default stands.
        async def dispatch() -> RuntimeRail[StudyReceipt]:
            kernel = Kernel.of(_resume_kernel, KernelTrait.HOSTILE)
            return (await lane.offload(kernel, self._by_key, self.responses, study, objective, seed)).bind(lambda rail: rail)

        facts = {"method": study.method.tag, "runs": len(self.runs), "seed": seed}
        settled = await evidence_run(EvidenceScope.HISTORY, "history.resume", dispatch, facts=facts, composition=composition)
        # this resume fold is the nearest async owner of its own fresh-admission census — the worker kernel binds no
        # plane — so the RECORD settlement lands here off the cleared receipt's `meter` projection, and a wholly-cached
        # resume (zero fresh cells) charges nothing rather than re-billing the prefix the original run already paid.
        # Each resume call is its own operation: a re-run re-evaluates its rows and the charge prices that performed
        # work, so replay dedup keys on operation identity at the causal-log owner — a content-keyed settlement here
        # would read two distinct evaluations over equal payloads as one.
        match settled:
            case Result(tag="ok", ok=receipt):
                return (await Journal.record(receipt.meter().to_list(), scope=composition)).map(lambda _landed: receipt)
            case refused:
                return Error(refused.error)

    def compare(
        self,
        *keys: ContentKey,
        stats: frozenset[CrossStat] = frozenset({CrossStat.RANK_CORRELATION}),
        composition: ScopeKey = DEFAULT_SCOPE,
    ) -> RuntimeRail[ComparisonReceipt]:
        # single-pair join is the two-key cohort; the statistic family is the `stats` parameter.
        return self._traced("compare", lambda: _compare(self._by_key, keys, stats), {"cohort": len(keys), "stats": len(stats)}, composition)

    def _traced[E: Traceable](
        self, op: str, thunk: Callable[[], E], facts: SpanFacts = Map.empty(), composition: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeRail[E]:
        # sync weave — span, fence over the beartype-guarded body, fenced receipt harvest — so a contract violation folds
        # through the `CLASSIFY` `api` row and a missing-cohort `KeyError` through the `boundary` row; the caller's
        # composition key threads onto the weave so an embedded composition's facts key to it.
        return evidence_run(EvidenceScope.HISTORY, f"history.{op}", lambda: boundary(f"history.{op}", thunk), facts=facts, composition=composition)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
