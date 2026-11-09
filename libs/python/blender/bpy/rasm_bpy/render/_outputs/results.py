"""Render products: completed frame files and their captured production facts, the compositor Viewer, intermediate renders, and finishing operations over written files."""

from collections import Counter
from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack, nullcontext
from copy import deepcopy
from dataclasses import dataclass, field
from functools import partial, reduce
from pathlib import Path
from shutil import copyfile
from tempfile import TemporaryDirectory
from threading import current_thread, main_thread
from types import MappingProxyType
from typing import Final, Literal, Self

import bpy
from expression import Option, Result, Some
from expression.collections import Map
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from rasm_bpy.data._extension.identity import Identity
from rasm_bpy.data._extension.registration import _DISPATCHERS, Acquisitions, Declaration, Handler, Resource
from rasm_bpy.data._extension.reports import Refusal
from rasm_bpy.data._ids.store import Handle
from rasm_bpy.data._rna.descriptors import _assign, Descriptor
from rasm_bpy.data._rna.snapshots import Snapshot
from rasm_bpy.render._color.spaces import _leaving, viewed
from rasm_bpy.render._outputs.formats import _depth, _FILE_FORMATS, EXR, OutputFormat
from rasm_bpy.render._outputs.framing import FrameSize
from rasm_bpy.render._outputs.paths import _scene_still_path
from rasm_bpy.scene.cameras import render_views
from rasm_imaging import captured_scene_source, Depth, DisplayView, ImageFile, Layer, Multilayer, Optics, read_codes
from rasm_records import accumulated, committed, Error, Fault, Nothing, Ok, Outcome, paired, Reason
from rasm_rendering import Output

# --- [TYPES] ----------------------------------------------------------------------------

type _ResultKind = Literal["RENDER_RESULT", "COMPOSITING"]
type Consume = Callable[[CompletedRender], Outcome[Option[str]]]
type ResultCapture = Callable[[bpy.types.Scene, PyOpenColorIO.Config], Outcome[Consume]]
type _Receipt = Callable[[bpy.types.Scene], Outcome[tuple[Written, ...]]]

# --- [CONSTANTS] ------------------------------------------------------------------------

COMBINED: Final = "Combined"
_COMPOSITE: Final = "Composite"
_SAVED: Final = OutputFormat(
    media_type="MULTI_LAYER_IMAGE", file_format="OPEN_EXR_MULTILAYER", color_depth="32", exr_codec="ZIP", use_exr_interleave=False, views_format="MULTIVIEW", color_management="FOLLOW_SCENE"
)

# --- [MODELS] ---------------------------------------------------------------------------


class RenderSource(msgspec.Struct, frozen=True, kw_only=True):
    """Captured raw encoding and native source selection, absent when a choice is unproven."""

    space: str
    layer: str | None = None
    view: DisplayView | None = None
    camera: tuple[float, float] | None = None

    @property
    def optics(self) -> Outcome[Option[Optics]]:
        """Physical optics from captured native lens millimeters and f-number."""
        if (camera := self.camera) is None:
            return Ok(Nothing)
        try:
            return Ok(Some(msgspec.convert({"focal_length": camera[0] / 1000.0, "f_number": camera[1]}, Optics)))
        except msgspec.ValidationError as error:
            return Error(Fault.of(Reason.OUTSIDE, f"{type(self).__name__}.camera", error, member=Optics.__name__))


