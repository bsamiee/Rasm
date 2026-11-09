"""Scene compositor effects: the effect a graph builder declares over its group interface and structure record with its operation facts and CPU stage, where its records come from, the entry record with its blend, and the entries a scene holds for it."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from enum import auto, Enum
from functools import partial
import inspect
from types import GenericAlias
from typing import ClassVar, Final, Generic, overload, TypeVar

import bpy
from expression import Option, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import OpenImageIO
import PyOpenColorIO

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import _acquired, Acquisitions, Declaration, Registrable
from rasm_bpy.data._ids.ownership import _retained, Mark, Ownership, Saved
from rasm_bpy.data._ids.store import IdName
from rasm_bpy.data._rna.animation import Animation, Driven, Keys
from rasm_bpy.data._rna.descriptors import Value, written
from rasm_bpy.data._rna.paths import _rna, Struct
from rasm_bpy.data._rna.properties import _titled
from rasm_bpy.nodes import functions
from rasm_bpy.nodes._addressing import InterfaceKeys
from rasm_bpy.nodes._desired import _OUTPUTS, End, INPUTS, item_key, Link, Node
from rasm_bpy.nodes._graphs import Expr, group_input
from rasm_bpy.nodes._interfaces import Interface, InterfacePanel, Socket
from rasm_bpy.nodes._trees import _endpoints, _links_of, _socket_at
from rasm_bpy.nodes.changes import made, NodeGroup, reconciled
from rasm_bpy.render._compositor import colors
from rasm_bpy.render._compositor.colors import _converted, _pointwise, COLOR_INPUT, COLOR_RESULT, with_alpha
from rasm_bpy.render._compositor.coordinates import PixelGrid, uv_mapped
from rasm_bpy.render._compositor.editing import _Arranger
from rasm_bpy.render._compositor.entries import _added_entry, _marked
from rasm_bpy.render._compositor.inputs import _FRAMING, DerivedInputs, entered, FRAME_ASPECT, FRAME_SCALE, GraphInputs, grouped, Holds, PreviewFrame, ResultOverBase
from rasm_bpy.render._compositor.masks import Blend, blended
from rasm_bpy.render._outputs.framing import FrameSize
from rasm_bpy.render._view_layers.passes import _required, _used, PassRequest
from rasm_bpy.settings._attachments import Stored
from rasm_bpy.ui._layout.rows import RowContent
from rasm_imaging import Estimate, Precision, Raster
from rasm_records import accumulated, Case, Error, Fault, Name, Nothing, Ok, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type Prepared = OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf] | Mapping[Key, Value] | tuple[Raster, Raster]
type _Graphed = tuple[Expr, Option[Expr]]


class Mix(Enum):
    """Entry input alpha, graph alpha, straight operation color, or replacement without blending."""

    INPUT = auto()
    RESULT = auto()
    STRAIGHT = auto()
    REPLACE = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_VIEWER: Final[Key] = "viewer"
_ENTRY: Final[Key] = "entry"

# --- [MODELS] ---------------------------------------------------------------------------


class Named(bpy.types.PropertyGroup):
    """Compositor entry selected by name."""

    name: bpy.props.StringProperty(name="Entry", description="Compositor entry name")  # ty: ignore[invalid-type-form]


class Stage(msgspec.Struct, frozen=True, kw_only=True):
    """Evaluated entry inputs, OCIO config, captured entering image and camera rasters, scene, and estimate precision for one CPU step."""

    identity: Identity
    values: Mapping[Key, Value]
    config: PyOpenColorIO.Config
    image: OpenImageIO.ImageBuf
    rasters: tuple[Raster, Raster]
    scene: bpy.types.Scene
    precision: Precision


class CompositorRecord(Case, frozen=True, kw_only=True):
    """Stored effect structure owning its graph and optional CPU and pass operations."""

    prepared_when: ClassVar[Socket | None] = None

    def graph(self, shell: GraphInputs) -> Outcome[Expr | ResultOverBase]:  # ruff: ignore[no-self-use]
        """Passes the entering image through a CPU-only effect."""
        return Ok(shell.color)

    @property
    def prepared(self) -> Callable[[Stage], Outcome[Option[Prepared]]] | None:
        """CPU preparation, absent for a native graph."""
        return None

    @property
    def requested(self) -> Callable[[str], Outcome[PassRequest]] | None:
        """Pass requirements, absent for a graph reading no render passes."""
        return None

    @property
    def operation_space(self) -> Name | None:
        """Operation colorspace, absent in the scene working space."""
        return None


M_co = TypeVar("M_co", bound=CompositorRecord, default=CompositorRecord, covariant=True)


class Entry(msgspec.Struct, Generic[M_co], frozen=True, kw_only=True):  # ruff: ignore[non-pep695-generic-class]
    """Compositor entry record with the blend of its result over its input."""

    record: M_co
    blend: Blend = Blend()


class Defaults(msgspec.Struct, frozen=True, kw_only=True):
    """Records start at the record's defaults."""

    @staticmethod
    def seeded(kind: type[CompositorRecord], _scene: bpy.types.Scene) -> Outcome[CompositorRecord]:
        """Returns the record's defaults."""
        return Ok(kind())


