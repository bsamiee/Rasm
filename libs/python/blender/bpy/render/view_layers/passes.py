"""View layer pass routing into File Output nodes with derived layers and ID colors."""

from collections import deque
from collections.abc import Callable
import datetime
import functools
import itertools
from typing import Annotated, ClassVar, Literal, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Map
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import Call, Done, Empty, InvokePopup, Operator, Outcome, tallied, Unchanged
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.conditions import AllOf, AnyOf, Condition, holds, Not, Setting
from blender.bpy.data.rna.descriptors import Descriptor, IdName, rooted, struct_at, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import PathHandler
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderCollisionFault, BlenderFaults, BlenderImagingFault, BlenderResult, BlenderUnmetFault
from blender.bpy.nodes.changes import Added, Changes, changes, made, owned, reconciled
from blender.bpy.nodes.graphs import DataType, End, ended, FileItem, FileItems, Fragment, FrameNode, INPUTS, Link, math, mixed, nested, Node, node, NodeGroup, summed
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.nodes.trees import socket_at
from blender.bpy.render.outputs.finishing import Finish, stamped, Written
from blender.bpy.render.outputs.formats import extension, Kind, OutputFormat
from blender.bpy.render.outputs.paths import NodePaths, OutputPath, Pathed, ScenePaths
from blender.bpy.render.view_layers.overrides import colors, encoded, Layer, LayerOverride, Role, Spectrum
from blender.bpy.scene.lights.groups import used
from blender.bpy.scene.materials import DataPass, Materials, Utility
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.presets import Preset
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import Caption, Flow, joined, Label, Line, Member, OperatorButton, placed, Property, Row, RowDraw
from blender.bpy.ui.layout.sections import Hosted, Listing, Single, Store
from blender.imaging.color.processors import unassociated
from blender.imaging.faults import accumulated, paired
from blender.imaging.files import multilayer
from blender.imaging.files.cryptomatte import Matte
from blender.imaging.files.documents import Document, Group, Raster
from blender.imaging.files.images import Deflate, Depth, Storage
from blender.imaging.files.multilayer import Multilayer
from blender.imaging.hashing import content_hash, rng
from blender.imaging.parameters import Choice, Gated, Rgb
from blender.rendering.edits import Owner, transfer, Tree
from blender.rendering.history import Retention
from blender.rendering.outputs.conventions import Axis, Blender, Conventions, Space
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type ItemKind = Literal["FLOAT", "RGBA"]


class Family(Choice):
    """Light pass family, member name matching `Terms` field of its summed term."""

    prefix: str
    direct: tuple[str, ...]
    indirect: tuple[str, ...]
    color: tuple[str, ...]

    def __init__(self, value: int, label: str, prefix: str, direct: tuple[str, ...], indirect: tuple[str, ...], color: tuple[str, ...]) -> None:
        """Sets member label, socket prefix, and flag identifiers."""
        super().__init__(value, label)
        self.prefix, self.direct, self.indirect, self.color = prefix, direct, indirect, color

    def socket(self, slot: Key) -> str:
        """Returns Render Layers socket name of a family pass slot."""
        return f"{self.prefix} {slot.title()}"

    DIFFUSE = 1, "Diffuse Total", "Diffuse", ("use_pass_diffuse_direct",), ("use_pass_diffuse_indirect",), ("use_pass_diffuse_color",)
    GLOSSY = 2, "Glossy Total", "Glossy", ("use_pass_glossy_direct",), ("use_pass_glossy_indirect",), ("use_pass_glossy_color",)
    TRANSMISSION = 3, "Transmission Total", "Transmission", ("use_pass_transmission_direct",), ("use_pass_transmission_indirect",), ("use_pass_transmission_color",)
    VOLUME = 4, "Volume Total", "Volume", ("cycles.use_pass_volume_direct", "eevee.use_pass_volume_direct"), ("cycles.use_pass_volume_indirect",), ()


class Derived(Choice):
    """Layer built from render passes with item kind and output class."""

    kind: ItemKind
    output: Output

    def __init__(self, value: int, label: str, kind: ItemKind, output: Output) -> None:
        """Sets member label, item kind, and output class."""
        super().__init__(value, label)
        self.kind, self.output = kind, output

    DIRECT = 1, "Direct Light", "RGBA", Output.BEAUTY
    INDIRECT = 2, "Indirect Light", "RGBA", Output.BEAUTY
    DEPTH = 3, "Depth Map", "FLOAT", Output.DATA
    INVERSE = 4, "Inverse Depth", "FLOAT", Output.DATA


class Entity(Choice):
    """Cryptomatte kind with stored number and pass name."""

    OBJECT = 1, "Object"
    MATERIAL = 2, "Material"
    ASSET = 3, "Asset"

    @property
    def pass_name(self) -> str:
        """Blender pass name of kind."""
        return f"Crypto{self.name.title()}"

    @property
    def flag(self) -> str:
        """Pass flag identifier enabling kind ranks."""
        return f"use_pass_cryptomatte_{self.name.lower()}"

    @classmethod
    def named(cls, name: str) -> Option[Self]:
        """Returns kind with pass name `name`, `Nothing` for another."""
        return Option.of_optional(next((member for member in cls if member.pass_name == name), None))

    def member(self) -> BlenderResult[Descriptor[bpy.types.FloatProperty]]:
        """Resolves color member of kind blocks, `Object.color` or `Material.diffuse_color`."""
        match self:
            case Entity.OBJECT | Entity.ASSET:
                return Descriptor.resolve(bpy.types.Object, "color", bpy.types.FloatProperty)
            case Entity.MATERIAL:
                return Descriptor.resolve(bpy.types.Material, "diffuse_color", bpy.types.FloatProperty)


class FileSplit(Choice):
    """Split of layer passes into files."""

    ONE = 1, "One File"
    CLASSES = 2, "File per Class"
    PASSES = 3, "File per Pass"


# --- [CHOICES] --------------------------------------------------------------------------

offered = Choices(
    lambda identity, _owner, context: (
        Option.of_optional(context).map(lambda held: unfiled(identity, held).default_with(lambda faults: tuple(("", text, "") for _, text in rendered(faults)))).default_value(())
    )
)


# --- [MODELS] ---------------------------------------------------------------------------


class FamilyFlags(msgspec.Struct, frozen=True, kw_only=True):
    """Resolved direct, indirect, and color flags of one family."""

    direct: frozenset[Descriptor[bpy.types.BoolProperty]]
    indirect: frozenset[Descriptor[bpy.types.BoolProperty]]
    color: frozenset[Descriptor[bpy.types.BoolProperty]]


class PassFlags(msgspec.Struct, frozen=True, kw_only=True):
    """Pass flags of running build, resolved once."""

    every: "tuple[tuple[type[bpy.types.bpy_struct[object]], Descriptor[bpy.types.BoolProperty]], ...]"
    families: dict[Family, FamilyFlags]
    entities: dict[Entity, Descriptor[bpy.types.BoolProperty]]
    depth: Descriptor[bpy.types.BoolProperty]

    @classmethod
    def of(cls) -> BlenderResult["PassFlags"]:
        """Resolves every pass flag, faulting each identifier the build lacks."""

        def resolved(paths: tuple[str, ...]) -> BlenderResult[frozenset[Descriptor[bpy.types.BoolProperty]]]:
            return accumulated(Descriptor.resolve(bpy.types.ViewLayer, path, bpy.types.BoolProperty) for path in paths).map(frozenset)

        def grouped(family: Family, slots: tuple[frozenset[Descriptor[bpy.types.BoolProperty]], ...]) -> tuple[Family, FamilyFlags]:
            direct, indirect, color = slots
            return family, FamilyFlags(direct=direct, indirect=indirect, color=color)

        def family_flags(family: Family) -> BlenderResult[tuple[Family, FamilyFlags]]:
            return accumulated((resolved(family.direct), resolved(family.indirect), resolved(family.color))).map(lambda slots: grouped(family, slots))

        def entity_flag(entity: Entity) -> BlenderResult[tuple[Entity, Descriptor[bpy.types.BoolProperty]]]:
            return Descriptor.resolve(bpy.types.ViewLayer, entity.flag, bpy.types.BoolProperty).map(lambda flag: (entity, flag))

        def built(keyed: tuple[tuple[tuple[Family, FamilyFlags], ...], tuple[tuple[Entity, Descriptor[bpy.types.BoolProperty]], ...]], depth: Descriptor[bpy.types.BoolProperty]) -> PassFlags:
            families, entities = keyed
            return cls(every=pass_flags(), families=dict(families), entities=dict(entities), depth=depth)

        depth = Descriptor.resolve(bpy.types.ViewLayer, "use_pass_z", bpy.types.BoolProperty)
        return paired(paired(accumulated(map(family_flags, Family)), accumulated(map(entity_flag, Entity))), depth).map(lambda held: built(*held))

    def opened(self, routing: "Routing") -> frozenset[Descriptor[bpy.types.BoolProperty]]:
        """Returns flags a routing reads."""

        def derived(item: Derived) -> frozenset[Descriptor[bpy.types.BoolProperty]]:
            match item:
                case Derived.DIRECT:
                    return frozenset().union(*(flags.direct | flags.color for flags in self.families.values()))
                case Derived.INDIRECT:
                    return frozenset().union(*(flags.indirect | flags.color for flags in self.families.values()))
                case Derived.DEPTH | Derived.INVERSE:
                    return frozenset()

        return frozenset().union(
            *(self.families[family].direct | self.families[family].indirect | self.families[family].color for family in routing.totals),
            *map(derived, routing.derived),
            *(frozenset({self.entities[entity]}) for entity in routing.ids),
        )


