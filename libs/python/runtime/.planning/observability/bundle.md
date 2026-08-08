# [PY_RUNTIME_BUNDLE]

`SupportBundle` folds the daemon's whole evidence state into one pull-driven diagnostic capsule — the C# support-bundle peer at Python grain. One `COLLECTORS` table owns the capture surface — interpreter stacks and the native frame, the gated heap ranking, every hook REPLAY window, the install-receipt roster, the backend-free measurement reading, the admitted-context render, the supervision verdict — each row fenced, so a refusing collector lands as a skipped roster entry beside its rejected receipt, never a failed capture. Archive encoding passes collector facts through the receipts-owned `Redaction`, compresses with `compression.zstd`, and mints the `ContentIdentity.key`; identical state keys identically.

Capture starts nothing and serializes whole-capsule cost through one in-flight band. Heap analysis reads only an already-tracing `tracemalloc`; snapshot and ranking cost still scale with the traced allocation set, while `HEAP_ROWS` caps only the emitted ranking. Replay rings arrive pre-trimmed to their registered `HookPoint.buffer`, and the stack dump spans exactly the live thread set — no sampling loop lands beside the admitted profilers. `Redaction`/`OPEN`, `Receipt`, `ENCODE`, and the fault fences arrive settled from `observability/receipts#RECEIPT`; the REPLAY rings from `observability/hooks#HOOKS`; the install receipts from their `observability/telemetry#TELEMETRY`, `observability/metrics#METRIC`, `observability/logging#PIPELINE`, and `observability/profiles#PROFILES` owners, the measurement reading from that telemetry owner's `snapshot`; the wire pair from `transport/shapes#VOCABULARY`; the verdict projection as data off the `execution/workers#SUPERVISION` accessor. Serve mounts one diagnostic `Route` through `SupportBundle.handler`, and the shapes registry proves all four of its wire facts against the compiled descriptors before the first RPC — the request and reply rows, and the service and rpc names it dials under — so a producer-side rename refuses at boot rather than at a pull.

## [01]-[INDEX]

- [02]-[BUNDLE]: one fenced collectors table, redaction-then-encode archive fold, content-keyed `Bundle` evidence, and the serve-facing handler.

## [02]-[BUNDLE]

- Owner: `Collector` is one capture row — name, availability gate, collect — and `COLLECTORS` the closed roster every capture folds; `Bundle` carries the archive body beside its `ContentKey` and the collected/skipped rosters, contributing key, byte length, and roster counts to the receipt stream while the body stays bytes — the key correlates two captures on a log line, the archive itself never rides one.
- Cases: a gate-closed row (the heap row with no tracer running) skips silently into the roster; a raising collector converts through the `boundary` fence into a `rejected` receipt under `bundle.<row>` and joins `skipped`; a collected row lands its redacted facts under its name in the one document. Archive finalization — deterministic encode, `zstd` compress, key mint — runs under its own `bundle.archive` fence, so `capture` returns `RuntimeRail[Bundle]`, a finalization fault lands as a rejected receipt beside the rail's refusal, and the handler projects the rail instead of throwing past the route. Self-emission rides a SECOND fence outside that one: a wedged sink is the condition a bundle gets pulled under, so the drained line stays evidence OF a capture rather than a term in it and a built archive survives a render or sink fault whole.
- Entry: `capture(subject, *, selected, redaction)` is the one fold — an empty selection runs every row, a named selection bounds the roster — and `handler(verdicts, redaction, *, scope)` binds the capture into the serve-shaped async callable the composition root mounts as the diagnostic `Route`, offloading the dump-and-compress body through one single-token band so a capture never stalls the event loop and a concurrent second pull queues instead of doubling the dump cost. That band reports itself: the capture bracket registers the module-level `_capturing` probe under `band="capture"`, so queue depth reads off the standing occupancy level instead of being inferred from a reply that has not come back, and probe identity is what keeps concurrent pulls one reading of one limiter rather than N copies of it. `Subject` carries the admitted-context render, verdict thunk, and scope as one value, so replay and emitted evidence stay inside the mounting composition while the static table remains closed.
- Auto: the document encodes through the receipts-owned deterministic `ENCODE`, so key order is stable and the `ContentKey` replays across captures of identical state; `zstd.compress` bounds the wire body; redaction applies per collector BEFORE encoding and classifies by key name at EVERY depth, so the caller-supplied context, the verdict facts, and the nested `_installs` receipt maps and `_replay` hook rings all scrub in place even under a permissive sink; the capture self-emits its `Bundle` facts through the contributor stream, so every pull leaves a drained line beside the served bytes.
- Growth: a new evidence source is one `Collector` row; a new capture input is one `Subject` field; a new redaction class stays the receipts owner's `Classification` growth; the wire pair grows only at the shapes registry, and the served service and rpc only at its `SERVICE_VOCABULARY`; a new route fact this owner genuinely holds is one constant beside `BUNDLE_WIRE`.
- Boundary: collection never starts an agent, thread, tracer, or sampling loop — the profilers stay the admitted owners, the heap gate reads, never arms, `tracemalloc`, and the readings row reads, never mounts, the diagnostic reader whose arming is the composition's `SignalProfile` value — and the capsule serves only through the registered diagnostic route; the calling host pulls over the standing wire and re-mints nothing. `memray` is DECLINED on that same law — its allocation profiler arms a tracker the capture then owns, the exact agent this row forecloses — so the heap artifact stays the read-only `tracemalloc` ranking and the continuous rail stays `pyroscope-io`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import faulthandler
import json
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
from rasm.runtime.logging import LogPipeline
from rasm.runtime.metrics import Instrumentation, Metrics
from rasm.runtime.profiles import Profiles
from rasm.runtime.receipts import DEFAULT_SCOPE, ENCODE, OPEN, EventDict, Receipt, Redaction, ScopeKey, Signals
from rasm.runtime.shapes import SupportBundleReply, SupportBundleRequest
from rasm.runtime.telemetry import Telemetry

