"""Render queue jobs and stored records."""

from collections.abc import Callable
from graphlib import TopologicalSorter
from pathlib import Path
from typing import Annotated

from expression import Error, Ok, Option
from expression.extra.result import catch
import msgspec

from blender.imaging.hashing import content_hash, ContentHash
from blender.imaging.parameters import Case
from blender.rendering.faults import RenderingReadFault, RenderingResult, RenderingWriteFault
from blender.rendering.outputs.framing import Side
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type Frame = Annotated[int, msgspec.Meta(ge=0)]
type JobCamera = Bound | Fixed


# --- [MODELS] ---------------------------------------------------------------------------


class Frames(msgspec.Struct, frozen=True, kw_only=True):
    """Frame range with step."""

    start: Annotated[Frame, msgspec.Meta(description="First frame rendered")]
    end: Annotated[Frame, msgspec.Meta(description="Last frame of range")]
    step: Annotated[Frame, msgspec.Meta(description="Frames between renders, 0 renders start frame alone")]

    def __post_init__(self) -> None:
        """Validates start at or before end."""
        if self.start > self.end:
            raise ValueError((self.start, self.end))

    @property
    def numbers(self) -> range:
        """Frames a render writes."""
        return range(self.start, self.end + 1, self.step) if self.step else range(self.start, self.start + 1)


class Marker(msgspec.Struct, frozen=True, kw_only=True):
    """Timeline marker binding camera from its frame on."""

    frame: int
    camera: str


class Bound(Case, frozen=True, kw_only=True, tag=1):
    """Scene camera switched at camera markers."""

    title = "Scene Camera"
    camera: str | None = None
    markers: tuple[Marker, ...] = ()

    def at(self, frame: int) -> Option[str]:
        """Returns camera of last marker reached, else first marker, else scene camera."""
        preceding = tuple(marker for marker in self.markers if marker.frame <= frame)
        marker = max(preceding, key=lambda marker: marker.frame) if preceding else min(self.markers, key=lambda marker: marker.frame) if self.markers else None
        return Option.of_optional(marker).map(lambda marker: marker.camera).or_else_with(lambda: Option.of_optional(self.camera))


class Fixed(Case, frozen=True, kw_only=True, tag=2):
    """One camera for every job frame."""

    title = "Fixed Camera"
    camera: str


class RenderFrames(msgspec.Struct, frozen=True, kw_only=True, tag=2):
    """Scene frames a farm worker renders."""

    scene: str
    frames: Annotated[tuple[Frame, ...], msgspec.Meta(min_length=1)]


class Job(msgspec.Struct, frozen=True, kw_only=True):
    """Scene or view layer render over frame range."""

    file: str
    digest: Annotated[bytes, msgspec.Meta(min_length=32, max_length=32)]
    scene: str
    layer: str | None = None
    engine: str
    frames: Frames
    camera: JobCamera
    pixels: tuple[Side, Side]
    samples: Annotated[int, msgspec.Meta(ge=1)]
    output_root: str
    outputs: dict[Output, str]
    passes: tuple[str, ...]
    prerequisites: tuple[Frame, ...] = ()

    @property
    def title(self) -> str:
        """Scene, layer when set, and frame range."""
        return " ".join((self.scene, *Option.of_optional(self.layer).to_list(), f"{self.frames.start}-{self.frames.end}"))

    @property
    def weight(self) -> int:
        """Rendered pixels times samples."""
        width, height = self.pixels
        return width * height * self.samples


class RenderQueue(msgspec.Struct, frozen=True, kw_only=True):
    """Jobs in render order with Blender and extension versions."""

    jobs: Annotated[tuple[Job, ...], msgspec.Meta(min_length=1)]
    blender: str
    extension: str

    def __post_init__(self) -> None:
        """Validates prerequisites as acyclic job indices."""
        if unknown := ({prerequisite for job in self.jobs for prerequisite in job.prerequisites}.difference(range(len(self.jobs)))):
            raise ValueError(unknown)
        TopologicalSorter({part: job.prerequisites for part, job in enumerate(self.jobs)}).prepare()

    @property
    def key(self) -> ContentHash:
        """Content hash identifying queue."""
        return content_hash(RenderQueue, self)

    @property
    def stem(self) -> str:
        """Hex key naming queue files and folders."""
        return self.key.hex()

    @property
    def steps(self) -> tuple[tuple[int, int], ...]:
        """Job index and frame of every render, in job order."""
        return tuple((index, frame) for index, job in enumerate(self.jobs) for frame in job.frames.numbers)


# --- [OPERATIONS] -----------------------------------------------------------------------


def load[T](path: Path, kind: type[T]) -> RenderingResult[T]:
    """Reads and decodes stored record."""
    try:
        return Ok(msgspec.json.decode(path.read_bytes(), type=kind))
    except (OSError, msgspec.DecodeError) as error:
        return Error((RenderingReadFault(request=path, error=error),))


def store(path: Path, content: msgspec.Struct | bytes) -> RenderingResult[Path]:
    """Writes sorted-key record or bytes through one rename."""

    def replaced() -> Path:
        staged = path.with_name(f"{path.name}.partial")
        path.parent.mkdir(parents=True, exist_ok=True)
        staged.write_bytes(content if isinstance(content, bytes) else msgspec.json.encode(content, order="deterministic"))
        return staged.replace(path)

    return catch(replaced, exception=OSError)().map_error(lambda error: (RenderingWriteFault(request=path, error=error),))


def revised[T: msgspec.Struct](path: Path, kind: type[T], change: Callable[[T], T]) -> RenderingResult[T]:
    """Stores change to stored record and returns it."""
    return load(path, kind).map(change).bind(lambda held: store(path, held).map(lambda _: held))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bound", "Fixed", "Frame", "Frames", "Job", "JobCamera", "Marker", "RenderFrames", "RenderQueue", "load", "revised", "store"]
