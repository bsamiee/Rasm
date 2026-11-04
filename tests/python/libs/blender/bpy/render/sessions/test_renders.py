"""Native render codes, raster geometry, render policies, and camera bindings."""

from collections.abc import Generator, Iterable
from contextlib import contextmanager, ExitStack
from functools import partial
from itertools import chain, product
from pathlib import Path

import bpy
from expression import Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO
import pytest

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Handler
from blender.bpy.render.color.spaces import NO_LOOK
from blender.bpy.render.outputs.formats import OutputFormat
from blender.bpy.render.outputs.framing import Border, Extents, FrameSize, Whole
from blender.bpy.render.outputs.results import combined, CompletedRender, Finish, Finishing, intermediate, RENDER_ORIGINS, ResultRead, Written
from blender.bpy.render.sessions.engines import Engine, Tile
from blender.bpy.render.sessions.plans import cameras, JobScope
from blender.bpy.render.sessions.renders import native, RenderCall
from blender.imaging.files.buffers import Depth
from blender.imaging.files.images import emitted, read_codes, Subimage
from blender.imaging.geometry.frames import Raster, Region
from blender.records.faults import Error, Nothing, Ok, Outcome, paired, Reason
from blender.rendering.batch.queue import FixedCamera, Frames, MarkedCamera, Marker
from blender.rendering.outputs.naming import Output
from tests.python.libs.blender.bpy.background.session import Acquired

# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def render_scene() -> Generator[tuple[bpy.types.Scene, bpy.types.Camera]]:
    """Owns a tiny native render scene, camera, and two enabled layers until scope exit."""
    with ExitStack() as release:
        scene = bpy.data.scenes.new("Native Render Policy")
        camera_data = bpy.data.cameras.new("Native Render Camera")
        camera = bpy.data.objects.new(camera_data.name, camera_data)
        release.callback(bpy.data.batch_remove, ids=(scene, camera, camera_data))
        scene.collection.objects.link(camera)
        scene.camera = camera
        scene.view_layers[0].name = "A"
        scene.view_layers.new("B")
        scene.render.resolution_x, scene.render.resolution_y, scene.render.resolution_percentage = 8, 4, 100
        scene.render.use_compositing = False
        yield scene, camera_data


def _gray_render(engine: bpy.types.RenderEngine, _depsgraph: bpy.types.Depsgraph) -> Iterable[Outcome[Tile]]:
    pixels = np.asarray((0.18, 0.18, 0.18, 1.0), dtype=np.float32)
    return (Ok(Tile(x=0, y=0, planes=((Engine.combined, np.broadcast_to(pixels, (engine.resolution_y, engine.resolution_x, len(pixels)))),))),)


def _standard_png(scene: bpy.types.Scene, engine: str, target: Path) -> Outcome[None]:
    return assigned(
        scene,
        {
            ("render", "engine"): engine,
            ("render", "filepath"): str(target),
            ("render", "use_multiview"): False,
            ("render", "dither_intensity"): 0.0,
            ("render", "image_settings", "media_type"): "IMAGE",
            ("render", "image_settings", "file_format"): "PNG",
            ("render", "image_settings", "color_mode"): "RGBA",
            ("render", "image_settings", "color_depth"): "8",
            ("render", "image_settings", "color_management"): "FOLLOW_SCENE",
            ("display_settings", "display_device"): "sRGB",
            ("view_settings", "view_transform"): "Standard",
            ("view_settings", "look"): NO_LOOK,
            ("view_settings", "exposure"): 0.0,
            ("view_settings", "gamma"): 1.0,
            ("view_settings", "use_curve_mapping"): False,
            ("view_settings", "use_white_balance"): False,
        },
    )


def _render_images() -> tuple[dict[str, object], ...]:
    return tuple(
        {
            "image": (image.name, image.as_pointer(), image.type, image.source),
            "slots": tuple((slot.as_pointer(), slot.name) for slot in image.render_slots),
            "active": Option.of_optional(image.render_slots.active).map(lambda slot: (slot.as_pointer(), slot.name)).to_optional(),
        }
        for image in bpy.data.images
        if image.type == "RENDER_RESULT"
    )


