"""Queue notices posted to ntfy."""

from fractions import Fraction
from http import HTTPMethod
from typing import Annotated, ClassVar, override

from expression import Ok, Some
import msgspec

from blender.imaging.parameters import Gated
from blender.rendering.batch.estimate import Estimate
from blender.rendering.batch.exchange import Endpoint, sent
from blender.rendering.faults import RenderingResult

# --- [TYPES] ----------------------------------------------------------------------------

type Notice = Started | Estimated | Completed | Cancelled | Failed | Exhausted | Test


# --- [MODELS] ---------------------------------------------------------------------------


class Listener(msgspec.Struct, frozen=True, kw_only=True):
    """ntfy server and topic."""

    server: Annotated[Endpoint, msgspec.Meta(description="ntfy server")] = Endpoint(url="https://ntfy.sh")
    topic: Annotated[Gated[str], msgspec.Meta(description="Topic devices subscribe to, no notices when off")] = Gated(enabled=False, value="")


class Posted(msgspec.Struct, frozen=True, kw_only=True):
    """Notice with its ntfy title, priority, tags, and message template over its fields."""

    title: ClassVar[str]
    priority: ClassVar[int] = 3
    tags: ClassVar[tuple[str, ...]] = ()
    template: ClassVar[str]

    @property
    def message(self) -> str:
        """Template formatted over the notice fields."""
        return self.template.format_map(msgspec.structs.asdict(self))


class Started(Posted, frozen=True, kw_only=True):
    """Fresh run started."""

    title = "Render Started"
    tags = ("racehorse",)
    template = "{frames} frames in {jobs} jobs"
    jobs: int
    frames: int


class Estimated(Posted, frozen=True, kw_only=True):
    """First estimate over every job."""

    title = "Render Estimated"
    tags = ("hourglass",)
    estimate: Estimate

    @property
    @override
    def message(self) -> str:
        """Seconds left with the clock time of the finish when it is representable."""
        return f"{self.estimate.seconds:,.0f} s left{self.estimate.finish.map(lambda moment: f', done at {moment:%H:%M}').default_value('')}"


class Completed(Posted, frozen=True, kw_only=True):
    """Run finished."""

    title = "Render Finished"
    tags = ("white_check_mark",)
    template = "{frames} frames in {seconds:,.0f} s"
    frames: int
    seconds: Fraction


class Cancelled(Posted, frozen=True, kw_only=True):
    """Run stopped by user."""

    title = "Render Canceled"
    priority = 4
    tags = ("warning",)
    template = "Stopped after {frames} frames"
    frames: int


class Failed(Posted, frozen=True, kw_only=True):
    """Run ended by fault."""

    title = "Render Failed"
    priority = 5
    tags = ("x",)
    template = "{job} · {reason}"
    job: str
    reason: str


class Exhausted(Posted, frozen=True, kw_only=True):
    """Run ended at crash restart limit."""

    title = "Render Failed"
    priority = 5
    tags = ("x",)
    template = "{job} crashed {attempts} times"
    job: str
    attempts: int


class Test(Posted, frozen=True, kw_only=True):
    """Test post to listener."""

    title = "Render Queue Test"
    template = "Render queue notifications configured"


class Publication(msgspec.Struct, frozen=True, kw_only=True):
    """Notice in ntfy JSON publish form."""

    topic: str
    title: str
    message: str
    priority: Annotated[int, msgspec.Meta(ge=1, le=5)]
    tags: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def published(listener: Listener, notice: Notice) -> RenderingResult[None]:
    """Posts notice when topic is enabled."""
    publication = Publication(topic=listener.topic.value, title=notice.title, message=notice.message, priority=notice.priority, tags=notice.tags)
    return listener.server.opened(lambda client: sent(client, HTTPMethod.POST, "/", Some(publication)).map(lambda _: None)) if listener.topic.enabled else Ok(None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Cancelled", "Completed", "Estimated", "Exhausted", "Failed", "Listener", "Notice", "Posted", "Publication", "Started", "Test", "published"]