class Seeded(msgspec.Struct, frozen=True, kw_only=True):
    """Records start from values the scene holds."""

    seed: Callable[[bpy.types.Scene], CompositorRecord]

    def seeded(self, _kind: type[CompositorRecord], scene: bpy.types.Scene) -> Outcome[CompositorRecord]:
        """Returns the record the scene seeds."""
        return Ok(self.seed(scene))


class FromFile(msgspec.Struct, frozen=True, kw_only=True):
    """Records built from a chosen file, one entry per file, its extensions the file browser and drop handler filter by."""

    extensions: Callable[[], tuple[str, ...]]
    of: Callable[[str], CompositorRecord]

    @staticmethod
    def seeded(kind: type[CompositorRecord], _scene: bpy.types.Scene) -> Outcome[CompositorRecord]:
        """Refuses a record no file names."""
        return Error(Fault.of(Reason.ABSENT, item_key(kind), member="file"))


class CompositorEffect(msgspec.Struct, frozen=True, kw_only=True):
    """Scene compositor declaration of a behavior record, group interface, blend, source, scene requirements, and editor rows.

    Interface sockets hold native entry inputs, and the stored record owns the graph, operation space, pass request, and CPU preparation.
    Scene-derived inputs and animation, holds, checks, and registered rows compose independently of that record.
    """

    # --- [STATE]

    record: type[CompositorRecord]
    interface: Interface = Interface()
    mix: Mix = Mix.INPUT
    pointwise: bool = False
    derives: tuple[DerivedInputs, ...] = ()
    animates: Option[Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect], Outcome[tuple[Animation, ...]]]] = Nothing
    check: Option[Callable[[Identity, bpy.types.Scene, Option[bpy.types.SceneCompositorEffect]], Outcome[None]]] = Nothing
    source: Defaults | Seeded | FromFile = Defaults()
    holds: Holds = Holds()
    rows: tuple[RowContent, ...] = ()
    registers: tuple[Registrable, ...] = ()

    # --- [REGISTRATION]

    @property
    def kind(self) -> Key:
        """Record key naming ownerships and commands."""
        return item_key(self.record)

    @property
    def title(self) -> str:
        """Record class name in `titled` words naming entries, groups, and commands."""
        return _titled(self.record)

    @property
    def description(self) -> str:
        """Record documentation describing the effect's group and commands."""
        return Option.of_optional(inspect.getdoc(self.record)).default_value("")

    @property
    def framed(self) -> tuple[DerivedInputs, ...]:
        """Camera frame derivation every entry holds, then the effect's own."""
        return (_FRAMING, *self.derives)

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
        """Frame inputs and placement of the owned nodes each group builds."""
        return (*FrameSize.prerequisites, _Arranger())

    def seeded(self, scene: bpy.types.Scene) -> Outcome[CompositorRecord]:
        """Returns the record a new entry starts with from the effect's source."""
        return self.source.seeded(self.record, scene)

    def ownership(self, record: Entry[CompositorRecord]) -> Ownership[bpy.types.NodeTree]:
        """Selects the shared group for an unmasked record at the defaults its source starts from, the per-entry group otherwise."""
        shared, personal = self.ownerships
        return shared if not record.blend.masks and isinstance(self.source, Defaults) and record.record == self.record() else personal

    @staticmethod
    def stored[R: CompositorRecord](identity: Identity, form: type[R]) -> Stored[Entry[R]]:
        """Entry record codec on the entry's group."""
        return Stored.keyed(identity, _ENTRY, msgspec.msgpack.Decoder(GenericAlias(Entry, (form,))))

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns ownerships, registered rows, and what the editor rows hold."""
        return (*self.ownerships, *self.registers, *_acquired(self.rows))

    # --- [ENTRIES]

    def entries(self, identity: Identity, scene: bpy.types.Scene) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns the entries whose groups hold either ownership mark of the effect, in stack order."""
        keys = frozenset(owner.key for owner in self.ownerships)
        return tuple(entry for entry, mark in _marked(identity, scene) if mark.key in keys)

    def earliest(self, identity: Identity, scene: bpy.types.Scene) -> Option[bpy.types.SceneCompositorEffect]:
        """Returns the first entry of the effect in stack order."""
        keys = frozenset(owner.key for owner in self.ownerships)
        return Option.of_optional(next((entry for entry, mark in _marked(identity, scene) if mark.key in keys), None))

    @overload
    def read(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[Entry[CompositorRecord]]: ...
    @overload
    def read[R: CompositorRecord](self, identity: Identity, entry: bpy.types.SceneCompositorEffect, *, form: type[R]) -> Outcome[Entry[R]]: ...
    def read(self, identity: Identity, entry: bpy.types.SceneCompositorEffect, *, form: type[CompositorRecord] | None = None) -> Outcome[Entry[CompositorRecord]]:
        """Reads the entry record its group stores, absent before the group stores one."""
        codec = self.stored(identity, self.record if form is None else form)
        return grouped(entry).bind(lambda group: codec.read(group).bind(lambda held: held.to_result_with(partial(Fault.of, Reason.ABSENT, repr(group), member=codec.name))))

    @overload
    def current(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[Option[CompositorRecord]]: ...
    @overload
    def current[R: CompositorRecord](self, identity: Identity, scene: bpy.types.Scene, *, form: type[R]) -> Outcome[Option[R]]: ...
    def current(self, identity: Identity, scene: bpy.types.Scene, *, form: type[CompositorRecord] | None = None) -> Outcome[Option[CompositorRecord]]:
        """Reads earliest owned record, Nothing when absent."""
        return (
            self.earliest(identity, scene).map(lambda entry: self.read(identity, entry, form=self.record if form is None else form).map(lambda record: Some(record.record))).default_value(Ok(Nothing))
        )

    def named(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Finds named entry among the effect's owned groups."""
        return (
            catch(lambda: scene.compositor_effects[name], exception=KeyError)()
            .to_option()
            .filter(lambda entry: entry in self.entries(identity, scene))
            .to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.compositor_effects), member=name))
        )

    def appended(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[CompositorRecord],
        *,
        preview: Option[PreviewFrame] = Nothing,
        name: Option[Name] = Nothing,
        rasters: Option[tuple[Raster, Raster]] = Nothing,
    ) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Creates an entry of the effect at the stack end once its requirements and group succeed, named after the effect unless stated."""
        with ExitStack() as rollback:
            built = self.met(identity, scene, record.record, Nothing).bind(lambda request: self.made(identity, scene, record, PyOpenColorIO.GetCurrentConfig(), preview, request, rasters=rasters))
            return _retained(
                built.bind(
                    lambda held: _added_entry(rollback, scene, held[0], name=Some(name.default_value(self.title))).bind(
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
        record: Entry[CompositorRecord],
        *,
        preview: Option[PreviewFrame] = Nothing,
        source: Option[bpy.types.Scene] = Nothing,
        rasters: Option[tuple[Raster, Raster]] = Nothing,
    ) -> Outcome[bool]:
        """Reconciles group, then stores the entry record and writes derived inputs, answering whether any changed."""
        return self.met(identity, scene, record.record, Some(entry), source).bind(
            lambda request: self.made(identity, scene, record, PyOpenColorIO.GetCurrentConfig(), preview, request, Option.of_optional(entry.node_group), rasters=rasters).bind(
                lambda built: written(entry, ("node_group",), built[0]).bind(
                    lambda pointed: self.filled(identity, scene, entry, built[0], record, source).map(lambda filled: built[1] or pointed.is_some() or filled)
                )
            )
        )

    def revised[R: CompositorRecord](self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, change: Callable[[R], Outcome[R]], /, *, form: type[R]) -> Outcome[bool]:
        """Reads the entry's record and writes its changed record keeping the blend, answering whether the group or its store changed."""
        return self.read(identity, entry, form=form).bind(lambda held: change(held.record).bind(lambda record: self.write(identity, scene, entry, msgspec.structs.replace(held, record=record))))

    def refreshed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> Outcome[bool]:
        """Writes frame, derived values, and changed keyed channels, answering whether any changed."""
        return self.checked(identity, scene, Some(entry)).bind(lambda _: self.derive(identity, scene, entry))

    # --- [GROUPS]

    def grouped(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[CompositorRecord],
        config: PyOpenColorIO.Config,
        *,
        entry: Option[bpy.types.SceneCompositorEffect] = Nothing,
        preview: Option[PreviewFrame] = Nothing,
        staged: Option[IdName] = Nothing,
        rasters: Option[tuple[Raster, Raster]] = Nothing,
    ) -> Outcome[NodeGroup]:
        """Builds entry group from graph builder and blend, the image a CPU stage prepared entering the graph in place of the entry input when staged."""
        return self.met(identity, scene, record.record, entry).bind(lambda request: self.built(identity, scene, record, config, preview, request, staged, rasters=rasters))

    def checked(self, identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect]) -> Outcome[None]:
        """Runs the scene check of the entry."""
        return self.check.map(lambda check: check(identity, scene, entry)).default_value(Ok(None))

    def met(
        self, identity: Identity, scene: bpy.types.Scene, record: CompositorRecord, entry: Option[bpy.types.SceneCompositorEffect], source: Option[bpy.types.Scene] = Nothing
    ) -> Outcome[tuple[str, Option[PassRequest]]]:
        """Runs the scene check, then names the scene's first rendered view layer the group reads and makes its pass request."""
        layer = Block.of_seq(_used(scene)).try_head().map(lambda held: held.name).to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene.view_layers), member="used view layer"))
        request = Option.of_optional(record.requested)
        return paired(self.checked(identity, source.default_value(scene), entry), layer).bind(
            lambda pair: (
                request
                .map(lambda requested: requested(pair[1]).bind(lambda found: _required(scene, bpy.context, (found,)).map(lambda _: Some(found))))
                .default_value(Ok(Nothing))
                .map(lambda found: (pair[1], found))
            )
        )

    def built(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[CompositorRecord],
        config: PyOpenColorIO.Config,
        preview: Option[PreviewFrame],
        met: tuple[str, Option[PassRequest]],
        staged: Option[IdName] = Nothing,
        *,
        rasters: Option[tuple[Raster, Raster]] = Nothing,
        camera: Option[Raster] = Nothing,
    ) -> Outcome[NodeGroup]:
        """Builds the owned group of a record and blend over the met view layer and pass request, its interface the image, the effect's sockets, the hidden frame and derived sockets, and the blend panel of a blended effect.

        Graph reads the staged image in place of the entry input when staged, and a blended result mixes back over the base the graph answers, else over the entry input.

        Returns:
            `Ok` of the group. `Error` of graph, colorspace, and blend faults.
        """
        layer, request = met
        original = group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor)
        entering = staged.map(
            lambda image: uv_mapped(
                Expr(kind=bpy.types.CompositorNodeImage, members={"image": image}, socket="Image", socket_type=bpy.types.NodeSocketColor), PixelGrid.of(original).base, interpolation="Nearest"
            )
        ).default_value(original)
        space = Option.of_optional(record.record.operation_space).filter(lambda name: name != PyOpenColorIO.ROLE_SCENE_LINEAR)
        straight = self.mix is Mix.STRAIGHT
        derived = tuple(msgspec.structs.replace(socket, hide_in_modifier=True) for part in self.framed for socket in part.sockets)
        interface = Interface(
            outputs=(COLOR_RESULT,), inputs=(COLOR_INPUT, *self.interface.inputs, *derived), panels=(*self.interface.panels, *((record.blend.panel,) if self.mix is not Mix.REPLACE else ()))
        )
        walked = tuple(interface.walked())
        sockets = {
            **{key: item[0] for key, item, _, _ in walked if isinstance(item, tuple)},
            **{key: Socket(key=key, kind=bpy.types.NodeSocketBool) for key, item, _, _ in walked if isinstance(item, InterfacePanel) and item.toggle.is_some()},
        }
        render = Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(scene), "layer": layer}, socket="Image", socket_type=bpy.types.NodeSocketColor).keyed("render")
        kept = (render,) if request.filter(lambda held: bool(held.flags)).is_some() else ()

        def converting(key: Key, source: str, target: str, term: Expr) -> Outcome[Expr]:
            return _converted(key, config, source, target, term) if straight else _pointwise(f"{key}_scope", term, partial(_converted, key, config, source, target))

        def leaving(key: Key, term: Expr) -> Outcome[Expr]:
            return space.map(lambda name: converting(key, name, PyOpenColorIO.ROLE_SCENE_LINEAR, term)).default_value(Ok(term))

        def split(graphed: Expr | ResultOverBase) -> Outcome[_Graphed]:
            match graphed:
                case ResultOverBase(result=result, base=base):
                    return paired(leaving("space_out", result), leaving("space_base", base)).map(lambda parts: (parts[0], Some(parts[1])))
                case _:
                    return leaving("space_out", graphed).map(lambda result: (result, Nothing))

        def operation(source: Expr) -> Outcome[_Graphed]:
            shell = GraphInputs(
                identity=identity,
                config=config,
                color=source,
                render=render,
                sockets=sockets,
                camera=lambda: (
                    camera.or_else(rasters.map(lambda held: held[1])).map(Ok).default_with(lambda: FrameSize().bound(identity).bind(lambda frame: frame.rasters(scene)).map(lambda held: held[1]))
                ),
                rasters=rasters,
                preview=preview,
            )
            return (
                space
                .map(lambda name: converting("space_in", PyOpenColorIO.ROLE_SCENE_LINEAR, name, source))
                .default_value(Ok(source))
                .bind(lambda color: record.record.graph(msgspec.structs.replace(shell, color=color)))
                .bind(split)
            )

        def associated(parts: _Graphed) -> Outcome[_Graphed]:
            result, base = parts
            color = _pointwise("color", entering, lambda _: Ok(result))
            return (
                color.map(lambda held: (held, Nothing))
                if base.is_none()
                else paired(color, _pointwise("color_base", original, lambda _: Ok(base.some))).map(lambda held: (held[0], Option[Expr](some=held[1])))
            )

        def assembled(result: Expr) -> Outcome[NodeGroup]:
            ownership = self.ownership(record)
            return functions.built(ownership.key, interface, {COLOR_RESULT.key: result.keeping(*kept)}, bpy.types.CompositorNodeTree, name=Some(self.title)).map(
                lambda group: msgspec.structs.replace(group, single=ownership.single, graph=msgspec.structs.replace(group.graph, members={"description": self.description}))
            )

        def finished(parts: _Graphed) -> Outcome[NodeGroup]:
            result, base = parts
            match self.mix:
                case Mix.REPLACE:
                    return assembled(result)
                case Mix.INPUT | Mix.RESULT | Mix.STRAIGHT:
                    restored = with_alpha(result, original["Alpha"].keyed(item_key(COLOR_RESULT.key, "alpha_split"))).keyed(item_key(COLOR_RESULT.key, "alpha")) if self.mix is Mix.INPUT else result
                    frame = (original, group_input(FRAME_ASPECT.key), group_input(FRAME_SCALE.key))
                    return blended(record.blend, base.default_value(original), restored, frame, config).bind(assembled)

        return (operation(colors.straight(entering)).bind(associated) if straight else operation(entering)).bind(finished)

    def made(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[CompositorRecord],
        config: PyOpenColorIO.Config,
        preview: Option[PreviewFrame],
        met: tuple[str, Option[PassRequest]],
        tree: Option[bpy.types.NodeTree] = Nothing,
        *,
        rasters: Option[tuple[Raster, Raster]] = Nothing,
    ) -> Outcome[tuple[bpy.types.NodeTree, bool]]:
        """Reconciles the held group of the record's ownership, keeping an owned Viewer and its feed, else makes one, answering whether it changed."""

        def refreshed(group: NodeGroup, live: bpy.types.NodeTree, keys: Mapping[Key, str]) -> Outcome[tuple[bpy.types.NodeTree, bool]]:
            keyed = {node: mark.key for node in live.nodes for mark in Mark.read(identity, node).to_list()}
            viewers = tuple(node for node, key in keyed.items() if key == _VIEWER)
            semantic = {identifier: key for key, identifier in keys.items()}
            placed = group.graph.placed

            def endpoint(node: bpy.types.Node, socket: bpy.types.NodeSocket) -> End:
                return End(node=keyed[node], socket=semantic.get(socket.identifier, socket.identifier) if keyed[node] in {INPUTS, _OUTPUTS} else socket.identifier)

            def fed(socket: bpy.types.NodeSocket) -> tuple[Link, ...]:
                return tuple(
                    Link(source=endpoint(source, outlet), target=endpoint(target, inlet))
                    for link in _links_of(socket)
                    for source, outlet, target, inlet in (_endpoints(link),)
                    if source in keyed and keyed[source] in placed
                )

            graph = msgspec.structs.replace(
                group.graph,
                nodes={**group.graph.nodes, **({_VIEWER: Node(kind=bpy.types.CompositorNodeViewer)} if viewers else {})},
                links=(*group.graph.links, *(link for viewer in viewers for socket in _socket_at(viewer.inputs, "Image").to_option().to_list() for link in fed(socket))),
            )
            return reconciled(msgspec.structs.replace(group, graph=graph), live, identity).map(lambda changes: (live, any(changes)))

        own = self.ownership(record)
        owner = Some(scene)
        held = tree.or_else_with(lambda: Block.of_seq(own.held(identity)).filter(lambda pair: pair[1].of == owner).try_head().map(lambda pair: pair[0]) if own.single else Nothing)

        def adopted(group: NodeGroup, live: bpy.types.NodeTree) -> Outcome[tuple[bpy.types.NodeTree, bool]]:
            if Mark.read(identity, live).filter(lambda mark: mark.key == own.key and mark.of == owner).is_some():
                return refreshed(group, live, InterfaceKeys.of(live, identity).sockets)
            with ExitStack() as rollback:
                copied: bpy.types.NodeTree = live.copy()  # ty: ignore[invalid-assignment]
                return _retained(own.owned(identity, copied, of=owner, scope=Some(rollback)).bind(lambda copy: refreshed(group, copy, InterfaceKeys.of(copy, identity).sockets)), rollback)

        return self.built(identity, scene, record, config, preview, met, camera=rasters.map(lambda held: held[1])).bind(
            lambda group: held.map(partial(adopted, group)).default_with(lambda: made(group, identity, of=owner).map(lambda live: (live, True)))
        )

    def filled(
        self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree, record: Entry[CompositorRecord], source: Option[bpy.types.Scene] = Nothing
    ) -> Outcome[bool]:
        """Stores the entry record on its group when it differs, then writes derived inputs, answering whether any changed."""
        codec = self.stored(identity, self.record)
        stored = codec.read(group).bind(lambda held: Ok(value=False) if held == Some(record) else codec.write(group, record).map(lambda _: True))
        return stored.bind(lambda wrote: self.derive(identity, scene, entry, source).map(lambda refreshed: wrote or refreshed))

    def derive(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, source: Option[bpy.types.Scene] = Nothing) -> Outcome[bool]:
        """Writes the camera frame and derived values, then changed keyframes of keyed channels, refusing cycle-forming drivers, answering whether any changed."""

        def keyed(channel: Animation) -> Outcome[bool]:
            match channel.source:
                case Driven():
                    return Error(Fault.of(Reason.OUTSIDE, _rna(channel.path), Driven.__name__, member=Keys.__name__))
                case Keys():
                    return channel.apply(scene)

        values = accumulated(part.of(identity, source.default_value(scene), entry).bind(lambda found: entered(identity, entry, found)) for part in self.framed).map(any)
        channels = self.animates.map(lambda keys: keys(identity, scene, entry).bind(lambda found: accumulated(map(keyed, found)).map(any)))
        return accumulated((values, channels.default_value(Ok(value=False)))).map(any)


# --- [OPERATIONS] -----------------------------------------------------------------------


def precise(layer: str) -> Outcome[PassRequest]:
    """Requests full compositor precision with no render pass."""
    return Ok(PassRequest(layer=layer, precise=True))


def depth_precise(layer: str) -> Outcome[PassRequest]:
    """Requests the Depth pass at full compositor precision."""
    return Ok(PassRequest.depth(layer, precise=True))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CompositorEffect", "CompositorRecord", "Defaults", "Entry", "FromFile", "Mix", "Named", "Prepared", "Seeded", "Stage", "depth_precise", "precise"]
