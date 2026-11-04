"""Native follow-up planning and prepared pass sequences."""

from collections.abc import Generator
from contextlib import contextmanager
import datetime
from pathlib import Path
from typing import Final

import bpy
from expression import Option, Some
import pytest

from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.functions import built
from blender.bpy.nodes.graphs import group_input
from blender.bpy.nodes.interfaces import Interface
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT
from blender.bpy.render.outputs.paths import Pathed, ScenePaths
from blender.bpy.render.outputs.results import COMBINED
from blender.bpy.render.sessions.followups import FollowedRender, FollowUp, FollowUpSettings, WrittenPass
from blender.bpy.render.sessions.plans import JobScope, Plan
from blender.bpy.render.sessions.queue import QueueRun
from blender.bpy.render.sessions.sets import RenderSet
from blender.bpy.render.sessions.setups import CATEGORIES
from blender.bpy.render.view_layers.routing import RoutingRow
from blender.bpy.render.view_layers.sources import FileSplit
from blender.records.faults import accumulated, Error, Ok, Outcome, Reason
from blender.rendering.batch.queue import MarkedCamera
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [CONSTANTS] ------------------------------------------------------------------------

_VECTOR: Final = Descriptor(root=bpy.types.ViewLayer, prop=bpy.types.ViewLayer.bl_rna.properties["use_pass_vector"])

# --- [MODELS] ---------------------------------------------------------------------------


class FollowupSettings(FollowUpSettings):
    """Follow-up setting counting actual native load callbacks."""

    calls: bpy.props.IntProperty(name="Calls")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CALLBACKS]


def followed(identity: Identity, scene: bpy.types.Scene, render: FollowedRender, settings: FollowupSettings, base: Path) -> Outcome[None]:
    """Records native callback arguments on its scene and advances the setting's load count."""
    settings.calls += 1

    def recorded(passes: tuple[WrittenPass, ...]) -> None:
        """Writes the callback's native values onto its actual scene."""
        scene[identity.identifier(followed.__name__)] = {
            "bounds": (render.first, render.last),
            "enabled": settings.enabled,
            "base": str(base),
            "passes": {name: {"path": written.path, "output": written.output} for name, written in zip(FOLLOWUP.names, passes, strict=True)},
        }

    return accumulated(render.written(name) for name in FOLLOWUP.names).map(recorded)


# --- [SCENE]


@contextmanager
def followup_scope(identity: Identity, folder: Path) -> Generator[tuple[bpy.types.Scene, Plan, Pathed]]:
    """Yields an isolated scene, resolving cleanup targets again after native file loads."""
    preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    original_output = preferences.filepaths.render_output_directory
    original_dirty = preferences.is_dirty
    prior_groups = frozenset(map(IdName.of, bpy.data.node_groups))
    scene = bpy.data.scenes.new("Follow-up Scope")
    scene.use_fake_user = True
    scene_key = IdName.of(scene)
    camera_data = bpy.data.cameras.new("Follow-up Camera")
    camera = bpy.data.objects.new(camera_data.name, camera_data)
    camera_key, camera_data_key = IdName.of(camera), IdName.of(camera_data)
    scene.collection.objects.link(camera)
    scene.camera = camera

    def removed() -> None:
        current_preferences: bpy.types.Preferences = bpy.context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        current_preferences.filepaths.render_output_directory = original_output
        current_preferences.is_dirty = original_dirty
        blocks = (
            *scene_key.resolve(bpy.types.Scene).to_list(),
            *camera_key.resolve(bpy.types.Object).to_list(),
            *camera_data_key.resolve(bpy.types.Camera).to_list(),
            *(group for group in bpy.data.node_groups if IdName.of(group) not in prior_groups),
        )
        bpy.data.batch_remove(ids=blocks)

    try:
        preferences.filepaths.render_output_directory = str(folder)
        scene.frame_start, scene.frame_end, scene.frame_step = 3, 9, 2
        scene.render.resolution_x, scene.render.resolution_y, scene.render.resolution_percentage = 16, 16, 100
        scene.render.use_motion_blur = True
        scene.render.use_compositing = True
        scene.view_layers[0].name = "A"
        selected = scene.view_layers.new("B")
        for layer in scene.view_layers:
            layer.use, layer.use_pass_combined, layer.use_pass_vector = True, True, False

        pathed = ScenePaths().bound(identity).default_with(lambda faults: pytest.fail(str(faults)))
        outputs = pathed.attached.struct(scene)
        outputs.template = "v{version:3}/{layer}/{pass}/{frame:4}"
        outputs.use_variable = False
        outputs.version = 4
        bound_settings = FOLLOWUP.settings.read(identity, scene).default_with(lambda faults: pytest.fail(str(faults)))
        bound_settings.enabled = True
        bound_routing = RoutingRow.bound(identity).default_with(lambda faults: pytest.fail(str(faults)))
        for layer in scene.view_layers:
            bound_routing.routing.struct(layer).split = FileSplit.PASSES.name

        authored = (
            built(
                "queue_followup_authored",
                Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,)),
                {COLOR_RESULT.key: group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor)},
                bpy.types.CompositorNodeTree,
            )
            .bind(lambda group: made(group, identity))
            .default_with(lambda faults: pytest.fail(str(faults)))
        )
        effect = scene.compositor_effects.new(name=authored.name)
        effect.node_group = authored
        effect.enable_for_render = True
        plan = Plan(moment=datetime.datetime.now(datetime.UTC), scopes=(JobScope(scene=scene.name, camera=MarkedCamera(camera=camera.name), layer=selected.name),))
        yield scene, plan, pathed
    finally:
        removed()


