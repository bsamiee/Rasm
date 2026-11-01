"""Color transforms exchanged as files: the ASC CDL grade in the color timing space with corrections read from and written to ASC and transform files, and LUT files applied in their declared colorspace."""

from collections.abc import Mapping
import functools
from pathlib import Path
from typing import Final, Literal

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.invocations import Open, Save
from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.graphs import Expr
from blender.bpy.nodes.interfaces import Interface, Socket
from blender.bpy.render.color.luts import current, FORMATS
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.editors import FilePath
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, FromFile, Named, Prepared, Stage
from blender.bpy.render.compositor.grades import latticed, tabled, TILES
from blender.bpy.render.compositor.inputs import captured, entered, Shell, value, valued
from blender.imaging.color.luts import corrections, Lattice, LutFormat, LutOutput, Rational, split
from blender.imaging.color.processors import bracketed, ColorSpace, converted, processor
from blender.imaging.files.buffers import located, pixels
from blender.records.faults import Fault, Outcome, paired, Reason
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type _Triple = tuple[float, float, float]
type _Parts = tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_SLOPE: Final = Socket(key="slope", kind=bpy.types.NodeSocketVector, description="Channel multipliers before offset", default_value=(1.0, 1.0, 1.0), min_value=0.0)
_OFFSET: Final = Socket(key="offset", kind=bpy.types.NodeSocketVector, description="Channel values added after slope", default_value=(0.0, 0.0, 0.0))
_POWER: Final = Socket(key="power", kind=bpy.types.NodeSocketVector, description="Channel exponents on nonnegative values", default_value=(1.0, 1.0, 1.0), min_value=0.0)
_SATURATION: Final = Socket(key="saturation", kind=bpy.types.NodeSocketFloat, description="Mix from CDL luma to graded color", min_value=0.0, default_value=1.0)

# --- [MODELS] ---------------------------------------------------------------------------


class CDL(Case, frozen=True, kw_only=True):
    """ASC slope, offset, power, and saturation in the config's color timing space."""


class Imported(Named):
    """ASC correction chosen in the file browser."""

    correction: bpy.props.EnumProperty(name="Correction", items=lambda operator, _context: _named(bpy.path.abspath(operator.filepath)))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Exported(Named):
    """Correction written in an exact transform format, alone or resolved between scene-linear conversions."""

    format: bpy.props.EnumProperty(name="Format", items=choices((row.name, f"{row.name} (.{row.extension})", "") for row in LutFormat.writable()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    source: bpy.props.EnumProperty(name="Source", items=choices((("CORRECTION", "Correction", "Correction alone"), ("SCENE_LINEAR", "Scene Linear", "Correction between scene-linear conversions"))))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Lookup(Case, frozen=True, kw_only=True):
    """LUT file applied in its declared colorspace, the operations before its first 3D table sampled on a rational lattice of the stated size, or applied by the CPU render while no size is stated."""

    source: FilePath
    space: ColorSpace = PyOpenColorIO.ROLE_COLOR_PICKING
    lattice: Literal[17, 33, 65] | None = None


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CDL]


def _correction(values: Mapping[Key, Value]) -> Outcome[PyOpenColorIO.CDLTransform]:
    """Fresh OCIO CDL operation of the entry's slope, offset, power, and saturation."""
    return paired(value(values, _Triple, _SLOPE), value(values, _Triple, _OFFSET), value(values, _Triple, _POWER), value(values, float, _SATURATION)).map(
        lambda held: PyOpenColorIO.CDLTransform(slope=list(held[0]), offset=list(held[1]), power=list(held[2]), sat=held[3])
    )


@functools.cache
def _named(path: str) -> tuple[tuple[str, str, str], ...]:
    """Correction rows of an ASC file in file order, each named by its ID or position, held for the enum the file browser draws."""
    return tuple((str(position), name.default_value(str(position + 1)), "") for found in corrections(Path(path)).to_option().to_list() for position, (name, _) in enumerate(found))


def import_cdl(call: Call[Imported]) -> Outcome[Answer]:
    """Read an ASC correction into the entry, keeping its blend and masks."""
    path, properties = str(call.paths[0]), call.properties

    def chosen(found: tuple[tuple[Option[str], PyOpenColorIO.CDLTransform], ...]) -> Outcome[PyOpenColorIO.CDLTransform]:
        return (
            Block
            .of_seq(enumerate(found))
            .filter(lambda row: str(row[0]) == properties.correction)
            .map(lambda row: row[1][1])
            .try_head()
            .to_result_with(functools.partial(Fault.of, Reason.ABSENT, path, member=properties.correction))
        )

    entry = rooted(bpy.types.Scene, call.context).bind(lambda scene: cdl.named(call.identity, scene, properties.name))
    return (
        paired(current(call.identity, path, PyOpenColorIO.GetCurrentConfig()).bind(lambda resolved: corrections(Path(resolved))).bind(chosen), entry)
        .bind(
            lambda pair: entered(
                call.identity, pair[1], {_SLOPE.key: tuple(pair[0].getSlope()), _OFFSET.key: tuple(pair[0].getOffset()), _POWER.key: tuple(pair[0].getPower()), _SATURATION.key: pair[0].getSat()}
            )
        )
        .map(lambda changed: Done() if changed else Unchanged(reason=f"{properties.name} holds the chosen correction"))
    )


def export_cdl(call: Call[Exported]) -> Outcome[Answer]:
    """Write the entry's correction in the chosen format under its entry name."""
    properties = call.properties

    def operations(held: PyOpenColorIO.CDLTransform) -> Outcome[PyOpenColorIO.GroupTransform]:
        match properties.source:
            case "SCENE_LINEAR":
                return processor(PyOpenColorIO.GetCurrentConfig(), bracketed(PyOpenColorIO.ROLE_COLOR_TIMING, held)).map(PyOpenColorIO.Processor.createGroupTransform)
            case _:
                held.getFormatMetadata().setID(properties.name)
                return Ok(PyOpenColorIO.GroupTransform([held]))

    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: cdl.named(call.identity, scene, properties.name))
        .bind(lambda entry: captured(call.identity, entry))
        .map(valued)
        .bind(_correction)
        .bind(operations)
        .bind(lambda built: LutOutput(format=FORMATS[properties.format]).written({call.paths[0]: built}))
        .map(Done.wrote)
    )


