# [PY_ARTIFACTS_BENCH]

`CORPUS` is the artifact-producer benchmark roster — per-`ArtifactKind` `BenchSubject` rows graded against threshold policy through the runtime `Bench.run`/`BenchmarkReceipt` tier, so a producer regression surfaces as a graded verdict before an office notices slow sheet sets. Timing stays runtime-owned — measurement, quantiles, and the `rasm.bench.duration`/`rasm.bench.throughput` instruments are the runtime bench family's — so this page owns the roster, the recipes, and the thresholds, concentrated on the native-offload class the `core/receipt#SIGNALS` `[SPAN_CLASS]` row names.

Kernels follow the runtime lane law — a caller supplies its kernel, the corpus never imports one upward: the codec pair rides page-owned recipes over a seeded deterministic byte corpus, while the native-offload trio and the media frame subject arrive as caller recipes at `benched`. Every recipe consumes its row's typed `BenchFeed` edge before yielding the measured kernel — the media frame recipe replays its bound `media/synthesis#SYNTHESIS` `SynthOp` signal, and the typography and chart recipes derive their input from the `_SEED` anchor — so every run replays byte-comparable work and the input ruling is an executable corpus policy. A regressed subject is a graded verdict, never a fault — refusal is reserved for a corpus row no recipe covers or a recipe that rejects its feed.

## [01]-[INDEX]

- [01]-[CORPUS]: the `BenchSubject` roster and threshold policy, the seeded recipe pair, the kernel-coverage law, and the `benched` grade fold over the runtime bench tier.

## [02]-[CORPUS]

- Owner: `CORPUS` is the one subject roster — each `BenchSubject` row binds a dot-path subject id (the `Bench.run` subject and the `domain="bench"` metric kind), its `ArtifactKind`, its `BenchMode`, its `BenchFeed` deterministic-input edge, its round/warmup policy, and its `BenchThreshold` — and `BenchVerdict.graded` is the one grade projection, `passed` the conjunction of the p95 ceiling and the throughput floor read off the runtime receipt. Thresholds are policy rows an office tunes without code: the native-offload trio (`typography/layout`, `typography/shape`, `visualization/chart/export`) carries the tight ceilings because those subjects cross the runtime lane onto foreign native kernels where a regression hides from the request-duration histogram.
- Cases: `BenchMode` is the row's graded-bar policy on the runtime receipt, never a second measurement contract — `Bench.run` folds one uniform per-round wall-clock sample stream for every mode and threads `mode` onto `BenchmarkReceipt.of` as evidence — so `LATENCY` gates on the p95 ceiling with the rate bar vacuous at its zero `floor_hz` default, and `THROUGHPUT` adds the throughput floor while keeping its ceiling, because a rate subject still owns a per-op latency budget; `graded` holds the one uniform conjunction over both bars. `BenchFeed` is the row's typed deterministic-input edge — `owned` marks a page-owned recipe, `signal` binds the ruled `SynthOp` replay value, `seeded` binds the seed a caller kernel derives its input from — so the deterministic-corpus ruling lives as a policy value on the roster.
- Recipes: `RECIPES` owns the kernels this page composes downward — `pack` closes over the seeded two-band corpus (one repetitive band the dictionary matcher folds, one `default_rng(_SEED)` band it cannot) and `recover` closes over the blob one setup pack produced — each recipe a setup-then-op pair so per-round timing never pays construction; the corpus bytes derive from the one `_SEED` anchor, so a threshold breach is a code regression, never input drift. Caller recipes merge under a collision refusal — an owned recipe is never overridden, because a swapped input silently un-anchors the threshold history.
- Entry: `benched(recipes)` is the one entry — it merges `RECIPES` with the caller recipes, refuses an uncovered or colliding subject through `BoundaryFault.config`, binds `setup(row.feed)` before `Bench.run(row.subject, kernel, mode=..., rounds=..., warmup=...)`, drives every row of the receipt's `contribute` return onto the runtime stream through `Signals.emit` under `OPEN` — the `domain="bench"` instrument projection fires inside `contribute`, and the returned receipt rows enter the harvest instead of dropping — and folds the graded verdicts under `Disposition.ACCUMULATE` so every subject reports even when one refuses.
- Packages: `numpy` (`default_rng` the seeded corpus band), `msgspec` (`Struct` rows), `expression` (`Block`/`Map`, `tagged_union` the feed edge), runtime (`Bench`/`BenchmarkReceipt`/`BenchMode`, `traversed`/`Disposition`/`BoundaryFault`/`RuntimeRail`, `Signals`/`Receipt`/`OPEN` the contribution harvest), package plane (`Codec.pack`/`recover`, `CodecProfile`/`ZstdKnobs` — the one downward producer import the recipes earn), media plane (`SynthOp` the replay-signal vocabulary the `signal` feed binds).
- Growth: a new bench subject is one `CORPUS` row and one recipe; a tightened regression bar is one `BenchThreshold` value; a new deterministic-input kind is one `BenchFeed` case; a new bench statistic graduates at the runtime `BenchmarkReceipt` and reaches every verdict through `graded` with no roster edit; a new bench instrument stays one runtime `InstrumentSpec` row, never an artifacts-side meter.
- Boundary: no timing, quantile, or instrument construction at artifacts grain — `Bench.run` measures, `BenchmarkReceipt.contribute` projects, and a page-local `perf_counter` bracket is the deleted form; no `ArtifactReceipt` case for bench evidence, because a benchmark grades the producer, never an artifact; a process-terminal corpus run rides the runtime `JobRun.bounded` envelope so the final projection flushes, and cross-runtime benchmark authority stays the C# owner's, reached only through the wire.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping
from typing import Final, Literal, Self

