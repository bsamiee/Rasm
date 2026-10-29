"""Scene compositor effects: the effect a graph builder declares over its group interface and structure record, where its records come from, and the entries a scene holds for it."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from enum import auto, Enum, StrEnum
from functools import partial
import inspect
from types import GenericAlias
from typing import Annotated, Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec
import PyOpenColorIO

from blender.bpy.data.ids.content import endpoints
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.animation import Animation
from blender.bpy.data.rna.descriptors import Descriptor, written
from blender.bpy.data.rna.paths import Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes import functions
from blender.bpy.nodes.addressing import Addressed, slot
from blender.bpy.nodes.changes import Changes, made, reconciled
from blender.bpy.nodes.desired import End, INPUTS, Link, Node, NodeGroup, OUTPUTS
from blender.bpy.nodes.graphs import Expr, group_input, Term
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, Socket
from blender.bpy.nodes.trees import links_of, socket_at
from blender.bpy.render.compositor.blending import blended, Entry
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT, converted, pointwise, with_alpha
from blender.bpy.render.compositor.inputs import applied_channel, Derives, entered, FRAME_ASPECT, FRAME_SCALE, FRAMING, grouped, Holds
from blender.bpy.render.compositor.shells import Graph, PreviewFrame, Shell
from blender.bpy.render.view_layers.passes import PassRequest, required, used
from blender.bpy.settings.attachments import Stored
from blender.imaging.color.processors import ColorSpace
from blender.records.faults import accumulated, Fault, Outcome, paired
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type EffectName = Annotated[str, msgspec.Meta(min_length=1)]
type Mix = Blended | Replaced
type Source[R] = Defaults | Seeded[R] | Opened[R]
type Request[R] = Callable[[R, str], Outcome[PassRequest]]
type Keying[R] = Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, R], Outcome[tuple[Animation, ...]]]
type Check[R] = Callable[[Identity, bpy.types.Scene, Option[bpy.types.SceneCompositorEffect], R, Option[PreviewFrame]], Outcome[None]]


class Alpha(Enum):
    """Alpha a blended result mixes back with: the entry input's, the graph's coverage, or straight color through the operation."""

    INPUT = auto()
    RESULT = auto()
    STRAIGHT = auto()


class Enabled(StrEnum):
    """Compositor effect enable flag an evaluation reads, valued by its RNA identifier."""

    RENDER = "enable_for_render"
    PREVIEW = "enable_for_preview"

    @property
    def prop(self) -> bpy.types.Property:
        """Owning RNA property."""
        return bpy.types.SceneCompositorEffect.bl_rna.properties[self.value]

    @property
    def descriptor(self) -> Descriptor:
        """Owning RNA property for reads and writes."""
        return Descriptor(root=bpy.types.SceneCompositorEffect, prop=self.prop)


# --- [CONSTANTS] ------------------------------------------------------------------------

PRECISION: Final = Descriptor.declared(bpy.types.Scene, ("render", "compositor_precision"), bpy.types.EnumProperty)
VIEWER: Final[Key] = "viewer"
_DESCRIPTION: Final = Descriptor.declared(bpy.types.CompositorNodeTree, ("description",), bpy.types.StringProperty)
_ENTRY: Final[Key] = "entry"

# --- [MODELS] ---------------------------------------------------------------------------


class Named(bpy.types.PropertyGroup):
    """Compositor entry selected by name."""

    name: bpy.props.StringProperty(name="Entry", description="Compositor entry name")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Defaults(msgspec.Struct, frozen=True, kw_only=True):
    """Records start at the record's defaults."""

    @staticmethod
    def seeded[K](kind: type[K], _scene: bpy.types.Scene) -> Outcome[K]:
        """Returns the record's defaults."""
        return Ok(kind())


