"""LUT formats, compositor and sequencer bakes, and changed-file reads."""

from collections.abc import Callable, Generator
from contextlib import contextmanager
from functools import cached_property
import os
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Annotated, ClassVar, Final, override, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Call, Done, Operator, Save
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import vacant
from blender.bpy.data.rna.conditions import AllOf, Count, Job
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.compositor.effects import Enabled
from blender.bpy.render.previews.scenes import PreviewScene
from blender.bpy.settings.properties import Choices
from blender.imaging.color.luts import Lattice, LutFormat, LutOutput
from blender.imaging.color.processors import processor
from blender.imaging.files.images import applied, ImageFile, read_file, Storage
from blender.records.faults import accumulated, DomainFault, FileFault, paired

# --- [CHOICES] --------------------------------------------------------------------------


def _formats(provider: Callable[[], tuple[LutFormat, ...]]) -> Choices:
    return Choices(lambda _identity, _owner, _context: ((row.name, f"{row.name} (.{row.extension})", "") for row in provider()))


bakeable = _formats(LutFormat.bakeable)
writable = _formats(LutFormat.writable)
matrices = _formats(LutFormat.matrices)

# --- [MODELS] ---------------------------------------------------------------------------


class Formatted(msgspec.Struct, frozen=True, kw_only=True):
    """Format name with its operation's format provider."""

    format: str
    formats: ClassVar[Callable[[], tuple[LutFormat, ...]]]

    def offered(self) -> LutFormat:
        """Returns selected format or provider's first dialog item."""
        return self.chosen().default_with(lambda _: type(self).formats()[0])

    def chosen(self) -> BlenderResult[LutFormat]:
        """Returns requested format, a choice fault when absent."""
        rows = type(self).formats()
        return Option.of_optional(next((row for row in rows if row.name == self.format), None)).to_result_with(
            lambda: (DomainFault(subject="format", value=self.format, accepted=", ".join(row.name for row in rows)),)
        )

    def seeded(self, _event: bpy.types.Event, _identity: Identity) -> Self:
        """Seeds file browser with offered format."""
        return msgspec.structs.replace(self, format=self.offered().name)

    def extensions(self) -> tuple[str, *tuple[str, ...]]:
        """Returns offered format's extension."""
        return (f".{self.offered().extension}",)

    def output(self) -> BlenderResult[LutOutput]:
        """Returns exact writer for chosen native format."""
        return self.chosen().map(lambda row: LutOutput(format=row))


class Baking(Formatted, frozen=True, kw_only=True):
    """Native bake format and its sampled lattice."""

    formats = staticmethod(LutFormat.bakeable)
    format: Annotated[str, msgspec.Meta(title="Format"), bakeable]
    lattice: Lattice = Lattice()

    @override
    def output(self) -> BlenderResult[LutOutput]:
        """Returns exact writer for a writable format, else a lattice bake."""
        return self.chosen().map(lambda row: LutOutput(format=row, lattice=Nothing if row in LutFormat.writable() else Some(self.lattice)))


class Writing(Formatted, frozen=True, kw_only=True):
    """Exact transform output in a native matrix-capable format."""

    formats = staticmethod(LutFormat.matrices)
    format: Annotated[str, msgspec.Meta(title="Format"), matrices]


