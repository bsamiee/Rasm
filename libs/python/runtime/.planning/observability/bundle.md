# [PY_RUNTIME_BUNDLE]

`SupportBundle` folds the daemon's evidence state into one local, pull-driven diagnostic capsule. One `COLLECTORS` table owns the capture surface — interpreter stacks and the native frame, the gated heap ranking, every hook REPLAY window, the install-receipt roster, the backend-free measurement reading, the admitted-context render, and the supervision verdict — each row fenced, so a refusing collector lands as a skipped roster entry beside its rejected receipt, never a failed capture. Archive encoding passes collector facts through the receipts-owned `Redaction`, compresses with `compression.zstd`, and mints the `ContentIdentity.key`; identical state keys identically.

Capture starts nothing. Heap analysis reads only an already-tracing `tracemalloc`; snapshot and ranking cost still scale with the traced allocation set, while `HEAP_ROWS` caps only the emitted ranking. Replay rings arrive pre-trimmed to their registered `HookPoint.buffer`, and the stack dump spans exactly the live thread set — no sampling loop lands beside the admitted profilers. `Redaction`/`OPEN`, `Receipt`, `ENCODE`, and the fault fences arrive settled from `observability/receipts#RECEIPT`; the REPLAY rings from `observability/hooks#HOOKS`; the install receipts from their `observability/telemetry#TELEMETRY`, `observability/metrics#METRIC`, `observability/logging#PIPELINE`, and `observability/profiles#PROFILES` owners; the measurement reading from that telemetry owner's `snapshot`; and the verdict projection as data off the `execution/workers#SUPERVISION` accessor.

## [01]-[INDEX]

- [02]-[BUNDLE]: one fenced collectors table, redaction-then-encode archive fold, and content-keyed `Bundle` evidence.

## [02]-[BUNDLE]

- Owner: `Collector` is one capture row — name, availability gate, collect — and `COLLECTORS` the closed roster every capture folds; `Bundle` carries the archive body beside its `ContentKey` and the collected/skipped rosters, contributing key, byte length, and roster counts to the receipt stream while the body stays bytes — the key correlates two captures on a log line, the archive itself never rides one.
- Cases: a gate-closed row (the heap row with no tracer running) skips silently into the roster; a raising collector converts through the `boundary` fence into a `rejected` receipt under `bundle.<row>` and joins `skipped`; a collected row lands its redacted facts under its name in the one document. Archive finalization — deterministic encode, `zstd` compress, key mint — runs under its own `bundle.archive` fence, so `capture` returns `RuntimeRail[Bundle]` and a finalization fault lands as a rejected receipt beside the rail's refusal. Self-emission rides a SECOND fence outside that one: a wedged sink is the condition a bundle gets pulled under, so the drained line stays evidence OF a capture rather than a term in it and a built archive survives a render or sink fault whole.
- Entry: `capture(subject, *, selected, redaction)` is the one fold — an empty selection runs every row and a named selection bounds the roster. `Subject` carries the admitted-context render, verdict thunk, and scope as one value, so replay and emitted evidence stay inside the calling composition while the static table remains closed.
- Auto: the document encodes through the receipts-owned deterministic `ENCODE`, so key order is stable and the `ContentKey` replays across captures of identical state; `zstd.compress` bounds the archive body; redaction applies per collector BEFORE encoding and classifies by key name at EVERY depth, so the caller-supplied context, the verdict facts, and the nested `_installs` receipt maps and `_replay` hook rings all scrub in place even under a permissive sink; the capture self-emits its `Bundle` facts through the contributor stream, so every pull leaves a drained line beside the built archive.
- Law: every fence resolves ONE `reliability/faults#FAULT` `RAISES` anchor under `RuntimeLeg.BUNDLE`; the collector name stays on the emitted receipt where an operator reads it, so a per-collector fence subject bought a coordinate the receipt already carries. Two fences keep a catch-all and state why — a collector body and the self-emitting sink are both the plane a capsule gets pulled UNDER, so neither may raise past `capture`.
- Growth: a new evidence source is one `Collector` row; a new capture input is one `Subject` field; a new redaction transform stays the receipts owner's `Scrub` growth.
- Boundary: collection never starts an agent, thread, tracer, or sampling loop — the profilers stay the admitted owners, the heap gate reads, never arms, `tracemalloc`, and the readings row reads, never mounts, the diagnostic reader whose arming is the composition's `SignalProfile` value. `memray` is DECLINED on that same law — its allocation profiler arms a tracker the capture then owns, the exact agent this row forecloses — so the heap artifact stays the read-only `tracemalloc` ranking and the continuous rail stays `pyroscope-io`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import faulthandler
import json
import tracemalloc
from collections.abc import Callable, Iterable
from tempfile import TemporaryFile
from typing import Final, assert_never

