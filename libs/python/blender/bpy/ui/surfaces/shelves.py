"""Asset shelves over extension asset marks or any asset predicate, with actions, active asset, context menu rows over the clicked asset, and the popover placement."""

from collections.abc import Callable
from functools import wraps
from typing import TYPE_CHECKING

import bpy
from expression import Error, Nothing, Option, Some, try_downcast
import msgspec

from blender.bpy.commands.calls import Called, Opened, Placement
from blender.bpy.commands.operators import Answer, Call, Empty, Operator
from blender.bpy.data.rna.conditions import Condition, polled
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.files.assets import AssetMark
from blender.bpy.ui.layout.rows import acquired, realized, Row, RowDraw
from blender.records.faults import accumulated, Fault, Outcome, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Action(msgspec.Struct, frozen=True, kw_only=True):
    """Operator body over tile's asset, its docstring the operator description."""

    use: Callable[[Call[Empty], bpy.types.AssetRepresentation], Outcome[Answer]]

    def operator(self, key: Key) -> Operator[Empty]:
        """Returns internal operator over context asset under key, described by the body docstring."""

        @wraps(self.use)
        def used(call: Call[Empty]) -> Outcome[Answer]:
            return Option.of_optional(call.context.asset).map(lambda asset: self.use(call, asset)).default_with(lambda: Error(Fault.of(BlenderReason.ABSENT, "bpy.context", member="asset")))

        return Operator(form=Empty, internal=True, run=used, named=Some(key))


class Shelf(msgspec.Struct, frozen=True, kw_only=True):
    """Asset shelf listing the assets an extension marked under the keys `assets` names, or the assets its predicate accepts, context menu rows drawing over the clicked asset."""

    key: Key
    space: "rna_enums.SpaceTypeItems"
    filters: frozenset[str]
    assets: frozenset[Key] | Callable[[bpy.types.AssetRepresentation], bool]
    activate: Option[Action] = Nothing
    drag: bool | Action = True
    active: Option[Callable[[bpy.types.Context], Option[bpy.types.AssetWeakReference]]] = Nothing
    menu: tuple[Row | RowDraw, ...] = ()
    preview: Option[int] = Nothing
    visible: bool = False
    catalogs_in_preferences: bool = True
    condition: Option[Condition] = Nothing

    @property
    def popup(self) -> Placement:
        """Placement buttons and keymaps open the shelf through as a popover."""
        return Placement(Opened(self.key, Called.SHELF))

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Returns shelf class with its action operators and menu rows.

        Args:
            identity: Extension owning class names and asset marks.

        Returns:
            Acquisitions, or fault per refused name, absent filter, and failed menu row.
        """
        struct = bpy.types.AssetShelf.bl_rna
        actions = tuple(
            (role, action.operator(f"{role}_{self.key}"))
            for role, offered in (("activate", self.activate), ("drag", Option.of_optional(try_downcast(Action, self.drag))))
            for action in offered.to_list()
        )
        named = accumulated(identity.name(bpy.types.Operator, row.key).map(lambda name: (role, row, name)) for role, row in actions)
        filtered = accumulated(
            Option
            .of_optional(struct.properties.get(key))
            .filter(lambda prop: isinstance(prop, bpy.types.BoolProperty) and prop.is_registered_optional)
            .map(lambda _: key)
            .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(bpy.types.AssetShelf), member=key))
            for key in sorted(self.filters)
        )

        def admitted(_cls: type[bpy.types.AssetShelf], asset: bpy.types.AssetRepresentation) -> bool:
            """Whether the shelf lists the asset: one the extension marked under a named key, or one the predicate accepts."""
            match self.assets:
                case frozenset(keys):
                    return AssetMark.key_of(identity, asset).filter(keys.__contains__).is_some()
                case accepts:
                    return accepts(asset)

        def built(parts: tuple[str, tuple[tuple[str, Operator[Empty], str], ...], tuple[str, ...], RowDraw]) -> Acquisitions:
            name, operators, filters, draw = parts
            kind = type(
                name,
                (bpy.types.AssetShelf,),
                {
                    "bl_idname": name,
                    "bl_space_type": self.space,
                    "bl_options": {
                        *(("STORE_ENABLED_CATALOGS_IN_PREFERENCES",) if self.catalogs_in_preferences else ()),
                        *(("DEFAULT_VISIBLE",) if self.visible else ()),
                        *(("NO_ASSET_DRAG",) if self.drag is False else ()),
                        *(("ACTIVATE_FOR_CONTEXT_MENU",) if self.activate.is_some() and self.menu else ()),
                    },
                    "asset_poll": classmethod(admitted),
                    **dict.fromkeys(filters, True),
                    **polled(self.condition),
                    **self.active.map(lambda active: {"get_active_asset": classmethod(lambda _cls: active(bpy.context).to_optional())}).default_value({}),
                    **({"draw_context_menu": classmethod(lambda _cls, context, asset, layout: draw(layout, asset, context))} if self.menu else {}),
                    **{f"bl_{role}_operator": name for role, _, name in operators},
                    **self.preview.map(lambda size: {"bl_default_preview_size": size}).default_value({}),
                },
            )
            return (*(row for _, row, _ in operators), *acquired(self.menu), kind)

        return paired(identity.name(bpy.types.AssetShelf, self.key), named, filtered, realized(self.menu, identity)).map(built)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Action", "Shelf"]
