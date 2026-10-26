"""View layer pass routing into File Output nodes with derived layers and ID colors."""

from collections.abc import Callable, Mapping
import datetime
import functools
import itertools
from typing import Annotated, ClassVar, Final, Literal, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Map
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.operators import Call, Done, Empty, InvokePopup, Operator, Outcome, tallied, Unchanged
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.conditions import AllOf, AnyOf, Condition, holds, Not, Setting
from blender.bpy.data.rna.descriptors import Descriptor, rooted, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import LOAD_POST, PathHandler, SAVE_POST, staged
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderCollisionFault, BlenderFaults, BlenderResult, BlenderUnmetFault
from blender.bpy.nodes.changes import Added, Changes, changes, made, owned, reconciled
from blender.bpy.nodes.graphs import DataType, Expr, FileItem, FileItems, FrameNode, group_input, Link, lowered, NodeGroup, rgba, Term, xyz
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.nodes.trees import nested, socket_at
from blender.bpy.render.outputs.finishing import Finish, stamped, Written
from blender.bpy.render.outputs.formats import extension, Kind, OutputFormat
from blender.bpy.render.outputs.paths import NodePaths, OutputPath, Pathed, ScenePaths
from blender.bpy.render.view_layers.overrides import colors, encoded, LayerOverride, LayerTag, Role, Spectrum
from blender.bpy.scene.lights.groups import used
from blender.bpy.scene.materials import DataPass, Materials, Utility
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.history import Retention
from blender.bpy.settings.presets import Preset
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.rows import Caption, Flow, joined, Label, Line, Member, OperatorButton, placed, Property, Row, RowDraw
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.values import Hosted, Listing, Store
from blender.imaging.files import multilayer
from blender.imaging.files.cryptomatte import Matte
from blender.imaging.files.documents import Document, Group, Raster
from blender.imaging.files.images import Deflate, Depth, Rounded, Storage, unassociated
from blender.imaging.files.multilayer import Multilayer
from blender.records.faults import accumulated, paired
from blender.records.fields import Choice, Gated, heading, Rgb
from blender.records.identity import content_hash, rng
from blender.records.trees import Owner, transfer, Tree
from blender.rendering.outputs.conventions import Axis, Blender, Conventions, Space
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type ItemKind = Literal["FLOAT", "RGBA"]


class Family(Choice):
    """Light pass family of direct, indirect, and color passes, each a term recombined beauty sums under its own gain."""

    prefix: str
    direct: tuple[str, ...]
    indirect: tuple[str, ...]
    color: tuple[str, ...]

    def __init__(self, value: int, label: str, prefix: str, direct: tuple[str, ...], indirect: tuple[str, ...], color: tuple[str, ...]) -> None:
        """Sets member label, socket prefix, and flag paths per slot."""
        super().__init__(value, label)
        self.prefix, self.direct, self.indirect, self.color = prefix, direct, indirect, color

    def socket(self, slot: Key) -> str:
        """Returns Render Layers socket name of a family pass slot."""
        return f"{self.prefix} {heading(slot)}"

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


class PassOutput(Choice):
    """Output class a pass file row routes its pass to, every class but the preview."""

    BEAUTY = 1
    DATA = 2
    MATTE = 3

    @property
    def output(self) -> Output:
        """Output class of the same name."""
        return Output[self.name]


class Entity(Choice):
    """Cryptomatte kind with its pass name, enabling flag, and block color member."""

    pass_name: str
    flag: Descriptor[bpy.types.BoolProperty]
    color: Descriptor[bpy.types.FloatProperty]

    def __init__(self, value: int, label: str, pass_name: str, flag: str, owner: "type[bpy.types.bpy_struct[object]]", color: str) -> None:
        """Sets member label, Blender pass name, and the flag and color members of its blocks."""
        super().__init__(value, label)
        self.pass_name = pass_name
        self.flag = Descriptor.declared(bpy.types.ViewLayer, flag, bpy.types.BoolProperty)
        self.color = Descriptor.declared(owner, color, bpy.types.FloatProperty)

    OBJECT = 1, "Object", "CryptoObject", "use_pass_cryptomatte_object", bpy.types.Object, "color"
    MATERIAL = 2, "Material", "CryptoMaterial", "use_pass_cryptomatte_material", bpy.types.Material, "diffuse_color"
    ASSET = 3, "Asset", "CryptoAsset", "use_pass_cryptomatte_asset", bpy.types.Object, "color"

    @classmethod
    def named(cls, name: str) -> Option[Self]:
        """Returns kind with pass name `name`, `Nothing` for another."""
        return Option.of_optional(next((member for member in cls if member.pass_name == name), None))


class FileSplit(Choice):
    """Split of layer passes into files."""

    ONE = 1, "One File"
    CLASSES = 2, "File per Class"
    PASSES = 3, "File per Pass"


# --- [CONSTANTS] ------------------------------------------------------------------------