class PassRequest(msgspec.Struct, frozen=True, kw_only=True):
    """Passes, condition, and precision a graph needs of one view layer."""

    layer: str
    flags: frozenset[Descriptor[bpy.types.BoolProperty]] = frozenset()
    condition: Option[Condition] = Nothing
    precise: bool = False

    @classmethod
    def depth(cls, layer: str, *, precise: bool) -> BlenderResult[Self]:
        """Returns Depth pass request of a layer."""
        return Descriptor.resolve(bpy.types.ViewLayer, "use_pass_z", bpy.types.BoolProperty).map(lambda flag: cls(layer=layer, flags=frozenset({flag}), precise=precise))


class PassFile(msgspec.Struct, frozen=True, kw_only=True):
    """File choice of one source."""

    name: Annotated[str, msgspec.Meta(min_length=1, title="Pass", description="Pass or derived layer name")]
    output: Annotated[Gated[Output], msgspec.Meta(title="File", description="Output class file holding pass, skipped when off")] = Gated(enabled=True, value=Output.BEAUTY)


class Routing(msgspec.Struct, frozen=True, kw_only=True):
    """Routing of view layer passes into files."""

    split: Annotated[FileSplit, msgspec.Meta(title="Files", description="One file, a file per output class, or a file per pass")] = FileSplit.ONE
    convention: Annotated[Conventions, msgspec.Meta(title="Reader", description="Layer names and axes reading application expects")] = Blender()
    totals: Annotated[frozenset[Family], msgspec.Meta(title="Totals", description="Light families summed into one layer each")] = frozenset()
    derived: Annotated[frozenset[Derived], msgspec.Meta(title="Derived", description="Light sums and depth layers built from passes")] = frozenset()
    ids: Annotated[frozenset[Entity], msgspec.Meta(title="ID Colors", description="Cryptomatte kinds drawn as one color per name")] = frozenset()
    files: Annotated[tuple[PassFile, ...], msgspec.Meta(title="Passes", description="File choice per pass")] = ()


class PassPreset(msgspec.Struct, frozen=True, kw_only=True):
    """Pass flags and routing saved together."""

    flags: Snapshot
    routing: Routing


class PassName(msgspec.Struct, frozen=True, kw_only=True):
    """Source name a file row adds from sources no row holds."""

    name: Annotated[str, msgspec.Meta(title="Pass", description="Pass the file row names"), offered] = ""


class Source(msgspec.Struct, frozen=True, kw_only=True):
    """Socket a layer's files write."""

    name: str
    end: End
    kind: DataType
    dimensions: Option[int]
    output: Output


class Filing(msgspec.Struct, frozen=True, kw_only=True):
    """Sources, class, and kind of one File Output node."""

    output: Output
    kind: Kind
    sources: tuple[Source, ...]


class LayerPlan(msgspec.Struct, frozen=True, kw_only=True):
    """Frame key and filing per File Output key of one used view layer."""

    frame: Key
    filings: dict[Key, Filing]


