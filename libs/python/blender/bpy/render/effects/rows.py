"""Effect rows: the row declaration with its parts, the graph context its builder reads, and the entries a scene holds for it."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from enum import auto, Enum
import functools
import inspect
import typing
from typing import Annotated, ClassVar, Final, Generic, Literal, Self, TYPE_CHECKING, TypeVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import msgspec.inspect as native
import numpy as np
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.operators import Operator
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import IdName, vacant
from blender.bpy.data.rna.animation import Animation, Driven, Keys
from blender.bpy.data.rna.conditions import check, Condition
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderEditFault, BlenderResult
from blender.bpy.nodes.changes import Changes, made, reconciled
from blender.bpy.nodes.graphs import DataType, End, Expr, group_input, INPUTS, Link, menu, Node, NodeGroup, OUTPUTS, Term
from blender.bpy.nodes.interface import addresses, Interface, InterfacePanel, item_key, MenuItems, slot, Socket, under
from blender.bpy.nodes.trees import socket_at
from blender.bpy.render.color.spaces import config_of, converted, pointwise
from blender.bpy.render.effects.compositing.blending import mixed_back
from blender.bpy.render.effects.compositing.masks import coverage
from blender.bpy.render.effects.coordinates import framed, FrameSpace
from blender.bpy.render.effects.records import active, case_values, CurveEditor, entered, entry_input, Field, graph_values, inputs, KeyPath, reached, stored_graphs
from blender.bpy.render.outputs.framing import Framed, FrameSize, Framing
from blender.bpy.render.view_layers.passes import PassRequest, required
from blender.bpy.settings.attachments import Stored
from blender.imaging.chain import Effect
from blender.imaging.color.processors import Spaced
from blender.imaging.compositing.blending import Blend, Entry
from blender.imaging.compositing.masks import Mask
from blender.imaging.effects.tone.exposure import Exposure
from blender.imaging.faults import accumulated, ImagingResult, paired
from blender.imaging.files.images import ImageFile
from blender.imaging.parameters import Case, Gated, GraphForm, heading, Rgb
from blender.imaging.sampling import Estimate, Precision

if TYPE_CHECKING:
    from blender.bpy.render.effects.stack import EffectStack

# --- [TYPES] ----------------------------------------------------------------------------

type EffectName = Annotated[str, msgspec.Meta(min_length=1)]
type Context = Option[tuple[tuple[int, int], Framing]]
type Animated[R: Effect] = Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, R], BlenderResult[tuple[Animation, ...]]]
type Preparing[R: Effect] = Callable[
    [Identity, bpy.types.Scene, R, PyOpenColorIO.Config, OpenImageIO.ImageBuf, Precision, CameraFrame, bpy.types.SceneCompositorEffect],
    BlenderResult[tuple[R, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]],
]
type Restoring = Callable[[], BlenderResult[None]]
_R_co = TypeVar("_R_co", bound=Effect, covariant=True)
_D_co = TypeVar("_D_co", bound=msgspec.Struct, covariant=True)


class Alpha(Enum):
    """Alpha of a row's result: the stage input's, the graph's coverage, or straight color through the operation."""

    INPUT = auto()
    RESULT = auto()
    STRAIGHT = auto()


class Enabled(Enum):
    """Compositor effect enable flag a stack evaluation reads."""

    RENDER = bpy.types.SceneCompositorEffect.bl_rna.properties["enable_for_render"]
    PREVIEW = bpy.types.SceneCompositorEffect.bl_rna.properties["enable_for_preview"]

    @property
    def descriptor(self) -> Descriptor:
        """Owning RNA property for reads and writes."""
        return Descriptor(root=bpy.types.SceneCompositorEffect, owner="", prop=self.value)


# --- [CONSTANTS] ------------------------------------------------------------------------

