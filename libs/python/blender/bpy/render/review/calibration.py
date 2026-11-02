"""Chart calibration: a scene's pairing of a sampled source chart with reference values or a second image's chart, its fit, and the fit written as a shader matrix node or as forward and inverse LUT and transform files, beside the actions a consumer adds."""

from collections.abc import Callable
from contextlib import nullcontext
from enum import auto, Enum
from functools import cached_property, partial
import math
from pathlib import Path
from typing import ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import PyOpenColorIO

from blender.bpy.commands.invocations import Save
from blender.bpy.commands.operators import Answer, Call, Done, Empty, NoUndo, Operator
from blender.bpy.data.rna.conditions import AllOf, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.properties import Callbacks, choices, enum_choices
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.nodes.editing import inserted
from blender.bpy.render.color.luts import Baking, extension, FORMATS, output, Writing
from blender.bpy.render.review.charts import Need, PlacedChart
from blender.bpy.render.review.editing import ChartEditing
from blender.bpy.scene.materials import color_matrix
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import grouped, Layout, OperatorButton, Property, RowContent
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.bpy.ui.screens.areas import NodeEditing
from blender.imaging.color.calibration import Calibration, Colors
from blender.imaging.color.luts import LutOutput
from blender.records.faults import Fault, Faults, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Aim(Enum):
    """Colors samples map onto: published chart values in a config colorspace, or a second image's samples."""

    REFERENCE = auto()
    IMAGE = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_KEY: Final = "chart_calibration"
_SPACES: Final = choices(Descriptor.declared(bpy.types.ColorManagedInputColorspaceSettings, ("name",), bpy.types.EnumProperty).prop.enum_items)
_DIRECTIONS: Final = (PyOpenColorIO.TRANSFORM_DIR_FORWARD, PyOpenColorIO.TRANSFORM_DIR_INVERSE)

# --- [MODELS] ---------------------------------------------------------------------------


