"""View layer pass routing: the per-layer plans, the routing group reconciled into File Output nodes, the files it wrote, presets, and its commands."""

from collections.abc import Callable, Mapping
import datetime
import functools
import itertools
from types import MappingProxyType
from typing import ClassVar, Final

import bpy
from expression import Option, Some
from expression.collections import Block, Map
import mathutils
import msgspec

from rasm_bpy.commands.invocations import Popup
from rasm_bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from rasm_bpy.data._extension.events import _runtime
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions, Handler, Registrable, Subscription
from rasm_bpy.data._ids.ownership import Ownership, Saved
from rasm_bpy.data._ids.store import IdName
from rasm_bpy.data._rna.paths import rooted, Struct
from rasm_bpy.data._rna.properties import _flagged, choices, EnumItem
from rasm_bpy.data._rna.snapshots import assigned, Snapshot
from rasm_bpy.nodes._addressing import owned
from rasm_bpy.nodes._desired import FrameNode, Link
from rasm_bpy.nodes._graphs import Expr, group_input
from rasm_bpy.nodes._interfaces import Interface, Socket
from rasm_bpy.nodes.changes import Changes, made, NodeGroup, reconciled
from rasm_bpy.nodes.functions import built, lowered
from rasm_bpy.render._outputs.exports import Layering
from rasm_bpy.render._outputs.formats import _FileItems
from rasm_bpy.render._outputs.paths import _file_output_path, _today, OutputPath, Outputs, Pathed, ScenePaths
from rasm_bpy.render._outputs.results import Written
from rasm_bpy.render._view_layers.layers import _LAYER_TAG, LayerTag, Role
from rasm_bpy.render._view_layers.passes import _EMISSION, _ENVIRONMENT, _layer_of, _listed, _OCCLUSION, _required, _synced, _used, CryptomatteKind, Derived, Family, Flags, PassFlags, PassRequest
from rasm_bpy.render._view_layers.sources import _document, _file_node, _file_outputs, _lobe, _NUKE_CHANNELS, _ROUTING, _sources, FileOutput, FileSocket, FileSplit, ids_recolor, Recoloring, Routing
from rasm_bpy.scene.cameras import render_views
from rasm_bpy.settings._attachments import Attached
from rasm_bpy.settings._presets import Preset
from rasm_bpy.ui._layout.rows import _FUNCTIONS, _realized, ConditionalRow, Layout, OperatorButton, RowDraw
from rasm_bpy.ui._layout.sections import Section
from rasm_bpy.ui._layout.values import Listing, Store
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, paired, Reason, Retention, transfer

# --- [CONSTANTS] ------------------------------------------------------------------------

_WATCHED: Final = tuple(
    (owner, owner.bl_rna.properties[member])
    for owner, members in (
        (bpy.types.ViewLayer, ("use", "name", "pass_cryptomatte_depth", "material_override")),
        (bpy.types.AOV, ("name", "type")),
        (bpy.types.Lightgroup, ("name",)),
        (bpy.types.Object, ("lightgroup", "color", "hide_render", "parent")),
        (bpy.types.World, ("lightgroup",)),
        (bpy.types.Material, ("diffuse_color",)),
        (bpy.types.RenderSettings, ("engine",)),
        (bpy.types.Scene, ("camera",)),
    )
    for member in members
)

# --- [MODELS] ---------------------------------------------------------------------------


class LayerPlan(msgspec.Struct, frozen=True, kw_only=True):
    """Frame key and plan per File Output key of one used view layer."""

    frame: Key
    file_outputs: Mapping[Key, FileOutput]


class PassPreset(msgspec.Struct, frozen=True, kw_only=True):
    """Pass flags and routing saved together."""

    flags: Snapshot
    routing: Snapshot


