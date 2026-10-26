"""Scene compositor effects: the effect declared from a record with its parts, the graph context its builder reads, and the entries a scene holds for it."""

from collections.abc import Callable, Mapping
from contextlib import ExitStack
from enum import auto, Enum
import inspect
import typing
from typing import Annotated, ClassVar, Final, Literal, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import msgspec.inspect
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
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderResult
from blender.bpy.nodes.changes import Changes, made, reconciled
from blender.bpy.nodes.graphs import DataType, End, Expr, group_input, INPUTS, Link, menu, Node, NodeGroup, OUTPUTS, Term
from blender.bpy.nodes.interface import addresses, Interface, InterfacePanel, item_key, MenuItems, slot, Socket, under
from blender.bpy.nodes.trees import socket_at
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.compositor.blending import Blend, Entry, mixed_back
from blender.bpy.render.compositor.colors import converted, pointwise
from blender.bpy.render.compositor.coordinates import framed, FrameSpace, Shutter
from blender.bpy.render.compositor.inputs import active, case_values, entered, entry_input, Field, graph_values, inputs, KeyPath, reached, stored_graphs
from blender.bpy.render.compositor.masks import coverage, Mask
from blender.bpy.render.outputs.framing import Framed, FrameSize, Framing
from blender.bpy.render.view_layers.passes import PassRequest, required
from blender.bpy.settings.attachments import Stored
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.color.processors import ColorSpace, Domain
from blender.imaging.files.images import ImageFile
from blender.imaging.sampling.integration import Estimate, Precision
from blender.records.faults import accumulated, DomainFault, paired
from blender.records.fields import Case, Gated, heading, Rgb

# --- [TYPES] ----------------------------------------------------------------------------

type EffectName = Annotated[str, msgspec.Meta(min_length=1)]
type Context = Option[tuple[tuple[int, int], Framing]]
type Restoring = Callable[[], BlenderResult[None]]
type Prepared[R] = tuple[R, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]
type Part[R] = Requests[R] | Derives[R, msgspec.Struct] | Animates[R] | Checks[R] | Conditions[R] | Prepares[R] | Spaces[R] | Opens[R] | Seeds[R] | Measures[R] | Inspects | Holds | Inputs | Registers
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
        return Descriptor(root=bpy.types.SceneCompositorEffect, owner="", prop=self.value)


# --- [CONSTANTS] ------------------------------------------------------------------------

