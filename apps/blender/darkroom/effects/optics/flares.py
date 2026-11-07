"""Light shafts and lens flares generated about a frame position or a bound scene object seen through the camera."""

from enum import auto, Enum
from math import tau
from typing import Annotated, Final, override

import bpy
from expression import Option, Some
import msgspec

from blender.bpy.data.rna.conditions import Present
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes import textures
from blender.bpy.nodes.content import Ramp, Stop
from blender.bpy.nodes.desired import MenuItem, MenuItems
from blender.bpy.nodes.functions import EXPRESSIONS
from blender.bpy.nodes.graphs import Expr, math, mixed, Scalar, switched, vector, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket, StockInput
from blender.bpy.render.compositor import masks, motion
from blender.bpy.render.compositor.coordinates import dimensions
from blender.bpy.render.compositor.effects import CompositorEffect, CompositorRecord, depth_precise
from blender.bpy.render.compositor.inputs import captured, chosen, entry_input, GraphInputs, valued
from blender.bpy.render.compositor.lenses import Sighted
from blender.bpy.render.compositor.shapes import coverage, rotated
from blender.bpy.render.view_layers.passes import PassRequest
from blender.bpy.scene.cameras import camera_type
from blender.imaging.geometry.lenses import Aperture, fringed
from blender.imaging.sampling import tiles
from blender.records.faults import accumulated, Ok, Outcome, paired

from .bases import bases, Basis, Perlin, textured
from .emission import Emission, emission, emitted, Gradient, Tint

# --- [TYPES] ----------------------------------------------------------------------------


class Grading(Enum):
    """Ghost color sampled per aperture image or continuously along axis."""

    INSTANCE = auto()
    AXIS = auto()


class _Kind(Enum):
    """Source at a frame position, needing no camera or depth, or at a bound scene object."""

    FRAME_POSITION = auto()
    OBJECT = auto()


class _Reading(Enum):
    """Bound object read as a location, or as the direction toward a light at infinity."""

    POSITION = auto()
    DIRECTION = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [SOURCES]

_LIGHT: Final[Key] = "emission"
_STREAKS: Final[Key] = "basis"
_KIND: Final = Socket(
    key="kind", kind=bpy.types.NodeSocketMenu, description="Frame position or bound scene object", default_value=MenuItem.of(_Kind.FRAME_POSITION).name, items=MenuItems.of(_Kind).items
)
_POSITION: Final = Socket(key="position", kind=bpy.types.NodeSocketVector2D, description="Source in frame fractions", default_value=(0.3, 0.6))
_BOUND: Final = Socket(key="object", kind=bpy.types.NodeSocketObject, description="Bound scene object")
_READING: Final = Socket(
    key="reading", kind=bpy.types.NodeSocketMenu, description="Location or local +Z toward distant light", default_value=MenuItem.of(_Reading.POSITION).name, items=MenuItems.of(_Reading).items
)
_OCCLUSION: Final = Socket(key="occlusion", kind=bpy.types.NodeSocketBool, description="Nearer surfaces hide source", default_value=True)
_SOURCE: Final = InterfacePanel.of("source", Interface(inputs=(_KIND, _POSITION, _BOUND, _READING, _OCCLUSION)), description="Frame position or bound scene object the light comes from")

# --- [RAYS]

_THRESHOLD, _SMOOTHNESS = masks.HIGHLIGHTS.inputs
_LENGTH: Final = Socket(key="length", kind=bpy.types.NodeSocketFloat, description="Longest march over greater frame side", min_value=0.0, default_value=0.5)
_JITTER: Final = Socket(key="jitter", kind=StockInput(node=bpy.types.CompositorNodeGlare, input="Jitter"))
_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Scene-linear shaft tint and strength", default_value=(1.0, 1.0, 1.0, 1.0))

# --- [FLARE]

