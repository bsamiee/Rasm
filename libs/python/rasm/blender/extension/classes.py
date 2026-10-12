"""Class registration ordered by native property and panel dependencies."""

from collections.abc import Generator, Iterator
import contextlib
from contextlib import AbstractContextManager
import graphlib
from typing import get_type_hints

import bpy
from expression import Option, Result
from expression.extra.result import catch
import msgspec

from rasm.blender.faults import BlenderFaults, BlenderRegisterFault, BlenderResult

# --- [SERVICES] -------------------------------------------------------------------------


class Classes(msgspec.Struct, frozen=True, kw_only=True):
    """Distinct classes one acquisition batch registers."""

    members: tuple[type, ...]

    # --- [ORDERING]

    @property
    def order(self) -> BlenderResult[tuple[type, ...]]:
        """Members after their property targets and parent panels, or the refusing cycle."""
        panels = {panel.bl_idname: panel for panel in self.members if issubclass(panel, bpy.types.Panel)}
        graph = {
            cls: tuple(member for member in self.members if member in targets)
            for cls in self.members
            for targets in [
                frozenset(
                    prop.keywords["type"]
                    for prop in get_type_hints(cls).values()
                    if isinstance(prop, bpy.props._PropertyDeferred) and prop.function in {bpy.props.PointerProperty, bpy.props.CollectionProperty}  # ruff: ignore[private-member-access]
                )
                | frozenset(Option.of_optional(vars(cls).get("bl_parent_id")).bind(lambda parent: Option.of_optional(panels.get(parent))).to_list())
            ]
        }
        return catch(lambda: tuple(graphlib.TopologicalSorter(graph).static_order()), exception=graphlib.CycleError)().map_error(
            lambda error: (BlenderRegisterFault(request=tuple(cls.__name__ for cls in self.members), error=error),)
        )

    # --- [REGISTRATION]

    def registrations(self) -> Iterator[AbstractContextManager[BlenderResult[None]]]:
        """Returns managers registering members in dependency order.

        Returns:
            One manager per member yielding Ok(None) after acquisition or a register fault;
            an ordering cycle returns one manager yielding that fault before any acquisition.
        """

        @contextlib.contextmanager
        def registered(cls: type) -> Generator[BlenderResult[None]]:
            """Yields the native class registration and releases whatever Blender holds, a property refusal leaving the class registered."""
            register, unregister = bpy.utils.register_classes_factory((cls,))
            try:
                register()
                answer = Result[None, BlenderFaults].Ok(None)
            except (ValueError, RuntimeError) as error:
                answer = Result[None, BlenderFaults].Error((BlenderRegisterFault(request=(cls.__name__,), error=error),))
            try:
                yield answer
            finally:
                if "bl_rna" in vars(cls):
                    unregister()

        return self.order.map(lambda order: map(registered, order)).default_with(lambda faults: iter((contextlib.nullcontext(Result[None, BlenderFaults].Error(faults)),)))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Classes"]
