"""Scene compositor effects: the effect declared over its group interface and structure record with its parts, the graph context its builder reads, and the entries a scene holds for it."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from enum import auto, Enum
import inspect
from types import GenericAlias
from typing import Annotated, ClassVar, Final, Literal, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.animation import Animation, Driven, Keys
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Descriptor, rooted, Struct, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.changes import Changes, made, reconciled
from blender.bpy.nodes.graphs import DataType, End, Expr, group_input, INPUTS, Link, menu, Node, NodeGroup, OUTPUTS, Term
from blender.bpy.nodes.interface import addresses, Interface, InterfacePanel, item_key, slot, Socket, under
from blender.bpy.nodes.trees import socket_at
from blender.bpy.render.compositor.blending import Entry, mixed_back
from blender.bpy.render.compositor.colors import converted, pointwise
from blender.bpy.render.compositor.coordinates import framed, FrameSpace, SHUTTER, Shutter
from blender.bpy.render.compositor.inputs import entry_input
from blender.bpy.render.compositor.masks import coverage
from blender.bpy.render.outputs.framing import Framed, FrameSize, Framing
from blender.bpy.render.view_layers.passes import PassRequest, required
from blender.bpy.settings.attachments import Stored
from blender.imaging.color.processors import ColorSpace, Domain
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type EffectName = Annotated[str, msgspec.Meta(min_length=1)]
type Context = Option[tuple[tuple[int, int], Framing]]
type Restoring = Callable[[], Outcome[None]]
type Part[R] = Requests[R] | Derives[R] | Animates[R] | Checks[R] | Conditions[R] | Spaces[R] | Opens[R] | Seeds[R] | Holds | Registers
type Mix = Blended | Replaced


class Alpha(Enum):
    """Alpha of an effect's result: the entry input's, the graph's coverage, or straight color through the operation."""

    INPUT = auto()
    RESULT = auto()
    STRAIGHT = auto()


class Enabled(Enum):
    """Compositor effect enable flag an evaluation reads."""

    RENDER = bpy.types.SceneCompositorEffect.bl_rna.properties["enable_for_render"]
    PREVIEW = bpy.types.SceneCompositorEffect.bl_rna.properties["enable_for_preview"]

    @property
    def descriptor(self) -> Descriptor:
        """Owning RNA property for reads and writes."""
        return Descriptor(root=bpy.types.SceneCompositorEffect, prop=self.value)


# --- [CONSTANTS] ------------------------------------------------------------------------

DESCRIPTION: Final = Descriptor.declared(bpy.types.CompositorNodeTree, "description", bpy.types.StringProperty)
PRECISION: Final = Descriptor.declared(bpy.types.Scene, "render.compositor_precision", bpy.types.EnumProperty)
FRAME: Final = tuple(
    Socket(key=f"frame_{name}", name=bpy.path.display_name(name), kind=bpy.types.NodeSocketFloat, description=description, members={"hide_in_modifier": True, "default_value": default})
    for name, description, default in (("aspect", "Pixel aspect", 1.0), ("scale", "Camera share of overscan", 1.0), ("meters", "Meters per Blender unit", 1.0))
)

# --- [MODELS] ---------------------------------------------------------------------------


