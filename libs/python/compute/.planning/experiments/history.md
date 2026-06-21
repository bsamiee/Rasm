# [PY_COMPUTE_HISTORY]

The experiment-run persistence, resume, and comparison rail on the study spine. `RunHistory` carries the prior study receipts paired with their cached response vectors and re-runs only the incomplete grid cells of a study keyed by its design `ContentKey`, so a complete prior run short-circuits, an incomplete prior run evaluates only its remaining cells over the cached prefix, and a fresh design runs the whole grid once. Because a SALib variance-, moment-, or derivative-based index is meaningless over a design tail slice, the partial arm reconstitutes the full response vector — cached prefix concatenated with the freshly-evaluated tail — and recomputes the sensitivity indices over the whole design, so a resumed receipt carries indices indistinguishable from an unbroken run. The comparison join reads any cohort of run receipts by content key and folds each to one projected row, then derives the cross-run axis-rank agreement matrix over the shared sensitivity axes — a normalized Spearman-footprint computed from `np.argsort` rank transforms — so two runs are read by how much their per-axis sensitivity orderings concur, not merely by a side-by-side index transpose. This rail rides the same param-axis and sample-grid spine as the study owner rather than a parallel experiment tracker, and is a distinct owner from the single-run study receipt: `experiments/study.md#STUDY` owns one grid evaluation, `RunHistory` owns the multi-run cohort that persists, resumes, and compares those evaluations. The codemap fixes `history.py` as that owner's source file; the resume keys distinctly on the design `ContentKey` so a data or method change never collides to a stale cache hit.

## [01]-[INDEX]

- [01]-[RUN_HISTORY]: content-key-keyed run persistence with cached response vectors, full-design-recompute partial-cell resume over a bounded `ResumePlan` vocabulary, and cohort comparison with cross-run rank-agreement on one `RunHistory` owner.

## [02]-[RUN_HISTORY]

- Owner: `RunHistory` — the experiment-run persistence, resume, and comparison rail on the study spine; it carries the prior `StudyReceipt` cohort paired with the cached response vectors and re-runs only the incomplete grid cells. This is the multi-run owner standing beside the single-run `experiments/study.md#STUDY`, not folded into it: the study receipt is the per-grid evidence, `RunHistory` is the cohort that keys, resumes, and compares those receipts. No parallel experiment tracker stands beside it.
- Cases: `ResumePlan` discriminates the three resume outcomes against the prior run on the design key — `Complete(prior)` short-circuits a finished run, `Partial(prior, done, cached)` evaluates only `design[done:]`, concatenates the freshly-evaluated tail onto the cached response prefix, and recomputes the sensitivity indices over the whole reconstituted response vector, `Fresh()` runs the whole grid once. The plan is read from the prior receipt's `cells_completed`/`cells_total` and its cached responses by `ResumePlan.of`, so a new resume policy is one plan case and one `match` arm, never a new entrypoint.
- Entry: `RunHistory.resume` returns `RuntimeRail[StudyReceipt]` through one `boundary`; it rebuilds the design from the study seed, keys the prior run, reads the `ResumePlan`, evaluates only the remaining design rows, reconstitutes the full response vector, and recomputes the receipt over the whole design so the resumed indices are statistically identical to an unbroken run. `RunHistory.compare` folds any cohort of keyed runs to one `ComparisonReceipt`: each run projects to a `(name, cells, completion, indices)` row in a single pass, and the shared-axis sensitivity vectors fold to a symmetric cross-run agreement matrix via `np.argsort` rank transforms. The single-pair join is the two-key cohort.
- Receipt: a resumed run emits the recomputed `StudyReceipt` from `experiments/study.md#STUDY`; the receipt carries the design `ContentKey` so a later resume keys distinctly when the data or method changes, never colliding to a stale cache hit. `compare` emits a `ComparisonReceipt` carrying the per-run cell counts, completion fractions, the shared-axis index table, and the cross-run rank-agreement matrix, and contributes through the same `Receipt` sink as the study receipt — the agreement scores ride the fact map, not a parallel reporting surface.
- Packages: `numpy` (`stack`, `concatenate`, `argsort`, `mean`, `std`), `experiments/study.md#STUDY` (`Study`, `StudyReceipt`, `Objective`, the `_design`/`_indices` build), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`).
- Growth: a new resume outcome is one `ResumePlan` case and its `match` arm; a new comparison projection is one field on the projected row and one fold column; a new cross-run statistic is one kernel over the shared-axis vectors; zero new entrypoint.
- Boundary: no job framework, scheduler, or production run store; offline run history only. A parallel experiment-tracker owner, a resume that re-runs completed cells, a partial resume that computes SALib indices over a tail slice, and an absorption of the cohort into the single-run study receipt are the deleted forms. The persistence/cohort spine — the `ResumePlan` vocabulary, the cached-response concatenation, and the `compare`/`_agreement`/`_spearman` rank-agreement fold — is pure cp315-core numpy; the `resume` recompute inherits the study owner's `python_version<'3.15'` gating exactly where `_design`/`_indices` route through the scipy `qmc` engine family and the SALib analyzers, and lands on the cp315 numpy factorial/polynomial floors otherwise. This owner takes no gated dependency of its own.

```python signature
from __future__ import annotations