_RADIUS: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Source disc radius over greater frame side", min_value=0.0, default_value=0.01)
_EDGE: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloat, description="Edge ramp past one pixel over greater frame side", min_value=0.0, default_value=0.01)
_STREAK_LENGTH: Final = Socket(key="length", kind=bpy.types.NodeSocketFloat, description="Streak reach over greater frame side", min_value=0.0, default_value=0.25)
_STREAK_DENSITY: Final = Socket(key="density", kind=bpy.types.NodeSocketFloat, description="Noise cells around source", min_value=0.0, default_value=24.0)
_SHARPNESS: Final = Socket(key="sharpness", kind=bpy.types.NodeSocketFloat, description="Exponent narrowing each streak", min_value=1.0, default_value=3.0)
_BAR_LENGTH: Final = Socket(key="length", kind=bpy.types.NodeSocketFloat, description="Bar half length over greater frame side", min_value=0.0, default_value=0.6)
_THICKNESS: Final = Socket(key="thickness", kind=bpy.types.NodeSocketFloat, description="Bar width over greater frame side", min_value=0.0, default_value=0.004)
_SQUEEZE_ANGLE: Final = Socket(key="angle", kind=bpy.types.NodeSocketFloatAngle, description="Counterclockwise squeeze axis turn")
_MAGNIFICATION: Final = Socket(key="magnification", kind=bpy.types.NodeSocketFloat, description="Ring offset from frame center over source offset", default_value=-0.4)
_RING_RADIUS: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Ring radius over greater frame side", min_value=0.0, default_value=0.2)
_RING_WIDTH: Final = Socket(key="width", kind=bpy.types.NodeSocketFloat, description="Ring width over greater frame side", min_value=0.0, default_value=0.02)
_RING_EDGE: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloat, description="Edge ramp past one pixel over greater frame side", min_value=0.0, default_value=0.02)
_SQUEEZE: Final = Socket(key="squeeze", kind=bpy.types.NodeSocketFloat, description="Horizontal aperture image compression", min_value=0.0, default_value=1.0)
_FRINGE: Final = Socket(key="fringe", kind=bpy.types.NodeSocketFloat, description="Channel radius shift, toward blue above zero", min_value=-1.0, max_value=1.0)
_FIRST: Final = Socket(key="first", kind=bpy.types.NodeSocketFloat, description="First ghost magnification", default_value=0.6)
_LAST: Final = Socket(key="last", kind=bpy.types.NodeSocketFloat, description="Last ghost magnification", default_value=-1.0)
_GHOST_RADIUS: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Largest ghost radius over greater frame side", min_value=0.0, default_value=0.03)
_VARIATION: Final = Socket(key="variation", kind=bpy.types.NodeSocketFloatFactor, description="Size and position jitter within slots", default_value=0.5)
_GHOST_EDGE: Final = Socket(key="softness", kind=bpy.types.NodeSocketFloatFactor, description="Edge ramp over ghost radius", default_value=0.3)

# --- [MODELS] ---------------------------------------------------------------------------


