# [H1][MODEL_DESIGN]
>**Dictum:** *Five axis enums and one Base policy generate every shape; algorithm evidence is one tagged `detail`, never a new report struct.*

Collapses `tools/quality`'s ~25 `Literal` aliases, 14 report structs, and three model systems into behavior-carrying `StrEnum`s plus one `Base`. Verified against msgspec Structs reference. `RailStatus` is owned by `status.py`; `model.py` imports it and adds nothing status-shaped.

**Canonical:** [`TYPE_SYSTEM.md`](TYPE_SYSTEM.md) §3–§4 · [`snippets/model-status.py.md`](snippets/model-status.py.md) · [`AOT.md`](AOT.md) — wire shapes only; aspects never add fields here.

---
## [1][AXIS_ENUMS]
>**Dictum:** *One `StrEnum` per axis; `__new__` carries the behavior so the member is the CLI param, the wire value, and the dispatch key at once.*

```python
class Runner(StrEnum):
    prefix: tuple[str, ...]
    DIRECT = "direct", ()
    MODULE = "module", ("uv", "run", "python", "-m")
    UV = "uv", ("uv", "run")
    DOTNET = "dotnet", ("dotnet",)
    PNPM = "pnpm", ("pnpm", "exec")
    def __new__(cls, value: str, prefix: tuple[str, ...]) -> Self: ...

class Input(StrEnum):
    flag: tuple[str, ...]
    scoped: bool
    FILES = "files", (), False
    INCLUDE = "include", ("--include",), False
    PROJECT = "project", (), True
    SOLUTION = "solution", (), True
    GLOB = "glob", (), False
    NONE = "none", (), True

class Language(StrEnum):
    strategy: str
    suffixes: frozenset[str]
    CSHARP = "csharp", "closure", frozenset((".cs", ".csproj", ".props", ".targets"))
    PYTHON = "python", "glob", frozenset((".py", ".pyi"))
    TYPESCRIPT = "typescript", "glob", frozenset((".ts", ".tsx", ".cts", ".mts"))
    DOCS = "docs", "glob", frozenset((".md", ".mmd"))

class Mode(StrEnum):
    stream: bool
    writes: bool
    CHECK = "check", False, False
    WRITE = "write", False, True
    RESTORE = "restore", False, False
    BUILD = "build", True, False
    RUN = "run", False, False
    LIST = "list", False, False
    MUTATION = "mutation", True, False
    CLIENT = "client", False, False
    VERIFY = "verify", True, False
    QUERY = "query", False, False
    STAGE = "stage", False, False
    DEPLOY = "deploy", False, False
    PUBLISH = "publish", False, False
    def __new__(cls, value: str, stream: bool, writes: bool) -> Self: ...

class Claim(StrEnum):
    STATIC = "static"
    TEST = "test"
    BRIDGE = "bridge"
    PACKAGE = "package"
    API = "api"
    DOCS = "docs"

class ArtifactKind(StrEnum):
    LOCKS = "locks"
    PROCESS = "process"
    TEST = "test"
    MUTATION = "mutation"
    RHINO = "rhino"
    SCOPE = "scope"
```

`Language.strategy` replaces the standalone `Strategy` enum (`routing.md` merges `closure`/`glob` into `Language` only). Unified `Mode` carries operation kind plus `stream` (capture vs tee) and `writes` (mutating tools); retires separate capture-only `CAPTURE`/`STREAM` and `Tool.mutates`. `Tool.mode` defaults to `Mode.CHECK` after unify (`catalog.md`).

---
## [2][BASE_POLICY]
>**Dictum:** *Declare `frozen`/`gc=False`/`omit_defaults` once; never repeat config on ten structs.*

```python
class Base(msgspec.Struct, frozen=True, gc=False, omit_defaults=True):
    """Shared kwargs base. msgspec inherits config to subclasses."""
```