import numpy as np
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.artifacts.core.receipt import ArtifactKind
from rasm.artifacts.media.synthesis import SynthOp
from rasm.artifacts.package.bundle import CodecProfile, ZstdKnobs
from rasm.artifacts.package.codec import Codec
from rasm.runtime.faults import BoundaryFault, Disposition, RuntimeRail, traversed
from rasm.runtime.profiles import Bench, BenchMode, BenchmarkReceipt
from rasm.runtime.receipts import OPEN, Signals

# --- [TYPES] ----------------------------------------------------------------------------

type BenchKernel = Callable[[], object]


@tagged_union(frozen=True)
class BenchFeed:
    # typed deterministic-input edge: the ruling a caller kernel replays, carried on the roster row.
    tag: Literal["owned", "signal", "seeded"] = tag()
    owned: None = case()  # page-owned recipe supplies its own seeded corpus
    signal: SynthOp = case()  # the ruled media test-signal the caller kernel replays
    seeded: int = case()  # the seed a caller kernel derives its deterministic input from


type BenchRecipe = Callable[[BenchFeed], RuntimeRail[BenchKernel]]  # setup admits the ruled feed before timing


# --- [CONSTANTS] ------------------------------------------------------------------------

_SEED: Final[int] = 41
_BLOCK: Final[int] = 1 << 20

# --- [MODELS] ---------------------------------------------------------------------------


class BenchThreshold(Struct, frozen=True, gc=False):
    p95_ceiling_ms: float
    floor_hz: float = 0.0


class BenchSubject(Struct, frozen=True, gc=False):
    subject: str
    kind: ArtifactKind
    mode: BenchMode
    threshold: BenchThreshold
    feed: BenchFeed = BenchFeed(owned=None)
    rounds: int = 32
    warmup: int = 4


class BenchVerdict(Struct, frozen=True, gc=False):
    subject: str
    kind: ArtifactKind
    passed: bool
    p95_ms: float
    ceiling_ms: float
    throughput_hz: float
    floor_hz: float

    @classmethod
    def graded(cls, row: BenchSubject, receipt: BenchmarkReceipt, /) -> Self:
        bar = row.threshold
        return cls(
            subject=row.subject,
            kind=row.kind,
            passed=receipt.p95_ms <= bar.p95_ceiling_ms and receipt.throughput_hz >= bar.floor_hz,
            p95_ms=receipt.p95_ms,
            ceiling_ms=bar.p95_ceiling_ms,
            throughput_hz=receipt.throughput_hz,
            floor_hz=bar.floor_hz,
        )