DESCRIPTION: Final = Descriptor.declared(bpy.types.CompositorNodeTree, "description", bpy.types.StringProperty)
PRECISION: Final = Descriptor.declared(bpy.types.Scene, "render.compositor_precision", bpy.types.EnumProperty)
HIDDEN: Final = Descriptor.declared(bpy.types.NodeTreeInterfaceSocket, "hide_in_modifier", bpy.types.BoolProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Named(msgspec.Struct, frozen=True, kw_only=True):
    """Compositor entry selected by name."""

    name: EffectName


class Opened(msgspec.Struct, Generic[_R_co], frozen=True, kw_only=True):
    """File extensions and effect built from selected path."""

    extensions: Callable[[], tuple[str, ...]]
    of: Callable[[str], _R_co]


class CameraFrame(msgspec.Struct, frozen=True, kw_only=True):
    """Pixel aspect, camera share of overscan, and meters per Blender unit."""

    aspect: float = 1.0
    scale: float = 1.0
    meters: float

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


class Shutter(msgspec.Struct, frozen=True, kw_only=True):
    """Shutter length in frames and opening offset relative to frame."""

    length: float = 0.5
    opening: float = -0.5


class Inspected(msgspec.Struct, frozen=True, kw_only=True):
    """Graph output a Viewer shows under its own command, titled `View <Key Heading>`."""

    key: Key
    end: End
    description: str


class Measured(msgspec.Struct, Generic[_R_co], frozen=True, kw_only=True):
    """Record revision from the frame entering a row's entry, and the command arming it."""

    revise: Callable[[PyOpenColorIO.Config, _R_co, ImageFile[np.float32]], BlenderResult[Option[_R_co]]]
    command: Option[Operator[Named]] = Nothing


class Derived(msgspec.Struct, Generic[_R_co, _D_co], frozen=True, kw_only=True):
    """Hidden inputs written from scene values: a record of hidden sockets, else values by input key."""

    of: Callable[[Identity, bpy.types.Scene, _R_co], BlenderResult[_D_co | Mapping[Key, Value]]]
    record: Option[type[_D_co]] = Nothing
    reads: "tuple[tuple[type[bpy.types.bpy_struct[object]], bpy.types.Property], ...]" = ()


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


class Shell[R: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Graph builder's record, stage input color, render layer, group input readers, menu items, retained cases, and prepared images."""

    record: R
    identity: Identity
    config: PyOpenColorIO.Config
    color: Expr
    render: Expr
    items: Mapping[KeyPath, MenuItems]
    cases: Mapping[KeyPath, tuple[Case, ...]]
    images: Mapping[Key, Expr] = {}
    context: Context = Nothing
    image: ClassVar[Key] = "image"
    result: ClassVar[Key] = "result"
    viewer: ClassVar[Key] = "viewer"
    blend: ClassVar[Key] = "blend"
    frame: ClassVar[Key] = "frame"

    def input(self, *path: Key | int | type[Case]) -> Expr:
        """Reads the group input of a record path of field names, item indices, and case types, a gated field at its value."""
        field = next((field for field in Field.of(type(self.record), value=Some(self.record)) if field.path == path), None)
        match field:
            case Field(shape=native.StructType(cls=kind)) if (typing.get_origin(kind) or kind) is Gated:
                return _entered(item_key(*path, Field.of(kind)[0].info.name), Field.of(kind)[0].shape)
            case Field(shape=shape):
                return _entered(item_key(*path), shape)
            case None:
                return _entered(item_key(*path), native.FloatType())

    def gate(self, *path: Key | int | type[Case]) -> Expr:
        """Reads the enabled socket of a gated record field."""
        return _entered(item_key(*path), native.BoolType())

    def passed(self) -> BlenderResult[Expr]:
        """Returns the stage input, the graph of rows whose preparation renders the effect."""
        return Ok(self.color)

    @property
    def space(self) -> FrameSpace:
        """Camera frame of the stage input."""
        aspect, scale, _ = CameraFrame.__struct_fields__
        return framed(self.color, self.input(Shell.frame, aspect), self.input(Shell.frame, scale))

    def layer(self, socket: str) -> Expr:
        """Reads one Render Layers output of the row's pass request layer."""
        return msgspec.structs.replace(self.render, socket=socket)

    def menu(self, *path: Key | int | type[Case], cases: Mapping[str, Term], kind: DataType | Literal["MENU", "BOOLEAN"]) -> Expr:
        """Switches between case terms by item name at a choice or case union field's menu input."""
        return menu(self.input(*path), self.items[path], cases, kind=kind)

    def case[C: Case](self, kind: type[C], *parts: Key | int | type[Case]) -> Option[C]:
        """Returns active or retained case of kind at path."""
        return Option.of_optional(next((value for value in self.cases[parts] if isinstance(value, kind)), None))


class CompositorEffect(msgspec.Struct, Generic[_R_co], frozen=True, kw_only=True):
    """Effect row: record, graph builder, alpha form, requirements, derived inputs, and the commands and holds it brings."""

    record: type[_R_co]
    graph: Callable[[Shell[_R_co]], BlenderResult[Expr]]
    alpha: Alpha = Alpha.INPUT
    mix_base: Option[End] = Nothing
    requested: Option[Callable[[_R_co, str], BlenderResult[PassRequest]]] = Nothing
    derived: Option[Derived[_R_co, msgspec.Struct]] = Nothing
    animated: Option[Animated[_R_co]] = Nothing
    condition: Option[Callable[[_R_co], Condition]] = Nothing
    validated: Option[Callable[[_R_co], ImagingResult[_R_co]]] = Nothing
    native_validated: Option[Callable[[Identity, bpy.types.Scene, Option[bpy.types.SceneCompositorEffect], _R_co, Context], BlenderResult[None]]] = Nothing
    preparation: Option[Preparing[_R_co]] = Nothing
    opened: Option[Opened[_R_co]] = Nothing
    seed: Option[Callable[[bpy.types.Scene], _R_co]] = Nothing
    measured: Option[Measured[_R_co]] = Nothing
    inspected: tuple[Inspected, ...] = ()
    inputs: tuple[Socket, ...] = ()
    holds: Mapping[str, Value] = msgspec.field(default_factory=dict)
    cameras: Mapping[str, Value] = msgspec.field(default_factory=dict)
    editors: tuple[CurveEditor, ...] = ()
    registrables: tuple[Registrable, ...] = ()
    blended: ClassVar[bool] = True

    # --- [ROW_VALUES]

    @property
    def kind(self) -> Key:
        """Record key naming ownerships and commands."""
        return item_key(self.record)

    @property
    def description(self) -> str:
        """Record documentation describing the row's group and commands."""
        return Option.of_optional(inspect.getdoc(self.record)).default_value("")

    @property
    def requests(self) -> bool:
        """Whether row makes render pass request."""
        return self.requested.is_some()

    @property
    def staged(self) -> bool:
        """Whether row prepares entering image before group build."""
        return self.preparation.is_some()

    @property
    def imaged(self) -> bool:
        """Whether the group reads render passes or images, rebuilt on every refresh."""
        return self.requests or any(GraphForm.IMAGE in field.forms for field in Field.of(self.record))

    @property
    def reads(self) -> "tuple[tuple[type[bpy.types.bpy_struct[object]], bpy.types.Property], ...]":
        """RNA members derived inputs read."""
        return self.derived.map(lambda derived: derived.reads).default_value(())

    def seeded(self, scene: bpy.types.Scene) -> BlenderResult[_R_co]:
        """Returns scene seed, record defaults, or absent file fault for opened rows."""
        return self.seed.map(lambda seed: Ok(seed(scene))).default_with(lambda: Error((BlenderAbsentFault(owner=self.record.title, key="file"),)) if self.opened.is_some() else Ok(self.record()))

    def ownership(self, record: Entry[Effect]) -> Ownership[bpy.types.NodeTree]:
        """Selects per-entry ownership for staged, graphed, collection, or masked records."""
        shared, personal = ownerships(self.record)
        fields = Field.of(self.record)
        return personal if self.staged or record.blend.masks or any(field.graphed or field.collection.is_some() for field in fields) else shared

    # --- [HOOKS]

    def placing[E: Effect](self, _identity: Identity, _scene: bpy.types.Scene, _stack: "EffectStack[E]", /) -> BlenderResult[None]:
        """Prepares the scene before the row's first entry is placed."""
        return Ok(None)

    def holding(self, _identity: Identity, _scene: bpy.types.Scene, /) -> BlenderResult[None]:
        """Writes scene state the row holds beyond its declared holds while it has an entry."""
        return Ok(None)

    def removing(self, _identity: Identity, _scene: bpy.types.Scene, _entry: bpy.types.SceneCompositorEffect, /) -> BlenderResult[Restoring]:
        """Reads what the row restores once its entry and holds are gone, answering the restoring step."""
        return Ok(lambda: Ok(None))

    # --- [ENTRIES]

    def entries(self, identity: Identity, scene: bpy.types.Scene) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns owned entries of the row in stack order."""
        return entries(identity, scene, self.record)

    def read(self, identity: Identity, entry: bpy.types.SceneCompositorEffect, *, edited: bool = True, graphs: Mapping[Key, object] = {}) -> BlenderResult[Entry[_R_co]]:
        """Reads graph store and group inputs, then validates record."""
        domain = self.record.stage.domain
        records = (((), self.record, Nothing, domain, self.editors), *((((Shell.blend,), Blend, Nothing, domain, ()),) if self.blended else ()))

        def validated[R: Effect](row: CompositorEffect[R], value: R) -> BlenderResult[R]:
            return row.validated.map(lambda accept: accept(value)).default_value(Ok(value))

        def decoded(values: Mapping[Key, object]) -> BlenderResult[Entry[_R_co]]:
            accepted = inputs(identity, entry, (), self.record, values).bind(functools.partial(validated, self))
            return paired(accepted, inputs(identity, entry, (Shell.blend,), Blend, values) if self.blended else Ok(Blend())).map(lambda pair: Entry(effect=pair[0], blend=pair[1]))

        return graph_values(identity, entry, records, edited=edited).bind(lambda values: decoded({**values, **graphs}))

    def current(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[Option[_R_co]]:
        """Reads earliest owned effect, Nothing when absent."""
        return Option.of_optional(next(iter(self.entries(identity, scene)), None)).map(lambda entry: self.read(identity, entry).map(lambda record: Some(record.effect))).default_value(Ok(Nothing))

    def named(self, identity: Identity, scene: bpy.types.Scene, name: str) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Finds named entry among row's owned groups."""
        return Option.of_optional(next((entry for entry in self.entries(identity, scene) if entry.name == name), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=name),)
        )

    def write(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        entry: bpy.types.SceneCompositorEffect,
        record: Entry[Effect],
        *,
        context: Context = Nothing,
        source: Option[bpy.types.Scene] = Nothing,
        prepared: bool = False,
        images: Mapping[Key, Expr] = {},
    ) -> BlenderResult[bool]:
        """Reconciles group, then writes rebuilt inputs, answering whether any changed."""
        return (
            editable(scene)
            .bind(lambda _: self.narrowed(record))
            .bind(
                lambda typed: paired(configured(typed.effect), self.met(identity, scene, typed.effect, Some(entry), context, source, prepared=prepared)).bind(
                    lambda parts: self.made(identity, scene, typed, parts[0], context, parts[1], Option.of_optional(entry.node_group), images=images).bind(
                        lambda built: written(entry, "node_group", built[0]).bind(
                            lambda pointed: self.filled(identity, scene, entry, built[0], typed, source).map(lambda filled: built[1] or pointed.is_some() or filled)
                        )
                    )
                )
            )
        )

    def appended(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[Effect], *, context: Context = Nothing, name: Option[EffectName] = Nothing
    ) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Creates entry once requirements and group succeed."""
        label = vacant(name.default_value(self.record.title), (entry.name for entry in scene.compositor_effects))

        def appended(group: bpy.types.NodeTree, typed: Entry[_R_co]) -> BlenderResult[bpy.types.SceneCompositorEffect]:
            with ExitStack() as rollback:
                entry = scene.compositor_effects.new(label)
                rollback.callback(scene.compositor_effects.remove, entry)
                return (
                    accumulated((written(entry, "node_group", group), *(enabled.descriptor.write(entry, True) for enabled in Enabled)))
                    .bind(lambda _: self.filled(identity, scene, entry, group, typed))
                    .map(lambda _: rollback.pop_all())
                    .map(lambda _: entry)
                )

        return (
            editable(scene)
            .bind(lambda _: self.narrowed(record))
            .bind(
                lambda typed: paired(configured(typed.effect), self.met(identity, scene, typed.effect, Nothing, context)).bind(
                    lambda parts: self.made(identity, scene, typed, parts[0], context, parts[1]).bind(lambda built: appended(built[0], typed))
                )
            )
        )

    def refreshed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[bool]:
        """Writes frame, derived values, and changed keyed channels, answering whether any changed."""
        return (
            editable(scene)
            .bind(lambda _: self.read(identity, entry))
            .bind(
                lambda record: (
                    Ok(None)
                    if self.staged and not record.effect.pointwise
                    else self.native_validated.map(lambda accept: accept(identity, scene, Some(entry), record.effect, Nothing)).default_value(Ok(None))
                ).bind(lambda _: self.derive(identity, scene, entry, record.effect))
            )
        )

    def prepared(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[Effect], config: PyOpenColorIO.Config, image: OpenImageIO.ImageBuf, precision: Precision, entry: bpy.types.SceneCompositorEffect
    ) -> BlenderResult[tuple[Entry[_R_co], Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
        """Prepares effective record and named images, keeping authored blend."""
        return self.narrowed(record).bind(
            lambda typed: self.preparation.map(
                lambda prepare: (
                    inputs(identity, entry, (Shell.frame,), CameraFrame)
                    .bind(lambda frame: prepare(identity, scene, typed.effect, config, image, precision, frame, entry))
                    .map(lambda value: (msgspec.structs.replace(typed, effect=value[0]), value[1]))
                )
            ).default_value(Ok((typed, {})))
        )

    # --- [GROUPS]

    def grouped(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[Effect],
        config: PyOpenColorIO.Config,
        *,
        entry: Option[bpy.types.SceneCompositorEffect] = Nothing,
        context: Context = Nothing,
        prepared: bool = False,
        images: Mapping[Key, Expr] = {},
    ) -> BlenderResult[NodeGroup]:
        """Builds entry group from graph builder and blend."""
        return self.narrowed(record).bind(
            lambda typed: self.met(identity, scene, typed.effect, entry, context, prepared=prepared).bind(
                lambda request: self.built(identity, scene, typed, config, entry.bind(lambda value: Option.of_optional(value.node_group)), context, request, images=images)
            )
        )

    def narrowed(self, record: Entry[Effect]) -> BlenderResult[Entry[_R_co]]:
        """Narrows an entry record to the row's record type."""
        effect = record.effect
        return (
            Ok(Entry(effect=effect, blend=record.blend)) if isinstance(effect, self.record) else Error((BlenderChoiceFault(path=self.kind, identifier=item_key(type(effect)), accepted=(self.kind,)),))
        )

    def met(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Effect,
        entry: Option[bpy.types.SceneCompositorEffect],
        context: Context,
        source: Option[bpy.types.Scene] = Nothing,
        *,
        prepared: bool = False,
    ) -> BlenderResult[Option[PassRequest]]:
        """Checks condition and native validation, then makes the pass request."""
        reading = source.default_value(scene)

        def checked[R: Effect](row: CompositorEffect[R], held: R) -> BlenderResult[Option[PassRequest]]:
            return paired(
                row.condition.map(lambda condition: check(condition(held), bpy.context, Some(reading))).default_value(Ok(None)),
                row.native_validated.map(lambda accept: accept(identity, reading, entry, held, context)).default_value(Ok(None)) if prepared or not row.staged or held.pointwise else Ok(None),
            ).bind(
                lambda _: row.requested.map(
                    lambda request: request(held, scene.view_layers[0].name).bind(lambda found: required(scene, bpy.context, (found,), PRECISION).map(lambda _: Some(found)))
                ).default_value(Ok(Nothing))
            )

        return self.narrowed(Entry(effect=record)).bind(lambda typed: checked(self, typed.effect))

    def built(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[Effect],
        config: PyOpenColorIO.Config,
        tree: Option[bpy.types.NodeTree],
        context: Context,
        request: Option[PassRequest],
        *,
        images: Mapping[Key, Expr] = {},
    ) -> BlenderResult[NodeGroup]:
        """Builds the owned group of a record, blend, and requested pass layer."""
        match self.narrowed(record):
            case Result(tag="ok", ok=typed):
                pass
            case Result(error=faults):
                return Error(faults)
        original = group_input(Shell.image, "RGBA")
        space = Some(typed.effect.space).filter(lambda name: name != PyOpenColorIO.ROLE_SCENE_LINEAR) if isinstance(typed.effect, Spaced) else Nothing
        straight = self.alpha is Alpha.STRAIGHT
        render = Expr(
            kind=bpy.types.CompositorNodeRLayers,
            members=(("scene", IdName.of(scene)), ("layer", request.map(lambda held: held.layer).default_with(lambda: scene.view_layers[0].name))),
            socket="Image",
            shape="RGBA",
        ).named("render")
        kept = (*images.values(), *((render,) if request.filter(lambda held: bool(held.flags)).is_some() else ()))

        def converting(key: Key, source: str, target: str) -> Callable[[Term], BlenderResult[Expr]]:
            def straightened(entering: Term) -> BlenderResult[Expr]:
                return converted(key, config, source, target, entering)

            return straightened if straight else lambda entering: pointwise(f"{key}_scope", entering, straightened)

        def body(cases: Mapping[KeyPath, tuple[Case, ...]]) -> BlenderResult[Expr]:
            def graphed(source: Expr) -> BlenderResult[Expr]:
                items = {field.path: held for field in Field.of(self.record, value=Some(typed.effect)) for held in field.menu.to_list()}
                return self.graph(Shell(record=typed.effect, identity=identity, config=config, color=source, render=render, items=items, cases=cases, images=images, context=context))

            def operation(entering: Expr) -> BlenderResult[Expr]:
                return (
                    space
                    .map(lambda name: converting("space_in", PyOpenColorIO.ROLE_SCENE_LINEAR, name)(entering))
                    .default_value(Ok(entering))
                    .bind(graphed)
                    .bind(lambda made: space.map(lambda name: converting("space_out", name, PyOpenColorIO.ROLE_SCENE_LINEAR)(made)).default_value(Ok(made)))
                )

            entering = images.get(Shell.image, original)
            return pointwise("color", entering, operation) if straight else operation(entering)

        def assembled(result: Option[Expr], face: Interface) -> BlenderResult[NodeGroup]:
            ownership = self.ownership(typed)
            requested = tree.map(lambda held: identity.name(Name.ATTACHMENT, Shell.viewer).bind(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(End)).read(held))).default_value(
                Ok(Nothing)
            )

            def viewed(group: NodeGroup, target: Option[End]) -> NodeGroup:
                return (
                    msgspec.structs.replace(
                        group,
                        graph=msgspec.structs.replace(
                            group.graph,
                            nodes=group.graph.nodes | {Shell.viewer: Node(kind=bpy.types.CompositorNodeViewer)},
                            links=(*group.graph.links, *(Link(source=end, target=End(node=Shell.viewer, socket="Image")) for end in target.to_list())),
                        ),
                    )
                    if images and target.is_some()
                    else group
                )

            return (
                paired(NodeGroup.built(ownership.key, self.record.title, face, dict(result.map(lambda value: (Shell.result, value)).to_list())), requested)
                .map(lambda pair: viewed(*pair))
                .map(lambda group: msgspec.structs.replace(group, single=ownership.single, graph=msgspec.structs.replace(group.graph, members={DESCRIPTION: self.description})))
            )

        def finished(made: Expr, face: Interface) -> BlenderResult[NodeGroup]:
            if not self.blended:
                return assembled(Some(made.keeping(*kept)), face)
            split, alpha = item_key(Shell.result, "alpha_split"), item_key(Shell.result, "alpha")
            restored = (
                Expr(
                    kind=bpy.types.CompositorNodeSetAlpha,
                    inputs=(
                        ("Image", made),
                        ("Alpha", Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", original),), members=(("mode", "RGB"),), socket="Alpha").named(split)),
                        ("Type", "Replace Alpha"),
                    ),
                    socket="Image",
                    shape="RGBA",
                ).named(alpha)
                if self.alpha is Alpha.INPUT
                else made
            )
            base: BlenderResult[Term] = self.mix_base.map(
                lambda end: space.map(lambda name: converting("space_base", name, PyOpenColorIO.ROLE_SCENE_LINEAR)(end)).default_value(Ok(end))
            ).default_value(Ok(original))
            aspect, scale, _ = (item_key(Shell.frame, name) for name in CameraFrame.__struct_fields__)
            mode, opacity, masks = (item_key(Shell.blend, name) for name in Blend.__struct_fields__)
            weight = group_input(opacity)
            weighted = coverage(len(typed.blend.masks), original, group_input(aspect), group_input(scale), lambda key: group_input(item_key(masks, key))).map(
                lambda mask: mask.map(lambda held: weight * held).default_value(weight)
            )
            return paired(base, weighted).bind(lambda parts: mixed_back(Shell.blend, parts[0], restored, group_input(mode), parts[1])).bind(lambda value: assembled(Some(value.keeping(*kept)), face))

        interface = (
            entry_interface(self.record, len(typed.blend.masks), blended=self.blended)
            .bind(lambda face: expanded(face, typed.effect))
            .map(lambda face: msgspec.structs.replace(face, inputs=(*face.inputs, *self.inputs)))
        )
        derived = self.derived.bind(lambda held: held.record).map(lambda kind: Interface.of(kind).map(hidden)).default_value(Ok(Interface()))
        combined = paired(interface, derived).map(lambda pair: msgspec.structs.replace(pair[0], inputs=(*pair[0].inputs, *pair[1].inputs), panels=(*pair[0].panels, *pair[1].panels)))
        if self.staged and not images and not typed.effect.pointwise:
            return combined.bind(lambda face: assembled(Nothing, face))
        return paired(combined, case_values(typed.effect, tree, identity)).bind(lambda parts: body(parts[1]).bind(lambda made: finished(made, parts[0])))

    def made(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[Effect],
        config: PyOpenColorIO.Config,
        context: Context,
        request: Option[PassRequest],
        tree: Option[bpy.types.NodeTree] = Nothing,
        *,
        images: Mapping[Key, Expr] = {},
    ) -> BlenderResult[tuple[bpy.types.NodeTree, bool]]:
        """Reconciles the held group of the record's ownership, else makes one, answering whether it changed."""

        def refreshed(group: NodeGroup, live: bpy.types.NodeTree, keys: Mapping[Key, str]) -> BlenderResult[tuple[bpy.types.NodeTree, bool]]:
            marked = {node: mark.key for node in live.nodes for mark in Mark.read(identity, node).to_list()}
            viewers = tuple(node for node, key in marked.items() if key == Shell.viewer)
            semantic = {identifier: key for key, identifier in keys.items()}

            def endpoint(socket: bpy.types.NodeSocket) -> End:
                return End(node=marked[socket.node], socket=semantic.get(socket.identifier, socket.identifier) if marked[socket.node] in {INPUTS, OUTPUTS} else socket.identifier)  # type: ignore[index]  # ty: ignore[invalid-argument-type]

            graph = msgspec.structs.replace(
                group.graph,
                nodes=group.graph.nodes | ({Shell.viewer: Node(kind=bpy.types.CompositorNodeViewer)} if viewers else {}),
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
        return self.built(identity, scene, record, config, held, context, request, images=images).bind(
            lambda group: held.map(lambda live: slot(identity).bind(lambda name: refreshed(group, live, addresses(live, name)))).default_with(
                lambda: made(group, identity, of=owner).map(lambda live: (live, True))
            )
        )

    def filled(
        self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree, typed: Entry[Effect], source: Option[bpy.types.Scene] = Nothing
    ) -> BlenderResult[bool]:
        """Stores graph values and writes record and blend inputs, then derived values, answering whether any changed."""
        parts: tuple[tuple[KeyPath, msgspec.Struct], ...] = (((), typed.effect), *((((Shell.blend,), typed.blend),) if self.blended else ()))
        return accumulated((
            stored_graphs(identity, group, tuple((prefix, value, self.record.stage.domain) for prefix, value in parts)),
            *(entered(identity, entry, prefix, value) for prefix, value in parts),
        )).bind(lambda outcomes: self.derive(identity, scene, entry, typed.effect, source).map(lambda refreshed: any(outcomes) or refreshed))

    def derive(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, record: Effect, source: Option[bpy.types.Scene] = Nothing) -> BlenderResult[bool]:
        """Writes camera frame, derived values, and keyed channels, answering whether any changed."""
        reading = source.default_value(scene)

        def keyed(values: msgspec.Struct | Mapping[Key, Value]) -> BlenderResult[bool]:
            match values:
                case msgspec.Struct():
                    return entered(identity, entry, (), values)
                case _:
                    return accumulated(entry_input(identity, entry, key).bind(lambda owner: written(owner, "value", value)).map(Option.is_some) for key, value in values.items()).map(any)

        def derived[R: Effect](row: CompositorEffect[R], held: R) -> BlenderResult[bool]:
            frame = FrameSize().bound(identity).bind(lambda framed: camera_frame(framed, reading)).bind(lambda value: entered(identity, entry, (Shell.frame,), value))
            values = row.derived.map(lambda derive: derive.of(identity, reading, held).bind(keyed)).default_value(Ok(False))
            channels = row.animated.map(lambda build: build(identity, scene, entry, held).bind(lambda found: accumulated(applied_channel(channel, scene) for channel in found).map(any))).default_value(
                Ok(False)
            )
            return accumulated((frame, values, channels)).map(any)

        return self.narrowed(Entry(effect=record)).bind(lambda typed: derived(self, typed.effect))

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns ownerships and registrables."""
        return Ok((*ownerships(self.record), *self.registrables))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ENTRY_EDITS]


def revised[R: Effect](row: CompositorEffect[R], identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, change: Callable[[R], BlenderResult[R]]) -> BlenderResult[bool]:
    """Reads entry's record, writes its changed effect keeping the blend, answering whether any input changed."""
    return row.read(identity, entry).bind(lambda held: change(held.effect).bind(lambda effect: row.write(identity, scene, entry, msgspec.structs.replace(held, effect=effect))))


def stops(identity: Identity, scene: bpy.types.Scene, gain: CompositorEffect[Exposure]) -> BlenderResult[float]:
    """Reads the earliest gain entry's absolute stops, zero without one."""
    return gain.current(identity, scene).map(lambda held: held.map(lambda record: record.stops).default_value(0.0))


# --- [INTERFACES]


def hidden(face: Interface) -> Interface:
    """Hides derived sockets, keeping their stored values."""
    return msgspec.structs.replace(
        face, inputs=tuple(msgspec.structs.replace(socket, members=socket.members | {HIDDEN: True}) for socket in face.inputs), panels=tuple(hidden(panel) for panel in face.panels)
    )


def entry_interface(kind: type[Effect], masks: int, *, blended: bool = True) -> BlenderResult[Interface]:
    """Composes image sockets, record fields, frame values, and, for blended rows, blend and mask panels."""
    _, _, masks_field = Blend.__struct_fields__
    key = item_key(Shell.blend, masks_field)

    def masked_panels(face: Interface, items: tuple[Interface, ...]) -> Interface:
        return msgspec.structs.replace(
            face,
            panels=tuple(
                msgspec.structs.replace(
                    panel, panels=tuple(InterfacePanel(key=item_key(key, index), name=f"Mask {index}", inputs=item.inputs, panels=item.panels) for index, item in enumerate(items, 1))
                )
                if panel.key == key
                else panel
                for panel in face.panels
            ),
        )

    blend = (
        paired(
            Interface.of(Blend).map(lambda face: under(Shell.blend, face)), accumulated(Interface.of(Mask).map(lambda face: under(item_key(key, index), face)) for index in range(1, masks + 1))
        ).map(lambda pair: (InterfacePanel(key=Shell.blend, name=heading(Shell.blend), inputs=masked_panels(*pair).inputs, panels=masked_panels(*pair).panels),))
        if blended
        else Ok(())
    )
    return paired(Interface.of(kind), Interface.of(CameraFrame).map(lambda face: hidden(under(Shell.frame, face))), blend).map(
        lambda parts: Interface(
            outputs=(Socket(key=Shell.result, name="Image", kind=bpy.types.NodeSocketColor),),
            inputs=(Socket(key=Shell.image, name="Image", kind=bpy.types.NodeSocketColor), *parts[0].inputs, *parts[1].inputs),
            panels=(*parts[0].panels, *parts[2]),
        )
    )


def expanded(face: Interface, value: msgspec.Struct) -> BlenderResult[Interface]:
    """Builds collection item panels from record values."""
    collections = {field.key: (field.path, reached(value, field.path)) for field in Field.of(type(value), value=Some(value)) if field.collection.is_some() and active(value, field.path)}

    def expanded_panel(panel: InterfacePanel) -> BlenderResult[InterfacePanel]:
        def items(collection: tuple[KeyPath, object]) -> BlenderResult[InterfacePanel]:
            path, values = collection
            return accumulated(
                Interface
                .of(type(item))
                .bind(lambda inner: expanded(inner, item))
                .map(lambda inner: under(item_key(*path, index), inner))
                .map(lambda inner: InterfacePanel(key=item_key(*path, index), name=f"{panel.name} {index}", inputs=inner.inputs, panels=inner.panels))
                for index, item in enumerate(values if isinstance(values, tuple | list) else (), 1)
                if isinstance(item, msgspec.Struct)
            ).map(lambda found: msgspec.structs.replace(panel, panels=found))

        return (
            Option
            .of_optional(collections.get(panel.key))
            .map(items)
            .default_with(lambda: accumulated(map(expanded_panel, panel.panels)).map(lambda panels: msgspec.structs.replace(panel, panels=panels)))
        )

    return accumulated(map(expanded_panel, face.panels)).map(lambda panels: msgspec.structs.replace(face, panels=panels))


# --- [ENTRY_HOLDERS]


def ownerships(record: type[Effect]) -> tuple[Ownership[bpy.types.NodeTree], Ownership[bpy.types.NodeTree]]:
    """Declares shared and per-entry group ownerships."""
    key = item_key(record)
    return Ownership(kind=bpy.types.NodeTree, key=key, saved=Saved.USED), Ownership(kind=bpy.types.NodeTree, key=f"{key}_entry", saved=Saved.USED, single=False)


def entries(identity: Identity, scene: bpy.types.Scene, record: type[Effect]) -> tuple[bpy.types.SceneCompositorEffect, ...]:
    """Returns entries with either ownership mark of record in stack order."""
    return Marks.of(identity, scene).keyed(*(owner.key for owner in ownerships(record)))


def editable(scene: bpy.types.Scene) -> BlenderResult[bpy.types.Scene]:
    """Refuses compositor edits library override scenes cannot keep."""
    return Ok(scene) if scene.override_library is None else Error((BlenderEditFault(request=repr(scene.override_library), error=AttributeError(name="compositor_effects")),))


def configured(record: Effect) -> BlenderResult[PyOpenColorIO.Config]:
    """Returns live OCIO config resolving process space of spaced records."""
    return config_of((record.space,) if isinstance(record, Spaced) else ())


# --- [SCENE_VALUES]


def camera_frame(framed: Framed, scene: bpy.types.Scene) -> BlenderResult[CameraFrame]:
    """Reads pixel aspect, overscan camera share, and unit scale."""
    render = scene.render
    return framed.base(scene).map(
        lambda base: CameraFrame(
            aspect=render.pixel_aspect_x / render.pixel_aspect_y, scale=base.map(lambda original: original.size[0] / render.resolution_x).default_value(1.0), meters=scene.unit_settings.scale_length
        )
    )


def framing(identity: Identity) -> BlenderResult[Derived[Effect, CameraFrame]]:
    """Binds frame size settings into camera frame derivation and RNA reads."""
    return (
        FrameSize()
        .bound(identity)
        .map(
            lambda framed: Derived(
                record=Some(CameraFrame),
                of=lambda _identity, scene, _record: camera_frame(framed, scene),
                reads=tuple(
                    (owner, owner.bl_rna.properties[name])
                    for owner, name in (
                        (bpy.types.RenderSettings, "pixel_aspect_x"),
                        (bpy.types.RenderSettings, "pixel_aspect_y"),
                        (bpy.types.RenderSettings, "resolution_x"),
                        (bpy.types.RenderSettings, "resolution_y"),
                        (bpy.types.RenderSettings, "resolution_percentage"),
                        (bpy.types.UnitSettings, "scale_length"),
                    )
                ),
            )
        )
    )


def shuttered(scene: bpy.types.Scene) -> Shutter:
    """Reads shutter length and the opening offset `motion_blur_position` places in shutter lengths."""
    match scene.render.motion_blur_position:
        case "START":
            opening = 0.0
        case "CENTER":
            opening = -0.5
        case "END":
            opening = -1.0
    return Shutter(length=scene.render.motion_blur_shutter, opening=opening)


def precise(_record: Effect, layer: str) -> BlenderResult[PassRequest]:
    """Requests full compositor precision with no render pass."""
    return Ok(PassRequest(layer=layer, precise=True))


def depth_precise(_record: Effect, layer: str) -> BlenderResult[PassRequest]:
    """Requests the Depth pass at full compositor precision."""
    return PassRequest.depth(layer, precise=True)


def applied_channel(channel: Animation, scene: bpy.types.Scene) -> BlenderResult[bool]:
    """Writes changed keyframes, answering whether it wrote, refusing cycle-forming drivers."""
    match channel.source:
        case Driven():
            return Error((BlenderChoiceFault(path=channel.path, identifier=Driven.title, accepted=(Keys.title,)),))
        case Keys():
            return channel.apply(scene, bpy.context)


def _entered(key: Key, shape: native.Type) -> Expr:
    """Group input expression of one item key in the shape its record field takes."""
    match shape:
        case native.StructType(cls=kind) if (typing.get_origin(kind) or kind) is Rgb:
            return Expr(kind=bpy.types.NodeGroupInput, socket=key, shape="RGBA", label=Some(INPUTS))
        case native.TupleType(item_types=(native.FloatType(), *_)):
            return Expr(kind=bpy.types.NodeGroupInput, socket=key, shape="VECTOR", label=Some(INPUTS))
        case _:
            return Expr(kind=bpy.types.NodeGroupInput, socket=key, label=Some(INPUTS))


# --- [COMPOSITION] ----------------------------------------------------------------------

shutter = Derived[Effect, Shutter](
    record=Some(Shutter),
    of=lambda _identity, scene, _record: Ok(shuttered(scene)),
    reads=tuple((bpy.types.RenderSettings, bpy.types.RenderSettings.bl_rna.properties[name]) for name in ("motion_blur_shutter", "motion_blur_position")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "DESCRIPTION",
    "HIDDEN",
    "PRECISION",
    "Alpha",
    "Animated",
    "CameraFrame",
    "CompositorEffect",
    "Context",
    "Derived",
    "EffectName",
    "Enabled",
    "Inspected",
    "Marks",
    "Measured",
    "Named",
    "Opened",
    "Preparing",
    "Restoring",
    "Shell",
    "Shutter",
    "applied_channel",
    "camera_frame",
    "configured",
    "depth_precise",
    "editable",
    "entries",
    "entry_interface",
    "expanded",
    "framing",
    "hidden",
    "ownerships",
    "precise",
    "revised",
    "shutter",
    "shuttered",
    "stops",
]
