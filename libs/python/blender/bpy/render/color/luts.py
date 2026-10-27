"""LUT formats, sequencer strip bakes, and changed-file reads."""

from collections.abc import Generator
from contextlib import contextmanager, nullcontext
from functools import cached_property
import os
from pathlib import Path
from tempfile import TemporaryDirectory
from types import MappingProxyType
from typing import ClassVar, Final, get_args

import bpy
from expression import Error, Nothing, Ok, Result, Some
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Call, Done, Operator, Save
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.conditions import AllOf, Count, Job
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderReason
from blender.imaging.color.luts import Lattice, LutFormat, LutOutput, Spacing
from blender.imaging.color.processors import processor
from blender.imaging.files.buffers import applied
from blender.imaging.files.images import ImageFile, read_file, Storage
from blender.records.faults import accumulated, Fault, Faults, Outcome, RecordReason

# --- [CONSTANTS] ------------------------------------------------------------------------

SPACINGS: Final = MappingProxyType({case.__name__: case for case in get_args(Spacing.__value__)})
FORMATS: Final = MappingProxyType({row.name: row for row in (*LutFormat.writable(), *LutFormat.bakeable())})

# --- [MODELS] ---------------------------------------------------------------------------


class Baking(bpy.types.PropertyGroup):
    """Native bake format and its sampled lattice."""

    format: bpy.props.EnumProperty(name="Format", items=tuple((row.name, f"{row.name} (.{row.extension})", "") for row in LutFormat.bakeable()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    size: bpy.props.IntProperty(name="Size", description="Nodes along each axis", min=2, max=129, default=33)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    spacing: bpy.props.EnumProperty(name="Spacing", description="Encoding before table", items=tuple((name, name, "") for name in SPACINGS))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Writing(bpy.types.PropertyGroup):
    """Exact transform output in a native matrix-capable format."""

    format: bpy.props.EnumProperty(name="Format", items=tuple((row.name, f"{row.name} (.{row.extension})", "") for row in LutFormat.matrices()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LutExport(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Strip LUT outputs baked from the active strip's modifiers in its sequencer space."""

    lattices: ClassVar[Ownership[bpy.types.Scene]] = Ownership(kind=bpy.types.Scene, key="lattice", saved=Saved.NEVER, single=False)
    modifiers: ClassVar[Descriptor[bpy.types.CollectionProperty]] = Descriptor.declared(bpy.types.WorkSpace, "sequencer_scene.sequence_editor.active_strip.modifiers", bpy.types.CollectionProperty)

    # --- [COMMANDS]

    @cached_property
    def stripping(self) -> Operator[Baking]:
        """Command baking the active strip's modifiers."""
        return Operator(
            form=Baking, run=self.bake_strip, condition=Some(AllOf(conditions=(Count(collection=self.modifiers), Job(kind="RENDER")))), invoke=Some(Save[Baking](extensions=extension)), undo=False
        )

    # --- [BAKES]

    @contextmanager
    def staged(self, identity: Identity) -> Generator[Outcome[bpy.types.Scene]]:
        """Owns a marked runtime scene, removed on exit."""
        made = self.lattices.made(
            identity, lambda: catch(bpy.data.scenes.new, exception=RuntimeError)("LUT Lattice").map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject="LUT Lattice", cause=error),))
        )
        try:
            yield made
        finally:
            for scene in made.to_option().to_list():
                bpy.data.scenes.remove(scene)

    def sequenced(self, call: Call[Baking], active: bpy.types.Strip, space: str) -> Outcome[Done]:
        """Samples strip modifiers in one sequencer render over their native space."""
        lattice = spaced(call.properties)

        def arranged(scene: bpy.types.Scene, nodes: str, look: Path) -> Outcome[None]:
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
                    return Error((Fault(reason=BlenderReason.ABSENT, subject=repr(strip), member="colorspace_settings"),))

        def rendered(scene: bpy.types.Scene, config: PyOpenColorIO.Config, folder: Path) -> Outcome[ImageFile[np.float32]]:
            samples: Outcome[npt.NDArray[np.float32]] = lattice.samples()
            return (
                Result[npt.NDArray[np.float32], Faults]
                .bind(samples, lambda plane: ImageFile(pixels=plane, colorspace=space).write(str(folder / "nodes.exr"), Storage(), {}, config))
                .bind(lambda nodes: arranged(scene, nodes, folder / "look"))
                .bind(lambda _: called(bpy.ops.render.render, {"scene": scene.name, "animation": True}))
                .bind(lambda _: read_file(scene.render.frame_path(frame=1), config))
            )

        def tabled(config: PyOpenColorIO.Config, look: ImageFile[np.float32]) -> Outcome[PyOpenColorIO.GroupTransform]:
            return processor(config, PyOpenColorIO.ColorSpaceTransform(src=look.colorspace, dst=space)).map(
                lambda built: lattice.transform(applied(built.getDefaultCPUProcessor(), np.ascontiguousarray(look.pixels[..., :3])))
            )

        config = PyOpenColorIO.GetCurrentConfig()
        with self.staged(call.identity) as made, TemporaryDirectory(dir=bpy.app.tempdir) as folder:
            return (
                made
                .bind(lambda scene: rendered(scene, config, Path(folder)))
                .bind(lambda look: tabled(config, look))
                .bind(lambda built: output(call.properties).written({str(call.paths[0]): built}))
                .map(Done.wrote)
            )

    def bake_strip(self, call: Call[Baking]) -> Outcome[Done]:
        """Bake active strip modifiers in sequencer space."""
        match self.modifiers.owner_of(call.context).to_optional():
            case bpy.types.Strip(id_data=bpy.types.Scene(sequencer_colorspace_settings=bpy.types.ColorManagedSequencerColorspaceSettings(name=space))) as active:
                return self.sequenced(call, active, space)
            case _:
                return Error((Fault(reason=BlenderReason.ABSENT, subject=repr(self.modifiers.root), member=self.modifiers.owner),))

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns lattice ownership, read stamps, and the strip bake command."""
        return Ok((self.lattices, STAMPS, self.stripping))


# --- [SERVICES] -------------------------------------------------------------------------

STAMPS: Final = Scoped[dict[str, tuple[int, int]]](made=lambda _identity: nullcontext({}))

# --- [OPERATIONS] -----------------------------------------------------------------------


def spaced(properties: Baking) -> Lattice:
    """Returns the lattice the bake properties choose, each spacing at its defaults."""
    return Lattice(size=properties.size, spacing=SPACINGS[properties.spacing]())


def output(properties: Baking) -> LutOutput:
    """Returns exact writer for a writable format, else a lattice bake."""
    row = FORMATS[properties.format]
    return LutOutput(format=row, lattice=Nothing if row in LutFormat.writable() else Some(spaced(properties)))


def extension(properties: Baking | Writing) -> tuple[str]:
    """Returns the chosen format's file extension."""
    return (f".{FORMATS[properties.format].extension}",)


def current(identity: Identity, path: str, config: PyOpenColorIO.Config) -> Outcome[str]:
    """Returns the resolved path, clearing OCIO's process-wide caches when the file's modification time or size differs from the extension's last read."""
    resolved, stamps = Path(bpy.path.abspath(path)).resolve(), identity.held[STAMPS]

    def held(status: os.stat_result) -> str:
        if stamps.get(str(resolved)) != (stamp := (status.st_mtime_ns, status.st_size)):
            PyOpenColorIO.ClearAllCaches()
            config.clearProcessorCache()
            stamps[str(resolved)] = stamp
        return str(resolved)

    return catch(resolved.stat, exception=OSError)().map_error(lambda error: (Fault(reason=RecordReason.UNREADABLE, subject=str(resolved), cause=error),)).map(held)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FORMATS", "SPACINGS", "STAMPS", "Baking", "LutExport", "Writing", "current", "extension", "output", "spaced"]
