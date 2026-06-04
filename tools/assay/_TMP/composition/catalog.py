"""The polyglot program table: one dense ``Tool`` row per program, one ``select`` fold.

Owns ``TOOLS`` — the full C#/Python/TypeScript/Bash/SQL/Docs row set — plus the by-reference
parsers and the deterministic ``select(claim, language)`` slice. Variance is carried entirely by
the five axis enums (``Runner``/``Input``/``Language``/``Mode``/``Claim``); the catalog body is
data, never control flow. A new program is one literal row; a new language is one ``Language``
member plus one ``core/routing.py`` arm plus rows. Parsers attach as the actual
``Callable[[Completed], AnyDetail | None]`` identity — a renamed decoder is an import-time type
error, never a silent miss; rows whose evidence is "ran, exit code N" carry ``parser=None`` and
fold via ``RailStatus.from_returncode``.
"""

from typing import TYPE_CHECKING

import msgspec

from tools.assay._TMP.core.model import (  # noqa: PLC2701  # intra-staging import; _TMP is the package root
    ApiSurface,
    Claim,
    Input,
    Language,
    Mode,
    Runner,
    SourceKind,
    SymbolShape,
    TestRun,
    Tool,
    VerifySummary,
)


if TYPE_CHECKING:
    from tools.assay._TMP.core.model import AnyDetail, Completed  # intra-staging import; _TMP is the package root


# --- [MODELS] ---------------------------------------------------------------------------


class _Finding(msgspec.Struct, frozen=True, gc=False):
    """One py-analyzer diagnostic (``parse_findings``)."""

    rule: str
    message: str = ""
    path: str = ""
    line: int = 0


class _Surface(msgspec.Struct, frozen=True, gc=False):
    """The ``ilspycmd`` surface roster (``parse_surface``): source provenance + symbol shape evidence."""

    source_kind: SourceKind = SourceKind.TOOL
    source_id: str = ""
    version: str = ""
    shape: SymbolShape = SymbolShape.SEARCH
    signature: str = ""
    doc: str = ""
    preview: str = ""


class _VerifyTelemetry(msgspec.Struct, frozen=True, gc=False):
    """The bridge ``verify`` JSON (``parse_verify``): scenario exception telemetry + capture pointer."""

    exceptions: int = 0
    report_dir: str = ""
    first_failure: str = ""
    first_fault_phase: str = ""
    first_fault_output: str = ""


class _TestTelemetry(msgspec.Struct, frozen=True, gc=False):
    """The ``dotnet test --list-tests`` JSON (``parse_tests``): flat mutation/coverage scalars."""

    mutation: str = "off"
    coverage: float | None = None
    killed: int = 0
    survived: int = 0
    selected: int = 0


class _ShellMessage(msgspec.Struct, frozen=True, gc=False):
    """One ShellCheck JSON1 ``comments`` row (``parse_shellcheck``)."""

    code: int = 0
    message: str = ""
    file: str = ""
    line: int = 0
    level: str = ""


class _Shellcheck(msgspec.Struct, frozen=True, gc=False):
    """The ShellCheck JSON1 envelope (``-f json1``): a ``comments`` array of ``_ShellMessage`` rows."""

    comments: tuple[_ShellMessage, ...] = ()


# --- [CONSTANTS] ------------------------------------------------------------------------

_FINDINGS = msgspec.json.Decoder(tuple[_Finding, ...])
_SURFACE = msgspec.json.Decoder(_Surface)
_VERIFY = msgspec.json.Decoder(_VerifyTelemetry)
_TESTS = msgspec.json.Decoder(_TestTelemetry)
_SHELLCHECK = msgspec.json.Decoder(_Shellcheck)


# --- [OPERATIONS] -----------------------------------------------------------------------


def parse_findings(done: Completed) -> AnyDetail | None:
    """Decode py-analyzer ``--format json`` into validated ``_Finding`` rows.

    The per-rule ``PYS####`` rows are ``Match`` evidence the rail projects onto ``Report.results``;
    no ``AnyDetail`` variant models per-finding rows, so the decode validates but returns ``None``
    and the ``FAILED`` status rides ``Completed`` via ``from_returncode``.
    """
    _FINDINGS.decode(done.stdout or b"[]")
    return None


def parse_build(done: Completed) -> AnyDetail | None:
    """Surface the cs-analyzer MSBuild pass: ``CSP####`` are exit-code evidence, not JSON.

    ``dotnet build /clp:ErrorsOnly`` emits free-form ``CSP####`` lines; the count rides
    ``Report.counts`` via ``fold`` and the defect status rides ``Completed.status`` via
    ``from_returncode``, so no ``AnyDetail`` variant is constructed.
    """
    _ = done
    return None


def parse_tests(done: Completed) -> AnyDetail | None:
    """Decode ``dotnet test`` telemetry into a ``TestRun`` evidence variant."""
    t = _TESTS.decode(done.stdout or b"{}")
    return TestRun(mutation=t.mutation, coverage=t.coverage, killed=t.killed, survived=t.survived, selected=t.selected)


