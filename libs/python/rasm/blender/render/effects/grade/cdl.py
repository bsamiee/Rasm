# mypy: disable-error-code="attr-defined"
# ty: ignore[unresolved-attribute]
"""Native slope, offset, power, and saturation with editable ASC file reads."""

from typing import Annotated

import bpy
from expression import Option, Result, Some
import msgspec
import PyOpenColorIO

from rasm.blender.commands.operators import Call, Done, Open, Operator, Save
from rasm.blender.faults import BlenderAbsentFault, BlenderFaults, BlenderImagingFault, BlenderResult
from rasm.blender.nodes.graphs import End, ended, Fragment, INPUTS, item_key, node, vector
from rasm.blender.render.color.luts import Formatted, lut_files, writable
from rasm.blender.render.effects.stack import CompositorEffect, EffectName, Named, Shell
from rasm.blender.settings.properties import Choices
from rasm.imaging.color.config import admitted
from rasm.imaging.color.luts import Exact, LutFormat, LutOutput, written
from rasm.imaging.color.processors import bracketed
from rasm.imaging.effects.grade.cdl import Cdl, Channel, read
from rasm.imaging.faults import ImagingResult, paired
from rasm.imaging.parameters import Choice

# --- [TYPES] ----------------------------------------------------------------------------


class Source(Choice):
    """Input values of the exported correction."""

    CORRECTION = 1, "Correction"
    SCENE_LINEAR = 2, "Scene Linear"


# --- [CHOICES] --------------------------------------------------------------------------

corrections = Choices(
    lambda _identity, owner, _context: (
        (str(position), name.default_value(str(position + 1)), "") for rows in loaded(owner.filepath).to_option().to_list() for position, (name, _record) in enumerate(rows)
    )
)


# --- [MODELS] ---------------------------------------------------------------------------


class Imported(Named, frozen=True, kw_only=True):
    """Editable ASC correction selected from the native file browser."""

    correction: Annotated[str, msgspec.Meta(title="Correction"), corrections] = "0"


class Exported(Formatted, frozen=True, kw_only=True):
    """Editable correction written through a native exact transform format."""

    formats = staticmethod(lambda: LutFormat.of(PyOpenColorIO.GroupTransform.GetWriteFormats()))
    name: EffectName
    format: Annotated[str, msgspec.Meta(title="Format"), writable]
    source: Source = Source.CORRECTION

    def output(self) -> BlenderResult[LutOutput]:
        """Returns the requested exact native writer."""
        return self.chosen().map(lambda row: Exact(format=row))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GRAPH]


