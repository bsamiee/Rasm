"""Entry inputs by declaration path with their typed reads, captures, restores, defaults, and writes, hidden inputs derived from scene values, the camera frame, the shell an effect graph reads and the result it answers, entry colors kept in the working space, and scene holds."""

from collections import deque
from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack, nullcontext
from functools import partial
from typing import Final, Self

import bpy
from expression import Option, Some
from expression.collections import Block
import msgspec
import numpy as np
import OpenImageIO
import PyOpenColorIO
from typing_extensions import TypeForm

from rasm_bpy.data._extension.events import updated
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions, Handler, Resource, Startup
from rasm_bpy.data._rna.descriptors import converted_value, Descriptor, Value, written
from rasm_bpy.data._rna.paths import Native, rna, RnaPath, Struct, struct_at
from rasm_bpy.data._rna.snapshots import Snapshot
from rasm_bpy.nodes._addressing import interface_sockets, InterfaceKeys
from rasm_bpy.nodes._desired import Choice, item_key, MenuItem
from rasm_bpy.nodes._graphs import Expr, group_input, math, menu, Term, vector, xyz
from rasm_bpy.nodes._interfaces import InterfacePanel, Socket
from rasm_bpy.render._compositor.coordinates import FrameSpace, PixelGrid
from rasm_bpy.render._outputs.framing import FrameSize, Framing
from rasm_bpy.scene import cameras
from rasm_bpy.settings._attachments import Stored
from rasm_imaging import called, processor, Raster, Region
from rasm_records import accumulated, Fault, Nothing, Ok, Outcome, Owner, paired, Reason, Tree, typed

# --- [TYPES] ----------------------------------------------------------------------------

type Step = Key | int | Socket | InterfacePanel

# --- [CONSTANTS] ------------------------------------------------------------------------

FRAME_ASPECT: Final = Socket(key="frame_aspect", kind=bpy.types.NodeSocketFloat, description="Pixel aspect", hide_in_modifier=True, default_value=1.0)
FRAME_SCALE: Final = Socket(key="frame_scale", kind=bpy.types.NodeSocketFloat, description="Camera share of overscan", hide_in_modifier=True, default_value=1.0)
FRAME_METERS: Final = Socket(key="frame_meters", kind=bpy.types.NodeSocketFloat, description="Meters per Blender unit", hide_in_modifier=True, default_value=1.0)
FRAME: Final = (FRAME_ASPECT, FRAME_SCALE, FRAME_METERS)
_INPUTS: Final = "SceneCompositorEffectInterfaceInputs"

# --- [MODELS] ---------------------------------------------------------------------------


class DerivedInputs(msgspec.Struct, frozen=True, kw_only=True):
    """Hidden sockets written from scene values by key, rewritten when one of `reads` changes."""

    of: Callable[[Identity, bpy.types.Scene, bpy.types.SceneCompositorEffect], Outcome[Mapping[Key, Value]]]
    sockets: tuple[Socket, ...] = ()
    reads: tuple[tuple[type[Struct], bpy.types.Property], ...] = ()


class Holds(msgspec.Struct, frozen=True, kw_only=True):
    """Scene and camera values held while an entry exists, the scene values stated or derived from the scene, the prior scene values restored once the last entry goes."""

    scene: Snapshot | Callable[[bpy.types.Scene], Outcome[Snapshot]] = msgspec.field(default_factory=lambda: Snapshot.of(bpy.types.Scene, {}))
    cameras: Snapshot = msgspec.field(default_factory=lambda: Snapshot.of(bpy.types.Camera, {}))

    def held(self, scene: bpy.types.Scene, /) -> Outcome[Snapshot]:
        """Returns the scene values held over `scene`, stated or derived."""
        return Ok(self.scene) if isinstance(self.scene, Snapshot) else self.scene(scene)


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
        return entry_values(identity, entry, (socket.key for socket in FRAME), partial(typed, float)).map(
            lambda read: cls(aspect=read[FRAME_ASPECT.key], scale=read[FRAME_SCALE.key], meters=read[FRAME_METERS.key])
        )

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
    """Preview plane's native array size (height, width) and represented render framing."""

    size: tuple[int, int]
    framing: Framing