class PassRouting(msgspec.Struct, frozen=True, kw_only=True):
    """`PassSet` bound to an identity, members resolved once."""

    identity: Identity
    routing: Attached[Routing]
    pathed: Pathed
    layers: Attached[Layer]
    flags: PassFlags
    lobe: NodeGroup
    id_colors: dict[Entity, Descriptor[bpy.types.FloatProperty]]
    use: Descriptor[bpy.types.BoolProperty]
    precision: Descriptor[bpy.types.EnumProperty]
    label: Descriptor[bpy.types.StringProperty]
    custom: Descriptor[bpy.types.BoolProperty]
    tint: Descriptor[bpy.types.FloatProperty]
    usage: Descriptor[bpy.types.BoolProperty]
    dimensions: Descriptor[bpy.types.IntProperty]

    def used(self, scene: bpy.types.Scene) -> tuple[bpy.types.ViewLayer, ...]:
        """Returns scene layers with `use` on."""
        return tuple(layer for layer in scene.view_layers if bool(self.use.read(layer)))

    def grouped(self, frames: tuple[tuple[FrameNode, tuple[Link, ...], LayerPlan], ...]) -> BlenderResult[NodeGroup]:
        """Builds routing group around image pass-through with each layer frame."""
        interface = Interface(outputs=(Socket(key="result", name="Image", kind=bpy.types.NodeSocketColor),), inputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),))
        return NodeGroup.built(PassSet.ownership.key, "Passes", interface, {"result": End(node=INPUTS, socket="image")}).map(
            lambda group: msgspec.structs.replace(
                group,
                graph=msgspec.structs.replace(
                    group.graph,
                    members={self.usage: True},
                    nodes={**group.graph.nodes, **{plan.frame: frame for frame, _, plan in frames}},
                    links=tuple(dict.fromkeys((*group.graph.links, *(link for _, links, _ in frames for link in links)))),
                ),
            )
        )

    def held(self, scene: bpy.types.Scene) -> Option[bpy.types.NodeTree]:
        """Returns routing tree identity holds for scene."""
        return PassSet.ownership.found(self.identity, of=Some(scene))

    def framed(self, scene: bpy.types.Scene, held: dict[Key, bpy.types.Node], path: OutputPath, index: int, layer: bpy.types.ViewLayer) -> BlenderResult[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
        """Plans frame, Render Layers node, sources, and File Output nodes of one used layer."""
        prefix = f"layer_{index}"

        def filed_all(routing: Routing, shown: tuple[float, ...], render: Fragment, built: Fragment, found: tuple[Source, ...]) -> BlenderResult[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
            plan = {f"{prefix}_{filing.output.name.lower()}_{filing.kind.name.lower()}": filing for filing in filings(found, routing.split)}
            return (
                accumulated(filed(self.pathed, path, layer.name, filing, key, routing.convention, self.dimensions) for key, filing in plan.items())
                .bind(lambda parts: Fragment.merged((render, built, *parts)))
                .map(lambda body: (FrameNode(members={self.label: layer.name, self.custom: True, self.tint: shown}, nodes=body.nodes), body.links, LayerPlan(frame=prefix, filings=plan)))
            )

        def routed(read: tuple[Routing, Layer], render: Fragment) -> BlenderResult[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
            routing, record = read
            shown = encoded(record.color)
            return (
                Option
                .of_optional(held.get(f"{prefix}_render"))
                .map(lambda live: sources(scene, layer, prefix, live, routing, record.role, self))
                .default_value(Ok((Fragment(), ())))
                .bind(lambda found: filed_all(routing, shown, render, *found))
            )

        return paired(paired(self.routing.read(layer), self.layers.read(layer)), node(f"{prefix}_render", bpy.types.CompositorNodeRLayers, {}, {"scene": IdName.of(scene), "layer": layer.name})).bind(
            lambda pair: routed(*pair)
        )

    def planned(self, scene: bpy.types.Scene, tree: Option[bpy.types.NodeTree], path: OutputPath) -> BlenderResult[tuple[NodeGroup, dict[str, LayerPlan]]]:
        """Plans scene routing group, reading offered sockets from held tree."""
        held = tree.map(lambda found: owned(found, self.identity)).default_value({})
        used = self.used(scene)
        return accumulated(self.framed(scene, held, path, index, layer) for index, layer in enumerate(used)).bind(
            lambda frames: self.grouped(frames).map(lambda group: (group, {layer.name: plan for layer, (_, _, plan) in zip(used, frames)}))
        )

    def reconciled(self, scene: bpy.types.Scene, context: bpy.types.Context, path: OutputPath) -> BlenderResult[Changes]:
        """Reconciles passes, AOVs, light groups, and routing to scene settings, called after overrides apply or revert."""
        held = self.held(scene)
        before = held.map(lambda found: items(found, self.identity)).default_value({})

        def applied(tree: bpy.types.NodeTree, group: NodeGroup, plans: dict[str, LayerPlan]) -> BlenderResult[Changes]:
            return reconciled(group, tree, self.identity).bind(lambda rows: formatted(tree, group, plans, rows, before, self.identity).map(lambda _: rows))

        def rebuilt(first: NodeGroup, _: dict[str, LayerPlan]) -> BlenderResult[Changes]:
            return made(first, self.identity, of=Some(scene)).bind(lambda tree: self.planned(scene, Some(tree), path).bind(lambda planned: applied(tree, *planned)))

        def requested(layer: bpy.types.ViewLayer) -> BlenderResult[PassRequest]:
            return self.routing.read(layer).map(lambda routing: PassRequest(layer=layer.name, flags=self.flags.opened(routing)))

        return (
            accumulated(map(requested, self.used(scene)))
            .bind(lambda requests: required(scene, context, requests, self.precision))
            .bind(lambda _: synced(scene, self.identity))
            .bind(lambda _: self.planned(scene, held, path))
            .bind(lambda first: rebuilt(*first))
        )

    def refreshed(self, context: bpy.types.Context, day: datetime.date) -> BlenderResult[None]:
        """Reconciles every editable scene with stored layer routing, owned runtime scenes skipped."""
        if (data := context.blend_data) is None:
            return Error((BlenderAbsentFault(owner=repr(context), key="blend_data"),))
        scenes = (
            scene
            for scene in data.scenes
            if scene.is_editable and not (scene.is_runtime_data and self.identity.manifest.id in scene) and any(self.routing.stored(layer) for layer in scene.view_layers)  # type: ignore[operator]
        )

        def refresh(scene: bpy.types.Scene) -> BlenderResult[Changes]:
            return self.pathed.path(scene, day).bind(lambda path: self.reconciled(scene, context, path))

        return accumulated(map(refresh, scenes)).map(lambda _: None)

    def stale(self, context: bpy.types.Context) -> BlenderResult[Option[str]]:
        """Returns pending routing group writes for context scene, `Nothing` for a current tree."""
        if (scene := context.scene) is None:
            return Ok(Nothing)

        def pending(tree: bpy.types.NodeTree, group: NodeGroup, _: dict[str, LayerPlan]) -> BlenderResult[Option[str]]:
            return changes(group, tree, self.identity).map(lambda rows: sum(map(len, msgspec.structs.astuple(rows)))).map(lambda count: Some(f"{count} changes pending") if count else Nothing)

        return self.pathed.path(scene, datetime.datetime.now(datetime.UTC).astimezone().date()).bind(
            lambda path: self.held(scene).map(lambda tree: self.planned(scene, Some(tree), path).bind(lambda planned: pending(tree, *planned))).default_value(Ok(Some("Update builds routing group")))
        )

    def watched(self) -> BlenderResult[tuple[Subscription, ...]]:
        """Builds subscriptions refreshing routing, one per watched setting."""
        watched = (
            (bpy.types.ViewLayer, "use"),
            (bpy.types.ViewLayer, "name"),
            (bpy.types.ViewLayer, "pass_cryptomatte_depth"),
            (bpy.types.ViewLayer, "material_override"),
            (bpy.types.AOV, "name"),
            (bpy.types.AOV, "type"),
            (bpy.types.Lightgroup, "name"),
            (bpy.types.Object, "lightgroup"),
            (bpy.types.Object, "color"),
            (bpy.types.Object, "hide_render"),
            (bpy.types.Object, "parent"),
            (bpy.types.World, "lightgroup"),
            (bpy.types.Material, "diffuse_color"),
            (bpy.types.RenderSettings, "engine"),
            (bpy.types.Scene, "camera"),
        )
        groups = tuple((attached.group.struct, prop) for attached in (self.routing, self.pathed.attached, self.layers) for prop in attached.group.struct.bl_rna.properties if prop.is_runtime)

        def resolved(owner: "type[bpy.types.bpy_struct[object]]", member: str) -> "BlenderResult[tuple[type[bpy.types.bpy_struct[object]], bpy.types.Property]]":
            return Descriptor.resolve(owner, member, bpy.types.Property).map(lambda found: (owner, found.prop))

        return accumulated(itertools.starmap(resolved, watched)).map(
            lambda resolved: tuple(
                Subscription(owner=owner, setting=prop, body=lambda: self.refreshed(bpy.context, datetime.datetime.now(datetime.UTC).astimezone().date()))
                for owner, prop in (*((owner, flag.prop) for owner, flag in self.flags.every), *resolved, *groups)
            )
        )

    def operators(self) -> tuple[Operator[Empty], Operator[Empty], Operator[Empty], Operator[PassName]]:
        """Returns routing operators."""

        def recoloring(chosen: Callable[[tuple[Rgb, ...]], tuple[Rgb, ...]]) -> Callable[[Call[Empty]], BlenderResult[Outcome]]:
            def painted(layer: bpy.types.ViewLayer, routing: Routing) -> BlenderResult[Outcome]:
                if not routing.ids:
                    return Ok(Unchanged(reason=f"{layer.name} draws no ID colors"))
                return accumulated(recolored(layer, entity, self.id_colors[entity], chosen) for entity in sorted(routing.ids)).map(
                    lambda counts: Done() if sum(counts) else Unchanged(reason=f"{layer.name} ID colors hold chosen colors")
                )

            return lambda call: Result[bpy.types.ViewLayer, BlenderFaults].bind(call.root(bpy.types.ViewLayer), lambda layer: self.routing.read(layer).bind(lambda routing: painted(layer, routing)))

        def added(call: Call[PassName]) -> BlenderResult[Done]:
            if (layer := (call.context.view_layer)) is None:
                return Error((BlenderAbsentFault(owner=repr(call.context), key="view_layer"),))
            return (
                self.routing
                .read(layer)
                .bind(lambda routing: self.routing.write(layer, msgspec.structs.replace(routing, files=(*routing.files, PassFile(name=call.record.name)))))
                .map(lambda _: Done())
            )

        def updated(call: Call[Empty]) -> BlenderResult[Done]:
            return (
                call
                .root(bpy.types.Scene)
                .bind(lambda scene: self.pathed.path(scene, datetime.datetime.now(datetime.UTC).astimezone().date()).bind(lambda path: self.reconciled(scene, call.context, path)))
                .map(lambda _: Done())
            )

        return (
            Operator(key="passes_update", title="Update File Outputs", description="Reconcile scene pass routing and File Output nodes", form=Empty, run=updated),
            Operator(
                key="ids_spread",
                title="Spread ID Colors",
                description="Set evenly spaced hues on layer ID names",
                form=Empty,
                run=recoloring(lambda held: colors(Spectrum(saturation=1.0, value=1.0), len(held))),
            ),
            Operator(
                key="ids_shuffle",
                title="Shuffle ID Colors",
                description="Shuffle colors among layer ID names",
                form=Empty,
                run=recoloring(lambda held: tuple(held[index] for index in rng(content_hash(Routing, *held)).permutation(len(held)))),
            ),
            Operator(key="pass_file_add", title="Add Pass File", description="Add file choice for a layer pass", form=PassName, invoke=Some(InvokePopup.SEARCH), run=added),
        )

    def listing(self) -> Listing[PassPreset]:
        """Returns pass presets over layer flags and routing, applied to every used layer."""
        selection = Some(tuple(flag for _, flag in self.flags.every))
        families = frozenset().union(*(flags.direct | flags.indirect | flags.color for flags in self.flags.families.values()))
        eevee = frozenset().union(
            *(self.flags.families[family].direct | self.flags.families[family].color for family in (Family.DIFFUSE, Family.GLOSSY)),
            frozenset(flag for flag in self.flags.families[Family.VOLUME].direct if flag.owner == "eevee"),
        )
        common = frozenset({"use_pass_emit", "use_pass_environment"})
        family_paths = frozenset(flag.path for flag in families)
        eevee_paths = frozenset(flag.path for flag in eevee)
        recipes = (
            ("lightray_rebuild", common | family_paths, Routing(derived=frozenset({Derived.DIRECT, Derived.INDIRECT}))),
            ("material_rebuild", common | family_paths, Routing(totals=frozenset(Family))),
            ("multipass_breakdown", common | family_paths, Routing()),
            ("eevee_material_rebuild", common | eevee_paths, Routing(totals=frozenset({Family.DIFFUSE, Family.GLOSSY, Family.VOLUME}))),
            ("eevee_multipass", common | eevee_paths | {"eevee.use_pass_transparent", "use_pass_shadow", "use_pass_ambient_occlusion"}, Routing()),
        )

        def stock(enabled: frozenset[str], routing: Routing) -> PassPreset:
            owners = tuple(dict.fromkeys(flag.owner for _, flag in self.flags.every))
            return PassPreset(
                flags=Snapshot(
                    struct=bpy.types.ViewLayer.__name__,
                    tree=Tree(roots=owners, owners={owner: Owner(values={flag.prop.identifier: flag.path in enabled for _, flag in self.flags.every if flag.owner == owner}) for owner in owners}),
                ),
                routing=routing,
            )

        def held(layer: bpy.types.ViewLayer) -> BlenderResult[PassPreset]:
            return paired(Snapshot.capture(layer, selection), self.routing.read(layer)).map(lambda pair: PassPreset(flags=pair[0], routing=pair[1]))

        def same(current: PassPreset, value: PassPreset) -> bool:
            return not transfer(current.flags.tree, value.flags.tree) and current.routing == value.routing

        def captured(context: bpy.types.Context) -> BlenderResult[PassPreset]:
            return rooted(bpy.types.ViewLayer, context).bind(held)

        def matches(context: bpy.types.Context, values: tuple[PassPreset, ...]) -> BlenderResult[tuple[bool, ...]]:
            return captured(context).map(lambda current: tuple(same(current, value) for value in values))

        def applied(context: bpy.types.Context, value: PassPreset) -> BlenderResult[Outcome]:
            def wrote(scene: bpy.types.Scene) -> BlenderResult[Outcome]:
                layers = self.used(scene)

                def chosen(current: tuple[PassPreset, ...]) -> BlenderResult[Outcome]:
                    if not (differing := tuple(layer for layer, preset in zip(layers, current) if not same(preset, value))):
                        return Ok(Unchanged(reason=f"Used view layers of {scene.name} match preset"))
                    return tallied(tuple(paired(value.flags.apply(layer).outcome, self.routing.write(layer, value.routing)) for layer in differing))

                return accumulated(map(held, layers)).bind(chosen)

            return rooted(bpy.types.Scene, context).bind(wrote)

        return Listing(preset=msgspec.structs.replace(PassSet.presets, stock={name: stock(enabled, routing) for name, enabled, routing in recipes}), current=captured, matches=matches, applied=applied)


class PassSet(Line, frozen=True):
    """Row of pass routing."""

    attachment: ClassVar[Attachment[Routing]] = Attachment(key="passes", form=Routing, owner=bpy.types.ViewLayer)
    presets: ClassVar[Preset[PassPreset]] = Preset(key="passes", form=PassPreset)
    ownership: ClassVar[Ownership[bpy.types.CompositorNodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key="passes", saved=Saved.USED)

    def bound(self, identity: Identity) -> BlenderResult[PassRouting]:
        """Binds attachments and resolves flags, lobe group, and descriptors."""
        attached = paired(paired(self.attachment.bound(identity), ScenePaths().bound(identity)), LayerOverride.layer.bound(identity))
        switches = accumulated((
            Descriptor.resolve(bpy.types.ViewLayer, "use", bpy.types.BoolProperty),
            Descriptor.resolve(bpy.types.NodeFrame, "use_custom_color", bpy.types.BoolProperty),
            Descriptor.resolve(bpy.types.CompositorNodeTree, "allow_usage_in_scene_compositor_effect", bpy.types.BoolProperty),
        ))
        frames = paired(Descriptor.resolve(bpy.types.NodeFrame, "label", bpy.types.StringProperty), Descriptor.resolve(bpy.types.NodeFrame, "color", bpy.types.FloatProperty))
        others = paired(
            Descriptor.resolve(bpy.types.Scene, "render.compositor_precision", bpy.types.EnumProperty),
            Descriptor.resolve(bpy.types.NodeCompositorFileOutputItem, "vector_socket_dimensions", bpy.types.IntProperty),
        )
        members = accumulated(entity.member() for entity in Entity).map(lambda found: dict(zip(Entity, found)))

        def built(
            held: tuple[tuple[tuple[Attached[Routing], Pathed], Attached[Layer]], tuple[tuple[PassFlags, NodeGroup], dict[Entity, Descriptor[bpy.types.FloatProperty]]]],
            resolved: tuple[
                tuple[Descriptor[bpy.types.BoolProperty], ...],
                tuple[tuple[Descriptor[bpy.types.StringProperty], Descriptor[bpy.types.FloatProperty]], tuple[Descriptor[bpy.types.EnumProperty], Descriptor[bpy.types.IntProperty]]],
            ],
        ) -> PassRouting:
            (((routing, pathed), layers), ((flags, group), id_colors)), (toggles, ((label, tint), (precision, dimensions))) = held, resolved
            use, custom, usage = toggles
            return PassRouting(
                identity=identity,
                routing=routing,
                pathed=pathed,
                layers=layers,
                flags=flags,
                lobe=group,
                id_colors=id_colors,
                use=use,
                precision=precision,
                label=label,
                custom=custom,
                tint=tint,
                usage=usage,
                dimensions=dimensions,
            )

        return paired(paired(attached, paired(paired(PassFlags.of(), lobe()), members)), paired(switches, paired(frames, others))).map(lambda parts: built(*parts))

    def written(self, scene: bpy.types.Scene, frame: int, identity: Identity, *, path: Option[OutputPath] = Nothing) -> BlenderResult[tuple[Written, ...]]:
        """Returns files scene File Output nodes wrote for a frame at `path` or scene path, the `Files` port."""

        def filing_files(
            routing: PassRouting, path: OutputPath, layer: bpy.types.ViewLayer, live: bpy.types.CompositorNodeOutputFile, filing: Filing, convention: Conventions
        ) -> BlenderResult[tuple[Written, ...]]:
            names = tuple(convention.layer(source.name) for source in filing.sources)

            def targeted(paths: NodePaths) -> tuple[tuple[bpy.types.ImageFormatSettings, Option[str], Map[str, str]], ...]:
                match filing.kind:
                    case Kind.LAYERED:
                        return tuple((settings, Nothing, Map.of_seq(zip(paths.items, (source.name for source in filing.sources)))) for settings in Option.of_optional(live.format).to_list())
                    case Kind.EXR | Kind.IMAGE:
                        stored = {item.name: item for item in live.file_output_items}
                        return tuple(
                            (settings, Some(name), Map.of_seq((("", source.name),)))
                            for name, source, kept in zip(names, filing.sources, paths.items)
                            for item in Option.of_optional(stored.get(kept)).to_list()
                            for settings in Option.of_optional(item.format if item.override_node_format else live.format).to_list()
                        )

            def filed_at(settings: bpy.types.ImageFormatSettings, item: Option[str], passes: Map[str, str], suffix: str) -> BlenderResult[Option[Written]]:
                return path.written(layer.name, filing.output, item, frame, suffix).map(lambda file: Written.of(str(file), settings, filing.output, convention, layer.name, passes))

            return (
                routing.pathed
                .node(path, layer.name, filing.output, names, filing.kind)
                .bind(lambda paths: accumulated(filed_at(settings, item, passes, suffix) for settings, item, passes in targeted(paths) for suffix in extension(settings).to_list()))
                .map(lambda found: tuple(file for option in found for file in option.to_list()))
            )

        def collected(routing: PassRouting, path: OutputPath) -> BlenderResult[tuple[Written, ...]]:
            tree = routing.held(scene)
            held = tree.map(lambda found: owned(found, identity)).default_value({})

            def layer_files(plans: dict[str, LayerPlan], layer: bpy.types.ViewLayer) -> BlenderResult[tuple[Written, ...]]:
                return (
                    routing.routing
                    .read(layer)
                    .bind(
                        lambda read: accumulated(
                            filing_files(routing, path, layer, live, filing, read.convention)
                            for key, filing in plans[layer.name].filings.items()
                            for live in (held.get(key),)
                            if isinstance(live, bpy.types.CompositorNodeOutputFile)
                        )
                    )
                    .map(lambda parts: tuple(itertools.chain.from_iterable(parts)))
                )

            def gathered(_: NodeGroup, plans: dict[str, LayerPlan]) -> BlenderResult[tuple[Written, ...]]:
                return accumulated(layer_files(plans, layer) for layer in routing.used(scene)).map(lambda parts: tuple(itertools.chain.from_iterable(parts)))

            return routing.planned(scene, tree, path).bind(lambda planned: gathered(*planned))

        return (
            self
            .bound(identity)
            .bind(lambda routing: path.map(Ok).default_with(lambda: routing.pathed.path(scene, datetime.datetime.now(datetime.UTC).astimezone().date())).map(lambda chosen: (routing, chosen)))
            .bind(lambda found: collected(*found))
        )

    @property
    def page_rows(self) -> tuple[Attachment[Routing]]:
        """Routing attachment preferences page stores."""
        return (self.attachment,)

    def document(self, identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> BlenderResult[Document[np.uint8 | np.uint16 | np.float32]]:
        """Builds ID document of a written frame with one group per matte, the `Documents` port."""
        size = (frame.parts[0].height, frame.parts[0].width)

        def built(
            members: dict[Entity, Descriptor[bpy.types.FloatProperty]], mattes: tuple[Matte, ...], coded: Callable[[npt.NDArray[np.float32]], npt.NDArray[np.uint8 | np.uint16 | np.float32]]
        ) -> BlenderResult[Document[np.uint8 | np.uint16 | np.float32]]:
            def covered(matte: Matte, name: str, color: Rgb) -> BlenderResult[tuple[str, Rgb, npt.NDArray[np.float32]]]:
                return matte.coverage(frozenset({name})).map(lambda plane: (name, color, plane)).map_error(BlenderImagingFault.lifted)

            def grouped(index: int, matte: Matte) -> BlenderResult[tuple[Group[np.uint8 | np.uint16 | np.float32], npt.NDArray[np.float32]]]:
                def listed(layer: bpy.types.ViewLayer, entity: Entity) -> BlenderResult[tuple[tuple[str, Rgb, npt.NDArray[np.float32]], ...]]:
                    return accumulated(covered(matte, name, color) for name, _, color in named(layer, entity, members[entity]))

                layer, _, kind = matte.name.rpartition(".")
                located: BlenderResult[tuple[bpy.types.ViewLayer, Entity]] = paired(
                    Option.of_optional(scene.view_layers.get(layer)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.view_layers), key=layer),)),
                    Entity.named(kind).to_result_with(lambda: (BlenderAbsentFault(owner=matte.file.path, key=matte.name),)),
                )
                return (
                    Result[tuple[bpy.types.ViewLayer, Entity], BlenderFaults]
                    .bind(located, lambda pair: listed(*pair))
                    .map(
                        lambda rows: (
                            Group(
                                name=matte.name,
                                visible=index == 0,
                                layers=tuple(
                                    Raster(name=name, color=coded(np.broadcast_to(np.asarray(encoded(color), dtype=np.float32), (*size, 3))), alpha=coded(plane)) for name, color, plane in rows
                                ),
                            ),
                            functools.reduce(np.add, (plane[..., None] * np.asarray((*encoded(color), 1.0), dtype=np.float32) for _, color, plane in rows), np.zeros((*size, 4), np.float32)),
                        )
                    )
                )

            return accumulated(itertools.starmap(grouped, enumerate(mattes))).map(
                lambda groups: Document(
                    layers=tuple(group for group, _ in groups),
                    composite=coded(unassociated(next((filled for _, filled in groups), np.zeros((*size, 4), np.float32)))),
                    colorspace="sRGB",
                    density=scene.render.ppm_factor / scene.render.ppm_base,
                )
            )

        return paired(self.bound(identity), Matte.found(frame).map_error(BlenderImagingFault.lifted)).bind(lambda pair: built(pair[0].id_colors, pair[1], depth.coded))

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Draws film transparency, pending changes, operator, and preset rows."""
        tested = paired(
            paired(Descriptor.resolve(bpy.types.Scene, "render.film_transparent", bpy.types.BoolProperty), Descriptor.resolve(bpy.types.Scene, "render.engine", bpy.types.EnumProperty)),
            paired(
                Descriptor.resolve(bpy.types.ViewLayer, "cycles.use_pass_shadow_catcher", bpy.types.BoolProperty),
                Descriptor.resolve(bpy.types.ViewLayer, "eevee.use_pass_transparent", bpy.types.BoolProperty),
            ),
        )

        def rows(
            routing: PassRouting,
            descriptors: tuple[tuple[Descriptor[bpy.types.BoolProperty], Descriptor[bpy.types.EnumProperty]], tuple[Descriptor[bpy.types.BoolProperty], Descriptor[bpy.types.BoolProperty]]],
        ) -> tuple[Row, ...]:
            (film, engine), (catcher, transparent) = descriptors
            update, spread, shuffle, add = routing.operators()
            opaque = AllOf(
                conditions=tuple(
                    AnyOf(conditions=(Setting(descriptor=flag, accepted=(False,)), Not(condition=Setting(descriptor=engine, accepted=(running,)))))
                    for flag, running in ((catcher, "CYCLES"), (transparent, "BLENDER_EEVEE"))
                )
            )
            return (
                Property(target=Member(root=bpy.types.Scene, path="render.film_transparent"), condition=Some(AnyOf(conditions=(Setting(descriptor=film), opaque)))),
                Label(text=lambda context: routing.stale(context).default_with(lambda faults: Some("\n".join(text for _, text in rendered(faults))))),
                Caption(rows=(OperatorButton(target=update), OperatorButton(target=spread), OperatorButton(target=shuffle), OperatorButton(target=add)), flow=Flow.GRID),
                routing.listing(),
            )

        return paired(self.bound(identity), tested).bind(lambda pair: placed(rows(*pair), identity)).map(joined)

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns routing acquisitions, attachment left to preferences page."""

        def acquired(routing: PassRouting, rows: tuple[Subscription, ...]) -> Acquisitions:
            handlers = (bpy.app.handlers.load_post, bpy.app.handlers.save_post)
            return (
                self.ownership,
                routing.lobe,
                *routing.operators(),
                routing.listing(),
                *(PathHandler(handlers=held, body=lambda _: routing.refreshed(bpy.context, datetime.datetime.now(datetime.UTC).astimezone().date())) for held in handlers),  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                *rows,
            )

        return self.bound(identity).bind(lambda routing: routing.watched().map(lambda rows: acquired(routing, rows)))

    def section(self, retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing) -> Single[Routing]:
        """Returns File Output section over context layer routing store."""
        return Single(
            key="passes", title="File Output", rows=(self,), store=Some(Hosted(host=Member(root=bpy.types.Context, path="view_layer"), store=Store(attachment=self.attachment, retention=retention)))
        )


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [FLAGS]


def pass_flags() -> "tuple[tuple[type[bpy.types.bpy_struct[object]], Descriptor[bpy.types.BoolProperty]], ...]":
    """Returns writable pass flags of `ViewLayer` and its pointer structs in RNA order with owning class."""
    root = bpy.types.ViewLayer.bl_rna
    structs = (
        ("", bpy.types.ViewLayer, root.properties),
        *((prop.identifier, type(fixed), fixed.properties) for prop in root.properties if isinstance(prop, bpy.types.PointerProperty) for fixed in Option.of_optional(prop.fixed_type).to_list()),
    )
    return tuple(
        (owner, Descriptor(root=bpy.types.ViewLayer, owner=path, prop=prop))
        for path, owner, properties in structs
        for prop in properties
        if isinstance(prop, bpy.types.BoolProperty) and not prop.is_readonly and prop.identifier.startswith("use_pass_")
    )


def required(scene: bpy.types.Scene, context: bpy.types.Context, requests: tuple[PassRequest, ...], precision: Descriptor[bpy.types.EnumProperty]) -> BlenderResult[None]:
    """Enables passes and precision each request needs, faulting unmet conditions."""

    def held(descriptor: Descriptor, root: bpy.types.ViewLayer | bpy.types.Scene, value: Value) -> BlenderResult[None]:
        return (
            struct_at(root, descriptor.owner)
            .map(lambda owner: descriptor.write(owner, value).map(lambda _: None))
            .default_with(lambda: Error((BlenderAbsentFault(owner=repr(root), key=descriptor.path),)))
        )

    def met(request: PassRequest) -> BlenderResult[None]:
        gate: BlenderResult[None] = (
            request.condition.filter(lambda condition: not holds(condition, context, Some(scene))).map(lambda condition: Error((BlenderUnmetFault(requirement=condition),))).default_value(Ok(None))
        )
        return (
            gate
            .bind(lambda _: Option.of_optional(scene.view_layers.get(request.layer)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.view_layers), key=request.layer),)))
            .bind(
                lambda layer: accumulated((
                    *(held(flag, layer, value=True) for flag in sorted(request.flags, key=lambda flag: flag.path)),
                    *((held(precision, scene, value="FULL"),) if request.precise else ()),
                ))
            )
            .map(lambda _: None)
        )

    return accumulated(map(met, requests)).map(lambda _: None)


