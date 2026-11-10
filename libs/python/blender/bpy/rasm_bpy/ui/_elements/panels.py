"""Panels an extension registers, its add-on preferences page included."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from functools import partial
import re
from typing import Final, Generic, Literal, Self, TYPE_CHECKING, TypeVar

import _bpy_types  # ruff: ignore[import-private-name]
from bl_ui.space_properties import PropertiesAnimationMixin
import bpy
from expression import Option, Some
from expression.extra.result import catch
import msgspec

from rasm_bpy.commands import invocations, keymaps, operators
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions, Declaration, entered, Gathering, Tier
from rasm_bpy.data._rna.conditions import Condition, satisfied
from rasm_bpy.data._rna.paths import rna_definition, Struct
from rasm_bpy.data._rna.properties import declaring
from rasm_bpy.settings._attachments import Attached, Attachment
from rasm_bpy.ui._drawing.handlers import Signal
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import layout_method, OperatorButton, Property, realized, RowContent, RowDraw
from rasm_bpy.ui._layout.sections import field_rows, Section, SectionDraws, sections_in
from rasm_bpy.ui._resources.icons import ICONS
from rasm_bpy.ui._screens.areas import Located
from rasm_records import accumulated, Error, Fault, Nothing, Ok, Outcome, paired, Reason, typed

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type FixedPlacement = Beside | Child | Sidebar | EditorRegion
type _PanelPlacement = FixedPlacement | _DynamicPlacement
_G_co = TypeVar("_G_co", bound=bpy.types.PropertyGroup, covariant=True, default=bpy.types.PropertyGroup)

# --- [MODELS] ---------------------------------------------------------------------------

# --- [PLACEMENT]


class Beside(msgspec.Struct, frozen=True):
    """Place of a stock panel: its editor and region, with the Properties tab or Sidebar tab it declares."""

    panel: type[_bpy_types.Panel]

    def members(self, _identity: Identity) -> dict[str, object]:
        """Copies the stock panel's editor, region, and whichever of tab context and Sidebar category it declares."""
        declared: type[bpy.types.Panel] = self.panel  # ty: ignore[invalid-assignment]
        tabs = {"bl_context": catch(lambda: declared.bl_context, exception=AttributeError)(), "bl_category": catch(lambda: declared.bl_category, exception=AttributeError)()}
        return {"bl_space_type": declared.bl_space_type, "bl_region_type": declared.bl_region_type, **{member: tab for member, held in tabs.items() for tab in held.to_option().to_list()}}


class Child(msgspec.Struct, frozen=True, kw_only=True):
    """Place under a stock parent panel, which draws its children whatever their category."""

    parent: type[_bpy_types.Panel]

    def members(self, _identity: Identity) -> dict[str, object]:
        """Copies the parent's editor and region and names it as parent."""
        parent: type[bpy.types.Panel] = self.parent  # ty: ignore[invalid-assignment]
        return {"bl_space_type": parent.bl_space_type, "bl_region_type": parent.bl_region_type, "bl_parent_id": rna_definition(parent).identifier}


class Sidebar(msgspec.Struct, frozen=True, kw_only=True):
    """Extension Sidebar tab of an editor, marked with the extension's brand icon."""

    space: "rna_enums.SpaceTypeItems"
    category: str

    def members(self, identity: Identity) -> dict[str, object]:
        """Places the panel in the tab, its icon the brand icon while one is packaged."""
        mark = ICONS.get(identity).bind(lambda icons: icons.brand)
        return {"bl_space_type": self.space, "bl_region_type": "UI", "bl_category": self.category, **mark.map(lambda value: {"bl_icon_value": value}).default_value(dict[str, int]())}


class EditorRegion(msgspec.Struct, frozen=True, kw_only=True):
    """Editor region laying out panels with no tab."""

    space: "rna_enums.SpaceTypeItems"
    region: "rna_enums.RegionTypeItems"

    def members(self, _identity: Identity) -> dict[str, object]:
        """Places the panel in the region."""
        return {"bl_space_type": self.space, "bl_region_type": self.region}


class _DynamicPlacement(msgspec.Struct, frozen=True, kw_only=True):
    """Placement read at registration and again after each `changed` firing."""

    read: Callable[[Identity], Outcome[FixedPlacement]]
    changed: Signal


