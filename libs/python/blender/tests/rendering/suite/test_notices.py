"""The library's notices against an ntfy server the test hosts, each case from the ntfy documentation `docs/publish.md` or `server/server_test.go` at v2.37.2, or httpx2 `test_exceptions.py` at v2.16.0."""

from collections.abc import Callable, Sequence
from datetime import timedelta
import functools
from http import HTTPStatus
import socket
import uuid

from expression import Nothing
import httpx2
import msgspec
import pytest

from rasm_rendering import Cancelled, Completed, Endpoint, Estimated, Failed, Forecast, Listener, Notice, published, Started, Test
from rasm_testing import raised
from rasm_testing.hosting import Answer, Hosted, Received

# --- [TYPES] ----------------------------------------------------------------------------

type Notified = Callable[[str, str, Notice], None]

# --- [CONSTANTS] ------------------------------------------------------------------------

JSON = {"content-type": "application/json"}
LEAST, MOST = 1, 5
LIMIT = 42901
LIMITED = Answer(
    status=HTTPStatus.TOO_MANY_REQUESTS,
    headers={**JSON, "retry-after": "1", "ratelimit-policy": '"requests";q=60;w=300', "ratelimit": '"requests";r=0;t=1'},
    body=msgspec.json.encode({"code": LIMIT, "http": HTTPStatus.TOO_MANY_REQUESTS, "error": "limit reached: too many requests"}),
)
PRIORITIES = {"min": 1, "low": 2, "default": 3, "high": 4, "max": 5, "urgent": 5} | {str(level): level for level in range(1, 6)}
NOTICES: tuple[Notice, ...] = (
    Started(jobs=2, frames=48),
    Estimated(forecast=Forecast(left=timedelta(minutes=5), finish=Nothing)),
    Completed(frames=48, elapsed=timedelta(seconds=90)),
    Cancelled(frames=12),
    Failed(job="Scene 1-24", reason="exit 1"),
    Test(),
)
EACH = pytest.mark.parametrize("notice", NOTICES, ids=lambda notice: type(notice).__name__)

# --- [MODELS] ---------------------------------------------------------------------------


class Published(msgspec.Struct, frozen=True, kw_only=True):
    """JSON publish of the ntfy documentation's "Publish as JSON" table: the required topic and the message, title, tags, and priority fields."""

    topic: str
    message: str = "triggered"
    title: str = ""
    tags: tuple[str, ...] = ()
    priority: int = 3


# --- [OPERATIONS] -----------------------------------------------------------------------


def notifying(messages: list[Published], received: Received, /) -> Answer:
    """Takes one publish as an ntfy server does: a JSON object on the root URL, or a text body on a topic URL with title, priority, and tags in headers, keeps its message, and answers it back, a body or priority outside the documented forms answered 400."""
    headers = received.headers
    try:
        message = (
            msgspec.json.decode(received.body, type=Published)
            if received.path == "/"
            else Published(
                topic=received.path.removeprefix("/"),
                message=received.body.decode() or "triggered",
                title=next((headers[name] for name in ("x-title", "title", "ti", "t") if name in headers), ""),
                tags=tuple(tag for name in ("x-tags", "tags", "tag", "ta") if name in headers for tag in headers[name].split(",")),
                priority=PRIORITIES[next((headers[name] for name in ("x-priority", "priority", "prio", "p") if name in headers), "default")],
            )
        )
    except (msgspec.DecodeError, KeyError) as error:
        return Answer(status=HTTPStatus.BAD_REQUEST, headers=JSON, body=msgspec.json.encode({"code": 40000, "http": 400, "error": str(error)}))
    messages.append(message)
    return Answer(headers=JSON, body=msgspec.json.encode({"id": uuid.uuid4().hex[:12], "time": 0, "event": "message"} | msgspec.to_builtins(message)))


def notified(url: str, topic: str, notice: Notice, /) -> None:
    """Publishes one notice through the library to a topic of an ntfy server, raising the exception a refused publish holds."""
    return raised(published(Listener(server=Endpoint(url=url), topic=topic), notice))


@EACH
def test_a_notice_is_one_message_on_its_topic(ntfy: tuple[str, Sequence[Published]], notify: Notified, notice: Notice) -> None:
    """`TestServer_PublishAndPoll`, `TestServer_PublishAsJSON`, "Message priority": one publish yields one message on the topic, its text set and its priority one of the five levels."""
    url, messages = ntfy
    notify(url, "mytopic", notice)
    (message,) = messages
    assert message.topic == "mytopic"
    assert message.message
    assert LEAST <= message.priority <= MOST


@EACH
def test_a_notice_published_as_json_goes_to_the_root_url(ntfy: tuple[str, Sequence[Published]], notify: Notified, notice: Notice) -> None:
    """Ntfy "Publish as JSON" note: JSON posted to a topic URL is taken as the message text, so no delivered message is a JSON object naming its topic."""
    url, messages = ntfy
    notify(url, "mytopic", notice)
    assert not messages[0].message.startswith("{")


@EACH
def test_a_rate_limited_publish_is_sent_again_after_its_retry_after(hosted: Hosted, notify: Notified, notice: Notice) -> None:
    """`TestServer_HandleError_RetryAfter`, RFC 9110 "Retry-After": one 429 of the request limit, with the `Retry-After`, `RateLimit-Policy`, and `RateLimit` rows ntfy sends, is followed by the same publish no sooner than its `Retry-After` second, which the server takes as one message."""
    messages: list[Published] = []
    first = iter([LIMITED])
    url, received = hosted(lambda request: next(first, None) or notifying(messages, request))
    notify(url, "mytopic", notice)
    limited, taken = received
    assert (limited.method, limited.path, limited.body) == (taken.method, taken.path, taken.body)
    assert taken.at - limited.at >= timedelta(seconds=1)
    assert [message.topic for message in messages] == ["mytopic"]


@EACH
def test_a_publish_that_stays_rate_limited_raises_its_status_error(hosted: Hosted, notify: Notified, notice: Notice) -> None:
    """`TestServer_PublishTooManyRequests_Defaults`, `test_export_max_retries`: a server answering 429 without end is asked six times, and the publish raises `HTTPStatusError` holding code 42901."""
    url, received = hosted(lambda _request: LIMITED)
    with pytest.raises(httpx2.HTTPStatusError) as refused:
        notify(url, "mytopic", notice)
    assert refused.value.response.json()["code"] == LIMIT
    assert len(received) == 6


@EACH
@pytest.mark.enable_socket
def test_a_closed_port_raises_a_connect_error(notify: Notified, notice: Notice) -> None:
    """`test_httpcore_exception_mapping`: publishing to a port nothing listens on raises `ConnectError`."""
    with socket.socket() as unused:
        unused.bind(("127.0.0.1", 0))
        port = unused.getsockname()[1]
    with pytest.raises(httpx2.ConnectError):
        notify(f"http://127.0.0.1:{port}", "mytopic", notice)


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def ntfy(hosted: Hosted) -> tuple[str, Sequence[Published]]:
    """Returns the base URL of an ntfy server the test hosts and the messages published to it."""
    messages: list[Published] = []
    url, _ = hosted(functools.partial(notifying, messages))
    return url, messages


@pytest.fixture
def notify() -> Notified:
    """Returns the library's publish of one notice to a topic of an ntfy server."""
    return notified