class Calibrating(bpy.types.PropertyGroup):
    """Scene's calibration pairing and fit choices, its image pointers choosing images holding sampled charts."""

    source: bpy.props.PointerProperty(type=bpy.types.Image, name="Source", description="Sampled source image")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    target: bpy.props.EnumProperty(name="Target", description="Colors samples map onto", items=enum_choices(Aim))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    space: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Space", description="Reference value colorspace", items=_SPACES, default=PyOpenColorIO.GetCurrentConfig().getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()
    )
    image: bpy.props.PointerProperty(type=bpy.types.Image, name="Image", description="Image source samples map onto")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    direction: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Direction",
        description="Source to target or back",
        items=choices((PyOpenColorIO.TransformDirectionToString(direction), bpy.path.display_name(PyOpenColorIO.TransformDirectionToString(direction)), "") for direction in _DIRECTIONS),
    )
    gain: bpy.props.BoolProperty(name="Keep Gain", description="Fold exposure gain into transform")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    normalized: bpy.props.BoolProperty(name="Normalize", description="Keep source neutral luma, gain held as exposure", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    clamped: bpy.props.BoolProperty(name="Clamp", description="Clip exported values to [0, 1]")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Calibrated(msgspec.Struct, frozen=True, kw_only=True):
    """Scene calibration choices, the calibration they select, and the fit it came from."""

    chosen: Calibrating
    calibration: Calibration
    fit: Calibration


# --- [SERVICES] -------------------------------------------------------------------------


class ChartCalibration(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Scene calibration over the sampled charts `charts` edits, its fit warned past `ceiling` condition, `actions` drawn beside its file commands, its scene values seeded by `seed`."""

    charts: ChartEditing
    actions: tuple[RowContent, ...] = ()
    ceiling: float = 1e4
    seed: Option[Callable[[Identity], Outcome[Snapshot]]] = Nothing
    shader: ClassVar[AllOf] = Setting(descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, ("tree_type",), bpy.types.EnumProperty), accepted=(bpy.types.ShaderNodeTree.__name__,)) & Setting(
        descriptor=Descriptor.declared(bpy.types.SpaceNodeEditor, ("edit_tree", "is_editable"), bpy.types.BoolProperty)
    )

    @cached_property
    def calibrations(self) -> Attachment[Calibrating]:
        """Scene calibration choices, seeded by `seed`, its image pointers choosing sampled charts."""
        return Attachment(
            key="calibration",
            form=Calibrating,
            owner=bpy.types.Scene,
            seed=self.seed,
            callbacks=Some(lambda identity: nullcontext({name: Callbacks(poll=partial(self.charts.charting.sampled, identity)) for name in ("source", "image")})),
        )

    # --- [FITS]

    def conditioned(self, fit: Calibration) -> tuple[tuple["rna_enums.WmReportItems", str], ...]:
        """Warns of a fit conditioned past the ceiling."""
        return (("WARNING", f"Fitted transform poorly conditioned ({fit.condition:.2e}), check chart alignment"),) if fit.condition > self.ceiling else ()

    def calibrated(self, identity: Identity, chosen: Calibrating) -> Outcome[Calibrated]:
        """Returns the chosen direction's calibration, normalized and without gain as chosen, with the choices and the fit it came from."""
        config, target = PyOpenColorIO.GetCurrentConfig(), Aim[chosen.target]
        forward = PyOpenColorIO.TransformDirectionFromString(chosen.direction) == PyOpenColorIO.TRANSFORM_DIR_FORWARD

        def held(image: bpy.types.Image | None, role: str) -> Outcome[tuple[bpy.types.Image, PlacedChart]]:
            return Option.of_optional(image).to_result_with(lambda: Fault.of(Need.CHOSEN, role)).bind(lambda found: self.charts.charting.record(identity, found).map(lambda record: (found, record)))

        def samples(pair: tuple[bpy.types.Image, PlacedChart]) -> Outcome[Colors]:
            return pair[1].sampled.to_result_with(lambda: Fault.of(Need.SAMPLED, pair[0].name))

        def aimed(source: PlacedChart) -> Outcome[Colors]:
            match target:
                case Aim.REFERENCE:
                    return source.placement.chart.values(config, chosen.space)
                case Aim.IMAGE:
                    return (
                        held(chosen.image, "target")
                        .filter_with(
                            lambda pair: pair[1].placement.chart is source.placement.chart, lambda pair: Fault.of(Need.MATCHED, f"{source.placement.chart.name}, got {pair[1].placement.chart.name}")
                        )
                        .bind(samples)
                    )

        def fitted(image: bpy.types.Image, record: PlacedChart, source: Colors, goal: Colors) -> Outcome[Calibrated]:
            neutral, linear = record.placement.chart.neutral, PyOpenColorIO.ROLE_SCENE_LINEAR
            aimed_space = chosen.space if target is Aim.REFERENCE else linear

            def selected(fit: Calibration) -> Outcome[Calibration]:
                calibration, first, last, source_space, target_space = (
                    (fit, source[neutral], goal[neutral], linear, aimed_space) if forward else (fit.inverted, goal[neutral], source[neutral], aimed_space, linear)
                )
                normalized = (
                    calibration.normalized(first, last, config, source_space=source_space, target_space=target_space)
                    if chosen.normalized
                    else Result[Option[Calibration], Faults].Ok(Some(calibration))
                )
                return normalized.bind(lambda found: found.to_result_with(lambda: Fault.of(Need.LIT, image.name))).map(
                    lambda kept: kept if chosen.gain else msgspec.convert({"color_matrix": kept.matrix.tolist()}, type(kept))
                )

            return Calibration.fit(tuple(zip(source, goal, strict=True))).bind(lambda fit: selected(fit).map(lambda calibration: Calibrated(chosen=chosen, calibration=calibration, fit=fit)))

        return held(chosen.source, "source").bind(lambda pair: paired(samples(pair), aimed(pair[1])).bind(lambda rows: fitted(*pair, *rows)))

    def fitting[T: bpy.types.PropertyGroup](self, call: Call[T]) -> Outcome[Calibrated]:
        """Returns the calling scene's calibration choices with the calibration they fit and its fit."""
        return rooted(bpy.types.Scene, call.context).bind(lambda scene: self.calibrations.read(call.identity, scene)).bind(partial(self.calibrated, call.identity))

    # --- [COMMANDS]

    def insert_calibration(self, call: Call[Empty]) -> Outcome[Answer]:
        """Insert calibration matrix after active node."""

        def spliced(chosen: NodeEditing, made: Calibrated) -> Outcome[Answer]:
            calibration = made.calibration
            rows = dict(zip(("red", "green", "blue"), map(tuple, calibration.matrix.tolist()), strict=True))
            return inserted(chosen.tree, chosen.active, chosen.cursor, color_matrix.group, call.identity, inputs={**rows, "gain": math.exp2(calibration.exposure)}).map(
                lambda _: Done(notes=(*self.conditioned(made.fit), ("INFO", "Inserted calibration matrix")))
            )

        return self.fitting(call).bind(lambda made: NodeEditing.of(call.context).bind(lambda chosen: spliced(chosen, made)))

    def filed[T: Baking | Writing](self, written: LutOutput, call: Call[T]) -> Outcome[Answer]:
        """Writes the chosen direction's calibration to its direction's destination and its inverse to the other through an output, clamped to the unit range when chosen."""

        def exported(chosen: Calibrating, calibration: Calibration) -> Outcome[tuple[Path, ...]]:
            clamp = (PyOpenColorIO.RangeTransform(minInValue=0.0, maxInValue=1.0, minOutValue=0.0, maxOutValue=1.0),) if chosen.clamped else ()
            wanted = PyOpenColorIO.TransformDirectionFromString(chosen.direction)
            return written.written({
                destination: PyOpenColorIO.GroupTransform(transforms=[PyOpenColorIO.GroupTransform([calibration.transform], direction=_DIRECTIONS[0] if each == wanted else _DIRECTIONS[1]), *clamp])
                for each, destination in zip(_DIRECTIONS, call.paths, strict=True)
            })

        return self.fitting(call).bind(
            lambda made: exported(made.chosen, made.calibration).map(lambda paths: Done(notes=(*self.conditioned(made.fit), *(("INFO", f"Wrote {path}") for path in paths))))
        )

    def bake_calibration_lut(self, call: Call[Baking]) -> Outcome[Answer]:
        """Bake forward and inverse calibration LUTs."""
        return self.filed(output(call.properties), call)

    def write_calibration(self, call: Call[Writing]) -> Outcome[Answer]:
        """Write forward and inverse calibration transform files."""
        return self.filed(LutOutput(format=FORMATS[call.properties.format]), call)

    @staticmethod
    def sided(path: Path, _properties: Baking | Writing) -> tuple[Path, ...]:
        """Returns the forward and inverse destinations beside a chosen path."""
        return tuple(path.with_stem(f"{path.stem}_{PyOpenColorIO.TransformDirectionToString(direction)}") for direction in _DIRECTIONS)

    @cached_property
    def inserting(self) -> Operator[Empty]:
        """Command splicing the calibration matrix after the active shader node."""
        return Operator.of(Empty, condition=Some(self.shader))(self.insert_calibration)

    @cached_property
    def baking(self) -> Operator[Baking]:
        """Command baking the forward and inverse calibration LUTs."""
        return Operator.of(Baking, invoke=Some(Save[Baking](extensions=extension, destinations=Some(self.sided))), undo=NoUndo())(self.bake_calibration_lut)

    @cached_property
    def writing(self) -> Operator[Writing]:
        """Command writing the forward and inverse calibration transform files."""
        return Operator.of(Writing, invoke=Some(Save[Writing](extensions=extension, destinations=Some(self.sided))), undo=NoUndo())(self.write_calibration)

    # --- [SECTIONS]

    def section(self) -> Section[Calibrating]:
        """Image editor calibration section: the shown image's chart, the consumer's actions and the file commands, then the scene's calibration fields."""
        return Section(
            key=_KEY,
            rows=(*self.charts.rows, Layout(rows=(*self.actions, OperatorButton(target=self.baking), OperatorButton(target=self.writing)), container=Some(grouped))),
            store=Some(Store(attachment=self.calibrations)),
        )

    def node_section(self) -> Section:
        """Node editor matrix splice section, inactive outside an editable shader tree."""
        return Section(
            key=f"{_KEY}_node",
            title=Some(bpy.path.display_name(_KEY)),
            rows=(*(Property(target=Member(root=self.calibrations, path=(name,))) for name in ("source", "direction")), OperatorButton(target=self.inserting)),
            condition=Some(self.shader),
        )

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Charts the calibration reads."""
        return (self.charts,)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the scene calibration, the matrix group, and the calibration commands."""
        return Ok((self.calibrations, color_matrix.group, self.inserting, self.baking, self.writing))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Aim", "Calibrated", "Calibrating", "ChartCalibration"]