class GraphInputs(msgspec.Struct, frozen=True, kw_only=True):
    """Graph builder's entry input color, render layer, and group sockets by item key, toggled panels reading as their toggles."""

    identity: Identity
    config: PyOpenColorIO.Config
    color: Expr
    render: Expr
    sockets: Mapping[Key, Socket]
    camera: Callable[[], Outcome[Raster]]
    rasters: Option[tuple[Raster, Raster]] = Nothing
    preview: Option[PreviewFrame] = Nothing

    def input(self, *path: Step) -> Expr:
        """Reads the declared group input at a key path in its native socket shape."""
        key = path_key(*path)
        return socket_input(self.sockets[key], key)

    @property
    def space(self) -> FrameSpace:
        """Camera frame of the entry input."""
        if self.rasters.is_none():
            return FrameSpace.of(self.color, self.input(FRAME_ASPECT), self.input(FRAME_SCALE))
        data, camera = self.rasters.some
        grid = PixelGrid.of(self.color)
        width, height = data.bounds.right - data.bounds.left, data.bounds.top - data.bounds.bottom
        extent = xyz(camera.bounds.right - camera.bounds.left, camera.bounds.top - camera.bounds.bottom)
        point = vector("MULTIPLY_ADD", grid.base, (width, height, 0.0), (data.bounds.left - camera.bounds.left, data.bounds.bottom - camera.bounds.bottom, 0.0))
        pitch = data.step
        return FrameSpace(
            point=point,
            extent=extent,
            greater=math("DIVIDE", 1.0, float(pitch[1])),
            fraction=vector("DIVIDE", point, extent),
            scale=xyz(1.0 / float(pitch[0]), 1.0 / float(pitch[1])),
            uv_scale=vector("DIVIDE", extent, (width, height, 1.0)),
            margin=xyz((camera.bounds.left - data.bounds.left) / width, (camera.bounds.bottom - data.bounds.bottom) / height, 1.0),
        )

    def layer(self, socket: str, socket_type: type[bpy.types.NodeSocket] = bpy.types.NodeSocketColor) -> Expr:
        """Reads one Render Layers output of the effect's pass request layer, in the shape of a color unless stated."""
        return self.render.at(socket, socket_type)

    def menu(self, *path: Step, cases: Mapping[MenuItem, Term], socket_type: Option[type[bpy.types.NodeSocket]] = Nothing) -> Expr:
        """Switches between terms in the declared menu order."""
        selector, key = self.input(*path), path_key(*path)
        return menu(selector, {item: cases[item] for item in self.sockets[key].items}, socket_type=socket_type)


class ResultOverBase(msgspec.Struct, frozen=True, kw_only=True):
    """Graph result with the base a blended effect mixes it back over in place of the entry input, both in the effect's operation space."""

    result: Expr
    base: Expr


# --- [SERVICES] -------------------------------------------------------------------------