from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.compute.experiments.study import Objective, Study, StudyReceipt, _design, _indices
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


@tagged_union(frozen=True)
class ResumePlan:
    tag: Literal["complete", "partial", "fresh"] = tag()
    complete: StudyReceipt = case()
    partial: tuple[StudyReceipt, int, np.ndarray] = case()
    fresh: tuple[()] = case()

    @staticmethod
    def Complete(prior: StudyReceipt) -> ResumePlan:
        return ResumePlan(complete=prior)

    @staticmethod
    def Partial(prior: StudyReceipt, done: int, cached: np.ndarray) -> ResumePlan:
        return ResumePlan(partial=(prior, done, cached))

    @staticmethod
    def Fresh() -> ResumePlan:
        return ResumePlan(fresh=())

    @staticmethod
    def of(prior: StudyReceipt | None, cached: np.ndarray | None) -> ResumePlan:
        match prior, cached:
            case StudyReceipt(cells_completed=done, cells_total=total), _ if done >= total:
                return ResumePlan.Complete(prior)
            case StudyReceipt(cells_completed=done), np.ndarray() as prefix:
                return ResumePlan.Partial(prior, done, prefix[:done])
            case _:
                return ResumePlan.Fresh()


class ComparisonReceipt(Struct, frozen=True):
    names: tuple[str, ...]
    cells: dict[str, tuple[int, int]]
    completion: dict[str, float]
    indices: dict[str, dict[str, float]]
    agreement: dict[str, float]

    def contribute(self) -> Receipt:
        return Receipt.of(
            "emitted",
            "compute.history.compare",
            ",".join(self.names),
            {
                **{f"cells[{k}]": f"{c}/{t}" for k, (c, t) in self.cells.items()},
                **{f"done[{k}]": repr(self.completion[k]) for k in self.names},
                **{f"agree[{pair}]": repr(rho) for pair, rho in self.agreement.items()},
            },
        )


class RunHistory(Struct, frozen=True):
    runs: tuple[StudyReceipt, ...]
    responses: tuple[np.ndarray, ...] = ()

    def resume(self, study: Study, objective: Objective, /, *, seed: int = 0) -> RuntimeRail[StudyReceipt]:
        cache = {r.content_key: y for r, y in zip(self.runs, self.responses, strict=False)}
        return boundary("study.resume", lambda: _resume(self.runs, cache, study, objective, seed))

    def compare(self, *keys: ContentKey) -> ComparisonReceipt:
        by_key = {r.content_key: r for r in self.runs}
        rows = [(_label(by_key[k]), by_key[k]) for k in keys]
        return ComparisonReceipt(
            names=tuple(name for name, _ in rows),
            cells={name: (r.cells_completed, r.cells_total) for name, r in rows},
            completion={name: (r.cells_completed / r.cells_total if r.cells_total else 0.0) for name, r in rows},
            indices={name: r.indices for name, r in rows},
            agreement=_agreement(rows),
        )


def _label(receipt: StudyReceipt) -> str:
    return f"{receipt.method}:{receipt.content_key.hex[:8]}"


def _agreement(rows: list[tuple[str, StudyReceipt]]) -> dict[str, float]:
    shared = sorted(set.intersection(*(set(r.indices) for _, r in rows))) if rows else []
    if len(shared) < 2:
        return {}
    ranks = {name: np.argsort(np.argsort(np.array([r.indices[a] for a in shared]))) for name, r in rows}
    return {
        f"{a}~{b}": _spearman(ranks[a], ranks[b])
        for i, (a, _) in enumerate(rows)
        for b, _ in rows[i + 1 :]
    }


def _spearman(u: np.ndarray, v: np.ndarray) -> float:
    du, dv = u - u.mean(), v - v.mean()
    denom = float(du.std() * dv.std())
    return float((du * dv).mean() / denom) if denom else 1.0


