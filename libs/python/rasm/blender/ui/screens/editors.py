"""Ordered RNA editor changes and the state they replace."""

from collections import deque
from collections.abc import Mapping
from typing import Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from rasm.blender.data.rna.descriptors import Descriptor, struct_at, Value
from rasm.blender.faults import BlenderResult
from rasm.imaging.faults import accumulated

# --- [MODELS] ---------------------------------------------------------------------------


class EditorState(msgspec.Struct, frozen=True, kw_only=True):
    """RNA values in dependency order, rooted at a space or its held struct."""

    holder: str = ""
    values: "tuple[tuple[Descriptor[bpy.types.Property], Value | bytes | bpy.types.bpy_struct[object]], ...]" = ()

    @classmethod
    def of(cls, root: "type[bpy.types.bpy_struct[object]]", values: "Mapping[str, Value | bytes | bpy.types.bpy_struct[object]]", holder: str = "") -> BlenderResult[Self]:
        """Returns resolved values in declaration order, or all refused RNA paths."""
        return accumulated(Descriptor.resolve(root, path, bpy.types.Property).map(lambda setting: (setting, value)) for path, value in values.items()).map(
            lambda pairs: cls(holder=holder, values=pairs)
        )

    def applied(self, area: bpy.types.Area) -> BlenderResult[Option[Self]]:
        """Returns prior state, waits for absent holders, or rolls back a refused write."""
        completed: deque[tuple[bpy.types.bpy_struct[object] | bpy.types.bpy_prop_collection[bpy.types.bpy_struct[object]], Descriptor[bpy.types.Property], Value | bpy.types.bpy_struct[object]]] = (
            deque()
        )
        try:
            for setting, value in self.values:
                owner = (
                    Option
                    .of_optional(area.spaces.active)
                    .bind(lambda space: struct_at(space, self.holder))
                    .filter(lambda struct: isinstance(struct, setting.root))
                    .bind(lambda struct: struct_at(struct, setting.owner))
                )
                match owner.tag:
                    case "none":
                        return accumulated(descriptor.write(held, prior) for held, descriptor, prior in completed).map(lambda _: Nothing)
                    case "some":
                        held = owner.some
                answer = setting.write(held, value)
                match answer.tag:
                    case "ok":
                        completed.extendleft((held, setting, value) for value in answer.ok.to_list())
                    case "error":
                        return accumulated((Error(answer.error), *(descriptor.write(held, prior) for held, descriptor, prior in completed))).map(lambda _: Nothing)
            return Ok(Some(msgspec.structs.replace(self, values=tuple((setting, prior) for _, setting, prior in completed))))
        finally:
            if completed:
                area.tag_redraw()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EditorState"]
