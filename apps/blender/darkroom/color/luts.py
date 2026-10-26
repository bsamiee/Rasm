"""Darkroom LUT exports: the chain baked as one transform, strip modifier bakes, and a sampled tone entry installed as a view read on restart."""

from functools import cached_property
from typing import Annotated, Final

from bl_ui.space_topbar import TOPBAR_MT_file_export
import bpy
from expression import Ok, Option, Some
import msgspec

from blender.bpy.commands.operators import Call, Done, Operator, Outcome, Properties
from blender.bpy.data.rna.conditions import Job
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.render.color.luts import LutExport
from blender.bpy.render.color.spaces import ConfigView
from blender.bpy.ui.layout.rows import OperatorButton
from blender.bpy.ui.surfaces.appends import Append
from blender.imaging.color.luts import Lattice

from ..catalog import chain
from ..chain.stages import Stage
from .films import install

# --- [MODELS] ---------------------------------------------------------------------------


class ToneView(msgspec.Struct, frozen=True, kw_only=True):
    """Tone entry sampled into a named installed view."""

    entry: Annotated[str, msgspec.Meta(min_length=1, title="Effect")]
    view: Annotated[str, msgspec.Meta(min_length=1, title="View")]
    description: str = ""
    lattice: Lattice = Lattice()


# --- [SERVICES] -------------------------------------------------------------------------


class ToneViews(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Tone entries of the chain sampled through `export` and installed beside the film views."""

    export: LutExport

    @staticmethod
    def entries(identity: Identity, scene: bpy.types.Scene) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns the scene's tone entries in stack order."""
        return tuple(held.entry for held in chain.rows.owned(identity, scene) if held.row.record.stage is Stage.TONE)

    def installed(self, call: Call[ToneView]) -> BlenderResult[Outcome]:
        """Installs a pointwise tone entry as a view read on restart."""
        record = call.record

        def sampled(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[Outcome]:
            return (
                chain
                .pointwise(call.identity, scene, entry)
                .bind(lambda _: self.export.toned(call.identity, scene, (entry,), record.lattice))
                .bind(lambda table: msgspec.structs.replace(install, views=(*install.views, ConfigView(name=record.view, description=record.description, transform=lambda _base: Ok(table)))).written())
                .map(lambda path: Done(notes=(("INFO", f"Installed view {record.view} into {path}, restart Blender to read it"),)))
            )

        def found(scene: bpy.types.Scene) -> BlenderResult[bpy.types.SceneCompositorEffect]:
            return Option.of_optional(next((entry for entry in self.entries(call.identity, scene) if entry.name == record.entry), None)).to_result_with(
                lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=record.entry),)
            )

        return call.root(bpy.types.Scene).bind(lambda scene: found(scene).bind(lambda entry: sampled(scene, entry)))

    @cached_property
    def toning(self) -> Operator[ToneView]:
        """Command installing a sampled tone entry, seeded with the scene's first tone entry."""
        return Operator(
            key="install_tone_view",
            title="Install Tone View",
            description="Install sampled tone entry as a view read on restart",
            form=ToneView,
            run=self.installed,
            condition=Some(Job(kind="RENDER")),
            invoke=Some(Properties()),
            seed=Some(
                lambda record, _event, identity: (
                    Option
                    .of_optional(bpy.context.scene)
                    .bind(lambda scene: Option.of_optional(next(iter(self.entries(identity, scene)), None)))
                    .map(lambda entry: msgspec.structs.replace(record, entry=entry.name))
                    .default_value(record)
                )
            ),
            undo=False,
        )


# --- [COMPOSITION] ----------------------------------------------------------------------

luts: Final = LutExport(transform=chain.forming)
tones: Final = ToneViews(export=luts)
rows: Final = (luts, tones.toning, Append(target=TOPBAR_MT_file_export, rows=(OperatorButton(target=luts.forming), OperatorButton(target=luts.stripping))))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ToneView", "ToneViews", "luts", "rows", "tones"]
