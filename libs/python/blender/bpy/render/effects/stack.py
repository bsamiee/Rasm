"""Effect stack rows, entry values, owned groups, and display formation."""

from collections import deque
from collections.abc import Callable, Generator, Iterator, Mapping
from contextlib import contextmanager, ExitStack
from enum import auto, Enum
from functools import reduce
import inspect
from operator import or_
import typing
from typing import Annotated, ClassVar, Generic, Literal, Protocol, TypeVar

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import msgspec.inspect as native
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO
from typing_extensions import TypeForm

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Call, Done, Empty, Open, Operator, Outcome, Unchanged
from blender.bpy.data.ids.images import produced, productions
from blender.bpy.data.ids.ownership import Mark, Ownership, Saved
from blender.bpy.data.ids.store import Handle, named, vacant
from blender.bpy.data.rna.animation import Animation, Driven, Keys, sampled
from blender.bpy.data.rna.conditions import check, Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor, IdName, struct_at, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import FrameHandler, PathHandler, SceneHandler, UpdateHandler
from blender.bpy.extension.events.subscriptions import Subscription
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions, Registrable
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderCollisionFault, BlenderEditFault, BlenderFaults, BlenderImagingFault, BlenderReadFault, BlenderResult
from blender.bpy.files.interchange import Importing, Interchange
from blender.bpy.nodes.changes import Changes, made, owned, reconciled
from blender.bpy.nodes.graphs import End, ended, Fragment, INPUTS, Link, Node, node, NodeGroup, Operand, OUTPUTS
from blender.bpy.nodes.interface import addresses, Interface, InterfacePanel, item_key, MenuItems, slot, Socket, under
from blender.bpy.nodes.trees import active_output, socket_at
from blender.bpy.render.color.exposure import Gain
from blender.bpy.render.color.luts import Enabled
from blender.bpy.render.color.spaces import config_of, converted, pointwise
from blender.bpy.render.effects.compositing.blending import mixed_back, product
from blender.bpy.render.effects.compositing.masks import masked
from blender.bpy.render.outputs.formats import owners
from blender.bpy.render.outputs.framing import Framed, FrameSize
from blender.bpy.render.view_layers.passes import PassRequest, required
from blender.bpy.scene import cameras
from blender.bpy.settings.attachments import Stored
from blender.bpy.settings.properties import RecordGroup, Unit
from blender.bpy.settings.shapes import unwrapped, vector
from blender.bpy.ui.layout.rows import Member, OperatorButton, realized, RowDraw
from blender.bpy.ui.layout.templates import Curve
from blender.bpy.ui.surfaces.menus import Menu, Submenus
from blender.imaging.chain import Domain, Effect, Stage
from blender.imaging.color.processors import bracketed, DisplayView, Spaced
from blender.imaging.compositing.blending import Blend, Entry
from blender.imaging.compositing.masks import Mask
from blender.imaging.effects.generators.noise import Estimate, Precision
from blender.imaging.faults import accumulated, ImagingResult, paired
from blender.imaging.parameters import Case, Choice, ColorMode, Gated, GraphForm, HuePath, Interpolation, Knot, markers, Presentation, Ramp, Rgb, Stop
from blender.rendering.outputs.framing import Framing

# --- [TYPES] ----------------------------------------------------------------------------

type KeyPath = tuple[Key | int | type[Case], ...]
type Built[R: Effect] = Callable[[Shell[R]], BlenderResult[tuple[Fragment, End]]]
type Context = Option[tuple[tuple[int, int], Framing]]
type Requested[R: Effect] = Callable[[R, str], BlenderResult[PassRequest]]
type Animated[R: Effect] = Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, R], BlenderResult[tuple[Animation, ...]]]
type Preparing[R: Effect] = Callable[
    [Identity, bpy.types.Scene, R, PyOpenColorIO.Config, OpenImageIO.ImageBuf, Precision, CameraFrame, bpy.types.SceneCompositorEffect],
    BlenderResult[tuple[R, Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]],
]
type EffectName = Annotated[str, msgspec.Meta(min_length=1)]
_R_co = TypeVar("_R_co", bound=Effect, covariant=True)
_E_co = TypeVar("_E_co", bound=Effect, default=Effect, covariant=True)


class Coverage(Enum):
    """Output alpha source, input alpha or result coverage."""

    INPUT = auto()
    RESULT = auto()


class Edge(Enum):
    """Stack end holding unranked pinned group."""

    HEAD = auto()
    TAIL = auto()


class Toward(Choice):
    """Move target, neighbor or processing rank."""

    UP = 1, "Up", "Move effect up the stack"
    DOWN = 2, "Down", "Move effect down the stack"
    RANK = 3, "Processing Order", "Move effect into processing order"


class ShutterPosition(Enum):
    """Shutter opening offset in shutter lengths."""

    START = 0.0
    CENTER = -0.5
    END = -1.0


class Placeable(Registrable, Protocol[_E_co]):
    """Offered stack row protocol over any effect record."""

    @property
    def record(self) -> type[_E_co]:
        """Effect record type."""
        ...

    @property
    def registrables(self) -> tuple[Registrable, ...]:
        """Row-owned declarations."""
        ...

    @property
    def holds(self) -> Mapping[str, Value]:
        """Scene settings held while row has an entry."""
        ...

    @property
    def cameras(self) -> Mapping[str, Value]:
        """Camera settings row requires."""
        ...

    @property
    def opened(self) -> Option["Opened[_E_co]"]:
        """File open declaration of rows taking a file."""
        ...

    @property
    def kind(self) -> Key:
        """Record-derived ownership key."""
        ...

    @property
    def editors(self) -> tuple["CurveEditor", ...]:
        """Native curve nodes holding several knot fields of the record."""
        ...

    @property
    def requests(self) -> bool:
        """Whether row makes render pass request."""
        ...

    @property
    def staged(self) -> bool:
        """Whether row prepares entering image before group build."""
        ...

    def prepared(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[Effect], config: PyOpenColorIO.Config, image: OpenImageIO.ImageBuf, precision: Precision, entry: bpy.types.SceneCompositorEffect
    ) -> BlenderResult[tuple[Entry[_E_co], Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
        """Prepares effective record and named images in job scene."""
        ...

    @property
    def initial_view(self) -> Option[Callable[[PyOpenColorIO.Config, bpy.types.Scene], BlenderResult[DisplayView]]]:
        """Display View placed with first entry when scene holds none."""
        ...

    @property
    def reads(self) -> "tuple[tuple[type[bpy.types.bpy_struct[object]], bpy.types.Property], ...]":
        """RNA members refreshing derived inputs on write."""
        ...

    def seeded(self, scene: bpy.types.Scene) -> BlenderResult[_E_co]:
        """Returns initial effect for new entry, or absent file fault for opened rows."""
        ...

    def read(self, identity: Identity, entry: bpy.types.SceneCompositorEffect, *, edited: bool = True, graphs: Mapping[Key, object] = {}) -> BlenderResult[Entry[_E_co]]:
        """Reads entry inputs and graph store."""
        ...

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
        images: Mapping[Key, tuple[Fragment, End]] = {},
    ) -> BlenderResult[bool]:
        """Reconciles group and writes record inputs, answering whether any changed."""
        ...

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
        images: Mapping[Key, tuple[Fragment, End]] = {},
    ) -> BlenderResult[NodeGroup]:
        """Builds owned group for record."""
        ...

    def named(self, identity: Identity, scene: bpy.types.Scene, name: str) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Finds named entry of row."""
        ...

    def appended(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[Effect], *, context: Context = Nothing, name: Option[EffectName] = Nothing
    ) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Creates entry once requirements and group succeed."""
        ...

    def refreshed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[bool]:
        """Rewrites derived inputs and keyed channels, answering whether any changed."""
        ...

    def ownership(self, record: Entry[Effect]) -> Ownership[bpy.types.NodeTree]:
        """Selects shared or per-entry group ownership."""
        ...


class GainRow(Placeable, Gain, Protocol):
    """Exposure row taking scene view exposure on Display View placement."""


# --- [MODELS] ---------------------------------------------------------------------------


class Opened(msgspec.Struct, Generic[_R_co], frozen=True, kw_only=True):
    """File extensions and effect built from selected path."""

    extensions: Callable[[], tuple[str, ...]]
    of: Callable[[str], _R_co]


class Named(msgspec.Struct, frozen=True, kw_only=True):
    """Compositor entry selected by name."""

    name: EffectName


class Shift(Named, frozen=True, kw_only=True):
    """Named entry and requested move."""

    toward: Toward = Toward.RANK


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


