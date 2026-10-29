"""View layer pass routing: the per-layer plans, the routing group reconciled into File Output nodes, the files it wrote, presets, and its commands."""

from collections.abc import Callable, Mapping
import datetime
import functools
import itertools
from types import MappingProxyType
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Map
import mathutils
import msgspec

from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Unchanged
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import written
from blender.bpy.data.rna.mirrors import held_choices
from blender.bpy.data.rna.paths import rooted, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Handler, runtime, Subscription
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.nodes.addressing import owned
from blender.bpy.nodes.changes import Changes, made, reconciled
from blender.bpy.nodes.desired import FrameNode, Interface, Link, NodeGroup, Socket
from blender.bpy.nodes.functions import built, lowered
from blender.bpy.nodes.graphs import Expr, group_input
from blender.bpy.render.outputs.formats import extension
from blender.bpy.render.outputs.paths import NodePaths, OutputPath, Outputs, Pathed, ScenePaths, today
from blender.bpy.render.outputs.results import Written
from blender.bpy.render.view_layers.layers import LAYER, LayerTag, Role
from blender.bpy.render.view_layers.passes import Derived, EMISSION, ENVIRONMENT, Family, Flags, layer_of, listed, OCCLUSION, PassFlags, PassRequest, required, synced, used
from blender.bpy.render.view_layers.sources import filed, FileSplit, Filing, filings, ids_recolor, layer_name, lobe, NUKE_CHANNELS, Recoloring, ROUTING, Routing, sources
from blender.bpy.settings.attachments import Attached
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import FUNCTIONS, Layout, Line, OperatorButton, realized, RowDraw
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Listing, Store
from blender.records.faults import accumulated, Faults, Outcome, paired
from blender.records.fields import Retention
from blender.records.trees import transfer

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
    """Frame key and filing per File Output key of one used view layer."""

    frame: Key
    filings: Mapping[Key, Filing]


class PassPreset(msgspec.Struct, frozen=True, kw_only=True):
    """Pass flags and routing saved together."""

    flags: Snapshot
    routing: Snapshot