class CompletedRender(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Owned multilayer file and captured production facts held through deferred consumption."""

    file: Multilayer
    config: PyOpenColorIO.Config
    number: int | None
    encoding: Outcome[Option[PyOpenColorIO.Processor]]
    source_encoding: Outcome[Option[PyOpenColorIO.Processor]]
    scene: Option[Handle[bpy.types.Scene]] = Nothing
    space: Option[str] = Nothing
    raw: Option[RenderSource] = Nothing
    view: Option[DisplayView] = Nothing
    codes: Outcome[Option[npt.NDArray[np.uint8 | np.uint16]]] = Ok(Nothing)

    @classmethod
    def stored(cls, file: Multilayer, config: PyOpenColorIO.Config, *, space: Option[str], raw: Option[RenderSource] = Nothing, number: int | None = None) -> Self:
        """Reads an explicitly selected ordinary file under current color definitions, retaining only supplied historical facts."""
        held = deepcopy(config)
        encodings = {name: captured_scene_source(held, name) for name in dict.fromkeys((*space.to_list(), *(source.space for source in raw.to_list())))}
        return cls(
            file=file,
            config=held,
            space=space,
            raw=raw,
            number=number,
            encoding=space.to_result_with(partial(Fault.of, Reason.ABSENT, str(file.path), member="encoding colorspace")).bind(lambda name: encodings[name]),
            source_encoding=raw.to_result_with(partial(Fault.of, Reason.ABSENT, str(file.path), member="raw render source")).bind(lambda source: encodings[source.space]),
        )

    def read(self, *, view: Option[str] = Nothing) -> Outcome[ImageFile]:
        """Reads final pixels in their captured encoding."""
        return self.space.to_result_with(partial(Fault.of, Reason.ABSENT, str(self.file.path), member="encoding colorspace")).bind(
            lambda space: _combined_part(self.file, self.config, partial(_output, preferred=self.raw.bind(lambda raw: Option.of_optional(raw.layer))), view).map(
                lambda file: msgspec.structs.replace(file, colorspace=space)
            )
        )

    def source(self, *, layer: Option[str] = Nothing, view: Option[str] = Nothing) -> Outcome[ImageFile]:
        """Reads pre-effects pixels using captured native source selection."""
        return self.raw.to_result_with(partial(Fault.of, Reason.ABSENT, str(self.file.path), member="raw render source")).bind(lambda raw: _source(self.file, self.config, raw, layer=layer, view=view))


class _Frame(msgspec.Struct, frozen=True, kw_only=True):
    """Production facts and prepared consumers of the last successful frame."""

    scene: Handle[bpy.types.Scene]
    config: PyOpenColorIO.Config
    output: Option[tuple[str, Option[DisplayView]]]
    source_space: str
    encodings: Mapping[str, Outcome[Option[PyOpenColorIO.Processor]]]
    compositor_layer: str
    requested_layer: Option[str]
    view: DisplayView
    camera: tuple[float, float] | None
    number: int
    consumers: tuple[Outcome[Consume], ...]
    coding: Outcome[Option[tuple[Snapshot, str]]] | None

    def completed(self, file: Multilayer, codes: Outcome[Option[npt.NDArray[np.uint8 | np.uint16]]]) -> CompletedRender:
        """Resolves actual final and raw source ownership from native saved parts."""
        owners = _owners(file)
        raw = tuple(name for name in owners if name and name != _COMPOSITE)
        layer = Some(self.compositor_layer) if _COMPOSITE in owners else self.requested_layer.or_else(Some(raw[0]) if len(raw) == 1 else Nothing)
        output = self.output if _COMPOSITE in owners else Some((self.source_space, Some(self.view)))
        space, formation = output.map(lambda held: held[0]), output.bind(lambda held: held[1])
        return CompletedRender(
            file=file,
            config=self.config,
            scene=Some(self.scene),
            space=space,
            raw=Some(RenderSource(space=self.source_space, layer=layer.to_optional(), view=self.view, camera=self.camera)) if raw else Nothing,
            view=formation,
            number=self.number,
            encoding=space.to_result_with(partial(Fault.of, Reason.ABSENT, str(file.path), member="encoding colorspace")).bind(lambda name: self.encodings[name]),
            source_encoding=self.encodings[self.source_space],
            codes=codes,
        )


class Written(msgspec.Struct, frozen=True, kw_only=True):
    """Written file with its stored format and depth, pass names, and display window."""

    path: str
    output: Output
    format: OutputFormat
    depth: Depth
    layer: Option[str]
    channels: Mapping[str, str] = MappingProxyType({})
    passes: Map[str, str] = Map.empty()
    display: Option[OpenImageIO.ROI] = Nothing
    view: Option[str] = Nothing

    @classmethod
    def of(
        cls, path: str, settings: bpy.types.ImageFormatSettings, output: Output, channels: Mapping[str, str], layer: Option[str], passes: Map[str, str], *, view: Option[str] = Nothing
    ) -> Option[Self]:
        """Returns written file when native depth maps to a storage depth, `channels` renaming one-channel source passes for a downstream reader."""
        return _depth(settings).map(lambda held: cls(path=path, output=output, format=OutputFormat.stored(settings), depth=held, layer=layer, channels=channels, passes=passes, view=view))

    @classmethod
    def scene(cls, scene: bpy.types.Scene, paths: tuple[tuple[Path, Option[str]], ...]) -> tuple[Self, ...]:
        """Records caller-owned scene output paths with the native format and camera-view ownership."""
        return tuple(file for path, view in paths for file in cls.of(str(path), scene.render.image_settings, Output.PREVIEW, MappingProxyType({}), Nothing, Map.empty(), view=view).to_list())


class _Job(msgspec.Struct, frozen=True, kw_only=True):
    """Render image and target slot held until the native job completes."""

    target: Option[tuple[bpy.types.Image, bpy.types.RenderSlot]]
    files: Option[_Receipt]
    layer: Option[str]
    animation: bool
    frame: Outcome[Option[_Frame]] = Ok(Nothing)
    written: Option[tuple[Outcome[tuple[Written, ...]], Outcome[None]]] = Nothing

    def exported(self, scene: bpy.types.Scene, path: Path) -> Outcome[Path]:
        """Writes the borrowed native target under current settings, restoring the displayed selection."""

        def selected(target: tuple[bpy.types.Image, bpy.types.RenderSlot]) -> Outcome[Path]:
            image, slot = target
            prior = image.render_slots.active
            with ExitStack() as selection:
                selection.callback(setattr, image.render_slots, "active", prior)
                return _assign(image.render_slots, "active", slot).map_error(partial(Fault.of, Refusal.REFUSED, repr(image.render_slots))).bind(lambda _: _exported(image, scene, path))

        return self.target.to_result_with(partial(Fault.of, Refusal.REQUIRED, repr(scene), member="pre-operator render target slot")).bind(selected)

    def saved(self, scene: bpy.types.Scene, path: Path) -> Outcome[Path]:
        """Retains a borrowed native slot or one authoritative multilayer scene receipt."""

        def copied(files: tuple[Written, ...]) -> Outcome[Path]:
            match tuple(file for file in files if file.output is Output.PREVIEW and file.format.file_format == "OPEN_EXR_MULTILAYER"):
                case (file,):
                    return catch(copyfile, exception=OSError)(file.path, path).map(lambda _: path).map_error(partial(Fault.of, Reason.FAILED, str(path)))
                case found:
                    return Error(Fault.of(Refusal.REQUIRED, repr(scene), tuple(file.path for file in found), member="one native multilayer render receipt"))

        if self.target.is_none():
            return self.written.to_result_with(partial(Fault.of, Refusal.REQUIRED, repr(scene), member="pre-operator target or native written receipt")).bind(lambda produced: produced[0].bind(copied))
        with _SAVED.held(scene.render.image_settings) as formatted:
            return formatted.bind(lambda _: self.exported(scene, path))

    def codes(self, scene: bpy.types.Scene, frame: _Frame, folder: Path, identity: Identity) -> Outcome[Option[npt.NDArray[np.uint8 | np.uint16]]]:
        """Reads finished native file codes, exporting a no-write preview under captured native formation."""
        if (coding := frame.coding) is None:
            return Ok(Nothing)

        def written(files: tuple[Written, ...]) -> Outcome[Option[npt.NDArray[np.uint8 | np.uint16]]]:
            match tuple(file for file in files if file.output is Output.PREVIEW):
                case ():
                    return Ok(Nothing)
                case (file,):
                    return read_codes(Path(file.path))
                case found:
                    return Error(Fault.of(Refusal.REQUIRED, repr(scene), tuple(file.view for file in found), member="one native camera-view code receipt"))

        def preview(capture: tuple[Snapshot, str]) -> Outcome[Option[npt.NDArray[np.uint8 | np.uint16]]]:
            settings, suffix = capture
            path = folder / f"preview{suffix}"

            def exported(before: Snapshot) -> Outcome[Option[npt.NDArray[np.uint8 | np.uint16]]]:
                try:
                    answer = (
                        settings
                        .apply(scene)
                        .outcome.bind(lambda _: self.exported(scene, path))
                        .bind(lambda _: _RENDER_ORIGINS[identity].finished(scene, Written.scene(scene, ((path, Nothing),)), identity))
                        .bind(lambda _: read_codes(path))
                    )
                finally:
                    restored = before.apply(scene).outcome
                return paired(answer, restored).map(lambda held: held[0])

            return Snapshot.capture(scene, Some(settings.selection(scene)), inactive=True).bind(exported)

        return self.written.map(lambda produced: produced[1].bind(lambda _: produced[0].bind(written))).default_with(
            lambda: coding.bind(lambda capture: capture.map(preview).default_value(Ok(Nothing)))
        )


class ResultRead(msgspec.Struct, frozen=True, kw_only=True):
    """Completed-frame consumer delivered on the main thread with requested native output codes."""

    capture: ResultCapture
    codes: bool = False

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Consumer list holding the one render completion handler."""
        return (_RENDER_ORIGINS,)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Acquisitions]:
        """Holds the consumer on the extension's consumer list until scope exit."""
        consumers = _RENDER_ORIGINS[identity].consumers
        with ExitStack() as stack:
            consumers.append(self)
            stack.callback(consumers.remove, self)
            yield ()


