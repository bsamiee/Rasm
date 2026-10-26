"""ASC CDL grade in the config's color timing space, with corrections read from and written to ASC and transform files."""

from collections.abc import Mapping
import functools
from typing import ClassVar

import bpy
from expression import Error, Ok, Option, Some
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Open, Operator, Save, Unchanged
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.graphs import Expr
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.render.color.luts import current, FORMATS
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, entered, Named, Registers, Shell, Spaces
from blender.bpy.render.compositor.inputs import captured
from blender.imaging.color.luts import corrections, LutFormat, LutOutput
from blender.imaging.color.processors import bracketed, processor
from blender.records.faults import Fault, Outcome, paired, RecordReason

from ...chain.stages import Effect, Stage, transform

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

    format: bpy.props.EnumProperty(name="Format", items=tuple((row.name, f"{row.name} (.{row.extension})", "") for row in LutFormat.writable()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    source: bpy.props.EnumProperty(name="Source", items=(("CORRECTION", "Correction", "Correction alone"), ("SCENE_LINEAR", "Scene Linear", "Correction between scene-linear conversions")))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GRADE]


def correction(values: Mapping[Key, Value]) -> Outcome[PyOpenColorIO.CDLTransform]:
    """Fresh OCIO CDL operation of the entry's slope, offset, power, and saturation."""
    match values:
        case {"slope": tuple(slope), "offset": tuple(offset), "power": tuple(power), "saturation": float(saturation)}:
            return Ok(PyOpenColorIO.CDLTransform(slope=list(map(float, slope)), offset=list(map(float, offset)), power=list(map(float, power)), sat=saturation))
        case _:
            return Error((Fault(reason=RecordReason.UNREADABLE, subject=Cdl.__name__, cause=repr(values)),))


@transform.register
def _(_record: Cdl, values: Mapping[Key, Value], _config: PyOpenColorIO.Config, /) -> Outcome[Option[PyOpenColorIO.Transform]]:
    """Returns the CDL operation."""
    return correction(values).map(Some)


def corrected(shell: Shell[Cdl]) -> Outcome[Expr]:
    """Binds the grade's inputs to the node form of OCIO's CDL."""
    return Ok(grades.cdl(shell.color, shell.input("slope"), shell.input("offset"), shell.input("power"), shell.input("saturation")))


# --- [EXCHANGE]


@functools.cache
def _named(path: str) -> tuple[tuple[str, str, str], ...]:
    """Correction rows of an ASC file in file order, each named by its ID or position, held for the enum the file browser draws."""
    return tuple((str(position), name.default_value(str(position + 1)), "") for found in corrections(path).to_option().to_list() for position, (name, _) in enumerate(found))


def import_cdl(call: Call[Imported]) -> Outcome[Answer]:
    """Read an ASC correction into the entry, keeping its blend and masks."""
    path, properties = str(call.paths[0]), call.properties

    def chosen(found: tuple[tuple[Option[str], PyOpenColorIO.CDLTransform], ...]) -> Outcome[PyOpenColorIO.CDLTransform]:
        return Option.of_optional(next((held for position, (_, held) in enumerate(found) if str(position) == properties.correction), None)).to_result_with(
            lambda: (Fault(reason=BlenderReason.ABSENT, subject=path, member=properties.correction),)
        )

    entry = call.root(bpy.types.Scene).bind(lambda scene: cdl.named(call.identity, scene, properties.name))
    return (
        paired(current(call.identity, path, PyOpenColorIO.GetCurrentConfig()).bind(corrections).bind(chosen), entry)
        .bind(
            lambda pair: entered(call.identity, pair[1], {"slope": tuple(pair[0].getSlope()), "offset": tuple(pair[0].getOffset()), "power": tuple(pair[0].getPower()), "saturation": pair[0].getSat()})
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
        call
        .root(bpy.types.Scene)
        .bind(lambda scene: cdl.named(call.identity, scene, properties.name))
        .bind(lambda entry: captured(call.identity, entry))
        .bind(lambda values: correction({key: owner.values.get("value") for key, owner in values.tree.owners.items() if key}))
        .bind(operations)
        .bind(lambda built: LutOutput(format=FORMATS[properties.format]).written({str(call.paths[0]): built}))
        .map(Done.wrote)
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

cdl = CompositorEffect(
    record=Cdl,
    graph=corrected,
    alpha=Alpha.STRAIGHT,
    interface=Interface(
        inputs=(
            *(
                Socket(key=key, name=bpy.path.display_name(key), kind=bpy.types.NodeSocketVector, description=description, members={"dimensions": 3, "default_value": default, **bounds})
                for key, description, default, bounds in (
                    ("slope", "Channel multipliers before offset", (1.0, 1.0, 1.0), {"min_value": 0.0}),
                    ("offset", "Channel values added after slope", (0.0, 0.0, 0.0), {}),
                    ("power", "Channel exponents on nonnegative values", (1.0, 1.0, 1.0), {"min_value": 0.0}),
                )
            ),
            Socket(key="saturation", name="Saturation", kind=bpy.types.NodeSocketFloat, description="Mix from CDL luma to graded color", members={"min_value": 0.0, "default_value": 1.0}),
        )
    ),
    parts=(
        Spaces(space=lambda _record: Cdl.space),
        Registers(
            rows=(
                Operator(form=Imported, run=import_cdl, invoke=Some(Open[Imported](extensions=lambda _properties: tuple(dict.fromkeys(f".{row.extension}" for row in LutFormat.readable()))))),
                Operator(form=Exported, run=export_cdl, invoke=Some(Save[Exported](extensions=lambda properties: (f".{FORMATS[properties.format].extension}",))), undo=False),
            )
        ),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Cdl", "Exported", "Imported", "cdl"]
