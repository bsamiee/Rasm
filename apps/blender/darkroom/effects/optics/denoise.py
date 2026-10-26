"""Render noise removal: firefly suppression and guided Open Image Denoise over the frame, light passes, or light groups, and the motion-warped temporal denoise job after a render."""

from collections.abc import Callable
import functools
from pathlib import Path
from typing import Annotated, ClassVar, Final, Literal

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.data.ids.images import ImageFrames, Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.graphs import Component, End, Expr, group_input, Loaded, math, menu, mixed, NodeGroup, Scalar, switched, where, xyz
from blender.bpy.nodes.interface import Interface, item_key, MenuItems, Socket
from blender.bpy.render.compositor import filters
from blender.bpy.render.compositor.blending import Blend
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT
from blender.bpy.render.compositor.coordinates import gridded
from blender.bpy.render.compositor.effects import Blended, CompositorEffect, Enabled, Holds, Inspects, Requests, Shell
from blender.bpy.render.compositor.layers import Layer, Source, TermFiltering
from blender.bpy.render.compositor.masks import LUMINANCE, luminance
from blender.bpy.render.compositor.recombination import Lobes, Recombined, TermFilter
from blender.bpy.render.outputs.exports import COMBINED
from blender.bpy.render.outputs.finishing import Written
from blender.bpy.render.outputs.paths import OutputPath
from blender.bpy.render.sessions.queue import Followup
from blender.bpy.render.view_layers.passes import Family, PassRequest, PassSet
from blender.bpy.settings.attachments import Attachment
from blender.records.faults import accumulated, paired
from blender.records.fields import Case, Choice, Gated, heading, NonNegative, Presentation
from blender.rendering.batch.queue import Frames

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------


class Guides(Choice):
    """Render passes guiding a spatial filter."""

    NONE = 1
    ALBEDO = 2
    ALBEDO_NORMAL = 3, "Albedo and Normal"


type Target = Frame | Passes | Groups

# --- [CONSTANTS] ------------------------------------------------------------------------

ENGINE: Final = Descriptor.declared(bpy.types.Scene, "render.engine", bpy.types.EnumProperty)
VECTOR_PASS: Final = Descriptor.declared(bpy.types.ViewLayer, "use_pass_vector", bpy.types.BoolProperty)
MOTION_BLUR: Final = Descriptor.declared(bpy.types.Scene, "render.use_motion_blur", bpy.types.BoolProperty)
_FRAME_BASE: Final = End(node="frame_base", socket="Output")
_NOISY: Final = Socket(key="noisy", name="Filter Input", kind=bpy.types.NodeSocketColor)
_ALBEDO: Final = Socket(key="albedo", name="Albedo", kind=bpy.types.NodeSocketColor)
_NORMAL: Final = Socket(key="normal", name="Normal", kind=bpy.types.NodeSocketVector)
_RING: Final = tuple((float(dx), float(dy)) for dy in range(-2, 3) for dx in range(-2, 3) if max(abs(dx), abs(dy)) == 2)
_RANK: Final = 4
TEMPORAL: Final[Key] = "temporal_denoise"
_LIT: Final = 0.0001

# --- [MODELS] ---------------------------------------------------------------------------

# --- [SPATIAL]


class Filter(msgspec.Struct, frozen=True, kw_only=True):
    """Open Image Denoise filter over color input, its menus under the Denoise node's item names."""

    guides: Annotated[Guides, msgspec.Meta(description="Passes guiding filter")] = Guides.ALBEDO_NORMAL
    prefilter: Annotated[Literal["None", "Fast", "Accurate"], msgspec.Meta(description="Guide denoising before filtering")] = "Accurate"
    quality: Annotated[Literal["Follow Scene", "High", "Balanced", "Fast"], msgspec.Meta(description="Quality against run time")] = "Follow Scene"


class Frame(Case, frozen=True, kw_only=True):
    """Incoming frame denoised as one image."""

    filter: Annotated[Filter, msgspec.Meta(description="Filter over frame")] = Filter()