class Seeded[R](msgspec.Struct, frozen=True, kw_only=True):
    """Records start from values the scene holds."""

    seed: Callable[[bpy.types.Scene], R]

    def seeded(self, _kind: type[R], scene: bpy.types.Scene) -> Outcome[R]:
        """Returns the record the scene seeds."""
        return Ok(self.seed(scene))


class Opened[R](msgspec.Struct, frozen=True, kw_only=True):
    """Records built from a chosen file, one entry per file, its extensions the file browser and drop handler filter by."""

    extensions: Callable[[], tuple[str, ...]]
    of: Callable[[str], R]

    @staticmethod
    def seeded[K: Case](kind: type[K], _scene: bpy.types.Scene) -> Outcome[K]:
        """Refuses a record no file names."""
        return Error(Fault.of(BlenderReason.ABSENT, item_key(kind), member="file"))


class Blended(msgspec.Struct, frozen=True, kw_only=True):
    """Result mixed back over a base through blend mode, opacity, and masks, the entry input unless a graph end in the operation's space, under the alpha it mixes with."""

    base: Option[End] = Nothing
    alpha: Alpha = Alpha.INPUT


class Replaced(msgspec.Struct, frozen=True, kw_only=True):
    """Result replacing the entry input with no blend inputs."""


class _Built[R: Case](msgspec.Struct, frozen=True):
    """Graph builder of an effect, the one positional member so `CompositorEffect(_graph, record=..., ...)` names it first."""

    graph: Graph[R]


