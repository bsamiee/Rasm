"""Light shafts and lens flares generated about a frame position or a bound scene object seen through the camera."""

from enum import auto, Enum
from math import tau
from typing import Annotated, Final

import bpy
from expression import Ok, Option, Some
import msgspec

from blender.bpy.data.rna.conditions import Present
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes import textures
from blender.bpy.nodes.content import Ramp, Stop
from blender.bpy.nodes.functions import EXPRESSIONS
from blender.bpy.nodes.graphs import Expr, math, mixed, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.coordinates import dimensions, sampled
from blender.bpy.render.compositor.effects import CompositorEffect, depth_precise
from blender.bpy.render.compositor.inputs import entry_input
from blender.bpy.render.compositor.lenses import Sighted
from blender.bpy.render.compositor.motion import clocked, origin
from blender.bpy.render.compositor.shapes import coverage, rotated
from blender.bpy.render.compositor.shells import PreviewFrame, Shell
from blender.bpy.render.view_layers.passes import PassRequest
from blender.bpy.scene.cameras import camera_type
from blender.imaging.geometry.lenses import Aperture
from blender.imaging.sampling import tiles
from blender.records.faults import accumulated, Outcome, paired

from ...chain.stages import Effect, Stage
from .procedural import bases, Basis, clock, Emission, emission, emitted, Gradient, Perlin, textured, Tint

# --- [TYPES] ----------------------------------------------------------------------------


class Grading(Enum):
    """Ghost color sampled per aperture image or continuously along axis."""

    INSTANCE = auto()
    AXIS = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_FRAME_POSITION: Final = MenuItem(name="Frame Position", description="Source at frame position, needing no camera or depth")