class Derived[R: Effect, D: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Hidden record inputs written from scene values or keyed channels."""

    record: type[D]
    of: Option[Callable[[Identity, bpy.types.Scene, R], BlenderResult[D]]] = Nothing
    reads: "tuple[tuple[type[bpy.types.bpy_struct[object]], bpy.types.Property], ...]" = ()


class Shell[R: Effect](msgspec.Struct, frozen=True, kw_only=True):
    """Graph builder record and group input ends."""

    record: R
    identity: Identity
    config: PyOpenColorIO.Config
    source: End
    aspect: End
    scale: End
    meters: End
    items: Mapping[KeyPath, MenuItems]
    cases: Mapping[KeyPath, tuple[Case, ...]]
    images: Mapping[Key, End] = {}
    context: Context = Nothing
    image: ClassVar[Key] = "image"
    result: ClassVar[Key] = "result"
    viewer: ClassVar[Key] = "viewer"
    blend: ClassVar[Key] = "blend"
    frame: ClassVar[Key] = "frame"

    @staticmethod
    def socket(*parts: Key | int | type[Case]) -> End:
        """Returns group input end at parameter path."""
        return End(node=INPUTS, socket=item_key(*parts))

    @staticmethod
    def layer(socket: str) -> End:
        """Returns output end of row's Render Layers node."""
        return End(node="render", socket=socket)

    def passed(self) -> BlenderResult[tuple[Fragment, End]]:
        """Returns source with no nodes, graph of effects rendered during preparation."""
        return Ok((Fragment(), self.source))

    def case[C: Case](self, kind: type[C], *parts: Key | int | type[Case]) -> Option[C]:
        """Returns active or retained case of kind at path."""
        return Option.of_optional(next((value for value in self.cases[parts] if isinstance(value, kind)), None))


class Plot(msgspec.Struct, frozen=True, kw_only=True):
    """Record curve placed in its editor's native axes, the shown height `scale * y + slope * x + offset`."""

    scale: float = 1.0
    slope: float = 0.0
    offset: float = 0.0

    def shown(self, x: float, y: float) -> float:
        """Returns editor height of record point."""
        return self.scale * y + self.slope * x + self.offset

    def held(self, x: float, y: float) -> float:
        """Returns record height of editor point."""
        return (y - self.slope * x - self.offset) / self.scale


class CurveEditor(msgspec.Struct, frozen=True, kw_only=True):
    """Native curve node holding knot fields of a record as its curves in field order, drawn as one editor with channel tabs.

    `plots` place each field's curve in the node's own axes, and `view` is the node's own view in place of the record bounds.
    """

    key: Key
    node: type[bpy.types.Node]
    fields: tuple[str, ...]
    plots: tuple[Plot, ...] = ()
    view: Option[tuple[float, float, float, float]] = Nothing

    def plot(self, field: object) -> Plot:
        """Returns placement of field's curve, identity when the editor declares none."""
        return self.plots[self.fields.index(field)] if self.plots else Plot()


class GraphWidget(msgspec.Struct, frozen=True, kw_only=True):
    """Owned node editing one active graphed path under its heading, a shared editor standing once for its fields."""

    node: Key
    heading: str
    shape: native.Type
    field: msgspec.structs.FieldInfo
    path: KeyPath
    value: msgspec.Struct


class Pinned(msgspec.Struct, frozen=True, kw_only=True):
    """Unranked owned group with entry title, stack end, enables, and conditions."""

    ownership: Ownership[bpy.types.CompositorNodeTree]
    title: str
    edge: Edge
    enables: frozenset[Enabled] = frozenset(Enabled)
    conditions: Option[Callable[[Identity, bpy.types.SceneCompositorEffect], BlenderResult[dict[Key, Condition]]]] = Nothing


class StackEntry(msgspec.Struct, Generic[_E_co], frozen=True, kw_only=True):
    """Compositor entry and its offered row when owned."""

    entry: bpy.types.SceneCompositorEffect
    row: Option[Placeable[_E_co]]


class Configured(msgspec.Struct, Generic[_E_co], frozen=True, kw_only=True):
    """Effect record with render and preview enables."""

    record: Entry[_E_co]
    render: Annotated[bool, Enabled.RENDER]
    preview: Annotated[bool, Enabled.PREVIEW]


class EffectOperators(msgspec.Struct, frozen=True, kw_only=True):
    """Stack edit commands menus and shortcuts call."""

    moving: Operator[Shift]
    removing: Operator[Named]
    resetting: Operator[Named]
    adding: tuple[Operator[Empty], ...]


class CompositorEffect[R: Effect, D: msgspec.Struct = msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Effect row with record, graph builder, and requirements."""

    record: type[R]
    graph: Built[R]
    straight: bool = False
    seed: Option[Callable[[bpy.types.Scene], R]] = Nothing
    initial_view: Option[Callable[[PyOpenColorIO.Config, bpy.types.Scene], BlenderResult[DisplayView]]] = Nothing
    coverage: Coverage = Coverage.INPUT
    mix_base: Option[End] = Nothing
    enables: frozenset[Enabled] = frozenset(Enabled)
    requested: Option[Requested[R]] = Nothing
    derived: Option[Derived[R, D] | Callable[[Identity, bpy.types.Scene, R], BlenderResult[Mapping[Key, Value]]]] = Nothing
    animated: Option[Animated[R]] = Nothing
    condition: Option[Callable[[R], Condition]] = Nothing
    inputs: tuple[Socket, ...] = ()
    holds: Mapping[str, Value] = msgspec.field(default_factory=dict)
    cameras: Mapping[str, Value] = msgspec.field(default_factory=dict)
    registrables: tuple[Registrable, ...] = ()
    validated: Option[Callable[[R], ImagingResult[R]]] = Nothing
    native_validated: Option[Callable[[Identity, bpy.types.Scene, Option[bpy.types.SceneCompositorEffect], R, Context], BlenderResult[None]]] = Nothing
    preparation: Option[Preparing[R]] = Nothing
    opened: Option[Opened[R]] = Nothing
    editors: tuple[CurveEditor, ...] = ()

    # --- [ROW_VALUES]

    @property
    def kind(self) -> Key:
        """Record key naming ownerships and commands."""
        return item_key(self.record)

    @property
    def requests(self) -> bool:
        """Whether row makes render pass request."""
        return self.requested.is_some()

    @property
    def staged(self) -> bool:
        """Whether row prepares entering image before group build."""
        return self.preparation.is_some()

    def prepared(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[Effect], config: PyOpenColorIO.Config, image: OpenImageIO.ImageBuf, precision: Precision, entry: bpy.types.SceneCompositorEffect
    ) -> BlenderResult[tuple[Entry[R], Mapping[Key, OpenImageIO.ImageBuf | Estimate[OpenImageIO.ImageBuf]]]]:
        """Prepares effective record and named images, keeping authored blend."""
        return self._narrowed(record).bind(
            lambda typed: self.preparation.map(
                lambda prepare: (
                    inputs(identity, entry, (Shell.frame,), CameraFrame)
                    .bind(lambda frame: prepare(identity, scene, typed.effect, config, image, precision, frame, entry))
                    .map(lambda value: (msgspec.structs.replace(typed, effect=value[0]), value[1]))
                )
            ).default_value(Ok((typed, {})))
        )

    @property
    def reads(self) -> "tuple[tuple[type[bpy.types.bpy_struct[object]], bpy.types.Property], ...]":
        """RNA members derived record reads."""
        return self.derived.map(lambda derived: derived.reads if isinstance(derived, Derived) else ()).default_value(())

    def seeded(self, scene: bpy.types.Scene) -> BlenderResult[R]:
        """Returns scene seed, record defaults, or absent file fault for opened rows."""
        return self.seed.map(lambda seed: Ok(seed(scene))).default_with(lambda: Error((BlenderAbsentFault(owner=self.record.title, key="file"),)) if self.opened.is_some() else Ok(self.record()))

    def _narrowed(self, record: Entry[Effect]) -> BlenderResult[Entry[R]]:
        return (
            Ok(Entry(effect=record.effect, blend=record.blend))
            if isinstance(record.effect, self.record)
            else Error((BlenderChoiceFault(path=self.kind, identifier=item_key(type(record.effect)), accepted=(self.kind,)),))
        )

    def ownership(self, record: Entry[Effect]) -> Ownership[bpy.types.NodeTree]:
        """Selects per-entry ownership for staged, graphed, collection, or masked records."""
        shared, personal = ownerships(self.record)
        collection = any(isinstance(unwrapped(shape), native.VarTupleType) for _, _, shape in rows(self.record))
        return personal if self.staged or record.blend.masks or graphed(self.record) or collection else shared

    # --- [ENTRY_VALUES]

    def read(self, identity: Identity, entry: bpy.types.SceneCompositorEffect, *, edited: bool = True, graphs: Mapping[Key, object] = {}) -> BlenderResult[Entry[R]]:
        """Reads graph store and group inputs, then validates record."""

        def decoded(graphs: Mapping[Key, object]) -> BlenderResult[Entry[R]]:
            accepted = inputs(identity, entry, (), self.record, graphs).bind(
                lambda value: self.validated.map(lambda accept: accept(value).map_error(BlenderImagingFault.lifted)).default_value(Ok(value))
            )
            return paired(accepted, Ok(Blend()) if self.record is DisplayView else inputs(identity, entry, (Shell.blend,), Blend, graphs)).map(lambda pair: Entry(effect=pair[0], blend=pair[1]))

        records = (
            (((), self.record, Nothing, self.record.stage.domain, self.editors),)
            if self.record is DisplayView
            else (((), self.record, Nothing, self.record.stage.domain, self.editors), ((Shell.blend,), Blend, Nothing, self.record.stage.domain, ()))
        )
        return graph_values(identity, entry, records, edited=edited).bind(lambda values: decoded({**values, **graphs}))

    def current(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[Option[R]]:
        """Reads earliest owned effect, Nothing when absent."""
        return (
            Option.of_optional(next(iter(entries(identity, scene, self.record)), None)).map(lambda entry: self.read(identity, entry).map(lambda record: Some(record.effect))).default_value(Ok(Nothing))
        )

    def named(self, identity: Identity, scene: bpy.types.Scene, name: str) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Finds named entry among row's owned groups."""
        return Option.of_optional(next((entry for entry in entries(identity, scene, self.record) if entry.name == name), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=name),)
        )

    def _met(
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
        reading = source.default_value(scene)
        return paired(
            self.condition.map(lambda condition: check(condition(record), bpy.context, Some(reading))).default_value(Ok(None)),
            self.native_validated.map(lambda accept: accept(identity, reading, entry, record, context)).default_value(Ok(None)) if prepared or not self.staged or record.pointwise else Ok(None),
        ).bind(
            lambda _: self.requested.map(
                lambda request: paired(request(record, scene.view_layers[0].name), Descriptor.resolve(bpy.types.Scene, "render.compositor_precision", bpy.types.EnumProperty)).bind(
                    lambda pair: required(scene, bpy.context, (pair[0],), pair[1]).map(lambda _: Some(pair[0]))
                )
            ).default_value(Ok(Nothing))
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
        images: Mapping[Key, tuple[Fragment, End]] = {},
    ) -> BlenderResult[bool]:
        """Reconciles group, then writes rebuilt inputs, answering whether any changed."""

        def wrote(typed: Entry[R], config: PyOpenColorIO.Config) -> BlenderResult[bool]:
            return (
                self
                ._met(identity, scene, typed.effect, Some(entry), context, source, prepared=prepared)
                .bind(lambda request: self._made(identity, scene, typed, config, context, request, Option.of_optional(entry.node_group), images=images))
                .bind(
                    lambda built: written(entry, "node_group", built[0]).bind(
                        lambda pointed: self._filled(identity, scene, entry, built[0], typed, source).map(lambda filled: built[1] or pointed.is_some() or filled)
                    )
                )
            )

        return editable(scene).bind(lambda _: self._narrowed(record)).bind(lambda typed: configured(typed.effect).bind(lambda config: wrote(typed, config)))

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
        images: Mapping[Key, tuple[Fragment, End]] = {},
    ) -> BlenderResult[NodeGroup]:
        """Builds entry group from graph builder and blend."""
        return self._narrowed(record).bind(
            lambda typed: self._met(identity, scene, typed.effect, entry, context, prepared=prepared).bind(
                lambda request: self._grouped(identity, scene, typed, config, entry.bind(lambda value: Option.of_optional(value.node_group)), context, request, images=images)
            )
        )

    def _grouped(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[R],
        config: PyOpenColorIO.Config,
        tree: Option[bpy.types.NodeTree],
        context: Context,
        request: Option[PassRequest],
        *,
        images: Mapping[Key, tuple[Fragment, End]] = {},
    ) -> BlenderResult[NodeGroup]:
        original = Shell.socket(Shell.image)
        frame = {field: Shell.socket(Shell.frame, field) for field in CameraFrame.__struct_fields__}
        space = Some(record.effect.space).filter(lambda name: name != PyOpenColorIO.ROLE_SCENE_LINEAR) if isinstance(record.effect, Spaced) else Nothing

        def built(cases: Mapping[KeyPath, tuple[Case, ...]]) -> BlenderResult[tuple[Fragment, End]]:
            def operation(entering: End | tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
                def body(source: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
                    fragment, end = source
                    shell = Shell(
                        record=record.effect,
                        identity=identity,
                        config=config,
                        source=end,
                        aspect=frame["aspect"],
                        scale=frame["scale"],
                        meters=frame["meters"],
                        items=menus(self.record, value=Some(record.effect)),
                        cases=cases,
                        images={key: value[1] for key, value in images.items()},
                        context=context,
                    )
                    return self.graph(shell).bind(lambda made: Fragment.merged((fragment, made[0])).map(lambda joined: (joined, made[1])))

                return (
                    space
                    .map(
                        lambda name: (
                            converted("space_in", config, PyOpenColorIO.ROLE_SCENE_LINEAR, name, entering)
                            if self.straight
                            else pointwise("space_in_scope", entering, lambda color: converted("space_in", config, PyOpenColorIO.ROLE_SCENE_LINEAR, name, color))
                        )
                    )
                    .default_with(lambda: Ok(entering if isinstance(entering, tuple) else (Fragment(), entering)))
                    .bind(body)
                    .bind(
                        lambda made: space.map(
                            lambda name: (
                                converted("space_out", config, name, PyOpenColorIO.ROLE_SCENE_LINEAR, made)
                                if self.straight
                                else pointwise("space_out_scope", made, lambda color: converted("space_out", config, name, PyOpenColorIO.ROLE_SCENE_LINEAR, color))
                            )
                        ).default_value(Ok(made))
                    )
                )

            body = (
                Fragment
                .merged(value[0] for value in images.values())
                .map(lambda joined: (joined, images.get(Shell.image, (Fragment(), original))[1]))
                .bind(lambda held: pointwise("color", held, operation) if self.straight else operation(held))
            )
            render = (
                request
                .filter(lambda request: bool(request.flags))
                .map(lambda request: node("render", bpy.types.CompositorNodeRLayers, {}, {"scene": IdName.of(scene), "layer": request.layer}))
                .default_value(Ok(Fragment()))
            )
            return paired(body, render).bind(lambda parts: Fragment.merged((parts[0][0], parts[1])).map(lambda joined: (joined, parts[0][1])))

        def assembled(result: Option[tuple[Fragment, End]], interface: Interface) -> BlenderResult[NodeGroup]:
            ownership = self.ownership(record)
            requested = tree.map(lambda original: identity.name(Name.ATTACHMENT, Shell.viewer).bind(lambda name: Stored(name=name, decoder=msgspec.msgpack.Decoder(End)).read(original))).default_value(
                Ok(Nothing)
            )
            return (
                paired(NodeGroup.built(ownership.key, self.record.title, interface, dict(result.map(lambda value: (Shell.result, value)).to_list())), requested)
                .map(
                    lambda pair: (
                        msgspec.structs.replace(
                            pair[0],
                            graph=msgspec.structs.replace(
                                pair[0].graph,
                                nodes=pair[0].graph.nodes | {Shell.viewer: Node(kind=bpy.types.CompositorNodeViewer)},
                                links=(*pair[0].graph.links, Link(source=pair[1].value, target=End(node=Shell.viewer, socket="Image"))),
                            ),
                        )
                        if images and pair[1].is_some()
                        else pair[0]
                    )
                )
                .bind(
                    lambda group: Descriptor.resolve(bpy.types.CompositorNodeTree, "description", bpy.types.StringProperty).map(
                        lambda descriptor: msgspec.structs.replace(group, single=ownership.single, graph=msgspec.structs.replace(group.graph, members={descriptor: inspect.getdoc(self.record) or ""}))
                    )
                )
            )

        def finished(body: tuple[Fragment, End], interface: Interface) -> BlenderResult[NodeGroup]:
            if self.record is DisplayView:
                return assembled(Some(body), interface)
            split, alpha = item_key(Shell.result, "alpha_split"), item_key(Shell.result, "alpha")
            restored = (
                node(split, bpy.types.CompositorNodeSeparateColor, {"Image": original}, {"mode": "RGB"}).bind(
                    lambda separated: node(alpha, bpy.types.CompositorNodeSetAlpha, {"Image": body, "Alpha": ended(split, "Alpha")(separated), "Type": "Replace Alpha"}, {}).map(ended(alpha, "Image"))
                )
                if self.coverage is Coverage.INPUT and not self.straight
                else Ok(body)
            )
            base: BlenderResult[Operand] = Ok(self.mix_base.map(lambda base: (body[0], base)).default_value(original))
            if self.mix_base.is_some():
                base = base.bind(
                    lambda operand: space.map(
                        lambda name: pointwise("space_base_scope", operand, lambda color: converted("space_base_out", config, name, PyOpenColorIO.ROLE_SCENE_LINEAR, color))
                    ).default_value(Ok(operand))
                )
            weighted = masked("blend_masks", len(record.blend.masks), original, frame["aspect"], frame["scale"], lambda key: Shell.socket(Shell.blend, "masks", key)).bind(
                lambda mask: product("blend_coverage", (Shell.socket(Shell.blend, "opacity"), *mask.to_list()))
            )
            return (
                paired(paired(base, restored), weighted)
                .bind(lambda parts: mixed_back(Shell.blend, *parts[0], Shell.socket(Shell.blend, "mode"), parts[1]))
                .bind(lambda value: assembled(Some(value), interface))
            )

        interface = (
            entry_interface(self.record, len(record.blend.masks)).bind(lambda face: expanded(face, record.effect)).map(lambda face: msgspec.structs.replace(face, inputs=(*face.inputs, *self.inputs)))
        )
        derived = self.derived.map(lambda derived: Interface.of(derived.record).map(hidden) if isinstance(derived, Derived) else Ok(Interface())).default_value(Ok(Interface()))
        if self.staged and not images and not record.effect.pointwise:
            return (
                paired(interface, derived)
                .map(lambda pair: msgspec.structs.replace(pair[0], inputs=(*pair[0].inputs, *pair[1].inputs), panels=(*pair[0].panels, *pair[1].panels)))
                .bind(lambda face: assembled(Nothing, face))
            )
        return paired(paired(interface, derived), case_values(record.effect, tree, identity)).bind(
            lambda parts: built(parts[1]).bind(
                lambda body: finished(body, msgspec.structs.replace(parts[0][0], inputs=(*parts[0][0].inputs, *parts[0][1].inputs), panels=(*parts[0][0].panels, *parts[0][1].panels)))
            )
        )

    def _made(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        record: Entry[R],
        config: PyOpenColorIO.Config,
        context: Context,
        request: Option[PassRequest],
        tree: Option[bpy.types.NodeTree] = Nothing,
        *,
        images: Mapping[Key, tuple[Fragment, End]] = {},
    ) -> BlenderResult[tuple[bpy.types.NodeTree, bool]]:
        own = self.ownership(record)
        owner = Some(scene) if not own.single or request.filter(lambda request: bool(request.flags)).is_some() else Nothing
        held = tree.filter(lambda group: Mark.read(identity, group).filter(lambda mark: mark.key == own.key and mark.of == owner).is_some()).or_else_with(
            lambda: Option.of_optional(next((group for group, mark in own.held(identity) if mark.of == owner), None)) if own.single else Nothing
        )

        def refreshed(group: NodeGroup, tree: bpy.types.NodeTree, keys: Mapping[Key, str]) -> BlenderResult[tuple[bpy.types.NodeTree, bool]]:
            marked = {live: mark.key for live in tree.nodes for mark in Mark.read(identity, live).to_list()}
            viewers = tuple(live for live, key in marked.items() if key == Shell.viewer)
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
            return reconciled(msgspec.structs.replace(group, graph=graph), tree, identity).map(lambda changes: (tree, changes != Changes()))

        return self._grouped(identity, scene, record, config, held, context, request, images=images).bind(
            lambda group: held.map(lambda tree: slot(identity).bind(lambda name: refreshed(group, tree, addresses(tree, name)))).default_with(
                lambda: made(group, identity, of=owner).map(lambda tree: (tree, True))
            )
        )

    # --- [PLACEMENT_AND_REFRESH]

    def appended(
        self, identity: Identity, scene: bpy.types.Scene, record: Entry[Effect], *, context: Context = Nothing, name: Option[EffectName] = Nothing
    ) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Creates entry once requirements and group succeed."""
        label = vacant(name.default_value(self.record.title), (entry.name for entry in scene.compositor_effects))

        def created(typed: Entry[R], config: PyOpenColorIO.Config) -> BlenderResult[bpy.types.SceneCompositorEffect]:
            def appended(group: bpy.types.NodeTree) -> BlenderResult[bpy.types.SceneCompositorEffect]:
                with ExitStack() as rollback:
                    entry = scene.compositor_effects.new(label)
                    rollback.callback(scene.compositor_effects.remove, entry)
                    return (
                        accumulated((written(entry, "node_group", group), *(enabled.descriptor.write(entry, enabled in self.enables) for enabled in Enabled)))
                        .bind(lambda _: self._filled(identity, scene, entry, group, typed))
                        .map(lambda _: rollback.pop_all())
                        .map(lambda _: entry)
                    )

            return self._met(identity, scene, typed.effect, Nothing, context).bind(lambda request: self._made(identity, scene, typed, config, context, request)).bind(lambda built: appended(built[0]))

        return editable(scene).bind(lambda _: self._narrowed(record)).bind(lambda typed: configured(typed.effect).bind(lambda config: created(typed, config)))

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
                ).bind(lambda _: self._refreshed(identity, scene, entry, record.effect))
            )
        )

    def _filled(
        self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, group: bpy.types.NodeTree, typed: Entry[R], source: Option[bpy.types.Scene] = Nothing
    ) -> BlenderResult[bool]:
        parts: tuple[tuple[KeyPath, msgspec.Struct], ...] = (((), typed.effect),) if self.record is DisplayView else (((), typed.effect), ((Shell.blend,), typed.blend))
        return accumulated((
            stored_graphs(identity, group, tuple((prefix, value, self.record.stage.domain) for prefix, value in parts)),
            *(entered(identity, entry, prefix, value) for prefix, value in parts),
        )).bind(lambda outcomes: self._refreshed(identity, scene, entry, typed.effect, source).map(lambda refreshed: any(outcomes) or refreshed))

    def _refreshed(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, record: R, source: Option[bpy.types.Scene] = Nothing) -> BlenderResult[bool]:
        reading = source.default_value(scene)
        frame = FrameSize().bound(identity).bind(lambda framed: camera_frame(framed, reading)).bind(lambda value: entered(identity, entry, (Shell.frame,), value))
        values = self.derived.map(
            lambda derived: (
                derived.of.map(lambda build: build(identity, reading, record).bind(lambda value: entered(identity, entry, (), value))).default_value(Ok(False))
                if isinstance(derived, Derived)
                else derived(identity, reading, record).bind(
                    lambda values: accumulated(entry_input(identity, entry, key).bind(lambda owner: written(owner, "value", value)).map(Option.is_some) for key, value in values.items()).map(any)
                )
            )
        ).default_value(Ok(False))
        channels = self.animated.map(
            lambda build: build(identity, scene, entry, record).bind(lambda channels: accumulated(applied_channel(channel, scene) for channel in channels).map(any))
        ).default_value(Ok(False))
        return accumulated((frame, values, channels)).map(any)

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns ownerships and registrables."""
        return Ok((*ownerships(self.record), *self.registrables))


class EffectStack[E: Effect = Effect]:
    """Offered effect rows, pinned groups, and placement."""

    effects: tuple[Placeable[E], ...]
    pinned: tuple[Pinned, ...]
    gain: Option[GainRow]
    operators: EffectOperators
    precision: Callable[[Identity], BlenderResult[Precision]]
    executing: set[int]

    # --- [OFFERED_ROWS]

    @property
    def rows(self) -> tuple[Placeable[E | DisplayView], ...]:
        """Formation row followed by offered rows."""
        return (formation, *self.effects)

    @property
    def offered(self) -> TypeForm[E]:
        """Union of offered row record types."""
        return reduce(or_, (row.record for row in self.effects))

    @property
    def form(self) -> TypeForm[E | DisplayView]:
        """Union of row record types."""
        return reduce(or_, (row.record for row in self.rows))

    @property
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Frame size attachment camera frame derivation binds."""
        return (FrameSize.attachment,)

    @property
    def standalone(self) -> tuple[Placeable[E], ...]:
        """Rows needing neither render pass request nor file."""
        return tuple(row for row in self.effects if not row.requests and row.opened.is_none())

    def config(self) -> BlenderResult[PyOpenColorIO.Config]:
        """Returns live OCIO config resolving row default process spaces."""
        name = Spaced.space.__name__
        spaces = tuple(
            dict.fromkeys(
                value
                for row in self.rows
                if isinstance(row.record, Spaced)
                for value in ({field.name: field.default for field in msgspec.structs.fields(row.record)}.get(name, inspect.getattr_static(row.record, name)),)
                if isinstance(value, str)
            )
        )
        return config_of(spaces)

    def offering(self, kind: type[Effect]) -> BlenderResult[Placeable[E | DisplayView]]:
        """Finds row of exact effect type."""
        return Option.of_optional(next((row for row in self.rows if row.record is kind), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=repr(tuple(row.kind for row in self.rows)), key=item_key(kind)),)
        )

    def menu(self, key: Key, title: str, description: str) -> Menu:
        """Builds add menu of every row, one submenu per imaging phase, rows in stage order."""
        return Menu(
            key=key,
            title=title,
            description=description,
            body=Submenus(
                entries=tuple(
                    (row.record.stage.phase, OperatorButton(target=operator, text=Some(row.record.title)))
                    for row, operator in sorted(zip(self.rows, self.operators.adding), key=lambda pair: pair[0].record.stage)
                )
            ),
        )

    def entries(self, identity: Identity, scene: bpy.types.Scene, selection: Stage | type[Effect]) -> tuple[bpy.types.SceneCompositorEffect, ...]:
        """Returns owned entries of stage or exact record type."""
        return tuple(held.entry for held in self.held(identity, scene) for row in held.row.to_list() if row.record.stage is selection or row.record is selection)

    def stages(self, identity: Identity, scene: bpy.types.Scene, enabled: Enabled = Enabled.RENDER) -> tuple[tuple[bpy.types.SceneCompositorEffect, Placeable[E | DisplayView]], ...]:
        """Returns enabled staged entries with their rows in stack order."""
        return tuple((held.entry, row) for held in self.held(identity, scene) if enabled.descriptor.read(held.entry) is True for row in held.row.filter(lambda row: row.staged).to_list())

    def held(self, identity: Identity, scene: bpy.types.Scene) -> tuple[StackEntry[E | DisplayView], ...]:
        """Returns every entry with offered row from its group mark."""
        offered = {owner.key: row for row in self.rows for owner in ownerships(row.record)}
        return tuple(
            StackEntry(entry=entry, row=Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group)).bind(lambda mark: Option.of_optional(offered.get(mark.key))))
            for entry in scene.compositor_effects
        )

    def found(self, identity: Identity, scene: bpy.types.Scene, name: str) -> BlenderResult[StackEntry[E | DisplayView]]:
        """Finds named owned entry, skipping user groups."""
        return Option.of_optional(next((held for held in self.held(identity, scene) if held.entry.name == name and held.row.is_some()), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=repr(scene.compositor_effects), key=name),)
        )

    # --- [RECIPES]

    def captured(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[dict[EffectName, Configured[E | DisplayView]]]:
        """Reads owned entry records and enables by name in stack order."""
        return accumulated(
            row.read(identity, held.entry).map(lambda record: (held.entry.name, Configured(record=record, render=held.entry.enable_for_render, preview=held.entry.enable_for_preview)))
            for held in self.held(identity, scene)
            for row in held.row.to_list()
        ).map(dict)

    def applied(
        self,
        identity: Identity,
        scene: bpy.types.Scene,
        recipe: Mapping[EffectName, Configured[Effect]],
        *,
        locked: frozenset[EffectName] = frozenset(),
        fields: Mapping[EffectName, frozenset[tuple[Key | int, ...]]] = {},
        initial_view: Option[DisplayView] = Nothing,
        context: Context = Nothing,
    ) -> BlenderResult[None]:
        """Applies sparse named records, keeping locked, unlisted, and user entries."""

        def applied(current: Mapping[EffectName, Configured[Effect]]) -> BlenderResult[None]:
            desired = projected(current, recipe, locked=locked, fields=fields)
            if not (requested := ({name: desired[name] for name in recipe if name not in locked})) and initial_view.is_none():
                return Ok(None)

            def checked(rows: tuple[Placeable, ...]) -> BlenderResult[None]:
                def write(initial: Option[DisplayView]) -> BlenderResult[None]:
                    native = {entry.name: entry for entry in scene.compositor_effects}
                    order = tuple(native.values())
                    prior = next((entry for entry in order if entry.is_active), None)

                    def written_entry(name: EffectName, value: Configured[Effect], row: Placeable) -> BlenderResult[tuple[bpy.types.SceneCompositorEffect, Placeable]]:
                        opening = self._folded(identity, scene) if row.record is DisplayView and not any(isinstance(value.record.effect, DisplayView) for value in current.values()) else Ok(None)
                        return opening.bind(
                            lambda _: (
                                Option
                                .of_optional(native.get(name))
                                .map(lambda entry: row.write(identity, scene, entry, value.record, context=context).map(lambda _: entry))
                                .default_with(lambda: row.appended(identity, scene, value.record, context=context, name=Some(name)))
                            )
                        ).bind(lambda entry: paired(written(entry, "enable_for_render", value.render), written(entry, "enable_for_preview", value.preview)).map(lambda _: (entry, row)))

                    def arranged(written_entries: tuple[tuple[bpy.types.SceneCompositorEffect, Placeable], ...]) -> BlenderResult[None]:
                        live = dict(zip(requested, (entry for entry, _ in written_entries)))
                        selected = iter(native.get(name, live[name]) if name in live else native[name] for name in desired)
                        target = tuple(next(selected) if entry.name in current else entry for entry in order) + tuple(selected)
                        owned = frozenset(native[name] for name in current) | frozenset(live.values())
                        retired = tuple(
                            dict.fromkeys(type(value.record.effect) for value in current.values() if not any(type(record.record.effect) is type(value.record.effect) for record in desired.values()))
                        )
                        return (
                            accumulated(
                                self
                                .offering(kind)
                                .bind(lambda row: self.release(identity, scene, row))
                                .bind(
                                    lambda _: (
                                        displayed(scene, next(value.record.effect for value in current.values() if isinstance(value.record.effect, DisplayView))) if kind is DisplayView else Ok(None)
                                    )
                                )
                                for kind in retired
                            )
                            .bind(lambda _: accumulated(hold(identity, scene, row) for _, row in written_entries))
                            .bind(lambda _: accumulated(relocated(scene, entry, index) for index, entry in enumerate(target) if entry in owned))
                            .bind(lambda _: initial.map(lambda view: self.placed(identity, scene, formation, Entry(effect=view), context=context).map(lambda _: None)).default_value(Ok(None)))
                            .map(lambda _: None)
                        )

                    with ExitStack() as scope, bpy.context.temp_override(scene=scene):
                        if prior is not None:
                            scope.callback(setattr, prior, "is_active", True)
                        return accumulated(written_entry(name, value, row) for (name, value), row in zip(requested.items(), rows)).bind(arranged)

                initial = next(
                    (
                        seed
                        for row in rows
                        for seed in row.initial_view.or_else_with(lambda: Some(lambda _config, scene: formation.seeded(scene)) if row.record.stage.domain is Domain.DISPLAY else Nothing).to_list()
                    ),
                    None,
                )
                opening = (
                    initial_view.map(lambda view: Ok(Some(view))).default_with(lambda: self.config().bind(lambda config: initial(config, scene)).map(Some) if initial is not None else Ok(Nothing))
                    if not entries(identity, scene, DisplayView) and not any(isinstance(value.record.effect, DisplayView) for value in desired.values())
                    else Ok(Nothing)
                )
                return opening.bind(write)

            def named(prop: Descriptor[bpy.types.StringProperty]) -> BlenderResult[None]:
                names = accumulated(
                    Descriptor.validated(prop.prop, name).bind(
                        lambda _: Error((BlenderCollisionFault(request=name, error=repr(scene.compositor_effects[name])),)) if name not in current and name in scene.compositor_effects else Ok(None)
                    )
                    for name in requested
                )
                return paired(names, accumulated(self.offering(type(value.record.effect)) for value in requested.values())).bind(lambda pair: checked(pair[1]))

            return Descriptor.resolve(bpy.types.SceneCompositorEffect, "name", bpy.types.StringProperty).bind(named)

        return editable(scene).bind(lambda _: self.captured(identity, scene)).bind(applied)

    # --- [PLACEMENT]

    def placed(self, identity: Identity, scene: bpy.types.Scene, row: Placeable, record: Entry[Effect], *, context: Context = Nothing) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Places entry at rank, applying first-entry settings, restoring active entry."""
        prior = Option.of_optional(next((entry for entry in scene.compositor_effects if entry.is_active), None))
        existing = Option.of_optional(next(iter(self.entries(identity, scene, row.record.stage)), None)) if row.record.stage.single else Nothing

        def added() -> BlenderResult[bpy.types.SceneCompositorEffect]:
            view = row.initial_view.or_else_with(lambda: Some(lambda _config, scene: formation.seeded(scene)) if row.record.stage.domain is Domain.DISPLAY else Nothing)
            opening = (
                view.map(
                    lambda seed: (
                        self.config().bind(lambda config: seed(config, scene)).bind(lambda view: self.placed(identity, scene, formation, Entry(effect=view), context=context)).map(lambda _: None)
                    )
                ).default_value(Ok(None))
                if not entries(identity, scene, DisplayView)
                else Ok(None)
            )
            return (
                opening
                .bind(lambda _: self._folded(identity, scene) if row.record is DisplayView else Ok(None))
                .bind(lambda _: row.appended(identity, scene, record, context=context))
                .bind(
                    lambda entry: (
                        hold(identity, scene, row)
                        .bind(lambda _: self.moved(identity, scene, Shift(name=entry.name)))
                        .bind(lambda _: prior.map(lambda active: written(active, "is_active", True).map(lambda _: None)).default_value(Ok(None)))
                        .map(lambda _: entry)
                    )
                )
            )

        return editable(scene).bind(lambda _: existing.map(Ok).default_with(added))

    def assigned(self, identity: Identity, scene: bpy.types.Scene, effect: Effect, *, context: Context = Nothing) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Writes earliest entry of effect type or places new one."""

        def assigned(row: Placeable) -> BlenderResult[bpy.types.SceneCompositorEffect]:
            return (
                Option
                .of_optional(next(iter(entries(identity, scene, type(effect))), None))
                .map(lambda entry: row.read(identity, entry).bind(lambda prior: row.write(identity, scene, entry, msgspec.structs.replace(prior, effect=effect), context=context)).map(lambda _: entry))
                .default_with(lambda: self.placed(identity, scene, row, Entry(effect=effect), context=context))
            )

        return self.offering(type(effect)).bind(assigned)

    def moved(self, identity: Identity, scene: bpy.types.Scene, shift: Shift) -> BlenderResult[bool]:
        """Moves named owned entry alone, answering whether it moved."""

        def moved(held: StackEntry) -> BlenderResult[bool]:
            order = tuple(scene.compositor_effects)
            position = order.index(held.entry)
            rank = held.row.map(lambda row: row.record.stage).default_value(Stage.DENOISE)

            def above(entry: bpy.types.SceneCompositorEffect) -> bool:
                mark = Option.of_optional(entry.node_group).bind(lambda group: Mark.read(identity, group))
                return any(mark.filter(lambda mark: mark.key == pinned.ownership.key).is_some() and pinned.edge is Edge.TAIL for pinned in self.pinned) or any(
                    item.entry == entry and item.row.filter(lambda row: row.record.stage > rank).is_some() for item in self.held(identity, scene)
                )

            match shift.toward:
                case Toward.UP:
                    index = max(0, position - 1)
                case Toward.DOWN:
                    index = min(len(order) - 1, position + 1)
                case Toward.RANK:
                    others = tuple(entry for entry in order if entry != held.entry)
                    index = next((index for index, entry in enumerate(others) if above(entry)), len(others))
            return relocated(scene, held.entry, index)

        return editable(scene).bind(lambda _: self.found(identity, scene, shift.name)).bind(moved)

    def removed(self, identity: Identity, scene: bpy.types.Scene, name: str) -> BlenderResult[None]:
        """Removes owned entry, unused groups, and last-entry row holds, writing removed Display View to scene."""

        def removed(held: StackEntry) -> BlenderResult[None]:
            def row_removed(row: Placeable) -> BlenderResult[None]:
                display = formation.read(identity, held.entry).map(lambda record: Some(record.effect)) if row.record is DisplayView else Ok(Nothing)
                return (
                    display
                    .bind(
                        lambda record: (
                            Result[None, BlenderFaults]
                            .bind(
                                catch(lambda: scene.compositor_effects.remove(held.entry), exception=RuntimeError)().map_error(lambda error: (BlenderEditFault(request=repr(scene), error=error),)),
                                lambda _: self.release(identity, scene, row) if not entries(identity, scene, row.record) else Ok(None),
                            )
                            .bind(lambda _: record.map(lambda view: displayed(scene, view)).default_value(Ok(None)))
                        )
                    )
                    .bind(lambda _: accumulated(owner.removed(identity) for owner in ownerships(row.record)))
                    .map(lambda _: None)
                )

            return Result[Placeable, BlenderFaults].bind(held.row.to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key=name),)), row_removed)

        return editable(scene).bind(lambda _: self.found(identity, scene, name)).bind(removed)

    # --- [SCENE_SETTINGS]

    def _folded(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[None]:
        def folded(gain: GainRow) -> BlenderResult[None]:
            exposure = scene.view_settings.exposure  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            placed = Ok(None) if entries(identity, scene, gain.record) else gain.seeded(scene).bind(lambda effect: self.placed(identity, scene, gain, Entry(effect=effect))).map(lambda _: None)
            return (
                placed
                .bind(lambda _: gain.stops(identity, scene))
                .bind(lambda stops: gain.written(identity, scene, stops + exposure))
                .bind(lambda _: written(scene.view_settings, "exposure", 0.0))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                .map(lambda _: None)
                if exposure
                else Ok(None)
            )

        return self.gain.map(folded).default_value(Ok(None))

    def release(self, identity: Identity, scene: bpy.types.Scene, row: Placeable) -> BlenderResult[None]:
        """Restores held scene settings, reapplying camera requirements of remaining rows."""

        def released(pair: tuple[str, dict[Key, Snapshot]]) -> BlenderResult[None]:
            slot, held = pair

            def dropped(_: object) -> None:
                scene[slot] = msgspec.json.encode({key: value for key, value in held.items() if key != row.kind}, order="deterministic").decode()

            return (
                Option
                .of_optional(held.get(row.kind))
                .map(lambda snapshot: snapshot.apply(scene).outcome)
                .default_value(Ok(None))
                .map(dropped)
                .bind(lambda _: cameras.released(identity))
                .bind(
                    lambda _: accumulated(
                        cameras.required(Snapshot.of(bpy.types.Camera, dict(offered.cameras)), target, identity)
                        for target in bpy.data.scenes
                        for offered in self.rows
                        if entries(identity, target, offered.record)
                    ).map(lambda _: None)
                )
            )

        return stored(identity, scene, "held", dict[Key, Snapshot]).bind(released)

    # --- [REFRESH]

    def refreshed(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[bool]:
        """Separates per-entry groups, places missing pinned entries, and releases stale holds, answering whether an entry changed."""

        def separated(entry: bpy.types.SceneCompositorEffect, row: Placeable, owner: Ownership[bpy.types.NodeTree], group: bpy.types.NodeTree) -> BlenderResult[bool]:
            label = vacant(row.record.title, bpy.data.node_groups.keys())
            copied: bpy.types.NodeTree = group.copy()  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return named(copied, label).bind(lambda labelled: owner.owned(identity, labelled, of=Some(scene))).bind(lambda owned: written(entry, "node_group", owned)).map(lambda _: True)

        def placed(pinned: Pinned, group: bpy.types.CompositorNodeTree) -> BlenderResult[bool]:
            with ExitStack() as rollback:
                entry = scene.compositor_effects.new(vacant(pinned.title, (held.name for held in scene.compositor_effects)))
                rollback.callback(scene.compositor_effects.remove, entry)
                return (
                    accumulated((written(entry, "node_group", group), *(enabled.descriptor.write(entry, enabled in pinned.enables) for enabled in Enabled)))
                    .bind(lambda _: relocated(scene, entry, 0 if pinned.edge is Edge.HEAD else len(scene.compositor_effects) - 1))
                    .map(lambda _: rollback.pop_all())
                    .map(lambda _: True)
                )

        def refreshed(_: bpy.types.Scene) -> BlenderResult[bool]:
            personal = tuple(
                (held.entry, row, owner, group)
                for held in self.held(identity, scene)
                for row in held.row.to_list()
                for owner in (ownerships(row.record)[1],)
                for group in Option.of_optional(held.entry.node_group).to_list()
                if Mark.read(identity, group).filter(lambda mark: mark.key == owner.key).is_some()
            )
            keeper = {group.session_uid: entry for entry, _, _, group in reversed(personal)}
            absent = (
                (pinned, group)
                for pinned in self.pinned
                for group in Option.of_optional(next((group for group, mark in pinned.ownership.held(identity) if mark.of == Some(scene)), None)).to_list()
                if not any(entry.node_group == group for entry in scene.compositor_effects)
            )
            return (
                reduce(
                    lambda done, shared: done.bind(lambda _: separated(*shared)), ((entry, row, owner, group) for entry, row, owner, group in personal if keeper[group.session_uid] != entry), Ok(False)
                )
                .bind(lambda changed: reduce(lambda done, missing: done.bind(lambda _: placed(*missing)), absent, Ok(changed)))
                .bind(
                    lambda changed: (
                        stored(identity, scene, "held", dict[Key, Snapshot])
                        .bind(lambda pair: accumulated(self.release(identity, scene, row) for row in self.rows if row.kind in pair[1] and not entries(identity, scene, row.record)))
                        .map(lambda _: changed)
                    )
                )
            )

        return editable(scene).bind(refreshed)

    def pointwise(self, identity: Identity, scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[bpy.types.SceneCompositorEffect]:
        """Returns owned entry when its effect is pointwise."""
        return (
            self
            .found(identity, scene, entry.name)
            .bind(lambda found: found.row.to_result_with(lambda: (BlenderAbsentFault(owner=repr(found.entry), key=entry.name),)))
            .bind(lambda row: row.read(identity, entry))
            .bind(lambda record: local_entry(entry, record))
        )

    def forming(self, identity: Identity, scene: bpy.types.Scene, config: PyOpenColorIO.Config, enabled: Enabled) -> BlenderResult[Option[PyOpenColorIO.Transform]]:
        """Groups enabled entry transforms, closed by scene view absent Display View, Nothing when an entry has none."""

        def transformed(entry: bpy.types.SceneCompositorEffect, record: Entry[Effect]) -> BlenderResult[Option[PyOpenColorIO.Transform]]:
            blend: BlenderResult[None] = Ok(None) if record.blend == Blend() else Error((BlenderChoiceFault(path=entry.name, identifier=repr(record.blend), accepted=(repr(Blend()),)),))
            return paired(local_entry(entry, record), blend).bind(
                lambda _: (bracketed(record.effect, config) if isinstance(record.effect, Spaced) else record.effect.transform(config)).map_error(BlenderImagingFault.lifted)
            )

        operations = accumulated(
            held.row.map(lambda row: row.read(identity, held.entry).bind(lambda record: transformed(held.entry, record))).default_value(
                Error((BlenderAbsentFault(owner=repr(scene.compositor_effects), key=held.entry.name),))
            )
            for held in self.held(identity, scene)
            if enabled.descriptor.read(held.entry) is True
        )
        closing = viewed(scene).transform(config).map(lambda operation: (operation,)).map_error(BlenderImagingFault.lifted) if not entries(identity, scene, DisplayView) else Ok(())
        return (
            paired(operations, closing)
            .map(lambda pair: (*pair[0], *pair[1]))
            .map(
                lambda operations: (
                    Some(PyOpenColorIO.GroupTransform([operation for optional in operations for operation in optional.to_list()])) if all(operation.is_some() for operation in operations) else Nothing
                )
            )
        )

    # --- [COMMANDS_AND_LIFETIME]

    def __init__(
        self,
        *,
        effects: tuple[Placeable[E], ...],
        precision: Callable[[Identity], BlenderResult[Precision]] = lambda _identity: Ok(Precision()),
        pinned: tuple[Pinned, ...] = (),
        gain: Option[GainRow] = Nothing,
    ) -> None:
        """Declares stack edit commands over offered rows."""
        self.effects, self.precision, self.pinned, self.gain = effects, precision, pinned, gain
        self.executing: set[int] = set()

        def reset(call: Call[Named]) -> BlenderResult[Outcome]:
            def reset(scene: bpy.types.Scene) -> BlenderResult[bool]:
                return self.found(call.identity, scene, call.record.name).bind(
                    lambda held: Result[Placeable, BlenderFaults].bind(
                        held.row.to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key=call.record.name),)),
                        lambda row: row.seeded(scene).bind(lambda effect: row.write(call.identity, scene, held.entry, Entry(effect=effect))),
                    )
                )

            return call.root(bpy.types.Scene).bind(reset).map(lambda changed: Done() if changed else Unchanged(reason=f"{call.record.name} holds defaults"))

        def added(row: Placeable) -> Operator[Empty]:
            def run(call: Call[Empty]) -> BlenderResult[Done]:
                return (
                    call
                    .root(bpy.types.Scene)
                    .bind(
                        lambda scene: accumulated(
                            record.bind(lambda effect: self.placed(call.identity, scene, row, Entry(effect=effect)))
                            for record in row.opened.map(lambda opened: tuple(Ok(opened.of(str(path))) for path in call.paths)).default_with(lambda: (row.seeded(scene),))
                        )
                    )
                    .map(lambda _: Done())
                )

            return Operator(
                key=f"effect_add_{row.kind}",
                title=f"Add {row.record.title}",
                description=Option.of_optional(inspect.getdoc(row.record)).default_value(""),
                form=Empty,
                run=run,
                invoke=row.opened.map(lambda opened: Open(extensions=lambda _record: opened.extensions())),
                undo=True,
            )

        self.operators = EffectOperators(
            moving=Operator(
                key="effect_move",
                title="Move Effect",
                description="Move effect up, down, or to processing order",
                form=Shift,
                run=lambda call: (
                    call
                    .root(bpy.types.Scene)
                    .bind(lambda scene: self.moved(call.identity, scene, call.record))
                    .map(lambda changed: Done() if changed else Unchanged(reason=f"{call.record.name} holds requested position"))
                ),
                undo=True,
            ),
            removing=Operator(
                key="effect_remove",
                title="Remove Effect",
                description="Remove compositor effect",
                form=Named,
                run=lambda call: call.root(bpy.types.Scene).bind(lambda scene: self.removed(call.identity, scene, call.record.name)).map(lambda _: Done()),
                undo=True,
            ),
            resetting=Operator(key="effect_reset", title="Reset Effect", description="Reset effect to defaults", form=Named, run=reset, undo=True),
            adding=tuple(map(added, self.rows)),
        )

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Yields stack registrables, handlers, and subscriptions with deferred refresh."""

        def refreshed(handle: Handle[bpy.types.Scene], groups: tuple[int, ...]) -> BlenderResult[Option[str]]:
            def update(scene: bpy.types.Scene) -> BlenderResult[Option[str]]:
                if scene.session_uid in self.executing:
                    return Ok(Nothing)

                def reconciled(row: Placeable, entry: bpy.types.SceneCompositorEffect) -> BlenderResult[bool]:
                    match row.read(identity, entry):
                        case Result(tag="ok", ok=record):
                            return row.write(identity, scene, entry, record)
                        case Result(error=faults):
                            restored = row.read(identity, entry, edited=False).bind(lambda stored: row.write(identity, scene, entry, stored))
                            return Error((*faults, *restored.swap().default_value(())))

                return self.refreshed(identity, scene).bind(
                    lambda changed: accumulated(
                        reconciled(row, held.entry)
                        if row.requests
                        or any(GraphForm.IMAGE in markers(field.type, GraphForm) for _, field, _ in rows(row.record))
                        or any(group.session_uid in groups for group in Option.of_optional(held.entry.node_group).to_list())
                        else row.refreshed(identity, scene, held.entry)
                        for held in self.held(identity, scene)
                        for row in held.row.to_list()
                    ).map(lambda outcomes: Some("Refresh Effects") if changed or any(outcomes) else Nothing)
                )

            return handle.resolve().bind(update)

        with identity.reports.deferred(refreshed) as defer:

            def loaded(_path: object) -> BlenderResult[None]:
                return accumulated(
                    self.refreshed(identity, scene).bind(lambda _: rendered(scene)).bind(lambda _: accumulated(hold(identity, scene, row) for row in self.rows if entries(identity, scene, row.record)))
                    for source in bpy.data.scenes
                    for scene in editable(source).to_option().to_list()
                ).map(lambda _: None)

            def updated(scene: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> BlenderResult[None]:
                for editable_scene in editable(scene).to_option().to_list():
                    defer(Handle.of(editable_scene), tuple(update.id.session_uid for update in updates if isinstance(update.id, bpy.types.CompositorNodeTree)))
                return Ok(None)

            def rendered(scene: bpy.types.Scene, source: Option[bpy.types.Scene] = Nothing) -> BlenderResult[None]:
                if scene.session_uid in self.executing:
                    return Ok(None)
                return accumulated(
                    source
                    .map(lambda evaluated: row.named(identity, evaluated, entry.name))
                    .default_value(Ok(entry))
                    .bind(lambda reading: row.read(identity, reading))
                    .bind(lambda record: row.write(identity, editable_scene, entry, record, source=source))
                    if source.is_some() or row.requests or any(GraphForm.IMAGE in markers(field.type, GraphForm) for _, field, _ in rows(row.record))
                    else row.refreshed(identity, editable_scene, entry)
                    for editable_scene in editable(scene).to_option().to_list()
                    for row in self.rows
                    for entry in entries(identity, editable_scene, row.record)
                ).map(lambda _: None)

            def built(frame: Derived[Effect, CameraFrame]) -> Acquisitions:
                def invalidated(_scene: bpy.types.Scene) -> BlenderResult[None]:
                    for image in bpy.data.images:
                        if image.type in {"RENDER_RESULT", "COMPOSITING"}:
                            produced(image, Nothing)
                    return Ok(None)

                operators = self.operators
                reads = tuple(dict.fromkeys((*frame.reads, *(read for row in self.rows for read in row.reads))))
                loading = PathHandler(handlers=bpy.app.handlers.load_post, body=loaded)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                changing = FrameHandler(handlers=bpy.app.handlers.frame_change_post, body=lambda scene, graph: rendered(scene, Some(graph.scene_eval)))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
                return (
                    productions,
                    *self.rows,
                    operators.moving,
                    operators.removing,
                    operators.resetting,
                    *operators.adding,
                    *(
                        Interchange(key=row.kind, title=row.record.title, rows=Importing(operator=operator, record=Empty(), drop=frozenset({"NODE_EDITOR", "IMAGE_EDITOR"})))
                        for row, operator in zip(self.rows, operators.adding)
                        if row.opened.is_some()
                    ),
                    loading,
                    SceneHandler(handlers=bpy.app.handlers.render_init, body=invalidated),
                    UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Scene, bpy.types.Action, bpy.types.CompositorNodeTree), body=updated),
                    changing,
                    *(Subscription(owner=owner, setting=setting, body=lambda: accumulated(rendered(scene) for scene in bpy.data.scenes).map(lambda _: None)) for owner, setting in reads),
                    Subscription(owner=bpy.types.ColorManagedDisplaySettings, setting=bpy.types.ColorManagedDisplaySettings.bl_rna.properties["display_device"], body=lambda: followed(identity)),
                )

            yield framing(identity).map(built)


# --- [OPERATIONS] -----------------------------------------------------------------------


def curve_editors(identity: Identity) -> BlenderResult[dict[type[bpy.types.Node], RowDraw]]:
    """Realizes the native curve editor per curve node kind effect graphs own, channel tabs by kind."""
    kinds: tuple[tuple[type[bpy.types.Node], Literal["COLOR", "HUE", "NONE"]], ...] = (
        (bpy.types.CompositorNodeCurveRGB, "COLOR"),
        (bpy.types.CompositorNodeHueCorrect, "HUE"),
        (bpy.types.ShaderNodeFloatCurve, "NONE"),
    )
    return accumulated(realized(Curve(mapping=Member(root=kind, path="mapping"), type=label), identity).map(lambda draw: (kind, draw)) for kind, label in kinds).map(dict)


def titled(field: msgspec.structs.FieldInfo) -> str:
    """Returns field's declared title, else its display name."""
    return next((meta.title for meta in markers(field.type, msgspec.Meta) if meta.title is not None), bpy.path.display_name(field.name))


def graph_widgets(value: msgspec.Struct, prefix: KeyPath, editors: tuple[CurveEditor, ...] = ()) -> tuple[GraphWidget, ...]:
    """Returns active graphed paths of value in field order, each shared editor once at its first field."""
    shared = {field: editor for editor in editors for field in editor.fields}
    return tuple(
        GraphWidget(
            node=item_key(*prefix, editor.key) if editor is not None else item_key(*prefix, *path),
            heading=bpy.path.display_name(editor.key) if editor is not None else titled(field),
            shape=shape,
            field=field,
            path=path,
            value=value,
        )
        for path, (field, shape) in graphed(type(value), value=Some(value)).items()
        if active(value, path)
        for editor in (shared.get(field.name),)
        if editor is None or field.name == editor.fields[0]
    )


def graph_drawn(
    layout: bpy.types.UILayout, node: bpy.types.Node, widget: GraphWidget, editing: tuple[Mapping[type[bpy.types.Node], RowDraw], RowDraw], context: bpy.types.Context
) -> BlenderResult[None]:
    """Draws widget heading over its node's native editor: curve, ramp, or node inputs."""
    curves, ramp = editing
    inner = layout.column()
    inner.use_property_split = False
    inner.label(text=widget.heading)
    match widget.shape:
        case native.VarTupleType(item_type=native.StructType(cls=kind)) if (typing.get_origin(kind) or kind) is Knot:
            return Option.of_optional(curves.get(type(node))).map(lambda draw: draw(inner, node, context)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(node), key="mapping"),))
        case native.StructType(cls=kind) if kind is Ramp:
            return Ok(ramp(inner, node, context))
        case native.VarTupleType():
            return Ok(None)
        case _:
            inner.template_node_inputs(node)
            return Ok(None)


