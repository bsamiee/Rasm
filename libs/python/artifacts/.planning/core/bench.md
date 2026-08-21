# [PY_ARTIFACTS_BENCH]

`CORPUS` is the artifact-producer benchmark roster — `BenchEntry` rows pairing a runtime `BenchSubject` with the deterministic-input edge this stratum owns, graded against threshold policy through the runtime `Bench.graded` tier, so a producer regression surfaces as a graded verdict before an office notices slow sheet sets. Measurement AND grading are runtime-owned — quantiles, thresholds, verdicts, the `rasm.bench.duration`/`.throughput`/`.verdicts` instruments, and the estate tool roster the host floor resolves through all seat at `observability/profiles#BENCH`, reachable from every stratum — so this page owns the roster, the recipes, and the seeded corpus, concentrated on the native-offload class the `core/receipt#SIGNALS` `[SPAN_CLASS]` row names.

Kernels follow the runtime lane law — a caller supplies its kernel, the corpus never imports one upward: the codec pair rides page-owned recipes over a seeded deterministic byte corpus, while the native-offload trio, the media frame subject, and the deep-pixel texture trio arrive as caller recipes at `benched`. Every recipe consumes its entry's typed `BenchFeed` edge before yielding the measured kernel — the media frame recipe replays its bound `media/synthesis#SYNTHESIS` `SynthOp` signal, the typography and chart recipes derive their input from the `_SEED` anchor alone, and a texel-cost recipe reads a `BenchPlane` carrying that seed BESIDE the extent its bar was set against — so every run replays byte-comparable work and the input ruling is an executable corpus policy. That resolution is exactly what keeps `BenchFeed`, `BenchPlane`, and `SynthOp` at this stratum: `benched` hands the grader bound `BenchKernel` thunks, and the tier above never learns what derived them. Host requirements ride the subject's `floor` column naming rows in the runtime tool roster, because what a kernel derives and what its floor leg spawns are orthogonal facts one union case cannot hold without capping a subject at one binary. Regressed subjects grade as verdicts, never faults — refusal is reserved for an owned recipe a caller tried to override here, and for the roster and host defects the grader names.

## [01]-[INDEX]

- [02]-[CORPUS]: the `BenchEntry` roster pairing runtime subjects with their deterministic-input edges, the seeded recipe pair, and the `benched` feed-resolution entry onto the runtime grader.

## [02]-[CORPUS]

- Owner: `CORPUS` is the one subject roster — each `BenchEntry` pairs a runtime `BenchSubject` (its dot-path subject id, doubling as the `Bench.run` subject and the `domain="bench"` metric kind, its artifact kind label, its `BenchMode`, its `floor` host requirement, its round/warmup policy, and its `BenchThreshold`) with the `BenchFeed` edge that stratum cannot hold. Pairing beats a subject-keyed sidecar table: there a subject lands with no feed and nothing says so. Thresholds are policy rows an office tunes without code: the native-offload trio (`typography/layout`, `typography/shape`, `visualization/chart/export`) carries the tight ceilings because those subjects cross the runtime lane onto foreign native kernels where a regression hides from the request-duration histogram.
- Cases: `BenchMode` is the row's graded-bar policy on the runtime receipt, never a second measurement contract — one uniform per-round wall-clock sample stream serves every mode and `mode` threads onto the receipt as evidence — so `LATENCY` gates on the p95 ceiling with the rate bar vacuous at its zero `floor_hz` default and `THROUGHPUT` adds the rate floor while keeping its ceiling, the grader holding the one uniform conjunction. `BenchFeed` is the entry's typed deterministic-input edge and carries INPUT alone — `owned` marks a page-owned recipe, `signal` binds the ruled `SynthOp` replay value, `seeded` binds the seed a scalar-corpus kernel derives from, and `planar` binds a `BenchPlane` whose extent anchors a per-texel bar as tightly as its seed anchors the bytes.
- Law: `RECIPES` owns the kernels this page composes downward — `pack` closes over the seeded two-band corpus (one repetitive band the dictionary matcher folds, one `default_rng(_SEED)` band it cannot) and `recover` closes over the blob one setup pack produced — each recipe a setup-then-op pair so per-round timing never pays construction; the corpus bytes derive from the one `_SEED` anchor, so a threshold breach is a code regression, never input drift. That anchoring is why a texel-cost entry declares its extent: a seed alone leaves the working set to the recipe, where a bar set at one edge and re-graded at another reads an input change as a regression. Caller recipes merge under a collision refusal — an owned recipe is never overridden, because a swapped input silently un-anchors the threshold history.
- Entry: `benched(recipes)` is the one entry — it merges `RECIPES` with the caller recipes, refuses an override of an owned one, binds every covered entry's `recipe(entry.feed)` into a `BenchKernel` under `Disposition.ACCUMULATE` so a recipe rejecting its feed refuses under its own subject name before any timing runs, and hands the whole roster plus that kernel map to `Bench.graded`. A subject with no recipe never enters the map, so the grader's coverage refusal names it exactly when the host provisions it — the one place that can be decided, since a host lacking a binary cannot be asked for the kernel that spawns it.
- Packages: `numpy` (`default_rng` the seeded corpus band), `msgspec` (`Struct` rows), `expression` (`Block`/`Map`, `tagged_union` the feed edge), runtime (`Bench.graded`/`Bench`/`BenchKernel`/`BenchMode`/`BenchSubject`/`BenchThreshold`/`BenchVerdict` and `KTX_TOOL` the roster id every `floor` keys off, `traversed`/`Disposition`/`FaultRow`/`RuntimeRail`, its `RAISES` roster anchored on `ArtifactsLeg.BENCH`), package plane (`Codec.pack`/`recover`, `CodecProfile`/`ZstdKnobs` — the one downward producer import the recipes earn), media plane (`SynthOp` the replay-signal vocabulary the `signal` feed binds).
- Growth: a new bench subject is one `CORPUS` entry and one recipe; a tightened regression bar is one `BenchThreshold` value on that entry's subject; a new deterministic-input kind is one `BenchFeed` case; a subject demanding a host binary is one `floor` element and a second binary one more, any feed kind; a new external tool is one row on the runtime roster that `floor` names, zero edits here; a new bench statistic, run outcome, or instrument graduates entirely at the runtime tier and reaches every verdict with no roster edit.
- Boundary: no timing, quantile, threshold, verdict, or instrument construction at artifacts grain — the runtime tier measures and grades, and a page-local `perf_counter` bracket or a second tool-discovery ladder is the deleted form; no `ArtifactReceipt` case for bench evidence, because a benchmark grades the producer, never an artifact; a process-terminal corpus run rides the runtime `JobRun.bounded` envelope so the final projection flushes. A `floor` names the runtime roster's own `KTX_TOOL` PROVISION id, never the texture plane's spawn-command constant — one is the key a host is probed under and the other the executable a leg launches, and conflating them makes both surfaces candidate owners of one spelling. The import direction is one-way by construction: the grader and the tool roster seat at a tier every stratum reaches, so a producer plane composes them directly and no lookup reaches upward.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Mapping
from typing import Final, Literal

