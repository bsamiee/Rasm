# [PY_COMPUTE_HISTORY]

The experiment-run persistence, resume, and comparison rail on the study spine, distinct from the single-run study receipt: `experiments/study.md#STUDY` owns one grid evaluation, `RunHistory` owns the multi-run cohort that persists, resumes, and compares those evaluations on the same param-axis and sample-grid spine, never a parallel experiment tracker. `RunHistory` carries the prior `StudyReceipt` cohort paired with cached response vectors as one `Map[ContentKey, np.ndarray]`, keyed by each design's `ContentKey` so a data or method change keys distinctly and never collides to a stale cache hit. The three resume outcomes are the `ResumePlan` union: `Complete` short-circuits a finished run, `Partial` evaluates only the remaining grid rows over the cached prefix, `Fresh` runs the whole grid once. Because a SALib variance-, moment-, or derivative-based index is undefined over a design tail slice, the `Partial` arm reconstitutes the full response vector — cached prefix `np.concatenate`d with the freshly-evaluated tail through the study owner's `Objective.rows` serial stack — and recomputes the indices over the whole design through the `StudyMethod` union folds and the `StudyReceipt.graded` builder, so a resumed receipt is statistically indistinguishable from an unbroken run.

`RunHistory.compare` reads any cohort of run receipts by content key, `Block.map`s each to one `RunProjection` in a single pass, and folds the cross-run agreement over the shared sensitivity axes through the `CrossStat` kernel table — `RANK_CORRELATION` (Spearman), `RANK_DISTANCE` (Spearman footrule), `KENDALL_TAU`, and `LINEAR_CORRELATION` over the `np.argsort` rank transform — so two runs are read by how much their per-axis sensitivity orderings concur under a selectable statistic family, not by a side-by-side index transpose. The comparison parameterizes both axes: input is the variadic `*keys` cohort, output is the per-`CrossStat` agreement table the caller selects through the `stats` set. Both entrypoints ride one `_traced` weave — a `content.history.{op}` `opentelemetry-api` span over the `boundary` fault fence over a `@beartype(conf=FAULT_CONF)`-guarded thunk, the `@receipted(_REDACTION)` egress aspect harvesting either the resumed `StudyReceipt` or the `ComparisonReceipt` `contribute` stream — exactly the `experiments/model.md#ASSET` multi-entry `_traced` form, so a resume and a comparison emit on the same receipt sink as a fresh study run.

## [01]-[INDEX]

- [01]-[RUN_HISTORY]: content-key-keyed run persistence over a `Map[ContentKey, np.ndarray]` response cache, full-design-recompute partial-row resume over a bounded `ResumePlan` vocabulary composing the `StudyMethod` folds and `StudyReceipt.graded`, cohort comparison with the input-and-output-parameterized `CrossStat`-owned cross-run agreement fold (the statistic family owning its `_rank`/`_correlation`/`_tau` kernels and `_KERNELS` table), and the one shared traced/fault-fenced/`@receipted` `_traced` rail both entrypoints weave on one `RunHistory` owner.

## [02]-[RUN_HISTORY]

