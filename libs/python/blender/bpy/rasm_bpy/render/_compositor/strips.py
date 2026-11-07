"""Compositor effect groups used outside the scene stack: sequencer strip modifiers and strips running standalone effects, and the active effect group saved into user asset libraries."""

from contextlib import ExitStack
from enum import Enum
from functools import cached_property, partial
from pathlib import Path
from typing import ClassVar, Final

from bl_ui.space_sequencer import SEQUENCER_MT_modifier_add
import bpy
from expression import Option, Result, Some
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from rasm_bpy.commands.invocations import Popup
from rasm_bpy.commands.operators import Answer, Call, Done, Operator, Placement, PopupKind, PopupTarget
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._ids.ownership import _retained
from rasm_bpy.data._rna.conditions import AllOf, Present, Setting
from rasm_bpy.data._rna.descriptors import Descriptor, written
from rasm_bpy.data._rna.paths import _rna, _struct_at, id_data, RnaPath, rooted, Struct
from rasm_bpy.data._rna.properties import Callbacks, choices, EnumItem
from rasm_bpy.files._assets import _AssetMark, _published
from rasm_bpy.nodes.changes import made
from rasm_bpy.render._compositor.effects import CompositorEffect, CompositorRecord, Entry
from rasm_bpy.render._compositor.offered import Choice, OfferedEffects
from rasm_bpy.ui._elements.menus import Append, Dropdown, Menu
from rasm_bpy.ui._layout.rows import _operator_button, Layout, Opens
from rasm_imaging import called
from rasm_records import accumulated, Error, Fault, Faults, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------


class StripHolder(Enum):
    """What runs an effect on the active strip, valued by its description."""

    MODIFIER = "Compositor strip modifier on the active strip"
    STRIP = "Compositor strip over the active strip"


# --- [CONSTANTS] ------------------------------------------------------------------------

_ACTIVE: Final[RnaPath] = ("workspace", "sequencer_scene", "sequence_editor", "active_strip")
_ACTIVE_PROPERTY: Final[bpy.types.PointerProperty] = bpy.types.SequenceEditor.bl_rna.properties[_ACTIVE[-1]]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_STRIP: Final[Key] = "strip"
_COMPOSITOR: Final = "COMPOSITOR"

# --- [CHOICES] --------------------------------------------------------------------------


