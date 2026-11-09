"""Layer stack composition over the effects an extension offers: presets, the operator of every stack command, menus, the editor section with its layer list, the preset popover, the scene look pick tiling the presets, and the layered document of formed color and raw mask coverage with its export."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, nullcontext
from functools import cached_property, partial
from types import GenericAlias, MappingProxyType
from typing import Final

import bpy
from expression import Option, Some
from expression.collections import Block
import msgspec
import PyOpenColorIO

from rasm_bpy.commands.invocations import Open, Popup
from rasm_bpy.commands.operators import Answer, BoundBody, Call, Command, Done, Empty, Invoke, Operator, Placement, PopupKind, PopupTarget, Unchanged
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions, Declaration, Handler, Resource, Subscription
from rasm_bpy.data._ids.store import Handle
from rasm_bpy.data._rna.paths import id_data, root_in, rooted, Struct
from rasm_bpy.data._rna.properties import _rebound, _titled, Callbacks, Changed, choices, EnumItem, IconedItem
from rasm_bpy.nodes._addressing import InterfaceKeys, owned
from rasm_bpy.nodes._desired import item_key
from rasm_bpy.render._color.spaces import viewed
from rasm_bpy.render._compositor import stack_commands
from rasm_bpy.render._compositor.documents import _layered
from rasm_bpy.render._compositor.editing import _Arranger
from rasm_bpy.render._compositor.editors import _node_editors
from rasm_bpy.render._compositor.effects import CompositorEffect, Entry
from rasm_bpy.render._compositor.inputs import _FRAMING, _interface_input, captured
from rasm_bpy.render._compositor.layers import (
    _OWNERSHIP,
    _TITLE,
    _VIEW,
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
    PassSourced,
    RecombinedSourced,
    StackLayer,
    StackLook,
    StackTarget,
    StoredLayer,
    Switched,
    TermFiltering,
)
from rasm_bpy.render._compositor.offered import Choice, OfferedEffects
from rasm_bpy.render._compositor.recipes import Valued
from rasm_bpy.render._compositor.stack_service import LayerStackService, Selection
from rasm_bpy.render._outputs.exports import Layering
from rasm_bpy.render._outputs.framing import FrameSize
from rasm_bpy.render._view_layers.passes import PassFlags
from rasm_bpy.settings._attachments import Attachment, Stored
from rasm_bpy.settings._presets import Preset
from rasm_bpy.ui._elements.menus import Dropdown, Menu, Submenus
from rasm_bpy.ui._elements.panels import _POPOVER, Panel
from rasm_bpy.ui._layout.lists import _ItemList
from rasm_bpy.ui._layout.members import Member
from rasm_bpy.ui._layout.rows import _operator_button, _realized, Layout, Opens, OperatorButton, Property, RowContent, RowDraw
from rasm_bpy.ui._layout.sections import Section
from rasm_bpy.ui._layout.templates import Gallery
from rasm_bpy.ui._layout.values import Listing
from rasm_bpy.ui._resources.icons import Icons
from rasm_imaging import called, Depth, DisplayView, Document, Multilayer
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Operators = Mapping[object, Command]
type _Listed = Listing[Valued[LayerStack]] | Listing[Valued[StackLayer]] | Listing[Valued[Entry]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_ADD_LAYER: Final[Key] = "add_layer"
_ADD_EFFECT: Final[Key] = "layer_effect_add"
_SPECIALS: Final[Key] = f"{_OWNERSHIP.key}_specials"
_UNPICKED: Final = "NONE"
_MEDIA: Final = (*sorted(bpy.path.extensions_image), *sorted(bpy.path.extensions_movie))  # ty: ignore[unresolved-attribute]
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


def _presets_drawn(stacking: LayerStackService, creating: Command, listings: tuple[tuple[_Listed, RowDraw], ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the preset listing of each held stack, layer, or effect, the New Stack button while no stack exists."""
    if rooted(bpy.types.Scene, context).bind(stacking.head).is_error():
        stacking.identity.reports.faulted(_operator_button(layout, stacking.identity, Placement(creating), {"text": creating.title}))
    for listing, draw in listings:
        if listing.current(context).is_ok():
            draw(layout, holder, context)


# --- [COMPOSITION] ----------------------------------------------------------------------


