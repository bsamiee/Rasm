"""Render noise removal: firefly suppression and guided Open Image Denoise over the frame, light passes, or light groups, and the motion-warped temporal denoise job after a render."""

from collections.abc import Callable
import functools
from itertools import starmap
from pathlib import Path
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.collections import Block
import msgspec

from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.paths import RnaPath
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events import Handler
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.nodes import functions
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.content import ImageFrames, Loaded
from blender.bpy.nodes.desired import End, Interface, InterfacePanel, item_key, MenuItem, MenuItems, NodeGroup, Socket, under
from blender.bpy.nodes.graphs import Component, Expr, group_input, math, menu, mixed, Scalar, switched, where, xyz
from blender.bpy.render.compositor import filters
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT, with_alpha
from blender.bpy.render.compositor.coordinates import PixelGrid
from blender.bpy.render.compositor.effects import Blended, CompositorEffect
from blender.bpy.render.compositor.inputs import Holds, Shell
from blender.bpy.render.compositor.layers import Source, StackLayer, TermFiltering
from blender.bpy.render.compositor.masks import BLEND, LUMINANCE, OPACITY, working
from blender.bpy.render.compositor.recombination import Lobes, Recombined, TermFilter
from blender.bpy.render.outputs.paths import ScenePaths
from blender.bpy.render.outputs.results import COMBINED, Written
from blender.bpy.render.sessions.plans import JobScope, Plan
from blender.bpy.render.view_layers.passes import Family, PassFlags, PassRequest, required, used
from blender.bpy.render.view_layers.routing import PassSet
from blender.bpy.settings.attachments import Attachment
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

from ...chain.stages import Effect, Stage

# --- [CONSTANTS] ------------------------------------------------------------------------

ENGINE: Final = Descriptor.declared(bpy.types.Scene, ("render", "engine"), bpy.types.EnumProperty)
VECTOR_PASS: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_vector",), bpy.types.BoolProperty)
TEMPORAL: Final[Key] = "temporal_denoise"
_NOISY: Final = Socket(key="noisy", name="Filter Input", kind=bpy.types.NodeSocketColor)
_ALBEDO: Final = Socket(key="albedo", name="Albedo", kind=bpy.types.NodeSocketColor)
_NORMAL: Final = Socket(key="normal", name="Normal", kind=bpy.types.NodeSocketVector)
_RING: Final = tuple((float(dx), float(dy)) for dy in range(-2, 3) for dx in range(-2, 3) if max(abs(dx), abs(dy)) == 2)
GUIDES: Final = MenuItems(items=(MenuItem(name="None"), MenuItem(name="Albedo"), MenuItem(name="Albedo and Normal")))
_FRAME_TARGET: Final = MenuItem(name="Frame", description="Incoming frame denoised as one image")
_PASSES_TARGET: Final = MenuItem(name="Light Passes", description="Light terms of a recombined layer filtered before they sum, one filter per recombined term")
_GROUPS_TARGET: Final = MenuItem(name="Light Groups", description="Light-group passes of a recombined layer filtered before they sum")
TARGETS: Final = MenuItems(items=(_FRAME_TARGET, _PASSES_TARGET, _GROUPS_TARGET))
_TARGET: Final = Socket(key="target", kind=bpy.types.NodeSocketMenu, description="Frame or recombined light terms to denoise", default_value=_FRAME_TARGET.name, items=Some(TARGETS))
_GUIDES: Final = Socket(key="guides", kind=bpy.types.NodeSocketMenu, description="Passes guiding filter", default_value="Albedo and Normal", items=Some(GUIDES))
_PREFILTER: Final = Socket(
    key="prefilter",
    kind=bpy.types.NodeSocketMenu,
    description="Guide denoising before filtering",
    default_value="Accurate",
    items=Some(MenuItems(items=(MenuItem(name="None"), MenuItem(name="Fast"), MenuItem(name="Accurate")))),
)
_QUALITY: Final = Socket(
    key="quality",
    kind=bpy.types.NodeSocketMenu,
    description="Quality against run time",
    default_value="Follow Scene",
    items=Some(MenuItems(items=(MenuItem(name="Follow Scene"), MenuItem(name="High"), MenuItem(name="Balanced"), MenuItem(name="Fast")))),
)
FILTER: Final = Interface(inputs=(_GUIDES, _PREFILTER, _QUALITY))
_FIREFLIES_THRESHOLD: Final = Socket(
    key="fireflies_threshold", name="Threshold", kind=bpy.types.NodeSocketFloat, description="Luminance over fourth brightest ring neighbor", min_value=1.0, max_value=10.0, default_value=2.0
)
FIREFLIES: Final = InterfacePanel(key="fireflies", name="Fireflies", description="Firefly suppression before filtering", toggle=Some(True), inputs=(_FIREFLIES_THRESHOLD,))

