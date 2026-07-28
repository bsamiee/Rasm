# [PY_ARTIFACTS_BENCH]

`CORPUS` is the artifact-producer benchmark roster — per-`ArtifactKind` `BenchSubject` rows graded against threshold policy through the runtime `Bench.run`/`BenchmarkReceipt` tier, so a producer regression surfaces as a graded verdict before an office notices slow sheet sets. Timing stays runtime-owned — measurement, quantiles, and the `rasm.bench.duration`/`rasm.bench.throughput` instruments are the runtime bench family's — so this page owns the roster, the recipes, and the thresholds, concentrated on the native-offload class the `core/receipt#SIGNALS` `[SPAN_CLASS]` row names.

Kernels follow the runtime lane law — a caller supplies its kernel, the corpus never imports one upward: the codec pair rides page-owned recipes over a seeded deterministic byte corpus, while the native-offload trio, the media frame subject, and the deep-pixel texture trio arrive as caller recipes at `benched`. Every recipe consumes its row's typed `BenchFeed` edge before yielding the measured kernel — the media frame recipe replays its bound `media/synthesis#SYNTHESIS` `SynthOp` signal, the typography and chart recipes derive their input from the `_SEED` anchor alone, and a texel-cost recipe reads a `BenchPlane` carrying that seed BESIDE the extent its bar was set against — so every run replays byte-comparable work and the input ruling is an executable corpus policy. Host requirements ride a separate `floor` column, because what a kernel derives and what its floor leg spawns are orthogonal facts one union case cannot hold without capping a subject at one binary. Regressed subjects grade as verdicts, never faults — refusal is reserved for a corpus row no recipe covers, a floor naming a tool no probe body keys, a recipe that rejects its feed, or a host on which not one subject is provisioned.

## [01]-[INDEX]

- [02]-[CORPUS]: `BenchSubject` roster and threshold policy, the seeded recipe pair, the kernel-coverage law, and the `benched` grade fold over the runtime bench tier.

## [02]-[CORPUS]

