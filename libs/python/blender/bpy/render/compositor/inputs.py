"""Entry inputs by interface key with their captures, restores, defaults, and writes, hidden inputs derived from scene values, the camera frame, the shell an effect graph reads, entry colors kept in the working space, and scene holds."""

from collections import deque
from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack, nullcontext
from functools import partial
from typing import Final, Self

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.collections import Block
import msgspec
import numpy as np
import OpenImageIO
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.paths import Native, rna, RnaPath, Struct, struct_at
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Handler, Startup, updated
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes.addressing import Addressed, input_sockets
from blender.bpy.nodes.desired import InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.nodes.graphs import Expr, group_input, menu, Shape, Term
from blender.bpy.render.compositor.coordinates import FrameSpace
from blender.bpy.render.outputs.framing import FrameSize, Framing
from blender.bpy.scene import cameras
from blender.bpy.settings.attachments import Stored
from blender.imaging.color.processors import processor
from blender.imaging.geometry.frames import Region
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason, typed
from blender.records.fields import Case
from blender.records.trees import Owner, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type Graph[R: Case] = Callable[[Shell[R]], Outcome[Expr]]

# --- [CONSTANTS] ------------------------------------------------------------------------

FRAME_ASPECT: Final = Socket(key="frame_aspect", kind=bpy.types.NodeSocketFloat, description="Pixel aspect", hide_in_modifier=True, default_value=1.0)
FRAME_SCALE: Final = Socket(key="frame_scale", kind=bpy.types.NodeSocketFloat, description="Camera share of overscan", hide_in_modifier=True, default_value=1.0)
FRAME_METERS: Final = Socket(key="frame_meters", kind=bpy.types.NodeSocketFloat, description="Meters per Blender unit", hide_in_modifier=True, default_value=1.0)
FRAME: Final = (FRAME_ASPECT, FRAME_SCALE, FRAME_METERS)

# --- [MODELS] ---------------------------------------------------------------------------


class Derives[R](msgspec.Struct, frozen=True, kw_only=True):
    """Hidden sockets written from scene values by key, rewritten when one of `reads` changes."""

    of: Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect, R], Outcome[Mapping[Key, Value]]]
    sockets: tuple[Socket, ...] = ()
    reads: tuple[tuple[type[Struct], bpy.types.Property], ...] = ()


class Holds(msgspec.Struct, frozen=True, kw_only=True):
    """Scene and camera values held while an entry exists, the prior scene values restored once the last entry goes."""

    scene: Snapshot = msgspec.field(default_factory=lambda: Snapshot.of(bpy.types.Scene, {}))
    cameras: Snapshot = msgspec.field(default_factory=lambda: Snapshot.of(bpy.types.Camera, {}))


