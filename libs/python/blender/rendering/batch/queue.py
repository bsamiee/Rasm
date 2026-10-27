"""Render queue jobs, cameras, and frame ranges."""

from collections.abc import Mapping
from graphlib import TopologicalSorter
from pathlib import PurePath
from typing import Annotated

from expression import Option
import msgspec

from blender.records.fields import Case, Count, Side
from blender.records.identity import content_hash, ContentHash
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type _Frame = Annotated[int, msgspec.Meta(ge=0)]
type _Digest = Annotated[bytes, msgspec.Meta(min_length=32, max_length=32)]
type JobCamera = Bound | Fixed


# --- [MODELS] ---------------------------------------------------------------------------


class Frames(msgspec.Struct, frozen=True, kw_only=True):
    """Frame range with step."""

    start: _Frame
    end: _Frame
    step: _Frame

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


class Bound(Case, frozen=True, kw_only=True):
    """Scene camera switched at camera markers."""

    camera: str | None = None
    markers: tuple[Marker, ...] = ()

    def at(self, frame: int) -> Option[str]:
        """Returns camera of last marker reached, else first marker, else scene camera."""
        preceding = tuple(marker for marker in self.markers if marker.frame <= frame)
        marker = max(preceding, key=lambda marker: marker.frame) if preceding else min(self.markers, key=lambda marker: marker.frame) if self.markers else None
        return Option.of_optional(marker).map(lambda marker: marker.camera).or_else_with(lambda: Option.of_optional(self.camera))


class Fixed(Case, frozen=True, kw_only=True):
    """One camera for every job frame."""

    camera: str


class RenderFrames(msgspec.Struct, frozen=True, kw_only=True, tag=True):
    """Scene frames a farm worker renders."""

    scene: str
    frames: Annotated[tuple[_Frame, ...], msgspec.Meta(min_length=1)]


class Placed(msgspec.Struct, frozen=True, kw_only=True):
    """Local file a job sends to the farm, at its POSIX path in the job's pack, with its SHA-256."""

    source: str
    path: str
    digest: _Digest


class Job(msgspec.Struct, frozen=True, kw_only=True):
    """Scene or view layer render over frame range, sending its pack, the blend alone at its file name when the pack is empty."""

    file: str
    digest: _Digest
    scene: str
    layer: str | None = None
    engine: str
    frames: Frames
    camera: JobCamera
    pixels: tuple[Side, Side]
    samples: Count
    output_root: str
    outputs: Mapping[Output, str]
    passes: tuple[str, ...]
    prerequisites: tuple[_Frame, ...] = ()
    pack: tuple[Placed, ...] = ()

    def __post_init__(self) -> None:
        """Validates a pack holding the job's blend."""
        if self.pack and all(placed.source != self.file for placed in self.pack):
            raise ValueError(self.file)

    @property
    def placements(self) -> tuple[Placed, ...]:
        """Files the job sends."""
        return self.pack or (Placed(source=self.file, path=PurePath(self.file).name, digest=self.digest),)

    @property
    def blend(self) -> Placed:
        """Placement of the job's blend."""
        return next(placed for placed in self.placements if placed.source == self.file)

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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bound", "Fixed", "Frames", "Job", "JobCamera", "Marker", "Placed", "RenderFrames", "RenderQueue"]
