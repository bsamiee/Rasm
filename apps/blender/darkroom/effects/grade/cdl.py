"""ASC CDL grade in the config's color timing space, with corrections read from and written to ASC and transform files."""

from typing import Annotated, ClassVar, override, Self

import bpy
from expression import Error, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Open, Operator, Outcome, Save, Unchanged
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.color.luts import current, Formatted, writable
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.compositor import grades
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, EffectName, Named, Registers, revised, Shell, Spaces
from blender.bpy.settings.properties import Choices
from blender.imaging.color.luts import corrections, LutFormat
from blender.imaging.color.processors import bracketed, processor
from blender.imaging.faults import ImagingResult
from blender.records.faults import accumulated, FileFault, paired
from blender.records.fields import Choice, Finite, NonNegative, Positive

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------


class Source(Choice):
    """Input space of an exported correction."""

    CORRECTION = 1
    SCENE_LINEAR = 2


# --- [MODELS] ---------------------------------------------------------------------------


class Cdl(Effect, frozen=True, kw_only=True):
    """ASC slope, offset, power, and saturation in the config's color timing space."""

    stage = Stage.CDL
    title = "ASC CDL"
    pointwise = True
    space: ClassVar[str] = PyOpenColorIO.ROLE_COLOR_TIMING
    slope: Annotated[tuple[NonNegative, NonNegative, NonNegative], msgspec.Meta(description="Channel multipliers before offset")] = (1.0, 1.0, 1.0)
    offset: Annotated[tuple[Finite, Finite, Finite], msgspec.Meta(description="Channel values added after slope")] = (0.0, 0.0, 0.0)
    power: Annotated[tuple[Positive, Positive, Positive], msgspec.Meta(description="Channel exponents on nonnegative values")] = (1.0, 1.0, 1.0)
    saturation: Annotated[NonNegative, msgspec.Meta(description="Mix from CDL luma to graded color")] = 1.0

    @classmethod
    def of(cls, path: str, correction: PyOpenColorIO.CDLTransform) -> ImagingResult[Self]:
        """Reads a file's correction into a grade, refusing values outside the grade's domains."""
        try:
            return Ok(msgspec.convert({"slope": correction.getSlope(), "offset": correction.getOffset(), "power": correction.getPower(), "saturation": correction.getSat()}, type=cls))
        except msgspec.ValidationError as error:
            return Error((FileFault(access="read", request=path, error=error),))

    @property
    def correction(self) -> PyOpenColorIO.CDLTransform:
        """Fresh OCIO CDL operation of the grade."""
        return PyOpenColorIO.CDLTransform(slope=list(self.slope), offset=list(self.offset), power=list(self.power), sat=self.saturation)

    @override
    def transform(self, _config: PyOpenColorIO.Config, /) -> ImagingResult[Option[PyOpenColorIO.Transform]]:
        """Returns the CDL operation."""
        return Ok(Some(self.correction))


class Imported(Named, frozen=True, kw_only=True):
    """ASC correction chosen in the file browser."""

    correction: Annotated[
        str,
        msgspec.Meta(title="Correction"),
        Choices(
            lambda _identity, owner, _context: (
                (str(position), name.default_value(str(position + 1)), "")
                for path in (owner.path_resolve("filepath"),)
                if isinstance(path, str)
                for rows in loaded(path).to_option().to_list()
                for position, (name, _grade) in enumerate(rows)
            )
        ),
    ] = "0"


class Exported(Formatted, frozen=True, kw_only=True):
    """Correction written in an exact transform format."""

    formats = staticmethod(LutFormat.writable)
    name: EffectName
    format: Annotated[str, msgspec.Meta(title="Format"), writable]
    source: Source = Source.CORRECTION


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GRAPH]


def corrected(shell: Shell[Cdl]) -> BlenderResult[Expr]:
    """Binds the grade's inputs to the node form of OCIO's CDL."""
    slope, offset, power, saturation = map(shell.input, Cdl.__struct_fields__)
    return Ok(grades.cdl(shell.color, slope, offset, power, saturation))


# --- [EXCHANGE]


def loaded(path: str) -> BlenderResult[tuple[tuple[Option[str], Cdl], ...]]:
    """Reads every correction of an ASC file in file order, each with its ID when named, after refreshing changed-file caches."""
    return current(path, PyOpenColorIO.GetCurrentConfig()).bind(corrections).bind(lambda found: accumulated(Cdl.of(path, correction).map(lambda grade: (name, grade)) for name, correction in found))


def imported(call: Call[Imported]) -> BlenderResult[Outcome]:
    """Writes the chosen correction into the named entry, keeping its blend and masks."""
    path, record = str(call.paths[0]), call.record

    def chosen(rows: tuple[tuple[Option[str], Cdl], ...]) -> BlenderResult[Cdl]:
        return Option.of_optional(next((grade for position, (_name, grade) in enumerate(rows) if str(position) == record.correction), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=path, key=record.correction),)
        )

    entry = call.root(bpy.types.Scene).bind(lambda scene: cdl.named(call.identity, scene, record.name).map(lambda held: (scene, held)))
    return (
        paired(loaded(path).bind(chosen), entry)
        .bind(lambda pair: revised(cdl, call.identity, *pair[1], lambda _: Ok(pair[0])))
        .map(lambda changed: Done() if changed else Unchanged(reason=f"{record.name} holds the chosen correction"))
    )


def exported(call: Call[Exported]) -> BlenderResult[Done]:
    """Writes the entry's correction alone under its entry name, or resolved between scene-linear conversions through the live config."""
    record = call.record

    def operations(grade: Cdl) -> BlenderResult[PyOpenColorIO.GroupTransform]:
        correction = grade.correction
        match record.source:
            case Source.CORRECTION:
                correction.getFormatMetadata().setID(record.name)
                return Ok(PyOpenColorIO.GroupTransform([correction]))
            case Source.SCENE_LINEAR:
                return config_of((Cdl.space,)).bind(lambda config: processor(config, bracketed(Cdl.space, correction))).map(PyOpenColorIO.Processor.createGroupTransform)

    held = call.root(bpy.types.Scene).bind(lambda scene: cdl.named(call.identity, scene, record.name)).bind(lambda entry: cdl.read(call.identity, entry))
    return paired(held, record.output()).bind(lambda pair: operations(pair[0].record).bind(lambda built: pair[1].written({str(call.paths[0]): built}))).map(Done.wrote)


# --- [COMPOSITION] ----------------------------------------------------------------------

importer = Operator(
    key="import_cdl",
    title="Import ASC CDL",
    description="Read an ASC correction into the entry",
    form=Imported,
    run=imported,
    invoke=Some(Open(extensions=lambda _record: tuple(dict.fromkeys(f".{row.extension}" for row in LutFormat.readable())))),
)
exporter = Operator(
    key="export_cdl",
    title="Export ASC CDL",
    description="Write the entry's correction in the chosen format",
    form=Exported,
    run=exported,
    invoke=Some(Save[Exported](extensions=Exported.extensions)),
    seed=Some(Exported.seeded),
    undo=False,
)
cdl = CompositorEffect(record=Cdl, graph=corrected, alpha=Alpha.STRAIGHT, parts=(Spaces(space=lambda _record: Cdl.space), Registers(rows=(importer, exporter))))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Cdl", "cdl"]
