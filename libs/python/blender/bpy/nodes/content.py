"""Node content the node's own members cannot state, each case reading, writing, and comparing itself on a live node: color ramps, curve maps, and loaded images."""

from collections.abc import Mapping
import os
from pathlib import Path
from typing import ClassVar, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.ids.images import ImageFrames, loaded, Tag
from blender.bpy.data.rna.descriptors import Descriptor, differing, Value, written
from blender.bpy.data.rna.paths import definition, RnaPath, struct_at
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.records.faults import accumulated, Fault, Outcome
from blender.records.fields import Unit
from blender.records.trees import Owner, Tree

# --- [TYPES] ----------------------------------------------------------------------------

type Points = tuple[tuple[float, float], ...]

# --- [MODELS] ---------------------------------------------------------------------------

# --- [RAMPS]


class Stop(msgspec.Struct, frozen=True, kw_only=True):
    """`ColorRampElement` position and color, written at alpha 1."""

    position: Unit
    color: tuple[float, float, float]


class Ramp(msgspec.Struct, frozen=True, kw_only=True):
    """`ColorRamp` elements and interpolation members holding host identifiers, the host refusing a bad identifier or stop count at the write."""

    stops: tuple[Stop, ...] = (Stop(position=0.0, color=(0.0, 0.0, 0.0)), Stop(position=1.0, color=(1.0, 1.0, 1.0)))
    interpolation: str = "LINEAR"
    color_mode: str = "RGB"
    hue_interpolation: str = "NEAR"

    nested: ClassVar[tuple[()]] = ()

    @classmethod
    def read(cls, ramp: bpy.types.ColorRamp) -> Self:
        """Reads elements in host order through `foreach_get` with the ramp's interpolation members."""
        positions, colors = np.empty(len(ramp.elements), dtype=np.float32), np.empty((len(ramp.elements), 4), dtype=np.float32)
        ramp.elements.foreach_get("position", positions)
        ramp.elements.foreach_get("color", colors.reshape(-1))
        return cls(
            stops=tuple(Stop(position=position, color=(red, green, blue)) for position, (red, green, blue, _) in zip(positions.tolist(), colors.tolist())),
            interpolation=ramp.interpolation,
            color_mode=ramp.color_mode,
            hue_interpolation=ramp.hue_interpolation,
        )

    @property
    def keyed(self) -> object:
        """Builtin form the lowering digest reads."""
        return msgspec.to_builtins(self)

    @property
    def table(self) -> npt.NDArray[np.float32]:
        """Stops as float32 rows of position and RGB in position order."""
        return np.array(sorted((stop.position, *stop.color) for stop in self.stops), dtype=np.float32)

    def applied(self, ramp: bpy.types.ColorRamp) -> Outcome[None]:
        """Writes members and elements by position onto a host ramp, adding and removing elements to the stop count."""
        elements: tuple[RnaPath, ...] = tuple(("elements", index) for index in range(len(self.stops)))
        tree: Tree[Value, RnaPath, RnaPath] = Tree(
            roots=((),),
            rows=(
                ((), Owner(rows=((("interpolation",), self.interpolation), (("color_mode",), self.color_mode), (("hue_interpolation",), self.hue_interpolation)), children=(("elements",),))),
                (("elements",), Owner(children=elements)),
                *((key, Owner[Value, RnaPath, RnaPath](rows=((("position",), position), (("color",), (*color, 1.0))))) for key, (position, *color) in zip(elements, self.table.tolist())),
            ),
        )
        return Snapshot(struct=bpy.types.ColorRamp.__name__, tree=tree).apply(ramp).outcome

    def holds(self, live: bpy.types.Node, _identity: Identity, /) -> Outcome[bool]:
        """Whether the node's ramp holds these members and, at float32 storage, these stops."""

        def held(ramp: bpy.types.ColorRamp) -> bool:
            now = self.read(ramp)
            return (now.interpolation, now.color_mode, now.hue_interpolation) == (self.interpolation, self.color_mode, self.hue_interpolation) and np.array_equal(now.table, self.table)

        return _ramp(live).map(held)

    def write(self, live: bpy.types.Node, _identity: Identity, /) -> Outcome[None]:
        """Writes the ramp onto the node's `color_ramp`."""
        return _ramp(live).bind(self.applied)

    @staticmethod
    def sockets(_live: bpy.types.Node, _identity: Identity, _slot: str, /) -> Mapping[str, str]:
        """No item sockets."""
        return {}


# --- [CURVES]


class CurveAxes(msgspec.Struct, frozen=True, kw_only=True):
    """Record curve placed in its node's own axes, the shown height `scale * y + slope * x + offset`."""

    scale: float = 1.0
    slope: float = 0.0
    offset: float = 0.0

    def shown(self, x: float, y: float) -> float:
        """Returns node height of record point."""
        return self.scale * y + self.slope * x + self.offset

    def held(self, x: float, y: float) -> float:
        """Returns record height of node point."""
        return (y - self.slope * x - self.offset) / self.scale


