"""Menus of ordered rows, context-keyed rows, or pie slots."""

from collections.abc import Callable, Generator, Hashable, Iterable, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import Enum

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec

from blender.bpy.commands.calls import MenuPopup, Stock
from blender.bpy.commands.operators import answered, Outcome, Unchanged
from blender.bpy.data.rna.conditions import Condition, polled
from blender.bpy.data.rna.descriptors import Descriptor, rooted
from blender.bpy.data.rna.snapshots import Applied, Scoped, Snapshot
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import acquired, OperatorButton, painted, placed, Property, Pulldown, realized, RowContent, RowDraw, SurfaceDraw
from blender.bpy.ui.layout.values import Listing, Store
from blender.records.faults import accumulated
from blender.records.fields import heading

# --- [MODELS] ---------------------------------------------------------------------------


class Dropdown(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered rows with search on key press."""

    entries: tuple[RowContent, ...]
    search: bool = False


class Keyed[K: Hashable](msgspec.Struct, frozen=True, kw_only=True):
    """Entries drawn in key order a context returns."""

    entries: Mapping[K, RowContent]
    keys: Callable[[bpy.types.Context], Iterable[K]]
    search: bool = False


class Submenus[G: Enum](msgspec.Struct, frozen=True, kw_only=True):
    """Entries filed into one submenu per group in group declaration order, the native add-menu form."""

    entries: tuple[tuple[G, RowContent], ...]
    search: bool = False


class Pie(msgspec.Struct, frozen=True, kw_only=True):
    """Pie slots in Blender's radial fill order."""

    west: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    east: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    south: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    north: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    north_west: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    north_east: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    south_west: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    south_east: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing


class Menu[K: Hashable = str](msgspec.Struct, frozen=True, kw_only=True):
    """Extension menu with poll condition."""

    key: Key
    title: str
    description: str
    body: Dropdown | Keyed[K] | Submenus[Enum] | Pie
    condition: Option[Condition] = Nothing

    # --- [PLACEMENT]

    @property
    def popup(self) -> MenuPopup:
        """Popup target operators and keymaps open."""
        return MenuPopup(menu=self.key, pie=isinstance(self.body, Pie))

    # --- [ACQUISITION]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns submenus, row acquisitions, then menu class."""
        match self.body:
            case Dropdown(entries=rows, search=search):
                submenus: tuple[Menu, ...] = ()

                def drawing() -> BlenderResult[SurfaceDraw]:
                    return placed(rows, identity).map(painted)

            case Keyed(entries=entries, keys=keys, search=search):
                offered, submenus = tuple(entries.items()), ()
                rows = tuple(row for _, row in offered)

                def keyed(draws: Mapping[K, RowDraw]) -> SurfaceDraw:
                    def shown(layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
                        for key in keys(context):
                            draws[key](layout, context, context)

                    return shown

                def drawing() -> BlenderResult[SurfaceDraw]:
                    return accumulated(realized(row, identity).map(lambda draw: (key, draw)) for key, row in offered).map(dict).map(keyed)

            case Submenus(entries=entries, search=search):
                present = frozenset(held for held, _ in entries)
                submenus = tuple(
                    Menu(
                        key=f"{self.key}_{group.name.lower()}",
                        title=heading(group.name),
                        description=f"{heading(group.name)}: {self.description}",
                        body=Dropdown(entries=tuple(row for held, row in entries if held is group)),
                        condition=self.condition,
                    )
                    for kind in dict.fromkeys(type(held) for held, _ in entries)
                    for group in kind
                    if group in present
                )
                rows = tuple(Pulldown(menu=group.key) for group in submenus)

                def drawing() -> BlenderResult[SurfaceDraw]:
                    return placed(rows, identity).map(painted)

            case Pie():
                slots, search, submenus = msgspec.structs.astuple(self.body), False, ()
                rows = tuple(row for slot in slots for row in slot.to_list())

                def radial(draws: tuple[Option[RowDraw], ...]) -> SurfaceDraw:
                    def shown(layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
                        pie = layout.menu_pie()
                        for draw in draws:
                            match draw:
                                case Option(tag="some", some=row):
                                    row(pie.column(), context, context)
                                case _:
                                    pie.separator()

                    return shown

                def slot(row: OperatorButton | Pulldown | Property | RowDraw) -> BlenderResult[RowDraw]:
                    checked = row.target.resolved().bind(lambda held: held.accepts((bpy.types.BoolProperty, bpy.types.EnumProperty))).map(lambda _: None) if isinstance(row, Property) else Ok(None)
                    return checked.bind(lambda _: realized(row, identity))

                def drawing() -> BlenderResult[SurfaceDraw]:
                    return accumulated(each.map(lambda row: slot(row).map(Some)).default_with(lambda: Ok(Nothing)) for each in slots).map(radial)

        return Ok((*submenus, *acquired(rows), (Band.INTERFACE, registered(identity, self.key, self.title, self.description, drawing, search=search, condition=self.condition))))


class Captured(msgspec.Struct, frozen=True, kw_only=True):
    """Selected settings captured from context and applied over a scope."""

    preset: Preset[Snapshot]
    scope: Scoped
    selection: tuple[str, ...]

    @property
    def key(self) -> Key:
        """Menu key shared with preset listing."""
        return Listing.keyed(self.preset)

    def listing(self) -> BlenderResult[Listing[Snapshot]]:
        """Resolves selected settings once into listing."""

        def listed(selected: tuple[Descriptor, ...]) -> Listing[Snapshot]:
            def captured(context: bpy.types.Context) -> "BlenderResult[tuple[bpy.types.bpy_struct[object], Snapshot]]":
                return rooted(self.scope.owner_type, context).bind(lambda owner: Snapshot.capture(owner, Some(selected)).map(lambda snapshot: (owner, snapshot)))

            def finished(outcomes: tuple[Applied, ...]) -> BlenderResult[Outcome]:
                owner = self.scope.owner_type.__name__
                if not outcomes:
                    return Ok(Unchanged(reason=f"No {owner} in scope"))
                absent = (Applied(outcome=Error((BlenderAbsentFault(owner=owner, key=path),))) for path in dict.fromkeys(path for outcome in outcomes for path in outcome.skipped))
                return answered(Applied.join((*outcomes, *absent)), "Settings already hold preset")

            return Listing(
                preset=self.preset,
                current=lambda context: captured(context).map(lambda pair: pair[1]),
                matches=lambda context, stored: captured(context).bind(lambda held: accumulated(value.projected(held[0], held[1]).map(lambda pair: pair[1] == held[1]) for value in stored)),
                applied=lambda context, stored: self.scope.owners(context).map(stored.applied).bind(finished),
            )

        return accumulated(Descriptor.resolve(self.scope.owner_type, path, bpy.types.Property) for path in self.selection).map(listed)


class PresetMenu[T: msgspec.Struct = Snapshot](msgspec.Struct, frozen=True, kw_only=True):
    """Searchable preset listing with trailing rows."""

    source: Store[T] | Listing[T] | Captured
    entries: tuple[RowContent, ...] = ()

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns source acquisitions, then preset menu."""

        def menu[S: msgspec.Struct](listing: Listing[S]) -> AbstractContextManager[BlenderResult[None]]:
            name = bpy.path.display_name(listing.preset.key)
            owned = (listing, *self.entries)
            return registered(identity, listing.key, f"{name} Presets", f"Apply, add, or import {name.lower()} presets", lambda: placed(owned, identity).map(painted), search=True)

        def listed[S: msgspec.Struct](listing: Listing[S]) -> BlenderResult[Acquisitions]:
            return Ok((*acquired((listing, *self.entries)), (Band.INTERFACE, menu(listing))))

        match self.source:
            case Store() as store:

                @contextmanager
                def registered_store() -> Generator[BlenderResult[None]]:
                    with ExitStack() as stack:
                        result: BlenderResult[None] = store.bound(identity).bind(
                            lambda slots: slots.listing.map(lambda listing: stack.enter_context(menu(listing))).default_value(Result[None, BlenderFaults](ok=None))
                        )
                        yield result

                return Ok((store.attachment, store, *acquired(self.entries), (Band.INTERFACE, registered_store())))
            case Listing() as listing:
                return listed(listing)
            case Captured() as captured:
                return captured.listing().bind(listed)


# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def registered(
    identity: Identity, key: Key, title: str, description: str, drawing: Callable[[], BlenderResult[SurfaceDraw]], *, search: bool, condition: Option[Condition] = Nothing
) -> Generator[BlenderResult[None]]:
    """Registers menu class after nested rows, a searchable menu opened from code or a key leading with its search entry."""

    def built(draw: SurfaceDraw, name: str, heading: Option[RowDraw]) -> type[bpy.types.Menu]:
        def shown(menu: bpy.types.Menu, context: bpy.types.Context) -> None:
            for layout in Option.of_optional(menu.layout).to_list():
                for entry in heading.to_list() if layout.operator_context == "EXEC_REGION_WIN" else ():
                    entry(layout, context, context)
                    layout.separator()
                draw(layout, context)

        return type(
            name,
            (bpy.types.Menu,),
            {"bl_idname": name, "bl_label": title, "bl_description": description, "bl_options": {"SEARCH_ON_KEY_PRESS"} if search else set(), "draw": shown, **polled(condition)},
        )

    with ExitStack() as stack:
        yield (
            drawing()
            .bind(
                lambda draw: identity.name(Name.MENU, key).bind(
                    lambda name: (
                        realized(OperatorButton(target=Stock(operator=bpy.ops.wm.search_single_menu, values={"menu_idname": name}), text="Search...", icon=Some("VIEWZOOM")), identity).map(Some)
                        if search
                        else Ok(Nothing)
                    ).map(lambda heading: built(draw, name, heading))
                )
            )
            .bind(lambda native: entered(stack, (native,)))
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Captured", "Dropdown", "Keyed", "Menu", "Pie", "PresetMenu", "Submenus"]
