"""Spatial denoiser units and motion-aligned temporal reconstruction."""

from collections.abc import Callable
import functools
from itertools import starmap
from typing import ClassVar, Final, Self

import bpy
from expression import Nothing, Option, Some
import msgspec
from msgspec import structs

from blender.bpy.data.ids.images import ImageFrames, Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Component, End, Expr, group_input, Loaded, math, menu, mixed, NodeGroup, Scalar, switched, vector, where, xyz
from blender.bpy.nodes.interface import Interface, item_key, MenuItems, Socket
from blender.bpy.render.color.spaces import COLOR_INPUT, COLOR_RESULT
from blender.bpy.render.effects.compositing.masks import LUMINANCE, luminance
from blender.bpy.render.effects.coordinates import gridded
from blender.bpy.render.effects.records import KeyPath
from blender.bpy.render.effects.rows import CompositorEffect, Inspected, Shell
from blender.bpy.render.outputs.formats import Kind
from blender.bpy.render.view_layers.passes import Family, Filing, PassRequest, Source
from blender.imaging.compositing.recombination import Terms
from blender.imaging.effects.detail.denoise import Filter, Frame, Groups, Guides, Passes, SpatialDenoise, TemporalDenoise
from blender.imaging.faults import paired
from blender.imaging.parameters import Gated
from blender.rendering.outputs.naming import Output

# --- [CONSTANTS] ------------------------------------------------------------------------

ENGINE: Final = Descriptor.declared(bpy.types.Scene, "render.engine", bpy.types.EnumProperty)
VECTOR_PASS: Final = Descriptor.declared(bpy.types.ViewLayer, "use_pass_vector", bpy.types.BoolProperty)
MOTION_BLUR: Final = Descriptor.declared(bpy.types.Scene, "render.use_motion_blur", bpy.types.BoolProperty)
SETTINGS: Final = tuple(
    Descriptor.declared(bpy.types.Scene, f"render.{member}", bpy.types.EnumProperty)
    for member in ("compositor_denoise_device", "compositor_denoise_preview_quality", "compositor_denoise_final_quality")
)
_FRAME_BASE: Final = End(node="frame_base", socket="Output")
_NOISY: Final = Socket(key="noisy", name="Filter Input", kind=bpy.types.NodeSocketColor)
_ALBEDO: Final = Socket(key="albedo", name="Albedo", kind=bpy.types.NodeSocketColor)
_NORMAL: Final = Socket(key="normal", name="Normal", kind=bpy.types.NodeSocketVector)


# --- [MODELS] ---------------------------------------------------------------------------


class Pair(msgspec.Struct, frozen=True, kw_only=True):
    """Albedo and normal guides of one denoised image."""

    albedo: Scalar
    normal: Scalar

    @classmethod
    def of(cls, layer: Callable[[str], Scalar], family: Option[Family]) -> Self:
        """Returns Cycles denoising guides, family color pass as albedo when present."""
        albedo = family.filter(lambda held: bool(held.color)).map(lambda held: held.socket("color"))
        return cls(albedo=layer(albedo.default_value("Denoising Albedo")), normal=layer("Denoising Normal"))


class Gate(msgspec.Struct, frozen=True, kw_only=True):
    """Enable and amount of denoised term."""

    enabled: Scalar
    amount: Scalar


class Filters(msgspec.Struct, frozen=True, kw_only=True):
    """Input key paths of spatial filters and target."""

    target: KeyPath
    fireflies: KeyPath
    frame: KeyPath
    terms: Terms[KeyPath]
    occlusion: KeyPath
    groups: KeyPath

    @classmethod
    def of(cls) -> Self:
        """Derives paths from record field names."""
        fireflies, target_field = SpatialDenoise.__struct_fields__
        (filter_frame,) = Frame.__struct_fields__
        terms, occlusion = Passes.__struct_fields__
        (filter_groups,) = Groups.__struct_fields__
        _, value = Gated.__struct_fields__
        target = (target_field,)
        return cls(
            target=target,
            fireflies=(fireflies,),
            frame=(*target, Frame, filter_frame),
            terms=Terms(**{name: (*target, Passes, terms, name, value) for name in Terms.__struct_fields__}),
            occlusion=(*target, Passes, occlusion, value),
            groups=(*target, Groups, filter_groups),
        )

    @property
    def filters(self) -> tuple[KeyPath, ...]:
        """Frame, term, occlusion, and group paths in order."""
        return self.frame, *structs.astuple(self.terms), self.occlusion, self.groups


