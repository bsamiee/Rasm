"""Native transformed-prefix preparation, pupil gates, shared instances, and view domains."""

from collections.abc import Callable, Iterable
from contextlib import ExitStack
from pathlib import Path
from typing import override

import bpy
from expression import Option, Some
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO
import pytest

from apps.blender.darkroom.effects.optics.glare import Glare, glare
from blender.bpy.data.ids.images import filled
from blender.bpy.data.ids.ownership import Mark
from blender.bpy.data.ids.store import IdName
from blender.bpy.data.rna.paths import definition
from blender.bpy.data.rna.snapshots import assigned
from blender.bpy.extension.identity import Identity
from blender.bpy.nodes import functions
from blender.bpy.nodes.addressing import owned
from blender.bpy.nodes.changes import made
from blender.bpy.nodes.desired import item_key
from blender.bpy.nodes.graphs import Expr, group_input
from blender.bpy.nodes.interfaces import Interface
from blender.bpy.render.compositor.colors import COLOR_INPUT, COLOR_RESULT
from blender.bpy.render.compositor.effects import CompositorEffect, CompositorRecord, Entry, Mix, Prepared, Stage
from blender.bpy.render.compositor.inputs import entered, GraphInputs
from blender.bpy.render.compositor.layers import EFFECTS, IMAGE, LayerStack, NodeSource, RenderPass, StackLayer
from blender.bpy.render.compositor.offered import OfferedEffects, Phase
from blender.bpy.render.compositor.stack_extension import LayerStackExtension
from blender.bpy.render.outputs.formats import Colorspace, FileItem, FileItems, OutputFormat
from blender.bpy.render.sessions.engines import Engine, Tile
from blender.bpy.render.sessions.renders import native, RenderCall
from blender.bpy.render.sessions.staging import StagedRender
from blender.imaging.files.buffers import checked
from blender.imaging.files.multilayer import Multilayer
from blender.imaging.sampling.tiles import Pupil
from blender.records.faults import Ok, Outcome
from tests.python.libs.blender.bpy.background.session import Acquired
from tests.python.libs.blender.bpy.render.sessions.test_renders import render_scene

# --- [MODELS] ---------------------------------------------------------------------------


class Doubled(CompositorRecord, frozen=True, kw_only=True):
    """CPU exposure with a native exposure control."""

    @property
    @override
    def prepared(self) -> Callable[[Stage], Outcome[Option[Prepared]]]:
        """Doubles captured RGB and retains alpha and native windows."""
        return lambda stage: checked(OpenImageIO.ImageBufAlgo.mul(stage.image, (2.0, 2.0, 2.0, 1.0))).map(Some)


