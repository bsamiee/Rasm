"""Darkroom's effect chain over scene compositor entries: effects ranked by stage, single stages, phase menus, the chain as one OCIO transform, and its CPU render."""

from collections.abc import Callable
from functools import cached_property
from itertools import starmap
from typing import TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderResult
from blender.bpy.render.color.spaces import config_of, viewed
from blender.bpy.render.compositor.blending import Blend, Entry
from blender.bpy.render.compositor.effects import CompositorEffect, Enabled, Spaces
from blender.bpy.render.compositor.entries import EffectRows, Pinned
from blender.bpy.render.sessions.renders import RenderCall
from blender.imaging.color.processors import bracketed, DisplayView
from blender.imaging.sampling.integration import Precision
from blender.records.faults import accumulated, DomainFault, paired

from . import execution
from .stages import Effect, Stage

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Formed = tuple[CompositorEffect[Effect], Entry[Effect]]

# --- [SERVICES] -------------------------------------------------------------------------


class Chain(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Catalog effects and the formation row over scene compositor entries, ranked by stage, one entry per single stage, offered by phase, with the view and gain their entries hold."""

    effects: tuple[CompositorEffect[Effect], ...]
    formation: CompositorEffect[Effect]
    view: Callable[[Identity, bpy.types.Scene], BlenderResult[Option[DisplayView]]]
    gain: Callable[[Identity, bpy.types.Scene], BlenderResult[float]]
    pinned: tuple[Pinned, ...] = ()
    precision: Callable[[Identity], BlenderResult[Precision]] = lambda _identity: Ok(Precision())

    # --- [ROWS]

    @cached_property
    def rows(self) -> EffectRows[Effect]:
        """Formation row and catalog effects, placed by stage, one entry per single stage, the add menu one submenu per phase."""
        return EffectRows(
            rows=(self.formation, *self.effects),
            pinned=self.pinned,
            rank=Some(lambda row: row.record.stage),
            single=lambda row: row.record.stage in Stage.single,
            groups=Some(lambda row: row.record.stage.phase),
        )

    # --- [FORMING]

    def pointwise(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[Formed]:
        """Reads an owned entry whose effect reads each pixel alone under the default blend."""
        return self.rows.found(identity, scene, entry.name).bind(lambda held: held.row.read(identity, entry).bind(lambda record: local(entry, record)).map(lambda record: (held.row, record)))

    def forming(self, identity: Identity, scene: bpy.types.Scene, enabled: Enabled) -> BlenderResult[Option[PyOpenColorIO.Transform]]:
        """Groups the OCIO operations of enabled entries in stack order, closed by the scene view without a formation entry, Nothing when an entry has none."""
        entries = tuple(entry for entry in scene.compositor_effects if enabled.descriptor.read(entry) is True)

        def operations(config: PyOpenColorIO.Config, held: tuple[Formed, ...]) -> BlenderResult[tuple[Option[PyOpenColorIO.Transform], ...]]:
            def operation(row: CompositorEffect[Effect], record: Entry[Effect]) -> BlenderResult[Option[PyOpenColorIO.Transform]]:
                space = row.first(Spaces).map(lambda part: part.space(record.record))
                return record.record.transform(config).map(lambda found: found.map(lambda op: space.map(lambda name: bracketed(name, op)).default_value(op)))

            closing = Ok(()) if self.formation.entries(identity, scene) else viewed(scene).transform(config).map(lambda processor: (Some(processor.createGroupTransform()),))
            return paired(accumulated(starmap(operation, held)), closing).map(lambda pair: (*pair[0], *pair[1]))

        def grouped(parts: tuple[Option[PyOpenColorIO.Transform], ...]) -> Option[PyOpenColorIO.Transform]:
            return Some(PyOpenColorIO.GroupTransform([op for part in parts for op in part.to_list()])) if all(part.is_some() for part in parts) else Nothing

        held = accumulated(self.pointwise(identity, scene, entry) for entry in entries)
        return held.bind(lambda found: config_of(part.space(record.record) for row, record in found for part in row.found(Spaces)).bind(lambda config: operations(config, found))).map(grouped)

    # --- [RENDERING]

    def rendered(self, identity: Identity, scene: bpy.types.Scene, call: RenderCall) -> BlenderResult[frozenset["rna_enums.OperatorReturnItems"]]:
        """Renders through the chain's CPU stages under compositing, else natively."""
        return self.precision(identity).bind(lambda precision: execution.rendered(identity, self, scene, precision, call))


# --- [OPERATIONS] -----------------------------------------------------------------------


def local(entry: bpy.types.SceneCompositorEffect, record: Entry[Effect]) -> BlenderResult[Entry[Effect]]:
    """Returns the record when its effect reads each pixel alone under the default blend."""
    return paired(
        Ok(None) if record.record.pointwise else Error((DomainFault(subject=entry.name, value=type(record.record).title, accepted="pointwise effects"),)),
        Ok(None) if record.blend == Blend() else Error((DomainFault(subject=entry.name, value=record.blend, accepted=repr(Blend())),)),
    ).map(lambda _: record)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Chain", "Formed", "local"]
