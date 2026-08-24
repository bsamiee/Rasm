"""Model laws for wire determinism, status algebra, report folding, envelopes, and enum payloads.

Every registered wire struct sweeps through encode/decode/re-encode identity; the RailStatus join/fold
algebra pins its full truth table; report arithmetic pins count consistency and fold projection laws.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import itertools
from pathlib import Path
from typing import get_args

from hypothesis import example, given, strategies as st, target
import msgspec
import msgspec.inspect as msgspec_inspect
import pytest

from assay.core.model import (
    AnyDetail,
    ApiResolution,
    ApiSource,
    ApiSurface,
    Artifact,
    ArtifactKind,
    Band,
    Base,
    BaseParams,
    Bind,
    Check,
    Claim,
    Completed,
    ContractsRun,
    Counts,
    Detail,
    Diagnostic,
    Envelope,
    envelope,
    ExecReceipt,
    Fault,
    field_cap,
    HOST_BOUND_CLAIMS,
    Input,
    Language,
    language_choice,
    Match,
    PackageRun,
    Parser,
    ProvisionRun,
    RailStatus,
    receipt,
    Report,
    RunDelta,
    Runner,
    RunSnapshot,
    SarifStatus,
    Stage,
    StaticRun,
    TestRun,
    Tool,
    ToolArgs,
    ToolGroup,
    validate_detail,
    VerifySummary,
    wire_encode,
    wire_safe,
)
from assay.diagnostics import fold
from tests.python._testkit.laws import spec
from tests.python._testkit.spec import assert_roundtrip, idempotent, metamorphic, refutes
from tests.python.tools.assay.kit import (
    api_resolution_st,
    api_source_st,
    api_surface_st,
    artifact_st,
    assert_counts_consistent,
    binds_st,
    completed_st,
    contracts_run_st,
    counts_st,
    detail_st,
    diagnostic_st,
    envelope_st,
    exec_receipt_st,
    fault_st,
    match_st,
    package_run_st,
    provision_run_st,
    rail_status_st,
    report_st,
    run_delta_st,
    run_snapshot_st,
    stage_st,
    static_run_st,
    test_run_st,
    tool_st,
    verify_summary_st,
    WIRE_ENCODER,
)


# --- [CONSTANTS] ------------------------------------------------------------------------

# get_args avoids a manual parallel list that would drift from the union definition.
_DETAIL_VARIANTS: tuple[type[Detail], ...] = get_args(AnyDetail.__value__)

# Each row drives registration and round-trip coverage for one wire struct.
_WIRE_ROWS: tuple[tuple[type[Base], st.SearchStrategy[Base]], ...] = (
    (Stage, stage_st),
    (Tool, tool_st),
    (Artifact, artifact_st),
    (Completed, completed_st),
    (ExecReceipt, exec_receipt_st),
    (Fault, fault_st),
    (Counts, counts_st),
    (Match, match_st),
    (ApiSource, api_source_st),
    (ApiSurface, api_surface_st),
    (VerifySummary, verify_summary_st),
    (TestRun, test_run_st),
    (StaticRun, static_run_st),
    (PackageRun, package_run_st),
    (ProvisionRun, provision_run_st),
    (ContractsRun, contracts_run_st),
    (ApiResolution, api_resolution_st),
    (Diagnostic, diagnostic_st),
    (RunSnapshot, run_snapshot_st),
    (RunDelta, run_delta_st),
    (Report, report_st),
    (Envelope, envelope_st),
)

_STATUSES: tuple[RailStatus, ...] = tuple(RailStatus)

_FROM_RC: tuple[tuple[int, RailStatus], ...] = (
    (0, RailStatus.EMPTY),
    (5, RailStatus.BUSY),
    (124, RailStatus.TIMEOUT),
    (1, RailStatus.FAILED),
    (2, RailStatus.FAILED),
    (127, RailStatus.FAILED),
    (255, RailStatus.FAILED),
)

# Pre-trace history fixture keeps additive Diagnostic defaults hermetic.
_PRE_TRACE_ENVELOPE: bytes = (
    b'{"claim":"static","verb":"fix","status":"faulted","exit_code":2,'
    b'"run_id":"2026-06-10T05-34-31.783586-61744","error":{"argv":[],"message":"parse: x"},'
    b'"error_context":{"kind":"diagnostic","failing_step":"parse",'
    b'"recent_events":["dispatch=static","static fix","dispatch=static","static fix"],'
    b'"elapsed_ms":0.0,"hint":"parse: x after 0.0ms",'
    b'"resource":[["mem.rss_bytes",95715328.0],["sys.mem_percent",52.6],["sys.swap_percent",80.9]]}}'
)

COVERS: tuple[object, ...] = (
    *(row[0] for row in _WIRE_ROWS),
    Band,
    Base,
    BaseParams,
    Bind,
    Detail,
    envelope,
    field_cap,
    fold,
    language_choice,
    receipt,
    ToolArgs,
    validate_detail,
    wire_encode,
    wire_safe,
)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [WIRE_ROUNDTRIP]


@spec(Check, law="execution_model_never_wire")
def test_check_draws_live_paths_and_the_wire_encoder_refuses_them(check: Check) -> None:
    """Check is the in-process execution plan, never a wire struct.

    Drawn ``cwd`` Paths are live values, and the hook-free wire encoder refuses a cwd-bearing
    Check by construction.
    """
    match check.cwd:
        case None:
            pass
        case cwd:
            assert isinstance(cwd, Path), f"cwd drew {cwd!r}, not a Path"
            with pytest.raises(TypeError, match="unsupported"):
                wire_encode(check)


@pytest.mark.mutation
@pytest.mark.parametrize("subject, strategy", _WIRE_ROWS, ids=[row[0].__name__ for row in _WIRE_ROWS])
def test_wire_struct_round_trips(subject: type[Base], strategy: st.SearchStrategy[Base]) -> None:
    """Every conftest-registered wire struct survives the deterministic codec byte-identically.

    The re-encode identity step catches non-deterministic codecs and non-decode-clean fields.
    """

    @given(strategy)
    def _probe(value: Base) -> None:
        assert_roundtrip(value, subject, codec=WIRE_ENCODER)

    _probe()


@given(detail_st)
def test_any_detail_contract(detail: AnyDetail) -> None:
    """Every AnyDetail variant subclasses Detail, survives both codecs, validates, and rejects surplus keys."""
    assert isinstance(detail, Detail)
    assert_roundtrip(detail, type(detail), codec=WIRE_ENCODER)
    assert msgspec.json.decode(wire_encode(detail), type=type(detail)) == detail
    assert validate_detail(detail) == detail
    raw: dict[str, object] = msgspec.json.decode(WIRE_ENCODER.encode(detail), type=dict)
    raw["__probe__"] = 1
    with pytest.raises(msgspec.ValidationError, match="__probe__"):
        msgspec.json.decode(msgspec.json.encode(raw), type=type(detail))


def test_any_detail_tags_are_injective() -> None:
    """Every AnyDetail variant carries a unique msgspec tag — wire disambiguation is injective."""
    tags = [t.tag for v in _DETAIL_VARIANTS if isinstance(t := msgspec_inspect.type_info(v), msgspec_inspect.StructType)]
    assert len(tags) == len(set(tags)), f"duplicate tags: {tags}"


# --- [STATUS_ALGEBRA]


@pytest.mark.mutation
def test_dominant_full_truth_table_left_wins_ties() -> None:
    """Dominant is max-by-severity returning an operand, left on ties.

    Severity injectivity pins ties to the diagonal today; the ``>=`` oracle keeps left-bias law-bound
    should a future member share a rank. The full product subsumes associativity, commutativity-on-
    severity, monotonicity, EMPTY-identity, and FAULTED-absorption as corollaries of max.
    """
    assert len({s.severity for s in _STATUSES}) == len(_STATUSES), "severity ranks must stay injective"
    for a in _STATUSES:
        assert RailStatus.dominant(a, a) is a
        for b in _STATUSES:
            assert RailStatus.dominant(a, b) is (a if a.severity >= b.severity else b)


@pytest.mark.mutation
@given(st.lists(rail_status_st, max_size=8))
def test_fold_is_max_severity_floored_at_empty(members: list[RailStatus]) -> None:
    """RailStatus.fold returns the max-by-severity member floored at the EMPTY seed; ``fold()`` is EMPTY."""
    result = RailStatus.fold(*members)
    assert result in {*members, RailStatus.EMPTY}
    assert result.severity == max([RailStatus.EMPTY.severity, *(m.severity for m in members)])
    assert RailStatus.fold() is RailStatus.EMPTY


def test_severity_ordering_is_strict_declaration_ascent() -> None:
    """Members ascend strictly in severity in declaration order: SKIP is minimal, FAULTED the absorbing max."""
    assert all(lo.severity < hi.severity for lo, hi in itertools.pairwise(_STATUSES))
    assert _STATUSES[0] is RailStatus.SKIP
    assert _STATUSES[-1] is RailStatus.FAULTED


@pytest.mark.mutation
@pytest.mark.parametrize("rc,expected", _FROM_RC, ids=[f"rc={r}" for r, _ in _FROM_RC])
def test_from_returncode_closed_table(rc: int, expected: RailStatus) -> None:
    """from_returncode maps {0->EMPTY, 5->BUSY, 124->TIMEOUT, *->FAILED} exactly."""
    assert RailStatus.from_returncode(rc) is expected


def test_alias_skipped_resolves_to_skip() -> None:
    """RailStatus('skipped') is RailStatus.SKIP — the wire alias contract."""
    assert RailStatus("skipped") is RailStatus.SKIP  # type: ignore[call-arg]  # enum value lookup, not the 3-arg member constructor


# --- [CENSUS]


def _census_law(statuses: tuple[RailStatus, ...], expected: Counts) -> None:
    """Oracle: seating ``statuses`` yields exactly ``expected``.

    Raises:
        AssertionError: When the census over ``statuses`` differs from ``expected``.
    """
    assert Counts.of(*statuses) == expected, f"census {Counts.of(*statuses)} != {expected}"


def test_only_a_reached_verdict_bands_as_proved_or_refused() -> None:
    """The band column is the closed partition of the status vocabulary: a verdict bands PROVED or REFUSED, everything else UNPROVED.

    This is the anti-drift gate on ``RailStatus.band``. A member minted without a band cannot compile; a member minted
    with the wrong band lands here, because the roster each band admits is spelled out rather than derived from the column.
    """
    banded = {band: {status for status in RailStatus if status.band is band} for band in Band}
    assert banded[Band.PROVED] == {RailStatus.OK, RailStatus.EMPTY}, "only a lane that ran and answered clean is proved"
    assert banded[Band.REFUSED] == {RailStatus.FAILED}, "only a lane that ran and found defects is refused"
    assert banded[Band.UNPROVED] == {
        RailStatus.SKIP,
        RailStatus.DEGRADED,
        RailStatus.CANDIDATE,
        RailStatus.UNSUPPORTED,
        RailStatus.BUSY,
        RailStatus.TIMEOUT,
        RailStatus.FAULTED,
    }, "a lane that reached no verdict proves nothing, however clean its exit code"
    assert set().union(*banded.values()) == set(RailStatus), "the bands must cover the vocabulary"


@pytest.mark.mutation
@given(st.lists(rail_status_st, max_size=12))
def test_census_seats_every_leaf_under_its_own_status(statuses: list[RailStatus]) -> None:
    """Every leaf seats one row under its own status: nothing folds onto a neighbour, nothing vanishes, rows keep vocabulary order.

    Falsified by: bucketing SKIP with OK, dropping a terminal status from the tally, or emitting an unordered or empty row.
    """
    counts = Counts.of(*statuses)
    target(float(len(statuses)), label="census_leaf_count")
    assert counts.total == len(statuses)
    assert dict(counts.by_status) == {status: statuses.count(status) for status in set(statuses)}
    assert tuple(status for status, _ in counts.by_status) == tuple(status for status in RailStatus if status in set(statuses))
    assert all(rows > 0 for _, rows in counts.by_status)


@pytest.mark.mutation
@given(st.lists(rail_status_st, max_size=12))
def test_census_bands_partition_the_leaves(statuses: list[RailStatus]) -> None:
    """The band projection partitions the census: each band totals its own statuses and the three sum to the whole."""
    counts = Counts.of(*statuses)
    assert sum(counts.band(band) for band in Band) == counts.total
    for band in Band:
        assert counts.band(band) == sum(1 for status in statuses if status.band is band), band


@given(st.lists(rail_status_st, max_size=8), st.lists(rail_status_st, max_size=8))
def test_census_combines_associatively_across_partial_folds(left: list[RailStatus], right: list[RailStatus]) -> None:
    """Seating two partial censuses equals seating both leaf sets at once, with the empty census as identity.

    This is why the bands are read-time projections rather than stored slots: partial sums of a lossy partition
    cannot recombine, while per-status rows can.
    """
    whole = Counts.of(*left, *right)
    assert Counts.of(Counts.of(*left), Counts.of(*right)) == whole
    assert Counts.of(Counts(), whole) == whole
    assert Counts.of() == Counts()


def test_census_law_refutes_every_way_a_leaf_could_be_miscounted() -> None:
    """The census oracle fails on each historical miscount: a skip folded onto a proof, and a terminal status erased."""
    _census_law((RailStatus.SKIP,), Counts.of(RailStatus.SKIP))
    _census_law((RailStatus.FAULTED, RailStatus.FAILED), Counts.of(RailStatus.FAILED, RailStatus.FAULTED))
    refutes((RailStatus.SKIP,), _census_law, Counts.of(RailStatus.OK))  # a governed skip counted as a proof
    refutes((RailStatus.EMPTY,), _census_law, Counts.of(RailStatus.SKIP))  # a clean silent exit counted as never run
    refutes((RailStatus.FAULTED,), _census_law, Counts())  # a broken tool erased from the tally
    refutes((RailStatus.TIMEOUT,), _census_law, Counts())  # a lane cut short erased from the tally
    refutes((RailStatus.OK, RailStatus.OK), _census_law, Counts.of(RailStatus.OK))  # a repeated status seated once


# --- [FOLD]


@pytest.mark.mutation
@given(st.lists(completed_st, min_size=0, max_size=20))
def test_fold_census_seats_one_row_per_outcome(outcomes: list[Completed]) -> None:
    """The fold seats every outcome in the census under its own status, so no lane is folded onto a proof or dropped."""
    tup = tuple(outcomes)
    report = fold(Claim.STATIC, "check", tup)
    target(float(len(tup)), label="fold_outcome_count")
    assert report.counts.total == len(tup)
    assert dict(report.counts.by_status) == {status: sum(1 for o in tup if o.status is status) for status in {o.status for o in tup}}
    assert report.counts.band(Band.PROVED) == sum(1 for o in tup if o.status in {RailStatus.OK, RailStatus.EMPTY})
    assert report.counts.band(Band.REFUSED) == sum(1 for o in tup if o.status is RailStatus.FAILED)


@pytest.mark.mutation
@given(st.lists(completed_st, min_size=1, max_size=10))
def test_fold_mints_one_process_defect_row_per_refused_outcome(outcomes: list[Completed]) -> None:
    """Each REFUSED outcome mints exactly one process-defect row, and no other outcome does.

    The census band and the defect-row predicate are the one partition, so the two can never disagree about which
    outcome carried a defect. Parsed tool diagnostics ride the same ``results`` tuple as separate evidence — a
    ``Parser``-stamped clean lane yields code rows and no defect row — so the law counts process rows, not rows.
    """
    report = fold(Claim.CODE, "check", tuple(outcomes))
    defects = tuple(m for m in report.results if m.kind is ArtifactKind.PROCESS)
    assert len(defects) == report.counts.band(Band.REFUSED)
    assert all(m.severity == "failed" for m in defects)
    assert report.counts.band(Band.REFUSED) == sum(1 for o in outcomes if o.status is RailStatus.FAILED)
    assert_counts_consistent(report)


def test_fold_empty_outcomes_is_empty_report() -> None:
    """Fold over an empty tuple yields an unseated census and EMPTY status."""
    report = fold(Claim.TEST, "run", ())
    assert report.counts == Counts()
    assert report.status is RailStatus.EMPTY


def test_fold_merges_every_remote_receipt_not_just_the_first() -> None:
    """A multi-check remote fold folds every outcome's ExecReceipt: push/pull counts sum, notes concat, host identity stays.

    A fan-out over one ``exec_target`` yields one receipt per check; the carrier must surface all transfer evidence,
    not silently drop every receipt after the first as ``next(...)`` did.
    """
    rx = ExecReceipt(target="ssh://root@vps:22", host="vps", exit_status=0, pushed=3, pulled=2, notes=("a",))
    ry = ExecReceipt(target="ssh://root@vps:22", host="vps", exit_status=0, pushed=4, pulled=1, notes=("b",))
    outcomes = (msgspec.structs.replace(receipt(("x",), 0), exec=rx), msgspec.structs.replace(receipt(("y",), 0), exec=ry))
    report = fold(Claim.STATIC, "check", outcomes)
    assert report.exec is not None
    assert (report.exec.pushed, report.exec.pulled) == (7, 3), f"counts did not sum across receipts: {report.exec!r}"
    assert report.exec.notes == ("a", "b"), f"notes did not concat across receipts: {report.exec.notes!r}"
    assert (report.exec.host, report.exec.target) == ("vps", "ssh://root@vps:22")


def test_fold_local_run_leaves_exec_carrier_none() -> None:
    """A fold over outcomes that never offloaded leaves the dedicated exec carrier None."""
    assert fold(Claim.STATIC, "check", (receipt(("ruff",), 0),)).exec is None


def test_fold_failed_defect_row_carries_argv_id_and_stderr_tail() -> None:
    """A FAILED receipt's defect row ids the shell-rendered argv and carries the 4 KiB stderr tail."""
    payload = b"x" * 5000
    report = fold(Claim.STATIC, "build", (receipt(("dotnet", "format", "src/App.csproj"), 1, stderr=payload),))
    assert report.results
    assert report.results[0].id == "dotnet format src/App.csproj"
    assert report.results[0].text == payload[-4096:].decode()