class CameraFrame(msgspec.Struct, frozen=True, kw_only=True):
    """Pixel aspect, camera share of overscan, and meters per Blender unit, held by the hidden frame inputs."""

    aspect: float = 1.0
    scale: float = 1.0
    meters: float

    @property
    def values(self) -> dict[Key, Value]:
        """Frame input values by socket key."""
        return {FRAME_ASPECT.key: self.aspect, FRAME_SCALE.key: self.scale, FRAME_METERS.key: self.meters}

    @classmethod
    def read(cls, identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[Self]:
        """Reads the frame inputs of an entry."""

        def value(socket: Socket) -> Outcome[float]:
            return entry_value(identity, entry, socket.key).bind(partial(typed, float, socket.key))

        return paired(value(FRAME_ASPECT), value(FRAME_SCALE), value(FRAME_METERS)).map(lambda read: cls(aspect=read[0], scale=read[1], meters=read[2]))

    @classmethod
    def of(cls, identity: Identity, scene: bpy.types.Scene) -> Outcome[Self]:
        """Reads the pixel aspect, overscan camera share, and unit scale of a scene under the extension's frame size settings."""
        render = scene.render
        return (
            FrameSize()
            .bound(identity)
            .bind(lambda framed: framed.base(scene))
            .map(
                lambda base: cls(
                    aspect=render.pixel_aspect_x / render.pixel_aspect_y,
                    scale=base.map(lambda original: original.size[0] / render.resolution_x).default_value(1.0),
                    meters=scene.unit_settings.scale_length,
                )
            )
        )

    def window(self, spec: OpenImageIO.ImageSpec) -> tuple[Region, tuple[float, float]]:
        """Maps captured data window into original camera coordinates."""
        greater = max(spec.full_width * self.aspect, spec.full_height) * self.scale
        margin = (1.0 - self.scale) / 2.0
        return (
            Region(
                left=(spec.x - spec.full_x - margin * spec.full_width) * self.aspect / greater,
                bottom=(spec.full_y + spec.full_height - spec.y - spec.height - margin * spec.full_height) / greater,
                right=(spec.x + spec.width - spec.full_x - margin * spec.full_width) * self.aspect / greater,
                top=(spec.full_y + spec.full_height - spec.y - margin * spec.full_height) / greater,
            ),
            (spec.full_width * self.scale * self.aspect / greater, spec.full_height * self.scale / greater),
        )


class PreviewFrame(msgspec.Struct, frozen=True, kw_only=True):
    """Stored pixel size of a preview plane and the render framing it stands for."""

    size: tuple[int, int]
    framing: Framing


class Shell[R: Case](msgspec.Struct, frozen=True, kw_only=True):
    """Graph builder's record, entry input color, render layer, and group sockets by item key, toggled panels reading as their toggles."""

    record: R
    identity: Identity
    config: PyOpenColorIO.Config
    color: Expr
    render: Expr
    sockets: Mapping[Key, Socket]
    preview: Option[PreviewFrame] = Nothing

    def input(self, *path: Key | int | Socket | InterfacePanel) -> Expr:
        """Reads the group input of the declaring socket or toggled panel that ends a key path, a color or vector socket in its shape and every other socket as a value.

        Returns:
            Group input expression of the declaration.

        Raises:
            LookupError: For a path ending in no declaration the group holds, at graph build.
        """
        key = item_key(*(part.key if isinstance(part, Socket | InterfacePanel) else part for part in path))
        match path:
            case (*_, Socket() | InterfacePanel()) if key in self.sockets:
                return read(self.sockets[key], key)
            case _:
                raise LookupError(f"{key} names no input the group declares")

    @property
    def space(self) -> FrameSpace:
        """Camera frame of the entry input."""
        return FrameSpace.of(self.color, self.input(FRAME_ASPECT), self.input(FRAME_SCALE))

    def layer(self, socket: str) -> Expr:
        """Reads one Render Layers output of the effect's pass request layer."""
        return msgspec.structs.replace(self.render, socket=socket)

    def menu(self, *path: Key | int | Socket, cases: Mapping[MenuItem, Term], shape: Option[Shape] = Nothing) -> Expr:
        """Switches between case terms at the menu socket ending a key path, one case per item the socket declares in its order.

        Returns:
            Menu Switch expression over the cases in item order.

        Raises:
            LookupError: For a path ending in no menu socket the group holds, at graph build.
            ValueError: For cases holding other items than the socket declares, at graph build.
        """
        selector = self.input(*path)
        key = item_key(*(part.key if isinstance(part, Socket) else part for part in path))
        match self.sockets[key].items.default_value(None):
            case MenuItems(items=declared) if set(cases) == set(declared):
                return menu(selector, {item: cases[item] for item in declared}, shape=shape)
            case MenuItems(items=declared):
                raise ValueError(f"{key} cases {[item.name for item in cases]} differ from its items {[item.name for item in declared]}")
            case _:
                raise LookupError(f"{key} names no menu socket the group declares")


# --- [SERVICES] -------------------------------------------------------------------------


class WorkingSpace(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Entry color inputs kept in the working space across its changes, each scene stamped with the space its entry colors hold, the session's previous space read for a scene holding no stamp."""

    previous: Resource[deque[str]] = msgspec.field(default_factory=lambda: Resource(made=lambda _identity: nullcontext(deque(maxlen=1))))

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields the previous-space cell, its startup seed, and the deferred conversion a depsgraph update finding a stale stamp queues."""

        def start(stamp: Stored[str], scene: bpy.types.Scene, target: str) -> Outcome[str]:
            previous = identity.held[self.previous]
            return stamp.read(scene).map(lambda found: found.or_else(Block.of_seq(previous).try_head()).default_value(target))

        def recolored(stamp: Stored[str]) -> Outcome[Option[str]]:
            target = bpy.data.colorspace.working_space

            def converted(scene: bpy.types.Scene, cpu: PyOpenColorIO.CPUProcessor) -> Outcome[bool]:
                held = tuple(
                    (owner, np.asarray(descriptor.read(owner), dtype=np.float32))
                    for entry in scene.compositor_effects
                    for group in Option.of_optional(entry.node_group).to_list()
                    for identifier in input_sockets(group)
                    for owner in interface_input(entry, identifier).to_option().to_list()
                    for descriptor in Descriptor.resolve(owner, ("value",), bpy.types.FloatProperty).to_option().to_list()
                    if descriptor.prop.subtype == "COLOR"
                )
                for _, values in held:
                    cpu.applyRGB(values[:3])
                return accumulated(written(owner, ("value",), tuple(values.tolist())).map(Option.is_some) for owner, values in held).map(any)

            def each(scene: bpy.types.Scene, source: str) -> Outcome[bool]:
                return (
                    Ok(False)
                    if source == target
                    else processor(PyOpenColorIO.GetCurrentConfig(), PyOpenColorIO.ColorSpaceTransform(src=source, dst=target))
                    .map(PyOpenColorIO.Processor.getDefaultCPUProcessor)
                    .bind(partial(converted, scene))
                    .bind(lambda wrote: stamp.write(scene, target).map(lambda _: wrote))
                )

            counted = accumulated(start(stamp, scene, target).bind(partial(each, scene)) for scene in bpy.data.scenes)
            identity.held[self.previous].append(target)
            return counted.map(lambda wrote: Some("Convert Effect Colors") if any(wrote) else Nothing)

        with ExitStack() as scope:

            def built(stamp: Stored[str]) -> Acquisitions:
                later = scope.enter_context(identity.reports.deferred(lambda: recolored(stamp)))

                def seeded() -> Outcome[None]:
                    identity.held[self.previous].append(bpy.data.colorspace.working_space)
                    return Ok(None)

                def checked(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                    target = bpy.data.colorspace.working_space
                    return start(stamp, scene, target).map(lambda source: None if source == target else later())

                return (self.previous, Startup(body=seeded), Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.ID,), checked)))

            yield Stored.keyed(identity, "working_space", msgspec.msgpack.Decoder(str)).map(built)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [READS]


def grouped(entry: bpy.types.SceneCompositorEffect) -> Outcome[bpy.types.NodeTree]:
    """Returns the entry's group, absent before one is set."""
    return Option.of_optional(entry.node_group).to_result_with(partial(Fault.of, Reason.ABSENT, repr(entry), member="node_group"))


def keyed(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[tuple[bpy.types.NodeTree, Mapping[Key, str]]]:
    """Returns the entry's group with its live input socket identifiers by interface item key, panel keys at their toggles."""
    return Result[bpy.types.NodeTree, Faults].bind(grouped(entry), lambda group: Addressed.of(group, identity).map(lambda live: (group, live.sockets)))


def inputs_of(entry: bpy.types.SceneCompositorEffect) -> Outcome[Native]:
    """Resolves the entry's generated `properties.inputs`, absent without a group."""
    return struct_at(entry, ("properties", "inputs")).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(entry), member=rna(("properties", "inputs"))))


def interface_input(entry: bpy.types.SceneCompositorEffect, identifier: str) -> Outcome[Struct]:
    """Resolves live entry input by interface socket identifier."""
    return struct_at(entry, ("properties", "inputs", identifier), bpy.types.bpy_struct).to_result_with(partial(Fault.of, Reason.ABSENT, repr(entry), member=identifier))


def entry_input(identity: Identity, entry: bpy.types.SceneCompositorEffect, key: Key) -> Outcome[Struct]:
    """Resolves live entry input by interface item key after interface rebuilds."""
    return keyed(identity, entry).bind(lambda held: _addressed(entry, held[1], key))


def entry_value(identity: Identity, entry: bpy.types.SceneCompositorEffect, key: Key) -> Outcome[Value]:
    """Reads an entry input value by interface item key."""
    return entry_input(identity, entry, key).bind(lambda held: Descriptor.resolve(held, ("value",), bpy.types.Property).map(lambda descriptor: descriptor.read(held)))


def valued(snapshot: Snapshot) -> Mapping[Key, Value]:
    """Returns the input values of a capture by item key."""
    return {rna(key): owner.values["value",] for key, owner in snapshot.tree.rows if key and ("value",) in owner.values}


def _addressed(entry: bpy.types.SceneCompositorEffect, sockets: Mapping[Key, str], key: Key) -> Outcome[Struct]:
    """Resolves the live entry input at an interface item key among the group's addressed socket identifiers."""
    return Option.of_optional(sockets.get(key)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(entry), member=key)).bind(partial(interface_input, entry))


