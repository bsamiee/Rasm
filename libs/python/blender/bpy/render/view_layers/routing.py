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
import msgspec

from blender.bpy.commands.invocations import Popup
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, tallied, Unchanged
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Handler, runtime, Subscription
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.addressing import owned
from blender.bpy.nodes.changes import Changes, made, reconciled
from blender.bpy.nodes.desired import FrameNode, Link, NodeGroup
from blender.bpy.nodes.functions import built, lowered
from blender.bpy.nodes.graphs import Expr, group_input
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.render.outputs.finishing import Written
from blender.bpy.render.outputs.formats import extension
from blender.bpy.render.outputs.paths import NodePaths, OutputPath, Outputs, Pathed, ScenePaths, today
from blender.bpy.render.view_layers.ids import ids_shuffle, ids_spread
from blender.bpy.render.view_layers.overrides import encoded, LayerOverride, LayerTag, Role
from blender.bpy.render.view_layers.passes import Derived, EMISSION, ENVIRONMENT, Family, OCCLUSION, PassFlags, PassRequest, required, synced, used
from blender.bpy.render.view_layers.sources import filed, FileSocket, FileSplit, Filing, filings, layer_name, lobe, NUKE_CHANNELS, PassName, ROUTING, Routing, sources
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.presets import Listing
from blender.bpy.ui.layout.rows import Layout, Line, OperatorButton, realized, RowDraw
from blender.bpy.ui.layout.sections import Section
from blender.bpy.ui.layout.values import Store
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.records.fields import Retention
from blender.records.trees import Owner, transfer, Tree

# --- [CONSTANTS] ------------------------------------------------------------------------