- Owner: `CORPUS` is the one subject roster — each `BenchSubject` row binds a dot-path subject id (the `Bench.run` subject and the `domain="bench"` metric kind), its `ArtifactKind`, its `BenchMode`, its `BenchFeed` deterministic-input edge, its `floor` host requirement, its round/warmup policy, and its `BenchThreshold` — and `BenchVerdict.graded` is the one grade projection, `passed` the conjunction of the p95 ceiling and the throughput floor read off the runtime receipt. Thresholds are policy rows an office tunes without code: the native-offload trio (`typography/layout`, `typography/shape`, `visualization/chart/export`) carries the tight ceilings because those subjects cross the runtime lane onto foreign native kernels where a regression hides from the request-duration histogram.
- Cases: `BenchMode` is the row's graded-bar policy on the runtime receipt, never a second measurement contract — `Bench.run` folds one uniform per-round wall-clock sample stream for every mode and threads `mode` onto `BenchmarkReceipt.of` as evidence — so `LATENCY` gates on the p95 ceiling with the rate bar vacuous at its zero `floor_hz` default, and `THROUGHPUT` adds the throughput floor while keeping its ceiling, because a rate subject still owns a per-op latency budget; `graded` holds the one uniform conjunction over both bars. `BenchFeed` is the row's typed deterministic-input edge and carries INPUT alone — `owned` marks a page-owned recipe, `signal` binds the ruled `SynthOp` replay value, `seeded` binds the seed a scalar-corpus kernel derives from, and `planar` binds a `BenchPlane` whose extent anchors a per-texel bar as tightly as its seed anchors the bytes. `BenchOutcome` closes the verdict counter's value axis at one literal, so silence is a spelled outcome rather than a roster-minus-graded subtraction a reader performs.
- Law: `RECIPES` owns the kernels this page composes downward — `pack` closes over the seeded two-band corpus (one repetitive band the dictionary matcher folds, one `default_rng(_SEED)` band it cannot) and `recover` closes over the blob one setup pack produced — each recipe a setup-then-op pair so per-round timing never pays construction; the corpus bytes derive from the one `_SEED` anchor, so a threshold breach is a code regression, never input drift. That anchoring is why a texel-cost row declares its extent: a seed alone leaves the working set to the recipe, where a bar set at one edge and re-graded at another reads an input change as a regression. Caller recipes merge under a collision refusal — an owned recipe is never overridden, because a swapped input silently un-anchors the threshold history.
- Entry: `benched(recipes)` is the one entry — it merges `RECIPES` with the caller recipes, splits the roster on `_provisioned` so a row whose floor binaries are absent leaves the graded set (coverage is then demanded of the graded rows alone, because a host lacking the binary cannot be asked for the kernel that spawns it, and a run grading nothing refuses by naming the quiet roster), refuses a colliding subject, a floor naming a tool no probe body keys, an uncovered subject, and a wholly unprovisioned host through `BoundaryFault.config` BEFORE any counter writes, binds `setup(row.feed)` before `Bench.run(row.subject, kernel, mode=..., rounds=..., warmup=...)`, drives every row of the receipt's `contribute` return onto the runtime stream through `Signals.emit` under `OPEN` — the `domain="bench"` instrument projection fires inside `contribute`, and the returned receipt rows enter the harvest instead of dropping — writes every outcome through the one `_verdicted` site, because a grade returned to a caller alone reaches no board and no burn rule — and folds the graded verdicts under `Disposition.ACCUMULATE` so every subject reports even when one refuses.
- Packages: `numpy` (`default_rng` the seeded corpus band), `msgspec` (`Struct` rows), `expression` (`Block`/`Map`, `tagged_union` the feed edge), stdlib `shutil.which` (the one `_PROVISION` probe body — presence on the resolved PATH, never a spawn the roster pays for), runtime (`Bench`/`BenchmarkReceipt`/`BenchMode`, `traversed`/`Disposition`/`BoundaryFault`/`RuntimeRail`, `Signals`/`Receipt`/`OPEN` the contribution harvest, `Metrics`/`Dimension` the verdict counter), package plane (`Codec.pack`/`recover`, `CodecProfile`/`ZstdKnobs` — the one downward producer import the recipes earn), media plane (`SynthOp` the replay-signal vocabulary the `signal` feed binds).
- Growth: a new bench subject is one `CORPUS` row and one recipe; a tightened regression bar is one `BenchThreshold` value; a new deterministic-input kind is one `BenchFeed` case; a subject demanding a host binary is one `floor` element and a second binary one more, any feed kind; a new tool is one `_PROVISION` row that `floor` names, and a tool whose presence is not a bare PATH lookup grows its own probe body there; a new run outcome is one `BenchOutcome` member reaching the counter through the single `_verdicted` site; a new bench statistic graduates at the runtime `BenchmarkReceipt` and reaches every verdict through `graded` with no roster edit; a new bench instrument stays one runtime `InstrumentSpec` row, never an artifacts-side meter, and its write rides `Metrics.record` under `domain="bench"` exactly as the verdict counter does.
- Boundary: no timing, quantile, or instrument construction at artifacts grain — `Bench.run` measures, `BenchmarkReceipt.contribute` projects, and a page-local `perf_counter` bracket is the deleted form; no `ArtifactReceipt` case for bench evidence, because a benchmark grades the producer, never an artifact; a process-terminal corpus run rides the runtime `JobRun.bounded` envelope so the final projection flushes, and benchmark authority stays branch-local, so no peer runtime's figure is graded or cited here.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping
from shutil import which
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
from rasm.runtime.metrics import Dimension, Metrics
from rasm.runtime.profiles import Bench, BenchMode, BenchmarkReceipt
from rasm.runtime.receipts import OPEN, Signals

# --- [TYPES] ----------------------------------------------------------------------------

type BenchKernel = Callable[[], object]
type BenchOutcome = Literal["passed", "regressed", "unprovisioned"]  # the counter's whole value axis, one literal
type BenchRecipe = Callable[[BenchFeed], RuntimeRail[BenchKernel]]  # setup admits the ruled feed before timing

