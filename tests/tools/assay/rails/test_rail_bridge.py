"""Bridge rail laws."""

from typing import TYPE_CHECKING

import msgspec

from tools.assay.core.model import Completed
from tools.assay.core.status import RailStatus
from tools.assay.rails.bridge import _BridgeResult, _decode_result, _ensure_dir, _first_fault  # noqa: PLC2701


if TYPE_CHECKING:
    from pathlib import Path


def test_decode_result_prefers_report_file(tmp_path: Path) -> None:
    """Bridge result JSON on disk is authoritative over process stdout/stderr."""
    result = tmp_path / "scenario.json"
    result.write_bytes(b'{"command":"check","status":"ok"}')

    decoded = _decode_result(Completed(("bridge",), 0, stdout=b'{"status":"failed"}'), result)

    assert decoded.status is RailStatus.OK


def test_first_fault_reads_phase_output() -> None:
    """First failing bridge phase preserves phase and bounded output."""
    raw = b'{"phases":[{"phase":"setup","status":"ok"},{"phase":"execute","status":"failed","outputs":[{"source":"stderr","text":"boom"}]}]}'
    decoded = msgspec.json.decode(raw, type=_BridgeResult)

    assert _first_fault(decoded) == ("execute", "boom")


def test_ensure_dir_maps_os_errors(tmp_path: Path) -> None:
    """Report directory setup succeeds through the Result channel."""
    outcome = _ensure_dir(tmp_path / "reports" / "verify")

    assert outcome.is_ok()