class Finish(msgspec.Struct, frozen=True, kw_only=True):
    """Finishing operation and written storage classes it consumes."""

    run: Callable[[bpy.types.Scene, Written, Identity], Outcome[None]]
    outputs: frozenset[Output] = frozenset(Output)
    formats: frozenset[str] = _FILE_FORMATS
    depths: frozenset[Depth] = frozenset(Depth)

    def accepts(self, file: Written) -> bool:
        """Returns whether operation consumes file."""
        return file.output in self.outputs and file.format.file_format in self.formats and file.depth in self.depths


class Finishing(msgspec.Struct, frozen=True, kw_only=True):
    """Finishing operations with handlers spanning one render job, `files` reading the files marked compositor outputs wrote for a scene frame."""

    rows: tuple[Finish, ...]
    files: Callable[[bpy.types.Scene, int, Identity, Option[bool]], Outcome[tuple[Written, ...]]]

    def finished(self, scene: bpy.types.Scene, files: tuple[Written, ...], identity: Identity) -> Outcome[None]:
        """Chains operations per file, faults accumulating across files."""
        return accumulated(reduce(lambda done, row: done.bind(lambda _: row.run(scene, file, identity)), (row for row in self.rows if row.accepts(file)), Ok(None)) for file in files).map(
            lambda _: None
        )

    def windowed(self, scene: bpy.types.Scene, files: tuple[Written, ...], identity: Identity) -> Outcome[None]:
        """Finishes files under the original camera display window."""
        return (
            FrameSize()
            .bound(identity)
            .bind(lambda framed: framed.base(scene))
            .map(lambda held: held.map(lambda base: base.bounds[1]))
            .bind(lambda display: self.finished(scene, tuple(msgspec.structs.replace(file, display=display) for file in files), identity))
        )

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Native production and original camera framing used by finishing."""
        return (FrameSize.attachment, _INTERMEDIATES, _RENDER_ORIGINS)

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Acquisitions]:
        """Registers finishing for native scene writes, owned preview exports, and routed File Output files."""
        service, intermediates = _RENDER_ORIGINS[identity], _INTERMEDIATES[identity]
        with ExitStack() as scope:
            service.finishers.append(self)
            scope.callback(service.finishers.remove, self)
            yield (
                Handler(
                    handlers=bpy.app.handlers.render_post,
                    body=lambda scene: (
                        self.files(scene, scene.frame_current, identity, Some(job.animation)).bind(lambda files: self.windowed(scene, files, identity))
                        if scene.session_uid not in intermediates and (job := service.pending.get(scene.session_uid)) is not None
                        else Ok(None)
                    ),
                ),
            )


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(slots=True, kw_only=True, eq=False)
class _RenderOrigins:
    """Explicit render requests, borrowed native targets, and prepared consumers."""

    requests: dict[int, _Job] = field(default_factory=dict)
    pending: dict[int, _Job] = field(default_factory=dict)
    consumers: list[ResultRead] = field(default_factory=list)
    finishers: list[Finishing] = field(default_factory=list)
    files: set[ExitStack] = field(default_factory=set)

    def renewed(self) -> Self:
        """Releases the loaded file's native facts and saved frames, retaining registered consumers."""
        self.requests.clear()
        self.pending.clear()
        for scope in tuple(self.files):
            scope.close()
        return self

    @contextmanager
    def requested(self, scene: bpy.types.Scene, layer: Option[str], files: Option[_Receipt], *, animation: bool) -> Generator[None]:
        """Borrows the pre-operator image and slot, which the caller keeps alive until completion."""
        key, prior = scene.session_uid, self.requests.get(scene.session_uid)
        target = _built("RENDER_RESULT").to_option().bind(lambda image: Option.of_optional(image.render_slots.active).map(lambda slot: (image, slot)))
        self.requests[key] = _Job(target=target, layer=layer, files=files, animation=animation)
        try:
            yield
        finally:
            if prior is None:
                del self.requests[key]
            else:
                self.requests[key] = prior

    def finished(self, scene: bpy.types.Scene, files: tuple[Written, ...], identity: Identity) -> Outcome[None]:
        """Runs registered finishing pipelines in their declaration order."""
        return reduce(lambda done, row: done.bind(lambda _: row.windowed(scene, files, identity)), self.finishers, Ok(None))