@pytest.fixture
def followup_scene(acquired: Acquired, tmp_path: Path) -> Generator[tuple[Identity, bpy.types.Scene, Plan, Pathed]]:
    """Acquires native declarations and releases the scene after each test."""
    with acquired(ROWS) as identity, followup_scope(identity, tmp_path) as (scene, plan, pathed):
        yield identity, scene, plan, pathed


def _metadata(plan: Plan) -> tuple[tuple[str, str | None, tuple[int, ...]], ...]:
    """Returns declared scene, layer, and dependency of each scope."""
    return tuple((scope.scene, scope.layer, scope.prerequisites) for scope in plan.scopes)


# --- [PLANNING]


def test_followup_planning_keeps_native_state(followup_scene: tuple[Identity, bpy.types.Scene, Plan, Pathed]) -> None:
    """Repeated declarations preserve native passes, groups, effects, paths, and held settings."""
    identity, scene, source, pathed = followup_scene
    settings = FOLLOWUP.settings.read(identity, scene).default_with(lambda faults: pytest.fail(str(faults)))

    def state() -> tuple[object, ...]:
        """Returns native values that preparation writes and declaration preserves."""
        return (
            tuple((layer.name, layer.use, layer.use_pass_vector) for layer in scene.view_layers),
            tuple((group.name_full, group.session_uid, len(group.nodes), len(group.links)) for group in bpy.data.node_groups),
            tuple((effect.name, effect.enable_for_render, Option.of_optional(effect.node_group).map(lambda group: group.name_full).to_optional()) for effect in scene.compositor_effects),
            scene.render.filepath,
            scene.render.use_motion_blur,
            (scene.frame_start, scene.frame_end, scene.frame_step),
            (pathed.attached.struct(scene).template, pathed.attached.struct(scene).version),
            (settings.enabled, settings.calls, tuple(scene.keys())),
        )

    before = state()
    first = FOLLOWUP.planned(identity, source).default_with(lambda faults: pytest.fail(str(faults)))
    between = state()
    second = FOLLOWUP.planned(identity, source)
    after = state()
    assert second == Ok(first)
    assert before == between == after
    assert _metadata(first) == ((scene.name, "B", ()), (scene.name, "B", (0,)))
    base = first.scopes[0].values
    assert base is not None
    assert base.apply(scene).outcome == Ok(None)
    assert scene.render.use_motion_blur is False


def test_followup_preparation_records_selected_layer(followup_scene: tuple[Identity, bpy.types.Scene, Plan, Pathed]) -> None:
    """Prepared B scopes read B sequences with their frame bounds and source dependency."""
    identity, scene, source, pathed = followup_scene
    bounds = scene.frame_start, scene.frame_end
    version = pathed.attached.struct(scene).version
    declared = FOLLOWUP.planned(identity, source).default_with(lambda faults: pytest.fail(str(faults)))
    with bpy.context.temp_override(scene=scene):
        prepared = FOLLOWUP.prepared(identity, source).default_with(lambda faults: pytest.fail(str(faults)))
    assert _metadata(prepared) == _metadata(declared)
    assert all(layer.use_pass_vector for layer in scene.view_layers)
    assert RoutingRow.ownership.found(identity, of=Some(scene)).is_some()
    copied = pathed.path(scene, source.moment.astimezone().date()).map(lambda path: path.copied).default_with(lambda faults: pytest.fail(str(faults)))
    files = RoutingRow().written(scene, scene.frame_start, identity, Some(True)).default_with(lambda faults: pytest.fail(str(faults)))
    expected = {
        layer: {name: (Path(written.path).relative_to(copied.base).as_posix(), item) for written in files if written.layer == Some(layer) for item, name in written.passes.items()}
        for layer in ("A", "B")
    }
    follow = prepared.scopes[1]
    assert follow.prerequisites == (0,)
    assert follow.values is not None
    assert follow.values.apply(scene).outcome == Ok(None)
    record = FOLLOWUP.record.read(identity, scene).default_with(lambda faults: pytest.fail(str(faults)))
    assert (record.first, record.last) == bounds
    for name, socket in ((COMBINED, "Image"), (_VECTOR.prop.name, _VECTOR.prop.name)):
        written = record.written(name).default_with(lambda faults: pytest.fail(str(faults)))
        actual = written.path, written.output
        assert actual == expected["B"][socket]
        assert actual != expected["A"][socket]
    assert all(not effect.enable_for_render for effect in scene.compositor_effects)
    assert pathed.attached.struct(scene).version == version + 1
    assert record.written("First").map_error(lambda faults: tuple(fault.reason for fault in faults)) == Error((Reason.ABSENT,))
    assert record.written("Unrequested Pass").map_error(lambda faults: tuple((fault.reason, type(fault.cause)) for fault in faults)) == Error(((Reason.ABSENT, ValueError),))


# --- [COMPOSITION] ----------------------------------------------------------------------

FOLLOWUP: Final = FollowUp(key="queue_followup_case", form=FollowupSettings, built=followed, passes=(_VECTOR,), held=Some(Snapshot.of(bpy.types.Scene, {("render", "use_motion_blur"): False})))
QUEUE: Final = QueueRun(sets=RenderSet(capture=CATEGORIES))
ROWS: Final = (FOLLOWUP, RoutingRow.ownership, QUEUE)