@contextmanager
def capturing(identity: Identity, before: bpy.types.SceneCompositorEffect, output: Fragment, *, stop: bool = False) -> Generator[BlenderResult[bpy.types.SceneCompositorEffect]]:
    """Places scoped pass-through capture entry before entry."""
    scene: bpy.types.Scene = before.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    active = next((entry for entry in scene.compositor_effects if entry.is_active), None)
    at = tuple(scene.compositor_effects).index(before)
    face = Interface(inputs=(Socket(key=Shell.image, name="Image", kind=bpy.types.NodeSocketColor),), outputs=(Socket(key=Shell.result, name="Image", kind=bpy.types.NodeSocketColor),))
    with ExitStack() as scope:
        if active is not None:
            scope.callback(setattr, active, "is_active", True)

        def placed(tree: bpy.types.NodeTree) -> BlenderResult[bpy.types.SceneCompositorEffect]:
            scope.callback(bpy.data.node_groups.remove, tree)
            entry = scene.compositor_effects.new("Input Capture")
            scope.callback(scene.compositor_effects.remove, entry)
            return written(entry, "node_group", tree).bind(lambda _: relocated(scene, entry, at)).map(lambda _: entry)

        enabled = Setting(descriptor=Descriptor(root=bpy.types.Scene, owner="render", prop=bpy.types.RenderSettings.bl_rna.properties["use_compositing"]), accepted=(True,))
        suffix = tuple(scene.compositor_effects)[at:] if stop else ()
        for entry in suffix:
            scope.callback(setattr, entry, "enable_for_render", entry.enable_for_render)
        yield (
            editable(scene)
            .bind(lambda _: check(enabled, bpy.context, Some(scene)))
            .bind(lambda _: accumulated(written(entry, "enable_for_render", False) for entry in suffix))
            .bind(lambda _: NodeGroup.built("input_capture", "Input Capture", face, {Shell.result: (output, End(node=INPUTS, socket=Shell.image))}))
            .map(lambda group: msgspec.structs.replace(group, single=False, saved=Saved.NEVER))
            .bind(lambda group: made(group, identity, of=Some(scene)))
            .bind(placed)
        )


