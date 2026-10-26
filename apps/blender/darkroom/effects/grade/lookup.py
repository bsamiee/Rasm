"""LUT files applied in their declared colorspace: the operations before a file's first 3D table sampled on an authored lattice or applied on the CPU, the table itself tetrahedral in the compositor."""

from collections.abc import Mapping
from pathlib import Path

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.color.luts import current
from blender.bpy.render.compositor.effects import Alpha, CameraFrame, CompositorEffect, Named, Opens, Registers, revised, Shell, Spaces
from blender.bpy.render.compositor.tables import latticed, tabled, TILES
from blender.imaging.color.luts import Lattice, LutFormat, split
from blender.imaging.color.processors import bracketed, ColorSpace, converted
from blender.imaging.files.buffers import located, pixels
from blender.imaging.sampling.integration import Estimate, Precision
from blender.records.faults import Outcome

from ...chain.stages import Effect, prepared, Stage, transform

# --- [TYPES] ----------------------------------------------------------------------------

type Parts = tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]

# --- [MODELS] ---------------------------------------------------------------------------


class Lookup(Effect, frozen=True, kw_only=True):
    """LUT file applied in its declared colorspace, the lattice sampling the operations before its first 3D table in place of the CPU, and the image the CPU render prepared in place of the entering color, absent outside one."""

    stage = Stage.LOOKUP
    pointwise = True
    source: str
    space: ColorSpace = PyOpenColorIO.ROLE_COLOR_PICKING
    prefix: Lattice | None = None
    staged: IdName | None = None

    @property
    def file(self) -> PyOpenColorIO.FileTransform:
        """Fresh file operation at its best supported interpolation."""
        return PyOpenColorIO.FileTransform(src=self.source, interpolation=PyOpenColorIO.INTERP_BEST)


# --- [OPERATIONS] -----------------------------------------------------------------------


@transform.register
def _(record: Lookup, _values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns the file operation."""
    return Ok(Some(record.file))


@prepared.register
def _(
    record: Lookup, _values: Mapping[Key, Value], config: PyOpenColorIO.Config, image: OpenImageIO.ImageBuf, _precision: Precision, _frame: CameraFrame, _scene: bpy.types.Scene, /
) -> Outcome[Option[OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]]]:
    """Applies the file's unbounded operations before its first 3D table to the entering pixels in the LUT's colorspace, Nothing while a lattice samples them or the file holds none."""
    file = msgspec.structs.replace(record, source=str(Path(bpy.path.abspath(record.source)).resolve())).file
    return split(config, file).bind(
        lambda parts: (
            Ok(Nothing)
            if record.prefix is not None or not len(parts[0])
            else pixels(image).bind(lambda values: converted(config, bracketed(record.space, parts[0]), values)).bind(lambda values: located(values, image)).map(Some)
        )
    )


def looked_up(shell: Shell[Lookup]) -> Outcome[Expr]:
    """Samples the prefix on its authored lattice where set, then the file's first 3D table and the rest, the CPU-prepared image entering while staged."""
    record = shell.record
    name = Path(record.source).name

    def built(parts: Parts) -> Outcome[Expr]:
        head, suffix = parts
        entering = (
            Ok(Expr(kind=bpy.types.CompositorNodeImage, members=(("image", record.staged),), socket="Image", shape="RGBA"))
            if record.staged is not None
            else latticed("prefix", f"{name} Prefix", shell.config, head, record.prefix, shell.color)
            if len(head) and record.prefix is not None
            else Ok(shell.color)
        )
        return entering.bind(lambda held: suffix.map(lambda table: tabled("source", name, *table, held, shell.config)).default_value(Ok(held)))

    return current(shell.identity, record.source, shell.config).bind(lambda path: split(shell.config, msgspec.structs.replace(record, source=path).file)).bind(built)


def lookup_reload(call: Call[Named]) -> Outcome[Answer]:
    """Rebuild the entry from its LUT file."""
    return (
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: lookup.named(call.identity, scene, call.properties.name).bind(lambda entry: revised(lookup, call.identity, scene, entry, Ok)))
        .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.properties.name} matches its current file"))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

lookup = CompositorEffect(
    record=Lookup,
    graph=looked_up,
    alpha=Alpha.STRAIGHT,
    parts=(
        Spaces(space=lambda record: record.space),
        Opens(extensions=lambda: tuple(dict.fromkeys(f".{row.extension}" for row in LutFormat.readable())), of=lambda path: Lookup(source=path)),
        Registers(rows=(TILES, Operator(form=Named, run=lookup_reload))),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Lookup", "lookup"]
