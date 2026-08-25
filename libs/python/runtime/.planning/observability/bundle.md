# [PY_RUNTIME_BUNDLE]

`SupportBundle` folds the daemon's evidence state into one local, pull-driven diagnostic capsule. One `COLLECTORS` table owns the capture surface — interpreter stacks and the native frame, the gated heap ranking, every hook REPLAY window, the install roster, the backend-free measurement reading, the admitted-context render, and the supervision verdict — each row fenced, so a refusing collector lands as a skipped roster entry beside its logged fault, never a failed capture. Archive encoding passes collector facts through the observe-owned `Redaction`, compresses with `compression.zstd`, and mints the `ContentIdentity.key`; identical state keys identically.

Capture starts nothing. Heap analysis reads only an already-tracing `tracemalloc`; snapshot and ranking cost still scale with the traced allocation set, while `HEAP_ROWS` caps only the emitted ranking. Replay rings arrive pre-trimmed to their registered `HookPoint.buffer`, and the stack dump spans exactly the live thread set — no sampling loop lands beside the admitted profilers. `Redaction`/`OPEN`, `ENCODE`, and the scope-bound `logger` arrive settled from `observability/observe#OBSERVE`, the fault fences from `reliability/faults#FAULT`; the REPLAY rings from `observability/hooks#HOOKS`; the install records from their `observability/telemetry#TELEMETRY`, `observability/metrics#METRIC`, `observability/logging#PIPELINE`, and `observability/profiles#PROFILES` owners; the measurement reading from that telemetry owner's `snapshot`; and the verdict projection as data off the `execution/workers#SUPERVISION` accessor.

## [01]-[INDEX]

- [02]-[BUNDLE]: one fenced collectors table, redaction-then-encode archive fold, and content-keyed `Bundle` evidence.

## [02]-[BUNDLE]

