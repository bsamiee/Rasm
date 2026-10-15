"""Native menus compiled from ordered rows or context-selected keyed content."""

from collections.abc import Callable, Generator, Hashable, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from enum import Enum

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
from msgspec import inspect

from blender.bpy.commands.calls import MenuPopup
from blender.bpy.commands.operators import answered, Outcome, Stock, Unchanged
from blender.bpy.data.rna.conditions import Condition, polled
from blender.bpy.data.rna.descriptors import Descriptor, root_in
from blender.bpy.data.rna.snapshots import Applied, Scoped, Snapshot
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import acquired, bound, fits, OperatorButton, painted, placed, Property, Pulldown, realized, RowContent, RowDraw, SurfaceDraw
from blender.bpy.ui.layout.sections import Listing, Store
from blender.imaging.faults import accumulated

# --- [MODELS] ---------------------------------------------------------------------------


class Dropdown(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered native rows with optional search on key press."""

    entries: tuple[RowContent, ...]
    search: bool = False


class Keyed[K: Hashable](msgspec.Struct, frozen=True, kw_only=True):
    """Offered entries drawn in the context's current key order."""

    entries: Mapping[K, RowContent]
    keys: Callable[[bpy.types.Context], Iterable[K]]
    search: bool = False


class Pie(msgspec.Struct, frozen=True, kw_only=True):
    """Eight typed or native callback slots in Blender's radial fill order."""

    west: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    east: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    south: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    north: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    north_west: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    north_east: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    south_west: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing
    south_east: Option[OperatorButton | Pulldown | Property | RowDraw] = Nothing


class Menu[K: Hashable = str](msgspec.Struct, frozen=True, kw_only=True):
    """Extension menu with native availability and acquired row content."""

    key: Key
    title: str
    description: str
    body: Dropdown | Keyed[K] | Pie
    condition: Option[Condition] = Nothing

    # --- [PLACEMENT]

    @property
    def popup(self) -> MenuPopup:
        """Native call target for operators and keymaps."""
        return MenuPopup(menu=self.key, pie=isinstance(self.body, Pie))

    # --- [ACQUISITION]

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Retains nested content before the native menu's realization and registration."""
        match self.body:
            case Dropdown(entries=entries, search=search):
                rows = entries

                def drawing() -> BlenderResult[SurfaceDraw]:
                    return placed(rows, identity).map(painted)

            case Keyed(entries=entries, keys=keys, search=search):
                offered = tuple(entries.items())
                rows = tuple(row for _, row in offered)

                def keyed(draws: Mapping[K, RowDraw]) -> SurfaceDraw:
                    def shown(layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
                        for key in keys(context):
                            draws[key](layout, context, context)

                    return shown

                def drawing() -> BlenderResult[SurfaceDraw]:
                    return accumulated(realized(row, identity).map(lambda draw: (key, draw)) for key, row in offered).map(dict).map(keyed)

            case Pie():
                slots = msgspec.structs.astuple(self.body)
                rows = tuple(row for slot in slots for row in slot.to_list())
                search = False

                def slot(row: OperatorButton | Pulldown | Property | RowDraw) -> BlenderResult[RowDraw]:
                    match row:
                        case Property(target=target):
                            return bound(
                                ((target, lambda held: fits(held, (bpy.types.BoolProperty, bpy.types.EnumProperty), (inspect.BoolType, Enum))),),
                                Ok(lambda held: held[0].map(lambda target: realized(msgspec.structs.replace(row, target=target), identity).map(Some)).default_value(Ok(Nothing))),
                            )
                        case _:
                            return realized(row, identity)

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

                def drawing() -> BlenderResult[SurfaceDraw]:
                    return accumulated(each.map(lambda row: slot(row).map(Some)).default_with(lambda: Ok(Nothing)) for each in slots).map(radial)

        return Ok((*acquired(rows), (Band.INTERFACE, registered(identity, self.key, self.title, self.description, drawing, search=search, condition=self.condition))))


class Captured(msgspec.Struct, frozen=True, kw_only=True):
    """Selected native settings captured from the context and applied over a scope."""

    preset: Preset[Snapshot]
    scope: Scoped
    selection: tuple[str, ...]

    @property
    def key(self) -> Key:
        """Menu key shared with the acquired preset listing."""
        return Listing.keyed(self.preset)

    def listing(self) -> BlenderResult[Listing[Snapshot]]:
        """Resolves selected settings once into capture, match, and apply policies."""

        def listed(selected: tuple[Descriptor, ...]) -> Listing[Snapshot]:
            def captured(context: bpy.types.Context) -> "BlenderResult[tuple[bpy.types.bpy_struct[object], Snapshot]]":
                return (
                    root_in(self.scope.owner_type, context)
                    .map(lambda owner: Snapshot.capture(owner, Some(selected)).map(lambda snapshot: (owner, snapshot)))
                    .default_with(lambda: Error((BlenderAbsentFault(owner="bpy.context", key=self.scope.owner_type.__name__),)))
                )

            def finished(outcomes: tuple[Applied, ...]) -> BlenderResult[Outcome]:
                owner = self.scope.owner_type.__name__
                if not outcomes:
                    return Ok(Unchanged(reason=f"No {owner} in scope"))
                absent = (Applied(outcome=Error((BlenderAbsentFault(owner=owner, key=path),))) for path in dict.fromkeys(path for outcome in outcomes for path in outcome.skipped))
                return answered(Applied.join((*outcomes, *absent)), "The settings already hold the preset")

            return Listing(
                preset=self.preset,
                current=lambda context: captured(context).map(lambda pair: pair[1]),
                matches=lambda context, stored: captured(context).bind(lambda held: accumulated(value.projected(held[0], held[1]).map(lambda pair: pair[1] == held[1]) for value in stored)),
                applied=lambda context, stored: stored.apply_all(self.scope, context).bind(finished),
            )

        return accumulated(Descriptor.resolve(self.scope.owner_type, path, bpy.types.Property) for path in self.selection).map(listed)


class PresetMenu[T: msgspec.Struct = Snapshot](msgspec.Struct, frozen=True, kw_only=True):
    """Searchable preset listing with optional trailing native content."""

    source: Store[T] | Listing[T] | Captured
    entries: tuple[RowContent, ...] = ()

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Retains the source's resources before realizing its preset menu."""

        def listed[S: msgspec.Struct](listing: Listing[S], owned: tuple[RowContent, ...]) -> BlenderResult[Acquisitions]:
            return Ok((
                *acquired(owned),
                (Band.INTERFACE, registered(identity, listing.key, f"{bpy.path.display_name(listing.preset.key)} Presets", "", lambda: placed(owned, identity).map(painted), search=True)),
            ))

        match self.source:
            case Store() as store:

                @contextmanager
                def registered_store() -> Generator[BlenderResult[None]]:
                    with ExitStack() as stack:

                        def held(listing: Listing[T]) -> BlenderResult[None]:
                            return stack.enter_context(
                                registered(
                                    identity, listing.key, f"{bpy.path.display_name(listing.preset.key)} Presets", "", lambda: placed((listing, *self.entries), identity).map(painted), search=True
                                )
                            )

                        result: BlenderResult[None] = store.bound(identity).bind(lambda slots: slots.listing.map(held).default_value(Result[None, BlenderFaults](ok=None)))
                        yield result

                return Ok((store.attachment, store, *acquired(self.entries), (Band.INTERFACE, registered_store())))
            case Listing() as listing:
                return listed(listing, (listing, *self.entries))
            case Captured() as captured:
                return captured.listing().bind(lambda listing: listed(listing, (listing, *self.entries)))


# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def registered(
    identity: Identity, key: Key, title: str, description: str, drawing: Callable[[], BlenderResult[SurfaceDraw]], *, search: bool, condition: Option[Condition] = Nothing
) -> Generator[BlenderResult[None]]:
    """Realizes and registers a menu after its nested declarations enter their native bands."""

    def searched(draw: SurfaceDraw, heading: RowDraw) -> SurfaceDraw:
        def shown(layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
            if layout.operator_context == "EXEC_REGION_WIN":
                heading(layout, context, context)
                layout.separator()
            draw(layout, context)

        return shown

    def built(draw: SurfaceDraw, name: str, heading: Option[RowDraw]) -> type[bpy.types.Menu]:
        paint = heading.map(lambda held: searched(draw, held)).default_value(draw)

        def shown(menu: bpy.types.Menu, context: bpy.types.Context) -> None:
            for layout in Option.of_optional(menu.layout).to_list():
                paint(layout, context)

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
                        realized(OperatorButton(target=Stock(operator=bpy.ops.wm.search_single_menu, values={"menu_idname": name}), text=Some("Search..."), icon=Some("VIEWZOOM")), identity).map(Some)
                        if search
                        else Ok(Nothing)
                    ).map(lambda heading: built(draw, name, heading))
                )
            )
            .bind(lambda native: entered(stack, (native,)))
        )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Captured", "Dropdown", "Keyed", "Menu", "Pie", "PresetMenu"]