# --- [CONSTANTS] ------------------------------------------------------------------------

_SEED: Final[int] = 41
_BLOCK: Final[int] = 1 << 20
_TEXEL_EDGE: Final[int] = 2048  # the deep-pixel working edge every planar row anchors to; an equirect halves it

# --- [MODELS] ---------------------------------------------------------------------------


class BenchPlane(Struct, frozen=True, gc=False):
    # Carries the deterministic working set an extent-scaled kernel derives. Every texel-cost threshold anchors to the
    # EXTENT as tightly as to the seed, because a row re-graded at another extent reads an input change as a code
    # regression — that un-anchoring the collision refusal forbids on the kernel side, arriving through the input.
    width: int
    height: int
    channels: int = 4
    seed: int = _SEED


@tagged_union(frozen=True)
class BenchFeed:
    # typed deterministic-input edge: the ruling a caller kernel replays, carried on the roster row. A host binary is
    # NOT a case here — what a kernel DERIVES and what its floor leg SPAWNS are orthogonal, so folding them onto one
    # case admits exactly one tool, denies every other feed kind a floor, and hands the recipe a name it already spells.
    tag: Literal["owned", "signal", "seeded", "planar"] = tag()
    owned: None = case()  # page-owned recipe supplies its own seeded corpus
    signal: SynthOp = case()  # the ruled media test-signal the caller kernel replays
    seeded: int = case()  # the seed a scalar-corpus caller kernel derives its deterministic input from
    planar: BenchPlane = case()  # seed AND extent, for a kernel whose cost scales per texel


class BenchThreshold(Struct, frozen=True, gc=False):
    p95_ceiling_ms: float
    floor_hz: float = 0.0