def _under(prefix: Key, key: Key) -> bool:
    """Whether an item key sits under a prefix, every key under the empty prefix."""
    return not prefix or key == prefix or key.startswith(f"{prefix}_")


# --- [CAPTURES]


def regrouped(snapshot: Snapshot, owners: Mapping[RnaPath, Owner[Value, RnaPath, RnaPath]]) -> Snapshot:
    """Returns snapshot holding owners in order as the children of its root owner."""
    return msgspec.structs.replace(snapshot, tree=Tree(roots=((),), rows=(((), Owner[Value, RnaPath, RnaPath](children=tuple(owners))), *owners.items())))


def captured(identity: Identity, entry: bpy.types.SceneCompositorEffect, prefix: Key = "") -> Outcome[Snapshot]:
    """Captures the entry's input values at the item keys under a prefix, every key while it is empty, each input owner named by its item key."""

    def renamed(snapshot: Snapshot, keys: Mapping[Key, str]) -> Snapshot:
        named: Mapping[RnaPath, RnaPath] = {(identifier,): (key,) for key, identifier in keys.items() if _under(prefix, key)}
        return regrouped(snapshot, {named[at]: owner for at, owner in snapshot.tree.rows if at in named})

    return paired(inputs_of(entry).bind(Snapshot.capture), keyed(identity, entry)).map(lambda pair: renamed(pair[0], pair[1][1]))


def rebased(values: Snapshot, source: Key, target: Key) -> Snapshot:
    """Returns captured input values with the item keys under `source` moved under `target`, the empty key holding every item key."""

    def moved(key: Key) -> Key:
        return item_key(*(part for part in (target, key.removeprefix(source).removeprefix("_")) if part)) if _under(source, key) else key

    return regrouped(values, {(moved(rna(key)),): owner for key, owner in values.tree.rows if key})


# --- [WRITES]


def restored(identity: Identity, entry: bpy.types.SceneCompositorEffect, values: Snapshot) -> Outcome[bool]:
    """Writes captured input values at the entry's item keys they name, answering whether any changed."""

    def placed(inputs: Native, keys: Mapping[Key, str]) -> Outcome[bool]:
        applied = regrouped(values, {(keys[rna(key)],): owner for key, owner in values.tree.rows if key and rna(key) in keys}).apply(inputs)
        return applied.outcome.map(lambda _: bool(applied.written))

    return paired(inputs_of(entry), keyed(identity, entry)).bind(lambda pair: placed(pair[0], pair[1][1]))


def defaulted(identity: Identity, entry: bpy.types.SceneCompositorEffect, prefix: Key = "") -> Outcome[bool]:
    """Writes each group socket's interface default into the entry's inputs at the item keys under a prefix, every key while it is empty, answering whether any changed."""

    def reset(group: bpy.types.NodeTree, keys: Mapping[Key, str]) -> Outcome[bool]:
        sockets = input_sockets(group)
        defaults = {
            identifier: descriptor.read(sockets[identifier])
            for key, identifier in keys.items()
            if _under(prefix, key) and identifier in sockets
            for descriptor in Descriptor.resolve(sockets[identifier], ("default_value",), bpy.types.Property).to_option().to_list()
        }
        return accumulated(interface_input(entry, identifier).bind(lambda held: written(held, ("value",), value)).map(Option.is_some) for identifier, value in defaults.items()).map(any)

    return keyed(identity, entry).bind(lambda held: reset(*held))


