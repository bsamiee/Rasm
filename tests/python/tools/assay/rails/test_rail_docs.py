"""Laws for tools.assay.rails.docs covering three contract surfaces.

DocsParams: default and explicit strict/paths construction invariants.
FaultedPromotion: Exception subclass semantics and the strict-mode promotion
    matrix (EMPTY/SKIP raise, FAILED/OK are preserved unchanged).
check: fan_out fault propagation, claim stamping, strict flag threading, and
    first-fault short-circuit across multi-error fan_out results.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from expression import Error, Ok
from hypothesis import given, HealthCheck, settings, strategies as st
import msgspec
import pytest

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_error, assert_error_status, assert_ok, support_matrix, validity_matrix, ValidityCase
from tests.python._testkit.strategies import resolve
from tests.python.tools.assay.kit import AssayHarness  # noqa: TC001
from tools.assay.core.model import Check, Claim, Completed, Fault, fold, Language, Mode  # noqa: TC001  # Check annotates a local captured list
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus
from tools.assay.rails import docs as docs_rail
from tools.assay.rails.docs import check, DocsParams, FaultedPromotion


# --- [CONSTANTS] ------------------------------------------------------------------------

_EMPTY_REPORT = fold(Claim.DOCS, "check", ())
_FAILED_REPORT = fold(Claim.DOCS, "check", (Completed(("mmdc",), 1, status=RailStatus.FAILED),))
_OK_REPORT = fold(Claim.DOCS, "check", (Completed(("mmdc",), 0, status=RailStatus.OK),))

# --- [LAW_COVERAGE]

register_law(DocsParams, "docs_params_default_strict_is_false")
register_law(DocsParams, "docs_params_roundtrip")
register_law(DocsParams, "docs_params_strict_matrix")
register_law(DocsParams, "docs_params_paths_default_empty")

register_law(FaultedPromotion, "faulted_promotion_is_exception_subclass")
register_law(FaultedPromotion, "faulted_promotion_message_is_no_docs_changed")
register_law(FaultedPromotion, "faulted_promotion_strict_raises_on_empty")
register_law(FaultedPromotion, "faulted_promotion_strict_raises_on_skip")
register_law(FaultedPromotion, "faulted_promotion_strict_false_does_not_raise_on_empty")
register_law(FaultedPromotion, "faulted_promotion_preserves_failed_report")
register_law(FaultedPromotion, "faulted_promotion_promotion_matrix")

register_law(check, "check_fan_out_fault_propagates_on_error_rail")
register_law(check, "check_no_files_yields_ok_rail")
register_law(check, "check_strict_empty_becomes_error")
register_law(check, "check_ok_receipt_yields_ok_report")
register_law(check, "check_failed_receipt_yields_failed_report")
register_law(check, "check_claim_is_docs")
register_law(check, "check_strict_flag_propagated")
register_law(check, "check_multi_fault_first_fault_wins")
register_law(check, "check_verb_is_check")
register_law(check, "check_mode_threaded_selects_mmdc")
register_law(check, "check_routing_settings_threaded_to_root")
register_law(check, "check_fan_out_receives_real_dependencies")

register_law(docs_rail._outcomes, "outcomes_per_file_argv_shape")
register_law(docs_rail._outcomes, "outcomes_verb_stamped_into_report")
register_law(docs_rail._outcomes, "outcomes_fan_out_receives_real_dependencies")
register_law(docs_rail._outcomes, "outcomes_folds_result_rows_and_artifacts")
register_law(docs_rail._outcomes, "outcomes_empty_fold_promotes_to_ok")

# --- [LAWS_DOCSPARAMS]


def test_docs_params_default_strict_is_false() -> None:
    """DocsParams.strict defaults to False — enabling opt-in only."""
    assert DocsParams().strict is False


@given(resolve(DocsParams))
@settings(max_examples=50)
def test_docs_params_roundtrip(p: DocsParams) -> None:
    """DocsParams instances survive a structural copy cycle unchanged (immutability invariant)."""
    raw = msgspec.json.encode({"paths": list(p.paths), "strict": p.strict})
    decoded = msgspec.json.decode(raw, type=dict)
    assert decoded["strict"] == p.strict


def test_docs_params_strict_matrix() -> None:
    """DocsParams.strict distinguishes opt-in and default modes across explicit construction cases."""
    validity_matrix(
        [
            ValidityCase(label="default_not_strict", value=DocsParams(), expected=False),
            ValidityCase(label="explicit_false", value=DocsParams(strict=False), expected=False),
            ValidityCase(label="explicit_true", value=DocsParams(strict=True), expected=True),
        ],
        lambda p: p.strict,
    )


def test_docs_params_paths_default_empty() -> None:
    """DocsParams.paths defaults to empty tuple (no files → SKIP/EMPTY, not fault)."""
    support_matrix(
        ("default_paths_empty", lambda: DocsParams().paths == (), True),
        ("explicit_paths_retained", lambda: DocsParams(paths=("a.md",)).paths == ("a.md",), True),
    )


# --- [LAWS_FAULTED_PROMOTION]


def test_faulted_promotion_is_exception_subclass() -> None:
    """FaultedPromotion is an Exception subclass (not BaseException) so the registry can catch it."""
    assert issubclass(FaultedPromotion, Exception)


def test_faulted_promotion_message_is_no_docs_changed() -> None:
    """FaultedPromotion carries the canonical 'no docs changed' sentinel message."""
    assert str(FaultedPromotion()) == "no docs changed"


def test_faulted_promotion_strict_raises_on_empty() -> None:
    """_strict raises FaultedPromotion for EMPTY status under strict=True — the promotion invariant."""
    with pytest.raises(FaultedPromotion):
        docs_rail._strict(_EMPTY_REPORT, strict=True)


def test_faulted_promotion_strict_raises_on_skip() -> None:
    """_strict raises FaultedPromotion for SKIP status under strict=True — symmetry with EMPTY arm."""
    skip_report = fold(Claim.DOCS, "check", (Completed(("mmdc",), 0, status=RailStatus.SKIP),))
    with pytest.raises(FaultedPromotion):
        docs_rail._strict(skip_report, strict=True)


def test_faulted_promotion_strict_false_does_not_raise_on_empty() -> None:
    """_strict with strict=False never raises FaultedPromotion — opt-in semantics are exclusive."""
    result = docs_rail._strict(_EMPTY_REPORT, strict=False)
    assert result.status is RailStatus.EMPTY


def test_faulted_promotion_preserves_failed_report() -> None:
    """_strict does NOT rewrite real FAILED reports even under strict=True — real defects stay failed."""
    result = docs_rail._strict(_FAILED_REPORT, strict=True)
    assert result.status is RailStatus.FAILED


def test_faulted_promotion_promotion_matrix() -> None:
    """_strict promotion matrix: EMPTY+strict→raise, SKIP+strict→raise, FAILED+strict→preserve, OK+strict→preserve."""
    skip_report = fold(Claim.DOCS, "check", (Completed(("mmdc",), 0, status=RailStatus.SKIP),))

    for label, report, strict, should_raise in [
        ("empty_strict_raises", _EMPTY_REPORT, True, True),
        ("skip_strict_raises", skip_report, True, True),
        ("empty_not_strict", _EMPTY_REPORT, False, False),
        ("failed_strict_preserves", _FAILED_REPORT, True, False),
        ("ok_strict_preserves", _OK_REPORT, True, False),
    ]:
        if should_raise:
            with pytest.raises(FaultedPromotion, match="no docs changed"):
                docs_rail._strict(report, strict=strict)
        else:
            docs_rail._strict(report, strict=strict)  # must not raise
        _ = label  # suppress F841 unused-loop-variable


# --- [LAWS_CHECK]


def test_check_fan_out_fault_propagates_on_error_rail(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check propagates spawn faults from fan_out onto the Error rail without swallowing."""
    fault = Fault(("mmdc",), RailStatus.UNSUPPORTED, "mmdc not found")
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Error(fault),))

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("README.md",)))
    assert_error_status(result, RailStatus.UNSUPPORTED)