def _stamped(done: Completed, parser: Parser) -> Completed:
    # Mirrors the engine's receipt-time stamp; receipt() itself never keys a diagnostics family.
    return msgspec.structs.replace(done, parser=parser)


def test_fold_ignores_argv_text_without_parser_stamp() -> None:
    """An unstamped receipt contributes no parsed diagnostic rows even when argv names a known tool — argv sniffing is dead."""
    payload = b"pkg/a.py:3:5: error: Incompatible types in assignment [assignment]\n"
    report = fold(Claim.STATIC, "check", (receipt(("uv", "run", "mypy"), 1, stdout=payload),))
    assert [m.severity for m in report.results] == ["failed"], "unstamped output must fold to the defect tail only"


def test_fold_non_static_claims_keep_converted_rows_and_stay_inert_for_none() -> None:
    """A non-static claim keeps the rows its stamped parser converted ahead of the defect tail; a Parser.NONE receipt converts to nothing."""
    annotation = b'{"path":"p.proto","start_line":4,"start_column":1,"end_line":4,"end_column":9,"type":"PACKAGE_DIRECTORY_MATCH","message":"m"}\n'
    stamped = _stamped(receipt(("buf", "lint"), 100, stdout=annotation, status=RailStatus.FAILED), Parser.BUF)
    report = fold(Claim.CONTRACTS, "check", (stamped,))
    assert [(m.id, m.severity) for m in report.results] == [("buf:package_directory_match", "error"), ("buf lint", "failed")]
    plain = fold(Claim.CONTRACTS, "check", (receipt(("buf", "lint"), 100, stdout=annotation, status=RailStatus.FAILED),))
    assert [m.severity for m in plain.results] == ["failed"], "an unstamped receipt must fold to the defect tail alone"