_OBJECT: Final = MenuItem(name="Object", description="Bound scene object read as location or local +Z direction")
_POSITION: Final = MenuItem(name="Position", description="Object location")
_DIRECTION: Final = MenuItem(name="Direction", description="Direction toward a light at infinity")
_SOURCE_KIND: Final = Socket(
    key="source_kind",
    name="Source",
    kind=bpy.types.NodeSocketMenu,
    description="Frame position or bound scene object",
    default_value=_FRAME_POSITION.name,
    items=Some(MenuItems(items=(_FRAME_POSITION, _OBJECT))),
)
_SOURCE_POSITION: Final = Socket(key="source_position", name="Position", kind=bpy.types.NodeSocketVector2D, description="Source in frame fractions", default_value=(0.3, 0.6))
_SOURCE_OBJECT: Final = Socket(key="source_object", name="Object", kind=bpy.types.NodeSocketObject, description="Bound scene object")
_SOURCE_READING: Final = Socket(
    key="source_reading",
    name="Reading",
    kind=bpy.types.NodeSocketMenu,
    description="Location or local +Z toward distant light",
    default_value=_POSITION.name,
    items=Some(MenuItems(items=(_POSITION, _DIRECTION))),
)
_SOURCE_OCCLUSION: Final = Socket(key="source_occlusion", name="Occlusion", kind=bpy.types.NodeSocketBool, description="Nearer surfaces hide source", default_value=True)
_SOURCE: Final = InterfacePanel(
    key="source", name="Source", description="Frame position or bound scene object the light comes from", inputs=(_SOURCE_KIND, _SOURCE_POSITION, _SOURCE_OBJECT, _SOURCE_READING, _SOURCE_OCCLUSION)
)
_THRESHOLD, _SMOOTHNESS = masks.HIGHLIGHTS.inputs
_LENGTH: Final = Socket(key="length", kind=bpy.types.NodeSocketFloat, description="Longest march over greater frame side", min_value=0.0, default_value=0.5)
_JITTER: Final = Socket(key="jitter", kind=bpy.types.NodeSocketFloatFactor, description="Share of march steps skipped at hashed shifts")
_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Scene-linear shaft tint and strength", default_value=(1.0, 1.0, 1.0, 1.0))
_RADIUS: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Source disc radius over greater frame side", min_value=0.0, default_value=0.01)
_SEED: Final = Socket(key="seed", kind=bpy.types.NodeSocketInt, description="Number selecting streak and ghost pattern", min_value=0, max_value=2**24)
_HOTSPOT_SOFTNESS: Final = Socket(
    key="hotspot_softness", name="Softness", kind=bpy.types.NodeSocketFloat, description="Edge ramp past one pixel over greater frame side", min_value=0.0, default_value=0.01
)
_STARBURST_LENGTH: Final = Socket(key="starburst_length", name="Length", kind=bpy.types.NodeSocketFloat, description="Streak reach over greater frame side", min_value=0.0, default_value=0.25)
_STARBURST_DENSITY: Final = Socket(key="starburst_density", name="Density", kind=bpy.types.NodeSocketFloat, description="Noise cells around source", min_value=0.0, default_value=24.0)
_STARBURST_SHARPNESS: Final = Socket(key="starburst_sharpness", name="Sharpness", kind=bpy.types.NodeSocketFloat, description="Exponent narrowing each streak", min_value=1.0, default_value=3.0)
_STARBURST_CLOCK: Final = clock("starburst_clock", speed=0.0)
_STARBURST_CLOCK_SPEED, _STARBURST_CLOCK_HOLD = _STARBURST_CLOCK.inputs
_ANAMORPHIC_LENGTH: Final = Socket(key="anamorphic_length", name="Length", kind=bpy.types.NodeSocketFloat, description="Bar half length over greater frame side", min_value=0.0, default_value=0.6)
_ANAMORPHIC_THICKNESS: Final = Socket(key="anamorphic_thickness", name="Thickness", kind=bpy.types.NodeSocketFloat, description="Bar width over greater frame side", min_value=0.0, default_value=0.004)
_ANAMORPHIC_ANGLE: Final = Socket(key="anamorphic_angle", name="Angle", kind=bpy.types.NodeSocketFloatAngle, description="Counterclockwise squeeze axis turn")
_HALO_MAGNIFICATION: Final = Socket(key="halo_magnification", name="Magnification", kind=bpy.types.NodeSocketFloat, description="Ring offset from frame center over source offset", default_value=-0.4)
_HALO_RADIUS: Final = Socket(key="halo_radius", name="Radius", kind=bpy.types.NodeSocketFloat, description="Ring radius over greater frame side", min_value=0.0, default_value=0.2)
_HALO_WIDTH: Final = Socket(key="halo_width", name="Width", kind=bpy.types.NodeSocketFloat, description="Ring width over greater frame side", min_value=0.0, default_value=0.02)
_HALO_SOFTNESS: Final = Socket(key="halo_softness", name="Softness", kind=bpy.types.NodeSocketFloat, description="Edge ramp past one pixel over greater frame side", min_value=0.0, default_value=0.02)
_GHOSTS_SQUEEZE: Final = Socket(key="ghosts_squeeze", name="Squeeze", kind=bpy.types.NodeSocketFloat, description="Horizontal aperture image compression", min_value=0.0, default_value=1.0)
_GHOSTS_FRINGE: Final = Socket(key="ghosts_fringe", name="Fringe", kind=bpy.types.NodeSocketFloat, description="Channel radius shift, toward blue above zero", min_value=-1.0, max_value=1.0)
_GHOSTS_FIRST: Final = Socket(key="ghosts_first", name="First", kind=bpy.types.NodeSocketFloat, description="First ghost magnification", default_value=0.6)
_GHOSTS_LAST: Final = Socket(key="ghosts_last", name="Last", kind=bpy.types.NodeSocketFloat, description="Last ghost magnification", default_value=-1.0)
_GHOSTS_RADIUS: Final = Socket(key="ghosts_radius", name="Radius", kind=bpy.types.NodeSocketFloat, description="Largest ghost radius over greater frame side", min_value=0.0, default_value=0.03)
_GHOSTS_VARIATION: Final = Socket(
    key="ghosts_variation", name="Variation", kind=bpy.types.NodeSocketFloat, description="Size and position jitter within slots", min_value=0.0, max_value=0.999, default_value=0.5
)
_GHOSTS_SOFTNESS: Final = Socket(key="ghosts_softness", name="Softness", kind=bpy.types.NodeSocketFloatFactor, description="Edge ramp over ghost radius", default_value=0.3)