class Passes(Case, frozen=True, kw_only=True):
    """Light terms of a recombined layer filtered before they sum, one filter per recombined term."""

    title = "Light Passes"
    diffuse: Annotated[Gated[Filter], msgspec.Meta(description="Filter over diffuse light")] = Gated(enabled=True, value=Filter())
    glossy: Annotated[Gated[Filter], msgspec.Meta(description="Filter over glossy light")] = Gated(enabled=True, value=Filter())
    transmission: Annotated[Gated[Filter], msgspec.Meta(description="Filter over transmitted light")] = Gated(enabled=True, value=Filter())
    volume: Annotated[Gated[Filter], msgspec.Meta(description="Filter over volume light")] = Gated(enabled=True, value=Filter())
    emission: Annotated[Gated[Filter], msgspec.Meta(description="Filter over emitters seen by camera")] = Gated(enabled=False, value=Filter())
    environment: Annotated[Gated[Filter], msgspec.Meta(description="Filter over background seen by camera")] = Gated(enabled=False, value=Filter())
    occlusion: Annotated[Gated[Filter], msgspec.Meta(title="Ambient Occlusion", description="Filter over occlusion pass")] = Gated(enabled=False, value=Filter())


class Groups(Case, frozen=True, kw_only=True):
    """Light-group passes of a recombined layer filtered before they sum."""

    title = "Light Groups"
    filter: Annotated[Filter, msgspec.Meta(description="Filter per light group pass")] = Filter()


class SpatialDenoise(Effect, frozen=True, kw_only=True):
    """Guided spatial denoising after optional firefly suppression."""

    stage = Stage.DENOISE
    title = "Denoise"
    pointwise = False
    fireflies: Annotated[
        Gated[Annotated[float, msgspec.Meta(ge=1.0, le=10.0)]], msgspec.Meta(title="Firefly Threshold", description="Luminance over fourth brightest ring neighbor"), Presentation(precision=Some(2))
    ] = Gated(enabled=True, value=2.0)
    target: Annotated[Target, msgspec.Meta(description="Frame or recombined light terms to denoise")] = Frame()


# --- [TEMPORAL]


class TemporalDenoise(msgspec.Struct, frozen=True, kw_only=True):
    """Scene setting of the job blending motion-warped neighboring frames by color similarity after each render."""

    enabled: Annotated[bool, msgspec.Meta(description="Queue a temporal denoise job after each render")] = False
    hue: Annotated[NonNegative, msgspec.Meta(title="Hue Threshold", description="Hue distance zeroing neighbor weight")] = 0.1
    saturation: Annotated[NonNegative, msgspec.Meta(title="Saturation Threshold", description="Chroma distance zeroing neighbor weight")] = 0.05
    value: Annotated[NonNegative, msgspec.Meta(title="Value Threshold", description="Value distance zeroing neighbor weight")] = 0.05
    max_weight: Annotated[float, msgspec.Meta(ge=0.0, le=1 / 3, description="Largest share per neighbor frame")] = 0.3
    tolerance: Annotated[NonNegative, msgspec.Meta(title="Motion Tolerance", description="Allowed reverse-motion error in pixels")] = 1.0


class Plane(msgspec.Struct, frozen=True, kw_only=True):
    """Written sequence path and image output of one plane."""

    path: str
    name: str

    def rooted(self, base: Path) -> "Plane":
        """Plane under an output root."""
        return msgspec.structs.replace(self, path=str(base / self.path))