_INTERMEDIATES: Final = Resource[set[int]](made=lambda _identity: nullcontext(set()))

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RESULTS]


@contextmanager
def _origins(identity: Identity) -> Generator[_RenderOrigins]:
    """Captures requested frames before native slots can be reused, then queues their prepared consumers."""
    service, intermediates = _RenderOrigins(), _INTERMEDIATES[identity]

    def supported() -> bool:
        manager: bpy.types.WindowManager = bpy.context.window_manager  # ty: ignore[invalid-assignment]
        return current_thread() is main_thread() or manager.is_interface_locked

    def opened(scene: bpy.types.Scene) -> Outcome[None]:
        if scene.session_uid not in intermediates and (job := service.requests.get(scene.session_uid)) is not None:
            service.pending[scene.session_uid] = job
        return Ok(None)

    def captured(scene: bpy.types.Scene) -> Outcome[None]:
        if scene.session_uid in intermediates or (job := service.pending.get(scene.session_uid)) is None or not (captures := tuple(service.consumers)):
            return Ok(None)
        if not supported():
            service.pending[scene.session_uid] = msgspec.structs.replace(job, frame=Error(Fault.of(Refusal.REQUIRED, repr(scene), member="native interface lock for automatic render capture")))
            return Ok(None)
        config = deepcopy(PyOpenColorIO.GetCurrentConfig())
        linear = config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR).getName()
        enabled = (entry for entry in scene.compositor_effects if entry.enable_for_render) if scene.render.use_compositing else ()
        view = viewed(scene)

        def resolved(origin: str | DisplayView) -> Option[tuple[str, Option[DisplayView]]]:
            match origin:
                case DisplayView():
                    return origin.colorspace(config).map(lambda color: (color.getName(), Nothing))
                case space:
                    color = config.getColorSpace(space)
                    return Some((space, Some(view) if not color.isData() and color.getReferenceSpaceType() == PyOpenColorIO.REFERENCE_SPACE_SCENE else Nothing))

        output = _leaving(config, enabled, Some(linear)).bind(resolved)
        render = scene.render
        settings = render.image_settings
        match scene.camera:
            case bpy.types.Object(data=bpy.types.Camera(type="PERSP") as camera):
                dof: bpy.types.CameraDOFSettings = camera.dof  # ty: ignore[invalid-assignment]
                optics: tuple[float, float] | None = camera.lens, dof.aperture_fstop
            case _:
                optics = None
        prepared = tuple((row, row.capture(scene, config)) for row in captures)
        frame = _Frame(
            scene=Handle.of(scene),
            config=config,
            output=output,
            source_space=linear,
            encodings=MappingProxyType({name: captured_scene_source(config, name) for name in dict.fromkeys((linear, *(held[0] for held in output.to_list())))}),
            compositor_layer=scene.view_layers[0].name,
            requested_layer=job.layer,
            view=view,
            camera=optics,
            number=scene.frame_current,
            consumers=tuple(answer for _, answer in prepared),
            coding=(
                _depth(settings)
                .filter(lambda held: held in {Depth.UINT8, Depth.UINT16})
                .map(
                    lambda _: (
                        Error(Fault.of(Refusal.REQUIRED, repr(scene), member="explicit individual camera-view preview receipt"))
                        if render.use_multiview and settings.views_format == "INDIVIDUAL"
                        else Snapshot.capture(
                            scene,
                            Some(tuple(Descriptor(root=bpy.types.Scene, prop=bpy.types.Scene.bl_rna.properties[member]) for member in ("display_settings", "view_settings", "render"))),
                            inactive=True,
                        ).map(lambda held: Some((held, render.file_extension)))
                    )
                )
                .default_value(Ok(Nothing))
                if any(row.codes and answer.is_ok() for row, answer in prepared)
                else None
            ),
        )
        service.pending[scene.session_uid] = msgspec.structs.replace(job, frame=Ok(Some(frame)), written=Nothing)
        return Ok(None)

    def written(scene: bpy.types.Scene) -> Outcome[None]:
        if scene.session_uid in intermediates or (job := service.pending.get(scene.session_uid)) is None or not (service.consumers or service.finishers):
            return Ok(None)
        files = job.files.to_result_with(lambda: Fault.of(Refusal.REQUIRED, f"owned native write receipt for {scene!r}")).bind(lambda provider: provider(scene))
        finished = files.bind(lambda receipts: service.finished(scene, receipts, identity))
        service.pending[scene.session_uid] = msgspec.structs.replace(job, written=Some((files, finished)))
        return finished

    def completed(scene: bpy.types.Scene) -> Outcome[None]:
        if scene.session_uid in intermediates or (job := service.pending.pop(scene.session_uid, None)) is None:
            return Ok(None)

        def retained(frame: _Frame) -> Outcome[None]:
            scope = ExitStack()
            files = service.files
            files.add(scope)
            scope.callback(files.remove, scope)

            def saved(folder: TemporaryDirectory[str]) -> Outcome[CompletedRender]:
                root = Path(scope.enter_context(folder))
                return job.saved(scene, root / "render.exr").bind(Multilayer.read).map(lambda file: frame.completed(file, job.codes(scene, frame, root, identity)))

            saved_frame = catch(TemporaryDirectory[str], exception=OSError)(dir=bpy.app.tempdir).map_error(partial(Fault.of, Reason.UNWRITABLE, bpy.app.tempdir)).bind(saved)
            match saved_frame:
                case Result(tag="ok", ok=held):
                    deferred(held, frame.consumers, scope)
                case _:
                    scope.close()
                    return accumulated((*[held.map(lambda _: None) for held in frame.consumers], saved_frame.map(lambda _: None))).map(lambda _: None)
            return Ok(None)

        return job.frame.bind(lambda frames: accumulated(map(retained, frames.to_list())).map(lambda _: None))

    def cancelled(scene: bpy.types.Scene) -> Outcome[None]:
        if scene.session_uid not in intermediates:
            service.pending.pop(scene.session_uid, None)
        return Ok(None)

    def consumed(completed: CompletedRender, prepared: tuple[Outcome[Consume], ...], scope: ExitStack) -> Outcome[Option[str]]:
        with scope:
            return accumulated(held.bind(lambda consume: consume(completed)) for held in prepared).map(lambda steps: Some(", ".join(step for named in steps for step in named.to_list())).filter(bool))

    with ExitStack() as lifetime:
        lifetime.callback(service.renewed)
        deferred = lifetime.enter_context(identity.reports.deferred(consumed))
        for handlers, body in (
            (bpy.app.handlers.render_init, opened),
            (bpy.app.handlers.render_post, captured),
            (bpy.app.handlers.render_write, written),
            (bpy.app.handlers.render_complete, completed),
            (bpy.app.handlers.render_cancel, cancelled),
        ):
            _ = lifetime.enter_context(Handler(handlers=handlers, body=body).held(identity))
        yield service