def _resume(
    runs: tuple[StudyReceipt, ...], cache: dict[ContentKey, np.ndarray], study: Study, objective: Objective, seed: int
) -> StudyReceipt:
    design = _design(study, seed)
    key = ContentIdentity.of("study", design.tobytes(), IdentityPolicy())
    prior = next((r for r in runs if r.content_key == key), None)
    match ResumePlan.of(prior, cache.get(key)):
        case ResumePlan(tag="complete", complete=done_run):
            return done_run
        case ResumePlan(tag="partial", partial=(_, done, prefix)):
            tail = np.stack([objective(row) for row in design[done:]])
            responses = np.concatenate([prefix, tail])
            return _receipt(study, design, responses, key)
        case ResumePlan(tag="fresh"):
            responses = np.stack([objective(row) for row in design])
            return _receipt(study, design, responses, key)
        case unreachable:
            assert_never(unreachable)


def _receipt(study: Study, design: np.ndarray, responses: np.ndarray, key: ContentKey) -> StudyReceipt:
    return StudyReceipt(study.method.tag, study.mode, int(responses.size), int(design.shape[0]), _indices(study, design, responses), key)
```

The `ResumePlan` vocabulary closes the resume policy: `of` reads the prior receipt's completed-and-total counts against its cached response prefix into one of three outcomes, and `_resume` folds each outcome to the receipt in a total `match` with `assert_never` on the unreachable arm, every arm landing on the one `_receipt` builder. The complete arm returns the prior receipt unchanged; the partial arm evaluates only `design[done:]`, concatenates the freshly-evaluated tail onto the cached prefix, and recomputes the indices over the whole reconstituted response vector — never over the tail slice, since a SALib variance-, moment-, or derivative-based index is undefined on a structured-design fragment — so a resumed receipt is statistically identical to an unbroken run; the fresh arm runs the whole grid once. `compare` is variadic over the run cohort: it folds any number of keyed runs to one projected row each in a single pass, then derives the cross-run agreement. `_agreement` intersects the runs' sensitivity-axis names, rank-transforms each run's shared-axis index vector through the double `np.argsort` idiom, and folds the upper-triangular run pairs to a Spearman rank-correlation score per pair — the actual analytical read of comparing two studies, how much their per-axis sensitivity orderings concur, computed from cp315-core numpy with no gated dependency. The receipt contributes the cell counts, completion fractions, and the pairwise agreement scores through the same `Receipt` sink as the study receipt, so a comparison reads on the unified telemetry rail rather than a parallel reporting surface.

## [03]-[RESEARCH]

- [RESUME_DESIGN_SLICE]: the partial-resume slice assumes `_design(study, seed)` is row-stable across runs at a fixed seed, so `design[done:]` addresses exactly the cells the prior run left unevaluated and the cached `prefix[:done]` addresses exactly the cells it completed. This holds for the numpy LHS/factorial/polynomial floors (seeded `default_rng`), the scipy `qmc` rows (seeded `Sobol`/`Halton`), and the SALib samplers (seeded). The reconstituted `np.concatenate([prefix, tail])` is therefore the same response vector a single unbroken run produces, and `_indices` runs once over the full design — the only statistically valid recompute, since SALib's Saltelli/Morris/FAST estimators read the whole structured design. `RunHistory` caches each prior run's response vector in `responses` paired to `runs`; a run without a cached vector cannot resume partially and falls to `Fresh`. The `_design`/`_indices` free functions and the `StudyReceipt` `method`/`mode`/`cells_completed`/`cells_total`/`indices`/`content_key` fields are owned by `experiments/study.md#STUDY` and composed here, never re-declared.
- [RANK_AGREEMENT]: `_agreement` reads cross-run sensitivity concurrence over only cp315-core numpy. It intersects the shared sensitivity-axis names, rank-transforms each run's shared-axis index vector via the double-`np.argsort` ordinal-rank idiom, and computes the per-pair Spearman coefficient as the normalized covariance of the rank vectors (`(du*dv).mean()/(du.std()*dv.std())`); a degenerate zero-variance vector folds to perfect agreement `1.0`. No `scipy.stats.spearmanr` or `rankdata` dependency is taken — those carry the `python_version<'3.15'` gate and the closed-form over `argsort`/`mean`/`std` (all `numpy.md`-catalogued) is exact for the no-ties ordinal ranks the transform yields. Fewer than two shared axes yields an empty agreement map rather than a singleton self-correlation.
- [CONTENT_KEY_LABEL]: `_label` reads the `ContentKey.hex` property (`{value:032x}:{fmt}`, owned by `runtime/evidence/identity.md#IDENTITY`) for the comparison row key, slicing its leading characters for a compact axis label; `ContentIdentity.of`/`ContentKey`/`IdentityPolicy` are composed exactly as `experiments/study.md#STUDY` keys its design, never re-declared. `ContentIdentity.of` takes the `CANONICAL_POLICY` default, so the explicit `IdentityPolicy()` argument here keys identically to the study owner's design key at a fixed format and policy.
