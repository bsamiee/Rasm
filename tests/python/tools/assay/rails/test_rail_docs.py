"""Law matrix for docs promotion, engine check construction, and NDJSON finding rows."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

from expression import Error, Ok, Result  # noqa: TC002  # canned executor lanes return Result instances at runtime
import pytest

from tests.python._testkit.spec import assert_error, assert_ok
from tests.python.tools.assay.kit import SeamExecutor
from tools.assay.core.model import Claim, Completed, Fault, RailStatus, receipt, Runner
from tools.assay.rails.docs import check, DocsParams, FaultedPromotion


if TYPE_CHECKING:
    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.core.model import Check, Report


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (check, DocsParams, FaultedPromotion)

_OK = Ok(Completed(("engine",), 0, status=RailStatus.OK))
_FAILED = Ok(Completed(("engine",), 1, status=RailStatus.FAILED))
_SKIP = Ok(Completed(("engine",), 0, status=RailStatus.SKIP))
_FAULT_A = Fault(("engine",), RailStatus.FAULTED, "first fault")
_FAULT_B = Fault(("engine",), RailStatus.UNSUPPORTED, "second fault")

# One NDJSON row per finding: ``check`` names the emitting check; a checkless row folds under the ``engine`` kind.
_NDJSON = (
    b'{"file":"docs/diagram.md","line":7,"status":"fail","detail":"broken edge","check":"graph-logic"}\n'
    b'{"file":"docs/diagram.md","line":2,"status":"warn","detail":"weak label"}\n'
    b'{"file":"docs/diagram.md","line":4,"status":"fail","detail":"probably","check":"hedge"}\n'
    b'{"file":"docs/diagram.md","line":1,"status":"ok","check":"render"}\n'
    b"engine banner, never a finding row\n"
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _check(
    assay_root: AssayHarness, receipts: tuple[Result[Completed, Fault], ...], *, strict: bool = False, paths: tuple[str, ...] = ("README.md",)
) -> Result[Report, Fault]:
    executor = SeamExecutor(fan_fn=lambda *_a, **_k: receipts)
    return check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=paths, strict=strict), executor)


# --- [PROMOTION_MATRIX]


@pytest.mark.parametrize(
    "receipts, strict, expect",
    [
        ((_OK,), True, RailStatus.OK),
        ((_FAILED,), True, RailStatus.FAILED),  # strict never rewrites a real defect
        ((), False, RailStatus.EMPTY),
        ((), True, "raises"),
        ((_SKIP,), True, RailStatus.OK),  # SKIP ranks below the EMPTY fold seed, so a live fan promotes clean to OK
        ((Error(_FAULT_B),), False, "fault"),
        ((Error(_FAULT_A), Error(_FAULT_B)), False, "first-fault"),
    ],
    ids=["ok", "failed-strict-preserved", "empty-not-strict", "empty-strict-raises", "skip-promotes-ok", "fan-fault", "first-fault-wins"],
)
def test_check_promotion_and_fault_matrix(
    assay_root: AssayHarness,
    receipts: tuple[Result[Completed, Fault], ...],
    strict: bool,  # noqa: FBT001  # parametrized bool flag
    expect: RailStatus | str,
) -> None:
    """Check folds receipts onto one rail: strict promotes only a checkless EMPTY, faults short-circuit first-wins."""
    match expect:
        case "raises":
            assert issubclass(FaultedPromotion, Exception)  # registry-catchable, never BaseException
            with pytest.raises(FaultedPromotion, match="no docs changed"):
                _check(assay_root, receipts, strict=strict)
        case "fault":
            assert assert_error(_check(assay_root, receipts, strict=strict)) is _FAULT_B
        case "first-fault":
            assert assert_error(_check(assay_root, receipts, strict=strict)) is _FAULT_A
        case RailStatus() as status:
            report = assert_ok(_check(assay_root, receipts, strict=strict))
            assert report.status is status
            assert report.claim is Claim.DOCS
            assert report.verb == "check"
        case _:
            pytest.fail(f"unmapped expectation row: {expect!r}")


# --- [ENGINE_ROUTING]


def test_check_builds_one_check_per_engine_per_file_and_threads_dependencies(assay_root: AssayHarness) -> None:
    """Check routes paths under settings.root and builds one input-only Check per (engine, owned file); fan receives the live deps."""
    captured: list[Check] = []
    seen: dict[str, object] = {}
    executor = SeamExecutor(fan_fn=lambda checks, **kw: (captured.extend(checks), seen.update(checks=checks, **kw), (_OK,) * len(checks))[-1])
    files = ("docs/a/README.md", "docs/b/README.md", "docs/c/flow.mmd")
    [assay_root.write(f, "# d") for f in files]
    markdown = tuple(f for f in files if f.endswith(".md"))
    scope = assay_root.scope(Claim.DOCS)

    assert_ok(check(assay_root.settings, scope, DocsParams(paths=files), executor))

    assert len(captured) == 7, "the docs claim fans every skill engine over every routed file it owns"
    pairs = {(chk.tool.name, chk.args.input) for chk in captured}
    expected = {("validate-mermaid", f) for f in files} | {(engine, f) for engine in ("prose-gate", "planning-gate") for f in markdown}
    assert pairs == expected, f"engine x owned-file product broke: {pairs}"
    scripts = {"validate-mermaid": "validate_mermaid.py", "prose-gate": "prose_gate.py"}
    for chk in captured:
        if chk.tool.runner is Runner.INPROC:
            assert chk.thunk is not None, "an INPROC engine check carries its thunk"
            continue
        cmd = chk.args.fill(chk.tool.command)
        assert cmd[:3] == ("uv", "run", "--no-project"), "engines launch through the project-free uv runner"
        assert cmd[3].endswith(scripts[chk.tool.name]), f"{chk.tool.name} argv resolves the wrong engine script: {cmd[3]}"
        assert cmd[-2:] == ("--json", chk.args.input), "argv terminates at --json and the input file; assay never places a sink"
    assert seen["settings"] is assay_root.settings
    assert seen["scope"] is scope
    assert set(seen) >= {"checks", "settings", "scope", "routed"}, "no fan_out keyword is dropped"


# --- [FINDING_ROWS]


def test_check_parses_ndjson_findings_and_keeps_crash_tails(assay_root: AssayHarness) -> None:
    """Engine NDJSON fail/warn rows fold into typed finding rows; ok rows and banners vanish; an unparsable crash keeps fold's raw tail."""
    assay_root.write("docs/diagram.md", "# d")
    parsed = assert_ok(_check(assay_root, (Ok(receipt(("engine",), 0, stdout=_NDJSON)),), paths=("docs/diagram.md",)))
    assert [(m.id, m.severity, m.line, m.path, m.message) for m in parsed.results] == [
        ("docs:graph-logic", "error", 7, "docs/diagram.md", "broken edge"),
        ("docs:engine", "warning", 2, "docs/diagram.md", "weak label"),
        ("docs:hedge", "error", 4, "docs/diagram.md", "probably"),
    ], f"NDJSON rows misfolded: {parsed.results}"
    assert parsed.status is RailStatus.OK, "finding rows ride the report; status follows the process fold"

    crashed = assert_ok(
        _check(assay_root, (Ok(receipt(("engine",), 1, stdout=b"Traceback (most recent call last): boom")),), paths=("docs/diagram.md",))
    )
    assert crashed.status is RailStatus.FAILED, "a FAILED receipt is never promoted away"
    assert len(crashed.results) == 1 and "Traceback" in crashed.results[0].text, "an unparsable crash surfaces fold's raw defect tail"
    assert crashed.results[0].severity == "failed", "the crash row carries the receipt severity"
