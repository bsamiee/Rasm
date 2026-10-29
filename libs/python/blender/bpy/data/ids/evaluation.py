"""Evaluated dependency graphs, durable updates, copied instances, and scoped geometry."""

from collections.abc import Generator, Iterable
from contextlib import contextmanager, ExitStack
from functools import partial
from typing import Protocol, Self

import bpy
from expression import Nothing, Option, Result, Some
from expression.extra.result import catch
from mathutils import Matrix, Vector
import msgspec

from blender.bpy.data.ids.store import Handle, original
from blender.bpy.extension.events import Change
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class _Instance(Protocol):
    """Depsgraph object instance with the member types the host answers, which the stubs declare as methods."""

    @property
    def object(self) -> bpy.types.Object: ...

    @property
    def parent(self) -> bpy.types.Object | None: ...

    @property
    def particle_system(self) -> bpy.types.ParticleSystem | None: ...

    @property
    def matrix_world(self) -> Matrix: ...

    @property
    def orco(self) -> Vector: ...

    @property
    def uv(self) -> Iterable[float]: ...

    @property
    def persistent_id(self) -> Iterable[int]: ...

    @property
    def random_id(self) -> int: ...

    @property
    def show_self(self) -> bool: ...

    @property
    def show_particles(self) -> bool: ...


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


class Hit(msgspec.Struct, frozen=True, kw_only=True):
    """Object-space surface hit, location and normal as coordinates."""

    location: tuple[float, ...]
    normal: tuple[float, ...]
    face: int


# --- [OPERATIONS] -----------------------------------------------------------------------


def instances(depsgraph: bpy.types.Depsgraph) -> tuple[Instance, ...]:
    """Copies object instances in iterator order."""
    items: Iterable[_Instance] = depsgraph.object_instances  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    def copied(item: _Instance) -> Instance:

        def instancing(parent: bpy.types.Object) -> Instancing:
            return Instancing(
                instancer=original(parent),
                persistent_id=tuple(item.persistent_id),
                random_id=item.random_id,
                orco=item.orco.copy(),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                uv=Vector(tuple(item.uv)),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                particle_system=Option.of_optional(item.particle_system).map(lambda system: system.name),
            )

        return Instance(
            source=original(item.object),
            matrix=item.matrix_world.copy(),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            show_self=item.show_self,
            show_particles=item.show_particles,
            instancing=Option.of_optional(item.parent).map(instancing),
        )

    return tuple(map(copied, items))


def evaluated(view_layer: bpy.types.ViewLayer) -> Outcome[bpy.types.Depsgraph]:
    """Returns updated layer depsgraph, edit fault while evaluation runs."""
    return (
        catch(view_layer.update, exception=RuntimeError)()
        .map_error(partial(Fault.of, BlenderReason.REFUSED, repr(view_layer)))
        .bind(lambda _: Option.of_optional(view_layer.depsgraph).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(view_layer), member="depsgraph")))
    )


@contextmanager
def meshed(source: bpy.types.Object, depsgraph: bpy.types.Depsgraph, *, preserve_all_data_layers: bool = False) -> Generator[Outcome[bpy.types.Mesh]]:
    """Yields evaluated mesh, cleared at scope exit."""
    copy = source.evaluated_get(depsgraph)
    with ExitStack() as lifetime:
        lifetime.callback(copy.to_mesh_clear)
        yield (
            catch(copy.to_mesh, exception=RuntimeError)(preserve_all_data_layers=preserve_all_data_layers, depsgraph=depsgraph)
            .map_error(partial(Fault.of, BlenderReason.REFUSED, repr(source)))
            .bind(lambda mesh: Option.of_optional(mesh).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(source), member="to_mesh")))
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

    def answered(found: tuple[bool, Vector, Vector, int]) -> Option[Hit]:
        held, location, normal, face = found
        return Some(Hit(location=location[:], normal=normal[:], face=face)) if held else Nothing

    queried: Result[tuple[bool, Vector, Vector, int], RuntimeError] = direction.map(
        lambda toward: catch(source.ray_cast, exception=RuntimeError)(origin, toward, distance=distance, depsgraph=depsgraph)
    ).default_with(lambda: catch(source.closest_point_on_mesh, exception=RuntimeError)(origin, distance=distance, depsgraph=depsgraph))
    return queried.map(answered).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(source)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Hit", "Instance", "Instancing", "Update", "evaluated", "hit", "instances", "meshed"]