# --- [SARIF_FOLD]


def _sarif_result(rule: str, level: str | None, line: int, message: str) -> dict[str, object]:
    location = {"physicalLocation": {"artifactLocation": {"uri": "src/Probe.cs"}, "region": {"startLine": line, "startColumn": 1}}}
    base: dict[str, object] = {"ruleId": rule, "message": {"text": message}, "locations": [location]}
    return base if level is None else {**base, "level": level}


def _sarif_doc(*results: dict[str, object], runs: int = 1) -> bytes:
    run = {"tool": {"driver": {"name": "Microsoft (R) Visual C# Compiler", "rules": []}}, "columnKind": "utf16CodeUnits", "results": list(results)}
    return msgspec.json.encode({"$schema": "https://json.schemastore.org/sarif-2.1.0.json", "version": "2.1.0", "runs": [run] * runs})


def _sarif_drop(tmp_path: Path, **files: bytes) -> str:
    sarif_dir = tmp_path / "sarif"
    sarif_dir.mkdir(exist_ok=True)
    for name, payload in files.items():
        (sarif_dir / f"{name}.sarif").write_bytes(payload)
    return str(sarif_dir)


@pytest.mark.parametrize(
    "level, severity",
    [("error", "error"), ("warning", "warning"), ("note", "info"), ("none", "info"), (None, "warning")],
    ids=["error", "warning", "note_rides_info", "none_rides_info", "absent_defaults_warning"],
)
def test_fold_sarif_level_maps_to_assay_severity(level: str | None, severity: str, tmp_path: Path) -> None:
    """SARIF levels map to assay severity while absent levels keep the SARIF warning default."""
    sarif_dir = _sarif_drop(tmp_path, probe=_sarif_doc(_sarif_result("CSP0903", level, 12, "tone probe")))
    report = fold(Claim.STATIC, "build", (receipt(("dotnet",), 0, status=RailStatus.OK),), sarif_dir=sarif_dir)
    assert [(m.id, m.kind, m.severity, m.path, m.line, m.column, m.message) for m in report.results] == [
        ("csp0903", ArtifactKind.CODE, severity, "src/Probe.cs", 12, 1, "tone probe")
    ]
    assert "src/Probe.cs" in report.results[0].text
    assert "tone probe" in report.results[0].text