class Rendered(msgspec.Struct, frozen=True, kw_only=True):
    """Written color and pixel-motion sequences over one inclusive frame range, paths relative to the output root."""

    color: Plane
    motion: Plane
    first: int
    last: int
    ownership: ClassVar[Ownership[bpy.types.CompositorNodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key=TEMPORAL, saved=Saved.USED)

    def group(self, record: TemporalDenoise) -> BlenderResult[NodeGroup]:
        """Builds the temporal reconstruction of the current frame from its sequence neighbors.

        Each neighbor warps along its motion pass, keeps the share its reverse motion and HSV distance from the current frame allow, and
        mixes in by that share inside the rendered range.

        Args:
            record: Color thresholds, reverse-motion tolerance, and neighbor weight bound.

        Returns:
            Compositor sequence group, or graph faults.
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

        def side(offset: int, toward: tuple[Component, Component], back: tuple[Component, Component], scale: Expr, inside: Expr) -> tuple[Expr, Expr]:
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
        next_weight, following = side(1, ("Blue", "Alpha"), ("Red", "Green"), grid.inverse * (-1.0, -1.0, -1.0), time < float(self.last))
        total = previous_weight + next_weight
        blend = mixed(total, color, mixed(next_weight / total, previous, following))
        restored = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", blend), ("Alpha", hsv(color, "Alpha")), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")
        return NodeGroup.built(key=self.ownership.key, name=heading(TemporalDenoise.__name__), interface=Interface(outputs=(COLOR_RESULT,)), outputs={COLOR_RESULT.key: restored})


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SPATIAL]


def _denoiser(working: NodeGroup) -> BlenderResult[NodeGroup]:
    """Builds the shared firefly and guided denoise unit.

    Fireflies scale toward the fourth brightest valid luminance on the 5x5 ring.

    Args:
        working: Working luminance group with image input and luminance output.

    Returns:
        Group with filtered and firefly-cleaned outputs, or graph faults.
    """
    fireflies, _ = SpatialDenoise.__struct_fields__
    _, value = Gated.__struct_fields__
    guides, prefilter, quality = Filter.__struct_fields__
    image = group_input(COLOR_INPUT.key, "RGBA")
    level = working.placed({COLOR_INPUT.key: image}, LUMINANCE.key)
    fourth, valid = filters.ranked(level, _RING, _RANK)
    marked = (level > fourth * group_input(item_key(fireflies, value))) * (1.0 - (level < _LIT)) * (valid > float(_RANK))
    cleaned = switched(group_input(fireflies), image, mixed(1.0, image, where(marked, fourth / level, 1.0), blend="MULTIPLY"))
    joint = Guides.ALBEDO_NORMAL.label
    albedo = menu(group_input(guides), MenuItems.of(Guides), {Guides.ALBEDO.label: group_input(_ALBEDO.key, "RGBA"), joint: group_input(_ALBEDO.key, "RGBA")}, kind="RGBA")
    normal = switched(msgspec.structs.replace(albedo, socket=joint, shape="FLOAT"), (0.0, 0.0, 0.0), group_input(_NORMAL.key, "VECTOR"))
    filtered = filters.denoised(cleaned, albedo=Some(albedo), normal=Some(normal), prefilter=Some(group_input(prefilter)), quality=Some(group_input(quality)))

    def assembled(controls: Interface, entry: Interface) -> BlenderResult[NodeGroup]:
        face = Interface(outputs=(COLOR_RESULT, _NOISY), inputs=(COLOR_INPUT, _ALBEDO, _NORMAL, *controls.inputs), panels=tuple(panel for panel in entry.panels if panel.key == fireflies))
        return NodeGroup.built(key="denoiser", name="Denoiser", interface=face, outputs={COLOR_RESULT.key: filtered, _NOISY.key: cleaned})

    return paired(Interface.of(Filter), Interface.of(SpatialDenoise)).bind(lambda faces: assembled(*faces))


def _guides(render: Expr, family: Option[Family]) -> tuple[Expr, Expr]:
    """Cycles denoising albedo and normal of a render layer, a family's color pass as the albedo of its term."""
    albedo = family.filter(lambda held: bool(held.color)).map(lambda held: held.socket("color")).default_value("Denoising Albedo")
    return msgspec.structs.replace(render, socket=albedo, shape="RGBA"), msgspec.structs.replace(render, socket="Denoising Normal", shape="VECTOR")


def _unit(group: NodeGroup, read: Callable[[Key], Scalar], filter_at: Key, source: Scalar, guides: tuple[Expr, Expr]) -> Expr:
    """Places the denoise unit over a source, its filter controls read under `filter_at` and the firefly controls at the record root."""
    fireflies, _ = SpatialDenoise.__struct_fields__
    _, value = Gated.__struct_fields__
    albedo, normal = guides
    controls = {name: read(item_key(filter_at, name)) for name in Filter.__struct_fields__} | {held: read(held) for held in (fireflies, item_key(fireflies, value))}
    return group.placed({COLOR_INPUT.key: source, _ALBEDO.key: albedo, _NORMAL.key: normal} | controls, COLOR_RESULT.key)


def denoised(shell: Shell[SpatialDenoise]) -> BlenderResult[Expr]:
    """Filters the frame under its target, the light-term targets passing the stage input to the layer that filters its terms."""
    _, target = SpatialDenoise.__struct_fields__
    (filter_field,) = Frame.__struct_fields__

    def built(group: NodeGroup) -> Expr:
        frame = _unit(group, group_input, item_key(target, Frame, filter_field), shell.color, _guides(shell.render, Nothing)).named(item_key(target, Frame))
        chosen = shell.menu(target, cases={Frame.title: frame, Passes.title: shell.color, Groups.title: shell.color}, kind="RGBA").named(item_key(target))
        baseline = switched(msgspec.structs.replace(chosen, socket=Frame.title, shape="FLOAT"), shell.color, msgspec.structs.replace(frame, socket=_NOISY.key)).named(_FRAME_BASE.node)
        return chosen.keeping(baseline)

    return luminance(shell.config).bind(_denoiser).map(built)


def _guided(_record: SpatialDenoise, layer: str) -> BlenderResult[PassRequest]:
    """Requests Cycles denoising guide passes under the Cycles engine."""
    return Descriptor.resolve(bpy.types.ViewLayer, "cycles.denoising_store_passes", bpy.types.BoolProperty).map(
        lambda flag: PassRequest(layer=layer, flags=frozenset({flag}), condition=Some(Setting(descriptor=ENGINE, accepted=("CYCLES",))))
    )


# --- [LAYERS]


def _term_filter(layer: Layer[Source, Effect], _outer: Callable[[Key], Expr], working: NodeGroup) -> BlenderResult[Option[TermFilter]]:
    """Filters a recombined layer's terms through its denoise effect while the effect targets light passes or light groups."""
    _, target = SpatialDenoise.__struct_fields__
    (groups_filter,) = Groups.__struct_fields__
    _, value = Gated.__struct_fields__
    found = next((index for index, gated in enumerate(layer.effects.value, 1) if isinstance(gated.value.record, SpatialDenoise)), None)

    def filtered(group: NodeGroup, index: int) -> TermFilter:
        prefix = item_key(layer.key, "effects", index)

        def read(key: Key) -> Expr:
            return group_input(item_key(prefix, key))

        chosen = menu(read(item_key(target)), MenuItems.of((Frame, Passes, Groups)), {case.title: 0.0 for case in (Frame, Passes, Groups)}, kind="FLOAT")
        _, opacity, _ = Blend.__struct_fields__

        def term(key: Key, source: Expr, family: Option[Family], render: Expr) -> Expr:
            match layer.source:
                case Recombined(recombination=Lobes()):
                    at, active = item_key(target, Passes, key, value), msgspec.structs.replace(chosen, socket=Passes.title) * read(item_key(target, Passes, key))
                case _:
                    at, active = item_key(target, Groups, groups_filter), msgspec.structs.replace(chosen, socket=Groups.title)
            cleaned = _unit(group, read, at, source, _guides(render, family))
            return switched(active, source, mixed(read(item_key(Shell.blend, opacity)), msgspec.structs.replace(cleaned, socket=_NOISY.key), cleaned))

        return term

    return Ok(Nothing) if found is None else _denoiser(working).map(lambda group: Some(filtered(group, found)))


def _term_passes(layer: Layer[Source, Effect]) -> BlenderResult[tuple[PassRequest, ...]]:
    """Requests guide passes of a recombined layer holding a denoise effect."""
    match layer.source:
        case Recombined(layer=name) if any(isinstance(gated.value.record, SpatialDenoise) for gated in layer.effects.value):
            return _guided(SpatialDenoise(), name).map(lambda request: (request,))
        case _:
            return Ok(())


# --- [TEMPORAL]


def _wanted(identity: Identity, scene: bpy.types.Scene) -> BlenderResult[bool]:
    """Whether the scene asks for the temporal job."""
    return SETTINGS.read(identity, scene).map(lambda record: record.enabled)


def _staged(identity: Identity, scene: bpy.types.Scene, copied: OutputPath, frames: Frames) -> BlenderResult[None]:
    """Stores the rendered color and motion sequences the job reads and stops every compositor effect re-applying over the composited render."""

    def plane(files: tuple[Written, ...], source: str) -> BlenderResult[Plane]:
        return Option.of_optional(
            next((Plane(path=Path(file.path).relative_to(copied.base).as_posix(), name=item) for file in files for item, held in file.passes.items() if held == source), None)
        ).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key=source),))

    rendered = (
        PassSet()
        .written(scene, frames.start, identity, path=Some(copied))
        .bind(lambda files: paired(plane(files, COMBINED), plane(files, VECTOR_PASS.prop.name)))
        .map(lambda planes: Rendered(color=planes[0], motion=planes[1], first=frames.start, last=frames.end))
    )
    muted = accumulated(Enabled.RENDER.descriptor.write(entry, False) for entry in scene.compositor_effects)
    return rendered.bind(lambda held: paired(RENDERED.bound(identity).bind(lambda attached: attached.write(scene, held)), muted)).map(lambda _: None)


