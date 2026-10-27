"""Online Manual links from manifest website over extension operators, types, and property groups."""

from collections import deque
from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from functools import partial

import bpy
from expression import Option
import msgspec

from blender.bpy.data.rna.descriptors import Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Outcome

# --- [SERVICES] -------------------------------------------------------------------------


class ManualMap(msgspec.Struct, frozen=True, kw_only=True):
    """Online Manual links from manifest website and extension operators, types, and property groups."""

    def acquisitions(self, identity: Identity) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
        """Builds manual hook acquisition over manifest website.

        Args:
            identity: Extension with website and generated names hook reads.

        Returns:
            Ok of one registry acquisition, or absent website fault.
        """
        return (
            Option
            .of_optional(identity.manifest.website)
            .map(lambda website: ((Band.REGISTRIES, self._registered(identity, website)),))
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, identity.package, member="website"))
        )

    @staticmethod
    @contextmanager
    def _registered(identity: Identity, website: str) -> Generator[None]:
        """Holds the manual hook over extension operators, types, and property groups while the scope stands."""
        hook = partial(_mapped, identity.package, identity.manifest.id.lower(), website)
        with ExitStack() as stack:
            bpy.utils.register_manual_map(hook)
            stack.callback(bpy.utils.unregister_manual_map, hook)
            yield


# --- [OPERATIONS] -----------------------------------------------------------------------


def _mapped(package: str, category: str, website: str) -> tuple[str, list[tuple[str, str]]]:
    """Returns website with extension operator, type, and property patterns, then one pattern per property group behind an extension-prefixed property reachable from `BlendData` or nested in such a group or in the extension's preferences."""

    def definition(owner: Struct | type[Struct]) -> bpy.types.Struct:
        definition: bpy.types.Struct = owner.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return definition

    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    group = definition(bpy.types.PropertyGroup)

    def grouped(struct: bpy.types.Struct) -> bool:
        return struct.base == group

    attached = tuple(target for identifier, target in _walked((definition(bpy.types.BlendData),), lambda _: True) if identifier.startswith(f"{category}_") and grouped(target))
    owned = Option.of_optional(preferences.addons.get(package)).bind(lambda addon: Option.of_optional(addon.preferences)).map(definition).to_list()
    nested = tuple(target for _, target in _walked((*attached, *owned), grouped) if grouped(target))
    return website, [
        *((f"bpy.ops.{category}.{key}", f"#{key}") for key in dir(getattr(bpy.ops, category))),
        (f"bpy.types.{category}_*", ""),
        (f"bpy.types.*.{category}_*", ""),
        *((f"bpy.types.{identifier.lower()}.*", "") for identifier in dict.fromkeys(group.identifier for group in (*attached, *nested))),
    ]


def _walked(starts: tuple[bpy.types.Struct, ...], followed: Callable[[bpy.types.Struct], bool]) -> tuple[tuple[str, bpy.types.Struct], ...]:
    """Returns identifier and fixed target of every pointer and collection property breadth-first from starts, entering each target `followed` accepts once."""
    seen, frontier, edges = {start.identifier for start in starts}, deque(starts), list[tuple[str, bpy.types.Struct]]()
    while frontier:
        properties = frontier.popleft().properties
        for identifier, target in (
            (prop.identifier, target) for prop in properties if isinstance(prop, bpy.types.PointerProperty | bpy.types.CollectionProperty) for target in Option.of_optional(prop.fixed_type).to_list()
        ):
            edges.append((identifier, target))
            if target.identifier not in seen and followed(target):
                seen.add(target.identifier)
                frontier.append(target)
    return tuple(edges)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ManualMap"]