class BenchSubject(Struct, frozen=True, gc=False):
    subject: str
    kind: ArtifactKind
    mode: BenchMode
    threshold: BenchThreshold
    feed: BenchFeed = BenchFeed(owned=None)
    floor: tuple[str, ...] = ()  # the host binaries this subject's floor leg spawns; empty demands none, two demand both
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
    # Deep-pixel trio: every row crosses the lane onto a foreign native core over float32 planes, where the
    # per-texel cost scales with the extent the caller feeds rather than with a page count, so each declares the
    # extent its bar was set against and carries the round economy its own working set earns instead of the shared
    # thirty-two. The prefilter row halves its height because an environment plane is admitted at 2:1 or refused.
    BenchSubject(
        "artifacts.graphic.texture.derive.chained", "texture", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=400.0),
        BenchFeed(planar=BenchPlane(width=_TEXEL_EDGE, height=_TEXEL_EDGE)), rounds=16
    ),
    BenchSubject(
        "artifacts.graphic.texture.ibl.prefilter", "texture", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=6000.0),
        BenchFeed(planar=BenchPlane(width=_TEXEL_EDGE, height=_TEXEL_EDGE // 2, channels=3)), rounds=8, warmup=1
    ),
    BenchSubject(
        "artifacts.graphic.texture.plane.ktx_encode",
        "texture",
        BenchMode.THROUGHPUT,
        BenchThreshold(p95_ceiling_ms=1500.0, floor_hz=1.0),
        BenchFeed(planar=BenchPlane(width=_TEXEL_EDGE, height=_TEXEL_EDGE)),
        floor=("ktx",),
        rounds=8,
        warmup=1,
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

# One probe body per tool id, taking the name it is keyed by so one body serves every tool a bare PATH lookup
# answers and a tool needing more grows its own. Phase-0 provisioning put `ktx` on the bare PATH beside
# `ktx2check`/`toktx`; the probe asserts PRESENCE alone because every `ktx` binary prints `GIT-NOTFOUND` for
# `--version`, so version text proves nothing about the encoder behind it.
_PROVISION: Final[Map[str, Callable[[str], bool]]] = Map.of_seq([("ktx", lambda name: which(name) is not None)])


def _provisioned(row: BenchSubject, /) -> bool:
    # every declared FLOOR binary must answer, and a name no probe row keys reads absent rather than raising — the
    # roster defect refuses by name at admission instead of killing the run from inside a filter. An in-process
    # acceleration leg that happens to import never substitutes: it is a different encoder, so grading it against
    # that floor's own threshold history un-anchors the history exactly as a swapped input or extent does.
    return all(_PROVISION.try_find(tool).map(lambda probe: probe(tool)).default_value(False) for tool in row.floor)


def _verdicted(subject: str, outcome: BenchOutcome, /) -> None:
    # THE recording site for the row-level bar: two sites under one measure double a series and strand its
    # aggregation on the next emission edit. A grade living only in the returned value leaves the board with nothing
    # to trend and the alert plane with nothing to fire on — the timing ladder shows a regression's shape where this
    # counter shows whether the row's own bar was crossed. Kind stays the subject id every `domain="bench"` write
    # carries, so the three outcomes read as one series a share expression divides.
    Metrics.record({"rasm.bench.verdicts": 1.0}, domain="bench", kind=subject, dimensions={Dimension.OUTCOME: outcome})


def benched(recipes: Mapping[str, BenchRecipe], /) -> RuntimeRail[Block[BenchVerdict]]:
    merged: dict[str, BenchRecipe] = {**dict(RECIPES.items()), **dict(recipes)}
    collided = Block.of_seq(RECIPES.keys()).filter(lambda subject: subject in recipes)
    unrostered = frozenset(CORPUS.collect(lambda row: Block.of_seq(row.floor)).filter(lambda tool: tool not in _PROVISION))
    live, quiet = CORPUS.partition(_provisioned)  # ONE probe pass: a second filter re-spawns every lookup per row
    uncovered = live.map(lambda row: row.subject).filter(lambda subject: subject not in merged)

    def one(row: BenchSubject) -> RuntimeRail[BenchVerdict]:
        def graded(receipt: BenchmarkReceipt, /) -> BenchVerdict:
            Signals.emit(receipt, OPEN)
            verdict = BenchVerdict.graded(row, receipt)
            _verdicted(row.subject, "passed" if verdict.passed else "regressed")
            return verdict

        # `Bench.run` rails: a window truncated by a raising round still grades off the samples it measured, and a
        # window measuring nothing refuses by name into this row's own accumulate fold rather than grading a fiction.
        def measured(kernel: BenchKernel, /) -> RuntimeRail[BenchVerdict]:
            return Bench.run(row.subject, kernel, mode=row.mode, rounds=row.rounds, warmup=row.warmup).map(graded)

        return merged[row.subject](row.feed).bind(measured)

    if not collided.is_empty():
        return Error(BoundaryFault(config=("artifacts.bench", f"owned recipes are not overridable: {','.join(sorted(collided))}")))
    if unrostered:
        # a floor naming a tool no probe body keys is a ROSTER defect, not a quiet host: reading it as absence would
        # retire the subject on every machine and read as an uninstalled binary nobody can install.
        return Error(BoundaryFault(config=("artifacts.bench", f"no probe body keys: {','.join(sorted(unrostered))}")))
    if not uncovered.is_empty():
        return Error(BoundaryFault(config=("artifacts.bench", f"no kernel covers: {','.join(sorted(uncovered))}")))
    if live.is_empty():
        # a run that graded nothing is a host misconfiguration, never a pass — the quiet roster names what to install
        return Error(BoundaryFault(config=("artifacts.bench", f"no subject is provisioned: {','.join(sorted(row.subject for row in quiet))}")))
    for row in quiet:
        # Third outcome writes only once the run is ADMITTED: a refused run publishes nothing, because a counter
        # rising on a configuration the caller never got past reports a host state off a call that never measured one.
        # Quiet rows MEASURED NOTHING and reach no verdict — a passed or regressed grade there is a reading no run
        # took — so the outcome axis carries silence itself rather than leaving it to a roster-minus-graded subtraction.
        _verdicted(row.subject, "unprovisioned")
    return traversed(live.map(one), by=Disposition.ACCUMULATE)


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ("CORPUS", "RECIPES", "BenchFeed", "BenchSubject", "BenchThreshold", "BenchVerdict", "benched")
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
