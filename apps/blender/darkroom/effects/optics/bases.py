"""Procedural texture bases cut to density over greater frame sides: their cases, the panel declaring them, their node form, and their evaluation at frame points."""

from collections.abc import Callable, Mapping
from math import pi, tau
from types import MemberDescriptorType
from typing import Annotated, Final, get_args

import bpy
from expression import Option, Some
import msgspec
import numpy as np
import numpy.typing as npt

from rasm_bpy.data import Identity, Key, Value
from rasm_bpy.nodes import decoded, Expr, Interface, InterfacePanel, item_key, math, MenuItem, MenuItems, menus, POSITION, sampled, Scalar, Socket, switched, Term, textures, vector, W, xyz
from rasm_bpy.render import chosen, GraphInputs, origin
from rasm_records import Case, Nothing, Outcome, paired, Positive

# --- [TYPES] ----------------------------------------------------------------------------

type Basis = Perlin | Voronoi | Wave | Gabor | Magic | White

# --- [MODELS] ---------------------------------------------------------------------------


class Perlin(Case, frozen=True, kw_only=True):
    """Fractal Perlin noise over greater frame sides cut to a density."""

    texture: textures.Noise = textures.Noise(noise_dimensions="4D", scale=1.0)
    stretch: Annotated[Positive, msgspec.Meta(description="Feature height over width")] = 1.0
    cut: textures.MapRange = textures.MapRange()


class Voronoi(Case, frozen=True, kw_only=True):
    """Cellular distances over greater frame sides cut to a density."""

    texture: textures.Voronoi = textures.Voronoi(voronoi_dimensions="4D", normalize=True, scale=10.0)
    cell_color: Annotated[bool, msgspec.Meta(description="Cell red sets tone, green scales density")] = False
    cut: textures.MapRange = textures.MapRange(interpolation_type="SMOOTHSTEP")

    @property
    def feature_cells(self) -> textures.Voronoi:
        """Cells whose `Color` belongs to the nearest feature, F1 Euclidean cells where the feature measures edges."""
        return msgspec.structs.replace(self.texture, feature="F1", distance="EUCLIDEAN") if self.texture.feature == "DISTANCE_TO_EDGE" else self.texture


class Wave(Case, frozen=True, kw_only=True):
    """Bands or rings of a periodic profile over greater frame sides cut to a density, the phase traveling one period per clock unit."""

    texture: textures.Wave = textures.Wave(rings_direction="Z", scale=pi)
    cut: textures.MapRange = textures.MapRange(interpolation_type="SMOOTHSTEP")


class Gabor(Case, frozen=True, kw_only=True):
    """Sparse windowed oscillations over greater frame sides cut to a density."""

    texture: textures.Gabor = textures.Gabor(gabor_type="3D", scale=10.0)
    cut: textures.MapRange = textures.MapRange(interpolation_type="SMOOTHSTEP")


class Magic(Case, frozen=True, kw_only=True):
    """Nested trigonometric turbulence over greater frame sides cut to a density."""

    texture: textures.Magic = textures.Magic(scale=10.0)
    cut: textures.MapRange = textures.MapRange(interpolation_type="SMOOTHSTEP")


class White(Case, frozen=True, kw_only=True):
    """Constant random values on lattice cells cut to a density."""

    texture: textures.WhiteNoise = textures.WhiteNoise(noise_dimensions="4D")
    spacing: Annotated[Positive, msgspec.Meta(description="Cell width over greater frame side")] = 0.1
    cut: textures.MapRange = textures.MapRange(interpolation_type="SMOOTHSTEP")


# --- [INTERFACES] -----------------------------------------------------------------------

_KINDS: Final[tuple[type[Basis], ...]] = get_args(Basis.__value__)  # ty: ignore[unsound-assignment]
_TEXTURE: Final = Socket(key="texture", kind=bpy.types.NodeSocketMenu, description="Texture case", items=MenuItems.of(Basis).items)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CASES]