class CompositorEffect[R: Case](_Built[R], frozen=True, kw_only=True):
    """Scene compositor effect of one structure record and its group interface, declared as `name: Final = CompositorEffect(_graph, record=..., ...)` over a private graph function.

    The interface sockets are the effect's parameters, the entry's inputs their values; the record holds what the group cannot, persisted on the group. The pass request, derived and keyed inputs,
    scene check, operation space, record source, scene holds, and registered rows decide what else it does.
    """

    record: type[R]
    interface: Interface = Interface()
    mix: Mix = Blended()
    request: Option[Request[R]] = Nothing
    derives: tuple[Derives[R], ...] = ()
    animates: Option[Keying[R]] = Nothing
    check: Option[Check[R]] = Nothing
    space: Option[Callable[[R], ColorSpace]] = Nothing
    source: Source[R] = Defaults()
    holds: Holds = Holds()
    registers: tuple[Registrable, ...] = ()

    # --- [DECLARATION]

    @property
    def kind(self) -> Key:
        """Record key naming ownerships and commands."""
        return item_key(self.record)

    @property
    def title(self) -> str:
        """Display name of the record key naming entries, groups, and commands."""
        return bpy.path.display_name(self.kind)

    @property
    def description(self) -> str:
        """Record documentation describing the effect's group and commands."""
        return Option.of_optional(inspect.getdoc(self.record)).default_value("")

    @property
    def framed(self) -> tuple[Derives[Case] | Derives[R], ...]:
        """Camera frame derivation every entry holds, then the effect's own."""
        return (FRAMING, *self.derives)

    @property
    def reads(self) -> tuple[tuple[type[Struct], bpy.types.Property], ...]:
        """RNA members derived inputs read."""
        return tuple(read for derives in self.framed for read in derives.reads)

    def seeded(self, scene: bpy.types.Scene) -> Outcome[R]:
        """Returns the record a new entry starts with from the effect's source."""
        return self.source.seeded(self.record, scene)

    def ownership(self, record: Entry[R]) -> Ownership[bpy.types.NodeTree]:
        """Selects the per-entry group for a masked, opened, or non-default record, the shared group otherwise."""
        shared, personal = ownerships(self.record)
        return personal if record.blend.masks or isinstance(self.source, Opened) or record.record != self.record() else shared

    def stored(self, identity: Identity) -> Outcome[Stored[Entry[R]]]:
        """Entry record codec on the entry's group."""
        return Stored.keyed(identity, _ENTRY, msgspec.msgpack.Decoder(GenericAlias(Entry, (self.record,))))

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns ownerships and registered rows."""
        return Ok((*ownerships(self.record), *self.registers))

    # --- [ENTRIES]

    def entries(self, identity: Identity, scene: bpy.types.Scene) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns owned entries of the effect in stack order."""
        return entries(identity, scene, self.record)

    def read(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[Entry[R]]:
        """Reads the entry record its group stores, absent before the group stores one."""
        return paired(self.stored(identity), grouped(entry)).bind(
            lambda pair: pair[0].read(pair[1]).bind(lambda held: held.to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(pair[1]), member=pair[0].name)))
        )

    def current(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[Option[R]]:
        """Reads earliest owned record, Nothing when absent."""
        return Block.of_seq(self.entries(identity, scene)).try_head().map(lambda entry: self.read(identity, entry).map(lambda record: Some(record.record))).default_value(Ok(Nothing))

    def named(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Finds named entry among the effect's owned groups."""
        return (
            Option
            .of_optional(scene.compositor_effects.get(name))
            .filter(lambda entry: entry in self.entries(identity, scene))
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(scene.compositor_effects), member=name))
        )

    def write(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        entry: bpy.types.SceneCompositorEffect,
        record: Entry[R],
        *,
        preview: Option[PreviewFrame] = Nothing,
        source: Option[bpy.types.Scene] = Nothing,
    ) -> Outcome[bool]:
        """Reconciles group, then stores the entry record and writes derived inputs, answering whether any changed."""
        return self.met(identity, scene, record.record, Some(entry), preview, source).bind(
            lambda request: self.made(identity, scene, record, PyOpenColorIO.GetCurrentConfig(), preview, request, Option.of_optional(entry.node_group)).bind(
                lambda built: written(entry, ("node_group",), built[0]).bind(
                    lambda pointed: self.filled(identity, scene, entry, built[0], record, source).map(lambda filled: built[1] or pointed.is_some() or filled)
                )
            )
        )

    def appended(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[R], *, preview: Option[PreviewFrame] = Nothing, name: Option[EffectName] = Nothing
    ) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Creates entry once requirements and group succeed."""

        def appended(group: bpy.types.NodeTree) -> Outcome[bpy.types.SceneCompositorEffect]:
            with ExitStack() as rollback:
                entry = scene.compositor_effects.new(name.default_value(self.title))
                rollback.callback(scene.compositor_effects.remove, entry)
                return (
                    accumulated((written(entry, ("node_group",), group), *(enabled.descriptor.write(entry, True) for enabled in Enabled)))
                    .bind(lambda _: self.filled(identity, scene, entry, group, record))
                    .map(lambda _: rollback.pop_all())
                    .map(lambda _: entry)
                )

        return self.met(identity, scene, record.record, Nothing, preview).bind(
            lambda request: self.made(identity, scene, record, PyOpenColorIO.GetCurrentConfig(), preview, request).bind(lambda built: appended(built[0]))
        )

    def refreshed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> Outcome[bool]:
        """Writes frame, derived values, and changed keyed channels, answering whether any changed."""
        return self.read(identity, entry).bind(lambda record: self.checked(identity, scene, Some(entry), record.record, Nothing).bind(lambda _: self.derive(identity, scene, entry, record.record)))

    # --- [GROUPS]

    def grouped(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[R],
        config: PyOpenColorIO.Config,
        *,
        entry: Option[bpy.types.SceneCompositorEffect] = Nothing,
        preview: Option[PreviewFrame] = Nothing,
    ) -> Outcome[NodeGroup]:
        """Builds entry group from graph builder and blend."""
        return self.met(identity, scene, record.record, entry, preview).bind(lambda request: self.built(identity, scene, record, config, preview, request))

    def checked(self, identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], record: R, preview: Option[PreviewFrame]) -> Outcome[None]:
        """Runs the scene check of the record."""
        return self.check.map(lambda check: check(identity, scene, entry, record, preview)).default_value(Ok(None))

    def met(
        self, identity: Identity, scene: bpy.types.Scene, record: R, entry: Option[bpy.types.SceneCompositorEffect], preview: Option[PreviewFrame], source: Option[bpy.types.Scene] = Nothing
    ) -> Outcome[tuple[str, Option[PassRequest]]]:
        """Runs the scene check, then names the scene's view layer the group reads and makes its pass request."""
        layer = layered(scene)
        return paired(self.checked(identity, source.default_value(scene), entry, record, preview), layer).bind(
            lambda pair: (
                self.request
                .map(lambda request: request(record, pair[1]).bind(lambda found: required(scene, bpy.context, (found,), PRECISION).map(lambda _: Some(found))))
                .default_value(Ok(Nothing))
                .map(lambda found: (pair[1], found))
            )
        )

    def faced(self, record: Entry[R]) -> Interface:
        """Entry group interface: image, the effect's sockets, hidden frame and derived sockets, and for blended effects the blend panel."""
        derived = tuple(msgspec.structs.replace(socket, hide_in_modifier=True) for part in self.framed for socket in part.sockets)
        return Interface(
            outputs=(COLOR_RESULT,), inputs=(COLOR_INPUT, *self.interface.inputs, *derived), panels=(*self.interface.panels, *((record.blend.panel,) if isinstance(self.mix, Blended) else ()))
        )

    def built(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[R], config: PyOpenColorIO.Config, preview: Option[PreviewFrame], met: tuple[str, Option[PassRequest]]
    ) -> Outcome[NodeGroup]:
        """Builds the owned group of a record and blend over the met view layer and pass request."""
        layer, request = met
        original = group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor)
        space = self.space.map(lambda part: part(record.record)).filter(lambda name: name != PyOpenColorIO.ROLE_SCENE_LINEAR)
        straight = isinstance(self.mix, Blended) and self.mix.alpha is Alpha.STRAIGHT
        face = self.faced(record)
        walked = tuple(face.walked())
        sockets = {
            **{key: item[0] for key, item, _, _ in walked if isinstance(item, tuple)},
            **{key: Socket(key=key, kind=bpy.types.NodeSocketBool) for key, item, _, _ in walked if isinstance(item, InterfacePanel) and item.toggle.is_some()},
        }
        render = Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(scene), "layer": layer}, socket="Image", shape=bpy.types.NodeSocketColor).named("render")
        kept = (render,) if request.filter(lambda held: bool(held.flags)).is_some() else ()

        def converting(key: Key, source: str, target: str) -> Callable[[Term], Outcome[Expr]]:
            def straightened(entering: Term) -> Outcome[Expr]:
                return converted(key, config, source, target, entering)

            return straightened if straight else partial(pointwise, f"{key}_scope", operation=straightened)

        def operation(entering: Expr) -> Outcome[Expr]:
            shell = Shell(record=record.record, identity=identity, config=config, color=entering, render=render, sockets=sockets, preview=preview)
            return (
                space
                .map(lambda name: converting("space_in", PyOpenColorIO.ROLE_SCENE_LINEAR, name)(entering))
                .default_value(Ok(entering))
                .bind(lambda source: self.graph(msgspec.structs.replace(shell, color=source)))
                .bind(lambda result: space.map(lambda name: converting("space_out", name, PyOpenColorIO.ROLE_SCENE_LINEAR)(result)).default_value(Ok(result)))
            )

        def assembled(result: Expr) -> Outcome[NodeGroup]:
            ownership = self.ownership(record)
            return functions.built(ownership.key, self.title, face, {COLOR_RESULT.key: result.keeping(*kept)}, bpy.types.CompositorNodeTree).map(
                lambda group: msgspec.structs.replace(group, single=ownership.single, graph=msgspec.structs.replace(group.graph, members={_DESCRIPTION.path: self.description}))
            )

        def finished(result: Expr) -> Outcome[NodeGroup]:
            match self.mix:
                case Replaced():
                    return assembled(result)
                case Blended(base=base, alpha=alpha):
                    restored = with_alpha(result, original["Alpha"].named(item_key(COLOR_RESULT.key, "alpha_split"))).named(item_key(COLOR_RESULT.key, "alpha")) if alpha is Alpha.INPUT else result
                    beneath: Outcome[Term] = base.map(lambda end: space.map(lambda name: converting("space_base", name, PyOpenColorIO.ROLE_SCENE_LINEAR)(end)).default_value(Ok(end))).default_value(
                        Ok(original)
                    )
                    frame = (original, group_input(FRAME_ASPECT.key), group_input(FRAME_SCALE.key))
                    return beneath.bind(lambda under: blended(record.blend, under, restored, frame, config)).bind(assembled)

        return (pointwise("color", original, operation) if straight else operation(original)).bind(finished)

    def made(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[R],
        config: PyOpenColorIO.Config,
        preview: Option[PreviewFrame],
        met: tuple[str, Option[PassRequest]],
        tree: Option[bpy.types.NodeTree] = Nothing,
    ) -> Outcome[tuple[bpy.types.NodeTree, bool]]:
        """Reconciles the held group of the record's ownership, keeping an owned Viewer and its feed, else makes one, answering whether it changed."""

        def refreshed(group: NodeGroup, live: bpy.types.NodeTree, keys: Mapping[Key, str]) -> Outcome[tuple[bpy.types.NodeTree, bool]]:
            marked = {node: mark.key for node in live.nodes for mark in Mark.read(identity, node).to_list()}
            viewers = tuple(node for node, key in marked.items() if key == VIEWER)
            semantic = {identifier: key for key, identifier in keys.items()}

            def endpoint(socket: bpy.types.NodeSocket) -> End:
                return End(node=marked[socket.node], socket=semantic.get(socket.identifier, socket.identifier) if marked[socket.node] in {INPUTS, OUTPUTS} else socket.identifier)  # type: ignore[index]  # ty: ignore[invalid-argument-type]

            def fed(socket: bpy.types.NodeSocket) -> tuple[Link, ...]:
                return tuple(
                    Link(source=endpoint(outlet), target=endpoint(inlet))
                    for link in links_of(socket)
                    for source, outlet, _, inlet in (endpoints(link),)
                    if source in marked and marked[source] in group.graph.placed
                )

            graph = msgspec.structs.replace(
                group.graph,
                nodes={**group.graph.nodes, **({VIEWER: Node(kind=bpy.types.CompositorNodeViewer)} if viewers else {})},
                links=(*group.graph.links, *(link for viewer in viewers for socket in socket_at(viewer.inputs, "Image").to_option().to_list() for link in fed(socket))),
            )
            return reconciled(msgspec.structs.replace(group, graph=graph), live, identity).map(lambda changes: (live, changes != Changes()))

        own = self.ownership(record)
        owner = Some(scene) if not own.single or met[1].filter(lambda held: bool(held.flags)).is_some() else Nothing
        held = tree.filter(lambda group: Mark.read(identity, group).filter(lambda mark: mark.key == own.key and mark.of == owner).is_some()).or_else_with(
            lambda: Block.of_seq(own.held(identity)).filter(lambda pair: pair[1].of == owner).try_head().map(lambda pair: pair[0]) if own.single else Nothing
        )
        return self.built(identity, scene, record, config, preview, met).bind(
            lambda group: held.map(lambda live: slot(identity).bind(lambda name: refreshed(group, live, Addressed.of(live, name).sockets))).default_with(
                lambda: made(group, identity, of=owner).map(lambda live: (live, True))
            )
        )

    def filled(
        self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree, record: Entry[R], source: Option[bpy.types.Scene] = Nothing
    ) -> Outcome[bool]:
        """Stores the entry record on its group when it differs, then writes derived inputs, answering whether any changed."""
        stored = self.stored(identity).bind(lambda codec: codec.read(group).bind(lambda held: Ok(False) if held == Some(record) else codec.write(group, record).map(lambda _: True)))
        return stored.bind(lambda wrote: self.derive(identity, scene, entry, record.record, source).map(lambda refreshed: wrote or refreshed))

    def derive(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, record: R, source: Option[bpy.types.Scene] = Nothing) -> Outcome[bool]:
        """Writes the camera frame and derived values, then keyed channels, answering whether any changed."""
        reading = source.default_value(scene)
        values = accumulated(part.of(identity, reading, entry, record).bind(lambda found: entered(identity, entry, found)) for part in self.framed).map(any)
        channels = self.animates.map(lambda keys: keys(identity, scene, entry, record).bind(lambda found: accumulated(applied_channel(channel, scene) for channel in found).map(any)))
        return accumulated((values, channels.default_value(Ok(False)))).map(any)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DECLARATION]