def entered(identity: Identity, entry: bpy.types.SceneCompositorEffect, values: Mapping[Key, Value]) -> Outcome[bool]:
    """Writes entry input values by interface key over one addressing pass, answering whether any changed."""
    return keyed(identity, entry).bind(
        lambda held: accumulated(_addressed(entry, held[1], key).bind(lambda owner: written(owner, ("value",), value)).map(Option.is_some) for key, value in values.items()).map(any)
    )


# --- [HOLDS]


def holding(identity: Identity, scene: bpy.types.Scene) -> Outcome[tuple[Stored[dict[Key, Snapshot]], dict[Key, Snapshot]]]:
    """Store of the scene values each holding effect captured before its first entry, by effect key, with the values the scene holds, empty before any effect holds."""
    return Stored.keyed(identity, "held", msgspec.msgpack.Decoder(dict[Key, Snapshot])).bind(lambda store: store.read(scene).map(lambda values: (store, values.default_value({}))))


def hold(identity: Identity, scene: bpy.types.Scene, key: Key, holds: Holds) -> Outcome[None]:
    """Captures the scene members an effect's holds assign and applies its scene holds once, then applies its camera holds, an effect holding nothing writing nothing."""

    def applied(store: Stored[dict[Key, Snapshot]], values: dict[Key, Snapshot]) -> Outcome[None]:
        first = Snapshot.capture(scene, Some(holds.scene.selection(scene))).bind(lambda prior: store.write(scene, values | {key: prior})).bind(lambda _: holds.scene.apply(scene).outcome)
        return (Ok(None) if key in values else first).bind(lambda _: cameras.required(holds.cameras, scene, identity))

    return Ok(None) if holds == Holds() else holding(identity, scene).bind(lambda pair: applied(*pair))


# --- [SHELL]


def read(socket: Socket, key: Key = "") -> Expr:
    """Reads the group input of a socket at its key, or at `key` for a socket nested under a prefix, a color or vector socket in its shape and every other socket as a value."""
    base = socket.base
    return group_input(key or socket.key, base if base in {bpy.types.NodeSocketColor, bpy.types.NodeSocketVector} else bpy.types.NodeSocketFloat)


# --- [COMPOSITION] ----------------------------------------------------------------------

WORKING_SPACE: Final = WorkingSpace()
FRAMING: Final = Derives[Case](
    of=lambda identity, scene, _entry, _record: CameraFrame.of(identity, scene).map(lambda frame: frame.values),
    sockets=FRAME,
    reads=(
        *((bpy.types.RenderSettings, bpy.types.RenderSettings.bl_rna.properties[name]) for name in ("pixel_aspect_x", "pixel_aspect_y", "resolution_x", "resolution_y", "resolution_percentage")),
        (bpy.types.UnitSettings, bpy.types.UnitSettings.bl_rna.properties["scale_length"]),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "FRAME",
    "FRAME_ASPECT",
    "FRAME_METERS",
    "FRAME_SCALE",
    "FRAMING",
    "WORKING_SPACE",
    "CameraFrame",
    "Derives",
    "Graph",
    "Holds",
    "PreviewFrame",
    "Shell",
    "WorkingSpace",
    "captured",
    "defaulted",
    "entered",
    "entry_input",
    "entry_value",
    "grouped",
    "hold",
    "holding",
    "inputs_of",
    "interface_input",
    "keyed",
    "read",
    "rebased",
    "regrouped",
    "restored",
    "valued",
]
