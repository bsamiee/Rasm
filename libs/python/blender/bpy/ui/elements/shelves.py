"""Asset shelves listing marked or accepted assets, with activate and drag operators and context menu rows."""

from collections.abc import Callable
from functools import partial, wraps
from typing import TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.calls import Placement, PopupKind, PopupTarget
from blender.bpy.commands.operators import Answer, Call, Empty, Operator
from blender.bpy.data.rna.conditions import Condition, polled
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Tier
from blender.bpy.files.assets import AssetMark
from blender.bpy.ui.layout.rows import acquired, realized, RowContent, RowDraw
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type _Use = Callable[[Call[Empty], bpy.types.AssetRepresentation], Outcome[Answer]]

# --- [MODELS] ---------------------------------------------------------------------------


class Shelf(msgspec.Struct, frozen=True, kw_only=True):
    """Asset shelf listing the assets an extension marked under the keys `assets` names, or the assets its predicate accepts, context menu rows drawing over the clicked asset.

    `activate` and a `drag` body each register an internal operator over the context asset, described by the body's docstring.
    """

    key: Key
    space: "rna_enums.SpaceTypeItems"
    filters: frozenset[str]
    assets: frozenset[Key] | Callable[[bpy.types.AssetRepresentation], bool]
    activate: Option[_Use] = Nothing
    drag: bool | _Use = True
    active: Option[Callable[[bpy.types.Context], Option[bpy.types.AssetWeakReference]]] = Nothing
    menu: tuple[RowContent, ...] = ()
    preview: Option[int] = Nothing
    visible: bool = False
    catalogs_in_preferences: bool = True
    condition: Option[Condition] = Nothing

    @property
    def popup(self) -> Placement:
        """Placement buttons and keymaps open the shelf through as a popover."""
        return Placement(PopupTarget(self.key, PopupKind.SHELF))

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Returns the action operators and menu rows, then the shelf class built when the interface band opens.

        Args:
            identity: Extension owning class names and asset marks.

        Returns:
            Acquisitions, the class build faulting per refused name, absent filter, and failed menu row.
        """
        dragging = Nothing if isinstance(self.drag, bool) else Some(self.drag)
        actions = tuple(
            (member, Operator(form=Empty, internal=True, run=wraps(use)(partial(_acted, use)), named=Some(f"{role}_{self.key}")))
            for role, member, offered in (("activate", "bl_activate_operator", self.activate), ("drag", "bl_drag_operator", dragging))
            for use in offered.to_list()
        )

        def classed() -> Outcome[tuple[type, ...]]:
            """Builds the shelf class once its operator names, filters, and menu rows resolve."""
            struct = bpy.types.AssetShelf.bl_rna
            named = accumulated(identity.name(bpy.types.Operator, row.key).map(lambda name: (member, name)) for member, row in actions)
            filtered = accumulated(
                Option
                .of_optional(struct.properties.get(key))
                .filter(lambda prop: isinstance(prop, bpy.types.BoolProperty) and prop.is_registered_optional)
                .map(lambda _: key)
                .to_result_with(lambda: Fault.of(Reason.ABSENT, repr(bpy.types.AssetShelf), member=key))
                for key in sorted(self.filters)
            )

            def listed(_cls: type[bpy.types.AssetShelf], asset: bpy.types.AssetRepresentation) -> bool:
                """Whether the shelf lists the asset: one the extension marked under a named key, or one the predicate accepts."""
                match self.assets:
                    case frozenset(keys):
                        return AssetMark.key_of(identity, asset).filter(keys.__contains__).is_some()
                    case accepts:
                        return accepts(asset)

            def built(parts: tuple[str, tuple[tuple[str, str], ...], tuple[str, ...], RowDraw]) -> tuple[type, ...]:
                name, operators, filters, draw = parts
                options = {
                    *(("STORE_ENABLED_CATALOGS_IN_PREFERENCES",) if self.catalogs_in_preferences else ()),
                    *(("DEFAULT_VISIBLE",) if self.visible else ()),
                    *(("NO_ASSET_DRAG",) if self.drag is False else ()),
                    *(("ACTIVATE_FOR_CONTEXT_MENU",) if self.activate.is_some() and self.menu else ()),
                }
                native = {
                    "bl_idname": name,
                    "bl_space_type": self.space,
                    "bl_options": options,
                    "asset_poll": classmethod(listed),
                    **dict.fromkeys(filters, True),
                    **polled(self.condition),
                    **self.active.map(lambda active: {"get_active_asset": classmethod(lambda _cls: active(bpy.context).to_optional())}).default_value({}),
                    **({"draw_context_menu": classmethod(lambda _cls, context, asset, layout: draw(layout, asset, context))} if self.menu else {}),
                    **dict(operators),
                    **self.preview.map(lambda size: {"bl_default_preview_size": size}).default_value({}),
                }
                return (type(name, (bpy.types.AssetShelf,), native),)

            return paired(identity.name(bpy.types.AssetShelf, self.key), named, filtered, realized(self.menu, identity)).map(built)

        return Ok((*(row for _, row in actions), *acquired(self.menu), (Tier.INTERFACE, classed)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _acted(use: _Use, call: Call[Empty]) -> Outcome[Answer]:
    """Runs a shelf action over the context asset, absent fault without one."""
    return Option.of_optional(call.context.asset).map(lambda asset: use(call, asset)).default_with(lambda: Error(Fault.of(Reason.ABSENT, "bpy.context", member="asset")))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Shelf"]
