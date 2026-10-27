"""Darkroom's effect chain over scene compositor entries: effects ranked by stage, single stages, phase menus, the chain as one OCIO transform, and its CPU render."""

from collections.abc import Callable, Mapping
from functools import cached_property
from itertools import starmap
from typing import Any, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.blending import Entry, MODE, OPACITY
from blender.bpy.render.compositor.effects import CompositorEffect, Enabled, Shell, Spaces
from blender.bpy.render.compositor.entries import EffectRows, Pinned
from blender.bpy.render.compositor.inputs import captured, valued
from blender.bpy.render.sessions.renders import RenderCall
from blender.imaging.color.processors import bracketed, DisplayView
from blender.imaging.sampling.integration import Precision
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason

from . import execution
from .stages import Effect, Stage, transform

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Formed = tuple[CompositorEffect[Effect], Entry[Effect], Mapping[Key, Value]]

# --- [SERVICES] -------------------------------------------------------------------------


class Chain(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Catalog effects and the formation row over scene compositor entries, ranked by stage, one entry per single stage, offered by phase, with the view and gain their entries hold."""

    effects: tuple[CompositorEffect[Any], ...]
    formation: CompositorEffect[Any]
    view: Callable[[Identity, bpy.types.Scene], Outcome[Option[DisplayView]]]
    gain: Callable[[Identity, bpy.types.Scene], Outcome[float]]
    pinned: tuple[Pinned, ...] = ()
    precision: Callable[[Identity], Outcome[Precision]] = lambda _identity: Ok(Precision())

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

    def pointwise(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> Outcome[_Formed]:
        """Reads an owned entry and its input values by item key, its effect reading each pixel alone under an unmasked full-opacity plain mix."""

        def local(record: Entry[Effect], values: Mapping[Key, Value]) -> Outcome[tuple[Entry[Effect], Mapping[Key, Value]]]:
            plain = not record.blend.masks and all(values.get(item_key(Shell.blend, socket.key)) == socket.members["default_value"] for socket in (MODE, OPACITY))
            return paired(
                Ok(None) if record.record.pointwise else Error((Fault(reason=RecordReason.OUTSIDE, subject=entry.name, member="pointwise effects", cause=type(record.record).__name__),)),
                Ok(None) if plain else Error((Fault(reason=RecordReason.OUTSIDE, subject=entry.name, member=Shell.blend, cause=repr(record.blend)),)),
            ).map(lambda _: (record, values))

        return self.rows.found(identity, scene, entry.name).bind(
            lambda held: (
                paired(held.row.read(identity, entry), captured(identity, entry))
                .map(lambda pair: (pair[0], valued(pair[1])))
                .bind(lambda pair: local(*pair))
                .map(lambda pair: (held.row, *pair))
            )
        )

    def forming(self, identity: Identity, scene: bpy.types.Scene, enabled: Enabled) -> Outcome[Option[PyOpenColorIO.Transform]]:
        """Groups the OCIO operations of enabled entries in stack order, closed by the scene view without a formation entry, Nothing when an entry has none."""
        config = PyOpenColorIO.GetCurrentConfig()
        entries = tuple(entry for entry in scene.compositor_effects if enabled.descriptor.read(entry) is True)

        def operation(row: CompositorEffect[Effect], record: Entry[Effect], values: Mapping[Key, Value]) -> Outcome[Option[PyOpenColorIO.Transform]]:
            space = row.first(Spaces).map(lambda part: part.space(record.record))
            return transform(record.record, values, config).map(lambda found: found.map(lambda op: space.map(lambda name: bracketed(name, op)).default_value(op)))

        def grouped(parts: tuple[Option[PyOpenColorIO.Transform], ...]) -> Option[PyOpenColorIO.Transform]:
            return Some(PyOpenColorIO.GroupTransform([op for part in parts for op in part.to_list()])) if all(part.is_some() for part in parts) else Nothing

        closing = Ok(()) if self.formation.entries(identity, scene) else viewed(scene).transform(config).map(lambda processor: (Some(processor.createGroupTransform()),))
        held = accumulated(self.pointwise(identity, scene, entry) for entry in entries).bind(lambda found: accumulated(starmap(operation, found)))
        return paired(held, closing).map(lambda pair: grouped((*pair[0], *pair[1])))

    # --- [RENDERING]

    def rendered(self, identity: Identity, scene: bpy.types.Scene, call: RenderCall) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
        """Renders through the chain's CPU stages under compositing, else natively."""
        return self.precision(identity).bind(lambda precision: execution.rendered(identity, self, scene, precision, call))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Chain"]