def test_check_no_files_yields_ok_rail(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check with no routed files returns Ok(Report) with EMPTY/SKIP status — not a hard fault."""
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: ())

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("docs/guide.md",)))
    report = assert_ok(result)
    assert report.claim is Claim.DOCS


def test_check_strict_empty_becomes_error(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check with strict=True and no outputs raises FaultedPromotion, which propagates as an exception."""
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: ())

    with pytest.raises(FaultedPromotion):
        check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("docs/guide.md",), strict=True))


def test_check_ok_receipt_yields_ok_report(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check folds a single OK receipt into Ok(Report) with status OK — the happy-path rail."""
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Ok(Completed(("mmdc",), 0, status=RailStatus.OK)),))

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("README.md",)))
    report = assert_ok(result)
    assert report.status is RailStatus.OK


def test_check_failed_receipt_yields_failed_report(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check folds a FAILED receipt into Ok(Report) with status FAILED — real defects stay on the Ok rail."""
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Ok(Completed(("mmdc",), 1, status=RailStatus.FAILED)),))

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("README.md",)))
    report = assert_ok(result)
    assert report.status is RailStatus.FAILED


def test_check_claim_is_docs(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check always produces a Report stamped with Claim.DOCS — not silently reusing another claim."""
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: ())

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("docs/guide.md",)))
    report = assert_ok(result)
    assert report.claim is Claim.DOCS


@given(st.booleans())
@settings(max_examples=20, suppress_health_check=[HealthCheck.function_scoped_fixture])
def test_check_strict_flag_propagated(
    assay_root: AssayHarness,
    monkeypatch: pytest.MonkeyPatch,
    strict: bool,  # noqa: FBT001 — bool param is the exact signal under test, not a flag smell
) -> None:
    """check(strict=strict) with no outputs raises iff strict is True — strict flag is load-bearing."""
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: ())

    params = DocsParams(paths=("docs/guide.md",), strict=strict)
    if strict:
        with pytest.raises(FaultedPromotion):
            check(assay_root.settings, assay_root.scope(Claim.DOCS), params)
    else:
        assert_ok(check(assay_root.settings, assay_root.scope(Claim.DOCS), params))


def test_check_multi_fault_first_fault_wins(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """fan_out returning multiple Error slots causes check to short-circuit on the first fault."""
    f1 = Fault(("mmdc",), RailStatus.FAULTED, "first fault")
    f2 = Fault(("mmdc",), RailStatus.UNSUPPORTED, "second fault")
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Error(f1), Error(f2)))

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("a.md",)))
    err = assert_error(result)
    assert err is f1


def test_check_verb_is_check(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check stamps the literal verb "check" into Report.verb across _thin_rail and the fold.

    Kills the `verb=None`, `verb="XXcheckXX"`, and `verb="CHECK"` mutations at the check boundary
    (and the threaded `_outcomes(verb=None)`): each changes the exact Report.verb token, which the
    happy-path fold here surfaces and this asserts to be the byte-exact "check".
    """
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Ok(Completed(("mmdc",), 0, status=RailStatus.OK)),))

    result = check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("README.md",)))
    report = assert_ok(result)
    assert report.verb == "check", "verb literal is lowercase 'check', not None, 'CHECK', or a marker string"