class CurvePoints(msgspec.Struct, frozen=True, kw_only=True):
    """Curve map points with view bounds and evaluation clipping, `handle` a `CurveMapPoint.handle_type` identifier."""

    maps: tuple[Points, *tuple[Points, ...]]
    clip: Option[tuple[float, float, float, float]] = Nothing
    handle: str = "VECTOR"
    clipped: bool = False

    nested: ClassVar[tuple[()]] = ()

    @property
    def keyed(self) -> object:
        """Builtin form the lowering digest reads."""
        return msgspec.to_builtins(self)

    @property
    def tree(self) -> Tree[Value, RnaPath, RnaPath]:
        """Curve point settings by node RNA path, collections owning items as `Snapshot.capture` reads them."""
        curves: Mapping[RnaPath, Points] = {("mapping", "curves", index): points for index, points in enumerate(self.maps)}
        keys: Mapping[RnaPath, tuple[RnaPath, ...]] = {curve: tuple((*curve, "points", index) for index in range(len(points))) for curve, points in curves.items()}
        bounds = self.clip.map(lambda span: tuple(zip((("clip_min_x",), ("clip_min_y",), ("clip_max_x",), ("clip_max_y",)), span))).default_value(())
        return Tree(
            roots=(("mapping",),),
            rows=(
                (("mapping",), Owner(rows=((("use_clip",), self.clipped), (("extend",), "EXTRAPOLATED"), *bounds), children=(("mapping", "curves"),))),
                (("mapping", "curves"), Owner(children=tuple(curves))),
                *((curve, Owner[Value, RnaPath, RnaPath](children=((*curve, "points"),))) for curve in curves),
                *(((*curve, "points"), Owner[Value, RnaPath, RnaPath](children=keys[curve])) for curve in curves),
                *((key, Owner[Value, RnaPath, RnaPath](rows=((("location",), point), (("handle_type",), self.handle)))) for curve, points in curves.items() for key, point in zip(keys[curve], points)),
            ),
        )

    def holds(self, live: bpy.types.Node, _identity: Identity, /) -> Outcome[bool]:
        """Whether the node's mapping holds these settings and points."""
        desired = self.tree
        return (
            accumulated(Descriptor.resolve(type(live), root, bpy.types.PointerProperty) for root in desired.roots)
            .bind(lambda roots: Snapshot.capture(live, Some(roots)))
            .map(
                lambda held: all(
                    key in held.tree.owners and held.tree.owners[key].children == owner.children and all(not differing(struct, owner.values) for struct in struct_at(live, key).to_list())
                    for key, owner in desired.owners.items()
                )
            )
        )

    def write(self, live: bpy.types.Node, _identity: Identity, /) -> Outcome[None]:
        """Writes the points onto the node's mapping, then resets its view and updates its evaluation."""

        def refreshed(_: object) -> None:
            for curves in struct_at(live, ("mapping",), bpy.types.CurveMapping).to_list():
                curves.reset_view()
                curves.update()

        return Snapshot(struct=definition(live).identifier, tree=self.tree).apply(live).outcome.map(refreshed)

    @staticmethod
    def sockets(_live: bpy.types.Node, _identity: Identity, _slot: str, /) -> Mapping[str, str]:
        """No item sockets."""
        return {}


# --- [IMAGES]


class Loaded(msgspec.Struct, frozen=True, kw_only=True):
    """Image file or sequence a node shows."""

    path: Path
    tag: Tag = Tag()
    frames: Option[ImageFrames] = Nothing

    nested: ClassVar[tuple[()]] = ()

    @property
    def keyed(self) -> object:
        """Builtin form the lowering digest reads."""
        return (str(self.path), msgspec.to_builtins(self.tag), msgspec.to_builtins(self.frames))

    def holds(self, live: bpy.types.Node, _identity: Identity, /) -> Outcome[bool]:
        """Whether the node shows this file, both paths normalized as `bpy_extras.io_utils.path_reference` compares them, with this tag and frame window."""

        def shown(image: bpy.types.Image) -> bool:
            return Path(os.path.normpath(bpy.path.abspath(image.filepath, library=image.library))) == Path(os.path.normpath(self.path)) and (image.source == "SEQUENCE") == self.tag.sequence

        return (
            self.frames
            .map(lambda want: _framed(live).map(want.holds))
            .default_value(Ok(True))
            .map(lambda framed: framed and struct_at(live, ("image",), bpy.types.Image).map(shown).default_value(False))
        )

    def write(self, live: bpy.types.Node, _identity: Identity, /) -> Outcome[None]:
        """Loads or reuses the file image, points the node at it, and writes the frame window."""
        return (
            loaded(str(self.path), self.tag).bind(lambda image: written(live, ("image",), image)).bind(lambda _: self.frames.map(lambda want: _framed(live).bind(want.applied)).default_value(Ok(None)))
        )

    @staticmethod
    def sockets(_live: bpy.types.Node, _identity: Identity, _slot: str, /) -> Mapping[str, str]:
        """No item sockets."""
        return {}


# --- [OPERATIONS] -----------------------------------------------------------------------


def _ramp(holder: bpy.types.Node | bpy.types.Texture) -> Outcome[bpy.types.ColorRamp]:
    """Holder's `color_ramp`, else `absent` fault naming it."""
    return struct_at(holder, ("color_ramp",), bpy.types.ColorRamp).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(holder), member="color_ramp"))


def _framed(live: bpy.types.Node) -> Outcome[bpy.types.ImageUser | bpy.types.CompositorNodeImage]:
    """Frame window holder, compositor image node itself or shader image node `image_user`."""
    match live:
        case bpy.types.CompositorNodeImage():
            return Ok(live)
        case bpy.types.ShaderNodeTexImage() | bpy.types.ShaderNodeTexEnvironment():
            return Ok(live.image_user)
        case _:
            return Error(Fault.of(BlenderReason.ABSENT, repr(live), member="image_user"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CurveAxes", "CurvePoints", "Loaded", "Points", "Ramp", "Stop"]
