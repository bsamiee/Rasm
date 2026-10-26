"""Darkroom LUT exports: the chain baked as one transform or sampled through its enabled entries, strip modifier bakes, and a sampled tone entry installed as a view read on restart."""

from typing import Final

from bl_ui.space_topbar import TOPBAR_MT_file_export
import bpy
from expression import Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Answer, Call, Done, Operator, Properties, Save
from blender.bpy.data.rna.conditions import Job
from blender.bpy.extension.identity import Identity
from blender.bpy.render.color.luts import Baking, extension, LutExport, output, spaced, SPACINGS
from blender.bpy.render.color.spaces import ConfigView
from blender.bpy.render.compositor.effects import Enabled
from blender.bpy.render.previews.scenes import PreviewScene
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.surfaces.appends import Append
from blender.imaging.color.luts import Lattice
from blender.records.faults import Outcome

from ..catalog import chain
from ..chain.stages import Stage
from .films import install

# --- [MODELS] ---------------------------------------------------------------------------


class ToneView(bpy.types.PropertyGroup):
    """Tone entry sampled into a named installed view."""

    entry: bpy.props.StringProperty(name="Effect", description="Pointwise entry to sample, the first tone entry by default")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    view: bpy.props.StringProperty(name="View", description="Name of the installed view")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    description: bpy.props.StringProperty(name="Description", description="Description of the installed view")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    size: bpy.props.IntProperty(name="Size", description="Nodes along each axis", min=2, max=129, default=Lattice().size)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    spacing: bpy.props.EnumProperty(name="Spacing", description="Encoding before table", items=tuple((name, name, "") for name in SPACINGS), default=type(Lattice().spacing).__name__)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------


def toned(identity: Identity, scene: bpy.types.Scene, entries: tuple[bpy.types.SceneCompositorEffect, ...], lattice: Lattice) -> Outcome[PyOpenColorIO.GroupTransform]:
    """Samples every RGB channel through entries over the lattice domain."""
    samples: Outcome[npt.NDArray[np.float32]] = lattice.samples()
    return samples.bind(lambda plane: PreviewScene().sampled(identity, scene, entries, plane)).map(lattice.transform)


def tone_entries(identity: Identity, scene: bpy.types.Scene) -> tuple[bpy.types.SceneCompositorEffect, ...]:
    """Returns the scene's tone entries in stack order."""
    return tuple(held.entry for held in chain.rows.owned(identity, scene) if held.row.record.stage is Stage.TONE)


def bake_formation(call: Call[Baking]) -> Outcome[Done]:
    """Bake the enabled chain into a native LUT, exactly where its entries form one OCIO operation, else sampled through them."""
    built = output(call.properties)

    def baked(scene: bpy.types.Scene) -> Outcome[PyOpenColorIO.GroupTransform]:
        def sampled() -> Outcome[PyOpenColorIO.GroupTransform]:
            return toned(call.identity, scene, tuple(entry for entry in scene.compositor_effects if bool(Enabled.RENDER.descriptor.read(entry))), spaced(call.properties))

        return chain.forming(call.identity, scene, Enabled.RENDER).bind(
            lambda stated: stated.map(lambda transform: Ok(PyOpenColorIO.GroupTransform([transform]))).default_with(sampled) if built.lattice.is_none() else sampled()
        )

    return call.root(bpy.types.Scene).bind(baked).bind(lambda transform: built.written({str(call.paths[0]): transform})).map(Done.wrote)


def install_tone_view(call: Call[ToneView]) -> Outcome[Answer]:
    """Install a sampled pointwise entry as a view read on restart."""
    properties = call.properties
    lattice = Lattice(size=properties.size, spacing=SPACINGS[properties.spacing]())

    def sampled(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> Outcome[Answer]:
        return (
            chain
            .pointwise(call.identity, scene, entry)
            .bind(lambda _: toned(call.identity, scene, (entry,), lattice))
            .bind(
                lambda table: msgspec.structs.replace(
                    install, views=(*install.views, ConfigView(name=properties.view, description=properties.description, transform=lambda _base: Ok(table)))
                ).written()
            )
            .map(lambda path: Done(notes=(("INFO", f"Installed view {properties.view} into {path}, restart Blender to read it"),)))
        )

    return call.root(bpy.types.Scene).bind(lambda scene: chain.rows.found(call.identity, scene, properties.entry).bind(lambda held: sampled(scene, held.entry)))


# --- [COMPOSITION] ----------------------------------------------------------------------

luts: Final = LutExport()
forming: Final = Operator(form=Baking, run=bake_formation, condition=Some(Job(kind="RENDER")), invoke=Some(Save[Baking](extensions=extension)), undo=False)
toning: Final = Operator(
    form=ToneView,
    run=install_tone_view,
    condition=Some(Job(kind="RENDER")),
    invoke=Some(Properties()),
    seed=Some(
        lambda _properties, _event, identity: (
            Option.of_optional(bpy.context.scene).bind(lambda scene: Option.of_optional(next(iter(tone_entries(identity, scene)), None))).map(lambda entry: {"entry": entry.name}).default_value({})
        )
    ),
    undo=False,
)
rows: Final = (luts, toning, Append(target=TOPBAR_MT_file_export, rows=(OperatorButton(target=forming), OperatorButton(target=luts.stripping))))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ToneView", "bake_formation", "forming", "install_tone_view", "luts", "rows", "tone_entries", "toned", "toning"]