def parse_verify(done: Completed) -> AnyDetail | None:
    """Decode the bridge ``verify`` JSON into a ``VerifySummary`` evidence variant."""
    v = _VERIFY.decode(done.stdout or b"{}")
    return VerifySummary(
        exceptions=v.exceptions,
        report_dir=v.report_dir,
        first_failure=v.first_failure,
        first_fault_phase=v.first_fault_phase,
        first_fault_output=v.first_fault_output,
    )


def parse_surface(done: Completed) -> AnyDetail | None:
    """Decode the ``ilspycmd`` surface roster into an ``ApiSurface`` evidence variant."""
    s = _SURFACE.decode(done.stdout or b"{}")
    return ApiSurface(
        source_kind=s.source_kind, source_id=s.source_id, version=s.version, shape=s.shape, signature=s.signature, doc=s.doc, preview=s.preview
    )


def parse_shellcheck(done: Completed) -> AnyDetail | None:
    """Decode ShellCheck ``-f json1`` into validated ``_ShellMessage`` rows.

    The ``comments`` rows are ``Match`` evidence the rail projects onto ``Report.results``; no
    ``AnyDetail`` variant models per-comment rows, so the decode validates but returns ``None``
    and the ``FAILED`` status rides ``Completed`` via ``from_returncode``.
    """
    _SHELLCHECK.decode(done.stdout or b'{"comments":[]}')
    return None


# --- [TABLES] ---------------------------------------------------------------------------

DIRECT, MODULE, UV, DOTNET, PNPM = Runner.DIRECT, Runner.MODULE, Runner.UV, Runner.DOTNET, Runner.PNPM
FILES, INCLUDE, PROJECT, SOLUTION, GLOB, NONE = (Input.FILES, Input.INCLUDE, Input.PROJECT, Input.SOLUTION, Input.GLOB, Input.NONE)
PY, TS, CS, BASH, SQL, DOCS = (Language.PYTHON, Language.TYPESCRIPT, Language.CSHARP, Language.BASH, Language.SQL, Language.DOCS)