class Plane(msgspec.Struct, frozen=True, kw_only=True):
    """Written sequence path and image output of its plane."""

    path: str
    name: str


class Rendered(msgspec.Struct, frozen=True, kw_only=True):
    """Written color and pixel-motion sequences over one inclusive frame range."""

    color: Plane
    motion: Plane
    first: int
    last: int
    ownership: ClassVar[Ownership[bpy.types.CompositorNodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key=item_key(TemporalDenoise), saved=Saved.USED)

    def group(self, record: TemporalDenoise) -> BlenderResult[NodeGroup]:
        """Builds temporal reconstruction from current, previous, and next sequence frames.

        Each neighbor warps along its motion pass, keeps the share its reverse motion and HSV distance from the current frame allow, and
        mixes in by that share inside the rendered range.

        Args:
            record: Color thresholds, reverse-motion tolerance, and neighbor weight bound.

        Returns:
            Compositor-only sequence group, or accumulated graph faults.
        """

        def loaded(plane: Plane, offset: int) -> Expr:
            frames = ImageFrames(frame_start=self.first, frame_duration=self.last - self.first + 1, frame_offset=self.first - 1 + offset)
            return Expr(kind=bpy.types.CompositorNodeImage, content=Some(Loaded(path=plane.path, tag=Tag(sequence=True), frames=Some(frames))), socket=plane.name, shape="RGBA")

        def hsv(image: Expr, channel: str) -> Expr:
            return Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", image),), members=(("mode", "HSV"),), socket=channel)

        def mapped(image: Expr, uv: Expr, interpolation: str) -> Expr:
            return Expr(
                kind=bpy.types.CompositorNodeMapUV,
                inputs=(("Image", image), ("UV", uv), ("Interpolation", interpolation), ("Extension X", "Clip"), ("Extension Y", "Clip")),
                socket="Image",
                shape="RGBA",
            )

        color, motion = loaded(self.color, 0), loaded(self.motion, 0)
        grid = gridded(color)
        time = Expr(kind=bpy.types.CompositorNodeSceneTime, socket="Frame")
        current_value = hsv(color, "Blue")
        current_chroma = hsv(color, "Green") * current_value

        def side(offset: int, toward: tuple[Component, Component], back: tuple[Component, Component], scale: Scalar, inside: Expr) -> tuple[Expr, Expr]:
            uv = grid.at(xyz(*(motion[channel] for channel in toward)), scale)
            warped, moved = mapped(loaded(self.color, offset), uv, "Bilinear"), mapped(loaded(self.motion, offset), uv, "Nearest")
            agree = functools.reduce(lambda held, axis: held * axis, (1.0 - (abs(moved[reverse] - motion[forward]) > record.tolerance) for reverse, forward in zip(back, toward)))
            spread = abs(hsv(color, "Red") - hsv(warped, "Red"))
            distances = (
                (math("MINIMUM", spread, 1.0 - spread), record.hue),
                (abs(current_chroma - hsv(warped, "Green") * hsv(warped, "Blue")), record.saturation),
                (abs(current_value - hsv(warped, "Blue")), record.value),
            )
            alike = functools.reduce(lambda held, weight: held * weight, (math("DIVIDE", threshold - distance, threshold, clamp=True) for distance, threshold in distances))
            return switched(inside, 0.0, where(agree, alike, 0.0) * record.max_weight), switched(inside, color, warped)

        previous_weight, previous = side(-1, ("Red", "Green"), ("Blue", "Alpha"), grid.inverse, time > float(self.first))
        next_weight, following = side(1, ("Blue", "Alpha"), ("Red", "Green"), vector("MULTIPLY", grid.inverse, (-1.0, -1.0, -1.0)), time < float(self.last))
        total = previous_weight + next_weight
        blend = mixed(total, color, mixed(next_weight / total, previous, following))
        restored = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", blend), ("Alpha", hsv(color, "Alpha")), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")
        return NodeGroup.built(key=self.ownership.key, name=TemporalDenoise.title, interface=Interface(outputs=(COLOR_RESULT,)), outputs={COLOR_RESULT.key: restored})


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [REQUESTS]