DESCRIPTION: Final = Descriptor.declared(bpy.types.CompositorNodeTree, "description", bpy.types.StringProperty)
PRECISION: Final = Descriptor.declared(bpy.types.Scene, "render.compositor_precision", bpy.types.EnumProperty)
HIDDEN: Final = Descriptor.declared(bpy.types.NodeTreeInterfaceSocket, "hide_in_modifier", bpy.types.BoolProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Named(msgspec.Struct, frozen=True, kw_only=True):
    """Compositor entry selected by name."""

    name: EffectName


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
    """Graph builder's record, entry input color, render layer, group input readers, menu items, retained cases, and prepared images."""

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
        match next((field for field in Field.of(type(self.record), value=Some(self.record)) if field.path == path), None):
            case Field(shape=msgspec.inspect.StructType(cls=kind)) if (typing.get_origin(kind) or kind) is Gated:
                gated = Field.of(kind)[0]
                return _entered(item_key(*path, gated.info.name), gated.shape)
            case Field(shape=shape):
                return _entered(item_key(*path), shape)
            case None:
                return _entered(item_key(*path), msgspec.inspect.FloatType())

    def gate(self, *path: Key | int | type[Case]) -> Expr:
        """Reads the enabled socket of a gated record field."""
        return _entered(item_key(*path), msgspec.inspect.BoolType())

    def passed(self) -> BlenderResult[Expr]:
        """Returns the entry input, the graph of effects whose preparation renders the result."""
        return Ok(self.color)

    @property
    def space(self) -> FrameSpace:
        """Camera frame of the entry input."""
        aspect, scale, _ = CameraFrame.__struct_fields__
        return framed(self.color, self.input(Shell.frame, aspect), self.input(Shell.frame, scale))

    def layer(self, socket: str) -> Expr:
        """Reads one Render Layers output of the effect's pass request layer."""
        return msgspec.structs.replace(self.render, socket=socket)

    def menu(self, *path: Key | int | type[Case], cases: Mapping[str, Term], kind: DataType | Literal["MENU", "BOOLEAN"]) -> Expr:
        """Switches between case terms by item name at a choice or case union field's menu input."""
        return menu(self.input(*path), self.items[path], cases, kind=kind)

    def case[C: Case](self, kind: type[C], *parts: Key | int | type[Case]) -> Option[C]:
        """Returns active or retained case of kind at path."""
        return Option.of_optional(next((value for value in self.cases[parts] if isinstance(value, kind)), None))


class Requests[R](msgspec.Struct, frozen=True, kw_only=True):
    """Render passes and compositor precision the graph reads from the named view layer."""

    request: Callable[[R, str], BlenderResult[PassRequest]]


class Derives[R, D: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Hidden inputs written from scene values: a record of hidden sockets, else values by input key, rewritten when one of `reads` changes."""

    of: Callable[[Identity, bpy.types.Scene, R], BlenderResult[D | Mapping[Key, Value]]]
    record: Option[type[D]] = Nothing
    reads: "tuple[tuple[type[bpy.types.bpy_struct[object]], bpy.types.Property], ...]" = ()


class Animates[R](msgspec.Struct, frozen=True, kw_only=True):
    """Keyframes of entry inputs derived from source channels."""

    keys: Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, R], BlenderResult[tuple[Animation, ...]]]


class Checks[R](msgspec.Struct, frozen=True, kw_only=True):
    """Scene refusal of a record over keyed inputs, the held entry when one exists."""

    check: Callable[[Identity, bpy.types.Scene, Option[bpy.types.SceneCompositorEffect], R, Context], BlenderResult[None]]


class Conditions[R](msgspec.Struct, frozen=True, kw_only=True):
    """Context condition a record needs to build."""

    condition: Callable[[R], Condition]


class Prepares[R](msgspec.Struct, frozen=True, kw_only=True):
    """CPU staging of the entering image into the record and named images, for records `when` selects."""

    prepare: Callable[[Identity, bpy.types.Scene, R, PyOpenColorIO.Config, OpenImageIO.ImageBuf, Precision, CameraFrame, bpy.types.SceneCompositorEffect], BlenderResult[Prepared[R]]]
    when: Callable[[R], bool] = lambda _record: True


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


class Measures[R](msgspec.Struct, frozen=True, kw_only=True):
    """Record revision from the frame entering the effect's entry, and the command arming it."""

    revise: Callable[[PyOpenColorIO.Config, R, ImageFile[np.float32]], BlenderResult[Option[R]]]
    command: Option[Operator[Named]] = Nothing


class Inspects(msgspec.Struct, frozen=True, kw_only=True):
    """Graph output a Viewer shows under its own command, titled `View <Key Heading>`."""

    key: Key
    end: End
    description: str


class Holds(msgspec.Struct, frozen=True, kw_only=True):
    """Host values held while an entry exists and restored on removal: scene and camera values, and the scene writes beyond them."""

    scene: Mapping[str, Value] = {}
    cameras: Mapping[str, Value] = {}
    placed: Callable[[Identity, bpy.types.Scene], BlenderResult[None]] = lambda _identity, _scene: Ok(None)
    held: Callable[[Identity, bpy.types.Scene], BlenderResult[None]] = lambda _identity, _scene: Ok(None)
    restored: Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect], BlenderResult[Restoring]] = lambda _identity, _scene, _entry: Ok(lambda: Ok(None))


class Inputs(msgspec.Struct, frozen=True, kw_only=True):
    """Group sockets a record cannot hold, such as an object pointer."""

    sockets: tuple[Socket, ...]


class Registers(msgspec.Struct, frozen=True, kw_only=True):
    """Commands and owners the effect registers beside its group ownerships."""

    rows: tuple[Registrable, ...]