# --- [MODELS] ---------------------------------------------------------------------------

# --- [SPATIAL]


class SpatialDenoise(Effect, frozen=True, kw_only=True):
    """Guided spatial denoising after optional firefly suppression."""

    stage = Stage.DENOISE
    pointwise = False


# --- [TEMPORAL]


class TemporalDenoise(bpy.types.PropertyGroup):
    """Scene setting of the job blending motion-warped neighboring frames by color similarity after each render."""

    enabled: bpy.props.BoolProperty(name="Enabled", description="Queue a temporal denoise job after each render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    hue: bpy.props.FloatProperty(name="Hue Threshold", description="Hue distance zeroing neighbor weight", min=0.0, default=0.1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    saturation: bpy.props.FloatProperty(name="Saturation Threshold", description="Chroma distance zeroing neighbor weight", min=0.0, default=0.05)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    value: bpy.props.FloatProperty(name="Value Threshold", description="Value distance zeroing neighbor weight", min=0.0, default=0.05)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    max_weight: bpy.props.FloatProperty(name="Max Weight", description="Largest share per neighbor frame", min=0.0, max=1 / 3, default=0.3)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    tolerance: bpy.props.FloatProperty(name="Motion Tolerance", description="Allowed reverse-motion error in pixels", min=0.0, default=1.0)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Rendered(bpy.types.PropertyGroup):
    """Written color and pixel-motion sequences of the render a temporal denoise job reads, paths relative to the output root, empty outside the job's file."""

    ownership = Ownership(kind=bpy.types.CompositorNodeTree, key=TEMPORAL, saved=Saved.USED)
    color_path: bpy.props.StringProperty(name="Color Path", description="Color sequence file of the first frame")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    color_name: bpy.props.StringProperty(name="Color Output", description="Image output holding color")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    motion_path: bpy.props.StringProperty(name="Motion Path", description="Pixel-motion sequence file of the first frame")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    motion_name: bpy.props.StringProperty(name="Motion Output", description="Image output holding pixel motion")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    first: bpy.props.IntProperty(name="First", description="First rendered frame", default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    last: bpy.props.IntProperty(name="Last", description="Last rendered frame", default=1)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class TemporalGroups(msgspec.Struct, frozen=True, kw_only=True):
    """Load handler building the temporal reconstruction group in a job file whose scene holds the sequences of a finished render."""

    settings: Attachment[TemporalDenoise]
    rendered: Attachment[Rendered]

    def built(self, identity: Identity) -> Outcome[None]:
        """Builds the temporal group of every scene holding rendered sequences, under its stored output root, the stack's pinned head placing it.

        Each neighbor warps along its motion pass, keeps the share its reverse motion and HSV distance from the current frame allow, and
        mixes in by that share inside the rendered range.

        Returns:
            `Ok` once every group is made, or graph faults.
        """

        def grouped(held: Rendered, record: TemporalDenoise, base: Path) -> Outcome[NodeGroup]:
            def loaded(path: str, name: str, offset: int) -> Expr:
                frames = ImageFrames(frame_start=held.first, frame_duration=held.last - held.first + 1, frame_offset=held.first - 1 + offset)
                return Expr(kind=bpy.types.CompositorNodeImage, content=Some(Loaded(path=base / path, tag=Tag(sequence=True), frames=Some(frames))), socket=name, shape=bpy.types.NodeSocketColor)

            def hsv(image: Expr, channel: str) -> Expr:
                return Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", image),), members={"mode": "HSV"}, socket=channel)

            def mapped(image: Expr, uv: Expr, interpolation: str) -> Expr:
                return Expr(
                    kind=bpy.types.CompositorNodeMapUV,
                    inputs=(("Image", image), ("UV", uv), ("Interpolation", interpolation), ("Extension X", "Clip"), ("Extension Y", "Clip")),
                    socket="Image",
                    shape=bpy.types.NodeSocketColor,
                )

            color, motion = loaded(held.color_path, held.color_name, 0), loaded(held.motion_path, held.motion_name, 0)
            grid = PixelGrid.of(color)
            time = Expr(kind=bpy.types.CompositorNodeSceneTime, socket="Frame")
            current_value = hsv(color, "Blue")
            current_chroma = hsv(color, "Green") * current_value

            def side(offset: int, toward: tuple[Component, Component], back: tuple[Component, Component], scale: Expr, inside: Expr) -> tuple[Expr, Expr]:
                uv = grid.at(xyz(*(motion[channel] for channel in toward)), scale)
                warped, moved = mapped(loaded(held.color_path, held.color_name, offset), uv, "Bilinear"), mapped(loaded(held.motion_path, held.motion_name, offset), uv, "Nearest")
                agree = functools.reduce(lambda kept, axis: kept * axis, (1.0 - (abs(moved[reverse] - motion[forward]) > record.tolerance) for reverse, forward in zip(back, toward)))
                spread = abs(hsv(color, "Red") - hsv(warped, "Red"))
                distances = (
                    (math("MINIMUM", spread, 1.0 - spread), record.hue),
                    (abs(current_chroma - hsv(warped, "Green") * hsv(warped, "Blue")), record.saturation),
                    (abs(current_value - hsv(warped, "Blue")), record.value),
                )
                alike = functools.reduce(lambda kept, weight: kept * weight, (math("DIVIDE", threshold - distance, threshold, clamp=True) for distance, threshold in distances))
                return switched(inside, 0.0, where(agree, alike, 0.0) * record.max_weight), switched(inside, color, warped)

            previous_weight, previous = side(-1, ("Red", "Green"), ("Blue", "Alpha"), grid.inverse, time > float(held.first))
            next_weight, following = side(1, ("Blue", "Alpha"), ("Red", "Green"), grid.inverse * (-1.0, -1.0, -1.0), time < float(held.last))
            total = previous_weight + next_weight
            blend = mixed(total, color, mixed(next_weight / total, previous, following))
            restored = with_alpha(blend, hsv(color, "Alpha"))
            return functions.built(Rendered.ownership.key, Interface(outputs=(COLOR_RESULT,)), {COLOR_RESULT.key: restored}, bpy.types.CompositorNodeTree, name=Some(bpy.path.display_name(TEMPORAL)))

        def staged(scene: bpy.types.Scene) -> Outcome[None]:
            return paired(self.rendered.read(identity, scene), self.settings.read(identity, scene), ScenePaths().bound(identity).bind(lambda pathed: pathed.scope.read(scene))).bind(
                lambda parts: (
                    parts[2]
                    .filter(lambda _: bool(parts[0].color_path))
                    .map(lambda path: grouped(parts[0], parts[1], path.base).bind(lambda group: made(group, identity, of=Some(scene))).map(lambda _: None))
                    .default_value(Ok(None))
                )
            )

        return accumulated(map(staged, bpy.data.scenes)).map(lambda _: None)

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns the load handler building temporal groups."""
        return Ok((Handler(handlers=bpy.app.handlers.load_post, body=functools.partial(self.built, identity)),))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SPATIAL]


def _denoiser(luminance: NodeGroup) -> Outcome[NodeGroup]:
    """Builds the shared firefly and guided denoise unit.

    Fireflies scale toward the fourth brightest valid luminance on the 5x5 ring.

    Args:
        luminance: Working luminance group with image input and luminance output.

    Returns:
        Group with filtered and firefly-cleaned outputs, or graph faults.
    """
    image = group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor)
    level = functions.placed(luminance, {COLOR_INPUT.key: image}, LUMINANCE.key)
    rank, lit = 4, 0.0001
    fourth, valid = filters.ranked(level, _RING, rank)
    marked = (level > fourth * group_input(_FIREFLIES_THRESHOLD.key)) * (1.0 - (level < lit)) * (valid > float(rank))
    cleaned = switched(group_input(FIREFLIES.key), image, mixed(1.0, image, where(marked, fourth / level, 1.0), blend="MULTIPLY"))
    albedo = menu(group_input(_GUIDES.key), {GUIDES.items[0]: (0.8, 0.8, 0.8, 1.0)} | {item: group_input(_ALBEDO.key, bpy.types.NodeSocketColor) for item in GUIDES.items[1:]})
    normal = switched(msgspec.structs.replace(albedo, socket="Albedo and Normal", shape=bpy.types.NodeSocketFloat), (0.0, 0.0, 0.0), group_input(_NORMAL.key, bpy.types.NodeSocketVector))
    filtered = Expr(
        kind=bpy.types.CompositorNodeDenoise,
        inputs=(("Image", cleaned), ("Albedo", albedo), ("Normal", normal), ("HDR", True), ("Prefilter", group_input(_PREFILTER.key)), ("Quality", group_input(_QUALITY.key))),
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    )
    face = Interface(outputs=(COLOR_RESULT, _NOISY), inputs=(COLOR_INPUT, _ALBEDO, _NORMAL, *FILTER.inputs), panels=(FIREFLIES,))
    return functions.built("denoiser", face, {COLOR_RESULT.key: filtered, _NOISY.key: cleaned}, bpy.types.CompositorNodeTree)


def _guides(render: Expr, family: Option[Family]) -> tuple[Expr, Expr]:
    """Cycles denoising albedo and normal of a render layer, a family's color pass as the albedo of its term."""
    albedo = family.bind(lambda held: Block.of_seq(sorted(flag.prop.name for flag in PassFlags.of().families[held][2])).try_head()).default_value("Denoising Albedo")
    return msgspec.structs.replace(render, socket=albedo, shape=bpy.types.NodeSocketColor), msgspec.structs.replace(render, socket="Denoising Normal", shape=bpy.types.NodeSocketVector)


def _unit(group: NodeGroup, read: Callable[[Key], Scalar], filter_at: Key, source: Scalar, guides: tuple[Expr, Expr]) -> Expr:
    """Places the denoise unit over a source, its filter controls read under `filter_at` and the firefly controls at the record root."""
    albedo, normal = guides
    controls = {socket.key: read(item_key(filter_at, socket.key)) for socket in FILTER.inputs} | {key: read(key) for key in (FIREFLIES.key, *(socket.key for socket in FIREFLIES.inputs))}
    return functions.placed(group, {COLOR_INPUT.key: source, _ALBEDO.key: albedo, _NORMAL.key: normal} | controls, COLOR_RESULT.key)


def _guided(_record: SpatialDenoise, layer: str) -> Outcome[PassRequest]:
    """Requests Cycles denoising guide passes under the Cycles engine."""
    return Descriptor.resolve(bpy.types.ViewLayer, ("cycles", "denoising_store_passes"), bpy.types.BoolProperty).map(
        lambda flag: PassRequest(layer=layer, flags=frozenset({flag}), condition=Some(Setting(descriptor=ENGINE, accepted=("CYCLES",))))
    )


def denoised(shell: Shell[SpatialDenoise]) -> Outcome[Expr]:
    """Filters the frame under its target, the light-term targets passing the stage input to the layer that filters its terms."""

    def built(group: NodeGroup) -> Expr:
        frame = _unit(group, group_input, "frame", shell.color, _guides(shell.render, Nothing)).named("target_frame")
        chosen = shell.menu(_TARGET, cases={_FRAME_TARGET: frame, _PASSES_TARGET: shell.color, _GROUPS_TARGET: shell.color}).named("target")
        return chosen.keeping(
            switched(msgspec.structs.replace(chosen, socket=_FRAME_TARGET.name, shape=bpy.types.NodeSocketFloat), shell.color, msgspec.structs.replace(frame, socket=_NOISY.key)).named("frame_base")
        )

    return working(shell.config).bind(_denoiser).map(built)


# --- [LAYERS]


def _term_filter(layer: StackLayer[Source, Effect], _outer: Callable[[Key], Expr], luminance: NodeGroup) -> Outcome[Option[TermFilter]]:
    """Filters a recombined layer's terms through its denoise effect while the effect targets light passes or light groups."""
    found = next((index for index, held in enumerate(layer.effects, 1) if isinstance(held.record, SpatialDenoise)), None)

    def filtered(group: NodeGroup, index: int) -> TermFilter:
        prefix = item_key(layer.key, "effects", index)

        def read(key: Key) -> Expr:
            return group_input(item_key(prefix, key))

        chosen = menu(read("target"), dict.fromkeys(TARGETS.items, 0.0))

        def term(key: Key, source: Expr, family: Option[Family], render: Expr) -> Expr:
            match layer.source:
                case Recombined(recombination=Lobes()):
                    at, active = item_key("passes", key), msgspec.structs.replace(chosen, socket="Light Passes") * read(item_key("passes", key))
                case _:
                    at, active = "groups", msgspec.structs.replace(chosen, socket="Light Groups")
            cleaned = _unit(group, read, at, source, _guides(render, family))
            return switched(active, source, mixed(read(item_key(BLEND, OPACITY.key)), msgspec.structs.replace(cleaned, socket=_NOISY.key), cleaned))

        return term

    return Ok(Nothing) if found is None else _denoiser(luminance).map(lambda group: Some(filtered(group, found)))


def _term_passes(layer: StackLayer[Source, Effect]) -> Outcome[tuple[PassRequest, ...]]:
    """Requests guide passes of a recombined layer holding a denoise effect."""
    match layer.source:
        case Recombined(layer=name) if any(isinstance(held.record, SpatialDenoise) for held in layer.effects):
            return _guided(SpatialDenoise(), name).map(lambda request: (request,))
        case _:
            return Ok(())


# --- [TEMPORAL]


def temporal(identity: Identity, plan: Plan) -> Outcome[Plan]:
    """Appends a temporal denoise job after each scope whose scene asks for one.

    The scope's render writes the vector pass without motion blur; the job's file mutes every compositor entry, holds the rendered color and
    motion sequences for the temporal group its load builds, and writes its frames one output version later.

    Args:
        identity: Extension holding the temporal settings and the scene output settings.
        plan: Planned job scopes.

    Returns:
        Plan with the base scopes revised and one job appended per wanting scope, or settings, path, and pass faults.
    """
    day = plan.moment.astimezone().date()

    def followed(index: int, scope: JobScope) -> Outcome[tuple[JobScope, Option[JobScope]]]:
        scene = bpy.data.scenes[scope.scene]
        requests = tuple(PassRequest(layer=layer.name, flags=frozenset({VECTOR_PASS})) for layer in used(scene))

        def located(files: tuple[Written, ...], source: str) -> Outcome[tuple[str, str]]:
            return (
                Block
                .of_seq((written.path, item) for written in files for item, held in written.passes.items() if held == source)
                .try_head()
                .to_result_with(functools.partial(Fault.of, Reason.ABSENT, repr(scene), member=source))
            )

        def planned(rendered: str, outputs: str, version: int, base: Path, color: tuple[str, str], motion: tuple[str, str]) -> tuple[JobScope, Option[JobScope]]:
            values: dict[RnaPath, Value] = {
                (rendered, "color_path"): Path(color[0]).relative_to(base).as_posix(),
                (rendered, "color_name"): color[1],
                (rendered, "motion_path"): Path(motion[0]).relative_to(base).as_posix(),
                (rendered, "motion_name"): motion[1],
                (rendered, "first"): scene.frame_start,
                (rendered, "last"): scene.frame_end,
                (outputs, "version"): version + 1,
                **{("compositor_effects", position, "enable_for_render"): False for position in range(len(scene.compositor_effects))},
            }
            blur = Snapshot.of(bpy.types.Scene, {("render", "use_motion_blur"): False})
            return (
                msgspec.structs.replace(scope, values=Option.of_optional(scope.values).map(lambda held: msgspec.structs.replace(held, tree=held.tree.merged(blur.tree))).default_value(blur)),
                Some(JobScope(scene=scope.scene, camera=scope.camera, layer=scope.layer, values=Snapshot.of(bpy.types.Scene, values), prerequisites=(index,))),
            )

        def wanted(settings: TemporalDenoise) -> Outcome[tuple[JobScope, Option[JobScope]]]:
            if not settings.enabled:
                return Ok((scope, Nothing))
            paths = paired(ScenePaths().bound(identity), PassSet.bound(identity), RENDERED.bound(identity), ScenePaths.attachment.bound(identity))
            return paths.bind(
                lambda bound: (
                    bound[0]
                    .path(scene, day)
                    .bind(lambda path: required(scene, bpy.context, requests).bind(lambda _: bound[1].reconciled(scene, bpy.context, path)).map(lambda _: path.copied))
                    .bind(
                        lambda copied: (
                            PassSet()
                            .written(scene, scene.frame_start, identity, path=Some(copied))
                            .bind(lambda files: paired(located(files, COMBINED), located(files, VECTOR_PASS.prop.name)))
                            .map(lambda planes: planned(bound[2].name, bound[3].name, bound[3].struct(scene).version, copied.base, *planes))
                        )
                    )
                )
            )

        return SETTINGS.read(identity, scene).bind(wanted)

    return accumulated(starmap(followed, enumerate(plan.scopes))).map(
        lambda pairs: msgspec.structs.replace(plan, scopes=(*(base for base, _ in pairs), *(follow for _, held in pairs for follow in held.to_list())))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

spatial_denoise: Final = CompositorEffect(
    denoised,
    record=SpatialDenoise,
    interface=Interface(
        inputs=(_TARGET,),
        panels=(
            FIREFLIES,
            InterfacePanel(key="frame", name="Frame", description="Filter over frame", inputs=under("frame", FILTER).inputs),
            InterfacePanel(
                key="passes",
                name="Light Passes",
                description="Filter per recombined light term",
                panels=tuple(
                    InterfacePanel(key=item_key("passes", key), name=name, description=description, toggle=Some(enabled), inputs=under(item_key("passes", key), FILTER).inputs)
                    for key, name, description, enabled in (
                        ("diffuse", "Diffuse", "Filter over diffuse light", True),
                        ("glossy", "Glossy", "Filter over glossy light", True),
                        ("transmission", "Transmission", "Filter over transmitted light", True),
                        ("volume", "Volume", "Filter over volume light", True),
                        ("emission", "Emission", "Filter over emitters seen by camera", False),
                        ("environment", "Environment", "Filter over background seen by camera", False),
                        ("occlusion", "Ambient Occlusion", "Filter over occlusion pass", False),
                    )
                ),
            ),
            InterfacePanel(key="groups", name="Light Groups", description="Filter per light group pass", inputs=under("groups", FILTER).inputs),
        ),
    ),
    mix=Blended(base=Some(End(node="frame_base", socket="Output"))),
    request=Some(_guided),
    holds=Holds(scene=Snapshot.of(bpy.types.Scene, {("cycles", "use_denoising"): False})),
)
SETTINGS: Final = Attachment(key=TEMPORAL, form=TemporalDenoise, owner=bpy.types.Scene)
RENDERED: Final = Attachment(key="temporal_inputs", form=Rendered, owner=bpy.types.Scene)
prerequisites: Final = (SETTINGS, RENDERED, TemporalGroups(settings=SETTINGS, rendered=RENDERED))
filtering: Final = TermFiltering[Effect](filter=_term_filter, requests=_term_passes)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "ENGINE",
    "FILTER",
    "FIREFLIES",
    "GUIDES",
    "RENDERED",
    "SETTINGS",
    "TARGETS",
    "TEMPORAL",
    "VECTOR_PASS",
    "Rendered",
    "SpatialDenoise",
    "TemporalDenoise",
    "TemporalGroups",
    "filtering",
    "prerequisites",
    "spatial_denoise",
    "temporal",
]