@contextmanager
def _intermediate(identity: Identity, scene: bpy.types.Scene) -> Generator[None]:
    """Hides scene renders from the extension's completed-image consumers during scope."""
    held = _INTERMEDIATES[identity]
    with ExitStack() as stack:
        if scene.session_uid not in held:
            held.add(scene.session_uid)
            stack.callback(held.remove, scene.session_uid)
        yield


def _built(kind: _ResultKind) -> Outcome[bpy.types.Image]:
    """Returns Render Result or shared compositor Viewer image, or absent fault before Blender creates it."""
    return next((Some(image) for image in bpy.data.images if image.type == kind), Nothing).to_result_with(partial(Fault.of, Reason.ABSENT, type(bpy.data.images).__name__, member=kind))


def _saved(scene: bpy.types.Scene) -> Outcome[str]:
    """Exports the displayed render result to a temporary EXR, multilayer over view layers and one unnamed RGBA part for a sequencer frame, the scene's format restored after on every path."""
    path = Path(bpy.app.tempdir, f"{scene.session_uid}.exr")

    def exported(image: bpy.types.Image) -> Outcome[Path]:
        with _SAVED.held(scene.render.image_settings) as formatted:
            return formatted.bind(lambda _: _exported(image, scene, path))

    return _built("RENDER_RESULT").bind(exported).map(str)


