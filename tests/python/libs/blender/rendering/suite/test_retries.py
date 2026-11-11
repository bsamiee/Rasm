"""The library's Manager reads under failures, each case from httpx2 `test_client.py`, `test_timeouts.py`, or `test_exceptions.py` at v2.16.0, opentelemetry-python `test_proto_span_exporter.py` at v1.45.2, the Flamenco Manager OpenAPI `Error` schema, or RFC 9110."""

from collections import Counter
from collections.abc import Callable, Mapping, Sequence
from datetime import timedelta
import socket

import httpx2
import pytest

from rasm_rendering import Endpoint, Manager
from rasm_testing import raised
from rasm_testing.hosting import Received

# --- [TYPES] ----------------------------------------------------------------------------

type Busy = Callable[[int, int, Mapping[str, str]], tuple[str, Sequence[Received]]]
type Queried = Callable[[Endpoint], Manager]

# --- [CONSTANTS] ------------------------------------------------------------------------

ALWAYS = 10_000

# --- [OPERATIONS] -----------------------------------------------------------------------


def test_a_manager_answers_its_reads(manager: tuple[str, Sequence[Received]], queried: Queried) -> None:
    """OpenAPI read operations: every read of a stock Manager is a GET the document names, each sent once and answered 200."""
    url, received = manager
    queried(Endpoint(url=url))
    assert {request.method for request in received} == {"GET"}
    assert set(Counter(request.path for request in received).values()) == {1}


@pytest.mark.parametrize("status", [400, 404, 500, 505])
def test_a_failure_status_raises_its_status_error(busy: Busy, queried: Queried, status: int) -> None:
    """`test_raise_for_status`, OpenAPI `Error`: a 4xx or 5xx answer raises `HTTPStatusError` holding the response, its request path, and the Manager's error body."""
    url, received = busy(ALWAYS, status, {})
    with pytest.raises(httpx2.HTTPStatusError) as refused:
        queried(Endpoint(url=url))
    assert refused.value.response.status_code == status
    assert any(request.path == refused.value.request.url.path for request in received)
    assert refused.value.response.json() == {"code": status, "message": "refused"}


@pytest.mark.parametrize("status", [400, 401, 403, 404, 412])
def test_a_client_error_is_sent_once(busy: Busy, queried: Queried, status: int) -> None:
    """`test_export_non_retryable_status_codes`: a request the Manager refuses as the client's error is not sent again."""
    url, received = busy(ALWAYS, status, {})
    with pytest.raises(httpx2.HTTPStatusError):
        queried(Endpoint(url=url))
    assert set(Counter(request.path for request in received).values()) == {1}


def test_a_busy_manager_is_asked_again_after_its_retry_after(busy: Busy, queried: Queried) -> None:
    """`test_export_retry_after_header`, OpenAPI `Error` code 503, RFC 9110 "Retry-After": one 503 of a busy database is followed by the same request no sooner than its `Retry-After` second, and the read succeeds."""
    url, received = busy(1, 503, {"retry-after": "1"})
    queried(Endpoint(url=url))
    first, second, *_ = received
    assert (first.method, first.path) == (second.method, second.path)
    assert second.at - first.at >= timedelta(seconds=1)


def test_retries_of_a_manager_that_stays_busy_end(busy: Busy, queried: Queried) -> None:
    """`test_export_max_retries`: a Manager answering 503 without end is asked six times for each read, and the read raises its 503."""
    url, received = busy(ALWAYS, 503, {"retry-after": "0"})
    with pytest.raises(httpx2.HTTPStatusError) as refused:
        queried(Endpoint(url=url))
    assert refused.value.response.status_code == httpx2.codes.SERVICE_UNAVAILABLE
    assert set(Counter(request.path for request in received).values()) == {6}


def test_a_retry_past_the_deadline_is_not_made(busy: Busy, queried: Queried) -> None:
    """`test_export_retry_after_header` under a deadline: a 503 whose `Retry-After` ends past the deadline is not asked again, and the read raises it."""
    url, received = busy(ALWAYS, 503, {"retry-after": "5"})
    with pytest.raises(httpx2.HTTPStatusError):
        queried(Endpoint(url=url, deadline=timedelta(seconds=1)))
    assert set(Counter(request.path for request in received).values()) == {1}


def test_a_silent_manager_raises_a_read_timeout(stalled: tuple[str, Sequence[Received]], queried: Queried) -> None:
    """`test_read_timeout`, `test_httpcore_exception_mapping`: an answer held past the socket timeout raises `ReadTimeout`."""
    url, _ = stalled
    with pytest.raises(httpx2.ReadTimeout):
        queried(Endpoint(url=url, timeout=timedelta(milliseconds=10)))


@pytest.mark.enable_socket
def test_a_closed_port_raises_a_connect_error(queried: Queried) -> None:
    """`test_httpcore_exception_mapping`: a port nothing listens on raises `ConnectError`."""
    with socket.socket() as unused:
        unused.bind(("127.0.0.1", 0))
        port = unused.getsockname()[1]
    with pytest.raises(httpx2.ConnectError):
        queried(Endpoint(url=f"http://127.0.0.1:{port}"))


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def queried() -> Queried:
    """Returns the library's read of a Manager's storage location, job type, and worker tags through one client scope of a server, raising the exception a refused read holds."""
    return lambda server: raised(server.opened(Manager.queried))