def render_inputs(scene: bpy.types.Scene, enabled: Enabled = Enabled.RENDER) -> tuple[tuple[bpy.types.CompositorNodeRLayers, bpy.types.NodeSocket], ...]:
    """Returns Render Layers outputs enabled entries reach through nested groups."""
    pending: deque[tuple[bpy.types.NodeSocket, tuple[bpy.types.CompositorNodeGroup, ...]]] = deque(
        (socket, ())
        for entry in scene.compositor_effects
        if enabled.descriptor.read(entry) is True
        for tree in Option.of_optional(entry.node_group).to_list()
        for node in (*active_output(tree).to_option().to_list(), *(node for node in tree.nodes if isinstance(node, bpy.types.CompositorNodeOutputFile)))
        for socket in node.inputs  # ty: ignore[not-iterable]
    )
    seen: set[tuple[int, tuple[int, ...]]] = set()
    found: dict[tuple[int, int], tuple[bpy.types.CompositorNodeRLayers, bpy.types.NodeSocket]] = {}
    while pending:
        socket, parents = pending.popleft()
        if (key := (socket.as_pointer(), tuple(parent.as_pointer() for parent in parents))) in seen:
            continue
        seen.add(key)
        if not socket.is_output:
            pending.extend((link.from_socket, parents) for link in socket.links)  # type: ignore[misc, union-attr]  # ty: ignore[invalid-argument-type, not-iterable]
            continue
        match socket.node:
            case bpy.types.CompositorNodeRLayers() as node:
                found[node.as_pointer(), socket.as_pointer()] = node, socket
            case bpy.types.CompositorNodeGroup() as node:
                pending.extend(
                    (inside, (*parents, node))
                    for tree in Option.of_optional(node.node_tree).to_list()
                    for output in active_output(tree).to_option().to_list()
                    for inside in socket_at(output.inputs, socket.identifier).to_option().to_list()
                )
            case bpy.types.NodeGroupInput():
                pending.extend((outside, parents[:-1]) for parent in parents[-1:] for outside in socket_at(parent.inputs, socket.identifier).to_option().to_list())
            case node:
                pending.extend((inside, parents) for inside in node.inputs if not node.mute or any(link.from_socket == inside and link.to_socket == socket for link in node.internal_links))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    return tuple(found.values())