- Owner: `RunHistory` — the experiment-run persistence, resume, and comparison rail on the study spine; it carries the prior `StudyReceipt` cohort paired with the cached response vectors and re-runs only the incomplete grid cells. This is the multi-run owner standing beside the single-run `experiments/study.md#STUDY`, not folded into it: the study receipt is the per-grid evidence, `RunHistory` is the cohort that keys, resumes, and compares those receipts. No parallel experiment tracker stands beside it.
- Cases: `ResumePlan` discriminates the three resume outcomes against the prior run on the design key — `Complete(prior)` short-circuits a finished run, `Partial(prior, done, cached)` evaluates only `design[done:]`, `np.concatenate`s the freshly-evaluated tail onto the cached response prefix, and recomputes the sensitivity indices over the whole reconstituted response vector, `Fresh()` runs the whole grid once. The plan is read by `ResumePlan.of(prior, cached, total)`: `done` is the cached vector's ROW count `len(prefix)` against the design height `total`, never the scalar element count `StudyReceipt.response_width` records, while `design[done:]`/`prefix[:done]` address rows; the prior receipt's `design_cells` field is itself the prior run's row count, so the cached vector length and that field measure one axis. Its total `match` over the `(StudyReceipt | None, np.ndarray | None)` pair folds to the case rather than nested `if`s, so a new resume policy is one plan case and one `match` arm, never a new entrypoint.
- Entry: `RunHistory.resume` returns `RuntimeRail[StudyReceipt]` through the one `_traced` weave; it draws the design once through the union-owned `study.method.design(study.axes, seed)`, rails the design key through the railed `ContentIdentity.of` and `bind`s it into `_traced`, reads the `ResumePlan`, evaluates only the remaining design rows through the study owner's `Objective.rows` serial stack, reconstitutes the full response vector, and recomputes the receipt through the `StudyReceipt.graded` builder over a `Measured(responses, 0.0, Nothing)` so the resumed indices are statistically identical to an unbroken run. `RunHistory.compare` returns `RuntimeRail[ComparisonReceipt]` through the same `_traced("compare", ...)` weave: it `Block.map`s any cohort of keyed runs to one `RunProjection` value object each in a single pass through `_compare`, resolves the cohort `Option`-natively through `Block.partition` over `Map.try_find` so the missing keys are named in one `KeyError` the fence folds to a typed fault rather than escaping unclassified, and folds the shared-axis sensitivity vectors to one symmetric cross-run agreement table per selected `CrossStat` over the `np.argsort` rank transform. `_traced` is the one shared rail — the `content.history.{op}` span over the `boundary` fence over the `@beartype(conf=FAULT_CONF)`-guarded thunk, the `@receipted(_REDACTION)` `_emit` aspect harvesting the contributor stream on exit, the `Ok` arm batching the receipt's bounded `span_facts` through `span.set_attributes` behind the `is_recording()` gate and setting `Status(StatusCode.OK)` while the `Error` arm leaves the cause the fence's `_convert` already recorded — exactly the `experiments/model.md#ASSET` multi-entry `_traced` form, not thin indirection since two entrypoints feed it. The single-pair join is the two-key cohort; the statistic family is the `stats` parameter, defaulting to the rank-correlation row.
- Receipt: a resumed run emits the recomputed `StudyReceipt` from `experiments/study.md#STUDY` through the `_traced` `@receipted` aspect, so the resumed receipt's own `contribute` stream lands on the receipt sink rather than being silently dropped; the receipt carries the design `ContentKey` so a later resume keys distinctly when the data or method changes, never colliding to a stale cache hit. `compare` emits a `ComparisonReceipt` carrying the per-run `(design_cells, response_width)` pair, the per-run `width_ratio` arity normalization, the shared-axis index table, and the per-`CrossStat` cross-run agreement matrix, contributing through the canonical two-argument `Receipt.of(owner, evidence)` factory over the `("emitted", subject, facts)` triple on the same `Receipt` sink as the study receipt — the row/width counts ride as native `int` and the ratios and agreement scores as native `float` on the `EventDict` `dict[str, object]` fact map the `Encoder(enc_hook=repr, order="deterministic")` renderer serializes without a `str()` coerce or an `f"{rows}x{width}"` pre-format, not a parallel reporting surface. Both `StudyReceipt` and `ComparisonReceipt` satisfy the `Traceable` port (`contribute` plus the bounded `span_facts` projection), so the one bound-`E` `_emit` aspect harvests either egress and `_traced` writes the receipt's `span_facts` scalars onto the `Ok` span behind the `is_recording()` gate exactly as `study.run` does, never leaving the resume/compare span bare.
- Packages: `numpy` (`concatenate`, `argsort`, `mean`, `std`, `abs`, `asarray`, the `subtract.outer`/`sign` rank-difference pair contracted to the Kendall-tau scalar through `einsum("ij,ij->", su, sv)` — the general-contraction owner the `numpy.md` `RAIL_LAW` mandates over a materialized product-matrix `sum`), `expression` (`tagged_union`/`tag`/`case`, `Ok`/`Error`/`Nothing` the `_traced` rail match, `Block.of_seq`/`Block.map`/`Block.mapi`/`Block.skip`/`Block.choose`/`Block.collect`/`Block.reduce`/`Block.partition`, `Map.of_seq`/`Map.try_find` with the `Option.to_optional`/`Option.map`/`Option.is_some` lowers and the `Block.partition`-over-`try_find` cohort-resolution fold, the `Block.choose`-over-`Option.map` resolved-side projection), `experiments/study.md#STUDY` (`Study`, `StudyReceipt` with its `design_cells`/`response_width`/`indices`/`content_key` fields, `Measured`, `Objective` with its `rows` serial stack, the union-owned `StudyMethod.design`/`discrepancy`/`indices` folds reached through `study.method`, and the `StudyReceipt.graded(study, design, measured, key)` builder), `beartype` (`@beartype(conf=FAULT_CONF)` fencing the `_traced` thunk so a recompute/lookup contract violation folds onto the rail through the `CLASSIFY` `api` row), `opentelemetry-api` (`trace.get_tracer`/`start_as_current_span`/`Span.set_attribute`/`Span.set_attributes`/`is_recording`/`set_status`/`Status`/`StatusCode` the one `content.history.{op}` span over the `history.{op}` `boundary` subject, the `Ok` arm batching `span_facts` through `set_attributes`), runtime (`FAULT_CONF`/`RuntimeRail`/`boundary`, the railed `ContentIdentity.of` with `Result.bind`/`ContentKey`/`IdentityPolicy`, `Receipt`/`Redaction`/`ReceiptContributor`/`receipted` the `@receipted(_REDACTION)` egress aspect over the bound-`E` `Traceable` extension of the `ReceiptContributor` port).
- Growth: a new resume outcome is one `ResumePlan` case and its `match` arm; a new comparison projection is one field on `RunProjection`; a new cross-run statistic is one `CrossStat` member plus one `CrossStat._KERNELS` kernel row over the shared-axis rank/index vectors, reaching `CrossStat.agreement` and the receipt by that single add; a new entrypoint shares the one `_traced` weave by passing its `Traceable`-returning thunk (a receipt with `contribute` plus a bounded `span_facts` projection); zero new emit surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import Final, Literal, Protocol, assert_never, runtime_checkable

