"""Evaluated dependency graphs, durable updates, copied instances, and scoped geometry."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack
from functools import partial
from typing import Self

import bpy
from expression import Nothing, Option, Result, Some
from expression.extra.result import catch
from mathutils import Vector
import msgspec
import numpy as np

from blender.bpy.data.ids.store import Handle
from blender.bpy.extension.events import Change
from blender.records.faults import Fault, Outcome, Reason

# --- [MODELS] ---------------------------------------------------------------------------


class Update(msgspec.Struct, frozen=True, kw_only=True):
    """Original ID changes kept past handler call."""

    handle: Handle[bpy.types.ID]
    changes: Change

    @classmethod
    def of(cls, item: bpy.types.DepsgraphUpdate) -> Self:
        """Copies native update during handler call."""
        block: bpy.types.ID = item.id  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return cls(handle=Handle.of(block), changes=Change.of(item))


class Instancing(msgspec.Struct, frozen=True, kw_only=True):
    """Instancer data of generated instance, generated and UV coordinates as values."""

    instancer: bpy.types.Object
    persistent_id: tuple[int, ...]
    random_id: int
    orco: tuple[float, ...]
    uv: tuple[float, ...]
    particle_system: Option[str]


class Instance(msgspec.Struct, frozen=True, kw_only=True):
    """Copied depsgraph instance, its world matrix as rows."""

    source: bpy.types.Object
    matrix: tuple[tuple[float, ...], ...]
    show_self: bool
    show_particles: bool
    instancing: Option[Instancing]


class Hit(msgspec.Struct, frozen=True, kw_only=True):
    """Object-space surface hit, location and normal as coordinates."""

    location: tuple[float, ...]
    normal: tuple[float, ...]
    face: int


# --- [OPERATIONS] -----------------------------------------------------------------------


def instances(depsgraph: bpy.types.Depsgraph) -> tuple[Instance, ...]:
    """Copies object instances in iterator order, sources and instancers as their originals."""

    def copied(item: bpy.types.DepsgraphObjectInstance) -> Instance:
        source: bpy.types.Object = item.object.original  # type: ignore[union-attr, assignment]  # ty: ignore[unresolved-attribute, invalid-assignment]

        def instancing(parent: bpy.types.Object) -> Instancing:
            instancer: bpy.types.Object = parent.original  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return Instancing(
                instancer=instancer,
                persistent_id=tuple(np.asarray(item.persistent_id).tolist()),
                random_id=item.random_id,
                orco=item.orco[:],
                uv=tuple(np.asarray(item.uv).tolist()),
                particle_system=Option.of_optional(item.particle_system).map(lambda system: system.name),
            )

        return Instance(
            source=source,
            matrix=tuple(row[:] for row in item.matrix_world[:]),
            show_self=item.show_self,
            show_particles=item.show_particles,
            instancing=Option.of_optional(item.parent).map(instancing),
        )

    return tuple(map(copied, depsgraph.object_instances))


def evaluated(view_layer: bpy.types.ViewLayer) -> Outcome[bpy.types.Depsgraph]:
    """Returns updated layer depsgraph, edit fault while evaluation runs."""
    return (
        catch(view_layer.update, exception=RuntimeError)()
        .map_error(partial(Fault.of, Reason.REFUSED, repr(view_layer)))
        .bind(lambda _: Option.of_optional(view_layer.depsgraph).to_result_with(partial(Fault.of, Reason.ABSENT, repr(view_layer), member="depsgraph")))
    )


@contextmanager
def meshed(source: bpy.types.Object, depsgraph: bpy.types.Depsgraph, *, preserve_all_data_layers: bool = False) -> Generator[Outcome[bpy.types.Mesh]]:
    """Yields evaluated mesh, cleared at scope exit."""
    copy = source.evaluated_get(depsgraph)
    with ExitStack() as lifetime:
        lifetime.callback(copy.to_mesh_clear)
        yield (
            catch(copy.to_mesh, exception=RuntimeError)(preserve_all_data_layers=preserve_all_data_layers, depsgraph=depsgraph)
            .map_error(partial(Fault.of, Reason.REFUSED, repr(source)))
            .bind(lambda mesh: Option.of_optional(mesh).to_result_with(partial(Fault.of, Reason.ABSENT, repr(source), member="to_mesh")))
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
    queried: Result[tuple[bool, Vector, Vector, int], RuntimeError] = direction.map(
        lambda toward: catch(source.ray_cast, exception=RuntimeError)(origin, toward, distance=distance, depsgraph=depsgraph)
    ).default_with(lambda: catch(source.closest_point_on_mesh, exception=RuntimeError)(origin, distance=distance, depsgraph=depsgraph))
    return queried.map(lambda found: Some(found).filter(lambda held: held[0]).map(lambda held: Hit(location=held[1][:], normal=held[2][:], face=held[3]))).map_error(
        partial(Fault.of, Reason.REFUSED, repr(source))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Hit", "Instance", "Instancing", "Update", "evaluated", "hit", "instances", "meshed"]
