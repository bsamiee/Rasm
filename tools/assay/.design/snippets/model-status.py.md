# [H1][SNIPPET_MODEL_STATUS]
>**Dictum:** *Wave 3 production spine for `core/status.py` + `core/model.py`; one vocabulary instance, one receipt algebra, one error channel.*

Authoritative over stale shards (`research-msgspec.md` §6 capture-only `Mode`, `research-holistic-shapes.md` `tag=str.lower`). Resolves CRITIQUE-SHAPES P0.1–P0.3 in-code. Engine surface names: `run_check`, `fan_out`, `fold` — no `Engine`/`Parser` Protocols.

| [P0] | [SNIPPET LAW] |
| --- | --- |
| Completed receipt | `Completed.status` + `notes`; fold reads outcomes, never phantom `Completed.artifacts`. |
| Fault channel | Spawn/lease/timeout → `Fault` → `Envelope.error`; process exit → `receipt()` → `Completed` + `from_returncode`. |
| Detail tags | Explicit `tag="verify"` … — never `tag=str.lower`. |
| Counts | `Report.counts` only; `VerifySummary` holds non-derivable bridge fields. |

Snippet truncates non-pin fields (`Language`, `PackageRun`, extra `Mode` members, `Envelope` metadata) — full shapes stay in `model.md`.

---
## [1][SPINE]

```python
from collections.abc import Callable
from enum import StrEnum
from functools import reduce
from typing import Annotated, Self
import msgspec

class RailStatus(StrEnum):
    exit_code: int
    severity: int
    SKIP = "skip", 0, 0, "skipped"
    EMPTY = "empty", 0, 1
    OK = "ok", 0, 2
    UNSUPPORTED = "unsupported", 3, 3
    BUSY = "busy", 5, 4
    TIMEOUT = "timeout", 5, 5
    FAILED = "failed", 1, 6

    def __new__(cls, value: str, exit_code: int, severity: int, *aliases: str) -> Self:
        m = str.__new__(cls, value)
        m._value_, m.exit_code, m.severity = value, exit_code, severity
        for a in aliases:
            m._add_value_alias_(a)
        return m

    @classmethod
    def from_returncode(cls, rc: int) -> RailStatus:
        match rc:
            case 0: return cls.EMPTY
            case 5: return cls.BUSY
            case 124: return cls.TIMEOUT
            case _: return cls.FAILED

    def join(self, other: RailStatus) -> RailStatus:
        return self if self.severity >= other.severity else other

    @staticmethod
    def fold(*members: RailStatus) -> RailStatus:
        return reduce(lambda a, b: a.join(b), members, RailStatus.EMPTY)

class Base(msgspec.Struct, frozen=True, gc=False, omit_defaults=True, repr_omit_defaults=True): ...
class Detail(Base, forbid_unknown_fields=True, tag_field="kind"): ...

class Mode(StrEnum):
    stream: bool
    writes: bool
    CHECK = "check", False, False
    VERIFY = "verify", True, False

    def __new__(cls, value: str, stream: bool, writes: bool) -> Self:
        m = str.__new__(cls, value)
        m._value_, m.stream, m.writes = value, stream, writes
        return m

class Claim(StrEnum):
    STATIC, TEST, BRIDGE, PACKAGE, API, DOCS = "static", "test", "bridge", "package", "api", "docs"

class ArtifactKind(StrEnum):
    LOCKS, PROCESS, TEST, MUTATION, RHINO, SCOPE = "locks", "process", "test", "mutation", "rhino", "scope"

type Parser = Callable[["Completed"], Detail | None]

class Completed(Base):
    argv: tuple[str, ...]
    returncode: int
    stdout: bytes = b""
    stderr: bytes = b""
    status: RailStatus = RailStatus.EMPTY
    notes: tuple[str, ...] = ()

class Fault(Base):
    argv: tuple[str, ...]
    status: RailStatus = RailStatus.FAILED
    message: str = ""

def receipt(argv: tuple[str, ...], rc: int, *, stdout: bytes = b"", stderr: bytes = b"",
            status: RailStatus | None = None, notes: tuple[str, ...] = ()) -> Completed:
    return Completed(argv, rc, stdout, stderr, status or RailStatus.from_returncode(rc), notes)

class Counts(Base):
    ok: int = 0
    failed: int = 0
    total: int = 0

class VerifySummary(Detail, tag="verify"):
    exceptions: int = 0
    report_dir: str = ""
    first_failure: str = ""

class ApiSurface(Detail, tag="api"):
    source_kind: str = ""
    source_id: str = ""
    version: str = ""

class Artifact(Base):
    id: str
    kind: ArtifactKind
    path: str

class Match(Base):
    id: str
    kind: str
    text: str

class Report(Base):
    claim: Claim
    verb: str
    status: RailStatus = RailStatus.OK
    counts: Counts = Counts()
    artifacts: tuple[Artifact, ...] = ()
    results: tuple[Match, ...] = ()
    notes: tuple[str, ...] = ()
    detail: ApiSurface | VerifySummary | None = None

class Envelope(Base, omit_defaults=False, kw_only=True):
    schema_version: int = 1
    claim: Claim
    verb: str
    status: RailStatus = RailStatus.OK
    exit_code: int = 0
    report: Report | None = None
    error: Fault | None = None

def _count(done: Completed) -> tuple[int, int]:
    match done.status:
        case RailStatus.OK | RailStatus.EMPTY | RailStatus.SKIP: return 1, 0
        case RailStatus.FAILED: return 0, 1
        case _: return 0, 0

def fold(claim: Claim, verb: str, outcomes: tuple[Completed, ...], *, detail: Detail | None = None) -> Report:
    pairs = tuple(map(_count, outcomes))
    ok_n, fail_n = (sum(a) for a in zip(*pairs)) if pairs else (0, 0)
    return Report(claim, verb, RailStatus.fold(*(o.status for o in outcomes)), Counts(ok_n, fail_n, ok_n + fail_n),
                  notes=tuple(n for o in outcomes for n in o.notes), detail=detail)

def envelope(payload: Report | Fault, *, claim: Claim, verb: str) -> Envelope:
    match payload:
        case Report() as r:
            return Envelope(claim=claim, verb=verb, status=r.status, exit_code=r.status.exit_code, report=r)
        case Fault() as f:
            return Envelope(claim=claim, verb=verb, status=f.status, exit_code=f.status.exit_code, error=f)
```