class LightRays(CompositorRecord, frozen=True, kw_only=True):
    """Highlight shafts marched toward a source through gaps in nearer surfaces."""

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Adds highlight shafts marched toward the source through its depth gaps."""
        key = masks.highlight(shell.color, shell.input(_THRESHOLD), shell.input(_SMOOTHNESS))
        position, front, occluded = _located(shell)
        rays = Expr(
            kind=bpy.types.CompositorNodeGlare,
            inputs=(
                ("Image", vector("SCALE", key, 1.0 - occluded)),
                ("Type", "Sun Beams"),
                ("Quality", "High"),
                ("Highlights Threshold", 0.0),
                ("Highlights Smoothness", 0.0),
                ("Sun Position", shell.space.sampled(position)),
                ("Size", shell.input(_LENGTH) * shell.space.greater / vector("LENGTH", dimensions(shell.color))),
                ("Jitter", shell.input(_JITTER)),
            ),
            socket="Glare",
            socket_type=bpy.types.NodeSocketColor,
        )
        return Ok(mixed(1.0, shell.color, vector("SCALE", vector("MULTIPLY", rays, shell.input(_COLOR)), front), blend="ADD"))

    @staticmethod
    @override
    def requested(layer: str) -> Outcome[PassRequest]:
        """Requests depth for source occlusion."""
        return Ok(PassRequest.depth(layer, precise=False))


class LensFlare(CompositorRecord, frozen=True, kw_only=True):
    """Source disc, radial streaks, anamorphic bar, halo, and ghosts generated as light, each section's emission and the streak basis holding their cases, the ghosts their iris, grading, and count."""

    hotspot_emission: Emission = Emission(spectrum=Tint(start=(1.0, 0.9, 0.8, 1.0), end=(1.0, 0.9, 0.8, 1.0)), intensity=3.0)
    starburst_emission: Emission = Emission(spectrum=Tint(start=(1.0, 0.9, 0.8, 1.0), end=(1.0, 0.9, 0.8, 1.0)), intensity=0.0)
    starburst_basis: Basis = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=1.0, roughness=0.563))
    anamorphic_emission: Emission = Emission(spectrum=Tint(start=(0.04, 0.34, 1.0, 1.0), end=(0.04, 0.34, 1.0, 1.0)), intensity=0.0)
    halo_emission: Emission = Emission(spectrum=Tint(start=(0.124, 0.009, 0.194, 1.0), end=(0.124, 0.009, 0.194, 1.0)), intensity=0.0)
    ghosts_emission: Emission = Emission(
        spectrum=Gradient(ramp=Ramp(stops=(Stop(position=0.0, color=(0.12, 0.02, 0.2)), Stop(position=0.5, color=(0.02, 0.1, 0.08)), Stop(position=1.0, color=(0.2, 0.08, 0.02))))), intensity=0.0
    )
    iris: Annotated[Aperture, msgspec.Meta(description="Ghost outline")] = Aperture()
    grading: Annotated[Grading, msgspec.Meta(description="Ghost color per aperture image or along axis")] = Grading.INSTANCE
    count: Annotated[int, msgspec.Meta(ge=1, description="Aperture images along source axis")] = 5

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Adds the source-centered flare sections to the stage input under finite-disc visibility."""
        read, frame = shell.input, shell.space
        center = frame.extent * 0.5
        seed = read(motion.SEED)

        def ramped(distance: Expr, length: Scalar) -> tuple[Expr, Expr]:
            remaining = math("MAXIMUM", math("SUBTRACT", length, distance), 0.0) / length
            return math("MINIMUM", distance / length, 1.0), remaining * remaining

        position, front, occluded = _located(shell)
        axis, offset = position * frame.extent - center, frame.point - position * frame.extent
        rho, pixel, radius = vector("LENGTH", offset), 1.0 / frame.greater, read(_RADIUS)
        edge, diameter = rho - radius, Some(radius * 2.0)
        disc = coverage(edge, diameter, 0.0, pixel)
        hidden, shown = (Expr(kind=bpy.types.CompositorNodeLevels, inputs=(("Image", image), ("Channel", "Red")), socket="Mean") for image in (disc * occluded, disc))
        visible = front * (1.0 - hidden / shown)
        direction = xyz(*(math(operation, math("ARCTAN2", offset["Y"], offset["X"])) for operation in ("COSINE", "SINE")))
        time = motion.clocked(read, _STARBURST.panel(motion.CLOCK.key))
        ((streak, _),) = textured(
            shell, _STARBURST.panel(_STREAKS), self.starburst_basis, vector("SCALE", direction, read(_STARBURST, _STREAK_DENSITY) / tau), time, seed, streams=(tiles.stream(LensFlare.starburst_basis),)
        )
        star_tone, star_falloff = ramped(rho, read(_STARBURST, _STREAK_LENGTH))
        turned = rotated(offset, read(_ANAMORPHIC, _SQUEEZE_ANGLE), inverted=True)
        bar_tone, bar_falloff = ramped(abs(turned["X"]), read(_ANAMORPHIC, _BAR_LENGTH))
        delta = vector("LENGTH", frame.point - vector("MULTIPLY_ADD", axis, read(_HALO, _MAGNIFICATION), center)) - read(_HALO, _RING_RADIUS)
        width, thickness = read(_HALO, _RING_WIDTH), read(_ANAMORPHIC, _THICKNESS)
        sections = (
            (_HOTSPOT, self.hotspot_emission, coverage(edge, diameter, read(_HOTSPOT, _EDGE), pixel), ramped(rho, radius)[0]),
            (_STARBURST, self.starburst_emission, math("MAXIMUM", math("MULTIPLY_ADD", streak, 2.0, -1.0), 0.0) ** read(_STARBURST, _SHARPNESS) * star_falloff, star_tone),
            (_ANAMORPHIC, self.anamorphic_emission, coverage(math("MULTIPLY_ADD", thickness, -0.5, abs(turned["Y"])), Some(thickness), 0.0, pixel) * bar_falloff, bar_tone),
            (_HALO, self.halo_emission, coverage(math("MULTIPLY_ADD", width, -0.5, abs(delta)), Some(width), read(_HALO, _RING_EDGE), pixel), math("ADD", delta / width, 0.5, clamp=True)),
        )
        iris = self.iris
        first, last, variation = read(_GHOSTS, _FIRST), read(_GHOSTS, _LAST), read(_GHOSTS, _VARIATION)

        def imaged(index: int) -> tuple[Expr, Expr]:
            drawn = motion.hashed(seed, float(tiles.stream(LensFlare.ghosts_emission)), instance=index)
            shrink = 1.0 - variation * drawn["X"]
            slot = math("MULTIPLY_ADD", drawn["Y"] - 0.5, variation, index + 0.5) / self.count
            magnification = mixed(slot, first, last)
            flipped = vector("SCALE", frame.point - vector("MULTIPLY_ADD", axis, magnification, center), math("MULTIPLY_ADD", magnification < 0.0, -2.0, 1.0))
            point = xyz(flipped["X"] * read(_GHOSTS, _SQUEEZE), flipped["Y"] * -1.0)
            bearing, reach = math("ARCTAN2", point["Y"], point["X"]), vector("LENGTH", point)
            outer, inner = iris.radial(bearing, EXPRESSIONS), iris.radial(bearing, EXPRESSIONS, iris.obstruction)
            size = read(_GHOSTS, _GHOST_RADIUS) * shrink
            ramp = read(_GHOSTS, _GHOST_EDGE) * size + pixel

            def channel(factor: Expr) -> Expr:
                extent = size * factor
                kept = math("SUBTRACT", 0.5, (reach - extent * outer) / ramp, clamp=True)
                held = math("MINIMUM", kept, math("ADD", 1.0, (reach - extent * inner) / ramp, clamp=True)) if iris.obstruction > 0.0 else kept
                return switched(extent > 0.0, 0.0, held)

            channels = xyz(*map(channel, fringed(read(_GHOSTS, _FRINGE), EXPRESSIONS)))
            return (vector("SCALE", channels, 1.0 / (shrink * shrink)) if iris.obstruction < 1.0 else xyz()), slot

        images = tuple(map(imaged, range(self.count)))
        along = vector("DOT_PRODUCT", frame.point - center, axis) / vector("DOT_PRODUCT", axis, axis)
        graded = math("DIVIDE", along - first, last - first, clamp=True)
        head, *tail = (slot if self.grading is Grading.INSTANCE else graded for _, slot in images)

        def summed(lights: tuple[tuple[tuple[Expr, ...], ...], tuple[Expr, ...]]) -> Expr:
            section_lights, ghost_lights = lights
            first_light, *lighted = (switched(read(panel), (0.0, 0.0, 0.0), vector("SCALE", light, weight)) for (panel, _, weight, _), (light, *_) in zip(sections, section_lights))
            glow = tuple(vector("MULTIPLY", mask, light) for (mask, _), light in zip(images, ghost_lights))
            total = sum((*lighted, switched(read(_GHOSTS), (0.0, 0.0, 0.0), sum(glow[1:], glow[0]))), first_light)
            return mixed(1.0, shell.color, vector("SCALE", total, visible), blend="ADD")

        return paired(
            accumulated(emitted(shell, panel.panel(_LIGHT), light, (tone,)) for panel, light, _, tone in sections), emitted(shell, _GHOSTS.panel(_LIGHT), self.ghosts_emission, (head, *tail))
        ).map(summed)

    requested = staticmethod(depth_precise)


# --- [INTERFACES] -----------------------------------------------------------------------

_DEFAULTS: Final = LensFlare()
_HOTSPOT: Final = InterfacePanel.of(
    "hotspot", Interface(inputs=(_EDGE,), panels=(emission(_LIGHT, _DEFAULTS.hotspot_emission, "Light from disc center to rim"),)), description="Source disc", toggle=Some(True)
)
_STARBURST: Final = InterfacePanel.of(
    "starburst",
    Interface(
        inputs=(_STREAK_LENGTH, _STREAK_DENSITY, _SHARPNESS),
        panels=(
            emission(_LIGHT, _DEFAULTS.starburst_emission, "Light from streak root to tip"),
            bases(_STREAKS, _DEFAULTS.starburst_basis, "Texture forming streaks"),
            InterfacePanel.of(motion.CLOCK.key, motion.Clock(speed=0.0)),
        ),
    ),
    description="Radial streaks about source",
    toggle=Some(True),
)
_ANAMORPHIC: Final = InterfacePanel.of(
    "anamorphic",
    Interface(inputs=(_BAR_LENGTH, _THICKNESS, _SQUEEZE_ANGLE), panels=(emission(_LIGHT, _DEFAULTS.anamorphic_emission, "Light from bar center to ends"),)),
    name="Anamorphic Bar",
    description="Line through source along squeeze axis",
    toggle=Some(False),
)
_HALO: Final = InterfacePanel.of(
    "halo",
    Interface(inputs=(_MAGNIFICATION, _RING_RADIUS, _RING_WIDTH, _RING_EDGE), panels=(emission(_LIGHT, _DEFAULTS.halo_emission, "Light from inner to outer ring edge"),)),
    description="Ring centered along source axis",
    toggle=Some(True),
)
_GHOSTS: Final = InterfacePanel.of(
    "ghosts",
    Interface(inputs=(_SQUEEZE, _FRINGE, _FIRST, _LAST, _GHOST_RADIUS, _VARIATION, _GHOST_EDGE), panels=(emission(_LIGHT, _DEFAULTS.ghosts_emission, "Light from first ghost to last"),)),
    description="Aperture images along source axis",
    toggle=Some(True),
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _located(shell: GraphInputs) -> tuple[Expr, Expr, Expr]:
    """Returns the source's frame fractions, its ahead gate, and its depth-pass occlusion, zero unless a bound object occludes."""
    direction = shell.menu(_SOURCE, _READING, cases={MenuItem.of(_Reading.POSITION): False, MenuItem.of(_Reading.DIRECTION): True})
    bound = shell.menu(_SOURCE, _KIND, cases={MenuItem.of(_Kind.FRAME_POSITION): False, MenuItem.of(_Kind.OBJECT): True})
    sighted = Sighted.of(shell.input(_SOURCE, _BOUND), direction)
    return (switched(bound, shell.input(_SOURCE, _POSITION), sighted.position), switched(bound, 1.0, sighted.ahead), (shell.layer("Depth") < sighted.depth) * shell.input(_SOURCE, _OCCLUSION) * bound)