class PassRouting(msgspec.Struct, frozen=True, kw_only=True):
    """`PassSet` bound to an identity, with the attachments its plans read."""

    identity: Identity
    routing: Attached[Routing]
    pathed: Pathed
    layers: Attached[LayerTag]
    lobe: NodeGroup

    def held(self, scene: bpy.types.Scene) -> Option[bpy.types.NodeTree]:
        """Returns routing tree identity holds for scene."""
        return PassSet.ownership.found(self.identity, of=Some(scene))

    def filed(self, scene: bpy.types.Scene, view: str, filing: Filing, key: Key) -> Outcome[Expr]:
        """Builds the File Output node of one filing of a view layer under its routing at today's scene path, for term filters filing what they read."""
        return layer_of(scene, view).bind(lambda layer: self.pathed.path(scene, today()).bind(lambda path: filed(self.pathed, path, view, filing, key, nuke=self.routing.struct(layer).nuke)))

    def framed(self, scene: bpy.types.Scene, held: dict[Key, bpy.types.Node], path: OutputPath, index: int, layer: bpy.types.ViewLayer) -> Outcome[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
        """Plans frame, Render Layers node, sources, and File Output nodes of one used layer."""
        prefix = f"layer_{index}"
        rendered = Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(scene), "layer": layer.name}, label=Some(f"{prefix}_render"))
        routing, record = self.routing.struct(layer), self.layers.struct(layer)

        def planned(plan: Mapping[Key, Filing]) -> Outcome[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
            return (
                accumulated(filed(self.pathed, path, layer.name, filing, key, nuke=routing.nuke) for key, filing in plan.items())
                .bind(lambda files: lowered(bpy.types.CompositorNodeTree, rendered, *files))
                .map(
                    lambda merged: (
                        FrameNode(members={"label": layer.name, "use_custom_color": True, "color": mathutils.Color(record.color).from_scene_linear_to_srgb()[:]}, nodes=merged.nodes),
                        merged.links,
                        LayerPlan(frame=prefix, filings=plan),
                    )
                )
            )

        return (
            Option
            .of_optional(held.get(f"{prefix}_render"))
            .map(lambda live: sources(scene, layer, rendered, live, routing, Role[record.role], self.lobe))
            .default_value(Ok(()))
            .bind(lambda found: filings(found, FileSplit[routing.split]))
            .bind(lambda filed_out: planned(MappingProxyType({f"{prefix}_{filing.output.name.lower()}_{'layered' if filing.format.layered else 'exr'}": filing for filing in filed_out})))
        )

    def planned(self, scene: bpy.types.Scene, tree: Option[bpy.types.NodeTree], path: OutputPath) -> Outcome[tuple[NodeGroup, dict[bpy.types.ViewLayer, LayerPlan]]]:
        """Plans scene routing group, reading offered sockets from held tree."""
        held = tree.map(lambda found: owned(found, self.identity)).default_value({})
        layers = used(scene)
        return accumulated(self.framed(scene, held, path, index, layer) for index, layer in enumerate(layers)).bind(
            lambda frames: grouped(frames).map(lambda group: (group, {layer: plan for layer, (_, _, plan) in zip(layers, frames)}))
        )

    def reconciled(self, scene: bpy.types.Scene, context: bpy.types.Context, path: OutputPath) -> Outcome[Changes]:
        """Reconciles passes, AOVs, light groups, and routing to scene settings, File Output nodes and items taking their formats, called after overrides apply or revert."""
        flags = PassFlags.of()

        def rebuilt(first: NodeGroup) -> Outcome[Changes]:
            return made(first, self.identity, of=Some(scene)).bind(lambda tree: self.planned(scene, Some(tree), path).bind(lambda planned: reconciled(planned[0], tree, self.identity)))

        return (
            required(
                scene,
                context,
                tuple(PassRequest(layer=layer.name, flags=flags.opened(routing.families, routing.layers, routing.kinds)) for layer in used(scene) for routing in (self.routing.struct(layer),)),
            )
            .bind(lambda _: synced(scene, self.identity))
            .bind(lambda _: self.planned(scene, self.held(scene), path))
            .bind(lambda first: rebuilt(first[0]))
        )

    def placed(self, scene: bpy.types.Scene, path: OutputPath) -> Outcome[OutputPath]:
        """Stores path as the scene's render path, then reconciles routing to it."""
        return self.pathed.placed(scene, path).bind(lambda _: self.reconciled(scene, bpy.context, path)).map(lambda _: path)

    def refreshed(self, context: bpy.types.Context, day: datetime.date) -> Outcome[None]:
        """Reconciles every editable scene with stored layer routing at `day`, owned runtime scenes skipped."""

        def refresh(scene: bpy.types.Scene) -> Outcome[Changes]:
            return self.pathed.path(scene, day).bind(lambda path: self.reconciled(scene, context, path))

        return rooted(bpy.types.BlendData, context).bind(
            lambda data: accumulated(
                refresh(scene) for scene in data.scenes if scene.is_editable and not runtime(self.identity, scene) and any(self.routing.stored(layer) for layer in scene.view_layers)
            ).map(lambda _: None)
        )

    def watched(self) -> tuple[Subscription, ...]:
        """Builds subscriptions refreshing routing, one per pass flag, watched setting, and runtime attachment member."""
        groups = tuple((form, prop) for form in (Routing, Outputs, LayerTag) for prop in form.bl_rna.properties if prop.is_runtime)
        return tuple(
            Subscription(owner=owner, setting=prop, body=lambda: self.refreshed(bpy.context, today()))
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
        common = frozenset({EMISSION, ENVIRONMENT})
        shadows = frozenset(flag for _, flag in flags.every if flag.prop.identifier in {"use_pass_transparent", "use_pass_shadow"})
        recipes = (
            ("lightray_rebuild", common | flags.lobes, (), (Derived.DIRECT_LIGHT, Derived.INDIRECT_LIGHT)),
            ("material_rebuild", common | flags.lobes, tuple(Family), ()),
            ("multipass_breakdown", common | flags.lobes, (), ()),
            ("eevee_material_rebuild", common | eevee, (Family.DIFFUSE, Family.GLOSSY, Family.VOLUME), ()),
            ("eevee_multipass", common | eevee | shadows | {OCCLUSION}, (), ()),
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

        return listed(msgspec.structs.replace(PassSet.presets, stock={name: stock(enabled, totals, derived) for name, enabled, totals, derived in recipes}), captured, same, applied)


class PassSet(Line, frozen=True):
    """Row of pass routing."""

    key: ClassVar[Key] = ROUTING.key
    prerequisites: ClassVar[tuple[Registrable, ...]] = (ROUTING, LAYER, ScenePaths.attachment)
    presets: ClassVar[Preset[PassPreset]] = Preset(key=key, form=PassPreset)
    ownership: ClassVar[Ownership[bpy.types.CompositorNodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key=key, saved=Saved.USED)
    title: ClassVar[str] = bpy.path.display_name(key)

    @staticmethod
    def bound(identity: Identity) -> Outcome[PassRouting]:
        """Binds attachments and the lobe group."""
        return paired(ROUTING.bound(identity), ScenePaths().bound(identity), LAYER.bound(identity), lobe()).map(
            lambda parts: PassRouting(identity=identity, routing=parts[0], pathed=parts[1], layers=parts[2], lobe=parts[3])
        )

    def written(self, scene: bpy.types.Scene, frame: int, identity: Identity, *, path: Option[OutputPath] = Nothing) -> Outcome[tuple[Written, ...]]:
        """Returns files scene File Output nodes wrote for a frame at `path` or scene path, the finishing files callable."""

        def files(routing: PassRouting, path: OutputPath, layer: bpy.types.ViewLayer, live: bpy.types.CompositorNodeOutputFile, filing: Filing) -> Outcome[tuple[Written, ...]]:
            nuke = routing.routing.struct(layer).nuke
            names = tuple(layer_name(source.name, nuke=nuke) for source in filing.sources)

            def targeted(paths: NodePaths) -> tuple[tuple[bpy.types.ImageFormatSettings, Option[str], Map[str, str]], ...]:
                if filing.format.layered:
                    return tuple((settings, Nothing, Map.of_seq(zip(paths.items, (source.name for source in filing.sources)))) for settings in Option.of_optional(live.format).to_list())
                stored = {item.name: item for item in live.file_output_items}
                return tuple(
                    (settings, Some(name), Map.of_seq((("", source.name),)))
                    for name, source, kept in zip(names, filing.sources, paths.items)
                    for item in Option.of_optional(stored.get(kept)).to_list()
                    for settings in Option.of_optional(item.format if item.override_node_format else live.format).to_list()
                )

            def filed_at(settings: bpy.types.ImageFormatSettings, item: Option[str], passes: Map[str, str], suffix: str) -> Outcome[Option[Written]]:
                return path.written(layer.name, filing.output, item, frame, suffix).map(
                    lambda file: Written.of(str(file), settings, filing.output, NUKE_CHANNELS if nuke else MappingProxyType({}), layer.name, passes)
                )

            return (
                routing.pathed
                .node(path, layer.name, filing.output, names, layered=filing.format.layered)
                .bind(lambda paths: accumulated(filed_at(settings, item, passes, suffix) for settings, item, passes in targeted(paths) for suffix in extension(settings).to_list()))
                .map(lambda found: tuple(file for option in found for file in option.to_list()))
            )

        def collected(routing: PassRouting, path: OutputPath) -> Outcome[tuple[Written, ...]]:
            tree = routing.held(scene)
            held = tree.map(lambda found: owned(found, identity)).default_value({})
            return (
                routing
                .planned(scene, tree, path)
                .bind(
                    lambda planned: accumulated(
                        files(routing, path, layer, live, filing)
                        for layer, plan in planned[1].items()
                        for key, filing in plan.filings.items()
                        for live in (held.get(key),)
                        if isinstance(live, bpy.types.CompositorNodeOutputFile)
                    )
                )
                .map(lambda parts: tuple(itertools.chain.from_iterable(parts)))
            )

        return self.bound(identity).bind(lambda routing: path.map(Ok).default_with(lambda: routing.pathed.path(scene, today())).bind(lambda chosen: collected(routing, chosen)))

    @staticmethod
    def commands(identity: Identity) -> tuple[Operator[Empty] | Operator[Recoloring] | Operator[bpy.types.PropertyGroup], ...]:
        """Returns the routing commands, the file row search listing the context layer's unfiled sources."""
        return (passes_update, ids_recolor, _adding(identity.package))

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Draws the routing commands over the preset rows."""
        buttons = (
            OperatorButton(target=passes_update),
            OperatorButton(target=ids_recolor, function=FUNCTIONS["operator_enum"], keywords={"property": "mode"}),
            OperatorButton(target=_adding(identity.package)),
        )
        return self.bound(identity).bind(
            lambda routing: realized((Layout(rows=buttons, container=Some(lambda layout: layout.grid_flow(row_major=True, even_columns=True))), routing.listing()), identity)
        )

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns routing acquisitions."""
        return self.bound(identity).map(
            lambda routing: (
                self.ownership,
                routing.lobe,
                *self.commands(identity),
                routing.listing(),
                *(Handler(handlers=handlers, body=lambda: routing.refreshed(bpy.context, today())) for handlers in (bpy.app.handlers.load_post, bpy.app.handlers.save_post)),
                *routing.watched(),
            )
        )

    def section(self, retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing) -> Section[Routing]:
        """Returns File Output section over context layer routing store."""
        return Section(key=self.key, title="File Output", rows=(self,), store=Some(Store(attachment=ROUTING, retention=retention)))


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [GROUP]


def grouped(frames: tuple[tuple[FrameNode, tuple[Link, ...], LayerPlan], ...]) -> Outcome[NodeGroup]:
    """Builds routing group around image pass-through with each layer frame."""
    interface = Interface(outputs=(Socket(key="result", name="Image", kind=bpy.types.NodeSocketColor),), inputs=(Socket(key="image", kind=bpy.types.NodeSocketColor),))
    return built(PassSet.ownership.key, interface, {"result": group_input("image", bpy.types.NodeSocketColor)}, bpy.types.CompositorNodeTree).map(
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


def unfiled(identity: Identity, context: bpy.types.Context) -> Outcome[tuple[tuple[str, str, str], ...]]:
    """Returns file row search items for context layer sources no file row holds."""

    def searched(routing: PassRouting, layer: bpy.types.ViewLayer, scene: bpy.types.Scene) -> Outcome[tuple[tuple[str, str, str], ...]]:
        taken = {row.name for row in routing.routing.struct(layer).files}
        return routing.pathed.path(scene, today()).bind(
            lambda path: routing.planned(scene, routing.held(scene), path).map(
                lambda planned: tuple(
                    (source.name, source.name, f"{layer.name} pass {source.name}")
                    for plan in Option.of_optional(planned[1].get(layer)).to_list()
                    for filing in plan.filings.values()
                    for source in filing.sources
                    if source.name not in taken
                )
            )
        )

    return paired(PassSet.bound(identity), rooted(bpy.types.ViewLayer, context), rooted(bpy.types.Scene, context)).bind(lambda found: searched(*found))


@Operator.of(Empty)
def passes_update(call: Call[Empty]) -> Outcome[Answer]:
    """Reconcile scene pass routing and File Output nodes."""
    return (
        paired(PassSet.bound(call.identity), rooted(bpy.types.Scene, call.context))
        .bind(lambda pair: pair[0].pathed.path(pair[1], today()).bind(lambda path: pair[0].reconciled(pair[1], call.context, path)))
        .map(lambda _: Done())
    )


def pass_file_add(call: Call[bpy.types.PropertyGroup]) -> Outcome[Answer]:
    """Add file choice for a layer pass."""
    name = call.properties.name
    return (
        Result[bpy.types.ViewLayer, Faults]
        .bind(rooted(bpy.types.ViewLayer, call.context), lambda layer: ROUTING.read(call.identity, layer).bind(lambda routing: written(routing.files.add(), ("name",), name)))
        .map(lambda _: Done())
        if name
        else Ok(Unchanged(reason="Every layer pass has a file row"))
    )


@functools.cache
def _adding(package: str) -> Operator[bpy.types.PropertyGroup]:
    """Builds the file row command of an extension once, its search listing the context layer's unfiled sources."""

    def items(_owner: Struct, context: bpy.types.Context | None) -> tuple[tuple[str, str, str, str, int], ...]:
        rows = Option.of_optional(context).map(lambda held: Identity.read(package).bind(lambda identity: unfiled(identity, held)).default_value(())).default_value(())
        return held_choices(rows)

    form: type[bpy.types.PropertyGroup] = type(
        "PassName",
        (bpy.types.PropertyGroup,),
        {"__doc__": "Source name a file row adds from sources no row holds.", "__annotations__": {"name": bpy.props.EnumProperty(name="Pass", description="Pass the file row names", items=items)}},
    )
    return Operator(form=form, invoke=Some(Popup(functools.partial(bpy.types.WindowManager.invoke_search_popup))), primary=Some("name"), run=pass_file_add)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["LayerPlan", "PassPreset", "PassRouting", "PassSet", "grouped", "pass_file_add", "passes_update", "unfiled"]
