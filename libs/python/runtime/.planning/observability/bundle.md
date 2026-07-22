# [PY_RUNTIME_BUNDLE]

`SupportBundle` folds the daemon's whole evidence state into one pull-driven diagnostic capsule — the C# support-bundle peer at Python grain. One `COLLECTORS` table owns the capture surface: interpreter stacks and the native frame through `faulthandler`, the gated managed-heap ranking through `tracemalloc`, every hook REPLAY window, the install-receipt roster off the four install owners, the admitted-context render, and the supervision verdict projection — each row fenced, so a refusing collector lands as a skipped roster entry beside its rejected receipt, never a failed capture. Archive encoding passes collector facts through the receipts-owned `Redaction`, compresses with `compression.zstd`, and mints the `ContentIdentity.key`; identical state keys identically.

Capture starts nothing and serializes whole-capsule cost through one in-flight band. Heap analysis reads only an already-tracing `tracemalloc`; snapshot and ranking cost still scale with the traced allocation set, while `HEAP_ROWS` caps only the emitted ranking. Replay rings arrive pre-trimmed to their registered `HookPoint.buffer`, and the stack dump spans exactly the live thread set — no sampling loop lands beside the admitted profilers. `Redaction`/`OPEN`, `Receipt`, `ENCODE`, and the fault fences arrive settled from `observability/receipts#RECEIPT`; the REPLAY rings from `observability/hooks#HOOKS`; the install receipts from their `observability/telemetry#TELEMETRY`, `observability/metrics#METRIC`, and `observability/profiles#PROFILES` owners; the wire pair from `transport/shapes#VOCABULARY`; the verdict projection crosses in as data off the `execution/workers#SUPERVISION` accessor. Serve mounts one diagnostic `Route` through `SupportBundle.handler`; the shapes research row owns the missing producer trigger that makes registration refuse at boot.

## [01]-[INDEX]

- [01]-[BUNDLE]: the fenced collectors table, the redaction-then-encode archive fold, the content-keyed `Bundle` evidence, and the serve-facing handler.

## [02]-[BUNDLE]

- Owner: `Collector` is one capture row — name, availability gate, collect — and `COLLECTORS` the closed roster every capture folds; `Bundle` carries the archive body beside its `ContentKey` and the collected/skipped rosters, contributing key, byte length, and roster counts to the receipt stream while the body stays bytes — the key correlates two captures on a log line, the archive itself never rides one.
- Cases: a gate-closed row (the heap row with no tracer running) skips silently into the roster; a raising collector converts through the `boundary` fence into a `rejected` receipt under `bundle.<row>` and joins `skipped`; a collected row lands its redacted facts under its name in the one document. Archive finalization — deterministic encode, `zstd` compress, key mint, and the capsule's own emission — runs under its own `bundle.archive` fence, so `capture` returns `RuntimeRail[Bundle]`, a finalization fault lands as a rejected receipt beside the rail's refusal, and the handler projects the rail instead of throwing past the route.
- Entry: `capture(subject, *, selected, redaction)` is the one fold — an empty selection runs every row, a named selection bounds the roster — and `handler(verdicts, redaction, *, scope)` binds the capture into the serve-shaped async callable the composition root mounts as the diagnostic `Route`, offloading the dump-and-compress body through one single-token band so a capture never stalls the event loop and a concurrent second pull queues instead of doubling the dump cost. `Subject` carries the admitted-context render, verdict thunk, and scope as one value, so replay and emitted evidence stay inside the mounting composition while the static table remains closed.
- Auto: the document encodes through the receipts-owned deterministic `ENCODE`, so key order is stable and the `ContentKey` replays across captures of identical state; `zstd.compress` bounds the wire body; redaction applies per collector BEFORE encoding, so a classified field never reaches the archive even under a permissive sink; the capture self-emits its `Bundle` facts through the contributor stream, so every pull leaves a drained line beside the served bytes.
- Growth: a new evidence source is one `Collector` row; a new capture input is one `Subject` field; a new redaction class stays the receipts owner's `Classification` growth; the wire pair grows only at the shapes registry; a new route fact is one constant beside `BUNDLE_WIRE`.
- Boundary: collection never starts an agent, thread, tracer, or sampling loop — the profilers stay the admitted owners and the heap gate reads, never arms, `tracemalloc` — and the capsule serves only through the registered diagnostic route; the C# host pulls over the standing wire and re-mints nothing.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import faulthandler
import tracemalloc
from collections.abc import Awaitable, Callable, Iterable
from functools import partial
from tempfile import TemporaryFile
from typing import Final, assert_never