def _reloaded(identity: Identity, scene: bpy.types.Scene, base: Path) -> BlenderResult[None]:
    """Builds the temporal group over the rendered sequences under the job's output root, the stack's pinned head placing it."""
    return RENDERED.bound(identity).bind(
        lambda attached: (
            paired(attached.read(scene), SETTINGS.read(identity, scene))
            .bind(lambda parts: msgspec.structs.replace(parts[0], color=parts[0].color.rooted(base), motion=parts[0].motion.rooted(base)).group(parts[1]))
            .bind(lambda group: made(group, identity, of=Some(scene)))
            .map(lambda _: None)
            if attached.stored(scene)
            else Ok(None)
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

SETTINGS: Final = Attachment(key=TEMPORAL, form=TemporalDenoise, owner=bpy.types.Scene)
RENDERED: Final = Attachment(key="temporal_inputs", form=Rendered, owner=bpy.types.Scene)
spatial_denoise = CompositorEffect(
    record=SpatialDenoise,
    graph=denoised,
    mix=Blended(base=Some(_FRAME_BASE)),
    parts=(
        Requests[SpatialDenoise](request=_guided),
        Holds(scene={"cycles.use_denoising": False}),
        Inspects(key=item_key(SpatialDenoise, Frame), end=End(node=item_key("target", Frame), socket=COLOR_RESULT.key), description="Frame filter output before target selection"),
    ),
)
filtering: Final = TermFiltering[Effect](filter=_term_filter, requests=_term_passes)
temporal: Final = Followup(
    key=TEMPORAL,
    wanted=_wanted,
    passes=lambda layer: PassRequest(layer=layer, flags=frozenset({VECTOR_PASS})),
    staged=_staged,
    reloaded=_reloaded,
    settings=(Setting(descriptor=MOTION_BLUR, accepted=(False,)),),
    prerequisites=(SETTINGS, RENDERED),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ENGINE",
    "MOTION_BLUR",
    "RENDERED",
    "SETTINGS",
    "VECTOR_PASS",
    "Filter",
    "Frame",
    "Groups",
    "Guides",
    "Passes",
    "Plane",
    "Rendered",
    "SpatialDenoise",
    "TemporalDenoise",
    "filtering",
    "spatial_denoise",
    "temporal",
]