import numpy as np
from beartype import beartype
from expression import Error, Nothing, Ok, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from opentelemetry import trace
from opentelemetry.trace import Status, StatusCode

from rasm.compute.experiments.study import Measured, Objective, Study, StudyReceipt
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, Redaction, ReceiptContributor, receipted

# --- [TYPES] ----------------------------------------------------------------------------

# every kernel reads the raw shared-axis index vectors AND their ordinal ranks, so the rank/raw
# split is one signature both the rank-correlation/footrule/tau and the raw-magnitude rows fold over.
type CrossKernel = Callable[[np.ndarray, np.ndarray, np.ndarray, np.ndarray], float]


# the `_traced` egress bound: a receipt both streams its `contribute` facts to the sink AND projects
# the bounded `str | int | float` scalars the `Ok` arm writes onto the active span behind the
# `is_recording()` gate, exactly the `StudyReceipt.span_facts` contract `experiments/study.md#STUDY`
# folds on `Ok`. `StudyReceipt` already satisfies it (it carries both members), and `ComparisonReceipt`
# does too, so the one bound `E` carries either egress and the resume/compare spans are not bare.
@runtime_checkable
class Traceable(ReceiptContributor, Protocol):
    @property
    def span_facts(self) -> dict[str, str | int | float]: ...


