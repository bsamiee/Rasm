"""Queue notices posted to ntfy."""

import datetime
from http import HTTPMethod
from typing import Annotated, ClassVar

from expression import Ok, Option, Some
import msgspec

from blender.records.faults import Outcome
from blender.rendering.batch.exchange import Endpoint, sent
from blender.rendering.batch.progress import Forecast

# --- [TYPES] ----------------------------------------------------------------------------

type Notice = Started | Estimated | Completed | Cancelled | Failed | Test

# --- [MODELS] ---------------------------------------------------------------------------


class Listener(msgspec.Struct, frozen=True, kw_only=True):
    """ntfy server and the topic devices subscribe to, no notices when the topic is absent."""

    server: Endpoint = Endpoint(url="https://ntfy.sh")
    topic: str | None = None


class _Posted(msgspec.Struct, frozen=True, kw_only=True):
    """Notice case base holding the ntfy priority and tags a case leaves at their defaults."""

    priority: ClassVar[int] = 3
    tags: ClassVar[tuple[str, ...]] = ()


class Started(_Posted, frozen=True, kw_only=True):
    """Fresh run started."""

    tags = ("racehorse",)
    jobs: int
    frames: int

    @property
    def message(self) -> str:
        """Frame and job counts."""
        return f"{self.frames} frames in {self.jobs} jobs"


class Estimated(_Posted, frozen=True, kw_only=True):
    """First forecast over every job."""

    tags = ("hourglass",)
    forecast: Forecast

    @property
    def message(self) -> str:
        """Time left and the local finish clock time."""
        return f"{_whole(self.forecast.left)} left{self.forecast.finish.map(lambda moment: f', done at {moment.astimezone():%H:%M}').default_value('')}"


class Completed(_Posted, frozen=True, kw_only=True):
    """Run finished."""

    tags = ("white_check_mark",)
    frames: int
    elapsed: datetime.timedelta

    @property
    def message(self) -> str:
        """Frame count and render time."""
        return f"{self.frames} frames in {_whole(self.elapsed)}"


class Cancelled(_Posted, frozen=True, kw_only=True):
    """Run stopped by user."""

    priority = 4
    tags = ("warning",)
    frames: int

    @property
    def message(self) -> str:
        """Frames rendered before the stop."""
        return f"Stopped after {self.frames} frames"


class Failed(_Posted, frozen=True, kw_only=True):
    """Run ended by fault."""

    priority = 5
    tags = ("x",)
    job: str
    reason: str

    @property
    def message(self) -> str:
        """Job and fault text."""
        return f"{self.job} · {self.reason}"


class Test(_Posted, frozen=True, kw_only=True):
    """Test post to listener."""

    @property
    def message(self) -> str:
        """Configuration confirmation."""
        return "Render queue notifications configured"


class _Publication(msgspec.Struct, frozen=True, kw_only=True):
    """Notice in ntfy JSON publish form."""

    topic: str
    title: str
    message: str
    priority: Annotated[int, msgspec.Meta(ge=1, le=5)]
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
