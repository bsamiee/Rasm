"""Smoke laws proving each assay fixture works end to end.

Tight tests assert isolation produces an Envelope under tmp, the socket-free mock host yields a canned
status, bridge result JSON decodes defensively, and package-shape fixtures materialize valid metadata.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

from pathlib import Path
from typing import TYPE_CHECKING

import pytest  # noqa: TC002 — pytest.MonkeyPatch/CaptureFixture used in runtime annotations (no PEP 563)

from tests.tools.assay.conftest import RailProbe, read_one_envelope
from tools.assay.composition import registry  # private package surface under test
from tools.assay.core.model import Bind, Claim, Envelope  # private package surface under test
from tools.assay.core.status import RailStatus  # private package surface under test
from tools.assay.rails import bridge as bridge_rail, package as package_rail  # private package surface under test


if TYPE_CHECKING:
    from expression import Result

    from tests.tools.assay.conftest import AssayHarness, BridgeResult, YakShape
    from tools.assay.core.model import Fault, Report


# --- [OPERATIONS] ----------------------------------------------------------------------


def test_isolation_produces_envelope_under_tmp(assay_root: AssayHarness) -> None:
    """A bare ``package list`` rail folds an OK Envelope rooted entirely under ``<tmp>/.artifacts/assay``."""
    scope = assay_root.scope(Claim.PACKAGE)
    outcome: Result[Report, Fault] = package_rail.list(assay_root.settings, scope, package_rail.PackageParams())
    assert outcome.is_ok()
    env = assay_root.envelope_of(outcome.ok, claim=Claim.PACKAGE, verb="list")
    assert isinstance(env, Envelope)
    assert env.status is RailStatus.OK
    assert Path(scope.path).is_relative_to(assay_root.root)
    assert str(assay_root.settings.store_root).startswith(str(assay_root.root))


def test_rail_probe_yields_canned_status(assay_root: AssayHarness, rail_probe: RailProbe, monkeypatch: pytest.MonkeyPatch) -> None:
    """Patching ``bridge_rail.run_check`` to a canned ``Completed(OK)`` folds an OK Report with no live host."""
    rail_probe.install(monkeypatch, bridge_rail, "run_check", RailProbe.ok())
    scope = assay_root.scope(Claim.BRIDGE)
    outcome = bridge_rail.check(assay_root.settings, scope, bridge_rail.BridgeParams())
    assert outcome.is_ok()
    assert outcome.ok.status is RailStatus.OK
    assert any(member == "run_check" for member, _args, _kwargs in rail_probe.calls)


def test_bridge_result_variants_decode(bridge_result: BridgeResult) -> None:
    """The valid ``_BridgeResult`` decodes to an OK execute phase; the adversarial variants stay readable bytes."""
    valid = bridge_result.valid()
    decoded = bridge_rail._RESULT_DECODER.decode(valid.read_bytes())
    assert decoded.status is RailStatus.OK
    assert decoded.phases[0].phase == "execute"
    assert bridge_result.malformed().read_bytes() == b"{not json"
    assert bridge_result.partial().read_bytes() == b"{}"
    assert not bridge_result.missing().exists()


def test_yak_shape_materializes_valid_meta(assay_root: AssayHarness, yak_shape: YakShape) -> None:
    """The fake-yak materializer lays an executable ``yak`` + ``.rhp`` tree and the props cover every ``_META_PROPS``."""
    meta = yak_shape.materialize(assay_root)
    assert meta.yak_path.is_file()
    assert (meta.target_dir / f"{meta.assembly_name}{meta.target_ext}").is_file()
    props = yak_shape.props(meta)
    required = tuple(name for name in package_rail._META_PROPS if name != "YakPushSource")
    assert all(props[name] for name in required)


def test_envelope_oracle_decodes_single_stdout_line(
    assay_root: AssayHarness,
    capsys: pytest.CaptureFixture[str],
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    """The real registry ``rail`` runner writes exactly one stdout Envelope; the oracle decodes it.

    CLI isolation is env-only (the runner reads a bare ``AssaySettings()``), so ``ASSAY_ROOT`` redirects
    I/O under tmp; the runner seats its own per-fire ``_WRITES`` ContextVar (reset in ``finally``), so the
    one-Envelope guard ranks this write at 0 (stdout, not the doubled-write stderr fault) without any patch.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))
    runner = registry.rail(Bind(Claim.PACKAGE, "list", package_rail.list, package_rail.PackageParams, ""))
    runner(package_rail.PackageParams())
    decoded = read_one_envelope(capsys)
    assert decoded.claim is Claim.PACKAGE
    assert decoded.verb == "list"
    assert decoded.status is RailStatus.OK