class PanelCopy(msgspec.Struct, frozen=True, kw_only=True):
    """Further placement registering its own copy of the panel, with its own poll condition."""

    at: FixedPlacement
    condition: Option[Condition] = Nothing


# --- [PANELS]


class Panel(msgspec.Struct, frozen=True, kw_only=True):
    """Section registered as a panel at its placement and at each of `copies`, with child panels and a popover panel per store values popover its sections reach.

    `condition` hides the panel at its own placement while it fails, as does a section holder the context does not reach, and `units` sets its width as a popover.
    """

    section: Section[bpy.types.PropertyGroup]
    at: _PanelPlacement
    condition: Option[Condition] = Nothing
    copies: tuple[PanelCopy, ...] = ()
    children: tuple[Section[bpy.types.PropertyGroup], ...] = ()
    options: frozenset[Literal["HIDE_HEADER", "HEADER_LAYOUT_EXPAND"]] = frozenset()
    units: Option[int] = Nothing

    @classmethod
    def popovers(cls, rows: Iterable[object]) -> tuple[Self, ...]:
        """Returns a popover panel per store values popover the rows' sections reach, opened by a popover button or `wm.call_panel` alone."""
        return tuple(cls(section=popover, at=POPOVER) for section in sections_in(rows) for popover in section.popover.to_list())

    def acquisitions(self, identity: Identity) -> Acquisitions:
        """Returns the sections, a popover panel per values popover they reach, and the scope registering every placement."""
        roots = (self.section, *self.children)
        return (*roots, *Panel.popovers(roots), (Tier.INTERFACE, _displayed(self, identity)))


# --- [PREFERENCES]