---
## [2][ACCEPTANCE]

[VERIFY] Gate before `engine.py` consumes these shapes:

- [ ] `RailStatus`: `encode(decode(x)) is x`; alias `"skipped"` → `SKIP`; `from_returncode` never yields `OK`/`UNSUPPORTED`.
- [ ] `join`/`fold`: associative on severity; seed `EMPTY` for rail-level vacuous; `FAILED` absorbs `TIMEOUT`/`BUSY`.
- [ ] `Mode`: every member carries `stream` + `writes`; full 13 members in `model.md`; no `CAPTURE`/`STREAM`/`Tool.mutates`.
- [ ] `Parser`: `Tool.parser` maps `Completed` → `Detail | None`; parsers may set `Completed.status=OK`.
- [ ] `Language.strategy` is sole route discriminant (`model.md`); no `Strategy` enum.
- [ ] `TestRun`/`PackageRun` use `tag="test"`/`tag="package"` (`model.md`); snippet shows `verify` + `api` tags.
- [ ] `Completed` carries `status`/`notes`; process non-zero → `receipt`, not `Fault`.
- [ ] `Fault` only for operational failures; rides `Envelope.error`, never `Report.detail`.
- [ ] `Detail` decodes with explicit tags (`verify`, `test`, `package`, `api`); unknown fields → `ValidationError`.
- [ ] `Counts` only on `Report`; detail variants omit derivable ok/failed/total.
- [ ] `envelope`: polymorphic `Report | Fault`; success sets `report=`, failure sets `error=`; `exit_code` from `status.exit_code`.
- [ ] No second status type, no `Literal` vocab alias, no `Engine`/`Parser` Protocol — `Parser` type alias + module `fold`/`receipt` only.

---
## [FURTHER_CONSIDERATION]

- Propagate enriched `Completed` back into `model.md` §3 and retire `engine.md` non-zero→`Fault` prose.
- `Match.kind: str` remains P1; consider `MatchKind` before parser tables land in `catalog.md`.
- Esoteric: module-level `msgspec.json.Encoder`/`Decoder(Report)` cache — add to wire-law tests per `TYPE_SYSTEM.md` §7.