# --- [SOURCES]


def lobe() -> BlenderResult[NodeGroup]:
    """Builds shared lobe total group, `(direct + indirect) * color`."""
    outputs = (
        mixed("sum", "RGBA", 1.0, End(node=INPUTS, socket="direct"), End(node=INPUTS, socket="indirect"), "ADD")
        .bind(lambda total: mixed("product", "RGBA", 1.0, total, End(node=INPUTS, socket="color"), "MULTIPLY"))
        .map(lambda product: {"total": product})
    )
    interface = Descriptor.resolve(bpy.types.NodeTreeInterfaceSocketColor, "default_value", bpy.types.FloatProperty).map(
        lambda default: Interface(
            outputs=(Socket(key="total", name="Total", kind=bpy.types.NodeSocketColor),),
            inputs=tuple(
                Socket(key=key, name=key.title(), kind=bpy.types.NodeSocketColor, members={default: value})
                for key, value in (("direct", (0.0, 0.0, 0.0, 1.0)), ("indirect", (0.0, 0.0, 0.0, 1.0)), ("color", (1.0, 1.0, 1.0, 1.0)))
            ),
        )
    )
    return paired(interface, outputs).bind(lambda pair: NodeGroup.built("lobe_total", "Lobe Total", *pair))


def term(key: Key, family: Family, slots: tuple[Key, *tuple[Key, ...]], render: Key, offered: dict[str, str], group: NodeGroup) -> Option[BlenderResult[tuple[Fragment, End]]]:
    """Builds lobe term of one family over offered slots, `Nothing` without first slot socket."""
    first, *_ = slots
    linked = {slot: End(node=render, socket=offered[name]) for slot in slots if (name := family.socket(slot)) in offered}
    return Some(nested(key, group, linked, "total")) if first in linked else Nothing


