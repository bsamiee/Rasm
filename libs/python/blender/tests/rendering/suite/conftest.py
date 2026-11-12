"""Flamenco Manager a render farm client reaches, hosted on a loopback port and answering the v3 operations of its OpenAPI document, stock, stalled, or busy."""

from collections.abc import Callable, Mapping, Sequence
from datetime import timedelta
import functools
from http import HTTPStatus
import uuid

from expression import Result
from expression.extra.result import catch
import msgspec
import pytest

from rasm_testing.hosting import Answer, Hosted, Received

# --- [CONSTANTS] ------------------------------------------------------------------------

JSON = {"content-type": "application/json"}

# --- [MODELS] ---------------------------------------------------------------------------


class SubmittedJob(msgspec.Struct, frozen=True, kw_only=True):
    """`SubmittedJob` of the Flamenco OpenAPI document: its required properties, and the job type etag a Manager checks where one is sent."""

    name: str
    type: str
    priority: int
    submitter_platform: str
    type_etag: str | None = None


# --- [OPERATIONS] -----------------------------------------------------------------------


def refusing(status: int, message: str, headers: Mapping[str, str], /) -> Answer:
    """Returns a Manager's `Error` answer of one status under the given headers, its code repeated in the body beside the message."""
    return Answer(status=status, headers={**JSON, **headers}, body=msgspec.json.encode({"code": status, "message": message}))


def managing(received: Received, /) -> Answer:
    """Answers one request as a Flamenco Manager v3.9.3 does for the version, configuration, shared storage, job types, worker tag, job submission, and job check operations, and with a 404 `Error` for any other.

    Returns:
        The operation's answer, a submission or check answered 412 for an etag other than the job type's, the default `Error` for a body outside `SubmittedJob`, else the stored `Job` or the empty 204 of a check.
    """
    current = "9f2b1c"
    job = catch(msgspec.json.decode, exception=msgspec.DecodeError)(received.body, type=SubmittedJob)
    match (received.method, received.path.removeprefix("/api/v3/").split("/"), job):
        case ("GET", ["version"], _):
            return Answer(headers=JSON, body=msgspec.json.encode({"version": "3.9.3", "shortversion": "3.9.3", "name": "Flamenco", "git": "v3.9.3"}))
        case ("GET", ["configuration"], _):
            return Answer(headers=JSON, body=msgspec.json.encode({"storageLocation": "/render/jobs", "shamanEnabled": False, "isFirstRun": False}))
        case ("GET", ["configuration", "shared-storage", "workers" | "users" as audience, platform], _):
            return Answer(headers=JSON, body=msgspec.json.encode({"location": "/render/jobs", "audience": audience, "platform": platform, "shamanEnabled": False}))
        case ("GET", ["jobs", "types"], _):
            return Answer(
                headers=JSON, body=msgspec.json.encode({"job_types": [{"name": name, "label": name, "settings": [], "etag": current} for name in ("echo-sleep-test", "simple-blender-render")]})
            )
        case ("GET", ["jobs", "type", name], _):
            return Answer(headers=JSON, body=msgspec.json.encode({"name": name, "label": name, "settings": [], "etag": current}))
        case ("GET", ["worker-mgt", "tags"], _):
            return Answer(headers=JSON, body=msgspec.json.encode({"tags": [{"id": str(uuid.uuid4()), "name": "GPU-EEVEE"}]}))
        case ("POST", ["jobs"] | ["jobs", "check"], Result(tag="error", error=refusal)):
            return refusing(HTTPStatus.BAD_REQUEST, str(refusal), {})
        case ("POST", ["jobs"] | ["jobs", "check"], Result(ok=SubmittedJob(type_etag=str(etag)))) if etag != current:
            return refusing(HTTPStatus.PRECONDITION_FAILED, f"job type etag {etag} does not match {current}", {})
        case ("POST", ["jobs", "check"], _):
            return Answer(status=HTTPStatus.NO_CONTENT)
        case ("POST", ["jobs"], _):
            stored = {"id": str(uuid.uuid4()), "created": "2026-11-11T00:00:00Z", "updated": "2026-11-11T00:00:00Z", "status": "queued", "activity": "", "steps_completed": 0, "steps_total": 1}
            return Answer(headers=JSON, body=msgspec.json.encode(msgspec.json.decode(received.body, type=dict[str, object]) | stored))
        case _:
            return refusing(HTTPStatus.NOT_FOUND, f"{received.method} {received.path} is no operation", {})


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def manager(hosted: Hosted) -> tuple[str, Sequence[Received]]:
    """Returns the base URL of a Flamenco Manager the test hosts and the requests it received."""
    return hosted(managing)


@pytest.fixture
def stalled(hosted: Hosted) -> tuple[str, Sequence[Received]]:
    """Returns the base URL of a Flamenco Manager holding every answer back one second and the requests it received."""
    return hosted(lambda received: msgspec.structs.replace(managing(received), delay=timedelta(seconds=1)))


@pytest.fixture
def busy(hosted: Hosted) -> Callable[[int, int, Mapping[str, str]], tuple[str, Sequence[Received]]]:
    """Returns the start of a Flamenco Manager refusing its first requests with one `Error` status, a busy database's 503 with its `Retry-After` among them, before it answers as `manager` does."""
    return lambda count, status, headers: hosted(functools.partial(lambda first, received: next(first, None) or managing(received), iter([refusing(status, "refused", headers)] * count)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = []
