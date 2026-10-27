"""View layer pass routing into File Output nodes with derived layers and ID colors."""

from collections.abc import Callable, Iterable, Mapping
import datetime
from enum import auto, Enum
import functools
import itertools
from types import MappingProxyType
from typing import ClassVar, Final, Literal, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block, Map
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Answer, Call, Done, Empty, Operator, Properties, tallied, Unchanged
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.conditions import AllOf, Condition, Setting
from blender.bpy.data.rna.descriptors import choices, Descriptor, RnaPath, rooted, Struct, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Handler, staged, Subscription
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import Added, Changes, changes, made, owned, reconciled
from blender.bpy.nodes.graphs import DataType, Expr, FileItem, FileItems, FrameNode, group_input, Link, lowered, NodeGroup, rgba, Term, xyz
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.nodes.trees import nested, socket_at
from blender.bpy.render.outputs.finishing import Finish, stamped, Written
from blender.bpy.render.outputs.formats import extension, Kind, OutputFormat
from blender.bpy.render.outputs.paths import NodePaths, OutputPath, Outputs, Pathed, ScenePaths
from blender.bpy.render.view_layers.overrides import colors, encoded, LayerOverride, LayerTag, Role, Spectrum
from blender.bpy.scene.materials import AOVS, DataPass, Shading, stored, Utility
from blender.bpy.settings.attachments import Attached, Attachment
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.rows import Caption, Flow, joined, Label, Line, Member, OperatorButton, placed, Property, Row, RowDraw
from blender.bpy.ui.layout.sections import Single
from blender.bpy.ui.layout.values import Hosted, Listing, Store
from blender.imaging.files.buffers import unassociated
from blender.imaging.files.coding import Depth, Rounded
from blender.imaging.files.cryptomatte import Matte
from blender.imaging.files.documents import Document, LayerGroup, PixelLayer
from blender.imaging.files.images import Deflate, Storage
from blender.imaging.files.multilayer import Multilayer, Pass, PassLayout, PassRole
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired
from blender.records.identity import content_hash, rng
from blender.records.trees import Owner, Retention, transfer, Tree
from blender.rendering.outputs.conventions import Axis, Nuke, Space
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type ItemKind = Literal["FLOAT", "RGBA"]


class Family(Enum):
    """Light pass family of direct, indirect, and color passes, each a term recombined beauty sums under its own gain."""

    DIFFUSE = auto()
    GLOSSY = auto()
    TRANSMISSION = auto()
    VOLUME = auto()


class Derived(Enum):
    """Layer built from render passes."""

    DIRECT = auto()
    INDIRECT = auto()
    DEPTH = auto()
    INVERSE = auto()


class PassOutput(Enum):
    """Output class a pass file row routes its pass to, every class but the preview."""

    BEAUTY = auto()
    DATA = auto()
    MATTE = auto()

    @property
    def output(self) -> Output:
        """Output class of the same name."""
        return Output[self.name]


class Entity(Enum):
    """Cryptomatte kind."""

    OBJECT = auto()
    MATERIAL = auto()
    ASSET = auto()

    @classmethod
    def named(cls, name: str) -> Option[Self]:
        """Returns kind with pass name `name`, `Nothing` for another."""
        return Block.of_seq(cls).filter(lambda member: ENTITIES[member].pass_name == name).try_head()


class FileSplit(Enum):
    """Split of layer passes into files."""

    ONE = auto()
    CLASSES = auto()
    PASSES = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_DEPTH: Final = Descriptor.declared(bpy.types.ViewLayer, "use_pass_z", bpy.types.BoolProperty)
_FILM: Final = Descriptor.declared(bpy.types.Scene, ("render", "film_transparent"), bpy.types.BoolProperty)
_ENGINE: Final = Descriptor.declared(bpy.types.Scene, ("render", "engine"), bpy.types.EnumProperty)
_TRANSPARENT: Final = Descriptor.declared(bpy.types.ViewLayer, ("eevee", "use_pass_transparent"), bpy.types.BoolProperty)
_EMISSION: Final = Descriptor.declared(bpy.types.ViewLayer, "use_pass_emit", bpy.types.BoolProperty)
_ENVIRONMENT: Final = Descriptor.declared(bpy.types.ViewLayer, "use_pass_environment", bpy.types.BoolProperty)
_SHADOW: Final = Descriptor.declared(bpy.types.ViewLayer, "use_pass_shadow", bpy.types.BoolProperty)
_OCCLUSION: Final = Descriptor.declared(bpy.types.ViewLayer, "use_pass_ambient_occlusion", bpy.types.BoolProperty)
_KINDS: Final[Mapping[str, DataType]] = MappingProxyType({"VALUE": "FLOAT", "VECTOR": "VECTOR", "RGBA": "RGBA"})
_NUKE: Final = Nuke(names=MappingProxyType({"Combined": "rgba", "Position": "Pworld", "Depth": "z"}), channels=MappingProxyType({"Depth": "Z"}))
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

# --- [MODELS] ---------------------------------------------------------------------------


class FamilyPasses(msgspec.Struct, frozen=True, kw_only=True):
    """Total layer name and flag paths per slot of one light family."""

    name: str
    direct: tuple[RnaPath, ...]
    indirect: tuple[RnaPath, ...]
    color: tuple[RnaPath, ...]


class DerivedLayer(msgspec.Struct, frozen=True, kw_only=True):
    """Layer name, item kind, and output class of a derived layer."""

    name: str
    kind: ItemKind
    output: Output


class IdPass(msgspec.Struct, frozen=True, kw_only=True):
    """Cryptomatte kind's name, Blender pass name, enabling flag, and block color member."""

    name: str
    pass_name: str
    flag: Descriptor[bpy.types.BoolProperty]
    color: Descriptor[bpy.types.FloatProperty]