class CrossStat(StrEnum):
    RANK_CORRELATION = "rank_correlation"  # Spearman rho: Pearson over the no-ties argsort ranks
    RANK_DISTANCE = "rank_distance"  # Spearman footrule: 1 - normalized L1 rank displacement
    KENDALL_TAU = "kendall_tau"  # concordant-minus-discordant pair fraction over the ranks
    LINEAR_CORRELATION = "linear_correlation"  # Pearson over the raw shared-axis index magnitudes

    # the statistic family OWNS its kernels exactly as `experiments/study.md#STUDY`'s `StudyMethod`
    # owns `_qmc`/`_salib`/`_axis_r2`: the rank transform, the two closed-form kernels, and the
    # `_KERNELS` dispatch table are static members of this owner, never module-level free functions
    # the enum reaches back down into. `score` reads `_KERNELS[self]` over the raw-and-rank pair.
    def score(self, u_idx: np.ndarray, v_idx: np.ndarray) -> float:
        u_rank, v_rank = CrossStat._rank(u_idx), CrossStat._rank(v_idx)
        return CrossStat._KERNELS[self](u_idx, v_idx, u_rank, v_rank)

    # the upper-triangular cohort fold is the statistic's own concern, not a single-caller free
    # function: every shared-axis run pair scores through `self.score` and lands in one agreement map.
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
        # concordant-minus-discordant over all unordered pairs as the sign-product contraction of the
        # rank outer-differences. `einsum("ij,ij->", su, sv)` contracts the two sign matrices to the
        # scalar whole-matrix sum without materializing their product (the `numpy.md` `RAIL_LAW`
        # general-contraction owner); symmetric with a zero diagonal under no-ties ranks, so the
        # contraction `/ 2` is the upper-triangular pair sum with no explicit `triu` extract.
        su = np.sign(np.subtract.outer(u_rank, u_rank))
        sv = np.sign(np.subtract.outer(v_rank, v_rank))
        n = len(u_rank)
        return float(np.einsum("ij,ij->", su, sv)) / 2.0 / max(n * (n - 1) // 2, 1)

    # the four statistics collapse to one dispatch table reading the raw `(u, v)` pair AND the ordinal
    # `(ur, vr)` ranks: the rank-correlation/footrule/tau rows fold over the ranks, the linear row over
    # the raw magnitudes, so a new statistic is one enum member plus one row, never a per-stat method.
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
        # the resume index is the cached vector's ROW count `len(prefix)` against the design height
        # `total`; `design[done:]`/`prefix[:done]` address rows, never the scalar element count
        # `StudyReceipt.response_width` carries. `design_cells` on the prior receipt is already the
        # prior run's row count, so the cached vector length and that field measure the same axis.
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
        # the `StudyReceipt` owner carries `design_cells`/`response_width` (NOT `cells_completed`/
        # `cells_total`); this projection reads those canonical fields rather than a stale shape.
        return RunProjection(
            name=f"{receipt.method}:{receipt.content_key.hex[:8]}",
            design_cells=receipt.design_cells,
            response_width=receipt.response_width,
            indices=receipt.indices,
        )

    @property
    def width_ratio(self) -> float:
        # per-cell output arity normalized to the row count — a scalar-objective run reads near zero,
        # a wide multi-output run reads high, a parameterized fact rather than a completion fraction
        # over a `cells_completed > cells_total` contradiction the renamed receipt no longer admits.
        return self.response_width / self.design_cells if self.design_cells else 0.0


class ComparisonReceipt(Struct, frozen=True):
    names: tuple[str, ...]
    cells: dict[str, tuple[int, int]]  # name -> (design_cells, response_width)
    ratios: dict[str, float]  # name -> RunProjection.width_ratio, the per-cell arity normalization
    indices: dict[str, dict[str, float]]
    agreement: dict[str, dict[str, float]]  # stat -> {pair -> score}, the per-CrossStat matrix

    @property
    def span_facts(self) -> dict[str, str | int | float]:
        # the bounded-scalar source the `content.history.compare` span reads on `Ok` — exactly the
        # `str | int | float` set `Span.set_attributes` admits, mirroring `StudyReceipt.span_facts`; the
        # full per-pair agreement matrix and per-run index ledger ride the receipt facts only.
        shared = sorted(frozenset.intersection(*(frozenset(idx) for idx in self.indices.values()))) if self.indices else []
        return {"runs": len(self.names), "shared_axes": len(shared), "stats": len(self.agreement)}

    def contribute(self) -> Iterable[Receipt]:
        # the `ReceiptContributor` port yields an `Iterable[Receipt]`; one `emitted` receipt streams the
        # cohort row/width/arity-ratio counts as native ints and floats and the per-`CrossStat` pairwise
        # agreement scores as native floats the `EventDict` `dict[str, object]` carries without a `str()`/
        # `repr` coerce — no `f"{rows}x{width}"` pre-format where the `enc_hook=repr` renderer keeps types.
        facts: dict[str, object] = {
            **{f"rows[{k}]": rows for k, (rows, _) in self.cells.items()},
            **{f"width[{k}]": width for k, (_, width) in self.cells.items()},
            **{f"ratio[{k}]": ratio for k, ratio in self.ratios.items()},
            **{f"agree[{stat}:{pair}]": score for stat, m in self.agreement.items() for pair, score in m.items()},
        }
        return (Receipt.of("compute.history", ("emitted", ",".join(self.names), facts)),)


# --- [SERVICES] -------------------------------------------------------------------------

_TRACER: Final[trace.Tracer] = trace.get_tracer("compute.history")
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())  # run-history facts carry no secret field

