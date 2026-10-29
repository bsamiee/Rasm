"""Layer stack composition over the effects an extension offers: presets, the operators of every stack command, menus, the editor section with its layer list, the preset section, and the layered document port."""

from collections.abc import Callable, Mapping
from contextlib import AbstractContextManager, nullcontext
from enum import Enum
from functools import cached_property, partial
from types import GenericAlias, MappingProxyType
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec

from blender.bpy.commands.calls import Called, Command, Opened, Placement
from blender.bpy.commands.operators import Answer, Done, Unchanged
from blender.bpy.data.rna.mirrors import Changed, held_choices
from blender.bpy.data.rna.paths import root_in, rooted, Struct
from blender.bpy.extension.events import Handler, Subscription
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.addressing import Addressed, owned, slot
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor import stack_commands
from blender.bpy.render.compositor.blending import Entry
from blender.bpy.render.compositor.documents import layered
from blender.bpy.render.compositor.edits import chosen
from blender.bpy.render.compositor.effects import CompositorEffect
from blender.bpy.render.compositor.entries import EffectRows
from blender.bpy.render.compositor.inputs import captured, entry_input, FRAMING
from blender.bpy.render.compositor.layers import Edit, EffectAt, EffectMaskAt, EFFECTS, LayerAt, LayerMaskAt, LayerStack, Source, StackLayer, Target, TermFiltering
from blender.bpy.render.compositor.recipes import Valued
from blender.bpy.render.compositor.stack_commands import Commanding, Copied, copying, Forms, SOURCING, STACKING, switching
from blender.bpy.render.compositor.stacking import LayerRow, LayerRows, LayerStacking, OWNERSHIP, Selection, StoredLayer, TITLE, VIEW
from blender.bpy.render.outputs.exports import Documents
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.view_layers.passes import PassFlags
from blender.bpy.settings.attachments import Attachment, Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.presets import Listing
from blender.bpy.ui.layout.rows import alerted, Layout, Opens, OperatorButton, pressed, Property, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.surfaces.menus import Dropdown, Menu, Submenus
from blender.imaging.color.processors import DisplayView
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type _Listings[M: Case] = tuple[Listing[Valued[LayerStack[Source, M]]], Listing[Valued[StackLayer[Source, M]]], Listing[Valued[Entry[M]]]]
type _Preset[M: Case] = Listing[Valued[LayerStack[Source, M]]] | Listing[Valued[StackLayer[Source, M]]] | Listing[Valued[Entry[M]]]
type Operators = Mapping[object, Command]

# --- [CONSTANTS] ------------------------------------------------------------------------

_ADDING: Final[Key] = "layer_add"
_EFFECTING: Final[Key] = "layer_effect_add"
_SPECIALS: Final[Key] = f"{OWNERSHIP.key}_specials"
_WATCHED: Final = tuple(
    (owner, owner.bl_rna.properties[name])
    for owner, name in ((bpy.types.RenderSettings, "film_transparent"), (bpy.types.ViewLayer, "name"), (bpy.types.ViewLayer, "use"), (bpy.types.Object, "lightgroup"), (bpy.types.World, "lightgroup"))
)

# --- [COMPOSITION] ----------------------------------------------------------------------