def corrected(shell: Shell[Cdl]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns the ASC equation with native CDL luminance coefficients."""
    channels = (("X", Cdl.red.__name__), ("Y", Cdl.green.__name__), ("Z", Cdl.blue.__name__))
    members = tuple(field.name for field in msgspec.structs.fields(Channel))
    slope, offset, power = (End(node=member, socket="Vector") for member in (Channel.slope.__name__, Channel.offset.__name__, Channel.power.__name__))
    entering = shell.source
    sloped, positive, negative, powered, graded, chroma = (End(node=key, socket="Vector") for key in ("sloped", "positive", "negative", "powered", "graded", "chroma"))
    luma = End(node="luma")
    return Fragment.joined(
        *(node(member, bpy.types.ShaderNodeCombineXYZ, {axis: End(node=INPUTS, socket=item_key(channel, member)) for axis, channel in channels}, {}) for member in members),
        vector(sloped.node, "MULTIPLY_ADD", entering, slope, offset),
        vector(positive.node, "MAXIMUM", sloped, (0.0, 0.0, 0.0)),
        vector(negative.node, "MINIMUM", sloped, (0.0, 0.0, 0.0)),
        vector(powered.node, "POWER", positive, power),
        vector(graded.node, "ADD", powered, negative),
        vector(luma.node, "DOT_PRODUCT", graded, tuple(PyOpenColorIO.CDLTransform().getSatLumaCoefs())),
        vector(chroma.node, "SUBTRACT", graded, luma),
        vector("saturated", "MULTIPLY_ADD", chroma, End(node=INPUTS, socket=item_key(Cdl.saturation.__name__)), luma),
    ).map(ended("saturated", "Vector"))


# --- [EXCHANGE]


def loaded(path: str) -> BlenderResult[tuple[tuple[Option[str], Cdl], ...]]:
    """Returns editable corrections after refreshing native LUT caches.

    Args:
        path: Absolute ASC CDL, CC, or CCC path.

    Returns:
        Ordered optional identifiers and corrections, or file and native ASC faults.
    """
    return lut_files.current(path, PyOpenColorIO.GetCurrentConfig()).bind(lambda current: read(current).map_error(lambda faults: (BlenderImagingFault(faults=faults),)))


def imported(call: Call[Imported]) -> BlenderResult[Done]:
    """Writes the selected editable correction while retaining blend and masks."""
    path, record = str(call.paths[0]), call.record

    def chosen(rows: tuple[tuple[Option[str], Cdl], ...]) -> BlenderResult[Cdl]:
        return Option.of_optional(next((correction for position, (_name, correction) in enumerate(rows) if str(position) == record.correction), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=path, key=record.correction),)
        )

    scene: BlenderResult[bpy.types.Scene] = Option.of_optional(call.context.scene).to_result_with(lambda: (BlenderAbsentFault(owner=repr(call.context), key="scene"),))
    entry = Result[bpy.types.Scene, BlenderFaults].bind(scene, lambda scene: cdl.named(call.identity, scene, record.name))
    return (
        paired(loaded(path).bind(chosen), entry)
        .bind(
            lambda pair: cdl.read(call.identity, pair[1]).bind(lambda held: cdl.write(call.identity, pair[1].id_data, pair[1], msgspec.structs.replace(held, effect=pair[0])))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        )
        .map(lambda _: Done())
    )


def exported(call: Call[Exported]) -> BlenderResult[Done]:
    """Writes the correction alone or its full scene-linear bracket."""
    record = call.record

    def source(correction: Cdl) -> ImagingResult[PyOpenColorIO.GroupTransform]:
        match record.source:
            case Source.CORRECTION:
                config = PyOpenColorIO.Config()

                def named(operation: Option[PyOpenColorIO.CDLTransform]) -> PyOpenColorIO.GroupTransform:
                    for op in operation.to_list():
                        op.getFormatMetadata().setID(record.name)
                    return PyOpenColorIO.GroupTransform(operation.to_list())

                return correction.transform(config).map(named)
            case Source.SCENE_LINEAR:
                return admitted(PyOpenColorIO.GetCurrentConfig(), (Cdl.space,)).bind(
                    lambda config: bracketed(correction, config).map(lambda operation: PyOpenColorIO.GroupTransform(operation.to_list()))
                )

    scene: BlenderResult[bpy.types.Scene] = Option.of_optional(call.context.scene).to_result_with(lambda: (BlenderAbsentFault(owner=repr(call.context), key="scene"),))
    held = Result[bpy.types.Scene, BlenderFaults].bind(scene, lambda scene: cdl.named(call.identity, scene, record.name)).bind(lambda entry: cdl.read(call.identity, entry))
    output = paired(held, record.output()).bind(
        lambda pair: source(pair[0].effect).bind(lambda built: written(pair[1], {str(call.paths[0]): built})).map_error(lambda faults: (BlenderImagingFault(faults=faults),))
    )
    return output.map(lambda paths: Done(notes=tuple(("INFO", f"Wrote {path}") for path in paths)))


# --- [COMPOSITION] ----------------------------------------------------------------------

importer = Operator(
    key="import_cdl",
    title="Import ASC CDL",
    description="Reads an editable correction into the entry",
    form=Imported,
    run=imported,
    invoke=Some(Open(extensions=lambda _record: tuple(dict.fromkeys(f".{row.extension}" for row in LutFormat.of(PyOpenColorIO.FileTransform.getFormats()))))),
)
exporter = Operator(
    key="export_cdl",
    title="Export ASC CDL",
    description="Writes the entry's correction through a native format",
    form=Exported,
    run=exported,
    invoke=Some(Save[Exported](extensions=Exported.extensions)),
    seed=Some(Exported.seeded),
    undo=False,
)
cdl = CompositorEffect(record=Cdl, graph=corrected, straight=True, registrables=(importer, exporter))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Exported", "Imported", "Source", "cdl", "corrected", "corrections", "exported", "exporter", "imported", "importer", "loaded"]