@pytest.mark.parametrize("human_bytes,suffix", [pytest.param(31, "r", id="ascii-31"), pytest.param(32, "r", id="ascii-32"), pytest.param(31, "é", id="utf8-31"), pytest.param(32, "é", id="utf8-32")])
def test_replayed_engine_executes_at_native_identifier_byte_boundary(acquired: Acquired, tmp_path: Path, human_bytes: int, suffix: str) -> None:
    """Human and opaque engine identifiers select replayed native pixels across the UTF-8 storage boundary."""
    observed: list[tuple[bpy.types.Scene, str, str]] = []
    with acquired((RENDER_ORIGINS,)) as identity, render_scene() as (scene, _camera):
        target = tmp_path / "replayed.exr"
        prefix = f"{identity.manifest.id.upper()}_"
        key = f"{'r' * (human_bytes - len(prefix.encode()) - len(suffix.upper().encode()))}{suffix}"
        human = f"{prefix}{key.upper()}"
        pixels = np.broadcast_to(np.asarray((0.125, 0.25, 0.5, 1.0), dtype=np.float32), (scene.render.resolution_y, scene.render.resolution_x, 4))
        tile = Tile(x=0, y=0, planes=((Engine.combined, pixels),))

        def replayed(source: bpy.types.Scene, layer: bpy.types.ViewLayer) -> tuple[Tile, ...]:
            observed.append((source, layer.name, source.render.engine))
            return (tile,)

        with Engine.replayed(identity, key, replayed) as registered:
            name = registered.default_with(lambda faults: pytest.fail(str(faults)))
            assert len(human.encode()) == human_bytes
            assert name == identity.name(bpy.types.RenderEngine, key)
            if human_bytes == 31:
                assert name == human
            else:
                assert name.startswith("_")
                assert name != human
            assert len(name.encode()) <= 31
            assert assigned(scene, {("render", "engine"): name, ("render", "filepath"): str(target)}) == Ok(None)
            assert OutputFormat(media_type="IMAGE", file_format="OPEN_EXR", color_mode="RGBA", color_depth="32").applied(scene.render.image_settings) == Ok(None)
            assert scene.render.engine == name
            before = _render_images()
            answer = native(identity, scene, RenderCall(write_still=True, layer=Some("A")), files=Some(lambda produced: Ok(Written.scene(produced, ((target, Nothing),)))))
            assert answer == Ok(frozenset({"FINISHED"})), {
                "scene": (scene.name, scene.session_uid, scene.as_pointer()),
                "layers": tuple((layer.name, layer.use) for layer in scene.view_layers),
                "before": before,
                "after": _render_images(),
                "callbacks": tuple((source.name, source.session_uid, source.as_pointer(), layer, selected) for source, layer, selected in observed),
            }
            assert scene.render.engine == name
            assert (scene, "A", name) in observed
            image = combined(str(target), PyOpenColorIO.GetCurrentConfig()).default_with(lambda faults: pytest.fail(str(faults)))
            assert image.header.format == OpenImageIO.FLOAT
            assert image.pixels.dtype == np.float32
            np.testing.assert_array_equal(image.pixels, pixels)


