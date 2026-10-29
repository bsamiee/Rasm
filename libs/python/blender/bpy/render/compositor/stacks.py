"""Layer stack composition over the effects an extension offers: presets, the operator of every stack command, menus, the editor section with its layer list, the preset section, and the layered document of formed color and raw mask coverage."""

from collections.abc import Callable, Mapping
from contextlib import nullcontext
from enum import Enum
from functools import cached_property, partial
from pathlib import Path
from types import GenericAlias, MappingProxyType
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.calls import Called, Command, Opened, Placement
from blender.bpy.commands.invocations import Open, Popup
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Invoke, Operator, Over, Unchanged
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.mirrors import choices, held_choices, itemized
from blender.bpy.data.rna.paths import root_in, rooted, Struct
from blender.bpy.extension.events import Handler, Subscription
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.extension.reports import alerted
from blender.bpy.nodes.addressing import Addressed, grouped_value, owned
from blender.bpy.nodes.desired import item_key
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.compositor.effects import CompositorEffect, Entry
from blender.bpy.render.compositor.entries import Choice, EffectRows
from blender.bpy.render.compositor.inputs import captured, entry_input, entry_value, FRAMING
from blender.bpy.render.compositor.layers import (
    Adjustment,
    Copied,
    Edit,
    EffectAt,
    EffectMaskAt,
    EFFECTS,
    LAYER_BLEND,
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
    Source,
    stack_file,
    StackLayer,
    StoredLayer,
    Switched,
    Target,
    TermFiltering,
    TITLE,
    VIEW,
)
from blender.bpy.render.compositor.masks import MODE, OPACITY, photoshop
from blender.bpy.render.compositor.recipes import Valued
from blender.bpy.render.compositor.stacking import LayerStacking, nested_tree, Selection
from blender.bpy.render.outputs.formats import extension
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.view_layers.passes import PassFlags
from blender.bpy.settings.attachments import Attachment, Stored
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.lists import ItemList
from blender.bpy.ui.layout.rows import Layout, Member, Opens, OperatorButton, pressed, Property, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Listing
from blender.bpy.ui.surfaces.menus import Dropdown, Menu, Submenus
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.buffers import Coded, Depth, unassociated
from blender.imaging.files.documents import Document, DocumentLayers, LayerGroup, PixelLayer
from blender.imaging.files.multilayer import Multilayer
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type Operators = Mapping[Callable[..., object], Command]
type _Planes = tuple[DocumentLayers[Coded], npt.NDArray[np.float32]]
type _Listed[M: Case] = Listing[Valued[LayerStack[Source, M]]] | Listing[Valued[StackLayer[Source, M]]] | Listing[Valued[Entry[M]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

_ADDING: Final[Key] = "add_layer"
_EFFECTING: Final[Key] = "layer_effect_add"
_SPECIALS: Final[Key] = f"{OWNERSHIP.key}_specials"
_MEDIA: Final = (*sorted(bpy.path.extensions_image), *sorted(bpy.path.extensions_movie))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_WATCHED: Final = tuple(
    (owner, owner.bl_rna.properties[name])
    for owner, name in ((bpy.types.RenderSettings, "film_transparent"), (bpy.types.ViewLayer, "name"), (bpy.types.ViewLayer, "use"), (bpy.types.Object, "lightgroup"), (bpy.types.World, "lightgroup"))
)
_STACKING: Final = (
    LayerStacking.duplicate_stack,
    LayerStacking.switch_stack,
    LayerStacking.edit_nested_stack,
    LayerStacking.copy_layer,
    LayerStacking.copy_all_layers,
    LayerStacking.clear_layers,
    LayerStacking.remove_stack,
)
_SOURCING: Final = (
    LayerStacking.add_adjustment_layer,
    LayerStacking.add_render_pass_layer,
    LayerStacking.add_recombined_layer,
    LayerStacking.add_node_layer,
    LayerStacking.add_nested_stack,
    LayerStacking.add_media,
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _presets_drawn[M: Case](
    stacking: LayerStacking[M], creating: Command, listings: tuple[tuple[_Listed[M], RowDraw], ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context
) -> None:
    """Draws the preset listing of each held stack, layer, or effect, the New Stack button while no stack exists."""
    if rooted(bpy.types.Scene, context).bind(stacking.head).is_error():
        alerted(layout, pressed(layout, stacking.identity, Placement(creating), {"text": creating.title}))
    for listing, draw in listings:
        if listing.current(context).is_ok():
            draw(layout, holder, context)


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
        return Attachment(key=f"{OWNERSHIP.key}_layers", form=LayerRows, owner=bpy.types.NodeTree, changed=Some(lambda identity: nullcontext(lambda _owner, _context: identity.held[self.refresh]())))

    @cached_property
    def preset(self) -> Preset[Valued[LayerStack[Source, M]]]:
        """Stack presets with their input values over the stock looks."""
        return Preset(key=OWNERSHIP.key, form=GenericAlias(Valued, (GenericAlias(LayerStack, (Source, self.rows.form)),)), stock=self.looks)  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    @cached_property
    def layer_preset(self) -> Preset[Valued[StackLayer[Source, M]]]:
        """Presets of one layer with its input values."""
        return Preset(key=f"{OWNERSHIP.key}_layer", form=GenericAlias(Valued, (GenericAlias(StackLayer, (Source, self.rows.form)),)))  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    @cached_property
    def entry_preset(self) -> Preset[Valued[Entry[M]]]:
        """Presets of one effect with its blend and input values."""
        return Preset(key=f"{OWNERSHIP.key}_entry", form=GenericAlias(Valued, (GenericAlias(Entry, (self.rows.form,)),)))  # type: ignore[arg-type]  # ty: ignore[invalid-type-form]

    # --- [COMMANDS]

    def command[T: bpy.types.PropertyGroup](
        self, body: Callable[[LayerStacking[M], Call[T]], Outcome[Answer]], form: type[T], invoke: Option[Invoke[T]] = Nothing
    ) -> tuple[Callable[..., object], Command]:
        """Returns a stack command body with the operator running it over the calling extension's stacking, keyed and described by the body."""
        return body, Operator[T](form=form, run=Over(self.bound, body), invoke=invoke)

    def operators(self, identity: Identity) -> Operators:
        """Builds the operator of every stack command once for an extension, its stack pickers listing the extension's stacks and the chosen stack's layer rows, its effect choice the accepted effects."""

        def stacks(_owner: Struct, _context: bpy.types.Context | None) -> tuple[tuple[str, str, str, str, int], ...]:
            return held_choices(tuple((tree.name, tree.name, "") for tree, _ in OWNERSHIP.held(identity)))

        def layers(owner: Struct, _context: bpy.types.Context | None) -> tuple[tuple[str, str, str, str, int], ...]:
            stack: str = owner.stack  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            rows = self.layered.bound(identity).map(
                lambda attached: tuple((row.key, row.name, "") for tree, _ in OWNERSHIP.held(identity) if tree.name == stack for row in attached.struct(tree).layers)
            )
            return held_choices(rows.default_value(()))

        switched, copied = itemized(Switched, stack=stacks), itemized(Copied, stack=stacks, layer=layers)
        chosen = itemized(Choice, kind=choices((row.kind, row.title, row.description) for row in self.accepted))
        return MappingProxyType(
            dict((
                self.command(LayerStacking.new_stack, Empty),
                self.command(LayerStacking.duplicate_stack, Empty),
                self.command(LayerStacking.switch_stack, switched, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup)))),
                self.command(LayerStacking.edit_nested_stack, Empty),
                self.command(LayerStacking.copy_layer, copied, Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog)))),
                self.command(LayerStacking.copy_all_layers, switched, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup)))),
                self.command(LayerStacking.clear_layers, Empty),
                self.command(LayerStacking.remove_stack, Empty),
                self.command(LayerStacking.add_adjustment_layer, Empty),
                self.command(LayerStacking.add_render_pass_layer, PassSourced, Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog)))),
                self.command(LayerStacking.add_recombined_layer, RecombinedSourced, Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog)))),
                self.command(LayerStacking.add_node_layer, NodeKind, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup)))),
                self.command(LayerStacking.add_nested_stack, switched, Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup)))),
                self.command(LayerStacking.add_media, Empty, Some(Open(extensions=lambda _: _MEDIA))),
                self.command(LayerStacking.select_output, OutputChoice),
                self.command(LayerStacking.add_mask, LayerEdit),
                self.command(LayerStacking.edit_layer, LayerEdit),
                self.command(LayerStacking.add_effect_to_layer, chosen),
            ))
        )

    # --- [MENUS]

    def menus(self, operators: Operators) -> tuple[Menu, ...]:
        """Menus adding a layer of each source, appending an accepted effect, one submenu per group where groups are given, and the stack commands beside the layer rows."""
        effects = tuple((row, OperatorButton(target=Placement(operators[LayerStacking.add_effect_to_layer], {"kind": row.kind}), keywords={"text": row.title})) for row in self.accepted)
        return (
            Menu(key=_ADDING, description="Add layer source to stack", body=Dropdown(entries=tuple(OperatorButton(target=operators[body]) for body in _SOURCING))),
            Menu(
                key=_EFFECTING,
                title=Some("Add Effect"),
                description="Add effect to selected layer",
                body=self.groups.map(lambda group: Submenus(entries=tuple((group(row), button) for row, button in effects))).default_with(
                    lambda: Dropdown(entries=tuple(button for _, button in effects), search=True)
                ),
            ),
            Menu(key=_SPECIALS, description="Stack and layer commands beside the layer rows", body=Dropdown(entries=tuple(OperatorButton(target=operators[body]) for body in _STACKING))),
        )

    # --- [BINDING]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Stack refresh and operators, view attachment, and layer rows with the frame size binding stacks read."""
        return self.refresh, self.operating, VIEW, self.layered, FrameSize.attachment

    def bound(self, identity: Identity) -> Outcome[LayerStacking[M]]:
        """Binds the store of each layer's structure, the layer rows and view attachments, and pass flags of an extension."""
        stored = Stored.keyed(identity, OWNERSHIP.key, msgspec.msgpack.Decoder(GenericAlias(dict, (Key, GenericAlias(StoredLayer, (Source, self.rows.form))))))
        return paired(stored, self.layered.bound(identity), VIEW.bound(identity)).map(
            lambda parts: LayerStacking(identity=identity, rows=self.rows, filtering=self.filtering, store=parts[0], layer_rows=parts[1], view=parts[2], flags=PassFlags.of())
        )

    # --- [DOCUMENTS]

    def document(self, identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> Outcome[Document[Coded]]:
        """Layered document of formed color and raw mask coverage of the scene's head stack at a saved frame, formed through the stack view, else the scene's, nested stacks as groups.

        Returns:
            `Ok` of the document. `Error` of `absent` for a missing file, layer, or frame number, and `refused` for an unreadable number or opacity.
        """
        config = PyOpenColorIO.GetCurrentConfig()
        number = (
            Block
            .of_seq(frame.parts[:1])
            .choose(lambda part: Option.of_optional(part.header.get("Frame")))
            .try_head()
            .to_result_with(partial(Fault.of, Reason.ABSENT, str(frame.path), member="Frame"))
            .bind(lambda text: catch(int, exception=ValueError)(text).map_error(partial(Fault.of, Reason.REFUSED, str(frame.path))))
        )

        def documented(stacking: LayerStacking[M], view: DisplayView) -> Outcome[Document[Coded]]:
            def read_tree(tree: bpy.types.NodeTree, record: LayerStack[Source, M], at: int, valued: Callable[[Key], Outcome[Value]]) -> Outcome[_Planes]:
                shown = frozenset(layer.key for layer, _ in record.composited())
                path = (
                    Block
                    .of_seq(owned(tree, identity).values())
                    .choose(lambda node: Option.of_optional(try_downcast(bpy.types.CompositorNodeOutputFile, node)))
                    .try_head()
                    .bind(lambda node: Option.of_optional(node.format).bind(extension).map(lambda suffix: Path(bpy.path.abspath(node.directory)) / f"{stack_file(tree.session_uid, Some(at))}{suffix}"))
                    .to_result_with(partial(Fault.of, Reason.ABSENT, repr(tree.nodes), member=bpy.types.CompositorNodeOutputFile.__name__))
                )

                def planes(file: Multilayer) -> Outcome[_Planes]:
                    def raw(name: str) -> Outcome[npt.NDArray[np.float32]]:
                        found = Block.of_seq(file.layers).filter(lambda layer: layer.name == name).try_head().to_result_with(partial(Fault.of, Reason.ABSENT, str(file.path), member=name))
                        return found.bind(lambda layer: file.plane(layer, layer.channels))

                    def color(name: str) -> Outcome[tuple[npt.NDArray[Coded], npt.NDArray[Coded]]]:
                        return raw(name).bind(lambda pixels: view.form(config, pixels)).map(unassociated).bind(lambda pixels: paired(depth.coded(pixels[..., :3]), depth.coded(pixels[..., 3])))

                    def blend(layer: StackLayer[Source, M]) -> Outcome[tuple[object, float]]:
                        read = partial(item_key, layer.key, LAYER_BLEND)
                        return paired(valued(read(MODE.key)), valued(read(OPACITY.key))).bind(
                            lambda pair: (
                                catch(msgspec.convert, exception=msgspec.ValidationError)(pair[1], float)
                                .map_error(partial(Fault.of, Reason.REFUSED, layer.key))
                                .map(lambda opacity: (pair[0], opacity))
                            )
                        )

                    def each(layer: StackLayer[Source, M]) -> Outcome[PixelLayer[Coded] | LayerGroup[Coded]]:
                        visible = layer.key in shown
                        if isinstance(layer.source, Adjustment):
                            return color(f"{layer.key} Composite").map(lambda codes: PixelLayer(name=layer.name, color=codes[0], alpha=codes[1], visible=visible))
                        mask = raw(f"{layer.key} Mask").bind(lambda pixels: depth.coded(pixels[..., 0])).map(Some) if layer.blend.masks or layer.matte else Ok(Nothing)
                        nested = (
                            nested_tree(identity, tree, layer)
                            .filter(lambda _: layer.plain)
                            .map(
                                lambda inner: paired(stacking.read(inner), blend(layer)).bind(
                                    lambda parts: read_tree(inner, parts[0], at, partial(grouped_value, identity, inner, owned(tree, identity)[layer.node])).map(
                                        lambda held: LayerGroup(name=layer.name, layers=held[0], mode=Some(photoshop(str(parts[1][0]))), opacity=parts[1][1], visible=visible)
                                    )
                                )
                            )
                        )
                        return nested.default_with(
                            lambda: paired(color(layer.key), mask, blend(layer)).map(
                                lambda parts: PixelLayer(name=layer.name, color=parts[0][0], alpha=parts[0][1], mask=parts[1], mode=photoshop(str(parts[2][0])), opacity=parts[2][1], visible=visible)
                            )
                        )

                    return paired(accumulated(each(layer) for layer in record.layers), raw("Composite").bind(lambda pixels: view.form(config, pixels)).map(unassociated))

                return path.bind(Multilayer.read).bind(planes)

            def composed(entry: bpy.types.SceneCompositorEffect, tree: bpy.types.NodeTree, record: LayerStack[Source, M], at: int) -> Outcome[Document[Coded]]:
                density = scene.render.ppm_factor / scene.render.ppm_base
                space = view.colorspace(config).to_result_with(partial(Fault.of, Reason.ABSENT, repr(config), member=f"{view.display}/{view.view}"))
                return paired(read_tree(tree, record, at, partial(entry_value, identity, entry)), space).bind(
                    lambda parts: depth.coded(parts[0][1]).map(lambda composite: Document(layers=parts[0][0], composite=composite, colorspace=parts[1].getName(), density=density))
                )

            return stacking.head(scene).bind(lambda held: paired(stacking.read(held[1]), number).bind(lambda parts: composed(held[0], held[1], *parts)))

        return paired(self.bound(identity), self.view(identity, scene)).bind(lambda parts: documented(parts[0], parts[1].default_with(lambda: viewed(scene))))

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
            listings = self._listings(stacking)
            creating = identity.held[self.operating][LayerStacking.new_stack]
            return accumulated(listing.drawn(identity) for listing in listings).map(lambda draws: partial(_presets_drawn, stacking, creating, tuple(zip(listings, draws))))

        return self.bound(identity).bind(listed)

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Returns the draw of the layer list, then the selected layer's source, inputs, and node editors, and one panel per layer effect."""
        return self.bound(identity).bind(
            lambda stacking: paired(self._listing(stacking).drawn(identity), realized((Opens(target=Opened(_EFFECTING, Called.MENU), icon=Some("ADD")),), identity)).map(
                lambda parts: partial(self._editor_drawn, stacking, *parts)
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
            return (OWNERSHIP, *operators.values(), *self.menus(operators), *self._listings(stacking), self._listing(stacking), *handlers)

        return self.bound(identity).map(rows)

    # --- [LISTINGS]

    def _listings(self, stacking: LayerStacking[M]) -> tuple[Listing[Valued[LayerStack[Source, M]]], Listing[Valued[StackLayer[Source, M]]], Listing[Valued[Entry[M]]]]:
        """Stack, active layer, and last effect preset listings, each preset holding the input values at its item keys."""

        def applied(context: bpy.types.Context, value: Valued[LayerStack[Source, M]] | Valued[StackLayer[Source, M]] | Valued[Entry[M]]) -> Outcome[Answer]:
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
                lambda found: (
                    Block
                    .of_seq(found.effects[-1:])
                    .try_head()
                    .to_result_with(partial(Fault.of, Reason.ABSENT, found.name, member=item_key(EFFECTS)))
                    .map(lambda entry: (entry, item_key(found.key, EFFECTS, len(found.effects))))
                )
            )

        stack: Callable[[bpy.types.Context], Outcome[Valued[LayerStack[Source, M]]]] = partial(valued, lambda held: Ok((held.record, "")))
        layer: Callable[[bpy.types.Context], Outcome[Valued[StackLayer[Source, M]]]] = partial(valued, lambda held: held.layer.map(lambda found: (found, found.key)))
        effect: Callable[[bpy.types.Context], Outcome[Valued[Entry[M]]]] = partial(valued, last)
        return (
            Listing(preset=self.preset, current=stack, matches=partial(matched, stack), applied=applied),
            Listing(preset=self.layer_preset, current=layer, matches=partial(matched, layer), applied=applied),
            Listing(preset=self.entry_preset, current=effect, matches=partial(matched, effect), applied=applied),
        )

    # --- [EDITOR]

    def _listing(self, stacking: LayerStacking[M]) -> ItemList[bpy.types.PropertyGroup, LayerRow]:
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
                Opens(target=Opened(_ADDING, Called.MENU), keywords={"text": ""}, icon=Some("ADD")),
                partial(pressing, Edit.REMOVE),
                spaced,
                Opens(target=Opened(_SPECIALS, Called.MENU), keywords={"text": ""}, icon=Some("DOWNARROW_HLT")),
                spaced,
                *(partial(pressing, edit) for edit in (Edit.DUPLICATE, Edit.UP, Edit.DOWN, Edit.RESET)),
            ),
            absent=(OperatorButton(target=stacking.identity.held[self.operating][LayerStacking.new_stack]),),
            move=Some(stacking.moved),
        )

    def _edited(self, layout: bpy.types.UILayout, identity: Identity, edit: Edit, target: Target) -> Outcome[None]:
        """Draws the icon button of one edit over a target."""
        return pressed(layout, identity, Placement(identity.held[self.operating][LayerStacking.edit_layer], {**msgspec.to_builtins(target), "action": edit.name}), {"text": "", "icon": edit.value})

    def _editor_drawn(self, stacking: LayerStacking[M], listing: RowDraw, adding: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the layer list top first, then the selected layer's source, inputs, and node editors, and one panel per layer effect."""
        identity = stacking.identity
        operators = identity.held[self.operating]

        def edits_row(layout: bpy.types.UILayout, target: Target) -> None:
            line = layout.row(align=True)
            for edit in Edit:
                alerted(line, self._edited(line, identity, edit, target))

        def mask(layout: bpy.types.UILayout, target: LayerAt | EffectAt) -> None:
            masking = operators[LayerStacking.add_mask]
            alerted(layout, pressed(layout, identity, Placement(masking, msgspec.to_builtins(target)), {"text": masking.title, "icon": "MOD_MASK"}))

        def widgets(layout: bpy.types.UILayout, held: Selection[M], prefix: Key, inner: tuple[Key, ...]) -> None:
            def mine(key: Key) -> bool:
                return key.startswith(f"{prefix}_") and not any(key.startswith(f"{nested}_") for nested in inner)

            for node in (node for key, node in owned(held.tree, identity).items() if mine(key)):
                column = layout.column()
                column.use_property_split = False
                column.label(text=node.label or node.bl_label)
                column.template_node_inputs(node)
            for key in (key for keys in Addressed.of(held.tree, identity).map(lambda live: live.sockets).to_option().to_list() for key in keys if mine(key)):
                alerted(layout, entry_input(identity, held.entry, key).map(lambda owner: layout.prop(owner, "value")))

        def effected(layout: bpy.types.UILayout, held: Selection[M], at: EffectAt, place: Key, entry: Entry[M]) -> None:
            header, body = layout.panel(f"{place}_{held.tree.session_uid}", default_closed=False)
            header.label(text=self.rows.offering(type(entry.record)).map(lambda row: row.title).default_value(bpy.path.display_name(item_key(type(entry.record)))))
            edits_row(header, at)
            for inner in Option.of_optional(body).to_list():
                widgets(inner, held, place, ())
                mask(inner, at)
                for index in range(entry.blend.masks):
                    edits_row(inner, EffectMaskAt(layer=at.layer, entry=at.entry, mask=index))

        def detail(held: Selection[M], layer: StackLayer[Source, M]) -> None:
            at = LayerAt(layer=held.active)
            places = tuple(item_key(layer.key, EFFECTS, index) for index in range(1, len(layer.effects) + 1))
            for source in Option.of_optional(owned(held.tree, identity).get(layer.node)).to_list():
                source.draw_buttons_ext(context, layout)
                layout.template_node_inputs(source)
                outputs = layout.row(align=True)
                for output in (output for output in source.outputs if output.enabled):
                    alerted(
                        outputs, pressed(outputs, identity, Placement(operators[LayerStacking.select_output], {"layer": at.layer, "output": output.name}), {"text": output.name, "translate": False})
                    )
            widgets(layout, held, layer.key, places)
            buttons = layout.row(align=True)
            mask(buttons, at)
            alerted(buttons, self._edited(buttons, identity, Edit.RESET, at))
            for index in range(layer.blend.masks):
                edits_row(layout, LayerMaskAt(layer=at.layer, mask=index))
            for index, entry in enumerate(layer.effects):
                effected(layout, held, EffectAt(layer=at.layer, entry=index), places[index], entry)

        listing(layout, holder, context)
        for held in stacking.selected(context).to_option().to_list():
            for layer in held.layer.to_option().to_list():
                detail(held, layer)
                adding(layout, holder, context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LayerStacks"]