def turned(prefix: str, end: End, axes: tuple[Axis, Axis, Axis], camera: Option[bpy.types.Object]) -> BlenderResult[tuple[Fragment, End]]:
    """Converts vector to reader axes, in camera space when given a camera."""
    split, join = f"{prefix}_split", f"{prefix}_join"
    rotated = camera.map(
        lambda held: (
            node(f"{prefix}_camera", bpy.types.GeometryNodeObjectInfo, {"Object": IdName.of(held)}, {})
            .map(ended(f"{prefix}_camera", "Rotation"))
            .bind(lambda info: node(f"{prefix}_invert", bpy.types.FunctionNodeInvertRotation, {"Rotation": info}, {}).map(ended(f"{prefix}_invert", "Rotation")))
            .bind(lambda inverse: node(f"{prefix}_turn", bpy.types.FunctionNodeRotateVector, {"Vector": end, "Rotation": inverse}, {}).map(ended(f"{prefix}_turn", "Vector")))
        )
    ).default_value(Ok((Fragment(), end)))

    def swizzled(source: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        def component(separated: Fragment, index: int, axis: Axis) -> BlenderResult[tuple[Fragment, End]]:
            match axis:
                case 1 | -1:
                    channel = "X"
                case 2 | -2:
                    channel = "Y"
                case 3 | -3:
                    channel = "Z"
            read = ended(split, channel)(separated)
            return math(f"{prefix}_negate_{index}", "MULTIPLY", read, -1.0) if axis < 0 else Ok(read)

        return (
            node(split, bpy.types.ShaderNodeSeparateXYZ, {"Vector": source}, {})
            .bind(lambda separated: accumulated(component(separated, index, axis) for index, axis in enumerate(axes)))
            .bind(lambda parts: node(join, bpy.types.ShaderNodeCombineXYZ, dict(zip("XYZ", parts)), {}))
            .map(ended(join, "Vector"))
        )

    return rotated.bind(lambda source: swizzled(source) if axes != (1, 2, 3) else Ok(source))


def reordered(prefix: str, end: End, order: tuple[int, int, int, int]) -> BlenderResult[tuple[Fragment, End]]:
    """Reorders motion vector channels to reader order."""
    split, join, channels = f"{prefix}_split", f"{prefix}_join", ("Red", "Green", "Blue", "Alpha")
    return (
        node(split, bpy.types.CompositorNodeSeparateColor, {"Image": end}, {})
        .bind(lambda separated: node(join, bpy.types.CompositorNodeCombineColor, {channel: ended(split, channels[source - 1])(separated) for channel, source in zip(channels, order)}, {}))
        .map(ended(join, "Image"))
    )


def sources(
    scene: bpy.types.Scene, layer: bpy.types.ViewLayer, prefix: str, live: bpy.types.Node, routing: Routing, role: Role, bound: PassRouting
) -> BlenderResult[tuple[Fragment, tuple[Source, ...]]]:
    """Returns sources layer files write in socket order, derived layers and ID images after."""
    render, convention = f"{prefix}_render", routing.convention
    kinds: dict[str, DataType] = {"VALUE": "FLOAT", "VECTOR": "VECTOR", "RGBA": "RGBA"}
    offered = {socket.name: socket for socket in live.outputs if socket.enabled and socket.name != "Alpha" and socket.type in kinds}
    identifiers = {name: socket.identifier for name, socket in offered.items()}
    spaces = {"Normal": convention.normals, "Position": Space.WORLD, DataPass.POSITION.name.lower(): Space.WORLD, DataPass.REFERENCE.name.lower(): Space.WORLD}
    gates = {row.name: row.output for row in routing.files}

    def missing(name: str) -> BlenderFaults:
        return (BlenderAbsentFault(owner=repr(live.outputs), key=name),)

    def oriented(name: str, socket: bpy.types.NodeSocket) -> BlenderResult[tuple[Fragment, End]]:
        end, key = End(node=render, socket=socket.identifier), f"{prefix}_pass_{socket.identifier.encode().hex()}"
        camera: BlenderResult[bpy.types.Object] = Option.of_optional(scene.camera).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key="camera"),))
        return (
            Option
            .of_optional(spaces.get(name))
            .map(
                lambda space: (
                    Result[bpy.types.Object, BlenderFaults].bind(camera, lambda held: turned(key, end, convention.axes[space], Some(held)))
                    if space is Space.CAMERA
                    else turned(key, end, convention.axes[space], Nothing)
                )
            )
            .default_with(lambda: reordered(key, end, convention.motion) if name == "Vector" and convention.motion != (1, 2, 3, 4) else Ok((Fragment(), end)))
        )

    def classed(name: str, socket: bpy.types.NodeSocket) -> Output:
        return (
            Output.MATTE
            if any(name.startswith(entity.pass_name) and name.removeprefix(entity.pass_name).isdigit() for entity in Entity)
            else Output.DATA
            if role is Role.DATA or socket.type != "RGBA"
            else Output.BEAUTY
        )

    def depth() -> BlenderResult[End]:
        return Option.of_optional(identifiers.get(DataPass.DEPTH.name.lower())).map(lambda identifier: End(node=render, socket=identifier)).to_result_with(lambda: missing(DataPass.DEPTH.name.lower()))

    def lit(item: Derived) -> BlenderResult[Option[tuple[Fragment, End]]]:
        def added(terms: tuple[tuple[Fragment, End], ...]) -> BlenderResult[Option[tuple[Fragment, End]]]:
            match terms:
                case (first, *rest):
                    return summed(f"{prefix}_{item.name.lower()}", (first, *rest)).map(Some)
                case _:
                    return Ok(Nothing)

        slot = item.name.lower()
        match item:
            case Derived.DIRECT | Derived.INDIRECT:
                return accumulated(built for family in Family for built in term(f"{prefix}_{family.name.lower()}_{slot}", family, (slot, "color"), render, identifiers, bound.lobe).to_list()).bind(
                    added
                )
            case Derived.DEPTH:
                return depth().bind(lambda end: node(f"{prefix}_depth_map", bpy.types.CompositorNodeNormalize, {"Value": end}, {})).map(ended(f"{prefix}_depth_map", "Value")).map(Some)
            case Derived.INVERSE:
                return depth().bind(lambda end: math(f"{prefix}_inverse_depth", "DIVIDE", 1.0, end)).map(Some)

    def gated(source: Source) -> BlenderResult[Option[Source]]:
        gate = gates.get(source.name, Gated(enabled=True, value=source.output))
        match gate.value:
            case Output.BEAUTY | Output.DATA | Output.MATTE as chosen:
                return Ok(Some(msgspec.structs.replace(source, output=chosen)).filter(lambda _: gate.enabled))
            case Output.PREVIEW:
                accepted = tuple(member.name for member in Output if member is not Output.PREVIEW)
                return Error((BlenderChoiceFault(path=repr(bound.routing.struct(layer)), identifier=gate.value.name, accepted=accepted),)) if gate.enabled else Ok(Nothing)

    def joined_sources(built: tuple[tuple[Fragment, Source], ...]) -> BlenderResult[tuple[Fragment, tuple[Source, ...]]]:
        return paired(accumulated(gated(source) for _, source in built), Fragment.merged(fragment for fragment, _ in built)).map(
            lambda pair: (pair[1], tuple(source for option in pair[0] for source in option.to_list()))
        )

    def sourced(built: tuple[Fragment, End], name: str, kind: DataType, dimensions: Option[int], output: Output) -> tuple[Fragment, Source]:
        fragment, end = built
        return fragment, Source(name=name, end=end, kind=kind, dimensions=dimensions, output=output)

    def passed(name: str, socket: bpy.types.NodeSocket) -> BlenderResult[tuple[Fragment, Source]]:
        dimensions = Descriptor.resolve(socket, "default_value", bpy.types.FloatProperty).map(lambda member: Some(member.prop.array_length)) if socket.type == "VECTOR" else Ok(Nothing)
        return paired(oriented(name, socket), dimensions).map(lambda pair: sourced(pair[0], name, kinds[socket.type], pair[1], classed(name, socket)))

    def totaled(family: Family) -> BlenderResult[tuple[Fragment, Source]]:
        return (
            term(f"{prefix}_{family.name.lower()}_total", family, ("direct", "indirect", "color"), render, identifiers, bound.lobe)
            .default_with(lambda: Error(missing(family.socket("direct"))))
            .map(lambda built: sourced(built, family.label, "RGBA", Nothing, Output.BEAUTY))
        )

    def derived_of(item: Derived) -> BlenderResult[tuple[tuple[Fragment, Source], ...]]:
        return lit(item).map(lambda found: tuple(sourced(built, item.label, item.kind, Nothing, item.output) for built in found.to_list()))

    def identified(entity: Entity) -> BlenderResult[tuple[tuple[Fragment, Source], ...]]:
        return colored(prefix, scene, layer, entity, named(layer, entity, bound.id_colors[entity])).map(
            lambda found: tuple(sourced(built, f"{entity.label} IDs", "RGBA", Nothing, Output.MATTE) for built in found.to_list())
        )

    passes = accumulated(itertools.starmap(passed, offered.items()))
    totals = accumulated(map(totaled, sorted(routing.totals)))
    derived = accumulated(map(derived_of, sorted(routing.derived)))
    ids = accumulated(map(identified, sorted(routing.ids)))
    return accumulated((passes, totals, derived.map(lambda found: tuple(itertools.chain.from_iterable(found))), ids.map(lambda found: tuple(itertools.chain.from_iterable(found))))).bind(
        lambda parts: joined_sources(tuple(itertools.chain.from_iterable(parts)))
    )