import compression.zstd as zstd
from expression import Result
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.runtime.faults import BUNDLE_ARCHIVE, BUNDLE_COLLECT, BUNDLE_EMIT, RuntimeRail, boundary
from rasm.runtime.hooks import Hooks
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.logging import LogPipeline
from rasm.runtime.metrics import Instrumentation, Metrics
from rasm.runtime.profiles import Profiles
from rasm.runtime.receipts import DEFAULT_SCOPE, ENCODE, OPEN, EventDict, Receipt, Redaction, ScopeKey, Signals
from rasm.runtime.telemetry import Telemetry

# --- [TYPES] ----------------------------------------------------------------------------

type Verdicts = Callable[[], Map[str, str]]
type Collect = Callable[["Subject"], EventDict]

# --- [CONSTANTS] ------------------------------------------------------------------------


HEAP_ROWS: Final[int] = 64  # output-row cap; snapshot and statistics still scan the full traced allocation set

# --- [MODELS] ---------------------------------------------------------------------------


class Subject(Struct, frozen=True):
    # per-capture inputs crossing as one value, so the COLLECTORS table stays closed while per-daemon data varies:
    # `facts` is the admitted-context render, `verdicts` the supervisor projection thunk, and `scope` the mounting composition.
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

    def contribute(self) -> Iterable[Receipt]:
        # roster counts and the key alone reach the line — the archive body never rides a receipt.
        facts: dict[str, object] = {"key": self.key.project("hex"), "bytes": len(self.body), "collected": len(self.collected), "skipped": len(self.skipped)}
        return (Receipt.of("runtime.bundle", ("emitted", "capture", facts)),)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _dumped(dump: Callable[..., None], **kwargs: object) -> str:
    # Exemption: faulthandler writes at file-descriptor grain, so the dump lands in a real temporary file and reads
    # back whole — the fd bracket is the platform-forced seam no expression reaches; the file dies with the block.
    with TemporaryFile(mode="w+") as sink:
        dump(file=sink, **kwargs)
        sink.seek(0)
        return sink.read()


def _stacks(_: Subject) -> EventDict:
    return {"threads": _dumped(faulthandler.dump_traceback, all_threads=True)}


def _native(_: Subject) -> EventDict:
    return {"frames": _dumped(faulthandler.dump_c_stack)}


def _heap(_: Subject) -> EventDict:
    # Reads an ALREADY-tracing tracemalloc; HEAP_ROWS caps output only.
    return {"rows": tuple(str(stat) for stat in tracemalloc.take_snapshot().statistics("lineno")[:HEAP_ROWS])}


def _replay(subject: Subject) -> EventDict:
    # the window's OWN accounting rides beside its facts: a capsule reading a trimmed ring as whole under-reports
    # exactly what pressure evicted, so `Ring.facts()` publishes the cap, the retained count, and both loss counters.
    return {
        point.value: {"facts": tuple(structs.asdict(fact) for fact in ring.held), **ring.facts()}
        for point, ring in Hooks.replayed(scope=subject.scope).items()
    }


def _isolated(subject: Subject) -> EventDict:
    # subscriber faults the emitter's rail cannot carry BY LAW live in exactly one place — the registry's isolation
    # window — so a capsule pulled under a wedged tap is what separates a broken subscriber from a broken producer.
    # Receipts project through their own total fold rather than an `asdict` the union does not answer.
    ring = Hooks.faults(scope=subject.scope)
    return {
        "receipts": tuple({"level": level, "event": event, **facts} for level, event, facts in (row.project() for row in ring.held)),
        **ring.facts(),
    }


def _installs(subject: Subject) -> EventDict:
    # every process-custody owner projects its receipt as facts through the one `receipt() -> Option[…]` accessor they
    # share; an uninstalled owner renders empty rather than absent, so the archive always answers "what was installed"
    # with a total roster. The logging row is what makes a capture answer which floor, egress arm, payload caps, and
    # composition roster the lines beside it were written under. Producer-folder legs deposit their own receipts on the
    # scoped `Hooks` ledger — runtime imports no producer folder, so that registry is where a `GraduationInstall` or a
    # compute point block reaches an archive at all, and an absent producer row is the diagnosis that its leg never ran.
    held = {
        "telemetry": Telemetry.receipt(),
        "metrics": Metrics.receipt(),
        "instrumentation": Instrumentation.receipt(),
        "logging": LogPipeline.receipt(),
        "profiles": Profiles.receipt(),
    }
    process = {owner: receipt.map(structs.asdict).default_value({}) for owner, receipt in held.items()}
    return process | {owner: dict(structs.asdict(receipt)) for owner, receipt in Hooks.installs(scope=subject.scope).items()}