class _WorkingSpace(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Entry color inputs kept in the working space across its changes, each scene stamped with the space its entry colors hold, the session's previous space read for a scene holding no stamp."""

    previous: Resource[deque[str]] = msgspec.field(default_factory=lambda: Resource(made=lambda _identity: nullcontext(deque(maxlen=1))))

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Acquisitions]:
        """Yields the previous-space cell, its startup seed, and the deferred conversion a depsgraph update finding a stale stamp queues."""

        def start(stamp: Stored[str], scene: bpy.types.Scene, target: str) -> Outcome[str]:
            previous = self.previous[identity]
            return stamp.read(scene).map(lambda found: found.or_else(Block.of_seq(previous).try_head()).default_value(target))

        def recolored(stamp: Stored[str]) -> Outcome[Option[str]]:
            target = bpy.data.colorspace.working_space

            def converted(scene: bpy.types.Scene, cpu: PyOpenColorIO.CPUProcessor) -> Outcome[bool]:
                held = tuple(
                    (owner, np.asarray(descriptor.read(owner), dtype=np.float32))
                    for entry in scene.compositor_effects
                    for group in Option.of_optional(entry.node_group).to_list()
                    for socket in interface_sockets(group)
                    for owner in interface_input(entry, socket.identifier).to_option().to_list()
                    for descriptor in Descriptor.resolve(owner, ("value",), bpy.types.FloatProperty).to_option().to_list()
                    if descriptor.prop.subtype == "COLOR"
                )
                for _, values in held:
                    cpu.applyRGB(values[:3])
                return accumulated(written(owner, ("value",), tuple(values.tolist())).map(Option.is_some) for owner, values in held).map(any)

            def each(scene: bpy.types.Scene, source: str) -> Outcome[bool]:
                return (
                    Ok(value=False)
                    if source == target
                    else called(PyOpenColorIO.GetCurrentConfig, scene.name)
                    .bind(lambda config: processor(config, PyOpenColorIO.ColorSpaceTransform(src=source, dst=target)))
                    .map(PyOpenColorIO.Processor.getDefaultCPUProcessor)
                    .bind(partial(converted, scene))
                    .bind(lambda wrote: stamp.write(scene, target).map(lambda _: wrote))
                )

            counted = accumulated(start(stamp, scene, target).bind(partial(each, scene)) for scene in bpy.data.scenes)
            self.previous[identity].append(target)
            return counted.map(lambda wrote: Some("Convert Effect Colors") if any(wrote) else Nothing)

        with ExitStack() as scope:

            def built(stamp: Stored[str]) -> Acquisitions:
                later = scope.enter_context(identity.reports.deferred(lambda: recolored(stamp)))

                def seeded() -> Outcome[None]:
                    self.previous[identity].append(bpy.data.colorspace.working_space)
                    return Ok(None)

                def checked(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> Outcome[None]:
                    target = bpy.data.colorspace.working_space
                    return start(stamp, scene, target).map(lambda source: None if source == target else later())

                return (self.previous, Startup(body=seeded), Handler(handlers=bpy.app.handlers.depsgraph_update_post, body=partial(updated, (bpy.types.ID,), checked)))

            yield built(Stored.keyed(identity, "working_space", msgspec.msgpack.Decoder(str)))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [READS]


def grouped(entry: bpy.types.SceneCompositorEffect) -> Outcome[bpy.types.NodeTree]:
    """Returns the entry's group, absent before one is set."""
    return Option.of_optional(entry.node_group).to_result_with(partial(Fault.of, Reason.ABSENT, repr(entry), member="node_group"))


def keyed(identity: Identity, entry: bpy.types.SceneCompositorEffect) -> Outcome[tuple[bpy.types.NodeTree, Mapping[Key, str]]]:
    """Returns the entry's group with its live input socket identifiers by interface item key, panel keys at their toggles."""
    return grouped(entry).map(lambda group: (group, InterfaceKeys.of(group, identity).sockets))


def inputs_of(entry: bpy.types.SceneCompositorEffect) -> Outcome[Native]:
    """Resolves the entry's generated `properties.inputs`, absent without a group."""
    return struct_at(entry, ("properties", "inputs")).to_result_with(lambda: Fault.of(Reason.ABSENT, repr(entry), member=rna(("properties", "inputs"))))


def interface_input(entry: bpy.types.SceneCompositorEffect, identifier: str) -> Outcome[Struct]:
    """Resolves live entry input by interface socket identifier."""
    found: Option[Struct] = struct_at(entry, ("properties", "inputs", identifier), bpy.types.bpy_struct)
    return found.to_result_with(partial(Fault.of, Reason.ABSENT, repr(entry), member=identifier))


def path_key(*path: Step) -> Key:
    """Joins a declaration path into the item key its group input sits at, sockets and panels by their keys."""
    return item_key(*(part.key if isinstance(part, Socket | InterfacePanel) else part for part in path))


def entry_input(identity: Identity, entry: bpy.types.SceneCompositorEffect, *path: Step) -> Outcome[Struct]:
    """Resolves the live entry input a declaration path ends at after interface rebuilds."""
    return keyed(identity, entry).bind(lambda held: addressed(entry, held[1], path_key(*path)))


def entry_value(identity: Identity, entry: bpy.types.SceneCompositorEffect, *path: Step) -> Outcome[Value]:
    """Reads the entry input value a declaration path ends at."""
    return entry_input(identity, entry, *path).bind(lambda held: Descriptor.resolve(held, ("value",), bpy.types.Property).map(lambda descriptor: descriptor.read(held)))


def entry_values[T](identity: Identity, entry: bpy.types.SceneCompositorEffect, keys: Iterable[Key], read: Callable[[Key, Value], Outcome[T]]) -> Outcome[Mapping[Key, T]]:
    """Reads and decodes requested keys in one interface pass, accumulating each key's faults in request order."""
    requested = tuple(keys)
    return (
        Ok(dict[Key, T]())
        if not requested
        else keyed(identity, entry).bind(
            lambda held: accumulated(
                addressed(entry, held[1], key).bind(
                    lambda owner: Descriptor.resolve(owner, ("value",), bpy.types.Property).bind(lambda descriptor: read(key, descriptor.read(owner)).map(lambda value: (key, value)))
                )
                for key in requested
            ).map(dict)
        )
    )


def value[T](values: Mapping[Key, Value], form: TypeForm[T], /, *path: Step) -> Outcome[T]:
    """Reads the input value a declaration path ends at from values by item key, converted to form.

    Returns:
        `Ok` of the converted value. `Error` of `absent` for a path no value sits at, and of `outside` for a value outside form.
    """
    key = path_key(*path)
    return Option.of_optional(values.get(key)).to_result_with(partial(Fault.of, Reason.ABSENT, "input values", member=key)).bind(lambda held: converted_value(held, form, key))


def toggled[T](values: Mapping[Key, Value], read: Callable[[], Outcome[T]], /, *path: Step) -> Outcome[Option[T]]:
    """Reads what the panel toggle a declaration path ends at gates: the read's value under an enabled toggle, `Nothing` under a disabled one with the read never run."""
    return value(values, bool, *path).bind(lambda enabled: read().map(Some) if enabled else Ok(Nothing))


def chosen[C: Choice](values: Mapping[Key, Value], choices: Iterable[C], /, *path: Step) -> Outcome[C]:
    """Reads the menu value a declaration path ends at as the enum member or case class whose item names it.

    Returns:
        `Ok` of the choice. `Error` of `absent` and `invalid` as `value` reads them, and of `outside` for an item no choice names.
    """
    held = tuple(choices)
    return value(values, str, *path).bind(
        lambda name: MenuItem.member(held, name).to_result_with(partial(Fault.of, Reason.OUTSIDE, path_key(*path), name, member=", ".join(MenuItem.of(choice).name for choice in held)))
    )


def valued(snapshot: Snapshot) -> Mapping[Key, Value]:
    """Returns the input values of a capture by item key."""
    return {rna(key): value for key, owner in snapshot.tree.rows if key for name, value in owner.rows if name == ("value",)}


def addressed(entry: bpy.types.SceneCompositorEffect, sockets: Mapping[Key, str], key: Key) -> Outcome[Struct]:
    """Resolves the live entry input at an interface item key among the group's addressed socket identifiers."""
    return Option.of_optional(sockets.get(key)).to_result_with(partial(Fault.of, Reason.ABSENT, repr(entry), member=key)).bind(partial(interface_input, entry))


def _under(prefix: Key, key: Key) -> bool:
    """Whether an item key sits under a prefix, every key under the empty prefix."""
    return not prefix or key == prefix or key.startswith(f"{prefix}_")


# --- [CAPTURE]


def regrouped(snapshot: Snapshot, owners: Mapping[RnaPath, Owner[Value, RnaPath, RnaPath]]) -> Snapshot:
    """Returns snapshot holding owners in order as the children of its root owner."""
    return msgspec.structs.replace(snapshot, tree=Tree(roots=((),), rows=(((), Owner[Value, RnaPath, RnaPath](children=tuple(owners))), *owners.items())))


def snapshot(values: Mapping[Key, Value], /) -> Snapshot:
    """Returns the capture holding input values by item key, the inverse of `valued`, a preset or look stating its values without an entry."""
    return regrouped(Snapshot(struct=_INPUTS, tree=Tree(roots=(), rows=())), {(key,): Owner[Value, RnaPath, RnaPath](rows=((("value",), held),)) for key, held in values.items()})


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
        sockets = {socket.identifier: socket for socket in interface_sockets(group)}
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
        lambda held: accumulated(addressed(entry, held[1], key).bind(lambda owner: written(owner, ("value",), value)).map(Option.is_some) for key, value in values.items()).map(any)
    )