class Exposure(CompositorRecord, frozen=True, kw_only=True):
    """Native one-stop reference."""

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Doubles native RGB while retaining its domain."""
        return Ok(Expr(kind=bpy.types.CompositorNodeExposure, inputs=(("Image", shell.color), ("Exposure", 1.0)), socket="Image", socket_type=bpy.types.NodeSocketColor))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _patterned(engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph) -> Iterable[Outcome[Tile]]:
    scene: bpy.types.Scene = depsgraph.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    names = tuple(view.name for view in scene.render.views if view.use) if scene.render.use_multiview else ("",)
    plane = np.zeros((engine.resolution_y, engine.resolution_x, 4), dtype=np.float32)
    plane[..., 3] = 1.0
    plane[1:3, 3:5, :3] = (1.0, 0.5, 0.2)
    return tuple(Ok(Tile(x=0, y=0, view=name, planes=((Engine.combined, plane),))) for name in names)


def _source(identity: Identity, scene: bpy.types.Scene, names: tuple[str, ...]) -> None:
    source = Expr(kind=bpy.types.CompositorNodeRLayers, members={"scene": IdName.of(scene), "layer": scene.view_layers[0].name}, socket="Image", socket_type=bpy.types.NodeSocketColor)
    outputs = []
    for index, name in enumerate(names):
        scaled = Expr(kind=bpy.types.CompositorNodeScale, inputs=(("Image", source), ("X", 1.5 / (index + 1)), ("Y", 0.75 * (index + 1))), socket="Image", socket_type=bpy.types.NodeSocketColor)
        outputs.append((name, Expr(kind=bpy.types.CompositorNodeTranslate, inputs=(("Image", scaled), ("X", 1.25), ("Y", -0.75)), socket="Image", socket_type=bpy.types.NodeSocketColor)))
    image = outputs[0][1] if len(outputs) == 1 else Expr(kind=bpy.types.CompositorNodeSwitchView, inputs=tuple(outputs), socket="Image", socket_type=bpy.types.NodeSocketColor)
    group = functions.built("staging_source", Interface(outputs=(COLOR_RESULT,)), {COLOR_RESULT.key: image}, bpy.types.CompositorNodeTree).default_with(lambda faults: pytest.fail(str(faults)))
    entry = scene.compositor_effects.new("Transformed Source")
    entry.node_group = made(group, identity).default_with(lambda faults: pytest.fail(str(faults)))


def _capture(identity: Identity, scene: bpy.types.Scene, directory: Path) -> bpy.types.CompositorNodeOutputFile:
    image = group_input(COLOR_INPUT.key, bpy.types.NodeSocketColor)
    capture_format = OutputFormat(
        media_type="IMAGE",
        file_format="OPEN_EXR",
        color_mode="RGBA",
        color_depth="32",
        exr_codec="ZIP",
        use_exr_interleave=False,
        views_format="MULTIVIEW",
        color_management="OVERRIDE",
        linear_colorspace_settings=Colorspace(name=PyOpenColorIO.GetCurrentConfig().getColorSpace(PyOpenColorIO.ROLE_DATA).getName()),
    )
    files = Expr(
        kind=bpy.types.CompositorNodeOutputFile,
        inputs=(("image", image),),
        members={"directory": str(directory), "file_name": "", **capture_format.members(("format",))},
        content=Some(FileItems(items={"image": FileItem(name="observed", socket_type="RGBA")})),
    )
    group = functions.built("staging_capture", Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,)), {COLOR_RESULT.key: image.keeping(files)}, bpy.types.CompositorNodeTree).default_with(
        lambda faults: pytest.fail(str(faults))
    )
    entry = scene.compositor_effects.new("Capture")
    entry.node_group = made(group, identity).default_with(lambda faults: pytest.fail(str(faults)))
    return next(node for node in entry.node_group.nodes if isinstance(node, bpy.types.CompositorNodeOutputFile))


def _read(path: Path) -> dict[str, tuple[tuple[int, ...], npt.NDArray[np.float32]]]:
    file = Multilayer.read(path).default_with(lambda faults: pytest.fail(str(faults)))
    return {
        spec.get_string_attribute("view"): (
            (spec.x, spec.y, spec.width, spec.height, spec.full_x, spec.full_y, spec.full_width, spec.full_height),
            file.plane(layer, ("R", "G", "B", "A")).default_with(lambda faults: pytest.fail(str(faults))),
        )
        for layer in file.layers
        for spec in (file.parts[layer.part],)
    }


# --- [TESTS] ----------------------------------------------------------------------------


@pytest.mark.parametrize("multiview", [False, True])
def test_pupil_gate_and_transformed_view_domains(acquired: Acquired, tmp_path: Path, *, multiview: bool) -> None:
    """Off uses one native render; on preserves translated view domains and changes real pixels."""
    rows = OfferedEffects(phases=(Phase(key="optics", members=(glare,)),))
    renderer = StagedRender(rows=rows)
    engine = Engine(key="staging_pattern", render=_patterned)
    with acquired((renderer, engine)) as identity, render_scene() as (scene, _camera), ExitStack() as release:
        before = frozenset(bpy.data.all_ids)
        release.callback(lambda: bpy.data.batch_remove(ids=tuple(frozenset(bpy.data.all_ids) - before)))
        scene.render.engine = identity.name(bpy.types.RenderEngine, engine.key)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        scene.render.use_compositing, scene.render.use_multiview = True, multiview
        names = tuple(view.name for view in scene.render.views if view.use) if multiview else ("",)
        with bpy.context.temp_override(scene=scene, view_layer=scene.view_layers[0]):
            _source(identity, scene, names)
            entry = glare.appended(identity, scene, Entry(record=Glare(pupil=Pupil(core=0.17)))).default_with(lambda faults: pytest.fail(str(faults)))
            target = _capture(identity, scene, tmp_path)
            renders: list[str] = []

            def started(rendered: bpy.types.Scene) -> None:
                renders.append(rendered.name)

            bpy.app.handlers.render_pre.append(started)
            release.callback(bpy.app.handlers.render_pre.remove, started)
            outputs = []
            for enabled in (False, True):
                folder = tmp_path / str(enabled)
                folder.mkdir()
                target.directory = str(folder)
                values = {socket.key: enabled for socket in Option.of_optional(Glare.prepared_when).to_list()}
                values.update({panel.key: False for panel in glare.interface.panels if panel.toggle.is_some()})
                assert entered(identity, entry, values).is_ok()
                renders.clear()
                assert renderer.rendered(identity, scene, RenderCall()) == Ok(frozenset({"FINISHED"})), identity.reports.entries
                assert len(renders) == (2 if enabled else 1)
                outputs.append(_read(folder / "observed.exr"))
            off, on = outputs
            assert set(off) == set(on) == set(names)
            for name in names:
                assert off[name][0] == on[name][0]
                assert np.isfinite(on[name][1]).all()
                support = np.any(off[name][1][..., :3] != 0.0, axis=-1)
                assert np.any(on[name][1][..., :3][~support] != 0.0)
            assert {
                tuple(image.size)  # type: ignore[arg-type]
                for image in bpy.data.images
                if Mark.read(identity, image).bind(lambda mark: mark.content).is_some()
            } >= {(2 * item[1].shape[1] - 1, 2 * item[1].shape[0] - 1) for item in off.values()}


def test_processed_pixels_replay_on_translated_native_domain(acquired: Acquired, tmp_path: Path) -> None:
    """A nonidentity CPU exposure matches native exposure through original translated UV coordinates."""
    doubled, exposure = (CompositorEffect(record=record, mix=Mix.REPLACE) for record in (Doubled, Exposure))
    rows = OfferedEffects(phases=(Phase(key="exposure", members=(doubled, exposure)),))
    renderer = StagedRender(rows=rows)
    engine = Engine(key="staging_pattern", render=_patterned)
    with acquired((renderer, engine)) as identity, render_scene() as (scene, _camera), ExitStack() as release:
        before = frozenset(bpy.data.all_ids)
        release.callback(lambda: bpy.data.batch_remove(ids=tuple(frozenset(bpy.data.all_ids) - before)))
        scene.render.engine = identity.name(bpy.types.RenderEngine, engine.key)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        scene.render.use_compositing = True
        with bpy.context.temp_override(scene=scene, view_layer=scene.view_layers[0]):
            _source(identity, scene, ("",))
            control = exposure.appended(identity, scene, Entry(record=Exposure())).default_with(lambda faults: pytest.fail(str(faults)))
            candidate = doubled.appended(identity, scene, Entry(record=Doubled())).default_with(lambda faults: pytest.fail(str(faults)))
            candidate.enable_for_render = False
            target = _capture(identity, scene, tmp_path)
            assert native(identity, scene, RenderCall()) == Ok(frozenset({"FINISHED"}))
            expected = _read(tmp_path / "observed.exr")
            control.enable_for_render, candidate.enable_for_render = False, True
            assert renderer.rendered(identity, scene, RenderCall()) == Ok(frozenset({"FINISHED"})), identity.reports.entries
            actual = _read(Path(target.directory) / "observed.exr")
            assert actual[""][0] == expected[""][0]
            np.testing.assert_array_equal(actual[""][1], expected[""][1])


@pytest.mark.parametrize("container,effect", [(False, True), (True, False), (True, True)])
def test_shared_nested_pupils_obey_parent_gates(acquired: Acquired, tmp_path: Path, *, container: bool, effect: bool) -> None:
    """Shared group instances retain independent preparation and skip disabled effect parents."""
    rows = OfferedEffects(phases=(Phase(key="optics", members=(glare,)),))
    stacks = LayerStackExtension(rows=rows)
    renderer = StagedRender(rows=rows, stacks=Some(stacks.bound))
    engine = Engine(key="staging_pattern", render=_patterned)
    with acquired((renderer, stacks, engine)) as identity, render_scene() as (scene, _camera), ExitStack() as release:
        before = frozenset(bpy.data.all_ids)
        release.callback(lambda: bpy.data.batch_remove(ids=tuple(frozenset(bpy.data.all_ids) - before)))
        scene.render.engine = identity.name(bpy.types.RenderEngine, engine.key)  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        scene.render.use_compositing = True
        record = Entry(record=Glare(pupil=Pupil(core=0.17)))
        layers = (
            StackLayer(key="first", name="First", source=RenderPass(layer="A"), effects=(record,)),
            StackLayer(key="second", name="Second", source=NodeSource(node=definition(bpy.types.CompositorNodeImage).identifier, output=IMAGE), effects=(record,)),
        )
        with bpy.context.temp_override(scene=scene, view_layer=scene.view_layers[0]):
            service = stacks.bound(identity).default_with(lambda faults: pytest.fail(str(faults)))
            tree = service.made(scene, bpy.context).default_with(lambda faults: pytest.fail(str(faults)))
            assert service.rebuilt(scene, tree, LayerStack(layers=layers), bpy.context).is_ok()
            image = bpy.data.images.new("Distinct Source", width=12, height=6, alpha=True, float_buffer=True, is_data=True)
            plane = np.zeros((6, 12, 4), dtype=np.float32)
            plane[..., 3] = 1.0
            plane[2:4, 5:7, :3] = (0.2, 0.5, 1.0)
            assert filled(image, plane) == Ok(None)
            assert assigned(owned(tree, identity)[layers[1].node], {("image",): IdName.of(image)}) == Ok(None)
            entry = service.head(scene).default_with(lambda faults: pytest.fail(str(faults)))[0]
            groups = {key: node for key, node in owned(tree, identity).items() if isinstance(node, bpy.types.CompositorNodeGroup)}
            first, second = (groups[layer.places[0]] for layer in layers)
            second.node_tree = first.node_tree
            shared = first.node_tree
            values = {item_key(layer.places[0], socket.key): True for layer in layers for socket in Option.of_optional(Glare.prepared_when).to_list()}
            values.update({item_key(layer.places[0], panel.key): False for layer in layers for panel in glare.interface.panels if panel.toggle.is_some()})
            values.update({item_key(layer.key, EFFECTS): container for layer in layers})
            values.update(dict.fromkeys((layer.places[0] for layer in layers), effect))
            assert entered(identity, entry, values).is_ok()
            for node in tree.nodes:
                if isinstance(node, bpy.types.CompositorNodeOutputFile):
                    node.directory = str(tmp_path)
            _capture(identity, scene, tmp_path)
            renders: list[str] = []

            def started(rendered: bpy.types.Scene) -> None:
                renders.append(rendered.name)

            bpy.app.handlers.render_pre.append(started)
            release.callback(bpy.app.handlers.render_pre.remove, started)
            assert renderer.rendered(identity, scene, RenderCall()) == Ok(frozenset({"FINISHED"})), identity.reports.entries
            assert len(renders) == (3 if container and effect else 1)
            assert first.node_tree == second.node_tree == shared
            assert np.isfinite(_read(tmp_path / "observed.exr")[""][1]).all()
            if container and effect:
                assert {
                    tuple(image.size)  # type: ignore[arg-type]
                    for image in bpy.data.images
                    if Mark.read(identity, image).bind(lambda mark: mark.content).is_some()
                } >= {(2 * scene.render.resolution_x - 1, 2 * scene.render.resolution_y - 1), (2 * plane.shape[1] - 1, 2 * plane.shape[0] - 1)}