class Named(bpy.types.PropertyGroup):
    """Compositor entry selected by name."""

    name: bpy.props.StringProperty(name="Entry", description="Compositor entry name")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class CameraFrame(msgspec.Struct, frozen=True, kw_only=True):
    """Pixel aspect, camera share of overscan, and meters per Blender unit, held by the hidden frame inputs."""

    aspect: float = 1.0
    scale: float = 1.0
    meters: float

    @property
    def values(self) -> dict[Key, Value]:
        """Frame input values by socket key."""
        return {"frame_aspect": self.aspect, "frame_scale": self.scale, "frame_meters": self.meters}

    @classmethod
    def read(cls, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[Self]:
        """Reads the frame inputs of an entry."""
        return accumulated(
            entry_input(identity, entry, socket.key).bind(
                lambda held: Descriptor.resolve(held, "value", bpy.types.FloatProperty).map(lambda descriptor: (socket.key.removeprefix("frame_"), descriptor.read(held)))
            )
            for socket in FRAME
        ).bind(
            lambda values: catch(msgspec.convert, exception=msgspec.ValidationError)(dict(values), cls).map_error(
                lambda error: (Fault(reason=RecordReason.UNREADABLE, subject=repr(entry), cause=error),)
            )
        )

    def window(self, spec: OpenImageIO.ImageSpec) -> tuple[tuple[float, float, float, float], tuple[float, float]]:
        """Maps captured data window into original camera coordinates."""
        greater = max(spec.full_width * self.aspect, spec.full_height) * self.scale
        margin = (1.0 - self.scale) / 2.0
        return (
            (
                (spec.x - spec.full_x - margin * spec.full_width) * self.aspect / greater,
                (spec.full_y + spec.full_height - spec.y - spec.height - margin * spec.full_height) / greater,
                (spec.x + spec.width - spec.full_x - margin * spec.full_width) * self.aspect / greater,
                (spec.full_y + spec.full_height - spec.y - margin * spec.full_height) / greater,
            ),
            (spec.full_width * self.scale * self.aspect / greater, spec.full_height * self.scale / greater),
        )


class Marks(msgspec.Struct, frozen=True, kw_only=True):
    """Scene entries whose groups carry an extension ownership mark, in stack order, each mark read once."""

    held: tuple[tuple[bpy.types.SceneCompositorEffect, Mark], ...]

    @classmethod
    def of(cls, identity: Identity, scene: bpy.types.Scene) -> Self:
        """Reads each entry group's mark once."""
        return cls(held=tuple((entry, mark) for entry in scene.compositor_effects for group in Option.of_optional(entry.node_group).to_list() for mark in Mark.read(identity, group).to_list()))

    def keyed(self, *keys: Key) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns entries marked by any of keys."""
        return tuple(entry for entry, mark in self.held if mark.key in keys)


class Shell[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Graph builder's record, entry input color, render layer, and group sockets by key."""

    record: R
    identity: Identity
    config: PyOpenColorIO.Config
    color: Expr
    render: Expr
    sockets: Mapping[Key, Socket]
    context: Context = Nothing
    image: ClassVar[Key] = "image"
    result: ClassVar[Key] = "result"
    viewer: ClassVar[Key] = "viewer"
    blend: ClassVar[Key] = "blend"
    frame: ClassVar[Key] = "frame"
    entry: ClassVar[Key] = "entry"

    def input(self, *path: Key | int | type[Case]) -> Expr:
        """Reads the group input of an interface item key path, in the shape its socket kind takes, a panel key reading its toggle."""
        key = item_key(*path)
        match Option.of_optional(self.sockets.get(key)).map(lambda socket: socket.kind).to_optional():
            case bpy.types.NodeSocketColor:
                return group_input(key, "RGBA")
            case bpy.types.NodeSocketVector:
                return group_input(key, "VECTOR")
            case _:
                return group_input(key)

    @property
    def space(self) -> FrameSpace:
        """Camera frame of the entry input."""
        return framed(self.color, self.input(Shell.frame, "aspect"), self.input(Shell.frame, "scale"))

    def layer(self, socket: str) -> Expr:
        """Reads one Render Layers output of the effect's pass request layer."""
        return msgspec.structs.replace(self.render, socket=socket)

    def menu(self, *path: Key | int | type[Case], cases: Mapping[str, Term], kind: DataType | Literal["MENU", "BOOLEAN"]) -> Outcome[Expr]:
        """Switches between case terms by item name at a menu socket's input, absent for a key holding no menu socket."""
        key = item_key(*path)
        return (
            Option
            .of_optional(self.sockets.get(key))
            .bind(lambda socket: socket.items)
            .map(lambda items: menu(self.input(*path), items, cases, kind=kind))
            .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=self.identity.package, member=key),))
        )


