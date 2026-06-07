"""Aspect layer laws."""

from collections import deque

from expression import Ok, Result  # noqa: TC002
import pytest

from tools.assay.core.aspect import _RING, compose, Inversion, logged, ring_processor, Slot  # noqa: PLC2701
from tools.assay.core.model import Fault  # noqa: TC001


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
