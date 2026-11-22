"""Requests a client scope sends: body encodings, refused statuses as faults, and the attempts of a request a busy server refuses."""

from collections import Counter
from collections.abc import Callable, Iterator, Sequence
from datetime import datetime, timedelta, UTC
from email.utils import format_datetime
from http import HTTPMethod

from expression import Error, NOTHING, Option, Some
import httpx2
import msgspec
import pytest

from rasm.records import Fault, Outcome
from rasm.rendering import Endpoint
from rasm.testing.hosting import Answer, Hosted, Received

# --- [TYPES] ----------------------------------------------------------------------------

type Server = Callable[[Iterator[Answer]], tuple[str, Sequence[Received]]]

# --- [MODELS] ---------------------------------------------------------------------------


class _Job(msgspec.Struct, frozen=True, kw_only=True):
    """Body a request sends as JSON."""

    name: str
    frames: int


# --- [OPERATIONS] -----------------------------------------------------------------------


def _sent(url: str, body: Option[_Job | bytes | Iterator[bytes]] = NOTHING, /, *, deadline: timedelta = timedelta(minutes=1)) -> Outcome[httpx2.Response]:
    return Endpoint(url=url, deadline=deadline).opened(lambda session: session.sent(HTTPMethod.POST, "/jobs", body))


def _refusal(outcome: Outcome[httpx2.Response], /) -> int:
    match outcome:
        case Error((Fault(cause=httpx2.HTTPStatusError(response=response)),)):
            return response.status_code
        case _:
            pytest.fail(f"no status refusal: {outcome}")


@pytest.mark.parametrize(
    "body,content_type,content",
    [
        pytest.param(Some(_Job(name="Scene", frames=48)), "application/json", b'{"name":"Scene","frames":48}', id="record"),
        pytest.param(Some(b"\x00blend"), "application/octet-stream", b"\x00blend", id="bytes"),
        pytest.param(NOTHING, None, b"", id="none"),
    ],
)
def test_a_body_is_sent_in_the_encoding_of_its_kind(server: Server, body: Option[_Job | bytes], content_type: str | None, content: bytes) -> None:
    """A record posts as JSON, bytes post as an octet stream, and no body posts empty with no content type."""
    url, received = server(iter(()))
    _sent(url, body)
    (request,) = received
    assert (request.headers.get("content-type"), request.body) == (content_type, content)


@pytest.mark.parametrize("status", [400, 404, 412, 500])
def test_a_refused_status_is_one_fault_sent_once(server: Server, status: int) -> None:
    """A 4xx or 5xx answer other than 429 and 503 is a fault holding the response, after one attempt."""
    url, received = server(iter([Answer(status=status)]))
    assert _refusal(_sent(url)) == status
    assert len(received) == 1


@pytest.mark.parametrize("status", [429, 503])
def test_a_busy_server_is_asked_again_after_its_retry_after_delay(server: Server, status: int) -> None:
    """A 429 or 503 with `Retry-After: 1` repeats the same request no sooner than one second later, and the repeat's answer is the outcome."""
    url, received = server(iter([Answer(status=status, headers={"retry-after": "1"})]))
    assert _sent(url, Some(b"frame")).is_ok()
    first, second = received
    assert (first.method, first.path, first.body) == (second.method, second.path, second.body)
    assert second.at - first.at >= timedelta(seconds=1)


def test_a_retry_after_date_in_the_past_is_asked_again_at_once(server: Server) -> None:
    """An HTTP-date `Retry-After` already past repeats the request without waiting."""
    past = format_datetime(datetime(2026, 1, 1, tzinfo=UTC), usegmt=True)
    url, received = server(iter([Answer(status=503, headers={"retry-after": past})]))
    assert _sent(url).is_ok()
    first, second = received
    assert second.at - first.at < timedelta(milliseconds=500)


def test_a_server_that_stays_busy_is_asked_six_times(server: Server) -> None:
    """A server answering 503 to every attempt gets the request six times, and the outcome is its last 503."""
    url, received = server(iter([Answer(status=503, headers={"retry-after": "0"})] * 10))
    assert _refusal(_sent(url)) == 503
    assert Counter(request.path for request in received) == {"/jobs": 6}


def test_a_retry_past_the_deadline_is_not_made(server: Server) -> None:
    """A `Retry-After` beyond the scope's deadline leaves the first 503 as the outcome."""
    url, received = server(iter([Answer(status=503, headers={"retry-after": "5"})]))
    assert _refusal(_sent(url, deadline=timedelta(seconds=1))) == 503
    assert len(received) == 1


def test_a_streamed_body_is_sent_once(server: Server) -> None:
    """A body read from an iterator cannot be sent again, so a 503 ends its request."""
    url, received = server(iter([Answer(status=503, headers={"retry-after": "0"})]))
    assert _refusal(_sent(url, Some(iter([b"part 1", b"part 2"])))) == 503
    assert [request.body for request in received] == [b"part 1part 2"]


def test_a_reply_outside_its_type_is_a_decode_fault(hosted: Hosted) -> None:
    """A JSON reply missing a field the requested record requires is a fault holding the validation error, not a raise."""
    url, _ = hosted(lambda _: Answer(headers={"content-type": "application/json"}, body=b'{"name":"Scene"}'))
    match Endpoint(url=url).opened(lambda session: session.exchanged(HTTPMethod.GET, "/jobs/1", _Job)):
        case Error((Fault(cause=msgspec.ValidationError() as error),)):
            assert "frames" in str(error)
        case other:
            pytest.fail(f"no decode fault: {other}")


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def server(hosted: Hosted) -> Server:
    """Returns the start of a server answering with each given answer in turn, then 200, and the requests it received."""
    return lambda answers: hosted(lambda _: next(answers, Answer()))
