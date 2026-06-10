"""Laws for tools.assay.rails.package — PackageParams, YakMeta, deploy, evaluate_meta, list, plan, publish, stage."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable
import os
from pathlib import Path
from typing import TYPE_CHECKING
from unittest.mock import patch

from expression import Error, Ok, Result
import msgspec
import pytest

from tests.python._testkit._aspect import register_law, spec
from tests.python._testkit._spec import assert_error, assert_error_status, assert_ok
from tools.assay.composition.settings import ArtifactScope, AssaySettings
from tools.assay.core.engine import exclusive_lease
from tools.assay.core.model import ArtifactKind, Claim, Fault, fold, Mode, PackageRun, receipt, Report
from tools.assay.core.status import RailStatus
from tools.assay.rails import package as _pkg_mod
from tools.assay.rails.package import (
    _commit_or_fail,
    _drive_steps,
    _finish,
    _lone_match,
    _merge_stage,
    _read_bytes,
    _resolve_package_file,
    _safe_package_pattern,
    _slug_from_bytes,
    _stamp_version,
    deploy,
    evaluate_meta,
    list,  # noqa: A004
    PackageParams,
    plan,
    publish,
    stage,
    YakMeta,
)


if TYPE_CHECKING:
    import builtins

    from tests.python.tools.assay.conftest import AssayHarness, YakShape
    from tools.assay.core.model import Check, Completed


# --- [TYPES] ----------------------------------------------------------------------------

type _VerbFn = Callable[[AssaySettings, ArtifactScope, PackageParams], Result[Report, Fault]]
type _MetaMutator = Callable[[YakMeta], YakMeta]


# --- [CONSTANTS] ------------------------------------------------------------------------

_NAMESPACED_XML = b'<Project xmlns="urn:test"><PropertyGroup><YakPackageSlug>rasm-bridge</YakPackageSlug></PropertyGroup></Project>'
_PLAIN_XML = b"<Project><PropertyGroup><YakPackageSlug>rasm-bridge</YakPackageSlug></PropertyGroup></Project>"
_NO_SLUG_XML = b"<Project><PropertyGroup/></Project>"

_PATTERN_CASES: tuple[tuple[str, str, bool], ...] = (
    ("plain_yak", "rasm-rh9_1-mac.yak", True),
    ("versioned_yak", "pkg-v1.0-mac.yak", True),
    ("empty", "", False),
    ("slash", "sub/file.yak", False),
    ("backslash", "sub\\file.yak", False),
    ("dotdot", "../escape.yak", False),
    ("null_byte", "file\x00null.yak", False),
)

_LONE_CASES: tuple[tuple[str, tuple[tuple[str, str], ...], bool, bool], ...] = (
    ("pkg", (("a.csproj", "pkg"),), True, False),
    ("pkg", (("a.csproj", "other"),), False, False),
    ("pkg", (("a.csproj", "pkg"), ("b.csproj", "pkg")), False, True),
    ("pkg", (), False, False),
)

_VALIDATE_FAILURE_CASES: tuple[tuple[str, str | None, _MetaMutator | None, str], ...] = (
    ("slug_mismatch", "other-slug", None, "slug mismatch"),
    ("wrong_ext", None, lambda m: msgspec.structs.replace(m, target_ext=".dll"), ".rhp"),
    ("escaped_package_dir", None, lambda m: msgspec.structs.replace(m, package_dir=Path("/escaped-workspace-outside-root")), "escaped workspace"),
    ("unsafe_pattern", None, lambda m: msgspec.structs.replace(m, package_pattern="../bad.yak"), "unsafe package pattern"),
)

_NON_OK_STATUSES: tuple[RailStatus, ...] = (RailStatus.FAILED, RailStatus.FAULTED, RailStatus.TIMEOUT, RailStatus.BUSY)

_VERBS: tuple[tuple[_VerbFn, str], ...] = ((stage, "stage"), (deploy, "deploy"), (publish, "publish"))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PACKAGE_PARAMS]


register_law(PackageParams, "defaults_are_blank")
register_law(PackageParams, "arity_is_zero")


def test_packageparams_defaults() -> None:
    """Default PackageParams has blank slug and version."""
    p = PackageParams()
    assert not p.slug
    assert not p.version


@spec(PackageParams, law="roundtrip_encode_clean")
def test_packageparams_roundtrip(p: PackageParams) -> None:
    """PackageParams round-trips through msgspec JSON (valid instances are encode-clean)."""
    from tests.python._testkit._spec import assert_roundtrip  # noqa: PLC0415

    assert_roundtrip(p, PackageParams)


def test_package_arity_is_zero() -> None:
    """PackageParams._arity is zero for every verb; positional tokens are surplus."""
    assert PackageParams()._arity("stage") == 0
    assert PackageParams()._arity("publish") == 0
    surplus = PackageParams(paths=("extra",)).bound("stage")
    assert isinstance(surplus, Fault)
    assert "unexpected positional" in surplus.message


# --- [SAFE_PACKAGE_PATTERN]

register_law(_safe_package_pattern, "truth_table")


@pytest.mark.parametrize("label, pattern, expected", _PATTERN_CASES)
def test_safe_package_pattern_truth_table(label: str, pattern: str, expected: bool) -> None:  # noqa: FBT001
    """_safe_package_pattern accepts only non-empty, path-separator-free, null-free names."""
    assert _safe_package_pattern(pattern) is expected, f"[{label}]: {pattern!r}"


# --- [SLUG_FROM_BYTES]

register_law(_slug_from_bytes, "extraction_cases")


@pytest.mark.parametrize(
    "raw, expected",
    [(_PLAIN_XML, "rasm-bridge"), (_NAMESPACED_XML, "rasm-bridge"), (_NO_SLUG_XML, ""), (b"{not xml", ""), (b"", ""), (b"<Project/>", "")],
)
def test_slug_from_bytes_cases(raw: bytes, expected: str) -> None:
    """_slug_from_bytes extracts YakPackageSlug, strips XML namespaces, returns '' on absence/malformed."""
    assert _slug_from_bytes(raw) == expected


# --- [LONE_MATCH]

register_law(_lone_match, "resolution")
register_law(_lone_match, "duplicate_message")


@pytest.mark.parametrize("slug, pairs, expect_ok, expect_dup", _LONE_CASES)
def test_lone_match_resolution(slug: str, pairs: tuple[tuple[str, str], ...], expect_ok: bool, expect_dup: bool) -> None:  # noqa: FBT001
    """_lone_match Ok-on-exactly-one, Error-on-zero, Error-on-duplicate; duplicate carries 'duplicates'."""
    result = _lone_match(slug, pairs)
    match expect_ok:
        case True:
            assert_ok(result)
        case False:
            e = assert_error(result)
            assert e.status is RailStatus.FAULTED
            if expect_dup:
                assert "duplicates" in e.message


# --- [YAK_META]

register_law(YakMeta, "validate_precondition_matrix")
register_law(YakMeta, "from_props_missing_fields")


def test_yakmeta_validate_success(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """YakMeta.validate Ok when all filesystem preconditions hold."""
    meta = yak_shape.materialize(assay_root)
    assert_ok(meta.validate(assay_root.settings, yak_shape.slug, yak_shape.slug))


@pytest.mark.parametrize("label, evaluated_slug, mutator, error_fragment", _VALIDATE_FAILURE_CASES)
def test_yakmeta_validate_failures(
    label: str, evaluated_slug: str | None, mutator: _MetaMutator | None, error_fragment: str, assay_root: AssayHarness, yak_shape: YakShape
) -> None:
    """YakMeta.validate faults on slug mismatch, wrong extension, workspace escape, unsafe pattern."""
    meta = yak_shape.materialize(assay_root)
    mutated = mutator(meta) if mutator is not None else meta
    slug_arg = evaluated_slug if evaluated_slug is not None else yak_shape.slug
    e = assert_error(mutated.validate(assay_root.settings, yak_shape.slug, slug_arg))
    assert error_fragment in e.message, f"[{label}] expected {error_fragment!r} in {e.message!r}"


def test_yakmeta_from_props_success(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """YakMeta.from_props reconstructs validated metadata from a complete MSBuild properties dict."""
    meta = yak_shape.materialize(assay_root)
    assert_ok(YakMeta.from_props(meta.project, yak_shape.props(meta), assay_root.settings, yak_shape.slug))


@pytest.mark.parametrize("drop_key", ["AssemblyName", "TargetDir", "YakPath", "TargetExt"])
def test_yakmeta_from_props_missing_fields(drop_key: str, assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """YakMeta.from_props faults when required MSBuild properties are absent."""
    meta = yak_shape.materialize(assay_root)
    props = {k: v for k, v in yak_shape.props(meta).items() if k != drop_key}
    e = assert_error(YakMeta.from_props(meta.project, props, assay_root.settings, yak_shape.slug))
    assert "missing MSBuild properties" in e.message
    assert drop_key in e.message


@spec(YakMeta, law="roundtrip_encode_clean")
def test_yakmeta_roundtrip(m: YakMeta) -> None:
    """YakMeta round-trips through msgspec JSON; Path-typed fields resolve to None via CustomType arm."""
    from tests.python._testkit._spec import assert_roundtrip  # noqa: PLC0415

    none_path = m.manifest_dir is None or m.target_dir is None or m.yak_path is None
    none_dir = m.package_dir is None or m.project_dir is None
    if none_path or none_dir:
        return
    assert_roundtrip(m, YakMeta)


# --- [EVALUATE_META]

register_law(evaluate_meta, "propagates_run_check_fault")
register_law(evaluate_meta, "decodes_msbuild_props_to_meta")
register_law(evaluate_meta, "malformed_msbuild_output_faults")


def test_evaluate_meta_propagates_run_check_fault(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """evaluate_meta propagates a Fault when dotnet MSBuild exits with bad output."""
    meta = yak_shape.materialize(assay_root)
    result = evaluate_meta(assay_root.settings, assay_root.scope(Claim.PACKAGE), meta.project, yak_shape.slug, "1.0.0")
    match result:
        case _ if result.is_ok():
            assert result.ok.assembly_name, "evaluate_meta must not yield empty assembly_name"
        case _:
            assert result.error.status in {RailStatus.FAULTED, RailStatus.FAILED}


def test_evaluate_meta_decodes_canned_msbuild_props(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """evaluate_meta decodes canned MSBuild -getProperty JSON into validated YakMeta."""
    meta = yak_shape.materialize(assay_root)
    monkeypatch.setattr(_pkg_mod, "run_check", _flow_run_check(yak_shape, meta))
    decoded = assert_ok(evaluate_meta(assay_root.settings, assay_root.scope(Claim.PACKAGE), meta.project, yak_shape.slug, "3.1.4"))
    assert decoded.assembly_name == yak_shape.assembly_name
    assert decoded.package_pattern == yak_shape.package_pattern
    assert decoded.target_ext == ".rhp"


def test_evaluate_meta_malformed_output_faults(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """evaluate_meta turns non-JSON MSBuild output into a bounded metadata Fault."""
    meta = yak_shape.materialize(assay_root)
    monkeypatch.setattr(
        _pkg_mod,
        "run_check",
        lambda _c, **_kw: Ok(receipt(("dotnet", "msbuild"), 1, stdout=b"MSB1009: project file does not exist", status=RailStatus.FAILED)),
    )
    result = evaluate_meta(assay_root.settings, assay_root.scope(Claim.PACKAGE), meta.project, yak_shape.slug, "1.0.0")
    e = assert_error_status(result, RailStatus.FAULTED)
    assert "msbuild metadata evaluation failed" in e.message
    assert "MSB1009" in e.message


# --- [PLAN]

register_law(plan, "produces_package_run_detail")
register_law(plan, "propagates_resolve_fault")
register_law(plan, "propagates_meta_fault")


def test_plan_produces_package_run_detail(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """Plan Ok-path: detail is PackageRun carrying project/pattern/version/platform fields."""
    meta = yak_shape.materialize(assay_root)
    version = "2.3.4"
    with patch.object(_pkg_mod, "_resolve_project", return_value=Ok(meta.project)), patch.object(_pkg_mod, "evaluate_meta", return_value=Ok(meta)):
        report = assert_ok(plan(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version=version)))
    assert isinstance(report.detail, PackageRun)
    assert report.detail.project == meta.project
    assert report.detail.version == version
    assert report.detail.pattern == meta.package_pattern
    assert report.detail.platform == meta.yak_platform


def test_plan_propagates_resolve_fault(assay_root: AssayHarness) -> None:
    """Plan propagates the Fault when slug resolution fails."""
    slug_fault = Fault(("package", "missing"), message="expected one package project for missing, found 0")
    with patch.object(_pkg_mod, "_resolve_project", return_value=Error(slug_fault)):
        e = assert_error(plan(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug="missing", version="1.0.0")))
    assert "missing" in e.message


def test_plan_propagates_meta_fault(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """Plan propagates evaluate_meta Fault without wrapping."""
    meta = yak_shape.materialize(assay_root)
    meta_fault = Fault(("yak", yak_shape.slug), message="msbuild metadata evaluation failed (exit 1): error text")
    with (
        patch.object(_pkg_mod, "_resolve_project", return_value=Ok(meta.project)),
        patch.object(_pkg_mod, "evaluate_meta", return_value=Error(meta_fault)),
    ):
        e = assert_error(plan(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version="1.0.0")))
    assert "msbuild" in e.message


# --- [LIST]

register_law(list, "empty_workspace")
register_law(list, "discovers_yak_projects")
register_law(list, "ignores_non_yak_csproj")


def test_list_empty_workspace(assay_root: AssayHarness) -> None:
    """List Ok with empty results when no yak projects exist in the workspace."""
    report = assert_ok(list(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams()))
    assert report.results == ()
    assert report.status is RailStatus.OK


def test_list_discovers_yak_projects(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """List returns one Match per yak project with slug=id and project path=text, kind=SCOPE."""
    yak_shape.materialize(assay_root)
    report = assert_ok(list(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams()))
    assert len(report.results) == 1
    match_row = report.results[0]
    assert match_row.id == yak_shape.slug
    assert match_row.text == str(Path(yak_shape.project).as_posix())
    assert match_row.kind is ArtifactKind.SCOPE


def test_list_ignores_non_yak_csproj(assay_root: AssayHarness) -> None:
    """List omits .csproj files that carry no YakPackageSlug property."""
    non_yak = assay_root.write("apps/core/core.csproj", "<Project><PropertyGroup/></Project>")
    report = assert_ok(list(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams()))
    assert all(m.id for m in report.results)
    assert not any(non_yak.stem in (m.text or "") for m in report.results)


# --- [STAGE_DEPLOY_PUBLISH]

register_law(stage, "slug_lease_acquired")
register_law(deploy, "slug_lease_acquired")
register_law(publish, "slug_lease_acquired")
register_law(stage, "fault_propagation")
register_law(deploy, "fault_propagation")
register_law(publish, "fault_propagation")


@pytest.mark.parametrize("verb_fn, verb_name", _VERBS)
def test_verbs_acquire_slug_lease(
    verb_fn: _VerbFn, verb_name: str, assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch
) -> None:
    """stage/deploy/publish all wrap the pipeline under a package-<slug> slug-level lease."""
    meta = yak_shape.materialize(assay_root)
    staged_report = fold(Claim.PACKAGE, verb_name, ())
    leased_resources = []

    def fake_lease(resource: str, action: Callable[[object], Result[Report, Fault]], **_kwargs: object) -> Result[Report, Fault]:
        leased_resources.append(resource)
        return action(object())

    monkeypatch.setattr(_pkg_mod, "leased", fake_lease)
    monkeypatch.setattr(_pkg_mod, "_resolve_project", lambda *_a, **_kw: Ok(meta.project))
    monkeypatch.setattr(_pkg_mod, "evaluate_meta", lambda *_a, **_kw: Ok(meta))
    monkeypatch.setattr(_pkg_mod, "_stage_meta", lambda *_a, **_kw: Ok(staged_report))
    monkeypatch.setattr(_pkg_mod, "_finish", lambda *_a, **_kw: Ok(staged_report))

    assert_ok(verb_fn(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version="1.0.0")))
    assert f"package-{yak_shape.slug}" in leased_resources, f"[{verb_name}] expected slug lease in {leased_resources}"


@pytest.mark.parametrize("verb_fn, verb_name", _VERBS)
def test_verbs_propagate_slug_resolution_fault(verb_fn: _VerbFn, verb_name: str, assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """stage/deploy/publish propagate the Fault when slug resolution fails."""
    slug_fault = Fault(("package", "missing"), message="expected one package project for missing, found 0")
    monkeypatch.setattr(_pkg_mod, "_resolve_project", lambda *_a, **_kw: Error(slug_fault))
    assert_error_status(
        verb_fn(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug="missing", version="1.0.0")), RailStatus.FAULTED
    )


# --- [LIFECYCLE_FLOW]


def _props_stdout(yak_shape: YakShape, meta: YakMeta) -> bytes:
    return msgspec.json.encode({"Properties": yak_shape.props(meta)})


def _flow_run_check(
    yak_shape: YakShape,
    meta: YakMeta,
    *,
    build_status: RailStatus = RailStatus.OK,
    stage_status: RailStatus = RailStatus.OK,
    build_fault: Fault | None = None,
) -> Callable[..., Result[Completed, Fault]]:
    """Build a fake run_check that simulates the full stage pipeline.

    The STAGE arm writes a .yak file so _commit_or_fail finds a real artifact on disk;
    all other arms return minimal receipts keyed by mode.

    Returns:
        A callable accepting a Check that dispatches on tool.mode.
    """

    def fake(check: Check, **_kwargs: object) -> Result[Completed, Fault]:
        mode = check.tool.mode
        match mode:
            case Mode.QUERY:
                return Ok(receipt(check.tool.command, 0, stdout=_props_stdout(yak_shape, meta), status=RailStatus.OK))
            case Mode.BUILD:
                return Error(build_fault) if build_fault is not None else Ok(receipt(("dotnet", "build"), 0, status=build_status))
            case Mode.STAGE:
                (Path(str(check.cwd)) / meta.package_pattern).write_bytes(b"PK\x03\x04yak")
                return Ok(receipt((str(meta.yak_path), "build"), 0, status=stage_status))
            case _:
                return Ok(receipt((str(meta.yak_path), str(mode)), 0, status=RailStatus.OK))

    return fake


register_law(stage, "full_flow_commits_distribution")
register_law(deploy, "full_flow_runs_install_step")
register_law(publish, "full_flow_runs_install_and_push_steps")
register_law(stage, "build_failure_short_circuits_before_yak")
register_law(stage, "yak_stage_failure_short_circuits_before_commit")


def _install_flow(
    verb_fn: _VerbFn, assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch
) -> tuple[Result[Report, Fault], builtins.list[str]]:
    """Run verb_fn with a fake pipeline and capture bridge call args.

    Returns:
        Tuple of (verb result, list of positional args forwarded to the fake bridge client).
    """
    meta = yak_shape.materialize(assay_root)
    monkeypatch.setattr(_pkg_mod, "run_check", _flow_run_check(yak_shape, meta))
    calls: builtins.list[str] = []

    def fake_bridge(_settings: object, *args: str, **_kw: object) -> Result[Completed, Fault]:
        calls.extend(args)
        return Ok(receipt(("rasm-bridge", *args), 0, status=RailStatus.OK))

    monkeypatch.setattr(_pkg_mod, "client_run", fake_bridge)
    return verb_fn(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version="9.9.9")), calls


def test_stage_full_flow_commits_distribution(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """Stage drives build -> artifact copy -> yak build -> commit; detail carries committed package_dir + version."""
    result, bridge_calls = _install_flow(stage, assay_root, yak_shape, monkeypatch)
    report = assert_ok(result)
    assert report.status is RailStatus.OK
    assert isinstance(report.detail, PackageRun)
    assert report.detail.version == "9.9.9"
    assert report.detail.project == yak_shape.project.as_posix()
    assert (assay_root.root / yak_shape.project.parent / "yak" / yak_shape.package_pattern).is_file()
    assert bridge_calls == []


def test_deploy_full_flow_runs_install_step(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Deploy folds an install step (yak install) for a non-bridge slug — no bridge client call."""
    from tests.python.tools.assay.conftest import YakShape  # noqa: PLC0415

    non_bridge = YakShape(slug="other-pkg", project=Path("apps/widget/widget.csproj"), assembly_name="Widget")
    result, bridge_calls = _install_flow(deploy, assay_root, non_bridge, monkeypatch)
    report = assert_ok(result)
    assert report.status is RailStatus.OK
    assert report.verb == "deploy"
    assert bridge_calls == []
    assert report.counts.ok >= 2


