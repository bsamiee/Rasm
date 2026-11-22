"""Server addresses of `Endpoint` and the client scope they open: refused addresses and durations, the closed client, trace context, timeouts, and unreachable servers."""

from datetime import timedelta
from http import HTTPMethod
import re
import socket

from expression import Ok
import httpx2
from opentelemetry.sdk.trace import TracerProvider
import pytest

from rasm.rendering import Endpoint
from rasm.testing import raised
from rasm.testing.hosting import Answer, Hosted

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "url", [pytest.param("ftp://farm.local", id="scheme-not-http"), pytest.param("://farm.local", id="no-scheme"), pytest.param("http://", id="no-host"), pytest.param("http://[::1", id="unparsable")]
)
def test_an_address_without_an_http_scheme_and_host_is_refused(url: str) -> None:
    """A URL whose scheme is not `http` or `https`, that names no host, or does not parse raises `ValueError` naming it at construction."""
    with pytest.raises(ValueError, match=re.escape(url)):
        Endpoint(url=url)


@pytest.mark.parametrize("durations", [pytest.param({"timeout": timedelta()}, id="zero-timeout"), pytest.param({"deadline": timedelta(seconds=-1)}, id="negative-deadline")])
def test_a_duration_not_above_zero_is_refused(durations: dict[str, timedelta]) -> None:
    """A socket timeout or deadline at or below zero raises `ValueError` at construction."""
    with pytest.raises(ValueError, match="timedelta"):
        Endpoint(url="https://farm.local", **durations)


def test_the_least_positive_durations_construct() -> None:
    """A socket timeout and deadline of one microsecond, the least durations above zero, construct."""
    endpoint = Endpoint(url="https://farm.local", timeout=timedelta(microseconds=1), deadline=timedelta(microseconds=1))
    assert (endpoint.timeout, endpoint.deadline) == (timedelta(microseconds=1), timedelta(microseconds=1))


def test_the_client_closes_with_its_scope(hosted: Hosted) -> None:
    """A client used past the scope that opened it is closed and refuses a request."""
    url, received = hosted(lambda _: Answer())
    client = raised(Endpoint(url=url).opened(lambda session: Ok(session.client)))
    with pytest.raises(RuntimeError, match="closed"):
        client.get("/")
    assert received == []


def test_a_request_carries_the_current_trace(hosted: Hosted) -> None:
    """A request inside a sampled span sends a W3C `traceparent` naming the span's trace with the sampled flag set."""
    url, received = hosted(lambda _: Answer())
    with TracerProvider(shutdown_on_exit=False).get_tracer("suite").start_as_current_span("render") as span:
        raised(Endpoint(url=url).opened(lambda session: session.sent(HTTPMethod.GET, "/")))
    (request,) = received
    stated = re.fullmatch(rf"00-{span.get_span_context().trace_id:032x}-[0-9a-f]{{16}}-([0-9a-f]{{2}})", request.headers["traceparent"])
    assert stated
    assert int(stated[1], 16) & 1


def test_an_answer_held_past_the_timeout_is_a_read_timeout(hosted: Hosted) -> None:
    """An answer arriving after the endpoint's socket timeout is a fault holding `ReadTimeout`."""
    url, _ = hosted(lambda _: Answer(delay=timedelta(milliseconds=400)))
    with pytest.raises(httpx2.ReadTimeout):
        raised(Endpoint(url=url, timeout=timedelta(milliseconds=50)).opened(lambda session: session.sent(HTTPMethod.GET, "/")))


@pytest.mark.enable_socket
def test_a_port_nothing_listens_on_is_a_connect_fault() -> None:
    """A request to a closed port is a fault holding `ConnectError`, not a raise past the scope."""
    with socket.socket() as unused:
        unused.bind(("127.0.0.1", 0))
        port = unused.getsockname()[1]
    outcome = Endpoint(url=f"http://127.0.0.1:{port}").opened(lambda session: session.sent(HTTPMethod.GET, "/"))
    with pytest.raises(httpx2.ConnectError):
        raised(outcome)