def test_fold_sarif_error_rows_fail_static_report(tmp_path: Path) -> None:
    """Error-level SARIF rows make static folds fail while exact duplicate source diagnostics dedupe before capping."""
    sarif_dir = _sarif_drop(
        tmp_path,
        a=_sarif_doc(_sarif_result("CSP0101", "error", 3, "alpha"), _sarif_result("CSP0202", "warning", 7, "beta")),
        b=_sarif_doc(_sarif_result("CSP0903", "note", 1, "gamma"), runs=2),
        broken=b"{ not sarif",
    )
    report = fold(Claim.STATIC, "build", (receipt(("dotnet",), 0, status=RailStatus.OK),), sarif_dir=sarif_dir)
    assert [m.id for m in report.results] == ["csp0101", "csp0202", "csp0903"]
    assert report.status is RailStatus.FAILED
    assert report.counts == Counts.of(RailStatus.OK)
    assert envelope(report, claim=Claim.STATIC, verb="build").exit_code == 1


def test_fold_sarif_suppressed_error_is_dropped_and_does_not_gate(tmp_path: Path) -> None:
    """A pragma-suppressed error-level SARIF result never surfaces or gates, so the rail tracks dotnet build rc=0."""
    suppressed = {**_sarif_result("CA1822", "error", 5, "suppressed in source"), "suppressions": [{"kind": "inSource"}]}
    sarif_dir = _sarif_drop(tmp_path, probe=_sarif_doc(suppressed))
    report = fold(Claim.STATIC, "build", (receipt(("dotnet",), 0, status=RailStatus.OK),), sarif_dir=sarif_dir)
    assert report.results == ()
    assert report.status is RailStatus.OK


def test_fold_sarif_findings_scope_to_built_project_stem(tmp_path: Path) -> None:
    """A .csproj build keys only its own <stem>.sarif so a dependency project never leaks cross-project findings."""
    sarif_dir = _sarif_drop(
        tmp_path, a=_sarif_doc(_sarif_result("CSP0101", "error", 3, "target")), b=_sarif_doc(_sarif_result("CSP0202", "error", 7, "dependency"))
    )
    report = fold(Claim.STATIC, "build", (receipt(("dotnet", "build", "a.csproj"), 0, status=RailStatus.OK),), sarif_dir=sarif_dir)
    assert [m.id for m in report.results] == ["csp0101"]


def test_fold_sarif_reads_build_scoped_csp_sarif_dirs(tmp_path: Path) -> None:
    """Build receipts read their own typed sarif_dir stamp, with .csproj rows still scoped to the target project stem."""
    sarif_dir = tmp_path / "sarif"
    app_dir, lib_dir = sarif_dir / "App-a1", sarif_dir / "Lib-b2"
    app_dir.mkdir(parents=True)
    lib_dir.mkdir()
    (app_dir / "App.sarif").write_bytes(_sarif_doc(_sarif_result("CSP0101", "error", 3, "target")))
    (app_dir / "Dep.sarif").write_bytes(_sarif_doc(_sarif_result("CSP0202", "error", 7, "dependency")))
    (lib_dir / "Lib.sarif").write_bytes(_sarif_doc(_sarif_result("CSP0303", "warning", 11, "second")))
    # The engine stamps Completed.sarif_dir from Check.args; the fold never re-parses the argv token.
    outcomes = (
        msgspec.structs.replace(receipt(("dotnet", "build", "src/App/App.csproj"), 0, status=RailStatus.OK), sarif_dir=str(app_dir)),
        msgspec.structs.replace(receipt(("dotnet", "build", "src/Lib/Lib.csproj"), 0, status=RailStatus.OK), sarif_dir=str(lib_dir)),
    )
    report = fold(Claim.STATIC, "build", outcomes, sarif_dir=str(sarif_dir))
    assert [m.id for m in report.results] == ["csp0101", "csp0303"]
    assert report.status is RailStatus.FAILED