class PassFlags(msgspec.Struct, frozen=True, kw_only=True):
    """Pass flags of running build, resolved once."""

    every: tuple[tuple[type[Struct], Descriptor[bpy.types.BoolProperty]], ...]
    families: Mapping[Family, tuple[frozenset[Descriptor[bpy.types.BoolProperty]], frozenset[Descriptor[bpy.types.BoolProperty]], frozenset[Descriptor[bpy.types.BoolProperty]]]]

    @classmethod
    def of(cls) -> Outcome[Self]:
        """Resolves every pass flag and each family's direct, indirect, and color flags from its declared paths, faulting each path the build lacks."""

        def slot(paths: tuple[RnaPath, ...]) -> Outcome[frozenset[Descriptor[bpy.types.BoolProperty]]]:
            return accumulated(Descriptor.resolve(bpy.types.ViewLayer, path, bpy.types.BoolProperty) for path in paths).map(frozenset)

        return accumulated(paired(slot(held.direct), slot(held.indirect), slot(held.color)) for held in FAMILIES.values()).map(
            lambda families: cls(every=pass_flags(), families=dict(zip(FAMILIES, families)))
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
            *(slot for family in flagged(Family, routing.totals) for slot in self.families[family]),
            *(held for item in flagged(Derived, routing.derived) for held in lit(item)),
            (ENTITIES[entity].flag for entity in flagged(Entity, routing.ids)),
        )


class PassRequest(msgspec.Struct, frozen=True, kw_only=True):
    """Passes, condition, and precision a graph needs of one view layer."""

    layer: str
    flags: frozenset[Descriptor[bpy.types.BoolProperty]] = frozenset()
    condition: Option[Condition] = Nothing
    precise: bool = False

    @classmethod
    def depth(cls, layer: str, *, precise: bool) -> Outcome[Self]:
        """Returns Depth pass request of a layer."""
        return Ok(cls(layer=layer, flags=frozenset({_DEPTH}), precise=precise))


class PassFile(bpy.types.PropertyGroup):
    """File choice of the source the row names."""

    use_output: bpy.props.BoolProperty(name="File", description="Write the pass, skipped when off", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    output: bpy.props.EnumProperty(name="Output", description="Output class file holding pass", items=choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in PassOutput))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Routing(bpy.types.PropertyGroup):
    """Routing of view layer passes into files."""

    split: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Files", description="One file, a file per output class, or a file per pass", items=choices((member.name, bpy.path.display_name(member.name.lower()), "") for member in FileSplit)
    )
    nuke: bpy.props.BoolProperty(name="Nuke", description="Write the layer names and axes Nuke reads, host names when off")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    normals: bpy.props.EnumProperty(name="Normals", description="Normal pass space", items=choices((space.name, bpy.path.display_name(space.name.lower()), "") for space in Space))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    totals: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Totals", description="Light families summed into one layer each", options={"ENUM_FLAG"}, items=tuple((member.name, bpy.path.display_name(member.name.lower()), "") for member in Family)
    )
    derived: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Derived",
        description="Light sums and depth layers built from passes",
        options={"ENUM_FLAG"},
        items=tuple((member.name, bpy.path.display_name(member.name.lower()), "") for member in Derived),
    )
    ids: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="ID Colors",
        description="Cryptomatte kinds drawn as one color per name",
        options={"ENUM_FLAG"},
        items=tuple((member.name, bpy.path.display_name(member.name.lower()), "") for member in Entity),
    )
    files: bpy.props.CollectionProperty(type=PassFile, name="Passes", description="File choice per pass")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class PassPreset(msgspec.Struct, frozen=True, kw_only=True):
    """Pass flags and routing saved together."""

    flags: Snapshot
    routing: Snapshot


class PassName(bpy.types.PropertyGroup):
    """Source name a file row adds from sources no row holds."""

    name: bpy.props.StringProperty(name="Pass", description="Pass the file row names")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


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
    filings: Mapping[Key, Filing]
    convention: Option[Nuke]