def _exported(image: bpy.types.Image, scene: bpy.types.Scene, path: Path) -> Outcome[Path]:
    """Writes a native image through its producing scene under the current native output settings."""
    return committed(path, lambda stage: catch(image.save_render, exception=RuntimeError)(str(stage), scene=scene).map_error(partial(Fault.of, Refusal.REFUSED, repr(image)))).map(lambda _: path)


def _animation_files(scene: bpy.types.Scene) -> Outcome[tuple[Written, ...]]:
    """Reads native animation paths, preserving each configured individual camera view."""
    if (render := scene.render).is_movie_format:
        return Ok(())
    views = tuple(Some(view.name) for view in render_views(render)) if render.use_multiview and render.image_settings.views_format == "INDIVIDUAL" else (Nothing,)
    return accumulated(
        catch(render.frame_path, exception=RuntimeError)(frame=scene.frame_current, view=view.default_value(""))
        .map(lambda path: (Path(path), view))
        .map_error(partial(Fault.of, Refusal.REFUSED, repr(render)))
        for view in views
    ).map(lambda paths: Written.scene(scene, paths))


def _still_files(scene: bpy.types.Scene) -> Outcome[tuple[Written, ...]]:
    """Reads native still-write paths under the actual format and configured camera views."""
    render = scene.render
    views = tuple(map(Some, render_views(render))) if render.use_multiview and render.image_settings.views_format == "INDIVIDUAL" else (Nothing,)
    return accumulated(_scene_still_path(scene, view=view).map(lambda path: (path, view.map(lambda held: held.name))) for view in views).map(lambda paths: Written.scene(scene, paths))