class Requests[R](msgspec.Struct, frozen=True, kw_only=True):
    """Render passes and compositor precision the graph reads from the named view layer."""

    request: Callable[[R, str], Outcome[PassRequest]]


class Derives[R](msgspec.Struct, frozen=True, kw_only=True):
    """Hidden sockets written from scene values by key, rewritten when one of `reads` changes."""

    of: Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, R], Outcome[Mapping[Key, Value]]]
    sockets: tuple[Socket, ...] = ()
    reads: tuple[tuple[type[Struct], bpy.types.Property], ...] = ()


class Animates[R](msgspec.Struct, frozen=True, kw_only=True):
    """Keyframes of entry inputs derived from source channels."""

    keys: Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, R], Outcome[tuple[Animation, ...]]]


class Checks[R](msgspec.Struct, frozen=True, kw_only=True):
    """Scene refusal of a record over keyed inputs, the held entry when one exists."""

    check: Callable[[Identity, bpy.types.Scene, Option[bpy.types.SceneCompositorEffect], R, Context], Outcome[None]]


class Conditions[R](msgspec.Struct, frozen=True, kw_only=True):
    """Context condition a record needs to build."""

    condition: Callable[[R], Condition]


class Spaces[R](msgspec.Struct, frozen=True, kw_only=True):
    """Colorspace or role the record's operation runs in, entered from and returned to scene linear."""

    space: Callable[[R], ColorSpace]


class Opens[R](msgspec.Struct, frozen=True, kw_only=True):
    """File extensions and the record built from a selected path."""

    extensions: Callable[[], tuple[str, ...]]
    of: Callable[[str], R]


class Seeds[R](msgspec.Struct, frozen=True, kw_only=True):
    """Record read from the scene in place of record defaults."""

    seed: Callable[[bpy.types.Scene], R]


class Holds(msgspec.Struct, frozen=True, kw_only=True):
    """Scene and camera values held while an entry exists, the prior scene values restored once the last entry goes."""

    scene: Snapshot = msgspec.field(default_factory=lambda: Snapshot.of(bpy.types.Scene, {}))
    cameras: Snapshot = msgspec.field(default_factory=lambda: Snapshot.of(bpy.types.Camera, {}))


class Registers(msgspec.Struct, frozen=True, kw_only=True):
    """Commands and owners the effect registers beside its group ownerships."""

    rows: tuple[Registrable, ...]


class Blended(msgspec.Struct, frozen=True, kw_only=True):
    """Result mixed back over a base through blend mode, opacity, and masks, the entry input unless a graph end in the operation's space."""

    base: Option[End] = Nothing


class Replaced(msgspec.Struct, frozen=True, kw_only=True):
    """Result replacing the entry input with no blend inputs."""