def _cdl(shell: Shell[CDL]) -> Outcome[Expr]:
    """Binds the grade's inputs to the node form of OCIO's CDL."""
    return Ok(grades.cdl(shell.color, shell.input(_SLOPE), shell.input(_OFFSET), shell.input(_POWER), shell.input(_SATURATION)))


# --- [LOOKUP]


def _parts(identity: Identity, record: Lookup, config: PyOpenColorIO.Config) -> Outcome[_Parts]:
    """Splits the current file into the operations before its first 3D table and that table on its own lattice."""
    return current(identity, record.source, config).bind(lambda path: split(config, PyOpenColorIO.FileTransform(src=path, interpolation=PyOpenColorIO.INTERP_BEST)))


def _applied(stage: Stage[Lookup]) -> Outcome[Option[Prepared]]:
    """Applies the file's operations before its first 3D table to the entering pixels in the LUT's colorspace, Nothing while a lattice samples them or the file holds none."""
    record = stage.record
    return _parts(stage.identity, record, stage.config).bind(
        lambda parts: (
            Ok(Nothing)
            if record.lattice is not None or not len(parts[0])
            else pixels(stage.image).bind(lambda values: converted(stage.config, bracketed(record.space, parts[0]), values)).bind(lambda values: located(values, stage.image)).map(Some)
        )
    )


def lookup_reload(call: Call[Named]) -> Outcome[Answer]:
    """Rebuild the entry from its LUT file."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: lookup.named(call.identity, scene, call.properties.name).bind(lambda entry: lookup.revised(call.identity, scene, entry, Ok)))
        .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.properties.name} matches its current file"))
    )


def _lookup(shell: Shell[Lookup]) -> Outcome[Expr]:
    """Samples the operations before the file's first 3D table on the stated lattice, then that table and the rest."""
    record = shell.record
    name = Path(record.source).name

    def built(parts: _Parts) -> Outcome[Expr]:
        head, suffix = parts
        entering = (
            Option
            .of_optional(record.lattice)
            .filter(lambda _: len(head) > 0)
            .map(lambda size: latticed("prefix", f"{name} Prefix", shell.config, head, Lattice(size=size, spacing=Rational()), shell.color))
            .default_value(Ok(shell.color))
        )
        return entering.bind(lambda held: suffix.map(lambda table: tabled("source", name, *table, held, shell.config)).default_value(Ok(held)))

    return _parts(shell.identity, record, shell.config).bind(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

cdl: Final = CompositorEffect(
    _cdl,
    record=CDL,
    mix=Blended(alpha=Alpha.STRAIGHT),
    interface=Interface(inputs=(_SLOPE, _OFFSET, _POWER, _SATURATION)),
    pointwise=True,
    space=Some(lambda _record: PyOpenColorIO.ROLE_COLOR_TIMING),
    registers=(
        Operator(form=Imported, run=import_cdl, invoke=Some(Open[Imported](extensions=lambda _properties: LutFormat.readable_suffixes()))),
        Operator(form=Exported, run=export_cdl, invoke=Some(Save[Exported](extensions=lambda properties: (f".{FORMATS[properties.format].extension}",))), undo=Nothing),
    ),
)
lookup: Final = CompositorEffect(
    _lookup,
    record=Lookup,
    mix=Blended(alpha=Alpha.STRAIGHT),
    pointwise=True,
    prepared=Some(_applied),
    space=Some(lambda record: record.space),
    source=FromFile(extensions=LutFormat.readable_suffixes, of=lambda path: Lookup(source=path)),
    registers=(TILES, Operator(form=Named, run=lookup_reload)),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CDL", "Exported", "Imported", "Lookup", "cdl", "lookup"]
