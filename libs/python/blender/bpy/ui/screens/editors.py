"""Ordered editor space writes and values they replace."""

from collections.abc import Mapping
from functools import reduce
from typing import Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block
import msgspec

from blender.bpy.data.rna.descriptors import Descriptor, Native, RnaPath, Struct, struct_at, Value
from blender.records.faults import accumulated, Outcome

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Written = tuple[tuple[tuple[Native, Descriptor[bpy.types.Property], Value | Struct], ...], Outcome[None]]

# --- [MODELS] ---------------------------------------------------------------------------


class EditorState(msgspec.Struct, frozen=True, kw_only=True):
    """RNA values in dependency order under the active space, or under a region of a type."""

    region: "Option[rna_enums.RegionTypeItems]" = Nothing
    values: tuple[tuple[Descriptor[bpy.types.Property], Value | bytes | Struct], ...] = ()

    @classmethod
    def of[K: str | RnaPath](cls, root: type[Struct], values: Mapping[K, Value | bytes | Struct], region: "Option[rna_enums.RegionTypeItems]" = Nothing) -> Outcome[Self]:
        """Resolves values in declaration order."""
        return accumulated(Descriptor.resolve(root, path, bpy.types.Property).map(lambda setting: (setting, value)) for path, value in values.items()).map(
            lambda pairs: cls(region=region, values=pairs)
        )

    def applied(self, area: bpy.types.Area) -> Outcome[Option[Self]]:
        """Writes values in order, rolling back on refusal or absent owner.

        Returns:
            Replaced values, Nothing while the base or an owner is absent.
        """
        base = self.region.map(lambda kind: Block.of_seq(area.regions).filter(lambda region: region.type == kind).try_head()).default_with(lambda: Option.of_optional(area.spaces.active))

        def written(state: _Written, setting: Descriptor[bpy.types.Property], value: Value | bytes | Struct) -> Result[_Written, _Written]:
            match base.filter(lambda held: isinstance(held, setting.root)).bind(lambda held: struct_at(held, setting.owner)).map(lambda held: (held, setting.write(held, value))):
                case Option(some=(held, Result(tag="ok", ok=prior))):
                    return Ok(((*state[0], *((held, setting, replaced) for replaced in prior.to_list())), Ok(None)))
                case missed:
                    return Error((state[0], missed.map(lambda pair: pair[1].map(lambda _: None)).default_value(Ok(None))))

        done, stopped = (outcome := reduce(lambda state, pair: state.bind(lambda held: written(held, *pair)), self.values, Result[_Written, _Written](ok=((), Ok(None))))).merge()
        if done:
            area.tag_redraw()
        return (
            Ok(Some(msgspec.structs.replace(self, values=tuple((setting, prior) for _, setting, prior in reversed(done)))))
            if outcome.is_ok()
            else accumulated((stopped, *(descriptor.write(held, prior) for held, descriptor, prior in reversed(done)))).map(lambda _: Nothing)
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EditorState"]
