"""LUT files applied in their declared colorspace: the operations before a file's first 3D table sampled on an authored lattice or applied on the CPU, the table itself tetrahedral in the compositor."""

from collections.abc import Mapping
from pathlib import Path
from typing import Annotated, override

import bpy
from expression import Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator, Outcome, Unchanged
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.color.luts import current
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.compositor.effects import Alpha, CameraFrame, CompositorEffect, Named, Opens, Prepares, Registers, revised, Shell, Spaces
from blender.bpy.render.compositor.tables import latticed, tabled, TILES
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.color.luts import Lattice, LutFormat, split
from blender.imaging.color.processors import bracketed, ColorSpace, converted
from blender.imaging.faults import ImagingResult
from blender.imaging.files.images import located, pixels
from blender.imaging.sampling.integration import Estimate, Precision
from blender.records.fields import Gated

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------

type Parts = tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]

# --- [MODELS] ---------------------------------------------------------------------------


class Lookup(Effect, frozen=True, kw_only=True):
    """LUT file applied in its declared colorspace."""

    stage = Stage.LOOKUP
    title = "LUT"
    pointwise = True
    source: Annotated[str, msgspec.Meta(min_length=1, title="File", description="LUT file in a format OpenColorIO reads"), GraphForm.IMAGE]
    space: Annotated[ColorSpace, GraphForm.COLOR_SPACE, msgspec.Meta(title="Color Space", description="Colorspace the LUT reads and writes")] = PyOpenColorIO.ROLE_COLOR_PICKING
    prefix: Annotated[Gated[Lattice], msgspec.Meta(title="Prefix Sampling", description="Lattice sampling the operations before the first 3D table in place of the CPU")] = Gated(
        enabled=False, value=Lattice()
    )

    @property
    def file(self) -> PyOpenColorIO.FileTransform:
        """Fresh file operation at its best supported interpolation."""
        return PyOpenColorIO.FileTransform(src=self.source, interpolation=PyOpenColorIO.INTERP_BEST)

    @override
    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns the file operation."""
        return Ok(Some(self.file))

    def parts(self, config: PyOpenColorIO.Config) -> BlenderResult[Parts]:
        """Splits the current file into the operations before its first 3D table and that table sampled on its own lattice."""
        return current(self.source, config).bind(lambda path: split(config, msgspec.structs.replace(self, source=path).file))

    def staged(self) -> bool:
        """Whether the CPU applies the file's operations before its first 3D table, the prefix unsampled and the file holding such operations."""
        return not self.prefix.enabled and config_of().bind(self.parts).map(lambda found: len(found[0]) > 0).default_value(False)


# --- [OPERATIONS] -----------------------------------------------------------------------


def looked_up(shell: Shell[Lookup]) -> BlenderResult[Expr]:
    """Samples the prefix on its authored lattice where enabled, then the file's first 3D table and the rest, the CPU-applied prefix entering otherwise."""
    record = shell.record
    name = Path(record.source).name
    source, _, prefix = Lookup.__struct_fields__

    def built(parts: Parts) -> BlenderResult[Expr]:
        head, suffix = parts
        entering = latticed(prefix, f"{name} Prefix", shell.config, head, record.prefix.value, shell.color) if len(head) and record.prefix.enabled else Ok(shell.color)
        return entering.bind(lambda held: suffix.map(lambda table: tabled(source, name, *table, held, shell.config)).default_value(Ok(held)))

    return record.parts(shell.config).bind(built)


def prepared(
    _identity: Identity,
    _scene: bpy.types.Scene,
    record: Lookup,
    config: PyOpenColorIO.Config,
    image: OpenImageIO.ImageBuf,
    _precision: Precision,
    _frame: CameraFrame,
    _entry: bpy.types.SceneCompositorEffect,
) -> BlenderResult[tuple[Lookup, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
    """Applies the file's unbounded operations before its first 3D table to the entering pixels in the LUT's colorspace."""
    return (
        record
        .parts(config)
        .bind(lambda parts: pixels(image).bind(lambda values: converted(config, bracketed(record.space, parts[0]), values)))
        .bind(lambda values: located(values, image))
        .map(lambda staged: (record, {Shell.image: staged}))
    )


def reloaded(call: Call[Named]) -> BlenderResult[Outcome]:
    """Rebuilds the named LUT entry from its current file."""
    return (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: lookup.named(call.identity, scene, call.record.name).bind(lambda entry: revised(lookup, call.identity, scene, entry, Ok)))
        .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.record.name} matches its current file"))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

reloading = Operator(key="lookup_reload", title="Reload LUT", description="Rebuild the entry from its LUT file", form=Named, run=reloaded)
lookup = CompositorEffect(
    record=Lookup,
    graph=looked_up,
    alpha=Alpha.STRAIGHT,
    parts=(
        Spaces(space=lambda record: record.space),
        Prepares(prepare=prepared, when=Lookup.staged),
        Opens(extensions=lambda: tuple(dict.fromkeys(f".{row.extension}" for row in LutFormat.readable())), of=lambda path: Lookup(source=path)),
        Registers(rows=(TILES, reloading)),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Lookup", "lookup"]