class Preference(msgspec.Struct, Generic[_G_co], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """Form group a row keeps on its extension's add-on preferences under its key, the preferences page drawing its fields and then `rows` in one section titled after the key.

    Row needing user preferences declares its preference among what it acquires, and every preference of one key holds one form.
    """

    key: Key
    form: type[_G_co]
    rows: tuple[RowContent, ...] = ()

    @staticmethod
    def acquisitions(_identity: Identity, /) -> Acquisitions:
        """Acquires nothing of its own, the preferences page gathering it."""
        return ()

    def read(self, identity: Identity, /) -> Outcome[_G_co]:
        """Returns the group the extension's add-on preferences hold under the key, absent fault while the extension holds none."""
        return (
            identity
            .preferences(bpy.types.AddonPreferences)
            .to_result_with(lambda: Fault.of(Reason.ABSENT, identity.package, member=self.key))
            .bind(lambda held: typed(self.form, repr(held), getattr(held, self.key)))
        )


class PreferencesPage(msgspec.Struct, frozen=True, kw_only=True):
    """Add-on preferences page over the preferences it and every other row declare, with its own sections, firing `changed` on each edit.

    Page derives one section per preference, then its own sections, Confirmations turning a dismissed confirmation back on,
    Shortcuts over each keymap, and Stored Settings clearing every attachment's stored values, each derived section present while what it draws is declared.
    Each suppressible command's `ask_<key>` flag sits on the preferences beside the preference groups.
    """

    preferences: tuple[Preference[bpy.types.PropertyGroup], ...] = ()
    sections: tuple[Section[bpy.types.PropertyGroup], ...] = ()
    changed: Signal = msgspec.field(default_factory=Signal)

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Preferences the page declares."""
        return self.preferences

    @contextmanager
    def gathered(self, identity: Identity, declared: tuple[Declaration | Gathering, ...], /) -> Generator[Acquisitions]:
        """Holds preference callbacks while the preferences class, commands, sections, and value popovers register."""
        asks = tuple(row for row in declared if isinstance(row, operators.Operator) and any(isinstance(form, invocations.Suppressible) for form in row.invoke.to_list()))
        bindings = tuple(row for row in declared if isinstance(row, keymaps.Keymap))
        stores = tuple(row for row in declared if isinstance(row, Attachment))
        preferred = tuple(row for row in declared if isinstance(row, Preference))

        def attached[V](read: Callable[[Attached[bpy.types.PropertyGroup]], V]) -> Outcome[tuple[V, ...]]:
            return accumulated(row.bound(identity) for row in stores).map(lambda held: tuple(map(read, held)))

        def clear_stored_settings(_call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
            """Resets extension settings stored on data-blocks to defaults."""
            return attached(Attached.purged).map(
                lambda counts: (
                    operators.Done(notes=(("INFO", bpy.app.translations.pgettext_rpt("Cleared settings stored on {} data-blocks").format(count)),))
                    if (count := sum(counts))
                    else operators.Unchanged(reason="No data-block stores extension settings")
                )
            )

        confirmation = invocations.Popup[operators.Empty](
            partial(bpy.types.WindowManager.invoke_confirm, confirm_text="Clear", icon="WARNING"),
            message=Some(
                lambda *_: attached(lambda held: sum(held.stored(host) for host in held.hosts())).map(
                    lambda counts: bpy.app.translations.pgettext_iface("Clear settings stored on {} data-blocks").format(sum(counts))
                )
            ),
        )
        clear = operators.Operator.of(operators.Empty, icon=Some("TRASH"), options=frozenset({"INTERNAL", "UNDO"}), invoke=Some(confirmation))(clear_stored_settings)

        def changed(_owner: Struct, _context: bpy.types.Context) -> None:
            identity.reports.faulted(self.changed.fired())

        flags = {invocations.Suppressible.flag(row.key): bpy.props.BoolProperty(name=row.title, description=row.description, default=True, update=changed) for row in asks}
        bodies: list[RowDraw] = []

        def draw(holder: bpy.types.AddonPreferences, context: bpy.types.Context) -> None:
            layout = holder.layout
            layout.use_property_split, layout.use_property_decorate = True, False
            for body in bodies:
                body(layout, holder, context)

        pointers = {row.key: bpy.props.PointerProperty(type=row.form, name=bpy.path.display_name(row.key)) for row in preferred}
        kind = type(identity.package, (bpy.types.AddonPreferences,), {"bl_idname": identity.package, "__annotations__": {**pointers, **flags}, "draw": draw})
        sections = (
            *(
                Section(key=f"preferences_{row.key}", title=Some(bpy.path.display_name(row.key)), holder=Some(Member(root=kind, path=(row.key,))), rows=(*field_rows(row.form, row.key), *row.rows))
                for row in preferred
            ),
            *self.sections,
            *((Section(key="preferences_confirmations", title=Some("Confirmations"), rows=tuple(Property(target=Member(root=kind, path=(ask,))) for ask in flags)),) if flags else ()),
            *((Section(key="preferences_shortcuts", title=Some("Shortcuts"), rows=tuple(binding.drawn for binding in bindings)),) if bindings else ()),
            *((Section(key="preferences_stored", title=Some("Stored Settings"), rows=(OperatorButton(target=clear),)),) if stores else ()),
        )

        @contextmanager
        def prepared() -> Generator[Outcome[None]]:
            with ExitStack() as stack:
                stack.callback(bodies.clear)
                yield entered(identity, stack, (kind,)).bind(lambda _: realized(sections, identity)).map(bodies.append)

        with ExitStack() as stack:
            groups = tuple(group for row in preferred for group in stack.enter_context(declaring(row.form, {}, Some(changed))))
            yield (*groups, show_preferences, *((clear,) if stores else ()), *sections, *Panel.popovers(sections), (Tier.PREFERENCES, prepared()))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [REGISTRATION]


@contextmanager
def _displayed(panel: Panel, identity: Identity) -> Generator[Outcome[None]]:
    """Registers the panel classes at every placement for the scope, re-registering a docked placement after each `changed` firing that moves it."""

    def slug(at: FixedPlacement) -> str:
        """Returns a copy's key suffix: the stock panel's identifier, or the editor with its tab or region, in lowercase snake case."""
        match at:
            case Beside(panel=stock) | Child(parent=stock):
                text = stock.__name__
            case Sidebar(space=space, category=named) | EditorRegion(space=space, region=named):
                text = f"{space} {named}"
        return "_".join(match.group() for match in re.finditer(r"[a-z0-9]+", text.lower()))

    sites = ((panel.section.key, panel.at, panel.condition), *((f"{panel.section.key}_{slug(copy.at)}", copy.at, copy.condition) for copy in panel.copies))
    drawn = paired(panel.section.realized(identity), accumulated(child.realized(identity) for child in panel.children))
    held: list[tuple[FixedPlacement, ...]] = []
    classes = ExitStack()

    def classed(
        section: Section[bpy.types.PropertyGroup], draws: SectionDraws, name: str, members: Mapping[str, object], options: frozenset[str], condition: Option[Condition]
    ) -> type[bpy.types.Panel]:
        """Builds a panel class over the section's draws, polled by the condition and whether the context reaches the section's holder."""

        def poll(_cls: type[bpy.types.Panel], context: bpy.types.Context) -> bool:
            return satisfied(condition, context) and draws.present(context)

        headers = {member: layout_method(draw) for member, shown in (("draw_header", draws.draw_header), ("draw_header_preset", draws.draw_header_preset)) for draw in shown.to_list()}
        return type(
            name,
            (bpy.types.Panel,),
            {
                **members,
                "bl_idname": name,
                "bl_label": section.label,
                "bl_description": section.description,
                "bl_translation_context": section.translation_context,
                "bl_options": {*options, *(() if section.opened else ("DEFAULT_CLOSED",))},
                "poll": classmethod(poll),
                "draw": layout_method(draws.draw),
                **headers,
            },
        )

    def placed(draws: tuple[SectionDraws, tuple[SectionDraws, ...]], stem: str, at: FixedPlacement, condition: Option[Condition]) -> tuple[type[bpy.types.Panel], ...]:
        """Builds the panel class at one placement, ordered above the Animation panel, then its child panel classes in registration order."""
        top, children = draws
        placing = {**at.members(identity), "bl_owner_id": identity.package}
        ordered = {**placing, "bl_order": PropertiesAnimationMixin.bl_order - 1, **panel.units.map(lambda width: {"bl_ui_units_x": width}).default_value({})}
        shared = {key: value for key, value in placing.items() if key != "bl_parent_id"}

        name = identity.name(bpy.types.Panel, stem)
        return (
            classed(panel.section, top, name, ordered, panel.options, condition),
            *(classed(child, each, identity.name(bpy.types.Panel, f"{stem}_{child.key}"), {**shared, "bl_parent_id": name}, frozenset(), Nothing) for child, each in zip(panel.children, children)),
        )

    def resolved() -> Outcome[tuple[FixedPlacement, ...]]:
        return accumulated(at.read(identity) if isinstance(at, _DynamicPlacement) else Ok(at) for _, at, _ in sites)

    def registered(now: tuple[FixedPlacement, ...]) -> Outcome[None]:
        with ExitStack() as entering:
            if (answer := drawn.bind(lambda draws: entered(identity, entering, tuple(cls for (stem, _, condition), at in zip(sites, now) for cls in placed(draws, stem, at, condition))))).is_ok():
                classes.enter_context(entering.pop_all())
                held[:] = [now]
            return answer

    def rebuilt() -> Outcome[Option[str]]:
        def replaced(now: tuple[FixedPlacement, ...]) -> Outcome[None]:
            if held == [now]:
                return Ok(None)
            previous = tuple(held)
            classes.close()
            held.clear()
            return registered(now).or_else_with(lambda faults: paired(Error(faults), accumulated(map(registered, previous))).map(lambda _: None))

        return resolved().bind(replaced).map(lambda _: Nothing)

    with ExitStack() as scope:
        scope.enter_context(classes)
        pending = scope.enter_context(identity.reports.deferred(rebuilt))
        followed = accumulated(scope.enter_context(signal.followed(lambda: Ok(pending()))) for signal in dict.fromkeys(at.changed for _, at, _ in sites if isinstance(at, _DynamicPlacement)))
        yield followed.bind(lambda _: resolved().bind(registered))


# --- [PREFERENCES]


@operators.Operator.of(operators.Empty, icon=Some("PREFERENCES"), options=frozenset())
def show_preferences(call: operators.Call[operators.Empty]) -> Outcome[operators.Answer]:
    """Opens extension preferences from context window and area."""
    return Located.current(call.context).bind(lambda located: located.run(bpy.ops.preferences.addon_show, {"module": call.identity.package})).map(lambda _: operators.Done())


# --- [COMPOSITION] ----------------------------------------------------------------------

POPOVER: Final = EditorRegion(space="TOPBAR", region="HEADER")

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["POPOVER", "Beside", "Child", "EditorRegion", "FixedPlacement", "Panel", "PanelCopy", "Preference", "PreferencesPage", "Sidebar", "show_preferences"]