def test_publish_full_flow_runs_install_and_push_steps(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """Publish folds install + push steps onto the stage report; merged counts include post-stage rows."""
    result, _ = _install_flow(publish, assay_root, yak_shape, monkeypatch)
    report = assert_ok(result)
    assert report.status is RailStatus.OK
    assert report.verb == "publish"
    assert report.counts.total >= 3
    assert report.counts.ok >= 3


@pytest.mark.parametrize("build_status, stage_status", [(RailStatus.FAILED, RailStatus.OK), (RailStatus.OK, RailStatus.FAILED)])
def test_stage_step_failure_short_circuits(
    build_status: RailStatus, stage_status: RailStatus, assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch
) -> None:
    """A failed build or yak build folds the receipt into the stage report and never commits the distribution."""
    meta = yak_shape.materialize(assay_root)
    monkeypatch.setattr(_pkg_mod, "run_check", _flow_run_check(yak_shape, meta, build_status=build_status, stage_status=stage_status))
    report = assert_ok(stage(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version="1.0.0")))
    assert report.status is RailStatus.FAILED
    assert not (assay_root.root / yak_shape.project.parent / "yak").exists()


# --- [POST_STAGE_POLICY]

register_law(_finish, "stage_verb_short_circuits_policy")
register_law(_finish, "non_ok_stage_skips_steps")
register_law(_drive_steps, "bridge_steps_acquire_bridge_lock")
register_law(_resolve_package_file, "ambiguous_glob_faults")
register_law(_merge_stage, "merges_counts_results_artifacts_notes")
register_law(_stamp_version, "stamps_packagerun_and_synthesises_default")
register_law(_commit_or_fail, "non_ok_done_folds_without_commit")
register_law(_read_bytes, "absent_file_is_empty_oserror_is_fault")