class LayerStacks[M: Case](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Layer stacks over the effects an extension offers: the rows a layer accepts and how its add menu groups them, stock looks, term filters, and the document view."""

    rows: EffectRows[M]
    accepts: Callable[[CompositorEffect[M]], bool] = lambda _row: True
    groups: Option[Callable[[CompositorEffect[M]], Enum]] = Nothing
    looks: Mapping[str, Valued[LayerStack[Source, M]]] = MappingProxyType({})
    filtering: TermFiltering[M] = TermFiltering()
    view: Callable[[Identity, bpy.types.Scene], Outcome[Option[DisplayView]]] = lambda _identity, _scene: Ok(Nothing)

    # --- [DECLARATIONS]

    @cached_property
    def form(self) -> type[LayerStack[Source, M]]:
        """Stack record over the offered effect records."""
        return GenericAlias(LayerStack, (Source, self.rows.form))  # type: ignore[return-value]  # ty: ignore[invalid-return-type]

    @cached_property
    def accepted(self) -> tuple[CompositorEffect[M], ...]:
        """Effects a layer accepts in rank order."""
        return tuple(sorted(filter(self.accepts, self.rows.rows), key=self.rows.rank))

    @cached_property
    def refresh(self) -> Resource[Callable[[], None]]:
        """Deferred refresh of every stack of an extension, the one queue layer row edits, scene changes, and file loads call."""
        return Resource[Callable[[], None]](made=lambda identity: identity.reports.deferred(lambda: self.bound(identity).bind(LayerStacking.refreshed)))

    @cached_property
    def operating(self) -> Resource[Operators]:
        """Operators of every stack command an extension registers, built once per extension, its stack pickers reading that extension's stacks and layer rows."""
        return Resource[Operators](made=lambda identity: nullcontext(self.operators(identity)))

    @cached_property
    def layered(self) -> Attachment[LayerRows]:
        """Layer rows on the stack group owning each layer's name, flags, and order, a row edit queueing the stack refresh."""
        return Attachment(key=f"{OWNERSHIP.key}_layers", form=LayerRows, owner=bpy.types.NodeTree, changed=Some(self.changed))

    def changed(self, identity: Identity) -> AbstractContextManager[Changed]:
        """Change callback of a layer row edit, queueing the extension's stack refresh."""
        return nullcontext(lambda _owner, _context: identity.held[self.refresh]())

    @cached_property
    def preset(self) -> Preset[Valued[LayerStack[Source, M]]]:
        """Stack presets with their input values over the stock looks."""
        return Preset(key=OWNERSHIP.key, form=GenericAlias(Valued, (self.form,)), stock=self.looks)  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    @cached_property
    def layer_preset(self) -> Preset[Valued[StackLayer[Source, M]]]:
        """Presets of one layer with its input values."""
        return Preset(key=f"{OWNERSHIP.key}_layer", form=GenericAlias(Valued, (GenericAlias(StackLayer, (Source, self.rows.form)),)))  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    @cached_property
    def entry_preset(self) -> Preset[Valued[Entry[M]]]:
        """Presets of one effect with its blend and input values."""
        return Preset(key=f"{OWNERSHIP.key}_entry", form=GenericAlias(Valued, (GenericAlias(Entry, (self.rows.form,)),)))  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    # --- [COMMANDS]

    def operators(self, identity: Identity) -> Operators:
        """Builds the operator of every stack command once for an extension, its stack pickers listing the extension's stacks and the chosen stack's layer rows, its effect choice the accepted effects."""

        def stacks(_owner: Struct, _context: bpy.types.Context | None) -> tuple[tuple[str, str, str, str, int], ...]:
            return held_choices(tuple((tree.name, tree.name, "") for tree, _ in OWNERSHIP.held(identity)))

        def layers(owner: Copied, _context: bpy.types.Context | None) -> tuple[tuple[str, str, str, str, int], ...]:
            rows = self.layered.bound(identity).map(
                lambda attached: tuple((row.key, row.name, "") for tree, _ in OWNERSHIP.held(identity) if tree.name == owner.stack for row in attached.struct(tree).layers)
            )
            return held_choices(rows.default_value(()))

        forms = Forms(switched=switching(stacks), copied=copying(stacks, layers), choice=chosen(self.accepted))
        commands: tuple[Commanding, ...] = (
            stack_commands.new_stack,
            *STACKING,
            *SOURCING,
            stack_commands.select_output,
            stack_commands.add_mask,
            stack_commands.edit_layer,
            stack_commands.add_effect_to_layer,
        )
        return MappingProxyType({command: command.operator(self.bound, forms) for command in commands})

    # --- [MENUS]

    def menus(self, operators: Operators) -> tuple[Menu, ...]:
        """Menus adding a layer of each source, appending an accepted effect, one submenu per group where groups are given, and the stack commands beside the layer rows."""
        effects = tuple((row, OperatorButton(target=Placement(operators[stack_commands.add_effect_to_layer], {"kind": row.kind}), keywords={"text": row.title})) for row in self.accepted)
        return (
            Menu(key=_ADDING, title="Add Layer", description="Add layer source to stack", body=Dropdown(entries=tuple(OperatorButton(target=operators[command]) for command in SOURCING))),
            Menu(
                key=_EFFECTING,
                title="Add Effect",
                description="Add effect to selected layer",
                body=self.groups.map(lambda group: Submenus(entries=tuple((group(row), button) for row, button in effects))).default_with(
                    lambda: Dropdown(entries=tuple(button for _, button in effects), search=True)
                ),
            ),
            Menu(
                key=_SPECIALS,
                title=f"{TITLE} Specials",
                description="Stack and layer commands beside the layer rows",
                body=Dropdown(entries=tuple(OperatorButton(target=operators[command]) for command in STACKING)),
            ),
        )

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Stack refresh and operators, view attachment, and layer rows with the frame size binding stacks read."""
        return self.refresh, self.operating, VIEW, self.layered, FrameSize.attachment

    def bound(self, identity: Identity) -> Outcome[LayerStacking[M]]:
        """Binds the store of each layer's structure, the layer rows and view attachments, and pass flags of an extension."""
        stored = Stored.keyed(identity, OWNERSHIP.key, msgspec.msgpack.Decoder(GenericAlias(dict, (Key, GenericAlias(StoredLayer, (Source, self.rows.form))))))
        return paired(stored, self.layered.bound(identity), VIEW.bound(identity), PassFlags.of()).map(
            lambda parts: LayerStacking(identity=identity, rows=self.rows, filtering=self.filtering, store=parts[0], layer_rows=parts[1], view=parts[2], flags=parts[3])
        )

    @cached_property
    def document(self) -> Documents:
        """Layered document of the scene's head stack at a saved frame, formed through the stack view, else the scene's."""
        return lambda identity, scene, frame, depth: paired(self.bound(identity), self.view(identity, scene)).bind(
            lambda parts: layered(parts[0], scene, frame, depth, parts[1].default_with(lambda: viewed(scene)))
        )

    # --- [SECTIONS]

    def section(self, header: tuple[RowContent, ...] = ()) -> Section:
        """Returns the editor section under header rows, presets first."""
        return Section(key=OWNERSHIP.key, title=TITLE, header=(Opens(target=Opened(self.presets().key, Called.PANEL), keywords={"text": ""}, icon=Some("PRESET")), *header), rows=(self,), opened=True)

    def presets(self) -> Section:
        """Returns the section of stack, layer, and effect preset listings."""
        return Section(
            key=f"{OWNERSHIP.key}_presets",
            title=f"{TITLE} Presets",
            description=f"Apply, add, or import {TITLE.lower()}, layer, and effect presets",
            rows=(Layout(rows=lambda identity: self.presets_drawn(identity).map(lambda draw: (draw,)), prerequisites=(self,)),),
        )

    def presets_drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns the preset listing draws of each held stack, layer, or effect, the New Stack button while no stack exists."""

        def listed(stacking: LayerStacking[M]) -> Outcome[RowDraw]:
            stack, layer, entry = _listings(self, stacking)
            creating = identity.held[self.operating][stack_commands.new_stack]
            return accumulated((stack.drawn(identity), layer.drawn(identity), entry.drawn(identity))).map(lambda draws: _presets(stacking, creating, tuple(zip((stack, layer, entry), draws))))

        return self.bound(identity).bind(listed)

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns the draw of the layer list, then the selected layer's source, inputs, and node editors, and one panel per layer effect."""
        return self.bound(identity).bind(
            lambda stacking: paired(_listing(self, stacking).drawn(identity), realized((Opens(target=Opened(_EFFECTING, Called.MENU), icon=Some("ADD")),), identity)).map(
                lambda parts: _editor(self, stacking, *parts)
            )
        )

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the extension's stack operators and menus over them, listings, and the handlers and subscriptions queueing the stack refresh."""
        operators = identity.held[self.operating]

        def updated_scenes() -> Outcome[None]:
            identity.held[self.refresh]()
            return Ok(None)

        def rows(stacking: LayerStacking[M]) -> Acquisitions:
            handlers: tuple[Declaration, ...] = (
                Handler(handlers=bpy.app.handlers.load_post, body=updated_scenes),
                *(Subscription(owner=owner, setting=prop, body=updated_scenes) for owner, prop in (*FRAMING.reads, *_WATCHED)),
            )
            return (OWNERSHIP, *operators.values(), *self.menus(operators), *_listings(self, stacking), _listing(self, stacking), *handlers)

        return self.bound(identity).map(rows)


def _listings[M: Case](stacks: LayerStacks[M], stacking: LayerStacking[M]) -> _Listings[M]:
    """Stack, active layer, and last effect preset listings, each preset holding the input values at its item keys."""

    def applied(context: bpy.types.Context, value: Valued[LayerStack[Source, M]] | Valued[StackLayer[Source, M]] | Valued[Entry[M]]) -> Outcome[Answer]:
        return stacking.applied(context, value).map(lambda wrote: Done() if wrote else Unchanged(reason="Stack matches preset"))

    def matched[T](read: Callable[[bpy.types.Context], Outcome[Valued[T]]], context: bpy.types.Context, values: tuple[Valued[T], ...]) -> Outcome[tuple[bool, ...]]:
        def equal(held: Valued[T], value: Valued[T]) -> bool:
            return held.structure == value.structure and value.rebased(held.prefix).map(lambda values: values == held.values).default_value(True)

        return read(context).map(lambda held: tuple(equal(held, value) for value in values))

    def valued[T](held: Selection[M], structure: T, prefix: Key) -> Outcome[Valued[T]]:
        return captured(stacking.identity, held.entry, prefix).map(lambda values: Valued(structure=structure, values=values, prefix=prefix))

    def stack(context: bpy.types.Context) -> Outcome[Valued[LayerStack[Source, M]]]:
        return stacking.selected(context).bind(lambda held: valued(held, held.record, ""))

    def layer(context: bpy.types.Context) -> Outcome[Valued[StackLayer[Source, M]]]:
        return stacking.selected(context).bind(lambda held: held.layer.bind(lambda found: valued(held, found, found.key)))

    def last(context: bpy.types.Context) -> Outcome[Valued[Entry[M]]]:
        return stacking.selected(context).bind(
            lambda held: held.layer.bind(
                lambda found: (
                    Block
                    .of_seq(found.effects[-1:])
                    .try_head()
                    .to_result_with(partial(Fault.of, BlenderReason.ABSENT, found.name, member=item_key(EFFECTS)))
                    .bind(lambda entry: valued(held, entry, item_key(found.key, EFFECTS, len(found.effects))))
                )
            )
        )

    return (
        Listing(preset=stacks.preset, current=stack, matches=partial(matched, stack), applied=applied),
        Listing(preset=stacks.layer_preset, current=layer, matches=partial(matched, layer), applied=applied),
        Listing(preset=stacks.entry_preset, current=last, matches=partial(matched, last), applied=applied),
    )


def _listing[M: Case](stacks: LayerStacks[M], stacking: LayerStacking[M]) -> ItemList[bpy.types.PropertyGroup, LayerRow]:
    """Layer list top first with visibility, solo, and matte columns and edit side buttons, dragged to reorder."""

    def pressing(edit: Edit, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        alerted(layout, stacking.selected(context).bind(lambda held: _edited(layout, stacking.identity, stacks, edit, LayerAt(layer=held.active))))

    def spaced(layout: bpy.types.UILayout, _holder: Struct, _context: bpy.types.Context) -> None:
        layout.separator(type="SPACE")

    return ItemList(
        key=f"{OWNERSHIP.key}_layers",
        item=LayerRow,
        collection=Member(
            root=bpy.types.NodeTree,
            path=(stacking.layer_rows.name, "layers"),
            reach=Some(lambda context: root_in(bpy.types.Scene, context).bind(lambda scene: stacking.head(scene).to_option()).map(lambda held: held[1])),
        ),
        active=Some(Member(root=bpy.types.Screen, path=(stacking.view.name, "active"), reach=Some(lambda context: Option.of_optional(context.screen)))),
        cells=tuple(Property(target=Member(root=LayerRow, path=(member,)), keywords={"text": ""}) for member in ("visible", "solo", "matte")),
        enabled=Some(lambda context, _ui, row: stacking.selected(context).map(lambda found: any(shown.key == row.key for shown, _ in found.record.composited()))),
        keywords={"sort_reverse": True},
        side=(
            Opens(target=Opened(_ADDING, Called.MENU), keywords={"text": ""}, icon=Some("ADD")),
            partial(pressing, Edit.REMOVE),
            spaced,
            Opens(target=Opened(_SPECIALS, Called.MENU), keywords={"text": ""}, icon=Some("DOWNARROW_HLT")),
            spaced,
            *(partial(pressing, edit) for edit in (Edit.DUPLICATE, Edit.UP, Edit.DOWN, Edit.RESET)),
        ),
        absent=(OperatorButton(target=stacking.identity.held[stacks.operating][stack_commands.new_stack]),),
        move=Some(stacking.moved),
    )


def _edited[M: Case](layout: bpy.types.UILayout, identity: Identity, stacks: LayerStacks[M], edit: Edit, target: Target) -> Outcome[None]:
    """Draws the icon button of one edit over a target."""
    return pressed(layout, identity, Placement(identity.held[stacks.operating][stack_commands.edit_layer], {**msgspec.to_builtins(target), "action": edit.name}), {"text": "", "icon": edit.value})


def _editor[M: Case](stacks: LayerStacks[M], stacking: LayerStacking[M], listing: RowDraw, adding: RowDraw) -> RowDraw:
    """Layer list top first, then the selected layer's source, inputs, and node editors, and one panel per layer effect."""
    identity = stacking.identity

    def edits_row(layout: bpy.types.UILayout, target: Target) -> None:
        line = layout.row(align=True)
        for edit in Edit:
            alerted(line, _edited(line, identity, stacks, edit, target))

    operators = identity.held[stacks.operating]

    def mask(layout: bpy.types.UILayout, target: LayerAt | EffectAt) -> None:
        masking = operators[stack_commands.add_mask]
        alerted(layout, pressed(layout, identity, Placement(masking, msgspec.to_builtins(target)), {"text": masking.title, "icon": "MOD_MASK"}))

    def widgets(layout: bpy.types.UILayout, held: Selection[M], prefix: Key, inner: tuple[Key, ...]) -> None:
        def mine(key: Key) -> bool:
            return key.startswith(f"{prefix}_") and not any(key.startswith(f"{nested}_") for nested in inner)

        for node in (node for key, node in owned(held.tree, identity).items() if mine(key)):
            column = layout.column()
            column.use_property_split = False
            column.label(text=node.label or node.bl_label)
            column.template_node_inputs(node)
        for key in (key for keys in slot(identity).map(lambda name: Addressed.of(held.tree, name).sockets).to_option().to_list() for key in keys if mine(key)):
            alerted(layout, entry_input(identity, held.entry, key).map(lambda owner: layout.prop(owner, "value")))

    def effected(layout: bpy.types.UILayout, held: Selection[M], at: EffectAt, place: Key, entry: Entry[M]) -> None:
        header, body = layout.panel(f"{place}_{held.tree.session_uid}", default_closed=False)
        header.label(text=stacks.rows.offering(type(entry.record)).map(lambda row: row.title).default_value(bpy.path.display_name(item_key(type(entry.record)))))
        edits_row(header, at)
        for inner in Option.of_optional(body).to_list():
            widgets(inner, held, place, ())
            mask(inner, at)
            for index in range(entry.blend.masks):
                edits_row(inner, EffectMaskAt(layer=at.layer, entry=at.entry, mask=index))

    def detail(layout: bpy.types.UILayout, held: Selection[M], layer: StackLayer[Source, M], context: bpy.types.Context) -> None:
        at = LayerAt(layer=held.active)
        places = tuple(item_key(layer.key, EFFECTS, index) for index in range(1, len(layer.effects) + 1))
        for source in Option.of_optional(owned(held.tree, identity).get(layer.node)).to_list():
            source.draw_buttons_ext(context, layout)
            layout.template_node_inputs(source)
            outputs = layout.row(align=True)
            for output in (output for output in source.outputs if output.enabled):
                alerted(outputs, pressed(outputs, identity, Placement(operators[stack_commands.select_output], {"layer": at.layer, "output": output.name}), {"text": output.name, "translate": False}))
        widgets(layout, held, layer.key, places)
        buttons = layout.row(align=True)
        mask(buttons, at)
        alerted(buttons, _edited(buttons, identity, stacks, Edit.RESET, at))
        for index in range(layer.blend.masks):
            edits_row(layout, LayerMaskAt(layer=at.layer, mask=index))
        for index, entry in enumerate(layer.effects):
            effected(layout, held, EffectAt(layer=at.layer, entry=index), places[index], entry)

    def drawn(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        listing(layout, holder, context)
        for held in stacking.selected(context).to_option().to_list():
            for layer in held.layer.to_option().to_list():
                detail(layout, held, layer, context)
                adding(layout, holder, context)

    return drawn


def _presets[M: Case](stacking: LayerStacking[M], creating: Command, listings: tuple[tuple[_Preset[M], RowDraw], ...]) -> RowDraw:
    """Preset listing draws of each held stack, layer, or effect, the New Stack button while no stack exists."""

    def drawn(layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        if rooted(bpy.types.Scene, context).bind(stacking.head).is_error():
            alerted(layout, pressed(layout, stacking.identity, Placement(creating), {"text": creating.title}))
        for listing, draw in listings:
            if listing.current(context).is_ok():
                draw(layout, holder, context)

    return drawn


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LayerStacks"]
