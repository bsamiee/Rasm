"""Display formation row: the Display View entry, scene views it reads and writes, and the formation a stage's first entry opens with."""

from collections.abc import Callable
from typing import ClassVar, Final, override, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator, Outcome, Unchanged
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.changes import owned
from blender.bpy.nodes.graphs import Expr
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.effects.rows import CompositorEffect, editable, Named, Restoring, revised, Shell, stops
from blender.bpy.render.outputs.formats import owners
from blender.imaging.chain import Domain, Effect, Stage
from blender.imaging.color.processors import DisplayView
from blender.imaging.compositing.blending import Entry
from blender.imaging.effects.tone.exposure import Exposure
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Gated

if TYPE_CHECKING:
    from blender.bpy.render.effects.stack import EffectStack

# --- [TYPES] ----------------------------------------------------------------------------

type Seeding = Callable[[PyOpenColorIO.Config, bpy.types.Scene], BlenderResult[DisplayView]]

# --- [CONSTANTS] ------------------------------------------------------------------------

NONE: Final = "None"

# --- [MODELS] ---------------------------------------------------------------------------


class FormationEffect(CompositorEffect[DisplayView], frozen=True, kw_only=True):
    """Display View row: unblended, folding scene exposure into the gain row, mirroring its view to overriding outputs and back to the scene."""

    blended: ClassVar[bool] = False

    @override
    def placing[E: Effect](self, identity: Identity, scene: bpy.types.Scene, stack: "EffectStack[E]", /) -> BlenderResult[None]:
        """Folds scene view exposure into the stack's gain row, placing one when absent."""
        exposure = scene.view_settings.exposure  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]

        def folded(gain: CompositorEffect[Exposure]) -> BlenderResult[None]:
            placed = Ok(None) if gain.entries(identity, scene) else gain.seeded(scene).bind(lambda effect: stack.placed(identity, scene, gain, Entry(effect=effect))).map(lambda _: None)
            return (
                placed
                .bind(lambda _: Option.of_optional(next(iter(gain.entries(identity, scene)), None)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=gain.kind),)))
                .bind(lambda entry: revised(gain, identity, scene, entry, lambda record: Ok(msgspec.structs.replace(record, stops=record.stops + exposure))))
                .bind(lambda _: written(scene.view_settings, "exposure", 0.0))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                .map(lambda _: None)
            )

        return stack.gain.map(folded).default_value(Ok(None)) if exposure else Ok(None)

    @override
    def holding(self, _identity: Identity, scene: bpy.types.Scene, /) -> BlenderResult[None]:
        """Applies the formation's holds to every output overriding color management."""
        return accumulated(
            Snapshot.of(bpy.types.ColorManagedViewSettings, {path.removeprefix("view_settings."): value for path, value in self.holds.items()}).apply(owner.view_settings).outcome  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            for owner in owners(scene)
            if owner.color_management == "OVERRIDE"
        ).map(lambda _: None)

    @override
    def removing(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, /) -> BlenderResult[Restoring]:
        """Reads the entry's view, restored to the scene and overriding outputs once the entry and its holds are gone."""
        return self.read(identity, entry).map(lambda record: lambda: displayed(scene, record.effect))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [VIEWS]


def viewed(holder: bpy.types.Scene | bpy.types.CompositorNodeConvertToDisplay) -> DisplayView:
    """Reads Display View from scene or display node, look `NONE` as disabled."""
    look = holder.view_settings.look  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    return DisplayView(display=holder.display_settings.display_device, view=holder.view_settings.view_transform, look=Gated(enabled=look != NONE, value=look))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


def formed(shell: Shell[DisplayView]) -> BlenderResult[Expr]:
    """Builds Convert to Display node keyed `display`, writing display before view and look."""
    record = shell.record
    return Ok(
        Expr(
            kind=bpy.types.CompositorNodeConvertToDisplay,
            inputs=(("Image", shell.color),),
            members=(("display_settings.display_device", record.display), ("view_settings.view_transform", record.view), ("view_settings.look", record.look.value if record.look.enabled else NONE)),
            socket="Image",
            shape="RGBA",
        ).named("display")
    )


def displayed(scene: bpy.types.Scene, view: DisplayView) -> BlenderResult[None]:
    """Writes display, view, and look to scene and every output overriding color management."""

    def written_view(holder: bpy.types.Scene | bpy.types.ImageFormatSettings) -> BlenderResult[None]:
        return (
            written(holder.display_settings, "display_device", view.display)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            .bind(lambda _: written(holder.view_settings, "view_transform", view.view))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            .bind(lambda _: written(holder.view_settings, "look", view.look.value if view.look.enabled else NONE))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            .map(lambda _: None)
        )

    return paired(written_view(scene), accumulated(written_view(owner) for owner in owners(scene) if owner.color_management == "OVERRIDE")).map(lambda _: None)


def displaying(identity: Identity, scene: bpy.types.Scene) -> Option[tuple[bpy.types.SceneCompositorEffect, bpy.types.CompositorNodeConvertToDisplay]]:
    """Returns the earliest owned formation with the display node it forms through."""
    return Option.of_optional(
        next(
            (
                (entry, node)
                for entry in formation.entries(identity, scene)
                for group in Option.of_optional(entry.node_group).to_list()
                for node in Option.of_optional(owned(group, identity).get("display")).to_list()
                if isinstance(node, bpy.types.CompositorNodeConvertToDisplay)
            ),
            None,
        )
    )


