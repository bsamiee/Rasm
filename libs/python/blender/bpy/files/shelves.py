"""Asset shelves with actions, active asset, and context menu rows."""

from collections.abc import Callable
from typing import TYPE_CHECKING

import bpy
from expression import Error, Nothing, Option, Some
import msgspec

from blender.bpy.commands.operators import Call, Empty, Operator, Outcome
from blender.bpy.data.rna.conditions import Condition, polled
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.ui.layout.rows import acquired, painted, placed, Row, RowDraw, SurfaceDraw
from blender.imaging.faults import accumulated, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [MODELS] ---------------------------------------------------------------------------


class Action(msgspec.Struct, frozen=True, kw_only=True):
    """Operator body over tile's asset."""

    title: str
    description: str
    use: Callable[[Call[Empty], bpy.types.AssetRepresentation], BlenderResult[Outcome]]

    def row(self, key: Key) -> Operator[Empty]:
        """Returns internal operator over context asset."""
        return Operator(
            key=key,
            title=self.title,
            description=self.description,
            form=Empty,
            internal=True,
            run=lambda call: Option.of_optional(call.context.asset).map(lambda asset: self.use(call, asset)).default_with(lambda: Error((BlenderAbsentFault(owner="bpy.context", key="asset"),))),
        )


class Shelf(msgspec.Struct, frozen=True, kw_only=True):
    """Asset shelf filtered by extension marks."""

    key: Key
    space: "rna_enums.SpaceTypeItems"
    filters: frozenset[str]
    marks: frozenset[Key]
    activate: Option[Action] = Nothing
    drag: bool | Action = True
    active: Option[Callable[[bpy.types.Context], Option[bpy.types.AssetWeakReference]]] = Nothing
    menu: tuple[Row | RowDraw, ...] = ()
    preview: Option[int] = Nothing
    visible: bool = False
    condition: Option[Condition] = Nothing

    def acquisitions(self, identity: Identity, /) -> BlenderResult[Acquisitions]:
        """Returns shelf class with its action operators and menu rows.

        Args:
            identity: Extension owning class names and asset marks.

        Returns:
            Acquisitions, or fault per refused name, absent filter, and failed menu row.
        """
        struct = bpy.types.AssetShelf.bl_rna
        actions = tuple(
            (role, action.row(f"{role}_{self.key}"))
            for role, offered in (("activate", self.activate), ("drag", Some(self.drag) if isinstance(self.drag, Action) else Nothing))
            for action in offered.to_list()
        )
        named = accumulated(identity.name(Name.OPERATOR, row.key).map(lambda name: (role, row, name)) for role, row in actions)
        filtered = accumulated(
            Option
            .of_optional(struct.properties.get(key))
            .filter(lambda prop: isinstance(prop, bpy.types.BoolProperty) and prop.is_registered_optional)
            .map(lambda _: key)
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(bpy.types.AssetShelf), key=key),))
            for key in sorted(self.filters)
        )

        def built(parts: tuple[tuple[str, tuple[tuple[str, Operator[Empty], str], ...]], tuple[tuple[str, ...], SurfaceDraw]]) -> Acquisitions:
            (name, operators), (filters, paint) = parts
            kind = type(
                name,
                (bpy.types.AssetShelf,),
                {
                    "bl_idname": name,
                    "bl_space_type": self.space,
                    "bl_options": {
                        "STORE_ENABLED_CATALOGS_IN_PREFERENCES",
                        *(("DEFAULT_VISIBLE",) if self.visible else ()),
                        *(("NO_ASSET_DRAG",) if self.drag is False else ()),
                        *(("ACTIVATE_FOR_CONTEXT_MENU",) if self.activate.is_some() and self.menu else ()),
                    },
                    "asset_poll": classmethod(lambda _cls, asset: asset.metadata.get(identity.manifest.id) in self.marks),
                    **dict.fromkeys(filters, True),
                    **polled(self.condition),
                    **self.active.map(lambda active: {"get_active_asset": classmethod(lambda _cls: active(bpy.context).to_optional())}).default_value({}),
                    **({"draw_context_menu": classmethod(lambda _cls, context, _asset, layout: paint(layout, context))} if self.menu else {}),
                    **{f"bl_{role}_operator": name for role, _, name in operators},
                    **self.preview.map(lambda size: {"bl_default_preview_size": size}).default_value({}),
                },
            )
            return (*(row for _, row, _ in operators), *acquired(self.menu), (Band.INTERFACE, kind))

        return paired(paired(identity.name(Name.SHELF, self.key), named), paired(filtered, placed(self.menu, identity).map(painted))).map(built)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Action", "Shelf"]