def test_fold_static_source_diagnostics_precede_defect_rows(tmp_path: Path) -> None:
    """Static folds rank source diagnostics ahead of process-tail fallback rows."""
    sarif_dir = _sarif_drop(tmp_path, probe=_sarif_doc(_sarif_result("CSP0101", "error", 3, "alpha")))
    report = fold(Claim.STATIC, "build", (receipt(("dotnet",), 1, stderr=b"CS0103: boom"),), sarif_dir=sarif_dir)
    assert [(m.id, m.severity) for m in report.results] == [("csp0101", "error"), ("dotnet", "failed")]
    assert report.status is RailStatus.FAILED
    assert report.counts == Counts.of(RailStatus.FAILED)


def test_fold_static_note_only_sarif_promotes_empty_receipt_to_ok(tmp_path: Path) -> None:
    """``promote_empty`` keeps a note-only SARIF static run ok, not empty."""
    sarif_dir = _sarif_drop(tmp_path, probe=_sarif_doc(_sarif_result("CSP0903", "note", 1, "gamma")))
    report = fold(Claim.STATIC, "build", (receipt(("dotnet",), 0, status=RailStatus.EMPTY),), sarif_dir=sarif_dir, promote_empty=True)
    assert [(m.id, m.severity) for m in report.results] == [("csp0903", "info")]
    assert report.status is RailStatus.OK
    assert report.counts == Counts.of(RailStatus.EMPTY)


def test_fold_static_green_executed_rows_are_ok() -> None:
    """A ``promote_empty`` static check that ran clean with no diagnostics reports ok instead of empty."""
    report = fold(Claim.STATIC, "check", (receipt(("ruff",), 0, status=RailStatus.EMPTY),), promote_empty=True)
    assert report.results == ()
    assert report.status is RailStatus.OK
    assert report.counts == Counts.of(RailStatus.EMPTY)


def test_fold_promote_empty_is_opt_in_per_claim() -> None:
    """``promote_empty`` gates the promotion: an eligible claim stays empty by default and folds to ok only on opt-in."""
    for claim in (Claim.STATIC, Claim.BRIDGE, Claim.PACKAGE, Claim.PROVISION, Claim.CONTRACTS, Claim.TEST):
        outcomes = (receipt((claim.value,), 0, status=RailStatus.EMPTY),)
        assert fold(claim, "check", outcomes).status is RailStatus.EMPTY
        promoted = fold(claim, "check", outcomes, promote_empty=True)
        assert promoted.status is RailStatus.OK
        assert promoted.counts == Counts.of(RailStatus.EMPTY)


def test_fold_csharp_process_output_parses_and_dedupes_source_diagnostics() -> None:
    """Dotnet format/build output becomes source Match rows before fallback tails, with exact duplicates collapsed."""
    line = b"src/App/HostControl.cs(148,71): error VSTHRD002: Synchronously waiting on tasks may deadlock [src/App/App.csproj]"
    report = fold(Claim.STATIC, "build", (_stamped(receipt(("dotnet", "build"), 1, stdout=line + b"\n" + line), Parser.CS_CONSOLE),))
    assert [(m.id, m.kind, m.severity, m.path, m.line, m.column, m.score, m.project) for m in report.results[:2]] == [
        ("vsthrd002", ArtifactKind.CODE, "error", "src/App/HostControl.cs", 148, 71, 71, "src/App/App.csproj"),
        ("dotnet build", ArtifactKind.PROCESS, "failed", "", 0, 0, 0, ""),
    ]
    assert report.results[0].message == "Synchronously waiting on tasks may deadlock"
    assert "HostControl.cs(148,71)" in report.results[0].text


@pytest.mark.parametrize(
    "parser, payload, expected",
    [
        (Parser.RUFF, b"error[F401]: unused import\n --> pkg/a.py:1:1\n", ("ruff:f401", "error", "pkg/a.py", 1, 1, "unused import")),
        (
            Parser.RUFF,
            b"line-too-long: Line too long (165 > 150)\n --> pkg/a.py:3:151\n",
            ("ruff:line-too-long", "error", "pkg/a.py", 3, 151, "Line too long (165 > 150)"),
        ),
        (
            Parser.TY,
            b"error[unresolved-attribute]: object has no member\n --> pkg/a.py:2:9\n",
            ("ty:unresolved-attribute", "error", "pkg/a.py", 2, 9, "object has no member"),
        ),
        (
            Parser.MYPY,
            b"pkg/a.py:3:5: error: Incompatible types in assignment [assignment]\n",
            ("mypy:assignment", "error", "pkg/a.py", 3, 5, "Incompatible types in assignment"),
        ),
        (
            Parser.TSC,
            b"src/a.ts(4,7): error TS2322: Type 'number' is not assignable to type 'string'.\n",
            ("tsc:ts2322", "error", "src/a.ts", 4, 7, "Type 'number' is not assignable to type 'string'."),
        ),
        (Parser.RUFF_FORMAT, b"Would reformat: pkg/a.py\n", ("ruff-format:format", "error", "pkg/a.py", 0, 0, "file would be reformatted")),
        (
            Parser.RUFF_FORMAT,
            b"unformatted: File would be reformatted\n --> pkg/a.py:1:1\n",
            ("ruff-format:unformatted", "error", "pkg/a.py", 1, 1, "File would be reformatted"),
        ),
    ],
    ids=["ruff", "ruff-full", "ty", "mypy", "tsc", "ruff-format", "ruff-format-full"],
)
def test_fold_static_text_tools_emit_structured_diagnostics(parser: Parser, payload: bytes, expected: tuple[str, str, str, int, int, str]) -> None:
    """Text diagnostics from Python and TypeScript tools become first-class source Match rows, keyed by the parser stamp."""
    report = fold(Claim.STATIC, "check", (_stamped(receipt(("tool",), 1, stdout=payload), parser),))
    row = report.results[0]
    assert (row.id, row.severity, row.path, row.line, row.column, row.message) == expected


def test_fold_static_json_tools_emit_structured_diagnostics() -> None:
    """Embedded Biome JSON diagnostics become first-class source Match rows."""
    biome_payload = msgspec.json.encode({
        "diagnostics": [
            {
                "severity": "warning",
                "message": "unused variable",
                "category": "lint/correctness/noUnusedVariables",
                "location": {"path": "src/a.ts", "start": {"line": 6, "column": 3}},
            }
        ]
    })
    report = fold(
        Claim.STATIC,
        "check",
        (_stamped(receipt(("pnpm", "exec", "biome"), 1, stdout=b"Checked 1 file\n" + biome_payload + b"\nFound 1 error.\n"), Parser.BIOME),),
    )
    assert [(row.id, row.severity, row.path, row.line, row.column, row.message) for row in report.results[:1]] == [
        ("biome:lint/correctness/nounusedvariables", "warning", "src/a.ts", 6, 3, "unused variable")
    ]