import anyio.to_thread
import compression.zstd as zstd
from anyio import CapacityLimiter
from expression import Result
from expression.collections import Block, Map
from msgspec import Struct, structs

from rasm.runtime.admission import RuntimeContext
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.hooks import Hooks
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.metrics import Instrumentation, Metrics
from rasm.runtime.profiles import Profiles
from rasm.runtime.receipts import DEFAULT_SCOPE, ENCODE, OPEN, EventDict, Receipt, Redaction, ScopeKey, Signals
from rasm.runtime.shapes import SupportBundleReply, SupportBundleRequest
from rasm.runtime.telemetry import Telemetry

# --- [TYPES] ----------------------------------------------------------------------------

type Verdicts = Callable[[], Map[str, str]]
type Collect = Callable[["Subject"], EventDict]

# --- [CONSTANTS] ------------------------------------------------------------------------

# serve-facing route facts the composition root mounts; the wire pair names the shapes registry rows.
BUNDLE_SERVICE: Final[str] = "rasm.runtime.Diagnostic"
BUNDLE_METHOD: Final[str] = "CaptureBundle"
BUNDLE_DESCRIPTOR: Final[str] = "rasm.runtime.diagnostic/capture"
BUNDLE_WIRE: Final[tuple[str, str]] = ("support_bundle", "support_bundle_reply")

HEAP_ROWS: Final[int] = 64  # output-row cap; snapshot and statistics still scan the full traced allocation set

# one in-flight capture: the dump-and-compress body rides a worker thread, and a concurrent second pull queues here.
_CAPTURE_BAND: Final[CapacityLimiter] = CapacityLimiter(1)

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
    # Reads an ALREADY-tracing tracemalloc; the capture band serializes full-snapshot cost, and HEAP_ROWS caps output only.
    return {"rows": tuple(str(stat) for stat in tracemalloc.take_snapshot().statistics("lineno")[:HEAP_ROWS])}


def _replay(subject: Subject) -> EventDict:
    return {point: tuple(dict(structs.asdict(fact)) for fact in ring) for point, ring in Hooks.replayed(scope=subject.scope).items()}


def _installs(_: Subject) -> EventDict:
    # four install owners project process receipts as facts; an uninstalled owner renders empty rather than absent,
    # so the archive always answers "what was installed" with a total roster.
    held = {
        "telemetry": Telemetry.receipt(),
        "metrics": Metrics.receipt(),
        "instrumentation": Instrumentation.receipt(),
        "profiles": Profiles.receipt(),
    }
    return {owner: receipt.map(lambda live: dict(structs.asdict(live))).default_value({}) for owner, receipt in held.items()}


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
    Collector("installs", _always, _installs),
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
            match boundary(f"bundle.{row.name}", lambda: redaction.apply(row.collect(subject)) if row.gated() else None):
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
            bundle = Bundle(key=ContentIdentity.key("bundle", body), body=body, collected=tuple(collected), skipped=tuple(skipped))
            Signals.emit(bundle, redaction, scope=subject.scope)
            return bundle

        # finalization rides its own fence: an encode, compress, key-mint, or emission fault returns on the rail as a
        # rejected receipt beside the collector evidence, never a raise past the capture.
        outcome = boundary("bundle.archive", archived)
        outcome.swap().map(lambda fault: Signals.emit(Receipt.of("bundle.archive", fault), OPEN, scope=subject.scope))
        return outcome

    @staticmethod
    def handler(
        verdicts: Verdicts, redaction: Redaction = OPEN, *, scope: ScopeKey = DEFAULT_SCOPE
    ) -> Callable[[SupportBundleRequest, RuntimeContext], Awaitable[RuntimeRail[SupportBundleReply]]]:
        # serve-shaped bind: the admitted context renders into the capture subject, the dump-and-compress body offloads
        # under the one-token band, and the reply projects the capsule onto the wire pair.
        async def captured(request: SupportBundleRequest, context: RuntimeContext) -> RuntimeRail[SupportBundleReply]:
            subject = Subject(facts={key: str(value) for key, value in context.attribute().items()}, verdicts=verdicts, scope=scope)
            railed = await anyio.to_thread.run_sync(
                partial(SupportBundle.capture, subject, selected=tuple(request.collectors), redaction=redaction),
                abandon_on_cancel=True,
                limiter=_CAPTURE_BAND,
            )
            return railed.map(
                lambda bundle: SupportBundleReply(
                    content_key=bundle.key.project("hex"), archive=bundle.body, collected=bundle.collected, skipped=bundle.skipped
                )
            )

        return captured
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