def _readings(_: Subject) -> EventDict:
    # Bundles get pulled exactly when the exporter, collector, or store is what failed, so measurement evidence
    # reads the composition-root diagnostic reader and no backend. `to_json` is the SDK's own projection of its own
    # tree, decoded back to a mapping so redaction classifies at every depth — a rendered string would carry the
    # tenant attribute values past the scrub. The gate is what makes an unarmed profile a SKIPPED roster entry
    # rather than an empty document that reads like a process measuring nothing.
    data = Telemetry.snapshot()
    return {"metrics": data.map(lambda tree: json.loads(tree.to_json(indent=None))).default_value({})}


def _context(subject: Subject) -> EventDict:
    return dict(subject.facts)


def _verdicts(subject: Subject) -> EventDict:
    return dict(subject.verdicts().items())


def _always() -> bool:
    return True


# --- [TABLES] ---------------------------------------------------------------------------

# one closed capture roster: the gate answers availability, collect runs fenced inside `capture`; the heap row's
# gate is the tracemalloc tracer state itself, so the pull-driven law is a table fact rather than a branch.
COLLECTORS: Final[Block[Collector]] = Block.of_seq([
    Collector("stacks", _always, _stacks),
    Collector("native", _always, _native),
    Collector("heap", tracemalloc.is_tracing, _heap),
    Collector("replay", _always, _replay),
    Collector("isolated", _always, _isolated),  # the hook registry's counted subscriber-fault window, the one plane the rail cannot carry
    Collector("installs", _always, _installs),  # process-custody owners beside the scope's producer-install ledger
    # gate reads the ARMING off the install receipt rather than probing the reader: probing collects the whole
    # tree once to answer a boolean, then the row collects it again.
    Collector("readings", lambda: Telemetry.receipt().map(lambda r: r.signal_profile.diagnostic_read).default_value(False), _readings),
    Collector("context", _always, _context),
    Collector("verdicts", _always, _verdicts),
])

# --- [SERVICES] -------------------------------------------------------------------------


class SupportBundle:
    @staticmethod
    def capture(subject: Subject, *, selected: tuple[str, ...] = (), redaction: Redaction = OPEN) -> RuntimeRail[Bundle]:
        # one archive fold: roster-bounded rows run fenced, redaction lands per collector BEFORE encode, a refusing
        # collector joins `skipped` beside its rejected receipt, and the deterministic encode keys the capsule.
        known = frozenset(row.name for row in COLLECTORS)
        roster = COLLECTORS if not selected else COLLECTORS.filter(lambda row: row.name in selected)
        absent = Block.of_seq(name for name in selected if name not in known)

        def folded(acc: tuple[Map[str, EventDict], Block[str], Block[str]], row: Collector) -> tuple[Map[str, EventDict], Block[str], Block[str]]:
            document, collected, skipped = acc
            match boundary(BUNDLE_COLLECT, lambda: redaction.apply(row.collect(subject)) if row.gated() else None, catch=Exception):
                case Result(tag="ok", ok=None):
                    return document, collected, skipped.append(Block.singleton(row.name))
                case Result(tag="ok", ok=facts):
                    return document.add(row.name, facts), collected.append(Block.singleton(row.name)), skipped
                case Result(tag="error", error=fault):
                    Signals.emit(Receipt.of(f"bundle.{row.name}", fault), OPEN, scope=subject.scope)
                    return document, collected, skipped.append(Block.singleton(row.name))
                case _ as unreachable:
                    assert_never(unreachable)

        document, collected, skipped = roster.fold(folded, (Map.empty(), Block.empty(), absent))

        def archived() -> Bundle:
            body = zstd.compress(ENCODE({name: facts for name, facts in document.items()}))
            return Bundle(key=ContentIdentity.key("bundle", body), body=body, collected=tuple(collected), skipped=tuple(skipped))

        # finalization rides its own fence: an encode, compress, or key-mint fault returns on the rail as a rejected
        # receipt beside the collector evidence, never a raise past the capture. Self-emission rides a SECOND fence
        # OUTSIDE it — a wedged sink is the condition a bundle gets pulled under, so a render or sink fault
        # never voids a built archive, and that fault stays unreported precisely because the reporting path is what broke.
        outcome = boundary(BUNDLE_ARCHIVE, archived, catch=(zstd.ZstdError, TypeError, ValueError))
        outcome.swap().map(lambda fault: Signals.emit(Receipt.of("bundle.archive", fault), OPEN, scope=subject.scope))
        outcome.map(lambda bundle: boundary(BUNDLE_EMIT, lambda: Signals.emit(bundle, redaction, scope=subject.scope), catch=Exception))
        return outcome

```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