# --- [MODELS] ---------------------------------------------------------------------------


class LightRays(Effect, frozen=True, kw_only=True):
    """Highlight shafts marched toward a source through gaps in nearer surfaces."""

    stage = Stage.GENERATE
    pointwise = False


class LensFlare(Effect, frozen=True, kw_only=True):
    """Source disc, radial streaks, anamorphic bar, halo, and ghosts generated as light, each section's emission and the streak basis holding their cases, the ghosts their iris, grading, and count."""

    stage = Stage.GENERATE
    pointwise = False
    hotspot: Emission = Emission(spectrum=Tint(start=(1.0, 0.9, 0.8), end=(1.0, 0.9, 0.8)), intensity=3.0)
    starburst: Emission = Emission(spectrum=Tint(start=(1.0, 0.9, 0.8), end=(1.0, 0.9, 0.8)), intensity=0.0)
    basis: Basis = Perlin(texture=textures.Noise(noise_dimensions="4D", scale=1.0, roughness=0.563))
    anamorphic: Emission = Emission(spectrum=Tint(start=(0.04, 0.34, 1.0), end=(0.04, 0.34, 1.0)), intensity=0.0)
    halo: Emission = Emission(spectrum=Tint(start=(0.124, 0.009, 0.194), end=(0.124, 0.009, 0.194)), intensity=0.0)
    ghosts: Emission = Emission(
        spectrum=Gradient(ramp=Ramp(stops=(Stop(position=0.0, color=(0.12, 0.02, 0.2)), Stop(position=0.5, color=(0.02, 0.1, 0.08)), Stop(position=1.0, color=(0.2, 0.08, 0.02))))), intensity=0.0
    )
    iris: Aperture = Aperture()
    grading: Grading = Grading.INSTANCE
    count: Annotated[int, msgspec.Meta(ge=1)] = 5


# --- [INTERFACES] -----------------------------------------------------------------------

_HOTSPOT_EMISSION: Final = emission("hotspot_emission", LensFlare().hotspot, "Light from disc center to rim")
_STARBURST_EMISSION: Final = emission("starburst_emission", LensFlare().starburst, "Light from streak root to tip")
_STARBURST_BASIS: Final = bases("starburst_basis", LensFlare().basis, "Texture forming streaks", "Noise")
_ANAMORPHIC_EMISSION: Final = emission("anamorphic_emission", LensFlare().anamorphic, "Light from bar center to ends")
_HALO_EMISSION: Final = emission("halo_emission", LensFlare().halo, "Light from inner to outer ring edge")
_GHOSTS_EMISSION: Final = emission("ghosts_emission", LensFlare().ghosts, "Light from first ghost to last")
_HOTSPOT: Final = InterfacePanel(key="hotspot", name="Hotspot", description="Source disc", toggle=Some(True), inputs=(_HOTSPOT_SOFTNESS,), panels=(_HOTSPOT_EMISSION,))
_STARBURST: Final = InterfacePanel(
    key="starburst",
    name="Starburst",
    description="Radial streaks about source",
    toggle=Some(True),
    inputs=(_STARBURST_LENGTH, _STARBURST_DENSITY, _STARBURST_SHARPNESS),
    panels=(_STARBURST_EMISSION, _STARBURST_BASIS, _STARBURST_CLOCK),
)
_ANAMORPHIC: Final = InterfacePanel(
    key="anamorphic",
    name="Anamorphic Bar",
    description="Line through source along squeeze axis",
    toggle=Some(False),
    inputs=(_ANAMORPHIC_LENGTH, _ANAMORPHIC_THICKNESS, _ANAMORPHIC_ANGLE),
    panels=(_ANAMORPHIC_EMISSION,),
)
_HALO: Final = InterfacePanel(
    key="halo", name="Halo", description="Ring centered along source axis", toggle=Some(True), inputs=(_HALO_MAGNIFICATION, _HALO_RADIUS, _HALO_WIDTH, _HALO_SOFTNESS), panels=(_HALO_EMISSION,)
)
_GHOSTS: Final = InterfacePanel(
    key="ghosts",
    name="Ghosts",
    description="Aperture images along source axis",
    toggle=Some(True),
    inputs=(_GHOSTS_SQUEEZE, _GHOSTS_FRINGE, _GHOSTS_FIRST, _GHOSTS_LAST, _GHOSTS_RADIUS, _GHOSTS_VARIATION, _GHOSTS_SOFTNESS),
    panels=(_GHOSTS_EMISSION,),
)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SOURCE]