import numpy as np
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.artifacts.core.hooks import ArtifactsLeg
from rasm.artifacts.media.synthesis import SynthOp
from rasm.artifacts.package.bundle import CodecProfile, ZstdKnobs
from rasm.artifacts.package.codec import Codec
from rasm.runtime.faults import TERMINAL, Disposition, FaultRow, RuntimeRail, rostered, traversed
from rasm.runtime.profiles import KTX_TOOL, Bench, BenchKernel, BenchMode, BenchSubject, BenchThreshold, BenchVerdict

# --- [TYPES] ----------------------------------------------------------------------------

type BenchRecipe = Callable[[BenchFeed], RuntimeRail[BenchKernel]]  # setup admits the ruled feed before timing

# --- [TABLES] ---------------------------------------------------------------------------

# this page's whole raise roster. Both owned recipes refuse the same law — a recipe bound to the owned corpus cannot
# run against a foreign feed — so ONE parameterized row carries which recipe and which feed rather than two rows
# spelling one defect twice. Every refusal here is TERMINAL: a re-run over the same roster and the same feed refuses
# identically, and every downstream refusal past the collision belongs to the runtime grader.
BENCH_FEED: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.BENCH, point="recipe", arm="config", defect="feed-refused", retriability=TERMINAL, slots=("recipe", "feed")
)
BENCH_OWNED: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.BENCH, point="roster", arm="config", defect="owned-override", retriability=TERMINAL, slots=("subjects",)
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([BENCH_FEED, BENCH_OWNED]))

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


class BenchEntry(Struct, frozen=True, gc=False):
    # the artifacts-side roster row: the runtime `BenchSubject` the grader reads BESIDE the deterministic-input edge
    # this stratum owns and the runtime tier cannot hold — a `BenchFeed` binds a `SynthOp` and a `BenchPlane`, both S3
    # values, so carrying one upward would drag the media and texture planes into the runtime tier. Pairing them on one
    # row is what a subject-keyed sidecar table gives up: there, a subject can land with no feed and nothing says so.
    subject: BenchSubject
    feed: BenchFeed = BenchFeed(owned=None)


# --- [TABLES] ---------------------------------------------------------------------------

