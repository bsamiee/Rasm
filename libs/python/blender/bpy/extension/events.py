"""Native event paths, runtime-scene policy, and copied dependency-graph change kinds."""

from collections.abc import Callable
from enum import auto, Flag
from functools import reduce
from pathlib import Path
from typing import Self

import bpy
from expression import Option

from blender.bpy.extension.identity import Identity
from blender.records.faults import Nothing, Ok, Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class Change(Flag):
    """Change kinds depsgraph update reports, in the order of the update flags reading them."""

    GEOMETRY = auto()
    TRANSFORM = auto()
    SHADING = auto()

    @classmethod
    def of(cls, update: bpy.types.DepsgraphUpdate, /) -> Self:
        """Reads the change kinds an update reports during its handler call, `Change(0)` for none."""
        flags = (update.is_updated_geometry, update.is_updated_transform, update.is_updated_shading)
        return reduce(cls.__or__, (kind for kind, flagged in zip(cls, flags) if flagged), cls(0))


# --- [OPERATIONS] -----------------------------------------------------------------------


def runtime(identity: Identity, scene: bpy.types.Scene, /) -> bool:
    """Whether a scene a callback received is a runtime scene holding the extension's manifest ID as its mark."""
    return scene.is_runtime_data and identity.manifest.id in scene  # type: ignore[operator]


def located(body: Callable[[Option[Path]], Outcome[None]], path: str, /) -> Outcome[None]:
    """Passes the path a load or save handler receives to body, Nothing for the startup file's empty path."""
    return body(Option.of_optional(path or None).map(Path))


def updated(
    ids: tuple[type[bpy.types.ID], ...],
    body: Callable[[bpy.types.Scene, tuple[bpy.types.DepsgraphUpdate, ...]], Outcome[None]],
    scene: bpy.types.Scene,
    depsgraph: bpy.types.Depsgraph,
    /,
    *,
    changes: Option[Change] = Nothing,
) -> Outcome[None]:
    """Passes the scene and depsgraph updates of ID types reporting a change kind (any for Nothing) to body, Ok(None) when none matches."""
    updates = tuple(item for item in depsgraph.updates if isinstance(item.id, ids) and changes.map(lambda kinds: bool(Change.of(item) & kinds)).default_value(value=True))
    return body(scene, updates) if updates else Ok(None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Change", "located", "runtime", "updated"]