def test_check_mode_threaded_selects_mmdc(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check threads Mode.CHECK so the `t.mode is mode` filter admits the mmdc tool for a routed file.

    Kills `mode=None` (at check and at the threaded `_outcomes(mode=None)`): with the real mode the
    select-filter yields exactly one mmdc Check for one routed file; with mode=None the filter
    rejects every tool, fan_out sees zero checks, and the captured count collapses to zero.
    """
    captured: list[Check] = []
    monkeypatch.setattr(docs_rail, "fan_out", lambda checks, **_k: captured.extend(checks) or (Ok(Completed(("mmdc",), 0, status=RailStatus.OK)),))
    src = assay_root.write("docs/diagram.md", "# d")
    assert src.exists()

    check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("docs/diagram.md",)))
    assert len(captured) == 1, "Mode.CHECK admits the single mmdc tool; mode=None would admit none"
    assert captured[0].tool.command[0] == "mmdc"


def test_check_routing_settings_threaded_to_root(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check threads settings into route so enumeration resolves paths under settings.root, not cwd.

    Kills `route(..., settings=None)` / dropped-settings: the harness writes a real .md under its
    isolated root, so correct settings enumerate one file into one Check; settings=None roots the
    LocalSource at the process cwd where the file is absent, routing zero files and zero checks.
    """
    captured: list[Check] = []
    monkeypatch.setattr(docs_rail, "fan_out", lambda checks, **_k: captured.extend(checks) or (Ok(Completed(("mmdc",), 0, status=RailStatus.OK)),))
    assay_root.write("docs/only-here.md", "# here")

    check(assay_root.settings, assay_root.scope(Claim.DOCS), DocsParams(paths=("docs/only-here.md",)))
    assert len(captured) == 1, "settings.root routing finds the harness-local file; cwd routing finds nothing"
    cmd = captured[0].tool.command
    assert cmd[cmd.index("-i") + 1] == "docs/only-here.md", "the routed file is the -i input of the single Check"


def test_check_fan_out_receives_real_dependencies(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Check threads the live settings and scope down to fan_out through _thin_rail and _outcomes.

    Kills `_thin_rail(None, scope, ...)` and the threaded `_outcomes(settings=None)`: the captured
    fan_out kwargs must carry the exact harness settings and scope; a nulled settings argument would
    surface as `seen["settings"] is None`.
    """
    seen: dict[str, object] = {}
    receipt = (Ok(Completed(("mmdc",), 0, status=RailStatus.OK)),)
    monkeypatch.setattr(docs_rail, "fan_out", lambda checks, **kw: (seen.update(checks=checks, **kw), receipt)[1])
    assay_root.write("docs/dep.md", "# dep")
    scope = assay_root.scope(Claim.DOCS)

    check(assay_root.settings, scope, DocsParams(paths=("docs/dep.md",)))
    assert seen["settings"] is assay_root.settings, "fan_out receives the settings threaded through check, not None"
    assert seen["scope"] is scope, "fan_out receives the scope threaded through check, not None"


# --- [LAWS_OUTCOMES]


def test_outcomes_per_file_argv_shape(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_outcomes builds one mmdc Check per file with -i/-a/-o and a collision-free slugged sink stem.

    The argv must end in the -o markdown sink (Input.OWNED contributes only an empty tail, so the raw
    input file never re-appends as a bare positional). The -a artefacts path and -o sink both ride the
    per-run scope path; the sink stem is the slugged full relative path (parts joined by `__`), so two
    same-basename files never share a sink, and -o ends in <slug>.md.
    """
    captured: list[Check] = []
    monkeypatch.setattr(docs_rail, "fan_out", lambda checks, **_k: captured.extend(checks) or ())

    scope = assay_root.scope(Claim.DOCS)
    routed = Routed(language=Language.DOCS, scope=Scope.CHANGED, files=("docs/a.md", "tools/assay/README.md"))
    docs_rail._outcomes(routed, settings=assay_root.settings, scope=scope, claim=Claim.DOCS, verb="check", mode=Mode.CHECK)

    assert len(captured) == 2, "one Check per routed file"
    for chk, src, stem in zip(captured, routed.files, ("docs__a", "tools__assay__README"), strict=True):
        cmd = chk.tool.command
        assert cmd[0] == "mmdc"
        assert cmd[-6:] == ("-i", src, "-a", scope.path, "-o", f"{scope.path}/{stem}.md")
        assert cmd[-1].endswith(".md"), "argv terminates at the -o markdown sink, not a bare input positional"
    assert docs_rail._sink_stem("a/README.md") != docs_rail._sink_stem("b/README.md"), "same-basename files slug to distinct sinks"


def test_outcomes_folds_result_rows_and_artifacts(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_outcomes folds one source:<file>:1 result row per routed file and Artifact rows for produced sink files.

    A worked run must surface, in the envelope itself: (a) a per-file result row whose id is the
    `source:<rel>:1` form and whose severity tracks the mmdc receipt's exit, and (b) Artifact rows for the
    SVG/MD the run produced under the scope (so an agent reads paths from the envelope, never the scope shape).
    """
    assay_root.write("docs/diagram.md", "# d")
    scope = assay_root.scope(Claim.DOCS)
    scope.ensure()
    stem = docs_rail._sink_stem("docs/diagram.md")
    scope.store.write_bytes(b"<svg/>", *scope.path.removeprefix(f"{scope.store.root}/").split("/"), f"{stem}-1.svg")
    scope.store.write_bytes(b"# out", *scope.path.removeprefix(f"{scope.store.root}/").split("/"), f"{stem}.md")
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Ok(Completed(("mmdc",), 0, status=RailStatus.OK)),))

    routed = Routed(language=Language.DOCS, scope=Scope.CHANGED, files=("docs/diagram.md",))
    report = assert_ok(docs_rail._outcomes(routed, settings=assay_root.settings, scope=scope, claim=Claim.DOCS, verb="check", mode=Mode.CHECK))

    assert [m.id for m in report.results] == ["source:docs/diagram.md:1"], "one source:<file>:1 row per routed file"
    assert report.results[0].severity is None, "an exit-0 mmdc receipt folds to a non-failed row"
    artifact_names = {a.path.rsplit("/", 1)[-1] for a in report.artifacts}
    assert {f"{stem}-1.svg", f"{stem}.md"} <= artifact_names, "produced SVG + MD ride the envelope as Artifact rows"


def test_outcomes_empty_fold_promotes_to_ok(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_outcomes promotes an EMPTY base fold to OK once result rows fold in, but FAILED receipts stay FAILED.

    The mmdc receipt is rc 0 → EMPTY base status; the worked run carries result rows, so the report status
    promotes to OK (an agent must see a green run, not an ambiguous EMPTY). A FAILED receipt keeps FAILED and
    stamps the row severity, so a real render defect is never masked by the promotion.
    """
    assay_root.write("docs/ok.md", "# ok")
    scope = assay_root.scope(Claim.DOCS)
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Ok(Completed(("mmdc",), 0, status=RailStatus.OK)),))
    ok_routed = Routed(language=Language.DOCS, scope=Scope.CHANGED, files=("docs/ok.md",))
    ok_report = assert_ok(docs_rail._outcomes(ok_routed, settings=assay_root.settings, scope=scope, claim=Claim.DOCS, verb="check", mode=Mode.CHECK))
    assert ok_report.status is RailStatus.OK, "EMPTY base + result rows promotes to OK"

    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Ok(Completed(("mmdc",), 1, status=RailStatus.FAILED)),))
    bad_report = assert_ok(docs_rail._outcomes(ok_routed, settings=assay_root.settings, scope=scope, claim=Claim.DOCS, verb="check", mode=Mode.CHECK))
    assert bad_report.status is RailStatus.FAILED, "a FAILED receipt is never promoted away"
    assert bad_report.results[0].severity == "failed", "the FAILED receipt stamps the row severity"