def test_fold_csharp_same_location_distinct_messages_are_distinct() -> None:
    """Same-location compiler rows with different messages remain separate structured diagnostics."""
    first = b"src/App/Probe.cs(30,35): error CS0736: ShellStub.PingAsync cannot implement static member"
    second = b"src/App/Probe.cs(30,35): error CS0736: ShellStub.PrepareQuitAsync cannot implement static member"
    report = fold(Claim.STATIC, "build", (_stamped(receipt(("dotnet", "build"), 1, stdout=b"\n".join((first, second))), Parser.CS_CONSOLE),))
    assert [row.message for row in report.results[:2]] == [
        "ShellStub.PingAsync cannot implement static member",
        "ShellStub.PrepareQuitAsync cannot implement static member",
    ]


def test_fold_static_dedupes_process_and_sarif_source_diagnostics(tmp_path: Path) -> None:
    """The same source diagnostic from compiler text and SARIF collapses to one normalized Match row."""
    source = str(tmp_path / "src" / "App" / "HostControl.cs")
    project = str(tmp_path / "src" / "App" / "App.csproj")
    line = f"{source}(148,71): error VSTHRD002: Synchronously waiting on tasks may deadlock [{project}]".encode()
    sarif_dir = _sarif_drop(
        tmp_path,
        probe=msgspec.json.encode({
            "version": "2.1.0",
            "runs": [
                {
                    "results": [
                        {
                            "ruleId": "VSTHRD002",
                            "level": "error",
                            "message": {"text": "Synchronously waiting on tasks may deadlock"},
                            "locations": [
                                {
                                    "physicalLocation": {
                                        "artifactLocation": {"uri": f"file://{source}"},
                                        "region": {"startLine": 148, "startColumn": 71},
                                    }
                                }
                            ],
                        }
                    ]
                }
            ],
        }),
    )
    report = fold(Claim.STATIC, "build", (_stamped(receipt(("dotnet", "build"), 1, stdout=line), Parser.CS_CONSOLE),), sarif_dir=sarif_dir)
    assert [(m.id, m.path, m.line, m.column) for m in report.results[:2]] == [("vsthrd002", source, 148, 71), ("dotnet build", "", 0, 0)]
    assert "diagnostics: total=1 source=1 generated=0 error=1 warning=0 info=0" in report.notes
    assert "diagnostics.rules: vsthrd002=1" in report.notes


def test_fold_static_dedupes_relative_console_against_absolute_sarif(tmp_path: Path) -> None:
    """A cwd-relative console path and an absolute SARIF uri at one location collapse to a single source row.

    Dropping ``/clp:ErrorsOnly`` lets the console emit the cwd-relative path form while the analyzer's SARIF keeps the
    absolute ``file://`` uri; both anchor on the assay cwd, so the cross-channel dedup key matches and the row appears once.
    """
    relative = "src/App/HostControl.cs"
    absolute = str((Path.cwd() / relative).as_posix())
    line = f"{relative}(148,71): error VSTHRD002: Synchronously waiting on tasks may deadlock".encode()
    sarif_dir = _sarif_drop(
        tmp_path,
        probe=msgspec.json.encode({
            "version": "2.1.0",
            "runs": [
                {
                    "results": [
                        {
                            "ruleId": "VSTHRD002",
                            "level": "error",
                            "message": {"text": "Synchronously waiting on tasks may deadlock"},
                            "locations": [
                                {
                                    "physicalLocation": {
                                        "artifactLocation": {"uri": f"file://{absolute}"},
                                        "region": {"startLine": 148, "startColumn": 71},
                                    }
                                }
                            ],
                        }
                    ]
                }
            ],
        }),
    )
    report = fold(Claim.STATIC, "build", (_stamped(receipt(("dotnet", "build"), 1, stdout=line), Parser.CS_CONSOLE),), sarif_dir=sarif_dir)
    source_rows = tuple(m for m in report.results if m.id == "vsthrd002")
    assert len(source_rows) == 1
    assert "diagnostics: total=1 source=1 generated=0 error=1 warning=0 info=0" in report.notes


def test_fold_static_groups_generated_diagnostics_after_source_rows() -> None:
    """Generated obj diagnostics are grouped so repeated CS0436 rows cannot crowd source errors out of the cap."""
    source = b"tools/rhino-bridge/Shell/ShellHost.cs(297,22): error MA0006: Use string.Equals instead of Equals operator"
    generated = (
        b".artifacts/assay/build/abc/Release/obj/Debug/net10.0/Scenarios.GlobalUsings.g.cs(18,25): "
        b"warning CS0436: The type conflicts with imported type [tools/rhino-bridge/Scenarios.csproj]"
    )
    stamped = _stamped(receipt(("dotnet", "build"), 1, stdout=b"\n".join((generated, source, generated))), Parser.CS_CONSOLE)
    report = fold(Claim.STATIC, "build", (stamped,))
    assert [(m.id, m.kind, m.severity, m.count) for m in report.results[:3]] == [
        ("ma0006", ArtifactKind.CODE, "error", 0),
        ("cs0436", ArtifactKind.PROCESS, "warning", 2),
        ("dotnet build", ArtifactKind.PROCESS, "failed", 0),
    ]
    assert report.results[1].text.startswith("generated diagnostics grouped count=2:")
    assert "diagnostics: total=3 source=1 generated=2 error=1 warning=2 info=0" in report.notes
    assert "diagnostics.rules: cs0436=2 ma0006=1" in report.notes


def test_fold_static_generated_errors_are_evidence_not_failure() -> None:
    """Generated ``obj`` diagnostics stay visible without making an otherwise successful static fold fail."""
    generated = (
        b".artifacts/assay/build/abc/Release/obj/Debug/net10.0/Scenarios.GlobalUsings.g.cs(18,25): "
        b"error CS0436: The type conflicts with imported type [tools/rhino-bridge/Scenarios.csproj]"
    )
    stamped = _stamped(receipt(("dotnet", "build"), 0, status=RailStatus.EMPTY, stdout=generated), Parser.CS_CONSOLE)
    report = fold(Claim.STATIC, "build", (stamped,), promote_empty=True)
    assert [(m.id, m.kind, m.severity, m.count) for m in report.results] == [("cs0436", ArtifactKind.PROCESS, "error", 1)]
    assert report.status is RailStatus.OK
    assert "diagnostics: total=1 source=0 generated=1 error=1 warning=0 info=0" in report.notes