- Owner: `Collector` is one capture row — name, availability gate, collect — and `COLLECTORS` the closed roster every capture folds; `Bundle` carries the archive body beside its `ContentKey` and the collected/skipped rosters, and `facts` projects key, byte length, and roster counts onto the one `bundle.captured` line while the body stays bytes — the key correlates two captures on a log line, the archive itself never rides one.
- Cases: a gate-closed row (the heap row with no tracer running) skips silently into the roster; a raising collector converts through the `boundary` fence into one `bundle.collect` warning naming the row and joins `skipped`; a collected row lands its redacted facts under its name in the one document. Archive finalization — deterministic encode, `zstd` compress, key mint — runs under its own `bundle.archive` fence, so `capture` returns `RuntimeRail[Bundle]` and a finalization fault lands as one warning beside the rail's refusal. The captured line writes AFTER the archive is built, so a wedged sink — the condition a bundle gets pulled under — never costs the archive.
- Entry: `capture(subject, *, selected, redaction)` is the one fold — an empty selection runs every row and a named selection bounds the roster. `Subject` carries the admitted-context render, verdict thunk, and scope as one value, so replay and emitted evidence stay inside the calling composition while the static table remains closed.
- Auto: the document encodes through the observe-owned deterministic `ENCODE`, so key order is stable and the `ContentKey` replays across captures of identical state; `zstd.compress` bounds the archive body; redaction applies per collector BEFORE encoding and classifies by key name at EVERY depth, so the caller-supplied context, the verdict facts, and the nested `_installs` maps and `_replay` hook rings all scrub in place even under a permissive sink; the capture writes its `Bundle` facts as one line, so every pull leaves a line beside the built archive.
- Law: every fence resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.BUNDLE`; the collector name stays on the warning line where an operator reads it, so a per-collector fence subject bought a coordinate the line already carries. The collector fence keeps a catch-all and states why — a collector body is the plane a capsule gets pulled UNDER, so it may never raise past `capture`.
- Growth: a new evidence source is one `Collector` row; a new capture input is one `Subject` field; a new redaction transform stays the observe owner's `Scrub` growth.
- Boundary: collection never starts an agent, thread, tracer, or sampling loop — the profilers stay the admitted owners, the heap gate reads, never arms, `tracemalloc`, and the readings row reads, never mounts, the diagnostic reader whose arming is the composition's `SignalProfile` value. `memray` is DECLINED on that same law — its allocation profiler arms a tracker the capture then owns, the exact agent this row forecloses — so the heap artifact stays the read-only `tracemalloc` ranking and the continuous rail stays `pyroscope-io`.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import faulthandler
import json
import tracemalloc
from collections.abc import Callable
from tempfile import TemporaryFile
from typing import Final, assert_never

import compression.zstd as zstd
from expression import Result
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.runtime.faults import BUNDLE_ARCHIVE, BUNDLE_COLLECT, RuntimeRail, boundary
from rasm.runtime.hooks import Hooks
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.logging import LogPipeline
from rasm.runtime.metrics import Instrumentation, Metrics
from rasm.runtime.profiles import Profiles
from rasm.runtime.observe import DEFAULT_SCOPE, ENCODE, OPEN, Facts, Redaction, ScopeKey, logger
from rasm.runtime.telemetry import Telemetry

# --- [TYPES] ----------------------------------------------------------------------------

type Verdicts = Callable[[], Map[str, str]]
type Collect = Callable[["Subject"], Facts]

# --- [CONSTANTS] ------------------------------------------------------------------------


HEAP_ROWS: Final[int] = 64

# --- [MODELS] ---------------------------------------------------------------------------


class Subject(Struct, frozen=True):
    facts: dict[str, str]
    verdicts: Verdicts
    scope: ScopeKey = DEFAULT_SCOPE


class Collector(Struct, frozen=True):
    name: str
    gated: Callable[[], bool]
    collect: Collect


class Bundle(Struct, frozen=True):
    key: ContentKey
    body: bytes
    collected: tuple[str, ...]
    skipped: tuple[str, ...]

    def facts(self) -> Facts:
        return {"key": self.key.project("hex"), "bytes": len(self.body), "collected": len(self.collected), "skipped": len(self.skipped)}


# --- [OPERATIONS] -----------------------------------------------------------------------


def _dumped(dump: Callable[..., None], **kwargs: object) -> str:
    with TemporaryFile(mode="w+") as sink:
        dump(file=sink, **kwargs)
        sink.seek(0)
        return sink.read()


def _stacks(_: Subject) -> Facts:
    return {"threads": _dumped(faulthandler.dump_traceback, all_threads=True)}


def _native(_: Subject) -> Facts:
    return {"frames": _dumped(faulthandler.dump_c_stack)}


def _heap(_: Subject) -> Facts:
    return {"rows": tuple(str(stat) for stat in tracemalloc.take_snapshot().statistics("lineno")[:HEAP_ROWS])}


def _replay(subject: Subject) -> Facts:
    return {
        point.value: {"facts": tuple(structs.asdict(fact) for fact in ring.held), **ring.facts()}
        for point, ring in Hooks.replayed(scope=subject.scope).items()
    }


def _isolated(subject: Subject) -> Facts:
    ring = Hooks.faults(scope=subject.scope)
    return {
        "faults": tuple(fault.facts() for fault in ring.held),
        **ring.facts(),
    }


def _installs(subject: Subject) -> Facts:
    held = {
        "telemetry": Telemetry.installed(),
        "metrics": Metrics.installed(),
        "instrumentation": Instrumentation.installed(),
        "logging": LogPipeline.installed(),
        "profiles": Profiles.installed(),
    }
    process = {owner: install.map(structs.asdict).default_value({}) for owner, install in held.items()}
    return process | {owner: dict(structs.asdict(install)) for owner, install in Hooks.installs(scope=subject.scope).items()}


def _readings(_: Subject) -> Facts:
    data = Telemetry.snapshot()
    return {"metrics": data.map(lambda tree: json.loads(tree.to_json(indent=None))).default_value({})}


def _context(subject: Subject) -> Facts:
    return dict(subject.facts)


def _verdicts(subject: Subject) -> Facts:
    return dict(subject.verdicts().items())


def _always() -> bool:
    return True


# --- [TABLES] ---------------------------------------------------------------------------

COLLECTORS: Final[Block[Collector]] = Block.of_seq([
    Collector("stacks", _always, _stacks),
    Collector("native", _always, _native),
    Collector("heap", tracemalloc.is_tracing, _heap),
    Collector("replay", _always, _replay),
    Collector("isolated", _always, _isolated),
    Collector("installs", _always, _installs),
    Collector("readings", lambda: Telemetry.installed().map(lambda r: r.signal_profile.diagnostic_read).default_value(False), _readings),
    Collector("context", _always, _context),
    Collector("verdicts", _always, _verdicts),
])

# --- [SERVICES] -------------------------------------------------------------------------


class SupportBundle:
    @staticmethod
    def capture(subject: Subject, *, selected: tuple[str, ...] = (), redaction: Redaction = OPEN) -> RuntimeRail[Bundle]:
        known = frozenset(row.name for row in COLLECTORS)
        roster = COLLECTORS if not selected else COLLECTORS.filter(lambda row: row.name in selected)
        absent = Block.of_seq(name for name in selected if name not in known)

        def folded(acc: tuple[Map[str, Facts], Block[str], Block[str]], row: Collector) -> tuple[Map[str, Facts], Block[str], Block[str]]:
            document, collected, skipped = acc
            match boundary(BUNDLE_COLLECT, lambda: redaction.apply(row.collect(subject)) if row.gated() else None, catch=Exception):
                case Result(tag="ok", ok=None):
                    return document, collected, skipped.append(Block.singleton(row.name))
                case Result(tag="ok", ok=facts):
                    return document.add(row.name, facts), collected.append(Block.singleton(row.name)), skipped
                case Result(tag="error", error=fault):
                    logger(subject.scope).warning("bundle.collect", collector=row.name, **fault.facts())
                    return document, collected, skipped.append(Block.singleton(row.name))
                case _ as unreachable:
                    assert_never(unreachable)

        document, collected, skipped = roster.fold(folded, (Map.empty(), Block.empty(), absent))

        def archived() -> Bundle:
            body = zstd.compress(ENCODE({name: facts for name, facts in document.items()}))
            return Bundle(key=ContentIdentity.key("bundle", body), body=body, collected=tuple(collected), skipped=tuple(skipped))

        outcome = boundary(BUNDLE_ARCHIVE, archived, catch=(zstd.ZstdError, TypeError, ValueError))
        outcome.swap().map(lambda fault: logger(subject.scope).warning("bundle.archive", **fault.facts()))
        outcome.map(lambda bundle: logger(subject.scope).info("bundle.captured", **bundle.facts()))
        return outcome
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
