"""LUT formats, sequencer strip bakes, and changed-file reads."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack, nullcontext
from functools import cached_property, partial
import os
from pathlib import Path
from tempfile import TemporaryDirectory
from types import MappingProxyType
from typing import ClassVar, Final

import bpy
from expression import Some
from expression.extra.result import catch
import msgspec
import numpy as np
import PyOpenColorIO

from rasm_bpy.commands.invocations import Save
from rasm_bpy.commands.operators import Call, called, Done, Operator
from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import Acquisitions, Resource
from rasm_bpy.data._ids.ownership import Ownership, Saved
from rasm_bpy.data._rna.conditions import Count, Job
from rasm_bpy.data._rna.descriptors import Descriptor, Value
from rasm_bpy.data._rna.paths import _rna, RnaPath
from rasm_bpy.data._rna.properties import case_choices, choices
from rasm_bpy.data._rna.snapshots import assigned, Snapshot
from rasm_bpy.nodes._desired import _member_paths
from rasm_bpy.render._outputs.formats import OutputFormat
from rasm_bpy.render._outputs.results import _intermediate, _INTERMEDIATES
from rasm_imaging import converted, ImageFile, Lattice, LATTICE_SIZES, LutFormat, LutOutput, read_file, Spacing, Storage
from rasm_records import accumulated, Error, Fault, Nothing, Outcome, paired, Reason

# --- [CONSTANTS] ------------------------------------------------------------------------

FORMATS: Final = MappingProxyType({row.name: row for row in (*LutFormat.writable(), *LutFormat.bakeable())})
_LATTICE: Final = OutputFormat(media_type="IMAGE", file_format="OPEN_EXR", color_depth="32")

# --- [MODELS] ---------------------------------------------------------------------------


class LatticeFields(bpy.types.PropertyGroup):
    """Lattice size and node spacing of bake and view install forms."""

    size: bpy.props.IntProperty(name="Size", description="Nodes along each axis", min=LATTICE_SIZES.start, max=LATTICE_SIZES[-1], default=Lattice().size)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    spacing: bpy.props.EnumProperty(name="Spacing", description="Encoding before table", items=case_choices(Spacing), default=type(Lattice().spacing).__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Baking(LatticeFields):
    """Native bake format and its sampled lattice."""

    format: bpy.props.EnumProperty(name="Format", items=choices((row.name, f"{row.name} (.{row.extension})", "") for row in LutFormat.bakeable()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Writing(bpy.types.PropertyGroup):
    """Exact transform output in a native matrix-capable format."""

    format: bpy.props.EnumProperty(name="Format", items=choices((row.name, f"{row.name} (.{row.extension})", "") for row in LutFormat.writable()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class LutExport(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Strip LUT outputs baked from the active strip's modifiers in its sequencer space."""

    # --- [STATE]

    lattices: ClassVar[Ownership[bpy.types.Scene]] = Ownership(kind=bpy.types.Scene, key="lattice", saved=Saved.NEVER, single=False)
    modifiers: ClassVar[Descriptor[bpy.types.CollectionProperty]] = Descriptor[bpy.types.CollectionProperty](
        root=bpy.types.WorkSpace,
        owner=("sequencer_scene", "sequence_editor", "active_strip"),
        prop=bpy.types.Strip.bl_rna.properties["modifiers"],  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    )

    # --- [COMMANDS]

    @cached_property
    def strip_baking(self) -> Operator[Baking]:
        """Command baking the active strip's modifiers."""
        return Operator(form=Baking, run=self.bake_strip, condition=Some(Count(collection=self.modifiers) & Job(kind="RENDER")), invoke=Some(Save[Baking](extensions=extension)), options=frozenset())

    # --- [BAKES]

    @contextmanager
    def staged(self, identity: Identity) -> Generator[Outcome[bpy.types.Scene]]:
        """Owns a marked runtime scene, removed on exit on every path."""
        made = self.lattices.created(identity, bpy.data.scenes.new, "LUT Lattice")
        with ExitStack() as scope:
            for scene in made.to_option().to_list():
                scope.callback(bpy.data.scenes.remove, scene)
                scope.enter_context(_intermediate(identity, scene))
            yield made

    def sequenced(self, call: Call[Baking], active: bpy.types.Strip, space: str) -> Outcome[Done]:
        """Samples strip modifiers in one sequencer render over their native space."""
        lattice = lattice_of(call.properties)

        def arranged(scene: bpy.types.Scene, nodes: Path, look: Path) -> Outcome[None]:
            match scene.sequence_editor_create().strips.new_image(name="Lattice", filepath=str(nodes), channel=1, frame_start=1), scene.sequencer_colorspace_settings:
                case bpy.types.ImageStrip(
                    colorspace_settings=bpy.types.ColorManagedInputColorspaceSettings() as strip_space
                ) as strip, bpy.types.ColorManagedSequencerColorspaceSettings() as sequencer:
                    settings: dict[RnaPath, Value] = {
                        ("render", "resolution_x"): lattice.size**2,
                        ("render", "resolution_y"): lattice.size,
                        ("render", "resolution_percentage"): 100,
                        ("render", "use_sequencer"): True,
                        ("render", "use_compositing"): False,
                        ("render", "filepath"): str(look),
                        **_member_paths(_LATTICE.members(("render", "image_settings"))),
                        ("frame_start",): 1,
                        ("frame_end",): 1,
                    }
                    return paired(assigned(scene, settings), assigned(sequencer, {("name",): space}), assigned(strip_space, {("name",): space})).bind(
                        lambda _: accumulated(
                            Snapshot.capture(modifier).bind(lambda snapshot: snapshot.apply(strip.modifiers.new(name=modifier.name, type=modifier.type)).outcome) for modifier in active.modifiers
                        ).map(lambda _: None)
                    )
                case strip, _:
                    return Error(Fault.of(Reason.ABSENT, repr(strip), member="colorspace_settings"))

        def rendered(scene: bpy.types.Scene, config: PyOpenColorIO.Config, folder: Path) -> Outcome[ImageFile]:
            return (
                lattice
                .samples()
                .bind(lambda plane: ImageFile(pixels=plane, colorspace=space).written(folder / "nodes.exr", Storage(), {}, config))
                .bind(lambda nodes: arranged(scene, nodes, folder / "look"))
                .bind(lambda _: called(bpy.ops.render.render, {"scene": scene.name, "animation": True}))
                .bind(lambda _: read_file(Path(scene.render.frame_path(frame=1)), config))
            )

        def tabled(config: PyOpenColorIO.Config, look: ImageFile) -> Outcome[PyOpenColorIO.GroupTransform]:
            return converted(config, PyOpenColorIO.ColorSpaceTransform(src=look.colorspace, dst=space), np.ascontiguousarray(look.pixels[..., :3])).map(lattice.transform)

        config = PyOpenColorIO.GetCurrentConfig()
        with self.staged(call.identity) as made, TemporaryDirectory(dir=bpy.app.tempdir) as folder:
            return (
                made
                .bind(lambda scene: rendered(scene, config, Path(folder)))
                .bind(lambda look: tabled(config, look))
                .bind(lambda built: output(call.properties).written({call.paths[0]: built}))
                .map(Done.wrote)
            )

    def bake_strip(self, call: Call[Baking]) -> Outcome[Done]:
        """Bake active strip modifiers in sequencer space."""
        match self.modifiers.owner_of(call.context).to_optional():
            case bpy.types.Strip(id_data=bpy.types.Scene(sequencer_colorspace_settings=bpy.types.ColorManagedSequencerColorspaceSettings(name=space))) as active:
                return self.sequenced(call, active, space)
            case _:
                return Error(Fault.of(Reason.ABSENT, repr(self.modifiers.root), member=_rna(self.modifiers.owner)))

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns lattice ownership, read stamps, and the strip bake command."""
        return self.lattices, _STAMPS, _INTERMEDIATES, self.strip_baking


# --- [SERVICES] -------------------------------------------------------------------------

_STAMPS: Final = Resource[dict[str, tuple[int, int]]](made=lambda _identity: nullcontext({}))

# --- [OPERATIONS] -----------------------------------------------------------------------


def lattice_of(fields: LatticeFields) -> Lattice:
    """Returns the lattice the fields choose, each spacing at its defaults."""
    return Lattice(size=fields.size, spacing=msgspec.convert({"kind": fields.spacing}, Spacing.__value__))


def output(properties: Baking | Writing) -> LutOutput:
    """Returns exact writer for a writable format, else a lattice bake."""
    row = FORMATS[properties.format]
    return LutOutput(format=row, lattice=Some(lattice_of(properties)) if isinstance(properties, Baking) and row not in LutFormat.writable() else Nothing)


def extension(properties: Baking | Writing) -> tuple[str]:
    """Returns the chosen format's file extension."""
    return (f".{FORMATS[properties.format].extension}",)


def reloaded(identity: Identity, path: str, config: PyOpenColorIO.Config) -> Outcome[str]:
    """Returns the resolved path, clearing OCIO's process-wide caches when the file's modification time or size differs from the extension's last read."""
    resolved, stamps = Path(bpy.path.abspath(path)).resolve(), _STAMPS[identity]

    def held(status: os.stat_result) -> str:
        if stamps.get(str(resolved)) != (stamp := (status.st_mtime_ns, status.st_size)):
            PyOpenColorIO.ClearAllCaches()
            config.clearProcessorCache()
            stamps[str(resolved)] = stamp
        return str(resolved)

    return catch(resolved.stat, exception=OSError)().map_error(partial(Fault.of, Reason.UNREADABLE, str(resolved))).map(held)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FORMATS", "Baking", "LatticeFields", "LutExport", "Writing", "extension", "lattice_of", "output", "reloaded"]