# --- [FILES]


def filings(found: tuple[Source, ...], split: FileSplit) -> tuple[Filing, ...]:
    """Splits layer sources into File Output nodes, one per output class unless `ONE`."""
    match split:
        case FileSplit.ONE:
            return (Filing(output=Output.BEAUTY, kind=Kind.LAYERED, sources=found),) if found else ()
        case FileSplit.CLASSES | FileSplit.PASSES:
            return tuple(
                Filing(output=output, kind=Kind.LAYERED if split is FileSplit.CLASSES else Kind.EXR, sources=held)
                for output in Output
                if output is not Output.PREVIEW
                for held in (tuple(source for source in found if source.output is output),)
                if held
            )


def filed(pathed: Pathed, path: OutputPath, layer: str, filing: Filing, key: Key, convention: Conventions, dimensions: Descriptor[bpy.types.IntProperty]) -> BlenderResult[Fragment]:
    """Builds File Output node of a filing with its items and links."""

    def sized(size: int) -> dict[Descriptor, Value]:
        return {dimensions: size}

    return pathed.node(path, layer, filing.output, tuple(convention.layer(source.name) for source in filing.sources), filing.kind).map(
        lambda paths: Fragment(
            nodes={
                key: Node(
                    kind=bpy.types.CompositorNodeOutputFile,
                    members=paths.members,
                    content=Some(
                        FileItems(items=tuple(FileItem(name=stored, kind=source.kind, members=source.dimensions.map(sized).default_value({})) for source, stored in zip(filing.sources, paths.items)))
                    ),
                )
            },
            links=tuple(Link(source=source.end, target=End(node=key, socket=stored)) for source, stored in zip(filing.sources, paths.items)),
        )
    )