class LutExport(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Formation and strip LUT outputs, the formation baked from the transform a caller states or sampled through the scene's enabled compositor entries."""

    sampling: PreviewScene = PreviewScene()
    transform: Callable[[Identity, bpy.types.Scene, Enabled], BlenderResult[Option[PyOpenColorIO.Transform]]] = lambda _identity, _scene, _enabled: Ok(Nothing)
    lattices: ClassVar[Ownership[bpy.types.Scene]] = Ownership(kind=bpy.types.Scene, key="lattice", saved=Saved.NEVER, single=False)
    modifiers: ClassVar[Descriptor[bpy.types.CollectionProperty]] = Descriptor.declared(bpy.types.WorkSpace, "sequencer_scene.sequence_editor.active_strip.modifiers", bpy.types.CollectionProperty)

    # --- [COMMANDS]

    @cached_property
    def forming(self) -> Operator[Baking]:
        """Command baking the enabled scene formation."""
        return Operator(
            key="bake_formation",
            title="Bake Formation LUT",
            description="Bake enabled scene formation into a native LUT",
            form=Baking,
            run=self.formed,
            condition=Some(Job(kind="RENDER")),
            invoke=Some(Save[Baking](extensions=Baking.extensions)),
            seed=Some(Baking.seeded),
            undo=False,
        )

    @cached_property
    def stripping(self) -> Operator[Baking]:
        """Command baking the active strip's modifiers."""
        return Operator(
            key="bake_strip",
            title="Bake Strip LUT",
            description="Bake active strip modifiers in sequencer space",
            form=Baking,
            run=self.stripped,
            condition=Some(AllOf(conditions=(Count(collection=self.modifiers), Job(kind="RENDER")))),
            invoke=Some(Save[Baking](extensions=Baking.extensions)),
            seed=Some(Baking.seeded),
            undo=False,
        )

    # --- [BAKES]

    def toned(self, identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], lattice: Lattice) -> BlenderResult[PyOpenColorIO.GroupTransform]:
        """Samples every RGB channel through entries over lattice domain."""
        samples: BlenderResult[npt.NDArray[np.float32]] = lattice.samples()
        return Result[npt.NDArray[np.float32], BlenderFaults].bind(samples, lambda plane: self.sampling.sampled(identity, scene, entries, plane)).map(lattice.transform)

    def formed(self, call: Call[Baking]) -> BlenderResult[Done]:
        """Writes enabled scene formation through atomic LUT writer, a lattice output or an unstated transform sampling the enabled entries."""
        lattice = call.record.lattice

        def baked(scene: bpy.types.Scene, output: LutOutput) -> BlenderResult[PyOpenColorIO.GroupTransform]:
            def sampled() -> BlenderResult[PyOpenColorIO.GroupTransform]:
                return self.toned(call.identity, scene, tuple(entry for entry in scene.compositor_effects if bool(Enabled.RENDER.descriptor.read(entry))), lattice)

            return self.transform(call.identity, scene, Enabled.RENDER).bind(
                lambda stated: stated.map(lambda transform: Ok(PyOpenColorIO.GroupTransform([transform]))).default_with(sampled) if output.lattice.is_none() else sampled()
            )

        return (
            paired(call.root(bpy.types.Scene), call.record.output())
            .bind(lambda parts: baked(*parts).map(lambda built: (parts[1], built)))
            .bind(lambda pair: pair[0].written({str(call.paths[0]): pair[1]}))
            .map(Done.wrote)
        )

    @contextmanager
    def staged(self, identity: Identity) -> Generator[BlenderResult[bpy.types.Scene]]:
        """Owns a marked runtime scene, removed on exit."""
        made = self.lattices.made(identity, lambda: BlenderEditFault.attempted("LUT Lattice", lambda: bpy.data.scenes.new(vacant("LUT Lattice", bpy.data.scenes.keys()))))
        try:
            yield made
        finally:
            for scene in made.to_option().to_list():
                bpy.data.scenes.remove(scene)

    def sequenced(self, call: Call[Baking], active: bpy.types.Strip, space: str) -> BlenderResult[Done]:
        """Samples strip modifiers in one sequencer render over their native space."""
        lattice = call.record.lattice

        def arranged(scene: bpy.types.Scene, nodes: str, look: Path) -> BlenderResult[None]:
            match scene.sequence_editor_create().strips.new_image(name="Lattice", filepath=nodes, channel=1, frame_start=1), scene.sequencer_colorspace_settings:
                case bpy.types.ImageStrip(
                    colorspace_settings=bpy.types.ColorManagedInputColorspaceSettings() as strip_space
                ) as strip, bpy.types.ColorManagedSequencerColorspaceSettings() as sequencer:
                    render = scene.render
                    render.resolution_x, render.resolution_y, render.resolution_percentage = lattice.size**2, lattice.size, 100
                    render.use_sequencer, render.use_compositing, render.filepath = True, False, str(look)
                    render.image_settings.file_format, render.image_settings.color_depth = "OPEN_EXR", "32"
                    scene.frame_start = scene.frame_end = 1
                    sequencer.name = strip_space.name = space  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                    return accumulated(
                        Snapshot.capture(modifier).bind(lambda snapshot: snapshot.apply(strip.modifiers.new(name=modifier.name, type=modifier.type)).outcome) for modifier in active.modifiers
                    ).map(lambda _: None)
                case strip, _:
                    return Error((BlenderAbsentFault(owner=repr(strip), key="colorspace_settings"),))

        def rendered(scene: bpy.types.Scene, config: PyOpenColorIO.Config, folder: Path) -> BlenderResult[ImageFile[np.float32]]:
            samples: BlenderResult[npt.NDArray[np.float32]] = lattice.samples()
            return (
                Result[npt.NDArray[np.float32], BlenderFaults]
                .bind(samples, lambda plane: ImageFile(pixels=plane, colorspace=space).write(str(folder / "nodes.exr"), Storage(), {}, config))
                .bind(lambda nodes: arranged(scene, nodes, folder / "look"))
                .bind(lambda _: called(bpy.ops.render.render, {"scene": scene.name, "animation": True}))
                .bind(lambda _: read_file(scene.render.frame_path(frame=1), config))
            )

        def tabled(config: PyOpenColorIO.Config, look: ImageFile[np.float32]) -> BlenderResult[PyOpenColorIO.GroupTransform]:
            return processor(config, PyOpenColorIO.ColorSpaceTransform(src=look.colorspace, dst=space)).map(
                lambda built: lattice.transform(applied(built.getDefaultCPUProcessor(), np.ascontiguousarray(look.pixels[..., :3])))
            )

        with self.staged(call.identity) as made, TemporaryDirectory(dir=bpy.app.tempdir) as folder:
            return (
                paired(paired(made, config_of((space,))), call.record.output())
                .bind(lambda parts: rendered(*parts[0], Path(folder)).bind(lambda look: tabled(parts[0][1], look)).map(lambda built: (parts[1], built)))
                .bind(lambda pair: pair[0].written({str(call.paths[0]): pair[1]}))
                .map(Done.wrote)
            )

    def stripped(self, call: Call[Baking]) -> BlenderResult[Done]:
        """Bakes active strip modifiers in the strip scene's sequencer space."""
        match self.modifiers.owner_of(call.context).to_optional():
            case bpy.types.Strip(id_data=bpy.types.Scene(sequencer_colorspace_settings=bpy.types.ColorManagedSequencerColorspaceSettings(name=space))) as active:
                return self.sequenced(call, active, space)
            case _:
                return Error((BlenderAbsentFault(owner=repr(self.modifiers.root), key=self.modifiers.owner),))

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns sampling scene, lattice ownership, and LUT commands."""
        return Ok((self.sampling, self.lattices, self.forming, self.stripping))


# --- [SERVICES] -------------------------------------------------------------------------

STAMPS: Final[dict[str, tuple[int, int]]] = {}

# --- [OPERATIONS] -----------------------------------------------------------------------


def current(path: str, config: PyOpenColorIO.Config) -> BlenderResult[str]:
    """Returns the resolved path, clearing OCIO's process-wide caches when the file's modification time or size differs from its last read."""
    resolved = Path(bpy.path.abspath(path)).resolve()

    def held(status: os.stat_result) -> str:
        if STAMPS.get(str(resolved)) != (stamp := (status.st_mtime_ns, status.st_size)):
            PyOpenColorIO.ClearAllCaches()
            config.clearProcessorCache()
            STAMPS[str(resolved)] = stamp
        return str(resolved)

    return catch(resolved.stat, exception=OSError)().map_error(lambda error: (FileFault(access="read", request=str(resolved), error=error),)).map(held)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["STAMPS", "Baking", "Formatted", "LutExport", "Writing", "bakeable", "current", "matrices", "writable"]