def followed(identity: Identity) -> BlenderResult[None]:
    """Writes changed scene display device to owned display nodes, storing Blender's matched view."""

    def updated(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, display: bpy.types.CompositorNodeConvertToDisplay) -> BlenderResult[bool]:
        return written(display.display_settings, "display_device", scene.display_settings.display_device).bind(  # type: ignore[arg-type, union-attr]  # ty: ignore[invalid-argument-type, unresolved-attribute]
            lambda _: matched(identity, scene, entry, display)
        )

    return accumulated(
        updated(scene, entry, node)
        for source in bpy.data.scenes
        for scene in editable(source).to_option().to_list()
        for entry, node in displaying(identity, scene).to_list()
        if viewed(node).display != viewed(scene).display
    ).map(lambda _: None)


def looked(identity: Identity, scene: bpy.types.Scene, look: str) -> BlenderResult[None]:
    """Writes a look into the earliest owned formation, storing Blender's matched record, else into the scene's view."""
    return (
        displaying(identity, scene)
        .map(
            lambda pair: written(pair[1].view_settings, "look", look).bind(lambda _: matched(identity, scene, *pair)).map(lambda _: None)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        )
        .default_with(lambda: written(scene.view_settings, "look", look).map(lambda _: None))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
    )


def matched(identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, display: bpy.types.CompositorNodeConvertToDisplay) -> BlenderResult[bool]:
    """Stores the view a display node holds after a host write as the entry's record."""
    return formation.read(identity, entry, edited=False).bind(lambda record: formation.write(identity, scene, entry, msgspec.structs.replace(record, effect=viewed(display))))


def managed(identity: Identity, stack: "EffectStack[Effect]", scene: bpy.types.Scene, plane: npt.NDArray[np.float32]) -> BlenderResult[npt.NDArray[np.float32]]:
    """Forms scene-linear pixels as scene color management shows them, the stack's gain and earliest formation standing in for the view exposure and transform they hold."""
    settings = scene.view_settings
    gain = stack.gain.map(lambda row: stops(identity, scene, row)).default_value(Ok(0.0))
    return paired(formation.current(identity, scene), gain, config_of()).bind(
        lambda parts: parts[0].default_with(lambda: viewed(scene)).form(parts[2], plane, exposure=parts[1] + settings.exposure, gamma=settings.gamma)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    )


def opening(record: type[Effect]) -> Option[Seeding]:
    """Formation a scene lacking one opens with for a record's first entry: an encoding-only view after tone, the scene view for display stages."""
    match record.stage:
        case Stage.TONE:
            return Some(encoding)
        case stage if stage.domain is Domain.DISPLAY:
            return Some(lambda _config, scene: formation.seeded(scene))
        case _:
            return Nothing


def encoding(config: PyOpenColorIO.Config, scene: bpy.types.Scene) -> BlenderResult[DisplayView]:
    """Returns the first encoding-only view of the scene display."""
    return Result[Option[tuple[DisplayView, PyOpenColorIO.Processor]], BlenderFaults].bind(
        DisplayView.for_encoding(config, viewed(scene).display),
        lambda view: view.map(lambda pair: pair[0]).to_result_with(lambda: (BlenderAbsentFault(owner=scene.name, key=DisplayView.for_encoding.__name__),)),
    )


def encoded(call: Call[Named]) -> BlenderResult[Outcome]:
    """Selects the first encoding-only view of the formation display and disables its look."""

    def chosen(scene: bpy.types.Scene, config: PyOpenColorIO.Config, held: Option[DisplayView]) -> BlenderResult[Outcome]:
        display = held.map(lambda record: record.display).default_value(viewed(scene).display)

        def selected(target: DisplayView) -> BlenderResult[Outcome]:
            return (
                Option
                .of_optional(next(iter(formation.entries(call.identity, scene)), None))
                .map(lambda entry: revised(formation, call.identity, scene, entry, lambda _: Ok(target)))
                .default_with(
                    lambda: written(scene, "view_settings.view_transform", target.view).bind(lambda view: written(scene, "view_settings.look", NONE).map(lambda look: view.is_some() or look.is_some()))
                )
                .map(lambda changed: Done() if changed else Unchanged(reason=f"Formation encodes through {target.view}"))
            )

        return (
            Result[Option[tuple[DisplayView, PyOpenColorIO.Processor]], BlenderFaults]
            .bind(
                DisplayView.for_encoding(config, display),
                lambda view: view.map(lambda pair: pair[0]).to_result_with(lambda: (BlenderAbsentFault(owner=display, key=DisplayView.for_encoding.__name__),)),
            )
            .bind(selected)
        )

    return call.root(bpy.types.Scene).bind(lambda scene: paired(config_of(), formation.current(call.identity, scene)).bind(lambda pair: chosen(scene, *pair)))


# --- [COMPOSITION] ----------------------------------------------------------------------

formation = FormationEffect(
    record=DisplayView,
    graph=formed,
    seed=Some(viewed),
    holds={"view_settings.view_transform": "Raw", "view_settings.exposure": 0.0, "view_settings.use_white_balance": False, "view_settings.use_curve_mapping": False},
)
encode_formation = Operator(key="encode_formation", title="Use Display Encoding", description="Select encoding-only formation after tone curve", form=Named, run=encoded)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "NONE",
    "FormationEffect",
    "Seeding",
    "displayed",
    "displaying",
    "encode_formation",
    "encoded",
    "encoding",
    "followed",
    "formation",
    "formed",
    "looked",
    "managed",
    "matched",
    "opening",
    "viewed",
]