class Blended(msgspec.Struct, frozen=True, kw_only=True):
    """Result mixed back over a base through blend mode, opacity, and masks, the entry input unless a graph end in the operation's space."""

    base: Option[End] = Nothing


class Replaced(msgspec.Struct, frozen=True, kw_only=True):
    """Result replacing the entry input with no blend inputs."""


class CompositorEffect[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Scene compositor effect of one record: its graph builder, encoding domain, alpha form, mix, and the parts deciding what else it does."""

    record: type[R]
    graph: Callable[[Shell[R]], BlenderResult[Expr]]
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
    def description(self) -> str:
        """Record documentation describing the effect's group and commands."""
        return Option.of_optional(inspect.getdoc(self.record)).default_value("")

    @property
    def requests(self) -> bool:
        """Whether the effect makes a render pass request."""
        return bool(self.found(Requests))

    @property
    def staged(self) -> bool:
        """Whether the effect stages entering images on the CPU."""
        return bool(self.found(Prepares))

    def staging(self, record: R) -> bool:
        """Whether the record's entering image passes through CPU staging before its graph reads it."""
        return any(part.when(record) for part in self.found(Prepares))

    @property
    def imaged(self) -> bool:
        """Whether the group reads render passes or images, rebuilt on every refresh."""
        return self.requests or any(GraphForm.IMAGE in field.forms for field in Field.of(self.record))

    @property
    def reads(self) -> "tuple[tuple[type[bpy.types.bpy_struct[object]], bpy.types.Property], ...]":
        """RNA members derived inputs read."""
        return tuple(read for derives in self.found(Derives) for read in derives.reads)

    @property
    def holds(self) -> dict[str, Value]:
        """Scene values held while an entry exists."""
        return {path: value for part in self.found(Holds) for path, value in part.scene.items()}

    @property
    def cameras(self) -> dict[str, Value]:
        """Camera values held while an entry exists."""
        return {path: value for part in self.found(Holds) for path, value in part.cameras.items()}

    def seeded(self, scene: bpy.types.Scene) -> BlenderResult[R]:
        """Returns scene seed, record defaults, or absent file fault for opened effects."""
        return (
            self
            .first(Seeds)
            .map(lambda seeds: Ok(seeds.seed(scene)))
            .default_with(lambda: Error((BlenderAbsentFault(owner=self.record.title, key="file"),)) if self.found(Opens) else Ok(self.record()))
        )

    def ownership(self, record: Entry[Case]) -> Ownership[bpy.types.NodeTree]:
        """Selects per-entry ownership for staged, graphed, collection, or masked records."""
        shared, personal = ownerships(self.record)
        return personal if self.staged or record.blend.masks or any(field.graphed or field.collection.is_some() for field in Field.of(self.record)) else shared

    def config(self, record: R) -> BlenderResult[PyOpenColorIO.Config]:
        """Returns live OCIO config resolving the record's operation spaces."""
        return config_of(tuple(part.space(record) for part in self.found(Spaces)))

    # --- [HOLDS]

    def placing(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[None]:
        """Prepares the scene before the effect's first entry is placed."""
        return accumulated(part.placed(identity, scene) for part in self.found(Holds)).map(lambda _: None)

    def holding(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[None]:
        """Writes scene state held beyond the declared values while the effect has an entry."""
        return accumulated(part.held(identity, scene) for part in self.found(Holds)).map(lambda _: None)

    def removing(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[Restoring]:
        """Reads what the effect restores once its entry and holds are gone, answering the restoring step."""
        return accumulated(part.restored(identity, scene, entry) for part in self.found(Holds)).map(lambda steps: lambda: accumulated(step() for step in steps).map(lambda _: None))

    # --- [ENTRIES]

    def entries(self, identity: Identity, scene: bpy.types.Scene) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns owned entries of the effect in stack order."""
        return entries(identity, scene, self.record)

    def read(self, identity: Identity, entry: bpy.types.SceneCompositorEffect, *, edited: bool = True, graphs: Mapping[Key, object] = {}) -> BlenderResult[Entry[R]]:
        """Reads graph store and group inputs into the entry record and its blend."""
        blended = isinstance(self.mix, Blended)
        records = (((), self.record, Nothing, self.domain), *((((Shell.blend,), Blend, Nothing, self.domain),) if blended else ()))

        def decoded(values: Mapping[Key, object]) -> BlenderResult[Entry[R]]:
            return paired(inputs(identity, entry, (), self.record, values), inputs(identity, entry, (Shell.blend,), Blend, values) if blended else Ok(Blend())).map(
                lambda pair: Entry(record=pair[0], blend=pair[1])
            )

        return graph_values(identity, entry, records, edited=edited).bind(lambda values: decoded({**values, **graphs}))

    def current(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[Option[R]]:
        """Reads earliest owned record, Nothing when absent."""
        return Option.of_optional(next(iter(self.entries(identity, scene)), None)).map(lambda entry: self.read(identity, entry).map(lambda record: Some(record.record))).default_value(Ok(Nothing))

    def named(self, identity: Identity, scene: bpy.types.Scene, name: str) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Finds named entry among the effect's owned groups."""
        return Option.of_optional(next((entry for entry in self.entries(identity, scene) if entry.name == name), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=name),)
        )

    def write(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        entry: bpy.types.SceneCompositorEffect,
        record: Entry[Case],
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
                lambda typed: paired(self.config(typed.record), self.met(identity, scene, typed.record, Some(entry), context, source, prepared=prepared)).bind(
                    lambda parts: self.made(identity, scene, typed, parts[0], context, parts[1], Option.of_optional(entry.node_group), images=images).bind(
                        lambda built: written(entry, "node_group", built[0]).bind(
                            lambda pointed: self.filled(identity, scene, entry, built[0], typed, source).map(lambda filled: built[1] or pointed.is_some() or filled)
                        )
                    )
                )
            )
        )

    def appended(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[Case], *, context: Context = Nothing, name: Option[EffectName] = Nothing
    ) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Creates entry once requirements and group succeed."""
        label = vacant(name.default_value(self.record.title), (entry.name for entry in scene.compositor_effects))

        def appended(group: bpy.types.NodeTree, typed: Entry[R]) -> BlenderResult[bpy.types.SceneCompositorEffect]:
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
                lambda typed: paired(self.config(typed.record), self.met(identity, scene, typed.record, Nothing, context)).bind(
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
                lambda record: (Ok(None) if self.staging(record.record) else self.checked(identity, scene, Some(entry), record.record, Nothing)).bind(
                    lambda _: self.derive(identity, scene, entry, record.record)
                )
            )
        )

    def prepared(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[Case], config: PyOpenColorIO.Config, image: OpenImageIO.ImageBuf, precision: Precision, entry: bpy.types.SceneCompositorEffect
    ) -> BlenderResult[tuple[Entry[R], Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
        """Prepares effective record and named images, keeping authored blend."""
        return self.narrowed(record).bind(
            lambda typed: (
                self
                .first(Prepares)
                .map(
                    lambda part: (
                        inputs(identity, entry, (Shell.frame,), CameraFrame)
                        .bind(lambda frame: part.prepare(identity, scene, typed.record, config, image, precision, frame, entry))
                        .map(lambda value: (msgspec.structs.replace(typed, record=value[0]), value[1]))
                    )
                )
                .default_value(Ok((typed, {})))
            )
        )

    # --- [GROUPS]

    def grouped(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[Case],
        config: PyOpenColorIO.Config,
        *,
        entry: Option[bpy.types.SceneCompositorEffect] = Nothing,
        context: Context = Nothing,
        prepared: bool = False,
        images: Mapping[Key, Expr] = {},
    ) -> BlenderResult[NodeGroup]:
        """Builds entry group from graph builder and blend."""
        return self.narrowed(record).bind(
            lambda typed: self.met(identity, scene, typed.record, entry, context, prepared=prepared).bind(
                lambda request: self.built(identity, scene, typed, config, entry.bind(lambda value: Option.of_optional(value.node_group)), context, request, images=images)
            )
        )

    def narrowed(self, entry: Entry[Case]) -> BlenderResult[Entry[R]]:
        """Narrows an entry to the effect's record type."""
        held = entry.record
        return Ok(Entry(record=held, blend=entry.blend)) if isinstance(held, self.record) else Error((DomainFault(subject=self.kind, value=item_key(type(held)), accepted=self.kind),))

    def checked(self, identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], record: R, context: Context) -> BlenderResult[None]:
        """Runs every scene check of the record."""
        return accumulated(part.check(identity, scene, entry, record, context) for part in self.found(Checks)).map(lambda _: None)

    def met(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: R,
        entry: Option[bpy.types.SceneCompositorEffect],
        context: Context,
        source: Option[bpy.types.Scene] = Nothing,
        *,
        prepared: bool = False,
    ) -> BlenderResult[Option[PassRequest]]:
        """Checks conditions and scene checks, then makes the pass request."""
        reading = source.default_value(scene)
        return paired(
            accumulated(check(part.condition(record), bpy.context, Some(reading)) for part in self.found(Conditions)),
            self.checked(identity, reading, entry, record, context) if prepared or not self.staging(record) else Ok(None),
        ).bind(
            lambda _: (
                self
                .first(Requests)
                .map(lambda part: part.request(record, scene.view_layers[0].name).bind(lambda found: required(scene, bpy.context, (found,), PRECISION).map(lambda _: Some(found))))
                .default_value(Ok(Nothing))
            )
        )

    def built(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[Case],
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
        space = Option.of_optional(next((part.space(typed.record) for part in self.found(Spaces)), None)).filter(lambda name: name != PyOpenColorIO.ROLE_SCENE_LINEAR)
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
            items = {field.path: held for field in Field.of(self.record, value=Some(typed.record)) for held in field.menu.to_list()}

            def operation(entering: Expr) -> BlenderResult[Expr]:
                return (
                    space
                    .map(lambda name: converting("space_in", PyOpenColorIO.ROLE_SCENE_LINEAR, name)(entering))
                    .default_value(Ok(entering))
                    .bind(
                        lambda source: self.graph(Shell(record=typed.record, identity=identity, config=config, color=source, render=render, items=items, cases=cases, images=images, context=context))
                    )
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
            match self.mix:
                case Replaced():
                    return assembled(Some(made.keeping(*kept)), face)
                case Blended(base=base):
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
                    beneath: BlenderResult[Term] = base.map(
                        lambda end: space.map(lambda name: converting("space_base", name, PyOpenColorIO.ROLE_SCENE_LINEAR)(end)).default_value(Ok(end))
                    ).default_value(Ok(original))
                    aspect, scale, _ = (item_key(Shell.frame, name) for name in CameraFrame.__struct_fields__)
                    mode, opacity, masks = (item_key(Shell.blend, name) for name in Blend.__struct_fields__)
                    weight = group_input(opacity)
                    weighted = coverage(len(typed.blend.masks), original, group_input(aspect), group_input(scale), lambda key: group_input(item_key(masks, key)), config).map(
                        lambda mask: mask.map(lambda held: weight * held).default_value(weight)
                    )
                    return (
                        paired(beneath, weighted)
                        .bind(lambda parts: mixed_back(Shell.blend, parts[0], restored, group_input(mode), parts[1]))
                        .bind(lambda value: assembled(Some(value.keeping(*kept)), face))
                    )

        interface = (
            entry_interface(self.record, len(typed.blend.masks), blended=isinstance(self.mix, Blended))
            .bind(lambda face: expanded(face, typed.record))
            .map(lambda face: msgspec.structs.replace(face, inputs=(*face.inputs, *(socket for part in self.found(Inputs) for socket in part.sockets))))
        )
        derived = accumulated(Interface.of(kind).map(hidden) for part in self.found(Derives) for kind in part.record.to_list())
        combined = paired(interface, derived).map(
            lambda pair: msgspec.structs.replace(
                pair[0], inputs=(*pair[0].inputs, *(socket for face in pair[1] for socket in face.inputs)), panels=(*pair[0].panels, *(panel for face in pair[1] for panel in face.panels))
            )
        )
        if self.staging(typed.record) and not images:
            return combined.bind(lambda face: assembled(Nothing, face))
        return paired(combined, case_values(typed.record, tree, identity)).bind(lambda parts: body(parts[1]).bind(lambda made: finished(made, parts[0])))

    def made(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[R],
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
        self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree, typed: Entry[R], source: Option[bpy.types.Scene] = Nothing
    ) -> BlenderResult[bool]:
        """Stores graph values and writes record and blend inputs, then derived values, answering whether any changed."""
        parts: tuple[tuple[KeyPath, msgspec.Struct], ...] = (((), typed.record), *((((Shell.blend,), typed.blend),) if isinstance(self.mix, Blended) else ()))
        return accumulated((
            stored_graphs(identity, group, tuple((prefix, value, self.domain) for prefix, value in parts)),
            *(entered(identity, entry, prefix, value) for prefix, value in parts),
        )).bind(lambda outcomes: self.derive(identity, scene, entry, typed.record, source).map(lambda refreshed: any(outcomes) or refreshed))

    def derive(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, record: R, source: Option[bpy.types.Scene] = Nothing) -> BlenderResult[bool]:
        """Writes camera frame, derived values, and keyed channels, answering whether any changed."""
        reading = source.default_value(scene)

        def keyed(values: msgspec.Struct | Mapping[Key, Value]) -> BlenderResult[bool]:
            match values:
                case msgspec.Struct():
                    return entered(identity, entry, (), values)
                case _:
                    return accumulated(entry_input(identity, entry, key).bind(lambda owner: written(owner, "value", value)).map(Option.is_some) for key, value in values.items()).map(any)

        frame = FrameSize().bound(identity).bind(lambda framed: camera_frame(framed, reading)).bind(lambda value: entered(identity, entry, (Shell.frame,), value))
        values = accumulated(part.of(identity, reading, record).bind(keyed) for part in self.found(Derives)).map(any)
        channels = accumulated(
            part.keys(identity, scene, entry, record).bind(lambda found: accumulated(applied_channel(channel, scene) for channel in found).map(any)) for part in self.found(Animates)
        ).map(any)
        return accumulated((frame, values, channels)).map(any)

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns ownerships and registered rows."""
        return Ok((*ownerships(self.record), *(row for part in self.found(Registers) for row in part.rows)))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [ENTRY_EDITS]


def revised[R: Case](effect: CompositorEffect[R], identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, change: Callable[[R], BlenderResult[R]]) -> BlenderResult[bool]:
    """Reads entry's record, writes its changed record keeping the blend, answering whether any input changed."""
    return effect.read(identity, entry).bind(lambda held: change(held.record).bind(lambda record: effect.write(identity, scene, entry, msgspec.structs.replace(held, record=record))))


# --- [INTERFACES]


def hidden(face: Interface) -> Interface:
    """Hides derived sockets, keeping their stored values."""
    return msgspec.structs.replace(
        face, inputs=tuple(msgspec.structs.replace(socket, members=socket.members | {HIDDEN: True}) for socket in face.inputs), panels=tuple(hidden(panel) for panel in face.panels)
    )


def entry_interface(kind: type[msgspec.Struct], masks: int, *, blended: bool = True) -> BlenderResult[Interface]:
    """Composes image sockets, record fields, frame values, and, for blended effects, blend and mask panels."""
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
        paired(Interface.of(Blend).map(lambda face: under(Shell.blend, face)), accumulated(Interface.of(Mask).map(lambda face: under(item_key(key, index), face)) for index in range(1, masks + 1)))
        .map(lambda pair: masked_panels(*pair))
        .map(lambda face: (InterfacePanel(key=Shell.blend, name=heading(Shell.blend), inputs=face.inputs, panels=face.panels),))
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


def ownerships(record: type[Case]) -> tuple[Ownership[bpy.types.NodeTree], Ownership[bpy.types.NodeTree]]:
    """Declares shared and per-entry group ownerships."""
    key = item_key(record)
    return Ownership(kind=bpy.types.NodeTree, key=key, saved=Saved.USED), Ownership(kind=bpy.types.NodeTree, key=f"{key}_entry", saved=Saved.USED, single=False)


def entries(identity: Identity, scene: bpy.types.Scene, record: type[Case]) -> tuple[bpy.types.SceneCompositorEffect, ...]:
    """Returns entries with either ownership mark of record in stack order."""
    return Marks.of(identity, scene).keyed(*(owner.key for owner in ownerships(record)))


def editable(scene: bpy.types.Scene) -> BlenderResult[bpy.types.Scene]:
    """Refuses compositor edits library override scenes cannot keep."""
    return Ok(scene) if scene.override_library is None else Error((BlenderEditFault(request=repr(scene.override_library), error=AttributeError(name="compositor_effects")),))


# --- [SCENE_VALUES]


def camera_frame(framed: Framed, scene: bpy.types.Scene) -> BlenderResult[CameraFrame]:
    """Reads pixel aspect, overscan camera share, and unit scale."""
    render = scene.render
    return framed.base(scene).map(
        lambda base: CameraFrame(
            aspect=render.pixel_aspect_x / render.pixel_aspect_y, scale=base.map(lambda original: original.size[0] / render.resolution_x).default_value(1.0), meters=scene.unit_settings.scale_length
        )
    )


def framing(identity: Identity) -> BlenderResult[Derives[Case, CameraFrame]]:
    """Binds frame size settings into camera frame derivation and the RNA members it reads."""
    return (
        FrameSize()
        .bound(identity)
        .map(
            lambda framed: Derives(
                record=Some(CameraFrame),
                of=lambda _identity, scene, _record: camera_frame(framed, scene),
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


def precise(_record: Case, layer: str) -> BlenderResult[PassRequest]:
    """Requests full compositor precision with no render pass."""
    return Ok(PassRequest(layer=layer, precise=True))


def depth_precise(_record: Case, layer: str) -> BlenderResult[PassRequest]:
    """Requests the Depth pass at full compositor precision."""
    return PassRequest.depth(layer, precise=True)


def applied_channel(channel: Animation, scene: bpy.types.Scene) -> BlenderResult[bool]:
    """Writes changed keyframes, answering whether it wrote, refusing cycle-forming drivers."""
    match channel.source:
        case Driven():
            return Error((DomainFault(subject=channel.path, value=Driven.title, accepted=Keys.title),))
        case Keys():
            return channel.apply(scene, bpy.context)


def _entered(key: Key, shape: msgspec.inspect.Type) -> Expr:
    """Group input expression of one item key in the shape its record field takes."""
    match shape:
        case msgspec.inspect.StructType(cls=kind) if (typing.get_origin(kind) or kind) is Rgb:
            return Expr(kind=bpy.types.NodeGroupInput, socket=key, shape="RGBA", label=Some(INPUTS))
        case msgspec.inspect.TupleType(item_types=(msgspec.inspect.FloatType(), *_)):
            return Expr(kind=bpy.types.NodeGroupInput, socket=key, shape="VECTOR", label=Some(INPUTS))
        case _:
            return Expr(kind=bpy.types.NodeGroupInput, socket=key, label=Some(INPUTS))


# --- [COMPOSITION] ----------------------------------------------------------------------

shutter = Derives[Case, Shutter](
    record=Some(Shutter),
    of=lambda _identity, scene, _record: Ok(Shutter.of(scene)),
    reads=tuple((bpy.types.RenderSettings, bpy.types.RenderSettings.bl_rna.properties[name]) for name in ("motion_blur_shutter", "motion_blur_position")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "DESCRIPTION",
    "HIDDEN",
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
    "Inputs",
    "Inspects",
    "Marks",
    "Measures",
    "Mix",
    "Named",
    "Opens",
    "Part",
    "Prepared",
    "Prepares",
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
]