# --- [OPERATIONS] -----------------------------------------------------------------------


@beartype(conf=FAULT_CONF)
def _compare(by_key: Map[ContentKey, StudyReceipt], keys: tuple[ContentKey, ...], stats: frozenset[CrossStat]) -> ComparisonReceipt:
    # the cohort lookup stays `Option`-native through `Block.partition` over `try_find`: the resolved
    # receipts and the unresolved keys split in one pass, so a missing cohort key names every absent
    # hex in one `KeyError` the `content.history.compare` fence folds to a typed fault rather than a
    # first-miss raise smuggled through a `default_with` thunk. Inside the already-fenced body this raise
    # is the one boundary-to-rail conversion the `CLASSIFY` `boundary` row owns. The resolved side lowers
    # through `Block.choose` (the filter-map-over-`Option` combinator) projected by `RunProjection.of`,
    # so the cohort never reads a raw `Option.value` outside the documented lowering surface.
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
    # full-design recompute through the study owner's `graded` over a `Measured(responses, 0.0, Nothing)`:
    # the resumed receipt is statistically identical to an unbroken run; `graded` re-derives the indices
    # and discrepancy through the `StudyMethod` union folds, so this owner re-declares no design algebra.
    # elapsed is zero and speedup absent because the timing belongs to the original evaluation.
    return StudyReceipt.graded(study, design, Measured(responses, 0.0, Nothing), key)


# --- [COMPOSITION] ----------------------------------------------------------------------


class RunHistory(Struct, frozen=True):
    runs: tuple[StudyReceipt, ...]
    responses: Map[ContentKey, np.ndarray] = Map.empty()  # content-keyed cache; a run absent here falls to `Fresh`

    @property
    def _by_key(self) -> Map[ContentKey, StudyReceipt]:
        return Map.of_seq((r.content_key, r) for r in self.runs)

    def resume(self, study: Study, objective: Objective, /, *, seed: int = 0) -> RuntimeRail[StudyReceipt]:
        # the resumed receipt rides the same traced/fault-fenced/`@receipted` weave the study run does:
        # `ContentIdentity.of` rails the design key, and `bind`ing it into the `_traced` rail keeps a
        # content-identity encode fault and a resume fault on one rail under one `content.history.resume` span.
        # `responses` is `ContentKey`-keyed at construction (the `_by_key` discipline), so the cache reads
        # by the same key `_resume` looks the prior receipt up by — never a positional `zip` that mis-pairs
        # a non-prefix response subset against `runs` or silently truncates under `strict=False`.
        design = study.method.design(study.axes, seed)
        return ContentIdentity.of("study", design.tobytes(), IdentityPolicy()).bind(
            lambda key: self._traced("resume", lambda: _resume(self._by_key, self.responses, study, objective, design, key))
        )

    def compare(self, *keys: ContentKey, stats: frozenset[CrossStat] = frozenset({CrossStat.RANK_CORRELATION})) -> RuntimeRail[ComparisonReceipt]:
        # variadic over the run cohort; the single-pair join is the two-key cohort and the statistic
        # family is the `stats` parameter, parameterizing the comparison on input shape AND output table.
        return self._traced("compare", lambda: _compare(self._by_key, keys, stats))

    def _traced[E: Traceable](self, op: str, thunk: Callable[[], E]) -> RuntimeRail[E]:
        # one woven rail both entrypoints share — exactly the `experiments/model.md#ASSET` multi-entry
        # `_traced` form, not thin indirection: the `boundary` fence wraps the thunk inside one
        # `content.history.{op}` span (the `content.` prefix the sibling compute spans carry), and the
        # thunk calls a `@beartype(conf=FAULT_CONF)`-guarded body (`_resume`/`_compare`, decorated exactly
        # as the sibling `study._execute`/`model._load_and_run`), so a recompute/lookup contract violation
        # folds through the `CLASSIFY` `api` row and a missing-cohort `KeyError` through the `boundary` row
        # rather than escaping; the `Ok` arm batches the receipt's bounded `span_facts` behind the
        # `is_recording()` gate and sets a total `Status` exactly as `study.run` does, while the `Error` arm
        # needs no body because the fence's `_convert` already recorded the cause and set `StatusCode.ERROR`
        # on the active span. Both receipts satisfy `Traceable`, so the one bound `E` carries either egress.
        with _TRACER.start_as_current_span(f"content.history.{op}") as span:
            if span.is_recording():
                span.set_attribute("history.op", op)
            rail = boundary(f"history.{op}", lambda: RunHistory._emit(thunk()))
            match rail:
                case Ok(evidence):
                    if span.is_recording():
                        span.set_attributes(evidence.span_facts)
                    span.set_status(Status(StatusCode.OK))
                case Error(_):
                    pass
            return rail

    @staticmethod
    @receipted(_REDACTION)
    def _emit[E: Traceable](evidence: E) -> E:
        return evidence