def projected[E: Effect](
    current: Mapping[EffectName, Configured[E]],
    recipe: Mapping[EffectName, Configured[E]],
    *,
    locked: frozenset[EffectName] = frozenset(),
    fields: Mapping[EffectName, frozenset[tuple[Key | int, ...]]] = {},
) -> dict[EffectName, Configured[E]]:
    """Merges sparse recipe values and order into current records, keeping locks."""

    def kept(prior: object, value: object, selected: frozenset[tuple[Key | int, ...]]) -> object:
        if not selected:
            return value
        if () in selected or type(prior) is not type(value):
            return prior
        match value, prior:
            case msgspec.Struct(), msgspec.Struct():
                return msgspec.structs.replace(
                    value,
                    **{
                        field.name: kept(getattr(prior, field.name), getattr(value, field.name), frozenset(path[1:] for path in selected if path and path[0] == field.name))
                        for field in msgspec.structs.fields(value)
                    },
                )
            case tuple(), tuple():
                return (
                    prior
                    if len(value) != len(prior)
                    else tuple(kept(before, after, frozenset(path[1:] for path in selected if path and path[0] == index)) for index, (before, after) in enumerate(zip(prior, value), 1))
                )
            case _:
                return value

    changed = {
        name: msgspec.structs.replace(
            value,
            **{
                field.name: kept(getattr(current[name], field.name), getattr(value, field.name), frozenset(path[1:] for path in fields.get(name, frozenset()) if path and path[0] == field.name))
                for field in msgspec.structs.fields(value)
            },
        )
        if name in current
        else value
        for name, value in recipe.items()
        if name not in locked
    }
    ordered = iter(name for name in changed if name in current)
    return {selected: changed[selected] if selected in changed else current[selected] for name in current for selected in (next(ordered) if name in changed else name,)} | {
        name: value for name, value in changed.items() if name not in current
    }