def cased[T: msgspec.Struct](kinds: tuple[type[T], ...], current: T) -> tuple[T, ...]:
    """Every case of a union in union order, the current case in its own place and every other at its defaults."""
    return tuple(current if isinstance(current, kind) else kind() for kind in kinds)


# --- [PANELS]


def bases(key: Key, chosen: Basis, description: str, name: str | None = None) -> InterfacePanel:
    """Basis panel at `key`: its texture menu defaulting to the chosen case, then one panel per case holding its texture inputs, its own fields, and its density cut, every other case at its defaults."""

    def panel(case: Basis) -> InterfacePanel:
        own = InterfacePanel.of(item_key(type(case)), case, name=MenuItem.of(type(case)).name)
        texture = InterfacePanel.of(item_key(type(case), type(case).texture), Interface(inputs=case.texture.sockets)).inputs
        cut = InterfacePanel.of(item_key(type(case), type(case).cut), Interface(inputs=case.cut.sockets), name="Density", description="Density the texture value maps to")
        return msgspec.structs.replace(own, inputs=(*texture, *own.inputs), panels=(cut,))

    menu = msgspec.structs.replace(_TEXTURE, default_value=MenuItem.of(type(chosen)).name)
    return InterfacePanel.of(key, Interface(inputs=(menu,), panels=tuple(map(panel, cased(_KINDS, chosen)))), name=name, description=description)


# --- [NODES]


def textured(
    shell: GraphInputs, panel: InterfacePanel, chosen: Basis, point: Expr, time: Scalar, seed: Scalar, *, loss: Scalar = 0.0, streams: tuple[int, *tuple[int, ...]] = (0,)
) -> tuple[tuple[Expr, Expr], ...]:
    """Returns density and tone per stream of the basis panel's texture menu, every stream switched by the one menu.

    Args:
        shell: Entry inputs.
        panel: Basis panel `bases` declared, as the effect's interface holds it.
        chosen: Record basis holding the node members of its case, every other case at its defaults.
        point: Greater-side frame position.
        time: Signed evolution coordinate.
        seed: Seed operand.
        loss: Spatial loss subtracted before the cut.
        streams: Draw families in output order, from `tiles.stream`.

    Returns:
        Density and tone per stream.
    """

    def drawn(case: Basis) -> tuple[Expr, ...]:
        read = Some(lambda *path: shell.input(panel.socket(type(case), *path)))
        return tuple(part for stream in streams for part in _drawn(case, point, time, origin(seed, float(stream)), read, loss))

    first, *rest = menus(shell.input(panel, _TEXTURE), {MenuItem.of(type(case)): drawn(case) for case in cased(_KINDS, chosen)})
    columns = (first, *rest)
    return tuple(zip(columns[::2], columns[1::2]))


# --- [EVALUATION]


def selected(panel: InterfacePanel, current: Basis, values: Mapping[Key, Value]) -> Outcome[Basis]:
    """Returns the basis entry inputs hold, retaining current node settings for its selected case.

    Args:
        panel: Basis panel `bases` declared.
        current: Basis holding node settings for its case, other cases taking defaults.
        values: Entry input values by item key.

    Returns:
        `Ok` of the held basis. `Error` of `absent` for no texture value, `outside` for an item no case names, and `invalid` for a value outside its field.
    """

    def held(case: Basis) -> Outcome[Basis]:
        prefix = item_key(panel.key, type(case))
        return paired(decoded(case.texture, values, item_key(prefix, type(case).texture)), decoded(case.cut, values, item_key(prefix, type(case).cut))).bind(
            lambda parts: decoded(msgspec.structs.replace(case, texture=parts[0], cut=parts[1]), values, prefix)
        )

    return chosen(values, _KINDS, panel, _TEXTURE).map(lambda kind: current if isinstance(current, kind) else kind()).bind(held)


