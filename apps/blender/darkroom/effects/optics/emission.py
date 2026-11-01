"""Light a generator adds over its coverage: tint, blackbody, and ramp spectra by field tone, the panels declaring them, their node form, and their scene-linear evaluation."""

from collections.abc import Mapping
from itertools import starmap
from typing import Annotated, Final, get_args

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes.content import Ramp
from blender.bpy.nodes.desired import item_key, MenuItem, MenuItems
from blender.bpy.nodes.functions import sampled, W
from blender.bpy.nodes.graphs import Expr, math, menus, mixed, Scalar, vector
from blender.bpy.nodes.interfaces import decoded, Interface, InterfacePanel, Socket, StockInput
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.inputs import path_key, Shell, Step
from blender.imaging.color import colorimetry, processors
from blender.imaging.color.processors import Stops
from blender.records.faults import accumulated, Outcome
from blender.records.fields import Case

from .bases import cased, picked

# --- [TYPES] ----------------------------------------------------------------------------

type Spectrum = Tint | Incandescence | Gradient
type Color = Annotated[tuple[float, float, float, float], bpy.types.NodeSocketColor]
type Temperature = Annotated[float, StockInput(node=bpy.types.ShaderNodeBlackbody, input="Temperature")]

# --- [CONSTANTS] ------------------------------------------------------------------------

_SPECTRUM: Final[Key] = "spectrum"

# --- [MODELS] ---------------------------------------------------------------------------


class Tint(Case, frozen=True, kw_only=True):
    """Color graded from start to end by field tone."""

    start: Annotated[Color, msgspec.Meta(description="Scene-linear color at tone 0")] = (1.0, 1.0, 1.0, 1.0)
    end: Annotated[Color, msgspec.Meta(description="Scene-linear color at tone 1")] = (1.0, 1.0, 1.0, 1.0)


class Incandescence(Case, frozen=True, kw_only=True):
    """Unit-luminance blackbody color graded from cool to hot temperature by field tone."""

    cool: Annotated[Temperature, msgspec.Meta(description="Temperature at tone 0")] = 1000.0
    hot: Annotated[Temperature, msgspec.Meta(description="Temperature at tone 1")] = 2000.0


class Gradient(Case, frozen=True, kw_only=True):
    """Color read from a color ramp at field tone."""

    ramp: Ramp = Ramp()


class Emission(msgspec.Struct, frozen=True, kw_only=True):
    """Light a generator adds over its coverage, colored by tone, its luminance at full density in stops."""

    spectrum: Spectrum = Incandescence()
    intensity: Annotated[Stops, msgspec.Meta(description="Luminance at full density in stops")] = 2.0


# --- [INTERFACES] -----------------------------------------------------------------------

_KINDS: Final[tuple[type[Spectrum], ...]] = get_args(Spectrum.__value__)
_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketMenu, description="Spectrum coloring each tone", items=Some(MenuItems.of(Spectrum)))

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PANELS]


def spectra(key: Key, chosen: Spectrum, description: str, toggle: Option[bool] = Nothing) -> InterfacePanel:
    """Spectrum panel at `key`: its color menu defaulting to the chosen case, then the tint and blackbody panels, the ramp held by the record."""
    cases = tuple(InterfacePanel.of(item_key(type(case)), case, name=MenuItem.of(type(case)).name) for case in cased(_KINDS, chosen) if not isinstance(case, Gradient))
    menu = msgspec.structs.replace(_COLOR, default_value=MenuItem.of(type(chosen)).name)
    return InterfacePanel.of(key, Interface(inputs=(menu,), panels=cases), description=description, toggle=toggle)


def emission(key: Key, light: Emission, description: str, toggle: Option[bool] = Nothing) -> InterfacePanel:
    """Emission panel at `key`: luminance in stops, then the spectrum panel defaulting to the light's case."""
    own = InterfacePanel.of(key, light, description=description, toggle=toggle)
    return msgspec.structs.replace(own, panels=InterfacePanel.of(key, Interface(panels=(spectra(_SPECTRUM, light.spectrum, "Color emitted at each tone"),))).panels)


# --- [NODES]