def rows(
    kind: type[msgspec.Struct], *, value: Option[msgspec.Struct] = Nothing, prefix: KeyPath = (), counts: Mapping[Key, int] = {}
) -> tuple[tuple[KeyPath, msgspec.structs.FieldInfo, native.Type], ...]:
    """Walks record fields, reading collection indices from values."""

    def member(field: msgspec.structs.FieldInfo, shape: native.Type, at: KeyPath, selected: Option[object]) -> Iterator[tuple[KeyPath, msgspec.structs.FieldInfo, native.Type]]:
        yield at, field, shape
        if markers(field.type, GraphForm):
            return
        match unwrapped(shape):
            case native.TupleType() as content if not vector(content):
                components = RecordGroup.components(msgspec.structs.replace(field, default=selected.default_value(msgspec.NODEFAULT)))
                for index, component in enumerate(components, 1):
                    yield from member(component, native.type_info(component.type), (*at, index), Some(component.default) if component.default is not msgspec.NODEFAULT else Nothing)
            case native.StructType(cls=nested) if (typing.get_origin(nested) or nested) not in {Rgb, Ramp, Knot}:
                yield from walked(nested, at, selected)
            case native.UnionType(types=cases):
                for alternative in cases:
                    if isinstance(alternative, native.StructType) and issubclass(alternative.cls, Case):
                        yield from walked(alternative.cls, (*at, alternative.cls), selected.filter(lambda record: isinstance(record, alternative.cls)))
            case native.VarTupleType(item_type=native.StructType() as nested) if (typing.get_origin(nested.cls) or nested.cls) is not Knot:
                values = selected.to_optional()
                if isinstance(values, tuple | list):
                    for index, entry in enumerate(values, 1):
                        yield from walked(nested.cls, (*at, index), Some(entry))
                elif selected.is_none():
                    for index in range(1, counts.get(item_key(*at), 0) + 1):
                        yield from walked(nested.cls, (*at, index), Nothing)

    def walked(record: type[msgspec.Struct], path: KeyPath, held: Option[object]) -> Iterator[tuple[KeyPath, msgspec.structs.FieldInfo, native.Type]]:
        fields = msgspec.structs.fields(record)
        for field, shape in zip(fields, native.multi_type_info(field.type for field in fields)):
            if (typing.get_origin(record) or record) is Gated and field.name == "enabled":
                continue
            yield from member(field, shape, (*path, field.name), held.map(lambda record: getattr(record, field.name)))

    return tuple(walked(kind, prefix, value))


def graphed(
    kind: type[msgspec.Struct], *, value: Option[msgspec.Struct] = Nothing, prefix: KeyPath = (), counts: Mapping[Key, int] = {}
) -> dict[KeyPath, tuple[msgspec.structs.FieldInfo, native.Type]]:
    """Returns graph-owned fields with unwrapped shapes."""

    def graph(field: msgspec.structs.FieldInfo, shape: native.Type) -> Option[native.Type]:
        match shape:
            case native.StructType(cls=record):
                return Some(shape) if markers(field.type, GraphForm) or (typing.get_origin(record) or record) is Ramp else Nothing
            case native.VarTupleType(item_type=native.StructType(cls=record)):
                return Some(shape)
            case _:
                return Some(shape) if markers(field.type, GraphForm) else Nothing

    return {path: (field, shape) for path, field, raw in rows(kind, value=value, prefix=prefix, counts=counts) for shape in graph(field, unwrapped(raw)).to_list()}


def menus(kind: type[msgspec.Struct], *, value: Option[msgspec.Struct] = Nothing) -> dict[KeyPath, MenuItems]:
    """Derives menu items from enum and tagged-union fields."""

    def menu(shape: native.Type) -> Option[MenuItems]:
        match unwrapped(shape):
            case native.EnumType(cls=choices) if issubclass(choices, Choice):
                return Some(MenuItems.of(choices))
            case native.UnionType(types=cases):
                return Some(MenuItems.of(tuple(record for case in cases if isinstance(case, native.StructType) for record in (case.cls,) if issubclass(record, Case))))
            case _:
                return Nothing

    return {path: items for path, field, shape in rows(kind, value=value) if not markers(field.type, GraphForm) for items in menu(shape).to_list()}


def hidden(face: Interface) -> Interface:
    """Hides derived sockets, keeping their stored values."""
    descriptor = Descriptor(root=bpy.types.NodeTreeInterfaceSocket, owner="", prop=bpy.types.NodeTreeInterfaceSocket.bl_rna.properties["hide_in_modifier"])
    return msgspec.structs.replace(
        face, inputs=tuple(msgspec.structs.replace(socket, members=socket.members | {descriptor: True}) for socket in face.inputs), panels=tuple(hidden(panel) for panel in face.panels)
    )


def entry_interface(kind: type[Effect], masks: int) -> BlenderResult[Interface]:
    """Composes image sockets, record fields, frame values, and mask panels."""

    def combined(parts: tuple[Interface, Interface, Interface]) -> Interface:
        body, frame, blend = parts
        return Interface(
            outputs=(Socket(key=Shell.result, name="Image", kind=bpy.types.NodeSocketColor),),
            inputs=(Socket(key=Shell.image, name="Image", kind=bpy.types.NodeSocketColor), *body.inputs, *frame.inputs),
            panels=(*body.panels, *((InterfacePanel(key=Shell.blend, name="Blend", inputs=blend.inputs, panels=blend.panels),) if kind is not DisplayView else ())),
        )

    def blended(face: Interface, items: tuple[Interface, ...]) -> Interface:
        key = item_key(Shell.blend, "masks")
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
            Interface.of(Blend).map(lambda face: under(Shell.blend, face)),
            accumulated(Interface.of(Mask).map(lambda face: under(item_key(Shell.blend, "masks", index), face)) for index in range(1, masks + 1)),
        ).map(lambda pair: blended(*pair))
        if kind is not DisplayView
        else Ok(Interface())
    )
    return paired(paired(Interface.of(kind), Interface.of(CameraFrame).map(lambda face: hidden(under(Shell.frame, face)))), blend).map(lambda parts: combined((*parts[0], parts[1])))


def expanded(face: Interface, value: msgspec.Struct) -> BlenderResult[Interface]:
    """Builds collection item panels from record values."""
    collections = {
        item_key(*path): (path, getattr(reached(value, path[:-1]), field.name))
        for path, field, raw in rows(type(value), value=Some(value))
        for shape in (unwrapped(raw),)
        if isinstance(shape, native.VarTupleType)
        and isinstance(shape.item_type, native.StructType)
        and (typing.get_origin(shape.item_type.cls) or shape.item_type.cls) is not Knot
        and active(value, path)
    }

    def expanded_panel(panel: InterfacePanel) -> BlenderResult[InterfacePanel]:
        def items(collection: tuple[KeyPath, tuple[msgspec.Struct, ...]]) -> BlenderResult[InterfacePanel]:
            path, values = collection
            return accumulated(
                Interface
                .of(type(item))
                .bind(lambda face: expanded(face, item))
                .map(lambda face: under(item_key(*path, index), face))
                .map(lambda face: InterfacePanel(key=item_key(*path, index), name=f"{panel.name} {index}", inputs=face.inputs, panels=face.panels))
                for index, item in enumerate(values, 1)
            ).map(lambda items: msgspec.structs.replace(panel, panels=items))

        return (
            Option
            .of_optional(collections.get(panel.key))
            .map(items)
            .default_with(lambda: accumulated(map(expanded_panel, panel.panels)).map(lambda panels: msgspec.structs.replace(panel, panels=panels)))
        )

    return accumulated(map(expanded_panel, face.panels)).map(lambda panels: msgspec.structs.replace(face, panels=panels))


def ownerships(record: type[Effect]) -> tuple[Ownership[bpy.types.NodeTree], Ownership[bpy.types.NodeTree]]:
    """Declares shared and per-entry group ownerships."""
    key = item_key(record)
    return Ownership(kind=bpy.types.NodeTree, key=key, saved=Saved.USED), Ownership(kind=bpy.types.NodeTree, key=f"{key}_entry", saved=Saved.USED, single=False)


def local_entry(entry: bpy.types.SceneCompositorEffect, record: Entry[Effect]) -> BlenderResult[bpy.types.SceneCompositorEffect]:
    """Returns entry when its effect is pointwise."""
    return Ok(entry) if record.effect.pointwise else Error((BlenderChoiceFault(path=entry.name, identifier="False", accepted=("True",)),))


def entries(identity: Identity, scene: bpy.types.Scene, record: type[Effect]) -> tuple[bpy.types.SceneCompositorEffect, ...]:
    """Returns entries with either ownership mark of record in stack order."""
    keys = frozenset(owner.key for owner in ownerships(record))
    return tuple(entry for entry in scene.compositor_effects for group in Option.of_optional(entry.node_group).to_list() if Mark.read(identity, group).filter(lambda mark: mark.key in keys).is_some())


def editable(scene: bpy.types.Scene) -> BlenderResult[bpy.types.Scene]:
    """Refuses compositor edits library override scenes cannot keep."""
    return Ok(scene) if scene.override_library is None else Error((BlenderEditFault(request=repr(scene.override_library), error=AttributeError(name="compositor_effects")),))


def configured(record: Effect) -> BlenderResult[PyOpenColorIO.Config]:
    """Returns live OCIO config resolving process space of spaced records."""
    return config_of((record.space,) if isinstance(record, Spaced) else ())