def guided(layer: str) -> BlenderResult[PassRequest]:
    """Requests Cycles denoising guide passes under the Cycles engine."""
    return Descriptor.resolve(bpy.types.ViewLayer, "cycles.denoising_store_passes", bpy.types.BoolProperty).map(
        lambda flag: PassRequest(layer=layer, flags=frozenset({flag}), condition=Some(Setting(descriptor=ENGINE, accepted=("CYCLES",))))
    )


def motioned(layer: str) -> tuple[PassRequest, Setting]:
    """Requests vector pass with motion blur off for sequence render."""
    return PassRequest(layer=layer, flags=frozenset({VECTOR_PASS})), Setting(descriptor=MOTION_BLUR, accepted=(False,))


def filing(terms: tuple[tuple[str, Expr], ...]) -> Filing:
    """Routes denoised terms to beauty layers."""
    return Filing(output=Output.BEAUTY, kind=Kind.LAYERED, sources=tuple(Source(name=f"{label} Denoised", term=term, kind="RGBA", dimensions=Nothing, output=Output.BEAUTY) for label, term in terms))


# --- [GRAPHS]


def denoiser(working: NodeGroup) -> BlenderResult[NodeGroup]:
    """Builds shared firefly and HDR denoise group.

    Fireflies scale toward the fourth brightest valid luminance on the 5x5 ring, ranked by a Batcher merge network that orders invalid
    taps first.

    Args:
        working: Working-space luminance group with image input and luminance output.

    Returns:
        Group with filtered and noisy outputs, or accumulated graph faults.
    """
    fireflies, _ = SpatialDenoise.__struct_fields__
    _, value = Gated.__struct_fields__
    guides, prefilter, quality = structs.fields(Filter)
    joint = Guides.ALBEDO_NORMAL.label
    image = group_input(COLOR_INPUT.key, "RGBA")
    level = working.placed({COLOR_INPUT.key: image}, LUMINANCE.key)
    grid = gridded(image)
    ring = tuple((float(dx), float(dy), 0.0) for dy in range(-2, 3) for dx in range(-2, 3) if max(abs(dx), abs(dy)) == 2)
    count, rank, lit = len(ring), 4, 0.0001
    pairs = tuple(
        (i + j, i + j + k)
        for p in (1 << exponent for exponent in range(count.bit_length() - 1))
        for k in (p >> shift for shift in range(p.bit_length()))
        for j in range(k % p, count - k, 2 * k)
        for i in range(min(k, count - j - k))
        if (i + j) // (2 * p) == (i + j + k) // (2 * p)
    )

    def tap(offset: tuple[float, float, float]) -> tuple[Expr, Expr]:
        uv = grid.at(offset, grid.inverse)
        sampled = Expr(kind=bpy.types.CompositorNodeMapUV, inputs=(("Image", level), ("UV", uv), ("Interpolation", "Nearest"), ("Extension X", "Clip"), ("Extension Y", "Clip")), socket="Image")
        return sampled, (uv["X"] > 0.0) * (uv["X"] < 1.0) * ((uv["Y"] > 0.0) * (uv["Y"] < 1.0))

    def exchanged(wires: tuple[tuple[Expr, Expr], ...], pair: tuple[int, int]) -> tuple[tuple[Expr, Expr], ...]:
        low, high = pair
        (left, left_valid), (right, right_valid) = wires[low], wires[high]
        choose = (left_valid > right_valid) + math("COMPARE", left_valid, right_valid, 0.0) * (left > right)
        moved = {low: (where(choose, right, left), math("MINIMUM", left_valid, right_valid)), high: (where(choose, left, right), math("MAXIMUM", left_valid, right_valid))}
        return tuple(starmap(moved.get, enumerate(wires)))

    taps = tuple(map(tap, ring))
    fourth, _ = functools.reduce(exchanged, pairs, taps)[count - rank]
    valid = sum((held for _, held in taps[1:]), taps[0][1])
    marked = (level > fourth * group_input(item_key(fireflies, value))) * (1.0 - (level < lit)) * (valid > float(rank))
    cleaned = switched(group_input(fireflies), image, mixed(1.0, image, where(marked, fourth / level, 1.0), blend="MULTIPLY"))
    albedo = menu(group_input(guides.name), MenuItems.of(Guides), {Guides.ALBEDO.label: group_input(_ALBEDO.key, "RGBA"), joint: group_input(_ALBEDO.key, "RGBA")}, kind="RGBA")
    normal = switched(structs.replace(albedo, socket=joint, shape="FLOAT"), (0.0, 0.0, 0.0), group_input(_NORMAL.key, "VECTOR"))
    filtered = Expr(
        kind=bpy.types.CompositorNodeDenoise,
        inputs=(("Image", cleaned), ("Albedo", albedo), ("Normal", normal), ("HDR", True), ("Prefilter", group_input(prefilter.name)), ("Quality", group_input(quality.name))),
        socket="Image",
        shape="RGBA",
    )

    def assembled(filters: Interface, entry: Interface) -> BlenderResult[NodeGroup]:
        face = Interface(outputs=(COLOR_RESULT, _NOISY), inputs=(COLOR_INPUT, _ALBEDO, _NORMAL, *filters.inputs), panels=tuple(panel for panel in entry.panels if panel.key == fireflies))
        return NodeGroup.built(key="denoiser", name="Denoiser", interface=face, outputs={COLOR_RESULT.key: filtered, _NOISY.key: cleaned})

    return paired(Interface.of(Filter), Interface.of(SpatialDenoise)).bind(lambda faces: assembled(*faces))