def items(tree: bpy.types.NodeTree, identity: Identity) -> dict[Key, frozenset[str]]:
    """Returns item names of each owned File Output node by key."""
    return {key: frozenset(item.name for item in held.file_output_items) for key, held in owned(tree, identity).items() if isinstance(held, bpy.types.CompositorNodeOutputFile)}


def formatted(tree: bpy.types.NodeTree, group: NodeGroup, plans: dict[str, LayerPlan], rows: Changes, before: dict[Key, frozenset[str]], identity: Identity) -> BlenderResult[None]:
    """Formats File Output nodes reconcile added and items single-layer nodes gained."""
    held, placed = owned(tree, identity), group.graph.placed
    filings = {key: filing for plan in plans.values() for key, filing in plan.filings.items()}
    added = frozenset(row.key for row in rows.structure if isinstance(row, Added))
    nodes = accumulated(OutputFormat.of(filing.output, filing.kind).bind(lambda settings: settings.applied(held[key])) for key, filing in filings.items() if key not in before or key in added)
    kinds = {
        key: {item.name: item.kind for content in placed[key][0].content.to_list() if isinstance(content, FileItems) for item in content.items}
        for key, filing in filings.items()
        if filing.kind is Kind.EXR
    }
    gained = accumulated(
        OutputFormat.typed(stated[item.name]).bind(lambda typed: typed.map(lambda settings: settings.applied(item)).default_value(Ok(None)))
        for key, stated in kinds.items()
        for live in (held[key],)
        if isinstance(live, bpy.types.CompositorNodeOutputFile)
        for item in live.file_output_items
        if item.name not in before.get(key, frozenset())
    )
    return paired(nodes, gained).map(lambda _: None)


def synced(scene: bpy.types.Scene, identity: Identity) -> BlenderResult[None]:
    """Adds AOVs and light groups each used layer lacks, removing an AOV Blender renames or invalidates."""

    def scanned(trees: tuple[bpy.types.NodeTree, ...]) -> BlenderResult[dict[str, str]]:
        pending = deque(trees)
        reached: dict[bpy.types.NodeTree, None] = {}
        while pending:
            if (tree := (pending.popleft())) not in reached:
                reached[tree] = None
                pending.extend(child for held in tree.nodes if isinstance(held, bpy.types.ShaderNodeGroup) for child in Option.of_optional(held.node_tree).to_list())
        return accumulated(
            socket_at(held.inputs, "Color").map(lambda socket: (held.aov_name, "COLOR" if socket.is_linked else "VALUE"))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            for tree in reached
            for held in tree.nodes
            if isinstance(held, bpy.types.ShaderNodeOutputAOV)
        ).map(dict)

    def setup(material: bpy.types.Material) -> BlenderResult[dict[str, str]]:
        return (
            Materials.attachment
            .bound(identity)
            .bind(lambda attached: Option.of_optional(material.node_tree).map(attached.read).default_with(lambda: Error((BlenderAbsentFault(owner=repr(material), key="node_tree"),))))
            .map(lambda shading: {member.name.lower(): member.kind for member in shading.setup.passes} if isinstance(shading.setup, Utility) else {})
        )

    def wanted(layer: bpy.types.ViewLayer) -> BlenderResult[dict[str, str]]:
        override = Option.of_optional(layer.material_override)
        materials = override.map(lambda material: (material,)).default_with(lambda: tuple(slot.material for item in scene.objects for slot in item.material_slots if slot.material))
        trees = tuple(tree for tree in (*(material.node_tree for material in materials), *(world.node_tree for world in Option.of_optional(scene.world).to_list())) if tree)
        return override.filter(lambda material: Utility.ownership.marked(identity, material).is_some()).map(setup).default_with(lambda: scanned(trees))

    def aovs(layer: bpy.types.ViewLayer, names: dict[str, str]) -> BlenderResult[None]:
        held = {aov.name: aov for aov in layer.aovs}

        def synced_aov(name: str, kind: str) -> BlenderResult[None]:
            aov = held.get(name) or layer.aovs.add()
            answer = (
                paired(written(aov, "name", name), written(aov, "type", kind))
                .filter_with(lambda _: aov.name == name and aov.is_valid, lambda _: (BlenderCollisionFault(request=name, error=aov.name),))
                .map(lambda _: None)
            )
            if answer.is_error() and name not in held:
                layer.aovs.remove(aov)
            return answer

        return accumulated(itertools.starmap(synced_aov, names.items())).map(lambda _: None)

    def groups(layer: bpy.types.ViewLayer) -> BlenderResult[None]:
        def added(name: str) -> BlenderResult[None]:
            stored = layer.lightgroups.add(name=name).name
            holders = (*(item for item in scene.objects if item.lightgroup == name), *Option.of_optional(scene.world).filter(lambda world: world.lightgroup == name).to_list())
            return accumulated(written(holder, "lightgroup", stored) for holder in holders if stored != name).map(lambda _: None)

        return accumulated(map(added, sorted(used(scene) - {group.name for group in layer.lightgroups}))).map(lambda _: None)

    def layered(layer: bpy.types.ViewLayer) -> BlenderResult[None]:
        return wanted(layer).bind(lambda names: paired(aovs(layer, names), groups(layer))).map(lambda _: None)

    return accumulated(layered(layer) for layer in scene.view_layers if layer.use).map(lambda _: None)


# --- [IDS]