def hold(identity: Identity, scene: bpy.types.Scene, row: Placeable) -> BlenderResult[None]:
    """Captures and applies row holds once, then camera requirements and Display View holds on overriding outputs."""

    def raw() -> BlenderResult[None]:
        return accumulated(
            Snapshot.of(bpy.types.ColorManagedViewSettings, {path.removeprefix("view_settings."): value for path, value in formation.holds.items()}).apply(owner.view_settings).outcome  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            for owner in owners(scene)
            if owner.color_management == "OVERRIDE"
        ).map(lambda _: None)

    def held(pair: tuple[str, dict[Key, Snapshot]], descriptors: tuple[Descriptor, ...]) -> BlenderResult[None]:
        slot, values = pair
        prior = Option.of_optional(values.get(row.kind))

        def captured(snapshot: Snapshot) -> BlenderResult[None]:
            if prior.is_none():
                scene[slot] = msgspec.json.encode(values | {row.kind: snapshot}, order="deterministic").decode()
            return (
                (Snapshot.of(bpy.types.Scene, dict(row.holds)).apply(scene).outcome if prior.is_none() else Ok(None))
                .bind(lambda _: cameras.required(Snapshot.of(bpy.types.Camera, dict(row.cameras)), scene, identity))
                .bind(lambda _: raw() if row.record is DisplayView else Ok(None))
                .map(lambda _: None)
            )

        return prior.map(Ok).default_with(lambda: Snapshot.capture(scene, Some(descriptors))).bind(captured)

    return paired(stored(identity, scene, "held", dict[Key, Snapshot]), accumulated(Descriptor.resolve(bpy.types.Scene, path, bpy.types.Property) for path in row.holds)).bind(lambda pair: held(*pair))


def displayed(scene: bpy.types.Scene, view: DisplayView) -> BlenderResult[None]:
    """Writes display, view, and look to scene and every output overriding color management."""

    def written_view(holder: bpy.types.Scene | bpy.types.ImageFormatSettings) -> BlenderResult[None]:
        return (
            written(holder.display_settings, "display_device", view.display)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            .bind(lambda _: written(holder.view_settings, "view_transform", view.view))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            .bind(lambda _: written(holder.view_settings, "look", view.look.value if view.look.enabled else "None"))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            .map(lambda _: None)
        )

    return paired(written_view(scene), accumulated(written_view(owner) for owner in owners(scene) if owner.color_management == "OVERRIDE")).map(lambda _: None)


def entry_input(identity: Identity, entry: bpy.types.SceneCompositorEffect, key: Key) -> "BlenderResult[bpy.types.bpy_struct[object]]":
    """Resolves live entry input by key after interface rebuilds."""
    return (
        Result[bpy.types.NodeTree, BlenderFaults]
        .bind(
            Option.of_optional(entry.node_group).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key="node_group"),)),
            lambda group: slot(identity).map(lambda name: addresses(group, name)),
        )
        .bind(lambda keys: Option.of_optional(keys.get(key)).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key=key),)))
        .bind(lambda address: interface_input(entry, address))
    )


def relocated(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, index: int) -> BlenderResult[bool]:
    """Moves entry to stack index unless it sits there, answering whether it moved."""
    return (
        Ok(False)
        if scene.compositor_effects.find(entry.name) == index
        else called(bpy.ops.scene.move_compositor_effect_to_index, {"name": entry.name, "index": index}, override=Some(bpy.context.temp_override(scene=scene))).map(lambda _: True)
    )


def interface_input(entry: bpy.types.SceneCompositorEffect, identifier: str) -> "BlenderResult[bpy.types.bpy_struct[object]]":
    """Resolves live entry input by interface socket identifier."""
    match struct_at(entry, f"properties.inputs.{identifier}").to_optional():
        case bpy.types.bpy_struct() as value:
            return Ok(value)
        case _:
            return Error((BlenderAbsentFault(owner=repr(entry), key=identifier),))


def path(identity: Identity, entry: bpy.types.SceneCompositorEffect, key: Key) -> BlenderResult[str]:
    """Returns animation data path of entry input value."""
    return entry_input(identity, entry, key).map(lambda value: value.path_from_id("value"))


def stored[T](identity: Identity, holder: bpy.types.ID, name: Key, kind: type[T]) -> BlenderResult[tuple[str, T]]:
    """Reads typed JSON store under extension attachment name."""
    return identity.name(Name.ATTACHMENT, name).bind(
        lambda slot: (
            catch(lambda: msgspec.json.decode(holder.get(slot, b"{}"), type=kind), exception=msgspec.DecodeError)()
            .map(lambda value: (slot, value))
            .map_error(lambda error: (BlenderReadFault(request=repr(holder), error=error),))
        )
    )


def inputs[T: msgspec.Struct](
    identity: Identity, entry: bpy.types.SceneCompositorEffect, prefix: KeyPath, record: type[T], graphs: Mapping[Key, object] = {}, *, baseline: Option[T] = Nothing
) -> BlenderResult[T]:
    """Reads record from graph values and group inputs or from baseline."""
    graph_paths = frozenset(graphed(record, value=baseline, prefix=prefix))

    def socket(at: KeyPath) -> BlenderResult[Value]:
        return entry_input(identity, entry, item_key(*at)).bind(lambda value: Descriptor.resolve(value, "value", bpy.types.Property).map(lambda descriptor: descriptor.read(value)))

    def read(at: KeyPath, field: msgspec.structs.FieldInfo, shape: native.Type, annotation: object) -> BlenderResult[object]:
        key = item_key(*at)
        match shape, baseline:
            case _, _ if key in graphs and (
                markers(field.type, GraphForm)
                or not isinstance(shape, native.VarTupleType)
                or (isinstance(shape.item_type, native.StructType) and (typing.get_origin(shape.item_type.cls) or shape.item_type.cls) is Knot)
            ):
                return Ok(graphs[key])
            case _, Option(tag="some", some=held) if not any(path[: len(at)] == at for path in graph_paths):
                return Ok(msgspec.to_builtins(reached(held, at[len(prefix) :])))
            case native.StructType(cls=kind, fields=(_, value)), _ if (typing.get_origin(kind) or kind) is Gated:
                enabled_field, value_field = Gated.__struct_fields__
                _, declared = msgspec.structs.fields(kind)
                selected_annotation = annotation if markers(annotation, Presentation) else declared.type
                enabled = baseline.map(lambda held: Ok(reached(held, (*at[len(prefix) :], enabled_field)))).default_with(lambda: socket(at))
                return paired(enabled, read((*at, value.name), declared, unwrapped(value.type), selected_annotation)).map(lambda pair: {enabled_field: pair[0], value_field: pair[1]})
            case native.StructType(cls=kind), _ if (typing.get_origin(kind) or kind) is Rgb:
                return entry_input(identity, entry, key).map(lambda value: value.value[:3])  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            case native.StructType(cls=kind), _:
                return lowered(kind, at)
            case native.UnionType(types=cases), Option(tag="some", some=held):
                selected_type = type(reached(held, at[len(prefix) :]))
                selected_kind, selected_shape = next((case.cls, case) for case in cases if isinstance(case, native.StructType) and issubclass(case.cls, Case) and case.cls is selected_type)
                return lowered(selected_kind, (*at, selected_kind)).map(lambda values: values | ({selected_shape.tag_field: selected_shape.tag} if selected_shape.tag_field is not None else {}))
            case native.UnionType(types=cases), _:
                alternatives: tuple[tuple[type[Case], native.StructType], ...] = tuple((case.cls, case) for case in cases if isinstance(case, native.StructType) and issubclass(case.cls, Case))

                def selected(label: object) -> BlenderResult[object]:
                    chosen: BlenderResult[tuple[type[Case], native.StructType]] = Option.of_optional(next((case for case in alternatives if case[0].title == label), None)).to_result_with(
                        lambda: (BlenderChoiceFault(path=key, identifier=str(label), accepted=tuple(kind.title for kind, _ in alternatives)),)
                    )
                    return Result[tuple[type[Case], native.StructType], BlenderFaults].bind(
                        chosen, lambda case: lowered(case[0], (*at, case[0])).map(lambda values: values | ({case[1].tag_field: case[1].tag} if case[1].tag_field is not None else {}))
                    )

                return socket(at).bind(selected)
            case native.EnumType(cls=choices), _ if issubclass(choices, Choice):
                members = tuple((choice.label, choice.value) for choice in choices)
                return socket(at).bind(
                    lambda label: Option.of_optional(next((value for title, value in members if title == label), None)).to_result_with(
                        lambda: (BlenderChoiceFault(path=key, identifier=str(label), accepted=tuple(title for title, _ in members)),)
                    )
                )
            case native.SetType(item_type=native.EnumType(cls=choices)) | native.FrozenSetType(item_type=native.EnumType(cls=choices)), _:
                return accumulated(socket((*at, choice.name)).map(lambda selected: (choice, selected)) for choice in choices).map(
                    lambda values: tuple(choice.value for choice, selected in values if selected is True)
                )
            case native.FloatType(), _:
                return socket(at).bind(lambda value: Unit.of(annotation, shape).restored(value, shape))
            case native.TupleType(), _ if vector(shape):
                return socket(at).bind(lambda value: Unit.of(annotation, shape).restored(value, shape))
            case native.TupleType(), _:
                return accumulated(read((*at, index), component, unwrapped(native.type_info(component.type)), component.type) for index, component in enumerate(RecordGroup.components(field), 1))
            case native.VarTupleType(item_type=native.StructType() as item), _:
                match graphs.get(key):
                    case tuple() | list() as collection:
                        return accumulated(lowered(item.cls, (*at, index)) for index in range(1, len(collection) + 1))
                    case None:
                        return Error((BlenderAbsentFault(owner=repr(entry), key=key),))
                    case malformed:
                        return Error((BlenderReadFault(request=key, error=TypeError(repr(malformed))),))
            case _:
                return socket(at)

    def lowered(kind: type[msgspec.Struct], at: KeyPath) -> BlenderResult[dict[str, object]]:
        fields = msgspec.structs.fields(kind)
        return accumulated(
            read((*at, field.name), field, unwrapped(shape), field.type).map(lambda value: (field.encode_name, value))
            for field, shape in zip(fields, native.multi_type_info(field.type for field in fields))
        ).map(dict)

    return lowered(record, prefix).bind(
        lambda values: catch(lambda: msgspec.convert(values, record), exception=msgspec.ValidationError)().map_error(lambda error: (BlenderReadFault(request=repr(entry), error=error),))
    )


def entered(identity: Identity, entry: bpy.types.SceneCompositorEffect, prefix: KeyPath, record: msgspec.Struct) -> BlenderResult[bool]:
    """Writes inputs differing from record outside graph fields, answering whether any changed."""

    def write(at: KeyPath, shape: native.Type, annotation: object, value: object) -> BlenderResult[bool]:
        def socket(held: Value) -> BlenderResult[bool]:
            return entry_input(identity, entry, item_key(*at)).bind(lambda item: written(item, "value", held)).map(Option.is_some)

        if isinstance(value, Gated):
            return socket(value.enabled)
        if isinstance(value, Rgb):
            return socket((*msgspec.structs.astuple(value), 1.0))
        if isinstance(value, Case) and isinstance(shape, native.UnionType):
            return socket(type(value).title)
        if isinstance(value, Choice):
            return socket(value.label)
        if isinstance(value, set | frozenset) and isinstance(shape, native.SetType | native.FrozenSetType) and isinstance(shape.item_type, native.EnumType):
            return accumulated(
                entry_input(identity, entry, item_key(*at, choice.name)).bind(lambda item: written(item, "value", choice in value)).map(Option.is_some) for choice in shape.item_type.cls
            ).map(any)
        if isinstance(shape, native.TupleType) and not vector(shape):
            return Ok(False)
        if isinstance(shape, native.FloatType | native.TupleType):
            return Unit.of(annotation, shape).stored(value, shape).bind(socket)
        if isinstance(shape, native.StructType | native.VarTupleType):
            return Ok(False)
        return socket(msgspec.to_builtins(value))

    def value(at: KeyPath) -> object:
        return reached(record, at[len(prefix) :])

    fields = rows(type(record), value=Some(record), prefix=prefix)
    return accumulated(
        write(
            at,
            unwrapped(shape),
            next(
                (
                    parent.type
                    for path, parent, raw in fields
                    for kind in (unwrapped(raw),)
                    if path == at[:-1] and isinstance(kind, native.StructType) and (typing.get_origin(kind.cls) or kind.cls) is Gated and markers(parent.type, Presentation)
                ),
                field.type,
            ),
            value(at),
        )
        for at, field, shape in fields
        if active(record, at[len(prefix) :]) and not markers(field.type, GraphForm)
    ).map(any)


def reached(value: msgspec.Struct, path: KeyPath) -> object:
    """Reads value at record path, skipping case type parts."""
    held: object = value
    for part in path:
        if isinstance(part, int) and isinstance(held, tuple | list):
            held = held[part - 1]
        elif isinstance(part, str):
            held = getattr(held, part)
    return held


def active(value: msgspec.Struct, path: KeyPath) -> bool:
    """Answers whether record holds every case its path names."""
    return all(not isinstance(part, type) or isinstance(reached(value, path[:index]), part) for index, part in enumerate(path))