def located[R: LightRays | LensFlare](shell: Shell[R]) -> tuple[Expr, Expr, Expr]:
    """Returns the source's frame fractions, its ahead gate, and its depth-pass occlusion, zero unless a bound object occludes."""
    direction = shell.menu(_SOURCE_READING, cases={_POSITION: False, _DIRECTION: True})
    bound = shell.menu(_SOURCE_KIND, cases={_FRAME_POSITION: False, _OBJECT: True})
    sighted = Sighted.of(shell.input(_SOURCE_OBJECT), direction)
    return (switched(bound, shell.input(_SOURCE_POSITION), sighted.position), switched(bound, 1.0, sighted.ahead), (shell.layer("Depth") < sighted.depth) * shell.input(_SOURCE_OCCLUSION) * bound)


# --- [CHECKS]


def _source_checked[R: LightRays | LensFlare](identity: Identity, scene: bpy.types.Scene, entry: Option[bpy.types.SceneCompositorEffect], _record: R, _preview: Option[PreviewFrame]) -> Outcome[None]:
    """Refuses a bound object source without an object or under a camera that cannot project it."""

    def valued(held: bpy.types.SceneCompositorEffect, key: Key) -> Outcome[Value]:
        return entry_input(identity, held, key).bind(lambda socket: Descriptor.resolve(socket, ("value",), bpy.types.Property).map(lambda descriptor: descriptor.read(socket)))

    def bound(held: bpy.types.SceneCompositorEffect, kind: Value, reading: Value) -> Outcome[None]:
        present = entry_input(identity, held, _SOURCE_OBJECT.key).bind(
            lambda socket: Descriptor.resolve(socket, ("value",), bpy.types.PointerProperty).bind(lambda descriptor: Present(descriptor=descriptor).check(bpy.context, Some(socket)))
        )
        camera = camera_type(*(("PERSP",) if reading == _DIRECTION.name else ("PERSP", "ORTHO"))).check(bpy.context, Some(scene))
        return paired(present, camera).map(lambda _: None) if kind == _OBJECT.name else Ok(None)

    return entry.map(lambda held: paired(valued(held, _SOURCE_KIND.key), valued(held, _SOURCE_READING.key)).bind(lambda pair: bound(held, *pair))).default_value(Ok(None))


# --- [GRAPHS]


def rayed(shell: Shell[LightRays]) -> Outcome[Expr]:
    """Adds highlight shafts marched toward the source through its depth gaps."""
    key = masks.highlight(shell.color, shell.input(_THRESHOLD), shell.input(_SMOOTHNESS))
    diagonal = vector("LENGTH", dimensions(shell.color))
    position, front, occluded = located(shell)
    rays = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(
            ("Image", vector("SCALE", key, 1.0 - occluded)),
            ("Type", "Sun Beams"),
            ("Quality", "High"),
            ("Clamp Highlights", False),
            ("Highlights Threshold", 0.0),
            ("Highlights Smoothness", 0.0),
            ("Sun Position", sampled(shell.space, position)),
            ("Size", shell.input(_LENGTH) * shell.space.greater / diagonal),
            ("Jitter", shell.input(_JITTER)),
        ),
        socket="Glare",
        shape=bpy.types.NodeSocketColor,
    )
    return Ok(mixed(1.0, shell.color, vector("SCALE", vector("MULTIPLY", rays, shell.input(_COLOR)), front), blend="ADD"))


