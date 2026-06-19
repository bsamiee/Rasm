# [PY_COMPUTE_HISTORY]

The experiment-run persistence, resume, and comparison rail on the study spine. `RunHistory` carries the prior study receipts and re-runs only the incomplete grid cells of a study keyed by its design `ContentKey`, so a complete prior run short-circuits, an incomplete prior run evaluates only its remaining cells, and a fresh design runs the whole grid once. The comparison join reads two run receipts by content key and projects their completed-and-total cell counts. This rail rides the same param-axis and sample-grid spine as the study owner rather than a parallel experiment tracker.

## [01]-[INDEX]

- [01]-[RUN_HISTORY]: content-key-keyed run persistence, partial-cell resume, and run comparison on one `RunHistory` owner.

## [02]-[RUN_HISTORY]

- Owner: `RunHistory` — the experiment-run persistence and resume rail on the study spine; it carries the prior `StudyReceipt` cohort and re-runs only the incomplete grid cells. `resume` keys a study by its design `ContentKey`, short-circuits a complete prior run, evaluates the remaining cells of an incomplete prior run, and runs the whole grid for a fresh design. `compare` joins two run receipts by content key and projects their cell counts. No parallel experiment tracker stands beside it.
- Entry: `RunHistory.resume` returns `RuntimeRail[StudyReceipt]` through one `boundary`; the partial-cell resume rebuilds the design from the study seed, finds the prior run on the same key, and folds the remaining responses into a merged receipt carrying the cumulative completed count. `RunHistory.compare` returns the completed-and-total cell counts for the two keyed runs.
- Receipt: a resumed run emits the merged `StudyReceipt` from `experiments/study.md#STUDY`; the receipt carries the design `ContentKey` so a later resume keys distinctly when the data or method changes, never colliding to a stale cache hit.
- Packages: `numpy` (`apply_along_axis`), `experiments/study.md#STUDY` (`Study`, `StudyReceipt`, `Objective`, the design build), runtime (`RuntimeRail`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `boundary`).
- Growth: a new resume policy is one `match` arm on the prior receipt; a new comparison projection is one field on the compare result; zero new surface.
- Boundary: no job framework, scheduler, or production run store; offline run history only. A parallel experiment-tracker owner and a resume that re-runs completed cells are the deleted forms. This rail is pure-numpy over the study design and runs unconditionally on cp315.

```python signature
import numpy as np
from msgspec import Struct

from rasm.compute.experiments.study import Objective, Study, StudyReceipt, _design, _indices
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary


class RunHistory(Struct, frozen=True):
    runs: tuple[StudyReceipt, ...]

    def resume(self, study: Study, objective: Objective, /, *, seed: int = 0) -> "RuntimeRail[StudyReceipt]":
        return boundary("study.resume", lambda: _resume(self.runs, study, objective, seed))

    def compare(self, left: ContentKey, right: ContentKey, /) -> dict[str, tuple[int, int]]:
        by_key = {r.content_key: r for r in self.runs}
        a, b = by_key[left], by_key[right]
        return {"cells": (a.cells_completed, b.cells_completed), "total": (a.cells_total, b.cells_total)}


def _resume(runs: tuple[StudyReceipt, ...], study: Study, objective: Objective, seed: int) -> StudyReceipt:
    design = _design(study, seed)
    key = ContentIdentity.of("study", design.tobytes(), IdentityPolicy())
    prior = next((r for r in runs if r.content_key == key), None)
    match prior:
        case StudyReceipt(cells_completed=done, cells_total=total) if done >= total:
            return prior
        case StudyReceipt(cells_completed=done):
            remaining = np.apply_along_axis(objective, 1, design[done:])
            indices = _indices(study, design, remaining) if done == 0 else {}
            return StudyReceipt(study.method.tag, study.mode, done + int(remaining.size), int(design.shape[0]), indices, key)
        case _:
            responses = np.apply_along_axis(objective, 1, design)
            return StudyReceipt(study.method.tag, study.mode, int(responses.size), int(design.shape[0]), _indices(study, design, responses), key)
```