# --- [TABLES] ---------------------------------------------------------------------------

# native-offload rows carry the tight ceilings: those renders cross the lane onto foreign
# native kernels whose interior the request-duration histogram cannot attribute.
CORPUS: Final[Block[BenchSubject]] = Block.of_seq([
    BenchSubject("artifacts.package.codec.pack", "bundle", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=250.0)),
    BenchSubject("artifacts.package.codec.recover", "bundle", BenchMode.THROUGHPUT, BenchThreshold(p95_ceiling_ms=100.0, floor_hz=20.0)),
    BenchSubject("artifacts.typography.layout.fit", "document", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=50.0), BenchFeed(seeded=_SEED)),
    BenchSubject("artifacts.typography.shape.run", "document", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=50.0), BenchFeed(seeded=_SEED)),
    BenchSubject("artifacts.visualization.chart.export", "chart", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=2000.0), BenchFeed(seeded=_SEED)),
    BenchSubject(
        "artifacts.media.synthesis.frame", "media", BenchMode.THROUGHPUT, BenchThreshold(p95_ceiling_ms=42.0, floor_hz=24.0), BenchFeed(signal=SynthOp.Bars(1.0))
    ),
])

# --- [OPERATIONS] -----------------------------------------------------------------------

_PROFILE: Final[CodecProfile] = CodecProfile(zstd=ZstdKnobs(level=3))


def _payloads() -> tuple[bytes, ...]:
    banded = bytes(range(256)) * (_BLOCK // 256)
    noisy = np.random.default_rng(_SEED).integers(0, 256, _BLOCK, dtype=np.uint8).tobytes()
    return (banded, noisy)


def _pack_recipe(feed: BenchFeed, /) -> RuntimeRail[BenchKernel]:
    if feed.tag != "owned":
        return Error(BoundaryFault(config=("artifacts.bench", f"pack recipe rejects {feed.tag} feed")))
    payloads = _payloads()
    return Ok(lambda: Codec.pack(payloads, _PROFILE))


def _recover_recipe(feed: BenchFeed, /) -> RuntimeRail[BenchKernel]:
    if feed.tag != "owned":
        return Error(BoundaryFault(config=("artifacts.bench", f"recover recipe rejects {feed.tag} feed")))
    blob, _evidence = Codec.pack(_payloads(), _PROFILE)
    return Ok(lambda: Codec.recover(blob, _PROFILE))


RECIPES: Final[Map[str, BenchRecipe]] = Map.of_seq([
    ("artifacts.package.codec.pack", _pack_recipe),
    ("artifacts.package.codec.recover", _recover_recipe),
])


def benched(recipes: Mapping[str, BenchRecipe], /) -> RuntimeRail[Block[BenchVerdict]]:
    collided = Block.of_seq(RECIPES.keys()).filter(lambda subject: subject in recipes)
    merged: dict[str, BenchRecipe] = {**dict(RECIPES.items()), **dict(recipes)}
    uncovered = CORPUS.map(lambda row: row.subject).filter(lambda subject: subject not in merged)

    def one(row: BenchSubject) -> RuntimeRail[BenchVerdict]:
        def measured(kernel: BenchKernel, /) -> BenchVerdict:
            receipt = Bench.run(row.subject, kernel, mode=row.mode, rounds=row.rounds, warmup=row.warmup)
            Signals.emit(receipt, OPEN)
            return BenchVerdict.graded(row, receipt)

        return merged[row.subject](row.feed).map(measured)

    if not collided.is_empty():
        return Error(BoundaryFault(config=("artifacts.bench", f"owned recipes are not overridable: {','.join(sorted(collided))}")))
    if not uncovered.is_empty():
        return Error(BoundaryFault(config=("artifacts.bench", f"no kernel covers: {','.join(sorted(uncovered))}")))
    return traversed(CORPUS.map(one), by=Disposition.ACCUMULATE)


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ("CORPUS", "RECIPES", "BenchFeed", "BenchSubject", "BenchThreshold", "BenchVerdict", "benched")
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
