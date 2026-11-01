"""Darkroom LUTs: the chain's render-enabled pointwise entries sampled through the compositor into a LUT file, a tone entry sampled into an installed view, and strip modifier bakes."""

from contextlib import nullcontext
from functools import partial
from typing import Any, Final

from bl_ui.space_topbar import TOPBAR_MT_file_export
import bpy
from expression import Nothing, Ok, Option, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.invocations import Popup, Save
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Notes, Operator
from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.rna import conditions
from blender.bpy.data.rna.mirrors import Callbacks, held_choices, IconedItem
from blender.bpy.data.rna.paths import rooted, Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.render.color.luts import Baking, extension, LatticeFields, LutExport, output, spaced
from blender.bpy.render.color.spaces import ConfigInstall, ConfigView, viewed
from blender.bpy.render.compositor.entries import Owned
from blender.bpy.render.previews.scenes import PreviewScene
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.surfaces.menus import Append
from blender.records.faults import Fault, Outcome, paired

from ..catalog import chain, tones
from ..effects.formation.display import formation

# --- [MODELS] ---------------------------------------------------------------------------


class ToneView(LatticeFields):
    """Tone entry sampled into a named installed view, over the sampled lattice."""

    entry: bpy.props.EnumProperty(name="Effect", description="Tone entry to sample, the first in stack order by default", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    view: bpy.props.StringProperty(name="View", description="Name of the installed view")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    description: bpy.props.StringProperty(name="Description", description="Description of the installed view")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SAMPLING]


def selected(identity: Identity, scene: bpy.types.Scene, kinds: Option[frozenset[Key]]) -> tuple[tuple[bpy.types.SceneCompositorEffect, ...], tuple[Fault, ...]]:
    """Returns the render-enabled entries of `kinds`, every entry while unset, that read each pixel alone under a plain mix, and the faults of those left out, pinned entries skipped."""
    pinned = frozenset(held.ownership.key for held in chain.pinned)

    def wanted(entry: bpy.types.SceneCompositorEffect) -> bool:
        unpinned = Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group)).filter(lambda mark: mark.key in pinned).is_none()
        return unpinned and kinds.map(lambda held: chain.row(identity, entry).filter(lambda row: row.kind in held).is_some()).default_value(True)

    checked = tuple((entry, chain.pointwise(identity, entry)) for entry in scene.compositor_effects if entry.enable_for_render and wanted(entry))
    return tuple(entry for entry, read in checked if read.is_ok()), tuple(fault for _, read in checked for faults in read.swap().to_option().to_list() for fault in faults)


def formed(identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], plane: npt.NDArray[np.float32]) -> Outcome[npt.NDArray[np.float32]]:
    """Samples a plane through the entries in the compositor, closed through the scene view while no formation entry forms it."""
    sampled = PreviewScene().sampled(identity, scene, entries, plane)
    closed = any(chain.row(identity, entry).filter(lambda row: row.kind == formation.kind).is_some() for entry in entries)
    return sampled if closed else sampled.bind(lambda values: viewed(scene).form(PyOpenColorIO.GetCurrentConfig(), values))


# --- [COMMANDS]


@Operator.of(Baking, condition=Some(conditions.Job(kind="RENDER")), invoke=Some(Save[Baking](extensions=extension)), undo=Nothing)
def bake_chain(call: Call[Baking]) -> Outcome[Answer]:
    """Bake the render-enabled pointwise chain entries into a LUT sampled through the compositor."""
    lattice = spaced(call.properties)

    def baked(scene: bpy.types.Scene) -> Outcome[Done]:
        entries, left = selected(call.identity, scene, Nothing)
        notes: Notes = tuple(("WARNING", fault.message) for fault in left)
        return (
            lattice
            .samples()
            .bind(lambda plane: formed(call.identity, scene, entries, plane))
            .bind(lambda values: output(call.properties).written({call.paths[0]: lattice.transform(values)}))
            .map(lambda paths: Done(notes=(*Done.wrote(paths).notes, *notes)))
        )

    return rooted(bpy.types.Scene, call.context).bind(baked)


def tone_entries(identity: Identity, scene: bpy.types.Scene) -> tuple[Owned[Any], ...]:
    """Returns the scene's tone entries in stack order."""
    members = frozenset(row.kind for row in tones.members)
    return tuple(held for held in chain.owned(identity, scene) if held.row.kind in members)


def offered(identity: Identity, _owner: Struct, context: bpy.types.Context | None) -> tuple[IconedItem, ...]:
    """Item rows of the context scene's tone entries, each titled by its effect, the first the default."""
    scenes = Option.of_optional(context).bind(lambda held: rooted(bpy.types.Scene, held).to_option()).to_list()
    return held_choices(tuple((held.entry.name, held.entry.name, held.row.title, 0, index) for scene in scenes for index, held in enumerate(tone_entries(identity, scene))))


# --- [COMPOSITION] ----------------------------------------------------------------------

TONE_VIEW: Final = Attachment(key="tone_view", form=ToneView, owner=bpy.types.WindowManager, callbacks=Some(lambda identity: nullcontext({"entry": Callbacks(items=partial(offered, identity))})))


@Operator.of(Empty, condition=Some(conditions.Job(kind="RENDER")), invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), draw=Some(TONE_VIEW.fields), undo=Nothing)
def install_tone_view(call: Call[Empty]) -> Outcome[Answer]:
    """Install a sampled tone entry as a view read on restart."""

    def installed(scene: bpy.types.Scene, settings: ToneView) -> Outcome[Answer]:
        lattice = spaced(settings)
        return (
            chain
            .found(call.identity, scene, settings.entry)
            .bind(lambda held: chain.pointwise(call.identity, held.entry).map(lambda _: held.entry))
            .bind(lambda entry: lattice.samples().bind(lambda plane: PreviewScene().sampled(call.identity, scene, (entry,), plane)))
            .bind(lambda values: ConfigInstall(views=(ConfigView(name=settings.view, description=settings.description, transform=lambda _base: Ok(lattice.transform(values))),)).written())
            .map(lambda path: Done(notes=(("INFO", f"Installed view {settings.view} into {path}, restart Blender to read it"),)))
        )

    settings = rooted(bpy.types.WindowManager, call.context).bind(lambda manager: TONE_VIEW.read(call.identity, manager))
    return paired(rooted(bpy.types.Scene, call.context), settings).bind(lambda parts: installed(*parts))


luts: Final = LutExport()
rows: Final = (luts, TONE_VIEW, Append(target=TOPBAR_MT_file_export, rows=(OperatorButton(target=bake_chain), OperatorButton(target=luts.stripping))))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["TONE_VIEW", "ToneView", "bake_chain", "formed", "install_tone_view", "luts", "offered", "rows", "selected", "tone_entries"]