def _combined(path: str, config: PyOpenColorIO.Config, layer: Option[str] = Nothing, *, view: Option[str] = Nothing) -> Outcome[ImageFile]:
    """Reads a saved render's Combined RGBA.

    Args:
        path: Saved multilayer render.
        config: Configuration resolving native file colorspace tags.
        layer: View layer to read, else the compositor's output, then the unnamed sequencer layer, then the first view layer.
        view: Camera view from native part metadata, required when a layer holds more than one view.

    Returns:
        Associated RGBA image with native source colorspace, absent fault without a matching layer or view, required fault for an ambiguous camera view.
    """
    return Multilayer.read(Path(path)).bind(lambda file: _combined_part(file, config, lambda owners: layer.or_else_with(lambda: _output(owners)), view))


def _source(file: Multilayer, config: PyOpenColorIO.Config, raw: RenderSource, *, layer: Option[str] = Nothing, view: Option[str] = Nothing) -> Outcome[ImageFile]:
    """Reads a captured raw encoding and selected native view layer, requiring a choice when no default is proven."""
    return (
        layer
        .or_else_with(lambda: Option.of_optional(raw.layer))
        .to_result_with(partial(Fault.of, Refusal.REQUIRED, str(file.path), member="raw render view layer"))
        .bind(lambda name: _combined_part(file, config, lambda _: Some(name), view))
        .map(lambda image: msgspec.structs.replace(image, colorspace=raw.space))
    )


