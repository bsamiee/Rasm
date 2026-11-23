"""Client scope an `Endpoint` opens: the client closed with it, trace context on each request, timeouts, and unreachable servers."""

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
    url, _ = hosted(lambda _: Answer(delay=timedelta(milliseconds=150)))
    outcome = Endpoint(url=url, timeout=timedelta(milliseconds=50)).opened(lambda session: session.sent(HTTPMethod.GET, "/"))
    with pytest.raises(httpx2.ReadTimeout):
        raised(outcome)


@pytest.mark.enable_socket
def test_a_port_nothing_listens_on_is_a_connect_fault() -> None:
    """A request to a closed port is a fault holding `ConnectError`, not a raise past the scope."""
    with socket.socket() as unused:
        unused.bind(("127.0.0.1", 0))
        port = unused.getsockname()[1]
    outcome = Endpoint(url=f"http://127.0.0.1:{port}").opened(lambda session: session.sent(HTTPMethod.GET, "/"))
    with pytest.raises(httpx2.ConnectError):
        raised(outcome)
