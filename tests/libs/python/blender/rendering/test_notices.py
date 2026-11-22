"""Queue notices `published` to an ntfy server the test hosts: one JSON publish per notice on the root URL, its title, message, priority, and tags from its case, and none without a topic."""

from collections.abc import Sequence
from datetime import datetime, timedelta, UTC

from expression import NOTHING, Ok, Some
import msgspec
import pytest

from rasm.rendering import Cancelled, Completed, Endpoint, Estimated, Failed, Forecast, Listener, Notice, published, Started, Test
from rasm.testing import raised
from rasm.testing.hosting import Answer, Hosted, Received

# --- [MODELS] ---------------------------------------------------------------------------


class _Publish(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    """JSON publish of the ntfy documentation's "Publish as JSON" table, the fields a notice sets."""

    topic: str
    title: str
    message: str
    priority: int
    tags: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "notice,title,message,priority,tags",
    [
        pytest.param(Started(jobs=2, frames=48), "Render Started", "48 frames in 2 jobs", 3, ("racehorse",), id="started"),
        pytest.param(Estimated(forecast=Forecast(left=timedelta(minutes=5, seconds=7, milliseconds=900), finish=NOTHING)), "Render Estimated", "0:05:07 left", 3, ("hourglass",), id="estimated"),
        pytest.param(Completed(frames=48, elapsed=timedelta(seconds=90, microseconds=600_000)), "Render Completed", "48 frames in 0:01:30", 3, ("white_check_mark",), id="completed"),
        pytest.param(Cancelled(frames=12), "Render Cancelled", "Stopped after 12 frames", 4, ("warning",), id="cancelled"),
        pytest.param(Failed(job="Scene 1-24", reason="exit 1"), "Render Failed", "Scene 1-24 · exit 1", 5, ("x",), id="failed"),
        pytest.param(Test(), "Render Test", "Render queue notifications configured", 3, (), id="test"),
    ],
)
def test_a_notice_is_one_json_publish_of_its_case(ntfy: tuple[str, Sequence[Received]], notice: Notice, title: str, message: str, priority: int, tags: tuple[str, ...]) -> None:
    """Each notice posts one JSON object to the root URL naming the topic, its case as title, its message with durations in whole seconds, and its case's priority and tags."""
    url, received = ntfy
    assert published(Listener(server=Endpoint(url=url), topic="studio"), notice) == Ok(None)
    (request,) = received
    assert (request.method, request.path) == ("POST", "/")
    assert msgspec.json.decode(request.body, type=_Publish) == _Publish(topic="studio", title=title, message=message, priority=priority, tags=tags)


def test_an_estimate_with_a_finish_states_its_local_clock_time(ntfy: tuple[str, Sequence[Received]]) -> None:
    """An estimate holding a finish instant states it as hours and minutes in the local zone."""
    url, received = ntfy
    finish = datetime(2026, 11, 22, 18, 45, tzinfo=UTC)
    raised(published(Listener(server=Endpoint(url=url), topic="studio"), Estimated(forecast=Forecast(left=timedelta(minutes=5), finish=Some(finish)))))
    assert msgspec.json.decode(received[0].body, type=_Publish).message == f"0:05:00 left, done at {finish.astimezone():%H:%M}"


def test_a_listener_without_a_topic_publishes_nothing(ntfy: tuple[str, Sequence[Received]]) -> None:
    """A listener holding no topic succeeds without a request."""
    url, received = ntfy
    assert published(Listener(server=Endpoint(url=url)), Test()) == Ok(None)
    assert received == []


# --- [COMPOSITION] ----------------------------------------------------------------------


@pytest.fixture
def ntfy(hosted: Hosted) -> tuple[str, Sequence[Received]]:
    """Returns the base URL of an ntfy server answering every publish 200 and the requests it received."""
    return hosted(lambda _: Answer(headers={"content-type": "application/json"}, body=b'{"id":"a1","event":"message"}'))