# --- [TYPES] ----------------------------------------------------------------------------

type Verdicts = Callable[[], Map[str, str]]
type Collect = Callable[["Subject"], EventDict]

# --- [CONSTANTS] ------------------------------------------------------------------------

# serve-facing route facts the composition root mounts; the wire pair names the shapes registry rows. The dialed
# service and rpc are NOT here: both are `transport/shapes#REGISTRY_AND_DRIFT` `SERVICE_VOCABULARY` members the boot
# gate resolves against the compiled descriptor, and a local literal beside them dialed a name no descriptor set
# emits — a miss no boot could see and no pull answered with anything but UNIMPLEMENTED.
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


def _capturing() -> int:
    # the band's own borrowed-slot read, and MODULE-LEVEL identity is the whole contract: `Metrics.occupied` keys its
    # registration on the probe OBJECT, so every concurrent pull enrols THIS one object and `rasm.band.in_flight`
    # reports one reading of one limiter. A per-call closure would enrol a fresh row per queued pull and sum the same
    # borrowed count once per row, publishing a saturation the process never reached.
    return _CAPTURE_BAND.borrowed_tokens


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
    return {point: tuple(structs.asdict(fact) for fact in ring) for point, ring in Hooks.replayed(scope=subject.scope).items()}


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
            return Bundle(key=ContentIdentity.key("bundle", body), body=body, collected=tuple(collected), skipped=tuple(skipped))

        # finalization rides its own fence: an encode, compress, or key-mint fault returns on the rail as a rejected
        # receipt beside the collector evidence, never a raise past the capture. Self-emission rides a SECOND fence
        # OUTSIDE it — a wedged sink is the condition a bundle gets pulled under, so a render or sink fault
        # never voids a built archive, and that fault stays unreported precisely because the reporting path is what broke.
        outcome = boundary("bundle.archive", archived)
        outcome.swap().map(lambda fault: Signals.emit(Receipt.of("bundle.archive", fault), OPEN, scope=subject.scope))
        outcome.map(lambda bundle: boundary("bundle.emitted", lambda: Signals.emit(bundle, redaction, scope=subject.scope)))
        return outcome

    @staticmethod
    def handler(
        verdicts: Verdicts, redaction: Redaction = OPEN, *, scope: ScopeKey = DEFAULT_SCOPE
    ) -> Callable[[SupportBundleRequest, RuntimeContext], Awaitable[RuntimeRail[SupportBundleReply]]]:
        # serve-shaped bind: the admitted context renders into the capture subject, the dump-and-compress body offloads
        # under the one-token band, and the reply projects the capsule onto the wire pair.
        async def captured(request: SupportBundleRequest, context: RuntimeContext) -> RuntimeRail[SupportBundleReply]:
            subject = Subject(facts={key: str(value) for key, value in context.attribute().items()}, verdicts=verdicts, scope=scope)
            # the band publishes its own occupancy for the bracket that holds it, so an operator reading a stalled
            # capture sees the queue depth on the standing level rather than inferring it from a missing reply. The
            # registration retires with the bracket, and a band no pull holds publishes NO point at all.
            with Metrics.occupied(_capturing, band="capture", scope=scope):
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

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