class PassRouting(msgspec.Struct, frozen=True, kw_only=True):
    """`PassSet` bound to an identity, with the members its plans write."""

    use: ClassVar[Descriptor[bpy.types.BoolProperty]] = Descriptor.declared(bpy.types.ViewLayer, "use", bpy.types.BoolProperty)
    precision: ClassVar[Descriptor[bpy.types.EnumProperty]] = Descriptor.declared(bpy.types.Scene, ("render", "compositor_precision"), bpy.types.EnumProperty)
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

    def path(self, scene: bpy.types.Scene) -> Outcome[OutputPath]:
        """Returns scene output path on the local calendar date."""
        return self.pathed.path(scene, datetime.datetime.now(datetime.UTC).astimezone().date())

    def held(self, scene: bpy.types.Scene) -> Option[bpy.types.NodeTree]:
        """Returns routing tree identity holds for scene."""
        return PassSet.ownership.found(self.identity, of=Some(scene))

    def framed(self, scene: bpy.types.Scene, held: dict[Key, bpy.types.Node], path: OutputPath, index: int, layer: bpy.types.ViewLayer) -> Outcome[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
        """Plans frame, Render Layers node, sources, and File Output nodes of one used layer."""
        prefix = f"layer_{index}"
        rendered = Expr(kind=bpy.types.CompositorNodeRLayers, members=(("scene", IdName.of(scene)), ("layer", layer.name)), label=Some(f"{prefix}_render"))

        def filed_all(routing: Routing, record: LayerTag, found: tuple[FileSocket, ...]) -> Outcome[tuple[FrameNode, tuple[Link, ...], LayerPlan]]:
            plan = {f"{prefix}_{filing.output.name.lower()}_{filing.kind.name.lower()}": filing for filing in filings(found, FileSplit[routing.split])}
            convention = reading(routing)
            return (
                accumulated(filed(self.pathed, path, layer.name, filing, key, convention, self.dimensions) for key, filing in plan.items())
                .bind(lambda files: lowered(bpy.types.CompositorNodeTree, rendered, *files))
                .map(
                    lambda merged: (
                        FrameNode(members={self.label: layer.name, self.custom: True, self.tint: encoded(record.color)}, nodes=merged.nodes),
                        merged.links,
                        LayerPlan(frame=prefix, filings=plan, convention=convention),
                    )
                )
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
        used = self.used(scene)
        return accumulated(self.framed(scene, held, path, index, layer) for index, layer in enumerate(used)).bind(
            lambda frames: grouped(frames).map(lambda group: (group, {layer.name: plan for layer, (_, _, plan) in zip(used, frames)}))
        )

    def reconciled(self, scene: bpy.types.Scene, context: bpy.types.Context, path: OutputPath) -> Outcome[Changes]:
        """Reconciles passes, AOVs, light groups, and routing to scene settings, called after overrides apply or revert."""
        held = self.held(scene)
        before = held.map(lambda found: items(found, self.identity)).default_value({})

        def applied(tree: bpy.types.NodeTree, group: NodeGroup, plans: dict[str, LayerPlan]) -> Outcome[Changes]:
            return reconciled(group, tree, self.identity).bind(lambda rows: formatted(tree, group, plans, rows, before, self.identity).map(lambda _: rows))

        def rebuilt(first: NodeGroup, _: dict[str, LayerPlan]) -> Outcome[Changes]:
            return made(first, self.identity, of=Some(scene)).bind(lambda tree: self.planned(scene, Some(tree), path).bind(lambda planned: applied(tree, *planned)))

        def requested(layer: bpy.types.ViewLayer) -> Outcome[PassRequest]:
            return Ok(PassRequest(layer=layer.name, flags=self.flags.opened(self.routing.struct(layer))))

        return (
            accumulated(map(requested, self.used(scene)))
            .bind(lambda requests: required(scene, context, requests, self.precision))
            .bind(lambda _: _synced(scene, self.identity))
            .bind(lambda _: self.planned(scene, held, path))
            .bind(lambda first: rebuilt(*first))
        )

    def refreshed(self, context: bpy.types.Context) -> Outcome[None]:
        """Reconciles every editable scene with stored layer routing, owned runtime scenes skipped."""

        def refresh(scene: bpy.types.Scene) -> Outcome[Changes]:
            return self.path(scene).bind(lambda path: self.reconciled(scene, context, path))

        return rooted(bpy.types.BlendData, context).bind(
            lambda data: accumulated(
                refresh(scene) for scene in data.scenes if scene.is_editable and not staged(self.identity, scene) and any(self.routing.stored(layer) for layer in scene.view_layers)
            ).map(lambda _: None)
        )

    def stale(self, context: bpy.types.Context) -> Outcome[Option[str]]:
        """Returns pending routing group writes for context scene, `Nothing` for a current tree."""

        def pending(tree: bpy.types.NodeTree, group: NodeGroup, _: dict[str, LayerPlan]) -> Outcome[Option[str]]:
            return changes(group, tree, self.identity).map(lambda rows: sum(map(len, msgspec.structs.astuple(rows)))).map(lambda count: Some(f"{count} changes pending") if count else Nothing)

        def scened(scene: bpy.types.Scene) -> Outcome[Option[str]]:
            return self.path(scene).bind(
                lambda path: (
                    self.held(scene).map(lambda tree: self.planned(scene, Some(tree), path).bind(lambda planned: pending(tree, *planned))).default_value(Ok(Some("Update builds routing group")))
                )
            )

        return Option.of_optional(context.scene).map(scened).default_value(Ok(Nothing))

    def watched(self) -> tuple[Subscription, ...]:
        """Builds subscriptions refreshing routing, one per pass flag, watched setting, and runtime attachment member."""
        groups = tuple((form, prop) for form in (Routing, Outputs, LayerTag) for prop in form.bl_rna.properties if prop.is_runtime)
        return tuple(
            Subscription(owner=owner, setting=prop, body=lambda: self.refreshed(bpy.context)) for owner, prop in (*((owner, flag.prop) for owner, flag in self.flags.every), *_WATCHED, *groups)
        )

    def listing(self) -> Listing[PassPreset]:
        """Returns pass presets over layer flags and routing, applied to every used layer."""
        selection = Some(tuple(flag for _, flag in self.flags.every))
        eevee = frozenset().union(
            *(direct | color for family in (Family.DIFFUSE, Family.GLOSSY) for direct, _, color in (self.flags.families[family],)),
            self.flags.families[Family.VOLUME][0] & frozenset(flag for owner, flag in self.flags.every if owner is bpy.types.ViewLayerEEVEE),
        )
        common = frozenset({_EMISSION, _ENVIRONMENT})
        recipes = (
            ("lightray_rebuild", common | self.flags.lobes, (), (Derived.DIRECT, Derived.INDIRECT)),
            ("material_rebuild", common | self.flags.lobes, tuple(Family), ()),
            ("multipass_breakdown", common | self.flags.lobes, (), ()),
            ("eevee_material_rebuild", common | eevee, (Family.DIFFUSE, Family.GLOSSY, Family.VOLUME), ()),
            ("eevee_multipass", common | eevee | {_TRANSPARENT, _SHADOW, _OCCLUSION}, (), ()),
        )
        owners = tuple(dict.fromkeys(flag.owner for _, flag in self.flags.every))

        def stock(enabled: frozenset[Descriptor[bpy.types.BoolProperty]], totals: tuple[Family, ...], derived: tuple[Derived, ...]) -> PassPreset:
            return PassPreset(
                flags=Snapshot(
                    struct=bpy.types.ViewLayer.__name__,
                    tree=Tree(roots=owners, rows=tuple((owner, Owner(values={flag.prop.identifier: flag in enabled for _, flag in self.flags.every if flag.owner == owner})) for owner in owners)),
                ),
                routing=Snapshot.of(Routing, {"totals": tuple(sorted(member.name for member in totals)), "derived": tuple(sorted(member.name for member in derived))}),
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
                layers = self.used(scene)

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

    key: ClassVar[Key] = "passes"
    attachment: ClassVar[Attachment[Routing]] = Attachment(key=key, form=Routing, owner=bpy.types.ViewLayer)
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
            names = tuple(plan.convention.map(lambda nuke: nuke.layer(source.name)).default_value(source.name) for source in filing.sources)

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

            def filed_at(settings: bpy.types.ImageFormatSettings, item: Option[str], passes: Map[str, str], suffix: str) -> Outcome[Option[Written]]:
                return path.written(layer, filing.output, item, frame, suffix).map(lambda file: Written.of(str(file), settings, filing.output, plan.convention, layer, passes))

            return (
                routing.pathed
                .node(path, layer, filing.output, names, filing.kind)
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

        return self.bound(identity).bind(lambda routing: path.map(Ok).default_with(lambda: routing.path(scene)).bind(lambda chosen: collected(routing, chosen)))

    @classmethod
    def document(cls, _identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> Outcome[Document[np.uint8 | np.uint16 | np.float32]]:
        """Builds ID document of a written frame with one group per matte, the `Documents` port."""
        size = (frame.parts[0].height, frame.parts[0].width)
        coded = functools.partial(depth.coded, coding=Rounded())

        def grouped(index: int, matte: Matte) -> Outcome[tuple[LayerGroup[np.uint8 | np.uint16 | np.float32], npt.NDArray[np.float32]]]:
            def cover(name: str, color: tuple[float, float, float]) -> Outcome[tuple[str, tuple[float, float, float], npt.NDArray[np.float32]]]:
                return matte.coverage(frozenset({name})).map(lambda plane: (name, color, plane))

            def covered(layer: bpy.types.ViewLayer, entity: Entity) -> Outcome[tuple[tuple[str, tuple[float, float, float], npt.NDArray[np.float32]], ...]]:
                return accumulated(cover(name, color) for name, _, color in named(layer, entity))

            return located(scene, matte, matte.name.rpartition(".")[0]).bind(
                lambda pair: covered(*pair).map(
                    lambda rows: (
                        LayerGroup(
                            name=matte.name,
                            visible=index == 0,
                            layers=tuple(
                                PixelLayer(name=name, color=coded(np.broadcast_to(np.asarray(encoded(color), dtype=np.float32), (*size, 3))), alpha=coded(plane)) for name, color, plane in rows
                            ),
                        ),
                        functools.reduce(np.add, (plane[..., None] * np.asarray((*encoded(color), 1.0), dtype=np.float32) for _, color, plane in rows), np.zeros((*size, 4), np.float32)),
                    )
                )
            )

        return Result[tuple[Matte, ...], Faults].bind(
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

    def drawn(self, identity: Identity) -> Outcome[RowDraw]:
        """Draws film transparency, pending changes, operator, and preset rows."""

        def rows(routing: PassRouting, catcher: Descriptor[bpy.types.BoolProperty]) -> tuple[Row, ...]:
            opaque = AllOf(
                conditions=tuple(
                    Setting(descriptor=flag, accepted=(False,)) | ~Setting(descriptor=_ENGINE, accepted=(running,)) for flag, running in ((catcher, "CYCLES"), (_TRANSPARENT, "BLENDER_EEVEE"))
                )
            )
            return (
                Property(target=Member(root=bpy.types.Scene, path=("render", "film_transparent")), condition=Some(Setting(descriptor=_FILM) | opaque)),
                Label(text=lambda context: routing.stale(context).default_with(lambda faults: Some("\n".join(fault.message for fault in faults)))),
                Caption(rows=tuple(OperatorButton(target=command) for command in COMMANDS), flow=Flow.GRID),
                routing.listing(),
            )

        return (
            paired(self.bound(identity), Descriptor.resolve(bpy.types.ViewLayer, ("cycles", "use_pass_shadow_catcher"), bpy.types.BoolProperty))
            .bind(lambda pair: placed(rows(*pair), identity))
            .map(joined)
        )

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns routing acquisitions."""
        return self.bound(identity).map(
            lambda routing: (
                self.ownership,
                routing.lobe,
                *COMMANDS,
                routing.listing(),
                *(Handler(handlers=handlers, body=lambda: routing.refreshed(bpy.context)) for handlers in (bpy.app.handlers.load_post, bpy.app.handlers.save_post)),
                *routing.watched(),
            )
        )

    def section(self, retention: Option[Callable[[Identity], Outcome[Retention]]] = Nothing) -> Single[Routing]:
        """Returns File Output section over context layer routing store."""
        return Single(key=self.key, title="File Output", rows=(self,), store=Some(Hosted(store=Store(attachment=self.attachment, retention=retention))))


# --- [TABLES] ---------------------------------------------------------------------------

FAMILIES: Final[Mapping[Family, FamilyPasses]] = MappingProxyType({
    Family.DIFFUSE: FamilyPasses(name="Diffuse Total", direct=(("use_pass_diffuse_direct",),), indirect=(("use_pass_diffuse_indirect",),), color=(("use_pass_diffuse_color",),)),
    Family.GLOSSY: FamilyPasses(name="Glossy Total", direct=(("use_pass_glossy_direct",),), indirect=(("use_pass_glossy_indirect",),), color=(("use_pass_glossy_color",),)),
    Family.TRANSMISSION: FamilyPasses(
        name="Transmission Total", direct=(("use_pass_transmission_direct",),), indirect=(("use_pass_transmission_indirect",),), color=(("use_pass_transmission_color",),)
    ),
    Family.VOLUME: FamilyPasses(name="Volume Total", direct=(("cycles", "use_pass_volume_direct"), ("eevee", "use_pass_volume_direct")), indirect=(("cycles", "use_pass_volume_indirect"),), color=()),
})
DERIVED: Final[Mapping[Derived, DerivedLayer]] = MappingProxyType({
    Derived.DIRECT: DerivedLayer(name="Direct Light", kind="RGBA", output=Output.BEAUTY),
    Derived.INDIRECT: DerivedLayer(name="Indirect Light", kind="RGBA", output=Output.BEAUTY),
    Derived.DEPTH: DerivedLayer(name="Depth Map", kind="FLOAT", output=Output.DATA),
    Derived.INVERSE: DerivedLayer(name="Inverse Depth", kind="FLOAT", output=Output.DATA),
})
ENTITIES: Final[Mapping[Entity, IdPass]] = MappingProxyType({
    entity: IdPass(
        name=bpy.path.display_name(entity.name.lower()),
        pass_name=pass_name,
        flag=Descriptor.declared(bpy.types.ViewLayer, flag, bpy.types.BoolProperty),
        color=Descriptor.declared(owner, color, bpy.types.FloatProperty),
    )
    for entity, pass_name, flag, owner, color in (
        (Entity.OBJECT, "CryptoObject", "use_pass_cryptomatte_object", bpy.types.Object, "color"),
        (Entity.MATERIAL, "CryptoMaterial", "use_pass_cryptomatte_material", bpy.types.Material, "diffuse_color"),
        (Entity.ASSET, "CryptoAsset", "use_pass_cryptomatte_asset", bpy.types.Object, "color"),
    )
})

# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [FLAGS]


def flagged[E: Enum](kind: type[E], names: Iterable[str]) -> tuple[E, ...]:
    """Returns the members of an enum flag property's identifier set in declaration order."""
    held = frozenset(names)
    return tuple(member for member in kind if member.name in held)


def reading(routing: Routing) -> Option[Nuke]:
    """Returns the Nuke convention the routing asks for, Nothing for host names."""
    return Some(_NUKE) if routing.nuke else Nothing


def grouped(frames: tuple[tuple[FrameNode, tuple[Link, ...], LayerPlan], ...]) -> Outcome[NodeGroup]:
    """Builds routing group around image pass-through with each layer frame."""
    interface = Interface(outputs=(Socket(key="result", name="Image", kind=bpy.types.NodeSocketColor),), inputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),))
    return NodeGroup.built(PassSet.ownership.key, PassSet.title, interface, {"result": group_input("image", "RGBA")}, bpy.types.CompositorNodeTree).map(
        lambda group: msgspec.structs.replace(
            group,
            graph=msgspec.structs.replace(
                group.graph,
                nodes={**group.graph.nodes, **{plan.frame: frame for frame, _, plan in frames}},
                links=tuple(dict.fromkeys((*group.graph.links, *(link for _, links, _ in frames for link in links)))),
            ),
        )
    )


def pass_flags() -> tuple[tuple[type[Struct], Descriptor[bpy.types.BoolProperty]], ...]:
    """Returns writable pass flags of `ViewLayer` and its pointer structs in RNA order with owning class."""
    root = bpy.types.ViewLayer.bl_rna
    structs = (
        ((), bpy.types.ViewLayer, root.properties),
        *(((prop.identifier,), type(fixed), fixed.properties) for prop in root.properties if isinstance(prop, bpy.types.PointerProperty) for fixed in Option.of_optional(prop.fixed_type).to_list()),
    )
    return tuple(
        (owner, Descriptor(root=bpy.types.ViewLayer, owner=path, prop=prop))
        for path, owner, properties in structs
        for prop in properties
        if isinstance(prop, bpy.types.BoolProperty) and not prop.is_readonly and prop.identifier.startswith("use_pass_")
    )


def required(scene: bpy.types.Scene, context: bpy.types.Context, requests: tuple[PassRequest, ...], precision: Descriptor[bpy.types.EnumProperty]) -> Outcome[None]:
    """Enables passes and precision each request needs, faulting unmet conditions."""

    def met(request: PassRequest) -> Outcome[None]:
        gate: Outcome[None] = request.condition.map(lambda condition: condition.check(context, Some(scene))).default_value(Ok(None))
        return (
            gate
            .bind(lambda _: Option.of_optional(scene.view_layers.get(request.layer)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(scene.view_layers), member=request.layer)))
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


def lobe() -> Outcome[NodeGroup]:
    """Builds shared lobe total group, `(direct + indirect) * color`."""
    interface = Interface(
        outputs=(Socket(key="total", name="Total", kind=bpy.types.NodeSocketColor),),
        inputs=tuple(
            Socket(key=key, name=bpy.path.display_name(key), kind=bpy.types.NodeSocketColor, members={"default_value": value})
            for key, value in (("direct", (0.0, 0.0, 0.0, 1.0)), ("indirect", (0.0, 0.0, 0.0, 1.0)), ("color", (1.0, 1.0, 1.0, 1.0)))
        ),
    )
    return NodeGroup.built(
        "lobe_total", "Lobe Total", interface, {"total": (group_input("direct", "RGBA") + group_input("indirect", "RGBA")) * group_input("color", "RGBA")}, bpy.types.CompositorNodeTree
    )


def lobed(
    slots: tuple[frozenset[Descriptor[bpy.types.BoolProperty]], frozenset[Descriptor[bpy.types.BoolProperty]], frozenset[Descriptor[bpy.types.BoolProperty]]],
    offered: Mapping[str, Term],
    group: NodeGroup,
) -> Option[Expr]:
    """Places lobe group over the live Render Layers outputs a family's direct, indirect, and color flags name, `Nothing` without a direct or indirect output."""
    linked = {key: offered[name] for key, flags in zip(("direct", "indirect", "color"), slots, strict=True) for name in sorted(flag.prop.name for flag in flags) if name in offered}
    return Some(group.placed(linked, "total")) if linked.keys() & {"direct", "indirect"} else Nothing


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


def sources(
    scene: bpy.types.Scene, layer: bpy.types.ViewLayer, rendered: Expr, live: bpy.types.Node, routing: Routing, role: Role, flags: PassFlags, group: NodeGroup
) -> Outcome[tuple[FileSocket, ...]]:
    """Returns sources layer files write in socket order, derived layers and ID images after, each under its file row's gate."""
    convention = reading(routing)
    offered = {socket.name: socket for socket in live.outputs if socket.enabled and socket.name != "Alpha" and socket.type in _KINDS}
    outputs = {name: msgspec.structs.replace(rendered, socket=socket.identifier, shape=_KINDS[socket.type]) for name, socket in offered.items()}
    spaces = {"Normal": Space[routing.normals], "Position": Space.WORLD, DataPass.POSITION.name.lower(): Space.WORLD, DataPass.REFERENCE.name.lower(): Space.WORLD}
    gates = {row.name: row for row in routing.files}

    def missing(name: str) -> Faults:
        return Fault.of(BlenderReason.ABSENT, repr(live.outputs), member=name)

    def sourced(expr: Expr, name: str, kind: DataType, output: Output, dimensions: Option[int] = Nothing) -> Outcome[tuple[FileSocket, ...]]:
        match gates.get(name):
            case None:
                return Ok((FileSocket(name=name, term=expr, kind=kind, dimensions=dimensions, output=output),))
            case PassFile(use_output=True, output=held):
                return Ok((FileSocket(name=name, term=expr, kind=kind, dimensions=dimensions, output=PassOutput[held].output),))
            case _:
                return Ok(())

    def oriented(name: str) -> Outcome[Expr]:
        read = outputs[name]
        match spaces.get(name), convention.to_optional():
            case Space.CAMERA, nuke:
                return (
                    Option
                    .of_optional(scene.camera)
                    .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(scene), member="camera"))
                    .map(lambda held: turned(read, nuke.axes[Space.CAMERA] if nuke else (1, 2, 3), Some(held)))
                )
            case Space() as space, Nuke() as nuke:
                return Ok(turned(read, nuke.axes[space], Nothing))
            case None, Nuke() as nuke if name == "Vector":
                return Ok(reordered(read, nuke.motion))
            case _:
                return Ok(read)

    def passed(name: str, socket: bpy.types.NodeSocket) -> Outcome[tuple[FileSocket, ...]]:
        dimensions = Descriptor.resolve(socket, "default_value", bpy.types.FloatProperty).map(lambda member: Some(member.prop.array_length)) if socket.type == "VECTOR" else Ok(Nothing)
        classed = (
            Output.MATTE
            if any(name.startswith(held.pass_name) and name.removeprefix(held.pass_name).isdigit() for held in ENTITIES.values())
            else Output.DATA
            if role is Role.DATA or socket.type != "RGBA"
            else Output.BEAUTY
        )
        return paired(oriented(name), dimensions).bind(lambda pair: sourced(pair[0], name, _KINDS[socket.type], classed, pair[1]))

    def totaled(family: Family) -> Outcome[tuple[FileSocket, ...]]:
        return lobed(flags.families[family], outputs, group).to_result_with(lambda: missing(FAMILIES[family].name)).bind(lambda total: sourced(total, FAMILIES[family].name, "RGBA", Output.BEAUTY))

    def derived(item: Derived) -> Outcome[tuple[FileSocket, ...]]:
        layered = DERIVED[item]
        depth = Option.of_optional(outputs.get(DataPass.DEPTH.name.lower())).to_result_with(lambda: missing(DataPass.DEPTH.name.lower()))
        match item:
            case Derived.DIRECT | Derived.INDIRECT:
                match tuple(
                    found
                    for direct, indirect, color in flags.families.values()
                    for found in lobed((direct, frozenset(), color) if item is Derived.DIRECT else (frozenset(), indirect, color), outputs, group).to_list()
                ):
                    case (first, *rest):
                        return sourced(sum(rest, first), layered.name, layered.kind, layered.output)
                    case _:
                        return Ok(())
            case Derived.DEPTH:
                return depth.bind(lambda read: sourced(Expr(kind=bpy.types.CompositorNodeNormalize, inputs=(("Value", read),)), layered.name, layered.kind, layered.output))
            case Derived.INVERSE:
                return depth.bind(lambda read: sourced(1.0 / read, layered.name, layered.kind, layered.output))

    def identified(entity: Entity) -> Outcome[tuple[FileSocket, ...]]:
        return entity_image(scene, layer, entity, named(layer, entity)).bind(
            lambda found: found.map(lambda image: sourced(image, f"{ENTITIES[entity].name} IDs", "RGBA", Output.MATTE)).default_with(lambda: Ok(()))
        )

    return accumulated((
        *itertools.starmap(passed, offered.items()),
        *map(totaled, flagged(Family, routing.totals)),
        *map(derived, flagged(Derived, routing.derived)),
        *map(identified, flagged(Entity, routing.ids)),
    )).map(lambda parts: tuple(source for part in parts for source in part))


# --- [COMMANDS]


def passes_update(call: Call[Empty]) -> Outcome[Done]:
    """Reconcile scene pass routing and File Output nodes."""
    return (
        paired(PassSet().bound(call.identity), call.root(bpy.types.Scene))
        .bind(lambda pair: pair[0].path(pair[1]).bind(lambda path: pair[0].reconciled(pair[1], call.context, path)))
        .map(lambda _: Done())
    )


def recolored(chosen: Callable[[tuple[tuple[float, float, float], ...]], tuple[tuple[float, float, float], ...]], call: Call[Empty]) -> Outcome[Answer]:
    """Writes chosen colors onto the names of each Cryptomatte kind the active layer draws as ID colors."""

    def painted(layer: bpy.types.ViewLayer, routing: Routing) -> Outcome[Answer]:
        if not routing.ids:
            return Ok(Unchanged(reason=f"{layer.name} draws no ID colors"))
        return accumulated(colors_written(layer, entity, chosen) for entity in flagged(Entity, routing.ids)).map(
            lambda counts: Done() if sum(counts) else Unchanged(reason=f"{layer.name} ID colors hold chosen colors")
        )

    return Result[bpy.types.ViewLayer, Faults].bind(call.root(bpy.types.ViewLayer), lambda layer: PassSet.attachment.read(call.identity, layer).bind(lambda routing: painted(layer, routing)))


def ids_spread(call: Call[Empty]) -> Outcome[Answer]:
    """Set evenly spaced hues on layer ID names."""
    return recolored(lambda held: colors(Spectrum(saturation=1.0, value=1.0), len(held)), call)


def ids_shuffle(call: Call[Empty]) -> Outcome[Answer]:
    """Shuffle colors among layer ID names."""
    return recolored(lambda held: tuple(held[index] for index in rng(content_hash(Routing, arrays=(np.asarray(held),))).permutation(len(held))), call)


def pass_file_add(call: Call[PassName]) -> Outcome[Answer]:
    """Add file choice for a layer pass."""
    return (
        Result[bpy.types.ViewLayer, Faults]
        .bind(call.root(bpy.types.ViewLayer), lambda layer: PassSet.attachment.read(call.identity, layer).bind(lambda routing: written(routing.files.add(), "name", call.properties.name)))
        .map(lambda prior: Done() if prior.is_some() else Unchanged(reason="Pass file row needs a pass name"))
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


def filed(pathed: Pathed, path: OutputPath, layer: str, filing: Filing, key: Key, convention: Option[Nuke], dimensions: Descriptor[bpy.types.IntProperty]) -> Outcome[Expr]:
    """Builds File Output node of a filing under `key`, each item reading its source."""

    def sized(size: int) -> dict[Descriptor, Value]:
        return {dimensions: size}

    return pathed.node(path, layer, filing.output, tuple(convention.map(lambda nuke: nuke.layer(source.name)).default_value(source.name) for source in filing.sources), filing.kind).map(
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


def formatted(tree: bpy.types.NodeTree, group: NodeGroup, plans: dict[str, LayerPlan], rows: Changes, before: dict[Key, frozenset[str]], identity: Identity) -> Outcome[None]:
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


def _synced(scene: bpy.types.Scene, identity: Identity) -> Outcome[None]:
    """Adds AOVs and the light groups scene emitters name to each used layer, removing an AOV Blender renames or invalidates."""

    def linked(node: bpy.types.Node) -> Outcome[str]:
        return socket_at(node.inputs, "Color").map(lambda socket: "COLOR" if socket.is_linked else "VALUE")

    def scanned(trees: tuple[bpy.types.NodeTree, ...]) -> Outcome[Mapping[str, str]]:
        return accumulated(linked(held).map(lambda kind: (held.aov_name, kind)) for tree in nested(trees) for held in tree.nodes if isinstance(held, bpy.types.ShaderNodeOutputAOV)).map(dict)

    def setup(material: bpy.types.Material) -> Outcome[Mapping[str, str]]:
        def kinds(held: Option[Shading]) -> Mapping[str, str]:
            match held.to_optional():
                case Shading(setup=Utility(passes=passes)):
                    return {str(member.name).lower(): str(AOVS[member].kind) for member in passes}
                case _:
                    return {}

        tree: Outcome[bpy.types.NodeTree] = Option.of_optional(material.node_tree).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(material), member="node_tree"))
        return Result[bpy.types.NodeTree, Faults].bind(tree, lambda found: stored(identity).bind(lambda store: store.read(found))).map(kinds)

    def wanted(layer: bpy.types.ViewLayer) -> Outcome[Mapping[str, str]]:
        override = Option.of_optional(layer.material_override)
        materials = override.map(lambda material: (material,)).default_with(lambda: tuple(slot.material for item in scene.objects for slot in item.material_slots if slot.material))
        trees = tuple(tree for tree in (*(material.node_tree for material in materials), *(world.node_tree for world in Option.of_optional(scene.world).to_list())) if tree)
        return override.filter(lambda material: Utility.ownership.marked(identity, material).is_some()).map(setup).default_with(lambda: scanned(trees))

    def aovs(layer: bpy.types.ViewLayer, names: Mapping[str, str]) -> Outcome[None]:
        held = {aov.name: aov for aov in layer.aovs}

        def synced_aov(name: str, kind: str) -> Outcome[None]:
            aov = held.get(name) or layer.aovs.add()
            answer = (
                paired(written(aov, "name", name), written(aov, "type", kind))
                .filter_with(lambda _: aov.name == name and aov.is_valid, lambda _: Fault.of(BlenderReason.TAKEN, name, member=aov.name))
                .map(lambda _: None)
            )
            if answer.is_error() and name not in held:
                layer.aovs.remove(aov)
            return answer

        return accumulated(itertools.starmap(synced_aov, names.items())).map(lambda _: None)

    def layered(layer: bpy.types.ViewLayer) -> Outcome[None]:
        return (
            wanted(layer)
            .bind(lambda names: paired(aovs(layer, names), called(bpy.ops.scene.view_layer_add_used_lightgroups, {}, override=Some(bpy.context.temp_override(scene=scene, view_layer=layer)))))
            .map(lambda _: None)
        )

    return accumulated(layered(layer) for layer in scene.view_layers if layer.use).map(lambda _: None)


# --- [IDS]


def named(layer: bpy.types.ViewLayer, entity: Entity) -> tuple[tuple[str, bpy.types.ID, tuple[float, float, float]], ...]:
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
    return tuple((name, block, (red, green, blue)) for name, block in sorted(blocks.items()) for red, green, blue, *_ in (getattr(block, ENTITIES[entity].color.prop.identifier),))


def entity_image(scene: bpy.types.Scene, layer: bpy.types.ViewLayer, entity: Entity, rows: tuple[tuple[str, bpy.types.ID, tuple[float, float, float]], ...]) -> Outcome[Option[Expr]]:
    """Builds entity ID image of name colors under mattes summed into alpha, `Nothing` without names."""

    def matte(name: str) -> Expr:
        return Expr(
            kind=bpy.types.CompositorNodeCryptomatteV2,
            members=(("source", "RENDER"), ("scene", IdName.of(scene)), ("layer_name", f"{layer.name}.{ENTITIES[entity].pass_name}"), ("matte_id", name)),
            socket="Matte",
        )

    def image(covers: tuple[tuple[Expr, tuple[float, float, float]], ...]) -> Option[Expr]:
        match tuple(cover * (*color, 1.0) for cover, color in covers), tuple(cover for cover, _ in covers):
            case (first, *rest), (base, *others):
                return Some(Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", sum(rest, first)), ("Alpha", sum(others, base)), ("Type", "Replace Alpha")), socket="Image", shape="RGBA"))
            case _:
                return Nothing

    return accumulated(
        Ok((matte(name), color)) if "," not in name and name == name.strip() else Error(Fault.of(BlenderReason.TAKEN, name, member=f"{layer.name}.{ENTITIES[entity].pass_name}"))
        for name, _, color in rows
    ).map(image)


def colors_written(layer: bpy.types.ViewLayer, entity: Entity, chosen: Callable[[tuple[tuple[float, float, float], ...]], tuple[tuple[float, float, float], ...]]) -> Outcome[int]:
    """Writes chosen colors onto entity names, each block keeping its alpha."""
    rows = named(layer, entity)
    return accumulated(
        ENTITIES[entity].color.write(block, (*color, getattr(block, ENTITIES[entity].color.prop.identifier)[3])) for (_, block, _), color in zip(rows, chosen(tuple(color for _, _, color in rows)))
    ).map(lambda priors: sum(prior.is_some() for prior in priors))


def located(scene: bpy.types.Scene, matte: Matte, layer: str) -> Outcome[tuple[bpy.types.ViewLayer, Entity]]:
    """Returns view layer `layer` and the Cryptomatte kind its matte names after the layer prefix."""
    return paired(
        Option.of_optional(scene.view_layers.get(layer)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(scene.view_layers), member=layer)),
        Entity.named(matte.name.removeprefix(f"{layer}.")).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, matte.file.path, member=matte.name)),
    )