def emitted[R: Case](shell: Shell[R], panel: InterfacePanel, light: Emission, tones: tuple[Scalar, *tuple[Scalar, ...]]) -> Outcome[tuple[Expr, ...]]:
    """Returns scene-linear emitted light at each tone through the one color menu of an emission panel.

    Args:
        shell: Working config and entry inputs.
        panel: Emission panel `emission` declared, as the effect's interface holds it.
        light: Record emission holding the ramp of its gradient case.
        tones: Tone operands in output order.

    Returns:
        Light per tone, or the working luminance fault.
    """

    def read(*path: object) -> Expr:
        return shell.input(panel.socket(*path))

    ramp = light.spectrum.ramp if isinstance(light.spectrum, Gradient) else Gradient().ramp
    brightness = math("POWER", 2.0, read(Emission.intensity))

    def toned(index: int, tone: Scalar) -> Outcome[tuple[Expr, Expr, Expr]]:
        body = Expr(
            kind=bpy.types.ShaderNodeBlackbody,
            inputs=(("Temperature", mixed(tone, read(_SPECTRUM, Incandescence, Incandescence.cool), read(_SPECTRUM, Incandescence, Incandescence.hot))),),
            socket="Color",
            shape=bpy.types.NodeSocketColor,
        )
        gradient = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", tone),), content=Some(ramp), socket="Color", shape=bpy.types.NodeSocketColor)
        named = item_key(panel.key, _SPECTRUM, Gradient, Gradient.ramp) if index == 0 else item_key(panel.key, index, Gradient.ramp)
        tint = mixed(tone, read(_SPECTRUM, Tint, Tint.start), read(_SPECTRUM, Tint, Tint.end))
        return masks.luminance(shell.config, body).map(lambda level: (tint, vector("DIVIDE", body, level), gradient.named(named)))

    def lit(cases: tuple[tuple[Expr, Expr, Expr], ...]) -> tuple[Expr, ...]:
        columns = {MenuItem.of(kind): tuple(case[position] for case in cases) for position, kind in enumerate(_KINDS)}
        return tuple(vector("SCALE", color, brightness) for color in menus(shell.input(panel, _SPECTRUM, _COLOR), columns))

    return accumulated(starmap(toned, enumerate(tones))).map(lit)


# --- [EVALUATION]


def colored(values: Mapping[Key, Value], chosen: Spectrum, /, *path: Step) -> Outcome[Spectrum]:
    """Returns the spectrum entry input values hold under the spectrum panel a path ends at: the case its color menu names, decoded, the gradient's ramp as `chosen` holds it.

    Args:
        values: Entry input values by item key.
        chosen: Record spectrum, its ramp kept.
        *path: Path to a panel `spectra` declared, such as `(panel,)`.

    Returns:
        `Ok` of the held spectrum. `Error` of `absent` for no color value, `outside` for an item no case names, and `invalid` for a value outside its field.
    """
    return picked(_KINDS, chosen, values, *path, menu=_COLOR).bind(lambda case: decoded(case, values, item_key(path_key(*path), type(case))))


def selected(panel: InterfacePanel, light: Emission, values: Mapping[Key, Value]) -> Outcome[Emission]:
    """Returns the emission entry input values hold under an emission panel `emission` declared, its intensity and spectrum decoded."""
    return colored(values, light.spectrum, panel, _SPECTRUM).bind(lambda spectrum: decoded(msgspec.structs.replace(light, spectrum=spectrum), values, panel.key))


def spectrum(identity: Identity, request: Spectrum, tones: npt.NDArray[np.float64], config: PyOpenColorIO.Config) -> Outcome[npt.NDArray[np.float64]]:
    """Evaluates scene-linear RGB of linear tints, host ramps, or unit-luminance blackbodies integrated against the observer at each tone."""
    match request:
        case Tint(start=start, end=end):
            return Ok((1.0 - tones[:, None]) * np.asarray(start[:3]) + tones[:, None] * np.asarray(end[:3]))
        case Gradient(ramp=ramp):
            gradient = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", W),), content=Some(ramp), socket="Color", shape=bpy.types.NodeSocketColor)
            return sampled(identity, {"w": tones}, gradient).map(lambda values: values[0][:, :3].astype(np.float64))
        case Incandescence(cool=cool, hot=hot):
            tristimulus = colorimetry.blackbody((1.0 - tones) * cool + tones * hot)
            return processors.basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR).map(lambda matrix: np.asarray(np.linalg.solve(matrix, tristimulus.T).T, dtype=np.float64))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Emission", "Gradient", "Incandescence", "Spectrum", "Tint", "colored", "emission", "emitted", "selected", "spectra", "spectrum"]