def test_outcomes_verb_stamped_into_report(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_outcomes stamps the `verb` argument verbatim into Report.verb via the fold.

    Kills `fold(claim, None, ...)`: dropping/nulling the verb argument changes the observable
    Report.verb from the passed token to None (or to another literal), which this asserts exactly.
    """
    monkeypatch.setattr(docs_rail, "fan_out", lambda *_a, **_k: (Ok(Completed(("mmdc",), 0, status=RailStatus.OK)),))

    routed = Routed(language=Language.DOCS, scope=Scope.CHANGED, files=("docs/a.md",))
    result = docs_rail._outcomes(
        routed, settings=assay_root.settings, scope=assay_root.scope(Claim.DOCS), claim=Claim.DOCS, verb="audit", mode=Mode.CHECK
    )
    report = assert_ok(result)
    assert report.verb == "audit", "Report.verb is the verb argument, not a hardcoded or nulled value"


def test_outcomes_fan_out_receives_real_dependencies(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_outcomes forwards the live settings, scope, and routed objects into fan_out unchanged.

    Kills the `fan_out(settings=None | scope=None | routed=None | dropped)` family: each nulled or
    dropped keyword changes the kwargs fan_out actually receives, which this captures and asserts
    by identity against the real dependency objects.
    """
    seen: dict[str, object] = {}
    monkeypatch.setattr(docs_rail, "fan_out", lambda checks, **kw: (seen.update(checks=checks, **kw), ())[1])

    scope = assay_root.scope(Claim.DOCS)
    routed = Routed(language=Language.DOCS, scope=Scope.CHANGED, files=("docs/a.md",))
    docs_rail._outcomes(routed, settings=assay_root.settings, scope=scope, claim=Claim.DOCS, verb="check", mode=Mode.CHECK)

    assert seen["settings"] is assay_root.settings, "fan_out receives the live settings, not None"
    assert seen["scope"] is scope, "fan_out receives the live scope, not None"
    assert seen["routed"] is routed, "fan_out receives the live routed projection, not None"
    assert set(seen) == {"checks", "settings", "scope", "routed"}, "no fan_out keyword is dropped"
