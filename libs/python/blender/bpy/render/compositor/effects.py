"""Scene compositor effects: the effect a graph builder declares over its group interface and structure record with its operation facts and CPU stage, where its records come from, the entry record with its blend, and the entries a scene holds for it."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from enum import auto, Enum
from functools import partial
import inspect
from types import GenericAlias
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.ownership import Mark, Ownership, retained, Saved
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.animation import Animation, Driven, Keys
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.mirrors import titled
from blender.bpy.data.rna.paths import rna, Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Declaration, Registrable
from blender.bpy.nodes import functions
from blender.bpy.nodes.addressing import Addressed
from blender.bpy.nodes.changes import made, reconciled
from blender.bpy.nodes.desired import End, INPUTS, item_key, Link, Node, OUTPUTS
from blender.bpy.nodes.editing import Arranger
from blender.bpy.nodes.graphs import Expr, group_input
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, NodeGroup, Socket
from blender.bpy.nodes.trees import endpoints, links_of, socket_at
from blender.bpy.render.compositor import colors
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT, converted, with_alpha
from blender.bpy.render.compositor.inputs import Based, CameraFrame, Derives, entered, FRAME_ASPECT, FRAME_SCALE, FRAMING, Graph, grouped, Holds, PreviewFrame, Shell
from blender.bpy.render.compositor.masks import Blend, blended
from blender.bpy.render.compositor.placement import added_entry
from blender.bpy.render.view_layers.passes import PassRequest, required, used
from blender.bpy.settings.attachments import Stored
from blender.bpy.ui.layout.rows import acquired, RowContent
from blender.imaging.sampling.integration import Estimate, Precision
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case, Name

# --- [TYPES] ----------------------------------------------------------------------------

type Prepared = OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value]
type _Graphed = tuple[Expr, Option[Expr]]


class Alpha(Enum):
    """Alpha a blended result mixes back with: the entry input's, the graph's coverage, or straight color through the operation."""

    INPUT = auto()
    RESULT = auto()
    STRAIGHT = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

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


class FromFile[R](msgspec.Struct, frozen=True, kw_only=True):
    """Records built from a chosen file, one entry per file, its extensions the file browser and drop handler filter by."""

    extensions: Callable[[], tuple[str, ...]]
    of: Callable[[str], R]

    @staticmethod
    def seeded[K: Case](kind: type[K], _scene: bpy.types.Scene) -> Outcome[K]:
        """Refuses a record no file names."""
        return Error(Fault.of(Reason.ABSENT, item_key(kind), member="file"))


class Blended(msgspec.Struct, frozen=True, kw_only=True):
    """Result mixed back over its base through blend mode, opacity, and masks under the alpha it mixes with, the base the entry input unless the graph answers one."""

    alpha: Alpha = Alpha.INPUT


class Replaced(msgspec.Struct, frozen=True, kw_only=True):
    """Result replacing the entry input with no blend inputs."""


class Entry[M: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Compositor entry record with the blend of its result over its input."""

    record: M
    blend: Blend = Blend()