`forbid_unknown_fields` is **not** on `Base` (external bridge/C# JSON carries extras), only on `Detail` variants. `Envelope` overrides `omit_defaults=False` so `schema_version` is always emitted.

---
## [3][CANONICAL_STRUCTS]
>**Dictum:** *`Tool` is data; `Check` is its runnable binding; `Completed`/`Fault` are engine receipts; `Artifact`/`Match` are the only row shapes.*

```python
type Parser = Callable[[Completed], Detail | None]

class Tool(Base):
    name: str
    runner: Runner
    command: tuple[str, ...]
    input: Input
    language: Language
    claim: Claim
    mode: Mode = Mode.CHECK
    timeout: Annotated[float, msgspec.Meta(gt=0)] | None = None
    parser: Parser | None = None

class Check(Base):
    tool: Tool
    paths: tuple[str, ...] = ()
    owner: str = ""
    solution: str = ""
    glob: str = ""
    cwd: Path | None = None

class Completed(Base):
    argv: tuple[str, ...]
    returncode: int
    stdout: bytes = b""
    stderr: bytes = b""
    duration_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    status: RailStatus = RailStatus.EMPTY
    notes: tuple[str, ...] = ()   # per-check notes; fold concatenates into Report.notes

def receipt(argv: tuple[str, ...], rc: int, *, stdout: bytes = b"", stderr: bytes = b"",
            duration_ms: float = 0.0, status: RailStatus | None = None,
            notes: tuple[str, ...] = ()) -> Completed:
    """Process exit → Completed (never Fault). See snippets/model-status.py.md."""

class Fault(Base):
    argv: tuple[str, ...]
    status: RailStatus = RailStatus.FAILED
    returncode: int = 2
    message: str = ""
    stderr: str = ""

class Artifact(Base):
    id: str
    kind: ArtifactKind
    path: str
    bytes: Annotated[int, msgspec.Meta(ge=0)] = 0
    lines: Annotated[int, msgspec.Meta(ge=0)] = 0

class Match(Base):
    id: str
    kind: str
    text: str
    line: Annotated[int, msgspec.Meta(ge=0)] = 0
    score: int = 0

class Counts(Base):
    ok: int = 0
    failed: int = 0
    total: int = 0
```

`Check` inlines routed fields; `run_check(check, *, settings, scope, routed)` receives `ArtifactScope` and `Routed` as parameters (`engine.md` §1–§2). No `Engine` or `Parser` `Protocol` — module functions and `Callable` types only (`ARCHITECTURE.md` §5, backlog item 7).

---
## [4][REPORT_AND_DETAIL_UNION]
>**Dictum:** *One `Report` crosses every rail; algorithm evidence is one tagged `detail`, decoded in a single pass keyed by `kind`.*

```python
class Detail(Base, forbid_unknown_fields=True, tag_field="kind"):
    """Tagged base; subclasses pin explicit short wire tags."""

class ApiSurface(Detail, tag="api"):
    source_kind: str = ""
    source_id: str = ""
    version: str = ""
    shape: str = "search"
    signature: str = ""
    doc: str = ""
    preview: str = ""

class VerifySummary(Detail, tag="verify"):
    exceptions: int = 0
    report_dir: str = ""
    first_failure: str = ""
    # ok/failed/total live on Report.counts only (TYPE_SYSTEM.md §4; snippet §1)

class TestRun(Detail, tag="test"):
    mutation: str = "off"
    coverage: Annotated[float, msgspec.Meta(ge=0, le=100)] | None = None
    killed: int = 0
    survived: int = 0
    selected: int = 0

class PackageRun(Detail, tag="package"):
    stage: str = ""
    project: str = ""
    pattern: str = ""
    version: str = ""

class Report(Base):
    claim: Claim
    verb: str
    status: RailStatus = RailStatus.OK
    counts: Counts = Counts()
    artifacts: tuple[Artifact, ...] = ()
    results: tuple[Match, ...] = ()
    notes: tuple[str, ...] = ()
    detail: ApiSurface | VerifySummary | TestRun | PackageRun | None = None

class Envelope(Base, omit_defaults=False, kw_only=True):
    schema_version: int = 1
    claim: Claim
    verb: str
    command_path: tuple[str, ...] = ()
    status: RailStatus = RailStatus.OK
    exit_code: int = 0
    run_id: str = ""
    duration_ms: Annotated[float, msgspec.Meta(ge=0)] = 0.0
    report: Report | None = None
    error: Fault | None = None
    truncated: bool = False
    notes: tuple[str, ...] = ()

class Bind(Base):
    claim: Claim
    verb: str
    handler: object   # `Handler` alias in registry.md; not wire-encoded
    params: type        # frozen `@dataclass` per verb
    help: str = ""
```

`fold` / `envelope` live in `core/model.py` as module functions — see [`snippets/model-status.py.md`](snippets/model-status.py.md) §1. Detail tags are explicit short strings (`verify`, `test`, `package`, `api`); **never** `tag=str.lower(classname)` (supersedes `ARCHITECTURE.md` / `research-holistic-shapes.md` lowercased-class examples).

`Configuration(StrEnum)` for Debug/Release lives in `composition/settings.py` only — not duplicated as `Literal` in model (`research-pydantic-settings.md` §5).

---
## [5][DEFSTRUCT_ESCAPE_HATCH]
>**Dictum:** *Irregular one-off evidence is generated from catalog metadata as data, never hand-written as a tenth type.*

```python
def detail_type(tool: Tool, fields: tuple[tuple[str, type, object], ...]) -> type[Detail]:
    return msgspec.defstruct(
        f"{tool.name}_detail", fields, bases=(Detail,), tag=tool.name,
    )
```

---
## [6][ROUND_TRIP_PROOF_AND_OPEN_DECISIONS]
>**Dictum:** *The tagged union round-trips by `kind`; remaining choices are performance and external-payload seams.*

```python
r = Report(claim=Claim.BRIDGE, verb="verify", counts=Counts(ok=3, failed=1, total=4),
           detail=VerifySummary(exceptions=0, report_dir=".artifacts/assay/bridge/<run>/"))
raw = msgspec.json.encode(r)
back = msgspec.json.decode(raw, type=Report)
assert back == r and isinstance(back.detail, VerifySummary)
```

**Open decisions.** (1) **`msgspec.Raw`** for bridge C# payloads. (2) **`omit_defaults` vs presence** on `Envelope`. (3) Cross-field validation at rail boundary as `Result`, not `__post_init__` raise.