def _source_checked(identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect]) -> Outcome[None]:
    """Refuses a bound object source without an object or under a camera that cannot project it."""

    def bound(held: bpy.types.SceneCompositorEffect, kind: _Kind, reading: _Reading) -> Outcome[None]:
        present = entry_input(identity, held, _SOURCE, _BOUND).bind(
            lambda socket: Descriptor.resolve(socket, ("value",), bpy.types.PointerProperty).bind(lambda descriptor: Present(descriptor=descriptor).check(bpy.context, Some(socket)))
        )
        camera = camera_type(*(("PERSP",) if reading is _Reading.DIRECTION else ("PERSP", "ORTHO"))).check(bpy.context, Some(scene))
        return paired(present, camera).map(lambda _: None) if kind is _Kind.OBJECT else Ok(None)

    def read(held: bpy.types.SceneCompositorEffect) -> Outcome[None]:
        return (
            captured(identity, held).map(valued).bind(lambda values: paired(chosen(values, _Kind, _SOURCE, _KIND), chosen(values, _Reading, _SOURCE, _READING))).bind(lambda pair: bound(held, *pair))
        )

    return entry.map(read).default_value(Ok(None))


# --- [COMPOSITION] ----------------------------------------------------------------------

light_rays: Final = CompositorEffect(record=LightRays, interface=Interface(inputs=(_THRESHOLD, _SMOOTHNESS, _LENGTH, _JITTER, _COLOR), panels=(_SOURCE,)), check=Some(_source_checked))
lens_flare: Final = CompositorEffect(
    record=LensFlare, interface=Interface(inputs=(_RADIUS, motion.SEED), panels=(_SOURCE, _HOTSPOT, _STARBURST, _ANAMORPHIC, _HALO, _GHOSTS)), check=Some(_source_checked), derives=(motion.timing,)
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Grading", "LensFlare", "LightRays", "lens_flare", "light_rays"]