# native-offload rows carry the tight ceilings: those renders cross the lane onto foreign
# native kernels whose interior the request-duration histogram cannot attribute.
CORPUS: Final[Block[BenchEntry]] = Block.of_seq([
    BenchEntry(BenchSubject("artifacts.package.codec.pack", "bundle", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=250.0))),
    BenchEntry(
        BenchSubject("artifacts.package.codec.recover", "bundle", BenchMode.THROUGHPUT, BenchThreshold(p95_ceiling_ms=100.0, floor_hz=20.0))
    ),
    BenchEntry(
        BenchSubject("artifacts.typography.layout.fit", "document", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=50.0)),
        BenchFeed(seeded=_SEED),
    ),
    BenchEntry(
        BenchSubject("artifacts.typography.shape.run", "document", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=50.0)),
        BenchFeed(seeded=_SEED),
    ),
    BenchEntry(
        BenchSubject("artifacts.visualization.chart.export", "chart", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=2000.0)),
        BenchFeed(seeded=_SEED),
    ),
    BenchEntry(
        BenchSubject("artifacts.media.synthesis.frame", "media", BenchMode.THROUGHPUT, BenchThreshold(p95_ceiling_ms=42.0, floor_hz=24.0)),
        BenchFeed(signal=SynthOp.Bars(1.0)),
    ),
    # Deep-pixel trio: every row crosses the lane onto a foreign native core over float32 planes, where the
    # per-texel cost scales with the extent the caller feeds rather than with a page count, so each declares the
    # extent its bar was set against and carries the round economy its own working set earns instead of the shared
    # thirty-two. The prefilter row halves its height because an environment plane is admitted at 2:1 or refused.
    BenchEntry(
        BenchSubject("artifacts.graphic.texture.derive.chained", "texture", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=400.0), rounds=16),
        BenchFeed(planar=BenchPlane(width=_TEXEL_EDGE, height=_TEXEL_EDGE)),
    ),
    BenchEntry(
        BenchSubject(
            "artifacts.graphic.texture.ibl.ggx_prefilter", "texture", BenchMode.LATENCY, BenchThreshold(p95_ceiling_ms=6000.0), rounds=8, warmup=1
        ),
        BenchFeed(planar=BenchPlane(width=_TEXEL_EDGE, height=_TEXEL_EDGE // 2, channels=3)),
    ),
    BenchEntry(
        BenchSubject(
            "artifacts.graphic.texture.plane.ktx_encode",
            "texture",
            BenchMode.THROUGHPUT,
            BenchThreshold(p95_ceiling_ms=1500.0, floor_hz=1.0),
            floor=(KTX_TOOL,),
            rounds=8,
            warmup=1,
        ),
        BenchFeed(planar=BenchPlane(width=_TEXEL_EDGE, height=_TEXEL_EDGE)),
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
        return Error(BENCH_FEED.raised("pack", feed.tag))
    payloads = _payloads()
    return Ok(lambda: Codec.pack(payloads, _PROFILE))


def _recover_recipe(feed: BenchFeed, /) -> RuntimeRail[BenchKernel]:
    if feed.tag != "owned":
        return Error(BENCH_FEED.raised("recover", feed.tag))
    blob, _evidence = Codec.pack(_payloads(), _PROFILE)
    return Ok(lambda: Codec.recover(blob, _PROFILE))


RECIPES: Final[Map[str, BenchRecipe]] = Map.of_seq([
    ("artifacts.package.codec.pack", _pack_recipe),
    ("artifacts.package.codec.recover", _recover_recipe),
])


def benched(recipes: Mapping[str, BenchRecipe], /) -> RuntimeRail[Block[BenchVerdict]]:
    # this page's whole remaining job: merge the recipes, refuse an override of an owned one, resolve EVERY entry's
    # feed to a bound kernel, and hand the roster plus that kernel map to the runtime grader. The feed resolution is
    # exactly what keeps `BenchFeed`, `BenchPlane`, and `SynthOp` at this stratum — the grader reads a
    # `Callable[[], object]` and never learns what derived it — and every refusal past the collision (an unrostered
    # tool, an uncovered subject, a wholly unprovisioned host) is the grader's, so this page states none of them twice.
    merged: dict[str, BenchRecipe] = {**dict(RECIPES.items()), **dict(recipes)}
    collided = Block.of_seq(RECIPES.keys()).filter(lambda subject: subject in recipes)
    covered = CORPUS.filter(lambda entry: entry.subject.subject in merged)
    return (
        Error(BENCH_OWNED.raised(",".join(sorted(collided))))
        if not collided.is_empty()
        else traversed(covered.map(lambda entry: _bound(merged, entry)), by=Disposition.ACCUMULATE).bind(
            lambda pairs: Bench.graded(CORPUS.map(lambda entry: entry.subject), Map.of_seq(pairs))
        )
    )


def _bound(recipes: Mapping[str, BenchRecipe], entry: BenchEntry, /) -> RuntimeRail[tuple[str, BenchKernel]]:
    # a subject with no recipe never reaches this fold and never enters the kernel map, so the grader's own coverage
    # refusal names it exactly when the host provisions it — the one place that can be decided, since a host lacking a
    # binary cannot be asked for the kernel that spawns it. Every recipe that IS present binds its feed here, so a
    # recipe rejecting its feed refuses under its own subject name before any timing runs.
    subject = entry.subject.subject
    return recipes[subject](entry.feed).map(lambda kernel: (subject, kernel))


# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ("CORPUS", "RECIPES", "BenchEntry", "BenchFeed", "BenchPlane", "benched")
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
