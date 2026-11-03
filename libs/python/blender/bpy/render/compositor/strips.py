"""Compositor effect groups used outside the scene stack: sequencer strip modifiers and strips running standalone effects, and the active effect group saved into user asset libraries."""

from contextlib import ExitStack
from enum import Enum
from functools import cached_property, partial
from pathlib import Path
from typing import ClassVar, Final

from bl_ui.space_sequencer import SEQUENCER_MT_modifier_add
import bpy
from expression import Error, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import PyOpenColorIO

from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import Answer, Call, Done, NoUndo, Operator, Placement, PopupKind, PopupTarget
from blender.bpy.data.ids.ownership import retained
from blender.bpy.data.rna.conditions import AllOf, Present, Setting
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import rna, RnaPath, rooted, Struct
from blender.bpy.data.rna.properties import Callbacks, choices, EnumItem, held_choices, rebound
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.files.assets import AssetMark, published
from blender.bpy.nodes.changes import made
from blender.bpy.render.compositor.effects import CompositorEffect, Entry
from blender.bpy.render.compositor.offered import Choice, OfferedEffects
from blender.bpy.ui.elements.menus import Append, Dropdown, Menu
from blender.bpy.ui.layout.rows import Opens, OperatorButton
from blender.records.faults import Fault, Faults, Outcome, paired, Reason
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------


class StripHolder(Enum):
    """What runs an effect on the active strip, valued by its description."""

    MODIFIER = "Compositor strip modifier on the active strip"
    STRIP = "Compositor strip over the active strip"


# --- [CONSTANTS] ------------------------------------------------------------------------

_ACTIVE: Final[RnaPath] = ("workspace", "sequencer_scene", "sequence_editor", "active_strip")
_STRIP: Final[Key] = "strip"
_COMPOSITOR: Final = "COMPOSITOR"

# --- [CHOICES] --------------------------------------------------------------------------


def libraries(_owner: Struct, context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
    """User asset library items of the context preferences."""
    return held_choices(
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

    library: bpy.props.EnumProperty(name="Library", items=libraries)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    catalog: bpy.props.StringProperty(name="Catalog", default="Effects")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class Strips[R: Case](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Strip placement command for the standalone effects of a set of rows, the menu listing them that the stock strip modifier add menu opens, and the command saving the active effect group as an asset."""

    active: ClassVar[Descriptor[bpy.types.PointerProperty]] = Descriptor.declared(bpy.types.Context, _ACTIVE, bpy.types.PointerProperty)
    placeable: ClassVar[AllOf] = Present(descriptor=active) & ~Setting(descriptor=Descriptor.declared(bpy.types.Context, (*_ACTIVE, "type"), bpy.types.EnumProperty), accepted=("SOUND",))
    rows: OfferedEffects[R]

    @cached_property
    def placing(self) -> Operator[StripChoice]:
        """Command running the chosen standalone effect on the active strip, an item per standalone row under its key, title, and record documentation."""
        form = rebound(StripChoice, kind=Callbacks(items=choices((row.kind, row.title, row.description) for row in self.rows.standalone)))
        return Operator(condition=Some(Strips.placeable), form=form, run=self.add_strip_effect)

    @cached_property
    def menu(self) -> Menu:
        """Menu of every standalone effect, each placing it on the active strip."""
        return Menu(
            key=self.placing.key,
            description=self.placing.description,
            condition=Some(Strips.placeable),
            body=Dropdown(entries=tuple(OperatorButton(target=Placement(self.placing, {"kind": row.kind}), keywords={"text": row.title}) for row in self.rows.standalone), search=True),
        )

    def add_strip_effect(self, call: Call[StripChoice]) -> Outcome[Answer]:
        """Run chosen effect on active strip as a modifier or a strip over it."""
        match Strips.active.owner_of(call.context).to_optional():
            case bpy.types.SequenceEditor(active_strip=bpy.types.Strip() as strip, id_data=bpy.types.Scene() as scene) as editor:
                target: Outcome[tuple[bpy.types.Scene, bpy.types.SequenceEditor, bpy.types.Strip]] = Ok((scene, editor, strip))
            case _:
                target = Error(Fault.of(Reason.ABSENT, repr(call.context.workspace), member=rna(_ACTIVE[1:])))
        offered = self.rows.offering(call.properties.kind)

        def built(scene: bpy.types.Scene, editor: bpy.types.SequenceEditor, strip: bpy.types.Strip, effect: CompositorEffect[R], record: R) -> Outcome[Answer]:
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
                    effect
                    .grouped(call.identity, scene, Entry(record=record), PyOpenColorIO.GetCurrentConfig())
                    .bind(lambda group: made(msgspec.structs.replace(group, key=f"{group.key}_{_STRIP}"), call.identity, of=Some(scene)))
                    .bind(
                        lambda group: Result[bpy.types.Strip | bpy.types.StripModifier, Faults].bind(
                            catch(held, exception=RuntimeError)().map_error(partial(Fault.of, Reason.REFUSED, repr(strip))), lambda holder: written(holder, ("node_group",), group)
                        )
                    )
                )
                return retained(placed, rollback).map(lambda _: Done(notes=(("INFO", f"Added {effect.title} to {strip.name}"),)))

        return paired(target, offered).bind(lambda pair: rooted(bpy.types.Scene, call.context).bind(pair[1].seeded).bind(lambda record: built(*pair[0], pair[1], record)))

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns strip group ownerships, the placement command, its menu with the stock strip modifier add menu opening it, and the asset save command."""
        return Ok((
            *(msgspec.structs.replace(owner, key=f"{owner.key}_{_STRIP}") for row in self.rows.standalone for owner in row.ownerships),
            self.placing,
            self.menu,
            Append(target=SEQUENCER_MT_modifier_add, rows=(Opens(target=PopupTarget(self.menu.key, PopupKind.MENU)),)),
            save_effect,
        ))


# --- [COMPOSITION] ----------------------------------------------------------------------


@Operator.of(Saving, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))), undo=NoUndo())
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

    def stored(group: bpy.types.NodeTree, folder: Path) -> Outcome[Answer]:
        return (
            written(group, ("allow_usage_in_scene_compositor_effect",), True)
            .bind(lambda _: published(call.identity, folder, bpy.path.clean_name(group.name), ((group, AssetMark(key=save_effect.key, catalog=chosen.catalog)),), kept=True))
            .map(lambda path: Done(notes=(("INFO", f"Saved {group.name} to {path}"),)))
        )

    return paired(group, folder).bind(lambda pair: stored(*pair))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Saving", "StripChoice", "StripHolder", "Strips", "libraries", "save_effect"]
