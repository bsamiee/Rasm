"""Queue notices `published` to ntfy: one publish of the ntfy JSON form on the root URL, durations stated in whole seconds, and no request without a topic."""

from datetime import timedelta
import re
from typing import Annotated

from expression import NOTHING, Ok
from hypothesis import example, given, strategies
import msgspec

from rasm.rendering import Completed, Endpoint, Estimated, Failed, Forecast, Listener, published, Test
from rasm.testing.hosting import Answer, Hosted

# --- [MODELS] ---------------------------------------------------------------------------


class _Publish(msgspec.Struct, frozen=True, kw_only=True):
    """JSON publish of the ntfy documentation's "Publish as JSON" table, priority 1 (min) to 5 (max)."""

    topic: str
    message: str
    priority: Annotated[int, msgspec.Meta(ge=1, le=5)] = 3


# --- [OPERATIONS] -----------------------------------------------------------------------


def test_a_notice_is_one_ntfy_json_publish_on_the_root_url(hosted: Hosted) -> None:
    """A failure notice posts one JSON object of the ntfy form to the root URL under the topic, its message naming the job and reason it holds."""
    url, received = hosted(lambda _: Answer(headers={"content-type": "application/json"}, body=b'{"id":"a1","event":"message"}'))
    assert published(Listener(server=Endpoint(url=url), topic="studio"), Failed(job="Scene 7-31", reason="exit 139")) == Ok(None)
    (request,) = received
    publish = msgspec.json.decode(request.body, type=_Publish)
    assert (request.method, request.path, publish.topic) == ("POST", "/", "studio")
    assert ("Scene 7-31" in publish.message, "exit 139" in publish.message) == (True, True)


@given(elapsed=strategies.timedeltas(min_value=timedelta(), max_value=timedelta(days=3)))
@example(elapsed=timedelta(seconds=59, microseconds=999_999))
@example(elapsed=timedelta(days=1, microseconds=1))
def test_a_duration_is_stated_in_the_whole_seconds_already_passed(elapsed: timedelta) -> None:
    """Elapsed and remaining time read as a whole-second duration at most the one held and less than a second under it."""
    for message in (Completed(frames=48, elapsed=elapsed).message, Estimated(forecast=Forecast(left=elapsed, finish=NOTHING)).message):
        stated = re.search(r"(?:(\d+) days?, )?(\d+):(\d{2}):(\d{2})(\.\d+)?", message)
        assert stated is not None, message
        days, hours, minutes, seconds, fraction = stated.groups()
        rendered = timedelta(days=int(days or 0), hours=int(hours), minutes=int(minutes), seconds=int(seconds))
        assert (fraction, rendered <= elapsed < rendered + timedelta(seconds=1)) == (None, True), message


def test_a_listener_without_a_topic_sends_no_request(hosted: Hosted) -> None:
    """A listener holding no topic succeeds and its server receives nothing."""
    url, received = hosted(lambda _: Answer())
    assert published(Listener(server=Endpoint(url=url)), Test()) == Ok(None)
    assert received == []