class PassRouting(msgspec.Struct, frozen=True, kw_only=True):
    """`RoutingRow` bound to an identity, with the attachments its plans read."""

    identity: Identity
    routing: Attached[Routing]
    pathed: Pathed
    layers: Attached[LayerTag]
    lobe: NodeGroup

    def held(self, scene: bpy.types.Scene) -> Option[bpy.types.NodeTree]:
        """Returns routing tree identity holds for scene."""
        return RoutingRow.ownership.found(self.identity, of=Some(scene))

    def file_node(self, scene: bpy.types.Scene, view: str, file_output: FileOutput, key: Key) -> Outcome[Expr]:
        """Builds one File Output node of a view layer under its routing at today's scene path."""
        return _layer_of(scene, view).bind(
            lambda layer: self.pathed.path(scene, _today()).bind(lambda path: _file_node(self.pathed, path, view, file_output, key, nuke=self.routing.struct(layer).nuke))
        )

    def framed(
        self, scene: bpy.types.Scene, held: dict[Key, bpy.types.Node], path: Outcome[OutputPath], prefix: Key, layer: bpy.types.ViewLayer
    ) -> Outcome[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
        """Plans frame, Render Layers node, sources, and File Output nodes of one used layer, the output path read only by File Output nodes."""
        render_key = f"{prefix}_render"
        rendered = Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(scene), "layer": layer.name}, key=Some(render_key))
        routing, record = self.routing.struct(layer), self.layers.struct(layer)

        def planned(plan: Mapping[Key, FileOutput]) -> Outcome[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
            return (
                (path.bind(lambda at: accumulated(_file_node(self.pathed, at, layer.name, file_output, key, nuke=routing.nuke) for key, file_output in plan.items())) if plan else Ok(()))
                .bind(lambda files: lowered(bpy.types.CompositorNodeTree, rendered, *files))
                .map(
                    lambda merged: (
                        FrameNode(members={"label": layer.name, "use_custom_color": True, "color": mathutils.Color(record.color).from_scene_linear_to_srgb()[:]}, nodes=merged.nodes),
                        merged.links,
                        LayerPlan(frame=prefix, file_outputs=plan),
                    )
                )
            )

        return (
            Option
            .of_optional(held.get(render_key))
            .map(lambda live: _sources(scene, layer, rendered, live, routing, Role[record.role], self.lobe))
            .default_value(Ok(()))
            .bind(lambda found: _file_outputs(found, FileSplit[routing.split]))
            .bind(lambda found: planned(MappingProxyType({f"{prefix}_{file_output.output.name.lower()}_{'layered' if file_output.format.layered else 'exr'}": file_output for file_output in found})))
        )

    def planned(self, scene: bpy.types.Scene, tree: Option[bpy.types.NodeTree], path: Outcome[OutputPath]) -> Outcome[tuple[NodeGroup, dict[bpy.types.ViewLayer, LayerPlan]]]:
        """Plans scene routing group, reading offered sockets from held tree."""
        held = tree.map(lambda found: owned(found, self.identity)).default_value({})
        layers = _used(scene)
        return accumulated(self.framed(scene, held, path, f"layer_{index}", layer) for index, layer in enumerate(layers)).bind(
            lambda frames: _grouped(frames).map(lambda group: (group, {layer: plan for layer, (_, _, plan) in zip(layers, frames)}))
        )

    def reconciled(self, scene: bpy.types.Scene, context: bpy.types.Context, path: Outcome[OutputPath]) -> Outcome[Changes]:
        """Reconciles passes, AOVs, light groups, and routing to scene settings, File Output nodes and items taking their formats, called after overrides apply or revert."""
        flags = PassFlags.of()

        def rebuilt(first: NodeGroup) -> Outcome[Changes]:
            return made(first, self.identity, of=Some(scene)).bind(lambda tree: self.planned(scene, Some(tree), path).bind(lambda planned: reconciled(planned[0], tree, self.identity)))

        return (
            _required(
                scene,
                context,
                tuple(
                    PassRequest(layer=layer.name, flags=flags.enabled_for(_flagged(Family, routing.totals), _flagged(Derived, routing.derived), _flagged(CryptomatteKind, routing.ids)))
                    for layer in _used(scene)
                    for routing in (self.routing.struct(layer),)
                ),
            )
            .bind(lambda _: _synced(scene, self.identity))
            .bind(lambda _: self.planned(scene, self.held(scene), path))
            .bind(lambda first: rebuilt(first[0]))
        )

    def placed(self, scene: bpy.types.Scene, path: OutputPath) -> Outcome[OutputPath]:
        """Stores path as the scene's render path, then reconciles routing to it."""
        return self.pathed.placed(scene, path).bind(lambda _: self.reconciled(scene, bpy.context, Ok(path))).map(lambda _: path)

    def refreshed(self, context: bpy.types.Context, day: datetime.date) -> Outcome[None]:
        """Reconciles every editable scene with stored layer routing at `day`, owned runtime scenes skipped."""

        def refresh(scene: bpy.types.Scene) -> Outcome[Changes]:
            return self.reconciled(scene, context, self.pathed.path(scene, day))

        return rooted(bpy.types.BlendData, context).bind(
            lambda data: accumulated(
                refresh(scene) for scene in data.scenes if scene.is_editable and not _runtime(self.identity, scene) and any(self.routing.stored(layer) for layer in scene.view_layers)
            ).map(lambda _: None)
        )

    def watched(self) -> tuple[Subscription, ...]:
        """Builds subscriptions refreshing routing, one per pass flag, watched setting, and runtime attachment member."""
        groups = tuple((form, prop) for form in (Routing, Outputs, LayerTag) for prop in form.bl_rna.properties if prop.is_runtime)
        return tuple(
            Subscription(owner=owner, setting=prop, body=lambda: self.refreshed(bpy.context, _today()))
            for owner, prop in (*((owner, flag.prop) for owner, flag in PassFlags.of().every), *_WATCHED, *groups)
        )

    def listing(self) -> Listing[PassPreset]:
        """Returns pass presets over layer flags and routing, applied to every used layer."""
        flags = PassFlags.of()
        families = flags.families
        selection = Some(tuple(flag for _, flag in flags.every))
        eevee = frozenset().union(
            *(direct | color for direct, _, color in (families[Family.DIFFUSE], families[Family.GLOSSY])),
            families[Family.VOLUME][0] & frozenset(flag for owner, flag in flags.every if owner is bpy.types.ViewLayerEEVEE),
        )
        common = frozenset({_EMISSION, _ENVIRONMENT})
        shadows = frozenset(flag for _, flag in flags.every if flag.prop.identifier in {"use_pass_transparent", "use_pass_shadow"})
        recipes = (
            ("lightray_rebuild", common | flags.lobes, (), (Derived.DIRECT_LIGHT, Derived.INDIRECT_LIGHT)),
            ("material_rebuild", common | flags.lobes, tuple(Family), ()),
            ("multipass_breakdown", common | flags.lobes, (), ()),
            ("eevee_material_rebuild", common | eevee, (Family.DIFFUSE, Family.GLOSSY, Family.VOLUME), ()),
            ("eevee_multipass", common | eevee | shadows | {_OCCLUSION}, (), ()),
        )

        def stock(enabled: Flags, totals: tuple[Family, ...], derived: tuple[Derived, ...]) -> PassPreset:
            return PassPreset(
                flags=Snapshot.of(bpy.types.ViewLayer, {flag.path: flag in enabled for _, flag in flags.every}),
                routing=Snapshot.of(Routing, {("totals",): tuple(sorted(str(member.name) for member in totals)), ("derived",): tuple(sorted(str(member.name) for member in derived))}),
            )

        def captured(layer: bpy.types.ViewLayer) -> Outcome[PassPreset]:
            return paired(Snapshot.capture(layer, selection), Snapshot.capture(self.routing.struct(layer))).map(lambda pair: PassPreset(flags=pair[0], routing=pair[1]))

        def same(current: PassPreset, value: PassPreset) -> bool:
            return not transfer(current.flags.tree, value.flags.tree) and not transfer(current.routing.tree, value.routing.tree)

        def applied(value: PassPreset, layer: bpy.types.ViewLayer) -> Outcome[object]:
            return paired(value.flags.apply(layer).outcome, value.routing.apply(self.routing.struct(layer)).outcome)

        return _listed(msgspec.structs.replace(RoutingRow.presets, stock={name: stock(enabled, totals, derived) for name, enabled, totals, derived in recipes}), captured, same, applied)


class RoutingRow(ConditionalRow, frozen=True):
    """Row of pass routing, its layering the ID document export of a written frame."""

    key: ClassVar[Key] = _ROUTING.key
    prerequisites: ClassVar[tuple[Registrable, ...]] = (_ROUTING, _LAYER_TAG, ScenePaths.attachment)
    presets: ClassVar[Preset[PassPreset]] = Preset(key=key, form=PassPreset)
    ownership: ClassVar[Ownership[bpy.types.CompositorNodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key=key, saved=Saved.USED)
    layering: ClassVar[Layering] = Layering(key=key, document=_document)
    title: ClassVar[str] = bpy.path.display_name(key)

    @staticmethod
    def bound(identity: Identity) -> Outcome[PassRouting]:
        """Binds attachments and the lobe group."""
        return paired(_ROUTING.bound(identity), ScenePaths().bound(identity), _LAYER_TAG.bound(identity), _lobe()).map(
            lambda parts: PassRouting(identity=identity, routing=parts[0], pathed=parts[1], layers=parts[2], lobe=parts[3])
        )

    def written(self, scene: bpy.types.Scene, frame: int, identity: Identity, animation: Option[bool]) -> Outcome[tuple[Written, ...]]:
        """Reads routed native filenames and formats under the producing call's animation mode, none without a routing tree."""

        def files(routing: PassRouting, layer: bpy.types.ViewLayer, live: bpy.types.CompositorNodeOutputFile, file_output: FileOutput) -> Outcome[tuple[Written, ...]]:
            nuke = routing.routing.struct(layer).nuke

            def targeted() -> tuple[tuple[bpy.types.ImageFormatSettings, Option[bpy.types.NodeCompositorFileOutputItem], Map[str, str]], ...]:
                stored = _FileItems.held(live, identity)
                settings: bpy.types.ImageFormatSettings = live.format  # ty: ignore[invalid-assignment]
                if settings.file_format == "OPEN_EXR_MULTILAYER":
                    return ((settings, Nothing, Map.of_seq((item.name, source.name) for source in file_output.sources for item in Option.of_optional(stored.get(source.key)).to_list())),)

                def formatted(item: bpy.types.NodeCompositorFileOutputItem) -> bpy.types.ImageFormatSettings:
                    held: bpy.types.ImageFormatSettings = item.format if item.override_node_format else settings  # ty: ignore[invalid-assignment]
                    return held

                return tuple((formatted(item), Some(item), Map.of_seq((("", source.name),))) for source in file_output.sources for item in Option.of_optional(stored.get(source.key)).to_list())

            def written_at(settings: bpy.types.ImageFormatSettings, item: Option[bpy.types.NodeCompositorFileOutputItem], passes: Map[str, str]) -> Outcome[tuple[Written, ...]]:
                individual = settings.views_format == "INDIVIDUAL" or (settings.file_format == "OPEN_EXR_MULTILAYER" and settings.views_format != "MULTIVIEW")
                views = tuple(map(Some, render_views(scene.render))) if scene.render.use_multiview and individual else (Nothing,)
                return accumulated(
                    _file_output_path(scene, live, settings, frame, animation=animation, item=item, view=view).map(
                        lambda file: Written.of(
                            str(file), settings, file_output.output, _NUKE_CHANNELS if nuke else MappingProxyType({}), Some(layer.name), passes, view=view.map(lambda held: held.name)
                        )
                    )
                    for view in views
                ).map(lambda found: tuple(file for held in found for file in held.to_list()))

            return accumulated(itertools.starmap(written_at, targeted())).map(lambda found: tuple(file for files in found for file in files))

        def collected(routing: PassRouting, tree: bpy.types.NodeTree) -> Outcome[tuple[Written, ...]]:
            held = owned(tree, identity)
            keys = {node: key for key, node in held.items()}
            current = routing.pathed.path(scene, _today())
            return (
                accumulated(
                    _layer_of(scene, source.layer).bind(lambda layer: routing.framed(scene, held, current, prefix, layer).map(lambda frame: (layer, frame[2])))
                    for source in held.values()
                    if isinstance(source, bpy.types.CompositorNodeRLayers)
                    for parent in Option.of_optional(source.parent).to_list()
                    for prefix in Option.of_optional(keys.get(parent)).to_list()
                )
                .bind(
                    lambda plans: accumulated(
                        files(routing, layer, live, file_output)
                        for layer, plan in plans
                        for key, file_output in plan.file_outputs.items()
                        for live in (held.get(key),)
                        if isinstance(live, bpy.types.CompositorNodeOutputFile)
                    )
                )
                .map(lambda parts: tuple(itertools.chain.from_iterable(parts)))
            )

        return self.bound(identity).bind(lambda routing: routing.held(scene).map(functools.partial(collected, routing)).default_value(Ok(())))

    @staticmethod
    def commands(identity: Identity) -> tuple[Operator[Empty] | Operator[Recoloring] | Operator[bpy.types.PropertyGroup], ...]:
        """Returns the routing commands, the file row search listing the context layer's unlisted sources."""
        return (passes_update, ids_recolor, _adding(identity.package))

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Draws the routing commands over the preset rows."""
        buttons = (
            OperatorButton(target=passes_update),
            OperatorButton(target=ids_recolor, function=_FUNCTIONS["operator_enum"], keywords={"property": "mode"}),
            OperatorButton(target=_adding(identity.package)),
        )
        return self.bound(identity).bind(
            lambda routing: _realized((Layout(rows=buttons, container=Some(lambda layout: layout.grid_flow(row_major=True, even_columns=True))), routing.listing()), identity)
        )

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns routing acquisitions."""
        return self.bound(identity).map(
            lambda routing: (
                self.ownership,
                routing.lobe,
                *self.commands(identity),
                routing.listing(),
                *(Handler(handlers=handlers, body=lambda: routing.refreshed(bpy.context, _today())) for handlers in (bpy.app.handlers.load_post, bpy.app.handlers.save_post)),
                *routing.watched(),
            )
        )

    def section(self, retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing) -> Section[Routing]:
        """Returns File Output section over context layer routing store."""
        return Section(key=self.key, title=Some("File Output"), rows=(self,), store=Some(Store(attachment=_ROUTING, retention=retention)))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GROUPS]


def _grouped(frames: tuple[tuple[FrameNode, tuple[Link, ...], LayerPlan], ...]) -> Outcome[NodeGroup]:
    """Builds routing group around image pass-through with each layer frame."""
    interface = Interface(outputs=(Socket(key="result", name="Image", kind=bpy.types.NodeSocketColor),), inputs=(Socket(key="image", kind=bpy.types.NodeSocketColor),))
    return built(RoutingRow.ownership.key, interface, {"result": group_input("image", bpy.types.NodeSocketColor)}, bpy.types.CompositorNodeTree).map(
        lambda group: msgspec.structs.replace(
            group,
            graph=msgspec.structs.replace(
                group.graph,
                nodes={**group.graph.nodes, **{plan.frame: frame for frame, _, plan in frames}},
                links=tuple(dict.fromkeys((*group.graph.links, *(link for _, links, _ in frames for link in links)))),
            ),
        )
    )


# --- [COMMANDS]


def _unlisted(identity: Identity, context: bpy.types.Context) -> Outcome[tuple[FileSocket, ...]]:
    """Returns context layer sources no file row holds."""

    def searched(routing: PassRouting, layer: bpy.types.ViewLayer, scene: bpy.types.Scene) -> Outcome[tuple[FileSocket, ...]]:
        taken = {row.name for row in routing.routing.struct(layer).files}
        return routing.planned(scene, routing.held(scene), routing.pathed.path(scene, _today())).map(
            lambda planned: tuple(
                source for plan in Option.of_optional(planned[1].get(layer)).to_list() for file_output in plan.file_outputs.values() for source in file_output.sources if source.name not in taken
            )
        )

    return paired(RoutingRow.bound(identity), rooted(bpy.types.ViewLayer, context), rooted(bpy.types.Scene, context)).bind(lambda found: searched(*found))


@Operator.of(Empty)
def passes_update(call: Call[Empty]) -> Outcome[Answer]:
    """Reconcile scene pass routing and File Output nodes."""
    return (
        paired(RoutingRow.bound(call.identity), rooted(bpy.types.Scene, call.context))
        .bind(lambda pair: pair[0].reconciled(pair[1], call.context, pair[0].pathed.path(pair[1], _today())))
        .map(lambda _: Done())
    )


def pass_file_add(call: Call[bpy.types.PropertyGroup]) -> Outcome[Answer]:
    """Add file choice for a layer pass."""
    name = call.properties.name

    def added(source: FileSocket) -> Outcome[Answer]:
        return (
            rooted(bpy.types.ViewLayer, call.context)
            .bind(lambda layer: _ROUTING.read(call.identity, layer))
            .bind(lambda routing: assigned(routing.files.add(), {("name",): name, ("output",): source.output.name}))
            .map(lambda _: Done())
        )

    return (
        _unlisted(call.identity, call.context)
        .bind(lambda found: Block.of_seq(found).filter(lambda source: source.name == name).try_head().to_result_with(lambda: Fault.of(Reason.ABSENT, repr(call.context.view_layer), member=name)))
        .bind(added)
        if name
        else Ok(Unchanged(reason="Every layer pass has a file row"))
    )


@functools.cache
def _adding(package: str) -> Operator[bpy.types.PropertyGroup]:
    """Builds the file row command of an extension once, its search listing the context layer's unlisted sources."""

    def items(_owner: Struct, context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
        found = Option.of_optional(context).map(lambda held: Identity.read(package).bind(lambda identity: _unlisted(identity, held)).default_value(())).default_value(())
        return choices((source.name, source.name, f"{source.output.name.title()} pass") for source in found)

    form: type[bpy.types.PropertyGroup] = type(
        "PassName",
        (bpy.types.PropertyGroup,),
        {"__doc__": "Source name a file row adds from sources no row holds.", "__annotations__": {"name": bpy.props.EnumProperty(name="Pass", description="Pass the file row names", items=items)}},
    )
    return Operator(form=form, invoke=Some(Popup(functools.partial(bpy.types.WindowManager.invoke_search_popup))), primary=Some("name"), run=pass_file_add)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LayerPlan", "PassPreset", "PassRouting", "RoutingRow", "pass_file_add", "passes_update"]