def named(layer: bpy.types.ViewLayer, entity: Entity, member: Descriptor[bpy.types.FloatProperty]) -> tuple[tuple[str, bpy.types.ID, Rgb], ...]:
    """Returns name, block, and scene-linear color per Cryptomatte name of a layer in name order."""
    shown = tuple(item for item in layer.objects if not item.hide_render and item.data is not None)

    def root(item: bpy.types.Object) -> bpy.types.Object:
        return root(item.parent) if item.parent else item

    blocks: dict[str, bpy.types.ID]
    match entity:
        case Entity.OBJECT:
            blocks = {item.name: item for item in shown}
        case Entity.ASSET:
            blocks = {top.name: top for top in map(root, shown)}
        case Entity.MATERIAL:
            blocks = {held.name: held for held in ((layer.material_override,) if layer.material_override else (slot.material for item in shown for slot in item.material_slots if slot.material))}
    return tuple((name, block, Rgb(red=red, green=green, blue=blue)) for name, block in sorted(blocks.items()) for red, green, blue, *_ in (getattr(block, member.prop.identifier),))


def colored(prefix: str, scene: bpy.types.Scene, layer: bpy.types.ViewLayer, entity: Entity, rows: tuple[tuple[str, bpy.types.ID, Rgb], ...]) -> BlenderResult[Option[tuple[Fragment, End]]]:
    """Builds entity ID image of name colors under mattes summed into alpha, `Nothing` without names."""
    key = f"{prefix}_{entity.name.lower()}"

    def matte(index: int, name: str, color: Rgb) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        held = f"{key}_{index}"
        return (
            node(held, bpy.types.CompositorNodeCryptomatteV2, {}, {"source": "RENDER", "scene": IdName.of(scene), "layer_name": f"{layer.name}.{entity.pass_name}", "matte_id": name})
            .map(ended(held, "Matte"))
            .bind(lambda cover: mixed(f"{held}_tint", "RGBA", 1.0, cover, (color.red, color.green, color.blue, 1.0), "MULTIPLY").map(lambda tint: (tint, cover)))
        )

    def alpha(pairs: tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], ...]) -> BlenderResult[Option[tuple[Fragment, End]]]:
        routes = {"Image": summed(f"{key}_colors", tuple(tint for tint, _ in pairs)), "Alpha": summed(f"{key}_mattes", tuple(cover for _, cover in pairs))}
        return (
            accumulated(routes.values())
            .bind(lambda ends: node(f"{key}_ids", bpy.types.CompositorNodeSetAlpha, dict(zip(routes, ends)) | {"Type": "Replace Alpha"}, {}))
            .map(ended(f"{key}_ids", "Image"))
            .map(Some)
        )

    checked: BlenderResult[tuple[None, ...]] = accumulated(
        Ok(None) if "," not in name and name == name.strip() else Error((BlenderCollisionFault(request=name, error=f"{layer.name}.{entity.pass_name}"),)) for name, _, _ in rows
    )
    return (
        Result[tuple[None, ...], BlenderFaults]
        .bind(checked, lambda _: accumulated(matte(index, name, color) for index, (name, _, color) in enumerate(rows)))
        .bind(lambda pairs: alpha(pairs) if pairs else Ok(Nothing))
    )


def recolored(layer: bpy.types.ViewLayer, entity: Entity, member: Descriptor[bpy.types.FloatProperty], chosen: Callable[[tuple[Rgb, ...]], tuple[Rgb, ...]]) -> BlenderResult[int]:
    """Writes chosen colors onto entity names, each block keeping its alpha."""
    rows = named(layer, entity, member)
    return accumulated(
        member.write(block, (color.red, color.green, color.blue, getattr(block, member.prop.identifier)[3])) for (_, block, _), color in zip(rows, chosen(tuple(color for _, _, color in rows)))
    ).map(lambda priors: sum(prior.is_some() for prior in priors))


def exported(scene: bpy.types.Scene, file: Written, identity: Identity) -> BlenderResult[None]:
    """Writes one ID layer file per matte of a written multilayer file, refusing a path equal to source."""
    frame = scene.frame_current

    def stored(matte: Matte, known: dict[str, Rgb], target: str, _: tuple[None, ...]) -> BlenderResult[str]:
        return multilayer.written(
            target,
            tuple(multilayer.Pass(layer=name, channels=("R", "G", "B", "A"), role=multilayer.Role.COLOR) for name in matte.ids),
            multilayer.Layout.PARTS,
            Storage(depth=16, compression=Some(Deflate())),
            (matte.file.parts[0].width, matte.file.parts[0].height),
            stamped(identity),
            lambda held: matte.filled(frozenset({held.layer}), known[held.layer]),
        ).map_error(BlenderImagingFault.lifted)

    def painted(members: dict[Entity, Descriptor[bpy.types.FloatProperty]], path: OutputPath, matte: Matte, layer: bpy.types.ViewLayer, entity: Entity) -> BlenderResult[str]:
        known = {name: color for name, _, color in named(layer, entity, members[entity])}
        ready: BlenderResult[tuple[str, tuple[None, ...]]] = paired(
            path
            .written(file.layer, Output.MATTE, Some(entity.pass_name), frame, ".exr")
            .map(str)
            .bind(lambda target: Error((BlenderCollisionFault(request=target, error=file.path),)) if target == file.path else Ok(target)),
            accumulated(Ok(None) if name in known else Error((BlenderAbsentFault(owner=repr(layer.objects), key=name),)) for name in matte.ids),
        )
        return Result[tuple[str, tuple[None, ...]], BlenderFaults].bind(ready, lambda found: stored(matte, known, *found))

    def located(members: dict[Entity, Descriptor[bpy.types.FloatProperty]], path: OutputPath, matte: Matte) -> BlenderResult[str]:
        found: BlenderResult[tuple[bpy.types.ViewLayer, Entity]] = paired(
            Option.of_optional(scene.view_layers.get(file.layer)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.view_layers), key=file.layer),)),
            Entity.named(matte.name.removeprefix(f"{file.layer}.")).to_result_with(lambda: (BlenderAbsentFault(owner=file.path, key=matte.name),)),
        )
        return Result[tuple[bpy.types.ViewLayer, Entity], BlenderFaults].bind(found, lambda pair: painted(members, path, matte, *pair))

    def wrote(routing: PassRouting, path: OutputPath, mattes: tuple[Matte, ...]) -> BlenderResult[tuple[str, ...]]:
        return accumulated(located(routing.id_colors, path, matte) for matte in mattes)

    return (
        PassSet()
        .bound(identity)
        .bind(
            lambda routing: Result[tuple[OutputPath, tuple[Matte, ...]], BlenderFaults].bind(
                paired(routing.pathed.path(scene, datetime.datetime.now(datetime.UTC).astimezone().date()), Multilayer.read(file.path).bind(Matte.found).map_error(BlenderImagingFault.lifted)),
                lambda found: wrote(routing, *found),
            )
        )
        .map(lambda _: None)
    )


def unfiled(identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[tuple[str, str, str], ...]]:
    """Returns file row search items for context layer sources no file row holds."""
    layer, scene = context.view_layer, context.scene
    match layer, scene:
        case bpy.types.ViewLayer(), bpy.types.Scene():
            pass
        case _:
            first, *rest = tuple(BlenderAbsentFault(owner=repr(context), key=key) for key, value in (("view_layer", layer), ("scene", scene)) if value is None)
            return Error((first, *rest))

    def listed(layer: bpy.types.ViewLayer, record: Routing, _: NodeGroup, plans: dict[str, LayerPlan]) -> tuple[tuple[str, str, str], ...]:
        taken = {row.name for row in record.files}
        return tuple(
            (source.name, source.name, f"{layer.name} pass {source.name}")
            for plan in Option.of_optional(plans.get(layer.name)).to_list()
            for filing in plan.filings.values()
            for source in filing.sources
            if source.name not in taken
        )

    def searched(routing: PassRouting, record: Routing, path: OutputPath) -> BlenderResult[tuple[tuple[str, str, str], ...]]:
        return routing.planned(scene, routing.held(scene), path).map(lambda planned: listed(layer, record, *planned))

    return (
        PassSet()
        .bound(identity)
        .bind(lambda routing: paired(routing.routing.read(layer), routing.pathed.path(scene, datetime.datetime.now(datetime.UTC).astimezone().date())).bind(lambda found: searched(routing, *found)))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

ids_export = Finish(run=exported, kinds=frozenset({Kind.LAYERED}))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Derived",
    "Entity",
    "Family",
    "FamilyFlags",
    "FileSplit",
    "Filing",
    "LayerPlan",
    "PassFile",
    "PassFlags",
    "PassName",
    "PassPreset",
    "PassRequest",
    "PassRouting",
    "PassSet",
    "Routing",
    "Source",
    "colored",
    "exported",
    "filed",
    "filings",
    "formatted",
    "ids_export",
    "items",
    "lobe",
    "named",
    "offered",
    "pass_flags",
    "recolored",
    "reordered",
    "required",
    "sources",
    "synced",
    "term",
    "turned",
    "unfiled",
]
