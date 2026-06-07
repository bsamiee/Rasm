"""Aspect layer laws."""

from collections import deque
from pathlib import Path
import sys
from typing import Protocol

import anyio
from expression import Ok, Result  # noqa: TC002
from pydantic import ValidationError
import pytest

import tools.assay as assay_pkg
from tools.assay.composition.registry import _checked_report, _guard  # noqa: PLC2701
from tools.assay.composition.settings import ArtifactScope, AssaySettings  # noqa: TC001  # beartype boundary test needs runtime annotations
from tools.assay.core.aspect import _RING, compose, Inversion, logged, ring_processor, Slot  # noqa: PLC2701
from tools.assay.core.model import Claim, Fault, fold, Report  # noqa: TC001
from tools.assay.rails.static import StaticParams


class _ProcessResult(Protocol):
    returncode: int
    stdout: bytes


def test_ring_processor_records_level_event_and_passes_dict() -> None:
    """Structlog events seed the recent-event ring without mutating unrelated keys."""
    ring: deque[str] = deque(maxlen=4)
    token = _RING.set(ring)
    try:
        event = {"event": "rail.finish", "level": "info", "status": "ok"}
        assert ring_processor(None, "info", event) is event
        assert tuple(ring) == ("info:rail.finish",)
    finally:
        _RING.reset(token)


def test_compose_rejects_layer_inversion() -> None:
    """Layer slots are monotonic; a lower slot after logged raises Inversion."""

    def identity() -> Result[None, Fault]:
        return Ok(None)

    with pytest.raises(TypeError, match=r"Slot\.logged") as raised:
        compose(logged(event="x", keys=dict), (Slot.checked, lambda fn: fn))(identity)

    assert isinstance(raised.value.args[0], Inversion)


def test_assay_claw_imports_package() -> None:
    """ASSAY_CLAW import-time beartype checking loads the package instead of failing on aspect annotations."""
    root = Path(__file__).resolve().parents[4]

    async def run() -> _ProcessResult:
        return await anyio.run_process([sys.executable, "-c", "import tools.assay"], cwd=root, env={"ASSAY_CLAW": "1"}, check=False)

    done = anyio.run(run)

    assert done.returncode == 0
    assert done.stdout == b""


def test_bootstrap_error_preserves_import_time_settings_fault(monkeypatch: pytest.MonkeyPatch) -> None:
    """Import-time settings validation faults are inspectable instead of discarded."""
    try:
        AssaySettings(run_id="")
    except ValidationError as exc:
        fault = exc
    else:  # pragma: no cover
        pytest.fail("expected invalid run_id to raise ValidationError")
    monkeypatch.setattr(assay_pkg, "_BOOTSTRAP_ERROR", fault)

    assert assay_pkg.bootstrap_error() is fault


def test_install_tracing_is_endpoint_gated(monkeypatch: pytest.MonkeyPatch) -> None:
    """Tracing provider installation is an explicit lifecycle call, not package-import side effect."""
    calls: list[str] = []

    def install(endpoint: str) -> None:
        calls.append(endpoint)

    monkeypatch.setattr(assay_pkg, "_install_tracing", install)

    assay_pkg.install_tracing("")
    assay_pkg.install_tracing("http://collector")

    assert calls == ["http://collector"]


def test_registry_checked_layer_faults_beartype_violations() -> None:
    """Registry checked layer turns wrong boundary shapes into validation Faults through the real guard."""

    def handler(settings: AssaySettings, scope: ArtifactScope, params: object) -> Result[Report, Fault]:
        _ = (settings, scope, params)
        return Ok(fold(Claim.STATIC, "report", ()))

    checked = _checked_report(handler)
    invoke = getattr(checked, "__call__")  # noqa: B004, B009  # intentional runtime-call boundary to prove beartype catches bad shapes
    outcome = _guard(lambda: invoke("not-settings", object(), StaticParams()))

    assert outcome.is_error()
    assert outcome.error.message.startswith("validation:")
