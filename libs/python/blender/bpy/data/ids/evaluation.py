"""Evaluated dependency graphs, durable updates, copied instances, and scoped geometry."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack
from typing import Self

import bpy
from expression import Nothing, Option, Result, Some
from expression.extra.result import catch
from mathutils import Matrix, Vector
import msgspec

from blender.bpy.data.ids.store import Handle
from blender.bpy.extension.events import Change, changed
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Faults, Outcome

# --- [MODELS] ---------------------------------------------------------------------------


class Update(msgspec.Struct, frozen=True, kw_only=True):
    """Original ID changes kept past handler call."""

    handle: Handle[bpy.types.ID]
    changes: Change

    @classmethod
    def of(cls, item: bpy.types.DepsgraphUpdate) -> Self:
        """Copies native update during handler call."""
        return cls(handle=Handle.of(item.id), changes=changed(item))  # type: ignore[arg-type]


class Instancing(msgspec.Struct, frozen=True, kw_only=True):
    """Instancer data of generated instance."""

    instancer: bpy.types.Object
    persistent_id: tuple[int, ...]
    random_id: int
    orco: Vector
    uv: Vector
    particle_system: Option[str]


class Instance(msgspec.Struct, frozen=True, kw_only=True):
    """Copied depsgraph instance."""

    source: bpy.types.Object
    matrix: Matrix
    show_self: bool
    show_particles: bool
    instancing: Option[Instancing]

    @classmethod
    def read(cls, depsgraph: bpy.types.Depsgraph) -> tuple[Self, ...]:
        """Copies object instances in iterator order."""
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


class Hit(msgspec.Struct, frozen=True, kw_only=True):
    """Object-space surface hit."""

    location: Vector
    normal: Vector
    face: int


# --- [OPERATIONS] -----------------------------------------------------------------------


def evaluated(view_layer: bpy.types.ViewLayer) -> Outcome[bpy.types.Depsgraph]:
    """Returns updated layer depsgraph, edit fault while evaluation runs."""
    return Result[None, Faults].bind(
        catch(view_layer.update, exception=RuntimeError)().map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(view_layer), cause=error),)),
        lambda _: Option.of_optional(view_layer.depsgraph).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(view_layer), member="depsgraph"),)),
    )


@contextmanager
def meshed(source: bpy.types.Object, depsgraph: bpy.types.Depsgraph, *, preserve_all_data_layers: bool = False) -> Generator[Outcome[bpy.types.Mesh]]:
    """Yields evaluated mesh, cleared at scope exit."""
    copy = source.evaluated_get(depsgraph)
    with ExitStack() as lifetime:
        lifetime.callback(copy.to_mesh_clear)
        yield (
            catch(copy.to_mesh, exception=RuntimeError)(preserve_all_data_layers=preserve_all_data_layers, depsgraph=depsgraph)
            .map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(source), cause=error),))
            .bind(lambda mesh: Option.of_optional(mesh).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(source), member="to_mesh"),)))
        )


def hit(source: bpy.types.Object, depsgraph: bpy.types.Depsgraph, origin: Vector, distance: float, direction: Option[Vector] = Nothing) -> Outcome[Option[Hit]]:
    """Returns object-space surface hit, Nothing on miss.

    Args:
        source: Object whose evaluated surface answers.
        depsgraph: Evaluation the surface reads.
        origin: Object-space query point.
        distance: Farthest answered distance.
        direction: Ray direction casting from origin, Nothing for the nearest surface point.

    Returns:
        Optional hit, edit fault for a refused query.
    """

    def answered() -> Option[Hit]:
        found, location, normal, face = direction.map(lambda toward: source.ray_cast(origin, toward, distance=distance, depsgraph=depsgraph)).default_with(
            lambda: source.closest_point_on_mesh(origin, distance=distance, depsgraph=depsgraph)
        )
        return Some(Hit(location=location, normal=normal, face=face)) if found else Nothing  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

    return catch(answered, exception=RuntimeError)().map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(source), cause=error),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Hit", "Instance", "Instancing", "Update", "evaluated", "hit", "meshed"]