def _libraries(_owner: Struct, context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
    """User asset library items of the context preferences."""
    return choices(
        tuple(
            (row.name, row.name, row.path)
            for preferences in Option.of_optional(context).bind(lambda held: Option.of_optional(held.preferences)).to_list()
            for row in preferences.filepaths.asset_libraries
            if isinstance(row, bpy.types.PreferencesAssetLibrary)
        )
    )


# --- [MODELS] ---------------------------------------------------------------------------


class StripChoice(Choice):
    """Effect placement on active sequencer strip."""

    placement: bpy.props.EnumProperty(name="Placement", items=choices((holder.name, bpy.path.display_name(holder.name.lower()), holder.value) for holder in StripHolder))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Saving(bpy.types.PropertyGroup):
    """User asset library and catalog for active effect group."""

    library: bpy.props.EnumProperty(name="Library", items=_libraries)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    catalog: bpy.props.StringProperty(name="Catalog", default="Effects")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class Strips(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Strip placement command for the standalone effects of a set of rows, the menu listing them that the stock strip modifier add menu opens, and the command saving the active effect group as an asset."""

    active: ClassVar[Descriptor[bpy.types.PointerProperty]] = Descriptor(root=bpy.types.Context, owner=_ACTIVE[:-1], prop=_ACTIVE_PROPERTY)
    placeable: ClassVar[AllOf] = Present(descriptor=active) & ~Setting(
        descriptor=Descriptor(root=bpy.types.Context, owner=_ACTIVE, prop=bpy.types.Strip.bl_rna.properties["type"]), accepted=("SOUND",)
    )
    rows: OfferedEffects

    @cached_property
    def placing(self) -> Operator[StripChoice]:
        """Command running the chosen standalone effect on the active strip, an item per standalone row under its key, title, and record documentation."""
        return Operator(condition=Some(Strips.placeable), form=StripChoice, callbacks=Some(lambda identity: {"kind": Callbacks(items=partial(self.choices, identity))}), run=self.add_strip_effect)

    def choices(self, identity: Identity, _owner: StripChoice, context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
        """Native choices from the actual sequencer scene, logging seed failures."""
        return (
            Option
            .of_optional(context)
            .bind(lambda context: _struct_at(context, Strips.active.owner, bpy.types.SequenceEditor))
            .bind(partial(id_data, bpy.types.Scene))
            .bind(identity.reports.reported(self.rows.standalone))
            .map(lambda rows: choices((row.kind, row.title, row.description) for row in rows))
            .default_value(())
        )

    @cached_property
    def menu(self) -> Menu:
        """Menu of every standalone effect, each placing it on the active strip."""
        return Menu(
            key=self.placing.key,
            description=self.placing.description,
            condition=Some(Strips.placeable),
            body=Dropdown(entries=(Layout(builder=Some(lambda identity: Ok((partial(self.menu_drawn, identity),)))),), search=True),
        )

    def menu_drawn(self, identity: Identity, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the scene's standalone effects after their seed and pass requirements resolve."""
        rows = (
            _struct_at(context, Strips.active.owner, bpy.types.SequenceEditor)
            .bind(partial(id_data, bpy.types.Scene))
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(context.workspace), member=_rna(_ACTIVE[1:])))
            .bind(self.rows.standalone)
        )
        identity.reports.faulted(rows.bind(lambda found: accumulated(_operator_button(layout, identity, Placement(self.placing, {"kind": row.kind}), {"text": row.title}) for row in found)))

    def add_strip_effect(self, call: Call[StripChoice]) -> Outcome[Answer]:
        """Run chosen effect on active strip as a modifier or a strip over it."""
        match Strips.active.owner_of(call.context).to_optional():
            case bpy.types.SequenceEditor(active_strip=bpy.types.Strip() as strip, id_data=bpy.types.Scene() as scene) as editor:
                target: Outcome[tuple[bpy.types.Scene, bpy.types.SequenceEditor, bpy.types.Strip]] = Ok((scene, editor, strip))
            case _:
                target = Error(Fault.of(Reason.ABSENT, repr(call.context.workspace), member=_rna(_ACTIVE[1:])))
        offered = self.rows.offering(call.properties.kind)

        def built(scene: bpy.types.Scene, editor: bpy.types.SequenceEditor, strip: bpy.types.Strip, effect: CompositorEffect, record: CompositorRecord) -> Outcome[Answer]:
            with ExitStack() as rollback:

                def held() -> bpy.types.Strip | bpy.types.StripModifier:
                    holder: bpy.types.Strip | bpy.types.StripModifier
                    match StripHolder[call.properties.placement]:
                        case StripHolder.MODIFIER:
                            holder = strip.modifiers.new(name=effect.title, type=_COMPOSITOR)
                            rollback.callback(strip.modifiers.remove, holder)
                        case StripHolder.STRIP:
                            holder = editor.strips.new_effect(name=effect.title, type=_COMPOSITOR, channel=strip.channel + 1, frame_start=strip.left_handle, length=strip.duration, input1=strip)
                            rollback.callback(editor.strips.remove, holder)
                    return holder

                placed = (
                    called(PyOpenColorIO.GetCurrentConfig, effect.kind)
                    .bind(lambda config: effect.grouped(call.identity, scene, Entry(record=record), config))
                    .bind(lambda group: made(msgspec.structs.replace(group, key=f"{group.key}_{_STRIP}"), call.identity, of=Some(scene)))
                    .bind(
                        lambda group: Result[bpy.types.Strip | bpy.types.StripModifier, Faults].bind(
                            catch(held, exception=RuntimeError)().map_error(partial(Fault.of, Reason.REFUSED, repr(strip))), lambda holder: written(holder, ("node_group",), group)
                        )
                    )
                )
                return _retained(placed, rollback).map(lambda _: Done(notes=(("INFO", f"Added {effect.title} to {strip.name}"),)))

        return paired(target, offered).bind(lambda pair: pair[1].seeded(pair[0][0]).bind(lambda record: built(*pair[0], pair[1], record)))

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns strip group ownerships, the placement command, its menu with the stock strip modifier add menu opening it, and the asset save command."""
        return (
            *(msgspec.structs.replace(owner, key=f"{owner.key}_{_STRIP}") for row in self.rows.rows for owner in row.ownerships),
            self.placing,
            self.menu,
            Append(target=SEQUENCER_MT_modifier_add, rows=(Opens(target=PopupTarget(self.menu.key, PopupKind.MENU)),)),
            save_effect,
        )


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Saving, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), options=frozenset())
def save_effect(call: Call[Saving]) -> Outcome[Answer]:
    """Save active effect to user asset library."""
    chosen = call.properties
    group = rooted(bpy.types.Scene, call.context).bind(
        lambda scene: (
            Option
            .of_optional(scene.compositor_effects.active)
            .bind(lambda entry: Option.of_optional(entry.node_group))
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member="active"))
        )
    )
    folder = rooted(bpy.types.Preferences, call.context).bind(
        lambda preferences: (
            Option
            .of_optional(preferences.filepaths.asset_libraries.get(chosen.library))
            .map(lambda row: Path(row.path))
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(preferences.filepaths.asset_libraries), member=chosen.library))
        )
    )

    def stored(mark: _AssetMark, group: bpy.types.NodeTree, folder: Path) -> Outcome[Answer]:
        return (
            written(group, ("allow_usage_in_scene_compositor_effect",), value=True)
            .bind(lambda _: _published(call.identity, folder, bpy.path.clean_name(group.name), ((group, mark),), kept=True))
            .map(lambda path: Done(notes=(("INFO", f"Saved {group.name} to {path}"),)))
        )

    return (
        catch(lambda: _AssetMark(key=save_effect.key, catalog=chosen.catalog), exception=msgspec.ValidationError)()
        .map_error(partial(Fault.of, Reason.INVALID, chosen.catalog))
        .bind(lambda mark: paired(group, folder).bind(lambda pair: stored(mark, *pair)))
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Saving", "StripChoice", "StripHolder", "Strips", "save_effect"]