_TRANSPARENT: Final = Descriptor.declared(bpy.types.ViewLayer, ("eevee", "use_pass_transparent"), bpy.types.BoolProperty)
_SHADOW: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_shadow",), bpy.types.BoolProperty)
_WATCHED: Final = tuple(
    (owner, Descriptor.declared(owner, (member,), bpy.types.Property).prop)
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
    """Frame key, filing per File Output key, and whether the files take the names and axes Nuke reads, of one used view layer."""

    frame: Key
    filings: Mapping[Key, Filing]
    nuke: bool


class PassPreset(msgspec.Struct, frozen=True, kw_only=True):
    """Pass flags and routing saved together."""

    flags: Snapshot
    routing: Snapshot


class PassRouting(msgspec.Struct, frozen=True, kw_only=True):
    """`PassSet` bound to an identity, with the members its plans write."""

    precision: ClassVar[Descriptor[bpy.types.EnumProperty]] = Descriptor.declared(bpy.types.Scene, ("render", "compositor_precision"), bpy.types.EnumProperty)
    label: ClassVar[Descriptor[bpy.types.StringProperty]] = Descriptor.declared(bpy.types.NodeFrame, ("label",), bpy.types.StringProperty)
    custom: ClassVar[Descriptor[bpy.types.BoolProperty]] = Descriptor.declared(bpy.types.NodeFrame, ("use_custom_color",), bpy.types.BoolProperty)
    tint: ClassVar[Descriptor[bpy.types.FloatProperty]] = Descriptor.declared(bpy.types.NodeFrame, ("color",), bpy.types.FloatProperty)
    identity: Identity
    routing: Attached[Routing]
    pathed: Pathed
    layers: Attached[LayerTag]
    flags: PassFlags
    lobe: NodeGroup

    def held(self, scene: bpy.types.Scene) -> Option[bpy.types.NodeTree]:
        """Returns routing tree identity holds for scene."""
        return PassSet.ownership.found(self.identity, of=Some(scene))

    def filed(self, scene: bpy.types.Scene, view: str, filing: Filing, key: Key) -> Outcome[Expr]:
        """Builds the File Output node of one filing of a view layer under its routing at today's scene path, for term filters filing what they read."""
        return (
            Option
            .of_optional(scene.view_layers.get(view))
            .to_result_with(functools.partial(Fault.of, BlenderReason.ABSENT, repr(scene.view_layers), member=view))
            .bind(lambda layer: self.pathed.path(scene, today()).bind(lambda path: filed(self.pathed, path, view, filing, key, nuke=self.routing.struct(layer).nuke)))
        )

    def framed(self, scene: bpy.types.Scene, held: dict[Key, bpy.types.Node], path: OutputPath, index: int, layer: bpy.types.ViewLayer) -> Outcome[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
        """Plans frame, Render Layers node, sources, and File Output nodes of one used layer."""
        prefix = f"layer_{index}"
        rendered = Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(scene), "layer": layer.name}, label=Some(f"{prefix}_render"))

        def filed_all(routing: Routing, record: LayerTag, found: tuple[FileSocket, ...]) -> Outcome[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
            def planned(plan: Mapping[Key, Filing]) -> Outcome[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
                return (
                    accumulated(filed(self.pathed, path, layer.name, filing, key, nuke=routing.nuke) for key, filing in plan.items())
                    .bind(lambda files: lowered(bpy.types.CompositorNodeTree, rendered, *files))
                    .map(
                        lambda merged: (
                            FrameNode(members={self.label.path: layer.name, self.custom.path: True, self.tint.path: encoded(record.color)}, nodes=merged.nodes),
                            merged.links,
                            LayerPlan(frame=prefix, filings=plan, nuke=routing.nuke),
                        )
                    )
                )

            return filings(found, FileSplit[routing.split]).bind(
                lambda filed_out: planned(MappingProxyType({f"{prefix}_{filing.output.name.lower()}_{'layered' if filing.format.layered else 'exr'}": filing for filing in filed_out}))
            )

        def routed(routing: Routing, record: LayerTag) -> Outcome[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
            return (
                Option
                .of_optional(held.get(f"{prefix}_render"))
                .map(lambda live: sources(scene, layer, rendered, live, routing, Role[record.role], self.flags, self.lobe))
                .default_value(Ok(()))
                .bind(lambda found: filed_all(routing, record, found))
            )

        return routed(self.routing.struct(layer), self.layers.struct(layer))

    def planned(self, scene: bpy.types.Scene, tree: Option[bpy.types.NodeTree], path: OutputPath) -> Outcome[tuple[NodeGroup, dict[str, LayerPlan]]]:
        """Plans scene routing group, reading offered sockets from held tree."""
        held = tree.map(lambda found: owned(found, self.identity)).default_value({})
        layers = used(scene)
        return accumulated(self.framed(scene, held, path, index, layer) for index, layer in enumerate(layers)).bind(
            lambda frames: grouped(frames).map(lambda group: (group, {layer.name: plan for layer, (_, _, plan) in zip(layers, frames)}))
        )

    def reconciled(self, scene: bpy.types.Scene, context: bpy.types.Context, path: OutputPath) -> Outcome[Changes]:
        """Reconciles passes, AOVs, light groups, and routing to scene settings, File Output nodes and items taking their formats, called after overrides apply or revert."""

        def rebuilt(first: NodeGroup, _: dict[str, LayerPlan]) -> Outcome[Changes]:
            return made(first, self.identity, of=Some(scene)).bind(lambda tree: self.planned(scene, Some(tree), path).bind(lambda planned: reconciled(planned[0], tree, self.identity)))

        def requested(layer: bpy.types.ViewLayer) -> Outcome[PassRequest]:
            routing = self.routing.struct(layer)
            return Ok(PassRequest(layer=layer.name, flags=self.flags.opened(routing.families, routing.layers, routing.kinds)))

        return (
            accumulated(map(requested, used(scene)))
            .bind(lambda requests: required(scene, context, requests, self.precision))
            .bind(lambda _: synced(scene, self.identity))
            .bind(lambda _: self.planned(scene, self.held(scene), path))
            .bind(lambda first: rebuilt(*first))
        )

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
            for owner, prop in (*((owner, flag.prop) for owner, flag in self.flags.every), *_WATCHED, *groups)
        )

    def listing(self) -> Listing[PassPreset]:
        """Returns pass presets over layer flags and routing, applied to every used layer."""
        selection = Some(tuple(flag for _, flag in self.flags.every))
        eevee = frozenset().union(
            *(direct | color for family in (Family.DIFFUSE, Family.GLOSSY) for direct, _, color in (self.flags.families[family],)),
            self.flags.families[Family.VOLUME][0] & frozenset(flag for owner, flag in self.flags.every if owner is bpy.types.ViewLayerEEVEE),
        )
        common = frozenset({EMISSION, ENVIRONMENT})
        recipes = (
            ("lightray_rebuild", common | self.flags.lobes, (), (Derived.DIRECT, Derived.INDIRECT)),
            ("material_rebuild", common | self.flags.lobes, tuple(Family), ()),
            ("multipass_breakdown", common | self.flags.lobes, (), ()),
            ("eevee_material_rebuild", common | eevee, (Family.DIFFUSE, Family.GLOSSY, Family.VOLUME), ()),
            ("eevee_multipass", common | eevee | {_TRANSPARENT, _SHADOW, OCCLUSION}, (), ()),
        )
        owners = tuple(dict.fromkeys(flag.owner for _, flag in self.flags.every))

        def stock(enabled: frozenset[Descriptor[bpy.types.BoolProperty]], totals: tuple[Family, ...], derived: tuple[Derived, ...]) -> PassPreset:
            return PassPreset(
                flags=Snapshot(
                    struct=bpy.types.ViewLayer.__name__,
                    tree=Tree(
                        roots=owners, rows=tuple((owner, Owner(rows=tuple(((flag.prop.identifier,), flag in enabled) for _, flag in self.flags.every if flag.owner == owner))) for owner in owners)
                    ),
                ),
                routing=Snapshot.of(Routing, {("totals",): tuple(sorted(str(member.name) for member in totals)), ("derived",): tuple(sorted(str(member.name) for member in derived))}),
            )

        def held(layer: bpy.types.ViewLayer) -> Outcome[PassPreset]:
            return paired(Snapshot.capture(layer, selection), Snapshot.capture(self.routing.struct(layer))).map(lambda pair: PassPreset(flags=pair[0], routing=pair[1]))

        def same(current: PassPreset, value: PassPreset) -> bool:
            return not transfer(current.flags.tree, value.flags.tree) and not transfer(current.routing.tree, value.routing.tree)

        def captured(context: bpy.types.Context) -> Outcome[PassPreset]:
            return rooted(bpy.types.ViewLayer, context).bind(held)

        def matches(context: bpy.types.Context, values: tuple[PassPreset, ...]) -> Outcome[tuple[bool, ...]]:
            return captured(context).map(lambda current: tuple(same(current, value) for value in values))

        def applied(context: bpy.types.Context, value: PassPreset) -> Outcome[Answer]:
            def wrote(scene: bpy.types.Scene) -> Outcome[Answer]:
                layers = used(scene)

                def differed(current: tuple[PassPreset, ...]) -> Outcome[Answer]:
                    if not (differing := tuple(layer for layer, preset in zip(layers, current) if not same(preset, value))):
                        return Ok(Unchanged(reason=f"Used view layers of {scene.name} match preset"))
                    return tallied(tuple(paired(value.flags.apply(layer).outcome, value.routing.apply(self.routing.struct(layer)).outcome) for layer in differing))

                return accumulated(map(held, layers)).bind(differed)

            return rooted(bpy.types.Scene, context).bind(wrote)

        return Listing(
            preset=msgspec.structs.replace(PassSet.presets, stock={name: stock(enabled, totals, derived) for name, enabled, totals, derived in recipes}),
            current=captured,
            matches=matches,
            applied=applied,
        )


class PassSet(Line, frozen=True):
    """Row of pass routing."""

    key: ClassVar[Key] = ROUTING.key
    attachment: ClassVar[Attachment[Routing]] = ROUTING
    prerequisites: ClassVar[tuple[Registrable, ...]] = (attachment,)
    presets: ClassVar[Preset[PassPreset]] = Preset(key=key, form=PassPreset)
    ownership: ClassVar[Ownership[bpy.types.CompositorNodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key=key, saved=Saved.USED)
    title: ClassVar[str] = bpy.path.display_name(key)

    def bound(self, identity: Identity) -> Outcome[PassRouting]:
        """Binds attachments and resolves flags and lobe group."""
        return paired(self.attachment.bound(identity), ScenePaths().bound(identity), LayerOverride.layer.bound(identity), PassFlags.of(), lobe()).map(
            lambda parts: PassRouting(identity=identity, routing=parts[0], pathed=parts[1], layers=parts[2], flags=parts[3], lobe=parts[4])
        )

    def written(self, scene: bpy.types.Scene, frame: int, identity: Identity, *, path: Option[OutputPath] = Nothing) -> Outcome[tuple[Written, ...]]:
        """Returns files scene File Output nodes wrote for a frame at `path` or scene path, the `Files` port."""

        def files(routing: PassRouting, path: OutputPath, layer: str, plan: LayerPlan, live: bpy.types.CompositorNodeOutputFile, filing: Filing) -> Outcome[tuple[Written, ...]]:
            names = tuple(layer_name(source.name, nuke=plan.nuke) for source in filing.sources)

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
                return path.written(layer, filing.output, item, frame, suffix).map(
                    lambda file: Written.of(str(file), settings, filing.output, NUKE_CHANNELS if plan.nuke else MappingProxyType({}), layer, passes)
                )

            return (
                routing.pathed
                .node(path, layer, filing.output, names, layered=filing.format.layered)
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
                        files(routing, path, layer, plan, live, filing)
                        for layer, plan in planned[1].items()
                        for key, filing in plan.filings.items()
                        for live in (held.get(key),)
                        if isinstance(live, bpy.types.CompositorNodeOutputFile)
                    )
                )
                .map(lambda parts: tuple(itertools.chain.from_iterable(parts)))
            )

        return self.bound(identity).bind(lambda routing: path.map(Ok).default_with(lambda: routing.pathed.path(scene, today())).bind(lambda chosen: collected(routing, chosen)))

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Draws the routing commands over the preset rows."""
        return self.bound(identity).bind(
            lambda routing: realized(
                (Layout(rows=tuple(OperatorButton(target=command) for command in COMMANDS), container=Some(lambda layout: layout.grid_flow(row_major=True, even_columns=True))), routing.listing()),
                identity,
            )
        )

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns routing acquisitions."""
        return self.bound(identity).map(
            lambda routing: (
                self.ownership,
                routing.lobe,
                *COMMANDS,
                routing.listing(),
                *(Handler(handlers=handlers, body=lambda: routing.refreshed(bpy.context, today())) for handlers in (bpy.app.handlers.load_post, bpy.app.handlers.save_post)),
                *routing.watched(),
            )
        )

    def section(self, retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing) -> Section[Routing]:
        """Returns File Output section over context layer routing store."""
        return Section(key=self.key, title="File Output", rows=(self,), store=Some(Store(attachment=self.attachment, retention=retention)))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [GROUP]


def grouped(frames: tuple[tuple[FrameNode, tuple[Link, ...], LayerPlan], ...]) -> Outcome[NodeGroup]:
    """Builds routing group around image pass-through with each layer frame."""
    interface = Interface(outputs=(Socket(key="result", name="Image", kind=bpy.types.NodeSocketColor),), inputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),))
    return built(PassSet.ownership.key, PassSet.title, interface, {"result": group_input("image", bpy.types.NodeSocketColor)}, bpy.types.CompositorNodeTree).map(
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


def passes_update(call: Call[Empty]) -> Outcome[Done]:
    """Reconcile scene pass routing and File Output nodes."""
    return (
        paired(PassSet().bound(call.identity), rooted(bpy.types.Scene, call.context))
        .bind(lambda pair: pair[0].pathed.path(pair[1], today()).bind(lambda path: pair[0].reconciled(pair[1], call.context, path)))
        .map(lambda _: Done())
    )


def pass_file_add(call: Call[PassName]) -> Outcome[Answer]:
    """Add file choice for a layer pass."""
    name = call.properties.name
    return (
        Result[bpy.types.ViewLayer, Faults]
        .bind(rooted(bpy.types.ViewLayer, call.context), lambda layer: ROUTING.read(call.identity, layer).bind(lambda routing: written(routing.files.add(), ("name",), name)))
        .map(lambda _: Done())
        if name
        else Ok(Unchanged(reason="Pass file row needs a pass name"))
    )


def unfiled(identity: Identity, context: bpy.types.Context) -> Outcome[tuple[tuple[str, str, str], ...]]:
    """Returns file row search items for context layer sources no file row holds."""

    def listed(layer: bpy.types.ViewLayer, record: Routing, _: NodeGroup, plans: dict[str, LayerPlan]) -> tuple[tuple[str, str, str], ...]:
        taken = {row.name for row in record.files}
        return tuple(
            (source.name, source.name, f"{layer.name} pass {source.name}")
            for plan in Option.of_optional(plans.get(layer.name)).to_list()
            for filing in plan.filings.values()
            for source in filing.sources
            if source.name not in taken
        )

    def searched(routing: PassRouting, layer: bpy.types.ViewLayer, scene: bpy.types.Scene) -> Outcome[tuple[tuple[str, str, str], ...]]:
        return routing.pathed.path(scene, today()).bind(lambda path: routing.planned(scene, routing.held(scene), path).map(lambda planned: listed(layer, routing.routing.struct(layer), *planned)))

    return paired(PassSet().bound(identity), rooted(bpy.types.ViewLayer, context), rooted(bpy.types.Scene, context)).bind(lambda found: searched(*found))


# --- [COMPOSITION] ----------------------------------------------------------------------

COMMANDS: Final[tuple[Operator[Empty] | Operator[PassName], ...]] = (
    Operator(form=Empty, run=passes_update),
    Operator(form=Empty, run=ids_spread),
    Operator(form=Empty, run=ids_shuffle),
    Operator(form=PassName, invoke=Some(Popup(functools.partial(bpy.types.WindowManager.invoke_props_dialog))), run=pass_file_add),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COMMANDS", "LayerPlan", "PassPreset", "PassRouting", "PassSet", "grouped", "pass_file_add", "passes_update", "unfiled"]
