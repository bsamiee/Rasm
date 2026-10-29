"""Queue notices posted to ntfy."""

import datetime
from http import HTTPMethod
from typing import ClassVar, override

from expression import Ok, Option, Some
import msgspec

from blender.records.faults import Outcome
from blender.rendering.batch.queue import Forecast
from blender.rendering.batch.targets import Endpoint, sent

# --- [TYPES] ----------------------------------------------------------------------------

type Notice = Started | Estimated | Completed | Cancelled | Failed | Test

# --- [MODELS] ---------------------------------------------------------------------------


class Listener(msgspec.Struct, frozen=True, kw_only=True):
    """ntfy server and the topic devices subscribe to, no notices when the topic is absent."""

    server: Endpoint = Endpoint(url="https://ntfy.sh")
    topic: str | None = None


class _Posted(msgspec.Struct, frozen=True, kw_only=True):
    """Notice case base holding the ntfy priority and tags a case leaves at their defaults, its message the case template over the case fields."""

    priority: ClassVar[int] = 3
    tags: ClassVar[tuple[str, ...]] = ()
    template: ClassVar[str] = ""

    @property
    def message(self) -> str:
        """Case template filled with the case fields, durations in whole seconds."""
        return self.template.format_map({key: _whole(value) if isinstance(value, datetime.timedelta) else value for key, value in msgspec.structs.asdict(self).items()})


class Started(_Posted, frozen=True, kw_only=True):
    """Fresh run started."""

    tags = ("racehorse",)
    template = "{frames} frames in {jobs} jobs"
    jobs: int
    frames: int


class Estimated(_Posted, frozen=True, kw_only=True):
    """First forecast over every job."""

    tags = ("hourglass",)
    forecast: Forecast

    @property
    @override
    def message(self) -> str:
        """Time left and the local finish clock time."""
        return f"{_whole(self.forecast.left)} left{self.forecast.finish.map(lambda moment: f', done at {moment.astimezone():%H:%M}').default_value('')}"


class Completed(_Posted, frozen=True, kw_only=True):
    """Run finished."""

    tags = ("white_check_mark",)
    template = "{frames} frames in {elapsed}"
    frames: int
    elapsed: datetime.timedelta


class Cancelled(_Posted, frozen=True, kw_only=True):
    """Run stopped by user."""

    priority = 4
    tags = ("warning",)
    template = "Stopped after {frames} frames"
    frames: int


class Failed(_Posted, frozen=True, kw_only=True):
    """Run ended by fault."""

    priority = 5
    tags = ("x",)
    template = "{job} · {reason}"
    job: str
    reason: str


class Test(_Posted, frozen=True, kw_only=True):
    """Test post to listener."""

    template = "Render queue notifications configured"


class _Publication(msgspec.Struct, frozen=True, kw_only=True):
    """Notice in ntfy JSON publish form."""

    topic: str
    title: str
    message: str
    priority: int
    tags: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _whole(span: datetime.timedelta) -> datetime.timedelta:
    return span - span % datetime.timedelta(seconds=1)


def published(listener: Listener, notice: Notice) -> Outcome[None]:
    """Posts notice titled by its case when the listener holds a topic."""
    return (
        Option
        .of_optional(listener.topic)
        .map(lambda topic: _Publication(topic=topic, title=f"Render {type(notice).__name__}", message=notice.message, priority=notice.priority, tags=notice.tags))
        .map(lambda publication: listener.server.opened(lambda client: sent(client, HTTPMethod.POST, "/", Some(publication)).map(lambda _: None)))
        .default_value(Ok(None))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Cancelled", "Completed", "Estimated", "Failed", "Listener", "Notice", "Started", "Test", "published"]