class CompositorEffect[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Scene compositor effect of one structure record and its group interface: its graph builder, encoding domain, alpha form, mix, and the parts deciding what else it does.

    The interface sockets are the effect's parameters, the entry's inputs their values; the record holds what the group cannot, persisted on the group.
    """

    record: type[R]
    graph: Callable[[Shell[R]], Outcome[Expr]]
    interface: Interface = Interface()
    domain: Domain = Domain.SCENE_LINEAR
    alpha: Alpha = Alpha.INPUT
    mix: Mix = Blended()
    parts: tuple[Part[R], ...] = ()

    # --- [PARTS]

    def found[P](self, kind: type[P]) -> tuple[P, ...]:
        """Returns parts of kind in declaration order."""
        return tuple(part for part in self.parts if isinstance(part, kind))

    def first[P](self, kind: type[P]) -> Option[P]:
        """Returns the first part of kind."""
        return Option.of_optional(next(iter(self.found(kind)), None))

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
    def requests(self) -> bool:
        """Whether the effect makes a render pass request."""
        return bool(self.found(Requests))

    @property
    def imaged(self) -> bool:
        """Whether the group reads render passes, rebuilt on every refresh; groups showing an image rebuild when the depsgraph updates that image."""
        return self.requests

    @property
    def reads(self) -> tuple[tuple[type[Struct], bpy.types.Property], ...]:
        """RNA members derived inputs read."""
        return tuple(read for derives in self.found(Derives) for read in derives.reads)

    def seeded(self, scene: bpy.types.Scene) -> Outcome[R]:
        """Returns scene seed, record defaults, or absent file fault for opened effects."""
        return (
            self
            .first(Seeds)
            .map(lambda seeds: Ok(seeds.seed(scene)))
            .default_with(lambda: Error((Fault(reason=BlenderReason.ABSENT, subject=self.kind, member="file"),)) if self.found(Opens) else Ok(self.record()))
        )

    def ownership(self, record: Entry[R]) -> Ownership[bpy.types.NodeTree]:
        """Selects per-entry ownership for structured or masked records, the shared group otherwise."""
        shared, personal = ownerships(self.record)
        return personal if record.blend.masks or msgspec.structs.fields(self.record) else shared

    def stored(self, identity: Identity) -> Outcome[Stored[Entry[R]]]:
        """Entry record codec on the entry's group."""
        return identity.identifier(Shell.entry).map(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(GenericAlias(Entry, (self.record,)))))

    # --- [ENTRIES]

    def entries(self, identity: Identity, scene: bpy.types.Scene) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns owned entries of the effect in stack order."""
        return entries(identity, scene, self.record)

    def read(self, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[Entry[R]]:
        """Reads the entry record its group stores, absent before the group stores one."""
        return paired(self.stored(identity), Option.of_optional(entry.node_group).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(entry), member="node_group"),))).bind(
            lambda pair: pair[0].read(pair[1]).bind(lambda held: held.to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(pair[1]), member=pair[0].name),)))
        )

    def current(self, identity: Identity, scene: bpy.types.Scene) -> Outcome[Option[R]]:
        """Reads earliest owned record, Nothing when absent."""
        return Option.of_optional(next(iter(self.entries(identity, scene)), None)).map(lambda entry: self.read(identity, entry).map(lambda record: Some(record.record))).default_value(Ok(Nothing))

    def named(self, identity: Identity, scene: bpy.types.Scene, name: str) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Finds named entry among the effect's owned groups."""
        return Option.of_optional(next((entry for entry in self.entries(identity, scene) if entry.name == name), None)).to_result_with(
            lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(scene.compositor_effects), member=name),)
        )

    def write(
        self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, record: Entry[R], *, context: Context = Nothing, source: Option[bpy.types.Scene] = Nothing
    ) -> Outcome[bool]:
        """Reconciles group, then stores the entry record and writes derived inputs, answering whether any changed."""
        return self.met(identity, scene, record.record, Some(entry), context, source).bind(
            lambda request: self.made(identity, scene, record, PyOpenColorIO.GetCurrentConfig(), context, request, Option.of_optional(entry.node_group)).bind(
                lambda built: written(entry, "node_group", built[0]).bind(
                    lambda pointed: self.filled(identity, scene, entry, built[0], record, source).map(lambda filled: built[1] or pointed.is_some() or filled)
                )
            )
        )

    def appended(self, identity: Identity, scene: bpy.types.Scene, record: Entry[R], *, context: Context = Nothing, name: Option[EffectName] = Nothing) -> Outcome[bpy.types.SceneCompositorEffect]:
        """Creates entry once requirements and group succeed."""

        def appended(group: bpy.types.NodeTree) -> Outcome[bpy.types.SceneCompositorEffect]:
            with ExitStack() as rollback:
                entry = scene.compositor_effects.new(name.default_value(self.title))
                rollback.callback(scene.compositor_effects.remove, entry)
                return (
                    accumulated((written(entry, "node_group", group), *(enabled.descriptor.write(entry, True) for enabled in Enabled)))
                    .bind(lambda _: self.filled(identity, scene, entry, group, record))
                    .map(lambda _: rollback.pop_all())
                    .map(lambda _: entry)
                )

        return self.met(identity, scene, record.record, Nothing, context).bind(
            lambda request: self.made(identity, scene, record, PyOpenColorIO.GetCurrentConfig(), context, request).bind(lambda built: appended(built[0]))
        )

    def refreshed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> Outcome[bool]:
        """Writes frame, derived values, and changed keyed channels, answering whether any changed."""
        return self.read(identity, entry).bind(lambda record: self.checked(identity, scene, Some(entry), record.record, Nothing).bind(lambda _: self.derive(identity, scene, entry, record.record)))

    # --- [GROUPS]

    def grouped(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[R], config: PyOpenColorIO.Config, *, entry: Option[bpy.types.SceneCompositorEffect] = Nothing, context: Context = Nothing
    ) -> Outcome[NodeGroup]:
        """Builds entry group from graph builder and blend."""
        return self.met(identity, scene, record.record, entry, context).bind(lambda request: self.built(identity, scene, record, config, context, request))

    def checked(self, identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], record: R, context: Context) -> Outcome[None]:
        """Runs every scene check of the record."""
        return accumulated(part.check(identity, scene, entry, record, context) for part in self.found(Checks)).map(lambda _: None)

    def met(
        self, identity: Identity, scene: bpy.types.Scene, record: R, entry: Option[bpy.types.SceneCompositorEffect], context: Context, source: Option[bpy.types.Scene] = Nothing
    ) -> Outcome[Option[PassRequest]]:
        """Checks conditions and scene checks, then makes the pass request of the context view layer."""
        reading = source.default_value(scene)
        return paired(accumulated(part.condition(record).check(bpy.context, Some(reading)) for part in self.found(Conditions)), self.checked(identity, reading, entry, record, context)).bind(
            lambda _: (
                self
                .first(Requests)
                .map(
                    lambda part: (
                        rooted(bpy.types.ViewLayer, bpy.context)
                        .bind(lambda layer: part.request(record, layer.name))
                        .bind(lambda found: required(scene, bpy.context, (found,), PRECISION).map(lambda _: Some(found)))
                    )
                )
                .default_value(Ok(Nothing))
            )
        )

    def faced(self, record: Entry[R]) -> Interface:
        """Entry group interface: image, the effect's sockets, hidden frame and derived sockets, and for blended effects the blend panel."""
        derived = tuple(msgspec.structs.replace(socket, members={**socket.members, "hide_in_modifier": True}) for part in self.found(Derives) for socket in part.sockets)
        blend = under(Shell.blend, record.blend.interface)
        return Interface(
            outputs=(Socket(key=Shell.result, name="Image", kind=bpy.types.NodeSocketColor),),
            inputs=(Socket(key=Shell.image, name="Image", kind=bpy.types.NodeSocketColor), *self.interface.inputs, *FRAME, *derived),
            panels=(*self.interface.panels, *((InterfacePanel(key=Shell.blend, name="Blend", inputs=blend.inputs, panels=blend.panels),) if isinstance(self.mix, Blended) else ())),
        )

    def built(self, identity: Identity, scene: bpy.types.Scene, record: Entry[R], config: PyOpenColorIO.Config, context: Context, request: Option[PassRequest]) -> Outcome[NodeGroup]:
        """Builds the owned group of a record, blend, and requested pass layer."""
        original = group_input(Shell.image, "RGBA")
        space = Option.of_optional(next((part.space(record.record) for part in self.found(Spaces)), None)).filter(lambda name: name != PyOpenColorIO.ROLE_SCENE_LINEAR)
        straight = self.alpha is Alpha.STRAIGHT
        face = self.faced(record)
        sockets = {key: item[0] for key, item, _, _ in face.walked() if isinstance(item, tuple)}
        active: bpy.types.ViewLayer = bpy.context.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        render = Expr(
            kind=bpy.types.CompositorNodeRLayers, members=(("scene", IdName.of(scene)), ("layer", request.map(lambda held: held.layer).default_value(active.name))), socket="Image", shape="RGBA"
        ).named("render")
        kept = (render,) if request.filter(lambda held: bool(held.flags)).is_some() else ()

        def converting(key: Key, source: str, target: str) -> Callable[[Term], Outcome[Expr]]:
            def straightened(entering: Term) -> Outcome[Expr]:
                return converted(key, config, source, target, entering)

            return straightened if straight else lambda entering: pointwise(f"{key}_scope", entering, straightened)

        def operation(entering: Expr) -> Outcome[Expr]:
            shell = Shell(record=record.record, identity=identity, config=config, color=entering, render=render, sockets=sockets, context=context)
            return (
                space
                .map(lambda name: converting("space_in", PyOpenColorIO.ROLE_SCENE_LINEAR, name)(entering))
                .default_value(Ok(entering))
                .bind(lambda source: self.graph(msgspec.structs.replace(shell, color=source)))
                .bind(lambda made: space.map(lambda name: converting("space_out", name, PyOpenColorIO.ROLE_SCENE_LINEAR)(made)).default_value(Ok(made)))
            )

        def assembled(result: Expr) -> Outcome[NodeGroup]:
            ownership = self.ownership(record)
            return NodeGroup.built(ownership.key, self.title, face, {Shell.result: result}, bpy.types.CompositorNodeTree).map(
                lambda group: msgspec.structs.replace(group, single=ownership.single, graph=msgspec.structs.replace(group.graph, members={DESCRIPTION: self.description}))
            )

        def finished(made: Expr) -> Outcome[NodeGroup]:
            match self.mix:
                case Replaced():
                    return assembled(made.keeping(*kept))
                case Blended(base=base):
                    restored = (
                        Expr(
                            kind=bpy.types.CompositorNodeSetAlpha,
                            inputs=(
                                ("Image", made),
                                (
                                    "Alpha",
                                    Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", original),), members=(("mode", "RGB"),), socket="Alpha").named(
                                        item_key(Shell.result, "alpha_split")
                                    ),
                                ),
                                ("Type", "Replace Alpha"),
                            ),
                            socket="Image",
                            shape="RGBA",
                        ).named(item_key(Shell.result, "alpha"))
                        if self.alpha is Alpha.INPUT
                        else made
                    )
                    beneath: Outcome[Term] = base.map(lambda end: space.map(lambda name: converting("space_base", name, PyOpenColorIO.ROLE_SCENE_LINEAR)(end)).default_value(Ok(end))).default_value(
                        Ok(original)
                    )
                    weight = group_input(item_key(Shell.blend, "opacity"))
                    weighted = coverage(
                        record.blend.masks,
                        original,
                        group_input(item_key(Shell.frame, "aspect")),
                        group_input(item_key(Shell.frame, "scale")),
                        lambda key: group_input(item_key(Shell.blend, "masks", key)),
                        config,
                    ).map(lambda mask: mask.map(lambda held: weight * held).default_value(weight))
                    return (
                        paired(beneath, weighted)
                        .bind(lambda parts: mixed_back(Shell.blend, parts[0], restored, group_input(item_key(Shell.blend, "mode")), parts[1]))
                        .bind(lambda value: assembled(value.keeping(*kept)))
                    )

        return (pointwise("color", original, operation) if straight else operation(original)).bind(finished)

    def made(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[R], config: PyOpenColorIO.Config, context: Context, request: Option[PassRequest], tree: Option[bpy.types.NodeTree] = Nothing
    ) -> Outcome[tuple[bpy.types.NodeTree, bool]]:
        """Reconciles the held group of the record's ownership, else makes one, answering whether it changed."""

        def refreshed(group: NodeGroup, live: bpy.types.NodeTree, keys: Mapping[Key, str]) -> Outcome[tuple[bpy.types.NodeTree, bool]]:
            marked = {node: mark.key for node in live.nodes for mark in Mark.read(identity, node).to_list()}
            viewers = tuple(node for node, key in marked.items() if key == Shell.viewer)
            semantic = {identifier: key for key, identifier in keys.items()}

            def endpoint(socket: bpy.types.NodeSocket) -> End:
                return End(node=marked[socket.node], socket=semantic.get(socket.identifier, socket.identifier) if marked[socket.node] in {INPUTS, OUTPUTS} else socket.identifier)  # type: ignore[index]  # ty: ignore[invalid-argument-type]

            graph = msgspec.structs.replace(
                group.graph,
                nodes={**group.graph.nodes, **({Shell.viewer: Node(kind=bpy.types.CompositorNodeViewer)} if viewers else {})},
                links=(
                    *group.graph.links,
                    *(
                        Link(source=endpoint(link.from_socket), target=endpoint(link.to_socket))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                        for viewer in viewers
                        for socket in socket_at(viewer.inputs, "Image").to_option().to_list()
                        for link in socket.links  # type: ignore[union-attr]  # ty: ignore[not-iterable]
                        if link.from_node in marked and marked[link.from_node] in group.graph.placed
                    ),
                ),
            )
            return reconciled(msgspec.structs.replace(group, graph=graph), live, identity).map(lambda changes: (live, changes != Changes()))

        own = self.ownership(record)
        owner = Some(scene) if not own.single or request.filter(lambda held: bool(held.flags)).is_some() else Nothing
        held = tree.filter(lambda group: Mark.read(identity, group).filter(lambda mark: mark.key == own.key and mark.of == owner).is_some()).or_else_with(
            lambda: Option.of_optional(next((group for group, mark in own.held(identity) if mark.of == owner), None)) if own.single else Nothing
        )
        return self.built(identity, scene, record, config, context, request).bind(
            lambda group: held.map(lambda live: slot(identity).bind(lambda name: refreshed(group, live, addresses(live, name)))).default_with(
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
        """Writes camera frame, derived values, and keyed channels, answering whether any changed."""
        reading = source.default_value(scene)
        frame = FrameSize().bound(identity).bind(lambda framed: camera_frame(framed, reading)).bind(lambda value: entered(identity, entry, value.values))
        values = accumulated(part.of(identity, reading, entry, record).bind(lambda found: entered(identity, entry, found)) for part in self.found(Derives)).map(any)
        channels = accumulated(
            part.keys(identity, scene, entry, record).bind(lambda found: accumulated(applied_channel(channel, scene) for channel in found).map(any)) for part in self.found(Animates)
        ).map(any)
        return accumulated((frame, values, channels)).map(any)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns ownerships and registered rows."""
        return Ok((*ownerships(self.record), *(row for part in self.found(Registers) for row in part.rows)))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ENTRY_EDITS]


def revised[R: Case](effect: CompositorEffect[R], identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, change: Callable[[R], Outcome[R]]) -> Outcome[bool]:
    """Reads entry's record, writes its changed record keeping the blend, answering whether the group or its store changed."""
    return effect.read(identity, entry).bind(lambda held: change(held.record).bind(lambda record: effect.write(identity, scene, entry, msgspec.structs.replace(held, record=record))))


def entered(identity: Identity, entry: bpy.types.SceneCompositorEffect, values: Mapping[Key, Value]) -> Outcome[bool]:
    """Writes entry input values by interface key, answering whether any changed."""
    return accumulated(entry_input(identity, entry, key).bind(lambda owner: written(owner, "value", value)).map(Option.is_some) for key, value in values.items()).map(any)


# --- [ENTRY_HOLDERS]


def ownerships(record: type[Case]) -> tuple[Ownership[bpy.types.NodeTree], Ownership[bpy.types.NodeTree]]:
    """Declares shared and per-entry group ownerships."""
    key = item_key(record)
    return Ownership(kind=bpy.types.NodeTree, key=key, saved=Saved.USED), Ownership(kind=bpy.types.NodeTree, key=f"{key}_entry", saved=Saved.USED, single=False)


def entries(identity: Identity, scene: bpy.types.Scene, record: type[Case]) -> tuple[bpy.types.SceneCompositorEffect, ...]:
    """Returns entries with either ownership mark of record in stack order."""
    return Marks.of(identity, scene).keyed(*(owner.key for owner in ownerships(record)))


# --- [SCENE_VALUES]


def camera_frame(framed: Framed, scene: bpy.types.Scene) -> Outcome[CameraFrame]:
    """Reads pixel aspect, overscan camera share, and unit scale."""
    render = scene.render
    return framed.base(scene).map(
        lambda base: CameraFrame(
            aspect=render.pixel_aspect_x / render.pixel_aspect_y, scale=base.map(lambda original: original.size[0] / render.resolution_x).default_value(1.0), meters=scene.unit_settings.scale_length
        )
    )


def framing(identity: Identity) -> Outcome[Derives[Case]]:
    """Binds frame size settings into camera frame derivation and the RNA members it reads."""
    return (
        FrameSize()
        .bound(identity)
        .map(
            lambda framed: Derives(
                of=lambda _identity, scene, _entry, _record: camera_frame(framed, scene).map(lambda frame: frame.values),
                reads=(
                    *(
                        (bpy.types.RenderSettings, bpy.types.RenderSettings.bl_rna.properties[name])
                        for name in ("pixel_aspect_x", "pixel_aspect_y", "resolution_x", "resolution_y", "resolution_percentage")
                    ),
                    (bpy.types.UnitSettings, bpy.types.UnitSettings.bl_rna.properties["scale_length"]),
                ),
            )
        )
    )


def precise(_record: Case, layer: str) -> Outcome[PassRequest]:
    """Requests full compositor precision with no render pass."""
    return Ok(PassRequest(layer=layer, precise=True))


def depth_precise(_record: Case, layer: str) -> Outcome[PassRequest]:
    """Requests the Depth pass at full compositor precision."""
    return PassRequest.depth(layer, precise=True)


def applied_channel(channel: Animation, scene: bpy.types.Scene) -> Outcome[bool]:
    """Writes changed keyframes, answering whether it wrote, refusing cycle-forming drivers."""
    match channel.source:
        case Driven():
            return Error((Fault(reason=RecordReason.OUTSIDE, subject=channel.path, member=Keys.__name__, cause=Driven.__name__),))
        case Keys():
            return channel.apply(scene, bpy.context)


# --- [COMPOSITION] ----------------------------------------------------------------------

shutter = Derives[Case](
    of=lambda _identity, scene, _entry, _record: Ok({"length": Shutter.of(scene).length, "opening": Shutter.of(scene).opening}),
    sockets=SHUTTER,
    reads=tuple((bpy.types.RenderSettings, bpy.types.RenderSettings.bl_rna.properties[name]) for name in ("motion_blur_shutter", "motion_blur_position")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "DESCRIPTION",
    "FRAME",
    "PRECISION",
    "Alpha",
    "Animates",
    "Blended",
    "CameraFrame",
    "Checks",
    "CompositorEffect",
    "Conditions",
    "Context",
    "Derives",
    "EffectName",
    "Enabled",
    "Holds",
    "Marks",
    "Mix",
    "Named",
    "Opens",
    "Part",
    "Registers",
    "Replaced",
    "Requests",
    "Restoring",
    "Seeds",
    "Shell",
    "Spaces",
    "applied_channel",
    "camera_frame",
    "depth_precise",
    "entered",
    "entries",
    "framing",
    "ownerships",
    "precise",
    "revised",
    "shutter",
]