_DEPTH: Final = Descriptor.declared(bpy.types.ViewLayer, "use_pass_z", bpy.types.BoolProperty)
_DEFAULT: Final = Descriptor.declared(bpy.types.NodeTreeInterfaceSocketColor, "default_value", bpy.types.FloatProperty)
_FILM: Final = Descriptor.declared(bpy.types.Scene, "render.film_transparent", bpy.types.BoolProperty)
_ENGINE: Final = Descriptor.declared(bpy.types.Scene, "render.engine", bpy.types.EnumProperty)
_TRANSPARENT: Final = Descriptor.declared(bpy.types.ViewLayer, "eevee.use_pass_transparent", bpy.types.BoolProperty)
_KINDS: Final[Mapping[str, DataType]] = {"VALUE": "FLOAT", "VECTOR": "VECTOR", "RGBA": "RGBA"}
_WATCHED: Final = tuple(
    (owner, Descriptor.declared(owner, member, bpy.types.Property).prop)
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

# --- [CHOICES] --------------------------------------------------------------------------

offered = Choices(
    lambda identity, _owner, context: (
        Option.of_optional(context).map(lambda held: unfiled(identity, held).default_with(lambda faults: tuple(("", fault.message, "") for fault in faults))).default_value(())
    )
)


# --- [MODELS] ---------------------------------------------------------------------------


class PassFlags(msgspec.Struct, frozen=True, kw_only=True):
    """Pass flags of running build, resolved once."""

    every: "tuple[tuple[type[bpy.types.bpy_struct[object]], Descriptor[bpy.types.BoolProperty]], ...]"
    families: Mapping[Family, tuple[frozenset[Descriptor[bpy.types.BoolProperty]], frozenset[Descriptor[bpy.types.BoolProperty]], frozenset[Descriptor[bpy.types.BoolProperty]]]]

    @classmethod
    def of(cls) -> BlenderResult[Self]:
        """Resolves every pass flag and each family's direct, indirect, and color flags from its declared paths, faulting each path the build lacks."""

        def slot(paths: tuple[str, ...]) -> BlenderResult[frozenset[Descriptor[bpy.types.BoolProperty]]]:
            return accumulated(Descriptor.resolve(bpy.types.ViewLayer, path, bpy.types.BoolProperty) for path in paths).map(frozenset)

        return accumulated(paired(slot(family.direct), slot(family.indirect), slot(family.color)) for family in Family).map(
            lambda families: cls(every=pass_flags(), families=dict(zip(Family, families)))
        )

    @property
    def lobes(self) -> frozenset[Descriptor[bpy.types.BoolProperty]]:
        """Every family flag."""
        return frozenset().union(*(slot for slots in self.families.values() for slot in slots))

    def opened(self, routing: "Routing") -> frozenset[Descriptor[bpy.types.BoolProperty]]:
        """Returns flags a routing reads."""

        def lit(item: Derived) -> tuple[frozenset[Descriptor[bpy.types.BoolProperty]], ...]:
            match item:
                case Derived.DIRECT:
                    return tuple(direct | color for direct, _, color in self.families.values())
                case Derived.INDIRECT:
                    return tuple(indirect | color for _, indirect, color in self.families.values())
                case Derived.DEPTH | Derived.INVERSE:
                    return ()

        return frozenset().union(
            *(slot for family in routing.totals for slot in self.families[family]), *(held for item in routing.derived for held in lit(item)), (entity.flag for entity in routing.ids)
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
        return Ok(cls(layer=layer, flags=frozenset({_DEPTH}), precise=precise))


class PassFile(msgspec.Struct, frozen=True, kw_only=True):
    """File choice of one source."""

    name: Annotated[str, msgspec.Meta(min_length=1, title="Pass", description="Pass or derived layer name")]
    output: Annotated[Gated[PassOutput], msgspec.Meta(title="File", description="Output class file holding pass, skipped when off")] = Gated(enabled=True, value=PassOutput.BEAUTY)


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


class FileSocket(msgspec.Struct, frozen=True, kw_only=True):
    """Socket a layer's files write."""

    name: str
    term: Expr
    kind: DataType
    dimensions: Option[int]
    output: Output


class Filing(msgspec.Struct, frozen=True, kw_only=True):
    """Sources, class, and kind of one File Output node."""

    output: Output
    kind: Kind
    sources: tuple[FileSocket, ...]


class LayerPlan(msgspec.Struct, frozen=True, kw_only=True):
    """Frame key, filing per File Output key, and file reader convention of one used view layer."""

    frame: Key
    filings: dict[Key, Filing]
    convention: Conventions


class PassRouting(msgspec.Struct, frozen=True, kw_only=True):
    """`PassSet` bound to an identity, with the members its plans write."""

    use: ClassVar[Descriptor[bpy.types.BoolProperty]] = Descriptor.declared(bpy.types.ViewLayer, "use", bpy.types.BoolProperty)
    precision: ClassVar[Descriptor[bpy.types.EnumProperty]] = Descriptor.declared(bpy.types.Scene, "render.compositor_precision", bpy.types.EnumProperty)
    dimensions: ClassVar[Descriptor[bpy.types.IntProperty]] = Descriptor.declared(bpy.types.NodeCompositorFileOutputItem, "vector_socket_dimensions", bpy.types.IntProperty)
    label: ClassVar[Descriptor[bpy.types.StringProperty]] = Descriptor.declared(bpy.types.NodeFrame, "label", bpy.types.StringProperty)
    custom: ClassVar[Descriptor[bpy.types.BoolProperty]] = Descriptor.declared(bpy.types.NodeFrame, "use_custom_color", bpy.types.BoolProperty)
    tint: ClassVar[Descriptor[bpy.types.FloatProperty]] = Descriptor.declared(bpy.types.NodeFrame, "color", bpy.types.FloatProperty)
    identity: Identity
    routing: Attached[Routing]
    pathed: Pathed
    layers: Attached[LayerTag]
    flags: PassFlags
    lobe: NodeGroup

    def used(self, scene: bpy.types.Scene) -> tuple[bpy.types.ViewLayer, ...]:
        """Returns scene layers with `use` on."""
        return tuple(layer for layer in scene.view_layers if bool(self.use.read(layer)))

    def path(self, scene: bpy.types.Scene) -> BlenderResult[OutputPath]:
        """Returns scene output path on the local calendar date."""
        return self.pathed.path(scene, datetime.datetime.now(datetime.UTC).astimezone().date())

    def grouped(self, frames: tuple[tuple[FrameNode, tuple[Link, ...], LayerPlan], ...]) -> BlenderResult[NodeGroup]:
        """Builds routing group around image pass-through with each layer frame."""
        interface = Interface(outputs=(Socket(key="result", name="Image", kind=bpy.types.NodeSocketColor),), inputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),))
        return NodeGroup.built(PassSet.ownership.key, PassSet.title, interface, {"result": group_input("image", "RGBA")}).map(
            lambda group: msgspec.structs.replace(
                group,
                graph=msgspec.structs.replace(
                    group.graph,
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
        rendered = Expr(kind=bpy.types.CompositorNodeRLayers, members=(("scene", IdName.of(scene)), ("layer", layer.name)), label=Some(f"{prefix}_render"))

        def filed_all(routing: Routing, record: LayerTag, found: tuple[FileSocket, ...]) -> BlenderResult[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
            plan = {f"{prefix}_{filing.output.name.lower()}_{filing.kind.name.lower()}": filing for filing in filings(found, routing.split)}
            return (
                accumulated(filed(self.pathed, path, layer.name, filing, key, routing.convention, self.dimensions) for key, filing in plan.items())
                .bind(lambda files: lowered(rendered, *files))
                .map(
                    lambda merged: (
                        FrameNode(members={self.label: layer.name, self.custom: True, self.tint: encoded(record.color)}, nodes=merged.nodes),
                        merged.links,
                        LayerPlan(frame=prefix, filings=plan, convention=routing.convention),
                    )
                )
            )

        def routed(routing: Routing, record: LayerTag) -> BlenderResult[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
            return (
                Option
                .of_optional(held.get(f"{prefix}_render"))
                .map(lambda live: sources(scene, layer, rendered, live, routing, record.role, self.lobe))
                .default_value(Ok(()))
                .bind(lambda found: filed_all(routing, record, found))
            )

        return paired(self.routing.read(layer), self.layers.read(layer)).bind(lambda pair: routed(*pair))

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

    def refreshed(self, context: bpy.types.Context) -> BlenderResult[None]:
        """Reconciles every editable scene with stored layer routing, owned runtime scenes skipped."""

        def refresh(scene: bpy.types.Scene) -> BlenderResult[Changes]:
            return self.path(scene).bind(lambda path: self.reconciled(scene, context, path))

        return rooted(bpy.types.BlendData, context).bind(
            lambda data: accumulated(
                refresh(scene) for scene in data.scenes if scene.is_editable and not staged(self.identity, scene) and any(self.routing.stored(layer) for layer in scene.view_layers)
            ).map(lambda _: None)
        )

    def stale(self, context: bpy.types.Context) -> BlenderResult[Option[str]]:
        """Returns pending routing group writes for context scene, `Nothing` for a current tree."""

        def pending(tree: bpy.types.NodeTree, group: NodeGroup, _: dict[str, LayerPlan]) -> BlenderResult[Option[str]]:
            return changes(group, tree, self.identity).map(lambda rows: sum(map(len, msgspec.structs.astuple(rows)))).map(lambda count: Some(f"{count} changes pending") if count else Nothing)

        def scened(scene: bpy.types.Scene) -> BlenderResult[Option[str]]:
            return self.path(scene).bind(
                lambda path: (
                    self.held(scene).map(lambda tree: self.planned(scene, Some(tree), path).bind(lambda planned: pending(tree, *planned))).default_value(Ok(Some("Update builds routing group")))
                )
            )

        return Option.of_optional(context.scene).map(scened).default_value(Ok(Nothing))

    def watched(self) -> tuple[Subscription, ...]:
        """Builds subscriptions refreshing routing, one per pass flag, watched setting, and runtime attachment member."""
        groups = tuple((attached.group.struct, prop) for attached in (self.routing, self.pathed.attached, self.layers) for prop in attached.group.struct.bl_rna.properties if prop.is_runtime)
        return tuple(
            Subscription(owner=owner, setting=prop, body=lambda: self.refreshed(bpy.context)) for owner, prop in (*((owner, flag.prop) for owner, flag in self.flags.every), *_WATCHED, *groups)
        )

    def listing(self) -> Listing[PassPreset]:
        """Returns pass presets over layer flags and routing, applied to every used layer."""
        selection = Some(tuple(flag for _, flag in self.flags.every))
        eevee = frozenset().union(
            *(direct | color for family in (Family.DIFFUSE, Family.GLOSSY) for direct, _, color in (self.flags.families[family],)),
            frozenset(flag for flag in self.flags.families[Family.VOLUME][0] if flag.owner == "eevee"),
        )
        common = frozenset({"use_pass_emit", "use_pass_environment"})
        family_paths = frozenset(flag.path for flag in self.flags.lobes)
        eevee_paths = frozenset(flag.path for flag in eevee)
        recipes = (
            ("lightray_rebuild", common | family_paths, Routing(derived=frozenset({Derived.DIRECT, Derived.INDIRECT}))),
            ("material_rebuild", common | family_paths, Routing(totals=frozenset(Family))),
            ("multipass_breakdown", common | family_paths, Routing()),
            ("eevee_material_rebuild", common | eevee_paths, Routing(totals=frozenset({Family.DIFFUSE, Family.GLOSSY, Family.VOLUME}))),
            ("eevee_multipass", common | eevee_paths | {"eevee.use_pass_transparent", "use_pass_shadow", "use_pass_ambient_occlusion"}, Routing()),
        )
        owners = tuple(dict.fromkeys(flag.owner for _, flag in self.flags.every))

        def stock(enabled: frozenset[str], routing: Routing) -> PassPreset:
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
    prerequisites: ClassVar[tuple[Registrable, ...]] = (attachment,)
    presets: ClassVar[Preset[PassPreset]] = Preset(key="passes", form=PassPreset)
    ownership: ClassVar[Ownership[bpy.types.CompositorNodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key="passes", saved=Saved.USED)
    title: ClassVar[str] = "Passes"

    def bound(self, identity: Identity) -> BlenderResult[PassRouting]:
        """Binds attachments and resolves flags and lobe group."""
        return paired(self.attachment.bound(identity), ScenePaths().bound(identity), LayerOverride.layer.bound(identity), PassFlags.of(), lobe()).map(
            lambda parts: PassRouting(identity=identity, routing=parts[0], pathed=parts[1], layers=parts[2], flags=parts[3], lobe=parts[4])
        )

    def written(self, scene: bpy.types.Scene, frame: int, identity: Identity, *, path: Option[OutputPath] = Nothing) -> BlenderResult[tuple[Written, ...]]:
        """Returns files scene File Output nodes wrote for a frame at `path` or scene path, the `Files` port."""

        def files(routing: PassRouting, path: OutputPath, layer: str, plan: LayerPlan, live: bpy.types.CompositorNodeOutputFile, filing: Filing) -> BlenderResult[tuple[Written, ...]]:
            names = tuple(plan.convention.layer(source.name) for source in filing.sources)

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
                return path.written(layer, filing.output, item, frame, suffix).map(lambda file: Written.of(str(file), settings, filing.output, plan.convention, layer, passes))

            return (
                routing.pathed
                .node(path, layer, filing.output, names, filing.kind)
                .bind(lambda paths: accumulated(filed_at(settings, item, passes, suffix) for settings, item, passes in targeted(paths) for suffix in extension(settings).to_list()))
                .map(lambda found: tuple(file for option in found for file in option.to_list()))
            )

        def collected(routing: PassRouting, path: OutputPath) -> BlenderResult[tuple[Written, ...]]:
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

        return self.bound(identity).bind(lambda routing: path.map(Ok).default_with(lambda: routing.path(scene)).bind(lambda chosen: collected(routing, chosen)))

    def document(self, _identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> BlenderResult[Document[np.uint8 | np.uint16 | np.float32]]:
        """Builds ID document of a written frame with one group per matte, the `Documents` port."""
        size = (frame.parts[0].height, frame.parts[0].width)
        coded = functools.partial(depth.coded, coding=Rounded())

        def grouped(index: int, matte: Matte) -> BlenderResult[tuple[Group[np.uint8 | np.uint16 | np.float32], npt.NDArray[np.float32]]]:
            def cover(name: str, color: Rgb) -> BlenderResult[tuple[str, Rgb, npt.NDArray[np.float32]]]:
                return matte.coverage(frozenset({name})).map(lambda plane: (name, color, plane))

            def covered(layer: bpy.types.ViewLayer, entity: Entity) -> BlenderResult[tuple[tuple[str, Rgb, npt.NDArray[np.float32]], ...]]:
                return accumulated(cover(name, color) for name, _, color in named(layer, entity))

            return located(scene, matte, matte.name.rpartition(".")[0]).bind(
                lambda pair: covered(*pair).map(
                    lambda rows: (
                        Group(
                            name=matte.name,
                            visible=index == 0,
                            layers=tuple(Raster(name=name, color=coded(np.broadcast_to(np.asarray(encoded(color), dtype=np.float32), (*size, 3))), alpha=coded(plane)) for name, color, plane in rows),
                        ),
                        functools.reduce(np.add, (plane[..., None] * np.asarray((*encoded(color), 1.0), dtype=np.float32) for _, color, plane in rows), np.zeros((*size, 4), np.float32)),
                    )
                )
            )

        return Result[tuple[Matte, ...], BlenderFaults].bind(
            Matte.found(frame),
            lambda mattes: accumulated(itertools.starmap(grouped, enumerate(mattes))).map(
                lambda groups: Document(
                    layers=tuple(group for group, _ in groups),
                    composite=coded(unassociated(next((filled for _, filled in groups), np.zeros((*size, 4), np.float32)))),
                    colorspace="sRGB",
                    density=scene.render.ppm_factor / scene.render.ppm_base,
                )
            ),
        )

    def drawn(self, identity: Identity) -> BlenderResult[RowDraw]:
        """Draws film transparency, pending changes, operator, and preset rows."""

        def rows(routing: PassRouting, catcher: Descriptor[bpy.types.BoolProperty]) -> tuple[Row, ...]:
            opaque = AllOf(
                conditions=tuple(
                    AnyOf(conditions=(Setting(descriptor=flag, accepted=(False,)), Not(condition=Setting(descriptor=_ENGINE, accepted=(running,)))))
                    for flag, running in ((catcher, "CYCLES"), (_TRANSPARENT, "BLENDER_EEVEE"))
                )
            )
            return (
                Property(target=Member(root=bpy.types.Scene, path="render.film_transparent"), condition=Some(AnyOf(conditions=(Setting(descriptor=_FILM), opaque)))),
                Label(text=lambda context: routing.stale(context).default_with(lambda faults: Some("\n".join(fault.message for fault in faults)))),
                Caption(rows=tuple(OperatorButton(target=command) for command in COMMANDS), flow=Flow.GRID),
                routing.listing(),
            )

        return (
            paired(self.bound(identity), Descriptor.resolve(bpy.types.ViewLayer, "cycles.use_pass_shadow_catcher", bpy.types.BoolProperty)).bind(lambda pair: placed(rows(*pair), identity)).map(joined)
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns routing acquisitions."""
        return self.bound(identity).map(
            lambda routing: (
                self.ownership,
                routing.lobe,
                *COMMANDS,
                routing.listing(),
                *(PathHandler(handlers=handlers, body=lambda _: routing.refreshed(bpy.context)) for handlers in (LOAD_POST, SAVE_POST)),
                *routing.watched(),
            )
        )

    def section(self, retention: Option[Callable[[Identity], BlenderResult[Retention]]] = Nothing) -> Single[Routing]:
        """Returns File Output section over context layer routing store."""
        return Single(key="passes", title="File Output", rows=(self,), store=Some(Hosted(store=Store(attachment=self.attachment, retention=retention))))


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

    def met(request: PassRequest) -> BlenderResult[None]:
        gate: BlenderResult[None] = (
            request.condition.filter(lambda condition: not holds(condition, context, Some(scene))).map(lambda condition: Error((BlenderUnmetFault(requirement=condition),))).default_value(Ok(None))
        )
        return (
            gate
            .bind(lambda _: Option.of_optional(scene.view_layers.get(request.layer)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.view_layers), key=request.layer),)))
            .bind(
                lambda layer: accumulated((
                    *(written(layer, flag.path, value=True) for flag in sorted(request.flags, key=lambda flag: flag.path)),
                    *((written(scene, precision.path, "FULL"),) if request.precise else ()),
                ))
            )
            .map(lambda _: None)
        )

    return accumulated(map(met, requests)).map(lambda _: None)


# --- [SOURCES]


def lobe() -> BlenderResult[NodeGroup]:
    """Builds shared lobe total group, `(direct + indirect) * color`."""
    interface = Interface(
        outputs=(Socket(key="total", name="Total", kind=bpy.types.NodeSocketColor),),
        inputs=tuple(
            Socket(key=key, name=heading(key), kind=bpy.types.NodeSocketColor, members={_DEFAULT: value})
            for key, value in (("direct", (0.0, 0.0, 0.0, 1.0)), ("indirect", (0.0, 0.0, 0.0, 1.0)), ("color", (1.0, 1.0, 1.0, 1.0)))
        ),
    )
    return NodeGroup.built("lobe_total", "Lobe Total", interface, {"total": (group_input("direct", "RGBA") + group_input("indirect", "RGBA")) * group_input("color", "RGBA")})


def lobed(family: Family, slots: tuple[Key, *tuple[Key, ...]], offered: Mapping[str, Term], group: NodeGroup) -> Option[Expr]:
    """Places lobe group over the offered slot sockets of one family, `Nothing` without its first slot socket."""
    linked = {slot: offered[name] for slot in slots if (name := family.socket(slot)) in offered}
    return Some(group.placed(linked, "total")) if slots[0] in linked else Nothing


def turned(vector: Expr, axes: tuple[Axis, Axis, Axis], camera: Option[bpy.types.Object]) -> Expr:
    """Converts vector to reader axes, in camera space when given a camera."""
    rotated = camera.map(
        lambda held: Expr(
            kind=bpy.types.FunctionNodeRotateVector,
            inputs=(
                ("Vector", vector),
                (
                    "Rotation",
                    Expr(
                        kind=bpy.types.FunctionNodeInvertRotation,
                        inputs=(("Rotation", Expr(kind=bpy.types.GeometryNodeObjectInfo, inputs=(("Object", IdName.of(held)),), socket="Rotation")),),
                        socket="Rotation",
                    ),
                ),
            ),
            socket="Vector",
            shape="VECTOR",
        )
    ).default_value(vector)
    names: Mapping[int, Literal["X", "Y", "Z"]] = {1: "X", 2: "Y", 3: "Z"}
    return rotated if axes == (1, 2, 3) else xyz(*(-rotated[names[-axis]] if axis < 0 else rotated[names[axis]] for axis in axes))


def reordered(vector: Expr, order: tuple[int, int, int, int]) -> Expr:
    """Reorders motion vector channels to reader order."""
    channels: tuple[Literal["Red", "Green", "Blue", "Alpha"], ...] = ("Red", "Green", "Blue", "Alpha")
    red, green, blue, alpha = (vector[channels[source - 1]] for source in order)
    return rgba(red, green, blue, alpha)


def sources(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, rendered: Expr, live: bpy.types.Node, routing: Routing, role: Role, group: NodeGroup) -> BlenderResult[tuple[FileSocket, ...]]:
    """Returns sources layer files write in socket order, derived layers and ID images after, each under its file row's gate."""
    convention = routing.convention
    offered = {socket.name: socket for socket in live.outputs if socket.enabled and socket.name != "Alpha" and socket.type in _KINDS}
    outputs = {name: msgspec.structs.replace(rendered, socket=socket.identifier, shape=_KINDS[socket.type]) for name, socket in offered.items()}
    spaces = {"Normal": convention.normals, "Position": Space.WORLD, DataPass.POSITION.name.lower(): Space.WORLD, DataPass.REFERENCE.name.lower(): Space.WORLD}
    gates = {row.name: row.output for row in routing.files}

    def missing(name: str) -> BlenderFaults:
        return (BlenderAbsentFault(owner=repr(live.outputs), key=name),)

    def sourced(expr: Expr, name: str, kind: DataType, output: Output, dimensions: Option[int] = Nothing) -> BlenderResult[tuple[FileSocket, ...]]:
        match gates.get(name):
            case None:
                return Ok((FileSocket(name=name, term=expr, kind=kind, dimensions=dimensions, output=output),))
            case Gated(enabled=True, value=held):
                return Ok((FileSocket(name=name, term=expr, kind=kind, dimensions=dimensions, output=held.output),))
            case _:
                return Ok(())

    def oriented(name: str) -> BlenderResult[Expr]:
        read = outputs[name]
        match spaces.get(name):
            case Space.CAMERA:
                return (
                    Option
                    .of_optional(scene.camera)
                    .to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key="camera"),))
                    .map(lambda held: turned(read, convention.axes[Space.CAMERA], Some(held)))
                )
            case Space() as space:
                return Ok(turned(read, convention.axes[space], Nothing))
            case None if name == "Vector" and convention.motion != (1, 2, 3, 4):
                return Ok(reordered(read, convention.motion))
            case None:
                return Ok(read)

    def passed(name: str, socket: bpy.types.NodeSocket) -> BlenderResult[tuple[FileSocket, ...]]:
        dimensions = Descriptor.resolve(socket, "default_value", bpy.types.FloatProperty).map(lambda member: Some(member.prop.array_length)) if socket.type == "VECTOR" else Ok(Nothing)
        classed = (
            Output.MATTE
            if any(name.startswith(entity.pass_name) and name.removeprefix(entity.pass_name).isdigit() for entity in Entity)
            else Output.DATA
            if role is Role.DATA or socket.type != "RGBA"
            else Output.BEAUTY
        )
        return paired(oriented(name), dimensions).bind(lambda pair: sourced(pair[0], name, _KINDS[socket.type], classed, pair[1]))

    def totaled(family: Family) -> BlenderResult[tuple[FileSocket, ...]]:
        return (
            lobed(family, ("direct", "indirect", "color"), outputs, group)
            .to_result_with(lambda: missing(family.socket("direct")))
            .bind(lambda total: sourced(total, family.label, "RGBA", Output.BEAUTY))
        )

    def derived(item: Derived) -> BlenderResult[tuple[FileSocket, ...]]:
        depth = Option.of_optional(outputs.get(DataPass.DEPTH.name.lower())).to_result_with(lambda: missing(DataPass.DEPTH.name.lower()))
        match item:
            case Derived.DIRECT | Derived.INDIRECT:
                match tuple(found for family in Family for found in lobed(family, (item.name.lower(), "color"), outputs, group).to_list()):
                    case (first, *rest):
                        return sourced(sum(rest, first), item.label, item.kind, item.output)
                    case _:
                        return Ok(())
            case Derived.DEPTH:
                return depth.bind(lambda read: sourced(Expr(kind=bpy.types.CompositorNodeNormalize, inputs=(("Value", read),)), item.label, item.kind, item.output))
            case Derived.INVERSE:
                return depth.bind(lambda read: sourced(1.0 / read, item.label, item.kind, item.output))

    def identified(entity: Entity) -> BlenderResult[tuple[FileSocket, ...]]:
        return colored(scene, layer, entity, named(layer, entity)).bind(lambda found: found.map(lambda image: sourced(image, f"{entity.label} IDs", "RGBA", Output.MATTE)).default_with(lambda: Ok(())))

    return accumulated((*itertools.starmap(passed, offered.items()), *map(totaled, sorted(routing.totals)), *map(derived, sorted(routing.derived)), *map(identified, sorted(routing.ids)))).map(
        lambda parts: tuple(source for part in parts for source in part)
    )


# --- [COMMANDS]


def updated(call: Call[Empty]) -> BlenderResult[Done]:
    """Reconciles the context scene's pass routing and File Output nodes."""
    return (
        paired(PassSet().bound(call.identity), call.root(bpy.types.Scene))
        .bind(lambda pair: pair[0].path(pair[1]).bind(lambda path: pair[0].reconciled(pair[1], call.context, path)))
        .map(lambda _: Done())
    )


def recoloring(chosen: Callable[[tuple[Rgb, ...]], tuple[Rgb, ...]]) -> Callable[[Call[Empty]], BlenderResult[Outcome]]:
    """Returns the body writing chosen colors onto the names of each Cryptomatte kind the active layer draws as ID colors."""

    def painted(layer: bpy.types.ViewLayer, routing: Routing) -> BlenderResult[Outcome]:
        if not routing.ids:
            return Ok(Unchanged(reason=f"{layer.name} draws no ID colors"))
        return accumulated(recolored(layer, entity, chosen) for entity in sorted(routing.ids)).map(
            lambda counts: Done() if sum(counts) else Unchanged(reason=f"{layer.name} ID colors hold chosen colors")
        )

    return lambda call: Result[bpy.types.ViewLayer, BlenderFaults].bind(
        call.root(bpy.types.ViewLayer), lambda layer: PassSet.attachment.read(call.identity, layer).bind(lambda routing: painted(layer, routing))
    )


def added(call: Call[PassName]) -> BlenderResult[Done]:
    """Adds a file row for the chosen source to the active layer's routing."""
    return (
        Result[bpy.types.ViewLayer, BlenderFaults]
        .bind(
            call.root(bpy.types.ViewLayer),
            lambda layer: PassSet.attachment.revised(call.identity, layer, lambda routing: Ok(msgspec.structs.replace(routing, files=(*routing.files, PassFile(name=call.record.name))))),
        )
        .map(lambda _: Done())
    )


# --- [FILES]


def filings(found: tuple[FileSocket, ...], split: FileSplit) -> tuple[Filing, ...]:
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


def filed(pathed: Pathed, path: OutputPath, layer: str, filing: Filing, key: Key, convention: Conventions, dimensions: Descriptor[bpy.types.IntProperty]) -> BlenderResult[Expr]:
    """Builds File Output node of a filing under `key`, each item reading its source."""

    def sized(size: int) -> dict[Descriptor, Value]:
        return {dimensions: size}

    return pathed.node(path, layer, filing.output, tuple(convention.layer(source.name) for source in filing.sources), filing.kind).map(
        lambda paths: Expr(
            kind=bpy.types.CompositorNodeOutputFile,
            inputs=tuple((stored, source.term) for source, stored in zip(filing.sources, paths.items)),
            members=tuple((member.path, value) for member, value in paths.members.items()),
            content=Some(
                FileItems(items=tuple(FileItem(name=stored, kind=source.kind, members=source.dimensions.map(sized).default_value({})) for source, stored in zip(filing.sources, paths.items)))
            ),
            label=Some(key),
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

    def linked(node: bpy.types.Node) -> BlenderResult[str]:
        return socket_at(node.inputs, "Color").map(lambda socket: "COLOR" if socket.is_linked else "VALUE")

    def scanned(trees: tuple[bpy.types.NodeTree, ...]) -> BlenderResult[dict[str, str]]:
        return accumulated(linked(held).map(lambda kind: (held.aov_name, kind)) for tree in nested(trees) for held in tree.nodes if isinstance(held, bpy.types.ShaderNodeOutputAOV)).map(dict)

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


def named(layer: bpy.types.ViewLayer, entity: Entity) -> tuple[tuple[str, bpy.types.ID, Rgb], ...]:
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
    return tuple((name, block, Rgb(red=red, green=green, blue=blue)) for name, block in sorted(blocks.items()) for red, green, blue, *_ in (getattr(block, entity.color.prop.identifier),))


def colored(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, entity: Entity, rows: tuple[tuple[str, bpy.types.ID, Rgb], ...]) -> BlenderResult[Option[Expr]]:
    """Builds entity ID image of name colors under mattes summed into alpha, `Nothing` without names."""

    def matte(name: str) -> Expr:
        return Expr(
            kind=bpy.types.CompositorNodeCryptomatteV2,
            members=(("source", "RENDER"), ("scene", IdName.of(scene)), ("layer_name", f"{layer.name}.{entity.pass_name}"), ("matte_id", name)),
            socket="Matte",
        )

    def image(covers: tuple[tuple[Expr, Rgb], ...]) -> Option[Expr]:
        match tuple(cover * (color.red, color.green, color.blue, 1.0) for cover, color in covers), tuple(cover for cover, _ in covers):
            case (first, *rest), (base, *others):
                return Some(Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", sum(rest, first)), ("Alpha", sum(others, base)), ("Type", "Replace Alpha")), socket="Image", shape="RGBA"))
            case _:
                return Nothing

    return accumulated(
        Ok((matte(name), color)) if "," not in name and name == name.strip() else Error((BlenderCollisionFault(request=name, error=f"{layer.name}.{entity.pass_name}"),)) for name, _, color in rows
    ).map(image)


def recolored(layer: bpy.types.ViewLayer, entity: Entity, chosen: Callable[[tuple[Rgb, ...]], tuple[Rgb, ...]]) -> BlenderResult[int]:
    """Writes chosen colors onto entity names, each block keeping its alpha."""
    rows = named(layer, entity)
    return accumulated(
        entity.color.write(block, (color.red, color.green, color.blue, getattr(block, entity.color.prop.identifier)[3]))
        for (_, block, _), color in zip(rows, chosen(tuple(color for _, _, color in rows)))
    ).map(lambda priors: sum(prior.is_some() for prior in priors))


def located(scene: bpy.types.Scene, matte: Matte, layer: str) -> BlenderResult[tuple[bpy.types.ViewLayer, Entity]]:
    """Returns view layer `layer` and the Cryptomatte kind its matte names after the layer prefix."""
    return paired(
        Option.of_optional(scene.view_layers.get(layer)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene.view_layers), key=layer),)),
        Entity.named(matte.name.removeprefix(f"{layer}.")).to_result_with(lambda: (BlenderAbsentFault(owner=matte.file.path, key=matte.name),)),
    )


def exported(scene: bpy.types.Scene, file: Written, identity: Identity) -> BlenderResult[None]:
    """Writes one ID layer file per matte of a written multilayer file, one layer per matte name a scene block colors, refusing a path equal to source."""

    def painted(path: OutputPath, matte: Matte, layer: bpy.types.ViewLayer, entity: Entity) -> BlenderResult[str]:
        shown = {name: color for name, _, color in named(layer, entity)}
        known = {name: shown[name] for name in matte.ids if name in shown}
        return (
            path
            .written(file.layer, Output.MATTE, Some(entity.pass_name), scene.frame_current, ".exr")
            .map(str)
            .bind(lambda target: Error((BlenderCollisionFault(request=target, error=file.path),)) if target == file.path else Ok(target))
            .bind(
                lambda target: Multilayer.written(
                    target,
                    tuple(multilayer.Pass(layer=name, channels=("R", "G", "B", "A"), role=multilayer.Role.COLOR) for name in known),
                    multilayer.Layout.PARTS,
                    Storage(depth=16, compression=Some(Deflate())),
                    (matte.file.parts[0].width, matte.file.parts[0].height),
                    stamped(identity),
                    lambda held: matte.filled(frozenset({held.layer}), known[held.layer]),
                )
            )
        )

    def matted(path: OutputPath, matte: Matte) -> BlenderResult[str]:
        return located(scene, matte, file.layer).bind(lambda pair: painted(path, matte, *pair))

    return (
        PassSet()
        .bound(identity)
        .bind(lambda routing: paired(routing.path(scene), Multilayer.read(file.path).bind(Matte.found)))
        .bind(lambda found: accumulated(matted(found[0], matte) for matte in found[1]))
        .map(lambda _: None)
    )


def unfiled(identity: Identity, context: bpy.types.Context) -> BlenderResult[tuple[tuple[str, str, str], ...]]:
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

    def searched(routing: PassRouting, layer: bpy.types.ViewLayer, scene: bpy.types.Scene) -> BlenderResult[tuple[tuple[str, str, str], ...]]:
        return paired(routing.routing.read(layer), routing.path(scene)).bind(lambda found: routing.planned(scene, routing.held(scene), found[1]).map(lambda planned: listed(layer, found[0], *planned)))

    return paired(PassSet().bound(identity), rooted(bpy.types.ViewLayer, context), rooted(bpy.types.Scene, context)).bind(lambda found: searched(*found))


# --- [COMPOSITION] ----------------------------------------------------------------------

ids_export = Finish(run=exported, kinds=frozenset({Kind.LAYERED}))
COMMANDS: Final[tuple[Operator[Empty] | Operator[PassName], ...]] = (
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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "COMMANDS",
    "Derived",
    "Entity",
    "Family",
    "FileSocket",
    "FileSplit",
    "Filing",
    "LayerPlan",
    "PassFile",
    "PassFlags",
    "PassName",
    "PassOutput",
    "PassPreset",
    "PassRequest",
    "PassRouting",
    "PassSet",
    "Routing",
    "added",
    "colored",
    "exported",
    "filed",
    "filings",
    "formatted",
    "ids_export",
    "items",
    "lobe",
    "lobed",
    "located",
    "named",
    "offered",
    "pass_flags",
    "recolored",
    "recoloring",
    "reordered",
    "required",
    "sources",
    "synced",
    "turned",
    "unfiled",
    "updated",
]