@pytest.mark.parametrize(
    "path",
    [
        "libs/python/contracts/rasm/contracts/gen/rasm/contracts/compute/compute_connect.py",
        "libs/typescript/contracts/gen/rasm/contracts/compute/compute_pb.ts",
        "libs/csharp/Rasm.Contracts/Generated/Compute/ComputeGrpc.cs",
    ],
    ids=["python", "typescript", "csharp"],
)
def test_fold_static_contracts_out_roots_census_as_generated(path: str) -> None:
    """Rows under a committed generated out root census as generated evidence, and the tool's own exit still fails the lane.

    A relative path is the shape every text parser emits, so the roster matches without a leading slash; an unlisted
    root would census as source and flood the display cap on every generator bump.
    """
    payload = f"error[missing-override-decorator]: overrides without @override\n --> {path}:7:9\n".encode()
    report = fold(Claim.STATIC, "check", (_stamped(receipt(("ty",), 1, stdout=payload), Parser.TY),))
    assert [(m.id, m.kind, m.count) for m in report.results[:1]] == [("ty:missing-override-decorator", ArtifactKind.PROCESS, 1)]
    assert report.status is RailStatus.FAILED
    assert "diagnostics: total=1 source=0 generated=1 error=1 warning=0 info=0" in report.notes


def test_fold_sarif_absent_or_empty_dir_is_silent(tmp_path: Path) -> None:
    """A None, missing, or empty sarif directory contributes no rows and leaves the fold untouched."""
    for sarif_dir in (None, str(tmp_path / "missing" / "sarif"), _sarif_drop(tmp_path)):
        report = fold(Claim.STATIC, "build", (receipt(("dotnet",), 0, status=RailStatus.OK),), sarif_dir=sarif_dir)
        assert report.results == ()
        assert report.status is RailStatus.OK


# --- [ENVELOPE]


@given(st.lists(completed_st, min_size=0, max_size=5))
def test_envelope_projects_report_status(outcomes: list[Completed]) -> None:
    """envelope(report, ...) projects status and exit_code from the folded report."""
    report = fold(Claim.STATIC, "check", tuple(outcomes))
    env = envelope(report, claim=Claim.STATIC, verb="check")
    assert env.report is report
    assert env.error is None
    assert env.status == report.status
    assert env.exit_code == report.status.exit_code


@given(fault_st)
def test_envelope_projects_fault_status(fault: Fault) -> None:
    """envelope(fault, ...) carries the Fault in error, clears report, and projects exit_code."""
    env = envelope(fault, claim=Claim.CODE, verb="check")
    assert env.error is fault
    assert env.report is None
    assert env.status is fault.status
    assert env.exit_code == fault.status.exit_code


def test_envelope_decodes_pre_trace_history_artifact() -> None:
    """A pre-trace Envelope decodes with ``trace_id``/``span_id`` defaulted and survives deterministic encode/decode identity."""
    decoded = msgspec.json.decode(_PRE_TRACE_ENVELOPE, type=Envelope)
    ctx = decoded.error_context
    assert ctx is not None, "pre-trace artifact lost its Diagnostic through the new decoder"
    assert (ctx.trace_id, ctx.span_id) == ("", ""), f"additive fields not defaulted: {ctx.trace_id!r}/{ctx.span_id!r}"
    assert (ctx.failing_step, ctx.hint) == ("parse", "parse: x after 0.0ms")
    assert msgspec.json.decode(WIRE_ENCODER.encode(decoded), type=Envelope) == decoded, "pre-trace Envelope does not round-trip"


# --- [RECEIPT]


@pytest.mark.mutation
@pytest.mark.parametrize(
    "rc, explicit, expected",
    [(0, None, RailStatus.EMPTY), (1, None, RailStatus.FAILED), (5, None, RailStatus.BUSY), (0, RailStatus.OK, RailStatus.OK)],
)
def test_receipt_status_derivation(rc: int, explicit: RailStatus | None, expected: RailStatus) -> None:
    """Receipt derives status from the return code unless an explicit override is supplied."""
    done = receipt(("ruff",), rc) if explicit is None else receipt(("ruff",), rc, status=explicit)
    assert done.argv == ("ruff",)
    assert done.returncode == rc
    assert done.status is expected


# --- [FIELD_CAP]


@pytest.mark.mutation
@pytest.mark.parametrize(
    "subject, name, default, expected",
    [
        (Fault, "message", 0, 1024),
        (Diagnostic, "hint", 0, 256),
        (Match, "text", 0, 4096),
        (VerifySummary, "first_fault_output", 0, 256),
        (Fault, "argv", 42, 42),
        (Counts, "absent", 7, 7),
    ],
)
def test_field_cap_introspection(subject: type[msgspec.Struct], name: str, default: int, expected: int) -> None:
    """field_cap reads msgspec string caps and falls back for non-string or absent fields."""
    assert field_cap(subject, name, default=default) == expected


# --- [WIRE_CODEC]


@spec(Diagnostic, mutation=True, law="determinism")
def test_wire_encode_is_deterministic(detail: Diagnostic) -> None:
    """wire_encode agrees with the deterministic conftest encoder for any wire value."""
    metamorphic(detail, wire_encode, WIRE_ENCODER.encode)


def test_validate_detail_none_passthrough() -> None:
    """validate_detail(None) returns None — the optional slot survives the codec."""
    assert validate_detail(None) is None


@given(st.text(alphabet=st.characters(min_codepoint=0xDC80, max_codepoint=0xDCFF), min_size=1, max_size=8))
def test_wire_safe_neutralizes_surrogates(surrogates: str) -> None:
    """wire_safe makes any lone-surrogate string UTF-8 encodable, where the raw string would raise."""
    with pytest.raises(UnicodeEncodeError):
        surrogates.encode("utf-8")
    wire_safe(surrogates).encode("utf-8")  # would raise if a surrogate survived


@example(text="")
@example(text="\U0010ffff")  # max valid codepoint, multi-byte
@example(text="a" * 64)  # max-size boundary
@given(st.text(max_size=64))
def test_wire_safe_idempotent_on_clean(text: str) -> None:
    """wire_safe is idempotent: a UTF-8-clean string passes through unchanged."""
    idempotent(wire_safe(text), wire_safe)


# --- [PARAMS]


@given(st.text(max_size=64), st.lists(st.text(max_size=32), min_size=0, max_size=2000))
def test_baseparams_surplus_within_fault_cap(verb: str, tokens: list[str]) -> None:
    """BaseParams.surplus is total and always clips the Fault message to the 1024-byte wire cap."""
    fault = BaseParams.surplus(verb, tuple(tokens))
    assert isinstance(fault, Fault)
    assert fault.status is RailStatus.FAULTED
    target(float(len(fault.message.encode())), label="surplus_message_bytes")
    assert len(fault.message.encode()) <= field_cap(Fault, "message", default=0)