def test_finish_stage_verb_returns_staged_unchanged(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """_finish with verb='stage' returns the staged report verbatim, never resolving a package file."""
    meta = yak_shape.materialize(assay_root)
    staged = fold(Claim.PACKAGE, "stage", (receipt(("yak", "build"), 0, status=RailStatus.OK),))
    assert assert_ok(_finish(assay_root.settings, assay_root.scope(Claim.PACKAGE), meta, yak_shape.slug, "stage", staged)) is staged


@pytest.mark.parametrize("bad_status", _NON_OK_STATUSES)
def test_finish_non_ok_stage_skips_post_steps(
    bad_status: RailStatus, assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch
) -> None:
    """_finish short-circuits a non-OK stage status for deploy/publish without resolving or driving steps."""
    meta = yak_shape.materialize(assay_root)
    staged = msgspec.structs.replace(fold(Claim.PACKAGE, "deploy", ()), status=bad_status)
    monkeypatch.setattr(_pkg_mod, "_resolve_package_file", lambda *_a, **_kw: (_ for _ in ()).throw(AssertionError("must not run")))
    assert assert_ok(_finish(assay_root.settings, assay_root.scope(Claim.PACKAGE), meta, yak_shape.slug, "deploy", staged)).status is bad_status


def test_drive_steps_bridge_policy_acquires_bridge_lock(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """rasm-bridge deploy steps serialize on the shared "bridge" lease: a live holder turns the step fold BUSY; a free lease runs it."""
    meta = yak_shape.materialize(assay_root)
    monkeypatch.setattr(_pkg_mod, "run_check", _flow_run_check(yak_shape, meta))
    monkeypatch.setattr(_pkg_mod, "client_run", lambda *_a, **_kw: Ok(receipt(("rasm-bridge",), 0, status=RailStatus.OK)))
    staged = fold(Claim.PACKAGE, "deploy", (receipt(("yak", "build"), 0, status=RailStatus.OK),))
    steps = _pkg_mod._STEP_POLICY["deploy", True]
    package_file = assay_root.write(yak_shape.project.parent / "yak" / yak_shape.package_pattern, "yak")
    scope = assay_root.scope(Claim.PACKAGE)
    with exclusive_lease("bridge", "holder", settings=assay_root.settings) as held:
        assert_ok(held)
        assert_error_status(_drive_steps(assay_root.settings, scope, meta, "deploy", staged, package_file, steps), RailStatus.BUSY)
    assert_ok(_drive_steps(assay_root.settings, scope, meta, "deploy", staged, package_file, steps))


def test_resolve_package_file_ambiguous_glob_faults(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """_resolve_package_file faults when zero or many files match the package pattern."""
    meta = yak_shape.materialize(assay_root)
    (assay_root.root / meta.package_dir).mkdir(parents=True, exist_ok=True)
    e = assert_error_status(_resolve_package_file(meta), RailStatus.FAULTED)
    assert "expected one package" in e.message


def test_merge_stage_combines_evidence() -> None:
    """_merge_stage sums counts and concatenates results/artifacts/notes, joining the worst status."""
    staged = msgspec.structs.replace(fold(Claim.PACKAGE, "deploy", (receipt(("yak", "build"), 0, status=RailStatus.OK),)), notes=("staged-note",))
    steps = msgspec.structs.replace(fold(Claim.PACKAGE, "deploy", (receipt(("yak", "install"), 1, status=RailStatus.FAILED),)), notes=("step-note",))
    merged = _merge_stage(staged, steps)
    assert merged.counts.total == staged.counts.total + steps.counts.total
    assert merged.counts.ok == staged.counts.ok + steps.counts.ok
    assert merged.counts.failed == staged.counts.failed + steps.counts.failed
    assert merged.notes == ("staged-note", "step-note")
    assert merged.results == (*staged.results, *steps.results)
    assert merged.status is RailStatus.FAILED


def test_stamp_version_stamps_packagerun_and_defaults() -> None:
    """_stamp_version overwrites a PackageRun version and synthesises a fresh PackageRun for any other detail."""
    stamped = _stamp_version(PackageRun(project="p", version="old"), "new")
    assert stamped.project == "p"
    assert stamped.version == "new"
    synthesised = _stamp_version(None, "2.0.0")
    assert isinstance(synthesised, PackageRun)
    assert synthesised.version == "2.0.0"
    assert not synthesised.project


@pytest.mark.parametrize("bad_status", [RailStatus.FAILED, RailStatus.FAULTED, RailStatus.TIMEOUT])
def test_commit_or_fail_non_ok_folds_without_commit(
    bad_status: RailStatus, assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch
) -> None:
    """_commit_or_fail folds a non-OK yak receipt into a stage report and never commits the staged tree."""
    meta = yak_shape.materialize(assay_root)
    staged = Path(assay_root.write(yak_shape.project.parent / "staged-tmp" / yak_shape.package_pattern, "yak")).parent
    monkeypatch.setattr(_pkg_mod, "_commit", lambda *_a, **_kw: (_ for _ in ()).throw(AssertionError("commit must not run")))
    done = receipt((str(meta.yak_path), "build"), 1, status=bad_status)
    report = assert_ok(_commit_or_fail(meta, staged, yak_shape.slug, "1.0.0", done))
    assert report.status is bad_status
    assert not staged.exists()


def test_read_bytes_absent_and_oserror(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_read_bytes returns empty bytes for an absent file and a fault for a non-NotFound OSError."""
    assert assert_ok(_read_bytes(assay_root.root / "nope.csproj")) == b""
    monkeypatch.setattr(Path, "read_bytes", lambda _self: (_ for _ in ()).throw(PermissionError("denied")))
    e = assert_error_status(_read_bytes(assay_root.write("locked.csproj", "x")), RailStatus.FAULTED)
    assert "denied" in e.message


# --- [STAGING_ERROR_RAIL]

register_law(_pkg_mod._yak_tool, "no_catalog_row_faults")
register_law(_pkg_mod._stage_artifacts, "missing_manifest_or_primary_faults")
register_law(_pkg_mod._copy_tree, "copy_oserror_faults")
register_law(_pkg_mod._commit, "swap_oserror_rolls_back")
register_law(stage, "build_outputs_fault_cleans_staged_tree")


def test_yak_tool_no_catalog_row_faults(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """_yak_tool faults when the PACKAGE catalog row is absent."""
    meta = yak_shape.materialize(assay_root)
    monkeypatch.setattr(_pkg_mod, "select", lambda *_a, **_kw: ())
    e = assert_error_status(_pkg_mod._yak_tool(meta, ("build",), Mode.STAGE), RailStatus.FAULTED)
    assert "no yak catalog row" in e.message


@pytest.mark.parametrize("drop", ["manifest", "primary"])
def test_stage_artifacts_missing_inputs_fault(drop: str, assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """_stage_artifacts faults when the manifest or primary .rhp is absent before yak build."""
    meta = yak_shape.materialize(assay_root)
    target = {"manifest": meta.manifest_dir / "manifest.yml", "primary": meta.target_dir / f"{meta.assembly_name}{meta.target_ext}"}[drop]
    target.unlink()
    staged = Path(assay_root.write("staged-x/.keep", "")).parent
    e = assert_error_status(_pkg_mod._stage_artifacts(meta, staged), RailStatus.FAULTED)
    assert ("missing yak manifest" if drop == "manifest" else "missing primary artifact") in e.message


def test_copy_tree_oserror_faults(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """_copy_tree turns a copy failure into a bounded yak-build Fault."""
    meta = yak_shape.materialize(assay_root)
    staged = Path(assay_root.write("staged-c/.keep", "")).parent
    monkeypatch.setattr(_pkg_mod, "copy2", lambda *_a, **_kw: (_ for _ in ()).throw(OSError("disk full")))
    e = assert_error_status(_pkg_mod._copy_tree(meta, staged), RailStatus.FAULTED)
    assert "disk full" in e.message


def test_commit_swap_oserror_rolls_back(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """_commit faults and restores the prior package_dir when the staged swap raises OSError."""
    meta = yak_shape.materialize(assay_root)
    staged = Path(assay_root.write("staged-commit/dist.yak", "yak")).parent
    monkeypatch.setattr(Path, "replace", lambda _self, _target: (_ for _ in ()).throw(OSError("cross-device link")))
    e = assert_error_status(_pkg_mod._commit(meta, staged, yak_shape.slug), RailStatus.FAULTED)
    assert "cross-device link" in e.message


def test_stage_build_outputs_fault_cleans_staged_tree(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """A _build_outputs Fault (spawn failure) propagates and the staged temp tree is removed by _stage_meta."""
    meta = yak_shape.materialize(assay_root)
    parents_before = {p.name for p in (assay_root.root / yak_shape.project.parent).iterdir()}
    spawn_fault = Fault(("dotnet", "build"), message="spawn failed: ENOENT")
    monkeypatch.setattr(_pkg_mod, "run_check", _flow_run_check(yak_shape, meta, build_fault=spawn_fault))
    e = assert_error(stage(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version="1.0.0")))
    assert "spawn failed" in e.message
    parents_after = {p.name for p in (assay_root.root / yak_shape.project.parent).iterdir()}
    assert parents_after == parents_before, f"leaked staged tree: {parents_after - parents_before}"


# --- [COMMIT_SENTINEL]

register_law(_pkg_mod._recover, "absent_marker_is_noop")
register_law(_pkg_mod._recover, "dead_pid_marker_heals_forward")
register_law(_pkg_mod._recover, "dead_pid_marker_heals_back")
register_law(_pkg_mod._recover, "live_pid_marker_is_busy")
register_law(_pkg_mod._recover, "corrupt_marker_clears")
register_law(_pkg_mod._commit, "success_clears_pending_marker")
register_law(stage, "stage_heals_dead_pid_marker_under_lease")


def _marker_path(meta: YakMeta) -> Path:
    return meta.package_dir.with_name(f"{meta.package_dir.name}.commit-pending.json")


def _seed_marker(meta: YakMeta, pid: int, previous: Path) -> Path:
    marker = _marker_path(meta)
    marker.parent.mkdir(parents=True, exist_ok=True)
    marker.write_bytes(msgspec.json.encode({"pid": pid, "previous": str(previous)}))
    return marker


def _seed_previous(meta: YakMeta) -> Path:
    previous = meta.package_dir.with_name(f"{meta.package_dir.name}.previous.99999")
    previous.mkdir(parents=True, exist_ok=True)
    (previous / "dist.yak").write_bytes(b"old")
    return previous


# Beyond macOS (99999) and Linux (2**22) pid_max — psutil resolves it as NoSuchProcess, i.e. a guaranteed-dead committer.
_DEAD_PID = 99_999_999


def test_recover_absent_marker_is_noop(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """_recover with no sentinel on disk is a no-op reporting direction 'absent'."""
    meta = yak_shape.materialize(assay_root)
    assert assert_ok(_pkg_mod._recover(meta, yak_shape.slug)) == "absent"


def test_recover_dead_pid_marker_heals_forward(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """A dead-pid sentinel with package_dir present rolls forward: previous dropped, marker cleared, tree intact."""
    meta = yak_shape.materialize(assay_root)
    meta.package_dir.mkdir(parents=True, exist_ok=True)
    (meta.package_dir / "dist.yak").write_bytes(b"committed")
    previous = _seed_previous(meta)
    marker = _seed_marker(meta, _DEAD_PID, previous)
    assert assert_ok(_pkg_mod._recover(meta, yak_shape.slug)) == "forward"
    assert not previous.exists()
    assert not marker.exists()
    assert (meta.package_dir / "dist.yak").read_bytes() == b"committed"


def test_recover_dead_pid_marker_heals_back(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """A dead-pid sentinel with package_dir missing rolls back: previous restored as package_dir, marker cleared."""
    meta = yak_shape.materialize(assay_root)
    previous = _seed_previous(meta)
    marker = _seed_marker(meta, _DEAD_PID, previous)
    assert assert_ok(_pkg_mod._recover(meta, yak_shape.slug)) == "back"
    assert (meta.package_dir / "dist.yak").read_bytes() == b"old"
    assert not previous.exists()
    assert not marker.exists()


def test_recover_live_pid_marker_is_busy(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """A live-pid sentinel yields BUSY and preserves the marker so the live committer can finish its swap."""
    meta = yak_shape.materialize(assay_root)
    marker = _seed_marker(meta, os.getpid(), meta.package_dir.with_name(f"{meta.package_dir.name}.previous.{os.getpid()}"))
    e = assert_error_status(_pkg_mod._recover(meta, yak_shape.slug), RailStatus.BUSY)
    assert str(os.getpid()) in e.message
    assert marker.exists()


def test_recover_corrupt_marker_clears(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """An undecodable sentinel is cleared without touching package_dir or any previous tree."""
    meta = yak_shape.materialize(assay_root)
    marker = _marker_path(meta)
    marker.parent.mkdir(parents=True, exist_ok=True)
    marker.write_bytes(b"{not json")
    assert assert_ok(_pkg_mod._recover(meta, yak_shape.slug)) == "clear"
    assert not marker.exists()


def test_commit_success_clears_pending_marker(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """_commit brackets the swap with the sentinel and leaves no marker after a successful commit."""
    meta = yak_shape.materialize(assay_root)
    staged = Path(assay_root.write(f"staged-sentinel/{yak_shape.package_pattern}", "yak")).parent
    assert_ok(_pkg_mod._commit(meta, staged, yak_shape.slug))
    assert (meta.package_dir / yak_shape.package_pattern).is_file()
    assert not _marker_path(meta).exists()


def test_stage_heals_dead_pid_marker_under_lease(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """A dead-pid sentinel left by a crashed committer is healed inside the stage lease before restaging."""
    meta = yak_shape.materialize(assay_root)
    previous = _seed_previous(meta)
    marker = _seed_marker(meta, _DEAD_PID, previous)
    monkeypatch.setattr(_pkg_mod, "run_check", _flow_run_check(yak_shape, meta))
    report = assert_ok(stage(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version="1.0.0")))
    assert report.status is RailStatus.OK
    assert not marker.exists()
    assert not previous.exists()
    assert (meta.package_dir / yak_shape.package_pattern).is_file()
