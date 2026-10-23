"""Header types stacking after the stock header of an editor region."""

from collections.abc import Generator
from contextlib import contextmanager, ExitStack
from typing import Literal, TYPE_CHECKING

import bpy
from expression import Nothing, Ok, Option
import msgspec

from blender.bpy.data.rna.conditions import Condition, holds
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.faults import BlenderResult
from blender.bpy.ui.layout.rows import acquired, painted, placed, RowContent, SurfaceDraw

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Header(msgspec.Struct, frozen=True, kw_only=True):
    """Own header row drawn after the stock header of its editor region while its condition holds."""

    key: Key
    space: "rna_enums.SpaceTypeItems"
    rows: tuple[RowContent, ...]
    region: Literal["HEADER", "TOOL_HEADER", "FOOTER"] = "HEADER"
    condition: Option[Condition] = Nothing

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns row acquisitions, then header class."""

        def built(name: str, paint: SurfaceDraw) -> type[bpy.types.Header]:
            def draw(header: bpy.types.Header, context: bpy.types.Context) -> None:
                for layout in Option.of_optional(header.layout).to_list():
                    if all(holds(condition, context) for condition in self.condition.to_list()):
                        paint(layout, context)

            return type(name, (bpy.types.Header,), {"bl_idname": name, "bl_space_type": self.space, "bl_region_type": self.region, "draw": draw})

        @contextmanager
        def registered() -> Generator[BlenderResult[None]]:
            with ExitStack() as stack:
                yield identity.name(Name.HEADER, self.key).bind(lambda name: placed(self.rows, identity).map(lambda draws: built(name, painted(draws)))).bind(lambda kind: entered(stack, (kind,)))

        return Ok((*acquired(self.rows), (Band.INTERFACE, registered())))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Header"]