@given(st.lists(st.text(max_size=32), min_size=0, max_size=8), st.text(max_size=16))
def test_baseparams_default_arity_is_identity(paths: list[str], verb: str) -> None:
    """The default _arity is unbounded, so bound returns the params unchanged (never a Fault)."""
    params = BaseParams(paths=tuple(paths))
    bound = params.bound(verb)
    assert bound is params


# --- [TOOL_ARGS_SPLICE]


def test_tool_args_fill_embedded_substitution_and_empty_drop() -> None:
    """An embedded ``{name}`` hole substitutes the string field in place; an empty value drops the whole token."""
    args = ToolArgs(max_cpu="4", sarif_dir="")
    assert args.fill(("build", "-maxCpuCount:{max_cpu}", "-p:CspSarifDir={sarif_dir}")) == ("build", "-maxCpuCount:4")


def test_tool_args_fill_tuple_splice_and_passthrough() -> None:
    """A ``{name*}`` hole splices the tuple field's tokens whole; hole-free tokens pass through verbatim."""
    args = ToolArgs(filter=("--filter", "Cat=Fast"), argv=())
    assert args.fill(("test", "{filter*}", "--")) == ("test", "--filter", "Cat=Fast", "--")
    assert args.fill(("test", "{argv*}")) == ("test",)


@pytest.mark.parametrize(
    "template", [("{targets}",), ("prefix-{filter}",), ("{max_cpu*}",)], ids=["tuple-embedded", "tuple-in-token", "string-under-star"]
)
def test_tool_args_fill_kind_mismatch_faults(template: tuple[str, ...]) -> None:
    """A hole naming a field of the wrong kind (tuple embedded, or string under ``*``) raises ValueError."""
    with pytest.raises(ValueError, match="hole"):
        _ = ToolArgs(max_cpu="4", filter=("x",), targets=("t",)).fill(template)


@given(st.lists(st.text(alphabet=st.characters(exclude_characters="{}"), min_size=1), max_size=6))
def test_tool_args_fill_identity_on_hole_free_commands(tokens: list[str]) -> None:
    """``fill`` is the identity on commands carrying no holes."""
    assert ToolArgs().fill(tuple(tokens)) == tuple(tokens)


# --- [ENUM_PAYLOADS]


def test_host_bound_claims_partition() -> None:
    """HOST_BOUND_CLAIMS pins exactly the claims that cannot run off-host; all are real Claim members."""
    assert frozenset((Claim.BRIDGE, Claim.PACKAGE, Claim.PROVISION, Claim.CONTRACTS, Claim.INIT)) == HOST_BOUND_CLAIMS
    assert frozenset(Claim) > HOST_BOUND_CLAIMS


def test_enum_payloads_pin_launch_and_routing_data() -> None:
    """Runner prefixes partition by kind; Language suffixes are dot-prefixed with a closed strategy discriminant."""
    assert {r for r in Runner if r.prefix == ()} == {Runner.DIRECT, Runner.INPROC}
    assert all(lang.suffixes and lang.strategy in {"closure", "glob"} for lang in Language)
    assert all(s.startswith(".") for lang in Language for s in lang.suffixes)


def test_toolgroup_uv_flag_splits_dependency_groups_from_policy_tags() -> None:
    """Exactly the uv-flagged ToolGroup members name uv dependency groups; Tool.uv_groups keeps that subset, dropping policy tags."""
    uv_members = frozenset(g for g in ToolGroup if g.uv)
    assert uv_members == {ToolGroup.MUTATION}, "uv-dependency-group membership drifted from the MUTATION group"
    tool = msgspec.structs.replace(
        Tool("g", Runner.UV, ("ruff",), Input.NONE, Language.PYTHON, Claim.STATIC),
        groups=(ToolGroup.MUTATION, ToolGroup.RUN_DEFAULT, ToolGroup.REQUIRES_COVERAGE),
    )
    assert tool.uv_groups() == (ToolGroup.MUTATION,), "uv_groups must inject only genuine uv dependency groups for --group"
    assert msgspec.structs.replace(tool, groups=(ToolGroup.RUN_DEFAULT,)).uv_groups() == (), "a tag-only row injects no uv group"


@pytest.mark.parametrize(
    "status, results, expected",
    [
        (SarifStatus.PRODUCED, 0, "produced:0"),
        (SarifStatus.PRODUCED, 7, "produced:7"),
        (SarifStatus.INCREMENTAL, 7, "absent:incremental"),
        (SarifStatus.NO_BUILD, 7, "absent:no-build"),
        (SarifStatus.BUILD_FAILED, 7, "absent:build-failed"),
    ],
    ids=["produced_zero", "produced_qualified", "incremental", "no_build", "build_failed"],
)
def test_sarif_status_token_qualifies_produced_only(status: SarifStatus, results: int, expected: str) -> None:
    """SarifStatus.token qualifies a produced SARIF with its result count; every absent reason renders its bare class token."""
    assert status.token(results) == expected


@pytest.mark.parametrize(
    "flags, expected",
    [({}, None), ({"csharp": True}, Language.CSHARP), ({"python": True}, Language.PYTHON), ({"typescript": True}, Language.TYPESCRIPT)],
    ids=["unrestricted", "csharp", "python", "typescript"],
)
def test_language_choice_projects_single_flag(flags: dict[str, bool], expected: Language | None) -> None:
    """language_choice maps no flag to None (unrestricted) and one flag to that language."""
    assert language_choice("check", **flags) == expected


def test_language_choice_conflicting_flags_fault() -> None:
    """Two or more language flags fault with a parse-step message naming every conflicting flag."""
    fault = language_choice("check", csharp=True, python=True)
    assert isinstance(fault, Fault)
    assert fault.status is RailStatus.FAULTED
    assert "--csharp" in fault.message
    assert "--python" in fault.message


# --- [BIND]
# Bind carries callable/type objects, so registry shape is the law instead of wire round-trip.


@given(binds_st)
def test_bind_is_well_formed(bind: Bind) -> None:
    """Each registry Bind carries a callable handler, a Claim claim, a str verb, and a type params."""
    assert callable(bind.handler)
    assert isinstance(bind.claim, Claim)
    assert isinstance(bind.verb, str)
    assert bind.verb
    assert isinstance(bind.params, type)
    assert isinstance(bind.help, str)
