"""Queue notices posted to ntfy."""

from abc import abstractmethod
from fractions import Fraction
from http import HTTPMethod
from typing import Annotated, ClassVar, override

from expression import Ok, Option, Some
import msgspec

from blender.records.faults import Outcome
from blender.rendering.batch.exchange import Endpoint, sent
from blender.rendering.batch.progress import Forecast

# --- [MODELS] ---------------------------------------------------------------------------


class Listener(msgspec.Struct, frozen=True, kw_only=True):
    """ntfy server and the topic devices subscribe to, no notices when the topic is absent."""

    server: Endpoint = Endpoint(url="https://ntfy.sh")
    topic: str | None = None


class Notice(msgspec.Struct, frozen=True, kw_only=True):
    """Queue notice with its ntfy priority, tags, and message over its fields."""

    priority: ClassVar[int] = 3
    tags: ClassVar[tuple[str, ...]] = ()

    @property
    @abstractmethod
    def message(self) -> str:
        """Notice text over the case fields."""


class Started(Notice, frozen=True, kw_only=True):
    """Fresh run started."""

    tags = ("racehorse",)
    jobs: int
    frames: int

    @property
    @override
    def message(self) -> str:
        return f"{self.frames} frames in {self.jobs} jobs"


class Estimated(Notice, frozen=True, kw_only=True):
    """First forecast over every job."""

    tags = ("hourglass",)
    forecast: Forecast

    @property
    @override
    def message(self) -> str:
        return f"{self.forecast.seconds:,.0f} s left{self.forecast.finish.map(lambda moment: f', done at {moment:%H:%M}').default_value('')}"


class Completed(Notice, frozen=True, kw_only=True):
    """Run finished."""

    tags = ("white_check_mark",)
    frames: int
    seconds: Fraction

    @property
    @override
    def message(self) -> str:
        return f"{self.frames} frames in {self.seconds:,.0f} s"


class Cancelled(Notice, frozen=True, kw_only=True):
    """Run stopped by user."""

    priority = 4
    tags = ("warning",)
    frames: int

    @property
    @override
    def message(self) -> str:
        return f"Stopped after {self.frames} frames"


class Failed(Notice, frozen=True, kw_only=True):
    """Run ended by fault."""

    priority = 5
    tags = ("x",)
    job: str
    reason: str

    @property
    @override
    def message(self) -> str:
        return f"{self.job} · {self.reason}"


class Test(Notice, frozen=True, kw_only=True):
    """Test post to listener."""

    @property
    @override
    def message(self) -> str:
        return "Render queue notifications configured"


class _Publication(msgspec.Struct, frozen=True, kw_only=True):
    """Notice in ntfy JSON publish form."""

    topic: str
    title: str
    message: str
    priority: Annotated[int, msgspec.Meta(ge=1, le=5)]
    tags: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


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
