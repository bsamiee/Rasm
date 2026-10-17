"""Queue events posted to an ntfy listener."""

from fractions import Fraction
from http import HTTPMethod
from typing import Annotated

from expression import Ok, Some
import msgspec

from blender.imaging.parameters import Gated
from blender.rendering.batch.estimate import Estimate
from blender.rendering.batch.exchange import Endpoint, opened, sent
from blender.rendering.faults import RenderingResult

# --- [TYPES] ----------------------------------------------------------------------------

type Notice = Started | Estimated | Completed | Cancelled | Failed | Exhausted | Test


# --- [MODELS] ---------------------------------------------------------------------------


class Listener(msgspec.Struct, frozen=True, kw_only=True):
    """ntfy server and topic the queue notices post to."""

    server: Annotated[Endpoint, msgspec.Meta(description="ntfy server the notices post to")] = Endpoint(url="https://ntfy.sh")
    topic: Annotated[Gated[str], msgspec.Meta(description="ntfy topic the phone or desktop subscribes to, no notice posting while off")] = Gated(enabled=False, value="")


class Started(msgspec.Struct, frozen=True, kw_only=True):
    """Fresh run started."""

    jobs: int
    frames: int


class Estimated(msgspec.Struct, frozen=True, kw_only=True):
    """First estimate over every job."""

    estimate: Estimate


class Completed(msgspec.Struct, frozen=True, kw_only=True):
    """Run finished."""

    frames: int
    seconds: Fraction


class Cancelled(msgspec.Struct, frozen=True, kw_only=True):
    """Run stopped by the person."""

    frames: int


class Failed(msgspec.Struct, frozen=True, kw_only=True):
    """Run ended by a fault."""

    job: str
    reason: str


class Exhausted(msgspec.Struct, frozen=True, kw_only=True):
    """Run ended after its configured crash restarts."""

    job: str
    attempts: int


class Test(msgspec.Struct, frozen=True, kw_only=True):
    """Explicit test of the configured queue listener."""


class Publication(msgspec.Struct, frozen=True, kw_only=True):
    """Notice in ntfy's JSON publish form."""

    topic: str
    title: str
    message: str
    priority: Annotated[int, msgspec.Meta(ge=1, le=5)]
    tags: tuple[str, ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def published(listener: Listener, notice: Notice) -> RenderingResult[None]:
    """Posts a notice to the listener's topic."""

    def spent(seconds: float | Fraction) -> str:
        return f"{seconds:,.0f} s"

    tags: tuple[str, ...]
    match notice:
        case Test():
            title, message, priority, tags = "Render Queue Test", "Render queue notifications are configured", 3, ()
        case Started(jobs=jobs, frames=frames):
            title, message, priority, tags = "Render Started", f"{frames} frames in {jobs} jobs", 3, ("racehorse",)
        case Estimated(estimate=estimate):
            finish = estimate.finish.map(lambda moment: f", finishing at {moment:%H:%M}").default_value("")
            title, message, priority, tags = "Render Estimated", f"{spent(estimate.seconds)} left{finish}", 3, ("hourglass",)
        case Completed(frames=frames, seconds=seconds):
            title, message, priority, tags = "Render Finished", f"{frames} frames in {spent(seconds)}", 3, ("white_check_mark",)
        case Cancelled(frames=frames):
            title, message, priority, tags = "Render Cancelled", f"Stopped after {frames} frames", 4, ("warning",)
        case Failed(job=job, reason=reason):
            title, message, priority, tags = "Render Failed", f"{job}: {reason}", 5, ("x",)
        case Exhausted(job=job, attempts=attempts):
            title, message, priority, tags = "Render Failed", f"{job} crashed {attempts} times", 5, ("x",)
    publication = Publication(topic=listener.topic.value, title=title, message=message, priority=priority, tags=tags)
    return opened(listener.server, lambda client: sent(client, HTTPMethod.POST, "/", Some(publication)).map(lambda _: None)) if listener.topic.enabled else Ok(None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Cancelled", "Completed", "Estimated", "Exhausted", "Failed", "Listener", "Notice", "Publication", "Started", "Test", "published"]
