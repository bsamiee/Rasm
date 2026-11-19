"""HTTP client scope against hosted servers, each case from httpx2 `test_client.py`, the Flamenco Manager OpenAPI document, or W3C Trace Context."""

from collections.abc import Callable, Mapping, Sequence
from datetime import timedelta
import re
from typing import Protocol

from expression import Ok
import httpx2
import msgspec
from opentelemetry.sdk.trace import TracerProvider
import pytest

from rasm.rendering import Endpoint
from rasm.testing import raised
from rasm.testing.hosting import Answer, Hosted, Received

# --- [TYPES] ----------------------------------------------------------------------------


class Requested(Protocol):
    """Request through a server's client scope."""

    def __call__(self, url: str, method: str, path: str, /, *, json: Mapping[str, object] | None = None) -> httpx2.Response:
        """Returns the response, raising the exception a refused exchange holds."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

TEXT = {"content-type": "text/plain"}
JOB = {
    "name": "3Д рендеринг",
    "type": "simple-blender-render",
    "priority": 50,
    "submitter_platform": "linux",
    "settings": {"frames": "1-10", "chunk_size": 3},
    "metadata": {"project": "Sprite Fright"},
}

# --- [OPERATIONS] -----------------------------------------------------------------------


def plain(received: Received, /) -> Answer:
    """Answers as the httpx2 suite's server does: request headers as JSON on `/echo_headers`, else a greeting."""
    match received.path.removeprefix("/").split("/"):
        case ["echo_headers"]:
            return Answer(headers={"content-type": "application/json"}, body=msgspec.json.encode(received.headers))
        case _:
            return Answer(headers=TEXT, body=b"Hello, world!")


def requesting(url: str, method: str, path: str, /, *, json: Mapping[str, object] | None = None) -> httpx2.Response:
    """Returns a server's response through its client scope, raising the exception a refused exchange holds."""
    return raised(Endpoint(url=url).opened(lambda session: Ok(session.client.request(method, f"{url}{path}", json=json))))


def test_a_get_returns_the_response_of_an_http_1_1_exchange(server: tuple[str, Sequence[Received]], requested: Requested) -> None:
    """`test_get`: status, text, protocol version, encoding, request URL, redirect state, and elapsed time of a plain GET."""
    url, _ = server
    response = requested(url, "GET", "/")
    assert response.status_code == httpx2.codes.OK
    assert response.text == "Hello, world!"
    assert response.http_version == "HTTP/1.1"
    assert response.encoding == "utf-8"
    assert response.request.url == f"{url}/"
    assert response.is_redirect is False
    assert response.elapsed > timedelta(0)


@pytest.mark.parametrize("url", [pytest.param("invalid://example.org", id="scheme-not-http(s)"), pytest.param("://example.org", id="no-scheme"), pytest.param("http://", id="no-host")])
def test_an_invalid_url_is_refused_before_a_client_exists(requested: Requested, url: str) -> None:
    """`test_get_invalid_url` inputs: a URL without an HTTP scheme or host is refused where the server address is stated, before any request."""
    with pytest.raises(ValueError, match=re.escape(url)):
        requested(url, "GET", "")


def test_a_scope_closes_its_client(server: tuple[str, Sequence[Received]], scoped: Callable[[str], httpx2.Client]) -> None:
    """`test_client_closed_state_using_with_block`: past its scope a client is closed and refuses another request."""
    url, _ = server
    client = scoped(url)
    assert client.is_closed
    with pytest.raises(RuntimeError, match="closed"):
        client.get(url)


def test_a_request_carries_the_current_trace(server: tuple[str, Sequence[Received]], requested: Requested) -> None:
    """W3C Trace Context "traceparent Header": a request inside a sampled span states version 00, its trace id, a parent id, and flags with sampled bit set."""
    url, _ = server
    with TracerProvider(shutdown_on_exit=False).get_tracer("suite").start_as_current_span("render") as span:
        echoed = requested(url, "GET", "/echo_headers").json()
    stated = re.fullmatch(rf"00-{span.get_span_context().trace_id:032x}-[0-9a-f]{{16}}-([0-9a-f]{{2}})", echoed["traceparent"])
    assert stated
    assert int(stated[1], 16) & 1


def test_a_manager_states_its_version(manager: tuple[str, Sequence[Received]], requested: Requested) -> None:
    """`getVersion`: the 200 body holds every required property of `FlamencoVersion`."""
    url, _ = manager
    assert requested(url, "GET", "/api/v3/version").json().keys() == {"version", "shortversion", "name", "git"}


def test_a_json_body_posts_a_job_the_manager_stores(manager: tuple[str, Sequence[Received]], requested: Requested) -> None:
    """`test_post_json`, `submitJob` 200: a JSON body arrives as `application/json` and answers with the submitted job and its id."""
    url, received = manager
    response = requested(url, "POST", "/api/v3/jobs", json=JOB)
    assert response.status_code == httpx2.codes.OK
    assert received[-1].headers["content-type"] == "application/json"
    assert response.json().items() >= JOB.items()
    assert response.json()["id"]


def test_a_job_check_stores_nothing(manager: tuple[str, Sequence[Received]], requested: Requested) -> None:
    """`submitJobCheck` 204: a valid job answers with no content."""
    url, _ = manager
    response = requested(url, "POST", "/api/v3/jobs/check", json=JOB)
    assert (response.status_code, response.content) == (httpx2.codes.NO_CONTENT, b"")


@pytest.mark.parametrize("operation", ["/api/v3/jobs", "/api/v3/jobs/check"])
def test_a_stale_job_type_etag_is_a_412_error(manager: tuple[str, Sequence[Received]], requested: Requested, operation: str) -> None:
    """`submitJob` and `submitJobCheck` 412: a stale job type etag answers an `Error` with the status as its code."""
    url, _ = manager
    response = requested(url, "POST", operation, json=JOB | {"type_etag": "stale"})
    assert response.status_code == httpx2.codes.PRECONDITION_FAILED
    assert response.json()["code"] == httpx2.codes.PRECONDITION_FAILED
    assert response.json()["message"]


def test_a_job_outside_its_schema_is_the_default_error(manager: tuple[str, Sequence[Received]], requested: Requested) -> None:
    """`submitJob` default response: a body missing a required `SubmittedJob` property answers an `Error`."""
    url, _ = manager
    response = requested(url, "POST", "/api/v3/jobs", json={"name": "untyped"})
    assert response.is_client_error
    assert response.json().keys() == {"code", "message"}


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def server(hosted: Hosted) -> tuple[str, Sequence[Received]]:
    """Returns a hosted plain HTTP server's base URL and the requests it received."""
    return hosted(plain)


@pytest.fixture
def requested() -> Requested:
    """Returns a request through the client scope."""
    return requesting


@pytest.fixture
def scoped() -> Callable[[str], httpx2.Client]:
    """Returns a server's client after its scope ended."""
    return lambda url: raised(Endpoint(url=url).opened(Ok)).client
