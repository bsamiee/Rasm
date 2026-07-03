"""Law matrix for docs promotion, check folding, and outcome rows."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

from expression import Error, Ok, Result  # noqa: TC002  # canned executor lanes return Result instances at runtime
import pytest

from tests.python._testkit.spec import assert_error, assert_ok
from tests.python.tools.assay.kit import SeamExecutor
from tools.assay.core.model import Claim, Completed, Fault, RailStatus
from tools.assay.rails.docs import check, DocsParams, FaultedPromotion


if TYPE_CHECKING:
    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.core.model import Check


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (check, DocsParams, FaultedPromotion)

_OK = Ok(Completed(("mmdc",), 0, status=RailStatus.OK))
_FAILED = Ok(Completed(("mmdc",), 1, status=RailStatus.FAILED))
_SKIP = Ok(Completed(("mmdc",), 0, status=RailStatus.SKIP))
_FAULT_A = Fault(("mmdc",), RailStatus.FAULTED, "first fault")
_FAULT_B = Fault(("mmdc",), RailStatus.UNSUPPORTED, "second fault")

# --- [OPERATIONS] -----------------------------------------------------------------------


def _check(assay_root: AssayHarness, receipts: tuple[Result[Completed, Fault], ...], *, strict: bool = False, paths: tuple[str, ...] = ("README.md",)) -> Result[object, Fault]:
    return check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=paths, strict=strict), SeamExecutor(fan_fn=lambda *_a, **_k: receipts))


# --- [PROMOTION_MATRIX]


@pytest.mark.parametrize(
    "receipts, strict, expect",
    [
        ((_OK,), True, RailStatus.OK),
        ((_FAILED,), True, RailStatus.FAILED),  # strict never rewrites a real defect
        ((), False, RailStatus.EMPTY),
        ((), True, "raises"),
        ((_SKIP,), True, "raises"),
        ((Error(_FAULT_B),), False, "fault"),
        ((Error(_FAULT_A), Error(_FAULT_B)), False, "first-fault"),
    ],
    ids=["ok", "failed-strict-preserved", "empty-not-strict", "empty-strict-raises", "skip-strict-raises", "fan-fault", "first-fault-wins"],
)
def test_check_promotion_and_fault_matrix(
    assay_root: AssayHarness,
    receipts: tuple[Result[Completed, Fault], ...],
    strict: bool,  # noqa: FBT001  # parametrized bool flag
    expect: object,
) -> None:
    """Check folds receipts onto one rail: strict promotes only EMPTY/SKIP, faults short-circuit first-wins."""
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


# --- [ROUTING_AND_SINKS]


def test_check_routes_files_builds_per_file_argv_and_threads_dependencies(assay_root: AssayHarness) -> None:
    """Check routes paths under settings.root, builds one mmdc Check per file with collision-free sinks, and forwards live deps to fan."""
    captured: list[Check] = []
    seen: dict[str, object] = {}
    executor = SeamExecutor(fan_fn=lambda checks, **kw: (captured.extend(checks), seen.update(checks=checks, **kw), (_OK,) * len(checks))[-1])
    files = ("docs/a/README.md", "docs/b/README.md")
    [assay_root.write(f, "# d") for f in files]
    scope = assay_root.scope(Claim.DOCS)

    assert_ok(check(assay_root.settings, scope, DocsParams(paths=files), executor))

    assert len(captured) == 2, "settings.root routing finds harness-local files; cwd routing finds nothing"
    sinks = []
    for chk, src in zip(captured, files, strict=True):
        cmd = chk.args.fill(chk.tool.command)
        assert cmd[0] == "mmdc"
        assert cmd[cmd.index("-i") + 1] == src
        assert cmd[cmd.index("-a") + 1] == scope.path
        sinks.append(cmd[cmd.index("-o") + 1])
        assert sinks[-1].endswith(".md"), "argv terminates at the -o markdown sink, not a bare input positional"
    assert len(set(sinks)) == 2, "same-basename files slug to distinct sinks"
    assert seen["settings"] is assay_root.settings
    assert seen["scope"] is scope
    assert set(seen) >= {"checks", "settings", "scope", "routed"}, "no fan_out keyword is dropped"


def test_check_folds_result_rows_severity_and_sink_artifacts(assay_root: AssayHarness) -> None:
    """Check folds one source:<file>:1 row per routed file, stamps receipt severity, and rides produced sinks as artifacts."""
    assay_root.write("docs/diagram.md", "# d")
    scope = assay_root.scope(Claim.DOCS)
    scope.ensure()
    captured: list[Check] = []
    probe = SeamExecutor(fan_fn=lambda checks, **_k: (captured.extend(checks), (_OK,))[-1])
    check(assay_root.settings, scope, DocsParams(paths=("docs/diagram.md",)), probe)
    cmd = captured[0].args.fill(captured[0].tool.command)
    stem = cmd[cmd.index("-o") + 1].rsplit("/", 1)[-1].removesuffix(".md")  # sink stem learned from the public argv, not the private slugger
    parts = scope.path.removeprefix(f"{scope.store.root}/").split("/")
    scope.store.write_bytes(b"<svg/>", *parts, f"{stem}-1.svg")
    scope.store.write_bytes(b"# out", *parts, f"{stem}.md")

    ok_report = assert_ok(_check(assay_root, (_OK,), paths=("docs/diagram.md",)))
    assert [m.id for m in ok_report.results] == ["source:docs/diagram.md:1"]
    assert ok_report.results[0].severity is None, "an exit-0 mmdc receipt folds to a non-failed row"
    assert ok_report.status is RailStatus.OK, "EMPTY base + result rows promotes to OK"
    artifact_names = {a.path.rsplit("/", 1)[-1] for a in ok_report.artifacts}
    assert {f"{stem}-1.svg", f"{stem}.md"} <= artifact_names, "produced SVG + MD ride the envelope as Artifact rows"

    bad_report = assert_ok(_check(assay_root, (_FAILED,), paths=("docs/diagram.md",)))
    assert bad_report.status is RailStatus.FAILED, "a FAILED receipt is never promoted away"
    assert bad_report.results[0].severity == "failed", "the FAILED receipt stamps the row severity"
