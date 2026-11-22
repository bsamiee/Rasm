"""Flamenco Manager a render farm client reaches, hosted on a loopback port and answering the three reads of the v3 OpenAPI document a submission makes."""

from collections.abc import Callable, Mapping, Sequence
import platform

import msgspec
import pytest

from rasm.testing.hosting import Answer, Hosted, Received

# --- [CONSTANTS] ------------------------------------------------------------------------

READS = {
    f"/api/v3/configuration/shared-storage/users/{platform.system().lower()}": {"location": "/render/jobs", "audience": "users", "platform": platform.system().lower(), "shamanEnabled": False},
    "/api/v3/jobs/types": {"job_types": [{"name": "simple-blender-render", "label": "Simple", "etag": "1a"}, {"name": "rasm-render", "label": "Rasm", "etag": "9f2b1c"}]},
    "/api/v3/worker-mgt/tags": {"tags": [{"id": "4c1e", "name": "GPU-EEVEE"}]},
}

# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def managing(hosted: Hosted) -> Callable[[Mapping[str, object]], tuple[str, Sequence[Received]]]:
    """Returns the start of a Flamenco Manager answering the shared storage of this platform's users, its job types, and its worker tags, each path's body replaceable, and every other path 404."""
    return lambda replaced: hosted(
        lambda received: Answer(headers={"content-type": "application/json"}, body=msgspec.json.encode(body)) if (body := {**READS, **replaced}.get(received.path)) is not None else Answer(status=404)
    )


@pytest.fixture
def manager(managing: Callable[[Mapping[str, object]], tuple[str, Sequence[Received]]]) -> tuple[str, Sequence[Received]]:
    """Returns the base URL of a stock Flamenco Manager holding the `rasm-render` job type and one worker tag, and the requests it received."""
    return managing({})
