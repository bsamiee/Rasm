"""Manager state a Flamenco submission reads through `Manager.queried`, and its worker tag lookup."""

from collections.abc import Callable, Mapping, Sequence

from expression import Error, Ok, Some
import httpx2
import pytest

from rasm.records import Fault, Reason
from rasm.rendering import Endpoint, Manager
from rasm.testing import raised
from rasm.testing.hosting import Answer, Hosted, Received

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_a_manager_states_its_storage_rasm_job_type_and_tags(manager: tuple[str, Sequence[Received]]) -> None:
    """The read holds the users' shared storage of this platform, the `rasm-render` job type with its etag among the others, and every worker tag, each read one GET."""
    url, received = manager
    state = raised(Endpoint(url=url).opened(Manager.queried))
    assert (state.storage.location, state.job_type.map(lambda kind: kind.etag), [tag.name for tag in state.tags]) == ("/render/jobs", Some("9f2b1c"), ["GPU-EEVEE"])
    assert sorted((request.method, request.path.rsplit("/", 1)[0]) for request in received) == [
        ("GET", "/api/v3/configuration/shared-storage/users"),
        ("GET", "/api/v3/jobs"),
        ("GET", "/api/v3/worker-mgt"),
    ]


def test_a_manager_without_the_rasm_job_type_holds_none(managing: Callable[[Mapping[str, object]], tuple[str, Sequence[Received]]]) -> None:
    """A Manager listing other job types alone reads with no job type, not a fault."""
    url, _ = managing({"/api/v3/jobs/types": {"job_types": [{"name": "simple-blender-render", "etag": "1a"}]}})
    assert raised(Endpoint(url=url).opened(Manager.queried)).job_type.is_none()


def test_every_refused_read_is_reported(hosted: Hosted) -> None:
    """Reads refused together report one fault each, none hiding another."""
    url, _ = hosted(lambda _: Answer(status=404))
    match Endpoint(url=url).opened(Manager.queried):
        case Error(faults):
            assert [fault.cause.response.status_code for fault in faults if isinstance(fault.cause, httpx2.HTTPStatusError)] == [404, 404, 404]
        case other:
            pytest.fail(f"no faults: {other}")


@pytest.mark.parametrize("tag", ["GPU-EEVEE", "4c1e"])
def test_a_worker_tag_is_found_by_name_or_id(manager: tuple[str, Sequence[Received]], tag: str) -> None:
    """A tag named by its name or its id answers its id."""
    url, _ = manager
    assert raised(Endpoint(url=url).opened(Manager.queried)).worker_tag(tag) == Ok("4c1e")


def test_an_unknown_worker_tag_is_an_absent_fault(manager: tuple[str, Sequence[Received]]) -> None:
    """A tag the Manager does not hold is an `ABSENT` fault naming it."""
    url, _ = manager
    match raised(Endpoint(url=url).opened(Manager.queried)).worker_tag("CPU"):
        case Error((Fault(reason=Reason.ABSENT, member=str(member)),)):
            assert "'CPU'" in member
        case other:
            pytest.fail(f"no absent fault: {other}")