def _owners(file: Multilayer) -> dict[str, list[Layer]]:
    """Indexes every native Combined layer under its output owner."""
    owners: dict[str, list[Layer]] = {}
    for found in file.layers:
        owner, _, kind = found.name.rpartition(".")
        if kind in {COMBINED, ""}:
            owners.setdefault(owner, []).append(found)
    return owners


def _output(owners: Mapping[str, list[Layer]], *, preferred: Option[str] = Nothing) -> Option[str]:
    """Selects Composite, unnamed output, captured raw selection, then first raw output."""
    return next((Some(name) for name in (_COMPOSITE, "") if name in owners), preferred).or_else_with(lambda: next((Some(name) for name in owners), Nothing))


def _combined_part(file: Multilayer, config: PyOpenColorIO.Config, selection: Callable[[Mapping[str, list[Layer]]], Option[str]], view: Option[str]) -> Outcome[ImageFile]:
    """Selects the Combined owner before selecting its native camera-view part."""
    owners = _owners(file)
    views = {index: Some(spec.get_string_attribute("view")).filter(bool) for index, spec in file.parts.items()}

    def selected(owner: str) -> Outcome[ImageFile]:
        match tuple(found for found in owners.get(owner, ()) if view.is_none() or views[found.part] == view):
            case (found,):
                return file.image(found, ("R", "G", "B", "A"), config)
            case (first, *rest):
                return Error(Fault.of(Refusal.REQUIRED, f"camera view for {owner}.{COMBINED} in {file.path}", tuple(views[found.part] for found in (first, *rest))))
            case _:
                return Error(Fault.of(Reason.ABSENT, str(file.path), member=f"{owner}.{COMBINED}" + view.map(lambda name: f" ({name})").default_value("")))

    return selection(owners).to_result_with(partial(Fault.of, Reason.ABSENT, str(file.path), member=COMBINED)).bind(selected)


# --- [FINISHING]


def _rewritten(_scene: bpy.types.Scene, file: Written, _identity: Identity) -> Outcome[None]:
    """Rewrites file display window and the one-channel pass names its channel renames hold."""

    def channels(names: tuple[str, ...]) -> tuple[str, ...]:
        counts = Counter(name.rpartition(".")[0] for name in names)
        return tuple(
            file.passes
            .try_find(layer)
            .bind(lambda source: Option.of_optional(file.channels.get(source)))
            .filter(lambda _: counts[layer] == 1)
            .map(lambda channel: f"{layer}.{channel}" if layer else channel)
            .default_value(name)
            for name in names
            for layer in (name.rpartition(".")[0],)
        )

    return Multilayer.rewritten(Path(file.path), file.display, channels, file.format.codec).map(lambda _: None)


# --- [COMPOSITION] ----------------------------------------------------------------------

_RENDER_ORIGINS: Final = Resource[_RenderOrigins](made=_origins, renewed=Some(_RenderOrigins.renewed), prerequisites=(_DISPATCHERS, _INTERMEDIATES))
rewrite: Final = Finish(run=_rewritten, formats=EXR, depths=frozenset({Depth.HALF, Depth.FLOAT}))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COMBINED", "CompletedRender", "Consume", "Finish", "Finishing", "RenderSource", "ResultCapture", "ResultRead", "Written", "rewrite"]
