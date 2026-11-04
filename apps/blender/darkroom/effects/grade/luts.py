"""Color transforms exchanged as files: the ASC CDL grade in the color timing space with corrections read from and written to ASC and transform files, and LUT files applied in their declared colorspace."""

from collections.abc import Callable, Mapping
from enum import auto, Enum
from functools import partial
from pathlib import Path
from typing import Final, Literal, override

import bpy
from expression import Option, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.invocations import Open, Save
from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.properties import Callbacks, choices, enum_choices, EnumItem
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.graphs import Expr
from blender.bpy.nodes.interfaces import Interface, Socket
from blender.bpy.render.color.luts import FORMATS, reloaded
from blender.bpy.render.color.spaces import ColorSpace
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.editors import FilePath
from blender.bpy.render.compositor.effects import CompositorEffect, CompositorRecord, FromFile, Mix, Named, Prepared, Stage
from blender.bpy.render.compositor.grades import latticed, tabled, TILES
from blender.bpy.render.compositor.inputs import captured, entered, GraphInputs, value, valued
from blender.imaging.color.luts import corrections, cube_split, Lattice, LutFormat, LutOutput, Rational
from blender.imaging.color.processors import bracketed, converted, processor
from blender.imaging.files.buffers import located, pixels
from blender.records.faults import Fault, Nothing, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type _Triple = tuple[float, float, float]
type _Parts = tuple[PyOpenColorIO.GroupTransform, Option[tuple[Lattice, npt.NDArray[np.float32]]]]


class _Source(Enum):
    """Correction written alone or between scene-linear conversions."""

    CORRECTION = auto()
    SCENE_LINEAR = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_SLOPE: Final = Socket(key="slope", kind=bpy.types.NodeSocketVector, description="Channel multipliers before offset", default_value=(1.0, 1.0, 1.0), min_value=0.0)
_OFFSET: Final = Socket(key="offset", kind=bpy.types.NodeSocketVector, description="Channel values added after slope", default_value=(0.0, 0.0, 0.0))
_POWER: Final = Socket(key="power", kind=bpy.types.NodeSocketVector, description="Channel exponents on nonnegative values", default_value=(1.0, 1.0, 1.0), min_value=0.0)
_SATURATION: Final = Socket(key="saturation", kind=bpy.types.NodeSocketFloat, description="Mix from CDL luma to graded color", min_value=0.0, default_value=1.0)

# --- [MODELS] ---------------------------------------------------------------------------


class CDL(CompositorRecord, frozen=True, kw_only=True):
    """ASC slope, offset, power, and saturation in the config's color timing space."""

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Binds the grade's inputs to the node form of OCIO's CDL."""
        return Ok(grades.cdl(shell.color, shell.input(_SLOPE), shell.input(_OFFSET), shell.input(_POWER), shell.input(_SATURATION)))

    @property
    @override
    def operation_space(self) -> str:
        """Colorspace in which this effect operates."""
        return PyOpenColorIO.ROLE_COLOR_TIMING


class Imported(Named):
    """ASC correction chosen in the file browser."""

    correction: bpy.props.EnumProperty(name="Correction", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Exported(Named):
    """Correction written in an exact transform format, alone or resolved between scene-linear conversions."""

    format: bpy.props.EnumProperty(name="Format", items=choices((row.name, f"{row.name} (.{row.extension})", "") for row in LutFormat.writable()))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    source: bpy.props.EnumProperty(name="Source", items=enum_choices(_Source))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Lookup(CompositorRecord, frozen=True, kw_only=True):
    """LUT file applied in its declared colorspace, the operations before its first 3D table sampled on a rational lattice of the stated size, or applied by the CPU render while no size is stated."""

    source: FilePath
    space: ColorSpace = PyOpenColorIO.ROLE_COLOR_PICKING
    lattice: Literal[17, 33, 65] | None = None

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Samples the operations before the file's first 3D table on the stated lattice, then that table and the rest."""
        name = Path(self.source).name

        def built(parts: _Parts) -> Outcome[Expr]:
            head, suffix = parts
            entering = (
                Option
                .of_optional(self.lattice)
                .filter(lambda _: len(head) > 0)
                .map(lambda size: latticed("prefix", f"{name} Prefix", shell.config, head, Lattice(size=size, spacing=Rational()), shell.color))
                .default_value(Ok(shell.color))
            )
            return entering.bind(lambda held: suffix.map(lambda table: tabled("source", name, *table, held, shell.config)).default_value(Ok(held)))

        return _parts(shell.identity, self, shell.config).bind(built)

    @property
    @override
    def operation_space(self) -> str:
        """Colorspace in which this effect operates."""
        return self.space

    @property
    @override
    def prepared(self) -> Callable[[Stage], Outcome[Option[Prepared]]]:
        """CPU preparation of this record before its graph reads the input."""

        def prepared(stage: Stage) -> Outcome[Option[Prepared]]:
            """Applies the file's operations before its first 3D table to the entering pixels in the LUT's colorspace, Nothing while a lattice samples them or the file holds none."""
            return _parts(stage.identity, self, stage.config).bind(
                lambda parts: (
                    Ok(Nothing)
                    if self.lattice is not None or not len(parts[0])
                    else pixels(stage.image).bind(lambda values: converted(stage.config, bracketed(self.space, parts[0]), values)).bind(lambda values: located(values, stage.image)).map(Some)
                )
            )

        return prepared


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CDL]