```

`ResumePlan.of` matches the prior receipt against its cached prefix into one of three outcomes — keying `done` on `len(prefix)` against the design height `total`, never the scalar `response_width` element count — and `_resume` folds each outcome through a total `match` closed by `assert_never`. The `partial` arm evaluates only `design[done:]` through the study owner's `Objective.rows` serial stack, `np.concatenate`s the tail onto the cached prefix, and recomputes the indices over the whole reconstituted vector (never the tail slice, where a SALib variance-, moment-, or derivative-based index is undefined); both recompute arms land on `_recompute`, which composes `StudyReceipt.graded(study, design, Measured(responses, 0.0, Nothing), key)` so `graded` re-derives the indices and discrepancy through the `StudyMethod` union folds and this owner re-declares no design algebra, `elapsed=0.0`/`speedup=Nothing` because a resume inherits no fresh timing.

`CrossStat.agreement` intersects the runs' sensitivity-axis names through a `Block.reduce(frozenset.intersection)` fold, `CrossStat._rank`-transforms each run's shared-axis index vector through the double-`np.argsort` idiom, and folds the upper-triangular run pairs through `mapi`/`skip`/`collect` to each pair's `self.score` through the owner-internal `CrossStat._KERNELS` kernel table: `RANK_CORRELATION` (default), `RANK_DISTANCE` over the L1 displacement, `KENDALL_TAU` over the `np.subtract.outer`/`np.sign` pair contracted by `np.einsum("ij,ij->", su, sv)`, and `LINEAR_CORRELATION` over the raw magnitudes — the statistic family owning its kernels exactly as `StudyMethod` owns `_qmc`/`_salib`, all runtime numpy with no gated dependency. `ComparisonReceipt.contribute` and the resumed `StudyReceipt` both stream through the canonical two-argument `Receipt.of` factory the `_emit` aspect harvests, the native `float` scores riding the `EventDict` `dict[str, object]` without a `str()`/`repr` coerce.

## [03]-[RESEARCH]

- [RESUME_DESIGN_SLICE]: the partial-resume slice assumes `study.method.design(axes, seed)` is row-stable across runs at a fixed seed, so `design[done:]` addresses exactly the rows the prior run left unevaluated and the cached `prefix[:done]` addresses exactly the rows it completed. The resume index `done` is `len(prefix)` — the cached response vector's row count — capped at the design height, never the scalar `StudyReceipt.response_width` field, which `graded` records as `int(measured.responses.size // rows)`, the per-cell output arity rather than a row index; the prior receipt's `design_cells` field is itself `int(design.shape[0])`, the prior run's row count, so it and the cached vector length measure one axis and a resume could read either. The `ContentKey`-keyed cache tightens this: `cache.try_find(key)` hits only when the stored prefix was produced under the SAME design bytes the current `key` folds, so a `Partial` prefix is provably a prefix of the identical design rather than a positionally-zipped vector from a different run. This row-stability holds for the numpy factorial floor and the scipy `qmc` rows (seeded `LatinHypercube`/`Sobol`/`Halton` under the SPEC-007 `rng` keyword), and the SALib structured designs (seeded). The reconstituted `np.concatenate([prefix, tail])` is therefore the same response vector a single unbroken run produces, and `_recompute` runs `StudyMethod.indices` once over the full design — the only statistically valid recompute, since SALib's Saltelli/Morris/FAST estimators read the whole structured design. `resume` draws `study.method.design(study.axes, seed)` once and threads it into both the railed `ContentIdentity.of` key derivation and `_resume`, never re-drawing a QMC/SALib design twice on one resume. The tail is evaluated through the study owner's `Objective.rows` `np.stack` serial fold rather than a re-declared per-row evaluator, so the resume reuses the one batch-or-serial discipline `MeasurementMode.evaluate` holds. `RunHistory.responses` is the `Map[ContentKey, np.ndarray]` cache keyed by each prior run's design `ContentKey` at construction (the `_by_key` discipline), so `_resume` reads it by the same key it looks the prior receipt up by rather than a positional `zip` against `runs` that mis-pairs a non-prefix response subset or truncates under `strict=False`; a run absent from the cache cannot resume partially and falls to `Fresh`. The `StudyMethod.design`/`discrepancy`/`indices` union folds, the `StudyReceipt.graded(study, design, measured, key)` builder, and the `StudyReceipt` `method`/`mode`/`design_cells`/`response_width`/`indices`/`discrepancy`/`elapsed`/`speedup`/`content_key` fields are owned by `experiments/study.md#STUDY` and composed here, never re-declared — `_recompute` wrapping the responses in a `Measured(responses, 0.0, Nothing)` and calling `graded` rather than constructing `StudyReceipt` positionally, so the resumed receipt carries the full nine-field shape with `discrepancy` and `speedup` re-derived through the union rather than a positional constructor that mismatches the struct.
- [CONTENT_KEY_PROJECTION]: `RunProjection.of` reads the `ContentKey.hex` render (`{value:032x}:{fmt}`, the `project("hex")`-delegating property owned by `runtime/evidence/identity.md#IDENTITY`) for the comparison row key, slicing its leading characters for a compact axis label and folding the receipt's `design_cells`/`response_width` row-and-arity counts and `indices` into one frozen value object so the cohort projects in one `Block.map` pass rather than parallel per-field dict comprehensions over the raw receipts. The projection reads the canonical `StudyReceipt.design_cells`/`response_width` fields the study owner declares — never the phantom `cells_completed`/`cells_total` an older receipt shape carried, which would raise `AttributeError` against the live struct — and `RunProjection.width_ratio` derives the per-cell arity normalization the `_compare` projection folds into the `ComparisonReceipt.ratios` map and the receipt facts, rather than persisting a stale completion fraction over a `cells_completed > cells_total` contradiction the renamed receipt cannot express. `RunProjection` carries the `indices` container field, so it stays GC-tracked exactly as `StudyReceipt` does and never takes the `gc=False` opt-out the container-free `ParamAxis`/`SalibRoute`/`Measured` leaves carry. `ContentIdentity.of`/`ContentKey`/`IdentityPolicy` are composed exactly as `experiments/study.md#STUDY` keys its design, never re-declared; `ContentIdentity.of` returns `RuntimeRail[ContentKey]` and takes the `CANONICAL_POLICY` default, so the explicit `IdentityPolicy()` argument here keys identically to the study owner's design key at a fixed format and policy, and `resume` `bind`s the railed key into `_traced` rather than treating `of` as a bare-`ContentKey` call.