def evaluated(
    identity: Identity,
    field: Basis,
    points: npt.NDArray[np.float64],
    time: npt.NDArray[np.float64],
    *,
    seed: int,
    streams: tuple[int, ...],
    outputs: Callable[[Expr, Expr], tuple[Expr, ...]] = lambda density, tone: (density, tone),
) -> Outcome[tuple[npt.NDArray[np.float32], ...]]:
    """Returns selected basis outputs per stream in one native evaluation over frame points and evolution times.

    Args:
        identity: Extension owning the scratch evaluation tree.
        field: Basis as `selected` reads it.
        points: `(n, 2)` greater-side frame positions.
        time: `(n,)` evolution coordinates.
        seed: Seed.
        streams: Draw families from `tiles.stream`, in output order.
        outputs: Projection of density and tone expressions, retaining both by default.

    Returns:
        Native `(n,)` arrays in selected output order per stream, or scratch evaluation faults.
    """
    columns = {"position": np.column_stack((points, np.zeros(len(points)))), "w": time}
    fields = tuple(part for stream in streams for part in outputs(*_drawn(field, POSITION, W, origin(float(seed), float(stream)), Nothing)))
    return sampled(identity, columns, *fields)


def density_range(field: Basis) -> Outcome[tuple[float, float]]:
    """Encloses every finite native density, or returns a fault where its cut or cell-color support is unproved."""
    density = field.cut.bounds()
    match field:
        case Voronoi(cell_color=True):
            return paired(density, field.feature_cells.color_bounds()).map(lambda spans: (float((products := np.multiply.outer(*spans, dtype=np.float32)).min()), float(products.max())))
        case _:
            return density


# --- [FORMS]


def _drawn(case: Basis, point: Expr, time: Scalar, offset: Expr, read: Option[Callable[[*tuple[object, ...]], Expr]], loss: Scalar = 0.0) -> tuple[Expr, Expr]:
    """Density and tone of one case at a decorrelation offset, its sockets read through `read` where the graph holds them, the record's values where it reads `Nothing`."""
    kind = type(case)

    def operand(held: Scalar, *path: object) -> Scalar:
        return read.map(lambda reader: reader(*path)).default_value(held)

    def linked(field: object) -> Option[Callable[[MemberDescriptorType], Option[Term]]]:
        return read.map(lambda reader: lambda inner: Some(reader(field, inner)))

    raw: Expr
    match case:
        case Perlin(texture=noise, stretch=stretch):
            raw = textures.evolving(noise, (point + offset) / xyz(1.0, operand(stretch, Perlin.stretch), 1.0), time, linked=linked(Perlin.texture))
        case Voronoi(texture=cells):
            raw = textures.evolving(cells, point + offset, time, linked=linked(Voronoi.texture))
        case Wave(texture=wave):
            phase: MemberDescriptorType = textures.Wave.phase_offset  # ty: ignore[invalid-assignment]
            travel = math("MULTIPLY_ADD", time, -tau, operand(wave.phase_offset, Wave.texture, phase))
            raw = wave.node(point + vector("MULTIPLY", offset, (0.0, 0.0, 1.0)), linked=Some(lambda field: Some(travel) if field is phase else linked(Wave.texture).bind(lambda reader: reader(field))))
        case Gabor(texture=oscillations):
            raw = textures.evolving(oscillations, point + offset, time, linked=linked(Gabor.texture))
        case Magic(texture=turbulence):
            raw = textures.evolving(turbulence, point + offset, time, linked=linked(Magic.texture))
        case White(texture=noise, spacing=spacing):
            cell = operand(spacing, White.spacing)
            raw = textures.evolving(noise, vector("FLOOR", (point + offset) / xyz(cell, cell, 1.0)), math("FLOOR", time), linked=linked(White.texture))
    density = case.cut.node(raw - loss, linked=linked(kind.cut))
    match case:
        case Voronoi(cell_color=cell_color):
            color = textures.evolving(case.feature_cells, point + offset, time, socket="Color", socket_type=bpy.types.NodeSocketColor, linked=linked(Voronoi.texture))
            gate = operand(cell_color, Voronoi.cell_color)
            return switched(gate, density, density * color["Green"]), switched(gate, density, color["Red"])
        case _:
            return density, density


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Basis", "Gabor", "Magic", "Perlin", "Voronoi", "Wave", "White", "bases", "cased", "density_range", "evaluated", "selected", "textured"]
