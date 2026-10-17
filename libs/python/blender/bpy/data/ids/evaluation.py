"""Evaluated dependency graphs, durable updates, copied instances, and scoped geometry."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack
from typing import Self

import bpy
from expression import Nothing, Option, Result, Some
from mathutils import Matrix, Vector
import msgspec

from blender.bpy.data.ids.store import Handle
from blender.bpy.extension.events.handlers import Change, changed
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderFaults, BlenderResult

# --- [TYPES] ----------------------------------------------------------------------------


type Query = Ray | Near


# --- [MODELS] ---------------------------------------------------------------------------


class Update(msgspec.Struct, frozen=True, kw_only=True):
    """Original data-block identity and change kinds retained beyond a handler call."""

    handle: Handle[bpy.types.ID]
    changes: Change

    @classmethod
    def of(cls, item: bpy.types.DepsgraphUpdate) -> Self:
        """Returns the original handle and change kinds while the native update is valid."""
        return cls(handle=Handle.of(item.id), changes=changed(item))  # type: ignore[arg-type]


class Instancing(msgspec.Struct, frozen=True, kw_only=True):
    """Instancer, persistent identity, generated coordinates, and optional particle system."""

    instancer: bpy.types.Object
    persistent_id: tuple[int, ...]
    random_id: int
    orco: Vector
    uv: Vector
    particle_system: Option[str]


class Instance(msgspec.Struct, frozen=True, kw_only=True):
    """Original object, copied placement, visibility, and instancing metadata."""

    source: bpy.types.Object
    matrix: Matrix
    show_self: bool
    show_particles: bool
    instancing: Option[Instancing]

    @classmethod
    def read(cls, depsgraph: bpy.types.Depsgraph) -> tuple[Self, ...]:
        """Returns copied instances in native iterator order without retaining iterator items."""
        return tuple(
            cls(
                source=item.object.original,  # type: ignore[union-attr, arg-type]  # ty: ignore[unresolved-attribute, invalid-argument-type]
                matrix=item.matrix_world.copy(),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                show_self=item.show_self,
                show_particles=item.show_particles,
                instancing=Option.of_optional(item.parent).map(
                    lambda parent: Instancing(
                        instancer=parent.original,  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        persistent_id=tuple(item.persistent_id),  # type: ignore[arg-type]
                        random_id=item.random_id,
                        orco=item.orco.copy(),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        uv=Vector(item.uv),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        particle_system=Option.of_optional(item.particle_system).map(lambda system: system.name),
                    )
                ),
            )
            for item in depsgraph.object_instances
        )


class Ray(msgspec.Struct, frozen=True, kw_only=True):
    """Object-space ray origin, direction, and maximum distance."""

    origin: Vector
    direction: Vector
    distance: float


class Near(msgspec.Struct, frozen=True, kw_only=True):
    """Object-space nearest-surface origin and maximum distance."""

    origin: Vector
    distance: float


class Hit(msgspec.Struct, frozen=True, kw_only=True):
    """Found object-space location, surface normal, and native face index."""

    location: Vector
    normal: Vector
    face: int


# --- [OPERATIONS] -----------------------------------------------------------------------


def evaluated(view_layer: bpy.types.ViewLayer) -> BlenderResult[bpy.types.Depsgraph]:
    """Returns the updated layer graph, an edit fault when evaluation is already running, or the layer's absent graph."""
    return Result[None, BlenderFaults].bind(
        BlenderEditFault.attempted(repr(view_layer), view_layer.update),
        lambda _: Option.of_optional(view_layer.depsgraph).to_result_with(lambda: (BlenderAbsentFault(owner=repr(view_layer), key="depsgraph"),)),
    )


@contextmanager
def meshed(source: bpy.types.Object, depsgraph: bpy.types.Depsgraph, *, preserve_all_data_layers: bool = False) -> Generator[BlenderResult[bpy.types.Mesh]]:
    """Yields an evaluated mesh or typed refusal, clearing the native mesh when the scope ends."""
    copy = source.evaluated_get(depsgraph)
    with ExitStack() as lifetime:
        lifetime.callback(copy.to_mesh_clear)
        yield BlenderEditFault.attempted(repr(source), lambda: copy.to_mesh(preserve_all_data_layers=preserve_all_data_layers, depsgraph=depsgraph)).bind(
            lambda mesh: Option.of_optional(mesh).to_result_with(lambda: (BlenderAbsentFault(owner=repr(source), key="to_mesh"),))
        )


def hit(source: bpy.types.Object, depsgraph: bpy.types.Depsgraph, query: Query) -> BlenderResult[Option[Hit]]:
    """Returns a found surface hit or Nothing, preserving the native geometry refusal."""

    def answered() -> Option[Hit]:
        """Lifts the native found flag into an optional hit."""
        match query:
            case Ray(origin=origin, direction=direction, distance=distance):
                answer = source.ray_cast(origin, direction, distance=distance, depsgraph=depsgraph)
            case Near(origin=origin, distance=distance):
                answer = source.closest_point_on_mesh(origin, distance=distance, depsgraph=depsgraph)
        found, location, normal, face = answer
        return Some(Hit(location=location, normal=normal, face=face)) if found else Nothing  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    return BlenderEditFault.attempted(repr(source), answered)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Hit", "Instance", "Instancing", "Near", "Query", "Ray", "Update", "evaluated", "hit", "meshed"]