class LayerStackExtension(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Layer stacks over the effects an extension offers: layers take the effects ranked before `before`, every effect while unset, with stock looks, the term filter, the document view, and the preview icon of each look's tile.

    `tile` returns a stack preset's preview icon by its stem, the stock icon while it has none.
    """

    # --- [STATE]

    rows: OfferedEffects
    before: Option[CompositorEffect] = Nothing
    looks: Mapping[str, Valued[LayerStack]] = MappingProxyType({})
    filtering: Option[TermFiltering] = Nothing
    view: Callable[[Identity, bpy.types.Scene], Outcome[Option[DisplayView]]] = lambda _identity, _scene: Ok(Nothing)
    tile: Option[Callable[[Identity, bpy.types.Scene, str, Valued[LayerStack]], Option[int]]] = Nothing

    # --- [DECLARATIONS]

    def _preset[S: msgspec.Struct](self, key: Key, structure: type[S], stock: Mapping[str, Valued[S]] = MappingProxyType({})) -> Preset[Valued[S]]:
        """Presets of one generic structure over the offered records, with its input values."""
        return Preset(key=key, form=GenericAlias(Valued, (GenericAlias(structure, (self.rows.form,)),)), stock=stock)  # ty: ignore[invalid-type-form]

    @cached_property
    def accepted(self) -> tuple[CompositorEffect, ...]:
        """Effects a layer accepts in rank order."""
        return tuple(row for row in self.rows.rows if self.before.map(lambda before: self.rows.rank(row) < self.rows.rank(before)).default_value(value=True))

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
        return Attachment(key=f"{_OWNERSHIP.key}_layers", form=LayerRows, owner=bpy.types.NodeTree, changed=Some(lambda identity: nullcontext(lambda _owner, _context: self.refresh[identity]())))

    @cached_property
    def preset(self) -> Preset[Valued[LayerStack]]:
        """Stack presets with their input values over the stock looks."""
        return self._preset(_OWNERSHIP.key, LayerStack, self.looks)

    @cached_property
    def layer_preset(self) -> Preset[Valued[StackLayer]]:
        """Presets of one layer with its input values."""
        return self._preset(f"{_OWNERSHIP.key}_layer", StackLayer)

    @cached_property
    def entry_preset(self) -> Preset[Valued[Entry]]:
        """Presets of one effect with its blend and input values."""
        return self._preset(f"{_OWNERSHIP.key}_entry", Entry)

    @cached_property
    def layering(self) -> Layering:
        """Layered document export of the head stack at a saved frame."""
        return Layering(key=_OWNERSHIP.key, document=self.document)

    # --- [LOOKS]

    @cached_property
    def looking(self) -> Attachment[StackLook]:
        """Scene pick of a stack preset and its strength, its items the presets under their tiles, a pick or strength change writing the preset over the head stack."""
        return Attachment(
            key=f"{_OWNERSHIP.key}_look",
            form=StackLook,
            owner=bpy.types.Scene,
            callbacks=Some(lambda identity: nullcontext({"look": Callbacks(items=partial(self.offered, identity))})),
            changed=Some(self.picking),
        )

    @property
    def look(self) -> Layout:
        """Gallery of the stack presets the scene pick takes, then its strength slider."""
        return Layout(rows=(Gallery(target=Member(root=self.looking, path=("look",))), Property(target=Member(root=self.looking, path=("strength",)), keywords={"slider": True})))

    def offered(self, identity: Identity, owner: StackLook, _context: bpy.types.Context | None) -> tuple[IconedItem, ...]:
        """Item rows of the unpicked state under the clear icon at the value an unset pick holds, then the stack presets, each under its tile's preview icon."""
        preset = self.preset
        sources = preset.listed(identity)
        scene = id_data(bpy.types.Scene, owner)

        def icon(stem: str) -> int:
            return self.tile.bind(lambda tile: scene.bind(lambda held: preset.read(sources[stem]).to_option().bind(partial(tile, identity, held, stem)))).default_value(0)

        return tuple(
            (stem, name, description, held if isinstance(held, int) else icon(stem), number)
            for stem, name, description, held, number in preset.entries(
                sources, prefix=((_UNPICKED, "None", "Head stack as it stands, no preset written over it", Icons.value(identity, Some("X")).default_value(Nothing).default_value(0), 0),)
            )
        )

    @contextmanager
    def picking(self, identity: Identity) -> Generator[Changed]:
        """Holds the deferred preset write a pick or strength change queues."""

        def applied(scene: bpy.types.Scene, pick: StackLook, stacking: LayerStackService) -> Outcome[Option[str]]:
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

    def command[T: bpy.types.PropertyGroup](self, body: Callable[[LayerStackService, Call[T]], Outcome[Answer]], form: type[T], invoke: Option[Invoke[T]] = Nothing) -> tuple[object, Command]:
        """Returns a stack command body with the operator running it over the calling extension's stacking, keyed and described by the body."""
        return body, Operator[T](form=form, run=BoundBody(self.bound, body), invoke=invoke)

    def operators(self, identity: Identity) -> Operators:
        """Builds the operator of every stack command once for an extension, its stack pickers listing the extension's stacks and the chosen stack's layer rows, its effect choice the accepted effects."""

        def stacks(_owner: Struct, _context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
            return choices((tree.name, tree.name, "") for tree, _ in _OWNERSHIP.held(identity))

        def layers(owner: Copied, _context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
            rows: list[tuple[str, str, str]] = []
            for attached in self.layered.bound(identity).to_option().to_list():
                for tree, _ in _OWNERSHIP.held(identity):
                    if tree.name == owner.stack:
                        layers: Iterable[LayerRow] = attached.struct(tree).layers  # ty: ignore[unsound-assignment]
                        rows.extend((row.key, row.name, "") for row in layers)
            return choices(rows)

        switched, copied = _rebound(Switched, stack=Callbacks(items=stacks)), _rebound(Copied, stack=Callbacks(items=stacks), layer=Callbacks[Copied](items=layers))
        chosen = _rebound(Choice, kind=Callbacks(items=choices((row.kind, row.title, row.description) for row in self.accepted)))
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
                self.command(stack_commands.add_media, Empty, Some(Open[Empty](extensions=lambda _: _MEDIA))),
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
            Menu(key=_ADD_EFFECT, title=Some("Add Effect"), description="Add effect to selected layer", body=Submenus(entries=effects)),
            Menu(key=_SPECIALS, description="Stack and layer commands beside the layer rows", body=Dropdown(entries=tuple(OperatorButton(target=operators[body]) for body in _STACK_COMMANDS))),
        )

    # --- [BINDING]

    def bound(self, identity: Identity) -> Outcome[LayerStackService]:
        """Binds the store of each layer's structure, the layer rows and view attachments, and pass flags of an extension."""
        stored = Stored.keyed(identity, _OWNERSHIP.key, msgspec.msgpack.Decoder(GenericAlias(dict, (Key, GenericAlias(StoredLayer, (self.rows.form,))))))
        return paired(self.layered.bound(identity), _VIEW.bound(identity)).map(
            lambda parts: LayerStackService(identity=identity, rows=self.rows, filtering=self.filtering, store=stored, layer_rows=parts[0], view=parts[1], flags=PassFlags.of())
        )

    # --- [DOCUMENTS]

    def document(self, identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> Outcome[Document]:
        """Layered document of the scene's head stack at a saved frame, formed through the stack view, else the scene's."""
        return paired(self.bound(identity), self.view(identity, scene), called(PyOpenColorIO.GetCurrentConfig, str(frame.path))).bind(
            lambda parts: _layered(parts[0], parts[2], parts[1].default_with(lambda: viewed(scene)), scene, frame, depth)
        )

    # --- [SECTIONS]

    def section(self, header: tuple[RowContent, ...] = ()) -> Section:
        """Returns the editor section under header rows, presets first."""
        return Section(key=_OWNERSHIP.key, header=(Opens(target=PopupTarget(self.presets.key, PopupKind.PANEL), keywords={"text": ""}, icon=Some("PRESET")), *header), rows=(self,), opened=True)

    @cached_property
    def presets(self) -> Section:
        """Section of stack, layer, and effect preset listings, the popover the editor header opens."""
        return Section(
            key=f"{_OWNERSHIP.key}_presets",
            description=f"Apply, add, or import {_TITLE.lower()}, layer, and effect presets",
            rows=(Layout(builder=Some(lambda identity: self.presets_drawn(identity).map(lambda draw: (draw,))), prerequisites=(self,)),),
        )

    # --- [DRAWING]

    def presets_drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns the preset listing draws of each held stack, layer, or effect, the New Stack button while no stack exists."""

        def listed(stacking: LayerStackService) -> Outcome[RowDraw]:
            listings = self._preset_listings(stacking)
            creating = self.operating[identity][stack_commands.new_stack]
            return accumulated(listing.drawn(identity) for listing in listings).map(lambda draws: partial(_presets_drawn, stacking, creating, tuple(zip(listings, draws))))

        return self.bound(identity).bind(listed)

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns the draw of the layer list, then the selected layer's source, inputs, and node editors, and one panel per layer effect."""
        return self.bound(identity).bind(
            lambda stacking: paired(self._layer_list(stacking).drawn(identity), _realized((Opens(target=PopupTarget(_ADD_EFFECT, PopupKind.MENU), icon=Some("ADD")),), identity)).map(
                lambda parts: partial(self._editor_drawn, stacking, *parts)
            )
        )

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Stack refresh and operators, view attachment, layer rows with the frame size binding stacks read, and the arranger placing owned stack nodes."""
        return self.refresh, self.operating, _VIEW, self.layered, FrameSize.attachment, _Arranger()

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the extension's stack operators and menus over them, listings, the preset popover the section header opens, and the handlers and subscriptions queueing the stack refresh."""
        operators = self.operating[identity]

        def updated_scenes() -> Outcome[None]:
            self.refresh[identity]()
            return Ok(None)

        def rows(stacking: LayerStackService) -> Acquisitions:
            handlers: tuple[Declaration, ...] = (
                Handler(handlers=bpy.app.handlers.load_post, body=updated_scenes),
                *(Subscription(owner=owner, setting=prop, body=updated_scenes) for owner, prop in (*_FRAMING.reads, *_WATCHED)),
            )
            return (_OWNERSHIP, *operators.values(), *self.menus(operators), *self._preset_listings(stacking), self._layer_list(stacking), Panel(section=self.presets, at=_POPOVER), *handlers)

        return self.bound(identity).map(rows)

    # --- [LISTINGS]

    def _preset_listings(self, stacking: LayerStackService) -> tuple[Listing[Valued[LayerStack]], Listing[Valued[StackLayer]], Listing[Valued[Entry]]]:
        """Stack, active layer, and last effect preset listings, each preset holding the input values at its item keys."""

        def applied(context: bpy.types.Context, value: Valued[LayerStack] | Valued[StackLayer] | Valued[Entry]) -> Outcome[Answer]:
            return stacking.applied(context, value).map(lambda wrote: Done() if wrote else Unchanged(reason="Stack matches preset"))

        def matched[T](read: Callable[[bpy.types.Context], Outcome[Valued[T]]], context: bpy.types.Context, values: tuple[Valued[T], ...]) -> Outcome[tuple[bool, ...]]:
            def equal(held: Valued[T], value: Valued[T]) -> bool:
                return held.structure == value.structure and value.rebased(held.prefix).map(lambda values: values == held.values).default_value(value=True)

            return read(context).map(lambda held: tuple(equal(held, value) for value in values))

        def valued[T](pick: Callable[[Selection], Outcome[tuple[T, Key]]], context: bpy.types.Context) -> Outcome[Valued[T]]:
            return stacking.selected(context).bind(
                lambda held: pick(held).bind(lambda found: captured(stacking.identity, held.entry, found[1]).map(lambda values: Valued(structure=found[0], values=values, prefix=found[1])))
            )

        def last(held: Selection) -> Outcome[tuple[Entry, Key]]:
            return held.layer.bind(
                lambda found: Block.of_seq(tuple(zip(found.effects, found.places))[-1:]).try_head().to_result_with(partial(Fault.of, Reason.ABSENT, found.name, member=item_key(EFFECTS)))
            )

        stack = partial(valued, lambda held: Ok((held.record, "")))
        layer = partial(valued, lambda held: held.layer.map(lambda found: (found, found.key)))
        effect = partial(valued, last)
        return (
            Listing(preset=self.preset, current=stack, matches=partial(matched, stack), applied=applied),
            Listing(preset=self.layer_preset, current=layer, matches=partial(matched, layer), applied=applied),
            Listing(preset=self.entry_preset, current=effect, matches=partial(matched, effect), applied=applied),
        )

    # --- [EDITORS]

    def _layer_list(self, stacking: LayerStackService) -> _ItemList[bpy.types.PropertyGroup, LayerRow]:
        """Layer list top first with visibility, solo, and matte columns and edit side buttons, dragged to reorder."""

        def pressing(edit: Edit, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
            stacking.identity.reports.faulted(stacking.selected(context).bind(lambda held: self._edited(layout, stacking.identity, edit, LayerAt(layer=held.active))))

        def spaced(layout: bpy.types.UILayout, _holder: Struct, _context: bpy.types.Context) -> None:
            layout.separator(type="SPACE")

        return _ItemList(
            key=f"{_OWNERSHIP.key}_layers",
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
            absent=(OperatorButton(target=self.operating[stacking.identity][stack_commands.new_stack]),),
            move=Some(stacking.moved),
        )

    def _edited(self, layout: bpy.types.UILayout, identity: Identity, edit: Edit, target: StackTarget) -> Outcome[None]:
        """Draws the icon button of one edit over a target."""
        return _operator_button(
            layout, identity, Placement(self.operating[identity][stack_commands.edit_layer], {**msgspec.to_builtins(target), "action": edit.name}), {"text": "", "icon": edit.value}
        )

    def _editor_drawn(self, stacking: LayerStackService, listing: RowDraw, adding: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the layer list top first, then the selected layer's source, inputs, and node editors, and one panel per layer effect."""
        identity = stacking.identity
        operators = self.operating[identity]

        def edits_row(layout: bpy.types.UILayout, target: StackTarget) -> None:
            line = layout.row(align=True)
            for edit in Edit:
                identity.reports.faulted(self._edited(line, identity, edit, target))

        def mask(layout: bpy.types.UILayout, target: LayerAt | EffectAt) -> None:
            masking = operators[stack_commands.add_mask]
            identity.reports.faulted(_operator_button(layout, identity, Placement(masking, msgspec.to_builtins(target)), {"text": masking.title, "icon": "MOD_MASK"}))

        def detail(held: Selection, layer: StackLayer) -> None:
            nodes, sockets, places = owned(held.tree, identity), InterfaceKeys.of(held.tree, identity).sockets, layer.places

            def widgets(layout: bpy.types.UILayout, prefix: Key, inner: tuple[Key, ...]) -> None:
                keys = frozenset(key for key in (*nodes, *sockets) if key.startswith(f"{prefix}_") and not any(key.startswith(f"{nested}_") for nested in inner))
                _node_editors(layout, (node for key, node in nodes.items() if key in keys))
                for identifier in (identifier for key, identifier in sockets.items() if key in keys):
                    identity.reports.faulted(_interface_input(held.entry, identifier).map(lambda owner: layout.prop(owner, "value")))

            def effected(layout: bpy.types.UILayout, at: EffectAt, place: Key, entry: Entry) -> None:
                header, body = layout.panel(f"{place}_{held.tree.session_uid}", default_closed=False)
                header.label(text=_titled(type(entry.record)))
                edits_row(header, at)
                for inner in Option.of_optional(body).to_list():
                    widgets(inner, place, ())
                    mask(inner, at)
                    for index in range(entry.blend.masks):
                        edits_row(inner, EffectMaskAt(layer=at.layer, entry=at.entry, mask=index))

            at = LayerAt(layer=held.active)
            for source in Option.of_optional(nodes.get(layer.node)).to_list():
                source.draw_buttons_ext(context, layout)
                layout.template_node_inputs(source)
                outputs = layout.row(align=True)
                for output in (output for output in source.outputs if output.enabled):
                    identity.reports.faulted(
                        _operator_button(outputs, identity, Placement(operators[stack_commands.select_output], {"layer": at.layer, "output": output.name}), {"text": output.name, "translate": False})
                    )
            widgets(layout, layer.key, places)
            buttons = layout.row(align=True)
            mask(buttons, at)
            identity.reports.faulted(self._edited(buttons, identity, Edit.RESET, at))
            for index in range(layer.blend.masks):
                edits_row(layout, LayerMaskAt(layer=at.layer, mask=index))
            for index, (place, entry) in enumerate(zip(places, layer.effects)):
                effected(layout, EffectAt(layer=at.layer, entry=index), place, entry)

        listing(layout, holder, context)
        for held in stacking.selected(context).to_option().to_list():
            for layer in held.layer.to_option().to_list():
                detail(held, layer)
                adding(layout, holder, context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LayerStackExtension"]
