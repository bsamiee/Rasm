"""Render queues of jobs over scenes, view layers, cameras, and frame ranges, and their stored records."""

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
    """Frame range a job renders and the step between its frames."""

    start: Annotated[Frame, msgspec.Meta(description="First frame rendered")]
    end: Annotated[Frame, msgspec.Meta(description="Last frame of the range")]
    step: Annotated[Frame, msgspec.Meta(description="Frames advanced between renders, 0 rendering the start frame alone")]

    def __post_init__(self) -> None:
        """Admits an ordered frame interval at construction."""
        if self.start > self.end:
            raise ValueError((self.start, self.end))

    @property
    def numbers(self) -> range:
        """Frames a render writes."""
        return range(self.start, self.end + 1, self.step) if self.step else range(self.start, self.start + 1)


class Marker(msgspec.Struct, frozen=True, kw_only=True):
    """Timeline marker binding a camera from its frame on."""

    frame: int
    camera: str


class Bound(Case, frozen=True, kw_only=True, tag=1):
    """Scene camera, switched at each timeline marker bound to a camera."""

    title = "Scene Camera"
    camera: str | None = None
    markers: tuple[Marker, ...] = ()

    def at(self, frame: int) -> Option[str]:
        """Answers the camera rendering a frame."""
        preceding = tuple(marker for marker in self.markers if marker.frame <= frame)
        marker = max(preceding, key=lambda marker: marker.frame) if preceding else min(self.markers, key=lambda marker: marker.frame) if self.markers else None
        return Option.of_optional(marker).map(lambda marker: marker.camera).or_else_with(lambda: Option.of_optional(self.camera))


class Fixed(Case, frozen=True, kw_only=True, tag=2):
    """One camera for every frame of the job."""

    title = "Fixed Camera"
    camera: str


class RenderFrames(msgspec.Struct, frozen=True, kw_only=True, tag=2):
    """Frames of a saved scene rendered by a farm worker without local progress."""

    scene: str
    frames: Annotated[tuple[Frame, ...], msgspec.Meta(min_length=1)]


class Job(msgspec.Struct, frozen=True, kw_only=True):
    """Scene or view layer rendered from its own blend file over a frame range."""

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
    """Jobs in render order with the Blender and extension versions that built them."""

    jobs: Annotated[tuple[Job, ...], msgspec.Meta(min_length=1)]
    blender: str
    extension: str

    def __post_init__(self) -> None:
        """Admits a dependency graph over the queue's jobs at construction."""
        if unknown := ({prerequisite for job in self.jobs for prerequisite in job.prerequisites}.difference(range(len(self.jobs)))):
            raise ValueError(unknown)
        TopologicalSorter({part: job.prerequisites for part, job in enumerate(self.jobs)}).prepare()

    @property
    def key(self) -> ContentHash:
        """Content hash identifying the queue."""
        return content_hash(RenderQueue, self)

    @property
    def stem(self) -> str:
        """Hex digest of the key, naming the queue's files and folders."""
        return self.key.hex()

    @property
    def steps(self) -> tuple[tuple[int, int], ...]:
        """Job index and frame of every render, in job order."""
        return tuple((index, frame) for index, job in enumerate(self.jobs) for frame in job.frames.numbers)


# --- [OPERATIONS] -----------------------------------------------------------------------


def load[T](path: Path, kind: type[T]) -> RenderingResult[T]:
    """Reads a stored record."""
    try:
        return Ok(msgspec.json.decode(path.read_bytes(), type=kind))
    except (OSError, msgspec.DecodeError) as error:
        return Error((RenderingReadFault(request=path, error=error),))


def store(path: Path, content: bytes) -> RenderingResult[Path]:
    """Writes a file in one rename over its path."""

    def replaced() -> Path:
        staged = path.with_name(f"{path.name}.partial")
        path.parent.mkdir(parents=True, exist_ok=True)
        staged.write_bytes(content)
        return staged.replace(path)

    return catch(replaced, exception=OSError)().map_error(lambda error: (RenderingWriteFault(request=path, error=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bound", "Fixed", "Frame", "Frames", "Job", "JobCamera", "Marker", "RenderFrames", "RenderQueue", "load", "store"]