def precise(_record: Case, layer: str) -> Outcome[PassRequest]:
    """Requests full compositor precision with no render pass."""
    return Ok(PassRequest(layer=layer, precise=True))


def depth_precise(_record: Case, layer: str) -> Outcome[PassRequest]:
    """Requests the Depth pass at full compositor precision."""
    return Ok(PassRequest.depth(layer, precise=True))


def layered(scene: bpy.types.Scene) -> Outcome[str]:
    """Names the view layer an entry group of the scene reads, its first rendered layer."""
    return Block.of_seq(used(scene)).try_head().map(lambda layer: layer.name).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(scene.view_layers), member="used view layer"))


# --- [ENTRY_EDITS]


def revised[R: Case](effect: CompositorEffect[R], identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, change: Callable[[R], Outcome[R]]) -> Outcome[bool]:
    """Reads entry's record, writes its changed record keeping the blend, answering whether the group or its store changed."""
    return effect.read(identity, entry).bind(lambda held: change(held.record).bind(lambda record: effect.write(identity, scene, entry, msgspec.structs.replace(held, record=record))))


# --- [ENTRY_HOLDERS]


def ownerships(record: type[Case]) -> tuple[Ownership[bpy.types.NodeTree], Ownership[bpy.types.NodeTree]]:
    """Declares shared and per-entry group ownerships."""
    key = item_key(record)
    return Ownership(kind=bpy.types.NodeTree, key=key, saved=Saved.USED), Ownership(kind=bpy.types.NodeTree, key=f"{key}_entry", saved=Saved.USED, single=False)


def marked(identity: Identity, scene: bpy.types.Scene) -> tuple[tuple[bpy.types.SceneCompositorEffect, Mark], ...]:
    """Returns the scene entries whose groups carry an extension ownership mark, with that mark, in stack order."""
    return tuple((entry, mark) for entry in scene.compositor_effects for group in Option.of_optional(entry.node_group).to_list() for mark in Mark.read(identity, group).to_list())


def entries(identity: Identity, scene: bpy.types.Scene, record: type[Case]) -> tuple[bpy.types.SceneCompositorEffect, ...]:
    """Returns entries with either ownership mark of record in stack order."""
    keys = frozenset(owner.key for owner in ownerships(record))
    return tuple(entry for entry, mark in marked(identity, scene) if mark.key in keys)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "PRECISION",
    "VIEWER",
    "Alpha",
    "Blended",
    "Check",
    "CompositorEffect",
    "Defaults",
    "EffectName",
    "Enabled",
    "Keying",
    "Mix",
    "Named",
    "Opened",
    "Replaced",
    "Request",
    "Seeded",
    "Source",
    "depth_precise",
    "entries",
    "layered",
    "marked",
    "ownerships",
    "precise",
    "revised",
]
