"""Ordered editor space writes and values they replace."""

from collections.abc import Mapping
from typing import Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor, Native, struct_at, Value
from blender.bpy.faults import BlenderResult
from blender.records.faults import accumulated

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class EditorState(msgspec.Struct, frozen=True, kw_only=True):
    """RNA values in dependency order under the active space, or under a region of a type."""

    region: "Option[rna_enums.RegionTypeItems]" = Nothing
    values: "tuple[tuple[Descriptor[bpy.types.Property], Value | bytes | bpy.types.bpy_struct[object]], ...]" = ()

    @classmethod
    def of(
        cls, root: "type[bpy.types.bpy_struct[object]]", values: "Mapping[str, Value | bytes | bpy.types.bpy_struct[object]]", region: "Option[rna_enums.RegionTypeItems]" = Nothing
    ) -> BlenderResult[Self]:
        """Resolves values in declaration order."""
        return accumulated(Descriptor.resolve(root, path, bpy.types.Property).map(lambda setting: (setting, value)) for path, value in values.items()).map(
            lambda pairs: cls(region=region, values=pairs)
        )

    def applied(self, area: bpy.types.Area) -> BlenderResult[Option[Self]]:
        """Writes values in order, rolling back on refusal or absent owner.

        Returns:
            Replaced values, Nothing while the base or an owner is absent.
        """
        completed: list[tuple[Native, Descriptor[bpy.types.Property], Value | bpy.types.bpy_struct[object]]] = []
        base: Option[bpy.types.bpy_struct[object]] = self.region.map(lambda kind: Option.of_optional(next((region for region in area.regions if region.type == kind), None))).default_with(
            lambda: Option.of_optional(area.spaces.active)
        )

        def undone() -> BlenderResult[None]:
            return accumulated(descriptor.write(held, prior) for held, descriptor, prior in reversed(completed)).map(lambda _: None)

        try:
            for setting, value in self.values:
                match base.filter(lambda held: isinstance(held, setting.root)).bind(lambda held: struct_at(held, setting.owner)).map(lambda held: (held, setting.write(held, value))):
                    case Option(tag="none"):
                        return undone().map(lambda _: Nothing)
                    case Option(some=(_, Result(tag="error", error=faults))):
                        return accumulated((Error(faults), undone())).map(lambda _: Nothing)
                    case Option(some=(written, Result(ok=prior))):
                        completed.extend((written, setting, held) for held in prior.to_list())
            return Ok(Some(msgspec.structs.replace(self, values=tuple((setting, prior) for _, setting, prior in reversed(completed)))))
        finally:
            if completed:
                area.tag_redraw()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EditorState"]
