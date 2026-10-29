"""ASC CDL grade in the config's color timing space, with corrections read from and written to ASC and transform files."""

from collections.abc import Mapping
import functools
from pathlib import Path
from typing import ClassVar, Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block
import PyOpenColorIO

from blender.bpy.commands.invocations import Open, Save
from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.desired import Interface, Socket
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.color.luts import current, FORMATS
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, Named
from blender.bpy.render.compositor.inputs import captured, entered, Shell, valued
from blender.imaging.color.luts import corrections, LutFormat, LutOutput
from blender.imaging.color.processors import bracketed, processor
from blender.records.faults import Fault, Outcome, paired, Reason

from ...chain.stages import Effect, Stage, transform

# --- [CONSTANTS] ------------------------------------------------------------------------

_SLOPE: Final = Socket(key="slope", kind=bpy.types.NodeSocketVector, description="Channel multipliers before offset", default_value=(1.0, 1.0, 1.0), min_value=0.0)
_OFFSET: Final = Socket(key="offset", kind=bpy.types.NodeSocketVector, description="Channel values added after slope", default_value=(0.0, 0.0, 0.0))
_POWER: Final = Socket(key="power", kind=bpy.types.NodeSocketVector, description="Channel exponents on nonnegative values", default_value=(1.0, 1.0, 1.0), min_value=0.0)
_SATURATION: Final = Socket(key="saturation", kind=bpy.types.NodeSocketFloat, description="Mix from CDL luma to graded color", min_value=0.0, default_value=1.0)

# --- [MODELS] ---------------------------------------------------------------------------


class Cdl(Effect, frozen=True, kw_only=True):
    """ASC slope, offset, power, and saturation in the config's color timing space."""

    stage = Stage.CDL
    pointwise = True
    space: ClassVar[str] = PyOpenColorIO.ROLE_COLOR_TIMING


class Imported(Named):
    """ASC correction chosen in the file browser."""

    correction: bpy.props.EnumProperty(name="Correction", items=lambda operator, _context: _named(bpy.path.abspath(operator.filepath)))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Exported(Named):
    """Correction written in an exact transform format, alone or resolved between scene-linear conversions."""

    format: bpy.props.EnumProperty(name="Format", items=choices((row.name, f"{row.name} (.{row.extension})", "") for row in LutFormat.writable()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    source: bpy.props.EnumProperty(name="Source", items=choices((("CORRECTION", "Correction", "Correction alone"), ("SCENE_LINEAR", "Scene Linear", "Correction between scene-linear conversions"))))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GRADE]


def correction(values: Mapping[Key, Value]) -> Outcome[PyOpenColorIO.CDLTransform]:
    """Fresh OCIO CDL operation of the entry's slope, offset, power, and saturation."""
    match values:
        case {"slope": tuple(slope), "offset": tuple(offset), "power": tuple(power), "saturation": float(saturation)}:
            return Ok(PyOpenColorIO.CDLTransform(slope=list(map(float, slope)), offset=list(map(float, offset)), power=list(map(float, power)), sat=saturation))
        case _:
            return Error(Fault.of(Reason.UNREADABLE, Cdl.__name__, repr(values)))


@transform.register
def _(_record: Cdl, values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns the CDL operation."""
    return correction(values).map(Some)


# --- [EXCHANGE]


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
                return processor(PyOpenColorIO.GetCurrentConfig(), bracketed(Cdl.space, held)).map(PyOpenColorIO.Processor.createGroupTransform)
            case _:
                held.getFormatMetadata().setID(properties.name)
                return Ok(PyOpenColorIO.GroupTransform([held]))

    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: cdl.named(call.identity, scene, properties.name))
        .bind(lambda entry: captured(call.identity, entry))
        .map(valued)
        .bind(correction)
        .bind(operations)
        .bind(lambda built: LutOutput(format=FORMATS[properties.format]).written({call.paths[0]: built}))
        .map(Done.wrote)
    )


# --- [GRAPH]


def _cdl(shell: Shell[Cdl]) -> Outcome[Expr]:
    """Binds the grade's inputs to the node form of OCIO's CDL."""
    return Ok(grades.cdl(shell.color, shell.input(_SLOPE), shell.input(_OFFSET), shell.input(_POWER), shell.input(_SATURATION)))


cdl: Final = CompositorEffect(
    graph=_cdl,
    record=Cdl,
    mix=Blended(alpha=Alpha.STRAIGHT),
    interface=Interface(inputs=(_SLOPE, _OFFSET, _POWER, _SATURATION)),
    space=Some(lambda _record: Cdl.space),
    registers=(
        Operator(form=Imported, run=import_cdl, invoke=Some(Open[Imported](extensions=lambda _properties: tuple(dict.fromkeys(f".{row.extension}" for row in LutFormat.readable()))))),
        Operator(form=Exported, run=export_cdl, invoke=Some(Save[Exported](extensions=lambda properties: (f".{FORMATS[properties.format].extension}",))), undo=Nothing),
    ),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Cdl", "Exported", "Imported", "cdl"]