def exported(scene: bpy.types.Scene, file: Written, identity: Identity) -> Outcome[None]:
    """Writes one ID layer file per matte of a written multilayer file, one layer per matte name a scene block colors, refusing a path equal to source."""

    def painted(path: OutputPath, matte: Matte, layer: bpy.types.ViewLayer, entity: Entity) -> Outcome[str]:
        shown = {name: color for name, _, color in named(layer, entity)}
        known = {name: shown[name] for name in matte.ids if name in shown}
        return (
            path
            .written(file.layer, Output.MATTE, Some(ENTITIES[entity].pass_name), scene.frame_current, ".exr")
            .map(str)
            .filter_with(lambda target: target != file.path, lambda target: Fault.of(BlenderReason.TAKEN, target, member=file.path))
            .bind(
                lambda target: Multilayer.written(
                    target,
                    tuple(Pass(layer=name, channels=("R", "G", "B", "A"), role=PassRole.COLOR) for name in known),
                    PassLayout.PARTS,
                    Storage(depth=Depth.HALF, compression=Some(Deflate())),
                    (matte.file.parts[0].width, matte.file.parts[0].height),
                    stamped(identity),
                    lambda held: matte.filled(frozenset({held.layer}), np.asarray(known[held.layer])),
                )
            )
        )

    def matted(path: OutputPath, matte: Matte) -> Outcome[str]:
        return located(scene, matte, file.layer).bind(lambda pair: painted(path, matte, *pair))

    return (
        PassSet()
        .bound(identity)
        .bind(lambda routing: paired(routing.path(scene), Multilayer.read(file.path).bind(Matte.found)))
        .bind(lambda found: accumulated(matted(found[0], matte) for matte in found[1]))
        .map(lambda _: None)
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
        return routing.path(scene).bind(lambda path: routing.planned(scene, routing.held(scene), path).map(lambda planned: listed(layer, routing.routing.struct(layer), *planned)))

    return paired(PassSet().bound(identity), rooted(bpy.types.ViewLayer, context), rooted(bpy.types.Scene, context)).bind(lambda found: searched(*found))


# --- [COMPOSITION] ----------------------------------------------------------------------

ids_export = Finish(run=exported, kinds=frozenset({Kind.LAYERED}))
COMMANDS: Final[tuple[Operator[Empty] | Operator[PassName], ...]] = (
    Operator(form=Empty, run=passes_update),
    Operator(form=Empty, run=ids_spread),
    Operator(form=Empty, run=ids_shuffle),
    Operator(form=PassName, invoke=Some(Properties()), run=pass_file_add),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "COMMANDS",
    "DERIVED",
    "ENTITIES",
    "FAMILIES",
    "Derived",
    "DerivedLayer",
    "Entity",
    "Family",
    "FamilyPasses",
    "FileSocket",
    "FileSplit",
    "Filing",
    "IdPass",
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
    "colors_written",
    "entity_image",
    "exported",
    "filed",
    "filings",
    "flagged",
    "formatted",
    "grouped",
    "ids_export",
    "ids_shuffle",
    "ids_spread",
    "items",
    "lobe",
    "lobed",
    "located",
    "named",
    "pass_file_add",
    "pass_flags",
    "passes_update",
    "reading",
    "recolored",
    "reordered",
    "required",
    "sources",
    "turned",
    "unfiled",
]