class Stage[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """What one entry's CPU step reads: its record, evaluated input values by item key, the OCIO config, the entering image its captured prefix rendered, its camera frame, the evaluated scene, and the precision an estimate holds."""

    record: R
    identity: Identity
    values: Mapping[Key, Value]
    config: PyOpenColorIO.Config
    image: OpenImageIO.ImageBuf
    frame: CameraFrame
    scene: bpy.types.Scene
    precision: Precision


class CompositorEffect[R: Case](msgspec.Struct, frozen=True):
    """Scene compositor effect of one structure record and its group interface, declared as `name: Final = CompositorEffect(_graph, record=..., ...)` over a private graph function.

    The interface sockets are the effect's parameters, the entry's inputs their values; the record holds what the group cannot, persisted on the group. `pointwise` states that each result pixel
    reads its entering pixel alone, and `prepared` the CPU step a render runs before the graph, the image it answers entering the graph as `Shell.color`. The pass request, derived and keyed
    inputs, scene check, operation space, record source, scene holds, rows the active entry editor draws, and registered rows decide what else it does; derivations over any record, such as `motion.timing`, join the record's own.
    """

    graph: Graph[R]
    record: type[R]
    interface: Interface = Interface()
    mix: Blended | Replaced = Blended()
    pointwise: bool = False
    prepared: Option[Callable[[Stage[R]], Outcome[Option[Prepared]]]] = Nothing
    request: Option[Callable[[R, str], Outcome[PassRequest]]] = Nothing
    derives: tuple[Derives[R] | Derives[Case], ...] = ()
    animates: Option[Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, R], Outcome[tuple[Animation, ...]]]] = Nothing
    check: Option[Callable[[Identity, bpy.types.Scene, Option[bpy.types.SceneCompositorEffect], R, Option[PreviewFrame]], Outcome[None]]] = Nothing
    space: Option[Callable[[R], Name]] = Nothing
    source: Defaults | Seeded[R] | FromFile[R] = Defaults()
    holds: Holds = Holds()
    rows: tuple[RowContent, ...] = ()
    registers: tuple[Registrable, ...] = ()

    # --- [DECLARATION]

    @property
    def kind(self) -> Key:
        """Record key naming ownerships and commands."""
        return item_key(self.record)

    @property
    def title(self) -> str:
        """Record class name in `titled` words naming entries, groups, and commands."""
        return titled(self.record)

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

    @property
    def ownerships(self) -> tuple[Ownership[bpy.types.NodeTree], Ownership[bpy.types.NodeTree]]:
        """Shared group ownership, then the per-entry one."""
        return Ownership(kind=bpy.types.NodeTree, key=self.kind, saved=Saved.USED), Ownership(kind=bpy.types.NodeTree, key=f"{self.kind}_entry", saved=Saved.USED, single=False)

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Arranger placing the owned nodes of each group the effect builds."""
        return (Arranger(),)

    def seeded(self, scene: bpy.types.Scene) -> Outcome[R]:
        """Returns the record a new entry starts with from the effect's source."""
        return self.source.seeded(self.record, scene)

    def ownership(self, record: Entry[R]) -> Ownership[bpy.types.NodeTree]:
        """Selects the shared group for an unmasked record at the defaults its source starts from, the per-entry group otherwise."""
        shared, personal = self.ownerships
        return shared if not record.blend.masks and isinstance(self.source, Defaults) and record.record == self.record() else personal

    def stored(self, identity: Identity) -> Outcome[Stored[Entry[R]]]:
        """Entry record codec on the entry's group."""
        return Stored.keyed(identity, _ENTRY, msgspec.msgpack.Decoder(GenericAlias(Entry, (self.record,))))

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns ownerships, registered rows, and what the editor rows hold."""
        return Ok((*self.ownerships, *self.registers, *acquired(self.rows)))

    # --- [ENTRIES]

    def entries(self, identity: Identity, scene: bpy.types.Scene) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns the entries whose groups carry either ownership mark of the effect, in stack order."""
        keys = frozenset(owner.key for owner in self.ownerships)
        return tuple(
            entry for entry in scene.compositor_effects for group in Option.of_optional(entry.node_group).to_list() if Mark.read(identity, group).filter(lambda mark: mark.key in keys).is_some()
        )

    def read(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[Entry[R]]:
        """Reads the entry record its group stores, absent before the group stores one."""
        return paired(self.stored(identity), grouped(entry)).bind(
            lambda pair: pair[0].read(pair[1]).bind(lambda held: held.to_result_with(partial(Fault.of, Reason.ABSENT, repr(pair[1]), member=pair[0].name)))
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
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=name))
        )

    def appended(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[R], *, preview: Option[PreviewFrame] = Nothing, name: Option[Name] = Nothing
    ) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Creates an entry of the effect at the stack end once its requirements and group succeed, named after the effect unless stated."""
        with ExitStack() as rollback:
            built = self.met(identity, scene, record.record, Nothing, preview).bind(lambda request: self.made(identity, scene, record, PyOpenColorIO.GetCurrentConfig(), preview, request))
            return retained(
                built.bind(
                    lambda held: added_entry(rollback, scene, held[0], name=Some(name.default_value(self.title))).bind(
                        lambda entry: self.filled(identity, scene, entry, held[0], record).map(lambda _: entry)
                    )
                ),
                rollback,
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

    def revised(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, change: Callable[[R], Outcome[R]], /) -> Outcome[bool]:
        """Reads the entry's record and writes its changed record keeping the blend, answering whether the group or its store changed."""
        return self.read(identity, entry).bind(lambda held: change(held.record).bind(lambda record: self.write(identity, scene, entry, msgspec.structs.replace(held, record=record))))

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
        staged: Option[IdName] = Nothing,
    ) -> Outcome[NodeGroup]:
        """Builds entry group from graph builder and blend, the image a CPU stage prepared entering the graph in place of the entry input when staged."""
        return self.met(identity, scene, record.record, entry, preview).bind(lambda request: self.built(identity, scene, record, config, preview, request, staged))

    def checked(self, identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], record: R, preview: Option[PreviewFrame]) -> Outcome[None]:
        """Runs the scene check of the record."""
        return self.check.map(lambda check: check(identity, scene, entry, record, preview)).default_value(Ok(None))

    def met(
        self, identity: Identity, scene: bpy.types.Scene, record: R, entry: Option[bpy.types.SceneCompositorEffect], preview: Option[PreviewFrame], source: Option[bpy.types.Scene] = Nothing
    ) -> Outcome[tuple[str, Option[PassRequest]]]:
        """Runs the scene check, then names the scene's first rendered view layer the group reads and makes its pass request."""
        layer = Block.of_seq(used(scene)).try_head().map(lambda held: held.name).to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.view_layers), member="used view layer"))
        return paired(self.checked(identity, source.default_value(scene), entry, record, preview), layer).bind(
            lambda pair: (
                self.request
                .map(lambda request: request(record, pair[1]).bind(lambda found: required(scene, bpy.context, (found,)).map(lambda _: Some(found))))
                .default_value(Ok(Nothing))
                .map(lambda found: (pair[1], found))
            )
        )

    def built(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[R],
        config: PyOpenColorIO.Config,
        preview: Option[PreviewFrame],
        met: tuple[str, Option[PassRequest]],
        staged: Option[IdName] = Nothing,
    ) -> Outcome[NodeGroup]:
        """Builds the owned group of a record and blend over the met view layer and pass request, its interface the image, the effect's sockets, the hidden frame and derived sockets, and the blend panel of a blended effect.

        The graph reads the staged image in place of the entry input when staged, and a blended result mixes back over the base the graph answers, else over the entry input.

        Returns:
            `Ok` of the group. `Error` of graph, colorspace, and blend faults.
        """
        layer, request = met
        original = group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor)
        entering = staged.map(lambda image: Expr(kind=bpy.types.CompositorNodeImage, members={"image": image}, socket="Image", shape=bpy.types.NodeSocketColor)).default_value(original)
        space = self.space.map(lambda part: part(record.record)).filter(lambda name: name != PyOpenColorIO.ROLE_SCENE_LINEAR)
        straight = isinstance(self.mix, Blended) and self.mix.alpha is Alpha.STRAIGHT
        derived = tuple(msgspec.structs.replace(socket, hide_in_modifier=True) for part in self.framed for socket in part.sockets)
        face = Interface(
            outputs=(COLOR_RESULT,), inputs=(COLOR_INPUT, *self.interface.inputs, *derived), panels=(*self.interface.panels, *((record.blend.panel,) if isinstance(self.mix, Blended) else ()))
        )
        walked = tuple(face.walked())
        sockets = {
            **{key: item[0] for key, item, _, _ in walked if isinstance(item, tuple)},
            **{key: Socket(key=key, kind=bpy.types.NodeSocketBool) for key, item, _, _ in walked if isinstance(item, InterfacePanel) and item.toggle.is_some()},
        }
        render = Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(scene), "layer": layer}, socket="Image", shape=bpy.types.NodeSocketColor).named("render")
        kept = (render,) if request.filter(lambda held: bool(held.flags)).is_some() else ()

        def converting(key: Key, source: str, target: str, term: Expr) -> Outcome[Expr]:
            return converted(key, config, source, target, term) if straight else colors.pointwise(f"{key}_scope", term, partial(converted, key, config, source, target))

        def leaving(key: Key, term: Expr) -> Outcome[Expr]:
            return space.map(lambda name: converting(key, name, PyOpenColorIO.ROLE_SCENE_LINEAR, term)).default_value(Ok(term))

        def split(graphed: Expr | Based) -> _Graphed:
            match graphed:
                case Based(result=result, base=base):
                    return result, Some(base)
                case _:
                    return graphed, Nothing

        def operation(source: Expr) -> Outcome[_Graphed]:
            shell = Shell(record=record.record, identity=identity, config=config, color=source, render=render, sockets=sockets, preview=preview)
            return (
                space
                .map(lambda name: converting("space_in", PyOpenColorIO.ROLE_SCENE_LINEAR, name, source))
                .default_value(Ok(source))
                .bind(lambda color: self.graph(msgspec.structs.replace(shell, color=color)))
                .map(split)
                .bind(lambda parts: paired(leaving("space_out", parts[0]), parts[1].map(lambda base: leaving("space_base", base).map(Some)).default_value(Ok(Nothing))))
            )

        def associated(parts: _Graphed) -> Outcome[_Graphed]:
            result, base = parts
            return paired(
                colors.pointwise("color", entering, lambda _: Ok(result)), base.map(lambda held: colors.pointwise("color_base", original, lambda _: Ok(held)).map(Some)).default_value(Ok(Nothing))
            )

        def assembled(result: Expr) -> Outcome[NodeGroup]:
            ownership = self.ownership(record)
            return functions.built(ownership.key, face, {COLOR_RESULT.key: result.keeping(*kept)}, bpy.types.CompositorNodeTree, name=Some(self.title)).map(
                lambda group: msgspec.structs.replace(group, single=ownership.single, graph=msgspec.structs.replace(group.graph, members={_DESCRIPTION.path: self.description}))
            )

        def finished(parts: _Graphed) -> Outcome[NodeGroup]:
            result, base = parts
            match self.mix:
                case Replaced():
                    return assembled(result)
                case Blended(alpha=alpha):
                    restored = with_alpha(result, original["Alpha"].named(item_key(COLOR_RESULT.key, "alpha_split"))).named(item_key(COLOR_RESULT.key, "alpha")) if alpha is Alpha.INPUT else result
                    frame = (original, group_input(FRAME_ASPECT.key), group_input(FRAME_SCALE.key))
                    return blended(record.blend, base.default_value(original), restored, frame, config).bind(assembled)

        return (operation(colors.straight(entering)).bind(associated) if straight else operation(entering)).bind(finished)

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
            return reconciled(msgspec.structs.replace(group, graph=graph), live, identity).map(lambda changes: (live, any(changes)))

        own = self.ownership(record)
        owner = Some(scene) if not own.single or met[1].filter(lambda held: bool(held.flags)).is_some() else Nothing
        held = tree.filter(lambda group: Mark.read(identity, group).filter(lambda mark: mark.key == own.key and mark.of == owner).is_some()).or_else_with(
            lambda: Block.of_seq(own.held(identity)).filter(lambda pair: pair[1].of == owner).try_head().map(lambda pair: pair[0]) if own.single else Nothing
        )
        return self.built(identity, scene, record, config, preview, met).bind(
            lambda group: held.map(lambda live: Addressed.of(live, identity).bind(lambda addressed: refreshed(group, live, addressed.sockets))).default_with(
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
        """Writes the camera frame and derived values, then changed keyframes of keyed channels, refusing cycle-forming drivers, answering whether any changed."""

        def keyed(channel: Animation) -> Outcome[bool]:
            match channel.source:
                case Driven():
                    return Error(Fault.of(Reason.OUTSIDE, rna(channel.path), Driven.__name__, member=Keys.__name__))
                case Keys():
                    return channel.apply(scene)

        values = accumulated(part.of(identity, source.default_value(scene), entry, record).bind(lambda found: entered(identity, entry, found)) for part in self.framed).map(any)
        channels = self.animates.map(lambda keys: keys(identity, scene, entry, record).bind(lambda found: accumulated(map(keyed, found)).map(any)))
        return accumulated((values, channels.default_value(Ok(False)))).map(any)


# --- [OPERATIONS] -----------------------------------------------------------------------


def precise(_record: Case, layer: str) -> Outcome[PassRequest]:
    """Requests full compositor precision with no render pass."""
    return Ok(PassRequest(layer=layer, precise=True))


def depth_precise(_record: Case, layer: str) -> Outcome[PassRequest]:
    """Requests the Depth pass at full compositor precision."""
    return Ok(PassRequest.depth(layer, precise=True))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["VIEWER", "Alpha", "Blended", "CompositorEffect", "Defaults", "Entry", "FromFile", "Named", "Prepared", "Replaced", "Seeded", "Stage", "depth_precise", "precise"]