@pytest.mark.parametrize("requested", ["A", "B"])
def test_single_layer_multilayer_render_retains_prior_layers_and_saved_capture(acquired: Acquired, tmp_path: Path, requested: str) -> None:
    """Explicit layer rendering retains other native parts and selects fresh pixels through automatic saved capture."""
    observed: list[tuple[npt.NDArray[np.float32], npt.NDArray[np.float32]]] = []
    callbacks: list[str] = []

    def consumed(frame: CompletedRender) -> Outcome[Option[str]]:
        return paired(frame.source(), frame.read()).map(lambda files: (files[0].pixels, files[1].pixels)).map(observed.append).map(lambda _: Nothing)

    with acquired((ResultRead(capture=lambda _scene, _config: Ok(consumed)),)) as identity, render_scene() as (scene, _camera):
        shape = (scene.render.resolution_y, scene.render.resolution_x, 4)
        initial = {name: np.broadcast_to(np.asarray(color, dtype=np.float32), shape) for name, color in (("A", (0.125, 0.25, 0.5, 1.0)), ("B", (0.5, 0.25, 0.125, 1.0)))}
        changed = {name: np.broadcast_to(np.asarray(color, dtype=np.float32), shape) for name, color in (("A", (0.75, 0.5, 0.25, 1.0)), ("B", (0.25, 0.5, 0.75, 1.0)))}
        current = initial.copy()

        def replayed(_scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> tuple[Tile, ...]:
            callbacks.append(layer.name)
            return (Tile(x=0, y=0, planes=((Engine.combined, current[layer.name]),)),)

        with Engine.replayed(identity, "multilayer_merge", replayed) as registered:
            name = registered.default_with(lambda faults: pytest.fail(str(faults)))
            target = tmp_path / "retained.exr"
            assert assigned(scene, {("render", "engine"): name, ("render", "filepath"): str(target)}) == Ok(None)
            assert OutputFormat(media_type="MULTI_LAYER_IMAGE", file_format="OPEN_EXR_MULTILAYER", color_mode="RGBA", color_depth="32", use_exr_interleave=False).applied(
                scene.render.image_settings
            ) == Ok(None)
            with intermediate(identity, scene):
                assert native(identity, scene, RenderCall()) == Ok(frozenset({"FINISHED"}))
            assert set(callbacks) == initial.keys()
            current.update(changed)
            callbacks.clear()
            before = _render_images()
            answer = native(identity, scene, RenderCall(write_still=True, layer=Some(requested)), files=Some(lambda produced: Ok(Written.scene(produced, ((target, Nothing),)))))
            assert answer == Ok(frozenset({"FINISHED"})), {"requested": requested, "callbacks": tuple(callbacks), "before": before, "after": _render_images(), "reports": identity.reports.entries}
            assert set(callbacks) == {requested}
            assert {layer.name for layer in scene.view_layers if layer.use} == initial.keys()
            assert identity.reports.ticked() is None
            assert len(observed) == 1, identity.reports.entries
            for pixels in observed[0]:
                assert pixels.dtype == np.float32
                np.testing.assert_array_equal(pixels, changed[requested])
            for layer, previous in initial.items():
                image = combined(str(target), PyOpenColorIO.GetCurrentConfig(), Some(layer)).default_with(lambda faults: pytest.fail(str(faults)))
                assert image.header.format == OpenImageIO.FLOAT
                np.testing.assert_array_equal(image.pixels, changed[layer] if layer == requested else previous)


def test_completed_codes_read_native_png_after_finishing(acquired: Acquired, tmp_path: Path) -> None:
    """Completed codes retain exact unsigned endpoint patterns written by a native file finisher."""
    before: list[npt.NDArray[np.uint8 | np.uint16]] = []
    observed: list[npt.NDArray[np.uint8 | np.uint16]] = []

    def consumed(frame: CompletedRender) -> Outcome[Option[str]]:
        return frame.codes.map(lambda held: observed.extend(held.to_list())).map(lambda _: Nothing)

    with render_scene() as (scene, _camera):
        pattern = np.asarray(((0, 64, 255, 255), (255, 128, 0, 255)), dtype=np.uint8)
        expected = pattern[np.indices((scene.render.resolution_y, scene.render.resolution_x))[1] % len(pattern)]

        def rewritten(_scene: bpy.types.Scene, file: Written, _identity: Identity) -> Outcome[None]:
            original = read_codes(Path(file.path)).default_with(lambda faults: pytest.fail(str(faults))).default_with(lambda: pytest.fail("Native PNG holds no integer codes"))
            before.append(original)
            height, width, channels = expected.shape
            return emitted(Path(file.path), lambda _writer: (Subimage(spec=OpenImageIO.ImageSpec(width, height, channels, OpenImageIO.UINT8), attributes=(), pixels=partial(Ok, expected)),)).map(
                lambda _: None
            )

        engine = Engine(key="finished_native_codes", render=_gray_render)
        finishing = Finishing(
            rows=(Finish(run=rewritten, outputs=frozenset({Output.PREVIEW}), formats=frozenset({"PNG"}), depths=frozenset({Depth.UINT8})),), files=lambda _scene, _frame, _identity, _animation: Ok(())
        )
        with acquired((engine, finishing, ResultRead(capture=lambda _scene, _config: Ok(consumed), codes=True))) as identity:
            target = tmp_path / "finished.png"
            assert _standard_png(scene, identity.name(bpy.types.RenderEngine, engine.key), target) == Ok(None)
            with intermediate(identity, scene):
                assert native(identity, scene, RenderCall(layer=Some("A"))) == Ok(frozenset({"FINISHED"}))
            assert native(identity, scene, RenderCall(write_still=True, layer=Some("A")), files=Some(lambda produced: Ok(Written.scene(produced, ((target, Nothing),))))) == Ok(frozenset({"FINISHED"}))
            assert identity.reports.ticked() is None
            assert len(before) == len(observed) == 1, identity.reports.entries
            assert before[0].dtype == observed[0].dtype == np.uint8
            np.testing.assert_array_equal(before[0][..., :3], np.full((*expected.shape[:2], 3), 118, dtype=np.uint8))
            np.testing.assert_array_equal(observed[0], expected)


@pytest.mark.parametrize("requested", [False, True])
def test_no_write_codes_use_captured_native_settings_and_restore_later_edits(acquired: Acquired, tmp_path: Path, *, requested: bool) -> None:
    """Requested previews retain captured native settings; no-code reads skip preview finishing and preserve later edits."""
    observed: list[Option[npt.NDArray[np.uint8 | np.uint16]]] = []
    previews: list[Written] = []
    later: list[Snapshot] = []
    selection = (
        Descriptor(root=bpy.types.Scene, prop=bpy.types.Scene.bl_rna.properties["view_settings"]),
        Descriptor(root=bpy.types.Scene, owner=("render",), prop=bpy.types.RenderSettings.bl_rna.properties["dither_intensity"]),
    )

    def consumed(frame: CompletedRender) -> Outcome[Option[str]]:
        return frame.codes.map(observed.append).map(lambda _: Nothing)

    def edited(scene: bpy.types.Scene) -> Outcome[None]:
        view: bpy.types.ColorManagedViewSettings = scene.view_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        view.exposure, view.use_curve_mapping = 4.0, True
        mapping: bpy.types.CurveMapping = view.curve_mapping  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        for point in chain.from_iterable(curve.points for curve in mapping.curves):
            point.location.y *= 0.5
        mapping.update()
        scene.render.dither_intensity = 2.0
        return Snapshot.capture(scene, Some(selection), inactive=True).map(later.append)

    def previewed(_scene: bpy.types.Scene, file: Written, _identity: Identity) -> Outcome[None]:
        previews.append(file)
        return Ok(None)

    engine = Engine(key="captured_native_codes", render=_gray_render)
    finishing = Finishing(
        rows=(Finish(run=previewed, outputs=frozenset({Output.PREVIEW}), formats=frozenset({"PNG"}), depths=frozenset({Depth.UINT8})),), files=lambda _scene, _frame, _identity, _animation: Ok(())
    )
    with acquired((engine, finishing, ResultRead(capture=lambda _scene, _config: Ok(consumed), codes=requested))) as identity, render_scene() as (scene, _camera):
        target = tmp_path / "unwritten.png"
        assert _standard_png(scene, identity.name(bpy.types.RenderEngine, engine.key), target) == Ok(None)
        view: bpy.types.ColorManagedViewSettings = scene.view_settings  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        view.use_curve_mapping = True
        mapping: bpy.types.CurveMapping = view.curve_mapping  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        mapping.initialize()
        view.use_curve_mapping = False
        with intermediate(identity, scene):
            assert native(identity, scene, RenderCall(layer=Some("A"))) == Ok(frozenset({"FINISHED"}))
        with Handler(handlers=bpy.app.handlers.render_post, body=edited).held(identity) as held:
            assert held == Ok(None)
            assert native(identity, scene, RenderCall(layer=Some("A"))) == Ok(frozenset({"FINISHED"}))
        assert identity.reports.ticked() is None
        assert len(observed) == len(later) == 1, identity.reports.entries
        assert len(previews) == int(requested)
        if requested:
            pixels = observed[0].default_with(lambda: pytest.fail("Requested native preview holds no integer codes"))
            assert pixels.dtype == np.uint8
            np.testing.assert_array_equal(pixels[..., :3], np.full((scene.render.resolution_y, scene.render.resolution_x, 3), 118, dtype=np.uint8))
            np.testing.assert_array_equal(pixels[..., 3], np.full((scene.render.resolution_y, scene.render.resolution_x), 255, dtype=np.uint8))
        else:
            assert observed == [Nothing]
        assert Snapshot.capture(scene, Some(selection), inactive=True) == Ok(later[0])
        assert (view.exposure, view.use_curve_mapping, scene.render.dither_intensity) == (4.0, True, 2.0)
        assert not target.exists()


@pytest.mark.parametrize("locked,single", list(product((False, True), repeat=2)))
def test_native_render_scopes_layer_and_restores_policy(acquired: Acquired, *, locked: bool, single: bool) -> None:
    """Explicit B renders under the native lock and restores both policies after success and refusal."""
    events: list[tuple[str, str, bool, bool, int, float]] = []

    def rendered(engine: bpy.types.RenderEngine, depsgraph: bpy.types.Depsgraph) -> Iterable[Outcome[Tile]]:
        """Records native rendering state and supplies one Combined tile."""
        target: bpy.types.Scene = depsgraph.scene  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        layer: bpy.types.ViewLayer = depsgraph.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        events.append((target.name, layer.name, target.render.use_lock_interface, target.render.use_single_layer, target.frame_current, target.frame_subframe))
        return (Ok(Tile(x=0, y=0, planes=((Engine.combined, np.ones((engine.resolution_y, engine.resolution_x, 4), dtype=np.float32)),))),)

    engine = Engine(key="native_render_policy", render=rendered)
    with acquired((engine, RENDER_ORIGINS)) as identity, render_scene() as (scene, _camera):
        assert assigned(scene, {("render", "engine"): identity.name(bpy.types.RenderEngine, engine.key)}) == Ok(None)
        scene.render.use_lock_interface, scene.render.use_single_layer = locked, single
        scene.frame_set(7, subframe=0.25)
        context_scene, context_layer = bpy.context.scene, bpy.context.view_layer
        call = RenderCall(layer=Some("B"))
        assert native(identity, scene, call) == Ok(frozenset({"FINISHED"}))
        assert events == [(scene.name, "B", True, True, 7, 0.25)]
        assert (scene.render.use_lock_interface, scene.render.use_single_layer) == (locked, single)
        assert (bpy.context.scene, bpy.context.view_layer) == (context_scene, context_layer)
        assert (scene.frame_current, scene.frame_subframe) == (7, 0.25)
        scene.camera = None
        refused = native(identity, scene, call)
        assert refused.map_error(lambda faults: tuple((fault.reason, type(fault.cause)) for fault in faults)) == Error(((Reason.FAILED, RuntimeError),))
        assert events == [(scene.name, "B", True, True, 7, 0.25)]
        assert (scene.render.use_lock_interface, scene.render.use_single_layer) == (locked, single)
        assert (bpy.context.scene, bpy.context.view_layer) == (context_scene, context_layer)


@pytest.mark.parametrize(
    "extent,pixel_aspect,expected,fit_shape",
    [
        pytest.param(Whole(), 1.0, Raster(bounds=Region(top=0.6), shape=(6, 10)), (2, 4), id="whole"),
        pytest.param(
            Border(region=Region(left=0.2, bottom=1.0 / 6.0, right=0.8, top=5.0 / 6.0), crop=True),
            1.0,
            Raster(bounds=Region(left=0.2, bottom=0.1, right=0.8, top=0.5), shape=(4, 6)),
            (3, 4),
            id="cropped-border",
        ),
        pytest.param(Border(region=Region(left=0.2, bottom=1.0 / 6.0, right=0.8, top=5.0 / 6.0)), 1.0, Raster(bounds=Region(top=0.6), shape=(6, 10)), (2, 4), id="uncropped-border"),
        pytest.param(Whole(overscan=0.2), 1.0, Raster(bounds=Region(left=-0.2, bottom=-0.1, right=1.2, top=0.7), shape=(8, 14)), (2, 4), id="overscan"),
        pytest.param(Whole(), 2.0, Raster(bounds=Region(top=0.3), shape=(6, 10)), (2, 4), id="wide-pixels"),
    ],
)
def test_native_rasters_match_pixels_and_camera_geometry(acquired: Acquired, tmp_path: Path, extent: Extents, pixel_aspect: float, expected: Raster, fit_shape: tuple[int, int]) -> None:
    """Raw native EXR pixels, border extents, overscan cameras, and fitted shapes share the recorded camera domain."""
    with acquired((FrameSize(), RENDER_ORIGINS)) as identity, render_scene() as (scene, camera):
        target = tmp_path / "native.exr"
        assert assigned(
            scene,
            {
                ("render", "engine"): "CYCLES",
                ("render", "filepath"): str(target),
                ("render", "image_settings", "media_type"): "MULTI_LAYER_IMAGE",
                ("render", "image_settings", "file_format"): "OPEN_EXR_MULTILAYER",
                ("render", "image_settings", "color_mode"): "RGBA",
                ("render", "image_settings", "color_depth"): "32",
                ("cycles", "device"): "CPU",
                ("cycles", "samples"): 1,
                ("cycles", "use_denoising"): False,
            },
        ) == Ok(None)
        scene.render.resolution_x, scene.render.resolution_y = 10, 6
        camera.type, camera.ortho_scale = "ORTHO", 2.0
        framed = FrameSize().bound(identity).default_with(lambda faults: pytest.fail(str(faults)))
        assert framed.written(scene, msgspec.structs.replace(framed.read(scene), extent=extent, pixel_aspect_x=pixel_aspect, pixel_aspect_y=1.0)) == Ok(None)

        def corners() -> npt.NDArray[np.float64]:
            return np.asarray(tuple(point.to_tuple()[:2] for point in camera.view_frame(scene=scene)), dtype=np.float64)

        original = corners()
        span = np.ptp(original, axis=0)
        tolerance = float(np.finfo(np.float32).eps)
        assert framed.grown(scene).is_ok()
        data, camera_frame = framed.rasters(scene).default_with(lambda faults: pytest.fail(str(faults)))
        assert data == expected
        assert camera_frame.shape == (6, 10)
        np.testing.assert_allclose(
            (camera_frame.bounds.left, camera_frame.bounds.bottom, camera_frame.bounds.right, camera_frame.bounds.top), np.concatenate((np.zeros(2), span / span.max())), rtol=0.0, atol=tolerance
        )
        current = corners()
        bounds = data.bounds if isinstance(extent, Whole) else camera_frame.bounds
        np.testing.assert_allclose(
            (bounds.left, bounds.bottom, bounds.right, bounds.top),
            np.concatenate((current.min(axis=0) - original.min(axis=0), current.max(axis=0) - original.min(axis=0))) / span.max(),
            rtol=0.0,
            atol=tolerance,
        )
        assert native(identity, scene, RenderCall(write_still=True, layer=Some("A")), files=Some(lambda produced: Ok(Written.scene(produced, ((target, Nothing),))))) == Ok(frozenset({"FINISHED"}))
        file = combined(str(target), PyOpenColorIO.GetCurrentConfig(), Some("A")).default_with(lambda faults: pytest.fail(str(faults)))
        assert file.pixels.shape == (*data.shape, 4)
        assert (file.header.height, file.header.width) == data.shape
        assert (file.header.x, file.header.y, file.header.full_x, file.header.full_y) == (0, 0, 0, 0)
        assert (file.header.full_height, file.header.full_width) == data.shape
        assert file.header.get_float_attribute("PixelAspectRatio") == pixel_aspect
        proxy = file.fitted((4, 4)).default_with(lambda faults: pytest.fail(str(faults)))
        assert proxy.pixels.shape == (*fit_shape, 4)
        assert (proxy.header.height, proxy.header.width) == fit_shape
        assert file.fitted((file.header.width * 2, file.header.height * 2)).default_with(lambda faults: pytest.fail(str(faults))) is file
        assert framed.recovery.reverted(scene).is_ok()
        np.testing.assert_array_equal(corners(), original)


def test_camera_bindings_accumulate_native_absence_type_and_frame_gaps() -> None:
    """Absent demand leaves an uncovered frame; earliest markers bind earlier frames and retain every binding fault."""
    with ExitStack() as release:
        scene = bpy.data.scenes.new("Camera Refusals")
        camera_data = bpy.data.cameras.new("Valid Camera")
        camera = bpy.data.objects.new(camera_data.name, camera_data)
        mesh = bpy.data.meshes.new("Camera Mesh")
        wrong = bpy.data.objects.new(mesh.name, mesh)
        release.callback(bpy.data.batch_remove, ids=(scene, camera, camera_data, wrong, mesh))
        scene.collection.objects.link(camera)
        scene.collection.objects.link(wrong)
        removed = bpy.data.objects.new("Removed Camera", camera_data)
        removed_name = removed.name
        bpy.data.objects.remove(removed)
        frames = Frames(start=1, end=7, step=1)
        uncovered = cameras(scene, JobScope(scene=scene.name, camera=MarkedCamera()), frames)
        assert uncovered.map_error(lambda faults: tuple((fault.reason, fault.subject, fault.member) for fault in faults)) == Error(((Reason.ABSENT, repr(scene), "1"),))
        scope = JobScope(scene=scene.name, camera=MarkedCamera(markers=(Marker(frame=3, camera=wrong.name), Marker(frame=5, camera=removed_name))))
        refused = cameras(scene, scope, frames)
        assert refused.map_error(lambda faults: tuple((fault.reason, fault.subject, fault.member) for fault in faults)) == Error((
            (Reason.OUTSIDE, "marker at frame 3", bpy.types.Camera.__name__),
            (Reason.ABSENT, repr(bpy.data.objects), removed_name),
        ))
        assert refused.map_error(lambda faults: tuple(fault.cause if isinstance(fault.cause, type) else type(fault.cause) for fault in faults)) == Error((type(mesh), KeyError))
        assert cameras(scene, JobScope(scene=scene.name, camera=FixedCamera(camera=camera.name)), frames) == Ok(None)
