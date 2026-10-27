"""Color ramp records read from, written to, and evaluated by the host `ColorRamp`."""

from collections.abc import Iterable
from contextlib import ExitStack
from typing import Annotated, Literal, Self

import bpy
from expression import Option, Result
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.rna.descriptors import RnaPath, Value, written
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Faults, Outcome
from blender.records.fields import Unit
from blender.records.trees import Owner, Tree

# --- [MODELS] ---------------------------------------------------------------------------


class Stop(msgspec.Struct, frozen=True, kw_only=True):
    """`ColorRampElement` position and color, written at alpha 1."""

    position: Unit
    color: tuple[float, float, float]


class Ramp(msgspec.Struct, frozen=True, kw_only=True):
    """`ColorRamp` elements and interpolation members under their host names and values."""

    stops: Annotated[tuple[Stop, ...], msgspec.Meta(min_length=1, max_length=32)] = (Stop(position=0.0, color=(0.0, 0.0, 0.0)), Stop(position=1.0, color=(1.0, 1.0, 1.0)))
    interpolation: Literal["EASE", "CARDINAL", "LINEAR", "B_SPLINE", "CONSTANT"] = "LINEAR"
    color_mode: Literal["RGB", "HSV", "HSL"] = "RGB"
    hue_interpolation: Literal["NEAR", "FAR", "CW", "CCW"] = "NEAR"

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

    def write(self, ramp: bpy.types.ColorRamp) -> Outcome[None]:
        """Writes members and elements by position, adding and removing elements to the stop count."""
        elements: tuple[RnaPath, ...] = tuple(("elements", index) for index in range(len(self.stops)))
        tree: Tree[Value, RnaPath] = Tree(
            roots=((),),
            rows=(
                ((), Owner(values={"interpolation": self.interpolation, "color_mode": self.color_mode, "hue_interpolation": self.hue_interpolation}, children=(("elements",),))),
                (("elements",), Owner(values={}, children=elements)),
                *((key, Owner[Value, RnaPath](values={"position": position, "color": (*color, 1.0)})) for key, (position, *color) in zip(elements, self.table.tolist())),
            ),
        )
        return Snapshot(struct=bpy.types.ColorRamp.__name__, tree=tree).apply(ramp).outcome

    def holds(self, ramp: bpy.types.ColorRamp) -> bool:
        """Whether the host ramp holds these members and, at float32 storage, these stops."""
        live = self.read(ramp)
        return (live.interpolation, live.color_mode, live.hue_interpolation) == (self.interpolation, self.color_mode, self.hue_interpolation) and np.array_equal(live.table, self.table)

    def evaluated(self, positions: Iterable[float]) -> Outcome[npt.NDArray[np.float64]]:
        """Evaluates RGBA at each position through `ColorRamp.evaluate` of a scratch blend texture removed on every path."""

        def colors(texture: bpy.types.Texture) -> Outcome[npt.NDArray[np.float64]]:
            with ExitStack() as scratch:
                scratch.callback(bpy.data.textures.remove, texture)
                return (
                    written(texture, "use_color_ramp", True)
                    .bind(lambda _: Option.of_optional(texture.color_ramp).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(texture), member="color_ramp"),)))
                    .bind(
                        lambda ramp: self.write(ramp).map(
                            lambda _: np.array([msgspec.convert(ramp.evaluate(position), tuple[float, float, float, float]) for position in positions], dtype=np.float64).reshape(-1, 4)
                        )
                    )
                )

        return Result[bpy.types.Texture, Faults].bind(
            catch(bpy.data.textures.new, exception=RuntimeError)(type(self).__qualname__, "BLEND").map_error(
                lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(bpy.data.textures), cause=error),)
            ),
            colors,
        )

    @property
    def table(self) -> npt.NDArray[np.float32]:
        """Stops as float32 rows of position and RGB in position order."""
        return np.array(sorted((stop.position, *stop.color) for stop in self.stops), dtype=np.float32)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Ramp", "Stop"]
