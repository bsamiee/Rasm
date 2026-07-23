"""Laws for package params, Yak metadata, stage flow, post-stage policy, and the commit sentinel."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable
import os
from pathlib import Path
from typing import TYPE_CHECKING

from expression import Error, Ok
import msgspec
import pytest

from tests.python._testkit.laws import spec
from tests.python._testkit.spec import assert_error, assert_error_status, assert_ok, assert_roundtrip
from tests.python.tools.assay.kit import SeamExecutor, YakShape
from tools.assay.composition.settings import AssaySettings
from tools.assay.composition.store import ArtifactScope
from tools.assay.core.govern import exclusive_lease
from tools.assay.core.model import ArtifactKind, Claim, Fault, Mode, PackageRun, RailStatus, receipt
from tools.assay.core.routing import parse_csproj
from tools.assay.diagnostics import fold
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
    _stamp_version,
    evaluate_meta,
    list as pkg_list,
    PackageParams,
    plan,
    publish,
    YakMeta,
)


if TYPE_CHECKING:
    import builtins

    from expression import Result

    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.core.model import Check, Completed, Report


# --- [TYPES] ----------------------------------------------------------------------------

type _MetaMutator = Callable[[YakMeta], YakMeta]

# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (evaluate_meta, pkg_list, plan, publish, parse_csproj)

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

# error_fragment None = the Ok anchor row proving the failure rows can fail.
_VALIDATE_CASES: tuple[tuple[str, str | None, _MetaMutator | None, str | None], ...] = (
    ("ok", None, None, None),
    ("slug_mismatch", "other-slug", None, "slug mismatch"),
    ("wrong_ext", None, lambda m: msgspec.structs.replace(m, target_ext=".dll"), ".rhp"),
    ("escaped_package_dir", None, lambda m: msgspec.structs.replace(m, package_dir=Path("/escaped-workspace-outside-root")), "escaped workspace"),
    ("unsafe_pattern", None, lambda m: msgspec.structs.replace(m, package_pattern="../bad.yak"), "unsafe package pattern"),
)

_NON_OK_STATUSES: tuple[RailStatus, ...] = (RailStatus.FAILED, RailStatus.FAULTED, RailStatus.TIMEOUT, RailStatus.BUSY)

# Above common pid_max values; psutil resolves it as a dead process.
_DEAD_PID = 99_999_999

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FLOW_HARNESS]


def _props_stdout(yak_shape: YakShape, meta: YakMeta) -> bytes:
    return msgspec.json.encode({"Properties": yak_shape.props(meta)})


def _flow_run_check(
    yak_shape: YakShape,
    meta: YakMeta,
    *,
    build_status: RailStatus = RailStatus.OK,
    stage_status: RailStatus = RailStatus.OK,
    build_fault: Fault | None = None,
    bridge_verbs: builtins.list[str] | None = None,
) -> Callable[..., Result[Completed, Fault]]:
    """Build a canned executor run lane materializing the stage pipeline artifacts and playing supervisor sessions.

    Returns:
        Run-lane callable for ``SeamExecutor(run_fn=...)`` driving the whole publish pipeline.
    """

    def fake(check: Check, **kwargs: object) -> Result[Completed, Fault]:
        mode = check.tool.mode
        filled = check.args.fill(check.tool.command)
        match mode:
            case Mode.QUERY:
                return Ok(receipt(filled, 0, stdout=_props_stdout(yak_shape, meta), status=RailStatus.OK))
            case Mode.BUILD:
                scope = kwargs["scope"]
                settings = kwargs["settings"]
                assert isinstance(scope, ArtifactScope)
                assert isinstance(settings, AssaySettings)
                project = Path(filled[1])
                target = Path(scope.path) / "bin" / project.stem / settings.configuration.value.lower()
                target.mkdir(parents=True, exist_ok=True)
                (target / f"{meta.assembly_name}{meta.target_ext}").write_bytes(b"rhp")
                (target / f"{meta.assembly_name}.dll").write_bytes(b"dll")
                return Error(build_fault) if build_fault is not None else Ok(receipt(("dotnet", "build"), 0, status=build_status))
            case Mode.STAGE:
                (Path(str(check.cwd)) / meta.package_pattern).write_bytes(b"PK\x03\x04yak")
                return Ok(receipt((str(meta.yak_path), "build"), 0, status=stage_status))
            case Mode.VERIFY:
                # Bridge lifecycle steps ride the real client_run; the lane plays an OK supervisor session.
                bridge_verbs.append(str(check.args.verb)) if bridge_verbs is not None else None
                return Ok(receipt(filled, 0, stdout=msgspec.json.encode({"status": RailStatus.OK.value}), status=RailStatus.OK))
            case _:
                return Ok(receipt((str(meta.yak_path), str(mode)), 0, status=RailStatus.OK))

    return fake


def _install_flow(assay_root: AssayHarness, yak_shape: YakShape) -> tuple[Result[Report, Fault], builtins.list[str]]:
    """Run publish end-to-end with the canned executor pipeline and the materialized supervisor binary.

    Returns:
        Publish result paired with the captured bridge lifecycle verbs.
    """
    meta = yak_shape.materialize(assay_root)
    assay_root.supervisor()
    verbs: builtins.list[str] = []
    executor = SeamExecutor(run_fn=_flow_run_check(yak_shape, meta, bridge_verbs=verbs))
    return publish(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version="9.9.9"), executor), verbs


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


# --- [PACKAGE_PARAMS]


@spec(PackageParams, law="roundtrip_encode_clean")
def test_packageparams_roundtrip(p: PackageParams) -> None:
    assert_roundtrip(p, PackageParams)


def test_package_bound_requires_slug_and_publish_version() -> None:
    """bound() rejects surplus positionals, requires --slug for both verbs, and --version only for publish."""
    surplus = PackageParams(paths=("extra",)).bound("publish")
    assert isinstance(surplus, Fault)
    assert "unexpected positional" in surplus.message
    for verb in ("publish", "plan"):
        missing_slug = PackageParams().bound(verb)
        assert isinstance(missing_slug, Fault)
        assert "--slug is required" in missing_slug.message
    missing_version = PackageParams(slug="rasm-bridge").bound("publish")
    assert isinstance(missing_version, Fault)
    assert "--version is required" in missing_version.message
    assert isinstance(PackageParams(slug="rasm-bridge").bound("plan"), PackageParams)
    assert isinstance(PackageParams(slug="rasm-bridge", version="1.0.3").bound("publish"), PackageParams)


# --- [PROJECTION_MATRICES]


@pytest.mark.parametrize("label, pattern, expected", _PATTERN_CASES, ids=[label for label, _, _ in _PATTERN_CASES])
def test_safe_package_pattern_truth_table(label: str, pattern: str, expected: bool) -> None:  # ruff:ignore[boolean-type-hint-positional-argument]
    """_safe_package_pattern accepts only non-empty, path-separator-free, null-free names."""
    assert _safe_package_pattern(pattern) is expected, f"[{label}]: {pattern!r}"


@pytest.mark.parametrize(
    "raw, expected",
    [(_PLAIN_XML, "rasm-bridge"), (_NAMESPACED_XML, "rasm-bridge"), (_NO_SLUG_XML, ""), (b"{not xml", ""), (b"", ""), (b"<Project/>", "")],
)
def test_slug_from_bytes_cases(raw: bytes, expected: str) -> None:
    """parse_csproj extracts YakPackageSlug across namespace and malformed XML cases."""
    assert next(iter(parse_csproj(raw, "YakPackageSlug")), "") == expected


@pytest.mark.parametrize("slug, pairs, expect_ok, expect_dup", _LONE_CASES)
def test_lone_match_resolution(slug: str, pairs: tuple[tuple[str, str], ...], expect_ok: bool, expect_dup: bool) -> None:  # ruff:ignore[boolean-type-hint-positional-argument]
    """_lone_match admits one match and faults zero or duplicate matches."""
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


@pytest.mark.parametrize("label, evaluated_slug, mutator, error_fragment", _VALIDATE_CASES, ids=[c[0] for c in _VALIDATE_CASES])
def test_yakmeta_validate_matrix(
    label: str, evaluated_slug: str | None, mutator: _MetaMutator | None, error_fragment: str | None, assay_root: AssayHarness, yak_shape: YakShape
) -> None:
    """YakMeta.validate is Ok on the intact tree and faults on slug/extension/escape/pattern violations."""
    meta = yak_shape.materialize(assay_root)
    mutated = mutator(meta) if mutator is not None else meta
    slug_arg = evaluated_slug if evaluated_slug is not None else yak_shape.slug
    result = mutated.validate(assay_root.settings, yak_shape.slug, slug_arg)
    match error_fragment:
        case None:
            assert_ok(result)
        case fragment:
            assert fragment in assert_error(result).message, f"[{label}]"


@pytest.mark.parametrize("drop_key", [None, "AssemblyName", "TargetDir", "YakPath", "TargetExt"], ids=["complete", "asm", "dir", "yak", "ext"])
def test_yakmeta_from_props_matrix(drop_key: str | None, assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """YakMeta.from_props reconstructs validated metadata and faults per missing MSBuild property."""
    meta = yak_shape.materialize(assay_root)
    props = {k: v for k, v in yak_shape.props(meta).items() if k != drop_key}
    result = YakMeta.from_props(meta.project, props, assay_root.settings, yak_shape.slug)
    match drop_key:
        case None:
            assert_ok(result)
        case key:
            e = assert_error(result)
            assert "missing MSBuild properties" in e.message
            assert key in e.message


@spec(YakMeta, law="drawn_meta_carries_real_paths")
def test_yakmeta_drawn_paths_are_real(m: YakMeta) -> None:
    """Every drawn YakMeta carries live Path values — the resolver's opaque-leaf lane generates, never a None placeholder.

    YakMeta is an evaluation model, not a wire struct: msgspec owns no Path codec, and the plan
    detail crosses the wire as the string-projected ``PackageRun``.
    """
    for field in ("manifest_dir", "target_dir", "yak_path", "package_dir", "project_dir"):
        value = getattr(m, field)
        assert isinstance(value, Path), f"{field} drew {value!r}, not a Path"


# --- [EVALUATE_META_AND_PLAN]


def test_evaluate_meta_decodes_canned_msbuild_props(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """evaluate_meta decodes canned MSBuild -getProperty JSON into validated YakMeta."""
    meta = yak_shape.materialize(assay_root)
    executor = SeamExecutor(run_fn=_flow_run_check(yak_shape, meta))
    decoded = assert_ok(evaluate_meta(assay_root.settings, assay_root.scope(Claim.PACKAGE), meta.project, yak_shape.slug, "3.1.4", executor))
    assert decoded.assembly_name == yak_shape.assembly_name
    assert decoded.package_pattern == yak_shape.package_pattern
    assert decoded.target_ext == ".rhp"


def test_evaluate_meta_malformed_output_faults(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """evaluate_meta turns non-JSON MSBuild output into a bounded metadata Fault."""
    meta = yak_shape.materialize(assay_root)
    executor = SeamExecutor(
        run_fn=lambda _c, **_kw: Ok(receipt(("dotnet", "msbuild"), 1, stdout=b"MSB1009: project file does not exist", status=RailStatus.FAILED))
    )
    result = evaluate_meta(assay_root.settings, assay_root.scope(Claim.PACKAGE), meta.project, yak_shape.slug, "1.0.0", executor)
    e = assert_error_status(result, RailStatus.FAULTED)
    assert "msbuild metadata evaluation failed" in e.message
    assert "MSB1009" in e.message


def test_plan_produces_package_run_detail(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """Plan resolves the real workspace project, evaluates canned metadata, and details a PackageRun."""
    meta = yak_shape.materialize(assay_root)
    executor = SeamExecutor(run_fn=_flow_run_check(yak_shape, meta))
    version = "2.3.4"
    report = assert_ok(plan(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version=version), executor))
    assert isinstance(report.detail, PackageRun)
    assert report.detail.project == meta.project
    assert report.detail.version == version
    assert report.detail.pattern == meta.package_pattern
    assert report.detail.platform == meta.yak_platform


@pytest.mark.parametrize("verb_fn", [plan, publish], ids=["plan", "publish"])
def test_verbs_fault_on_unresolvable_slug(verb_fn: Callable[..., Result[object, Fault]], assay_root: AssayHarness) -> None:
    """Both verbs propagate the typed resolution Fault when no workspace project carries the slug."""
    result = verb_fn(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug="missing", version="1.0.0"), SeamExecutor())
    e = assert_error_status(result, RailStatus.FAULTED)
    assert "missing" in e.message


def test_plan_propagates_meta_fault(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """Plan propagates the evaluate_meta Fault without wrapping when MSBuild metadata fails."""
    yak_shape.materialize(assay_root)
    executor = SeamExecutor(run_fn=lambda _c, **_kw: Ok(receipt(("dotnet", "msbuild"), 1, stdout=b"error text", status=RailStatus.FAILED)))
    e = assert_error(plan(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version="1.0.0"), executor))
    assert "msbuild" in e.message


# --- [LIST]


def test_list_discovers_only_yak_projects(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """List is empty on a bare workspace, then returns one SCOPE Match per yak project, omitting slug-less csproj."""
    empty = assert_ok(pkg_list(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(), SeamExecutor()))
    assert empty.results == ()
    assert empty.status is RailStatus.OK

    non_yak = assay_root.write("apps/core/core.csproj", "<Project><PropertyGroup/></Project>")
    yak_shape.materialize(assay_root)
    report = assert_ok(pkg_list(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(), SeamExecutor()))
    assert len(report.results) == 1
    match_row = report.results[0]
    assert match_row.id == yak_shape.slug
    assert match_row.text == str(Path(yak_shape.project).as_posix())
    assert match_row.kind is ArtifactKind.SCOPE
    assert not any(non_yak.stem in (m.text or "") for m in report.results)


# --- [PUBLISH_FLOW]


def test_publish_acquires_slug_lease(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """Publish serializes on the package-<slug> lease: a held lease yields BUSY before any pipeline work."""
    yak_shape.materialize(assay_root)
    params = PackageParams(slug=yak_shape.slug, version="1.0.0")
    with exclusive_lease(f"package-{yak_shape.slug}", "holder", settings=assay_root.settings) as held:
        assert_ok(held)
        assert_error_status(publish(assay_root.settings, assay_root.scope(Claim.PACKAGE), params, SeamExecutor()), RailStatus.BUSY)


def test_publish_non_bridge_slug_runs_install_and_push(assay_root: AssayHarness) -> None:
    """Publish commits the distribution and folds install + push for a non-bridge slug — no bridge lifecycle step."""
    non_bridge = YakShape(slug="other-pkg", project=Path("apps/widget/widget.csproj"), assembly_name="Widget")
    result, bridge_verbs = _install_flow(assay_root, non_bridge)
    report = assert_ok(result)
    assert report.status is RailStatus.OK
    assert report.verb == "publish"
    assert isinstance(report.detail, PackageRun)
    assert report.detail.version == "9.9.9"
    assert report.detail.project == non_bridge.project.as_posix()
    assert (assay_root.root / non_bridge.project.parent / "yak" / non_bridge.package_pattern).is_file()
    assert bridge_verbs == []
    assert report.counts.ok >= 3  # stage + install + push


def test_publish_bridge_slug_cycles_host_with_install_and_push(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """Publish for the rasm-bridge slug cycles the live host: quit before install, status refresh after."""
    result, bridge_verbs = _install_flow(assay_root, yak_shape)
    report = assert_ok(result)
    assert report.status is RailStatus.OK
    assert report.verb == "publish"
    assert report.counts.total >= 3
    assert report.counts.ok >= 3
    assert bridge_verbs == ["quit", "status"]


@pytest.mark.parametrize("build_status, stage_status", [(RailStatus.FAILED, RailStatus.OK), (RailStatus.OK, RailStatus.FAILED)])
def test_publish_stage_step_failure_short_circuits(
    build_status: RailStatus, stage_status: RailStatus, assay_root: AssayHarness, yak_shape: YakShape
) -> None:
    """Failed build or yak build folds into the report without committing or running post-stage steps."""
    meta = yak_shape.materialize(assay_root)
    executor = SeamExecutor(run_fn=_flow_run_check(yak_shape, meta, build_status=build_status, stage_status=stage_status))
    report = assert_ok(publish(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version="1.0.0"), executor))
    assert report.status is RailStatus.FAILED
    assert not (assay_root.root / yak_shape.project.parent / "yak").exists()


# --- [POST_STAGE_POLICY]


def test_finish_empty_policy_returns_staged_unchanged(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """_finish with a verb carrying no policy row returns the staged report verbatim, never resolving a package file."""
    meta = yak_shape.materialize(assay_root)
    staged = fold(Claim.PACKAGE, "plan", (receipt(("yak", "build"), 0, status=RailStatus.OK),))
    assert assert_ok(_finish(assay_root.settings, assay_root.scope(Claim.PACKAGE), meta, yak_shape.slug, "plan", staged, SeamExecutor())) is staged


@pytest.mark.parametrize("bad_status", _NON_OK_STATUSES)
def test_finish_non_ok_stage_skips_post_steps(
    bad_status: RailStatus, assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch
) -> None:
    """_finish skips publish post-stage steps after a non-OK stage status."""
    meta = yak_shape.materialize(assay_root)
    staged = msgspec.structs.replace(fold(Claim.PACKAGE, "publish", ()), status=bad_status)
    monkeypatch.setattr(_pkg_mod, "_resolve_package_file", lambda *_a, **_kw: (_ for _ in ()).throw(AssertionError("must not run")))
    outcome = _finish(assay_root.settings, assay_root.scope(Claim.PACKAGE), meta, yak_shape.slug, "publish", staged, SeamExecutor())
    assert assert_ok(outcome).status is bad_status


def test_drive_steps_bridge_policy_acquires_bridge_lock(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """Bridge publish steps serialize on the shared bridge lease."""
    meta = yak_shape.materialize(assay_root)
    assay_root.supervisor()
    executor = SeamExecutor(run_fn=_flow_run_check(yak_shape, meta))
    staged = fold(Claim.PACKAGE, "publish", (receipt(("yak", "build"), 0, status=RailStatus.OK),))
    steps = _pkg_mod._STEP_POLICY["publish", True]
    package_file = assay_root.write(yak_shape.project.parent / "yak" / yak_shape.package_pattern, "yak")
    scope = assay_root.scope(Claim.PACKAGE)
    with exclusive_lease("bridge", "holder", settings=assay_root.settings) as held:
        assert_ok(held)
        assert_error_status(_drive_steps(assay_root.settings, scope, meta, "publish", staged, package_file, steps, executor), RailStatus.BUSY)
    assert_ok(_drive_steps(assay_root.settings, scope, meta, "publish", staged, package_file, steps, executor))


def test_resolve_package_file_ambiguous_glob_faults(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """_resolve_package_file faults when zero or many files match the package pattern."""
    meta = yak_shape.materialize(assay_root)
    (assay_root.root / meta.package_dir).mkdir(parents=True, exist_ok=True)
    e = assert_error_status(_resolve_package_file(meta), RailStatus.FAULTED)
    assert "expected one package" in e.message


def test_merge_stage_combines_evidence() -> None:
    """_merge_stage sums counts and concatenates results/artifacts/notes, joining the worst status."""
    staged = msgspec.structs.replace(fold(Claim.PACKAGE, "publish", (receipt(("yak", "build"), 0, status=RailStatus.OK),)), notes=("staged-note",))
    steps = msgspec.structs.replace(fold(Claim.PACKAGE, "publish", (receipt(("yak", "install"), 1, status=RailStatus.FAILED),)), notes=("step-note",))
    merged = _merge_stage(staged, steps)
    assert merged.counts.total == staged.counts.total + steps.counts.total
    assert merged.counts.ok == staged.counts.ok + steps.counts.ok
    assert merged.counts.failed == staged.counts.failed + steps.counts.failed
    assert merged.notes == ("staged-note", "step-note")
    assert merged.results == (*staged.results, *steps.results)
    assert merged.status is RailStatus.FAILED


def test_stamp_version_stamps_packagerun_and_defaults() -> None:
    """_stamp_version updates PackageRun and synthesizes missing detail."""
    stamped = _stamp_version(PackageRun(project="p", version="old"), "new")
    assert (stamped.project, stamped.version) == ("p", "new")
    synthesised = _stamp_version(None, "2.0.0")
    assert isinstance(synthesised, PackageRun)
    assert (synthesised.project, synthesised.version) == ("", "2.0.0")


@pytest.mark.parametrize("bad_status", [RailStatus.FAILED, RailStatus.FAULTED, RailStatus.TIMEOUT])
def test_commit_or_fail_non_ok_folds_without_commit(
    bad_status: RailStatus, assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch
) -> None:
    """_commit_or_fail folds non-OK yak receipts without committing."""
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


def test_yak_tool_no_catalog_row_faults(monkeypatch: pytest.MonkeyPatch) -> None:
    """_row faults when the named PACKAGE catalog row is absent."""
    monkeypatch.setattr(_pkg_mod, "select", lambda *_a, **_kw: ())
    e = assert_error_status(_pkg_mod._row("yak", Mode.STAGE), RailStatus.FAULTED)
    assert "no yak catalog row" in e.message


@pytest.mark.parametrize("drop", ["manifest", "primary"])
def test_stage_artifacts_missing_inputs_fault(drop: str, assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """_stage_artifacts faults when the manifest or primary .rhp is absent before yak build."""
    meta = yak_shape.materialize(assay_root)
    target = {"manifest": meta.manifest_dir / "manifest.yml", "primary": meta.target_dir / f"{meta.assembly_name}{meta.target_ext}"}[drop]
    target.unlink()
    staged = Path(assay_root.write("staged-x/.keep", "")).parent
    e = assert_error_status(_pkg_mod._stage_artifacts(meta, staged, meta.target_dir, ()), RailStatus.FAULTED)
    assert ("missing yak manifest" if drop == "manifest" else "missing primary artifact") in e.message


def test_copy_tree_oserror_faults(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """_copy_tree turns a copy failure into a bounded yak-build Fault."""
    meta = yak_shape.materialize(assay_root)
    staged = Path(assay_root.write("staged-c/.keep", "")).parent
    monkeypatch.setattr(_pkg_mod, "copy2", lambda *_a, **_kw: (_ for _ in ()).throw(OSError("disk full")))
    e = assert_error_status(_pkg_mod._copy_tree(meta, staged, meta.target_dir, ()), RailStatus.FAULTED)
    assert "disk full" in e.message


def test_commit_swap_oserror_rolls_back(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """_commit faults and restores the prior package_dir when the staged swap raises OSError."""
    meta = yak_shape.materialize(assay_root)
    staged = Path(assay_root.write("staged-commit/dist.yak", "yak")).parent
    monkeypatch.setattr(Path, "replace", lambda _self, _target: (_ for _ in ()).throw(OSError("cross-device link")))
    e = assert_error_status(_pkg_mod._commit(meta, staged, yak_shape.slug), RailStatus.FAULTED)
    assert "cross-device link" in e.message


def test_stage_build_outputs_fault_cleans_staged_tree(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """_build_outputs Fault propagation removes the staged temp tree."""
    meta = yak_shape.materialize(assay_root)
    parents_before = {p.name for p in (assay_root.root / yak_shape.project.parent).iterdir()}
    spawn_fault = Fault(("dotnet", "build"), message="spawn failed: ENOENT")
    executor = SeamExecutor(run_fn=_flow_run_check(yak_shape, meta, build_fault=spawn_fault))
    e = assert_error(publish(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version="1.0.0"), executor))
    assert "spawn failed" in e.message
    parents_after = {p.name for p in (assay_root.root / yak_shape.project.parent).iterdir()}
    assert parents_after == parents_before, f"leaked staged tree: {parents_after - parents_before}"


# --- [COMMIT_SENTINEL]


@pytest.mark.parametrize("direction, expected", [("absent", "absent"), ("corrupt", "clear"), ("forward", "forward"), ("back", "back")])
def test_recover_direction_matrix(direction: str, expected: str, assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """_recover reports 'absent' with no sentinel, clears corrupt markers, and heals dead-pid markers forward or back."""
    meta = yak_shape.materialize(assay_root)
    previous = _seed_previous(meta) if direction in {"forward", "back"} else None
    marker: Path | None = None
    match direction:
        case "corrupt":
            marker = _marker_path(meta)
            marker.parent.mkdir(parents=True, exist_ok=True)
            marker.write_bytes(b"{not json")
        case "forward" | "back":
            assert previous is not None
            if direction == "forward":
                meta.package_dir.mkdir(parents=True, exist_ok=True)
                (meta.package_dir / "dist.yak").write_bytes(b"committed")
            marker = _seed_marker(meta, _DEAD_PID, previous)
        case _:
            pass
    assert assert_ok(_pkg_mod._recover(meta, yak_shape.slug)) == expected
    assert marker is None or not marker.exists()
    assert previous is None or not previous.exists()
    if direction in {"forward", "back"}:
        assert (meta.package_dir / "dist.yak").read_bytes() == (b"committed" if direction == "forward" else b"old")


def test_recover_live_pid_marker_is_busy(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """Live-pid marker yields BUSY and preserves the pending commit."""
    meta = yak_shape.materialize(assay_root)
    marker = _seed_marker(meta, os.getpid(), meta.package_dir.with_name(f"{meta.package_dir.name}.previous.{os.getpid()}"))
    e = assert_error_status(_pkg_mod._recover(meta, yak_shape.slug), RailStatus.BUSY)
    assert str(os.getpid()) in e.message
    assert marker.exists()


def test_recover_delegates_liveness_to_engine_proc_dead(assay_root: AssayHarness, yak_shape: YakShape, monkeypatch: pytest.MonkeyPatch) -> None:
    """_recover delegates marker liveness to the engine-owned proc_dead ladder."""
    meta = yak_shape.materialize(assay_root)
    previous = _seed_previous(meta)
    marker = _seed_marker(meta, _DEAD_PID, previous)
    monkeypatch.setattr(_pkg_mod, "proc_dead", lambda _pid: False)
    e = assert_error_status(_pkg_mod._recover(meta, yak_shape.slug), RailStatus.BUSY)
    assert str(_DEAD_PID) in e.message
    assert marker.exists()
    assert previous.exists()


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


def test_stage_heals_dead_pid_marker_under_lease(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """Publish heals dead-pid stage markers inside the package lease before committing the distribution."""
    meta = yak_shape.materialize(assay_root)
    previous = _seed_previous(meta)
    marker = _seed_marker(meta, _DEAD_PID, previous)
    assay_root.supervisor()
    executor = SeamExecutor(run_fn=_flow_run_check(yak_shape, meta))
    report = assert_ok(publish(assay_root.settings, assay_root.scope(Claim.PACKAGE), PackageParams(slug=yak_shape.slug, version="1.0.0"), executor))
    assert report.status is RailStatus.OK
    assert not marker.exists()
    assert not previous.exists()
    assert (meta.package_dir / yak_shape.package_pattern).is_file()