def flared(shell: Shell[LensFlare]) -> Outcome[Expr]:
    """Adds the source-centered flare sections to the stage input under finite-disc visibility."""
    inlet, frame, record = shell.input, shell.space, shell.record
    center = frame.extent * 0.5

    def ramped(distance: Expr, length: Scalar) -> tuple[Expr, Expr]:
        remaining = math("MAXIMUM", math("SUBTRACT", length, distance), 0.0) / length
        return math("MINIMUM", distance / length, 1.0), remaining * remaining

    def lit(source: tuple[Expr, Expr, Expr]) -> Outcome[Expr]:
        position, front, occluded = source
        axis, offset = position * frame.extent - center, frame.point - position * frame.extent
        rho, pixel, radius = vector("LENGTH", offset), 1.0 / frame.greater, inlet(_RADIUS)
        edge, diameter = rho - radius, Some(radius * 2.0)
        disc = coverage(edge, diameter, 0.0, pixel)
        hidden, shown = (Expr(kind=bpy.types.CompositorNodeLevels, inputs=(("Image", image), ("Channel", "Red")), socket="Mean") for image in (disc * occluded, disc))
        visible = front * (1.0 - hidden / shown)
        direction = xyz(*(math(operation, math("ARCTAN2", offset["Y"], offset["X"])) for operation in ("COSINE", "SINE")))
        ((streak, _),) = textured(
            shell,
            _STARBURST_BASIS.key,
            record.basis,
            vector("SCALE", direction, inlet(_STARBURST_DENSITY) / tau),
            clocked(inlet(_STARBURST_CLOCK_SPEED), inlet(_STARBURST_CLOCK_HOLD)),
            inlet(_SEED),
            streams=(tiles.stream(LensFlare, "starburst_basis"),),
        )
        star_tone, star_falloff = ramped(rho, inlet(_STARBURST_LENGTH))
        turned = rotated(offset, inlet(_ANAMORPHIC_ANGLE), inverted=True)
        bar_tone, bar_falloff = ramped(abs(turned["X"]), inlet(_ANAMORPHIC_LENGTH))
        delta = vector("LENGTH", frame.point - vector("MULTIPLY_ADD", axis, inlet(_HALO_MAGNIFICATION), center)) - inlet(_HALO_RADIUS)
        width = inlet(_HALO_WIDTH)
        sections = (
            (_HOTSPOT, _HOTSPOT_EMISSION, record.hotspot, coverage(edge, diameter, inlet(_HOTSPOT_SOFTNESS), pixel), ramped(rho, radius)[0]),
            (_STARBURST, _STARBURST_EMISSION, record.starburst, math("MAXIMUM", math("MULTIPLY_ADD", streak, 2.0, -1.0), 0.0) ** inlet(_STARBURST_SHARPNESS) * star_falloff, star_tone),
            (
                _ANAMORPHIC,
                _ANAMORPHIC_EMISSION,
                record.anamorphic,
                coverage(math("MULTIPLY_ADD", inlet(_ANAMORPHIC_THICKNESS), -0.5, abs(turned["Y"])), Some(inlet(_ANAMORPHIC_THICKNESS)), 0.0, pixel) * bar_falloff,
                bar_tone,
            ),
            (_HALO, _HALO_EMISSION, record.halo, coverage(math("MULTIPLY_ADD", width, -0.5, abs(delta)), Some(width), inlet(_HALO_SOFTNESS), pixel), math("ADD", delta / width, 0.5, clamp=True)),
        )

        def imaged(index: int) -> tuple[Expr, Expr]:
            drawn = vector("SCALE", origin(inlet(_SEED), float(tiles.stream(LensFlare, "ghosts")), instance=index), 1.0 / 256.0)
            shrink = 1.0 - inlet(_GHOSTS_VARIATION) * drawn["X"]
            slot = math("MULTIPLY_ADD", drawn["Y"] - 0.5, inlet(_GHOSTS_VARIATION), index + 0.5) / record.count
            magnification = mixed(slot, inlet(_GHOSTS_FIRST), inlet(_GHOSTS_LAST))
            flipped = vector("SCALE", frame.point - vector("MULTIPLY_ADD", axis, magnification, center), math("MULTIPLY_ADD", magnification < 0.0, -2.0, 1.0))
            point = xyz(flipped["X"] * inlet(_GHOSTS_SQUEEZE), flipped["Y"] * -1.0)
            bearing, reach = math("ARCTAN2", point["Y"], point["X"]), vector("LENGTH", point)
            outer, inner = record.iris.radial(bearing, EXPRESSIONS), record.iris.radial(bearing, EXPRESSIONS, record.iris.obstruction)
            size = inlet(_GHOSTS_RADIUS) * shrink
            ramp = inlet(_GHOSTS_SOFTNESS) * size + pixel
            fringe = inlet(_GHOSTS_FRINGE)

            def channel(factor: Expr) -> Expr:
                extent = size * factor
                kept = math("SUBTRACT", 0.5, (reach - extent * outer) / ramp, clamp=True)
                covered = math("MINIMUM", kept, math("ADD", 1.0, (reach - extent * inner) / ramp, clamp=True)) if record.iris.obstruction > 0.0 else kept
                return switched(extent > 0.0, 0.0, covered)

            channels = xyz(*map(channel, (1.0 - math("MAXIMUM", fringe, 0.0), math("MULTIPLY_ADD", abs(fringe), -0.5, 1.0), 1.0 + math("MINIMUM", fringe, 0.0))))
            return (vector("SCALE", channels, 1.0 / (shrink * shrink)) if record.iris.obstruction < 1.0 else xyz()), slot

        images = tuple(map(imaged, range(record.count)))
        along = vector("DOT_PRODUCT", frame.point - center, axis) / vector("DOT_PRODUCT", axis, axis)
        graded = math("DIVIDE", along - inlet(_GHOSTS_FIRST), inlet(_GHOSTS_LAST) - inlet(_GHOSTS_FIRST), clamp=True)
        head, *tail = (slot if record.grading is Grading.INSTANCE else graded for _, slot in images)

        def summed(lights: tuple[tuple[tuple[Expr, ...], ...], tuple[Expr, ...]]) -> Expr:
            section_lights, ghost_lights = lights
            first_light, *lighted = (switched(inlet(gate), (0.0, 0.0, 0.0), vector("SCALE", light, weight)) for (gate, _, _, weight, _), (light, *_) in zip(sections, section_lights))
            glow = tuple(vector("MULTIPLY", mask, light) for (mask, _), light in zip(images, ghost_lights))
            total = sum((*lighted, switched(inlet(_GHOSTS), (0.0, 0.0, 0.0), sum(glow[1:], glow[0]))), first_light)
            return mixed(1.0, shell.color, vector("SCALE", total, visible), blend="ADD")

        return paired(accumulated(emitted(shell, panel.key, light, (tone,)) for _, panel, light, _, tone in sections), emitted(shell, _GHOSTS_EMISSION.key, record.ghosts, (head, *tail))).map(summed)

    return lit(located(shell))


# --- [COMPOSITION] ----------------------------------------------------------------------

light_rays: Final = CompositorEffect(
    rayed,
    record=LightRays,
    interface=Interface(inputs=(_THRESHOLD, _SMOOTHNESS, _LENGTH, _JITTER, _COLOR), panels=(_SOURCE,)),
    request=Some(lambda _record, layer: Ok(PassRequest.depth(layer, precise=False))),
    check=Some(_source_checked),
)
lens_flare: Final = CompositorEffect(
    flared,
    record=LensFlare,
    interface=Interface(inputs=(_RADIUS, _SEED), panels=(_SOURCE, _HOTSPOT, _STARBURST, _ANAMORPHIC, _HALO, _GHOSTS)),
    request=Some(depth_precise),
    check=Some(_source_checked),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Grading", "LensFlare", "LightRays", "lens_flare", "light_rays"]