def unit(group: NodeGroup, path: KeyPath, socket: Callable[[Key], Scalar], source: Scalar, pair: Pair) -> Expr:
    """Places the denoiser group over a source with the filter controls at a record path, its guides, and the firefly controls."""
    fireflies, _ = SpatialDenoise.__struct_fields__
    _, value = Gated.__struct_fields__
    controls = {field.name: socket(item_key(*path, field.name)) for field in structs.fields(Filter)} | {held: socket(held) for held in (fireflies, item_key(fireflies, value))}
    return group.placed({COLOR_INPUT.key: source, _ALBEDO.key: pair.albedo, _NORMAL.key: pair.normal} | controls, COLOR_RESULT.key)


def gated(source: Scalar, filtered: Expr, gate: Gate) -> Expr:
    """Mixes a denoised unit over its firefly-cleaned input by amount, the source passing through when disabled."""
    return switched(gate.enabled, source, mixed(gate.amount, structs.replace(filtered, socket=_NOISY.key), filtered))


def denoised(shell: Shell[SpatialDenoise]) -> BlenderResult[Expr]:
    """Builds every filter unit and selects the target, the frame unit's cleaned input as the mix baseline.

    Args:
        shell: Entry inputs, target menus, working config, and render-layer endpoints.

    Returns:
        Target image keeping the cleaned frame baseline and the other filter units, or accumulated graph faults.
    """
    paths, pair = Filters.of(), Pair.of(shell.layer, Nothing)

    def built(group: NodeGroup) -> Expr:
        frame, *rest = (unit(group, path, group_input, shell.color, pair).named(item_key(*path)) for path in paths.filters)
        target = shell.menu(*paths.target, cases={Frame.title: frame, Passes.title: shell.color, Groups.title: shell.color}, kind="RGBA").named(item_key(*paths.target))
        baseline = switched(structs.replace(target, socket=Frame.title, shape="FLOAT"), shell.color, structs.replace(frame, socket=_NOISY.key)).named(_FRAME_BASE.node)
        return target.keeping(baseline, *rest)

    return luminance(shell.config).bind(denoiser).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

spatial_denoise = CompositorEffect(
    record=SpatialDenoise,
    graph=denoised,
    mix_base=Some(_FRAME_BASE),
    requested=Some(lambda _record, layer: guided(layer)),
    holds={"cycles.use_denoising": False},
    inspected=(Inspected(key=item_key(SpatialDenoise, "frame"), end=End(node=item_key(*Filters.of().frame), socket=COLOR_RESULT.key), description="Frame filter output before target selection"),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["ENGINE", "MOTION_BLUR", "SETTINGS", "VECTOR_PASS", "Filters", "Gate", "Pair", "Plane", "Rendered", "denoised", "denoiser", "filing", "gated", "guided", "motioned", "unit"]