def _correction(values: Mapping[Key, Value]) -> Outcome[PyOpenColorIO.CDLTransform]:
    """Fresh OCIO CDL operation of the entry's slope, offset, power, and saturation."""
    return paired(value(values, _Triple, _SLOPE), value(values, _Triple, _OFFSET), value(values, _Triple, _POWER), value(values, float, _SATURATION)).map(
        lambda held: PyOpenColorIO.CDLTransform(slope=list(held[0]), offset=list(held[1]), power=list(held[2]), sat=held[3])
    )


def _named(identity: Identity, operator: Imported, _context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
    """ASC correction rows in file order, each value fixed to its file position."""
    path: str = operator.filepath  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute, unsound-assignment]
    found = reloaded(identity, path, PyOpenColorIO.GetCurrentConfig()).bind(lambda resolved: corrections(Path(resolved)))
    return choices((str(position), name.default_value(str(position + 1)), "", "NONE", position) for rows in found.to_option().to_list() for position, (name, _) in enumerate(rows))


def import_cdl(call: Call[Imported]) -> Outcome[Answer]:
    """Read an ASC correction into the entry, keeping its blend and masks."""
    path, properties = str(call.paths[0]), call.properties

    def chosen(found: tuple[tuple[Option[str], PyOpenColorIO.CDLTransform], ...]) -> Outcome[PyOpenColorIO.CDLTransform]:
        return Option.of_optional(next((correction for position, (_, correction) in enumerate(found) if str(position) == properties.correction), None)).to_result_with(
            partial(Fault.of, Reason.ABSENT, path, member=properties.correction)
        )

    entry = rooted(bpy.types.Scene, call.context).bind(lambda scene: cdl.named(call.identity, scene, properties.name))
    return (
        paired(reloaded(call.identity, path, PyOpenColorIO.GetCurrentConfig()).bind(lambda resolved: corrections(Path(resolved))).bind(chosen), entry)
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
        match _Source[properties.source]:
            case _Source.SCENE_LINEAR:
                return processor(PyOpenColorIO.GetCurrentConfig(), bracketed(PyOpenColorIO.ROLE_COLOR_TIMING, held)).map(PyOpenColorIO.Processor.createGroupTransform)
            case _Source.CORRECTION:
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


# --- [LOOKUP]


def _parts(identity: Identity, record: Lookup, config: PyOpenColorIO.Config) -> Outcome[_Parts]:
    """Splits the reloaded file into the operations before its first 3D table and that table on its own lattice."""
    return reloaded(identity, record.source, config).bind(lambda path: cube_split(config, PyOpenColorIO.FileTransform(src=path, interpolation=PyOpenColorIO.INTERP_BEST)))


def lookup_reload(call: Call[Named]) -> Outcome[Answer]:
    """Rebuild the entry from its LUT file."""
    return (
        rooted(bpy.types.Scene, call.context)
        .bind(lambda scene: lookup.named(call.identity, scene, call.properties.name).bind(lambda entry: lookup.revised(call.identity, scene, entry, Ok, form=Lookup)))
        .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.properties.name} matches its reloaded file"))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

cdl: Final = CompositorEffect(
    record=CDL,
    mix=Mix.STRAIGHT,
    interface=Interface(inputs=(_SLOPE, _OFFSET, _POWER, _SATURATION)),
    pointwise=True,
    registers=(
        Operator(
            form=Imported,
            run=import_cdl,
            invoke=Some(Open[Imported](extensions=lambda _properties: LutFormat.readable_suffixes())),
            callbacks=Some(lambda identity: {"correction": Callbacks[Imported](items=partial(_named, identity))}),
        ),
        Operator(form=Exported, run=export_cdl, invoke=Some(Save[Exported](extensions=lambda properties: (f".{FORMATS[properties.format].extension}",))), options=frozenset()),
    ),
)
lookup: Final = CompositorEffect(
    record=Lookup,
    mix=Mix.STRAIGHT,
    pointwise=True,
    source=FromFile(extensions=LutFormat.readable_suffixes, of=lambda path: Lookup(source=path)),
    registers=(TILES, Operator(form=Named, run=lookup_reload)),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CDL", "Exported", "Imported", "Lookup", "cdl", "lookup"]