# --- [HOLDS]


def holding(identity: Identity, scene: bpy.types.Scene) -> Outcome[tuple[Stored[dict[Key, Snapshot]], dict[Key, Snapshot]]]:
    """Store of the scene values each holding effect captured before its first entry, by effect key, with the values the scene holds, empty before any effect holds."""
    store = Stored.keyed(identity, "held", msgspec.msgpack.Decoder(dict[Key, Snapshot]))
    return store.read(scene).map(lambda values: (store, values.default_value({})))


def hold(identity: Identity, scene: bpy.types.Scene, key: Key, holds: Holds) -> Outcome[None]:
    """Captures the scene members an effect's holds assign and applies its scene holds once, then applies its camera holds, an effect holding nothing writing nothing."""

    def applied(store: Stored[dict[Key, Snapshot]], values: dict[Key, Snapshot]) -> Outcome[None]:
        return (
            Ok(None)
            if key in values
            else holds.held(scene).bind(
                lambda wanted: Snapshot.capture(scene, Some(wanted.selection(scene))).bind(lambda prior: store.write(scene, values | {key: prior})).bind(lambda _: wanted.apply(scene).outcome)
            )
        ).bind(lambda _: cameras.required(holds.cameras, scene, identity))

    return Ok(None) if holds == Holds() else holding(identity, scene).bind(lambda pair: applied(*pair))


# --- [SHELL]


def socket_input(socket: Socket, key: Key = "") -> Expr:
    """Reads the declared socket type at its key or a prefixed key."""
    return group_input(key or socket.key, socket.base)


# --- [COMPOSITION] ----------------------------------------------------------------------

WORKING_SPACE: Final = _WorkingSpace()
FRAMING: Final = DerivedInputs(
    of=lambda identity, scene, _entry: CameraFrame.of(identity, scene).map(lambda frame: frame.values),
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
    "DerivedInputs",
    "GraphInputs",
    "Holds",
    "PreviewFrame",
    "ResultOverBase",
    "Step",
    "addressed",
    "captured",
    "chosen",
    "defaulted",
    "entered",
    "entry_input",
    "entry_value",
    "entry_values",
    "grouped",
    "hold",
    "holding",
    "inputs_of",
    "interface_input",
    "keyed",
    "path_key",
    "rebased",
    "regrouped",
    "restored",
    "snapshot",
    "socket_input",
    "toggled",
    "value",
    "valued",
]