TOOLS: tuple[Tool, ...] = (
    # -- Python ------------------------------------------------------------------------------
    Tool("validate-pyproject", UV, ("validate-pyproject", "pyproject.toml"), PROJECT, PY, Claim.STATIC),
    Tool("ruff", UV, ("ruff", "check"), FILES, PY, Claim.STATIC),
    Tool("ruff", UV, ("ruff", "check", "--fix"), FILES, PY, Claim.STATIC, mode=Mode.WRITE),
    Tool("ruff-format", UV, ("ruff", "format", "--check"), FILES, PY, Claim.STATIC),
    Tool("ruff-format", UV, ("ruff", "format"), FILES, PY, Claim.STATIC, mode=Mode.WRITE),
    Tool("ty", UV, ("ty", "check", "--no-progress"), INCLUDE, PY, Claim.STATIC),
    Tool("mypy", UV, ("mypy", "--explicit-package-bases"), INCLUDE, PY, Claim.STATIC),
    Tool("ast-grep-py", PNPM, ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^no-", "--error"), GLOB, PY, Claim.STATIC),
    Tool(
        "ast-grep-py",
        PNPM,
        ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^no-", "--error", "tests/tools/ast-grep/fail", "--no-color"),
        FILES,
        PY,
        Claim.TEST,
        mode=Mode.VERIFY,
    ),
    Tool("py-analyzer", MODULE, ("tools.py_analyzer", "check", "--root", ".", "--format", "json"), NONE, PY, Claim.STATIC, parser=parse_findings),
    Tool("pytest", UV, ("pytest",), INCLUDE, PY, Claim.TEST, mode=Mode.RUN),
    Tool("pytest-deadfixtures", UV, ("pytest", "--dead-fixtures"), INCLUDE, PY, Claim.TEST, mode=Mode.LIST),
    Tool("pytest-benchmark", UV, ("pytest", "-m", "benchmark", "--benchmark-only", "--benchmark-autosave"), INCLUDE, PY, Claim.TEST, mode=Mode.RUN),
    Tool("coverage", UV, ("coverage", "run", "-m", "pytest"), INCLUDE, PY, Claim.TEST, mode=Mode.RUN),
    Tool("mutmut", UV, ("mutmut", "run"), PROJECT, PY, Claim.TEST, mode=Mode.MUTATION),
    # -- TypeScript --------------------------------------------------------------------------
    Tool("tsc", PNPM, ("tsc", "--noEmit", "-p", "tsconfig.base.json"), PROJECT, TS, Claim.STATIC, mode=Mode.BUILD),
    Tool("biome", PNPM, ("biome", "ci", ".", "--files-ignore-unknown=true"), GLOB, TS, Claim.STATIC),
    Tool("knip", PNPM, ("knip", "--exclude", "catalog", "--no-config-hints"), PROJECT, TS, Claim.STATIC),
    Tool("sherif", PNPM, ("sherif",), PROJECT, TS, Claim.STATIC),
    Tool("ast-grep-ts", PNPM, ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^ts-domain-", "--error"), GLOB, TS, Claim.STATIC),
    Tool(
        "ast-grep-ts",
        PNPM,
        ("ast-grep", "scan", "--config", "sgconfig.yml", "--filter", "^ts-domain-", "--error", "tests/tools/ast-grep/fail", "--no-color"),
        FILES,
        TS,
        Claim.TEST,
        mode=Mode.VERIFY,
    ),
    Tool("vitest", PNPM, ("vitest", "run"), NONE, TS, Claim.TEST, mode=Mode.RUN),
    # -- C# ----------------------------------------------------------------------------------
    Tool("dotnet-format", DOTNET, ("format", "--severity", "error", "--verify-no-changes"), INCLUDE, CS, Claim.STATIC),
    Tool("dotnet-format", DOTNET, ("format", "--severity", "error"), INCLUDE, CS, Claim.STATIC, mode=Mode.WRITE),
    Tool("dotnet-restore", DOTNET, ("restore", "--locked-mode"), PROJECT, CS, Claim.STATIC, mode=Mode.RESTORE),
    Tool(
        "dotnet-build",
        DOTNET,
        ("build", "--no-restore", "-v:quiet", "/clp:ErrorsOnly"),
        PROJECT,
        CS,
        Claim.STATIC,
        mode=Mode.BUILD,
        parser=parse_build,
    ),
    Tool("dotnet-test", DOTNET, ("test", "--minimum-expected-tests", "1"), PROJECT, CS, Claim.TEST, mode=Mode.RUN),
    Tool("dotnet-test", DOTNET, ("test", "--list-tests"), PROJECT, CS, Claim.TEST, mode=Mode.LIST, parser=parse_tests),
    Tool(
        "dotnet-stryker",
        DOTNET,
        ("tool", "run", "dotnet-stryker", "--", "--test-runner", "mtp", "--mutation-level", "Standard"),
        PROJECT,
        CS,
        Claim.TEST,
        mode=Mode.MUTATION,
        timeout=3600.0,
    ),
    Tool("rasm-bridge", DOTNET, ("run", "--no-build", "--", "verify"), PROJECT, CS, Claim.BRIDGE, mode=Mode.VERIFY, parser=parse_verify),
    Tool("ilspycmd", DOTNET, ("tool", "run", "ilspycmd", "--", "-l", "cisde"), NONE, CS, Claim.API, mode=Mode.QUERY, parser=parse_surface),
    Tool("yak", DIRECT, ("yak", "build"), NONE, CS, Claim.PACKAGE, mode=Mode.STAGE),
    # -- Bash (config-pending day-one rows; linters not yet configured in-repo) --------------
    Tool("shellcheck", DIRECT, ("shellcheck", "-f", "json1"), FILES, BASH, Claim.STATIC, parser=parse_shellcheck),
    Tool("shfmt", DIRECT, ("shfmt", "-d"), FILES, BASH, Claim.STATIC),
    Tool("shfmt", DIRECT, ("shfmt", "-w"), FILES, BASH, Claim.STATIC, mode=Mode.WRITE),
    # -- SQL (config-pending day-one rows; linters not yet configured in-repo) ---------------
    Tool("sqlfluff", UV, ("sqlfluff", "lint", "--dialect", "postgres"), FILES, SQL, Claim.STATIC),
    Tool("sqlfluff", UV, ("sqlfluff", "fix", "--dialect", "postgres"), FILES, SQL, Claim.STATIC, mode=Mode.WRITE),
    Tool("squawk", UV, ("squawk",), FILES, SQL, Claim.STATIC),
    # -- Docs --------------------------------------------------------------------------------
    Tool("mmdc", PNPM, ("mmdc", "-a", ".artifacts/mermaid", "-q"), INCLUDE, DOCS, Claim.DOCS),
)


def select(claim: Claim, language: Language | None = None) -> tuple[Tool, ...]:
    """Filter ``TOOLS`` to one rail's slice, deterministically sorted (independent of authoring order).

    The key ``(language.value, mode.value, name, command)`` groups by language for fan-out,
    sorts the ``(name, mode)`` write/check twins adjacently, and makes execution order stable.
    ``language=None`` is the polyglot request (``static``/``test``/``docs`` fold across every
    language); a C#-only rail (``bridge``/``package``/``api``) passes ``Language.CSHARP``.
    """
    return tuple(
        sorted(
            (t for t in TOOLS if t.claim is claim and (language is None or t.language is language)),
            key=lambda t: (t.language.value, t.mode.value, t.name, t.command),
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TOOLS", "parse_build", "parse_findings", "parse_shellcheck", "parse_surface", "parse_tests", "parse_verify", "select"]
