"""Layer stack composition over the effects an extension offers: presets, the operator of every stack command, menus, the editor section with its layer list, the preset popover, the scene look pick tiling the presets, and the layered document of formed color and raw mask coverage with its export."""

from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, nullcontext
from functools import cached_property, partial
from types import GenericAlias, MappingProxyType
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec

from blender.bpy.commands.calls import Command, Placement, PopupKind, PopupTarget
from blender.bpy.commands.invocations import Open, Popup
from blender.bpy.commands.operators import Answer, BoundBody, Call, Done, Empty, Invoke, Operator, Unchanged
from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.paths import id_data, root_in, rooted, Struct
from blender.bpy.data.rna.properties import Callbacks, Changed, choices, EnumItem, held_choices, IconedItem, rebound, titled
from blender.bpy.extension.events import Handler, Subscription
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.extension.reports import alerted
from blender.bpy.nodes.addressing import InterfaceKeys, owned
from blender.bpy.nodes.desired import item_key
from blender.bpy.nodes.editing import Arranger
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor import stack_commands
from blender.bpy.render.compositor.documents import layered
from blender.bpy.render.compositor.editors import node_editors
from blender.bpy.render.compositor.effects import CompositorEffect, Entry
from blender.bpy.render.compositor.inputs import captured, entry_input, FRAMING
from blender.bpy.render.compositor.layers import (
    Copied,
    Edit,
    EffectAt,
    EffectMaskAt,
    EFFECTS,
    LayerAt,
    LayerEdit,
    LayerMaskAt,
    LayerRow,
    LayerRows,
    LayerStack,
    NodeKind,
    OutputChoice,
    OWNERSHIP,
    PassSourced,
    RecombinedSourced,
    StackLayer,
    StackLook,
    StoredLayer,
    Switched,
    Target,
    TermFiltering,
    TITLE,
    VIEW,
)
from blender.bpy.render.compositor.offered import Choice, OfferedEffects
from blender.bpy.render.compositor.recipes import Valued
from blender.bpy.render.compositor.stack_service import LayerStackService, Selection
from blender.bpy.render.outputs.exports import Layering
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.view_layers.passes import PassFlags
from blender.bpy.settings.attachments import Attachment, Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.elements.menus import Dropdown, Menu, Submenus
from blender.bpy.ui.elements.panels import Panel, POPOVER
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import Layout, Opens, operator_button, OperatorButton, Property, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.templates import Gallery
from blender.bpy.ui.layout.values import Listing
from blender.bpy.ui.resources.icons import Icons
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.buffers import Coded, Depth
from blender.imaging.files.documents import Document
from blender.imaging.files.multilayer import Multilayer
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type Operators = Mapping[Callable[..., object], Command]
type _Listed[M: Case] = Listing[Valued[LayerStack[M]]] | Listing[Valued[StackLayer[M]]] | Listing[Valued[Entry[M]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_ADD_LAYER: Final[Key] = "add_layer"
_ADD_EFFECT: Final[Key] = "layer_effect_add"
_SPECIALS: Final[Key] = f"{OWNERSHIP.key}_specials"
_UNPICKED: Final = "NONE"
_MEDIA: Final = (*sorted(bpy.path.extensions_image), *sorted(bpy.path.extensions_movie))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_WATCHED: Final = tuple(
    (owner, owner.bl_rna.properties[name])
    for owner, name in ((bpy.types.RenderSettings, "film_transparent"), (bpy.types.ViewLayer, "name"), (bpy.types.ViewLayer, "use"), (bpy.types.Object, "lightgroup"), (bpy.types.World, "lightgroup"))
)
_STACK_COMMANDS: Final = (
    stack_commands.duplicate_stack,
    stack_commands.switch_stack,
    stack_commands.edit_nested_stack,
    stack_commands.copy_layer,
    stack_commands.copy_all_layers,
    stack_commands.clear_layers,
    stack_commands.remove_stack,
)
_LAYER_SOURCES: Final = (
    stack_commands.add_adjustment_layer,
    stack_commands.add_render_pass_layer,
    stack_commands.add_recombined_layer,
    stack_commands.add_node_layer,
    stack_commands.add_nested_stack,
    stack_commands.add_media,
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _presets_drawn[M: Case](
    stacking: LayerStackService[M], creating: Command, listings: tuple[tuple[_Listed[M], RowDraw], ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context
) -> None:
    """Draws the preset listing of each held stack, layer, or effect, the New Stack button while no stack exists."""
    if rooted(bpy.types.Scene, context).bind(stacking.head).is_error():
        alerted(layout, operator_button(layout, stacking.identity, Placement(creating), {"text": creating.title}))
    for listing, draw in listings:
        if listing.current(context).is_ok():
            draw(layout, holder, context)


# --- [COMPOSITION] ----------------------------------------------------------------------


class LayerStackExtension[M: Case](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Layer stacks over the effects an extension offers: layers take the effects ranked before `before`, every effect while unset, with stock looks, the term filter, the document view, and the preview icon of each look's tile.

    `tile` returns a stack preset's preview icon by its stem, the stock icon while it has none.
    """

    rows: OfferedEffects[M]
    before: Option[CompositorEffect[M]] = Nothing
    looks: Mapping[str, Valued[LayerStack[M]]] = MappingProxyType({})
    filtering: Option[TermFiltering[M]] = Nothing
    view: Callable[[Identity, bpy.types.Scene], Outcome[Option[DisplayView]]] = lambda _identity, _scene: Ok(Nothing)
    tile: Option[Callable[[Identity, str, Valued[LayerStack[M]]], Option[int]]] = Nothing

    # --- [DECLARATIONS]

    def _preset[S: msgspec.Struct](self, key: Key, structure: type[S], stock: Mapping[str, Valued[S]] = MappingProxyType({})) -> Preset[Valued[S]]:
        """Presets of one generic structure over the offered records, with its input values."""
        return Preset(key=key, form=GenericAlias(Valued, (GenericAlias(structure, (self.rows.form,)),)), stock=stock)  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    @cached_property
    def accepted(self) -> tuple[CompositorEffect[M], ...]:
        """Effects a layer accepts in rank order."""
        return tuple(row for row in self.rows.rows if self.before.map(lambda before: self.rows.rank(row) < self.rows.rank(before)).default_value(True))

    @cached_property
    def refresh(self) -> Resource[Callable[[], None]]:
        """Deferred refresh of every stack of an extension, the one queue layer row edits, scene changes, and file loads call."""
        return Resource[Callable[[], None]](made=lambda identity: identity.reports.deferred(lambda: self.bound(identity).bind(LayerStackService.refreshed)))

    @cached_property
    def operating(self) -> Resource[Operators]:
        """Operators of every stack command an extension registers, built once per extension, its stack pickers reading that extension's stacks and layer rows."""
        return Resource[Operators](made=lambda identity: nullcontext(self.operators(identity)))

    @cached_property
    def layered(self) -> Attachment[LayerRows]:
        """Layer rows on the stack group owning each layer's name, flags, and order, a row edit queueing the stack refresh."""
        return Attachment(
            key=f"{OWNERSHIP.key}_layers", form=LayerRows, owner=bpy.types.NodeTree, changed=Some(lambda identity: nullcontext(lambda _owner, _context: identity.resources[self.refresh]()))
        )

    @cached_property
    def preset(self) -> Preset[Valued[LayerStack[M]]]:
        """Stack presets with their input values over the stock looks."""
        return self._preset(OWNERSHIP.key, LayerStack, self.looks)

    @cached_property
    def layer_preset(self) -> Preset[Valued[StackLayer[M]]]:
        """Presets of one layer with its input values."""
        return self._preset(f"{OWNERSHIP.key}_layer", StackLayer)

    @cached_property
    def entry_preset(self) -> Preset[Valued[Entry[M]]]:
        """Presets of one effect with its blend and input values."""
        return self._preset(f"{OWNERSHIP.key}_entry", Entry)

    @cached_property
    def layering(self) -> Layering:
        """Layered document export of the head stack at a saved frame."""
        return Layering(key=OWNERSHIP.key, document=self.document)

    # --- [LOOKS]

    @cached_property
    def looking(self) -> Attachment[StackLook]:
        """Scene pick of a stack preset and its strength, its items the presets under their tiles, a pick or strength change writing the preset over the head stack."""
        return Attachment(
            key=f"{OWNERSHIP.key}_look",
            form=StackLook,
            owner=bpy.types.Scene,
            callbacks=Some(lambda identity: nullcontext({"look": Callbacks(items=partial(self.offered, identity))})),
            changed=Some(self.picking),
        )

    @property
    def look(self) -> Layout:
        """Gallery of the stack presets the scene pick takes, then its strength slider."""
        return Layout(rows=(Gallery(target=Member(root=self.looking, path=("look",))), Property(target=Member(root=self.looking, path=("strength",)), keywords={"slider": True})))

    def offered(self, identity: Identity, _owner: Struct, _context: bpy.types.Context | None) -> tuple[IconedItem, ...]:
        """Item rows of the unpicked state under the clear icon at the value an unset pick holds, then the stack presets, each under its tile's preview icon."""
        preset = self.preset

        def icon(stem: str) -> int:
            return self.tile.bind(lambda tile: preset.loaded(identity, stem).to_option().bind(partial(tile, identity, stem))).default_value(0)

        return held_choices((
            (_UNPICKED, "None", "Head stack as it stands, no preset written over it", Icons.value(identity, "X").default_value(0), 0),
            *((stem, name, description, icon(stem), number) for stem, name, description, _, number in preset.entries(identity)),
        ))

    @contextmanager
    def picking(self, identity: Identity) -> Generator[Changed]:
        """Holds the deferred preset write a pick or strength change queues."""

        def applied(scene: bpy.types.Scene, pick: StackLook, stacking: LayerStackService[M]) -> Outcome[Option[str]]:
            return (
                Ok(Nothing)
                if pick.look == _UNPICKED
                else self.preset
                .loaded(identity, pick.look)
                .bind(lambda preset: stacking.looked(scene, preset, pick.strength, bpy.context))
                .map(lambda wrote: Some(f"Look {bpy.path.display_name(pick.look)}") if wrote else Nothing)
            )

        def write(handle: Handle[bpy.types.Scene]) -> Outcome[Option[str]]:
            return handle.resolve().bind(lambda scene: paired(self.looking.read(identity, scene), self.bound(identity)).bind(lambda parts: applied(scene, *parts)))

        with identity.reports.deferred(write) as deferred:
            yield lambda owner, _context: id_data(bpy.types.Scene, owner).map(lambda scene: deferred(Handle.of(scene))).default_value(None)

    # --- [COMMANDS]

    def command[T: bpy.types.PropertyGroup](
        self, body: Callable[[LayerStackService[M], Call[T]], Outcome[Answer]], form: type[T], invoke: Option[Invoke[T]] = Nothing
    ) -> tuple[Callable[..., object], Command]:
        """Returns a stack command body with the operator running it over the calling extension's stacking, keyed and described by the body."""
        return body, Operator[T](form=form, run=BoundBody(self.bound, body), invoke=invoke)

    def operators(self, identity: Identity) -> Operators:
        """Builds the operator of every stack command once for an extension, its stack pickers listing the extension's stacks and the chosen stack's layer rows, its effect choice the accepted effects."""

        def stacks(_owner: Struct, _context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
            return held_choices(tuple((tree.name, tree.name, "") for tree, _ in OWNERSHIP.held(identity)))

        def layers(owner: Copied, _context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
            rows = self.layered.bound(identity).map(
                lambda attached: tuple((row.key, row.name, "") for tree, _ in OWNERSHIP.held(identity) if tree.name == owner.stack for row in attached.struct(tree).layers)
            )
            return held_choices(rows.default_value(()))

        switched, copied = rebound(Switched, stack=Callbacks(items=stacks)), rebound(Copied, stack=Callbacks(items=stacks), layer=Callbacks[Copied](items=layers))
        chosen = rebound(Choice, kind=Callbacks(items=choices((row.kind, row.title, row.description) for row in self.accepted)))
        return MappingProxyType(
            dict((
                self.command(stack_commands.new_stack, Empty),
                self.command(stack_commands.duplicate_stack, Empty),
                self.command(stack_commands.switch_stack, switched, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup)))),
                self.command(stack_commands.edit_nested_stack, Empty),
                self.command(stack_commands.copy_layer, copied, Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog)))),
                self.command(stack_commands.copy_all_layers, switched, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup)))),
                self.command(stack_commands.clear_layers, Empty),
                self.command(stack_commands.remove_stack, Empty),
                self.command(stack_commands.add_adjustment_layer, Empty),
                self.command(stack_commands.add_render_pass_layer, PassSourced, Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog)))),
                self.command(stack_commands.add_recombined_layer, RecombinedSourced, Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog)))),
                self.command(stack_commands.add_node_layer, NodeKind, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup)))),
                self.command(stack_commands.add_nested_stack, switched, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup)))),
                self.command(stack_commands.add_media, Empty, Some(Open(extensions=lambda _: _MEDIA))),
                self.command(stack_commands.select_output, OutputChoice),
                self.command(stack_commands.add_mask, LayerEdit),
                self.command(stack_commands.edit_layer, LayerEdit),
                self.command(stack_commands.add_effect_to_layer, chosen),
            ))
        )

    # --- [MENUS]

    def menus(self, operators: Operators) -> tuple[Menu, ...]:
        """Menus adding a layer of each source, appending an accepted effect, one submenu per phase where accepted effects span more than one, and the stack commands beside the layer rows."""
        effects = tuple(
            (self.rows.phase(row).key, OperatorButton(target=Placement(operators[stack_commands.add_effect_to_layer], {"kind": row.kind}), keywords={"text": row.title})) for row in self.accepted
        )
        return (
            Menu(key=_ADD_LAYER, description="Add layer source to stack", body=Dropdown(entries=tuple(OperatorButton(target=operators[body]) for body in _LAYER_SOURCES))),
            Menu(
                key=_ADD_EFFECT,
                title=Some("Add Effect"),
                description="Add effect to selected layer",
                body=Submenus(entries=effects) if len(dict.fromkeys(key for key, _ in effects)) > 1 else Dropdown(entries=tuple(button for _, button in effects), search=True),
            ),
            Menu(key=_SPECIALS, description="Stack and layer commands beside the layer rows", body=Dropdown(entries=tuple(OperatorButton(target=operators[body]) for body in _STACK_COMMANDS))),
        )

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Stack refresh and operators, view attachment, layer rows with the frame size binding stacks read, and the arranger placing owned stack nodes."""
        return self.refresh, self.operating, VIEW, self.layered, FrameSize.attachment, Arranger()

    def bound(self, identity: Identity) -> Outcome[LayerStackService[M]]:
        """Binds the store of each layer's structure, the layer rows and view attachments, and pass flags of an extension."""
        stored = Stored.keyed(identity, OWNERSHIP.key, msgspec.msgpack.Decoder(GenericAlias(dict, (Key, GenericAlias(StoredLayer, (self.rows.form,))))))
        return paired(stored, self.layered.bound(identity), VIEW.bound(identity)).map(
            lambda parts: LayerStackService(identity=identity, rows=self.rows, filtering=self.filtering, store=parts[0], layer_rows=parts[1], view=parts[2], flags=PassFlags.of())
        )

    # --- [DOCUMENTS]

    def document(self, identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> Outcome[Document[Coded]]:
        """Layered document of the scene's head stack at a saved frame, formed through the stack view, else the scene's."""
        return paired(self.bound(identity), self.view(identity, scene)).bind(lambda parts: layered(parts[0], parts[1].default_with(lambda: viewed(scene)), scene, frame, depth))

    # --- [SECTIONS]

    def section(self, header: tuple[RowContent, ...] = ()) -> Section:
        """Returns the editor section under header rows, presets first."""
        return Section(key=OWNERSHIP.key, header=(Opens(target=PopupTarget(self.presets.key, PopupKind.PANEL), keywords={"text": ""}, icon=Some("PRESET")), *header), rows=(self,), opened=True)

    @cached_property
    def presets(self) -> Section:
        """Section of stack, layer, and effect preset listings, the popover the editor header opens."""
        return Section(
            key=f"{OWNERSHIP.key}_presets",
            description=f"Apply, add, or import {TITLE.lower()}, layer, and effect presets",
            rows=(Layout(rows=lambda identity: self.presets_drawn(identity).map(lambda draw: (draw,)), prerequisites=(self,)),),
        )

    def presets_drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns the preset listing draws of each held stack, layer, or effect, the New Stack button while no stack exists."""

        def listed(stacking: LayerStackService[M]) -> Outcome[RowDraw]:
            listings = self._preset_listings(stacking)
            creating = identity.resources[self.operating][stack_commands.new_stack]
            return accumulated(listing.drawn(identity) for listing in listings).map(lambda draws: partial(_presets_drawn, stacking, creating, tuple(zip(listings, draws))))

        return self.bound(identity).bind(listed)

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns the draw of the layer list, then the selected layer's source, inputs, and node editors, and one panel per layer effect."""
        return self.bound(identity).bind(
            lambda stacking: paired(self._layer_list(stacking).drawn(identity), realized((Opens(target=PopupTarget(_ADD_EFFECT, PopupKind.MENU), icon=Some("ADD")),), identity)).map(
                lambda parts: partial(self._editor_drawn, stacking, *parts)
            )
        )

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the extension's stack operators and menus over them, listings, the preset popover the section header opens, and the handlers and subscriptions queueing the stack refresh."""
        operators = identity.resources[self.operating]

        def updated_scenes() -> Outcome[None]:
            identity.resources[self.refresh]()
            return Ok(None)

        def rows(stacking: LayerStackService[M]) -> Acquisitions:
            handlers: tuple[Declaration, ...] = (
                Handler(handlers=bpy.app.handlers.load_post, body=updated_scenes),
                *(Subscription(owner=owner, setting=prop, body=updated_scenes) for owner, prop in (*FRAMING.reads, *_WATCHED)),
            )
            return (OWNERSHIP, *operators.values(), *self.menus(operators), *self._preset_listings(stacking), self._layer_list(stacking), Panel(section=self.presets, at=POPOVER), *handlers)

        return self.bound(identity).map(rows)

    # --- [LISTINGS]

    def _preset_listings(self, stacking: LayerStackService[M]) -> tuple[Listing[Valued[LayerStack[M]]], Listing[Valued[StackLayer[M]]], Listing[Valued[Entry[M]]]]:
        """Stack, active layer, and last effect preset listings, each preset holding the input values at its item keys."""

        def applied(context: bpy.types.Context, value: Valued[LayerStack[M]] | Valued[StackLayer[M]] | Valued[Entry[M]]) -> Outcome[Answer]:
            return stacking.applied(context, value).map(lambda wrote: Done() if wrote else Unchanged(reason="Stack matches preset"))

        def matched[T](read: Callable[[bpy.types.Context], Outcome[Valued[T]]], context: bpy.types.Context, values: tuple[Valued[T], ...]) -> Outcome[tuple[bool, ...]]:
            def equal(held: Valued[T], value: Valued[T]) -> bool:
                return held.structure == value.structure and value.rebased(held.prefix).map(lambda values: values == held.values).default_value(True)

            return read(context).map(lambda held: tuple(equal(held, value) for value in values))

        def valued[T](pick: Callable[[Selection[M]], Outcome[tuple[T, Key]]], context: bpy.types.Context) -> Outcome[Valued[T]]:
            return stacking.selected(context).bind(
                lambda held: pick(held).bind(lambda found: captured(stacking.identity, held.entry, found[1]).map(lambda values: Valued(structure=found[0], values=values, prefix=found[1])))
            )

        def last(held: Selection[M]) -> Outcome[tuple[Entry[M], Key]]:
            return held.layer.bind(
                lambda found: Block.of_seq(tuple(zip(found.effects, found.places, strict=True))[-1:]).try_head().to_result_with(partial(Fault.of, Reason.ABSENT, found.name, member=item_key(EFFECTS)))
            )

        stack: Callable[[bpy.types.Context], Outcome[Valued[LayerStack[M]]]] = partial(valued, lambda held: Ok((held.record, "")))
        layer: Callable[[bpy.types.Context], Outcome[Valued[StackLayer[M]]]] = partial(valued, lambda held: held.layer.map(lambda found: (found, found.key)))
        effect: Callable[[bpy.types.Context], Outcome[Valued[Entry[M]]]] = partial(valued, last)
        return (
            Listing(preset=self.preset, current=stack, matches=partial(matched, stack), applied=applied),
            Listing(preset=self.layer_preset, current=layer, matches=partial(matched, layer), applied=applied),
            Listing(preset=self.entry_preset, current=effect, matches=partial(matched, effect), applied=applied),
        )

    # --- [EDITOR]

    def _layer_list(self, stacking: LayerStackService[M]) -> ItemList[bpy.types.PropertyGroup, LayerRow]:
        """Layer list top first with visibility, solo, and matte columns and edit side buttons, dragged to reorder."""

        def pressing(edit: Edit, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
            alerted(layout, stacking.selected(context).bind(lambda held: self._edited(layout, stacking.identity, edit, LayerAt(layer=held.active))))

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
                Opens(target=PopupTarget(_ADD_LAYER, PopupKind.MENU), keywords={"text": ""}, icon=Some("ADD")),
                partial(pressing, Edit.REMOVE),
                spaced,
                Opens(target=PopupTarget(_SPECIALS, PopupKind.MENU), keywords={"text": ""}, icon=Some("DOWNARROW_HLT")),
                spaced,
                *(partial(pressing, edit) for edit in (Edit.DUPLICATE, Edit.UP, Edit.DOWN, Edit.RESET)),
            ),
            absent=(OperatorButton(target=stacking.identity.resources[self.operating][stack_commands.new_stack]),),
            move=Some(stacking.moved),
        )

    def _edited(self, layout: bpy.types.UILayout, identity: Identity, edit: Edit, target: Target) -> Outcome[None]:
        """Draws the icon button of one edit over a target."""
        return operator_button(
            layout, identity, Placement(identity.resources[self.operating][stack_commands.edit_layer], {**msgspec.to_builtins(target), "action": edit.name}), {"text": "", "icon": edit.value}
        )

    def _editor_drawn(self, stacking: LayerStackService[M], listing: RowDraw, adding: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the layer list top first, then the selected layer's source, inputs, and node editors, and one panel per layer effect."""
        identity = stacking.identity
        operators = identity.resources[self.operating]

        def edits_row(layout: bpy.types.UILayout, target: Target) -> None:
            line = layout.row(align=True)
            for edit in Edit:
                alerted(line, self._edited(line, identity, edit, target))

        def mask(layout: bpy.types.UILayout, target: LayerAt | EffectAt) -> None:
            masking = operators[stack_commands.add_mask]
            alerted(layout, operator_button(layout, identity, Placement(masking, msgspec.to_builtins(target)), {"text": masking.title, "icon": "MOD_MASK"}))

        def widgets(layout: bpy.types.UILayout, held: Selection[M], prefix: Key, inner: tuple[Key, ...]) -> None:
            def mine(key: Key) -> bool:
                return key.startswith(f"{prefix}_") and not any(key.startswith(f"{nested}_") for nested in inner)

            node_editors(layout, (node for key, node in owned(held.tree, identity).items() if mine(key)))
            for key in (key for keys in InterfaceKeys.of(held.tree, identity).map(lambda live: live.sockets).to_option().to_list() for key in keys if mine(key)):
                alerted(layout, entry_input(identity, held.entry, key).map(lambda owner: layout.prop(owner, "value")))

        def effected(layout: bpy.types.UILayout, held: Selection[M], at: EffectAt, place: Key, entry: Entry[M]) -> None:
            header, body = layout.panel(f"{place}_{held.tree.session_uid}", default_closed=False)
            header.label(text=titled(type(entry.record)))
            edits_row(header, at)
            for inner in Option.of_optional(body).to_list():
                widgets(inner, held, place, ())
                mask(inner, at)
                for index in range(entry.blend.masks):
                    edits_row(inner, EffectMaskAt(layer=at.layer, entry=at.entry, mask=index))

        def detail(held: Selection[M], layer: StackLayer[M]) -> None:
            at = LayerAt(layer=held.active)
            for source in Option.of_optional(owned(held.tree, identity).get(layer.node)).to_list():
                source.draw_buttons_ext(context, layout)
                layout.template_node_inputs(source)
                outputs = layout.row(align=True)
                for output in (output for output in source.outputs if output.enabled):
                    alerted(
                        outputs,
                        operator_button(outputs, identity, Placement(operators[stack_commands.select_output], {"layer": at.layer, "output": output.name}), {"text": output.name, "translate": False}),
                    )
            widgets(layout, held, layer.key, layer.places)
            buttons = layout.row(align=True)
            mask(buttons, at)
            alerted(buttons, self._edited(buttons, identity, Edit.RESET, at))
            for index in range(layer.blend.masks):
                edits_row(layout, LayerMaskAt(layer=at.layer, mask=index))
            for index, entry in enumerate(layer.effects):
                effected(layout, held, EffectAt(layer=at.layer, entry=index), layer.places[index], entry)

        listing(layout, holder, context)
        for held in stacking.selected(context).to_option().to_list():
            for layer in held.layer.to_option().to_list():
                detail(held, layer)
                adding(layout, holder, context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LayerStackExtension"]