def graph_values(
    identity: Identity,
    entry: bpy.types.SceneCompositorEffect,
    records: tuple[tuple[KeyPath, type[msgspec.Struct], Option[msgspec.Struct], Domain, tuple[CurveEditor, ...]], ...],
    *,
    edited: bool = True,
) -> BlenderResult[dict[Key, object]]:
    """Overlays ramp and curve edits on stored or given graph values, knots read from the curve of a shared editor or the field node."""

    def values(group: bpy.types.NodeTree, held: dict[Key, object]) -> BlenderResult[dict[Key, object]]:
        marked = owned(group, identity)
        supplied = {
            item_key(*prefix, *path): msgspec.to_builtins(reached(record, path))
            for prefix, kind, value, _, _ in records
            for record in value.to_list()
            for path in graphed(kind, value=value)
            if active(record, path)
        }
        answer = held | supplied
        fields = tuple(
            (
                path,
                shape,
                domain,
                Option.of_optional(next(((item_key(*prefix, editor.key), editor.fields.index(path[-1]), editor.plot(path[-1])) for editor in editors if path[-1] in editor.fields), None)),
            )
            for prefix, kind, value, domain, editors in records
            for path, (_, shape) in graphed(kind, value=value, prefix=prefix, counts={key: len(value) for key, value in answer.items() if isinstance(value, tuple | list)}).items()
        )

        def overlaid(restored: dict[Key, tuple[str, Ramp]]) -> dict[Key, object]:
            answer.update({key: msgspec.to_builtins(value) for key, (_, value) in restored.items()})
            for path, shape, _domain, shared in fields:
                key = item_key(*path)
                if not edited or (key in restored and restored[key][0] != bpy.data.colorspace.working_space):
                    continue
                node, curve, plot = shared.map(lambda held: (marked.get(held[0]), held[1], held[2])).default_with(lambda: (marked.get(key), 0, Plot()))
                match shape, node:
                    case native.StructType(cls=record), bpy.types.ShaderNodeValToRGB() as node if record is Ramp:
                        ramp = node.color_ramp
                        value = Ramp(
                            stops=tuple(Stop(position=element.position, color=Rgb(red=element.color[0], green=element.color[1], blue=element.color[2])) for element in ramp.elements),  # type: ignore[index, union-attr]  # ty: ignore[unresolved-attribute]
                            interpolation=Interpolation[ramp.interpolation],  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                            mode=ColorMode[ramp.color_mode],  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                            hue_path=HuePath[ramp.hue_interpolation],  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                        )
                        answer[key] = msgspec.to_builtins(value)
                    case (
                        native.VarTupleType(item_type=native.StructType(cls=record)),
                        bpy.types.ShaderNodeFloatCurve() | bpy.types.CompositorNodeCurveRGB() | bpy.types.CompositorNodeHueCorrect() as held,
                    ) if (typing.get_origin(record) or record) is Knot:
                        answer[key] = tuple((point.location.x, plot.held(point.location.x, point.location.y)) for point in held.mapping.curves[curve].points)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            return answer

        return (
            accumulated(
                catch(lambda: msgspec.convert(held[key], tuple[str, Ramp]), exception=msgspec.ValidationError)()
                .map_error(lambda error: (BlenderReadFault(request=key, error=error),))
                .map(lambda value: (key, value))
                for path, shape, domain, _ in fields
                for key in (item_key(*path),)
                if isinstance(shape, native.StructType) and shape.cls is Ramp and domain is Domain.DISPLAY and key in held and key not in supplied
            )
            .map(dict)
            .map(overlaid)
        )

    return Result[bpy.types.NodeTree, BlenderFaults].bind(
        Option.of_optional(entry.node_group).to_result_with(lambda: (BlenderAbsentFault(owner=repr(entry), key="node_group"),)),
        lambda group: (stored(identity, group, "values", dict[Key, object]).map(lambda pair: pair[1]) if any(value.is_none() for _, _, value, _, _ in records) else Ok({})).bind(
            lambda held: values(group, held)
        ),
    )


def stored_graphs(identity: Identity, group: bpy.types.NodeTree, records: tuple[tuple[KeyPath, msgspec.Struct, Domain], ...]) -> BlenderResult[bool]:
    """Writes active graph values, keeping inactive case values, answering whether store changed."""

    def wrote(pair: tuple[str, dict[Key, object]]) -> bool:
        slot, held = pair
        live = {item_key(*prefix, *path): (reached(record, path), domain) for prefix, record, domain in records for path in graphed(type(record), value=Some(record)) if active(record, path)}
        values = {key: value for key, value in held.items() if key not in live} | {
            key: (bpy.data.colorspace.working_space, msgspec.to_builtins(value)) if isinstance(value, Ramp) else msgspec.to_builtins(value)
            for key, (value, domain) in live.items()
            if not isinstance(value, Ramp) or domain is Domain.DISPLAY
        }
        if changed := (text := msgspec.json.encode(values, order="deterministic")) != msgspec.json.encode(held, order="deterministic"):
            group[slot] = text.decode()
        return changed

    return stored(identity, group, "values", dict[Key, object]).map(wrote)


def case_values(record: Effect, tree: Option[bpy.types.NodeTree], identity: Identity) -> BlenderResult[dict[KeyPath, tuple[Case, ...]]]:
    """Builds active case and complete stored alternatives per union path, nested unions yielding held case."""

    def made(values: Mapping[Key, object]) -> BlenderResult[dict[KeyPath, tuple[Case, ...]]]:
        def case(at: KeyPath, active: object, alternative: type[Case], fields: tuple[native.Field, ...]) -> Option[BlenderResult[Case]]:
            given = {field.encode_name: values[key] for field in fields for key in (item_key(*at, alternative, field.name),) if key in values}
            return (
                Some(Ok(active))
                if isinstance(active, alternative)
                else Nothing
                if any(field.required and field.encode_name not in given for field in fields)
                else Some(
                    catch(lambda: msgspec.convert(given, alternative), exception=msgspec.ValidationError)().map_error(
                        lambda error: (BlenderReadFault(request=item_key(*at, alternative), error=error),)
                    )
                )
            )

        def unions(owner: msgspec.Struct, prefix: KeyPath, *, alternatives: bool) -> BlenderResult[dict[KeyPath, tuple[Case, ...]]]:
            return accumulated(
                accumulated(
                    cased
                    for branch in shape.types
                    if isinstance(branch, native.StructType) and issubclass(branch.cls, Case) and (alternatives or isinstance(active, branch.cls))
                    for cased in case(at, active, branch.cls, branch.fields).to_list()
                ).bind(lambda cases: accumulated(unions(held, (*at, type(held)), alternatives=alternatives and held is active) for held in cases).map(lambda inner: reduce(or_, inner, {at: cases})))
                for at, _, raw in rows(type(owner), value=Some(owner), prefix=prefix)
                if not any(isinstance(part, type) for part in at[len(prefix) :])
                for shape in (unwrapped(raw),)
                if isinstance(shape, native.UnionType)
                for active in (reached(owner, at[len(prefix) :]),)
            ).map(lambda found: reduce(or_, found, {}))

        return unions(record, (), alternatives=True)

    return tree.map(lambda group: stored(identity, group, "values", dict[Key, object]).bind(lambda pair: made(pair[1]))).default_with(lambda: made({}))


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

    def bound(framed: Framed) -> Derived[Effect, CameraFrame]:
        return Derived(
            record=CameraFrame,
            of=Some(lambda _identity, scene, _record: camera_frame(framed, scene)),
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

    return FrameSize().bound(identity).map(bound)


def precise(_record: Effect, layer: str) -> BlenderResult[PassRequest]:
    """Requests full compositor precision with no render pass."""
    return Ok(PassRequest(layer=layer, precise=True))


def applied_channel(channel: Animation, scene: bpy.types.Scene) -> BlenderResult[bool]:
    """Writes changed keyframes, answering whether it wrote, refusing cycle-forming drivers."""
    match channel.source:
        case Driven():
            return Error((BlenderChoiceFault(path=channel.path, identifier=Driven.title, accepted=(Keys.title,)),))
        case Keys():
            return channel.apply(scene, bpy.context)


def keyed_product(
    identity: Identity,
    scene: bpy.types.Scene,
    entry: bpy.types.SceneCompositorEffect,
    source: type[Effect],
    source_key: Key,
    target_key: Key,
    transform: Callable[[npt.NDArray[np.float64]], npt.NDArray[np.float64]],
) -> BlenderResult[tuple[Animation, ...]]:
    """Keys product of later source gains weighted by opacity and render enable."""
    frames = range(scene.frame_start, scene.frame_end + 1)
    ordered = tuple(scene.compositor_effects)
    following = ordered[ordered.index(entry) + 1 :]

    def samples(held: bpy.types.SceneCompositorEffect) -> BlenderResult[tuple[tuple[float, ...], ...]]:
        _, opacity, _ = Blend.__struct_fields__
        return paired(path(identity, held, source_key), path(identity, held, item_key(Shell.blend, opacity))).bind(
            lambda paths: accumulated(sampled(scene, address, frames) for address in (*paths, held.path_from_id(Enabled.RENDER.descriptor.prop.identifier)))
        )

    def formed(parts: tuple[str, tuple[tuple[tuple[float, ...], ...], ...]]) -> tuple[Animation, ...]:
        address, sources = parts
        values, opacity, enabled = np.asarray(sources, np.float64).reshape(-1, 3, len(frames)).transpose(1, 0, 2)
        gains = np.prod(1.0 + opacity * enabled * (transform(values) - 1.0), axis=0)
        return (Animation(struct=address, member="value", source=Keys(points=tuple(zip(map(float, frames), map(float, gains))))),)

    return paired(entry_input(identity, entry, target_key).map(lambda held: held.path_from_id()), accumulated(samples(held) for held in entries(identity, scene, source) if held in following)).map(
        formed
    )


def viewed(holder: bpy.types.Scene | bpy.types.CompositorNodeConvertToDisplay) -> DisplayView:
    """Reads Display View from scene or display node, look None as disabled."""
    look = holder.view_settings.look  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    return DisplayView(display=holder.display_settings.display_device, view=holder.view_settings.view_transform, look=Gated(enabled=look != "None", value=look))  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]


def formed(shell: Shell[DisplayView]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds Convert to Display node, writing display before view and look."""
    record = shell.record
    return node(
        "display",
        bpy.types.CompositorNodeConvertToDisplay,
        {"Image": shell.source},
        {"display_settings.display_device": record.display, "view_settings.view_transform": record.view, "view_settings.look": record.look.value if record.look.enabled else "None"},
    ).map(ended("display", "Image"))


def followed(identity: Identity) -> BlenderResult[None]:
    """Writes changed scene display device to owned display nodes, storing Blender's matched view."""

    def updated(scene: bpy.types.Scene, entry: bpy.types.SceneCompositorEffect, display: bpy.types.CompositorNodeConvertToDisplay) -> BlenderResult[bool]:
        return (
            written(display.display_settings, "display_device", scene.display_settings.display_device)  # type: ignore[arg-type, union-attr]  # ty: ignore[invalid-argument-type, unresolved-attribute]
            .bind(lambda _: formation.read(identity, entry, edited=False))
            .bind(lambda record: formation.write(identity, scene, entry, msgspec.structs.replace(record, effect=viewed(display))))
        )

    return accumulated(
        updated(scene, entry, node)
        for source in bpy.data.scenes
        for scene in editable(source).to_option().to_list()
        for entry in entries(identity, scene, DisplayView)
        for group in Option.of_optional(entry.node_group).to_list()
        for node in Option.of_optional(owned(group, identity).get("display")).to_list()
        if isinstance(node, bpy.types.CompositorNodeConvertToDisplay)  # ty: ignore[redundant-condition-strict]
        and node.display_settings.display_device != scene.display_settings.display_device  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
    ).map(lambda _: None)


def shuttered(scene: bpy.types.Scene) -> Shutter:
    """Reads shutter length and opening offset."""
    return Shutter(length=scene.render.motion_blur_shutter, opening=ShutterPosition[scene.render.motion_blur_position].value)


# --- [COMPOSITION] ----------------------------------------------------------------------

shutter = Derived[Effect, Shutter](
    record=Shutter,
    of=Some(lambda _identity, scene, _record: Ok(shuttered(scene))),
    reads=tuple((bpy.types.RenderSettings, bpy.types.RenderSettings.bl_rna.properties[name]) for name in ("motion_blur_shutter", "motion_blur_position")),
)
formation = CompositorEffect(
    record=DisplayView,
    graph=formed,
    seed=Some(viewed),
    holds={"view_settings.view_transform": "Raw", "view_settings.exposure": 0.0, "view_settings.use_white_balance": False, "view_settings.use_curve_mapping": False},
)
composites = Setting(descriptor=Descriptor(root=bpy.types.Scene, owner="render", prop=bpy.types.RenderSettings.bl_rna.properties["use_compositing"]))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Animated",
    "Built",
    "CameraFrame",
    "CompositorEffect",
    "Configured",
    "Context",
    "Coverage",
    "CurveEditor",
    "Derived",
    "Edge",
    "EffectName",
    "EffectOperators",
    "EffectStack",
    "GainRow",
    "GraphWidget",
    "KeyPath",
    "Named",
    "Opened",
    "Pinned",
    "Placeable",
    "Plot",
    "Preparing",
    "Requested",
    "Shell",
    "Shift",
    "Shutter",
    "ShutterPosition",
    "StackEntry",
    "active",
    "applied_channel",
    "camera_frame",
    "capturing",
    "case_values",
    "composites",
    "curve_editors",
    "entered",
    "entries",
    "entry_input",
    "entry_interface",
    "followed",
    "formation",
    "formed",
    "framing",
    "graph_drawn",
    "graph_values",
    "graph_widgets",
    "graphed",
    "hidden",
    "inputs",
    "interface_input",
    "keyed_product",
    "menus",
    "ownerships",
    "path",
    "precise",
    "projected",
    "reached",
    "relocated",
    "render_inputs",
    "rows",
    "shutter",
    "shuttered",
    "stored_graphs",
    "titled",
    "viewed",
]
